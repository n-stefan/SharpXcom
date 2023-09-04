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
 * Equipment screen that lets the player
 * pick the equipment to carry on a craft.
 */
internal class CraftEquipmentState : State
{
    uint _sel, _craft;
    Base _base;
    int _totalItems;
    byte _ammoColor;
    Window _window;
    TextButton _btnOk, _btnClear, _btnInventory;
    Text _txtTitle, _txtItem, _txtStores, _txtAvailable, _txtUsed, _txtCrew;
    TextList _lstEquipment;
    List<string> _items;
    Timer _timerLeft, _timerRight;

    /**
     * Initializes all the elements in the Craft Equipment screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param craft ID of the selected craft.
     */
    internal CraftEquipmentState(Base @base, uint craft)
    {
        _sel = 0;
        _craft = craft;
        _base = @base;
        _totalItems = 0;
        _ammoColor = 0;

        Craft c = _base.getCrafts()[(int)_craft];
        bool craftHasACrew = c.getNumSoldiers() > 0;
        bool isNewBattle = _game.getSavedGame().getMonthsPassed() == -1;

        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        _btnOk = new TextButton((craftHasACrew || isNewBattle) ? 148 : 288, 16, (craftHasACrew || isNewBattle) ? 164 : 16, 176);
        _btnClear = new TextButton(148, 16, 8, 176);
        _btnInventory = new TextButton(148, 16, 8, 176);
        _txtTitle = new Text(300, 17, 16, 7);
        _txtItem = new Text(144, 9, 16, 32);
        _txtStores = new Text(150, 9, 160, 32);
        _txtAvailable = new Text(110, 9, 16, 24);
        _txtUsed = new Text(110, 9, 130, 24);
        _txtCrew = new Text(71, 9, 244, 24);
        _lstEquipment = new TextList(288, 128, 8, 40);

        // Set palette
        setInterface("craftEquipment");

        _ammoColor = (byte)_game.getMod().getInterface("craftEquipment").getElement("ammoColor").color;

        add(_window, "window", "craftEquipment");
        add(_btnOk, "button", "craftEquipment");
        add(_btnClear, "button", "craftEquipment");
        add(_btnInventory, "button", "craftEquipment");
        add(_txtTitle, "text", "craftEquipment");
        add(_txtItem, "text", "craftEquipment");
        add(_txtStores, "text", "craftEquipment");
        add(_txtAvailable, "text", "craftEquipment");
        add(_txtUsed, "text", "craftEquipment");
        add(_txtCrew, "text", "craftEquipment");
        add(_lstEquipment, "list", "craftEquipment");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK04.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _btnClear.setText(tr("STR_UNLOAD_CRAFT"));
        _btnClear.onMouseClick(btnClearClick);
        _btnClear.setVisible(isNewBattle);

        _btnInventory.setText(tr("STR_INVENTORY"));
        _btnInventory.onMouseClick(btnInventoryClick);
        _btnInventory.setVisible(craftHasACrew && !isNewBattle);

        _txtTitle.setBig();
        _txtTitle.setText(tr("STR_EQUIPMENT_FOR_CRAFT").arg(c.getName(_game.getLanguage())));

        _txtItem.setText(tr("STR_ITEM"));

        _txtStores.setText(tr("STR_STORES"));

        _txtAvailable.setText(tr("STR_SPACE_AVAILABLE").arg(c.getSpaceAvailable()));

        _txtUsed.setText(tr("STR_SPACE_USED").arg(c.getSpaceUsed()));

        string ss3 = $"{tr("STR_SOLDIERS_UC")}>{Unicode.TOK_COLOR_FLIP}{c.getNumSoldiers()}";
        _txtCrew.setText(ss3);

        _lstEquipment.setArrowColumn(203, ArrowOrientation.ARROW_HORIZONTAL);
        _lstEquipment.setColumns(3, 156, 83, 41);
        _lstEquipment.setSelectable(true);
        _lstEquipment.setBackground(_window);
        _lstEquipment.setMargin(8);
        _lstEquipment.onLeftArrowPress(lstEquipmentLeftArrowPress);
        _lstEquipment.onLeftArrowRelease(lstEquipmentLeftArrowRelease);
        _lstEquipment.onLeftArrowClick(lstEquipmentLeftArrowClick);
        _lstEquipment.onRightArrowPress(lstEquipmentRightArrowPress);
        _lstEquipment.onRightArrowRelease(lstEquipmentRightArrowRelease);
        _lstEquipment.onRightArrowClick(lstEquipmentRightArrowClick);
        _lstEquipment.onMousePress(lstEquipmentMousePress);

        uint row = 0;
        List<string> items = _game.getMod().getItemsList();
        foreach (var i in items)
        {
            RuleItem rule = _game.getMod().getItem(i);
            int cQty = 0;
            if (rule.isFixed())
            {
                cQty = c.getVehicleCount(i);
            }
            else
            {
                cQty = c.getItems().getItem(i);
                _totalItems += cQty;
            }
            if (rule.getBigSprite() > -1 && rule.getBattleType() != BattleType.BT_NONE && rule.getBattleType() != BattleType.BT_CORPSE &&
                _game.getSavedGame().isResearched(rule.getRequirements()) &&
                (_base.getStorageItems().getItem(i) > 0 || cQty > 0))
            {
                _items.Add(i);
                string ss, ss2;
                if (_game.getSavedGame().getMonthsPassed() > -1)
                {
                    ss = _base.getStorageItems().getItem(i).ToString();
                }
                else
                {
                    ss = "-";
                }
                ss2 = cQty.ToString();

                string s = tr(i);
                if (rule.getBattleType() == BattleType.BT_AMMO)
                {
                    s = s.Insert(0, "  ");
                }
                _lstEquipment.addRow(3, s, ss, ss2);

                byte color;
                if (cQty == 0)
                {
                    if (rule.getBattleType() == BattleType.BT_AMMO)
                    {
                        color = _ammoColor;
                    }
                    else
                    {
                        color = _lstEquipment.getColor();
                    }
                }
                else
                {
                    color = _lstEquipment.getSecondaryColor();
                }
                _lstEquipment.setRowColor(row, color);

                ++row;
            }
        }

        _timerLeft = new Timer(250);
        _timerLeft.onTimer((StateHandler)moveLeft);
        _timerRight = new Timer(250);
        _timerRight.onTimer((StateHandler)moveRight);
    }

