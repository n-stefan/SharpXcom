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

namespace SharpXcom.Engine;

enum ApplicationState { RUNNING = 0, SLOWED = 1, PAUSED = 2 }

/**
 * The core of the game engine, manages the game's entire contents and structure.
 * Takes care of encapsulating all the core SDL commands, provides access to all
 * the game's resources and contains a stack state machine to handle all the
 * initializations, events and blits of each state, as well as transitions.
 */
internal class Game
{
    const double VOLUME_GRADIENT = 10.0;

    Screen _screen;
    Interface.Cursor _cursor;
    Language _lang;
    SavedGame _save;
    Mod.Mod _mod;
    bool _quit, _init;
    FpsCounter _fpsCounter;
    bool _mouseActive;
    uint _timeOfLastFrame;
    int _timeUntilNextFrame;
    List<State> _states, _deleted;
    SDL_Event _event;

    /**
     * Starts up all the SDL subsystems,
     * creates the display screen and sets up the cursor.
     * @param title Title of the game window.
     */
    internal Game(string title)
    {
        _screen = null;
        _cursor = null;
        _lang = null;
        _save = null;
        _mod = null;
        _quit = false;
        _init = false;
        _mouseActive = true;
        _timeUntilNextFrame = 0;

        Options.reload = false;
        Options.mute = false;

        // Initialize SDL
        if (SDL_Init(SDL_INIT_VIDEO) < 0)
        {
            throw new Exception(SDL_GetError());
        }
        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} SDL initialized successfully.");

