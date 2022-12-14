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
 * Represents a craft weapon equipped by a craft.
 * Contains variable info about a craft weapon like ammo.
 * @sa RuleCraftWeapon
 */
internal class CraftWeapon
{
    RuleCraftWeapon _rules;
    int _ammo;
    bool _rearming;

    internal CraftWeapon() { }

    /**
     * Initializes a craft weapon of the specified type.
     * @param rules Pointer to ruleset.
     * @param ammo Initial ammo.
     */
    CraftWeapon(RuleCraftWeapon rules, int ammo)
    {
        _rules = rules;
        _ammo = ammo;
        _rearming = false;
    }

    /**
     *
     */
    ~CraftWeapon() { }
}
