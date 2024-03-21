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

record struct MCDReplacement
{
	internal int set, entry;
};

struct TunnelData
{
    internal Dictionary<string, MCDReplacement> replacements;
    internal int level;

    internal MCDReplacement getMCDReplacement(string type)
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
    List<int> _groupsTemp, _blocksTemp, _frequenciesTemp, _maxUsesTemp;

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
		if (node["type"] is YamlNode map1)
		{
			command = map1 != null ? map1.ToString() : string.Empty;
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

		if (node["rects"] is YamlSequenceNode map2)
		{
			foreach (var i in map2.Children)
			{
				SDL_Rect rect = new SDL_Rect();
				rect.x = int.Parse(i[0].ToString());
				rect.y = int.Parse(i[1].ToString());
				rect.w = int.Parse(i[2].ToString());
				rect.h = int.Parse(i[3].ToString());
				_rects.Add(rect);
			}
		}
		if (node["tunnelData"] is YamlNode map3)
		{
			_tunnelData = new TunnelData();
			_tunnelData.level = int.Parse(map3["level"].ToString());
			if (map3["MCDReplacements"] is YamlSequenceNode data)
			{
				foreach (var i in data.Children)
				{
					MCDReplacement replacement = new MCDReplacement();
					string type = i["type"] != null ? i["type"].ToString() : string.Empty;
					replacement.entry = i["entry"] != null ? int.Parse(i["entry"].ToString()) : -1;
					replacement.set = i["set"] != null ? int.Parse(i["set"].ToString()) : -1;
					_tunnelData.replacements[type] = replacement;
				}
			}
		}
		if (node["conditionals"] is YamlNode map4)
		{
			if (map4.NodeType == YamlNodeType.Sequence)
			{
                _conditionals = ((YamlSequenceNode)map4).Children.Select(x => int.Parse(x.ToString())).ToList();
			}
			else
			{
				_conditionals.Add(int.Parse(map4.ToString()));
			}
		}
		if (node["size"] is YamlNode map5)
		{
			if (map5.NodeType == YamlNodeType.Sequence)
			{
				int[] sizes = { _sizeX, _sizeY, _sizeZ };
				int entry = 0;
				foreach (var i in ((YamlSequenceNode)map5).Children)
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
				_sizeX = int.Parse(map5.ToString());
				_sizeY = _sizeX;
			}
		}

		if (node["groups"] is YamlNode map6)
		{
			_groups.Clear();
			if (map6.NodeType == YamlNodeType.Sequence)
			{
				foreach (var i in ((YamlSequenceNode)map6).Children)
				{
					_groups.Add(int.Parse(i.ToString()));
				}
			}
			else
			{
				_groups.Add(int.Parse(map6.ToString()));
			}
		}
        uint selectionSize = (uint)_groups.Count;
		if (node["blocks"] is YamlNode map7)
		{
			_groups.Clear();
			if (map7.NodeType == YamlNodeType.Sequence)
			{
				foreach (var i in ((YamlSequenceNode)map7).Children)
				{
					_blocks.Add(int.Parse(i.ToString()));
				}
			}
			else
			{
				_blocks.Add(int.Parse(map7.ToString()));
			}
			selectionSize = (uint)_blocks.Count;
		}

        _frequencies = Enumerable.Repeat(1, (int)selectionSize).ToList();
		_maxUses = Enumerable.Repeat(-1, (int)selectionSize).ToList();

		if (node["freqs"] is YamlNode map8)
		{
			if (map8.NodeType == YamlNodeType.Sequence)
			{
				uint entry = 0;
				foreach (var i in ((YamlSequenceNode)map8).Children)
				{
					if (entry == selectionSize)
						break;
					_frequencies[(int)entry] = i != null ? int.Parse(i.ToString()) : 1;
					entry++;
				}
			}
			else
			{
				_frequencies[0] = map8 != null ? int.Parse(map8.ToString()) : 1;
			}
		}
		if (node["maxUses"] is YamlNode map9)
		{
			if (map9.NodeType == YamlNodeType.Sequence)
			{
				uint entry = 0;
				foreach (var i in ((YamlSequenceNode)map9).Children)
				{
					if (entry == selectionSize)
						break;
					_maxUses[(int)entry] = i != null ? int.Parse(i.ToString()) : -1;
					entry++;
				}
			}
			else
			{
				_maxUses[0] = map9 != null ? int.Parse(map9.ToString()) : -1;
			}
		}

		if (node["direction"] is YamlNode map10)
		{
			string direction = map10 != null ? map10.ToString() : string.Empty;
			if (!string.IsNullOrEmpty(direction))
			{
				char dir = char.ToUpper(direction[0]);
				switch (dir)
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
						throw new Exception("direction must be [V]ertical, [H]orizontal, or [B]oth, what does " + direction + " mean?");
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

    /// Gets the label for this command.
    internal int getLabel() =>
		_label;

    /// Gets what conditions apply to this command.
    internal List<int> getConditionals() =>
		_conditionals;

    /// Get the chances of this command executing.
    internal int getChancesOfExecution() =>
		_executionChances;

    /// Gets how many times this command repeats (1 repeat means 2 executions)
    internal int getExecutions() =>
		_executions;

    /// Gets what type of command this is.
    internal MapScriptCommand getType() =>
		_type;

    /**
     * Gets a random map block from a given terrain, using either the groups or the blocks defined.
     * @param terrain the terrain to pick a block from.
     * @return Pointer to a randomly chosen map block, given the options available.
     */
    internal MapBlock getNextBlock(RuleTerrain terrain)
    {
        if (!_blocks.Any())
        {
            return terrain.getRandomMapBlock(_sizeX * 10, _sizeY * 10, getGroupNumber());
        }
        int result = getBlockNumber();
        if (result < terrain.getMapBlocks().Count && result != (int)MapBlockType.MT_UNDEFINED)
        {
            return terrain.getMapBlocks()[result];
        }
        return null;
    }

    /**
     * Gets a random block number from the array, accounting for frequencies and max uses.
     * If no blocks are defined, it will use a group instead.
     * @return Block number.
     */
    int getBlockNumber()
    {
        if (_cumulativeFrequency > 0)
        {
            int pick = RNG.generate(0, _cumulativeFrequency - 1);
            for (int i = 0; i != _blocksTemp.Count; ++i)
            {
                if (pick < _frequenciesTemp[i])
                {
                    int retVal = _blocksTemp[i];

                    if (_maxUsesTemp[i] > 0)
                    {
                        if (--_maxUsesTemp[i] == 0)
                        {
                            _blocksTemp.RemoveAt(i);
                            _cumulativeFrequency -= _frequenciesTemp[i];
                            _frequenciesTemp.RemoveAt(i);
                            _maxUsesTemp.RemoveAt(i);
                        }
                    }
                    return retVal;
                }
                pick -= _frequenciesTemp[i];
            }
        }
        return (int)MapBlockType.MT_UNDEFINED;
    }

    /**
     * Gets a random group number from the array, accounting for frequencies and max uses.
     * If no groups or blocks are defined, this command will return the default" group,
     * If all the max uses are used up, it will return "undefined".
     * @return Group number.
     */
    int getGroupNumber()
    {
        if (_groups.Count == 0)
        {
            return (int)MapBlockType.MT_DEFAULT;
        }
        if (_cumulativeFrequency > 0)
        {
            int pick = RNG.generate(0, _cumulativeFrequency - 1);
            for (int i = 0; i != _groupsTemp.Count; ++i)
            {
                if (pick < _frequenciesTemp[i])
                {
                    int retVal = _groupsTemp[i];

                    if (_maxUsesTemp[i] > 0)
                    {
                        if (--_maxUsesTemp[i] == 0)
                        {
                            _groupsTemp.RemoveAt(i);
                            _cumulativeFrequency -= _frequenciesTemp[i];
                            _frequenciesTemp.RemoveAt(i);
                            _maxUsesTemp.RemoveAt(i);
                        }
                    }
                    return retVal;
                }
                pick -= _frequenciesTemp[i];
            }
        }
        return (int)MapBlockType.MT_UNDEFINED;
    }

    /// Gets the rects, describing the areas this command applies to.
    internal List<SDL_Rect> getRects() =>
		_rects;

    /// Gets the direction this command goes (for lines and tunnels).
    internal MapDirection getDirection() =>
		_direction;

	/**
	 * Gets the name of the UFO in the case of "setUFO"
	 * @return the UFO name.
	 */
	internal string getUFOName() =>
		_ufoName;

    /// Gets the mcd replacement data for tunnel replacements.
    internal TunnelData getTunnelData() =>
		_tunnelData;

    /// Gets the groups vector for iteration.
    internal List<int> getGroups() =>
		_groups;

    /// Gets the blocks vector for iteration.
    internal List<int> getBlocks() =>
		_blocks;

    /// Gets the X size for this command.
    internal int getSizeX() =>
		_sizeX;

	/// Gets the Y size for this command.
	internal int getSizeY() =>
		_sizeY;

	/// Gets the Z size for this command.
	internal int getSizeZ() =>
		_sizeZ;

    /**
     * Initializes all the various scratch values and such for the command.
     */
    internal void init()
    {
        _cumulativeFrequency = 0;
        _blocksTemp.Clear();
        _groupsTemp.Clear();
        _frequenciesTemp.Clear();
        _maxUsesTemp.Clear();

        foreach (var i in _frequencies)
        {
            _cumulativeFrequency += i;
        }
        _blocksTemp = _blocks;
        _groupsTemp = _groups;
        _frequenciesTemp = _frequencies;
        _maxUsesTemp = _maxUses;
    }
}
