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
    const int DAMAGE_TYPES = 10;
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
}
