/*
**
** File: fmopl.c -- software implementation of FM sound generator
**
** Copyright (C) 1999,2000 Tatsuyuki Satoh , MultiArcadeMachineEmurator development
**
** Version 0.37a
**
*/

/*
 * Modified version for X-COM (increased polyphony) by Volutar
*/

/* This version of fmopl.c is a fork of the MAME one, relicensed under the LGPL.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

/* --- select emulation chips --- */
#define BUILD_YM3812
//#define BUILD_YM3526 (HAS_YM3526)
//#define BUILD_Y8950  (HAS_Y8950)

#define DELTAT_MIXING_LEVEL // DELTA-T ADPCM MIXING LEVEL

namespace SharpXcom.Engine.Adlib;

delegate byte OPL_PORTHANDLER_R(int param);
delegate void OPL_PORTHANDLER_W(int param, byte data);
delegate void OPL_TIMERHANDLER(int channel, double interval_Sec);
delegate void OPL_IRQHANDLER(int param, int irq);
delegate void OPL_UPDATEHANDLER(int param, int min_interval_us);

/* Saving is necessary for member of the 'R' mark for suspend/resume */
/* ---------- OPL one of slot  ---------- */
struct OPL_SLOT
{
    internal int TL;       /* total level     :TL << 8            */
    internal int TLL;      /* adjusted now TL                     */
    internal byte KSR;      /* key scale rate  :(shift down bit)   */
    internal Memory<int> AR;      /* attack rate     :&AR_TABLE[AR<<2]   */
    internal Memory<int> DR;      /* decay rate      :&DR_TALBE[DR<<2]   */
    internal int SL;       /* sustin level    :SL_TALBE[SL]       */
    internal Memory<int> RR;      /* release rate    :&DR_TABLE[RR<<2]   */
    internal byte ksl;      /* keyscale level  :(shift down bits)  */
    internal byte ksr;      /* key scale rate  :kcode>>KSR         */
    internal uint mul;     /* multiple        :ML_TABLE[ML]       */
    internal uint Cnt;     /* frequency count :                   */
    internal uint Incr;    /* frequency step  :                   */
    /* envelope generator state */
    internal byte eg_typ;   /* envelope type flag                  */
    internal byte evm;      /* envelope phase                      */
    internal int evc;      /* envelope counter                    */
    internal int eve;      /* envelope counter end point          */
    internal int evs;      /* envelope counter step               */
    internal int evsa; /* envelope step for AR :AR[ksr]           */
    internal int evsd; /* envelope step for DR :DR[ksr]           */
    internal int evsr; /* envelope step for RR :RR[ksr]           */
    /* LFO */
    internal byte ams;      /* ams flag                            */
    internal byte vib;      /* vibrate flag                        */
    /* wave selector */
    internal Memory<int[]> wavetable;
}

/* ---------- OPL one of channel  ---------- */
struct OPL_CH
{
    internal OPL_SLOT[] SLOT = new OPL_SLOT[2];
    internal byte CON;          /* connection type                     */
    internal byte FB;           /* feed back       :(shift down bit)   */
    internal Memory<int> connect1;    /* slot1 output pointer                */
    internal Memory<int> connect2;    /* slot2 output pointer                */
    internal int[] op1_out = new int[2];   /* slot1 output for selfeedback        */
    /* phase generator state */
    internal uint block_fnum;  /* block+fnum      :                   */
    internal byte kcode;        /* key code        : KeyScaleCode      */
    internal uint fc;          /* Freq. Increment base                */
    internal uint ksl_base;    /* KeyScaleLevel Base step             */
    internal byte keyon;     /* key on/off flag                     */

    public OPL_CH() { }
}

/* OPL state */
record struct FM_OPL
{
    internal byte type;         /* chip type                         */
    internal int clock;          /* master clock  (Hz)                */
    internal int rate;           /* sampling rate (Hz)                */
    internal double freqbase;    /* frequency base                    */
    internal double TimerBase;   /* Timer base time (==sampling time) */
    internal byte address;      /* address register                  */
    internal byte status;       /* status flag                       */
    internal byte statusmask;   /* status mask                       */
    internal uint mode;        /* Reg.08 : CSM , notesel,etc.       */
    /* Timer */
    internal int[] T = new int[2];           /* timer counter                     */
    internal byte[] st = new byte[2];        /* timer enable                      */
    /* FM channel slots */
    internal Memory<OPL_CH> P_CH;       /* pointer of CH                     */
    internal int max_ch;         /* maximum channel                   */
    /* Rythm sention */
    internal byte rythm;        /* Rythm mode , key flag */
#if BUILD_Y8950
	/* Delta-T ADPCM unit (Y8950) */
	YM_DELTAT *deltat;			/* DELTA-T ADPCM       */
#endif
    /* Keyboard / I/O interface unit (Y8950) */
    byte portDirection;
    byte portLatch;
    OPL_PORTHANDLER_R porthandler_r;
    OPL_PORTHANDLER_W porthandler_w;
    int port_param;
    OPL_PORTHANDLER_R keyboardhandler_r;
    OPL_PORTHANDLER_W keyboardhandler_w;
    int keyboard_param;
    /* time tables */
    internal int[] AR_TABLE = new int[75]; /* attack rate tables */
    internal int[] DR_TABLE = new int[75]; /* decay rate tables   */
    internal uint[] FN_TABLE = new uint[1024];  /* fnumber -> increment counter */
    /* LFO */
    internal Memory<int> ams_table;
    internal Memory<int> vib_table;
    internal int amsCnt;
    internal int amsIncr;
    internal int vibCnt;
    internal int vibIncr;
    /* wave selector enable flag */
    internal byte wavesel;
    /* external event callback handler */
    internal OPL_TIMERHANDLER TimerHandler;      /* TIMER handler   */
    internal int TimerParam;                     /* TIMER parameter */
    internal OPL_IRQHANDLER IRQHandler;      /* IRQ handler    */
    internal int IRQParam;                       /* IRQ parameter  */
    internal OPL_UPDATEHANDLER UpdateHandler;    /* stream update handler   */
    internal int UpdateParam;                   /* stream update parameter */

    public FM_OPL() { }
}

internal class fmopl
{
    const double PI = 3.14159265358979323846;

    /* --- system optimize --- */
    /* select bit size of output : 8 or 16 */
    const int OPL_OUTPUT_BIT = 16;

    /* -------------------- preliminary define section --------------------- */
    /* attack/decay rate time rate */
    const int OPL_ARRATE = 141280;  /* RATE 4 =  2826.24ms @ 3.6MHz */
    const int OPL_DRRATE = 1956000;  /* RATE 4 = 39280.64ms @ 3.6MHz */

    const int FREQ_BITS = 24;			/* frequency turn          */

    /* counter bits = 20 , octerve 7 */
    const int FREQ_RATE = (1 << (FREQ_BITS - 20));
    const int TL_BITS = (FREQ_BITS + 2);

    /* final output shift , limit minimum and maximum */
    const int OPL_OUTSB = (TL_BITS + 1 - 16);		/* OPL output final shift 16bit */
    const int OPL_MAXOUT = (0x7fff << OPL_OUTSB);
    const int OPL_MINOUT = (-(0x8000 << OPL_OUTSB));

