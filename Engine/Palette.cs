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
 * Container for palettes (sets of 8bpp colors).
 * Works as an encapsulation for SDL's SDL_Color struct and
 * provides shortcuts for common tasks to make code more readable.
 */
internal class Palette
{
    /// Position of the background colors block in an X-Com palette (used for background images in screens).
    internal const int backPos = 224;

    SDL_Color[] _colors;
    int _count;

    /**
     * Initializes a brand new palette.
     */
    internal Palette()
    {
        //_colors(0)
        _count = 0;
    }

    /**
     * Deletes any colors contained within.
     */
    ~Palette() =>
        _colors = null;

    /**
     * Converts an SDL_Color struct into an hexadecimal RGBA color value.
     * Mostly used for operations with SDL_gfx that require colors in this format.
     * @param pal Requested palette.
     * @param color Requested color in the palette.
     * @return Hexadecimal RGBA value.
     */
    internal static uint getRGBA(SDL_Color[] pal, byte color) =>
        ((uint)pal[color].r << 24) | ((uint)pal[color].g << 16) | ((uint)pal[color].b << 8) | 0xFF;

    /// Gets the position of a certain color block in a palette.
    /**
	 * Returns the position of a certain color block in an X-Com palette (they're usually split in 16-color gradients).
	 * Makes setting element colors a lot easier than determining the exact color position.
	 * @param block Requested block.
	 * @return Color position.
	 */
    internal static byte blockOffset(byte block) =>
        (byte)(block * 16);

    /**
     * Provides access to colors contained in the palette.
     * @param offset Offset to a specific color.
     * @return Pointer to the requested SDL_Color.
     */
    internal Span<SDL_Color> getColors(int offset = 0) =>
        _colors.AsSpan(offset);

    /// Gets the position of a given palette.
    /**
	 * Returns the position of a palette inside an X-Com palette file (each is a 768-byte chunks).
	 * Handy for loading the palettes from the game files.
	 * @param palette Requested palette.
	 * @return Palette position in bytes.
	 */
    internal static int palOffset(int palette) =>
        palette * (768 + 6);

    /**
     * Loads an X-Com palette from a file. X-Com palettes are just a set
     * of RGB colors in a row, on a 0-63 scale, which have to be adjusted
     * for modern computers (0-255 scale).
     * @param filename Filename of the palette.
     * @param ncolors Number of colors in the palette.
     * @param offset Position of the palette in the file (in bytes).
     * @sa http://www.ufopaedia.org/index.php?title=PALETTES.DAT
     */
    internal void loadDat(string filename, int ncolors, int offset = 0)
    {
	    if (_colors != null)
		    throw new Exception("loadDat can be run only once");
	    _count = ncolors;
	    _colors = new SDL_Color[_count];

        try
        {
            // Load file and put colors in palette
            using var palFile = new FileStream(filename, FileMode.Open);

            // Move pointer to proper palette
            palFile.Seek(offset, SeekOrigin.Begin);

	        var value = new byte[3];

	        for (int i = 0; i < _count && palFile.Read(value, 0, 3) != 0; ++i)
	        {
		        // Correct X-Com colors to RGB colors
		        _colors[i].r = (byte)(value[0] * 4);
		        _colors[i].g = (byte)(value[1] * 4);
		        _colors[i].b = (byte)(value[2] * 4);
		        _colors[i].a = 255;
	        }
	        _colors[0].a = 0;

	        palFile.Close();
        }
        catch (Exception)
        {
		    throw new Exception(filename + " not found");
        }
    }

    internal void setColors(SDL_Color[] pal, int ncolors)
    {
        if (_colors != null)
            throw new Exception("setColors can be run only once");
        _count = ncolors;
        _colors = new SDL_Color[_count];

        for (int i = 0; i < _count; ++i)
        {
            // TFTD's LBM colors are good the way they are - no need for adjustment here, except...
            _colors[i].r = pal[i].r;
            _colors[i].g = pal[i].g;
            _colors[i].b = pal[i].b;
            _colors[i].a = 255;
            if (i > 15 && _colors[i].r == _colors[0].r &&
                _colors[i].g == _colors[0].g &&
                _colors[i].b == _colors[0].b)
            {
                // SDL "optimizes" surfaces by using RGB colour matching to reassign pixels to an "earlier" matching colour in the palette,
                // meaning any pixels in a surface that are meant to be black will be reassigned as colour 0, rendering them transparent.
                // avoid this eventuality by altering the "later" colours just enough to disambiguate them without causing them to look significantly different.
                // SDL 2.0 has some functionality that should render this hack unnecessary.
                _colors[i].r++;
                _colors[i].g++;
                _colors[i].b++;
            }
        }
        _colors[0].a = 0;
    }

    void savePal(string file)
    {
	    using var @out = new BinaryWriter(new FileStream(file, FileMode.Create));
	    short count = (short)_count;

	    // RIFF header
	    @out.Write("RIFF");
	    int length = 4 + 4 + 4 + 4 + 2 + 2 + count * 4;
	    @out.Write(length);
	    @out.Write("PAL ");

	    // Data chunk
	    @out.Write("data");
	    int data = count * 4 + 4;
	    @out.Write(data);
	    short version = 0x0300;
	    @out.Write(version);
	    @out.Write(count);

	    // Colors
	    Span<SDL_Color> color = getColors();
	    for (short i = 0; i < count; ++i)
	    {
		    byte c = 0;
            @out.Write(color[i].r);
            @out.Write(color[i].g);
            @out.Write(color[i].b);
		    @out.Write(c);
	    }
	    @out.Close();
    }
}
