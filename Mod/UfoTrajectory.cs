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
 * Information for points on a UFO trajectory.
 */
struct TrajectoryWaypoint
{
    /// The mission zone.
    internal uint zone;
    /// The altitude to reach.
    internal uint altitude;
    /// The speed percentage ([0..100])
    internal uint speed;

    /**
	 * Loads the TrajectoryWaypoint from a YAML file.
	 * @param node YAML node.
	 */
    internal void load(YamlNode node)
    {
        zone = uint.Parse(node["zone"].ToString());
        altitude = uint.Parse(node["altitude"].ToString());
        speed = uint.Parse(node["speed"].ToString());
    }
};

/**
 * Holds information about a specific trajectory.
 * Ufo trajectories are a sequence of mission zones, altitudes and speed percentages.
 */
internal class UfoTrajectory : IRule
{
    internal const string RETALIATION_ASSAULT_RUN = "__RETALIATION_ASSAULT_RUN";

    string _id;
    uint _groundTimer;
    List<TrajectoryWaypoint> _waypoints;

    internal UfoTrajectory() { }

    UfoTrajectory(string id)
    {
        _id = id;
        _groundTimer = 5;
    }

    public IRule Create(string type) =>
        new UfoTrajectory(type);

    /**
	 * Gets the trajectory's ID.
	 * @return The trajectory's ID.
	 */
    internal string getID() =>
        _id;

    /**
     * Overwrites trajectory data with the data stored in @a node.
     * Only the fields contained in the node will be overwritten.
     * @param node The node containing the new values.
     */
    internal void load(YamlNode node)
    {
	    _id = node["id"].ToString();
	    _groundTimer = uint.Parse(node["groundTimer"].ToString());
        _waypoints = ((YamlSequenceNode)node["waypoints"]).Children.Select(x =>
        {
            var waypoint = new TrajectoryWaypoint(); waypoint.load(x); return waypoint;
        }).ToList();
    }

    /**
	 * Gets the zone index at a waypoint.
	 * @param wp The waypoint.
	 * @return The zone index.
	 */
    internal uint getZone(uint wp) =>
        _waypoints[(int)wp].zone;

    /**
	 * Gets the number of waypoints in this trajectory.
	 * @return The number of waypoints.
	 */
    internal uint getWaypointCount() =>
        (uint)_waypoints.Count;

    /**
     * Gets the altitude at a waypoint.
     * @param wp The waypoint.
     * @return The altitude.
     */
    internal string getAltitude(uint wp) =>
	    Ufo.ALTITUDE_STRING[_waypoints[(int)wp].altitude];

    /**
	 * Gets the speed percentage at a waypoint.
	 * @param wp The waypoint.
	 * @return The speed as a percentage.
	 */
    internal float getSpeedPercentage(uint wp) =>
        _waypoints[(int)wp].speed / 100.0f;

    /**
	 * Gets the number of seconds UFOs should spend on the ground.
	 * @return The number of seconds.
	 */
    internal uint groundTimer() =>
        _groundTimer;
}
