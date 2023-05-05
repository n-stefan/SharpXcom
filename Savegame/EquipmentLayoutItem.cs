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
 * Represents a soldier-equipment layout item which is used
 * on the beginning of the Battlescape.
 */
internal class EquipmentLayoutItem
{
    string _itemType;
    string _slot;
    int _slotX, _slotY;
    string _ammoItem;
    int _fuseTimer;

    /**
     * Initializes a new soldier-equipment layout item from YAML.
     * @param node YAML node.
     */
    internal EquipmentLayoutItem(YamlNode node) =>
	    load(node);

    /**
     * Initializes a new soldier-equipment layout item.
     * @param itemType Item's type.
     * @param slot Occupied slot's id.
     * @param slotX Position-X in the occupied slot.
     * @param slotY Position-Y in the occupied slot.
     * @param ammoItem The ammo has to be loaded into the item. (it's type)
     * @param fuseTimer The turn until explosion of the item. (if it's an activated grenade-type)
     */
    EquipmentLayoutItem(string itemType, string slot, int slotX, int slotY, string ammoItem, int fuseTimer)
    {
        _itemType = itemType;
        _slot = slot;
        _slotX = slotX;
        _slotY = slotY;
        _ammoItem = ammoItem;
        _fuseTimer = fuseTimer;
    }

    /**
     *
     */
    ~EquipmentLayoutItem() { }

    /**
     * Saves the soldier-equipment layout item to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "itemType", _itemType },
            { "slot", _slot }
        };
        // only save this info if it's needed, reduce clutter in saves
        if (_slotX != 0)
	    {
		    node.Add("slotX", _slotX.ToString());
	    }
	    if (_slotY != 0)
	    {
		    node.Add("slotY", _slotY.ToString());
	    }
	    if (_ammoItem != "NONE")
	    {
		    node.Add("ammoItem", _ammoItem);
	    }
	    if (_fuseTimer >= 0)
	    {
		    node.Add("fuseTimer", _fuseTimer.ToString());
	    }
        return node;
    }

    /**
     * Returns the slot to be occupied.
     * @return slot name.
     */
    internal string getSlot() =>
	    _slot;

    /**
     * Loads the soldier-equipment layout item from a YAML file.
     * @param node YAML node.
     */
    void load(YamlNode node)
    {
	    _itemType = node["itemType"].ToString();
	    _slot = node["slot"].ToString();
	    _slotX = int.TryParse(node["slotX"].ToString(), out int slotX) ? slotX : 0;
	    _slotY = int.TryParse(node["slotY"].ToString(), out int slotY) ? slotY : 0;
	    _ammoItem = node["ammoItem"].ToString() ?? "NONE";
	    _fuseTimer = int.TryParse(node["fuseTimer"].ToString(), out int fuseTimer) ? fuseTimer : -1;
    }
}
