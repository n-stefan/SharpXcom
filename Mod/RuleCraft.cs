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
 * Represents a specific type of craft.
 * Contains constant info about a craft like
 * costs, speed, capacities, consumptions, etc.
 * @sa Craft
 */
internal class RuleCraft : IListOrder, IRule
{
    string _type;
    int _sprite, _marker;
    int _fuelMax, _damageMax, _speedMax, _accel, _weapons, _soldiers, _vehicles, _costBuy, _costRent, _costSell;
    int _repairRate, _refuelRate, _radarRange, _radarChance, _sightRange, _transferTime, _score;
    RuleTerrain _battlescapeTerrainData;
    bool _spacecraft;
    int _listOrder, _maxItems, _maxAltitude;

    /**
     * Creates a blank ruleset for a certain
     * type of craft.
     * @param type String defining the type.
     */
    RuleCraft(string type)
    {
        _type = type;
        _sprite = -1;
        _marker = -1;
        _fuelMax = 0;
        _damageMax = 0;
        _speedMax = 0;
        _accel = 0;
        _weapons = 0;
        _soldiers = 0;
        _vehicles = 0;
        _costBuy = 0;
        _costRent = 0;
        _costSell = 0;
        _repairRate = 1;
        _refuelRate = 1;
        _radarRange = 672;
        _radarChance = 100;
        _sightRange = 1696;
        _transferTime = 0;
        _score = 0;
        _battlescapeTerrainData = null;
        _spacecraft = false;
        _listOrder = 0;
        _maxItems = 0;
        _maxAltitude = -1;
    }

    public IRule Create(string type) =>
        new RuleCraft(type);

    /**
     *
     */
    ~RuleCraft() =>
        _battlescapeTerrainData = null;

    /**
     * Gets the maximum number of weapons that
     * can be equipped onto the craft.
     * @return The weapon capacity.
     */
    internal uint getWeapons() =>
        (uint)_weapons;

    /**
     * Gets the maximum speed of the craft flying
     * around the Geoscape.
     * @return The speed in knots.
     */
    internal int getMaxSpeed() =>
	    _speedMax;

    /**
     * Returns the globe marker for the craft type.
     * @return Marker sprite, -1 if none.
     */
    internal int getMarker() =>
	    _marker;

    /**
     * Gets the list weight for this research item.
     * @return The list weight.
     */
    public int getListOrder() =>
        _listOrder;
}
