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

    /**
     * Automatically updates the slider
     * when the mouse moves.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void handle(Action action, State state)
    {
	    base.handle(action, state);
	    //_button.handle(action, state);
	    if (_pressed && (action.getDetails().type == SDL_EventType.SDL_MOUSEMOTION || action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN))
	    {
		    int cursorX = (int)action.getAbsoluteXMouse();
		    double buttonX = Math.Clamp(cursorX + _offsetX, _minX, _maxX);
		    double pos = (buttonX - _minX) / (_maxX - _minX);
		    int value = _min + (int)Math.Round((_max - _min) * pos);
		    setValue(value);
		    if (_change != null)
		    {
			    _change(action);
		    }
	    }
    }

    /**
     * Blits the slider contents
     * @param surface Pointer to surface to blit onto.
     */
    internal override void blit(Surface surface)
    {
	    base.blit(surface);
	    if (_visible && !_hidden)
	    {
		    _txtMinus.blit(surface);
		    _txtPlus.blit(surface);
		    _frame.blit(surface);
		    _button.blit(surface);
	    }
    }

    /**
     * The slider only moves while the button is pressed.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void mousePress(Action action, State state)
    {
	    base.mousePress(action, state);
	    if (action.getDetails().button.button == SDL_BUTTON_LEFT)
	    {
		    _pressed = true;
		    int cursorX = (int)action.getAbsoluteXMouse();
		    if (cursorX >= _button.getX() && cursorX < _button.getX() + _button.getWidth())
		    {
			    _offsetX = _button.getX() - cursorX;
		    }
		    else
		    {
			    _offsetX = -_button.getWidth() / 2;
		    }
	    }
    }

    /**
     * The slider stops moving when the button is released.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void mouseRelease(Action action, State state)
    {
	    base.mouseRelease(action, state);
	    if (action.getDetails().button.button == SDL_BUTTON_LEFT)
	    {
		    _pressed = false;
		    _offsetX = 0;
	    }
    }

    /**
     * Changes the various resources needed for text rendering.
     * The different fonts need to be passed in advance since the
     * text size can change mid-text, and the language affects
     * how the text is rendered.
     * @param big Pointer to large-size font.
     * @param small Pointer to small-size font.
     * @param lang Pointer to current language.
     */
    internal override void initText(Font big, Font small, Language lang)
    {
	    _txtMinus.initText(big, small, lang);
	    _txtPlus.initText(big, small, lang);
	    _button.initText(big, small, lang);
    }

    /**
     * Replaces a certain amount of colors in the slider's palette.
     * @param colors Pointer to the set of colors.
     * @param firstcolor Offset of the first color to replace.
     * @param ncolors Amount of colors to replace.
     */
    internal override void setPalette(SDL_Color[] colors, int firstcolor, int ncolors)
    {
	    base.setPalette(colors, firstcolor, ncolors);
	    _txtMinus.setPalette(colors, firstcolor, ncolors);
	    _txtPlus.setPalette(colors, firstcolor, ncolors);
	    _frame.setPalette(colors, firstcolor, ncolors);
	    _button.setPalette(colors, firstcolor, ncolors);
    }

    /**
     * Changes the position of the surface in the X axis.
     * @param x X position in pixels.
     */
    internal override void setX(int x)
    {
	    base.setX(x);
	    _txtMinus.setX(x - 1);
	    _txtPlus.setX(x + getWidth() - _textness);
	    _frame.setX(getX() + _textness);

	    _minX = _frame.getX();
	    _maxX = _frame.getX() + _frame.getWidth() - _button.getWidth();
	    setValue((int)_pos);
    }

    /**
     * Changes the position of the surface in the Y axis.
     * @param y Y position in pixels.
     */
    internal override void setY(int y)
    {
	    base.setY(y);
	    _txtMinus.setY(y);
	    _txtPlus.setY(y);
	    _frame.setY(getY() + (getHeight() - _thickness) / 2);
	    _button.setY(getY());
    }
}
