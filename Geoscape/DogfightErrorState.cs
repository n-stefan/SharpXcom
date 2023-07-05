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
 * an error occurs with a dogfight procedure.
 */
internal class DogfightErrorState : State
{
    Craft _craft;
    Window _window;
    TextButton _btnIntercept, _btnBase;
    Text _txtCraft, _txtMessage;

    /**
     * Initializes all the elements in a Dogfight Error window.
     * @param game Pointer to the core game.
     * @param state Pointer to the Geoscape state.
     * @param msg Error message.
     */
    internal DogfightErrorState(Craft craft, string msg)
    {
        _craft = craft;

        _screen = false;

        // Create objects
        _window = new Window(this, 208, 120, 24, 48, WindowPopup.POPUP_BOTH);
        _btnIntercept = new TextButton(180, 12, 38, 128);
        _btnBase = new TextButton(180, 12, 38, 144);
        _txtCraft = new Text(198, 16, 29, 63);
        _txtMessage = new Text(198, 20, 29, 94);

        // Set palette
        setInterface("dogfightInfo");

        add(_window, "window", "dogfightInfo");
        add(_btnIntercept, "button", "dogfightInfo");
        add(_btnBase, "button", "dogfightInfo");
        add(_txtCraft, "text", "dogfightInfo");
        add(_txtMessage, "text", "dogfightInfo");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK15.SCR"));

        _btnIntercept.setText(tr("STR_CONTINUE_INTERCEPTION_PURSUIT"));
        _btnIntercept.onMouseClick(btnInterceptClick);
        _btnIntercept.onKeyboardPress(btnInterceptClick, Options.keyCancel);

        _btnBase.setText(tr("STR_RETURN_TO_BASE"));
        _btnBase.onMouseClick(btnBaseClick);
        _btnBase.onKeyboardPress(btnBaseClick, Options.keyOk);

        _txtCraft.setAlign(TextHAlign.ALIGN_CENTER);
        _txtCraft.setBig();
        _txtCraft.setText(_craft.getName(_game.getLanguage()));

        _txtMessage.setAlign(TextHAlign.ALIGN_CENTER);
        _txtMessage.setWordWrap(true);
        _txtMessage.setText(msg);
    }

    /**
     *
     */
    ~DogfightErrorState() { }

    /**
     * Closes the window.
     * @param action Pointer to an action.
     */
    void btnInterceptClick(Engine.Action _) =>
        _game.popState();

    /**
     * Returns the craft to base.
     * @param action Pointer to an action.
     */
    void btnBaseClick(Engine.Action _)
    {
        _craft.returnToBase();
        _game.popState();
    }
}
