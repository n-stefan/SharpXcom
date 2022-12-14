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
    uint size;
    byte[] data;
}

/// MIDI track.
struct track
{
    seq seq;
	uint channel;
}

/// MIDI stream.
struct gmstream
{
    int tempo, nsubs, ntracks;
    seq[] subs = new seq[256];
	track[] tracks = new track[256];

    public gmstream() { }
}

/**
 * Subclass of CatFile to handle gm.cat files
 * that contain MIDI music streams.
 */
internal class GMCatFile : CatFile
{
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

		byte[] raw = load(i);

		if (raw == null)
			return music;

		// stream info
		var stream = new gmstream();
		if (gmext_read_stream(out stream, getObjectSize(i), raw) == -1) {
			raw = null;
			return music;
		}

		//List<byte> midi = new List<byte>(65536);
		var midi = new byte[65536];

		// fields in stream still point into raw
		if (gmext_write_midi(out stream, midi) == -1) {
			raw = null;
			return music;
		}

		raw = null;

		music.load(midi , midi.Length);

		return music;
	}
}
