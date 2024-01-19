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

/**
 * Represents a craft stored in a base.
 * Contains variable info about a craft like
 * position, fuel, damage, etc.
 * @sa RuleCraft
 */
internal class Craft : MovingTarget
{
    RuleCraft _rules;
    Base _base;
    int _fuel, _damage, _interceptionOrder, _takeoff;
    List<Vehicle> _vehicles;
    string _status;
    bool _lowFuel, _mission, _inBattlescape, _inDogfight;
    List<CraftWeapon> _weapons;
    ItemContainer _items;
    double _speedMaxRadian;

    internal Craft() { }

    /**
     * Initializes a craft of the specified type and
     * assigns it the latest craft ID available.
     * @param rules Pointer to ruleset.
     * @param base Pointer to base of origin.
     * @param id ID to assign to the craft (0 to not assign).
     */
    internal Craft(RuleCraft rules, Base @base, int id) : base()
    {
        _rules = rules;
        _base = @base;
        _fuel = 0;
        _damage = 0;
        _interceptionOrder = 0;
        _takeoff = 0;
        _status = "STR_READY";
        _lowFuel = false;
        _mission = false;
        _inBattlescape = false;
        _inDogfight = false;

        _items = new ItemContainer();
        if (id != 0)
        {
            _id = id;
        }
        for (uint i = 0; i < _rules.getWeapons(); ++i)
        {
            _weapons.Add(new CraftWeapon());
        }
        if (@base != null)
        {
            setBase(@base);
        }
        _speedMaxRadian = calculateRadianSpeed(_rules.getMaxSpeed()) * 120;
    }

    /**
     * Delete the contents of the craft from memory.
     */
    ~Craft()
    {
        _weapons.Clear();
        _items = null;
        _vehicles.Clear();
    }

    /**
     * Changes the base the craft belongs to.
     * @param base Pointer to base.
     * @param move Move the craft to the base coordinates.
     */
    internal void setBase(Base @base, bool move = true)
    {
        _base = @base;
        if (move)
        {
            _lon = getLongitude();
            _lat = getLatitude();
        }
    }

    /**
     * Sends the craft back to its origin base.
     */
    internal void returnToBase() =>
        setDestination(_base);

    /**
     * Changes the destination the craft is heading to.
     * @param dest Pointer to new destination.
     */
    void setDestination(Target dest)
    {
        if (_status != "STR_OUT")
        {
            _takeoff = 60;
        }
        if (dest == null)
            setSpeed(_rules.getMaxSpeed() / 2);
        else
            setSpeed(_rules.getMaxSpeed());
        base.setDestination(dest);
    }

    /**
     * Returns the globe marker for the craft.
     * @return Marker sprite, -1 if none.
     */
    internal override int getMarker()
    {
	    if (_status != "STR_OUT")
		    return -1;
	    else if (_rules.getMarker() == -1)
		    return 1;
        return _rules.getMarker();
    }

    /**
     * Changes the ruleset for the craft's type.
     * @param rules Pointer to ruleset.
     * @warning ONLY FOR NEW BATTLE USE!
     */
    internal void changeRules(RuleCraft rules)
    {
        _rules = rules;
        _weapons.Clear();
        for (int i = 0; i < _rules.getWeapons(); ++i)
        {
            _weapons.Add(new CraftWeapon());
        }
    }

    /**
     * Returns the list of weapons currently equipped
     * in the craft.
     * @return Pointer to weapon list.
     */
    internal List<CraftWeapon> getWeapons() =>
        _weapons;

    /**
     * Returns the list of vehicles currently equipped
     * in the craft.
     * @return Pointer to vehicle list.
     */
    internal List<Vehicle> getVehicles() =>
        _vehicles;

    /**
     * Returns the list of items in the craft.
     * @return Pointer to the item list.
     */
    internal ItemContainer getItems() =>
        _items;

