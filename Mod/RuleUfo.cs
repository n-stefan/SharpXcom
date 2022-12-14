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
 * Represents a specific type of UFO.
 * Contains constant info about a UFO like
 * speed, weapons, scores, etc.
 * @sa Ufo
 */
internal class RuleUfo : IRule
{
    string _type, _size;
    int _sprite, _marker, _markerLand, _markerCrash;
    int _damageMax, _speedMax, _power, _range, _score, _reload, _breakOffTime, _sightRange, _missionScore;
    RuleTerrain _battlescapeTerrainData;

    /**
     * Creates a blank ruleset for a certain
     * type of UFO.
     * @param type String defining the type.
     */
    RuleUfo(string type)
    {
        _type = type;
        _size = "STR_VERY_SMALL";
        _sprite = -1;
        _marker = -1;
        _markerLand = -1;
        _markerCrash = -1;
        _damageMax = 0;
        _speedMax = 0;
        _power = 0;
        _range = 0;
        _score = 0;
        _reload = 0;
        _breakOffTime = 0;
        _sightRange = 268;
        _missionScore = 1;
        _battlescapeTerrainData = null;
    }

    public IRule Create(string type) =>
        new RuleUfo(type);

    /**
     *
     */
    ~RuleUfo() =>
        _battlescapeTerrainData = null;

    /**
     * Gets the language string that names
     * this UFO. Each UFO type has a unique name.
     * @return The Ufo's name.
     */
    internal string getType() =>
	    _type;

    /**
     * Returns the globe marker for the UFO when crashed.
     * @return Marker sprite, -1 if none.
     */
    internal int getCrashMarker() =>
	    _markerCrash;

    /**
     * Returns the globe marker for the UFO while landed.
     * @return Marker sprite, -1 if none.
     */
    internal int getLandMarker() =>
	    _markerLand;

    /**
     * Returns the globe marker for the UFO while in flight.
     * @return Marker sprite, -1 if none.
     */
    internal int getMarker() =>
	    _marker;
}
