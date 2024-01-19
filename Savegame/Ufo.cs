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

enum UfoStatus { FLYING, LANDED, CRASHED, DESTROYED };

/**
 * Represents an alien UFO on the map.
 * Contains variable info about a UFO like
 * position, damage, speed, etc.
 * @sa RuleUfo
 */
internal class Ufo : MovingTarget
{
    internal static string[] ALTITUDE_STRING = {
        "STR_GROUND",
        "STR_VERY_LOW",
        "STR_LOW_UC",
        "STR_HIGH_UC",
        "STR_VERY_HIGH"
    };

    RuleUfo _rules;
    int _crashId, _landId, _damage;
    string _direction, _altitude;
    UfoStatus _status;
    uint _secondsRemaining;
    bool _inBattlescape;
    AlienMission _mission;
    UfoTrajectory _trajectory;
    uint _trajectoryPoint;
    bool _detected, _hyperDetected, _processedIntercept;
    int _shootingAt, _hitFrame, _fireCountdown, _escapeCountdown;
	KeyValuePair<string, int> _shotDownByCraftId;

    /**
     * Initializes a UFO of the specified type.
     * @param rules Pointer to ruleset.
     */
    internal Ufo(RuleUfo rules) : base()
    {
        _rules = rules;
        _crashId = 0;
        _landId = 0;
        _damage = 0;
        _direction = "STR_NORTH";
        _altitude = "STR_HIGH_UC";
        _status = UfoStatus.FLYING;
        _secondsRemaining = 0;
        _inBattlescape = false;
        _mission = null;
        _trajectory = null;
        _trajectoryPoint = 0;
        _detected = false;
        _hyperDetected = false;
        _processedIntercept = false;
        _shootingAt = 0;
        _hitFrame = 0;
        _fireCountdown = 0;
        _escapeCountdown = 0;
    }

    /**
     * Make sure our mission forget's us, and we only delete targets we own (waypoints).
     *
     */
    ~Ufo()
    {
        if (_mission != null)
        {
            _mission.decreaseLiveUfos();
        }
        if (_dest is Waypoint)
        {
            _dest = null;
        }
    }

    /**
     * Saves the UFO to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save(bool newBattle)
    {
        var node = (YamlMappingNode)base.save();
        node.Add("type", _rules.getType());
	    if (_crashId != 0)
	    {
		    node.Add("crashId", _crashId.ToString());
	    }
	    else if (_landId != 0)
	    {
		    node.Add("landId", _landId.ToString());
	    }
	    node.Add("damage", _damage.ToString());
	    node.Add("altitude", _altitude);
	    node.Add("direction", _direction);
	    node.Add("status", ((int)_status).ToString());
        if (_detected)
            node.Add("detected", _detected.ToString());
	    if (_hyperDetected)
		    node.Add("hyperDetected", _hyperDetected.ToString());
	    if (_secondsRemaining != 0)
		    node.Add("secondsRemaining", _secondsRemaining.ToString());
	    if (_inBattlescape)
		    node.Add("inBattlescape", _inBattlescape.ToString());
	    if (!newBattle)
	    {
            node.Add("mission", _mission.getId().ToString());
		    node.Add("trajectory", _trajectory.getID());
		    node.Add("trajectoryPoint", _trajectoryPoint.ToString());
	    }

	    node.Add("fireCountdown", _fireCountdown.ToString());
	    node.Add("escapeCountdown", _escapeCountdown.ToString());
        return node;
    }

    /**
     * Returns the globe marker for the UFO.
     * @return Marker sprite, -1 if none.
     */
    internal override int getMarker()
    {
        if (!_detected)
            return -1;
	    switch (_status)
	    {
	        case UfoStatus.LANDED:
		        return _rules.getLandMarker() == -1 ? 3 : _rules.getLandMarker();
	        case UfoStatus.CRASHED:
		        return _rules.getCrashMarker() == -1 ? 4 : _rules.getCrashMarker();
	        default:
		        return _rules.getMarker() == -1 ? 2 : _rules.getMarker();
	    }
    }

    /**
     * Sets the UFO's crash site ID.
     * @param id the UFO's crash site ID.
     */
    internal void setCrashId(int id) =>
        _crashId = id;

    /**
     * Sets the UFO's landing site ID.
     * @param id landing site ID.
     */
    internal void setLandId(int id) =>
        _landId = id;

    /**
     * Changes the amount of remaining seconds the UFO has left on the ground.
     * After this many seconds thet UFO will take off, if landed, or disappear, if
     * crashed.
     * @param seconds Amount of seconds.
     */
    internal void setSecondsRemaining(uint seconds) =>
        _secondsRemaining = seconds;

