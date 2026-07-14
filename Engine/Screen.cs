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
 * A display screen, handles rendering onto the game window.
 * In SDL a Screen is treated like a Surface, so this is just
 * a specialized version of a Surface with functionality more
 * relevant for display screens. Contains a Surface buffer
 * where all the contents are kept, so any filters or conversions
 * can be applied before rendering the screen.
 */
internal class Screen
{
    internal const int ORIGINAL_WIDTH = 320;
    internal const int ORIGINAL_HEIGHT = 200;
    const int VIDEO_WINDOW_POS_LEN = 40;
    const string SDL_VIDEO_CENTERED_UNSET = "SDL_VIDEO_CENTERED=";
    const string SDL_VIDEO_CENTERED_CENTER = "SDL_VIDEO_CENTERED=center";
    const string SDL_VIDEO_WINDOW_POS_UNSET = "SDL_VIDEO_WINDOW_POS=";

    static char[] VIDEO_WINDOW_POS = new char[VIDEO_WINDOW_POS_LEN];
    int _baseWidth, _baseHeight;
    double _scaleX, _scaleY;
    SDL_WindowFlags _flags;
    int _numColors, _firstColor;
    bool _pushPalette;
    unsafe SDL_Color* deferredPalette; //SDL_Color[] deferredPalette = new SDL_Color[256];
    int _topBlackBand, _bottomBlackBand, _leftBlackBand, _rightBlackBand, _cursorTopBlackBand, _cursorLeftBlackBand;
    SDL_Rect _clear;
    int _bpp;
    OpenGL glOutput;
    unsafe SDL_Window* _screen;
    unsafe static SDL_Renderer* _renderer;
    unsafe SDL_Texture* _texture;
    Surface _surface;

    unsafe internal static SDL_Renderer* Renderer => _renderer;

    /**
     * Initializes a new display screen for the game to render contents to.
     * The screen is set up based on the current options.
     */
    internal Screen()
    {
        _baseWidth = ORIGINAL_WIDTH;
        _baseHeight = ORIGINAL_HEIGHT;
        _scaleX = 1.0;
        _scaleY = 1.0;
        _flags = 0;
        _numColors = 0;
        _firstColor = 0;
        _pushPalette = false;
        _surface = null;

        resetDisplay();
    }

    /**
     * Deletes the buffer from memory. The display screen itself
     * is automatically freed once SDL shuts down.
     */
    ~Screen() =>
        _surface = null;

    /**
     * Changes a given scale, and if necessary, switch the current base resolution.
     * @param type the new scale level.
     * @param width reference to which x scale to adjust.
     * @param height reference to which y scale to adjust.
     * @param change should we change the current scale.
     */
    internal static void updateScale(int type, ref int width, ref int height, bool change)
    {
        double pixelRatioY = 1.0;

        if (Options.nonSquarePixelRatio)
        {
            pixelRatioY = 1.2;
        }

        switch ((ScaleType)type)
        {
            case ScaleType.SCALE_15X:
                width = (int)(ORIGINAL_WIDTH * 1.5);
                height = (int)(ORIGINAL_HEIGHT * 1.5);
                break;
            case ScaleType.SCALE_2X:
                width = ORIGINAL_WIDTH * 2;
                height = ORIGINAL_HEIGHT * 2;
                break;
            case ScaleType.SCALE_SCREEN_DIV_3:
                width = (int)(Options.displayWidth / 3.0);
                height = (int)(Options.displayHeight / pixelRatioY / 3.0);
                break;
            case ScaleType.SCALE_SCREEN_DIV_2:
                width = (int)(Options.displayWidth / 2.0);
                height = (int)(Options.displayHeight / pixelRatioY / 2.0);
                break;
            case ScaleType.SCALE_SCREEN:
                width = Options.displayWidth;
                height = (int)(Options.displayHeight / pixelRatioY);
                break;
            case ScaleType.SCALE_ORIGINAL:
            default:
                width = ORIGINAL_WIDTH;
                height = ORIGINAL_HEIGHT;
                break;
        }

        // don't go under minimum resolution... it's bad, mmkay?
        width = Math.Max(width, ORIGINAL_WIDTH);
        height = Math.Max(height, ORIGINAL_HEIGHT);

        if (change && (Options.baseXResolution != width || Options.baseYResolution != height))
        {
            Options.baseXResolution = width;
            Options.baseYResolution = height;
        }
    }

