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

enum SpecialTileType
{
    TILE = 0,
    START_POINT,
    UFO_POWER_SOURCE,
    UFO_NAVIGATION,
    UFO_CONSTRUCTION,
    ALIEN_FOOD,
    ALIEN_REPRODUCTION,
    ALIEN_ENTERTAINMENT,
    ALIEN_SURGERY,
    EXAM_ROOM,
    ALIEN_ALLOYS,
    ALIEN_HABITAT,
    DEAD_TILE,
    END_POINT,
    MUST_DESTROY
};

enum VoxelType { V_EMPTY = -1, V_FLOOR, V_WESTWALL, V_NORTHWALL, V_OBJECT, V_UNIT, V_OUTOFBOUNDS };

enum TilePart { O_FLOOR, O_WESTWALL, O_NORTHWALL, O_OBJECT };

enum MovementType { MT_WALK, MT_FLY, MT_SLIDE, MT_FLOAT, MT_SINK };

/**
 * MapData is the smallest piece of a Battlescape terrain, holding info about a certain object, wall, floor, ...
 * @sa MapDataSet.
 */
internal class MapData
{
    MapDataSet _dataset;
    SpecialTileType _specialType;
    bool _isUfoDoor, _stopLOS, _isNoFloor, _isGravLift, _isDoor, _blockFire, _blockSmoke, _baseModule;
    int _yOffset, _TUWalk, _TUFly, _TUSlide, _terrainLevel, _footstepSound, _dieMCD, _altMCD;
    TilePart _objectType;
    int _lightSource;
    int _armor, _flammable, _fuel, _explosive, _explosiveType, _bigWall;
    ushort _miniMapIndex;
    int[] _loftID = new int[12];
    int[] _block = new int[6];
    int[] _sprite = new int[8];

    /**
     * Creates a new Map Data Object.
     * @param dataset The dataset this object belongs to.
     */
    internal MapData(MapDataSet dataset)
    {
        _dataset = dataset;
        _specialType = SpecialTileType.TILE;
        _isUfoDoor = false;
        _stopLOS = false;
        _isNoFloor = false;
        _isGravLift = false;
        _isDoor = false;
        _blockFire = false;
        _blockSmoke = false;
        _baseModule = false;
        _yOffset = 0;
        _TUWalk = 0;
        _TUFly = 0;
        _TUSlide = 0;
        _terrainLevel = 0;
        _footstepSound = 0;
        _dieMCD = 0;
        _altMCD = 0;
        _objectType = TilePart.O_FLOOR;
        _lightSource = 0;
        _armor = 0;
        _flammable = 0;
        _fuel = 0;
        _explosive = 0;
        _explosiveType = 0;
        _bigWall = 0;
        _miniMapIndex = 0;
    }

    /**
     * Destroys the object.
     */
    ~MapData() { }

    /**
     * Adds this to the graphical Y offset of units or objects on this tile.
     * @return The Y offset.
     */
    internal int getTerrainLevel() =>
	    _terrainLevel;

    /**
     * Gets whether this is a big wall, which blocks all surrounding paths.
     *
     * Return value key:
     * 0: not a bigWall
     * 1: regular bigWall
     * 2: allows movement in ne/sw direction
     * 3: allows movement in nw/se direction
     * 4: acts as a west wall
     * 5: acts as a north wall
     * 6: acts as an east wall
     * 7: acts as a south wall
     * 8: acts as a south and east wall.
     * 9: acts as a north and west wall.
     * @return An integer representing what kind of bigwall this is.
     */
    internal int getBigWall() =>
	    _bigWall;

    /**
     * Gets whether this is a floor.
     * @return True if this is a floor.
     */
    internal bool isNoFloor() =>
	    _isNoFloor;

    /**
     * Gets whether this is a normal door.
     * @return True if this is a normal door.
     */
    internal bool isDoor() =>
	    _isDoor;

    /**
     * Gets whether this is an animated ufo door.
     * @return True if this is an animated ufo door.
     */
    internal bool isUFODoor() =>
	    _isUfoDoor;

    /**
     * Gets the loft index for a certain layer.
     * @param layer The layer.
     * @return The loft index.
     */
    internal int getLoftID(int layer) =>
	    _loftID[layer];

    /**
     * Gets whether this is a grav lift.
     * @return True if this is a grav lift.
     */
    internal bool isGravLift() =>
	    _isGravLift;

