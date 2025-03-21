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

namespace SharpXcom.Savegame;

enum UnitStatus { STATUS_STANDING, STATUS_WALKING, STATUS_FLYING, STATUS_TURNING, STATUS_AIMING, STATUS_COLLAPSING, STATUS_DEAD, STATUS_UNCONSCIOUS, STATUS_PANICKING, STATUS_BERSERK, STATUS_IGNORE_ME };

enum UnitFaction { FACTION_PLAYER, FACTION_HOSTILE, FACTION_NEUTRAL };

enum UnitSide { SIDE_FRONT, SIDE_LEFT, SIDE_RIGHT, SIDE_REAR, SIDE_UNDER };

enum UnitBodyPart { BODYPART_HEAD, BODYPART_TORSO, BODYPART_RIGHTARM, BODYPART_LEFTARM, BODYPART_RIGHTLEG, BODYPART_LEFTLEG };

/**
 * Represents a moving unit in the battlescape, player controlled or AI controlled
 * it holds info about it's position, items carrying, stats, etc
 */
internal class BattleUnit
{
    const int SPEC_WEAPON_MAX = 3;
    internal const int MAX_SOLDIER_ID = 1000000;

    int[] _currentArmor = new int[5], _maxArmor = new int[5];
    Position _pos;
    int _tu, _energy, _health, _morale, _stunlevel;
    bool _kneeled, _floating, _dontReselect;
    UnitFaction _faction, _originalFaction;
    List<BattleItem> _inventory;
    Tile _tile;
    string _activeHand;
    AIModule _currentAIState;
    int _direction, _toDirection;
    List<BattleUnit> _visibleUnits, _unitsSpottedThisTurn;
    UnitStatus _status;
    bool _visible;
    int _murdererId;    // used to credit the murderer with the kills that this unit got by blowing up on death
    int _mindControllerID;	// used to credit the mind controller with the kills of the mind controllee
    int _id;
    BattleUnitStatistics _statistics;
    string _murdererWeapon, _murdererWeaponAmmo;
    UnitSide _fatalShotSide;
    UnitBodyPart _fatalShotBodyPart;
    int _kills;
    UnitFaction _killedBy;
    int _turnsSinceSpotted;
    BattleItem[] _specWeapon = new BattleItem[SPEC_WEAPON_MAX];
    List<Tile> _visibleTiles;
    int _directionTurret, _toDirectionTurret;
    int _walkPhase, _fallPhase;
    bool _cacheInvalid;
    Position _lastPos;
    int _verticalDirection;
    Position _destination;
    Surface[] _cache = new Surface[5];
    string _spawnUnit;
    int _fire;
    int _expBravery, _expReactions, _expFiring, _expThrowing, _expPsiSkill, _expPsiStrength, _expMelee;
    int _moraleRestored;
    int _motionPoints;
    int[] _fatalWounds = new int[6];
    bool _hitByFire, _hitByAnything;
    BattleUnit _charging;
    int _faceDirection; // used only during strafeing moves
    internal Position lastCover;

    // static data
    string _type;
    string _rank;
    string _race;
    string _name;
    UnitStats _stats;
    int _standHeight, _kneelHeight, _floatHeight;
    List<int> _deathSound;
    int _value, _aggroSound, _moveSound;
    int _intelligence, _aggression;
    SpecialAbility _specab;
    Armor _armor;
    SoldierGender _gender;
    Soldier _geoscapeSoldier;
    List<int> _loftempsSet;
    Unit _unitRules;
    int _rankInt;
    int _turretType;
    int _breathFrame;
    bool _breathing;
    bool _hidingForTurn, _floorAbove, _respawn;
    MovementType _movementType;
    List<KeyValuePair<byte, byte>> _recolor;
    bool _capturable;

    /**
     * Initializes a BattleUnit from a Soldier
     * @param soldier Pointer to the Soldier.
     * @param depth the depth of the battlefield (used to determine movement type in case of MT_FLOAT).
     */
    internal BattleUnit(Soldier soldier, int depth)
    {
        _faction = UnitFaction.FACTION_PLAYER;
        _originalFaction = UnitFaction.FACTION_PLAYER;
        _killedBy = UnitFaction.FACTION_PLAYER;
        _id = 0;
        _tile = null;
        _lastPos = new Position();
        _direction = 0;
        _toDirection = 0;
        _directionTurret = 0;
        _toDirectionTurret = 0;
        _verticalDirection = 0;
        _status = UnitStatus.STATUS_STANDING;
        _walkPhase = 0;
        _fallPhase = 0;
        _kneeled = false;
        _floating = false;
        _dontReselect = false;
        _fire = 0;
        _currentAIState = null;
        _visible = false;
        _cacheInvalid = true;
        _expBravery = 0;
        _expReactions = 0;
        _expFiring = 0;
        _expThrowing = 0;
        _expPsiSkill = 0;
        _expPsiStrength = 0;
        _expMelee = 0;
        _motionPoints = 0;
        _kills = 0;
        _hitByFire = false;
        _hitByAnything = false;
        _moraleRestored = 0;
        _charging = null;
        _turnsSinceSpotted = 255;
        _statistics = new BattleUnitStatistics();
        _murdererId = 0;
        _mindControllerID = 0;
        _fatalShotSide = UnitSide.SIDE_FRONT;
        _fatalShotBodyPart = UnitBodyPart.BODYPART_HEAD;
        _geoscapeSoldier = soldier;
        _unitRules = null;
        _rankInt = 0;
        _turretType = -1;
        _hidingForTurn = false;
        _respawn = false;
        _capturable = true;

        _name = soldier.getName(true);
        _id = soldier.getId();
        _type = "SOLDIER";
        _rank = soldier.getRankString();
        _stats = soldier.getCurrentStats();
        _standHeight = soldier.getRules().getStandHeight();
        _kneelHeight = soldier.getRules().getKneelHeight();
        _floatHeight = soldier.getRules().getFloatHeight();
        _deathSound = new List<int>(); // this one is hardcoded
        _aggroSound = -1;
        _moveSound = -1;  // this one is hardcoded
        _intelligence = 2;
        _aggression = 1;
        _specab = SpecialAbility.SPECAB_NONE;
        _armor = soldier.getArmor();
        _movementType = _armor.getMovementType();
        if (_movementType == MovementType.MT_FLOAT)
        {
            if (depth > 0)
            {
                _movementType = MovementType.MT_FLY;
            }
            else
            {
                _movementType = MovementType.MT_WALK;
            }
        }
        else if (_movementType == MovementType.MT_SINK)
        {
            if (depth == 0)
            {
                _movementType = MovementType.MT_FLY;
            }
            else
            {
                _movementType = MovementType.MT_WALK;
            }
        }
        _stats += _armor.getStats();  // armors may modify effective stats
        _loftempsSet = _armor.getLoftempsSet();
        _gender = soldier.getGender();
        _faceDirection = -1;
        _breathFrame = -1;
        if (_armor.drawBubbles())
        {
            _breathFrame = 0;
        }
        _floorAbove = false;
        _breathing = false;

        int rankbonus = 0;

        switch (soldier.getRank())
        {
            case SoldierRank.RANK_SERGEANT: rankbonus = 1; break;
            case SoldierRank.RANK_CAPTAIN: rankbonus = 3; break;
            case SoldierRank.RANK_COLONEL: rankbonus = 6; break;
            case SoldierRank.RANK_COMMANDER: rankbonus = 10; break;
            default: rankbonus = 0; break;
        }

        _value = soldier.getRules().getValue() + soldier.getMissions() + rankbonus;

        _tu = _stats.tu;
        _energy = _stats.stamina;
        _health = _stats.health;
        _morale = 100;
        _stunlevel = 0;
        _maxArmor[(int)UnitSide.SIDE_FRONT] = _armor.getFrontArmor();
        _maxArmor[(int)UnitSide.SIDE_LEFT] = _armor.getSideArmor();
        _maxArmor[(int)UnitSide.SIDE_RIGHT] = _armor.getSideArmor();
        _maxArmor[(int)UnitSide.SIDE_REAR] = _armor.getRearArmor();
        _maxArmor[(int)UnitSide.SIDE_UNDER] = _armor.getUnderArmor();
        _currentArmor[(int)UnitSide.SIDE_FRONT] = _maxArmor[(int)UnitSide.SIDE_FRONT];
        _currentArmor[(int)UnitSide.SIDE_LEFT] = _maxArmor[(int)UnitSide.SIDE_LEFT];
        _currentArmor[(int)UnitSide.SIDE_RIGHT] = _maxArmor[(int)UnitSide.SIDE_RIGHT];
        _currentArmor[(int)UnitSide.SIDE_REAR] = _maxArmor[(int)UnitSide.SIDE_REAR];
        _currentArmor[(int)UnitSide.SIDE_UNDER] = _maxArmor[(int)UnitSide.SIDE_UNDER];
        for (int i = 0; i < 6; ++i)
            _fatalWounds[i] = 0;
        for (int i = 0; i < 5; ++i)
            _cache[i] = null;
        for (int i = 0; i < SPEC_WEAPON_MAX; ++i)
            _specWeapon[i] = null;

        _activeHand = "STR_RIGHT_HAND";

        lastCover = new Position(-1, -1, -1);

        _statistics = new BattleUnitStatistics();

        deriveRank();

        int look = (int)soldier.getGender() + 2 * (int)soldier.getLook();
        setRecolor(look, look, _rankInt);
    }

