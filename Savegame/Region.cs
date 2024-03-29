﻿/*
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
 * Represents a region of the world.
 * Contains variable info about a region like
 * X-Com and alien activity in it.
 */
internal class Region
{
    RuleRegion _rules;
    List<int> _activityXcom, _activityAlien;

    /**
     * Initializes a region of the specified type.
     * @param rules Pointer to ruleset.
     */
    internal Region(RuleRegion rules)
    {
        _rules = rules;
        _activityAlien.Add(0);
        _activityXcom.Add(0);
    }

    /**
     *
     */
    ~Region() { }

    /**
     * Saves the region to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "type", _rules.getType() },
            { "activityXcom", new YamlSequenceNode(_activityXcom.Select(x => new YamlScalarNode(x.ToString()))) },
            { "activityAlien", new YamlSequenceNode(_activityAlien.Select(x => new YamlScalarNode(x.ToString()))) }
        };
        return node;
    }

    /**
     * Gets the region's alien activity level.
     * @return activity level.
     */
    internal ref List<int> getActivityAlien() =>
	    ref _activityAlien;

    /**
     * Gets the region's xcom activity level.
     * @return activity level.
     */
    internal ref List<int> getActivityXcom() =>
	    ref _activityXcom;

    /**
     * Returns the ruleset for the region's type.
     * @return Pointer to ruleset.
     */
    internal RuleRegion getRules() =>
	    _rules;

    /**
     * Adds to the country's alien activity level.
     * @param activity how many points to add.
     */
    internal void addActivityAlien(int activity) =>
        _activityAlien[^1] += activity;

    /**
     * Adds to the region's xcom activity level.
     * @param activity Amount to add.
     */
    internal void addActivityXcom(int activity) =>
        _activityXcom[^1] += activity;

    /**
     * Store last month's counters, start new counters.
     */
    internal void newMonth()
    {
        _activityAlien.Add(0);
        _activityXcom.Add(0);
        if (_activityAlien.Count > 12)
            _activityAlien.RemoveAt(0);
        if (_activityXcom.Count > 12)
            _activityXcom.RemoveAt(0);
    }

    /**
     * Loads the region from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
        _activityXcom = ((YamlSequenceNode)node["activityXcom"]).Children.Select(x => int.Parse(x.ToString())).ToList();
        _activityAlien = ((YamlSequenceNode)node["activityAlien"]).Children.Select(x => int.Parse(x.ToString())).ToList();
    }
}
