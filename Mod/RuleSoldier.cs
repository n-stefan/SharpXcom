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
 * Represents the creation data for an X-COM unit.
 * This info is copied to either Soldier for Geoscape or BattleUnit for Battlescape.
 * @sa Soldier BattleUnit
 */
internal class RuleSoldier : IRule
{
    string _type;
    int _costBuy, _costSalary, _standHeight, _kneelHeight, _floatHeight;
    int _femaleFrequency, _value, _transferTime;
    List<SoldierNamePool> _names;
    UnitStats _minStats, _maxStats, _statCaps;

    /**
     * Creates a blank ruleunit for a certain
     * type of soldier.
     * @param type String defining the type.
     */
    RuleSoldier(string type)
    {
        _type = type;
        _costBuy = 0;
        _costSalary = 0;
        _standHeight = 0;
        _kneelHeight = 0;
        _floatHeight = 0;
        _femaleFrequency = 50;
        _value = 20;
        _transferTime = 0;
    }

    public IRule Create(string type) =>
        new RuleSoldier(type);

    /**
     *
     */
    ~RuleSoldier() =>
        _names.Clear();

    /**
     * Gets the female appearance ratio.
     * @return The percentage ratio.
     */
    internal int getFemaleFrequency() =>
	    _femaleFrequency;

    /**
     * Returns the list of soldier name pools.
     * @return Pointer to soldier name pool list.
     */
    internal List<SoldierNamePool> getNames() =>
	    _names;

    /**
     * Gets the minimum stats for the random stats generator.
     * @return The minimum stats.
     */
    internal UnitStats getMinStats() =>
	    _minStats;

    /**
     * Gets the maximum stats for the random stats generator.
     * @return The maximum stats.
     */
    internal UnitStats getMaxStats() =>
	    _maxStats;

    /**
     * Returns the language string that names
     * this soldier. Each soldier type has a unique name.
     * @return Soldier name.
     */
    internal string getType() =>
	    _type;
}
