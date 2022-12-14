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
 * Container of a set of sounds.
 * Used to manage file sets that contain a pack
 * of sounds inside.
 */
internal class SoundSet
{
    Dictionary<int, Sound> _sounds;
    int _sharedSounds;

    /**
     * Sets up a new empty sound set.
     */
    internal SoundSet() =>
        _sharedSounds = int.MaxValue;

    /**
     * Deletes the sounds from memory.
     */
    ~SoundSet() =>
        _sounds.Clear();

    /**
     * Returns the total amount of sounds currently
     * stored in the set.
     * @return Number of sounds.
     */
    internal uint getTotalSounds() =>
        (uint)_sounds.Count;

    /**
     * Set number of shared sound indexs that are accessible for all mods.
     */
    internal void setMaxSharedSounds(int i)
    {
        if (i >= 0)
        {
            _sharedSounds = i;
        }
        else
        {
            _sharedSounds = 0;
        }
    }

	/**
	 * Loads the contents of an X-Com CAT file which usually contains
	 * a set of sound files. The CAT starts with an index of the offset
	 * and size of every file contained within. Each file consists of a
	 * filename followed by its contents.
	 * @param filename Filename of the CAT set.
	 * @param wav Are the sounds in WAV format?
	 * @sa http://www.ufopaedia.org/index.php?title=SOUND
	 */
	internal void loadCat(string filename, bool wav)
	{
		// Load CAT file
		var sndFile = new CatFile(filename);
		if (sndFile == null)
		{
			throw new Exception(filename + " not found");
		}

		// Load each sound file
		for (int i = 0; i < sndFile.getAmount(); ++i)
		{
			// Read WAV chunk
			byte[] sound = (byte[])sndFile.load((uint)i);
			uint size = sndFile.getObjectSize((uint)i);

			// If there's no WAV header (44 bytes), add it
			// Assuming sounds are 6-bit 8000Hz (DOS version)
			byte[] newsound = null;
			const int headerSize = 44;
			if (!wav)
			{
				if (size > 5) size -= 5; // skip 5 garbage name bytes at beginning
				if (size != 0) size--; // omit trailing null byte
				if (size != 0)
				{
					byte[] header = { (byte)'R', (byte)'I', (byte)'F', (byte)'F', 0x00, 0x00, 0x00, 0x00, (byte)'W', (byte)'A', (byte)'V', (byte)'E', (byte)'f', (byte)'m', (byte)'t', (byte)' ',
						0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x11, 0x2b, 0x00, 0x00, 0x11, 0x2b, 0x00, 0x00, 0x01, 0x00, 0x08, 0x00,
                        (byte)'d', (byte)'a', (byte)'t', (byte)'a', 0x00, 0x00, 0x00, 0x00 };

					// scale to 8 bits
					for (uint n = 0; n < size; ++n) sound[5 + n] *= 4;

					// copy and do the conversion...
					newsound = new byte[headerSize + size * 2];
					Array.Copy(header, newsound, headerSize);
					Array.Copy(sound, 5, newsound, headerSize, size);
                    int newsize = convertSampleRate(sound.AsSpan(5), size, newsound.AsSpan(headerSize));
					size = (uint)(newsize + headerSize);

					// Rewrite the number of samples in the WAV file
					int headersize = newsize + 36;
					int soundsize = newsize;
					Array.Copy(BitConverter.GetBytes(headersize), 0, newsound, 4, sizeof(int));
					Array.Copy(BitConverter.GetBytes(soundsize), 0, newsound, 40, sizeof(int));
				}
			}
			// so it's WAV, but in 8 khz, we have to convert it to 11 khz sound
			else if (0x40 == sound[0x18] && 0x1F == sound[0x19] && 0x00 == sound[0x1A] && 0x00 == sound[0x1B])
			{
				byte[] sound2 = new byte[size * 2];

				// rewrite the samplerate in the header to 11 khz
				sound[0x18]=0x11; sound[0x19]=0x2B; sound[0x1C]=0x11; sound[0x1D]=0x2B;

				// copy and do the conversion...
				Array.Copy(sound, sound2, size);
				int newsize = convertSampleRate(sound.AsSpan(headerSize), size - headerSize, sound2.AsSpan(headerSize));
				size = (uint)(newsize + headerSize);

				// Rewrite the number of samples in the WAV file
				Array.Copy(BitConverter.GetBytes(newsize), 0, sound2, 0x28, sizeof(int));

				// Ok, now replace the original with the converted:
				sound = null;
				sound = sound2;
			}

			Sound s = new Sound();
			try
			{
				if (size == 0)
				{
					throw new Exception("Invalid sound file");
				}
				if (wav)
					s.load(sound, size);
				else
					s.load(newsound, size);
			}
			catch (Exception)
			{
				// Ignore junk in the file
			}
			_sounds[i] = s;

			sound = null;
			if (!wav)
			{
				newsound = null;
			}
		}
	}

