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
 * Represents the relations between a Geoscape texture
 * and the corresponding Battlescape mission attributes.
 */
internal class Texture
{
    int _id;
    Dictionary<string, int> _deployments;
    List<TerrainCriteria> _terrain;

    /**
     * Initializes a globe texture.
     * @param id Texture identifier.
     */
    internal Texture(int id) =>
        _id = id;

    /**
     *
     */
    ~Texture() { }

    /**
     * Loads the texture type from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    _id = int.Parse(node["id"].ToString());
        _deployments = ((YamlMappingNode)node["deployments"]).Children.ToDictionary(x => x.Key.ToString(), x => int.Parse(x.Value.ToString()));
        _terrain = ((YamlSequenceNode)node["terrain"]).Children.Select(x =>
        {
            var terrain = new TerrainCriteria(); terrain.load(x); return terrain;
        }).ToList();
    }
}
