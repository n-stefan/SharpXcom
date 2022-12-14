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
    Region(RuleRegion rules)
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
}
