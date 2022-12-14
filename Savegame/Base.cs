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

    /**
     * Initializes an empty base.
     * @param mod Pointer to mod.
     */
    Base(Mod.Mod mod) : base()
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
    internal override int getMarker()
    {
	    // Cheap hack to hide bases when they haven't been placed yet
	    if (AreSame(_lon, 0.0) && AreSame(_lat, 0.0))
		    return -1;
	    return 0;
    }
}
