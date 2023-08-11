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

namespace SharpXcom.Battlescape;

enum bigWallTypes { BLOCK = 1, BIGWALLNESW, BIGWALLNWSE, BIGWALLWEST, BIGWALLNORTH, BIGWALLEAST, BIGWALLSOUTH, BIGWALLEASTANDSOUTH, BIGWALLWESTANDNORTH };

/**
 * A utility class that calculates the shortest path between two points on the battlescape map.
 */
internal class Pathfinding
{
    internal const int DIR_UP = 8;
    internal const int DIR_DOWN = 9;
    const int O_BIGWALL = -1;
    internal static int red = 3;
    internal static int yellow = 10;
    internal static int green = 4;

    SavedBattleGame _save;
    BattleUnit _unit;
    bool _pathPreviewed;
    bool _strafeMove;
    int _totalTUCost;
    bool _modifierUsed;
    MovementType _movementType;
    int _size;
    List<PathfindingNode> _nodes;
    List<int> _path;

    /**
     * Sets up a Pathfinding.
     * @param save pointer to SavedBattleGame object.
     */
    internal Pathfinding(SavedBattleGame save)
    {
        _save = save;
        _unit = null;
        _pathPreviewed = false;
        _strafeMove = false;
        _totalTUCost = 0;
        _modifierUsed = false;
        _movementType = MovementType.MT_WALK;

        _size = _save.getMapSizeXYZ();
        // Initialize one node per tile
        _nodes = new List<PathfindingNode>(_size);
        for (int i = 0; i < _size; ++i)
        {
            var p = new Position();
            _save.getTileCoords(i, out p.x, out p.y, out p.z);
            _nodes.Add(new PathfindingNode(p));
        }
    }

    /**
     * Deletes the Pathfinding.
     * @internal This is required to be here because it requires the PathfindingNode class definition.
     */
    ~Pathfinding()
    {
        // Nothing more to do here.
    }

