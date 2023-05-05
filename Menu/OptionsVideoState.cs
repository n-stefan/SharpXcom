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
 * Screen that lets the user configure various
 * Video options.
 */
internal class OptionsVideoState : OptionsBaseState
{
    const string GL_EXT = "OpenGL.shader";
    const string GL_FOLDER = "Shaders/";
    const string GL_STRING = "*";

    InteractiveSurface _displaySurface;
    Text _txtDisplayResolution, _txtDisplayX;
    TextEdit _txtDisplayWidth, _txtDisplayHeight;
    ArrowButton _btnDisplayResolutionUp, _btnDisplayResolutionDown;
    Text _txtLanguage, _txtFilter, _txtGeoScale, _txtBattleScale;
    ComboBox _cbxLanguage, _cbxFilter, _cbxDisplayMode, _cbxGeoScale, _cbxBattleScale;
    Text _txtMode;
    Text _txtOptions;
    ToggleTextButton _btnLetterbox, _btnLockMouse, _btnRootWindowedMode;
    int _resAmount, _resCurrent;
    /* SDL_Rect */ SDL_DisplayMode[] _res;
    List<string> _langs, _filters;

    /**
     * Initializes all the elements in the Video Options screen.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     */
    internal OptionsVideoState(OptionsOrigin origin) : base(origin)
    {
        setCategory(_btnVideo);

        // Create objects
        _displaySurface = new InteractiveSurface(110, 32, 94, 18);
        _txtDisplayResolution = new Text(114, 9, 94, 8);
        _txtDisplayWidth = new TextEdit(this, 40, 17, 94, 26);
        _txtDisplayX = new Text(16, 17, 132, 26);
        _txtDisplayHeight = new TextEdit(this, 40, 17, 144, 26);
        _btnDisplayResolutionUp = new ArrowButton(ArrowShape.ARROW_BIG_UP, 14, 14, 186, 18);
        _btnDisplayResolutionDown = new ArrowButton(ArrowShape.ARROW_BIG_DOWN, 14, 14, 186, 36);

        _txtLanguage = new Text(114, 9, 94, 52);
        _cbxLanguage = new ComboBox(this, 104, 16, 94, 62);

        _txtFilter = new Text(114, 9, 206, 52);
        _cbxFilter = new ComboBox(this, 104, 16, 206, 62);

        _txtMode = new Text(114, 9, 206, 22);
        _cbxDisplayMode = new ComboBox(this, 104, 16, 206, 32);

        _txtGeoScale = new Text(114, 9, 94, 82);
        _cbxGeoScale = new ComboBox(this, 104, 16, 94, 92);

        _txtBattleScale = new Text(114, 9, 94, 112);
        _cbxBattleScale = new ComboBox(this, 104, 16, 94, 122);

        _txtOptions = new Text(114, 9, 206, 82);
        _btnLetterbox = new ToggleTextButton(104, 16, 206, 92);
        _btnLockMouse = new ToggleTextButton(104, 16, 206, 110);
        _btnRootWindowedMode = new ToggleTextButton(104, 16, 206, 128);

        // Get available fullscreen modes
        for (int j = 0; j < SDL_GetNumDisplayModes(0); ++j)
        {
            SDL_GetDisplayMode(0, j, out _res[j]); //SDL_ListModes(NULL, SDL_FULLSCREEN)
        }
        if (_res != null)
        {
            int i;
            _resCurrent = -1;
            for (i = 0; i < _res.Length; ++i)
            {
                if (_resCurrent == -1 &&
                    ((_res[i].w == Options.displayWidth && _res[i].h <= Options.displayHeight) || _res[i].w < Options.displayWidth))
                {
                    _resCurrent = i;
                }
            }
            _resAmount = i;
        }
        else
        {
            _resCurrent = -1;
            _resAmount = 0;
            _btnDisplayResolutionDown.setVisible(false);
            _btnDisplayResolutionUp.setVisible(false);
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} Couldn't get display resolutions");
        }

