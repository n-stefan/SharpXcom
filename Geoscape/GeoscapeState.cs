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

namespace SharpXcom.Geoscape;

/**
 * Geoscape screen which shows an overview of
 * the world and lets the player manage the game.
 */
internal class GeoscapeState : State
{
    bool _pause, _zoomInEffectDone, _zoomOutEffectDone;
    uint _minimizedDogfights;
    Surface _bg, _sideLine, _sidebar;
    Globe _globe;
    TextButton _btnIntercept, _btnBases, _btnGraphs, _btnUfopaedia, _btnOptions, _btnFunding;
    TextButton _btn5Secs, _btn1Min, _btn5Mins, _btn30Mins, _btn1Hour, _btn1Day;
    InteractiveSurface _btnRotateLeft, _btnRotateRight, _btnRotateUp, _btnRotateDown, _btnZoomIn, _btnZoomOut;
    TextButton _sideTop, _sideBottom;
    Text _txtFunds, _txtHour, _txtHourSep, _txtMin, _txtMinSep, _txtSec, _txtWeekday, _txtDay, _txtMonth, _txtYear;
    TextButton _timeSpeed;
    Timer _gameTimer, _zoomInEffectTimer, _zoomOutEffectTimer, _dogfightStartTimer, _dogfightTimer;
    Text _txtDebug;
    List<DogfightState> _dogfights, _dogfightsToBeStarted;
    List<State> _popups;

    /**
     * Initializes all the elements in the Geoscape screen.
     * @param game Pointer to the core game.
     */
    internal GeoscapeState()
    {
        _pause = false;
        _zoomInEffectDone = false;
        _zoomOutEffectDone = false;
        _minimizedDogfights = 0;

        int screenWidth = Options.baseXGeoscape;
        int screenHeight = Options.baseYGeoscape;

        // Create objects
        Surface hd = _game.getMod().getSurface("ALTGEOBORD.SCR");
        _bg = new Surface(hd.getWidth(), hd.getHeight(), 0, 0);
        _sideLine = new Surface(64, screenHeight, screenWidth - 64, 0);
        _sidebar = new Surface(64, 200, screenWidth - 64, screenHeight / 2 - 100);

        _globe = new Globe(_game, (screenWidth - 64) / 2, screenHeight / 2, screenWidth - 64, screenHeight, 0, 0);
        _bg.setX((_globe.getWidth() - _bg.getWidth()) / 2);
        _bg.setY((_globe.getHeight() - _bg.getHeight()) / 2);

        _btnIntercept = new TextButton(63, 11, screenWidth - 63, screenHeight / 2 - 100);
        _btnBases = new TextButton(63, 11, screenWidth - 63, screenHeight / 2 - 88);
        _btnGraphs = new TextButton(63, 11, screenWidth - 63, screenHeight / 2 - 76);
        _btnUfopaedia = new TextButton(63, 11, screenWidth - 63, screenHeight / 2 - 64);
        _btnOptions = new TextButton(63, 11, screenWidth - 63, screenHeight / 2 - 52);
        _btnFunding = new TextButton(63, 11, screenWidth - 63, screenHeight / 2 - 40);

        _btn5Secs = new TextButton(31, 13, screenWidth - 63, screenHeight / 2 + 12);
        _btn1Min = new TextButton(31, 13, screenWidth - 31, screenHeight / 2 + 12);
        _btn5Mins = new TextButton(31, 13, screenWidth - 63, screenHeight / 2 + 26);
        _btn30Mins = new TextButton(31, 13, screenWidth - 31, screenHeight / 2 + 26);
        _btn1Hour = new TextButton(31, 13, screenWidth - 63, screenHeight / 2 + 40);
        _btn1Day = new TextButton(31, 13, screenWidth - 31, screenHeight / 2 + 40);

        _btnRotateLeft = new InteractiveSurface(12, 10, screenWidth - 61, screenHeight / 2 + 76);
        _btnRotateRight = new InteractiveSurface(12, 10, screenWidth - 37, screenHeight / 2 + 76);
        _btnRotateUp = new InteractiveSurface(13, 12, screenWidth - 49, screenHeight / 2 + 62);
        _btnRotateDown = new InteractiveSurface(13, 12, screenWidth - 49, screenHeight / 2 + 87);
        _btnZoomIn = new InteractiveSurface(23, 23, screenWidth - 25, screenHeight / 2 + 56);
        _btnZoomOut = new InteractiveSurface(13, 17, screenWidth - 20, screenHeight / 2 + 82);

        int height = (screenHeight - Screen.ORIGINAL_HEIGHT) / 2 + 10;
        _sideTop = new TextButton(63, height, screenWidth - 63, _sidebar.getY() - height - 1);
        _sideBottom = new TextButton(63, height, screenWidth - 63, _sidebar.getY() + _sidebar.getHeight() + 1);

        _txtHour = new Text(20, 16, screenWidth - 61, screenHeight / 2 - 26);
        _txtHourSep = new Text(4, 16, screenWidth - 41, screenHeight / 2 - 26);
        _txtMin = new Text(20, 16, screenWidth - 37, screenHeight / 2 - 26);
        _txtMinSep = new Text(4, 16, screenWidth - 17, screenHeight / 2 - 26);
        _txtSec = new Text(11, 8, screenWidth - 13, screenHeight / 2 - 20);
        _txtWeekday = new Text(59, 8, screenWidth - 61, screenHeight / 2 - 13);
        _txtDay = new Text(29, 8, screenWidth - 61, screenHeight / 2 - 6);
        _txtMonth = new Text(29, 8, screenWidth - 32, screenHeight / 2 - 6);
        _txtYear = new Text(59, 8, screenWidth - 61, screenHeight / 2 + 1);
        _txtFunds = new Text(59, 8, screenWidth - 61, screenHeight / 2 - 27);

        _timeSpeed = _btn5Secs;
        _gameTimer = new Timer((uint)Options.geoClockSpeed);

        _zoomInEffectTimer = new Timer((uint)Options.dogfightSpeed);
        _zoomOutEffectTimer = new Timer((uint)Options.dogfightSpeed);
        _dogfightStartTimer = new Timer((uint)Options.dogfightSpeed);
        _dogfightTimer = new Timer((uint)Options.dogfightSpeed);

        _txtDebug = new Text(200, 32, 0, 0);

        // Set palette
        setInterface("geoscape");

        add(_bg);
        add(_sideLine);
        add(_sidebar);
        add(_globe);

        add(_btnIntercept, "button", "geoscape");
        add(_btnBases, "button", "geoscape");
        add(_btnGraphs, "button", "geoscape");
        add(_btnUfopaedia, "button", "geoscape");
        add(_btnOptions, "button", "geoscape");
        add(_btnFunding, "button", "geoscape");

        add(_btn5Secs, "button", "geoscape");
        add(_btn1Min, "button", "geoscape");
        add(_btn5Mins, "button", "geoscape");
        add(_btn30Mins, "button", "geoscape");
        add(_btn1Hour, "button", "geoscape");
        add(_btn1Day, "button", "geoscape");

        add(_btnRotateLeft);
        add(_btnRotateRight);
        add(_btnRotateUp);
        add(_btnRotateDown);
        add(_btnZoomIn);
        add(_btnZoomOut);

        add(_sideTop, "button", "geoscape");
        add(_sideBottom, "button", "geoscape");

        add(_txtFunds, "text", "geoscape");
        add(_txtHour, "text", "geoscape");
        add(_txtHourSep, "text", "geoscape");
        add(_txtMin, "text", "geoscape");
        add(_txtMinSep, "text", "geoscape");
        add(_txtSec, "text", "geoscape");
        add(_txtWeekday, "text", "geoscape");
        add(_txtDay, "text", "geoscape");
        add(_txtMonth, "text", "geoscape");
        add(_txtYear, "text", "geoscape");

        add(_txtDebug, "text", "geoscape");

        // Set up objects
        Surface geobord = _game.getMod().getSurface("GEOBORD.SCR");
        geobord.setX(_sidebar.getX() - geobord.getWidth() + _sidebar.getWidth());
        geobord.setY(_sidebar.getY());
        _sidebar.copy(geobord);
        _game.getMod().getSurface("ALTGEOBORD.SCR").blit(_bg);

        _sideLine.drawRect(0, 0, (short)_sideLine.getWidth(), (short)_sideLine.getHeight(), 15);

        _btnIntercept.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btnIntercept.setText(tr("STR_INTERCEPT"));
        _btnIntercept.onMouseClick(btnInterceptClick);
        _btnIntercept.onKeyboardPress(btnInterceptClick, Options.keyGeoIntercept);
        _btnIntercept.setGeoscapeButton(true);

        _btnBases.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btnBases.setText(tr("STR_BASES"));
        _btnBases.onMouseClick(btnBasesClick);
        _btnBases.onKeyboardPress(btnBasesClick, Options.keyGeoBases);
        _btnBases.setGeoscapeButton(true);

        _btnGraphs.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btnGraphs.setText(tr("STR_GRAPHS"));
        _btnGraphs.onMouseClick(btnGraphsClick);
        _btnGraphs.onKeyboardPress(btnGraphsClick, Options.keyGeoGraphs);
        _btnGraphs.setGeoscapeButton(true);

        _btnUfopaedia.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btnUfopaedia.setText(tr("STR_UFOPAEDIA_UC"));
        _btnUfopaedia.onMouseClick(btnUfopaediaClick);
        _btnUfopaedia.onKeyboardPress(btnUfopaediaClick, Options.keyGeoUfopedia);
        _btnUfopaedia.setGeoscapeButton(true);

        _btnOptions.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btnOptions.setText(tr("STR_OPTIONS_UC"));
        _btnOptions.onMouseClick(btnOptionsClick);
        _btnOptions.onKeyboardPress(btnOptionsClick, Options.keyGeoOptions);
        _btnOptions.setGeoscapeButton(true);

        _btnFunding.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btnFunding.setText(tr("STR_FUNDING_UC"));
        _btnFunding.onMouseClick(btnFundingClick);
        _btnFunding.onKeyboardPress(btnFundingClick, Options.keyGeoFunding);
        _btnFunding.setGeoscapeButton(true);

        _btn5Secs.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btn5Secs.setBig();
        _btn5Secs.setText(tr("STR_5_SECONDS"));
        _btn5Secs.setGroup(_timeSpeed);
        _btn5Secs.onKeyboardPress(btnTimerClick, Options.keyGeoSpeed1);
        _btn5Secs.setGeoscapeButton(true);

        _btn1Min.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btn1Min.setBig();
        _btn1Min.setText(tr("STR_1_MINUTE"));
        _btn1Min.setGroup(_timeSpeed);
        _btn1Min.onKeyboardPress(btnTimerClick, Options.keyGeoSpeed2);
        _btn1Min.setGeoscapeButton(true);

        _btn5Mins.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btn5Mins.setBig();
        _btn5Mins.setText(tr("STR_5_MINUTES"));
        _btn5Mins.setGroup(_timeSpeed);
        _btn5Mins.onKeyboardPress(btnTimerClick, Options.keyGeoSpeed3);
        _btn5Mins.setGeoscapeButton(true);

        _btn30Mins.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btn30Mins.setBig();
        _btn30Mins.setText(tr("STR_30_MINUTES"));
        _btn30Mins.setGroup(_timeSpeed);
        _btn30Mins.onKeyboardPress(btnTimerClick, Options.keyGeoSpeed4);
        _btn30Mins.setGeoscapeButton(true);

        _btn1Hour.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btn1Hour.setBig();
        _btn1Hour.setText(tr("STR_1_HOUR"));
        _btn1Hour.setGroup(_timeSpeed);
        _btn1Hour.onKeyboardPress(btnTimerClick, Options.keyGeoSpeed5);
        _btn1Hour.setGeoscapeButton(true);

        _btn1Day.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btn1Day.setBig();
        _btn1Day.setText(tr("STR_1_DAY"));
        _btn1Day.setGroup(_timeSpeed);
        _btn1Day.onKeyboardPress(btnTimerClick, Options.keyGeoSpeed6);
        _btn1Day.setGeoscapeButton(true);

        _sideBottom.setGeoscapeButton(true);
        _sideTop.setGeoscapeButton(true);

        _btnRotateLeft.onMousePress(btnRotateLeftPress);
        _btnRotateLeft.onMouseRelease(btnRotateLeftRelease);
        _btnRotateLeft.onKeyboardPress(btnRotateLeftPress, Options.keyGeoLeft);
        _btnRotateLeft.onKeyboardRelease(btnRotateLeftRelease, Options.keyGeoLeft);

        _btnRotateRight.onMousePress(btnRotateRightPress);
        _btnRotateRight.onMouseRelease(btnRotateRightRelease);
        _btnRotateRight.onKeyboardPress(btnRotateRightPress, Options.keyGeoRight);
        _btnRotateRight.onKeyboardRelease(btnRotateRightRelease, Options.keyGeoRight);

        _btnRotateUp.onMousePress(btnRotateUpPress);
        _btnRotateUp.onMouseRelease(btnRotateUpRelease);
        _btnRotateUp.onKeyboardPress(btnRotateUpPress, Options.keyGeoUp);
        _btnRotateUp.onKeyboardRelease(btnRotateUpRelease, Options.keyGeoUp);

        _btnRotateDown.onMousePress(btnRotateDownPress);
        _btnRotateDown.onMouseRelease(btnRotateDownRelease);
        _btnRotateDown.onKeyboardPress(btnRotateDownPress, Options.keyGeoDown);
        _btnRotateDown.onKeyboardRelease(btnRotateDownRelease, Options.keyGeoDown);

        _btnZoomIn.onMouseClick(btnZoomInLeftClick, (byte)SDL_BUTTON_LEFT);
        _btnZoomIn.onMouseClick(btnZoomInRightClick, (byte)SDL_BUTTON_RIGHT);
        _btnZoomIn.onKeyboardPress(btnZoomInLeftClick, Options.keyGeoZoomIn);

        _btnZoomOut.onMouseClick(btnZoomOutLeftClick, (byte)SDL_BUTTON_LEFT);
        _btnZoomOut.onMouseClick(btnZoomOutRightClick, (byte)SDL_BUTTON_RIGHT);
        _btnZoomOut.onKeyboardPress(btnZoomOutLeftClick, Options.keyGeoZoomOut);

        _txtFunds.setAlign(TextHAlign.ALIGN_CENTER);
        _txtFunds.setVisible(Options.showFundsOnGeoscape);

        _txtHour.setBig();
        _txtHour.setAlign(TextHAlign.ALIGN_RIGHT);

        _txtHourSep.setBig();
        _txtHourSep.setText(":");

        _txtMin.setBig();

        _txtMinSep.setBig();
        _txtMinSep.setText(":");

        _txtWeekday.setAlign(TextHAlign.ALIGN_CENTER);

        _txtDay.setAlign(TextHAlign.ALIGN_CENTER);

        _txtMonth.setAlign(TextHAlign.ALIGN_CENTER);

        _txtYear.setAlign(TextHAlign.ALIGN_CENTER);

        if (Options.showFundsOnGeoscape)
        {
            _txtHour.setY(_txtHour.getY() + 6);
            _txtHour.setSmall();
            _txtHourSep.setY(_txtHourSep.getY() + 6);
            _txtHourSep.setSmall();
            _txtMin.setY(_txtMin.getY() + 6);
            _txtMin.setSmall();
            _txtMinSep.setX(_txtMinSep.getX() - 10);
            _txtMinSep.setY(_txtMinSep.getY() + 6);
            _txtMinSep.setSmall();
            _txtSec.setX(_txtSec.getX() - 10);
        }

        _gameTimer.onTimer((StateHandler)timeAdvance);
        _gameTimer.start();

        _zoomInEffectTimer.onTimer((StateHandler)zoomInEffect);
        _zoomOutEffectTimer.onTimer((StateHandler)zoomOutEffect);
        _dogfightStartTimer.onTimer((StateHandler)startDogfight);
        _dogfightTimer.onTimer((StateHandler)handleDogfights);

        timeDisplay();
    }

