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
 * along with OpenXcom.  If not, see <http:///www.gnu.org/licenses/>.
 */

namespace SharpXcom.Battlescape;

/**
 * Window that allows the player
 * to select a battlescape action.
 */
internal class ActionMenuState : State
{
	BattleAction _action;
	ActionMenuItem[] _actionMenu = new ActionMenuItem[6];

	/**
	 * Initializes all the elements in the Action Menu window.
	 * @param game Pointer to the core game.
	 * @param action Pointer to the action.
	 * @param x Position on the x-axis.
	 * @param y position on the y-axis.
	 */
	internal ActionMenuState(BattleAction action, int x, int y)
	{
		_action = action;

		_screen = false;

		// Set palette
		_game.getSavedGame().getSavedBattle().setPaletteByDepth(this);

		for (int i = 0; i < 6; ++i)
		{
			_actionMenu[i] = new ActionMenuItem(i, _game, x, y);
			add(_actionMenu[i]);
			_actionMenu[i].setVisible(false);
			_actionMenu[i].onMouseClick(btnActionMenuItemClick);
		}

		// Build up the popup menu
		int id = 0;
		RuleItem weapon = _action.weapon.getRules();

		// throwing (if not a fixed weapon)
		if (!weapon.isFixed())
		{
			addItem(BattleActionType.BA_THROW, "STR_THROW", ref id);
		}

		// priming
		if ((weapon.getBattleType() == BattleType.BT_GRENADE || weapon.getBattleType() == BattleType.BT_PROXIMITYGRENADE)
			&& _action.weapon.getFuseTimer() == -1)
		{
			addItem(BattleActionType.BA_PRIME, "STR_PRIME_GRENADE", ref id);
		}

		if (weapon.getBattleType() == BattleType.BT_FIREARM)
		{
			if (weapon.getWaypoints() != 0 || (_action.weapon.getAmmoItem() != null && _action.weapon.getAmmoItem().getRules().getWaypoints() != 0))
			{
				addItem(BattleActionType.BA_LAUNCH, "STR_LAUNCH_MISSILE", ref id);
			}
			else
			{
				if (weapon.getAccuracyAuto() != 0)
				{
					addItem(BattleActionType.BA_AUTOSHOT, "STR_AUTO_SHOT", ref id);
				}
				if (weapon.getAccuracySnap() != 0)
				{
					addItem(BattleActionType.BA_SNAPSHOT, "STR_SNAP_SHOT", ref id);
				}
				if (weapon.getAccuracyAimed() != 0)
				{
					addItem(BattleActionType.BA_AIMEDSHOT, "STR_AIMED_SHOT", ref id);
				}
			}
		}

		if (weapon.getTUMelee() != 0)
		{
			// stun rod
			if (weapon.getBattleType() == BattleType.BT_MELEE && weapon.getDamageType() == ItemDamageType.DT_STUN)
			{
				addItem(BattleActionType.BA_HIT, "STR_STUN", ref id);
			}
			else
			// melee weapon
			{
				addItem(BattleActionType.BA_HIT, "STR_HIT_MELEE", ref id);
			}
		}
		// special items
		else if (weapon.getBattleType() == BattleType.BT_MEDIKIT)
		{
			addItem(BattleActionType.BA_USE, "STR_USE_MEDI_KIT", ref id);
		}
		else if (weapon.getBattleType() == BattleType.BT_SCANNER)
		{
			addItem(BattleActionType.BA_USE, "STR_USE_SCANNER", ref id);
		}
		else if (weapon.getBattleType() == BattleType.BT_PSIAMP && _action.actor.getBaseStats().psiSkill > 0)
		{
			addItem(BattleActionType.BA_MINDCONTROL, "STR_MIND_CONTROL", ref id);
			addItem(BattleActionType.BA_PANIC, "STR_PANIC_UNIT", ref id);
		}
		else if (weapon.getBattleType() == BattleType.BT_MINDPROBE)
		{
			addItem(BattleActionType.BA_USE, "STR_USE_MIND_PROBE", ref id);
		}
	}

	/**
	 * Deletes the ActionMenuState.
	 */
	~ActionMenuState() { }

	/**
	 * Adds a new menu item for an action.
	 * @param ba Action type.
	 * @param name Action description.
	 * @param id Pointer to the new item ID.
	 */
	void addItem(BattleActionType ba, string name, ref int id)
	{
		string s1 = null, s2;
		int acc = _action.actor.getFiringAccuracy(ba, _action.weapon);
		if (ba == BattleActionType.BA_THROW)
			acc = (int)_action.actor.getThrowingAccuracy();
		int tu = _action.actor.getActionTUs(ba, _action.weapon);

		if (ba == BattleActionType.BA_THROW || ba == BattleActionType.BA_AIMEDSHOT || ba == BattleActionType.BA_SNAPSHOT || ba == BattleActionType.BA_AUTOSHOT || ba == BattleActionType.BA_LAUNCH || ba == BattleActionType.BA_HIT)
			s1 = tr("STR_ACCURACY_SHORT").arg(Unicode.formatPercentage(acc));
		s2 = tr("STR_TIME_UNITS_SHORT").arg(tu);
		_actionMenu[id].setAction(ba, tr(name), s1, s2, tu);
		_actionMenu[id].setVisible(true);
		id++;
	}

