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

delegate void ActionHandler(Action action);

/**
 * Surface that the user can interact with.
 * Specialized version of the standard Surface that
 * processes all the various SDL events and turns
 * them into useful interactions with the Surface,
 * so specialized subclasses don't need to worry about it.
 */
internal class InteractiveSurface : Surface
{
    const int NUM_BUTTONS = 7;

    byte _buttonsPressed;
    protected ActionHandler _in, _over, _out;
    protected bool _isHovered, _isFocused, _listButton;
    protected Dictionary<byte, ActionHandler> _click, _press, _release;
    protected Dictionary<SDL_Keycode, ActionHandler> _keyPress, _keyRelease;

    /**
     * Sets up a blank interactive surface with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal InteractiveSurface(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _buttonsPressed = 0;
        _in = null;
        _over = null;
        _out = null;
        _isHovered = false;
        _isFocused = true;
        _listButton = false;
    }

    /**
     *
     */
    ~InteractiveSurface() { }

    /**
     * Called whenever an action occurs, and processes it to
     * check if it's relevant to the surface and convert it
     * into a meaningful interaction like a "click", calling
     * the respective handlers.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    internal void handle(Action action, State state)
    {
        if (!_visible || _hidden)
            return;

        action.setSender(this);

        if (action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONUP || action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
        {
            action.setMouseAction(action.getDetails().button.x, action.getDetails().button.y, getX(), getY());
        }
        else if (action.getDetails().type == SDL_EventType.SDL_MOUSEMOTION)
        {
            action.setMouseAction(action.getDetails().motion.x, action.getDetails().motion.y, getX(), getY());
        }

        if (action.isMouseAction())
        {
            if ((action.getAbsoluteXMouse() >= getX() && action.getAbsoluteXMouse() < getX() + getWidth()) &&
                (action.getAbsoluteYMouse() >= getY() && action.getAbsoluteYMouse() < getY() + getHeight()))
            {
                if (!_isHovered)
                {
                    _isHovered = true;
                    mouseIn(action, state);
                }
                if (_listButton && action.getDetails().type == SDL_EventType.SDL_MOUSEMOTION)
                {
                    _buttonsPressed = (byte)SDL_GetMouseState(0, 0);
                    for (byte i = 1; i <= NUM_BUTTONS; ++i)
                    {
                        if (isButtonPressed(i))
                        {
                            action.getDetails().button.button = i;
                            mousePress(action, state);
                        }
                    }
                }
                mouseOver(action, state);
            }
            else
            {
                if (_isHovered)
                {
                    _isHovered = false;
                    mouseOut(action, state);
                    if (_listButton && action.getDetails().type == SDL_EventType.SDL_MOUSEMOTION)
                    {
                        for (byte i = 1; i <= NUM_BUTTONS; ++i)
                        {
                            if (isButtonPressed(i))
                            {
                                setButtonPressed(i, false);
                            }
                            action.getDetails().button.button = i;
                            mouseRelease(action, state);
                        }
                    }
                }
            }
        }

        if (action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
        {
            if (_isHovered && !isButtonPressed(action.getDetails().button.button))
            {
                setButtonPressed(action.getDetails().button.button, true);
                mousePress(action, state);
            }
        }
        else if (action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONUP)
        {
            if (isButtonPressed(action.getDetails().button.button))
            {
                setButtonPressed(action.getDetails().button.button, false);
                mouseRelease(action, state);
                if (_isHovered)
                {
                    mouseClick(action, state);
                }
            }
        }

        if (_isFocused)
        {
            if (action.getDetails().type == SDL_EventType.SDL_KEYDOWN)
            {
                keyboardPress(action, state);
            }
            else if (action.getDetails().type == SDL_EventType.SDL_KEYUP)
            {
                keyboardRelease(action, state);
            }
        }
    }

    bool isButtonPressed(byte button = 0)
    {
	    if (button == 0)
	    {
            return (_buttonsPressed != 0);
	    }
	    else
	    {
		    return (_buttonsPressed & SDL_BUTTON(button)) != 0;
	    }
    }

    void setButtonPressed(byte button, bool pressed)
    {
        if (pressed)
        {
            _buttonsPressed |= (byte)SDL_BUTTON(button);
        }
        else
        {
            _buttonsPressed &= (byte)(~SDL_BUTTON(button));
        }
    }

    /**
     * Called every time there's a mouse press over the surface.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    void mousePress(Action action, State state)
    {
        if (_press.TryGetValue(0, out ActionHandler handler))
        {
            handler(action);
        }
        if (_press.TryGetValue(action.getDetails().button.button, out ActionHandler handler2))
        {
            handler2(action);
        }
    }

    /**
     * Called every time there's a mouse release over the surface.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    void mouseRelease(Action action, State state)
    {
        if (_release.TryGetValue(0, out ActionHandler handler))
        {
            handler(action);
        }
        if (_release.TryGetValue(action.getDetails().button.button, out ActionHandler handler2))
        {
            handler2(action);
        }
    }

    /**
     * Called every time there's a mouse click on the surface.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    void mouseClick(Action action, State state)
    {
        if (_click.TryGetValue(0, out ActionHandler handler))
        {
            handler(action);
        }
        if (_click.TryGetValue(action.getDetails().button.button, out ActionHandler handler2))
        {
            handler2(action);
        }
    }

    /**
     * Called every time there's a keyboard press when the surface is focused.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    void keyboardPress(Action action, State state)
    {
        //TODO: SDLK_ANY?
        // Check if Ctrl, Alt and Shift aren't pressed
        bool mod = ((action.getDetails().key.keysym.mod & (SDL_Keymod.KMOD_CTRL | SDL_Keymod.KMOD_ALT | SDL_Keymod.KMOD_SHIFT)) != 0);
        if (_keyPress.TryGetValue(action.getDetails().key.keysym.sym, out ActionHandler handler) && !mod)
        {
            handler(action);
        }
    }

    /**
     * Called every time there's a keyboard release over the surface.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    void keyboardRelease(Action action, State state)
    {
        //TODO: SDLK_ANY?
        // Check if Ctrl, Alt and Shift aren't pressed
        bool mod = ((action.getDetails().key.keysym.mod & (SDL_Keymod.KMOD_CTRL | SDL_Keymod.KMOD_ALT | SDL_Keymod.KMOD_SHIFT)) != 0);
        if (_keyRelease.TryGetValue(action.getDetails().key.keysym.sym, out ActionHandler handler) && !mod)
        {
            handler(action);
        }
    }

    /**
     * Called every time the mouse moves into the surface.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    void mouseIn(Action action, State state) =>
        _in?.Invoke(action);

    /**
     * Called every time the mouse moves over the surface.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    void mouseOver(Action action, State state) =>
        _over?.Invoke(action);

    /**
     * Called every time the mouse moves out of the surface.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    void mouseOut(Action action, State state) =>
        _out?.Invoke(action);

    /**
     * Simulates a "mouse button release". Used in circumstances
     * where the surface is unpressed without user input.
     * @param state Pointer to running state.
     */
    internal void unpress(State state)
    {
        if (isButtonPressed())
        {
            _buttonsPressed = 0;
            var ev = new SDL_Event();
            ev.type = SDL_EventType.SDL_MOUSEBUTTONUP;
            ev.button.button = (byte)SDL_BUTTON_LEFT;
            Action a = new Action(ev, 0.0, 0.0, 0, 0);
            mouseRelease(a, state);
        }
    }

