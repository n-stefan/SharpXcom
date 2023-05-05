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
 * Represents an alien base on the world.
 */
internal class AlienBase : Target
{
    bool _inBattlescape, _discovered;
    AlienDeployment _deployment;
    string _race;

    internal AlienBase() { }

    /**
     * Initializes an alien base
     */
    internal AlienBase(AlienDeployment deployment) : base()
    {
        _inBattlescape = false;
        _discovered = false;
        _deployment = deployment;
    }

    /**
     *
     */
    ~AlienBase() { }

    /**
     * Saves the alien base to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
	    var node = (YamlMappingNode)base.save();
	    node.Add("race", _race);
	    if (_inBattlescape)
		    node.Add("inBattlescape", _inBattlescape.ToString());
        if (_discovered)
            node.Add("discovered", _discovered.ToString());
	    node.Add("deployment", _deployment.getType());
	    return node;
    }

    /**
     * Returns the globe marker for the alien base.
     * @return Marker sprite, -1 if none.
     */
    internal override int getMarker()
    {
	    if (!_discovered)
		    return -1;
	    return _deployment.getMarkerIcon();
    }

    /**
     * Changes the alien race currently residing in the alien base.
     * @param race Alien race.
     */
    internal void setAlienRace(string race) =>
	    _race = race;

    /**
     * Sets the alien base's geoscape status.
     * @param discovered Has the base been discovered?
     */
    internal void setDiscovered(bool discovered) =>
        _discovered = discovered;

    internal AlienDeployment getDeployment() =>
	    _deployment;

    /**
     * Gets the alien base's geoscape status.
     * @return Has the base been discovered?
     */
    internal bool isDiscovered() =>
	    _discovered;
}
