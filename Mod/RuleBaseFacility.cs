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

	    mod.loadSpriteOffset(_type, ref _spriteShape, node["spriteShape"], "BASEBITS.PCK");
	    mod.loadSpriteOffset(_type, ref _spriteFacility, node["spriteFacility"], "BASEBITS.PCK");

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

	    mod.loadSoundOffset(_type, ref _fireSound, node["fireSound"], "GEO.CAT");
	    mod.loadSoundOffset(_type, ref _hitSound, node["hitSound"], "GEO.CAT");

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

    /**
     * Checks if this facility has hyperwave detection
     * capabilities. This allows it to get extra details about UFOs.
     * @return True if it has hyperwave detection.
     */
    internal bool isHyperwave() =>
	    _hyper;

    /**
     * Gets the radar range this facility provides for the
     * detection of UFOs.
     * @return The range in nautical miles.
     */
    internal int getRadarRange() =>
	    _radarRange;

    /**
     * Gets the chance of UFOs that come within the facility's
     * radar range being detected.
     * @return The chance as a percentage.
     */
    internal int getRadarChance() =>
	    _radarChance;

    /**
     * Gets the size of the facility on the base grid.
     * @return The length in grid squares.
     */
    internal int getSize() =>
	    _size;

    /**
     * Checks if this facility has a mind shield,
     * which covers your base from alien detection.
     * @return True if it has a mind shield.
     */
    internal bool isMindShield() =>
	    _mind;

    /**
     * Gets the defense value of this facility's weaponry
     * against UFO invasions on the base.
     * @return The defense value.
     */
    internal int getDefenseValue() =>
	    _defense;

    /**
     * Gets the battlescape map block name for this facility
     * to construct the base defense mission map.
     * @return The map name.
     */
    internal string getMapName() =>
	    _mapName;

    /**
     * Gets the number of base personnel (soldiers, scientists,
     * engineers) this facility can contain.
     * @return The number of personnel.
     */
    internal int getPersonnel() =>
	    _personnel;

    /**
     * Gets the amount of laboratory space this facility provides
     * for research projects.
     * @return The laboratory space.
     */
    internal int getLaboratories() =>
	    _labs;

    /**
     * Gets the amount of workshop space this facility provides
     * for manufacturing projects.
     * @return The workshop space.
     */
    internal int getWorkshops() =>
	    _workshops;

    /**
     * Gets the number of base craft this facility can contain.
     * @return The number of craft.
     */
    internal int getCrafts() =>
	    _crafts;

    /**
     * Gets the number of captured live aliens this facility
     * can contain.
     * @return The number of aliens.
     */
    internal int getAliens() =>
	    _aliens;

    /**
     * Checks if this facility is the core access lift
     * of a base. Every base has an access lift and all
     * facilities have to be connected to it.
     * @return True if it's a lift.
     */
    internal bool isLift() =>
	    _lift;

    /**
     * Gets the amount of time that this facility takes
     * to be constructed since placement.
     * @return The time in days.
     */
    internal int getBuildTime() =>
	    _buildTime;

    /**
     * Checks if this facility has a grav shield,
     * which doubles base defense's fire ratio.
     * @return True if it has a grav shield.
     */
    internal bool isGravShield() =>
	    _grav;

    /**
     * Gets the fire sound of this facility's weaponry.
     * @return The sound index number.
     */
    internal int getFireSound() =>
	    _fireSound;

    /**
     * Gets the hit ratio of this facility's weaponry
     * against UFO invasions on the base.
     * @return The hit ratio as a percentage.
     */
    internal int getHitRatio() =>
	    _hitRatio;

    /**
     * Gets the hit sound of this facility's weaponry.
     * @return The sound index number.
     */
    internal int getHitSound() =>
	    _hitSound;

    /**
     * Gets the amount of funds that this facility costs
     * to build on a base.
     * @return The building cost.
     */
    internal int getBuildCost() =>
	    _buildCost;

    /**
     * Gets the ID of the sprite used to draw the
     * base structure of the facility that defines its shape.
     * @return The sprite ID.
     */
    internal int getSpriteShape() =>
	    _spriteShape;

    /**
     * Gets the ID of the sprite used to draw the
     * facility's contents inside the base shape.
     * @return The sprite ID.
     */
    internal int getSpriteFacility() =>
	    _spriteFacility;
}