    /* -------------------- quality selection --------------------- */

    /* sinwave entries */
    /* used static memory = SIN_ENT * 4 (byte) */
    const int SIN_ENT = 2048;

    /* output level entries (envelope,sinwave) */
    /* envelope counter lower bits */
    const int ENV_BITS = 16;
    /* envelope output entries */
    const int EG_ENT = 4096;
    /* used dynamic memory = EG_ENT*4*4(byte)or EG_ENT*6*4(byte) */
    /* used static  memory = EG_ENT*4 (byte)                     */

    const int EG_OFF = ((2 * EG_ENT) << ENV_BITS);  /* OFF          */
    const int EG_DED = EG_OFF;
    const int EG_DST = (EG_ENT << ENV_BITS);      /* DECAY  START */
    const int EG_AED = EG_DST;
    const int EG_AST = 0;                       /* ATTACK START */

    const double EG_STEP = (96.0 / EG_ENT); /* OPL is 0.1875 dB step  */

    /* LFO table entries */
    const int VIB_ENT = 512;
    const int VIB_SHIFT = (32 - 9);
    const int AMS_ENT = 512;
    const int AMS_SHIFT = (32 - 9);

    const int VIB_RATE = 256;

    /* register number to channel number , slot offset */
    const int SLOT1 = 0;
    const int SLOT2 = 1;

    /* envelope phase */
    const int ENV_MOD_RR = 0x00;
    const int ENV_MOD_DR = 0x01;
    const int ENV_MOD_AR = 0x02;

    const int OPL_TYPE_WAVESEL = 0x01; /* waveform select    */
    /* ---------- Generic interface section ---------- */
    internal const int OPL_TYPE_YM3812 = OPL_TYPE_WAVESEL;

    const int ML = 2;

    const double DV = (EG_STEP / 2);

    const int TL_MAX = (EG_ENT * 2); /* limit(tl + ksr + envelope) + sinwave */

    /* -------------------- static state --------------------- */

    /* lock level of common table */
    static int num_lock = 0;

    /* work table */
    static byte[] cur_chip = null;	/* current chip point */
    /* current chip state */
    /* static OPLSAMPLE  *bufL,*bufR; */
    static Memory<OPL_CH> S_CH;
    static Memory<OPL_CH> E_CH;
    static OPL_SLOT SLOT7_1,SLOT7_2,SLOT8_1,SLOT8_2;

    static int ams;
    static int vib;
    static Memory<int> ams_table;
    static Memory<int> vib_table;
    static int amsIncr;
    static int vibIncr;

    /* sustain level table (3db per step) */
    /* 0 - 15: 0, 3, 6, 9,12,15,18,21,24,27,30,33,36,39,42,93 (dB)*/
    static int[] SL_TABLE = {
        SC( 0),SC( 1),SC( 2),SC(3 ),SC(4 ),SC(5 ),SC(6 ),SC( 7),
        SC( 8),SC( 9),SC(10),SC(11),SC(12),SC(13),SC(14),SC(31)
    };

