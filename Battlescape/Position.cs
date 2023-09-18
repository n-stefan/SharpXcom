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

namespace SharpXcom.Battlescape;

/**
 * Easy handling of X-Y-Z coordinates.
 */
internal class Position
{
	internal int x, y, z;

    /// Null position constructor.
    internal Position()
    {
        x = 0;
        y = 0;
        z = 0;
    }

    /// X Y Z position constructor.
    internal Position(int x_, int y_, int z_)
    {
        x = x_;
        y = y_;
        z = z_;
    }

    /// Copy constructor.
    internal Position(Position pos)
    {
        x = pos.x;
        y = pos.y;
        z = pos.z;
    }

    public static Position operator +(Position a, Position b) =>
        new(a.x + b.x, a.y + b.y, a.z + b.z);

    public static Position operator -(Position a, Position b) =>
        new(a.x - b.x, a.y - b.y, a.z - b.z);

    public static Position operator *(Position a, Position b) =>
        new(a.x * b.x, a.y * b.y, a.z * b.z);

    public static Position operator /(Position a, Position b) =>
        new(a.x / b.x, a.y / b.y, a.z / b.z);

    public static Position operator /(Position a, int b) =>
        new(a.x / b, a.y / b, a.z / b);

    /**
	 * Loads the Position from a YAML file.
	 * @param node YAML node.
	 */
    internal void load(YamlNode node)
    {
        x = int.Parse(node["x"].ToString());
        y = int.Parse(node["y"].ToString());
        z = int.Parse(node["z"].ToString());
    }

    /**
     * Saves the Position to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "x", x.ToString() },
            { "y", y.ToString() },
            { "z", z.ToString() }
        };
        return node;
    }
}
