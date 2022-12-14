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
    Position target;
    internal List<Position> waypoints;
    internal int TU;
    internal bool targeting;
    int value;
    internal string result;
    bool strafe, run;
    int diff;
    int autoShotCounter;
    Position cameraPosition;
    bool desperate; // ignoring newly-spotted units
    int finalFacing;
    bool finalAction;
    int number; // first action of turn, second, etc.?

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
    static bool _debugPlay;

    /**
     * Initializes all the elements in the Battlescape screen.
     * @param save Pointer to the save game.
     * @param parentState Pointer to the parent battlescape state.
     */
    BattlescapeGame(SavedBattleGame save, BattlescapeState parentState)
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
    void cleanupDeleted() =>
        _deleted.Clear();

    /**
      * Cancels the current action the user had selected (firing, throwing,..)
      * @param bForce Force the action to be cancelled.
      * @return Whether an action was cancelled or not.
      */
    bool cancelCurrentAction(bool bForce = false)
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
    void setupCursor()
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
    internal bool checkReservedTU(BattleUnit bu, int tu, bool justChecking)
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
    void checkForCasualties(BattleItem murderweapon, BattleUnit origMurderer, bool hiddenExplosion = false, bool terrainExplosion = false)
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
            _parentState.showPsiButton(bu != null && bu.getSpecialWeapon(Mod.BattleType.BT_PSIAMP) != null && !bu.isOut());
        }
    }

    /**
     * Pushes a state as the next state after the current one.
     * @param bs Battlestate.
     */
    void statePushNext(BattleState bs)
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
    TileEngine getTileEngine() =>
        _save.getTileEngine();

    /**
     * Gets the mod.
     * @return mod.
     */
    internal Mod.Mod getMod() =>
        _parentState.getGame().getMod();
}
