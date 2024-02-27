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

    /**
     * Updates the thumb according to the current list position.
     */
    internal override void draw()
    {
	    base.draw();
	    drawTrack();
	    drawThumb();
    }

    /**
     * Draws the track (background bar) semi-transparent.
     */
    void drawTrack()
    {
	    if (_bg != null)
	    {
		    _track.copy(_bg);
		    if (_list.getComboBox() != null)
		    {
			    _track.offset(+1, Palette.backPos);
		    }
		    else
		    {
			    _track.offsetBlock(-5);
		    }
	    }
    }

    /**
     * Draws the thumb (button) as a hollow square.
     */
    void drawThumb()
    {
	    double scale = (double)getHeight() / _list.getRows();
	    _thumbRect.x = 0;
	    _thumbRect.y = (int)Math.Floor(_list.getScroll() * scale);
	    _thumbRect.w = _thumb.getWidth();
	    _thumbRect.h = (int)Math.Ceiling(_list.getVisibleRows() * scale);

	    // Draw base button
	    _thumb.clear();
	    _thumb.@lock();

	    SDL_Rect square = _thumbRect;
	    int color = _color + 2;

	    square.w--;
	    square.h--;

	    _thumb.drawRect(ref square, (byte)color);

	    square.x++;
	    square.y++;
	    color = _color + 5;

	    _thumb.drawRect(ref square, (byte)color);

	    square.w--;
	    square.h--;
	    color = _color + 4;

	    _thumb.drawRect(ref square, (byte)color);

	    _thumb.setPixel(_thumbRect.x, _thumbRect.y, (byte)(_color + 1));
	    _thumb.setPixel(_thumbRect.x, _thumbRect.y + _thumbRect.h - 1, (byte)(_color + 4));
	    _thumb.setPixel(_thumbRect.x + _thumbRect.w - 1, _thumbRect.y, (byte)(_color + 4));

	    // Hollow it out
	    if ((int)square.h - 4 > 0)
	    {
		    color = _color + 5;

		    square.x++;
		    square.y++;
		    square.w -= 3;
		    square.h -= 3;

		    _thumb.drawRect(ref square, (byte)color);

		    square.x++;
		    square.y++;
		    color = _color + 2;

		    _thumb.drawRect(ref square, (byte)color);

		    square.w--;
		    square.h--;
		    color = 0;

		    _thumb.drawRect(ref square, (byte)color);

		    _thumb.setPixel(_thumbRect.x + 2 + _thumbRect.w - 1 - 4, _thumbRect.y + 2 + _thumbRect.h - 1 - 4, (byte)(_color + 1));
		    _thumb.setPixel(_thumbRect.x + 2, _thumbRect.y + 2 + _thumbRect.h - 1 - 4, (byte)(_color + 4));
		    _thumb.setPixel(_thumbRect.x + 2 + _thumbRect.w - 1 - 4, _thumbRect.y + 2, (byte)(_color + 4));
	    }
	    _thumb.unlock();
    }

    /**
     * Blits the scrollbar contents.
     * @param surface Pointer to surface to blit onto.
     */
    protected override void blit(Surface surface)
    {
	    base.blit(surface);
	    if (_visible && !_hidden)
	    {
		    _track.blit(surface);
		    _thumb.blit(surface);
		    invalidate();
	    }
    }

	/**
	 * The scrollbar only moves while the button is pressed.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void mousePress(Action action, State state)
	{
		base.mousePress(action, state);
		if (action.getDetails().button.button == SDL_BUTTON_LEFT)
		{
			int cursorY = (int)(action.getAbsoluteYMouse() - getY());
			if (cursorY >= _thumbRect.y && cursorY < _thumbRect.y + _thumbRect.h)
			{
				_offset = _thumbRect.y - cursorY;
			}
			else
			{
				_offset = -_thumbRect.h / 2;
			}
			_pressed = true;
		}
		else if (action.getDetails().wheel.y > 0) //button.button == SDL_BUTTON_WHEELUP
		{
			_list.scrollUp(false, true);
		}
		else if (action.getDetails().wheel.y < 0) //button.button == SDL_BUTTON_WHEELDOWN
		{
			_list.scrollDown(false, true);
		}
	}

	/**
	 * The scrollbar stops moving when the button is released.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void mouseRelease(Action action, State state)
	{
		base.mouseRelease(action, state);
		if (action.getDetails().button.button == SDL_BUTTON_LEFT)
		{
			_pressed = false;
			_offset = 0;
		}
	}
}
