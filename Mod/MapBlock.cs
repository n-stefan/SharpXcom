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
		if (node["groups"] is YamlSequenceNode groups)
		{
			_groups.Clear();
			if (groups.NodeType == YamlNodeType.Sequence)
			{
                _groups = groups.Children.Select(x => int.Parse(x.ToString())).ToList();
			}
			else
			{
				_groups.Add(int.Parse(groups.ToString()));
			}
		}
		if (node["revealedFloors"] is YamlSequenceNode revealedFloors)
		{
			_revealedFloors.Clear();
			if (revealedFloors.NodeType == YamlNodeType.Sequence)
			{
                _revealedFloors = revealedFloors.Children.Select(x => int.Parse(x.ToString())).ToList();
			}
			else
			{
				_revealedFloors.Add(int.Parse(revealedFloors.ToString()));
			}
		}
        _items = new Dictionary<string, List<Position>>();
        foreach (var item in ((YamlMappingNode)node["items"]).Children)
        {
            var key = item.Key.ToString();
            var value = ((YamlSequenceNode)item.Value).Children.Select(x =>
			{
				var pos = new Position(); pos.load(x); return pos;
            }).ToList();
			_items.Add(key, value);
        }
	}
}
