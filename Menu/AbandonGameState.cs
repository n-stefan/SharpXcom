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
 * Abandon Game window shown before
 * quitting the game from the Geoscape.
 */
internal class AbandonGameState : State
{
    OptionsOrigin _origin;
    Window _window;
    TextButton _btnYes, _btnNo;
    Text _txtTitle;

    /**
     * Initializes all the elements in the Abandon Game screen.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     */
    internal AbandonGameState(OptionsOrigin origin)
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
        _btnYes = new TextButton(50, 20, x + 18, 140);
        _btnNo = new TextButton(50, 20, x + 148, 140);
        _txtTitle = new Text(206, 17, x + 5, 70);

        // Set palette
        setInterface("geoscape", false, _game.getSavedGame() != null ? _game.getSavedGame().getSavedBattle() : null);

        add(_window, "genericWindow", "geoscape");
        add(_btnYes, "genericButton2", "geoscape");
        add(_btnNo, "genericButton2", "geoscape");
        add(_txtTitle, "genericText", "geoscape");

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
        _txtTitle.setText(tr("STR_ABANDON_GAME_QUESTION"));

        if (_origin == OptionsOrigin.OPT_BATTLESCAPE)
        {
            applyBattlescapeTheme();
        }
    }

    /**
     *
     */
    ~AbandonGameState() { }

    /**
     * Goes back to the Main Menu.
     * @param action Pointer to an action.
     */
    void btnYesClick(Action _)
    {
        if (_origin == OptionsOrigin.OPT_BATTLESCAPE && _game.getSavedGame().getSavedBattle().getAmbientSound() != -1)
            _game.getMod().getSoundByDepth(0, (uint)_game.getSavedGame().getSavedBattle().getAmbientSound()).stopLoop();
        if (!_game.getSavedGame().isIronman())
        {
            Screen.updateScale(Options.geoscapeScale, ref Options.baseXGeoscape, ref Options.baseYGeoscape, true);
            _game.getScreen().resetDisplay(false);

            _game.setState(new MainMenuState());
            _game.setSavedGame(null);
        }
        else
        {
            _game.pushState(new SaveGameState(OptionsOrigin.OPT_GEOSCAPE, SaveType.SAVE_IRONMAN_END, _palette));
        }
    }

    /**
     * Closes the window.
     * @param action Pointer to an action.
     */
    void btnNoClick(Action _) =>
        _game.popState();
}
