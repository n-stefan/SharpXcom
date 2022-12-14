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
 * Container of a set of surfaces.
 * Used to manage single images that contain series of
 * frames inside, like animated sprites, making them easier
 * to access without constant cropping.
 */
internal class SurfaceSet
{
    Dictionary<int, Surface> _frames;
    int _width, _height;
    int _sharedFrames;

	internal SurfaceSet() { }

    /**
	 * Sets up a new empty surface set for frames of the specified size.
	 * @param width Frame width in pixels.
	 * @param height Frame height in pixels.
	 */
    internal SurfaceSet(int width, int height)
	{
		_width = width;
		_height = height;
		_sharedFrames = int.MaxValue;
	}

	/**
	 * Performs a deep copy of an existing surface set.
	 * @param other Surface set to copy from.
	 */
	internal SurfaceSet(SurfaceSet other)
	{
		_width = other._width;
		_height = other._height;
		_sharedFrames = other._sharedFrames;

		foreach (var frame in other._frames)
		{
            _frames[frame.Key] = new Surface(frame.Value);
		}
	}

	/**
	 * Deletes the images from memory.
	 */
	~SurfaceSet() =>
		_frames.Clear();

    /**
     * Replaces a certain amount of colors in all of the frames.
     * @param colors Pointer to the set of colors.
     * @param firstcolor Offset of the first color to replace.
     * @param ncolors Amount of colors to replace.
     */
    internal void setPalette(SDL_Color[] colors, int firstcolor = 0, int ncolors = 256)
    {
        foreach (var frame in _frames)
        {
            frame.Value.setPalette(colors, firstcolor, ncolors);
        }
    }

    /**
     * Returns a particular frame from the surface set.
     * @param i Frame number in the set.
     * @return Pointer to the respective surface.
     */
    internal Surface getFrame(int i) =>
        _frames.TryGetValue(i, out var surface) ? surface : null;

	/**
	 * Returns the full width of a frame in the set.
	 * @return Width in pixels.
	 */
	internal int getWidth() =>
		_width;

	/**
	 * Returns the full height of a frame in the set.
	 * @return Height in pixels.
	 */
	internal int getHeight() =>
		_height;

    /**
	 * Returns the total amount of frames currently
	 * stored in the set.
	 * @return Number of frames.
	 */
    internal uint getTotalFrames() =>
        (uint)_frames.Count;

	/**
	 * Gets number of shared frame indexs that are accessible for all mods.
	 */
	internal int getMaxSharedFrames() =>
		_sharedFrames;

    /**
     * Set number of shared frame indexs that are accessible for all mods.
     */
    internal void setMaxSharedFrames(int i)
    {
        if (i >= 0)
        {
            _sharedFrames = i;
        }
        else
        {
            _sharedFrames = 0;
        }
    }

    /**
     * Creates and returns a particular frame in the surface set.
     * @param i Frame number in the set.
     * @return Pointer to the respective surface.
     */
    internal Surface addFrame(int i)
    {
        _frames[i] = new Surface(_width, _height);
        return _frames[i];
    }

    internal Dictionary<int, Surface> getFrames() =>
        _frames;

	/**
	 * Loads the contents of an X-Com DAT image file into the
	 * surface. Unlike the PCK, a DAT file is an uncompressed
	 * image with no offsets so these have to be figured out
	 * manually, usually by splitting the image into equal portions.
	 * @param filename Filename of the DAT image.
	 * @sa http://www.ufopaedia.org/index.php?title=Image_Formats#SCR_.26_DAT
	 */
	internal void loadDat(string filename)
	{
		int nframes = 0;

		try
		{
			// Load file and put pixels in surface
			using var imgFile = new FileStream(filename, FileMode.Open);

            //imgFile.Seek(0, SeekOrigin.End);
			var size = imgFile.Length;
			//imgFile.Seek(0, SeekOrigin.Begin);

			nframes = (int)size / (_width * _height);

			for (int i = 0; i < nframes; ++i)
			{
				Surface surface = new Surface(_width, _height);
				_frames[i] = surface;
			}

			int value;
			int x = 0, y = 0, frame = 0;

            // Lock the surface
            _frames[frame].@lock();

			while ((value = imgFile.ReadByte()) != -1)
            {
                _frames[frame].setPixelIterative(ref x, ref y, (byte)value);

                if (y >= _height)
				{
					// Unlock the surface
					_frames[frame].unlock();

					frame++;
					x = 0;
					y = 0;

					if (frame >= nframes)
						break;
					else
						_frames[frame].@lock();
				}
			}

			imgFile.Close();
		}
		catch (Exception)
		{
			throw new Exception(filename + " not found");
		}
	}

	/**
	 * Loads the contents of an X-Com set of PCK/TAB image files
	 * into the surface. The PCK file contains an RLE compressed
	 * image, while the TAB file contains the offsets to each
	 * frame in the image.
	 * @param pck Filename of the PCK image.
	 * @param tab Filename of the TAB offsets.
	 * @sa http://www.ufopaedia.org/index.php?title=Image_Formats#PCK
	 */
	internal void loadPck(string pck, string tab)
	{
		int nframes = 0;

		// Load TAB and get image offsets
		if (!string.IsNullOrEmpty(tab))
		{
			try
			{
				using var offsetFile = new BinaryReader(new FileStream(tab, FileMode.Open));
                long begin, end;
				begin = offsetFile.BaseStream.Position;
				int off = offsetFile.ReadInt32();
				offsetFile.BaseStream.Seek(0, SeekOrigin.End);
				end = offsetFile.BaseStream.Position;
				int size = (int)(end - begin);
				// 16-bit offsets
				if (off != 0)
				{
					nframes = size / 2;
				}
				// 32-bit offsets
				else
				{
					nframes = size / 4;
				}
				offsetFile.Close();
				for (int frame = 0; frame < nframes; ++frame)
				{
					_frames[frame] = new Surface(_width, _height);
				}
			}
			catch (Exception)
			{
				throw new Exception(tab + " not found");
			}
		}
		else
		{
			nframes = 1;
			_frames[0] = new Surface(_width, _height);
		}

		try
		{
			// Load PCK and put pixels in surfaces
			using var imgFile = new FileStream(pck, FileMode.Open);

            byte value;

			for (int frame = 0; frame < nframes; ++frame)
			{
				int x = 0, y = 0;

				// Lock the surface
				_frames[frame].@lock();

                value = (byte)imgFile.ReadByte();
				for (int i = 0; i < value; ++i)
				{
					for (int j = 0; j < _width; ++j)
					{
						_frames[frame].setPixelIterative(ref x, ref y, 0);
					}
				}

				while ((value = (byte)imgFile.ReadByte()) != 255)
				{
					if (value == 254)
					{
                        value = (byte)imgFile.ReadByte();
						for (int i = 0; i < value; ++i)
						{
							_frames[frame].setPixelIterative(ref x, ref y, 0);
						}
					}
					else
					{
						_frames[frame].setPixelIterative(ref x, ref y, value);
					}
				}

				// Unlock the surface
				_frames[frame].unlock();
			}

			imgFile.Close();
		}
		catch (Exception)
		{
			throw new Exception(pck + " not found");
		}
	}
}
