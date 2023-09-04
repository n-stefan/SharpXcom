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
 * A Melee Attack state.
 */
internal class MeleeAttackBState : BattleState
{
	BattleUnit _unit, _target;
	BattleItem _weapon, _ammo;
	bool _initialized;
	Position _voxel;

    /**
     * Sets up a MeleeAttackBState.
     */
    MeleeAttackBState(BattlescapeGame parent, BattleAction action) : base(parent, action)
    {
        _unit = null;
        _target = null;
        _weapon = null;
        _ammo = null;
        _initialized = false;
    }

    /**
     * Deletes the MeleeAttackBState.
     */
    ~MeleeAttackBState() { }

	/**
	 * Initializes the sequence.
	 * does a lot of validity checking.
	 */
	protected override void init()
	{
		if (_initialized) return;
		_initialized = true;

		_weapon = _action.weapon;
		if (_weapon == null) // can't shoot without weapon
		{
			_parent.popState();
			return;
		}
		_ammo = _weapon.getAmmoItem();
		if (_ammo == null)
		{
			_ammo = _weapon;
		}

		if (_parent.getSave().getTile(_action.target) == null) // invalid target position
		{
			_parent.popState();
			return;
		}

		_unit = _action.actor;

		if (_unit.isOut() || _unit.getHealth() == 0 || _unit.getHealth() < _unit.getStunlevel())
		{
			// something went wrong - we can't shoot when dead or unconscious, or if we're about to fall over.
			_parent.popState();
			return;
		}

		// reaction fire
		if (_unit.getFaction() != _parent.getSave().getSide())
		{
			// no ammo or target is dead: give the time units back and cancel the shot.
			if (_ammo == null
				|| _parent.getSave().getTile(_action.target).getUnit() == null
				|| _parent.getSave().getTile(_action.target).getUnit().isOut()
				|| _parent.getSave().getTile(_action.target).getUnit() != _parent.getSave().getSelectedUnit())
			{
				_unit.setTimeUnits(_unit.getTimeUnits() + _unit.getActionTUs(_action.type, _action.weapon));
				_parent.popState();
				return;
			}
			_unit.lookAt(_action.target, _unit.getTurretType() != -1);
			while (_unit.getStatus() == UnitStatus.STATUS_TURNING)
			{
				_unit.turn();
			}
		}

		AIModule ai = _unit.getAIModule();

		if (_unit.getFaction() == _parent.getSave().getSide() &&
			_unit.getFaction() != UnitFaction.FACTION_PLAYER &&
			BattlescapeGame._debugPlay == false &&
			ai != null && ai.getTarget() != null)
		{
			_target = ai.getTarget();
		}
		else
		{
			_target = _parent.getSave().getTile(_action.target).getUnit();
		}

		int height = _target.getFloatHeight() + (_target.getHeight() / 2) - _parent.getSave().getTile(_action.target).getTerrainLevel();
		_voxel = _action.target * new Position(16, 16, 24) + new Position(8, 8, height);

		performMeleeAttack();
	}

	/**
	 * Sets up a melee attack, inserts an explosion into the map and make noises.
	 */
	void performMeleeAttack()
	{
		// set the soldier in an aiming position
		_unit.aim(true);
		_unit.setCache(null);
		_parent.getMap().cacheUnit(_unit);
		// make some noise
		if (_ammo != null && _ammo.getRules().getMeleeAttackSound() != -1)
		{
			_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)_ammo.getRules().getMeleeAttackSound()).play(-1, _parent.getMap().getSoundAngle(_action.target));
		}
		else if (_weapon.getRules().getMeleeAttackSound() != -1)
		{
			_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)_weapon.getRules().getMeleeAttackSound()).play(-1, _parent.getMap().getSoundAngle(_action.target));
		}
		// use up ammo if applicable
		if (!_parent.getSave().getDebugMode() && _weapon.getRules().getBattleType() == BattleType.BT_MELEE && _ammo != null && _ammo.spendBullet() == false)
		{
			_parent.getSave().removeItem(_ammo);
			_action.weapon.setAmmoItem(null);
		}
		_parent.getMap().setCursorType(CursorType.CT_NONE);

		// make an explosion animation
		_parent.statePushFront(new ExplosionBState(_parent, _voxel, _action.weapon, _action.actor, null, true, true));
	}
}
