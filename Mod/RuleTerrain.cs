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
 * Represents a specific type of Battlescape Terrain.
 * - the names of the objectsets needed in this specific terrain.
 * - the mapblocks that can be used to build this terrain.
 * @sa http://www.ufopaedia.org/index.php?title=TERRAIN
 */
internal class RuleTerrain : IRule
{
    List<MapBlock> _mapBlocks;
    string _name, _script;
    int _minDepth, _maxDepth, _ambience;
    double _ambientVolume;
    List<MapDataSet> _mapDataSets;
    List<string> _civilianTypes, _music;

    internal RuleTerrain() { }

    /**
     * RuleTerrain construction.
     */
    internal RuleTerrain(string name)
    {
        _name = name;
        _script = "DEFAULT";
        _minDepth = 0;
        _maxDepth = 0;
        _ambience = -1;
        _ambientVolume = 0.5;
    }

    public IRule Create(string type) =>
        new RuleTerrain(type);

    /**
     * Ruleterrain only holds mapblocks. Map datafiles are referenced.
     */
    ~RuleTerrain() =>
        _mapBlocks.Clear();

	/**
	 * Loads the terrain from a YAML file.
	 * @param node YAML node.
	 * @param mod Mod for the terrain.
	 */
	internal void load(YamlNode node, Mod mod)
	{
		if (node["mapDataSets"] is YamlSequenceNode map1)
		{
			_mapDataSets.Clear();
			foreach (var i in map1.Children)
			{
				_mapDataSets.Add(mod.getMapDataSet(i.ToString()));
			}
		}
		if (node["mapBlocks"] is YamlSequenceNode map2)
		{
			_mapBlocks.Clear();
			foreach (var i in map2.Children)
			{
				MapBlock mapBlock = new MapBlock(i["name"].ToString());
				mapBlock.load(i);
				_mapBlocks.Add(mapBlock);
			}
		}
		_name = node["name"].ToString();
		if (node["civilianTypes"] is YamlSequenceNode civs)
		{
            _civilianTypes = civs.Children.Select(x => x.ToString()).ToList();
		}
		else
		{
			_civilianTypes.Add("MALE_CIVILIAN");
			_civilianTypes.Add("FEMALE_CIVILIAN");
		}
		foreach (var i in ((YamlSequenceNode)node["music"]).Children)
		{
			_music.Add(i != null ? i.ToString() : string.Empty);
		}
		if (node["depth"] != null)
		{
			_minDepth = int.Parse(node["depth"][0].ToString());
			_maxDepth = int.Parse(node["depth"][1].ToString());
		}
		mod.loadSoundOffset(_name, _ambience, node["ambience"], "BATTLE.CAT");
		_ambientVolume = double.Parse(node["ambientVolume"].ToString());
		_script = node["script"].ToString();
	}

	/**
	 * Gets The generation script name.
	 * @return The name of the script to use.
	 */
	internal string getScript() =>
		_script;

	/**
	 * Gets the max depth.
	 * @return max depth.
	 */
	internal int getMaxDepth() =>
		_maxDepth;

	/**
	 * Gets the min depth.
	 * @return The min depth.
	 */
	internal int getMinDepth() =>
		_minDepth;

	/**
	 * Gets The list of musics this terrain has to choose from.
	 * @return The list of track names.
	 */
	internal List<string> getMusic() =>
		_music;

	/**
	 * Gets the list of civilian types to use on this terrain (default MALE_CIVILIAN and FEMALE_CIVILIAN)
	 * @return list of civilian types to use.
	 */
	internal List<string> getCivilianTypes() =>
		_civilianTypes;

	/**
	 * Gets The ambient sound effect.
	 * @return The ambient sound effect.
	 */
	internal int getAmbience() =>
		_ambience;

	internal double getAmbientVolume() =>
		_ambientVolume;

    /**
     * Gets the array of mapdatafiles.
     * @return Pointer to the array of mapdatafiles.
     */
    internal List<MapDataSet> getMapDataSets() =>
        _mapDataSets;

    /**
     * Gets a random mapblock within the given constraints.
     * @param maxsize The maximum size of the mapblock (10 or 20 or 999 - don't care).
     * @param type Whether this must be a block of a certain type.
     * @param force Whether to enforce the max size.
     * @return Pointer to the mapblock.
     */
    internal MapBlock getRandomMapBlock(int maxSizeX, int maxSizeY, int group, bool force = true)
    {
        var compliantMapBlocks = new List<MapBlock>();

        foreach (var i in _mapBlocks)
        {
            if ((i.getSizeX() == maxSizeX ||
                (!force && i.getSizeX() < maxSizeX)) &&
                (i.getSizeY() == maxSizeY ||
                (!force && i.getSizeY() < maxSizeY)) &&
                i.isInGroup(group))
            {
                compliantMapBlocks.Add(i);
            }
        }

        if (!compliantMapBlocks.Any()) return null;

        int n = RNG.generate(0, compliantMapBlocks.Count - 1);

        return compliantMapBlocks[n];
    }

    /**
     * Gets the array of mapblocks.
     * @return Pointer to the array of mapblocks.
     */
    internal List<MapBlock> getMapBlocks() =>
        _mapBlocks;

	/**
	 * Gets a mapdata object.
	 * @param id The id in the terrain.
	 * @param mapDataSetID The id of the map data set.
	 * @return Pointer to MapData object.
	 */
	internal MapData getMapData(ref uint id, ref int mapDataSetID)
	{
		MapDataSet mdf = null;
		int i;
		for (i = 0; i < _mapDataSets.Count; ++i)
		{
			mdf = _mapDataSets[i];
			if (id < mdf.getSize())
			{
				break;
			}
			id -= mdf.getSize();
			(mapDataSetID)++;
		}
		if (i >= _mapDataSets.Count)
		{
			// oops! someone at microprose made an error in the map!
			// set this broken tile reference to BLANKS 0.
			mdf = _mapDataSets.First();
			id = 0;
			mapDataSetID = 0;
		}
		return mdf.getObject(id);
	}

	/**
	 * Gets a mapblock with a given name.
	 * @param name The name of the mapblock.
	 * @return Pointer to mapblock.
	 */
	internal MapBlock getMapBlock(string name)
	{
		foreach (var i in _mapBlocks)
		{
			if (i.getName() == name)
				return i;
		}
		return null;
	}

	/**
	 * Gets the terrain name.
	 * @return The terrain name.
	 */
	internal string getName() =>
		_name;
}
