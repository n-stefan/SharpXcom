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
}
