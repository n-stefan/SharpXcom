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
    uint _totalWeight; //!< The total weight of all options.

    /**
	 * Add the weighted options from a YAML::Node to a WeightedOptions.
	 * The weight option list is not replaced, only values in @a nd will be added /
	 * changed.
	 * @param nd The YAML node (containing a map) with the new values.
	 */
    internal void load(YamlNode nd)
	{
		foreach (var val in ((YamlMappingNode)nd).Children)
		{
			string id = val.Key.ToString();
            uint w = uint.Parse(val.Value.ToString());
			set(id, w);
		}
	}

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
        foreach (var ii in _choices)
        {
            names.Add(ii.Key);
        }
        return names;
    }

	/**
	 * Set an option's weight.
	 * If @a weight is set to 0, the option is removed from the list of choices.
	 * If @a id already exists, the new weight replaces the old one, otherwise
	 * @a id is added to the list of choices, with @a weight as the weight.
	 * @param id The option name.
	 * @param weight The option's new weight.
	 */
	internal void set(string id, uint weight)
	{
		if (_choices.ContainsKey(id))
		{
			_totalWeight -= _choices[id];
			if (0 != weight)
			{
				_choices[id] = weight;
				_totalWeight += weight;
			}
			else
			{
				_choices.Remove(id);
			}
		}
		else if (0 != weight)
		{
			_choices.Add(id, weight);
			_totalWeight += weight;
		}
	}

    /// Is this empty?
    internal bool empty() =>
		0 == _totalWeight;

    /// Remove all entries.
    internal void clear() { _totalWeight = 0; _choices.Clear(); }

	/**
	 * Select a random choice from among the contents.
	 * This MUST be called on non-empty objects.
	 * Each time this is called, the returned value can be different.
	 * @return The key of the selected choice.
	 */
	internal string choose()
	{
		if (_totalWeight == 0)
		{
			return string.Empty;
		}
		uint var = (uint)RNG.generate(0, _totalWeight);
		foreach (var ii in _choices)
		{
			if (var <= ii.Value)
				return ii.Key;
			var -= ii.Value;
		}
		// We always have a valid iterator here.
		return string.Empty;
	}
}