    /**
     * Loads the craft from a YAML file.
     * @param node YAML node.
     * @param mod Mod for the saved game.
     * @param save Pointer to the saved game.
     */
    internal void load(YamlNode node, Mod.Mod mod, SavedGame save)
    {
	    base.load(node);
	    _fuel = int.Parse(node["fuel"].ToString());
	    _damage = int.Parse(node["damage"].ToString());

        int j = 0;
	    foreach (var i in (YamlSequenceNode)node["weapons"])
	    {
		    if (_rules.getWeapons() > j)
		    {
			    string type = i["type"].ToString();
			    if (type != "0" && mod.getCraftWeapon(type) != null)
			    {
				    CraftWeapon w = new CraftWeapon(mod.getCraftWeapon(type), 0);
				    w.load(i);
				    _weapons[j] = w;
			    }
			    else
			    {
				    _weapons[j] = null;
				    if (type != "0")
				    {
                        Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load craft weapon {type}");
				    }
			    }
			    j++;
		    }
	    }

	    _items.load(node["items"]);
        // Some old saves have bad items, better get rid of them to avoid further bugs
        var contents = _items.getContents();
        foreach (var i in contents)
	    {
		    if (mod.getItem(i.Key) == null)
		    {
                Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load item {i.Key}");
			    contents.Remove(i.Key);
		    }
		    //else
		    //{
			//    ++i;
		    //}
	    }
	    foreach (var i in (YamlSequenceNode)node["vehicles"])
	    {
		    string type = i["type"].ToString();
		    if (mod.getItem(type) != null)
		    {
			    Vehicle v = new Vehicle(mod.getItem(type), 0, 4);
			    v.load(i);
			    _vehicles.Add(v);
		    }
		    else
		    {
                Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load item {type}");
		    }
	    }
	    _status = node["status"].ToString();
	    _lowFuel = bool.Parse(node["lowFuel"].ToString());
	    _mission = bool.Parse(node["mission"].ToString());
	    _interceptionOrder = int.Parse(node["interceptionOrder"].ToString());
        YamlNode dest = node["dest"];
        if (dest != null)
	    {
		    string type = dest["type"].ToString();
		    int id = int.Parse(dest["id"].ToString());
		    if (type == "STR_BASE")
		    {
			    returnToBase();
		    }
		    else if (type == "STR_UFO")
		    {
			    foreach (var i in save.getUfos())
			    {
				    if (i.getId() == id)
				    {
					    setDestination(i);
					    break;
				    }
			    }
		    }
		    else if (type == "STR_WAY_POINT")
		    {
			    foreach (var i in save.getWaypoints())
			    {
				    if (i.getId() == id)
				    {
					    setDestination(i);
					    break;
				    }
			    }
		    }
		    else
		    {
			    // Backwards compatibility
			    if (type == "STR_ALIEN_TERROR")
				    type = "STR_TERROR_SITE";
			    bool found = false;
                var missionSites = save.getMissionSites();
                for (var i = 0; i < missionSites.Count && !found; ++i)
			    {
				    if (missionSites[i].getId() == id && missionSites[i].getDeployment().getMarkerName() == type)
				    {
					    setDestination(missionSites[i]);
					    found = true;
				    }
			    }
                var alienBases = save.getAlienBases();
                for (var i = 0; i < alienBases.Count && !found; ++i)
			    {
				    if (alienBases[i].getId() == id && alienBases[i].getDeployment().getMarkerName() == type)
				    {
					    setDestination(alienBases[i]);
					    found = true;
				    }
			    }
		    }
	    }
	    _takeoff = int.Parse(node["takeoff"].ToString());
	    _inBattlescape = bool.Parse(node["inBattlescape"].ToString());
	    if (_inBattlescape)
		    setSpeed(0);
    }

    /**
     * Returns the ruleset for the craft's type.
     * @return Pointer to ruleset.
     */
    internal RuleCraft getRules() =>
	    _rules;

