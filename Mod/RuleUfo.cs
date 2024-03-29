﻿/*
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
    string _modSprite;

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

    /**
     * Loads the UFO from a YAML file.
     * @param node YAML node.
     * @param mod Mod for the UFO.
     */
    internal void load(YamlNode node, Mod mod)
    {
	    _type = node["type"].ToString();
	    _size = node["size"].ToString();
	    _sprite = int.Parse(node["sprite"].ToString());
	    if (node["marker"] != null)
	    {
		    _marker = mod.getOffset(int.Parse(node["marker"].ToString()), 8);
	    }
	    if (node["markerLand"] != null)
	    {
		    _markerLand = mod.getOffset(int.Parse(node["markerLand"].ToString()), 8);
	    }
	    if (node["markerCrash"] != null)
	    {
		    _markerCrash = mod.getOffset(int.Parse(node["markerCrash"].ToString()), 8);
	    }
	    _damageMax = int.Parse(node["damageMax"].ToString());
	    _speedMax = int.Parse(node["speedMax"].ToString());
	    _power = int.Parse(node["power"].ToString());
	    _range = int.Parse(node["range"].ToString());
	    _score = int.Parse(node["score"].ToString());
	    _reload = int.Parse(node["reload"].ToString());
	    _breakOffTime = int.Parse(node["breakOffTime"].ToString());
	    _sightRange = int.Parse(node["sightRange"].ToString());
	    _missionScore = int.Parse(node["missionScore"].ToString());
	    if (node["battlescapeTerrainData"] is YamlNode terrain)
	    {
		    if (_battlescapeTerrainData != null)
			    _battlescapeTerrainData = null;
		    RuleTerrain rule = new RuleTerrain(terrain["name"].ToString());
		    rule.load(terrain, mod);
		    _battlescapeTerrainData = rule;
	    }
	    _modSprite = node["modSprite"].ToString();
    }

    /**
     * Gets the maximum damage (damage the UFO can take)
     * of the UFO.
     * @return The maximum damage.
     */
    internal int getMaxDamage() =>
	    _damageMax;

    /**
     * Gets the amount of points awarded every 30 minutes
     * while the UFO is on a mission (doubled when landed).
     * @return Score.
     */
    internal int getMissionScore() =>
	    _missionScore;

    /**
     * Gets the size of this type of UFO.
     * @return The Ufo's size.
     */
    internal string getSize() =>
	    _size;

    /**
     * Gets the UFO's radar range
     * for detecting bases.
     * @return The range in nautical miles.
     */
    internal int getSightRange() =>
	    _sightRange;

    /**
     * Gets the maximum speed of the UFO flying
     * around the Geoscape.
     * @return The maximum speed.
     */
    internal int getMaxSpeed() =>
	    _speedMax;

    /**
     * Gets the terrain data needed to draw the UFO in the battlescape.
     * @return The RuleTerrain.
     */
    internal RuleTerrain getBattlescapeTerrainData() =>
	    _battlescapeTerrainData;

    /**
     * For user-defined UFOs, use a surface for the "preview" image.
     * @return The name of the surface that represents this UFO.
     */
    internal string getModSprite() =>
	    _modSprite;

    /**
     * Gets the ID of the sprite used to draw the UFO
     * in the Dogfight window.
     * @return The sprite ID.
     */
    internal int getSprite() =>
	    _sprite;

    /**
     * Gets the UFO's break off time.
     * @return The UFO's break off time in game seconds.
     */
    internal int getBreakOffTime() =>
	    _breakOffTime;

    /**
     * Gets the maximum damage done by the
     * UFO's weapons per shot.
     * @return The weapon power.
     */
    internal int getWeaponPower() =>
	    _power;

    /**
     * Gets the maximum range for the
     * UFO's weapons.
     * @return The weapon range.
     */
    internal int getWeaponRange() =>
	    _range;

    /**
     * Gets the amount of points the player
     * gets for shooting down the UFO.
     * @return The score.
     */
    internal int getScore() =>
	    _score;

    /**
     * Gets the weapon reload for UFO ships.
     * @return The UFO weapon reload time.
     */
    internal int getWeaponReload() =>
	    _reload;

    /**
     * Gets the radius of this type of UFO
     * on the dogfighting window.
     * @return The radius in pixels.
     */
    internal int getRadius()
    {
	    if (_size == "STR_VERY_SMALL")
	    {
		    return 2;
	    }
	    else if (_size == "STR_SMALL")
	    {
		    return 3;
	    }
	    else if (_size == "STR_MEDIUM_UC")
	    {
		    return 4;
	    }
	    else if (_size == "STR_LARGE")
	    {
		    return 5;
	    }
	    else if (_size == "STR_VERY_LARGE")
	    {
		    return 6;
	    }
	    return 0;
    }
}