    /**
     * Marks tiles for the path preview.
     * @param bRemove Remove preview?
     * @return True, if a path is previewed.
     */
    bool previewPath(bool bRemove)
    {
        if (!_path.Any())
            return false;

        if (!bRemove && _pathPreviewed)
            return false;

        _pathPreviewed = !bRemove;

        Position pos = _unit.getPosition();
        int tus = _unit.getTimeUnits();
        if (_unit.isKneeled())
        {
            tus -= 8;
        }
        int energy = _unit.getEnergy();
        int size = _unit.getArmor().getSize() - 1;
        int total = _unit.isKneeled() ? 8 : 0;
        bool switchBack = false;
        if (_save.getBattleGame().getReservedAction() == BattleActionType.BA_NONE)
        {
            switchBack = true;
            _save.getBattleGame().setTUReserved(BattleActionType.BA_AUTOSHOT);
        }
        _modifierUsed = (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0;
        bool running = Options.strafe && _modifierUsed && _unit.getArmor().getSize() == 1 && _path.Count > 1;

        for (var i = _path.Count - 1; i >= 0; i--)
        {
            int dir = _path[i];
            int tu = getTUCost(pos, dir, out var destination, _unit, null, false); // gets tu cost, but also gets the destination position.
            int energyUse = tu;
            if (dir >= DIR_UP)
            {
                energyUse = 0;
            }
            else if (running)
            {
                tu = (int)(tu * 0.75);
                energyUse = (int)(energyUse * 1.5);
            }
            energy -= energyUse / 2;
            tus -= tu;
            total += tu;
            bool reserve = _save.getBattleGame().checkReservedTU(_unit, total, true);
            pos = destination;
            for (int x = size; x >= 0; x--)
            {
                for (int y = size; y >= 0; y--)
                {
                    Tile tile = _save.getTile(pos + new Position(x, y, 0));
                    Tile tileAbove = _save.getTile(pos + new Position(x, y, 1));
                    if (!bRemove)
                    {
                        if (i == 0)
                        {
                            tile.setPreview(10);
                        }
                        else
                        {
                            int nextDir = _path[i - 1];
                            tile.setPreview(nextDir);
                        }
                        if ((x != 0 && y != 0) || size == 0)
                        {
                            tile.setTUMarker(Math.Max(0, tus));
                        }
                        if (tileAbove != null && tileAbove.getPreview() == 0 && tu == 0 && _movementType != MovementType.MT_FLY) //unit fell down, retroactively make the tile above's direction marker to DOWN
                        {
                            tileAbove.setPreview(DIR_DOWN);
                        }
                    }
                    else
                    {
                        tile.setPreview(-1);
                        tile.setTUMarker(-1);
                    }
                    tile.setMarkerColor(bRemove ? 0 : ((tus >= 0 && energy >= 0) ? (reserve ? green : yellow) : red));
                }
            }
        }
        if (switchBack)
        {
            _save.getBattleGame().setTUReserved(BattleActionType.BA_NONE);
        }
        return true;
    }

    /**
     * Unmarks the tiles used for the path preview.
     * @return True, if the previewed path was removed.
     */
    internal bool removePreview()
    {
        if (!_pathPreviewed)
            return false;
        previewPath(true);
        return true;
    }

    /**
	 * Gets the TU cost to move from 1 tile to the other (ONE STEP ONLY).
	 * But also updates the endPosition, because it is possible
	 * the unit goes upstairs or falls down while walking.
	 * @param startPosition The position to start from.
	 * @param direction The direction we are facing.
	 * @param endPosition The position we want to reach.
	 * @param unit The unit moving.
	 * @param target The target unit.
	 * @param missile Is this a guided missile?
	 * @return TU cost or 255 if movement is impossible.
	 */
    int getTUCost(Position startPosition, int direction, out Position endPosition, BattleUnit unit, BattleUnit target, bool missile)
    {
        _unit = unit;
        directionToVector(direction, out endPosition);
        endPosition += startPosition;
        bool fellDown = false;
        bool triedStairs = false;
        int size = _unit.getArmor().getSize() - 1;
        int cost = 0;
        int numberOfPartsGoingUp = 0;
        int numberOfPartsGoingDown = 0;
        int numberOfPartsFalling = 0;
        int numberOfPartsChangingHeight = 0;
        int totalCost = 0;

        for (int x = 0; x <= size; ++x)
            for (int y = 0; y <= size; ++y)
            {
                Position offset = new Position(x, y, 0);
                Tile startTile = _save.getTile(startPosition + offset);
                Tile destinationTile = _save.getTile(endPosition + offset);
                Tile belowDestination = _save.getTile(endPosition + offset + new Position(0, 0, -1));
                Tile aboveDestination = _save.getTile(endPosition + offset + new Position(0, 0, 1));

                // this means the destination is probably outside the map
                if (startTile == null || destinationTile == null)
                    return 255;
                if (x == 0 && y == 0 && _movementType != MovementType.MT_FLY && canFallDown(startTile, size + 1))
                {
                    if (direction != DIR_DOWN)
                    {
                        return 255; //cannot walk on air
                    }
                    else
                    {
                        fellDown = true;
                    }
                }
                if (direction < DIR_UP && startTile.getTerrainLevel() > -16)
                {
                    // check if we can go this way
                    if (isBlocked(startTile, destinationTile, direction, target))
                        return 255;
                    if (startTile.getTerrainLevel() - destinationTile.getTerrainLevel() > 8)
                        return 255;
                }

                // this will later be used to re-cast the start tile again.
                var verticalOffset = new Position(0, 0, 0);

                // if we are on a stairs try to go up a level
                if (direction < DIR_UP && startTile.getTerrainLevel() <= -16 && aboveDestination != null && !aboveDestination.hasNoFloor(destinationTile))
                {
                    numberOfPartsGoingUp++;
                    verticalOffset.z++;

                    if (!triedStairs)
                    {
                        endPosition.z++;
                        destinationTile = _save.getTile(endPosition + offset);
                        belowDestination = _save.getTile(endPosition + new Position(x, y, -1));
                        triedStairs = true;
                    }
                }
                else if (direction < DIR_UP && !fellDown && _movementType != MovementType.MT_FLY && belowDestination != null && canFallDown(destinationTile) && belowDestination.getTerrainLevel() <= -12)
                {
                    numberOfPartsGoingDown++;

                    if (numberOfPartsGoingDown == (size + 1) * (size + 1))
                    {
                        endPosition.z--;
                        destinationTile = _save.getTile(endPosition + offset);
                        belowDestination = _save.getTile(endPosition + new Position(x, y, -1));
                        fellDown = true;
                    }
                }
                else if (!missile && _movementType == MovementType.MT_FLY && belowDestination != null && belowDestination.getUnit() != null && belowDestination.getUnit() != unit)
                {
                    // 2 or more voxels poking into this tile = no go
                    if (belowDestination.getUnit().getHeight() + belowDestination.getUnit().getFloatHeight() - belowDestination.getTerrainLevel() > 26)
                    {
                        return 255;
                    }
                }

                // this means the destination is probably outside the map
                if (destinationTile == null)
                    return 255;

                if (direction < DIR_UP && endPosition.z == startTile.getPosition().z)
                {
                    // check if we can go this way
                    if (isBlocked(startTile, destinationTile, direction, target))
                        return 255;
                    if (startTile.getTerrainLevel() - destinationTile.getTerrainLevel() > 8)
                        return 255;
                }
                else if (direction >= DIR_UP && !fellDown)
                {
                    // check if we can go up or down through gravlift or fly
                    if (validateUpDown(unit, startPosition + offset, direction, missile))
                    {
                        cost = 8; // vertical movement by flying suit or grav lift
                    }
                    else
                    {
                        return 255;
                    }
                }

                // check if we have floor, else fall down
                if (_movementType != MovementType.MT_FLY && !fellDown && canFallDown(startTile))
                {
                    numberOfPartsFalling++;

                    if (numberOfPartsFalling == (size + 1) * (size + 1) && direction != DIR_DOWN)
                    {
                        return 0;
                    }
                }
                startTile = _save.getTile(startTile.getPosition() + verticalOffset);


                if (direction < DIR_UP && numberOfPartsGoingUp != 0)
                {
                    // check if we can go this way
                    if (isBlocked(startTile, destinationTile, direction, target))
                        return 255;
                    if (startTile.getTerrainLevel() - destinationTile.getTerrainLevel() > 8)
                        return 255;
                }

                int wallcost = 0; // walking through rubble walls, but don't charge for walking diagonally through doors (which is impossible),
                                  // they're a special case unto themselves, if we can walk past them diagonally, it means we can go around,
                                  // as there is no wall blocking us.
                if (direction == 0 || direction == 7 || direction == 1)
                    wallcost += startTile.getTUCost((int)TilePart.O_NORTHWALL, _movementType);
                if (!fellDown && (direction == 2 || direction == 1 || direction == 3))
                    wallcost += destinationTile.getTUCost((int)TilePart.O_WESTWALL, _movementType);
                if (!fellDown && (direction == 4 || direction == 3 || direction == 5))
                    wallcost += destinationTile.getTUCost((int)TilePart.O_NORTHWALL, _movementType);
                if (direction == 6 || direction == 5 || direction == 7)
                    wallcost += startTile.getTUCost((int)TilePart.O_WESTWALL, _movementType);
                // don't let tanks phase through doors.
                if (x != 0 && y != 0)
                {
                    if ((destinationTile.getMapData(TilePart.O_NORTHWALL) != null && destinationTile.getMapData(TilePart.O_NORTHWALL).isDoor()) ||
                        (destinationTile.getMapData(TilePart.O_WESTWALL) != null && destinationTile.getMapData(TilePart.O_WESTWALL).isDoor()))
                    {
                        return 255;
                    }
                }
                // check if the destination tile can be walked over
                if (isBlocked(destinationTile, (int)TilePart.O_FLOOR, target) || isBlocked(destinationTile, (int)TilePart.O_OBJECT, target))
                {
                    return 255;
                }

                // if we don't want to fall down and there is no floor, we can't know the TUs so it's default to 4
                if (direction < DIR_UP && !fellDown && destinationTile.hasNoFloor(null))
                {
                    cost = 4;
                }
                // calculate the cost by adding floor walk cost and object walk cost
                if (direction < DIR_UP)
                {
                    cost += destinationTile.getTUCost((int)TilePart.O_FLOOR, _movementType);
                    if (!fellDown && !triedStairs && destinationTile.getMapData(TilePart.O_OBJECT) != null)
                    {
                        cost += destinationTile.getTUCost((int)TilePart.O_OBJECT, _movementType);
                    }
                    // climbing up a level costs one extra
                    if (verticalOffset.z > 0)
                    {
                        cost++;
                    }
                }

                // diagonal walking (uneven directions) costs 50% more tu's
                if (direction < DIR_UP && (direction & 1) != 0)
                {
                    wallcost /= 2;
                    cost = (int)((double)cost * 1.5);
                }
                cost += wallcost;
                if (_unit.getFaction() != UnitFaction.FACTION_PLAYER &&
                    _unit.getSpecialAbility() < (int)SpecialAbility.SPECAB_BURNFLOOR &&
                    destinationTile.getFire() > 0)
                    cost += 32; // try to find a better path, but don't exclude this path entirely.

                // TFTD thing: underwater tiles on fire or filled with smoke cost 2 TUs more for whatever reason.
                if (_save.getDepth() > 0 && (destinationTile.getFire() > 0 || destinationTile.getSmoke() > 0))
                {
                    cost += 2;
                }

                // Strafing costs +1 for forwards-ish or sidewards, propose +2 for backwards-ish directions
                // Maybe if flying then it makes no difference?
                if (Options.strafe && _strafeMove)
                {
                    if (size != 0)
                    {
                        // 4-tile units not supported.
                        // Turn off strafe move and continue
                        _strafeMove = false;
                    }
                    else
                    {
                        if (Math.Min(Math.Abs(8 + direction - _unit.getDirection()), Math.Min(Math.Abs(_unit.getDirection() - direction), Math.Abs(8 + _unit.getDirection() - direction))) > 2)
                        {
                            // Strafing backwards-ish currently unsupported, turn it off and continue.
                            _strafeMove = false;
                        }
                        else
                        {
                            if (_unit.getDirection() != direction)
                            {
                                cost += 1;
                            }
                        }
                    }
                }
                totalCost += cost;
                cost = 0;
            }

        // for bigger sized units, check the path between parts in an X shape at the end position
        if (size != 0)
        {
            totalCost /= (size + 1) * (size + 1);
            Tile startTile = _save.getTile(endPosition + new Position(1, 1, 0));
            Tile destinationTile = _save.getTile(endPosition);
            int tmpDirection = 7;
            if (isBlocked(startTile, destinationTile, tmpDirection, target))
                return 255;
            if (!fellDown && Math.Abs(startTile.getTerrainLevel() - destinationTile.getTerrainLevel()) > 10)
                return 255;
            startTile = _save.getTile(endPosition + new Position(1, 0, 0));
            destinationTile = _save.getTile(endPosition + new Position(0, 1, 0));
            tmpDirection = 5;
            if (isBlocked(startTile, destinationTile, tmpDirection, target))
                return 255;
            if (!fellDown && Math.Abs(startTile.getTerrainLevel() - destinationTile.getTerrainLevel()) > 10)
                return 255;
            // also check if we change level, that there are two parts changing level,
            // so a big sized unit can not go up a small sized stairs
            if (numberOfPartsChangingHeight == 1)
                return 255;
        }

        if (missile)
            return 0;
        else
            return totalCost;
    }

    static Position oneTileNorth = new Position(0, -1, 0);
    static Position oneTileEast = new Position(1, 0, 0);
    static Position oneTileSouth = new Position(0, 1, 0);
    static Position oneTileWest = new Position(-1, 0, 0);
    /**
	 * Determines whether going from one tile to another blocks movement.
	 * @param startTile The tile to start from.
	 * @param endTile The tile we want to reach.
	 * @param direction The direction we are facing.
	 * @param missileTarget Target for a missile.
	 * @return True if the movement is blocked.
	 */
    internal bool isBlocked(Tile startTile, Tile _ /*endTile*/, int direction, BattleUnit missileTarget)
    {
        // check if the difference in height between start and destination is not too high
        // so we can not jump to the highest part of the stairs from the floor
        // stairs terrainlevel goes typically -8 -16 (2 steps) or -4 -12 -20 (3 steps)
        // this "maximum jump height" is therefore set to 8

        Position currentPosition = startTile.getPosition();

        switch (direction)
        {
            case 0: // north
                if (isBlocked(startTile, (int)TilePart.O_NORTHWALL, missileTarget)) return true;
                break;
            case 1: // north-east
                if (isBlocked(startTile, (int)TilePart.O_NORTHWALL, missileTarget)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileNorth + oneTileEast), (int)TilePart.O_WESTWALL, missileTarget)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileEast), (int)TilePart.O_WESTWALL, missileTarget)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileEast), (int)TilePart.O_NORTHWALL, missileTarget)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileEast), O_BIGWALL, missileTarget, (int)bigWallTypes.BIGWALLNESW)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileNorth), O_BIGWALL, missileTarget, (int)bigWallTypes.BIGWALLNESW)) return true;
                break;
            case 2: // east
                if (isBlocked(_save.getTile(currentPosition + oneTileEast), (int)TilePart.O_WESTWALL, missileTarget)) return true;
                break;
            case 3: // south-east
                if (isBlocked(_save.getTile(currentPosition + oneTileEast), (int)TilePart.O_WESTWALL, missileTarget)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileSouth), (int)TilePart.O_NORTHWALL, missileTarget)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileSouth + oneTileEast), (int)TilePart.O_NORTHWALL, missileTarget)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileSouth + oneTileEast), (int)TilePart.O_WESTWALL, missileTarget)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileEast), O_BIGWALL, missileTarget, (int)bigWallTypes.BIGWALLNWSE)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileSouth), O_BIGWALL, missileTarget, (int)bigWallTypes.BIGWALLNWSE)) return true;
                break;
            case 4: // south
                if (isBlocked(_save.getTile(currentPosition + oneTileSouth), (int)TilePart.O_NORTHWALL, missileTarget)) return true;
                break;
            case 5: // south-west
                if (isBlocked(startTile, (int)TilePart.O_WESTWALL, missileTarget)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileSouth), (int)TilePart.O_WESTWALL, missileTarget)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileSouth), (int)TilePart.O_NORTHWALL, missileTarget)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileSouth), O_BIGWALL, missileTarget, (int)bigWallTypes.BIGWALLNESW)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileWest), O_BIGWALL, missileTarget, (int)bigWallTypes.BIGWALLNESW)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileSouth + oneTileWest), (int)TilePart.O_NORTHWALL, missileTarget)) return true;
                break;
            case 6: // west
                if (isBlocked(startTile, (int)TilePart.O_WESTWALL, missileTarget)) return true;
                break;
            case 7: // north-west
                if (isBlocked(startTile, (int)TilePart.O_WESTWALL, missileTarget)) return true;
                if (isBlocked(startTile, (int)TilePart.O_NORTHWALL, missileTarget)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileWest), (int)TilePart.O_NORTHWALL, missileTarget)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileNorth), (int)TilePart.O_WESTWALL, missileTarget)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileNorth), O_BIGWALL, missileTarget, (int)bigWallTypes.BIGWALLNWSE)) return true;
                if (isBlocked(_save.getTile(currentPosition + oneTileWest), O_BIGWALL, missileTarget, (int)bigWallTypes.BIGWALLNWSE)) return true;
                break;
        }

        return false;
    }

    /**
	 * Determines whether a certain part of a tile blocks movement.
	 * @param tile Specified tile, can be a null pointer.
	 * @param part Part of the tile.
	 * @param missileTarget Target for a missile.
	 * @return True if the movement is blocked.
	 */
    bool isBlocked(Tile tile, int part, BattleUnit missileTarget, int bigWallExclusion = -1)
    {
        if (tile == null) return true; // probably outside the map here

        if (part == O_BIGWALL)
        {
            if (tile.getMapData(TilePart.O_OBJECT) != null &&
                tile.getMapData(TilePart.O_OBJECT).getBigWall() != 0 &&
                tile.getMapData(TilePart.O_OBJECT).getBigWall() <= (int)bigWallTypes.BIGWALLNWSE &&
                tile.getMapData(TilePart.O_OBJECT).getBigWall() != bigWallExclusion)
                return true; // blocking part
            else
                return false;
        }
        if (part == (int)TilePart.O_WESTWALL)
        {
            if (tile.getMapData(TilePart.O_OBJECT) != null &&
                (tile.getMapData(TilePart.O_OBJECT).getBigWall() == (int)bigWallTypes.BIGWALLWEST ||
                tile.getMapData(TilePart.O_OBJECT).getBigWall() == (int)bigWallTypes.BIGWALLWESTANDNORTH))
                return true; // blocking part
            Tile tileWest = _save.getTile(tile.getPosition() + new Position(-1, 0, 0));
            if (tileWest == null) return true;  // do not look outside of map
            if (tileWest.getMapData(TilePart.O_OBJECT) != null &&
                (tileWest.getMapData(TilePart.O_OBJECT).getBigWall() == (int)bigWallTypes.BIGWALLEAST ||
                tileWest.getMapData(TilePart.O_OBJECT).getBigWall() == (int)bigWallTypes.BIGWALLEASTANDSOUTH))
                return true; // blocking part
        }
        if (part == (int)TilePart.O_NORTHWALL)
        {
            if (tile.getMapData(TilePart.O_OBJECT) != null &&
                (tile.getMapData(TilePart.O_OBJECT).getBigWall() == (int)bigWallTypes.BIGWALLNORTH ||
                tile.getMapData(TilePart.O_OBJECT).getBigWall() == (int)bigWallTypes.BIGWALLWESTANDNORTH))
                return true; // blocking part
            Tile tileNorth = _save.getTile(tile.getPosition() + new Position(0, -1, 0));
            if (tileNorth == null) return true; // do not look outside of map
            if (tileNorth.getMapData(TilePart.O_OBJECT) != null &&
                (tileNorth.getMapData(TilePart.O_OBJECT).getBigWall() == (int)bigWallTypes.BIGWALLSOUTH ||
                tileNorth.getMapData(TilePart.O_OBJECT).getBigWall() == (int)bigWallTypes.BIGWALLEASTANDSOUTH))
                return true; // blocking part
        }
        if (part == (int)TilePart.O_FLOOR)
        {
            if (tile.getUnit() != null)
            {
                BattleUnit unit = tile.getUnit();
                if (unit == _unit || unit == missileTarget || unit.isOut()) return false;
                if (missileTarget != null && unit != missileTarget && unit.getFaction() == UnitFaction.FACTION_HOSTILE)
                    return true;            // AI pathfinding with missiles shouldn't path through their own units
                if (_unit != null)
                {
                    if (_unit.getFaction() == UnitFaction.FACTION_PLAYER && unit.getVisible()) return true;     // player know all visible units
                    if (_unit.getFaction() == unit.getFaction()) return true;
                    if (_unit.getFaction() == UnitFaction.FACTION_HOSTILE &&
                        _unit.getUnitsSpottedThisTurn().Contains(unit)) return true;
                }
            }
            else if (tile.hasNoFloor(null) && _movementType != MovementType.MT_FLY) // this whole section is devoted to making large units not take part in any kind of falling behaviour
            {
                Position pos = tile.getPosition();
                while (pos.z >= 0)
                {
                    Tile t = _save.getTile(pos);
                    BattleUnit unit = t.getUnit();

                    if (unit != null && unit != _unit)
                    {
                        // don't let large units fall on other units
                        if (_unit != null && _unit.getArmor().getSize() > 1)
                        {
                            return true;
                        }
                        // don't let any units fall on large units
                        if (unit != _unit && unit != missileTarget && !unit.isOut() && unit.getArmor().getSize() > 1)
                        {
                            return true;
                        }
                    }
                    // not gonna fall any further, so we can stop checking.
                    if (!t.hasNoFloor(null))
                    {
                        break;
                    }
                    pos.z--;
                }
            }
        }
        // missiles can't pathfind through closed doors.
        {
            TilePart tp = (TilePart)part;
            if (missileTarget != null && tile.getMapData(tp) != null &&
                (tile.getMapData(tp).isDoor() ||
                (tile.getMapData(tp).isUFODoor() &&
                !tile.isUfoDoorOpen(tp))))
            {
                return true;
            }
        }
        if (tile.getTUCost(part, _movementType) == 255) return true; // blocking part
        return false;
    }

    /**
	 * Determines whether a unit can fall down from this tile.
	 * We can fall down here, if the tile does not exist, the tile has no floor
	 * the current position is higher than 0, if there is no unit standing below us.
	 * @param here The current tile.
	 * @return True if a unit can fall down.
	 */
    bool canFallDown(Tile here)
    {
        if (here.getPosition().z == 0)
            return false;
        Tile tileBelow = _save.getTile(here.getPosition() - new Position(0, 0, 1));

        return here.hasNoFloor(tileBelow);
    }

    /**
	 * Determines whether a unit can fall down from this tile.
	 * We can fall down here, if the tile does not exist, the tile has no floor
	 * the current position is higher than 0, if there is no unit standing below us.
	 * @param here The current tile.
	 * @param size The size of the unit.
	 * @return True if a unit can fall down.
	 */
    bool canFallDown(Tile here, int size)
    {
        for (int x = 0; x != size; ++x)
        {
            for (int y = 0; y != size; ++y)
            {
                Position checkPos = here.getPosition() + new Position(x, y, 0);
                Tile checkTile = _save.getTile(checkPos);
                if (!canFallDown(checkTile))
                    return false;
            }
        }
        return true;
    }

    /**
     * Converts direction to a vector. Direction starts north = 0 and goes clockwise.
     * @param direction Source direction.
     * @param vector Pointer to a position (which acts as a vector).
     */
    internal static void directionToVector(int direction, out Position vector)
    {
        int[] x = { 0, 1, 1, 1, 0, -1, -1, -1, 0, 0 };
        int[] y = { -1, -1, 0, 1, 1, 1, 0, -1, 0, 0 };
        int[] z = { 0, 0, 0, 0, 0, 0, 0, 0, 1, -1 };
        vector = new Position
        {
            x = x[direction],
            y = y[direction],
            z = z[direction]
        };
    }

    /**
	 * Checks, for the up/down button, if the movement is valid. Either there is a grav lift or the unit can fly and there are no obstructions.
	 * @param bu Pointer to unit.
	 * @param startPosition Unit starting position.
	 * @param direction Up or Down
	 * @return bool Whether it's valid.
	 */
    bool validateUpDown(BattleUnit bu, Position startPosition, int direction, bool missile)
    {
        directionToVector(direction, out var endPosition);
        endPosition += startPosition;
        Tile startTile = _save.getTile(startPosition);
        Tile belowStart = _save.getTile(startPosition + new Position(0, 0, -1));
        Tile destinationTile = _save.getTile(endPosition);
        if (startTile.getMapData(TilePart.O_FLOOR) != null && destinationTile != null && destinationTile.getMapData(TilePart.O_FLOOR) != null &&
            (startTile.getMapData(TilePart.O_FLOOR).isGravLift() && destinationTile.getMapData(TilePart.O_FLOOR).isGravLift()))
        {
            if (missile)
            {
                if (direction == DIR_UP)
                {
                    if (destinationTile.getMapData(TilePart.O_FLOOR).getLoftID(0) != 0)
                        return false;
                }
                else if (startTile.getMapData(TilePart.O_FLOOR).getLoftID(0) != 0)
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            if (bu.getMovementType() == MovementType.MT_FLY)
            {
                if ((direction == DIR_UP && destinationTile != null && destinationTile.hasNoFloor(startTile)) // flying up only possible when there is no roof
                    || (direction == DIR_DOWN && destinationTile != null && startTile.hasNoFloor(belowStart)) // falling down only possible when there is no floor
                    )
                {
                    return true;
                }
            }
        }

        return false;
    }

    /**
     * Converts direction to a vector. Direction starts north = 0 and goes clockwise.
     * @param vector Pointer to a position (which acts as a vector).
     * @param dir Resulting direction.
     */
    internal static void vectorToDirection(Position vector, out int dir)
    {
        dir = -1;
        int[] x = { 0, 1, 1, 1, 0, -1, -1, -1 };
        int[] y = { -1, -1, 0, 1, 1, 1, 0, -1 };
        for (int i = 0; i < 8; ++i)
        {
            if (x[i] == vector.x && y[i] == vector.y)
            {
                dir = i;
                return;
            }
        }
    }

    /**
     * Sets _unit in order to abuse low-level pathfinding functions from outside the class.
     * @param unit Unit taking the path.
     */
    internal void setUnit(BattleUnit unit)
    {
        _unit = unit;
        if (unit != null)
        {
            _movementType = unit.getMovementType();
        }
        else
        {
            _movementType = MovementType.MT_WALK;
        }
    }
}
