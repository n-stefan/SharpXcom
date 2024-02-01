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

namespace SharpXcom.Battlescape;

/**
 * Interactive view of an inventory.
 * Lets the player view and manage a soldier's equipment.
 */
internal class Inventory : InteractiveSurface
{
    Game _game;
    BattleUnit _selUnit;
    BattleItem _selItem;
    bool _tu, _base;
    BattleItem _mouseOverItem;
    int _groundOffset, _animFrame;
    int _depth;
    Surface _grid, _items, _selection;
    WarningMessage _warning;
    NumberText _stackNumber;
    Timer _animTimer;
    List<KeyValuePair<int, int>> _grenadeIndicators;
    Dictionary<int, Dictionary<int, int>> _stackLevel;

    /**
     * Sets up an inventory with the specified size and position.
     * @param game Pointer to core game.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     * @param base Is the inventory being called from the basescape?
     */
    internal Inventory(Game game, int width, int height, int x, int y, bool @base) : base(width, height, x, y)
    {
        _game = game;
        _selUnit = null;
        _selItem = null;
        _tu = true;
        _base = @base;
        _mouseOverItem = null;
        _groundOffset = 0;
        _animFrame = 0;

        _depth = _game.getSavedGame().getSavedBattle().getDepth();
        _grid = new Surface(width, height, 0, 0);
        _items = new Surface(width, height, 0, 0);
        _selection = new Surface(RuleInventory.HAND_W * RuleInventory.SLOT_W, RuleInventory.HAND_H * RuleInventory.SLOT_H, x, y);
        _warning = new WarningMessage(224, 24, 48, 176);
        _stackNumber = new NumberText(15, 15, 0, 0);
        _stackNumber.setBordered(true);

        _warning.initText(_game.getMod().getFont("FONT_BIG"), _game.getMod().getFont("FONT_SMALL"), _game.getLanguage());
        _warning.setColor((byte)_game.getMod().getInterface("battlescape").getElement("warning").color2);
        _warning.setTextColor((byte)_game.getMod().getInterface("battlescape").getElement("warning").color);

        _animTimer = new Timer(125);
        _animTimer.onTimer((SurfaceHandler)drawPrimers);
        _animTimer.start();
    }

    /**
     * Deletes inventory surfaces.
     */
    ~Inventory()
    {
        _grid = null;
        _items = null;
        _selection = null;
        _warning = null;
        _stackNumber = null;
        _animTimer = null;
    }

    /**
     * Shows primer warnings on all live grenades.
     */
    void drawPrimers()
    {
        int[] Pulsate = { 0, 1, 2, 3, 4, 3, 2, 1 };
        if (_animFrame == 8)
        {
            _animFrame = 0;
        }
        Surface tempSurface = _game.getMod().getSurfaceSet("SCANG.DAT").getFrame(6);
        foreach (var i in _grenadeIndicators)
        {
            tempSurface.blitNShade(_items, i.Key, i.Value, Pulsate[_animFrame]);
        }
        _animFrame++;
    }

