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

/**
 * The battlescape data that gets written to disk when the game is saved.
 * A saved game holds all the variable info in a game like mapdata,
 * soldiers, items, etc.
 */
internal class SavedBattleGame
{
    BattlescapeState _battleState;
    int _mapsize_x, _mapsize_y, _mapsize_z;
    BattleUnit _selectedUnit, _lastSelectedUnit;
    Pathfinding _pathfinding;
    TileEngine _tileEngine;
    int _globalShade;
    UnitFaction _side;
    int _turn;
    bool _debugMode;
    bool _aborted;
    int _itemId;
    int _objectiveType, _objectivesDestroyed, _objectivesNeeded;
    bool _unitsFalling, _cheating;
    BattleActionType _tuReserved;
    bool _kneelReserved;
    int _depth, _ambience;
    double _ambientVolume;
    int _turnLimit, _cheatTurn;
    ChronoTrigger _chronoTrigger;
    bool _beforeGame;
    List<Position> _tileSearch, _storageSpace;
    Tile[] _tiles;
    List<MapDataSet> _mapDataSets;
    List<Node> _nodes;
    List<BattleUnit> _units;
    List<BattleItem> _items, _deleted;
    List<BattleItem> _recoverGuaranteed, _recoverConditional;
    List<BattleUnit> _fallingUnits;
    string _missionType;
    List<List<KeyValuePair<int, int>>> _baseModules;
    string _music;

    /**
     * Initializes a brand new battlescape saved game.
     */
    internal SavedBattleGame()
    {
        _battleState = null;
        _mapsize_x = 0;
        _mapsize_y = 0;
        _mapsize_z = 0;
        _selectedUnit = null;
        _lastSelectedUnit = null;
        _pathfinding = null;
        _tileEngine = null;
        _globalShade = 0;
        _side = UnitFaction.FACTION_PLAYER;
        _turn = 1;
        _debugMode = false;
        _aborted = false;
        _itemId = 0;
        _objectiveType = -1;
        _objectivesDestroyed = 0;
        _objectivesNeeded = 0;
        _unitsFalling = false;
        _cheating = false;
        _tuReserved = BattleActionType.BA_NONE;
        _kneelReserved = false;
        _depth = 0;
        _ambience = -1;
        _ambientVolume = 0.5;
        _turnLimit = 0;
        _cheatTurn = 20;
        _chronoTrigger = ChronoTrigger.FORCE_LOSE;
        _beforeGame = true;

        for (int i = 0; i < 121; ++i)
        {
            _tileSearch[i] = new Position
            {
                x = ((i % 11) - 5),
                y = ((i / 11) - 5)
            };
        }
    }

    /**
     * Deletes the game content from memory.
     */
    ~SavedBattleGame()
    {
        _tiles = null;

        foreach (var mapDataSet in _mapDataSets)
        {
            mapDataSet.unloadData();
        }

        _nodes.Clear();
        _units.Clear();
        _items.Clear();
        _recoverGuaranteed.Clear();
        _recoverConditional.Clear();
        _deleted.Clear();

        _pathfinding = null;
        _tileEngine = null;
    }

    /**
     * Gets the map size in tiles.
     * @return The map size.
     */
    internal int getMapSizeXYZ() =>
	    _mapsize_x * _mapsize_y * _mapsize_z;

    /**
     * Converts a tile index to coordinates.
     * @param index The (unique) tileindex.
     * @param x Pointer to the X coordinate.
     * @param y Pointer to the Y coordinate.
     * @param z Pointer to the Z coordinate.
     */
    internal void getTileCoords(int index, out int x, out int y, out int z)
    {
	    z = index / (_mapsize_y * _mapsize_x);
	    y = (index % (_mapsize_y * _mapsize_x)) / _mapsize_x;
	    x = (index % (_mapsize_y * _mapsize_x)) % _mapsize_x;
    }

    /**
     * Gets the ambient sound effect volume.
     * @return the ambient sound volume.
     */
    internal double getAmbientVolume() =>
	    _ambientVolume;

    /**
     * check the depth of the battlescape.
     * @return depth.
     */
    internal int getDepth() =>
	    _depth;

    /**
     * Gets the pathfinding object.
     * @return Pointer to the pathfinding object.
     */
    internal Pathfinding getPathfinding() =>
	    _pathfinding;

    /**
     * Gets the map width.
     * @return The map width (Size X) in tiles.
     */
    internal int getMapSizeX() =>
	    _mapsize_x;

    /**
     * Gets the map length.
     * @return The map length (Size Y) in tiles.
     */
    internal int getMapSizeY() =>
	    _mapsize_y;

    /**
     * Gets the map height.
     * @return The map height (Size Z) in layers.
     */
    internal int getMapSizeZ() =>
	    _mapsize_z;

    /**
     * Gets the currently selected unit
     * @return Pointer to BattleUnit.
     */
    internal BattleUnit getSelectedUnit() =>
	    _selectedUnit;

    /**
     * Gets the BattlescapeState.
     * @return Pointer to the BattlescapeState.
     */
    internal BattlescapeGame getBattleGame() =>
        _battleState.getBattleGame();

    /**
	 * Converts coordinates into a unique index.
	 * getTile() calls this every time, so should be inlined along with it.
	 * @param pos The position to convert.
	 * @return A unique index.
	 */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int getTileIndex(Position pos) =>
        pos.z * _mapsize_y * _mapsize_x + pos.y * _mapsize_x + pos.x;

    /**
	 * Gets the Tile at a given position on the map.
	 * This method is called over 50mil+ times per turn so it seems useful
	 * to inline it.
	 * @param pos Map position.
	 * @return Pointer to the tile at that position.
	 */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Tile getTile(Position pos)
	{
        if (pos.x < 0 || pos.y < 0 || pos.z < 0
            || pos.x >= _mapsize_x || pos.y >= _mapsize_y || pos.z >= _mapsize_z)
            return null;

		return _tiles[getTileIndex(pos)];
	}

    /**
     * Gets the TU reserved type.
     * @return A battleactiontype.
     */
    internal BattleActionType getTUReserved() =>
	    _tuReserved;

    /**
     * Sets the TU reserved type.
     * @param reserved A battleactiontype.
     */
    internal void setTUReserved(BattleActionType reserved) =>
        _tuReserved = reserved;

    /**
     * Gets the side currently playing.
     * @return The unit faction currently playing.
     */
    internal UnitFaction getSide() =>
	    _side;

    /**
     * Gets the kneel reservation setting.
     * @return Should we reserve an extra 4 TUs to kneel?
     */
    internal bool getKneelReserved() =>
	    _kneelReserved;

    /**
     * Gets the list of units.
     * @return Pointer to the list of units.
     */
    internal List<BattleUnit> getUnits() =>
        _units;