    /**
     * Changes the visibility of the surface. A hidden surface
     * isn't blitted nor receives events.
     * @param visible New visibility.
     */
    internal void setVisible(bool visible)
    {
        base.setVisible(visible);
        // Unpress button if it was hidden
        if (!_visible)
        {
            unpress(null);
        }
    }

    /**
     * Sets a flag for this button to say "i'm a member of a textList" to true.
     */
    internal void setListButton() =>
        _listButton = true;

    /**
     * Sets a function to be called every time the surface is mouse clicked.
     * @param handler Action handler.
     * @param button Mouse button to check for. Set to 0 for any button.
     */
    internal void onMouseClick(ActionHandler handler, byte button = (byte)SDL_BUTTON_LEFT)
    {
        if (handler != null)
        {
            _click[button] = handler;
        }
        else
        {
            _click.Remove(button);
        }
    }

    /**
     * Sets a function to be called every time the surface is mouse pressed.
     * @param handler Action handler.
     * @param button Mouse button to check for. Set to 0 for any button.
     */
    internal void onMousePress(ActionHandler handler, byte button = 0)
    {
        if (handler != null)
        {
            _press[button] = handler;
        }
        else
        {
            _press.Remove(button);
        }
    }

    /**
     * Sets a function to be called every time the surface is mouse released.
     * @param handler Action handler.
     * @param button Mouse button to check for. Set to 0 for any button.
     */
    internal void onMouseRelease(ActionHandler handler, byte button = 0)
    {
	    if (handler != null)
	    {
		    _release[button] = handler;
	    }
	    else
	    {
		    _release.Remove(button);
	    }
    }
}
