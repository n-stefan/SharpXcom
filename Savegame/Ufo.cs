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
    bool isDestroyed() =>
	    (_damage >= _rules.getMaxDamage());

    /**
     * Returns if this UFO took enough damage
     * to cause it to crash.
     * @return Crashed status.
     */
    bool isCrashed() =>
	    (_damage > _rules.getMaxDamage() / 2);
}
