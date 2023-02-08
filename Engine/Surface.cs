/*
 * Copyright 2010-2016 OpenXcom Developers.
 *
 * This file is part of OpenXcom.
 *
 * OpenXcom is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * OpenXcom is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with OpenXcom.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace SharpXcom.Engine;

/**
 * help class used for Surface::blitNShade
 */
struct ColorReplace : IColorFunc<byte, byte, int, int, int>
{
    /**
	* Function used by ShaderDraw in Surface::blitNShade
	* set shade and replace color in that surface
	* @param dest destination pixel
	* @param src source pixel
	* @param shade value of shade of this surface
	* @param newColor new color to set (it should be offseted by 4)
	*/
    public void func(ref byte dest, byte src, int shade, int newColor, int _)
	{
		if (src != default)
		{
			int newShade = (src & 15) + shade;
            if (newShade > 15)
                // so dark it would flip over to another color - make it black instead
                dest = 15;
            else
                dest = (byte)(newColor | newShade);
		}
	}
};

/**
 * help class used for Surface::blitNShade
 */
struct StandardShade : IColorFunc<byte, byte, int, int, int>
{
    /**
	* Function used by ShaderDraw in Surface::blitNShade
	* set shade
	* @param dest destination pixel
	* @param src source pixel
	* @param shade value of shade of this surface
	* @param notused
	* @param notused
	*/
    public void func(ref byte dest, byte src, int shade, int _, int __)
	{
		if (src != default)
		{
			int newShade = (src & 15) + shade;
			if (newShade > 15)
				// so dark it would flip over to another color - make it black instead
				dest = 15;
			else
				dest = (byte)((src & (15 << 4)) | newShade);
		}
	}
};

/**
 * Element that is blit (rendered) onto the screen.
 * Mainly an encapsulation for SDL's SDL_Surface struct, so it
 * borrows a lot of its terminology. Takes care of all the common
 * rendering tasks and color effects, while serving as the base
 * class for more specialized screen elements.
 */
internal class Surface
{
    protected SDL_Surface _surface;
    protected int _x, _y;
    protected SDL_Rect _crop, _clear;
    protected bool _visible, _hidden, _redraw, _tftdMode;
    protected nint /* void* */ _alignedBuffer;

    /**
     * Sets up a blank 8bpp surface with the specified size and position,
     * with pure black as the transparent color.
     * @note Surfaces don't have to fill the whole size since their
     * background is transparent, specially subclasses with their own
     * drawing logic, so it just covers the maximum drawing area.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     * @param bpp Bits-per-pixel depth.
     */
    internal Surface(int width, int height, int x = 0, int y = 0, int bpp = 8)
    {
        _x = x;
        _y = y;
        _visible = true;
        _hidden = false;
        _redraw = false;
        _tftdMode = false;

        _alignedBuffer = NewAligned(bpp, width, height);

        var surfacePtr = SDL_CreateRGBSurfaceFrom(_alignedBuffer, width, height, bpp, GetPitch(bpp, width), 0, 0, 0, 0);
        if (surfacePtr == nint.Zero)
        {
            throw new Exception(SDL_GetError());
        }
        _surface = Marshal.PtrToStructure<SDL_Surface>(surfacePtr);

        SDL_SetColorKey(_surface.pixels, (int)SDL_bool.SDL_TRUE /* SDL_SRCCOLORKEY */, 0);

        _crop.w = 0;
        _crop.h = 0;
        _crop.x = 0;
        _crop.y = 0;
        _clear.x = 0;
        _clear.y = 0;
        _clear.w = getWidth();
        _clear.h = getHeight();
    }

