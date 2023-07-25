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

namespace SharpXcom.Menu;

/**
 * Confirmation window when restoring the
 * default game options.
 */
internal class OptionsDefaultsState : State
{
    OptionsOrigin _origin;
    OptionsBaseState _state;
    Window _window;
    TextButton _btnYes, _btnNo;
    Text _txtTitle;

    /**
     * Initializes all the elements in the Restore Defaults screen.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     * @param state Pointer to the base Options state.
     */
    internal OptionsDefaultsState(OptionsOrigin origin, OptionsBaseState state)
    {
        _origin = origin;
        _state = state;

        _screen = false;

        // Create objects
        _window = new Window(this, 256, 100, 32, 50, WindowPopup.POPUP_BOTH);
        _btnYes = new TextButton(60, 18, 60, 122);
        _btnNo = new TextButton(60, 18, 200, 122);
        _txtTitle = new Text(246, 32, 37, 70);

        // Set palette
        setInterface("optionsMenu", false, _game.getSavedGame() != null ? _game.getSavedGame().getSavedBattle() : null);

        add(_window, "confirmDefaults", "optionsMenu");
        add(_btnYes, "confirmDefaults", "optionsMenu");
        add(_btnNo, "confirmDefaults", "optionsMenu");
        add(_txtTitle, "confirmDefaults", "optionsMenu");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

        _btnYes.setText(tr("STR_YES"));
        _btnYes.onMouseClick(btnYesClick);
        _btnYes.onKeyboardPress(btnYesClick, Options.keyOk);

        _btnNo.setText(tr("STR_NO"));
        _btnNo.onMouseClick(btnNoClick);
        _btnNo.onKeyboardPress(btnNoClick, Options.keyCancel);

        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setBig();
        _txtTitle.setWordWrap(true);
        _txtTitle.setText(tr("STR_RESTORE_DEFAULTS_QUESTION"));

        if (_origin == OptionsOrigin.OPT_BATTLESCAPE)
        {
            applyBattlescapeTheme();
        }
    }

    /**
     *
     */
    ~OptionsDefaultsState() { }

    /**
     * Restores the default options.
     * @param action Pointer to an action.
     */
    void btnYesClick(Action action)
    {
        Options.resetDefault(false);
        _game.loadLanguages();
        _game.popState();
        _state.btnOkClick(action);
    }

    /**
     * Closes the window.
     * @param action Pointer to an action.
     */
    void btnNoClick(Action _) =>
        _game.popState();
}
