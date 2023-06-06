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
 * Represents a polygon in the world map.
 * Polygons constitute the textured land portions
 * of the X-Com globe and typically have 3-4 points.
 */
internal class Polygon
{
	double[] _lat, _lon;
	short[] _x, _y;
	int _points, _texture;

    /**
	 * Initializes the polygon with arrays to store each point's coordinates.
	 * @param points Number of points.
	 */
    internal Polygon(int points)
	{
		_points = points;
		_texture = 0;

		_lat = new double[_points];
		_lon = new double[_points];
		_x = new short[_points];
		_y = new short[_points];
		for (int i = 0; i < _points; ++i)
		{
			_lat[i] = 0.0;
			_lon[i] = 0.0;
			_x[i] = 0;
			_y[i] = 0;
		}
	}

	/**
	 * Performs a deep copy of an existing polygon.
	 * @param other Polygon to copy from.
	 */
	internal Polygon(Polygon other)
	{
		_points = other._points;
		_lat = new double[_points];
		_lon = new double[_points];
		_x = new short[_points];
		_y = new short[_points];
		for (int i = 0; i < _points; ++i)
		{
			_lat[i] = other._lat[i];
			_lon[i] = other._lon[i];
			_x[i] = other._x[i];
			_y[i] = other._y[i];
		}
		_texture = other._texture;
	}

	/**
	 * Deletes the arrays from memory.
	 */
	~Polygon()
	{
		_lat = null;
		_lon = null;
		_x = null;
		_y = null;
	}

	/**
	 * Returns the number of points (vertexes) that make up the polygon.
	 * @return Number of points.
	 */
	internal int getPoints() =>
		_points;

	/**
	 * Returns the latitude of a given point.
	 * @param i Point number (0-max).
	 * @return Point's latitude.
	 */
	internal double getLatitude(int i) =>
		_lat[i];

	/**
	 * Returns the longitude of a given point.
	 * @param i Point number (0-max).
	 * @return Point's longitude.
	 */
	internal double getLongitude(int i) =>
		_lon[i];

    /**
     * Changes the X coordinate of a given point.
     * @param i Point number (0-max).
     * @param x Point's X coordinate.
     */
    internal void setX(int i, short x) =>
        _x[i] = x;

    /**
     * Changes the Y coordinate of a given point.
     * @param i Point number (0-max).
     * @param y Point's Y coordinate.
     */
    internal void setY(int i, short y) =>
        _y[i] = y;

	/**
	 * Loads the polygon from a YAML file.
	 * @param node YAML node.
	 */
	internal void load(YamlNode node)
	{
		_lat = null;
		_lon = null;
		_x = null;
		_y = null;

        var coords = ((YamlSequenceNode)node).Children.Select(x => double.Parse(x.ToString())).ToList();
        _points = (coords.Count - 1) / 2;
		_lat = new double[_points];
		_lon = new double[_points];
		_x = new short[_points];
		_y = new short[_points];

		_texture = (int)coords[0];
		for (var i = 1; i < coords.Count; i += 2)
		{
            uint j = (uint)((i - 1) / 2);
			_lon[j] = Deg2Rad(coords[i]);
			_lat[j] = Deg2Rad(coords[i+1]);
			_x[j] = 0;
			_y[j] = 0;
		}
	}

    /**
     * Changes the latitude of a given point.
     * @param i Point number (0-max).
     * @param lon Point's longitude.
     */
    internal void setLongitude(int i, double lon) =>
        _lon[i] = lon;

    /**
     * Changes the latitude of a given point.
     * @param i Point number (0-max).
     * @param lat Point's latitude.
     */
    internal void setLatitude(int i, double lat) =>
        _lat[i] = lat;

    /**
     * Changes the texture used to draw the polygon.
     * @param tex Texture sprite number.
     */
    internal void setTexture(int tex) =>
        _texture = tex;

	/**
	 * Returns the texture used to draw the polygon
	 * (textures are stored in a set).
	 * @return Texture sprite number.
	 */
	internal int getTexture() =>
		_texture;
}
