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
    Timer _timer;

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

        _timer = new Timer(50);
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

    /**
     * Changes the shape for the arrow button.
     * @param shape Shape of the arrow.
     */
    internal void setShape(ArrowShape shape)
    {
        _shape = shape;
        _redraw = true;
    }

    /**
     * Keeps the scrolling timers running.
     */
    protected override void think() =>
	    _timer.think(null, this);

	/**
	 * Draws the button with the specified arrow shape.
	 */
	protected override void draw()
	{
		base.draw();
		@lock();

		// Draw button
		SDL_Rect square;
		int color = _color + 2;

		square.x = 0;
		square.y = 0;
		square.w = getWidth() - 1;
		square.h = getHeight() - 1;

		drawRect(ref square, (byte)color);

		square.x++;
		square.y++;
		color = _color + 5;

		drawRect(ref square, (byte)color);

		square.w--;
		square.h--;
		color = _color + 4;

		drawRect(ref square, (byte)color);

		setPixel(0, 0, (byte)(_color + 1));
		setPixel(0, getHeight() - 1, (byte)(_color + 4));
		setPixel(getWidth() - 1, 0, (byte)(_color + 4));

		color = _color + 1;

		switch (_shape)
		{
		case ArrowShape.ARROW_BIG_UP:
			// Draw arrow square
			square.x = 5;
			square.y = 8;
			square.w = 3;
			square.h = 3;

			drawRect(ref square, (byte)color);

			// Draw arrow triangle
			square.x = 2;
			square.y = 7;
			square.w = 9;
			square.h = 1;

			for (; square.w > 1; square.w -= 2)
			{
				drawRect(ref square, (byte)color);
				square.x++;
				square.y--;
			}
			drawRect(ref square, (byte)color);
			break;
		case ArrowShape.ARROW_BIG_DOWN:
			// Draw arrow square
			square.x = 5;
			square.y = 3;
			square.w = 3;
			square.h = 3;

			drawRect(ref square, (byte)color);

			// Draw arrow triangle
			square.x = 2;
			square.y = 6;
			square.w = 9;
			square.h = 1;

			for (; square.w > 1; square.w -= 2)
			{
				drawRect(ref square, (byte)color);
				square.x++;
				square.y++;
			}
			drawRect(ref square, (byte)color);
			break;
		case ArrowShape.ARROW_SMALL_UP:
			// Draw arrow triangle 1
			square.x = 1;
			square.y = 5;
			square.w = 9;
			square.h = 1;

			for (; square.w > 1; square.w -= 2)
			{
				drawRect(ref square, (byte)(color + 2));
				square.x++;
				square.y--;
			}
			drawRect(ref square, (byte)(color + 2));

			// Draw arrow triangle 2
			square.x = 2;
			square.y = 5;
			square.w = 7;
			square.h = 1;

			for (; square.w > 1; square.w -= 2)
			{
				drawRect(ref square, (byte)color);
				square.x++;
				square.y--;
			}
			drawRect(ref square, (byte)color);
			break;
		case ArrowShape.ARROW_SMALL_DOWN:
			// Draw arrow triangle 1
			square.x = 1;
			square.y = 2;
			square.w = 9;
			square.h = 1;

			for (; square.w > 1; square.w -= 2)
			{
				drawRect(ref square, (byte)(color + 2));
				square.x++;
				square.y++;
			}
			drawRect(ref square, (byte)(color + 2));

			// Draw arrow triangle 2
			square.x = 2;
			square.y = 2;
			square.w = 7;
			square.h = 1;

			for (; square.w > 1; square.w -= 2)
			{
				drawRect(ref square, (byte)color);
				square.x++;
				square.y++;
			}
			drawRect(ref square, (byte)color);
			break;
		case ArrowShape.ARROW_SMALL_LEFT:
			// Draw arrow triangle 1
			square.x = 2;
			square.y = 4;
			square.w = 2;
			square.h = 1;

			for (; square.h < 5; square.h += 2)
			{
				drawRect(ref square, (byte)(color + 2));
				square.x += 2;
				square.y--;
			}
			square.w = 1;
			drawRect(ref square, (byte)(color + 2));

			// Draw arrow triangle 2
			square.x = 3;
			square.y = 4;
			square.w = 2;
			square.h = 1;

			for (; square.h < 5; square.h += 2)
			{
				drawRect(ref square, (byte)color);
				square.x += 2;
				square.y--;
			}
			square.w = 1;
			drawRect(ref square, (byte)color);
			break;
		case ArrowShape.ARROW_SMALL_RIGHT:
			// Draw arrow triangle 1
			square.x = 7;
			square.y = 4;
			square.w = 2;
			square.h = 1;

			for (; square.h < 5; square.h += 2)
			{
				drawRect(ref square, (byte)(color + 2));
				square.x -= 2;
				square.y--;
			}
			square.x++;
			square.w = 1;
			drawRect(ref square, (byte)(color + 2));

			// Draw arrow triangle 2
			square.x = 6;
			square.y = 4;
			square.w = 2;
			square.h = 1;

			for (; square.h < 5; square.h += 2)
			{
				drawRect(ref square, (byte)color);
				square.x -= 2;
				square.y--;
			}
			square.x++;
			square.w = 1;
			drawRect(ref square, (byte)color);
			break;
		default:
			break;
		}

		unlock();
	}
}
