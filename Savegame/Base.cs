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
 * Represents a player base on the globe.
 * Bases can contain facilities, personnel, crafts and equipment.
 */
internal class Base : Target
{
    Mod.Mod _mod;
    int _scientists, _engineers;
    bool _inBattlescape;
    bool _retaliationTarget;
    ItemContainer _items;
    List<BaseFacility> _facilities;
    List<Soldier> _soldiers;
    List<Craft> _crafts;
    List<Transfer> _transfers;
    List<Production> _productions;
    List<ResearchProject> _research;
    List<Vehicle> _vehicles;
    List<BaseFacility> _defenses;

    /**
     * Initializes an empty base.
     * @param mod Pointer to mod.
     */
    internal Base(Mod.Mod mod) : base()
    {
        _mod = mod;
        _scientists = 0;
        _engineers = 0;
        _inBattlescape = false;
        _retaliationTarget = false;

        _items = new ItemContainer();
    }

    /**
     * Deletes the contents of the base from memory.
     */
    ~Base()
    {
        _facilities.Clear();
        _soldiers.Clear();
        _crafts.Clear();
        _transfers.Clear();
        _productions.Clear();
        _items = null;
        _research.Clear();
        _vehicles.Clear();
    }

    /**
     * Returns the list of soldiers in the base.
     * @return Pointer to the soldier list.
     */
    internal List<Soldier> getSoldiers() =>
        _soldiers;

    /**
     * Saves the base to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = (YamlMappingNode)base.save();
        node.Add("facilities", new YamlSequenceNode(_facilities.Select(x => x.save())));
        node.Add("soldiers", new YamlSequenceNode(_soldiers.Select(x => x.save())));
        node.Add("crafts", new YamlSequenceNode(_crafts.Select(x => x.save())));
        node.Add("items", _items.save());
	    node.Add("scientists", _scientists.ToString());
        node.Add("engineers", _engineers.ToString());
	    if (_inBattlescape)
		    node.Add("inBattlescape", _inBattlescape.ToString());
        node.Add("transfers", new YamlSequenceNode(_transfers.Select(x => x.save())));
        node.Add("research", new YamlSequenceNode(_research.Select(x => x.save())));
        node.Add("productions", new YamlSequenceNode(_productions.Select(x => x.save())));
	    if (_retaliationTarget)
		    node.Add("retaliationTarget", _retaliationTarget.ToString());
        return node;
    }

    /**
     * Returns the list of crafts in the base.
     * @return Pointer to the craft list.
     */
    internal List<Craft> getCrafts() =>
        _crafts;

    /**
     * Returns the globe marker for the base.
     * @return Marker sprite, -1 if none.
     */
    protected override int getMarker()
    {
	    // Cheap hack to hide bases when they haven't been placed yet
	    if (AreSame(_lon, 0.0) && AreSame(_lat, 0.0))
		    return -1;
	    return 0;
    }

    /**
     * Returns the list of facilities in the base.
     * @return Pointer to the facility list.
     */
    internal List<BaseFacility> getFacilities() =>
        _facilities;

    /**
     * Changes the amount of engineers currently in the base.
     * @param engineers Number of engineers.
     */
    internal void setEngineers(int engineers) =>
        _engineers = engineers;

    /**
     * Changes the amount of scientists currently in the base.
     * @param scientists Number of scientists.
     */
    internal void setScientists(int scientists) =>
        _scientists = scientists;

    /**
     * Returns the list of items in the base storage rooms.
     * Does NOT return items assigned to craft or in transfer.
     * @return Pointer to the item list.
     */
    internal ItemContainer getStorageItems() =>
        _items;

    /**
     * Returns the list of transfers destined
     * to this base.
     * @return Pointer to the transfer list.
     */
    internal List<Transfer> getTransfers() =>
        _transfers;

    /**
     * Returns the list of all base's ResearchProject
     * @return list of base's ResearchProject
     */
    internal List<ResearchProject> getResearch() =>
	    _research;

