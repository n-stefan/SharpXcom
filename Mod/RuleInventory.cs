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

struct RuleSlot
{
    internal int x, y;

    /**
	 * Loads the RuleSlot from a YAML file.
	 * @param node YAML node.
	 */
    internal void load(YamlNode node)
    {
        x = int.Parse(node["x"].ToString());
        y = int.Parse(node["y"].ToString());
    }
};

/**
 * Represents a specific section of the inventory,
 * containing information like available slots and
 * screen position.
 */
internal class RuleInventory : IListOrder, IRule
{
    internal const int SLOT_W = 16;
    internal const int SLOT_H = 16;
    internal const int HAND_W = 2;
    internal const int HAND_H = 3;

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
        _costs = ((YamlMappingNode)node["costs"]).Children.ToDictionary(x => x.Key.ToString(), x => int.Parse(x.Value.ToString()));
	    _listOrder = node["listOrder"] != null ? int.Parse(node["listOrder"].ToString()) : listOrder;
    }

    /**
     * Gets all the slots in the inventory section.
     * @return The list of slots.
     */
    internal List<RuleSlot> getSlots() =>
	    _slots;

    /**
     * Checks if an item completely fits when
     * placed in a certain slot.
     * @param item Pointer to item ruleset.
     * @param x Slot X position.
     * @param y Slot Y position.
     * @return True if there's a slot there.
     */
    internal bool fitItemInSlot(RuleItem item, int x, int y)
    {
	    if (_type == InventoryType.INV_HAND)
	    {
		    return true;
	    }
	    else if (_type == InventoryType.INV_GROUND)
	    {
		    int width = (320 - _x) / SLOT_W;
		    int height = (200 - _y) / SLOT_H;
		    int xOffset = 0;
		    while (x >= xOffset + width)
			    xOffset += width;
		    for (int xx = x; xx < x + item.getInventoryWidth(); ++xx)
		    {
			    for (int yy = y; yy < y + item.getInventoryHeight(); ++yy)
			    {
				    if (!(xx >= xOffset && xx < xOffset + width && yy >= 0 && yy < height))
					    return false;
			    }
		    }
		    return true;
	    }
	    else
	    {
		    int totalSlots = item.getInventoryWidth() * item.getInventoryHeight();
		    int foundSlots = 0;
		    for (var i = 0; i < _slots.Count && foundSlots < totalSlots; ++i)
		    {
			    if (_slots[i].x >= x && _slots[i].x < x + item.getInventoryWidth() &&
                    _slots[i].y >= y && _slots[i].y < y + item.getInventoryHeight())
			    {
				    foundSlots++;
			    }
		    }
		    return (foundSlots == totalSlots);
	    }
    }

    /**
     * Gets the X position of the inventory section on the screen.
     * @return The X position in pixels.
     */
    internal int getX() =>
	    _x;

    /**
     * Gets the Y position of the inventory section on the screen.
     * @return The Y position in pixels.
     */
    internal int getY() =>
	    _y;

    /**
     * Gets the time unit cost to place an item in another section.
     * @param slot The new section id.
     * @return The time unit cost.
     */
    internal int getCost(RuleInventory slot)
    {
	    if (slot == this)
		    return 0;
	    return _costs[slot.getId()];
    }

    /**
     * Gets the slot located in the specified mouse position.
     * @param x Mouse X position. Returns the slot's X position.
     * @param y Mouse Y position. Returns the slot's Y position.
     * @return True if there's a slot there.
     */
    internal bool checkSlotInPosition(ref int x, ref int y)
    {
	    int mouseX = x, mouseY = y;
	    if (_type == InventoryType.INV_HAND)
	    {
		    for (int xx = 0; xx < HAND_W; ++xx)
		    {
			    for (int yy = 0; yy < HAND_H; ++yy)
			    {
				    if (mouseX >= _x + xx * SLOT_W && mouseX < _x + (xx + 1) * SLOT_W &&
					    mouseY >= _y + yy * SLOT_H && mouseY < _y + (yy + 1) * SLOT_H)
				    {
					    x = 0;
					    y = 0;
					    return true;
				    }
			    }
		    }
	    }
	    else if (_type == InventoryType.INV_GROUND)
	    {
		    if (mouseX >= _x && mouseX < 320 && mouseY >= _y && mouseY < 200)
		    {
			    x = (int)Math.Floor((double)(mouseX - _x) / SLOT_W);
			    y = (int)Math.Floor((double)(mouseY - _y) / SLOT_H);
			    return true;
		    }
	    }
	    else
	    {
		    foreach (var i in _slots)
		    {
			    if (mouseX >= _x + i.x * SLOT_W && mouseX < _x + (i.x + 1) * SLOT_W &&
				    mouseY >= _y + i.y * SLOT_H && mouseY < _y + (i.y + 1) * SLOT_H)
			    {
				    x = i.x;
				    y = i.y;
				    return true;
			    }
		    }
	    }
	    return false;
    }
}
