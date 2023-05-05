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
    internal void setGroup(ref TextButton group)
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
}
