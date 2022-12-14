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
 * Represents a country that funds the player.
 * Contains variable info about a country like
 * monthly funding and various activities.
 */
internal class Country
{
    RuleCountry _rules;
    bool _pact, _newPact;
    List<int> _funding, _activityXcom, _activityAlien;
    int _satisfaction;

    /**
     * Initializes a country of the specified type.
     * @param rules Pointer to ruleset.
     * @param gen Generate new funding.
     */
    Country(RuleCountry rules, bool gen)
    {
        _rules = rules;
        _pact = false;
        _newPact = false;
        _funding = null;
        _satisfaction = 2;

        if (gen)
        {
            _funding.Add(_rules.generateFunding());
        }
        _activityAlien.Add(0);
        _activityXcom.Add(0);
    }

    /**
     *
     */
    ~Country() { }

    /**
     * Saves the country to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "type", _rules.getType() },
            { "funding", new YamlSequenceNode(_funding.Select(x => new YamlScalarNode(x.ToString()))) },
            { "activityXcom", new YamlSequenceNode(_activityXcom.Select(x => new YamlScalarNode(x.ToString()))) },
            { "activityAlien", new YamlSequenceNode(_activityAlien.Select(x => new YamlScalarNode(x.ToString()))) }
        };
        if (_pact)
	    {
            node.Add("pact", _pact.ToString());
	    }
	    else if (_newPact)
	    {
		    node.Add("newPact", _newPact.ToString());
	    }
	    return node;
    }
}
