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
 * Horizontal scrollbar control to select from a range of values.
 */
internal class ScrollBar : InteractiveSurface
{
    TextList _list;
    byte _color;
    bool _pressed, _contrast;
    int _offset;
    Surface _bg;
    Surface _track, _thumb;
    SDL_Rect _thumbRect;

    /**
     * Sets up a scrollbar with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal ScrollBar(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _list = null;
        _color = 0;
        _pressed = false;
        _contrast = false;
        _offset = 0;
        _bg = null;

        _track = new Surface(width - 2, height, x + 1, y);
        _thumb = new Surface(width, height, x, y);
        _thumbRect.x = 0;
        _thumbRect.y = 0;
        _thumbRect.w = 0;
        _thumbRect.h = 0;
    }

    /**
     * Deletes contents.
     */
    ~ScrollBar()
    {
        _track = null;
        _thumb = null;
    }

    /**
     * Changes the list associated with the scrollbar.
     * This makes the button scroll that list.
     * @param list Pointer to text list.
     */
    internal void setTextList(TextList list) =>
        _list = list;

    /**
     * Changes the surface used to draw the background of the track.
     * @param bg New background.
     */
    internal void setBackground(Surface bg) =>
        _bg = bg;

    /**
     * Changes the color used to render the scrollbar.
     * @param color Color value.
     */
    internal void setColor(byte color) =>
        _color = color;

    /**
     * Returns the color used to render the scrollbar.
     * @return Color value.
     */
    internal byte getColor() =>
	    _color;

    /**
     * Automatically updates the scrollbar
     * when the mouse moves.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void handle(Action action, State state)
    {
	    base.handle(action, state);
	    if (_pressed && (action.getDetails().type == SDL_EventType.SDL_MOUSEMOTION || action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN))
	    {
		    int cursorY = (int)(action.getAbsoluteYMouse() - getY());
		    int y = Math.Clamp(cursorY + _offset, 0, getHeight() - _thumbRect.h + 1);
		    double scale = (double)_list.getRows() / getHeight();
		    int scroll = (int)Math.Round(y * scale);
		    _list.scrollTo((uint)scroll);
	    }
    }
}