    /**
     * Gets the Horizontal offset from the mid-point of the screen, in pixels.
     * @return the horizontal offset.
     */
    internal int getDX() =>
        (_baseWidth - ORIGINAL_WIDTH) / 2;

    /**
     * Gets the Vertical offset from the mid-point of the screen, in pixels.
     * @return the vertical offset.
     */
    internal int getDY() =>
        (_baseHeight - ORIGINAL_HEIGHT) / 2;

    /**
     * Returns the screen's 8bpp palette.
     * @return Pointer to the palette's colors.
     */
    unsafe internal SDL_Color* getPalette() =>
        deferredPalette;

    /**
     * Resets the screen surfaces based on the current display options,
     * as they don't automatically take effect.
     * @param resetVideo Reset display surface.
     */
    unsafe internal void resetDisplay(bool resetVideo = true)
    {
        int width = Options.displayWidth;
        int height = Options.displayHeight;

        makeVideoFlags();

        if (_surface == null || (Surface.getFormat(_surface.getSurface())->bits_per_pixel != _bpp ||
            _surface.getSurface()->w != _baseWidth ||
            _surface.getSurface()->h != _baseHeight)) // don't reallocate _surface if not necessary, it's a waste of CPU cycles
        {
            if (_surface != null) _surface = null;
            _surface = new Surface(_baseWidth, _baseHeight, 0, 0, use32bitScaler() ? 32 : 8); // only HQX/XBRZ needs 32bpp for this surface; the OpenGL class has its own 32bpp buffer
            if (Surface.getFormat(_surface.getSurface())->bits_per_pixel == 8) _surface.setPalette(deferredPalette);
        }
        SDL_SetSurfaceColorKey(_surface.getSurface(), false, 0); // turn off color key!

        if (resetVideo || getBitsPerPixel() != _bpp)
        {
            if (OperatingSystem.IsLinux())
            {
                SDL_WindowFlags oldFlags = _flags;
                // Workaround for segfault when switching to opengl
                if (!((oldFlags & SDL_WindowFlags.SDL_WINDOW_OPENGL) != 0) && (_flags & SDL_WindowFlags.SDL_WINDOW_OPENGL) != 0)
                {
                    byte* cursor = (byte*)Marshal.AllocHGlobal(1);
                    string _oldtitle = SDL_GetWindowTitle(_screen); //SDL_WM_GetCaption(&_oldtitle, NULL);
                    string title = _oldtitle;
                    SDL_QuitSubSystem(SDL_InitFlags.SDL_INIT_VIDEO);
                    SDL_InitSubSystem(SDL_InitFlags.SDL_INIT_VIDEO);
                    SDL_ShowCursor(/* SDL_ENABLE */);
                    //SDL_EnableUNICODE(1);
                    SDL_SetWindowTitle(_screen, title); //SDL_WM_SetCaption(title, 0);
                    SDL_SetCursor(SDL_CreateCursor(cursor, cursor, 1, 1, 0, 0));
                }
            }

            Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Attempting to set display to {width}x{height}x{_bpp}...");
            SDL_PropertiesID props = SDL_CreateProperties();
            SDL_SetStringProperty(props, SDL_PROP_WINDOW_CREATE_TITLE_STRING, string.Empty);
            SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_X_NUMBER, SDL_WINDOWPOS_UNDEFINED);
            SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_Y_NUMBER, SDL_WINDOWPOS_UNDEFINED);
            SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_WIDTH_NUMBER, width);
            SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_HEIGHT_NUMBER, height);
            SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_FLAGS_NUMBER, (long)_flags);
            _screen = SDL_CreateWindowWithProperties(props);
            _renderer = SDL_CreateRenderer(_screen, (byte*)null);
            _texture = SDL_CreateTexture(_renderer, SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888, SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, width, height);
            if (_screen == null)
            {
                Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} {SDL_GetError()}");
                Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Attempting to set display to default resolution...");
                SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_WIDTH_NUMBER, 640);
                SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_HEIGHT_NUMBER, 400);
                _screen = SDL_CreateWindowWithProperties(props);
                _renderer = SDL_CreateRenderer(_screen, (byte*)null);
                _texture = SDL_CreateTexture(_renderer, SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888, SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, 640, 400);
                if (_screen == null)
                {
                    if ((_flags & SDL_WindowFlags.SDL_WINDOW_OPENGL) != 0)
                    {
                        Options.useOpenGL = false;
                    }
                    throw new Exception(SDL_GetError());
                }
            }
            SDL_DestroyProperties(props);
            Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Display set to {getWidth()}x{getHeight()}x{(int)getBitsPerPixel()}.");
        }
        else
        {
            clear();
        }

        Options.displayWidth = getWidth();
        Options.displayHeight = getHeight();
        _scaleX = getWidth() / (double)_baseWidth;
        _scaleY = getHeight() / (double)_baseHeight;
        _clear.x = 0;
        _clear.y = 0;
        _clear.w = getWidth();
        _clear.h = getHeight();

        double pixelRatioY = 1.0;
        if (Options.nonSquarePixelRatio && !Options.allowResize)
        {
            pixelRatioY = 1.2;
        }
        bool cursorInBlackBands;
        if (!Options.keepAspectRatio)
        {
            cursorInBlackBands = false;
        }
        else if (Options.fullscreen)
        {
            cursorInBlackBands = Options.cursorInBlackBandsInFullscreen;
        }
        else if (!Options.borderless)
        {
            cursorInBlackBands = Options.cursorInBlackBandsInWindow;
        }
        else
        {
            cursorInBlackBands = Options.cursorInBlackBandsInBorderlessWindow;
        }

        if (_scaleX > _scaleY && Options.keepAspectRatio)
        {
            int targetWidth = (int)Math.Floor(_scaleY * (double)_baseWidth);
            _topBlackBand = _bottomBlackBand = 0;
            _leftBlackBand = (getWidth() - targetWidth) / 2;
            if (_leftBlackBand < 0)
            {
                _leftBlackBand = 0;
            }
            _rightBlackBand = getWidth() - targetWidth - _leftBlackBand;
            _cursorTopBlackBand = 0;

            if (cursorInBlackBands)
            {
                _scaleX = _scaleY;
                _cursorLeftBlackBand = _leftBlackBand;
            }
            else
            {
                _cursorLeftBlackBand = 0;
            }
        }
        else if (_scaleY > _scaleX && Options.keepAspectRatio)
        {
            int targetHeight = (int)Math.Floor(_scaleX * (double)_baseHeight * pixelRatioY);
            _topBlackBand = (getHeight() - targetHeight) / 2;
            if (_topBlackBand < 0)
            {
                _topBlackBand = 0;
            }
            _bottomBlackBand = getHeight() - targetHeight - _topBlackBand;
            if (_bottomBlackBand < 0)
            {
                _bottomBlackBand = 0;
            }
            _leftBlackBand = _rightBlackBand = 0;
            _cursorLeftBlackBand = 0;

            if (cursorInBlackBands)
            {
                _scaleY = _scaleX;
                _cursorTopBlackBand = _topBlackBand;
            }
            else
            {
                _cursorTopBlackBand = 0;
            }
        }
        else
        {
            _topBlackBand = _bottomBlackBand = _leftBlackBand = _rightBlackBand = _cursorTopBlackBand = _cursorLeftBlackBand = 0;
        }

        if (useOpenGL())
        {
#if !__NO_OPENGL
            OpenGL.checkErrors = Options.checkOpenGLErrors;
            glOutput.init(_baseWidth, _baseHeight);
            glOutput.linear = Options.useOpenGLSmoothing; // setting from shader file will override this, though
            if (!FileMap.isResourcesEmpty())
            {
                if (!glOutput.set_shader(FileMap.getFilePath(Options.useOpenGLShader)))
                {
                    Options.useOpenGLShader = string.Empty;
                }
            }
            glOutput.setVSync(Options.vSyncForOpenGL);
#endif
        }

        if (getBitsPerPixel() == 8)
        {
            setPalette(getPalette());
        }
    }

    /**
     * Sets up all the internal display flags depending on
     * the current video settings.
     */
    void makeVideoFlags()
    {
        //_flags = SDL_HWSURFACE | SDL_DOUBLEBUF | SDL_HWPALETTE;
        //if (Options.asyncBlit)
        //{
        //    _flags |= SDL_ASYNCBLIT;
        //}
        if (useOpenGL())
        {
            _flags = SDL_WindowFlags.SDL_WINDOW_OPENGL;
            SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_RED_SIZE, 5);
            SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_GREEN_SIZE, 5);
            SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_BLUE_SIZE, 5);
            SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_DEPTH_SIZE, 16);
            SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_DOUBLEBUFFER, 1);
        }
        if (Options.allowResize)
        {
            _flags |= SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
        }

        // Handle window positioning
        if (!Options.fullscreen && Options.rootWindowedMode)
        {
            VIDEO_WINDOW_POS = $"SDL_VIDEO_WINDOW_POS={Options.windowedModePositionX},{Options.windowedModePositionY}".ToCharArray(0, VIDEO_WINDOW_POS_LEN);
            Environment.SetEnvironmentVariable("VIDEO_WINDOW_POS", new string(VIDEO_WINDOW_POS));
            Environment.SetEnvironmentVariable("SDL_VIDEO_CENTERED_UNSET", SDL_VIDEO_CENTERED_UNSET);
        }
        else if (Options.borderless)
        {
            Environment.SetEnvironmentVariable("SDL_VIDEO_WINDOW_POS_UNSET", SDL_VIDEO_WINDOW_POS_UNSET);
            Environment.SetEnvironmentVariable("SDL_VIDEO_CENTERED_CENTER", SDL_VIDEO_CENTERED_CENTER);
        }
        else
        {
            Environment.SetEnvironmentVariable("SDL_VIDEO_WINDOW_POS_UNSET", SDL_VIDEO_WINDOW_POS_UNSET);
            Environment.SetEnvironmentVariable("SDL_VIDEO_CENTERED_UNSET", SDL_VIDEO_CENTERED_UNSET);
        }

        // Handle display mode
        if (Options.fullscreen)
        {
            _flags |= SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
        }
        if (Options.borderless)
        {
            _flags |= SDL_WindowFlags.SDL_WINDOW_BORDERLESS; //SDL_NOFRAME
        }

        _bpp = (use32bitScaler() || useOpenGL()) ? 32 : 8;
        _baseWidth = Options.baseXResolution;
        _baseHeight = Options.baseYResolution;
    }

    /**
     * Returns the width of the screen.
     * @return Width in pixels.
     */
    unsafe internal int getWidth()
    {
        var surface = SDL_GetWindowSurface(_screen);
        return surface->w;
    }

    /**
     * Returns the height of the screen.
     * @return Height in pixels
     */
    unsafe internal int getHeight()
    {
        var surface = SDL_GetWindowSurface(_screen);
        return surface->h;
    }

    unsafe internal byte getBitsPerPixel()
    {
        var surface = SDL_GetWindowSurface(_screen);
        return Surface.getFormat(surface)->bits_per_pixel;
    }

    unsafe internal SDL_Window* getWindow() =>
        _screen;

    /**
     * Returns the screen's internal buffer surface. Any
     * contents that need to be shown will be blitted to this.
     * @return Pointer to the buffer surface.
     */
    internal Surface getSurface()
    {
        _pushPalette = true;
        return _surface;
    }

    /**
     * Check if OpenGL is enabled.
     * @return if it is enabled.
     */
    internal static bool useOpenGL()
    {
#if __NO_OPENGL
	    return false;
#else
        return Options.useOpenGL;
#endif
    }

    /**
     * Check whether a 32bpp scaler has been selected.
     * @return if it is enabled with a compatible resolution.
     */
    static bool use32bitScaler()
    {
        int w = Options.displayWidth;
        int h = Options.displayHeight;
        int baseW = Options.baseXResolution;
        int baseH = Options.baseYResolution;
        int maxScale = 0;

        if (Options.useHQXFilter)
        {
            maxScale = 4;
        }
        else if (Options.useXBRZFilter)
        {
            maxScale = 6;
        }

        for (int i = 2; i <= maxScale; i++)
        {
            if (w == baseW * i && h == baseH * i)
            {
                return true;
            }
        }
        return false;
    }

    /**
     * Clears all the contents out of the internal buffer.
     */
    unsafe internal void clear()
    {
        _surface.clear();
        //TODO: Needed?
        //if ((_flags & SDL_SWSURFACE) != 0)
        //{
        //    var surface = SDL_GetWindowSurface(_screen);
        //    NativeMemory.Fill((void*)surface->pixels, (nuint)(surface->h * surface->pitch), 0); //memset(_screen->pixels, 0, _screen->h*_screen->pitch);
        //}
        /*else*/ fixed (SDL_Rect* p = &_clear) { SDL_FillSurfaceRect(SDL_GetWindowSurface(_screen), p, 0); }
    }

    /**
     * Changes the 8bpp palette used to render the screen's contents.
     * @param colors Pointer to the set of colors.
     * @param firstcolor Offset of the first color to replace.
     * @param ncolors Amount of colors to replace.
     * @param immediately Apply palette changes immediately, otherwise wait for next blit.
     */
    unsafe internal void setPalette(SDL_Color* colors, int firstcolor = 0, int ncolors = 256, bool immediately = false)
    {
        if (_numColors != 0 && (_numColors != ncolors) && (_firstColor != firstcolor))
	    {
            // an initial palette setup has not been committed to the screen yet
            // just update it with whatever colors are being sent now
            NativeMemory.Copy(colors, &deferredPalette[firstcolor], (nuint)(4 * ncolors)); //memmove(&(deferredPalette[firstcolor]), colors, sizeof(SDL_Color) * ncolors);
            _numColors = 256; // all the use cases are just a full palette with 16-color follow-ups
            _firstColor = 0;
	    }
        else
	    {
            NativeMemory.Copy(colors, &deferredPalette[firstcolor], (nuint)(4 * ncolors)); //memmove(&(deferredPalette[firstcolor]), colors, sizeof(SDL_Color) * ncolors);
            _numColors = ncolors;
            _firstColor = firstcolor;
	    }

        _surface.setPalette(colors, firstcolor, ncolors);

        // defer actual update of screen until SDL_Flip()
        if (immediately && getBitsPerPixel() == 8 && !SDL_SetPaletteColors(SDL_GetSurfacePalette(SDL_GetWindowSurface(_screen)), colors, firstcolor, ncolors))
        {
            Console.WriteLine($"{Log(SeverityLevel.LOG_DEBUG)} Display palette doesn't match requested palette");
        }

	    // Sanity check
	    /*
	    SDL_Color *newcolors = _screen->format->palette->colors;
	    for (int i = firstcolor, j = 0; i < firstcolor + ncolors; i++, j++)
	    {
		    Log(LOG_DEBUG) << (int)newcolors[i].r << " - " << (int)newcolors[i].g << " - " << (int)newcolors[i].b;
		    Log(LOG_DEBUG) << (int)colors[j].r << " + " << (int)colors[j].g << " + " << (int)colors[j].b;
		    if (newcolors[i].r != colors[j].r ||
			    newcolors[i].g != colors[j].g ||
			    newcolors[i].b != colors[j].b)
		    {
			    Log(LOG_ERROR) << "Display palette doesn't match requested palette";
			    break;
		    }
	    }
	    */
    }

    /**
     * Returns the screen's X scale.
     * @return Scale factor.
     */
    internal double getXScale() =>
        _scaleX;

    /**
     * Returns the screen's Y scale.
     * @return Scale factor.
     */
    internal double getYScale() =>
        _scaleY;

    /**
     * Returns the screen's top black forbidden to cursor band's height.
     * @return Height in pixel.
     */
    internal int getCursorTopBlackBand() =>
        _cursorTopBlackBand;

    /**
     * Returns the screen's left black forbidden to cursor band's width.
     * @return Width in pixel.
     */
    internal int getCursorLeftBlackBand() =>
        _cursorLeftBlackBand;

    /**
     * Handles screen key shortcuts.
     * @param action Pointer to an action.
     */
    internal void handle(Action action)
    {
        if (Options.debug)
        {
            if (action.getDetails().Type == SDL_EventType.SDL_EVENT_KEY_DOWN && action.getDetails().key.key == SDL_Keycode.SDLK_F8)
            {
                switch (Timer.gameSlowSpeed)
                {
                    case 1: Timer.gameSlowSpeed = 5; break;
                    case 5: Timer.gameSlowSpeed = 15; break;
                    default: Timer.gameSlowSpeed = 1; break;
                }
            }
        }

        if (action.getDetails().Type == SDL_EventType.SDL_EVENT_KEY_DOWN && action.getDetails().key.key == SDL_Keycode.SDLK_RETURN && (SDL_GetModState() & SDL_Keymod.SDL_KMOD_ALT) != 0)
        {
            Options.fullscreen = !Options.fullscreen;
            resetDisplay();
        }
        else if (action.getDetails().Type == SDL_EventType.SDL_EVENT_KEY_DOWN && action.getDetails().key.key == Options.keyScreenshot)
        {
            string ss;
            int i = 0;
            do
            {
                ss = $"{Options.getMasterUserFolder()}screen{i:D3}.png";
                i++;
            }
            while (CrossPlatform.fileExists(ss));
            screenshot(ss);
            return;
        }
    }

    /**
     * Renders the buffer's contents onto the screen, applying
     * any necessary filters or conversions in the process.
     * If the scaling factor is bigger than 1, the entire contents
     * of the buffer are resized by that factor (eg. 2 = doubled)
     * before being put on screen.
     */
    unsafe internal void flip()
    {
        var windowSurface = SDL_GetWindowSurface(_screen);
        if (getWidth() != _baseWidth || getHeight() != _baseHeight || useOpenGL())
        {
            Zoom.flipWithZoom(_surface.getSurface(), _screen, _topBlackBand, _bottomBlackBand, _leftBlackBand, _rightBlackBand, glOutput);
        }
        else
        {
            SDL_BlitSurface(_surface.getSurface(), null, windowSurface, null);
        }
        
        // perform any requested palette update
        if (_pushPalette && _numColors != 0 && getBitsPerPixel() == 8)
        {
            if (getBitsPerPixel() == 8 && !SDL_SetPaletteColors(SDL_GetSurfacePalette(windowSurface), &deferredPalette[_firstColor], _firstColor, _numColors))
            {
                Console.WriteLine($"{Log(SeverityLevel.LOG_DEBUG)} Display palette doesn't match requested palette");
            }
            _numColors = 0;
            _pushPalette = false;
        }
        
        SDL_UpdateTexture(_texture, null, windowSurface->pixels, windowSurface->pitch);
        SDL_RenderClear(_renderer);
        SDL_RenderTexture(_renderer, _texture, null, null);
        SDL_RenderPresent(_renderer); //SDL_Flip(_screen)
    }

    /**
     * Saves a screenshot of the screen's contents.
     * @param filename Filename of the PNG file.
     */
    unsafe void screenshot(string filename)
    {
        SDL_Surface* screenshot = SDL_CreateSurface(getWidth() - getWidth() % 4, getHeight(), SDL_GetPixelFormatForMasks(24, 0xff, 0xff00, 0xff0000, 0)); //SDL_AllocSurface

        if (useOpenGL())
        {
#if !__NO_OPENGL
            uint format = GL_RGB;

            for (int y = 0; y < getHeight(); ++y)
            {
                glReadPixels(0, getHeight() - (y + 1), getWidth() - getWidth() % 4, 1, format, GL_UNSIGNED_BYTE, nint.Add(screenshot->pixels, y * screenshot->pitch));
            }
            glErrorCheck();
#endif
        }
        else
        {
            SDL_BlitSurface(SDL_GetWindowSurface(_screen), null, screenshot, null);
        }

        //unsigned error = lodepng::encode(filename, (const unsigned char *)(screenshot->pixels), getWidth() - getWidth()%4, getHeight(), LCT_RGB);
        if (!IMG_SavePNG(screenshot, filename))
        {
            Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Saving to PNG failed: {SDL_GetError()}");
        }

        SDL_DestroySurface(screenshot);
    }
}
