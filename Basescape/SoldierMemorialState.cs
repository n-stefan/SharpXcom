﻿/*
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

namespace SharpXcom.Basescape;

/**
 * Screen that shows all the soldiers
 * that have died throughout the game.
 */
internal class SoldierMemorialState : State
{
	Window _window;
	TextButton _btnOk, _btnStatistics;
	Text _txtTitle, _txtName, _txtRank, _txtDate, _txtRecruited, _txtLost;
	TextList _lstSoldiers;

	/**
	 * Initializes all the elements in the Soldier Memorial screen.
	 * @param game Pointer to the core game.
	 */
	internal SoldierMemorialState()
	{
		// Create objects
		_window = new Window(this, 320, 200, 0, 0);
		_btnOk = new TextButton(148, 16, 164, 176);
		_btnStatistics = new TextButton(148, 16, 8, 176);
		_txtTitle = new Text(310, 17, 5, 8);
		_txtName = new Text(114, 9, 16, 36);
		_txtRank = new Text(102, 9, 130, 36);
		_txtDate = new Text(90, 9, 218, 36);
		_txtRecruited = new Text(150, 9, 16, 24);
		_txtLost = new Text(150, 9, 160, 24);
		_lstSoldiers = new TextList(288, 120, 8, 44);

		// Set palette
		setInterface("soldierMemorial");

		add(_window, "window", "soldierMemorial");
		add(_btnOk, "button", "soldierMemorial");
		add(_btnStatistics, "button", "soldierMemorial");
		add(_txtTitle, "text", "soldierMemorial");
		add(_txtName, "text", "soldierMemorial");
		add(_txtRank, "text", "soldierMemorial");
		add(_txtDate, "text", "soldierMemorial");
		add(_txtRecruited, "text", "soldierMemorial");
		add(_txtLost, "text", "soldierMemorial");
		add(_lstSoldiers, "list", "soldierMemorial");

		centerAllSurfaces();

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK02.SCR"));

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

		_btnStatistics.setText(tr("STR_STATISTICS"));
		_btnStatistics.onMouseClick(btnStatisticsClick);

		_txtTitle.setBig();
		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setText(tr("STR_MEMORIAL"));

		_txtName.setText(tr("STR_NAME_UC"));

		_txtRank.setText(tr("STR_RANK"));

		_txtDate.setText(tr("STR_DATE_UC"));

		var deadSoldiers = _game.getSavedGame().getDeadSoldiers();
		int lost = deadSoldiers.Count;
		int recruited = lost;
		foreach (var i in _game.getSavedGame().getBases())
		{
			recruited += i.getTotalSoldiers();
		}

		_txtRecruited.setText(tr("STR_SOLDIERS_RECRUITED_UC").arg(recruited));

		_txtLost.setText(tr("STR_SOLDIERS_LOST_UC").arg(lost));

		_lstSoldiers.setColumns(5, 114, 88, 30, 25, 35);
		_lstSoldiers.setSelectable(true);
		_lstSoldiers.setBackground(_window);
		_lstSoldiers.setMargin(8);
		_lstSoldiers.onMouseClick(lstSoldiersClick);

		for (var i = deadSoldiers.Count - 1; i >= 0; i--)
		{
			SoldierDeath death = deadSoldiers[i].getDeath();

			string saveDay = death.getTime().getDayString(_game.getLanguage());
			string saveMonth = tr(death.getTime().getMonthString());
			string saveYear = death.getTime().getYear().ToString();
			_lstSoldiers.addRow(5, deadSoldiers[i].getName(), tr(deadSoldiers[i].getRankString()), saveDay, saveMonth, saveYear);
		}
	}

	/**
	 *
	 */
	~SoldierMemorialState() { }

	/**
	 * Returns to the previous screen.
	 * @param action Pointer to an action.
	 */
	void btnOkClick(Action _)
	{
		_game.popState();
		_game.getMod().playMusic("GMGEO");
	}

	/**
	* Shows the Statistics screen.
	* @param action Pointer to an action.
	*/
	void btnStatisticsClick(Action _) =>
		_game.pushState(new StatisticsState());

	/**
	 * Shows the selected soldier's info.
	 * @param action Pointer to an action.
	 */
	void lstSoldiersClick(Action _) =>
		_game.pushState(new SoldierInfoState(null, _lstSoldiers.getSelectedRow()));
}