    /**
     * Returns the current status of the craft.
     * @return Status string.
     */
    internal string getStatus() =>
	    _status;

    /**
     * Repairs the craft's damage every hour
     * while it's docked in the base.
     */
    internal void repair()
    {
        setDamage(_damage - _rules.getRepairRate());
        if (_damage <= 0)
        {
            _status = "STR_REARMING";
        }
    }

    /**
     * Changes the amount of damage this craft has taken.
     * @param damage Amount of damage.
     */
    internal void setDamage(int damage)
    {
        _damage = damage;
        if (_damage < 0)
        {
            _damage = 0;
        }
    }

    /**
     * Rearms the craft's weapons by adding ammo every hour
     * while it's docked in the base.
     * @param mod Pointer to mod.
     * @return The ammo ID missing for rearming, or "" if none.
     */
    internal string rearm(Mod.Mod mod)
    {
	    string ammo = null;
	    for (int i = 0; ; ++i)
	    {
		    if (i == _weapons.Count)
		    {
			    _status = "STR_REFUELLING";
			    break;
		    }
		    if (_weapons[i] != null && _weapons[i].isRearming())
		    {
			    string clip = _weapons[i].getRules().getClipItem();
			    int available = _base.getStorageItems().getItem(clip);
			    if (string.IsNullOrEmpty(clip))
			    {
                    _weapons[i].rearm(0, 0);
			    }
			    else if (available > 0)
			    {
				    int used = _weapons[i].rearm(available, mod.getItem(clip).getClipSize());

				    if (used == available && _weapons[i].isRearming())
				    {
					    ammo = clip;
                        _weapons[i].setRearming(false);
				    }

				    _base.getStorageItems().removeItem(clip, used);
			    }
			    else
			    {
				    ammo = clip;
                    _weapons[i].setRearming(false);
			    }
			    break;
		    }
	    }
	    return ammo;
    }

    /**
     * Checks the condition of all the craft's systems
     * to define its new status (eg. when arriving at base).
     */
    internal void checkup()
    {
        int available = 0, full = 0;
        foreach (var i in _weapons)
        {
            if (i == null)
                continue;
            available++;
            if (i.getAmmo() >= i.getRules().getAmmoMax())
            {
                full++;
            }
            else
            {
                i.setRearming(true);
            }
        }

        if (_damage > 0)
        {
            _status = "STR_REPAIRS";
        }
        else if (available != full)
        {
            _status = "STR_REARMING";
        }
        else
        {
            _status = "STR_REFUELLING";
        }
    }

    /**
     * Changes the current status of the craft.
     * @param status Status string.
     */
    internal void setStatus(string status) =>
	    _status = status;

    /**
     * Checks if an item can be reused by the craft and
     * updates its status appropriately.
     * @param item Item ID.
     */
    internal void reuseItem(string item)
    {
	    if (_status != "STR_READY")
		    return;
	    // Check if it's ammo to reload the craft
	    foreach (var w in _weapons)
	    {
		    if (w != null && item == w.getRules().getClipItem() && w.getAmmo() < w.getRules().getAmmoMax())
		    {
			    w.setRearming(true);
			    _status = "STR_REARMING";
		    }
	    }
	    // Check if it's fuel to refuel the craft
	    if (item == _rules.getRefuelItem() && _fuel < _rules.getMaxFuel())
		    _status = "STR_REFUELLING";
    }

