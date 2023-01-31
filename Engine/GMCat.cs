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

/// MIDI sequence.
struct seq
{
    internal uint size;
    internal byte[] data;
}

/// MIDI track.
struct track
{
    internal seq seq;
	internal uint channel;
}

/// MIDI stream.
struct gmstream
{
    internal int tempo, nsubs, ntracks;
    internal seq[] subs = new seq[256];
	internal track[] tracks = new track[256];

    public gmstream() { }
}

/// Output status.
struct output_status
{
    internal uint delta;
    internal uint patch;
    internal byte prevcmd;
};

/**
 * Subclass of CatFile to handle gm.cat files
 * that contain MIDI music streams.
 */
internal class GMCatFile : CatFile
{
	static uint[] volume = {
		100,100,100,100,100, 90,100,100,100,100,100, 90,100,100,100,100,
		100,100, 85,100,100,100,100,100,100,100,100,100, 90,90, 110, 80,
		100,100,100, 90, 70,100,100,100,100,100,100,100,100,100,100,100,
		100,100, 90,100,100,100,100,100,100,120,100,100,100,120,100,127,
		100,100, 90,100,100,100,100,100,100, 95,100,100,100,100,100,100,
		100,100,100,100,100,100,100,115,100,100,100,100,100,100,100,100,
		100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,
		100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,
	};

    /// Inherit constructor.
    internal GMCatFile(string path) : base(path) { }

	/**
	 * Loads a MIDI object into memory.
	 * @param i Music number to load.
	 * @return Pointer to the loaded music.
	 */
	internal Music loadMIDI(uint i)
	{
		var music = new Music();

		Span<byte> raw = load(i);

		if (raw == null)
			return music;

		// stream info
		var stream = new gmstream();
		if (gmext_read_stream(ref stream, getObjectSize(i), raw) == -1) {
			raw = null;
			return music;
		}

		var midi = new List<byte>(65536);

		// fields in stream still point into raw
		if (gmext_write_midi(ref stream, midi) == -1) {
			raw = null;
			return music;
		}

		raw = null;

        music.load(midi, midi.Count);

		return music;
	}

	static uint read_uint32_le(Span<byte> p) =>
		p[0] + (((uint)p[1]) << 8) + (((uint)p[2]) << 16) + (((uint)p[3]) << 24);

	static int gmext_read_stream(ref gmstream p, uint n, Span<byte> data)
	{
		if (n-- == 0)
			return -1;
		p.tempo = data[0]; data = data[1..];

		// subsequences
		if (n-- == 0)
			return -1;
		p.nsubs = data[0]; data = data[1..];

		for (int i=0; i<p.nsubs; ++i) {
			if (n < 4)
				return -1;
			uint s = read_uint32_le(data);
			if (s < 4)
				return -1;
			p.subs[i].size = s - 4;
			p.subs[i].data = data[4..(int)s].ToArray();
			n -= s;
			data = data[(int)s..];
		}

		// tracks
		if (n-- == 0)
			return -1;
		p.ntracks = data[0]; data = data[1..];

		for (int i=0; i<p.ntracks; ++i) {
			if (n-- < 5)
				return -1;
			p.tracks[i].channel = data[0]; data = data[1..];
			uint s = read_uint32_le(data);
			if (s < 4)
				return -1;
			p.tracks[i].seq.size = s - 4;
			p.tracks[i].seq.data = data[4..(int)s].ToArray();
			n -= s;
			data = data[(int)s..];
		}

		return n != 0 ? -1 : 0;
	}

	static byte[] midi_file_signature = { (byte)'M', (byte)'T', (byte)'h', (byte)'d', 0, 0, 0, 6 };
	static byte[] midi_track_header = { (byte)'M', (byte)'T', (byte)'r', (byte)'k', 0, 0, 0, 11 };
	static byte[] midi_track_init = { /* 0, 0xB0, */ 0x78, 0, 0, 0x79, 0, 0, 0x7B, 0 };
	static int gmext_write_midi(ref gmstream stream, List<byte> midi)
	{
		// write MIDI file header
		for (int i=0; i<8; ++i)
			midi.Add(midi_file_signature[i]);
		gmext_write_int16(midi, 1);
		gmext_write_int16(midi, (uint)(stream.ntracks + 1));
		gmext_write_int16(midi, 24);

		// write global tempo track
		for (int i=0; i<8; ++i)
			midi.Add(midi_track_header[i]);
		gmext_write_delta(midi, 0);
		gmext_write_tempo_ev(midi, (uint)stream.tempo);
		gmext_write_delta(midi, 0);
		gmext_write_end_ev(midi);

		// write tracks
		for (int j=0; j<stream.ntracks; ++j) {

			// header
			for (int i=0; i<4; ++i)
				midi.Add(midi_track_header[i]);

            int loffset = midi.Count;
			for (int i=0; i<4; ++i)
				midi.Add(0);

			// initial data
			midi.Add(0);
			midi.Add((byte)(0xB0 | stream.tracks[j].channel));
			for (int i=0; i<8; ++i)
				midi.Add(midi_track_init[i]);

			// body
			var status = new output_status { delta = 0, patch = 0, prevcmd = 0 };
			if (gmext_write_sequence(midi, stream,
					stream.tracks[j].channel,
					ref stream.tracks[j].seq, ref status) == -1)
				return -1;

			// end of track
			gmext_write_delta(midi, status.delta);
			gmext_write_end_ev(midi);

            // rewrite track length
			int length = midi.Count - loffset - 4;
            midi[loffset] = (byte)(length >> 24);
            midi[loffset + 1] = (byte)(length >> 16);
            midi[loffset + 2] = (byte)(length >> 8);
            midi[loffset + 3] = (byte)length;
		}

		return 0;
	}