    /**
     * Gets the morale modifier for
     * - either XCom based on the highest ranked, living XCom unit,
     * - or the unit passed to this function.
     * @param unit Unit.
     * @return The morale modifier.
     */
    internal int getMoraleModifier(BattleUnit unit = null)
    {
        int result = 100;

        if (unit == null)
        {
            BattleUnit leader = getHighestRankedXCom();
            if (leader != null)
            {
                switch (leader.getRankInt())
                {
                    case 5:
                        result += 25;
                        goto case 4;
                    case 4:
                        result += 10;
                        goto case 3;
                    case 3:
                        result += 5;
                        goto case 2;
                    case 2:
                        result += 10;
                        goto default;
                    default:
                        break;
                }
            }
        }
        else if (unit.getFaction() == UnitFaction.FACTION_PLAYER)
        {
            switch (unit.getRankInt())
            {
                case 5:
                    result += 25;
                    goto case 4;
                case 4:
                    result += 20;
                    goto case 3;
                case 3:
                    result += 10;
                    goto case 2;
                case 2:
                    result += 20;
                    goto default;
                default:
                    break;
            }
        }
        return result;
    }

    /**
     * Gets the highest ranked, living XCom unit.
     * @return The highest ranked, living XCom unit.
     */
    BattleUnit getHighestRankedXCom()
    {
        BattleUnit highest = null;
        foreach (var unit in _units)
        {
            if (unit.getOriginalFaction() == UnitFaction.FACTION_PLAYER && !unit.isOut())
            {
                if (highest == null || unit.getRankInt() > highest.getRankInt())
                {
                    highest = unit;
                }
            }
        }
        return highest;
    }

    /**
     * Gets the current turn number.
     * @return The current turn.
     */
    internal int getTurn() =>
	    _turn;

    internal bool isBeforeGame() =>
	    _beforeGame;

    /**
     * Gets the list of nodes.
     * @return Pointer to the list of nodes.
     */
    internal List<Node> getNodes() =>
        _nodes;

    /**
     * Gets the terrain modifier object.
     * @return Pointer to the terrain modifier object.
     */
    internal TileEngine getTileEngine() =>
	    _tileEngine;

    /**
     * Gets the list of items.
     * @return Pointer to the list of items.
     */
    internal List<BattleItem> getItems() =>
        _items;

    /**
     * Gets the array of tiles.
     * @return A pointer to the Tile array.
     */
    internal Tile[] getTiles() =>
	    _tiles;

    /**
     * Gets the BattlescapeState.
     * @return Pointer to the BattlescapeState.
     */
    internal BattlescapeState getBattleState() =>
        _battleState;

    /**
     * Sets the BattlescapeState.
     * @param bs A Pointer to a BattlescapeState.
     */
    internal void setBattleState(BattlescapeState bs) =>
        _battleState = bs;

    /**
     * Removes the body item that corresponds to the unit.
     */
    internal void removeUnconsciousBodyItem(BattleUnit bu)
    {
        // remove the unconscious body item corresponding to this unit
        foreach (var item in getItems())
        {
            if (item.getUnit() == bu)
            {
                removeItem(item);
                break;
            }
        }
    }

    /**
     * Removes an item from the game. Eg. when ammo item is depleted.
     * @param item The Item to remove.
     */
    internal void removeItem(BattleItem item)
    {
        // only delete once
        foreach (var it in _deleted)
        {
            if (it == item)
            {
                return;
            }
        }

        // due to strange design, the item has to be removed from the tile it is on too (if it is on a tile)
        Tile t = item.getTile();
        BattleUnit b = item.getOwner();
        if (t != null)
        {
            foreach (var it in t.getInventory())
            {
                if (it == item)
                {
                    t.getInventory().Remove(it);
                    break;
                }
            }
        }
        if (b != null)
        {
            foreach (var it in b.getInventory())
            {
                if (it == item)
                {
                    b.getInventory().Remove(it);
                    break;
                }
            }
        }

        foreach (var i in _items)
        {
            if (i == item)
            {
                _items.Remove(i);
                break;
            }
        }

        _deleted.Add(item);
        /*
        for (int i = 0; i < _mapsize_x * _mapsize_y * _mapsize_z; ++i)
        {
            for (std::vector<BattleItem*>::iterator it = _tiles[i]->getInventory()->begin(); it != _tiles[i]->getInventory()->end(); )
            {
                if ((*it) == item)
                {
                    it = _tiles[i]->getInventory()->erase(it);
                    return;
                }
                ++it;
            }
        }
        */
    }

    /**
     * Gets the current item ID.
     * @return Current item ID pointer.
     */
    internal ref int getCurrentItemId() =>
        ref _itemId;

    /**
     * Adds this unit to the vector of falling units,
     * if it doesn't already exist.
     * @param unit The unit.
     * @return Was the unit added?
     */
    internal bool addFallingUnit(BattleUnit unit)
    {
        bool add = true;
        foreach (var item in _fallingUnits)
        {
            if (unit == item)
            {
                add = false;
                break;
            }
        }
        if (add)
        {
            _fallingUnits.Insert(0, unit);
            _unitsFalling = true;
        }
        return add;
    }

    /**
     * Gets the mission type.
     * @return The mission type.
     */
    internal string getMissionType() =>
	    _missionType;

