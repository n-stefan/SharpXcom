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

enum ForcedTorso { TORSO_USE_GENDER, TORSO_ALWAYS_MALE, TORSO_ALWAYS_FEMALE };

/**
 * Represents a specific type of armor.
 * Not only soldier armor, but also alien armor - some alien races wear
 * Soldier Armor, Leader Armor or Commander Armor depending on their rank.
 */
internal class Armor : IRule
{
    internal const int DAMAGE_TYPES = 10;
    internal const string NONE = "STR_NONE";

    string _type, _spriteSheet, _spriteInv, _corpseGeo, _storeItem, _specWeapon;
    int _frontArmor, _sideArmor, _rearArmor, _underArmor, _drawingRoutine;
    bool _drawBubbles;
    MovementType _movementType;
    int _size, _weight;
    int _deathFrames;
    bool _constantAnimation, _hasInventory;
    ForcedTorso _forcedTorso;
    int _faceColorGroup, _hairColorGroup, _utileColorGroup, _rankColorGroup;
    float[] _damageModifier = new float[DAMAGE_TYPES];
    List<string> _corpseBattle;
    UnitStats _stats;
    List<int> _loftempsSet;
    List<int> _faceColor, _hairColor, _utileColor, _rankColor;
    List<string> _units;

    /**
     * Creates a blank ruleset for a certain
     * type of armor.
     * @param type String defining the type.
     */
    Armor(string type)
    {
        _type = type;
        _frontArmor = 0;
        _sideArmor = 0;
        _rearArmor = 0;
        _underArmor = 0;
        _drawingRoutine = 0;
        _drawBubbles = false;
        _movementType = MovementType.MT_WALK;
        _size = 1;
        _weight = 0;
        _deathFrames = 3;
        _constantAnimation = false;
        _hasInventory = true;
        _forcedTorso = ForcedTorso.TORSO_USE_GENDER;
        _faceColorGroup = 0;
        _hairColorGroup = 0;
        _utileColorGroup = 0;
        _rankColorGroup = 0;
        for (int i = 0; i < DAMAGE_TYPES; i++)
            _damageModifier[i] = 1.0f;
    }

    public IRule Create(string type) =>
        new Armor(type);

    /**
     *
     */
    ~Armor() { }

    /**
     * Gets the size of the unit. Normally this is 1 (small) or 2 (big).
     * @return The unit's size.
     */
    internal int getSize() =>
	    _size;

    /**
     * Gets number of death frames.
     * @return number of death frames.
     */
    internal int getDeathFrames() =>
	    _deathFrames;

    /**
     * Gets the list of corpse items dropped by the unit
     * in the Battlescape (one per unit tile).
     * @return The list of corpse items.
     */
    internal List<string> getCorpseBattlescape() =>
	    _corpseBattle;

    /**
     * Can this unit's inventory be accessed for any reason?
     * @return if we can access the inventory.
     */
    internal bool hasInventory() =>
	    _hasInventory;

    /**
     * Returns the language string that names
     * this armor. Each armor has a unique name. Coveralls, Power Suit,...
     * @return The armor name.
     */
    internal string getType() =>
	    _type;

    /**
     * Gets the storage item needed to equip this.
     * Every soldier armor needs an item.
     * @return The name of the store item (STR_NONE for infinite armor).
     */
    internal string getStoreItem() =>
	    _storeItem;

    /**
     * Loads the armor from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    _type = node["type"].ToString();
	    _spriteSheet = node["spriteSheet"].ToString();
	    _spriteInv = node["spriteInv"].ToString();
	    _hasInventory = bool.Parse(node["allowInv"].ToString());
	    if (node["corpseItem"] != null)
	    {
            _corpseBattle.Clear();
		    _corpseBattle.Add(node["corpseItem"].ToString());
		    _corpseGeo = _corpseBattle[0];
	    }
	    else if (node["corpseBattle"] != null)
	    {
            _corpseBattle = ((YamlSequenceNode)node["corpseBattle"]).Children.Select(x => x.ToString()).ToList();
		    _corpseGeo = _corpseBattle[0];
	    }
	    _corpseGeo = node["corpseGeo"].ToString();
	    _storeItem = node["storeItem"].ToString();
	    _specWeapon = node["specialWeapon"].ToString();
	    _frontArmor = int.Parse(node["frontArmor"].ToString());
	    _sideArmor = int.Parse(node["sideArmor"].ToString());
	    _rearArmor = int.Parse(node["rearArmor"].ToString());
	    _underArmor = int.Parse(node["underArmor"].ToString());
	    _drawingRoutine = int.Parse(node["drawingRoutine"].ToString());
	    _drawBubbles = bool.Parse(node["drawBubbles"].ToString());
	    _movementType = (MovementType)int.Parse(node["movementType"].ToString());
	    _size = int.Parse(node["size"].ToString());
	    _weight = int.Parse(node["weight"].ToString());
        var stats = new UnitStats();
        stats.load(node["stats"]);
        _stats.merge(stats);
	    if (node["damageModifier"] is YamlSequenceNode dmg)
	    {
		    for (var i = 0; i < dmg.Children.Count && i < (uint)DAMAGE_TYPES; ++i)
		    {
			    _damageModifier[i] = float.Parse(dmg.Children[i].ToString());
		    }
	    }
        _loftempsSet = ((YamlSequenceNode)node["loftempsSet"]).Children.Select(x => int.Parse(x.ToString())).ToList();
	    if (node["loftemps"] != null)
	    {
		    _loftempsSet.Clear();
		    _loftempsSet.Add(int.Parse(node["loftemps"].ToString()));
	    }
	    _deathFrames = int.Parse(node["deathFrames"].ToString());
	    _constantAnimation = bool.Parse(node["constantAnimation"].ToString());
	    _forcedTorso = (ForcedTorso)int.Parse(node["forcedTorso"].ToString());

	    _faceColorGroup = int.Parse(node["spriteFaceGroup"].ToString());
	    _hairColorGroup = int.Parse(node["spriteHairGroup"].ToString());
	    _rankColorGroup = int.Parse(node["spriteRankGroup"].ToString());
	    _utileColorGroup = int.Parse(node["spriteUtileGroup"].ToString());
        _faceColor = ((YamlSequenceNode)node["spriteFaceColor"]).Children.Select(x => int.Parse(x.ToString())).ToList();
        _hairColor = ((YamlSequenceNode)node["spriteHairColor"]).Children.Select(x => int.Parse(x.ToString())).ToList();
        _rankColor = ((YamlSequenceNode)node["spriteRankColor"]).Children.Select(x => int.Parse(x.ToString())).ToList();
        _utileColor = ((YamlSequenceNode)node["spriteUtileColor"]).Children.Select(x => int.Parse(x.ToString())).ToList();
        _units = ((YamlSequenceNode)node["units"]).Children.Select(x => x.ToString()).ToList();
    }

    /**
     * Gets the corpse item used in the Geoscape.
     * @return The name of the corpse item.
     */
    internal string getCorpseGeoscape() =>
	    _corpseGeo;

