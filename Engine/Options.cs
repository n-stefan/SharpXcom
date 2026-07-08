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
}

enum ScaleType
{
    SCALE_ORIGINAL,
    SCALE_15X,
    SCALE_2X,
    SCALE_SCREEN_DIV_3,
    SCALE_SCREEN_DIV_2,
    SCALE_SCREEN
}

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
    private static int _keyboardMode;
    internal static KeyboardType keyboardMode => (KeyboardType)_keyboardMode;
    private static int _saveOrder;
    internal static SaveSort saveOrder { get => (SaveSort)_saveOrder; set => _saveOrder = (int)value; }
    private static int _preferredMusic;
    internal static MusicFormat preferredMusic { get => (MusicFormat)_preferredMusic; set => _preferredMusic = (int)value; }
    private static int _preferredSound;
    internal static SoundFormat preferredSound { get => (SoundFormat)_preferredSound; set => _preferredSound = (int)value; }
    private static int _preferredVideo;
    internal static VideoFormat preferredVideo { get => (VideoFormat)_preferredVideo; set => _preferredVideo = (int)value; }
    private static bool _captureMouse;
    internal static SDLBool /* SDL_GrabMode */ captureMouse { get => _captureMouse; set => _captureMouse = value; }
    private static int _wordwrap;
    internal static TextWrapping wordwrap => (TextWrapping)_wordwrap;
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
    private static int _battleEdgeScroll;
    internal static ScrollType battleEdgeScroll { get => (ScrollType)_battleEdgeScroll; set => _battleEdgeScroll = (int)value; }
    private static int _battleNewPreviewPath;
    internal static PathPreview battleNewPreviewPath { get => (PathPreview)_battleNewPreviewPath; set => _battleNewPreviewPath = (int)value; }
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
    internal static List<KeyValuePair<string, bool>> mods = []; // ordered list of available mods (lowest priority to highest) and whether they are active
    internal static SoundFormat currentSound;

    static string _masterMod;
    static string _dataFolder;
    static List<string> _dataList = [];
    static string _userFolder;
    static string _configFolder;
    static Dictionary<string, string> _commandLine = [];
    static List<OptionInfo> _info = [];
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
     * Returns the game's Config folder where
     * settings are stored in. Normally the same
     * as the User folder.
     * @return Full path to Config folder.
     */
    string getConfigFolder() =>
        _configFolder;

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
        //TODO: Uncomment
        //updateOptions();

        string s = getUserFolder();
        s += "sharpxcom.log";
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

        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} SharpXcom Version: {SHARPXCOM_VERSION_SHORT}{SHARPXCOM_VERSION_GIT}");
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
        help.AppendLine($"SharpXcom v{SHARPXCOM_VERSION_SHORT}");
        help.AppendLine($"Usage: sharpxcom [OPTION]...{Environment.NewLine}");
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
    internal static void resetDefault(bool includeMods)
    {
        for (var i = 0; i < _info.Count; i++)
        {
            _info[i].reset();
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
    internal static void backupDisplay()
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
        _info.Add(new OptionInfo("displayWidth", ref displayWidth, Screen.ORIGINAL_WIDTH * 2));
        _info.Add(new OptionInfo("displayHeight", ref displayHeight, Screen.ORIGINAL_HEIGHT * 2));
        _info.Add(new OptionInfo("fullscreen", ref fullscreen, false));
        _info.Add(new OptionInfo("asyncBlit", ref asyncBlit, true));
        _info.Add(new OptionInfo("keyboardMode", ref _keyboardMode, (int)KeyboardType.KEYBOARD_ON));

        _info.Add(new OptionInfo("maxFrameSkip", ref maxFrameSkip, 0));
        _info.Add(new OptionInfo("traceAI", ref traceAI, false));
        _info.Add(new OptionInfo("verboseLogging", ref verboseLogging, false));
        _info.Add(new OptionInfo("StereoSound", ref StereoSound, true));
        //_info.Add(new OptionInfo("baseXResolution", baseXResolution, Screen.ORIGINAL_WIDTH));
        //_info.Add(new OptionInfo("baseYResolution", baseYResolution, Screen.ORIGINAL_HEIGHT));
        //_info.Add(new OptionInfo("baseXGeoscape", baseXGeoscape, Screen.ORIGINAL_WIDTH));
        //_info.Add(new OptionInfo("baseYGeoscape", baseYGeoscape, Screen.ORIGINAL_HEIGHT));
        //_info.Add(new OptionInfo("baseXBattlescape", baseXBattlescape, Screen.ORIGINAL_WIDTH));
        //_info.Add(new OptionInfo("baseYBattlescape", baseYBattlescape, Screen.ORIGINAL_HEIGHT));
        _info.Add(new OptionInfo("geoscapeScale", ref geoscapeScale, 0));
        _info.Add(new OptionInfo("battlescapeScale", ref battlescapeScale, 0));
        _info.Add(new OptionInfo("useScaleFilter", ref useScaleFilter, false));
        _info.Add(new OptionInfo("useHQXFilter", ref useHQXFilter, false));
        _info.Add(new OptionInfo("useXBRZFilter", ref useXBRZFilter, false));
        _info.Add(new OptionInfo("useOpenGL", ref useOpenGL, false));
        _info.Add(new OptionInfo("checkOpenGLErrors", ref checkOpenGLErrors, false));
        _info.Add(new OptionInfo("useOpenGLShader", ref useOpenGLShader, "Shaders/Raw.OpenGL.shader"));
        _info.Add(new OptionInfo("vSyncForOpenGL", ref vSyncForOpenGL, true));
        _info.Add(new OptionInfo("useOpenGLSmoothing", ref useOpenGLSmoothing, true));
        _info.Add(new OptionInfo("debug", ref debug, false));
        _info.Add(new OptionInfo("debugUi", ref debugUi, false));
        _info.Add(new OptionInfo("soundVolume", ref soundVolume, (int)(2 * (/* MIX_MAX_VOLUME */ 1.0f / 3))));
        _info.Add(new OptionInfo("musicVolume", ref musicVolume, (int)(2 * (/* MIX_MAX_VOLUME */ 1.0f / 3))));
        _info.Add(new OptionInfo("uiVolume", ref uiVolume, (int)(/* MIX_MAX_VOLUME */ 1.0f / 3)));
        _info.Add(new OptionInfo("language", ref language, string.Empty));
        _info.Add(new OptionInfo("battleScrollSpeed", ref battleScrollSpeed, 8));
        _info.Add(new OptionInfo("battleEdgeScroll", ref _battleEdgeScroll, (int)ScrollType.SCROLL_AUTO));
        _info.Add(new OptionInfo("battleDragScrollButton", ref battleDragScrollButton, (int)SDL_BUTTON_MIDDLE));
        _info.Add(new OptionInfo("dragScrollTimeTolerance", ref dragScrollTimeTolerance, 300)); // miliSecond
        _info.Add(new OptionInfo("dragScrollPixelTolerance", ref dragScrollPixelTolerance, 10)); // count of pixels
        _info.Add(new OptionInfo("battleFireSpeed", ref battleFireSpeed, 6));
        _info.Add(new OptionInfo("battleXcomSpeed", ref battleXcomSpeed, 30));
        _info.Add(new OptionInfo("battleAlienSpeed", ref battleAlienSpeed, 30));
        _info.Add(new OptionInfo("battleNewPreviewPath", ref _battleNewPreviewPath, (int)PathPreview.PATH_NONE)); // requires double-click to confirm moves
        _info.Add(new OptionInfo("fpsCounter", ref fpsCounter, false));
        _info.Add(new OptionInfo("globeDetail", ref globeDetail, true));
        _info.Add(new OptionInfo("globeRadarLines", ref globeRadarLines, true));
        _info.Add(new OptionInfo("globeFlightPaths", ref globeFlightPaths, true));
        _info.Add(new OptionInfo("globeAllRadarsOnBaseBuild", ref globeAllRadarsOnBaseBuild, true));
        _info.Add(new OptionInfo("audioSampleRate", ref audioSampleRate, 22050));
        _info.Add(new OptionInfo("audioBitDepth", ref audioBitDepth, 16));
        _info.Add(new OptionInfo("audioChunkSize", ref audioChunkSize, 1024));
        _info.Add(new OptionInfo("pauseMode", ref pauseMode, 0));
        _info.Add(new OptionInfo("battleNotifyDeath", ref battleNotifyDeath, false));
        _info.Add(new OptionInfo("showFundsOnGeoscape", ref showFundsOnGeoscape, false));
        _info.Add(new OptionInfo("allowResize", ref allowResize, false));
        _info.Add(new OptionInfo("windowedModePositionX", ref windowedModePositionX, 0));
        _info.Add(new OptionInfo("windowedModePositionY", ref windowedModePositionY, 0));
        _info.Add(new OptionInfo("borderless", ref borderless, false));
        _info.Add(new OptionInfo("captureMouse", ref _captureMouse, false));
        _info.Add(new OptionInfo("battleTooltips", ref battleTooltips, true));
        _info.Add(new OptionInfo("keepAspectRatio", ref keepAspectRatio, true));
        _info.Add(new OptionInfo("nonSquarePixelRatio", ref nonSquarePixelRatio, false));
        _info.Add(new OptionInfo("cursorInBlackBandsInFullscreen", ref cursorInBlackBandsInFullscreen, false));
        _info.Add(new OptionInfo("cursorInBlackBandsInWindow", ref cursorInBlackBandsInWindow, true));
        _info.Add(new OptionInfo("cursorInBlackBandsInBorderlessWindow", ref cursorInBlackBandsInBorderlessWindow, false));
        _info.Add(new OptionInfo("saveOrder", ref _saveOrder, (int)SaveSort.SORT_DATE_DESC));
        _info.Add(new OptionInfo("geoClockSpeed", ref geoClockSpeed, 80));
        _info.Add(new OptionInfo("dogfightSpeed", ref dogfightSpeed, 30));
        _info.Add(new OptionInfo("geoScrollSpeed", ref geoScrollSpeed, 20));
        _info.Add(new OptionInfo("geoDragScrollButton", ref geoDragScrollButton, (int)SDL_BUTTON_MIDDLE));
        _info.Add(new OptionInfo("preferredMusic", ref _preferredMusic, (int)MusicFormat.MUSIC_AUTO));
        _info.Add(new OptionInfo("preferredSound", ref _preferredSound, (int)SoundFormat.SOUND_AUTO));
        _info.Add(new OptionInfo("preferredVideo", ref _preferredVideo, (int)VideoFormat.VIDEO_FMV));
        _info.Add(new OptionInfo("wordwrap", ref _wordwrap, (int)TextWrapping.WRAP_AUTO));
        _info.Add(new OptionInfo("musicAlwaysLoop", ref musicAlwaysLoop, false));
        _info.Add(new OptionInfo("touchEnabled", ref touchEnabled, false));
        _info.Add(new OptionInfo("rootWindowedMode", ref rootWindowedMode, false));
        _info.Add(new OptionInfo("lazyLoadResources", ref lazyLoadResources, true));
        _info.Add(new OptionInfo("backgroundMute", ref backgroundMute, false));

        // advanced options
        _info.Add(new OptionInfo("playIntro", ref playIntro, true, "STR_PLAYINTRO", "STR_GENERAL"));
        _info.Add(new OptionInfo("autosave", ref autosave, true, "STR_AUTOSAVE", "STR_GENERAL"));
        _info.Add(new OptionInfo("autosaveFrequency", ref autosaveFrequency, 5, "STR_AUTOSAVE_FREQUENCY", "STR_GENERAL"));
        _info.Add(new OptionInfo("newSeedOnLoad", ref newSeedOnLoad, false, "STR_NEWSEEDONLOAD", "STR_GENERAL"));
        _info.Add(new OptionInfo("mousewheelSpeed", ref mousewheelSpeed, 3, "STR_MOUSEWHEEL_SPEED", "STR_GENERAL"));
        _info.Add(new OptionInfo("changeValueByMouseWheel", ref changeValueByMouseWheel, 0, "STR_CHANGEVALUEBYMOUSEWHEEL", "STR_GENERAL"));
        _info.Add(new OptionInfo("soldierDiaries", ref soldierDiaries, true));

        _info.Add(new OptionInfo("maximizeInfoScreens", ref maximizeInfoScreens, false, "STR_MAXIMIZE_INFO_SCREENS", "STR_GENERAL"));

        _info.Add(new OptionInfo("geoDragScrollInvert", ref geoDragScrollInvert, false, "STR_DRAGSCROLLINVERT", "STR_GEOSCAPE")); // true drags away from the cursor, false drags towards (like a grab)
        _info.Add(new OptionInfo("aggressiveRetaliation", ref aggressiveRetaliation, false, "STR_AGGRESSIVERETALIATION", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("customInitialBase", ref customInitialBase, false, "STR_CUSTOMINITIALBASE", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("allowBuildingQueue", ref allowBuildingQueue, false, "STR_ALLOWBUILDINGQUEUE", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("craftLaunchAlways", ref craftLaunchAlways, false, "STR_CRAFTLAUNCHALWAYS", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("storageLimitsEnforced", ref storageLimitsEnforced, false, "STR_STORAGELIMITSENFORCED", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("canSellLiveAliens", ref canSellLiveAliens, false, "STR_CANSELLLIVEALIENS", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("anytimePsiTraining", ref anytimePsiTraining, false, "STR_ANYTIMEPSITRAINING", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("globeSeasons", ref globeSeasons, false, "STR_GLOBESEASONS", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("psiStrengthEval", ref psiStrengthEval, false, "STR_PSISTRENGTHEVAL", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("canTransferCraftsWhileAirborne", ref canTransferCraftsWhileAirborne, false, "STR_CANTRANSFERCRAFTSWHILEAIRBORNE", "STR_GEOSCAPE")); // When the craft can reach the destination base with its fuel
        _info.Add(new OptionInfo("retainCorpses", ref retainCorpses, false, "STR_RETAINCORPSES", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("fieldPromotions", ref fieldPromotions, false, "STR_FIELDPROMOTIONS", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("meetingPoint", ref meetingPoint, false, "STR_MEETINGPOINT", "STR_GEOSCAPE"));

        _info.Add(new OptionInfo("battleDragScrollInvert", ref battleDragScrollInvert, false, "STR_DRAGSCROLLINVERT", "STR_BATTLESCAPE")); // true drags away from the cursor, false drags towards (like a grab)
        _info.Add(new OptionInfo("sneakyAI", ref sneakyAI, false, "STR_SNEAKYAI", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("battleUFOExtenderAccuracy", ref battleUFOExtenderAccuracy, false, "STR_BATTLEUFOEXTENDERACCURACY", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("showMoreStatsInInventoryView", ref showMoreStatsInInventoryView, false, "STR_SHOWMORESTATSININVENTORYVIEW", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("battleHairBleach", ref battleHairBleach, true, "STR_BATTLEHAIRBLEACH", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("battleInstantGrenade", ref battleInstantGrenade, false, "STR_BATTLEINSTANTGRENADE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("includePrimeStateInSavedLayout", ref includePrimeStateInSavedLayout, false, "STR_INCLUDE_PRIMESTATE_IN_SAVED_LAYOUT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("battleExplosionHeight", ref battleExplosionHeight, 0, "STR_BATTLEEXPLOSIONHEIGHT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("battleAutoEnd", ref battleAutoEnd, false, "STR_BATTLEAUTOEND", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("battleSmoothCamera", ref battleSmoothCamera, false, "STR_BATTLESMOOTHCAMERA", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("disableAutoEquip", ref disableAutoEquip, false, "STR_DISABLEAUTOEQUIP", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("battleConfirmFireMode", ref battleConfirmFireMode, false, "STR_BATTLECONFIRMFIREMODE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("weaponSelfDestruction", ref weaponSelfDestruction, false, "STR_WEAPONSELFDESTRUCTION", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("allowPsionicCapture", ref allowPsionicCapture, false, "STR_ALLOWPSIONICCAPTURE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("allowPsiStrengthImprovement", ref allowPsiStrengthImprovement, false, "STR_ALLOWPSISTRENGTHIMPROVEMENT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("strafe", ref strafe, false, "STR_STRAFE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("forceFire", ref forceFire, true, "STR_FORCE_FIRE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("skipNextTurnScreen", ref skipNextTurnScreen, false, "STR_SKIPNEXTTURNSCREEN", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("noAlienPanicMessages", ref noAlienPanicMessages, false, "STR_NOALIENPANICMESSAGES", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("alienBleeding", ref alienBleeding, false, "STR_ALIENBLEEDING", "STR_BATTLESCAPE"));

        // controls
        _info.Add(new OptionInfo("keyOk", ref keyOk, SDL_Keycode.SDLK_RETURN, "STR_OK", "STR_GENERAL"));
        _info.Add(new OptionInfo("keyCancel", ref keyCancel, SDL_Keycode.SDLK_ESCAPE, "STR_CANCEL", "STR_GENERAL"));
        _info.Add(new OptionInfo("keyScreenshot", ref keyScreenshot, SDL_Keycode.SDLK_F12, "STR_SCREENSHOT", "STR_GENERAL"));
        _info.Add(new OptionInfo("keyFps", ref keyFps, SDL_Keycode.SDLK_F7, "STR_FPS_COUNTER", "STR_GENERAL"));
        _info.Add(new OptionInfo("keyQuickSave", ref keyQuickSave, SDL_Keycode.SDLK_F5, "STR_QUICK_SAVE", "STR_GENERAL"));
        _info.Add(new OptionInfo("keyQuickLoad", ref keyQuickLoad, SDL_Keycode.SDLK_F9, "STR_QUICK_LOAD", "STR_GENERAL"));
        _info.Add(new OptionInfo("keyGeoLeft", ref keyGeoLeft, SDL_Keycode.SDLK_LEFT, "STR_ROTATE_LEFT", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoRight", ref keyGeoRight, SDL_Keycode.SDLK_RIGHT, "STR_ROTATE_RIGHT", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoUp", ref keyGeoUp, SDL_Keycode.SDLK_UP, "STR_ROTATE_UP", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoDown", ref keyGeoDown, SDL_Keycode.SDLK_DOWN, "STR_ROTATE_DOWN", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoZoomIn", ref keyGeoZoomIn, SDL_Keycode.SDLK_PLUS, "STR_ZOOM_IN", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoZoomOut", ref keyGeoZoomOut, SDL_Keycode.SDLK_MINUS, "STR_ZOOM_OUT", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoSpeed1", ref keyGeoSpeed1, SDL_Keycode.SDLK_1, "STR_5_SECONDS", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoSpeed2", ref keyGeoSpeed2, SDL_Keycode.SDLK_2, "STR_1_MINUTE", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoSpeed3", ref keyGeoSpeed3, SDL_Keycode.SDLK_3, "STR_5_MINUTES", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoSpeed4", ref keyGeoSpeed4, SDL_Keycode.SDLK_4, "STR_30_MINUTES", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoSpeed5", ref keyGeoSpeed5, SDL_Keycode.SDLK_5, "STR_1_HOUR", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoSpeed6", ref keyGeoSpeed6, SDL_Keycode.SDLK_6, "STR_1_DAY", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoIntercept", ref keyGeoIntercept, SDL_Keycode.SDLK_I, "STR_INTERCEPT", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoBases", ref keyGeoBases, SDL_Keycode.SDLK_B, "STR_BASES", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoGraphs", ref keyGeoGraphs, SDL_Keycode.SDLK_G, "STR_GRAPHS", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoUfopedia", ref keyGeoUfopedia, SDL_Keycode.SDLK_U, "STR_UFOPAEDIA_UC", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoOptions", ref keyGeoOptions, SDL_Keycode.SDLK_ESCAPE, "STR_OPTIONS_UC", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoFunding", ref keyGeoFunding, SDL_Keycode.SDLK_F, "STR_FUNDING_UC", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoToggleDetail", ref keyGeoToggleDetail, SDL_Keycode.SDLK_TAB, "STR_TOGGLE_COUNTRY_DETAIL", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyGeoToggleRadar", ref keyGeoToggleRadar, SDL_Keycode.SDLK_R, "STR_TOGGLE_RADAR_RANGES", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect1", ref keyBaseSelect1, SDL_Keycode.SDLK_1, "STR_SELECT_BASE_1", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect2", ref keyBaseSelect2, SDL_Keycode.SDLK_2, "STR_SELECT_BASE_2", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect3", ref keyBaseSelect3, SDL_Keycode.SDLK_3, "STR_SELECT_BASE_3", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect4", ref keyBaseSelect4, SDL_Keycode.SDLK_4, "STR_SELECT_BASE_4", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect5", ref keyBaseSelect5, SDL_Keycode.SDLK_5, "STR_SELECT_BASE_5", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect6", ref keyBaseSelect6, SDL_Keycode.SDLK_6, "STR_SELECT_BASE_6", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect7", ref keyBaseSelect7, SDL_Keycode.SDLK_7, "STR_SELECT_BASE_7", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBaseSelect8", ref keyBaseSelect8, SDL_Keycode.SDLK_8, "STR_SELECT_BASE_8", "STR_GEOSCAPE"));
        _info.Add(new OptionInfo("keyBattleLeft", ref keyBattleLeft, SDL_Keycode.SDLK_LEFT, "STR_SCROLL_LEFT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleRight", ref keyBattleRight, SDL_Keycode.SDLK_RIGHT, "STR_SCROLL_RIGHT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleUp", ref keyBattleUp, SDL_Keycode.SDLK_UP, "STR_SCROLL_UP", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleDown", ref keyBattleDown, SDL_Keycode.SDLK_DOWN, "STR_SCROLL_DOWN", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleLevelUp", ref keyBattleLevelUp, SDL_Keycode.SDLK_PAGEUP, "STR_VIEW_LEVEL_ABOVE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleLevelDown", ref keyBattleLevelDown, SDL_Keycode.SDLK_PAGEDOWN, "STR_VIEW_LEVEL_BELOW", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterUnit", ref keyBattleCenterUnit, SDL_Keycode.SDLK_HOME, "STR_CENTER_SELECTED_UNIT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattlePrevUnit", ref keyBattlePrevUnit, SDL_Keycode.SDLK_LSHIFT, "STR_PREVIOUS_UNIT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleNextUnit", ref keyBattleNextUnit, SDL_Keycode.SDLK_TAB, "STR_NEXT_UNIT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleDeselectUnit", ref keyBattleDeselectUnit, SDL_Keycode.SDLK_BACKSLASH, "STR_DESELECT_UNIT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleUseLeftHand", ref keyBattleUseLeftHand, SDL_Keycode.SDLK_Q, "STR_USE_LEFT_HAND", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleUseRightHand", ref keyBattleUseRightHand, SDL_Keycode.SDLK_E, "STR_USE_RIGHT_HAND", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleInventory", ref keyBattleInventory, SDL_Keycode.SDLK_I, "STR_INVENTORY", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleMap", ref keyBattleMap, SDL_Keycode.SDLK_M, "STR_MINIMAP", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleOptions", ref keyBattleOptions, SDL_Keycode.SDLK_ESCAPE, "STR_OPTIONS", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleEndTurn", ref keyBattleEndTurn, SDL_Keycode.SDLK_BACKSPACE, "STR_END_TURN", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleAbort", ref keyBattleAbort, SDL_Keycode.SDLK_A, "STR_ABORT_MISSION", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleStats", ref keyBattleStats, SDL_Keycode.SDLK_S, "STR_UNIT_STATS", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleKneel", ref keyBattleKneel, SDL_Keycode.SDLK_K, "STR_KNEEL", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleReload", ref keyBattleReload, SDL_Keycode.SDLK_R, "STR_RELOAD", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattlePersonalLighting", ref keyBattlePersonalLighting, SDL_Keycode.SDLK_L, "STR_TOGGLE_PERSONAL_LIGHTING", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleReserveNone", ref keyBattleReserveNone, SDL_Keycode.SDLK_F1, "STR_DONT_RESERVE_TIME_UNITS", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleReserveSnap", ref keyBattleReserveSnap, SDL_Keycode.SDLK_F2, "STR_RESERVE_TIME_UNITS_FOR_SNAP_SHOT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleReserveAimed", ref keyBattleReserveAimed, SDL_Keycode.SDLK_F3, "STR_RESERVE_TIME_UNITS_FOR_AIMED_SHOT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleReserveAuto", ref keyBattleReserveAuto, SDL_Keycode.SDLK_F4, "STR_RESERVE_TIME_UNITS_FOR_AUTO_SHOT", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleReserveKneel", ref keyBattleReserveKneel, SDL_Keycode.SDLK_J, "STR_RESERVE_TIME_UNITS_FOR_KNEEL", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleZeroTUs", ref keyBattleZeroTUs, SDL_Keycode.SDLK_DELETE, "STR_EXPEND_ALL_TIME_UNITS", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy1", ref keyBattleCenterEnemy1, SDL_Keycode.SDLK_1, "STR_CENTER_ON_ENEMY_1", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy2", ref keyBattleCenterEnemy2, SDL_Keycode.SDLK_2, "STR_CENTER_ON_ENEMY_2", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy3", ref keyBattleCenterEnemy3, SDL_Keycode.SDLK_3, "STR_CENTER_ON_ENEMY_3", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy4", ref keyBattleCenterEnemy4, SDL_Keycode.SDLK_4, "STR_CENTER_ON_ENEMY_4", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy5", ref keyBattleCenterEnemy5, SDL_Keycode.SDLK_5, "STR_CENTER_ON_ENEMY_5", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy6", ref keyBattleCenterEnemy6, SDL_Keycode.SDLK_6, "STR_CENTER_ON_ENEMY_6", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy7", ref keyBattleCenterEnemy7, SDL_Keycode.SDLK_7, "STR_CENTER_ON_ENEMY_7", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy8", ref keyBattleCenterEnemy8, SDL_Keycode.SDLK_8, "STR_CENTER_ON_ENEMY_8", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy9", ref keyBattleCenterEnemy9, SDL_Keycode.SDLK_9, "STR_CENTER_ON_ENEMY_9", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleCenterEnemy10", ref keyBattleCenterEnemy10, SDL_Keycode.SDLK_0, "STR_CENTER_ON_ENEMY_10", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyBattleVoxelView", ref keyBattleVoxelView, SDL_Keycode.SDLK_F10, "STR_SAVE_VOXEL_VIEW", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyInvCreateTemplate", ref keyInvCreateTemplate, SDL_Keycode.SDLK_C, "STR_CREATE_INVENTORY_TEMPLATE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyInvApplyTemplate", ref keyInvApplyTemplate, SDL_Keycode.SDLK_V, "STR_APPLY_INVENTORY_TEMPLATE", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyInvClear", ref keyInvClear, SDL_Keycode.SDLK_X, "STR_CLEAR_INVENTORY", "STR_BATTLESCAPE"));
        _info.Add(new OptionInfo("keyInvAutoEquip", ref keyInvAutoEquip, SDL_Keycode.SDLK_Z, "STR_AUTO_EQUIP", "STR_BATTLESCAPE"));

        _info.Add(new OptionInfo("FPS", ref FPS, 60, "STR_FPS_LIMIT", "STR_GENERAL"));
        _info.Add(new OptionInfo("FPSInactive", ref FPSInactive, 30, "STR_FPS_INACTIVE_LIMIT", "STR_GENERAL"));
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
    internal static bool load(string filename = "options")
    {
        string s = _configFolder + filename + ".cfg";
        try
        {
            using var input = new StreamReader(s);
            var yaml = new YamlStream();
            yaml.Load(input);
            var doc = (YamlMappingNode)yaml.Documents[0].RootNode;
            // Ignore old options files
            if (doc.Children["options"]["NewBattleMission"] != null)
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
        try
        {
            using var sav = new StreamWriter(s);
            var @out = new Emitter(sav);

            YamlMappingNode doc = new(), node = new();
            foreach (var i in _info)
            {
                i.save(node);
            }
            doc.Add("options", node);

            foreach (var i in mods)
            {
                YamlMappingNode mod = new()
                {
                    { "id", i.Key },
                    { "active", i.Value.ToString() }
                };
                ((YamlSequenceNode)doc["mods"]).Add(mod);
            }

            @out.Emit(new StreamStart());
            @out.Emit(new DocumentStart());

            writeNode(doc, @out);
            //var serializer = new Serializer();
            //serializer.Serialize(@out, doc);

            @out.Emit(new DocumentEnd(false));
            @out.Emit(new StreamEnd());

            sav.WriteLine();
            sav.Close();
        }
        catch (YamlException e)
        {
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {e.Message}");
            return false;
        }
        catch (Exception)
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

    internal static void mapResources()
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
        foreach (var i in _modInfos)
        {
            if (i.Value.isMaster())
            {
                masters.Add(i.Key);
            }
        }

        // create master subfolders if they don't already exist
        var saves = new List<string>();
        foreach (var i in masters)
        {
            string masterFolder = _userFolder + i;
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
                for (var j = 0; j < saves.Count;)
                {
                    string srcFile = _userFolder + saves[j];
                    using var input = new StreamReader(srcFile);
                    var yaml = new YamlStream();
                    yaml.Load(input);
                    var doc = (YamlMappingNode)yaml.Documents[0].RootNode;
                    var srcMods = doc["mods"] != null ? ((YamlSequenceNode)doc["mods"]).Children.Select(x => x.ToString()).ToList() : new List<string>();
                    if (srcMods.Contains(i))
                    {
                        string dstFile = masterFolder + Path.PathSeparator + saves[j];
                        CrossPlatform.moveFile(srcFile, dstFile);
                        saves.RemoveAt(j);
                    }
                    else
                    {
                        ++j;
                    }
                }
            }
        }
    }

    internal static void refreshMods()
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

    /**
     * Switches old/new display options for temporarily
     * testing a new display setup.
     */
    internal static void switchDisplay()
    {
        (newDisplayWidth, displayWidth) = (displayWidth, newDisplayWidth);
        (newDisplayHeight, displayHeight) = (displayHeight, newDisplayHeight);
        (newOpenGL, useOpenGL) = (useOpenGL, newOpenGL);
        (newScaleFilter, useScaleFilter) = (useScaleFilter, newScaleFilter);
        (newBattlescapeScale, battlescapeScale) = (battlescapeScale, newBattlescapeScale);
        (newGeoscapeScale, geoscapeScale) = (geoscapeScale, newGeoscapeScale);
        (newHQXFilter, useHQXFilter) = (useHQXFilter, newHQXFilter);
        (newOpenGLShader, useOpenGLShader) = (useOpenGLShader, newOpenGLShader);
        (newXBRZFilter, useXBRZFilter) = (useXBRZFilter, newXBRZFilter);
        (newRootWindowedMode, rootWindowedMode) = (rootWindowedMode, newRootWindowedMode);
        (newWindowedModePositionX, windowedModePositionX) = (windowedModePositionX, newWindowedModePositionX);
        (newWindowedModePositionY, windowedModePositionY) = (windowedModePositionY, newWindowedModePositionY);
        (newFullscreen, fullscreen) = (fullscreen, newFullscreen);
        (newAllowResize, allowResize) = (allowResize, newAllowResize);
        (newBorderless, borderless) = (borderless, newBorderless);
    }

    /**
     * Returns the game's list of all available option information.
     * @return List of OptionInfo's.
     */
    internal static List<OptionInfo> getOptionInfo() =>
        _info;

    static void writeNode(YamlNode node, Emitter emitter)
    {
        switch (node.NodeType)
        {
            case YamlNodeType.Sequence:
                {
                    emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
                    for (var i = 0; i < ((YamlSequenceNode)node).Count(); i++)
                    {
                        writeNode(node[i], emitter);
                    }
                    emitter.Emit(new SequenceEnd());
                    break;
                }
            case YamlNodeType.Mapping:
                {
                    emitter.Emit(new MappingStart());

                    // First collect all the keys
                    var keys = new List<string>(((YamlMappingNode)node).Count());
                    int key_it = 0;
                    foreach (var it in ((YamlMappingNode)node).Children)
                    {
                        keys[key_it++] = it.Key.ToString();
                    }

                    // Then sort them
                    keys.Sort();

                    // Then emit all the entries in sorted order.
                    for (var i = 0; i < keys.Count; i++)
                    {
                        //emitter << YAML::Key;
                        emitter.Emit(new Scalar(keys[i]));
                        //emitter << YAML::Value;
                        writeNode(node[keys[i]], emitter);
                    }
                    emitter.Emit(new MappingEnd());
                    break;
                }
            default:
                emitter.Emit(new Scalar(node.ToString()));
                break;
        }
    }
}
