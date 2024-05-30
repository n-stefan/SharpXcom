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

enum SpecialAbility { SPECAB_NONE, SPECAB_EXPLODEONDEATH, SPECAB_BURNFLOOR, SPECAB_BURN_AND_EXPLODE };

/**
 * This struct holds some plain unit attribute data together.
 */
struct UnitStats
{
    internal int tu, stamina, health, bravery, reactions, firing, throwing, strength, psiStrength, psiSkill, melee;

    /**
     * Loads the unit stats from a YAML file.
     * @param node YAML node.
     */
    internal static UnitStats decode(YamlNode node)
    {
        if (node.NodeType != YamlNodeType.Mapping)
        	return default;

        var us = new UnitStats
        {
            tu = int.Parse(node["tu"].ToString()),
            stamina = int.Parse(node["stamina"].ToString()),
            health = int.Parse(node["health"].ToString()),
            bravery = int.Parse(node["bravery"].ToString()),
            reactions = int.Parse(node["reactions"].ToString()),
            firing = int.Parse(node["firing"].ToString()),
            throwing = int.Parse(node["throwing"].ToString()),
            strength = int.Parse(node["strength"].ToString()),
            psiStrength = int.Parse(node["psiStrength"].ToString()),
            psiSkill = int.Parse(node["psiSkill"].ToString()),
            melee = int.Parse(node["melee"].ToString())
        };
        return us;
    }

    /**
     * Saves the unit stats to a YAML file.
     * @return YAML node.
     */
    internal static YamlNode encode(UnitStats us)
    {
        var node = new YamlMappingNode
        {
            { "tu", us.tu.ToString() },
            { "stamina", us.stamina.ToString() },
            { "health", us.health.ToString() },
            { "bravery", us.bravery.ToString() },
            { "reactions", us.reactions.ToString() },
            { "firing", us.firing.ToString() },
            { "throwing", us.throwing.ToString() },
            { "strength", us.strength.ToString() },
            { "psiStrength", us.psiStrength.ToString() },
            { "psiSkill", us.psiSkill.ToString() },
            { "melee", us.melee.ToString() }
        };
        return node;
    }

    internal void merge(UnitStats stats)
    {
        tu = (stats.tu != 0 ? stats.tu : tu);
        stamina = (stats.stamina != 0 ? stats.stamina : stamina);
        health = (stats.health != 0 ? stats.health : health);
        bravery = (stats.bravery != 0 ? stats.bravery : bravery);
        reactions = (stats.reactions != 0 ? stats.reactions : reactions);
        firing = (stats.firing != 0 ? stats.firing : firing);
        throwing = (stats.throwing != 0 ? stats.throwing : throwing);
        strength = (stats.strength != 0 ? stats.strength : strength);
        psiStrength = (stats.psiStrength != 0 ? stats.psiStrength : psiStrength);
        psiSkill = (stats.psiSkill != 0 ? stats.psiSkill : psiSkill);
        melee = (stats.melee != 0 ? stats.melee : melee);
    }

    public static UnitStats operator +(UnitStats a, UnitStats b) =>
        new() { tu = a.tu + b.tu, stamina = a.stamina + b.stamina, health = a.health + b.health, bravery = a.bravery + b.bravery, reactions = a.reactions + b.reactions, firing = a.firing + b.firing, throwing = a.throwing + b.throwing, strength = a.strength + b.strength, psiStrength = a.psiStrength + b.psiStrength, psiSkill = a.psiSkill + b.psiSkill, melee = a.melee + b.melee };

    public static UnitStats operator -(UnitStats a, UnitStats b) =>
        new() { tu = a.tu - b.tu, stamina = a.stamina - b.stamina, health = a.health - b.health, bravery = a.bravery - b.bravery, reactions = a.reactions - b.reactions, firing = a.firing - b.firing, throwing = a.throwing - b.throwing, strength = a.strength - b.strength, psiStrength = a.psiStrength - b.psiStrength, psiSkill = a.psiSkill - b.psiSkill, melee = a.melee - b.melee };
}

struct StatAdjustment
{
    internal UnitStats statGrowth;
    internal int growthMultiplier;
    internal double aimAndArmorMultiplier;
};

/**
 * Represents the static data for a unit that is generated on the battlescape, this includes: HWPs, aliens and civilians.
 * @sa Soldier BattleUnit
 */
internal class Unit : IRule
{
    string _type;
    int _standHeight, _kneelHeight, _floatHeight;
    int _value, _aggroSound, _moveSound;
    int _intelligence, _aggression, _energyRecovery;
    SpecialAbility _specab;
    bool _livingWeapon;
    string _meleeWeapon, _psiWeapon;
    bool _capturable;
    string _race;
    string _rank;
    UnitStats _stats;
    string _armor;
    List<List<string>> _builtInWeapons;
    string _spawnUnit;
    List<int> _deathSound;

    /**
     * Creates a certain type of unit.
     * @param type String defining the type.
     */
    Unit(string type)
    {
        _type = type;
        _standHeight = 0;
        _kneelHeight = 0;
        _floatHeight = 0;
        _value = 0;
        _aggroSound = -1;
        _moveSound = -1;
        _intelligence = 0;
        _aggression = 0;
        _energyRecovery = 30;
        _specab = SpecialAbility.SPECAB_NONE;
        _livingWeapon = false;
        _psiWeapon = "ALIEN_PSI_WEAPON";
        _capturable = true;
    }

    public IRule Create(string type) =>
        new Unit(type);

    /**
     *
     */
    ~Unit() { }

    /**
     * Gets the alien's race.
     * @return The alien's race.
     */
    internal string getRace() =>
	    _race;

    /**
     * Gets the unit's rank.
     * @return The unit's rank.
     */
    internal string getRank() =>
	    _rank;

