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
 * Represents an alien mission site on the world.
 */
internal class MissionSite : Target
{
    RuleAlienMission _rules;
    AlienDeployment _deployment;
    int _texture;
    uint _secondsRemaining;
    bool _inBattlescape, _detected;
    string _race, _city;

    /**
     * Initializes a mission site.
     */
    MissionSite(RuleAlienMission rules, AlienDeployment deployment) : base()
    {
        _rules = rules;
        _deployment = deployment;
        _texture = -1;
        _secondsRemaining = 0;
        _inBattlescape = false;
        _detected = false;
    }

    /**
     *
     */
    ~MissionSite() { }

    /**
     * Saves the mission site to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = (YamlMappingNode)base.save();
	    node.Add("type", _rules.getType());
	    node.Add("deployment", _deployment.getType());
	    node.Add("texture", _texture.ToString());
	    if (_secondsRemaining != 0)
		    node.Add("secondsRemaining", _secondsRemaining.ToString());
	    node.Add("race", _race);
	    if (_inBattlescape)
		    node.Add("inBattlescape", _inBattlescape.ToString());
	    node.Add("detected", _detected.ToString());
        return node;
    }

    /**
     * Returns the globe marker for the mission site.
     * @return Marker sprite, -1 if none.
     */
    internal override int getMarker()
    {
	    if (!_detected)
		    return -1;
	    if (_deployment.getMarkerIcon() == -1)
		    return 5;
        return _deployment.getMarkerIcon();
    }
}
