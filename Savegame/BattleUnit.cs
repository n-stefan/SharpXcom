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

    //TODO: ctor, dtor

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

    int getActionTUs(BattleActionType actionType, RuleItem item)
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
	string getActiveHand()
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
    internal BattleItem getMainHandWeapon(bool quickest)
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
    int getStandHeight() =>
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
    internal BattleItem getSpecialWeapon(Mod.BattleType type)
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
            { "position", _pos.save() },
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
}
