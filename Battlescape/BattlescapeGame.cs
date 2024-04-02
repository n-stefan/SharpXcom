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

enum BattleActionType { BA_NONE, BA_TURN, BA_WALK, BA_PRIME, BA_THROW, BA_AUTOSHOT, BA_SNAPSHOT, BA_AIMEDSHOT, BA_HIT, BA_USE, BA_LAUNCH, BA_MINDCONTROL, BA_PANIC, BA_RETHINK };

struct BattleAction
{
    internal BattleActionType type;
    internal BattleUnit actor;
    internal BattleItem weapon;
    internal Position target;
    internal List<Position> waypoints;
    internal int TU;
    internal bool targeting;
    internal int value;
    internal string result;
    internal bool strafe, run;
    internal int diff;
    internal int autoShotCounter;
    internal Position cameraPosition;
    internal bool desperate; // ignoring newly-spotted units
    internal int finalFacing;
    internal bool finalAction;
    internal int number; // first action of turn, second, etc.?

    public BattleAction()
    {
        type = BattleActionType.BA_NONE;
        actor = null;
        weapon = null;
        TU = 0;
        targeting = false;
        value = 0;
        strafe = false;
        run = false;
        diff = 0;
        autoShotCounter = 0;
        cameraPosition = new Position(0, 0, -1);
        desperate = false;
        finalFacing = -1;
        finalAction = false;
        number = 0;
    }
};

/**
 * Battlescape game - the core game engine of the battlescape game.
 */
internal class BattlescapeGame
{
    SavedBattleGame _save;
    BattlescapeState _parentState;
    bool _playerPanicHandled;
    int _AIActionCounter;
    bool _AISecondMove, _playedAggroSound;
    bool _endTurnRequested, _endTurnProcessed;
    BattleAction _currentAction;
    List<BattleState> _states, _deleted;
    int _currentState;
    /// is debug mode enabled in the battlescape?
    internal static bool _debugPlay;
    List<InfoboxOKState> _infoboxQueue;

    /**
     * Initializes all the elements in the Battlescape screen.
     * @param save Pointer to the save game.
     * @param parentState Pointer to the parent battlescape state.
     */
    internal BattlescapeGame(SavedBattleGame save, BattlescapeState parentState)
    {
        _save = save;
        _parentState = parentState;
        _playerPanicHandled = true;
        _AIActionCounter = 0;
        _AISecondMove = false;
        _playedAggroSound = false;
        _endTurnRequested = false;
        _endTurnProcessed = false;

        _currentAction.actor = null;
        _currentAction.targeting = false;
        _currentAction.type = BattleActionType.BA_NONE;

        _debugPlay = false;

        checkForCasualties(null, null, true);
        cancelCurrentAction();
    }

    /**
     * Delete BattlescapeGame.
     */
    ~BattlescapeGame()
    {
        _states.Clear();
        cleanupDeleted();
    }

    /**
     * Cleans up all the deleted states.
     */
    internal void cleanupDeleted() =>
        _deleted.Clear();

    /**
      * Cancels the current action the user had selected (firing, throwing,..)
      * @param bForce Force the action to be cancelled.
      * @return Whether an action was cancelled or not.
      */
    internal bool cancelCurrentAction(bool bForce = false)
    {
        bool bPreviewed = Options.battleNewPreviewPath != PathPreview.PATH_NONE;

        if (_save.getPathfinding().removePreview() && bPreviewed) return true;

        if (!_states.Any() || bForce)
        {
            if (_currentAction.targeting)
            {
                if (_currentAction.type == BattleActionType.BA_LAUNCH && _currentAction.waypoints.Any())
                {
                    _currentAction.waypoints.RemoveAt(_currentAction.waypoints.Count - 1);
                    var waypoints = getMap().getWaypoints();
                    if (waypoints.Any())
                    {
                        waypoints.RemoveAt(waypoints.Count - 1);
                    }
                    if (!_currentAction.waypoints.Any())
                    {
                        _parentState.showLaunchButton(false);
                    }
                    return true;
                }
                else
                {
                    if (Options.battleConfirmFireMode && _currentAction.waypoints.Any())
                    {
                        _currentAction.waypoints.RemoveAt(_currentAction.waypoints.Count - 1);
                        var waypoints = getMap().getWaypoints();
                        waypoints.RemoveAt(waypoints.Count - 1);
                        return true;
                    }
                    _currentAction.targeting = false;
                    _currentAction.type = BattleActionType.BA_NONE;
                    setupCursor();
                    _parentState.getGame().getCursor().setVisible(true);
                    return true;
                }
            }
        }
        else if (_states.Any() && _states.First() != null)
        {
            _states.First().cancel();
            return true;
        }

        return false;
    }

    /**
     * Gets the map.
     * @return map.
     */
    internal Map getMap() =>
        _parentState.getMap();

    /**
     * Sets the cursor according to the selected action.
     */
    internal void setupCursor()
    {
        if (_currentAction.targeting)
        {
            if (_currentAction.type == BattleActionType.BA_THROW)
            {
                getMap().setCursorType(CursorType.CT_THROW);
            }
            else if (_currentAction.type == BattleActionType.BA_MINDCONTROL || _currentAction.type == BattleActionType.BA_PANIC || _currentAction.type == BattleActionType.BA_USE)
            {
                getMap().setCursorType(CursorType.CT_PSI);
            }
            else if (_currentAction.type == BattleActionType.BA_LAUNCH)
            {
                getMap().setCursorType(CursorType.CT_WAYPOINT);
            }
            else
            {
                getMap().setCursorType(CursorType.CT_AIM);
            }
        }
        else if (_currentAction.type != BattleActionType.BA_HIT)
        {
            _currentAction.actor = _save.getSelectedUnit();
            if (_currentAction.actor != null)
            {
                getMap().setCursorType(CursorType.CT_NORMAL, _currentAction.actor.getArmor().getSize());
            }
            else
            {
                getMap().setCursorType(CursorType.CT_NORMAL);
            }
        }
    }

    /**
     * Returns the action type that is reserved.
     * @return The type of action that is reserved.
     */
    internal BattleActionType getReservedAction() =>
        _save.getTUReserved();

    /**
     * Sets the TU reserved type.
     * @param tur A battleactiontype.
     * @param player is this requested by the player?
     */
    internal void setTUReserved(BattleActionType tur) =>
        _save.setTUReserved(tur);

    /**
     * Checks against reserved time units.
     * @param bu Pointer to the unit.
     * @param tu Number of time units to check.
     * @param justChecking True to suppress error messages, false otherwise.
     * @return bool Whether or not we got enough time units.
     */
    internal bool checkReservedTU(BattleUnit bu, int tu, bool justChecking = false)
    {
        BattleActionType effectiveTuReserved = _save.getTUReserved(); // avoid changing _tuReserved in this method

        if (_save.getSide() != bu.getFaction() || _save.getSide() == UnitFaction.FACTION_NEUTRAL)
        {
            return tu <= bu.getTimeUnits();
        }

        if (_save.getSide() == UnitFaction.FACTION_HOSTILE && !_debugPlay) // aliens reserve TUs as a percentage rather than just enough for a single action.
        {
            AIModule ai = bu.getAIModule();
            if (ai != null)
            {
                effectiveTuReserved = ai.getReserveMode();
            }
            switch (effectiveTuReserved)
            {
                case BattleActionType.BA_SNAPSHOT: return tu + (bu.getBaseStats().tu / 3) <= bu.getTimeUnits(); // 33%
                case BattleActionType.BA_AUTOSHOT: return tu + ((bu.getBaseStats().tu / 5) * 2) <= bu.getTimeUnits(); // 40%
                case BattleActionType.BA_AIMEDSHOT: return tu + (bu.getBaseStats().tu / 2) <= bu.getTimeUnits(); // 50%
                default: return tu <= bu.getTimeUnits();
            }
        }

        // check TUs against slowest weapon if we have two weapons
        BattleItem slowestWeapon = bu.getMainHandWeapon(false);
        // if the weapon has no autoshot, reserve TUs for snapshot
        if (bu.getActionTUs(effectiveTuReserved, slowestWeapon) == 0 && effectiveTuReserved == BattleActionType.BA_AUTOSHOT)
        {
            effectiveTuReserved = BattleActionType.BA_SNAPSHOT;
        }
        // likewise, if we don't have a snap shot available, try aimed.
        if (bu.getActionTUs(effectiveTuReserved, slowestWeapon) == 0 && effectiveTuReserved == BattleActionType.BA_SNAPSHOT)
        {
            effectiveTuReserved = BattleActionType.BA_AIMEDSHOT;
        }
        int tuKneel = (_save.getKneelReserved() && !bu.isKneeled() && bu.getType() == "SOLDIER") ? 4 : 0;
        // no aimed shot available? revert to none.
        if (bu.getActionTUs(effectiveTuReserved, slowestWeapon) == 0 && effectiveTuReserved == BattleActionType.BA_AIMEDSHOT)
        {
            if (tuKneel > 0)
            {
                effectiveTuReserved = BattleActionType.BA_NONE;
            }
            else
            {
                return true;
            }
        }

        if ((effectiveTuReserved != BattleActionType.BA_NONE || _save.getKneelReserved()) &&
            tu + tuKneel + bu.getActionTUs(effectiveTuReserved, slowestWeapon) > bu.getTimeUnits() &&
            (tuKneel + bu.getActionTUs(effectiveTuReserved, slowestWeapon) <= bu.getTimeUnits() || justChecking))
        {
            if (!justChecking)
            {
                if (tuKneel != 0)
                {
                    switch (effectiveTuReserved)
                    {
                        case BattleActionType.BA_NONE: _parentState.warning("STR_TIME_UNITS_RESERVED_FOR_KNEELING"); break;
                        default: _parentState.warning("STR_TIME_UNITS_RESERVED_FOR_KNEELING_AND_FIRING"); break;
                    }
                }
                else
                {
                    switch (_save.getTUReserved())
                    {
                        case BattleActionType.BA_SNAPSHOT: _parentState.warning("STR_TIME_UNITS_RESERVED_FOR_SNAP_SHOT"); break;
                        case BattleActionType.BA_AUTOSHOT: _parentState.warning("STR_TIME_UNITS_RESERVED_FOR_AUTO_SHOT"); break;
                        case BattleActionType.BA_AIMEDSHOT: _parentState.warning("STR_TIME_UNITS_RESERVED_FOR_AIMED_SHOT"); break;
                        default: break;
                    }
                }
            }
            return false;
        }

        return true;
    }

