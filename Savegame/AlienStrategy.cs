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
 * Stores the information about alien strategy.
 */
internal class AlienStrategy
{
    /// The chances of each mission type for each region.
    Dictionary<string, WeightedOptions> _regionMissions;
    /// The chances of each region to be targeted for a mission.
    WeightedOptions _regionChances;
    Dictionary<string, int> _missionRuns;
    Dictionary<string, List<KeyValuePair<string, int>>> _missionLocations;

    /**
	 * Create an AlienStrategy with no values.
	 * Running a game like this will most likely crash.
	 */
    internal AlienStrategy()
	{
		// Empty by design!
	}

	/**
	 * Free all resources used by this AlienStrategy.
	 */
	~AlienStrategy()
	{
		// Free allocated memory.
		_regionMissions.Clear();
	}

	/**
	 * Saves the alien data to a YAML file.
	 * @return YAML node.
	 */
	internal YamlNode save()
	{
        var node = new YamlMappingNode
        {
            { "regions", _regionChances.save() },
            { "possibleMissions", new YamlSequenceNode() }
        };
        foreach (var regionMission in _regionMissions)
		{
            var subnode = new YamlMappingNode
            {
                { "region", regionMission.Key },
                { "missions", regionMission.Value.save() }
            };
            ((YamlSequenceNode)node["possibleMissions"]).Add(subnode);
		}
		node.Add("missionLocations",
			new YamlSequenceNode(_missionLocations.Select(x => new YamlMappingNode(x.Key,
			new YamlSequenceNode(x.Value.Select(y => new YamlMappingNode(y.Key, y.Value.ToString())))))));
        node.Add("missionsRun", new YamlSequenceNode(_missionRuns.Select(x => new YamlMappingNode(x.Key, x.Value.ToString()))));
		return node;
	}

	/**
	 * Increments the number of missions run labelled as "varName".
	 * @param varName the variable name that we want to use to keep track of this.
	 * @param increment the value to increment by.
	 */
	internal void addMissionRun(string varName, int increment)
	{
		if (string.IsNullOrEmpty(varName))
			return;
		_missionRuns[varName] += increment;
	}

	/**
	 * Loads the data from a YAML file.
	 * @param node YAML node.
	 */
	internal void load(YamlNode node)
	{
		// Free allocated memory.
		_regionMissions.Clear();
		_regionChances.clear();
		_regionChances.load(node["regions"]);
        foreach (var nn in ((YamlSequenceNode)node["possibleMissions"]).Children)
		{
			string region = nn["region"].ToString();
			YamlNode missions = nn["missions"];
			WeightedOptions options = new WeightedOptions();
			options.load(missions);
			_regionMissions.Add(region, options);
		}
		foreach (var child in ((YamlSequenceNode)node["missionLocations"]).Children)
		{
            _missionLocations.Add(child[0].ToString(), ((YamlSequenceNode)child[1]).Select(x =>
				KeyValuePair.Create(x[0].ToString(), int.Parse(x[1].ToString()))).ToList());
        }
		foreach (var child in ((YamlSequenceNode)node["missionsRun"]).Children)
		{
			_missionRuns.Add(child[0].ToString(), int.Parse(child[1].ToString()));
        }
	}

	/**
	 * Checks the number of missions run labelled as "varName".
	 * @return the number of missions run under the variable name.
	 */
	internal int getMissionsRun(string varName) =>
		_missionRuns.TryGetValue(varName, out int missionRun) ? missionRun : 0;
}
