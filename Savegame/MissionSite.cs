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
    internal MissionSite(RuleAlienMission rules, AlienDeployment deployment) : base()
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
    protected override int getMarker()
    {
	    if (!_detected)
		    return -1;
	    if (_deployment.getMarkerIcon() == -1)
		    return 5;
        return _deployment.getMarkerIcon();
    }

    /**
     * Sets the mission site's detection state.
     * @param detected whether we want this site to show on the geoscape or not.
     */
    internal void setDetected(bool detected) =>
        _detected = detected;

    /**
     * Changes the number of seconds before the mission site expires.
     * @param seconds Amount of seconds.
     */
    internal void setSecondsRemaining(uint seconds) =>
        _secondsRemaining = seconds;

    /**
     * Changes the alien race currently residing in the mission site.
     * @param race Alien race.
     */
    internal void setAlienRace(string race) =>
	    _race = race;

    /**
     * Returns the ruleset for the mission's deployment.
     * @return Pointer to deployment rules.
     */
    internal AlienDeployment getDeployment() =>
	    _deployment;

    /**
     * Gets the detection state for this mission site.
     * used for popups of sites spawned directly rather than by UFOs.
     * @return whether or not this site has been detected.
     */
    internal bool getDetected() =>
	    _detected;

    /**
     * Returns the number of seconds remaining before the mission site expires.
     * @return Amount of seconds.
     */
    internal uint getSecondsRemaining() =>
	    _secondsRemaining;

    /**
     * Gets the mission site's associated texture.
     * @return Texture ID.
     */
    internal int getTexture() =>
	    _texture;

    /**
     * Sets the mission site's associated texture.
     * @param texture Texture ID.
     */
    internal void setTexture(int texture) =>
        _texture = texture;

    /**
     * Sets the mission site's associated city, if any.
     * @param city String ID for the city, "" if none.
     */
    internal void setCity(string city) =>
	    _city = city;

    /**
     * Gets the mission site's battlescape status.
     * @return Is the mission currently in battle?
     */
    internal bool isInBattlescape() =>
	    _inBattlescape;

    /**
     * Returns the alien race currently residing in the mission site.
     * @return Alien race.
     */
    internal string getAlienRace() =>
	    _race;

    /**
     * Sets the mission site's battlescape status.
     * @param inbattle True if it's in battle, False otherwise.
     */
    internal void setInBattlescape(bool inbattle) =>
        _inBattlescape = inbattle;

    /**
     * Gets the mission site's associated city, if any.
     * @return String ID for the city, "" if none.
     */
    internal string getCity() =>
	    _city;
}
