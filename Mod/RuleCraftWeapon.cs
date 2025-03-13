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
 * Represents a specific type of craft weapon.
 * Contains constant info about a craft weapon like
 * damage, range, accuracy, items used, etc.
 * @sa CraftWeapon
 */
internal class RuleCraftWeapon : IRule
{
    string _type;
    int _sprite, _sound, _damage, _range, _accuracy, _reloadCautious, _reloadStandard, _reloadAggressive, _ammoMax, _rearmRate, _projectileSpeed;
    CraftWeaponProjectileType _projectileType;
    bool _underwaterOnly;
    string _launcher, _clip;

    /**
     * Creates a blank ruleset for a certain type of craft weapon.
     * @param type String defining the type.
     */
    RuleCraftWeapon(string type)
    {
        _type = type;
        _sprite = -1;
        _sound = -1;
        _damage = 0;
        _range = 0;
        _accuracy = 0;
        _reloadCautious = 0;
        _reloadStandard = 0;
        _reloadAggressive = 0;
        _ammoMax = 0;
        _rearmRate = 1;
        _projectileSpeed = 0;
        _projectileType = CraftWeaponProjectileType.CWPT_CANNON_ROUND;
        _underwaterOnly = false;
    }

    public IRule Create(string type) =>
        new RuleCraftWeapon(type);

    /**
     *
     */
    ~RuleCraftWeapon() { }

    /**
     * Gets the language string of the item used to
     * equip this craft weapon.
     * @return The item name.
     */
    internal string getLauncherItem() =>
	    _launcher;

    /**
     * Loads the craft weapon from a YAML file.
     * @param node YAML node.
     * @param mod Mod for the craft weapon.
     */
    internal void load(YamlNode node, Mod mod)
    {
	    _type = node["type"].ToString();
	    if (node["sprite"] != null)
	    {
		    // used in
		    // Surface set (baseOffset):
		    //   BASEBITS.PCK (48)
		    //   INTICON.PCK (5)
		    //
		    // Final index in surfaceset is `baseOffset + sprite + (sprite > 5 ? modOffset : 0)`
		    _sprite = mod.getOffset(int.Parse(node["sprite"].ToString()), 5);
	    }
	    mod.loadSoundOffset(_type, ref _sound, node["sound"], "GEO.CAT");
	    _damage = int.Parse(node["damage"].ToString());
	    _range = int.Parse(node["range"].ToString());
	    _accuracy = int.Parse(node["accuracy"].ToString());
	    _reloadCautious = int.Parse(node["reloadCautious"].ToString());
	    _reloadStandard = int.Parse(node["reloadStandard"].ToString());
	    _reloadAggressive = int.Parse(node["reloadAggressive"].ToString());
	    _ammoMax = int.Parse(node["ammoMax"].ToString());
	    _rearmRate = int.Parse(node["rearmRate"].ToString());
	    _projectileType = (CraftWeaponProjectileType)int.Parse(node["projectileType"].ToString());
	    _projectileSpeed = int.Parse(node["projectileSpeed"].ToString());
	    _launcher = node["launcher"].ToString();
	    _clip = node["clip"].ToString();
	    _underwaterOnly = bool.Parse(node["underwaterOnly"].ToString());
    }

    /**
     * Gets the language string of the item used to
     * load this craft weapon with ammo.
     * @return The item name.
     */
    internal string getClipItem() =>
	    _clip;

    /**
     * Gets how much ammo is added to the craft weapon
     * while rearming (the amount of ammo in each clip item).
     * @return The amount of ammo.
     */
    internal int getRearmRate() =>
	    _rearmRate;

    /**
     * Gets the maximum amount of ammo the craft weapon
     * can carry.
     * @return The amount of ammo.
     */
    internal int getAmmoMax() =>
	    _ammoMax;

    /**
     * Gets the amount of time the craft weapon takes to
     * reload in cautious mode.
     * @return The time in game seconds.
     */
    internal int getCautiousReload() =>
	    _reloadCautious;

    /**
     * Gets the maximum range of this craft weapon.
     * @return The range in km.
     */
    internal int getRange() =>
	    _range;

    /**
     * Gets the amount of time the craft weapon takes to
     * reload in standard mode.
     * @return The time in game seconds.
     */
    internal int getStandardReload() =>
	    _reloadStandard;

    /**
     * Gets the amount of time the craft weapon takes to
     * reload in aggressive mode.
     * @return The time in game seconds.
     */
    internal int getAggressiveReload() =>
	    _reloadAggressive;

    /**
     * Gets the ID of the sprite used to draw the craft weapon
     * in the Equip Craft and Interception screens.
     * @return The sprite ID.
     */
    internal int getSprite() =>
	    _sprite;

    /**
     * Gets the language string that names this craft weapon.
     * Each craft weapon type has a unique name.
     * @return The craft weapon's name.
     */
    internal string getType() =>
	    _type;

    /**
     * Gets the amount of damage this craft weapon
     * inflicts on enemy crafts.
     * @return The damage amount.
     */
    internal int getDamage() =>
	    _damage;

    /**
     * Gets the percentage chance of each shot of
     * this craft weapon hitting an enemy craft.
     * @return The accuracy as a percentage.
     */
    internal int getAccuracy() =>
	    _accuracy;

    /**
     * Gets the ID of the sound used when firing the weapon
     * in the Dogfight screen.
     * @return The sound ID.
     */
    internal int getSound() =>
	    _sound;

    /**
     * Gets the Projectile Type this weapon will fire
     * @return The projectile type.
     */
    internal CraftWeaponProjectileType getProjectileType() =>
	    _projectileType;

    /**
     * Gets the speed of the projectile fired by this weapon
     * @return The projectile speed.
     */
    internal int getProjectileSpeed() =>
	    _projectileSpeed;

    /**
     * Can this item be used on land or is it underwater only?
     * @return if this is an underwater weapon or not.
     */
    bool isWaterOnly() =>
	    _underwaterOnly;
}
