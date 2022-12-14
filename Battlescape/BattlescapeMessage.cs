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

namespace SharpXcom.Battlescape;

/**
 * Generic window used to display messages
 * over the Battlescape map.
 */
internal class BattlescapeMessage : Surface
{
    Window _window;
    Text _text;

    /**
     * Sets up a blank Battlescape message with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal BattlescapeMessage(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _window = new Window(null, width, height, x, y, WindowPopup.POPUP_NONE);
        _window.setColor((byte)(Palette.blockOffset(0) - 1));
        _window.setHighContrast(true);

        _text = new Text(width, height, x, y);
        _text.setColor((byte)(Palette.blockOffset(0) - 1));
        _text.setAlign(TextHAlign.ALIGN_CENTER);
        _text.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
        _text.setHighContrast(true);
    }

    /**
     * Deletes surfaces.
     */
    ~BattlescapeMessage()
    {
        _window = null;
        _text = null;
    }

    /*
     * Sets the text color of the battlescape message.
     * @param color the new color.
     */
    internal void setTextColor(byte color) =>
        _text.setColor(color);
}
