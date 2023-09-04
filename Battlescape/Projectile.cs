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
 * A class that represents a projectile. Map is the owner of an instance of this class during its short life.
 * It calculates its own trajectory and then moves along this precalculated trajectory in voxel space.
 */
internal class Projectile
{
	/// Offset of voxel path where item should be drop.
	internal const int ItemDropVoxelOffset = -2;

    Mod.Mod _mod;
    SavedBattleGame _save;
    BattleAction _action;
    Position _origin, _targetVoxel;
    uint _position;
    int _bulletSprite;
    bool _reversed;
    int _vaporColor, _vaporDensity, _vaporProbability;
    int _speed;
    Surface _sprite;
	List<Position> _trajectory;

    /**
     * Sets up a UnitSprite with the specified size and position.
     * @param mod Pointer to mod.
     * @param save Pointer to battlesavegame.
     * @param action An action.
     * @param origin Position the projectile originates from.
     * @param targetVoxel Position the projectile is targeting.
     * @param ammo the ammo that produced this projectile, where applicable.
     */
    internal Projectile(Mod.Mod mod, SavedBattleGame save, BattleAction action, Position origin, Position targetVoxel, BattleItem ammo)
    {
        _mod = mod;
        _save = save;
        _action = action;
        _origin = origin;
        _targetVoxel = targetVoxel;
        _position = 0;
        _bulletSprite = -1;
        _reversed = false;
        _vaporColor = -1;
        _vaporDensity = -1;
        _vaporProbability = 5;

        // this is the number of pixels the sprite will move between frames
        _speed = Options.battleFireSpeed;
        if (_action.weapon != null)
        {
            if (_action.type == BattleActionType.BA_THROW)
            {
                _sprite = _mod.getSurfaceSet("FLOOROB.PCK").getFrame(getItem().getRules().getFloorSprite());
            }
            else
            {
                // try to get all the required info from the ammo, if present
                if (ammo != null)
                {
                    _bulletSprite = ammo.getRules().getBulletSprite();
                    _vaporColor = ammo.getRules().getVaporColor();
                    _vaporDensity = ammo.getRules().getVaporDensity();
                    _vaporProbability = ammo.getRules().getVaporProbability();
                    _speed = Math.Max(1, _speed + ammo.getRules().getBulletSpeed());
                }

                // no ammo, or the ammo didn't contain the info we wanted, see what the weapon has on offer.
                if (_bulletSprite == -1)
                {
                    _bulletSprite = _action.weapon.getRules().getBulletSprite();
                }
                if (_vaporColor == -1)
                {
                    _vaporColor = _action.weapon.getRules().getVaporColor();
                }
                if (_vaporDensity == -1)
                {
                    _vaporDensity = _action.weapon.getRules().getVaporDensity();
                }
                if (_vaporProbability == 5)
                {
                    _vaporProbability = _action.weapon.getRules().getVaporProbability();
                }
                if (ammo == null || (ammo != _action.weapon || ammo.getRules().getBulletSpeed() == 0))
                {
                    _speed = Math.Max(1, _speed + _action.weapon.getRules().getBulletSpeed());
                }
            }
        }
        if ((targetVoxel.x - origin.x) + (targetVoxel.y - origin.y) >= 0)
        {
            _reversed = true;
        }
    }

    /**
     * Deletes the Projectile.
     */
    ~Projectile() { }

    /**
     * Gets the project tile item.
     * Returns 0 when there is no item thrown.
     * @return Pointer to BattleItem.
     */
    BattleItem getItem()
    {
        if (_action.type == BattleActionType.BA_THROW)
            return _action.weapon;
        else
            return null;
    }

    /**
     * Gets the Position of origin for the projectile
     * @return origin as a tile position.
     */
    internal Position getOrigin() =>
	    // instead of using the actor's position, we'll use the voxel origin translated to a tile position
	    // this is a workaround for large units.
	    _trajectory.First() / new Position(16,16,24);

    /**
     * Calculates the trajectory for a straight path.
     * @param accuracy The unit's accuracy.
     * @return The objectnumber(0-3) or unit(4) or out of map (5) or -1 (no line of fire).
     */
    internal int calculateTrajectory(double accuracy)
    {
	    Position originVoxel = _save.getTileEngine().getOriginVoxel(_action, _save.getTile(_origin));
	    return calculateTrajectory(accuracy, originVoxel);
    }

