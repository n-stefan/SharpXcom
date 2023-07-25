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
 * Stores window that displays all
 * the items currently stored in a base.
 */
internal class StoresState : State
{
    Base _base;
    Window _window;
    TextButton _btnOk;
    Text _txtTitle, _txtItem, _txtQuantity, _txtSpaceUsed;
    TextList _lstStores;

    /**
     * Initializes all the elements in the Stores window.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     */
    internal StoresState(Base @base)
    {
        _base = @base;

        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        _btnOk = new TextButton(300, 16, 10, 176);
        _txtTitle = new Text(310, 17, 5, 8);
        _txtItem = new Text(142, 9, 10, 32);
        _txtQuantity = new Text(88, 9, 152, 32);
        _txtSpaceUsed = new Text(74, 9, 240, 32);
        _lstStores = new TextList(288, 128, 8, 40);

        // Set palette
        setInterface("storesInfo");

        add(_window, "window", "storesInfo");
        add(_btnOk, "button", "storesInfo");
        add(_txtTitle, "text", "storesInfo");
        add(_txtItem, "text", "storesInfo");
        add(_txtQuantity, "text", "storesInfo");
        add(_txtSpaceUsed, "text", "storesInfo");
        add(_lstStores, "list", "storesInfo");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK13.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_STORES"));

        _txtItem.setText(tr("STR_ITEM"));

        _txtQuantity.setText(tr("STR_QUANTITY_UC"));

        _txtSpaceUsed.setText(tr("STR_SPACE_USED_UC"));

        _lstStores.setColumns(3, 162, 92, 32);
        _lstStores.setSelectable(true);
        _lstStores.setBackground(_window);
        _lstStores.setMargin(2);

        List<string> items = _game.getMod().getItemsList();
        foreach (var i in items)
        {
            int qty = _base.getStorageItems().getItem(i);
            if (qty > 0)
            {
                RuleItem rule = _game.getMod().getItem(i, true);
                string ss = qty.ToString();
                string ss2 = (qty * rule.getSize()).ToString();
                _lstStores.addRow(3, tr(i), ss, ss2);
            }
        }
    }

    /**
     *
     */
    ~StoresState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();
}
