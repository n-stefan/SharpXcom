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
		if (node["missionWeights"] is YamlNode missionWeights)
		{
			foreach (var missionWeight in ((YamlMappingNode)missionWeights).Children)
			{
				WeightedOptions nw = new WeightedOptions();
				nw.load(missionWeight.Value);
				_missionWeights.Add(KeyValuePair.Create(uint.Parse(missionWeight.Key.ToString()), nw));
			}
		}
		if (node["raceWeights"] is YamlNode raceWeights)
		{
			foreach (var raceWeight in ((YamlMappingNode)raceWeights).Children)
			{
				WeightedOptions nw = new WeightedOptions();
				nw.load(raceWeight.Value);
				_raceWeights.Add(KeyValuePair.Create(uint.Parse(raceWeight.Key.ToString()), nw));
			}
		}
		if (node["regionWeights"] is YamlNode regionWeights)
		{
			foreach (var regionWeight in ((YamlMappingNode)regionWeights).Children)
			{
				WeightedOptions nw = new WeightedOptions();
				nw.load(regionWeight.Value);
				_regionWeights.Add(KeyValuePair.Create(uint.Parse(regionWeight.Key.ToString()), nw));
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
}