    /**
     * Returns the amount of scientists currently in the base.
     * @return Number of scientists.
     */
    internal int getScientists() =>
	    _scientists;

    /**
     * Add A new ResearchProject to Base
     * @param project The project to add
     */
    internal void addResearch(ResearchProject project) =>
        _research.Add(project);

    /**
     * Add a new Production to the Base
     * @param p A pointer to a Production
     */
    internal void addProduction(Production p) =>
        _productions.Add(p);

    /**
     * Returns the amount of engineers currently in the base.
     * @return Number of engineers.
     */
    internal int getEngineers() =>
	    _engineers;

    /**
     * Mark the base as a valid alien retaliation target.
     * @param mark Mark (if @c true) or unmark (if @c false) the base.
     */
    internal void setRetaliationTarget(bool mark) =>
        _retaliationTarget = mark;

    /**
     * Returns the total amount of all the maintenance
     * monthly costs in the base.
     * @return Maintenance costs.
     */
    internal int getMonthlyMaintenace() =>
	    getCraftMaintenance() + getPersonnelMaintenance() + getFacilityMaintenance();

    /**
     * Returns the total amount of monthly costs
     * for maintaining the craft in the base.
     * @return Maintenance costs.
     */
    int getCraftMaintenance()
    {
	    int total = 0;
	    foreach (var i in _transfers)
	    {
		    if (i.getType() == TransferType.TRANSFER_CRAFT)
		    {
			    total += i.getCraft().getRules().getRentCost();
		    }
	    }
	    foreach (var i in _crafts)
	    {
		    total += i.getRules().getRentCost();
	    }
	    return total;
    }

    /**
     * Returns the total amount of monthly costs
     * for maintaining the personnel in the base.
     * @return Maintenance costs.
     */
    int getPersonnelMaintenance()
    {
	    int total = 0;
	    foreach (var i in _transfers)
	    {
		    if (i.getType() == TransferType.TRANSFER_SOLDIER)
		    {
			    total += i.getSoldier().getRules().getSalaryCost();
		    }
	    }
	    foreach (var i in _soldiers)
	    {
		    total += i.getRules().getSalaryCost();
	    }
	    total += getTotalEngineers() * _mod.getEngineerCost();
	    total += getTotalScientists() * _mod.getScientistCost();
	    return total;
    }

    /**
     * Returns the total amount of monthly costs
     * for maintaining the facilities in the base.
     * @return Maintenance costs.
     */
    int getFacilityMaintenance()
    {
	    int total = 0;
	    foreach (var i in _facilities)
	    {
		    if (i.getBuildTime() == 0)
		    {
			    total += i.getRules().getMonthlyCost();
		    }
	    }
	    return total;
    }

    /**
     * Returns the total amount of engineers contained
     * in the base.
     * @return Number of engineers.
     */
    int getTotalEngineers()
    {
	    int total = _engineers;
	    foreach (var i in _transfers)
	    {
		    if (i.getType() == TransferType.TRANSFER_ENGINEER)
		    {
			    total += i.getQuantity();
		    }
	    }
	    foreach (var iter in _productions)
	    {
		    total += iter.getAssignedEngineers();
	    }
	    return total;
    }

    /**
     * Returns the total amount of scientists contained
     * in the base.
     * @return Number of scientists.
     */
    int getTotalScientists()
    {
	    int total = _scientists;
	    foreach (var i in _transfers)
	    {
		    if (i.getType() == TransferType.TRANSFER_SCIENTIST)
		    {
			    total += i.getQuantity();
		    }
	    }
	    foreach (var itResearch in getResearch())
	    {
		    total += itResearch.getAssigned();
	    }
	    return total;
    }

    /**
     * Returns the total amount of Psi Lab Space
     * available in the base.
     * @return Psi Lab space.
     */
    internal int getAvailablePsiLabs()
    {
	    int total = 0;
	    foreach (var i in _facilities)
	    {
		    if (i.getBuildTime() == 0)
		    {
			    total += i.getRules().getPsiLaboratories();
		    }
	    }
	    return total;
    }

