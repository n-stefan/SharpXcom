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

enum MissionObjective { OBJECTIVE_SCORE, OBJECTIVE_INFILTRATION, OBJECTIVE_BASE, OBJECTIVE_SITE, OBJECTIVE_RETALIATION, OBJECTIVE_SUPPLY };

/**
 * @brief Information about a mission wave.
 * Mission waves control the UFOs that will be generated during an alien mission.
 */
struct MissionWave
{
    /// The type of the spawned UFOs.
    internal string ufoType;
    /// The number of UFOs that will be generated.
    /**
	 * The UFOs are generated sequentially, one every @a spawnTimer minutes.
	 */
    internal uint ufoCount;
    /// The trajectory ID for this wave's UFOs.
    /**
	 * Trajectories control the way UFOs fly around the Geoscape.
	 */
    internal string trajectory;
    /// Number of minutes between UFOs in the wave.
    /**
	 * The actual value used is spawnTimer/4 or 3*spawnTimer/4.
	 */
    internal uint spawnTimer;
    /// This wave performs the mission objective.
    /**
	 * The UFO executes a special action based on the mission objective.
	 */
    internal bool objective;

    /**
	 * Loads the MissionWave from a YAML file.
	 * @param node YAML node.
	 */
    internal void load(YamlNode node)
    {
        ufoType = node["ufoType"].ToString();
        ufoCount = uint.Parse(node["ufoCount"].ToString());
        trajectory = node["trajectory"].ToString();
        spawnTimer = uint.Parse(node["spawnTimer"].ToString());
        objective = bool.Parse(node["objective"].ToString());
    }
};

/**
 * Stores fixed information about a mission type.
 * It stores the mission waves and the distribution of the races that can
 * undertake the mission based on game date.
 */
internal class RuleAlienMission : IRule
{
    /// The mission's type ID.
    string _type;
    /// The mission's points.
    int _points;
    /// The mission's objective.
    MissionObjective _objective;
    /// The mission zone to use for spawning.
    int _spawnZone;
    /// The odds that this mission will result in retaliation
    int _retaliationOdds;
    /// The race distribution over game time.
    List<KeyValuePair<uint, WeightedOptions>> _raceDistribution;
    /// The mission's waves.
    List<MissionWave> _waves;
    /// The UFO to use for spawning.
    string _spawnUfo;
    /// The mission's weights.
    Dictionary<uint, int> _weights;
    /// the type of missionSite to spawn (if any)
    string _siteType;

    RuleAlienMission(string type)
    {
        _type = type;
        _points = 0;
        _objective = MissionObjective.OBJECTIVE_SCORE;
        _spawnZone = -1;
        _retaliationOdds = -1;
    }

    public IRule Create(string type) =>
        new RuleAlienMission(type);

    /**
     * Ensures the allocated memory is released.
     */
    ~RuleAlienMission() =>
        _raceDistribution.Clear();

    /// Gets the mission's type.
    internal string getType() =>
        _type;

    /// Gets the objective for this mission.
    internal MissionObjective getObjective() =>
        _objective;

	/**
	 * Loads the mission data from a YAML node.
	 * @param node YAML node.
	 */
	internal void load(YamlNode node)
	{
		_type = node["type"].ToString();
		_points = int.Parse(node["points"].ToString());
        _waves = ((YamlSequenceNode)node["waves"]).Children.Select(x =>
        {
            var wave = new MissionWave(); wave.load(x); return wave;
        }).ToList();
        _objective = (MissionObjective)int.Parse(node["objective"].ToString());
		_spawnUfo = node["spawnUfo"].ToString();
		_spawnZone = int.Parse(node["spawnZone"].ToString());
        _weights = ((YamlMappingNode)node["missionWeights"]).Children.ToDictionary(x => uint.Parse(x.Key.ToString()), x => int.Parse(x.Value.ToString()));
		_retaliationOdds = int.Parse(node["retaliationOdds"].ToString());
		_siteType = node["siteType"].ToString();
		//Only allow full replacement of mission racial distribution.
		if (node["raceWeights"] is YamlNode weights)
		{
            var assoc = new Dictionary<uint, WeightedOptions>();
			//Place in the associative container so we can index by month and keep entries sorted.
			foreach (var raceDistribution in _raceDistribution)
			{
				assoc.Add(raceDistribution.Key, raceDistribution.Value);
			}

			// Now go through the node contents and merge with existing data.
			foreach (var weight in ((YamlMappingNode)weights).Children)
			{
				uint month = uint.Parse(weight.Key.ToString());
				if (!assoc.ContainsKey(month))
				{
					// New entry, load and add it.
					WeightedOptions nw = new WeightedOptions();
					nw.load(weight.Value);
					assoc.Add(month, nw);
				}
				else
				{
					// Existing entry, update it.
					assoc[month].load(weight.Value);
				}
			}

			// Now replace values in our actual member variable!
			_raceDistribution.Clear();
			_raceDistribution = new List<KeyValuePair<uint, WeightedOptions>>(assoc.Count);
			foreach (var ii in assoc)
			{
				if (ii.Value.empty())
				{
					// Don't keep empty lists.
					assoc.Remove(ii.Key);
				}
				else
				{
					// Place it
					_raceDistribution.Add(ii);
				}
			}
		}
	}

    /// Gets the number of waves.
    internal int getWaveCount() =>
        _waves.Count;

    /// Gets the full wave information.
    internal MissionWave getWave(uint index) =>
        _waves[(int)index];

    /// Gets the zone for spawning an alien site or base.
    internal int getSpawnZone() =>
        _spawnZone;

    /// the type of missionSite to spawn (if any)
    internal string getSiteType() =>
        _siteType;

    /**
     * Returns the Alien score for this mission.
     * @return Amount of points.
     */
    internal int getPoints() =>
	    _points;

    /// Gets the UFO type for special spawns.
    internal string getSpawnUfo() =>
        _spawnUfo;

    /**
     * Chooses one of the available races for this mission.
     * The racial distribution may vary based on the current game date.
     * @param monthsPassed The number of months that have passed in the game world.
     * @return The string id of the race.
     */
    internal string generateRace(uint monthsPassed)
    {
        int rc;
        for (rc = _raceDistribution.Count - 1; rc >= 0 && monthsPassed < _raceDistribution[rc].Key; --rc);
        if (rc < 0)
		    return string.Empty;
	    return _raceDistribution[rc].Value.choose();
    }
}