	/**
	 * Converts a 8Khz sample to 11Khz.
	 * @param oldsound Pointer to original sample buffer.
	 * @param oldsize Original buffer size.
	 * @param newsound Pointer to converted sample buffer.
	 * @return Converted buffer size.
	 */
	int convertSampleRate(Span<byte> oldsound, uint oldsize, Span<byte> newsound)
	{
		const int step16 = (8000 << 16) / 11025;
		int newsize = 0;
		for (int i = 0, offset16 = 0; (offset16 >> 16) < oldsize; offset16 += step16, ++i, ++newsize)
		{
			newsound[i] = oldsound[offset16 >> 16];
		}
		return newsize;
    }

	/**
	 * Loads individual contents of a TFTD CAT file by index.
	 * a set of sound files. The CAT starts with an index of the offset
	 * and size of every file contained within. Each file consists of a
	 * filename followed by its contents.
	 * @param filename Filename of the CAT set.
	 * @param index which index in the cat file do we load?
	 * @sa http://www.ufopaedia.org/index.php?title=SOUND
	 */
	internal void loadCatbyIndex(string filename, int index)
	{
		// Load CAT file
		var sndFile = new CatFile(filename);
		if (sndFile == null)
		{
			throw new Exception(filename + " not found");
		}
		if (index >= sndFile.getAmount())
		{
			string err = $"{filename} does not contain {index} sound files.";
			throw new Exception(err);
		}

		// Read WAV chunk
		byte[] sound = (byte[])sndFile.load((uint)index);
		uint size = sndFile.getObjectSize((uint)index);

		// there's no WAV header (44 bytes), add it
		// sounds are 8-bit 11025Hz, signed
		byte[] newsound = null;

		if (size != 0)
		{
			byte[] header = { (byte)'R', (byte)'I', (byte)'F', (byte)'F', 0x00, 0x00, 0x00, 0x00, (byte)'W', (byte)'A', (byte)'V', (byte)'E', (byte)'f', (byte)'m', (byte)'t', (byte)' ',
				0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x11, 0x2b, 0x00, 0x00, 0x11, 0x2b, 0x00, 0x00, 0x01, 0x00, 0x08, 0x00,
				(byte)'d', (byte)'a', (byte)'t', (byte)'a', 0x00, 0x00, 0x00, 0x00 };

			if (size > 5) size -= 5; // skip 5 garbage name bytes at beginning
			if (size != 0) size--; // omit trailing null byte

			int headersize = (int)(size + 36);
			int soundsize = (int)size;
			Array.Copy(BitConverter.GetBytes(headersize), 0, header, 4, sizeof(int));
			Array.Copy(BitConverter.GetBytes(soundsize), 0, header, 40, sizeof(int));

			newsound = new byte[44 + size];
			Array.Copy(header, newsound, 44);

			// TFTD sounds are signed, so we need to convert them.
			for (uint n = 5; n < size + 5; ++n)
			{
				int value = (int)sound[n] + 128;
				sound[n] = (byte)value;
			}

			if (size != 0) Array.Copy(sound, 5, newsound, 44, size);
			size = size + 44;
		}

		Sound s = new Sound();
		try
		{
			if (size == 0)
			{
				throw new Exception("Invalid sound file");
			}
			s.load(newsound, size);
		}
		catch (Exception)
		{
			// Ignore junk in the file
		}
		_sounds[(int)getTotalSounds()] = s;

		sound = null;
		newsound = null;
	}

    /**
     * Returns a particular wave from the sound set.
     * @param i Sound number in the set.
     * @return Pointer to the respective sound.
     */
    internal Sound getSound(uint i)
    {
        if (_sounds.ContainsKey((int)i))
        {
            return _sounds[(int)i];
        }
        return null;
    }

	/**
	 * Gets number of shared sound indexs that are accessible for all mods.
	 */
	internal int getMaxSharedSounds() =>
		_sharedSounds;

    /**
     * Creates and returns a particular wave in the sound set.
     * @param i Sound number in the set.
     * @return Pointer to the respective sound.
     */
    internal Sound addSound(uint i)
    {
        _sounds[(int)i] = new Sound();
		return _sounds[(int)i];
    }
}
