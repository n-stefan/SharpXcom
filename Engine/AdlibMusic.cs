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
 * Container for Adlib music tracks.
 * Uses a custom YM3812 music player passed to SDL_mixer.
 */
internal class AdlibMusic : Music
{
    FM_OPL[] opl = new FM_OPL[2];

    byte[] _data;
    uint _size;
    float _volume;
    static int delay, rate;
    static Dictionary<int, int> delayRates;

    /**
     * Initializes a new music track.
     * @param volume Music volume modifier (1.0 = 100%).
     */
    internal AdlibMusic(float volume) : base()
    {
        _data = null;
        _size = 0;
        _volume = volume;

        rate = Options.audioSampleRate;
        if (opl[0] == default)
        {
            opl[0] = OPLCreate(OPL_TYPE_YM3812, 3579545, rate);
        }
        if (opl[1] == default)
        {
            opl[1] = OPLCreate(OPL_TYPE_YM3812, 3579545, rate);
        }
        // magic value - length of 1 tick per samplerate
        if (!delayRates.Any())
        {
            delayRates[8000] = 114 * 4;
            delayRates[11025] = 157 * 4;
            delayRates[16000] = 228 * 4;
            delayRates[22050] = 314 * 4;
            delayRates[32000] = 456 * 4;
            delayRates[44100] = 629 * 4;
            delayRates[48000] = 685 * 4;
        }
    }

    /**
     * Deletes the loaded music content.
     */
    ~AdlibMusic()
    {
        if (opl[0] != default)
        {
            stop();
            OPLDestroy(opl[0]);
            opl[0] = default;
        }
        if (opl[1] != default)
        {
            OPLDestroy(opl[1]);
            opl[1] = default;
        }
        _data = null;
    }

    /**
     * Loads a music file from a specified filename.
     * @param filename Filename of the music file.
     */
    internal override void load(string filename)
    {
        try
        {
            using var file = new FileStream(filename, FileMode.Open);
	    
	        //file.Seek(0, SeekOrigin.End);
	        //_size = (uint)file.Position;
	        //file.Seek(0, SeekOrigin.Begin);
            _size = (uint)file.Length;

	        _data = new byte[_size];
	        file.Read(_data);

            file.Close();
        }
        catch (Exception)
        {
		    throw new Exception(filename + " not found");
        }
    }

    /**
     * Loads a music file from a specified memory chunk.
     * @param data Pointer to the music file in memory
     * @param size Size of the music file in bytes.
     */
    internal override void load(byte[] data, int size)
    {
	    _data = data;
	    if (_data[0]<=56) size+=_data[0];
	    _size = (uint)size;
    }

    /**
     * Plays the contained music track.
     * @param loop Amount of times to loop the track. -1 = infinite
     */
    internal override void play(int loop = -1)
    {
#if !__NO_MUSIC
	    if (!Options.mute)
	    {
		    stop();
		    func_setup_music(_data, (int)_size);
		    func_set_music_volume((int)(127 * _volume));
		    Mix_HookMusic(player, nint.Zero /*(void*)this*/);
	    }
#endif
    }

    /**
     * Custom audio player.
     * @param udata User data to send to the player.
     * @param stream Raw audio to output.
     * @param len Length of audio to output.
     */
    unsafe void player(nint udata, nint stream, int len)
    {
#if !__NO_MUSIC
	    // Check SDL volume for Background Mute functionality
	    if (Options.musicVolume == 0 || Mix_VolumeMusic(-1) == 0)
		    return;
	    if (Options.musicAlwaysLoop && !func_is_music_playing())
	    {
		    //AdlibMusic *music = (AdlibMusic*)udata;
		    /*music.*/ play();
		    return;
	    }
	    while (len != 0)
	    {
		    if (opl[0] == default || opl[1] == default)
			    return;
		    int i = Math.Min(delay, len);
		    if (i != 0)
		    {
			    float volume = (float)Game.volumeExponent(Options.musicVolume);
                var buffer = new short[i / 2];
                Marshal.Copy(stream, buffer, 0, i);
			    YM3812UpdateOne(opl[0], buffer.AsSpan(), i / 2, 2, volume);
			    YM3812UpdateOne(opl[1], buffer.AsSpan(1), i / 2, 2, volume);
			    stream += i;
			    delay -= i;
			    len -= i;
		    }
		    if (len == 0)
			    return;
		    func_play_tick();

		    delay = delayRates[rate];
	    }
#endif
    }
}
