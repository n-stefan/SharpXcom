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
 * Represents a specific funding country.
 * Contains constant info like its location in the
 * world and starting funding range.
 */
internal class RuleCountry : IRule
{
    string _type;
    int _fundingBase, _fundingCap;
    double _labelLon, _labelLat;
    List<double> _lonMin, _lonMax, _latMin, _latMax;

    /**
     * Creates a blank ruleset for a certain
     * type of country.
     * @param type String defining the type.
     */
    RuleCountry(string type)
    {
        _type = type;
        _fundingBase = 0;
        _fundingCap = 0;
        _labelLon = 0.0;
        _labelLat = 0.0;
    }

    public IRule Create(string type) =>
        new RuleCountry(type);

    /**
     *
     */
    ~RuleCountry() { }

    /**
     * Generates the random starting funding for the country.
     * @return The monthly funding.
     */
    internal int generateFunding() =>
	    RNG.generate(_fundingBase, _fundingBase * 2) * 1000;

    /**
     * Gets the language string that names
     * this country. Each country type
     * has a unique name.
     * @return The country's name.
     */
    internal string getType() =>
	    _type;

    /**
     * Loads the country type from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    _type = node["type"].ToString();
	    _fundingBase = int.Parse(node["fundingBase"].ToString());
	    _fundingCap = int.Parse(node["fundingCap"].ToString());
	    if (node["labelLon"] != null)
		    _labelLon = Deg2Rad(double.Parse(node["labelLon"].ToString()));
	    if (node["labelLat"] != null)
		    _labelLat = Deg2Rad(double.Parse(node["labelLat"].ToString()));
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
    }

    /**
     * Checks if a point is inside this country.
     * @param lon Longitude in radians.
     * @param lat Latitude in radians.
     * @return True if it's inside, false if it's outside.
     */
    internal bool insideCountry(double lon, double lat)
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
}
