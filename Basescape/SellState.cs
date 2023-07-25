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
 * Sell/Sack screen that lets the player sell
 * any items in a particular base.
 */
internal class SellState : State
{
    Base _base;
    uint _sel;
    int _total;
    double _spaceChange;
    OptionsOrigin _origin;
    Window _window;
    TextButton _btnOk, _btnCancel;
    Text _txtTitle, _txtSales, _txtFunds, _txtQuantity, _txtSell, _txtValue, _txtSpaceUsed;
    ComboBox _cbxCategory;
    TextList _lstItems;
    byte _ammoColor;
    List<string> _cats;
    HashSet<string> _craftWeapons, _armors;
    List<TransferRow> _items;
    Timer _timerInc, _timerDec;
    List<int> _rows;

    /**
     * Initializes all the elements in the Sell/Sack screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param origin Game section that originated this state.
     */
    internal SellState(Base @base, OptionsOrigin origin = OptionsOrigin.OPT_GEOSCAPE)
    {
        _base = @base;
        _sel = 0;
        _total = 0;
        _spaceChange = 0;
        _origin = origin;

        bool overfull = Options.storageLimitsEnforced && _base.storesOverfull();

        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        _btnOk = new TextButton(overfull ? 288 : 148, 16, overfull ? 16 : 8, 176);
        _btnCancel = new TextButton(148, 16, 164, 176);
        _txtTitle = new Text(310, 17, 5, 8);
        _txtSales = new Text(150, 9, 10, 24);
        _txtFunds = new Text(150, 9, 160, 24);
        _txtSpaceUsed = new Text(150, 9, 160, 34);
        _txtQuantity = new Text(54, 9, 136, 44);
        _txtSell = new Text(96, 9, 190, 44);
        _txtValue = new Text(40, 9, 270, 44);
        _cbxCategory = new ComboBox(this, 120, 16, 10, 36);
        _lstItems = new TextList(287, 120, 8, 54);

        // Set palette
        setInterface("sellMenu");

        _ammoColor = (byte)_game.getMod().getInterface("sellMenu").getElement("ammoColor").color;

        add(_window, "window", "sellMenu");
        add(_btnOk, "button", "sellMenu");
        add(_btnCancel, "button", "sellMenu");
        add(_txtTitle, "text", "sellMenu");
        add(_txtSales, "text", "sellMenu");
        add(_txtFunds, "text", "sellMenu");
        add(_txtSpaceUsed, "text", "sellMenu");
        add(_txtQuantity, "text", "sellMenu");
        add(_txtSell, "text", "sellMenu");
        add(_txtValue, "text", "sellMenu");
        add(_lstItems, "list", "sellMenu");
        add(_cbxCategory, "text", "sellMenu");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK13.SCR"));

        _btnOk.setText(tr("STR_SELL_SACK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

        _btnCancel.setText(tr("STR_CANCEL"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        if (overfull)
        {
            _btnCancel.setVisible(false);
            _btnOk.setVisible(false);
        }

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_SELL_ITEMS_SACK_PERSONNEL"));

        _txtSales.setText(tr("STR_VALUE_OF_SALES").arg(Unicode.formatFunding(_total)));

        _txtFunds.setText(tr("STR_FUNDS").arg(Unicode.formatFunding(_game.getSavedGame().getFunds())));

        _txtSpaceUsed.setVisible(Options.storageLimitsEnforced);

        string ss = $"{_base.getUsedStores()}:{_base.getAvailableStores()}";
        _txtSpaceUsed.setText(ss);
        _txtSpaceUsed.setText(tr("STR_SPACE_USED").arg(ss));

        _txtQuantity.setText(tr("STR_QUANTITY_UC"));

        _txtSell.setText(tr("STR_SELL_SACK"));

        _txtValue.setText(tr("STR_VALUE"));

        _lstItems.setArrowColumn(182, ArrowOrientation.ARROW_VERTICAL);
        _lstItems.setColumns(4, 156, 54, 24, 53);
        _lstItems.setSelectable(true);
        _lstItems.setBackground(_window);
        _lstItems.setMargin(2);
        _lstItems.onLeftArrowPress(lstItemsLeftArrowPress);
        _lstItems.onLeftArrowRelease(lstItemsLeftArrowRelease);
        _lstItems.onLeftArrowClick(lstItemsLeftArrowClick);
        _lstItems.onRightArrowPress(lstItemsRightArrowPress);
        _lstItems.onRightArrowRelease(lstItemsRightArrowRelease);
        _lstItems.onRightArrowClick(lstItemsRightArrowClick);
        _lstItems.onMousePress(lstItemsMousePress);

        _cats.Add("STR_ALL_ITEMS");

        List<string> cw = _game.getMod().getCraftWeaponsList();
        foreach (var i in cw)
        {
            RuleCraftWeapon rule = _game.getMod().getCraftWeapon(i);
            _craftWeapons.Add(rule.getLauncherItem());
            _craftWeapons.Add(rule.getClipItem());
        }
        List<string> ar = _game.getMod().getArmorsList();
        foreach (var i in ar)
        {
            Armor rule = _game.getMod().getArmor(i);
            _armors.Add(rule.getStoreItem());
        }

        foreach (var i in _base.getSoldiers())
        {
            if (i.getCraft() == null)
            {
                var row = new TransferRow { type = TransferType.TRANSFER_SOLDIER, rule = i, name = i.getName(true), cost = 0, qtySrc = 1, qtyDst = 0, amount = 0 };
                _items.Add(row);
                string cat = getCategory(_items.Count - 1);
                if (!_cats.Contains(cat))
                {
                    _cats.Add(cat);
                }
            }
        }
        foreach (var i in _base.getCrafts())
        {
            if (i.getStatus() != "STR_OUT")
            {
                var row = new TransferRow { type = TransferType.TRANSFER_CRAFT, rule = i, name = i.getName(_game.getLanguage()), cost = i.getRules().getSellCost(), qtySrc = 1, qtyDst = 0, amount = 0 };
                _items.Add(row);
                string cat = getCategory(_items.Count - 1);
                if (!_cats.Contains(cat))
                {
                    _cats.Add(cat);
                }
            }
        }
        if (_base.getAvailableScientists() > 0)
        {
            var row = new TransferRow { type = TransferType.TRANSFER_SCIENTIST, rule = null, name = tr("STR_SCIENTIST"), cost = 0, qtySrc = _base.getAvailableScientists(), qtyDst = 0, amount = 0 };
            _items.Add(row);
            string cat = getCategory(_items.Count - 1);
            if (!_cats.Contains(cat))
            {
                _cats.Add(cat);
            }
        }
        if (_base.getAvailableEngineers() > 0)
        {
            var row = new TransferRow { type = TransferType.TRANSFER_ENGINEER, rule = null, name = tr("STR_ENGINEER"), cost = 0, qtySrc = _base.getAvailableEngineers(), qtyDst = 0, amount = 0 };
            _items.Add(row);
            string cat = getCategory(_items.Count - 1);
            if (!_cats.Contains(cat))
            {
                _cats.Add(cat);
            }
        }
        List<string> items = _game.getMod().getItemsList();
        foreach (var i in items)
        {
            int qty = _base.getStorageItems().getItem(i);
            if (Options.storageLimitsEnforced && _origin == OptionsOrigin.OPT_BATTLESCAPE)
            {
                foreach (var j in _base.getTransfers())
                {
                    if (j.getItems() == i)
                    {
                        qty += j.getQuantity();
                    }
                }
                foreach (var j in _base.getCrafts())
                {
                    qty += j.getItems().getItem(i);
                }
            }
            RuleItem rule = _game.getMod().getItem(i, true);
            if (qty > 0 && (Options.canSellLiveAliens || !rule.isAlien()))
            {
                var row = new TransferRow { type = TransferType.TRANSFER_ITEM, rule = rule, name = tr(i), cost = rule.getSellCost(), qtySrc = qty, qtyDst = 0, amount = 0 };
                _items.Add(row);
                string cat = getCategory(_items.Count - 1);
                if (!_cats.Contains(cat))
                {
                    _cats.Add(cat);
                }
            }
        }

        _cbxCategory.setOptions(_cats, true);
        _cbxCategory.onChange(cbxCategoryChange);

        updateList();

        _timerInc = new Timer(250);
        _timerInc.onTimer((StateHandler)increase);
        _timerDec = new Timer(250);
        _timerDec.onTimer((StateHandler)decrease);
    }

    /**
     *
     */
    ~SellState()
    {
        _timerInc = null;
        _timerDec = null;
    }

    /**
     * Starts increasing the item.
     * @param action Pointer to an action.
     */
    void lstItemsLeftArrowPress(Action action)
    {
        _sel = _lstItems.getSelectedRow();
        if (action.getDetails().button.button == SDL_BUTTON_LEFT && !_timerInc.isRunning()) _timerInc.start();
    }

    /**
     * Stops increasing the item.
     * @param action Pointer to an action.
     */
    void lstItemsLeftArrowRelease(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            _timerInc.stop();
        }
    }

    /**
     * Increases the selected item;
     * by one on left-click, to max on right-click.
     * @param action Pointer to an action.
     */
    void lstItemsLeftArrowClick(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_RIGHT) changeByValue(int.MaxValue, 1);
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            changeByValue(1, 1);
            _timerInc.setInterval(250);
            _timerDec.setInterval(250);
        }
    }

    /**
     * Starts decreasing the item.
     * @param action Pointer to an action.
     */
    void lstItemsRightArrowPress(Action action)
    {
        _sel = _lstItems.getSelectedRow();
        if (action.getDetails().button.button == SDL_BUTTON_LEFT && !_timerDec.isRunning()) _timerDec.start();
    }

    /**
     * Stops decreasing the item.
     * @param action Pointer to an action.
     */
    void lstItemsRightArrowRelease(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            _timerDec.stop();
        }
    }

