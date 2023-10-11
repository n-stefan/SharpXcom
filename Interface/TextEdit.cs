﻿/*
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
    internal TextEdit(State state, int width, int height, int x, int y) : base(width, height, x, y)
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
    protected override void think() =>
	    _timer.think(null, this);
}
