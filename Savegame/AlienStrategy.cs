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
    internal void addMissionRun(string varName, int increment = 1)
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
		foreach (var i in ((YamlMappingNode)node["missionLocations"]).Children)
		{
            _missionLocations.Add(i.Key.ToString(), ((YamlMappingNode)i.Value).Select(x =>
				KeyValuePair.Create(x.Key.ToString(), int.Parse(x.Value.ToString()))).ToList());
        }
		foreach (var i in ((YamlMappingNode)node["missionsRun"]).Children)
		{
			_missionRuns.Add(i.Key.ToString(), int.Parse(i.Value.ToString()));
        }
	}

	/**
	 * Checks the number of missions run labelled as "varName".
	 * @return the number of missions run under the variable name.
	 */
	internal int getMissionsRun(string varName) =>
		_missionRuns.TryGetValue(varName, out int missionRun) ? missionRun : 0;

	/**
	 * Checks that a given mission location (city or whatever) isn't stored in our list of previously attacked locations.
	 * @param varName the name of the variable that is storing our data.
	 * @param regionName the name of the region we're looking for.
	 * @param zoneNumber the number in the region that we want to check.
	 * @return if the region is valid (meaning it is not in our table).
	 */
	internal bool validMissionLocation(string varName, string regionName, int zoneNumber)
	{
		if (_missionLocations.ContainsKey(varName))
		{
			foreach (var i in _missionLocations[varName])
			{
				if (i.Key == regionName && i.Value == zoneNumber)
					return false;
			}
		}
		return true;
	}

	/**
	 * Adds a mission location to our storage array.
	 * @param varName the name on the variable under which to store this info.
	 * @param regionName the name of the region we're using.
	 * @param zoneNumber the number of the zone within that region we're using.
	 * @param maximum the maximum size of the list we want to maintain.
	 */
	internal void addMissionLocation(string varName, string regionName, int zoneNumber, int maximum)
	{
		if (maximum <= 0) return;
		_missionLocations[varName].Add(KeyValuePair.Create(regionName, zoneNumber));
		if (_missionLocations[varName].Count > (uint)(maximum))
		{
			_missionLocations.Remove(_missionLocations.First().Key);
		}
	}

	/**
	 * Checks that a given region appears in our strategy table.
	 * @param region the region we want to check for validity.
	 * @return if the region appears in the table or not.
	 */
	internal bool validMissionRegion(string region) =>
		_regionMissions.ContainsKey(region);

	/**
	 * Remove @a mission from the list of possible missions for @a region.
	 * @param region The region id.
	 * @param mission The mission id.
	 * @return If there are no more regions with missions available.
	 */
	internal bool removeMission(string region, string mission)
	{
		if (_regionMissions.TryGetValue(region, out WeightedOptions found))
		{
			found.set(mission, 0);
			if (found.empty())
			{
				_regionMissions.Remove(region);
				_regionChances.set(region, 0);
			}
		}
		return !_regionMissions.Any();
    }

	/**
	 * Choose one of the regions for a mission.
	 * @param mod Pointer to the mod.
	 * @return The region id.
	 */
	internal string chooseRandomRegion(Mod.Mod mod)
	{
		string chosen = _regionChances.choose();
		if (string.IsNullOrEmpty(chosen))
		{
			// no more missions to choose from: refresh.
			// First, free allocated memory.
			_regionMissions.Clear();
			// re-initialize the list
			init(mod);
			// now let's try that again.
			chosen = _regionChances.choose();
		}
		Debug.Assert(string.Empty != chosen);
		return chosen;
	}

	/**
	 * Choose one missions available for @a region.
	 * @param region The region id.
	 * @return The mission id.
	 */
	internal string chooseRandomMission(string region)
	{
        _regionMissions.TryGetValue(region, out WeightedOptions found);
		Debug.Assert(found != null);
		return found.choose();
	}

	/**
	 * Get starting values from the rules.
	 * @param mod Pointer to the game mod.
	 */
	internal void init(Mod.Mod mod)
	{
		foreach (var rr in mod.getRegionsList())
		{
			RuleRegion region = mod.getRegion(rr, true);
			_regionChances.set(rr, region.getWeight());
			WeightedOptions missions = region.getAvailableMissions();
			_regionMissions.Add(rr, missions);
		}
	}
}
