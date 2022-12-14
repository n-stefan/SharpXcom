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

enum MapDirection { MD_NONE, MD_VERTICAL, MD_HORIZONTAL, MD_BOTH };

enum MapScriptCommand { MSC_UNDEFINED = -1, MSC_ADDBLOCK, MSC_ADDLINE, MSC_ADDCRAFT, MSC_ADDUFO, MSC_DIGTUNNEL, MSC_FILLAREA, MSC_CHECKBLOCK, MSC_REMOVE, MSC_RESIZE };

struct MCDReplacement { int set, entry; };

struct TunnelData
{
    Dictionary<string, MCDReplacement> replacements;
    int level;
    MCDReplacement getMCDReplacement(string type)
	{
		if (!replacements.ContainsKey(type))
		{
            return default;
        }

        return replacements[type];
	}
};

internal class MapScript
{
    MapScriptCommand _type;
    List<SDL_Rect> _rects;
    int _sizeX, _sizeY, _sizeZ, _executionChances, _executions, _cumulativeFrequency, _label;
    MapDirection _direction;
    TunnelData _tunnelData;

    internal MapScript()
    {
        _type = MapScriptCommand.MSC_UNDEFINED;
        _sizeX = 1;
        _sizeY = 1;
        _sizeZ = 0;
        _executionChances = 100;
        _executions = 1;
        _cumulativeFrequency = 0;
        _label = 0;
        _direction = MapDirection.MD_NONE;
        _tunnelData = default;
    }

    ~MapScript()
    {
        _rects.Clear();
        _tunnelData = default;
    }
}
