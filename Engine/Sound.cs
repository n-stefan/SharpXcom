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
 * Container for sound effects.
 * Handles loading and playing various formats through SDL_mixer.
 */
internal class Sound
{
    unsafe MIX_Audio* _sound;

    /**
     * Initializes a new sound effect.
     */
    unsafe internal Sound() =>
        _sound = null;

    /**
     * Deletes the loaded sound content.
     */
    unsafe ~Sound() =>
        MIX_DestroyAudio(_sound);

    /**
     * Stops all sounds playing.
     */
    unsafe internal static void stop()
    {
        if (!Options.mute)
        {
            for (var i = 1; i < Game.Tracks.Length; i++) MIX_StopTrack(Game.Tracks[i], 0);
        }
    }

    /**
     * Plays the contained sound effect.
     * @param channel Use specified channel, -1 to use any channel
     */
    unsafe internal void play(int channel = -1, int angle = 0, int distance = 0)
    {
        if (!Options.mute && _sound != null)
        {
            MIX_SetTrackAudio(Game.Tracks[channel], _sound);
            SDL_PropertiesID props = MIX_GetAudioProperties(_sound);
            SDLBool success = MIX_PlayTrack(Game.Tracks[channel], props);
            SDL_DestroyProperties(props);
            if (!success)
            {
                Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {SDL_GetError()}");
            }
            else if (Options.StereoSound)
            {
                float radians = angle * (SDL_PI_F / 180.0f);
                MIX_Point3D position;
                position.x = SDL_cosf(radians) * (float)distance;
                position.y = 0.0f;
                position.z = SDL_sinf(radians) * (float)distance;
                if (!MIX_SetTrack3DPosition(Game.Tracks[channel], &position))
                {
                    Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {SDL_GetError()}");
                }
            }
        }
    }

    /**
     * Loads a sound file from a specified memory chunk.
     * @param data Pointer to the sound file in memory
     * @param size Size of the sound file in bytes.
     */
    unsafe internal void load(byte[] data, uint size)
    {
        nint dataPtr = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, dataPtr, data.Length);
        SDL_IOStream* io = SDL_IOFromConstMem(dataPtr, size);
        _sound = MIX_LoadAudio_IO(Game.Mixer, io, true, true);
        if (_sound == null)
        {
            throw new Exception(SDL_GetError());
        }
    }

    /**
     * Loads a sound file from a specified filename.
     * @param filename Filename of the sound file.
     */
    unsafe internal void load(string filename)
    {
        string utf8 = Unicode.convPathToUtf8(filename);
        _sound = MIX_LoadAudio(Game.Mixer, utf8, true);
        if (_sound == null)
        {
            string err = filename + ":" + SDL_GetError();
            throw new Exception(err);
        }
    }

    /**
     * Stops the contained sound from looping.
     */
    unsafe internal void stopLoop()
    {
        if (!Options.mute)
        {
            MIX_StopTrack(Game.Tracks[3], 0);
        }
    }

    /**
     * Plays the contained sound effect repeatedly on the reserved ambience channel.
     */
    unsafe internal void loop()
    {
        if (!Options.mute && _sound != null && !MIX_TrackPlaying(Game.Tracks[3]))
        {
            MIX_SetTrackAudio(Game.Tracks[3], _sound);
            SDL_PropertiesID props = MIX_GetAudioProperties(_sound);
            SDLBool success = MIX_PlayTrack(Game.Tracks[3], props);
            SDL_DestroyProperties(props);
            if (!success)
            {
                Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {SDL_GetError()}");
            }
        }
    }
}