    /**
     * Checks for casualties and adjusts morale accordingly.
     * @param murderweapon Need to know this, for a HE explosion there is an instant death.
     * @param origMurderer This is needed for credits for the kill.
     * @param hiddenExplosion Set to true for the explosions of UFO Power sources at start of battlescape.
     * @param terrainExplosion Set to true for the explosions of terrain.
     */
    internal void checkForCasualties(BattleItem murderweapon, BattleUnit origMurderer, bool hiddenExplosion = false, bool terrainExplosion = false)
    {
        // If the victim was killed by the murderer's death explosion, fetch who killed the murderer and make HIM the murderer!
        if (origMurderer != null && origMurderer.getGeoscapeSoldier() == null && (origMurderer.getUnitRules().getSpecialAbility() == (int)SpecialAbility.SPECAB_EXPLODEONDEATH || origMurderer.getUnitRules().getSpecialAbility() == (int)SpecialAbility.SPECAB_BURN_AND_EXPLODE)
            && origMurderer.getStatus() == UnitStatus.STATUS_DEAD && origMurderer.getMurdererId() != 0)
        {
            foreach (var i in _save.getUnits())
            {
                if (i.getId() == origMurderer.getMurdererId())
                {
                    origMurderer = i;
                }
            }
        }

        // Fetch the murder weapon
        string tempWeapon = "STR_WEAPON_UNKNOWN", tempAmmo = "STR_WEAPON_UNKNOWN";
        if (origMurderer != null)
        {
            if (murderweapon != null)
            {
                tempAmmo = murderweapon.getRules().getName();
                tempWeapon = tempAmmo;
            }

            BattleItem weapon = origMurderer.getItem("STR_RIGHT_HAND");
            if (weapon != null)
            {
                foreach (var c in weapon.getRules().getCompatibleAmmo())
                {
                    if (c == tempAmmo)
                    {
                        tempWeapon = weapon.getRules().getName();
                    }
                }
            }
            weapon = origMurderer.getItem("STR_LEFT_HAND");
            if (weapon != null)
            {
                foreach (var c in weapon.getRules().getCompatibleAmmo())
                {
                    if (c == tempAmmo)
                    {
                        tempWeapon = weapon.getRules().getName();
                    }
                }
            }
        }

        foreach (var j in _save.getUnits())
        {
            if (j.getStatus() == UnitStatus.STATUS_IGNORE_ME) continue;
            BattleUnit victim = j;
            BattleUnit murderer = origMurderer;

            var killStat = new BattleUnitKills();
            killStat.mission = _parentState.getGame().getSavedGame().getMissionStatistics().Count;
            killStat.setTurn(_save.getTurn(), _save.getSide());
            killStat.setUnitStats(victim);
            killStat.faction = victim.getFaction();
            killStat.side = victim.getFatalShotSide();
            killStat.bodypart = victim.getFatalShotBodyPart();
            killStat.id = victim.getId();
            killStat.weapon = tempWeapon;
            killStat.weaponAmmo = tempAmmo;

            // Determine murder type
            if (j.getStatus() != UnitStatus.STATUS_DEAD)
            {
                if (j.getHealth() == 0)
                {
                    killStat.status = UnitStatus.STATUS_DEAD;
                }
                else if (j.getStunlevel() >= j.getHealth() && j.getStatus() != UnitStatus.STATUS_UNCONSCIOUS)
                {
                    killStat.status = UnitStatus.STATUS_UNCONSCIOUS;
                }
            }

            // Assume that, in absence of a murderer and an explosion, the laster unit to hit the victim is the murderer.
            // Possible causes of death: bleed out, fire.
            // Possible causes of unconciousness: wounds, smoke.
            // Assumption : The last person to hit the victim is the murderer.
		    if (murderer == null && !terrainExplosion)
            {
                foreach (var i in _save.getUnits())
                {
                    if (i.getId() == victim.getMurdererId())
                    {
                        murderer = i;
                        killStat.weapon = victim.getMurdererWeapon();
                        killStat.weaponAmmo = victim.getMurdererWeaponAmmo();
                        break;
                    }
                }
            }

            if (murderer != null && killStat.status != UnitStatus.STATUS_IGNORE_ME)
            {
                if (murderer.getFaction() == UnitFaction.FACTION_PLAYER && murderer.getOriginalFaction() != UnitFaction.FACTION_PLAYER)
                {
                    // This must be a mind controlled unit. Find out who mind controlled him and award the kill to that unit.
                    foreach (var i in _save.getUnits())
                    {
                        if (i.getId() == murderer.getMindControllerId() && i.getGeoscapeSoldier() != null)
                        {
                            i.getStatistics().kills.Add(killStat);
                            if (victim.getFaction() == UnitFaction.FACTION_HOSTILE)
                            {
                                i.getStatistics().slaveKills++;
                            }
                            victim.setMurdererId(i.getId());
                            break;
                        }
                    }
                }
                else if (!murderer.getStatistics().duplicateEntry(killStat.status, victim.getId()))
                {
                    murderer.getStatistics().kills.Add(killStat);
                    victim.setMurdererId(murderer.getId());
                }
            }

            bool noSound = false;
            bool noCorpse = false;
            if (j.getStatus() != UnitStatus.STATUS_DEAD)
            {
                if (j.getHealth() == 0)
                {
                    if (j.getStatus() == UnitStatus.STATUS_UNCONSCIOUS)
                    {
                        noCorpse = true;
                    }
                    if (murderer != null)
                    {
                        murderer.addKillCount();
                        victim.killedBy(murderer.getFaction());
                        int modifier = murderer.getFaction() == UnitFaction.FACTION_PLAYER ? _save.getMoraleModifier() : 100;

                        // if there is a known murderer, he will get a morale bonus if he is of a different faction (what with neutral?)
                        if ((victim.getOriginalFaction() == UnitFaction.FACTION_PLAYER && murderer.getFaction() == UnitFaction.FACTION_HOSTILE) ||
                            (victim.getOriginalFaction() == UnitFaction.FACTION_HOSTILE && murderer.getFaction() == UnitFaction.FACTION_PLAYER))
                        {
                            murderer.moraleChange(20 * modifier / 100);
                        }
                        // murderer will get a penalty with friendly fire
                        if (victim.getOriginalFaction() == murderer.getOriginalFaction())
                        {
                            murderer.moraleChange(-(2000 / modifier));
                        }
                        if (victim.getOriginalFaction() == UnitFaction.FACTION_NEUTRAL)
                        {
                            if (murderer.getOriginalFaction() == UnitFaction.FACTION_PLAYER)
                            {
                                murderer.moraleChange(-(1000 / modifier));
                            }
                            else
                            {
                                murderer.moraleChange(10);
                            }
                        }
                    }

                    if (victim.getFaction() != UnitFaction.FACTION_NEUTRAL)
                    {
                        int modifier = _save.getMoraleModifier(victim);
                        int loserMod = victim.getFaction() == UnitFaction.FACTION_HOSTILE ? 100 : _save.getMoraleModifier();
                        int winnerMod = victim.getFaction() == UnitFaction.FACTION_HOSTILE ? _save.getMoraleModifier() : 100;
                        foreach (var i in _save.getUnits())
                        {
                            if (!i.isOut() && i.getArmor().getSize() == 1)
                            {
                                // the losing squad all get a morale loss
                                if (i.getOriginalFaction() == victim.getOriginalFaction())
                                {
                                    int bravery = (110 - i.getBaseStats().bravery) / 10;
                                    i.moraleChange(-(modifier * 200 * bravery / loserMod / 100));

                                    if (victim.getFaction() == UnitFaction.FACTION_HOSTILE && murderer != null)
                                    {
                                        murderer.setTurnsSinceSpotted(0);
                                    }
                                }
                                // the winning squad all get a morale increase
                                else
                                {
                                    i.moraleChange(10 * winnerMod / 100);
                                }
                            }
                        }
                    }
                    if (murderweapon != null)
                    {
                        statePushNext(new UnitDieBState(this, j, murderweapon.getRules().getDamageType(), noSound, noCorpse));
                    }
                    else
                    {
                        if (hiddenExplosion)
                        {
                            // this is instant death from UFO powersources, without screaming sounds
                            noSound = true;
                            statePushNext(new UnitDieBState(this, j, ItemDamageType.DT_HE, noSound, noCorpse));
                        }
                        else
                        {
                            if (terrainExplosion)
                            {
                                // terrain explosion
                                statePushNext(new UnitDieBState(this, j, ItemDamageType.DT_HE, noSound, noCorpse));
                            }
                            else
                            {
                                // no murderer, and no terrain explosion, must be fatal wounds
                                statePushNext(new UnitDieBState(this, j, ItemDamageType.DT_NONE, noSound, noCorpse));  // DT_NONE = STR_HAS_DIED_FROM_A_FATAL_WOUND
                            }
                        }
                    }
                    // one of our own died, record the murderer instead of the victim
                    if (victim.getGeoscapeSoldier() != null)
                    {
                        victim.getStatistics().KIA = true;
                        BattleUnitKills deathStat = killStat;
                        if (murderer != null)
                        {
                            deathStat.setUnitStats(murderer);
                            deathStat.faction = murderer.getFaction();
                        }
                        _parentState.getGame().getSavedGame().killSoldier(victim.getGeoscapeSoldier(), deathStat);
                    }
                }
                else if (j.getStunlevel() >= j.getHealth() && j.getStatus() != UnitStatus.STATUS_UNCONSCIOUS)
                {
                    if (victim.getGeoscapeSoldier() != null)
                    {
                        victim.getStatistics().wasUnconcious = true;
                    }
                    noSound = true;
                    statePushNext(new UnitDieBState(this, j, ItemDamageType.DT_STUN, noSound, noCorpse));
                }
            }
        }

        BattleUnit bu = _save.getSelectedUnit();
        if (_save.getSide() == UnitFaction.FACTION_PLAYER)
        {
            _parentState.showPsiButton(bu != null && bu.getSpecialWeapon(BattleType.BT_PSIAMP) != null && !bu.isOut());
        }
    }