    /**
     * Changes whether this UFO has been detected by radars.
     * @param detected Detection status.
     */
    internal void setDetected(bool detected) =>
        _detected = detected;

    /**
     * Changes the ruleset for the UFO's type.
     * @param rules Pointer to ruleset.
     * @warning ONLY FOR NEW BATTLE USE!
     */
    internal void changeRules(RuleUfo rules) =>
	    _rules = rules;

    /// Gets the UFO status
    internal UfoStatus getStatus() =>
        _status;

    /**
     * Returns the amount of remaining seconds the UFO has left on the ground.
     * After this many seconds thet UFO will take off, if landed, or disappear, if
     * crashed.
     * @return Amount of seconds.
     */
    internal uint getSecondsRemaining() =>
	    _secondsRemaining;

    /**
     * Loads the UFO from a YAML file.
     * @param node YAML node.
     * @param mod The game mod. Use to access the trajectory rules.
     * @param game The game data. Used to find the UFO's mission.
     */
    internal void load(YamlNode node, Mod.Mod mod, SavedGame game)
    {
	    base.load(node);
	    _crashId = int.Parse(node["crashId"].ToString());
	    _landId = int.Parse(node["landId"].ToString());
	    _damage = int.Parse(node["damage"].ToString());
	    _altitude = node["altitude"].ToString();
	    _direction = node["direction"].ToString();
	    _detected = bool.Parse(node["detected"].ToString());
	    _hyperDetected = bool.Parse(node["hyperDetected"].ToString());
	    _secondsRemaining = uint.Parse(node["secondsRemaining"].ToString());
	    _inBattlescape = bool.Parse(node["inBattlescape"].ToString());
	    double lon = _lon;
	    double lat = _lat;
        YamlNode dest = node["dest"];
        if (dest != null)
	    {
		    lon = double.Parse(dest["lon"].ToString());
		    lat = double.Parse(dest["lat"].ToString());
	    }
	    _dest = new Waypoint();
	    _dest.setLongitude(lon);
	    _dest.setLatitude(lat);
        YamlNode status = node["status"];
        if (status != null)
	    {
		    _status = (UfoStatus)int.Parse(status.ToString());
	    }
	    else
	    {
		    if (isDestroyed())
		    {
			    _status = UfoStatus.DESTROYED;
		    }
		    else if (isCrashed())
		    {
			    _status = UfoStatus.CRASHED;
		    }
		    else if (_altitude == "STR_GROUND")
		    {
			    _status = UfoStatus.LANDED;
		    }
		    else
		    {
			    _status = UfoStatus.FLYING;
		    }
	    }
	    if (game.getMonthsPassed() != -1)
	    {
		    int missionID = int.Parse(node["mission"].ToString());
		    var found = game.getAlienMissions().Find(x => x.getId() == missionID);
		    if (found == null)
		    {
			    // Corrupt save file.
			    throw new Exception("Unknown UFO mission, save file is corrupt.");
		    }
		    _mission = found;

		    string tid = node["trajectory"].ToString();
		    _trajectory = mod.getUfoTrajectory(tid);
		    if (_trajectory == null)
		    {
			    // Corrupt save file.
			    throw new Exception("Unknown UFO trajectory, save file is corrupt.");
		    }
		    _trajectoryPoint = uint.Parse(node["trajectoryPoint"].ToString());
	    }
	    _fireCountdown = int.Parse(node["fireCountdown"].ToString());
	    _escapeCountdown = int.Parse(node["escapeCountdown"].ToString());
	    if (_inBattlescape)
		    setSpeed(0);
    }

    /**
     * Returns if this UFO took enough damage
     * to cause it to crash.
     * @return Crashed status.
     */
    internal bool isDestroyed() =>
	    (_damage >= _rules.getMaxDamage());

    /**
     * Returns if this UFO took enough damage
     * to cause it to crash.
     * @return Crashed status.
     */
    internal bool isCrashed() =>
	    (_damage > _rules.getMaxDamage() / 2);

    /**
     * Returns whether this UFO has been detected by hyper-wave.
     * @return Detection status.
     */
    internal bool getHyperDetected() =>
	    _hyperDetected;

    /**
     * Changes whether this UFO has been detected by hyper-wave.
     * @param hyperdetected Detection status.
     */
    internal void setHyperDetected(bool hyperdetected) =>
        _hyperDetected = hyperdetected;

    /**
     * Returns the ruleset for the UFO's type.
     * @return Pointer to ruleset.
     */
    internal RuleUfo getRules() =>
	    _rules;

    /**
     * Returns whether this UFO has been detected by radars.
     * @return Detection status.
     */
    internal bool getDetected() =>
	    _detected;