    /* key scale level */
    /* table is 3dB/OCT , DV converts this in TL step at 6dB/OCT */
    static uint[] KSL_TABLE =
    {
	    /* OCT 0 */
	    U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),
        U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),
        U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),
        U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),
	    /* OCT 1 */
	    U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),
        U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),
        U( 0.000/DV),U( 0.750/DV),U( 1.125/DV),U( 1.500/DV),
        U( 1.875/DV),U( 2.250/DV),U( 2.625/DV),U( 3.000/DV),
	    /* OCT 2 */
	    U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),
        U( 0.000/DV),U( 1.125/DV),U( 1.875/DV),U( 2.625/DV),
        U( 3.000/DV),U( 3.750/DV),U( 4.125/DV),U( 4.500/DV),
        U( 4.875/DV),U( 5.250/DV),U( 5.625/DV),U( 6.000/DV),
	    /* OCT 3 */
	    U( 0.000/DV),U( 0.000/DV),U( 0.000/DV),U( 1.875/DV),
        U( 3.000/DV),U( 4.125/DV),U( 4.875/DV),U( 5.625/DV),
        U( 6.000/DV),U( 6.750/DV),U( 7.125/DV),U( 7.500/DV),
        U( 7.875/DV),U( 8.250/DV),U( 8.625/DV),U( 9.000/DV),
	    /* OCT 4 */
	    U( 0.000/DV),U( 0.000/DV),U( 3.000/DV),U( 4.875/DV),
        U( 6.000/DV),U( 7.125/DV),U( 7.875/DV),U( 8.625/DV),
        U( 9.000/DV),U( 9.750/DV),U(10.125/DV),U(10.500/DV),
        U(10.875/DV),U(11.250/DV),U(11.625/DV),U(12.000/DV),
	    /* OCT 5 */
	    U( 0.000/DV),U( 3.000/DV),U( 6.000/DV),U( 7.875/DV),
        U( 9.000/DV),U(10.125/DV),U(10.875/DV),U(11.625/DV),
        U(12.000/DV),U(12.750/DV),U(13.125/DV),U(13.500/DV),
        U(13.875/DV),U(14.250/DV),U(14.625/DV),U(15.000/DV),
	    /* OCT 6 */
	    U( 0.000/DV),U( 6.000/DV),U( 9.000/DV),U(10.875/DV),
        U(12.000/DV),U(13.125/DV),U(13.875/DV),U(14.625/DV),
        U(15.000/DV),U(15.750/DV),U(16.125/DV),U(16.500/DV),
        U(16.875/DV),U(17.250/DV),U(17.625/DV),U(18.000/DV),
	    /* OCT 7 */
	    U( 0.000/DV),U( 9.000/DV),U(12.000/DV),U(13.875/DV),
        U(15.000/DV),U(16.125/DV),U(16.875/DV),U(17.625/DV),
        U(18.000/DV),U(18.750/DV),U(19.125/DV),U(19.500/DV),
        U(19.875/DV),U(20.250/DV),U(20.625/DV),U(21.000/DV)
    };

    /* multiple table */
    static uint[] MUL_TABLE = {
    /* 1/2, 1, 2, 3, 4, 5, 6, 7, 8, 9,10,11,12,13,14,15 */
        ML/2, 1*ML, 2*ML, 3*ML, 4*ML, 5*ML, 6*ML, 7*ML,
        8*ML, 9*ML,10*ML,10*ML,12*ML,12*ML,15*ML,15*ML
    };

    /* TotalLevel : 48 24 12  6  3 1.5 0.75 (dB) */
    /* TL_TABLE[ 0      to TL_MAX          ] : plus  section */
    /* TL_TABLE[ TL_MAX to TL_MAX+TL_MAX-1 ] : minus section */
    static int[] TL_TABLE;

    /* pointers to TL_TABLE with sinwave output offset */
    static Memory<int[]> SIN_TABLE;

    /* LFO table */
    static int[] AMS_TABLE;
    static int[] VIB_TABLE;

    static int[] outd = new int[1];

    static int feedback2;       /* connect for SLOT 2 */

    /* envelope output curve table */
    /* attack + decay + OFF */
    static int[] ENV_CURVE = new int[2 * EG_ENT + 1];

    /* dummy attack / decay rate ( when rate == 0 ) */
    static int[] RATE_0 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    static int SC(int db) =>
        ((int)((db * ((3 / EG_STEP) * (1 << ENV_BITS))) + EG_DST));

    static uint U(double x) =>
        ((uint)x);

    /* --------------------- subroutines  --------------------- */

    static int Limit( int val, int max, int min )
    {
	    if ( val > max )
		    val = max;
	    else if ( val < min )
		    val = min;

	    return val;
    }

    /* ---------- YM3812 I/O interface ---------- */
    internal static int OPLWrite(FM_OPL OPL, int a, int v)
    {
        if (!((a & 1) != 0))
        {   /* address port */
            OPL.address = (byte)(v & 0xff);
        }
        else
        {   /* data port */
            if (OPL.UpdateHandler != null) OPL.UpdateHandler(OPL.UpdateParam, 0);
#if OPL_OUTPUT_LOG
            if (opl_dbg_fp)
            {
                for (opl_dbg_chip = 0; opl_dbg_chip < opl_dbg_maxchip; opl_dbg_chip++)
                    if (opl_dbg_opl[opl_dbg_chip] == OPL) break;
                fprintf(opl_dbg_fp, "%c%c%c", 0x10 + opl_dbg_chip, OPL.address, v);
            }
#endif
            OPLWriteReg(OPL, OPL.address, v);
        }
        return OPL.status >> 7;
    }

    /* ----------  Create one of virtual YM3812 ----------       */
    /* 'rate'  is sampling rate and 'bufsiz' is the size of the  */
    internal static FM_OPL OPLCreate(int type, int clock, int rate)
    {
        //char* ptr;
        FM_OPL OPL;
        //int state_size;
        int max_ch = 12; /* normally 9 channels */

        if (OPL_LockTable() == -1) return default;
        /* allocate OPL state space */
        //state_size = sizeof(FM_OPL);
        //state_size += sizeof(OPL_CH) * max_ch;
#if BUILD_Y8950
	if(type&OPL_TYPE_ADPCM) state_size+= sizeof(YM_DELTAT);
#endif
        /* allocate memory block */
        //ptr = (char*)malloc(state_size);
        //if (ptr == null) return default;
        /* clear */
        //memset(ptr, 0, state_size);
        OPL = new FM_OPL();
        //OPL = (FM_OPL*)ptr; ptr += sizeof(FM_OPL);
        //OPL.P_CH = (OPL_CH*)ptr; ptr += sizeof(OPL_CH) * max_ch;
#if BUILD_Y8950
	if(type&OPL_TYPE_ADPCM) OPL.deltat = (YM_DELTAT *)ptr; ptr+=sizeof(YM_DELTAT);
#endif
        /* set channel state pointer */
        OPL.type = (byte)type;
        OPL.clock = clock;
        OPL.rate = rate;
        OPL.max_ch = max_ch;
        /* init global tables */
        OPL_initalize(OPL);
        /* reset chip */
        OPLResetChip(OPL);
#if OPL_OUTPUT_LOG
        if (!opl_dbg_fp)
        {
            opl_dbg_fp = fopen("opllog.opl", "wb");
            opl_dbg_maxchip = 0;
        }
        if (opl_dbg_fp)
        {
            opl_dbg_opl[opl_dbg_maxchip] = OPL;
            fprintf(opl_dbg_fp, "%c%c%c%c%c%c", 0x00 + opl_dbg_maxchip,
                type,
                clock & 0xff,
                (clock / 0x100) & 0xff,
                (clock / 0x10000) & 0xff,
                (clock / 0x1000000) & 0xff);
            opl_dbg_maxchip++;
        }
#endif
        return OPL;
    }

    /* ----------  Destroy one of virtual YM3812 ----------       */
    internal static void OPLDestroy(FM_OPL OPL)
    {
        if (OPL == default)
        {
            return;
        }
#if OPL_OUTPUT_LOG
        if (opl_dbg_fp)
        {
            fclose(opl_dbg_fp);
            opl_dbg_fp = NULL;
        }
#endif
        OPL_UnLockTable();
        OPL = default;
    }

    /* lock/unlock for common table */
    static int OPL_LockTable()
    {
        num_lock++;
        if (num_lock > 1) return 0;
        /* first time */
        cur_chip = null;
        /* allocate total level table (128kb space) */
        if (OPLOpenTable() == 0)
        {
            num_lock--;
            return -1;
        }
        return 0;
    }

    static void OPL_UnLockTable()
    {
        if (num_lock != 0) num_lock--;
        if (num_lock != 0) return;
        /* last time */
        cur_chip = null;
        OPLCloseTable();
    }

    /* ---------- reset one of chip ---------- */
    static void OPLResetChip(FM_OPL OPL)
    {
        int c, s;
        int i;

        /* reset chip */
        OPL.mode = 0;  /* normal mode */
        OPL_STATUS_RESET(OPL, 0x7f);
        /* reset with register write */
        OPLWriteReg(OPL, 0x01, 0); /* wabesel disable */
        OPLWriteReg(OPL, 0x02, 0); /* Timer1 */
        OPLWriteReg(OPL, 0x03, 0); /* Timer2 */
        OPLWriteReg(OPL, 0x04, 0); /* IRQ mask clear */
        for (i = 0xff; i >= 0x20; i--) OPLWriteReg(OPL, i, 0);
        /* reset OPerator parameter */
        for (c = 0; c < OPL.max_ch; c++)
        {
            OPL_CH CH = OPL.P_CH.Span[c];
            /* OPL.P_CH[c].PAN = OPN_CENTER; */
            for (s = 0; s < 2; s++)
            {
                /* wave table */
                CH.SLOT[s].wavetable = SIN_TABLE;
                /* CH.SLOT[s].evm = ENV_MOD_RR; */
                CH.SLOT[s].evc = EG_OFF;
                CH.SLOT[s].eve = EG_OFF + 1;
                CH.SLOT[s].evs = 0;
            }
        }
#if BUILD_Y8950
	if(OPL.type&OPL_TYPE_ADPCM)
	{
		YM_DELTAT *DELTAT = OPL.deltat;

		DELTAT.freqbase = OPL.freqbase;
		DELTAT.output_pointer = outd;
		DELTAT.portshift = 5;
		DELTAT.output_range = DELTAT_MIXING_LEVEL<<TL_BITS;
		YM_DELTAT_ADPCM_Reset(DELTAT,0);
	}
#endif
    }

    /* ---------- opl initialize ---------- */
    static void OPL_initalize(FM_OPL OPL)
    {
        int fn;

        /* frequency base */
        OPL.freqbase = (OPL.rate != 0) ? ((double)OPL.clock / OPL.rate) / 72 : 0;
        /* Timer base time */
        OPL.TimerBase = 1.0 / ((double)OPL.clock / 72.0);
        /* make time tables */
        init_timetables(OPL, OPL_ARRATE, OPL_DRRATE);
        /* make fnumber -> increment counter table */
        for (fn = 0; fn < 1024; fn++)
        {
            OPL.FN_TABLE[fn] = (uint)(OPL.freqbase * fn * FREQ_RATE * (1 << 7) / 2);
        }
        /* LFO freq.table */
        OPL.amsIncr = (int)(OPL.rate != 0 ? (double)AMS_ENT * (1 << AMS_SHIFT) / OPL.rate * 3.7 * ((double)OPL.clock / 3600000) : 0);
        OPL.vibIncr = (int)(OPL.rate != 0 ? (double)VIB_ENT * (1 << VIB_SHIFT) / OPL.rate * 6.4 * ((double)OPL.clock / 3600000) : 0);
    }

    /* status reset and IRQ handling */
    static void OPL_STATUS_RESET(FM_OPL OPL, int flag)
    {
        /* reset status flag */
        OPL.status = (byte)(OPL.status & ~flag);
        if ((OPL.status & 0x80) != 0)
        {
            if (!((OPL.status & OPL.statusmask) != 0))
            {
                OPL.status &= 0x7f;
                /* callback user interrupt handler (IRQ is ON to OFF) */
                if (OPL.IRQHandler != null) OPL.IRQHandler(OPL.IRQParam, 0);
            }
        }
    }

    /* ----------- initialize time tables ----------- */
    static void init_timetables(FM_OPL OPL, int ARRATE, int DRRATE)
    {
        int i;
        double rate;

        /* make attack rate & decay rate tables */
        for (i = 0; i < 4; i++) OPL.AR_TABLE[i] = OPL.DR_TABLE[i] = 0;
        for (i = 4; i <= 60; i++)
        {
            rate = OPL.freqbase;                       /* frequency rate */
            if (i < 60) rate *= 1.0 + (i & 3) * 0.25;       /* b0-1 : x1 , x1.25 , x1.5 , x1.75 */
            rate *= 1 << ((i >> 2) - 1);                        /* b2-5 : shift bit */
            rate = (double)(rate * (EG_ENT << ENV_BITS));
            OPL.AR_TABLE[i] = (int)(rate / ARRATE);
            OPL.DR_TABLE[i] = (int)(rate / DRRATE);
        }
        for (i = 60; i < 75; i++)
        {
            OPL.AR_TABLE[i] = EG_AED - 1;
            OPL.DR_TABLE[i] = OPL.DR_TABLE[60];
        }
//TODO
//#if 0
//	for (i = 0;i < 64 ;i++){	/* make for overflow area */
//		LOG(LOG_WAR,("rate %2d , ar %f ms , dr %f ms \n",i,
//			((double)(EG_ENT<<ENV_BITS) / OPL.AR_TABLE[i]) * (1000.0 / OPL.rate),
//			((double)(EG_ENT<<ENV_BITS) / OPL.DR_TABLE[i]) * (1000.0 / OPL.rate) ));
//	}
//#endif
    }

    /* ---------- generic table initialize ---------- */
    static int OPLOpenTable()
    {
        int s, t;
        double rate;
        int i, j;
        double pom;

        /* allocate dynamic tables */
        TL_TABLE = new int[TL_MAX * 2];
        SIN_TABLE = new int[SIN_ENT * 4][];
        AMS_TABLE = new int[AMS_ENT * 2];
        VIB_TABLE = new int[VIB_ENT * 2];
        /* make total level table */
        for (t = 0; t < EG_ENT - 1; t++)
        {
            rate = ((1 << TL_BITS) - 1) / Math.Pow(10, EG_STEP * t / 20);    /* dB -> voltage */
            TL_TABLE[t] = (int)rate;
            TL_TABLE[TL_MAX + t] = -TL_TABLE[t];
            /*		LOG(LOG_INF,("TotalLevel(%3d) = %x\n",t,TL_TABLE[t]));*/
        }
        /* fill volume off area */
        for (t = EG_ENT - 1; t < TL_MAX; t++)
        {
            TL_TABLE[t] = TL_TABLE[TL_MAX + t] = 0;
        }

        /* make sinwave table (total level offset) */
        /* degree 0 = degree 180                   = off */
        SIN_TABLE.Span[0] = SIN_TABLE.Span[SIN_ENT / 2] = TL_TABLE[(EG_ENT - 1)..];
        for (s = 1; s <= SIN_ENT / 4; s++)
        {
            pom = Math.Sin(2 * PI * s / SIN_ENT); /* sin     */
            pom = 20 * Math.Log10(1 / pom);     /* decibel */
            j = (int)(pom / EG_STEP);         /* TL_TABLE steps */

            /* degree 0   -  90    , degree 180 -  90 : plus section */
            SIN_TABLE.Span[s] = SIN_TABLE.Span[SIN_ENT / 2 - s] = TL_TABLE[j..];
            /* degree 180 - 270    , degree 360 - 270 : minus section */
            SIN_TABLE.Span[SIN_ENT / 2 + s] = SIN_TABLE.Span[SIN_ENT - s] = TL_TABLE[(TL_MAX + j)..];
            /*		LOG(LOG_INF,("sin(%3d) = %f:%f db\n",s,pom,(double)j * EG_STEP));*/
        }
        for (s = 0; s < SIN_ENT; s++)
        {
            SIN_TABLE.Span[SIN_ENT * 1 + s] = s < (SIN_ENT / 2) ? SIN_TABLE.Span[s] : TL_TABLE[EG_ENT..];
            SIN_TABLE.Span[SIN_ENT * 2 + s] = SIN_TABLE.Span[s % (SIN_ENT / 2)];
            SIN_TABLE.Span[SIN_ENT * 3 + s] = ((s / (SIN_ENT / 4)) & 1) != 0 ? TL_TABLE[EG_ENT..] : SIN_TABLE.Span[SIN_ENT * 2 + s];
        }

        /* envelope counter -> envelope output table */
        for (i = 0; i < EG_ENT; i++)
        {
            /* ATTACK curve */
            pom = Math.Pow(((double)(EG_ENT - 1 - i) / EG_ENT), 8) * EG_ENT;
            /* if( pom >= EG_ENT ) pom = EG_ENT-1; */
            ENV_CURVE[i] = (int)pom;
            /* DECAY ,RELEASE curve */
            ENV_CURVE[(EG_DST >> ENV_BITS) + i] = i;
        }
        /* off */
        ENV_CURVE[EG_OFF >> ENV_BITS] = EG_ENT - 1;
        /* make LFO ams table */
        for (i = 0; i < AMS_ENT; i++)
        {
            pom = (1.0 + Math.Sin(2 * PI * i / AMS_ENT)) / 2; /* sin */
            AMS_TABLE[i] = (int)((1.0 / EG_STEP) * pom); /* 1dB   */
            AMS_TABLE[AMS_ENT + i] = (int)((4.8 / EG_STEP) * pom); /* 4.8dB */
        }
        /* make LFO vibrate table */
        for (i = 0; i < VIB_ENT; i++)
        {
            /* 100cent = 1seminote = 6% ?? */
            pom = (double)VIB_RATE * 0.06 * Math.Sin(2 * PI * i / VIB_ENT); /* +-100sect step */
            VIB_TABLE[i] = (int)(VIB_RATE + (pom * 0.07)); /* +- 7cent */
            VIB_TABLE[VIB_ENT + i] = (int)(VIB_RATE + (pom * 0.14)); /* +-14cent */
            /* LOG(LOG_INF,("vib %d=%d\n",i,VIB_TABLE[VIB_ENT+i])); */
        }
        return 1;
    }

    static void OPLCloseTable()
    {
        TL_TABLE = null;
        SIN_TABLE = null;
        AMS_TABLE = null;
        VIB_TABLE = null;
    }

    /* ---------- write a OPL registers ---------- */
    static void OPLWriteReg(FM_OPL OPL, int r, int v)
    {
        OPL_CH CH;
        int slot;
        int block_fnum;

        switch (r & 0xe0)
        {
            case 0x00: /* 00-1f:control */
                switch (r & 0x1f)
                {
                    case 0x01:
                        /* wave selector enable */
                        if ((OPL.type & OPL_TYPE_WAVESEL) != 0)
                        {
                            OPL.wavesel = (byte)(v & 0x20);
                            if (OPL.wavesel == 0)
                            {
                                /* preset compatible mode */
                                int c;
                                for (c = 0; c < OPL.max_ch; c++)
                                {
                                    OPL.P_CH.Span[c].SLOT[SLOT1].wavetable = SIN_TABLE;
                                    OPL.P_CH.Span[c].SLOT[SLOT2].wavetable = SIN_TABLE;
                                }
                            }
                        }
                        return;
                    case 0x02:  /* Timer 1 */
                        OPL.T[0] = (256 - v) * 4;
                        break;
                    case 0x03:  /* Timer 2 */
                        OPL.T[1] = (256 - v) * 16;
                        return;
                    case 0x04:  /* IRQ clear / mask and Timer enable */
                        if ((v & 0x80) != 0)
                        {   /* IRQ flag clear */
                            OPL_STATUS_RESET(OPL, 0x7f);
                        }
                        else
                        {   /* set IRQ mask ,timer enable*/
                            byte st1 = (byte)(v & 1);
                            byte st2 = (byte)((v >> 1) & 1);
                            /* IRQRST,T1MSK,t2MSK,EOSMSK,BRMSK,x,ST2,ST1 */
                            OPL_STATUS_RESET(OPL, v & 0x78);
                            OPL_STATUSMASK_SET(OPL, ((~v) & 0x78) | 0x01);
                            /* timer 2 */
                            if (OPL.st[1] != st2)
                            {
                                double interval = st2 != 0 ? (double)OPL.T[1] * OPL.TimerBase : 0.0;
                                OPL.st[1] = st2;
                                if (OPL.TimerHandler != null) OPL.TimerHandler(OPL.TimerParam + 1, interval);
                            }
                            /* timer 1 */
                            if (OPL.st[0] != st1)
                            {
                                double interval = st1 != 0 ? (double)OPL.T[0] * OPL.TimerBase : 0.0;
                                OPL.st[0] = st1;
                                if (OPL.TimerHandler != null) OPL.TimerHandler(OPL.TimerParam + 0, interval);
                            }
                        }
                        return;
#if BUILD_Y8950
		case 0x06:		/* Key Board OUT */
			if(OPL.type&OPL_TYPE_KEYBOARD)
			{
				if(OPL.keyboardhandler_w)
					OPL.keyboardhandler_w(OPL.keyboard_param,v);
				else
					LOG(LOG_WAR,("OPL:write unmapped KEYBOARD port\n"));
			}
			return;
		case 0x07:	/* DELTA-T control : START,REC,MEMDATA,REPT,SPOFF,x,x,RST */
			if(OPL.type&OPL_TYPE_ADPCM)
				YM_DELTAT_ADPCM_Write(OPL.deltat,r-0x07,v);
			return;
		case 0x08:	/* MODE,DELTA-T : CSM,NOTESEL,x,x,smpl,da/ad,64k,rom */
			OPL.mode = v;
			v&=0x1f;	/* for DELTA-T unit */
		case 0x09:		/* START ADD */
		case 0x0a:
		case 0x0b:		/* STOP ADD  */
		case 0x0c:
		case 0x0d:		/* PRESCALE   */
		case 0x0e:
		case 0x0f:		/* ADPCM data */
		case 0x10: 		/* DELTA-N    */
		case 0x11: 		/* DELTA-N    */
		case 0x12: 		/* EG-CTRL    */
			if(OPL.type&OPL_TYPE_ADPCM)
				YM_DELTAT_ADPCM_Write(OPL.deltat,r-0x07,v);
			return;
//TODO
//#if 0
//		case 0x15:		/* DAC data    */
//		case 0x16:
//		case 0x17:		/* SHIFT    */
//			return;
//		case 0x18:		/* I/O CTRL (Direction) */
//			if(OPL.type&OPL_TYPE_IO)
//				OPL.portDirection = v&0x0f;
//			return;
//		case 0x19:		/* I/O DATA */
//			if(OPL.type&OPL_TYPE_IO)
//			{
//				OPL.portLatch = v;
//				if(OPL.porthandler_w)
//					OPL.porthandler_w(OPL.port_param,v&OPL.portDirection);
//			}
//			return;
//		case 0x1a:		/* PCM data */
//			return;
//#endif
#endif
                }
                break;
            case 0x20:  /* am,vib,ksr,eg type,mul */
                slot = slot_array[r & 0x1f];
                if (slot == -1) return;
                set_mul(OPL, slot, v);
                return;
            case 0x40:
                slot = slot_array[r & 0x1f];
                if (slot == -1) return;
                set_ksl_tl(OPL, slot, v);
                return;
            case 0x60:
                slot = slot_array[r & 0x1f];
                if (slot == -1) return;
                set_ar_dr(OPL, slot, v);
                return;
            case 0x80:
                slot = slot_array[r & 0x1f];
                if (slot == -1) return;
                set_sl_rr(OPL, slot, v);
                return;
            case 0xa0:
                switch (r)
                {
                    case 0xbd:
                        /* amsep,vibdep,r,bd,sd,tom,tc,hh */
                        {
                            byte rkey = (byte)(OPL.rythm ^ v);
                            OPL.ams_table = AMS_TABLE.AsMemory((v & 0x80) != 0 ? AMS_ENT : 0);
                            OPL.vib_table = VIB_TABLE.AsMemory((v & 0x40) != 0 ? VIB_ENT : 0);
                            OPL.rythm = (byte)(v & 0x3f);
                            if ((OPL.rythm & 0x20) != 0)
                            {
//TODO
//#if 0
//				usrintf_showmessage("OPL Rythm mode select");
//#endif
                                /* BD key on/off */
                                if ((rkey & 0x10) != 0)
                                {
                                    if ((v & 0x10) != 0)
                                    {
                                        OPL.P_CH.Span[6].op1_out[0] = OPL.P_CH.Span[6].op1_out[1] = 0;
                                        OPL_KEYON(OPL.P_CH.Span[6].SLOT[SLOT1]);
                                        OPL_KEYON(OPL.P_CH.Span[6].SLOT[SLOT2]);
                                    }
                                    else
                                    {
                                        OPL_KEYOFF(OPL.P_CH.Span[6].SLOT[SLOT1]);
                                        OPL_KEYOFF(OPL.P_CH.Span[6].SLOT[SLOT2]);
                                    }
                                }
                                /* SD key on/off */
                                if ((rkey & 0x08) != 0)
                                {
                                    if ((v & 0x08) != 0) OPL_KEYON(OPL.P_CH.Span[7].SLOT[SLOT2]);
                                    else OPL_KEYOFF(OPL.P_CH.Span[7].SLOT[SLOT2]);
                                }/* TAM key on/off */
                                if ((rkey & 0x04) != 0)
                                {
                                    if ((v & 0x04) != 0) OPL_KEYON(OPL.P_CH.Span[8].SLOT[SLOT1]);
                                    else OPL_KEYOFF(OPL.P_CH.Span[8].SLOT[SLOT1]);
                                }
                                /* TOP-CY key on/off */
                                if ((rkey & 0x02) != 0)
                                {
                                    if ((v & 0x02) != 0) OPL_KEYON(OPL.P_CH.Span[8].SLOT[SLOT2]);
                                    else OPL_KEYOFF(OPL.P_CH.Span[8].SLOT[SLOT2]);
                                }
                                /* HH key on/off */
                                if ((rkey & 0x01) != 0)
                                {
                                    if ((v & 0x01) != 0) OPL_KEYON(OPL.P_CH.Span[7].SLOT[SLOT1]);
                                    else OPL_KEYOFF(OPL.P_CH.Span[7].SLOT[SLOT1]);
                                }
                            }
                        }
                        return;
                }
                /* keyon,block,fnum */
                if ((r & 0x0f) > 11) return; //8
                CH = OPL.P_CH.Span[r & 0x0f];
                if (!((r & 0x10) != 0))
                {   /* a0-a8 */
                    block_fnum = (int)((CH.block_fnum & 0x1f00) | v);
                }
                else
                {   /* b0-b8 */
                    int keyon = (v >> 5) & 1;
                    block_fnum = (int)(((v & 0x1f) << 8) | (CH.block_fnum & 0xff));
                    if (CH.keyon != keyon)
                    {
                        if ((CH.keyon = (byte)keyon) != 0)
                        {
                            CH.op1_out[0] = CH.op1_out[1] = 0;
                            OPL_KEYON(CH.SLOT[SLOT1]);
                            OPL_KEYON(CH.SLOT[SLOT2]);
                        }
                        else
                        {
                            OPL_KEYOFF(CH.SLOT[SLOT1]);
                            OPL_KEYOFF(CH.SLOT[SLOT2]);
                        }
                    }
                }
                /* update */
                if ((int)CH.block_fnum != block_fnum)
                {
                    int blockRv = 7 - (block_fnum >> 10);
                    int fnum = block_fnum & 0x3ff;
                    CH.block_fnum = (uint)block_fnum;

                    CH.ksl_base = KSL_TABLE[block_fnum >> 6];
                    CH.fc = OPL.FN_TABLE[fnum] >> blockRv;
                    CH.kcode = (byte)(CH.block_fnum >> 9);
                    if (((OPL.mode & 0x40) != 0) && (CH.block_fnum & 0x100) != 0) CH.kcode |= 1;
                    CALC_FCSLOT(CH, CH.SLOT[SLOT1]);
                    CALC_FCSLOT(CH, CH.SLOT[SLOT2]);
                }
                return;
            case 0xc0:
                /* FB,C */
                if ((r & 0x0f) > 11) return;//8
                CH = OPL.P_CH.Span[r & 0x0f];
                {
                    int feedback = (v >> 1) & 7;
                    CH.FB = (byte)(feedback != 0 ? (8 + 1) - feedback : 0);
                    CH.CON = (byte)(v & 1);
                    set_algorythm(CH);
                }
                return;
            case 0xe0: /* wave type */
                slot = slot_array[r & 0x1f];
                if (slot == -1) return;
                CH = OPL.P_CH.Span[slot / 2];
                if (OPL.wavesel != 0)
                {
                    /* LOG(LOG_INF,("OPL SLOT %d wave select %d\n",slot,v&3)); */
                    CH.SLOT[slot & 1].wavetable = SIN_TABLE.Slice((v & 0x03) * SIN_ENT);
                }
                return;
        }
    }

    /* ----- key on  ----- */
    static void OPL_KEYON(OPL_SLOT SLOT)
    {
        /* sin wave restart */
        SLOT.Cnt = 0;
        /* set attack */
        SLOT.evm = ENV_MOD_AR;
        SLOT.evs = SLOT.evsa;
        SLOT.evc = EG_AST;
        SLOT.eve = EG_AED;
    }

    /* ----- key off ----- */
    static void OPL_KEYOFF(OPL_SLOT SLOT)
    {
        if (SLOT.evm > ENV_MOD_RR)
        {
            /* set envelope counter from envelope output */
            SLOT.evm = ENV_MOD_RR;
            if (!((SLOT.evc & EG_DST) != 0))
                SLOT.evc = (ENV_CURVE[SLOT.evc >> ENV_BITS] << ENV_BITS) + EG_DST;
            //SLOT.evc = EG_DST;
            SLOT.eve = EG_DED;
            SLOT.evs = SLOT.evsr;
        }
    }

    /* ---------- frequency counter for operator update ---------- */
    static void CALC_FCSLOT(OPL_CH CH, OPL_SLOT SLOT)
    {
        int ksr;

        /* frequency step counter */
        SLOT.Incr = CH.fc * SLOT.mul;
        ksr = CH.kcode >> SLOT.KSR;

        if (SLOT.ksr != ksr)
        {
            SLOT.ksr = (byte)ksr;
            /* attack , decay rate recalcration */
            SLOT.evsa = SLOT.AR.Span[ksr];
            SLOT.evsd = SLOT.DR.Span[ksr];
            SLOT.evsr = SLOT.RR.Span[ksr];
        }
        SLOT.TLL = (int)(SLOT.TL + (CH.ksl_base >> SLOT.ksl));
    }

    /* set algorithm connection */
    static void set_algorythm(OPL_CH CH)
    {
        var carrier = outd.AsMemory();
        CH.connect1 = CH.CON != 0 ? carrier : new[] { feedback2 };
        CH.connect2 = carrier;
    }

    /* set sustain level & release rate */
    static void set_sl_rr(FM_OPL OPL, int slot, int v)
    {
        OPL_CH CH = OPL.P_CH.Span[slot / 2];
        OPL_SLOT SLOT = CH.SLOT[slot & 1];
        int sl = v >> 4;
        int rr = v & 0x0f;

        SLOT.SL = SL_TABLE[sl];
        if (SLOT.evm == ENV_MOD_DR) SLOT.eve = SLOT.SL;
        SLOT.RR = OPL.DR_TABLE.AsMemory(rr << 2);
        SLOT.evsr = SLOT.RR.Span[SLOT.ksr];
        if (SLOT.evm == ENV_MOD_RR) SLOT.evs = SLOT.evsr;
    }

    /* set attack rate & decay rate  */
    static void set_ar_dr(FM_OPL OPL, int slot, int v)
    {
        OPL_CH CH = OPL.P_CH.Span[slot / 2];
        OPL_SLOT SLOT = CH.SLOT[slot & 1];
        int ar = v >> 4;
        int dr = v & 0x0f;

        SLOT.AR = ar != 0 ? OPL.AR_TABLE.AsMemory(ar << 2) : RATE_0;
        SLOT.evsa = SLOT.AR.Span[SLOT.ksr];
        if (SLOT.evm == ENV_MOD_AR) SLOT.evs = SLOT.evsa;

        SLOT.DR = dr != 0 ? OPL.DR_TABLE.AsMemory(dr << 2) : RATE_0;
        SLOT.evsd = SLOT.DR.Span[SLOT.ksr];
        if (SLOT.evm == ENV_MOD_DR) SLOT.evs = SLOT.evsd;
    }

    /* set ksl & tl */
    static void set_ksl_tl(FM_OPL OPL, int slot, int v)
    {
        OPL_CH CH = OPL.P_CH.Span[slot / 2];
        OPL_SLOT SLOT = CH.SLOT[slot & 1];
        int ksl = v >> 6; /* 0 / 1.5 / 3 / 6 db/OCT */

        SLOT.ksl = (byte)(ksl != 0 ? 3 - ksl : 31);
        SLOT.TL = (int)((v & 0x3f) * (0.75 / EG_STEP)); /* 0.75db step */

        if (!((OPL.mode & 0x80) != 0))
        {   /* not CSM latch total level */
            SLOT.TLL = (int)(SLOT.TL + (CH.ksl_base >> SLOT.ksl));
        }
    }

    /* set multi,am,vib,EG-TYP,KSR,mul */
    static void set_mul(FM_OPL OPL, int slot, int v)
    {
        OPL_CH CH = OPL.P_CH.Span[slot / 2];
        OPL_SLOT SLOT = CH.SLOT[slot & 1];

        SLOT.mul = MUL_TABLE[v & 0x0f];
        SLOT.KSR = (byte)(((v & 0x10) != 0) ? 0 : 2);
        SLOT.eg_typ = (byte)((v & 0x20) >> 5);
        SLOT.vib = ((byte)(v & 0x40));
        SLOT.ams = ((byte)(v & 0x80));
        CALC_FCSLOT(CH, SLOT);
    }

    /* IRQ mask set */
    static void OPL_STATUSMASK_SET(FM_OPL OPL, int flag)
    {
        OPL.statusmask = (byte)flag;
        /* IRQ handling check */
        OPL_STATUS_SET(OPL, 0);
        OPL_STATUS_RESET(OPL, 0);
    }

    /* status set and IRQ handling */
    static void OPL_STATUS_SET(FM_OPL OPL, int flag)
    {
        /* set status flag */
        OPL.status = (byte)(OPL.status | flag);
        if (!((OPL.status & 0x80) != 0))
        {
            if ((OPL.status & OPL.statusmask) != 0)
            {   /* IRQ on */
                OPL.status |= 0x80;
                /* callback user interrupt handler (IRQ is OFF to ON) */
                if (OPL.IRQHandler != null) OPL.IRQHandler(OPL.IRQParam, 1);
            }
        }
    }

