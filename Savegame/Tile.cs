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

struct SerializationKey
{
    // how many bytes to store for each variable or each member of array of the same name
    internal byte index; // for indexing the actual tile array
    internal byte _mapDataSetID;
    internal byte _mapDataID;
    internal byte _smoke;
    internal byte _fire;
    internal byte boolFields;
    internal uint totalBytes; // per structure, including any data not mentioned here and accounting for all array members!
}

/**
 * Basic element of which a battle map is build.
 * @sa http://www.ufopaedia.org/index.php?title=MAPS
 */
internal class Tile
{
    protected const int LIGHTLAYERS = 3;
    protected int _smoke;
    protected int _fire;
    protected int _explosive;
    protected int _explosiveType;
    protected Position _pos;
    protected BattleUnit _unit;
    protected int _animationOffset;
    protected int _markerColor;
    protected int _visible;
    protected int _preview;
    protected int _TUMarker;
    protected int _overlaps;
    protected bool _danger;
    protected int _obstacle;
    protected MapData[] _objects = new MapData[4];
    protected int[] _mapDataID = new int[4];
    protected int[] _mapDataSetID = new int[4];
    protected int[] _currentFrame = new int[4];
    protected int[] _light = new int[LIGHTLAYERS], _lastLight = new int[LIGHTLAYERS];
    protected bool[] _discovered = new bool[3];
    protected List<BattleItem> _inventory;
    protected List<Particle> _particles;
    internal static SerializationKey serializationKey;

    /**
     * constructor
     * @param pos Position.
     */
    Tile(Position pos)
    {
        _smoke = 0;
        _fire = 0;
        _explosive = 0;
        _explosiveType = 0;
        _pos = pos;
        _unit = null;
        _animationOffset = 0;
        _markerColor = 0;
        _visible = 0;
        _preview = -1;
        _TUMarker = -1;
        _overlaps = 0;
        _danger = false;
        _obstacle = 0;

        for (int i = 0; i < 4; ++i)
        {
            _objects[i] = null;
            _mapDataID[i] = -1;
            _mapDataSetID[i] = -1;
            _currentFrame[i] = 0;
        }
        for (int layer = 0; layer < LIGHTLAYERS; layer++)
        {
            _light[layer] = 0;
            _lastLight[layer] = -1;
        }
        for (int i = 0; i < 3; ++i)
        {
            _discovered[i] = false;
        }
    }

    /**
     * destructor
     */
    ~Tile()
    {
        _inventory.Clear();
        _particles.Clear();
    }

    /**
     * retrieve the direction stored by the pathfinding.
     * @return preview
     */
    internal int getPreview() =>
	    _preview;

    /**
     * set the direction used for path previewing.
     * @param dir
     */
    internal void setPreview(int dir) =>
        _preview = dir;

    /**
     * set the number to be displayed for pathfinding preview.
     * @param tu
     */
    internal void setTUMarker(int tu) =>
        _TUMarker = tu;

    /**
     * Set the marker color on this tile.
     * @param color
     */
    internal void setMarkerColor(int color) =>
        _markerColor = color;

    /**
     * Get the inventory on this tile.
     * @return pointer to a vector of battleitems.
     */
    internal List<BattleItem> getInventory() =>
        _inventory;

    /**
     * If an object stand on this tile, this returns how high the unit is it standing.
     * @return the level in pixels (so negative values are higher)
     */
    internal int getTerrainLevel()
    {
	    int level = 0;

	    if (_objects[(int)TilePart.O_FLOOR] != null)
		    level = _objects[(int)TilePart.O_FLOOR].getTerrainLevel();
	    // whichever's higher, but not the sum.
	    if (_objects[(int)TilePart.O_OBJECT] != null)
		    level = Math.Min(_objects[(int)TilePart.O_OBJECT].getTerrainLevel(), level);

        return level;
    }

