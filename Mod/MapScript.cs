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

struct MCDReplacement
{
	internal int set, entry;
};

struct TunnelData
{
    internal Dictionary<string, MCDReplacement> replacements;
    internal int level;

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
    List<int> _groups, _blocks, _frequencies, _maxUses, _conditionals;
    string _ufoName;

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

	/**
	 * Loads a map script command from YAML.
	 * @param node the YAML node from which to read.
	 */
	internal void load(YamlNode node)
	{
		string command;
		if (node["type"] is YamlNode type)
		{
			command = type != null ? type.ToString() : string.Empty;
			if (command == "addBlock")
				_type = MapScriptCommand.MSC_ADDBLOCK;
			else if (command == "addLine")
				_type = MapScriptCommand.MSC_ADDLINE;
			else if (command == "addCraft")
			{
				_type = MapScriptCommand.MSC_ADDCRAFT;
				_groups.Add(1); // this is a default, and can be overridden
			}
			else if (command == "addUFO")
			{
				_type = MapScriptCommand.MSC_ADDUFO;
				_groups.Add(1); // this is a default, and can be overridden
			}
			else if (command == "digTunnel")
				_type = MapScriptCommand.MSC_DIGTUNNEL;
			else if (command == "fillArea")
				_type = MapScriptCommand.MSC_FILLAREA;
			else if (command == "checkBlock")
				_type = MapScriptCommand.MSC_CHECKBLOCK;
			else if (command == "removeBlock")
				_type = MapScriptCommand.MSC_REMOVE;
			else if (command == "resize")
			{
				_type = MapScriptCommand.MSC_RESIZE;
				_sizeX = _sizeY = 0; // defaults: don't resize anything unless specified.
			}
			else
			{
				throw new Exception("Unknown command: " + command);
			}
		}
		else
		{
			throw new Exception("Missing command type.");
		}

		if (node["rects"] is YamlSequenceNode rects)
		{
			foreach (var i in rects.Children)
			{
				SDL_Rect rect = new SDL_Rect();
				rect.x = int.Parse(i[0].ToString());
				rect.y = int.Parse(i[1].ToString());
				rect.w = int.Parse(i[2].ToString());
				rect.h = int.Parse(i[3].ToString());
				_rects.Add(rect);
			}
		}
		if (node["tunnelData"] is YamlNode tunnelData)
		{
			_tunnelData = new TunnelData();
			_tunnelData.level = int.Parse(tunnelData["level"].ToString());
			if (tunnelData["MCDReplacements"] is YamlSequenceNode data)
			{
				foreach (var i in data.Children)
				{
					MCDReplacement replacement = new MCDReplacement();
					string replacementType = i["type"] != null ? i["type"].ToString() : string.Empty;
					replacement.entry = i["entry"] != null ? int.Parse(i["entry"].ToString()) : -1;
					replacement.set = i["set"] != null ? int.Parse(i["set"].ToString()) : -1;
					_tunnelData.replacements[replacementType] = replacement;
				}
			}
		}
		if (node["conditionals"] is YamlNode conditionals)
		{
			if (conditionals.NodeType == YamlNodeType.Sequence)
			{
                _conditionals = ((YamlSequenceNode)conditionals).Children.Select(x => int.Parse(x.ToString())).ToList();
			}
			else
			{
				_conditionals.Add(int.Parse(conditionals.ToString()));
			}
		}
		if (node["size"] is YamlNode size)
		{
			if (size.NodeType == YamlNodeType.Sequence)
			{
				int[] sizes = { _sizeX, _sizeY, _sizeZ };
				int entry = 0;
				foreach (var i in ((YamlSequenceNode)size).Children)
				{
					sizes[entry] = i != null ? int.Parse(i.ToString()) : 1;
                    entry++;
					if (entry == 3)
					{
						break;
					}
				}
			}
			else
			{
				_sizeX = int.Parse(size.ToString());
				_sizeY = _sizeX;
			}
		}

		if (node["groups"] is YamlNode groups)
		{
			_groups.Clear();
			if (groups.NodeType == YamlNodeType.Sequence)
			{
				foreach (var i in ((YamlSequenceNode)groups).Children)
				{
					_groups.Add(int.Parse(i.ToString()));
				}
			}
			else
			{
				_groups.Add(int.Parse(groups.ToString()));
			}
		}
        uint selectionSize = (uint)_groups.Count;
		if (node["blocks"] is YamlNode blocks)
		{
			_groups.Clear();
			if (blocks.NodeType == YamlNodeType.Sequence)
			{
				foreach (var i in ((YamlSequenceNode)blocks).Children)
				{
					_blocks.Add(int.Parse(i.ToString()));
				}
			}
			else
			{
				_blocks.Add(int.Parse(blocks.ToString()));
			}
			selectionSize = (uint)_blocks.Count;
		}

        _frequencies = Enumerable.Repeat(1, (int)selectionSize).ToList();
		_maxUses = Enumerable.Repeat(-1, (int)selectionSize).ToList();

		if (node["freqs"] is YamlNode freqs)
		{
			if (freqs.NodeType == YamlNodeType.Sequence)
			{
				uint entry = 0;
				foreach (var i in ((YamlSequenceNode)freqs).Children)
				{
					if (entry == selectionSize)
						break;
					_frequencies[(int)entry] = i != null ? int.Parse(i.ToString()) : 1;
					entry++;
				}
			}
			else
			{
				_frequencies[0] = freqs != null ? int.Parse(freqs.ToString()) : 1;
			}
		}
		if (node["maxUses"] is YamlNode maxUses)
		{
			if (maxUses.NodeType == YamlNodeType.Sequence)
			{
				uint entry = 0;
				foreach (var i in ((YamlSequenceNode)maxUses).Children)
				{
					if (entry == selectionSize)
						break;
					_maxUses[(int)entry] = i != null ? int.Parse(i.ToString()) : -1;
					entry++;
				}
			}
			else
			{
				_maxUses[0] = maxUses != null ? int.Parse(maxUses.ToString()) : -1;
			}
		}

		if (node["direction"] is YamlNode direction)
		{
			string sdir = direction != null ? direction.ToString() : string.Empty;
			if (!string.IsNullOrEmpty(sdir))
			{
				char cdir = char.ToUpper(sdir[0]);
				switch (cdir)
				{
					case 'V':
						_direction = MapDirection.MD_VERTICAL;
						break;
					case 'H':
						_direction = MapDirection.MD_HORIZONTAL;
						break;
					case 'B':
						_direction = MapDirection.MD_BOTH;
						break;
					default:
						throw new Exception("direction must be [V]ertical, [H]orizontal, or [B]oth, what does " + sdir + " mean?");
				}
			}
		}

		if (_direction == MapDirection.MD_NONE)
		{
			if (_type == MapScriptCommand.MSC_DIGTUNNEL || _type == MapScriptCommand.MSC_ADDLINE)
			{
				throw new Exception("no direction defined for " + command + " command, must be [V]ertical, [H]orizontal, or [B]oth");
			}
		}


		_executionChances = int.Parse(node["executionChances"].ToString());
		_executions = int.Parse(node["executions"].ToString());
		_ufoName = node["UFOName"].ToString();
		// take no chances, don't accept negative values here.
		_label = Math.Abs(int.Parse(node["label"].ToString()));
	}
}
