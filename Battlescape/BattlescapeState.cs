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
 * along with OpenXcom.  If not, see <http:///www.gnu.org/licenses/>.
 */

namespace SharpXcom.Battlescape;

/**
 * Battlescape screen which shows the tactical battle.
 */
internal class BattlescapeState : State
{
    internal const int DEFAULT_ANIM_SPEED = 100;

    Map _map;
    BattlescapeButton _btnEndTurn, _btnAbort, _btnLaunch, _btnPsi, _reserve;
    BattlescapeGame _battleGame;
    WarningMessage _warning;
    Engine.Timer _animTimer, _gameTimer;

    //TODO: ctor, dtor

    /**
     * Gets pointer to the map. Some states need this info.
     * @return Pointer to map.
     */
    internal Map getMap() =>
	    _map;

    /**
     * Gets pointer to the game. Some states need this info.
     * @return Pointer to game.
     */
    internal Game getGame() =>
	    _game;

    /**
     * Shows the launch button.
     * @param show Show launch button?
     */
    internal void showLaunchButton(bool show) =>
        _btnLaunch.setVisible(show);

    /**
     * Returns a pointer to the battlegame, in case we need its functions.
     */
    internal BattlescapeGame getBattleGame() =>
        _battleGame;

    /**
     * Shows a warning message.
     * @param message Warning message.
     */
    internal void warning(string message) =>
        _warning.showMessage(tr(message));

    /**
     * Shows the PSI button.
     * @param show Show PSI button?
     */
    internal void showPsiButton(bool show) =>
        _btnPsi.setVisible(show);

    /**
     * Sets the timer interval for think() calls of the state.
     * @param interval An interval in ms.
     */
    internal void setStateInterval(uint interval) =>
        _gameTimer.setInterval(interval);
}
