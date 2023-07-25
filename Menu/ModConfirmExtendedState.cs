/*
 * Copyright 2010-2019 OpenXcom Developers.
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
 * Confirmation window when enabling
 * mods that require OXCE.
 */
internal class ModConfirmExtendedState : State
{
	ModListState _state;
	bool _isMaster;
	Window _window;
	TextButton _btnYes, _btnNo;
	Text _txtTitle;

	/**
	 * Initializes all the elements in the Confirm OXCE screen.
	 * @param state Pointer to the Options|Mod state.
	 * @param modInfo What exactly mod caused this question?
	 */
	internal ModConfirmExtendedState(ModListState state, ModInfo modInfo)
	{
		_state = state;
		_isMaster = modInfo.isMaster();

		_screen = false;

		// Create objects
		_window = new Window(this, 256, 100, 32, 50, WindowPopup.POPUP_BOTH);
		_btnYes = new TextButton(60, 18, 60, 122);
		_btnNo = new TextButton(60, 18, 200, 122);
		_txtTitle = new Text(246, 50, 37, 64);

		// Set palette
		setInterface("optionsMenu");

		add(_window, "confirmDefaults", "optionsMenu");
		add(_btnYes, "confirmDefaults", "optionsMenu");
		add(_btnNo, "confirmDefaults", "optionsMenu");
		add(_txtTitle, "confirmDefaults", "optionsMenu");

		centerAllSurfaces();

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

		_btnYes.setText(tr("STR_YES"));
		_btnYes.onMouseClick(btnYesClick);

		_btnNo.setText(tr("STR_NO"));
		_btnNo.onMouseClick(btnNoClick);

		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setBig();
		_txtTitle.setWordWrap(true);
		_txtTitle.setText(tr("STR_OXCE_REQUIRED_QUESTION").arg(modInfo.getRequiredExtendedEngine()));
	}

	/**
	 *
	 */
	~ModConfirmExtendedState() { }

	/**
	 * Closes the window. Enables the mod.
	 * @param action Pointer to an action.
	 */
	void btnYesClick(Action _)
	{
		_game.popState();

		if (_isMaster)
		{
			_state.changeMasterMod();
		}
		else
		{
			_state.toggleMod();
		}
	}

	/**
	 * Closes the window. Does not enable the mod.
	 * @param action Pointer to an action.
	 */
	void btnNoClick(Action _)
	{
		_game.popState();

		if (_isMaster)
		{
			_state.revertMasterMod();
		}
	}
}
