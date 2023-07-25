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

namespace SharpXcom.Battlescape;

/**
 * Notifies the player about things like soldiers going unconscious or dying from wounds.
 */
internal class InfoboxOKState : State
{
    Frame _frame;
    TextButton _btnOk;
    Text _txtTitle;

    /**
	 * Initializes all the elements.
	 * @param game Pointer to the core game.
	 * @param msg Message string.
	 */
    internal InfoboxOKState(string msg)
	{
		_screen = false;

		// Create objects
		_frame = new Frame(261, 89, 30, 48);
		_btnOk = new TextButton(120, 18, 100, 112);
		_txtTitle = new Text(255, 61, 33, 51);

		// Set palette
		_game.getSavedGame().getSavedBattle().setPaletteByDepth(this);

		add(_frame, "infoBoxOK", "battlescape");
		add(_btnOk, "infoBoxOKButton", "battlescape");
		add(_txtTitle, "infoBoxOK", "battlescape");

		centerAllSurfaces();

		// Set up objects
		_frame.setThickness(3);
		_frame.setHighContrast(true);

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);
		_btnOk.setHighContrast(true);

		_txtTitle.setBig();
		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
		_txtTitle.setHighContrast(true);
		_txtTitle.setWordWrap(true);
		_txtTitle.setText(msg);

		_game.getCursor().setVisible(true);
	}

	/**
	 *
	 */
	~InfoboxOKState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();
}
