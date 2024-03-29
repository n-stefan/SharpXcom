﻿/*
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
 * Audio options.
 */
internal class OptionsAudioState : OptionsBaseState
{
    Text _txtMusicVolume, _txtSoundVolume, _txtUiVolume;
    Slider _slrMusicVolume, _slrSoundVolume, _slrUiVolume;
    Text _txtMusicFormat, _txtCurrentMusic, _txtSoundFormat, _txtCurrentSound, _txtVideoFormat;
    ComboBox _cbxMusicFormat, _cbxSoundFormat, _cbxVideoFormat;
    Text _txtOptions;
    ToggleTextButton _btnBackgroundMute;
    /* MUS_NONE, MUS_CMD, MUS_WAV, MUS_MOD, MUS_MID, MUS_OGG, MUS_MP3, MUS_MP3_MAD, MUS_FLAC, MUS_MODPLUG */
    static string[] musFormats = { "Adlib", "?", "WAV", "MOD", "MIDI", "OGG", "MP3", "MP3", "FLAC", "MOD" };
    static string[] sndFormats = { "?", "1.4", "1.0" };

    /**
     * Initializes all the elements in the Audio Options screen.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     */
    internal OptionsAudioState(OptionsOrigin origin) : base(origin)
    {
        setCategory(_btnAudio);

        // Create objects
        _txtMusicVolume = new Text(114, 9, 94, 8);
        _slrMusicVolume = new Slider(104, 16, 94, 18);

        _txtSoundVolume = new Text(114, 9, 94, 40);
        _slrSoundVolume = new Slider(104, 16, 94, 50);

        _txtUiVolume = new Text(114, 9, 94, 72);
        _slrUiVolume = new Slider(104, 16, 94, 82);

        _txtMusicFormat = new Text(114, 9, 206, 40);
        _cbxMusicFormat = new ComboBox(this, 104, 16, 206, 50);
        _txtCurrentMusic = new Text(114, 9, 206, 68);

        _txtSoundFormat = new Text(114, 9, 206, 82);
        _cbxSoundFormat = new ComboBox(this, 104, 16, 206, 92);
        _txtCurrentSound = new Text(114, 9, 206, 110);

        _txtVideoFormat = new Text(114, 9, 206, 8);
        _cbxVideoFormat = new ComboBox(this, 104, 16, 206, 18);

        _txtOptions = new Text(114, 9, 94, 104);
        _btnBackgroundMute = new ToggleTextButton(104, 16, 94, 114);

        add(_txtMusicVolume, "text", "audioMenu");
        add(_slrMusicVolume, "button", "audioMenu");

        add(_txtSoundVolume, "text", "audioMenu");
        add(_slrSoundVolume, "button", "audioMenu");

        add(_txtUiVolume, "text", "audioMenu");
        add(_slrUiVolume, "button", "audioMenu");

        add(_txtVideoFormat, "text", "audioMenu");
        add(_txtMusicFormat, "text", "audioMenu");
        add(_txtCurrentMusic, "text", "audioMenu");
        add(_txtSoundFormat, "text", "audioMenu");
        add(_txtCurrentSound, "text", "audioMenu");

        add(_cbxSoundFormat, "button", "audioMenu");
        add(_cbxMusicFormat, "button", "audioMenu");
        add(_cbxVideoFormat, "button", "audioMenu");

        add(_txtOptions, "text", "audioMenu");
        add(_btnBackgroundMute, "button", "audioMenu");

        centerAllSurfaces();

        // Set up objects
        _txtMusicVolume.setText(tr("STR_MUSIC_VOLUME"));

        _slrMusicVolume.setRange(0, SDL_MIX_MAXVOLUME);
        _slrMusicVolume.setValue(Options.musicVolume);
        _slrMusicVolume.onChange(slrMusicVolumeChange);
        _slrMusicVolume.setTooltip("STR_MUSIC_VOLUME_DESC");
        _slrMusicVolume.onMouseIn(txtTooltipIn);
        _slrMusicVolume.onMouseOut(txtTooltipOut);

        _txtSoundVolume.setText(tr("STR_SFX_VOLUME"));

        _slrSoundVolume.setRange(0, SDL_MIX_MAXVOLUME);
        _slrSoundVolume.setValue(Options.soundVolume);
        _slrSoundVolume.onChange(slrSoundVolumeChange);
        _slrSoundVolume.onMouseRelease(slrSoundVolumeRelease);
        _slrSoundVolume.setTooltip("STR_SFX_VOLUME_DESC");
        _slrSoundVolume.onMouseIn(txtTooltipIn);
        _slrSoundVolume.onMouseOut(txtTooltipOut);

        _txtUiVolume.setText(tr("STR_UI_VOLUME"));

        _slrUiVolume.setRange(0, SDL_MIX_MAXVOLUME);
        _slrUiVolume.setValue(Options.uiVolume);
        _slrUiVolume.onChange(slrUiVolumeChange);
        _slrUiVolume.onMouseRelease(slrUiVolumeRelease);
        _slrUiVolume.setTooltip("STR_UI_VOLUME_DESC");
        _slrUiVolume.onMouseIn(txtTooltipIn);
        _slrUiVolume.onMouseOut(txtTooltipOut);

        /* MUSIC_AUTO, MUSIC_FLAC, MUSIC_OGG, MUSIC_MP3, MUSIC_MOD, MUSIC_WAV, MUSIC_ADLIB, MUSIC_GM, MUSIC_MIDI */
        var musicText = new List<string>
        {
            tr("STR_PREFERRED_FORMAT_AUTO"),
            "FLAC",
            "OGG",
            "MP3",
            "MOD",
            "WAV",
            "Adlib",
            "GM",
            "MIDI"
        };

        var soundText = new List<string>
        {
            tr("STR_PREFERRED_FORMAT_AUTO"),
            "1.4",
            "1.0"
        };

        var videoText = new List<string>
        {
            tr("STR_PREFERRED_VIDEO_ANIMATION"),
            tr("STR_PREFERRED_VIDEO_SLIDESHOW")
        };

        _txtMusicFormat.setText(tr("STR_PREFERRED_MUSIC_FORMAT"));

        _cbxMusicFormat.setOptions(musicText);
        _cbxMusicFormat.setSelected((uint)Options.preferredMusic);
        _cbxMusicFormat.setTooltip("STR_PREFERRED_MUSIC_FORMAT_DESC");
        _cbxMusicFormat.onChange(cbxMusicFormatChange);
        _cbxMusicFormat.onMouseIn(txtTooltipIn);
        _cbxMusicFormat.onMouseOut(txtTooltipOut);

        string curMusic = musFormats[(int)Mix_GetMusicType(0)];
        _txtCurrentMusic.setText(tr("STR_CURRENT_FORMAT").arg(curMusic));

        _txtSoundFormat.setText(tr("STR_PREFERRED_SFX_FORMAT"));

        _cbxSoundFormat.setOptions(soundText);
        _cbxSoundFormat.setSelected((uint)Options.preferredSound);
        _cbxSoundFormat.setTooltip("STR_PREFERRED_SFX_FORMAT_DESC");
        _cbxSoundFormat.onChange(cbxSoundFormatChange);
        _cbxSoundFormat.onMouseIn(txtTooltipIn);
        _cbxSoundFormat.onMouseOut(txtTooltipOut);

        string curSound = sndFormats[(int)Options.currentSound];
        _txtCurrentSound.setText(tr("STR_CURRENT_FORMAT").arg(curSound));

        _txtVideoFormat.setText(tr("STR_PREFERRED_VIDEO_FORMAT"));

        _cbxVideoFormat.setOptions(videoText);
        _cbxVideoFormat.setSelected((uint)Options.preferredVideo);
        _cbxVideoFormat.setTooltip("STR_PREFERRED_VIDEO_FORMAT_DESC");
        _cbxVideoFormat.onChange(cbxVideoFormatChange);
        _cbxVideoFormat.onMouseIn(txtTooltipIn);
        _cbxVideoFormat.onMouseOut(txtTooltipOut);

        // These options require a restart, so don't enable them in-game
        _txtMusicFormat.setVisible(_origin == OptionsOrigin.OPT_MENU);
        _cbxMusicFormat.setVisible(_origin == OptionsOrigin.OPT_MENU);
        _txtCurrentMusic.setVisible(_origin == OptionsOrigin.OPT_MENU);

        // These options only apply to UFO
        _txtSoundFormat.setVisible(_origin == OptionsOrigin.OPT_MENU && !_game.getMod().getSoundDefinitions().Any());
        _cbxSoundFormat.setVisible(_origin == OptionsOrigin.OPT_MENU && !_game.getMod().getSoundDefinitions().Any());
        _txtCurrentSound.setVisible(_origin == OptionsOrigin.OPT_MENU && !_game.getMod().getSoundDefinitions().Any());

        _txtOptions.setText(tr("STR_SOUND_OPTIONS"));

        _btnBackgroundMute.setText(tr("STR_BACKGROUND_MUTE"));
        _btnBackgroundMute.setPressed(Options.backgroundMute);
        _btnBackgroundMute.onMouseClick(btnBackgroundMuteClick);
        _btnBackgroundMute.setTooltip("STR_BACKGROUND_MUTE_DESC");
        _btnBackgroundMute.onMouseIn(txtTooltipIn);
        _btnBackgroundMute.onMouseOut(txtTooltipOut);
    }

