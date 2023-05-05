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
    List<string> _dependencies, _unlocks, _getOneFree, _requires;

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

    /**
     * Loads the research project from a YAML file.
     * @param node YAML node.
     * @param listOrder The list weight for this research.
     */
    internal void load(YamlNode node, int listOrder)
    {
	    _name = node["name"].ToString();
	    _lookup = node["lookup"].ToString();
	    _cutscene = node["cutscene"].ToString();
	    _cost = int.Parse(node["cost"].ToString());
	    _points = int.Parse(node["points"].ToString());
        _dependencies = ((YamlSequenceNode)node["dependencies"]).Children.Select(x => x.ToString()).ToList();
        _unlocks = ((YamlSequenceNode)node["unlocks"]).Children.Select(x => x.ToString()).ToList();
        _getOneFree = ((YamlSequenceNode)node["getOneFree"]).Children.Select(x => x.ToString()).ToList();
        _requires = ((YamlSequenceNode)node["requires"]).Children.Select(x => x.ToString()).ToList();
	    _needItem = bool.Parse(node["needItem"].ToString());
	    _destroyItem = bool.Parse(node["destroyItem"].ToString());
        _listOrder = int.Parse(node["listOrder"].ToString());
	    if (_listOrder == 0)
	    {
            _listOrder = listOrder;
	    }
	    // This is necessary, research code assumes it!
	    if (_requires.Any() && _cost != 0)
	    {
		    throw new Exception("Research topic " + _name + " has requirements, but the cost is not zero. Sorry, this is not allowed!");
	    }
    }

    /**
     * Gets the cost of this ResearchProject.
     * @return The cost of this ResearchProject (in man/day).
     */
    internal int getCost() =>
	    _cost;

    /**
     * Gets the list of ResearchProjects granted at random for free by this research.
     * @return The list of ResearchProjects.
     */
    internal List<string> getGetOneFree() =>
	    _getOneFree;

    /**
     * Gets the requirements for this ResearchProject.
     * @return The requirement for this research.
     */
    internal List<string> getRequirements() =>
	    _requires;

    /**
     * Gets the list of ResearchProjects unlocked by this research.
     * @return The list of ResearchProjects.
     */
    internal List<string> getUnlocked() =>
	    _unlocks;

    /**
     * Checks if this ResearchProject needs a corresponding Item to be researched.
     *  @return True if the ResearchProject needs a corresponding item.
     */
    internal bool needItem() =>
	    _needItem;

    /**
     * Gets the list of dependencies, i.e. ResearchProjects, that must be discovered before this one.
     * @return The list of ResearchProjects.
     */
    internal List<string> getDependencies() =>
	    _dependencies;

    /**
     * Get the points earned for this ResearchProject.
     * @return The points earned for this ResearchProject.
     */
    internal int getPoints() =>
	    _points;

    /**
     * Checks if this ResearchProject needs a corresponding Item to be researched.
     *  @return True if the ResearchProject needs a corresponding item.
     */
    internal bool destroyItem() =>
	    _destroyItem;

    /**
     * Gets what article to look up in the ufopedia.
     * @return The article to look up in the ufopaedia
     */
    internal string getLookup() =>
	    _lookup;

    /**
     * Gets the cutscene to play when this research item is completed.
     * @return The cutscene id.
     */
    internal string getCutscene() =>
	    _cutscene;
}
