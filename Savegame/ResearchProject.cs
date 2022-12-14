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
   Represent a ResearchProject
   Contain information about assigned scientist, time already spent and cost of the project.
 */
internal class ResearchProject
{
    RuleResearch _project;
    int _assigned;
    int _spent;
    int _cost;

    ResearchProject(RuleResearch p, int c)
    {
        _project = p;
        _assigned = 0;
        _spent = 0;
        _cost = c;
    }

    /**
     * Saves the research project to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "project", getRules().getName() },
            { "assigned", getAssigned().ToString() },
            { "spent", getSpent().ToString() },
            { "cost", getCost().ToString() }
        };
        return node;
    }

    /**
     * Returns the cost of the ResearchProject
     * @return the cost of the ResearchProject(in man/day)
     */
    int getCost() =>
	    _cost;

    /**
     * Returns the time already spent on this project
     * @return the time already spent on this ResearchProject(in man/day)
     */
    int getSpent() =>
	    _spent;

    /**
     * Returns the number of scientist assigned to this project
     * @return Number of assigned scientist.
     */
    int getAssigned() =>
	    _assigned;

    RuleResearch getRules() =>
	    _project;
}
