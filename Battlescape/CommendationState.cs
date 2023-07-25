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
 * Medals screen that displays new soldier medals.
 */
internal class CommendationState : State
{
    Window _window;
    TextButton _btnOk;
    Text _txtTitle;
    TextList _lstSoldiers;

    /**
	 * Initializes all the elements in the Medals screen.
	 * @param soldiersMedalled List of soldiers with medals.
	 */
    internal CommendationState(List<Soldier> soldiersMedalled)
	{
		// Create objects
		_window = new Window(this, 320, 200, 0, 0);
		_btnOk = new TextButton(288, 16, 16, 176);
		_txtTitle = new Text(300, 16, 10, 8);
		_lstSoldiers = new TextList(288, 128, 8, 32);

		// Set palette
		setInterface("commendations");

		add(_window, "window", "commendations");
		add(_btnOk, "button", "commendations");
		add(_txtTitle, "heading", "commendations");
		add(_lstSoldiers, "list", "commendations");

		centerAllSurfaces();

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

		_txtTitle.setText(tr("STR_MEDALS"));
		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setBig();

		_lstSoldiers.setColumns(2, 204, 84);
		_lstSoldiers.setSelectable(true);
		_lstSoldiers.setBackground(_window);
		_lstSoldiers.setMargin(8);

		uint row = 0;
		uint titleRow = 0;
		Dictionary<string, RuleCommendations> commendationsList = _game.getMod().getCommendationsList();
		bool modularCommendation;
		string noun;
		bool titleChosen = true;

        var commList = commendationsList.GetEnumerator();
		commList.MoveNext();
        while (commList.Current.Key != null)
		{
			modularCommendation = false;
			noun = "noNoun";
			if (titleChosen)
			{
				_lstSoldiers.addRow(2, string.Empty, string.Empty); // Blank row, will be filled in later
				row++;
			}
			titleChosen = false;
			titleRow = row - 1;

			foreach (var s in soldiersMedalled)
			{
				foreach (var soldierComm in s.getDiary().getSoldierCommendations())
				{
					if (soldierComm.getType() == commList.Current.Key && soldierComm.isNew() && noun == "noNoun")
					{
						soldierComm.makeOld();
						row++;

						if (soldierComm.getNoun() != "noNoun")
						{
							noun = soldierComm.getNoun();
							modularCommendation = true;
						}

						// Soldier name
						string wssName = $"   {s.getName()}";
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
						_lstSoldiers.addRow(2, wssName, tr(soldierComm.getDecorationLevelName(skipCounter)));
						break;
					}
				}
			}
			if (titleRow != row - 1)
			{
				// Medal name
				if (modularCommendation)
				{
					_lstSoldiers.setCellText(titleRow, 0, tr(commList.Current.Key).arg(tr(noun)));
				}
				else
				{
					_lstSoldiers.setCellText(titleRow, 0, tr(commList.Current.Key));
				}
				_lstSoldiers.setRowColor(titleRow, _lstSoldiers.getSecondaryColor());
				titleChosen = true;
			}
			if (noun == "noNoun")
			{
				commList.MoveNext();
			}
		}
	}

	/**
	 *
	 */
	~CommendationState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();
}