    /**
     * Initializes a BattleUnit from a Unit (non-player) object.
     * @param unit Pointer to Unit object.
     * @param faction Which faction the units belongs to.
     * @param id Unique unit ID.
     * @param armor Pointer to unit Armor.
     * @param diff difficulty level (for stat adjustment).
     * @param depth the depth of the battlefield (used to determine movement type in case of MT_FLOAT).
     */
    internal BattleUnit(Unit unit, UnitFaction faction, int id, Armor armor, StatAdjustment adjustment, int depth)
    {
        _faction = faction;
        _originalFaction = faction;
        _killedBy = faction;
        _id = id;
        _tile = null;
        _lastPos = new Position();
        _direction = 0;
        _toDirection = 0;
        _directionTurret = 0;
        _toDirectionTurret = 0;
        _verticalDirection = 0;
        _status = UnitStatus.STATUS_STANDING;
        _walkPhase = 0;
        _fallPhase = 0;
        _kneeled = false;
        _floating = false;
        _dontReselect = false;
        _fire = 0;
        _currentAIState = null;
        _visible = false;
        _cacheInvalid = true;
        _expBravery = 0;
        _expReactions = 0;
        _expFiring = 0;
        _expThrowing = 0;
        _expPsiSkill = 0;
        _expPsiStrength = 0;
        _expMelee = 0;
        _motionPoints = 0;
        _kills = 0;
        _hitByFire = false;
        _hitByAnything = false;
        _moraleRestored = 0;
        _charging = null;
        _turnsSinceSpotted = 255;
        _statistics = new BattleUnitStatistics();
        _murdererId = 0;
        _mindControllerID = 0;
        _fatalShotSide = UnitSide.SIDE_FRONT;
        _fatalShotBodyPart = UnitBodyPart.BODYPART_HEAD;
        _armor = armor;
        _geoscapeSoldier = null;
        _unitRules = unit;
        _rankInt = 0;
        _turretType = -1;
        _hidingForTurn = false;
        _respawn = false;

        _type = unit.getType();
        _rank = unit.getRank();
        _race = unit.getRace();
        _stats = unit.getStats();
        _standHeight = unit.getStandHeight();
        _kneelHeight = unit.getKneelHeight();
        _floatHeight = unit.getFloatHeight();
        _loftempsSet = _armor.getLoftempsSet();
        _deathSound = unit.getDeathSounds();
        _aggroSound = unit.getAggroSound();
        _moveSound = unit.getMoveSound();
        _intelligence = unit.getIntelligence();
        _aggression = unit.getAggression();
        _specab = (SpecialAbility)unit.getSpecialAbility();
        _spawnUnit = unit.getSpawnUnit();
        _value = unit.getValue();
        _faceDirection = -1;
        _capturable = unit.getCapturable();

        _movementType = _armor.getMovementType();
        if (_movementType == MovementType.MT_FLOAT)
        {
            if (depth > 0)
            {
                _movementType = MovementType.MT_FLY;
            }
            else
            {
                _movementType = MovementType.MT_WALK;
            }
        }
        else if (_movementType == MovementType.MT_SINK)
        {
            if (depth == 0)
            {
                _movementType = MovementType.MT_FLY;
            }
            else
            {
                _movementType = MovementType.MT_WALK;
            }
        }

        _stats += _armor.getStats();  // armors may modify effective stats

        _breathFrame = -1; // most aliens don't breathe per-se, that's exclusive to humanoids
        if (armor.drawBubbles())
        {
            _breathFrame = 0;
        }
        _floorAbove = false;
        _breathing = false;

        _maxArmor[(int)UnitSide.SIDE_FRONT] = _armor.getFrontArmor();
        _maxArmor[(int)UnitSide.SIDE_LEFT] = _armor.getSideArmor();
        _maxArmor[(int)UnitSide.SIDE_RIGHT] = _armor.getSideArmor();
        _maxArmor[(int)UnitSide.SIDE_REAR] = _armor.getRearArmor();
        _maxArmor[(int)UnitSide.SIDE_UNDER] = _armor.getUnderArmor();

        if (faction == UnitFaction.FACTION_HOSTILE)
        {
            adjustStats(adjustment);
        }

        _tu = _stats.tu;
        _energy = _stats.stamina;
        _health = _stats.health;
        _morale = 100;
        _stunlevel = 0;
        _currentArmor[(int)UnitSide.SIDE_FRONT] = _maxArmor[(int)UnitSide.SIDE_FRONT];
        _currentArmor[(int)UnitSide.SIDE_LEFT] = _maxArmor[(int)UnitSide.SIDE_LEFT];
        _currentArmor[(int)UnitSide.SIDE_RIGHT] = _maxArmor[(int)UnitSide.SIDE_RIGHT];
        _currentArmor[(int)UnitSide.SIDE_REAR] = _maxArmor[(int)UnitSide.SIDE_REAR];
        _currentArmor[(int)UnitSide.SIDE_UNDER] = _maxArmor[(int)UnitSide.SIDE_UNDER];
        for (int i = 0; i < 6; ++i)
            _fatalWounds[i] = 0;
        for (int i = 0; i < 5; ++i)
            _cache[i] = null;
        for (int i = 0; i < SPEC_WEAPON_MAX; ++i)
            _specWeapon[i] = null;

        _activeHand = "STR_RIGHT_HAND";
        _gender = SoldierGender.GENDER_MALE;

        lastCover = new Position(-1, -1, -1);

        _statistics = new BattleUnitStatistics();

        var rand = new Random();
        int generalRank = 0;
        if (faction == UnitFaction.FACTION_HOSTILE)
        {
            const int max = 7;
            string[] rankList =
            {
                "STR_LIVE_SOLDIER",
                "STR_LIVE_ENGINEER",
                "STR_LIVE_MEDIC",
                "STR_LIVE_NAVIGATOR",
                "STR_LIVE_LEADER",
                "STR_LIVE_COMMANDER",
                "STR_LIVE_TERRORIST",
            };
            for (int i = 0; i < max; ++i)
            {
                if (_rank == rankList[i])
                {
                    generalRank = i;
                    break;
                }
            }
        }
        else if (faction == UnitFaction.FACTION_NEUTRAL)
        {
            generalRank = rand.Next() % 8;
        }

        setRecolor(rand.Next() % 8, rand.Next() % 8, generalRank);
    }

    /**
     *
     */
    ~BattleUnit()
    {
        for (int i = 0; i < 5; ++i)
            if (_cache[i] != null) _cache[i] = null;
        _statistics.kills.Clear();
        _statistics = default;
        _currentAIState = null;
    }

    /**
     * Get the armor value of a certain armor side.
     * @param side The side of the armor.
     * @return Amount of armor.
     */
    /// Get armor value.
    internal int getArmor(UnitSide side) =>
	    _currentArmor[(int)side];

    /**
     * Gets the unit's armor.
     * @return Pointer to armor.
     */
    internal Armor getArmor() =>
	    _armor;

    /**
     * Gets the BattleUnit's position.
     * @return position
     */
    internal Position getPosition() =>
	    _pos;

    /**
     * Returns the soldier's amount of time units.
     * @return Time units.
     */
    internal int getTimeUnits() =>
	    _tu;

    /**
     * Is kneeled down?
     * @return true/false
     */
    internal bool isKneeled() =>
	    _kneeled;

    /**
     * Returns the soldier's amount of energy.
     * @return Energy.
     */
    internal int getEnergy() =>
	    _energy;

    /**
     * Get the number of time units a certain action takes.
     * @param actionType
     * @param item
     * @return TUs
     */
    internal int getActionTUs(BattleActionType actionType, BattleItem item)
    {
        if (item == null)
        {
            return 0;
        }
        return getActionTUs(actionType, item.getRules());
    }

    internal int getActionTUs(BattleActionType actionType, RuleItem item)
    {
        if (item == null)
        {
            return 0;
        }

        int cost = 0;
        switch (actionType)
        {
            case BattleActionType.BA_PRIME:
                cost = 50; // maybe this should go in the ruleset
                break;
            case BattleActionType.BA_THROW:
                cost = 25;
                break;
            case BattleActionType.BA_AUTOSHOT:
                cost = item.getTUAuto();
                break;
            case BattleActionType.BA_SNAPSHOT:
                cost = item.getTUSnap();
                break;
            case BattleActionType.BA_HIT:
                cost = item.getTUMelee();
                break;
            case BattleActionType.BA_LAUNCH:
            case BattleActionType.BA_AIMEDSHOT:
                cost = item.getTUAimed();
                break;
            case BattleActionType.BA_USE:
            case BattleActionType.BA_MINDCONTROL:
            case BattleActionType.BA_PANIC:
                cost = item.getTUUse();
                break;
            default:
                cost = 0;
                break;
        }

        // if it's a percentage, apply it to unit TUs
        if (!item.getFlatRate() || actionType == BattleActionType.BA_THROW || actionType == BattleActionType.BA_PRIME)
        {
            cost = (int)Math.Floor(getBaseStats().tu * cost / 100.0f);
        }

        return cost;
    }

    /**
      * Gets pointer to the unit's stats.
      * @return stats Pointer to the unit's stats.
      */
    internal UnitStats getBaseStats() =>
        _stats;

    /**
     * Get unit type.
     * @return unit type.
     */
    internal string getType() =>
	    _type;

    /**
	 * Checks if there's an inventory item in
	 * the specified inventory position.
	 * @param slot Inventory slot.
	 * @param x X position in slot.
	 * @param y Y position in slot.
	 * @return Item in the slot, or NULL if none.
	 */
    internal BattleItem getItem(string slot, int x = 0, int y = 0)
	{
		// Soldier items
		if (slot != "STR_GROUND")
		{
			foreach (var item in _inventory)
			{
				if (item.getSlot() != null && item.getSlot().getId() == slot && item.occupiesSlot(x, y))
				{
					return item;
				}
			}
		}
		// Ground items
		else if (_tile != null)
		{
			foreach (var item in _tile.getInventory())
			{
				if (item.getSlot() != null && item.occupiesSlot(x, y))
				{
					return item;
				}
			}
		}
		return null;
	}

	/**
	 * Get unit's active hand.
	 * @return active hand.
	 */
	internal string getActiveHand()
	{
		if (getItem(_activeHand) != null) return _activeHand;
		if (getItem("STR_LEFT_HAND") != null) return "STR_LEFT_HAND";
		return "STR_RIGHT_HAND";
	}

    /**
     * Get the "main hand weapon" from the unit.
     * @param quickest Whether to get the quickest weapon, default true
     * @return Pointer to item.
     */
    internal BattleItem getMainHandWeapon(bool quickest = true)
    {
	    BattleItem weaponRightHand = getItem("STR_RIGHT_HAND");
        BattleItem weaponLeftHand = getItem("STR_LEFT_HAND");

	    // ignore weapons without ammo (rules out grenades)
	    if (weaponRightHand == null || weaponRightHand.getAmmoItem() == null || weaponRightHand.getAmmoItem().getAmmoQuantity() == 0)
		    weaponRightHand = null;
	    if (weaponLeftHand == null || weaponLeftHand.getAmmoItem() == null || weaponLeftHand.getAmmoItem().getAmmoQuantity() == 0)
		    weaponLeftHand = null;

	    // if there is only one weapon, it's easy:
	    if (weaponRightHand != null && weaponLeftHand == null)
		    return weaponRightHand;
	    else if (weaponRightHand == null && weaponLeftHand != null)
		    return weaponLeftHand;
	    else if (weaponRightHand == null && weaponLeftHand == null)
		    return null;

	    // otherwise pick the one with the least snapshot TUs
	    int tuRightHand = weaponRightHand.getRules().getTUSnap();
	    int tuLeftHand = weaponLeftHand.getRules().getTUSnap();
	    BattleItem weaponCurrentHand = getItem(getActiveHand());
	    //prioritize blasters
	    if (!quickest && _faction != UnitFaction.FACTION_PLAYER)
	    {
		    if (weaponRightHand.getRules().getWaypoints() != 0 || weaponRightHand.getAmmoItem().getRules().getWaypoints() != 0)
		    {
			    return weaponRightHand;
		    }
		    if (weaponLeftHand.getRules().getWaypoints() != 0 || weaponLeftHand.getAmmoItem().getRules().getWaypoints() != 0)
		    {
			    return weaponLeftHand;
		    }
	    }
	    // if only one weapon has snapshot, pick that one
	    if (tuLeftHand <= 0 && tuRightHand > 0)
		    return weaponRightHand;
	    else if (tuRightHand <= 0 && tuLeftHand > 0)
		    return weaponLeftHand;
	    // else pick the better one
	    else
	    {
		    if (tuLeftHand >= tuRightHand)
		    {
			    if (quickest)
			    {
				    return weaponRightHand;
			    }
			    else if (_faction == UnitFaction.FACTION_PLAYER)
			    {
				    return weaponCurrentHand;
			    }
			    else
			    {
				    return weaponLeftHand;
			    }
		    }
		    else
		    {
			    if (quickest)
			    {
				    return weaponLeftHand;
			    }
			    else if (_faction == UnitFaction.FACTION_PLAYER)
			    {
				    return weaponCurrentHand;
			    }
			    else
			    {
				    return weaponRightHand;
			    }
		    }
	    }
    }

    /**
     * Returns the unit's faction.
     * @return Faction. (player, hostile or neutral)
     */
    internal UnitFaction getFaction() =>
	    _faction;

    /**
     * Returns the current AI state.
     * @return Pointer to AI state.
     */
    internal AIModule getAIModule() =>
	    _currentAIState;

    /**
     * Get the number of turns an AI unit remembers a soldier's position.
     * @return intelligence.
     */
    internal int getIntelligence() =>
	    _intelligence;

    /**
     * Get this unit's original Faction.
     * @return original faction
     */
    internal UnitFaction getOriginalFaction() =>
	    _originalFaction;

    /**
     * Gets the BattleUnit's (horizontal) direction.
     * @return horizontal direction
     */
    internal int getDirection() =>
	    _direction;

    /**
     * Get the list of units spotted this turn.
     * @return List of units.
     */
    internal List<BattleUnit> getUnitsSpottedThisTurn() =>
	    _unitsSpottedThisTurn;

    /**
     * Returns whether the soldier is out of combat, dead or unconscious.
     * A soldier that is out, cannot perform any actions, cannot be selected, but it's still a unit.
     * @return flag if out or not.
     */
    internal bool isOut() =>
	    _status == UnitStatus.STATUS_DEAD || _status == UnitStatus.STATUS_UNCONSCIOUS || _status == UnitStatus.STATUS_IGNORE_ME;

