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

namespace SharpXcom.Basescape;

/**
 * Screen which displays needed elements to start productions (items/required workshop state/cost to build a unit, ...).
 */
internal class ManufactureStartState : State
{
	Base _base;
	RuleManufacture _item;
	Window _window;
	TextButton _btnCancel, _btnStart;
	Text _txtTitle, _txtManHour, _txtCost, _txtWorkSpace, _txtRequiredItemsTitle, _txtItemNameColumn, _txtUnitRequiredColumn, _txtUnitAvailableColumn;
	TextList _lstRequiredItems;

	/**
	 * Initializes all the elements in the productions start screen.
	 * @param game Pointer to the core game.
	 * @param base Pointer to the base to get info from.
	 * @param item The RuleManufacture to produce.
	 */
	internal ManufactureStartState(Base @base, RuleManufacture item)
	{
		_base = @base;
		_item = item;

		_screen = false;

		_window = new Window(this, 320, 160, 0, 20);
		_btnCancel = new TextButton(136, 16, 16, 155);
		_txtTitle = new Text(320, 17, 0, 30);
		_txtManHour = new Text(290, 9, 16, 50);
		_txtCost = new Text(290, 9, 16, 60);
		_txtWorkSpace = new Text(290, 9, 16, 70);

		_txtRequiredItemsTitle = new Text(290, 9, 16, 84);
		_txtItemNameColumn = new Text(60, 16, 30, 92);
		_txtUnitRequiredColumn = new Text(60, 16, 155, 92);
		_txtUnitAvailableColumn = new Text(60, 16, 230, 92);
		_lstRequiredItems = new TextList(270, 40, 30, 108);

		_btnStart = new TextButton(136, 16, 168, 155);

		// Set palette
		setInterface("allocateManufacture");

		add(_window, "window", "allocateManufacture");
		add(_txtTitle, "text", "allocateManufacture");
		add(_txtManHour, "text", "allocateManufacture");
		add(_txtCost, "text", "allocateManufacture");
		add(_txtWorkSpace, "text", "allocateManufacture");
		add(_btnCancel, "button", "allocateManufacture");

		add(_txtRequiredItemsTitle, "text", "allocateManufacture");
		add(_txtItemNameColumn, "text", "allocateManufacture");
		add(_txtUnitRequiredColumn, "text", "allocateManufacture");
		add(_txtUnitAvailableColumn, "text", "allocateManufacture");
		add(_lstRequiredItems, "list", "allocateManufacture");

		add(_btnStart, "button", "allocateManufacture");

		centerAllSurfaces();

		_window.setBackground(_game.getMod().getSurface("BACK17.SCR"));

		_txtTitle.setText(tr(_item.getName()));
		_txtTitle.setBig();
		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);

		_txtManHour.setText(tr("STR_ENGINEER_HOURS_TO_PRODUCE_ONE_UNIT").arg(_item.getManufactureTime()));

		_txtCost.setText(tr("STR_COST_PER_UNIT_").arg(Unicode.formatFunding(_item.getManufactureCost())));

		_txtWorkSpace.setText(tr("STR_WORK_SPACE_REQUIRED").arg(_item.getRequiredSpace()));

		_btnCancel.setText(tr("STR_CANCEL_UC"));
		_btnCancel.onMouseClick(btnCancelClick);
		_btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

		Dictionary<string, int> requiredItems = _item.getRequiredItems();
		int availableWorkSpace = _base.getFreeWorkshops();
		bool productionPossible = _item.haveEnoughMoneyForOneMoreUnit(_game.getSavedGame().getFunds());
		productionPossible &= (availableWorkSpace > 0);

		_txtRequiredItemsTitle.setText(tr("STR_SPECIAL_MATERIALS_REQUIRED"));
		_txtRequiredItemsTitle.setAlign(TextHAlign.ALIGN_CENTER);

		_txtItemNameColumn.setText(tr("STR_ITEM_REQUIRED"));
		_txtItemNameColumn.setWordWrap(true);

		_txtUnitRequiredColumn.setText(tr("STR_UNITS_REQUIRED"));
		_txtUnitRequiredColumn.setWordWrap(true);

		_txtUnitAvailableColumn.setText(tr("STR_UNITS_AVAILABLE"));
		_txtUnitAvailableColumn.setWordWrap(true);

		_lstRequiredItems.setColumns(3, 140, 75, 55);
		_lstRequiredItems.setBackground(_window);

		uint row = 0;
		foreach (var iter in requiredItems)
		{
			string s2 = null;
			string s1 = iter.Value.ToString();
			if (_game.getMod().getItem(iter.Key) != null)
			{
				s2 = @base.getStorageItems().getItem(iter.Key).ToString();
				productionPossible &= (@base.getStorageItems().getItem(iter.Key) >= iter.Value);
			}
			else if (_game.getMod().getCraft(iter.Key) != null)
			{
				s2 = @base.getCraftCount(iter.Key).ToString();
				productionPossible &= (@base.getCraftCount(iter.Key) >= iter.Value);
			}
			_lstRequiredItems.addRow(3, tr(iter.Key), s1, s2);
			_lstRequiredItems.setCellColor(row, 1, _lstRequiredItems.getSecondaryColor());
			_lstRequiredItems.setCellColor(row, 2, _lstRequiredItems.getSecondaryColor());
			row++;
		}
		_txtRequiredItemsTitle.setVisible(requiredItems.Any());
		_txtItemNameColumn.setVisible(requiredItems.Any());
		_txtUnitRequiredColumn.setVisible(requiredItems.Any());
		_txtUnitAvailableColumn.setVisible(requiredItems.Any());
		_lstRequiredItems.setVisible(requiredItems.Any());

		_btnStart.setText(tr("STR_START_PRODUCTION"));
		_btnStart.onMouseClick(btnStartClick);
		_btnStart.onKeyboardPress(btnStartClick, Options.keyOk);
		_btnStart.setVisible(productionPossible);
	}

	/**
	 * Returns to previous screen.
	 * @param action A pointer to an Action.
	 */
	void btnCancelClick(Action _) =>
		_game.popState();

	/**
	 * Go to the Production settings screen.
	 * @param action A pointer to an Action.
	 */
	void btnStartClick(Action _)
	{
		if (_item.getCategory() == "STR_CRAFT" && _base.getAvailableHangars() - _base.getUsedHangars() <= 0)
		{
			_game.pushState(new ErrorMessageState(tr("STR_NO_FREE_HANGARS_FOR_CRAFT_PRODUCTION"), _palette, (byte)_game.getMod().getInterface("basescape").getElement("errorMessage").color, "BACK17.SCR", _game.getMod().getInterface("basescape").getElement("errorPalette").color));
		}
		else if (_item.getRequiredSpace() > _base.getFreeWorkshops())
		{
			_game.pushState(new ErrorMessageState(tr("STR_NOT_ENOUGH_WORK_SPACE"), _palette, (byte)_game.getMod().getInterface("basescape").getElement("errorMessage").color, "BACK17.SCR", _game.getMod().getInterface("basescape").getElement("errorPalette").color));
		}
		else
		{
			_game.pushState(new ManufactureInfoState(_base, _item));
		}
	}
}
