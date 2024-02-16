/* ADLPLAYER
 *
 * player functions for midi-like adlib music
 *
 */

namespace SharpXcom.Engine.Adlib;

struct struc_adlib_channels
{
    internal byte cur_note;
    internal byte cur_instrument;
    internal byte cur_sample;
    internal byte cur_freq;
    internal byte hifreq;
    internal byte cur_volume;
    internal int duration;
    int pan;
}

struct struc_instruments
{
	internal byte sample_id;
	internal byte prev_cmd;
	internal byte volume;
	internal short cur_pitchbend;
	internal int cur_delay;
	internal Memory<byte> cur_address;
	internal Memory<byte> start_address;
	internal Memory<byte> return_address;
}

struct struc_sample
{
	internal byte reg20_op1;
	internal byte reg20_op2;
	internal byte reg40_op1;
	internal byte reg40_op2;
	internal byte reg60_op1;
	internal byte reg60_op2;
	internal byte reg80_op1;
	internal byte reg80_op2;
	internal byte regE0_op1;
	internal byte regE0_op2;
	internal byte regC0;
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

	static short[] adl_gv_freq_table = { // 9 * 12 -- notes frequency
		0x0B5,0x0C0,0x0CC,0x0D8,0x0E5,0x0F2,0x101,0x110,0x120,0x131,0x143,0x157,
		0x16B,0x181,0x198,0x1B0,0x1CA,0x1E5,0x202,0x220,0x241,0x263,0x287,0x2AE,
		0x16B,0x181,0x198,0x1B0,0x1CA,0x1E5,0x202,0x220,0x241,0x263,0x287,0x2AE,
		0x16B,0x181,0x198,0x1B0,0x1CA,0x1E5,0x202,0x220,0x241,0x263,0x287,0x2AE,
		0x16B,0x181,0x198,0x1B0,0x1CA,0x1E5,0x202,0x220,0x241,0x263,0x287,0x2AE,
		0x16B,0x181,0x198,0x1B0,0x1CA,0x1E5,0x202,0x220,0x241,0x263,0x287,0x2AE,
		0x16B,0x181,0x198,0x1B0,0x1CA,0x1E5,0x202,0x220,0x241,0x263,0x287,0x2AE,
		0x16B,0x181,0x198,0x1B0,0x1CA,0x1E5,0x202,0x220,0x241,0x263,0x287,0x2AE,
		0x16B,0x181,0x198,0x1B0,0x1CA,0x1E5,0x202,0x220,0x241,0x263,0x287,0x2AE};

	static sbyte[] adl_gv_octave_table = { // 9 * 12 -- octaves of notes
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
		7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7};

	static sbyte[] adl_gv_detune_table = { // 9 * 12 -- pitch bend scale values depending on note
		3, 3, 3, 3, 4, 4, 4, 4, 4, 5, 5, 5,
		3, 3, 3, 3, 4, 4, 4, 4, 4, 5, 5, 5,
		3, 3, 3, 3, 4, 4, 4, 4, 4, 5, 5, 5,
		3, 3, 3, 3, 4, 4, 4, 4, 4, 5, 5, 5,
		3, 3, 3, 3, 4, 4, 4, 4, 4, 5, 5, 5,
		3, 3, 3, 3, 4, 4, 4, 4, 4, 5, 5, 5,
		3, 3, 3, 3, 4, 4, 4, 4, 4, 5, 5, 5,
		3, 3, 3, 3, 4, 4, 4, 4, 4, 5, 5, 5,
		3, 3, 3, 3, 4, 4, 4, 4, 4, 5, 5, 5};

    internal static sbyte[] slot_array =
    {
         0, 2, 4, 1, 3, 5,-1,-1,
         6, 8,10, 7, 9,11,-1,-1,
        12,14,16,13,15,17,-1,-1,
        18,20,22,19,21,23,-1,-1
    };

