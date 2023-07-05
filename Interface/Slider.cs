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
 * Horizontal slider control to select from a range of values.
 */
internal class Slider : InteractiveSurface
{
    double _pos;
    int _min, _max, _value;
    bool _pressed;
    ActionHandler _change;
    int _thickness, _textness, _minX, _maxX, _offsetX;
    Text _txtMinus, _txtPlus;
    Frame _frame;
    TextButton _button;

    /**
     * Sets up a slider with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal Slider(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _pos = 0.0;
        _min = 0;
        _max = 100;
        _pressed = false;
        _change = null;
        _offsetX = 0;

        _thickness = 5;
        _textness = 8;
        _txtMinus = new Text(_textness, height - 2, x - 1, y);
        _txtPlus = new Text(_textness, height - 2, x + width - _textness, y);
        _frame = new Frame(width - _textness * 2, _thickness, x + _textness, y + (height - _thickness) / 2);
        _button = new TextButton(10, height, x, y);

        _frame.setThickness(_thickness);

        _txtMinus.setAlign(TextHAlign.ALIGN_CENTER);
        _txtMinus.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
        _txtMinus.setText("-");

        _txtPlus.setAlign(TextHAlign.ALIGN_CENTER);
        _txtPlus.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
        _txtPlus.setText("+");

        _minX = _frame.getX();
        _maxX = _frame.getX() + _frame.getWidth() - _button.getWidth();

        setValue((int)_pos);
    }

    /**
     * Deletes contents.
     */
    ~Slider()
    {
        _txtMinus = null;
        _txtPlus = null;
        _frame = null;
        _button = null;
    }

    /**
     * Changes the current value of the slider and
     * positions it appropriately.
     * @param value New value.
     */
    internal void setValue(int value)
    {
        if (_min < _max)
        {
            _value = Math.Clamp(value, _min, _max);
        }
        else
        {
            _value = Math.Clamp(value, _max, _min);
        }
        double pos = (double)(_value - _min) / (double)(_max - _min);
        setPosition(pos);
    }

    /**
     * Moves the slider to the new position.
     * @param value New value.
     */
    void setPosition(double pos)
    {
        _pos = pos;
        _button.setX((int)Math.Floor(_minX + (_maxX - _minX) * _pos));
    }

    /**
     * Changes the color used to render the slider.
     * @param color Color value.
     */
    internal void setColor(byte color)
    {
        _txtMinus.setColor(color);
        _txtPlus.setColor(color);
        _frame.setColor(color);
        _button.setColor(color);
    }

    /**
     * Changes the range of values the slider
     * can contain.
     * @param min Minimum value.
     * @param max Maximum value.
     */
    internal void setRange(int min, int max)
    {
        _min = min;
        _max = max;
        setValue(_value);
    }

    /**
     * Sets a function to be called every time the slider's value changes.
     * @param handler Action handler.
     */
    internal void onChange(ActionHandler handler) =>
        _change = handler;

    /**
     * Returns the current value of the slider.
     * @return Value.
     */
    internal int getValue() =>
	    _value;
}