    /**
     * Saves the saved battle game to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
	    var node = new YamlMappingNode();
	    if (_objectivesNeeded != 0)
	    {
		    node.Add("objectivesDestroyed", _objectivesDestroyed.ToString());
		    node.Add("objectivesNeeded", _objectivesNeeded.ToString());
		    node.Add("objectiveType", _objectiveType.ToString());
	    }
	    node.Add("width", _mapsize_x.ToString());
	    node.Add("length", _mapsize_y.ToString());
	    node.Add("height", _mapsize_z.ToString());
	    node.Add("missionType", _missionType);
	    node.Add("globalshade", _globalShade.ToString());
        node.Add("turn", _turn.ToString());
        node.Add("selectedUnit", (_selectedUnit != null ? _selectedUnit.getId() : -1).ToString());
        node.Add("mapdatasets", new YamlSequenceNode(_mapDataSets.Select(x => new YamlScalarNode(x.getName()))));
        //TODO
        //#if 0
	    //    for (int i = 0; i < _mapsize_z * _mapsize_y * _mapsize_x; ++i)
	    //    {
	    //	    if (!_tiles[i]->isVoid())
	    //	    {
	    //		    node["tiles"].push_back(_tiles[i]->save());
	    //	    }
	    //    }
        //#else
	    // first, write out the field sizes we're going to use to write the tile data
	    node.Add("tileIndexSize", ((sbyte)Tile.serializationKey.index).ToString());
	    node.Add("tileTotalBytesPer", Tile.serializationKey.totalBytes.ToString());
	    node.Add("tileFireSize", ((sbyte)Tile.serializationKey._fire).ToString());
	    node.Add("tileSmokeSize", ((sbyte)Tile.serializationKey._smoke).ToString());
	    node.Add("tileIDSize", ((sbyte)Tile.serializationKey._mapDataID).ToString());
	    node.Add("tileSetIDSize", ((sbyte)Tile.serializationKey._mapDataSetID).ToString());
	    node.Add("tileBoolFieldsSize", ((sbyte)Tile.serializationKey.boolFields).ToString());

        uint tileDataSize = (uint)(Tile.serializationKey.totalBytes * _mapsize_z * _mapsize_y * _mapsize_x);
        var tileData = new byte[tileDataSize];
        var w = tileData.AsSpan();

        for (int i = 0; i < _mapsize_z * _mapsize_y * _mapsize_x; ++i)
	    {
		    if (!_tiles[i].isVoid())
		    {
                serializeInt(ref w, Tile.serializationKey.index, i);
                _tiles[i].saveBinary(ref w);
            }
            else
		    {
			    tileDataSize -= Tile.serializationKey.totalBytes;
		    }
	    }
        node.Add("totalTiles", (tileDataSize / Tile.serializationKey.totalBytes).ToString()); // not strictly necessary, just convenient
        node.Add("binTiles", $"!!binary {Convert.ToBase64String(tileData, 0, (int)tileDataSize)}");
        tileData = null;
        //#endif
        node.Add("nodes", new YamlSequenceNode(_nodes.Select(x => x.save())));
	    if (_missionType == "STR_BASE_DEFENSE")
	    {
            node.Add("moduleMap",
                new YamlSequenceNode(_baseModules.Select(x =>
                new YamlSequenceNode(x.Select(y => new YamlMappingNode(y.Key.ToString(), y.Value.ToString()))))));
        }
        node.Add("units", new YamlSequenceNode(_units.Select(x => x.save())));
        node.Add("items", new YamlSequenceNode(_items.Select(x => x.save())));
	    node.Add("tuReserved", ((int)_tuReserved).ToString());
	    node.Add("kneelReserved", _kneelReserved.ToString());
	    node.Add("depth", _depth.ToString());
	    node.Add("ambience", _ambience.ToString());
	    node.Add("ambientVolume", _ambientVolume.ToString());
        node.Add("recoverGuaranteed", new YamlSequenceNode(_recoverGuaranteed.Select(x => x.save())));
        node.Add("recoverConditional", new YamlSequenceNode(_recoverConditional.Select(x => x.save())));
	    node.Add("music", _music);
	    node.Add("turnLimit", _turnLimit.ToString());
	    node.Add("chronoTrigger", ((int)_chronoTrigger).ToString());
        node.Add("cheatTurn", _cheatTurn.ToString());

        return node;
    }

    /**
     * uses the depth variable to choose a palette.
     * @param state the state to set the palette for.
     */
    internal void setPaletteByDepth(State state)
    {
        if (_depth == 0)
        {
            state.setPalette("PAL_BATTLESCAPE");
        }
        else
        {
            string ss = $"PAL_BATTLESCAPE_{_depth}";
            state.setPalette(ss);
        }
    }

    /**
     * Loads the resources required by the map in the battle save.
     * @param mod Pointer to the mod.
     */
    internal void loadMapResources(Mod.Mod mod)
    {
        foreach (var mapDataSet in _mapDataSets)
        {
            mapDataSet.loadData(mod.getMCDPatch(mapDataSet.getName()));
        }

        int mdsID, mdID;

        for (int i = 0; i < _mapsize_z * _mapsize_y * _mapsize_x; ++i)
        {
            for (var part = TilePart.O_FLOOR; part <= TilePart.O_OBJECT; part++)
            {
                TilePart tp = (TilePart)part;
                _tiles[i].getMapData(out mdID, out mdsID, tp);
                if (mdID != -1 && mdsID != -1)
                {
                    _tiles[i].setMapData(_mapDataSets[mdsID].getObject((uint)mdID), mdID, mdsID, tp);
                }
            }
        }

        initUtilities(mod);
        getTileEngine().calculateSunShading();
        getTileEngine().calculateTerrainLighting();
        getTileEngine().calculateUnitLighting();
        getTileEngine().recalculateFOV();
    }

    /**
     * Initializes the map utilities.
     * @param mod Pointer to mod.
     */
    internal void initUtilities(Mod.Mod mod)
    {
        _pathfinding = new Pathfinding(this);
        _tileEngine = new TileEngine(this, mod.getVoxelData());
    }

    /**
     * Gets the global shade.
     * @return The global shade.
     */
    internal int getGlobalShade() =>
	    _globalShade;

    /**
     * Sets the mission type.
     * @param missionType The mission type.
     */
    internal void setMissionType(string missionType) =>
	    _missionType = missionType;

    /**
     * Sets the turn limit for this mission.
     * @param limit the turn limit.
     */
    internal void setTurnLimit(int limit) =>
        _turnLimit = limit;

    /**
     * Sets the action type to occur when the timer runs out.
     * @param trigger the action type to perform.
     */
    internal void setChronoTrigger(ChronoTrigger trigger) =>
        _chronoTrigger = trigger;

    /**
     * Sets the turn at which the players become exposed to the AI.
     * @param turn the turn to start cheating.
     */
    internal void setCheatTurn(int turn) =>
        _cheatTurn = turn;

    /**
     * Sets the global shade.
     * @param shade The global shade.
     */
    internal void setGlobalShade(int shade) =>
        _globalShade = shade;

    /**
     * set the depth of the battlescape game.
     * @param depth the intended depth 0-3.
     */
    internal void setDepth(int depth) =>
        _depth = depth;

    /**
     * Set the music track for this battle.
     * @param track the track name.
     */
    internal void setMusic(string track) =>
	    _music = track;

    /**
     * Finds a fitting node where a unit can spawn.
     * @param nodeRank Rank of the node (this is not the rank of the alien!).
     * @param unit Pointer to the unit (to get its position).
     * @return Pointer to the chosen node.
     */
    internal Node getSpawnNode(int nodeRank, BattleUnit unit)
    {
        int highestPriority = -1;
        var compliantNodes = new List<Node>();

        foreach (var i in getNodes())
        {
            if (i.isDummy())
            {
                continue;
            }
            if (i.getRank() == (NodeRank)nodeRank                     // ranks must match
                && (!((i.getType() & Node.TYPE_SMALL) == Node.TYPE_SMALL)
                    || unit.getArmor().getSize() == 1)                // the small unit bit is not set or the unit is small
                && (!((i.getType() & Node.TYPE_FLYING) == Node.TYPE_FLYING)
                    || unit.getMovementType() == MovementType.MT_FLY) // the flying unit bit is not set or the unit can fly
                && i.getPriority() > 0                                // priority 0 is no spawnplace
                && setUnitPosition(unit, i.getPosition(), true))      // check if not already occupied
            {
                if (i.getPriority() > highestPriority)
                {
                    highestPriority = i.getPriority();
                    compliantNodes.Clear(); // drop the last nodes, as we found a higher priority now
                }
                if (i.getPriority() == highestPriority)
                {
                    compliantNodes.Add(i);
                }
            }
        }

        if (!compliantNodes.Any()) return null;

        int n = RNG.generate(0, compliantNodes.Count - 1);

        return compliantNodes[n];
    }

