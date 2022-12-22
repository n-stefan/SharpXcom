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

namespace SharpXcom.Mod;

enum InventoryType { INV_SLOT, INV_HAND, INV_GROUND };

/**
 * Represents a specific section of the inventory,
 * containing information like available slots and
 * screen position.
 */
internal class RuleInventory : IListOrder, IRule
{
    string _id;
    int _x, _y;
    InventoryType _type;
    int _listOrder;
    List<RuleSlot> _slots;
    Dictionary<string, int> _costs;

    /**
     * Creates a blank ruleset for a certain
     * type of inventory section.
     * @param id String defining the id.
     */
    RuleInventory(string id)
    {
        _id = id;
        _x = 0;
        _y = 0;
        _type = InventoryType.INV_SLOT;
        _listOrder = 0;
    }

    public IRule Create(string type) =>
        new RuleInventory(type);

    ~RuleInventory() { }

    /**
     * Gets the type of the inventory section.
     * Slot-based contain a limited number of slots.
     * Hands only contain one slot but can hold any item.
     * Ground can hold infinite items but don't attach to soldiers.
     * @return The inventory type.
     */
    internal InventoryType getType() =>
	    _type;

    /**
     * Gets the language string that names
     * this inventory section. Each section has a unique name.
     * @return The section name.
     */
    internal string getId() =>
	    _id;

    public int getListOrder() =>
	    _listOrder;

    /**
     * Loads the inventory from a YAML file.
     * @param node YAML node.
     * @param listOrder The list weight for this inventory.
     */
    internal void load(YamlNode node, int listOrder)
    {
	    _id = node["id"].ToString();
	    _x = int.Parse(node["x"].ToString());
	    _y = int.Parse(node["y"].ToString());
	    _type = (InventoryType)int.Parse(node["type"].ToString());
        _slots = ((YamlSequenceNode)node["slots"]).Children.Select(x =>
        {
            var slot = new RuleSlot(); slot.load(x); return slot;
        }).ToList();
        _costs = ((YamlSequenceNode)node["costs"]).Children.ToDictionary(x => x[0].ToString(), x => int.Parse(x[1].ToString()));
	    _listOrder = int.Parse(node["listOrder"].ToString());
    }
}
