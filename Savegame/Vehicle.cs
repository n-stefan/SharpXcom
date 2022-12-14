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
    Vehicle(RuleItem rules, int ammo, int size)
    {
        _rules = rules;
        _ammo = ammo;
        _size = size;
    }

    /**
     *
     */
    ~Vehicle() { }
}
