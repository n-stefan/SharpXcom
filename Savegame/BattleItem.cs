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

namespace SharpXcom.Savegame;

/**
 * Represents a single item in the battlescape.
 * Contains battle-related info about an item like the position, ammo quantity, ...
 * @sa RuleItem
 * @sa Item
 */
internal class BattleItem
{
    int _id;
    RuleItem _rules;
    BattleUnit _owner, _previousOwner;
    BattleUnit _unit;
    Tile _tile;
    RuleInventory _inventorySlot;
    int _inventoryX, _inventoryY;
    BattleItem _ammoItem;
    int _fuseTimer, _ammoQuantity;
    int _painKiller, _heal, _stimulant;
    bool _XCOMProperty, _droppedOnAlienTurn, _isAmmo;

    /**
     * Initializes a item of the specified type.
     * @param rules Pointer to ruleset.
     * @param id The id of the item.
     */
    internal BattleItem(RuleItem rules, ref int id)
    {
        _id = id;
        _rules = rules;
        _owner = null;
        _previousOwner = null;
        _unit = null;
        _tile = null;
        _inventorySlot = null;
        _inventoryX = 0;
        _inventoryY = 0;
        _ammoItem = null;
        _fuseTimer = -1;
        _ammoQuantity = 0;
        _painKiller = 0;
        _heal = 0;
        _stimulant = 0;
        _XCOMProperty = false;
        _droppedOnAlienTurn = false;
        _isAmmo = false;

        id++;
        if (_rules != null)
        {
            setAmmoQuantity(_rules.getClipSize());
            if (_rules.getBattleType() == BattleType.BT_MEDIKIT)
            {
                setHealQuantity(_rules.getHealQuantity());
                setPainKillerQuantity(_rules.getPainKillerQuantity());
                setStimulantQuantity(_rules.getStimulantQuantity());
            }

            // weapon does not need ammo, ammo item points to weapon
            else if ((_rules.getBattleType() == BattleType.BT_FIREARM || _rules.getBattleType() == BattleType.BT_MELEE) && !_rules.getCompatibleAmmo().Any())
            {
                _ammoItem = this;
            }
        }
    }

    /**
     *
     */
    ~BattleItem() { }

    /**
     * Changes the quantity of ammo in this item.
     * @param qty Ammo quantity.
     */
    internal void setAmmoQuantity(int qty) =>
        _ammoQuantity = qty;

    /**
     * Sets the heal quantity of the item.
     * @param heal The new heal quantity.
     */
    internal void setHealQuantity(int heal) =>
        _heal = heal;

    /**
     * Sets the pain killer quantity of the item.
     * @param pk The new pain killer quantity.
     */
    internal void setPainKillerQuantity(int pk) =>
        _painKiller = pk;

    /**
     * Sets the stimulant quantity of the item.
     * @param stimulant The new stimulant quantity.
     */
    internal void setStimulantQuantity(int stimulant) =>
        _stimulant = stimulant;

    /**
     * Gets the ruleset for the item's type.
     * @return Pointer to ruleset.
     */
    internal RuleItem getRules() =>
	    _rules;

    /**
     * Gets the item's ammo item.
     * @return The ammo item.
     */
    internal BattleItem getAmmoItem() =>
        _ammoItem;

    /**
     * Gets the quantity of ammo in this item.
     * @return Ammo quantity.
     */
    internal int getAmmoQuantity()
    {
	    if (_rules.getClipSize() == -1)
	    {
		    return 255;
	    }
	    return _ammoQuantity;
    }

    /**
     * Gets the item's inventory slot.
     * @return The slot id.
     */
    internal RuleInventory getSlot() =>
	    _inventorySlot;

    /**
     * Checks if the item is covering certain inventory slot(s).
     * @param x Slot X position.
     * @param y Slot Y position.
     * @param item Item to check for overlap, or NULL if none.
     * @return True if it is covering.
     */
    internal bool occupiesSlot(int x, int y, BattleItem item = null)
    {
        if (item == this)
            return false;
        if (_inventorySlot.getType() == InventoryType.INV_HAND)
            return true;
	    if (item == null)
	    {
		    return (x >= _inventoryX && x < _inventoryX + _rules.getInventoryWidth() &&
				    y >= _inventoryY && y < _inventoryY + _rules.getInventoryHeight());
	    }
	    else
	    {
		    return !(x >= _inventoryX + _rules.getInventoryWidth() ||
				    x + item.getRules().getInventoryWidth() <= _inventoryX ||
				    y >= _inventoryY + _rules.getInventoryHeight() ||
				    y + item.getRules().getInventoryHeight() <= _inventoryY);
	    }
    }

