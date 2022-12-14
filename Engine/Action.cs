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

namespace SharpXcom.Engine;

/**
 * Container for all the information associated with a
 * given user action, like mouse clicks, key presses, etc.
 * @note Called action because event is reserved.
 */
internal class Action
{
    SDL_Event _ev;
    double _scaleX, _scaleY;
    int _topBlackBand, _leftBlackBand, _mouseX, _mouseY, _surfaceX, _surfaceY;
    InteractiveSurface _sender;

    /**
     * Creates a new action.
     * @param scaleX Screen's X scaling factor.
     * @param scaleY Screen's Y scaling factor.
     * @param topBlackBand Screen's top black band height.
     * @param leftBlackBand Screen's left black band width.
     * @param ev Pointer to SDL_event.
     */
    internal Action(SDL_Event ev, double scaleX, double scaleY, int topBlackBand, int leftBlackBand)
    {
        _ev = ev;
        _scaleX = scaleX;
        _scaleY = scaleY;
        _topBlackBand = topBlackBand;
        _leftBlackBand = leftBlackBand;
        _mouseX = -1;
        _mouseY = -1;
        _surfaceX = -1;
        _surfaceY = -1;
        _sender = null;
    }

    ~Action() { }

    /**
     * Returns the details about this action.
     * @return Pointer to SDL_event.
     */
    internal ref SDL_Event getDetails() =>
	    ref _ev;

    /**
     * Returns the absolute X position of the
     * mouse cursor relative to the game window,
     * corrected for screen scaling.
     * @return Mouse's absolute X position.
     */
    internal double getAbsoluteXMouse()
    {
	    if (_mouseX == -1)
		    return -1;
	    return _mouseX / _scaleX;
    }

    /**
     * Returns the absolute Y position of the
     * mouse cursor relative to the game window,
     * corrected for screen scaling.
     * @return Mouse's absolute X position.
     */
    internal double getAbsoluteYMouse()
    {
	    if (_mouseY == -1)
		    return -1;
	    return _mouseY / _scaleY;
    }

    /**
     * Sets this action as a mouse action with
     * the respective mouse properties.
     * @param mouseX Mouse's X position.
     * @param mouseY Mouse's Y position.
     * @param surfaceX Surface's X position.
     * @param surfaceY Surface's Y position.
     */
    internal void setMouseAction(int mouseX, int mouseY, int surfaceX, int surfaceY)
    {
        _mouseX = mouseX - _leftBlackBand;
        _mouseY = mouseY - _topBlackBand;
        _surfaceX = surfaceX;
        _surfaceY = surfaceY;
    }

    internal bool isMouseAction() =>
	    (_mouseX != -1);

    /**
     * Changes the interactive surface that triggered
     * this action (the sender).
     * @param sender Pointer to interactive surface.
     */
    internal void setSender(InteractiveSurface sender) =>
        _sender = sender;

    /**
     * Returns the height in pixel of the
     * top black band if any.
     * @return Screen's top black band.
     */
    internal int getTopBlackBand() =>
	    _topBlackBand;

    /**
     * Returns the width in pixel of the
     * left black band if any.
     * @return Screen's left black band.
     */
    internal int getLeftBlackBand() =>
	    _leftBlackBand;

    /**
     * Returns the X scaling factor used by the screen
     * when this action was fired (used to correct mouse input).
     * @return Screen's X scaling factor.
     */
    internal double getXScale() =>
	    _scaleX;

    /**
     * Returns the Y scaling factor used by the screen
     * when this action was fired (used to correct mouse input).
     * @return Screen's Y scaling factor.
     */
    internal double getYScale() =>
	    _scaleY;
}