        add(_displaySurface);
        add(_txtDisplayResolution, "text", "videoMenu");
        add(_txtDisplayWidth, "resolution", "videoMenu");
        add(_txtDisplayX, "resolution", "videoMenu");
        add(_txtDisplayHeight, "resolution", "videoMenu");
        add(_btnDisplayResolutionUp, "button", "videoMenu");
        add(_btnDisplayResolutionDown, "button", "videoMenu");

        add(_txtLanguage, "text", "videoMenu");
        add(_txtFilter, "text", "videoMenu");

        add(_txtMode, "text", "videoMenu");

        add(_txtOptions, "text", "videoMenu");
        add(_btnLetterbox, "button", "videoMenu");
        add(_btnLockMouse, "button", "videoMenu");
        add(_btnRootWindowedMode, "button", "videoMenu");

        add(_cbxFilter, "button", "videoMenu");
        add(_cbxDisplayMode, "button", "videoMenu");

        add(_txtBattleScale, "text", "videoMenu");
        add(_cbxBattleScale, "button", "videoMenu");

        add(_txtGeoScale, "text", "videoMenu");
        add(_cbxGeoScale, "button", "videoMenu");

        add(_cbxLanguage, "button", "videoMenu");
        centerAllSurfaces();

        // Set up objects
        _txtDisplayResolution.setText(tr("STR_DISPLAY_RESOLUTION"));

        _displaySurface.setTooltip("STR_DISPLAY_RESOLUTION_DESC");
        _displaySurface.onMouseIn(txtTooltipIn);
        _displaySurface.onMouseOut(txtTooltipOut);

        _txtDisplayWidth.setAlign(TextHAlign.ALIGN_CENTER);
        _txtDisplayWidth.setBig();
        _txtDisplayWidth.setConstraint(TextEditConstraint.TEC_NUMERIC_POSITIVE);
        _txtDisplayWidth.onChange(txtDisplayWidthChange);

        _txtDisplayX.setAlign(TextHAlign.ALIGN_CENTER);
        _txtDisplayX.setBig();
        _txtDisplayX.setText("x");

        _txtDisplayHeight.setAlign(TextHAlign.ALIGN_CENTER);
        _txtDisplayHeight.setBig();
        _txtDisplayHeight.setConstraint(TextEditConstraint.TEC_NUMERIC_POSITIVE);
        _txtDisplayHeight.onChange(txtDisplayHeightChange);

        string ssW = Options.displayWidth.ToString();
        string ssH = Options.displayHeight.ToString();
        _txtDisplayWidth.setText(ssW);
        _txtDisplayHeight.setText(ssH);

        _btnDisplayResolutionUp.onMouseClick(btnDisplayResolutionUpClick);
        _btnDisplayResolutionDown.onMouseClick(btnDisplayResolutionDownClick);

        _txtMode.setText(tr("STR_DISPLAY_MODE"));

        _txtOptions.setText(tr("STR_DISPLAY_OPTIONS"));

        _btnLetterbox.setText(tr("STR_LETTERBOXED"));
        _btnLetterbox.setPressed(Options.keepAspectRatio);
        _btnLetterbox.onMouseClick(btnLetterboxClick);
        _btnLetterbox.setTooltip("STR_LETTERBOXED_DESC");
        _btnLetterbox.onMouseIn(txtTooltipIn);
        _btnLetterbox.onMouseOut(txtTooltipOut);

        _btnLockMouse.setText(tr("STR_LOCK_MOUSE"));
        _btnLockMouse.setPressed(Options.captureMouse == SDL_bool.SDL_TRUE /* SDL_GRAB_ON */);
        _btnLockMouse.onMouseClick(btnLockMouseClick);
        _btnLockMouse.setTooltip("STR_LOCK_MOUSE_DESC");
        _btnLockMouse.onMouseIn(txtTooltipIn);
        _btnLockMouse.onMouseOut(txtTooltipOut);

