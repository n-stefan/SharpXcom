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
 * Mods window to manage the installed mods.
 */
internal class ModListState : State
{
	uint _curMasterIdx;
	Window _window;
	Text _txtMaster;
	ComboBox _cbxMasters;
	TextList _lstMods;
	TextButton _btnOk, _btnCancel;
	Text _txtTooltip;
	string _curMasterId;
	List<ModInfo> _masters;
	string _currentTooltip;
	List<KeyValuePair<string, bool>> _mods;

	/**
	 * Initializes all the elements in the Mod Options window.
	 * @param game Pointer to the core game.
	 */
	internal ModListState()
	{
		_curMasterIdx = 0;

		// Create objects
		_window = new Window(this, 320, 200, 0, 0);

		_txtMaster = new Text(305, 9, 8, 8);
		_cbxMasters = new ComboBox(this, 305, 16, 8, 18);
		_lstMods = new TextList(288, 104, 8, 40);

		_btnOk = new TextButton(100, 16, 8, 176);
		_btnCancel = new TextButton(100, 16, 212, 176);

		_txtTooltip = new Text(305, 25, 8, 148);

		// Set palette
		setInterface("modsMenu");

		add(_window, "window", "modsMenu");

		add(_txtMaster, "text", "modsMenu");
		add(_lstMods, "optionLists", "modsMenu");
		add(_btnOk, "button2", "modsMenu");
		add(_btnCancel, "button2", "modsMenu");
		add(_txtTooltip, "tooltip", "modsMenu");

		add(_cbxMasters, "button1", "modsMenu");

		centerAllSurfaces();

		// how much room do we need for YES/NO
		Text text = new Text(100, 9, 0, 0);
		text.initText(_game.getMod().getFont("FONT_BIG"), _game.getMod().getFont("FONT_SMALL"), _game.getLanguage());
		text.setText(tr("STR_YES"));
		int yes = text.getTextWidth();
		text.setText(tr("STR_NO"));
		int no = text.getTextWidth();

		int rightcol = Math.Max(yes, no) + 2;
		int arrowCol = 25;
		int leftcol = _lstMods.getWidth() - (rightcol + arrowCol);

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

		_txtMaster.setText(tr("STR_BASE_GAME"));

		// scan for masters
		Options.refreshMods();
		Dictionary<string, ModInfo> modInfos = Options.getModInfos();
		var masterNames = new List<string>();
		foreach (var i in Options.mods)
		{
			string modId = i.Key;
			ModInfo modInfo = modInfos[modId];
			if (!modInfo.isMaster())
			{
				continue;
			}

			if (i.Value)
			{
				_curMasterId = modId;
			}
			else if (string.IsNullOrEmpty(_curMasterId))
			{
				++_curMasterIdx;
			}
			_masters.Add(modInfo);
			masterNames.Add(modInfo.getName());
		}

		_cbxMasters.setOptions(masterNames);
		_cbxMasters.setSelected(_curMasterIdx);
		_cbxMasters.onChange(cbxMasterChange);
		_cbxMasters.onMouseIn(txtTooltipIn);
		_cbxMasters.onMouseOut(txtTooltipOut);
		_cbxMasters.onMouseOver(cbxMasterHover);
		_cbxMasters.onListMouseIn(txtTooltipIn);
		_cbxMasters.onListMouseOut(txtTooltipOut);
		_cbxMasters.onListMouseOver(cbxMasterHover);

		_lstMods.setArrowColumn(leftcol + 1, ArrowOrientation.ARROW_VERTICAL);
		_lstMods.setColumns(3, leftcol, arrowCol, rightcol);
		_lstMods.setAlign(TextHAlign.ALIGN_RIGHT, 1);
		_lstMods.setSelectable(true);
		_lstMods.setBackground(_window);
		_lstMods.setWordWrap(true);
		_lstMods.onMouseClick(lstModsClick);
		_lstMods.onLeftArrowClick(lstModsLeftArrowClick);
		_lstMods.onRightArrowClick(lstModsRightArrowClick);
		_lstMods.onMousePress(lstModsMousePress);
		_lstMods.onMouseIn(txtTooltipIn);
		_lstMods.onMouseOut(txtTooltipOut);
		_lstMods.onMouseOver(lstModsHover);
		lstModsRefresh(0);

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

		_btnCancel.setText(tr("STR_CANCEL"));
		_btnCancel.onMouseClick(btnCancelClick);
		_btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

		_txtTooltip.setWordWrap(true);
	}