    /**
     * Remove a ResearchProject from base
     * @param project the project to remove
     */
    internal void removeResearch(ResearchProject project)
    {
        _scientists += project.getAssigned();
        if (_research.Contains(project))
        {
            _research.Remove(project);
        }

        RuleResearch ruleResearch = project.getRules();
        if (!project.isFinished())
        {
            if (ruleResearch.needItem() && ruleResearch.destroyItem())
            {
                getStorageItems().addItem(ruleResearch.getName(), 1);
            }
        }
    }

    /**
     * Returns the custom name for the base.
     * @param lang Language to get strings from (unused).
     * @return Name.
     */
    internal string getName(Language lang = null) =>
	    _name;

    /**
     * Get the list of Base Production's
     * @return the list of Base Production's
     */
    internal List<Production> getProductions() =>
	    _productions;

    /**
     * Remove a Production from the Base
     * @param p A pointer to a Production
     */
    internal void removeProduction(Production p)
    {
        _engineers += p.getAssignedEngineers();
        if (_productions.Contains(p))
        {
            _productions.Remove(p);
        }
    }

    /**
     * Checks if the base's stores are overfull.
     *
     * Supplying an offset will add/subtract to the used capacity before performing the check.
     * A positive offset simulates adding items to the stores, whereas a negative offset
     * can be used to check whether sufficient items have been removed to stop the stores overflowing.
     * @param offset Adjusts the used capacity.
     * @return True if the base's stores are over their limit.
     */
    internal bool storesOverfull(double offset = 0.0)
    {
        int capacity = getAvailableStores() * 100;
        double used = (getUsedStores() + offset) * 100;
        return (int)used > capacity;
    }

    /**
     * Returns the total amount of stores
     * available in the base.
     * @return Storage space.
     */
    int getAvailableStores()
    {
	    int total = 0;
	    foreach (var i in _facilities)
	    {
		    if (i.getBuildTime() == 0)
		    {
			    total += i.getRules().getStorage();
		    }
	    }
	    return total;
    }

    /**
     * Returns the amount of stores used up by equipment in the base,
     * and equipment about to arrive.
     * @return Storage space.
     */
    double getUsedStores()
    {
        double total = _items.getTotalSize(_mod);
        foreach (var i in _crafts)
        {
            total += i.getItems().getTotalSize(_mod);
            foreach (var j in i.getVehicles())
            {
                total += j.getRules().getSize();
            }
        }
        foreach (var i in _transfers)
        {
            if (i.getType() == TransferType.TRANSFER_ITEM)
            {
                total += i.getQuantity() * _mod.getItem(i.getItems(), true).getSize();
            }
            else if (i.getType() == TransferType.TRANSFER_CRAFT)
            {
                Craft craft = i.getCraft();
                total += craft.getItems().getTotalSize(_mod);
            }
        }
        total -= getIgnoredStores();
        return total;
    }

    /**
     * Determines space taken up by ammo clips about to rearm craft.
     * @return Ignored storage space.
     */
    double getIgnoredStores()
    {
        double space = 0;
        foreach (var c in getCrafts())
        {
            if (c.getStatus() == "STR_REARMING")
            {
                foreach (var w in c.getWeapons())
                {
                    if (w != null && w.isRearming())
                    {
                        string clip = w.getRules().getClipItem();
                        int available = getStorageItems().getItem(clip);
                        if (!string.IsNullOrEmpty(clip) && available > 0)
                        {
                            int clipSize = _mod.getItem(clip, true).getClipSize();
                            int needed = 0;
                            if (clipSize > 0)
                            {
                                needed = (w.getRules().getAmmoMax() - w.getAmmo()) / clipSize;
                            }
                            space += Math.Min(available, needed) * _mod.getItem(clip, true).getSize();
                        }
                    }
                }
            }
        }
        return space;
    }

