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
 * Represents one research project.
 * Dependency is the list of RuleResearchs which must be discovered before a RuleResearch became available.
 * Unlocks are used to immediately unlock a RuleResearch (even if not all the dependencies have been researched).
 *
 * Fake ResearchProjects: A RuleResearch is fake one, if its cost is 0. They are used to to create check points in the dependency tree.
 *
 * For example, if we have a Research E which needs either A & B or C & D, we create two fake research projects:
 *  - F which need A & B
 *  - G which need C & D
 * both F and G can unlock E.
 */
internal class RuleResearch : IListOrder, IRule
{
    string _name, _lookup, _cutscene;
    int _cost, _points;
    bool _needItem, _destroyItem;
    int _listOrder;

    RuleResearch(string name)
    {
        _name = name;
        _cost = 0;
        _points = 0;
        _needItem = false;
        _destroyItem = false;
        _listOrder = 0;
    }

    public IRule Create(string type) =>
        new RuleResearch(type);

    /**
     * Gets the name of this ResearchProject.
     * @return The name of this ResearchProject.
     */
    internal string getName() =>
	    _name;

    /**
     * Gets the list weight for this research item.
     * @return The list weight for this research item.
     */
    public int getListOrder() =>
	    _listOrder;
}
