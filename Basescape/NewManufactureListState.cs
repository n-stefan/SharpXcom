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
 * Screen which list possible productions.
 */
internal class NewManufactureListState : State
{
	Base _base;
	Window _window;
	TextButton _btnOk;
	Text _txtTitle, _txtItem, _txtCategory;
	TextList _lstManufacture;
	ComboBox _cbxCategory;
	List<RuleManufacture> _possibleProductions;
	List<string> _displayedStrings;
	List<string> _catStrings;

	/**
	 * Initializes all the elements in the productions list screen.
	 * @param game Pointer to the core game.
	 * @param base Pointer to the base to get info from.
	 */
	internal NewManufactureListState(Base @base)
	{
		_base = @base;

		_screen = false;

		_window = new Window(this, 320, 156, 0, 22, WindowPopup.POPUP_BOTH);
		_btnOk = new TextButton(304, 16, 8, 154);
		_txtTitle = new Text(320, 17, 0, 30);
		_txtItem = new Text(156, 9, 10, 62);
		_txtCategory = new Text(130, 9, 166, 62);
		_lstManufacture = new TextList(288, 80, 8, 70);
		_cbxCategory = new ComboBox(this, 146, 16, 166, 46);

		// Set palette
		setInterface("selectNewManufacture");

		add(_window, "window", "selectNewManufacture");
		add(_btnOk, "button", "selectNewManufacture");
		add(_txtTitle, "text", "selectNewManufacture");
		add(_txtItem, "text", "selectNewManufacture");
		add(_txtCategory, "text", "selectNewManufacture");
		add(_lstManufacture, "list", "selectNewManufacture");
		add(_cbxCategory, "catBox", "selectNewManufacture");

		centerAllSurfaces();

		_window.setBackground(_game.getMod().getSurface("BACK17.SCR"));

		_txtTitle.setText(tr("STR_PRODUCTION_ITEMS"));
		_txtTitle.setBig();
		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);

		_txtItem.setText(tr("STR_ITEM"));

		_txtCategory.setText(tr("STR_CATEGORY"));

		_lstManufacture.setColumns(2, 156, 130);
		_lstManufacture.setSelectable(true);
		_lstManufacture.setBackground(_window);
		_lstManufacture.setMargin(2);
		_lstManufacture.onMouseClick(lstProdClick);

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

		_possibleProductions.Clear();
		_game.getSavedGame().getAvailableProductions(_possibleProductions, _game.getMod(), _base);
		_catStrings.Add("STR_ALL_ITEMS");

		foreach (var it in _possibleProductions)
		{
			bool addCategory = true;
			for (int x = 0; x < _catStrings.Count; ++x)
			{
				if (it.getCategory() == _catStrings[x])
				{
					addCategory = false;
					break;
				}
			}
			if (addCategory)
			{
				_catStrings.Add(it.getCategory());
			}
		}

		_cbxCategory.setOptions(_catStrings, true);
		_cbxCategory.onChange(cbxCategoryChange);
	}

	/**
	 * Opens the Production settings screen.
	 * @param action A pointer to an Action.
	 */
	void lstProdClick(Action _)
	{
		RuleManufacture rule = null;
		foreach (var it in _possibleProductions)
		{
			if (it.getName() == _displayedStrings[(int)_lstManufacture.getSelectedRow()])
			{
				rule = it;
				break;
			}
		}
		_game.pushState(new ManufactureStartState(_base, rule));
	}

	/**
	 * Returns to the previous screen.
	 * @param action A pointer to an Action.
	 */
	void btnOkClick(Action _) =>
		_game.popState();

	/**
	 * Updates the production list to match the category filter
	 */
	void cbxCategoryChange(Action _) =>
		fillProductionList();

	/**
	 * Fills the list of possible productions.
	 */
	void fillProductionList()
	{
		_lstManufacture.clearList();
		_possibleProductions.Clear();
		_game.getSavedGame().getAvailableProductions(_possibleProductions, _game.getMod(), _base);
		_displayedStrings.Clear();

		foreach (var it in _possibleProductions)
		{
			if ((it.getCategory() == _catStrings[(int)_cbxCategory.getSelected()]) || (_catStrings[(int)_cbxCategory.getSelected()] == "STR_ALL_ITEMS"))
			{
				_lstManufacture.addRow(2, tr(it.getName()), tr(it.getCategory()));
				_displayedStrings.Add(it.getName());
			}
		}
	}

	/**
	 * Initializes state (fills list of possible productions).
	 */
	protected override void init()
	{
		base.init();
		fillProductionList();
	}
}
