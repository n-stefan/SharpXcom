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
    MIX_Chunk _sound;

    /**
     * Initializes a new sound effect.
     */
    internal Sound() =>
        _sound = default;

    /**
     * Deletes the loaded sound content.
     */
    ~Sound() =>
        Mix_FreeChunk(_sound.abuf);

    /**
     * Stops all sounds playing.
     */
    internal static void stop()
    {
        if (!Options.mute)
        {
            Mix_HaltChannel(-1);
        }
    }

    /**
     * Plays the contained sound effect.
     * @param channel Use specified channel, -1 to use any channel
     */
    internal void play(int channel = -1, int angle = 0, int distance = 0)
    {
	    if (!Options.mute && _sound.abuf != nint.Zero)
 	    {
            int chan = Mix_PlayChannel(channel, _sound.abuf, 0);
		    if (chan == -1)
		    {
                Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {Mix_GetError()}");
            }
            else if (Options.StereoSound)
		    {
                if (Mix_SetPosition(chan, (short)angle, (byte)distance) == 0)
			    {
                    Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {Mix_GetError()}");
                }
            }
	    }
    }

    /**
     * Loads a sound file from a specified memory chunk.
     * @param data Pointer to the sound file in memory
     * @param size Size of the sound file in bytes.
     */
    internal void load(byte[] data, uint size)
    {
        nint dataPtr = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, dataPtr, data.Length);
        nint rw = SDL_RWFromConstMem(dataPtr, (int)size);
        _sound.abuf = Mix_LoadWAV_RW(rw, 1);
        if (_sound.abuf == nint.Zero)
        {
            throw new Exception(Mix_GetError());
        }
    }

    /**
     * Loads a sound file from a specified filename.
     * @param filename Filename of the sound file.
     */
    internal void load(string filename)
    {
	    string utf8 = Unicode.convPathToUtf8(filename);
	    _sound.abuf = Mix_LoadWAV(utf8);
	    if (_sound.abuf == nint.Zero)
	    {
		    string err = filename + ":" + Mix_GetError();
		    throw new Exception(err);
	    }
    }
}