    /**
     * Get whether this unit is visible.
     * @return flag
     */
    internal bool getVisible()
    {
	    if (getFaction() == UnitFaction.FACTION_PLAYER)
	    {
		    return true;
	    }
	    else
	    {
            return _visible;
	    }
    }

    /**
     * Gets the unit height taking into account kneeling/standing.
     * @return Unit's height.
     */
    internal int getHeight() =>
        isKneeled() ? getKneelHeight() : getStandHeight();

    /**
      * Get the unit's kneel height.
      * @return The unit's height in voxels, when kneeling.
      */
    int getKneelHeight() =>
	    _kneelHeight;

    /**
      * Get the unit's stand height.
      * @return The unit's height in voxels, when standing up.
      */
    internal int getStandHeight() =>
	    _standHeight;

    /**
      * Get the unit's floating elevation.
      * @return The unit's elevation over the ground in voxels, when flying.
      */
    internal int getFloatHeight() =>
	    _floatHeight;

    /**
     * use this instead of checking the rules of the armor.
     */
    internal MovementType getMovementType() =>
	    _movementType;

    /**
     * Returns the unit's special ability.
     * @return special ability.
     */
    internal int getSpecialAbility() =>
        (int)_specab;

    /**
     * Gets the unit murderer's id.
     * @return int murderer id.
     */
    internal int getMurdererId() =>
	    _murdererId;

    /**
     * Gets the unit's status.
     * @return the unit's status
     */
    internal UnitStatus getStatus() =>
	    _status;

    /**
     * Morale change with bounds check.
     * @param change can be positive or negative
     */
    internal void moraleChange(int change)
    {
        if (!isFearable()) return;

        _morale += change;
        if (_morale > 100)
            _morale = 100;
        if (_morale < 0)
            _morale = 0;
    }

    /**
     * Get whether the unit is affected by morale loss.
     * Normally only small units are affected by morale loss.
     * @return Is the unit affected by morale?
     */
    bool isFearable() =>
	    (_armor.getSize() == 1);

    /**
     * Return the numeric version of the unit's rank.
     * @return unit rank, 0 = lowest
     */
    internal int getRankInt() =>
	    _rankInt;

    /**
     * Get the geoscape-soldier object.
     * @return soldier.
     */
    internal Soldier getGeoscapeSoldier() =>
	    _geoscapeSoldier;

    /**
     * Returns the BattleUnit's unique ID.
     * @return Unique ID.
     */
    internal int getId() =>
	    _id;

    /**
     * Get the unit's statistics.
     * @return BattleUnitStatistics statistics.
     */
    internal ref BattleUnitStatistics getStatistics() =>
        ref _statistics;

    /**
     * Returns the soldier's amount of health.
     * @return Health.
     */
    internal int getHealth() =>
	    _health;

    internal int getStunlevel() =>
	    _stunlevel;

    internal Unit getUnitRules() =>
        _unitRules;

    /**
     * Sets the unit murderer's id.
     * @param int murderer id.
     */
    internal void setMurdererId(int id) =>
        _murdererId = id;

    /**
     * Gets the unit murderer's weapon.
     * @return int murderer weapon.
     */
    internal string getMurdererWeapon() =>
	    _murdererWeapon;

    /**
     * Gets the unit murderer's weapon's ammo.
     * @return int murderer weapon ammo.
     */
    internal string getMurdererWeaponAmmo() =>
	    _murdererWeaponAmmo;

    /**
     * Gets the unit mind controller's id.
     * @return int mind controller id.
     */
    internal int getMindControllerId() =>
	    _mindControllerID;

    /**
     * Get information on the unit's fatal shot's side.
     * @return UnitSide fatal shot's side.
     */
    internal UnitSide getFatalShotSide() =>
	    _fatalShotSide;

    /**
     * Get information on the unit's fatal shot's body part.
     * @return UnitBodyPart fatal shot's body part.
     */
    internal UnitBodyPart getFatalShotBodyPart() =>
        _fatalShotBodyPart;

    /**
     * Add a kill to the counter.
     */
    internal void addKillCount() =>
        _kills++;

    /**
     * Set the faction the unit was killed by.
     * @param f faction
     */
    internal void killedBy(UnitFaction f) =>
        _killedBy = f;

    /**
     * Set how long since this unit was last exposed.
     * @param turns number of turns
     */
    internal void setTurnsSinceSpotted(int turns) =>
        _turnsSinceSpotted = turns;

    /**
     * Get special weapon.
     */
    internal BattleItem getSpecialWeapon(BattleType type)
    {
	    for (int i = 0; i < SPEC_WEAPON_MAX; ++i)
	    {
		    if (_specWeapon[i] != null && _specWeapon[i].getRules().getBattleType() == type)
		    {
			    return _specWeapon[i];
		    }
	    }
	    return null;
    }

    /**
     * Clear visible tiles.
     */
    internal void clearVisibleTiles()
    {
        foreach (var visibleTile in _visibleTiles)
        {
            visibleTile.setVisible(-1);
        }

        _visibleTiles.Clear();
    }

    /**
     * Clear visible units.
     */
    internal void clearVisibleUnits() =>
        _visibleUnits.Clear();

    internal void freePatrolTarget()
    {
        if (_currentAIState != null)
        {
            _currentAIState.freePatrolTarget();
        }
    }

    /**
     * Changes the BattleUnit's (horizontal) direction.
     * Only used for initial unit placement.
     * @param direction new horizontal direction
     */
    internal void setDirection(int direction)
    {
        _direction = direction;
        _toDirection = direction;
        _directionTurret = direction;
    }

    /**
     * Initialises the falling sequence. Occurs after death or stunned.
     */
    internal void startFalling()
    {
        _status = UnitStatus.STATUS_COLLAPSING;
        _fallPhase = 0;
        _cacheInvalid = true;
    }

    /**
     * Advances the phase of falling sequence.
     */
    internal void keepFalling()
    {
        _fallPhase++;
        if (_fallPhase == _armor.getDeathFrames())
        {
            _fallPhase--;
            if (_health == 0)
            {
                _status = UnitStatus.STATUS_DEAD;
            }
            else
                _status = UnitStatus.STATUS_UNCONSCIOUS;
        }

        _cacheInvalid = true;
    }

    /**
     * Get the pointer to the vector of inventory items.
     * @return pointer to vector.
     */
    internal List<BattleItem> getInventory() =>
        _inventory;

    /**
     * Changes the BattleUnit's position.
     * @param pos position
     * @param updateLastPos refresh last stored position
     */
    internal void setPosition(Position pos, bool updateLastPos = true)
    {
        if (updateLastPos) { _lastPos = _pos; }
        _pos = pos;
    }

    /**
     * Checks if this unit has an inventory. Large units and/or
     * terror units generally don't have inventories.
     * @return True if an inventory is available, false otherwise.
     */
    internal bool hasInventory() =>
	    (_armor.hasInventory());

    /**
     * Sets the unit's tile it's standing on
     * @param tile Pointer to tile.
     * @param tileBelow Pointer to tile below.
     */
    internal void setTile(Tile tile, Tile tileBelow = null)
    {
        _tile = tile;
        if (_tile == null)
        {
            _floating = false;
            return;
        }
        // unit could have changed from flying to walking or vice versa
        if (_status == UnitStatus.STATUS_WALKING && _tile.hasNoFloor(tileBelow) && _movementType == MovementType.MT_FLY)
        {
            _status = UnitStatus.STATUS_FLYING;
            _floating = true;
        }
        else if (_status == UnitStatus.STATUS_FLYING && !_tile.hasNoFloor(tileBelow) && _verticalDirection == 0)
        {
            _status = UnitStatus.STATUS_WALKING;
            _floating = false;
        }
        else if (_status == UnitStatus.STATUS_UNCONSCIOUS)
        {
            _floating = _movementType == MovementType.MT_FLY && _tile.hasNoFloor(tileBelow);
        }
    }

    /**
     * Stops the turning towards the target direction.
     */
    internal void abortTurn() =>
        _status = UnitStatus.STATUS_STANDING;

    /**
     * Initialises variables to start walking.
     * @param direction Which way to walk.
     * @param destination The position we should end up on.
     * @param tileBelowMe Which tile is currently below the unit.
     * @param cache Update cache?
     */
    internal void startWalking(int direction, Position destination, Tile tileBelowMe, bool cache)
    {
        if (direction >= Pathfinding.DIR_UP)
        {
            _verticalDirection = direction;
            _status = UnitStatus.STATUS_FLYING;
        }
        else
        {
            _direction = direction;
            _status = UnitStatus.STATUS_WALKING;
        }
        bool floorFound = false;
        if (!_tile.hasNoFloor(tileBelowMe))
        {
            floorFound = true;
        }
        if (!floorFound || direction >= Pathfinding.DIR_UP)
        {
            _status = UnitStatus.STATUS_FLYING;
            _floating = true;
        }
        else
        {
            _floating = false;
        }

        _walkPhase = 0;
        _destination = destination;
        _lastPos = _pos;
        _cacheInvalid = cache;
        _kneeled = false;
        if (_breathFrame >= 0)
        {
            _breathing = false;
            _breathFrame = 0;
        }
    }

    /**
     * Gets the unit's tile.
     * @return Tile
     */
    internal Tile getTile() =>
	    _tile;

    /**
     * Set whether this unit is visible.
     * @param flag
     */
    internal void setVisible(bool flag) =>
        _visible = flag;

    /**
     * Get the pointer to the vector of visible units.
     * @return pointer to vector.
     */
    internal List<BattleUnit> getVisibleUnits() =>
        _visibleUnits;

    /**
     * Add this unit to the list of visible units. Returns true if this is a new one.
     * @param unit
     * @return
     */
    internal bool addToVisibleUnits(BattleUnit unit)
    {
        bool add = true;
        foreach (var item in _unitsSpottedThisTurn)
        {
            if (item == unit)
            {
                add = false;
                break;
            }
        }
        if (add)
        {
            _unitsSpottedThisTurn.Add(unit);
        }
        foreach (var item in _visibleUnits)
        {
            if (item == unit)
            {
                return false;
            }
        }
        _visibleUnits.Add(unit);
        return true;
    }

    /**
     * Add this unit to the list of visible tiles. Returns true if this is a new one.
     * @param tile
     * @return
     */
    internal bool addToVisibleTiles(Tile tile)
    {
        _visibleTiles.Add(tile);
        return true;
    }

    /**
      * Get the turret type. -1 is no turret.
      * @return type
      */
    internal int getTurretType() =>
	    _turretType;

    /**
     * Gets the BattleUnit's turret direction.
     * @return direction
     */
    internal int getTurretDirection() =>
	    _directionTurret;

    /**
      * Get the unit's loft ID, one per unit tile.
      * Each tile only has one loft, as it is repeated over the entire height of the unit.
      * @param entry Unit tile
      * @return The unit's line of fire template ID.
      */
    internal int getLoftemps(int entry = 0) =>
        _loftempsSet[entry];

    /**
     * Sets the unit's cache flag.
     * @param cache Pointer to cache surface to use, NULL to redraw from scratch.
     * @param part Unit part to cache.
     */
    internal void setCache(Surface cache, int part = 0)
    {
        if (cache == null)
        {
            _cacheInvalid = true;
        }
        else
        {
            _cache[part] = cache;
            _cacheInvalid = false;
        }
    }

