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
    const int MAX_DARKNESS_TO_SEE_UNITS = 9;

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
    TileEngine(SavedBattleGame save, List<ushort> voxelData)
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
	internal void itemDrop(Tile t, BattleItem item, Mod.Mod mod, bool newItem, bool removeItem)
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
		else if (item.getRules().getBattleType() != Mod.BattleType.BT_GRENADE && item.getRules().getBattleType() != Mod.BattleType.BT_PROXIMITYGRENADE)
		{
			item.setOwner(null);
		}

		applyGravity(_save.getTile(p));

		if (item.getRules().getBattleType() == Mod.BattleType.BT_FLARE)
		{
			calculateTerrainLighting();
            calculateFOV(p);
		}
	}

    /**
      * Recalculates lighting for the terrain: objects,items,fire.
      */
    void calculateTerrainLighting()
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
                if (item.getRules().getBattleType() == Mod.BattleType.BT_FLARE)
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
    Tile applyGravity(Tile t)
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
    void calculateFOV(Position position)
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
    bool calculateFOV(BattleUnit unit)
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
    int distance(Position pos1, Position pos2)
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
    bool canTargetUnit(Position originVoxel, Tile tile, Position scanVoxel, BattleUnit excludeUnit, bool rememberObstacles, BattleUnit potentialUnit = null)
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
    int calculateLine(Position origin, Position target, bool storeTrajectory, List<Position> trajectory, BattleUnit excludeUnit, bool doVoxelCheck = true, bool onlyVisible = false, BattleUnit excludeAllBut = null)
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
    int horizontalBlockage(Tile startTile, Tile endTile, ItemDamageType type, bool skipObject)
    {
        // safety check
        if (startTile == null || endTile == null) return 0;
        if (startTile.getPosition().z != endTile.getPosition().z) return 0;
        Tile tmpTile;

        int direction = 0;
        Pathfinding.vectorToDirection(endTile.getPosition() - startTile.getPosition(), ref direction);
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
    VoxelType voxelCheck(Position voxel, BattleUnit excludeUnit, bool excludeAllUnits, bool onlyVisible, BattleUnit excludeAllBut)
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
        for (int i = (int)VoxelType.V_FLOOR; i <= (int)VoxelType.V_OBJECT; ++i)
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
}