    /** Gets the loftempSet.
     * @return The loftempsSet.
     */
    internal List<int> getLoftempsSet() =>
	    _loftempsSet;

    /**
     * Gets the movement type of this armor.
     * Useful for determining whether the armor can fly.
     * @important: do not use this function outside the BattleUnit constructor,
     * unless you are SURE you know what you are doing.
     * for more information, see the BattleUnit constructor.
     * @return The movement type.
     */
    internal MovementType getMovementType() =>
	    _movementType;

    /**
      * Gets pointer to the armor's stats.
      * @return stats Pointer to the armor's stats.
      */
    internal UnitStats getStats() =>
	    _stats;

    /**
     * Gets whether or not to draw bubbles (breathing animation).
     * @return True if breathing animation is enabled, false otherwise.
     */
    internal bool drawBubbles() =>
	    _drawBubbles;

    /**
     * Gets the front armor level.
     * @return The front armor level.
     */
    internal int getFrontArmor() =>
	    _frontArmor;

    /**
     * Gets the side armor level.
     * @return The side armor level.
     */
    internal int getSideArmor() =>
	    _sideArmor;

    /**
     * Gets the rear armor level.
     * @return The rear armor level.
     */
    internal int getRearArmor() =>
	    _rearArmor;

    /**
     * Gets the under armor level.
     * @return The under armor level.
     */
    internal int getUnderArmor() =>
	    _underArmor;

    /**
     * Gets hair base color group for replacement, if 0 then don't replace colors.
     * @return Color group or 0.
     */
    internal int getFaceColorGroup() =>
	    _faceColorGroup;

    /**
     * Gets new face colors for replacement, if 0 then don't replace colors.
     * @return Color index or 0.
     */
    internal int getFaceColor(int i)
    {
	    if ((uint)i < _faceColor.Count)
	    {
		    return _faceColor[i];
	    }
	    else
	    {
		    return 0;
	    }
    }

    /**
     * Gets hair base color group for replacement, if 0 then don't replace colors.
     * @return Color group or 0.
     */
    internal int getHairColorGroup() =>
	    _hairColorGroup;

    /**
     * Gets new hair colors for replacement, if 0 then don't replace colors.
     * @return Color index or 0.
     */
    internal int getHairColor(int i)
    {
	    if ((uint)i < _hairColor.Count)
	    {
		    return _hairColor[i];
	    }
	    else
	    {
		    return 0;
	    }
    }

    /**
     * Gets utile base color group for replacement, if 0 then don't replace colors.
     * @return Color group or 0.
     */
    internal int getUtileColorGroup() =>
	    _utileColorGroup;

    /**
     * Gets new utile colors for replacement, if 0 then don't replace colors.
     * @return Color index or 0.
     */
    internal int getUtileColor(int i)
    {
	    if ((uint)i < _utileColor.Count)
	    {
		    return _utileColor[i];
	    }
	    else
	    {
		    return 0;
	    }
    }

    /**
     * Gets rank base color group for replacement, if 0 then don't replace colors.
     * @return Color group or 0.
     */
    internal int getRankColorGroup() =>
	    _rankColorGroup;

    /**
     * Gets new rank colors for replacement, if 0 then don't replace colors.
     * @return Color index or 0.
     */
    internal int getRankColor(int i)
    {
	    if ((uint)i < _rankColor.Count)
	    {
		    return _rankColor[i];
	    }
	    else
	    {
		    return 0;
	    }
    }

    /**
     * Gets the damage modifier for a certain damage type.
     * @param dt The damageType.
     * @return The damage modifier 0->1.
     */
    internal float getDamageModifier(ItemDamageType dt) =>
	    _damageModifier[(int)dt];

    /**
     * Gets the armor's weight.
     * @return the weight of the armor.
     */
    internal int getWeight() =>
	    _weight;

    /**
     * Gets the type of special weapon.
     * @return The name of the special weapon.
     */
    internal string getSpecialWeapon() =>
	    _specWeapon;

    /**
    * Gets the list of units this armor applies to.
    * @return The list of unit IDs (empty = applies to all).
    */
    internal List<string> getUnits() =>
	    _units;

    /**
     * Gets the unit's inventory sprite.
     * @return The inventory sprite name.
     */
    internal string getSpriteInventory() =>
	    _spriteInv;

    /**
     * Gets the unit's sprite sheet.
     * @return The sprite sheet name.
     */
    internal string getSpriteSheet() =>
	    _spriteSheet;
}
