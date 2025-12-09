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
 * Represents a vehicle (tanks etc.) kept in a craft.
 * Contains variable info about a vehicle like ammo.
 * @sa RuleItem
 */
internal class Vehicle
{
    RuleItem _rules;
    int _ammo, _size;

    /**
     * Initializes a vehicle of the specified type.
     * @param rules Pointer to ruleset.
     * @param ammo Initial ammo.
     * @param size Size in tiles.
     */
    internal Vehicle(RuleItem rules, int ammo, int size)
    {
        _rules = rules;
        _ammo = ammo;
        _size = size;
    }

    /**
     *
     */
    ~Vehicle() { }

    /**
     * Loads the vehicle from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    _ammo = int.Parse(node["ammo"].ToString());
	    _size = int.Parse(node["size"].ToString());
    }

    /**
     * Returns the ruleset for the vehicle's type.
     * @return Pointer to ruleset.
     */
    internal RuleItem getRules() =>
	    _rules;

    /**
     * Returns the ammo contained in this vehicle.
     * @return Weapon ammo.
     */
    internal int getAmmo()
    {
	    if (_ammo == -1)
	    {
		    return 255;
	    }
	    return _ammo;
    }

    /**
     * Returns the size occupied by this vehicle
     * in a transport craft.
     * @return Size in tiles.
     */
    internal int getSize() =>
	    _size;

    /**
     * Saves the base to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
	    var node = new YamlMappingNode();
	    node.Add("type", _rules.getType());
	    node.Add("ammo", _ammo.ToString());
	    node.Add("size", _size.ToString());
	    return node;
    }

    /**
     * Changes the ammo contained in this vehicle.
     * @param ammo Weapon ammo.
     */
    void setAmmo(int ammo)
    {
	    if (_ammo != -1)
	    {
		    _ammo = ammo;
	    }
    }
}
