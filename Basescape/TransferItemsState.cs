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
 * Transfer screen that lets the player pick
 * what items to transfer between bases.
 */
internal class TransferItemsState : State
{
    Base _baseFrom, _baseTo;
    uint _sel;
    int _total, _pQty, _cQty, _aQty;
    double _iQty;
    double _distance;
    byte _ammoColor;
    Window _window;
    TextButton _btnOk, _btnCancel;
    Text _txtTitle, _txtQuantity, _txtAmountTransfer, _txtAmountDestination;
    ComboBox _cbxCategory;
    TextList _lstItems;
    Timer _timerInc, _timerDec;
    List<string> _cats;
    HashSet<string> _craftWeapons, _armors;
    List<TransferRow> _items;
    List<int> _rows;

    /**
     * Initializes all the elements in the Transfer screen.
     * @param game Pointer to the core game.
     * @param baseFrom Pointer to the source base.
     * @param baseTo Pointer to the destination base.
     */
    internal TransferItemsState(Base baseFrom, Base baseTo)
    {
        _baseFrom = baseFrom;
        _baseTo = baseTo;
        _sel = 0;
        _total = 0;
        _pQty = 0;
        _cQty = 0;
        _aQty = 0;
        _iQty = 0.0;
        _distance = 0.0;
        _ammoColor = 0;

        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        _btnOk = new TextButton(148, 16, 8, 176);
        _btnCancel = new TextButton(148, 16, 164, 176);
        _txtTitle = new Text(310, 17, 5, 8);
        _txtQuantity = new Text(50, 9, 150, 24);
        _txtAmountTransfer = new Text(60, 17, 200, 24);
        _txtAmountDestination = new Text(60, 17, 260, 24);
        _cbxCategory = new ComboBox(this, 120, 16, 10, 24);
        _lstItems = new TextList(287, 128, 8, 44);

        // Set palette
        setInterface("transferMenu");

        _ammoColor = (byte)_game.getMod().getInterface("transferMenu").getElement("ammoColor").color;

        add(_window, "window", "transferMenu");
        add(_btnOk, "button", "transferMenu");
        add(_btnCancel, "button", "transferMenu");
        add(_txtTitle, "text", "transferMenu");
        add(_txtQuantity, "text", "transferMenu");
        add(_txtAmountTransfer, "text", "transferMenu");
        add(_txtAmountDestination, "text", "transferMenu");
        add(_lstItems, "list", "transferMenu");
        add(_cbxCategory, "text", "transferMenu");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK13.SCR"));

        _btnOk.setText(tr("STR_TRANSFER"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

        _btnCancel.setText(tr("STR_CANCEL"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_TRANSFER"));

        _txtQuantity.setText(tr("STR_QUANTITY_UC"));

        _txtAmountTransfer.setText(tr("STR_AMOUNT_TO_TRANSFER"));
        _txtAmountTransfer.setWordWrap(true);

        _txtAmountDestination.setText(tr("STR_AMOUNT_AT_DESTINATION"));
        _txtAmountDestination.setWordWrap(true);

        _lstItems.setArrowColumn(193, ArrowOrientation.ARROW_VERTICAL);
        _lstItems.setColumns(4, 162, 58, 40, 27);
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

        _distance = getDistance();

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

        foreach (var i in _baseFrom.getSoldiers())
        {
            if (i.getCraft() == null)
            {
                var row = new TransferRow { type = TransferType.TRANSFER_SOLDIER, rule = i, name = i.getName(true), cost = (int)(5 * _distance), qtySrc = 1, qtyDst = 0, amount = 0 };
                _items.Add(row);
                string cat = getCategory(_items.Count - 1);
                if (!_cats.Contains(cat))
                {
                    _cats.Add(cat);
                }
            }
        }
        foreach (var i in _baseFrom.getCrafts())
        {
            if (i.getStatus() != "STR_OUT" || (Options.canTransferCraftsWhileAirborne && i.getFuel() >= i.getFuelLimit(_baseTo)))
            {
                var row = new TransferRow { type = TransferType.TRANSFER_CRAFT, rule = i, name = i.getName(_game.getLanguage()), cost = (int)(25 * _distance), qtySrc = 1, qtyDst = 0, amount = 0 };
                _items.Add(row);
                string cat = getCategory(_items.Count - 1);
                if (!_cats.Contains(cat))
                {
                    _cats.Add(cat);
                }
            }
        }
        if (_baseFrom.getAvailableScientists() > 0)
        {
            var row = new TransferRow { type = TransferType.TRANSFER_SCIENTIST, rule = null, name = tr("STR_SCIENTIST"), cost = (int)(5 * _distance), qtySrc = _baseFrom.getAvailableScientists(), qtyDst = _baseTo.getAvailableScientists(), amount = 0 };
            _items.Add(row);
            string cat = getCategory(_items.Count - 1);
            if (!_cats.Contains(cat))
            {
                _cats.Add(cat);
            }
        }
        if (_baseFrom.getAvailableEngineers() > 0)
        {
            var row = new TransferRow { type = TransferType.TRANSFER_ENGINEER, rule = null, name = tr("STR_ENGINEER"), cost = (int)(5 * _distance), qtySrc = _baseFrom.getAvailableEngineers(), qtyDst = _baseTo.getAvailableEngineers(), amount = 0 };
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
            int qty = _baseFrom.getStorageItems().getItem(i);
            if (qty > 0)
            {
                RuleItem rule = _game.getMod().getItem(i);
                var row = new TransferRow { type = TransferType.TRANSFER_ITEM, rule = rule, name = tr(i), cost = (int)(1 * _distance), qtySrc = qty, qtyDst = _baseTo.getStorageItems().getItem(i), amount = 0 };
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
    ~TransferItemsState()
    {
        _timerInc = null;
        _timerDec = null;
    }

    /**
     * Transfers the selected items.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.pushState(new TransferConfirmState(_baseTo, this));

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _)
    {
        _game.popState();
        _game.popState();
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
     * by one on left-click; to max on right-click.
     * @param action Pointer to an action.
     */
    void lstItemsLeftArrowClick(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_RIGHT) increaseByValue(int.MaxValue);
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            increaseByValue(1);
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
     * by one on left-click; to 0 on right-click.
     * @param action Pointer to an action.
     */
    void lstItemsRightArrowClick(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_RIGHT) decreaseByValue(int.MaxValue);
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            decreaseByValue(1);
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
                increaseByValue(Options.changeValueByMouseWheel);
            }
        }
        else if (action.getDetails().wheel.y < 0) //button.button == SDL_BUTTON_WHEELDOWN
        {
            _timerInc.stop();
            _timerDec.stop();
            if (action.getAbsoluteXMouse() >= _lstItems.getArrowsLeftEdge() &&
                action.getAbsoluteXMouse() <= _lstItems.getArrowsRightEdge())
            {
                decreaseByValue(Options.changeValueByMouseWheel);
            }
        }
    }

    /**
    * Updates the production list to match the category filter.
    */
    void cbxCategoryChange(Action _) =>
	    updateList();

    /**
     * Increases the quantity of the selected item to transfer by one.
     */
    void increase()
    {
        _timerDec.setInterval(50);
        _timerInc.setInterval(50);
        increaseByValue(1);
    }

    /**
     * Decreases the quantity of the selected item to transfer by one.
     */
    void decrease()
    {
        _timerInc.setInterval(50);
        _timerDec.setInterval(50);
        decreaseByValue(1);
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

    /// Gets the row of the current selection.
    TransferRow getRow() =>
        _items[_rows[(int)_sel]];

    /**
     * Increases the quantity of the selected item to transfer by "change".
     * @param change How much we want to add.
     */
    void increaseByValue(int change)
    {
        if (0 >= change || getRow().qtySrc <= getRow().amount) return;
        string errorMessage = null;
        RuleItem selItem = null;
        Craft craft = null;

        switch (getRow().type)
        {
            case TransferType.TRANSFER_SOLDIER:
            case TransferType.TRANSFER_SCIENTIST:
            case TransferType.TRANSFER_ENGINEER:
                if (_pQty + 1 > _baseTo.getAvailableQuarters() - _baseTo.getUsedQuarters())
                {
                    errorMessage = tr("STR_NO_FREE_ACCOMODATION");
                }
                break;
            case TransferType.TRANSFER_CRAFT:
                craft = (Craft)getRow().rule;
                if (_cQty + 1 > _baseTo.getAvailableHangars() - _baseTo.getUsedHangars())
                {
                    errorMessage = tr("STR_NO_FREE_HANGARS_FOR_TRANSFER");
                }
                else if (_pQty + craft.getNumSoldiers() > _baseTo.getAvailableQuarters() - _baseTo.getUsedQuarters())
                {
                    errorMessage = tr("STR_NO_FREE_ACCOMODATION_CREW");
                }
                else if (Options.storageLimitsEnforced && _baseTo.storesOverfull(_iQty + craft.getItems().getTotalSize(_game.getMod())))
                {
                    errorMessage = tr("STR_NOT_ENOUGH_STORE_SPACE_FOR_CRAFT");
                }
                break;
            case TransferType.TRANSFER_ITEM:
                selItem = (RuleItem)getRow().rule;
                if (!selItem.isAlien() && _baseTo.storesOverfull(selItem.getSize() + _iQty))
                {
                    errorMessage = tr("STR_NOT_ENOUGH_STORE_SPACE");
                }
                else if (selItem.isAlien() && Convert.ToInt32(Options.storageLimitsEnforced) * _aQty + 1 > _baseTo.getAvailableContainment() - Convert.ToInt32(Options.storageLimitsEnforced) * _baseTo.getUsedContainment())
                {
                    errorMessage = tr("STR_NO_ALIEN_CONTAINMENT_FOR_TRANSFER");
                }
                break;
        }

        if (string.IsNullOrEmpty(errorMessage))
        {
            int freeQuarters = _baseTo.getAvailableQuarters() - _baseTo.getUsedQuarters() - _pQty;
            switch (getRow().type)
            {
                case TransferType.TRANSFER_SOLDIER:
                case TransferType.TRANSFER_SCIENTIST:
                case TransferType.TRANSFER_ENGINEER:
                    change = Math.Min(Math.Min(freeQuarters, getRow().qtySrc - getRow().amount), change);
                    _pQty += change;
                    getRow().amount += change;
                    _total += getRow().cost * change;
                    break;
                case TransferType.TRANSFER_CRAFT:
                    _cQty++;
                    _pQty += craft.getNumSoldiers();
                    _iQty += craft.getItems().getTotalSize(_game.getMod());
                    getRow().amount++;
                    if (!Options.canTransferCraftsWhileAirborne || craft.getStatus() != "STR_OUT")
                        _total += getRow().cost;
                    break;
                case TransferType.TRANSFER_ITEM:
                    if (!selItem.isAlien())
                    {
                        double storesNeededPerItem = ((RuleItem)getRow().rule).getSize();
                        double freeStores = _baseTo.getAvailableStores() - _baseTo.getUsedStores() - _iQty;
                        double freeStoresForItem = (double)(int.MaxValue);
                        if (!AreSame(storesNeededPerItem, 0.0))
                        {
                            freeStoresForItem = (freeStores + 0.05) / storesNeededPerItem;
                        }
                        change = Math.Min(Math.Min((int)freeStoresForItem, getRow().qtySrc - getRow().amount), change);
                        _iQty += change * storesNeededPerItem;
                        getRow().amount += change;
                        _total += getRow().cost * change;
                    }
                    else
                    {
                        int freeContainment = Options.storageLimitsEnforced ? _baseTo.getAvailableContainment() - _baseTo.getUsedContainment() - _aQty : int.MaxValue;
                        change = Math.Min(Math.Min(freeContainment, getRow().qtySrc - getRow().amount), change);
                        _aQty += change;
                        getRow().amount += change;
                        _total += getRow().cost * change;
                    }
                    break;
            }
            updateItemStrings();
        }
        else
        {
            _timerInc.stop();
            RuleInterface menuInterface = _game.getMod().getInterface("transferMenu");
            _game.pushState(new ErrorMessageState(errorMessage, _palette, (byte)menuInterface.getElement("errorMessage").color, "BACK13.SCR", menuInterface.getElement("errorPalette").color));
        }
    }

    /**
     * Decreases the quantity of the selected item to transfer by "change".
     * @param change How much we want to remove.
     */
    void decreaseByValue(int change)
    {
        if (0 >= change || 0 >= getRow().amount) return;
        Craft craft = null;
        change = Math.Min(getRow().amount, change);

        switch (getRow().type)
        {
            case TransferType.TRANSFER_SOLDIER:
            case TransferType.TRANSFER_SCIENTIST:
            case TransferType.TRANSFER_ENGINEER:
                _pQty -= change;
                break;
            case TransferType.TRANSFER_CRAFT:
                craft = (Craft)getRow().rule;
                _cQty--;
                _pQty -= craft.getNumSoldiers();
                _iQty -= craft.getItems().getTotalSize(_game.getMod());
                break;
            case TransferType.TRANSFER_ITEM:
                RuleItem selItem = (RuleItem)getRow().rule;
                if (!selItem.isAlien())
                {
                    _iQty -= selItem.getSize() * change;
                }
                else
                {
                    _aQty -= change;
                }
                break;
        }
        getRow().amount -= change;
        if (!Options.canTransferCraftsWhileAirborne || null == craft || craft.getStatus() != "STR_OUT")
            _total -= getRow().cost * change;
        updateItemStrings();
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
		    string ssQtySrc = (_items[i].qtySrc - _items[i].amount).ToString();
		    string ssQtyDst = _items[i].qtyDst.ToString();
		    string ssAmount = _items[i].amount.ToString();
		    _lstItems.addRow(4, name, ssQtySrc, ssAmount, ssQtyDst);
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

    /**
     * Updates the quantity-strings of the selected item.
     */
    void updateItemStrings()
    {
        string ss1 = (getRow().qtySrc - getRow().amount).ToString();
        string ss2 = getRow().amount.ToString();
        _lstItems.setCellText(_sel, 1, ss1);
        _lstItems.setCellText(_sel, 2, ss2);

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
    }

    /**
     * Gets the shortest distance between the two bases.
     * @return Distance.
     */
    double getDistance()
    {
        double[] x = new double[3], y = new double[3], z = new double[3];
        double r = 51.2;
        Base @base = _baseFrom;
	    for (int i = 0; i < 2; ++i) {
		    x[i] = r * Math.Cos(@base.getLatitude()) * Math.Cos(@base.getLongitude());
		    y[i] = r * Math.Cos(@base.getLatitude()) * Math.Sin(@base.getLongitude());
		    z[i] = r * -Math.Sin(@base.getLatitude());
            @base = _baseTo;
	    }
	    x[2] = x[1] - x[0];
	    y[2] = y[1] - y[0];
	    z[2] = z[1] - z[0];
	    return Math.Sqrt(x[2] * x[2] + y[2] * y[2] + z[2] * z[2]);
    }

    /**
     * Gets the total cost of the current transfer.
     * @return Total cost.
     */
    internal int getTotal() =>
	    _total;

    /**
     * Completes the transfer between bases.
     */
    internal void completeTransfer()
    {
        int time = (int)Math.Floor(6 + _distance / 10.0);
        _game.getSavedGame().setFunds(_game.getSavedGame().getFunds() - _total);
        foreach (var i in _items)
        {
            if (i.amount > 0)
            {
                Transfer t = null;
                Craft craft = null;
                switch (i.type)
                {
                    case TransferType.TRANSFER_SOLDIER:
                        foreach (var s in _baseFrom.getSoldiers())
                        {
                            if (s == i.rule)
                            {
                                s.setPsiTraining(false);
                                t = new Transfer(time);
                                t.setSoldier(s);
                                _baseTo.getTransfers().Add(t);
                                _baseFrom.getSoldiers().Remove(s);
                                break;
                            }
                        }
                        break;
                    case TransferType.TRANSFER_CRAFT:
                        craft = (Craft)i.rule;
                        // Transfer soldiers inside craft
                        var soldiers = _baseFrom.getSoldiers();
                        for (var s = 0; s < soldiers.Count;)
                        {
                            if (soldiers[s].getCraft() == craft)
                            {
                                soldiers[s].setPsiTraining(false);
                                if (craft.getStatus() == "STR_OUT")
                                {
                                    _baseTo.getSoldiers().Add(soldiers[s]);
                                }
                                else
                                {
                                    t = new Transfer(time);
                                    t.setSoldier(soldiers[s]);
                                    _baseTo.getTransfers().Add(t);
                                }
                                soldiers.RemoveAt(s);
                            }
                            else
                            {
                                ++s;
                            }
                        }

                        // Transfer craft
                        _baseFrom.removeCraft(craft, false);
                        if (craft.getStatus() == "STR_OUT")
                        {
                            bool returning = (craft.getDestination() == (Target)craft.getBase());
                            _baseTo.getCrafts().Add(craft);
                            craft.setBase(_baseTo, false);
                            if (craft.getFuel() <= craft.getFuelLimit(_baseTo))
                            {
                                craft.setLowFuel(true);
                                craft.returnToBase();
                            }
                            else if (returning)
                            {
                                craft.setLowFuel(false);
                                craft.returnToBase();
                            }
                        }
                        else
                        {
                            t = new Transfer(time);
                            t.setCraft(craft);
                            _baseTo.getTransfers().Add(t);
                        }
                        break;
                    case TransferType.TRANSFER_SCIENTIST:
                        _baseFrom.setScientists(_baseFrom.getScientists() - i.amount);
                        t = new Transfer(time);
                        t.setScientists(i.amount);
                        _baseTo.getTransfers().Add(t);
                        break;
                    case TransferType.TRANSFER_ENGINEER:
                        _baseFrom.setEngineers(_baseFrom.getEngineers() - i.amount);
                        t = new Transfer(time);
                        t.setEngineers(i.amount);
                        _baseTo.getTransfers().Add(t);
                        break;
                    case TransferType.TRANSFER_ITEM:
                        _baseFrom.getStorageItems().removeItem(((RuleItem)i.rule).getType(), i.amount);
                        t = new Transfer(time);
                        t.setItems(((RuleItem)i.rule).getType(), i.amount);
                        _baseTo.getTransfers().Add(t);
                        break;
                }
            }
        }
    }

    /**
     * Runs the arrow timers.
     */
    internal override void think()
    {
	    base.think();

	    _timerInc.think(this, null);
	    _timerDec.think(this, null);
    }
}
