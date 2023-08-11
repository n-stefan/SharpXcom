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

namespace SharpXcom.Savegame;

/**
 * Container for mission statistics.
 */
internal struct MissionStatistics
{
    // Variables
    internal int id;
    int markerId;
    internal GameTime time;
    internal string region, country, type, ufo;
    internal bool success;
    internal int score;
    internal string alienRace;
    int daylight;
    bool valiantCrux;
    int lootValue;
    string markerName;
    string rating;
    internal Dictionary<int, int> injuryList;

    public MissionStatistics()
    {
        id = 0;
        markerId = 0;
        time = null;
        region = "STR_REGION_UNKNOWN";
        country = "STR_UNKNOWN";
        ufo = "NO_UFO";
        success = false;
        score = 0;
        alienRace = "STR_UNKNOWN";
        daylight = 0;
        valiantCrux = false;
        lootValue = 0;
    }

	/// Save
	internal YamlNode save()
	{
        var node = new YamlMappingNode
        {
            { "id", id.ToString() }
        };
        if (!string.IsNullOrEmpty(markerName))
		{
			node.Add("markerName", markerName);
			node.Add("markerId", markerId.ToString());
		}
		node.Add("time", time.save());
		node.Add("region", region);
		node.Add("country", country);
		node.Add("type", type);
		node.Add("ufo", ufo);
		node.Add("success", success.ToString());
		node.Add("score", score.ToString());
		node.Add("rating", rating);
		node.Add("alienRace", alienRace);
		node.Add("daylight", daylight.ToString());
        node.Add("injuryList", new YamlSequenceNode(injuryList.Select(x => new YamlMappingNode(x.Key.ToString(), x.Value.ToString()))));
		if (valiantCrux) node.Add("valiantCrux", valiantCrux.ToString());
		if (lootValue != 0) node.Add("lootValue", lootValue.ToString());
        return node;
	}

	internal string getMissionName(Language lang)
	{
		if (!string.IsNullOrEmpty(markerName))
		{
			return lang.getString(markerName).arg(markerId);
		}
		else
		{
			return lang.getString(type);
		}
	}

	internal string getRatingString(Language lang)
	{
		string ss;
		if (success)
		{
			ss = lang.getString("STR_VICTORY");
		}
		else
		{
			ss = lang.getString("STR_DEFEAT");
		}
		ss = $"{ss} - {lang.getString(rating)}";
		return ss;
	}

    internal bool isUfoMission()
	{
		if (ufo != "NO_UFO")
		{
			return true;
		}
		return false;
	}

    internal string getLocationString()
	{
		if (country == "STR_UNKNOWN")
		{
			return region;
		}
		else
		{
			return country;
		}
	}

    internal string getDaylightString()
	{
		if (isDarkness())
		{
			return "STR_NIGHT";
		}
		else
		{
			return "STR_DAY";
		}
	}

    internal bool isDarkness() =>
		daylight > TileEngine.MAX_DARKNESS_TO_SEE_UNITS;

	internal bool isAlienBase()
	{
		if (type.Contains("STR_ALIEN_BASE") || type.Contains("STR_ALIEN_COLONY"))
		{
			return true;
		}
		return false;
	}

	internal bool isBaseDefense()
	{
		if (type == "STR_BASE_DEFENSE")
		{
			return true;
		}
		return false;
	}
}
