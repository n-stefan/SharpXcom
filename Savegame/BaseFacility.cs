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
 * Represents a base facility placed in a base.
 * Contains variable info about a facility like
 * position and build time.
 * @sa RuleBaseFacility
 */
internal class BaseFacility
{
    RuleBaseFacility _rules;
    Base _base;
    int _x, _y, _buildTime;
    Craft _craftForDrawing;	// craft, used for drawing facility

    /**
     * Initializes a base facility of the specified type.
     * @param rules Pointer to ruleset.
     * @param base Pointer to base of origin.
     */
    internal BaseFacility(RuleBaseFacility rules, Base @base)
    {
        _rules = rules;
        _base = @base;
        _x = -1;
        _y = -1;
        _buildTime = 0;
        _craftForDrawing = null;
    }

    /**
     *
     */
    ~BaseFacility() { }

    /**
     * Saves the base facility to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "type", _rules.getType() },
            { "x", _x.ToString() },
            { "y", _y.ToString() }
        };
        if (_buildTime != 0)
		    node.Add("buildTime", _buildTime.ToString());
	    return node;
    }

    /**
     * Changes the base facility's X position on the
     * base grid that it's placed on.
     * @param x X position in grid squares.
     */
    internal void setX(int x) =>
        _x = x;

    /**
     * Changes the base facility's Y position on the
     * base grid that it's placed on.
     * @param y Y position in grid squares.
     */
    internal void setY(int y) =>
        _y = y;

    /**
     * Changes the base facility's remaining time
     * until it's finished building.
     * @param time Time left in days.
     */
    internal void setBuildTime(int time) =>
        _buildTime = time;

    /**
     * Returns the ruleset for the base facility's type.
     * @return Pointer to ruleset.
     */
    internal RuleBaseFacility getRules() =>
	    _rules;

    /**
     * Returns the base facility's remaining time
     * until it's finished building (0 = complete).
     * @return Time left in days.
     */
    internal int getBuildTime() =>
	    _buildTime;

    /**
     * Handles the facility building every day.
     */
    internal void build() =>
        _buildTime--;

    /**
     * Gets craft, used for drawing facility.
     * @return craft
     */
    internal Craft getCraft() =>
	    _craftForDrawing;

    /**
     * Sets craft, used for drawing facility.
     * @param craft for drawing hangar.
     */
    internal void setCraft(Craft craft) =>
        _craftForDrawing = craft;

    /**
     * Returns the base facility's X position on the
     * base grid that it's placed on.
     * @return X position in grid squares.
     */
    internal int getX() =>
	    _x;

    /**
     * Returns the base facility's Y position on the
     * base grid that it's placed on.
     * @return Y position in grid squares.
     */
    internal int getY() =>
	    _y;

    /**
     * Returns if this facility is currently being
     * used by its base.
     * @return True if it's under use, False otherwise.
     */
    internal bool inUse()
    {
	    if (_buildTime > 0)
	    {
		    return false;
	    }
	    return ((_rules.getPersonnel() > 0 && _base.getAvailableQuarters() - _rules.getPersonnel() < _base.getUsedQuarters()) ||
			    (_rules.getStorage() > 0 && _base.getAvailableStores() - _rules.getStorage() < _base.getUsedStores()) ||
			    (_rules.getLaboratories() > 0 && _base.getAvailableLaboratories() - _rules.getLaboratories() < _base.getUsedLaboratories()) ||
			    (_rules.getWorkshops() > 0 && _base.getAvailableWorkshops() - _rules.getWorkshops() < _base.getUsedWorkshops()) ||
			    (_rules.getCrafts() > 0 && _base.getAvailableHangars() - _rules.getCrafts() < _base.getUsedHangars()) ||
			    (_rules.getPsiLaboratories() > 0 && _base.getAvailablePsiLabs() - _rules.getPsiLaboratories() < _base.getUsedPsiLabs()) ||
			    (_rules.getAliens() > 0 && _base.getAvailableContainment() - _rules.getAliens() < _base.getUsedContainment()));
    }

    /**
     * Loads the base facility from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    _x = int.Parse(node["x"].ToString());
	    _y = int.Parse(node["y"].ToString());
	    _buildTime = int.Parse(node["buildTime"].ToString());
    }
}
