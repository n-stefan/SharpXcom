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
 * Screen which asks for confirmation to abort mission.
 */
internal class AbortMissionState : State
{
	SavedBattleGame _battleGame;
	BattlescapeState _state;
	int _inEntrance, _inExit, _outside;
	Window _window;
	Text _txtInEntrance, _txtInExit, _txtOutside, _txtAbort;
	TextButton _btnOk, _btnCancel;

	/**
	 * Initializes all the elements in the Abort Mission window.
	 * @param game Pointer to the core game.
	 * @param battleGame Pointer to the saved game.
	 * @param state Pointer to the Battlescape state.
	 */
	internal AbortMissionState(SavedBattleGame battleGame, BattlescapeState state)
	{
		_battleGame = battleGame;
		_state = state;
		_inEntrance = 0;
		_inExit = 0;
		_outside = 0;

		// Create objects
		_screen = false;
		_window = new Window(this, 320, 144, 0, 0);
		_txtInEntrance = new Text(304, 17, 16, 20);
		_txtInExit = new Text(304, 17, 16, 40);
		_txtOutside = new Text(304, 17, 16, 60);
		_txtAbort = new Text(320, 17, 0, 80);
		_btnOk = new TextButton(120, 16, 16, 110);
		_btnCancel = new TextButton(120, 16, 184, 110);

		// Set palette
		_battleGame.setPaletteByDepth(this);

		add(_window, "messageWindowBorder", "battlescape");
		add(_txtInEntrance, "messageWindows", "battlescape");
		add(_txtInExit, "messageWindows", "battlescape");
		add(_txtOutside, "messageWindows", "battlescape");
		add(_txtAbort, "messageWindows", "battlescape");
		add(_btnOk, "messageWindowButtons", "battlescape");
		add(_btnCancel, "messageWindowButtons", "battlescape");

		// Check available areas (maybe should be cached somewhere)
		bool exit = false, craft = true;
		AlienDeployment deployment = _game.getMod().getDeployment(_battleGame.getMissionType());
		if (deployment != null)
		{
			exit = !string.IsNullOrEmpty(deployment.getNextStage()) || deployment.getEscapeType() == EscapeType.ESCAPE_EXIT || deployment.getEscapeType() == EscapeType.ESCAPE_EITHER;
			List<MapScript> scripts = _game.getMod().getMapScript(deployment.getScript());
			if (scripts != null)
			{
				craft = false;
				foreach (var i in scripts)
				{
					if (i.getType() == MapScriptCommand.MSC_ADDCRAFT)
					{
						craft = true;
						break;
					}
				}
			}
		}
		if (exit)
		{
			exit = false;
			for (int i = 0; i < _battleGame.getMapSizeXYZ(); ++i)
			{
				Tile tile = _battleGame.getTiles()[i];
				if (tile != null && tile.getMapData(TilePart.O_FLOOR) != null && tile.getMapData(TilePart.O_FLOOR).getSpecialType() == SpecialTileType.END_POINT)
				{
					exit = true;
					break;
				}
			}
		}

		// Calculate values
		foreach (var i in _battleGame.getUnits())
		{
			if (i.getOriginalFaction() == UnitFaction.FACTION_PLAYER)
			{
				if (i.getStatus() != UnitStatus.STATUS_DEAD && i.getStatus() != UnitStatus.STATUS_IGNORE_ME)
				{
					Tile unitTile = _battleGame.getTile(i.getPosition());
					if (unitTile != null)
					{
						MapData floor = unitTile.getMapData(TilePart.O_FLOOR);
						if (floor != null)
						{
							if (floor.getSpecialType() == SpecialTileType.START_POINT)
							{
								_inEntrance++;
								continue;
							}
							else if (floor.getSpecialType() == SpecialTileType.END_POINT)
							{
								_inExit++;
								continue;
							}
						}
					}
					_outside++;
				}
			}
		}

		// Set up objects
		_window.setHighContrast(true);
		_window.setBackground(_game.getMod().getSurface("TAC00.SCR"));

		_txtInEntrance.setBig();
		_txtInEntrance.setHighContrast(true);
		if (craft)
		{
			_txtInEntrance.setText(tr("STR_UNITS_IN_CRAFT", (uint)_inEntrance));
		}
		else
		{
			_txtInEntrance.setText(tr("STR_UNITS_IN_ENTRANCE", (uint)_inEntrance));
		}

		_txtInExit.setBig();
		_txtInExit.setHighContrast(true);
		_txtInExit.setText(tr("STR_UNITS_IN_EXIT", (uint)_inExit));

		_txtOutside.setBig();
		_txtOutside.setHighContrast(true);
		_txtOutside.setText(tr("STR_UNITS_OUTSIDE", (uint)_outside));

		if (_battleGame.getMissionType() == "STR_BASE_DEFENSE")
		{
			_txtInEntrance.setVisible(false);
			_txtInExit.setVisible(false);
			_txtOutside.setVisible(false);
		}
		else if (!exit)
		{
			_txtInEntrance.setY(26);
			_txtOutside.setY(54);
			_txtInExit.setVisible(false);
		}

		_txtAbort.setBig();
		_txtAbort.setAlign(TextHAlign.ALIGN_CENTER);
		_txtAbort.setHighContrast(true);
		_txtAbort.setText(tr("STR_ABORT_MISSION_QUESTION"));

		_btnOk.setText(tr("STR_OK"));
		_btnOk.setHighContrast(true);
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

		_btnCancel.setText(tr("STR_CANCEL_UC"));
		_btnCancel.setHighContrast(true);
		_btnCancel.onMouseClick(btnCancelClick);
		_btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);
		_btnCancel.onKeyboardPress(btnCancelClick, Options.keyBattleAbort);

		centerAllSurfaces();
	}

	/**
	 *
	 */
	~AbortMissionState() { }

	/**
	 * Confirms mission abort.
	 * @param action Pointer to an action.
	 */
	void btnOkClick(Action _)
	{
		_game.popState();
		_battleGame.setAborted(true);
		_state.finishBattle(true, _inExit);
	}

	/**
	 * Returns to the previous screen.
	 * @param action Pointer to an action.
	 */
	void btnCancelClick(Action _) =>
		_game.popState();
}
