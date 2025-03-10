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

enum InversionType { INVERT_NONE, INVERT_CLICK, INVERT_TOGGLE };

/**
 * Regular image that works like a button.
 * Unlike the TextButton, this button doesn't draw
 * anything on its own. It takes an existing graphic and
 * treats it as a button, inverting colors when necessary.
 * This is necessary for special buttons like in the Geoscape.
 */
internal class BattlescapeButton : InteractiveSurface
{
    protected byte _color;
    protected BattlescapeButton _group;
    protected bool _inverted;
    protected InversionType _toggleMode;
    protected Surface _altSurface;

    /**
     * Sets up a battlescape button with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal BattlescapeButton(int width, int height, int x = 0, int y = 0) : base(width, height, x, y)
    {
        _color = 0;
        _group = null;
        _inverted = false;
        _toggleMode = InversionType.INVERT_NONE;
        _altSurface = null;
    }

    /**
     *
     */
    ~BattlescapeButton() =>
        _altSurface = null;

    /**
     * Initializes the alternate surface for swapping out as needed.
     * performs a colour swap for TFTD style buttons, and a palette inversion for coloured buttons
     * we use two separate surfaces because it's far easier to keep track of
     * whether or not this surface is inverted.
     */
    internal void initSurfaces()
    {
        _altSurface = new Surface(_surface.w, _surface.h, _x, _y);
        _altSurface.setPalette(getPalette());

        // Lock the surface
        _altSurface.@lock();

        // tftd mode: use a colour lookup table instead of simple palette inversion for our "pressed" state
        if (_tftdMode)
        {
            // this is our colour lookup table
            int[] colorFrom = { 1, 2, 3, 4, 7, 8, 31, 47, 153, 156, 159 };
            int[] colorTo = { 2, 3, 4, 5, 11, 10, 2, 2, 96, 9, 97 };

            for (int x = 0, y = 0; x < getWidth() && y < getHeight();)
            {
                byte pixel = getPixel(x, y);
                for (int i = 0; i != colorFrom.Length / 4 /* sizeof(colorFrom[0]) */; ++i)
                {
                    if (pixel == colorFrom[i])
                    {
                        pixel = (byte)colorTo[i];
                        break;
                    }
                }
                _altSurface.setPixelIterative(ref x, ref y, pixel);
            }
        }
        else
        {
            for (int x = 0, y = 0; x < getWidth() && y < getHeight();)
            {
                byte pixel = getPixel(x, y);
                if (pixel > 0)
                {
                    _altSurface.setPixelIterative(ref x, ref y, (byte)(pixel + 2 * ((int)_color + 3 - (int)pixel)));
                }
                else
                {
                    _altSurface.setPixelIterative(ref x, ref y, 0);
                }
            }
        }

        // Unlock the surface
        _altSurface.unlock();
    }

    /**
     * checks TFTD mode.
     * @return TFTD mode.
     */
    internal bool isTFTDMode() =>
	    _tftdMode;

    /**
     * Invert a button explicitly either ON or OFF and keep track of the state using our internal variables.
     * @param press Set this button as pressed.
     */
    internal void toggle(bool press)
    {
        if (_tftdMode || _toggleMode == InversionType.INVERT_TOGGLE || _inverted)
        {
            _inverted = press;
        }
    }

    /**
     * Changes the button group this battlescape button belongs to.
     * @param group Pointer to the pressed button pointer in the group.
     * Null makes it a regular button.
     */
    internal void setGroup(BattlescapeButton group)
    {
	    _group = group;
	    if (_group != null && _group == this)
		    _inverted = true;
    }

    /**
     * Toggle inversion mode: click to press, click to unpress.
     */
    internal void allowToggleInversion() =>
	    _toggleMode = InversionType.INVERT_TOGGLE;

    /**
     * Click inversion mode: click to press, release to unpress.
     */
    internal void allowClickInversion() =>
	    _toggleMode = InversionType.INVERT_CLICK;

    /**
     * Blits this surface or the alternate surface onto another one,
     * depending on whether the button is "pressed" or not.
     * @param surface Pointer to surface to blit onto.
     */
    internal override void blit(Surface surface)
    {
	    if (_inverted)
	    {
		    _altSurface.blit(surface);
	    }
	    else
	    {
		    base.blit(surface);
	    }
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
			    _group.toggle(false);
			    _group = this;
			    _inverted = true;
		    }
	    }
	    else if ((_tftdMode || _toggleMode == InversionType.INVERT_CLICK) && !_inverted && isButtonPressed() && isButtonHandled(action.getDetails().button.button))
	    {
		    _inverted = true;
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
	    }
	    base.mouseRelease(action, state);
    }

    /**
     * Changes the position of the surface in the X axis.
     * @param x X position in pixels.
     */
    internal override void setX(int x)
    {
	    base.setX(x);
	    if (_altSurface != null)
	    {
		    _altSurface.setX(x);
	    }
    }

    /**
     * Changes the position of the surface in the Y axis.
     * @param y Y position in pixels.
     */
    internal override void setY(int y)
    {
	    base.setY(y);
	    if (_altSurface != null)
	    {
		    _altSurface.setY(y);
	    }
    }

    /**
     * Changes the color for the battlescape button.
     * @param color Color value.
     */
    internal override void setColor(byte color) =>
	    _color = color;

    /**
     * Returns the color for the battlescape button.
     * @return Color value.
     */
    byte getColor() =>
	    _color;
}
