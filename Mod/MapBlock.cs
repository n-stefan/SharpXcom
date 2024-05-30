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

enum MapBlockType { MT_UNDEFINED = -1, MT_DEFAULT, MT_LANDINGZONE, MT_EWROAD, MT_NSROAD, MT_CROSSING };

/**
 * Represents a Terrain Map Block.
 * It contains constant info about this mapblock, like its name, dimensions, attributes...
 * Map blocks are stored in RuleTerrain objects.
 * @sa http://www.ufopaedia.org/index.php?title=MAPS_Terrain
 */
internal class MapBlock
{
    string _name;
    int _size_x, _size_y, _size_z;
    List<int> _groups, _revealedFloors;
    Dictionary<string, List<Position>> _items;

    internal MapBlock() { }

    /**
     * MapBlock construction.
     */
    internal MapBlock(string name)
    {
        _name = name;
        _size_x = 10;
        _size_y = 10;
        _size_z = 4;

        _groups.Add(0);
    }

    /**
     * MapBlock destruction.
     */
    ~MapBlock() { }

	/**
	 * Loads the map block from a YAML file.
	 * @param node YAML node.
	 */
	internal void load(YamlNode node)
	{
		_name = node["name"].ToString();
		_size_x = int.Parse(node["width"].ToString());
		_size_y = int.Parse(node["length"].ToString());
		_size_z = int.Parse(node["height"].ToString());
		if ((_size_x % 10) != 0 || (_size_y % 10) != 0)
		{
			string ss = $"Error: MapBlock {_name}: Size must be divisible by ten";
			throw new Exception(ss);
		}
		if (node["groups"] is YamlSequenceNode map1)
		{
			_groups.Clear();
			if (map1.NodeType == YamlNodeType.Sequence)
			{
                _groups = map1.Children.Select(x => int.Parse(x.ToString())).ToList();
			}
			else
			{
				_groups.Add(int.Parse(map1.ToString()));
			}
		}
		if (node["revealedFloors"] is YamlSequenceNode map2)
		{
			_revealedFloors.Clear();
			if (map2.NodeType == YamlNodeType.Sequence)
			{
                _revealedFloors = map2.Children.Select(x => int.Parse(x.ToString())).ToList();
			}
			else
			{
				_revealedFloors.Add(int.Parse(map2.ToString()));
			}
		}
        _items = new Dictionary<string, List<Position>>();
        foreach (var i in ((YamlMappingNode)node["items"]).Children)
        {
            var key = i.Key.ToString();
            var value = ((YamlSequenceNode)i.Value).Children.Select(x => Position.decode(x)).ToList();
			_items.Add(key, value);
        }
	}

	/**
	 * Gets the MapBlock size x.
	 * @return The size x in tiles.
	 */
	internal int getSizeX() =>
		_size_x;

	/**
	 * Gets the MapBlock size y.
	 * @return The size y in tiles.
	 */
	internal int getSizeY() =>
		_size_y;

    /**
     * Gets the type of mapblock.
     * @return The mapblock's type.
     */
    internal bool isInGroup(int group) =>
        _groups.Contains(group);

    /**
     * Sets the MapBlock size z.
     * @param size_z The size z.
     */
    internal void setSizeZ(int size_z) =>
        _size_z = size_z;

	/**
	 * Gets the MapBlock name (string).
	 * @return The name.
	 */
	internal string getName() =>
		_name;

    /**
     * Gets if this floor should be revealed or not.
     */
    internal bool isFloorRevealed(int floor) =>
        _revealedFloors.Contains(floor);

    /**
     * Gets the items and their positioning for any items associated with this block.
     * @return the items and their positions.
     */
    internal Dictionary<string, List<Position>> getItems() =>
        _items;

	/**
	 * Gets the MapBlock size z.
	 * @return The size z.
	 */
	internal int getSizeZ() =>
		_size_z;
}
