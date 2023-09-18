/* ADLPLAYER
 *
 * player functions for midi-like adlib music
 *
 */

namespace SharpXcom.Engine.Adlib;

struct struc_adlib_channels
{
    internal byte cur_note;
    byte cur_instrument;
    internal byte cur_sample;
    byte cur_freq;
    byte hifreq;
    byte cur_volume;
    int duration;
    int pan;
}

internal class adlplayer
{
    const int NEWBLOCK_LIMIT = 32;
    const double FREQ_OFFSET = 128.0; //128.0//96.0

    static byte[] iFMReg = new byte[256];
    static byte[] iTweakedFMReg = new byte[256];
    static byte[] iCurrentTweakedBlock = new byte[12];
    static byte[] iCurrentFNum = new byte[12];

    static FM_OPL[] opl = { default, default };

    internal static sbyte[] slot_array =
    {
	     0, 2, 4, 1, 3, 5,-1,-1,
	     6, 8,10, 7, 9,11,-1,-1,
	    12,14,16,13,15,17,-1,-1,
	    18,20,22,19,21,23,-1,-1
    };

    static sbyte[] adl_gv_operators1 = { 0, 1, 2, 8, 9, 10, 16, 17, 18, 24, 25, 26 };
    static struc_adlib_channels[] adlib_channels = new struc_adlib_channels[12];
    static int adl_gv_polyphony_level = 0;
    static bool adl_gv_music_playing = false;
    static int adl_gv_tmp_music_volume = 127;
    static bool adl_gv_want_fade = false;

    //MAIN FUNCTION - instantly stops music
    internal static void func_mute()
	{
		adl_gv_polyphony_level = 0;
		adl_gv_music_playing = false;
		adlib_reset_channels();
	}

    // resets (clears) adlib channels
    static void adlib_reset_channels()
    {
        clear_channels();
        for (int i = 0; i < 12; ++i)
        {
            adlib_reg(0xB0 + i, 0);
            adlib_set_amplitude(i, 0);
        }
    }

    // clear channel notes and samples
    static void clear_channels()
    {
        for (int i = 0; i < 12; ++i)
        {
            adlib_channels[i].cur_sample = 0xff;
            adlib_channels[i].cur_note = 0;
        }
    }

    // sets voice amplitude for particular adlib channel
    static void adlib_set_amplitude(int channel, int value) =>
        adlib_reg(0x43 + adl_gv_operators1[channel], ~(value >> 1) & 0x3f);

    static void adlib_reg(int i, int v)
    {
        if (opl[0] == default) return;
        int v2 = 0, i3 = -1, v3 = 0;
        Transpose(i, v, ref v2, ref i3, ref v3);

        OPLWrite(opl[0], 0, i);
        OPLWrite(opl[0], 1, v);
        OPLWrite(opl[1], 0, i);
        if (i >= 0x20 && i <= 0x3f) //no tremolo/vibrato
            v2 = (v2 & 0x3F);
        if (i >= 0xE0 && i <= 0xFC)
        {
            if ((slot_array[i & 0x1f] & 1) == 1) //wave form
                v2 = v2 & 0x02;
        }
        // if ((i >= 0x60 && i <= 0x7F) && ((slot_array[i & 0x1f] & 1) == 1)) //altered attack/decoy
        //	v2 = v2 ^ 0x20;
        OPLWrite(opl[1], 1, v2);
        if (i3 != -1)
        {
            OPLWrite(opl[1], 0, i3);
            OPLWrite(opl[1], 1, v3);
        }
    }

