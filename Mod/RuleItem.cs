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

enum ItemDamageType { DT_NONE, DT_AP, DT_IN, DT_HE, DT_LASER, DT_PLASMA, DT_STUN, DT_MELEE, DT_ACID, DT_SMOKE };

enum BattleType { BT_NONE, BT_FIREARM, BT_AMMO, BT_MELEE, BT_GRENADE, BT_PROXIMITYGRENADE, BT_MEDIKIT, BT_SCANNER, BT_MINDPROBE, BT_PSIAMP, BT_FLARE, BT_CORPSE };

/**
 * Represents a specific type of item.
 * Contains constant info about an item like
 * storage size, sell price, etc.
 * @sa Item
 */
internal class RuleItem : IListOrder, IRule
{
    string _type, _name; // two types of objects can have the same name
    double _size;
    int _costBuy, _costSell, _transferTime, _weight;
    int _bigSprite, _floorSprite, _handSprite, _bulletSprite;
    int _fireSound, _hitSound, _hitAnimation;
    int _power;
    ItemDamageType _damageType;
    int _accuracyAuto, _accuracySnap, _accuracyAimed, _tuAuto, _tuSnap, _tuAimed;
    int _clipSize, _accuracyMelee, _tuMelee;
    BattleType _battleType;
    bool _twoHanded, _fixedWeapon;
    int _waypoints, _invWidth, _invHeight;
    int _painKiller, _heal, _stimulant;
    int _woundRecovery, _healthRecovery, _stunRecovery, _energyRecovery;
    int _tuUse;
    int _recoveryPoints;
    int _armor;
    int _turretType;
    bool _recover, _ignoreInBaseDefense, _liveAlien;
    int _blastRadius, _attraction;
    bool _flatRate, _arcingShot;
    int _listOrder, _maxRange, _aimRange, _snapRange, _autoRange, _minRange, _dropoff, _bulletSpeed, _explosionSpeed, _autoShots, _shotgunPellets;
    bool _strengthApplied, _skillApplied, _LOSRequired, _underwaterOnly, _landOnly;
    int _meleeSound, _meleePower, _meleeAnimation, _meleeHitSound, _specialType, _vaporColor, _vaporDensity, _vaporProbability;
    List<string> _compatibleAmmo;
    List<string> _requires;
    string _zombieUnit;

    /**
     * Creates a blank ruleset for a certain type of item.
     * @param type String defining the type.
     */
    RuleItem(string type)
    {
        _type = type;
        _name = type;
        _size = 0.0;
        _costBuy = 0;
        _costSell = 0;
        _transferTime = 24;
        _weight = 3;
        _bigSprite = -1;
        _floorSprite = -1;
        _handSprite = 120;
        _bulletSprite = -1;
        _fireSound = -1;
        _hitSound = -1;
        _hitAnimation = -1;
        _power = 0;
        _damageType = ItemDamageType.DT_NONE;
        _accuracyAuto = 0;
        _accuracySnap = 0;
        _accuracyAimed = 0;
        _tuAuto = 0;
        _tuSnap = 0;
        _tuAimed = 0;
        _clipSize = 0;
        _accuracyMelee = 0;
        _tuMelee = 0;
        _battleType = BattleType.BT_NONE;
        _twoHanded = false;
        _fixedWeapon = false;
        _waypoints = 0;
        _invWidth = 1;
        _invHeight = 1;
        _painKiller = 0;
        _heal = 0;
        _stimulant = 0;
        _woundRecovery = 0;
        _healthRecovery = 0;
        _stunRecovery = 0;
        _energyRecovery = 0;
        _tuUse = 0;
        _recoveryPoints = 0;
        _armor = 20;
        _turretType = -1;
        _recover = true;
        _ignoreInBaseDefense = false;
        _liveAlien = false;
        _blastRadius = -1;
        _attraction = 0;
        _flatRate = false;
        _arcingShot = false;
        _listOrder = 0;
        _maxRange = 200;
        _aimRange = 200;
        _snapRange = 15;
        _autoRange = 7;
        _minRange = 0;
        _dropoff = 2;
        _bulletSpeed = 0;
        _explosionSpeed = 0;
        _autoShots = 3;
        _shotgunPellets = 0;
        _strengthApplied = false;
        _skillApplied = true;
        _LOSRequired = false;
        _underwaterOnly = false;
        _landOnly = false;
        _meleeSound = 39;
        _meleePower = 0;
        _meleeAnimation = 0;
        _meleeHitSound = -1;
        _specialType = -1;
        _vaporColor = -1;
        _vaporDensity = 0;
        _vaporProbability = 15;
    }

    public IRule Create(string type) =>
        new RuleItem(type);

    /**
     *
     */
    ~RuleItem() { }