    /**
     * Checks if an item in a certain slot position would
     * overlap with any other inventory item.
     * @param unit Pointer to current unit.
     * @param item Pointer to battle item.
     * @param slot Inventory slot, or NULL if none.
     * @param x X position in slot.
     * @param y Y position in slot.
     * @return If there's overlap.
     */
    internal static bool overlapItems(BattleUnit unit, BattleItem item, RuleInventory slot, int x, int y)
    {
        if (slot.getType() != InventoryType.INV_GROUND)
        {
            foreach (var i in unit.getInventory())
            {
                if (i.getSlot() == slot && i.occupiesSlot(x, y, item))
                {
                    return true;
                }
            }
        }
        else if (unit.getTile() != null)
        {
            foreach (var i in unit.getTile().getInventory())
            {
                if (i.occupiesSlot(x, y, item))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /**
     * Returns the item currently grabbed by the player.
     * @return Pointer to selected item, or NULL if none.
     */
    internal BattleItem getSelectedItem() =>
	    _selItem;

    /**
     * Changes the inventory's Time Units mode.
     * When True, inventory actions cost soldier time units (for battle).
     * When False, inventory actions don't cost anything (for pre-equip).
     * @param tu Time Units mode.
     */
    internal void setTuMode(bool tu) =>
        _tu = tu;

    /**
     * Changes the unit to display the inventory of.
     * @param unit Pointer to battle unit.
     */
    internal void setSelectedUnit(BattleUnit unit)
    {
        _selUnit = unit;
        _groundOffset = 999;
        arrangeGround();
    }

    /**
     * Returns the item currently under mouse cursor.
     * @return Pointer to selected item, or 0 if none.
     */
    internal BattleItem getMouseOverItem() =>
	    _mouseOverItem;

    /**
     * Arranges items on the ground for the inventory display.
     * Since items on the ground aren't assigned to anyone,
     * they don't actually have permanent slot positions.
     * @param alterOffset Whether to alter the ground offset.
     */
    internal void arrangeGround(bool alterOffset = true)
    {
        RuleInventory ground = _game.getMod().getInventory("STR_GROUND", true);

        int slotsX = (Screen.ORIGINAL_WIDTH - ground.getX()) / RuleInventory.SLOT_W;
        int slotsY = (Screen.ORIGINAL_HEIGHT - ground.getY()) / RuleInventory.SLOT_H;
        int x = 0;
        int y = 0;
        bool ok = false;
        int xMax = 0;
        _stackLevel.Clear();

        if (_selUnit != null)
        {
            // first move all items out of the way - a big number in X direction
            foreach (var i in _selUnit.getTile().getInventory())
            {
                i.setSlot(ground);
                i.setSlotX(1000000);
                i.setSlotY(0);
            }

            // now for each item, find the most topleft position that is not occupied and will fit
            foreach (var i in _selUnit.getTile().getInventory())
            {
                x = 0;
                y = 0;
                ok = false;
                while (!ok)
                {
                    ok = true; // assume we can put the item here, if one of the following checks fails, we can't.
                    for (int xd = 0; xd < i.getRules().getInventoryWidth() && ok; xd++)
                    {
                        if ((x + xd) % slotsX < x % slotsX)
                        {
                            ok = false;
                        }
                        else
                        {
                            for (int yd = 0; yd < i.getRules().getInventoryHeight() && ok; yd++)
                            {
                                BattleItem item = _selUnit.getItem(ground, x + xd, y + yd);
                                ok = item == null;
                                if (canBeStacked(item, i))
                                {
                                    ok = true;
                                }
                            }
                        }
                    }
                    if (ok)
                    {
                        i.setSlotX(x);
                        i.setSlotY(y);
                        // only increase the stack level if the item is actually visible.
                        if (i.getRules().getInventoryWidth() != 0)
                        {
                            _stackLevel[x][y] += 1;
                        }
                        xMax = Math.Max(xMax, x + i.getRules().getInventoryWidth());
                    }
                    else
                    {
                        y++;
                        if (y > slotsY - i.getRules().getInventoryHeight())
                        {
                            y = 0;
                            x++;
                        }
                    }
                }
            }
        }
        if (alterOffset)
        {
            if (xMax >= _groundOffset + slotsX)
            {
                _groundOffset += slotsX;
            }
            else
            {
                _groundOffset = 0;
            }
        }
        drawItems();
    }

    /**
     * Checks if two items can be stacked on one another.
     * @param itemA First item.
     * @param itemB Second item.
     * @return True, if the items can be stacked on one another.
     */
    bool canBeStacked(BattleItem itemA, BattleItem itemB)
    {
        //both items actually exist
        return (itemA != null && itemB != null &&
            //both items have the same ruleset
            itemA.getRules() == itemB.getRules() &&
            // either they both have no ammo
            ((itemA.getAmmoItem() == null && itemB.getAmmoItem() == null) ||
            // or they both have ammo
            (itemA.getAmmoItem() != null && itemB.getAmmoItem() != null &&
            // and the same ammo type
            itemA.getAmmoItem().getRules() == itemB.getAmmoItem().getRules() &&
            // and the same ammo quantity
            itemA.getAmmoItem().getAmmoQuantity() == itemB.getAmmoItem().getAmmoQuantity())) &&
            // and neither is set to explode
            itemA.getFuseTimer() == -1 && itemB.getFuseTimer() == -1 &&
            // and neither is a corpse or unconscious unit
            itemA.getUnit() == null && itemB.getUnit() == null &&
            // and if it's a medkit, it has the same number of charges
            itemA.getPainKillerQuantity() == itemB.getPainKillerQuantity() &&
            itemA.getHealQuantity() == itemB.getHealQuantity() &&
            itemA.getStimulantQuantity() == itemB.getStimulantQuantity());
    }

    /**
     * Shows a warning message.
     * @param msg The message to show.
     */
    internal void showWarning(string msg) =>
	    _warning.showMessage(msg);

    /**
     * Unloads the selected weapon, placing the gun
     * on the right hand and the ammo on the left hand.
     * @return The success of the weapon being unloaded.
     */
    internal bool unload()
    {
        // Must be holding an item
        if (_selItem == null)
        {
            return false;
        }

        // Item must be loaded
        if (_selItem.getAmmoItem() == null && _selItem.getRules().getCompatibleAmmo().Any())
        {
            _warning.showMessage(_game.getLanguage().getString("STR_NO_AMMUNITION_LOADED"));
        }
        if (_selItem.getAmmoItem() == null || !_selItem.needsAmmo())
        {
            return false;
        }

        // Hands must be free
        foreach (var i in _selUnit.getInventory())
        {
            if (i.getSlot().getType() == InventoryType.INV_HAND && i != _selItem)
            {
                _warning.showMessage(_game.getLanguage().getString("STR_BOTH_HANDS_MUST_BE_EMPTY"));
                return false;
            }
        }

        if (!_tu || _selUnit.spendTimeUnits(8))
        {
            moveItem(_selItem.getAmmoItem(), _game.getMod().getInventory("STR_LEFT_HAND", true), 0, 0);
            _selItem.getAmmoItem().moveToOwner(_selUnit);
            moveItem(_selItem, _game.getMod().getInventory("STR_RIGHT_HAND", true), 0, 0);
            _selItem.moveToOwner(_selUnit);
            _selItem.setAmmoItem(null);
            setSelectedItem(null);
        }
        else
        {
            _warning.showMessage(_game.getLanguage().getString("STR_NOT_ENOUGH_TIME_UNITS"));
            return false;
        }

        return true;
    }

    /**
     * Changes the item currently grabbed by the player.
     * @param item Pointer to selected item, or NULL if none.
     */
    void setSelectedItem(BattleItem item)
    {
        _selItem = (item != null && !item.getRules().isFixed()) ? item : null;
        if (_selItem == null)
        {
            _selection.clear();
        }
        else
        {
            if (_selItem.getSlot().getType() == InventoryType.INV_GROUND)
            {
                _stackLevel[_selItem.getSlotX()][_selItem.getSlotY()] -= 1;
            }
            _selItem.getRules().drawHandSprite(_game.getMod().getSurfaceSet("BIGOBS.PCK"), _selection);
        }
        drawItems();
    }

    /**
     * Moves an item to a specified slot in the
     * selected player's inventory.
     * @param item Pointer to battle item.
     * @param slot Inventory slot, or NULL if none.
     * @param x X position in slot.
     * @param y Y position in slot.
     */
    void moveItem(BattleItem item, RuleInventory slot, int x, int y)
    {
        // Make items vanish (eg. ammo in weapons)
        if (slot == null)
        {
            if (item.getSlot().getType() == InventoryType.INV_GROUND)
            {
                _selUnit.getTile().removeItem(item);
            }
            else
            {
                item.moveToOwner(null);
            }
        }
        else
        {
            // Handle dropping from/to ground.
            if (slot != item.getSlot())
            {
                if (slot.getType() == InventoryType.INV_GROUND)
                {
                    item.moveToOwner(null);
                    _selUnit.getTile().addItem(item, item.getSlot());
                    if (item.getUnit() != null && item.getUnit().getStatus() == UnitStatus.STATUS_UNCONSCIOUS)
                    {
                        item.getUnit().setPosition(_selUnit.getPosition());
                    }
                }
                else if (item.getSlot() == null || item.getSlot().getType() == InventoryType.INV_GROUND)
                {
                    item.moveToOwner(_selUnit);
                    _selUnit.getTile().removeItem(item);
                    item.setTurnFlag(false);
                    if (item.getUnit() != null && item.getUnit().getStatus() == UnitStatus.STATUS_UNCONSCIOUS)
                    {
                        item.getUnit().setPosition(new Position(-1, -1, -1));
                    }
                }
            }
            item.setSlot(slot);
            item.setSlotX(x);
            item.setSlotY(y);
        }
    }

    /**
     * Draws the items contained in the soldier's inventory.
     */
    void drawItems()
    {
        _items.clear();
        _grenadeIndicators.Clear();
        byte color = (byte)_game.getMod().getInterface("inventory").getElement("numStack").color;
        if (_selUnit != null)
        {
            SurfaceSet texture = _game.getMod().getSurfaceSet("BIGOBS.PCK");
            // Soldier items
            foreach (var i in _selUnit.getInventory())
            {
                if (i == _selItem)
                    continue;

                Surface frame = texture.getFrame(i.getRules().getBigSprite());
                if (i.getSlot().getType() == InventoryType.INV_SLOT)
                {
                    frame.setX(i.getSlot().getX() + i.getSlotX() * RuleInventory.SLOT_W);
                    frame.setY(i.getSlot().getY() + i.getSlotY() * RuleInventory.SLOT_H);
                }
                else if (i.getSlot().getType() == InventoryType.INV_HAND)
                {
                    frame.setX(i.getSlot().getX() + (RuleInventory.HAND_W - i.getRules().getInventoryWidth()) * RuleInventory.SLOT_W / 2);
                    frame.setY(i.getSlot().getY() + (RuleInventory.HAND_H - i.getRules().getInventoryHeight()) * RuleInventory.SLOT_H / 2);
                }
                texture.getFrame(i.getRules().getBigSprite()).blit(_items);

                // grenade primer indicators
                if (i.getFuseTimer() >= 0)
                {
                    _grenadeIndicators.Add(KeyValuePair.Create(frame.getX(), frame.getY()));
                }
            }
            Surface stackLayer = new Surface(getWidth(), getHeight(), 0, 0);
            stackLayer.setPalette(getPaletteColors());
            // Ground items
            foreach (var i in _selUnit.getTile().getInventory())
            {
                Surface frame = texture.getFrame(i.getRules().getBigSprite());
                // note that you can make items invisible by setting their width or height to 0 (for example used with tank corpse items)
                if (i == _selItem || i.getSlotX() < _groundOffset || i.getRules().getInventoryHeight() == 0 || i.getRules().getInventoryWidth() == 0 || frame == null)
                    continue;
                frame.setX(i.getSlot().getX() + (i.getSlotX() - _groundOffset) * RuleInventory.SLOT_W);
                frame.setY(i.getSlot().getY() + i.getSlotY() * RuleInventory.SLOT_H);
                texture.getFrame(i.getRules().getBigSprite()).blit(_items);

                // grenade primer indicators
                if (i.getFuseTimer() >= 0)
                {
                    _grenadeIndicators.Add(KeyValuePair.Create(frame.getX(), frame.getY()));
                }

                // item stacking
                if (_stackLevel[i.getSlotX()][i.getSlotY()] > 1)
                {
                    _stackNumber.setX((i.getSlot().getX() + ((i.getSlotX() + i.getRules().getInventoryWidth()) - _groundOffset) * RuleInventory.SLOT_W) - 4);
                    if (_stackLevel[i.getSlotX()][i.getSlotY()] > 9)
                    {
                        _stackNumber.setX(_stackNumber.getX() - 4);
                    }
                    _stackNumber.setY((i.getSlot().getY() + (i.getSlotY() + i.getRules().getInventoryHeight()) * RuleInventory.SLOT_H) - 6);
                    _stackNumber.setValue((uint)_stackLevel[i.getSlotX()][i.getSlotY()]);
                    _stackNumber.draw();
                    _stackNumber.setColor(color);
                    _stackNumber.blit(stackLayer);
                }
            }

            stackLayer.blit(_items);
            stackLayer = null;
        }
    }

    /**
     * Handles timers.
     */
    protected override void think()
    {
	    _warning.think();
	    _animTimer.think(null, this);
    }

    /**
     * Draws the inventory elements.
     */
    protected override void draw()
    {
	    drawGrid();
	    drawItems();
    }

    /**
     * Draws the inventory grid for item placement.
     */
    void drawGrid()
    {
	    _grid.clear();
	    Text text = new Text(80, 9, 0, 0);
	    text.setPalette(_grid.getPaletteColors());
	    text.initText(_game.getMod().getFont("FONT_BIG"), _game.getMod().getFont("FONT_SMALL"), _game.getLanguage());

	    RuleInterface rule = _game.getMod().getInterface("inventory");

	    text.setColor((byte)rule.getElement("textSlots").color);
	    text.setHighContrast(true);

	    byte color = (byte)rule.getElement("grid").color;

	    foreach (var i in _game.getMod().getInventories())
	    {
		    // Draw grid
		    if (i.Value.getType() == InventoryType.INV_SLOT)
		    {
			    foreach (var j in i.Value.getSlots())
			    {
				    SDL_Rect r;
				    r.x = i.Value.getX() + RuleInventory.SLOT_W * j.x;
				    r.y = i.Value.getY() + RuleInventory.SLOT_H * j.y;
				    r.w = RuleInventory.SLOT_W + 1;
				    r.h = RuleInventory.SLOT_H + 1;
				    _grid.drawRect(ref r, color);
				    r.x++;
				    r.y++;
				    r.w -= 2;
				    r.h -= 2;
				    _grid.drawRect(ref r, 0);
			    }
		    }
		    else if (i.Value.getType() == InventoryType.INV_HAND)
		    {
			    SDL_Rect r;
			    r.x = i.Value.getX();
			    r.y = i.Value.getY();
			    r.w = RuleInventory.HAND_W * RuleInventory.SLOT_W;
			    r.h = RuleInventory.HAND_H * RuleInventory.SLOT_H;
			    _grid.drawRect(ref r, color);
			    r.x++;
			    r.y++;
			    r.w -= 2;
			    r.h -= 2;
			    _grid.drawRect(ref r, 0);
		    }
		    else if (i.Value.getType() == InventoryType.INV_GROUND)
		    {
			    for (int x = i.Value.getX(); x <= 320; x += RuleInventory.SLOT_W)
			    {
				    for (int y = i.Value.getY(); y <= 200; y += RuleInventory.SLOT_H)
				    {
					    SDL_Rect r;
					    r.x = x;
					    r.y = y;
					    r.w = RuleInventory.SLOT_W + 1;
					    r.h = RuleInventory.SLOT_H + 1;
					    _grid.drawRect(ref r, color);
					    r.x++;
					    r.y++;
					    r.w -= 2;
					    r.h -= 2;
					    _grid.drawRect(ref r, 0);
				    }
			    }
		    }

		    // Draw label
		    text.setX(i.Value.getX());
		    text.setY(i.Value.getY() - text.getFont().getHeight() - text.getFont().getSpacing());
		    text.setText(_game.getLanguage().getString(i.Value.getId()));
		    text.blit(_grid);
	    }
    }

    /**
     * Blits the inventory elements.
     * @param surface Pointer to surface to blit onto.
     */
    protected override void blit(Surface surface)
    {
	    clear();
	    _grid.blit(this);
	    _items.blit(this);
	    _selection.blit(this);
	    _warning.blit(this);
	    base.blit(surface);
    }
}
