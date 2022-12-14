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

internal class SoundDefinition : IRule
{
    string _type;
    List<int> _soundList;
    string _catFile;

    SoundDefinition(string type) =>
        _type = type;

	public IRule Create(string type) =>
		new SoundDefinition(type);

    ~SoundDefinition() { }

    internal List<int> getSoundList() =>
	    _soundList;

    internal string getCATFile() =>
	    _catFile;

	internal void load(YamlNode node)
	{
		foreach (var i in ((YamlMappingNode)node["soundRanges"]).Children)
		{
			var key = int.Parse(i.Key.ToString());
			var value = int.Parse(i.Value.ToString());
            for (int j = key; j <= value; ++j)
			{
				_soundList.Add(j);
			}
		}
		foreach (var i in ((YamlSequenceNode)node["sounds"]).Children)
		{
			_soundList.Add(int.Parse(i.ToString()));
		}
        _catFile = node["file"].ToString();
	}
}