    static sbyte[] adl_gv_instr_order = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 12, 13, 14, 15, 9 };
    static sbyte[] adl_gv_operators1 = { 0, 1, 2, 8, 9, 10, 16, 17, 18, 24, 25, 26 };

    static struc_adlib_channels[] adlib_channels = new struc_adlib_channels[12];
    static struc_instruments[] instruments = new struc_instruments[16];
    static int adl_gv_master_music_volume = 127;
    static int adl_gv_tmp_music_volume = 127;
    static bool adl_gv_want_fade = false;
    static bool adl_gv_music_playing = false;
    static int adl_gv_tempo = 120;
    static int adl_gv_tempo_run = 60;
    static int adl_gv_tempo_inc = 70;
    static Memory<byte> adl_gv_samples_addr = null;
    static Memory<byte>[] adl_gv_subtracks = new Memory<byte>[128];
    static uint adl_gv_instruments_count = 0;
    static uint adl_gv_subtracks_count = 0;
    static int adl_gv_polyphony_level = 0;
    static byte[] adl_gv_chorus_instruments = new byte[16];
    static int adl_gv_FORMAT = 0;//0 = without title, 1=with title

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

    //MAIN FUNCTION - instantly stops music
    internal static void func_mute()
	{
		adl_gv_polyphony_level = 0;
		adl_gv_music_playing = false;
		adlib_reset_channels();
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

    //MAIN FUNCTION - setup music for playing
    internal static void func_setup_music(Memory<byte> music_ptr, int length)
    {
	    adl_gv_music_playing = false;
	    func_mute();
	    adl_gv_polyphony_level = 0;
	    adl_gv_want_fade = false;
	    adl_gv_tmp_music_volume = adl_gv_master_music_volume;
	    init_music_data(music_ptr,length);
	    init_music();
	    adlib_init();
	    adlib_reset_channels();
	    adl_gv_tempo = (int)(adl_gv_tempo * 0.4);
	    adl_gv_tempo_run = adl_gv_tempo;
	    adl_gv_music_playing = true;
    }

    //MAIN FUNCTION - check if music finished
    internal static bool func_is_music_playing() =>
	    adl_gv_music_playing;

    //MAIN FUNCTION - runs 1 tick of music according tempo
    internal static void func_play_tick()
    {
	    bool another_loop;

	    if (!adl_gv_music_playing) return;
	    fade_volume_if_need();
	    adl_gv_tempo_run -= adl_gv_tempo;
	    if (adl_gv_tempo_run>0) return;
	    adl_gv_tempo_run += adl_gv_tempo_inc;

	    do {
		    another_loop = false;
		    for (int i=0; i<16; ++i)
		    {
			    int instr = adl_gv_instr_order[i];
    //			if (instr!=10) continue;
			    if (instruments[instr].cur_address.IsEmpty) continue;
			    if (instruments[instr].cur_delay == 0)
			    {
				    instruments[instr].cur_delay = decode_op(instr,ref another_loop);
				    if (!adl_gv_music_playing) break;
			    }
			    --instruments[instr].cur_delay;
		    }
		    if (!another_loop && adl_gv_music_playing) break;
		    init_music();
		    clear_channels();
	    } while (another_loop);
    }

	// main decode function - reads data from stream and decodes it accordingly
	static int decode_op(int instrument, ref bool another_loop)
	{
	//	const track=2;
		struc_instruments instr1 = instruments[instrument];
		struc_instruments instr2;
		Memory<byte> music_ptr = instr1.cur_address;
		byte opcode,arg1,arg2;
		int delay = 0;

		do {
			opcode = music_ptr.Span[0]; music_ptr = music_ptr.Slice(1);
			if (opcode == 0xfe) //call for subtrack
			{
				arg1 = music_ptr.Span[0]; music_ptr = music_ptr.Slice(1);
					//printf("Call for subtrack [%d] %d\n",instrument,arg1);
				instr1.return_address = music_ptr;
				music_ptr = adl_gv_subtracks[arg1];
			}
			else if (opcode == 0xfd) //return from subtrack
			{
				if (instr1.return_address.IsEmpty)
				{
					//printf("Return from subtrack [%d] -> DOUBLE RETURN - ERROR\n",instrument);
				}
				else
				{
					music_ptr = instr1.return_address;
						//printf("Return from subtrack [%d] -> %x\n",instrument,instr1->return_address);
					instr1.return_address = null;
				}
			}
			else if (opcode == 0xff) //finishing track
			{
					//printf("Track finish [%d]\n",instrument);
				adl_gv_music_playing = false;
				delay = 0;
				break;
			}
			else if (opcode >= 0x80) //opcode
			{
				instr1.prev_cmd = opcode;
					//printf("Opcode [%d]: %02Xh\n", instrument, opcode);
				opcode = music_ptr.Span[0]; music_ptr = music_ptr.Slice(1); //read "repeated" opcode which goes to arg
			}

			if (opcode<0x80) //can be not a opcode, just the repeated command
			{
				arg1 = opcode;
				opcode = instr1.prev_cmd;
				switch (opcode & 0xf0) {
				case 0x80: //note off
					arg2 = music_ptr.Span[0]; music_ptr = music_ptr.Slice(1);
						//printf("Opcode [%d] NOTE OFF: %d\n", instrument, arg1);
					adlib_play_note(arg1,0,instrument);
					--adl_gv_polyphony_level;
					if (adl_gv_chorus_instruments[instrument] != 0)
					{
						adlib_play_note(arg1,0,adl_gv_chorus_instruments[instrument]);
						--adl_gv_polyphony_level;
					}
					break;
				case 0x90: //note on
						arg2 = music_ptr.Span[0]; music_ptr = music_ptr.Slice(1);
					if (arg2 == 0) //volume==0 - note off
					{
							//printf("Opcode [%d] NOTE off: %d, volume=%d\n", instrument, arg1, arg2);
						adlib_play_note(arg1,0,instrument);
						--adl_gv_polyphony_level;
						if (adl_gv_chorus_instruments[instrument] != 0)
						{
							adlib_play_note(arg1,0,adl_gv_chorus_instruments[instrument]);
							--adl_gv_polyphony_level;
						}
					}
					else
					{
							//printf("Opcode [%d] NOTE ON: %d, volume=%d\n", instrument, arg1, arg2);
						int vol = (arg2*instr1.volume)>>7;
						if (adl_gv_chorus_instruments[instrument] != 0)
						{
							if (free_channel_available())
							{
								instr2 = instruments[adl_gv_chorus_instruments[instrument]];
								instr2.sample_id = instr1.sample_id;
								instr2.cur_pitchbend = (short)(instr1.cur_pitchbend-1);
								adlib_play_note(arg1,vol,adl_gv_chorus_instruments[instrument]);
							}
							++adl_gv_polyphony_level; //increase it nevertheless, because it SHOULD play
						}
						adlib_play_note(arg1,vol,instrument);
						++adl_gv_polyphony_level;
					}
					break;
				case 0xB0: //set controller
					arg2 = music_ptr.Span[0]; music_ptr = music_ptr.Slice(1);
						//printf("Opcode [%d] CONTROLLER: %02Xh, %d\n", instrument, arg1, arg2);
					if (arg1 == 0 && arg2 != 0) //tempo change
						adl_gv_tempo = (int)(arg2 * 0.8);
					else if (arg1 == 7) //channel volume change
					{
						instr1.volume = arg2;
	/*					for (i=0; i<9; ++i)
						{
							if (instrument == adlib_channels[i].cur_instrument)
							{
								adlib_channels[i].cur_volume = arg2;
								adlib_set_amplitude(i, (arg2*adl_gv_tmp_music_volume)>>7);
							}
						}
	*/				}
					else if (arg1 == 0x7e) //setting up chorus (slave) instrument
						adl_gv_chorus_instruments[instrument] = (byte)(arg2 - 1);
					else if (arg1 == 0x7f) //clearing chorus (slave) instrument
						adl_gv_chorus_instruments[instrument] = 0;

					break;
				case 0xC0: //set sample
					if (arg1 == 0x7E)
					{
						another_loop = true;
							//printf("Opcode [%d] LOOP\n",instrument);
					}
					else
					{
							//printf("Opcode [%d] SAMPLE: %d\n",instrument, arg1);
						instr1.sample_id = arg1;
					}
					break;
				case 0xE0: //pitch bend
						//printf("Opcode [%d] PITCH BEND: %d\n", instrument, arg1-16);
					instr1.cur_pitchbend = (short)(arg1 - 16); // no need???
					adlib_set_instrument_pitch(instrument, arg1-16);
					if (adl_gv_chorus_instruments[instrument] != 0)
						adlib_set_instrument_pitch(adl_gv_chorus_instruments[instrument], arg1-17);
					break;
				}
			}
			delay = get_numseq(music_ptr);
		} while (delay==0);
		instr1.cur_address = music_ptr;
		//if (delay!=0) printf("* Delay[%d] = %d\n", instrument, delay);
		return delay;
	}

    internal static void func_set_music_volume(int value)
    {
	    adl_gv_master_music_volume = value;
	    adl_gv_tmp_music_volume = adl_gv_master_music_volume;

	    for (int i=0; i<12; ++i)
	    {
		    adlib_set_amplitude(i, (adlib_channels[i].cur_volume*adl_gv_tmp_music_volume)>>7);
	    }
    }

    //decrease volume until 0 with each call, and then stops music
    static void fade_volume_if_need()
    {
	    if (!adl_gv_want_fade) return;
	    if (--adl_gv_tmp_music_volume == 0)
	    {
		    func_mute();
		    adl_gv_want_fade = false;
		    adl_gv_tmp_music_volume = adl_gv_master_music_volume;
		    return;
	    }
	    for (int i=0; i<12; ++i)
	    {
		    adlib_set_amplitude(i, (adlib_channels[i].cur_volume*adl_gv_tmp_music_volume)>>7);
	    }
    }

    //initialize music tracks, prepare for playing
    static void init_music()
    {
	    for (int i=0; i<16; ++i)
	    {
		    instruments[i].cur_pitchbend = 0;
		    adl_gv_chorus_instruments[i] = 0;
		    if (!instruments[i].start_address.IsEmpty)
		    {
			    instruments[i].cur_address = instruments[i].start_address;
			    instruments[i].cur_delay = get_numseq(instruments[i].cur_address);
		    }
		    else
		    {
			    instruments[i].cur_address = null;
			    instruments[i].cur_delay = 0;
		    }
	    }
    }

    // initialises adlib parameters
    static void adlib_init()
    {
	    for (int i=1; i<0xf5; ++i) adlib_reg(i,0);
	    adlib_reg(0x04, 0x60); //set both timers
	    adlib_reg(0x04, 0x80); //reset timers
	    adlib_reg(0x01, 0x20); //bit5 defines any wave form
	    adlib_reg(0xa8, 0x01); //last channel freq -??
	    adlib_reg(0x08, 0x40); //note mode
	    adlib_reg(0xbd, 0xC0); // vibrato/AM depth
    }

    // music initialization function, sets up subblock array,
    // tempo, instrument addresses and samples
    static void init_music_data(Memory<byte> music_ptr,int length)
    {
	    int i, to_add, j, start, offset = 0;
	    for (i=0; i<16; ++i)
	    {
		    instruments[i].start_address = null;
	    }
	    adl_gv_subtracks_count = 0;

	    i = music_ptr.Span[0];
	    if (i>56) adl_gv_FORMAT=0; //switch to old
	    else adl_gv_FORMAT=1;
	    if (adl_gv_FORMAT==1) { start = music_ptr.Span[0] + 1; music_ptr = music_ptr.Slice(start); offset += start; } //skip name
        adl_gv_tempo = music_ptr.Span[0]; music_ptr = music_ptr.Slice(1); offset += 1;
	    adl_gv_samples_addr = music_ptr.Slice(1); //samples
        start = (music_ptr.Span[0] * 24) + 1; music_ptr = music_ptr.Slice(start); offset += start; //moving to next section - subtracks
        adl_gv_subtracks_count = music_ptr.Span[0]; music_ptr = music_ptr.Slice(1); offset += 1;
	    for(i=0; i<adl_gv_subtracks_count; ++i)
	    {
		    to_add = peek_u16(music_ptr); //reading 16bit length
		    adl_gv_subtracks[i] = music_ptr.Slice(4); //store subtrack pointers
		    music_ptr = music_ptr.Slice(to_add); offset += to_add;
	    }
        adl_gv_instruments_count = music_ptr.Span[0]; music_ptr = music_ptr.Slice(1); offset += 1;
	    for (i=0; i<adl_gv_instruments_count; ++i)
	    {
		    to_add = peek_u16(music_ptr); //reading 16bit length
		    if (adl_gv_FORMAT==1)
		    {
			    j = music_ptr.Span[4];
			    if (j>15) j=15;
			    instruments[j].start_address = music_ptr.Slice(5);
		    }
		    else
		    if (adl_gv_FORMAT==0)
		    {
			    j = i;
			    instruments[j].start_address = music_ptr.Slice(4); //old format, without title
		    }
		    music_ptr = music_ptr.Slice(to_add); offset += to_add;
		    if (offset>=length)
		    {
			    //printf("DATA ERROR: track number %d has a length of %d, and gone to offset %d\n",i,to_add,music_ptr-start);
			    break;
		    }
	    }
    }

    /* Reading a 2-byte value from an unaligned address requires byte-copies on some
     * systems, which the system-memcpy takes care of for us */
    static ushort peek_u16(Memory<byte> ptr)
    {
	    ushort value;
        value = BitConverter.ToUInt16(ptr.Span);
	    return value;
    }

    //reads running-length value from data stream
    static int get_numseq(Memory<byte> mus_ptr)
    {
	    byte c;
	    int v=0;
	    do {
		    c = mus_ptr.Span[0];
		    v = (v<<7) + (c & 0x7f);
		    mus_ptr = mus_ptr.Slice(1);
	    } while ((c&0x80) != 0);
	    return v;
    }

	//starts playing note of particular instrument
	static void adlib_play_note(int note, int volume, int instrument)
	{
		struc_sample cur_sample;
		int sample_id = instruments[instrument].sample_id;
		int channel;
		int ampl;
		int op1;
		bool same_sample = false;
	/*	if (instrument == 9) //percussion instrument?
		{
			if (note>65 || note<35) return;// skip if out of range
			sample_id = note-35;
			if (percus_mapping[sample_id*3]==0xff) return;
			cur_sample = (struc_sample*)&( percus_samples[(percus_mapping[sample_id*3])*11] );
			note = percus_mapping[sample_id*3+1];
			sample_id += 128; //percussion sample is beyond ordinary (for compare purpose)
		}
		else //ordinary sample
	*/	{
			cur_sample = MemoryMarshal.Read<struc_sample>(adl_gv_samples_addr.Slice(sample_id*24).Span);
		}
		note--;
		if (volume == 0) //stop note
		{
			for(int i=0; i<12; ++i)
			{
				if (adlib_channels[i].cur_note == note &&
					adlib_channels[i].cur_instrument == instrument)
				{
					adlib_channels[i].cur_note = 0; //clear channel
					adlib_reg(0xB0+i, adlib_channels[i].hifreq); //mute note
				}
			}
			return;
		}
		if (volume>127) volume=127;
		channel = adlib_get_unused_channel(sample_id, ref same_sample);
		adlib_channels[channel].cur_volume = (byte)volume;
		adlib_channels[channel].cur_note = (byte)note;
		adlib_channels[channel].cur_instrument = (byte)instrument;
		op1 = adl_gv_operators1[channel];
		if (!same_sample)
		{
			adlib_reg(0x20+op1, cur_sample.reg20_op1); //amp mod / vibrato / eg type
			adlib_reg(0x23+op1, cur_sample.reg20_op2); //key scaling / multiple
			ampl = cur_sample.reg40_op1;
			adlib_reg(0x40+op1, ((~ampl) & 0x3f) | (ampl & 0xc0)); // amplitude op1
		}

		adlib_reg(0xB0+channel, adlib_channels[channel].hifreq);  // reinit note
		adlib_reg(0x43+op1, (~((adl_gv_tmp_music_volume*volume)>>8))&0x3f); //amplitude op2

		if (!same_sample)
		{
			adlib_reg(0x60+op1, cur_sample.reg60_op1); //attack rate/decay rate
			adlib_reg(0x63+op1, cur_sample.reg60_op2);
			adlib_reg(0x80+op1, cur_sample.reg80_op1); //sustain level / release rate
			adlib_reg(0x83+op1, cur_sample.reg80_op2);
			adlib_reg(0xE0+op1, cur_sample.regE0_op1); //wave select
			adlib_reg(0xE3+op1, cur_sample.regE0_op2);
			adlib_reg(0xC0+channel, cur_sample.regC0 ^ 0x01); //feedback strength / connection type
		}

		int freq = get_pitched_freq_instr(note, instrument);
		adlib_channels[channel].cur_freq = (byte)freq;
		adlib_reg(0xA0+channel, freq & 0xff);
		int hf=(freq>>8) | (adl_gv_octave_table[note]<<2);
		adlib_channels[channel].hifreq = (byte)hf;
		adlib_reg(0xB0+channel, hf | 0x20); //reinit note
	}

	// sets pitch for particular instrument
	// apply for adlib channels with same instrument
	// !!! probably should also apply pitch for CHORUS instrument !!!
	static void adlib_set_instrument_pitch(int instrument, int pitch)
	{
		instruments[instrument].cur_pitchbend = (short)pitch;
		for (int i=0; i<12; ++i) //search through active adlib channels
		{
			int note = adlib_channels[i].cur_note;
			if (note != 0 && adlib_channels[i].cur_instrument == instrument)
			{
				int freq = get_pitched_freq_instr(note, instrument);
				adlib_channels[i].cur_freq = (byte)freq;
				adlib_reg(0xA0+i, freq & 0xff);
				int hf=((freq>>8) & 0x03) | (adl_gv_octave_table[note]<<2);
				adlib_channels[i].hifreq = (byte)hf;
				adlib_reg(0xB0+i, hf | 0x20);
			}
		}
	}

	// returns note frequency with pitch wheel value applied
	static int get_pitched_freq_instr(int note, int instrument)
	{
		int pitch = instruments[instrument].cur_pitchbend;
		if (pitch==0)
			return adl_gv_freq_table[note];
		else if (pitch>0)
			return adl_gv_freq_table[note] + adl_gv_detune_table[note]*pitch;
		else
			return adl_gv_freq_table[note] + adl_gv_detune_table[note-1]*pitch;
	}

	// checks if there are unused adlib channels
	static bool free_channel_available()
	{
		for (int i=0; i<12; ++i)
		{
			if (adlib_channels[i].cur_note==0)
				return true;
		}
		return false;
	}

	// gets unused adlib channel and checks if old sample is the same
	static int adlib_get_unused_channel(int sample_id, ref bool same_sample)
	{
		int maxchan=0, maxdur=0, i;
		//bool empty=false;

		for (i=0; i<12; ++i)
			++adlib_channels[i].duration;

		for (i=0; i<12; ++i) //12/9
		{
			if (adlib_channels[i].duration > maxdur)
			{
				maxdur = adlib_channels[i].duration;
				maxchan = i;
			}
			if (adlib_channels[i].cur_note == 0) //empty channel
			{
				maxchan = i;
				//empty = true;
				break;
			}
		}

		//if (!empty) printf("   POLYPHONY - channel %d replaced\n", maxchan);
		if (adlib_channels[maxchan].cur_sample == sample_id)
			same_sample = true;
		else
			adlib_channels[maxchan].cur_sample = (byte)sample_id;
		adlib_channels[maxchan].duration = 0;
		return maxchan;
	}
}
