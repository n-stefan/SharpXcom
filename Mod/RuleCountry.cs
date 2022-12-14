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
 * Represents a specific funding country.
 * Contains constant info like its location in the
 * world and starting funding range.
 */
internal class RuleCountry : IRule
{
    string _type;
    int _fundingBase, _fundingCap;
    double _labelLon, _labelLat;

    /**
     * Creates a blank ruleset for a certain
     * type of country.
     * @param type String defining the type.
     */
    RuleCountry(string type)
    {
        _type = type;
        _fundingBase = 0;
        _fundingCap = 0;
        _labelLon = 0.0;
        _labelLat = 0.0;
    }

    public IRule Create(string type) =>
        new RuleCountry(type);

    /**
     *
     */
    ~RuleCountry() { }

    /**
     * Generates the random starting funding for the country.
     * @return The monthly funding.
     */
    internal int generateFunding() =>
	    RNG.generate(_fundingBase, _fundingBase * 2) * 1000;

    /**
     * Gets the language string that names
     * this country. Each country type
     * has a unique name.
     * @return The country's name.
     */
    internal string getType() =>
	    _type;
}
