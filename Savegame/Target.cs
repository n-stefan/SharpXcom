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
    List<Craft> getCraftFollowers()
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
    internal virtual YamlNode save()
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
}
