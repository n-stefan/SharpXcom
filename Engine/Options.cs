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

/// Battlescape drag scrolling types.
enum ScrollType { SCROLL_NONE, SCROLL_TRIGGER, SCROLL_AUTO };

/// Keyboard input modes.
enum KeyboardType { KEYBOARD_OFF, KEYBOARD_ON, KEYBOARD_VIRTUAL };

/// Savegame sorting modes.
enum SaveSort { SORT_NAME_ASC, SORT_NAME_DESC, SORT_DATE_ASC, SORT_DATE_DESC };

/// Music format preferences.
enum MusicFormat { MUSIC_AUTO, MUSIC_FLAC, MUSIC_OGG, MUSIC_MP3, MUSIC_MOD, MUSIC_WAV, MUSIC_ADLIB, MUSIC_GM, MUSIC_MIDI };

/// Sound format preferences.
enum SoundFormat { SOUND_AUTO, SOUND_14, SOUND_10 };

/// Video format preferences.
enum VideoFormat { VIDEO_FMV, VIDEO_SLIDE };

/// Path preview modes (can be OR'd together).
enum PathPreview
{
    PATH_NONE = 0x00, // 0000 (must always be zero)
    PATH_ARROWS = 0x01, // 0001
    PATH_TU_COST = 0x02, // 0010
    PATH_FULL = 0x03  // 0011 (must always be all values combined)
};

enum ScaleType
{
    SCALE_ORIGINAL,
    SCALE_15X,
    SCALE_2X,
    SCALE_SCREEN_DIV_3,
    SCALE_SCREEN_DIV_2,
    SCALE_SCREEN
};

/**
 * Container for all the various global game options
 * and customizable settings.
 */
internal class Options
{
    // General options
    internal static int displayWidth, displayHeight, maxFrameSkip, baseXResolution, baseYResolution, baseXGeoscape, baseYGeoscape, baseXBattlescape, baseYBattlescape,
		soundVolume, musicVolume, uiVolume, audioSampleRate, audioBitDepth, audioChunkSize, pauseMode, windowedModePositionX, windowedModePositionY, FPS, FPSInactive,
		changeValueByMouseWheel, dragScrollTimeTolerance, dragScrollPixelTolerance, mousewheelSpeed, autosaveFrequency;
    internal static bool fullscreen, asyncBlit, playIntro, useScaleFilter, useHQXFilter, useXBRZFilter, useOpenGL, checkOpenGLErrors, vSyncForOpenGL, useOpenGLSmoothing,
		autosave, allowResize, borderless, debug, debugUi, fpsCounter, newSeedOnLoad, keepAspectRatio, nonSquarePixelRatio,
		cursorInBlackBandsInFullscreen, cursorInBlackBandsInWindow, cursorInBlackBandsInBorderlessWindow, maximizeInfoScreens, musicAlwaysLoop, StereoSound, verboseLogging, soldierDiaries, touchEnabled,
		rootWindowedMode, lazyLoadResources, backgroundMute;
    internal static string language, useOpenGLShader;
    internal static KeyboardType keyboardMode;
    internal static SaveSort saveOrder;
    internal static MusicFormat preferredMusic;
    internal static SoundFormat preferredSound;
    internal static VideoFormat preferredVideo;
	internal static SDL_bool /* SDL_GrabMode */ captureMouse;
	internal static TextWrapping wordwrap;
    internal static SDL_Keycode keyOk, keyCancel, keyScreenshot, keyFps, keyQuickLoad, keyQuickSave;

    // Geoscape options
    internal static int geoClockSpeed, dogfightSpeed, geoScrollSpeed, geoDragScrollButton, geoscapeScale;
    internal static bool includePrimeStateInSavedLayout, anytimePsiTraining, weaponSelfDestruction, retainCorpses, craftLaunchAlways, globeSeasons, globeDetail, globeRadarLines, globeFlightPaths, globeAllRadarsOnBaseBuild,
		storageLimitsEnforced, canSellLiveAliens, canTransferCraftsWhileAirborne, customInitialBase, aggressiveRetaliation, geoDragScrollInvert,
		allowBuildingQueue, showFundsOnGeoscape, psiStrengthEval, allowPsiStrengthImprovement, fieldPromotions, meetingPoint;
    internal static SDL_Keycode keyGeoLeft, keyGeoRight, keyGeoUp, keyGeoDown, keyGeoZoomIn, keyGeoZoomOut, keyGeoSpeed1, keyGeoSpeed2, keyGeoSpeed3, keyGeoSpeed4, keyGeoSpeed5, keyGeoSpeed6,
		keyGeoIntercept, keyGeoBases, keyGeoGraphs, keyGeoUfopedia, keyGeoOptions, keyGeoFunding, keyGeoToggleDetail, keyGeoToggleRadar,
		keyBaseSelect1, keyBaseSelect2, keyBaseSelect3, keyBaseSelect4, keyBaseSelect5, keyBaseSelect6, keyBaseSelect7, keyBaseSelect8;

    // Battlescape options
    internal static ScrollType battleEdgeScroll;
    internal static PathPreview battleNewPreviewPath;
    internal static int battleScrollSpeed, battleDragScrollButton, battleFireSpeed, battleXcomSpeed, battleAlienSpeed, battleExplosionHeight, battlescapeScale;
    internal static bool traceAI, sneakyAI, battleInstantGrenade, battleNotifyDeath, battleTooltips, battleHairBleach, battleAutoEnd,
		strafe, forceFire, showMoreStatsInInventoryView, allowPsionicCapture, skipNextTurnScreen, disableAutoEquip, battleDragScrollInvert,
		battleUFOExtenderAccuracy, battleConfirmFireMode, battleSmoothCamera, noAlienPanicMessages, alienBleeding;
    internal static SDL_Keycode keyBattleLeft, keyBattleRight, keyBattleUp, keyBattleDown, keyBattleLevelUp, keyBattleLevelDown, keyBattleCenterUnit, keyBattlePrevUnit, keyBattleNextUnit, keyBattleDeselectUnit,
		keyBattleUseLeftHand, keyBattleUseRightHand, keyBattleInventory, keyBattleMap, keyBattleOptions, keyBattleEndTurn, keyBattleAbort, keyBattleStats, keyBattleKneel,
		keyBattleReserveKneel, keyBattleReload, keyBattlePersonalLighting, keyBattleReserveNone, keyBattleReserveSnap, keyBattleReserveAimed, keyBattleReserveAuto,
		keyBattleCenterEnemy1, keyBattleCenterEnemy2, keyBattleCenterEnemy3, keyBattleCenterEnemy4, keyBattleCenterEnemy5, keyBattleCenterEnemy6, keyBattleCenterEnemy7, keyBattleCenterEnemy8,
		keyBattleCenterEnemy9, keyBattleCenterEnemy10, keyBattleVoxelView, keyBattleZeroTUs, keyInvCreateTemplate, keyInvApplyTemplate, keyInvClear, keyInvAutoEquip;

