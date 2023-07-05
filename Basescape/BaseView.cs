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

namespace SharpXcom.Basescape;

/**
 * Interactive view of a base.
 * Takes a certain base and displays all its facilities
 * and status, allowing players to manage them.
 */
internal class BaseView : InteractiveSurface
{
    const int BASE_SIZE = 6;

    Base _base;
    SurfaceSet _texture;
    BaseFacility _selFacility;
    BaseFacility[,] _facilities = new BaseFacility[BASE_SIZE, BASE_SIZE];
    Font _big, _small;
    Language _lang;
    int _gridX, _gridY, _selSize;
    Surface _selector;
    bool _blink;
    Engine.Timer _timer;
    byte _cellColor, _selectorColor;

    /**
     * Sets up a base view with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal BaseView(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _base = null;
        _texture = null;
        _selFacility = null;
        _big = null;
        _small = null;
        _lang = null;
        _gridX = 0;
        _gridY = 0;
        _selSize = 0;
        _selector = null;
        _blink = true;

        // Clear grid
        for (int i = 0; i < BASE_SIZE; ++i)
        {
            for (int j = 0; j < BASE_SIZE; ++j)
            {
                _facilities[i, j] = null;
            }
        }

        _timer = new Engine.Timer(100);
        _timer.onTimer((SurfaceHandler)blink);
        _timer.start();
    }

    /**
     * Deletes contents.
     */
    ~BaseView()
    {
        _selector = null;
        _timer = null;
    }

    /**
     * Makes the facility selector blink.
     */
    void blink()
    {
        _blink = !_blink;

        if (_selSize > 0)
        {
            SDL_Rect r;
            if (_blink)
            {
                r.w = _selector.getWidth();
                r.h = _selector.getHeight();
                r.x = 0;
                r.y = 0;
                _selector.drawRect(ref r, _selectorColor);
                r.w -= 2;
                r.h -= 2;
                r.x++;
                r.y++;
                _selector.drawRect(ref r, 0);
            }
            else
            {
                r.w = _selector.getWidth();
                r.h = _selector.getHeight();
                r.x = 0;
                r.y = 0;
                _selector.drawRect(ref r, 0);
            }
        }
    }

    /**
     * Returns the facility the mouse is currently over.
     * @return Pointer to base facility (0 if none).
     */
    internal BaseFacility getSelectedFacility() =>
	    _selFacility;

    /**
     * Changes the texture to use for drawing
     * the various base elements.
     * @param texture Pointer to SurfaceSet to use.
     */
    internal void setTexture(SurfaceSet texture) =>
        _texture = texture;
}
