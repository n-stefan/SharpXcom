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

using Action = SharpXcom.Engine.Action;

namespace SharpXcom.Menu;

/**
 * Load Game screen for listing info on available
 * saved games and loading them.
 */
internal class ListLoadState : ListGamesState
{
    TextButton _btnOld;

    /**
     * Initializes all the elements in the Load Game screen.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     */
    internal ListLoadState(OptionsOrigin origin) : base(origin, 0, true)
    {
        // Create objects
        _btnOld = new TextButton(80, 16, 60, 172);
        _btnCancel.setX(180);

        add(_btnOld, "button", "saveMenus");

        // Set up objects
        _txtTitle.setText(tr("STR_SELECT_GAME_TO_LOAD"));

        _btnOld.setText(tr("STR_ORIGINAL_XCOM"));
        _btnOld.onMouseClick(btnOldClick);

        centerAllSurfaces();
    }

    /**
     *
     */
    ~ListLoadState() { }

    /**
     * Switches to Original X-Com saves.
     * @param action Pointer to an action.
     */
    void btnOldClick(Action _) =>
        _game.pushState(new ListLoadOriginalState(_origin));
}
