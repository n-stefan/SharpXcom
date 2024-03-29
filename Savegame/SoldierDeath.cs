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

namespace SharpXcom.Savegame;

/**
 * Stores info about a soldier's death.
 */
internal class SoldierDeath
{
    GameTime _time;
    BattleUnitKills _cause;

    internal SoldierDeath()
    {
        _time = new GameTime(0, 0, 0, 0, 0, 0, 0);
        _cause = default;
    }

    /**
     * Initializes a death event.
     */
    internal SoldierDeath(GameTime time, BattleUnitKills cause)
    {
        _time = time;
        _cause = cause;
    }

    /**
     * Cleans up a death event.
     */
    ~SoldierDeath() =>
        _cause = default;

    /**
     * Saves the death to a YAML file.
     * @returns YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "time", _time.save() }
        };
        if (_cause != default)
	    {
            node.Add("cause", _cause.save());
	    }
	    return node;
    }

    /**
     * Loads the death from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    _time.load(node["time"]);
	    if (node["cause"] != null)
	    {
		    _cause = new BattleUnitKills();
		    _cause.load(node["cause"]);
	    }
    }

    /**
    * Returns the time of death of this soldier.
    * @return Pointer to the time.
    */
    internal BattleUnitKills getCause() =>
	    _cause;

    /**
     * Returns the time of death of this soldier.
     * @return Pointer to the time.
     */
    internal GameTime getTime() =>
	    _time;
}
