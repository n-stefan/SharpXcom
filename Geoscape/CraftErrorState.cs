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

namespace SharpXcom.Geoscape;

/**
 * Window used to notify the player when
 * an error occurs with a craft procedure.
 */
internal class CraftErrorState : State
{
    GeoscapeState _state;
    Window _window;
    TextButton _btnOk, _btnOk5Secs;
    Text _txtMessage;

    /**
     * Initializes all the elements in a Craft Error window.
     * @param game Pointer to the core game.
     * @param state Pointer to the Geoscape state.
     * @param msg Error message.
     */
    internal CraftErrorState(GeoscapeState state, string msg)
    {
        _state = state;

        _screen = false;

        // Create objects
        _window = new Window(this, 256, 160, 32, 20, WindowPopup.POPUP_BOTH);
        _btnOk = new TextButton(100, 18, 48, 150);
        _btnOk5Secs = new TextButton(100, 18, 172, 150);
        _txtMessage = new Text(246, 96, 37, 42);

        // Set palette
        setInterface("craftError");

        add(_window, "window", "craftError");
        add(_btnOk, "button", "craftError");
        add(_btnOk5Secs, "button", "craftError");
        add(_txtMessage, "text1", "craftError");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK12.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _btnOk5Secs.setText(tr("STR_OK_5_SECONDS"));
        _btnOk5Secs.onMouseClick(btnOk5SecsClick);
        _btnOk5Secs.onKeyboardPress(btnOk5SecsClick, Options.keyOk);

        _txtMessage.setAlign(TextHAlign.ALIGN_CENTER);
        _txtMessage.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
        _txtMessage.setBig();
        _txtMessage.setWordWrap(true);
        _txtMessage.setText(msg);
    }

    /**
     *
     */
    ~CraftErrorState() { }

    /**
     * Closes the window.
     * @param action Pointer to an action.
     */
    void btnOkClick(Engine.Action _) =>
        _game.popState();

    /**
     * Closes the window.
     * @param action Pointer to an action.
     */
    void btnOk5SecsClick(Engine.Action _)
    {
        _state.timerReset();
        _game.popState();
    }
}