    /**
     * Deletes timers.
     */
    ~GeoscapeState()
    {
        _gameTimer = null;
        _zoomInEffectTimer = null;
        _zoomOutEffectTimer = null;
        _dogfightStartTimer = null;
        _dogfightTimer = null;

        _dogfights.Clear();
        _dogfightsToBeStarted.Clear();
    }

    bool buttonsDisabled() =>
        _zoomInEffectTimer.isRunning() || _zoomOutEffectTimer.isRunning();

    /**
     * Opens the Intercept window.
     * @param action Pointer to an action.
     */
    void btnInterceptClick(Action _)
    {
        if (buttonsDisabled())
        {
            return;
        }
        _game.pushState(new InterceptState(_globe));
    }

    /**
     * Goes to the Basescape screen.
     * @param action Pointer to an action.
     */
    void btnBasesClick(Action _)
    {
        if (buttonsDisabled())
        {
            return;
        }
        timerReset();
        if (_game.getSavedGame().getBases().Any())
        {
            _game.pushState(new BasescapeState(_game.getSavedGame().getSelectedBase(), _globe));
        }
        else
        {
            _game.pushState(new BasescapeState(null, _globe));
        }
    }

    /**
     * Goes to the Graphs screen.
     * @param action Pointer to an action.
     */
    void btnGraphsClick(Action _)
    {
        if (buttonsDisabled())
        {
            return;
        }
        _game.pushState(new GraphsState());
    }

    /**
     * Goes to the Ufopaedia window.
     * @param action Pointer to an action.
     */
    void btnUfopaediaClick(Action _)
    {
        if (buttonsDisabled())
        {
            return;
        }
        Ufopaedia.Ufopaedia.open(_game);
    }

    /**
     * Opens the Options window.
     * @param action Pointer to an action.
     */
    void btnOptionsClick(Action _)
    {
        if (buttonsDisabled())
        {
            return;
        }
        _game.pushState(new PauseState(OptionsOrigin.OPT_GEOSCAPE));
    }

    /**
     * Goes to the Funding screen.
     * @param action Pointer to an action.
     */
    void btnFundingClick(Action _)
    {
        if (buttonsDisabled())
        {
            return;
        }
        _game.pushState(new FundingState());
    }

    /**
     * Handler for clicking on a timer button.
     * @param action pointer to the mouse action.
     */
    void btnTimerClick(Action action)
    {
        var ev = new SDL_Event();
        ev.type = SDL_EventType.SDL_MOUSEBUTTONDOWN;
        ev.button.button = (byte)SDL_BUTTON_LEFT;
        var a = new Action(ev, 0.0, 0.0, 0, 0);
        action.getSender().mousePress(a, this);
    }

    /**
     * Starts rotating the globe to the left.
     * @param action Pointer to an action.
     */
    void btnRotateLeftPress(Action _) =>
        _globe.rotateLeft();

    /**
     * Stops rotating the globe to the left.
     * @param action Pointer to an action.
     */
    void btnRotateLeftRelease(Action _) =>
        _globe.rotateStopLon();

    /**
     * Starts rotating the globe to the right.
     * @param action Pointer to an action.
     */
    void btnRotateRightPress(Action _) =>
        _globe.rotateRight();

    /**
     * Stops rotating the globe to the right.
     * @param action Pointer to an action.
     */
    void btnRotateRightRelease(Action _) =>
        _globe.rotateStopLon();

    /**
     * Starts rotating the globe upwards.
     * @param action Pointer to an action.
     */
    void btnRotateUpPress(Action _) =>
        _globe.rotateUp();

    /**
     * Stops rotating the globe upwards.
     * @param action Pointer to an action.
     */
    void btnRotateUpRelease(Action _) =>
        _globe.rotateStopLat();

    /**
     * Starts rotating the globe downwards.
     * @param action Pointer to an action.
     */
    void btnRotateDownPress(Action _) =>
        _globe.rotateDown();

    /**
     * Stops rotating the globe downwards.
     * @param action Pointer to an action.
     */
    void btnRotateDownRelease(Action _) =>
        _globe.rotateStopLat();

    /**
     * Zooms into the globe.
     * @param action Pointer to an action.
     */
    void btnZoomInLeftClick(Action _) =>
        _globe.zoomIn();

    /**
     * Zooms the globe maximum.
     * @param action Pointer to an action.
     */
    void btnZoomInRightClick(Action _) =>
        _globe.zoomMax();

    /**
     * Zooms out of the globe.
     * @param action Pointer to an action.
     */
    void btnZoomOutLeftClick(Action _) =>
        _globe.zoomOut();

    /**
     * Zooms the globe minimum.
     * @param action Pointer to an action.
     */
    void btnZoomOutRightClick(Action _) =>
        _globe.zoomMin();

