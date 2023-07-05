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

namespace SharpXcom.Battlescape;

/**
 * Explosion state not only handles explosions, but also bullet impacts!
 * Refactoring tip : ImpactBState.
 */
internal class ExplosionBState : BattleState
{
    BattleUnit _unit;
    Position _center;
    BattleItem _item;
    Tile _tile;
    int _power;
    bool _areaOfEffect, _lowerWeapon, _cosmetic;

    /**
     * Sets up an ExplosionBState.
     * @param parent Pointer to the BattleScape.
     * @param center Center position in voxelspace.
     * @param item Item involved in the explosion (eg grenade).
     * @param unit Unit involved in the explosion (eg unit throwing the grenade).
     * @param tile Tile the explosion is on.
     * @param lowerWeapon Whether the unit causing this explosion should now lower their weapon.
     */
    internal ExplosionBState(BattlescapeGame parent, Position center, BattleItem item, BattleUnit unit, Tile tile = null, bool lowerWeapon = false, bool cosmetic = false) : base(parent)
    {
        _unit = unit;
        _center = center;
        _item = item;
        _tile = tile;
        _power = 0;
        _areaOfEffect = false;
        _lowerWeapon = lowerWeapon;
        _cosmetic = cosmetic;
    }

    /**
     * Deletes the ExplosionBState.
     */
    ~ExplosionBState() { }
}
