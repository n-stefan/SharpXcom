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
 * Regular image that works like a button.
 * Unlike the TextButton, this button doesn't draw
 * anything on its own. It takes an existing graphic and
 * treats it as a button, inverting colors when necessary.
 * This is necessary for special buttons like in the Geoscape.
 */
internal class ImageButton : InteractiveSurface
{
    protected byte _color;
    ImageButton _group;
    bool _inverted;

    /**
     * Sets up an image button with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal ImageButton(int width, int height, int x = 0, int y = 0) : base(width, height, x, y)
    {
        _color = 0;
        _group = null;
        _inverted = false;
    }

    /**
     *
     */
    ~ImageButton() { }

    /**
     * Returns the color for the image button.
     * @return Color value.
     */
    internal byte getColor() =>
	    _color;

    /**
     * Changes the color for the image button.
     * @param color Color value.
     */
    internal override void setColor(byte color) =>
        _color = color;

    /**
     * Changes the button group this image button belongs to.
     * @param group Pointer to the pressed button pointer in the group.
     * Null makes it a regular button.
     */
    internal void setGroup(ImageButton group)
    {
        _group = group;
        if (_group != null && _group == this)
            invert((byte)(_color + 3));
    }

    /**
     * Sets the button as the pressed button if it's part of a group,
     * and inverts the colors when pressed.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    internal override void mousePress(Action action, State state)
    {
	    if (_group != null)
	    {
		    if (action.getDetails().button.button == SDL_BUTTON_LEFT)
		    {
			    _group.invert((byte)(_group.getColor() + 3));
			    _group = this;
			    invert((byte)(_color + 3));
		    }
	    }
	    else if (!_inverted && isButtonPressed() && isButtonHandled(action.getDetails().button.button))
	    {
		    _inverted = true;
		    invert((byte)(_color + 3));
	    }
	    base.mousePress(action, state);
    }

    /*
     * Sets the button as the released button if it's part of a group.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void mouseRelease(Action action, State state)
    {
	    if (_inverted && isButtonHandled(action.getDetails().button.button))
	    {
		    _inverted = false;
		    invert((byte)(_color + 3));
	    }
	    base.mouseRelease(action, state);
    }

    /**
     * Invert a button explicitly either ON or OFF and keep track of the state using our internal variables.
     * @param press Set this button as pressed.
     */
    void toggle(bool press)
    {
	    if (_inverted != press)
	    {
		    _inverted = !_inverted;
		    invert((byte)(_color + 3));
	    }
    }
}