    /**
     * Saves the soldier to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "id", _id.ToString() },
            { "genUnitType", _type },
            { "genUnitArmor", _armor.getType() },
            { "faction", ((int)_faction).ToString() },
            { "status", ((int)_status).ToString() },
            { "position", Position.encode(_pos) },
            { "direction", _direction.ToString() },
            { "directionTurret", _directionTurret.ToString() },
            { "tu", _tu.ToString() },
            { "health", _health.ToString() },
            { "stunlevel", _stunlevel.ToString() },
            { "energy", _energy.ToString() },
            { "morale", _morale.ToString() },
            { "kneeled", _kneeled.ToString() },
            { "floating", _floating.ToString() },
            { "armor", new YamlSequenceNode(_currentArmor.Select(x => new YamlScalarNode(x.ToString()))) },
            { "fatalWounds", new YamlSequenceNode(_fatalWounds.Select(x => new YamlScalarNode(x.ToString()))) },
            { "fire", _fire.ToString() },
            { "expBravery", _expBravery.ToString() },
            { "expReactions", _expReactions.ToString() },
            { "expFiring", _expFiring.ToString() },
            { "expThrowing", _expThrowing.ToString() },
            { "expPsiSkill", _expPsiSkill.ToString() },
            { "expPsiStrength", _expPsiStrength.ToString() },
            { "expMelee", _expMelee.ToString() },
            { "turretType", _turretType.ToString() },
            { "visible", _visible.ToString() },
            { "turnsSinceSpotted", _turnsSinceSpotted.ToString() },
            { "rankInt", _rankInt.ToString() },
            { "moraleRestored", _moraleRestored.ToString() }
        };
        if (getAIModule() != null)
	    {
            node.Add("AI", getAIModule().save());
	    }
        node.Add("killedBy", ((int)_killedBy).ToString());
	    if (_originalFaction != _faction)
		    node.Add("originalFaction", ((int)_originalFaction).ToString());
        if (_kills != 0)
            node.Add("kills", _kills.ToString());
	    if (_faction == UnitFaction.FACTION_PLAYER && _dontReselect)
		    node.Add("dontReselect", _dontReselect.ToString());
	    if (!string.IsNullOrEmpty(_spawnUnit))
		    node.Add("spawnUnit", _spawnUnit);
	    node.Add("motionPoints", _motionPoints.ToString());
	    node.Add("respawn", _respawn.ToString());
	    node.Add("activeHand", _activeHand);
        node.Add("tempUnitStatistics", _statistics.save());
	    node.Add("murdererId", _murdererId.ToString());
	    node.Add("fatalShotSide", ((int)_fatalShotSide).ToString());
	    node.Add("fatalShotBodyPart", ((int)_fatalShotBodyPart).ToString());
	    node.Add("murdererWeapon", _murdererWeapon);
        node.Add("murdererWeaponAmmo", _murdererWeaponAmmo);

        node.Add("recolor", new YamlSequenceNode());
        for (var i = 0; i < _recolor.Count; ++i)
	    {
            var p = new YamlSequenceNode
            {
                ((int)_recolor[i].Key).ToString(),
                ((int)_recolor[i].Value).ToString()
            };
            ((YamlSequenceNode)node["recolor"]).Add(p);
	    }
	    node.Add("mindControllerID", _mindControllerID.ToString());

        return node;
    }

    /**
     * Get the amount of turns this unit is on fire. 0 = no fire.
     * @return fire : amount of turns this tile is on fire.
     */
    internal int getFire() =>
	    _fire;

    /**
      * Set the turret type. -1 is no turret.
      * @param turretType
      */
    internal void setTurretType(int turretType) =>
        _turretType = turretType;

    /**
     * Changes the current AI state.
     * @param aiState Pointer to AI state.
     */
    internal void setAIModule(AIModule ai)
    {
        if (_currentAIState != null)
        {
            _currentAIState = null;
        }
        _currentAIState = ai;
    }

    /**
     * common function to adjust a unit's stats according to difficulty setting.
     * @param statAdjustment the stat adjustment variables coefficient value.
     */
    void adjustStats(StatAdjustment adjustment)
    {
	    _stats.tu += adjustment.statGrowth.tu * adjustment.growthMultiplier * _stats.tu / 100;
	    _stats.stamina += adjustment.statGrowth.stamina * adjustment.growthMultiplier * _stats.stamina / 100;
	    _stats.health += adjustment.statGrowth.health * adjustment.growthMultiplier * _stats.health / 100;
	    _stats.bravery += adjustment.statGrowth.bravery * adjustment.growthMultiplier * _stats.bravery / 100;
	    _stats.reactions += adjustment.statGrowth.reactions * adjustment.growthMultiplier * _stats.reactions / 100;
	    _stats.firing += adjustment.statGrowth.firing * adjustment.growthMultiplier * _stats.firing / 100;
	    _stats.throwing += adjustment.statGrowth.throwing * adjustment.growthMultiplier * _stats.throwing / 100;
	    _stats.strength += adjustment.statGrowth.strength * adjustment.growthMultiplier * _stats.strength / 100;
	    _stats.psiStrength += adjustment.statGrowth.psiStrength * adjustment.growthMultiplier * _stats.psiStrength / 100;
	    _stats.psiSkill += adjustment.statGrowth.psiSkill * adjustment.growthMultiplier * _stats.psiSkill / 100;
	    _stats.melee += adjustment.statGrowth.melee * adjustment.growthMultiplier * _stats.melee / 100;

	    _stats.firing = (int)(_stats.firing * adjustment.aimAndArmorMultiplier);
	    _maxArmor[0] = (int)(_maxArmor[0] * adjustment.aimAndArmorMultiplier);
	    _maxArmor[1] = (int)(_maxArmor[1] * adjustment.aimAndArmorMultiplier);
	    _maxArmor[2] = (int)(_maxArmor[2] * adjustment.aimAndArmorMultiplier);
	    _maxArmor[3] = (int)(_maxArmor[3] * adjustment.aimAndArmorMultiplier);
	    _maxArmor[4] = (int)(_maxArmor[4] * adjustment.aimAndArmorMultiplier);
    }

    /**
     * Prepare vector values for recolor.
     * @param basicLook select index for hair and face color.
     * @param utileLook select index for utile color.
     * @param rankLook select index for rank color.
     */
    void setRecolor(int basicLook, int utileLook, int rankLook)
    {
	    const int colorsMax = 4;
	    KeyValuePair<int, int>[] colors =
	    {
		    KeyValuePair.Create(_armor.getFaceColorGroup(), _armor.getFaceColor(basicLook)),
		    KeyValuePair.Create(_armor.getHairColorGroup(), _armor.getHairColor(basicLook)),
		    KeyValuePair.Create(_armor.getUtileColorGroup(), _armor.getUtileColor(utileLook)),
		    KeyValuePair.Create(_armor.getRankColorGroup(), _armor.getRankColor(rankLook)),
	    };

	    for (int i = 0; i < colorsMax; ++i)
	    {
		    if (colors[i].Key > 0 && colors[i].Value > 0)
		    {
			    _recolor.Add(KeyValuePair.Create((byte)(colors[i].Key << 4), (byte)colors[i].Value));
		    }
	    }
    }

    /**
     * Get total amount of fatal wounds this unit has.
     * @return Number of fatal wounds.
     */
    internal int getFatalWounds()
    {
	    int sum = 0;
	    for (int i = 0; i < 6; ++i)
		    sum += _fatalWounds[i];
	    return sum;
    }

    /**
     * Do an amount of damage.
     * @param relative The relative position of which part of armor and/or bodypart is hit.
     * @param power The amount of damage to inflict.
     * @param type The type of damage being inflicted.
     * @param ignoreArmor Should the damage ignore armor resistance?
     * @return damage done after adjustment
     */
    internal int damage(Position relative, int power, ItemDamageType type, bool ignoreArmor = false)
    {
        UnitSide side = UnitSide.SIDE_FRONT;
        UnitBodyPart bodypart = UnitBodyPart.BODYPART_TORSO;
        _hitByAnything = true;
        if (power <= 0)
        {
            return 0;
        }

        power = (int)Math.Floor(power * _armor.getDamageModifier(type));

        if (type == ItemDamageType.DT_SMOKE) type = ItemDamageType.DT_STUN; // smoke doesn't do real damage, but stun damage

        if (!ignoreArmor)
        {
            if (relative == new Position(0, 0, 0))
            {
                side = UnitSide.SIDE_UNDER;
            }
            else
            {
                int relativeDirection;
                int abs_x = Math.Abs(relative.x);
                int abs_y = Math.Abs(relative.y);
                if (abs_y > abs_x * 2)
                    relativeDirection = 8 + 4 * Convert.ToInt32(relative.y > 0);
                else if (abs_x > abs_y * 2)
                    relativeDirection = 10 + 4 * Convert.ToInt32(relative.x < 0);
                else
                {
                    if (relative.x < 0)
                    {
                        if (relative.y > 0)
                            relativeDirection = 13;
                        else
                            relativeDirection = 15;
                    }
                    else
                    {
                        if (relative.y > 0)
                            relativeDirection = 11;
                        else
                            relativeDirection = 9;
                    }
                }

                switch ((relativeDirection - _direction) % 8)
                {
                    case 0: side = UnitSide.SIDE_FRONT; break;
                    case 1: side = RNG.generate(0, 2) < 2 ? UnitSide.SIDE_FRONT : UnitSide.SIDE_RIGHT; break;
                    case 2: side = UnitSide.SIDE_RIGHT; break;
                    case 3: side = RNG.generate(0, 2) < 2 ? UnitSide.SIDE_REAR : UnitSide.SIDE_RIGHT; break;
                    case 4: side = UnitSide.SIDE_REAR; break;
                    case 5: side = RNG.generate(0, 2) < 2 ? UnitSide.SIDE_REAR : UnitSide.SIDE_LEFT; break;
                    case 6: side = UnitSide.SIDE_LEFT; break;
                    case 7: side = RNG.generate(0, 2) < 2 ? UnitSide.SIDE_FRONT : UnitSide.SIDE_LEFT; break;
                }
                if (relative.z >= getHeight())
                {
                    bodypart = UnitBodyPart.BODYPART_HEAD;
                }
                else if (relative.z > 4)
                {
                    switch (side)
                    {
                        case UnitSide.SIDE_LEFT: bodypart = UnitBodyPart.BODYPART_LEFTARM; break;
                        case UnitSide.SIDE_RIGHT: bodypart = UnitBodyPart.BODYPART_RIGHTARM; break;
                        default: bodypart = UnitBodyPart.BODYPART_TORSO; break;
                    }
                }
                else
                {
                    switch (side)
                    {
                        case UnitSide.SIDE_LEFT: bodypart = UnitBodyPart.BODYPART_LEFTLEG; break;
                        case UnitSide.SIDE_RIGHT: bodypart = UnitBodyPart.BODYPART_RIGHTLEG; break;
                        default:
                            bodypart = (UnitBodyPart)RNG.generate((int)UnitBodyPart.BODYPART_RIGHTLEG, (int)UnitBodyPart.BODYPART_LEFTLEG); break;
                    }
                }
            }
            power -= getArmor(side);
        }

        if (power > 0)
        {
            if (type == ItemDamageType.DT_STUN)
            {
                _stunlevel += power;
            }
            else
            {
                // health damage
                _health -= power;
                if (_health < 0)
                {
                    _health = 0;
                }

                if (type != ItemDamageType.DT_IN)
                {
                    if (_armor.getDamageModifier(ItemDamageType.DT_STUN) > 0.0)
                    {
                        // conventional weapons can cause additional stun damage
                        _stunlevel += RNG.generate(0, power / 4);
                    }
                    // fatal wounds
                    if (isWoundable())
                    {
                        if (RNG.generate(0, 10) < power)
                            _fatalWounds[(int)bodypart] += RNG.generate(1, 3);

                        if (_fatalWounds[(int)bodypart] != 0)
                            moraleChange(-_fatalWounds[(int)bodypart]);
                    }
                    // armor damage
                    setArmor(getArmor(side) - (power / 10) - 1, side);
                }
            }
        }

        setFatalShotInfo(side, bodypart);

        return power < 0 ? 0 : power;
    }

