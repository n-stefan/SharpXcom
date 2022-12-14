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

    /**
     * Creates a new Map Data Object.
     * @param dataset The dataset this object belongs to.
     */
    MapData(MapDataSet dataset)
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
}
