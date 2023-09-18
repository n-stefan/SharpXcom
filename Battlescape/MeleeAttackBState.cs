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
    internal MeleeAttackBState(BattlescapeGame parent, BattleAction action) : base(parent, action)
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

	/**
	 * Performs all the overall functions of the state, this code runs AFTER the explosion state pops.
	 */
	protected override void think()
	{
		_parent.getSave().getBattleState().clearMouseScrollingState();

		// if the unit burns floortiles, burn floortiles
		if (_unit.getSpecialAbility() == (int)SpecialAbility.SPECAB_BURNFLOOR || _unit.getSpecialAbility() == (int)SpecialAbility.SPECAB_BURN_AND_EXPLODE)
		{
			_parent.getSave().getTile(_action.target).ignite(15);
		}
		// Determine if the attack was successful
		// we do this here instead of letting the explosionBState take care of damage and casualty checking
		// this is because unlike regular bullet hits or explosions, melee attacks can MISS.
		// we also do it at this point instead of in performMeleeAttack because we want the scream to come AFTER the hit sound
		resolveHit();
			// aliens
		if (_unit.getFaction() != UnitFaction.FACTION_PLAYER &&
			// not performing a reaction attack
			_unit.getFaction() == _parent.getSave().getSide() &&
			// with enough TU for a second attack (*2 because they'll get charged for the initial attack when this state pops.)
			_unit.getTimeUnits() >= _unit.getActionTUs(BattleActionType.BA_HIT, _action.weapon) * 2 &&
			// whose target is still alive or at least conscious
			_target != null && _target.getHealth() > 0 &&
			_target.getHealth() > _target.getStunlevel() &&
			// and we still have ammo to make the attack
			_weapon.getAmmoItem() != null)
		{
			// spend the TUs immediately
			_unit.spendTimeUnits(_unit.getActionTUs(BattleActionType.BA_HIT, _weapon));
			performMeleeAttack();
		}
		else
		{
			if (_action.cameraPosition.z != -1)
			{
				_parent.getMap().getCamera().setMapOffset(_action.cameraPosition);
				_parent.getMap().invalidate();
			}
	//		melee doesn't trigger a reaction, remove comments to enable.
	//		if (!_parent.getSave().getUnitsFalling())
	//		{
	//			_parent.getTileEngine().checkReactionFire(_unit);
	//		}

			if (_unit.getFaction() == _parent.getSave().getSide()) // not a reaction attack
			{
				_parent.getCurrentAction().type = BattleActionType.BA_NONE; // do this to restore cursor
			}

			if (_parent.getSave().getSide() == UnitFaction.FACTION_PLAYER || _parent.getSave().getDebugMode())
			{
				_parent.setupCursor();
			}
			_parent.convertInfected();
			_parent.popState();
		}
	}

	/**
	 * Determines if the melee attack hit, and performs all the applicable duties.
	 */
	void resolveHit()
	{
		if (RNG.percent(_unit.getFiringAccuracy(BattleActionType.BA_HIT, _weapon)))
		{
			// Give soldiers XP
			if (_unit.getGeoscapeSoldier() != null &&
				_target != null && _target.getOriginalFaction() == UnitFaction.FACTION_HOSTILE)
			{
				_unit.addMeleeExp();
			}

			// check if this unit turns others into zombies
			if (_weapon.getRules().getBattleType() == BattleType.BT_MELEE
				&& _ammo != null
				&& !string.IsNullOrEmpty(_ammo.getRules().getZombieUnit())
				&& _target != null
				&& (_target.getGeoscapeSoldier() != null || _target.getUnitRules().getRace() == "STR_CIVILIAN")
				&& string.IsNullOrEmpty(_target.getSpawnUnit()))
			{
				// converts the victim to a zombie on death
				_target.setRespawn(true);
				_target.setSpawnUnit(_ammo.getRules().getZombieUnit());
			}

			// assume rifle butt to begin with.
			ItemDamageType type = ItemDamageType.DT_STUN;
			int power = _weapon.getRules().getMeleePower();
			// override it as needed.
			if (_weapon.getRules().getBattleType() == BattleType.BT_MELEE && _ammo != null)
			{
				type = _ammo.getRules().getDamageType();;
				power = _ammo.getRules().getPower();
			}

			// since melee aliens don't use a conventional weapon type, we use their strength instead.
			if (_weapon.getRules().isStrengthApplied())
			{
				power += _unit.getBaseStats().strength;
			}
			// make some noise to signal the hit.
			if (_weapon.getRules().getMeleeHitSound() != -1)
			{
				_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)_action.weapon.getRules().getMeleeHitSound()).play(-1, _parent.getMap().getSoundAngle(_action.target));
			}

			// offset the damage voxel ever so slightly so that the target knows which side the attack came from
			Position difference = _unit.getPosition() - _action.target;
			// large units may cause it to offset too much, so we'll clamp the values.
			difference.x = Math.Clamp(difference.x, -1, 1);
			difference.y = Math.Clamp(difference.y, -1, 1);

			Position damagePosition = _voxel + difference;
			// damage the unit.
			_parent.getSave().getTileEngine().hit(damagePosition, power, type, _unit);
			// now check for new casualties
			_parent.checkForCasualties(_ammo, _unit);
		}
	}
}
