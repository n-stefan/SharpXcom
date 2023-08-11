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
 * Asks user new coordinates when he pushes Fixed Borderless Pos button.
 * are changed.
 */
internal class SetWindowedRootState : State
{
	OptionsOrigin _origin;
	OptionsVideoState _optionsVideoState;
	Window _window;
	Text _txtTitle, _txtWindowedModePositionX, _txtWindowedModePositionY;
	TextButton _btnOk, _btnCancel;
	TextEdit _edtWindowedModePositionX, _edtWindowedModePositionY;

	/**
	 * Initializes all the elements in the Asks user new coordinates screen.
	 * @param game Pointer to the core game.
	 * @param OptionsVideoState Options screen that originated this state.
	 */
	internal SetWindowedRootState(OptionsOrigin origin, OptionsVideoState optionsVideoState)
	{
		_origin = origin;
		_optionsVideoState = optionsVideoState;

		_screen = false;

		// Create objects
		_window = new Window(this, 216, 100, 52, 50, WindowPopup.POPUP_BOTH);
		_txtTitle = new Text(206, 20, 57, 70);
		_txtWindowedModePositionX = new Text(160, 10, 25, 90);
		_txtWindowedModePositionY = new Text(160, 10, 25, 100);
		_btnOk = new TextButton(50, 20, 70, 120);
		_btnCancel = new TextButton(50, 20, 200, 120);
		_edtWindowedModePositionX = new TextEdit(this, 40, 10, 190, 90);
		_edtWindowedModePositionY = new TextEdit(this, 40, 10, 190, 100);

		// Set palette
		setInterface("optionsMenu", false, _game.getSavedGame() != null ? _game.getSavedGame().getSavedBattle() : null);

		add(_window, "confirmVideo", "optionsMenu");
		add(_btnOk, "confirmVideo", "optionsMenu");
		add(_btnCancel, "confirmVideo", "optionsMenu");
		add(_txtTitle, "confirmVideo", "optionsMenu");
		add(_txtWindowedModePositionX, "confirmVideo", "optionsMenu");
		add(_txtWindowedModePositionY, "confirmVideo", "optionsMenu");
		add(_edtWindowedModePositionX, "confirmVideo", "optionsMenu");
		add(_edtWindowedModePositionY, "confirmVideo", "optionsMenu");

		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setWordWrap(true);
		_txtTitle.setText(tr("STR_DISPLAY_SET_WINDOW_POSITION"));

		_txtWindowedModePositionX.setAlign(TextHAlign.ALIGN_RIGHT);
		_txtWindowedModePositionX.setWordWrap(true);
		_txtWindowedModePositionX.setText(tr("STR_DISPLAY_WINDOW_POSITION_NEW_X"));

		_txtWindowedModePositionY.setAlign(TextHAlign.ALIGN_RIGHT);
		_txtWindowedModePositionY.setWordWrap(true);
		_txtWindowedModePositionY.setText(tr("STR_DISPLAY_WINDOW_POSITION_NEW_Y"));

		string wss = Options.newWindowedModePositionX.ToString();
		string wss2 = Options.newWindowedModePositionY.ToString();

		_edtWindowedModePositionX.setText(wss);
		_edtWindowedModePositionX.setConstraint(TextEditConstraint.TEC_NUMERIC);

		_edtWindowedModePositionY.setText(wss2);
		_edtWindowedModePositionY.setConstraint(TextEditConstraint.TEC_NUMERIC);

		centerAllSurfaces();

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

		_btnCancel.setText(tr("STR_CANCEL"));
		_btnCancel.onMouseClick(btnCancelClick);
		_btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

		if (_origin == OptionsOrigin.OPT_BATTLESCAPE)
		{
			applyBattlescapeTheme();
		}
	}

	/**
	 *
	 */
	~SetWindowedRootState() { }

	/**
	 * Roots borderless window
	 * @param action Pointer to an action.
	 */
	void btnOkClick(Action _)
	{
		string convStreamX, convStreamY;
		int newWindowedModePositionX = 0, newWindowedModePositionY = 0;

		convStreamX = _edtWindowedModePositionX.getText();
		convStreamY = _edtWindowedModePositionY.getText();

		newWindowedModePositionX = int.Parse(convStreamX);
		newWindowedModePositionY = int.Parse(convStreamY);

		Options.newRootWindowedMode = true;
		Options.newWindowedModePositionX = newWindowedModePositionX;
		Options.newWindowedModePositionY = newWindowedModePositionY;

		_game.popState();
	}

	/**
	 * Cancels borderless window rooting
	 * @param action Pointer to an action.
	 */
	void btnCancelClick(Action _)
	{
		_optionsVideoState.unpressRootWindowedMode();

		_game.popState();
	}
}
