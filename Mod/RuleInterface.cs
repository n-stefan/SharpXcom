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

struct Element
{
    /// basic rect info, and 3 colors.
    internal int x, y, w, h, color, color2, border;
    /// defines inversion behaviour
    bool TFTDMode;
};

internal class RuleInterface : IRule
{
    string _type;
    string _music;
    Dictionary<string, Element> _elements;

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

    internal string getMusic() =>
	    _music;
}
