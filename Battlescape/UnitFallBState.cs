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
 * State for falling units.
 */
internal class UnitFallBState : BattleState
{
	TileEngine _terrain;
	List<Tile> tilesToFallInto;
	List<BattleUnit> unitsToMove;

    /**
     * Sets up an UnitFallBState.
     * @param parent Pointer to the Battlescape.
     */
    internal UnitFallBState(BattlescapeGame parent) : base(parent) =>
        _terrain = null;

    /**
     * Deletes the UnitWalkBState.
     */
    ~UnitFallBState() { }

    /**
     * Initializes the state.
     */
    protected override void init()
    {
	    _terrain = _parent.getTileEngine();
	    if (_parent.getSave().getSide() == UnitFaction.FACTION_PLAYER)
		    _parent.setStateInterval((uint)Options.battleXcomSpeed);
	    else
		    _parent.setStateInterval((uint)Options.battleAlienSpeed);
    }

	/**
	 * Runs state functionality every cycle.
	 * Progresses the fall, updates the battlescape, ...
	 */
	protected override void think()
	{
		var fallingUnits = _parent.getSave().getFallingUnits();
		for (var unit = 0; unit < fallingUnits.Count;)
		{
			if (fallingUnits[unit].getStatus() == UnitStatus.STATUS_TURNING)
			{
				fallingUnits[unit].abortTurn();
			}
			bool largeCheck = true;
			bool falling = true;
			int size = fallingUnits[unit].getArmor().getSize() - 1;
			if (fallingUnits[unit].getHealth() == 0 || fallingUnits[unit].getStunlevel() >= fallingUnits[unit].getHealth())
			{
				fallingUnits.RemoveAt(unit);
				continue;
			}
			bool onScreen = (fallingUnits[unit].getVisible() && _parent.getMap().getCamera().isOnScreen(fallingUnits[unit].getPosition(), true, size, false));
			Tile tileBelow = _parent.getSave().getTile(fallingUnits[unit].getPosition() + new Position(0,0,-1));
			for (int x = size; x >= 0; x--)
			{
				for (int y = size; y >= 0; y--)
				{
					Tile otherTileBelow = _parent.getSave().getTile(fallingUnits[unit].getPosition() + new Position(x,y,-1));
					if (!_parent.getSave().getTile(fallingUnits[unit].getPosition() + new Position(x,y,0)).hasNoFloor(otherTileBelow) || fallingUnits[unit].getMovementType() == MovementType.MT_FLY)
					{
						largeCheck = false;
					}
				}
			}

			if (fallingUnits[unit].getStatus() == UnitStatus.STATUS_WALKING || fallingUnits[unit].getStatus() == UnitStatus.STATUS_FLYING)
			{
				fallingUnits[unit].keepWalking(tileBelow, true); 	// advances the phase
				_parent.getMap().cacheUnit(fallingUnits[unit]);	// make sure the unit sprites are up to date

				if (fallingUnits[unit].getPosition() != fallingUnits[unit].getLastPosition())
				{
					// Reset tiles moved from.
					for (int x = size; x >= 0; x--)
					{
						for (int y = size; y >= 0; y--)
						{
							// A falling unit might have already taken up this position so check that this unit is still here.
							if (_parent.getSave().getTile(fallingUnits[unit].getLastPosition() + new Position(x,y,0)).getUnit() == fallingUnits[unit])
							{
								_parent.getSave().getTile(fallingUnits[unit].getLastPosition() + new Position(x,y,0)).setUnit(null);
							}
						}
					}
					// Update tiles moved to.
					for (int x = size; x >= 0; x--)
					{
						for (int y = size; y >= 0; y--)
						{
							_parent.getSave().getTile(fallingUnits[unit].getPosition() + new Position(x,y,0)).setUnit(fallingUnits[unit], _parent.getSave().getTile(fallingUnits[unit].getPosition() + new Position(x,y,-1)));
						}
					}
				}

				++unit;
				continue;
			}

			falling = largeCheck
				&& fallingUnits[unit].getPosition().z != 0
				&& fallingUnits[unit].getTile().hasNoFloor(tileBelow)
				&& fallingUnits[unit].getMovementType() != MovementType.MT_FLY
				&& fallingUnits[unit].getWalkingPhase() == 0;

			if (falling)
			{
				// Tile(s) unit is falling into.
				for (int x = fallingUnits[unit].getArmor().getSize() - 1; x >= 0; --x)
				{
					for (int y = fallingUnits[unit].getArmor().getSize() - 1; y >= 0; --y)
					{
						Tile tileTarget = _parent.getSave().getTile(fallingUnits[unit].getPosition() + new Position(x,y,-1));
						tilesToFallInto.Add(tileTarget);
					}
				}
				//var fallingUnits = _parent.getSave().getFallingUnits();
				// Check each tile for units that need moving out of the way.
				foreach (var i in tilesToFallInto)
				{
					BattleUnit unitBelow = i.getUnit();
					if (unitBelow != null
						&& !(fallingUnits.Contains(unitBelow))  // ignore falling units (including self)
						&& !(unitsToMove.Contains(unitBelow)))       // ignore already added units
					{
						unitsToMove.Add(unitBelow);
					}
				}
			}

			falling = largeCheck
				&& fallingUnits[unit].getPosition().z != 0
				&& fallingUnits[unit].getTile().hasNoFloor(tileBelow)
				&& fallingUnits[unit].getMovementType() != MovementType.MT_FLY
				&& fallingUnits[unit].getWalkingPhase() == 0;

			// we are just standing around, we are done falling.
			if (fallingUnits[unit].getStatus() == UnitStatus.STATUS_STANDING)
			{
				if (falling)
				{
					Position destination = fallingUnits[unit].getPosition() + new Position(0,0,-1);
					Tile tileDest = _parent.getSave().getTile(destination);
					fallingUnits[unit].startWalking(Pathfinding.DIR_DOWN, destination, tileDest, onScreen);
					fallingUnits[unit].setCache(null);
					_parent.getMap().cacheUnit(fallingUnits[unit]);
					++unit;
				}
				else
				{
					// if the unit burns floortiles, burn floortiles
					if (fallingUnits[unit].getSpecialAbility() == (int)SpecialAbility.SPECAB_BURNFLOOR || fallingUnits[unit].getSpecialAbility() == (int)SpecialAbility.SPECAB_BURN_AND_EXPLODE)
					{
						fallingUnits[unit].getTile().ignite(1);
						Position groundVoxel = (fallingUnits[unit].getPosition() * new Position(16,16,24)) + new Position(8,8,-(fallingUnits[unit].getTile().getTerrainLevel()));
						_parent.getTileEngine().hit(groundVoxel, fallingUnits[unit].getBaseStats().strength, ItemDamageType.DT_IN, fallingUnits[unit]);

						if (fallingUnits[unit].getStatus() != UnitStatus.STATUS_STANDING) // ie: we burned a hole in the floor and fell through it
						{
							_parent.getPathfinding().abortPath();
						}
					}
					// move our personal lighting with us
					_terrain.calculateUnitLighting();
					_parent.getMap().cacheUnit(fallingUnits[unit]);
					fallingUnits[unit].setCache(null);
					_terrain.calculateFOV(fallingUnits[unit]);
					_parent.checkForProximityGrenades(fallingUnits[unit]);
					if (fallingUnits[unit].getStatus() == UnitStatus.STATUS_STANDING)
					{
						if (_parent.getTileEngine().checkReactionFire(fallingUnits[unit]))
							_parent.getPathfinding().abortPath();
						_parent.getSave().getFallingUnits().RemoveAt(unit);
					}
				}
			}
			else
			{
				++unit;
			}
		}

		// Find somewhere to move the unit(s) In danger of being squashed.
		if (unitsToMove.Any())
		{
			var escapeTiles = new List<Tile>();
			while (unitsToMove.Any())
			{
				BattleUnit unitBelow = unitsToMove[0];
				bool escapeFound = false;

				// We need to move all sections of the unit out of the way.
				var bodySections = new List<Position>();
				for (int x = unitBelow.getArmor().getSize() - 1; x >= 0; --x)
				{
					for (int y = unitBelow.getArmor().getSize() - 1; y >= 0; --y)
					{
						Position bs = unitBelow.getPosition() + new Position(x, y, 0);
						bodySections.Add(bs);
					}
				}

				// Check in each compass direction.
				for (int dir = 0; dir < Pathfinding.DIR_UP && !escapeFound; dir++)
				{
					Position offset;
					Pathfinding.directionToVector(dir, out offset);

					for (var bs = 0; bs < bodySections.Count;)
					{
						Position originalPosition = bodySections[bs];
						Position endPosition = originalPosition + offset;
						Tile t = _parent.getSave().getTile(endPosition);
						Tile bt = _parent.getSave().getTile(endPosition + new Position(0,0,-1));

						bool aboutToBeOccupiedFromAbove = t != null && tilesToFallInto.Contains(t);
						bool alreadyTaken = t != null && escapeTiles.Contains(t);
						bool alreadyOccupied = t != null && t.getUnit() != null && (t.getUnit() != unitBelow);
						bool movementBlocked = _parent.getSave().getPathfinding().getTUCost(originalPosition, dir, out endPosition, unitsToMove[0], null, false) == 255;
						bool hasFloor = t != null && !t.hasNoFloor(bt);
						bool unitCanFly = unitBelow.getMovementType() == MovementType.MT_FLY;

						bool canMoveToTile = t != null && !alreadyOccupied && !alreadyTaken && !aboutToBeOccupiedFromAbove && !movementBlocked && (hasFloor || unitCanFly);
						if (canMoveToTile)
						{
							// Check next section of the unit.
							++bs;
						}
						else
						{
							// Try next direction.
							break;
						}

						// If all sections of the fallen onto unit can be moved, then we move it.
						if (bs == bodySections.Count)
						{
							if (_parent.getSave().addFallingUnit(unitBelow))
							{
								escapeFound = true;
								// Now ensure no other unit escapes to here too.
								for (int x = unitBelow.getArmor().getSize() - 1; x >= 0; --x)
								{
									for (int y = unitBelow.getArmor().getSize() - 1; y >= 0; --y)
									{
										Tile et = _parent.getSave().getTile(t.getPosition() + new Position(x,y,0));
										escapeTiles.Add(et);
									}
								}

								Tile bu = _parent.getSave().getTile(originalPosition + new Position(0,0,-1));
								unitBelow.startWalking(dir, unitBelow.getPosition() + offset, bu,
									(unitBelow.getVisible() && _parent.getMap().getCamera().isOnScreen(unitBelow.getPosition(), true, unitBelow.getArmor().getSize() - 1, false)));
								unitsToMove.RemoveAt(0);
							}
						}
					}
				}
				if (!escapeFound)
				{
					// STOMP THAT GOOMBAH!
					unitBelow.knockOut(_parent);
					unitsToMove.RemoveAt(0);
				}
			}
			_parent.checkForCasualties(null, null);
		}

		if (!_parent.getSave().getFallingUnits().Any())
		{
			tilesToFallInto.Clear();
			unitsToMove.Clear();
			_parent.popState();
			return;
		}
	}
}
