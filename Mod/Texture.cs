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

struct TerrainCriteria
{
    internal string name;
    internal int weight;
    internal double lonMin, lonMax, latMin, latMax;
    
    public TerrainCriteria()
    {
        weight = 1;
        lonMin = 0.0;
        lonMax = 360.0;
        latMin = -90.0;
        latMax = 90.0;
    }

    /**
	 * Loads the TerrainCriteria from a YAML file.
	 * @param node YAML node.
	 */
    internal void load(YamlNode node)
    {
        name = node["name"].ToString();
        weight = int.Parse(node["weight"].ToString());
        lonMin = double.Parse(node["lonMin"].ToString());
        lonMax = double.Parse(node["lonMax"].ToString());
        latMin = double.Parse(node["latMin"].ToString());
        latMax = double.Parse(node["latMax"].ToString());
    }
}

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

    /**
     * Calculates a random deployment for a mission target based
     * on the texture's available deployments.
     * @return the name of the picked deployment.
     */
    internal string getRandomDeployment()
    {
	    if (!_deployments.Any())
	    {
		    return string.Empty;
	    }

        if (_deployments.Count == 1)
        {
            return _deployments.First().Key;
        }
	    int totalWeight = 0;

	    foreach (var i in _deployments)
	    {
		    totalWeight += i.Value;
	    }

	    if (totalWeight >= 1)
	    {
		    int pick = RNG.generate(1, totalWeight);
		    foreach (var i in _deployments)
		    {
			    if (pick <= i.Value)
			    {
				    return i.Key;
			    }
			    else
			    {
				    pick -= i.Value;
			    }
		    }
	    }

	    return string.Empty;
    }

    /**
     * Returns the list of deployments associated
     * with this texture.
     * @return List of deployments.
     */
    internal Dictionary<string, int> getDeployments() =>
	    _deployments;

    /**
     * Returns the list of terrain criteria associated
     * with this texture.
     * @return List of terrain.
     */
    internal List<TerrainCriteria> getTerrain() =>
        _terrain;

    /**
     * Calculates a random terrain for a mission target based
     * on the texture's available terrain criteria.
     * @param target Pointer to the mission target.
     * @return the name of the picked terrain.
     */
    internal string getRandomTerrain(Target target)
    {
	    int totalWeight = 0;
	    var possibilities = new Dictionary<int, string>();
	    foreach (var i in _terrain)
	    {
		    if (i.weight > 0 &&
			    target.getLongitude() >= i.lonMin && target.getLongitude() < i.lonMax &&
			    target.getLatitude() >= i.latMin && target.getLatitude() < i.latMax)
		    {
			    totalWeight += i.weight;
			    possibilities[totalWeight] = i.name;
		    }
	    }
	    if (totalWeight > 0)
	    {
		    int pick = RNG.generate(1, totalWeight);
		    foreach (var i in possibilities)
		    {
			    if (pick <= i.Key)
			    {
				    return i.Value;
			    }
		    }
	    }
	    return string.Empty;
    }
}