    /**
     *
     */
    ~CraftEquipmentState()
    {
        _timerLeft = null;
        _timerRight = null;
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();

    /**
     * Empties the contents of the craft, moving all of the items back to the base.
     */
    void btnClearClick(Action _)
    {
        for (_sel = 0; _sel != _items.Count; ++_sel)
        {
            moveLeftByValue(int.MaxValue);
        }
    }

    /**
     * Displays the inventory screen for the soldiers
     * inside the craft.
     * @param action Pointer to an action.
     */
    void btnInventoryClick(Action _)
    {
        Craft craft = _base.getCrafts()[(int)_craft];
        if (craft.getNumSoldiers() != 0)
        {
            SavedBattleGame bgame = new SavedBattleGame();
            _game.getSavedGame().setBattleGame(bgame);

            BattlescapeGenerator bgen = new BattlescapeGenerator(_game);
            bgen.runInventory(craft);

            _game.getScreen().clear();
            _game.pushState(new InventoryState(false, null));
        }
    }

    /**
     * Starts moving the item to the base.
     * @param action Pointer to an action.
     */
    void lstEquipmentLeftArrowPress(Action action)
    {
        _sel = _lstEquipment.getSelectedRow();
        if (action.getDetails().button.button == SDL_BUTTON_LEFT && !_timerLeft.isRunning()) _timerLeft.start();
    }

    /**
     * Stops moving the item to the base.
     * @param action Pointer to an action.
     */
    void lstEquipmentLeftArrowRelease(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            _timerLeft.stop();
        }
    }

