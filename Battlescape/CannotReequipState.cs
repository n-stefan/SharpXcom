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
 * Screen shown when there's not enough equipment
 * to re-equip a craft after a mission.
 */
internal class CannotReequipState : State
{
    Window _window;
    TextButton _btnOk;
    Text _txtTitle, _txtItem, _txtQuantity, _txtCraft;
    TextList _lstItems;

    /**
	 * Initializes all the elements in the Cannot Reequip screen.
	 * @param game Pointer to the core game.
	 * @param missingItems List of items still needed for reequip.
	 */
    internal CannotReequipState(List<ReequipStat> missingItems)
	{
		// Create objects
		_window = new Window(this, 320, 200, 0, 0);
		_btnOk = new TextButton(120, 18, 100, 174);
		_txtTitle = new Text(220, 32, 50, 8);
		_txtItem = new Text(142, 9, 10, 50);
		_txtQuantity = new Text(88, 9, 152, 50);
		_txtCraft = new Text(74, 9, 218, 50);
		_lstItems = new TextList(288, 112, 8, 58);

		// Set palette
		setInterface("cannotReequip");

		add(_window, "window", "cannotReequip");
		add(_btnOk, "button", "cannotReequip");
		add(_txtTitle, "heading", "cannotReequip");
		add(_txtItem, "text", "cannotReequip");
		add(_txtQuantity, "text", "cannotReequip");
		add(_txtCraft, "text", "cannotReequip");
		add(_lstItems, "list", "cannotReequip");

		centerAllSurfaces();

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

		_txtTitle.setText(tr("STR_NOT_ENOUGH_EQUIPMENT_TO_FULLY_RE_EQUIP_SQUAD"));
		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setBig();
		_txtTitle.setWordWrap(true);

		_txtItem.setText(tr("STR_ITEM"));

		_txtQuantity.setText(tr("STR_QUANTITY_UC"));

		_txtCraft.setText(tr("STR_CRAFT"));

		_lstItems.setColumns(3, 162, 46, 80);
		_lstItems.setSelectable(true);
		_lstItems.setBackground(_window);
		_lstItems.setMargin(2);

		foreach (var i in missingItems)
		{
			string ss = i.qty.ToString();
			_lstItems.addRow(3, tr(i.item), ss, i.craft);
		}
	}

	/**
	 *
	 */
	~CannotReequipState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();
}
