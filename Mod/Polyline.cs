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
 * Represents a polyline in the world map.
 * Polylines constitute the detail portions of the
 * X-Com globe and typically represent borders and rivers.
 */
internal class Polyline
{
    double[] _lat, _lon;
	int _points;

    /**
     * Initializes the polyline with arrays to store each point's coordinates.
     * @param points Number of points.
     */
    internal Polyline(int points)
    {
        _points = points;

        _lat = new double[points];
        _lon = new double[points];
    }

    /**
     * Deletes the arrays from memory.
     */
    ~Polyline()
    {
        _lat = null;
        _lon = null;
    }

    /**
     * Loads the polyline from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    _lat = null;
	    _lon = null;

        var coords = ((YamlSequenceNode)node).Children.Select(x => double.Parse(x.ToString())).ToList();
        _points = coords.Count / 2;
	    _lat = new double[_points];
	    _lon = new double[_points];

	    for (var i = 0; i < coords.Count; i += 2)
	    {
            uint j = (uint)(i / 2);
		    _lon[j] = Deg2Rad(coords[i]);
		    _lat[j] = Deg2Rad(coords[i+1]);
	    }
    }

    /**
     * Returns the latitude (X) of a given point.
     * @param i Point number (0-max).
     * @return Point's latitude.
     */
    internal double getLatitude(int i) =>
	    _lat[i];

    /**
     * Returns the longitude (Y) of a given point.
     * @param i Point number (0-max).
     * @return Point's longitude.
     */
    internal double getLongitude(int i) =>
	    _lon[i];

    /**
     * Returns the number of points (vertexes) that make up the polyline.
     * @return Number of points.
     */
    internal int getPoints() =>
	    _points;

    /**
     * Changes the latitude of a given point.
     * @param i Point number (0-max).
     * @param lat Point's latitude.
     */
    void setLatitude(int i, double lat) =>
	    _lat[i] = lat;

    /**
     * Changes the latitude of a given point.
     * @param i Point number (0-max).
     * @param lon Point's longitude.
     */
    void setLongitude(int i, double lon) =>
	    _lon[i] = lon;
}
