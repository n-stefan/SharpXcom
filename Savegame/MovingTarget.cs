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
 * Base class for moving targets on the globe
 * with a certain speed and destination.
 */
internal class MovingTarget : Target
{
    protected Target _dest;
    protected double _speedLon, _speedLat, _speedRadian;
    protected double _meetPointLon, _meetPointLat;
    protected int _speed;
    protected bool _meetCalculated;

    /**
     * Initializes a moving target with blank coordinates.
     */
    internal MovingTarget() : base()
    {
        _dest = null;
        _speedLon = 0.0;
        _speedLat = 0.0;
        _speedRadian = 0.0;
        _meetPointLon = 0.0;
        _meetPointLat = 0.0;
        _speed = 0;
        _meetCalculated = false;
    }

    /**
     * Make sure to cleanup the target's destination followers.
     */
    ~MovingTarget() =>
        setDestination(null);

    /**
     * Changes the speed of the moving target
     * and converts it from standard knots (nautical miles per hour)
     * into radians per 5 in-game seconds.
     * @param speed Speed in knots.
     */
    internal void setSpeed(int speed)
    {
        _speed = speed;
        _speedRadian = calculateRadianSpeed(_speed);
        // Recalculate meeting point for any followers
        foreach (var follower in getFollowers())
        {
            follower.resetMeetPoint();
        }
        calculateSpeed();
    }

    /**
     * Calculates the speed vector based on the
     * great circle distance to destination and
     * current raw speed.
     */
    void calculateSpeed()
    {
        calculateMeetPoint();
        if (_dest != null)
        {
            double dLon, dLat, length;
            dLon = Math.Sin(_meetPointLon - _lon) * Math.Cos(_meetPointLat);
            dLat = Math.Cos(_lat) * Math.Sin(_meetPointLat) - Math.Sin(_lat) * Math.Cos(_meetPointLat) * Math.Cos(_meetPointLon - _lon);
            length = Math.Sqrt(dLon * dLon + dLat * dLat);
            _speedLat = dLat / length * _speedRadian;
            _speedLon = dLon / length * _speedRadian / Math.Cos(_lat + _speedLat);

            // Check for invalid speeds when a division by zero occurs due to near-zero values
            if (!(_speedLon == _speedLon) || !(_speedLat == _speedLat))
            {
                _speedLon = 0;
                _speedLat = 0;
            }
        }
        else
        {
            _speedLon = 0;
            _speedLat = 0;
        }
    }

    /**
     * Converts a speed in degrees to a speed in radians.
     * Each nautical mile is 1/60th of a degree.
     * Each hour contains 720 5-seconds.
     * @param speed Speed in degrees.
     * @return Speed in radians.
     */
    internal double calculateRadianSpeed(int speed) =>
        Nautical(speed) / 720.0;

    /**
     * Changes the destination the moving target is heading to.
     * @param dest Pointer to destination.
     */
    internal void setDestination(Target dest)
    {
        _meetCalculated = false;
        // Remove moving target from old destination's followers
        if (_dest != null)
        {
            _dest.getFollowers().Remove(this);
        }
        _dest = dest;
        // Add moving target to new destination's followers
        if (_dest != null)
        {
            _dest.getFollowers().Add(this);
        }
        // Recalculate meeting point for any followers
        foreach (var follower in getFollowers())
        {
            follower.resetMeetPoint();
        }
        calculateSpeed();
    }

    /**
     * Forces the meeting point to be recalculated in the event
     * that the target has changed direction.
     */
    protected void resetMeetPoint() =>
        _meetCalculated = false;

    /**
     * Returns the destination the moving target is heading to.
     * @return Pointer to destination.
     */
    internal Target getDestination() =>
	    _dest;

    /**
     * Returns the radial speed of the moving target.
     * @return Speed in 1 / 5 sec.
     */
    double getSpeedRadian() =>
	    _speedRadian;

    /**
     * Checks if the moving target has reached its destination.
     * @return True if it has, False otherwise.
     */
    internal bool reachedDestination()
    {
	    if (_dest == null)
	    {
            return false;
	    }
	    return ( AreSame(_dest.getLongitude(), _lon) && AreSame(_dest.getLatitude(), _lat) );
    }