    /**
     * Advances the game timer according to
     * the timer speed set, and calls the respective
     * triggers. The timer always advances in "5 secs"
     * cycles, regardless of the speed, otherwise it might
     * skip important steps. Instead, it just keeps advancing
     * the timer until the next speed step (eg. the next day
     * on 1 Day speed) or until an event occurs, since updating
     * the screen on each step would become cumbersomely slow.
     */
    void timeAdvance()
    {
        int timeSpan = 0;
        if (_timeSpeed == _btn5Secs)
        {
            timeSpan = 1;
        }
        else if (_timeSpeed == _btn1Min)
        {
            timeSpan = 12;
        }
        else if (_timeSpeed == _btn5Mins)
        {
            timeSpan = 12 * 5;
        }
        else if (_timeSpeed == _btn30Mins)
        {
            timeSpan = 12 * 5 * 6;
        }
        else if (_timeSpeed == _btn1Hour)
        {
            timeSpan = 12 * 5 * 6 * 2;
        }
        else if (_timeSpeed == _btn1Day)
        {
            timeSpan = 12 * 5 * 6 * 2 * 24;
        }

        for (int i = 0; i < timeSpan && !_pause; ++i)
        {
            TimeTrigger trigger;
            trigger = _game.getSavedGame().getTime().advance();
            switch (trigger)
            {
                case TimeTrigger.TIME_1MONTH:
                    time1Month();
                    goto case TimeTrigger.TIME_1DAY;
                case TimeTrigger.TIME_1DAY:
                    time1Day();
                    goto case TimeTrigger.TIME_1HOUR;
                case TimeTrigger.TIME_1HOUR:
                    time1Hour();
                    goto case TimeTrigger.TIME_30MIN;
                case TimeTrigger.TIME_30MIN:
                    time30Minutes();
                    goto case TimeTrigger.TIME_10MIN;
                case TimeTrigger.TIME_10MIN:
                    time10Minutes();
                    goto case TimeTrigger.TIME_5SEC;
                case TimeTrigger.TIME_5SEC:
                    time5Seconds();
                    break;
            }
        }

        _pause = _dogfightsToBeStarted.Any() || _zoomInEffectTimer.isRunning() || _zoomOutEffectTimer.isRunning();

        timeDisplay();
        _globe.draw();
    }

    /**
     * Updates the Geoscape clock with the latest
     * game time and date in human-readable format. (+Funds)
     */
    void timeDisplay()
    {
        if (Options.showFundsOnGeoscape)
        {
            _txtFunds.setText(Unicode.formatFunding(_game.getSavedGame().getFunds()));
        }

        string ss = _game.getSavedGame().getTime().getSecond().ToString("D2");
        _txtSec.setText(ss);

        string ss2 = _game.getSavedGame().getTime().getMinute().ToString("D2");
        _txtMin.setText(ss2);

        string ss3 = _game.getSavedGame().getTime().getHour().ToString();
        _txtHour.setText(ss3);

        string ss4 = _game.getSavedGame().getTime().getDayString(_game.getLanguage());
        _txtDay.setText(ss4);

        _txtWeekday.setText(tr(_game.getSavedGame().getTime().getWeekdayString()));

        _txtMonth.setText(tr(_game.getSavedGame().getTime().getMonthString()));

        string ss5 = _game.getSavedGame().getTime().getYear().ToString();
        _txtYear.setText(ss5);
    }

    /**
     * Zoom in effect for dogfights.
     */
    void zoomInEffect()
    {
        if (_globe.zoomDogfightIn())
        {
            _zoomInEffectDone = true;
            _zoomInEffectTimer.stop();
        }
    }

    /**
     * Zoom out effect for dogfights.
     */
    void zoomOutEffect()
    {
        if (_globe.zoomDogfightOut())
        {
            _zoomOutEffectDone = true;
            _zoomOutEffectTimer.stop();
            init();
        }
    }

    /**
     * Starts a new dogfight.
     */
    void startDogfight()
    {
        if (_globe.getZoom() < 3)
        {
            if (!_zoomInEffectTimer.isRunning())
            {
                _globe.saveZoomDogfight();
                _globe.rotateStop();
                _zoomInEffectTimer.start();
            }
        }
        else
        {
            _dogfightStartTimer.stop();
            _zoomInEffectTimer.stop();
            _dogfightTimer.start();
            timerReset();
            while (_dogfightsToBeStarted.Any())
            {
                _dogfights.Add(_dogfightsToBeStarted.Last());
                _dogfightsToBeStarted.RemoveAt(_dogfightsToBeStarted.Count - 1);
                _dogfights.Last().setInterceptionNumber(getFirstFreeDogfightSlot());
                _dogfights.Last().setInterceptionsCount((uint)(_dogfights.Count + _dogfightsToBeStarted.Count));
            }
            // Set correct number of interceptions for every dogfight.
            foreach (var d in _dogfights)
            {
                d.setInterceptionsCount((uint)_dogfights.Count);
            }
        }
    }

    /**
     * Dogfight logic. Moved here to have the code clean.
     */
    void handleDogfights()
    {
        // Handle dogfights logic.
        _minimizedDogfights = 0;

        foreach (var i in _dogfights)
        {
            i.getUfo().setInterceptionProcessed(false);
        }
        var d = 0;
        while (d < _dogfights.Count)
        {
            if (_dogfights[d].isMinimized())
            {
                if (_dogfights[d].getWaitForPoly() && _globe.insideLand(_dogfights[d].getUfo().getLongitude(), _dogfights[d].getUfo().getLatitude()))
                {
                    _dogfights[d].setMinimized(false);
                    _dogfights[d].setWaitForPoly(false);
                }
                else if (_dogfights[d].getWaitForAltitude() && _dogfights[d].getUfo().getAltitudeInt() <= _dogfights[d].getCraft().getRules().getMaxAltitude())
                {
                    _dogfights[d].setMinimized(false);
                    _dogfights[d].setWaitForAltitude(false);
                }
                else
                {
                    _minimizedDogfights++;
                }
            }
            else
            {
                _globe.rotateStop();
            }
            _dogfights[d].think();
            if (_dogfights[d].dogfightEnded())
            {
                if (_dogfights[d].isMinimized())
                {
                    _minimizedDogfights--;
                }
                _dogfights.RemoveAt(d);
            }
            else
            {
                ++d;
            }
        }
        if (!_dogfights.Any())
        {
            _dogfightTimer.stop();
            _zoomOutEffectTimer.start();
        }
    }

    /**
     * Returns the first free dogfight slot.
     * @return free slot
     */
    int getFirstFreeDogfightSlot()
    {
        int slotNo = 1;
        foreach (var d in _dogfights)
        {
            if (d.getInterceptionNumber() == slotNo)
            {
                ++slotNo;
            }
        }
        return slotNo;
    }

    /**
     * Slows down the timer back to minimum speed,
     * for when important events occur.
     */
    internal void timerReset()
    {
        var ev = new SDL_Event();
        ev.button.button = (byte)SDL_BUTTON_LEFT;
        var act = new Action(ev, _game.getScreen().getXScale(), _game.getScreen().getYScale(), _game.getScreen().getCursorTopBlackBand(), _game.getScreen().getCursorLeftBlackBand());
        _btn5Secs.mousePress(act, this);
    }

    /**
     * Takes care of any game logic that has to
     * run every game month, like funding.
     */
    void time1Month()
    {
        _game.getSavedGame().addMonth();

        // Determine alien mission for this month.
        determineAlienMissions();

        // Handle Psi-Training and initiate a new retaliation mission, if applicable
        bool psi = false;
        if (!Options.anytimePsiTraining)
        {
            foreach (var b in _game.getSavedGame().getBases())
            {
                if (b.getAvailablePsiLabs() > 0)
                {
                    psi = true;
                    foreach (var s in b.getSoldiers())
                    {
                        if (s.isInPsiTraining())
                        {
                            s.trainPsi();
                            s.calcStatString(_game.getMod().getStatStrings(), (Options.psiStrengthEval && _game.getSavedGame().isResearched(_game.getMod().getPsiRequirements())));
                        }
                    }
                }
            }
        }

        // Handle funding
        timerReset();
        _game.getSavedGame().monthlyFunding();
        popup(new MonthlyReportState(psi, _globe));

        // Handle Xcom Operatives discovering bases
        if (_game.getSavedGame().getAlienBases().Any() && RNG.percent(20))
        {
            foreach (var b in _game.getSavedGame().getAlienBases())
            {
                if (!b.isDiscovered())
                {
                    b.setDiscovered(true);
                    popup(new AlienBaseState(b, this));
                    break;
                }
            }
        }
    }

    /**
     * Adds a new popup window to the queue
     * (this prevents popups from overlapping)
     * and pauses the game timer respectively.
     * @param state Pointer to popup state.
     */
    internal void popup(State state)
    {
        _pause = true;
        _popups.Add(state);
    }

    /**
     * Determine the alien missions to start this month.
     */
    void determineAlienMissions()
    {
        SavedGame save = _game.getSavedGame();
        AlienStrategy strategy = save.getAlienStrategy();
        Mod.Mod mod = _game.getMod();
        int month = _game.getSavedGame().getMonthsPassed();
        var availableMissions = new List<RuleMissionScript>();
        var conditions = new Dictionary<int, bool>();

        // well, here it is, ladies and gents, the nuts and bolts behind the geoscape mission scheduling.

        // first we need to build a list of "valid" commands
        foreach (var i in mod.getMissionScriptList())
        {
            RuleMissionScript command = mod.getMissionScript(i);

            // level one condition check: make sure we're within our time constraints
            if (command.getFirstMonth() <= month &&
                (command.getLastMonth() >= month || command.getLastMonth() == -1) &&
                // make sure we haven't hit our run limit, if we have one
                (command.getMaxRuns() == -1 || command.getMaxRuns() > strategy.getMissionsRun(command.getVarName())) &&
                // and make sure we satisfy the difficulty restrictions
                command.getMinDifficulty() <= (int)save.getDifficulty())
            {
                // level two condition check: make sure we meet any research requirements, if any.
                bool triggerHappy = true;
                foreach (var j in command.getResearchTriggers())
                {
                    triggerHappy = (save.isResearched(j.Key) == j.Value);
                    if (!triggerHappy) break;
                }
                // levels one and two passed: insert this command into the array.
                if (triggerHappy)
                {
                    availableMissions.Add(command);
                }
            }
        }

        // start processing command array.
        foreach (var i in availableMissions)
        {
            RuleMissionScript command = i;
            bool process = true;
            bool success = false;
            // level three condition check: make sure our conditionals are met, if any. this list is dynamic, and must be checked here.
            var conditionals = command.getConditionals();
            for (int j = 0; j < conditionals.Count && process; ++j)
            {
                bool found = conditions.TryGetValue(Math.Abs(conditionals[j]), out bool condition);
                // just an FYI: if you add a 0 to your conditionals, this flag will never resolve to true, and your command will never run.
                process = (!found || (condition == true && conditionals[j] > 0) || (condition == false && conditionals[j] < 0));
            }
            if (command.getLabel() > 0 && conditions.ContainsKey(command.getLabel()))
            {
                string ss = $"Mission generator encountered an error: multiple commands: {command.getType()} and ";
                foreach (RuleMissionScript j in availableMissions)
                {
                    if (command.getLabel() == j.getLabel() && j != i)
                    {
                        ss = $"{ss}{j.getType()}, ";
                    }
                }
                ss = $"{ss}are sharing the same label: {command.getLabel()}";
                throw new Exception(ss);
            }
            // level four condition check: does random chance favour this command's execution?
            if (process && RNG.percent(command.getExecutionOdds()))
            {
                // good news, little command pointer! you're FDA approved! off to the main processing facility with you!
                success = processCommand(command);
            }
            if (command.getLabel() > 0)
            {
                // tsk, tsk. you really should be careful with these unique labels, they're supposed to be unique.
                if (conditions.ContainsKey(command.getLabel()))
                {
                    throw new Exception("Error in mission scripts: " + command.getType() + ". Two or more commands sharing the same label. That's bad, Mmmkay?");
                }
                // keep track of what happened to this command, so others may reference it.
                conditions[command.getLabel()] = success;
            }
        }
    }

