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
    internal void selectPreviousPlayerUnit(bool checkReselect, bool setReselect, bool checkInventory)
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
}
