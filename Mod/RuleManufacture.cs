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

/**
 * Represents the information needed to manufacture an object.
 */
internal class RuleManufacture : IListOrder, IRule
{
    string _name, _category;
    int _space, _time, _cost;
    int _listOrder;
    Dictionary<string, int> _requiredItems, _producedItems;

    /**
     * Creates a new Manufacture.
     * @param name The unique manufacture name.
     */
    RuleManufacture(string name)
    {
        _name = name;
        _space = 0;
        _time = 0;
        _cost = 0;
        _listOrder = 0;
        _producedItems[name] = 1;
    }

    public IRule Create(string type) =>
        new RuleManufacture(type);

    /**
     * Gets the unique name of the manufacture.
     * @return The name.
     */
    internal string getName() =>
	    _name;

    /**
     * Gets the list weight for this manufacture item.
     * @return The list weight for this manufacture item.
     */
    public int getListOrder() =>
	    _listOrder;
}