    /**
     * Takes care of any game logic that has to
     * run every game day, like constructions.
     */
    void time1Day()
    {
        foreach (var i in _game.getSavedGame().getBases())
        {
            // Handle facility construction
            foreach (var j in i.getFacilities())
            {
                if (j.getBuildTime() > 0)
                {
                    j.build();
                    if (j.getBuildTime() == 0)
                    {
                        popup(new ProductionCompleteState(i, tr(j.getRules().getType()), this, productionProgress_e.PROGRESS_CONSTRUCTION));
                    }
                }
            }

            // Handle science project
            // 1. gather finished research
            var finished = new List<ResearchProject>();
            foreach (var iter in i.getResearch())
            {
                if (iter.step())
                {
                    finished.Add(iter);
                }
            }
            // 2. remember available research before adding new finished research
            var before = new List<RuleResearch>();
            if (finished.Any())
            {
                _game.getSavedGame().getAvailableResearchProjects(before, _game.getMod(), i);
            }
            // 3. add finished research, including lookups and getonefrees (up to 4x)
            foreach (var iter in finished)
            {
                RuleResearch bonus = null;
                RuleResearch research = iter.getRules();

                // 3a. remove finished research from the base where it was researched
                i.removeResearch(iter);
                //iter = 0;

                // 3b. handle interrogation
                if (Options.retainCorpses && research.destroyItem() && _game.getMod().getUnit(research.getName()) != null)
                {
                    i.getStorageItems().addItem(_game.getMod().getArmor(_game.getMod().getUnit(research.getName()).getArmor(), true).getCorpseGeoscape());
                }
                // 3c. handle getonefrees (topic+lookup)
                if (research.getGetOneFree().Any())
                {
                    var possibilities = new List<string>();
                    foreach (var f in research.getGetOneFree())
                    {
                        if (!_game.getSavedGame().isResearched(f, false))
                        {
                            possibilities.Add(f);
                        }
                    }
                    if (possibilities.Any())
                    {
                        int pick = RNG.generate(0, possibilities.Count - 1);
                        string sel = possibilities[pick];
                        bonus = _game.getMod().getResearch(sel, true);
                        _game.getSavedGame().addFinishedResearch(bonus, _game.getMod(), i);
                        if (!string.IsNullOrEmpty(bonus.getLookup()))
                        {
                            _game.getSavedGame().addFinishedResearch(_game.getMod().getResearch(bonus.getLookup(), true), _game.getMod(), i);
                        }
                    }
                }
                // 3d. determine and remember if the ufopedia article should pop up again or not
                // Note: because different topics may lead to the same lookup
                RuleResearch newResearch = research;
                string name = string.IsNullOrEmpty(research.getLookup()) ? research.getName() : research.getLookup();
                if (_game.getSavedGame().isResearched(name, false))
                {
                    newResearch = null;
                }
                // 3e. handle core research (topic+lookup)
                _game.getSavedGame().addFinishedResearch(research, _game.getMod(), i);
                if (!string.IsNullOrEmpty(research.getLookup()))
                {
                    _game.getSavedGame().addFinishedResearch(_game.getMod().getResearch(research.getLookup(), true), _game.getMod(), i);
                }
                // 3e. handle cutscenes
                if (!string.IsNullOrEmpty(research.getCutscene()))
                {
                    popup(new CutsceneState(research.getCutscene()));
                }
                if (bonus != null && !string.IsNullOrEmpty(bonus.getCutscene()))
                {
                    popup(new CutsceneState(bonus.getCutscene()));
                }
                // 3e. handle research complete popup + ufopedia article popups (topic+bonus)
                popup(new ResearchCompleteState(newResearch, bonus, research));
                // 3f. reset timer
                timerReset();
                // 3g. warning if weapon is researched before its clip
                if (newResearch != null)
                {
                    RuleItem item = _game.getMod().getItem(newResearch.getName());
                    if (item != null && item.getBattleType() == BattleType.BT_FIREARM && item.getCompatibleAmmo().Any())
                    {
                        RuleManufacture man = _game.getMod().getManufacture(item.getType());
                        if (man != null && man.getRequirements().Any())
                        {
                            List<string> req = man.getRequirements();
                            RuleItem ammo = _game.getMod().getItem(item.getCompatibleAmmo().First());
                            if (ammo != null && req.Contains(ammo.getType()) && !_game.getSavedGame().isResearched(req, true))
                            {
                                popup(new ResearchRequiredState(item));
                            }
                        }
                    }
                }
                // 3h. inform about new possible research
                var after = new List<RuleResearch>();
                _game.getSavedGame().getAvailableResearchProjects(after, _game.getMod(), i);
                var newPossibleResearch = new List<RuleResearch>();
                _game.getSavedGame().getNewlyAvailableResearchProjects(ref before, ref after, ref newPossibleResearch);
                popup(new NewPossibleResearchState(i, newPossibleResearch));
                // 3i. inform about new possible manufacture
                var newPossibleManufacture = new List<RuleManufacture>();
                _game.getSavedGame().getDependableManufacture(newPossibleManufacture, research, _game.getMod(), i);
                if (newPossibleManufacture.Any())
                {
                    popup(new NewPossibleManufactureState(i, newPossibleManufacture));
                }
                // 3j. now iterate through all the bases and remove this project from their labs (unless it can still yield more stuff!)
                foreach (var j in _game.getSavedGame().getBases())
                {
                    foreach (var iter2 in j.getResearch())
                    {
                        if (research.getName() == iter2.getRules().getName())
                        {
                            if (!_game.getSavedGame().isResearched(research.getGetOneFree(), false))
                            {
                                // This research topic still has some more undiscovered "getOneFree" topics, keep it!
                            }
                            else if (_game.getSavedGame().hasUndiscoveredProtectedUnlock(research, _game.getMod()))
                            {
                                // This research topic still has one or more undiscovered "protected unlocks", keep it!
                            }
                            else
                            {
                                // This topic can't give you anything else anymore, remove it!
                                j.removeResearch(iter2);
                                break;
                            }
                        }
                    }
                }
            }

            // Handle soldier wounds
            foreach (var j in i.getSoldiers())
            {
                if (j.getWoundRecovery() > 0)
                {
                    j.heal();
                }
            }
            // Handle psionic training
            if (i.getAvailablePsiLabs() > 0 && Options.anytimePsiTraining)
            {
                foreach (var s in i.getSoldiers())
                {
                    s.trainPsi1Day();
                    s.calcStatString(_game.getMod().getStatStrings(), (Options.psiStrengthEval && _game.getSavedGame().isResearched(_game.getMod().getPsiRequirements())));
                }
            }
        }
        // handle regional and country points for alien bases
        foreach (var b in _game.getSavedGame().getAlienBases())
        {
            foreach (var k in _game.getSavedGame().getRegions())
            {
                if (k.getRules().insideRegion(b.getLongitude(), b.getLatitude()))
                {
                    k.addActivityAlien(b.getDeployment().getPoints());
                    break;
                }
            }
            foreach (var k in _game.getSavedGame().getCountries())
            {
                if (k.getRules().insideCountry(b.getLongitude(), b.getLatitude()))
                {
                    k.addActivityAlien(b.getDeployment().getPoints());
                    break;
                }
            }
        }

        // Handle resupply of alien bases.
        _game.getSavedGame().getAlienBases().ForEach(x => GenerateSupplyMission(x, _game.getMod(), _game.getSavedGame()));

        // Autosave 3 times a month
        int day = _game.getSavedGame().getTime().getDay();
        if (day == 10 || day == 20)
        {
            if (_game.getSavedGame().isIronman())
            {
                popup(new SaveGameState(OptionsOrigin.OPT_GEOSCAPE, SaveType.SAVE_IRONMAN, _palette));
            }
            else if (Options.autosave)
            {
                popup(new SaveGameState(OptionsOrigin.OPT_GEOSCAPE, SaveType.SAVE_AUTO_GEOSCAPE, _palette));
            }
        }
    }

    /**
     * Takes care of any game logic that has to
     * run every game hour, like transfers.
     */
    void time1Hour()
    {
        // Handle craft maintenance
        foreach (var i in _game.getSavedGame().getBases())
        {
            foreach (var j in i.getCrafts())
            {
                if (j.getStatus() == "STR_REPAIRS")
                {
                    j.repair();
                }
                else if (j.getStatus() == "STR_REARMING")
                {
                    string s = j.rearm(_game.getMod());
                    if (!string.IsNullOrEmpty(s))
                    {
                        string msg = tr("STR_NOT_ENOUGH_ITEM_TO_REARM_CRAFT_AT_BASE")
                                           .arg(tr(s))
                                           .arg(j.getName(_game.getLanguage()))
                                           .arg(i.getName());
                        popup(new CraftErrorState(this, msg));
                    }
                }
            }
        }

        // Handle transfers
        bool window = false;
        foreach (var i in _game.getSavedGame().getBases())
        {
            foreach (var j in i.getTransfers())
            {
                j.advance(i);
                if (!window && j.getHours() <= 0)
                {
                    window = true;
                }
            }
        }
        if (window)
        {
            popup(new ItemsArrivingState(this));
        }
        // Handle Production
        foreach (var i in _game.getSavedGame().getBases())
        {
            var toRemove = new Dictionary<Production, productionProgress_e>();
            foreach (var j in i.getProductions())
            {
                toRemove[j] = j.step(i, _game.getSavedGame(), _game.getMod());
            }
            foreach (var j in toRemove)
            {
                if (j.Value > productionProgress_e.PROGRESS_NOT_COMPLETE)
                {
                    popup(new ProductionCompleteState(i, tr(j.Key.getRules().getName()), this, j.Value));
                    i.removeProduction(j.Key);
                }
            }

            if (Options.storageLimitsEnforced && i.storesOverfull())
            {
                popup(new ErrorMessageState(tr("STR_STORAGE_EXCEEDED").arg(i.getName()), _palette, (byte)_game.getMod().getInterface("geoscape").getElement("errorMessage").color, "BACK13.SCR", _game.getMod().getInterface("geoscape").getElement("errorPalette").color));
                popup(new SellState(i));
            }
        }
        foreach (var i in _game.getSavedGame().getMissionSites())
        {
            if (!i.getDetected())
            {
                i.setDetected(true);
                popup(new MissionDetectedState(i, this));
                break;
            }
        }
    }

