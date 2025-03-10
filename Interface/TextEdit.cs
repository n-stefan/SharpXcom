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

enum TextEditConstraint { TEC_NONE, TEC_NUMERIC_POSITIVE, TEC_NUMERIC };

/**
 * Editable version of Text.
 * Receives keyboard input to allow the player
 * to change the text himself.
 */
internal class TextEdit : InteractiveSurface
{
    bool _blink, _modal;
    uint _char;
    uint _caretPos;
    TextEditConstraint _textEditConstraint;
    ActionHandler _change;
    State _state;
    Text _text, _caret;
    Timer _timer;
    string _value;

    /**
     * Sets up a blank text edit with the specified size and position.
     * @param state Pointer to state the text edit belongs to.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal TextEdit(State state, int width, int height, int x = 0, int y = 0) : base(width, height, x, y)
    {
        _blink = true;
        _modal = true;
        _char = 'A';
        _caretPos = 0;
        _textEditConstraint = TextEditConstraint.TEC_NONE;
        _change = null;
        _state = state;

        _isFocused = false;
        _text = new Text(width, height, 0, 0);
        _timer = new Timer(100);
        _timer.onTimer((SurfaceHandler)blink);
        _caret = new Text(16, 17, 0, 0);
        _caret.setText("|");
    }

    /**
     * Deletes contents.
     */
    ~TextEdit()
    {
        _text = null;
        _caret = null;
        _timer = null;
        // In case it was left focused
        //SDL_EnableKeyRepeat(0, SDL_DEFAULT_REPEAT_INTERVAL);
        _state.setModal(null);
    }

    /**
     * Plays the blinking animation when the
     * text edit is focused.
     */
    void blink()
    {
        _blink = !_blink;
        _redraw = true;
    }

    /**
     * Changes the way the text is aligned horizontally
     * relative to the drawing area.
     * @param align Horizontal alignment.
     */
    internal void setAlign(TextHAlign align) =>
        _text.setAlign(align);

    /**
     * Sets a function to be called every time the text changes.
     * @param handler Action handler.
     */
    internal void onChange(ActionHandler handler) =>
        _change = handler;

    /**
     * Returns the string displayed on screen.
     * @return Text string.
     */
    internal string getText() =>
	    Unicode.convUtf32ToUtf8(_value);

    /**
     * Changes the string displayed on screen.
     * @param text Text string.
     */
    internal void setText(string text)
    {
	    _value = Unicode.convUtf8ToUtf32(text);
	    _caretPos = (uint)_value.Length;
	    _redraw = true;
    }

    /**
     * Changes the text edit to use the big-size font.
     */
    internal void setBig()
    {
        _text.setBig();
        _caret.setBig();
    }

    /**
     * Restricts the text to only numerical input or signed numerical input.
     * @param constraint TextEditConstraint to be applied.
     */
    internal void setConstraint(TextEditConstraint constraint) =>
        _textEditConstraint = constraint;

    // Override the base class' method properly.
    internal override void setFocus(bool focus) =>
        setFocus(focus, true);

    /**
     * Controls the blinking animation when
     * the text edit is focused.
     * @param focus True if focused, false otherwise.
     * @param modal True to lock input to this control, false otherwise.
     */
    internal void setFocus(bool focus, bool modal)
    {
        _modal = modal;
        if (focus != _isFocused)
        {
            _redraw = true;
            base.setFocus(focus);
            if (_isFocused)
            {
                //SDL_EnableKeyRepeat(SDL_DEFAULT_REPEAT_DELAY, SDL_DEFAULT_REPEAT_INTERVAL);
                _caretPos = (uint)_value.Length;
                _blink = true;
                _timer.start();
                if (_modal)
                    _state.setModal(this);
            }
            else
            {
                _blink = false;
                _timer.stop();
                //SDL_EnableKeyRepeat(0, SDL_DEFAULT_REPEAT_INTERVAL);
                if (_modal)
                    _state.setModal(null);
            }
        }
    }

    /**
     * Changes the way the text is aligned vertically
     * relative to the drawing area.
     * @param valign Vertical alignment.
     */
    internal void setVerticalAlign(TextVAlign valign) =>
        _text.setVerticalAlign(valign);

    /**
     * Enables/disables text wordwrapping. When enabled, lines of
     * text are automatically split to ensure they stay within the
     * drawing area, otherwise they simply go off the edge.
     * @param wrap Wordwrapping setting.
     */
    internal void setWordWrap(bool wrap) =>
        _text.setWordWrap(wrap);

    /**
     * Keeps the animation timers running.
     */
    internal override void think() =>
	    _timer.think(null, this);

