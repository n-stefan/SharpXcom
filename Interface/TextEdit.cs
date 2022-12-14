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
    Engine.Timer _timer;

    /**
     * Sets up a blank text edit with the specified size and position.
     * @param state Pointer to state the text edit belongs to.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    TextEdit(State state, int width, int height, int x, int y) : base(width, height, x, y)
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
        _timer = new Engine.Timer(100);
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
}
