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
 * Notifies the player when a targeted UFO
 * goes outside radar range.
 */
internal class UfoLostState : State
{
    string _id;
    Window _window;
    TextButton _btnOk;
    Text _txtTitle;

    /**
     * Initializes all the elements in the Ufo Lost window.
     * @param game Pointer to the core game.
     * @param id Name of the UFO.
     */
    internal UfoLostState(string id)
    {
        _id = id;

        _screen = false;

        // Create objects
        _window = new Window(this, 192, 104, 32, 48, WindowPopup.POPUP_BOTH);
        _btnOk = new TextButton(60, 12, 98, 112);
        _txtTitle = new Text(160, 32, 48, 72);

        // Set palette
        setInterface("UFOLost");

        add(_window, "window", "UFOLost");
        add(_btnOk, "button", "UFOLost");
        add(_txtTitle, "text", "UFOLost");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK15.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        string s = _id;
        s += '\n';
        s += tr("STR_TRACKING_LOST");
        _txtTitle.setText(s);
    }

    /**
     *
     */
    ~UfoLostState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Engine.Action _) =>
        _game.popState();
}
