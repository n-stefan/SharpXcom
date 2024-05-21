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
 * Enumeration for the type of animation when a window pops up.
 */
enum WindowPopup { POPUP_NONE, POPUP_HORIZONTAL, POPUP_VERTICAL, POPUP_BOTH };

/**
 * Box with a coloured border and custom background.
 * Pretty much used as the background in most of the interface. In fact
 * it's also used in screens, so it's not really much of a window, just a... box.
 * But box sounds lame.
 */
internal class Window : Surface
{
    const double POPUP_SPEED = 0.05;

    int _dx, _dy;
    Surface _bg;
    byte _color;
    WindowPopup _popup;
    double _popupStep;
    State _state;
    bool _contrast, _screen, _thinBorder;
    Timer _timer;
    internal static Sound[] soundPopup = new Sound[3];

    /**
     * Sets up a blank window with the specified size and position.
     * @param state Pointer to state the window belongs to.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     * @param popup Popup animation.
     */
    internal Window(State state, int width, int height, int x = 0, int y = 0, WindowPopup popup = WindowPopup.POPUP_NONE) : base(width, height, x, y)
    {
        _dx = -x;
        _dy = -y;
        _bg = null;
        _color = 0;
        _popup = popup;
        _popupStep = 0.0;
        _state = state;
        _contrast = false;
        _screen = false;
        _thinBorder = false;

        _timer = new Timer(10);
        _timer.onTimer((SurfaceHandler)this.popup);

        if (_popup == WindowPopup.POPUP_NONE)
        {
            _popupStep = 1.0;
        }
        else
        {
            setHidden(true);
            _timer.start();
            if (_state != null)
            {
                _screen = state.isScreen();
                if (_screen)
                    _state.toggleScreen();
            }
        }
    }

    /**
     * Deletes timers.
     */
    ~Window() =>
        _timer = null;

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
     * Plays the window popup animation.
     */
    void popup()
    {
        if (AreSame(_popupStep, 0.0))
        {
            int sound = RNG.seedless(0, 2);
            if (soundPopup[sound] != null)
            {
                soundPopup[sound].play(Mix_GroupAvailable(0));
            }
        }
        if (_popupStep < 1.0)
        {
            _popupStep += POPUP_SPEED;
        }
        else
        {
            if (_screen)
            {
                _state.toggleScreen();
            }
            _state.showAll();
            _popupStep = 1.0;
            _timer.stop();
        }
        _redraw = true;
    }

    /**
     * Changes the window to have a thin border.
     */
    internal void setThinBorder() =>
        _thinBorder = true;

    /**
     * Changes the surface used to draw the background of the window.
     * @param bg New background.
     */
    internal void setBackground(Surface bg)
    {
        _bg = bg;
        _redraw = true;
    }

    /**
     * Changes the vertical offset of the surface in the Y axis.
     * @param dy Y position in pixels.
     */
    internal void setDY(int dy) =>
        _dy = dy;

    /**
     * Keeps the animation timers running.
     */
    internal override void think()
    {
	    if (_hidden && _popupStep < 1.0)
	    {
		    _state.hideAll();
		    setHidden(false);
	    }

	    _timer.think(null, this);
    }

    /**
     * Draws the bordered window with a graphic background.
     * The background never moves with the window, it's
     * always aligned to the top-left corner of the screen
     * and cropped to fit the inside area.
     */
    internal override void draw()
    {
	    base.draw();
	    SDL_Rect square;

	    if (_popup == WindowPopup.POPUP_HORIZONTAL || _popup == WindowPopup.POPUP_BOTH)
	    {
		    square.x = (int)((getWidth() - getWidth() * _popupStep) / 2);
		    square.w = (int)(getWidth() * _popupStep);
	    }
	    else
	    {
		    square.x = 0;
		    square.w = getWidth();
	    }
	    if (_popup == WindowPopup.POPUP_VERTICAL || _popup == WindowPopup.POPUP_BOTH)
	    {
		    square.y = (int)((getHeight() - getHeight() * _popupStep) / 2);
		    square.h = (int)(getHeight() * _popupStep);
	    }
	    else
	    {
		    square.y = 0;
		    square.h = getHeight();
	    }

	    int mul = 1;
	    if (_contrast)
	    {
		    mul = 2;
	    }
	    byte color = (byte)(_color + 3 * mul);

	    if (_thinBorder)
	    {
		    color = (byte)(_color + 1 * mul);
		    for (int i = 0; i < 5; ++i)
		    {
			    drawRect(ref square, color);

			    if (i % 2 == 0)
			    {
				    square.x++;
				    square.y++;
			    }
			    square.w--;
			    square.h--;

			    switch (i)
			    {
			        case 0:
				        color = (byte)(_color + 5 * mul);
				        setPixel(square.w, 0, color);
				        break;
			        case 1:
				        color = (byte)(_color + 2 * mul);
				        break;
			        case 2:
				        color = (byte)(_color + 4 * mul);
				        setPixel(square.w+1, 1, color);
				        break;
			        case 3:
				        color = (byte)(_color + 3 * mul);
				        break;
			    }
		    }
	    }
	    else
	    {
		    for (int i = 0; i < 5; ++i)
		    {
			    drawRect(ref square, color);
			    if (i < 2)
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
	    }

	    if (_bg != null)
	    {
		    _bg.getCrop().x = square.x - _dx;
		    _bg.getCrop().y = square.y - _dy;
		    _bg.getCrop().w = square.w ;
		    _bg.getCrop().h = square.h ;
		    _bg.setX(square.x);
		    _bg.setY(square.y);
		    _bg.blit(this);
	    }
    }
}