    /**
     * Gets the unit's special ability.
     * @return The unit's specab.
     */
    internal int getSpecialAbility() =>
	    (int)_specab;

    /**
     * Loads the unit from a YAML file.
     * @param node YAML node.
     * @param mod Mod for the unit.
     */
    internal void load(YamlNode node, Mod mod)
    {
	    _type = node["type"].ToString();
	    _race = node["race"].ToString();
	    _rank = node["rank"].ToString();
        var stats = UnitStats.decode(node["stats"]);
        _stats.merge(stats);
	    _armor = node["armor"].ToString();
	    _standHeight = int.Parse(node["standHeight"].ToString());
	    _kneelHeight = int.Parse(node["kneelHeight"].ToString());
	    _floatHeight = int.Parse(node["floatHeight"].ToString());
	    if (_floatHeight + _standHeight > 25)
	    {
		    throw new Exception("Error with unit "+ _type +": Unit height may not exceed 25");
	    }
	    _value = int.Parse(node["value"].ToString());
	    _intelligence = int.Parse(node["intelligence"].ToString());
	    _aggression = int.Parse(node["aggression"].ToString());
	    _energyRecovery = int.Parse(node["energyRecovery"].ToString());
	    _specab = (SpecialAbility)int.Parse(node["specab"].ToString());
	    _spawnUnit = node["spawnUnit"].ToString();
	    _livingWeapon = bool.Parse(node["livingWeapon"].ToString());
	    _meleeWeapon = node["meleeWeapon"].ToString();
	    _psiWeapon = node["psiWeapon"].ToString();
	    _capturable = bool.Parse(node["capturable"].ToString());
        foreach (var i in ((YamlSequenceNode)node["builtInWeaponSets"]).Children)
        {
            _builtInWeapons.Add(((YamlSequenceNode)i).Children.Select(x => x.ToString()).ToList());
        }
        if (node["builtInWeapons"] != null)
	    {
            _builtInWeapons.Add(((YamlSequenceNode)node["builtInWeapons"]).Children.Select(x => x.ToString()).ToList());
	    }
	    mod.loadSoundOffset(_type, _deathSound, node["deathSound"], "BATTLE.CAT");
	    mod.loadSoundOffset(_type, _aggroSound, node["aggroSound"], "BATTLE.CAT");
	    mod.loadSoundOffset(_type, _moveSound, node["moveSound"], "BATTLE.CAT");
    }

    /**
     * Gets the unit's armor type.
     * @return The unit's armor type.
     */
    internal string getArmor() =>
	    _armor;

    /**
     * What weapons does this unit have built in?
     * this is a vector of strings representing any
     * weapons that may be inherent to this creature.
     * note: unlike "livingWeapon" this is used in ADDITION to
     * any loadout or living weapon item that may be defined.
     * @return list of weapons that are integral to this unit.
     */
    internal List<List<string>> getBuiltInWeapons() =>
	    _builtInWeapons;

    /**
     * Returns the language string that names
     * this unit. Each unit type has a unique name.
     * @return The unit's name.
     */
    internal string getType() =>
	    _type;

    /**
     * Returns the unit's stats data object.
     * @return The unit's stats.
     */
    internal UnitStats getStats() =>
        _stats;

    /**
     * Returns the unit's height at standing.
     * @return The unit's height.
     */
    internal int getStandHeight() =>
	    _standHeight;

    /**
     * Returns the unit's height at kneeling.
     * @return The unit's kneeling height.
     */
    internal int getKneelHeight() =>
	    _kneelHeight;

    /**
     * Returns the unit's floating elevation.
     * @return The unit's floating height.
     */
    internal int getFloatHeight() =>
	    _floatHeight;

    /**
    * Get the unit's death sounds.
    * @return List of sound IDs.
    */
    internal List<int> getDeathSounds() =>
	    _deathSound;

    /**
     * Gets the unit's war cry.
     * @return The id of the unit's aggro sound.
     */
    internal int getAggroSound() =>
	    _aggroSound;

    /**
     * Gets the unit's move sound.
     * @return The id of the unit's move sound.
     */
    internal int getMoveSound() =>
	    _moveSound;

    /**
     * Gets the intelligence. This is the number of turns the AI remembers your troop positions.
     * @return The unit's intelligence.
     */
    internal int getIntelligence() =>
	    _intelligence;

    /**
     * Gets the aggression. Determines the chance of revenge and taking cover.
     * @return The unit's aggression.
     */
    internal int getAggression() =>
	    _aggression;

    /**
     * Gets the unit that is spawned when this one dies.
     * @return The unit's spawn unit.
     */
    internal string getSpawnUnit() =>
	    _spawnUnit;

    /**
     * Gets the unit's value - for scoring.
     * @return The unit's value.
     */
    internal int getValue() =>
	    _value;

    /**
    * Gets whether the alien can be captured alive.
    * @return a value determining whether the alien can be captured alive.
    */
    internal bool getCapturable() =>
	    _capturable;

    /**
     * What is this unit's built in melee weapon (if any).
     * @return the name of the weapon.
     */
    internal string getMeleeWeapon() =>
	    _meleeWeapon;

    /**
    * What is this unit's built in psi weapon (if any).
    * @return the name of the weapon.
    */
    internal string getPsiWeapon() =>
	    _psiWeapon;

    /**
     * Checks if this unit is a living weapon.
     * a living weapon ignores any loadout that may be available to
     * its rank and uses the one associated with its race.
     * @return True if this unit is a living weapon.
     */
    internal bool isLivingWeapon() =>
	    _livingWeapon;

    /**
     * How much energy does this unit recover per turn?
     * @return energy recovery amount.
     */
    internal int getEnergyRecovery() =>
	    _energyRecovery;
}