    /**
     * Pushes a state as the next state after the current one.
     * @param bs Battlestate.
     */
    internal void statePushNext(BattleState bs)
    {
        if (!_states.Any())
        {
            _states.Insert(0, bs);
            bs.init();
        }
        else
        {
            _states.Insert(++_currentState, bs);
        }
    }

    /**
     * Gets the save.
     * @return save.
     */
    internal SavedBattleGame getSave() =>
        _save;

    /**
     * Sets the timer interval for think() calls of the state.
     * @param interval An interval in ms.
     */
    internal void setStateInterval(uint interval) =>
        _parentState.setStateInterval(interval);

    /**
     * Drops an item to the floor and affects it with gravity.
     * @param position Position to spawn the item.
     * @param item Pointer to the item.
     * @param newItem Bool whether this is a new item.
     * @param removeItem Bool whether to remove the item from the owner.
     */
    internal void dropItem(Position position, BattleItem item, bool newItem = false, bool removeItem = false) =>
        getTileEngine().itemDrop(_save.getTile(position), item, getMod(), newItem, removeItem);

    /**
     * Gets the tilengine.
     * @return tilengine.
     */
    internal TileEngine getTileEngine() =>
        _save.getTileEngine();

    /**
     * Gets the mod.
     * @return mod.
     */
    internal Mod.Mod getMod() =>
        _parentState.getGame().getMod();

    /**
     * Ends the turn if auto-end battle is enabled
     * and all mission objectives are completed.
     */
    internal void autoEndBattle()
    {
        if (Options.battleAutoEnd)
        {
            bool end = false;
            if (_save.getObjectiveType() == SpecialTileType.MUST_DESTROY)
            {
                end = _save.allObjectivesDestroyed();
            }
            else
            {
                int liveAliens = 0;
                int liveSoldiers = 0;
                tallyUnits(out liveAliens, out liveSoldiers);
                end = (liveAliens == 0 || liveSoldiers == 0);
            }
            if (end)
            {
                _save.setSelectedUnit(null);
                cancelCurrentAction(true);
                requestEndTurn();
            }
        }
    }

    /**
     * Sets up a mission complete notification.
     */
    internal void missionComplete()
    {
        Game game = _parentState.getGame();
        if (game.getMod().getDeployment(_save.getMissionType()) != null)
        {
            string missionComplete = game.getMod().getDeployment(_save.getMissionType()).getObjectivePopup();
            if (!string.IsNullOrEmpty(missionComplete))
            {
                _infoboxQueue.Add(new InfoboxOKState(game.getLanguage().getString(missionComplete)));
            }
        }
    }

    /**
     * Requests the end of the turn (waits for explosions etc to really end the turn).
     */
    internal void requestEndTurn()
    {
        cancelCurrentAction();
        if (!_endTurnRequested)
        {
            _endTurnRequested = true;
            statePushBack(null);
        }
    }

    /**
     * Pushes a state to the back.
     * @param bs Battlestate.
     */
    internal void statePushBack(BattleState bs)
    {
        if (!_states.Any())
        {
            _states.Insert(0, bs);
            // end turn request?
            if (_states.First() == null)
            {
                _states.RemoveAt(0);
                endTurn();
                return;
            }
            else
            {
                bs.init();
            }
        }
        else
        {
            _states.Add(bs);
        }
    }

    /**
     * Tallies the living units in the game and, if required, converts units into their spawn unit.
     * @param &liveAliens The integer in which to store the live alien tally.
     * @param &liveSoldiers The integer in which to store the live XCom tally.
     * @param convert Should we convert infected units?
     */
    internal void tallyUnits(out int liveAliens, out int liveSoldiers)
    {
        liveSoldiers = 0;
        liveAliens = 0;

        foreach (var j in _save.getUnits())
        {
            if (!j.isOut())
            {
                if (j.getOriginalFaction() == UnitFaction.FACTION_HOSTILE)
                {
                    if (!Options.allowPsionicCapture || j.getFaction() != UnitFaction.FACTION_PLAYER || !j.getCapturable())
                    {
                        liveAliens++;
                    }
                }
                else if (j.getOriginalFaction() == UnitFaction.FACTION_PLAYER)
                {
                    if (j.getFaction() == UnitFaction.FACTION_PLAYER)
                    {
                        liveSoldiers++;
                    }
                    else
                    {
                        liveAliens++;
                    }
                }
            }
        }
    }

