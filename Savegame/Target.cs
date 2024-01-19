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

namespace SharpXcom.Savegame;

/**
 * Base class for targets on the globe
 * with a set of radian coordinates.
 */
internal class Target
{
    protected List<MovingTarget> _followers;
    protected double _lon, _lat;
    protected int _id;
    protected string _name;

    /**
     * Initializes a target with blank coordinates.
     */
    internal Target()
    {
        _lon = 0.0;
        _lat = 0.0;
        _id = 0;
    }

    /**
     * Make sure no crafts are chasing this target.
     */
    ~Target()
    {
        var followers = getCraftFollowers();
        foreach (var follower in followers)
        {
            follower.returnToBase();
        }
    }

    /**
     * Returns the list of crafts currently
     * following this target.
     * @return List of crafts.
     */
    internal List<Craft> getCraftFollowers()
    {
	    var crafts = new List<Craft>();
	    foreach (var follower in _followers)
	    {
		    if (follower is Craft craft)
		    {
			    crafts.Add(craft);
		    }
	    }
	    return crafts;
    }

    /**
     * Returns the longitude coordinate of the target.
     * @return Longitude in radian.
     */
    internal double getLongitude() =>
	    _lon;

    /**
     * Returns the latitude coordinate of the target.
     * @return Latitude in radian.
     */
    internal double getLatitude() =>
	    _lat;

    /**
     * Returns the list of targets currently
     * following this target.
     * @return Pointer to list of targets.
     */
    internal List<MovingTarget> getFollowers() =>
        _followers;

    /**
     * Saves the target to a YAML file.
     * @returns YAML node.
     */
    protected virtual YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "lon", _lon.ToString() },
            { "lat", _lat.ToString() }
        };
        if (_id != 0)
		    node.Add("id", _id.ToString());
	    if (!string.IsNullOrEmpty(_name))
		    node.Add("name", _name);
        return node;
    }

    /**
     * Saves the target's unique identifiers to a YAML file.
     * @return YAML node.
     */
    internal YamlMappingNode saveId()
    {
        var node = new YamlMappingNode
        {
            { "lon", _lon.ToString() },
            { "lat", _lat.ToString() },
            { "type", getType() },
            { "id", _id.ToString() }
        };
        return node;
    }

    /// Gets the target's type.
    protected virtual string getType() =>
        string.Empty;

    /// Gets the target's marker sprite.
    internal virtual int getMarker() =>
        0;

    /**
     * Changes the target's unique ID.
     * @param id Unique ID.
     */
    internal void setId(int id) =>
        _id = id;

    /**
     * Changes the longitude coordinate of the target.
     * @param lon Longitude in radian.
     */
    internal void setLongitude(double lon)
    {
        _lon = lon;

        // Keep between 0 and 2xPI
        while (_lon < 0)
            _lon += 2 * M_PI;
        while (_lon >= 2 * M_PI)
            _lon -= 2 * M_PI;
    }

    /**
     * Changes the latitude coordinate of the target.
     * @param lat Latitude in radian.
     */
    internal void setLatitude(double lat)
    {
        _lat = lat;
        // If you travel past a pole, continue on the other side of the globe.
        if (_lat < -M_PI / 2)
        {
            _lat = -M_PI - _lat;
            setLongitude(_lon + M_PI);
        }
        else if (_lat > M_PI / 2)
        {
            _lat = M_PI - _lat;
            setLongitude(_lon - M_PI);
        }
    }

    /**
     * Changes the target's custom name.
     * @param newName New custom name. If set to blank, the language default is used.
     */
    internal void setName(string newName) =>
	    _name = newName;

    /**
     * Returns the target's unique ID.
     * @return Unique ID, 0 if none.
     */
    internal int getId() =>
	    _id;

    /**
     * Loads the target from a YAML file.
     * @param node YAML node.
     */
    protected void load(YamlNode node)
    {
	    _lon = double.Parse(node["lon"].ToString());
	    _lat = double.Parse(node["lat"].ToString());
	    _id = int.Parse(node["id"].ToString());
        YamlNode name = node["name"];
        if (name != null)
	    {
		    _name = name.ToString();
	    }
    }

    /**
     * Returns the target's user-readable name.
     * If there's no custom name, the language default is used.
     * @param lang Language to get strings from.
     * @return Full name.
     */
    internal virtual string getName(Language lang)
    {
	    if (string.IsNullOrEmpty(_name))
		    return getDefaultName(lang);
	    return _name;
    }

    /**
     * Returns the target's unique default name.
     * @param lang Language to get strings from.
     * @return Full name.
     */
    internal virtual string getDefaultName(Language lang) =>
	    lang.getString(getMarkerName()).arg(_id);

    /**
     * Returns the name on the globe for the target.
     * @return String ID.
     */
    internal virtual string getMarkerName() =>
	    getType() + "_";

	/// Gets the distance to another target.
	internal double getDistance(Target target) =>
        getDistance(target.getLongitude(), target.getLatitude());

    /**
     * Returns the great circle distance to another
     * target on the globe.
     * @param lon Longitude.
     * @param lat Latitude.
     * @returns Distance in radian.
     */
    protected double getDistance(double lon, double lat)
    {
	    if (AreSame(lon, _lon) && AreSame(lat, _lat))
		    return 0.0;
	    return Math.Acos(Math.Cos(_lat) * Math.Cos(lat) * Math.Cos(lon - _lon) + Math.Sin(_lat) * Math.Sin(lat));
    }

    /**
     * Returns the marker ID on the globe for the target.
     * @return Marker ID.
     */
    internal virtual int getMarkerId() =>
	    _id;
}
