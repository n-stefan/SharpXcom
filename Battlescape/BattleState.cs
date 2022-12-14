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
 * This class sets the battlescape in a certain sub-state.
 * These states can be triggered by the player or the AI.
 */
internal class BattleState
{
    protected BattlescapeGame _parent;
    protected BattleAction _action;

    /**
     * Sets up a BattleState.
     * @param parent Pointer to the parent state.
     * @param action Struct containing info about the action.
     */
    BattleState(BattlescapeGame parent, BattleAction action)
    {
        _parent = parent;
        _action = action;
    }

    /**
     * Sets up a BattleState.
     * @param parent Pointer to the parent state.
     */
    internal BattleState(BattlescapeGame parent)
    {
        _parent = parent;

        _action.result = string.Empty;
        _action.targeting = false;
        _action.TU = 0;
    }

    /**
     * Deletes the BattleState.
     */
    ~BattleState() { }

    /**
     * Cancels the current BattleState.
     */
    internal virtual void cancel() { }

    /**
     * Start the current BattleState.
     */
    internal virtual void init() { }
}