        // Initialize SDL_mixer
        if (SDL_InitSubSystem(SDL_INIT_AUDIO) < 0)
        {
            Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} {SDL_GetError()}");
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} No sound device detected, audio disabled.");
            Options.mute = true;
        }
        else
        {
            initAudio();
        }

        // trap the mouse inside the window
        SDL_SetRelativeMouseMode(Options.captureMouse); //SDL_WM_GrabInput(Options.captureMouse);

        // Set the window icon
        CrossPlatform.setWindowIcon(/* IDI_ICON1, */ FileMap.getFilePath("openxcom.png"));

        // Set the window caption
        SDL_SetWindowTitle(IntPtr.Zero, title); //SDL_WM_SetCaption(title, 0);

        // Set up unicode
        //SDL_EnableUNICODE(1);
        Unicode.getUtf8Locale();

        // Create display
        _screen = new Screen();

        // Create cursor
        _cursor = new Interface.Cursor(9, 13);

        // Create invisible hardware cursor to workaround bug with absolute positioning pointing devices
        SDL_ShowCursor(SDL_ENABLE);
        IntPtr cursor = 0;
        SDL_SetCursor(SDL_CreateCursor(cursor, cursor, 1, 1, 0, 0));

        // Create fps counter
        _fpsCounter = new FpsCounter(15, 5, 0, 0);

        // Create blank language
        _lang = new Language();

        _timeOfLastFrame = 0;
    }

    /**
     * Deletes the display screen, cursor, states and shuts down all the SDL subsystems.
     */
    ~Game()
    {
        Sound.stop();
        Music.stop();

        _states.Clear();

        SDL_FreeCursor(SDL_GetCursor());

        _cursor = null;
        _lang = null;
        _save = null;
        _mod = null;
        _screen = null;
        _fpsCounter = null;

        Mix_CloseAudio();

        SDL_Quit();
    }

    /**
     * Returns the mouse cursor used by the game.
     * @return Pointer to the cursor.
     */
    internal Interface.Cursor getCursor() =>
	    _cursor;

    /**
     * Returns the display screen used by the game.
     * @return Pointer to the screen.
     */
    internal Screen getScreen() =>
	    _screen;

    /**
     * Returns the FpsCounter used by the game.
     * @return Pointer to the FpsCounter.
     */
    internal FpsCounter getFpsCounter() =>
	    _fpsCounter;

    /**
     * Returns the mod currently in use by the game.
     * @return Pointer to the mod.
     */
    internal Mod.Mod getMod() =>
	    _mod;

    /**
     * Returns the language currently in use by the game.
     * @return Pointer to the language.
     */
    internal Language getLanguage() =>
	    _lang;

    /**
     * Initializes the audio subsystem.
     */
    internal void initAudio()
    {
        ushort format = MIX_DEFAULT_FORMAT;
        if (Options.audioBitDepth == 8)
            format = AUDIO_S8;

        if (Options.audioSampleRate % 11025 != 0)
        {
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} Custom sample rate {Options.audioSampleRate}Hz, audio that doesn't match will be distorted!");
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} SDL_mixer only supports multiples of 11025Hz.");
        }
        int minChunk = Options.audioSampleRate / 11025 * 512;
        Options.audioChunkSize = Math.Max(minChunk, Options.audioChunkSize);

        if (Mix_OpenAudio(Options.audioSampleRate, format, MIX_DEFAULT_CHANNELS, Options.audioChunkSize) != 0)
        {
            Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} {Mix_GetError()}");
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} No sound device detected, audio disabled.");
            Options.mute = true;
        }
        else
        {
            Mix_AllocateChannels(16);
            // Set up UI channels
            Mix_ReserveChannels(4);
            Mix_GroupChannels(1, 2, 0);
            Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} SDL_mixer initialized successfully.");
            setVolume(Options.soundVolume, Options.musicVolume, Options.uiVolume);
        }
    }

    /**
     * Changes the audio volume of the music and
     * sound effect channels.
     * @param sound Sound volume, from 0 to MIX_MAX_VOLUME.
     * @param music Music volume, from 0 to MIX_MAX_VOLUME.
     * @param ui UI volume, from 0 to MIX_MAX_VOLUME.
     */
    void setVolume(int sound, int music, int ui)
    {
        if (!Options.mute)
        {
            if (sound >= 0)
            {
                sound = (int)(volumeExponent(sound) * SDL_MIX_MAXVOLUME);
                Mix_Volume(-1, sound);
                if (_save != null && _save.getSavedBattle() != null)
                {
                    Mix_Volume(3, (int)(sound * _save.getSavedBattle().getAmbientVolume()));
                }
                else
                {
                    // channel 3: reserved for ambient sound effect.
                    Mix_Volume(3, sound / 2);
                }
            }
            if (music >= 0)
            {
                music = (int)(volumeExponent(music) * SDL_MIX_MAXVOLUME);
                Mix_VolumeMusic(music);
            }
            if (ui >= 0)
            {
                ui = (int)(volumeExponent(ui) * SDL_MIX_MAXVOLUME);
                Mix_Volume(1, ui);
                Mix_Volume(2, ui);
            }
        }
    }

    double volumeExponent(int volume) =>
        (Math.Exp(Math.Log(VOLUME_GRADIENT + 1.0) * volume / SDL_MIX_MAXVOLUME) - 1.0) / VOLUME_GRADIENT;

    /**
     * Returns the saved game currently in use by the game.
     * @return Pointer to the saved game.
     */
    internal SavedGame getSavedGame() =>
	    _save;

    /**
     * Pops all the states currently in stack and pushes in the new state.
     * A shortcut for cleaning up all the old states when they're not necessary
     * like in one-way transitions.
     * @param state Pointer to the new state.
     */
    internal void setState(State state)
    {
        while (_states.Any())
        {
            popState();
        }
        pushState(state);
        _init = false;
    }

    /**
     * Pushes a new state into the top of the stack and initializes it.
     * The new state will be used once the next game cycle starts.
     * @param state Pointer to the new state.
     */
    void pushState(State state)
    {
        _states.Add(state);
        _init = false;
    }

    /**
     * Pops the last state from the top of the stack. Since states
     * can't actually be deleted mid-cycle, it's moved into a separate queue
     * which is cleared at the start of every cycle, so the transition
     * is seamless.
     */
    void popState()
    {
        _deleted.Add(_states.Last());
        _states.RemoveAt(_states.Count - 1);
        _init = false;
    }

    static ApplicationState[] kbFocusRun = { ApplicationState.RUNNING, ApplicationState.RUNNING, ApplicationState.SLOWED, ApplicationState.PAUSED };
    static ApplicationState[] stateRun = { ApplicationState.SLOWED, ApplicationState.PAUSED, ApplicationState.PAUSED, ApplicationState.PAUSED };
    /**
     * The state machine takes care of passing all the events from SDL to the
     * active state, running any code within and blitting all the states and
     * cursor to the screen. This is run indefinitely until the game quits.
     */
    internal void run()
    {
        var runningState = ApplicationState.RUNNING;
        // this will avoid processing SDL's resize event on startup, workaround for the heap allocation error it causes.
        bool startupEvent = Options.allowResize;
	    while (!_quit)
	    {
            // Clean up states
            _deleted.Clear();

		    // Initialize active state
		    if (!_init)
		    {
			    _init = true;
			    _states.Last().init();

                // Unpress buttons
                _states.Last().resetAll();

			    // Refresh mouse position
			    var ev = new SDL_Event();
                int x, y;
			    SDL_GetMouseState(out x, out y);
			    ev.type = SDL_EventType.SDL_MOUSEMOTION;
			    ev.motion.x = x;
                ev.motion.y = y;
			    Action action = new Action(ev, _screen.getXScale(), _screen.getYScale(), _screen.getCursorTopBlackBand(), _screen.getCursorLeftBlackBand());
                _states.Last().handle(action);
		    }

            // Process events
            while (SDL_PollEvent(out _event) == 1)
		    {
                if (CrossPlatform.isQuitShortcut(_event))
                    _event.type = SDL_EventType.SDL_QUIT;
			    switch (_event.type)
			    {
				    case SDL_EventType.SDL_QUIT:
                        quit();
                        break;
                    // An event other than SDL_APPMOUSEFOCUS change happened.
                    case SDL_EventType.SDL_WINDOWEVENT when _event.window.windowEvent != SDL_WindowEventID.SDL_WINDOWEVENT_ENTER && _event.window.windowEvent != SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE: //SDL_ACTIVEEVENT
                        // Game is minimized
                        if (_event.window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED)
                        {
                            runningState = stateRun[Options.pauseMode];
                            if (Options.backgroundMute)
                            {
                        	    setVolume(0, 0, 0);
                            }
                        }
                        // Game is not minimized but has no keyboard focus.
                        else if (_event.window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST)
                        {
                            runningState = kbFocusRun[Options.pauseMode];
                            if (Options.backgroundMute)
                            {
                        	    setVolume(0, 0, 0);
                            }
                        }
                        // Game has keyboard focus.
                        else if (_event.window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED)
                        {
                            runningState = ApplicationState.RUNNING;
                            if (Options.backgroundMute)
                            {
                        	    setVolume(Options.soundVolume, Options.musicVolume, Options.uiVolume);
                            }
                        }
                        else if (_event.window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED) //SDL_VIDEORESIZE
                        {
					        if (Options.allowResize)
					        {
						        if (!startupEvent)
						        {
                                    Options.newDisplayWidth = Options.displayWidth = Math.Max(Screen.ORIGINAL_WIDTH, _event.window.data1);
							        Options.newDisplayHeight = Options.displayHeight = Math.Max(Screen.ORIGINAL_HEIGHT, _event.window.data2);
							        int dX = 0, dY = 0;
							        Screen.updateScale(Options.battlescapeScale, ref Options.baseXBattlescape, ref Options.baseYBattlescape, false);
							        Screen.updateScale(Options.geoscapeScale, ref Options.baseXGeoscape, ref Options.baseYGeoscape, false);
							        foreach (var state in _states)
							        {
                                        state.resize(ref dX, ref dY);
							        }
							        _screen.resetDisplay();
						        }
						        else
						        {
                                    startupEvent = false;
						        }
					        }
                        }
					    break;
				    case SDL_EventType.SDL_MOUSEMOTION:
				    case SDL_EventType.SDL_MOUSEBUTTONDOWN:
				    case SDL_EventType.SDL_MOUSEBUTTONUP:
					    // Skip mouse events if they're disabled
					    if (!_mouseActive) continue;
					    // re-gain focus on mouse-over or keypress.
					    runningState = ApplicationState.RUNNING;
                        // Go on, feed the event to others
                        break;
				    default:
					    Action action = new Action(_event, _screen.getXScale(), _screen.getYScale(), _screen.getCursorTopBlackBand(), _screen.getCursorLeftBlackBand());
                        _screen.handle(action);
                        _cursor.handle(action);
					    _fpsCounter.handle(action);
					    if (action.getDetails().type == SDL_EventType.SDL_KEYDOWN)
					    {
						    // "ctrl-g" grab input
						    if (action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_g && (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0)
						    {
                                Options.captureMouse = Options.captureMouse == SDL_bool.SDL_TRUE ? SDL_bool.SDL_FALSE : SDL_bool.SDL_TRUE;
                                SDL_SetRelativeMouseMode(Options.captureMouse);
                            }
						    else if (Options.debug)
						    {
							    if (action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_t && (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0)
							    {
								    setState(new TestState());
							    }
							    // "ctrl-u" debug UI
							    else if (action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_u && (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0)
							    {
								    Options.debugUi = !Options.debugUi;
								    _states.Last().redrawText();
							    }
						    }
					    }
                        _states.Last().handle(action);
                        break;
			    }
			    if (!_init)
			    {
				    // States stack was changed, break the loop so new state
				    // can be initialized before processing new events
				    break;
			    }
		    }
		
		    // Process rendering
		    if (runningState != ApplicationState.PAUSED)
		    {
			    // Process logic
			    _states.Last().think();
                _fpsCounter.think();
			    if (Options.FPS > 0 && !(Options.useOpenGL && Options.vSyncForOpenGL))
			    {
                    // Update our FPS delay time based on the time of the last draw.
                    // SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED && _event.window.windowEvent != SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST
                    int fps = _event.window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED ? Options.FPS : Options.FPSInactive;

				    _timeUntilNextFrame = (int)((1000.0f / fps) - (SDL_GetTicks() - _timeOfLastFrame));
			    }
			    else
			    {
                    _timeUntilNextFrame = 0;
			    }

			    if (_init && _timeUntilNextFrame <= 0)
			    {
				    // make a note of when this frame update occurred.
				    _timeOfLastFrame = SDL_GetTicks();
				    _fpsCounter.addFrame();
				    _screen.clear();
                    var i = _states.Count - 1;
                    do
                    {
                        --i;
                    }
                    while (i >= 0 && !_states[i].isScreen());

				    for (; i < _states.Count; ++i)
				    {
					    _states[i].blit();
				    }
				    _fpsCounter.blit(_screen.getSurface());
				    _cursor.blit(_screen.getSurface());
                    _screen.flip();
			    }
		    }

		    // Save on CPU
		    switch (runningState)
		    {
			    case ApplicationState.RUNNING: 
				    SDL_Delay(1); //Save CPU from going 100%
				    break;
			    case ApplicationState.SLOWED: case ApplicationState.PAUSED:
				    SDL_Delay(100); break; //More slowing down.
		    }
	    }

        Options.save();
    }

    /**
     * Stops the state machine and the game is shut down.
     */
    void quit()
    {
        // Always save ironman
        if (_save != null && _save.isIronman() && !string.IsNullOrEmpty(_save.getName()))
        {
            string filename = CrossPlatform.sanitizeFilename(Unicode.convUtf8ToPath(_save.getName())) + ".sav";
            _save.save(filename);
        }
        _quit = true;
    }

    /**
     * Returns whether current state is *state
     * @param state The state to test against the stack state
     * @return Is state the current state?
     */
    internal bool isState(State state) =>
        _states.Any() && _states.Last() == state;

    /**
     * Loads the mods specified in the game options.
     */
    internal void loadMods()
    {
        Mod.Mod.resetGlobalStatics();
        _mod = null;
        _mod = new Mod.Mod();
        _mod.loadAll(FileMap.getRulesets());
    }

    /**
     * Loads the most appropriate languages
     * given current system and game options.
     */
    internal void loadLanguages()
    {
        string defaultLang = "en-US";
        string currentLang = defaultLang;

        string ss = $"common/Language/{defaultLang}.yml";
        string defaultPath = CrossPlatform.searchDataFile(ss);
        string path = defaultPath;

        // No language set, detect based on system
        if (string.IsNullOrEmpty(Options.language))
        {
            string locale = CrossPlatform.getLocale();
            string lang = locale.Substring(0, locale.IndexOf('-'));
            // Try to load full locale
            Unicode.replace(path, defaultLang, locale);
            if (Language.isSupported(locale) && CrossPlatform.fileExists(path))
            {
                currentLang = locale;
            }
            else
            {
                // Try to load language locale
                Unicode.replace(path, locale, lang);
                if (Language.isSupported(lang) && CrossPlatform.fileExists(path))
                {
                    currentLang = lang;
                }
                // Give up, use default
                else
                {
                    currentLang = defaultLang;
                }
            }
        }
        else
        {
            // Use options language
            Unicode.replace(path, defaultLang, Options.language);
            if (CrossPlatform.fileExists(path))
            {
                currentLang = Options.language;
            }
            // Language not found, use default
            else
            {
                currentLang = defaultLang;
            }
        }
        Options.language = currentLang;

        _lang = new Language();

        // Load default and current language
        string ssDefault, ssCurrent;
        ssDefault = $"/Language/{defaultLang}.yml";
        ssCurrent = $"/Language/{currentLang}.yml";

        _lang.loadFile(CrossPlatform.searchDataFile("common" + ssDefault));
        if (currentLang != defaultLang)
            _lang.loadFile(CrossPlatform.searchDataFile("common" + ssCurrent));

        // if this is a master but it has a master of its own, allow it to
        // chainload the "super" master, including its languages
        ModInfo modInfo = Options.getModInfo(Options.getActiveMaster());
        if (!string.IsNullOrEmpty(modInfo.getMaster()))
        {
            ModInfo masterInfo = Options.getModInfo(modInfo.getMaster());
            _lang.loadFile(masterInfo.getPath() + ssDefault);
            if (currentLang != defaultLang)
                _lang.loadFile(masterInfo.getPath() + ssCurrent);
        }

        List<ModInfo> activeMods = Options.getActiveMods();
        foreach (var activeMod in activeMods)
	    {
            _lang.loadFile(activeMod.getPath() + ssDefault);
            if (currentLang != defaultLang)
                _lang.loadFile(activeMod.getPath() + ssCurrent);
        }

        _lang.loadRule(_mod.getExtraStrings(), defaultLang);
        if (currentLang != defaultLang)
            _lang.loadRule(_mod.getExtraStrings(), currentLang);
    }
}
