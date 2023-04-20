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
    SavedBattleGame()
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
    internal void getTileCoords(int index, ref int x, ref int y, ref int z)
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
    int getTileIndex(Position pos) =>
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
                        break;
                    case 4:
                        result += 10;
                        break;
                    case 3:
                        result += 5;
                        break;
                    case 2:
                        result += 10;
                        break;
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
                    break;
                case 4:
                    result += 20;
                    break;
                case 3:
                    result += 10;
                    break;
                case 2:
                    result += 20;
                    break;
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
    void removeItem(BattleItem item)
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
            for (int part = (int)TilePart.O_FLOOR; part <= (int)TilePart.O_OBJECT; part++)
            {
                TilePart tp = (TilePart)part;
                _tiles[i].getMapData(out mdID, out mdsID, tp);
                if (mdID != -1 && mdsID != -1)
                {
                    _tiles[i].setMapData(_mapDataSets[mdsID].getObject(mdID), mdID, mdsID, tp);
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
    void initUtilities(Mod.Mod mod)
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
}
