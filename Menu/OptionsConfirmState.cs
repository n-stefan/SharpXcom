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

namespace SharpXcom.Menu;

/**
 * Confirmation window when Display Options
 * are changed.
 */
internal class OptionsConfirmState : State
{
    OptionsOrigin _origin;
    int _countdown;
    Window _window;
    TextButton _btnYes, _btnNo;
    Text _txtTitle, _txtTimer;
    Timer _timer;

    /**
     * Initializes all the elements in the Confirm Display Options screen.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     */
    internal OptionsConfirmState(OptionsOrigin origin)
    {
        _origin = origin;
        _countdown = 15;

        _screen = false;

        // Create objects
        _window = new Window(this, 216, 100, 52, 50, WindowPopup.POPUP_BOTH);
        _btnYes = new TextButton(50, 20, 70, 120);
        _btnNo = new TextButton(50, 20, 200, 120);
        _txtTitle = new Text(206, 20, 57, 70);
        _txtTimer = new Text(206, 20, 57, 100);
        _timer = new Timer(1000);

        // Set palette
        setInterface("optionsMenu", false, _game.getSavedGame() != null ? _game.getSavedGame().getSavedBattle() : null);

        add(_window, "confirmVideo", "optionsMenu");
        add(_btnYes, "confirmVideo", "optionsMenu");
        add(_btnNo, "confirmVideo", "optionsMenu");
        add(_txtTitle, "confirmVideo", "optionsMenu");
        add(_txtTimer, "confirmVideo", "optionsMenu");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

        _btnYes.setText(tr("STR_YES"));
        _btnYes.onMouseClick(btnYesClick);

        _btnNo.setText(tr("STR_NO"));
        _btnNo.onMouseClick(btnNoClick);
        // no keyboard shortcuts to make sure users can see the message

        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setWordWrap(true);
        _txtTitle.setText(tr("STR_DISPLAY_OPTIONS_CONFIRM"));

        _txtTimer.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTimer.setWordWrap(true);
        _txtTimer.setText(tr("STR_DISPLAY_OPTIONS_REVERT").arg(_countdown));

        if (_origin == OptionsOrigin.OPT_BATTLESCAPE)
        {
            applyBattlescapeTheme();
        }

        _timer.onTimer((StateHandler)countdown);
        _timer.start();
    }

    /**
     *
     */
    ~OptionsConfirmState() =>
        _timer = null;

    /**
     * Goes back to the Main Menu.
     * @param action Pointer to an action.
     */
    void btnYesClick(Action _)
    {
        _game.popState();
        OptionsBaseState.restart(_origin);
    }

    /**
     * Restores the original display options.
     * @param action Pointer to an action.
     */
    void btnNoClick(Action _)
    {
        Options.switchDisplay();
        Screen.updateScale(Options.battlescapeScale, ref Options.baseXBattlescape, ref Options.baseYBattlescape, _origin == OptionsOrigin.OPT_BATTLESCAPE);
        Screen.updateScale(Options.geoscapeScale, ref Options.baseXGeoscape, ref Options.baseYGeoscape, _origin != OptionsOrigin.OPT_BATTLESCAPE);
        Options.save();
        _game.getScreen().resetDisplay();
        _game.popState();
        OptionsBaseState.restart(_origin);
    }

    /**
     * Counts down the timer for reverting display options.
     */
    void countdown()
    {
        _countdown--;
        string ss = _countdown.ToString("D2");
        _txtTimer.setText(tr("STR_DISPLAY_OPTIONS_REVERT").arg(ss));
        if (_countdown == 0)
        {
            btnNoClick(null);
        }
    }

    /**
     * Runs the countdown timer.
     */
    protected override void think()
    {
	    base.think();

	    _timer.think(this, null);
    }
}