    /**
	 * Get the MapData pointer of a part of the tile.
	 * @param part TilePart whose data is needed.
	 * @return pointer to mapdata
	 */
    internal MapData getMapData(TilePart part) =>
		_objects[(int)part];

    /**
     * get the MapData references of part 0 to 3.
     * @param mapDataID
     * @param mapDataSetID
     * @param part is part of the tile to get data from
     * @return the object ID
     */
    internal void getMapData(out int mapDataID, out int mapDataSetID, TilePart part)
    {
	    mapDataID = _mapDataID[(int)part];
	    mapDataSetID = _mapDataSetID[(int)part];
    }

    /**
     * Set the MapData references of part 0 to 3.
     * @param dat pointer to the data object
     * @param mapDataID
     * @param mapDataSetID
     * @param part Part of the tile to set data of
     */
    internal void setMapData(MapData dat, int mapDataID, int mapDataSetID, TilePart part)
    {
        _objects[(int)part] = dat;
        _mapDataID[(int)part] = mapDataID;
        _mapDataSetID[(int)part] = mapDataSetID;
    }

    /**
	 * Gets the tile's position.
	 * @return position
	 */
    internal Position getPosition() =>
		_pos;

    /**
	 * Get the (alive) unit on this tile.
	 * @return BattleUnit.
	 */
    internal BattleUnit getUnit() =>
		_unit;

    /**
     * Get the amount of turns this tile is on fire. 0 = no fire.
     * @return fire : amount of turns this tile is on fire.
     */
    internal int getFire() =>
	    _fire;

    /**
     * Get the amount of turns this tile is smoking. 0 = no smoke.
     * @return smoke : amount of turns this tile is smoking.
     */
    internal int getSmoke() =>
	    _smoke;

    /**
     * Whether this tile has a floor or not. If no object defined as floor, it has no floor.
     * @param tileBelow
     * @return bool
     */
    internal bool hasNoFloor(Tile tileBelow)
    {
	    if (tileBelow != null && tileBelow.getTerrainLevel() == -24)
		    return false;
        if (_objects[(int)TilePart.O_FLOOR] != null)
            return _objects[(int)TilePart.O_FLOOR].isNoFloor();
        else
            return true;
    }

    /**
	 * Check if the ufo door is open or opening. Used for visibility/light blocking checks.
	 * This function assumes that there never are 2 doors on 1 tile or a door and another wall on 1 tile.
	 * @param part Tile part to look for door
	 * @return bool
	 */
    internal bool isUfoDoorOpen(TilePart tp)
	{
        int part = (int)tp;
		return (_objects[part] != null && _objects[part].isUFODoor() && _currentFrame[part] != 0);
	}

    /**
     * Gets the TU cost to walk over a certain part of the tile.
     * @param part The part number.
     * @param movementType The movement type.
     * @return TU cost.
     */
    internal int getTUCost(int part, MovementType movementType)
    {
	    if (_objects[part] != null)
	    {
		    if (_objects[part].isUFODoor() && _currentFrame[part] > 1)
			    return 0;
		    if (part == (int)TilePart.O_OBJECT && _objects[part].getBigWall() >= 4)
			    return 0;
		    return _objects[part].getTUCost(movementType);
	    }
	    else
		    return 0;
    }

    /**
     * Set the tile visible flag.
     * @param visibility
     */
    internal void setVisible(int visibility) =>
        _visible += visibility;

    /**
     * Add an item on the tile.
     * @param item
     * @param ground
     */
    internal void addItem(BattleItem item, RuleInventory ground)
    {
        item.setSlot(ground);
        _inventory.Add(item);
        item.setTile(this);
    }

    /**
     * Add the light amount on the tile. Only add light if the current light is lower.
     * @param light Amount of light to add.
     * @param layer Light is separated in 3 layers: Ambient, Static and Dynamic.
     */
    internal void addLight(int light, int layer)
    {
        if (_light[layer] < light)
            _light[layer] = light;
    }

