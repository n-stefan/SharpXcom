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
    Engine.Timer _gameTimer, _zoomInEffectTimer, _zoomOutEffectTimer, _dogfightStartTimer, _dogfightTimer;
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
        _gameTimer = new Engine.Timer((uint)Options.geoClockSpeed);

        _zoomInEffectTimer = new Engine.Timer((uint)Options.dogfightSpeed);
        _zoomOutEffectTimer = new Engine.Timer((uint)Options.dogfightSpeed);
        _dogfightStartTimer = new Engine.Timer((uint)Options.dogfightSpeed);
        _dogfightTimer = new Engine.Timer((uint)Options.dogfightSpeed);

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
        _btn5Secs.setGroup(ref _timeSpeed);
        _btn5Secs.onKeyboardPress(btnTimerClick, Options.keyGeoSpeed1);
        _btn5Secs.setGeoscapeButton(true);

        _btn1Min.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btn1Min.setBig();
        _btn1Min.setText(tr("STR_1_MINUTE"));
        _btn1Min.setGroup(ref _timeSpeed);
        _btn1Min.onKeyboardPress(btnTimerClick, Options.keyGeoSpeed2);
        _btn1Min.setGeoscapeButton(true);

        _btn5Mins.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btn5Mins.setBig();
        _btn5Mins.setText(tr("STR_5_MINUTES"));
        _btn5Mins.setGroup(ref _timeSpeed);
        _btn5Mins.onKeyboardPress(btnTimerClick, Options.keyGeoSpeed3);
        _btn5Mins.setGeoscapeButton(true);

        _btn30Mins.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btn30Mins.setBig();
        _btn30Mins.setText(tr("STR_30_MINUTES"));
        _btn30Mins.setGroup(ref _timeSpeed);
        _btn30Mins.onKeyboardPress(btnTimerClick, Options.keyGeoSpeed4);
        _btn30Mins.setGeoscapeButton(true);

        _btn1Hour.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btn1Hour.setBig();
        _btn1Hour.setText(tr("STR_1_HOUR"));
        _btn1Hour.setGroup(ref _timeSpeed);
        _btn1Hour.onKeyboardPress(btnTimerClick, Options.keyGeoSpeed5);
        _btn1Hour.setGeoscapeButton(true);

        _btn1Day.initText(_game.getMod().getFont("FONT_GEO_BIG"), _game.getMod().getFont("FONT_GEO_SMALL"), _game.getLanguage());
        _btn1Day.setBig();
        _btn1Day.setText(tr("STR_1_DAY"));
        _btn1Day.setGroup(ref _timeSpeed);
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
    void btnInterceptClick(Engine.Action _)
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
    void btnBasesClick(Engine.Action _)
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
            _game.pushState(new BasescapeState(0, _globe));
        }
    }

    /**
     * Goes to the Graphs screen.
     * @param action Pointer to an action.
     */
    void btnGraphsClick(Engine.Action _)
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
    void btnUfopaediaClick(Engine.Action _)
    {
        if (buttonsDisabled())
        {
            return;
        }
        Ufopaedia.open(_game);
    }

    /**
     * Opens the Options window.
     * @param action Pointer to an action.
     */
    void btnOptionsClick(Engine.Action _)
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
    void btnFundingClick(Engine.Action _)
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
    void btnTimerClick(Engine.Action action)
    {
        var ev = new SDL_Event();
        ev.type = SDL_EventType.SDL_MOUSEBUTTONDOWN;
        ev.button.button = (byte)SDL_BUTTON_LEFT;
        var a = new Engine.Action(ev, 0.0, 0.0, 0, 0);
        action.getSender().mousePress(a, this);
    }

    /**
     * Starts rotating the globe to the left.
     * @param action Pointer to an action.
     */
    void btnRotateLeftPress(Engine.Action _) =>
        _globe.rotateLeft();

    /**
     * Stops rotating the globe to the left.
     * @param action Pointer to an action.
     */
    void btnRotateLeftRelease(Engine.Action _) =>
        _globe.rotateStopLon();

    /**
     * Starts rotating the globe to the right.
     * @param action Pointer to an action.
     */
    void btnRotateRightPress(Engine.Action _) =>
        _globe.rotateRight();

    /**
     * Stops rotating the globe to the right.
     * @param action Pointer to an action.
     */
    void btnRotateRightRelease(Engine.Action _) =>
        _globe.rotateStopLon();

    /**
     * Starts rotating the globe upwards.
     * @param action Pointer to an action.
     */
    void btnRotateUpPress(Engine.Action _) =>
        _globe.rotateUp();

    /**
     * Stops rotating the globe upwards.
     * @param action Pointer to an action.
     */
    void btnRotateUpRelease(Engine.Action _) =>
        _globe.rotateStopLat();

    /**
     * Starts rotating the globe downwards.
     * @param action Pointer to an action.
     */
    void btnRotateDownPress(Engine.Action _) =>
        _globe.rotateDown();

    /**
     * Stops rotating the globe downwards.
     * @param action Pointer to an action.
     */
    void btnRotateDownRelease(Engine.Action _) =>
        _globe.rotateStopLat();

    /**
     * Zooms into the globe.
     * @param action Pointer to an action.
     */
    void btnZoomInLeftClick(Engine.Action _) =>
        _globe.zoomIn();

    /**
     * Zooms the globe maximum.
     * @param action Pointer to an action.
     */
    void btnZoomInRightClick(Engine.Action _) =>
        _globe.zoomMax();

    /**
     * Zooms out of the globe.
     * @param action Pointer to an action.
     */
    void btnZoomOutLeftClick(Engine.Action _) =>
        _globe.zoomOut();

    /**
     * Zooms the globe minimum.
     * @param action Pointer to an action.
     */
    void btnZoomOutRightClick(Engine.Action _) =>
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
                    break;
                case TimeTrigger.TIME_1DAY:
                    time1Day();
                    break;
                case TimeTrigger.TIME_1HOUR:
                    time1Hour();
                    break;
                case TimeTrigger.TIME_30MIN:
                    time30Minutes();
                    break;
                case TimeTrigger.TIME_10MIN:
                    time10Minutes();
                    break;
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
                _dogfights.Last().setInterceptionsCount(_dogfights.Count + _dogfightsToBeStarted.Count);
            }
            // Set correct number of interceptions for every dogfight.
            foreach (var d in _dogfights)
            {
                d.setInterceptionsCount(_dogfights.Count);
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
    void timerReset()
    {
        var ev = new SDL_Event();
        ev.button.button = (byte)SDL_BUTTON_LEFT;
        var act = new Engine.Action(ev, _game.getScreen().getXScale(), _game.getScreen().getYScale(), _game.getScreen().getCursorTopBlackBand(), _game.getScreen().getCursorLeftBlackBand());
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
    void popup(State state)
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
                _game.getSavedGame().getNewlyAvailableResearchProjects(before, after, newPossibleResearch);
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
        _game.getSavedGame().getAlienBases().ForEach(x => GenerateSupplyMission(_game.getMod(), _game.getSavedGame()));

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
}