    /**
     * Places a unit on or near a position.
     * @param unit The unit to place.
     * @param entryPoint The position around which to attempt to place the unit.
     * @return True if the unit was successfully placed.
     */
    internal bool placeUnitNearPosition(BattleUnit unit, Position entryPoint, bool largeFriend)
    {
	    if (setUnitPosition(unit, entryPoint))
	    {
		    return true;
	    }

	    int me = 0 - unit.getArmor().getSize();
	    int you = largeFriend ? 2 : 1;
	    int[] xArray = {0, you, you, you, 0, me, me, me};
	    int[] yArray = {me, me, 0, you, you, you, 0, me};
	    for (int dir = 0; dir <= 7; ++dir)
	    {
		    Position offset = new Position(xArray[dir], yArray[dir], 0);
		    Tile t = getTile(entryPoint + offset);
		    if (t != null && !getPathfinding().isBlocked(getTile(entryPoint + (offset / 2)), t, dir, null)
			    && setUnitPosition(unit, entryPoint + offset))
		    {
			    return true;
		    }
	    }

	    if (unit.getMovementType() == MovementType.MT_FLY)
	    {
		    Tile t = getTile(entryPoint + new Position(0, 0, 1));
		    if (t != null && t.hasNoFloor(getTile(entryPoint)) && setUnitPosition(unit, entryPoint + new Position(0, 0, 1)))
		    {
			    return true;
		    }
	    }
	    return false;
    }

    /**
     * Places units on the map. Handles large units that are placed on multiple tiles.
     * @param bu The unit to be placed.
     * @param position The position to place the unit.
     * @param testOnly If true then just checks if the unit can be placed at the position.
     * @return True if the unit could be successfully placed.
     */
    internal bool setUnitPosition(BattleUnit bu, Position position, bool testOnly = false)
    {
        int size = bu.getArmor().getSize() - 1;
        var zOffset = new Position(0,0,0);
        // first check if the tiles are occupied
        for (int x = size; x >= 0; x--)
        {
            for (int y = size; y >= 0; y--)
            {
                Tile t = getTile(position + new Position(x, y, 0) + zOffset);
                Tile tb = getTile(position + new Position(x, y, -1) + zOffset);
                if (t == null ||
                    (t.getUnit() != null && t.getUnit() != bu) ||
                    t.getTUCost((int)TilePart.O_OBJECT, bu.getMovementType()) == 255 ||
                    (t.hasNoFloor(tb) && bu.getMovementType() != MovementType.MT_FLY) ||
                    (t.getMapData(TilePart.O_OBJECT) != null && t.getMapData(TilePart.O_OBJECT).getBigWall() != 0 && t.getMapData(TilePart.O_OBJECT).getBigWall() <= 3))
                {
                    return false;
                }
                // move the unit up to the next level (desert and seabed terrains)
                if (t != null && t.getTerrainLevel() == -24)
                {
                    zOffset.z += 1;
                    x = size;
                    y = size + 1;
                }
            }
        }

        if (size > 0)
        {
            getPathfinding().setUnit(bu);
            for (int dir = 2; dir <= 4; ++dir)
            {
                if (getPathfinding().isBlocked(getTile(position + zOffset), null, dir, null))
                    return false;
            }
        }

        if (testOnly) return true;

        for (int x = size; x >= 0; x--)
        {
            for (int y = size; y >= 0; y--)
            {
                if (x == 0 && y == 0)
                {
                    bu.setPosition(position + zOffset);
                }
                getTile(position + new Position(x, y, 0) + zOffset).setUnit(bu, getTile(position + new Position(x, y, -1) + zOffset));
            }
        }

        return true;
    }

    /**
     * Set the objective type for the current battle.
     * @param the objective type.
     */
    internal void setObjectiveType(int type) =>
        _objectiveType = type;

    /**
     * increments the number of objectives to be destroyed.
     */
    internal void setObjectiveCount(int counter)
    {
        _objectivesNeeded = counter;
        _objectivesDestroyed = 0;
    }

    /**
     * Sets whether the objective is destroyed.
     */
    internal void addDestroyedObjective()
    {
        if (!allObjectivesDestroyed())
        {
            _objectivesDestroyed++;
            if (allObjectivesDestroyed())
            {
                if (getObjectiveType() == SpecialTileType.MUST_DESTROY)
                {
                    _battleState.getBattleGame().autoEndBattle();
                }
                else
                {
                    _battleState.getBattleGame().missionComplete();
                }
            }
        }
    }

    /**
     * Returns whether the objectives are destroyed.
     * @return True if the objectives are destroyed.
     */
    internal bool allObjectivesDestroyed() =>
	    (_objectivesNeeded > 0 && _objectivesDestroyed == _objectivesNeeded);

    /**
     * Get the objective type for the current battle.
     * @return the objective type.
     */
    internal SpecialTileType getObjectiveType() =>
	    (SpecialTileType)(_objectiveType);

    /**
     * Sets the currently selected unit.
     * @param unit Pointer to BattleUnit.
     */
    internal void setSelectedUnit(BattleUnit unit) =>
        _selectedUnit = unit;

    /**
     * Return a reference to the base module destruction map
     * this map contains information on how many destructible base modules
     * remain at any given grid reference in the basescape, using [x][y] format.
     * -1 for "no items" 0 for "destroyed" and any actual number represents how many left.
     * @return the base module damage map.
     */
    internal List<List<KeyValuePair<int, int>>> getModuleMap() =>
	    _baseModules;

    /**
     * Gets the maximum number of turns we have before this mission ends.
     * @return the turn limit.
     */
    internal int getTurnLimit() =>
	    _turnLimit;

    /**
     * Gets the action type to perform when the timer expires.
     * @return the action type to perform.
     */
    internal ChronoTrigger getChronoTrigger() =>
	    _chronoTrigger;

    /**
     * Sets whether the mission was aborted or successful.
     * @param flag True, if the mission was aborted, or false, if the mission was successful.
     */
    internal void setAborted(bool flag) =>
        _aborted = flag;

    /**
     * get the ambient battlescape sound effect.
     * @return the intended sound.
     */
    internal int getAmbientSound() =>
	    _ambience;

    /**
     * Gets the current debug mode.
     * @return Debug mode.
     */
    internal bool getDebugMode() =>
	    _debugMode;

    /**
     * set the ambient battlescape sound effect.
     * @param sound the intended sound.
     */
    internal void setAmbientSound(int sound) =>
        _ambience = sound;

    /**
     * Sets the ambient sound effect volume.
     * @param volume the ambient volume.
     */
    internal void setAmbientVolume(double volume) =>
        _ambientVolume = volume;

    /**
     * Gets the array of mapblocks.
     * @return Pointer to the array of mapblocks.
     */
    internal List<MapDataSet> getMapDataSets() =>
        _mapDataSets;

