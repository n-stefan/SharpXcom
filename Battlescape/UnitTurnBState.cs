﻿/*
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
 * State for turning units.
 */
internal class UnitTurnBState : BattleState
{
	BattleUnit _unit;
	bool _turret, _chargeTUs;

    /**
     * Sets up an UnitTurnBState.
     * @param parent Pointer to the Battlescape.
     * @param action Pointer to an action.
     */
	internal UnitTurnBState(BattlescapeGame parent, BattleAction action, bool chargeTUs = true) : base(parent, action)
    {
        _unit = null;
        _turret = false;
        _chargeTUs = chargeTUs;
    }

    /**
     * Deletes the UnitTurnBState.
     */
    ~UnitTurnBState() { }

	/**
	 * Initializes the state.
	 */
	internal override void init()
	{
		_unit = _action.actor;
		if (_unit.isOut())
		{
			_parent.popState();
			return;
		}
		_action.TU = 0;
		if (_unit.getFaction() == UnitFaction.FACTION_PLAYER)
			_parent.setStateInterval((uint)Options.battleXcomSpeed);
		else
			_parent.setStateInterval((uint)Options.battleAlienSpeed);

		// if the unit has a turret and we are turning during targeting, then only the turret turns
		_turret = _unit.getTurretType() != -1 && (_action.targeting || _action.strafe);

		_unit.lookAt(_action.target, _turret);

		if (_chargeTUs && _unit.getStatus() != UnitStatus.STATUS_TURNING)
		{
			if (_action.type == BattleActionType.BA_NONE)
			{
				// try to open a door
				int door = _parent.getTileEngine().unitOpensDoor(_unit, true);
				if (door == 0)
				{
					_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)Mod.Mod.DOOR_OPEN).play(-1, _parent.getMap().getSoundAngle(_unit.getPosition())); // normal door
				}
				if (door == 1)
				{
					_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)Mod.Mod.SLIDING_DOOR_OPEN).play(-1, _parent.getMap().getSoundAngle(_unit.getPosition())); // ufo door
				}
				if (door == 4)
				{
					_action.result = "STR_NOT_ENOUGH_TIME_UNITS";
				}
			}
			_parent.popState();
		}
	}

	/**
	 * Runs state functionality every cycle.
	 */
	internal override void think()
	{
		int tu = _chargeTUs ? 1 : 0;

		if (_chargeTUs && _unit.getFaction() == _parent.getSave().getSide() && _parent.getPanicHandled() && !_action.targeting && !_parent.checkReservedTU(_unit, tu))
		{
			_unit.abortTurn();
			_parent.popState();
			return;
		}

		if (_unit.spendTimeUnits(tu))
		{
			int unitSpotted = _unit.getUnitsSpottedThisTurn().Count;
			_unit.turn(_turret);
			_parent.getTileEngine().calculateFOV(_unit);
			_unit.setCache(null);
			_parent.getMap().cacheUnit(_unit);
			if (_chargeTUs && _unit.getFaction() == _parent.getSave().getSide() && _parent.getPanicHandled() && _action.type == BattleActionType.BA_NONE && _unit.getUnitsSpottedThisTurn().Count > unitSpotted)
			{
				_unit.abortTurn();
			}
			if (_unit.getStatus() == UnitStatus.STATUS_STANDING)
			{
				_parent.popState();
			}
		}
		else if (_parent.getPanicHandled())
		{
			_action.result = "STR_NOT_ENOUGH_TIME_UNITS";
			_unit.abortTurn();
			_parent.popState();
		}
	}

	/**
	 * Unit turning cannot be cancelled.
	 */
	internal override void cancel() { }
}
