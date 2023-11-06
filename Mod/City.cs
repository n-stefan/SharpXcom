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
 * Represents a city of the world.
 * Aliens target cities for certain missions.
 */
internal class City : Target
{
    /**
     * Initializes a city with certain data.
     * @param name Name of the city.
     * @param lon Longitude of the city.
     * @param lat Latitude of the city.
     */
    City(string name, double lon, double lat) : base()
    {
        _name = name;
        _lon = lon;
        _lat = lat;
    }

    /**
     *
     */
    ~City() { }

    /**
     * Returns the globe marker for the UFO.
     * @return Marker sprite, -1 if none.
     */
    protected override int getMarker() =>
	    8;

    /**
     * Returns the city's unique identifying name.
     * @param lang Language to get strings from.
     * @return Full name.
     */
    internal override string getName(Language lang) =>
	    lang.getString(_name);
}