	/**
	 * Executes the action corresponding to this action menu item.
	 * @param action Pointer to an action.
	 */
	void btnActionMenuItemClick(Action action)
	{
		_game.getSavedGame().getSavedBattle().getPathfinding().removePreview();

		int btnID = -1;
		RuleItem weapon = _action.weapon.getRules();
		string weaponUsable = _game.getSavedGame().getSavedBattle().getItemUsable(_action.weapon);

		// got to find out which button was pressed
		for (int i = 0; i < _actionMenu.Length /* / _actionMenu[0].Length */ && btnID == -1; ++i)
		{
			if (action.getSender() == _actionMenu[i])
			{
				btnID = i;
			}
		}

		if (btnID != -1)
		{
			_action.type = _actionMenu[btnID].getAction();
			_action.TU = _actionMenu[btnID].getTUs();
			if (_action.type != BattleActionType.BA_THROW &&
				_action.actor.getOriginalFaction() == UnitFaction.FACTION_PLAYER &&
				!_game.getSavedGame().isResearched(weapon.getRequirements()))
			{
				_action.result = "STR_UNABLE_TO_USE_ALIEN_ARTIFACT_UNTIL_RESEARCHED";
				_game.popState();
			}
			else if (_action.type != BattleActionType.BA_THROW && !string.IsNullOrEmpty(weaponUsable))
			{
				_action.result = weaponUsable;
				_game.popState();
			}
			else if (_action.type == BattleActionType.BA_PRIME)
			{
				if (weapon.getBattleType() == BattleType.BT_PROXIMITYGRENADE)
				{
					_action.value = 0;
					_game.popState();
				}
				else
				{
					_game.pushState(new PrimeGrenadeState(_action, false, null));
				}
			}
			else if (_action.type == BattleActionType.BA_USE && weapon.getBattleType() == BattleType.BT_MEDIKIT)
			{
				BattleUnit targetUnit = null;
				List<BattleUnit> units = _game.getSavedGame().getSavedBattle().getUnits();
				for (var i = 0; i < units.Count && targetUnit == null; ++i)
				{
					// we can heal a unit that is at the same position, unconscious and healable(=woundable)
					if (units[i].getPosition() == _action.actor.getPosition() && units[i] != _action.actor && units[i].getStatus() == UnitStatus.STATUS_UNCONSCIOUS && units[i].isWoundable())
					{
						targetUnit = units[i];
					}
				}
				if (targetUnit == null)
				{
					if (_game.getSavedGame().getSavedBattle().getTileEngine().validMeleeRange(
						_action.actor.getPosition(),
						_action.actor.getDirection(),
						_action.actor,
						null, out _action.target, false))
					{
						Tile tile = _game.getSavedGame().getSavedBattle().getTile(_action.target);
						if (tile != null && tile.getUnit() != null && tile.getUnit().isWoundable())
						{
							targetUnit = tile.getUnit();
						}
					}
				}
				if (targetUnit != null)
				{
					_game.popState();
					_game.pushState(new MedikitState(targetUnit, _action));
				}
				else
				{
					_action.result = "STR_THERE_IS_NO_ONE_THERE";
					_game.popState();
				}
			}
			else if (_action.type == BattleActionType.BA_USE && weapon.getBattleType() == BattleType.BT_SCANNER)
			{
				// spend TUs first, then show the scanner
				if (_action.actor.spendTimeUnits(_action.TU))
				{
					_game.popState();
					_game.pushState(new ScannerState(_action));
				}
				else
				{
					_action.result = "STR_NOT_ENOUGH_TIME_UNITS";
					_game.popState();
				}
			}
			else if (_action.type == BattleActionType.BA_LAUNCH)
			{
				// check beforehand if we have enough time units
				if (_action.TU > _action.actor.getTimeUnits())
				{
					_action.result = "STR_NOT_ENOUGH_TIME_UNITS";
				}
				else if (_action.weapon.getAmmoItem() == null || (_action.weapon.getAmmoItem() != null && _action.weapon.getAmmoItem().getAmmoQuantity() == 0))
				{
					_action.result = "STR_NO_AMMUNITION_LOADED";
				}
				else
				{
					_action.targeting = true;
				}
				_game.popState();
			}
			else if (_action.type == BattleActionType.BA_HIT)
			{
				// check beforehand if we have enough time units
				if (_action.TU > _action.actor.getTimeUnits())
				{
					_action.result = "STR_NOT_ENOUGH_TIME_UNITS";
				}
				else if (!_game.getSavedGame().getSavedBattle().getTileEngine().validMeleeRange(
					_action.actor.getPosition(),
					_action.actor.getDirection(),
					_action.actor,
					null, out _action.target))
				{
					_action.result = "STR_THERE_IS_NO_ONE_THERE";
				}
				_game.popState();
			}
			else
			{
				_action.targeting = true;
				_game.popState();
			}
			// meleeAttackBState won't be available to clear the action type, do it here instead.
			if (_action.type == BattleActionType.BA_HIT && !string.IsNullOrEmpty(_action.result))
			{
				_action.type = BattleActionType.BA_NONE;
			}
		}
	}

	/**
	 * Closes the window on right-click.
	 * @param action Pointer to an action.
	 */
	internal override void handle(Action action)
	{
		base.handle(action);
		if (action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN && action.getDetails().button.button == SDL_BUTTON_RIGHT)
		{
			_game.popState();
		}
		else if (action.getDetails().type == SDL_EventType.SDL_KEYDOWN &&
			(action.getDetails().key.keysym.sym == Options.keyCancel ||
			action.getDetails().key.keysym.sym == Options.keyBattleUseLeftHand ||
			action.getDetails().key.keysym.sym == Options.keyBattleUseRightHand))
		{
			_game.popState();
		}
	}

	/**
	 * Updates the scale.
	 * @param dX delta of X;
	 * @param dY delta of Y;
	 */
	internal override void resize(ref int dX, ref int dY) =>
		base.recenter(dX, dY * 2);
}
