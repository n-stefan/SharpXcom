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
 * Coloured button with a text label.
 * Drawn to look like a 3D-shaped box with text on top,
 * responds to mouse clicks. Can be attached to a group of
 * buttons to turn it into a radio button (only one button
 * pushed at a time).
 */
internal class TextButton : InteractiveSurface
{
    byte _color;
    TextButton _group;
    bool _contrast, _geoscapeButton;
    ComboBox _comboBox;
    Text _text;
    internal static Sound soundPress;

    /**
     * Sets up a text button with the specified size and position.
     * The text is centered on the button.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal TextButton(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _color = 0;
        _group = null;
        _contrast = false;
        _geoscapeButton = false;
        _comboBox = null;

        _text = new Text(width, height, 0, 0);
        _text.setSmall();
        _text.setAlign(TextHAlign.ALIGN_CENTER);
        _text.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
        _text.setWordWrap(true);
    }

    /**
     * Deletes the contained Text.
     */
    ~TextButton() =>
        _text = null;

    /**
     * Hooks up the button to work as part of an existing combobox,
     * toggling its state when it's pressed.
     * @param comboBox Pointer to ComboBox.
     */
    internal void setComboBox(ComboBox comboBox)
    {
        _comboBox = comboBox;
        if (_comboBox != null)
        {
            _text.setX(-6);
        }
        else
        {
            _text.setX(0);
        }
    }

    /**
     * Changes the color for the button and text.
     * @param color Color value.
     */
    internal void setColor(byte color)
    {
        _color = color;
        _text.setColor(color);
        _redraw = true;
    }

    /**
     * Changes the text of the button label.
     * @param text Text string.
     */
    internal void setText(string text)
    {
	    _text.setText(text);
	    _redraw = true;
    }

    /**
     * Returns the font currently used by the text.
     * @return Pointer to font.
     */
    internal Font getFont() =>
	    _text.getFont();

    /**
     * Enables/disables high contrast color. Mostly used for
     * Battlescape UI.
     * @param contrast High contrast setting.
     */
    internal void setHighContrast(bool contrast)
    {
        _contrast = contrast;
        _text.setHighContrast(contrast);
        _redraw = true;
    }

    /**
     * Changes the button group this button belongs to.
     * @param group Pointer to the pressed button pointer in the group.
     * Null makes it a regular button.
     */
    internal void setGroup(TextButton group)
    {
        _group = group;
        _redraw = true;
    }

    internal void setGeoscapeButton(bool geo) =>
        _geoscapeButton = geo;

    /**
     * Changes the text to use the big-size font.
     */
    internal void setBig()
    {
        _text.setBig();
        _redraw = true;
    }

    /**
     * Draws the labeled button.
     * The colors are inverted if the button is pressed.
     */
    protected override void draw()
    {
	    base.draw();
	    SDL_Rect square;

	    int mul = 1;
	    if (_contrast)
	    {
		    mul = 2;
	    }

	    int color = _color + 1 * mul;

	    square.x = 0;
	    square.y = 0;
	    square.w = getWidth();
	    square.h = getHeight();

	    for (int i = 0; i < 5; ++i)
	    {
		    drawRect(ref square, (byte)color);

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
			        color = _color + 5 * mul;
			        setPixel(square.w, 0, (byte)color);
			        break;
		        case 1:
			        color = _color + 2 * mul;
			        break;
		        case 2:
			        color = _color + 4 * mul;
			        setPixel(square.w+1, 1, (byte)color);
			        break;
		        case 3:
			        color = _color + 3 * mul;
			        break;
		        case 4:
			        if (_geoscapeButton)
			        {
				        setPixel(0, 0, _color);
				        setPixel(1, 1, _color);
			        }
			        break;
		    }
	    }

	    bool press;
	    if (_group == null)
		    press = isButtonPressed();
	    else
		    press = (_group == this);

	    if (press)
	    {
		    if (_geoscapeButton)
		    {
			    this.invert((byte)(_color + 2 * mul));
		    }
		    else
		    {
			    this.invert((byte)(_color + 3 * mul));
		    }
	    }
	    _text.setInvert(press);

	    _text.blit(this);
    }

    protected override bool isButtonHandled(byte button = 0)
    {
	    if (_comboBox != null)
	    {
		    return (button == SDL_BUTTON_LEFT);
	    }
	    else
	    {
		    return base.isButtonHandled(button);
	    }
    }

    /**
     * Sets the button as the pressed button if it's part of a group.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void mousePress(Action action, State state)
    {
	    if (action.getDetails().button.button == SDL_BUTTON_LEFT && _group != null)
	    {
		    TextButton old = _group;
		    _group = this;
            if (old != null)
			    old.draw();
		    draw();
	    }

	    if (isButtonHandled(action.getDetails().button.button))
	    {
		    if (soundPress != null && _group == null &&
			    action.getDetails().wheel.y == 0) //button.button != SDL_BUTTON_WHEELUP && button.button != SDL_BUTTON_WHEELDOWN
		    {
			    soundPress.play(Mix_GroupAvailable(0));
		    }

		    if (_comboBox != null)
		    {
			    _comboBox.toggle();
		    }

		    draw();
		    //_redraw = true;
	    }
	    base.mousePress(action, state);
    }
}
