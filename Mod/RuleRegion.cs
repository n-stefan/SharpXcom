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

/**
 * Represents a specific region of the world.
 * Contains constant info about a region like area
 * covered and base construction costs.
 */
internal class RuleRegion : IRule
{
    string _type;
    int _cost;
    List<City> _cities;
    /// Weight of this region when selecting regions for alien missions.
    uint _regionWeight;
    /// Weighted list of the different mission types for this region.
    WeightedOptions _missionWeights;

    /**
     * Creates a blank ruleset for a certain type of region.
     * @param type String defining the type.
     */
    RuleRegion(string type)
    {
        _type = type;
        _cost = 0;
        _regionWeight = 0;
    }

    public IRule Create(string type) =>
        new RuleRegion(type);

    /**
     * Deletes the cities from memory.
     */
    ~RuleRegion() =>
        _cities.Clear();

    /**
     * Gets the language string that names
     * this region. Each region type
     * has a unique name.
     * @return The region type.
     */
    internal string getType() =>
	    _type;

    /// Gets the weighted list of missions for this region.
    internal WeightedOptions getAvailableMissions() =>
        _missionWeights;
}
