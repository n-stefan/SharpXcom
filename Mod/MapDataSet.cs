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

struct MCD
{
    internal byte[] Frame = new byte[8];
    internal byte[] LOFT = new byte[12];
    internal ushort ScanG;
    byte u23;
    byte u24;
    byte u25;
    byte u26;
    byte u27;
    byte u28;
    byte u29;
    byte u30;
    internal byte UFO_Door;
    internal byte Stop_LOS;
    internal byte No_Floor;
    internal byte Big_Wall;
    internal byte Gravlift;
    internal byte Door;
    internal byte Block_Fire;
    internal byte Block_Smoke;
    byte u39;
    internal byte TU_Walk;
    internal byte TU_Slide;
    internal byte TU_Fly;
    internal byte Armor;
    internal byte HE_Block;
    internal byte Die_MCD;
    internal byte Flammable;
    internal byte Alt_MCD;
    byte u48;
    internal sbyte T_Level;
    internal byte P_Level;
    byte u51;
    internal byte Light_Block;
    internal byte Footstep;
    internal byte Tile_Type;
    internal byte HE_Type;
    internal byte HE_Strength;
    byte Smoke_Blockage;
    internal byte Fuel;
    internal byte Light_Source;
    internal byte Target_Type;
    internal byte Xcom_Base;
    byte u62;

    public MCD() { }

    internal bool Read(BinaryReader reader)
    {
		try
		{
            reader.Read(Frame);
            reader.Read(LOFT);
            ScanG = reader.ReadUInt16();
            u23 = reader.ReadByte();
            u24 = reader.ReadByte();
            u25 = reader.ReadByte();
            u26 = reader.ReadByte();
            u27 = reader.ReadByte();
            u28 = reader.ReadByte();
            u29 = reader.ReadByte();
            u30 = reader.ReadByte();
            UFO_Door = reader.ReadByte();
            Stop_LOS = reader.ReadByte();
            No_Floor = reader.ReadByte();
            Big_Wall = reader.ReadByte();
            Gravlift = reader.ReadByte();
            Door = reader.ReadByte();
            Block_Fire = reader.ReadByte();
            Block_Smoke = reader.ReadByte();
            u39 = reader.ReadByte();
            TU_Walk = reader.ReadByte();
            TU_Slide = reader.ReadByte();
            TU_Fly = reader.ReadByte();
            Armor = reader.ReadByte();
            HE_Block = reader.ReadByte();
            Die_MCD = reader.ReadByte();
            Flammable = reader.ReadByte();
            Alt_MCD = reader.ReadByte();
            u48 = reader.ReadByte();
            T_Level = reader.ReadSByte();
            P_Level = reader.ReadByte();
            u51 = reader.ReadByte();
            Light_Block = reader.ReadByte();
            Footstep = reader.ReadByte();
            Tile_Type = reader.ReadByte();
            HE_Type = reader.ReadByte();
            HE_Strength = reader.ReadByte();
            Smoke_Blockage = reader.ReadByte();
            Fuel = reader.ReadByte();
            Light_Source = reader.ReadByte();
            Target_Type = reader.ReadByte();
            Xcom_Base = reader.ReadByte();
            u62 = reader.ReadByte();
            return true;
        }
        catch (IOException)
		{
			return false;
		}
    }
}

/**
 * Represents a Terrain Map Datafile.
 * Which corresponds to an XCom MCD & PCK file.
 * The list of map datafiles is stored in RuleSet, but referenced in RuleTerrain.
 * @sa http://www.ufopaedia.org/index.php?title=MCD
 */
internal class MapDataSet
{
    static MapData _blankTile = null;
    static MapData _scorchedTile = null;

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
    internal MapData getObject(uint i)
    {
        if (i >= _objects.Count)
        {
            string ss = $"MCD {_name} has no object {i}";
            throw new Exception(ss);
        }
        return _objects[(int)i];
    }