    /**
     *
     */
    ~OptionsAudioState() { }

    /**
     * Updates the music volume.
     * @param action Pointer to an action.
     */
    void slrMusicVolumeChange(Action _)
    {
        Options.musicVolume = _slrMusicVolume.getValue();
        _game.setVolume(Options.soundVolume, Options.musicVolume, Options.uiVolume);
    }

    /**
     * Updates the sound volume with the slider.
     * @param action Pointer to an action.
     */
    void slrSoundVolumeChange(Action _)
    {
        Options.soundVolume = _slrSoundVolume.getValue();
        _game.setVolume(Options.soundVolume, Options.musicVolume, Options.uiVolume);
    }

    /**
     * Plays a game sound for volume preview.
     * @param action Pointer to an action.
     */
    void slrSoundVolumeRelease(Action _) =>
        _game.getMod().getSound("GEO.CAT", (uint)Mod.Mod.UFO_FIRE).play();

    /**
     * Updates the UI volume with the slider.
     * @param action Pointer to an action.
     */
    void slrUiVolumeChange(Action _)
    {
        Options.uiVolume = _slrUiVolume.getValue();
        _game.setVolume(Options.soundVolume, Options.musicVolume, Options.uiVolume);
    }

    /**
     * Plays a UI sound for volume preview.
     * @param action Pointer to an action.
     */
    void slrUiVolumeRelease(Action _) =>
        TextButton.soundPress.play(Mix_GroupAvailable(0));

    /**
     * Changes the Music Format option.
     * @param action Pointer to an action.
     */
    void cbxMusicFormatChange(Action _)
    {
        Options.preferredMusic = (MusicFormat)_cbxMusicFormat.getSelected();
        Options.reload = true;
    }

    /**
     * Changes the Sound Format option.
     * @param action Pointer to an action.
     */
    void cbxSoundFormatChange(Action _)
    {
        Options.preferredSound = (SoundFormat)_cbxSoundFormat.getSelected();
        Options.reload = true;
    }

    /**
     * Changes the Video Format option.
     * @param action Pointer to an action.
     */
    void cbxVideoFormatChange(Action _) =>
        Options.preferredVideo = (VideoFormat)_cbxVideoFormat.getSelected();

    /**
     * Updates the Background Mute option.
     * @param action Pointer to an action.
     */
    void btnBackgroundMuteClick(Action _) =>
        Options.backgroundMute = _btnBackgroundMute.getPressed();
}
