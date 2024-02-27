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
 * Bar graphic that represents a certain value.
 * Drawn with a coloured border and partially
 * filled content to contrast two values, typically
 * used for showing base and soldier stats.
 */
internal class Bar : Surface
{
    byte _color, _color2, _borderColor;
    double _scale, _max, _value, _value2;
    bool _secondOnTop;

    /**
     * Sets up a blank bar with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal Bar(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _color = 0;
        _color2 = 0;
        _borderColor = 0;
        _scale = 0;
        _max = 0;
        _value = 0;
        _value2 = 0;
        _secondOnTop = true;
    }

    /**
     *
     */
    ~Bar() { }

    /**
     * Changes the maximum value used to draw the outer border.
     * @param max Maximum value.
     */
    internal void setMax(double max)
    {
        _max = max;
        _redraw = true;
    }

    /**
     * Changes the value used to draw the inner contents.
     * @param value Current value.
     */
    internal void setValue(double value)
    {
        _value = (value < 0.0) ? 0.0 : value;
        _redraw = true;
    }

    /**
     * Changes the value used to draw the second inner contents.
     * @param value Current value.
     */
    internal void setValue2(double value)
    {
        _value2 = (value < 0.0) ? 0.0 : value;
        _redraw = true;
    }

    /**
     * Changes the scale factor used to draw the bar values.
     * @param scale Scale in pixels/unit.
     */
    internal void setScale(double scale)
    {
        _scale = scale;
        _redraw = true;
    }

    /**
     * Returns the color used to draw the bar.
     * @return Color value.
     */
    internal byte getColor() =>
	    _color;

    /**
     * Draws the bordered bar filled according
     * to its values.
     */
    internal override void draw()
    {
	    base.draw();
	    SDL_Rect square;

	    square.x = 0;
	    square.y = 0;
	    square.w = (ushort)(_scale * _max) + 1;
	    square.h = getHeight();

	    if (_borderColor != 0)
		    drawRect(ref square, _borderColor);
	    else
		    drawRect(ref square, (byte)(_color + 4));

	    square.y++;
	    square.w--;
	    square.h -= 2;

	    drawRect(ref square, 0);

	    if (_secondOnTop)
	    {
		    square.w = (ushort)(_scale * _value);
		    drawRect(ref square, _color);
		    square.w = (ushort)(_scale * _value2);
		    drawRect(ref square, _color2);
	    }
	    else
	    {
		    square.w = (ushort)(_scale * _value2);
		    drawRect(ref square, _color2);
		    square.w = (ushort)(_scale * _value);
		    drawRect(ref square, _color);
	    }
    }
}
