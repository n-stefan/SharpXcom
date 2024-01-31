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
 * Regular image that works like a button.
 * Unlike the TextButton, this button doesn't draw
 * anything on its own. It takes an existing graphic and
 * treats it as a button, inverting colors when necessary.
 * This is necessary for special buttons like in the Geoscape.
 */
internal class ImageButton : InteractiveSurface
{
    protected byte _color;
    ImageButton _group;
    bool _inverted;

    /**
     * Sets up an image button with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal ImageButton(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _color = 0;
        _group = null;
        _inverted = false;
    }

    /**
     *
     */
    ~ImageButton() { }

    /**
     * Returns the color for the image button.
     * @return Color value.
     */
    internal byte getColor() =>
	    _color;

    /**
     * Changes the color for the image button.
     * @param color Color value.
     */
    protected void setColor(byte color) =>
        _color = color;

    /**
     * Changes the button group this image button belongs to.
     * @param group Pointer to the pressed button pointer in the group.
     * Null makes it a regular button.
     */
    internal void setGroup(ImageButton group)
    {
        _group = group;
        if (_group != null && _group == this)
            invert((byte)(_color + 3));
    }
}
