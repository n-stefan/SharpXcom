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
 * For adding a set of extra sprite data to the game.
 */
internal class ExtraSprites
{
    ModData _current;
    int _width, _height;
    bool _singleImage;
    int _subX, _subY;
    bool _loaded;
    string _type;
    Dictionary<int, string> _sprites;

    /**
     * Creates a blank set of extra sprite data.
     */
    internal ExtraSprites()
    {
        _current = default;
        _width = 320;
        _height = 200;
        _singleImage = false;
        _subX = 0;
        _subY = 0;
        _loaded = false;
    }

    /**
     * Cleans up the extra sprite set.
     */
    ~ExtraSprites() { }

    /**
     * Gets the filename that this sprite represents.
     * @return The sprite name.
     */
    internal string getType() =>
	    _type;

    /**
     * Loads the external sprite into a new or existing surface.
     * @param surface Existing surface.
     * @return New surface.
     */
    internal Surface loadSurface(Surface surface)
    {
        if (!_singleImage)
            return surface;
        _loaded = true;

        if (surface == null)
        {
            Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} Creating new single image: {_type}");
        }
        else
        {
            Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} Adding/Replacing single image: {_type}");
            surface = null;
        }
        surface = new Surface(_width, _height);
        surface.loadImage(FileMap.getFilePath(_sprites.First().Value));
        return surface;
    }

    /**
     * Returns if the sprite is loaded.
     * @return True/false
     */
    internal bool isLoaded() =>
	    _loaded;

    /**
     * Returns whether this is a single surface as opposed to a set of surfaces.
     * @return True if this is a single surface.
     */
    internal bool getSingleImage() =>
	    _singleImage;

	/**
	 * Loads the external sprite into a new or existing surface set.
	 * @param set Existing surface set.
	 * @return New surface set.
	 */
	internal SurfaceSet loadSurfaceSet(SurfaceSet set)
	{
		if (_singleImage)
			return set;
		_loaded = true;

		bool subdivision = (_subX != 0 && _subY != 0);
		int surfaceSetX = subdivision ? _subX : _width;
		int surfaceSetY = subdivision ? _subY : _height;
		if (set == null)
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} Creating new surface set: {_type}");
			set = new SurfaceSet(surfaceSetX, surfaceSetY);
		}
		else
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} Adding/Replacing items in surface set: {_type}");
			if (set.getTotalFrames() == 0 && (set.getWidth() != surfaceSetX || set.getHeight() != surfaceSetY))
			{
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} Resize empty set to: {surfaceSetX} x {surfaceSetY}");
				int shared = set.getMaxSharedFrames();
				set = null;
				set = new SurfaceSet(surfaceSetX, surfaceSetY);
				set.setMaxSharedFrames(shared);
			}
		}

		foreach (var sprite in _sprites)
		{
			int startFrame = sprite.Key;
			string fileName = sprite.Value;
			if (fileName[fileName.Length - 1] == '/')
			{
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} Loading surface set from folder: {fileName} starting at frame: {startFrame}");
				int offset = startFrame;
				HashSet<string> contents = FileMap.getVFolderContents(fileName);
				foreach (var item in contents)
				{
					if (!isImageFile(item))
						continue;
					try
					{
						string fullPath = FileMap.getFilePath(fileName + item);
						getFrame(set, offset).loadImage(fullPath);
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
				string fullPath = FileMap.getFilePath(fileName);
				if (!subdivision)
				{
					getFrame(set, startFrame).loadImage(fullPath);
				}
				else
				{
					var temp = new Surface(_width, _height);
					temp.loadImage(fullPath);
					int xDivision = _width / _subX;
					int yDivision = _height / _subY;
					int frames = xDivision * yDivision;
                    Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} Subdividing into {frames} frames.");
					int offset = startFrame;

					for (int y = 0; y != yDivision; ++y)
					{
						for (int x = 0; x != xDivision; ++x)
						{
							Surface frame = getFrame(set, offset);
							// for some reason regular blit() doesn't work here how i want it, so i use this function instead.
							temp.blitNShade(frame, 0 - (x * _subX), 0 - (y * _subY), 0);
							++offset;
						}
					}
				}
			}
		}
		return set;
	}

	Surface getFrame(SurfaceSet set, int index)
	{
		int indexWithOffset = index;
		if (indexWithOffset >= set.getMaxSharedFrames())
		{
			if ((uint)indexWithOffset >= _current.size)
			{
				string err = $"ExtraSprites '{_type}' frame '{indexWithOffset}' exceeds mod '{_current.name}' size limit {_current.size}";
				throw new Exception(err);
			}
			indexWithOffset = (int)(indexWithOffset + _current.offset);
		}
		else if (indexWithOffset < 0)
		{
			string err = $"ExtraSprites '{_type}' frame '{indexWithOffset}' in mod '{_current.name}' is not allowed.";
			throw new Exception(err);
		}

		Surface frame = set.getFrame(indexWithOffset);
		if (frame != null)
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} Replacing frame: {index}, using index: {indexWithOffset}");
			frame.clear();
		}
		else
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} Adding frame: {index}, using index: {indexWithOffset}");
			frame = set.addFrame(indexWithOffset);
		}
		return frame;
	}

	static string[] exts = { "PNG", "GIF", "BMP", "LBM", "IFF", "PCX", "TGA", "TIF", "TIFF" };
	/**
	 * Determines if an image file is an acceptable format for the game.
	 * @param filename Image filename.
	 * @return True/false
	 */
	bool isImageFile(string filename)
	{
		for (var i = 0; i < exts.Length; ++i)
		{
			if (CrossPlatform.compareExt(filename, exts[i]))
				return true;
		}
		return false;
	}
}