    /**
     * Performs a deep copy of an existing surface.
     * @param other Surface to copy from.
     */
    unsafe internal Surface(Surface other)
    {
        nint surfacePtr;
        //if is native OpenXcom aligned surface
	    if (other._alignedBuffer != nint.Zero)
	    {
            //TODO
            var format = SDL_GetWindowPixelFormat(other._surface.pixels);
            byte bpp = SDL_BITSPERPIXEL(format); //byte bpp = other._surface.format.BitsPerPixel;
		    int width = other.getWidth();
		    int height = other.getHeight();
		    int pitch = GetPitch(bpp, width);
            _alignedBuffer = NewAligned(bpp, width, height);
            surfacePtr = SDL_CreateRGBSurfaceFrom(_alignedBuffer, width, height, bpp, pitch, 0, 0, 0, 0);
            _surface = Marshal.PtrToStructure<SDL_Surface>(surfacePtr);
            SDL_SetColorKey(_surface.pixels, (int)SDL_bool.SDL_TRUE /* SDL_SRCCOLORKEY */, 0);
            //cant call `setPalette` because its virtual function and it dont work correctly in constructor
            SDL_SetPaletteColors(_surface.pixels, other.getPaletteColors(), 0, 255);
            new Span<byte>((byte*)other._alignedBuffer, height * pitch).CopyTo(new Span<byte>((byte*)_alignedBuffer, height * pitch)); //memcpy(_alignedBuffer, other._alignedBuffer, height * pitch);
        }
        else
	    {
		    surfacePtr = SDL_ConvertSurface(other._surface.pixels, other._surface.format, other._surface.flags);
            _surface = Marshal.PtrToStructure<SDL_Surface>(surfacePtr);
            _alignedBuffer = 0;
	    }

        if (surfacePtr == nint.Zero)
	    {
            throw new Exception(SDL_GetError());
	    }
	    _x = other._x;
	    _y = other._y;
	    _crop.w = other._crop.w;
	    _crop.h = other._crop.h;
	    _crop.x = other._crop.x;
	    _crop.y = other._crop.y;
	    _clear.w = other._clear.w;
	    _clear.h = other._clear.h;
	    _clear.x = other._clear.x;
	    _clear.y = other._clear.y;
	    _visible = other._visible;
	    _hidden = other._hidden;
	    _redraw = other._redraw;
	    _tftdMode = other._tftdMode;
    }

    /**
     * Deletes the surface from memory.
     */
    ~Surface()
    {
        DeleteAligned(_alignedBuffer);
        SDL_FreeSurface(_surface.pixels);
    }

    /**
	 * Returns the width of the surface.
	 * @return Width in pixels.
	 */
    internal int getWidth() =>
		_surface.w;

    /**
	 * Returns the height of the surface.
	 * @return Height in pixels
	 */
    internal int getHeight() =>
		_surface.h;

    /**
     * Helper function counting pitch in bytes with 16byte padding
     * @param bpp bits per pixel
     * @param width number of pixel in row
     * @return pitch in bytes
     */
    int GetPitch(int bpp, int width) =>
        ((bpp / 8) * width + 15) & ~0xF;

    /**
     * Helper function creating aligned buffer
     * @param bpp bits per pixel
     * @param width number of pixel in row
     * @param height number of rows
     * @return pointer to memory
     */
    unsafe nint NewAligned(int bpp, int width, int height)
    {
        int pitch = GetPitch(bpp, width);
        int total = pitch * height;
        nint buffer = Marshal.AllocHGlobal(total);
        if (buffer == nint.Zero)
        {
            throw new Exception("Failed to allocate surface");
        }
        var span = new Span<byte>((byte*)buffer, total);
        span.Fill(0);
        return buffer;
    }

