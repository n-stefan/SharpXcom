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

/**
 * Container for music tracks.
 * Handles loading and playing various formats through SDL_mixer.
 */
internal class Music
{
    unsafe static MIX_Audio* _music;

    /**
     * Initializes a new music track.
     */
    unsafe internal Music() =>
        _music = null;

    /**
     * Deletes the loaded music content.
     */
    unsafe ~Music()
    {
#if !__NO_MUSIC
        stop();
        MIX_DestroyAudio(_music);
#endif
    }

    /**
     * Stops all music playing.
     */
    unsafe internal static void stop()
    {
#if !__NO_MUSIC
        if (!Options.mute)
        {
            func_mute();
            MIX_SetTrackAudioStream(Game.Tracks[0], null);
            MIX_StopTrack(Game.Tracks[0], 0);
        }
#endif
    }

    /**
     * Plays the contained music track.
     * @param loop Amount of times to loop the track. -1 = infinite
     */
    unsafe internal virtual void play(int loop = -1)
    {
#if !__NO_MUSIC
        if (!Options.mute)
        {
            if (_music != null)
            {
                stop();
                MIX_SetTrackAudio(Game.Tracks[0], _music);
                SDL_PropertiesID props = SDL_CreateProperties();
                SDL_SetNumberProperty(props, MIX_PROP_PLAY_LOOPS_NUMBER, loop);
                if (!MIX_PlayTrack(Game.Tracks[0], props))
                {
                    Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {SDL_GetError()}");
                }
                SDL_DestroyProperties(props);
            }
        }
#endif
    }

    /**
     * Loads a music file from a specified memory chunk.
     * @param data Pointer to the music file in memory
     * @param size Size of the music file in bytes.
     */
    unsafe internal virtual void load(byte[] data, int size)
    {
#if !__NO_MUSIC
        nint dataPtr = Marshal.AllocHGlobal(size);
        Marshal.Copy(data, 0, dataPtr, size);
        SDL_IOStream* iostream = SDL_IOFromConstMem(dataPtr, (nuint)size);
        _music = MIX_LoadAudio_IO(Game.Mixer, iostream, true, true);
        SDL_CloseIO(iostream);
        if (_music == null)
        {
            throw new Exception(SDL_GetError());
        }
#endif
    }

    /**
     * Loads a music file from a specified filename.
     * @param filename Filename of the music file.
     */
    unsafe internal virtual void load(string filename)
    {
#if !__NO_MUSIC
        string utf8 = Unicode.convPathToUtf8(filename);
        _music = MIX_LoadAudio(Game.Mixer, utf8, true);
        if (_music == null)
        {
            throw new Exception(SDL_GetError());
        }
#endif
    }

    /**
     * Pauses music playback when game loses focus.
     */
    unsafe static void pause()
    {
#if !__NO_MUSIC
        if (!Options.mute)
        {
            MIX_PauseTrack(Game.Tracks[0]);
            if (GetMusicType() == "NONE")
                MIX_SetTrackAudioStream(Game.Tracks[0], null);
        }
#endif
    }

    /**
     * Resumes music playback when game gains focus.
     */
    unsafe static void resume()
    {
#if !__NO_MUSIC
        if (!Options.mute)
        {
            MIX_ResumeTrack(Game.Tracks[0]);
            if (GetMusicType() == "NONE")
            {
                SDL_AudioStream* stream = SDL_CreateAudioStream(null, null);
                SDL_SetAudioStreamGetCallback(stream, &AdlibMusic.player, nint.Zero);
                MIX_SetTrackAudioStream(Game.Tracks[0], stream);
            }
        }
#endif
    }

    /**
     * Checks if any music is playing.
     */
    unsafe static bool isPlaying()
    {
#if !__NO_MUSIC
        if (!Options.mute)
        {
            return MIX_TrackPlaying(Game.Tracks[0]);
        }
#endif
        return false;
    }

    unsafe internal static string GetMusicType()
    {
        SDL_PropertiesID props = MIX_GetAudioProperties(_music);
        string musicType = SDL_GetStringProperty(props, MIX_PROP_AUDIO_DECODER_STRING, null);
        SDL_DestroyProperties(props);
        return musicType;
    }
}