    /**
     * Get whether the unit is affected by fatal wounds.
     * Normally only soldiers are affected by fatal wounds.
     * @return Is the unit affected by wounds?
     */
    internal bool isWoundable() =>
	    (_type=="SOLDIER" || (Options.alienBleeding && _originalFaction != UnitFaction.FACTION_PLAYER && _armor.getSize() == 1));

    /**
     * Set the armor value of a certain armor side.
     * @param armor Amount of armor.
     * @param side The side of the armor.
     */
    void setArmor(int armor, UnitSide side)
    {
        if (armor < 0)
        {
            armor = 0;
        }
        _currentArmor[(int)side] = armor;
    }

    /**
     * Set information on the unit's fatal blow.
     * @param UnitSide unit's side that was shot.
     * @param UnitBodyPart unit's body part that was shot.
     */
    void setFatalShotInfo(UnitSide side, UnitBodyPart bodypart)
    {
        _fatalShotSide = side;
        _fatalShotBodyPart = bodypart;
    }

    /**
     * Set the amount of turns this unit is on fire. 0 = no fire.
     * @param fire : amount of turns this tile is on fire.
     */
    internal void setFire(int fire)
    {
        if (_specab != SpecialAbility.SPECAB_BURNFLOOR && _specab != SpecialAbility.SPECAB_BURN_AND_EXPLODE)
            _fire = fire;
    }

    /**
     * Adds one to the firing exp counter.
     */
    internal void addFiringExp() =>
        _expFiring++;

    /**
    * Set health to 0 - used when getting killed unconscious.
    */
    internal void kill() =>
	    _health = 0;

    /**
     * Gets whether this unit can be captured alive (applies to aliens).
     */
    internal bool getCapturable() =>
	    _capturable;

    /**
     * Check if this unit is in the exit area.
     * @param stt Type of exit tile to check for.
     * @return Is in the exit area?
     */
    internal bool isInExitArea(SpecialTileType stt = SpecialTileType.START_POINT) =>
	    _tile != null && _tile.getMapData(TilePart.O_FLOOR) != null && (_tile.getMapData(TilePart.O_FLOOR).getSpecialType() == stt);

    /**
     * Get unit's name.
     * An aliens name is the translation of it's race and rank.
     * hence the language pointer needed.
     * @param lang Pointer to language.
     * @param debugAppendId Append unit ID to name for debug purposes.
     * @return name Widecharstring of the unit's name.
     */
    internal string getName(Language lang, bool debugAppendId = false)
    {
	    if (_type != "SOLDIER" && lang != null)
	    {
		    var ret = new StringBuilder();

		    if (_type.Contains("STR_"))
			    ret.Append(lang.getString(_type));
		    else
			    ret.Append(lang.getString(_race));

		    if (debugAppendId)
		    {
			    ret.Append($" {_id}");
		    }
		    return ret.ToString();
	    }

	    return _name;
    }

    /**
     * Returns the soldier's amount of morale.
     * @return Morale.
     */
    internal int getMorale() =>
	    _morale;

    /**
     * Get the units carried weight in strength units.
     * @param draggingItem item to ignore
     * @return weight
     */
    internal int getCarriedWeight(BattleItem draggingItem = null)
    {
	    int weight = _armor.getWeight();
	    foreach (var i in _inventory)
	    {
		    if (i == draggingItem) continue;
		    weight += i.getRules().getWeight();
		    if (i.getAmmoItem() != i && i.getAmmoItem() != null) weight += i.getAmmoItem().getRules().getWeight();
	    }
	    return Math.Max(0,weight);
    }

    /**
     * Set special weapon that is handled outside inventory.
     * @param save
     */
    internal void setSpecialWeapon(SavedBattleGame save, Mod.Mod mod)
    {
	    RuleItem item = null;
	    int i = 0;

	    if (getUnitRules() != null)
	    {
		    item = mod.getItem(getUnitRules().getMeleeWeapon());
		    if (item != null)
		    {
			    _specWeapon[i++] = createItem(save, this, item);
		    }
	    }
	    item = mod.getItem(getArmor().getSpecialWeapon());
	    if (item != null)
	    {
		    _specWeapon[i++] = createItem(save, this, item);
	    }
	    if (getBaseStats().psiSkill > 0 && getOriginalFaction() == UnitFaction.FACTION_HOSTILE)
	    {
		    item = mod.getItem(getUnitRules().getPsiWeapon());
		    if (item != null)
		    {
			    _specWeapon[i++] = createItem(save, this, item);
		    }
	    }
    }

    /**
     * Helper function used by `BattleUnit::setSpecialWeapon`
     */
    static BattleItem createItem(SavedBattleGame save, BattleUnit unit, RuleItem rule)
    {
        BattleItem item = new BattleItem(rule, ref save.getCurrentItemId());
        item.setOwner(unit);
        save.removeItem(item); //item outside inventory, deleted when game is shutdown.
        return item;
    }

    /**
     * Derive the numeric unit rank from the string rank
     * (for soldier units).
     */
    void deriveRank()
    {
        if (_geoscapeSoldier != null)
        {
            switch (_geoscapeSoldier.getRank())
            {
                case SoldierRank.RANK_ROOKIE: _rankInt = 0; break;
                case SoldierRank.RANK_SQUADDIE: _rankInt = 1; break;
                case SoldierRank.RANK_SERGEANT: _rankInt = 2; break;
                case SoldierRank.RANK_CAPTAIN: _rankInt = 3; break;
                case SoldierRank.RANK_COLONEL: _rankInt = 4; break;
                case SoldierRank.RANK_COMMANDER: _rankInt = 5; break;
                default: _rankInt = 0; break;
            }
        }
    }

    /**
     * Change the numeric version of the unit's rank.
     * @param rank unit rank, 0 = lowest
     */
    internal void setRankInt(int rank) =>
        _rankInt = rank;

    /**
     * Returns the direction from this unit to a given point.
     * 0 <-> y = -1, x = 0
     * 1 <-> y = -1, x = 1
     * 3 <-> y = 1, x = 1
     * 5 <-> y = 1, x = -1
     * 7 <-> y = -1, x = -1
     * @param point given position.
     * @return direction.
     */
    internal int directionTo(Position point)
    {
	    double ox = point.x - _pos.x;
	    double oy = point.y - _pos.y;
	    double angle = Math.Atan2(ox, -oy);
	    // divide the pie in 4 angles each at 1/8th before each quarter
	    double[] pie = {(M_PI_4 * 4.0) - M_PI_4 / 2.0, (M_PI_4 * 3.0) - M_PI_4 / 2.0, (M_PI_4 * 2.0) - M_PI_4 / 2.0, (M_PI_4 * 1.0) - M_PI_4 / 2.0};
	    int dir = 0;

	    if (angle > pie[0] || angle < -pie[0])
	    {
		    dir = 4;
	    }
	    else if (angle > pie[1])
	    {
		    dir = 3;
	    }
	    else if (angle > pie[2])
	    {
		    dir = 2;
	    }
	    else if (angle > pie[3])
	    {
		    dir = 1;
	    }
	    else if (angle < -pie[1])
	    {
		    dir = 5;
	    }
	    else if (angle < -pie[2])
	    {
		    dir = 6;
	    }
	    else if (angle < -pie[3])
	    {
		    dir = 7;
	    }
	    else if (angle < pie[0])
	    {
		    dir = 0;
	    }
	    return dir;
    }

    /**
     * Check if this unit lies (e.g. unconscious) in the exit area.
     * @param tile Unit's location.
     * @param stt Type of exit tile to check for.
     * @return Is in the exit area?
     */
    internal bool liesInExitArea(Tile tile, SpecialTileType stt = SpecialTileType.START_POINT) =>
	    tile != null && tile.getMapData(TilePart.O_FLOOR) != null && (tile.getMapData(TilePart.O_FLOOR).getSpecialType() == stt);

    /**
     * Elevates the unit to grand galactic inquisitor status,
     * meaning they will NOT take part in the current battle.
     */
    internal void goToTimeOut() =>
        _status = UnitStatus.STATUS_IGNORE_ME;

    /**
     * Get the pointer to the vector of visible tiles.
     * @return pointer to vector.
     */
    internal List<Tile> getVisibleTiles() =>
        _visibleTiles;

    /**
     * Mark this unit as not reselectable.
     */
    internal void dontReselect() =>
        _dontReselect = true;

    /**
     * Check whether reselecting this unit is allowed.
     * @return bool
     */
    internal bool reselectAllowed() =>
	    !_dontReselect;

    /**
     * Checks if this unit can be selected. Only alive units
     * belonging to the faction can be selected.
     * @param faction The faction to compare with.
     * @param checkReselect Check if the unit is reselectable.
     * @param checkInventory Check if the unit has an inventory.
     * @return True if the unit can be selected, false otherwise.
     */
    internal bool isSelectable(UnitFaction faction, bool checkReselect, bool checkInventory) =>
	    (_faction == faction && !isOut() && (!checkReselect || reselectAllowed()) && (!checkInventory || hasInventory()));

    /**
     * Prepare for a new turn.
     */
    internal void prepareNewTurn(bool fullProcess = true)
    {
        if (_status == UnitStatus.STATUS_IGNORE_ME)
        {
            return;
        }

        _unitsSpottedThisTurn.Clear();

        // revert to original faction
        // don't give it back its TUs or anything this round
        // because it's no longer a unit of the team getting TUs back
        if (_faction != _originalFaction)
        {
            _faction = _originalFaction;
            if (_faction == UnitFaction.FACTION_PLAYER && _currentAIState != null)
            {
                _currentAIState = null;
            }
        }
        else
        {
            recoverTimeUnits();
        }
        _dontReselect = false;

        _motionPoints = 0;

        // transition between stages, don't do damage or panic
        if (!fullProcess)
        {
            if (_kneeled)
            {
                // stand up if kneeling
                _kneeled = false;
            }
            return;
        }

        // suffer from fatal wounds
        _health -= getFatalWounds();

        // suffer from fire
        if (!_hitByFire && _fire > 0)
        {
            _health = (int)(_health - _armor.getDamageModifier(ItemDamageType.DT_IN) * RNG.generate(Mod.Mod.FIRE_DAMAGE_RANGE[0], Mod.Mod.FIRE_DAMAGE_RANGE[1]));
            _fire--;
        }

        if (_health < 0)
            _health = 0;

        // if unit is dead, AI state should be gone
        if (_health == 0 && _currentAIState != null)
        {
            _currentAIState = null;
        }

        // recover stun 1pt/turn
        if (_stunlevel > 0 &&
            (_armor.getSize() == 1 || !isOut()))
            healStun(1);

        if (!isOut())
        {
            int chance = 100 - (2 * getMorale());
            if (RNG.generate(1, 100) <= chance)
            {
                int type = RNG.generate(0, 100);
                _status = (type <= 33 ? UnitStatus.STATUS_BERSERK : UnitStatus.STATUS_PANICKING); // 33% chance of berserk, panic can mean freeze or flee, but that is determined later
            }
            else
            {
                // successfully avoided panic
                // increase bravery experience counter
                if (chance > 1)
                    _expBravery++;
            }
        }
        _hitByFire = false;
    }

    /**
     * Recovers a unit's TUs and energy, taking a number of factors into consideration.
     */
    internal void recoverTimeUnits()
    {
        // recover TUs
        int TURecovery = getBaseStats().tu;
        float encumbrance = (float)getBaseStats().strength / (float)getCarriedWeight();
        if (encumbrance < 1)
        {
            TURecovery = (int)(encumbrance * TURecovery);
        }
        // Each fatal wound to the left or right leg reduces the soldier's TUs by 10%.
        TURecovery -= (TURecovery * ((_fatalWounds[(int)UnitBodyPart.BODYPART_LEFTLEG] + _fatalWounds[(int)UnitBodyPart.BODYPART_RIGHTLEG]) * 10)) / 100;
        setTimeUnits(TURecovery);

        // recover energy
        if (!isOut())
        {
            int ENRecovery;
            if (_geoscapeSoldier != null)
            {
                ENRecovery = _geoscapeSoldier.getInitStats().tu / 3;
            }
            else
            {
                ENRecovery = _unitRules.getEnergyRecovery();
            }
            // Each fatal wound to the body reduces the soldier's energy recovery by 10%.
            ENRecovery -= (_energy * (_fatalWounds[(int)UnitBodyPart.BODYPART_TORSO] * 10)) / 100;
            _energy = Math.Max(0, Math.Min(getBaseStats().stamina, _energy + ENRecovery));
        }
    }

