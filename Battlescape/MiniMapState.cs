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
 * The MiniMap is a representation of a Battlescape map that allows you to see more of the map.
 */
internal class MiniMapState : State
{
	Surface _bg;
	MiniMapView _miniMapView;
	BattlescapeButton _btnLvlUp, _btnLvlDwn, _btnOk;
	Text _txtLevel;
	Timer _timerAnimate;

	/**
	 * Initializes all the elements in the MiniMapState screen.
	 * @param game Pointer to the core game.
	 * @param camera The Battlescape camera.
	 * @param battleGame The Battlescape save.
	 */
	internal MiniMapState(Camera camera, SavedBattleGame battleGame)
	{
		if (Options.maximizeInfoScreens)
		{
			Options.baseXResolution = Screen.ORIGINAL_WIDTH;
			Options.baseYResolution = Screen.ORIGINAL_HEIGHT;
			_game.getScreen().resetDisplay(false);
		}

		_bg = new Surface(320, 200);
		_miniMapView = new MiniMapView(221, 148, 48, 16, _game, camera, battleGame);
		_btnLvlUp = new BattlescapeButton(18, 20, 24, 62);
		_btnLvlDwn = new BattlescapeButton(18, 20, 24, 88);
		_btnOk = new BattlescapeButton(32, 32, 275, 145);
		_txtLevel = new Text(28, 16, 281, 75);

		// Set palette
		battleGame.setPaletteByDepth(this);

		add(_bg);
		_game.getMod().getSurface("SCANBORD.PCK").blit(_bg);
		add(_miniMapView);
		add(_btnLvlUp, "buttonUp", "minimap", _bg);
		add(_btnLvlDwn, "buttonDown", "minimap", _bg);
		add(_btnOk, "buttonOK", "minimap", _bg);
		add(_txtLevel, "textLevel", "minimap", _bg);

		centerAllSurfaces();

		if (_game.getScreen().getDY() > 50)
		{
			_screen = false;
			_bg.drawRect(46, 14, 223, 151, (byte)(Palette.blockOffset(15)+15));
		}

		_btnLvlUp.onMouseClick(btnLevelUpClick);
		_btnLvlDwn.onMouseClick(btnLevelDownClick);
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyBattleMap);
		_txtLevel.setBig();
		_txtLevel.setHighContrast(true);
		_txtLevel.setText(tr("STR_LEVEL_SHORT").arg(camera.getViewLevel()));
		_timerAnimate = new Timer(125);
		_timerAnimate.onTimer((StateHandler)animate);
		_timerAnimate.start();
		_miniMapView.draw();
	}

	/**
	 *
	 */
	~MiniMapState() =>
		_timerAnimate = null;

	/**
	 * Changes the currently displayed minimap level.
	 * @param action Pointer to an action.
	 */
	void btnLevelUpClick(Action _) =>
		_txtLevel.setText(tr("STR_LEVEL_SHORT").arg(_miniMapView.up()));

	/**
	 * Changes the currently displayed minimap level.
	 * @param action Pointer to an action.
	 */
	void btnLevelDownClick(Action _) =>
		_txtLevel.setText(tr("STR_LEVEL_SHORT").arg(_miniMapView.down()));

	/**
	 * Returns to the previous screen.
	 * @param action Pointer to an action.
	 */
	internal void btnOkClick(Action _)
	{
		if (Options.maximizeInfoScreens)
		{
			Screen.updateScale(Options.battlescapeScale, ref Options.baseXBattlescape, ref Options.baseYBattlescape, true);
			_game.getScreen().resetDisplay(false);
		}
		_game.popState();
	}

	/**
	 * Animation handler. Updates the minimap view animation.
	 */
	void animate() =>
		_miniMapView.animate();

	/**
	 * Handles timers.
	 */
	protected override void think()
	{
		base.think();
		_timerAnimate.think(this, null);
	}

	/**
	 * Handles mouse-wheeling.
	 * @param action Pointer to an action.
	 */
	protected override void handle(Action action)
	{
		base.handle(action);
		if (action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
		{
			if (action.getDetails().wheel.y > 0) //button.button == SDL_BUTTON_WHEELUP
			{
				btnLevelUpClick(action);
			}
			else if (action.getDetails().wheel.y < 0) //button.button == SDL_BUTTON_WHEELDOWN
			{
				btnLevelDownClick(action);
			}
		}
	}
}
