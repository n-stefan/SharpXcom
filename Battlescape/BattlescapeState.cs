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
    const int VISIBLE_MAX = 10;

    Map _map;
    BattlescapeButton _btnEndTurn, _btnAbort, _btnLaunch, _btnPsi, _reserve;
    BattlescapeGame _battleGame;
    WarningMessage _warning;
    Timer _animTimer, _gameTimer;
    SavedBattleGame _save;
    List<State> _popups;
    InteractiveSurface[] _btnVisibleUnit = new InteractiveSurface[VISIBLE_MAX];
    NumberText[] _numVisibleUnit = new NumberText[VISIBLE_MAX];
    BattleUnit[] _visibleUnit = new BattleUnit[VISIBLE_MAX];
    Surface _rank;
    NumberText _numTimeUnits, _numEnergy, _numHealth, _numMorale, _numLayers, _numAmmoLeft, _numAmmoRight;
    Bar _barTimeUnits, _barEnergy, _barHealth, _barMorale;
    InteractiveSurface _btnLeftHandItem, _btnRightHandItem;
    Text _txtName;
    BattlescapeButton _btnUnitUp, _btnUnitDown, _btnMapUp, _btnMapDown, _btnShowMap, _btnKneel;
    bool _isMouseScrolling, _isMouseScrolled;
    bool _autosave;
    bool _firstInit;
	BattlescapeButton _btnReserveNone, _btnReserveSnap, _btnReserveAimed, _btnReserveAuto, _btnReserveKneel, _btnZeroTUs;
	Text _txtDebug, _txtTooltip;
	Position _cursorPosition;
	int _xBeforeMouseScrolling, _yBeforeMouseScrolling;
	int _totalMouseMoveX, _totalMouseMoveY;
	bool _mouseMovedOverThreshold;
	bool _mouseOverIcons;
	InteractiveSurface _icons;
	BattlescapeButton _btnInventory, _btnCenter, _btnNextSoldier, _btnNextStop, _btnShowLayers, _btnHelp;
	InteractiveSurface _btnStats;
	byte _barHealthColor;
	uint _mouseScrollingStartTime;
	Position _mapOffsetBeforeMouseScrolling;
	string _currentTooltip;

	/**
	 * Initializes all the elements in the Battlescape screen.
	 * @param game Pointer to the core game.
	 */
	internal BattlescapeState()
	{
		_reserve = null;
		_firstInit = true;
		_isMouseScrolling = false;
		_isMouseScrolled = false;
		_xBeforeMouseScrolling = 0;
		_yBeforeMouseScrolling = 0;
		_totalMouseMoveX = 0;
		_totalMouseMoveY = 0;
		_mouseMovedOverThreshold = false;
		_mouseOverIcons = false;
		_autosave = false;

		for (var i = 0; i < 10; i++) _visibleUnit[i] = new BattleUnit(null, 0);

		int screenWidth = Options.baseXResolution;
		int screenHeight = Options.baseYResolution;
		int iconsWidth = _game.getMod().getInterface("battlescape").getElement("icons").w;
		int iconsHeight = _game.getMod().getInterface("battlescape").getElement("icons").h;
		int visibleMapHeight = screenHeight - iconsHeight;
		int x = screenWidth/2 - iconsWidth/2;
		int y = screenHeight - iconsHeight;

		// Create buttonbar - this should be on the centerbottom of the screen
		_icons = new InteractiveSurface(iconsWidth, iconsHeight, x, y);

		// Create the battlemap view
		// the actual map height is the total height minus the height of the buttonbar
		_map = new Map(_game, screenWidth, screenHeight, 0, 0, visibleMapHeight);

		_numLayers = new NumberText(3, 5, x + 232, y + 6);
		_rank = new Surface(26, 23, x + 107, y + 33);

		// Create buttons
		_btnUnitUp = new BattlescapeButton(32, 16, x + 48, y);
		_btnUnitDown = new BattlescapeButton(32, 16, x + 48, y + 16);
		_btnMapUp = new BattlescapeButton(32, 16, x + 80, y);
		_btnMapDown = new BattlescapeButton(32, 16, x + 80, y + 16);
		_btnShowMap = new BattlescapeButton(32, 16, x + 112, y);
		_btnKneel = new BattlescapeButton(32, 16, x + 112, y + 16);
		_btnInventory = new BattlescapeButton(32, 16, x + 144, y);
		_btnCenter = new BattlescapeButton(32, 16, x + 144, y + 16);
		_btnNextSoldier = new BattlescapeButton(32, 16, x + 176, y);
		_btnNextStop = new BattlescapeButton(32, 16, x + 176, y + 16);
		_btnShowLayers = new BattlescapeButton(32, 16, x + 208, y);
		_btnHelp = new BattlescapeButton(32, 16, x + 208, y + 16);
		_btnEndTurn = new BattlescapeButton(32, 16, x + 240, y);
		_btnAbort = new BattlescapeButton(32, 16, x + 240, y + 16);
		_btnStats = new InteractiveSurface(164, 23, x + 107, y + 33);
		_btnReserveNone = new BattlescapeButton(17, 11, x + 60, y + 33);
		_btnReserveSnap = new BattlescapeButton(17, 11, x + 78, y + 33);
		_btnReserveAimed = new BattlescapeButton(17, 11, x + 60, y + 45);
		_btnReserveAuto = new BattlescapeButton(17, 11, x + 78, y + 45);
		_btnReserveKneel = new BattlescapeButton(10, 23, x + 96, y + 33);
		_btnZeroTUs = new BattlescapeButton(10, 23, x + 49, y + 33);
		_btnLeftHandItem = new InteractiveSurface(32, 48, x + 8, y + 4);
		_numAmmoLeft = new NumberText(30, 5, x + 8, y + 4);
		_btnRightHandItem = new InteractiveSurface(32, 48, x + 280, y + 4);
		_numAmmoRight = new NumberText(30, 5, x + 280, y + 4);
		int visibleUnitX = _game.getMod().getInterface("battlescape").getElement("visibleUnits").x;
		int visibleUnitY = _game.getMod().getInterface("battlescape").getElement("visibleUnits").y;
		for (int i = 0; i < VISIBLE_MAX; ++i)
		{
			_btnVisibleUnit[i] = new InteractiveSurface(15, 12, x + visibleUnitX, y + visibleUnitY - (i * 13));
			_numVisibleUnit[i] = new NumberText(15, 12, _btnVisibleUnit[i].getX() + 6 , _btnVisibleUnit[i].getY() + 4);
		}
		_numVisibleUnit[9].setX(_numVisibleUnit[9].getX() - 2); // center number 10
		_warning = new WarningMessage(224, 24, x + 48, y + 32);
		_btnLaunch = new BattlescapeButton(32, 24, screenWidth - 32, 0); // we need screenWidth, because that is independent of the black bars on the screen
		_btnLaunch.setVisible(false);
		_btnPsi = new BattlescapeButton(32, 24, screenWidth - 32, 25); // we need screenWidth, because that is independent of the black bars on the screen
		_btnPsi.setVisible(false);

		// Create soldier stats summary
		_txtName = new Text(136, 10, x + 135, y + 32);

		_numTimeUnits = new NumberText(15, 5, x + 136, y + 42);
		_barTimeUnits = new Bar(102, 3, x + 170, y + 41);

		_numEnergy = new NumberText(15, 5, x + 154, y + 42);
		_barEnergy = new Bar(102, 3, x + 170, y + 45);

		_numHealth = new NumberText(15, 5, x + 136, y + 50);
		_barHealth= new Bar(102, 3, x + 170, y + 49);

		_numMorale = new NumberText(15, 5, x + 154, y + 50);
		_barMorale = new Bar(102, 3, x + 170, y + 53);

		_txtDebug = new Text(300, 10, 20, 0);
		_txtTooltip = new Text(300, 10, x + 2, y - 10);

		// Set palette
		_game.getSavedGame().getSavedBattle().setPaletteByDepth(this);

		if (_game.getMod().getInterface("battlescape").getElement("pathfinding") != default)
		{
			Element pathing = _game.getMod().getInterface("battlescape").getElement("pathfinding");

			Pathfinding.green = pathing.color;
			Pathfinding.yellow = pathing.color2;
			Pathfinding.red = pathing.border;
		}

		add(_map);
		add(_icons);

		// Add in custom reserve buttons
		Surface icons = _game.getMod().getSurface("ICONS.PCK");
		if (_game.getMod().getSurface("TFTDReserve", false) != null)
		{
			Surface tftdIcons = _game.getMod().getSurface("TFTDReserve");
			tftdIcons.setX(48);
			tftdIcons.setY(176);
			tftdIcons.blit(icons);
		}

		// there is some cropping going on here, because the icons image is 320x200 while we only need the bottom of it.
		SDL_Rect r = icons.getCrop();
		r.x = 0;
		r.y = 200 - iconsHeight;
		r.w = iconsWidth;
		r.h = iconsHeight;
		// we need to blit the icons before we add the battlescape buttons, as they copy the underlying parent surface.
		icons.blit(_icons);

		// this is a hack to fix the single transparent pixel on TFTD's icon panel.
		if (_game.getMod().getInterface("battlescape").getElement("icons").TFTDMode)
		{
			_icons.setPixel(46, 44, 8);
		}

		add(_rank, "rank", "battlescape", _icons);
		add(_btnUnitUp, "buttonUnitUp", "battlescape", _icons);
		add(_btnUnitDown, "buttonUnitDown", "battlescape", _icons);
		add(_btnMapUp, "buttonMapUp", "battlescape", _icons);
		add(_btnMapDown, "buttonMapDown", "battlescape", _icons);
		add(_btnShowMap, "buttonShowMap", "battlescape", _icons);
		add(_btnKneel, "buttonKneel", "battlescape", _icons);
		add(_btnInventory, "buttonInventory", "battlescape", _icons);
		add(_btnCenter, "buttonCenter", "battlescape", _icons);
		add(_btnNextSoldier, "buttonNextSoldier", "battlescape", _icons);
		add(_btnNextStop, "buttonNextStop", "battlescape", _icons);
		add(_btnShowLayers, "buttonShowLayers", "battlescape", _icons);
		add(_numLayers, "numLayers", "battlescape", _icons);
		add(_btnHelp, "buttonHelp", "battlescape", _icons);
		add(_btnEndTurn, "buttonEndTurn", "battlescape", _icons);
		add(_btnAbort, "buttonAbort", "battlescape", _icons);
		add(_btnStats, "buttonStats", "battlescape", _icons);
		add(_txtName, "textName", "battlescape", _icons);
		add(_numTimeUnits, "numTUs", "battlescape", _icons);
		add(_numEnergy, "numEnergy", "battlescape", _icons);
		add(_numHealth, "numHealth", "battlescape", _icons);
		add(_numMorale, "numMorale", "battlescape", _icons);
		add(_barTimeUnits, "barTUs", "battlescape", _icons);
		add(_barEnergy, "barEnergy", "battlescape", _icons);
		add(_barHealth, "barHealth", "battlescape", _icons);
		add(_barMorale, "barMorale", "battlescape", _icons);
		add(_btnReserveNone, "buttonReserveNone", "battlescape", _icons);
		add(_btnReserveSnap, "buttonReserveSnap", "battlescape", _icons);
		add(_btnReserveAimed, "buttonReserveAimed", "battlescape", _icons);
		add(_btnReserveAuto, "buttonReserveAuto", "battlescape", _icons);
		add(_btnReserveKneel, "buttonReserveKneel", "battlescape", _icons);
		add(_btnZeroTUs, "buttonZeroTUs", "battlescape", _icons);
		add(_btnLeftHandItem, "buttonLeftHand", "battlescape", _icons);
		add(_numAmmoLeft, "numAmmoLeft", "battlescape", _icons);
		add(_btnRightHandItem, "buttonRightHand", "battlescape", _icons);
		add(_numAmmoRight, "numAmmoRight", "battlescape", _icons);
		for (int i = 0; i < VISIBLE_MAX; ++i)
		{
			add(_btnVisibleUnit[i]);
			add(_numVisibleUnit[i]);
		}
		add(_warning, "warning", "battlescape", _icons);
		add(_txtDebug);
		add(_txtTooltip, "textTooltip", "battlescape", _icons);
		add(_btnLaunch);
		_game.getMod().getSurfaceSet("SPICONS.DAT").getFrame(0).blit(_btnLaunch);
		add(_btnPsi);
		_game.getMod().getSurfaceSet("SPICONS.DAT").getFrame(1).blit(_btnPsi);

		// Set up objects
		_save = _game.getSavedGame().getSavedBattle();
		_map.init();
		_map.onMouseOver(mapOver);
		_map.onMousePress(mapPress);
		_map.onMouseClick(mapClick, 0);
		_map.onMouseIn(mapIn);

		_numLayers.setColor((byte)(Palette.blockOffset(1)-2));
		_numLayers.setValue(1);

		_numAmmoLeft.setValue(999);

		_numAmmoRight.setValue(999);

		_icons.onMouseIn(mouseInIcons);
		_icons.onMouseOut(mouseOutIcons);

		_btnUnitUp.onMouseClick(btnUnitUpClick);
		_btnUnitUp.setTooltip("STR_UNIT_LEVEL_ABOVE");
		_btnUnitUp.onMouseIn(txtTooltipIn);
		_btnUnitUp.onMouseOut(txtTooltipOut);

		_btnUnitDown.onMouseClick(btnUnitDownClick);
		_btnUnitDown.setTooltip("STR_UNIT_LEVEL_BELOW");
		_btnUnitDown.onMouseIn(txtTooltipIn);
		_btnUnitDown.onMouseOut(txtTooltipOut);

		_btnMapUp.onMouseClick(btnMapUpClick);
		_btnMapUp.onKeyboardPress(btnMapUpClick, Options.keyBattleLevelUp);
		_btnMapUp.setTooltip("STR_VIEW_LEVEL_ABOVE");
		_btnMapUp.onMouseIn(txtTooltipIn);
		_btnMapUp.onMouseOut(txtTooltipOut);

		_btnMapDown.onMouseClick(btnMapDownClick);
		_btnMapDown.onKeyboardPress(btnMapDownClick, Options.keyBattleLevelDown);
		_btnMapDown.setTooltip("STR_VIEW_LEVEL_BELOW");
		_btnMapDown.onMouseIn(txtTooltipIn);
		_btnMapDown.onMouseOut(txtTooltipOut);

		_btnShowMap.onMouseClick(btnShowMapClick);
		_btnShowMap.onKeyboardPress(btnShowMapClick, Options.keyBattleMap);
		_btnShowMap.setTooltip("STR_MINIMAP");
		_btnShowMap.onMouseIn(txtTooltipIn);
		_btnShowMap.onMouseOut(txtTooltipOut);

		_btnKneel.onMouseClick(btnKneelClick);
		_btnKneel.onKeyboardPress(btnKneelClick, Options.keyBattleKneel);
		_btnKneel.setTooltip("STR_KNEEL");
		_btnKneel.onMouseIn(txtTooltipIn);
		_btnKneel.onMouseOut(txtTooltipOut);
		_btnKneel.allowToggleInversion();

		_btnInventory.onMouseClick(btnInventoryClick);
		_btnInventory.onKeyboardPress(btnInventoryClick, Options.keyBattleInventory);
		_btnInventory.setTooltip("STR_INVENTORY");
		_btnInventory.onMouseIn(txtTooltipIn);
		_btnInventory.onMouseOut(txtTooltipOut);

		_btnCenter.onMouseClick(btnCenterClick);
		_btnCenter.onKeyboardPress(btnCenterClick, Options.keyBattleCenterUnit);
		_btnCenter.setTooltip("STR_CENTER_SELECTED_UNIT");
		_btnCenter.onMouseIn(txtTooltipIn);
		_btnCenter.onMouseOut(txtTooltipOut);

		_btnNextSoldier.onMouseClick(btnNextSoldierClick);
		_btnNextSoldier.onKeyboardPress(btnNextSoldierClick, Options.keyBattleNextUnit);
		_btnNextSoldier.onKeyboardPress(btnPrevSoldierClick, Options.keyBattlePrevUnit);
		_btnNextSoldier.setTooltip("STR_NEXT_UNIT");
		_btnNextSoldier.onMouseIn(txtTooltipIn);
		_btnNextSoldier.onMouseOut(txtTooltipOut);

		_btnNextStop.onMouseClick(btnNextStopClick);
		_btnNextStop.onKeyboardPress(btnNextStopClick, Options.keyBattleDeselectUnit);
		_btnNextStop.setTooltip("STR_DESELECT_UNIT");
		_btnNextStop.onMouseIn(txtTooltipIn);
		_btnNextStop.onMouseOut(txtTooltipOut);

		_btnShowLayers.onMouseClick(btnShowLayersClick);
		_btnShowLayers.setTooltip("STR_MULTI_LEVEL_VIEW");
		_btnShowLayers.onMouseIn(txtTooltipIn);
		_btnShowLayers.onMouseOut(txtTooltipOut);

		_btnHelp.onMouseClick(btnHelpClick);
		_btnHelp.onKeyboardPress(btnHelpClick, Options.keyBattleOptions);
		_btnHelp.setTooltip("STR_OPTIONS");
		_btnHelp.onMouseIn(txtTooltipIn);
		_btnHelp.onMouseOut(txtTooltipOut);

		_btnEndTurn.onMouseClick(btnEndTurnClick);
		_btnEndTurn.onKeyboardPress(btnEndTurnClick, Options.keyBattleEndTurn);
		_btnEndTurn.setTooltip("STR_END_TURN");
		_btnEndTurn.onMouseIn(txtTooltipIn);
		_btnEndTurn.onMouseOut(txtTooltipOut);

		_btnAbort.onMouseClick(btnAbortClick);
		_btnAbort.onKeyboardPress(btnAbortClick, Options.keyBattleAbort);
		_btnAbort.setTooltip("STR_ABORT_MISSION");
		_btnAbort.onMouseIn(txtTooltipIn);
		_btnAbort.onMouseOut(txtTooltipOut);

		_btnStats.onMouseClick(btnStatsClick);
		_btnStats.onKeyboardPress(btnStatsClick, Options.keyBattleStats);
		_btnStats.setTooltip("STR_UNIT_STATS");
		_btnStats.onMouseIn(txtTooltipIn);
		_btnStats.onMouseOut(txtTooltipOut);

		_btnLeftHandItem.onMouseClick(btnLeftHandItemClick);
		_btnLeftHandItem.onKeyboardPress(btnLeftHandItemClick, Options.keyBattleUseLeftHand);
		_btnLeftHandItem.setTooltip("STR_USE_LEFT_HAND");
		_btnLeftHandItem.onMouseIn(txtTooltipIn);
		_btnLeftHandItem.onMouseOut(txtTooltipOut);

		_btnRightHandItem.onMouseClick(btnRightHandItemClick);
		_btnRightHandItem.onKeyboardPress(btnRightHandItemClick, Options.keyBattleUseRightHand);
		_btnRightHandItem.setTooltip("STR_USE_RIGHT_HAND");
		_btnRightHandItem.onMouseIn(txtTooltipIn);
		_btnRightHandItem.onMouseOut(txtTooltipOut);

		_btnReserveNone.onMouseClick(btnReserveClick);
		_btnReserveNone.onKeyboardPress(btnReserveClick, Options.keyBattleReserveNone);
		_btnReserveNone.setTooltip("STR_DONT_RESERVE_TIME_UNITS");
		_btnReserveNone.onMouseIn(txtTooltipIn);
		_btnReserveNone.onMouseOut(txtTooltipOut);

		_btnReserveSnap.onMouseClick(btnReserveClick);
		_btnReserveSnap.onKeyboardPress(btnReserveClick, Options.keyBattleReserveSnap);
		_btnReserveSnap.setTooltip("STR_RESERVE_TIME_UNITS_FOR_SNAP_SHOT");
		_btnReserveSnap.onMouseIn(txtTooltipIn);
		_btnReserveSnap.onMouseOut(txtTooltipOut);

		_btnReserveAimed.onMouseClick(btnReserveClick);
		_btnReserveAimed.onKeyboardPress(btnReserveClick, Options.keyBattleReserveAimed);
		_btnReserveAimed.setTooltip("STR_RESERVE_TIME_UNITS_FOR_AIMED_SHOT");
		_btnReserveAimed.onMouseIn(txtTooltipIn);
		_btnReserveAimed.onMouseOut(txtTooltipOut);

		_btnReserveAuto.onMouseClick(btnReserveClick);
		_btnReserveAuto.onKeyboardPress(btnReserveClick, Options.keyBattleReserveAuto);
		_btnReserveAuto.setTooltip("STR_RESERVE_TIME_UNITS_FOR_AUTO_SHOT");
		_btnReserveAuto.onMouseIn(txtTooltipIn);
		_btnReserveAuto.onMouseOut(txtTooltipOut);

		_btnReserveKneel.onMouseClick(btnReserveKneelClick);
		_btnReserveKneel.onKeyboardPress(btnReserveKneelClick, Options.keyBattleReserveKneel);
		_btnReserveKneel.setTooltip("STR_RESERVE_TIME_UNITS_FOR_KNEEL");
		_btnReserveKneel.onMouseIn(txtTooltipIn);
		_btnReserveKneel.onMouseOut(txtTooltipOut);
		_btnReserveKneel.allowToggleInversion();

		_btnZeroTUs.onMouseClick(btnZeroTUsClick, (byte)SDL_BUTTON_RIGHT);
		_btnZeroTUs.onKeyboardPress(btnZeroTUsClick, Options.keyBattleZeroTUs);
		_btnZeroTUs.setTooltip("STR_EXPEND_ALL_TIME_UNITS");
		_btnZeroTUs.onMouseIn(txtTooltipIn);
		_btnZeroTUs.onMouseOut(txtTooltipOut);
		_btnZeroTUs.allowClickInversion();

		// shortcuts without a specific button
		_btnStats.onKeyboardPress(btnReloadClick, Options.keyBattleReload);
		_btnStats.onKeyboardPress(btnPersonalLightingClick, Options.keyBattlePersonalLighting);

		SDL_Keycode[] buttons = {Options.keyBattleCenterEnemy1,
							Options.keyBattleCenterEnemy2,
							Options.keyBattleCenterEnemy3,
							Options.keyBattleCenterEnemy4,
							Options.keyBattleCenterEnemy5,
							Options.keyBattleCenterEnemy6,
							Options.keyBattleCenterEnemy7,
							Options.keyBattleCenterEnemy8,
							Options.keyBattleCenterEnemy9,
							Options.keyBattleCenterEnemy10};
		byte color = (byte)_game.getMod().getInterface("battlescape").getElement("visibleUnits").color;
		for (int i = 0; i < VISIBLE_MAX; ++i)
		{
			string tooltip;
			_btnVisibleUnit[i].onMouseClick(btnVisibleUnitClick);
			_btnVisibleUnit[i].onKeyboardPress(btnVisibleUnitClick, buttons[i]);
			tooltip = $"STR_CENTER_ON_ENEMY_{i+1}";
			_btnVisibleUnit[i].setTooltip(tooltip);
			_btnVisibleUnit[i].onMouseIn(txtTooltipIn);
			_btnVisibleUnit[i].onMouseOut(txtTooltipOut);
			_numVisibleUnit[i].setColor(color);
			_numVisibleUnit[i].setValue((uint)(i + 1));
		}
		_warning.setColor((byte)_game.getMod().getInterface("battlescape").getElement("warning").color2);
		_warning.setTextColor((byte)_game.getMod().getInterface("battlescape").getElement("warning").color);
		_btnLaunch.onMouseClick(btnLaunchClick);
		_btnPsi.onMouseClick(btnPsiClick);

		_txtName.setHighContrast(true);

		_barTimeUnits.setScale(1.0);
		_barEnergy.setScale(1.0);
		_barHealth.setScale(1.0);
		_barMorale.setScale(1.0);

		_txtDebug.setColor(Palette.blockOffset(8));
		_txtDebug.setHighContrast(true);

		_txtTooltip.setHighContrast(true);

		_btnReserveNone.setGroup(_reserve);
		_btnReserveSnap.setGroup(_reserve);
		_btnReserveAimed.setGroup(_reserve);
		_btnReserveAuto.setGroup(_reserve);

		// Set music
		if (string.IsNullOrEmpty(_save.getMusic()))
		{
			_game.getMod().playMusic("GMTACTIC");
		}
		else
		{
			_game.getMod().playMusic(_save.getMusic());
		}

		_animTimer = new Timer(DEFAULT_ANIM_SPEED, true);
		_animTimer.onTimer((StateHandler)animate);

		_gameTimer = new Timer(DEFAULT_ANIM_SPEED, true);
		_gameTimer.onTimer((StateHandler)handleState);

		_battleGame = new BattlescapeGame(_save, this);

		_barHealthColor = _barHealth.getColor();
	}

	/**
	 * Deletes the battlescapestate.
	 */
	~BattlescapeState()
	{
		_animTimer = null;
		_gameTimer = null;
		_battleGame = null;
	}

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

    /**
     * Finishes up the current battle, shuts down the battlescape
     * and presents the debriefing screen for the mission.
     * @param abort Was the mission aborted?
     * @param inExitArea Number of soldiers in the exit area OR number of survivors when battle finished due to either all aliens or objective being destroyed.
     */
    internal void finishBattle(bool abort, int inExitArea)
    {
        while (!_game.isState(this))
        {
            _game.popState();
        }
        _game.getCursor().setVisible(true);
        if (_save.getAmbientSound() != -1)
        {
            _game.getMod().getSoundByDepth(0, (uint)_save.getAmbientSound()).stopLoop();
        }
        AlienDeployment ruleDeploy = _game.getMod().getDeployment(_save.getMissionType());
        if (ruleDeploy == null)
        {
            foreach (var ufo in _game.getSavedGame().getUfos())
            {
                if (ufo.isInBattlescape())
                {
                    ruleDeploy = _game.getMod().getDeployment(ufo.getRules().getType());
                    break;
                }
            }
        }
        string nextStage = null;
        if (ruleDeploy != null)
        {
            nextStage = ruleDeploy.getNextStage();
        }

        if (!string.IsNullOrEmpty(nextStage) && inExitArea != 0)
        {
            // if there is a next mission stage + we have people in exit area OR we killed all aliens, load the next stage
            _popups.Clear();
            _save.setMissionType(nextStage);
            BattlescapeGenerator bgen = new BattlescapeGenerator(_game);
            bgen.nextStage();
            _game.popState();
            _game.pushState(new BriefingState(null, null));
        }
        else
        {
            _popups.Clear();
            _animTimer.stop();
            _gameTimer.stop();
            _game.popState();
            _game.pushState(new DebriefingState());
            string cutscene = null;
            if (ruleDeploy != null)
            {
                if (abort)
                {
                    cutscene = ruleDeploy.getAbortCutscene();
                }
                else if (inExitArea == 0)
                {
                    cutscene = ruleDeploy.getLoseCutscene();
                }
                else
                {
                    cutscene = ruleDeploy.getWinCutscene();
                }
            }
            if (!string.IsNullOrEmpty(cutscene))
            {
                // if cutscene is "wingame" or "losegame", then the DebriefingState
                // pushed above will get popped without being shown.  otherwise
                // it will get shown after the cutscene.
                _game.pushState(new CutsceneState(cutscene));

                if (cutscene == CutsceneState.WIN_GAME)
                {
                    _game.getSavedGame().setEnding(GameEnding.END_WIN);
                }
                else if (cutscene == CutsceneState.LOSE_GAME)
                {
                    _game.getSavedGame().setEnding(GameEnding.END_LOSE);
                }
                // Autosave if game is over
                if (_game.getSavedGame().getEnding() != GameEnding.END_NONE && _game.getSavedGame().isIronman())
                {
                    _game.pushState(new SaveGameState(OptionsOrigin.OPT_BATTLESCAPE, SaveType.SAVE_IRONMAN, _palette));
                }
            }
        }
    }

    /**
     * Updates a soldier's name/rank/tu/energy/health/morale.
     */
    internal void updateSoldierInfo(bool checkFOV = true)
    {
        BattleUnit battleUnit = _save.getSelectedUnit();

        for (int i = 0; i < VISIBLE_MAX; ++i)
        {
            _btnVisibleUnit[i].setVisible(false);
            _numVisibleUnit[i].setVisible(false);
            _visibleUnit[i] = null;
        }

        bool playableUnit = _battleGame.playableUnitSelected();
        _rank.setVisible(playableUnit);
        _numTimeUnits.setVisible(playableUnit);
        _barTimeUnits.setVisible(playableUnit);
        _barTimeUnits.setVisible(playableUnit);
        _numEnergy.setVisible(playableUnit);
        _barEnergy.setVisible(playableUnit);
        _barEnergy.setVisible(playableUnit);
        _numHealth.setVisible(playableUnit);
        _barHealth.setVisible(playableUnit);
        _barHealth.setVisible(playableUnit);
        _numMorale.setVisible(playableUnit);
        _barMorale.setVisible(playableUnit);
        _barMorale.setVisible(playableUnit);
        _btnLeftHandItem.setVisible(playableUnit);
        _btnRightHandItem.setVisible(playableUnit);
        _numAmmoLeft.setVisible(playableUnit);
        _numAmmoRight.setVisible(playableUnit);
        if (!playableUnit)
        {
            _txtName.setText(string.Empty);
            showPsiButton(false);
            toggleKneelButton(null);
            return;
        }

        _txtName.setText(battleUnit.getName(_game.getLanguage(), false));
        Soldier soldier = battleUnit.getGeoscapeSoldier();
        if (soldier != null)
        {
            SurfaceSet texture = _game.getMod().getSurfaceSet("SMOKE.PCK");
            texture.getFrame(20 + (int)soldier.getRank()).blit(_rank);
        }
        else
        {
            _rank.clear();
        }
        _numTimeUnits.setValue((uint)battleUnit.getTimeUnits());
        _barTimeUnits.setMax(battleUnit.getBaseStats().tu);
        _barTimeUnits.setValue(battleUnit.getTimeUnits());
        _numEnergy.setValue((uint)battleUnit.getEnergy());
        _barEnergy.setMax(battleUnit.getBaseStats().stamina);
        _barEnergy.setValue(battleUnit.getEnergy());
        _numHealth.setValue((uint)battleUnit.getHealth());
        _barHealth.setMax(battleUnit.getBaseStats().health);
        _barHealth.setValue(battleUnit.getHealth());
        _barHealth.setValue2(battleUnit.getStunlevel());
        _numMorale.setValue((uint)battleUnit.getMorale());
        _barMorale.setMax(100);
        _barMorale.setValue(battleUnit.getMorale());

        toggleKneelButton(battleUnit);

        BattleItem leftHandItem = getLeftHandItem(battleUnit);
        _btnLeftHandItem.clear();
        _numAmmoLeft.setVisible(false);
        if (leftHandItem != null)
        {
            leftHandItem.getRules().drawHandSprite(_game.getMod().getSurfaceSet("BIGOBS.PCK"), _btnLeftHandItem);
            if (leftHandItem.getRules().getBattleType() == BattleType.BT_FIREARM && (leftHandItem.needsAmmo() || leftHandItem.getRules().getClipSize() > 0))
            {
                _numAmmoLeft.setVisible(true);
                if (leftHandItem.getAmmoItem() != null)
                    _numAmmoLeft.setValue((uint)leftHandItem.getAmmoItem().getAmmoQuantity());
                else
                    _numAmmoLeft.setValue(0);
            }
        }
        BattleItem rightHandItem = getRightHandItem(battleUnit);
        _btnRightHandItem.clear();
        _numAmmoRight.setVisible(false);
        if (rightHandItem != null)
        {
            rightHandItem.getRules().drawHandSprite(_game.getMod().getSurfaceSet("BIGOBS.PCK"), _btnRightHandItem);
            if (rightHandItem.getRules().getBattleType() == BattleType.BT_FIREARM && (rightHandItem.needsAmmo() || rightHandItem.getRules().getClipSize() > 0))
            {
                _numAmmoRight.setVisible(true);
                if (rightHandItem.getAmmoItem() != null)
                    _numAmmoRight.setValue((uint)rightHandItem.getAmmoItem().getAmmoQuantity());
                else
                    _numAmmoRight.setValue(0);
            }
        }

        if (checkFOV)
        {
            _save.getTileEngine().calculateFOV(_save.getSelectedUnit());
        }
        int j = 0;
        var visibleUnits = battleUnit.getVisibleUnits();
        for (var i = 0; i < visibleUnits.Count && j < VISIBLE_MAX; ++i)
        {
            _btnVisibleUnit[j].setVisible(true);
            _numVisibleUnit[j].setVisible(true);
            _visibleUnit[j] = visibleUnits[i];
            ++j;
        }

        showPsiButton(battleUnit.getSpecialWeapon(BattleType.BT_PSIAMP) != null);
    }

    void toggleKneelButton(BattleUnit unit)
    {
        if (_btnKneel.isTFTDMode())
        {
            _btnKneel.toggle(unit != null && unit.isKneeled());
        }
        else
        {
            _game.getMod().getSurfaceSet("KneelButton").getFrame((unit != null && unit.isKneeled()) ? 1 : 0).blit(_btnKneel);
        }
    }

    /**
     * Gets the item currently accessible through the left hand slot in the battlescape UI.
     */
    BattleItem getLeftHandItem(BattleUnit battleUnit)
    {
        BattleItem melee = getSpecialMeleeWeapon(battleUnit);
        BattleItem leftHand = battleUnit.getItem("STR_LEFT_HAND");

        // If the unit has a melee weapon, and the right hand is already occupied,
        // allow access to the melee weapon through the left hand slot,
        // provided that the left hand is empty.
        return melee != null && battleUnit.getItem("STR_RIGHT_HAND") != null && leftHand == null
            ? melee
            : leftHand;
    }

    /**
     * Gets the item currently accessible through the right hand slot in the battlescape UI.
     */
    BattleItem getRightHandItem(BattleUnit battleUnit)
    {
        BattleItem melee = getSpecialMeleeWeapon(battleUnit);
        BattleItem rightHand = battleUnit.getItem("STR_RIGHT_HAND");

        // If the unit has a built-in melee weapon, and the right hand is not occupied,
        // allow access to the melee weapon through the right hand slot.
        return melee != null && rightHand == null ? melee : rightHand;
    }

    /**
     * Gets the built-in melee weapon of a unit, if any.
     */
    BattleItem getSpecialMeleeWeapon(BattleUnit battleUnit) =>
        battleUnit.getSpecialWeapon(BattleType.BT_MELEE);

    /**
     * Clears mouse-scrolling state (isMouseScrolling).
     */
    internal void clearMouseScrollingState() =>
        _isMouseScrolling = false;

    /**
     * Centers on the currently selected soldier.
     * @param action Pointer to an action.
     */
    internal void btnCenterClick(Action _)
    {
        if (playableUnitSelected())
        {
            _map.getCamera().centerOnPosition(_save.getSelectedUnit().getPosition());
            _map.refreshSelectorPosition();
        }
    }

    /**
     * Autosave the game the next time the battlescape is displayed.
     */
    internal void autosave() =>
        _autosave = true;

    /**
     * Determines whether a playable unit is selected. Normally only player side units can be selected, but in debug mode one can play with aliens too :)
     * Is used to see if action buttons will work.
     * @return Whether a playable unit is selected.
     */
    bool playableUnitSelected() =>
        _save.getSelectedUnit() != null && allowButtons();

    /**
     * Determines whether the player is allowed to press buttons.
     * Buttons are disabled in the middle of a shot, during the alien turn,
     * and while a player's units are panicking.
     * The save button is an exception as we want to still be able to save if something
     * goes wrong during the alien turn, and submit the save file for dissection.
     * @param allowSaving True, if the help button was clicked.
     * @return True if the player can still press buttons.
     */
    bool allowButtons(bool allowSaving = false)
    {
	    return ((allowSaving || _save.getSide() == UnitFaction.FACTION_PLAYER || _save.getDebugMode())
		    && (_battleGame.getPanicHandled() || _firstInit )
		    && (allowSaving || !_battleGame.isBusy() || _firstInit)
		    && (_map.getProjectile() == null));
    }

    /**
     * Selects the previous soldier.
     * @param checkReselect When true, don't select a unit that has been previously flagged.
     * @param setReselect When true, flag the current unit first.
     * @param checkInventory When true, don't select a unit that has no inventory.
     */
    internal void selectPreviousPlayerUnit(bool checkReselect = false, bool setReselect = false, bool checkInventory = false)
    {
        if (allowButtons())
        {
            BattleUnit unit = _save.selectPreviousPlayerUnit(checkReselect, setReselect, checkInventory);
            updateSoldierInfo();
            if (unit != null) _map.getCamera().centerOnPosition(unit.getPosition());
            _battleGame.cancelAllActions();
            _battleGame.getCurrentAction().actor = unit;
            _battleGame.setupCursor();
        }
    }

    /**
     * Selects the next soldier.
     * @param checkReselect When true, don't select a unit that has been previously flagged.
     * @param setReselect When true, flag the current unit first.
     * @param checkInventory When true, don't select a unit that has no inventory.
     */
    internal void selectNextPlayerUnit(bool checkReselect = false, bool setReselect = false, bool checkInventory = false, bool checkFOV = true)
    {
        if (allowButtons())
        {
            BattleUnit unit = _save.selectNextPlayerUnit(checkReselect, setReselect, checkInventory);
            updateSoldierInfo(checkFOV);
            if (unit != null) _map.getCamera().centerOnPosition(unit.getPosition());
            _battleGame.cancelAllActions();
            _battleGame.getCurrentAction().actor = unit;
            _battleGame.setupCursor();
        }
    }

    /**
     * Initializes the battlescapestate.
     */
    internal override void init()
    {
	    if (_save.getAmbientSound() != -1)
	    {
		    _game.getMod().getSoundByDepth((uint)_save.getDepth(), (uint)_save.getAmbientSound()).loop();
		    _game.setVolume(Options.soundVolume, Options.musicVolume, Options.uiVolume);
	    }

	    base.init();
	    _animTimer.start();
	    _gameTimer.start();
	    _map.setFocus(true);
	    _map.cacheUnits();
	    _map.draw();
	    _battleGame.init();
	    updateSoldierInfo();

	    switch (_save.getTUReserved())
	    {
	        case BattleActionType.BA_SNAPSHOT:
		        _reserve = _btnReserveSnap;
		        break;
	        case BattleActionType.BA_AIMEDSHOT:
		        _reserve = _btnReserveAimed;
		        break;
	        case BattleActionType.BA_AUTOSHOT:
		        _reserve = _btnReserveAuto;
		        break;
	        default:
		        _reserve = _btnReserveNone;
		        break;
	    }
	    if (_firstInit)
	    {
		    if (!playableUnitSelected())
		    {
			    selectNextPlayerUnit();
		    }
		    if (playableUnitSelected())
		    {
			    _battleGame.setupCursor();
			    _map.getCamera().centerOnPosition(_save.getSelectedUnit().getPosition());
		    }
		    _firstInit = false;
		    _btnReserveNone.setGroup(_reserve);
		    _btnReserveSnap.setGroup(_reserve);
		    _btnReserveAimed.setGroup(_reserve);
		    _btnReserveAuto.setGroup(_reserve);
	    }
	    _txtTooltip.setText(string.Empty);
	    _btnReserveKneel.toggle(_save.getKneelReserved());
	    _battleGame.setKneelReserved(_save.getKneelReserved());
	    if (_autosave)
	    {
		    _autosave = false;
		    if (_game.getSavedGame().isIronman())
		    {
			    _game.pushState(new SaveGameState(OptionsOrigin.OPT_BATTLESCAPE, SaveType.SAVE_IRONMAN, _palette));
		    }
		    else if (Options.autosave)
		    {
			    _game.pushState(new SaveGameState(OptionsOrigin.OPT_BATTLESCAPE, SaveType.SAVE_AUTO_BATTLESCAPE, _palette));
		    }
	    }
    }

	static bool popped = false;
    /**
     * Runs the timers and handles popups.
     */
    internal override void think()
    {
	    if (_gameTimer.isRunning())
	    {
		    if (!_popups.Any())
		    {
			    base.think();
			    _battleGame.think();
			    _animTimer.think(this, null);
			    _gameTimer.think(this, null);
			    if (popped)
			    {
				    _battleGame.handleNonTargetAction();
				    popped = false;
			    }
		    }
		    else
		    {
			    // Handle popups
			    _game.pushState(_popups.First());
			    _popups.Remove(_popups.First());
			    popped = true;
			    return;
		    }
	    }
    }

    /**
     * Shows a debug message in the topleft corner.
     * @param message Debug message.
     */
    internal void debug(string message)
    {
	    if (_save.getDebugMode())
	    {
		    _txtDebug.setText(message);
	    }
    }

    /**
     * Takes care of any events from the core game engine.
     * @param action Pointer to an action.
     */
    internal override void handle(Action action)
    {
	    if (!_firstInit)
	    {
		    if (_game.getCursor().getVisible() || ((action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN || action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONUP) && action.getDetails().button.button == SDL_BUTTON_RIGHT))
		    {
			    base.handle(action);

			    if (Options.touchEnabled == false && _isMouseScrolling && !Options.battleDragScrollInvert)
			    {
				    _map.setSelectorPosition((int)((_cursorPosition.x - _game.getScreen().getCursorLeftBlackBand()) / action.getXScale()), (int)((_cursorPosition.y - _game.getScreen().getCursorTopBlackBand()) / action.getYScale()));
			    }

			    if (action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
			    {
				    if (action.getDetails().button.button == SDL_BUTTON_X1)
				    {
					    btnNextSoldierClick(action);
				    }
				    else if (action.getDetails().button.button == SDL_BUTTON_X2)
				    {
					    btnPrevSoldierClick(action);
				    }
			    }

			    if (action.getDetails().type == SDL_EventType.SDL_KEYDOWN)
			    {
				    if (Options.debug)
				    {
					    // "ctrl-d" - enable debug mode
					    if (action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_d && (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0)
					    {
						    _save.setDebugMode();
						    debug("Debug Mode");
					    }
					    // "ctrl-v" - reset tile visibility
					    else if (_save.getDebugMode() && action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_v && (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0)
					    {
						    debug("Resetting tile visibility");
						    _save.resetTiles();
					    }
					    // "ctrl-k" - kill all aliens
					    else if (_save.getDebugMode() && action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_k && (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0)
					    {
						    debug("Influenza bacterium dispersed");
						    foreach (var i in _save.getUnits())
						    {
							    if (i.getOriginalFaction() == UnitFaction.FACTION_HOSTILE && !i.isOut())
							    {
								    i.damage(new Position(0,0,0), 1000, ItemDamageType.DT_AP, true);
							    }
							    _save.getBattleGame().checkForCasualties(null, null, true, false);
							    _save.getBattleGame().handleState();
						    }
					    }
					    // "ctrl-j" - stun all aliens
					    else if (_save.getDebugMode() && action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_j && (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0)
					    {
						    debug("Deploying Celine Dion album");
						    foreach (var i in _save.getUnits())
						    {
							    if (i.getOriginalFaction() == UnitFaction.FACTION_HOSTILE && !i.isOut())
							    {
								    i.damage(new Position(0,0,0), 1000, ItemDamageType.DT_STUN, true);
							    }
						    }
						    _save.getBattleGame().checkForCasualties(null, null, true, false);
						    _save.getBattleGame().handleState();
					    }
					    // "ctrl-w" - warp unit
					    else if (_save.getDebugMode() && action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_w && (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0)
					    {
						    debug("Beam me up Scotty");
						    BattleUnit unit = _save.getSelectedUnit();
                            _map.getSelectorPosition(out var newPos);
                            if (unit != null && newPos.x >= 0)
						    {
							    unit.getTile().setUnit(null);
							    unit.setPosition(newPos);
							    _save.getTile(newPos).setUnit(unit);
							    _save.getTileEngine().calculateUnitLighting();
							    _save.getBattleGame().handleState();
						    }
					    }
					    // f11 - voxel map dump
					    else if (action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_F11)
					    {
						    saveVoxelMap();
					    }
					    // f9 - ai
					    else if (action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_F9 && Options.traceAI)
					    {
						    saveAIMap();
					    }
				    }
				    // quick save and quick load
				    if (!_game.getSavedGame().isIronman())
				    {
					    if (action.getDetails().key.keysym.sym == Options.keyQuickSave)
					    {
						    _game.pushState(new SaveGameState(OptionsOrigin.OPT_BATTLESCAPE, SaveType.SAVE_QUICK, _palette));
					    }
					    else if (action.getDetails().key.keysym.sym == Options.keyQuickLoad)
					    {
						    _game.pushState(new LoadGameState(OptionsOrigin.OPT_BATTLESCAPE, SaveType.SAVE_QUICK, _palette));
					    }
				    }

				    // voxel view dump
				    if (action.getDetails().key.keysym.sym == Options.keyBattleVoxelView)
				    {
					    saveVoxelView();
				    }
			    }
		    }
	    }
    }

    /**
     * Selects the next soldier.
     * @param action Pointer to an action.
     */
    void btnNextSoldierClick(Action _)
    {
	    if (allowButtons())
	    {
		    selectNextPlayerUnit(true, false);
		    _map.refreshSelectorPosition();
	    }
    }

    /**
     * Selects next soldier.
     * @param action Pointer to an action.
     */
    void btnPrevSoldierClick(Action _)
    {
	    if (allowButtons())
	    {
		    selectPreviousPlayerUnit(true);
		    _map.refreshSelectorPosition();
	    }
    }

	static byte[] pal =
	    { 255,255,255, 224,224,224, 128,160,255, 255,160,128, 128,255,128, 192,0,255, 255,255,255, 255,255,255, 224,192,0, 255,64,128 };
    /**
     * Saves each layer of voxels on the bettlescape as a png.
     */
    void saveVoxelMap()
    {
	    string ss;
	    var image = new byte[_save.getMapSizeX()*16 * _save.getMapSizeY()*16];
		var ptr = 0;

	    Tile tile;

	    for (int z = 0; z < _save.getMapSizeZ()*12; ++z)
	    {
		    Array.Clear(image);

		    for (int y = 0; y < _save.getMapSizeY()*16; ++y)
		    {
			    for (int x = 0; x < _save.getMapSizeX()*16; ++x)
			    {
				    int test = (int)_save.getTileEngine().voxelCheck(new Position(x,y,z*2),null,false) +1;
				    float dist=1;
				    if (x%16==15)
				    {
					    dist*=0.9f;
				    }
				    if (y%16==15)
				    {
					    dist*=0.9f;
				    }

				    if (test == (int)VoxelType.V_OUTOFBOUNDS)
				    {
					    tile = _save.getTile(new Position(x/16, y/16, z/12));
					    if (tile.getUnit() != null)
					    {
						    if (tile.getUnit().getFaction()==UnitFaction.FACTION_NEUTRAL) test=9;
						    else
						    if (tile.getUnit().getFaction()==UnitFaction.FACTION_PLAYER) test=8;
					    }
					    else
					    {
						    tile = _save.getTile(new Position(x/16, y/16, z/12-1));
						    if (tile != null && tile.getUnit() != null)
						    {
							    if (tile.getUnit().getFaction()==UnitFaction.FACTION_NEUTRAL) test=9;
							    else
							    if (tile.getUnit().getFaction()==UnitFaction.FACTION_PLAYER) test=8;
						    }
					    }
				    }

					image[ptr++] = (byte)((float)pal[test*3+0]*dist);
					image[ptr++] = (byte)((float)pal[test*3+1]*dist);
					image[ptr++] = (byte)((float)pal[test*3+2]*dist);
			    }
		    }

		    ss = $"{Options.getMasterUserFolder()}voxel{z:D2}.png";

			var surface = Marshal.AllocHGlobal(image.Length);
			Marshal.Copy(image, 0, surface, image.Length);
			int error = IMG_SavePNG(surface, ss);
		    if (error != 0)
		    {
                Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Saving to PNG failed: {IMG_GetError()}");
		    }
			Marshal.FreeHGlobal(surface);
	    }
	    return;
    }

    /**
     * Saves a map as used by the AI.
     */
    unsafe void saveAIMap()
    {
	    uint start = SDL_GetTicks();
	    BattleUnit unit = _save.getSelectedUnit();
	    if (unit == null) return;

	    int w = _save.getMapSizeX();
	    int h = _save.getMapSizeY();

	    nint imgPtr = SDL_CreateRGBSurface(0, w * 8, h * 8, 24, 0xff, 0xff00, 0xff0000, 0); //SDL_AllocSurface
        SDL_Surface img = Marshal.PtrToStructure<SDL_Surface>(imgPtr);
        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} unit = {unit.getId()}");
        var span = new Span<byte>((byte*)img.pixels, img.pitch * img.h);
        span.Fill(0); //memset(img->pixels, 0, img->pitch * img->h);

	    Position tilePos = unit.getPosition();
	    SDL_Rect r;
	    r.h = 8;
	    r.w = 8;

	    for (int y = 0; y < h; ++y)
	    {
		    tilePos.y = y;
		    for (int x = 0; x < w; ++x)
		    {
			    tilePos.x = x;
			    Tile t = _save.getTile(tilePos);

			    if (t == null) continue;
			    if (!t.isDiscovered(2)) continue;
		    }
	    }

	    for (int y = 0; y < h; ++y)
	    {
		    tilePos.y = y;
		    for (int x = 0; x < w; ++x)
		    {
			    tilePos.x = x;
			    Tile t = _save.getTile(tilePos);

			    if (t == null) continue;
			    if (!t.isDiscovered(2)) continue;

			    r.x = x * r.w;
			    r.y = y * r.h;

			    if (t.getTUCost((int)TilePart.O_FLOOR, MovementType.MT_FLY) != 255 && t.getTUCost((int)TilePart.O_OBJECT, MovementType.MT_FLY) != 255)
			    {
				    SDL_FillRect(img.pixels, ref r, SDL_MapRGB(img.format, 255, 0, 0x20));
				    characterRGBA(img.pixels, (short)r.x, (short)r.y, (sbyte)'*', 0x7f, 0x7f, 0x7f, 0x7f);
			    } else
			    {
				    if (t.getUnit() == null) SDL_FillRect(img.pixels, ref r, SDL_MapRGB(img.format, 0x50, 0x50, 0x50)); // gray for blocked tile
			    }

			    for (int z = tilePos.z; z >= 0; --z)
			    {
				    Position pos = new Position(tilePos.x, tilePos.y, z);
				    t = _save.getTile(pos);
				    BattleUnit wat = t.getUnit();
				    if (wat != null)
				    {
					    switch(wat.getFaction())
					    {
					        case UnitFaction.FACTION_HOSTILE:
						        // #4080C0 is Volutar Blue
						        characterRGBA(img.pixels, (short)r.x, (short)r.y, (sbyte)((tilePos.z - z != 0) ? 'a' : 'A'), 0x40, 0x80, 0xC0, 0xff);
						        break;
					        case UnitFaction.FACTION_PLAYER:
						        characterRGBA(img.pixels, (short)r.x, (short)r.y, (sbyte)((tilePos.z - z != 0) ? 'x' : 'X'), 255, 255, 127, 0xff);
						        break;
					        case UnitFaction.FACTION_NEUTRAL:
						        characterRGBA(img.pixels, (short)r.x, (short)r.y, (sbyte)((tilePos.z - z != 0) ? 'c' : 'C'), 255, 127, 127, 0xff);
						        break;
					    }
					    break;
				    }
				    pos.z--;
				    if (z > 0 && !t.hasNoFloor(_save.getTile(pos))) break; // no seeing through floors
			    }

			    if (t.getMapData(TilePart.O_NORTHWALL) != null && t.getMapData(TilePart.O_NORTHWALL).getTUCost(MovementType.MT_FLY) == 255)
			    {
				    lineRGBA(img.pixels, (short)r.x, (short)r.y, (short)(r.x+r.w), (short)r.y, 0x50, 0x50, 0x50, 255);
			    }

			    if (t.getMapData(TilePart.O_WESTWALL) != null && t.getMapData(TilePart.O_WESTWALL).getTUCost(MovementType.MT_FLY) == 255)
			    {
				    lineRGBA(img.pixels, (short)r.x, (short)r.y, (short)r.x, (short)(r.y+r.h), 0x50, 0x50, 0x50, 255);
			    }
		    }
	    }

	    string ss;

	    ss = $"z = {tilePos.z}";
	    stringRGBA(img.pixels, 12, 12, ss, 0, 0, 0, 0x7f);

	    int i = 0;
	    do
	    {
		    ss = $"{Options.getMasterUserFolder()}AIExposure{i:D3}.png";
		    i++;
	    }
	    while (CrossPlatform.fileExists(ss));

		//unsigned error = lodepng::encode(ss.str(), (const unsigned char*)img->pixels, img->w, img->h, LCT_RGB);
        var error = IMG_SavePNG(img.pixels, ss);
        if (error == -1)
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Saving to PNG failed: {SDL_GetError()}");
	    }

	    SDL_FreeSurface(img.pixels);

        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} saveAIMap() completed in {SDL_GetTicks() - start}ms.");
    }

	static byte[] pal2 =
		//			ground		west wall	north wall		object		enem unit						xcom unit	neutr unit
		{ 0,0,0, 224,224,224,  192,224,255,  255,224,192, 128,255,128, 192,0,255,  0,0,0, 255,255,255,  224,192,0,  255,64,128 };
	/**
	 * Saves a first-person voxel view of the battlescape.
	 */
	void saveVoxelView()
	{
		BattleUnit bu = _save.getSelectedUnit();
		if (bu==null) return; //no unit selected
		var _trajectory = new List<Position>();

		double ang_x,ang_y;
		bool black;
		Tile tile = null;
		string ss;
		var image = new byte[512 * 512];
		var ptr = 0;
		int test;
		Position originVoxel = getBattleGame().getTileEngine().getSightOriginVoxel(bu);

		Position targetVoxel = new(),hitPos = new();
		double dist = 0;
		bool _debug = _save.getDebugMode();
		double dir = ((double)bu.getDirection()+4)/4*M_PI;
		//Array.Clear(image);
		for (int y = -256+32; y < 256+32; ++y)
		{
			ang_y = (((double)y)/640*M_PI+M_PI/2);
			for (int x = -256; x < 256; ++x)
			{
				ang_x = ((double)x/1024)*M_PI+dir;

				targetVoxel.x=originVoxel.x + (int)(-Math.Sin(ang_x)*1024*Math.Sin(ang_y));
				targetVoxel.y=originVoxel.y + (int)(Math.Cos(ang_x)*1024*Math.Sin(ang_y));
				targetVoxel.z=originVoxel.z + (int)(Math.Cos(ang_y)*1024);

				_trajectory.Clear();
				test = _save.getTileEngine().calculateLine(originVoxel, targetVoxel, false, _trajectory, bu, true, !_debug) +1;
				black = true;
				if (test!=0 && test!=6)
				{
					tile = _save.getTile(new Position(_trajectory[0].x/16, _trajectory[0].y/16, _trajectory[0].z/24));
					if (_debug
						|| (tile.isDiscovered(0) && test == 2)
						|| (tile.isDiscovered(1) && test == 3)
						|| (tile.isDiscovered(2) && (test == 1 || test == 4))
						|| test==5
						)
					{
						if (test==5)
						{
							if (tile.getUnit() != null)
							{
								if (tile.getUnit().getFaction()==UnitFaction.FACTION_NEUTRAL) test=9;
								else
								if (tile.getUnit().getFaction()==UnitFaction.FACTION_PLAYER) test=8;
							}
							else
							{
								tile = _save.getTile(new Position(_trajectory[0].x/16, _trajectory[0].y/16, _trajectory[0].z/24-1));
								if (tile != null && tile.getUnit() != null)
								{
									if (tile.getUnit().getFaction()==UnitFaction.FACTION_NEUTRAL) test=9;
									else
									if (tile.getUnit().getFaction()==UnitFaction.FACTION_PLAYER) test=8;
								}
							}
						}
						hitPos = new Position(_trajectory[0].x, _trajectory[0].y, _trajectory[0].z);
						dist = Math.Sqrt((double)((hitPos.x-originVoxel.x)*(hitPos.x-originVoxel.x)
							+ (hitPos.y-originVoxel.y)*(hitPos.y-originVoxel.y)
							+ (hitPos.z-originVoxel.z)*(hitPos.z-originVoxel.z)) );
						black = false;
					}
				}

				if (black)
				{
					dist = 0;
				}
				else
				{
					if (dist>1000) dist=1000;
					if (dist<1) dist=1;
					dist=(1000-(Math.Log(dist))*140)/700;//140

					if (hitPos.x%16==15)
					{
						dist*=0.9;
					}
					if (hitPos.y%16==15)
					{
						dist*=0.9;
					}
					if (hitPos.z%24==23)
					{
						dist*=0.9;
					}
					if (dist > 1) dist = 1;
					if (tile != null) dist *= (16 - (double)tile.getShade())/16;
				}

				image[ptr++] = (byte)((double)(pal2[test*3+0])*dist);
				image[ptr++] = (byte)((double)(pal2[test*3+1])*dist);
				image[ptr++] = (byte)((double)(pal2[test*3+2])*dist);
			}
		}

		int i = 0;
		do
		{
			ss = $"{Options.getMasterUserFolder()}fpslook{i:D3}.png";
			i++;
		}
		while (CrossPlatform.fileExists(ss));

		var surface = Marshal.AllocHGlobal(image.Length);
		Marshal.Copy(image, 0, surface, image.Length);
		int error = IMG_SavePNG(surface, ss);
		if (error != 0)
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Saving to PNG failed: {IMG_GetError()}");
		}
		Marshal.FreeHGlobal(surface);

		return;
	}

	/**
	 * Updates the scale.
	 * @param dX delta of X;
	 * @param dY delta of Y;
	 */
	internal override void resize(ref int dX, ref int dY)
	{
		dX = Options.baseXResolution;
		dY = Options.baseYResolution;
		int divisor = 1;
		double pixelRatioY = 1.0;

		if (Options.nonSquarePixelRatio)
		{
			pixelRatioY = 1.2;
		}
		switch ((ScaleType)Options.battlescapeScale)
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
		_map.setWidth(Options.baseXResolution);
		_map.setHeight(Options.baseYResolution);
		_map.getCamera().resize();
		_map.getCamera().jumpXY(dX/2, dY/2);

		foreach (var i in _surfaces)
		{
			if (i != _map && i != _btnPsi && i != _btnLaunch && i != _txtDebug)
			{
				i.setX(i.getX() + dX / 2);
				i.setY(i.getY() + dY);
			}
			else if (i != _map && i != _txtDebug)
			{
				i.setX(i.getX() + dX);
			}
		}
	}

	/**
	 * Processes any mouse moving over the map.
	 * @param action Pointer to an action.
	 */
	void mapOver(Action action)
	{
		if (_isMouseScrolling && action.getDetails().type == SDL_EventType.SDL_MOUSEMOTION)
		{
			// The following is the workaround for a rare problem where sometimes
			// the mouse-release event is missed for any reason.
			// (checking: is the dragScroll-mouse-button still pressed?)
			// However if the SDL is also missed the release event, then it is to no avail :(
			if ((SDL_GetMouseState(0,0) & SDL_BUTTON((uint)Options.battleDragScrollButton)) == 0)
			{ // so we missed again the mouse-release :(
				// Check if we have to revoke the scrolling, because it was too short in time, so it was a click
				if ((!_mouseMovedOverThreshold) && ((int)(SDL_GetTicks() - _mouseScrollingStartTime) <= (Options.dragScrollTimeTolerance)))
				{
					_map.getCamera().setMapOffset(_mapOffsetBeforeMouseScrolling);
				}
				_isMouseScrolled = _isMouseScrolling = false;
				stopScrolling(action);
				return;
			}

			_isMouseScrolled = true;

			if (Options.touchEnabled == false)
			{
				// Set the mouse cursor back
				SDL_EventState(SDL_EventType.SDL_MOUSEMOTION, SDL_IGNORE);
				SDL_WarpMouseGlobal(_game.getScreen().getWidth() / 2, _game.getScreen().getHeight() / 2 - _map.getIconHeight() / 2);
				SDL_EventState(SDL_EventType.SDL_MOUSEMOTION, SDL_ENABLE);
			}

			// Check the threshold
			_totalMouseMoveX += action.getDetails().motion.xrel;
			_totalMouseMoveY += action.getDetails().motion.yrel;
			if (!_mouseMovedOverThreshold)
			{
				_mouseMovedOverThreshold = ((Math.Abs(_totalMouseMoveX) > Options.dragScrollPixelTolerance) || (Math.Abs(_totalMouseMoveY) > Options.dragScrollPixelTolerance));
			}

			// Scrolling
			if (Options.battleDragScrollInvert)
			{
				_map.getCamera().setMapOffset(_mapOffsetBeforeMouseScrolling);
				int scrollX = -(int)((double)_totalMouseMoveX / action.getXScale());
				int scrollY = -(int)((double)_totalMouseMoveY / action.getYScale());
				Position delta2 = _map.getCamera().getMapOffset();
				_map.getCamera().scrollXY(scrollX, scrollY, true);
				delta2 = _map.getCamera().getMapOffset() - delta2;

				// Keep the limits...
				if (scrollX != delta2.x || scrollY != delta2.y)
				{
					_totalMouseMoveX = -(int) (delta2.x * action.getXScale());
					_totalMouseMoveY = -(int) (delta2.y * action.getYScale());
				}

				if (Options.touchEnabled == false)
				{
					action.getDetails().motion.x = _xBeforeMouseScrolling;
					action.getDetails().motion.y = _yBeforeMouseScrolling;
				}
				_map.setCursorType(CursorType.CT_NONE);
			}
			else
			{
				Position delta = _map.getCamera().getMapOffset();
				_map.getCamera().setMapOffset(_mapOffsetBeforeMouseScrolling);
				int scrollX = (int)((double)_totalMouseMoveX / action.getXScale());
				int scrollY = (int)((double)_totalMouseMoveY / action.getYScale());
				Position delta2 = _map.getCamera().getMapOffset();
				_map.getCamera().scrollXY(scrollX, scrollY, true);
				delta2 = _map.getCamera().getMapOffset() - delta2;
				delta = _map.getCamera().getMapOffset() - delta;

				// Keep the limits...
				if (scrollX != delta2.x || scrollY != delta2.y)
				{
					_totalMouseMoveX = (int) (delta2.x * action.getXScale());
					_totalMouseMoveY = (int) (delta2.y * action.getYScale());
				}

				int barWidth = _game.getScreen().getCursorLeftBlackBand();
				int barHeight = _game.getScreen().getCursorTopBlackBand();
				int cursorX = (int)(_cursorPosition.x + Round(delta.x * action.getXScale()));
				int cursorY = (int)(_cursorPosition.y + Round(delta.y * action.getYScale()));
				_cursorPosition.x = Math.Clamp(cursorX, barWidth, _game.getScreen().getWidth() - barWidth - (int)(Round(action.getXScale())));
				_cursorPosition.y = Math.Clamp(cursorY, barHeight, _game.getScreen().getHeight() - barHeight - (int)(Round(action.getYScale())));

				if (Options.touchEnabled == false)
				{
					action.getDetails().motion.x = _cursorPosition.x;
					action.getDetails().motion.y = _cursorPosition.y;
				}
			}

			// We don't want to look the mouse-cursor jumping :)
			_game.getCursor().handle(action);
		}
	}

	/**
	 * Move the mouse back to where it started after we finish drag scrolling.
	 * @param action Pointer to an action.
	 */
	void stopScrolling(Action action)
	{
		if (Options.battleDragScrollInvert)
		{
			SDL_WarpMouseGlobal(_xBeforeMouseScrolling, _yBeforeMouseScrolling);
			action.setMouseAction(_xBeforeMouseScrolling, _yBeforeMouseScrolling, _map.getX(), _map.getY());
			_battleGame.setupCursor();
			if (_battleGame.getCurrentAction().actor == null && (_save.getSide() == UnitFaction.FACTION_PLAYER || _save.getDebugMode()))
			{
				getMap().setCursorType(CursorType.CT_NORMAL);
			}
		}
		else
		{
			SDL_WarpMouseGlobal(_cursorPosition.x, _cursorPosition.y);
			action.setMouseAction(_cursorPosition.x, _cursorPosition.y, _map.getX(), _map.getY());
			_map.setSelectorPosition((int)action.getAbsoluteXMouse(), (int)action.getAbsoluteYMouse());
		}
		// reset our "mouse position stored" flag
		_cursorPosition.z = 0;
	}

	/**
	 * Processes any presses on the map.
	 * @param action Pointer to an action.
	 */
	void mapPress(Action action)
	{
		// don't handle mouseclicks over the buttons (it overlaps with map surface)
		if (_mouseOverIcons) return;

		if (action.getDetails().button.button == Options.battleDragScrollButton)
		{
			_isMouseScrolling = true;
			_isMouseScrolled = false;
			SDL_GetMouseState(out _xBeforeMouseScrolling, out _yBeforeMouseScrolling);
			_mapOffsetBeforeMouseScrolling = _map.getCamera().getMapOffset();
			if (!Options.battleDragScrollInvert && _cursorPosition.z == 0)
			{
				_cursorPosition.x = action.getDetails().motion.x;
				_cursorPosition.y = action.getDetails().motion.y;
				// the Z is irrelevant to our mouse position, but we can use it as a boolean to check if the position is set or not
				_cursorPosition.z = 1;
			}
			_totalMouseMoveX = 0; _totalMouseMoveY = 0;
			_mouseMovedOverThreshold = false;
			_mouseScrollingStartTime = SDL_GetTicks();
		}
	}

	/**
	 * Processes any clicks on the map to
	 * command units.
	 * @param action Pointer to an action.
	 */
	void mapClick(Action action)
	{
		// The following is the workaround for a rare problem where sometimes
		// the mouse-release event is missed for any reason.
		// However if the SDL is also missed the release event, then it is to no avail :(
		// (this part handles the release if it is missed and now an other button is used)
		if (_isMouseScrolling)
		{
			if (action.getDetails().button.button != Options.battleDragScrollButton
			&& (SDL_GetMouseState(0,0) & SDL_BUTTON((uint)Options.battleDragScrollButton)) == 0)
			{   // so we missed again the mouse-release :(
				// Check if we have to revoke the scrolling, because it was too short in time, so it was a click
				if ((!_mouseMovedOverThreshold) && ((int)(SDL_GetTicks() - _mouseScrollingStartTime) <= (Options.dragScrollTimeTolerance)))
				{
					_map.getCamera().setMapOffset(_mapOffsetBeforeMouseScrolling);
				}
				_isMouseScrolled = _isMouseScrolling = false;
				stopScrolling(action);
			}
		}

		// DragScroll-Button release: release mouse-scroll-mode
		if (_isMouseScrolling)
		{
			// While scrolling, other buttons are ineffective
			if (action.getDetails().button.button == Options.battleDragScrollButton)
			{
				_isMouseScrolling = false;
				stopScrolling(action);
			}
			else
			{
				return;
			}
			// Check if we have to revoke the scrolling, because it was too short in time, so it was a click
			if ((!_mouseMovedOverThreshold) && ((int)(SDL_GetTicks() - _mouseScrollingStartTime) <= (Options.dragScrollTimeTolerance)))
			{
				_isMouseScrolled = false;
				stopScrolling(action);
			}
			if (_isMouseScrolled) return;
		}

		// right-click aborts walking state
		if (action.getDetails().button.button == SDL_BUTTON_RIGHT)
		{
			if (_battleGame.cancelCurrentAction())
			{
				return;
			}
		}

		// don't handle mouseclicks over the buttons (it overlaps with map surface)
		if (_mouseOverIcons) return;

		// don't accept leftclicks if there is no cursor or there is an action busy
		if (_map.getCursorType() == CursorType.CT_NONE || _battleGame.isBusy()) return;

		Position pos;
		_map.getSelectorPosition(out pos);

		if (_save.getDebugMode())
		{
			string ss = $"Clicked {pos}";
			debug(ss);
		}

		if (_save.getTile(pos) != null) // don't allow to click into void
		{
			if ((action.getDetails().button.button == SDL_BUTTON_RIGHT || (action.getDetails().button.button == SDL_BUTTON_LEFT && (SDL_GetModState() & SDL_Keymod.KMOD_ALT) != 0)) && playableUnitSelected())
			{
				_battleGame.secondaryAction(pos);
			}
			else if (action.getDetails().button.button == SDL_BUTTON_LEFT)
			{
				_battleGame.primaryAction(pos);
			}
		}
	}

	/**
	 * Handles mouse entering the map surface.
	 * @param action Pointer to an action.
	 */
	void mapIn(Action _)
	{
		_isMouseScrolling = false;
		_map.setButtonsPressed((byte)Options.battleDragScrollButton, false);
	}

	/**
	 * Handler for the mouse moving over the icons, disabling the tile selection cube.
	 * @param action Pointer to an action.
	 */
	void mouseInIcons(Action _) =>
		_mouseOverIcons = true;

	/**
	 * Handler for the mouse going out of the icons, enabling the tile selection cube.
	 * @param action Pointer to an action.
	 */
	void mouseOutIcons(Action _) =>
		_mouseOverIcons = false;

	/**
	 * Moves the selected unit up.
	 * @param action Pointer to an action.
	 */
	void btnUnitUpClick(Action _)
	{
		if (playableUnitSelected() && _save.getPathfinding().validateUpDown(_save.getSelectedUnit(), _save.getSelectedUnit().getPosition(), Pathfinding.DIR_UP))
		{
			_battleGame.cancelAllActions();
			_battleGame.moveUpDown(_save.getSelectedUnit(), Pathfinding.DIR_UP);
		}
	}

	/**
	 * Shows a tooltip for the appropriate button.
	 * @param action Pointer to an action.
	 */
	void txtTooltipIn(Action action)
	{
		if (allowButtons() && Options.battleTooltips)
		{
			_currentTooltip = action.getSender().getTooltip();
			_txtTooltip.setText(tr(_currentTooltip));
		}
	}

	/**
	 * Clears the tooltip text.
	 * @param action Pointer to an action.
	 */
	void txtTooltipOut(Action action)
	{
		if (allowButtons() && Options.battleTooltips)
		{
			if (_currentTooltip == action.getSender().getTooltip())
			{
				_txtTooltip.setText(string.Empty);
			}
		}
	}

	/**
	 * Moves the selected unit down.
	 * @param action Pointer to an action.
	 */
	void btnUnitDownClick(Action _)
	{
		if (playableUnitSelected() && _save.getPathfinding().validateUpDown(_save.getSelectedUnit(), _save.getSelectedUnit().getPosition(), Pathfinding.DIR_DOWN))
		{
			_battleGame.cancelAllActions();
			_battleGame.moveUpDown(_save.getSelectedUnit(), Pathfinding.DIR_DOWN);
		}
	}

	/**
	 * Shows the next map layer.
	 * @param action Pointer to an action.
	 */
	void btnMapUpClick(Action _)
	{
		if (_save.getSide() == UnitFaction.FACTION_PLAYER || _save.getDebugMode())
			_map.getCamera().up();
	}

	/**
	 * Shows the previous map layer.
	 * @param action Pointer to an action.
	 */
	void btnMapDownClick(Action _)
	{
		if (_save.getSide() == UnitFaction.FACTION_PLAYER || _save.getDebugMode())
			_map.getCamera().down();
	}

	/**
	 * Shows the minimap.
	 * @param action Pointer to an action.
	 */
	void btnShowMapClick(Action _)
	{
		//MiniMapState
		if (allowButtons())
			_game.pushState(new MiniMapState(_map.getCamera(), _save));
	}

	/**
	 * Toggles the current unit's kneel/standup status.
	 * @param action Pointer to an action.
	 */
	void btnKneelClick(Action _)
	{
		if (allowButtons())
		{
			BattleUnit bu = _save.getSelectedUnit();
			if (bu != null)
			{
				_battleGame.kneel(bu);
				toggleKneelButton(bu);

				// update any path preview when unit kneels
				if (_battleGame.getPathfinding().isPathPreviewed())
				{
					_battleGame.getPathfinding().calculate(_battleGame.getCurrentAction().actor, _battleGame.getCurrentAction().target);
					_battleGame.getPathfinding().removePreview();
					_battleGame.getPathfinding().previewPath();
				}
			}
		}
	}

	/**
	 * Goes to the soldier info screen.
	 * Additionally resets TUs for current side in debug mode.
	 * @param action Pointer to an action.
	 */
	void btnInventoryClick(Action _)
	{
		if (_save.getDebugMode())
		{
			foreach (var i in _save.getUnits())
				if (i.getFaction() == _save.getSide())
					i.prepareNewTurn();
			updateSoldierInfo();
		}
		if (playableUnitSelected()
			&& (_save.getSelectedUnit().hasInventory() || _save.getDebugMode()))
		{
			_battleGame.cancelAllActions();
			_game.pushState(new InventoryState(!_save.getDebugMode(), this));
		}
	}

	/**
	 * Disables reselection of the current soldier and selects the next soldier.
	 * @param action Pointer to an action.
	 */
	void btnNextStopClick(Action _)
	{
		if (allowButtons())
		{
			selectNextPlayerUnit(true, true);
			_map.refreshSelectorPosition();
		}
	}

	/**
	 * Shows/hides all map layers.
	 * @param action Pointer to an action.
	 */
	void btnShowLayersClick(Action _) =>
		_numLayers.setValue((uint)_map.getCamera().toggleShowAllLayers());

	/**
	 * Shows options.
	 * @param action Pointer to an action.
	 */
	void btnHelpClick(Action _)
	{
		if (allowButtons(true))
			_game.pushState(new PauseState(OptionsOrigin.OPT_BATTLESCAPE));
	}

	/**
	 * Requests the end of turn. This will add a 0 to the end of the state queue,
	 * so all ongoing actions, like explosions are finished first before really switching turn.
	 * @param action Pointer to an action.
	 */
	void btnEndTurnClick(Action _)
	{
		if (allowButtons())
		{
			_txtTooltip.setText(string.Empty);
			_battleGame.requestEndTurn();
		}
	}

	/**
	 * Aborts the game.
	 * @param action Pointer to an action.
	 */
	void btnAbortClick(Action _)
	{
		if (allowButtons())
			_game.pushState(new AbortMissionState(_save, this));
	}

	/**
	 * Shows the selected soldier's info.
	 * @param action Pointer to an action.
	 */
	void btnStatsClick(Action action)
	{
		if (playableUnitSelected())
		{
			bool scroll = false;
			if (ScrollType.SCROLL_TRIGGER == Options.battleEdgeScroll &&
				SDL_EventType.SDL_MOUSEBUTTONUP == action.getDetails().type && SDL_BUTTON_LEFT == action.getDetails().button.button)
			{
				int posX = action.getXMouse();
				int posY = action.getYMouse();
				if ((posX < (Camera.SCROLL_BORDER * action.getXScale()) && posX > 0)
					|| (posX > (_map.getWidth() - Camera.SCROLL_BORDER) * action.getXScale())
					|| (posY < (Camera.SCROLL_BORDER * action.getYScale()) && posY > 0)
					|| (posY > (_map.getHeight() - Camera.SCROLL_BORDER) * action.getYScale()))
					// To avoid handling this event as a click
					// on the stats button when the mouse is on the scroll-border
					scroll = true;
			}
			if (!scroll)
			{
				_battleGame.cancelAllActions();
				popup(new UnitInfoState(_save.getSelectedUnit(), this, false, false));
			}
		}
	}

	/**
	 * Adds a new popup window to the queue
	 * (this prevents popups from overlapping).
	 * @param state Pointer to popup state.
	 */
	void popup(State state) =>
		_popups.Add(state);

	/**
	 * Shows an action popup menu. When clicked, creates the action.
	 * @param action Pointer to an action.
	 */
	void btnLeftHandItemClick(Action _)
	{
		if (playableUnitSelected())
		{
			// concession for touch devices:
			// click on the item to cancel action, and don't pop up a menu to select a new one
			// TODO: wrap this in an IFDEF ?
			if (_battleGame.getCurrentAction().targeting)
			{
				_battleGame.cancelCurrentAction();
				return;
			}

			_battleGame.cancelCurrentAction();

			BattleUnit unit = _save.getSelectedUnit();
			BattleItem leftHandItem = getLeftHandItem(unit);

			if (leftHandItem != getSpecialMeleeWeapon(unit))
			{
				unit.setActiveHand("STR_LEFT_HAND");
			}

			_map.cacheUnits();
			_map.draw();
			handleItemClick(leftHandItem);
		}
	}

	/**
	 * Popups a context sensitive list of actions the user can choose from.
	 * Some actions result in a change of gamestate.
	 * @param item Item the user clicked on (righthand/lefthand)
	 */
	void handleItemClick(BattleItem item)
	{
		// make sure there is an item, and the battlescape is in an idle state
		if (item != null && !_battleGame.isBusy())
		{
			_battleGame.getCurrentAction().weapon = item;
			popup(new ActionMenuState(_battleGame.getCurrentAction(), _icons.getX(), _icons.getY()+16));
		}
	}

	/**
	 * Shows an action popup menu. When clicked, create the action.
	 * @param action Pointer to an action.
	 */
	void btnRightHandItemClick(Action _)
	{
		if (playableUnitSelected())
		{
			// concession for touch devices:
			// click on the item to cancel action, and don't pop up a menu to select a new one
			// TODO: wrap this in an IFDEF ?
			if (_battleGame.getCurrentAction().targeting)
			{
				_battleGame.cancelCurrentAction();
				return;
			}

			_battleGame.cancelCurrentAction();

			BattleUnit unit = _save.getSelectedUnit();
			BattleItem rightHandItem = getRightHandItem(unit);

			if (rightHandItem != getSpecialMeleeWeapon(unit))
			{
				unit.setActiveHand("STR_RIGHT_HAND");
			}

			_map.cacheUnits();
			_map.draw();
			handleItemClick(rightHandItem);
		}
	}

	/**
	 * Reserves time units.
	 * @param action Pointer to an action.
	 */
	void btnReserveClick(Action action)
	{
		if (allowButtons())
		{
			var ev = new SDL_Event();
			ev.type = SDL_EventType.SDL_MOUSEBUTTONDOWN;
			ev.button.button = (byte)SDL_BUTTON_LEFT;
			var a = new Action(ev, 0.0, 0.0, 0, 0);
			action.getSender().mousePress(a, this);

			if (_reserve == _btnReserveNone)
				_battleGame.setTUReserved(BattleActionType.BA_NONE);
			else if (_reserve == _btnReserveSnap)
				_battleGame.setTUReserved(BattleActionType.BA_SNAPSHOT);
			else if (_reserve == _btnReserveAimed)
				_battleGame.setTUReserved(BattleActionType.BA_AIMEDSHOT);
			else if (_reserve == _btnReserveAuto)
				_battleGame.setTUReserved(BattleActionType.BA_AUTOSHOT);

			// update any path preview
			if (_battleGame.getPathfinding().isPathPreviewed())
			{
				_battleGame.getPathfinding().removePreview();
				_battleGame.getPathfinding().previewPath();
			}
		}
	}

	/**
	 * Reserves time units for kneeling.
	 * @param action Pointer to an action.
	 */
	void btnReserveKneelClick(Action action)
	{
		if (allowButtons())
		{
			var ev = new SDL_Event();
			ev.type = SDL_EventType.SDL_MOUSEBUTTONDOWN;
			ev.button.button = (byte)SDL_BUTTON_LEFT;
			var a = new Action(ev, 0.0, 0.0, 0, 0);
			action.getSender().mousePress(a, this);
			_battleGame.setKneelReserved(!_battleGame.getKneelReserved());

			_btnReserveKneel.toggle(_battleGame.getKneelReserved());

			// update any path preview
			if (_battleGame.getPathfinding().isPathPreviewed())
			{
				_battleGame.getPathfinding().removePreview();
				_battleGame.getPathfinding().previewPath();
			}
		}
	}

	/**
	 * Removes all time units.
	 * @param action Pointer to an action.
	 */
	void btnZeroTUsClick(Action action)
	{
		if (allowButtons())
		{
			var ev = new SDL_Event();
			ev.type = SDL_EventType.SDL_MOUSEBUTTONDOWN;
			ev.button.button = (byte)SDL_BUTTON_LEFT;
			var a = new Action(ev, 0.0, 0.0, 0, 0);
			action.getSender().mousePress(a, this);
			if (_battleGame.getSave().getSelectedUnit() != null)
			{
				_battleGame.getSave().getSelectedUnit().setTimeUnits(0);
				updateSoldierInfo();
			}
		}
	}

	/**
	 * Reloads the weapon in hand.
	 * @param action Pointer to an action.
	 */
	void btnReloadClick(Action _)
	{
		if (playableUnitSelected() && _save.getSelectedUnit().checkAmmo())
		{
			_game.getMod().getSoundByDepth((uint)_save.getDepth(), (uint)Mod.Mod.ITEM_RELOAD).play(-1, getMap().getSoundAngle(_save.getSelectedUnit().getPosition()));
			updateSoldierInfo();
		}
	}

	/**
	 * Toggles soldier's personal lighting (purely cosmetic).
	 * @param action Pointer to an action.
	 */
	void btnPersonalLightingClick(Action _)
	{
		if (allowButtons())
			_save.getTileEngine().togglePersonalLighting();
	}

	/**
	 * Centers on the unit corresponding to this button.
	 * @param action Pointer to an action.
	 */
	void btnVisibleUnitClick(Action action)
	{
		int btnID = -1;

		// got to find out which button was pressed
		for (int i = 0; i < VISIBLE_MAX && btnID == -1; ++i)
		{
			if (action.getSender() == _btnVisibleUnit[i])
			{
				btnID = i;
			}
		}

		if (btnID != -1)
		{
			_map.getCamera().centerOnPosition(_visibleUnit[btnID].getPosition());
		}

		action.getDetails().type = SDL_EventType.SDL_FIRSTEVENT; //SDL_NOEVENT // consume the event
	}

	/**
	 * Launches the blaster bomb.
	 * @param action Pointer to an action.
	 */
	void btnLaunchClick(Action action)
	{
		_battleGame.launchAction();
		action.getDetails().type = SDL_EventType.SDL_FIRSTEVENT; //SDL_NOEVENT // consume the event
	}

	/**
	 * Uses psionics.
	 * @param action Pointer to an action.
	 */
	void btnPsiClick(Action action)
	{
		_battleGame.psiButtonAction();
		action.getDetails().type = SDL_EventType.SDL_FIRSTEVENT; //SDL_NOEVENT // consume the event
	}

	/**
	 * Animates map objects on the map, also smoke,fire, ...
	 */
	void animate()
	{
		_map.animate(!_battleGame.isBusy());

		blinkVisibleUnitButtons();
		blinkHealthBar();
	}

	static int delta = 1, color = 32;
	/**
	 * Shifts the red colors of the visible unit buttons backgrounds.
	 */
	void blinkVisibleUnitButtons()
	{
		for (int i = 0; i < VISIBLE_MAX; ++i)
		{
			if (_btnVisibleUnit[i].getVisible() == true)
			{
				_btnVisibleUnit[i].drawRect(0, 0, 15, 12, 15);
				_btnVisibleUnit[i].drawRect(1, 1, 13, 10, (byte)color);
			}
		}

		if (color == 44) delta = -2;
		if (color == 32) delta = 1;

		color += delta;
	}

	static byte color2 = 0, maxcolor = 3, step = 0;
	/**
	 * Shifts the colors of the health bar when unit has fatal wounds.
	 */
	void blinkHealthBar()
	{
		step = (byte)(1 - step);	// 1, 0, 1, 0, ...
		BattleUnit bu = _save.getSelectedUnit();
		if (step == 0 || bu == null || !_barHealth.getVisible()) return;

		if (++color2 > maxcolor) color2 = (byte)(maxcolor - 3);

		for (int i = 0; i < 6; i++)
		{
			if (bu.getFatalWound(i) > 0)
			{
				_barHealth.setColor((byte)(_barHealthColor + color2));
				return;
			}
		}
		if (_barHealth.getColor() != _barHealthColor) // avoid redrawing if we don't have to
			_barHealth.setColor(_barHealthColor);
	}

	/**
	 * Handles the battle game state.
	 */
	void handleState() =>
		_battleGame.handleState();

	/**
	 * Checks if the mouse is over the icons.
	 * @return True, if the mouse is over the icons.
	 */
	internal bool getMouseOverIcons() =>
		_mouseOverIcons;
}
