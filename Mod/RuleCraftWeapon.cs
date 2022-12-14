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
 * Represents a specific type of craft weapon.
 * Contains constant info about a craft weapon like
 * damage, range, accuracy, items used, etc.
 * @sa CraftWeapon
 */
internal class RuleCraftWeapon : IRule
{
    string _type;
    int _sprite, _sound, _damage, _range, _accuracy, _reloadCautious, _reloadStandard, _reloadAggressive, _ammoMax, _rearmRate, _projectileSpeed;
    CraftWeaponProjectileType _projectileType;
    bool _underwaterOnly;
    string _launcher, _clip;

    /**
     * Creates a blank ruleset for a certain type of craft weapon.
     * @param type String defining the type.
     */
    RuleCraftWeapon(string type)
    {
        _type = type;
        _sprite = -1;
        _sound = -1;
        _damage = 0;
        _range = 0;
        _accuracy = 0;
        _reloadCautious = 0;
        _reloadStandard = 0;
        _reloadAggressive = 0;
        _ammoMax = 0;
        _rearmRate = 1;
        _projectileSpeed = 0;
        _projectileType = CraftWeaponProjectileType.CWPT_CANNON_ROUND;
        _underwaterOnly = false;
    }

    public IRule Create(string type) =>
        new RuleCraftWeapon(type);

    /**
     *
     */
    ~RuleCraftWeapon() { }

    /**
     * Gets the language string of the item used to
     * equip this craft weapon.
     * @return The item name.
     */
    internal string getLauncherItem() =>
	    _launcher;
}
