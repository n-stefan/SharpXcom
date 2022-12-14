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
 * Holds pairs of relative weights and IDs.
 * It is used to store options and make a random choice between them.
 */
internal class WeightedOptions
{
    Dictionary<string, uint> _choices; //!< Options and weights

    /**
	 * Send the WeightedOption contents to a YAML::Emitter.
	 * @return YAML node.
	 */
    internal YamlNode save()
	{
		var node = new YamlMappingNode();
		foreach (var choice in _choices)
		{
			node.Add(choice.Key, choice.Value.ToString());
		}
		return node;
	}

    /**
     * Get the list of strings associated with these weights.
     * @return the list of strings in these weights.
     */
    internal List<string> getNames()
    {
        var names = new List<string>();
        foreach (var choice in _choices)
        {
            names.Add(choice.Key);
        }
        return names;
    }
}