    /**
     * Set a specific number of timeunits.
     * @param tu
     */
    internal void setTimeUnits(int tu) =>
        _tu = Math.Max(0, tu);

    /**
     * Do an amount of stun recovery.
     * @param power
     */
    void healStun(int power)
    {
        _stunlevel -= power;
        if (_stunlevel < 0) _stunlevel = 0;
    }

    /**
     * Checks if there's an inventory item in
     * the specified inventory position.
     * @param slot Inventory slot.
     * @param x X position in slot.
     * @param y Y position in slot.
     * @return Item in the slot, or NULL if none.
     */
    internal BattleItem getItem(RuleInventory slot, int x = 0, int y = 0)
    {
	    // Soldier items
	    if (slot.getType() != InventoryType.INV_GROUND)
	    {
		    foreach (var i in _inventory)
		    {
			    if (i.getSlot() == slot && i.occupiesSlot(x, y))
			    {
				    return i;
			    }
		    }
	    }
	    // Ground items
	    else if (_tile != null)
	    {
		    foreach (var i in _tile.getInventory())
		    {
			    if (i.occupiesSlot(x, y))
			    {
				    return i;
			    }
		    }
	    }
	    return null;
    }

    /**
     * Spend time units if it can. Return false if it can't.
     * @param tu
     * @return flag if it could spend the time units or not.
     */
    internal bool spendTimeUnits(int tu)
    {
        if (tu <= _tu)
        {
            _tu -= tu;
            return true;
        }
        else
        {
            return false;
        }
    }

    /**
     * Get the max armor value of a certain armor side.
     * @param side The side of the armor.
     * @return Amount of armor.
     */
    internal int getMaxArmor(UnitSide side) =>
	    _maxArmor[(int)side];

    /**
     * Get the units's rank string.
     * @return rank.
     */
    internal string getRankString() =>
	    _rank;

    /**
     * Get how long since this unit was exposed.
     * @return number of turns
     */
    internal int getTurnsSinceSpotted() =>
	    _turnsSinceSpotted;

    /**
     * did this unit already take fire damage this turn?
     * (used to avoid damaging large units multiple times.)
     * @return ow it burns
     */
    internal bool tookFireDamage() =>
	    _hitByFire;

    /**
     * toggle the state of the fire damage tracking boolean.
     */
    internal void toggleFireDamage() =>
	    _hitByFire = !_hitByFire;

    /**
     * Kneel down.
     * @param kneeled to kneel or to stand up
     */
    internal void kneel(bool kneeled)
    {
	    _kneeled = kneeled;
	    _cacheInvalid = true;
    }

    /**
     * Advances the turning towards the target direction.
     * @param turret True to turn the turret, false to turn the unit.
     */
    internal void turn(bool turret = false)
    {
	    int a = 0;

	    if (turret)
	    {
		    if (_directionTurret == _toDirectionTurret)
		    {
			    abortTurn();
			    return;
		    }
		    a = _toDirectionTurret - _directionTurret;
	    }
	    else
	    {
		    if (_direction == _toDirection)
		    {
			    abortTurn();
			    return;
		    }
		    a = _toDirection - _direction;
	    }

	    if (a != 0) {
		    if (a > 0) {
			    if (a <= 4) {
				    if (!turret) {
					    if (_turretType > -1)
						    _directionTurret++;
					    _direction++;
				    } else _directionTurret++;
			    } else {
				    if (!turret) {
					    if (_turretType > -1)
						    _directionTurret--;
					    _direction--;
				    } else _directionTurret--;
			    }
		    } else {
			    if (a > -4) {
				    if (!turret) {
					    if (_turretType > -1)
						    _directionTurret--;
					    _direction--;
				    } else _directionTurret--;
			    } else {
				    if (!turret) {
					    if (_turretType > -1)
						    _directionTurret++;
					    _direction++;
				    } else _directionTurret++;
			    }
		    }
		    if (_direction < 0) _direction = 7;
		    if (_direction > 7) _direction = 0;
		    if (_directionTurret < 0) _directionTurret = 7;
		    if (_directionTurret > 7) _directionTurret = 0;
		    if (_visible || _faction == UnitFaction.FACTION_PLAYER)
			    _cacheInvalid = true;
	    }

	    if (turret)
	    {
		     if (_toDirectionTurret == _directionTurret)
		     {
			    // we officially reached our destination
			    _status = UnitStatus.STATUS_STANDING;
		     }
	    }
	    else if (_toDirection == _direction || _status == UnitStatus.STATUS_UNCONSCIOUS)
	    {
		    // we officially reached our destination
		    _status = UnitStatus.STATUS_STANDING;
	    }
    }

    /**
     * Check if the unit is still cached in the Map cache.
     * When the unit changes it needs to be re-cached.
     * @return True if it needs to be re-cached.
     */
    internal bool isCacheInvalid() =>
	    _cacheInvalid;

    /**
     * Returns the current cache surface.
     * When the unit changes it needs to be re-cached.
     * @param part Unit part to check.
     * @return Pointer to cache surface used.
     */
    internal Surface getCache(int part = 0)
    {
	    if (part < 0) part = 0;
	    return _cache[part];
    }

    /**
     * Set health to 0 and set status dead - used when getting zombified.
     */
    internal void instaKill()
    {
	    _health = 0;
	    _status = UnitStatus.STATUS_DEAD;
    }

    /**
      * Get the unit's value. Used for score at debriefing.
      * @return value score
      */
    internal int getValue() =>
	    _value;

    /**
     * Get the faction the unit was killed by.
     * @return faction
     */
    internal UnitFaction killedBy() =>
	    _killedBy;

    /**
     * Get the unit that is spawned when this one dies.
     * @return unit.
     */
    internal string getSpawnUnit() =>
	    _spawnUnit;

    /**
     * Converts unit to another faction (original faction is still stored).
     * @param f faction.
     */
    internal void convertToFaction(UnitFaction f) =>
	    _faction = f;

    internal void updateGeoscapeStats(Soldier soldier)
    {
	    soldier.addMissionCount();
	    soldier.addKillCount(_kills);
    }

    /**
     * Check if unit eligible for squaddie promotion. If yes, promote the unit.
     * Increase the mission counter. Calculate the experience increases.
     * @param geoscape Pointer to geoscape save.
     * @param statsDiff (out) The passed UnitStats struct will be filled with the stats differences.
     * @return True if the soldier was eligible for squaddie promotion.
     */
    internal bool postMissionProcedures(SavedGame geoscape, out UnitStats statsDiff)
    {
        statsDiff = default;
	    Soldier s = geoscape.getSoldier(_id);
	    if (s == null)
	    {
		    return false;
	    }

	    updateGeoscapeStats(s);

	    UnitStats stats = s.getCurrentStats();
	    statsDiff -= stats;        // subtract old stats
	    UnitStats caps = s.getRules().getStatCaps();
	    int healthLoss = _stats.health - _health;

	    s.setWoundRecovery((int)RNG.generate((healthLoss*0.5),(healthLoss*1.5)));

	    if (_expBravery != 0 && stats.bravery < caps.bravery)
	    {
		    if (_expBravery > RNG.generate(0,10)) stats.bravery += 10;
	    }
	    if (_expReactions != 0 && stats.reactions < caps.reactions)
	    {
		    stats.reactions += improveStat(_expReactions);
	    }
	    if (_expFiring != 0 && stats.firing < caps.firing)
	    {
		    stats.firing += improveStat(_expFiring);
	    }
	    if (_expMelee != 0 && stats.melee < caps.melee)
	    {
		    stats.melee += improveStat(_expMelee);
	    }
	    if (_expThrowing != 0 && stats.throwing < caps.throwing)
	    {
		    stats.throwing += improveStat(_expThrowing);
	    }
	    if (_expPsiSkill != 0 && stats.psiSkill < caps.psiSkill)
	    {
		    stats.psiSkill += improveStat(_expPsiSkill);
	    }
	    if (_expPsiStrength != 0 && stats.psiStrength < caps.psiStrength)
	    {
		    stats.psiStrength += improveStat(_expPsiStrength);
	    }

	    bool hasImproved = false;
	    if (_expBravery != 0 || _expReactions != 0 || _expFiring != 0 || _expPsiSkill != 0 || _expPsiStrength != 0 || _expMelee != 0)
	    {
		    hasImproved = true;
		    if (s.getRank() == SoldierRank.RANK_ROOKIE)
			    s.promoteRank();
		    int v;
		    v = caps.tu - stats.tu;
		    if (v > 0) stats.tu += RNG.generate(0, v/10 + 2);
		    v = caps.health - stats.health;
		    if (v > 0) stats.health += RNG.generate(0, v/10 + 2);
		    v = caps.strength - stats.strength;
		    if (v > 0) stats.strength += RNG.generate(0, v/10 + 2);
		    v = caps.stamina - stats.stamina;
		    if (v > 0) stats.stamina += RNG.generate(0, v/10 + 2);
	    }

	    statsDiff += stats; // add new stats

	    return hasImproved;
    }

    /**
     * Converts the number of experience to the stat increase.
     * @param Experience counter.
     * @return Stat increase.
     */
    int improveStat(int exp)
    {
	    if      (exp > 10) return RNG.generate(2, 6);
	    else if (exp > 5)  return RNG.generate(1, 4);
	    else if (exp > 2)  return RNG.generate(1, 3);
	    else if (exp > 0)  return RNG.generate(0, 1);
	    else               return 0;
    }

    /**
     * Look at a point.
     * @param point Position to look at.
     * @param turret True to turn the turret, false to turn the unit.
     */
    internal void lookAt(Position point, bool turret = false)
    {
	    int dir = directionTo(point);

	    if (turret)
	    {
		    _toDirectionTurret = dir;
		    if (_toDirectionTurret != _directionTurret)
		    {
			    _status = UnitStatus.STATUS_TURNING;
		    }
	    }
	    else
	    {
		    _toDirection = dir;
		    if (_toDirection != _direction
			    && _toDirection < 8
			    && _toDirection > -1)
		    {
			    _status = UnitStatus.STATUS_TURNING;
		    }
	    }
    }

    /**
     * Aim. (shows the right hand sprite and weapon holding)
     * @param aiming true/false
     */
    internal void aim(bool aiming)
    {
	    if (aiming)
		    _status = UnitStatus.STATUS_AIMING;
	    else
		    _status = UnitStatus.STATUS_STANDING;

	    if (_visible || _faction == UnitFaction.FACTION_PLAYER)
		    _cacheInvalid = true;
    }

    /**
     * Calculate throwing accuracy.
     * @return throwing Accuracy
     */
    internal double getThrowingAccuracy() =>
	    (double)(getBaseStats().throwing * getAccuracyModifier()) / 100.0;

