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

enum OptionsOrigin
{
    OPT_MENU,
    OPT_GEOSCAPE,
    OPT_BATTLESCAPE
}

/**
 * Options base state for common stuff
 * across Options windows.
 */
internal class OptionsBaseState : State
{
    protected OptionsOrigin _origin;
    protected TextButton _group;
    protected Window _window;
    protected TextButton _btnVideo, _btnAudio, _btnControls, _btnGeoscape, _btnBattlescape, _btnAdvanced;
    protected TextButton _btnOk, _btnCancel, _btnDefault;
    protected Text _txtTooltip;
    protected string _currentTooltip;

    /**
     * Initializes all the elements in the Options window.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     */
    protected OptionsBaseState(OptionsOrigin origin)
    {
        _origin = origin;
        _group = null;

        // Create objects
        _window = new Window(this, 320, 200, 0, 0);

        _btnVideo = new TextButton(80, 16, 8, 8);
        _btnAudio = new TextButton(80, 16, 8, 28);
        _btnControls = new TextButton(80, 16, 8, 48);
        _btnGeoscape = new TextButton(80, 16, 8, 68);
        _btnBattlescape = new TextButton(80, 16, 8, 88);
        _btnAdvanced = new TextButton(80, 16, 8, 108);

        _btnOk = new TextButton(100, 16, 8, 176);
        _btnCancel = new TextButton(100, 16, 110, 176);
        _btnDefault = new TextButton(100, 16, 212, 176);

        _txtTooltip = new Text(305, 25, 8, 148);

        // Set palette
        setInterface("optionsMenu", false, _game.getSavedGame() != null ? _game.getSavedGame().getSavedBattle() : null);

        add(_window, "window", "optionsMenu");

        add(_btnVideo, "button", "optionsMenu");
        add(_btnAudio, "button", "optionsMenu");
        add(_btnControls, "button", "optionsMenu");
        add(_btnGeoscape, "button", "optionsMenu");
        add(_btnBattlescape, "button", "optionsMenu");
        add(_btnAdvanced, "button", "optionsMenu");

        add(_btnOk, "button", "optionsMenu");
        add(_btnCancel, "button", "optionsMenu");
        add(_btnDefault, "button", "optionsMenu");

        add(_txtTooltip, "tooltip", "optionsMenu");

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

        _btnVideo.setText(tr("STR_VIDEO"));
        _btnVideo.onMousePress(btnGroupPress, (byte)SDL_BUTTON_LEFT);

        _btnAudio.setText(tr("STR_AUDIO"));
        _btnAudio.onMousePress(btnGroupPress, (byte)SDL_BUTTON_LEFT);

        _btnControls.setText(tr("STR_CONTROLS"));
        _btnControls.onMousePress(btnGroupPress, (byte)SDL_BUTTON_LEFT);

        _btnGeoscape.setText(tr("STR_GEOSCAPE_UC"));
        _btnGeoscape.onMousePress(btnGroupPress, (byte)SDL_BUTTON_LEFT);

        _btnBattlescape.setText(tr("STR_BATTLESCAPE_UC"));
        _btnBattlescape.onMousePress(btnGroupPress, (byte)SDL_BUTTON_LEFT);

        _btnAdvanced.setText(tr("STR_ADVANCED"));
        _btnAdvanced.onMousePress(btnGroupPress, (byte)SDL_BUTTON_LEFT);

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

        _btnCancel.setText(tr("STR_CANCEL"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        _btnDefault.setText(tr("STR_RESTORE_DEFAULTS"));
        _btnDefault.onMouseClick(btnDefaultClick);

        _txtTooltip.setWordWrap(true);
    }

    /**
     *
     */
    ~OptionsBaseState() { }

    void btnGroupPress(Engine.Action action)
    {
        Surface sender = action.getSender();
        //if (sender != _group)
        {
            _game.popState();
            if (sender == _btnVideo)
            {
                _game.pushState(new OptionsVideoState(_origin));
            }
            else if (sender == _btnAudio)
            {
                if (!Options.mute)
                {
                    _game.pushState(new OptionsAudioState(_origin));
                }
                else
                {
                    _game.pushState(new OptionsNoAudioState(_origin));
                }
            }
            else if (sender == _btnControls)
            {
                _game.pushState(new OptionsControlsState(_origin));
            }
            else if (sender == _btnGeoscape)
            {
                _game.pushState(new OptionsGeoscapeState(_origin));
            }
            else if (sender == _btnBattlescape)
            {
                _game.pushState(new OptionsBattlescapeState(_origin));
            }
            else if (sender == _btnAdvanced)
            {
                _game.pushState(new OptionsAdvancedState(_origin));
            }
        }
    }

    /**
     * Saves the new options and returns to the proper origin screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Engine.Action _)
    {
        Options.switchDisplay();
        int dX = Options.baseXResolution;
        int dY = Options.baseYResolution;
        Screen.updateScale(Options.battlescapeScale, ref Options.baseXBattlescape, ref Options.baseYBattlescape, _origin == OptionsOrigin.OPT_BATTLESCAPE);
        Screen.updateScale(Options.geoscapeScale, ref Options.baseXGeoscape, ref Options.baseYGeoscape, _origin != OptionsOrigin.OPT_BATTLESCAPE);
        dX = Options.baseXResolution - dX;
        dY = Options.baseYResolution - dY;
        recenter(dX, dY);
        Options.save();
        if (Options.reload && _origin == OptionsOrigin.OPT_MENU)
        {
            Options.mapResources();
        }
        _game.loadLanguages();
        SDL_SetRelativeMouseMode(Options.captureMouse); //SDL_WM_GrabInput(Options.captureMouse);
        _game.getScreen().resetDisplay();
        _game.setVolume(Options.soundVolume, Options.musicVolume, Options.uiVolume);
        if (Options.reload && _origin == OptionsOrigin.OPT_MENU)
        {
            _game.setState(new StartState());
        }
        else
        {
            // Confirm any video options changes
            if (Options.displayWidth != Options.newDisplayWidth ||
                Options.displayHeight != Options.newDisplayHeight ||
                Options.useOpenGL != Options.newOpenGL ||
                Options.useScaleFilter != Options.newScaleFilter ||
                Options.useHQXFilter != Options.newHQXFilter ||
                Options.useOpenGLShader != Options.newOpenGLShader)
            {
                _game.pushState(new OptionsConfirmState(_origin));
            }
            else
            {
                restart(_origin);
            }
        }
    }

    void restart(OptionsOrigin origin)
    {
        if (origin == OptionsOrigin.OPT_MENU)
        {
            _game.setState(new MainMenuState());
        }
        else if (origin == OptionsOrigin.OPT_GEOSCAPE)
        {
            _game.setState(new GeoscapeState());
        }
        else if (origin == OptionsOrigin.OPT_BATTLESCAPE)
        {
            _game.setState(new GeoscapeState());
            BattlescapeState bs = new BattlescapeState();
            _game.pushState(bs);
            _game.getSavedGame().getSavedBattle().setBattleState(bs);
        }
    }

    /**
     * Loads previous options and returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Engine.Action _)
    {
        Options.reload = false;
        Options.load();
        SDL_SetRelativeMouseMode(Options.captureMouse); //SDL_WM_GrabInput(Options.captureMouse);
        Screen.updateScale(Options.battlescapeScale, ref Options.baseXBattlescape, ref Options.baseYBattlescape, _origin == OptionsOrigin.OPT_BATTLESCAPE);
        Screen.updateScale(Options.geoscapeScale, ref Options.baseXGeoscape, ref Options.baseYGeoscape, _origin != OptionsOrigin.OPT_BATTLESCAPE);
        _game.setVolume(Options.soundVolume, Options.musicVolume, Options.uiVolume);
        _game.popState();
    }

    /**
     * Restores the Options to default settings.
     * @param action Pointer to an action.
     */
    void btnDefaultClick(Engine.Action _) =>
        _game.pushState(new OptionsDefaultsState(_origin, this));

    /**
     * Handles the pressed-button state for the category buttons.
     * @param button Button to press.
     */
    protected void setCategory(TextButton button)
    {
        _group = button;
        _btnVideo.setGroup(ref _group);
        _btnAudio.setGroup(ref _group);
        _btnControls.setGroup(ref _group);
        _btnGeoscape.setGroup(ref _group);
        _btnBattlescape.setGroup(ref _group);
        _btnAdvanced.setGroup(ref _group);
    }

    /**
     * Shows a tooltip for the appropriate button.
     * @param action Pointer to an action.
     */
    protected void txtTooltipIn(Engine.Action action)
    {
        _currentTooltip = action.getSender().getTooltip();
        _txtTooltip.setText(tr(_currentTooltip));
    }

    /**
     * Clears the tooltip text.
     * @param action Pointer to an action.
     */
    protected void txtTooltipOut(Engine.Action action)
    {
        if (_currentTooltip == action.getSender().getTooltip())
        {
            _txtTooltip.setText(string.Empty);
        }
    }
}
