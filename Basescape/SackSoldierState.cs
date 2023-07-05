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
 * Window shown when the player tries to
 * sack a soldier.
 */
internal class SackSoldierState : State
{
    Base _base;
    uint _soldierId;
    Window _window;
    TextButton _btnOk, _btnCancel;
    Text _txtTitle, _txtSoldier;

    /**
     * Initializes all the elements in a Sack Soldier window.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param soldierId ID of the soldier to sack.
     */
    internal SackSoldierState(Base @base, uint soldierId)
    {
        _base = @base;
        _soldierId = soldierId;

        _screen = false;

        // Create objects
        _window = new Window(this, 152, 80, 84, 60);
        _btnOk = new TextButton(44, 16, 100, 115);
        _btnCancel = new TextButton(44, 16, 176, 115);
        _txtTitle = new Text(142, 9, 89, 75);
        _txtSoldier = new Text(142, 9, 89, 85);

        // Set palette
        setInterface("sackSoldier");

        add(_window, "window", "sackSoldier");
        add(_btnOk, "button", "sackSoldier");
        add(_btnCancel, "button", "sackSoldier");
        add(_txtTitle, "text", "sackSoldier");
        add(_txtSoldier, "text", "sackSoldier");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK13.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

        _btnCancel.setText(tr("STR_CANCEL_UC"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_SACK"));

        string ss = $"{_base.getSoldiers()[(int)_soldierId].getName(true)}?";

        _txtSoldier.setAlign(TextHAlign.ALIGN_CENTER);
        _txtSoldier.setText(ss);
    }

    /**
     *
     */
    ~SackSoldierState() { }

    /**
     * Sacks the soldier and returns
     * to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Engine.Action _)
    {
        Soldier soldier = _base.getSoldiers()[(int)_soldierId];
        if (soldier.getArmor().getStoreItem() != Armor.NONE)
        {
            _base.getStorageItems().addItem(soldier.getArmor().getStoreItem());
        }
        _base.getSoldiers().RemoveAt((int)_soldierId);
        soldier = null;
        _game.popState();
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Engine.Action _) =>
        _game.popState();
}
