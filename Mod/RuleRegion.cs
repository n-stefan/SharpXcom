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
	double lonMin, lonMax, latMin, latMax;
	int texture;
	string name;

    public static bool operator ==(MissionArea a, MissionArea b) =>
		AreSame(a.lonMax, b.lonMax) && AreSame(a.lonMin, b.lonMin) && AreSame(a.latMax, b.latMax) && AreSame(a.latMin, b.latMin);

    public static bool operator !=(MissionArea a, MissionArea b) =>
        !(a == b);

    bool isPoint() =>
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
};

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
            areas[i].load(node["areas"][i]);
        }
    }
};

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
            var area = new List<double>();
            foreach (var j in ((YamlSequenceNode)i).Children)
            {
                area.Add(double.Parse(j.ToString()));
            }
            areas.Add(area);
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
        if (node["missionWeights"] != null)
	    {
		    _missionWeights.load(node["missionWeights"]);
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
}
