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
	    new YamlSequenceNode(_qty.Select(x => new YamlMappingNode(new YamlScalarNode(x.Key), new YamlScalarNode(x.Value.ToString()))));

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
}
