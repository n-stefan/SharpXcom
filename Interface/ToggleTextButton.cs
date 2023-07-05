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

internal class ToggleTextButton : TextButton
{
    int _originalColor, _invertedColor;
    TextButton _fakeGroup;
    bool _isPressed;

    internal ToggleTextButton(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _originalColor = -1;
        _invertedColor = -1;
        _fakeGroup = null;

        _isPressed = false;
        base.setGroup(ref _fakeGroup);
    }

    ~ToggleTextButton() { }

    /// set the _isPressed state of the button and force it to redraw
    internal void setPressed(bool pressed)
    {
        _isPressed = pressed;
        _fakeGroup = _isPressed ? this : null;
        if (_isPressed && _invertedColor > -1) base.setColor((byte)_invertedColor);
        else base.setColor((byte)_originalColor);
        _redraw = true;
    }

    internal bool getPressed() =>
        _isPressed;

    /// When this is set, Surface::invert() is called with the value from mid when it's time to invert the button
    internal void setInvertColor(byte color)
    {
        _invertedColor = color;
        _fakeGroup = null;
        _redraw = true;
    }
}
