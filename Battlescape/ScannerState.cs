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
 * The Scanner User Interface.
 */
internal class ScannerState : State
{
	BattleAction _action;
	InteractiveSurface _bg;
	Surface _scan;
	ScannerView _scannerView;
	Timer _timerAnimate;

	/**
	 * Initializes the Scanner State.
	 * @param game Pointer to the core game.
	 * @param action Pointer to an action.
	 */
	internal ScannerState(BattleAction action)
	{
		_action = action;

		if (Options.maximizeInfoScreens)
		{
			Options.baseXResolution = Screen.ORIGINAL_WIDTH;
			Options.baseYResolution = Screen.ORIGINAL_HEIGHT;
			_game.getScreen().resetDisplay(false);
		}
		_bg = new InteractiveSurface(320, 200);
		_scan = new Surface(320, 200);
		_scannerView = new ScannerView(152, 152, 56, 24, _game, _action.actor);

		if (_game.getScreen().getDY() > 50)
		{
			_screen = false;
		}

		// Set palette
		_game.getSavedGame().getSavedBattle().setPaletteByDepth(this);

		add(_scan);
		add(_scannerView);
		add(_bg);

		centerAllSurfaces();

		_game.getMod().getSurface("DETBORD.PCK").blit(_bg);
		_game.getMod().getSurface("DETBORD2.PCK").blit(_scan);
		_bg.onMouseClick(exitClick);
		_bg.onKeyboardPress(exitClick, Options.keyCancel);

		_timerAnimate = new Timer(125);
		_timerAnimate.onTimer((StateHandler)animate);
		_timerAnimate.start();

		update();
	}

	~ScannerState() =>
		_timerAnimate = null;

	/**
	 * Exits the screen.
	 * @param action Pointer to an action.
	 */
	void exitClick(Action _)
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
		_scannerView.animate();

	/**
	 * Updates scanner state.
	 */
	void update()
	{
		//_scannerView->draw();
	}

	/**
	 * Handles timers.
	 */
	internal override void think()
	{
		base.think();
		_timerAnimate.think(this, null);
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
			exitClick(action);
		}
	}
}