    /**
     * Returns the total amount of craft of
     * a certain type stored in the base.
     * @param craft Craft type.
     * @return Number of craft.
     */
    internal int getCraftCount(string craft)
    {
	    int total = 0;
	    foreach (var i in _transfers)
	    {
		    if (i.getType() == TransferType.TRANSFER_CRAFT && i.getCraft().getRules().getType() == craft)
		    {
			    total++;
		    }
	    }
	    foreach (var i in _crafts)
	    {
		    if (i.getRules().getType() == craft)
		    {
			    total++;
		    }
	    }
	    return total;
    }

    /**
     * Removes the craft and all associations from the base (does not destroy it!).
     * @param craft Pointer to craft.
     * @param unload Unload craft contents before removing.
     */
    internal Craft removeCraft(Craft craft, bool unload)
    {
        // Unload craft
        if (unload)
        {
            craft.unload(_mod);
        }

        // Clear hangar
        foreach (var f in _facilities)
        {
            if (f.getCraft() == craft)
            {
                f.setCraft(null);
                break;
            }
        }

        // Remove craft
        foreach (var c in _crafts)
        {
            if (c == craft)
            {
                _crafts.Remove(c);
                return c;
            }
        }
        return null;
    }

    /**
     * Returns if a certain target is inside the base's
     * radar range, taking in account the positions of both.
     * @param target Pointer to target to compare.
     * @return 0 - outside radar range, 1 - inside conventional radar range, 2 - inside hyper-wave decoder range.
     */
    internal int insideRadarRange(Target target)
    {
	    bool insideRange = false;
	    double distance = getDistance(target) * 60.0 * (180.0 / M_PI);
	    foreach (var i in _facilities)
	    {
		    if (i.getRules().getRadarRange() >= distance && i.getBuildTime() == 0)
		    {
			    if (i.getRules().isHyperwave())
			    {
				    return 2;
			    }
			    insideRange = true;
		    }
	    }

	    return insideRange? 1 : 0;
    }

    /**
     * Returns if a certain target is covered by the base's
     * radar range, taking in account the range and chance.
     * @param target Pointer to target to compare.
     * @return 0 - not detected, 1 - detected by conventional radar, 2 - detected by hyper-wave decoder.
     */
    internal int detect(Target target)
    {
	    int chance = 0;
	    double distance = getDistance(target) * 60.0 * (180.0 / M_PI);
	    foreach (var i in _facilities)
	    {
		    if (i.getRules().getRadarRange() >= distance && i.getBuildTime() == 0)
		    {
			    int radarChance = i.getRules().getRadarChance();
			    if (i.getRules().isHyperwave())
			    {
				    if (radarChance == 100 || RNG.percent(radarChance))
				    {
					    return 2;
				    }
			    }
			    else
			    {
				    chance += radarChance;
			    }
		    }
	    }
	    if (chance == 0) return 0;

	    Ufo u = (Ufo)target;
	    if (u != null)
	    {
		    chance = chance * (100 + u.getVisibility()) / 100;
	    }

	    return RNG.percent(chance)? 1 : 0;
    }

    /**
     * Calculate the detection chance of this base.
     * Big bases without mindshields are easier to detect.
     * @param difficulty The savegame difficulty.
     * @return The detection chance.
     */
    internal uint getDetectionChance()
    {
	    int mindShields = _facilities.Count(isMindShield);
	    int completedFacilities = 0;
	    foreach (var i in _facilities)
	    {
		    if (i.getBuildTime() == 0)
		    {
			    completedFacilities += i.getRules().getSize() * i.getRules().getSize();
		    }
	    }
	    return ((uint)((completedFacilities / 6 + 15) / (mindShields + 1)));
    }

    /**
     * Only fully operational facilities are checked.
     * @param facility Pointer to the facility to check.
     * @return If @a facility can act as a mind shield.
     */
    bool isMindShield(BaseFacility facility)
    {
	    if (facility.getBuildTime() != 0)
	    {
		    // Still building this
		    return false;
	    }
	    return (facility.getRules().isMindShield());
    }

    internal List<BaseFacility> getDefenses() =>
        _defenses;

