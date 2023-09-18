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
 * A Psi Attack state.
 */
internal class PsiAttackBState : BattleState
{
	BattleUnit _unit, _target;
	BattleItem _item;
	bool _initialized;

    /**
     * Sets up a PsiAttackBState.
     */
    PsiAttackBState(BattlescapeGame parent, BattleAction action) : base(parent, action)
    {
        _unit = null;
        _target = null;
        _item = null;
        _initialized = false;
    }

    /**
     * Deletes the PsiAttackBState.
     */
    ~PsiAttackBState() { }

	/**
	 * Initializes the sequence:
	 * - checks if the action is valid,
	 * - adds a psi attack animation to the world.
	 * - from that point on, the explode state takes precedence.
	 * - when that state pops, we'll do our first think()
	 */
	protected override void init()
	{
		if (_initialized) return;
		_initialized = true;

		_item = _action.weapon;
		_unit = _action.actor;

		if (_parent.getSave().getTile(_action.target) == null) // invalid target position
		{
			_parent.popState();
			return;
		}

		if (_unit.getTimeUnits() < _action.TU) // not enough time units
		{
			_action.result = "STR_NOT_ENOUGH_TIME_UNITS";
			_parent.popState();
			return;
		}

		_target = _parent.getSave().getTile(_action.target).getUnit();

		if (_target == null) // invalid target
		{
			_parent.popState();
			return;
		}

		if (_item == null) // can't make a psi attack without a weapon
		{
			_parent.popState();
			return;
		}
		else if (_item.getRules().getHitSound() != -1)
		{
			_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)_item.getRules().getHitSound()).play(-1, _parent.getMap().getSoundAngle(_action.target));
		}

		// make a cosmetic explosion
		int height = _target.getFloatHeight() + (_target.getHeight() / 2) - _parent.getSave().getTile(_action.target).getTerrainLevel();
		Position voxel = _action.target * new Position(16, 16, 24) + new Position(8, 8, height);
		_parent.statePushFront(new ExplosionBState(_parent, voxel, _item, _unit, null, false, true));
	}
}
