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
 * Promotions screen that displays new soldier ranks.
 */
internal class PromotionsState : State
{
    Window _window;
    TextButton _btnOk;
    Text _txtTitle, _txtName, _txtRank, _txtBase;
    TextList _lstSoldiers;

    /**
	 * Initializes all the elements in the Promotions screen.
	 * @param game Pointer to the core game.
	 */
    internal PromotionsState()
	{
		// Create objects
		_window = new Window(this, 320, 200, 0, 0);
		_btnOk = new TextButton(288, 16, 16, 176);
		_txtTitle = new Text(300, 17, 10, 8);
		_txtName = new Text(114, 9, 16, 32);
		_txtRank = new Text(90, 9, 130, 32);
		_txtBase = new Text(80, 9, 220, 32);
		_lstSoldiers = new TextList(288, 128, 8, 40);

		// Set palette
		setInterface("promotions");

		add(_window, "window", "promotions");
		add(_btnOk, "button", "promotions");
		add(_txtTitle, "heading", "promotions");
		add(_txtName, "text", "promotions");
		add(_txtRank, "text", "promotions");
		add(_txtBase, "text", "promotions");
		add(_lstSoldiers, "list", "promotions");

		centerAllSurfaces();

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

		_txtTitle.setText(tr("STR_PROMOTIONS"));
		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setBig();

		_txtName.setText(tr("STR_NAME"));

		_txtRank.setText(tr("STR_NEW_RANK"));

		_txtBase.setText(tr("STR_BASE"));

		_lstSoldiers.setColumns(3, 114, 90, 84);
		_lstSoldiers.setSelectable(true);
		_lstSoldiers.setBackground(_window);
		_lstSoldiers.setMargin(8);

		foreach (var i in _game.getSavedGame().getBases())
		{
			foreach (var j in i.getSoldiers())
			{
				if (j.isPromoted())
				{
					_lstSoldiers.addRow(3, j.getName(), tr(j.getRankString()), i.getName());
				}
			}
		}
	}

	/**
	 *
	 */
	~PromotionsState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();
}