	internal int calculateTrajectory(double accuracy, Position originVoxel, bool excludeUnit = true)
    {
	    Tile targetTile = _save.getTile(_action.target);
	    BattleUnit bu = _action.actor;

	    VoxelType test;
	    if (excludeUnit)
	    {
		    test = (VoxelType)_save.getTileEngine().calculateLine(originVoxel, _targetVoxel, false, _trajectory, bu);
	    }
	    else
	    {
		    test = (VoxelType)_save.getTileEngine().calculateLine(originVoxel, _targetVoxel, false, _trajectory, null);
	    }

	    if (test != VoxelType.V_EMPTY &&
		    _trajectory.Any() &&
		    _action.actor.getFaction() == UnitFaction.FACTION_PLAYER &&
		    _action.autoShotCounter == 1 &&
		    ((SDL_GetModState() & SDL_Keymod.KMOD_CTRL) == 0 || !Options.forceFire) &&
		    _save.getBattleGame().getPanicHandled() &&
		    _action.type != BattleActionType.BA_LAUNCH)
	    {
		    Position hitPos = new Position(_trajectory[0].x/16, _trajectory[0].y/16, _trajectory[0].z/24);
		    if (test == VoxelType.V_UNIT && _save.getTile(hitPos) != null && _save.getTile(hitPos).getUnit() == null) //no unit? must be lower
		    {
			    hitPos = new Position(hitPos.x, hitPos.y, hitPos.z-1);
		    }

		    if (hitPos != _action.target && string.IsNullOrEmpty(_action.result))
		    {
			    if (test == VoxelType.V_NORTHWALL)
			    {
				    if (hitPos.y - 1 != _action.target.y)
				    {
					    _trajectory.Clear();
					    return (int)VoxelType.V_EMPTY;
				    }
			    }
			    else if (test == VoxelType.V_WESTWALL)
			    {
				    if (hitPos.x - 1 != _action.target.x)
				    {
					    _trajectory.Clear();
					    return (int)VoxelType.V_EMPTY;
				    }
			    }
			    else if (test == VoxelType.V_UNIT)
			    {
				    BattleUnit hitUnit = _save.getTile(hitPos).getUnit();
				    BattleUnit targetUnit = targetTile.getUnit();
				    if (hitUnit != targetUnit)
				    {
					    _trajectory.Clear();
					    return (int)VoxelType.V_EMPTY;
				    }
			    }
			    else
			    {
				    _trajectory.Clear();
				    return (int)VoxelType.V_EMPTY;
			    }
		    }
	    }

	    _trajectory.Clear();

	    bool extendLine = true;
	    // even guided missiles drift, but how much is based on
	    // the shooter's faction, rather than accuracy.
	    if (_action.type == BattleActionType.BA_LAUNCH)
	    {
		    if (_action.actor.getFaction() == UnitFaction.FACTION_PLAYER)
		    {
			    accuracy = 0.60;
		    }
		    else
		    {
			    accuracy = 0.55;
		    }
		    extendLine = _action.waypoints.Count <= 1;
	    }

	    // apply some accuracy modifiers.
	    // This will results in a new target voxel
	    applyAccuracy(originVoxel, _targetVoxel, accuracy, false, extendLine);

	    // finally do a line calculation and store this trajectory.
	    return _save.getTileEngine().calculateLine(originVoxel, _targetVoxel, true, _trajectory, bu);
    }