    /**
     * Initializes the array of tiles and creates a pathfinding object.
     * @param mapsize_x
     * @param mapsize_y
     * @param mapsize_z
     */
    internal void initMap(int mapsize_x, int mapsize_y, int mapsize_z, bool resetTerrain = true)
    {
        // Clear old map data
        if (_mapsize_z * _mapsize_y * _mapsize_x > 0)
        {
            Array.Clear(_tiles);
        }

        _nodes.Clear();

        if (resetTerrain)
        {
            _mapDataSets.Clear();
        }

        // Create tile objects
        _mapsize_x = mapsize_x;
        _mapsize_y = mapsize_y;
        _mapsize_z = mapsize_z;
        _tiles = new Tile[_mapsize_z * _mapsize_y * _mapsize_x];
        for (int i = 0; i < _mapsize_z * _mapsize_y * _mapsize_x; ++i)
        {
            var pos = new Position();
            getTileCoords(i, out pos.x, out pos.y, out pos.z);
            _tiles[i] = new Tile(pos);
        }
    }

    /**
     * Gives access to the "storage space" vector, for distribution of items in base defense missions.
     * @return Vector of storage positions.
     */
    internal List<Position> getStorageSpace() =>
	    _storageSpace;

    /**
     * calculate the number of map modules remaining by counting the map objects
     * on the top floor who have the baseModule flag set. we store this data in the grid
     * as outlined in the comments above, in pairs representing initial and current values.
     */
    internal void calculateModuleMap()
    {
        for (var i = 0; i < _mapsize_x / 10; i++)
        {
            var t1 = new List<KeyValuePair<int, int>>();
            for (var j = 0; j < _mapsize_y / 10; j++) t1.Add(KeyValuePair.Create(-1, -1));
            _baseModules.Add(t1);
        }

        for (int x = 0; x != _mapsize_x; ++x)
        {
            for (int y = 0; y != _mapsize_y; ++y)
            {
                for (int z = 0; z != _mapsize_z; ++z)
                {
                    Tile tile = getTile(new Position(x, y, z));
                    if (tile != null && tile.getMapData(TilePart.O_OBJECT) != null && tile.getMapData(TilePart.O_OBJECT).isBaseModule())
                    {
                        var key = _baseModules[x / 10][y / 10].Key;
                        _baseModules[x / 10][y / 10] = KeyValuePair.Create(key + key > 0 ? 1 : 2, key);
                    }
                }
            }
        }
    }

    /**
     * Returns whether the mission was aborted or successful.
     * @return True, if the mission was aborted, or false, if the mission was successful.
     */
    internal bool isAborted() =>
	    _aborted;

    /**
     * get the list of items we're guaranteed to take with us (ie: items that were in the skyranger)
     * @return the list of items we're guaranteed.
     */
    internal List<BattleItem> getGuaranteedRecoveredItems() =>
        _recoverGuaranteed;

    /**
     * get the list of items we're not guaranteed to take with us (ie: items that were NOT in the skyranger)
     * @return the list of items we might get.
     */
    internal List<BattleItem> getConditionalRecoveredItems() =>
        _recoverConditional;

    /**
     * Resets the turn counter.
     */
    internal void resetTurnCounter()
    {
        _turn = 1;
        _cheating = false;
        _side = UnitFaction.FACTION_PLAYER;
        _beforeGame = true;
    }

    /**
     * Selects the next player unit.
     * @param checkReselect Whether to check if we should reselect a unit.
     * @param setReselect Don't reselect a unit.
     * @param checkInventory Whether to check if the unit has an inventory.
     * @return Pointer to new selected BattleUnit, NULL if none can be selected.
     * @sa selectPlayerUnit
     */
    internal BattleUnit selectNextPlayerUnit(bool checkReselect = false, bool setReselect = false, bool checkInventory = false) =>
        selectPlayerUnit(+1, checkReselect, setReselect, checkInventory);

    /**
     * Selects the next player unit in a certain direction.
     * @param dir Direction to select, eg. -1 for previous and 1 for next.
     * @param checkReselect Whether to check if we should reselect a unit.
     * @param setReselect Don't reselect a unit.
     * @param checkInventory Whether to check if the unit has an inventory.
     * @return Pointer to new selected BattleUnit, NULL if none can be selected.
     */
    BattleUnit selectPlayerUnit(int dir, bool checkReselect, bool setReselect, bool checkInventory)
    {
        if (_selectedUnit != null && setReselect)
        {
            _selectedUnit.dontReselect();
        }
        if (!_units.Any())
        {
            return null;
        }

        var begin = 0;
        var end = 0;
        if (dir > 0)
        {
            begin = 0;
            end = _units.Count - 1;
        }
        else if (dir < 0)
        {
            begin = _units.Count - 1;
            end = 0;
        }

        int i = _units.IndexOf(_selectedUnit);
        do
        {
            // no unit selected
            if (i == -1)
            {
                i = begin;
                continue;
            }
            if (i != end)
            {
                i += dir;
            }
            // reached the end, wrap-around
            else
            {
                i = begin;
            }
            // back to where we started... no more units found
            if (_units[i] == _selectedUnit)
            {
                if (checkReselect && !_selectedUnit.reselectAllowed())
                    _selectedUnit = null;
                return _selectedUnit;
            }
            else if (_selectedUnit == null && i == begin)
            {
                return _selectedUnit;
            }
        }
        while (!_units[i].isSelectable(_side, checkReselect, checkInventory));

        _selectedUnit = _units[i];
        return _selectedUnit;
    }

    /**
     * Selects the previous player unit.
     * @param checkReselect Whether to check if we should reselect a unit.
     * @param setReselect Don't reselect a unit.
     * @param checkInventory Whether to check if the unit has an inventory.
     * @return Pointer to new selected BattleUnit, NULL if none can be selected.
     * @sa selectPlayerUnit
     */
    internal BattleUnit selectPreviousPlayerUnit(bool checkReselect, bool setReselect, bool checkInventory) =>
        selectPlayerUnit(-1, checkReselect, setReselect, checkInventory);

    /**
     * Resets all the units to their current standing tile(s).
     */
    internal void resetUnitTiles()
    {
        foreach (var i in _units)
        {
            if (!i.isOut())
            {
                int size = i.getArmor().getSize() - 1;
                if (i.getTile() != null && i.getTile().getUnit() == i)
                {
                    for (int x = size; x >= 0; x--)
                    {
                        for (int y = size; y >= 0; y--)
                        {
                            getTile(i.getTile().getPosition() + new Position(x, y, 0)).setUnit(null);
                        }
                    }
                }
                for (int x = size; x >= 0; x--)
                {
                    for (int y = size; y >= 0; y--)
                    {
                        Tile t = getTile(i.getPosition() + new Position(x, y, 0));
                        t.setUnit(i, getTile(t.getPosition() + new Position(0, 0, -1)));
                    }
                }

            }
            if (i.getFaction() == UnitFaction.FACTION_PLAYER)
            {
                i.setVisible(true);
            }
        }
        _beforeGame = false;
    }

