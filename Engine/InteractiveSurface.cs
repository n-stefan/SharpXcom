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
    const SDL_Keycode SDLK_ANY = (SDL_Keycode)(-1); // using an unused keycode to represent an "any key"

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
    internal InteractiveSurface(int width, int height, int x = 0, int y = 0) : base(width, height, x, y)
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
    protected virtual void handle(Action action, State state)
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

    protected bool isButtonPressed(byte button = 0)
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

    protected void setButtonPressed(byte button, bool pressed)
    {
        if (pressed)
        {
            _buttonsPressed = (byte)(_buttonsPressed | SDL_BUTTON(button));
        }
        else
        {
            _buttonsPressed = (byte)(_buttonsPressed & ~SDL_BUTTON(button));
        }
    }

    /**
     * Called every time there's a mouse press over the surface.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected virtual void mousePress(Action action, State state)
    {
        if (_press.TryGetValue(0, out ActionHandler allHandler))
	    {
            allHandler(action);
	    }
        if (_press.TryGetValue(action.getDetails().button.button, out ActionHandler oneHandler))
	    {
            oneHandler(action);
	    }
    }

    /**
     * Called every time there's a mouse release over the surface.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected virtual void mouseRelease(Action action, State state)
    {
        if (_release.TryGetValue(0, out ActionHandler allHandler))
	    {
            allHandler(action);
	    }
        if (_release.TryGetValue(action.getDetails().button.button, out ActionHandler oneHandler))
	    {
            oneHandler(action);
	    }
    }

    /**
     * Called every time there's a mouse click on the surface.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected virtual void mouseClick(Action action, State state)
    {
        if (_click.TryGetValue(0, out ActionHandler allHandler))
	    {
            allHandler(action);
	    }
        if (_click.TryGetValue(action.getDetails().button.button, out ActionHandler oneHandler))
	    {
            oneHandler(action);
	    }
    }

    /**
     * Called every time there's a keyboard press when the surface is focused.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected virtual void keyboardPress(Action action, State state)
    {
	    if (_keyPress.TryGetValue(SDLK_ANY, out ActionHandler allHandler))
	    {
		    allHandler(action);
	    }
        // Check if Ctrl, Alt and Shift aren't pressed
        bool mod = ((action.getDetails().key.keysym.mod & (SDL_Keymod.KMOD_CTRL|SDL_Keymod.KMOD_ALT|SDL_Keymod.KMOD_SHIFT)) != 0);
        if (_keyPress.TryGetValue(action.getDetails().key.keysym.sym, out ActionHandler oneHandler) && !mod)
	    {
            oneHandler(action);
	    }
    }

    /**
     * Called every time there's a keyboard release over the surface.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected virtual void keyboardRelease(Action action, State state)
    {
	    if (_keyRelease.TryGetValue(SDLK_ANY, out ActionHandler allHandler))
	    {
		    allHandler(action);
	    }
        // Check if Ctrl, Alt and Shift aren't pressed
        bool mod = ((action.getDetails().key.keysym.mod & (SDL_Keymod.KMOD_CTRL|SDL_Keymod.KMOD_ALT|SDL_Keymod.KMOD_SHIFT)) != 0);
        if (_keyRelease.TryGetValue(action.getDetails().key.keysym.sym, out ActionHandler oneHandler) && !mod)
	    {
            oneHandler(action);
	    }
    }

    /**
     * Called every time the mouse moves into the surface.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected virtual void mouseIn(Action action, State state) =>
        _in?.Invoke(action);

    /**
     * Called every time the mouse moves over the surface.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected virtual void mouseOver(Action action, State state) =>
        _over?.Invoke(action);

    /**
     * Called every time the mouse moves out of the surface.
     * Allows the surface to have custom functionality for this action,
     * and can be called externally to simulate the action.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected virtual void mouseOut(Action action, State state) =>
        _out?.Invoke(action);

    /**
     * Simulates a "mouse button release". Used in circumstances
     * where the surface is unpressed without user input.
     * @param state Pointer to running state.
     */
    protected virtual void unpress(State state)
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
    internal override void setVisible(bool visible)
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

    /**
     * Sets a function to be called every time a key is pressed when the surface is focused.
     * @param handler Action handler.
     * @param key Keyboard button to check for (note: ignores key modifiers). Set to SDLK_ANY for any key.
     */
    internal void onKeyboardPress(ActionHandler handler, SDL_Keycode key = SDLK_ANY)
    {
        if (key == SDL_Keycode.SDLK_UNKNOWN)
        {
            // Ignore unknown keys
            return;
        }
        if (handler != null)
        {
            _keyPress[key] = handler;
        }
        else
        {
            _keyPress.Remove(key);
        }
    }

    /**
     * Sets a function to be called every time the mouse moves over the surface.
     * @param handler Action handler.
     */
    internal void onMouseOver(ActionHandler handler) =>
        _over = handler;

    /**
     * Sets a function to be called every time the mouse moves out of the surface.
     * @param handler Action handler.
     */
    internal void onMouseOut(ActionHandler handler) =>
        _out = handler;

    /**
     * Sets a function to be called every time the mouse moves into the surface.
     * @param handler Action handler.
     */
    internal void onMouseIn(ActionHandler handler) =>
        _in = handler;

    /**
     * Sets a function to be called every time a key is released when the surface is focused.
     * @param handler Action handler.
     * @param key Keyboard button to check for (note: ignores key modifiers). Set to SDLK_ANY for any key.
     */
    internal void onKeyboardRelease(ActionHandler handler, SDL_Keycode key)
    {
        if (key == SDL_Keycode.SDLK_UNKNOWN)
        {
            // Ignore unknown keys
            return;
        }
        if (handler != null)
        {
            _keyRelease[key] = handler;
        }
        else
        {
            _keyRelease.Remove(key);
        }
    }

    /**
     * Changes the surface's focus. Surfaces will only receive
     * keyboard events if focused.
     * @param focus Is it focused?
     */
    protected virtual void setFocus(bool focus) =>
        _isFocused = focus;

    /**
     * Returns the surface's focus. Surfaces will only receive
     * keyboard events if focused.
     * @return Is it focused?
     */
    internal bool isFocused() =>
	    _isFocused;

	/// Is this mouse button event handled?
    protected virtual bool isButtonHandled(byte button = 0)
    {
	    bool handled = (_click.ContainsKey(0) ||
					    _press.ContainsKey(0) ||
					    _release.ContainsKey(0));
	    if (!handled && button != 0)
	    {
		    handled = (_click.ContainsKey(button) ||
				       _press.ContainsKey(button) ||
				       _release.ContainsKey(button));
	    }
	    return handled;
    }
}
