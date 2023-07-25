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
 * New Game window that displays a list
 * of possible difficulties for creating
 * a saved game.
 */
internal class NewGameState : State
{
    Window _window;
    TextButton _btnBeginner, _btnExperienced, _btnVeteran, _btnGenius, _btnSuperhuman;
    ToggleTextButton _btnIronman;
    TextButton _btnOk, _btnCancel;
    Text _txtTitle, _txtIronman;
    TextButton _difficulty;

    /**
	 * Initializes all the elements in the Difficulty window.
	 * @param game Pointer to the core game.
	 */
    internal NewGameState()
	{
		// Create objects
		_window = new Window(this, 192, 180, 64, 10, WindowPopup.POPUP_VERTICAL);
		_btnBeginner = new TextButton(160, 18, 80, 32);
		_btnExperienced = new TextButton(160, 18, 80, 52);
		_btnVeteran = new TextButton(160, 18, 80, 72);
		_btnGenius = new TextButton(160, 18, 80, 92);
		_btnSuperhuman = new TextButton(160, 18, 80, 112);
		_btnIronman = new ToggleTextButton(78, 18, 80, 138);
		_btnOk = new TextButton(78, 16, 80, 164);
		_btnCancel = new TextButton(78, 16, 162, 164);
		_txtTitle = new Text(192, 9, 64, 20);
		_txtIronman = new Text(90, 24, 162, 135);

		_difficulty = _btnBeginner;

		// Set palette
		setInterface("newGameMenu");

		add(_window, "window", "newGameMenu");
		add(_btnBeginner, "button", "newGameMenu");
		add(_btnExperienced, "button", "newGameMenu");
		add(_btnVeteran, "button", "newGameMenu");
		add(_btnGenius, "button", "newGameMenu");
		add(_btnSuperhuman, "button", "newGameMenu");
		add(_btnIronman, "ironman", "newGameMenu");
		add(_btnOk, "button", "newGameMenu");
		add(_btnCancel, "button", "newGameMenu");
		add(_txtTitle, "text", "newGameMenu");
		add(_txtIronman, "ironman", "newGameMenu");

		centerAllSurfaces();

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

		_btnBeginner.setText(tr("STR_1_BEGINNER"));
		_btnBeginner.setGroup(_difficulty);

		_btnExperienced.setText(tr("STR_2_EXPERIENCED"));
		_btnExperienced.setGroup(_difficulty);

		_btnVeteran.setText(tr("STR_3_VETERAN"));
		_btnVeteran.setGroup(_difficulty);

		_btnGenius.setText(tr("STR_4_GENIUS"));
		_btnGenius.setGroup(_difficulty);

		_btnSuperhuman.setText(tr("STR_5_SUPERHUMAN"));
		_btnSuperhuman.setGroup(_difficulty);

		_btnIronman.setText(tr("STR_IRONMAN"));

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

		_btnCancel.setText(tr("STR_CANCEL"));
		_btnCancel.onMouseClick(btnCancelClick);
		_btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setText(tr("STR_SELECT_DIFFICULTY_LEVEL"));

		_txtIronman.setWordWrap(true);
		_txtIronman.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
		_txtIronman.setText(tr("STR_IRONMAN_DESC"));
	}

	/**
	 *
	 */
	~NewGameState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        GameDifficulty diff = GameDifficulty.DIFF_BEGINNER;
        if (_difficulty == _btnBeginner)
        {
            diff = GameDifficulty.DIFF_BEGINNER;
        }
        else if (_difficulty == _btnExperienced)
        {
            diff = GameDifficulty.DIFF_EXPERIENCED;
        }
        else if (_difficulty == _btnVeteran)
        {
            diff = GameDifficulty.DIFF_VETERAN;
        }
        else if (_difficulty == _btnGenius)
        {
            diff = GameDifficulty.DIFF_GENIUS;
        }
        else if (_difficulty == _btnSuperhuman)
        {
            diff = GameDifficulty.DIFF_SUPERHUMAN;
        }
        SavedGame save = _game.getMod().newSave();
        save.setDifficulty(diff);
        save.setIronman(_btnIronman.getPressed());
        _game.setSavedGame(save);

        GeoscapeState gs = new GeoscapeState();
        _game.setState(gs);
        gs.init();
        _game.pushState(new BuildNewBaseState(_game.getSavedGame().getBases().Last(), gs.getGlobe(), true));
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _)
    {
        _game.setSavedGame(null);
        _game.popState();
    }
}