    /**
     * Move all the leftover items in base defense missions to random locations in the storage facilities
     * @param t the tile where all our goodies are initially stored.
     */
    internal void randomizeItemLocations(Tile t)
    {
        if (_storageSpace.Any())
        {
            var inventory = t.getInventory();
            for (var it = 0; it < inventory.Count;)
            {
                if (inventory[it].getSlot().getId() == "STR_GROUND")
                {
                    getTile(_storageSpace[RNG.generate(0, _storageSpace.Count - 1)]).addItem(inventory[it], inventory[it].getSlot());
                    t.getInventory().RemoveAt(it);
                }
                else
                {
                    ++it;
                }
            }
        }
    }

    /**
     * Ends the current turn and progresses to the next one.
     */
    internal void endTurn()
    {
	    if (_side == UnitFaction.FACTION_PLAYER)
	    {
		    if (_selectedUnit != null && _selectedUnit.getOriginalFaction() == UnitFaction.FACTION_PLAYER)
			    _lastSelectedUnit = _selectedUnit;
		    _selectedUnit = null;
		    _side = UnitFaction.FACTION_HOSTILE;
	    }
	    else if (_side == UnitFaction.FACTION_HOSTILE)
	    {
		    _side = UnitFaction.FACTION_NEUTRAL;
		    // if there is no neutral team, we skip this and instantly prepare the new turn for the player
		    if (selectNextPlayerUnit() == null)
		    {
			    prepareNewTurn();
			    _turn++;
			    _side = UnitFaction.FACTION_PLAYER;
			    if (_lastSelectedUnit != null && _lastSelectedUnit.isSelectable(UnitFaction.FACTION_PLAYER, false, false))
				    _selectedUnit = _lastSelectedUnit;
			    else
				    selectNextPlayerUnit();
			    while (_selectedUnit != null && _selectedUnit.getFaction() != UnitFaction.FACTION_PLAYER)
				    selectNextPlayerUnit();
		    }

	    }
	    else if (_side == UnitFaction.FACTION_NEUTRAL)
	    {
		    prepareNewTurn();
		    _turn++;
		    _side = UnitFaction.FACTION_PLAYER;
		    if (_lastSelectedUnit != null && _lastSelectedUnit.isSelectable(UnitFaction.FACTION_PLAYER, false, false))
			    _selectedUnit = _lastSelectedUnit;
		    else
			    selectNextPlayerUnit();
		    while (_selectedUnit != null && _selectedUnit.getFaction() != UnitFaction.FACTION_PLAYER)
			    selectNextPlayerUnit();
	    }
	    int liveSoldiers, liveAliens;

	    _battleState.getBattleGame().tallyUnits(out liveAliens, out liveSoldiers);

	    if ((_turn > _cheatTurn / 2 && liveAliens <= 2) || _turn > _cheatTurn)
	    {
		    _cheating = true;
	    }

	    if (_side == UnitFaction.FACTION_PLAYER)
	    {
		    // update the "number of turns since last spotted"
		    foreach (var i in _units)
		    {
			    if (i.getTurnsSinceSpotted() < 255)
			    {
				    i.setTurnsSinceSpotted(i.getTurnsSinceSpotted() + 1);
			    }
			    if (_cheating && i.getFaction() == UnitFaction.FACTION_PLAYER && !i.isOut())
			    {
				    i.setTurnsSinceSpotted(0);
			    }
			    if (i.getAIModule() != null)
			    {
				    i.getAIModule().reset(); // clean up AI state
			    }
		    }
	    }
	    // hide all aliens (VOF calculations below will turn them visible again)
	    foreach (var i in _units)
	    {
		    if (i.getFaction() == _side)
		    {
			    i.prepareNewTurn();
		    }
		    if (i.getFaction() != UnitFaction.FACTION_PLAYER)
		    {
			    i.setVisible(false);
		    }
	    }

	    // re-run calculateFOV() *after* all aliens have been set not-visible
	    _tileEngine.recalculateFOV();

	    if (_side != UnitFaction.FACTION_PLAYER)
		    selectNextPlayerUnit();
    }

    /**
     * Carries out new turn preparations such as fire and smoke spreading.
     */
    void prepareNewTurn()
    {
	    var tilesOnFire = new List<Tile>();
	    var tilesOnSmoke = new List<Tile>();

	    // prepare a list of tiles on fire
	    for (int i = 0; i < _mapsize_x * _mapsize_y * _mapsize_z; ++i)
	    {
		    if (getTiles()[i].getFire() > 0)
		    {
			    tilesOnFire.Add(getTiles()[i]);
		    }
	    }

	    // first: fires spread
	    foreach (var i in tilesOnFire)
	    {
		    // if we haven't added fire here this turn
		    if (i.getOverlaps() == 0)
		    {
			    // reduce the fire timer
			    i.setFire(i.getFire() -1);

			    // if we're still burning
			    if (i.getFire() != 0)
			    {
				    // propagate in four cardinal directions (0, 2, 4, 6)
				    for (int dir = 0; dir <= 6; dir += 2)
				    {
					    Pathfinding.directionToVector(dir, out var pos);
					    Tile t = getTile(i.getPosition() + pos);
					    // if there's no wall blocking the path of the flames...
					    if (t != null && getTileEngine().horizontalBlockage(i, t, ItemDamageType.DT_IN) == 0)
					    {
						    // attempt to set this tile on fire
						    t.ignite(i.getSmoke());
					    }
				    }
			    }
			    // fire has burnt out
			    else
			    {
				    i.setSmoke(0);
				    // burn this tile, and any object in it, if it's not fireproof/indestructible.
				    if (i.getMapData(TilePart.O_OBJECT) != null)
				    {
					    if (i.getMapData(TilePart.O_OBJECT).getFlammable() != 255 && i.getMapData(TilePart.O_OBJECT).getArmor() != 255)
					    {
						    if (i.destroy(TilePart.O_OBJECT, getObjectiveType()))
						    {
							    addDestroyedObjective();
						    }
						    if (i.destroy(TilePart.O_FLOOR, getObjectiveType()))
						    {
							    addDestroyedObjective();
						    }
					    }
				    }
				    else if (i.getMapData(TilePart.O_FLOOR) != null)
				    {
					    if (i.getMapData(TilePart.O_FLOOR).getFlammable() != 255 && i.getMapData(TilePart.O_FLOOR).getArmor() != 255)
					    {
						    if (i.destroy(TilePart.O_FLOOR, getObjectiveType()))
						    {
							    addDestroyedObjective();
						    }
					    }
				    }
				    getTileEngine().applyGravity(i);
			    }
		    }
	    }

	    // prepare a list of tiles on fire/with smoke in them (smoke acts as fire intensity)
	    for (int i = 0; i < _mapsize_x * _mapsize_y * _mapsize_z; ++i)
	    {
		    if (getTiles()[i].getSmoke() > 0)
		    {
			    tilesOnSmoke.Add(getTiles()[i]);
		    }
		    getTiles()[i].setDangerous(false);
	    }

	    // now make the smoke spread.
	    foreach (var i in tilesOnSmoke)
	    {
		    // smoke and fire follow slightly different rules.
		    if (i.getFire() == 0)
		    {
			    // reduce the smoke counter
			    i.setSmoke(i.getSmoke() - 1);
			    // if we're still smoking
			    if (i.getSmoke() != 0)
			    {
				    // spread in four cardinal directions
				    for (int dir = 0; dir <= 6; dir += 2)
				    {
					    Pathfinding.directionToVector(dir, out var pos);
					    Tile t = getTile(i.getPosition() + pos);
					    // as long as there are no walls blocking us
					    if (t != null && getTileEngine().horizontalBlockage(i, t, ItemDamageType.DT_SMOKE) == 0)
					    {
						    // only add smoke to empty tiles, or tiles with no fire, and smoke that was added this turn
						    if (t.getSmoke() == 0 || (t.getFire() == 0 && t.getOverlaps() != 0))
						    {
							    t.addSmoke(i.getSmoke());
						    }
					    }
				    }
			    }
		    }
		    else
		    {
			    // smoke from fire spreads upwards one level if there's no floor blocking it.
			    Position pos = new Position(0,0,1);
			    Tile t = getTile(i.getPosition() + pos);
			    if (t != null && t.hasNoFloor(i))
			    {
				    // only add smoke equal to half the intensity of the fire
				    t.addSmoke(i.getSmoke()/2);
			    }
			    // then it spreads in the four cardinal directions.
			    for (int dir = 0; dir <= 6; dir += 2)
			    {
				    Pathfinding.directionToVector(dir, out pos);
				    t = getTile(i.getPosition() + pos);
				    if (t != null && getTileEngine().horizontalBlockage(i, t, ItemDamageType.DT_SMOKE) == 0)
				    {
					    t.addSmoke(i.getSmoke()/2);
				    }
			    }
		    }
	    }

	    if (tilesOnFire.Any() || tilesOnSmoke.Any())
	    {
		    // do damage to units, average out the smoke, etc.
		    for (int i = 0; i < _mapsize_x * _mapsize_y * _mapsize_z; ++i)
		    {
			    if (getTiles()[i].getSmoke() != 0)
				    getTiles()[i].prepareNewTurn(getDepth() == 0);
		    }
		    // fires could have been started, stopped or smoke could reveal/conceal units.
		    getTileEngine().calculateTerrainLighting();
	    }

	    reviveUnconsciousUnits();
    }