	~ModListState() { }

	void cbxMasterChange(Action _)
	{
		ModInfo masterModInfo = _masters[(int)_cbxMasters.getSelected()];

		// when changing a master mod, check if it requires OXCE
		{
			if (!masterModInfo.isEngineOk())
			{
				_game.pushState(new ModConfirmExtendedState(this, masterModInfo));
				return;
			}
		}

		changeMasterMod();
	}

	internal void changeMasterMod()
	{
		string masterId = _masters[(int)_cbxMasters.getSelected()].getId();
		var mods = Options.mods;
		for (var i = 0; i < mods.Count; i++)
		{
			if (masterId == mods[i].Key)
			{
				mods[i] = KeyValuePair.Create(mods[i].Key, true);
			}
			else if (_curMasterId == mods[i].Key)
			{
				mods[i] = KeyValuePair.Create(mods[i].Key, false);
			}
		}
		Options.reload = true;

		_curMasterIdx = _cbxMasters.getSelected();
		_curMasterId = masterId;
		lstModsRefresh(0);
	}

	/**
	 * Shows a tooltip for the appropriate button.
	 * @param action Pointer to an action.
	 */
	void txtTooltipIn(Action action)
	{
		_currentTooltip = action.getSender().getTooltip();
		_txtTooltip.setText(tr(_currentTooltip));
	}

	/**
	 * Clears the tooltip text.
	 * @param action Pointer to an action.
	 */
	void txtTooltipOut(Action action)
	{
		if (_currentTooltip == action.getSender().getTooltip())
		{
			_txtTooltip.setText(string.Empty);
		}
	}

	void cbxMasterHover(Action _) =>
		_txtTooltip.setText(makeTooltip(_masters[(int)_cbxMasters.getHoveredListIdx()]));

	void lstModsClick(Action action)
	{
		if (action.getAbsoluteXMouse() >= _lstMods.getArrowsLeftEdge() &&
			action.getAbsoluteXMouse() <= _lstMods.getArrowsRightEdge())
		{
			// don't count an arrow click as a mod enable toggle
			return;
		}

		KeyValuePair<string, bool> mod = _mods[(int)_lstMods.getSelectedRow()];

		// when activating a mod, check if it requires OXCE
		if (!mod.Value)
		{
			ModInfo modInfo = Options.getModInfos()[mod.Key];
			if (!modInfo.isEngineOk())
			{
				_game.pushState(new ModConfirmExtendedState(this, modInfo));
				return;
			}
		}

		// if deactivating, or if not OXCE mod
		toggleMod();
	}

	void lstModsLeftArrowClick(Action action)
	{
		uint row = _lstMods.getSelectedRow();
		if (row <= 0)
		{
			return;
		}

		if (action.getDetails().button.button == SDL_BUTTON_LEFT)
		{
			moveModUp(action, row);
		}
		else if (action.getDetails().button.button == SDL_BUTTON_RIGHT)
		{
			moveModUp(action, row, true);
		}
	}

	void lstModsRightArrowClick(Action action)
	{
		uint row = _lstMods.getSelectedRow();
		int numMods = _mods.Count;
		if (0 >= numMods || int.MaxValue < numMods || row >= numMods - 1)
		{
			return;
		}

		if (action.getDetails().button.button == SDL_BUTTON_LEFT)
		{
			moveModDown(action, row);
		}
		else if (action.getDetails().button.button == SDL_BUTTON_RIGHT)
		{
			moveModDown(action, row, true);
		}
	}