    /**
     * Unloads all the craft contents to the base.
     * @param mod Pointer to mod.
     */
    internal void unload(Mod.Mod mod)
    {
	    // Remove weapons
	    foreach (var w in _weapons)
	    {
		    if (w != null)
		    {
			    _base.getStorageItems().addItem(w.getRules().getLauncherItem());
			    _base.getStorageItems().addItem(w.getRules().getClipItem(), w.getClipsLoaded(mod));
		    }
	    }

	    // Remove items
	    foreach (var it in _items.getContents())
	    {
		    _base.getStorageItems().addItem(it.Key, it.Value);
	    }

	    // Remove vehicles
	    foreach (var v in _vehicles)
	    {
		    _base.getStorageItems().addItem(v.getRules().getType());
		    if (v.getRules().getCompatibleAmmo().Any())
		    {
			    _base.getStorageItems().addItem(v.getRules().getCompatibleAmmo().First(), v.getAmmo());
		    }
	    }
	    _vehicles.Clear();

	    // Remove soldiers
	    foreach (var s in _base.getSoldiers())
	    {
		    if (s.getCraft() == this)
		    {
			    s.setCraft(null);
		    }
	    }
    }

    /**
     * Returns if a certain target is inside the craft's
     * radar range, taking in account the positions of both.
     * @param target Pointer to target to compare.
     * @return True if inside radar range.
     */
    internal bool insideRadarRange(Target target)
    {
	    double range = Nautical(_rules.getRadarRange());
	    return (getDistance(target) <= range);
    }

    /**
     * Returns if a certain target is detected by the craft's
     * radar, taking in account the range and chance.
     * @param target Pointer to target to compare.
     * @return True if it's detected, False otherwise.
     */
    internal bool detect(Target target)
    {
	    if (_rules.getRadarRange() == 0 || !insideRadarRange(target))
		    return false;

	    // backward compatibility with vanilla
	    if (_rules.getRadarChance() == 100)
		    return true;

	    Ufo u = (Ufo)target;
	    int chance = _rules.getRadarChance() * (100 + u.getVisibility()) / 100;
	    return RNG.percent(chance);
    }

    /**
     * Refuels the craft every 30 minutes
     * while it's docked in the base.
     * @return The item ID missing for refuelling, or "" if none.
     */
    internal string refuel()
    {
        string fuel = null;
        if (_fuel < _rules.getMaxFuel())
        {
            string item = _rules.getRefuelItem();
            if (string.IsNullOrEmpty(item))
            {
                setFuel(_fuel + _rules.getRefuelRate());
            }
            else
            {
                if (_base.getStorageItems().getItem(item) > 0)
                {
                    _base.getStorageItems().removeItem(item);
                    setFuel(_fuel + _rules.getRefuelRate());
                    _lowFuel = false;
                }
                else if (!_lowFuel)
                {
                    fuel = item;
                    if (_fuel > 0)
                    {
                        _status = "STR_READY";
                    }
                    else
                    {
                        _lowFuel = true;
                    }
                }
            }
        }
        if (_fuel >= _rules.getMaxFuel())
        {
            _status = "STR_READY";
            foreach (var i in _weapons)
            {
                if (i != null && i.isRearming())
                {
                    _status = "STR_REARMING";
                    break;
                }
            }
        }
        return fuel;
    }

    /**
     * Changes the amount of fuel currently contained
     * in this craft.
     * @param fuel Amount of fuel.
     */
    void setFuel(int fuel)
    {
        _fuel = fuel;
        if (_fuel > _rules.getMaxFuel())
        {
            _fuel = _rules.getMaxFuel();
        }
        else if (_fuel < 0)
        {
            _fuel = 0;
        }
    }

    /**
     * Returns the amount of fuel currently contained
     * in this craft.
     * @return Amount of fuel.
     */
    internal int getFuel() =>
	    _fuel;

    /**
     * Returns whether the craft is currently low on fuel
     * (only has enough to head back to base).
     * @return True if it's low, false otherwise.
     */
    internal bool getLowFuel() =>
	    _lowFuel;

    /**
     * Changes whether the craft is currently low on fuel
     * (only has enough to head back to base).
     * @param low True if it's low, false otherwise.
     */
    internal void setLowFuel(bool low) =>
        _lowFuel = low;

    /**
     * Returns the minimum required fuel for the
     * craft to make it back to base.
     * @return Fuel amount.
     */
    internal int getFuelLimit() =>
	    getFuelLimit(_base);