#if (BUILD_YM3812 || BUILD_YM3526)
    /*******************************************************************************/
    /*		YM3812 local section                                                   */
    /*******************************************************************************/

    /* ---------- update one of chip ----------- */
    internal static void YM3812UpdateOne(FM_OPL OPL, Span<short> buffer, int length, int stripe, float volume)
    {
        int i;
	    int data;
	    var buf = buffer;
	    uint amsCnt  = (uint)OPL.amsCnt;
	    uint vibCnt  = (uint)OPL.vibCnt;
	    byte rythm = (byte)(OPL.rythm&0x20);
	    Memory<OPL_CH> CH,R_CH;

	    if( MemoryMarshal.Read<FM_OPL>(cur_chip) != OPL ) {
		    MemoryMarshal.Write(cur_chip, OPL);
		    /* channel pointers */
		    S_CH = OPL.P_CH;
		    E_CH = S_CH.Slice(9);
		    /* rythm slot */
		    SLOT7_1 = S_CH.Span[7].SLOT[SLOT1];
		    SLOT7_2 = S_CH.Span[7].SLOT[SLOT2];
		    SLOT8_1 = S_CH.Span[8].SLOT[SLOT1];
		    SLOT8_2 = S_CH.Span[8].SLOT[SLOT2];
		    /* LFO state */
		    amsIncr = OPL.amsIncr;
		    vibIncr = OPL.vibIncr;
		    ams_table = OPL.ams_table;
		    vib_table = OPL.vib_table;
	    }
	    R_CH = rythm != 0 ? S_CH.Slice(6) : E_CH;
        for( i=0; i < length ; i+=stripe )
	    {
            /*            channel A         channel B         channel C      */
            /* LFO */
            amsCnt = (uint)(amsCnt + amsIncr);
		    ams = ams_table.Span[(int)(amsCnt>>AMS_SHIFT)];
            vibCnt = (uint)(vibCnt + vibIncr);
		    vib = vib_table.Span[(int)(vibCnt>>VIB_SHIFT)];
		    outd[0] = 0;
		    /* FM part */
		    for(var j = 0; j < R_CH.Length; j++)
			    OPL_CALC_CH(S_CH.Span[j]);
		    /* Rythm part */
		    if(rythm != 0)
			    OPL_CALC_RH(S_CH);
		    outd[0] = (int)(outd[0] * volume);
		    /* limit check */
		    data = Limit( outd[0], OPL_MAXOUT, OPL_MINOUT );
		    /* store to sound buffer */
		    buf[i] = (short)(data >> OPL_OUTSB);
	    }

	    OPL.amsCnt = (int)amsCnt;
	    OPL.vibCnt = (int)vibCnt;
#if OPL_OUTPUT_LOG
	    if(opl_dbg_fp)
	    {
		    for(opl_dbg_chip=0;opl_dbg_chip<opl_dbg_maxchip;opl_dbg_chip++)
			    if( opl_dbg_opl[opl_dbg_chip] == OPL) break;
		    fprintf(opl_dbg_fp,"%c%c%c",0x20+opl_dbg_chip,length&0xff,length/256);
	    }
#endif
    }
