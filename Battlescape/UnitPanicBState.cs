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
 * State for panicking units.
 */
internal class UnitPanicBState : BattleState
{
	BattleUnit _unit;
	int _shotsFired;
	bool _berserking;

	/**
	 * Sets up an UnitPanicBState.
	 * @param parent Pointer to the Battlescape.
	 * @param unit Panicking unit.
	 */
	internal UnitPanicBState(BattlescapeGame parent, BattleUnit unit) : base(parent)
	{
		_unit = unit;
		_shotsFired = 0;

		_berserking = _unit.getStatus() == UnitStatus.STATUS_BERSERK;
		unit.abortTurn(); //makes the unit go to status STANDING :p
	}

	/**
	 * Deletes the UnitPanicBState.
	 */
	~UnitPanicBState() { }

	internal override void init() { }

	/**
	 * Runs state functionality every cycle.
	 * Ends the panicking when done.
	 */
	internal override void think()
	{
		if (_unit != null)
		{
			// berserking requires handling here, as the target selection isn't completely random
			// and needs updating between shots.
			if (!_unit.isOut() && _shotsFired < 10 && _berserking)
			{
				_shotsFired++;
				var ba = new BattleAction();
				ba.actor = _unit;
				ba.weapon = _unit.getMainHandWeapon();
				if (ba.weapon != null && (ba.weapon.getRules().getTUSnap() != 0 || ba.weapon.getRules().getTUAuto() != 0)
					&& _parent.getSave().isItemUsable(ba.weapon))
				{
					// make autoshots if possible.
					if (ba.weapon.getRules().getTUAuto() != 0)
						ba.type = BattleActionType.BA_AUTOSHOT;
					else
						ba.type = BattleActionType.BA_SNAPSHOT;

					ba.TU = _unit.getActionTUs(ba.type, ba.weapon);

					if (_unit.getTimeUnits() >= ba.TU)
					{
						// if we see enemies, shoot at the closest living one.
						if (_unit.getVisibleUnits().Any())
						{
							int dist = 255;
							foreach (var i in _unit.getVisibleUnits())
							{
								int newDist = _parent.getTileEngine().distance(_unit.getPosition(), i.getPosition());
								if (newDist < dist)
								{
									ba.target = i.getPosition();
									dist = newDist;
								}
							}
						}
						else // otherwise shoot randomly
						{
							ba.target = new Position(_unit.getPosition().x + RNG.generate(-6,6), _unit.getPosition().y + RNG.generate(-6,6), _unit.getPosition().z);
						}
						// include the cost for facing our target
						int turnCost = Math.Abs(_unit.getDirection() - _unit.directionTo(ba.target));
						if (turnCost > 4)
						{
							turnCost = 8-turnCost;
						}

						_parent.statePushFront(new UnitTurnBState(_parent, ba, false));
						// even if we don't have enough TUs to turn AND shoot, we still want to turn.
						if (_unit.spendTimeUnits(ba.TU + turnCost))
						{
							_parent.statePushNext(new ProjectileFlyBState(_parent, ba));
						}
						else
						{
							_unit.spendTimeUnits(turnCost);
						}
					}
				}
				return;
			}
			if (!_unit.isOut())
			{
				_unit.abortTurn(); // set the unit status to standing in case it wasn't otherwise changed from berserk/panicked
			}
			// reset the unit's time units when all panicking is done
			_unit.setTimeUnits(0);
		}
		_parent.popState();
		_parent.setupCursor();
	}

	/**
	 * Panicking cannot be cancelled.
	 */
	internal override void cancel() { }
}
