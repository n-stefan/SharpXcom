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

namespace SharpXcom.Mod;

/**
 * For adding a set of extra sound data to the game.
 */
internal class ExtraSounds
{
    ModData _current;
    string _type;
    Dictionary<int, string> _sounds;

    /**
     * Creates a blank set of extra sound data.
     */
    internal ExtraSounds() =>
        _current = default;

    /**
     * Cleans up the extra sound set.
     */
    ~ExtraSounds() { }

	/**
	 * Loads the external sounds into a new or existing soundset.
	 * @param set Existing soundset.
	 * @return New soundset.
	 */
	internal SoundSet loadSoundSet(SoundSet set)
	{
		if (set == null)
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} Creating new sound set: {_type}, this will likely have no in-game use.");
			set = new SoundSet();
		}
		else
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} Adding/Replacing items in sound set: {_type}");
		}
		foreach (var sound in _sounds)
		{
			int startSound = sound.Key;
			string fileName = sound.Value;
			if (fileName[fileName.Length - 1] == '/')
			{
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} Loading sound set from folder: {fileName} starting at index: {startSound}");
				int offset = startSound;
				HashSet<string> contents = FileMap.getVFolderContents(fileName);
				foreach (var item in contents)
				{
					try
					{
						loadSound(set, offset, fileName + item);
						offset++;
					}
					catch (Exception e)
					{
                        Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {e.Message}");
					}
				}
			}
			else
			{
				loadSound(set, startSound, fileName);
			}
		}
		return set;
	}

	void loadSound(SoundSet set, int index, string fileName)
	{
		uint indexWithOffset = (uint)index;
		if (indexWithOffset >= set.getMaxSharedSounds())
		{
			if (indexWithOffset >= _current.size)
			{
				string err = $"ExtraSounds '{_type}' sound '{indexWithOffset}' exceeds mod '{_current.name}' size limit {_current.size}";
				throw new Exception(err);
			}
			indexWithOffset += _current.offset;
		}

		string fullPath = FileMap.getFilePath(fileName);
		Sound sound = set.getSound(indexWithOffset);
		if (sound != null)
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} Replacing sound: {index}, using index: {indexWithOffset}");
		}
		else
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} Adding sound: {index}, using index: {indexWithOffset}");
			sound = set.addSound(indexWithOffset);
		}
		sound.load(fullPath);
	}
}
