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
}