	/**
	 * Calculates the new target in voxel space, based on the given accuracy modifier.
	 * @param origin Startposition of the trajectory in voxels.
	 * @param target Endpoint of the trajectory in voxels.
	 * @param accuracy Accuracy modifier.
	 * @param keepRange Whether range affects accuracy.
	 * @param extendLine should this line get extended to maximum distance?
	 */
	void applyAccuracy(Position origin, Position target, double accuracy, bool keepRange, bool extendLine)
	{
		int xdiff = origin.x - target.x;
		int ydiff = origin.y - target.y;
		double realDistance = Math.Sqrt((double)(xdiff*xdiff)+(double)(ydiff*ydiff));
		// maxRange is the maximum range a projectile shall ever travel in voxel space
		double maxRange = keepRange?realDistance:16*1000; // 1000 tiles
		maxRange = _action.type == BattleActionType.BA_HIT?46:maxRange; // up to 2 tiles diagonally (as in the case of reaper v reaper)
		RuleItem weapon = _action.weapon.getRules();

		if (_action.type != BattleActionType.BA_THROW && _action.type != BattleActionType.BA_HIT)
		{
			double modifier = 0.0;
			int upperLimit = weapon.getAimRange();
			int lowerLimit = weapon.getMinRange();
			if (Options.battleUFOExtenderAccuracy)
			{
				if (_action.type == BattleActionType.BA_AUTOSHOT)
				{
					upperLimit = weapon.getAutoRange();
				}
				else if (_action.type == BattleActionType.BA_SNAPSHOT)
				{
					upperLimit = weapon.getSnapRange();
				}
			}
			if (realDistance / 16 < lowerLimit)
			{
				modifier = (weapon.getDropoff() * (lowerLimit - realDistance / 16)) / 100;
			}
			else if (upperLimit < realDistance / 16)
			{
				modifier = (weapon.getDropoff() * (realDistance / 16 - upperLimit)) / 100;
			}
			accuracy = Math.Max(0.0, accuracy - modifier);
		}

		int xDist = Math.Abs(origin.x - target.x);
		int yDist = Math.Abs(origin.y - target.y);
		int zDist = Math.Abs(origin.z - target.z);
		int xyShift, zShift;

		if (xDist / 2 <= yDist)				//yes, we need to add some x/y non-uniformity
			xyShift = xDist / 4 + yDist;	//and don't ask why, please. it's The Commandment
		else
			xyShift = (xDist + yDist) / 2;	//that's uniform part of spreading

		if (xyShift <= zDist)				//slight z deviation
			zShift = xyShift / 2 + zDist;
		else
			zShift = xyShift + zDist / 2;

		int deviation = (int)(RNG.generate(0, 100) - (accuracy * 100));

		if (deviation >= 0)
			deviation += 50;				// add extra spread to "miss" cloud
		else
			deviation += 10;				//accuracy of 109 or greater will become 1 (tightest spread)

		deviation = Math.Max(1, zShift * deviation / 200);	//range ratio

		target.x += RNG.generate(0, deviation) - deviation / 2;
		target.y += RNG.generate(0, deviation) - deviation / 2;
		target.z += RNG.generate(0, deviation / 2) / 2 - deviation / 8;

		if (extendLine)
		{
			double rotation, tilt;
			rotation = Math.Atan2((double)(target.y - origin.y), (double)(target.x - origin.x)) * 180 / M_PI;
			tilt = Math.Atan2((double)(target.z - origin.z),
				Math.Sqrt((double)(target.x - origin.x)*(double)(target.x - origin.x)+(double)(target.y - origin.y)*(double)(target.y - origin.y))) * 180 / M_PI;
			// calculate new target
			// this new target can be very far out of the map, but we don't care about that right now
			double cos_fi = Math.Cos(Deg2Rad(tilt));
			double sin_fi = Math.Sin(Deg2Rad(tilt));
			double cos_te = Math.Cos(Deg2Rad(rotation));
			double sin_te = Math.Sin(Deg2Rad(rotation));
			target.x = (int)(origin.x + maxRange * cos_te * cos_fi);
			target.y = (int)(origin.y + maxRange * sin_te * cos_fi);
			target.z = (int)(origin.z + maxRange * sin_fi);
		}
	}

