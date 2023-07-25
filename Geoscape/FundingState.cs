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

namespace SharpXcom.Geoscape;

/**
 * Funding screen accessible from the Geoscape
 * that shows all the countries' funding.
 */
internal class FundingState : State
{
    Window _window;
    TextButton _btnOk;
    Text _txtTitle, _txtCountry, _txtFunding, _txtChange;
    TextList _lstCountries;

    /**
	 * Initializes all the elements in the Funding screen.
	 * @param game Pointer to the core game.
	 */
    internal FundingState()
	{
		_screen = false;

		// Create objects
		_window = new Window(this, 320, 200, 0, 0, WindowPopup.POPUP_BOTH);
		_btnOk = new TextButton(50, 12, 135, 180);
		_txtTitle = new Text(320, 17, 0, 8);
		_txtCountry = new Text(100, 9, 32, 30);
		_txtFunding = new Text(100, 9, 140, 30);
		_txtChange = new Text(72, 9, 240, 30);
		_lstCountries = new TextList(260, 136, 32, 40);

		// Set palette
		setInterface("fundingWindow");

		add(_window, "window", "fundingWindow");
		add(_btnOk, "button", "fundingWindow");
		add(_txtTitle, "text1", "fundingWindow");
		add(_txtCountry, "text2", "fundingWindow");
		add(_txtFunding, "text2", "fundingWindow");
		add(_txtChange, "text2", "fundingWindow");
		add(_lstCountries, "list", "fundingWindow");

		centerAllSurfaces();

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK13.SCR"));

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyGeoFunding);

		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setBig();
		_txtTitle.setText(tr("STR_INTERNATIONAL_RELATIONS"));

		_txtCountry.setText(tr("STR_COUNTRY"));

		_txtFunding.setText(tr("STR_FUNDING"));

		_txtChange.setText(tr("STR_CHANGE"));

		_lstCountries.setColumns(3, 108, 100, 52);
		_lstCountries.setDot(true);
		foreach (var i in _game.getSavedGame().getCountries())
		{
			string ss = $"{Unicode.TOK_COLOR_FLIP}{Unicode.formatFunding(i.getFunding()[i.getFunding().Count - 1])}{Unicode.TOK_COLOR_FLIP}";
			string ss2;
			if (i.getFunding().Count > 1)
			{
				ss2 = Unicode.TOK_COLOR_FLIP.ToString();
				int change = i.getFunding().Last() - i.getFunding()[i.getFunding().Count - 2];
				if (change > 0)
					ss2 = $"{ss2}+";
				ss2 = $"{ss2}{Unicode.formatFunding(change)}";
				ss2 = $"{ss2}{Unicode.TOK_COLOR_FLIP}";
			}
			else
			{
				ss2 = Unicode.formatFunding(0);
			}
			_lstCountries.addRow(3, tr(i.getRules().getType()), ss, ss2);
		}
		_lstCountries.addRow(2, tr("STR_TOTAL_UC"), Unicode.formatFunding(_game.getSavedGame().getCountryFunding()));
		_lstCountries.setRowColor((uint)_game.getSavedGame().getCountries().Count, _txtCountry.getColor());
	}

	/**
	 *
	 */
	~FundingState()	{ }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();
}
