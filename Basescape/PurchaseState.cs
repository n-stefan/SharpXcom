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
 * Purchase/Hire screen that lets the player buy
 * new items for a base.
 */
internal class PurchaseState : State
{
    Base _base;
    uint _sel;
    int _total, _pQty, _cQty;
    double _iQty;
    byte _ammoColor;
    Window _window;
    TextButton _btnOk, _btnCancel;
    Text _txtTitle, _txtFunds, _txtPurchases, _txtCost, _txtQuantity, _txtSpaceUsed;
    ComboBox _cbxCategory;
    TextList _lstItems;
    List<string> _cats;
    List<TransferRow> _items;
    Timer _timerInc, _timerDec;
    HashSet<string> _craftWeapons, _armors;
    List<int> _rows;

    /**
     * Initializes all the elements in the Purchase/Hire screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     */
    internal PurchaseState(Base @base)
    {
        _base = @base;
        _sel = 0;
        _total = 0;
        _pQty = 0;
        _cQty = 0;
        _iQty = 0.0;
        _ammoColor = 0;

        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        _btnOk = new TextButton(148, 16, 8, 176);
        _btnCancel = new TextButton(148, 16, 164, 176);
        _txtTitle = new Text(310, 17, 5, 8);
        _txtFunds = new Text(150, 9, 10, 24);
        _txtPurchases = new Text(150, 9, 160, 24);
        _txtSpaceUsed = new Text(150, 9, 160, 34);
        _txtCost = new Text(102, 9, 152, 44);
        _txtQuantity = new Text(60, 9, 256, 44);
        _cbxCategory = new ComboBox(this, 120, 16, 10, 36);
        _lstItems = new TextList(287, 120, 8, 54);

        // Set palette
        setInterface("buyMenu");

        _ammoColor = (byte)_game.getMod().getInterface("buyMenu").getElement("ammoColor").color;

        add(_window, "window", "buyMenu");
        add(_btnOk, "button", "buyMenu");
        add(_btnCancel, "button", "buyMenu");
        add(_txtTitle, "text", "buyMenu");
        add(_txtFunds, "text", "buyMenu");
        add(_txtPurchases, "text", "buyMenu");
        add(_txtSpaceUsed, "text", "buyMenu");
        add(_txtCost, "text", "buyMenu");
        add(_txtQuantity, "text", "buyMenu");
        add(_lstItems, "list", "buyMenu");
        add(_cbxCategory, "text", "buyMenu");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK13.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

        _btnCancel.setText(tr("STR_CANCEL"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_PURCHASE_HIRE_PERSONNEL"));

        _txtFunds.setText(tr("STR_CURRENT_FUNDS").arg(Unicode.formatFunding(_game.getSavedGame().getFunds())));

        _txtPurchases.setText(tr("STR_COST_OF_PURCHASES").arg(Unicode.formatFunding(_total)));

        _txtSpaceUsed.setVisible(Options.storageLimitsEnforced);
        string ss = $"{_base.getUsedStores()}:{_base.getAvailableStores()}";
        _txtSpaceUsed.setText(tr("STR_SPACE_USED").arg(ss));

        _txtCost.setText(tr("STR_COST_PER_UNIT_UC"));

        _txtQuantity.setText(tr("STR_QUANTITY_UC"));

        _lstItems.setArrowColumn(227, ArrowOrientation.ARROW_VERTICAL);
        _lstItems.setColumns(4, 150, 55, 50, 32);
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

        List<string> soldiers = _game.getMod().getSoldiersList();
        foreach (var i in soldiers)
        {
            RuleSoldier rule = _game.getMod().getSoldier(i);
            if (rule.getBuyCost() != 0 && _game.getSavedGame().isResearched(rule.getRequirements()))
            {
                var row = new TransferRow { type = TransferType.TRANSFER_SOLDIER, rule = rule, name = tr(rule.getType()), cost = rule.getBuyCost(), qtySrc = _base.getSoldierCount(rule.getType()), qtyDst = 0, amount = 0 };
                _items.Add(row);
                string cat = getCategory(_items.Count - 1);
                if (!_cats.Contains(cat))
                {
                    _cats.Add(cat);
                }
            }
        }
        {
            var row = new TransferRow { type = TransferType.TRANSFER_SCIENTIST, rule = null, name = tr("STR_SCIENTIST"), cost = _game.getMod().getScientistCost() * 2, qtySrc = _base.getTotalScientists(), qtyDst = 0, amount = 0 };
            _items.Add(row);
            string cat = getCategory(_items.Count - 1);
            if (!_cats.Contains(cat))
            {
                _cats.Add(cat);
            }
        }
        {
            var row = new TransferRow { type = TransferType.TRANSFER_ENGINEER, rule = null, name = tr("STR_ENGINEER"), cost = _game.getMod().getEngineerCost() * 2, qtySrc = _base.getTotalEngineers(), qtyDst = 0, amount = 0 };
            _items.Add(row);
            string cat = getCategory(_items.Count - 1);
            if (!_cats.Contains(cat))
            {
                _cats.Add(cat);
            }
        }
        List<string> crafts = _game.getMod().getCraftsList();
        foreach (var i in crafts)
        {
            RuleCraft rule = _game.getMod().getCraft(i);
            if (rule.getBuyCost() != 0 && _game.getSavedGame().isResearched(rule.getRequirements()))
            {
                var row = new TransferRow { type = TransferType.TRANSFER_CRAFT, rule = rule, name = tr(rule.getType()), cost = rule.getBuyCost(), qtySrc = _base.getCraftCount(rule.getType()), qtyDst = 0, amount = 0 };
                _items.Add(row);
                string cat = getCategory(_items.Count - 1);
                if (!_cats.Contains(cat))
                {
                    _cats.Add(cat);
                }
            }
        }
        List<string> items = _game.getMod().getItemsList();
        foreach (var i in items)
        {
            RuleItem rule = _game.getMod().getItem(i);
            if (rule.getBuyCost() != 0 && _game.getSavedGame().isResearched(rule.getRequirements()))
            {
                var row = new TransferRow { type = TransferType.TRANSFER_ITEM, rule = rule, name = tr(rule.getType()), cost = rule.getBuyCost(), qtySrc = _base.getStorageItems().getItem(rule.getType()), qtyDst = 0, amount = 0 };
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
    ~PurchaseState()
    {
        _timerInc = null;
        _timerDec = null;
    }

    /**
     * Purchases the selected items.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        _game.getSavedGame().setFunds(_game.getSavedGame().getFunds() - _total);
        foreach (var i in _items)
        {
            if (i.amount > 0)
            {
                Transfer t = null;
                switch (i.type)
                {
                    case TransferType.TRANSFER_SOLDIER:
                        for (int s = 0; s < i.amount; s++)
                        {
                            RuleSoldier rule = (RuleSoldier)i.rule;
                            int time = rule.getTransferTime();
                            if (time == 0)
                                time = _game.getMod().getPersonnelTime();
                            t = new Transfer(time);
                            t.setSoldier(_game.getMod().genSoldier(_game.getSavedGame(), rule.getType()));
                            _base.getTransfers().Add(t);
                        }
                        break;
                    case TransferType.TRANSFER_SCIENTIST:
                        t = new Transfer(_game.getMod().getPersonnelTime());
                        t.setScientists(i.amount);
                        _base.getTransfers().Add(t);
                        break;
                    case TransferType.TRANSFER_ENGINEER:
                        t = new Transfer(_game.getMod().getPersonnelTime());
                        t.setEngineers(i.amount);
                        _base.getTransfers().Add(t);
                        break;
                    case TransferType.TRANSFER_CRAFT:
                        for (int c = 0; c < i.amount; c++)
                        {
                            RuleCraft rule = (RuleCraft)i.rule;
                            t = new Transfer(rule.getTransferTime());
                            Craft craft = new Craft(rule, _base, _game.getSavedGame().getId(rule.getType()));
                            craft.setStatus("STR_REFUELLING");
                            t.setCraft(craft);
                            _base.getTransfers().Add(t);
                        }
                        break;
                    case TransferType.TRANSFER_ITEM:
                        {
                            RuleItem rule = (RuleItem)i.rule;
                            t = new Transfer(rule.getTransferTime());
                            t.setItems(rule.getType(), i.amount);
                            _base.getTransfers().Add(t);
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
     * Increases the item by one on left-click,
     * to max on right-click.
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
     * Decreases the item by one on left-click,
     * to 0 on right-click.
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
     * Updates the production list to match the category filter.
     */
    void cbxCategoryChange(Action _) =>
        updateList();

    /**
     * Increases the quantity of the selected item to buy by one.
     */
    void increase()
    {
        _timerDec.setInterval(50);
        _timerInc.setInterval(50);
        increaseByValue(1);
    }

    /**
     * Decreases the quantity of the selected item to buy by one.
     */
    void decrease()
    {
        _timerInc.setInterval(50);
        _timerDec.setInterval(50);
        decreaseByValue(1);
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
            string ssQty = _items[i].qtySrc.ToString();
            string ssAmount = _items[i].amount.ToString();
            _lstItems.addRow(4, name, Unicode.formatFunding(_items[i].cost), ssQty, ssAmount);
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
     * Increases the quantity of the selected item to buy by "change".
     * @param change How much we want to add.
     */
    void increaseByValue(int change)
    {
        if (0 >= change) return;
        string errorMessage = null;

        if (_total + getRow().cost > _game.getSavedGame().getFunds())
        {
            errorMessage = tr("STR_NOT_ENOUGH_MONEY");
        }
        else
        {
            RuleItem rule = null;
            switch (getRow().type)
            {
                case TransferType.TRANSFER_SOLDIER:
                case TransferType.TRANSFER_SCIENTIST:
                case TransferType.TRANSFER_ENGINEER:
                    if (_pQty + 1 > _base.getAvailableQuarters() - _base.getUsedQuarters())
                    {
                        errorMessage = tr("STR_NOT_ENOUGH_LIVING_SPACE");
                    }
                    break;
                case TransferType.TRANSFER_CRAFT:
                    if (_cQty + 1 > _base.getAvailableHangars() - _base.getUsedHangars())
                    {
                        errorMessage = tr("STR_NO_FREE_HANGARS_FOR_PURCHASE");
                    }
                    break;
                case TransferType.TRANSFER_ITEM:
                    rule = (RuleItem)getRow().rule;
                    if (_base.storesOverfull(_iQty + rule.getSize()))
                    {
                        errorMessage = tr("STR_NOT_ENOUGH_STORE_SPACE");
                    }
                    break;
            }
        }

        if (string.IsNullOrEmpty(errorMessage))
        {
            int maxByMoney = (int)((_game.getSavedGame().getFunds() - _total) / getRow().cost);
            if (maxByMoney >= 0)
                change = Math.Min(maxByMoney, change);
            switch (getRow().type)
            {
                case TransferType.TRANSFER_SOLDIER:
                case TransferType.TRANSFER_SCIENTIST:
                case TransferType.TRANSFER_ENGINEER:
                    {
                        int maxByQuarters = _base.getAvailableQuarters() - _base.getUsedQuarters() - _pQty;
                        change = Math.Min(maxByQuarters, change);
                        _pQty += change;
                    }
                    break;
                case TransferType.TRANSFER_CRAFT:
                    {
                        int maxByHangars = _base.getAvailableHangars() - _base.getUsedHangars() - _cQty;
                        change = Math.Min(maxByHangars, change);
                        _cQty += change;
                    }
                    break;
                case TransferType.TRANSFER_ITEM:
                    {
                        RuleItem rule = (RuleItem)getRow().rule;
                        double storesNeededPerItem = rule.getSize();
                        double freeStores = _base.getAvailableStores() - _base.getUsedStores() - _iQty;
                        double maxByStores = (double)(int.MaxValue);
                        if (!AreSame(storesNeededPerItem, 0.0))
                        {
                            maxByStores = (freeStores + 0.05) / storesNeededPerItem;
                        }
                        change = Math.Min((int)maxByStores, change);
                        _iQty += change * storesNeededPerItem;
                    }
                    break;
            }
            getRow().amount += change;
            _total += getRow().cost * change;
            updateItemStrings();
        }
        else
        {
            _timerInc.stop();
            RuleInterface menuInterface = _game.getMod().getInterface("buyMenu");
            _game.pushState(new ErrorMessageState(errorMessage, _palette, (byte)menuInterface.getElement("errorMessage").color, "BACK13.SCR", menuInterface.getElement("errorPalette").color));
        }
    }

    /// Gets the row of the current selection.
    TransferRow getRow() =>
        _items[_rows[(int)_sel]];

    /**
     * Decreases the quantity of the selected item to buy by "change".
     * @param change how much we want to add.
     */
    void decreaseByValue(int change)
    {
        if (0 >= change || 0 >= getRow().amount) return;
        change = Math.Min(getRow().amount, change);

        RuleItem rule = null;
        switch (getRow().type)
        {
            case TransferType.TRANSFER_SOLDIER:
            case TransferType.TRANSFER_SCIENTIST:
            case TransferType.TRANSFER_ENGINEER:
                _pQty -= change;
                break;
            case TransferType.TRANSFER_CRAFT:
                _cQty -= change;
                break;
            case TransferType.TRANSFER_ITEM:
                rule = (RuleItem)getRow().rule;
                _iQty -= rule.getSize() * change;
                break;
        }
        getRow().amount -= change;
        _total -= getRow().cost * change;
        updateItemStrings();
    }

    /**
     * Updates the quantity-strings of the selected item.
     */
    void updateItemStrings()
    {
        _txtPurchases.setText(tr("STR_COST_OF_PURCHASES").arg(Unicode.formatFunding(_total)));
        var ss5 = new StringBuilder();
        string ss = getRow().amount.ToString();
        _lstItems.setCellText(_sel, 3, ss);
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
        ss5.Append(_base.getUsedStores());
        if (Math.Abs(_iQty) > 0.05)
        {
            ss5.Append("(");
            if (_iQty > 0.05)
                ss5.Append("+");
            ss5.Append($"{_iQty:N1})");
        }
        ss5.Append($":{_base.getAvailableStores()}");
        _txtSpaceUsed.setText(tr("STR_SPACE_USED").arg(ss5));
    }

    /**
    * Runs the arrow timers.
    */
    protected override void think()
    {
	    base.think();

	    _timerInc.think(this, null);
	    _timerDec.think(this, null);
    }
}
