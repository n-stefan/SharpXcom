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
 * For adding a set of extra strings to the game.
 */
internal class ExtraStrings
{
    Dictionary<string, string> _strings;

    /**
     * Creates a blank set of extra strings data.
     */
    internal ExtraStrings() { }

    /**
     * Cleans up the extra strings set.
     */
    ~ExtraStrings() { }

    /**
     * Gets the list of strings defined my this mod.
     * @return The list of strings.
     */
    internal Dictionary<string, string> getStrings() =>
        _strings;

	/**
	 * Loads the extra strings set from YAML.
	 * @param node YAML node.
	 */
	internal void load(YamlNode node)
	{
		foreach (var i in ((YamlMappingNode)node["strings"]).Children)
		{
			// Regular strings
			if (i.Value is YamlScalarNode)
			{
				_strings[i.Key.ToString()] = i.Value.ToString();
			}
			// Strings with plurality
			else if (i.Value is YamlMappingNode m)
			{
				foreach (var j in m.Children)
				{
					string s = i.Key.ToString() + "_" + j.Key.ToString();
					_strings[s] = j.Value.ToString();
				}
			}
		}
	}
}
