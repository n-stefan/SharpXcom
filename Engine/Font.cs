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

struct FontImage
{
    internal int width, height, spacing;
    internal Surface surface;
};

/**
 * Takes care of loading and storing each character in a sprite font.
 * Sprite fonts consist of a set of characters split in fixed-size regions.
 * @note The characters don't all need to be the same size, they can
 * have blank space and will be automatically lined up properly.
 */
internal class Font
{
    List<FontImage> _images;
    bool _monospace;
    Dictionary<uint, KeyValuePair<uint, SDL_Rect>> _chars;

    /**
     * Initializes the font with a blank surface.
     */
    internal Font() =>
        _monospace = false;

    /**
     * Deletes the font's surface.
     */
    ~Font() =>
        _images.Clear();

    /**
     * Returns the maximum width for any character in the font.
     * @return Width in pixels.
     */
    internal int getWidth() =>
	    _images[0].width;

    /**
     * Returns the maximum height for any character in the font.
     * @return Height in pixels.
     */
    internal int getHeight() =>
	    _images[0].height;

    /**
     * Returns the font's 8bpp palette.
     * @return Pointer to the palette's colors.
     */
    internal SDL_Color[] getPalette() =>
	    _images[0].surface.getPaletteColors();

    /**
     * Replaces a certain amount of colors in the font's palette.
     * @param colors Pointer to the set of colors.
     * @param firstcolor Offset of the first color to replace.
     * @param ncolors Amount of colors to replace.
     */
    internal void setPalette(SDL_Color[] colors, int firstcolor, int ncolors)
    {
        foreach (var image in _images)
        {
            image.surface.setPalette(colors, firstcolor, ncolors);
        }
    }

    /**
     * Returns the spacing between any character in the font.
     * @return Spacing in pixels.
     * @note This does not refer to character spacing within the surface,
     * but to the spacing used between successive characters in a line.
     */
    internal int getSpacing() =>
	    _images[0].spacing;

    /**
     * Returns the dimensions of a particular character in the font.
     * @param c Font character.
     * @return Width and Height dimensions (X and Y are ignored).
     */
    internal SDL_Rect getCharSize(uint c)
    {
        var size = new SDL_Rect { x = 0, y = 0, w = 0, h = 0 };
        if (Unicode.isPrintable(c))
        {
            if (!_chars.ContainsKey(c))
                c = '?';

            FontImage image = _images[(int)_chars[c].Key];
            size.w = _chars[c].Value.w + image.spacing;
            size.h = _chars[c].Value.h + image.spacing;
        }
        else
        {
            if (_monospace)
                size.w = getWidth() + getSpacing();
            else if (c == Unicode.TOK_NBSP)
                size.w = getWidth() / 4;
            else if (c == '\t')
                size.w = getWidth() * 3 / 4;
            else
                size.w = getWidth() / 2;
            size.h = getHeight() + getSpacing();
        }
        // In case anyone mixes them up
        size.x = size.w;
        size.y = size.h;
        return size;
    }

    /**
     * Generates a pre-defined Codepage 437 (MS-DOS terminal) font.
     */
    internal void loadTerminal()
    {
        FontImage image;
        image.width = 9;
        image.height = 16;
        image.spacing = 0;
        _monospace = true;

        var dosFontPtr = Marshal.AllocHGlobal(DOSFONT_SIZE);
        Marshal.Copy(dosFont, 0, dosFontPtr, DOSFONT_SIZE);
        /* SDL_RWops */ IntPtr rw = SDL_RWFromConstMem(dosFontPtr, DOSFONT_SIZE);
        IntPtr s = SDL_LoadBMP_RW(rw, 0);
        SDL_FreeRW(rw);
        Marshal.FreeHGlobal(dosFontPtr);
        SDL_Surface surface = Marshal.PtrToStructure<SDL_Surface>(s);
        image.surface = new Surface(surface.w, surface.h);
        var terminal = new SDL_Color[] { new() { r = 0, g = 0, b = 0, a = 0 }, new() { r = 185, g = 185, b = 185, a = 255 } };
        image.surface.setPalette(terminal, 0, 2);
        SDL_BlitSurface(s, 0, image.surface.getSurface().pixels, 0);
        SDL_FreeSurface(s);
        _images.Add(image);

        string chars = Unicode.convUtf8ToUtf32(" !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~");
        init((uint)(_images.Count - 1), chars);
    }

    /**
     * Calculates the real size and position of each character in
     * the surface and stores them in SDL_Rect's for future use
     * by other classes.
     * @param index The index of the surface to use.
     * @param str A string of characters to map to the surface.
     */
    void init(uint index, string str)
    {
        FontImage image = _images[(int)index];
        Surface surface = image.surface;
	    surface.@lock();
	    int length = (surface.getWidth() / image.width);
	    if (_monospace)
	    {
		    for (int i = 0; i < str.Length; ++i)
		    {
			    SDL_Rect rect;
			    int startX = i % length * image.width;
			    int startY = i / length * image.height;
			    rect.x = startX;
			    rect.y = startY;
			    rect.w = image.width;
			    rect.h = image.height;
			    _chars[str[i]] = KeyValuePair.Create(index, rect);
		    }
	    }
	    else
	    {
		    for (int i = 0; i < str.Length; ++i)
		    {
			    SDL_Rect rect;
			    int left = -1, right = -1;
			    int startX = i % length * image.width;
			    int startY = i / length * image.height;
			    for (int x = startX; x < startX + image.width; ++x)
			    {
				    for (int y = startY; y < startY + image.height && left == -1; ++y)
				    {
					    byte pixel = surface.getPixel(x, y);
					    if (pixel != 0)
					    {
						    left = x;
					    }
				    }
			    }
			    for (int x = startX + image.width - 1; x >= startX; --x)
			    {
				    for (int y = startY + image.height; y-- != startY && right == -1;)
				    {
					    byte pixel = surface.getPixel(x, y);
					    if (pixel != 0)
					    {
						    right = x;
					    }
				    }
			    }
			    rect.x = left;
			    rect.y = startY;
			    rect.w = right - left + 1;
                rect.h = image.height;

                _chars[str[i]] = KeyValuePair.Create(index, rect);
		    }
	    }
	    surface.unlock();
    }

    /**
     * Returns a particular character from the set stored in the font.
     * @param c Character to use for size/position.
     * @return Pointer to the font's surface with the respective
     * cropping rectangle set up.
     */
    internal Surface getChar(uint c)
    {
        if (!_chars.ContainsKey(c))
            c = '?';
        Surface surface = _images[(int)_chars[c].Key].surface;
        surface.getCrop() = _chars[c].Value;
        return surface;
    }

    /**
     * Loads the font from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    int width = int.Parse(node["width"].ToString());
	    int height = int.Parse(node["height"].ToString());
	    int spacing = int.Parse(node["spacing"].ToString());
        _monospace = bool.Parse(node["monospace"].ToString());
	    foreach (var item in ((YamlSequenceNode)node["images"]).Children)
	    {
            var image = new FontImage();
            image.width = item["width"] != null ? int.Parse(item["width"].ToString()) : width;
		    image.height = item["height"] != null ? int.Parse(item["height"].ToString()) : height;
		    image.spacing = item["spacing"] != null ? int.Parse(item["spacing"].ToString()) : spacing;
		    string file = "Language/" + item["file"].ToString();
		    string chars = Unicode.convUtf8ToUtf32(item["chars"].ToString());
		    image.surface = new Surface(image.width, image.height);
		    image.surface.loadImage(FileMap.getFilePath(file));
		    _images.Add(image);
            init((uint)(_images.Count - 1), chars);
	    }
    }
}
