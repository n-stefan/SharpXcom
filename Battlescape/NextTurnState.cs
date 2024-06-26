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
 * Screen which announces the next turn.
 */
internal class NextTurnState : State
{
    const int NEXT_TURN_DELAY = 500;

    SavedBattleGame _battleGame;
    BattlescapeState _state;
    Timer _timer;
    Window _window;
    Text _txtTitle, _txtTurn, _txtSide, _txtMessage;
    Surface _bg;

    /**
     * Initializes all the elements in the Next Turn screen.
     * @param game Pointer to the core game.
     * @param battleGame Pointer to the saved game.
     * @param state Pointer to the Battlescape state.
     */
    internal NextTurnState(SavedBattleGame battleGame, BattlescapeState state)
    {
        _battleGame = battleGame;
        _state = state;
        _timer = null;

        // Create objects
        int y = state.getMap().getMessageY();

        _window = new Window(this, 320, 200, 0, 0);
        _txtTitle = new Text(320, 17, 0, 68);
        _txtTurn = new Text(320, 17, 0, 92);
        _txtSide = new Text(320, 17, 0, 108);
        _txtMessage = new Text(320, 17, 0, 132);
        _bg = new Surface(_game.getScreen().getWidth(), _game.getScreen().getWidth(), 0, 0);

        // Set palette
        battleGame.setPaletteByDepth(this);

        add(_bg);
        add(_window);
        add(_txtTitle, "messageWindows", "battlescape");
        add(_txtTurn, "messageWindows", "battlescape");
        add(_txtSide, "messageWindows", "battlescape");
        add(_txtMessage, "messageWindows", "battlescape");

        centerAllSurfaces();

        _bg.setX(0);
        _bg.setY(0);
        SDL_Rect rect;
        rect.h = _bg.getHeight();
        rect.w = _bg.getWidth();
        rect.x = rect.y = 0;

        _bg.drawRect(ref rect, (byte)(Palette.blockOffset(0) + 15));
        // make this screen line up with the hidden movement screen
        _window.setY(y);
        _txtTitle.setY(y + 68);
        _txtTurn.setY(y + 92);
        _txtSide.setY(y + 108);
        _txtMessage.setY(y + 132);

        // Set up objects
        _window.setColor((byte)(Palette.blockOffset(0) - 1));
        _window.setHighContrast(true);
        _window.setBackground(_game.getMod().getSurface("TAC00.SCR"));

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setHighContrast(true);
        _txtTitle.setText(tr("STR_OPENXCOM"));

        _txtTurn.setBig();
        _txtTurn.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTurn.setHighContrast(true);
        string ss = tr("STR_TURN").arg(_battleGame.getTurn());
        if (battleGame.getTurnLimit() > 0)
        {
            ss = $"/{battleGame.getTurnLimit()}";
            if (battleGame.getTurnLimit() - _battleGame.getTurn() <= 3)
            {
                // gonna borrow the inventory's "over weight" colour when we're down to the last three turns
                _txtTurn.setColor((byte)_game.getMod().getInterface("inventory").getElement("weight").color2);
            }
        }
        _txtTurn.setText(ss);

        _txtSide.setBig();
        _txtSide.setAlign(TextHAlign.ALIGN_CENTER);
        _txtSide.setHighContrast(true);
        _txtSide.setText(tr("STR_SIDE").arg(tr((_battleGame.getSide() == UnitFaction.FACTION_PLAYER ? "STR_XCOM" : "STR_ALIENS"))));

        _txtMessage.setBig();
        _txtMessage.setAlign(TextHAlign.ALIGN_CENTER);
        _txtMessage.setHighContrast(true);
        _txtMessage.setText(tr("STR_PRESS_BUTTON_TO_CONTINUE"));

        _state.clearMouseScrollingState();

        if (Options.skipNextTurnScreen)
        {
            _timer = new Timer(NEXT_TURN_DELAY);
            _timer.onTimer((StateHandler)close);
            _timer.start();
        }
    }

    /**
     *
     */
    ~NextTurnState() =>
        _timer = null;

    /**
     * Closes the window.
     */
    void close()
    {
        _battleGame.getBattleGame().cleanupDeleted();
        _game.popState();

        int liveAliens = 0;
        int liveSoldiers = 0;
        _state.getBattleGame().tallyUnits(out liveAliens, out liveSoldiers);

        if ((_battleGame.getObjectiveType() != SpecialTileType.MUST_DESTROY && liveAliens == 0) || liveSoldiers == 0)      // not the final mission and all aliens dead.
        {
            _state.finishBattle(false, liveSoldiers);
        }
        else
        {
            _state.btnCenterClick(null);

            // Autosave every set amount of turns
            if ((_battleGame.getTurn() == 1 || _battleGame.getTurn() % Options.autosaveFrequency == 0) && _battleGame.getSide() == UnitFaction.FACTION_PLAYER)
            {
                _state.autosave();
            }
        }
    }

    /**
     * Keeps the timer running.
     */
    internal override void think()
    {
	    if (_timer != null)
	    {
		    _timer.think(this, null);
	    }
    }

    /**
     * Closes the window.
     * @param action Pointer to an action.
     */
    internal override void handle(Action action)
    {
	    base.handle(action);

	    if (action.getDetails().type == SDL_EventType.SDL_KEYDOWN || action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
	    {
		    close();
	    }
    }

    internal override void resize(ref int dX, ref int dY)
    {
	    base.resize(ref dX, ref dY);
	    _bg.setX(0);
	    _bg.setY(0);
    }
}
