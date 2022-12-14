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

/* Refactoring tip : UnitDieBState */
/**
 * State for dying units.
 */
internal class UnitDieBState : BattleState
{
    BattleUnit _unit;
    ItemDamageType _damageType;
    bool _noSound, _noCorpse;
    int _extraFrame;

    /**
     * Sets up an UnitDieBState.
     * @param parent Pointer to the Battlescape.
     * @param unit Dying unit.
     * @param damageType Type of damage that caused the death.
     * @param noSound Whether to disable the death sound.
     * @param noCorpse Whether to disable the corpse spawn.
     */
    internal UnitDieBState(BattlescapeGame parent, BattleUnit unit, ItemDamageType damageType, bool noSound, bool noCorpse) : base(parent)
    {
        _unit = unit;
        _damageType = damageType;
        _noSound = noSound;
        _noCorpse = noCorpse;
        _extraFrame = 0;

        // don't show the "fall to death" animation when a unit is blasted with explosives or he is already unconscious
        if (_damageType == ItemDamageType.DT_HE || _unit.getStatus() == UnitStatus.STATUS_UNCONSCIOUS)
        {

            /********************************************************
            Proclamation from Lord Xenu:

            any unit that is going to skip its death pirouette
            MUST have its direction set to 3 first.

            Failure to comply is treason, and treason is punishable
            by death. (after being correctly oriented)

            ********************************************************/
            _unit.setDirection(3);


            _unit.startFalling();

            while (_unit.getStatus() == UnitStatus.STATUS_COLLAPSING)
            {
                _unit.keepFalling();
            }
            if (_parent.getSave().isBeforeGame())
            {
                if (!noCorpse)
                {
                    convertUnitToCorpse();
                }
                _extraFrame = 3; // shortcut to popState()
            }
        }
        else
        {
            if (_unit.getFaction() == UnitFaction.FACTION_PLAYER)
            {
                _parent.getMap().setUnitDying(true);
            }
            _parent.setStateInterval(BattlescapeState.DEFAULT_ANIM_SPEED);
            if (_unit.getDirection() != 3)
            {
                _parent.setStateInterval(BattlescapeState.DEFAULT_ANIM_SPEED / 3);
            }
        }

        _unit.clearVisibleTiles();
        _unit.clearVisibleUnits();
        _unit.freePatrolTarget();

        if (!_parent.getSave().isBeforeGame() && _unit.getFaction() == UnitFaction.FACTION_HOSTILE)
        {
            List<Node> nodes = _parent.getSave().getNodes();
            if (nodes == null) return; // this better not happen.

            foreach (var node in nodes)
            {
                if (!node.isDummy() && _parent.getSave().getTileEngine().distanceSq(node.getPosition(), _unit.getPosition()) < 4)
                {
                    node.setType(node.getType() | Node.TYPE_DANGEROUS);
                }
            }
        }
    }

    /**
     * Deletes the UnitDieBState.
     */
    ~UnitDieBState() { }

    /**
     * Converts unit to a corpse (item).
     */
    void convertUnitToCorpse()
    {
        Position lastPosition = _unit.getPosition();
        int size = _unit.getArmor().getSize();
        bool dropItems = (_unit.hasInventory() &&
            (!Options.weaponSelfDestruction ||
            (_unit.getOriginalFaction() != UnitFaction.FACTION_HOSTILE || _unit.getStatus() == UnitStatus.STATUS_UNCONSCIOUS)));

        if (!_noSound)
        {
            _parent.getSave().getBattleState().showPsiButton(false);
        }
        // remove the unconscious body item corresponding to this unit, and if it was being carried, keep track of what slot it was in
        if (lastPosition != new Position(-1, -1, -1))
        {
            _parent.getSave().removeUnconsciousBodyItem(_unit);
        }

        // move inventory from unit to the ground
        if (dropItems)
        {
            var itemsToKeep = new List<BattleItem>();
            foreach (var item in _unit.getInventory())
            {
                _parent.dropItem(lastPosition, item);
                if (!item.getRules().isFixed())
                {
                    item.setOwner(null);
                }
                else
                {
                    itemsToKeep.Add(item);
                }
            }

            _unit.getInventory().Clear();

            foreach (var item in itemsToKeep)
            {
                _unit.getInventory().Add(item);
            }
        }

        // remove unit-tile link
        _unit.setTile(null);

        if (lastPosition == new Position(-1, -1, -1)) // we're being carried
        {
            // replace the unconscious body item with a corpse in the carrying unit's inventory
            foreach (var item in _parent.getSave().getItems())
            {
                if (item.getUnit() == _unit)
                {
                    RuleItem corpseRules = _parent.getMod().getItem(_unit.getArmor().getCorpseBattlescape()[0], true); // we're in an inventory, so we must be a 1x1 unit
                    item.convertToCorpse(corpseRules);
                    break;
                }
            }
        }
        else
        {
            int i = size * size - 1;
            for (int y = size - 1; y >= 0; --y)
            {
                for (int x = size - 1; x >= 0; --x)
                {
                    var corpse = new BattleItem(_parent.getMod().getItem(_unit.getArmor().getCorpseBattlescape()[i], true), ref _parent.getSave().getCurrentItemId());
                    corpse.setUnit(_unit);
                    if (_parent.getSave().getTile(lastPosition + new Position(x, y, 0)).getUnit() == _unit) // check in case unit was displaced by another unit
                    {
                        _parent.getSave().getTile(lastPosition + new Position(x, y, 0)).setUnit(null);
                    }
                    _parent.dropItem(lastPosition + new Position(x, y, 0), corpse, true);
                    --i;
                }
            }
        }
    }
}
