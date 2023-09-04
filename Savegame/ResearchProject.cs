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
    const float PROGRESS_LIMIT_UNKNOWN = 0.333f;
    const float PROGRESS_LIMIT_POOR = 0.07f;
    const float PROGRESS_LIMIT_AVERAGE = 0.13f;
    const float PROGRESS_LIMIT_GOOD = 0.25f;

    RuleResearch _project;
    int _assigned;
    int _spent;
    int _cost;

    internal ResearchProject(RuleResearch p, int c)
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
    internal int getCost() =>
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
    internal int getAssigned() =>
	    _assigned;

    internal RuleResearch getRules() =>
	    _project;

    /**
     * Changes the number of scientist to the ResearchProject
     * @param nb number of scientist assigned to this ResearchProject
     */
    internal void setAssigned(int nb) =>
        _assigned = nb;

    /**
     * Changes the cost of the ResearchProject
     * @param spent new project cost(in man/day)
     */
    internal void setSpent(int spent) =>
        _spent = spent;

    /**
     * Called every day to compute time spent on this ResearchProject
     * @return true if the ResearchProject is finished
     */
    internal bool step()
    {
        _spent += _assigned;
        return isFinished();
    }

    /**
     * gets state of project.
     */
    internal bool isFinished() =>
        _spent >= getCost();

    /**
     * Return a string describing Research progress.
     * @return a string describing Research progress.
     */
    internal string getResearchProgress()
    {
	    float progress = (float)getSpent() / getRules().getCost();
	    if (getAssigned() == 0)
	    {
		    return "STR_NONE";
	    }
	    else if (progress <= PROGRESS_LIMIT_UNKNOWN)
	    {
		    return "STR_UNKNOWN";
	    }
	    else
	    {
		    float rating = (float)getAssigned();
		    rating /= getRules().getCost();
		    if (rating <= PROGRESS_LIMIT_POOR)
		    {
			    return "STR_POOR";
		    }
		    else if (rating <= PROGRESS_LIMIT_AVERAGE)
		    {
			    return "STR_AVERAGE";
		    }
		    else if (rating <= PROGRESS_LIMIT_GOOD)
		    {
			    return "STR_GOOD";
		    }
		    return "STR_EXCELLENT";
	    }
    }
}