	// Flags and other stuff that don't need OptionInfo's.
	internal static bool mute, reload, newOpenGL, newScaleFilter, newHQXFilter, newXBRZFilter, newRootWindowedMode, newFullscreen, newAllowResize, newBorderless;
    internal static int newDisplayWidth, newDisplayHeight, newBattlescapeScale, newGeoscapeScale, newWindowedModePositionX, newWindowedModePositionY;
    internal static string newOpenGLShader;
    internal static List<KeyValuePair<string, bool>> mods; // ordered list of available mods (lowest priority to highest) and whether they are active
    internal static SoundFormat currentSound;

    static string _masterMod;
    static string _dataFolder;
    static List<string> _dataList;
    static string _userFolder;
    static string _configFolder;
    static Dictionary<string, string> _commandLine;
    static List<OptionInfo> _info;
    static Dictionary<string, ModInfo> _modInfos;

    /**
     * Gets the currently active master mod.
     * @return Mod id.
     */
    internal static string getActiveMaster() =>
        _masterMod;

    /**
     * Returns the game's User folder where
     * saves are stored in.
     * @return Full path to User folder.
     */
    internal static string getUserFolder() =>
        _userFolder;

    /**
     * Handles the initialization of setting up default options
     * and finding and loading any existing ones.
     * @param argc Number of arguments.
     * @param argv Array of argument strings.
     * @return Do we start the game?
     */
    internal static bool init(string[] args)
    {
        if (showHelp(args))
            return false;
        create();
        resetDefault(true);
        loadArgs(args);
        setFolders();
        _setDefaultMods();
        updateOptions();

        string s = getUserFolder();
        s += "openxcom.log";
        Logger._logFile = s;
        using var file = new FileStream(Logger.logFile(), FileMode.Create, FileAccess.Write);
        if (file != null)
        {
            file.Flush();
            file.Close();
        }
        else
        {
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} Couldn't create log file, switching to stderr");
        }

        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} OpenXcom Version: {OPENXCOM_VERSION_SHORT}{OPENXCOM_VERSION_GIT}");
        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Platform: {Environment.OSVersion}");
        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Data folder is: {_dataFolder}");
        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Data search is: ");
        foreach (var item in _dataList)
        {
            Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} - {item}");
        }
        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} User folder is: {_userFolder}");
        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Config folder is: {_configFolder}");
        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Options loaded successfully.");

        return true;
    }

    /*
     * Displays command-line help when appropriate.
     * @param argc Number of arguments.
     * @param argv Array of argument strings.
     */
    static bool showHelp(string[] args)
    {
        var help = new StringBuilder();
        help.AppendLine($"OpenXcom v{OPENXCOM_VERSION_SHORT}");
        help.AppendLine($"Usage: openxcom [OPTION]...{Environment.NewLine}");
        help.AppendLine("-data PATH");
        help.AppendLine($"        use PATH as the default Data Folder instead of auto-detecting{Environment.NewLine}");
        help.AppendLine("-user PATH");
        help.AppendLine($"        use PATH as the default User Folder instead of auto-detecting{Environment.NewLine}");
        help.AppendLine("-cfg PATH  or  -config PATH");
        help.AppendLine($"        use PATH as the default Config Folder instead of auto-detecting{Environment.NewLine}");
        help.AppendLine("-master MOD");
        help.AppendLine($"        set MOD to the current master mod (eg. -master xcom2){Environment.NewLine}");
        help.AppendLine("-KEY VALUE");
        help.AppendLine($"        override option KEY with VALUE (eg. -displayWidth 640){Environment.NewLine}");
        help.AppendLine("-help");
        help.AppendLine("-?");
        help.AppendLine("        show command-line help");
        for (int i = 1; i < args.Length; ++i)
        {
            string arg = args[i];
            if ((arg[0] == '-' || arg[0] == '/') && arg.Length > 1)
            {
                string argname;
                if (arg[1] == '-' && arg.Length > 2)
                    argname = arg.Substring(2, arg.Length - 1);
                else
                    argname = arg.Substring(1, arg.Length - 1);
                argname = argname.ToLower();
                if (argname == "help" || argname == "?")
                {
                    Console.WriteLine(help.ToString());
                    return true;
                }
            }
        }
        return false;
    }

    /**
     * Resets the options back to their defaults.
     * @param includeMods Reset mods to default as well.
     */
    static void resetDefault(bool includeMods)
    {
        foreach (var item in _info)
        {
            item.reset();
        }
        backupDisplay();

        if (includeMods)
        {
            mods.Clear();
            if (_dataList.Any())
            {
                _setDefaultMods();
            }
        }
    }

    /**
     * Saves display settings temporarily to be able
     * to revert to old ones.
     */
    static void backupDisplay()
    {
        newDisplayWidth = displayWidth;
        newDisplayHeight = displayHeight;
        newBattlescapeScale = battlescapeScale;
        newGeoscapeScale = geoscapeScale;
        newOpenGL = useOpenGL;
        newScaleFilter = useScaleFilter;
        newHQXFilter = useHQXFilter;
        newOpenGLShader = useOpenGLShader;
        newXBRZFilter = useXBRZFilter;
        newRootWindowedMode = rootWindowedMode;
        newWindowedModePositionX = windowedModePositionX;
        newWindowedModePositionY = windowedModePositionY;
        newFullscreen = fullscreen;
        newAllowResize = allowResize;
        newBorderless = borderless;
    }

    static void _setDefaultMods()
    {
        bool haveUfo = _ufoIsInstalled();
        if (haveUfo)
        {
            mods.Add(KeyValuePair.Create("xcom1", true));
        }

        if (_tftdIsInstalled())
        {
            mods.Add(KeyValuePair.Create("xcom2", !haveUfo));
        }
    }

    static bool _ufoIsInstalled() =>
        _gameIsInstalled("UFO");

    static bool _tftdIsInstalled() =>
        _gameIsInstalled("TFTD");

    // we can get fancier with these detection routines, but for now just look for
    // *something* in the data folders.  case sensitivity can make actually verifying
    // that the *correct* files are there complex.
    static bool _gameIsInstalled(string gameName)
    {
	    // look for game data in either the data or user directories
	    string dataGameFolder = CrossPlatform.searchDataFolder(gameName);
        string userGameFolder = _userFolder + gameName;
        return (CrossPlatform.folderExists(dataGameFolder) && CrossPlatform.getFolderContents(dataGameFolder).Count >= 8)
            || (CrossPlatform.folderExists(userGameFolder) && CrossPlatform.getFolderContents(userGameFolder).Count >= 8);
    }

    /**
     * Returns the game's current Data folder where resources
     * and X-Com files are loaded from.
     * @return Full path to Data folder.
     */
    internal static string getDataFolder() =>
        _dataFolder;

    /**
     * Changes the game's current Data folder where resources
     * and X-Com files are loaded from.
     * @param folder Full path to Data folder.
     */
    internal static void setDataFolder(string folder) =>
        _dataFolder = folder;

    /**
     * Returns the game's list of possible Data folders.
     * @return List of Data paths.
     */
    internal static List<string> getDataList() =>
        _dataList;

    /**
     * Loads options from a set of command line arguments,
     * in the format "-option value".
     * @param argc Number of arguments.
     * @param argv Array of argument strings.
     */
    static void loadArgs(string[] args)
    {
        for (int i = 1; i < args.Length; ++i)
        {
            string arg = args[i];
            if ((arg[0] == '-' || arg[0] == '/') && arg.Length > 1)
            {
                string argname;
                if (arg[1] == '-' && arg.Length > 2)
                    argname = arg.Substring(2, arg.Length - 1);
                else
                    argname = arg.Substring(1, arg.Length - 1);
                argname = argname.ToLower();
                if (args.Length > i + 1)
                {
                    ++i; // we'll be consuming the next argument too

                    if (argname == "data")
                    {
                        _dataFolder = CrossPlatform.endPath(args[i]);
                    }
                    else if (argname == "user")
                    {
                        _userFolder = CrossPlatform.endPath(args[i]);
                    }
                    else if (argname == "cfg" || argname == "config")
                    {
                        _configFolder = CrossPlatform.endPath(args[i]);
                    }
                    else if (argname == "master")
                    {
                        _masterMod = args[i];
                    }
                    else
                    {
                        //save this command line option for now, we will apply it later
                        _commandLine[argname] = args[i];
                    }
                }
                else
                {
                    Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} Unknown option: {argname}");
                }
            }
        }
    }

    /**
     * Sets up the game's Data folder where the data files
     * are loaded from and the User folder and Config
     * folder where settings and saves are stored in.
     */
    static void setFolders()
    {
        _dataList = CrossPlatform.findDataFolders();
        if (!string.IsNullOrEmpty(_dataFolder))
        {
            _dataList.Insert(0, _dataFolder);
        }
        if (string.IsNullOrEmpty(_userFolder))
        {
            List<string> user = CrossPlatform.findUserFolders();

            if (string.IsNullOrEmpty(_configFolder))
            {
                _configFolder = CrossPlatform.findConfigFolder();
            }

            // Look for an existing user folder
            for (var i = user.Count - 1; i >= 0; i--)
            {
                if (CrossPlatform.folderExists(user[i]))
                {
                    _userFolder = user[i];
                    break;
                }
            }

            // Set up folders
            if (string.IsNullOrEmpty(_userFolder))
            {
                foreach (var item in user)
                {
                    if (CrossPlatform.createFolder(item))
                    {
                        _userFolder = item;
                        break;
                    }
                }
            }
        }
        if (!string.IsNullOrEmpty(_userFolder))
        {
            // create mod folder if it doesn't already exist
            CrossPlatform.createFolder(_userFolder + "mods");
        }

        if (string.IsNullOrEmpty(_configFolder))
        {
            _configFolder = _userFolder;
        }
    }

    /**
     * Updates the game's options with those in the configuration
     * file, if it exists yet, and any supplied on the command line.
     */
    static void updateOptions()
    {
        // Load existing options
        if (CrossPlatform.folderExists(_configFolder))
        {
            if (CrossPlatform.fileExists(_configFolder + "options.cfg"))
            {
                load();
            }
            else
            {
                save();
            }
        }
        // Create config folder and save options
        else
        {
            CrossPlatform.createFolder(_configFolder);
            save();
        }

        // now apply options set on the command line, overriding defaults and those loaded from config file
        //if (!_commandLine.empty())
        foreach (var item in _info)
        {
            item.load(_commandLine);
        }
    }

    /**
     * Sets up the options by creating their OptionInfo metadata.
     */
    static void create()
    {
        _info.Add(new OptionInfo("displayWidth", displayWidth, Screen.ORIGINAL_WIDTH * 2));
        _info.Add(new OptionInfo("displayHeight", displayHeight, Screen.ORIGINAL_HEIGHT * 2));
        _info.Add(new OptionInfo("fullscreen", fullscreen, false));
        _info.Add(new OptionInfo("asyncBlit", asyncBlit, true));
        _info.Add(new OptionInfo("keyboardMode", (int)keyboardMode, (int)KeyboardType.KEYBOARD_ON));

        _info.Add(new OptionInfo("maxFrameSkip", maxFrameSkip, 0));
        _info.Add(new OptionInfo("traceAI", traceAI, false));
        _info.Add(new OptionInfo("verboseLogging", verboseLogging, false));
        _info.Add(new OptionInfo("StereoSound", StereoSound, true));
        //_info.Add(new OptionInfo("baseXResolution", baseXResolution, Screen.ORIGINAL_WIDTH));
        //_info.Add(new OptionInfo("baseYResolution", baseYResolution, Screen.ORIGINAL_HEIGHT));
        //_info.Add(new OptionInfo("baseXGeoscape", baseXGeoscape, Screen.ORIGINAL_WIDTH));
        //_info.Add(new OptionInfo("baseYGeoscape", baseYGeoscape, Screen.ORIGINAL_HEIGHT));
        //_info.Add(new OptionInfo("baseXBattlescape", baseXBattlescape, Screen.ORIGINAL_WIDTH));
        //_info.Add(new OptionInfo("baseYBattlescape", baseYBattlescape, Screen.ORIGINAL_HEIGHT));
        _info.Add(new OptionInfo("geoscapeScale", geoscapeScale, 0));
        _info.Add(new OptionInfo("battlescapeScale", battlescapeScale, 0));
        _info.Add(new OptionInfo("useScaleFilter", useScaleFilter, false));
        _info.Add(new OptionInfo("useHQXFilter", useHQXFilter, false));
        _info.Add(new OptionInfo("useXBRZFilter", useXBRZFilter, false));
        _info.Add(new OptionInfo("useOpenGL", useOpenGL, false));
        _info.Add(new OptionInfo("checkOpenGLErrors", checkOpenGLErrors, false));
        _info.Add(new OptionInfo("useOpenGLShader", useOpenGLShader, "Shaders/Raw.OpenGL.shader"));
        _info.Add(new OptionInfo("vSyncForOpenGL", vSyncForOpenGL, true));
        _info.Add(new OptionInfo("useOpenGLSmoothing", useOpenGLSmoothing, true));
        _info.Add(new OptionInfo("debug", debug, false));
        _info.Add(new OptionInfo("debugUi", debugUi, false));
        _info.Add(new OptionInfo("soundVolume", soundVolume, 2 * (MIX_MAX_VOLUME / 3)));
        _info.Add(new OptionInfo("musicVolume", musicVolume, 2 * (MIX_MAX_VOLUME / 3)));
        _info.Add(new OptionInfo("uiVolume", uiVolume, MIX_MAX_VOLUME / 3));
        _info.Add(new OptionInfo("language", language, string.Empty));
        _info.Add(new OptionInfo("battleScrollSpeed", battleScrollSpeed, 8));
        _info.Add(new OptionInfo("battleEdgeScroll", (int)battleEdgeScroll, (int)ScrollType.SCROLL_AUTO));
        _info.Add(new OptionInfo("battleDragScrollButton", battleDragScrollButton, (int)SDL_BUTTON_MIDDLE));
        _info.Add(new OptionInfo("dragScrollTimeTolerance", dragScrollTimeTolerance, 300)); // miliSecond
        _info.Add(new OptionInfo("dragScrollPixelTolerance", dragScrollPixelTolerance, 10)); // count of pixels
        _info.Add(new OptionInfo("battleFireSpeed", battleFireSpeed, 6));
        _info.Add(new OptionInfo("battleXcomSpeed", battleXcomSpeed, 30));
        _info.Add(new OptionInfo("battleAlienSpeed", battleAlienSpeed, 30));
        _info.Add(new OptionInfo("battleNewPreviewPath", (int)battleNewPreviewPath, (int)PathPreview.PATH_NONE)); // requires double-click to confirm moves
        _info.Add(new OptionInfo("fpsCounter", fpsCounter, false));
        _info.Add(new OptionInfo("globeDetail", globeDetail, true));
        _info.Add(new OptionInfo("globeRadarLines", globeRadarLines, true));
        _info.Add(new OptionInfo("globeFlightPaths", globeFlightPaths, true));
        _info.Add(new OptionInfo("globeAllRadarsOnBaseBuild", globeAllRadarsOnBaseBuild, true));
        _info.Add(new OptionInfo("audioSampleRate", audioSampleRate, 22050));
        _info.Add(new OptionInfo("audioBitDepth", audioBitDepth, 16));
        _info.Add(new OptionInfo("audioChunkSize", audioChunkSize, 1024));
        _info.Add(new OptionInfo("pauseMode", pauseMode, 0));
        _info.Add(new OptionInfo("battleNotifyDeath", battleNotifyDeath, false));
        _info.Add(new OptionInfo("showFundsOnGeoscape", showFundsOnGeoscape, false));
        _info.Add(new OptionInfo("allowResize", allowResize, false));
        _info.Add(new OptionInfo("windowedModePositionX", windowedModePositionX, 0));
        _info.Add(new OptionInfo("windowedModePositionY", windowedModePositionY, 0));
        _info.Add(new OptionInfo("borderless", borderless, false));
        _info.Add(new OptionInfo("captureMouse", captureMouse == SDL_bool.SDL_TRUE, false));
        _info.Add(new OptionInfo("battleTooltips", battleTooltips, true));
        _info.Add(new OptionInfo("keepAspectRatio", keepAspectRatio, true));
        _info.Add(new OptionInfo("nonSquarePixelRatio", nonSquarePixelRatio, false));
        _info.Add(new OptionInfo("cursorInBlackBandsInFullscreen", cursorInBlackBandsInFullscreen, false));
        _info.Add(new OptionInfo("cursorInBlackBandsInWindow", cursorInBlackBandsInWindow, true));
        _info.Add(new OptionInfo("cursorInBlackBandsInBorderlessWindow", cursorInBlackBandsInBorderlessWindow, false));
        _info.Add(new OptionInfo("saveOrder", (int)saveOrder, (int)SaveSort.SORT_DATE_DESC));
        _info.Add(new OptionInfo("geoClockSpeed", geoClockSpeed, 80));
        _info.Add(new OptionInfo("dogfightSpeed", dogfightSpeed, 30));
        _info.Add(new OptionInfo("geoScrollSpeed", geoScrollSpeed, 20));
        _info.Add(new OptionInfo("geoDragScrollButton", geoDragScrollButton, (int)SDL_BUTTON_MIDDLE));
        _info.Add(new OptionInfo("preferredMusic", (int)preferredMusic, (int)MusicFormat.MUSIC_AUTO));
        _info.Add(new OptionInfo("preferredSound", (int)preferredSound, (int)SoundFormat.SOUND_AUTO));
        _info.Add(new OptionInfo("preferredVideo", (int)preferredVideo, (int)VideoFormat.VIDEO_FMV));
        _info.Add(new OptionInfo("wordwrap", (int)wordwrap, (int)TextWrapping.WRAP_AUTO));
        _info.Add(new OptionInfo("musicAlwaysLoop", musicAlwaysLoop, false));
        _info.Add(new OptionInfo("touchEnabled", touchEnabled, false));
        _info.Add(new OptionInfo("rootWindowedMode", rootWindowedMode, false));
        _info.Add(new OptionInfo("lazyLoadResources", lazyLoadResources, true));
        _info.Add(new OptionInfo("backgroundMute", backgroundMute, false));

        // advanced options
        _info.Add(new OptionInfo("playIntro", playIntro, true, "STR_PLAYINTRO", "STR_GENERAL"));
        _info.Add(new OptionInfo("autosave", autosave, true, "STR_AUTOSAVE", "STR_GENERAL"));
        _info.Add(new OptionInfo("autosaveFrequency", autosaveFrequency, 5, "STR_AUTOSAVE_FREQUENCY", "STR_GENERAL"));
        _info.Add(new OptionInfo("newSeedOnLoad", newSeedOnLoad, false, "STR_NEWSEEDONLOAD", "STR_GENERAL"));
        _info.Add(new OptionInfo("mousewheelSpeed", mousewheelSpeed, 3, "STR_MOUSEWHEEL_SPEED", "STR_GENERAL"));
        _info.Add(new OptionInfo("changeValueByMouseWheel", changeValueByMouseWheel, 0, "STR_CHANGEVALUEBYMOUSEWHEEL", "STR_GENERAL"));
        _info.Add(new OptionInfo("soldierDiaries", soldierDiaries, true));

        _info.Add(new OptionInfo("maximizeInfoScreens", maximizeInfoScreens, false, "STR_MAXIMIZE_INFO_SCREENS", "STR_GENERAL"));

        _info.Add(new OptionInfo("geoDragScrollInvert", geoDragScrollInvert, false, "STR_DRAGSCROLLINVERT", "STR_GEOSCAPE")); // true drags away from the cursor, false drags towards (like a grab)
        _info.Add(new OptionInfo("aggressiveRetaliation", aggressiveRetaliation, false, "STR_AGGRESSIVERETALIATION", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("customInitialBase", customInitialBase, false, "STR_CUSTOMINITIALBASE", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("allowBuildingQueue", allowBuildingQueue, false, "STR_ALLOWBUILDINGQUEUE", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("craftLaunchAlways", craftLaunchAlways, false, "STR_CRAFTLAUNCHALWAYS", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("storageLimitsEnforced", storageLimitsEnforced, false, "STR_STORAGELIMITSENFORCED", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("canSellLiveAliens", canSellLiveAliens, false, "STR_CANSELLLIVEALIENS", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("anytimePsiTraining", anytimePsiTraining, false, "STR_ANYTIMEPSITRAINING", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("globeSeasons", globeSeasons, false, "STR_GLOBESEASONS", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("psiStrengthEval", psiStrengthEval, false, "STR_PSISTRENGTHEVAL", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("canTransferCraftsWhileAirborne", canTransferCraftsWhileAirborne, false, "STR_CANTRANSFERCRAFTSWHILEAIRBORNE", "STR_GEOSCAPE")); // When the craft can reach the destination base with its fuel
        _info.Add(new OptionInfo("retainCorpses", retainCorpses, false, "STR_RETAINCORPSES", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("fieldPromotions", fieldPromotions, false, "STR_FIELDPROMOTIONS", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("meetingPoint", meetingPoint, false, "STR_MEETINGPOINT", "STR_GEOSCAPE"));

        _info.Add(new OptionInfo("battleDragScrollInvert", battleDragScrollInvert, false, "STR_DRAGSCROLLINVERT", "STR_BATTLESCAPE")); // true drags away from the cursor, false drags towards (like a grab)
        _info.Add(new OptionInfo("sneakyAI", sneakyAI, false, "STR_SNEAKYAI", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("battleUFOExtenderAccuracy", battleUFOExtenderAccuracy, false, "STR_BATTLEUFOEXTENDERACCURACY", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("showMoreStatsInInventoryView", showMoreStatsInInventoryView, false, "STR_SHOWMORESTATSININVENTORYVIEW", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("battleHairBleach", battleHairBleach, true, "STR_BATTLEHAIRBLEACH", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("battleInstantGrenade", battleInstantGrenade, false, "STR_BATTLEINSTANTGRENADE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("includePrimeStateInSavedLayout", includePrimeStateInSavedLayout, false, "STR_INCLUDE_PRIMESTATE_IN_SAVED_LAYOUT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("battleExplosionHeight", battleExplosionHeight, 0, "STR_BATTLEEXPLOSIONHEIGHT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("battleAutoEnd", battleAutoEnd, false, "STR_BATTLEAUTOEND", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("battleSmoothCamera", battleSmoothCamera, false, "STR_BATTLESMOOTHCAMERA", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("disableAutoEquip", disableAutoEquip, false, "STR_DISABLEAUTOEQUIP", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("battleConfirmFireMode", battleConfirmFireMode, false, "STR_BATTLECONFIRMFIREMODE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("weaponSelfDestruction", weaponSelfDestruction, false, "STR_WEAPONSELFDESTRUCTION", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("allowPsionicCapture", allowPsionicCapture, false, "STR_ALLOWPSIONICCAPTURE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("allowPsiStrengthImprovement", allowPsiStrengthImprovement, false, "STR_ALLOWPSISTRENGTHIMPROVEMENT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("strafe", strafe, false, "STR_STRAFE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("forceFire", forceFire, true, "STR_FORCE_FIRE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("skipNextTurnScreen", skipNextTurnScreen, false, "STR_SKIPNEXTTURNSCREEN", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("noAlienPanicMessages", noAlienPanicMessages, false, "STR_NOALIENPANICMESSAGES", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("alienBleeding", alienBleeding, false, "STR_ALIENBLEEDING", "STR_BATTLESCAPE"));

        // controls
        _info.Add(new OptionInfo("keyOk", keyOk, SDL_Keycode.SDLK_RETURN, "STR_OK", "STR_GENERAL"));
        _info.Add(new OptionInfo("keyCancel", keyCancel, SDL_Keycode.SDLK_ESCAPE, "STR_CANCEL", "STR_GENERAL"));
        _info.Add(new OptionInfo("keyScreenshot", keyScreenshot, SDL_Keycode.SDLK_F12, "STR_SCREENSHOT", "STR_GENERAL"));
        _info.Add(new OptionInfo("keyFps", keyFps, SDL_Keycode.SDLK_F7, "STR_FPS_COUNTER", "STR_GENERAL"));
        _info.Add(new OptionInfo("keyQuickSave", keyQuickSave, SDL_Keycode.SDLK_F5, "STR_QUICK_SAVE", "STR_GENERAL"));
        _info.Add(new OptionInfo("keyQuickLoad", keyQuickLoad, SDL_Keycode.SDLK_F9, "STR_QUICK_LOAD", "STR_GENERAL"));
        _info.Add(new OptionInfo("keyGeoLeft", keyGeoLeft, SDL_Keycode.SDLK_LEFT, "STR_ROTATE_LEFT", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoRight", keyGeoRight, SDL_Keycode.SDLK_RIGHT, "STR_ROTATE_RIGHT", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoUp", keyGeoUp, SDL_Keycode.SDLK_UP, "STR_ROTATE_UP", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoDown", keyGeoDown, SDL_Keycode.SDLK_DOWN, "STR_ROTATE_DOWN", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoZoomIn", keyGeoZoomIn, SDL_Keycode.SDLK_PLUS, "STR_ZOOM_IN", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoZoomOut", keyGeoZoomOut, SDL_Keycode.SDLK_MINUS, "STR_ZOOM_OUT", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoSpeed1", keyGeoSpeed1, SDL_Keycode.SDLK_1, "STR_5_SECONDS", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoSpeed2", keyGeoSpeed2, SDL_Keycode.SDLK_2, "STR_1_MINUTE", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoSpeed3", keyGeoSpeed3, SDL_Keycode.SDLK_3, "STR_5_MINUTES", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoSpeed4", keyGeoSpeed4, SDL_Keycode.SDLK_4, "STR_30_MINUTES", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoSpeed5", keyGeoSpeed5, SDL_Keycode.SDLK_5, "STR_1_HOUR", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoSpeed6", keyGeoSpeed6, SDL_Keycode.SDLK_6, "STR_1_DAY", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoIntercept", keyGeoIntercept, SDL_Keycode.SDLK_i, "STR_INTERCEPT", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoBases", keyGeoBases, SDL_Keycode.SDLK_b, "STR_BASES", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoGraphs", keyGeoGraphs, SDL_Keycode.SDLK_g, "STR_GRAPHS", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoUfopedia", keyGeoUfopedia, SDL_Keycode.SDLK_u, "STR_UFOPAEDIA_UC", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoOptions", keyGeoOptions, SDL_Keycode.SDLK_ESCAPE, "STR_OPTIONS_UC", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoFunding", keyGeoFunding, SDL_Keycode.SDLK_f, "STR_FUNDING_UC", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoToggleDetail", keyGeoToggleDetail, SDL_Keycode.SDLK_TAB, "STR_TOGGLE_COUNTRY_DETAIL", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoToggleRadar", keyGeoToggleRadar, SDL_Keycode.SDLK_r, "STR_TOGGLE_RADAR_RANGES", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect1", keyBaseSelect1, SDL_Keycode.SDLK_1, "STR_SELECT_BASE_1", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect2", keyBaseSelect2, SDL_Keycode.SDLK_2, "STR_SELECT_BASE_2", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect3", keyBaseSelect3, SDL_Keycode.SDLK_3, "STR_SELECT_BASE_3", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect4", keyBaseSelect4, SDL_Keycode.SDLK_4, "STR_SELECT_BASE_4", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect5", keyBaseSelect5, SDL_Keycode.SDLK_5, "STR_SELECT_BASE_5", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect6", keyBaseSelect6, SDL_Keycode.SDLK_6, "STR_SELECT_BASE_6", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect7", keyBaseSelect7, SDL_Keycode.SDLK_7, "STR_SELECT_BASE_7", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect8", keyBaseSelect8, SDL_Keycode.SDLK_8, "STR_SELECT_BASE_8", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBattleLeft", keyBattleLeft, SDL_Keycode.SDLK_LEFT, "STR_SCROLL_LEFT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleRight", keyBattleRight, SDL_Keycode.SDLK_RIGHT, "STR_SCROLL_RIGHT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleUp", keyBattleUp, SDL_Keycode.SDLK_UP, "STR_SCROLL_UP", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleDown", keyBattleDown, SDL_Keycode.SDLK_DOWN, "STR_SCROLL_DOWN", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleLevelUp", keyBattleLevelUp, SDL_Keycode.SDLK_PAGEUP, "STR_VIEW_LEVEL_ABOVE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleLevelDown", keyBattleLevelDown, SDL_Keycode.SDLK_PAGEDOWN, "STR_VIEW_LEVEL_BELOW", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterUnit", keyBattleCenterUnit, SDL_Keycode.SDLK_HOME, "STR_CENTER_SELECTED_UNIT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattlePrevUnit", keyBattlePrevUnit, SDL_Keycode.SDLK_LSHIFT, "STR_PREVIOUS_UNIT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleNextUnit", keyBattleNextUnit, SDL_Keycode.SDLK_TAB, "STR_NEXT_UNIT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleDeselectUnit", keyBattleDeselectUnit, SDL_Keycode.SDLK_BACKSLASH, "STR_DESELECT_UNIT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleUseLeftHand", keyBattleUseLeftHand, SDL_Keycode.SDLK_q, "STR_USE_LEFT_HAND", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleUseRightHand", keyBattleUseRightHand, SDL_Keycode.SDLK_e, "STR_USE_RIGHT_HAND", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleInventory", keyBattleInventory, SDL_Keycode.SDLK_i, "STR_INVENTORY", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleMap", keyBattleMap, SDL_Keycode.SDLK_m, "STR_MINIMAP", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleOptions", keyBattleOptions, SDL_Keycode.SDLK_ESCAPE, "STR_OPTIONS", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleEndTurn", keyBattleEndTurn, SDL_Keycode.SDLK_BACKSPACE, "STR_END_TURN", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleAbort", keyBattleAbort, SDL_Keycode.SDLK_a, "STR_ABORT_MISSION", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleStats", keyBattleStats, SDL_Keycode.SDLK_s, "STR_UNIT_STATS", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleKneel", keyBattleKneel, SDL_Keycode.SDLK_k, "STR_KNEEL", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleReload", keyBattleReload, SDL_Keycode.SDLK_r, "STR_RELOAD", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattlePersonalLighting", keyBattlePersonalLighting, SDL_Keycode.SDLK_l, "STR_TOGGLE_PERSONAL_LIGHTING", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleReserveNone", keyBattleReserveNone, SDL_Keycode.SDLK_F1, "STR_DONT_RESERVE_TIME_UNITS", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleReserveSnap", keyBattleReserveSnap, SDL_Keycode.SDLK_F2, "STR_RESERVE_TIME_UNITS_FOR_SNAP_SHOT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleReserveAimed", keyBattleReserveAimed, SDL_Keycode.SDLK_F3, "STR_RESERVE_TIME_UNITS_FOR_AIMED_SHOT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleReserveAuto", keyBattleReserveAuto, SDL_Keycode.SDLK_F4, "STR_RESERVE_TIME_UNITS_FOR_AUTO_SHOT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleReserveKneel", keyBattleReserveKneel, SDL_Keycode.SDLK_j, "STR_RESERVE_TIME_UNITS_FOR_KNEEL", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleZeroTUs", keyBattleZeroTUs, SDL_Keycode.SDLK_DELETE, "STR_EXPEND_ALL_TIME_UNITS", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy1", keyBattleCenterEnemy1, SDL_Keycode.SDLK_1, "STR_CENTER_ON_ENEMY_1", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy2", keyBattleCenterEnemy2, SDL_Keycode.SDLK_2, "STR_CENTER_ON_ENEMY_2", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy3", keyBattleCenterEnemy3, SDL_Keycode.SDLK_3, "STR_CENTER_ON_ENEMY_3", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy4", keyBattleCenterEnemy4, SDL_Keycode.SDLK_4, "STR_CENTER_ON_ENEMY_4", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy5", keyBattleCenterEnemy5, SDL_Keycode.SDLK_5, "STR_CENTER_ON_ENEMY_5", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy6", keyBattleCenterEnemy6, SDL_Keycode.SDLK_6, "STR_CENTER_ON_ENEMY_6", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy7", keyBattleCenterEnemy7, SDL_Keycode.SDLK_7, "STR_CENTER_ON_ENEMY_7", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy8", keyBattleCenterEnemy8, SDL_Keycode.SDLK_8, "STR_CENTER_ON_ENEMY_8", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy9", keyBattleCenterEnemy9, SDL_Keycode.SDLK_9, "STR_CENTER_ON_ENEMY_9", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy10", keyBattleCenterEnemy10, SDL_Keycode.SDLK_0, "STR_CENTER_ON_ENEMY_10", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleVoxelView", keyBattleVoxelView, SDL_Keycode.SDLK_F10, "STR_SAVE_VOXEL_VIEW", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyInvCreateTemplate", keyInvCreateTemplate, SDL_Keycode.SDLK_c, "STR_CREATE_INVENTORY_TEMPLATE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyInvApplyTemplate", keyInvApplyTemplate, SDL_Keycode.SDLK_v, "STR_APPLY_INVENTORY_TEMPLATE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyInvClear", keyInvClear, SDL_Keycode.SDLK_x, "STR_CLEAR_INVENTORY", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyInvAutoEquip", keyInvAutoEquip, SDL_Keycode.SDLK_z, "STR_AUTO_EQUIP", "STR_BATTLESCAPE"));

        _info.Add(new OptionInfo("FPS", FPS, 60, "STR_FPS_LIMIT", "STR_GENERAL"));
        _info.Add(new OptionInfo("FPSInactive", FPSInactive, 30, "STR_FPS_INACTIVE_LIMIT", "STR_GENERAL"));
    }

    /**
     * Returns the game's User folder for the
     * currently loaded master mod.
     * @return Full path to User folder.
     */
    internal static string getMasterUserFolder() =>
        _userFolder + _masterMod + Path.PathSeparator;

    /**
     * Loads options from a YAML file.
     * @param filename YAML filename.
     * @return Was the loading successful?
     */
    static bool load(string filename = "options")
    {
	    string s = _configFolder + filename + ".cfg";
	    try
	    {
            using var input = new StreamReader(s);
            var yaml = new YamlStream();
            yaml.Load(input);
            var doc = (YamlMappingNode)yaml.Documents[0].RootNode;

		    // Ignore old options files
		    if (doc.Children["options"]["NewBattleMission"].ToString() == bool.TrueString)
		    {
			    return false;
		    }
		    foreach (var i in _info)
		    {
                i.load(doc.Children["options"]);
		    }

            mods.Clear();
            foreach (var i in (YamlSequenceNode)doc.Children["mods"])
		    {
			    string id = i["id"].ToString();
			    bool active = bool.Parse(i["active"].ToString());
			    mods.Add(KeyValuePair.Create(id, active));
		    }
		    if (!mods.Any())
		    {
			    _setDefaultMods();
		    }
	    }
	    catch (YamlException e)
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {e.Message}");
		    return false;
	    }
	    return true;
    }

    /**
     * Saves options to a YAML file.
     * @param filename YAML filename.
     * @return Was the saving successful?
     */
    internal static bool save(string filename = "options")
    {
	    string s = _configFolder + filename + ".cfg";
	    using var sav = new StreamWriter(s);
	    if (sav == null)
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} Failed to save {filename}.cfg");
		    return false;
	    }
	    try
	    {
            var serializer = new Serializer();
            //TODO
            serializer.Serialize(sav, new
            {
                mods = mods,
                options = _info
            });
	    }
	    catch (YamlException e)
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {e.Message}");
		    return false;
	    }
	    sav.Close();
	    if (sav == null)
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} Failed to save {filename}.cfg");
		    return false;
	    }
	    return true;
    }

    /**
     * Returns a list of currently active mods.
     * They must be enabled and activable.
     * @sa ModInfo::canActivate
     * @return List of info for the active mods.
     */
    internal static List<ModInfo> getActiveMods()
    {
        var activeMods = new List<ModInfo>();
        foreach (var mod in mods)
        {
            if (mod.Value)
            {
                ModInfo info = _modInfos[mod.Key];
                if (info.canActivate(_masterMod))
                {
                    activeMods.Add(info);
                }
            }
        }
        return activeMods;
    }

    internal static void updateMods()
    {
	    // pick up stuff in common before-hand
	    FileMap.load("common", CrossPlatform.searchDataFolder("common"), true);

        refreshMods();
	    mapResources();
        userSplitMasters();

        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Active mods:");
	    var activeMods = getActiveMods();
	    foreach (var activeMod in activeMods)
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} - {activeMod.getId()} v{activeMod.getVersion()}");
	    }
    }

    static void mapResources()
    {
        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Mapping resource files...");
        FileMap.clear();

        for (var i = mods.Count - 1; i >= 0; i--)
        {
            if (!mods[i].Value)
            {
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} skipping inactive mod: {mods[i].Key}");
                continue;
            }

            ModInfo modInfo = _modInfos[mods[i].Key];
            if (!modInfo.canActivate(_masterMod))
            {
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} skipping mod for non-current master: {mods[i].Key}({modInfo.getMaster()} != {_masterMod})");
                continue;
            }

            var circDepCheck = new HashSet<string>();
            _loadMod(modInfo, circDepCheck);
        }
        // TODO: Figure out why we still need to check common here
        FileMap.load("common", CrossPlatform.searchDataFolder("common"), true);
        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Resources files mapped successfully.");
    }

    static void _loadMod(ModInfo modInfo, HashSet<string> circDepCheck)
    {
	    if (circDepCheck.Contains(modInfo.getId()))
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} circular dependency found in master chain: {modInfo.getId()}");
		    return;
	    }

        FileMap.load(modInfo.getId(), modInfo.getPath(), false);
	    foreach (var externalResourceDir in modInfo.getExternalResourceDirs())
	    {
		    // use external resource folders from the user dir if they exist
		    // and if not, fall back to searching the data dirs
		    string extResourceFolder = _userFolder + externalResourceDir;
		    if (!CrossPlatform.folderExists(extResourceFolder))
		    {
			    extResourceFolder = CrossPlatform.searchDataFolder(externalResourceDir);
		    }

		    // always ignore ruleset files in external resource dirs
		    FileMap.load(modInfo.getId(), extResourceFolder, true);
	    }

	    // if this is a master but it has a master of its own, allow it to
	    // chainload the "super" master, including its rulesets
	    if (modInfo.isMaster() && !string.IsNullOrEmpty(modInfo.getMaster()))
	    {
		    // add self to circDepCheck so we can avoid circular dependencies
		    circDepCheck.Add(modInfo.getId());
		    if (_modInfos.TryGetValue(modInfo.getMaster(), out var masterInfo))
		    {
                _loadMod(masterInfo, circDepCheck);
		    }
		    else
		    {
			    throw new Exception(modInfo.getId() + " mod requires " + modInfo.getMaster() + " master");
		    }
	    }
    }

    /**
     * Splits the game's User folder by master mod,
     * creating a subfolder for each one and moving
     * the appropriate user data among them.
     */
    static void userSplitMasters()
    {
	    // get list of master mods
	    var masters = new List<string>();
	    foreach (var modInfo in _modInfos)
	    {
		    if (modInfo.Value.isMaster())
		    {
			    masters.Add(modInfo.Key);
		    }
	    }

	    // create master subfolders if they don't already exist
	    var saves = new List<string>();
	    foreach (var master in masters)
	    {
		    string masterFolder = _userFolder + master;
		    if (!CrossPlatform.folderExists(masterFolder))
		    {
			    CrossPlatform.createFolder(masterFolder);
			    // move any old saves to the appropriate folders
			    if (!saves.Any())
			    {
				    saves = CrossPlatform.getFolderContents(_userFolder, "sav");
				    List<string> autosaves = CrossPlatform.getFolderContents(_userFolder, "asav");
                    saves.AddRange(autosaves);
			    }
                for (var i = 0; i < saves.Count;)
			    {
                    string srcFile = _userFolder + saves[i];
                    using var input = new StreamReader(srcFile);
                    var yaml = new YamlStream();
                    yaml.Load(input);
                    var doc = (YamlMappingNode)yaml.Documents[0].RootNode;
				    if (((YamlSequenceNode)doc.Children["mods"]).Any(x => x["id"].ToString() == master))
				    {
					    string dstFile = masterFolder + Path.PathSeparator + saves[i];
                        CrossPlatform.moveFile(srcFile, dstFile);
                        saves.RemoveAt(i);
				    }
				    else
				    {
					    ++i;
				    }
			    }
		    }
	    }
    }

    static void refreshMods()
    {
        if (reload)
        {
            _masterMod = string.Empty;
        }

        _modInfos.Clear();
        string modPath = CrossPlatform.searchDataFolder("standard");
        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Scanning standard mods in '{modPath}'...");
        _scanMods(modPath);
        modPath = _userFolder + "mods";
        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Scanning user mods in '{modPath}'...");
        _scanMods(modPath);

        // remove mods from list that no longer exist
        for (var i = 0; i < mods.Count;)
        {
            if (!_modInfos.ContainsKey(mods[i].Key)
                || (mods[i].Key == "xcom1" && !_ufoIsInstalled())
                || (mods[i].Key == "xcom2" && !_tftdIsInstalled()))
            {
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} removing references to missing mod: {mods[i].Key}");
                mods.RemoveAt(i);
                continue;
            }
            ++i;
        }

        // add in any new mods picked up from the scan and ensure there is but a single
        // master active
        string activeMaster = null;
        string inactiveMaster = null;
        foreach (var modInfo in _modInfos.Keys)
        {
            bool found = false;
            for (var j = 0; j < mods.Count; ++j)
            {
                if (modInfo == mods[j].Key)
                {
                    found = true;
                    if (_modInfos[modInfo].isMaster())
                    {
                        if (!string.IsNullOrEmpty(_masterMod))
                        {
                            mods[j] = KeyValuePair.Create(mods[j].Key, _masterMod == mods[j].Key);
                        }
                        if (mods[j].Value)
                        {
                            if (!string.IsNullOrEmpty(activeMaster))
                            {
                                Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} too many active masters detected; turning off {mods[j].Key}");
                                mods[j] = KeyValuePair.Create(mods[j].Key, false);
                            }
                            else
                            {
                                activeMaster = mods[j].Key;
                            }
                        }
                        else
                        {
                            // prefer activating standard masters over a possibly broken
                            // third party master
                            if (string.IsNullOrEmpty(inactiveMaster) || mods[j].Key == "xcom1" || mods[j].Key == "xcom2")
                            {
                                inactiveMaster = mods[j].Key;
                            }
                        }
                    }

                    break;
                }
            }
            if (found)
            {
                continue;
            }

            // not active by default
            var newMod = KeyValuePair.Create(modInfo, false);
            if (_modInfos[modInfo].isMaster())
            {
                // it doesn't matter what order the masters are in since
                // only one can be active at a time anyway
                mods.Insert(0, newMod);

                if (string.IsNullOrEmpty(inactiveMaster))
                {
                    inactiveMaster = modInfo;
                }
            }
            else
            {
                mods.Add(newMod);
            }
        }

        if (string.IsNullOrEmpty(activeMaster))
        {
            if (string.IsNullOrEmpty(inactiveMaster))
            {
                Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} no mod masters available");
                throw new Exception("No X-COM installations found");
            }
            else
            {
                Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} no master already active; activating {inactiveMaster}");
                var index = mods.FindIndex(x => x.Key == inactiveMaster && x.Value == false);
                mods[index] = KeyValuePair.Create(mods[index].Key, true);
                _masterMod = inactiveMaster;
            }
        }
        else
        {
            _masterMod = activeMaster;
        }
        save();
    }

    static void _scanMods(string modsDir, bool metadataOnly = false)
    {
	    if (!CrossPlatform.folderExists(modsDir))
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} skipping non-existent mod directory: '{modsDir}'");
		    return;
	    }

	    string metadataFile = "/metadata.yml";
	    if (metadataOnly && CrossPlatform.fileExists(modsDir + metadataFile))
	    {
		    var modInfo = new ModInfo(modsDir);
		    modInfo.load(modsDir + metadataFile);
		    _modInfos.Add(modInfo.getId(), modInfo);
	    }
	    else
	    {
		    List<string> contents = CrossPlatform.getFolderContents(modsDir);
		    foreach (var item in contents)
		    {
			    string modPath = modsDir + Path.PathSeparator + item;
			    if (!CrossPlatform.folderExists(modPath))
			    {
				    // skip non-directories (e.g. README.txt)
				    continue;
			    }

                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} - {modPath}");
			    var modInfo = new ModInfo(modPath);

			    string metadataPath = modPath + metadataFile;
			    if (!CrossPlatform.fileExists(metadataPath))
			    {
                    Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} {metadataPath} not found;");
				    if (metadataOnly)
				    {
                        Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} skipping invalid mod: {item}");
					    continue;
				    }
				    else
				    {
                        Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} using default values for mod: {item}");
				    }
			    }
			    else
			    {
				    modInfo.load(metadataPath);
			    }

                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   id: {modInfo.getId()}");
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   name: {modInfo.getName()}");
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   version: {modInfo.getVersion()}");
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   description: {modInfo.getDescription()}");
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   author: {modInfo.getAuthor()}");
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   master: {modInfo.getMaster()}");
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   isMaster: {modInfo.isMaster()}");
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   loadResources:");
			    List<string> externals = modInfo.getExternalResourceDirs();
			    foreach (var external in externals)
			    {
                    Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}     {external}");
			    }

			    if (("xcom1" == modInfo.getId() && !_ufoIsInstalled())
				    || ("xcom2" == modInfo.getId() && !_tftdIsInstalled()))
			    {
                    Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} skipping {modInfo.getId()} since related game data isn't installed");
				    continue;
			    }

                _modInfos.Add(modInfo.getId(), modInfo);
		    }
	    }
    }

    internal static ModInfo getModInfo(string id) =>
        _modInfos[id];

    internal static Dictionary<string, ModInfo> getModInfos() =>
        _modInfos;
}