    /**
     * Returns the minimum required fuel for the
     * craft to go to a base.
     * @param base Pointer to target base.
     * @return Fuel amount.
     */
    internal int getFuelLimit(Base @base) =>
        (int)Math.Floor(getFuelConsumption(_rules.getMaxSpeed()) * getDistance(@base) / _speedMaxRadian);

    /**
     * Consumes the craft's fuel every 10 minutes
     * while it's on the air.
     */
    internal void consumeFuel() =>
        setFuel(_fuel - getFuelConsumption());

    /**
     * Returns the amount of fuel the craft uses up
     * while it's on the air, based on its current speed.
     * @return Fuel amount.
     */
    int getFuelConsumption() =>
	    getFuelConsumption(_speed);

    /**
     * Returns the amount of fuel the craft uses up
     * while it's on the air.
     * @param speed Craft speed for estimation.
     * @return Fuel amount.
     */
    int getFuelConsumption(int speed)
    {
	    if (!string.IsNullOrEmpty(_rules.getRefuelItem()))
		    return 1;
	    return (int)Math.Floor(speed / 100.0);
    }

    /**
     * Returns the amount of soldiers from a list
     * that are currently attached to this craft.
     * @return Number of soldiers.
     */
    internal int getNumSoldiers()
    {
	    if (_rules.getSoldiers() == 0)
		    return 0;

	    int total = 0;

	    foreach (var i in _base.getSoldiers())
	    {
		    if (i.getCraft() == this)
			    total++;
	    }

	    return total;
    }

    /**
     * Returns the amount of vehicles currently
     * contained in this craft.
     * @return Number of vehicles.
     */
    internal int getNumVehicles() =>
	    _vehicles.Count;

    /**
     * Returns the craft's dogfight status.
     * @return Is the craft ion a dogfight?
     */
    internal bool isInDogfight() =>
	    _inDogfight;

    /**
     * Changes the craft's dogfight status.
     * @param inDogfight True if it's in dogfight, False otherwise.
     */
    internal void setInDogfight(bool inDogfight) =>
        _inDogfight = inDogfight;

    /// Returns the craft destroyed status.
    /**
     * If the amount of damage the craft take
     * is more than it's health it will be
     * destroyed.
     * @return Is the craft destroyed?
     */
    internal bool isDestroyed() =>
	    (_damage >= _rules.getMaxDamage());

    /**
     * Moves the craft to its destination.
     */
    internal void think()
    {
        if (_takeoff == 0)
        {
            move();
        }
        else
        {
            _takeoff--;
            resetMeetPoint();
        }
        if (reachedDestination() && _dest == (Target)_base)
        {
            setInterceptionOrder(0); // just to be sure
            checkup();
            setDestination(null);
            setSpeed(0);
            _lowFuel = false;
            _mission = false;
            _takeoff = 0;
        }
    }

    /**
     * Sets interception order (first craft to leave the base gets 1, second 2, etc.).
     * @param order Interception order.
     */
    internal void setInterceptionOrder(int order) =>
	    _interceptionOrder = order;

    /**
     * Returns the base the craft belongs to.
     * @return Pointer to base.
     */
    internal Base getBase() =>
	    _base;

    /**
     * Changes the craft's battlescape status.
     * @param inbattle True if it's in battle, False otherwise.
     */
    internal void setInBattlescape(bool inbattle)
    {
        if (inbattle)
            setSpeed(0);
        _inBattlescape = inbattle;
    }

    /**
     * Gets interception order.
     * @return Interception order.
     */
    internal int getInterceptionOrder() =>
	    _interceptionOrder;

    /**
     * Returns the ratio between the amount of damage this
     * craft can take and the total it can take before it's
     * destroyed.
     * @return Percentage of damage.
     */
    internal int getDamagePercentage() =>
	    (int)Math.Floor((double)_damage / _rules.getMaxDamage() * 100);