    /**
     * Checks for units that are unconscious and revives them if they shouldn't be.
     *
     * Revived units need a tile to stand on. If the unit's current position is occupied, then
     * all directions around the tile are searched for a free tile to place the unit in.
     * If no free tile is found the unit stays unconscious.
     */
    internal void reviveUnconsciousUnits()
    {
	    foreach (var i in getUnits())
	    {
		    if (i.getArmor().getSize() == 1)
		    {
			    Position originalPosition = i.getPosition();
			    if (originalPosition == new Position(-1, -1, -1))
			    {
				    foreach (var j in _items)
				    {
					    if (j.getUnit() != null && j.getUnit() == i && j.getOwner() != null)
					    {
						    originalPosition = j.getOwner().getPosition();
					    }
				    }
			    }
			    if (i.getStatus() == UnitStatus.STATUS_UNCONSCIOUS && i.getStunlevel() < i.getHealth() && i.getHealth() > 0)
			    {
				    Tile targetTile = getTile(originalPosition);
				    bool largeUnit = targetTile != null && targetTile.getUnit() != null && targetTile.getUnit() != i && targetTile.getUnit().getArmor().getSize() != 1;
				    if (placeUnitNearPosition(i, originalPosition, largeUnit))
				    {
					    // recover from unconscious
					    i.turn(false); // makes the unit stand up again
					    i.kneel(false);
					    i.setCache(null);
					    getTileEngine().calculateFOV(i);
					    getTileEngine().calculateUnitLighting();
					    removeUnconsciousBodyItem(i);
				    }
			    }
		    }
	    }
    }

    /**
     * Sets the kneel reservation setting.
     * @param reserved Should we reserve an extra 4 TUs to kneel?
     */
    internal void setKneelReserved(bool reserved) =>
	    _kneelReserved = reserved;

    /**
     * Converts a unit into a unit of another type.
     * @param unit The unit to convert.
     * @return Pointer to the new unit.
     */
    internal BattleUnit convertUnit(BattleUnit unit, SavedGame saveGame, Mod.Mod mod)
    {
	    string newType = unit.getSpawnUnit();
	    bool visible = unit.getVisible();
	    // in case the unit was unconscious
	    removeUnconsciousBodyItem(unit);

	    unit.instaKill();

	    foreach (var i in unit.getInventory())
	    {
		    getTileEngine().itemDrop(getTile(unit.getPosition()), i, mod);
		    i.setOwner(null);
	    }

	    unit.getInventory().Clear();

	    // remove unit-tile link
	    unit.setTile(null);

	    getTile(unit.getPosition()).setUnit(null);
	    Unit newRule = mod.getUnit(newType, true);
	    string newArmor = newRule.getArmor();
	    string terroristWeapon = newRule.getRace().Substring(4);
	    terroristWeapon += "_WEAPON";
	    RuleItem newItem = mod.getItem(terroristWeapon);

	    BattleUnit newUnit = new BattleUnit(newRule,
		    UnitFaction.FACTION_HOSTILE,
		    getUnits().Last().getId() + 1,
		    mod.getArmor(newArmor, true),
		    mod.getStatAdjustment((int)saveGame.getDifficulty()),
		    getDepth());

	    getTile(unit.getPosition()).setUnit(newUnit, getTile(unit.getPosition() + new Position(0,0,-1)));
	    newUnit.setPosition(unit.getPosition());
	    newUnit.setDirection(unit.getDirection());
	    newUnit.setCache(null);
	    newUnit.setTimeUnits(0);
	    newUnit.setSpecialWeapon(this, mod);
	    getUnits().Add(newUnit);
	    newUnit.setAIModule(new AIModule(this, newUnit, null));
	    if (newItem != null)
	    {
		    BattleItem bi = new BattleItem(newItem, ref getCurrentItemId());
		    bi.moveToOwner(newUnit);
		    bi.setSlot(mod.getInventory("STR_RIGHT_HAND", true));
		    getItems().Add(bi);
	    }
	    newUnit.setVisible(visible);
	    getTileEngine().calculateFOV(newUnit.getPosition());
	    getTileEngine().applyGravity(newUnit.getTile());
	    newUnit.dontReselect();
	    return newUnit;
    }