    /**
     * Reset the light amount on the tile. This is done before a light level recalculation.
     * @param layer Light is separated in 3 layers: Ambient, Static and Dynamic.
     */
    internal void resetLight(int layer)
    {
        _light[layer] = 0;
        _lastLight[layer] = _light[layer];
    }

    /**
     * Set a unit on this tile.
     * @param unit
     * @param tileBelow
     */
    internal void setUnit(BattleUnit unit, Tile tileBelow = null)
    {
        if (unit != null)
        {
            unit.setTile(this, tileBelow);
        }
        _unit = unit;
    }

    /**
     * Sets the tile's cache flag. - TODO: set this for each object separately?
     * @param flag true/false
     * @param part 0-2 westwall/northwall/content+floor
     */
    internal void setDiscovered(bool flag, int part)
    {
        if (_discovered[part] != flag)
        {
            _discovered[part] = flag;
            if (part == 2 && flag == true)
            {
                _discovered[0] = true;
                _discovered[1] = true;
            }
            // if light on tile changes, units and objects on it change light too
            if (_unit != null)
            {
                _unit.setCache(null);
            }
        }
    }

    /**
     * Gets the tile's shade amount 0-15. It returns the brightest of all light layers.
     * Shade level is the inverse of light level. So a maximum amount of light (15) returns shade level 0.
     * @return shade
     */
    internal int getShade()
    {
	    int light = 0;

	    for (int layer = 0; layer < LIGHTLAYERS; layer++)
	    {
		    if (_light[layer] > light)
			    light = _light[layer];
	    }

        return Math.Max(0, 15 - light);
    }

    /**
     * sets the flag of an obstacle for single part.
     */
    internal void setObstacle(int part) =>
        _obstacle |= (1 << part);

    /**
     * Whether this tile has a big wall.
     * @return bool
     */
    internal bool isBigWall()
    {
	    if (_objects[(int)TilePart.O_OBJECT] != null)
		    return (_objects[(int)TilePart.O_OBJECT].getBigWall() != 0);
	    else
		    return false;
    }

    /**
     * Gets whether this tile has no objects. Note that we can have a unit or smoke on this tile.
     * @return bool True if there is nothing but air on this tile.
     */
    internal bool isVoid() =>
	    _objects[0] == null && _objects[1] == null && _objects[2] == null && _objects[3] == null && _smoke == 0 && !_inventory.Any();

    /**
     * Saves the tile to binary.
     * @param buffer pointer to buffer.
     */
    internal void saveBinary(ref Span<byte> buffer)
    {
	    serializeInt(ref buffer, serializationKey._mapDataID, _mapDataID[0]);
	    serializeInt(ref buffer, serializationKey._mapDataID, _mapDataID[1]);
	    serializeInt(ref buffer, serializationKey._mapDataID, _mapDataID[2]);
	    serializeInt(ref buffer, serializationKey._mapDataID, _mapDataID[3]);
	    serializeInt(ref buffer, serializationKey._mapDataSetID, _mapDataSetID[0]);
	    serializeInt(ref buffer, serializationKey._mapDataSetID, _mapDataSetID[1]);
	    serializeInt(ref buffer, serializationKey._mapDataSetID, _mapDataSetID[2]);
	    serializeInt(ref buffer, serializationKey._mapDataSetID, _mapDataSetID[3]);

	    serializeInt(ref buffer, serializationKey._smoke, _smoke);
	    serializeInt(ref buffer, serializationKey._fire, _fire);

        byte boolFields = (byte)((_discovered[0] ? 1 : 0) + (_discovered[1] ? 2 : 0) + (_discovered[2] ? 4 : 0));
	    boolFields |= (byte)(isUfoDoorOpen(TilePart.O_WESTWALL) ? 8 : 0); // west
	    boolFields |= (byte)(isUfoDoorOpen(TilePart.O_NORTHWALL) ? 0x10 : 0); // north?
        serializeInt(ref buffer, serializationKey.boolFields, boolFields);
    }
}
