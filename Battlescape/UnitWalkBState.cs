﻿/*
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
 * State for walking units.
 */
internal class UnitWalkBState : BattleState
{
	BattleUnit _unit;
	Pathfinding _pf;
	TileEngine _terrain;
	bool _falling;
	bool _beforeFirstStep;
	uint _numUnitsSpotted;
	int _preMovementCost;
	Position _target;

    /**
     * Sets up an UnitWalkBState.
     * @param parent Pointer to the Battlescape.
     * @param action Pointer to an action.
     */
    internal UnitWalkBState(BattlescapeGame parent, BattleAction action) : base(parent, action)
    {
        _unit = null;
        _pf = null;
        _terrain = null;
        _falling = false;
        _beforeFirstStep = false;
        _numUnitsSpotted = 0;
        _preMovementCost = 0;
    }

    /**
     * Deletes the UnitWalkBState.
     */
    ~UnitWalkBState() { }

    /**
     * Initializes the state.
     */
    internal override void init()
    {
	    _unit = _action.actor;
	    _numUnitsSpotted = (uint)_unit.getUnitsSpottedThisTurn().Count;
	    setNormalWalkSpeed();
	    _pf = _parent.getPathfinding();
	    _terrain = _parent.getTileEngine();
	    _target = _action.target;
	    if (Options.traceAI) { Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Walking from: {_unit.getPosition()}, to {_target}"); }
	    int dir = _pf.getStartDirection();
	    if (!_action.strafe && dir != -1 && dir != _unit.getDirection())
	    {
		    _beforeFirstStep = true;
	    }
    }

    /**
     * Handles some calculations when the walking is finished.
     */
    void setNormalWalkSpeed()
    {
	    if (_unit.getFaction() == UnitFaction.FACTION_PLAYER)
		    _parent.setStateInterval((uint)Options.battleXcomSpeed);
	    else
		    _parent.setStateInterval((uint)Options.battleAlienSpeed);
    }

	/**
	 * Runs state functionality every cycle.
	 */
	internal override void think()
	{
		bool unitSpotted = false;
		int size = _unit.getArmor().getSize() - 1;
		bool onScreen = (_unit.getVisible() && _parent.getMap().getCamera().isOnScreen(_unit.getPosition(), true, size, false));
		if (_unit.isKneeled())
		{
			if (_parent.kneel(_unit))
			{
				_unit.setCache(null);
				_terrain.calculateFOV(_unit);
				_parent.getMap().cacheUnit(_unit);
				return;
			}
			else
			{
				_action.result = "STR_NOT_ENOUGH_TIME_UNITS";
				_pf.abortPath();
				_parent.popState();
				return;
			}
		}

		if (_unit.isOut())
		{
			_pf.abortPath();
			_parent.popState();
			return;
		}

		if (_unit.getStatus() == UnitStatus.STATUS_WALKING || _unit.getStatus() == UnitStatus.STATUS_FLYING)
		{
			Tile tileBelow = _parent.getSave().getTile(_unit.getPosition() + new Position(0,0,-1));
			if ((_parent.getSave().getTile(_unit.getDestination()).getUnit() == null) || // next tile must be not occupied
				(_parent.getSave().getTile(_unit.getDestination()).getUnit() == _unit))
			{
				bool onScreenBoundary = (_unit.getVisible() && _parent.getMap().getCamera().isOnScreen(_unit.getPosition(), true, size, true));
				_unit.keepWalking(tileBelow, onScreenBoundary); // advances the phase
				playMovementSound();
			}
			else if (!_falling)
			{
				_unit.lookAt(_unit.getDestination(), (_unit.getTurretType() != -1));	// turn to undiscovered unit
				_pf.abortPath();
			}

			// unit moved from one tile to the other, update the tiles
			if (_unit.getPosition() != _unit.getLastPosition())
			{
				bool largeCheck = true;
				for (int x = size; x >= 0; x--)
				{
					for (int y = size; y >= 0; y--)
					{
						Tile otherTileBelow = _parent.getSave().getTile(_unit.getPosition() + new Position(x,y,-1));
						if (!_parent.getSave().getTile(_unit.getPosition() + new Position(x,y,0)).hasNoFloor(otherTileBelow) || _unit.getMovementType() == MovementType.MT_FLY)
							largeCheck = false;
						_parent.getSave().getTile(_unit.getLastPosition() + new Position(x,y,0)).setUnit(null);
					}
				}
				for (int x = size; x >= 0; x--)
				{
					for (int y = size; y >= 0; y--)
					{
						_parent.getSave().getTile(_unit.getPosition() + new Position(x,y,0)).setUnit(_unit, _parent.getSave().getTile(_unit.getPosition() + new Position(x,y,-1)));
					}
				}
				_falling = largeCheck && _unit.getPosition().z != 0 && _unit.getTile().hasNoFloor(tileBelow) && _unit.getMovementType() != MovementType.MT_FLY && _unit.getWalkingPhase() == 0;

				if (_falling)
				{
					for (int x = size; x >= 0; --x)
					{
						for (int y = size; y >= 0; --y)
						{
							Tile otherTileBelow = _parent.getSave().getTile(_unit.getPosition() + new Position(x,y,-1));
							if (otherTileBelow != null && otherTileBelow.getUnit() != null)
							{
								_falling = false;
								_pf.dequeuePath();
								_parent.getSave().addFallingUnit(_unit);
								_parent.statePushFront(new UnitFallBState(_parent));
								return;
							}
						}
					}
				}

				if (!_parent.getMap().getCamera().isOnScreen(_unit.getPosition(), true, size, false) && _unit.getFaction() != UnitFaction.FACTION_PLAYER && _unit.getVisible())
					_parent.getMap().getCamera().centerOnPosition(_unit.getPosition());
				// if the unit changed level, camera changes level with
				_parent.getMap().getCamera().setViewLevel(_unit.getPosition().z);
			}

			// is the step finished?
			if (_unit.getStatus() == UnitStatus.STATUS_STANDING)
			{
				// update the TU display
				_parent.getSave().getBattleState().updateSoldierInfo();
				// if the unit burns floortiles, burn floortiles as long as we're not falling
				if (!_falling && (_unit.getSpecialAbility() == (int)SpecialAbility.SPECAB_BURNFLOOR || _unit.getSpecialAbility() == (int)SpecialAbility.SPECAB_BURN_AND_EXPLODE))
				{
					_unit.getTile().ignite(1);
					Position posHere = _unit.getPosition();
					Position voxelHere = (posHere * new Position(16,16,24)) + new Position(8,8,-(_unit.getTile().getTerrainLevel()));
					_parent.getTileEngine().hit(voxelHere, _unit.getBaseStats().strength, ItemDamageType.DT_IN, _unit);

					if (_unit.getStatus() != UnitStatus.STATUS_STANDING) // ie: we burned a hole in the floor and fell through it
					{
						_pf.abortPath();
						return;
					}
				}

				// move our personal lighting with us
				_terrain.calculateUnitLighting();
				if (_unit.getFaction() != UnitFaction.FACTION_PLAYER)
				{
					_unit.setVisible(false);
				}
				_terrain.calculateFOV(_unit.getPosition());
				unitSpotted = (!_falling && !_action.desperate && _parent.getPanicHandled() && _numUnitsSpotted != _unit.getUnitsSpottedThisTurn().Count);

				if (_parent.checkForProximityGrenades(_unit))
				{
					_parent.popState();
					return;
				}
				if (unitSpotted)
				{
					_unit.setCache(null);
					_parent.getMap().cacheUnit(_unit);
					_pf.abortPath();
					_parent.popState();
					return;
				}
				// check for reaction fire
				if (!_falling)
				{
					if (_terrain.checkReactionFire(_unit))
					{
						// unit got fired upon - stop walking
						_unit.setCache(null);
						_parent.getMap().cacheUnit(_unit);
						_pf.abortPath();
						_parent.popState();
						return;
					}
				}
			}
			else if (onScreen)
			{
				// make sure the unit sprites are up to date
				if (_pf.getStrafeMove())
				{
					// This is where we fake out the strafe movement direction so the unit "moonwalks"
					int dirTemp = _unit.getDirection();
					_unit.setDirection(_unit.getFaceDirection());
					_parent.getMap().cacheUnit(_unit);
					_unit.setDirection(dirTemp);
				}
				else
				{
					_parent.getMap().cacheUnit(_unit);
				}
			}
		}

		// we are just standing around, shouldn't we be walking?
		if (_unit.getStatus() == UnitStatus.STATUS_STANDING || _unit.getStatus() == UnitStatus.STATUS_PANICKING)
		{
			// check if we did spot new units
			if (unitSpotted && !_action.desperate && _unit.getCharging() == null && !_falling)
			{
				if (Options.traceAI) { Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Uh-oh! Company!"); }
				_unit.setHiding(false); // clearly we're not hidden now
				_parent.getMap().cacheUnit(_unit);
				postPathProcedures();
				return;
			}

			if (onScreen || _parent.getSave().getDebugMode())
			{
				setNormalWalkSpeed();
			}
			else
			{
				_parent.setStateInterval(0);
			}
			int dir = _pf.getStartDirection();
			if (_falling)
			{
				dir = Pathfinding.DIR_DOWN;
			}

			if (dir != -1)
			{
				if (_pf.getStrafeMove())
				{
					_unit.setFaceDirection(_unit.getDirection());
				}

				Position destination;
				int tu = _pf.getTUCost(_unit.getPosition(), dir, out destination, _unit, null, false); // gets tu cost, but also gets the destination position.
				if (_unit.getFaction() != UnitFaction.FACTION_PLAYER &&
					_unit.getSpecialAbility() < (int)SpecialAbility.SPECAB_BURNFLOOR &&
					_parent.getSave().getTile(destination) != null &&
					_parent.getSave().getTile(destination).getFire() > 0)
				{
					tu -= 32; // we artificially inflate the TU cost by 32 points in getTUCost under these conditions, so we have to deflate it here.
				}
				if (_falling)
				{
					tu = 0;
				}
				int energy = tu;
				if (dir >= Pathfinding.DIR_UP)
				{
					energy = 0;
				}
				else if (_action.run)
				{
					tu = (int)(tu * 0.75);
					energy = (int)(energy * 1.5);
				}
				if (tu > _unit.getTimeUnits())
				{
					if (_parent.getPanicHandled() && tu < 255)
					{
						_action.result = "STR_NOT_ENOUGH_TIME_UNITS";
					}
					_pf.abortPath();
					_unit.setCache(null);
					_parent.getMap().cacheUnit(_unit);
					_parent.popState();
					return;
				}

				if (energy / 2 > _unit.getEnergy())
				{
					if (_parent.getPanicHandled())
					{
						_action.result = "STR_NOT_ENOUGH_ENERGY";
					}
					_pf.abortPath();
					_unit.setCache(null);
					_parent.getMap().cacheUnit(_unit);
					_parent.popState();
					return;
				}

				if (_parent.getPanicHandled() && _parent.checkReservedTU(_unit, tu) == false)
				{
					_pf.abortPath();
					_unit.setCache(null);
					_parent.getMap().cacheUnit(_unit);
					return;
				}

				// we are looking in the wrong way, turn first (unless strafing)
				// we are not using the turn state, because turning during walking costs no tu
				if (dir != _unit.getDirection() && dir < Pathfinding.DIR_UP && !_pf.getStrafeMove())
				{
					_unit.lookAt(dir);
					_unit.setCache(null);
					_parent.getMap().cacheUnit(_unit);
					return;
				}

				// now open doors (if any)
				if (dir < Pathfinding.DIR_UP)
				{
					int door = _terrain.unitOpensDoor(_unit, false, dir);
					if (door == 3)
					{
						return; // don't start walking yet, wait for the ufo door to open
					}
					if (door == 0)
					{
						_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)Mod.Mod.DOOR_OPEN).play(-1, _parent.getMap().getSoundAngle(_unit.getPosition())); // normal door
					}
					if (door == 1)
					{
						_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)Mod.Mod.SLIDING_DOOR_OPEN).play(-1, _parent.getMap().getSoundAngle(_unit.getPosition())); // ufo door
						return; // don't start walking yet, wait for the ufo door to open
					}
				}
				for (int x = size; x >= 0; --x)
				{
					for (int y = size; y >= 0; --y)
					{
						BattleUnit unitInMyWay = _parent.getSave().getTile(destination + new Position(x,y,0)).getUnit();
						BattleUnit unitBelowMyWay = null;
						Tile belowDest = _parent.getSave().getTile(destination + new Position(x,y,-1));
						if (belowDest != null)
						{
							unitBelowMyWay = belowDest.getUnit();
						}
						// can't walk into units in this tile, or on top of other units sticking their head into this tile
						if (!_falling &&
							((unitInMyWay != null && unitInMyWay != _unit)
							|| (belowDest != null && unitBelowMyWay != null && unitBelowMyWay != _unit &&
							(-belowDest.getTerrainLevel() + unitBelowMyWay.getFloatHeight() + unitBelowMyWay.getHeight())
							>= 28)))  // 4+ voxels poking into the tile above, we don't kick people in the head here at XCom.
						{
							_action.TU = 0;
							_pf.abortPath();
							_unit.setCache(null);
							_parent.getMap().cacheUnit(_unit);
							_parent.popState();
							return;
						}
					}
				}
				// now start moving
				dir = _pf.dequeuePath();
				if (_falling)
				{
					dir = Pathfinding.DIR_DOWN;
				}

				if (_unit.spendTimeUnits(tu))
				{
					if (_unit.spendEnergy(energy))
					{
						Tile tileBelow = _parent.getSave().getTile(_unit.getPosition() + new Position(0,0,-1));
						_unit.startWalking(dir, destination, tileBelow, onScreen);
						_beforeFirstStep = false;
					}
				}
				// make sure the unit sprites are up to date
				if (onScreen)
				{
					if (_pf.getStrafeMove())
					{
						// This is where we fake out the strafe movement direction so the unit "moonwalks"
						int dirTemp = _unit.getDirection();
						_unit.setDirection(_unit.getFaceDirection());
						_parent.getMap().cacheUnit(_unit);
						_unit.setDirection(dirTemp);
					}
					else
					{
						_parent.getMap().cacheUnit(_unit);
					}
				}
			}
			else
			{
				postPathProcedures();
				return;
			}
		}

		// turning during walking costs no tu
		if (_unit.getStatus() == UnitStatus.STATUS_TURNING)
		{
			// except before the first step.
			if (_beforeFirstStep)
			{
				_preMovementCost++;
			}

			_unit.turn();

			// calculateFOV is unreliable for setting the unitSpotted bool, as it can be called from various other places
			// in the code, ie: doors opening, and this messes up the result.
			_terrain.calculateFOV(_unit);
			unitSpotted = (!_falling && !_action.desperate && _parent.getPanicHandled() && _numUnitsSpotted != _unit.getUnitsSpottedThisTurn().Count);

			// make sure the unit sprites are up to date
			_unit.setCache(null);
			_parent.getMap().cacheUnit(_unit);

			if (unitSpotted && !_action.desperate && _unit.getCharging() == null && !_falling)
			{
				if (_beforeFirstStep)
				{
					_unit.spendTimeUnits(_preMovementCost);
				}
				if (Options.traceAI) { Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Egads! A turn reveals new units! I must pause!"); }
				_unit.setHiding(false); // not hidden, are we...
				_pf.abortPath();
				_unit.abortTurn(); //revert to a standing state.
				_unit.setCache(null);
				_parent.getMap().cacheUnit(_unit);
				_parent.popState();
			}
		}
	}

	/**
	 * Handles the stepping sounds.
	 */
	void playMovementSound()
	{
		int size = _unit.getArmor().getSize() - 1;
		if ((!_unit.getVisible() && !_parent.getSave().getDebugMode()) || !_parent.getMap().getCamera().isOnScreen(_unit.getPosition(), true, size, false)) return;

		if (_unit.getMoveSound() != -1)
		{
			// if a sound is configured in the ruleset, play that one
			if (_unit.getWalkingPhase() == 0)
			{
				_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)_unit.getMoveSound()).play(-1, _parent.getMap().getSoundAngle(_unit.getPosition()));
			}
		}
		else
		{
			if (_unit.getStatus() == UnitStatus.STATUS_WALKING)
			{
				Tile tile = _unit.getTile();
				Tile tileBelow = _parent.getSave().getTile(tile.getPosition() + new Position(0,0,-1));
				// play footstep sound 1
				if (_unit.getWalkingPhase() == 3)
				{
					if (tile.getFootstepSound(tileBelow) > -1)
					{
						_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)(Mod.Mod.WALK_OFFSET + (tile.getFootstepSound(tileBelow)*2))).play(-1, _parent.getMap().getSoundAngle(_unit.getPosition()));
					}
				}
				// play footstep sound 2
				if (_unit.getWalkingPhase() == 7)
				{
					if (tile.getFootstepSound(tileBelow) > -1)
					{
						_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)(1 + Mod.Mod.WALK_OFFSET + (tile.getFootstepSound(tileBelow)*2))).play(-1, _parent.getMap().getSoundAngle(_unit.getPosition()));
					}
				}
			}
			else if (_unit.getMovementType() == MovementType.MT_FLY)
			{
				// play default flying sound
				if (_unit.getWalkingPhase() == 1 && !_falling)
				{
					_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)Mod.Mod.FLYING_SOUND).play(-1, _parent.getMap().getSoundAngle(_unit.getPosition()));
				}
			}
		}
	}

	/**
	 * Handles some calculations when the path is finished.
	 */
	void postPathProcedures()
	{
		_action.TU = 0;
		if (_unit.getFaction() != UnitFaction.FACTION_PLAYER)
		{
			int dir = _action.finalFacing;
			if (_action.finalAction)
			{
				_unit.dontReselect();
			}
			if (_unit.getCharging() != null)
			{
				dir = _parent.getTileEngine().getDirectionTo(_unit.getPosition(), _unit.getCharging().getPosition());
				if (_parent.getTileEngine().validMeleeRange(_unit, _action.actor.getCharging(), dir))
				{
					var action = new BattleAction();
					action.actor = _unit;
					action.target = _unit.getCharging().getPosition();
					action.weapon = _unit.getMeleeWeapon();
					action.type = BattleActionType.BA_HIT;
					action.TU = _unit.getActionTUs(action.type, action.weapon);
					action.targeting = true;
					_unit.setCharging(null);
					_parent.statePushBack(new MeleeAttackBState(_parent, action));
				}
			}
			else if (_unit.isHiding())
			{
				dir = _unit.getDirection() + 4;
				_unit.setHiding(false);
				_unit.dontReselect();
			}
			if (dir != -1)
			{
				if (dir >= 8)
				{
					dir -= 8;
				}
				_unit.lookAt(dir);
				while (_unit.getStatus() == UnitStatus.STATUS_TURNING)
				{
					_unit.turn();
					_parent.getTileEngine().calculateFOV(_unit);
				}
			}
		}
		else if (!_parent.getPanicHandled())
		{
			//todo: set the unit to aggrostate and try to find cover?
			_unit.setTimeUnits(0);
		}

		_unit.setCache(null);
		_terrain.calculateUnitLighting();
		_terrain.calculateFOV(_unit);
		_parent.getMap().cacheUnit(_unit);
		if (!_falling)
			_parent.popState();
	}

	/**
	 * Aborts unit walking.
	 */
	internal override void cancel()
	{
		if (_parent.getSave().getSide() == UnitFaction.FACTION_PLAYER && _parent.getPanicHandled())
		_pf.abortPath();
	}
}
