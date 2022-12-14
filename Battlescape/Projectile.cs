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
 * A class that represents a projectile. Map is the owner of an instance of this class during its short life.
 * It calculates its own trajectory and then moves along this precalculated trajectory in voxel space.
 */
internal class Projectile
{
    Mod.Mod _mod;
    SavedBattleGame _save;
    BattleAction _action;
    Position _origin, _targetVoxel;
    uint _position;
    int _bulletSprite;
    bool _reversed;
    int _vaporColor, _vaporDensity, _vaporProbability;
    int _speed;
    Surface _sprite;

    /**
     * Sets up a UnitSprite with the specified size and position.
     * @param mod Pointer to mod.
     * @param save Pointer to battlesavegame.
     * @param action An action.
     * @param origin Position the projectile originates from.
     * @param targetVoxel Position the projectile is targeting.
     * @param ammo the ammo that produced this projectile, where applicable.
     */
    Projectile(Mod.Mod mod, SavedBattleGame save, BattleAction action, Position origin, Position targetVoxel, BattleItem ammo)
    {
        _mod = mod;
        _save = save;
        _action = action;
        _origin = origin;
        _targetVoxel = targetVoxel;
        _position = 0;
        _bulletSprite = -1;
        _reversed = false;
        _vaporColor = -1;
        _vaporDensity = -1;
        _vaporProbability = 5;

        // this is the number of pixels the sprite will move between frames
        _speed = Options.battleFireSpeed;
        if (_action.weapon != null)
        {
            if (_action.type == BattleActionType.BA_THROW)
            {
                _sprite = _mod.getSurfaceSet("FLOOROB.PCK").getFrame(getItem().getRules().getFloorSprite());
            }
            else
            {
                // try to get all the required info from the ammo, if present
                if (ammo != null)
                {
                    _bulletSprite = ammo.getRules().getBulletSprite();
                    _vaporColor = ammo.getRules().getVaporColor();
                    _vaporDensity = ammo.getRules().getVaporDensity();
                    _vaporProbability = ammo.getRules().getVaporProbability();
                    _speed = Math.Max(1, _speed + ammo.getRules().getBulletSpeed());
                }

                // no ammo, or the ammo didn't contain the info we wanted, see what the weapon has on offer.
                if (_bulletSprite == -1)
                {
                    _bulletSprite = _action.weapon.getRules().getBulletSprite();
                }
                if (_vaporColor == -1)
                {
                    _vaporColor = _action.weapon.getRules().getVaporColor();
                }
                if (_vaporDensity == -1)
                {
                    _vaporDensity = _action.weapon.getRules().getVaporDensity();
                }
                if (_vaporProbability == 5)
                {
                    _vaporProbability = _action.weapon.getRules().getVaporProbability();
                }
                if (ammo == null || (ammo != _action.weapon || ammo.getRules().getBulletSpeed() == 0))
                {
                    _speed = Math.Max(1, _speed + _action.weapon.getRules().getBulletSpeed());
                }
            }
        }
        if ((targetVoxel.x - origin.x) + (targetVoxel.y - origin.y) >= 0)
        {
            _reversed = true;
        }
    }

    /**
     * Deletes the Projectile.
     */
    ~Projectile() { }

    /**
     * Gets the project tile item.
     * Returns 0 when there is no item thrown.
     * @return Pointer to BattleItem.
     */
    BattleItem getItem()
    {
        if (_action.type == BattleActionType.BA_THROW)
            return _action.weapon;
        else
            return null;
    }
}
