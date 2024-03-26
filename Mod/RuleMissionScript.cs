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

enum GenerationType { GEN_REGION, GEN_MISSION, GEN_RACE };

internal class RuleMissionScript : IRule
{
    string _type, _varName;
    int _firstMonth, _lastMonth, _label, _executionOdds, _targetBaseOdds, _minDifficulty, _maxRuns, _avoidRepeats, _delay;
    bool _useTable, _siteType;
    List<KeyValuePair<uint, WeightedOptions>> _regionWeights, _missionWeights, _raceWeights;
    List<int> _conditionals;
    Dictionary<string, bool> _researchTriggers;

    /**
     * RuleMissionScript: the rules for the alien mission progression.
     * Each script element is independent, and the saved game will probe the list of these each month to determine what's going to happen.
     */
    RuleMissionScript(string type)
    {
        _type = type;
        _firstMonth = 0;
        _lastMonth = -1;
        _label = 0;
        _executionOdds = 100;
        _targetBaseOdds = 0;
        _minDifficulty = 0;
        _maxRuns = -1;
        _avoidRepeats = 0;
        _delay = 0;
        _useTable = true;
        _siteType = false;
    }

    public IRule Create(string type) =>
        new RuleMissionScript(type);

    /**
     * Destructor, cleans up the mess we left in ram.
     */
    ~RuleMissionScript()
    {
        _missionWeights.Clear();
        _raceWeights.Clear();
        _regionWeights.Clear();
    }

    /**
     * @return a complete, unique list of all the mission types this command could possibly generate.
     */
    internal HashSet<string> getAllMissionTypes()
    {
	    var types = new HashSet<string>();
	    foreach (var missionWeight in _missionWeights)
	    {
		    List<string> names = missionWeight.Value.getNames();
		    foreach (var name in names)
		    {
			    types.Add(name);
		    }
	    }
	    return types;
    }

    /**
     * @param siteType set this command to be a missionSite type or not.
     */
    internal void setSiteType(bool siteType) =>
	    _siteType = siteType;

	/**
	 * Loads a missionScript from a YML file.
	 * @param node the node within the file we're reading.
	 */
	internal void load(YamlNode node)
	{
		_varName = node["varName"].ToString();
		_firstMonth = int.Parse(node["firstMonth"].ToString());
		_lastMonth = int.Parse(node["lastMonth"].ToString());
		_label = (int)uint.Parse(node["label"].ToString());
		_executionOdds = int.Parse(node["executionOdds"].ToString());
		_targetBaseOdds = int.Parse(node["targetBaseOdds"].ToString());
		_minDifficulty = int.Parse(node["minDifficulty"].ToString());
		_maxRuns = int.Parse(node["maxRuns"].ToString());
		_avoidRepeats = int.Parse(node["avoidRepeats"].ToString());
		_delay = int.Parse(node["startDelay"].ToString());
        _conditionals = ((YamlSequenceNode)node["conditionals"]).Children.Select(x => int.Parse(x.ToString())).ToList();
		if (node["missionWeights"] is YamlNode weights1)
		{
			foreach (var nn in ((YamlMappingNode)weights1).Children)
			{
				WeightedOptions nw = new WeightedOptions();
				nw.load(nn.Value);
				_missionWeights.Add(KeyValuePair.Create(uint.Parse(nn.Key.ToString()), nw));
			}
		}
		if (node["raceWeights"] is YamlNode weights2)
		{
			foreach (var nn in ((YamlMappingNode)weights2).Children)
			{
				WeightedOptions nw = new WeightedOptions();
				nw.load(nn.Value);
				_raceWeights.Add(KeyValuePair.Create(uint.Parse(nn.Key.ToString()), nw));
			}
		}
		if (node["regionWeights"] is YamlNode weights3)
		{
			foreach (var nn in ((YamlMappingNode)weights3).Children)
			{
				WeightedOptions nw = new WeightedOptions();
				nw.load(nn.Value);
				_regionWeights.Add(KeyValuePair.Create(uint.Parse(nn.Key.ToString()), nw));
			}
		}
		_researchTriggers = ((YamlMappingNode)node["researchTriggers"]).Children.ToDictionary(x => x.Key.ToString(), x => bool.Parse(x.Value.ToString()));
		_useTable = bool.Parse(node["useTable"].ToString());
		if (string.IsNullOrEmpty(_varName) && (_maxRuns > 0 || _avoidRepeats > 0))
		{
			throw new Exception("Error in mission script: " + _type + ": no varName provided for a script with maxRuns or repeatAvoidance.");
		}
	}