	void lstModsMousePress(Action action)
	{
		if (Options.changeValueByMouseWheel == 0)
			return;
		uint row = _lstMods.getSelectedRow();
		int numMods = _mods.Count;
		if (action.getDetails().wheel.y > 0 && //button.button == SDL_BUTTON_WHEELUP
			row > 0)
		{
			if (action.getAbsoluteXMouse() >= _lstMods.getArrowsLeftEdge() &&
				action.getAbsoluteXMouse() <= _lstMods.getArrowsRightEdge())
			{
				moveModUp(action, row);
			}
		}
		else if (action.getDetails().wheel.y < 0 && //button.button == SDL_BUTTON_WHEELDOWN
				 0 < numMods && int.MaxValue >= numMods && row < numMods - 1)
		{
			if (action.getAbsoluteXMouse() >= _lstMods.getArrowsLeftEdge() &&
				action.getAbsoluteXMouse() <= _lstMods.getArrowsRightEdge())
			{
				moveModDown(action, row);
			}
		}
	}

	void lstModsHover(Action _)
	{
		uint selectedRow = _lstMods.getSelectedRow();
		unchecked
		{
			if ((uint)-1 != selectedRow)
			{
				_txtTooltip.setText(makeTooltip(Options.getModInfos()[_mods[(int)selectedRow].Key]));
			}
		}
	}

	/**
	 * Restarts game with new mods.
	 * @param action Pointer to an action.
	 */
	void btnOkClick(Action _)
	{
		Options.save();
		if (Options.reload)
		{
			_game.setState(new StartState());
		}
		else
		{
			_game.popState();
		}
	}

	/**
	 * Ignores mod changes and returns to the previous screen.
	 * @param action Pointer to an action.
	 */
	void btnCancelClick(Action _)
	{
		Options.reload = false;
		Options.load();
		_game.popState();
	}

	void moveModUp(Action action, uint row, bool max = false)
	{
		if (max)
		{
			_moveAbove(_mods[(int)row], _mods[0]);
			// don't change the scroll position
			lstModsRefresh(_lstMods.getScroll());
		}
		else
		{
			// calculate target scroll pos
			int curScrollPos = (int)_lstMods.getScroll();
			int targetScrollPos = 0;
			for (uint i = 0; i < row - 1; ++i)
			{
				targetScrollPos += _lstMods.getNumTextLines(i);
			}
			if (curScrollPos < targetScrollPos)
			{
				int ydiff = _lstMods.getTextHeight(row - 1);
				SDL_WarpMouseGlobal(action.getLeftBlackBand() + action.getXMouse(),
					 action.getTopBlackBand() + action.getYMouse() - (ushort)(ydiff * action.getYScale()));
			}
			else
			{
				int ydiff = _lstMods.getRowY(row) - _lstMods.getY();
				SDL_WarpMouseGlobal(action.getLeftBlackBand() + action.getXMouse(),
					 action.getTopBlackBand() + action.getYMouse() - (ushort)(ydiff * action.getYScale()));
				_lstMods.scrollTo((uint)targetScrollPos);
			}

			_moveAbove(_mods[(int)row], _mods[(int)(row - 1)]);
			lstModsRefresh(_lstMods.getScroll());
		}
		Options.reload = true;
	}