    /**
     * Passes events to internal components.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    internal override void handle(Action action, State state)
    {
	    base.handle(action, state);
	    if (_isFocused && _modal && action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN &&
		    (action.getAbsoluteXMouse() < getX() || action.getAbsoluteXMouse() >= getX() + getWidth() ||
		     action.getAbsoluteYMouse() < getY() || action.getAbsoluteYMouse() >= getY() + getHeight()))
	    {
		    setFocus(false);
	    }
    }

    /**
     * Adds a flashing | caret to the text
     * to show when it's focused and editable.
     */
    internal override void draw()
    {
	    base.draw();
	    string newValue = _value;
	    if (Options.keyboardMode == KeyboardType.KEYBOARD_OFF)
	    {
		    if (_isFocused && _blink)
		    {
			    newValue += _char;
		    }
	    }
	    _text.setText(Unicode.convUtf32ToUtf8(_value));
	    clear();
	    _text.blit(this);
	    if (Options.keyboardMode == KeyboardType.KEYBOARD_ON)
	    {
		    if (_isFocused && _blink)
		    {
			    int x = 0;
			    switch (_text.getAlign())
			    {
			        case TextHAlign.ALIGN_LEFT:
				        x = 0;
				        break;
			        case TextHAlign.ALIGN_CENTER:
				        x = (_text.getWidth() - _text.getTextWidth()) / 2;
				        break;
			        case TextHAlign.ALIGN_RIGHT:
				        x = _text.getWidth() - _text.getTextWidth();
				        break;
			    }
			    for (int i = 0; i < _caretPos; ++i)
			    {
				    x += _text.getFont().getCharSize(_value[i]).w;
			    }
			    _caret.setX(x);
			    int y = 0;
			    switch (_text.getVerticalAlign())
			    {
			        case TextVAlign.ALIGN_TOP:
				        y = 0;
				        break;
			        case TextVAlign.ALIGN_MIDDLE:
				        y = (int)Math.Ceiling((getHeight() - _text.getTextHeight()) / 2.0);
				        break;
			        case TextVAlign.ALIGN_BOTTOM:
				        y = getHeight() - _text.getTextHeight();
				        break;
			    }
			    _caret.setY(y);
			    _caret.blit(this);
		    }
	    }
    }

    /**
     * Focuses the text edit when it's pressed on.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    internal override void mousePress(Action action, State state)
    {
	    if (action.getDetails().button.button == SDL_BUTTON_LEFT)
	    {
		    if (!_isFocused)
		    {
			    setFocus(true);
		    }
		    else
		    {
			    double mouseX = action.getRelativeXMouse();
			    double scaleX = action.getXScale();
			    double w = 0;
			    int c = 0;
			    foreach (var i in _value)
			    {
				    if (mouseX <= w)
				    {
					    break;
				    }
				    w += (double)_text.getFont().getCharSize(i).w / 2 * scaleX;
				    if (mouseX <= w)
				    {
					    break;
				    }
				    c++;
				    w += (double) _text.getFont().getCharSize(i).w / 2 * scaleX;
			    }
			    _caretPos = (uint)c;
		    }
	    }
	    base.mousePress(action, state);
    }

	/**
	 * Changes the text edit according to keyboard input, and
	 * unfocuses the text if Enter is pressed.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void keyboardPress(Action action, State state)
	{
		if (Options.keyboardMode == KeyboardType.KEYBOARD_OFF)
		{
			switch (action.getDetails().key.keysym.sym)
			{
			case SDL_Keycode.SDLK_UP:
				_char++;
				if (_char > '~')
				{
					_char = ' ';
				}
				break;
			case SDL_Keycode.SDLK_DOWN:
				_char--;
				if (_char < ' ')
				{
					_char = '~';
				}
				break;
			case SDL_Keycode.SDLK_LEFT:
				if (!string.IsNullOrEmpty(_value))
				{
					_value = _value[..(_value.Length - 1)];
				}
				break;
			case SDL_Keycode.SDLK_RIGHT:
				if (!exceedsMaxWidth((char)_char))
				{
					_value += _char;
				}
				break;
			default:
				break;
			}
		}
		else if (Options.keyboardMode == KeyboardType.KEYBOARD_ON)
		{
			switch (action.getDetails().key.keysym.sym)
			{
			case SDL_Keycode.SDLK_LEFT:
				if (_caretPos > 0)
				{
					_caretPos--;
				}
				break;
			case SDL_Keycode.SDLK_RIGHT:
				if (_caretPos < _value.Length)
				{
					_caretPos++;
				}
				break;
			case SDL_Keycode.SDLK_HOME:
				_caretPos = 0;
				break;
			case SDL_Keycode.SDLK_END:
				_caretPos = (uint)_value.Length;
				break;
			case SDL_Keycode.SDLK_BACKSPACE:
				if (_caretPos > 0)
				{
					_value.Remove((int)(_caretPos - 1), 1);
					_caretPos--;
				}
				break;
			case SDL_Keycode.SDLK_DELETE:
				if (_caretPos < _value.Length)
				{
					_value.Remove((int)_caretPos, 1);
				}
				break;
			case SDL_Keycode.SDLK_RETURN:
			case SDL_Keycode.SDLK_KP_ENTER:
				if (!string.IsNullOrEmpty(_value))
				{
					setFocus(false);
				}
				break;
			default:
				char c = (char)action.getDetails().key.keysym.unicode;
				if (isValidChar(c) && !exceedsMaxWidth(c))
				{
					_value.Insert((int)_caretPos, new string(c, 1));
					_caretPos++;
				}
				break;
			}
		}
		_redraw = true;
		if (_change != null)
		{
			_change(action);
		}

		base.keyboardPress(action, state);
	}

	/**
	 * Checks if adding a certain character to
	 * the text edit will exceed the maximum width.
	 * Used to make sure user input stays within bounds.
	 * @param c Character to add.
	 * @return True if it exceeds, False if it doesn't.
	 */
	bool exceedsMaxWidth(char c)
	{
		int w = 0;
		string s = _value;

		s += c;
		foreach (var i in s)
		{
			w += _text.getFont().getCharSize(i).w;
		}

		return (w > getWidth());
	}