    /**
     * Decreases the selected item;
     * by one on left-click, to 0 on right-click.
     * @param action Pointer to an action.
     */
    void lstItemsRightArrowClick(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_RIGHT) changeByValue(int.MaxValue, -1);
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            changeByValue(1, -1);
            _timerInc.setInterval(250);
            _timerDec.setInterval(250);
        }
    }

    /**
     * Handles the mouse-wheels on the arrow-buttons.
     * @param action Pointer to an action.
     */
    void lstItemsMousePress(Action action)
    {
        _sel = _lstItems.getSelectedRow();
        if (action.getDetails().wheel.y > 0) //button.button == SDL_BUTTON_WHEELUP
        {
            _timerInc.stop();
            _timerDec.stop();
            if (action.getAbsoluteXMouse() >= _lstItems.getArrowsLeftEdge() &&
                action.getAbsoluteXMouse() <= _lstItems.getArrowsRightEdge())
            {
                changeByValue(Options.changeValueByMouseWheel, 1);
            }
        }
        else if (action.getDetails().wheel.y < 0) //button.button == SDL_BUTTON_WHEELDOWN
        {
            _timerInc.stop();
            _timerDec.stop();
            if (action.getAbsoluteXMouse() >= _lstItems.getArrowsLeftEdge() &&
                action.getAbsoluteXMouse() <= _lstItems.getArrowsRightEdge())
            {
                changeByValue(Options.changeValueByMouseWheel, -1);
            }
        }
    }

    /**
     * Determines the category a row item belongs in.
     * @param sel Selected row.
     * @returns Item category.
     */
    string getCategory(int sel)
    {
	    RuleItem rule = null;
	    switch (_items[sel].type)
	    {
	        case TransferType.TRANSFER_SOLDIER:
	        case TransferType.TRANSFER_SCIENTIST:
	        case TransferType.TRANSFER_ENGINEER:
		        return "STR_PERSONNEL";
	        case TransferType.TRANSFER_CRAFT:
		        return "STR_CRAFT_ARMAMENT";
	        case TransferType.TRANSFER_ITEM:
		        rule = (RuleItem)_items[sel].rule;
		        if (rule.getBattleType() == BattleType.BT_CORPSE || rule.isAlien())
		        {
			        return "STR_ALIENS";
		        }
		        if (rule.getBattleType() == BattleType.BT_NONE)
		        {
			        if (_craftWeapons.Contains(rule.getType()))
			        {
				        return "STR_CRAFT_ARMAMENT";
			        }
			        if (_armors.Contains(rule.getType()))
			        {
				        return "STR_EQUIPMENT";
			        }
			        return "STR_COMPONENTS";
		        }
		        return "STR_EQUIPMENT";
	    }
	    return "STR_ALL_ITEMS";
    }

    /**
     * Increases or decreases the quantity of the selected item to sell.
     * @param change How much we want to add or remove.
     * @param dir Direction to change, +1 to increase or -1 to decrease.
     */
    void changeByValue(int change, int dir)
    {
        if (dir > 0)
        {
            if (0 >= change || getRow().qtySrc <= getRow().amount) return;
            change = Math.Min(getRow().qtySrc - getRow().amount, change);
        }
        else
        {
            if (0 >= change || 0 >= getRow().amount) return;
            change = Math.Min(getRow().amount, change);
        }
        getRow().amount += dir * change;
        _total += dir * getRow().cost * change;

        // Calculate the change in storage space.
        Craft craft;
        Soldier soldier;
        RuleItem armor, item, weapon, ammo;
        double total = 0.0;
        switch (getRow().type)
        {
            case TransferType.TRANSFER_SOLDIER:
                soldier = (Soldier)getRow().rule;
                if (soldier.getArmor().getStoreItem() != Armor.NONE)
                {
                    armor = _game.getMod().getItem(soldier.getArmor().getStoreItem(), true);
                    _spaceChange += dir * armor.getSize();
                }
                break;
            case TransferType.TRANSFER_CRAFT:
                craft = (Craft)getRow().rule;
                foreach (var w in craft.getWeapons())
                {
                    if (w != null)
                    {
                        weapon = _game.getMod().getItem(w.getRules().getLauncherItem(), true);
                        total += weapon.getSize();
                        ammo = _game.getMod().getItem(w.getRules().getClipItem());
                        if (ammo != null)
                            total += ammo.getSize() * w.getClipsLoaded(_game.getMod());
                    }
                }
                _spaceChange += dir * total;
                break;
            case TransferType.TRANSFER_ITEM:
                item = (RuleItem)getRow().rule;
                _spaceChange -= dir * change * item.getSize();
                break;
            default:
                //TRANSFER_SCIENTIST and TRANSFER_ENGINEER do not own anything that takes storage
                break;
        }

        updateItemStrings();
    }

    /// Gets the row of the current selection.
    TransferRow getRow() =>
        _items[_rows[(int)_sel]];

    /**
     * Sells the selected items.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        _game.getSavedGame().setFunds(_game.getSavedGame().getFunds() + _total);
        Soldier soldier;
        Craft craft;
        foreach (var i in _items)
        {
            if (i.amount > 0)
            {
                switch (i.type)
                {
                    case TransferType.TRANSFER_SOLDIER:
                        soldier = (Soldier)i.rule;
                        foreach (var s in _base.getSoldiers())
                        {
                            if (s == soldier)
                            {
                                if (s.getArmor().getStoreItem() != Armor.NONE)
                                {
                                    _base.getStorageItems().addItem(s.getArmor().getStoreItem());
                                }
                                _base.getSoldiers().Remove(s);
                                break;
                            }
                        }
                        soldier = null;
                        break;
                    case TransferType.TRANSFER_CRAFT:
                        craft = (Craft)i.rule;
                        _base.removeCraft(craft, true);
                        craft = null;
                        break;
                    case TransferType.TRANSFER_SCIENTIST:
                        _base.setScientists(_base.getScientists() - i.amount);
                        break;
                    case TransferType.TRANSFER_ENGINEER:
                        _base.setEngineers(_base.getEngineers() - i.amount);
                        break;
                    case TransferType.TRANSFER_ITEM:
                        RuleItem item = (RuleItem)i.rule;
                        if (_base.getStorageItems().getItem(item.getType()) < i.amount)
                        {
                            int toRemove = i.amount - _base.getStorageItems().getItem(item.getType());

                            // remove all of said items from base
                            _base.getStorageItems().removeItem(item.getType(), int.MaxValue);

                            // if we still need to remove any, remove them from the crafts first, and keep a running tally
                            var crafts = _base.getCrafts();
                            for (var j = 0; j < crafts.Count && toRemove != 0; ++j)
                            {
                                if (crafts[j].getItems().getItem(item.getType()) < toRemove)
                                {
                                    toRemove -= crafts[j].getItems().getItem(item.getType());
                                    crafts[j].getItems().removeItem(item.getType(), int.MaxValue);
                                }
                                else
                                {
                                    crafts[j].getItems().removeItem(item.getType(), toRemove);
                                    toRemove = 0;
                                }
                            }

                            var transfers = _base.getTransfers();
                            // if there are STILL any left to remove, take them from the transfers, and if necessary, delete it.
                            for (var j = 0; j < transfers.Count && toRemove != 0;)
                            {
                                if (transfers[j].getItems() == item.getType())
                                {
                                    if (transfers[j].getQuantity() <= toRemove)
                                    {
                                        toRemove -= transfers[j].getQuantity();
                                        _base.getTransfers().RemoveAt(j);
                                    }
                                    else
                                    {
                                        transfers[j].setItems(transfers[j].getItems(), transfers[j].getQuantity() - toRemove);
                                        toRemove = 0;
                                    }
                                }
                                else
                                {
                                    ++j;
                                }
                            }
                        }
                        else
                        {
                            _base.getStorageItems().removeItem(item.getType(), i.amount);
                        }
                        break;
                }
            }
        }
        _game.popState();
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _) =>
        _game.popState();

    /**
    * Updates the production list to match the category filter.
    */
    void cbxCategoryChange(Action _) =>
	    updateList();

    /**
     * Increases the quantity of the selected item to sell by one.
     */
    void increase()
    {
        _timerDec.setInterval(50);
        _timerInc.setInterval(50);
        changeByValue(1, 1);
    }

    /**
     * Decreases the quantity of the selected item to sell by one.
     */
    void decrease()
    {
        _timerInc.setInterval(50);
        _timerDec.setInterval(50);
        changeByValue(1, -1);
    }

    /**
     * Updates the quantity-strings of the selected item.
     */
    void updateItemStrings()
    {
        string ss = getRow().amount.ToString();
        _lstItems.setCellText(_sel, 2, ss);
        string ss2 = (getRow().qtySrc - getRow().amount).ToString();
        _lstItems.setCellText(_sel, 1, ss2);
        _txtSales.setText(tr("STR_VALUE_OF_SALES").arg(Unicode.formatFunding(_total)));

        if (getRow().amount > 0)
        {
            _lstItems.setRowColor(_sel, _lstItems.getSecondaryColor());
        }
        else
        {
            _lstItems.setRowColor(_sel, _lstItems.getColor());
            if (getRow().type == TransferType.TRANSFER_ITEM)
            {
                RuleItem rule = (RuleItem)getRow().rule;
                if (rule.getBattleType() == BattleType.BT_AMMO || (rule.getBattleType() == BattleType.BT_NONE && rule.getClipSize() > 0))
                {
                    _lstItems.setRowColor(_sel, _ammoColor);
                }
            }
        }

        string ss3 = _base.getUsedStores().ToString();
        if (Math.Abs(_spaceChange) > 0.05)
        {
            ss3 = $"{ss3}(";
            if (_spaceChange > 0.05)
                ss3 = $"{ss3}+";
            ss3 = $"{ss3}{_spaceChange:N1})";
        }
        ss3 = $"{ss3}:{_base.getAvailableStores()}";
        _txtSpaceUsed.setText(tr("STR_SPACE_USED").arg(ss3));
        if (Options.storageLimitsEnforced)
        {
            _btnOk.setVisible(!_base.storesOverfull(_spaceChange));
        }
    }

    /**
     * Filters the current list of items.
     */
    void updateList()
    {
        _lstItems.clearList();
        _rows.Clear();
        for (int i = 0; i < _items.Count; ++i)
        {
            string cat = _cats[(int)_cbxCategory.getSelected()];
            if (cat != "STR_ALL_ITEMS" && cat != getCategory(i))
            {
                continue;
            }
            string name = _items[i].name;
            bool ammo = false;
            if (_items[i].type == TransferType.TRANSFER_ITEM)
            {
                RuleItem rule = (RuleItem)_items[i].rule;
                ammo = (rule.getBattleType() == BattleType.BT_AMMO || (rule.getBattleType() == BattleType.BT_NONE && rule.getClipSize() > 0));
                if (ammo)
                {
                    name = name.Insert(0, "  ");
                }
            }
            string ssQty = (_items[i].qtySrc - _items[i].amount).ToString();
            string ssAmount = _items[i].amount.ToString();
            _lstItems.addRow(4, name, ssQty, ssAmount, Unicode.formatFunding(_items[i].cost));
            _rows.Add(i);
            if (_items[i].amount > 0)
            {
                _lstItems.setRowColor((uint)(_rows.Count - 1), _lstItems.getSecondaryColor());
            }
            else if (ammo)
            {
                _lstItems.setRowColor((uint)(_rows.Count - 1), _ammoColor);
            }
        }
    }
}
