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

enum AIMode { AI_PATROL, AI_AMBUSH, AI_COMBAT, AI_ESCAPE };

/**
 * This class is used by the BattleUnit AI.
 */
internal class AIModule
{
    SavedBattleGame _save;
    BattleUnit _unit;
    BattleUnit _aggroTarget;
    int _knownEnemies, _visibleEnemies, _spottingEnemies;
    int _escapeTUs, _ambushTUs;
    bool _rifle, _melee, _blaster;
    bool _traceAI, _didPsi;
    int _AIMode, _intelligence, _closestDist;
    Node _fromNode, _toNode;
    BattleActionType _reserve;
    BattleAction _escapeAction, _ambushAction, _attackAction, _patrolAction, _psiAction;
    UnitFaction _targetFaction;
    List<int> _reachable, _reachableWithAttack, _wasHitBy;

    /**
     * Sets up a BattleAIState.
     * @param save Pointer to the battle game.
     * @param unit Pointer to the unit.
     * @param node Pointer to the node the unit originates from.
     */
    internal AIModule(SavedBattleGame save, BattleUnit unit, Node node)
    {
        _save = save;
        _unit = unit;
        _aggroTarget = null;
        _knownEnemies = 0;
        _visibleEnemies = 0;
        _spottingEnemies = 0;
        _escapeTUs = 0;
        _ambushTUs = 0;
        _rifle = false;
        _melee = false;
        _blaster = false;
        _didPsi = false;
        _AIMode = (int)AIMode.AI_PATROL;
        _closestDist = 100;
        _fromNode = node;
        _toNode = null;

        _traceAI = Options.traceAI;

        _reserve = BattleActionType.BA_NONE;
        _intelligence = _unit.getIntelligence();
        _escapeAction = new BattleAction();
        _ambushAction = new BattleAction();
        _attackAction = new BattleAction();
        _patrolAction = new BattleAction();
        _psiAction = new BattleAction();
        _targetFaction = UnitFaction.FACTION_PLAYER;
        if (_unit.getOriginalFaction() == UnitFaction.FACTION_NEUTRAL)
        {
            _targetFaction = UnitFaction.FACTION_HOSTILE;
        }
    }

    /**
     * Deletes the BattleAIState.
     */
    ~AIModule()
    {
        _escapeAction = default;
        _ambushAction = default;
        _attackAction = default;
        _patrolAction = default;
        _psiAction = default;
    }

    /**
     * Checks the alien's reservation setting.
     * @return the reserve setting.
     */
    internal BattleActionType getReserveMode() =>
        _reserve;

    internal void freePatrolTarget()
    {
        if (_toNode != null)
        {
            _toNode.freeNode();
        }
    }

    /**
     * Saves the AI state to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
	    int fromNodeID = -1, toNodeID = -1;
	    if (_fromNode != null)
		    fromNodeID = _fromNode.getID();
	    if (_toNode != null)
		    toNodeID = _toNode.getID();

        var node = new YamlMappingNode
        {
            { "fromNode", fromNodeID.ToString() },
            { "toNode", toNodeID.ToString() },
            { "AIMode", _AIMode.ToString() },
            { "wasHitBy", new YamlSequenceNode(_wasHitBy.Select(x => new YamlScalarNode(x.ToString()))) }
        };
        return node;
    }

    /**
     * Resets the unsaved AI state.
     */
    internal void reset()
    {
	    // these variables are not saved in save() and also not initiated in think()
	    _escapeTUs = 0;
	    _ambushTUs = 0;
    }

    internal BattleUnit getTarget() =>
	    _aggroTarget;

    /*
     * sets the "was hit" flag to true.
     */
    internal void setWasHitBy(BattleUnit attacker)
    {
	    if (attacker.getFaction() != _unit.getFaction() && !getWasHitBy(attacker.getId()))
		    _wasHitBy.Add(attacker.getId());
    }

    /*
     * Gets whether the unit was hit.
     * @return if it was hit.
     */
    internal bool getWasHitBy(int attacker) =>
	    _wasHitBy.Contains(attacker);