    /**
     * Returns a UFO's visibility to radar detection.
     * The UFO's size and altitude affect the chances
     * of it being detected by radars.
     * @return Visibility modifier.
     */
    internal int getVisibility()
    {
	    int size = 0;
	    // size = 15*(3-ufosize);
	    if (_rules.getSize() == "STR_VERY_SMALL")
		    size = -30;
	    else if (_rules.getSize() == "STR_SMALL")
		    size = -15;
	    else if (_rules.getSize() == "STR_MEDIUM_UC")
		    size = 0;
	    else if (_rules.getSize() == "STR_LARGE")
		    size = 15;
	    else if (_rules.getSize() == "STR_VERY_LARGE")
		    size = 30;

	    int visibility = 0;
	    if (_altitude == "STR_GROUND")
		    visibility = -30;
	    else if (_altitude == "STR_VERY_LOW")
		    visibility = size - 20;
	    else if (_altitude == "STR_LOW_UC")
		    visibility = size - 10;
	    else if (_altitude == "STR_HIGH_UC")
		    visibility = size;
	    else if (_altitude == "STR_VERY_HIGH")
		    visibility = size - 10;

	    return visibility;
    }

    /// Set the UFO's status.
    internal void setStatus(UfoStatus status) =>
        _status = status;

    /// Gets the UFO's progress on the trajectory track.
    internal uint getTrajectoryPoint() =>
        _trajectoryPoint;

    /// Gets the UFO's trajectory.
    internal UfoTrajectory getTrajectory() =>
        _trajectory;

    /// Gets the UFO's mission object.
    internal AlienMission getMission() =>
        _mission;

    /**
     * Returns the current altitude of the UFO.
     * @return Altitude as integer (0-4).
     */
    internal int getAltitudeInt()
    {
	    for (int i = 0; i < 5; ++i)
	    {
		    if (ALTITUDE_STRING[i] == _altitude)
		    {
			    return i;
		    }
	    }
	    return -1;
    }

    /**
     * Changes the current altitude of the UFO.
     * @param altitude Altitude.
     */
    internal void setAltitude(string altitude)
    {
	    _altitude = altitude;
	    if (_altitude != "STR_GROUND")
	    {
		    _status = UfoStatus.FLYING;
	    }
	    else
	    {
		    _status = isCrashed() ? UfoStatus.CRASHED : UfoStatus.LANDED;
	    }
    }

    /// Sets the UFO's progress on the trajectory track.
    internal void setTrajectoryPoint(uint np) =>
        _trajectoryPoint = np;

    /**
     * Returns the current altitude of the UFO.
     * @return Altitude as string ID.
     */
    internal string getAltitude() =>
	    _altitude;

    /**
     * Gets the UFO's landing site ID.
     * @return landing site ID.
     */
    internal int getLandId() =>
	    _landId;

    /**
     * Returns the alien race currently residing in the UFO.
     * @return Alien race.
     */
    internal string getAlienRace() =>
	    _mission.getRace();

    /**
     * Sets the mission information of the UFO.
     * The UFO will start at the first point of the trajectory. The actual UFO
     * information is not changed here, this only sets the information kept on
     * behalf of the mission.
     * @param mission Pointer to the actual mission object.
     * @param trajectory Pointer to the actual mission trajectory.
     */
    internal void setMissionInfo(AlienMission mission, UfoTrajectory trajectory)
    {
	    Debug.Assert(_mission == null && mission != null && trajectory != null);
	    _mission = mission;
	    _mission.increaseLiveUfos();
	    _trajectoryPoint = 0;
	    _trajectory = trajectory;
    }

    /**
     * Moves the UFO to its destination.
     */
    internal void think()
    {
        switch (_status)
        {
            case UfoStatus.FLYING:
                move();
                if (reachedDestination())
                {
                    // Prevent further movement.
                    setSpeed(0);
                }
                break;
            case UfoStatus.LANDED:
                Debug.Assert(_secondsRemaining >= 5, "Wrong time management.");
                _secondsRemaining -= 5;
                break;
            case UfoStatus.CRASHED:
                if (!_detected)
                {
                    _detected = true;
                }
                goto case UfoStatus.DESTROYED;
                // This gets handled in GeoscapeState::time30Minutes()
                // Because the original game processes it every 30 minutes!
            case UfoStatus.DESTROYED:
                // Do nothing
                break;
        }
    }

    /**
     * Gets the UFO's battlescape status.
     * @return Is the UFO currently in battle?
     */
    internal bool isInBattlescape() =>
	    _inBattlescape;

    /**
     * Sets the UFO's battlescape status.
     * @param inbattle True if it's in battle, False otherwise.
     */
    internal void setInBattlescape(bool inbattle)
    {
        if (inbattle)
            setSpeed(0);
        _inBattlescape = inbattle;
    }

