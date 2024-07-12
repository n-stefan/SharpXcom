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
    internal override void setColor(byte color)
    {
        _color = color;
        _redraw = true;
    }

    /**
     * Draws the bordered frame with a graphic background.
     * The background never moves with the frame, it's
     * always aligned to the top-left corner of the screen
     * and cropped to fit the inside area.
     */
    internal override void draw()
    {
	    base.draw();
	    SDL_Rect square;

	    square.x = 0;
	    square.w = getWidth();
	    square.y = 0;
	    square.h = getHeight();

	    int mul = 1;
	    if (_contrast)
	    {
		    mul = 2;
	    }

	    // _color denotes our middle line color, so we start (half the thickness times the multiplier) steps darker and build up
	    byte color = (byte)(_color + ((1 + _thickness) * mul) / 2);
	    // we want the darkest version of this colour to outline any thick borders
	    byte darkest = (byte)(Palette.blockOffset((byte)(_color / 16)) + 15);
	    for (int i = 0; i < _thickness; ++i)
	    {
		    if (_thickness > 5 && (i == 0 || i == _thickness -1))
			    drawRect(ref square, darkest);
		    else
			    drawRect(ref square, color);
		    if (i < _thickness / 2)
			    color = (byte)(color - 1 * mul);
		    else
			    color = (byte)(color + 1 * mul);
		    square.x++;
		    square.y++;
		    if (square.w >= 2)
			    square.w -= 2;
		    else
			    square.w = 1;

		    if (square.h >= 2)
			    square.h -= 2;
		    else
			    square.h = 1;
	    }
	    drawRect(ref square, _bg);
    }

    /**
     * Returns the color used to draw the background.
     * @return Color value.
     */
    internal byte getSecondaryColor() =>
	    _bg;

    /**
     * Changes the color used to draw the background.
     * @param bg Color value.
     */
    internal override void setSecondaryColor(byte bg)
    {
	    _bg = bg;
	    _redraw = true;
    }

    /**
     * Changes the color used to draw the shaded border.
     * only really to be used in conjunction with the State add()
     * function as a convenience wrapper to avoid ugly hacks on that end
     * better to have them here!
     * @param color Color value.
     */
    internal override void setBorderColor(byte color) =>
	    setColor(color);

    /**
     * Enables/disables high contrast color. Mostly used for
     * Battlescape UI.
     * @param contrast High contrast setting.
     */
    internal override void setHighContrast(bool contrast)
    {
	    _contrast = contrast;
	    _redraw = true;
    }

    /**
     * Returns the color used to draw the shaded border.
     * @return Color value.
     */
    byte getColor() =>
	    _color;
}