    /**
     * Gets the item's battle type.
     * @return The battle type.
     */
    internal BattleType getBattleType() =>
	    _battleType;

    /**
     * Gets the item's ammo clip size.
     * @return The ammo clip size.
     */
    internal int getClipSize() =>
	    _clipSize;

    /**
     * Gets the heal quantity of the item.
     * @return The new heal quantity.
     */
    internal int getHealQuantity() =>
	    _heal;

    /**
     * Gets the pain killer quantity of the item.
     * @return The new pain killer quantity.
     */
    internal int getPainKillerQuantity() =>
	    _painKiller;

    /**
     * Gets the stimulant quantity of the item.
     * @return The new stimulant quantity.
     */
    internal int getStimulantQuantity() =>
	    _stimulant;

    /**
     * Gets a list of compatible ammo.
     * @return Pointer to a list of compatible ammo.
     */
    internal List<string> getCompatibleAmmo() =>
        _compatibleAmmo;

    /**
     * Gets the item's bullet sprite reference.
     * @return The sprite reference.
     */
    internal int getBulletSprite() =>
	    _bulletSprite;

    /**
     * Gets the color offset to use for the vapor trail.
     * @return the color offset.
     */
    internal int getVaporColor() =>
	    _vaporColor;

    /**
     * Gets the vapor cloud density for the vapor trail.
     * @return the vapor density.
     */
    internal int getVaporDensity() =>
	    _vaporDensity;

    /**
     * Gets the vapor cloud probability for the vapor trail.
     * @return the vapor probability.
     */
    internal int getVaporProbability() =>
	    _vaporProbability;

    /**
     * Gets the speed at which this bullet travels.
     * @return The speed.
     */
    internal int getBulletSpeed() =>
	    _bulletSpeed;

    /**
     * Gets the reference in FLOOROB.PCK for use in inventory.
     * @return The sprite reference.
     */
    internal int getFloorSprite() =>
	    _floorSprite;

    /**
     * Returns whether this item uses waypoints.
     * @return True if it uses waypoints.
     */
    internal int getWaypoints() =>
	    _waypoints;

    /**
     * Gets the item's time unit percentage for snapshots.
     * @return The snapshot TU percentage.
     */
    internal int getTUSnap() =>
	    _tuSnap;

    /**
     * Gets the item's width in a soldier's inventory.
     * @return The width.
     */
    internal int getInventoryWidth() =>
	    _invWidth;

    /**
     * Gets the item's height in a soldier's inventory.
     * @return The height.
     */
    internal int getInventoryHeight() =>
	    _invHeight;

    /**
     * Gets the item's time unit percentage for autoshots.
     * @return The autoshot TU percentage.
     */
    internal int getTUAuto() =>
	    _tuAuto;

    /**
     * Gets the item's time unit percentage for melee attacks.
     * @return The melee TU percentage.
     */
    internal int getTUMelee() =>
	    _tuMelee;

    /**
     * Gets the item's time unit percentage for aimed shots.
     * @return The aimed shot TU percentage.
     */
    internal int getTUAimed() =>
	    _tuAimed;

    /**
     * Gets the number of Time Units needed to use this item.
     * @return The number of Time Units needed to use this item.
     */
    internal int getTUUse() =>
	    _tuUse;

    /**
     * Returns whether this item charges a flat TU rate.
     * @return True if this item charges a flat TU rate.
     */
    internal bool getFlatRate() =>
	    _flatRate;

    /**
     * Gets the language string that names
     * this item. This is not necessarily unique.
     * @return  The item's name.
     */
    internal string getName() =>
	    _name;

    /**
     * Gets the item's damage type.
     * @return The damage type.
     */
    internal ItemDamageType getDamageType() =>
	    _damageType;

    /**
     * Returns whether this item is a fixed weapon.
     * You can't move/throw/drop fixed weapons - e.g. HWP turrets.
     * @return True if it is a fixed weapon.
     */
    internal bool isFixed() =>
	    _fixedWeapon;

    /**
     * Gets the item's power.
     * @return The power.
     */
    internal int getPower() =>
	    _power;

    /**
     * Gets the item type. Each item has a unique type.
     * @return The item's type.
     */
    internal string getType() =>
        _type;

    /**
     * Gets the list weight for this research item
     * @return The list weight.
     */
    public int getListOrder() =>
        _listOrder;

