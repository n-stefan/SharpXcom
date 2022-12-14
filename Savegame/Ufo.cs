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
    Ufo(RuleUfo rules) : base()
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
}