    /**
     * Helper function release aligned memory
     * @param buffer buffer to delete
     */
    void DeleteAligned(nint buffer)
    {
        if (buffer != nint.Zero)
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /**
	 * Returns the surface's 8bpp palette.
	 * @return Pointer to the palette's colors.
	 */
    internal SDL_Color[] getPaletteColors()
    {
        var format = Marshal.PtrToStructure<SDL_PixelFormat>(_surface.format);
        var palette = Marshal.PtrToStructure<SDL_Palette>(format.palette);
        var colors = Marshal.PtrToStructure<SDL_Color[]>(palette.colors);
        return colors;
    }

    internal static SDL_Palette getPalette(SDL_Surface surface)
    {
        var format = Marshal.PtrToStructure<SDL_PixelFormat>(surface.format);
        var palette = Marshal.PtrToStructure<SDL_Palette>(format.palette);
        return palette;
    }

    internal static SDL_PixelFormat getFormat(SDL_Surface surface) =>
        Marshal.PtrToStructure<SDL_PixelFormat>(surface.format);

    /**
	 * Returns the pointer to a specified pixel in the surface.
	 * @param x X position of the pixel.
	 * @param y Y position of the pixel.
	 * @return Pointer to the pixel.
	 */
    nint getRaw(int x, int y)
	{
        var bpp = getFormat(_surface).BytesPerPixel;
        return nint.Add(_surface.pixels, y * _surface.pitch + x * bpp);
	}

    /**
	 * Returns the color of a specified pixel in the surface.
	 * @param x X position of the pixel.
	 * @param y Y position of the pixel.
	 * @return Color of the pixel, zero if the position is invalid.
	 */
    internal byte getPixel(int x, int y)
	{
		if (x < 0 || x >= getWidth() || y < 0 || y >= getHeight())
		{
			return 0;
		}
		return Marshal.ReadByte(getRaw(x, y));
	}

    /**
	 * Changes the color of a pixel in the surface, relative to
	 * the top-left corner of the surface. Invalid positions are ignored.
	 * @param x X position of the pixel.
	 * @param y Y position of the pixel.
	 * @param pixel New color for the pixel.
	 */
    internal void setPixel(int x, int y, byte pixel)
    {
        if (x < 0 || x >= getWidth() || y < 0 || y >= getHeight())
        {
            return;
        }
        Marshal.WriteByte(getRaw(x, y), pixel);
    }

    /**
     * Locks the surface from outside access
     * for pixel-level access. Must be unlocked
     * afterwards.
     * @sa unlock()
     */
    internal void @lock() =>
        SDL_LockSurface(_surface.pixels);

    /**
     * Unlocks the surface after it's been locked
     * to resume blitting operations.
     * @sa lock()
     */
    internal void unlock() =>
        SDL_UnlockSurface(_surface.pixels);

    /**
     * Specific blit function to blit battlescape terrain data in different shades in a fast way.
     * Notice there is no surface locking here - you have to make sure you lock the surface yourself
     * at the start of blitting and unlock it when done.
     * @param surface to blit to
     * @param x
     * @param y
     * @param off
     * @param half some tiles are blitted only the right half
     * @param newBaseColor Attention: the actual color + 1, because 0 is no new base color.
     */
    internal void blitNShade(Surface surface, int x, int y, int off, bool half = false, int newBaseColor = 0)
    {
        var src = new ShaderMove<byte>(this, x, y);
        if (half)
        {
            var g = src.getDomain();
            g.beg_x = g.end_x / 2;
            src.setDomain(g);
        }
        if (newBaseColor != 0)
        {
            --newBaseColor;
            newBaseColor <<= 4;
            ShaderDraw(new ColorReplace(), ShaderSurface(surface), src, ShaderScalar(off), ShaderScalar(newBaseColor));
        }
        else
        {
            ShaderDraw(new StandardShade(), ShaderSurface(surface), src, ShaderScalar(off));
        }
    }

    /**
     * Changes the visibility of the surface. A hidden surface
     * isn't blitted nor receives events.
     * @param visible New visibility.
     */
    internal virtual void setVisible(bool visible) =>
        _visible = visible;

    /**
     * Replaces a certain amount of colors in the surface's palette.
     * @param colors Pointer to the set of colors.
     * @param firstcolor Offset of the first color to replace.
     * @param ncolors Amount of colors to replace.
     */
    internal virtual void setPalette(SDL_Color[] colors, int firstcolor = 0, int ncolors = 256)
    {
        //TODO
        var format = SDL_GetWindowPixelFormat(_surface.pixels);
        if (SDL_BITSPERPIXEL(format) == 8) //if (_surface.format.BitsPerPixel == 8)
            SDL_SetPaletteColors(_surface.pixels, colors, firstcolor, ncolors);
    }

    /// Initializes the surface's various text resources.
    internal virtual void initText(Font font1, Font font2, Language language) { }

	/**
	 * Returns the position of the surface in the X axis.
	 * @return X position in pixels.
	 */
    internal int getX() =>
        _x;

	/**
	 * Returns the position of the surface in the Y axis.
	 * @return Y position in pixels.
	 */
	internal int getY() =>
		_y;

    /**
     * Changes the position of the surface in the X axis.
     * @param x X position in pixels.
     */
    internal void setX(int x) =>
        _x = x;

    /**
     * Changes the position of the surface in the Y axis.
     * @param y Y position in pixels.
     */
    internal void setY(int y) =>
        _y = y;

    /**
	 * Returns the internal SDL_Surface for SDL calls.
	 * @return Pointer to the surface.
	 */
    internal SDL_Surface getSurface() =>
		_surface;

    /**
     * Draws the graphic that the surface contains before it
     * gets blitted onto other surfaces. The surface is only
     * redrawn if the flag is set by a property change, to
     * avoid unnecessary drawing.
     */
    internal virtual void draw()
    {
        _redraw = false;
        clear();
    }

    /**
     * Clears the entire contents of the surface, resulting
     * in a blank image of the specified color. (0 for transparent)
     * @param color the colour for the background of the surface.
     */
    unsafe internal void clear(uint color = 0)
    {
        if ((_surface.flags & SDL_SWSURFACE) != 0)
        {
            var span = new Span<uint>((uint*)_surface.pixels, _surface.h * _surface.pitch);
            span.Fill(color); //memset(_surface->pixels, color, _surface->h * _surface->pitch);
        }
        else
        {
            SDL_FillRect(_surface.pixels, ref _clear, color);
        }
    }

    /**
     * Draws a line on the surface.
     * @param x1 Start x coordinate in pixels.
     * @param y1 Start y coordinate in pixels.
     * @param x2 End x coordinate in pixels.
     * @param y2 End y coordinate in pixels.
     * @param color Color of the line.
     */
    internal void drawLine(short x1, short y1, short x2, short y2, byte color) =>
        lineColor(_surface.pixels, x1, y1, x2, y2, Palette.getRGBA(getPaletteColors(), color));

    /**
     * This is a separate visibility setting intended
     * for temporary effects like window popups,
     * so as to not override the default visibility setting.
     * @note Do not confuse with setVisible!
     * @param hidden Shown or hidden.
     */
    internal void setHidden(bool hidden) =>
        _hidden = hidden;

    /**
     * Set the surface to be redrawn.
     * @param valid true means redraw.
     */
    internal void invalidate(bool valid = true) =>
	    _redraw = valid;

    /**
     * Loads the contents of an image file of a
     * known format into the surface.
     * @param filename Filename of the image.
     */
    internal void loadImage(string filename)
    {
	    // Destroy current surface (will be replaced)
	    DeleteAligned(_alignedBuffer);
	    SDL_FreeSurface(_surface.pixels);
	    _alignedBuffer = nint.Zero;
	    _surface.pixels = nint.Zero;

        Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} Loading image: {filename}");

		string utf8 = Unicode.convPathToUtf8(filename);
        _surface.pixels = IMG_Load(utf8);

	    if (_surface.pixels == nint.Zero)
	    {
		    string err = filename + ":" + IMG_GetError();
		    throw new Exception(err);
	    }

	    _clear.w = getWidth();
	    _clear.h = getHeight();
    }

