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

enum LoadingPhase { LOADING_STARTED, LOADING_FAILED, LOADING_SUCCESSFUL, LOADING_DONE };

/**
 * Initializes the game and loads all required content.
 */
internal class StartState : State
{
    int _anim;
    /* SDL_Thread */ Thread _thread;
    string _oldMaster;
    Font _font;
    Language _lang;
    Text _text, _cursor;
    Timer _timer;
    string _output;

    static LoadingPhase loading;
    static string error;

    /**
     * Initializes all the elements in the Loading screen.
     * @param game Pointer to the core game.
     */
    internal StartState()
    {
        _anim = 0;

        //updateScale() uses newDisplayWidth/Height and needs to be set ahead of time
        Options.newDisplayWidth = Options.displayWidth;
        Options.newDisplayHeight = Options.displayHeight;
        Screen.updateScale(Options.geoscapeScale, ref Options.baseXGeoscape, ref Options.baseYGeoscape, false);
        Screen.updateScale(Options.battlescapeScale, ref Options.baseXBattlescape, ref Options.baseYBattlescape, false);
        Options.baseXResolution = Options.displayWidth;
        Options.baseYResolution = Options.displayHeight;
        _game.getScreen().resetDisplay(false);

        // Create objects
        _thread = null;
        loading = LoadingPhase.LOADING_STARTED;
        error = string.Empty;
        _oldMaster = Options.getActiveMaster();

        _font = new Font();
        _font.loadTerminal();
        _lang = new Language();

        _text = new Text(Options.baseXResolution, Options.baseYResolution, 0, 0);
        _cursor = new Text(_font.getWidth(), _font.getHeight(), 0, 0);
        _timer = new Timer(150);

        setPalette(_font.getPalette(), 0, 2);

        add(_text);
        add(_cursor);

        // Set up objects
        _text.initText(_font, _font, _lang);
        _text.setColor(0);
        _text.setWordWrap(true);

        _cursor.initText(_font, _font, _lang);
        _cursor.setColor(0);
        _cursor.setText("_");

        _timer.onTimer((StateHandler)animate);
        _timer.start();

        // Hide UI
        _game.getCursor().setVisible(false);
        _game.getFpsCounter().setVisible(false);

        if (Options.reload)
        {
            addLine("Restarting...");
            addLine(string.Empty);
        }
        else
        {
            addLine(CrossPlatform.getDosPath() + ">openxcom");
        }
    }

    /**
     * Kill the thread in case the game is quit early.
     */
    ~StartState()
    {
        _thread?.Join(); //SDL_KillThread(_thread);
        _font = null;
        _timer = null;
        _lang = null;
    }

    /**
     * Adds a line of text to the terminal and moves
     * the cursor appropriately.
     * @param str Text line to add.
     */
    void addLine(string str)
    {
        _output = $"{_output}{Environment.NewLine}{str}";
	    _text.setText(_output);
	    int y = _text.getTextHeight() - _font.getHeight();
	    int x = _text.getTextWidth(y / _font.getHeight());
	    _cursor.setX(x);
	    _cursor.setY(y);
    }

    /**
     * Blinks the cursor and spreads out terminal output.
     */
    void animate()
    {
        _cursor.setVisible(!_cursor.getVisible());
        _anim++;

        if (loading == LoadingPhase.LOADING_STARTED)
        {
            string ss = $"Loading OpenXcom {OPENXCOM_VERSION_SHORT} {OPENXCOM_VERSION_GIT}...";
            if (Options.reload)
            {
                if (_anim == 2)
                    addLine(ss);
            }
            else
            {
                switch (_anim)
                {
                    case 1:
                        addLine("DOS/4GW Protected Mode Run-time  Version 1.9");
                        addLine("Copyright (c) Rational Systems, Inc. 1990-1993");
                        break;
                    case 6:
                        addLine(string.Empty);
                        addLine("OpenXcom initialisation");
                        break;
                    case 7:
                        addLine(string.Empty);
                        if (Options.mute)
                        {
                            addLine("No Sound Detected");
                        }
                        else
                        {
                            addLine("SoundBlaster Sound Effects");
                            if (Options.preferredMusic == MusicFormat.MUSIC_MIDI)
                                addLine("General MIDI Music");
                            else
                                addLine("SoundBlaster Music");
                            addLine("Base Port 220  Irq 7  Dma 1");
                        }
                        addLine(string.Empty);
                        break;
                    case 9:
                        addLine(ss);
                        break;
                }
            }
        }
    }

    /**
     * Reset and reload data.
     */
    internal override void init()
    {
        base.init();

        // Silence!
        Sound.stop();
        Music.stop();
        if (!Options.mute && Options.reload)
        {
            Mix_CloseAudio();
            _game.initAudio();
        }

        // Load the game data in a separate thread
        _thread = new Thread(new ParameterizedThreadStart(load)); //SDL_CreateThread(load, (void*)_game);
        if (_thread == null)
        {
            // If we can't create the thread, just load it as usual
            load(_game);
        }
    }

    /**
     * Loads game data and updates status accordingly.
     * @param game_ptr Pointer to the game.
     * @return Thread status, 0 = ok
     */
    void load(object game_ptr)
    {
        var game = (Game)game_ptr;
	    try
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Loading data...");
            Options.updateMods();
            game.loadMods();
            Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Data loaded successfully.");
            Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Loading language...");
            game.loadLanguages();
            Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Language loaded successfully.");
		    loading = LoadingPhase.LOADING_SUCCESSFUL;
	    }
	    catch (Exception e)
	    {
		    error = e.Message;
            Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} {error}");
		    loading = LoadingPhase.LOADING_FAILED;
	    }
    }

    /**
     * If the loading fails, it shows an error, otherwise moves on to the game.
     */
    internal override void think()
    {
        base.think();
        _timer.think(this, null);

        switch (loading)
        {
            case LoadingPhase.LOADING_FAILED:
                CrossPlatform.flashWindow(_game.getScreen().getWindow());
                addLine(string.Empty);
                addLine("ERROR: " + error);
                addLine(string.Empty);
                addLine("More details here: " + Logger.logFile());
                addLine("Make sure OpenXcom and any mods are installed correctly.");
                addLine(string.Empty);
                addLine("Press any key to continue.");
                loading = LoadingPhase.LOADING_DONE;
                break;
            case LoadingPhase.LOADING_SUCCESSFUL:
                CrossPlatform.flashWindow(_game.getScreen().getWindow());
                Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} OpenXcom started successfully!");
                _game.setState(new GoToMainMenuState());
                if (_oldMaster != Options.getActiveMaster() && Options.playIntro)
                {
                    _game.pushState(new CutsceneState("intro"));
                }
                if (Options.reload)
                {
                    Options.reload = false;
                }
                _game.getCursor().setVisible(true);
                _game.getFpsCounter().setVisible(Options.fpsCounter);
                break;
            default:
                break;
        }
    }

    /**
     * The game quits if the player presses any key when an error
     * message is on display.
     * @param action Pointer to an action.
     */
    internal override void handle(Action action)
    {
	    base.handle(action);
	    if (loading == LoadingPhase.LOADING_DONE)
	    {
		    if (action.getDetails().type == SDL_EventType.SDL_KEYDOWN)
		    {
			    _game.quit();
		    }
	    }
    }
}
