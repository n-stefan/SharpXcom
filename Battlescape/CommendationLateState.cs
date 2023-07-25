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
 * Medals screen that displays dead soldier medals.
 */
internal class CommendationLateState : State
{
    Window _window;
    TextButton _btnOk;
    Text _txtTitle;
    TextList _lstSoldiers;

    /**
	 * Initializes all the elements in the Medals screen.
	 * @param soldiersMedalled List of soldiers with medals.
	 */
    internal CommendationLateState(List<Soldier> soldiersMedalled)
	{
		// Create objects
		_window = new Window(this, 320, 200, 0, 0);
		_btnOk = new TextButton(288, 16, 16, 176);
		_txtTitle = new Text(300, 16, 10, 8);
		_lstSoldiers = new TextList(288, 128, 8, 32);

		// Set palette
		setInterface("commendationsLate");

		add(_window, "window", "commendationsLate");
		add(_btnOk, "button", "commendationsLate");
		add(_txtTitle, "text", "commendationsLate");
		add(_lstSoldiers, "list", "commendationsLate");

		centerAllSurfaces();

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK02.SCR"));

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

		_txtTitle.setBig();
		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setText(tr("STR_LOST_IN_SERVICE"));

		_lstSoldiers.setColumns(5, 51, 51, 51, 51, 84);
		_lstSoldiers.setSelectable(true);
		_lstSoldiers.setBackground(_window);
		_lstSoldiers.setMargin(8);
		_lstSoldiers.setFlooding(true);

		/***

												LOST IN SERVICE

		SOLDIER NAME, RANK: ___, SCORE: ___, KILLS: ___, NUMBER OF MISSIONS: ___, DAYS WOUNDED: ___, TIMES HIT: ___
		  COMMENDATION
		  COMMENDATION
		  COMMENDATION
		  CAUSE OF DEATH: KILLED BY ALIEN_RACE ALIEN_RANK, USING WEAPON

		***/

		Dictionary<string, RuleCommendations> commendationsList = _game.getMod().getCommendationsList();
		bool modularCommendation;
		string noun;

		// Loop over dead soldiers
		foreach (var s in soldiersMedalled)
		{
			// Establish some base information
			_lstSoldiers.addRow(5, s.getName(),
								   string.Empty,
								   tr(s.getRankString()),
								   string.Empty,
								   tr("STR_KILLS").arg(s.getDiary().getKillTotal()));

            // Loop over all commendations
            var commList = commendationsList.GetEnumerator();
            commList.MoveNext();
            while (commList.Current.Key != null)
			{
				string wssCommendation;
				modularCommendation = false;
				noun = "noNoun";

				// Loop over soldier's commendations
				foreach (var soldierComm in s.getDiary().getSoldierCommendations())
				{
					if (soldierComm.getType() == commList.Current.Key && soldierComm.isNew() && noun == "noNoun")
					{
						soldierComm.makeOld();

						if (soldierComm.getNoun() != "noNoun")
						{
							noun = soldierComm.getNoun();
							modularCommendation = true;
						}
						// Decoration level name
						int skipCounter = 0;
						int lastInt = -2;
						int thisInt = -1;
						int vectorIterator = 0;
                        var criteria = commList.Current.Value.getCriteria().First().Value;
                        for (var k = 0; k < criteria.Count; ++k)
						{
							if (vectorIterator == soldierComm.getDecorationLevelInt() + 1)
							{
								break;
							}
							thisInt = criteria[k];
                            if (k != criteria[0])
							{
								--k;
								lastInt = criteria[k];
								++k;
							}
							if (thisInt == lastInt)
							{
								skipCounter++;
							}
							vectorIterator++;
						}
						// Establish comms name
						// Medal name
						wssCommendation = "   ";
						if (modularCommendation)
						{
							wssCommendation = $"{wssCommendation}{tr(commList.Current.Key).arg(tr(noun))}";
						}
						else
						{
							wssCommendation = $"{wssCommendation}{tr(commList.Current.Key)}";
						}
						_lstSoldiers.addRow(5, wssCommendation, string.Empty, string.Empty, string.Empty, tr(soldierComm.getDecorationLevelName(skipCounter)));
						break;
					}
				} // END SOLDIER COMMS LOOP

				if (noun == "noNoun")
				{
					commList.MoveNext();
				}
			} // END COMMS LOOPS
		} // END SOLDIER LOOP
	}

	/**
	 *
	 */
	~CommendationLateState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();
}