    /**
     * Returns the visible state of the surface.
     * @return Current visibility.
     */
    internal bool getVisible() =>
	    _visible;

    /**
     * Blits this surface onto another one, with its position
     * relative to the top-left corner of the target surface.
     * The cropping rectangle controls the portion of the surface
     * that is blitted.
     * @param surface Pointer to surface to blit onto.
     */
    internal virtual void blit(Surface surface)
    {
	    if (_visible && !_hidden)
	    {
		    if (_redraw)
			    draw();

		    SDL_Rect cropper;
		    var target = new SDL_Rect();
		    if (_crop.w == 0 && _crop.h == 0)
		    {
			    cropper = new SDL_Rect();
		    }
		    else
		    {
                cropper = _crop;
		    }
		    target.x = getX();
		    target.y = getY();
            SDL_BlitSurface(_surface.pixels, ref cropper, surface.getSurface().pixels, ref target);
	    }
    }

    /**
     * Runs any code the surface needs to keep updating every
     * game cycle, like animations and other real-time elements.
     */
    internal virtual void think() { }

    /**
     * Returns the cropping rectangle for this surface.
     * @return Pointer to the cropping rectangle.
     */
    internal ref SDL_Rect getCrop() =>
        ref _crop;

    /**
     * Changes the height of the surface.
     * @warning This is not a trivial setter!
     * It will force the surface to be recreated for the new size.
     * @param height New height in pixels.
     */
    internal void setHeight(int height)
    {
        resize(getWidth(), height);
        _redraw = true;
    }

