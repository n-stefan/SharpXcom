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

/**
 * A utility class that modifies tile properties on a battlescape map. This includes lighting, destruction, smoke, fire, fog of war.
 * Note that this function does not handle any sounds or animations.
 */
internal class TileEngine
{
    const int MAX_VIEW_DISTANCE = 20;
    const int MAX_VIEW_DISTANCE_SQR = MAX_VIEW_DISTANCE * MAX_VIEW_DISTANCE;
    const int MAX_VOXEL_VIEW_DISTANCE = MAX_VIEW_DISTANCE * 16;
    internal const int MAX_DARKNESS_TO_SEE_UNITS = 9;

    static int[] heightFromCenter = { 0, -2, +2, -4, +4, -6, +6, -8, +8, -12, +12 };
    SavedBattleGame _save;
    List<ushort> _voxelData;
    bool _personalLighting;
    Tile _cacheTile;
    Tile _cacheTileBelow;
    Position _cacheTilePos;

    /**
     * Sets up a TileEngine.
     * @param save Pointer to SavedBattleGame object.
     * @param voxelData List of voxel data.
     */
    internal TileEngine(SavedBattleGame save, List<ushort> voxelData)
    {
        _save = save;
        _voxelData = voxelData;
        _personalLighting = true;
        _cacheTile = null;
        _cacheTileBelow = null;

        _cacheTilePos = new Position(-1, -1, -1);
    }

    /**
     * Deletes the TileEngine.
     */
    ~TileEngine() { }

    /**
     * Calculates the distance squared between 2 points. No sqrt(), not floating point math, and sometimes it's all you need.
     * @param pos1 Position of first square.
     * @param pos2 Position of second square.
     * @param considerZ Whether to consider the z coordinate.
     * @return Distance.
     */
    internal int distanceSq(Position pos1, Position pos2, bool considerZ = true)
    {
	    int x = pos1.x - pos2.x;
	    int y = pos1.y - pos2.y;
	    int z = considerZ ? (pos1.z - pos2.z) : 0;
        return x * x + y * y + z * z;
    }

	/**
	 * Drops an item to the floor and affects it with gravity.
	 * @param position Position to spawn the item.
	 * @param item Pointer to the item.
	 * @param newItem Bool whether this is a new item.
	 * @param removeItem Bool whether to remove the item from the owner.
	 */
	internal void itemDrop(Tile t, BattleItem item, Mod.Mod mod, bool newItem = false, bool removeItem = false)
	{
		// don't spawn anything outside of bounds
		if (t == null)
			return;

		Position p = t.getPosition();

		// don't ever drop fixed items
		if (item.getRules().isFixed())
			return;

		t.addItem(item, mod.getInventory("STR_GROUND", true));

		if (item.getUnit() != null)
		{
			item.getUnit().setPosition(p);
		}

		if (newItem)
		{
			_save.getItems().Add(item);
		}
		else if (_save.getSide() != UnitFaction.FACTION_PLAYER)
		{
			item.setTurnFlag(true);
		}

		if (removeItem)
		{
			item.moveToOwner(null);
		}
		else if (item.getRules().getBattleType() != BattleType.BT_GRENADE && item.getRules().getBattleType() != BattleType.BT_PROXIMITYGRENADE)
		{
			item.setOwner(null);
		}

		applyGravity(_save.getTile(p));

		if (item.getRules().getBattleType() == BattleType.BT_FLARE)
		{
			calculateTerrainLighting();
            calculateFOV(p);
		}
	}

    /**
      * Recalculates lighting for the terrain: objects,items,fire.
      */
    internal void calculateTerrainLighting()
    {
        const int layer = 1; // Static lighting layer.
        const int fireLightPower = 15; // amount of light a fire generates

        // reset all light to 0 first
        for (int i = 0; i < _save.getMapSizeXYZ(); ++i)
        {
            _save.getTiles()[i].resetLight(layer);
        }

        // add lighting of terrain
        for (int i = 0; i < _save.getMapSizeXYZ(); ++i)
        {
            // only floors and objects can light up
            if (_save.getTiles()[i].getMapData(TilePart.O_FLOOR) != null
                && _save.getTiles()[i].getMapData(TilePart.O_FLOOR).getLightSource() != null)
            {
                addLight(_save.getTiles()[i].getPosition(), _save.getTiles()[i].getMapData(TilePart.O_FLOOR).getLightSource(), layer);
            }
            if (_save.getTiles()[i].getMapData(TilePart.O_OBJECT) != null
                && _save.getTiles()[i].getMapData(TilePart.O_OBJECT).getLightSource() != null)
            {
                addLight(_save.getTiles()[i].getPosition(), _save.getTiles()[i].getMapData(TilePart.O_OBJECT).getLightSource(), layer);
            }

            // fires
            if (_save.getTiles()[i].getFire() != 0)
            {
                addLight(_save.getTiles()[i].getPosition(), fireLightPower, layer);
            }

            foreach (var item in _save.getTiles()[i].getInventory())
            {
                if (item.getRules().getBattleType() == BattleType.BT_FLARE)
                {
                    addLight(_save.getTiles()[i].getPosition(), item.getRules().getPower(), layer);
                }
            }
        }
    }

    /**
     * Adds circular light pattern starting from center and losing power with distance travelled.
     * @param center Center.
     * @param power Power.
     * @param layer Light is separated in 3 layers: Ambient, Static and Dynamic.
     */
    void addLight(Position center, int power, int layer)
    {
        // only loop through the positive quadrant.
        for (int x = 0; x <= power; ++x)
        {
            for (int y = 0; y <= power; ++y)
            {
                for (int z = 0; z < _save.getMapSizeZ(); z++)
                {
                    int distance = (int)Math.Round(Math.Sqrt((float)(x * x + y * y)));

                    if (_save.getTile(new Position(center.x + x, center.y + y, z)) != null)
                        _save.getTile(new Position(center.x + x, center.y + y, z)).addLight(power - distance, layer);

                    if (_save.getTile(new Position(center.x - x, center.y - y, z)) != null)
                        _save.getTile(new Position(center.x - x, center.y - y, z)).addLight(power - distance, layer);

                    if (_save.getTile(new Position(center.x - x, center.y + y, z)) != null)
                        _save.getTile(new Position(center.x - x, center.y + y, z)).addLight(power - distance, layer);

                    if (_save.getTile(new Position(center.x + x, center.y - y, z)) != null)
                        _save.getTile(new Position(center.x + x, center.y - y, z)).addLight(power - distance, layer);
                }
            }
        }
    }

    /**
     * Applies gravity to a tile. Causes items and units to drop.
     * @param t Tile.
     * @return Tile where the items end up in eventually.
     */
    internal Tile applyGravity(Tile t)
    {
        if (t == null || (!t.getInventory().Any() && t.getUnit() == null)) return t; // skip this if there are no items

        Position p = t.getPosition();
        Tile rt = t;
        Tile rtb;
        BattleUnit occupant = t.getUnit();

        if (occupant != null)
        {
            Position unitpos = occupant.getPosition();
            while (unitpos.z >= 0)
            {
                bool canFall = true;
                for (int y = 0; y < occupant.getArmor().getSize() && canFall; ++y)
                {
                    for (int x = 0; x < occupant.getArmor().getSize() && canFall; ++x)
                    {
                        rt = _save.getTile(new Position(unitpos.x + x, unitpos.y + y, unitpos.z));
                        rtb = _save.getTile(new Position(unitpos.x + x, unitpos.y + y, unitpos.z - 1)); //below
                        if (!rt.hasNoFloor(rtb))
                        {
                            canFall = false;
                        }
                    }
                }
                if (!canFall)
                    break;
                unitpos.z--;
            }
            if (unitpos != occupant.getPosition())
            {
                if (occupant.getHealth() != 0 && occupant.getStunlevel() < occupant.getHealth())
                {
                    if (occupant.getMovementType() == MovementType.MT_FLY)
                    {
                        // move to the position you're already in. this will unset the kneeling flag, set the floating flag, etc.
                        occupant.startWalking(occupant.getDirection(), occupant.getPosition(), _save.getTile(occupant.getPosition() + new Position(0, 0, -1)), true);
                        // and set our status to standing (rather than walking or flying) to avoid weirdness.
                        occupant.abortTurn();
                    }
                    else
                    {
                        occupant.setPosition(occupant.getPosition()); // this is necessary to set the unit up for falling correctly, updating their "lastPos"
                        _save.addFallingUnit(occupant);
                    }
                }
                else if (occupant.isOut())
                {
                    Position origin = occupant.getPosition();
                    for (int y = occupant.getArmor().getSize() - 1; y >= 0; --y)
                    {
                        for (int x = occupant.getArmor().getSize() - 1; x >= 0; --x)
                        {
                            _save.getTile(origin + new Position(x, y, 0)).setUnit(null);
                        }
                    }
                    occupant.setPosition(unitpos);
                }
            }
        }
        rt = t;
        bool canFall2 = true;
        while (p.z >= 0 && canFall2)
        {
            rt = _save.getTile(p);
            rtb = _save.getTile(new Position(p.x, p.y, p.z - 1)); //below
            if (!rt.hasNoFloor(rtb))
                canFall2 = false;
            p.z--;
        }

        foreach (var item in t.getInventory())
        {
            if (item.getUnit() != null && t.getPosition() == item.getUnit().getPosition())
            {
                item.getUnit().setPosition(rt.getPosition());
            }
            if (t != rt)
            {
                rt.addItem(item, item.getSlot());
            }
        }

        if (t != rt)
        {
            // clear tile
            t.getInventory().Clear();
        }

        return rt;
    }

    /**
     * Calculates line of sight of a soldiers within range of the Position
     * (used when terrain has changed, which can reveal new parts of terrain or units).
     * @param position Position of the changed terrain.
     */
    internal void calculateFOV(Position position)
    {
        foreach (var item in _save.getUnits())
        {
            if (distanceSq(position, item.getPosition()) <= MAX_VIEW_DISTANCE_SQR)
            {
                calculateFOV(item);
            }
        }
    }

