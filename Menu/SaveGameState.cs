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
 * Saves the current game, with an optional message.
 */
internal class SaveGameState : State
{
    int _firstRun;
    OptionsOrigin _origin;
    string _filename;
    SaveType _type;
    Text _txtStatus;

    /**
     * Initializes all the elements in the Save Game screen.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     * @param filename Name of the save file without extension.
     * @param palette Parent state palette.
     */
    internal SaveGameState(OptionsOrigin origin, string filename, SDL_Color[] palette)
    {
        _firstRun = 0;
        _origin = origin;
        _filename = filename;
        _type = SaveType.SAVE_DEFAULT;

        buildUi(palette);
    }

    /**
     * Initializes all the elements in the Save Game screen.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     * @param type Type of auto-save being used.
     * @param palette Parent state palette.
     */
    internal SaveGameState(OptionsOrigin origin, SaveType type, SDL_Color[] palette)
    {
        _firstRun = 0;
        _origin = origin;
        _type = type;

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
            case SaveType.SAVE_IRONMAN:
            case SaveType.SAVE_IRONMAN_END:
                _filename = CrossPlatform.sanitizeFilename(Unicode.convUtf8ToPath(_game.getSavedGame().getName())) + ".sav";
                break;
            default:
                break;
        }

        buildUi(palette);
    }

    /**
     *
     */
    ~SaveGameState() { }

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
        }
        else
        {
            add(_txtStatus, "textLoad", "geoscape");
        }

        centerAllSurfaces();

        // Set up objects
        _txtStatus.setBig();
        _txtStatus.setAlign(TextHAlign.ALIGN_CENTER);
        _txtStatus.setText(tr("STR_SAVING_GAME"));
    }

    /**
     * Saves the current save.
     */
    internal override void think()
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

		    switch (_type)
		    {
		        case SaveType.SAVE_DEFAULT:
			        // manual save, close the save screen
			        _game.popState();
			        if (!_game.getSavedGame().isIronman())
			        {
				        // and pause screen too
				        _game.popState();
			        }
			        break;
		        case SaveType.SAVE_QUICK:
		        case SaveType.SAVE_AUTO_GEOSCAPE:
		        case SaveType.SAVE_AUTO_BATTLESCAPE:
			        // automatic save, give it a default name
			        _game.getSavedGame().setName(_filename);
                    goto default;
		        default:
			        break;
		    }

		    // Save the game
		    try
		    {
			    _game.getSavedGame().save(_filename);
			    if (_type == SaveType.SAVE_IRONMAN_END)
			    {
				    Screen.updateScale(Options.geoscapeScale, ref Options.baseXGeoscape, ref Options.baseYGeoscape, true);
				    _game.getScreen().resetDisplay(false);

				    _game.setState(new MainMenuState());
				    _game.setSavedGame(null);
			    }
		    }
		    catch (YamlException e)
		    {
			    error(e.Message);
		    }
		    catch (Exception e)
		    {
			    error(e.Message);
		    }
	    }
    }

    /**
     * Pops up a window with an error message.
     * @param msg Error message.
     */
    void error(string msg)
    {
        Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} {msg}");
	    string error = $"{tr("STR_SAVE_UNSUCCESSFUL")}{Unicode.TOK_NL_SMALL}{Unicode.convPathToUtf8(msg)}";
	    if (_origin != OptionsOrigin.OPT_BATTLESCAPE)
		    _game.pushState(new ErrorMessageState(error, _palette, (byte)_game.getMod().getInterface("errorMessages").getElement("geoscapeColor").color, "BACK01.SCR", _game.getMod().getInterface("errorMessages").getElement("geoscapePalette").color));
	    else
		    _game.pushState(new ErrorMessageState(error, _palette, (byte)_game.getMod().getInterface("errorMessages").getElement("battlescapeColor").color, "TAC00.SCR", _game.getMod().getInterface("errorMessages").getElement("battlescapePalette").color));
    }
}