    /**
     * Ends the turn.
     */
    void endTurn()
    {
        _debugPlay = false;
        _currentAction.type = BattleActionType.BA_NONE;
        getMap().getWaypoints().Clear();
        _currentAction.waypoints.Clear();
        _parentState.showLaunchButton(false);
        _currentAction.targeting = false;
        _AISecondMove = false;

        if (!_endTurnProcessed)
        {
            if (_save.getTileEngine().closeUfoDoors() != 0 && Mod.Mod.SLIDING_DOOR_CLOSE != -1)
            {
                getMod().getSoundByDepth((uint)_save.getDepth(), (uint)Mod.Mod.SLIDING_DOOR_CLOSE).play(); // ufo door closed
            }

            var p = new Position();

            // check for hot grenades on the ground
            if (_save.getSide() != UnitFaction.FACTION_NEUTRAL)
            {
                for (int i = 0; i < _save.getMapSizeXYZ(); ++i)
                {
                    var inventory = _save.getTiles()[i].getInventory();
                    for (var it = 0; it < inventory.Count;)
                    {
                        if (inventory[it].getRules().getBattleType() == BattleType.BT_GRENADE && inventory[it].getFuseTimer() == 0)  // it's a grenade to explode now
                        {
                            p.x = _save.getTiles()[i].getPosition().x * 16 + 8;
                            p.y = _save.getTiles()[i].getPosition().y * 16 + 8;
                            p.z = _save.getTiles()[i].getPosition().z * 24 - _save.getTiles()[i].getTerrainLevel();
                            statePushNext(new ExplosionBState(this, p, inventory[it], inventory[it].getPreviousOwner()));
                            _save.removeItem(inventory[it]);
                            statePushBack(null);
                            return;
                        }
                        ++it;
                    }
                }
            }
        }
        // check for terrain explosions
        Tile t = _save.getTileEngine().checkForTerrainExplosions();
        if (t != null)
        {
            Position p = new Position(t.getPosition().x * 16, t.getPosition().y * 16, t.getPosition().z * 24);
            statePushNext(new ExplosionBState(this, p, null, null, t));
            statePushBack(null);
            return;
        }

        if (!_endTurnProcessed)
        {
            if (_save.getSide() != UnitFaction.FACTION_NEUTRAL)
            {
                foreach (var it in _save.getItems())
                {
                    if ((it.getRules().getBattleType() == BattleType.BT_GRENADE || it.getRules().getBattleType() == BattleType.BT_PROXIMITYGRENADE) && it.getFuseTimer() > 0)
                    {
                        it.setFuseTimer(it.getFuseTimer() - 1);
                    }
                }
            }

            _save.endTurn();
            t = _save.getTileEngine().checkForTerrainExplosions();
            if (t != null)
            {
                Position p = new Position(t.getPosition().x * 16, t.getPosition().y * 16, t.getPosition().z * 24);
                statePushNext(new ExplosionBState(this, p, null, null, t));
                statePushBack(null);
                _endTurnProcessed = true;
                return;
            }
        }

        _endTurnProcessed = false;

        if (_save.getSide() == UnitFaction.FACTION_PLAYER)
        {
            setupCursor();
        }
        else
        {
            getMap().setCursorType(CursorType.CT_NONE);
        }

        checkForCasualties(null, null, false, false);

        // turn off MCed alien lighting.
        _save.getTileEngine().calculateUnitLighting();

        // if all units from either faction are killed - the mission is over.
        int liveAliens = 0;
        int liveSoldiers = 0;
        int inExit = 0;

        // Calculate values
        foreach (var j in _save.getUnits())
        {
            if (!j.isOut())
            {
                if (j.getOriginalFaction() == UnitFaction.FACTION_HOSTILE)
                {
                    if (!Options.allowPsionicCapture || j.getFaction() != UnitFaction.FACTION_PLAYER || !j.getCapturable())
                    {
                        liveAliens++;
                    }
                }
                else if (j.getOriginalFaction() == UnitFaction.FACTION_PLAYER)
                {
                    if (j.isInExitArea(SpecialTileType.END_POINT))
                    {
                        inExit++;
                    }
                    if (j.getFaction() == UnitFaction.FACTION_PLAYER)
                    {
                        liveSoldiers++;
                    }
                    else
                    {
                        liveAliens++;
                    }
                }
            }
        }

        if (_save.allObjectivesDestroyed() && _save.getObjectiveType() == SpecialTileType.MUST_DESTROY)
        {
            _parentState.finishBattle(false, liveSoldiers);
            return;
        }
        if (_save.getTurnLimit() > 0 && _save.getTurn() > _save.getTurnLimit())
        {
            switch (_save.getChronoTrigger())
            {
                case ChronoTrigger.FORCE_ABORT:
                    _save.setAborted(true);
                    _parentState.finishBattle(true, inExit);
                    return;
                case ChronoTrigger.FORCE_WIN:
                    _parentState.finishBattle(false, liveSoldiers);
                    return;
                case ChronoTrigger.FORCE_LOSE:
                default:
                    // force mission failure
                    _save.setAborted(true);
                    _parentState.finishBattle(false, 0);
                    return;
            }
        }

        if (liveAliens > 0 && liveSoldiers > 0)
        {
            showInfoBoxQueue();

            _parentState.updateSoldierInfo();

            if (playableUnitSelected())
            {
                getMap().getCamera().centerOnPosition(_save.getSelectedUnit().getPosition());
                setupCursor();
            }
        }

        bool battleComplete = liveAliens == 0 || liveSoldiers == 0;

        if ((_save.getSide() != UnitFaction.FACTION_NEUTRAL || battleComplete)
            && _endTurnRequested)
        {
            _parentState.getGame().pushState(new NextTurnState(_save, _parentState));
        }
        _endTurnRequested = false;
    }

    /**
     * Shows the infoboxes in the queue (if any).
     */
    void showInfoBoxQueue()
    {
        foreach (var i in _infoboxQueue)
        {
            _parentState.getGame().pushState(i);
        }

        _infoboxQueue.Clear();
    }

    /**
     * Determines whether a playable unit is selected. Normally only player side units can be selected, but in debug mode one can play with aliens too :)
     * Is used to see if stats can be displayed.
     * @return Whether a playable unit is selected.
     */
    internal bool playableUnitSelected() =>
	    _save.getSelectedUnit() != null && (_save.getSide() == UnitFaction.FACTION_PLAYER || _save.getDebugMode());

    /// Returns whether panic has been handled.
    internal bool getPanicHandled() =>
        _playerPanicHandled;

    /**
     * Determines whether an action is currently going on?
     * @return true or false.
     */
    internal bool isBusy() =>
	    _states.Any();

    internal List<BattleState> getStates() =>
        _states;

    /**
     * Gets a pointer to access action members directly.
     * @return Pointer to action.
     */
    internal ref BattleAction getCurrentAction() =>
        ref _currentAction;

    /**
      * Cancels all selected user actions.
      */
    internal void cancelAllActions()
    {
        _save.getPathfinding().removePreview();

        _currentAction.waypoints.Clear();
        getMap().getWaypoints().Clear();
        _parentState.showLaunchButton(false);

        _currentAction.targeting = false;
        _currentAction.type = BattleActionType.BA_NONE;
        setupCursor();
        _parentState.getGame().getCursor().setVisible(true);
    }

    /**
     * Sets the kneel reservation setting.
     * @param reserved Should we reserve an extra 4 TUs to kneel?
     */
    internal void setKneelReserved(bool reserved) =>
	    _save.setKneelReserved(reserved);

    /**
     * Initializes the Battlescape game.
     */
    internal void init()
    {
	    if (_save.getSide() == UnitFaction.FACTION_PLAYER && _save.getTurn() > 1)
	    {
		    _playerPanicHandled = false;
	    }
    }

    /**
     * Gets the depth of the battlescape.
     * @return the depth of the battlescape.
     */
    internal int getDepth() =>
	    _save.getDepth();