	/**
	 * Calculates the trajectory for a curved path.
	 * @param accuracy The unit's accuracy.
	 * @return True when a trajectory is possible.
	 */
	internal int calculateThrow(double accuracy)
	{
		Tile targetTile = _save.getTile(_action.target);

		Position originVoxel = _save.getTileEngine().getOriginVoxel(_action, null);
		Position targetVoxel;
		var targets = new List<Position>();
		double curvature = 0.0;
		targetVoxel = _action.target * new Position(16,16,24) + new Position(8,8, (1 + -targetTile.getTerrainLevel()));
		targets.Clear();
		bool forced = false;

		if (_action.type == BattleActionType.BA_THROW)
		{
			targets.Add(targetVoxel);
		}
		else
		{
			BattleUnit tu = targetTile.getUnit();
			if (tu == null && _action.target.z > 0 && targetTile.hasNoFloor(null))
				tu = _save.getTile(_action.target - new Position(0, 0, 1)).getUnit();
			if (Options.forceFire && (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0 && _save.getSide() == UnitFaction.FACTION_PLAYER)
			{
				targets.Add(_action.target * new Position(16, 16, 24) + new Position(0, 0, 12));
				forced = true;
			}
			else if (tu != null && ((_action.actor.getFaction() != UnitFaction.FACTION_PLAYER) ||
				tu.getVisible()))
			{ //unit
				targetVoxel.z += tu.getFloatHeight(); //ground level is the base
				targets.Add(targetVoxel + new Position(0, 0, tu.getHeight() / 2 + 1));
				targets.Add(targetVoxel + new Position(0, 0, 2));
				targets.Add(targetVoxel + new Position(0, 0, tu.getHeight() - 1));
			}
			else if (targetTile.getMapData(TilePart.O_OBJECT) != null)
			{
				targetVoxel = _action.target * new Position(16,16,24) + new Position(8,8,0);
				targets.Add(targetVoxel + new Position(0, 0, 13));
				targets.Add(targetVoxel + new Position(0, 0, 8));
				targets.Add(targetVoxel + new Position(0, 0, 23));
				targets.Add(targetVoxel + new Position(0, 0, 2));
			}
			else if (targetTile.getMapData(TilePart.O_NORTHWALL) != null)
			{
				targetVoxel = _action.target * new Position(16,16,24) + new Position(8,0,0);
				targets.Add(targetVoxel + new Position(0, 0, 13));
				targets.Add(targetVoxel + new Position(0, 0, 8));
				targets.Add(targetVoxel + new Position(0, 0, 20));
				targets.Add(targetVoxel + new Position(0, 0, 3));
			}
			else if (targetTile.getMapData(TilePart.O_WESTWALL) != null)
 			{
				targetVoxel = _action.target * new Position(16,16,24) + new Position(0,8,0);
				targets.Add(targetVoxel + new Position(0, 0, 13));
				targets.Add(targetVoxel + new Position(0, 0, 8));
				targets.Add(targetVoxel + new Position(0, 0, 20));
				targets.Add(targetVoxel + new Position(0, 0, 2));
			}
			else if (targetTile.getMapData(TilePart.O_FLOOR) != null)
			{
				targets.Add(targetVoxel);
			}
		}

		int test = (int)VoxelType.V_OUTOFBOUNDS;
		foreach (var i in targets)
		{
			targetVoxel = i;
			if (_save.getTileEngine().validateThrow(_action, originVoxel, targetVoxel, ref curvature, ref test, forced))
			{
				break;
			}
		}
		if (!forced && test == (int)VoxelType.V_OUTOFBOUNDS) return test; //no line of fire

		test = (int)VoxelType.V_OUTOFBOUNDS;
		// finally do a line calculation and store this trajectory, make sure it's valid.
		while (test == (int)VoxelType.V_OUTOFBOUNDS)
		{
			Position deltas = targetVoxel;
			// apply some accuracy modifiers
			_trajectory.Clear();
			if (_action.type == BattleActionType.BA_THROW)
			{
				applyAccuracy(originVoxel, deltas, accuracy, true, false); //calling for best flavor
				deltas -= targetVoxel;
			}
			else
			{
				applyAccuracy(originVoxel, targetVoxel, accuracy, true, false); //arcing shot deviation
				deltas = new Position(0,0,0);
			}

			test = _save.getTileEngine().calculateParabola(originVoxel, targetVoxel, true, _trajectory, _action.actor, curvature, deltas);
			if (forced) return (int)TilePart.O_OBJECT; //fake hit
			Position endPoint = getPositionFromEnd(_trajectory, ItemDropVoxelOffset) / new Position(16, 16, 24);
			Tile endTile = _save.getTile(endPoint);
			// check if the item would land on a tile with a blocking object
			if (_action.type == BattleActionType.BA_THROW
				&& endTile != null
				&& endTile.getMapData(TilePart.O_OBJECT) != null
				&& endTile.getMapData(TilePart.O_OBJECT).getTUCost(MovementType.MT_WALK) == 255
				&& !(endTile.isBigWall() && (endTile.getMapData(TilePart.O_OBJECT).getBigWall()<1 || endTile.getMapData(TilePart.O_OBJECT).getBigWall()>3)))
			{
				test = (int)VoxelType.V_OUTOFBOUNDS;
			}
		}
		return test;
	}

	/**
	 * Get Position at offset from start from trajectory vector.
	 * @param trajectory Vector that have trajectory.
	 * @param pos Offset counted from ending of trajectory.
	 * @return Position in voxel space.
	 */
	internal static Position getPositionFromEnd(List<Position> trajectory, int pos) =>
		getPositionFromStart(trajectory, trajectory.Count + pos - 1);

	/**
	 * Get Position at offset from start from trajectory vector.
	 * @param trajectory Vector that have trajectory.
	 * @param pos Offset counted from begining of trajectory.
	 * @return Position in voxel space.
	 */
	static Position getPositionFromStart(List<Position> trajectory, int pos)
	{
		if (pos >= 0 && pos < (int)trajectory.Count)
			return trajectory[pos];
		else if (pos < 0)
			return trajectory[0];
		else
			return trajectory[trajectory.Count - 1];
	}
}