    static void Transpose(int reg, int val, ref int val2, ref int reg3, ref int val3)
    {
        // Transpose the other channel to produce the harmonic effect
        int iChannel = -1;
        int iRegister = reg; // temp
        int iValue = val; // temp
        if ((iRegister >> 4 == 0xA) || (iRegister >> 4 == 0xB)) iChannel = iRegister & 0x0F;

        // Remember the FM state, so that the harmonic effect can access
        // previously assigned register values.
        /*if (((iRegister >> 4 == 0xB) && (iValue & 0x20) && !(this->iFMReg[iRegister] & 0x20)) ||
            (iRegister == 0xBD) && (
                ((iValue & 0x01) && !(this->iFMReg[0xBD] & 0x01))
            )) {
            this->iFMReg[iRegister] = iValue;
        }*/
        iFMReg[iRegister] = (byte)iValue;

        if ((iChannel >= 0 && iChannel < 12))
        {// && (i == 1)) {
            byte iBlock = (byte)((iFMReg[0xB0 + iChannel] >> 2) & 0x07);
            ushort iFNum = (ushort)(((iFMReg[0xB0 + iChannel] & 0x03) << 8) | iFMReg[0xA0 + iChannel]);
            //double dbOriginalFreq = 50000.0 * (double)iFNum * pow(2, iBlock - 20);
            double dbOriginalFreq = 49716.0 * (double)iFNum / Math.Pow((double)2, (20 - iBlock));

            byte iNewBlock = iBlock;
            ushort iNewFNum;

            // Adjust the frequency and calculate the new FNum
            //double dbNewFNum = (dbOriginalFreq+(dbOriginalFreq/FREQ_OFFSET)) / (50000.0 * pow(2, iNewBlock - 20));
            double dbNewFNum = calcFNum(dbOriginalFreq, iNewBlock);

            // Make sure it's in range for the OPL chip
            if (dbNewFNum > 1023 - NEWBLOCK_LIMIT)
            {
                // It's too high, so move up one block (octave) and recalculate

                if (iNewBlock > 6)
                {
                    // Uh oh, we're already at the highest octave!
                    // The best we can do here is to just play the same note out of the second OPL, so at least it shouldn't
                    // sound *too* bad (hopefully it will just miss out on the nice harmonic.)
                    iNewBlock = iBlock;
                    iNewFNum = iFNum;
                }
                else
                {
                    iNewBlock++;
                    iNewFNum = (ushort)calcFNum(dbOriginalFreq, iNewBlock);
                }
            }
            else if (dbNewFNum < 0 + NEWBLOCK_LIMIT)
            {
                // It's too low, so move down one block (octave) and recalculate

                if (iNewBlock == 0)
                {
                    // Uh oh, we're already at the lowest octave!
                    // The best we can do here is to just play the same note out of the second OPL, so at least it shouldn't
                    // sound *too* bad (hopefully it will just miss out on the nice harmonic.)
                    iNewBlock = iBlock;
                    iNewFNum = iFNum;
                }
                else
                {
                    iNewBlock--;
                    iNewFNum = (ushort)calcFNum(dbOriginalFreq, iNewBlock);
                }
            }
            else
            {
                // Original calculation is within range, use that
                iNewFNum = (ushort)dbNewFNum;
            }

            // Sanity check
            if (iNewFNum > 1023)
            {
                // Uh oh, the new FNum is still out of range! (This shouldn't happen)
                // The best we can do here is to just play the same note out of the second OPL, so at least it shouldn't
                // sound *too* bad (hopefully it will just miss out on the nice harmonic.)
                iNewBlock = iBlock;
                iNewFNum = iFNum;
            }

            if ((iRegister >= 0xB0) && (iRegister <= 0xBC))
            {

                // Overwrite the supplied value with the new F-Number and Block.
                iValue = (iValue & ~0x1F) | (iNewBlock << 2) | ((iNewFNum >> 8) & 0x03);

                iCurrentTweakedBlock[iChannel] = iNewBlock; // save it so we don't have to update register 0xB0 later on
                iCurrentFNum[iChannel] = (byte)iNewFNum;

                if (iTweakedFMReg[0xA0 + iChannel] != (iNewFNum & 0xFF))
                {
                    // Need to write out low bits
                    byte iAdditionalReg = (byte)(0xA0 + iChannel);
                    byte iAdditionalValue = (byte)(iNewFNum & 0xFF);
                    reg3 = iAdditionalReg;
                    val3 = iAdditionalValue;
                    iTweakedFMReg[iAdditionalReg] = iAdditionalValue;
                }
            }
            else if ((iRegister >= 0xA0) && (iRegister <= 0xAC))
            {

                // Overwrite the supplied value with the new F-Number.
                iValue = iNewFNum & 0xFF;

                // See if we need to update the block number, which is stored in a different register
                byte iNewB0Value = (byte)((iFMReg[0xB0 + iChannel] & ~0x1F) | (iNewBlock << 2) | ((iNewFNum >> 8) & 0x03));
                if (
                    ((iNewB0Value & 0x20) != 0) && // but only update if there's a note currently playing (otherwise we can just wait
                    (iTweakedFMReg[0xB0 + iChannel] != iNewB0Value)   // until the next noteon and update it then)
                )
                {
                    // The note is already playing, so we need to adjust the upper bits too
                    byte iAdditionalReg = (byte)(0xB0 + iChannel);
                    reg3 = iAdditionalReg;
                    val3 = iNewB0Value;
                    iTweakedFMReg[iAdditionalReg] = iNewB0Value;
                } // else the note is not playing, the upper bits will be set when the note is next played

            } // if (register 0xB0 or 0xA0)

        } // if (a register we're interested in)

        // Now write to the original register with a possibly modified value
        val2 = iValue;
        iTweakedFMReg[iRegister] = (byte)iValue;

        //#define calcFNum() ((dbOriginalFreq+(dbOriginalFreq/FREQ_OFFSET)) / (50000.0 * pow(2, iNewBlock - 20)))
        static double calcFNum(double dbOriginalFreq, byte iNewBlock) =>
            ((dbOriginalFreq + (dbOriginalFreq / FREQ_OFFSET)) / (49716.0 / Math.Pow(2.0f, 20 - iNewBlock)));
    }

    //MAIN FUNCTION - initialize fade procedure
    internal static void func_fade()
    {
	    if (adl_gv_tmp_music_volume == 0)
	    {
		    func_mute();
	    }
	    else
	    {
		    adl_gv_want_fade = true;
	    }
    }
}
