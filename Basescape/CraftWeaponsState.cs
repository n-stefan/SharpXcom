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
 * Select Armament window for
 * changing the weapon equipped on a craft.
 */
internal class CraftWeaponsState : State
{
    Base _base;
    uint _craft, _weapon;
    Window _window;
    TextButton _btnCancel;
    Text _txtTitle, _txtArmament, _txtQuantity, _txtAmmunition;
    TextList _lstWeapons;
    List<RuleCraftWeapon> _weapons;

    /**
     * Initializes all the elements in the Craft Weapons window.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param craft ID of the selected craft.
     * @param weapon ID of the selected weapon.
     */
    internal CraftWeaponsState(Base @base, uint craft, uint weapon)
    {
        _base = @base;
        _craft = craft;
        _weapon = weapon;

        _screen = false;

        // Create objects
        _window = new Window(this, 220, 160, 50, 20, WindowPopup.POPUP_BOTH);
        _btnCancel = new TextButton(140, 16, 90, 156);
        _txtTitle = new Text(208, 17, 56, 28);
        _txtArmament = new Text(76, 9, 66, 52);
        _txtQuantity = new Text(50, 9, 140, 52);
        _txtAmmunition = new Text(68, 17, 200, 44);
        _lstWeapons = new TextList(188, 80, 58, 68);

        // Set palette
        setInterface("craftWeapons");

        add(_window, "window", "craftWeapons");
        add(_btnCancel, "button", "craftWeapons");
        add(_txtTitle, "text", "craftWeapons");
        add(_txtArmament, "text", "craftWeapons");
        add(_txtQuantity, "text", "craftWeapons");
        add(_txtAmmunition, "text", "craftWeapons");
        add(_lstWeapons, "list", "craftWeapons");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK14.SCR"));

        _btnCancel.setText(tr("STR_CANCEL_UC"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_SELECT_ARMAMENT"));

        _txtArmament.setText(tr("STR_ARMAMENT"));

        _txtQuantity.setText(tr("STR_QUANTITY_UC"));

        _txtAmmunition.setText(tr("STR_AMMUNITION_AVAILABLE"));
        _txtAmmunition.setWordWrap(true);
        _txtAmmunition.setVerticalAlign(TextVAlign.ALIGN_BOTTOM);

        _lstWeapons.setColumns(3, 94, 50, 36);
        _lstWeapons.setSelectable(true);
        _lstWeapons.setBackground(_window);
        _lstWeapons.setMargin(8);

        _lstWeapons.addRow(1, tr("STR_NONE_UC"));
        _weapons.Add(null);

        List<string> weapons = _game.getMod().getCraftWeaponsList();
        foreach (var i in weapons)
        {
            RuleCraftWeapon w = _game.getMod().getCraftWeapon(i);
            if (_base.getStorageItems().getItem(w.getLauncherItem()) > 0)
            {
                _weapons.Add(w);
                string ss2;
                string ss = _base.getStorageItems().getItem(w.getLauncherItem()).ToString();
                if (!string.IsNullOrEmpty(w.getClipItem()))
                {
                    ss2 = _base.getStorageItems().getItem(w.getClipItem()).ToString();
                }
                else
                {
                    ss2 = tr("STR_NOT_AVAILABLE");
                }
                _lstWeapons.addRow(3, tr(w.getType()), ss, ss2);
            }
        }
        _lstWeapons.onMouseClick(lstWeaponsClick);
    }

    /**
     *
     */
    ~CraftWeaponsState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Engine.Action _) =>
        _game.popState();

    /**
     * Equips the weapon on the craft and returns to the previous screen.
     * @param action Pointer to an action.
     */
    void lstWeaponsClick(Engine.Action _)
    {
        CraftWeapon current = _base.getCrafts()[(int)_craft].getWeapons()[(int)_weapon];
        // Remove current weapon
        if (current != null)
        {
            _base.getStorageItems().addItem(current.getRules().getLauncherItem());
            _base.getStorageItems().addItem(current.getRules().getClipItem(), current.getClipsLoaded(_game.getMod()));
            current = null;
            _base.getCrafts()[(int)_craft].getWeapons()[(int)_weapon] = null;
        }

        // Equip new weapon
        if (_weapons[(int)_lstWeapons.getSelectedRow()] != null)
        {
            CraftWeapon sel = new CraftWeapon(_weapons[(int)_lstWeapons.getSelectedRow()], 0);
            sel.setRearming(true);
            _base.getStorageItems().removeItem(sel.getRules().getLauncherItem());
            _base.getCrafts()[(int)_craft].getWeapons()[(int)_weapon] = sel;
            if (_base.getCrafts()[(int)_craft].getStatus() == "STR_READY")
            {
                _base.getCrafts()[(int)_craft].setStatus("STR_REARMING");
            }
        }

        _game.popState();
    }
}
