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
 * Mouse cursor that replaces the system cursor.
 * Drawn as a shaded triangle-like shape, automatically
 * matches the mouse coordinates.
 */
internal class Cursor : Surface
{
    private byte _color;

    /**
     * Sets up a cursor with the specified size and position
     * and hides the system cursor.
     * @note The size and position don't really matter since
     * it's a 9x13 shape, they're just there for inheritance.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal Cursor(int width, int height, int x = 0, int y = 0) : base(width, height, x, y) =>
        _color = 0;

    /**
     *
     */
    ~Cursor() { }

    /**
     * Returns the cursor's base color.
     * @return Color value.
     */
    internal byte getColor() =>
	    _color;

    /**
     * Draws a pointer-shaped cursor graphic.
     */
    internal override void draw()
    {
        base.draw();
        byte color = _color;
        int x1 = 0, y1 = 0, x2 = getWidth() - 1, y2 = getHeight() - 1;

        @lock();
        for (int i = 0; i < 4; ++i)
        {
            drawLine((short)x1, (short)y1, (short)x1, (short)y2, color);
            drawLine((short)x1, (short)y1, (short)x2, (short)(getWidth() - 1), color);
            x1++;
            y1 += 2;
            y2--;
            x2--;
            color++;
        }
        setPixel(4, 8, --color);
        unlock();
    }

    /**
     * Automatically updates the cursor position
     * when the mouse moves.
     * @param action Pointer to an action.
     */
    internal void handle(Action action)
    {
        if (action.getDetails().type == SDL_EventType.SDL_MOUSEMOTION)
        {
            setX((int)Math.Floor((action.getDetails().motion.x - action.getLeftBlackBand()) / action.getXScale()));
            setY((int)Math.Floor((action.getDetails().motion.y - action.getTopBlackBand()) / action.getYScale()));
        }
    }

    /**
     * Changes the cursor's base color.
     * @param color Color value.
     */
    internal override void setColor(byte color)
    {
        _color = color;
        _redraw = true;
    }
}