    /**
     * Moves all the items to the base on right-click.
     * @param action Pointer to an action.
     */
    void lstEquipmentLeftArrowClick(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_RIGHT) moveLeftByValue(int.MaxValue);
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            moveLeftByValue(1);
            _timerRight.setInterval(250);
            _timerLeft.setInterval(250);
        }
    }

    /**
     * Starts moving the item to the craft.
     * @param action Pointer to an action.
     */
    void lstEquipmentRightArrowPress(Action action)
    {
        _sel = _lstEquipment.getSelectedRow();
        if (action.getDetails().button.button == SDL_BUTTON_LEFT && !_timerRight.isRunning()) _timerRight.start();
    }

    /**
     * Stops moving the item to the craft.
     * @param action Pointer to an action.
     */
    void lstEquipmentRightArrowRelease(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            _timerRight.stop();
        }
    }

    /**
     * Moves all the items (as much as possible) to the craft on right-click.
     * @param action Pointer to an action.
     */
    void lstEquipmentRightArrowClick(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_RIGHT) moveRightByValue(int.MaxValue);
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            moveRightByValue(1);
            _timerRight.setInterval(250);
            _timerLeft.setInterval(250);
        }
    }

    /**
     * Handles the mouse-wheels on the arrow-buttons.
     * @param action Pointer to an action.
     */
    void lstEquipmentMousePress(Action action)
    {
        _sel = _lstEquipment.getSelectedRow();
        if (action.getDetails().wheel.y > 0) //button.button == SDL_BUTTON_WHEELUP
        {
            _timerRight.stop();
            _timerLeft.stop();
            if (action.getAbsoluteXMouse() >= _lstEquipment.getArrowsLeftEdge() &&
                action.getAbsoluteXMouse() <= _lstEquipment.getArrowsRightEdge())
            {
                moveRightByValue(Options.changeValueByMouseWheel);
            }
        }
        else if (action.getDetails().wheel.y < 0) //button.button == SDL_BUTTON_WHEELDOWN
        {
            _timerRight.stop();
            _timerLeft.stop();
            if (action.getAbsoluteXMouse() >= _lstEquipment.getArrowsLeftEdge() &&
                action.getAbsoluteXMouse() <= _lstEquipment.getArrowsRightEdge())
            {
                moveLeftByValue(Options.changeValueByMouseWheel);
            }
        }
    }

    /**
     * Moves the selected item to the base.
     */
    void moveLeft()
    {
        _timerLeft.setInterval(50);
        _timerRight.setInterval(50);
        moveLeftByValue(1);
    }

    /**
     * Moves the selected item to the craft.
     */
    void moveRight()
    {
        _timerLeft.setInterval(50);
        _timerRight.setInterval(50);
        moveRightByValue(1);
    }

    /**
     * Moves the given number of items (selected) to the base.
     * @param change Item difference.
     */
    void moveLeftByValue(int change)
    {
        Craft c = _base.getCrafts()[(int)_craft];
        RuleItem item = _game.getMod().getItem(_items[(int)_sel], true);
        int cQty = 0;
        if (item.isFixed()) cQty = c.getVehicleCount(_items[(int)_sel]);
        else cQty = c.getItems().getItem(_items[(int)_sel]);
        if (change <= 0 || cQty <= 0) return;
        change = Math.Min(cQty, change);
        // Convert vehicle to item
        if (item.isFixed())
        {
            if (item.getCompatibleAmmo().Any())
            {
                // Calculate how much ammo needs to be added to the base.
                RuleItem ammo = _game.getMod().getItem(item.getCompatibleAmmo().First(), true);
                int ammoPerVehicle;
                if (ammo.getClipSize() > 0 && item.getClipSize() > 0)
                {
                    ammoPerVehicle = item.getClipSize() / ammo.getClipSize();
                }
                else
                {
                    ammoPerVehicle = ammo.getClipSize();
                }
                // Put the vehicles and their ammo back as separate items.
                if (_game.getSavedGame().getMonthsPassed() != -1)
                {
                    _base.getStorageItems().addItem(_items[(int)_sel], change);
                    _base.getStorageItems().addItem(ammo.getType(), ammoPerVehicle * change);
                }
                // now delete the vehicles from the craft.
                var vehicles = c.getVehicles();
                for (var i = 0; i < vehicles.Count && change > 0;)
                {
                    if (vehicles[i].getRules() == item)
                    {
                        c.getVehicles().RemoveAt(i);
                        --change;
                    }
                    else ++i;
                }
            }
            else
            {
                if (_game.getSavedGame().getMonthsPassed() != -1)
                {
                    _base.getStorageItems().addItem(_items[(int)_sel], change);
                }
                var vehicles = c.getVehicles();
                for (var i = 0; i < vehicles.Count && change > 0;)
                {
                    if (vehicles[i].getRules() == item)
                    {
                        c.getVehicles().RemoveAt(i);
                        --change;
                    }
                    else ++i;
                }
            }
        }
        else
        {
            c.getItems().removeItem(_items[(int)_sel], change);
            _totalItems -= change;
            if (_game.getSavedGame().getMonthsPassed() > -1)
            {
                _base.getStorageItems().addItem(_items[(int)_sel], change);
            }
        }
        updateQuantity();
    }

    /**
     * Moves the given number of items (selected) to the craft.
     * @param change Item difference.
     */
    void moveRightByValue(int change)
    {
        Craft c = _base.getCrafts()[(int)_craft];
        RuleItem item = _game.getMod().getItem(_items[(int)_sel], true);
        int bqty = _base.getStorageItems().getItem(_items[(int)_sel]);
        if (_game.getSavedGame().getMonthsPassed() == -1)
        {
            if (change == int.MaxValue)
            {
                change = 10;
            }
            bqty = change;
        }
        if (0 >= change || 0 >= bqty) return;
        change = Math.Min(bqty, change);
        // Do we need to convert item to vehicle?
        if (item.isFixed())
        {
            int size = 4;
            if (_game.getMod().getUnit(item.getType()) != null)
            {
                size = _game.getMod().getArmor(_game.getMod().getUnit(item.getType()).getArmor(), true).getSize();
                size *= size;
            }
            // Check if there's enough room
            int room = Math.Min(c.getRules().getVehicles() - c.getNumVehicles(), c.getSpaceAvailable() / size);
            if (room > 0)
            {
                change = Math.Min(room, change);
                if (item.getCompatibleAmmo().Any())
                {
                    // And now let's see if we can add the total number of vehicles.
                    RuleItem ammo = _game.getMod().getItem(item.getCompatibleAmmo().First(), true);
                    int ammoPerVehicle, clipSize;
                    if (ammo.getClipSize() > 0 && item.getClipSize() > 0)
                    {
                        clipSize = item.getClipSize();
                        ammoPerVehicle = clipSize / ammo.getClipSize();
                    }
                    else
                    {
                        clipSize = ammo.getClipSize();
                        ammoPerVehicle = clipSize;
                    }

                    int baseQty = _base.getStorageItems().getItem(ammo.getType()) / ammoPerVehicle;
                    if (_game.getSavedGame().getMonthsPassed() == -1)
                        baseQty = change;
                    int canBeAdded = Math.Min(change, baseQty);
                    if (canBeAdded > 0)
                    {
                        for (int i = 0; i < canBeAdded; ++i)
                        {
                            if (_game.getSavedGame().getMonthsPassed() != -1)
                            {
                                _base.getStorageItems().removeItem(ammo.getType(), ammoPerVehicle);
                                _base.getStorageItems().removeItem(_items[(int)_sel]);
                            }
                            c.getVehicles().Add(new Vehicle(item, clipSize, size));
                        }
                    }
                    else
                    {
                        // So we haven't managed to increase the count of vehicles because of the ammo
                        _timerRight.stop();
                        var msg = new LocalizedText(tr("STR_NOT_ENOUGH_AMMO_TO_ARM_HWP").arg(ammoPerVehicle).arg(tr(ammo.getType())));
                        _game.pushState(new ErrorMessageState(msg, _palette, (byte)_game.getMod().getInterface("craftEquipment").getElement("errorMessage").color, "BACK04.SCR", _game.getMod().getInterface("craftEquipment").getElement("errorPalette").color));
                    }
                }
                else
                    for (int i = 0; i < change; ++i)
                    {
                        c.getVehicles().Add(new Vehicle(item, item.getClipSize(), size));
                        if (_game.getSavedGame().getMonthsPassed() != -1)
                        {
                            _base.getStorageItems().removeItem(_items[(int)_sel]);
                        }
                    }
            }
        }
        else
        {
            if (c.getRules().getMaxItems() > 0 && _totalItems + change > c.getRules().getMaxItems())
            {
                _timerRight.stop();
                var msg = new LocalizedText(tr("STR_NO_MORE_EQUIPMENT_ALLOWED", (uint)c.getRules().getMaxItems()));
                _game.pushState(new ErrorMessageState(msg, _palette, (byte)_game.getMod().getInterface("craftEquipment").getElement("errorMessage").color, "BACK04.SCR", _game.getMod().getInterface("craftEquipment").getElement("errorPalette").color));
                change = c.getRules().getMaxItems() - _totalItems;
            }
            c.getItems().addItem(_items[(int)_sel], change);
            _totalItems += change;
            if (_game.getSavedGame().getMonthsPassed() > -1)
            {
                _base.getStorageItems().removeItem(_items[(int)_sel], change);
            }
        }
        updateQuantity();
    }

    /**
     * Updates the displayed quantities of the
     * selected item on the list.
     */
    void updateQuantity()
    {
        Craft c = _base.getCrafts()[(int)_craft];
        RuleItem item = _game.getMod().getItem(_items[(int)_sel], true);
        int cQty = 0;
        if (item.isFixed())
        {
            cQty = c.getVehicleCount(_items[(int)_sel]);
        }
        else
        {
            cQty = c.getItems().getItem(_items[(int)_sel]);
        }
        string ss, ss2;
        if (_game.getSavedGame().getMonthsPassed() > -1)
        {
            ss = _base.getStorageItems().getItem(_items[(int)_sel]).ToString();
        }
        else
        {
            ss = "-";
        }
        ss2 = cQty.ToString();

        byte color;
        if (cQty == 0)
        {
            RuleItem rule = _game.getMod().getItem(_items[(int)_sel], true);
            if (rule.getBattleType() == BattleType.BT_AMMO)
            {
                color = _ammoColor;
            }
            else
            {
                color = _lstEquipment.getColor();
            }
        }
        else
        {
            color = _lstEquipment.getSecondaryColor();
        }
        _lstEquipment.setRowColor(_sel, color);
        _lstEquipment.setCellText(_sel, 1, ss);
        _lstEquipment.setCellText(_sel, 2, ss2);

        _txtAvailable.setText(tr("STR_SPACE_AVAILABLE").arg(c.getSpaceAvailable()));
        _txtUsed.setText(tr("STR_SPACE_USED").arg(c.getSpaceUsed()));
    }

    /**
     * Resets the savegame when coming back from the inventory.
     */
    protected override void init()
    {
	    base.init();

	    _game.getSavedGame().setBattleGame(null);

	    Craft c = _base.getCrafts()[(int)_craft];
	    c.setInBattlescape(false);
    }
}
