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
 * Options window shown for loading/saving/quitting the game.
 * Not to be confused with the Game Options window
 * for changing game settings during runtime.
 */
internal class PauseState : State
{
    OptionsOrigin _origin;
    Window _window;
    TextButton _btnLoad, _btnSave, _btnAbandon, _btnOptions, _btnCancel;
    Text _txtTitle;

    /**
     * Initializes all the elements in the Pause window.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     */
    internal PauseState(OptionsOrigin origin)
    {
        _origin = origin;

        _screen = false;

        int x;
        if (_origin == OptionsOrigin.OPT_GEOSCAPE)
        {
            x = 20;
        }
        else
        {
            x = 52;
        }

        // Create objects
        _window = new Window(this, 216, 160, x, 20, WindowPopup.POPUP_BOTH);
        _btnLoad = new TextButton(180, 18, x + 18, 52);
        _btnSave = new TextButton(180, 18, x + 18, 74);
        _btnAbandon = new TextButton(180, 18, x + 18, 96);
        _btnOptions = new TextButton(180, 18, x + 18, 122);
        _btnCancel = new TextButton(180, 18, x + 18, 150);
        _txtTitle = new Text(206, 17, x + 5, 32);

        // Set palette
        setInterface("pauseMenu", false, _game.getSavedGame() != null ? _game.getSavedGame().getSavedBattle() : null);

        add(_window, "window", "pauseMenu");
        add(_btnLoad, "button", "pauseMenu");
        add(_btnSave, "button", "pauseMenu");
        add(_btnAbandon, "button", "pauseMenu");
        add(_btnOptions, "button", "pauseMenu");
        add(_btnCancel, "button", "pauseMenu");
        add(_txtTitle, "text", "pauseMenu");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

        _btnLoad.setText(tr("STR_LOAD_GAME"));
        _btnLoad.onMouseClick(btnLoadClick);

        _btnSave.setText(tr("STR_SAVE_GAME"));
        _btnSave.onMouseClick(btnSaveClick);

        _btnAbandon.setText(tr("STR_ABANDON_GAME"));
        _btnAbandon.onMouseClick(btnAbandonClick);

        _btnOptions.setText(tr("STR_GAME_OPTIONS"));
        _btnOptions.onMouseClick(btnOptionsClick);

        _btnCancel.setText(tr("STR_CANCEL_UC"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);
        if (origin == OptionsOrigin.OPT_GEOSCAPE)
        {
            _btnCancel.onKeyboardPress(btnCancelClick, Options.keyGeoOptions);
        }
        else if (origin == OptionsOrigin.OPT_BATTLESCAPE)
        {
            _btnCancel.onKeyboardPress(btnCancelClick, Options.keyBattleOptions);
            if (_game.getSavedGame().getSavedBattle().getBattleGame().getStates().Any())
            {
                _btnOptions.setVisible(false);
            }
        }

        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setBig();
        _txtTitle.setText(tr("STR_OPTIONS_UC"));

        if (_origin == OptionsOrigin.OPT_BATTLESCAPE)
        {
            applyBattlescapeTheme();
        }

        if (_game.getSavedGame().isIronman())
        {
            _btnLoad.setVisible(false);
            _btnSave.setVisible(false);
            _btnAbandon.setText(tr("STR_SAVE_AND_ABANDON_GAME"));
        }
    }

    /**
     *
     */
    ~PauseState() { }

    /**
     * Opens the Load Game screen.
     * @param action Pointer to an action.
     */
    void btnLoadClick(Action _) =>
        _game.pushState(new ListLoadState(_origin));

    /**
     * Opens the Save Game screen.
     * @param action Pointer to an action.
     */
    void btnSaveClick(Action _) =>
        _game.pushState(new ListSaveState(_origin));

    /**
     * Opens the Abandon Game window.
     * @param action Pointer to an action.
     */
    void btnAbandonClick(Action _) =>
        _game.pushState(new AbandonGameState(_origin));

    /**
     * Opens the Game Options screen.
     * @param action Pointer to an action.
     */
    void btnOptionsClick(Action _)
    {
        Options.backupDisplay();
        if (_origin == OptionsOrigin.OPT_GEOSCAPE)
        {
            _game.pushState(new OptionsGeoscapeState(_origin));
        }
        else if (_origin == OptionsOrigin.OPT_BATTLESCAPE)
        {
            _game.pushState(new OptionsBattlescapeState(_origin));
        }
        else
        {
            _game.pushState(new OptionsVideoState(_origin));
        }
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _) =>
        _game.popState();
}
