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
    AIModule(SavedBattleGame save, BattleUnit unit, Node node)
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
}
