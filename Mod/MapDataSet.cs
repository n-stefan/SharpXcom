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
 * Represents a Terrain Map Datafile.
 * Which corresponds to an XCom MCD & PCK file.
 * The list of map datafiles is stored in RuleSet, but referenced in RuleTerrain.
 * @sa http://www.ufopaedia.org/index.php?title=MCD
 */
internal class MapDataSet
{
    string _name;
    List<MapData> _objects;
    SurfaceSet _surfaceSet;
    bool _loaded;

    /**
     * MapDataSet construction.
     */
    internal MapDataSet(string name)
    {
        _name = name;
        _surfaceSet = null;
        _loaded = false;
    }

    /**
     * MapDataSet destruction.
     */
    ~MapDataSet()
    {
        unloadData();
    }

    /**
     * Unloads the terrain data.
     */
    internal void unloadData()
    {
        if (_loaded)
        {
            _objects.Clear();
            _surfaceSet = null;
            _loaded = false;
        }
    }

    /**
     * Gets the MapDataSet name (string).
     * @return The MapDataSet name.
     */
    internal string getName() =>
	    _name;

    /**
     * Loads the LOFTEMPS.DAT into the ruleset voxeldata.
     * @param filename Filename of the DAT file.
     * @param voxelData The ruleset.
     */
    internal static void loadLOFTEMPS(string filename, List<ushort> voxelData)
    {
        try
        {
            // Load file
            using var mapFile = new FileStream(filename, FileMode.Open);

            ushort value;
            Span<byte> buffer = new byte[2];

	        while (mapFile.Read(buffer) != 0)
            {
		        value = BitConverter.ToUInt16(buffer);
		        voxelData.Add(value);
	        }

            if (mapFile.Position != mapFile.Length)
	        {
		        throw new Exception("Invalid LOFTEMPS");
	        }

	        mapFile.Close();
        }
        catch (Exception)
        {
		    throw new Exception(filename + " not found");
        }
    }

    /**
     * Gets an object in this dataset.
     * @param i Object index.
     * @return Pointer to the object.
     */
    internal MapData getObject(int i)
    {
        if (i >= _objects.Count)
        {
            string ss = $"MCD {_name} has no object {i}";
            throw new Exception(ss);
        }
        return _objects[i];
    }
}
