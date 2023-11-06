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
 * Helper class for the medikit button.
 */
class MedikitButton : InteractiveSurface
{
	/**
	 * Initializes a Medikit button.
	 * @param y The button's y origin.
	 */
	internal MedikitButton(int y) : base(30, 20, 190, y) { }
}

/**
 * Helper class for the medikit value.
 */
class MedikitTxt : Text
{
	/**
	 * Initializes a Medikit text.
	 * @param y The text's y origin.
	 */
	internal MedikitTxt(int y) : base(33, 17, 220, y)
	{
		// Note: we can't set setBig here. The needed font is only set when added to State
		this.setColor(Palette.blockOffset(1));
		this.setHighContrast(true);
		this.setAlign(TextHAlign.ALIGN_CENTER);
	}
}

/**
 * Helper class for the medikit title.
 */
class MedikitTitle : Text
{
	/**
	 * Initializes a Medikit title.
	 * @param y The title's y origin.
	 * @param title The title.
	 */
	internal MedikitTitle(int y, string title) : base(73, 9, 186, y)
	{
		this.setText(title);
		this.setHighContrast(true);
		this.setAlign(TextHAlign.ALIGN_CENTER);
	}
}

/**
 * The Medikit User Interface. Medikit is an item for healing a soldier.
 */
internal class MedikitState : State
{
	BattleUnit _targetUnit, _unit;
	BattleAction _action;
	bool _revivedTarget;
	int _tu;
	BattleItem _item;
	Surface _bg;
	Text _pkText, _stimulantTxt, _healTxt, _partTxt, _woundTxt;
	MedikitView _medikitView;
	InteractiveSurface _endButton, _stimulantButton, _pkButton, _healButton;

	/**
	 * Initializes the Medikit State.
	 * @param game Pointer to the core game.
	 * @param targetUnit The wounded unit.
	 * @param action The healing action.
	 */
	internal MedikitState(BattleUnit targetUnit, BattleAction action)
	{
		_targetUnit = targetUnit;
		_action = action;
		_revivedTarget = false;

		if (Options.maximizeInfoScreens)
		{
			Options.baseXResolution = Screen.ORIGINAL_WIDTH;
			Options.baseYResolution = Screen.ORIGINAL_HEIGHT;
			_game.getScreen().resetDisplay(false);
		}

		_tu = action.TU;
		_unit = action.actor;
		_item = action.weapon;
		_bg = new Surface(320, 200);

		// Set palette
		_game.getSavedGame().getSavedBattle().setPaletteByDepth(this);

		if (_game.getScreen().getDY() > 50)
		{
			_screen = false;
			_bg.drawRect(67, 44, 190, 100, (byte)(Palette.blockOffset(15)+15));
		}
		_partTxt = new Text(62, 9, 82, 120);
		_woundTxt = new Text(14, 9, 145, 120);
		_medikitView = new MedikitView(52, 58, 95, 60, _game, _targetUnit, _partTxt, _woundTxt);
		_endButton = new InteractiveSurface(20, 20, 220, 140);
		_stimulantButton = new MedikitButton(84);
		_pkButton = new MedikitButton(48);
		_healButton = new MedikitButton(120);
		_pkText = new MedikitTxt(52);
		_stimulantTxt = new MedikitTxt(88);
		_healTxt = new MedikitTxt(124);
		add(_bg);
		add(_medikitView, "body", "medikit", _bg);
		add(_endButton, "buttonEnd", "medikit", _bg);
		add(new MedikitTitle(37, tr("STR_PAIN_KILLER")), "textPK", "medikit", _bg);
		add(new MedikitTitle(73, tr("STR_STIMULANT")), "textStim", "medikit", _bg);
		add(new MedikitTitle(109, tr("STR_HEAL")), "textHeal", "medikit", _bg);
		add(_healButton, "buttonHeal", "medikit", _bg);
		add(_stimulantButton, "buttonStim", "medikit", _bg);
		add(_pkButton, "buttonPK", "medikit", _bg);
		add(_pkText, "numPK", "medikit", _bg);
		add(_stimulantTxt, "numStim", "medikit", _bg);
		add(_healTxt, "numHeal", "medikit", _bg);
		add(_partTxt, "textPart", "medikit", _bg);
		add(_woundTxt, "numWounds", "medikit", _bg);

		centerAllSurfaces();

		_game.getMod().getSurface("MEDIBORD.PCK").blit(_bg);
		_pkText.setBig();
		_stimulantTxt.setBig();
		_healTxt.setBig();
		_partTxt.setHighContrast(true);
		_woundTxt.setHighContrast(true);
		_endButton.onMouseClick(onEndClick);
		_endButton.onKeyboardPress(onEndClick, Options.keyCancel);
		_healButton.onMouseClick(onHealClick);
		_stimulantButton.onMouseClick(onStimulantClick);
		_pkButton.onMouseClick(onPainKillerClick);
		update();
	}

