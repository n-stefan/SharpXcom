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

namespace SharpXcom.Battlescape;

/**
 * State for walking units.
 */
internal class UnitWalkBState : BattleState
{
	BattleUnit _unit;
	Pathfinding _pf;
	TileEngine _terrain;
	bool _falling;
	bool _beforeFirstStep;
	uint _numUnitsSpotted;
	int _preMovementCost;
	Position _target;

    /**
     * Sets up an UnitWalkBState.
     * @param parent Pointer to the Battlescape.
     * @param action Pointer to an action.
     */
    internal UnitWalkBState(BattlescapeGame parent, BattleAction action) : base(parent, action)
    {
        _unit = null;
        _pf = null;
        _terrain = null;
        _falling = false;
        _beforeFirstStep = false;
        _numUnitsSpotted = 0;
        _preMovementCost = 0;
    }

    /**
     * Deletes the UnitWalkBState.
     */
    ~UnitWalkBState() { }

    /**
     * Initializes the state.
     */
    protected override void init()
    {
	    _unit = _action.actor;
	    _numUnitsSpotted = (uint)_unit.getUnitsSpottedThisTurn().Count;
	    setNormalWalkSpeed();
	    _pf = _parent.getPathfinding();
	    _terrain = _parent.getTileEngine();
	    _target = _action.target;
	    if (Options.traceAI) { Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Walking from: {_unit.getPosition()}, to {_target}"); }
	    int dir = _pf.getStartDirection();
	    if (!_action.strafe && dir != -1 && dir != _unit.getDirection())
	    {
		    _beforeFirstStep = true;
	    }
    }

    /**
     * Handles some calculations when the walking is finished.
     */
    void setNormalWalkSpeed()
    {
	    if (_unit.getFaction() == UnitFaction.FACTION_PLAYER)
		    _parent.setStateInterval((uint)Options.battleXcomSpeed);
	    else
		    _parent.setStateInterval((uint)Options.battleAlienSpeed);
    }
}
