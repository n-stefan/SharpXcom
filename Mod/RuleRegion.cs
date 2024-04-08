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
 * Defines a rectangle in polar coordinates.
 * It is used to define areas for a mission zone.
 */
struct MissionArea
{
	internal double lonMin, lonMax, latMin, latMax;
	internal int texture;
	internal string name;

    public static bool operator ==(MissionArea a, MissionArea b) =>
		AreSame(a.lonMax, b.lonMax) && AreSame(a.lonMin, b.lonMin) && AreSame(a.latMax, b.latMax) && AreSame(a.latMin, b.latMin);

    public static bool operator !=(MissionArea a, MissionArea b) =>
        !(a == b);

    internal bool isPoint() =>
		AreSame(lonMin, lonMax) && AreSame(latMin, latMax);

    /**
     * Loads the MissionArea from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
        lonMin = double.Parse(node["lonMin"].ToString());
        lonMax = double.Parse(node["lonMax"].ToString());
        latMin = double.Parse(node["latMin"].ToString());
        latMax = double.Parse(node["latMax"].ToString());
        texture = int.Parse(node["texture"].ToString());
        name = node["name"].ToString();
    }
}

/**
 * A zone (set of areas) on the globe.
 */
struct MissionZone
{
    internal List<MissionArea> areas;

    void swap(MissionZone other) =>
        (other.areas, areas) = (areas, other.areas);

    /**
     * Loads the MissionZone from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
        for (var i = 0; i < areas.Count; i++)
        {
            areas[i].load(node[i]);
        }
    }
}

/**
 * Represents a specific region of the world.
 * Contains constant info about a region like area
 * covered and base construction costs.
 */
internal class RuleRegion : IRule
{
    string _type;
    int _cost;
    List<City> _cities;
    List<double> _lonMin, _lonMax, _latMin, _latMax;
    /// Weight of this region when selecting regions for alien missions.
    uint _regionWeight;
    /// Weighted list of the different mission types for this region.
    WeightedOptions _missionWeights;
    /// All the mission zones in this region.
    List<MissionZone> _missionZones;
    /// Do missions in the region defined by this string instead.
    string _missionRegion;

    /**
     * Creates a blank ruleset for a certain type of region.
     * @param type String defining the type.
     */
    RuleRegion(string type)
    {
        _type = type;
        _cost = 0;
        _regionWeight = 0;
    }

    public IRule Create(string type) =>
        new RuleRegion(type);

    /**
     * Deletes the cities from memory.
     */
    ~RuleRegion() =>
        _cities.Clear();

    /**
     * Gets the language string that names
     * this region. Each region type
     * has a unique name.
     * @return The region type.
     */
    internal string getType() =>
	    _type;

    /// Gets the weighted list of missions for this region.
    internal WeightedOptions getAvailableMissions() =>
        _missionWeights;

    /**
     * Loads the region type from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    _type = node["type"].ToString();
	    _cost = int.Parse(node["cost"].ToString());
        var areas = new List<List<double>>();
        foreach (var i in ((YamlSequenceNode)node["areas"]).Children)
        {
            areas.Add(((YamlSequenceNode)i).Children.Select(x => double.Parse(x.ToString())).ToList());
        }
        for (var i = 0; i != areas.Count; ++i)
        {
            _lonMin.Add(Deg2Rad(areas[i][0]));
            _lonMax.Add(Deg2Rad(areas[i][1]));
            _latMin.Add(Deg2Rad(areas[i][2]));
            _latMax.Add(Deg2Rad(areas[i][3]));

            if (_latMin[^1] > _latMax[^1])
                (_latMax[^1], _latMin[^1]) = (_latMin[^1], _latMax[^1]);
        }
        _missionZones = ((YamlSequenceNode)node["missionZones"]).Children.Select(x =>
        {
            var zone = new MissionZone(); zone.load(x); return zone;
        }).ToList();
        if (node["missionWeights"] is YamlNode weights)
	    {
		    _missionWeights.load(weights);
	    }
	    _regionWeight = uint.Parse(node["regionWeight"].ToString());
        _missionRegion = node["missionRegion"].ToString();
    }

    /**
     * Gets a list of all the missionZones in the region.
     * @return A list of missionZones.
     */
    internal List<MissionZone> getMissionZones() =>
	    _missionZones;

