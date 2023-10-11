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
    internal void setColor(byte color)
    {
        _color = color;
        _redraw = true;
    }

    /**
     * Enables/disables high contrast color. Mostly used for
     * Battlescape UI.
     * @param contrast High contrast setting.
     */
    internal void setHighContrast(bool contrast)
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
    protected override void think()
    {
	    if (_hidden && _popupStep < 1.0)
	    {
		    _state.hideAll();
		    setHidden(false);
	    }

	    _timer.think(null, this);
    }
}
