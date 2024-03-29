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

namespace SharpXcom.Geoscape;

/**
 * Window used to input a name for a new base.
 * Player's first Base uses this screen
 * additional bases use ConfirmNewBaseState
 */
internal class BaseNameState : State
{
    Base _base;
    Globe _globe;
    bool _first;
    Window _window;
    TextButton _btnOk;
    Text _txtTitle;
    TextEdit _edtName;

    /**
     * Initializes all the elements in a Base Name window.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to name.
     * @param globe Pointer to the Geoscape globe.
     * @param first Is this the first base in the game?
     */
    internal BaseNameState(Base @base, Globe globe, bool first)
    {
        _base = @base;
        _globe = globe;
        _first = first;

        _globe.onMouseOver(null);

        _screen = false;

        // Create objects
        _window = new Window(this, 192, 80, 32, 60, WindowPopup.POPUP_BOTH);
        _btnOk = new TextButton(162, 12, 47, 118);
        _txtTitle = new Text(182, 17, 37, 70);
        _edtName = new TextEdit(this, 127, 16, 59, 94);

        // Set palette
        setInterface("baseNaming");

        add(_window, "window", "baseNaming");
        add(_btnOk, "button", "baseNaming");
        add(_txtTitle, "text", "baseNaming");
        add(_edtName, "text", "baseNaming");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        //_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        //something must be in the name before it is acceptable
        _btnOk.setVisible(false);

        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setBig();
        _txtTitle.setText(tr("STR_BASE_NAME"));

        _edtName.setBig();
        _edtName.setFocus(true, false);
        _edtName.onChange(edtNameChange);
    }

    /**
     *
     */
    ~BaseNameState() { }

    /**
     * Returns to the previous screen
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        if (!string.IsNullOrEmpty(_edtName.getText()))
        {
            _game.popState();
            _game.popState();
            if (!_first || Options.customInitialBase)
            {
                if (!_first)
                {
                    _game.popState();
                }
                _game.pushState(new PlaceLiftState(_base, _globe, _first));
            }
        }
    }

    /**
     * Updates the base name and disables the OK button
     * if no name is entered.
     * @param action Pointer to an action.
     */
    void edtNameChange(Action action)
    {
        _base.setName(_edtName.getText());
        if (action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_RETURN ||
            action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_KP_ENTER)
        {
            if (!string.IsNullOrEmpty(_edtName.getText()))
            {
                btnOkClick(action);
            }
        }
        else
        {
            _btnOk.setVisible(!string.IsNullOrEmpty(_edtName.getText()));
        }
    }
}
