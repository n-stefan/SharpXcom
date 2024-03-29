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
        foreach (var i in ((YamlMappingNode)node["criteria"]).Children)
        {
            var key = i.Key.ToString();
            var value = ((YamlSequenceNode)i.Value).Children.Select(x => int.Parse(x.ToString())).ToList();
            _criteria.Add(key, value);
        }
	    _sprite = int.Parse(node["sprite"].ToString());
        _killCriteria = new List<List<KeyValuePair<int, List<string>>>>();
        foreach (var i in ((YamlSequenceNode)node["killCriteria"]).Children)
        {
            var list = new List<KeyValuePair<int, List<string>>>();
            foreach (var j in ((YamlMappingNode)i).Children)
            {
                var key = int.Parse(j.Key.ToString());
                var value = ((YamlSequenceNode)j.Value).Children.Select(x => x.ToString()).ToList();
                list.Add(KeyValuePair.Create(key, value));
            }
            _killCriteria.Add(list);
        }
    }

    /**
     * Get the commendation's description.
     * @return string Commendation description.
     */
    internal string getDescription() =>
	    _description;

    /**
     * Get the commendation's sprite.
     * @return int Sprite number.
     */
    internal int getSprite() =>
	    _sprite;

    /**
     * Get the commendation's award criteria.
     * @return map<string, int> Commendation criteria.
     */
    internal Dictionary<string, List<int>> getCriteria() =>
        _criteria;

    /**
     * Get the commendation's award kill criteria.
     * @return vecotr<string> Commendation kill criteria.
     */
    internal List<List<KeyValuePair<int, List<string>>>> getKillCriteria() =>
	    _killCriteria;
}