    /**
     * Draws a filled rectangle on the surface.
     * @param rect Pointer to Rect.
     * @param color Color of the rectangle.
     */
    internal void drawRect(ref SDL_Rect rect, byte color) =>
        SDL_FillRect(_surface.pixels, ref rect, color);

    /**
     * Recreates the surface with a new size.
     * Old contents will not be altered, and may be
     * cropped to fit the new size.
     * @param width Width in pixels.
     * @param height Height in pixels.
     */
    void resize(int width, int height)
    {
        // Set up new surface
        var bpp = getFormat(_surface).BitsPerPixel;
        int pitch = GetPitch(bpp, width);
        nint alignedBuffer = NewAligned(bpp, width, height);
        nint surface = SDL_CreateRGBSurfaceFrom(alignedBuffer, width, height, bpp, pitch, 0, 0, 0, 0);

        if (surface == nint.Zero)
        {
            throw new Exception(SDL_GetError());
        }

        // Copy old contents
        SDL_SetColorKey(surface, (int)SDL_bool.SDL_TRUE, 0);
        SDL_SetPaletteColors(surface, getPaletteColors(), 0, 256);
        SDL_BlitSurface(_surface.pixels, nint.Zero, surface, nint.Zero);

        // Delete old surface
        DeleteAligned(_alignedBuffer);
        SDL_FreeSurface(_surface.pixels);
        _alignedBuffer = alignedBuffer;
        _surface.pixels = surface;

        _clear.w = getWidth();
        _clear.h = getHeight();
    }

    /**
     * Shifts all the colors in the surface by a set amount.
     * This is a common method in 8bpp games to simulate color
     * effects for cheap.
     * @param off Amount to shift.
     * @param min Minimum color to shift to.
     * @param max Maximum color to shift to.
     * @param mul Shift multiplier.
     */
    internal void offset(int off, int min = -1, int max = -1, int mul = 1)
    {
        if (off == 0)
            return;

        // Lock the surface
        @lock();

        for (int x = 0, y = 0; x < getWidth() && y < getHeight();)
        {
            byte pixel = getPixel(x, y);
            int p;
            if (off > 0)
            {
                p = pixel * mul + off;
            }
            else
            {
                p = (pixel + off) / mul;
            }
            if (min != -1 && p < min)
            {
                p = min;
            }
            else if (max != -1 && p > max)
            {
                p = max;
            }

            if (pixel > 0)
            {
                setPixelIterative(ref x, ref y, (byte)p);
            }
            else
            {
                setPixelIterative(ref x, ref y, 0);
            }
        }

        // Unlock the surface
        unlock();
    }

    /**
	 * Changes the color of a pixel in the surface and returns the
	 * next pixel position. Useful when changing a lot of pixels in
	 * a row, eg. manipulating images.
	 * @param x Pointer to the X position of the pixel. Changed to the next X position in the sequence.
	 * @param y Pointer to the Y position of the pixel. Changed to the next Y position in the sequence.
	 * @param pixel New color for the pixel.
	 */
    internal void setPixelIterative(ref int x, ref int y, byte pixel)
    {
        setPixel(x, y, pixel);
        x++;
        if (x == getWidth())
        {
            y++;
            x = 0;
        }
    }

    /**
     * Loads the contents of an X-Com SCR image file into
     * the surface. SCR files are simply uncompressed images
     * containing the palette offset of each pixel.
     * @param filename Filename of the SCR image.
     * @sa http://www.ufopaedia.org/index.php?title=Image_Formats#SCR_.26_DAT
     */
    internal void loadScr(string filename)
    {
        // Load file and put pixels in surface
        try
        {
            using var imgFile = new FileStream(filename, FileMode.Open);
            var buffer = new byte[imgFile.Length];
            imgFile.Read(buffer);
            imgFile.Close();
            loadRaw(buffer);
        }
        catch (Exception)
        {
		    throw new Exception(filename + " not found");
        }
    }

