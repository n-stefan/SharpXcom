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

enum ChronoTrigger { FORCE_LOSE, FORCE_ABORT, FORCE_WIN };

enum EscapeType { ESCAPE_NONE, ESCAPE_EXIT, ESCAPE_ENTRY, ESCAPE_EITHER };

/**
 * Represents a specific type of Alien Deployment.
 * Contains constant info about a Alien Deployment like
 * the number of aliens for each alien type and what items they carry
 * (itemset depends on alien technology advancement level 0, 1 or 2).
 * - deployment type can be a craft's name, but also alien base or cydonia.
 * - alienRank is used to check which nodeRanks can be used to deploy this unit
 *   + to match to a specific unit (=race/rank combination) that should be deployed.
 * @sa Node
 */
internal class AlienDeployment : IRule
{
    string _type;
    int _width, _length, _height, _civilians;
    int _shade;
    bool _finalDestination, _isAlienBase;
    string _alert, _alertBackground;
    string _markerName, _objectivePopup, _objectiveCompleteText, _objectiveFailedText;
    int _markerIcon, _durationMin, _durationMax, _minDepth, _maxDepth, _genMissionFrequency;
    int _objectiveType, _objectivesRequired, _objectiveCompleteScore, _objectiveFailedScore, _despawnPenalty, _points, _turnLimit, _cheatTurn;
    ChronoTrigger _chronoTrigger;
    EscapeType _escapeType;

    /**
     * Creates a blank ruleset for a certain
     * type of deployment data.
     * @param type String defining the type.
     */
    AlienDeployment(string type)
    {
        _type = type;
        _width = 0;
        _length = 0;
        _height = 0;
        _civilians = 0;
        _shade = -1;
        _finalDestination = false;
        _isAlienBase = false;
        _alert = "STR_ALIENS_TERRORISE";
        _alertBackground = "BACK03.SCR";
        _markerName = "STR_TERROR_SITE";
        _markerIcon = -1;
        _durationMin = 0;
        _durationMax = 0;
        _minDepth = 0;
        _maxDepth = 0;
        _genMissionFrequency = 0;
        _objectiveType = -1;
        _objectivesRequired = 0;
        _objectiveCompleteScore = 0;
        _objectiveFailedScore = 0;
        _despawnPenalty = 0;
        _points = 0;
        _turnLimit = 0;
        _cheatTurn = 20;
        _chronoTrigger = ChronoTrigger.FORCE_LOSE;
        _escapeType = EscapeType.ESCAPE_NONE;
    }

    public IRule Create(string type) =>
        new AlienDeployment(type);

    /**
     *
     */
    ~AlienDeployment() { }

    /**
     * Returns the language string that names
     * this deployment. Each deployment type has a unique name.
     * @return Deployment name.
     */
    internal string getType() =>
	    _type;

    /**
     * Returns the globe marker icon for this mission.
     * @return Marker sprite, -1 if none.
     */
    internal int getMarkerIcon() =>
	    _markerIcon;
}
