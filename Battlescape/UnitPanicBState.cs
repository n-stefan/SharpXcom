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

	protected override void init() { }
}