    /**
     * Returns whether there are any units falling in the battlescape.
     * @return True if there are any units falling in the battlescape.
     */
    internal bool getUnitsFalling() =>
	    _unitsFalling;

    /**
     * Toggles the switch that says "there are units falling, start the fall state".
     * @param fall True if there are any units falling in the battlescape.
     */
    internal void setUnitsFalling(bool fall) =>
	    _unitsFalling = fall;

    /**
     * Resets all unit hit state flags.
     */
    internal void resetUnitHitStates()
    {
	    foreach (var i in _units)
	    {
		    i.resetHitState();
	    }
    }

    /**
     * get a pointer to the geoscape save
     * @return a pointer to the geoscape save.
     */
    internal SavedGame getGeoscapeSave() =>
	    _battleState.getGame().getSavedGame();

    /**
     * Checks if an item can be used in the current battlescape conditions.
     * @return True if it's usable, False otherwise.
     */
    internal bool isItemUsable(BattleItem item) =>
	    string.IsNullOrEmpty(getItemUsable(item));

    /**
     * Checks if an item can be used in the current battlescape conditions.
     * @return Error string if it can't be used, "" otherwise.
     */
    internal string getItemUsable(BattleItem item)
    {
	    if (_depth == 0 &&
		    (item.getRules().isWaterOnly() ||
		    (item.getAmmoItem() != null && item.getAmmoItem().getRules().isWaterOnly())))
	    {
		    return "STR_UNDERWATER_EQUIPMENT";
	    }
	    if (_depth != 0 &&
		    (item.getRules().isLandOnly() ||
		    (item.getAmmoItem() != null && item.getAmmoItem().getRules().isLandOnly())))
	    {
		    return "STR_LAND_EQUIPMENT";
	    }
	    return string.Empty;
    }

    /**
     * is the AI allowed to cheat?
     * @return true if cheating.
     */
    internal bool isCheating() =>
	    _cheating;

    /**
     * @return the tilesearch vector for use in AI functions.
     */
    internal List<Position> getTileSearch() =>
	    _tileSearch;

    /**
     * Finds a fitting node where a unit can patrol to.
     * @param scout Is the unit scouting?
     * @param unit Pointer to the unit (to get its position).
     * @param fromNode Pointer to the node the unit is at.
     * @return Pointer to the chosen node.
     */
    internal Node getPatrolNode(bool scout, BattleUnit unit, Node fromNode)
    {
	    var compliantNodes = new List<Node>();
	    Node preferred = null;

	    if (fromNode == null)
	    {
		    if (Options.traceAI) { Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} This alien got lost. :("); }
		    fromNode = getNodes()[RNG.generate(0, getNodes().Count - 1)];
		    while (fromNode.isDummy())
		    {
			    fromNode = getNodes()[RNG.generate(0, getNodes().Count - 1)];
		    }
	    }

	    // scouts roam all over while all others shuffle around to adjacent nodes at most:
	    int end = scout ? getNodes().Count : fromNode.getNodeLinks().Count;

	    for (int i = 0; i < end; ++i)
	    {
		    if (!scout && fromNode.getNodeLinks()[i] < 1) continue;

		    Node n = getNodes()[scout ? i : fromNode.getNodeLinks()[i]];
		    if ( !n.isDummy()																				        // don't consider dummy nodes.
			    && (n.getFlags() > 0 || n.getRank() > 0 || scout)											        // for non-scouts we find a node with a desirability above 0
			    && (!((n.getType() & Node.TYPE_SMALL) != 0) || unit.getArmor().getSize() == 1)					    // the small unit bit is not set or the unit is small
			    && (!((n.getType() & Node.TYPE_FLYING) != 0) || unit.getMovementType() == MovementType.MT_FLY)		// the flying unit bit is not set or the unit can fly
			    && !n.isAllocated()																		            // check if not allocated
			        && !((n.getType() & Node.TYPE_DANGEROUS) != 0)													// don't go there if an alien got shot there; stupid behavior like that
			    && setUnitPosition(unit, n.getPosition(), true)											            // check if not already occupied
			    && getTile(n.getPosition()) != null && getTile(n.getPosition()).getFire() == 0						// you are not a firefighter; do not patrol into fire
			    && (unit.getFaction() != UnitFaction.FACTION_HOSTILE || !getTile(n.getPosition()).getDangerous())	// aliens don't run into a grenade blast
			            && (!scout || n != fromNode)																// scouts push forward
			    && n.getPosition().x > 0 && n.getPosition().y > 0)
		    {
			    if (preferred == null
				    || (unit.getRankInt() >=0 &&
					    (int)preferred.getRank() == Node.nodeRank[unit.getRankInt(), 0] &&
					    preferred.getFlags() < n.getFlags())
				    || preferred.getFlags() < n.getFlags())
			    {
				    preferred = n;
			    }
			    compliantNodes.Add(n);
		    }
	    }

	    if (!compliantNodes.Any())
	    {
		    if (Options.traceAI) { Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} {(scout ? "Scout " : "Guard")} found on patrol node! XXX XXX XXX"); }
		    if (unit.getArmor().getSize() > 1 && !scout)
		    {
			    return getPatrolNode(true, unit, fromNode); // move dammit
		    }
		    else
			    return null;
	    }

	    if (scout)
	    {
		    // scout picks a random destination:
		    return compliantNodes[RNG.generate(0, compliantNodes.Count - 1)];
	    }
	    else
	    {
		    if (preferred == null) return null;

		    // non-scout patrols to highest value unoccupied node that's not fromNode
		    if (Options.traceAI) { Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Choosing node flagged {preferred.getFlags()}"); }
		    return preferred;
	    }
    }

    /**
     * Gets all units in the battlescape that are falling.
     * @return The falling units in the battlescape.
     */
    internal List<BattleUnit> getFallingUnits() =>
	    _fallingUnits;

    /**
     * Turns on debug mode.
     */
    internal void setDebugMode()
    {
	    for (int i = 0; i < _mapsize_z * _mapsize_y * _mapsize_x; ++i)
	    {
		    _tiles[i].setDiscovered(true, 2);
	    }

	    _debugMode = true;
    }

    /**
     * Resets visibility of all the tiles on the map.
     */
    internal void resetTiles()
    {
	    for (int i = 0; i != getMapSizeXYZ(); ++i)
	    {
		    _tiles[i].setDiscovered(false, 0);
		    _tiles[i].setDiscovered(false, 1);
		    _tiles[i].setDiscovered(false, 2);
	    }
    }

    /**
     * Get the music track for the current battle.
     * @return the name of the music track.
     */
    internal string getMusic() =>
	    _music;

    /**
     * Selects the unit at the given position on the map.
     * @param pos Position.
     * @return Pointer to a BattleUnit, or 0 when none is found.
     */
    internal BattleUnit selectUnit(Position pos)
    {
	    BattleUnit bu = getTile(pos).getUnit();

	    if (bu != null && bu.isOut())
	    {
		    return null;
	    }
	    else
	    {
		    return bu;
	    }
    }
}
