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
    internal Tile(Position pos)
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
	    boolFields = (byte)(boolFields | (isUfoDoorOpen(TilePart.O_WESTWALL) ? 8 : 0)); // west
	    boolFields = (byte)(boolFields | (isUfoDoorOpen(TilePart.O_NORTHWALL) ? 0x10 : 0)); // north?
        serializeInt(ref buffer, serializationKey.boolFields, boolFields);
    }

    /**
     * Get explosive on this tile.
     * @return explosive
     */
    internal int getExplosive() =>
	    _explosive;

    /**
     * Set a "virtual" explosive on this tile. We mark a tile this way to detonate it later.
     * We do it this way, because the same tile can be visited multiple times by an "explosion ray".
     * The explosive power on the tile is some kind of moving MAXIMUM of the explosive rays that passes it.
     * @param power Power of the damage.
     * @param damageType the damage type of the explosion (not the same as item damage types)
     * @param force Force damage.
     */
    internal void setExplosive(int power, int damageType, bool force = false)
    {
        if (force || _explosive < power)
        {
            _explosive = power;
            _explosiveType = damageType;
        }
    }

    /**
     * Set the amount of turns this tile is on fire. 0 = no fire.
     * @param fire : amount of turns this tile is on fire.
     */
    internal void setFire(int fire)
    {
        _fire = fire;
        _animationOffset = RNG.generate(0, 3);
    }

    /**
     * Set the amount of turns this tile is smoking. 0 = no smoke.
     * @param smoke : amount of turns this tile is smoking.
     */
    internal void setSmoke(int smoke)
    {
        _smoke = smoke;
        _animationOffset = RNG.generate(0, 3);
    }

    /*
     * Fuel of a tile is the highest fuel of it's objects.
     * @return how long to burn.
     */
    internal int getFuel()
    {
	    int fuel = 0;

	    for (int i=0; i<4; ++i)
		    if (_objects[i] != null && (_objects[i].getFuel() > fuel))
			    fuel = _objects[i].getFuel();

	    return fuel;
    }

    /*
     * Flammability of a tile is the lowest flammability of it's objects.
     * @return Flammability : the lower the value, the higher the chance the tile/object catches fire.
     */
    internal int getFlammability()
    {
	    int flam = 255;

	    for (int i=0; i<4; ++i)
		    if (_objects[i] != null && (_objects[i].getFlammable() < flam))
			    flam = _objects[i].getFlammable();

	    return flam;
    }

    /*
     * Flammability of the particular part of the tile
     * @return Flammability : the lower the value, the higher the chance the tile/object catches fire.
     */
    internal int getFlammability(TilePart part) =>
	    _objects[(int)part].getFlammable();

    /*
     * Fuel of particular part of the tile
     * @return how long to burn.
     */
    internal int getFuel(TilePart part) =>
	    _objects[(int)part].getFuel();

    /**
     * Destroy a part on this tile. We first remove the old object, then replace it with the destroyed one.
     * This is because the object type of the old and new one are not necessarily the same.
     * If the destroyed part is an explosive, set the tile's explosive value, which will trigger a chained explosion.
     * @param part the part to destroy.
     * @param type the objective type for this mission we are checking against.
     * @return bool Return true objective was destroyed.
     */
    internal bool destroy(TilePart part, SpecialTileType type)
    {
        bool _objective = false;
        var index = (int)part;
        if (_objects[index] != null)
        {
            if (_objects[index].isGravLift())
                return false;
            _objective = _objects[index].getSpecialType() == type;
            MapData originalPart = _objects[index];
            int originalMapDataSetID = _mapDataSetID[index];
            setMapData(null, -1, -1, part);
            if (originalPart.getDieMCD() != 0)
            {
                MapData dead = originalPart.getDataset().getObject((uint)originalPart.getDieMCD());
                setMapData(dead, originalPart.getDieMCD(), originalMapDataSetID, dead.getObjectType());
            }
            if (originalPart.getExplosive() != 0)
            {
                setExplosive(originalPart.getExplosive(), originalPart.getExplosiveType());
            }
        }
        /* check if the floor on the lowest level is gone */
        if (part == TilePart.O_FLOOR && getPosition().z == 0 && _objects[(int)TilePart.O_FLOOR] == null)
        {
            /* replace with scorched earth */
            setMapData(MapDataSet.getScorchedEarthTile(), 1, 0, TilePart.O_FLOOR);
        }
        return _objective;
    }

    internal int closeUfoDoor()
    {
        int retval = 0;

        for (var part = TilePart.O_FLOOR; part <= TilePart.O_NORTHWALL; ++part)
        {
            if (isUfoDoorOpen((TilePart)part))
            {
                _currentFrame[(int)part] = 0;
                retval = 1;
            }
        }

        return retval;
    }

    /**
     * Remove an item from the tile.
     * @param item
     */
    internal void removeItem(BattleItem item)
    {
        foreach (var i in _inventory)
        {
            if (i == item)
            {
                _inventory.Remove(i);
                break;
            }
        }
        item.setTile(null);
    }

    /**
     * get the overlap value of this tile.
     * @return overlap
     */
    internal int getOverlaps() =>
	    _overlaps;

    /**
     * Set the amount of turns this tile is smoking. 0 = no smoke.
     * @param smoke : amount of turns this tile is smoking.
     */
    internal void addSmoke(int smoke)
    {
	    if (_fire == 0)
	    {
		    if (_overlaps == 0)
		    {
			    _smoke = Math.Clamp(_smoke + smoke, 1, 15);
		    }
		    else
		    {
			    _smoke += smoke;
		    }
		    _animationOffset = RNG.generate(0,3);
		    addOverlap();
	    }
    }

    /**
     * set the danger flag on this tile.
     */
    internal void setDangerous(bool danger) =>
	    _danger = danger;

    /*
     * Ignite starts fire on a tile, it will burn <fuel> rounds. Fuel of a tile is the highest fuel of its objects.
     * NOT the sum of the fuel of the objects!
     */
    internal void ignite(int power)
    {
	    if (getFlammability() != 255)
	    {
		    power = power - (getFlammability() / 10) + 15;
		    if (power < 0)
		    {
			    power = 0;
		    }
		    if (RNG.percent(power) && getFuel() != 0)
		    {
			    if (_fire == 0)
			    {
				    _smoke = 15 - Math.Clamp(getFlammability() / 10, 1, 12);
				    _overlaps = 1;
				    _fire = getFuel() + 1;
				    _animationOffset = RNG.generate(0,3);
			    }
		    }
	    }
    }

    /**
     * New turn preparations.
     * average out any smoke added by the number of overlaps.
     * apply fire/smoke damage to units as applicable.
     */
    internal void prepareNewTurn(bool smokeDamage)
    {
	    // we've received new smoke in this turn, but we're not on fire, average out the smoke.
	    if ( _overlaps != 0 && _smoke != 0 && _fire == 0)
	    {
		    _smoke = Math.Clamp((_smoke / _overlaps) - 1, 0, 15);
	    }
	    // if we still have smoke/fire
	    if (_smoke != 0)
	    {
		    if (_unit != null && !_unit.isOut())
		    {
			    if (_fire != 0)
			    {
				    // this is how we avoid hitting the same unit multiple times.
				    if ((_unit.getArmor().getSize() == 1 || !_unit.tookFireDamage())
					    //and avoid setting fire elementals on fire
					    && _unit.getSpecialAbility() != (int)SpecialAbility.SPECAB_BURNFLOOR && _unit.getSpecialAbility() != (int)SpecialAbility.SPECAB_BURN_AND_EXPLODE)
				    {
					    _unit.toggleFireDamage();
					    // _smoke becomes our damage value
					    _unit.damage(new Position(0, 0, 0), _smoke, ItemDamageType.DT_IN, true);
					    // try to set the unit on fire.
					    if (RNG.percent((int)(40 * _unit.getArmor().getDamageModifier(ItemDamageType.DT_IN))))
					    {
						    int burnTime = RNG.generate(0, (int)(5.0f * _unit.getArmor().getDamageModifier(ItemDamageType.DT_IN)));
						    if (_unit.getFire() < burnTime)
						    {
							    _unit.setFire(burnTime);
						    }
					    }
				    }
			    }
			    // no fire: must be smoke
			    else
			    {
				    if (smokeDamage)
				    {
					    // try to knock this guy out.
					    if (_unit.getArmor().getDamageModifier(ItemDamageType.DT_SMOKE) > 0.0 && _unit.getArmor().getSize() == 1)
					    {
						    _unit.damage(new Position(0,0,0), (_smoke / 4) + 1, ItemDamageType.DT_SMOKE, true);
					    }
				    }
			    }
		    }
	    }
	    _overlaps = 0;
    }

    /**
     * increment the overlap value on this tile.
     */
    internal void addOverlap() =>
	    ++_overlaps;

    /**
     * resets obstacle flag for all parts of the tile.
     */
    internal void resetObstacle() =>
	    _obstacle = 0;

    /**
     * Open a door on this tile.
     * @param part
     * @param unit
     * @param reserve
     * @return a value: 0(normal door), 1(ufo door) or -1 if no door opened or 3 if ufo door(=animated) is still opening 4 if not enough TUs
     */
    internal int openDoor(TilePart part, BattleUnit unit = null, BattleActionType reserve = BattleActionType.BA_NONE)
    {
	    if (_objects[(int)part] == null) return -1;

	    if (_objects[(int)part].isDoor())
	    {
		    if (unit != null && unit.getArmor().getSize() > 1) // don't allow double-wide units to open swinging doors due to engine limitations
			    return -1;
		    if (unit != null && unit.getTimeUnits() < _objects[(int)part].getTUCost(unit.getMovementType()) + unit.getActionTUs(reserve, unit.getMainHandWeapon(false)))
			    return 4;
		    if (_unit != null && _unit != unit && _unit.getPosition() != getPosition())
			    return -1;
		    setMapData(_objects[(int)part].getDataset().getObject((uint)_objects[(int)part].getAltMCD()), _objects[(int)part].getAltMCD(), _mapDataSetID[(int)part],
				       _objects[(int)part].getDataset().getObject((uint)_objects[(int)part].getAltMCD()).getObjectType());
		    setMapData(null, -1, -1, part);
		    return 0;
	    }
	    if (_objects[(int)part].isUFODoor() && _currentFrame[(int)part] == 0) // ufo door part 0 - door is closed
	    {
		    if (unit != null && unit.getTimeUnits() < _objects[(int)part].getTUCost(unit.getMovementType()) + unit.getActionTUs(reserve, unit.getMainHandWeapon(false)))
			    return 4;
		    _currentFrame[(int)part] = 1; // start opening door
		    return 1;
	    }
	    if (_objects[(int)part].isUFODoor() && _currentFrame[(int)part] != 7) // ufo door != part 7 - door is still opening
	    {
		    return 3;
	    }
	    return -1;
    }

    /**
     * get the danger flag on this tile.
     * @return the danger flag for this tile.
     */
    internal bool getDangerous() =>
	    _danger;

    /**
     * damage terrain - check against armor
     * @param part Part to check.
     * @param power Power of the damage.
     * @param type the objective type for this mission we are checking against.
     * @return bool Return true objective was destroyed
     */
    internal bool damage(TilePart part, int power, SpecialTileType type)
    {
	    bool objective = false;
	    if (power >= _objects[(int)part].getArmor())
		    objective = destroy(part, type);
	    return objective;
    }

    /**
     * Get explosive on this tile.
     * @return explosive
     */
    internal int getExplosiveType() =>
	    _explosiveType;

    /**
     * Get the tile visible flag.
     * @return visibility
     */
    internal int getVisible() =>
	    _visible;

    /**
     * adds a particle to this tile's internal storage buffer.
     * @param particle the particle to add.
     */
    internal void addParticle(Particle particle) =>
	    _particles.Add(particle);

    /**
     * Gets the tile's footstep sound.
     * @param tileBelow
     * @return sound ID
     */
    internal int getFootstepSound(Tile tileBelow)
    {
	    int sound = -1;

	    if (_objects[(int)TilePart.O_FLOOR] != null)
		    sound = _objects[(int)TilePart.O_FLOOR].getFootstepSound();
	    if (_objects[(int)TilePart.O_OBJECT] != null && _objects[(int)TilePart.O_OBJECT].getBigWall() <= 1 && _objects[(int)TilePart.O_OBJECT].getFootstepSound() > -1)
		    sound = _objects[(int)TilePart.O_OBJECT].getFootstepSound();
	    if (_objects[(int)TilePart.O_FLOOR] == null && _objects[(int)TilePart.O_OBJECT] == null && tileBelow != null && tileBelow.getTerrainLevel() == -24)
		    sound = tileBelow.getMapData(TilePart.O_OBJECT).getFootstepSound();

	    return sound;
    }

    /**
     * Get the black fog of war state of this tile.
     * @param part 0-2 westwall/northwall/content+floor
     * @return bool True = discovered the tile.
     */
    internal bool isDiscovered(int part) =>
	    _discovered[part];

    /**
     * Animate the tile. This means to advance the current frame for every object.
     * Ufo doors are a bit special, they animated only when triggered.
     * When ufo doors are on frame 0(closed) or frame 7(open) they are not animated further.
     */
    internal void animate()
    {
	    int newframe;
	    for (int i=0; i < 4; ++i)
	    {
		    if (_objects[i] != null)
		    {
			    if (_objects[i].isUFODoor() && (_currentFrame[i] == 0 || _currentFrame[i] == 7)) // ufo door is static
			    {
				    continue;
			    }
			    newframe = _currentFrame[i] + 1;
			    if (_objects[i].isUFODoor() && _objects[i].getSpecialType() == SpecialTileType.START_POINT && newframe == 3)
			    {
				    newframe = 7;
			    }
			    if (newframe == 8)
			    {
				    newframe = 0;
			    }
			    _currentFrame[i] = newframe;
		    }
	    }
	    for (var i = 0; i < _particles.Count;)
	    {
		    if (!_particles[i].animate())
		    {
			    _particles.RemoveAt(i);
		    }
		    else
		    {
			    ++i;
		    }
	    }
    }

    /**
     * Get the marker color on this tile.
     * @return color
     */
    internal int getMarkerColor() =>
	    _markerColor;

    /**
     * Get the sprite of a certain part of the tile.
     * @param part
     * @return Pointer to the sprite.
     */
    internal Surface getSprite(int part)
    {
	    if (_objects[part] == null)
		    return null;

	    return _objects[part].getDataset().getSurfaceset().getFrame(_objects[part].getSprite(_currentFrame[part]));
    }

	/// gets single obstacle flag.
	internal bool getObstacle(int part) =>
		(_obstacle & (1 << part)) != 0;

    /**
     * Get the number of frames the fire or smoke animation is off-sync.
     * To void fire and smoke animations of different tiles moving nice in sync - it looks fake.
     * @return offset
     */
    internal int getAnimationOffset() =>
	    _animationOffset;

    /**
     * get the number to be displayed for pathfinding preview.
     * @return marker
     */
    internal int getTUMarker() =>
	    _TUMarker;

    /**
     * gets a pointer to this tile's particle array.
     * @return a pointer to the internal array of particles.
     */
    internal List<Particle> getParticleCloud() =>
	    _particles;

    /**
     * Get the topmost item sprite to draw on the battlescape.
     * @return item sprite ID in floorob, or -1 when no item
     */
    internal int getTopItemSprite()
    {
	    int biggestWeight = -1;
	    int biggestItem = -1;
	    foreach (var i in _inventory)
	    {
		    if (i.getRules().getWeight() > biggestWeight)
		    {
			    biggestWeight = i.getRules().getWeight();
			    biggestItem = i.getRules().getFloorSprite();
		    }
	    }
	    return biggestItem;
    }

	/// does the tile have obstacle flag set for at least one part?
	internal bool isObstacle() =>
		_obstacle != 0;

    /**
     * Load the tile from a YAML node.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    //_position = node["position"].as<Position>(_position);
	    for (int i = 0; i < 4; i++)
	    {
		    _mapDataID[i] = int.Parse(node["mapDataID"][i].ToString());
		    _mapDataSetID[i] = int.Parse(node["mapDataSetID"][i].ToString());
	    }
	    _fire = int.Parse(node["fire"].ToString());
	    _smoke = int.Parse(node["smoke"].ToString());
	    if (node["discovered"] != null)
	    {
		    for (int i = 0; i < 3; i++)
		    {
			    _discovered[i] = bool.Parse(node["discovered"][i].ToString());
		    }
	    }
	    if (node["openDoorWest"] != null)
	    {
		    _currentFrame[1] = 7;
	    }
	    if (node["openDoorNorth"] != null)
	    {
		    _currentFrame[2] = 7;
	    }
	    if (_fire != 0 || _smoke != 0)
	    {
		    _animationOffset = new Random().Next() % 4;
	    }
    }

    /**
     * Load the tile from binary.
     * @param buffer Pointer to buffer.
     * @param serKey Serialization key.
     */
    internal void loadBinary(Span<byte> buffer, SerializationKey serKey)
    {
	    _mapDataID[0] = unserializeInt(ref buffer, serKey._mapDataID);
	    _mapDataID[1] = unserializeInt(ref buffer, serKey._mapDataID);
	    _mapDataID[2] = unserializeInt(ref buffer, serKey._mapDataID);
	    _mapDataID[3] = unserializeInt(ref buffer, serKey._mapDataID);
	    _mapDataSetID[0] = unserializeInt(ref buffer, serKey._mapDataSetID);
	    _mapDataSetID[1] = unserializeInt(ref buffer, serKey._mapDataSetID);
	    _mapDataSetID[2] = unserializeInt(ref buffer, serKey._mapDataSetID);
	    _mapDataSetID[3] = unserializeInt(ref buffer, serKey._mapDataSetID);

	    _smoke = unserializeInt(ref buffer, serKey._smoke);
	    _fire = unserializeInt(ref buffer, serKey._fire);

	    byte boolFields = (byte)unserializeInt(ref buffer, serKey.boolFields);
	    _discovered[0] = (boolFields & 1) != 0 ? true : false;
	    _discovered[1] = (boolFields & 2) != 0 ? true : false;
	    _discovered[2] = (boolFields & 4) != 0 ? true : false;
	    _currentFrame[1] = (boolFields & 8) != 0 ? 7 : 0;
	    _currentFrame[2] = (boolFields & 0x10) != 0 ? 7 : 0;
	    if (_fire != 0 || _smoke != 0)
	    {
		    _animationOffset = new Random().Next() % 4;
	    }
    }
}