	/**
	 * Checks if input key character is valid to
	 * be inserted at caret position in the text edit
	 * without breaking the text edit constraint.
	 * @param c Character to validate.
	 * @return True if character can be inserted, False if it cannot.
	 */
	bool isValidChar(char c)
	{
		switch (_textEditConstraint)
		{
		case TextEditConstraint.TEC_NUMERIC_POSITIVE:
			return c >= '0' && c <= '9';

		// If constraint is "(signed) numeric", need to check:
		// - user does not input a character before '-' or '+'
		// - user enter either figure anywhere, or a sign at first position
		case TextEditConstraint.TEC_NUMERIC:
			if (_caretPos > 0)
			{
				return c >= '0' && c <= '9';
			}
			else
			{
				return ((c >= '0' && c <= '9') || c == '+' || c == '-') &&
						(string.IsNullOrEmpty(_value) || (_value[0] != '+' && _value[0] != '-'));
			}

		case TextEditConstraint.TEC_NONE:
			return (c >= ' ' && c <= '~') || c >= 160;

		default:
			return false;
		}
	}

	/**
	 * Changes the various fonts for the text edit to use.
	 * The different fonts need to be passed in advance since the
	 * text size can change mid-text.
	 * @param big Pointer to large-size font.
	 * @param small Pointer to small-size font.
	 * @param lang Pointer to current language.
	 */
	internal override void initText(Font big, Font small, Language lang)
	{
		_text.initText(big, small, lang);
		_caret.initText(big, small, lang);
	}

	/**
	 * Replaces a certain amount of colors in the text edit's palette.
	 * @param colors Pointer to the set of colors.
	 * @param firstcolor Offset of the first color to replace.
	 * @param ncolors Amount of colors to replace.
	 */
	internal override void setPalette(SDL_Color[] colors, int firstcolor = 0, int ncolors = 256)
	{
		base.setPalette(colors, firstcolor, ncolors);
		_text.setPalette(colors, firstcolor, ncolors);
		_caret.setPalette(colors, firstcolor, ncolors);
	}

	/**
	 * Changes the color used to render the text. Unlike regular graphics,
	 * fonts are greyscale so they need to be assigned a specific position
	 * in the palette to be displayed.
	 * @param color Color value.
	 */
	internal override void setColor(byte color)
	{
		_text.setColor(color);
		_caret.setColor(color);
	}

	/**
	 * Changes the secondary color used to render the text. The text
	 * switches between the primary and secondary color whenever there's
	 * a 0x01 in the string.
	 * @param color Color value.
	 */
	internal override void setSecondaryColor(byte color) =>
		_text.setSecondaryColor(color);

	/**
	 * Enables/disables high contrast color. Mostly used for
	 * Battlescape text.
	 * @param contrast High contrast setting.
	 */
	internal override void setHighContrast(bool contrast)
	{
		_text.setHighContrast(contrast);
		_caret.setHighContrast(contrast);
	}

	/**
	 * Changes the text edit to use the small-size font.
	 */
	void setSmall()
	{
		_text.setSmall();
		_caret.setSmall();
	}

	/**
	 * Enables/disables color inverting. Mostly used to make
	 * button text look pressed along with the button.
	 * @param invert Invert setting.
	 */
	void setInvert(bool invert)
	{
		_text.setInvert(invert);
		_caret.setInvert(invert);
	}

	/**
	 * Returns the color used to render the text.
	 * @return Color value.
	 */
	byte getColor() =>
		_text.getColor();

	/**
	 * Returns the secondary color used to render the text.
	 * @return Color value.
	 */
	byte getSecondaryColor() =>
		_text.getSecondaryColor();
}