    /**
     * Gets the corpse's unit.
     * @return Pointer to BattleUnit.
     */
    internal BattleUnit getUnit() =>
	    _unit;

    /**
     * Sets the "dropped on non-player turn" flag. This is set when the item is dropped in the battlescape
     * or picked up in the inventory screen.
     * @param flag True if the aliens dropped the item.
     */
    internal void setTurnFlag(bool flag) =>
        _droppedOnAlienTurn = flag;

    /**
     * Removes the item from the previous owner and moves it to the new owner.
     * @param owner Pointer to Battleunit.
     */
    internal void moveToOwner(BattleUnit owner)
    {
        _previousOwner = _owner != null ? _owner : owner;
        _owner = owner;
        if (_previousOwner != null)
        {
            foreach (var item in _previousOwner.getInventory())
            {
                if (item == this)
                {
                    _previousOwner.getInventory().Remove(item);
                    break;
                }
            }
        }
        if (_owner != null)
        {
            _owner.getInventory().Add(this);
        }
    }

    /**
     * Sets the item's owner.
     * @param owner Pointer to Battleunit.
     */
    internal void setOwner(BattleUnit owner)
    {
        _previousOwner = _owner;
        _owner = owner;
    }

    /**
     * Sets the item's tile.
     * @param tile The tile.
     */
    internal void setTile(Tile tile) =>
        _tile = tile;

    /**
     * Sets the item's inventory slot.
     * @param slot The slot id.
     */
    internal void setSlot(RuleInventory slot) =>
        _inventorySlot = slot;

    /**
     * Sets the corpse's unit.
     * @param unit Pointer to BattleUnit.
     */
    internal void setUnit(BattleUnit unit) =>
        _unit = unit;

    /**
     * Converts an unconscious body into a dead one.
     * @param rules the rules of the corpse item to convert this item into.
     */
    internal void convertToCorpse(RuleItem rules)
    {
        if (_unit != null && _rules.getBattleType() == BattleType.BT_CORPSE && rules.getBattleType() == BattleType.BT_CORPSE)
        {
            _rules = rules;
        }
    }

    /**
     * Gets the item's tile.
     * @return The tile.
     */
    internal Tile getTile() =>
	    _tile;

    /**
     * Gets the item's owner.
     * @return Pointer to Battleunit.
     */
    internal BattleUnit getOwner() =>
	    _owner;

    /**
     * Saves the item to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "id", _id.ToString() },
            { "type", _rules.getType() }
        };
        if (_owner != null)
		    node.Add("owner", _owner.getId().ToString());
	    if (_previousOwner != null)
		    node.Add("previousOwner", _previousOwner.getId().ToString());
	    if (_unit != null)
		    node.Add("unit", _unit.getId().ToString());

	    if (_inventorySlot != null)
		    node.Add("inventoryslot", _inventorySlot.getId());
	    node.Add("inventoryX", _inventoryX.ToString());
	    node.Add("inventoryY", _inventoryY.ToString());

	    if (_tile != null)
		    node.Add("position", _tile.getPosition().save());
        if (_ammoQuantity != 0)
            node.Add("ammoqty", _ammoQuantity.ToString());
	    if (_ammoItem != null)
		    node.Add("ammoItem", _ammoItem.getId().ToString());

	    if (_rules != null && _rules.getBattleType() == BattleType.BT_MEDIKIT)
	    {
		    node.Add("painKiller", _painKiller.ToString());
		    node.Add("heal", _heal.ToString());
		    node.Add("stimulant", _stimulant.ToString());
	    }
	    if (_fuseTimer != -1)
		    node.Add("fuseTimer", _fuseTimer.ToString());
	    if (_droppedOnAlienTurn)
		    node.Add("droppedOnAlienTurn", _droppedOnAlienTurn.ToString());
	    if (_XCOMProperty)
		    node.Add("XCOMProperty", _XCOMProperty.ToString());
	    return node;
    }

    /**
     * Gets the item's id.
     * @return The item's id.
     */
    internal int getId() =>
	    _id;

    /**
     * Gets the turns until detonation. -1 = unprimed grenade
     * @return turns until detonation.
     */
    internal int getFuseTimer() =>
	    _fuseTimer;

    /**
     * Gets the item's previous owner.
     * @return Pointer to Battleunit.
     */
    internal BattleUnit getPreviousOwner() =>
	    _previousOwner;

    /**
     * Sets the turn to explode on.
     * @param turns Turns until detonation (player/alien turns, not game turns).
     */
    internal void setFuseTimer(int turns) =>
        _fuseTimer = turns;

    /**
     * Determines if the item uses ammo.
     * @return True if ammo is used.
     */
    internal bool needsAmmo() =>
	    !(_ammoItem == this); // no ammo for this weapon is needed

