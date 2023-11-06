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
 * Save Game screen for listing info on available
 * saved games and saving them.
 */
internal class ListSaveState : ListGamesState
{
    int _previousSelectedRow, _selectedRow;
    TextEdit _edtSave;
    TextButton _btnSaveGame;
	string _selected;

    /**
     * Initializes all the elements in the Save Game screen.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     */
    internal ListSaveState(OptionsOrigin origin) : base(origin, 1, false)
    {
        _previousSelectedRow = -1;
        _selectedRow = -1;

        // Create objects
        _edtSave = new TextEdit(this, 168, 9, 0, 0);
        _btnSaveGame = new TextButton(_game.getSavedGame().isIronman() ? 200 : 80, 16, 60, 172);

        add(_edtSave);
        add(_btnSaveGame, "button", "saveMenus");

        // Set up objects
        _txtTitle.setText(tr("STR_SELECT_SAVE_POSITION"));

        if (_game.getSavedGame().isIronman())
        {
            _btnCancel.setVisible(false);
        }
        else
        {
            _btnCancel.setX(180);
        }

        _btnSaveGame.setText(tr("STR_SAVE_GAME"));
        _btnSaveGame.onMouseClick(btnSaveGameClick);

        _edtSave.setColor(_lstSaves.getSecondaryColor());
        _edtSave.setVisible(false);
        _edtSave.onKeyboardPress(edtSaveKeyPress);

        centerAllSurfaces();
    }

    /**
     *
     */
    ~ListSaveState() { }

    /**
     * Saves the selected save.
     * @param action Pointer to an action.
     */
    void btnSaveGameClick(Action _)
    {
        if (_selectedRow != -1)
        {
            saveGame();
        }
    }

    /**
     * Saves the selected save.
     */
    void saveGame()
    {
        _game.getSavedGame().setName(_edtSave.getText());
        string oldFilename, newFilename;
        newFilename = CrossPlatform.sanitizeFilename(Unicode.convUtf8ToPath(_edtSave.getText()));
        if (_selectedRow > 0)
        {
            oldFilename = _saves[_selectedRow - 1].fileName;
            if (oldFilename != newFilename + ".sav")
            {
                while (CrossPlatform.fileExists(Options.getMasterUserFolder() + newFilename + ".sav"))
                {
                    newFilename += "_";
                }
                string oldPath = Options.getMasterUserFolder() + oldFilename;
                string newPath = Options.getMasterUserFolder() + newFilename + ".sav";
                CrossPlatform.moveFile(oldPath, newPath);
            }
        }
        else
        {
            while (CrossPlatform.fileExists(Options.getMasterUserFolder() + newFilename + ".sav"))
            {
                newFilename += "_";
            }
        }
        newFilename += ".sav";
        _game.pushState(new SaveGameState(_origin, newFilename, _palette));
    }

    /**
     * Saves the selected save.
     * @param action Pointer to an action.
     */
    void edtSaveKeyPress(Action action)
    {
        if (action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_RETURN ||
            action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_KP_ENTER)
        {
            saveGame();
        }
    }

    /**
     * Updates the save game list with the current list
     * of available savegames.
     */
    protected override void updateList()
    {
	    _lstSaves.addRow(1, tr("STR_NEW_SAVED_GAME_SLOT"));
	    if (_origin != OptionsOrigin.OPT_BATTLESCAPE)
		    _lstSaves.setRowColor(0, _lstSaves.getSecondaryColor());
	    base.updateList();
    }

    /**
     * Names the selected save.
     * @param action Pointer to an action.
     */
    protected override void lstSavesPress(Action action)
    {
	    if (action.getDetails().button.button == SDL_BUTTON_RIGHT && _edtSave.isFocused())
	    {
		    _edtSave.setText(string.Empty);
		    _edtSave.setVisible(false);
		    _edtSave.setFocus(false, false);
		    _lstSaves.setScrolling(true);
	    }
	    base.lstSavesPress(action);
	    if (action.getDetails().button.button == SDL_BUTTON_LEFT)
	    {
		    _previousSelectedRow = _selectedRow;
		    _selectedRow = (int)_lstSaves.getSelectedRow();

		    switch (_previousSelectedRow)
		    {
			    case -1:	// first click on the savegame list
				    break;
			    case 0:
				    _lstSaves.setCellText((uint)_previousSelectedRow, 0, tr("STR_NEW_SAVED_GAME_SLOT"));
				    break;
			    default:
				    _lstSaves.setCellText((uint)_previousSelectedRow, 0, _selected);
                    break;
		    }

		    _selected = _lstSaves.getCellText(_lstSaves.getSelectedRow(), 0);
		    _lstSaves.setCellText(_lstSaves.getSelectedRow(), 0, string.Empty);
		    if (_lstSaves.getSelectedRow() == 0)
		    {
			    _edtSave.setText(string.Empty);
			    _selected = string.Empty;
		    }
		    else
		    {
			    _edtSave.setText(_selected);
		    }
		    _edtSave.setX(_lstSaves.getColumnX(0));
		    _edtSave.setY(_lstSaves.getRowY((uint)_selectedRow));
		    _edtSave.setVisible(true);
		    _edtSave.setFocus(true, false);
		    _lstSaves.setScrolling(false);
		    disableSort();
	    }
    }
}