    /**
     * Checks if a point is inside this region.
     * @param lon Longitude in radians.
     * @param lat Latitude in radians.
     * @return True if it's inside, false if it's outside.
     */
    internal bool insideRegion(double lon, double lat)
    {
	    for (int i = 0; i < _lonMin.Count; ++i)
	    {
		    bool inLon, inLat;

		    if (_lonMin[i] <= _lonMax[i])
			    inLon = (lon >= _lonMin[i] && lon < _lonMax[i]);
		    else
			    inLon = ((lon >= _lonMin[i] && lon < M_PI*2.0) || (lon >= 0 && lon < _lonMax[i]));

		    inLat = (lat >= _latMin[i] && lat < _latMax[i]);

		    if (inLon && inLat)
			    return true;
	    }
	    return false;
    }

    /**
     * Gets a random point that is guaranteed to be inside the given zone.
     * @param zone The target zone.
     * @return A pair of longitude and latitude.
     */
    internal KeyValuePair<double, double> getRandomPoint(uint z)
    {
        var zone = (int)z;
        if (zone < _missionZones.Count)
	    {
		    int a = RNG.generate(0, _missionZones[zone].areas.Count - 1);
		    double lonMin = _missionZones[zone].areas[a].lonMin;
		    double lonMax = _missionZones[zone].areas[a].lonMax;
		    double latMin = _missionZones[zone].areas[a].latMin;
		    double latMax = _missionZones[zone].areas[a].latMax;
		    if (lonMin > lonMax)
		    {
			    lonMin = _missionZones[zone].areas[a].lonMax;
			    lonMax = _missionZones[zone].areas[a].lonMin;
		    }
		    if (latMin > latMax)
		    {
			    latMin = _missionZones[zone].areas[a].latMax;
			    latMax = _missionZones[zone].areas[a].latMin;
		    }
		    double lon = RNG.generate(lonMin, lonMax);
		    double lat = RNG.generate(latMin, latMax);
		    return KeyValuePair.Create(lon, lat);
	    }
	    Debug.Assert(false, "Invalid zone number");
	    return KeyValuePair.Create(0.0, 0.0);
    }

    /// Gets the substitute mission region.
    internal string getMissionRegion() =>
        _missionRegion;

    /**
     * Gets the weight of this region for mission selection.
     * This is only used when creating a new game, since these weights change in the course of the game.
     * @return The initial weight of this region.
     */
    internal uint getWeight() =>
	    _regionWeight;

    /**
     * Gets the cost of building a base inside this region.
     * @return The construction cost.
     */
    internal int getBaseCost() =>
	    _cost;

	/// Gets the maximum longitude.
	internal List<double> getLonMax() =>
        _lonMax;

	/// Gets the minimum longitude.
	internal List<double> getLonMin() =>
        _lonMin;

	/// Gets the maximum latitude.
	internal List<double> getLatMax() =>
        _latMax;

	/// Gets the minimum latitude.
	internal List<double> getLatMin() =>
        _latMin;

    /**
     * Gets the list of cities contained in this region.
     * @return Pointer to a list.
     */
    internal List<City> getCities()
    {
	    // Build a cached list of all mission zones that are cities
	    // Saves us from constantly searching for them
	    if (!_cities.Any())
	    {
		    foreach (var i in _missionZones)
		    {
			    foreach (var j in i.areas)
			    {
				    if (j.isPoint() && !string.IsNullOrEmpty(j.name))
				    {
					    _cities.Add(new City(j.name, j.lonMin, j.latMin));
				    }
			    }
		    }
	    }
	    return _cities;
    }
}
