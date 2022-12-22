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

internal class RuleMusic : IRule
{
    string _type, _name;
    int _catPos;
    float _normalization;

    /**
     * initialize catpos as int_max to prevent trying to load files from cats that don't exist,
     * but allow for optional music to be listed regardless for loading .ogg or .mp3 versions
     * of said files, should they be present.
     * default normalization value is 0.76, this only applies to the adlib mixer as far as i know.
     * also, 0.76 is roughly optimal for all the TFTD tracks.
     * @param type String defining the type.
     */
    RuleMusic(string type)
    {
        _type = type;
        _catPos = int.MaxValue;
        _normalization = 0.76f;
    }

    public IRule Create(string type) =>
        new RuleMusic(type);

    ~RuleMusic() { }

    /**
     * Gets the track's index in the catalog file.
     * @return the track's index in the file.
     */
    internal int getCatPos() =>
	    _catPos;

    /**
     * Gets the track's normalization level (Adlib only).
     * @return the track's normalization value.
     */
    internal float getNormalization() =>
	    _normalization;

    /**
     * Gets the track's filename in the SOUND folder.
     * @return the track's filename (no extension).
     */
    internal string getName()
    {
	    if (string.IsNullOrEmpty(_name))
		    return _type;
	    return _name;
    }

    /**
     * Loads info about the music track.
     * @param node yaml node to read from.
     */
    internal void load(YamlNode node)
    {
	    _name = node["name"].ToString();
	    _catPos = int.Parse(node["catPos"].ToString());
	    _normalization = float.Parse(node["normalization"].ToString());
    }
}