        _btnRootWindowedMode.setText(tr("STR_FIXED_WINDOW_POSITION"));
        _btnRootWindowedMode.setPressed(Options.rootWindowedMode);
        _btnRootWindowedMode.onMouseClick(btnRootWindowedModeClick);
        _btnRootWindowedMode.setTooltip("STR_FIXED_WINDOW_POSITION_DESC");
        _btnRootWindowedMode.onMouseIn(txtTooltipIn);
        _btnRootWindowedMode.onMouseOut(txtTooltipOut);

        _txtLanguage.setText(tr("STR_DISPLAY_LANGUAGE"));

        var names = new List<string>();
        Language.getList(_langs, names);
        _cbxLanguage.setOptions(names);
        for (int i = 0; i < names.Count; ++i)
        {
            if (_langs[i] == Options.language)
            {
                _cbxLanguage.setSelected((uint)i);
                break;
            }
        }
        _cbxLanguage.onChange(cbxLanguageChange);
        _cbxLanguage.setTooltip("STR_DISPLAY_LANGUAGE_DESC");
        _cbxLanguage.onMouseIn(txtTooltipIn);
        _cbxLanguage.onMouseOut(txtTooltipOut);

        var filterNames = new List<string>
        {
            tr("STR_DISABLED"),
            "Scale",
            "HQx",
            "xBRZ"
        };
        _filters.Add(string.Empty);
        _filters.Add(string.Empty);
        _filters.Add(string.Empty);
        _filters.Add(string.Empty);

#if !__NO_OPENGL
        HashSet<string> filters = FileMap.filterFiles(FileMap.getVFolderContents(GL_FOLDER), GL_EXT);
        foreach (var i in filters)
        {
            string file = i;
            string path = GL_FOLDER + file;
            string name = file.Substring(0, file.Length - GL_EXT.Length - 1) + GL_STRING;
            filterNames.Add(ucWords(name));
            _filters.Add(path);
        }
#endif

        uint selFilter = 0;
        if (Screen.useOpenGL())
        {
#if !__NO_OPENGL
            string path = Options.useOpenGLShader;
            for (int i = 0; i < _filters.Count; ++i)
            {
                if (_filters[i] == path)
                {
                    selFilter = (uint)i;
                    break;
                }
            }
#endif
        }
        else if (Options.useScaleFilter)
        {
            selFilter = 1;
        }
        else if (Options.useHQXFilter)
        {
            selFilter = 2;
        }
        else if (Options.useXBRZFilter)
        {
            selFilter = 3;
        }

        _txtFilter.setText(tr("STR_DISPLAY_FILTER"));

        _cbxFilter.setOptions(filterNames);
        _cbxFilter.setSelected(selFilter);
        _cbxFilter.onChange(cbxFilterChange);
        _cbxFilter.setTooltip("STR_DISPLAY_FILTER_DESC");
        _cbxFilter.onMouseIn(txtTooltipIn);
        _cbxFilter.onMouseOut(txtTooltipOut);

        var displayModes = new List<string>
        {
            tr("STR_WINDOWED"),
            tr("STR_FULLSCREEN"),
            tr("STR_BORDERLESS"),
            tr("STR_RESIZABLE")
        };

        int displayMode = 0;
        if (Options.fullscreen)
        {
            displayMode = 1;
        }
        else if (Options.borderless)
        {
            displayMode = 2;
        }
        else if (Options.allowResize)
        {
            displayMode = 3;
        }

        _cbxDisplayMode.setOptions(displayModes);
        _cbxDisplayMode.setSelected((uint)displayMode);
        _cbxDisplayMode.onChange(updateDisplayMode);
        _cbxDisplayMode.setTooltip("STR_DISPLAY_MODE_DESC");
        _cbxDisplayMode.onMouseIn(txtTooltipIn);
        _cbxDisplayMode.onMouseOut(txtTooltipOut);

        _txtGeoScale.setText(tr("STR_GEOSCAPE_SCALE"));

        var scales = new List<string>
        {
            tr("STR_ORIGINAL"),
            tr("STR_1_5X"),
            tr("STR_2X"),
            tr("STR_THIRD_DISPLAY"),
            tr("STR_HALF_DISPLAY"),
            tr("STR_FULL_DISPLAY")
        };