    /**
     * Sets the XCom property flag. This is to determine at debriefing what goes into the base/craft.
     * @param flag True if it's XCom property.
     */
    internal void setXCOMProperty(bool flag) =>
        _XCOMProperty = flag;

    /**
     * Sets the item's ammo item.
     * @param item The ammo item.
     * @return -2 when ammo doesn't fit, or -1 when weapon already contains ammo.
     */
    internal int setAmmoItem(BattleItem item)
    {
        if (!needsAmmo()) return -2;

        if (item == null)
        {
            if (_ammoItem != null)
            {
                _ammoItem.setIsAmmo(false);
            }
            _ammoItem = null;
            return 0;
        }

        if (_ammoItem != null)
            return -1;

        foreach (var i in _rules.getCompatibleAmmo())
        {
            if (i == item.getRules().getType())
            {
                _ammoItem = item;
                item.setIsAmmo(true);
                return 0;
            }
        }

        return -2;
    }

    /**
     * Sets the flag on this item indicating whether or not it is a clip used in a weapon.
     * @param ammo set the ammo flag to this.
     */
    void setIsAmmo(bool ammo) =>
        _isAmmo = ammo;

    /**
     * Sets the item's inventory X position.
     * @param x X position.
     */
    internal void setSlotX(int x) =>
        _inventoryX = x;

    /**
     * Sets the item's inventory Y position.
     * @param y Y position.
     */
    internal void setSlotY(int y) =>
        _inventoryY = y;

    /**
     * Checks if this item is loaded into a weapon.
     * @return if this is loaded into a weapon or not.
     */
    internal bool isAmmo() =>
	    _isAmmo;

    /**
     * Gets the item's inventory X position.
     * @return X position.
     */
    internal int getSlotX() =>
	    _inventoryX;

    /**
     * Gets the item's inventory Y position.
     * @return Y position.
     */
    internal int getSlotY() =>
	    _inventoryY;

    /**
     * Gets the pain killer quantity of the item.
     * @return The new pain killer quantity.
     */
    internal int getPainKillerQuantity() =>
	    _painKiller;

    /**
     * Gets the stimulant quantity of the item.
     * @return The new stimulant quantity.
     */
    internal int getStimulantQuantity() =>
	    _stimulant;

    /**
     * Gets the heal quantity of the item.
     * @return The new heal quantity.
     */
    internal int getHealQuantity() =>
	    _heal;

    /**
     * Gets the XCom property flag. This is to determine at debriefing what goes into the base/craft.
     * @return True if it's XCom property.
     */
    internal bool getXCOMProperty() =>
	    _XCOMProperty;

    /**
     * Spends a bullet from the ammo in this item.
     * @return True if there are bullets left.
     */
    internal bool spendBullet()
    {
	    if (_ammoQuantity > 0)
		    _ammoQuantity--;

	    if (_ammoQuantity == 0)
		    return false;
	    else
		    return true;
    }

    /**
     * Gets the "dropped on non-player turn" flag. This is to determine whether or not
     * aliens should attempt to pick this item up, as items dropped by the player may be "honey traps".
     * @return True if the aliens dropped the item.
     */
    internal bool getTurnFlag() =>
	    _droppedOnAlienTurn;

    /**
     * Loads the item from a YAML file.
     * @param node YAML node.
     * @param mod Mod for the item.
     */
    internal void load(YamlNode node, Mod.Mod mod)
    {
	    string slot = node["inventoryslot"] != null ? node["inventoryslot"].ToString() : "NULL";
	    if (slot != "NULL")
	    {
		    if (mod.getInventory(slot) != null)
		    {
			    _inventorySlot = mod.getInventory(slot);

		    }
		    else
		    {
			    _inventorySlot = mod.getInventory("STR_GROUND");
		    }
	    }
	    _inventoryX = int.Parse(node["inventoryX"].ToString());
	    _inventoryY = int.Parse(node["inventoryY"].ToString());
	    _ammoQuantity = int.Parse(node["ammoqty"].ToString());
	    _painKiller = int.Parse(node["painKiller"].ToString());
	    _heal = int.Parse(node["heal"].ToString());
	    _stimulant = int.Parse(node["stimulant"].ToString());
	    _fuseTimer = int.Parse(node["fuseTimer"].ToString());
	    _droppedOnAlienTurn = bool.Parse(node["droppedOnAlienTurn"].ToString());
	    _XCOMProperty = bool.Parse(node["XCOMProperty"].ToString());
    }

    /**
     * Sets the item's previous owner.
     * @param owner Pointer to Battleunit.
     */
    internal void setPreviousOwner(BattleUnit owner) =>
	    _previousOwner = owner;
}