    /**
     * Loads the item from a YAML file.
     * @param node YAML node.
     * @param mod Mod for the item.
     * @param listOrder The list weight for this item.
     */
    internal void load(YamlNode node, Mod mod, int listOrder)
    {
        _type = node["type"].ToString();
        _name = node["name"].ToString();
        _requires = ((YamlSequenceNode)node["requires"]).Children.Select(x => x.ToString()).ToList();
	    _size = double.Parse(node["size"].ToString());
	    _costBuy = int.Parse(node["costBuy"].ToString());
	    _costSell = int.Parse(node["costSell"].ToString());
	    _transferTime = int.Parse(node["transferTime"].ToString());
	    _weight = int.Parse(node["weight"].ToString());

	    mod.loadSpriteOffset(_type, _bigSprite, node["bigSprite"], "BIGOBS.PCK");
	    mod.loadSpriteOffset(_type, _floorSprite, node["floorSprite"], "FLOOROB.PCK");
	    mod.loadSpriteOffset(_type, _handSprite, node["handSprite"], "HANDOB.PCK");
	    // Projectiles: 0-384 entries ((105*33) / (3*3)) (35 sprites per projectile(0-34), 11 projectiles (0-10))
	    mod.loadSpriteOffset(_type, _bulletSprite, node["bulletSprite"], "Projectiles", 35);

	    mod.loadSoundOffset(_type, _fireSound, node["fireSound"], "BATTLE.CAT");
	    mod.loadSoundOffset(_type, _hitSound, node["hitSound"], "BATTLE.CAT");
	    mod.loadSoundOffset(_type, _meleeSound, node["meleeSound"], "BATTLE.CAT");
	    mod.loadSpriteOffset(_type, _hitAnimation, node["hitAnimation"], "SMOKE.PCK");
	    mod.loadSpriteOffset(_type, _meleeAnimation, node["meleeAnimation"], "HIT.PCK");
	    mod.loadSoundOffset(_type, _meleeHitSound, node["meleeHitSound"], "BATTLE.CAT");

	    _power = int.Parse(node["power"].ToString());
        _compatibleAmmo = ((YamlSequenceNode)node["compatibleAmmo"]).Children.Select(x => x.ToString()).ToList();
	    _damageType = (ItemDamageType)int.Parse(node["damageType"].ToString());
	    _accuracyAuto = int.Parse(node["accuracyAuto"].ToString());
	    _accuracySnap = int.Parse(node["accuracySnap"].ToString());
	    _accuracyAimed = int.Parse(node["accuracyAimed"].ToString());
	    _tuAuto = int.Parse(node["tuAuto"].ToString());
	    _tuSnap = int.Parse(node["tuSnap"].ToString());
	    _tuAimed = int.Parse(node["tuAimed"].ToString());
	    _clipSize = int.Parse(node["clipSize"].ToString());
	    _accuracyMelee = int.Parse(node["accuracyMelee"].ToString());
	    _tuMelee = int.Parse(node["tuMelee"].ToString());
	    _battleType = (BattleType)int.Parse(node["battleType"].ToString());
	    if ((_battleType == BattleType.BT_MELEE || _battleType == BattleType.BT_FIREARM) && _clipSize == 0 && !_compatibleAmmo.Any())
	    {
		    throw new Exception("Weapon " + _type + " has clip size 0 and no ammo defined. Please use 'clipSize: -1' for unlimited ammo, or allocate a compatibleAmmo item.");
	    }
	    _twoHanded = bool.Parse(node["twoHanded"].ToString());
	    _waypoints = int.Parse(node["waypoints"].ToString());
	    _fixedWeapon = bool.Parse(node["fixedWeapon"].ToString());
	    _invWidth = int.Parse(node["invWidth"].ToString());
	    _invHeight = int.Parse(node["invHeight"].ToString());
	    _painKiller = int.Parse(node["painKiller"].ToString());
	    _heal = int.Parse(node["heal"].ToString());
	    _stimulant = int.Parse(node["stimulant"].ToString());
	    _woundRecovery = int.Parse(node["woundRecovery"].ToString());
	    _healthRecovery = int.Parse(node["healthRecovery"].ToString());
	    _stunRecovery = int.Parse(node["stunRecovery"].ToString());
	    _energyRecovery = int.Parse(node["energyRecovery"].ToString());
	    _tuUse = int.Parse(node["tuUse"].ToString());
	    _recoveryPoints = int.Parse(node["recoveryPoints"].ToString());
	    _armor = int.Parse(node["armor"].ToString());
	    _turretType = int.Parse(node["turretType"].ToString());
	    _recover = bool.Parse(node["recover"].ToString());
	    _ignoreInBaseDefense = bool.Parse(node["ignoreInBaseDefense"].ToString());
	    _liveAlien = bool.Parse(node["liveAlien"].ToString());
	    _blastRadius = int.Parse(node["blastRadius"].ToString());
	    _attraction = int.Parse(node["attraction"].ToString());
	    _flatRate = bool.Parse(node["flatRate"].ToString());
	    _arcingShot = bool.Parse(node["arcingShot"].ToString());
	    _listOrder = int.Parse(node["listOrder"].ToString());
	    _maxRange = int.Parse(node["maxRange"].ToString());
	    _aimRange = int.Parse(node["aimRange"].ToString());
	    _snapRange = int.Parse(node["snapRange"].ToString());
	    _autoRange = int.Parse(node["autoRange"].ToString());
	    _minRange = int.Parse(node["minRange"].ToString());
	    _dropoff = int.Parse(node["dropoff"].ToString());
	    _bulletSpeed = int.Parse(node["bulletSpeed"].ToString());
	    _explosionSpeed = int.Parse(node["explosionSpeed"].ToString());
	    _autoShots = int.Parse(node["autoShots"].ToString());
	    _shotgunPellets = int.Parse(node["shotgunPellets"].ToString());
	    _zombieUnit = node["zombieUnit"].ToString();
	    _strengthApplied = bool.Parse(node["strengthApplied"].ToString());
	    _skillApplied = bool.Parse(node["skillApplied"].ToString());
	    _LOSRequired = bool.Parse(node["LOSRequired"].ToString());
	    _meleePower = int.Parse(node["meleePower"].ToString());
	    _underwaterOnly = bool.Parse(node["underwaterOnly"].ToString());
	    _landOnly = bool.Parse(node["landOnly"].ToString());
	    _specialType = int.Parse(node["specialType"].ToString());
	    mod.loadTransparencyOffset(_type, _vaporColor, node["vaporColor"]);
	    _vaporDensity = int.Parse(node["vaporDensity"].ToString());
	    _vaporProbability = int.Parse(node["vaporProbability"].ToString());
	    if (_listOrder == 0)
	    {
            _listOrder = listOrder;
	    }
    }