    /**
     * Calculate firing accuracy.
     * Formula = accuracyStat * weaponAccuracy * kneelingbonus(1.15) * one-handPenalty(0.8) * woundsPenalty(% health) * critWoundsPenalty (-10%/wound)
     * @param actionType
     * @param item
     * @return firing Accuracy
     */
    internal int getFiringAccuracy(BattleActionType actionType, BattleItem item)
    {
	    int weaponAcc = item.getRules().getAccuracySnap();
	    if (actionType == BattleActionType.BA_AIMEDSHOT || actionType == BattleActionType.BA_LAUNCH)
		    weaponAcc = item.getRules().getAccuracyAimed();
	    else if (actionType == BattleActionType.BA_AUTOSHOT)
		    weaponAcc = item.getRules().getAccuracyAuto();
	    else if (actionType == BattleActionType.BA_HIT)
	    {
		    if (item.getRules().isSkillApplied())
		    {
			    return (getBaseStats().melee * item.getRules().getAccuracyMelee() / 100) * getAccuracyModifier(item) / 100;
		    }
		    return item.getRules().getAccuracyMelee() * getAccuracyModifier(item) / 100;
	    }

	    int result = getBaseStats().firing * weaponAcc / 100;

	    if (_kneeled)
	    {
		    result = result * 115 / 100;
	    }

	    if (item.getRules().isTwoHanded())
	    {
		    // two handed weapon, means one hand should be empty
		    if (getItem("STR_RIGHT_HAND") != null && getItem("STR_LEFT_HAND") != null)
		    {
			    result = result * 80 / 100;
		    }
	    }

	    return result * getAccuracyModifier(item) / 100;
    }

    /**
     * Adds one to the throwing exp counter.
     */
    internal void addThrowingExp() =>
	    _expThrowing++;

    /**
     * To calculate firing accuracy. Takes health and fatal wounds into account.
     * Formula = accuracyStat * woundsPenalty(% health) * critWoundsPenalty (-10%/wound)
     * @param item the item we are shooting right now.
     * @return modifier
     */
    int getAccuracyModifier(BattleItem item = null)
    {
	    int wounds = _fatalWounds[(int)UnitBodyPart.BODYPART_HEAD];

	    if (item != null)
	    {
		    if (item.getRules().isTwoHanded())
		    {
			    wounds += _fatalWounds[(int)UnitBodyPart.BODYPART_RIGHTARM] + _fatalWounds[(int)UnitBodyPart.BODYPART_LEFTARM];
		    }
		    else
		    {
			    if (getItem("STR_RIGHT_HAND") == item)
			    {
				    wounds += _fatalWounds[(int)UnitBodyPart.BODYPART_RIGHTARM];
			    }
			    else
			    {
				    wounds += _fatalWounds[(int)UnitBodyPart.BODYPART_LEFTARM];
			    }
		    }
	    }
	    return Math.Max(10, 25 * _health / getBaseStats().health + 75 + -10 * wounds);
    }

    /**
     * reset the unit hit state.
     */
    internal void resetHitState() =>
	    _hitByAnything = false;

    /**
     * Gets the soldier's gender.
     */
    internal SoldierGender getGender() =>
	    _gender;

    /**
     * Gets this unit's respawn flag.
     */
    internal bool getRespawn() =>
	    _respawn;

    /**
     * Sets this unit to respawn (or not).
     * @param respawn whether it should respawn.
     */
    internal void setRespawn(bool respawn) =>
	    _respawn = respawn;

    /**
     * Get the unit's total firing xp for this mission.
     */
    internal int getFiringXP() =>
	    _expFiring;

    /**
     * Set the murderer's weapon.
     * @param string murderer's weapon.
     */
    internal void setMurdererWeapon(string weapon) =>
	    _murdererWeapon = weapon;

    /**
     * Set the murderer's weapon's ammo.
     * @param string murderer weapon ammo.
     */
    internal void setMurdererWeaponAmmo(string weaponAmmo) =>
	    _murdererWeaponAmmo = weaponAmmo;

    /**
     * Artificially alter a unit's firing xp. (used for shotguns)
     */
    internal void nerfFiringXP(int newXP) =>
	    _expFiring = newXP;

    /**
     * Little formula to calculate reaction score.
     * @return Reaction score.
     */
    internal double getReactionScore()
    {
	    //(Reactions Stat) x (Current Time Units / Max TUs)
	    double score = ((double)getBaseStats().reactions * (double)getTimeUnits()) / (double)getBaseStats().tu;
	    return score;
    }

    /**
     * Adds one to the reaction exp counter.
     */
    internal void addReactionExp() =>
	    _expReactions++;

    /**
     * Was this unit just hit?
     */
    internal bool getHitState() =>
	    _hitByAnything;

    /**
     * this function checks if a tile is visible, using maths.
     * @param pos the position to check against
     * @return what the maths decide
     */
    internal bool checkViewSector(Position pos)
    {
	    int deltaX = pos.x - _pos.x;
	    int deltaY = _pos.y - pos.y;

	    switch (_direction)
	    {
		    case 0:
			    if ( (deltaX + deltaY >= 0) && (deltaY - deltaX >= 0) )
				    return true;
			    break;
		    case 1:
			    if ( (deltaX >= 0) && (deltaY >= 0) )
				    return true;
			    break;
		    case 2:
			    if ( (deltaX + deltaY >= 0) && (deltaY - deltaX <= 0) )
				    return true;
			    break;
		    case 3:
			    if ( (deltaY <= 0) && (deltaX >= 0) )
				    return true;
			    break;
		    case 4:
			    if ( (deltaX + deltaY <= 0) && (deltaY - deltaX <= 0) )
				    return true;
			    break;
		    case 5:
			    if ( (deltaX <= 0) && (deltaY <= 0) )
				    return true;
			    break;
		    case 6:
			    if ( (deltaX + deltaY <= 0) && (deltaY - deltaX >= 0) )
				    return true;
			    break;
		    case 7:
			    if ( (deltaY >= 0) && (deltaX <= 0) )
				    return true;
			    break;
		    default:
			    return false;
	    }
	    return false;
    }

    /**
     * Get the name of any melee weapon we may be carrying, or a built in one.
     * @return the name .
     */
    internal BattleItem getMeleeWeapon()
    {
	    BattleItem melee = getItem("STR_RIGHT_HAND");
	    if (melee != null && melee.getRules().getBattleType() == BattleType.BT_MELEE)
	    {
		    return melee;
	    }
	    melee = getItem("STR_LEFT_HAND");
	    if (melee != null && melee.getRules().getBattleType() == BattleType.BT_MELEE)
	    {
		    return melee;
	    }
	    melee = getSpecialWeapon(BattleType.BT_MELEE);
	    if (melee != null)
	    {
		    return melee;
	    }
	    return null;
    }

    /**
     * Adds one to the melee exp counter.
     */
    internal void addMeleeExp() =>
	    _expMelee++;

    /**
     * Set the unit that is spawned when this one dies.
     * @param spawnUnit unit.
     */
    internal void setSpawnUnit(string spawnUnit) =>
	    _spawnUnit = spawnUnit;

    /**
     * Get sound to play when unit aggros.
     * @return sound
     */
    internal int getAggroSound() =>
	    _aggroSound;

    /**
     * Get the units we are charging towards.
     * @return Charge Target
     */
    internal BattleUnit getCharging() =>
	    _charging;

	/// Sets this unit is in hiding for a turn (or not).
	internal void setHiding(bool hiding) =>
        _hidingForTurn = hiding;

    /**
     * Check if we have ammo and reload if needed (used for AI).
     * @return Do we have ammo?
     */
    internal bool checkAmmo()
    {
	    BattleItem weapon = getItem("STR_RIGHT_HAND");
	    if (weapon == null || weapon.getAmmoItem() != null || weapon.getRules().getBattleType() == BattleType.BT_MELEE || getTimeUnits() < 15)
	    {
		    weapon = getItem("STR_LEFT_HAND");
		    if (weapon == null || weapon.getAmmoItem() != null || weapon.getRules().getBattleType() == BattleType.BT_MELEE || getTimeUnits() < 15)
		    {
			    return false;
		    }
	    }
	    // we have a non-melee weapon with no ammo and 15 or more TUs - we might need to look for ammo then
	    BattleItem ammo = null;
	    bool wrong = true;
	    foreach (var i in getInventory())
	    {
		    ammo = i;
		    foreach (var c in weapon.getRules().getCompatibleAmmo())
		    {
			    if (c == ammo.getRules().getType())
			    {
				    wrong = false;
				    break;
			    }
		    }
		    if (!wrong) break;
	    }

	    if (wrong) return false; // didn't find any compatible ammo in inventory

	    spendTimeUnits(15);
	    weapon.setAmmoItem(ammo);
	    ammo.moveToOwner(null);

	    return true;
    }

    /**
     * Let AI do their thing.
     * @param action AI action.
     */
    internal void think(ref BattleAction action)
    {
	    checkAmmo();
	    _currentAIState.think(ref action);
    }

    /**
     * Set the units we are charging towards.
     * @param chargeTarget Charge Target
     */
    internal void setCharging(BattleUnit chargeTarget) =>
	    _charging = chargeTarget;

    /**
     * Get the unit's aggression.
     * @return aggression.
     */
    internal int getAggression() =>
	    _aggression;

    /**
     * Get a grenade from the belt (used for AI)
     * @return Pointer to item.
     */
    internal BattleItem getGrenadeFromBelt()
    {
	    foreach (var i in _inventory)
	    {
		    if (i.getRules().getBattleType() == BattleType.BT_GRENADE)
		    {
			    return i;
		    }
	    }
	    return null;
    }

    /**
     * Look at a direction.
     * @param direction Direction to look at.
     * @param force True to reset the direction, false to animate to it.
     */
    internal void lookAt(int direction, bool force = false)
    {
	    if (!force)
	    {
		    if (direction < 0 || direction >= 8) return;
		    _toDirection = direction;
		    if (_toDirection != _direction)
		    {
			    _status = UnitStatus.STATUS_TURNING;
		    }
	    }
	    else
	    {
		    _toDirection = direction;
		    _direction = direction;
	    }
    }

    /**
     * Adds one to the psi skill exp counter.
     */
    internal void addPsiSkillExp() =>
	    _expPsiSkill++;

    /**
     * Adds one to the psi strength exp counter.
     */
    internal void addPsiStrengthExp() =>
	    _expPsiStrength++;

    /**
     * Sets the unit mind controller's id.
     * @param int mind controller id.
     */
    internal void setMindControllerId(int id) =>
	    _mindControllerID = id;

    /**
     * Mark this unit as reselectable.
     */
    internal void allowReselect() =>
	    _dontReselect = false;

    /**
     * Get the unit's death sounds.
     * @return List of sound IDs.
     */
    internal List<int> getDeathSounds()
    {
	    if (!_deathSound.Any() && _geoscapeSoldier != null)
	    {
		    if (_gender == SoldierGender.GENDER_MALE)
			    return _geoscapeSoldier.getRules().getMaleDeathSounds();
		    else
			    return _geoscapeSoldier.getRules().getFemaleDeathSounds();
	    }
	    return _deathSound;
    }

    /**
     * Gets the BattleUnit's position.
     * @return position
     */
    internal Position getLastPosition() =>
	    _lastPos;

    /**
     * Gets the walking phase for animation and sound.
     * @return phase will always go from 0-7
     */
    internal int getWalkingPhase() =>
	    _walkPhase % 8;

    /**
     * Raises a unit's stun level sufficiently so that the unit is ready to become unconscious.
     * Used when another unit falls on top of this unit.
     * Zombified units first convert to their spawn unit.
     * @param battle Pointer to the battlescape game.
     */
    internal void knockOut(BattlescapeGame battle)
    {
	    if (!string.IsNullOrEmpty(_spawnUnit))
	    {
		    setRespawn(false);
		    BattleUnit newUnit = battle.convertUnit(this);
		    newUnit.knockOut(battle);
	    }
	    else
	    {
		    _stunlevel = _health;
	    }
    }

