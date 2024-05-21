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
 * A projectile state.
 */
internal class ProjectileFlyBState : BattleState
{
	BattleUnit _unit;
	BattleItem _ammo;
	BattleItem _projectileItem;
	Position _origin, _targetVoxel, _originVoxel;
	VoxelType _projectileImpact;
	bool _initialized, _targetFloor;

    /**
     * Sets up an ProjectileFlyBState.
     */
    ProjectileFlyBState(BattlescapeGame parent, BattleAction action, Position origin) : base(parent, action)
    {
        _unit = null;
        _ammo = null;
        _projectileItem = null;
        _origin = origin;
        _originVoxel = new(-1, -1, -1);
        _projectileImpact = 0;
        _initialized = false;
        _targetFloor = false;
    }

    internal ProjectileFlyBState(BattlescapeGame parent, BattleAction action) : base(parent, action)
    {
        _unit = null;
        _ammo = null;
        _projectileItem = null;
        _origin = action.actor.getPosition();
        _originVoxel = new(-1, -1, -1);
        _projectileImpact = 0;
        _initialized = false;
        _targetFloor = false;
    }

    /**
     * Deletes the ProjectileFlyBState.
     */
    ~ProjectileFlyBState() { }

	/**
	 * Initializes the sequence:
	 * - checks if the shot is valid,
	 * - calculates the base accuracy.
	 */
	internal override void init()
	{
		if (_initialized) return;
		_initialized = true;

		BattleItem weapon = _action.weapon;
		_projectileItem = null;

		if (weapon == null) // can't shoot without weapon
		{
			_parent.popState();
			return;
		}

		if (_parent.getSave().getTile(_action.target) == null) // invalid target position
		{
			_parent.popState();
			return;
		}

		if (_parent.getPanicHandled() &&
			_action.actor.getTimeUnits() < _action.TU)
		{
			_action.result = "STR_NOT_ENOUGH_TIME_UNITS";
			_parent.popState();
			return;
		}

		_unit = _action.actor;

		_ammo = weapon.getAmmoItem();

		if (_unit.isOut() || _unit.getHealth() == 0 || _unit.getHealth() < _unit.getStunlevel())
		{
			// something went wrong - we can't shoot when dead or unconscious, or if we're about to fall over.
			_parent.popState();
			return;
		}

		// reaction fire
		if (_unit.getFaction() != _parent.getSave().getSide())
		{
			// no ammo or target is dead: give the time units back and cancel the shot.
			if (_ammo == null
				|| _parent.getSave().getTile(_action.target).getUnit() == null
				|| _parent.getSave().getTile(_action.target).getUnit().isOut()
				|| _parent.getSave().getTile(_action.target).getUnit() != _parent.getSave().getSelectedUnit())
			{
				_unit.setTimeUnits(_unit.getTimeUnits() + _unit.getActionTUs(_action.type, _action.weapon));
				_parent.popState();
				return;
			}
			_unit.lookAt(_action.target, _unit.getTurretType() != -1);
			while (_unit.getStatus() == UnitStatus.STATUS_TURNING)
			{
				_unit.turn();
			}
		}

		Tile endTile = _parent.getSave().getTile(_action.target);
		int distanceSq = _parent.getTileEngine().distanceUnitToPositionSq(_action.actor, _action.target, false);
		bool isPlayer = _parent.getSave().getSide() == UnitFaction.FACTION_PLAYER;
		if (isPlayer) _parent.getMap().resetObstacles();
		switch (_action.type)
		{
			case BattleActionType.BA_SNAPSHOT:
			case BattleActionType.BA_AIMEDSHOT:
			case BattleActionType.BA_AUTOSHOT:
			case BattleActionType.BA_LAUNCH:
				if (_ammo == null)
				{
					_action.result = "STR_NO_AMMUNITION_LOADED";
					_parent.popState();
					return;
				}
				if (_ammo.getAmmoQuantity() == 0)
				{
					_action.result = "STR_NO_ROUNDS_LEFT";
					_parent.popState();
					return;
				}
				if (distanceSq > weapon.getRules().getMaxRangeSq())
				{
					// special handling for short ranges and diagonals
					{
						// special handling for maxRange 1: allow it to target diagonally adjacent tiles (one diagonal move)
						if (weapon.getRules().getMaxRange() == 1 && distanceSq <= 3)
						{
							break;
						}
						// special handling for maxRange 2: allow it to target diagonally adjacent tiles (one diagonal move + one straight move)
						else if (weapon.getRules().getMaxRange() == 2 && distanceSq <= 6)
						{
							break;
						}
					}
					// out of range
					_action.result = "STR_OUT_OF_RANGE";
					_parent.popState();
					return;
				}
				break;
			case BattleActionType.BA_THROW:
				if (!validThrowRange(_action, _parent.getTileEngine().getOriginVoxel(_action, null), _parent.getSave().getTile(_action.target)))
				{
					// out of range
					_action.result = "STR_OUT_OF_RANGE";
					_parent.popState();
					return;
				}
				if (endTile != null &&
					endTile.getTerrainLevel() == -24 &&
					endTile.getPosition().z + 1 < _parent.getSave().getMapSizeZ())
				{
					_action.target.z += 1;
				}
				_projectileItem = weapon;
				break;
			default:
				_parent.popState();
				return;
		}

		bool forceEnableObstacles = false;
		if (_action.type == BattleActionType.BA_LAUNCH || (Options.forceFire && (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0 && isPlayer) || !_parent.getPanicHandled())
		{
			// target nothing, targets the middle of the tile
			_targetVoxel = new Position(_action.target.x*16 + 8, _action.target.y*16 + 8, _action.target.z*24 + 12);
			if (_action.type == BattleActionType.BA_LAUNCH)
			{
				if (_targetFloor)
				{
					// launched missiles with two waypoints placed on the same tile: target the floor.
					_targetVoxel.z -= 10;
				}
				else
				{
					// launched missiles go slightly higher than the middle.
					_targetVoxel.z += 4;
				}
			}
		}
		else if (!_action.weapon.getRules().getArcingShot())
		{
			// determine the target voxel.
			// aim at the center of the unit, the object, the walls or the floor (in that priority)
			// if there is no LOF to the center, try elsewhere (more outward).
			// Store this target voxel.
			Tile targetTile = _parent.getSave().getTile(_action.target);
			Position hitPos;
			Position originVoxel = _parent.getTileEngine().getOriginVoxel(_action, _parent.getSave().getTile(_origin));
			if (targetTile.getUnit() != null &&
				((_unit.getFaction() != UnitFaction.FACTION_PLAYER) ||
				targetTile.getUnit().getVisible()))
			{
				if (_origin == _action.target || targetTile.getUnit() == _unit)
				{
					// don't shoot at yourself but shoot at the floor
					_targetVoxel = new Position(_action.target.x * 16 + 8, _action.target.y * 16 + 8, _action.target.z * 24);
				}
				else
				{
					if (!_parent.getTileEngine().canTargetUnit(originVoxel, targetTile, _targetVoxel, _unit, isPlayer))
					{
						_targetVoxel = new Position(-16, -16, -24); // out of bounds, even after voxel to tile calculation.
						if (isPlayer)
						{
							forceEnableObstacles = true;
						}
					}
				}
			}
			else if (targetTile.getMapData(TilePart.O_OBJECT) != null)
			{
				if (!_parent.getTileEngine().canTargetTile(originVoxel, targetTile, (int)TilePart.O_OBJECT, _targetVoxel, _unit, isPlayer))
				{
					_targetVoxel = new Position(_action.target.x*16 + 8, _action.target.y*16 + 8, _action.target.z*24 + 10);
				}
			}
			else if (targetTile.getMapData(TilePart.O_NORTHWALL) != null)
			{
				if (!_parent.getTileEngine().canTargetTile(originVoxel, targetTile, (int)TilePart.O_NORTHWALL, _targetVoxel, _unit, isPlayer))
				{
					_targetVoxel = new Position(_action.target.x*16 + 8, _action.target.y*16, _action.target.z*24 + 9);
				}
			}
			else if (targetTile.getMapData(TilePart.O_WESTWALL) != null)
			{
				if (!_parent.getTileEngine().canTargetTile(originVoxel, targetTile, (int)TilePart.O_WESTWALL, _targetVoxel, _unit, isPlayer))
				{
					_targetVoxel = new Position(_action.target.x*16, _action.target.y*16 + 8, _action.target.z*24 + 9);
				}
			}
			else if (targetTile.getMapData(TilePart.O_FLOOR) != null)
			{
				if (!_parent.getTileEngine().canTargetTile(originVoxel, targetTile, (int)TilePart.O_FLOOR, _targetVoxel, _unit, isPlayer))
				{
					_targetVoxel = new Position(_action.target.x*16 + 8, _action.target.y*16 + 8, _action.target.z*24 + 2);
				}
			}
			else
			{
				// dummy attempt (only to highlight obstacles)
				_parent.getTileEngine().canTargetTile(originVoxel, targetTile, MapData.O_DUMMY, _targetVoxel, _unit, isPlayer);

				// target nothing, targets the middle of the tile
				_targetVoxel = new Position(_action.target.x*16 + 8, _action.target.y*16 + 8, _action.target.z*24 + 12);
			}
		}

		if (createNewProjectile())
		{
			_parent.getMap().setCursorType(CursorType.CT_NONE);
			_parent.getMap().getCamera().stopMouseScrolling();
			_parent.getMap().disableObstacles();
		}
		else if (isPlayer && (_targetVoxel.z >= 0 || forceEnableObstacles))
		{
			_parent.getMap().enableObstacles();
		}
	}

	/**
	 * Tries to create a projectile sprite and add it to the map,
	 * calculating its trajectory.
	 * @return True, if the projectile was successfully created.
	 */
	bool createNewProjectile()
	{
		++_action.autoShotCounter;

		// create a new projectile
		Projectile projectile = new Projectile(_parent.getMod(), _parent.getSave(), _action, _origin, _targetVoxel, _ammo);

		// add the projectile on the map
		_parent.getMap().setProjectile(projectile);

		// set the speed of the state think cycle to 16 ms (roughly one think cycle per frame)
		_parent.setStateInterval(1000/60);

		// let it calculate a trajectory
		_projectileImpact = VoxelType.V_EMPTY;

		double accuracyDivider = 100.0;
		// berserking units are half as accurate
		if (!_parent.getPanicHandled())
		{
			accuracyDivider = 200.0;
		}

		if (_action.type == BattleActionType.BA_THROW)
		{
			_projectileImpact = (VoxelType)projectile.calculateThrow(_unit.getThrowingAccuracy() / accuracyDivider);
			if (_projectileImpact == VoxelType.V_FLOOR || _projectileImpact == VoxelType.V_UNIT || _projectileImpact == VoxelType.V_OBJECT)
			{
				if (_unit.getFaction() != UnitFaction.FACTION_PLAYER && _projectileItem.getRules().getBattleType() == BattleType.BT_GRENADE)
				{
					_projectileItem.setFuseTimer(0);
				}
				_projectileItem.moveToOwner(null);
				_unit.setCache(null);
				_parent.getMap().cacheUnit(_unit);
				_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)Mod.Mod.ITEM_THROW).play(-1, _parent.getMap().getSoundAngle(_unit.getPosition()));
				_unit.addThrowingExp();
			}
			else
			{
				// unable to throw here
				projectile = null;
				_parent.getMap().setProjectile(null);
				_action.result = "STR_UNABLE_TO_THROW_HERE";
				_action.TU = 0;
				_parent.popState();
				return false;
			}
		}
		else if (_action.weapon.getRules().getArcingShot()) // special code for the "spit" trajectory
		{
			_projectileImpact = (VoxelType)projectile.calculateThrow(_unit.getFiringAccuracy(_action.type, _action.weapon) / accuracyDivider);
			if (_projectileImpact != VoxelType.V_EMPTY && _projectileImpact != VoxelType.V_OUTOFBOUNDS)
			{
				// set the soldier in an aiming position
				_unit.aim(true);
				_unit.setCache(null);
				_parent.getMap().cacheUnit(_unit);
				// and we have a lift-off
				if (_ammo.getRules().getFireSound() != -1)
				{
					_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)_ammo.getRules().getFireSound()).play(-1, _parent.getMap().getSoundAngle(_unit.getPosition()));
				}
				else if (_action.weapon.getRules().getFireSound() != -1)
				{
					_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)_action.weapon.getRules().getFireSound()).play(-1, _parent.getMap().getSoundAngle(_unit.getPosition()));
				}
				if (!_parent.getSave().getDebugMode() && _action.type != BattleActionType.BA_LAUNCH && _ammo.spendBullet() == false)
				{
					_parent.getSave().removeItem(_ammo);
					_action.weapon.setAmmoItem(null);
				}
			}
			else
			{
				// no line of fire
				projectile = null;
				_parent.getMap().setProjectile(null);
				if (_parent.getPanicHandled())
				{
					_action.result = "STR_NO_LINE_OF_FIRE";
				}
				else
				{
					_unit.setTimeUnits(_unit.getTimeUnits() + _action.TU); // refund shot TUs for berserking
				}
				_unit.abortTurn();
				_parent.popState();
				return false;
			}
		}
		else
		{
			if (_originVoxel != new Position(-1,-1,-1))
			{
				_projectileImpact = (VoxelType)projectile.calculateTrajectory(_unit.getFiringAccuracy(_action.type, _action.weapon) / accuracyDivider, _originVoxel, false);
			}
			else
			{
				_projectileImpact = (VoxelType)projectile.calculateTrajectory(_unit.getFiringAccuracy(_action.type, _action.weapon) / accuracyDivider);
			}
			if (_targetVoxel != new Position(-16,-16,-24) && (_projectileImpact != VoxelType.V_EMPTY || _action.type == BattleActionType.BA_LAUNCH))
			{
				// set the soldier in an aiming position
				_unit.aim(true);
				_unit.setCache(null);
				_parent.getMap().cacheUnit(_unit);
				// and we have a lift-off
				if (_ammo.getRules().getFireSound() != -1)
				{
					_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)_ammo.getRules().getFireSound()).play(-1, _parent.getMap().getSoundAngle(projectile.getOrigin()));
				}
				else if (_action.weapon.getRules().getFireSound() != -1)
				{
					_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)_action.weapon.getRules().getFireSound()).play(-1, _parent.getMap().getSoundAngle(projectile.getOrigin()));
				}
				if (!_parent.getSave().getDebugMode() && _action.type != BattleActionType.BA_LAUNCH && _ammo.spendBullet() == false)
				{
					_parent.getSave().removeItem(_ammo);
					_action.weapon.setAmmoItem(null);
				}
			}
			else
			{
				// no line of fire
				projectile = null;
				_parent.getMap().setProjectile(null);
				if (_parent.getPanicHandled())
				{
					_action.result = "STR_NO_LINE_OF_FIRE";
				}
				else
				{
					_unit.setTimeUnits(_unit.getTimeUnits() + _action.TU); // refund shot TUs for berserking
				}
				_unit.abortTurn();
				_parent.popState();
				return false;
			}
		}

		if (_action.type != BattleActionType.BA_THROW && _action.type != BattleActionType.BA_LAUNCH)
			_unit.getStatistics().shotsFiredCounter++;

		return true;
	}

	/**
	 * Validates the throwing range.
	 * @param action Pointer to throw action.
	 * @param origin Position to throw from.
	 * @param target Tile to throw to.
	 * @return True when the range is valid.
	 */
	internal static bool validThrowRange(BattleAction action, Position origin, Tile target)
	{
		// note that all coordinates and thus also distances below are in number of tiles (not in voxels).
		if (action.type != BattleActionType.BA_THROW)
		{
			return true;
		}
		int offset = 2;
		int zd = (origin.z)-((action.target.z * 24 + offset) - target.getTerrainLevel());
		int weight = action.weapon.getRules().getWeight();
		if (action.weapon.getAmmoItem() != null && action.weapon.getAmmoItem() != action.weapon)
		{
			weight += action.weapon.getAmmoItem().getRules().getWeight();
		}
		double maxDistance = (getMaxThrowDistance(weight, action.actor.getBaseStats().strength, zd) + 8) / 16.0;
		int xdiff = action.target.x - action.actor.getPosition().x;
		int ydiff = action.target.y - action.actor.getPosition().y;
		double realDistance = Math.Sqrt((double)(xdiff*xdiff)+(double)(ydiff*ydiff));

		return realDistance <= maxDistance;
	}

	/**
	 * Validates the throwing range.
	 * @param weight the weight of the object.
	 * @param strength the strength of the thrower.
	 * @param level the difference in height between the thrower and the target.
	 * @return the maximum throwing range.
	 */
	static int getMaxThrowDistance(int weight, int strength, int level)
	{
		double curZ = level + 0.5;
		double dz = 1.0;
		int dist = 0;
		while (dist < 4000) //just in case
		{
			dist += 8;
			if (dz<-1)
				curZ -= 8;
			else
				curZ += dz * 8;

			if (curZ < 0 && dz < 0) //roll back
			{
				dz = Math.Max(dz, -1.0);
				if (Math.Abs(dz)>1e-10) //rollback horizontal
					dist = (int)(dist - curZ / dz);
				break;
			}
			dz = (double)(dz - 50 * weight / strength)/100;
			if (dz <= -2.0) //become falling
				break;
		}
		return dist;
	}

	/**
	 * Animates the projectile (moves to the next point in its trajectory).
	 * If the animation is finished the projectile sprite is removed from the map,
	 * and this state is finished.
	 */
	internal override void think()
	{
		_parent.getSave().getBattleState().clearMouseScrollingState();
		/* TODO refactoring : store the projectile in this state, instead of getting it from the map each time? */
		if (_parent.getMap().getProjectile() == null)
		{
			Tile t = _parent.getSave().getTile(_action.actor.getPosition());
			Tile bt = _parent.getSave().getTile(_action.actor.getPosition() + new Position(0,0,-1));
			bool hasFloor = t != null && !t.hasNoFloor(bt);
			bool unitCanFly = _action.actor.getMovementType() == MovementType.MT_FLY;

			if (_action.type == BattleActionType.BA_AUTOSHOT
				&& _action.autoShotCounter < _action.weapon.getRules().getAutoShots()
				&& !_action.actor.isOut()
				&& _ammo.getAmmoQuantity() != 0
				&& (hasFloor || unitCanFly))
			{
				createNewProjectile();
				if (_action.cameraPosition.z != -1)
				{
					_parent.getMap().getCamera().setMapOffset(_action.cameraPosition);
					_parent.getMap().invalidate();
				}
			}
			else
			{
				if (_action.cameraPosition.z != -1 && _action.waypoints.Count <= 1)
				{
					_parent.getMap().getCamera().setMapOffset(_action.cameraPosition);
					_parent.getMap().invalidate();
				}
				if (!_parent.getSave().getUnitsFalling() && _parent.getPanicHandled())
				{
					_parent.getTileEngine().checkReactionFire(_unit);
				}
				if (!_unit.isOut())
				{
					_unit.abortTurn();
				}
				if (_parent.getSave().getSide() == UnitFaction.FACTION_PLAYER || _parent.getSave().getDebugMode())
				{
					_parent.setupCursor();
				}
				_parent.convertInfected();
				_parent.popState();
			}
		}
		else
		{
			if (_action.type != BattleActionType.BA_THROW && _ammo != null && _ammo.getRules().getShotgunPellets() != 0)
			{
				// shotgun pellets move to their terminal location instantly as fast as possible
				_parent.getMap().getProjectile().skipTrajectory();
			}
			if (!_parent.getMap().getProjectile().move())
			{
				// impact !
				if (_action.type == BattleActionType.BA_THROW)
				{
					_parent.getMap().resetCameraSmoothing();
					Position pos = _parent.getMap().getProjectile().getPosition(Projectile.ItemDropVoxelOffset);
					pos.x /= 16;
					pos.y /= 16;
					pos.z /= 24;
					if (pos.y > _parent.getSave().getMapSizeY())
					{
						pos.y--;
					}
					if (pos.x > _parent.getSave().getMapSizeX())
					{
						pos.x--;
					}
					BattleItem item = _parent.getMap().getProjectile().getItem();
					_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)Mod.Mod.ITEM_DROP).play(-1, _parent.getMap().getSoundAngle(pos));

					if (Options.battleInstantGrenade && item.getRules().getBattleType() == BattleType.BT_GRENADE && item.getFuseTimer() == 0)
					{
						// it's a hot grenade to explode immediately
						_parent.statePushFront(new ExplosionBState(_parent, _parent.getMap().getProjectile().getPosition(Projectile.ItemDropVoxelOffset), item, _action.actor));
					}
					else
					{
						_parent.dropItem(pos, item);
						if (_unit.getFaction() != UnitFaction.FACTION_PLAYER && _projectileItem.getRules().getBattleType() == BattleType.BT_GRENADE)
						{
							_parent.getTileEngine().setDangerZone(pos, item.getRules().getExplosionRadius(), _action.actor);
						}
					}
				}
				else if (_action.type == BattleActionType.BA_LAUNCH && _action.waypoints.Count > 1 && _projectileImpact == VoxelType.V_EMPTY)
				{
					_origin = _action.waypoints.First();
					_action.waypoints.RemoveAt(0);
					_action.target = _action.waypoints.First();
					// launch the next projectile in the waypoint cascade
					ProjectileFlyBState nextWaypoint = new ProjectileFlyBState(_parent, _action, _origin);
					nextWaypoint.setOriginVoxel(_parent.getMap().getProjectile().getPosition(-1));
					if (_origin == _action.target)
					{
						nextWaypoint.targetFloor();
					}
					_parent.statePushNext(nextWaypoint);

				}
				else
				{
					if (_parent.getSave().getTile(_action.target).getUnit() != null)
					{
						_parent.getSave().getTile(_action.target).getUnit().getStatistics().shotAtCounter++; // Only counts for guns, not throws or launches
					}

					_parent.getMap().resetCameraSmoothing();
					if (_ammo != null && _action.type == BattleActionType.BA_LAUNCH && _ammo.spendBullet() == false)
					{
						_parent.getSave().removeItem(_ammo);
						_action.weapon.setAmmoItem(null);
					}

					if (_projectileImpact != VoxelType.V_OUTOFBOUNDS)
					{
						int offset = 0;
						// explosions impact not inside the voxel but two steps back (projectiles generally move 2 voxels at a time)
						if (_ammo != null && _ammo.getRules().getExplosionRadius() != 0 && _projectileImpact != VoxelType.V_UNIT)
						{
							offset = -2;
						}

						_parent.statePushFront(new ExplosionBState(_parent, _parent.getMap().getProjectile().getPosition(offset), _ammo, _action.actor, null, (_action.type != BattleActionType.BA_AUTOSHOT || _action.autoShotCounter == _action.weapon.getRules().getAutoShots() || _action.weapon.getAmmoItem() == null)));

						if (_projectileImpact == VoxelType.V_UNIT)
						{
							projectileHitUnit(_parent.getMap().getProjectile().getPosition(offset));
						}

						int firingXP = _unit.getFiringXP();
						// special shotgun behaviour: trace extra projectile paths, and add bullet hits at their termination points.
						if (_ammo != null && _ammo.getRules().getShotgunPellets() != 0)
						{
							int i = 1;
							while (i != _ammo.getRules().getShotgunPellets())
							{
								// create a projectile
								Projectile proj = new Projectile(_parent.getMod(), _parent.getSave(), _action, _origin, _targetVoxel, _ammo);
								// let it trace to the point where it hits
								int secondaryImpact = proj.calculateTrajectory(Math.Max(0.0, (_unit.getFiringAccuracy(_action.type, _action.weapon) / 100.0) - i * 5.0));
								if (secondaryImpact != (int)VoxelType.V_EMPTY)
								{
									// as above: skip the shot to the end of it's path
									proj.skipTrajectory();
									// insert an explosion and hit
									if (secondaryImpact != (int)VoxelType.V_OUTOFBOUNDS)
									{
										if (secondaryImpact == (int)VoxelType.V_UNIT)
										{
											projectileHitUnit(proj.getPosition(offset));
										}
										Explosion explosion = new Explosion(proj.getPosition(offset), _ammo.getRules().getHitAnimation());
										_parent.getMap().getExplosions().Add(explosion);
										if (_ammo.getRules().getExplosionRadius() != 0)
										{
											_parent.getTileEngine().explode(proj.getPosition(offset), _ammo.getRules().getPower(), _ammo.getRules().getDamageType(), _ammo.getRules().getExplosionRadius(), _unit);
										}
										else
										{
											_parent.getSave().getTileEngine().hit(proj.getPosition(offset), _ammo.getRules().getPower(), _ammo.getRules().getDamageType(), _unit);
										}
									}
								}
								++i;
								proj = null;
							}
						}

						if (_unit.getFiringXP() > firingXP + 1)
						{
							_unit.nerfFiringXP(firingXP + 1);
						}
					}
					else if (_action.type != BattleActionType.BA_AUTOSHOT || _action.autoShotCounter == _action.weapon.getRules().getAutoShots() || _action.weapon.getAmmoItem() == null)
					{
						_unit.aim(false);
						_unit.setCache(null);
						_parent.getMap().cacheUnits();
					}
				}

				_parent.getMap().setProjectile(null);
			}
		}
	}

	/**
	 * Set the origin voxel, used for the blaster launcher.
	 * @param pos the origin voxel.
	 */
	void setOriginVoxel(Position pos) =>
		_originVoxel = pos;

	/**
	 * Set the boolean flag to angle a blaster bomb towards the floor.
	 */
	void targetFloor() =>
		_targetFloor = true;

	void projectileHitUnit(Position pos)
	{
		BattleUnit victim = _parent.getSave().getTile(pos / new Position(16,16,24)).getUnit();
		BattleUnit targetVictim = _parent.getSave().getTile(_action.target).getUnit(); // Who we were aiming at (not necessarily who we hit)
		if (victim != null && !victim.isOut())
		{
			victim.getStatistics().hitCounter++;
			if (_unit.getOriginalFaction() == UnitFaction.FACTION_PLAYER && victim.getOriginalFaction() == UnitFaction.FACTION_PLAYER)
			{
				victim.getStatistics().shotByFriendlyCounter++;
				_unit.getStatistics().shotFriendlyCounter++;
			}
			if (victim == targetVictim) // Hit our target
			{
				int distanceSq = _parent.getTileEngine().distanceUnitToPositionSq(_action.actor, victim.getPosition(), false);
				int distance = (int)Math.Ceiling(Math.Sqrt((float)distanceSq));
				int accuracy = _unit.getFiringAccuracy(_action.type, _action.weapon);
				// code from Map.drawTerrain(), where the crosshair accuracy is calculated
				if (Options.battleUFOExtenderAccuracy)
				{
					RuleItem weapon = _action.weapon.getRules();
					int upperLimit =  weapon.getAimRange();
					int lowerLimit =  weapon.getMinRange();
					if (_action.type == BattleActionType.BA_AUTOSHOT)
					{
						upperLimit =  weapon.getAutoRange();
					}
					else if (_action.type == BattleActionType.BA_SNAPSHOT)
					{
						upperLimit =  weapon.getSnapRange();
					}
					if (distance > upperLimit)
					{
						accuracy -= (distance - upperLimit) * weapon.getDropoff();
					}
					else if (distance < lowerLimit)
					{
						accuracy -= (lowerLimit - distance) * weapon.getDropoff();
					}
					if (accuracy < 0)
					{
						accuracy = 0;	
					}
				}
			
				_unit.getStatistics().shotsLandedCounter++;			
				if (distance > 30)
				{
					_unit.getStatistics().longDistanceHitCounter++;
				}
				if (accuracy < distance)
				{
					_unit.getStatistics().lowAccuracyHitCounter++;
				}
			}
			if (victim.getFaction() == UnitFaction.FACTION_HOSTILE)
			{
				AIModule ai = victim.getAIModule();
				if (ai != null)
				{
					ai.setWasHitBy(_unit);
					_unit.setTurnsSinceSpotted(0);
				}
			}
			// Record the last unit to hit our victim. If a victim dies without warning*, this unit gets the credit.
			// *Because the unit died in a fire or bled out.
			victim.setMurdererId(_unit.getId());
			if (_action.weapon != null)
				victim.setMurdererWeapon(_action.weapon.getRules().getName());
			if (_ammo != null)
				victim.setMurdererWeaponAmmo(_ammo.getRules().getName());
		}
	}

	/**
	 * Flying projectiles cannot be cancelled,
	 * but they can be "skipped".
	 */
	internal override void cancel()
	{
		if (_parent.getMap().getProjectile() != null)
		{
			_parent.getMap().getProjectile().skipTrajectory();
			Position p = _parent.getMap().getProjectile().getPosition();
			if (!_parent.getMap().getCamera().isOnScreen(new Position(p.x/16, p.y/16, p.z/24), false, 0, false))
				_parent.getMap().getCamera().centerOnPosition(new Position(p.x/16, p.y/16, p.z/24));
		}
	}
}