    /**
     * Gets the escape timer for dogfights.
     * @return how many ticks until the ship tries to leave.
     */
    internal int getEscapeCountdown() =>
	    _escapeCountdown;

    /**
     * Sets the number of ticks until the ufo fires its weapon.
     * @param time number of ticks until refire.
     */
    internal void setFireCountdown(int time) =>
        _fireCountdown = time;

    /**
     * Sets the countdown timer for escaping a dogfight.
     * @param time how many ticks until the ship attempts to escape.
     */
    internal void setEscapeCountdown(int time) =>
        _escapeCountdown = time;

    /**
     * Sets a flag denoting that this ufo has had its timers decremented.
     * prevents multiple interceptions from decrementing or resetting an already running timer.
     * this flag is reset in advance each time the geoscape processes the dogfights.
     * @param processed whether or not we've had our timers processed.
     */
    internal void setInterceptionProcessed(bool processed) =>
        _processedIntercept = processed;

    /**
     * Returns the current direction the UFO is heading in.
     * @return Direction.
     */
    internal string getDirection() =>
	    _direction;

    /**
     * Returns the Mission type of the UFO.
     * @return Mission.
     */
    internal string getMissionType() =>
	    _mission.getRules().getType();

    /**
     * Returns the amount of damage this UFO has taken.
     * @return Amount of damage.
     */
    internal int getDamage() =>
	    _damage;

    /**
     * Changes the amount of damage this UFO has taken.
     * @param damage Amount of damage.
     */
    internal void setDamage(int damage)
    {
        _damage = damage;
        if (_damage < 0)
        {
            _damage = 0;
        }
        if (isDestroyed())
        {
            _status = UfoStatus.DESTROYED;
        }
        else if (isCrashed())
        {
            _status = UfoStatus.CRASHED;
        }
    }

    /**
     * Gets which interception window the UFO is active in.
     * @return which interception window the UFO is active in.
     */
    internal int getShootingAt() =>
	    _shootingAt;

    /**
     * Sets which interception window the UFO is active in.
     * @param target the window the UFO is active in.
     */
    internal void setShootingAt(int target) =>
	    _shootingAt = target;

    /**
     * Gets the number of ticks until the ufo is ready to fire.
     * @return ticks until weapon is ready.
     */
    internal int getFireCountdown() =>
	    _fireCountdown;

    internal KeyValuePair<string, int> getShotDownByCraftId() =>
	    _shotDownByCraftId;

    /**
     * Gets the UFO's hit frame.
     * @return the hit frame.
     */
    internal int getHitFrame() =>
	    _hitFrame;

    /**
     * Sets the UFO's hit frame.
     * @param frame the hit frame.
     */
    internal void setHitFrame(int frame) =>
	    _hitFrame = frame;

    /**
     * Gets the UFO's crash site ID.
     * @return the UFO's crash site ID.
     */
    internal int getCrashId() =>
	    _crashId;

    /**
     * Gets if the ufo has had its timers decremented on this cycle of interception updates.
     * @return if this ufo has already been processed.
     */
    internal bool getInterceptionProcessed() =>
	    _processedIntercept;

    internal void setShotDownByCraftId(KeyValuePair<string, int> craft) =>
	    _shotDownByCraftId = craft;

    /**
     * Returns the UFO's unique type used for
     * savegame purposes.
     * @return ID.
     */
    protected override string getType() =>
	    "STR_UFO";

    /**
     * Returns the UFO's unique default name.
     * @param lang Language to get strings from.
     * @return Full name.
     */
    internal override string getDefaultName(Language lang)
    {
	    switch (_status)
	    {
	        case UfoStatus.LANDED:
		        return lang.getString(getMarkerName()).arg(_landId);
	        case UfoStatus.CRASHED:
		        return lang.getString(getMarkerName()).arg(_crashId);
	        default:
		        return lang.getString(getMarkerName()).arg(_id);
	    }
    }

    /**
     * Returns the name on the globe for the UFO.
     * @return String ID.
     */
    internal override string getMarkerName()
    {
	    switch (_status)
	    {
	        case UfoStatus.LANDED:
		        return "STR_LANDING_SITE_";
	        case UfoStatus.CRASHED:
		        return "STR_CRASH_SITE_";
	        default:
		        return "STR_UFO_";
	    }
    }

    /**
     * Returns the marker ID on the globe for the UFO.
     * @return Marker ID.
     */
    internal override int getMarkerId()
    {
	    switch (_status)
	    {
	        case UfoStatus.LANDED:
		        return _landId;
	        case UfoStatus.CRASHED:
		        return _crashId;
	        default:
		        return _id;
	    }
    }
}
