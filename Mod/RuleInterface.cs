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

record struct Element
{
    /// basic rect info, and 3 colors.
    internal int x, y, w, h, color, color2, border;
    /// defines inversion behaviour
    internal bool TFTDMode;
};

internal class RuleInterface : IRule
{
    string _type;
    string _music;
    Dictionary<string, Element> _elements;
    string _palette;
    string _parent;

    /**
     * Creates a blank ruleset for a certain
     * type of interface, containing an index of elements that make it up.
     * @param type String defining the type.
     */
    RuleInterface(string type) =>
        _type = type;

    public IRule Create(string type) =>
        new RuleInterface(type);

    ~RuleInterface() { }

    /**
     * Retrieves info on an element
     * @param id String defining the element.
     */
    internal Element getElement(string id) =>
	    _elements.TryGetValue(id, out var element) ? element : default;

    internal string getPalette() =>
        _palette;

    internal string getParent() =>
        _parent;

    internal string getMusic() =>
	    _music;

	/**
	 * Loads the elements from a YAML file.
	 * @param node YAML node.
	 */
	internal void load(YamlNode node)
	{
		_palette = node["palette"].ToString();
		_parent = node["parent"].ToString();
		_music = node["music"].ToString();
		foreach (var item in ((YamlSequenceNode)node["elements"]).Children)
		{
			Element element;
			if (item["size"] != null)
			{
				var pos = item["size"];
				element.w = int.Parse(pos[0].ToString());
				element.h = int.Parse(pos[1].ToString());
			}
			else
			{
				element.w = element.h = int.MaxValue;
			}
			if (item["pos"] != null)
			{
				var pos = item["pos"];
				element.x = int.Parse(pos[0].ToString());
				element.y = int.Parse(pos[1].ToString());
			}
			else
			{
				element.x = element.y = int.MaxValue;
			}
			element.color = int.Parse(item["color"].ToString());
			element.color2 = int.Parse(item["color2"].ToString());
			element.border = int.Parse(item["border"].ToString());
			element.TFTDMode = bool.Parse(item["TFTDMode"].ToString());

			string id = item["id"].ToString();
			_elements[id] = element;
		}
	}
}
