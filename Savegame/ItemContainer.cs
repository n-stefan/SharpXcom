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
 * Represents the items contained by a certain entity,
 * like base stores, craft equipment, etc.
 * Handles all necessary item management tasks.
 */
internal class ItemContainer
{
    Dictionary<string, int> _qty;

    /**
     * Initializes an item container with no contents.
     */
    internal ItemContainer() { }

    /**
     *
     */
    ~ItemContainer() { }

    /**
     * Saves the item container to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save() =>
	    new YamlSequenceNode(_qty.Select(x => new YamlMappingNode(x.Key, x.Value.ToString())));

    /**
     * Adds an item amount to the container.
     * @param id Item ID.
     * @param qty Item quantity.
     */
    internal void addItem(string id, int qty = 1)
    {
		if (string.IsNullOrEmpty(id))
	    {
		    return;
	    }
		if (!_qty.ContainsKey(id))
	    {
		    _qty[id] = 0;
	    }
	    _qty[id] += qty;
    }

	/**
	 * Returns the quantity of an item in the container.
	 * @param id Item ID.
	 * @return Item quantity.
	 */
	internal int getItem(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return 0;
		}

		if (!_qty.ContainsKey(id))
		{
			return 0;
		}
		else
		{
			return _qty[id];
		}
	}

    /**
     * Returns all the items currently contained within.
     * @return List of contents.
     */
    internal Dictionary<string, int> getContents() =>
        _qty;

	/**
	 * Loads the item container from a YAML file.
	 * @param node YAML node.
	 */
	internal void load(YamlNode node) =>
        _qty = ((YamlMappingNode)node).Children.ToDictionary(x => x.Key.ToString(), x => int.Parse(x.Value.ToString()));

    /**
	 * Removes an item amount from the container.
	 * @param id Item ID.
	 * @param qty Item quantity.
	 */
    internal void removeItem(string id, int qty = 1)
	{
		if (string.IsNullOrEmpty(id) || !_qty.ContainsKey(id))
		{
			return;
		}
		if (qty < _qty[id])
		{
			_qty[id] -= qty;
		}
		else
		{
			_qty.Remove(id);
		}
	}

	/**
	 * Returns the total size of the items in the container.
	 * @param mod Pointer to mod.
	 * @return Total item size.
	 */
	internal double getTotalSize(Mod.Mod mod)
	{
		double total = 0;
		foreach (var i in _qty)
		{
			total += mod.getItem(i.Key, true).getSize() * i.Value;
		}
		return total;
	}

	/**
	 * Returns the total quantity of the items in the container.
	 * @return Total item quantity.
	 */
	internal int getTotalQuantity()
	{
		int total = 0;
		foreach (var i in _qty)
		{
			total += i.Value;
		}
		return total;
	}
}
