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
 * Transfers window that displays all
 * the items currently in-transit to a base.
 */
internal class TransfersState : State
{
    Base _base;
    Window _window;
    TextButton _btnOk;
    Text _txtTitle, _txtItem, _txtQuantity, _txtArrivalTime;
    TextList _lstTransfers;

    /**
     * Initializes all the elements in the Transfers window.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     */
    internal TransfersState(Base @base)
    {
        _base = @base;

        _screen = false;

        // Create objects
        _window = new Window(this, 320, 184, 0, 8, WindowPopup.POPUP_BOTH);
        _btnOk = new TextButton(288, 16, 16, 166);
        _txtTitle = new Text(278, 17, 21, 18);
        _txtItem = new Text(114, 9, 16, 34);
        _txtQuantity = new Text(54, 9, 152, 34);
        _txtArrivalTime = new Text(112, 9, 212, 34);
        _lstTransfers = new TextList(273, 112, 14, 50);

        // Set palette
        setInterface("transferInfo");

        add(_window, "window", "transferInfo");
        add(_btnOk, "button", "transferInfo");
        add(_txtTitle, "text", "transferInfo");
        add(_txtItem, "text", "transferInfo");
        add(_txtQuantity, "text", "transferInfo");
        add(_txtArrivalTime, "text", "transferInfo");
        add(_lstTransfers, "list", "transferInfo");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK13.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_TRANSFERS"));

        _txtItem.setText(tr("STR_ITEM"));

        _txtQuantity.setText(tr("STR_QUANTITY_UC"));

        _txtArrivalTime.setText(tr("STR_ARRIVAL_TIME_HOURS"));

        _lstTransfers.setColumns(3, 155, 75, 46);
        _lstTransfers.setSelectable(true);
        _lstTransfers.setBackground(_window);
        _lstTransfers.setMargin(2);

        foreach (var i in _base.getTransfers())
        {
            string ss = i.getQuantity().ToString();
            string ss2 = i.getHours().ToString();
            _lstTransfers.addRow(3, i.getName(_game.getLanguage()), ss, ss2);
        }
    }

    /**
     *
     */
    ~TransfersState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();
}