    /**
     * Loads the contents of a TFTD BDY image file into
     * the surface. BDY files are compressed with a custom
     * algorithm.
     * @param filename Filename of the BDY image.
     * @sa http://www.ufopaedia.org/index.php?title=Image_Formats#BDY
     */
    internal void loadBdy(string filename)
    {
        try
        {
	        // Load file and put pixels in surface
            using var imgFile = new FileStream(filename, FileMode.Open);

            // Lock the surface
            @lock();

	        byte dataByte;
	        int pixelCnt;
	        int x = 0, y = 0;
	        int currentRow = 0;

            int read;
            while ((read = imgFile.ReadByte()) != -1)
	        {
                dataByte = (byte)read;
                if (dataByte >= 129)
		        {
			        pixelCnt = 257 - (int)dataByte;
                    dataByte = (byte)imgFile.ReadByte();
			        currentRow = y;
			        for (int i = 0; i < pixelCnt; ++i)
			        {
				        setPixelIterative(ref x, ref y, dataByte);
				        if (currentRow != y) // avoid overscan into next row
					        break;
			        }
		        }
		        else
		        {
			        pixelCnt = 1 + (int)dataByte;
			        currentRow = y;
			        for (int i = 0; i < pixelCnt; ++i)
			        {
                        dataByte = (byte)imgFile.ReadByte();
				        if (currentRow == y) // avoid overscan into next row
					        setPixelIterative(ref x, ref y, dataByte);
			        }
		        }
	        }

	        // Unlock the surface
	        unlock();

	        imgFile.Close();
        }
        catch (Exception)
        {
		    throw new Exception(filename + " not found");
        }
    }

    /**
     * Loads the contents of an X-Com SPK image file into
     * the surface. SPK files are compressed with a custom
     * algorithm since they're usually full-screen images.
     * @param filename Filename of the SPK image.
     * @sa http://www.ufopaedia.org/index.php?title=Image_Formats#SPK
     */
    internal void loadSpk(string filename)
    {
        try
        {
	        // Load file and put pixels in surface
            using var imgFile = new FileStream(filename, FileMode.Open);

            // Lock the surface
            @lock();

	        ushort flag;
	        byte value;
            var buffer = new byte[2];
	        int x = 0, y = 0;

            while (imgFile.Read(buffer, 0, 2) != 0)
	        {
                flag = BitConverter.ToUInt16(buffer);

		        if (flag == 65535)
		        {
			        imgFile.Read(buffer, 0, 2);
                    flag = BitConverter.ToUInt16(buffer);

                    for (int i = 0; i < flag * 2; ++i)
			        {
				        setPixelIterative(ref x, ref y, 0);
			        }
		        }
		        else if (flag == 65534)
		        {
                    imgFile.Read(buffer, 0, 2);
                    flag = BitConverter.ToUInt16(buffer);

                    for (int i = 0; i < flag * 2; ++i)
			        {
                        value = (byte)imgFile.ReadByte();
                        setPixelIterative(ref x, ref y, value);
			        }
		        }
	        }

	        // Unlock the surface
	        unlock();

	        imgFile.Close();
        }
        catch (Exception)
        {
		    throw new Exception(filename + " not found");
        }
    }

    /**
     * Loads a raw array of pixels into the surface. The pixels must be
     * in the same BPP as the surface.
     * @param bytes Pixel array.
     */
    void loadRaw(byte[] bytes)
    {
        @lock();
	    rawCopy(bytes);
	    unlock();
    }

    /**
     * Performs a fast copy of a pixel array, accounting for pitch.
     * @param src Source array.
     */
    unsafe void rawCopy<T>(T[] src)
    {
	    // Copy whole thing
	    if (_surface.pitch == _surface.w)
	    {
            int end = Math.Min(_surface.w * _surface.h * getFormat(_surface).BytesPerPixel, src.Length);
            var source = MemoryExtensions.AsMemory(src, 0, end);
            Unsafe.Copy(_surface.pixels.ToPointer(), ref source);
	    }
	    // Copy row by row
	    else
	    {
		    for (int y = 0; y < _surface.h; ++y)
		    {
                int begin = y * _surface.w;
                int end = Math.Min(begin + _surface.w, src.Length);
			    if (begin >= src.Length)
				    break;
                var source = MemoryExtensions.AsMemory(src, begin, end - begin);
                Unsafe.Copy(getRaw(0, y).ToPointer(), ref source);
		    }
	    }
    }
}
