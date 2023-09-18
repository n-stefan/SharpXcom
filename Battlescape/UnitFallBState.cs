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
}