    /**
     * Gets the amount of space this item
     * takes up in a storage facility.
     * @return The storage size.
     */
    internal double getSize() =>
	    _size;

    /**
     * Gets the amount of money this item
     * is worth to sell.
     * @return The sell cost.
     */
    internal int getSellCost() =>
	    _costSell;

    /**
     * Returns the item's Turret Type.
     * @return The turret index (-1 for no turret).
     */
    internal int getTurretType() =>
	    _turretType;

    /**
     * Returns the item's armor.
     * The item is destroyed when an explosion power bigger than its armor hits it.
     * @return The armor.
     */
    internal int getArmor() =>
	    _armor;

    /**
     * Draws and centers the hand sprite on a surface
     * according to its dimensions.
     * @param texture Pointer to the surface set to get the sprite from.
     * @param surface Pointer to the surface to draw to.
     */
    internal void drawHandSprite(SurfaceSet texture, Surface surface)
    {
	    Surface frame = texture.getFrame(this.getBigSprite());
	    frame.setX((RuleInventory.HAND_W - this.getInventoryWidth()) * RuleInventory.SLOT_W/2);
	    frame.setY((RuleInventory.HAND_H - this.getInventoryHeight()) * RuleInventory.SLOT_H/2);
	    texture.getFrame(this.getBigSprite()).blit(surface);
    }

    /**
     * Gets the reference in BIGOBS.PCK for use in inventory.
     * @return The sprite reference.
     */
    internal int getBigSprite() =>
	    _bigSprite;

    /**
     * Gets the weight of the item.
     * @return The weight in strength units.
     */
    internal int getWeight() =>
	    _weight;

    /**
    * Checks if the item can be equipped in base defense mission.
    * @return True if it can be equipped.
    */
    internal bool canBeEquippedBeforeBaseDefense() =>
	    !_ignoreInBaseDefense;

    /**
     * Gets the list of research required to
     * use this item.
     * @return The list of research IDs.
     */
    internal List<string> getRequirements() =>
	    _requires;

    /**
     * is this item a rifle?
     * @return whether or not it is a rifle.
     */
    internal bool isRifle() =>
	    (_battleType == BattleType.BT_FIREARM || _battleType == BattleType.BT_MELEE) && _twoHanded;

    /**
     * is this item a pistol?
     * @return whether or not it is a pistol.
     */
    internal bool isPistol() =>
	    (_battleType == BattleType.BT_FIREARM || _battleType == BattleType.BT_MELEE) && !_twoHanded;

    /**
     * Returns if the item should be recoverable
     * from the battlescape.
     * @return True if it is recoverable.
     */
    internal bool isRecoverable() =>
	    _recover;

    /**
     * Returns if this is a live alien.
     * @return True if this is a live alien.
     */
    internal bool isAlien() =>
	    _liveAlien;

    /**
     * Gets the amount of time this item
     * takes to arrive at a base.
     * @return The time in hours.
     */
    internal int getTransferTime() =>
	    _transferTime;

    /**
     * Gets the amount of money this item
     * costs to purchase (0 if not purchasable).
     * @return The buy cost.
     */
    internal int getBuyCost() =>
	    _costBuy;
}