    /**
     * Returns the amount of soldiers contained
     * in the base without any assignments.
     * @param checkCombatReadiness does what it says on the tin.
     * @return Number of soldiers.
     */
    internal int getAvailableSoldiers(bool checkCombatReadiness)
    {
	    int total = 0;
	    foreach (var i in _soldiers)
	    {
		    if (!checkCombatReadiness && i.getCraft() == null)
		    {
			    total++;
		    }
		    else if (checkCombatReadiness && ((i.getCraft() != null && i.getCraft().getStatus() != "STR_OUT") ||
			    (i.getCraft() == null && i.getWoundRecovery() == 0)))
		    {
			    total++;
		    }
	    }
	    return total;
    }

    /**
     * Returns the list of vehicles currently equipped
     * in the base.
     * @return Pointer to vehicle list.
     */
    internal List<Vehicle> getVehicles() =>
        _vehicles;

    internal void setupDefenses()
    {
        _defenses.Clear();
        foreach (var i in _facilities)
        {
            if (i.getBuildTime() == 0 && i.getRules().getDefenseValue() != 0)
            {
                _defenses.Add(i);
            }
        }

        foreach (var i in getCrafts())
            foreach (var j in i.getVehicles())
                foreach (var k in _vehicles)
                    if (k == j) { _vehicles.Remove(k); break; } // to avoid calling a vehicle's destructor for tanks on crafts

        _vehicles.Clear();

        // add vehicles that are in the crafts of the base, if it's not out
        foreach (var c in getCrafts())
        {
            if (c.getStatus() != "STR_OUT")
            {
                foreach (var i in c.getVehicles())
                {
                    _vehicles.Add(i);
                }
            }
        }

        // add vehicles left on the base
        var e = _items.getContents().GetEnumerator();
        e.MoveNext();
        while (e.Current.Key != null)
        {
            string itemId = e.Current.Key;
            int itemQty = e.Current.Value;
            RuleItem rule = _mod.getItem(itemId, true);
            if (rule.isFixed())
            {
                int size = 4;
                if (_mod.getUnit(itemId) != null)
                {
                    size = _mod.getArmor(_mod.getUnit(itemId).getArmor(), true).getSize();
                }
                if (!rule.getCompatibleAmmo().Any()) // so this vehicle does not need ammo
                {
                    for (int j = 0; j < itemQty; ++j)
                    {
                        _vehicles.Add(new Vehicle(rule, rule.getClipSize(), size));
                    }
                    _items.removeItem(itemId, itemQty);
                }
                else // so this vehicle needs ammo
                {
                    RuleItem ammo = _mod.getItem(rule.getCompatibleAmmo().First(), true);
                    int ammoPerVehicle, clipSize;
                    if (ammo.getClipSize() > 0 && rule.getClipSize() > 0)
                    {
                        clipSize = rule.getClipSize();
                        ammoPerVehicle = clipSize / ammo.getClipSize();
                    }
                    else
                    {
                        clipSize = ammo.getClipSize();
                        ammoPerVehicle = clipSize;
                    }
                    int baseQty = _items.getItem(ammo.getType()) / ammoPerVehicle;
                    if (baseQty == 0)
                    {
                        e.MoveNext();
                        continue;
                    }
                    int canBeAdded = Math.Min(itemQty, baseQty);
                    for (int j = 0; j < canBeAdded; ++j)
                    {
                        _vehicles.Add(new Vehicle(rule, clipSize, size));
                        _items.removeItem(ammo.getType(), ammoPerVehicle);
                    }
                    _items.removeItem(itemId, canBeAdded);
                }

                e = _items.getContents().GetEnumerator(); // we have to start over because iterator is broken because of the removeItem
            }
            else e.MoveNext();
        }
    }

    /**
     * Get the base's retaliation status.
     * @return If the base is a valid target for alien retaliation.
     */
    internal bool getRetaliationTarget() =>
	    _retaliationTarget;

    /**
     * Changes the base's battlescape status.
     * @param inbattle True if it's in battle, False otherwise.
     */
    internal void setInBattlescape(bool inbattle) =>
        _inBattlescape = inbattle;
}