    /**
	 * Loads terrain data in XCom format (MCD & PCK files).
	 * @sa http://www.ufopaedia.org/index.php?title=MCD
	 */
    internal void loadData(MCDPatch patch)
	{
		// prevents loading twice
		if (_loaded) return;
		_loaded = true;

		int objNumber = 0;

		// the struct below helps to read the xcom file format
		//#pragma pack(push, 1)
		//#pragma pack(pop)

		var mcd = new MCD();

		// Load Terrain Data from MCD file
		string fname = "TERRAIN/" + _name + ".MCD";
		try
		{
            using var mapFile = new BinaryReader(new FileStream(FileMap.getFilePath(fname), FileMode.Open));
		    while (mcd.Read(mapFile))
		    {
			    MapData to = new MapData(this);
			    _objects.Add(to);

			    // set all the terrainobject properties:
			    for (int frame = 0; frame < 8; frame++)
			    {
				    to.setSprite(frame, (int)mcd.Frame[frame]);
			    }
			    to.setYOffset((int)mcd.P_Level);
			    to.setSpecialType((int)mcd.Target_Type, (TilePart)mcd.Tile_Type);
			    to.setTUCosts((int)mcd.TU_Walk, (int)mcd.TU_Fly, (int)mcd.TU_Slide);
			    to.setFlags(mcd.UFO_Door != 0, mcd.Stop_LOS != 0, mcd.No_Floor != 0, (int)mcd.Big_Wall, mcd.Gravlift != 0, mcd.Door != 0, mcd.Block_Fire != 0, mcd.Block_Smoke != 0, mcd.Xcom_Base != 0);
			    to.setTerrainLevel((int)mcd.T_Level);
			    to.setFootstepSound((int)mcd.Footstep);
			    to.setAltMCD((int)(mcd.Alt_MCD));
			    to.setDieMCD((int)(mcd.Die_MCD));
			    to.setBlockValue((int)mcd.Light_Block, (int)mcd.Stop_LOS, (int)mcd.HE_Block, (int)mcd.Block_Smoke, (int)mcd.Flammable, (int)mcd.HE_Block);
			    to.setLightSource((int)mcd.Light_Source);
			    to.setArmor((int)mcd.Armor);
			    to.setFlammable((int)mcd.Flammable);
			    to.setFuel((int)mcd.Fuel);
			    to.setExplosiveType((int)mcd.HE_Type);
			    to.setExplosive((int)mcd.HE_Strength);
                //mcd.ScanG = SDL_SwapLE16(mcd.ScanG);
			    to.setMiniMapIndex(mcd.ScanG);

			    for (int layer = 0; layer < 12; layer++)
			    {
				    int loft = (int)mcd.LOFT[layer];
				    to.setLoftID(loft, layer);
			    }

			    // store the 2 tiles of blanks in a static - so they are accessible everywhere
			    if (_name == "BLANKS")
			    {
				    if (objNumber == 0)
					    _blankTile = to;
				    else if (objNumber == 1)
					    _scorchedTile = to;
			    }
			    objNumber++;
		    }

		    if (mapFile.BaseStream.Position != mapFile.BaseStream.Length)
		    {
			    throw new Exception("Invalid MCD file " + fname);
		    }

		    mapFile.Close();
		}
		catch (Exception)
		{
			throw new Exception(fname + " not found");
		}

		// apply any ruleset patches before validation
		if (patch != null)
		{
			patch.modifyData(this);
		}

		// Validate MCD references
		bool validData = true;

		for (int i = 0; i < _objects.Count; ++i)
		{
			if ((uint)_objects[i].getDieMCD() >= _objects.Count)
			{
                Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} MCD {_name} object {i} has invalid DieMCD: {_objects[i].getDieMCD()}");
				validData = false;
			}
			if ((uint)_objects[i].getAltMCD() >= _objects.Count)
			{
                Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} MCD {_name} object {i} has invalid AltMCD: {_objects[i].getAltMCD()}");
				validData = false;
			}
			if (_objects[i].getArmor() == 0)
			{
                Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} MCD {_name} object {i} has 0 armor");
				validData = false;
			}
		}

		if (!validData)
		{
			throw new Exception("invalid MCD file: " + fname + ", check log file for more details.");
		}

		// Load terrain sprites/surfaces/PCK files into a surfaceset
		_surfaceSet = new SurfaceSet(32, 40);
		_surfaceSet.loadPck(FileMap.getFilePath("TERRAIN/" + _name + ".PCK"), FileMap.getFilePath("TERRAIN/" + _name + ".TAB"));
	}

    /**
     * Gets a scorched earth tile.
     * @return Pointer to a scorched earth tile.
     */
    internal static MapData getScorchedEarthTile() =>
        _scorchedTile;

    /**
     * Gets the MapDataSet size.
     * @return The size in number of records.
     */
    internal uint getSize() =>
        (uint)_objects.Count;

    /**
     * Gets the surfaces in this dataset.
     * @return Pointer to the surfaceset.
     */
    internal SurfaceSet getSurfaceset() =>
	    _surfaceSet;
}