    /**
     * Gets the TU cost to walk over the object.
     * @param movementType The movement type.
     * @return The TU cost.
     */
    internal int getTUCost(MovementType movementType)
    {
	    switch (movementType)
	    {
	        case MovementType.MT_WALK:
		        return _TUWalk;
	        case MovementType.MT_FLY:
		        return _TUFly;
	        case MovementType.MT_SLIDE:
		        return _TUSlide;
	        default:
		        break;
	    }
	    return 0;
    }

    /**
     * Gets the amount of light the object is emitting.
     * @return The amount of light emitted.
     */
    internal int getLightSource()
    {
	    // lamp posts have 1, but they should emit more light
	    if (_lightSource == 1)
		    return 15;
	    else
		    return _lightSource - 1;
    }

    /**
     * Gets the amount of blockage of a certain type.
     * @param type Type.
     * @return The blockage (0-255).
     */
    internal int getBlock(ItemDamageType type)
    {
	    switch (type)
	    {
	        case ItemDamageType.DT_NONE:
		        return _block[1];
	        case ItemDamageType.DT_SMOKE:
		        return _block[3];
	        case ItemDamageType.DT_HE:
	        case ItemDamageType.DT_IN:
	        case ItemDamageType.DT_STUN:
		        return _block[2];
	        default:
		        break;
	    }

	    return 0;
    }

    /**
     * Gets the amount of armor.
     * @return The amount of armor.
     */
    internal int getArmor() =>
	    _armor;

    /**
     * Gets the dead object ID.
     * @return The dead object ID.
     */
    internal int getDieMCD() =>
	    _dieMCD;

    /**
     * Gets the alternative object ID.
     * @return The alternative object ID.
     */
    internal int getAltMCD() =>
	    _altMCD;

    /**
     * Sets the sprite index for a certain frame.
     * @param frameID Animation frame
     * @param value The sprite index in the surfaceset of the mapdataset.
     */
    internal void setSprite(int frameID, int value) =>
        _sprite[frameID] = value;

    /**
     * Sets the offset on the Y axis for drawing this object.
     * @param value The offset.
     */
    internal void setYOffset(int value) =>
        _yOffset = value;

    /**
     * Sets a special tile type and object type.
     * @param value Special tile type.
     * @param otype Object type.
     */
    internal void setSpecialType(int value, TilePart otype)
    {
        _specialType = (SpecialTileType)value;
        _objectType = otype;
    }

    /**
     * Sets the TU cost to move over the object.
     * @param walk The walking TU cost.
     * @param fly The flying TU cost.
     * @param slide The sliding TU cost.
     */
    internal void setTUCosts(int walk, int fly, int slide)
    {
        _TUWalk = walk;
        _TUFly = fly;
        _TUSlide = slide;
    }

    /**
     * Sets all kinds of flags.
     * @param isUfoDoor True if this is a ufo door.
     * @param stopLOS True if this stops line of sight.
     * @param isNoFloor True if this is a floor.
     * @param bigWall True if this is a bigWall.
     * @param isGravLift True if this is a grav lift.
     * @param isDoor True if this is a normal door.
     * @param blockFire True if this blocks fire.
     * @param blockSmoke True if this blocks smoke.
     * @param baseModule True if this is a base module item.
     */
    internal void setFlags(bool isUfoDoor, bool stopLOS, bool isNoFloor, int bigWall, bool isGravLift, bool isDoor, bool blockFire, bool blockSmoke, bool baseModule)
    {
        _isUfoDoor = isUfoDoor;
        _stopLOS = stopLOS;
        _isNoFloor = isNoFloor;
        _bigWall = bigWall;
        _isGravLift = isGravLift;
        _isDoor = isDoor;
        _blockFire = blockFire;
        _blockSmoke = blockSmoke;
        _baseModule = baseModule;
    }

    /**
     * Sets the Y offset for units/objects on this tile.
     * @param value The Y offset.
     */
    internal void setTerrainLevel(int value) =>
        _terrainLevel = value;

    /**
     * Sets the index to the footstep sound.
     * @param value The sound ID.
     */
    internal void setFootstepSound(int value) =>
        _footstepSound = value;

    /**
     * Sets the alternative object ID.
     * @param value The alternative object ID.
     */
    internal void setAltMCD(int value) =>
        _altMCD = value;

    /**
     * Sets the dead object ID.
     * @param value The dead object ID.
     */
    internal void setDieMCD(int value) =>
        _dieMCD = value;

