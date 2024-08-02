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

enum AlienRank { AR_HUMAN = -1, AR_COMMANDER, AR_LEADER, AR_ENGINEER, AR_MEDIC, AR_NAVIGATOR, AR_SOLDIER, AR_TERRORIST, AR_TERRORIST2 };

/**
 * Represents a specific race "family", or a "main race" if you wish.
 * Here is defined which ranks it contains and also which accompanying terror units.
 */
internal class AlienRace : IRule
{
    string _id;
    List<string> _members;

    /**
     * Creates a blank alien race.
     * @param id String defining the id.
     */
    AlienRace(string id) =>
        _id = id;

    public IRule Create(string type) =>
        new AlienRace(type);

    ~AlienRace() { }

    /**
     * Loads the alien race from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    _id = node["id"].ToString();
        _members = ((YamlSequenceNode)node["members"]).Children.Select(x => x.ToString()).ToList();
    }

    /**
     * Gets a certain member of this alien race family.
     * @param id The member's id.
     * @return The member's name.
     */
    internal string getMember(int id) =>
	    _members[id];

    /**
     * Returns the language string that names
     * this alien race. Each race has a unique name.
     * @return Race name.
     */
    string getId() =>
	    _id;
}
