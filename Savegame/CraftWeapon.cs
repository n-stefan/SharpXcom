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
 * Represents a craft weapon equipped by a craft.
 * Contains variable info about a craft weapon like ammo.
 * @sa RuleCraftWeapon
 */
internal class CraftWeapon
{
    RuleCraftWeapon _rules;
    int _ammo;
    bool _rearming;

    internal CraftWeapon() { }

    /**
     * Initializes a craft weapon of the specified type.
     * @param rules Pointer to ruleset.
     * @param ammo Initial ammo.
     */
    internal CraftWeapon(RuleCraftWeapon rules, int ammo)
    {
        _rules = rules;
        _ammo = ammo;
        _rearming = false;
    }

    /**
     *
     */
    ~CraftWeapon() { }

    /**
     * Loads the craft weapon from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    _ammo = int.Parse(node["ammo"].ToString());
	    _rearming = bool.Parse(node["rearming"].ToString());
    }

    /**
     * Returns the ruleset for the craft weapon's type.
     * @return Pointer to ruleset.
     */
    internal RuleCraftWeapon getRules() =>
	    _rules;

    /**
     * Returns whether this craft weapon needs rearming.
     * @return Rearming status.
     */
    internal bool isRearming() =>
	    _rearming;

    /**
     * Rearms this craft weapon's ammo.
     * @param available number of clips available.
     * @param clipSize number of rounds in said clips.
     * @return number of clips used.
     */
    internal int rearm(int available, int clipSize)
    {
	    int ammoUsed = _rules.getRearmRate();

	    if (clipSize > 0)
	    {	// +(clipSize - 1) correction for rounding up
		    int needed = Math.Min(_rules.getRearmRate(), _rules.getAmmoMax() - _ammo + clipSize - 1) / clipSize;
		    ammoUsed = ((needed > available)? available : needed) * clipSize;
	    }

	    setAmmo(_ammo + ammoUsed);

	    _rearming = _ammo < _rules.getAmmoMax();

	    return (clipSize <= 0)? 0 : ammoUsed / clipSize;
    }

    /**
     * Changes the ammo contained in this craft weapon.
     * @param ammo Weapon ammo.
     * @return If the weapon ran out of ammo.
     */
    internal bool setAmmo(int ammo)
    {
        _ammo = ammo;
        if (_ammo < 0)
        {
            _ammo = 0;
            return false;
        }
        if (_ammo > _rules.getAmmoMax())
        {
            _ammo = _rules.getAmmoMax();
        }
        return true;
    }

    /**
     * Changes whether this craft weapon needs rearming
     * (for example, in case there's no more ammo).
     * @param rearming Rearming status.
     */
    internal void setRearming(bool rearming) =>
        _rearming = rearming;

    /**
     * Returns the ammo contained in this craft weapon.
     * @return Weapon ammo.
     */
    internal int getAmmo() =>
	    _ammo;

    /*
     * get how many clips are loaded into this weapon.
     * @param mod a pointer to the core mod.
     * @return number of clips loaded.
     */
    internal int getClipsLoaded(Mod.Mod mod)
    {
	    int retVal = (int)Math.Floor((double)_ammo / _rules.getRearmRate());
	    RuleItem clip = mod.getItem(_rules.getClipItem());

	    if (clip != null && clip.getClipSize() > 0)
	    {
		    retVal = (int)Math.Floor((double)_ammo / clip.getClipSize());
	    }

	    return retVal;
    }

    /*
     * Fires a projectile from crafts weapon.
     * @return Pointer to the new projectile.
     */
    internal CraftWeaponProjectile fire()
    {
	    CraftWeaponProjectile p = new CraftWeaponProjectile();
	    p.setType(this.getRules().getProjectileType());
	    p.setSpeed(this.getRules().getProjectileSpeed());
	    p.setAccuracy(this.getRules().getAccuracy());
	    p.setDamage(this.getRules().getDamage());
	    p.setRange(this.getRules().getRange());
	    return p;
    }

    /**
     * Saves the base to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
	    var node = new YamlMappingNode();
	    node.Add("type", _rules.getType());
	    node.Add("ammo", _ammo.ToString());
	    if (_rearming)
		    node.Add("rearming", _rearming.ToString());
	    return node;
    }
}