    /**
     * Calculate meeting point with the target.
     */
    void calculateMeetPoint()
    {
        if (!Options.meetingPoint) _meetCalculated = false;
        if (_meetCalculated) return;

        // Initialize
        if (_dest != null)
        {
            _meetPointLat = _dest.getLatitude();
            _meetPointLon = _dest.getLongitude();
        }
        else
        {
            _meetPointLat = _lat;
            _meetPointLon = _lon;
        }

        if (_dest == null || !Options.meetingPoint || reachedDestination()) return;

        var t = (MovingTarget)_dest;
        if (t == null || t.getDestination() == null) return;

        // Speed ratio
        if (AreSame(t.getSpeedRadian(), 0.0)) return;
        double speedRatio = _speedRadian / t.getSpeedRadian();

        // The direction pseudovector
        double nx = Math.Cos(t.getLatitude()) * Math.Sin(t.getLongitude()) * Math.Sin(t.getDestination().getLatitude()) -
                        Math.Sin(t.getLatitude()) * Math.Cos(t.getDestination().getLatitude()) * Math.Sin(t.getDestination().getLongitude()),
                ny = Math.Sin(t.getLatitude()) * Math.Cos(t.getDestination().getLatitude()) * Math.Cos(t.getDestination().getLongitude()) -
                        Math.Cos(t.getLatitude()) * Math.Cos(t.getLongitude()) * Math.Sin(t.getDestination().getLatitude()),
                nz = Math.Cos(t.getLatitude()) * Math.Cos(t.getDestination().getLatitude()) * Math.Sin(t.getDestination().getLongitude() - t.getLongitude());
        // Normalize and multiplex with radian speed
        double nk = _speedRadian / Math.Sqrt(nx * nx + ny * ny + nz * nz);
        nx *= nk;
        ny *= nk;
        nz *= nk;

        // Finding the meeting point. Don't search further than halfway across the
        // globe (distance from interceptor's current point >= 1), as that may
        // cause the interceptor to go the wrong way later.
        for (double path = 0, distance = 1;
            path < M_PI && distance - path * speedRatio > 0 && path * speedRatio < 1;
            path += _speedRadian)
        {
            _meetPointLat += nx * Math.Sin(_meetPointLon) - ny * Math.Cos(_meetPointLon);
            if (Math.Abs(_meetPointLat) < M_PI_2) _meetPointLon += nz - (nx * Math.Cos(_meetPointLon) + ny * Math.Sin(_meetPointLon)) * Math.Tan(_meetPointLat); else _meetPointLon += M_PI;

            distance = Math.Acos(Math.Cos(_lat) * Math.Cos(_meetPointLat) * Math.Cos(_meetPointLon - _lon) + Math.Sin(_lat) * Math.Sin(_meetPointLat));
        }

        // Correction overflowing angles
        double lonSign = Math.Sign(_meetPointLon);
        double latSign = Math.Sign(_meetPointLat);
        while (Math.Abs(_meetPointLon) > M_PI) _meetPointLon -= lonSign * 2 * M_PI;
        while (Math.Abs(_meetPointLat) > M_PI) _meetPointLat -= latSign * 2 * M_PI;
        if (Math.Abs(_meetPointLat) > M_PI_2) { _meetPointLat = latSign * Math.Abs(2 * M_PI - Math.Abs(_meetPointLat)); _meetPointLon -= lonSign * M_PI; }

        _meetCalculated = true;
    }

    /**
     * Saves the moving target to a YAML file.
     * @return YAML node.
     */
    protected virtual YamlNode save()
    {
	    var node = (YamlMappingNode)base.save();
	    if (_dest != null)
	    {
		    node.Add("dest", _dest.saveId());
	    }
	    node.Add("speedLon", _speedLon.ToString());
	    node.Add("speedLat", _speedLat.ToString());
	    node.Add("speedRadian", _speedRadian.ToString());
	    node.Add("speed", _speed.ToString());
	    return node;
    }

    /**
     * Returns the speed of the moving target.
     * @return Speed in knots.
     */
    internal int getSpeed() =>
	    _speed;

    /**
     * Loads the moving target from a YAML file.
     * @param node YAML node.
     */
    protected void load(YamlNode node)
    {
	    base.load(node);
	    _speedLon = double.Parse(node["speedLon"].ToString());
	    _speedLat = double.Parse(node["speedLat"].ToString());
	    _speedRadian = double.Parse(node["speedRadian"].ToString());
	    _speed = int.Parse(node["speed"].ToString());
    }

    /**
     * Executes a movement cycle for the moving target.
     */
    internal void move()
    {
        calculateSpeed();
        if (_dest != null)
        {
            if (getDistance(_meetPointLon, _meetPointLat) > _speedRadian)
            {
                setLongitude(_lon + _speedLon);
                setLatitude(_lat + _speedLat);
            }
            else
            {
                if (getDistance(_dest) > _speedRadian)
                {
                    setLongitude(_meetPointLon);
                    setLatitude(_meetPointLat);
                }
                else
                {
                    setLongitude(_dest.getLongitude());
                    setLatitude(_dest.getLatitude());
                }
                resetMeetPoint();
            }
        }
    }

    internal bool isMeetCalculated() =>
	    _meetCalculated;
}
