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
 * Select Armor window that allows changing
 * of the armor equipped on a soldier.
 */
internal class SoldierArmorState : State
{
    Base _base;
    uint _soldier;
    Window _window;
    TextButton _btnCancel;
    Text _txtTitle, _txtType, _txtQuantity;
    TextList _lstArmor;
    List<Armor> _armors;

    /**
     * Initializes all the elements in the Soldier Armor window.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param soldier ID of the selected soldier.
     */
    internal SoldierArmorState(Base @base, uint soldier)
    {
        _base = @base;
        _soldier = soldier;

        _screen = false;

        // Create objects
        _window = new Window(this, 192, 160, 64, 20, WindowPopup.POPUP_BOTH);
        _btnCancel = new TextButton(140, 16, 90, 156);
        _txtTitle = new Text(182, 16, 69, 28);
        _txtType = new Text(90, 9, 80, 52);
        _txtQuantity = new Text(70, 9, 190, 52);
        _lstArmor = new TextList(160, 80, 73, 68);

        // Set palette
        setInterface("soldierArmor");

        add(_window, "window", "soldierArmor");
        add(_btnCancel, "button", "soldierArmor");
        add(_txtTitle, "text", "soldierArmor");
        add(_txtType, "text", "soldierArmor");
        add(_txtQuantity, "text", "soldierArmor");
        add(_lstArmor, "list", "soldierArmor");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK14.SCR"));

        _btnCancel.setText(tr("STR_CANCEL_UC"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        Soldier s = _base.getSoldiers()[(int)_soldier];
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_SELECT_ARMOR_FOR_SOLDIER").arg(s.getName()));

        _txtType.setText(tr("STR_TYPE"));

        _txtQuantity.setText(tr("STR_QUANTITY_UC"));

        _lstArmor.setColumns(2, 132, 21);
        _lstArmor.setSelectable(true);
        _lstArmor.setBackground(_window);
        _lstArmor.setMargin(8);

        List<string> armors = _game.getMod().getArmorsList();
        foreach (var i in armors)
        {
            Armor a = _game.getMod().getArmor(i);
            if (a.getUnits().Any() &&
                !a.getUnits().Contains(s.getRules().getType()))
                continue;
            if (_base.getStorageItems().getItem(a.getStoreItem()) > 0)
            {
                _armors.Add(a);
                string ss;
                if (_game.getSavedGame().getMonthsPassed() > -1)
                {
                    ss = _base.getStorageItems().getItem(a.getStoreItem()).ToString();
                }
                else
                {
                    ss = "-";
                }
                _lstArmor.addRow(2, tr(a.getType()), ss);
            }
            else if (a.getStoreItem() == Armor.NONE)
            {
                _armors.Add(a);
                _lstArmor.addRow(1, tr(a.getType()));
            }
        }
        _lstArmor.onMouseClick(lstArmorClick);
    }

    /**
     *
     */
    ~SoldierArmorState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Engine.Action _) =>
        _game.popState();

    /**
     * Equips the armor on the soldier and returns to the previous screen.
     * @param action Pointer to an action.
     */
    void lstArmorClick(Engine.Action _)
    {
        Soldier soldier = _base.getSoldiers()[(int)_soldier];
        if (_game.getSavedGame().getMonthsPassed() != -1)
        {
            if (soldier.getArmor().getStoreItem() != Armor.NONE)
            {
                _base.getStorageItems().addItem(soldier.getArmor().getStoreItem());
            }
            if (_armors[(int)_lstArmor.getSelectedRow()].getStoreItem() != Armor.NONE)
            {
                _base.getStorageItems().removeItem(_armors[(int)_lstArmor.getSelectedRow()].getStoreItem());
            }
        }
        soldier.setArmor(_armors[(int)_lstArmor.getSelectedRow()]);
        SavedGame _save;
        _save = _game.getSavedGame();
        _save.setLastSelectedArmor(_armors[(int)_lstArmor.getSelectedRow()].getType());

        _game.popState();
    }
}