    /**
     * Takes care of any game logic that has to
     * run every game half hour, like UFO detection.
     */
    void time30Minutes()
    {
        var alienMissions = _game.getSavedGame().getAlienMissions();
        // Decrease mission countdowns
        alienMissions.ForEach(x => x.think(_game, _globe));
        // Remove finished missions
        for (var am = 0; am < alienMissions.Count;)
        {
            if (alienMissions[am].isOver())
            {
                alienMissions.RemoveAt(am);
            }
            else
            {
                ++am;
            }
        }

        // Handle crashed UFOs expiration
        _game.getSavedGame().getUfos().ForEach(x =>
	        /// Decrease UFO expiration timer.
	        {
		        if (x.getStatus() == UfoStatus.CRASHED)
		        {
			        if (x.getSecondsRemaining() >= 30 * 60)
			        {
				        x.setSecondsRemaining(x.getSecondsRemaining() - 30 * 60);
				        return;
			        }
			        // Marked expired UFOs for removal.
                    x.setStatus(UfoStatus.DESTROYED);
                }
            }
        );

        // Handle craft maintenance and alien base detection
        foreach (var i in _game.getSavedGame().getBases())
        {
            foreach (var j in i.getCrafts())
            {
                if (j.getStatus() == "STR_REFUELLING")
                {
                    string s = j.refuel();
                    if (!string.IsNullOrEmpty(s))
                    {
                        string msg = tr("STR_NOT_ENOUGH_ITEM_TO_REFUEL_CRAFT_AT_BASE")
                                            .arg(tr(s))
                                            .arg(j.getName(_game.getLanguage()))
                                            .arg(i.getName());
                        popup(new CraftErrorState(this, msg));
                    }
                }
            }
        }

        // Handle UFO detection and give aliens points
        foreach (var u in _game.getSavedGame().getUfos())
        {
            int points = u.getRules().getMissionScore(); //one point per UFO in-flight per half hour
            switch (u.getStatus())
            {
                case UfoStatus.LANDED:
                    points *= 2;
                    goto case UfoStatus.FLYING;
                case UfoStatus.FLYING:
                    // Get area
                    foreach (var k in _game.getSavedGame().getRegions())
                    {
                        if (k.getRules().insideRegion(u.getLongitude(), u.getLatitude()))
                        {
                            k.addActivityAlien(points);
                            break;
                        }
                    }
                    // Get country
                    foreach (var k in _game.getSavedGame().getCountries())
                    {
                        if (k.getRules().insideCountry(u.getLongitude(), u.getLatitude()))
                        {
                            k.addActivityAlien(points);
                            break;
                        }
                    }
                    if (!u.getDetected())
                    {
                        bool detected = false, hyperdetected = false;
                        var bases = _game.getSavedGame().getBases();
                        for (var b = 0; b < bases.Count && !hyperdetected; ++b)
                        {
                            switch (bases[b].detect(u))
                            {
                                case 2: // hyper-wave decoder
                                    u.setHyperDetected(true);
                                    hyperdetected = true;
                                    goto case 1;
                                case 1: // conventional radar
                                    detected = true;
                                    break;
                            }
                            var crafts = bases[b].getCrafts();
                            for (var c = 0; c < crafts.Count && !detected; ++c)
                            {
                                if (crafts[c].getStatus() == "STR_OUT" && crafts[c].detect(u))
                                {
                                    detected = true;
                                    break;
                                }
                            }
                        }
                        if (detected)
                        {
                            u.setDetected(true);
                            popup(new UfoDetectedState(u, this, true, u.getHyperDetected()));
                        }
                    }
                    else
                    {
                        bool detected = false, hyperdetected = false;
                        var bases = _game.getSavedGame().getBases();
                        for (var b = 0; b < bases.Count && !hyperdetected; ++b)
                        {
                            switch (bases[b].insideRadarRange(u))
                            {
                                case 2: // hyper-wave decoder
                                    detected = true;
                                    hyperdetected = true;
                                    u.setHyperDetected(true);
                                    break;
                                case 1: // conventional radar
                                    detected = true;
                                    hyperdetected = u.getHyperDetected();
                                    break;
                            }
                            var crafts = bases[b].getCrafts();
                            for (var c = 0; c < crafts.Count && !detected; ++c)
                            {
                                if (crafts[c].getStatus() == "STR_OUT" && crafts[c].insideRadarRange(u))
                                {
                                    detected = true;
                                    hyperdetected = u.getHyperDetected();
                                    break;
                                }
                            }
                        }
                        if (!detected)
                        {
                            u.setDetected(false);
                            u.setHyperDetected(false);
                            if (u.getFollowers().Any())
                            {
                                popup(new UfoLostState(u.getName(_game.getLanguage())));
                            }
                        }
                    }
                    break;
                case UfoStatus.CRASHED:
                case UfoStatus.DESTROYED:
                    break;
            }
        }

        // Processes MissionSites
        var missionSites = _game.getSavedGame().getMissionSites();
        for (var site = 0; site < missionSites.Count;)
        {
            if (processMissionSite(missionSites[site]))
            {
                missionSites.RemoveAt(site);
            }
            else
            {
                ++site;
            }
        }
    }

    /** @brief Process a MissionSite.
     * This function object will count down towards expiring a MissionSite, and handle expired MissionSites.
     * @param ts Pointer to mission site.
     * @return Has mission site expired?
     */
    bool processMissionSite(MissionSite site)
    {
	    bool removeSite = site.getSecondsRemaining() < 30 * 60;
	    if (!removeSite)
	    {
		    site.setSecondsRemaining(site.getSecondsRemaining() - 30 * 60);
	    }
	    else
	    {
		    removeSite = !site.getFollowers().Any(); // CHEEKY EXPLOIT
	    }

	    int score = removeSite ? site.getDeployment().getDespawnPenalty() : site.getDeployment().getPoints();

	    Region region = _game.getSavedGame().locateRegion(site);
	    if (region != null)
	    {
		    region.addActivityAlien(score);
	    }
	    foreach (var k in _game.getSavedGame().getCountries())
	    {
		    if (k.getRules().insideCountry(site.getLongitude(), site.getLatitude()))
		    {
			    k.addActivityAlien(score);
			    break;
		    }
	    }
	    if (!removeSite)
	    {
		    return false;
	    }
        return true;
    }

    /**
     * Takes care of any game logic that has to
     * run every game ten minutes, like fuel consumption.
     */
    void time10Minutes()
    {
        foreach (var i in _game.getSavedGame().getBases())
        {
            // Fuel consumption for XCOM craft.
            foreach (var j in i.getCrafts())
            {
                if (j.getStatus() == "STR_OUT")
                {
                    j.consumeFuel();
                    if (!j.getLowFuel() && j.getFuel() <= j.getFuelLimit())
                    {
                        j.setLowFuel(true);
                        j.returnToBase();
                        popup(new LowFuelState(j, this));
                    }

                    if (j.getDestination() == null)
                    {
                        double range = Nautical(j.getRules().getSightRange());
                        foreach (var b in _game.getSavedGame().getAlienBases())
                        {
                            if (j.getDistance(b) <= range)
                            {
                                if (RNG.percent((int)(50 - (j.getDistance(b) / range) * 50)) && !b.isDiscovered())
                                {
                                    b.setDiscovered(true);
                                }
                            }
                        }
                    }
                }
            }
        }
        if (Options.aggressiveRetaliation)
        {
            // Detect as many bases as possible.
            foreach (var iBase in _game.getSavedGame().getBases())
            {
                // Find a UFO that detected this base, if any.
                if (_game.getSavedGame().getUfos().Any(x => DetectXCOMBase(x, iBase)))
                {
                    // Base found
                    iBase.setRetaliationTarget(true);
                }
            }
        }
        else
        {
            // Only remember last base in each region.
            var discovered = new Dictionary<Region, Base>();
            foreach (var iBase in _game.getSavedGame().getBases())
            {
                // Find a UFO that detected this base, if any.
                if (_game.getSavedGame().getUfos().Any(x => DetectXCOMBase(x, iBase)))
                {
                    discovered[_game.getSavedGame().locateRegion(iBase)] = iBase;
                }
            }
            // Now mark the bases as discovered.
            foreach (var d in discovered)
            {
                d.Value.setRetaliationTarget(true);
            }
        }
    }

    /**
     * Only UFOs within detection range of the base have a chance to detect it.
     * @param ufo Pointer to the UFO attempting detection.
     * @return If the base is detected by @a ufo.
     */
    bool DetectXCOMBase(Ufo ufo, Base @base)
    {
	    if (ufo.getTrajectoryPoint() <= 1) return false;
	    if (ufo.getTrajectory().getZone(ufo.getTrajectoryPoint()) == 5) return false;
	    if ((ufo.getMission().getRules().getObjective() != MissionObjective.OBJECTIVE_RETALIATION && !Options.aggressiveRetaliation) ||	// only UFOs on retaliation missions actively scan for bases
		    ufo.getTrajectory().getID() == UfoTrajectory.RETALIATION_ASSAULT_RUN || 									// UFOs attacking a base don't detect!
		    ufo.isCrashed() ||                                                                                          // Crashed UFOs don't detect!
            @base.getDistance(ufo) >= Nautical(ufo.getRules().getSightRange()))											// UFOs have a detection range of 80 XCOM units. - we use a great circle fomrula and nautical miles.
	    {
		    return false;
	    }
	    return RNG.percent((int)@base.getDetectionChance());
    }