#endif //(BUILD_YM3812 || BUILD_YM3526)

    /* operator output calcrator */
    static int OP_OUT(OPL_SLOT slot, uint env, int con) =>
        slot.wavetable.Span[(int)((slot.Cnt + con) / (0x1000000 / SIN_ENT)) & (SIN_ENT - 1)][env];

    /* ---------- calcrate one of channel ---------- */
    static void OPL_CALC_CH( OPL_CH CH )
    {
	    uint env_out;
	    OPL_SLOT SLOT;

	    feedback2 = 0;
	    /* SLOT 1 */
	    SLOT = CH.SLOT[SLOT1];
	    env_out=OPL_CALC_SLOT(SLOT);
	    if( env_out < EG_ENT-1 )
	    {
		    /* PG */
		    if(SLOT.vib != 0) SLOT.Cnt = (uint)(SLOT.Cnt + (SLOT.Incr*vib/VIB_RATE));
		    else          SLOT.Cnt += SLOT.Incr;
		    /* connection */
		    if(CH.FB != 0)
		    {
			    int feedback1 = (CH.op1_out[0]+CH.op1_out[1])>>CH.FB;
			    CH.op1_out[1] = CH.op1_out[0];
			    CH.connect1 = CH.connect1.Slice(CH.op1_out[0] = OP_OUT(SLOT,env_out,feedback1));
		    }
		    else
		    {
			    CH.connect1 = CH.connect1.Slice(OP_OUT(SLOT,env_out,0));
		    }
	    }else
	    {
		    CH.op1_out[1] = CH.op1_out[0];
		    CH.op1_out[0] = 0;
	    }
	    /* SLOT 2 */
	    SLOT = CH.SLOT[SLOT2];
	    env_out=OPL_CALC_SLOT(SLOT);
	    if( env_out < EG_ENT-1 )
	    {
		    /* PG */
		    if(SLOT.vib != 0) SLOT.Cnt = (uint)(SLOT.Cnt + (SLOT.Incr*vib/VIB_RATE));
		    else          SLOT.Cnt += SLOT.Incr;
		    /* connection */
		    outd[0] += OP_OUT(SLOT,env_out, feedback2);
	    }
    }

    /* ---------- calcrate Envelope Generator & Phase Generator ---------- */
    /* return : envelope output */
    static uint OPL_CALC_SLOT( OPL_SLOT SLOT )
    {
	    /* calcrate envelope generator */
	    if( (SLOT.evc+=SLOT.evs) >= SLOT.eve )
	    {
		    switch( SLOT.evm ){
		    case ENV_MOD_AR: /* ATTACK -> DECAY1 */
			    /* next DR */
			    SLOT.evm = ENV_MOD_DR;
			    SLOT.evc = EG_DST;
			    SLOT.eve = SLOT.SL;
			    SLOT.evs = SLOT.evsd;
			    break;
		    case ENV_MOD_DR: /* DECAY -> SL or RR */
			    SLOT.evc = SLOT.SL;
			    SLOT.eve = EG_DED;
			    if(SLOT.eg_typ != 0)
			    {
				    SLOT.evs = 0;
			    }
			    else
			    {
				    SLOT.evm = ENV_MOD_RR;
				    SLOT.evs = SLOT.evsr;
			    }
			    break;
		    case ENV_MOD_RR: /* RR -> OFF */
			    SLOT.evc = EG_OFF;
			    SLOT.eve = EG_OFF+1;
			    SLOT.evs = 0;
			    break;
		    }
	    }
	    /* calcrate envelope */
	    return (uint)(SLOT.TLL+ENV_CURVE[SLOT.evc>>ENV_BITS]+(SLOT.ams != 0 ? ams : 0));
    }

    /* ---------- calcrate rythm block ---------- */
    const double WHITE_NOISE_db = 6.0;
    static void OPL_CALC_RH( Memory<OPL_CH> CH )
    {
	    uint env_tam,env_sd,env_top,env_hh;
	    int whitenoise = (int)((new Random().Next()&1)*(WHITE_NOISE_db/EG_STEP));
	    int tone8;

	    OPL_SLOT SLOT;
	    int env_out;

	    /* BD : same as FM serial mode and output level is large */
	    feedback2 = 0;
	    /* SLOT 1 */
	    SLOT = CH.Span[6].SLOT[SLOT1];
	    env_out = (int)OPL_CALC_SLOT(SLOT);
	    if( env_out < EG_ENT-1 )
	    {
		    /* PG */
		    if(SLOT.vib != 0) SLOT.Cnt = (uint)(SLOT.Cnt + (SLOT.Incr*vib/VIB_RATE));
		    else          SLOT.Cnt += SLOT.Incr;
		    /* connection */
		    if(CH.Span[6].FB != 0)
		    {
			    int feedback1 = (CH.Span[6].op1_out[0]+CH.Span[6].op1_out[1])>>CH.Span[6].FB;
			    CH.Span[6].op1_out[1] = CH.Span[6].op1_out[0];
			    feedback2 = CH.Span[6].op1_out[0] = OP_OUT(SLOT, (uint)env_out,feedback1);
		    }
		    else
		    {
			    feedback2 = OP_OUT(SLOT, (uint)env_out,0);
		    }
	    }else
	    {
		    feedback2 = 0;
		    CH.Span[6].op1_out[1] = CH.Span[6].op1_out[0];
		    CH.Span[6].op1_out[0] = 0;
	    }
	    /* SLOT 2 */
	    SLOT = CH.Span[6].SLOT[SLOT2];
	    env_out = (int)OPL_CALC_SLOT(SLOT);
	    if( env_out < EG_ENT-1 )
	    {
		    /* PG */
		    if(SLOT.vib != 0) SLOT.Cnt = (uint)(SLOT.Cnt + (SLOT.Incr*vib/VIB_RATE));
		    else          SLOT.Cnt += SLOT.Incr;
		    /* connection */
		    outd[0] += OP_OUT(SLOT, (uint)env_out, feedback2)*2;
	    }

	    // SD  (17) = mul14[fnum7] + white noise
	    // TAM (15) = mul15[fnum8]
	    // TOP (18) = fnum6(mul18[fnum8]+whitenoise)
	    // HH  (14) = fnum7(mul18[fnum8]+whitenoise) + white noise
	    env_sd = (uint)(OPL_CALC_SLOT(SLOT7_2) + whitenoise);
	    env_tam=OPL_CALC_SLOT(SLOT8_1);
	    env_top=OPL_CALC_SLOT(SLOT8_2);
	    env_hh = (uint)(OPL_CALC_SLOT(SLOT7_1) + whitenoise);

	    /* PG */
	    if(SLOT7_1.vib != 0) SLOT7_1.Cnt = (uint)(SLOT7_1.Cnt + (2*SLOT7_1.Incr*vib/VIB_RATE));
	    else             SLOT7_1.Cnt += 2*SLOT7_1.Incr;
	    if(SLOT7_2.vib != 0) SLOT7_2.Cnt = (uint)(SLOT7_2.Cnt + ((CH.Span[7].fc*8)*vib/VIB_RATE));
	    else             SLOT7_2.Cnt += (CH.Span[7].fc*8);
	    if(SLOT8_1.vib != 0) SLOT8_1.Cnt = (uint)(SLOT8_1.Cnt + (SLOT8_1.Incr*vib/VIB_RATE));
	    else             SLOT8_1.Cnt += SLOT8_1.Incr;
	    if(SLOT8_2.vib != 0) SLOT8_2.Cnt = (uint)(SLOT8_2.Cnt + ((CH.Span[8].fc*48)*vib/VIB_RATE));
	    else             SLOT8_2.Cnt += (CH.Span[8].fc*48);

	    tone8 = OP_OUT(SLOT8_2, (uint)whitenoise,0 );

	    /* SD */
	    if( env_sd < EG_ENT-1 )
		    outd[0] += OP_OUT(SLOT7_1,env_sd, 0)*8;
	    /* TAM */
	    if( env_tam < EG_ENT-1 )
		    outd[0] += OP_OUT(SLOT8_1,env_tam, 0)*2;
	    /* TOP-CY */
	    if( env_top < EG_ENT-1 )
		    outd[0] += OP_OUT(SLOT7_2,env_top,tone8)*2;
	    /* HH */
	    if( env_hh  < EG_ENT-1 )
		    outd[0] += OP_OUT(SLOT7_2,env_hh,tone8)*2;
    }
}
