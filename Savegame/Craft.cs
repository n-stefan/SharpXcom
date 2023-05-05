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
    void setDamage(int damage)
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
}
