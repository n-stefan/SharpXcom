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
}