    /**
     * This will increment the walking phase.
     * @param tileBelowMe Pointer to tile currently below the unit.
     * @param cache Refresh the unit cache.
     */
    internal void keepWalking(Tile tileBelowMe, bool cache)
    {
	    int middle, end;
	    if (_verticalDirection != 0)
	    {
		    middle = 4;
		    end = 8;
	    }
	    else
	    {
		    // diagonal walking takes double the steps
		    middle = 4 + 4 * (_direction % 2);
		    end = 8 + 8 * (_direction % 2);
		    if (_armor.getSize() > 1)
		    {
			    if (_direction < 1 || _direction > 5)
				    middle = end;
			    else if (_direction == 5)
				    middle = 12;
			    else if (_direction == 1)
				    middle = 5;
			    else
				    middle = 1;
		    }
	    }
	    if (!cache)
	    {
		    _pos = _destination;
		    end = 2;
	    }

	    _walkPhase++;

	    if (_walkPhase == middle)
	    {
		    // we assume we reached our destination tile
		    // this is actually a drawing hack, so soldiers are not overlapped by floortiles
		    _pos = _destination;
	    }

	    if (_walkPhase >= end)
	    {
		    if (_floating && !_tile.hasNoFloor(tileBelowMe))
		    {
			    _floating = false;
		    }
		    // we officially reached our destination tile
		    _status = UnitStatus.STATUS_STANDING;
		    _walkPhase = 0;
		    _verticalDirection = 0;
		    if (_faceDirection >= 0) {
			    // Finish strafing move facing the correct way.
			    _direction = _faceDirection;
			    _faceDirection = -1;
		    }

		    // motion points calculation for the motion scanner blips
		    if (_armor.getSize() > 1)
		    {
			    _motionPoints += 30;
		    }
		    else
		    {
			    // sectoids actually have less motion points
			    // but instead of create yet another variable,
			    // I used the height of the unit instead (logical)
			    if (getStandHeight() > 16)
				    _motionPoints += 4;
			    else
				    _motionPoints += 3;
		    }
	    }

	    _cacheInvalid = cache;
    }

    /**
     * Gets the BattleUnit's destination.
     * @return destination
     */
    internal Position getDestination() =>
	    _destination;

    /**
     * Gets the BattleUnit's (horizontal) face direction.
     * Used only during strafing moves.
     * @return face direction
     */
    internal int getFaceDirection() =>
	    _faceDirection;

    /**
     * Changes the BattleUnit's (horizontal) face direction.
     * Only used for strafing moves.
     * @param direction new face direction
     */
    internal void setFaceDirection(int direction) =>
	    _faceDirection = direction;

    /**
     * Spend energy  if it can. Return false if it can't.
     * @param tu
     * @return flag if it could spend the time units or not.
     */
    internal bool spendEnergy(int tu)
    {
	    int eu = tu / 2;

	    if (eu <= _energy)
	    {
		    _energy -= eu;
		    return true;
	    }
	    else
	    {
		    return false;
	    }
    }

    /**
     * Is floating? A unit is floating when there is no ground under him/her.
     * @return true/false
     */
    internal bool isFloating() =>
	    _floating;

    /**
     * Get the unit's move sound.
     * @return id.
     */
    internal int getMoveSound() =>
	    _moveSound;

	/// Checks if this unit is in hiding for a turn.
	internal bool isHiding() =>
        _hidingForTurn;

    /**
     * Heal a fatal wound of the soldier
     * @param part the body part to heal
     * @param woundAmount the amount of fatal wound healed
     * @param healthAmount The amount of health to add to soldier health
     */
    internal void heal(int part, int woundAmount, int healthAmount)
    {
	    if (part < 0 || part > 5 || _fatalWounds[part] == 0)
	    {
		    return;
	    }

	    _fatalWounds[part] -= woundAmount;
	    if (_fatalWounds[part] < 0)
	    {
		    _fatalWounds[part] = 0;
	    }

	    _health += healthAmount;
	    if (_health > getBaseStats().health)
	    {
		    _health = getBaseStats().health;
	    }
    }

    /**
     * Restore soldier energy and reduce stun level
     * @param energy The amount of energy to add
     * @param s The amount of stun level to reduce
     */
    internal void stimulant(int energy, int s)
    {
	    _energy += energy;
	    if (_energy > getBaseStats().stamina)
		    _energy = getBaseStats().stamina;
	    healStun(s);
    }

    /**
     * Restore soldier morale
     */
    internal void painKillers()
    {
	    int lostHealth = getBaseStats().health - _health;
	    if (lostHealth > _moraleRestored)
	    {
		    _morale = Math.Min(100, (lostHealth - _moraleRestored + _morale));
		    _moraleRestored = lostHealth;
	    }
    }

    /**
     * Set unit's active hand.
     * @param hand active hand.
     */
    internal void setActiveHand(string hand)
    {
	    if (_activeHand != hand) _cacheInvalid = true;
	    _activeHand = hand;
    }

    /**
     * Checks if the floor above flag has been set.
     * @return if we're under cover.
     */
    internal bool getFloorAbove() =>
	    _floorAbove;

    /**
     * Decides if we should start producing bubbles, and/or updates which bubble frame we are on.
     */
    internal void breathe()
    {
	    // _breathFrame of -1 means this unit doesn't produce bubbles
	    if (_breathFrame < 0 || isOut())
	    {
		    _breathing = false;
		    return;
	    }

	    if (!_breathing || _status == UnitStatus.STATUS_WALKING)
	    {
		    // deviation from original: TFTD used a static 10% chance for every animation frame,
		    // instead let's use 5%, but allow morale to affect it.
		    _breathing = (_status != UnitStatus.STATUS_WALKING && RNG.seedless(0, 99) < (105 - _morale));
		    _breathFrame = 0;
	    }

	    if (_breathing)
	    {
		    // advance the bubble frame
		    _breathFrame++;

		    // we've reached the end of the cycle, get rid of the bubbles
		    if (_breathFrame >= 17)
		    {
			    _breathFrame = 0;
			    _breathing = false;
		    }
	    }
    }

    /**
     * Get the amount of fatal wound for a body part
     * @param part The body part (in the range 0-5)
     * @return The amount of fatal wound of a body part
     */
    internal int getFatalWound(int part)
    {
	    if (part < 0 || part > 5)
		    return 0;
	    return _fatalWounds[part];
    }

    /**
     * Gets values used for recoloring sprites.
     * @param i what value choose.
     * @return Pairs of value, where first is color group to replace and second is new color group with shade.
     */
    internal List<KeyValuePair<byte, byte>> getRecolor() =>
	    _recolor;

    /**
     * If this unit is breathing, what frame should be displayed?
     * @return frame number.
     */
    internal int getBreathFrame()
    {
	    if (_floorAbove)
		    return 0;
	    return _breathFrame;
    }

    /**
     * Sets the flag for "this unit is under cover" meaning don't draw bubbles.
     * @param floor is there a floor.
     */
    internal void setFloorAbove(bool floor) =>
	    _floorAbove = floor;

    /**
     * Gets the BattleUnit's vertical direction. This is when going up or down.
     * @return direction
     */
    internal int getVerticalDirection() =>
	    _verticalDirection;

    /**
     * Gets the walking phase for diagonal walking.
     * @return phase this will be 0 or 8
     */
    internal int getDiagonalWalkingPhase() =>
	    (_walkPhase / 8) * 8;

    /**
     * Get the unit's minimap sprite index. Used to display the unit on the minimap
     * @return the unit minimap index
     */
    internal int getMiniMapSpriteIndex()
    {
	    //minimap sprite index:
	    // * 0-2   : Xcom soldier
	    // * 3-5   : Alien
	    // * 6-8   : Civilian
	    // * 9-11  : Item
	    // * 12-23 : Xcom HWP
	    // * 24-35 : Alien big terror unit(cyberdisk, ...)
	    if (isOut())
	    {
		    return 9;
	    }
	    switch (getFaction())
	    {
	        case UnitFaction.FACTION_HOSTILE:
		        if (_armor.getSize() == 1)
			        return 3;
		        else
			        return 24;
	        case UnitFaction.FACTION_NEUTRAL:
		        if (_armor.getSize() == 1)
			        return 6;
		        else
			        return 12;
	        default:
		        if (_armor.getSize() == 1)
			        return 0;
		        else
			        return 12;
	    }
    }

    /**
     * Get motion points for the motion scanner. More points
     * is a larger blip on the scanner.
     * @return points.
     */
    internal int getMotionPoints() =>
	    _motionPoints;

    /**
     * Returns the phase of the falling sequence.
     * @return phase
     */
    internal int getFallingPhase() =>
	    _fallPhase;

    /**
     * Loads the unit from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    _id = int.Parse(node["id"].ToString());
	    _faction = _originalFaction = (UnitFaction)int.Parse(node["faction"].ToString());
	    _status = (UnitStatus)int.Parse(node["status"].ToString());
        _pos = Position.decode(node["position"]);
	    _direction = _toDirection = int.Parse(node["direction"].ToString());
	    _directionTurret = _toDirectionTurret = int.Parse(node["directionTurret"].ToString());
	    _tu = int.Parse(node["tu"].ToString());
	    _health = int.Parse(node["health"].ToString());
	    _stunlevel = int.Parse(node["stunlevel"].ToString());
	    _energy = int.Parse(node["energy"].ToString());
	    _morale = int.Parse(node["morale"].ToString());
	    _kneeled = bool.Parse(node["kneeled"].ToString());
	    _floating = bool.Parse(node["floating"].ToString());
	    for (int i=0; i < 5; i++)
		    _currentArmor[i] = int.Parse(node["armor"][i].ToString());
	    for (int i=0; i < 6; i++)
		    _fatalWounds[i] = int.Parse(node["fatalWounds"][i].ToString());
	    _fire = int.Parse(node["fire"].ToString());
	    _expBravery = int.Parse(node["expBravery"].ToString());
	    _expReactions = int.Parse(node["expReactions"].ToString());
	    _expFiring = int.Parse(node["expFiring"].ToString());
	    _expThrowing = int.Parse(node["expThrowing"].ToString());
	    _expPsiSkill = int.Parse(node["expPsiSkill"].ToString());
	    _expPsiStrength = int.Parse(node["expPsiStrength"].ToString());
	    _expMelee = int.Parse(node["expMelee"].ToString());
	    _turretType = int.Parse(node["turretType"].ToString());
	    _visible = bool.Parse(node["visible"].ToString());
	    _turnsSinceSpotted = int.Parse(node["turnsSinceSpotted"].ToString());
	    _killedBy = (UnitFaction)int.Parse(node["killedBy"].ToString());
	    _moraleRestored = int.Parse(node["moraleRestored"].ToString());
	    _rankInt = int.Parse(node["rankInt"].ToString());
	    _originalFaction = (UnitFaction)int.Parse(node["originalFaction"].ToString());
	    _kills = int.Parse(node["kills"].ToString());
	    _dontReselect = bool.Parse(node["dontReselect"].ToString());
	    _charging = null;
	    _spawnUnit = node["spawnUnit"].ToString();
	    _motionPoints = int.Parse(node["motionPoints"].ToString());
	    _respawn = bool.Parse(node["respawn"].ToString());
	    _activeHand = node["activeHand"].ToString();
	    if (node["tempUnitStatistics"] != null)
	    {
		    _statistics.load(node["tempUnitStatistics"]);
	    }
	    _murdererId = int.Parse(node["murdererId"].ToString());
	    _fatalShotSide = (UnitSide)int.Parse(node["fatalShotSide"].ToString());
	    _fatalShotBodyPart = (UnitBodyPart)int.Parse(node["fatalShotBodyPart"].ToString());
	    _murdererWeapon = node["murdererWeapon"].ToString();
	    _murdererWeaponAmmo = node["murdererWeaponAmmo"].ToString();

	    if (node["recolor"] is YamlSequenceNode p)
	    {
		    _recolor.Clear();
		    for (var i = 0; i < p.Children.Count; ++i)
		    {
			    _recolor.Add(KeyValuePair.Create(byte.Parse(p.Children[i][0].ToString()), byte.Parse(p.Children[i][1].ToString())));
		    }
	    }
	    _mindControllerID = int.Parse(node["mindControllerID"].ToString());
    }

    /**
     * Gets the BattleUnit's turret To direction.
     * @return toDirectionTurret
     */
    int getTurretToDirection() =>
	    _toDirectionTurret;

    /**
     * invalidate cache; call after copying object :(
     */
    void invalidateCache()
    {
	    for (int i = 0; i < 5; ++i) { _cache[i] = null; }
	    _cacheInvalid = true;
    }
}
