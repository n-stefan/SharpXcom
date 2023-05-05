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
 * An MCD data Patch.
 */
internal class MCDPatch
{
    List<KeyValuePair<uint, int>> _bigWalls, _TUWalks, _TUFlys, _TUSlides, _deathTiles, _terrainHeight, _specialTypes, _armors, _explosives, _flammabilities, _fuels, _HEBlocks, _footstepSounds, _objectTypes;
    List<KeyValuePair<uint, bool>> _noFloors, _stopLOSses;
    List<KeyValuePair<uint, List<int>>> _LOFTS;

    /**
     * Initializes an MCD Patch.
     */
    internal MCDPatch() { }

    /**
     *
     */
    ~MCDPatch() { }

	/**
	 * Loads the MCD Patch from a YAML file.
	 * TODO: fill this out with more data.
	 * @param node YAML node.
	 */
	internal void load(YamlNode node)
	{
		YamlNode data = node["data"];
		foreach (var i in ((YamlSequenceNode)data).Children)
		{
			uint MCDIndex = uint.Parse(i["MCDIndex"].ToString());
			if (i["bigWall"] != null)
			{
				int bigWall = int.Parse(i["bigWall"].ToString());
				_bigWalls.Add(KeyValuePair.Create(MCDIndex, bigWall));
			}
			if (i["TUWalk"] != null)
			{
				int TUWalk = int.Parse(i["TUWalk"].ToString());
				_TUWalks.Add(KeyValuePair.Create(MCDIndex, TUWalk));
			}
			if (i["TUFly"] != null)
			{
				int TUFly = int.Parse(i["TUFly"].ToString());
				_TUFlys.Add(KeyValuePair.Create(MCDIndex, TUFly));
			}
			if (i["TUSlide"] != null)
			{
				int TUSlide = int.Parse(i["TUSlide"].ToString());
				_TUSlides.Add(KeyValuePair.Create(MCDIndex, TUSlide));
			}
			if (i["deathTile"] != null)
			{
				int deathTile = int.Parse(i["deathTile"].ToString());
				_deathTiles.Add(KeyValuePair.Create(MCDIndex, deathTile));
			}
			if (i["terrainHeight"] != null)
			{
				int terrainHeight = int.Parse(i["terrainHeight"].ToString());
				_terrainHeight.Add(KeyValuePair.Create(MCDIndex, terrainHeight));
			}
			if (i["specialType"] != null)
			{
				int specialType = int.Parse(i["specialType"].ToString());
				_specialTypes.Add(KeyValuePair.Create(MCDIndex, specialType));
			}
			if (i["explosive"] != null)
			{
				int explosive = int.Parse(i["explosive"].ToString());
				_explosives.Add(KeyValuePair.Create(MCDIndex, explosive));
			}
			if (i["armor"] != null)
			{
				int armor = int.Parse(i["armor"].ToString());
				_armors.Add(KeyValuePair.Create(MCDIndex, armor));
			}
			if (i["flammability"] != null)
			{
				int flammability = int.Parse(i["flammability"].ToString());
				_flammabilities.Add(KeyValuePair.Create(MCDIndex, flammability));
			}
			if (i["fuel"] != null)
			{
				int fuel = int.Parse(i["fuel"].ToString());
				_fuels.Add(KeyValuePair.Create(MCDIndex, fuel));
			}
			if (i["footstepSound"] != null)
			{
				int footstepSound = int.Parse(i["footstepSound"].ToString());
				_footstepSounds.Add(KeyValuePair.Create(MCDIndex, footstepSound));
			}
			if (i["HEBlock"] != null)
			{
				int HEBlock = int.Parse(i["HEBlock"].ToString());
				_HEBlocks.Add(KeyValuePair.Create(MCDIndex, HEBlock));
			}
			if (i["noFloor"] != null)
			{
				bool noFloor = bool.Parse(i["noFloor"].ToString());
				_noFloors.Add(KeyValuePair.Create(MCDIndex, noFloor));
			}
			if (i["LOFTS"] != null)
			{
                var lofts = ((YamlSequenceNode)i["LOFTS"]).Children.Select(x => int.Parse(x.ToString())).ToList();
				_LOFTS.Add(KeyValuePair.Create(MCDIndex, lofts));
			}
			if (i["stopLOS"] != null)
			{
				bool stopLOS = bool.Parse(i["stopLOS"].ToString());
				_stopLOSses.Add(KeyValuePair.Create(MCDIndex, stopLOS));
			}
			if (i["objectType"] != null)
			{
				int objectType = int.Parse(i["objectType"].ToString());
				_objectTypes.Add(KeyValuePair.Create(MCDIndex, objectType));
			}
		}
	}

	/**
	 * Applies an MCD patch to a mapDataSet.
	 * @param dataSet The MapDataSet we want to modify.
	 */
	internal void modifyData(MapDataSet dataSet)
	{
		foreach (var i in _bigWalls)
		{
			dataSet.getObject(i.Key).setBigWall(i.Value);
		}
		foreach (var i in _TUWalks)
		{
			dataSet.getObject(i.Key).setTUWalk(i.Value);
		}
		foreach (var i in _TUFlys)
		{
			dataSet.getObject(i.Key).setTUFly(i.Value);
		}
		foreach (var i in _TUSlides)
		{
			dataSet.getObject(i.Key).setTUSlide(i.Value);
		}
		foreach (var i in _deathTiles)
		{
			dataSet.getObject(i.Key).setDieMCD(i.Value);
		}
		foreach (var i in _terrainHeight)
		{
			dataSet.getObject(i.Key).setTerrainLevel(i.Value);
		}
		foreach (var i in _specialTypes)
		{
			dataSet.getObject(i.Key).setSpecialType(i.Value, dataSet.getObject(i.Key).getObjectType());
		}
		foreach (var i in _explosives)
		{
			dataSet.getObject(i.Key).setExplosive(i.Value);
		}
		foreach (var i in _armors)
		{
			dataSet.getObject(i.Key).setArmor(i.Value);
		}
		foreach (var i in _flammabilities)
		{
			dataSet.getObject(i.Key).setFlammable(i.Value);
		}
		foreach (var i in _fuels)
		{
			dataSet.getObject(i.Key).setFuel(i.Value);
		}
		foreach (var i in _HEBlocks)
		{
			dataSet.getObject(i.Key).setHEBlock(i.Value);
		}
		foreach (var i in _footstepSounds)
		{
			dataSet.getObject(i.Key).setFootstepSound(i.Value);
		}
		foreach (var i in _objectTypes)
		{
			dataSet.getObject(i.Key).setObjectType((TilePart)i.Value);
		}
		foreach (var i in _noFloors)
		{
			dataSet.getObject(i.Key).setNoFloor(i.Value);
		}
		foreach (var i in _stopLOSses)
		{
			dataSet.getObject(i.Key).setStopLOS(i.Value);
		}
		foreach (var i in _LOFTS)
		{
			int layer = 0;
			foreach (var j in i.Value)
			{
				dataSet.getObject(i.Key).setLoftID(j, layer);
				++layer;
			}
		}
	}
}
