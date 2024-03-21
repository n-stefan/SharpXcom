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
 * Subclass of std::ifstream to handle CAT files
 */
internal class CatFile : BinaryReader
{
    uint _amount;
    uint[] _offset, _size;

	/**
	 * Creates a CAT file stream. A CAT file starts with an index of the
	 * offset and size of every file contained within. Each file consists
	 * of a filename followed by its contents.
	 * @param path Full path to CAT file.
	 */
	internal CatFile(string path) : base(new FileStream(path, FileMode.Open))
    {
        _amount = 0;
		_offset = null;
		_size = null;

        // Get amount of files
        _amount = ReadUInt32();

		_amount /= 2 * sizeof(uint);

		// Get object offsets
		BaseStream.Seek(0, SeekOrigin.Begin);

		_offset = new uint[_amount];
		_size   = new uint[_amount];

		for (var i = 0; i < _amount; ++i)
		{
            _offset[i] = ReadUInt32();
            _size[i] = ReadUInt32();
		}
	}

	/**
	 * Frees associated memory.
	 */
	~CatFile()
	{
		_offset = null;
		_size = null;

		Close();
	}

    /// Get amount of objects.
    internal int getAmount() =>
        (int)_amount;

    /// Get object size.
    internal uint getObjectSize(uint i) =>
		(i<_amount) ? _size[i] : 0;

    /**
     * Loads an object into memory.
     * @param i Object number to load.
     * @param name Preserve internal file name.
     * @return Pointer to the loaded object.
     */
    internal byte[] load(uint i, bool name = false)
    {
        if (i >= _amount)
            return null;

        BaseStream.Seek(_offset[i], SeekOrigin.Begin);

        byte namesize = (byte)PeekChar();
        // Skip filename (if there's any)
        if (namesize <= 56)
        {
            if (!name)
            {
                BaseStream.Seek(namesize + 1, SeekOrigin.Current);
            }
            else
            {
                _size[i] = _size[i] + namesize + 1;
            }
        }

        // Read object
        byte[] @object = new byte[_size[i]];
        Read(@object, 0, (int)_size[i]);

        return @object;
    }
}
