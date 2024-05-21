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

// Utility class for enqueuing a state in the stack that goes to the main menu
class GoToMainMenuState : State
{
	internal override void init()
	{
		Screen.updateScale(Options.geoscapeScale, ref Options.baseXGeoscape, ref Options.baseYGeoscape, true);
		_game.getScreen().resetDisplay(false);
		_game.setState(new MainMenuState());
	}
}

/**
 * Main Menu window displayed when first
 * starting the game.
 */
internal class MainMenuState : State
{
    Window _window;
    TextButton _btnNewGame, _btnNewBattle, _btnLoad, _btnOptions, _btnMods, _btnQuit;
    Text _txtTitle;

    /**
	 * Initializes all the elements in the Main Menu window.
	 * @param game Pointer to the core game.
	 */
    internal MainMenuState()
	{
		// Create objects
		_window = new Window(this, 256, 160, 32, 20, WindowPopup.POPUP_BOTH);
		_btnNewGame = new TextButton(92, 20, 64, 90);
		_btnNewBattle = new TextButton(92, 20, 164, 90);
		_btnLoad = new TextButton(92, 20, 64, 118);
		_btnOptions = new TextButton(92, 20, 164, 118);
		_btnMods = new TextButton(92, 20, 64, 146);
		_btnQuit = new TextButton(92, 20, 164, 146);
		_txtTitle = new Text(256, 30, 32, 45);

		// Set palette
		setInterface("mainMenu");

		add(_window, "window", "mainMenu");
		add(_btnNewGame, "button", "mainMenu");
		add(_btnNewBattle, "button", "mainMenu");
		add(_btnLoad, "button", "mainMenu");
		add(_btnOptions, "button", "mainMenu");
		add(_btnMods, "button", "mainMenu");
		add(_btnQuit, "button", "mainMenu");
		add(_txtTitle, "text", "mainMenu");

		centerAllSurfaces();

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

		_btnNewGame.setText(tr("STR_NEW_GAME"));
		_btnNewGame.onMouseClick(btnNewGameClick);

		_btnNewBattle.setText(tr("STR_NEW_BATTLE"));
		_btnNewBattle.onMouseClick(btnNewBattleClick);

		_btnLoad.setText(tr("STR_LOAD_SAVED_GAME"));
		_btnLoad.onMouseClick(btnLoadClick);

		_btnOptions.setText(tr("STR_OPTIONS"));
		_btnOptions.onMouseClick(btnOptionsClick);

		_btnMods.setText(tr("STR_MODS"));
		_btnMods.onMouseClick(btnModsClick);

		_btnQuit.setText(tr("STR_QUIT"));
		_btnQuit.onMouseClick(btnQuitClick);

		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setBig();
		string title = $"{tr("STR_OPENXCOM")}{Unicode.TOK_NL_SMALL}{OPENXCOM_VERSION_SHORT}{OPENXCOM_VERSION_GIT}";
		_txtTitle.setText(title);
	}

	/**
	 *
	 */
	~MainMenuState() { }

    /**
     * Opens the New Game window.
     * @param action Pointer to an action.
     */
    void btnNewGameClick(Action _) =>
        _game.pushState(new NewGameState());

    /**
     * Opens the New Battle screen.
     * @param action Pointer to an action.
     */
    void btnNewBattleClick(Action _) =>
        _game.pushState(new NewBattleState());

    /**
     * Opens the Load Game screen.
     * @param action Pointer to an action.
     */
    void btnLoadClick(Action _) =>
        _game.pushState(new ListLoadState(OptionsOrigin.OPT_MENU));

    /**
     * Opens the Options screen.
     * @param action Pointer to an action.
     */
    void btnOptionsClick(Action _)
    {
        Options.backupDisplay();
        _game.pushState(new OptionsVideoState(OptionsOrigin.OPT_MENU));
    }

	/**
	* Opens the Mods screen.
	* @param action Pointer to an action.
	*/
	void btnModsClick(Action _) =>
		_game.pushState(new ModListState());

    /**
     * Quits the game.
     * @param action Pointer to an action.
     */
    void btnQuitClick(Action _) =>
        _game.quit();

	/**
	 * Updates the scale.
	 * @param dX delta of X;
	 * @param dY delta of Y;
	 */
	internal override void resize(ref int dX, ref int dY)
	{
		dX = Options.baseXResolution;
		dY = Options.baseYResolution;
		Screen.updateScale(Options.geoscapeScale, ref Options.baseXGeoscape, ref Options.baseYGeoscape, true);
		dX = Options.baseXResolution - dX;
		dY = Options.baseYResolution - dY;
		base.resize(ref dX, ref dY);
	}
}
