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
    protected override void init()
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
    protected override void think()
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
    protected override void handle(Action action)
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
	    var image = new List<byte>();

	    Tile tile;

	    for (int z = 0; z < _save.getMapSizeZ()*12; ++z)
	    {
		    image.Clear();

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

				    image.Add((byte)((float)pal[test*3+0]*dist));
				    image.Add((byte)((float)pal[test*3+1]*dist));
				    image.Add((byte)((float)pal[test*3+2]*dist));
			    }
		    }

		    ss = $"{Options.getMasterUserFolder()}voxel{z:D2}.png";

		    uint error = lodepng.encode(ss, image, _save.getMapSizeX()*16, _save.getMapSizeY()*16, LCT_RGB);
		    if (error != 0)
		    {
                Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Saving to PNG failed: {lodepng_error_text(error)}");
		    }
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
        var span = new Span<byte>(img.pixels.ToPointer(), img.pitch * img.h);
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
		var image = new List<byte>();
		int test;
		Position originVoxel = getBattleGame().getTileEngine().getSightOriginVoxel(bu);

		Position targetVoxel = new(),hitPos = new();
		double dist = 0;
		bool _debug = _save.getDebugMode();
		double dir = ((double)bu.getDirection()+4)/4*M_PI;
		image.Clear();
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

				image.Add((byte)((double)(pal2[test*3+0])*dist));
				image.Add((byte)((double)(pal2[test*3+1])*dist));
				image.Add((byte)((double)(pal2[test*3+2])*dist));
			}
		}

		int i = 0;
		do
		{
			ss = $"{Options.getMasterUserFolder()}fpslook{i:D3}.png";
			i++;
		}
		while (CrossPlatform.fileExists(ss));

		uint error = lodepng.encode(ss, image, 512, 512, LCT_RGB);
		if (error != 0)
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Saving to PNG failed: {lodepng_error_text(error)}");
		}

		return;
	}

	/**
	 * Updates the scale.
	 * @param dX delta of X;
	 * @param dY delta of Y;
	 */
	protected override void resize(ref int dX, ref int dY)
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
}
