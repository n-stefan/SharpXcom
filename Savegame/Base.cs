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
    const int BASE_SIZE = 6;

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
    internal override YamlNode save()
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
    internal override int getMarker()
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
    internal int getPersonnelMaintenance()
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
    internal int getFacilityMaintenance()
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
    internal int getTotalEngineers()
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
    internal int getTotalScientists()
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
    internal override string getName(Language _ = null) =>
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
    internal int getAvailableStores()
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
    internal double getUsedStores()
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
    internal int getAvailableSoldiers(bool checkCombatReadiness = false)
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

    /**
     * Returns the total amount of living quarters
     * available in the base.
     * @return Living space.
     */
    internal int getAvailableQuarters()
    {
	    int total = 0;
	    foreach (var i in _facilities)
	    {
		    if (i.getBuildTime() == 0)
		    {
			    total += i.getRules().getPersonnel();
		    }
	    }
	    return total;
    }

    /**
     * Returns the amount of living quarters used up
     * by personnel in the base.
     * @return Living space.
     */
    internal int getUsedQuarters() =>
	    getTotalSoldiers() + getTotalScientists() + getTotalEngineers();

    /**
     * Returns the total amount of soldiers contained
     * in the base.
     * @return Number of soldiers.
     */
    internal int getTotalSoldiers()
    {
	    int total = _soldiers.Count;
	    foreach (var i in _transfers)
	    {
		    if (i.getType() == TransferType.TRANSFER_SOLDIER)
		    {
			    total += i.getQuantity();
		    }
	    }
	    return total;
    }

    /**
     * Returns the total amount of laboratories
     * available in the base.
     * @return Laboratory space.
     */
    internal int getAvailableLaboratories()
    {
	    int total = 0;
	    foreach (var i in _facilities)
	    {
		    if (i.getBuildTime() == 0)
		    {
			    total += i.getRules().getLaboratories();
		    }
	    }
	    return total;
    }

    /**
     * Returns the amount of laboratories used up
     * by research projects in the base.
     * @return Laboratory space.
     */
    internal int getUsedLaboratories()
    {
	    List<ResearchProject> research = getResearch();
	    int usedLabSpace = 0;
	    foreach (var itResearch in research)
	    {
		    usedLabSpace += itResearch.getAssigned();
	    }
	    return usedLabSpace;
    }

    /**
     * Returns the total amount of workshops
     * available in the base.
     * @return Workshop space.
     */
    internal int getAvailableWorkshops()
    {
	    int total = 0;
	    foreach (var i in _facilities)
	    {
		    if (i.getBuildTime() == 0)
		    {
			    total += i.getRules().getWorkshops();
		    }
	    }
	    return total;
    }

    /**
     * Returns the amount of workshops used up
     * by manufacturing projects in the base.
     * @return Storage space.
     */
    internal int getUsedWorkshops()
    {
	    int usedWorkShop = 0;
	    foreach (var iter in _productions)
	    {
		    usedWorkShop += (iter.getAssignedEngineers() + iter.getRules().getRequiredSpace());
	    }
	    return usedWorkShop;
    }

    /**
     * Returns the total amount of hangars
     * available in the base.
     * @return Number of hangars.
     */
    internal int getAvailableHangars()
    {
	    int total = 0;
	    foreach (var i in _facilities)
	    {
		    if (i.getBuildTime() == 0)
		    {
			    total += i.getRules().getCrafts();
		    }
	    }
	    return total;
    }

    /**
     * Returns the amount of hangars used up
     * by crafts in the base.
     * @return Number of hangars.
     */
    internal int getUsedHangars()
    {
	    int total = _crafts.Count;
	    foreach (var i in _transfers)
	    {
		    if (i.getType() == TransferType.TRANSFER_CRAFT)
		    {
			    total += i.getQuantity();
		    }
	    }
	    foreach (var i in _productions)
	    {
		    if (i.getRules().getCategory() == "STR_CRAFT")
		    {
			    // This should be fixed on the case when (*i)->getInfiniteAmount() == TRUE
			    total += (i.getAmountTotal() - i.getAmountProduced());
		    }
	    }
	    return total;
    }

    /**
     * Returns the total amount of used
     * Psi Lab Space in the base.
     * @return used Psi Lab space.
     */
    internal int getUsedPsiLabs()
    {
	    int total = 0;
	    foreach (var s in _soldiers)
	    {
		    if (s.isInPsiTraining())
		    {
			    total++;
		    }
	    }
	    return total;
    }

    /**
     * Returns the total amount of Containment Space
     * available in the base.
     * @return Containment Lab space.
     */
    internal int getAvailableContainment()
    {
	    int total = 0;
	    foreach (var i in _facilities)
	    {
		    if (i.getBuildTime() == 0)
		    {
			    total += i.getRules().getAliens();
		    }
	    }
	    return total;
    }

    /**
     * Returns the total amount of used
     * Containment Space in the base.
     * @return Containment Lab space.
     */
    internal int getUsedContainment()
    {
	    int total = 0;
	    foreach (var i in _items.getContents())
	    {
		    if (_mod.getItem(i.Key, true).isAlien())
		    {
			    total += i.Value;
		    }
	    }
	    foreach (var i in _transfers)
	    {
		    if (i.getType() == TransferType.TRANSFER_ITEM)
		    {
			    if (_mod.getItem(i.getItems(), true).isAlien())
			    {
				    total += i.getQuantity();
			    }
		    }
	    }
	    foreach (var i in _research)
	    {
		    RuleResearch projRules = i.getRules();
		    if (projRules.needItem() && _mod.getUnit(projRules.getName()) != null)
		    {
			    ++total;
		    }
	    }
	    return total;
    }

    /**
     * Gets a sorted list of the facilities(=iterators) NOT connected to the Access Lift.
     * @param remove Facility to ignore (in case of facility dismantling).
     * @return a sorted list of iterators pointing to elements in _facilities.
     */
    internal List<BaseFacility> getDisconnectedFacilities(BaseFacility remove)
    {
        var result = new List<BaseFacility>();

        if (remove != null && remove.getRules().isLift())
        { // Theoretically this is impossible, but sanity check is good :)
            foreach (var i in _facilities)
            {
                if (i != remove) result.Add(i);
            }
            return result;
        }

        var facilitiesConnStates = new List<KeyValuePair<BaseFacility, bool>?>();
        var grid = new KeyValuePair<BaseFacility, bool>?[BASE_SIZE, BASE_SIZE];
        BaseFacility lift = null;

        for (int x = 0; x < BASE_SIZE; ++x)
        {
            for (int y = 0; y < BASE_SIZE; ++y)
            {
                grid[x, y] = null;
            }
        }

        // Ok, fill up the grid(+facilitiesConnStates), and search the lift
        foreach (var i in _facilities)
        {
            if (i != remove)
            {
                if (i.getRules().isLift()) lift = i;
                for (int x = 0; x != i.getRules().getSize(); ++x)
                {
                    for (int y = 0; y != i.getRules().getSize(); ++y)
                    {
                        var p = KeyValuePair.Create(i, false);
                        facilitiesConnStates.Add(p);
                        grid[i.getX() + x, i.getY() + y] = p;
                    }
                }
            }
        }

        // we're in real trouble if this happens...
        if (lift == null)
        {
            //TODO: something clever.
            return result;
        }

        // Now make the recursion manually using a stack
        var stack = new Stack<KeyValuePair<int, int>>();
        stack.Push(KeyValuePair.Create(lift.getX(), lift.getY()));
        while (stack.Any())
        {
            int x = stack.Peek().Key, y = stack.Peek().Value;
            stack.Pop();
            if (x >= 0 && x < BASE_SIZE && y >= 0 && y < BASE_SIZE && grid[x, y] != null && !grid[x, y].Value.Value)
            {
                grid[x, y] = KeyValuePair.Create(grid[x, y].Value.Key, true);
                BaseFacility fac = grid[x, y].Value.Key;
                BaseFacility neighborLeft = (x - 1 >= 0 && grid[x - 1, y] != null) ? grid[x - 1, y].Value.Key : null;
                BaseFacility neighborRight = (x + 1 < BASE_SIZE && grid[x + 1, y] != null) ? grid[x + 1, y].Value.Key : null;
                BaseFacility neighborTop = (y - 1 >= 0 && grid[x, y - 1] != null) ? grid[x, y - 1].Value.Key : null;
                BaseFacility neighborBottom = (y + 1 < BASE_SIZE && grid[x, y + 1] != null) ? grid[x, y + 1].Value.Key : null;
                if ((fac.getBuildTime() == 0) || (neighborLeft != null && (neighborLeft == fac || neighborLeft.getBuildTime() > neighborLeft.getRules().getBuildTime()))) stack.Push(KeyValuePair.Create(x - 1, y));
                if ((fac.getBuildTime() == 0) || (neighborRight != null && (neighborRight == fac || neighborRight.getBuildTime() > neighborRight.getRules().getBuildTime()))) stack.Push(KeyValuePair.Create(x + 1, y));
                if ((fac.getBuildTime() == 0) || (neighborTop != null && (neighborTop == fac || neighborTop.getBuildTime() > neighborTop.getRules().getBuildTime()))) stack.Push(KeyValuePair.Create(x, y - 1));
                if ((fac.getBuildTime() == 0) || (neighborBottom != null && (neighborBottom == fac || neighborBottom.getBuildTime() > neighborBottom.getRules().getBuildTime()))) stack.Push(KeyValuePair.Create(x, y + 1));
            }
        }

        BaseFacility lastFacility = null;
        for (var i = 0; i < facilitiesConnStates.Count; i++)
        {
            // Not a connected fac.? -> push its iterator into the list!
            // Oh, and we don't want duplicates (facilities with bigger sizes like hangar)
            if (facilitiesConnStates[i].Value.Key != lastFacility && !facilitiesConnStates[i].Value.Value) result.Add(facilitiesConnStates[i].Value.Key);
            lastFacility = facilitiesConnStates[i].Value.Key;
            facilitiesConnStates[i] = null; // We don't need the pair anymore.
        }

        return result;
    }

    /**
     * Returns the amount of scientists contained
     * in the base without any assignments.
     * @return Number of scientists.
     */
    internal int getAvailableScientists() =>
	    getScientists();

    /**
     * Returns the amount of engineers contained
     * in the base without any assignments.
     * @return Number of engineers.
     */
    internal int getAvailableEngineers() =>
	    getEngineers();

    /**
     * Returns the amount of engineers currently in use.
     * @return Amount of engineers.
     */
    internal int getAllocatedEngineers()
    {
	    int total = 0;
	    foreach (var iter in _productions)
	    {
		    total += iter.getAssignedEngineers();
	    }
	    return total;
    }

    /**
     * Return workshop space not used by a Production
     * @return workshop space not used by a Production
     */
    internal int getFreeWorkshops() =>
	    getAvailableWorkshops() - getUsedWorkshops();

    /**
     * Return containment space not in use
     * @return containment space not in use
     */
    internal int getFreeContainment() =>
	    getAvailableContainment() - getUsedContainment();

    /**
     * Return laboratories space not used by a ResearchProject
     * @return laboratories space not used by a ResearchProject
     */
    internal int getFreeLaboratories() =>
	    getAvailableLaboratories() - getUsedLaboratories();

    internal int getGravShields()
    {
	    int total = 0;
	    foreach (var i in _facilities)
	    {
		    if (i.getBuildTime() == 0 && i.getRules().isGravShield())
		    {
			    ++total;
		    }
	    }
	    return total;
    }

    /**
     * Cleans up the defenses vector and optionally reclaims the tanks and their ammo.
     * @param reclaimItems determines whether the HWPs should be returned to storage.
     */
    internal void cleanupDefenses(bool reclaimItems)
    {
        _defenses.Clear();

        foreach (var i in getCrafts())
            foreach (var j in i.getVehicles())
                foreach (var k in _vehicles)
                    if (k == j) { _vehicles.Remove(k); break; } // to avoid calling a vehicle's destructor for tanks on crafts

        while (_vehicles.Count > 0)
        {
            if (reclaimItems)
            {
                RuleItem rule = _vehicles[0].getRules();
                string type = rule.getType();
                _items.addItem(type);
                if (rule.getCompatibleAmmo().Any())
                {
                    RuleItem ammo = _mod.getItem(rule.getCompatibleAmmo().First(), true);
                    int ammoPerVehicle;
                    if (ammo.getClipSize() > 0 && rule.getClipSize() > 0)
                    {
                        ammoPerVehicle = rule.getClipSize() / ammo.getClipSize();
                    }
                    else
                    {
                        ammoPerVehicle = ammo.getClipSize();
                    }
                    _items.addItem(ammo.getType(), ammoPerVehicle);
                }
            }
            _vehicles.RemoveAt(0);
        }
    }

    /**
     * Returns the total amount of soldiers of
     * a certain type stored in the base.
     * @param soldier Soldier type.
     * @return Number of soldiers.
     */
    internal int getSoldierCount(string soldier)
    {
	    int total = 0;
	    foreach (var i in _transfers)
	    {
		    if (i.getType() == TransferType.TRANSFER_SOLDIER && i.getSoldier().getRules().getType() == soldier)
		    {
			    total++;
		    }
	    }
	    foreach (var i in _soldiers)
	    {
		    if (i.getRules().getType() == soldier)
		    {
			    total++;
		    }
	    }
	    return total;
    }

    /**
     * Returns the total defense value of all
     * the facilities in the base.
     * @return Defense value.
     */
    internal int getDefenseValue()
    {
	    int total = 0;
	    foreach (var i in _facilities)
	    {
		    if (i.getBuildTime() == 0)
		    {
			    total += i.getRules().getDefenseValue();
		    }
	    }
	    return total;
    }

    /**
     * Returns the total amount of short range
     * detection facilities in the base.
     * @return Defense value.
     */
    internal int getShortRangeDetection()
    {
	    int total = 0;
	    int minRadarRange = _mod.getMinRadarRange();

	    if (minRadarRange == 0) return 0;
	    foreach (var i in _facilities)
	    {
		    if (i.getRules().getRadarRange() == minRadarRange && i.getBuildTime() == 0)
		    {
			    total++;
		    }
	    }
	    return total;
    }

    /**
     * Returns the total amount of long range
     * detection facilities in the base.
     * @return Defense value.
     */
    internal int getLongRangeDetection()
    {
	    int total = 0;
	    int minRadarRange = _mod.getMinRadarRange();

	    foreach (var i in _facilities)
	    {
		    if (i.getRules().getRadarRange() > minRadarRange && i.getBuildTime() == 0)
		    {
			    total++;
		    }
	    }
	    return total;
    }

    /**
     * Returns the base's battlescape status.
     * @return Is the craft on the battlescape?
     */
    internal bool isInBattlescape() =>
	    _inBattlescape;

    /**
     * Destroys all disconnected facilities in the base.
     */
    internal void destroyDisconnectedFacilities()
    {
	    List<BaseFacility> disFacs = getDisconnectedFacilities(null);
	    for (var i = disFacs.Count - 1; i >= 0; i--)
	    {
		    destroyFacility(disFacs[i]);
	    }
    }

    /**
     * Removes a base module, and deals with the ramifications thereof.
     * @param facility An iterator reference to the facility to destroy and remove.
     */
    internal void destroyFacility(BaseFacility facility)
    {
	    if (facility.getRules().getCrafts() > 0)
	    {
		    // hangar destruction - destroy crafts and any production of crafts
		    // if this will mean there is no hangar to contain it
		    if (facility.getCraft() != null)
		    {
			    // remove all soldiers
			    if (facility.getCraft().getNumSoldiers() != 0)
			    {
				    foreach (var i in _soldiers)
				    {
					    if (i.getCraft() == facility.getCraft())
					    {
						    i.setCraft(null);
					    }
				    }
			    }
			    // remove all items
			    while (facility.getCraft().getItems().getContents().Any())
			    {
                    KeyValuePair<string, int> i = facility.getCraft().getItems().getContents().First();
				    _items.addItem(i.Key, i.Value);
				    facility.getCraft().getItems().removeItem(i.Key, i.Value);
			    }
			    foreach (var i in _crafts)
			    {
				    if (i == facility.getCraft())
				    {
					    _crafts.Remove(i);
					    break;
				    }
			    }
		    }
		    else
		    {
			    bool remove = true;
			    // no craft - check productions.
			    foreach (var i in _productions)
			    {
				    if (getAvailableHangars() - getUsedHangars() - facility.getRules().getCrafts() < 0 && i.getRules().getCategory() == "STR_CRAFT")
				    {
					    _engineers += i.getAssignedEngineers();
					    _productions.Remove(i);
					    remove = false;
					    break;
				    }
			    }
			    if (remove && _transfers.Any())
			    {
				    foreach (var i in _transfers)
				    {
					    if (i.getType() == TransferType.TRANSFER_CRAFT)
					    {
						    i.setCraft(null);
						    _transfers.Remove(i);
						    break;
					    }
				    }
			    }
		    }
	    }
	    if (facility.getRules().getPsiLaboratories() > 0)
	    {
		    // psi lab destruction: remove any soldiers over the maximum allowable from psi training.
		    int toRemove = facility.getRules().getPsiLaboratories() - getFreePsiLabs();
		    for (var i = 0; i < _soldiers.Count && toRemove > 0; ++i)
		    {
			    if (_soldiers[i].isInPsiTraining())
			    {
				    _soldiers[i].setPsiTraining(false);
				    --toRemove;
			    }
		    }
	    }
	    if (facility.getRules().getLaboratories() != 0)
	    {
		    // lab destruction: enforce lab space limits. take scientists off projects until
		    // it all evens out. research is not cancelled.
		    int toRemove = facility.getRules().getLaboratories() - getFreeLaboratories();
		    for (var i = 0; i < _research.Count && toRemove > 0;)
		    {
			    if (_research[i].getAssigned() >= toRemove)
			    {
				    _research[i].setAssigned(_research[i].getAssigned() - toRemove);
				    _scientists += toRemove;
				    break;
			    }
			    else
			    {
				    toRemove -= _research[i].getAssigned();
				    _scientists += _research[i].getAssigned();
				    _research[i].setAssigned(0);
				    ++i;
			    }
		    }
	    }
	    if (facility.getRules().getWorkshops() != 0)
	    {
		    // workshop destruction: similar to lab destruction, but we'll lay off engineers instead
		    // in this case, however, production IS cancelled, as it takes up space in the workshop.
		    int toRemove = facility.getRules().getWorkshops() - getFreeWorkshops();
		    for (var i = 0; i < _productions.Count && toRemove > 0;)
		    {
			    if (_productions[i].getAssignedEngineers() > toRemove)
			    {
				    _productions[i].setAssignedEngineers(_productions[i].getAssignedEngineers() - toRemove);
				    _engineers += toRemove;
				    break;
			    }
			    else
			    {
				    toRemove -= _productions[i].getAssignedEngineers();
				    _engineers += _productions[i].getAssignedEngineers();
				    _productions.RemoveAt(i);
			    }
		    }
	    }
	    if (facility.getRules().getStorage() != 0)
	    {
		    // we won't destroy the items physically AT the base,
		    // but any items in transit will end up at the dead letter office.
		    if (storesOverfull(facility.getRules().getStorage()) && _transfers.Any())
		    {
			    for (var i = 0; i < _transfers.Count;)
			    {
				    if (_transfers[i].getType() == TransferType.TRANSFER_ITEM)
				    {
					    _transfers.RemoveAt(i);
				    }
				    else
				    {
					    ++i;
				    }
			    }
		    }
	    }
	    if (facility.getRules().getPersonnel() != 0)
	    {
		    // as above, we won't actually fire people, but we'll block any new ones coming in.
		    if ((getAvailableQuarters() - getUsedQuarters()) - facility.getRules().getPersonnel() < 0 && _transfers.Any())
		    {
			    for (var i = 0; i < _transfers.Count;)
			    {
				    // let soldiers arrive, but block workers.
				    if (_transfers[i].getType() == TransferType.TRANSFER_ENGINEER || _transfers[i].getType() == TransferType.TRANSFER_SCIENTIST)
				    {
					    _transfers.RemoveAt(i);
				    }
				    else
				    {
					    ++i;
				    }
			    }
		    }
	    }
	    //facility = null;
	    _facilities.Remove(facility);
    }

    /**
     * Return psilab space not in use
     * @return psilab space not in use
     */
    int getFreePsiLabs() =>
	    getAvailablePsiLabs() - getUsedPsiLabs();

    /**
     * Returns the amount of scientists currently in use.
     * @return Amount of scientists.
     */
    internal int getAllocatedScientists()
    {
	    int total = 0;
	    List<ResearchProject> research = getResearch();
	    foreach (var itResearch in research)
	    {
		    total += itResearch.getAssigned();
	    }
	    return total;
    }

    /**
     * Returns the base's unique type used for
     * savegame purposes.
     * @return ID.
     */
    internal override string getType() =>
	    "STR_BASE";

    /**
     * Loads the base from a YAML file.
     * @param node YAML node.
     * @param save Pointer to saved game.
     * @param newGame Is this the first base of a new game?
     * @param newBattleGame Is this the base of a skirmish game?
     */
    internal void load(YamlNode node, SavedGame save, bool newGame, bool newBattleGame = false)
    {
	    base.load(node);

	    if (!newGame || !Options.customInitialBase || newBattleGame)
	    {
		    foreach (var i in ((YamlSequenceNode)node["facilities"]).Children)
		    {
			    string type = i["type"].ToString();
			    if (_mod.getBaseFacility(type) != null)
			    {
				    BaseFacility f = new BaseFacility(_mod.getBaseFacility(type), this);
				    f.load(i);
				    _facilities.Add(f);
			    }
			    else
			    {
					Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load facility {type}");
			    }
		    }
	    }

	    foreach (var i in ((YamlSequenceNode)node["crafts"]).Children)
	    {
		    string type = i["type"].ToString();
		    if (_mod.getCraft(type) != null)
		    {
			    Craft c = new Craft(_mod.getCraft(type), this);
			    c.load(i, _mod, save);
			    _crafts.Add(c);
		    }
		    else
		    {
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load craft {type}");
		    }
	    }

	    foreach (var i in ((YamlSequenceNode)node["soldiers"]).Children)
	    {
		    string type = i["type"] != null ? i["type"].ToString() : _mod.getSoldiersList().First();
		    if (_mod.getSoldier(type) != null)
		    {
			    Soldier s = new Soldier(_mod.getSoldier(type), null);
			    s.load(i, _mod, save);
			    s.setCraft(null);
			    if (i["craft"] is YamlNode craft)
			    {
				    KeyValuePair<string, int> craftId = Craft.loadId(craft);
				    foreach (var j in _crafts)
				    {
					    if ((j.getUniqueId().Key, j.getUniqueId().Value) == (craftId.Key, craftId.Value))
					    {
						    s.setCraft(j);
						    break;
					    }
				    }
			    }
			    _soldiers.Add(s);
		    }
		    else
		    {
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load soldier {type}");
		    }
	    }

	    _items.load(node["items"]);
	    // Some old saves have bad items, better get rid of them to avoid further bugs
        var k = _items.getContents().GetEnumerator();
        k.MoveNext();
        while (k.Current.Key != null)
	    {
		    if (_mod.getItem(k.Current.Key) == null)
		    {
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load item {k.Current.Key}");
                _items.getContents().Remove(k.Current.Key); k.MoveNext();
		    }
		    else
		    {
			    k.MoveNext();
		    }
	    }

	    _scientists = int.Parse(node["scientists"].ToString());
	    _engineers = int.Parse(node["engineers"].ToString());
	    _inBattlescape = bool.Parse(node["inBattlescape"].ToString());

	    foreach (var i in ((YamlSequenceNode)node["transfers"]).Children)
	    {
		    int hours = int.Parse(i["hours"].ToString());
		    Transfer t = new Transfer(hours);
		    if (t.load(i, this, _mod, save))
		    {
			    _transfers.Add(t);
		    }
	    }

	    foreach (var i in ((YamlSequenceNode)node["research"]).Children)
	    {
		    string research = i["project"].ToString();
		    if (_mod.getResearch(research) != null)
		    {
			    ResearchProject r = new ResearchProject(_mod.getResearch(research));
			    r.load(i);
			    _research.Add(r);
		    }
		    else
		    {
			    _scientists += int.Parse(i["assigned"].ToString());
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load research {research}");
		    }
	    }

	    foreach (var i in ((YamlSequenceNode)node["productions"]).Children)
	    {
		    string item = i["item"].ToString();
		    if (_mod.getManufacture(item) != null)
		    {
			    Production p = new Production(_mod.getManufacture(item), 0);
			    p.load(i);
			    _productions.Add(p);
		    }
		    else
		    {
			    _engineers += int.Parse(i["assigned"].ToString());
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load manufacture {item}");
		    }
	    }

	    _retaliationTarget = bool.Parse(node["retaliationTarget"].ToString());

	    isOverlappingOrOverflowing(); // don't crash, just report in the log file...
    }

    /**
     * Tests whether the base facilities are within the base boundaries and not overlapping.
     * @return True if the base has a problem.
     */
    bool isOverlappingOrOverflowing()
    {
	    bool result = false;
        BaseFacility[,] grid = new BaseFacility[BASE_SIZE, BASE_SIZE];

	    // i don't think i NEED to do this for a pointer array, but who knows what might happen on weird archaic linux distros if i don't?
	    for (int x = 0; x < BASE_SIZE; ++x)
	    {
		    for (int y = 0; y < BASE_SIZE; ++y)
		    {
			    grid[x, y] = null;
		    }
	    }

	    foreach (var f in _facilities)
	    {
		    RuleBaseFacility rules = f.getRules();
		    int facilityX = f.getX();
		    int facilityY = f.getY();
		    int facilitySize = rules.getSize();

		    if (facilityX < 0 || facilityY < 0 || facilityX + (facilitySize - 1) >= BASE_SIZE || facilityY + (facilitySize - 1) >= BASE_SIZE)
		    {
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Facility {rules.getType()} at [{facilityX}, {facilityY}] (size {facilitySize}) is outside of base boundaries.");
			    result = true;
			    continue;
		    }

		    for (int x = facilityX; x < facilityX + facilitySize; ++x)
		    {
			    for (int y = facilityY; y < facilityY + facilitySize; ++y)
			    {
				    if (grid[x, y] != null)
				    {
				        Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Facility {rules.getType()} at [{facilityX}, {facilityY}] (size {facilitySize}) overlaps with {grid[x, y].getRules().getType()} at [{x}, {y}] (size {grid[x, y].getRules().getSize()})");
					    result = true;
				    }
				    grid[x, y] = f;
			    }
		    }
	    }

	    return result;
    }
}