    /**
     * Removes the current state.
     *
     * This is a very important function. It is called by a BattleState (walking, projectile is flying, explosions,...) at the moment this state has finished its action.
     * Here we check the result of that action and do all the aftermath.
     * The state is popped off the list.
     */
    internal void popState()
    {
	    if (Options.traceAI)
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} BattlescapeGame.popState() #{_AIActionCounter} with {(_save.getSelectedUnit() != null ? _save.getSelectedUnit().getTimeUnits() : -9999)} TU");
	    }
	    bool actionFailed = false;

	    if (!_states.Any()) return;

	    BattleAction action = _states.First().getAction();

	    if (action.actor != null && action.result.Any() && action.actor.getFaction() == UnitFaction.FACTION_PLAYER
		    && _playerPanicHandled && (_save.getSide() == UnitFaction.FACTION_PLAYER || _debugPlay))
	    {
		    _parentState.warning(action.result);
		    actionFailed = true;
	    }
	    _deleted.Add(_states.First());
	    _states.RemoveAt(0);

	    // handle the end of this unit's actions
	    if (action.actor != null && noActionsPending(action.actor))
	    {
		    if (action.actor.getFaction() == UnitFaction.FACTION_PLAYER)
		    {
			    // spend TUs of "target triggered actions" (shooting, throwing) only
			    // the other actions' TUs (healing,scanning,..) are already take care of
			    if (action.targeting && _save.getSelectedUnit() != null && !actionFailed)
			    {
				    action.actor.spendTimeUnits(action.TU);
			    }
			    if (_save.getSide() == UnitFaction.FACTION_PLAYER)
			    {
				    // after throwing the cursor returns to default cursor, after shooting it stays in targeting mode and the player can shoot again in the same mode (autoshot,snap,aimed)
				    if ((action.type == BattleActionType.BA_THROW || action.type == BattleActionType.BA_LAUNCH) && !actionFailed)
				    {
					    // clean up the waypoints
					    if (action.type == BattleActionType.BA_LAUNCH)
					    {
						    _currentAction.waypoints.Clear();
					    }

					    cancelCurrentAction(true);
				    }
				    _parentState.getGame().getCursor().setVisible(true);
				    setupCursor();
			    }
		    }
		    else
		    {
			    // spend TUs
			    action.actor.spendTimeUnits(action.TU);
			    if (_save.getSide() != UnitFaction.FACTION_PLAYER && !_debugPlay)
			    {
				    // AI does three things per unit, before switching to the next, or it got killed before doing the second thing
				    if (_AIActionCounter > 2 || _save.getSelectedUnit() == null || _save.getSelectedUnit().isOut())
				    {
					    if (_save.getSelectedUnit() != null)
					    {
						    _save.getSelectedUnit().setCache(null);
						    getMap().cacheUnit(_save.getSelectedUnit());
					    }
					    _AIActionCounter = 0;
					    if (!_states.Any() && _save.selectNextPlayerUnit(true) == null)
					    {
						    if (!_save.getDebugMode())
						    {
							    _endTurnRequested = true;
							    statePushBack(null); // end AI turn
						    }
						    else
						    {
							    _save.selectNextPlayerUnit();
							    _debugPlay = true;
						    }
					    }
					    if (_save.getSelectedUnit() != null)
					    {
						    getMap().getCamera().centerOnPosition(_save.getSelectedUnit().getPosition());
					    }
				    }
			    }
			    else if (_debugPlay)
			    {
				    _parentState.getGame().getCursor().setVisible(true);
				    setupCursor();
			    }
		    }
	    }

	    if (_states.Any())
	    {
		    // end turn request?
		    if (_states.First() == null)
		    {
			    while (_states.Any())
			    {
				    if (_states.First() == null)
					    _states.RemoveAt(0);
				    else
					    break;
			    }
			    if (!_states.Any())
			    {
				    endTurn();
				    return;
			    }
			    else
			    {
				    _states.Add(null);
			    }
		    }
		    // init the next state in queue
		    _states.First().init();
	    }

	    // the currently selected unit died or became unconscious or disappeared inexplicably
	    if (_save.getSelectedUnit() == null || _save.getSelectedUnit().isOut())
	    {
		    cancelCurrentAction();
		    getMap().setCursorType(CursorType.CT_NORMAL, 1);
		    _parentState.getGame().getCursor().setVisible(true);
		    if (_save.getSide() == UnitFaction.FACTION_PLAYER)
			    _save.setSelectedUnit(null);
		    else
			    _save.selectNextPlayerUnit(true, true);
	    }
	    _parentState.updateSoldierInfo();
    }

    /**
     * Determines whether there are any actions pending for the given unit.
     * @param bu BattleUnit.
     * @return True if there are no actions pending.
     */
    bool noActionsPending(BattleUnit bu)
    {
	    if (!_states.Any()) return true;

	    foreach (var i in _states)
	    {
		    if (i != null && i.getAction().actor == bu)
			    return false;
	    }

	    return true;
    }

    /**
     * Pushes a state to the front of the queue and starts it.
     * @param bs Battlestate.
     */
    internal void statePushFront(BattleState bs)
    {
	    _states.Insert(0, bs);
	    bs.init();
    }

    /**
     * Gets the pathfinding.
     * @return pathfinding.
     */
    internal Pathfinding getPathfinding() =>
	    _save.getPathfinding();

    /**
     * Checks for units panicking or falling and so on.
     */
    internal void think()
    {
	    // nothing is happening - see if we need some alien AI or units panicking or what have you
	    if (!_states.Any())
	    {
		    if (_save.getUnitsFalling())
		    {
			    statePushFront(new UnitFallBState(this));
			    _save.setUnitsFalling(false);
			    return;
		    }
		    // it's a non player side (ALIENS or CIVILIANS)
		    if (_save.getSide() != UnitFaction.FACTION_PLAYER)
		    {
			    _save.resetUnitHitStates();
			    if (!_debugPlay)
			    {
				    if (_save.getSelectedUnit() != null)
				    {
					    if (!handlePanickingUnit(_save.getSelectedUnit()))
						    handleAI(_save.getSelectedUnit());
				    }
				    else
				    {
					    if (_save.selectNextPlayerUnit(true, _AISecondMove) == null)
					    {
						    if (!_save.getDebugMode())
						    {
							    _endTurnRequested = true;
							    statePushBack(null); // end AI turn
						    }
						    else
						    {
							    _save.selectNextPlayerUnit();
							    _debugPlay = true;
						    }
					    }
				    }
			    }
		    }
		    else
		    {
			    // it's a player side && we have not handled all panicking units
			    if (!_playerPanicHandled)
			    {
				    _playerPanicHandled = handlePanickingPlayer();
				    _save.getBattleState().updateSoldierInfo();
			    }
		    }
	    }
    }

    /**
     * Picks the first soldier that is panicking.
     * @return True when all panicking is over.
     */
    bool handlePanickingPlayer()
    {
	    foreach (var j in _save.getUnits())
	    {
		    if (j.getFaction() == UnitFaction.FACTION_PLAYER && j.getOriginalFaction() == UnitFaction.FACTION_PLAYER && handlePanickingUnit(j))
			    return false;
	    }
	    return true;
    }

    /**
     * Common function for handling panicking units.
     * @return False when unit not in panicking mode.
     */
    bool handlePanickingUnit(BattleUnit unit)
    {
	    UnitStatus status = unit.getStatus();
	    if (status != UnitStatus.STATUS_PANICKING && status != UnitStatus.STATUS_BERSERK) return false;
	    _save.setSelectedUnit(unit);
	    _parentState.getMap().setCursorType(CursorType.CT_NONE);

	    // show a little infobox with the name of the unit and "... is panicking"
	    Game game = _parentState.getGame();
	    if (unit.getVisible() || !Options.noAlienPanicMessages)
	    {
		    getMap().getCamera().centerOnPosition(unit.getPosition());
		    if (status == UnitStatus.STATUS_PANICKING)
		    {
			    game.pushState(new InfoboxState(game.getLanguage().getString("STR_HAS_PANICKED", (uint)unit.getGender()).arg(unit.getName(game.getLanguage()))));
		    }
		    else
		    {
			    game.pushState(new InfoboxState(game.getLanguage().getString("STR_HAS_GONE_BERSERK", (uint)unit.getGender()).arg(unit.getName(game.getLanguage()))));
		    }
	    }

	    int flee = RNG.generate(0,100);
	    BattleAction ba = default;
	    ba.actor = unit;
	    if (status == UnitStatus.STATUS_PANICKING && flee <= 50) // 1/2 chance to freeze and 1/2 chance try to flee, STATUS_BERSERK is handled in the panic state.
	    {
		    BattleItem item = unit.getItem("STR_RIGHT_HAND");
		    if (item != null)
		    {
			    dropItem(unit.getPosition(), item, false, true);
		    }
		    item = unit.getItem("STR_LEFT_HAND");
		    if (item != null)
		    {
			    dropItem(unit.getPosition(), item, false, true);
		    }
		    unit.setCache(null);
		    // let's try a few times to get a tile to run to.
		    for (int i= 0; i < 20; i++)
		    {
			    ba.target = new Position(unit.getPosition().x + RNG.generate(-5,5), unit.getPosition().y + RNG.generate(-5,5), unit.getPosition().z);

			    if (i >= 10 && ba.target.z > 0) // if we've had more than our fair share of failures, try going down.
			    {
				    ba.target.z--;
				    if (i >= 15 && ba.target.z > 0) // still failing? try further down.
				    {
					    ba.target.z--;
				    }
			    }
			    if (_save.getTile(ba.target) != null) // sanity check the tile.
			    {
				    _save.getPathfinding().calculate(ba.actor, ba.target);
				    if (_save.getPathfinding().getStartDirection() != -1) // sanity check the path.
				    {
					    statePushBack(new UnitWalkBState(this, ba));
					    break;
				    }
			    }
		    }
	    }
	    // Time units can only be reset after everything else occurs
	    statePushBack(new UnitPanicBState(this, ba.actor));
	    unit.moraleChange(+15);

	    return true;
    }

    internal bool convertInfected()
    {
	    bool retVal = false;
        var units = _save.getUnits();
	    for (var i = 0; i < units.Count; i++)
	    {
		    if (units[i].getHealth() > 0 && units[i].getHealth() >= units[i].getStunlevel() && units[i].getRespawn())
		    {
			    retVal = true;
			    units[i].setRespawn(false);
			    if (Options.battleNotifyDeath && units[i].getFaction() == UnitFaction.FACTION_PLAYER)
			    {
				    Game game = _parentState.getGame();
				    game.pushState(new InfoboxState(game.getLanguage().getString("STR_HAS_BEEN_KILLED", (uint)units[i].getGender()).arg(units[i].getName(game.getLanguage()))));
			    }

			    convertUnit(units[i]);
			    i = 0;
		    }
	    }
	    return retVal;
    }

    /**
     * Handles the result of non target actions, like priming a grenade.
     */
    internal void handleNonTargetAction()
    {
	    if (!_currentAction.targeting)
	    {
		    _currentAction.cameraPosition = new Position(0,0,-1);
		    if (!string.IsNullOrEmpty(_currentAction.result))
		    {
			    _parentState.warning(_currentAction.result);
			    _currentAction.result = string.Empty;
		    }
		    else if (_currentAction.type == BattleActionType.BA_PRIME && _currentAction.value > -1)
		    {
			    if (_currentAction.actor.spendTimeUnits(_currentAction.TU))
			    {
				    _parentState.warning("STR_GRENADE_IS_ACTIVATED");
				    _currentAction.weapon.setFuseTimer(_currentAction.value);
			    }
			    else
			    {
				    _parentState.warning("STR_NOT_ENOUGH_TIME_UNITS");
			    }
		    }
		    else if (_currentAction.type == BattleActionType.BA_USE)
		    {
			    _save.reviveUnconsciousUnits();
		    }
		    else if (_currentAction.type == BattleActionType.BA_HIT)
		    {
			    if (_currentAction.actor.spendTimeUnits(_currentAction.TU))
			    {
				    statePushBack(new MeleeAttackBState(this, _currentAction));
			    }
			    else
			    {
				    _parentState.warning("STR_NOT_ENOUGH_TIME_UNITS");
			    }
		    }
		    if (_currentAction.type != BattleActionType.BA_HIT) // don't clear the action type if we're meleeing, let the melee action state take care of that
		    {
			    _currentAction.type = BattleActionType.BA_NONE;
		    }
		    _parentState.updateSoldierInfo();
	    }

	    setupCursor();
    }

    /**
     * Converts a unit into a unit of another type.
     * @param unit The unit to convert.
     * @return Pointer to the new unit.
     */
    internal BattleUnit convertUnit(BattleUnit unit)
    {
	    getSave().getBattleState().showPsiButton(false);
	    BattleUnit newUnit = getSave().convertUnit(unit, _parentState.getGame().getSavedGame(), getMod());
	    getMap().cacheUnit(newUnit);
	    return newUnit;
    }

    /**
     * Handles the processing of the AI states of a unit.
     * @param unit Pointer to a unit.
     */
    void handleAI(BattleUnit unit)
    {
	    var ss = new StringBuilder();

	    if (unit.getTimeUnits() <= 5)
	    {
		    unit.dontReselect();
	    }
	    if (_AIActionCounter >= 2 || !unit.reselectAllowed())
	    {
		    if (_save.selectNextPlayerUnit(true, _AISecondMove) == null)
		    {
			    if (!_save.getDebugMode())
			    {
				    _endTurnRequested = true;
				    statePushBack(null); // end AI turn
			    }
			    else
			    {
				    _save.selectNextPlayerUnit();
				    _debugPlay = true;
			    }
		    }
		    if (_save.getSelectedUnit() != null)
		    {
			    _parentState.updateSoldierInfo();
			    getMap().getCamera().centerOnPosition(_save.getSelectedUnit().getPosition());
			    if (_save.getSelectedUnit().getId() <= unit.getId())
			    {
				    _AISecondMove = true;
			    }
		    }
		    _AIActionCounter = 0;
		    return;
	    }

	    unit.setVisible(false);

	    _save.getTileEngine().calculateFOV(unit.getPosition()); // might need this populate _visibleUnit for a newly-created alien
		    // it might also help chryssalids realize they've zombified someone and need to move on
		    // it should also hide units when they've killed the guy spotting them
		    // it's also for good luck

	    AIModule ai = unit.getAIModule();
	    if (ai == null)
	    {
		    // for some reason the unit had no AI routine assigned..
		    unit.setAIModule(new AIModule(_save, unit, null));
		    ai = unit.getAIModule();
	    }
	    _AIActionCounter++;
	    if (_AIActionCounter == 1)
	    {
		    _playedAggroSound = false;
		    unit.setHiding(false);
            if (Options.traceAI) { Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} #{unit.getId()}--{unit.getType()}"); }
        }

	    var action = new BattleAction();
	    action.actor = unit;
	    action.number = _AIActionCounter;
	    unit.think(ref action);

	    if (action.type == BattleActionType.BA_RETHINK)
	    {
		    _parentState.debug("Rethink");
		    unit.think(ref action);
	    }

	    _AIActionCounter = action.number;
	    BattleItem weapon = unit.getMainHandWeapon();
	    if (weapon == null || weapon.getAmmoItem() == null)
	    {
		    if (unit.getOriginalFaction() == UnitFaction.FACTION_HOSTILE && !unit.getVisibleUnits().Any())
		    {
			    findItem(ref action);
		    }
	    }

	    if (unit.getCharging() != null)
	    {
		    if (unit.getAggroSound() != -1 && !_playedAggroSound)
		    {
			    getMod().getSoundByDepth((uint)_save.getDepth(), (uint)unit.getAggroSound()).play(-1, getMap().getSoundAngle(unit.getPosition()));
			    _playedAggroSound = true;
		    }
	    }
	    if (action.type == BattleActionType.BA_WALK)
	    {
		    ss.Append($"Walking to {action.target}");
		    _parentState.debug(ss.ToString());

		    if (_save.getTile(action.target) != null)
		    {
			    _save.getPathfinding().calculate(action.actor, action.target);//, _save.getTile(action.target).getUnit());
		    }
		    if (_save.getPathfinding().getStartDirection() != -1)
		    {
			    statePushBack(new UnitWalkBState(this, action));
		    }
	    }

	    if (action.type == BattleActionType.BA_SNAPSHOT || action.type == BattleActionType.BA_AUTOSHOT || action.type == BattleActionType.BA_AIMEDSHOT || action.type == BattleActionType.BA_THROW || action.type == BattleActionType.BA_HIT || action.type == BattleActionType.BA_MINDCONTROL || action.type == BattleActionType.BA_PANIC || action.type == BattleActionType.BA_LAUNCH)
	    {
		    ss.Clear();
		    ss.Append($"Attack type={action.type} target={action.target} weapon={action.weapon.getRules().getName()}");
		    _parentState.debug(ss.ToString());
		    action.TU = unit.getActionTUs(action.type, action.weapon);
		    if (action.type == BattleActionType.BA_MINDCONTROL || action.type == BattleActionType.BA_PANIC)
		    {
			    statePushBack(new PsiAttackBState(this, action));
		    }
		    else
		    {
			    statePushBack(new UnitTurnBState(this, action));
			    if (action.type == BattleActionType.BA_HIT)
			    {
				    action.weapon = unit.getMeleeWeapon();
				    statePushBack(new MeleeAttackBState(this, action));
			    }
			    else
			    {
				    statePushBack(new ProjectileFlyBState(this, action));
			    }
		    }
	    }

	    if (action.type == BattleActionType.BA_NONE)
	    {
		    _parentState.debug("Idle");
		    _AIActionCounter = 0;
		    if (_save.selectNextPlayerUnit(true, _AISecondMove) == null)
		    {
			    if (!_save.getDebugMode())
			    {
				    _endTurnRequested = true;
				    statePushBack(null); // end AI turn
			    }
			    else
			    {
				    _save.selectNextPlayerUnit();
				    _debugPlay = true;
			    }
		    }
		    if (_save.getSelectedUnit() != null)
		    {
			    _parentState.updateSoldierInfo();
			    getMap().getCamera().centerOnPosition(_save.getSelectedUnit().getPosition());
			    if (_save.getSelectedUnit().getId() <= unit.getId())
			    {
				    _AISecondMove = true;
			    }
		    }
	    }
    }

    /**
     * Tries to find an item and pick it up if possible.
     */
    void findItem(ref BattleAction action)
    {
	    // terrorists don't have hands.
	    if (action.actor.getRankString() != "STR_LIVE_TERRORIST")
	    {
		    // pick the best available item
		    BattleItem targetItem = surveyItems(action);
		    // make sure it's worth taking
		    if (targetItem != null && worthTaking(targetItem, action))
		    {
			    // if we're already standing on it...
			    if (targetItem.getTile().getPosition() == action.actor.getPosition())
			    {
				    // try to pick it up
				    if (takeItemFromGround(targetItem, action) == 0)
				    {
					    // if it isn't loaded or it is ammo
					    if (targetItem.getAmmoItem() == null)
					    {
						    // try to load our weapon
						    action.actor.checkAmmo();
					    }
				    }
			    }
			    else if (targetItem.getTile().getUnit() == null || targetItem.getTile().getUnit().isOut())
			    {
				    // if we're not standing on it, we should try to get to it.
				    action.target = targetItem.getTile().getPosition();
				    action.type = BattleActionType.BA_WALK;
			    }
		    }
	    }
    }

    /**
     * Picks the item up from the ground.
     *
     * At this point we've decided it's worth our while to grab this item, so we try to do just that.
     * First we check to make sure we have time units, then that we have space (using horrifying logic)
     * then we attempt to actually recover the item.
     * @param item The item to attempt to take.
     * @param action A pointer to the action being performed.
     * @return 0 if successful, 1 for no TUs, 2 for not enough room, 3 for "won't fit" and -1 for "something went horribly wrong".
     */
    int takeItemFromGround(BattleItem item, BattleAction action)
    {
	    const int success = 0;
	    const int notEnoughTimeUnits = 1;
	    const int notEnoughSpace = 2;
	    const int couldNotFit = 3;
	    int freeSlots = 25;

	    // make sure we have time units
	    if (action.actor.getTimeUnits() < 6)
	    {
		    return notEnoughTimeUnits;
	    }
	    else
	    {
		    // check to make sure we have enough space by checking all the sizes of items in our inventory
		    foreach (var i in action.actor.getInventory())
		    {
			    freeSlots -= i.getRules().getInventoryHeight() * i.getRules().getInventoryWidth();
		    }
		    if (freeSlots < item.getRules().getInventoryHeight() * item.getRules().getInventoryWidth())
		    {
			    return notEnoughSpace;
		    }
		    else
		    {
			    // check that the item will fit in our inventory, and if so, take it
			    if (takeItem(item, action))
			    {
				    action.actor.spendTimeUnits(6);
				    item.getTile().removeItem(item);
				    return success;
			    }
			    else
			    {
				    return couldNotFit;
			    }
		    }
	    }
    }

    /**
     * Tries to fit an item into the unit's inventory, return false if you can't.
     * @param item The item to attempt to take.
     * @param action A pointer to the action being performed.
     * @return Whether or not the item was successfully retrieved.
     */
    bool takeItem(BattleItem item, BattleAction action)
    {
	    bool placed = false;
	    Mod.Mod mod = _parentState.getGame().getMod();
	    switch (item.getRules().getBattleType())
	    {
	        case BattleType.BT_AMMO:
		        // find equipped weapons that can be loaded with this ammo
		        if (action.actor.getItem("STR_RIGHT_HAND") != null && action.actor.getItem("STR_RIGHT_HAND").getAmmoItem() == null)
		        {
			        if (action.actor.getItem("STR_RIGHT_HAND").setAmmoItem(item) == 0)
			        {
				        placed = true;
			        }
		        }
		        else
		        {
			        for (int i = 0; i != 4; ++i)
			        {
				        if (action.actor.getItem("STR_BELT", i) == null)
				        {
					        item.moveToOwner(action.actor);
					        item.setSlot(mod.getInventory("STR_BELT", true));
					        item.setSlotX(i);
					        placed = true;
					        break;
				        }
			        }
		        }
		        break;
	        case BattleType.BT_GRENADE:
	        case BattleType.BT_PROXIMITYGRENADE:
		        for (int i = 0; i != 4; ++i)
		        {
			        if (action.actor.getItem("STR_BELT", i) == null)
			        {
				        item.moveToOwner(action.actor);
				        item.setSlot(mod.getInventory("STR_BELT", true));
				        item.setSlotX(i);
				        placed = true;
				        break;
			        }
		        }
		        break;
	        case BattleType.BT_FIREARM:
	        case BattleType.BT_MELEE:
		        if (action.actor.getItem("STR_RIGHT_HAND") == null)
		        {
			        item.moveToOwner(action.actor);
			        item.setSlot(mod.getInventory("STR_RIGHT_HAND", true));
			        placed = true;
		        }
		        break;
	        case BattleType.BT_MEDIKIT:
	        case BattleType.BT_SCANNER:
		        if (action.actor.getItem("STR_BACK_PACK") == null)
		        {
			        item.moveToOwner(action.actor);
			        item.setSlot(mod.getInventory("STR_BACK_PACK", true));
			        placed = true;
		        }
		        break;
	        case BattleType.BT_MINDPROBE:
		        if (action.actor.getItem("STR_LEFT_HAND") == null)
		        {
			        item.moveToOwner(action.actor);
			        item.setSlot(mod.getInventory("STR_LEFT_HAND", true));
			        placed = true;
		        }
		        break;
	        default: break;
	    }
	    return placed;
    }

    /**
     * Assesses whether this item is worth trying to pick up, taking into account how many units we see,
     * whether or not the Weapon has ammo, and if we have ammo FOR it,
     * or, if it's ammo, checks if we have the weapon to go with it,
     * assesses the attraction value of the item and compares it with the distance to the object,
     * then returns false anyway.
     * @param item The item to attempt to take.
     * @param action A pointer to the action being performed.
     * @return false.
     */
    bool worthTaking(BattleItem item, BattleAction action)
    {
	    int worthToTake = 0;
        var inventory = action.actor.getInventory();
        var compatibleAmmo = item.getRules().getCompatibleAmmo();

	    // don't even think about making a move for that gun if you can see a target, for some reason
	    // (maybe this should check for enemies spotting the tile the item is on?)
	    if (!action.actor.getVisibleUnits().Any())
	    {
		    // retrieve an insignificantly low value from the ruleset.
		    worthToTake = item.getRules().getAttraction();

		    // it's always going to be worth while to try and take a blaster launcher, apparently
		    if (item.getRules().getWaypoints() == 0 && item.getRules().getBattleType() != BattleType.BT_AMMO)
		    {
			    // we only want weapons that HAVE ammo, or weapons that we have ammo FOR
			    bool ammoFound = true;
			    if (item.getAmmoItem() == null)
			    {
				    ammoFound = false;
				    for (var i = 0; i < inventory.Count && !ammoFound; ++i)
				    {
					    if (inventory[i].getRules().getBattleType() == BattleType.BT_AMMO)
					    {
						    for (var j = 0; j < compatibleAmmo.Count && !ammoFound; ++j)
						    {
							    if (inventory[i].getRules().getName() == compatibleAmmo[j])
							    {
								    ammoFound = true;
								    break;
							    }
						    }
					    }
				    }
			    }
			    if (!ammoFound)
			    {
				    return false;
			    }
		    }

		    if (item.getRules().getBattleType() == BattleType.BT_AMMO)
		    {
			    // similar to the above, but this time we're checking if the ammo is suitable for a weapon we have.
			    bool weaponFound = false;
			    for (var i = 0; i < inventory.Count && !weaponFound; ++i)
			    {
				    if (inventory[i].getRules().getBattleType() == BattleType.BT_FIREARM)
				    {
                        var ammo = inventory[i].getRules().getCompatibleAmmo();
					    for (var j = 0; j < ammo.Count && !weaponFound; ++j)
					    {
						    if (inventory[i].getRules().getName() == ammo[j])
						    {
							    weaponFound = true;
							    break;
						    }
					    }
				    }
			    }
			    if (!weaponFound)
			    {
				    return false;
			    }
		    }
	    }

	    if (worthToTake != 0)
	    {
		    // use bad logic to determine if we'll have room for the item
		    int freeSlots = 25;
		    foreach (var i in inventory)
		    {
			    freeSlots -= i.getRules().getInventoryHeight() * i.getRules().getInventoryWidth();
		    }
		    int size = item.getRules().getInventoryHeight() * item.getRules().getInventoryWidth();
		    if (freeSlots < size)
		    {
			    return false;
		    }
	    }

	    // return false for any item that we aren't standing directly on top of with an attraction value less than 6 (aka always)
	    return (worthToTake - (_save.getTileEngine().distance(action.actor.getPosition(), item.getTile().getPosition())*2)) > 5;
    }

    /**
     * Searches through items on the map that were dropped on an alien turn, then picks the most "attractive" one.
     * @param action A pointer to the action being performed.
     * @return The item to attempt to take.
     */
    BattleItem surveyItems(BattleAction action)
    {
	    var droppedItems = new List<BattleItem>();

	    // first fill a vector with items on the ground that were dropped on the alien turn, and have an attraction value.
	    foreach (var i in _save.getItems())
	    {
		    if (i.getSlot() != null && i.getSlot().getId() == "STR_GROUND" && i.getTile() != null && i.getTurnFlag() && i.getRules().getAttraction() != 0)
		    {
			    droppedItems.Add(i);
		    }
	    }

	    BattleItem targetItem = null;
	    int maxWorth = 0;

	    // now select the most suitable candidate depending on attractiveness and distance
	    // (are we still talking about items?)
	    foreach (var i in droppedItems)
	    {
		    int currentWorth = i.getRules().getAttraction() / ((_save.getTileEngine().distance(action.actor.getPosition(), i.getTile().getPosition()) * 2)+1);
		    if (currentWorth > maxWorth)
		    {
			    maxWorth = currentWorth;
			    targetItem = i;
		    }
	    }

	    return targetItem;
    }

    /**
     * Checks if a unit has moved next to a proximity grenade.
     * Checks one tile around the unit in every direction.
     * For a large unit we check every tile it occupies.
     * @param unit Pointer to a unit.
     * @return True if a proximity grenade was triggered.
     */
    internal bool checkForProximityGrenades(BattleUnit unit)
    {
	    int size = unit.getArmor().getSize() - 1;
	    for (int x = size; x >= 0; x--)
	    {
		    for (int y = size; y >= 0; y--)
		    {
			    for (int tx = -1; tx < 2; tx++)
			    {
				    for (int ty = -1; ty < 2; ty++)
				    {
					    Tile t = _save.getTile(unit.getPosition() + new Position(x,y,0) + new Position(tx,ty,0));
					    if (t != null)
					    {
						    foreach (var i in t.getInventory())
						    {
							    if (i.getRules().getBattleType() == BattleType.BT_PROXIMITYGRENADE && i.getFuseTimer() == 0)
							    {
								    var p = new Position();
								    p.x = t.getPosition().x*16 + 8;
								    p.y = t.getPosition().y*16 + 8;
								    p.z = t.getPosition().z*24 + t.getTerrainLevel();
								    statePushNext(new ExplosionBState(this, p, i, i.getPreviousOwner()));
								    getSave().removeItem(i);
								    unit.setCache(null);
								    getMap().cacheUnit(unit);
								    return true;
							    }
						    }
					    }
				    }
			    }
		    }
	    }
	    return false;
    }

    /**
     * Toggles the Kneel/Standup status of the unit.
     * @param bu Pointer to a unit.
     * @return If the action succeeded.
     */
    internal bool kneel(BattleUnit bu)
    {
	    int tu = bu.isKneeled()?8:4;
	    if (bu.getType() == "SOLDIER" && !bu.isFloating() && ((!bu.isKneeled() && _save.getKneelReserved()) || checkReservedTU(bu, tu)))
	    {
		    if (bu.spendTimeUnits(tu))
		    {
			    bu.kneel(!bu.isKneeled());
			    // kneeling or standing up can reveal new terrain or units. I guess.
			    getTileEngine().calculateFOV(bu);
			    getMap().cacheUnits();
			    _parentState.updateSoldierInfo();
			    getTileEngine().checkReactionFire(bu);
			    return true;
		    }
		    else
		    {
			    _parentState.warning("STR_NOT_ENOUGH_TIME_UNITS");
		    }
	    }
	    return false;
    }

    /**
     * Gives time slice to the front state.
     */
    internal void handleState()
    {
	    if (_states.Any())
	    {
		    // end turn request?
		    if (_states.First() == null)
		    {
			    _states.RemoveAt(0);
			    endTurn();
			    return;
		    }
		    else
		    {
			    _states.First().think();
		    }
		    getMap().invalidate(); // redraw map
	    }
    }

    /**
     * Activates primary action (left click).
     * @param pos Position on the map.
     */
    internal void primaryAction(Position pos)
    {
	    bool bPreviewed = Options.battleNewPreviewPath != PathPreview.PATH_NONE;

	    getMap().resetObstacles();

	    if (_currentAction.targeting && _save.getSelectedUnit() != null)
	    {
		    if (_currentAction.type == BattleActionType.BA_LAUNCH)
		    {
			    int maxWaypoints = _currentAction.weapon.getRules().getWaypoints();
			    if (maxWaypoints == 0)
			    {
				    maxWaypoints = _currentAction.weapon.getAmmoItem().getRules().getWaypoints();
			    }
			    if ((int)_currentAction.waypoints.Count < maxWaypoints || maxWaypoints == -1)
			    {
				    _parentState.showLaunchButton(true);
				    _currentAction.waypoints.Add(pos);
				    getMap().getWaypoints().Add(pos);
			    }
		    }
		    else if (_currentAction.type == BattleActionType.BA_USE && _currentAction.weapon.getRules().getBattleType() == BattleType.BT_MINDPROBE)
		    {
			    if (_save.selectUnit(pos) != null && _save.selectUnit(pos).getFaction() != _save.getSelectedUnit().getFaction() && _save.selectUnit(pos).getVisible())
			    {
				    if (!_currentAction.weapon.getRules().isLOSRequired() ||
					    _currentAction.actor.getVisibleUnits().Contains(_save.selectUnit(pos)))
				    {
					    if (_currentAction.actor.spendTimeUnits(_currentAction.TU))
					    {
						    _parentState.getGame().getMod().getSoundByDepth((uint)_save.getDepth(), (uint)_currentAction.weapon.getRules().getHitSound()).play(-1, getMap().getSoundAngle(pos));
						    _parentState.getGame().pushState(new UnitInfoState(_save.selectUnit(pos), _parentState, false, true));
						    cancelCurrentAction();
					    }
					    else
					    {
						    _parentState.warning("STR_NOT_ENOUGH_TIME_UNITS");
					    }
				    }
				    else
				    {
					    _parentState.warning("STR_NO_LINE_OF_FIRE");
				    }
			    }
		    }
		    else if (_currentAction.type == BattleActionType.BA_PANIC || _currentAction.type == BattleActionType.BA_MINDCONTROL)
		    {
			    if (_save.selectUnit(pos) != null && _save.selectUnit(pos).getFaction() != _save.getSelectedUnit().getFaction() && _save.selectUnit(pos).getVisible())
			    {
				    _currentAction.TU = _currentAction.actor.getActionTUs(_currentAction.type, _currentAction.weapon);
				    _currentAction.target = pos;
				    if (!_currentAction.weapon.getRules().isLOSRequired() ||
					    _currentAction.actor.getVisibleUnits().Contains(_save.selectUnit(pos)))
				    {
					    // get the sound/animation started
					    getMap().setCursorType(CursorType.CT_NONE);
					    _parentState.getGame().getCursor().setVisible(false);
					    _currentAction.cameraPosition = getMap().getCamera().getMapOffset();
					    statePushBack(new PsiAttackBState(this, _currentAction));
				    }
				    else
				    {
					    _parentState.warning("STR_NO_LINE_OF_FIRE");
				    }
			    }
		    }
		    else if (Options.battleConfirmFireMode && (!_currentAction.waypoints.Any() || pos != _currentAction.waypoints.First()))
		    {
			    _currentAction.waypoints.Clear();
			    _currentAction.waypoints.Add(pos);
			    getMap().getWaypoints().Clear();
			    getMap().getWaypoints().Add(pos);
		    }
		    else
		    {
			    _currentAction.target = pos;
			    getMap().setCursorType(CursorType.CT_NONE);

			    if (Options.battleConfirmFireMode)
			    {
				    _currentAction.waypoints.Clear();
				    getMap().getWaypoints().Clear();
			    }

			    _parentState.getGame().getCursor().setVisible(false);
			    _currentAction.cameraPosition = getMap().getCamera().getMapOffset();
			    _states.Add(new ProjectileFlyBState(this, _currentAction));
			    statePushFront(new UnitTurnBState(this, _currentAction)); // first of all turn towards the target
		    }
	    }
	    else
	    {
		    _currentAction.actor = _save.getSelectedUnit();
		    BattleUnit unit = _save.selectUnit(pos);
		    if (unit != null && unit != _save.getSelectedUnit() && (unit.getVisible() || _debugPlay))
		    {
		    //  -= select unit =-
			    if (unit.getFaction() == _save.getSide())
			    {
				    _save.setSelectedUnit(unit);
				    _parentState.updateSoldierInfo();
				    cancelCurrentAction();
				    setupCursor();
				    _currentAction.actor = unit;
			    }
		    }
		    else if (playableUnitSelected())
		    {
			    bool modifierPressed = (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0;
			    if (bPreviewed &&
				    (_currentAction.target != pos || (_save.getPathfinding().isModifierUsed() != modifierPressed)))
			    {
				    _save.getPathfinding().removePreview();
			    }
			    _currentAction.target = pos;
			    _save.getPathfinding().calculate(_currentAction.actor, _currentAction.target);
			    _currentAction.run = false;
			    _currentAction.strafe = Options.strafe && modifierPressed && _save.getSelectedUnit().getArmor().getSize() == 1;
			    if (_currentAction.strafe && _save.getPathfinding().getPath().Count > 1)
			    {
				    _currentAction.run = true;
				    _currentAction.strafe = false;
			    }
			    if (bPreviewed && !_save.getPathfinding().previewPath() && _save.getPathfinding().getStartDirection() != -1)
			    {
				    _save.getPathfinding().removePreview();
				    bPreviewed = false;
			    }

			    if (!bPreviewed && _save.getPathfinding().getStartDirection() != -1)
			    {
				    //  -= start walking =-
				    getMap().setCursorType(CursorType.CT_NONE);
				    _parentState.getGame().getCursor().setVisible(false);
				    statePushBack(new UnitWalkBState(this, _currentAction));
			    }
		    }
	    }
    }

    /**
     * Activates secondary action (right click).
     * @param pos Position on the map.
     */
    internal void secondaryAction(Position pos)
    {
	    //  -= turn to or open door =-
	    _currentAction.target = pos;
	    _currentAction.actor = _save.getSelectedUnit();
	    _currentAction.strafe = Options.strafe && (SDL_GetModState() & SDL_Keymod.KMOD_CTRL) != 0 && _save.getSelectedUnit().getTurretType() > -1;
	    statePushBack(new UnitTurnBState(this, _currentAction));
    }

    /**
     * Moves a unit up or down.
     * @param unit The unit.
     * @param dir Direction DIR_UP or DIR_DOWN.
     */
    internal void moveUpDown(BattleUnit unit, int dir)
    {
	    _currentAction.target = unit.getPosition();
	    if (dir == Pathfinding.DIR_UP)
	    {
		    _currentAction.target.z++;
	    }
	    else
	    {
		    _currentAction.target.z--;
	    }
	    getMap().setCursorType(CursorType.CT_NONE);
	    _parentState.getGame().getCursor().setVisible(false);
	    if (_save.getSelectedUnit().isKneeled())
	    {
		    kneel(_save.getSelectedUnit());
	    }
	    _save.getPathfinding().calculate(_currentAction.actor, _currentAction.target);
	    statePushBack(new UnitWalkBState(this, _currentAction));
    }

    /**
     * Gets the kneel reservation setting.
     * @return Kneel reservation setting.
     */
    internal bool getKneelReserved() =>
	    _save.getKneelReserved();

    /**
     * Handler for the blaster launcher button.
     */
    internal void launchAction()
    {
	    _parentState.showLaunchButton(false);
	    getMap().getWaypoints().Clear();
	    _currentAction.target = _currentAction.waypoints.First();
	    getMap().setCursorType(CursorType.CT_NONE);
	    _parentState.getGame().getCursor().setVisible(false);
	    _currentAction.cameraPosition = getMap().getCamera().getMapOffset();
	    _states.Add(new ProjectileFlyBState(this, _currentAction));
	    statePushFront(new UnitTurnBState(this, _currentAction)); // first of all turn towards the target
    }

    /**
     * Handler for the psi button.
     */
    internal void psiButtonAction()
    {
	    if (_currentAction.waypoints.Any()) // in case waypoints were set with a blaster launcher, avoid accidental misclick
		    return;
	    _currentAction.weapon = _save.getSelectedUnit().getSpecialWeapon(BattleType.BT_PSIAMP);
	    _currentAction.targeting = true;
	    _currentAction.type = BattleActionType.BA_PANIC;
	    _currentAction.TU = _currentAction.weapon.getRules().getTUUse();
	    if (!_currentAction.weapon.getRules().getFlatRate())
	    {
		    _currentAction.TU = (int)Math.Floor(_save.getSelectedUnit().getBaseStats().tu * _currentAction.TU / 100.0f);
	    }
	    setupCursor();
    }
}
