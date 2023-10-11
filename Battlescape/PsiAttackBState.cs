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
    internal PsiAttackBState(BattlescapeGame parent, BattleAction action) : base(parent, action)
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

	/**
	 * After the explosion animation is done doing its thing,
	 * make the actual psi attack, and restore the camera/cursor.
	 */
	protected override void think()
	{
		//make the psi attack.
		psiAttack();

		if (_action.cameraPosition.z != -1)
		{
			_parent.getMap().getCamera().setMapOffset(_action.cameraPosition);
			_parent.getMap().invalidate();
		}
		if (_parent.getSave().getSide() == UnitFaction.FACTION_PLAYER || _parent.getSave().getDebugMode())
		{
			_parent.setupCursor();
		}
		_parent.popState();
	}

	/**
	 * Attempts a panic or mind control action.
	 */
	void psiAttack()
	{
		double attackStrength = _unit.getBaseStats().psiStrength * _unit.getBaseStats().psiSkill / 50.0;
		double defenseStrength = _target.getBaseStats().psiStrength
			+ ((_target.getBaseStats().psiSkill > 0) ? 10.0 + _target.getBaseStats().psiSkill / 5.0 : 10.0);
		double dist = _parent.getTileEngine().distance(_unit.getPosition(), _action.target);
		attackStrength -= dist;
		attackStrength += RNG.generate(0,55);

		if (_action.type == BattleActionType.BA_MINDCONTROL)
		{
			defenseStrength += 20;
		}

		_unit.addPsiSkillExp();
		if (Options.allowPsiStrengthImprovement) _target.addPsiStrengthExp();
		if (attackStrength > defenseStrength)
		{
			Game game = _parent.getSave().getBattleState().getGame();
			_action.actor.addPsiSkillExp();
			_action.actor.addPsiSkillExp();

			var killStat = new BattleUnitKills();
			killStat.setUnitStats(_target);
			killStat.setTurn(_parent.getSave().getTurn(), _parent.getSave().getSide());
			killStat.weapon = _action.weapon.getRules().getName();
			killStat.weaponAmmo = _action.weapon.getRules().getName(); //Psi weapons got no ammo, just filling up the field
			killStat.faction = _target.getFaction();
			killStat.mission = _parent.getSave().getGeoscapeSave().getMissionStatistics().Count;
			killStat.id = _target.getId();

			if (_action.type == BattleActionType.BA_PANIC)
			{
				int moraleLoss = (110-_target.getBaseStats().bravery);
				if (moraleLoss > 0)
				_target.moraleChange(-moraleLoss);
				_target.setMindControllerId(_unit.getId());
				// Award Panic battle unit kill
				if (!_unit.getStatistics().duplicateEntry(UnitStatus.STATUS_PANICKING, _target.getId()))
				{
					killStat.status = UnitStatus.STATUS_PANICKING;
					_unit.getStatistics().kills.Add(killStat);
				}
				if (_parent.getSave().getSide() == UnitFaction.FACTION_PLAYER)
				{
					game.pushState(new InfoboxState(game.getLanguage().getString("STR_MORALE_ATTACK_SUCCESSFUL")));
				}
			}
			else if (_action.type == BattleActionType.BA_MINDCONTROL)
			{
				// Award MC battle unit kill
				if (!_unit.getStatistics().duplicateEntry(UnitStatus.STATUS_TURNING, _target.getId()))
				{
					killStat.status = UnitStatus.STATUS_TURNING;
					_unit.getStatistics().kills.Add(killStat);
				}
				_target.setMindControllerId(_unit.getId());
				_target.convertToFaction(_unit.getFaction());
				_parent.getTileEngine().calculateFOV(_target.getPosition());
				_parent.getTileEngine().calculateUnitLighting();
				_target.recoverTimeUnits();
				_target.allowReselect();
				_target.abortTurn(); // resets unit status to STANDING
				// if all units from either faction are mind controlled - auto-end the mission.
				if (_parent.getSave().getSide() == UnitFaction.FACTION_PLAYER)
				{
					if (Options.allowPsionicCapture)
					{
						_parent.autoEndBattle();
					}
					game.pushState(new InfoboxState(game.getLanguage().getString("STR_MIND_CONTROL_SUCCESSFUL")));
					_parent.getSave().getBattleState().updateSoldierInfo();
				}
				else
				{
					// show a little infobox with the name of the unit and "... is under alien control"
					game.pushState(new InfoboxState(game.getLanguage().getString("STR_IS_UNDER_ALIEN_CONTROL", (uint)_target.getGender()).arg(_target.getName(game.getLanguage()))));
				}
			}
		}
		else
		{
			if (Options.allowPsiStrengthImprovement)
			{
				_target.addPsiStrengthExp();
			}
		}
	}
}
