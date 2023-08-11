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
    internal const int HP_LEFT = -1;
    internal const int HP_CENTER = 0;
    internal const int HP_RIGHT = 1;

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

    internal CraftWeaponProjectile()
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

    /*
     * Returns the global type of projectile.
     * @return 0 - if it's a missile, 1 if beam.
     */
    internal CraftWeaponProjectileGlobalType getGlobalType() =>
	    _globalType;

    /*
     * Gets the accuracy of the projectile.
     */
    internal int getAccuracy() =>
	    _accuracy;

    /*
     * Gets the amount of damage the projectile can do
     * when hitting it's target.
     */
    internal int getDamage() =>
	    _damage;

    /*
     * Gets the direction of the projectile.
     */
    internal int getDirection() =>
	    _direction;

    /*
     * Marks the projectile to be removed.
     */
    internal void remove() =>
	    _toBeRemoved = true;

    /*
     * Gets the y position of the projectile on the radar.
     */
    internal int getPosition() =>
	    _currentPosition;

    /*
     * Returns if a projectile should be removed.
     */
    internal bool toBeRemoved() =>
	    _toBeRemoved;

    /*
     * Marks the projectile as a one which missed it's target.
     */
    internal void setMissed(bool missed) =>
	    _missed = missed;

    /*
     * Returns maximum range of projectile.
     */
    internal int getRange() =>
	    _range;

    /*
     * Returns true if the projectile missed it's target.
     * Otherwise returns false.
     */
    internal bool getMissed() =>
	    _missed;

    /*
     * Sets the y position of the projectile on the radar.
     */
    internal void setPosition(int position) =>
	    _currentPosition = position;

    /*
     * Sets the type of projectile according to the type of
     * weapon it was shot from. This is used for drawing the
     * projectiles.
     */
    internal void setType(CraftWeaponProjectileType type)
    {
	    _type = type;
	    if (type >= CraftWeaponProjectileType.CWPT_LASER_BEAM)
	    {
		    _globalType = CraftWeaponProjectileGlobalType.CWPGT_BEAM;
		    _state = 8;
	    }
    }

    /*
     * Sets the accuracy of the projectile.
     */
    internal void setAccuracy(int accuracy) =>
	    _accuracy = accuracy;

    /*
     * Sets the amount of damage the projectile can do
     * when hitting it's target.
     */
    internal void setDamage(int damage) =>
	    _damage = damage;

    /*
     * Sets the direction of the projectile.
     */
    internal void setDirection(int directon)
    {
	    _direction = directon;
	    if (_direction == (int)Directions.D_UP)
	    {
		    _currentPosition = 0;
	    }
    }

    /*
     * Sets the x position of the projectile on the radar.
     * It's used only once for each projectile during firing.
     */
    internal void setHorizontalPosition(int position) =>
	    _horizontalPosition = position;

    /*
     * Sets the speed of the projectile.
     */
    internal void setSpeed(int speed) =>
	    _speed = speed;

    /*
     * Sets maximum range of projectile.
     */
    internal void setRange(int range) =>
	    _range = range;

    /*
     * Moves the projectile according to it's speed
     * or changes the phase of beam animation.
     */
    internal void move()
    {
	    if (_globalType == CraftWeaponProjectileGlobalType.CWPGT_MISSILE)
	    {
		    int positionChange = _speed;

		    // Check if projectile would reach its maximum range this tick.
		    if ((_distanceCovered / 8) < getRange() && ((_distanceCovered + _speed)/ 8) >= getRange())
			    positionChange = getRange() * 8 - _distanceCovered;

		    // Check if projectile passed its maximum range on previous tick.
		    if ((_distanceCovered / 8) >= getRange())
			    setMissed(true);

		    if (_direction == (int)Directions.D_UP)
		    {
			    _currentPosition += positionChange;
		    }
		    else if (_direction == (int)Directions.D_DOWN)
		    {
			    _currentPosition -= positionChange;
		    }

		    _distanceCovered += positionChange;
	    }
	    else if (_globalType == CraftWeaponProjectileGlobalType.CWPGT_BEAM)
	    {
		    _state /= 2;
		    if (_state == 1)
		    {
			    _toBeRemoved = true;
		    }
	    }
    }

    /*
     * Gets the x position of the projectile.
     */
    internal int getHorizontalPosition() =>
	    _horizontalPosition;

    /*
     * Returns the type of projectile.
     * @return Projectile type as an integer value.
     */
    internal CraftWeaponProjectileType getType() =>
	    _type;

    /*
     * Returns animation state of a beam.
     */
    internal int getState() =>
	    _state;
}
