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

namespace SharpXcom.Mod;

/**
 * Represents game-specific contents needed
 * for save conversion and ID matching.
 * @sa SaveConverter
 */
internal class RuleConverter
{
    Dictionary<string, int> _offsets;
    List<string> _markers, _countries, _regions, _facilities, _items, _crews, _crafts, _ufos, _craftWeapons, _missions, _armor, _alienRaces, _alienRanks, _research, _manufacture, _ufopaedia;

    /**
     * Creates a blank ruleset for converter data.
     */
    internal RuleConverter() { }

    /**
     *
     */
    ~RuleConverter() { }

	/**
	 * Loads the converter data from a YAML file.
	 * @param node YAML node.
	 */
	internal void load(YamlNode node)
	{
        _offsets = ((YamlMappingNode)node["offsets"]).Children.ToDictionary(x => x.Key.ToString(), x => int.Parse(x.Value.ToString()));
        _markers = ((YamlSequenceNode)node["markers"]).Children.Select(x => x.ToString()).ToList();
        _countries = ((YamlSequenceNode)node["countries"]).Children.Select(x => x.ToString()).ToList();
        _regions = ((YamlSequenceNode)node["regions"]).Children.Select(x => x.ToString()).ToList();
        _facilities = ((YamlSequenceNode)node["facilities"]).Children.Select(x => x.ToString()).ToList();
        _items = ((YamlSequenceNode)node["items"]).Children.Select(x => x.ToString()).ToList();
        _crews = ((YamlSequenceNode)node["crews"]).Children.Select(x => x.ToString()).ToList();
        _crafts = ((YamlSequenceNode)node["crafts"]).Children.Select(x => x.ToString()).ToList();
        _ufos = ((YamlSequenceNode)node["ufos"]).Children.Select(x => x.ToString()).ToList();
        _craftWeapons = ((YamlSequenceNode)node["craftWeapons"]).Children.Select(x => x.ToString()).ToList();
        _missions = ((YamlSequenceNode)node["missions"]).Children.Select(x => x.ToString()).ToList();
        _armor = ((YamlSequenceNode)node["armor"]).Children.Select(x => x.ToString()).ToList();
        _alienRaces = ((YamlSequenceNode)node["alienRaces"]).Children.Select(x => x.ToString()).ToList();
        _alienRanks = ((YamlSequenceNode)node["alienRanks"]).Children.Select(x => x.ToString()).ToList();
        _research = ((YamlSequenceNode)node["research"]).Children.Select(x => x.ToString()).ToList();
        _manufacture = ((YamlSequenceNode)node["manufacture"]).Children.Select(x => x.ToString()).ToList();
        _ufopaedia = ((YamlSequenceNode)node["ufopaedia"]).Children.Select(x => x.ToString()).ToList();
	}

    /// Gets the country ID list.
    internal List<string> getCountries() =>
        _countries;

    /// Gets the region ID list.
    internal List<string> getRegions() =>
        _regions;

    /// Gets the marker ID list.
    internal List<string> getMarkers() =>
        _markers;

    /// Gets the alien mission ID list.
    internal List<string> getMissions() =>
        _missions;

    /// Gets the UFO crew ID list.
    internal List<string> getCrews() =>
        _crews;

    /// Gets the UFO ID list.
    internal List<string> getUfos() =>
        _ufos;

    /// Gets the craft ID list.
    internal List<string> getCrafts() =>
        _crafts;

    /// Gets the offset for a specific attribute.
    internal int getOffset(string id) =>
        _offsets[id];

    /// Gets the facility ID list.
    internal List<string> getFacilities() =>
        _facilities;

    /// Gets the item ID list.
    internal List<string> getItems() =>
        _items;

    /// Gets the alien race ID list.
    internal List<string> getAlienRaces() =>
        _alienRaces;

    /// Gets the alien rank ID list.
    internal List<string> getAlienRanks() =>
        _alienRanks;

    /// Gets the craft weapon ID list.
    internal List<string> getCraftWeapons() =>
        _craftWeapons;

    /// Gets the armor ID list.
    internal List<string> getArmor() =>
        _armor;
}
