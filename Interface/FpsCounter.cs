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

namespace SharpXcom.Interface;

/**
 * Counts the amount of frames each second
 * and displays them in a NumberText surface.
 */
internal class FpsCounter : Surface
{
	NumberText _text;
    Timer _timer;
    int _frames;

    /**
     * Creates a FPS counter of the specified size.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal FpsCounter(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _frames = 0;

        _visible = Options.fpsCounter;

        _timer = new Timer(1000);
        _timer.onTimer((SurfaceHandler)update);
        _timer.start();

        _text = new NumberText(width, height, x, y);
    }

    /**
     * Deletes FPS counter content.
     */
    ~FpsCounter()
    {
        _text = null;
        _timer = null;
    }

    /**
     * Replaces a certain amount of colors in the FPS counter palette.
     * @param colors Pointer to the set of colors.
     * @param firstcolor Offset of the first color to replace.
     * @param ncolors Amount of colors to replace.
     */
    internal override void setPalette(SDL_Color[] colors, int firstcolor = 0, int ncolors = 256)
    {
        base.setPalette(colors, firstcolor, ncolors);
        _text.setPalette(colors, firstcolor, ncolors);
    }

    /**
     * Updates the amount of Frames per Second.
     */
    void update()
    {
        int fps = (int)Math.Floor((double)_frames / _timer.getTime() * 1000);
        _text.setValue((uint)fps);
        _frames = 0;
        _redraw = true;
    }

    internal void addFrame() =>
        _frames++;

    /**
     * Advances frame counter.
     */
    internal override void think() =>
        _timer.think(null, this);

    /**
     * Sets the text color of the counter.
     * @param color The color to set.
     */
    internal override void setColor(byte color) =>
        _text.setColor(color);

    /**
     * Shows / hides the FPS counter.
     * @param action Pointer to an action.
     */
    internal void handle(Action action)
    {
        if (action.getDetails().type == SDL_EventType.SDL_KEYDOWN && action.getDetails().key.keysym.sym == Options.keyFps)
        {
            _visible = !_visible;
            Options.fpsCounter = _visible;
        }
    }

    /**
     * Draws the FPS counter.
     */
    internal override void draw()
    {
        base.draw();
        _text.blit(this);
    }
}
