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
 * Represents an ongoing alien mission.
 * Contains variable info about the mission, like spawn counter, target region
 * and current wave.
 * @sa RuleAlienMission
 */
internal class AlienMission
{
    RuleAlienMission _rule;
    uint _nextWave;
    uint _nextUfoCounter;
    uint _spawnCountdown;
    uint _liveUfos;
    int _uniqueID, _missionSiteZone;
    AlienBase _base;
    string _region, _race;

    internal AlienMission() { }

    AlienMission(RuleAlienMission rule)
    {
        _rule = rule;
        _nextWave = 0;
        _nextUfoCounter = 0;
        _spawnCountdown = 0;
        _liveUfos = 0;
        _uniqueID = 0;
        _missionSiteZone = -1;
        _base = null;
    }

    ~AlienMission() { }

    /// Decrease number of live UFOs.
    internal void decreaseLiveUfos() =>
        --_liveUfos;

    /**
     * Saves the alien mission to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "type", _rule.getType() },
            { "region", _region },
            { "race", _race },
            { "nextWave", _nextWave.ToString() },
            { "nextUfoCounter", _nextUfoCounter.ToString() },
            { "spawnCountdown", _spawnCountdown.ToString() },
            { "liveUfos", _liveUfos.ToString() },
            { "uniqueID", _uniqueID.ToString() }
        };
        if (_base != null)
	    {
		    node.Add("alienBase", _base.saveId());
	    }
	    node.Add("missionSiteZone", _missionSiteZone.ToString());
	    return node;
    }

    /**
     * @return The unique ID assigned to this mission.
     */
    internal int getId()
    {
	    Debug.Assert(_uniqueID != 0, "Uninitialized mission!");
	    return _uniqueID;
    }
}
