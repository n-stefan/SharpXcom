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
 * Fancy frame border thing used for windows and other elements.
 */
internal class Frame : Surface
{
    byte _color, _bg;
    int _thickness;
    bool _contrast;

    /**
     * Sets up a blank frame with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal Frame(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _color = 0;
        _bg = 0;
        _thickness = 5;
        _contrast = false;
    }

    /**
     *
     */
    ~Frame() { }

    /**
     * Changes the thickness of the border to draw.
     * @param thickness Thickness in pixels.
     */
    internal void setThickness(int thickness)
    {
        _thickness = thickness;
        _redraw = true;
    }

    /**
     * Changes the color used to draw the shaded border.
     * @param color Color value.
     */
    internal void setColor(byte color)
    {
        _color = color;
        _redraw = true;
    }
}
