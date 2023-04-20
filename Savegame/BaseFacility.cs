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
}
