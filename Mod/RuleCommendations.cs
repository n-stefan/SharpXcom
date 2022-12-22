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
 * Represents a specific type of commendation.
 * Contains constant info about a commendation like
 * award criteria, sprite, description, etc.
 * @sa Commendation
 */
internal class RuleCommendations
{
    Dictionary<string, List<int>> _criteria;
    List<List<KeyValuePair<int, List<string>>>> _killCriteria;
    string _description;
    int _sprite;

    /**
     * Creates a blank set of commendation data.
     */
    internal RuleCommendations()
    {
        _criteria = null;
        _killCriteria = null;
        _description = string.Empty;
        _sprite = 0;
    }

    /**
     * Cleans up the commendation.
     */
    ~RuleCommendations() { }

    /**
     * Loads the commendations from YAML.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
        _description = node["description"].ToString();
        _criteria = new Dictionary<string, List<int>>();
        foreach (var criteria in ((YamlMappingNode)node["criteria"]).Children)
        {
            var key = criteria.Key.ToString();
            var value = new List<int>();
            foreach (var child in ((YamlSequenceNode)criteria.Value).Children)
            {
                value.Add(int.Parse(child.ToString()));
            }
            _criteria.Add(key, value);
        }
	    _sprite = int.Parse(node["sprite"].ToString());
        _killCriteria = new List<List<KeyValuePair<int, List<string>>>>();
        foreach (var killCriteria in ((YamlSequenceNode)node["killCriteria"]).Children)
        {
            var list = new List<KeyValuePair<int, List<string>>>();
            foreach (var child in ((YamlMappingNode)killCriteria).Children)
            {
                var key = int.Parse(child.Key.ToString());
                var value = ((YamlSequenceNode)child.Value).Children.Select(x => x.ToString()).ToList();
                list.Add(KeyValuePair.Create(key, value));
            }
            _killCriteria.Add(list);
        }
    }
}