    /**
     * Sets the amount of blockage for all types.
     * @param lightBlock The light blockage.
     * @param visionBlock The vision blockage.
     * @param HEBlock The high explosive blockage.
     * @param smokeBlock The smoke blockage.
     * @param fireBlock The fire blockage.
     * @param gasBlock The gas blockage.
     */
    internal void setBlockValue(int lightBlock, int visionBlock, int HEBlock, int smokeBlock, int fireBlock, int gasBlock)
    {
        _block[0] = lightBlock; // not used...
        _block[1] = visionBlock == 1 ? 255 : 0;
        _block[2] = HEBlock;
        _block[3] = smokeBlock == 1 ? 256 : 0;
        _block[4] = fireBlock;
        _block[5] = gasBlock;
    }

    /**
     * Sets the amount of light the object is emitting.
     * @param value The amount of light emitted.
     */
    internal void setLightSource(int value) =>
        _lightSource = value;

    /**
     * Sets the amount of armor.
     * @param value The amount of armor.
     */
    internal void setArmor(int value) =>
        _armor = value;

    /**
     * Sets the amount of flammable (how flammable this object is).
     * @param value The amount of flammable.
     */
    internal void setFlammable(int value) =>
        _flammable = value;

    /**
     * Sets the amount of fuel.
     * @param value The amount of fuel.
     */
    internal void setFuel(int value) =>
        _fuel = value;

    /**
     * Sets the type of explosive.
     * @param value The type of explosive.
     */
    internal void setExplosiveType(int value) =>
        _explosiveType = value;

    /**
     * Sets the amount of explosive.
     * @param value The amount of explosive.
     */
    internal void setExplosive(int value) =>
        _explosive = value;

    /**
     * Sets the SCANG.DAT index for minimap.
     * @param i The minimap index.
     */
    internal void setMiniMapIndex(ushort i) =>
        _miniMapIndex = i;

    /**
     * Sets the loft index for a certain layer.
     * @param loft The loft index.
     * @param layer The layer.
     */
    internal void setLoftID(int loft, int layer) =>
        _loftID[layer] = loft;

    /**
     * Sets the bigWall value.
     * @param bigWall The new bigWall value.
     */
    internal void setBigWall(int bigWall) =>
	    _bigWall = bigWall;

    /**
     * Sets the TUWalk value.
     * @param TUWalk The new TUWalk value.
     */
    internal void setTUWalk(int TUWalk) =>
	    _TUWalk = TUWalk;

    /**
     * Sets the TUFly value.
     * @param TUFly The new TUFly value.
     */
    internal void setTUFly(int TUFly) =>
	    _TUFly = TUFly;

    /**
     * Sets the TUSlide value.
     * @param TUSlide The new TUSlide value.
     */
    internal void setTUSlide(int TUSlide) =>
	    _TUSlide = TUSlide;

    /**
     * Gets the type of object.
     * @return Type of the part of the tile.
     */
    internal TilePart getObjectType() =>
	    _objectType;

    /**
     * Sets the amount of HE blockage.
     * @param HEBlock The high explosive blockage.
     */
    internal void setHEBlock(int HEBlock) =>
        _block[2] = HEBlock;

    /**
     * Sets the type of object.
     * @param type New type of the object.
     */
    internal void setObjectType(TilePart type) =>
        _objectType = type;

    /**
     * set the "no floor" flag.
     * @param isNoFloor set the flag to THIS.
     */
    internal void setNoFloor(bool isNoFloor) =>
        _isNoFloor = isNoFloor;

    /**
     * set the "stops LOS" flag.
     * @param stopLOS set the flag to THIS.
     */
    internal void setStopLOS(bool stopLOS)
    {
        _stopLOS = stopLOS;
        _block[1] = stopLOS ? 255 : 0;
    }

    /**
     * Gets info about special tile types.
     * @return The special tile type.
     */
    internal SpecialTileType getSpecialType() =>
	    _specialType;

    /**
     * Gets the amount of fuel.
     * @return The amount of fuel.
     */
    internal int getFuel() =>
	    _fuel;

    /**
     * Gets the amount of flammable (how flammable this object is).
     * @return The amount of flammable.
     */
    internal int getFlammable() =>
	    _flammable;

    /**
     * check if this is an xcom base object.
     * @return if it is a base object.
     */
    internal bool isBaseModule() =>
	    _baseModule;

    /**
     * Gets the dataset this object belongs to.
     * @return Pointer to MapDataSet.
     */
    internal MapDataSet getDataset() =>
	    _dataset;

    /**
     * Gets the amount of explosive.
     * @return The amount of explosive.
     */
    internal int getExplosive() =>
	    _explosive;

    /**
     * Gets the type of explosive.
     * @return The amount of explosive.
     */
    internal int getExplosiveType() =>
	    _explosiveType;
}
