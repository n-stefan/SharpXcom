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
        _waves = new List<MissionWave>();
        foreach (var wave in ((YamlSequenceNode)node["waves"]).Children)
        {
            var missionWave = new MissionWave();
            _waves.Add(missionWave.load(wave));
        }
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
}