    /**
     * Takes care of any game logic that has to
     * run every game second, like craft movement.
     */
    void time5Seconds()
    {
        // Game over if there are no more bases.
        if (!_game.getSavedGame().getBases().Any())
        {
            _game.getSavedGame().setEnding(GameEnding.END_LOSE);
        }
        if (_game.getSavedGame().getEnding() == GameEnding.END_LOSE)
        {
            _game.pushState(new CutsceneState(CutsceneState.LOSE_GAME));
            if (_game.getSavedGame().isIronman())
            {
                _game.pushState(new SaveGameState(OptionsOrigin.OPT_GEOSCAPE, SaveType.SAVE_IRONMAN, _palette));
            }
            return;
        }

        // Handle UFO logic
        foreach (var i in _game.getSavedGame().getUfos())
        {
            switch (i.getStatus())
            {
                case UfoStatus.FLYING:
                    i.think();
                    if (i.reachedDestination())
                    {
                        int count = _game.getSavedGame().getMissionSites().Count;
                        AlienMission mission = i.getMission();
                        bool detected = i.getDetected();
                        mission.ufoReachedWaypoint(i, _game, _globe);
                        if (detected != i.getDetected() && i.getFollowers().Any())
                        {
                            if (!(i.getTrajectory().getID() == UfoTrajectory.RETALIATION_ASSAULT_RUN && i.getStatus() == UfoStatus.LANDED))
                                popup(new UfoLostState(i.getName(_game.getLanguage())));
                        }
                        if (count < _game.getSavedGame().getMissionSites().Count)
                        {
                            MissionSite site = _game.getSavedGame().getMissionSites().Last();
                            site.setDetected(true);
                            popup(new MissionDetectedState(site, this));
                        }
                        // If UFO was destroyed, don't spawn missions
                        if (i.getStatus() == UfoStatus.DESTROYED)
                            return;
                        Base @base = (Base)i.getDestination();
                        if (@base != null)
                        {
                            mission.setWaveCountdown((uint)(30 * (RNG.generate(0, 400) + 48)));
                            i.setDestination(null);
                            @base.setupDefenses();
                            timerReset();
                            if (@base.getDefenses().Any())
                            {
                                popup(new BaseDefenseState(@base, i, this));
                            }
                            else
                            {
                                handleBaseDefense(@base, i);
                                return;
                            }
                        }
                    }
                    break;
                case UfoStatus.LANDED:
                    i.think();
                    if (i.getSecondsRemaining() == 0)
                    {
                        AlienMission mission = i.getMission();
                        bool detected = i.getDetected();
                        mission.ufoLifting(i, _game.getSavedGame());
                        if (detected != i.getDetected() && i.getFollowers().Any())
                        {
                            popup(new UfoLostState(i.getName(_game.getLanguage())));
                        }
                    }
                    break;
                case UfoStatus.CRASHED:
                    i.think();
                    if (i.getSecondsRemaining() == 0)
                    {
                        i.setDetected(false);
                        i.setStatus(UfoStatus.DESTROYED);
                    }
                    break;
                case UfoStatus.DESTROYED:
                    // Nothing to do
                    break;
            }
        }

        // Handle craft logic
        foreach (var i in _game.getSavedGame().getBases())
        {
            var crafts = i.getCrafts();
            for (var j = 0; j < crafts.Count;)
            {
                if (crafts[j].isDestroyed())
                {
                    foreach (var country in _game.getSavedGame().getCountries())
                    {
                        if (country.getRules().insideCountry(crafts[j].getLongitude(), crafts[j].getLatitude()))
                        {
                            country.addActivityXcom(-crafts[j].getRules().getScore());
                            break;
                        }
                    }
                    foreach (var region in _game.getSavedGame().getRegions())
                    {
                        if (region.getRules().insideRegion(crafts[j].getLongitude(), crafts[j].getLatitude()))
                        {
                            region.addActivityXcom(-crafts[j].getRules().getScore());
                            break;
                        }
                    }
                    // if a transport craft has been shot down, kill all the soldiers on board.
                    if (crafts[j].getRules().getSoldiers() > 0)
                    {
                        var soldiers = i.getSoldiers();
                        for (var k = 0; k < soldiers.Count;)
                        {
                            if (soldiers[k].getCraft() == crafts[j])
                            {
                                _game.getSavedGame().killSoldier(soldiers[k]);
                            }
                            else
                            {
                                ++k;
                            }
                        }
                    }
                    Craft craft = crafts[j];
                    crafts[j] = i.removeCraft(craft, false);
                    craft = null;
                    continue;
                }
                if (crafts[j].getDestination() != null)
                {
                    Ufo u = (Ufo)crafts[j].getDestination();
                    if (u != null)
                    {
                        if (!u.getDetected())
                        {
                            if (u.getTrajectory().getID() == UfoTrajectory.RETALIATION_ASSAULT_RUN && (u.getStatus() == UfoStatus.LANDED || u.getStatus() == UfoStatus.DESTROYED))
                            {
                                crafts[j].returnToBase();
                            }
                            else
                            {
                                Waypoint w = new Waypoint();
                                w.setLongitude(u.getLongitude());
                                w.setLatitude(u.getLatitude());
                                w.setId(u.getId());
                                crafts[j].setDestination(null);
                                popup(new GeoscapeCraftState(crafts[j], _globe, w));
                            }
                        }
                        if (u.getStatus() == UfoStatus.LANDED && crafts[j].isInDogfight())
                        {
                            crafts[j].setInDogfight(false);
                        }
                        else if (u.getStatus() == UfoStatus.DESTROYED)
                        {
                            crafts[j].returnToBase();
                        }
                    }
                    else
                    {
                        if (crafts[j].isInDogfight())
                        {
                            crafts[j].setInDogfight(false);
                        }
                    }
                }

                crafts[j].think();

                if (crafts[j].reachedDestination())
                {
                    Ufo u = (Ufo)crafts[j].getDestination();
                    Waypoint w = (Waypoint)crafts[j].getDestination();
                    MissionSite m = (MissionSite)crafts[j].getDestination();
                    AlienBase b = (AlienBase)crafts[j].getDestination();
                    if (u != null)
                    {
                        switch (u.getStatus())
                        {
                            case UfoStatus.FLYING:
                                // Not more than 4 interceptions at a time.
                                if (_dogfights.Count + _dogfightsToBeStarted.Count >= 4)
                                {
                                    ++j;
                                    continue;
                                }
                                // Can we actually fight it
                                if (!crafts[j].isInDogfight() && u.getSpeed() <= crafts[j].getRules().getMaxSpeed())
                                {
                                    var dogfight = new DogfightState(this, crafts[j], u);
                                    _dogfightsToBeStarted.Add(dogfight);
                                    if (crafts[j].getRules().isWaterOnly() && u.getAltitudeInt() > crafts[j].getRules().getMaxAltitude())
                                    {
                                        popup(new DogfightErrorState(crafts[j], tr("STR_UNABLE_TO_ENGAGE_DEPTH")));
                                        dogfight.setMinimized(true);
                                        dogfight.setWaitForAltitude(true);
                                    }
                                    else if (crafts[j].getRules().isWaterOnly() && !_globe.insideLand(crafts[j].getLongitude(), crafts[j].getLatitude()))
                                    {
                                        popup(new DogfightErrorState(crafts[j], tr("STR_UNABLE_TO_ENGAGE_AIRBORNE")));
                                        dogfight.setMinimized(true);
                                        dogfight.setWaitForPoly(true);
                                    }
                                    if (!_dogfightStartTimer.isRunning())
                                    {
                                        _pause = true;
                                        timerReset();
                                        _globe.center(crafts[j].getLongitude(), crafts[j].getLatitude());
                                        startDogfight();
                                        _dogfightStartTimer.start();
                                    }
                                    _game.getMod().playMusic("GMINTER");
                                }
                                break;
                            case UfoStatus.LANDED:
                            case UfoStatus.CRASHED:
                            case UfoStatus.DESTROYED: // Just before expiration
                                if (crafts[j].getNumSoldiers() > 0 || crafts[j].getNumVehicles() > 0)
                                {
                                    if (!crafts[j].isInDogfight())
                                    {
                                        // look up polygons texture
                                        int texture, shade;
                                        _globe.getPolygonTextureAndShade(u.getLongitude(), u.getLatitude(), out texture, out shade);
                                        timerReset();
                                        popup(new ConfirmLandingState(crafts[j], _game.getMod().getGlobe().getTexture(texture), shade));
                                    }
                                }
                                else if (u.getStatus() != UfoStatus.LANDED)
                                {
                                    crafts[j].returnToBase();
                                }
                                break;
                        }
                    }
                    else if (w != null)
                    {
                        popup(new CraftPatrolState(crafts[j], _globe));
                        crafts[j].setDestination(null);
                    }
                    else if (m != null)
                    {
                        if (crafts[j].getNumSoldiers() > 0 || crafts[j].getNumVehicles() > 0)
                        {
                            // look up polygons texture
                            int texture, shade;
                            _globe.getPolygonTextureAndShade(m.getLongitude(), m.getLatitude(), out texture, out shade);
                            if (_game.getMod().getGlobe().getTexture(m.getTexture()) != null)
                            {
                                texture = m.getTexture();
                            }
                            timerReset();
                            popup(new ConfirmLandingState(crafts[j], _game.getMod().getGlobe().getTexture(texture), shade));
                        }
                        else
                        {
                            crafts[j].returnToBase();
                        }
                    }
                    else if (b != null)
                    {
                        if (b.isDiscovered())
                        {
                            if (crafts[j].getNumSoldiers() > 0 || crafts[j].getNumVehicles() > 0)
                            {
                                int texture, shade;
                                _globe.getPolygonTextureAndShade(b.getLongitude(), b.getLatitude(), out texture, out shade);
                                timerReset();
                                popup(new ConfirmLandingState(crafts[j], _game.getMod().getGlobe().getTexture(texture), shade));
                            }
                            else
                            {
                                crafts[j].returnToBase();
                            }
                        }
                    }
                }
                ++j;
            }
        }

        // Clean up dead UFOs and end dogfights which were minimized.
        var ufos = _game.getSavedGame().getUfos();
        for (var i = 0; i < ufos.Count;)
        {
            if (ufos[i].getStatus() == UfoStatus.DESTROYED)
            {
                if (ufos[i].getFollowers().Any())
                {
                    // Remove all dogfights with this UFO.
                    for (var d = 0; d < _dogfights.Count;)
                    {
                        if (_dogfights[d].getUfo() == ufos[i])
                        {
                            _dogfights.RemoveAt(d);
                        }
                        else
                        {
                            ++d;
                        }
                    }
                }
                ufos.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }

        // Check any dogfights waiting to open
        foreach (var d in _dogfights)
        {
            if (d.isMinimized())
            {
                if ((d.getWaitForPoly() && _globe.insideLand(d.getUfo().getLongitude(), d.getUfo().getLatitude())) ||
                    (d.getWaitForAltitude() && d.getUfo().getAltitudeInt() <= d.getCraft().getRules().getMaxAltitude()))
                {
                    _pause = true; // the USO reached the sea during this interval period, stop the timer and let handleDogfights() take it from there.
                }
            }
        }

        // Clean up unused waypoints
        var waypoints = _game.getSavedGame().getWaypoints();
        for (var i = 0; i < waypoints.Count;)
        {
            if (!waypoints[i].getFollowers().Any())
            {
                waypoints.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }
    }

    /**
     * Handle base defense
     * @param base Base to defend.
     * @param ufo Ufo attacking base.
     */
    internal void handleBaseDefense(Base @base, Ufo ufo)
    {
        // Whatever happens in the base defense, the UFO has finished its duty
        ufo.setStatus(UfoStatus.DESTROYED);

        if (@base.getAvailableSoldiers(true) > 0 || @base.getVehicles().Any())
        {
            SavedBattleGame bgame = new SavedBattleGame();
            _game.getSavedGame().setBattleGame(bgame);
            bgame.setMissionType("STR_BASE_DEFENSE");
            var bgen = new BattlescapeGenerator(_game);
            bgen.setBase(@base);
            bgen.setAlienRace(ufo.getAlienRace());
            bgen.run();
            _pause = true;
            _game.pushState(new BriefingState(null, @base));
        }
        else
        {
            // Please garrison your bases in future
            popup(new BaseDestroyedState(@base));
        }
    }

    /**
     * Proccesses a directive to start up a mission, if possible.
     * @param command the directive from which to read information.
     * @return whether the command successfully produced a new mission.
     */
    bool processCommand(RuleMissionScript command)
    {
        SavedGame save = _game.getSavedGame();
        AlienStrategy strategy = save.getAlienStrategy();
        Mod.Mod mod = _game.getMod();
        int month = _game.getSavedGame().getMonthsPassed();
        string targetRegion = null;
        RuleAlienMission missionRules;
        string missionType = null;
        string missionRace;
        int targetZone = -1;

        // terror mission type deal? this will require special handling.
        if (command.getSiteType())
        {
            // we know for a fact that this command has mission weights defined, otherwise this flag could not be set.
            missionType = command.generate((uint)month, GenerationType.GEN_MISSION);
            List<string> missions = command.getMissionTypes(month);
            int maxMissions = missions.Count;
            bool targetBase = RNG.percent(command.getTargetBaseOdds());
            int currPos = 0;
            for (; currPos != maxMissions; ++currPos)
            {
                if (missions[currPos] == missionType)
                {
                    break;
                }
            }

            // let's build a list of regions with spawn zones to pick from
            var validAreas = new List<KeyValuePair<string, int>>();

            // this is actually a bit of a cheat, we ARE using the mission weights as defined, but we'll try them all if the one we pick first isn't valid.
            for (int h = 0; h != maxMissions; ++h)
            {
                // we'll use the regions listed in the command, if any, otherwise check all the regions in the ruleset looking for matches
                List<string> regions = (command.hasRegionWeights()) ? command.getRegions(month) : mod.getRegionsList();
                missionRules = mod.getAlienMission(missionType, true);
                targetZone = missionRules.getSpawnZone();

                if (targetBase)
                {
                    var regionsToKeep = new List<string>();
                    //if we're targetting a base, we ignore regions that don't contain bases, simple.
                    foreach (var i in save.getBases())
                    {
                        regionsToKeep.Add(save.locateRegion(i.getLongitude(), i.getLatitude()).getRules().getType());
                    }
                    for (var i = 0; i < regions.Count;)
                    {
                        if (!regionsToKeep.Contains(regions[i]))
                        {
                            regions.RemoveAt(i);
                        }
                        else
                        {
                            ++i;
                        }
                    }
                }

                for (var i = 0; i < regions.Count;)
                {
                    // we don't want the same mission running in any given region twice simultaneously, so prune the list as needed.
                    bool processThisRegion = true;
                    foreach (var j in save.getAlienMissions())
                    {
                        if (j.getRules().getType() == missionRules.getType() && j.getRegion() == regions[i])
                        {
                            processThisRegion = false;
                            break;
                        }
                    }
                    if (!processThisRegion)
                    {
                        regions.RemoveAt(i);
                        continue;
                    }
                    // ok, we found a region that doesn't have our mission in it, let's see if it has an appropriate landing zone.
                    // if it does, let's add it to our list of valid areas, taking note of which mission area(s) matched.
                    RuleRegion region = mod.getRegion(regions[i], true);
                    if ((int)(region.getMissionZones().Count) > targetZone)
                    {
                        List<MissionArea> areas = region.getMissionZones()[targetZone].areas;
                        int counter = 0;
                        foreach (var j in areas)
                        {
                            // validMissionLocation checks to make sure this city/whatever hasn't been used by the last n missions using this varName
                            // this prevents the same location getting hit more than once every n missions.
                            if (j.isPoint() && strategy.validMissionLocation(command.getVarName(), region.getType(), counter))
                            {
                                validAreas.Add(KeyValuePair.Create(region.getType(), counter));
                            }
                            counter++;
                        }
                    }
                    ++i;
                }

                // oh bother, we couldn't find anything valid, this mission won't run this month.
                if (!validAreas.Any())
                {
                    if (maxMissions > 1 && ++currPos == maxMissions)
                    {
                        currPos = 0;
                    }
                    missionType = missions[currPos];
                }
                else
                {
                    break;
                }
            }

            if (!validAreas.Any())
            {
                // now we're in real trouble, we've managed to make it out of the loop and we still don't have any valid choices
                // this command cannot run this month, we have failed, forgive us senpai.
                return false;
            }
            // reset this, we may have used it earlier, it longer represents the target zone type, but the target zone number within that type
            targetZone = -1;
            // everything went according to plan: we can now pick a city/whatever to attack.
            while (targetZone == -1)
            {
                if (command.hasRegionWeights())
                {
                    // if we have a weighted region list, we know we have at least one valid choice for this mission
                    targetRegion = command.generate((uint)month, GenerationType.GEN_REGION);
                }
                else
                {
                    // if we don't have a weighted list, we'll select a region at random from the ruleset,
                    // validate that it's in our list, and pick one of its cities at random
                    // this will give us an even distribution between regions regardless of the number of cities.
                    targetRegion = mod.getRegionsList()[RNG.generate(0, mod.getRegionsList().Count - 1)];
                }

                // we need to know the range of the region within our vector, in order to randomly select a city from it
                int min = -1;
                int max = -1;
                int curr = 0;
                foreach (var i in validAreas)
                {
                    if (i.Key == targetRegion)
                    {
                        if (min == -1)
                        {
                            min = curr;
                        }
                        max = curr;
                    }
                    else if (min > -1)
                    {
                        // if we've stopped detecting matches, we're done looking.
                        break;
                    }
                    ++curr;
                }
                if (min != -1)
                {
                    // we have our random range, we can make a selection, and we're done.
                    targetZone = validAreas[RNG.generate(min, max)].Value;
                }
            }
            // now add that city to the list of sites we've hit, store the array, etc.
            strategy.addMissionLocation(command.getVarName(), targetRegion, targetZone, command.getRepeatAvoidance());
        }
        else if (RNG.percent(command.getTargetBaseOdds()))
        {
            // build a list of the mission types we're dealing with, if any
            List<string> types = command.getMissionTypes(month);
            // now build a list of regions with bases in.
            var regionsMaster = new List<string>();
            foreach (var i in save.getBases())
            {
                regionsMaster.Add(save.locateRegion(i).getRules().getType());
            }
            // no defined mission types? then we'll prune the region list to ensure we only have a region that can generate a mission.
            if (!types.Any())
            {
                for (var i = 0; i < regionsMaster.Count;)
                {
                    if (!strategy.validMissionRegion(regionsMaster[i]))
                    {
                        regionsMaster.RemoveAt(i);
                        continue;
                    }
                    ++i;
                }
                // no valid missions in any base regions? oh dear, i guess we failed.
                if (!regionsMaster.Any())
                {
                    return false;
                }
                // pick a random region from our list
                targetRegion = regionsMaster[RNG.generate(0, regionsMaster.Count - 1)];
            }
            else
            {
                // we don't care about regional mission distributions, we're targetting a base with whatever mission we pick, so let's pick now
                // we'll iterate the mission list, starting at a random point, and wrapping around to the beginning
                int max = types.Count;
                int entry = RNG.generate(0, max - 1);
                List<string> regions;

                for (int i = 0; i != max; ++i)
                {
                    regions = regionsMaster;
                    foreach (var j in save.getAlienMissions())
                    {
                        // if the mission types match
                        if (types[entry] == j.getRules().getType())
                        {
                            for (var k = 0; k < regions.Count;)
                            {
                                // and the regions match
                                if (regions[k] == j.getRegion())
                                {
                                    // prune the entry from the list
                                    regions.RemoveAt(k);
                                    continue;
                                }
                                ++k;
                            }
                        }
                    }

                    // we have a valid list of regions containing bases, pick one.
                    if (regions.Any())
                    {
                        missionType = types[entry];
                        targetRegion = regions[RNG.generate(0, regions.Count - 1)];
                        break;
                    }
                    // otherwise, try the next mission in the list.
                    if (max > 1 && ++entry == max)
                    {
                        entry = 0;
                    }
                }
            }
        }
        // now the easy stuff
        else if (!command.hasRegionWeights())
        {
            // no regionWeights means we pick from the table
            targetRegion = strategy.chooseRandomRegion(mod);
        }
        else
        {
            // otherwise, let the command dictate the region.
            targetRegion = command.generate((uint)month, GenerationType.GEN_REGION);
        }

        if (string.IsNullOrEmpty(targetRegion))
        {
            // something went horribly wrong, we should have had at LEAST a region by now.
            return false;
        }

        // we're bound to end up with typos, so let's throw an exception instead of simply returning false
        // that way, the modder can fix their mistake
        if (mod.getRegion(targetRegion) == null)
        {
            throw new Exception("Error proccessing mission script named: " + command.getType() + ", region named: " + targetRegion + " is not defined");
        }

        if (string.IsNullOrEmpty(missionType)) // ie: not a terror mission, not targetting a base, or otherwise not already chosen
        {
            if (!command.hasMissionWeights())
            {
                // no weights means let the strategy pick
                missionType = strategy.chooseRandomMission(targetRegion);
            }
            else
            {
                // otherwise the command gives us the weights.
                missionType = command.generate((uint)month, GenerationType.GEN_MISSION);
            }
        }

        if (string.IsNullOrEmpty(missionType))
        {
            // something went horribly wrong, we didn't manage to choose a mission type
            return false;
        }

        missionRules = mod.getAlienMission(missionType);

        // we're bound to end up with typos, so let's throw an exception instead of simply returning false
        // that way, the modder can fix their mistake
        if (missionRules == null)
        {
            throw new Exception("Error proccessing mission script named: " + command.getType() + ", mission type: " + missionType + " is not defined");
        }

        // do i really need to comment this? shouldn't it be obvious what's happening here?
        if (!command.hasRaceWeights())
        {
            missionRace = missionRules.generateRace((uint)month);
        }
        else
        {
            missionRace = command.generate((uint)month, GenerationType.GEN_RACE);
        }

        if (string.IsNullOrEmpty(missionRace))
        {
            throw new Exception("Error proccessing mission script named: " + command.getType() + ", mission type: " + missionType + " has no available races");
        }

        // we're bound to end up with typos, so let's throw an exception instead of simply returning false
        // that way, the modder can fix their mistake
        if (mod.getAlienRace(missionRace) == null)
        {
            throw new Exception("Error proccessing mission script named: " + command.getType() + ", race: " + missionRace + " is not defined");
        }

        // ok, we've derived all the variables we need to start up our mission, let's do magic to turn those values into a mission
        AlienMission mission = new AlienMission(missionRules);
        mission.setRace(missionRace);
        mission.setId(_game.getSavedGame().getId("ALIEN_MISSIONS"));
        mission.setRegion(targetRegion, _game.getMod());
        mission.setMissionSiteZone(targetZone);
        strategy.addMissionRun(command.getVarName());
        mission.start((uint)command.getDelay());
        _game.getSavedGame().getAlienMissions().Add(mission);
        // if this flag is set, we want to delete it from the table so it won't show up again until the schedule resets.
        if (command.getUseTable())
        {
            strategy.removeMission(targetRegion, missionType);
        }

        // we did it, we can go home now.
        return true;
    }

    /**
     * Check and create supply mission for the given base.
     * There is a 6/101 chance of the mission spawning.
     * @param base A pointer to the alien base.
     */
    void GenerateSupplyMission(AlienBase @base, Mod.Mod mod, SavedGame save)
    {
	    string missionName = @base.getDeployment().chooseGenMissionType();
	    if (mod.getAlienMission(missionName) != null)
	    {
		    if (RNG.percent(@base.getDeployment().getGenMissionFrequency()))
		    {
			    //Spawn supply mission for this base.
			    RuleAlienMission rule = mod.getAlienMission(missionName);
			    AlienMission mission = new AlienMission(rule);
			    mission.setRegion(save.locateRegion(@base).getRules().getType(), mod);
			    mission.setId(save.getId("ALIEN_MISSIONS"));
			    mission.setRace(@base.getAlienRace());
			    mission.setAlienBase(@base);
			    mission.start();
			    save.getAlienMissions().Add(mission);
		    }
	    }
	    else if (!string.IsNullOrEmpty(missionName))
	    {
		    throw new Exception("Alien Base tried to generate undefined mission: " + missionName);
	    }
    }

    /**
     * Returns a pointer to the Geoscape globe for
     * access by other substates.
     * @return Pointer to globe.
     */
    internal Globe getGlobe() =>
	    _globe;

    /**
     * Updates the timer display and resets the palette
     * since it's bound to change on other screens.
     */
    internal override void init()
    {
	    base.init();
	    timeDisplay();

	    _globe.onMouseClick(globeClick);
	    _globe.onMouseOver(null);
	    _globe.rotateStop();
	    _globe.setFocus(true);
	    _globe.draw();

	    // Pop up save screen if it's a new ironman game
	    if (_game.getSavedGame().isIronman() && string.IsNullOrEmpty(_game.getSavedGame().getName()))
	    {
		    popup(new ListSaveState(OptionsOrigin.OPT_GEOSCAPE));
	    }

	    // Set music if it's not already playing
	    if (!_dogfights.Any() && !_dogfightStartTimer.isRunning())
	    {
		    if (_game.getSavedGame().getMonthsPassed() == -1)
		    {
			    _game.getMod().playMusic("GMGEO", 1);
		    }
		    else
		    {
			    _game.getMod().playMusic("GMGEO");
		    }
	    }
	    else
	    {
		    _game.getMod().playMusic("GMINTER");
	    }
	    _globe.setNewBaseHover(false);

		    // run once
	    if (_game.getSavedGame().getMonthsPassed() == -1 &&
		    // as long as there's a base
		    _game.getSavedGame().getBases().Any() &&
		    // and it has a name (THIS prevents it from running prior to the base being placed.)
		    !string.IsNullOrEmpty(_game.getSavedGame().getBases().First().getName()))
	    {
		    _game.getSavedGame().addMonth();
		    determineAlienMissions();
		    _game.getSavedGame().setFunds(_game.getSavedGame().getFunds() - (_game.getSavedGame().getBaseMaintenance() - _game.getSavedGame().getBases().First().getPersonnelMaintenance()));
	    }
    }

    /**
     * Processes any left-clicks on globe markers,
     * or right-clicks to scroll the globe.
     * @param action Pointer to an action.
     */
    void globeClick(Action action)
    {
	    int mouseX = (int)Math.Floor(action.getAbsoluteXMouse()), mouseY = (int)Math.Floor(action.getAbsoluteYMouse());

	    // Clicking markers on the globe
	    if (action.getDetails().button.button == SDL_BUTTON_LEFT)
	    {
		    List<Target> v = _globe.getTargets(mouseX, mouseY, false);
		    if (v.Any())
		    {
			    _game.pushState(new MultipleTargetsState(v, null, this));
		    }
	    }

	    if (_game.getSavedGame().getDebugMode())
	    {
		    double lon, lat;
		    int texture, shade;
		    _globe.cartToPolar((short)mouseX, (short)mouseY, out lon, out lat);
		    double lonDeg = lon / M_PI * 180, latDeg = lat / M_PI * 180;
		    _globe.getPolygonTextureAndShade(lon, lat, out texture, out shade);
		    var ss = new StringBuilder();
		    ss.Append($"rad: {lon}, {lat}{Environment.NewLine}");
		    ss.Append($"deg: {lonDeg}, {latDeg}{Environment.NewLine}");
		    ss.Append($"texture: {texture}, shade: {shade}{Environment.NewLine}");

		    _txtDebug.setText(ss.ToString());
	    }
    }

    /**
     * Runs the game timer and handles popups.
     */
    internal override void think()
    {
	    base.think();

	    _zoomInEffectTimer.think(this, null);
	    _zoomOutEffectTimer.think(this, null);
	    _dogfightStartTimer.think(this, null);

	    if (!_popups.Any() && !_dogfights.Any() && (!_zoomInEffectTimer.isRunning() || _zoomInEffectDone) && (!_zoomOutEffectTimer.isRunning() || _zoomOutEffectDone))
	    {
		    // Handle timers
		    _gameTimer.think(this, null);
	    }
	    else
	    {
		    if (_dogfights.Any() || _minimizedDogfights != 0)
		    {
			    // If all dogfights are minimized rotate the globe, etc.
			    if (_dogfights.Count == _minimizedDogfights)
			    {
				    _pause = false;
				    _gameTimer.think(this, null);
			    }
			    _dogfightTimer.think(this, null);
		    }
		    if (_popups.Any())
		    {
			    // Handle popups
			    _globe.rotateStop();
			    _game.pushState(_popups.First());
                _popups.RemoveAt(0);
		    }
	    }
    }

    /**
     * Handle key shortcuts.
     * @param action Pointer to an action.
     */
    internal override void handle(Action action)
    {
	    if (_dogfights.Count == _minimizedDogfights)
	    {
		    base.handle(action);
	    }

	    if (action.getDetails().type == SDL_EventType.SDL_KEYDOWN)
	    {
		    // "ctrl-d" - enable debug mode
		    if (Options.debug && action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_d && (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0)
		    {
			    _game.getSavedGame().setDebugMode();
			    if (_game.getSavedGame().getDebugMode())
			    {
				    _txtDebug.setText("DEBUG MODE");
			    }
			    else
			    {
				    _txtDebug.setText(string.Empty);
			    }
		    }
		    // "ctrl-c" - delete all soldier commendations
		    if (Options.debug && action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_c && (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0)
		    {
			    if (_game.getSavedGame().getDebugMode())
			    {
				    _txtDebug.setText("SOLDIER COMMENDATIONS DELETED");
				    foreach (var i in _game.getSavedGame().getBases())
				    {
					    foreach (var j in i.getSoldiers())
					    {
						    j.getDiary().getSoldierCommendations().Clear();
					    }
				    }
			    }
			    else
			    {
				    _txtDebug.setText(string.Empty);
			    }
		    }
		    // quick save and quick load
		    else if (!_game.getSavedGame().isIronman())
		    {
			    if (action.getDetails().key.keysym.sym == Options.keyQuickSave)
			    {
				    popup(new SaveGameState(OptionsOrigin.OPT_GEOSCAPE, SaveType.SAVE_QUICK, _palette));
			    }
			    else if (action.getDetails().key.keysym.sym == Options.keyQuickLoad)
			    {
				    popup(new LoadGameState(OptionsOrigin.OPT_GEOSCAPE, SaveType.SAVE_QUICK, _palette));
			    }
		    }
	    }
	    if (_dogfights.Any())
	    {
		    foreach (var it in _dogfights)
		    {
			    it.handle(action);
		    }
		    _minimizedDogfights = (uint)minimizedDogfightsCount();
	    }
    }

    /**
     * Gets the number of minimized dogfights.
     * @return Number of minimized dogfights.
     */
    int minimizedDogfightsCount()
    {
	    int minimizedDogfights = 0;
	    foreach (var d in _dogfights)
	    {
		    if (d.isMinimized())
		    {
			    ++minimizedDogfights;
		    }
	    }
	    return minimizedDogfights;
    }

    /**
     * Updates the scale.
     * @param dX delta of X;
     * @param dY delta of Y;
     */
    internal override void resize(ref int dX, ref int dY)
    {
	    if (_game.getSavedGame().getSavedBattle() != null)
		    return;
	    dX = Options.baseXResolution;
	    dY = Options.baseYResolution;
	    int divisor = 1;
	    double pixelRatioY = 1.0;

	    if (Options.nonSquarePixelRatio)
	    {
		    pixelRatioY = 1.2;
	    }
	    switch ((ScaleType)Options.geoscapeScale)
	    {
	        case ScaleType.SCALE_SCREEN_DIV_3:
		        divisor = 3;
		        break;
	        case ScaleType.SCALE_SCREEN_DIV_2:
		        divisor = 2;
		        break;
	        case ScaleType.SCALE_SCREEN:
		        break;
	        default:
		        dX = 0;
		        dY = 0;
		        return;
	    }

	    Options.baseXResolution = Math.Max(Screen.ORIGINAL_WIDTH, Options.displayWidth / divisor);
	    Options.baseYResolution = Math.Max(Screen.ORIGINAL_HEIGHT, (int)(Options.displayHeight / pixelRatioY / divisor));

	    dX = Options.baseXResolution - dX;
	    dY = Options.baseYResolution - dY;

	    _globe.resize();

	    foreach (var i in _surfaces)
	    {
		    if (i != _globe)
		    {
			    i.setX(i.getX() + dX);
			    i.setY(i.getY() + dY/2);
		    }
	    }

	    _bg.setX((_globe.getWidth() - _bg.getWidth()) / 2);
	    _bg.setY((_globe.getHeight() - _bg.getHeight()) / 2);

	    int height = (Options.baseYResolution - Screen.ORIGINAL_HEIGHT) / 2 + 10;
	    _sideTop.setHeight(height);
	    _sideTop.setY(_sidebar.getY() - height - 1);
	    _sideBottom.setHeight(height);
	    _sideBottom.setY(_sidebar.getY() + _sidebar.getHeight() + 1);

	    _sideLine.setHeight(Options.baseYResolution);
	    _sideLine.setY(0);
	    _sideLine.drawRect(0, 0, (short)_sideLine.getWidth(), (short)_sideLine.getHeight(), 15);
    }

    /**
     * Handle blitting of Geoscape and Dogfights.
     */
    internal override void blit()
    {
	    base.blit();
	    foreach (var it in _dogfights)
	    {
		    it.blit();
	    }
    }
}