	/**
	 * @return the first month this script should run.
	 */
	internal int getFirstMonth() =>
		_firstMonth;

	/**
	 * @return the last month this script should run.
	 */
	internal int getLastMonth() =>
		_lastMonth;

	/**
	 * @return the maximum runs for scripts tracking our varName.
	 */
	internal int getMaxRuns() =>
		_maxRuns;

	/**
	 * @return the label this command uses for conditional tracking.
	 */
	internal int getLabel() =>
		_label;

	/**
	 * @return the list of conditions that govern execution of this command.
	 */
	internal List<int> getConditionals() =>
		_conditionals;

	/**
	 * @return a list of research topics that govern execution of this script.
	 */
	internal Dictionary<string, bool> getResearchTriggers() =>
		_researchTriggers;

	/**
	 * @return the minimum difficulty for this script to run.
	 */
	internal int getMinDifficulty() =>
		_minDifficulty;

	/**
	 * @return the name of the variable we want to use to track in the saved game.
	 */
	internal string getVarName() =>
		_varName;

	/**
	 * Gets the name of this command.
	 * @return the name of the command.
	 */
	internal string getType() =>
		_type;

	/**
	 * @return the odds of this command's execution.
	 */
	internal int getExecutionOdds() =>
		_executionOdds;

	/**
	 * @return if this is a mission site type command or not.
	 */
	internal bool getSiteType() =>
		_siteType;

	/**
	 * Chooses one of the available races, regions, or missions for this command.
	 * @param monthsPassed The number of months that have passed in the game world.
	 * @param type the type of thing we want to generate, region, mission or race.
	 * @return The string id of the thing.
	 */
	internal string generate(uint monthsPassed, GenerationType type)
	{
		List<KeyValuePair<uint, WeightedOptions>> rw;
		if (type == GenerationType.GEN_RACE)
			rw = _raceWeights;
		else if (type == GenerationType.GEN_REGION)
			rw = _regionWeights;
		else
			rw = _missionWeights;
		int i = rw.Count - 1;
		while (monthsPassed < rw[i].Key)
			--i;
		return rw[i].Value.choose();
    }

	/**
	 * @param month the month for which we want info.
	 * @return a list of the possible missions for the given month.
	 */
	internal List<string> getMissionTypes(int month)
	{
		var missions = new List<string>();
		int rw = _missionWeights.Count - 1;
		while (month < (int)(_missionWeights[rw].Key))
		{
			--rw;
			if (rw < 0)
			{
				++rw;
				break;
			}
		}
		foreach (var i in _missionWeights[rw].Value.getNames())
		{
			missions.Add(i);
		}
		return missions;
	}

	/**
	 * @param month the month for which we want info.
	 * @return the list of regions we have to pick from this month.
	 */
	internal List<string> getRegions(int month)
	{
		var regions = new List<string>();
		int rw = _regionWeights.Count - 1;
		while (month < (int)(_regionWeights[rw].Key))
		{
			--rw;
			if (rw < 0)
			{
				++rw;
				break;
			}
		}
		foreach (var i in _regionWeights[rw].Value.getNames())
		{
			regions.Add(i);
		}
		return regions;
	}

	/**
	 * @return the odds of this command targetting a base.
	 */
	internal int getTargetBaseOdds() =>
		_targetBaseOdds;

	/**
	 * @return if this command uses a weighted distribution to pick a region.
	 */
	internal bool hasRegionWeights() =>
		_regionWeights.Any();

	/**
	 * @return the number of sites to avoid repeating missions against.
	 */
	internal int getRepeatAvoidance() =>
		_avoidRepeats;

	/**
	 * @return if this command uses a weighted distribution to pick a mission.
	 */
	internal bool hasMissionWeights() =>
		_missionWeights.Any();

	/**
	 * @return if this command uses a weighted distribution to pick a race.
	 */
	internal bool hasRaceWeights() =>
		_raceWeights.Any();

	/**
	 * @return the fixed delay on spawning the first wave (if any) to override whatever's written in the mission definition.
	 */
	internal int getDelay() =>
		_delay;

	/**
	 * @return if this command should remove the mission it generates from the strategy table.
	 */
	internal bool getUseTable() =>
		_useTable;
}
