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
 * Represents a specific type of base facility.
 * Contains constant info about a facility like
 * costs, capacities, size, etc.
 * @sa BaseFacility
 */
internal class RuleBaseFacility : IListOrder, IRule
{
    string _type;
    int _spriteShape, _spriteFacility;
    bool _lift, _hyper, _mind, _grav;
    int _size, _buildCost, _buildTime, _monthlyCost;
    int _storage, _personnel, _aliens, _crafts, _labs, _workshops, _psiLabs;
    int _radarRange, _radarChance, _defense, _hitRatio, _fireSound, _hitSound;
    int _listOrder;
    List<string> _requires;
    string _mapName;

    /**
     * Creates a blank ruleset for a certain
     * type of base facility.
     * @param type String defining the type.
     */
    RuleBaseFacility(string type)
    {
        _type = type;
        _spriteShape = -1;
        _spriteFacility = -1;
        _lift = false;
        _hyper = false;
        _mind = false;
        _grav = false;
        _size = 1;
        _buildCost = 0;
        _buildTime = 0;
        _monthlyCost = 0;
        _storage = 0;
        _personnel = 0;
        _aliens = 0;
        _crafts = 0;
        _labs = 0;
        _workshops = 0;
        _psiLabs = 0;
        _radarRange = 0;
        _radarChance = 0;
        _defense = 0;
        _hitRatio = 0;
        _fireSound = 0;
        _hitSound = 0;
        _listOrder = 0;
    }

    public IRule Create(string type) =>
        new RuleBaseFacility(type);

    /**
     *
     */
    ~RuleBaseFacility() { }

    /**
     * Gets the language string that names
     * this base facility. Each base facility type
     * has a unique name.
     * @return The facility's name.
     */
    internal string getType() =>
	    _type;

    /**
     * Gets the facility's list weight.
     * @return The list weight for this research item.
     */
    public int getListOrder() =>
        _listOrder;

    /**
     * Gets the number of soldiers this facility can contain
     * for monthly psi-training.
     * @return The number of soldiers.
     */
    internal int getPsiLaboratories() =>
	    _psiLabs;

    /**
     * Gets the list of research required to
     * build this base facility.
     * @return A list of research IDs.
     */
    internal List<string> getRequirements() =>
	    _requires;

    /**
     * Loads the base facility type from a YAML file.
     * @param node YAML node.
     * @param mod Mod for the facility.
     * @param listOrder The list weight for this facility.
     */
    internal void load(YamlNode node, Mod mod, int listOrder)
    {
	    _type = node["type"].ToString();
	    _requires = ((YamlSequenceNode)node["requires"]).Children.Select(x => x.ToString()).ToList();

	    mod.loadSpriteOffset(_type, _spriteShape, node["spriteShape"], "BASEBITS.PCK");
	    mod.loadSpriteOffset(_type, _spriteFacility, node["spriteFacility"], "BASEBITS.PCK");

	    _lift = bool.Parse(node["lift"].ToString());
	    _hyper = bool.Parse(node["hyper"].ToString());
	    _mind = bool.Parse(node["mind"].ToString());
	    _grav = bool.Parse(node["grav"].ToString());
	    _size = int.Parse(node["size"].ToString());
	    _buildCost = int.Parse(node["buildCost"].ToString());
	    _buildTime = int.Parse(node["buildTime"].ToString());
	    _monthlyCost = int.Parse(node["monthlyCost"].ToString());
	    _storage = int.Parse(node["storage"].ToString());
	    _personnel = int.Parse(node["personnel"].ToString());
	    _aliens = int.Parse(node["aliens"].ToString());
	    _crafts = int.Parse(node["crafts"].ToString());
	    _labs = int.Parse(node["labs"].ToString());
	    _workshops = int.Parse(node["workshops"].ToString());
	    _psiLabs = int.Parse(node["psiLabs"].ToString());
	    _radarRange = int.Parse(node["radarRange"].ToString());
	    _radarChance = int.Parse(node["radarChance"].ToString());
	    _defense = int.Parse(node["defense"].ToString());
	    _hitRatio = int.Parse(node["hitRatio"].ToString());

	    mod.loadSoundOffset(_type, _fireSound, node["fireSound"], "GEO.CAT");
	    mod.loadSoundOffset(_type, _hitSound, node["hitSound"], "GEO.CAT");

	    _mapName = node["mapName"].ToString();
	    _listOrder = int.Parse(node["listOrder"].ToString());
	    if (_listOrder == 0)
	    {
            _listOrder = listOrder;
	    }
    }

    /**
     * Gets the amount of funds this facility costs monthly
     * to maintain once it's fully built.
     * @return The monthly cost.
     */
    internal int getMonthlyCost() =>
	    _monthlyCost;

    /**
     * Gets the amount of storage space this facility provides
     * for base equipment.
     * @return The storage space.
     */
    internal int getStorage() =>
	    _storage;
}
