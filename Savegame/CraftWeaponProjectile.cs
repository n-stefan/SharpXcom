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

// Do not change the order of these enums because they are related to blob order.
enum CraftWeaponProjectileType { CWPT_STINGRAY_MISSILE, CWPT_AVALANCHE_MISSILE, CWPT_CANNON_ROUND, CWPT_FUSION_BALL, CWPT_LASER_BEAM, CWPT_PLASMA_BEAM };

enum CraftWeaponProjectileGlobalType { CWPGT_MISSILE, CWPGT_BEAM };

enum Directions { D_NONE, D_UP, D_DOWN };

internal class CraftWeaponProjectile
{
    CraftWeaponProjectileType _type;
    CraftWeaponProjectileGlobalType _globalType;
    int _speed;
    int _direction;
    int _currentPosition; // relative to interceptor, apparently, which is a problem when the interceptor disengages while projectile is in flight
    int _horizontalPosition;
    int _state;
    int _accuracy;
    int _damage;
    int _range;
    bool _toBeRemoved;
    bool _missed;
    int _distanceCovered;

    CraftWeaponProjectile()
    {
        _type = CraftWeaponProjectileType.CWPT_CANNON_ROUND;
        _globalType = CraftWeaponProjectileGlobalType.CWPGT_MISSILE;
        _speed = 0;
        _direction = (int)Directions.D_NONE;
        _currentPosition = 0;
        _horizontalPosition = 0;
        _state = 0;
        _accuracy = 0;
        _damage = 0;
        _range = 0;
        _toBeRemoved = false;
        _missed = false;
        _distanceCovered = 0;
    }

    ~CraftWeaponProjectile() { }
}
