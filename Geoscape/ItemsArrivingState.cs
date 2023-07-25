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
 * Items Arriving window that displays all
 * the items that have arrived at bases.
 */
internal class ItemsArrivingState : State
{
    GeoscapeState _state;
    Base _base;
    Window _window;
    TextButton _btnOk, _btnGotoBase;
    Text _txtTitle, _txtItem, _txtQuantity, _txtDestination;
    TextList _lstTransfers;

    /**
     * Initializes all the elements in the Items Arriving window.
     * @param game Pointer to the core game.
     * @param state Pointer to the Geoscape state.
     */
    internal ItemsArrivingState(GeoscapeState state)
    {
        _state = state;
        _base = null;

        _screen = false;

        // Create objects
        _window = new Window(this, 320, 184, 0, 8, WindowPopup.POPUP_BOTH);
        _btnOk = new TextButton(142, 16, 16, 166);
        _btnGotoBase = new TextButton(142, 16, 162, 166);
        _txtTitle = new Text(310, 17, 5, 18);
        _txtItem = new Text(114, 9, 16, 34);
        _txtQuantity = new Text(54, 9, 152, 34);
        _txtDestination = new Text(112, 9, 212, 34);
        _lstTransfers = new TextList(271, 112, 14, 50);

        // Set palette
        setInterface("itemsArriving");

        add(_window, "window", "itemsArriving");
        add(_btnOk, "button", "itemsArriving");
        add(_btnGotoBase, "button", "itemsArriving");
        add(_txtTitle, "text1", "itemsArriving");
        add(_txtItem, "text1", "itemsArriving");
        add(_txtQuantity, "text1", "itemsArriving");
        add(_txtDestination, "text1", "itemsArriving");
        add(_lstTransfers, "text2", "itemsArriving");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK13.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _btnGotoBase.setText(tr("STR_GO_TO_BASE"));
        _btnGotoBase.onMouseClick(btnGotoBaseClick);
        _btnGotoBase.onKeyboardPress(btnGotoBaseClick, Options.keyOk);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_ITEMS_ARRIVING"));

        _txtItem.setText(tr("STR_ITEM"));

        _txtQuantity.setText(tr("STR_QUANTITY_UC"));

        _txtDestination.setText(tr("STR_DESTINATION_UC"));

        _lstTransfers.setColumns(3, 155, 41, 98);
        _lstTransfers.setSelectable(true);
        _lstTransfers.setBackground(_window);
        _lstTransfers.setMargin(2);

        foreach (var i in _game.getSavedGame().getBases())
        {
            var transfers = i.getTransfers();
            for (var j = 0; j < transfers.Count;)
            {
                if (transfers[j].getHours() == 0)
                {
                    _base = i;

                    // Check if we have an automated use for an item
                    if (transfers[j].getType() == TransferType.TRANSFER_ITEM)
                    {
                        RuleItem item = _game.getMod().getItem(transfers[j].getItems(), true);
                        if (item.getBattleType() == BattleType.BT_NONE)
                        {
                            foreach (var c in i.getCrafts())
                            {
                                c.reuseItem(transfers[j].getItems());
                            }
                        }
                    }

                    // Remove transfer
                    string ss = transfers[j].getQuantity().ToString();
                    _lstTransfers.addRow(3, transfers[j].getName(_game.getLanguage()), ss, i.getName());
                    i.getTransfers().RemoveAt(j);
                }
                else
                {
                    ++j;
                }
            }
        }
    }

    /**
     *
     */
    ~ItemsArrivingState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();

    /**
     * Goes to the base for the respective transfer.
     * @param action Pointer to an action.
     */
    void btnGotoBaseClick(Action _)
    {
        _state.timerReset();
        _game.popState();
        _game.pushState(new BasescapeState(_base, _state.getGlobe()));
    }
}