    /**
     * Calculates line of sight of a soldier.
     * @param unit Unit to check line of sight of.
     * @return True when new aliens are spotted.
     */
    internal bool calculateFOV(BattleUnit unit)
    {
        uint oldNumVisibleUnits = (uint)unit.getUnitsSpottedThisTurn().Count;
        Position center = unit.getPosition();
        var test = new Position();
        int direction;
        bool swap;
        var _trajectory = new List<Position>();
        if (Options.strafe && (unit.getTurretType() > -1))
        {
            direction = unit.getTurretDirection();
        }
        else
        {
            direction = unit.getDirection();
        }
        swap = (direction == 0 || direction == 4);
        int[] signX = { +1, +1, +1, +1, -1, -1, -1, -1 };
        int[] signY = { -1, -1, -1, +1, +1, +1, -1, -1 };
        int y1, y2;

        unit.clearVisibleUnits();
        unit.clearVisibleTiles();

        if (unit.isOut())
            return false;
        Position pos = unit.getPosition();

        if ((unit.getHeight() + unit.getFloatHeight() + -_save.getTile(unit.getPosition()).getTerrainLevel()) >= 24 + 4)
        {
            Tile tileAbove = _save.getTile(pos + new Position(0, 0, 1));
            if (tileAbove != null && tileAbove.hasNoFloor(null))
            {
                ++pos.z;
            }
        }
        for (int x = 0; x <= MAX_VIEW_DISTANCE; ++x)
        {
            if (direction % 2 != 0)
            {
                y1 = 0;
                y2 = MAX_VIEW_DISTANCE;
            }
            else
            {
                y1 = -x;
                y2 = x;
            }
            for (int y = y1; y <= y2; ++y)
            {
                for (int z = 0; z < _save.getMapSizeZ(); z++)
                {
                    int distanceSqr = x * x + y * y;
                    test.z = z;
                    if (distanceSqr <= MAX_VIEW_DISTANCE_SQR)
                    {
                        test.x = center.x + signX[direction] * (swap ? y : x);
                        test.y = center.y + signY[direction] * (swap ? x : y);
                        if (_save.getTile(test) != null)
                        {
                            BattleUnit visibleUnit = _save.getTile(test).getUnit();
                            if (visibleUnit != null && !visibleUnit.isOut() && visible(unit, _save.getTile(test)))
                            {
                                if (unit.getFaction() == UnitFaction.FACTION_PLAYER)
                                {
                                    visibleUnit.getTile().setVisible(+1);
                                    visibleUnit.setVisible(true);
                                }
                                if ((visibleUnit.getFaction() == UnitFaction.FACTION_HOSTILE && unit.getFaction() == UnitFaction.FACTION_PLAYER)
                                    || (visibleUnit.getFaction() != UnitFaction.FACTION_HOSTILE && unit.getFaction() == UnitFaction.FACTION_HOSTILE))
                                {
                                    unit.addToVisibleUnits(visibleUnit);
                                    unit.addToVisibleTiles(visibleUnit.getTile());

                                    if (unit.getFaction() == UnitFaction.FACTION_HOSTILE && visibleUnit.getFaction() != UnitFaction.FACTION_HOSTILE)
                                    {
                                        visibleUnit.setTurnsSinceSpotted(0);
                                    }
                                }
                            }

                            if (unit.getFaction() == UnitFaction.FACTION_PLAYER)
                            {
                                // this sets tiles to discovered if they are in LOS - tile visibility is not calculated in voxelspace but in tilespace
                                // large units have "4 pair of eyes"
                                int size = unit.getArmor().getSize();
                                for (int xo = 0; xo < size; xo++)
                                {
                                    for (int yo = 0; yo < size; yo++)
                                    {
                                        Position poso = pos + new Position(xo, yo, 0);
                                        _trajectory.Clear();
                                        int tst = calculateLine(poso, test, true, _trajectory, unit, false);
                                        int tsize = _trajectory.Count;
                                        if (tst > 127) --tsize; //last tile is blocked thus must be cropped
                                        for (int i = 0; i < tsize; i++)
                                        {
                                            Position posi = _trajectory[i];
                                            //mark every tile of line as visible (as in original)
                                            //this is needed because of bresenham narrow stroke.
                                            _save.getTile(posi).setVisible(+1);
                                            _save.getTile(posi).setDiscovered(true, 2);
                                            // walls to the east or south of a visible tile, we see that too
                                            Tile t = _save.getTile(new Position(posi.x + 1, posi.y, posi.z));
                                            if (t != null) t.setDiscovered(true, 0);
                                            t = _save.getTile(new Position(posi.x, posi.y + 1, posi.z));
                                            if (t != null) t.setDiscovered(true, 1);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // we only react when there are at least the same amount of visible units as before AND the checksum is different
        // this way we stop if there are the same amount of visible units, but a different unit is seen
        // or we stop if there are more visible units seen
        if (unit.getUnitsSpottedThisTurn().Count > oldNumVisibleUnits && unit.getVisibleUnits().Any())
        {
            return true;
        }

        return false;
    }

    /**
     * Checks for an opposing unit on this tile.
     * @param currentUnit The watcher.
     * @param tile The tile to check for
     * @return True if visible.
     */
    bool visible(BattleUnit currentUnit, Tile tile)
    {
        // if there is no tile or no unit, we can't see it
        if (tile == null || tile.getUnit() == null)
        {
            return false;
        }

        // aliens can see in the dark, xcom can see at a distance of 9 or less, further if there's enough light.
        if ((currentUnit.getFaction() == UnitFaction.FACTION_PLAYER &&
            distance(currentUnit.getPosition(), tile.getPosition()) > 9 &&
            tile.getShade() > MAX_DARKNESS_TO_SEE_UNITS) ||
            distance(currentUnit.getPosition(), tile.getPosition()) > MAX_VIEW_DISTANCE)
        {
            return false;
        }

        if (currentUnit.getFaction() == tile.getUnit().getFaction()) return true; // friendlies are always seen

        Position originVoxel = getSightOriginVoxel(currentUnit);

        bool unitSeen = false;
        // for large units origin voxel is in the middle

        var scanVoxel = new Position();
        var _trajectory = new List<Position>();
        unitSeen = canTargetUnit(originVoxel, tile, scanVoxel, currentUnit, false);

        if (unitSeen)
        {
            // now check if we really see it taking into account smoke tiles
            // initial smoke "density" of a smoke grenade is around 15 per tile
            // we do density/3 to get the decay of visibility
            // so in fresh smoke we should only have 4 tiles of visibility
            // this is traced in voxel space, with smoke affecting visibility every step of the way
            _trajectory.Clear();
            calculateLine(originVoxel, scanVoxel, true, _trajectory, currentUnit);
            Tile t = _save.getTile(currentUnit.getPosition());
            int visibleDistance = _trajectory.Count;
            for (int i = 0; i < _trajectory.Count; i++)
            {
                if (t != _save.getTile(new Position(_trajectory[i].x / 16, _trajectory[i].y / 16, _trajectory[i].z / 24)))
                {
                    t = _save.getTile(new Position(_trajectory[i].x / 16, _trajectory[i].y / 16, _trajectory[i].z / 24));
                }
                if (t.getFire() == 0)
                {
                    visibleDistance += t.getSmoke() / 3;
                }
                if (visibleDistance > (uint)MAX_VOXEL_VIEW_DISTANCE)
                {
                    unitSeen = false;
                    break;
                }
            }
        }
        return unitSeen;
    }

    /**
     * Calculates the distance between 2 points. Rounded up to first INT.
     * @param pos1 Position of first square.
     * @param pos2 Position of second square.
     * @return Distance.
     */
    internal int distance(Position pos1, Position pos2)
    {
	    int x = pos1.x - pos2.x;
	    int y = pos1.y - pos2.y;
        return (int)Math.Ceiling(Math.Sqrt((float)(x * x + y * y)));
    }

    /**
     * Gets the origin voxel of a unit's eyesight (from just one eye or something? Why is it x+7??
     * @param currentUnit The watcher.
     * @return Approximately an eyeball voxel.
     */
    Position getSightOriginVoxel(BattleUnit currentUnit)
    {
        // determine the origin and target voxels for the raytrace
        Position originVoxel;
        originVoxel = new Position((currentUnit.getPosition().x * 16) + 7, (currentUnit.getPosition().y * 16) + 8, currentUnit.getPosition().z * 24);
        originVoxel.z += -_save.getTile(currentUnit.getPosition()).getTerrainLevel();
        originVoxel.z += currentUnit.getHeight() + currentUnit.getFloatHeight() - 1; //one voxel lower (eye level)
        Tile tileAbove = _save.getTile(currentUnit.getPosition() + new Position(0, 0, 1));
        if (currentUnit.getArmor().getSize() > 1)
        {
            originVoxel.x += 8;
            originVoxel.y += 8;
            originVoxel.z += 1; //topmost voxel
        }
        if (originVoxel.z >= (currentUnit.getPosition().z + 1) * 24 && (tileAbove == null || tileAbove.hasNoFloor(null) == null))
        {
            while (originVoxel.z >= (currentUnit.getPosition().z + 1) * 24)
            {
                originVoxel.z--;
            }
        }

        return originVoxel;
    }

    /**
     * Checks for another unit available for targeting and what particular voxel.
     * @param originVoxel Voxel of trace origin (eye or gun's barrel).
     * @param tile The tile to check for.
     * @param scanVoxel is returned coordinate of hit.
     * @param excludeUnit is self (not to hit self).
     * @param rememberObstacles Remember obstacles for no LOF indicator?
     * @param potentialUnit is a hypothetical unit to draw a virtual line of fire for AI. if left blank, this function behaves normally.
     * @return True if the unit can be targetted.
     */
    internal bool canTargetUnit(Position originVoxel, Tile tile, Position scanVoxel, BattleUnit excludeUnit, bool rememberObstacles, BattleUnit potentialUnit = null)
    {
        Position targetVoxel = new Position((tile.getPosition().x * 16) + 7, (tile.getPosition().y * 16) + 8, tile.getPosition().z * 24);
        var _trajectory = new List<Position>();
        bool hypothetical = potentialUnit != null;
        if (potentialUnit == null)
        {
            potentialUnit = tile.getUnit();
            if (potentialUnit == null) return false; //no unit in this tile, even if it elevated and appearing in it.
        }

        if (potentialUnit == excludeUnit) return false; //skip self

        int targetMinHeight = targetVoxel.z - tile.getTerrainLevel();
        targetMinHeight += potentialUnit.getFloatHeight();

        int targetMaxHeight = targetMinHeight;
        int targetCenterHeight;
        // if there is an other unit on target tile, we assume we want to check against this unit's height
        int heightRange;

        int unitRadius = potentialUnit.getLoftemps(); //width == loft in default loftemps set
        int targetSize = potentialUnit.getArmor().getSize() - 1;
        int xOffset = potentialUnit.getPosition().x - tile.getPosition().x;
        int yOffset = potentialUnit.getPosition().y - tile.getPosition().y;
        if (targetSize > 0)
        {
            unitRadius = 3;
        }
        // vector manipulation to make scan work in view-space
        Position relPos = targetVoxel - originVoxel;
        float normal = (float)(unitRadius / Math.Sqrt((float)(relPos.x * relPos.x + relPos.y * relPos.y)));
        int relX = (int)Math.Floor(((float)relPos.y) * normal + 0.5);
        int relY = (int)Math.Floor(((float)-relPos.x) * normal + 0.5);

        int[] sliceTargets = { 0, 0, relX, relY, -relX, -relY, relY, -relX, -relY, relX };

        if (!potentialUnit.isOut())
        {
            heightRange = potentialUnit.getHeight();
        }
        else
        {
            heightRange = 12;
        }

        targetMaxHeight += heightRange;
        targetCenterHeight = (targetMaxHeight + targetMinHeight) / 2;
        heightRange /= 2;
        if (heightRange > 10) heightRange = 10;
        if (heightRange <= 0) heightRange = 0;

        // scan ray from top to bottom  plus different parts of target cylinder
        for (int i = 0; i <= heightRange; ++i)
        {
            scanVoxel.z = targetCenterHeight + heightFromCenter[i];
            for (int j = 0; j < 5; ++j)
            {
                if (i < (heightRange - 1) && j > 2) break; //skip unnecessary checks
                scanVoxel.x = targetVoxel.x + sliceTargets[j * 2];
                scanVoxel.y = targetVoxel.y + sliceTargets[j * 2 + 1];
                _trajectory.Clear();
                int test = calculateLine(originVoxel, scanVoxel, false, _trajectory, excludeUnit, true, false);
                if (test == (int)VoxelType.V_UNIT)
                {
                    for (int x = 0; x <= targetSize; ++x)
                    {
                        for (int y = 0; y <= targetSize; ++y)
                        {
                            //voxel of hit must be inside of scanned box
                            if (_trajectory[0].x / 16 == (scanVoxel.x / 16) + x + xOffset &&
                                _trajectory[0].y / 16 == (scanVoxel.y / 16) + y + yOffset &&
                                _trajectory[0].z >= targetMinHeight &&
                                _trajectory[0].z <= targetMaxHeight)
                            {
                                return true;
                            }
                        }
                    }
                }
                else if (test == (int)VoxelType.V_EMPTY && hypothetical && _trajectory.Any())
                {
                    return true;
                }
                if (rememberObstacles && _trajectory.Count > 0)
                {
                    Tile tileObstacle = _save.getTile(new Position(_trajectory[0].x / 16, _trajectory[0].y / 16, _trajectory[0].z / 24));
                    if (tileObstacle != null) tileObstacle.setObstacle(test);
                }
            }
        }
        return false;
    }

    /**
     * Calculates a line trajectory, using bresenham algorithm in 3D.
     * @param origin Origin (voxel??).
     * @param target Target (also voxel??).
     * @param storeTrajectory True will store the whole trajectory - otherwise it just stores the last position.
     * @param trajectory A vector of positions in which the trajectory is stored.
     * @param excludeUnit Excludes this unit in the collision detection.
     * @param doVoxelCheck Check against voxel or tile blocking? (first one for units visibility and line of fire, second one for terrain visibility).
     * @param onlyVisible Skip invisible units? used in FPS view.
     * @param excludeAllBut [Optional] The only unit to be considered for ray hits.
     * @return the objectnumber(0-3) or unit(4) or out of map (5) or -1(hit nothing).
     */
    internal int calculateLine(Position origin, Position target, bool storeTrajectory, List<Position> trajectory, BattleUnit excludeUnit, bool doVoxelCheck = true, bool onlyVisible = false, BattleUnit excludeAllBut = null)
    {
        int x, x0, x1, delta_x, step_x;
        int y, y0, y1, delta_y, step_y;
        int z, z0, z1, delta_z, step_z;
        bool swap_xy, swap_xz;
        int drift_xy, drift_xz;
        int cx, cy, cz;
        Position lastPoint = origin;
        int result;
        int steps = 0;
        bool excludeAllUnits = false;
        if (_save.isBeforeGame())
        {
            excludeAllUnits = true; // don't start unit spotting before pre-game inventory stuff (large units on the craftInventory tile will cause a crash if they're "spotted")
        }

        //start and end points
        x0 = origin.x; x1 = target.x;
        y0 = origin.y; y1 = target.y;
        z0 = origin.z; z1 = target.z;

        //'steep' xy Line, make longest delta x plane
        swap_xy = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (swap_xy)
        {
            (y0, x0) = (x0, y0);
            (y1, x1) = (x1, y1);
        }

        //do same for xz
        swap_xz = Math.Abs(z1 - z0) > Math.Abs(x1 - x0);
        if (swap_xz)
        {
            (z0, x0) = (x0, z0);
            (z1, x1) = (x1, z1);
        }

        //delta is Length in each plane
        delta_x = Math.Abs(x1 - x0);
        delta_y = Math.Abs(y1 - y0);
        delta_z = Math.Abs(z1 - z0);

        //drift controls when to step in 'shallow' planes
        //starting value keeps Line centred
        drift_xy = (delta_x / 2);
        drift_xz = (delta_x / 2);

        //direction of line
        step_x = 1; if (x0 > x1) { step_x = -1; }
        step_y = 1; if (y0 > y1) { step_y = -1; }
        step_z = 1; if (z0 > z1) { step_z = -1; }

        //starting point
        y = y0;
        z = z0;

        if (doVoxelCheck) voxelCheckFlush();

        //step through longest delta (which we have swapped to x)
        for (x = x0; ; x += step_x)
        {
            //copy position
            cx = x; cy = y; cz = z;

            //unswap (in reverse)
            if (swap_xz) (cz, cx) = (cx, cz);
            if (swap_xy) (cy, cx) = (cx, cy);

            if (storeTrajectory != null && trajectory != null)
            {
                trajectory.Add(new Position(cx, cy, cz));
            }
            //passes through this point?
            if (doVoxelCheck)
            {
                result = (int)voxelCheck(new Position(cx, cy, cz), excludeUnit, false, onlyVisible, excludeAllBut);
                if (result != (int)VoxelType.V_EMPTY)
                {
                    if (trajectory != null)
                    { // store the position of impact
                        trajectory.Add(new Position(cx, cy, cz));
                    }
                    return result;
                }
            }
            else
            {
                int temp_res = verticalBlockage(_save.getTile(lastPoint), _save.getTile(new Position(cx, cy, cz)), ItemDamageType.DT_NONE);
                result = horizontalBlockage(_save.getTile(lastPoint), _save.getTile(new Position(cx, cy, cz)), ItemDamageType.DT_NONE, steps < 2);
                steps++;
                if (result == -1)
                {
                    if (temp_res > 127)
                    {
                        result = 0;
                    }
                    else
                    {
                        return result; // We hit a big wall
                    }
                }
                result += temp_res;
                if (result > 127)
                {
                    return result;
                }

                lastPoint = new Position(cx, cy, cz);
            }

            if (x == x1) break;

            //update progress in other planes
            drift_xy = drift_xy - delta_y;
            drift_xz = drift_xz - delta_z;

            //step in y plane
            if (drift_xy < 0)
            {
                y = y + step_y;
                drift_xy = drift_xy + delta_x;

                //check for xy diagonal intermediate voxel step
                if (doVoxelCheck)
                {
                    cx = x; cz = z; cy = y;
                    if (swap_xz) (cz, cx) = (cx, cz);
                    if (swap_xy) (cy, cx) = (cx, cy);
                    result = (int)voxelCheck(new Position(cx, cy, cz), excludeUnit, excludeAllUnits, onlyVisible, excludeAllBut);
                    if (result != (int)VoxelType.V_EMPTY)
                    {
                        if (trajectory != null)
                        { // store the position of impact
                            trajectory.Add(new Position(cx, cy, cz));
                        }
                        return result;
                    }
                }
            }

            //same in z
            if (drift_xz < 0)
            {
                z = z + step_z;
                drift_xz = drift_xz + delta_x;

                //check for xz diagonal intermediate voxel step
                if (doVoxelCheck)
                {
                    cx = x; cz = z; cy = y;
                    if (swap_xz) (cz, cx) = (cx, cz);
                    if (swap_xy) (cy, cx) = (cx, cy);
                    result = (int)voxelCheck(new Position(cx, cy, cz), excludeUnit, excludeAllUnits, onlyVisible, excludeAllBut);
                    if (result != (int)VoxelType.V_EMPTY)
                    {
                        if (trajectory != null)
                        { // store the position of impact
                            trajectory.Add(new Position(cx, cy, cz));
                        }
                        return result;
                    }
                }
            }
        }

        return (int)VoxelType.V_EMPTY;
    }

    /**
     * Calculates the amount this certain wall or floor-part of the tile blocks.
     * @param startTile The tile where the power starts.
     * @param part The part of the tile the power needs to go through.
     * @param type The type of power/damage.
     * @param direction Direction the power travels.
     * @return Amount of blockage.
     */
    int blockage(Tile tile, TilePart part, ItemDamageType type, int direction = -1, bool checkingFromOrigin = false)
    {
	    int blockage = 0;

	    if (tile == null) return 0; // probably outside the map here
	    if (tile.getMapData(part) != null)
	    {
		    bool check = true;
            bigWallTypes wall = (bigWallTypes)(-1);
		    if (direction != -1)
		    {
			    wall = (bigWallTypes)tile.getMapData(TilePart.O_OBJECT).getBigWall();

			    if (type != ItemDamageType.DT_SMOKE &&
				    checkingFromOrigin &&
				    (wall == bigWallTypes.BIGWALLNESW ||
				    wall == bigWallTypes.BIGWALLNWSE))
			    {
				    check = false;
			    }
			    switch (direction)
			    {
			    case 0: // north
				    if (wall == bigWallTypes.BIGWALLWEST ||
					    wall == bigWallTypes.BIGWALLEAST ||
					    wall == bigWallTypes.BIGWALLSOUTH ||
					    wall == bigWallTypes.BIGWALLEASTANDSOUTH)
				    {
					    check = false;
				    }
				    break;
			    case 1: // north east
				    if (wall == bigWallTypes.BIGWALLWEST ||
					    wall == bigWallTypes.BIGWALLSOUTH)
				    {
					    check = false;
				    }
				    break;
			    case 2: // east
				    if (wall == bigWallTypes.BIGWALLNORTH ||
					    wall == bigWallTypes.BIGWALLSOUTH ||
					    wall == bigWallTypes.BIGWALLWEST ||
					    wall == bigWallTypes.BIGWALLWESTANDNORTH)
				    {
					    check = false;
				    }
				    break;
			    case 3: // south east
				    if (wall == bigWallTypes.BIGWALLNORTH ||
					    wall == bigWallTypes.BIGWALLWEST ||
					    wall == bigWallTypes.BIGWALLWESTANDNORTH)
				    {
					    check = false;
				    }
				    break;
			    case 4: // south
				    if (wall == bigWallTypes.BIGWALLWEST ||
					    wall == bigWallTypes.BIGWALLEAST ||
					    wall == bigWallTypes.BIGWALLNORTH ||
					    wall == bigWallTypes.BIGWALLWESTANDNORTH)
				    {
					    check = false;
				    }
				    break;
			    case 5: // south west
				    if (wall == bigWallTypes.BIGWALLNORTH ||
					    wall == bigWallTypes.BIGWALLEAST)
				    {
					    check = false;
				    }
				    break;
			    case 6: // west
				    if (wall == bigWallTypes.BIGWALLNORTH ||
					    wall == bigWallTypes.BIGWALLSOUTH ||
					    wall == bigWallTypes.BIGWALLEAST ||
					    wall == bigWallTypes.BIGWALLEASTANDSOUTH)
				    {
					    check = false;
				    }
				    break;
			    case 7: // north west
				    if (wall == bigWallTypes.BIGWALLSOUTH ||
					    wall == bigWallTypes.BIGWALLEAST ||
					    wall == bigWallTypes.BIGWALLEASTANDSOUTH)
				    {
					    check = false;
				    }
				    break;
			    case 8: // up
			    case 9: // down
				    if (wall != 0 && wall != bigWallTypes.BLOCK)
				    {
					    check = false;
				    }
				    break;
			    default:
				    break;
			    }
		    }
		    else if (part == TilePart.O_FLOOR &&
					    tile.getMapData(part).getBlock(type) == 0)
		    {
			    if (type != ItemDamageType.DT_NONE)
			    {
				    blockage += tile.getMapData(part).getArmor();
			    }
			    else if (!tile.getMapData(part).isNoFloor())
			    {
				    return 256;
			    }
		    }

		    if (check)
		    {
			    // -1 means we have a regular wall, and anything over 0 means we have a bigwall.
			    if (type == ItemDamageType.DT_SMOKE && wall != 0 && !tile.isUfoDoorOpen(part))
			    {
				    return 256;
			    }
			    blockage += tile.getMapData(part).getBlock(type);
		    }
	    }

	    // open ufo doors are actually still closed behind the scenes
	    // so a special trick is needed to see if they are open, if they are, they obviously don't block anything
	    if (tile.isUfoDoorOpen(part))
		    blockage = 0;

	    return blockage;
    }

    /**
     * Calculates the amount of power that is blocked going from one tile to another on a different level.
     * @param startTile The tile where the power starts.
     * @param endTile The adjacent tile where the power ends.
     * @param type The type of power/damage.
     * @return Amount of blockage of this power.
     */
    int verticalBlockage(Tile startTile, Tile endTile, ItemDamageType type, bool skipObject = false)
    {
        int block = 0;

        // safety check
        if (startTile == null || endTile == null) return 0;
        int direction = endTile.getPosition().z - startTile.getPosition().z;

        if (direction == 0) return 0;

        int x = startTile.getPosition().x;
        int y = startTile.getPosition().y;
        int z = startTile.getPosition().z;

        if (direction < 0) // down
        {
            block += blockage(startTile, TilePart.O_FLOOR, type);
            if (!skipObject)
                block += blockage(startTile, TilePart.O_OBJECT, type, Pathfinding.DIR_DOWN);
            if (x != endTile.getPosition().x || y != endTile.getPosition().y)
            {
                x = endTile.getPosition().x;
                y = endTile.getPosition().y;
                // z remains same as startTile
                Tile currTile = _save.getTile(new Position(x, y, z));
                block += horizontalBlockage(startTile, currTile, type, skipObject);
                block += blockage(currTile, TilePart.O_FLOOR, type);
                if (!skipObject)
                    block += blockage(currTile, TilePart.O_OBJECT, type, Pathfinding.DIR_DOWN);
            }
        }
        else if (direction > 0) // up
        {
            z += 1;
            Tile currTile = _save.getTile(new Position(x, y, z));
            block += blockage(currTile, TilePart.O_FLOOR, type);
            if (!skipObject)
                block += blockage(currTile, TilePart.O_OBJECT, type, Pathfinding.DIR_UP);
            if (x != endTile.getPosition().x || y != endTile.getPosition().y)
            {
                x = endTile.getPosition().x;
                y = endTile.getPosition().y;
                currTile = _save.getTile(new Position(x, y, z));
                block += horizontalBlockage(startTile, currTile, type, skipObject);
                block += blockage(currTile, TilePart.O_FLOOR, type);
                if (!skipObject)
                    block += blockage(currTile, TilePart.O_OBJECT, type, Pathfinding.DIR_UP);
            }
        }

        return block;
    }

    static Position oneTileNorth = new Position(0, -1, 0);
    static Position oneTileEast = new Position(1, 0, 0);
    static Position oneTileSouth = new Position(0, 1, 0);
    static Position oneTileWest = new Position(-1, 0, 0);
    /**
     * Calculates the amount of power that is blocked going from one tile to another on the same level.
     * @param startTile The tile where the power starts.
     * @param endTile The adjacent tile where the power ends.
     * @param type The type of power/damage.
     * @return Amount of blockage.
     */
    internal int horizontalBlockage(Tile startTile, Tile endTile, ItemDamageType type, bool skipObject = false)
    {
        // safety check
        if (startTile == null || endTile == null) return 0;
        if (startTile.getPosition().z != endTile.getPosition().z) return 0;
        Tile tmpTile;

        int direction;
        Pathfinding.vectorToDirection(endTile.getPosition() - startTile.getPosition(), out direction);
        if (direction == -1) return 0;
        int block = 0;

        switch (direction)
        {
            case 0: // north
                block = blockage(startTile, TilePart.O_NORTHWALL, type);
                break;
            case 1: // north east
                if (type == ItemDamageType.DT_NONE) //this is two-way diagonal visibility check, used in original game
                {
                    block = blockage(startTile, TilePart.O_NORTHWALL, type) + blockage(endTile, TilePart.O_WESTWALL, type); //up+right
                    tmpTile = _save.getTile(startTile.getPosition() + oneTileNorth);
                    if (tmpTile != null && tmpTile.getMapData(TilePart.O_OBJECT) != null && tmpTile.getMapData(TilePart.O_OBJECT).getBigWall() != (int)bigWallTypes.BIGWALLNESW)
                        block += blockage(tmpTile, TilePart.O_OBJECT, type, 3);
                    if (block == 0) break; //this way is opened
                    block = blockage(_save.getTile(startTile.getPosition() + oneTileEast), TilePart.O_NORTHWALL, type)
                        + blockage(_save.getTile(startTile.getPosition() + oneTileEast), TilePart.O_WESTWALL, type); //right+up
                    tmpTile = _save.getTile(startTile.getPosition() + oneTileEast);
                    if (tmpTile != null && tmpTile.getMapData(TilePart.O_OBJECT) != null && tmpTile.getMapData(TilePart.O_OBJECT).getBigWall() != (int)bigWallTypes.BIGWALLNESW)
                        block += blockage(tmpTile, TilePart.O_OBJECT, type, 7);
                }
                else
                {
                    block = (blockage(startTile, TilePart.O_NORTHWALL, type) + blockage(endTile, TilePart.O_WESTWALL, type)) / 2
                        + (blockage(_save.getTile(startTile.getPosition() + oneTileEast), TilePart.O_WESTWALL, type)
                        + blockage(_save.getTile(startTile.getPosition() + oneTileEast), TilePart.O_NORTHWALL, type)) / 2;

                    block += (blockage(_save.getTile(startTile.getPosition() + oneTileNorth), TilePart.O_OBJECT, type, 4)
                        + blockage(_save.getTile(startTile.getPosition() + oneTileEast), TilePart.O_OBJECT, type, 6)) / 2;
                }
                break;
            case 2: // east
                block = blockage(endTile, TilePart.O_WESTWALL, type);
                break;
            case 3: // south east
                if (type == ItemDamageType.DT_NONE)
                {
                    block = blockage(_save.getTile(startTile.getPosition() + oneTileSouth), TilePart.O_NORTHWALL, type)
                        + blockage(endTile, TilePart.O_WESTWALL, type); //down+right
                    tmpTile = _save.getTile(startTile.getPosition() + oneTileSouth);
                    if (tmpTile != null && tmpTile.getMapData(TilePart.O_OBJECT) != null && tmpTile.getMapData(TilePart.O_OBJECT).getBigWall() != (int)bigWallTypes.BIGWALLNWSE)
                        block += blockage(tmpTile, TilePart.O_OBJECT, type, 1);
                    if (block == 0) break; //this way is opened
                    block = blockage(_save.getTile(startTile.getPosition() + oneTileEast), TilePart.O_WESTWALL, type)
                        + blockage(endTile, TilePart.O_NORTHWALL, type); //right+down
                    tmpTile = _save.getTile(startTile.getPosition() + oneTileEast);
                    if (tmpTile != null && tmpTile.getMapData(TilePart.O_OBJECT) != null && tmpTile.getMapData(TilePart.O_OBJECT).getBigWall() != (int)bigWallTypes.BIGWALLNWSE)
                        block += blockage(tmpTile, TilePart.O_OBJECT, type, 5);
                }
                else
                {
                    block = (blockage(endTile, TilePart.O_WESTWALL, type) + blockage(endTile, TilePart.O_NORTHWALL, type)) / 2
                        + (blockage(_save.getTile(startTile.getPosition() + oneTileEast), TilePart.O_WESTWALL, type)
                        + blockage(_save.getTile(startTile.getPosition() + oneTileSouth), TilePart.O_NORTHWALL, type)) / 2;
                    block += (blockage(_save.getTile(startTile.getPosition() + oneTileSouth), TilePart.O_OBJECT, type, 0)
                        + blockage(_save.getTile(startTile.getPosition() + oneTileEast), TilePart.O_OBJECT, type, 6)) / 2;
                }
                break;
            case 4: // south
                block = blockage(endTile, TilePart.O_NORTHWALL, type);
                break;
            case 5: // south west
                if (type == ItemDamageType.DT_NONE)
                {
                    block = blockage(_save.getTile(startTile.getPosition() + oneTileSouth), TilePart.O_NORTHWALL, type)
                        + blockage(_save.getTile(startTile.getPosition() + oneTileSouth), TilePart.O_WESTWALL, type); //down+left
                    tmpTile = _save.getTile(startTile.getPosition() + oneTileSouth);
                    if (tmpTile != null && tmpTile.getMapData(TilePart.O_OBJECT) != null && tmpTile.getMapData(TilePart.O_OBJECT).getBigWall() != (int)bigWallTypes.BIGWALLNESW)
                        block += blockage(tmpTile, TilePart.O_OBJECT, type, 7);
                    if (block == 0) break; //this way is opened
                    block = blockage(startTile, TilePart.O_WESTWALL, type) + blockage(endTile, TilePart.O_NORTHWALL, type); //left+down
                    tmpTile = _save.getTile(startTile.getPosition() + oneTileWest);
                    if (tmpTile != null && tmpTile.getMapData(TilePart.O_OBJECT) != null && tmpTile.getMapData(TilePart.O_OBJECT).getBigWall() != (int)bigWallTypes.BIGWALLNESW)
                        block += blockage(tmpTile, TilePart.O_OBJECT, type, 3);
                }
                else
                {
                    block = (blockage(endTile, TilePart.O_NORTHWALL, type) + blockage(startTile, TilePart.O_WESTWALL, type)) / 2
                        + (blockage(_save.getTile(startTile.getPosition() + oneTileSouth), TilePart.O_WESTWALL, type)
                        + blockage(_save.getTile(startTile.getPosition() + oneTileSouth), TilePart.O_NORTHWALL, type)) / 2;
                    block += (blockage(_save.getTile(startTile.getPosition() + oneTileSouth), TilePart.O_OBJECT, type, 0)
                        + blockage(_save.getTile(startTile.getPosition() + oneTileWest), TilePart.O_OBJECT, type, 2)) / 2;
                }
                break;
            case 6: // west
                block = blockage(startTile, TilePart.O_WESTWALL, type);
                break;
            case 7: // north west

                if (type == ItemDamageType.DT_NONE)
                {
                    block = blockage(startTile, TilePart.O_NORTHWALL, type)
                        + blockage(_save.getTile(startTile.getPosition() + oneTileNorth), TilePart.O_WESTWALL, type); //up+left
                    tmpTile = _save.getTile(startTile.getPosition() + oneTileNorth);
                    if (tmpTile != null && tmpTile.getMapData(TilePart.O_OBJECT) != null && tmpTile.getMapData(TilePart.O_OBJECT).getBigWall() != (int)bigWallTypes.BIGWALLNWSE)
                        block += blockage(tmpTile, TilePart.O_OBJECT, type, 5);
                    if (block == 0) break; //this way is opened
                    block = blockage(startTile, TilePart.O_WESTWALL, type)
                        + blockage(_save.getTile(startTile.getPosition() + oneTileWest), TilePart.O_NORTHWALL, type); //left+up
                    tmpTile = _save.getTile(startTile.getPosition() + oneTileWest);
                    if (tmpTile != null && tmpTile.getMapData(TilePart.O_OBJECT) != null && tmpTile.getMapData(TilePart.O_OBJECT).getBigWall() != (int)bigWallTypes.BIGWALLNWSE)
                        block += blockage(tmpTile, TilePart.O_OBJECT, type, 1);
                }
                else
                {
                    block = (blockage(startTile, TilePart.O_WESTWALL, type) + blockage(startTile, TilePart.O_NORTHWALL, type)) / 2
                        + (blockage(_save.getTile(startTile.getPosition() + oneTileNorth), TilePart.O_WESTWALL, type)
                        + blockage(_save.getTile(startTile.getPosition() + oneTileWest), TilePart.O_NORTHWALL, type)) / 2;
                    block += (blockage(_save.getTile(startTile.getPosition() + oneTileNorth), TilePart.O_OBJECT, type, 4)
                        + blockage(_save.getTile(startTile.getPosition() + oneTileWest), TilePart.O_OBJECT, type, 2)) / 2;
                }
                break;
        }

        if (!skipObject || (type == ItemDamageType.DT_NONE && startTile.isBigWall()))
            block += blockage(startTile, TilePart.O_OBJECT, type, direction);

        if (type != ItemDamageType.DT_NONE)
        {
            // not too sure about removing this line,
            // i have a sneaking suspicion we might end up blocking things that we shouldn't

            //if (skipObject) return block;

            direction += 4;
            if (direction > 7)
                direction -= 8;
            if (endTile.isBigWall())
                block += blockage(endTile, TilePart.O_OBJECT, type, direction, true);
        }
        else
        {
            if (block <= 127)
            {
                direction += 4;
                if (direction > 7)
                    direction -= 8;
                if (blockage(endTile, TilePart.O_OBJECT, type, direction, true) > 127)
                {
                    return -1; //hit bigwall, reveal bigwall tile
                }
            }
        }

        return block;
    }

    /**
     * Checks if we hit a voxel.
     * @param voxel The voxel to check.
     * @param excludeUnit Don't do checks on this unit.
     * @param excludeAllUnits Don't do checks on any unit.
     * @param onlyVisible Whether to consider only visible units.
     * @param excludeAllBut If set, the only unit to be considered for ray hits.
     * @return The objectnumber(0-3) or unit(4) or out of map (5) or -1 (hit nothing).
     */
    VoxelType voxelCheck(Position voxel, BattleUnit excludeUnit, bool excludeAllUnits = false, bool onlyVisible = false, BattleUnit excludeAllBut = null)
    {
        if (voxel.x < 0 || voxel.y < 0 || voxel.z < 0) //preliminary out of map
        {
            return VoxelType.V_OUTOFBOUNDS;
        }
        Position pos = voxel / new Position(16, 16, 24);
        Tile tile, tileBelow;
        if (_cacheTilePos == pos)
        {
            tile = _cacheTile;
            tileBelow = _cacheTileBelow;
        }
        else
        {
            tile = _save.getTile(pos);
            if (tile == null) // check if we are not out of the map
            {
                return VoxelType.V_OUTOFBOUNDS; //not even cache
            }
            tileBelow = _save.getTile(pos + new Position(0, 0, -1));
            _cacheTilePos = pos;
            _cacheTile = tile;
            _cacheTileBelow = tileBelow;
        }

        if (tile.isVoid() && tile.getUnit() == null && (tileBelow == null || tileBelow.getUnit() == null))
        {
            return VoxelType.V_EMPTY;
        }

        if (tile.getMapData(TilePart.O_FLOOR) != null && tile.getMapData(TilePart.O_FLOOR).isGravLift() && (voxel.z % 24 == 0 || voxel.z % 24 == 1))
        {
            if ((tile.getPosition().z == 0) || (tileBelow != null && tileBelow.getMapData(TilePart.O_FLOOR) != null && !tileBelow.getMapData(TilePart.O_FLOOR).isGravLift()))
            {
                return VoxelType.V_FLOOR;
            }
        }

        // first we check terrain voxel data, not to allow 2x2 units stick through walls
        for (var i = VoxelType.V_FLOOR; i <= VoxelType.V_OBJECT; ++i)
        {
            TilePart tp = (TilePart)i;
            MapData mp = tile.getMapData(tp);
            if (((tp == TilePart.O_WESTWALL) || (tp == TilePart.O_NORTHWALL)) && tile.isUfoDoorOpen(tp))
                continue;
            if (mp != null)
            {
                int x = 15 - voxel.x % 16;
                int y = voxel.y % 16;
                int idx = (mp.getLoftID((voxel.z % 24) / 2) * 16) + y;
                if ((_voxelData[idx] & (1 << x)) != 0)
                {
                    return (VoxelType)i;
                }
            }
        }

        if (!excludeAllUnits)
        {
            BattleUnit unit = tile.getUnit();
            // sometimes there is unit on the tile below, but sticks up to this tile with his head,
            // in this case we couldn't have unit standing at current tile.
            if (unit == null && tile.hasNoFloor(tileBelow))
            {
                if (tileBelow != null)
                {
                    tile = tileBelow;
                    unit = tile.getUnit();
                }
            }
            if (unit != null && unit != excludeUnit && (excludeAllBut == null || unit == excludeAllBut) && (!onlyVisible || unit.getVisible()))
            {
                Position tilepos;
                Position unitpos = unit.getPosition();
                int terrainHeight = 0;
                for (int x = 0; x < unit.getArmor().getSize(); ++x)
                {
                    for (int y = 0; y < unit.getArmor().getSize(); ++y)
                    {
                        Tile tempTile = _save.getTile(unitpos + new Position(x, y, 0));
                        if (tempTile.getTerrainLevel() < terrainHeight)
                        {
                            terrainHeight = tempTile.getTerrainLevel();
                        }
                    }
                }
                int tz = unitpos.z * 24 + unit.getFloatHeight() - terrainHeight; //bottom most voxel, terrain heights are negative, so we subtract.
                if ((voxel.z > tz) && (voxel.z <= tz + unit.getHeight()))
                {
                    int x = voxel.x % 16;
                    int y = voxel.y % 16;
                    int part = 0;
                    if (unit.getArmor().getSize() > 1)
                    {
                        tilepos = tile.getPosition();
                        part = tilepos.x - unitpos.x + (tilepos.y - unitpos.y) * 2;
                    }
                    int idx = (unit.getLoftemps(part) * 16) + y;
                    if ((_voxelData[idx] & (1 << x)) != 0)
                    {
                        return VoxelType.V_UNIT;
                    }
                }
            }
        }
        return VoxelType.V_EMPTY;
    }

    void voxelCheckFlush()
    {
        _cacheTilePos = new Position(-1, -1, -1);
        _cacheTile = null;
        _cacheTileBelow = null;
    }

    /**
     * Recalculates FOV of all units in-game.
     */
    internal void recalculateFOV()
    {
        foreach (var unit in _save.getUnits())
        {
            if (unit.getTile() != null)
            {
                calculateFOV(unit);
            }
        }
    }

    /**
      * Calculates sun shading for the whole terrain.
      */
    internal void calculateSunShading()
    {
        const int layer = 0; // Ambient lighting layer.

        for (int i = 0; i < _save.getMapSizeXYZ(); ++i)
        {
            _save.getTiles()[i].resetLight(layer);
            calculateSunShading(_save.getTiles()[i]);
        }
    }

    /**
      * Calculates sun shading for 1 tile. Sun comes from above and is blocked by floors or objects.
      * TODO: angle the shadow according to the time? - link to Options.globeSeasons (or whatever the realistic lighting one is)
      * @param tile The tile to calculate sun shading for.
      */
    void calculateSunShading(Tile tile)
    {
        const int layer = 0; // Ambient lighting layer.

        int power = 15 - _save.getGlobalShade();

        // At night/dusk sun isn't dropping shades blocked by roofs
        if (_save.getGlobalShade() <= 4)
        {
            int block = 0;
            int x = tile.getPosition().x;
            int y = tile.getPosition().y;
            for (int z = _save.getMapSizeZ() - 1; z > tile.getPosition().z; z--)
            {
                block += blockage(_save.getTile(new Position(x, y, z)), TilePart.O_FLOOR, ItemDamageType.DT_NONE);
                block += blockage(_save.getTile(new Position(x, y, z)), TilePart.O_OBJECT, ItemDamageType.DT_NONE, Pathfinding.DIR_DOWN);
            }
            if (block > 0)
            {
                power -= 2;
            }
        }
        tile.addLight(power, layer);
    }

    /**
      * Recalculates lighting for the units.
      */
    internal void calculateUnitLighting()
    {
        const int layer = 2; // Dynamic lighting layer.
        const int personalLightPower = 15; // amount of light a unit generates
        const int fireLightPower = 15; // amount of light a fire generates

        // reset all light to 0 first
        for (int i = 0; i < _save.getMapSizeXYZ(); ++i)
        {
            _save.getTiles()[i].resetLight(layer);
        }

        foreach (var unit in _save.getUnits())
        {
            // add lighting of soldiers
            if (_personalLighting && unit.getFaction() == UnitFaction.FACTION_PLAYER && !unit.isOut())
            {
                addLight(unit.getPosition(), personalLightPower, layer);
            }
            // add lighting of units on fire
            if (unit.getFire() != 0)
            {
                addLight(unit.getPosition(), fireLightPower, layer);
            }
        }
    }

    /**
     * Checks for chained explosions.
     *
     * Chained explosions are explosions which occur after an explosive map object is destroyed.
     * May be due a direct hit, other explosion or fire.
     * @return tile on which a explosion occurred
     */
    internal Tile checkForTerrainExplosions()
    {
        for (int i = 0; i < _save.getMapSizeXYZ(); ++i)
        {
            if (_save.getTiles()[i].getExplosive() != 0)
            {
                return _save.getTiles()[i];
            }
        }
        return null;
    }

    /**
     * Handles explosions.
     *
     * HE, smoke and fire explodes in a circular pattern on 1 level only. HE however damages floor tiles of the above level. Not the units on it.
     * HE destroys an object if its armor is lower than the explosive power, then it's HE blockage is applied for further propagation.
     * See http://www.ufopaedia.org/index.php?title=Explosions for more info.
     * @param center Center of the explosion in voxelspace.
     * @param power Power of the explosion.
     * @param type The damage type of the explosion.
     * @param maxRadius The maximum radius of the explosion.
     * @param unit The unit that caused the explosion.
     */
    internal void explode(Position center, int power, ItemDamageType type, int maxRadius, BattleUnit unit = null)
    {
        double centerZ = center.z / 24 + 0.5;
        double centerX = center.x / 16 + 0.5;
        double centerY = center.y / 16 + 0.5;
        int hitSide = 0;
        int diagonalWall = 0;
        int power_;
        var tilesAffected = new HashSet<Tile>();
        //KeyValuePair<HashSet<Tile>, bool> ret;

        if (type == ItemDamageType.DT_IN)
        {
            power /= 2;
        }

        int exHeight = Math.Clamp(Options.battleExplosionHeight, 0, 3);
        int vertdec = 1000; //default flat explosion
        int dmgRng = type == ItemDamageType.DT_HE ? Mod.Mod.EXPLOSIVE_DAMAGE_RANGE : Mod.Mod.DAMAGE_RANGE;

        switch (exHeight)
        {
            case 1:
                vertdec = 30;
                break;
            case 2:
                vertdec = 10;
                break;
            case 3:
                vertdec = 5;
                break;
        }

        Tile origin = _save.getTile(new Position((int)centerX, (int)centerY, (int)centerZ));
        Tile dest;
        if (origin.isBigWall()) //precalculations for bigwall deflection
        {
            diagonalWall = origin.getMapData(TilePart.O_OBJECT).getBigWall();
            if (diagonalWall == (int)bigWallTypes.BIGWALLNWSE) //  3 |
                hitSide = (center.x % 16 - center.y % 16) > 0 ? 1 : -1;
            if (diagonalWall == (int)bigWallTypes.BIGWALLNESW) //  2 --
                hitSide = (center.x % 16 + center.y % 16 - 15) > 0 ? 1 : -1;
        }

        for (int fi = -90; fi <= 90; fi += 5)
        {
            // raytrace every 3 degrees makes sure we cover all tiles in a circle.
            for (int te = 0; te <= 360; te += 3)
            {
                double cos_te = Math.Cos(Deg2Rad(te));
                double sin_te = Math.Sin(Deg2Rad(te));
                double sin_fi = Math.Sin(Deg2Rad(fi));
                double cos_fi = Math.Cos(Deg2Rad(fi));

                origin = _save.getTile(new Position((int)centerX, (int)centerY, (int)centerZ));
                dest = origin;
                double l = 0;
                int tileX, tileY, tileZ;
                power_ = power;
                while (power_ > 0 && l <= maxRadius)
                {
                    if (power_ > 0)
                    {
                        if (type == ItemDamageType.DT_HE)
                        {
                            // explosives do 1/2 damage to terrain and 1/2 up to 3/2 random damage to units (the halving is handled elsewhere)
                            dest.setExplosive(power_, 0);
                        }

                        var ret = tilesAffected.Add(dest); // check if we had this tile already
                        if (ret)
                        {
                            int min = power_ * (100 - dmgRng) / 100;
                            int max = power_ * (100 + dmgRng) / 100;
                            BattleUnit bu = dest.getUnit();
                            Tile tileBelow = _save.getTile(dest.getPosition() - new Position(0, 0, 1));
                            int wounds = 0;
                            if (bu == null && dest.getPosition().z > 0 && dest.hasNoFloor(tileBelow))
                            {
                                bu = tileBelow.getUnit();
                                if (bu != null && bu.getHeight() + bu.getFloatHeight() - tileBelow.getTerrainLevel() <= 24)
                                {
                                    bu = null; // if the unit below has no voxels poking into the tile, don't damage it.
                                }
                            }
                            if (bu != null && unit != null)
                            {
                                wounds = bu.getFatalWounds();
                            }
                            switch (type)
                            {
                                case ItemDamageType.DT_STUN:
                                    // power 0 - 200%
                                    if (bu != null)
                                    {
                                        if (distance(dest.getPosition(), new Position((int)centerX, (int)centerY, (int)centerZ)) < 2)
                                        {
                                            bu.damage(new Position(0, 0, 0), RNG.generate(min, max), type);
                                        }
                                        else
                                        {
                                            bu.damage(new Position((int)centerX, (int)centerY, (int)centerZ) - dest.getPosition(), RNG.generate(min, max), type);
                                        }
                                    }
                                    foreach (var it in dest.getInventory())
                                    {
                                        if (it.getUnit() != null)
                                        {
                                            it.getUnit().damage(new Position(0, 0, 0), RNG.generate(min, max), type);
                                        }
                                    }
                                    break;
                                case ItemDamageType.DT_HE:
                                    {
                                        // power 50 - 150%
                                        if (bu != null)
                                        {
                                            if (
                                                    (
                                                        Math.Abs(dest.getPosition().x - (int)centerX) < 2
                                                        && Math.Abs(dest.getPosition().y - (int)centerY) < 2
                                                        && dest.getPosition().z == (int)centerZ
                                                    )
                                                    || dest.getPosition().z > (int)centerZ
                                                )
                                            {
                                                // ground zero effect is in effect, or unit is above explosion
                                                bu.damage(new Position(0, 0, 0), (RNG.generate(min, max)), type);
                                            }
                                            else
                                            {
                                                // directional damage relative to explosion position.
                                                // units above the explosion will be hit in the legs, units lateral to or below will be hit in the torso
                                                bu.damage(new Position((int)centerX, (int)centerY, (int)(centerZ + 5)) - dest.getPosition(), (RNG.generate(min, max)), type);
                                            }
                                        }
                                        List<BattleItem> temp = dest.getInventory(); // copy this list since it might change
                                        foreach (var it in temp)
                                        {
                                            if (power_ > it.getRules().getArmor())
                                            {
                                                if (it.getUnit() != null && it.getUnit().getStatus() == UnitStatus.STATUS_UNCONSCIOUS)
                                                {
                                                    it.getUnit().kill();
                                                }
                                                _save.removeItem(it);
                                            }
                                        }
                                    }
                                    break;

                                case ItemDamageType.DT_SMOKE:
                                    // smoke from explosions always stay 6 to 14 turns - power of a smoke grenade is 60
                                    if (dest.getSmoke() < 10 && dest.getTerrainLevel() > -24)
                                    {
                                        dest.setFire(0);
                                        dest.setSmoke(RNG.generate(7, 15));
                                    }
                                    break;

                                case ItemDamageType.DT_IN:
                                    if (!dest.isVoid())
                                    {
                                        if (dest.getFire() == 0 && (dest.getMapData(TilePart.O_FLOOR) != null || dest.getMapData(TilePart.O_OBJECT) != null))
                                        {
                                            dest.setFire(dest.getFuel() + 1);
                                            dest.setSmoke(Math.Clamp(15 - (dest.getFlammability() / 10), 1, 12));
                                        }
                                        if (bu != null)
                                        {
                                            float resistance = bu.getArmor().getDamageModifier(ItemDamageType.DT_IN);
                                            if (resistance > 0.0)
                                            {
                                                bu.damage(new Position(0, 0, 12 - dest.getTerrainLevel()), RNG.generate(Mod.Mod.FIRE_DAMAGE_RANGE[0], Mod.Mod.FIRE_DAMAGE_RANGE[1]), ItemDamageType.DT_IN, true);
                                                int burnTime = RNG.generate(0, (int)(5.0f * resistance));
                                                if (bu.getFire() < burnTime)
                                                {
                                                    bu.setFire(burnTime); // catch fire and burn
                                                }
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            if (unit != null && bu != null && bu.getFaction() != unit.getFaction())
                            {
                                unit.addFiringExp();
                                // if it's going to bleed to death and it's not a player, give credit for the kill.
                                if (wounds < bu.getFatalWounds() && bu.getFaction() != UnitFaction.FACTION_PLAYER)
                                {
                                    bu.killedBy(unit.getFaction());
                                }
                            }

                        }
                    }

                    l += 1.0;

                    tileX = (int)(Math.Floor(centerX + l * sin_te * cos_fi));
                    tileY = (int)(Math.Floor(centerY + l * cos_te * cos_fi));
                    tileZ = (int)(Math.Floor(centerZ + l * sin_fi));

                    origin = dest;
                    dest = _save.getTile(new Position(tileX, tileY, tileZ));

                    if (dest == null) break; // out of map!

                    // blockage by terrain is deducted from the explosion power
                    power_ -= 10; // explosive damage decreases by 10 per tile
                    if (origin.getPosition().z != tileZ)
                        power_ -= vertdec; //3d explosion factor

                    if (type == ItemDamageType.DT_IN)
                    {
                        int dir;
                        Pathfinding.vectorToDirection(origin.getPosition() - dest.getPosition(), out dir);
                        if (dir != -1 && dir % 2 != 0) power_ -= 5; // diagonal movement costs an extra 50% for fire.
                    }
                    if (l > 0.5)
                    {
                        if (l > 1.5)
                        {
                            power_ -= verticalBlockage(origin, dest, type, false) * 2;
                            power_ -= horizontalBlockage(origin, dest, type, false) * 2;
                        }
                        else //tricky bigwall deflection /Volutar
                        {
                            bool skipObject = diagonalWall == 0;
                            if (diagonalWall == (int)bigWallTypes.BIGWALLNESW) // --
                            {
                                if (hitSide < 0 && te >= 135 && te < 315)
                                    skipObject = true;
                                if (hitSide > 0 && (te < 135 || te > 315))
                                    skipObject = true;
                            }
                            if (diagonalWall == (int)bigWallTypes.BIGWALLNWSE) // |
                            {
                                if (hitSide > 0 && te >= 45 && te < 225)
                                    skipObject = true;
                                if (hitSide < 0 && (te < 45 || te > 225))
                                    skipObject = true;
                            }
                            power_ -= verticalBlockage(origin, dest, type, skipObject) * 2;
                            power_ -= horizontalBlockage(origin, dest, type, skipObject) * 2;

                        }
                    }
                }
            }
        }
        // now detonate the tiles affected with HE

        if (type == ItemDamageType.DT_HE)
        {
            foreach (var i in tilesAffected)
            {
                if (detonate(i))
                {
                    _save.addDestroyedObjective();
                }
                applyGravity(i);
                Tile j = _save.getTile(i.getPosition() + new Position(0, 0, 1));
                if (j != null)
                    applyGravity(j);
            }
        }

        calculateSunShading(); // roofs could have been destroyed
        calculateTerrainLighting(); // fires could have been started
        calculateFOV(center / new Position(16, 16, 24));
    }

    static TilePart[] parts = { TilePart.O_FLOOR, TilePart.O_WESTWALL, TilePart.O_NORTHWALL, TilePart.O_FLOOR, TilePart.O_WESTWALL, TilePart.O_NORTHWALL, TilePart.O_OBJECT, TilePart.O_OBJECT, TilePart.O_OBJECT }; //6th is the object of current
    /**
     * Applies the explosive power to the tile parts. This is where the actual destruction takes place.
     * Must affect 9 objects (6 box sides and the object inside plus 2 outer walls).
     * @param tile Tile affected.
     * @return True if the objective was destroyed.
     */
    bool detonate(Tile tile)
    {
        int explosive = tile.getExplosive();
        if (explosive == 0) return false; // no damage applied for this tile
        tile.setExplosive(0, 0, true);
        bool objective = false;
        Tile[] tiles = new Tile[9];
        Position pos = tile.getPosition();

        tiles[0] = _save.getTile(new Position(pos.x, pos.y, pos.z + 1)); //ceiling
        tiles[1] = _save.getTile(new Position(pos.x + 1, pos.y, pos.z)); //east wall
        tiles[2] = _save.getTile(new Position(pos.x, pos.y + 1, pos.z)); //south wall
        tiles[3] = tiles[4] = tiles[5] = tiles[6] = tile;
        tiles[7] = _save.getTile(new Position(pos.x, pos.y - 1, pos.z)); //north bigwall
        tiles[8] = _save.getTile(new Position(pos.x - 1, pos.y, pos.z)); //west bigwall

        int remainingPower, fireProof, fuel;
        bool destroyed, bigwalldestroyed = true, skipnorthwest = false;
        for (int i = 8; i >= 0; --i)
        {
            if (tiles[i] == null || tiles[i].getMapData(parts[i]) == null)
                continue; //skip out of map and emptiness
            int bigwall = tiles[i].getMapData(parts[i]).getBigWall();
            if (i > 6 && !((bigwall == 1) || (bigwall == 8) || (i == 8 && bigwall == 6) || (i == 7 && bigwall == 7)))
                continue;
            if ((bigwall != 0)) skipnorthwest = true;
            if (!bigwalldestroyed && i < 6) //when ground shouldn't be destroyed
                continue;
            if (skipnorthwest && (i == 2 || i == 1)) continue;
            remainingPower = explosive;
            destroyed = false;
            int volume = 0;
            TilePart currentpart = parts[i], currentpart2;
            int diemcd;
            fireProof = tiles[i].getFlammability(currentpart);
            fuel = tiles[i].getFuel(currentpart) + 1;
            // get the volume of the object by checking it's loftemps objects.
            for (int j = 0; j < 12; j++)
            {
                if (tiles[i].getMapData(currentpart).getLoftID(j) != 0)
                    ++volume;
            }
            if (i == 6 &&
                (bigwall == 2 || bigwall == 3) && //diagonals
                (2 * tiles[i].getMapData(currentpart).getArmor()) > remainingPower) //not enough to destroy
            {
                bigwalldestroyed = false;
            }
            // iterate through tile armor and destroy if can
            while (tiles[i].getMapData(currentpart) != null &&
                    (2 * tiles[i].getMapData(currentpart).getArmor()) <= remainingPower &&
                    tiles[i].getMapData(currentpart).getArmor() != 255)
            {
                if (i == 6 && (bigwall == 2 || bigwall == 3)) //diagonals for the current tile
                {
                    bigwalldestroyed = true;
                }
                if (i == 6 && (bigwall == 6 || bigwall == 7 || bigwall == 8)) //n/w/nw
                {
                    skipnorthwest = false;
                }
                remainingPower -= 2 * tiles[i].getMapData(currentpart).getArmor();
                destroyed = true;
                if (_save.getMissionType() == "STR_BASE_DEFENSE" &&
                    tiles[i].getMapData(currentpart).isBaseModule())
                {
                    var pair = _save.getModuleMap()[tile.getPosition().x / 10][tile.getPosition().y / 10];
                    _save.getModuleMap()[tile.getPosition().x / 10][tile.getPosition().y / 10] = KeyValuePair.Create(pair.Key, pair.Value - 1);
                }
                //this trick is to follow transformed object parts (object can become a ground)
                diemcd = tiles[i].getMapData(currentpart).getDieMCD();
                if (diemcd != 0)
                    currentpart2 = tiles[i].getMapData(currentpart).getDataset().getObject((uint)diemcd).getObjectType();
                else
                    currentpart2 = currentpart;
                if (tiles[i].destroy(currentpart, _save.getObjectiveType()))
                    objective = true;
                currentpart = currentpart2;
                if (tiles[i].getMapData(currentpart) != null) // take new values
                {
                    fireProof = tiles[i].getFlammability(currentpart);
                    fuel = tiles[i].getFuel(currentpart) + 1;
                }
            }
            // set tile on fire
            if ((2 * fireProof) < remainingPower)
            {
                if (tiles[i].getMapData(TilePart.O_FLOOR) != null || tiles[i].getMapData(TilePart.O_OBJECT) != null)
                {
                    tiles[i].setFire(fuel);
                    tiles[i].setSmoke(Math.Clamp(15 - (fireProof / 10), 1, 12));
                }
            }
            // add some smoke if tile was destroyed and not set on fire
            if (destroyed)
            {
                if (tiles[i].getFire() != 0 && tiles[i].getMapData(TilePart.O_FLOOR) == null && tiles[i].getMapData(TilePart.O_OBJECT) == null)
                {
                    tiles[i].setFire(0);// if the object set the floor on fire, and the floor was subsequently destroyed, the fire needs to go out
                }

                if (tiles[i].getFire() == 0)
                {
                    int smoke = RNG.generate(1, (volume / 2) + 3) + (volume / 2);
                    if (smoke > tiles[i].getSmoke())
                    {
                        tiles[i].setSmoke(Math.Clamp(smoke, 0, 15));
                    }
                }
            }
        }
        return objective;
    }

    /**
     * Closes ufo doors.
     * @return Whether doors are closed.
     */
    internal int closeUfoDoors()
    {
        int doorsclosed = 0;

        // prepare a list of tiles on fire/smoke & close any ufo doors
        for (int i = 0; i < _save.getMapSizeXYZ(); ++i)
        {
            if (_save.getTiles()[i].getUnit() != null && _save.getTiles()[i].getUnit().getArmor().getSize() > 1)
            {
                BattleUnit bu = _save.getTiles()[i].getUnit();
                Tile tile = _save.getTiles()[i];
                Tile oneTileNorth = _save.getTile(tile.getPosition() + new Position(0, -1, 0));
                Tile oneTileWest = _save.getTile(tile.getPosition() + new Position(-1, 0, 0));
                if ((tile.isUfoDoorOpen(TilePart.O_NORTHWALL) && oneTileNorth != null && oneTileNorth.getUnit() != null && oneTileNorth.getUnit() == bu) ||
                    (tile.isUfoDoorOpen(TilePart.O_WESTWALL) && oneTileWest != null && oneTileWest.getUnit() != null && oneTileWest.getUnit() == bu))
                {
                    continue;
                }
            }
            doorsclosed += _save.getTiles()[i].closeUfoDoor();
        }

        return doorsclosed;
    }

    /**
     * Gets the AI to look through a window.
     * @param position Current position.
     * @return Direction or -1 when no window found.
     */
    internal int faceWindow(Position position)
    {
        Tile tile = _save.getTile(position);
        if (tile != null && tile.getMapData(TilePart.O_NORTHWALL) != null && tile.getMapData(TilePart.O_NORTHWALL).getBlock(ItemDamageType.DT_NONE) == 0) return 0;
        tile = _save.getTile(position + oneTileEast);
        if (tile != null && tile.getMapData(TilePart.O_WESTWALL) != null && tile.getMapData(TilePart.O_WESTWALL).getBlock(ItemDamageType.DT_NONE) == 0) return 2;
        tile = _save.getTile(position + oneTileSouth);
        if (tile != null && tile.getMapData(TilePart.O_NORTHWALL) != null && tile.getMapData(TilePart.O_NORTHWALL).getBlock(ItemDamageType.DT_NONE) == 0) return 4;
        tile = _save.getTile(position);
        if (tile != null && tile.getMapData(TilePart.O_WESTWALL) != null && tile.getMapData(TilePart.O_WESTWALL).getBlock(ItemDamageType.DT_NONE) == 0) return 6;

        return -1;
    }

    /**
     * Calculates the distance squared between a unit and a point position.
     * @param unit The unit.
     * @param pos The point position.
     * @param considerZ Whether to consider the z coordinate.
     * @return Distance squared.
     */
    internal int distanceUnitToPositionSq(BattleUnit unit, Position pos, bool considerZ)
    {
	    int x = unit.getPosition().x - pos.x;
	    int y = unit.getPosition().y - pos.y;
	    int z = considerZ ? (unit.getPosition().z - pos.z) : 0;
	    if (unit.getArmor().getSize() > 1)
	    {
		    if (unit.getPosition().x < pos.x)
			    x++;
		    if (unit.getPosition().y < pos.y)
			    y++;
	    }
	    return x*x + y*y + z*z;
    }

    /**
     * Gets the origin voxel of a certain action.
     * @param action Battle action.
     * @param tile Pointer to the action tile.
     * @return origin position.
     */
    internal Position getOriginVoxel(BattleAction action, Tile tile)
    {
	    int[] dirYshift = {1, 1, 8, 15,15,15,8, 1};
	    int[] dirXshift = {8, 14,15,15,8, 1, 1, 1};
	    if (tile == null)
	    {
		    tile = action.actor.getTile();
	    }

	    Position origin = tile.getPosition();
	    Tile tileAbove = _save.getTile(origin + new Position(0,0,1));
	    Position originVoxel = new Position(origin.x*16, origin.y*16, origin.z*24);

	    // take into account soldier height and terrain level if the projectile is launched from a soldier
	    if (action.actor.getPosition() == origin || action.type != BattleActionType.BA_LAUNCH)
	    {
		    // calculate offset of the starting point of the projectile
		    originVoxel.z += -tile.getTerrainLevel();

		    originVoxel.z += action.actor.getHeight() + action.actor.getFloatHeight();

		    if (action.type == BattleActionType.BA_THROW)
		    {
			    originVoxel.z -= 3;
		    }
		    else
		    {
			    originVoxel.z -= 4;
		    }

		    if (originVoxel.z >= (origin.z + 1)*24)
		    {
			    if (tileAbove != null && tileAbove.hasNoFloor(null))
			    {
				    origin.z++;
			    }
			    else
			    {
				    while (originVoxel.z >= (origin.z + 1)*24)
				    {
					    originVoxel.z--;
				    }
				    originVoxel.z -= 4;
			    }
		    }
		    int direction = getDirectionTo(origin, action.target);
		    originVoxel.x += dirXshift[direction]*action.actor.getArmor().getSize();
		    originVoxel.y += dirYshift[direction]*action.actor.getArmor().getSize();
	    }
	    else
	    {
		    // don't take into account soldier height and terrain level if the projectile is not launched from a soldier(from a waypoint)
		    originVoxel.x += 8;
		    originVoxel.y += 8;
		    originVoxel.z += 16;
	    }
	    return originVoxel;
    }

    static int[] sliceObjectSpiral = {8,8, 8,6, 10,6, 10,8, 10,10, 8,10, 6,10, 6,8, 6,6, //first circle
        8,4, 10,4, 12,4, 12,6, 12,8, 12,10, 12,12, 10,12, 8,12, 6,12, 4,12, 4,10, 4,8, 4,6, 4,4, 6,4, //second circle
        8,1, 12,1, 15,1, 15,4, 15,8, 15,12, 15,15, 12,15, 8,15, 4,15, 1,15, 1,12, 1,8, 1,4, 1,1, 4,1}; //third circle
    static int[] westWallSpiral = {0,7, 0,9, 0,6, 0,11, 0,4, 0,13, 0,2};
    static int[] northWallSpiral = {7,0, 9,0, 6,0, 11,0, 4,0, 13,0, 2,0};
    /**
     * Checks for a tile part available for targeting and what particular voxel.
     * @param originVoxel Voxel of trace origin (gun's barrel).
     * @param tile The tile to check for.
     * @param part Tile part to check for.
     * @param scanVoxel Is returned coordinate of hit.
     * @param excludeUnit Is self (not to hit self).
     * @param rememberObstacles Remember obstacles for no LOF indicator?
     * @return True if the tile can be targetted.
     */
    internal bool canTargetTile(Position originVoxel, Tile tile, int part, Position scanVoxel, BattleUnit excludeUnit, bool rememberObstacles)
    {
	    Position targetVoxel = new Position((tile.getPosition().x * 16), (tile.getPosition().y * 16), tile.getPosition().z * 24);
	    var _trajectory = new List<Position>();

	    int[] spiralArray;
	    int spiralCount;

	    int minZ = 0, maxZ = 0;
	    bool minZfound = false, maxZfound = false;
	    bool dummy = false;

	    if (part == (int)TilePart.O_OBJECT)
	    {
		    spiralArray = sliceObjectSpiral;
		    spiralCount = 41;
	    }
	    else
	    if (part == (int)TilePart.O_NORTHWALL)
	    {
		    spiralArray = northWallSpiral;
		    spiralCount = 7;
	    }
	    else
	    if (part == (int)TilePart.O_WESTWALL)
	    {
		    spiralArray = westWallSpiral;
		    spiralCount = 7;
	    }
	    else if (part == (int)TilePart.O_FLOOR)
	    {
		    spiralArray = sliceObjectSpiral;
		    spiralCount = 41;
		    minZfound = true; minZ=0;
		    maxZfound = true; maxZ=0;
	    }
	    else if (part == MapData.O_DUMMY) // used only for no line of fire indicator
	    {
		    spiralArray = sliceObjectSpiral;
		    spiralCount = 41;
		    minZfound = true; minZ = 12;
		    maxZfound = true; maxZ = 12;
	    }
	    else
	    {
		    return false;
	    }
	    voxelCheckFlush();
    // find out height range

	    if (!minZfound)
	    {
		    for (int j = 1; j < 12; ++j)
		    {
			    if (minZfound) break;
			    for (int i = 0; i < spiralCount; ++i)
			    {
				    int tX = spiralArray[i*2];
				    int tY = spiralArray[i*2+1];
				    if (voxelCheck(new Position(targetVoxel.x + tX, targetVoxel.y + tY, targetVoxel.z + j*2),null,true) == (VoxelType)part) //bingo
				    {
					    if (!minZfound)
					    {
						    minZ = j*2;
						    minZfound = true;
						    break;
					    }
				    }
			    }
		    }
	    }

	    if (!minZfound)
	    {
		    if (rememberObstacles)
		    {
			    // dummy attempt (only to highlight obstacles)
			    minZfound = true;
			    minZ = 10;
			    dummy = true;
		    }
		    else
		    {
			    return false;//empty object!!!
		    }
	    }

	    if (!maxZfound)
	    {
		    for (int j = 10; j >= 0; --j)
		    {
			    if (maxZfound) break;
			    for (int i = 0; i < spiralCount; ++i)
			    {
				    int tX = spiralArray[i*2];
				    int tY = spiralArray[i*2+1];
				    if (voxelCheck(new Position(targetVoxel.x + tX, targetVoxel.y + tY, targetVoxel.z + j*2),null,true) == (VoxelType)part) //bingo
				    {
					    if (!maxZfound)
					    {
						    maxZ = j*2;
						    maxZfound = true;
						    break;
					    }
				    }
			    }
		    }
	    }

	    if (!maxZfound)
	    {
		    if (rememberObstacles)
		    {
			    // dummy attempt (only to highlight obstacles)
			    maxZfound = true;
			    maxZ = 10;
			    dummy = true;
		    }
		    else
		    {
			    return false;//it's impossible to get there
		    }
	    }

	    if (minZ > maxZ) minZ = maxZ;
	    int rangeZ = maxZ - minZ;
	    if (rangeZ>10) rangeZ = 10; //as above, clamping height range to prevent buffer overflow
	    int centerZ = (maxZ + minZ)/2;

	    for (int j = 0; j <= rangeZ; ++j)
	    {
		    scanVoxel.z = targetVoxel.z + centerZ + heightFromCenter[j];
		    for (int i = 0; i < spiralCount; ++i)
		    {
			    scanVoxel.x = targetVoxel.x + spiralArray[i*2];
			    scanVoxel.y = targetVoxel.y + spiralArray[i*2+1];
			    _trajectory.Clear();
			    int test = calculateLine(originVoxel, scanVoxel, false, _trajectory, excludeUnit, true);
			    if (test == part && !dummy) //bingo
			    {
				    if (_trajectory[0].x/16 == scanVoxel.x/16 &&
                        _trajectory[0].y/16 == scanVoxel.y/16 &&
                        _trajectory[0].z/24 == scanVoxel.z/24)
				    {
					    return true;
				    }
			    }
			    if (rememberObstacles && _trajectory.Count>0)
			    {
				    Tile tileObstacle = _save.getTile(new Position(_trajectory[0].x / 16, _trajectory[0].y / 16, _trajectory[0].z / 24));
				    if (tileObstacle != null) tileObstacle.setObstacle(test);
			    }
		    }
	    }
	    return false;
    }

    /**
     * Returns the direction from origin to target.
     * @param origin The origin point of the action.
     * @param target The target point of the action.
     * @return direction.
     */
    int getDirectionTo(Position origin, Position target)
    {
	    double ox = target.x - origin.x;
	    double oy = target.y - origin.y;
	    double angle = Math.Atan2(ox, -oy);
	    // divide the pie in 4 angles each at 1/8th before each quarter
	    double[] pie = {(M_PI_4 * 4.0) - M_PI_4 / 2.0, (M_PI_4 * 3.0) - M_PI_4 / 2.0, (M_PI_4 * 2.0) - M_PI_4 / 2.0, (M_PI_4 * 1.0) - M_PI_4 / 2.0};
	    int dir = 0;

	    if (angle > pie[0] || angle < -pie[0])
	    {
		    dir = 4;
	    }
	    else if (angle > pie[1])
	    {
		    dir = 3;
	    }
	    else if (angle > pie[2])
	    {
		    dir = 2;
	    }
	    else if (angle > pie[3])
	    {
		    dir = 1;
	    }
	    else if (angle < -pie[1])
	    {
		    dir = 5;
	    }
	    else if (angle < -pie[2])
	    {
		    dir = 6;
	    }
	    else if (angle < -pie[3])
	    {
		    dir = 7;
	    }
	    else if (angle < pie[0])
	    {
		    dir = 0;
	    }
	    return dir;
    }

    /**
     * Validates a throw action.
     * @param action The action to validate.
     * @param originVoxel The origin point of the action.
     * @param targetVoxel The target point of the action.
     * @param curve The curvature of the throw.
     * @param voxelType The type of voxel at which this parabola terminates.
     * @return Validity of action.
     */
    internal bool validateThrow(BattleAction action, Position originVoxel, Position targetVoxel, ref double curve, ref int voxelType, bool forced)
    {
	    bool foundCurve = false;
	    double curvature = 0.5;
	    if (action.type == BattleActionType.BA_THROW)
	    {
		    curvature = Math.Max(0.48, 1.73 / Math.Sqrt(Math.Sqrt((double)(action.actor.getBaseStats().strength) / (double)(action.weapon.getRules().getWeight()))) + (action.actor.isKneeled()? 0.1 : 0.0));
	    }
	    else
	    {
		    // arcing projectile weapons assume a fixed strength and weight.(70 and 10 respectively)
		    // curvature should be approximately 1.06358350461 at this point.
		    curvature = 1.73 / Math.Sqrt(Math.Sqrt(70.0 / 10.0)) + (action.actor.isKneeled()? 0.1 : 0.0);
	    }

	    Tile targetTile = _save.getTile(action.target);
	    Position targetPos = (targetVoxel / new Position(16, 16, 24));
	    // object blocking - can't throw here
	    if (action.type == BattleActionType.BA_THROW
		    && targetTile != null
		    && targetTile.getMapData(TilePart.O_OBJECT) != null
		    && targetTile.getMapData(TilePart.O_OBJECT).getTUCost(MovementType.MT_WALK) == 255
		    && !(targetTile.isBigWall()
		    && (targetTile.getMapData(TilePart.O_OBJECT).getBigWall()<1
		    || targetTile.getMapData(TilePart.O_OBJECT).getBigWall()>3)))
	    {
		    return false;
	    }
	    // out of range - can't throw here
	    if (ProjectileFlyBState.validThrowRange(action, originVoxel, targetTile) == false)
	    {
		    return false;
	    }

	    var trajectory = new List<Position>(16*20);
	    // thows should be around 10 tiles far, make one allocation that fit 99% cases with some margin
	    // we try 8 different curvatures to try and reach our goal.
	    int test = (int)VoxelType.V_OUTOFBOUNDS;
	    while (!foundCurve && curvature < 5.0)
	    {
		    trajectory.Clear();
		    test = calculateParabola(originVoxel, targetVoxel, true, trajectory, action.actor, curvature, new Position(0,0,0));
		    //position that item hit
		    Position hitPos = (trajectory.Last() + new Position(0,0,1)) / new Position(16, 16, 24);
		    //position where item will land
		    Position tilePos = Projectile.getPositionFromEnd(trajectory, Projectile.ItemDropVoxelOffset) / new Position(16, 16, 24);
		    if (forced || (test != (int)VoxelType.V_OUTOFBOUNDS && tilePos == targetPos))
		    {
			    if (voxelType != 0)
			    {
				    voxelType = test;
			    }
			    foundCurve = true;
		    }
		    else
		    {
			    curvature += 0.5;
			    if (test != (int)VoxelType.V_OUTOFBOUNDS && action.actor.getFaction() == UnitFaction.FACTION_PLAYER) //obstacle indicator is only for player
			    {
				    Tile hitTile = _save.getTile(hitPos);
				    if (hitTile != null)
				    {
					    hitTile.setObstacle(test);
				    }
			    }
		    }
	    }
	    if (curvature >= 5.0)
	    {
		    return false;
	    }
	    if (curve != 0.0)
	    {
		    curve = curvature;
	    }

	    return true;
    }

    /**
     * Calculates a parabola trajectory, used for throwing items.
     * @param origin Origin in voxelspace.
     * @param target Target in voxelspace.
     * @param storeTrajectory True will store the whole trajectory - otherwise it just stores the last position.
     * @param trajectory A vector of positions in which the trajectory is stored.
     * @param excludeUnit Makes sure the trajectory does not hit the shooter itself.
     * @param curvature How high the parabola goes: 1.0 is almost straight throw, 3.0 is a very high throw, to throw over a fence for example.
     * @param delta Is the deviation of the angles it should take into account, 0,0,0 is perfection.
     * @return The objectnumber(0-3) or unit(4) or out of map (5) or -1(hit nothing).
     */
    internal int calculateParabola(Position origin, Position target, bool storeTrajectory, List<Position> trajectory, BattleUnit excludeUnit, double curvature, Position delta)
    {
	    double ro = Math.Sqrt((double)((target.x - origin.x) * (target.x - origin.x) + (target.y - origin.y) * (target.y - origin.y) + (target.z - origin.z) * (target.z - origin.z)));

	    if (AreSame(ro, 0.0)) return (int)VoxelType.V_EMPTY;//just in case

	    double fi = Math.Acos((double)(target.z - origin.z) / ro);
	    double te = Math.Atan2((double)(target.y - origin.y), (double)(target.x - origin.x));

	    te += (delta.x / ro) / 2 * M_PI; //horizontal magic value
	    fi += ((delta.z + delta.y) / ro) / 14 * M_PI * curvature; //another magic value (vertical), to make it in line with fire spread

	    double zA = Math.Sqrt(ro)*curvature;
	    double zK = 4.0 * zA / ro / ro;

	    int x = origin.x;
	    int y = origin.y;
	    int z = origin.z;
	    int i = 8;
	    int result = (int)VoxelType.V_EMPTY;
	    Position lastPosition = new Position(x,y,z);
	    Position nextPosition = lastPosition;

	    if (storeTrajectory && trajectory != null)
	    {
		    //initla value for small hack to glue `calculateLine` into one continuous arc
		    trajectory.Add(lastPosition);
	    }
	    while (z > 0)
	    {
		    x = (int)((double)origin.x + (double)i * Math.Cos(te) * Math.Sin(fi));
		    y = (int)((double)origin.y + (double)i * Math.Sin(te) * Math.Sin(fi));
		    z = (int)((double)origin.z + (double)i * Math.Cos(fi) - zK * ((double)i - ro / 2.0) * ((double)i - ro / 2.0) + zA);
		    //passes through this point?
		    nextPosition = new Position(x,y,z);

		    if (storeTrajectory && trajectory != null)
		    {
			    //remove end point of previus trajectory part, becasue next one will add this point again
			    trajectory.RemoveAt(trajectory.Count - 1);
		    }
		    result = calculateLine(lastPosition, nextPosition, storeTrajectory, storeTrajectory ? trajectory : null, excludeUnit);
		    if (result != (int)VoxelType.V_EMPTY)
		    {
			    if (!storeTrajectory && trajectory != null)
			    {
				    result = calculateLine(lastPosition, nextPosition, false, trajectory, excludeUnit); //pick the INSIDE position of impact
			    }
			    break;
		    }
		    lastPosition = nextPosition;
		    ++i;
	    }
	    return result;
    }
}