    /**
     * Returns whether the craft has just done a ground mission,
     * and is forced to return to base.
     * @return True if it's returning, false otherwise.
     */
    internal bool getMissionComplete() =>
	    _mission;

    /**
     * Returns the amount of weapons currently
     * equipped on this craft.
     * @return Number of weapons.
     */
    internal int getNumWeapons()
    {
	    if (_rules.getWeapons() == 0)
	    {
		    return 0;
	    }

	    int total = 0;

	    foreach (var i in _weapons)
	    {
		    if (i != null)
		    {
			    total++;
		    }
	    }

	    return total;
    }

    /**
     * Returns the current altitude of the craft.
     * @return Altitude.
     */
    internal string getAltitude()
    {
	    Ufo u = (Ufo)_dest;
	    if (u != null && u.getAltitude() != "STR_GROUND")
	    {
		    return u.getAltitude();
	    }
	    else
	    {
		    return "STR_VERY_LOW";
	    }
    }

    /**
     * Returns the ratio between the amount of fuel currently
     * contained in this craft and the total it can carry.
     * @return Percentage of fuel.
     */
    internal int getFuelPercentage() =>
	    (int)Math.Floor((double)_fuel / _rules.getMaxFuel() * 100.0);

    /**
     * Returns the maximum range the craft can travel
     * from its origin base on its current fuel.
     * @return Range in radians.
     */
    internal double getBaseRange() =>
	    _fuel / 2.0 / getFuelConsumption(_rules.getMaxSpeed()) * _speedMaxRadian;

    /**
     * Returns the amount of space available for
     * soldiers and vehicles.
     * @return Space available.
     */
    internal int getSpaceAvailable() =>
	    _rules.getSoldiers() - getSpaceUsed();

    /**
     * Returns the amount of space in use by
     * soldiers and vehicles.
     * @return Space used.
     */
    internal int getSpaceUsed()
    {
	    int vehicleSpaceUsed = 0;
	    foreach (var i in _vehicles)
	    {
		    vehicleSpaceUsed += i.getSize();
	    }
	    return getNumSoldiers() + vehicleSpaceUsed;
    }

    /**
     * Returns the total amount of vehicles of
     * a certain type stored in the craft.
     * @param vehicle Vehicle type.
     * @return Number of vehicles.
     */
    internal int getVehicleCount(string vehicle)
    {
	    int total = 0;
	    foreach (var i in _vehicles)
	    {
		    if (i.getRules().getType() == vehicle)
		    {
			    total++;
		    }
	    }
	    return total;
    }

    /**
     * Gets the craft's unique id.
     * @return A tuple of the craft's type and per-type id.
     */
    internal KeyValuePair<string, int> getUniqueId() =>
        KeyValuePair.Create(_rules.getType(), _id);

    /**
     * Returns the amount of damage this craft has taken.
     * @return Amount of damage.
     */
    internal int getDamage() =>
	    _damage;

    /**
     * Returns the amount of equipment currently
     * equipped on this craft.
     * @return Number of items.
     */
    internal int getNumEquipment() =>
	    _items.getTotalQuantity();

    /**
     * Returns the craft's battlescape status.
     * @return Is the craft currently in battle?
     */
    internal bool isInBattlescape() =>
	    _inBattlescape;

    /**
     * Changes whether the craft has just done a ground mission,
     * and is forced to return to base.
     * @param mission True if it's returning, false otherwise.
     */
    internal void setMissionComplete(bool mission) =>
	    _mission = mission;

    /**
     * Returns the craft's unique type used for
     * savegame purposes.
     * @return ID.
     */
    protected override string getType() =>
	    _rules.getType();

    /**
     * Returns the craft's unique default name.
     * @param lang Language to get strings from.
     * @return Full name.
     */
    internal override string getDefaultName(Language lang) =>
	    lang.getString("STR_CRAFTNAME").arg(lang.getString(getType())).arg(_id);
}
