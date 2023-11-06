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

namespace SharpXcom.Menu;

/**
 * Loads a saved game, with an optional message.
 */
internal class LoadGameState : State
{
	int _firstRun;
	OptionsOrigin _origin;
	string _filename;
	Text _txtStatus;

	/**
	 * Initializes all the elements in the Load Game screen.
	 * @param game Pointer to the core game.
	 * @param origin Game section that originated this state.
	 * @param filename Name of the save file without extension.
	 * @param palette Parent state palette.
	 */
	internal LoadGameState(OptionsOrigin origin, string filename, SDL_Color[] palette)
	{
		_firstRun = 0;
		_origin = origin;
		_filename = filename;

		buildUi(palette);
	}

	/**
	 * Initializes all the elements in the Load Game screen.
	 * @param game Pointer to the core game.
	 * @param origin Game section that originated this state.
	 * @param type Type of auto-load being used.
	 * @param palette Parent state palette.
	 */
	internal LoadGameState(OptionsOrigin origin, SaveType type, SDL_Color[] palette)
	{
		_firstRun = 0;
		_origin = origin;

		switch (type)
		{
			case SaveType.SAVE_QUICK:
				_filename = SavedGame.QUICKSAVE;
				break;
			case SaveType.SAVE_AUTO_GEOSCAPE:
				_filename = SavedGame.AUTOSAVE_GEOSCAPE;
				break;
			case SaveType.SAVE_AUTO_BATTLESCAPE:
				_filename = SavedGame.AUTOSAVE_BATTLESCAPE;
				break;
			default:
				// can't auto-load ironman games
				break;
		}

		buildUi(palette);
	}

	/**
	 *
	 */
	~LoadGameState() { }

	/**
	 * Builds the interface.
	 * @param palette Parent state palette.
	 */
	void buildUi(SDL_Color[] palette)
	{
		_screen = false;

		// Create objects
		_txtStatus = new Text(320, 17, 0, 92);

		// Set palette
		setPalette(palette);

		if (_origin == OptionsOrigin.OPT_BATTLESCAPE)
		{
			add(_txtStatus, "textLoad", "battlescape");
			_txtStatus.setHighContrast(true);
			if (_game.getSavedGame().getSavedBattle().getAmbientSound() != -1)
			{
				_game.getMod().getSoundByDepth(0, (uint)_game.getSavedGame().getSavedBattle().getAmbientSound()).stopLoop();
			}
		}
		else
		{
			add(_txtStatus, "textLoad", "geoscape");
		}

		centerAllSurfaces();

		// Set up objects
		_txtStatus.setBig();
		_txtStatus.setAlign(TextHAlign.ALIGN_CENTER);
		_txtStatus.setText(tr("STR_LOADING_GAME"));
	}

	/**
	 * Ignore quick loads without a save available.
	 */
	protected override void init()
	{
		base.init();
		if (_filename == SavedGame.QUICKSAVE && !CrossPlatform.fileExists(Options.getMasterUserFolder() + _filename))
		{
			_game.popState();
			return;
		}
	}

	/**
	 * Loads the specified save.
	 */
	protected override void think()
	{
		base.think();
		// Make sure it gets drawn properly
		if (_firstRun < 10)
		{
			_firstRun++;
		}
		else
		{
			_game.popState();

			// Load the game
			SavedGame s = new SavedGame();
			try
			{
				s.load(_filename, _game.getMod());
				_game.setSavedGame(s);
				if (_game.getSavedGame().getEnding() != GameEnding.END_NONE)
				{
					Options.baseXResolution = Screen.ORIGINAL_WIDTH;
					Options.baseYResolution = Screen.ORIGINAL_HEIGHT;
					_game.getScreen().resetDisplay(false);
					_game.setState(new StatisticsState());
				}
				else
				{
					Options.baseXResolution = Options.baseXGeoscape;
					Options.baseYResolution = Options.baseYGeoscape;
					_game.getScreen().resetDisplay(false);
					_game.setState(new GeoscapeState());
					if (_game.getSavedGame().getSavedBattle() != null)
					{
						_game.getSavedGame().getSavedBattle().loadMapResources(_game.getMod());
						Options.baseXResolution = Options.baseXBattlescape;
						Options.baseYResolution = Options.baseYBattlescape;
						_game.getScreen().resetDisplay(false);
						BattlescapeState bs = new BattlescapeState();
						_game.pushState(bs);
						_game.getSavedGame().getSavedBattle().setBattleState(bs);
					}
				}
			}
			catch (YamlException e)
			{
				error(e.Message, s);
			}
			catch (Exception e)
			{
				error(e.Message, s);
			}
			CrossPlatform.flashWindow(_game.getScreen().getWindow());
		}
	}

	/**
	 * Pops up a window with an error message
	 * and cleans up afterwards.
	 * @param msg Error message.
	 * @param save Pending save.
	 */
	void error(string msg, SavedGame save)
	{
        Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} {msg}");
		string error = $"{tr("STR_LOAD_UNSUCCESSFUL")}{Unicode.TOK_NL_SMALL}{Unicode.convPathToUtf8(msg)}";
		if (_origin != OptionsOrigin.OPT_BATTLESCAPE)
			_game.pushState(new ErrorMessageState(error, _palette, (byte)_game.getMod().getInterface("errorMessages").getElement("geoscapeColor").color, "BACK01.SCR", _game.getMod().getInterface("errorMessages").getElement("geoscapePalette").color));
		else
			_game.pushState(new ErrorMessageState(error, _palette, (byte)_game.getMod().getInterface("errorMessages").getElement("battlescapeColor").color, "TAC00.SCR", _game.getMod().getInterface("errorMessages").getElement("battlescapePalette").color));

		if (_game.getSavedGame() == save)
			_game.setSavedGame(null);
		else
			save = null;
	}
}