    /**
     * Decides if it worth our while to create an explosion here.
     * @param targetPos The target's position.
     * @param attackingUnit The attacking unit.
     * @param radius How big the explosion will be.
     * @param diff Game difficulty.
     * @param grenade Is the explosion coming from a grenade?
     * @return True if it is worthwhile creating an explosion in the target position.
     */
    internal bool explosiveEfficacy(Position targetPos, BattleUnit attackingUnit, int radius, int diff, bool grenade = false)
    {
	    // i hate the player and i want him dead, but i don't want to piss him off.
	    Mod.Mod mod = _save.getBattleState().getGame().getMod();
	    if ((!grenade && _save.getTurn() < mod.getTurnAIUseBlaster()) ||
		     (grenade && _save.getTurn() < mod.getTurnAIUseGrenade()))
	    {
		    return false;
	    }

	    Tile targetTile = _save.getTile(targetPos);

	    // don't throw grenades at flying enemies.
	    if (grenade && targetPos.z > 0 && targetTile.hasNoFloor(_save.getTile(targetPos - new Position(0,0,1))))
	    {
		    return false;
	    }

	    if (diff == -1)
	    {
		    diff = _save.getBattleState().getGame().getSavedGame().getDifficultyCoefficient();
	    }
	    int distance = _save.getTileEngine().distance(attackingUnit.getPosition(), targetPos);
	    int injurylevel = attackingUnit.getBaseStats().health - attackingUnit.getHealth();
	    int desperation = (100 - attackingUnit.getMorale()) / 10;
	    int enemiesAffected = 0;
	    // if we're below 1/3 health, let's assume things are dire, and increase desperation.
	    if (injurylevel > (attackingUnit.getBaseStats().health / 3) * 2)
		    desperation += 3;

	    int efficacy = desperation;

	    // don't go kamikaze unless we're already doomed.
	    if (Math.Abs(attackingUnit.getPosition().z - targetPos.z) <= Options.battleExplosionHeight && distance <= radius)
	    {
		    efficacy -= 4;
	    }

	    // allow difficulty to have its influence
	    efficacy += diff/2;

	    // account for the unit we're targetting
	    BattleUnit target = targetTile.getUnit();
	    if (target != null && !targetTile.getDangerous())
	    {
		    ++enemiesAffected;
		    ++efficacy;
	    }

	    foreach (var i in _save.getUnits())
	    {
			    // don't grenade dead guys
		    if (!i.isOut() &&
			    // don't count ourself twice
			    i != attackingUnit &&
			    // don't count the target twice
			    i != target &&
			    // don't count units that probably won't be affected cause they're out of range
			    Math.Abs(i.getPosition().z - targetPos.z) <= Options.battleExplosionHeight &&
			    _save.getTileEngine().distance(i.getPosition(), targetPos) <= radius)
		    {
				    // don't count people who were already grenaded this turn
			    if (i.getTile().getDangerous() ||
				    // don't count units we don't know about
				    (i.getFaction() == _targetFaction && i.getTurnsSinceSpotted() > _intelligence))
				    continue;

			    // trace a line from the grenade origin to the unit we're checking against
			    Position voxelPosA = new Position ((targetPos.x * 16)+8, (targetPos.y * 16)+8, (targetPos.z * 24)+12);
			    Position voxelPosB = new Position ((i.getPosition().x * 16)+8, (i.getPosition().y * 16)+8, (i.getPosition().z * 24)+12);
			    var traj = new List<Position>();
			    int collidesWith = _save.getTileEngine().calculateLine(voxelPosA, voxelPosB, false, traj, target, true, false, i);

			    if (collidesWith == (int)VoxelType.V_UNIT && traj.First() / new Position(16,16,24) == i.getPosition())
			    {
				    if (i.getFaction() == _targetFaction)
				    {
					    ++enemiesAffected;
					    ++efficacy;
				    }
				    else if (i.getFaction() == attackingUnit.getFaction() || (attackingUnit.getFaction() == UnitFaction.FACTION_NEUTRAL && i.getFaction() == UnitFaction.FACTION_PLAYER))
					    efficacy -= 2; // friendlies count double
			    }
		    }
	    }
	    // don't throw grenades at single targets, unless morale is in the danger zone
	    // or we're halfway towards panicking while bleeding to death.
	    if (grenade && desperation < 6 && enemiesAffected < 2)
	    {
		    return false;
	    }
	    return (efficacy > 0 || enemiesAffected >= 10);
    }
}