	/**
	 * Returns to the previous screen.
	 * @param action Pointer to an action.
	 */
	void onEndClick(Action _)
	{
		if (Options.maximizeInfoScreens)
		{
			Screen.updateScale(Options.battlescapeScale, ref Options.baseXBattlescape, ref Options.baseYBattlescape, true);
			_game.getScreen().resetDisplay(false);
		}
		_game.popState();
	}

	/**
	 * Handler for clicking on the heal button.
	 * @param action Pointer to an action.
	 */
	void onHealClick(Action _)
	{
		int heal = _item.getHealQuantity();
		RuleItem rule = _item.getRules();
		if (heal == 0)
		{
			return;
		}
		if (_unit.spendTimeUnits(_tu))
		{
			_targetUnit.heal(_medikitView.getSelectedPart(), rule.getWoundRecovery(), rule.getHealthRecovery());
			_item.setHealQuantity(--heal);
			_medikitView.updateSelectedPart();
			_medikitView.invalidate();
			update();

			if (_targetUnit.getStatus() == UnitStatus.STATUS_UNCONSCIOUS && _targetUnit.getStunlevel() < _targetUnit.getHealth() && _targetUnit.getHealth() > 0)
			{
				if (!_revivedTarget)
				{
					_targetUnit.setTimeUnits(0);
					if(_targetUnit.getOriginalFaction() == UnitFaction.FACTION_PLAYER)
					{
						_action.actor.getStatistics().revivedSoldier++;
					}
					else if(_targetUnit.getOriginalFaction() == UnitFaction.FACTION_HOSTILE)
					{
						_action.actor.getStatistics().revivedHostile++;
					}
					else
					{
						_action.actor.getStatistics().revivedNeutral++;
					}
					_revivedTarget = true;
				}
				// if the unit has revived and has no more wounds, we quit this screen automatically
				if (_targetUnit.getFatalWounds() == 0)
				{
					onEndClick(null);
				}
			}
			_unit.getStatistics().woundsHealed++;
		}
		else
		{
			_action.result = "STR_NOT_ENOUGH_TIME_UNITS";
			onEndClick(null);
		}
	}

	/**
	 * Updates the medikit state.
	 */
	void update()
	{
		_pkText.setText(_item.getPainKillerQuantity().ToString());
		_stimulantTxt.setText(_item.getStimulantQuantity().ToString());
		_healTxt.setText(_item.getHealQuantity().ToString());
		_medikitView.invalidate();
	}

	/**
	 * Handler for clicking on the stimulant button.
	 * @param action Pointer to an action.
	 */
	void onStimulantClick(Action _)
	{
		int stimulant = _item.getStimulantQuantity();
		RuleItem rule = _item.getRules();
		if (stimulant == 0)
		{
			return;
		}
		if (_unit.spendTimeUnits (_tu))
		{
			_targetUnit.stimulant(rule.getEnergyRecovery(), rule.getStunRecovery());
			_item.setStimulantQuantity(--stimulant);
			_action.actor.getStatistics().appliedStimulant++;
			update();

			// if the unit has revived we quit this screen automatically
			if (_targetUnit.getStatus() == UnitStatus.STATUS_UNCONSCIOUS && _targetUnit.getStunlevel() < _targetUnit.getHealth() && _targetUnit.getHealth() > 0)
			{
				_targetUnit.setTimeUnits(0);
				if(_targetUnit.getOriginalFaction() == UnitFaction.FACTION_PLAYER)
				{
					_action.actor.getStatistics().revivedSoldier++;
				}
				else if(_targetUnit.getOriginalFaction() == UnitFaction.FACTION_HOSTILE)
				{
					_action.actor.getStatistics().revivedHostile++;
				}
				else
				{
					_action.actor.getStatistics().revivedNeutral++;
				}
				onEndClick(null);
			}
		}
		else
		{
			_action.result = "STR_NOT_ENOUGH_TIME_UNITS";
			onEndClick(null);
		}
	}

	/**
	 * Handler for clicking on the pain killer button.
	 * @param action Pointer to an action.
	 */
	void onPainKillerClick(Action _)
	{
		int pk = _item.getPainKillerQuantity();
		if (pk == 0)
		{
			return;
		}
		if (_unit.spendTimeUnits(_tu))
		{
			_targetUnit.painKillers();
			_item.setPainKillerQuantity(--pk);
			_action.actor.getStatistics().appliedPainKill++;
			update();
		}
		else
		{
			_action.result = "STR_NOT_ENOUGH_TIME_UNITS";
			onEndClick(null);
		}
	}

	/**
	 * Closes the window on right-click.
	 * @param action Pointer to an action.
	 */
	protected override void handle(Action action)
	{
		base.handle(action);
		if (action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN && action.getDetails().button.button == SDL_BUTTON_RIGHT)
		{
			onEndClick(null);
		}
	}
}
