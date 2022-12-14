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
    internal void load(YamlNode node)
    {
        tu = int.Parse(node["tu"].ToString());
        stamina = int.Parse(node["stamina"].ToString());
        health = int.Parse(node["health"].ToString());
        bravery = int.Parse(node["bravery"].ToString());
        reactions = int.Parse(node["reactions"].ToString());
        firing = int.Parse(node["firing"].ToString());
        throwing = int.Parse(node["throwing"].ToString());
        strength = int.Parse(node["strength"].ToString());
        psiStrength = int.Parse(node["psiStrength"].ToString());
        psiSkill = int.Parse(node["psiSkill"].ToString());
        melee = int.Parse(node["melee"].ToString());
    }

    /**
     * Saves the unit stats to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "tu", tu.ToString() },
            { "stamina", stamina.ToString() },
            { "health", health.ToString() },
            { "bravery", bravery.ToString() },
            { "reactions", reactions.ToString() },
            { "firing", firing.ToString() },
            { "throwing", throwing.ToString() },
            { "strength", strength.ToString() },
            { "psiStrength", psiStrength.ToString() },
            { "psiSkill", psiSkill.ToString() },
            { "melee", melee.ToString() }
        };
        return node;
    }
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
    string _psiWeapon;
    bool _capturable;
    string _race;
    string _rank;

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
}
