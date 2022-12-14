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
    int id;
    int markerId;
    GameTime time;
    string region, country, type, ufo;
    bool success;
    int score;
    string alienRace;
    int daylight;
    bool valiantCrux;
    int lootValue;
    string markerName;
    string rating;
    Dictionary<int, int> injuryList;

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
}
