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
    nint /* Mix_Music */ _music;

    /**
     * Initializes a new music track.
     */
    internal Music() =>
        _music = nint.Zero;

    /**
     * Deletes the loaded music content.
     */
    ~Music()
    {
#if !__NO_MUSIC
        stop();
        Mix_FreeMusic(_music);
#endif
    }

    /**
     * Stops all music playing.
     */
    internal static void stop()
    {
#if !__NO_MUSIC
        if (!Options.mute)
        {
            func_mute();
            Mix_HookMusic(null, nint.Zero);
            Mix_HaltMusic();
        }
#endif
    }

    /**
     * Plays the contained music track.
     * @param loop Amount of times to loop the track. -1 = infinite
     */
    protected virtual void play(int loop = -1)
    {
#if !__NO_MUSIC
        if (!Options.mute)
	    {
		    if (_music != nint.Zero)
		    {
			    stop();
			    if (Mix_PlayMusic(_music, loop) == -1)
			    {
                    Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {Mix_GetError()}");
			    }
		    }
	    }
#endif
    }

    /**
     * Loads a music file from a specified memory chunk.
     * @param data Pointer to the music file in memory
     * @param size Size of the music file in bytes.
     */
    internal void load(byte[] data, int size)
    {
#if !__NO_MUSIC
        nint dataPtr = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, dataPtr, data.Length);
        nint rwops = SDL_RWFromConstMem(dataPtr, size);
        _music = Mix_LoadMUS_RW(rwops);
        SDL_FreeRW(rwops);
        if (_music == nint.Zero)
        {
            throw new Exception(Mix_GetError());
        }
#endif
    }

    //TODO: Consolidate
    /**
     * Loads a music file from a specified memory chunk.
     * @param data Pointer to the music file in memory
     * @param size Size of the music file in bytes.
     */
    unsafe internal void load(List<byte> data, int size)
    {
#if !__NO_MUSIC
        nint dataPtr = Marshal.AllocHGlobal(data.Count);
        Unsafe.Copy((byte*)dataPtr, ref data);
        nint rwops = SDL_RWFromConstMem(dataPtr, size);
        _music = Mix_LoadMUS_RW(rwops);
        SDL_FreeRW(rwops);
        if (_music == nint.Zero)
        {
            throw new Exception(Mix_GetError());
        }
#endif
    }

    /**
     * Loads a music file from a specified filename.
     * @param filename Filename of the music file.
     */
    internal void load(string filename)
    {
#if !__NO_MUSIC
        string utf8 = Unicode.convPathToUtf8(filename);
	    _music = Mix_LoadMUS(utf8);
	    if (_music == nint.Zero)
	    {
		    throw new Exception(Mix_GetError());
	    }
#endif
    }
}