        _cbxGeoScale.setOptions(scales);
        _cbxGeoScale.setSelected((uint)Options.geoscapeScale);
        _cbxGeoScale.onChange(updateGeoscapeScale);
        _cbxGeoScale.setTooltip("STR_GEOSCAPESCALE_SCALE_DESC");
        _cbxGeoScale.onMouseIn(txtTooltipIn);
        _cbxGeoScale.onMouseOut(txtTooltipOut);

        _txtBattleScale.setText(tr("STR_BATTLESCAPE_SCALE"));

        _cbxBattleScale.setOptions(scales);
        _cbxBattleScale.setSelected((uint)Options.battlescapeScale);
        _cbxBattleScale.onChange(updateBattlescapeScale);
        _cbxBattleScale.setTooltip("STR_BATTLESCAPE_SCALE_DESC");
        _cbxBattleScale.onMouseIn(txtTooltipIn);
        _cbxBattleScale.onMouseOut(txtTooltipOut);
    }

    /**
     *
     */
    ~OptionsVideoState() { }

    /**
     * Changes the Display Width option.
     * @param action Pointer to an action.
     */
    void txtDisplayWidthChange(Engine.Action _)
    {
        string ss;
        int width = 0;
        ss = _txtDisplayWidth.getText();
        width = int.Parse(ss);
        Options.newDisplayWidth = width;
        // Update resolution mode
        if (_res != null)
        {
            int i;
            _resCurrent = -1;
            for (i = 0; i < _res.Length; ++i)
            {
                if (_resCurrent == -1 &&
                    ((_res[i].w == Options.newDisplayWidth && _res[i].h <= Options.newDisplayHeight) || _res[i].w < Options.newDisplayWidth))
                {
                    _resCurrent = i;
                }
            }
        }
    }

    /**
     * Changes the Display Height option.
     * @param action Pointer to an action.
     */
    void txtDisplayHeightChange(Engine.Action _)
    {
        string ss;
        int height = 0;
        ss = _txtDisplayHeight.getText();
        height = int.Parse(ss);
        Options.newDisplayHeight = height;
        // Update resolution mode
        if (_res != null)
        {
            int i;
            _resCurrent = -1;
            for (i = 0; i < _res.Length; ++i)
            {
                if (_resCurrent == -1 &&
                    ((_res[i].w == Options.newDisplayWidth && _res[i].h <= Options.newDisplayHeight) || _res[i].w < Options.newDisplayWidth))
                {
                    _resCurrent = i;
                }
            }
        }
    }

    /**
     * Changes the Display Mode options.
     * @param action Pointer to an action.
     */
    void updateDisplayMode(Engine.Action _)
    {
        switch (_cbxDisplayMode.getSelected())
        {
            case 0:
                Options.newFullscreen = false;
                Options.newBorderless = false;
                Options.newAllowResize = false;
                break;
            case 1:
                Options.newFullscreen = true;
                Options.newBorderless = false;
                Options.newAllowResize = false;
                break;
            case 2:
                Options.newFullscreen = false;
                Options.newBorderless = true;
                Options.newAllowResize = false;
                break;
            case 3:
                Options.newFullscreen = false;
                Options.newBorderless = false;
                Options.newAllowResize = true;
                break;
            default:
                break;
        }
    }

    /**
     * Changes the geoscape scale.
     * @param action Pointer to an action.
     */
    void updateGeoscapeScale(Engine.Action _) =>
        Options.newGeoscapeScale = (int)_cbxGeoScale.getSelected();

    /**
     * Updates the Battlescape scale.
     * @param action Pointer to an action.
     */
    void updateBattlescapeScale(Engine.Action _) =>
        Options.newBattlescapeScale = (int)_cbxBattleScale.getSelected();

    /**
     * Changes the Filter options.
     * @param action Pointer to an action.
     */
    void cbxFilterChange(Engine.Action _)
    {
        switch (_cbxFilter.getSelected())
        {
            case 0:
                Options.newOpenGL = false;
                Options.newScaleFilter = false;
                Options.newHQXFilter = false;
                Options.newXBRZFilter = false;
                break;
            case 1:
                Options.newOpenGL = false;
                Options.newScaleFilter = true;
                Options.newHQXFilter = false;
                Options.newXBRZFilter = false;
                break;
            case 2:
                Options.newOpenGL = false;
                Options.newScaleFilter = false;
                Options.newHQXFilter = true;
                Options.newXBRZFilter = false;
                break;
            case 3:
                Options.newOpenGL = false;
                Options.newScaleFilter = false;
                Options.newHQXFilter = false;
                Options.newXBRZFilter = true;
                break;
            default:
                Options.newOpenGL = true;
                Options.newScaleFilter = false;
                Options.newHQXFilter = false;
                Options.newXBRZFilter = false;
                Options.newOpenGLShader = _filters[(int)_cbxFilter.getSelected()];
                break;
        }
    }

    /**
     * Changes the Language option.
     * @param action Pointer to an action.
     */
    void cbxLanguageChange(Engine.Action _) =>
        Options.language = _langs[(int)_cbxLanguage.getSelected()];

    /**
     * Selects a bigger display resolution.
     * @param action Pointer to an action.
     */
    void btnDisplayResolutionUpClick(Engine.Action _)
    {
        if (_resAmount == 0)
            return;
        if (_resCurrent <= 0)
        {
            _resCurrent = _resAmount - 1;
        }
        else
        {
            _resCurrent--;
        }
        updateDisplayResolution();
    }

    /**
     * Selects a smaller display resolution.
     * @param action Pointer to an action.
     */
    void btnDisplayResolutionDownClick(Engine.Action _)
    {
        if (_resAmount == 0)
            return;
        if (_resCurrent >= _resAmount - 1)
        {
            _resCurrent = 0;
        }
        else
        {
            _resCurrent++;
        }
        updateDisplayResolution();
    }

    /**
     * Updates the display resolution based on the selection.
     */
    void updateDisplayResolution()
    {
        string ssW = _res[_resCurrent].w.ToString();
        string ssH = _res[_resCurrent].h.ToString();
        _txtDisplayWidth.setText(ssW);
        _txtDisplayHeight.setText(ssH);

        Options.newDisplayWidth = _res[_resCurrent].w;
        Options.newDisplayHeight = _res[_resCurrent].h;
    }

    /**
     * Changes the Letterboxing option.
     * @param action Pointer to an action.
     */
    void btnLetterboxClick(Engine.Action _) =>
        Options.keepAspectRatio = _btnLetterbox.getPressed();

    /**
     * Changes the Lock Mouse option.
     * @param action Pointer to an action.
     */
    void btnLockMouseClick(Engine.Action _)
    {
        Options.captureMouse = _btnLockMouse.getPressed() ? SDL_bool.SDL_TRUE : SDL_bool.SDL_FALSE; //SDL_GrabMode
        SDL_SetRelativeMouseMode(Options.captureMouse); //SDL_WM_GrabInput(Options.captureMouse);
    }

    /**
     * Ask user where he wants to root screen.
     * @param action Pointer to an action.
     */
    void btnRootWindowedModeClick(Engine.Action _)
    {
        if (_btnRootWindowedMode.getPressed())
        {
            _game.pushState(new SetWindowedRootState(_origin, this));
        }
        else
        {
            Options.newRootWindowedMode = false;
        }
    }

    /**
     * Uppercases all the words in a string.
     * @param text Source string.
     * @return Destination string.
     */
    string ucWords(string text)
    {
        var str = text.ToCharArray();
        for (int i = 0; i < str.Length; ++i)
        {
            if (i == 0)
                str[0] = char.ToUpper(str[0]);
            else if (str[i] == ' ' || str[i] == '-' || str[i] == '_')
            {
                str[i] = ' ';
                if (str.Length > i + 1)
                    str[i + 1] = char.ToUpper(str[i + 1]);
            }
        }
        return new string(str);
    }
}
