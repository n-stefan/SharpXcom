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
 * Equip Craft screen that lets the player
 * manage all the crafts in a base.
 */
internal class CraftsState : State
{
    Base _base;
    Window _window;
    TextButton _btnOk;
    Text _txtTitle, _txtBase, _txtName, _txtStatus, _txtWeapon, _txtCrew, _txtHwp;
    TextList _lstCrafts;

    /**
     * Initializes all the elements in the Equip Craft screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     */
    internal CraftsState(Base @base)
    {
        _base = @base;

        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        _btnOk = new TextButton(288, 16, 16, 176);
        _txtTitle = new Text(298, 17, 16, 8);
        _txtBase = new Text(298, 17, 16, 24);
        _txtName = new Text(94, 9, 16, 40);
        _txtStatus = new Text(50, 9, 110, 40);
        _txtWeapon = new Text(50, 17, 160, 40);
        _txtCrew = new Text(58, 9, 210, 40);
        _txtHwp = new Text(46, 9, 268, 40);
        _lstCrafts = new TextList(288, 118, 8, 58);

        // Set palette
        setInterface("craftSelect");

        add(_window, "window", "craftSelect");
        add(_btnOk, "button", "craftSelect");
        add(_txtTitle, "text", "craftSelect");
        add(_txtBase, "text", "craftSelect");
        add(_txtName, "text", "craftSelect");
        add(_txtStatus, "text", "craftSelect");
        add(_txtWeapon, "text", "craftSelect");
        add(_txtCrew, "text", "craftSelect");
        add(_txtHwp, "text", "craftSelect");
        add(_lstCrafts, "list", "craftSelect");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK14.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setText(tr("STR_INTERCEPTION_CRAFT"));

        _txtBase.setBig();
        _txtBase.setText(tr("STR_BASE_").arg(_base.getName()));

        _txtName.setText(tr("STR_NAME_UC"));

        _txtStatus.setText(tr("STR_STATUS"));

        _txtWeapon.setText(tr("STR_WEAPON_SYSTEMS"));
        _txtWeapon.setWordWrap(true);

        _txtCrew.setText(tr("STR_CREW"));

        _txtHwp.setText(tr("STR_HWPS"));
        _lstCrafts.setColumns(5, 94, 68, 44, 46, 28);
        _lstCrafts.setSelectable(true);
        _lstCrafts.setBackground(_window);
        _lstCrafts.setMargin(8);
        _lstCrafts.onMouseClick(lstCraftsClick);
    }

    /**
     *
     */
    ~CraftsState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        _game.popState();

        if (_game.getSavedGame().getMonthsPassed() > -1 && Options.storageLimitsEnforced && _base.storesOverfull())
        {
            _game.pushState(new SellState(_base));
            _game.pushState(new ErrorMessageState(tr("STR_STORAGE_EXCEEDED").arg(_base.getName()), _palette, (byte)_game.getMod().getInterface("craftSelect").getElement("errorMessage").color, "BACK01.SCR", _game.getMod().getInterface("craftSelect").getElement("errorPalette").color));
        }
    }

    /**
     * Shows the selected craft's info.
     * @param action Pointer to an action.
     */
    void lstCraftsClick(Action _)
    {
        if (_base.getCrafts()[(int)_lstCrafts.getSelectedRow()].getStatus() != "STR_OUT")
        {
            _game.pushState(new CraftInfoState(_base, _lstCrafts.getSelectedRow()));
        }
    }

    /**
     * The soldier names can change
     * after going into other screens.
     */
    protected override void init()
    {
	    base.init();
	    _lstCrafts.clearList();
	    foreach (var i in _base.getCrafts())
	    {
		    string ss = $"{i.getNumWeapons()}/{i.getRules().getWeapons()}";
		    string ss2 = i.getNumSoldiers().ToString();
		    string ss3 = i.getNumVehicles().ToString();
		    _lstCrafts.addRow(5, i.getName(_game.getLanguage()), tr(i.getStatus()), ss, ss2, ss3);
	    }
    }
}