    static void gmext_write_int16(List<byte> midi, uint n)
    {
        midi.Add((byte)(n >> 8));
        midi.Add((byte)n);
    }

    static void gmext_write_delta(List<byte> midi, uint delta)
    {
        var data = new byte[4];
        uint i = 0;

        delta &= ((1 << 28) - 1);
        do
        {
            data[i++] = (byte)(delta & 0x7F);
            delta >>= 7;
        } while (delta > 0 && i <= 3);

        while (--i != 0)
            midi.Add((byte)(data[i] | 0x80));

        midi.Add(data[0]);
    }

    static void gmext_write_end_ev(List<byte> midi)
    {
        midi.Add(0xFF);
        midi.Add(0x2F);
        midi.Add(0);
    }

    static void gmext_write_tempo_ev(List<byte> midi, uint tempo)
    {
        midi.Add(0xFF);
        midi.Add(0x51);
        midi.Add(3);
        tempo = 60000000 / tempo;
        midi.Add((byte)(tempo >> 16));
        midi.Add((byte)(tempo >> 8));
        midi.Add((byte)tempo);
    }

	static int gmext_write_sequence(List<byte> midi, gmstream stream, uint channel, ref seq seq, ref output_status status)
	{
		Span<byte> data = seq.data;
		uint left = seq.size;

		byte cmd;
        unchecked { cmd = (byte)-1; }

		while (left != 0) {

			// read delta
			uint ndelta = 0;

			for (int i=0; ; ) {
                byte c = data[0]; data = data[1..];
				left--;
                ndelta += (uint)(c & 0x7F);
				if ((c & 0x80) == 0)
					break;
				if ((++i == 4) || left == 0)
					return -1;
				ndelta <<= 7;
			}

			status.delta += ndelta;

			// read cmd byte
			if (left == 0)
				return -1;

			if ((data[0] & 0x80) != 0) {
				// actual cmd byte
				cmd = data[0]; data = data[1..];
				left--;
				switch (cmd) {
					case 0xFF:	  // end track
					case 0xFD:	  // end subsequence
						return 0;
					case 0xFE:	  // insert subsequence
						if (left-- == 0)
							return -1;
						if (data[0] >= stream.nsubs)
							// invalid subsequence
							return -1;
						if (gmext_write_sequence(midi,
							stream, channel,
							ref stream.subs[data[0]], ref status)
								== -1)
							return -1;
                        data = data[1..];
						cmd = 0;
						continue;
					default:
						cmd &= 0xF0;
						break;
				}
			} else if (cmd == 0)
				return -1;	  // invalid running mode

			if (left-- == 0)
				return -1;
			byte data1 = data[0]; data = data[1..];

			switch (cmd) {

				case 0x80:
				case 0x90: {
					if (left-- == 0)
						return -1;
					byte data2 = data[0]; data = data[1..];
					if (data2 != 0)
						data2 = (byte)((uint) data2 *
							(channel==9 ? 80 : volume[status.patch]) >> 7);
					gmext_write_delta(midi, status.delta);
					midi.Add((byte)(cmd | channel));
					midi.Add(data1);
					midi.Add(data2);
					} break;

				case 0xC0:
					if (data1 == 0x7E)
						return 0;	   // restart stream
					status.patch = data1;
					if ((data1 == 0x57) || (data1 == 0x3F))
						data1 = 0x3E;
					gmext_write_delta(midi, status.delta);
					midi.Add((byte)(cmd | channel));
					midi.Add(data1);
					break;

				case 0xB0: {
					if (left-- == 0)
						return -1;
						byte data2 = data[0]; data = data[1..];
					if (data1 == 0x7E)
						continue;
					if (data1 == 0) {
						if (data2 == 0)
							continue;
						gmext_write_delta(midi, status.delta);
						gmext_write_tempo_ev(midi, (uint)(2 * data2));
						break;
					}
					if (data1 == 0x5B)
						data2 = 0x1E;
					gmext_write_delta(midi, status.delta);
					midi.Add((byte)(cmd | channel));
					midi.Add(data1);
					midi.Add(data2);
					} break;

				case 0xE0: {
					if (left-- == 0)
						return -1;
					byte data2 = data[0]; data = data[1..];
					gmext_write_delta(midi, status.delta);
					midi.Add((byte)(cmd | channel));
					midi.Add(data1);
					midi.Add(data2);
					} break;

				default:		// unhandled cmd byte
					return -1;
			}

			status.delta = 0;
		}

		return 0;
	}
}
