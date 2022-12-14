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
 * Represents a craft stored in a base.
 * Contains variable info about a craft like
 * position, fuel, damage, etc.
 * @sa RuleCraft
 */
internal class Craft : MovingTarget
{
    RuleCraft _rules;
    Base _base;
    int _fuel, _damage, _interceptionOrder, _takeoff;
    List<Vehicle> _vehicles;
    string _status;
    bool _lowFuel, _mission, _inBattlescape, _inDogfight;
    List<CraftWeapon> _weapons;
    ItemContainer _items;
    double _speedMaxRadian;

    internal Craft() { }

    /**
     * Initializes a craft of the specified type and
     * assigns it the latest craft ID available.
     * @param rules Pointer to ruleset.
     * @param base Pointer to base of origin.
     * @param id ID to assign to the craft (0 to not assign).
     */
    Craft(RuleCraft rules, Base @base, int id) : base()
    {
        _rules = rules;
        _base = @base;
        _fuel = 0;
        _damage = 0;
        _interceptionOrder = 0;
        _takeoff = 0;
        _status = "STR_READY";
        _lowFuel = false;
        _mission = false;
        _inBattlescape = false;
        _inDogfight = false;

        _items = new ItemContainer();
        if (id != 0)
        {
            _id = id;
        }
        for (uint i = 0; i < _rules.getWeapons(); ++i)
        {
            _weapons.Add(new CraftWeapon());
        }
        if (@base != null)
        {
            setBase(@base);
        }
        _speedMaxRadian = calculateRadianSpeed(_rules.getMaxSpeed()) * 120;
    }

    /**
     * Delete the contents of the craft from memory.
     */
    ~Craft()
    {
        _weapons.Clear();
        _items = null;
        _vehicles.Clear();
    }

    /**
     * Changes the base the craft belongs to.
     * @param base Pointer to base.
     * @param move Move the craft to the base coordinates.
     */
    void setBase(Base @base, bool move = true)
    {
        _base = @base;
        if (move)
        {
            _lon = getLongitude();
            _lat = getLatitude();
        }
    }

    /**
     * Sends the craft back to its origin base.
     */
    internal void returnToBase() =>
        setDestination(_base);

    /**
     * Changes the destination the craft is heading to.
     * @param dest Pointer to new destination.
     */
    void setDestination(Target dest)
    {
        if (_status != "STR_OUT")
        {
            _takeoff = 60;
        }
        if (dest == null)
            setSpeed(_rules.getMaxSpeed() / 2);
        else
            setSpeed(_rules.getMaxSpeed());
        base.setDestination(dest);
    }

    /**
     * Returns the globe marker for the craft.
     * @return Marker sprite, -1 if none.
     */
    internal override int getMarker()
    {
	    if (_status != "STR_OUT")
		    return -1;
	    else if (_rules.getMarker() == -1)
		    return 1;
        return _rules.getMarker();
    }
}
