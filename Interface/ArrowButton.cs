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

enum ArrowShape { ARROW_NONE, ARROW_BIG_UP, ARROW_BIG_DOWN, ARROW_SMALL_UP, ARROW_SMALL_DOWN, ARROW_SMALL_LEFT, ARROW_SMALL_RIGHT };

/**
 * Button with an arrow on it. Can be used for
 * scrolling lists, spinners, etc. Contains various
 * arrow shapes.
 */
internal class ArrowButton : ImageButton
{
    ArrowShape _shape;
    TextList _list;
    Engine.Timer _timer;

    /**
     * Sets up an arrow button with the specified size and position.
     * @param shape Shape of the arrow.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal ArrowButton(ArrowShape shape, int width, int height, int x, int y) : base(width, height, x, y)
    {
        _shape = shape;
        _list = null;

        _timer = new Engine.Timer(50);
        _timer.onTimer((SurfaceHandler)scroll);
    }

    /**
     * Deletes timers.
     */
    ~ArrowButton() =>
        _timer = null;

    /**
     * Scrolls the list.
     */
    void scroll()
    {
        if (_shape == ArrowShape.ARROW_BIG_UP)
        {
            _list.scrollUp(false);
        }
        else if (_shape == ArrowShape.ARROW_BIG_DOWN)
        {
            _list.scrollDown(false);
        }
    }

    /**
     * Changes the list associated with the arrow button.
     * This makes the button scroll that list.
     * @param list Pointer to text list.
     */
    internal void setTextList(TextList list) =>
        _list = list;

    /**
     * Changes the color for the image button.
     * @param color Color value.
     */
    internal void setColor(byte color)
    {
        base.setColor(color);
        _redraw = true;
    }


}