	void moveModDown(Action action, uint row, bool max = false)
	{
		if (max)
		{
			_moveBelow(_mods[(int)row], _mods.Last());
			// don't change the scroll position
			lstModsRefresh(_lstMods.getScroll());
		}
		else
		{
			// calculate target scroll pos
			int curScrollPos = (int)_lstMods.getScroll();
			int targetScrollPos = 0;
			for (uint i = 0; i <= row + 1; ++i)
			{
				if (i == row)
				{
					// don't count the current row -- it will be moved down
					continue;
				}
				targetScrollPos += _lstMods.getNumTextLines(i);
			}
			if (curScrollPos + (int)_lstMods.getVisibleRows() > targetScrollPos)
			{
				int ydiff = _lstMods.getTextHeight(row + 1);
				SDL_WarpMouseGlobal(action.getLeftBlackBand() + action.getXMouse(),
					 action.getTopBlackBand() + action.getYMouse() + (ushort)(ydiff * action.getYScale()));
			}
			else
			{
				int ydiff = _lstMods.getY() + _lstMods.getHeight() - (_lstMods.getRowY(row) + _lstMods.getTextHeight(row));
				SDL_WarpMouseGlobal(action.getLeftBlackBand() + action.getXMouse(),
					 action.getTopBlackBand() + action.getYMouse() + (ushort)(ydiff * action.getYScale()));
				_lstMods.scrollTo((uint)(targetScrollPos - _lstMods.getVisibleRows() + 1));
			}

			_moveBelow(_mods[(int)row], _mods[(int)(row + 1)]);
			lstModsRefresh(_lstMods.getScroll());
		}
		Options.reload = true;
	}

	void lstModsRefresh(uint scrollLoc)
	{
		_lstMods.clearList();
		_mods.Clear();

		// only show mods that work with the current master
		foreach (var i in Options.mods)
		{
			ModInfo modInfo = Options.getModInfo(i.Key);
			if (modInfo.isMaster() || !modInfo.canActivate(_curMasterId))
			{
				continue;
			}

			string modName = modInfo.getName();
			_lstMods.addRow(3, modName, string.Empty, (i.Value ? tr("STR_YES") : tr("STR_NO")));
			_mods.Add(i);
		}

		_lstMods.scrollTo(scrollLoc);
	}

	static void _moveAbove(KeyValuePair<string, bool> srcMod, KeyValuePair<string, bool> destMod)
	{
		var mods = Options.mods;

		// insert copy of srcMod above destMod
		for (var i = 0; i < mods.Count; i++)
		{
			if (destMod.Key == mods[i].Key)
			{
				mods.Insert(i, srcMod);
				break;
			}
		}

		// remove old copy of srcMod in separate loop since the insert above invalidated the iterator
		for (var i = mods.Count - 1; i >= 0; i--)
		{
			if (srcMod.Key == mods[i].Key)
			{
				mods.RemoveAt(i /* - 1 */);
				break;
			}
		}
	}

	static void _moveBelow(KeyValuePair<string, bool> srcMod, KeyValuePair<string, bool> destMod)
	{
		var mods = Options.mods;

		// insert copy of srcMod below destMod
		for (var i = mods.Count - 1; i >= 0; i--)
		{
			if (destMod.Key == mods[i].Key)
			{
				mods.Insert(i, srcMod);
				break;
			}
		}

		// remove old copy of srcMod in separate loop since the insert above invalidated the iterator
		for (var i = 0; i < mods.Count; i++)
		{
			if (srcMod.Key == mods[i].Key)
			{
				mods.RemoveAt(i);
				break;
			}
		}
	}

	string makeTooltip(ModInfo modInfo) =>
		tr("STR_MODS_TOOLTIP").arg(modInfo.getVersion()).arg(modInfo.getAuthor()).arg(modInfo.getDescription());

	internal void toggleMod()
	{
		KeyValuePair<string, bool> mod = _mods[(int)_lstMods.getSelectedRow()];

		var mods = Options.mods;
		for (var i = 0; i < mods.Count; i++)
		{
			if (mod.Key != mods[i].Key)
			{
				continue;
			}

			mod = KeyValuePair.Create(mod.Key, !mod.Value);
			mods[i] = KeyValuePair.Create(mods[i].Key, mod.Value);
			_lstMods.setCellText(_lstMods.getSelectedRow(), 2, (mod.Value ? tr("STR_YES") : tr("STR_NO")));

			break;
		}
		Options.reload = true;
	}

	internal void revertMasterMod() =>
		_cbxMasters.setSelected(_curMasterIdx);
}
