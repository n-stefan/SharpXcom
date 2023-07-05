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

namespace SharpXcom.Battlescape;

/**
 * A utility class that generates the initial battlescape data. Taking into account mission type, craft and ufo involved, terrain type,...
 */
internal class BattlescapeGenerator
{
    Game _game;
    SavedBattleGame _save;
    Mod.Mod _mod;
    Craft _craft;
    Ufo _ufo;
    Base _base;
    MissionSite _mission;
    AlienBase _alienBase;
    RuleTerrain _terrain;
    int _mapsize_x, _mapsize_y, _mapsize_z;
    Texture _worldTexture;
    int _worldShade;
    int _unitSequence;
    Tile _craftInventoryTile;
    int _alienItemLevel;
    bool _allowAutoLoadout, _baseInventory, _generateFuel, _craftDeployed;
    int _craftZ;
    int _blocksToDo;
    MapBlock _dummy;
    string _alienRace;
    SDL_Rect _craftPos;
    List<List<MapBlock>> _blocks;
    List<SDL_Rect> _ufoPos;
    List<List<int>> _segments, _drillMap;
    List<List<bool>> _landingzone;

    /**
     * Sets up a BattlescapeGenerator.
     * @param game pointer to Game object.
     */
    internal BattlescapeGenerator(Game game)
    {
        _game = game;
        _save = game.getSavedGame().getSavedBattle();
        _mod = game.getMod();
        _craft = null;
        _ufo = null;
        _base = null;
        _mission = null;
        _alienBase = null;
        _terrain = null;
        _mapsize_x = 0;
        _mapsize_y = 0;
        _mapsize_z = 0;
        _worldTexture = null;
        _worldShade = 0;
        _unitSequence = 0;
        _craftInventoryTile = null;
        _alienItemLevel = 0;
        _baseInventory = false;
        _generateFuel = true;
        _craftDeployed = false;
        _craftZ = 0;
        _blocksToDo = 0;
        _dummy = null;

        _allowAutoLoadout = !Options.disableAutoEquip;
    }

    /**
     * Deletes the BattlescapeGenerator.
     */
    ~BattlescapeGenerator() { }

    /**
     * Sets the XCom base involved in the battle.
     * @param base Pointer to XCom base.
     */
    internal void setBase(Base @base)
    {
        _base = @base;
        _base.setInBattlescape(true);
    }

    /**
     * Sets the alien race on the mission. This is used to determine the various alien types to spawn.
     * @param alienRace Alien (main) race.
     */
    internal void setAlienRace(string alienRace) =>
        _alienRace = alienRace;

    /**
     * Starts the generator; it fills up the battlescapesavegame with data.
     */
    internal void run()
    {
        AlienDeployment ruleDeploy = _game.getMod().getDeployment(_ufo != null ? _ufo.getRules().getType() : _save.getMissionType(), true);

        _save.setTurnLimit(ruleDeploy.getTurnLimit());
        _save.setChronoTrigger(ruleDeploy.getChronoTrigger());
        _save.setCheatTurn(ruleDeploy.getCheatTurn());
        ruleDeploy.getDimensions(out _mapsize_x, out _mapsize_y, out _mapsize_z);

        _unitSequence = BattleUnit.MAX_SOLDIER_ID; // geoscape soldier IDs should stay below this number

        if (_terrain == null)
        {
            if (_worldTexture == null || !_worldTexture.getTerrain().Any() || ruleDeploy.getTerrains().Any())
            {
                if (ruleDeploy.getTerrains().Any())
                {
                    int pick = RNG.generate(0, ruleDeploy.getTerrains().Count - 1);
                    _terrain = _game.getMod().getTerrain(ruleDeploy.getTerrains()[pick], true);
                }
                else // trouble: no texture and no deployment terrain, most likely scenario is a UFO landing on water: use the first available terrain
                {
                    _terrain = _game.getMod().getTerrain(_game.getMod().getTerrainList().First(), true);
                }
            }
            else
            {
                Target target = _ufo;
                if (_mission != null) target = _mission;
                _terrain = _game.getMod().getTerrain(_worldTexture.getRandomTerrain(target), true);
            }
        }

        if (_terrain == null)
        {
            throw new Exception("Map generator encountered an error: No valid terrain found.");
        }

        setDepth(ruleDeploy, false);

        if (ruleDeploy.getShade() != -1)
        {
            _worldShade = ruleDeploy.getShade();
        }

        List<MapScript> script = _game.getMod().getMapScript(_terrain.getScript());
        if (_game.getMod().getMapScript(ruleDeploy.getScript()) != null)
        {
            script = _game.getMod().getMapScript(ruleDeploy.getScript());
        }
        else if (!string.IsNullOrEmpty(ruleDeploy.getScript()))
        {
            throw new Exception("Map generator encountered an error: " + ruleDeploy.getScript() + " script not found.");
        }
        if (script == null)
        {
            throw new Exception("Map generator encountered an error: " + _terrain.getScript() + " script not found.");
        }

        generateMap(script);

        setupObjectives(ruleDeploy);

        deployXCOM();

        int unitCount = _save.getUnits().Count;

        deployAliens(ruleDeploy);

        if (unitCount == _save.getUnits().Count)
        {
            throw new Exception("Map generator encountered an error: no alien units could be placed on the map.");
        }

        deployCivilians(ruleDeploy.getCivilians());

        if (_generateFuel)
        {
            fuelPowerSources();
        }

        if (_ufo != null && _ufo.getStatus() == UfoStatus.CRASHED)
        {
            explodePowerSources();
        }

        setMusic(ruleDeploy, false);
        // set shade (alien bases are a little darker, sites depend on worldshade)
        _save.setGlobalShade(_worldShade);

        _save.getTileEngine().calculateSunShading();
        _save.getTileEngine().calculateTerrainLighting();
        _save.getTileEngine().calculateUnitLighting();
    }

    /**
    * Sets the depth based on the terrain or the provided AlienDeployment rule.
    * @param ruleDeploy the deployment data we're gleaning data from.
    * @param nextStage whether the mission is progressing to the next stage.
    */
    void setDepth(AlienDeployment ruleDeploy, bool nextStage)
    {
        if (_save.getDepth() > 0 && !nextStage)
        {
            // new battle menu will have set the depth already
            return;
        }

        if (ruleDeploy.getMaxDepth() > 0)
        {
            _save.setDepth(RNG.generate(ruleDeploy.getMinDepth(), ruleDeploy.getMaxDepth()));
        }
        else if (_terrain.getMaxDepth() > 0 || nextStage)
        {
            _save.setDepth(RNG.generate(_terrain.getMinDepth(), _terrain.getMaxDepth()));
        }
    }

    /**
    * Sets the background music based on the terrain or the provided AlienDeployment rule.
    * @param ruleDeploy the deployment data we're gleaning data from.
    * @param nextStage whether the mission is progressing to the next stage.
    */
    void setMusic(AlienDeployment ruleDeploy, bool nextStage)
    {
        if (ruleDeploy.getMusic().Any())
        {
            _save.setMusic(ruleDeploy.getMusic()[RNG.generate(0, ruleDeploy.getMusic().Count - 1)]);
        }
        else if (_terrain.getMusic().Any())
        {
            _save.setMusic(_terrain.getMusic()[RNG.generate(0, _terrain.getMusic().Count - 1)]);
        }
        else if (nextStage)
        {
            _save.setMusic(string.Empty);
        }
    }

    /**
     * Fill power sources with an alien fuel object.
     */
    void fuelPowerSources()
    {
        for (int i = 0; i < _save.getMapSizeXYZ(); ++i)
        {
            if (_save.getTiles()[i].getMapData(TilePart.O_OBJECT) != null
                && _save.getTiles()[i].getMapData(TilePart.O_OBJECT).getSpecialType() == SpecialTileType.UFO_POWER_SOURCE)
            {
                BattleItem alienFuel = new BattleItem(_game.getMod().getItem(_game.getMod().getAlienFuelName(), true), ref _save.getCurrentItemId());
                _save.getItems().Add(alienFuel);
                _save.getTiles()[i].addItem(alienFuel, _game.getMod().getInventory("STR_GROUND", true));
            }
        }
    }

    /**
     * When a UFO crashes, there is a 75% chance for each powersource to explode.
     */
    void explodePowerSources()
    {
        for (int i = 0; i < _save.getMapSizeXYZ(); ++i)
        {
            if (_save.getTiles()[i].getMapData(TilePart.O_OBJECT) != null
                && _save.getTiles()[i].getMapData(TilePart.O_OBJECT).getSpecialType() == SpecialTileType.UFO_POWER_SOURCE && RNG.percent(75))
            {
                var pos = new Position();
                pos.x = _save.getTiles()[i].getPosition().x * 16;
                pos.y = _save.getTiles()[i].getPosition().y * 16;
                pos.z = (_save.getTiles()[i].getPosition().z * 24) + 12;
                _save.getTileEngine().explode(pos, 180 + RNG.generate(0, 70), ItemDamageType.DT_HE, 10);
            }
        }
        Tile t = _save.getTileEngine().checkForTerrainExplosions();
        while (t != null)
        {
            Position p = new Position(t.getPosition().x * 16, t.getPosition().y * 16, t.getPosition().z * 24);
            p += new Position(8, 8, 0);
            _save.getTileEngine().explode(p, t.getExplosive(), ItemDamageType.DT_HE, t.getExplosive() / 10);
            t = _save.getTileEngine().checkForTerrainExplosions();
        }
    }

    /**
     * Spawns civilians on a terror mission.
     * @param max Maximum number of civilians to spawn.
     */
    void deployCivilians(int max)
    {
        if (max != 0)
        {
            // inevitably someone will point out that ufopaedia says 0-16 civilians.
            // to that person: this is only partially true;
            // 0 civilians would only be a possibility if there were already 80 units,
            // or no spawn nodes for civilians, but it would always try to spawn at least 8.
            int number = RNG.generate(max / 2, max);

            if (number > 0)
            {
                int month;
                if (_game.getSavedGame().getMonthsPassed() != -1)
                {
                    month =
                    ((uint)_game.getSavedGame().getMonthsPassed()) > _game.getMod().getAlienItemLevels().Count - 1 ?  // if
                    _game.getMod().getAlienItemLevels().Count - 1 : // then
                    _game.getSavedGame().getMonthsPassed();  // else
                }
                else
                {
                    month = _alienItemLevel;
                }
                for (int i = 0; i < number; ++i)
                {
                    int pick = RNG.generate(0, _terrain.getCivilianTypes().Count - 1);
                    Unit rule = _game.getMod().getUnit(_terrain.getCivilianTypes()[pick], true);
                    BattleUnit civ = addCivilian(rule);
                    if (civ != null)
                    {
                        int itemLevel = _game.getMod().getAlienItemLevels()[month][RNG.generate(0, 9)];
                        // Built in weapons: civilians may have levelled item lists with randomized distributions
                        // following the same basic rules as the alien item levels.
                        if (rule.getBuiltInWeapons().Any())
                        {
                            if (itemLevel >= rule.getBuiltInWeapons().Count)
                            {
                                itemLevel = rule.getBuiltInWeapons().Count - 1;
                            }
                            foreach (var j in rule.getBuiltInWeapons()[itemLevel])
                            {
                                RuleItem ruleItem = _game.getMod().getItem(j);
                                if (ruleItem != null)
                                {
                                    BattleItem item = new BattleItem(ruleItem, ref _save.getCurrentItemId());
                                    if (!addItem(item, civ))
                                    {
                                        item = null;
                                    }
                                    else if (ruleItem.getTurretType() != -1)
                                    {
                                        civ.setTurretType(ruleItem.getTurretType());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /**
     * Adds a civilian to the game and places him on a free spawnpoint.
     * @param rules Pointer to the Unit which holds info about the civilian.
     * @return Pointer to the created unit.
     */
    BattleUnit addCivilian(Unit rules)
    {
        BattleUnit unit = new BattleUnit(rules, UnitFaction.FACTION_NEUTRAL, _unitSequence++, _game.getMod().getArmor(rules.getArmor(), true), default, _save.getDepth());
        Node node = _save.getSpawnNode(0, unit);

        if (node != null)
        {
            _save.setUnitPosition(unit, node.getPosition());
            unit.setAIModule(new AIModule(_save, unit, node));
            unit.setDirection(RNG.generate(0, 7));

            // we only add a unit if it has a node to spawn on.
            // (stops them spawning at 0,0,0)
            _save.getUnits().Add(unit);
        }
        else if (placeUnitNearFriend(unit))
        {
            unit.setAIModule(new AIModule(_save, unit, node));
            unit.setDirection(RNG.generate(0, 7));
            _save.getUnits().Add(unit);
        }
        else
        {
            unit = null;
        }
        return unit;
    }

    /**
     * Places a unit near a friendly unit.
     * @param unit Pointer to the unit in question.
     * @return If we successfully placed the unit.
     */
    bool placeUnitNearFriend(BattleUnit unit)
    {
        if (!_save.getUnits().Any())
        {
            return false;
        }
        for (int i = 0; i != 10; ++i)
        {
            Position entryPoint = new Position(-1, -1, -1);
            int tries = 100;
            bool largeUnit = false;
            while (entryPoint == new Position(-1, -1, -1) && tries != 0)
            {
                BattleUnit k = _save.getUnits()[RNG.generate(0, _save.getUnits().Count - 1)];
                if (k.getFaction() == unit.getFaction() && k.getPosition() != new Position(-1, -1, -1) && k.getArmor().getSize() >= unit.getArmor().getSize())
                {
                    entryPoint = k.getPosition();
                    largeUnit = (k.getArmor().getSize() != 1);
                }
                --tries;
            }
            if (tries != 0 && _save.placeUnitNearPosition(unit, entryPoint, largeUnit))
            {
                return true;
            }
        }
        return false;
    }

    /**
     * Adds an item to an XCom soldier (auto-equip).
     * @param item Pointer to the Item.
     * @param unit Pointer to the Unit.
     * @param allowSecondClip allow the unit to take a second clip or not. (only applies to xcom soldiers, aliens are allowed regardless of this flag)
     * @return if the item was placed or not.
     */
    bool addItem(BattleItem item, BattleUnit unit, bool allowSecondClip = false) =>
        _addItem(item, unit, _game.getMod(), _save, _allowAutoLoadout, allowSecondClip);

    /**
     * Sets up the objectives for the map.
     * @param ruleDeploy the deployment data we're gleaning data from.
     */
    void setupObjectives(AlienDeployment ruleDeploy)
    {
        int targetType = ruleDeploy.getObjectiveType();

        if (targetType > -1)
        {
            int objectives = ruleDeploy.getObjectivesRequired();
            int actualCount = 0;

            for (int i = 0; i < _save.getMapSizeXYZ(); ++i)
            {
                for (int j = (int)TilePart.O_FLOOR; j <= (int)TilePart.O_OBJECT; ++j)
                {
                    TilePart tp = (TilePart)j;
                    if (_save.getTiles()[i].getMapData(tp) != null && (int)_save.getTiles()[i].getMapData(tp).getSpecialType() == targetType)
                    {
                        actualCount++;
                    }
                }
            }

            if (actualCount > 0)
            {
                _save.setObjectiveType(targetType);

                if (actualCount < objectives || objectives == 0)
                {
                    _save.setObjectiveCount(actualCount);
                }
                else
                {
                    _save.setObjectiveCount(objectives);
                }
            }
        }
    }

    /**
     * Adds an item to an XCom soldier (auto-equip).
     * @param item Pointer to the Item.
     * @param unit Pointer to the Unit.
     * @param mod Pointer to the mod in use by the game.
     * @param addToSave if non-NULL and the item was successfully placed, will add the item to the specified SavedBattleGame
     * @param allowAutoLoadout allow auto-equip to function
     * @param allowSecondClip allow the unit to take a second clip or not. (only applies to xcom soldiers, aliens are allowed regardless of this flag)
     * @return if the item was placed.
     */
    static bool _addItem(BattleItem item, BattleUnit unit, Mod.Mod mod, SavedBattleGame addToSave, bool allowAutoLoadout, bool allowSecondClip)
    {
        RuleInventory rightHand = mod.getInventory("STR_RIGHT_HAND", true);
        RuleInventory leftHand = mod.getInventory("STR_LEFT_HAND", true);
        bool placed = false;
        bool loaded = false;
        BattleItem rightWeapon = unit.getItem("STR_RIGHT_HAND");
        BattleItem leftWeapon = unit.getItem("STR_LEFT_HAND");
        int weight = 0;

        // tanks and aliens don't care about weight or multiple items,
        // their loadouts are defined in the rulesets and more or less set in stone.
        if (unit.getFaction() == UnitFaction.FACTION_PLAYER && unit.hasInventory())
        {
            weight = unit.getCarriedWeight() + item.getRules().getWeight();
            if (item.getAmmoItem() != null && item.getAmmoItem() != item)
            {
                weight += item.getAmmoItem().getRules().getWeight();
            }
            // allow all weapons to be loaded by avoiding this check,
            // they'll return false later anyway if the unit has something in his hand.
            if (!item.getRules().getCompatibleAmmo().Any())
            {
                int tally = 0;
                foreach (var i in unit.getInventory())
                {
                    if (item.getRules().getType() == i.getRules().getType())
                    {
                        if (allowSecondClip && item.getRules().getBattleType() == BattleType.BT_AMMO)
                        {
                            tally++;
                            if (tally == 2)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            // we already have one, thanks.
                            return false;
                        }
                    }
                }
            }
        }
        // fixed weapon should be always placed in hand slots
        if (item.getRules().isFixed())
        {
            if (rightWeapon == null || leftWeapon == null)
            {
                item.moveToOwner(unit);
                item.setSlot(rightWeapon == null ? rightHand : leftHand);
                placed = true;
                if (addToSave != null)
                {
                    addToSave.getItems().Add(item);
                }
                item.setXCOMProperty(unit.getFaction() == UnitFaction.FACTION_PLAYER);
            }
            return placed;
        }

        bool keep = true;
        switch (item.getRules().getBattleType())
        {
            case BattleType.BT_FIREARM:
            case BattleType.BT_MELEE:
                if (item.getAmmoItem() != null || unit.getFaction() != UnitFaction.FACTION_PLAYER || !unit.hasInventory())
                {
                    loaded = true;
                }

                if (loaded && (unit.getGeoscapeSoldier() == null || allowAutoLoadout))
                {
                    if (rightWeapon == null && unit.getBaseStats().strength * 0.66 >= weight) // weight is always considered 0 for aliens
                    {
                        item.moveToOwner(unit);
                        item.setSlot(rightHand);
                        placed = true;
                    }
                    else if (leftWeapon == null && unit.getFaction() != UnitFaction.FACTION_PLAYER)
                    {
                        item.moveToOwner(unit);
                        item.setSlot(leftHand);
                        placed = true;
                    }
                }
                break;
            case BattleType.BT_AMMO:
                // xcom weapons will already be loaded, aliens and tanks, however, get their ammo added afterwards.
                // so let's try to load them here.
                if (rightWeapon != null && (rightWeapon.getRules().isFixed() || unit.getFaction() != UnitFaction.FACTION_PLAYER) &&
                    rightWeapon.getRules().getCompatibleAmmo().Any() &&
                    rightWeapon.getAmmoItem() == null &&
                    rightWeapon.setAmmoItem(item) == 0)
                {
                    item.setSlot(rightHand);
                    placed = true;
                    break;
                }
                if (leftWeapon != null && (leftWeapon.getRules().isFixed() || unit.getFaction() != UnitFaction.FACTION_PLAYER) &&
                    leftWeapon.getRules().getCompatibleAmmo().Any() &&
                    leftWeapon.getAmmoItem() == null &&
                    leftWeapon.setAmmoItem(item) == 0)
                {
                    item.setSlot(leftHand);
                    placed = true;
                    break;
                }
                // don't take ammo for weapons we don't have.
                keep = (unit.getFaction() != UnitFaction.FACTION_PLAYER);
                if (rightWeapon != null)
                {
                    foreach (var i in rightWeapon.getRules().getCompatibleAmmo())
                    {
                        if (i == item.getRules().getType())
                        {
                            keep = true;
                            break;
                        }
                    }
                }
                if (leftWeapon != null)
                {
                    foreach (var i in leftWeapon.getRules().getCompatibleAmmo())
                    {
                        if (i == item.getRules().getType())
                        {
                            keep = true;
                            break;
                        }
                    }
                }
                if (!keep)
                {
                    break;
                }
                goto default;
            default:
                if ((unit.getGeoscapeSoldier() == null || allowAutoLoadout))
                {
                    if (unit.getBaseStats().strength >= weight) // weight is always considered 0 for aliens
                    {
                        var invsList = mod.getInvsList();
                        for (var i = 0; i < invsList.Count && !placed; ++i)
                        {
                            RuleInventory slot = mod.getInventory(invsList[i]);
                            if (slot.getType() == InventoryType.INV_SLOT)
                            {
                                var slots = slot.getSlots();
                                for (var j = 0; j < slots.Count && !placed; ++j)
                                {
                                    if (!Inventory.overlapItems(unit, item, slot, slots[j].x, slots[j].y) && slot.fitItemInSlot(item.getRules(), slots[j].x, slots[j].y))
                                    {
                                        item.moveToOwner(unit);
                                        item.setSlot(slot);
                                        item.setSlotX(slots[j].x);
                                        item.setSlotY(slots[j].y);
                                        placed = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                break;
        }

        if (placed && addToSave != null)
        {
            addToSave.getItems().Add(item);
        }
        item.setXCOMProperty(unit.getFaction() == UnitFaction.FACTION_PLAYER);

        return placed;
    }

    /**
     * Deploys all the X-COM units and equipment based
     * on the Geoscape base / craft.
     * @param inventoryTile The tile to place all the extra equipment on.
     */
    void deployXCOM()
    {
        RuleInventory ground = _game.getMod().getInventory("STR_GROUND", true);

        if (_craft != null)
            _base = _craft.getBase();

        // add vehicles that are in the craft - a vehicle is actually an item, which you will never see as it is converted to a unit
        // however the item itself becomes the weapon it "holds".
        if (!_baseInventory)
        {
            if (_craft != null)
            {
                foreach (var i in _craft.getVehicles())
                {
                    BattleUnit unit = addXCOMVehicle(i);
                    if (unit != null && _save.getSelectedUnit() == null)
                        _save.setSelectedUnit(unit);
                }
            }
            else if (_base != null)
            {
                // add vehicles that are in the base inventory
                foreach (var i in _base.getVehicles())
                {
                    BattleUnit unit = addXCOMVehicle(i);
                    if (unit != null && _save.getSelectedUnit() == null)
                        _save.setSelectedUnit(unit);
                }
                // we only add vehicles from the craft in new battle mode,
                // otherwise the base's vehicle vector will already contain these
                // due to the geoscape calling base.setupDefenses()
                if (_game.getSavedGame().getMonthsPassed() == -1)
                {
                    foreach (var i in _base.getCrafts())
                    {
                        foreach (var j in i.getVehicles())
                        {
                            BattleUnit unit = addXCOMVehicle(j);
                            if (unit != null && _save.getSelectedUnit() == null)
                                _save.setSelectedUnit(unit);
                        }
                    }
                }
            }
        }

        // add soldiers that are in the craft or base
        foreach (var i in _base.getSoldiers())
        {
            if ((_craft != null && i.getCraft() == _craft) ||
                (_craft == null && i.getWoundRecovery() == 0 && (i.getCraft() == null || i.getCraft().getStatus() != "STR_OUT")))
            {
                BattleUnit unit = addXCOMUnit(new BattleUnit(i, _save.getDepth()));
                if (unit != null && _save.getSelectedUnit() == null)
                    _save.setSelectedUnit(unit);
            }
        }

        if (!_save.getUnits().Any())
        {
            throw new Exception("Map generator encountered an error: no xcom units could be placed on the map.");
        }

        // maybe we should assign all units to the first tile of the skyranger before the inventory pre-equip and then reassign them to their correct tile afterwards?
        // fix: make them invisible, they are made visible afterwards.
        foreach (var i in _save.getUnits())
        {
            if (i.getFaction() == UnitFaction.FACTION_PLAYER)
            {
                _craftInventoryTile.setUnit(i);
                i.setVisible(false);
            }
        }

        if (_craft != null)
        {
            // add items that are in the craft
            foreach (var i in _craft.getItems().getContents())
            {
                for (int count = 0; count < i.Value; count++)
                {
                    _craftInventoryTile.addItem(new BattleItem(_game.getMod().getItem(i.Key, true), ref _save.getCurrentItemId()), ground);
                }
            }
        }
        else
        {
            // only use the items in the craft in new battle mode.
            if (_game.getSavedGame().getMonthsPassed() != -1)
            {
                // add items that are in the base
                var i = _base.getStorageItems().getContents().GetEnumerator();
                i.MoveNext();
                while (i.Current.Key != null)
                {
                    // only put items in the battlescape that make sense (when the item got a sprite, it's probably ok)
                    RuleItem rule = _game.getMod().getItem(i.Current.Key, true);
                    if (rule.canBeEquippedBeforeBaseDefense() && rule.getBigSprite() > -1 && rule.getBattleType() != BattleType.BT_NONE && rule.getBattleType() != BattleType.BT_CORPSE && !rule.isFixed() && _game.getSavedGame().isResearched(rule.getRequirements()))
                    {
                        for (int count = 0; count < i.Current.Value; count++)
                        {
                            _craftInventoryTile.addItem(new BattleItem(_game.getMod().getItem(i.Current.Key, true), ref _save.getCurrentItemId()), ground);
                        }
                        KeyValuePair<string, int> tmp = i.Current;
                        i.MoveNext();
                        _base.getStorageItems().removeItem(tmp.Key, tmp.Value);
                    }
                    else
                    {
                        i.MoveNext();
                    }
                }
            }
            // add items from crafts in base
            foreach (var c in _base.getCrafts())
            {
                if (c.getStatus() == "STR_OUT")
                    continue;
                foreach (var i in c.getItems().getContents())
                {
                    for (int count = 0; count < i.Value; count++)
                    {
                        _craftInventoryTile.addItem(new BattleItem(_game.getMod().getItem(i.Key, true), ref _save.getCurrentItemId()), ground);
                    }
                }
            }
        }

        // equip soldiers based on equipment-layout
        foreach (var i in _craftInventoryTile.getInventory())
        {
            // set all the items on this tile as belonging to the XCOM faction.
            i.setXCOMProperty(true);
            // don't let the soldiers take extra ammo yet
            if (i.getRules().getBattleType() == BattleType.BT_AMMO)
                continue;
            placeItemByLayout(i);
        }

        // load weapons before loadouts take extra clips.
        loadWeapons();

        foreach (var i in _craftInventoryTile.getInventory())
        {
            // we only need to distribute extra ammo at this point.
            if (i.getRules().getBattleType() != BattleType.BT_AMMO)
                continue;
            placeItemByLayout(i);
        }

        // auto-equip soldiers (only soldiers without layout) and clean up moved items
        autoEquip(_save.getUnits(), _game.getMod(), _save, _craftInventoryTile.getInventory(), ground, _worldShade, _allowAutoLoadout, false);
    }

    /**
     * Adds an XCom vehicle to the game.
     * Sets the correct turret depending on the ammo type.
     * @param v Pointer to the Vehicle.
     * @return Pointer to the spawned unit.
     */
    BattleUnit addXCOMVehicle(Vehicle v)
    {
        string vehicle = v.getRules().getType();
        Unit rule = _game.getMod().getUnit(vehicle, true);
        BattleUnit unit = addXCOMUnit(new BattleUnit(rule, UnitFaction.FACTION_PLAYER, _unitSequence++, _game.getMod().getArmor(rule.getArmor(), true), default, _save.getDepth()));
        if (unit != null)
        {
            BattleItem item = new BattleItem(_game.getMod().getItem(vehicle, true), ref _save.getCurrentItemId());
            if (!addItem(item, unit))
            {
                item = null;
            }
            if (v.getRules().getCompatibleAmmo().Any())
            {
                string ammo = v.getRules().getCompatibleAmmo().First();
                BattleItem ammoItem = new BattleItem(_game.getMod().getItem(ammo, true), ref _save.getCurrentItemId());
                addItem(ammoItem, unit);
                ammoItem.setAmmoQuantity(v.getAmmo());
            }
            unit.setTurretType(v.getRules().getTurretType());

            if (rule.getBuiltInWeapons().Any())
            {
                // not gonna randomize what weapon set tanks use, don't want to confuse players.
                foreach (var i in rule.getBuiltInWeapons().First())
                {
                    RuleItem ruleItem = _game.getMod().getItem(i);
                    if (ruleItem != null)
                    {
                        BattleItem weapon = new BattleItem(ruleItem, ref _save.getCurrentItemId());
                        if (!addItem(weapon, unit))
                        {
                            weapon = null;
                        }
                    }
                }
            }
        }
        return unit;
    }

    /**
     * Adds a soldier to the game and places him on a free spawnpoint.
     * Spawnpoints are either tiles in case of an XCom craft that landed.
     * Or they are mapnodes in case there's no craft.
     * @param soldier Pointer to the Soldier.
     * @return Pointer to the spawned unit.
     */
    BattleUnit addXCOMUnit(BattleUnit unit)
    {
        if (_baseInventory)
        {
            if (unit.hasInventory())
            {
                _save.getUnits().Add(unit);
                unit.setSpecialWeapon(_save, _game.getMod());
                return unit;
            }
        }
        else
        {
            if (_craft == null || !_craftDeployed)
            {
                Node node = _save.getSpawnNode((int)NodeRank.NR_XCOM, unit);
                if (node != null)
                {
                    _save.setUnitPosition(unit, node.getPosition());
                    _craftInventoryTile = _save.getTile(node.getPosition());
                    unit.setDirection(RNG.generate(0, 7));
                    _save.getUnits().Add(unit);
                    _save.getTileEngine().calculateFOV(unit);
                    unit.setSpecialWeapon(_save, _game.getMod());
                    return unit;
                }
                else if (_save.getMissionType() != "STR_BASE_DEFENSE")
                {
                    if (placeUnitNearFriend(unit))
                    {
                        _craftInventoryTile = _save.getTile(unit.getPosition());
                        unit.setDirection(RNG.generate(0, 7));
                        _save.getUnits().Add(unit);
                        _save.getTileEngine().calculateFOV(unit);
                        unit.setSpecialWeapon(_save, _game.getMod());
                        return unit;
                    }
                }
            }
            else if (_craft != null && _craft.getRules().getDeployment().Any())
            {
                foreach (var i in _craft.getRules().getDeployment())
                {
                    Position pos = new Position(i[0] + (_craftPos.x * 10), i[1] + (_craftPos.y * 10), i[2] + _craftZ);
                    int dir = i[3];
                    bool canPlace = true;
                    for (int x = 0; x < unit.getArmor().getSize(); ++x)
                    {
                        for (int y = 0; y < unit.getArmor().getSize(); ++y)
                        {
                            canPlace = (canPlace && canPlaceXCOMUnit(_save.getTile(pos + new Position(x, y, 0))));
                        }
                    }
                    if (canPlace)
                    {
                        if (_save.setUnitPosition(unit, pos))
                        {
                            _save.getUnits().Add(unit);
                            unit.setDirection(dir);
                            unit.setSpecialWeapon(_save, _game.getMod());
                            return unit;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < _mapsize_x * _mapsize_y * _mapsize_z; ++i)
                {
                    if (canPlaceXCOMUnit(_save.getTiles()[i]))
                    {
                        if (_save.setUnitPosition(unit, _save.getTiles()[i].getPosition()))
                        {
                            _save.getUnits().Add(unit);
                            unit.setSpecialWeapon(_save, _game.getMod());
                            return unit;
                        }
                    }
                }
            }
        }
        unit = null;
        return null;
    }

    /**
     * Checks if a soldier/tank can be placed on a given tile.
     * @param tile the given tile.
     * @return whether the unit can be placed here.
     */
    bool canPlaceXCOMUnit(Tile tile)
    {
        // to spawn an xcom soldier, there has to be a tile, with a floor, with the starting point attribute and no object in the way
        if (tile != null &&
            tile.getMapData(TilePart.O_FLOOR) != null &&
            tile.getMapData(TilePart.O_FLOOR).getSpecialType() == SpecialTileType.START_POINT &&
            tile.getMapData(TilePart.O_OBJECT) == null &&
            tile.getMapData(TilePart.O_FLOOR).getTUCost(MovementType.MT_WALK) < 255)
        {
            if (_craftInventoryTile == null)
                _craftInventoryTile = tile;

            return true;
        }
        return false;
    }

    /**
     * Places an item on an XCom soldier based on equipment layout.
     * @param item Pointer to the Item.
     * @return Pointer to the Item.
     */
    bool placeItemByLayout(BattleItem item)
    {
        RuleInventory ground = _game.getMod().getInventory("STR_GROUND", true);
        if (item.getSlot() == ground)
        {
            bool loaded;
            RuleInventory righthand = _game.getMod().getInventory("STR_RIGHT_HAND", true);

            // find the first soldier with a matching layout-slot
            foreach (var i in _save.getUnits())
            {
                // skip the vehicles, we need only X-Com soldiers WITH equipment-layout
                if (i.getArmor().getSize() > 1 || i.getGeoscapeSoldier() == null || !i.getGeoscapeSoldier().getEquipmentLayout().Any())
                {
                    continue;
                }

                // find the first matching layout-slot which is not already occupied
                List<EquipmentLayoutItem> layoutItems = i.getGeoscapeSoldier().getEquipmentLayout();
                foreach (var j in layoutItems)
                {
                    if (item.getRules().getType() != j.getItemType()
                    || i.getItem(j.getSlot(), j.getSlotX(), j.getSlotY()) != null) continue;

                    if (j.getAmmoItem() == "NONE")
                    {
                        loaded = true;
                    }
                    else
                    {
                        loaded = false;
                        // maybe we find the layout-ammo on the ground to load it with
                        var inventory = _craftInventoryTile.getInventory();
                        for (var k = 0; k < inventory.Count && (!loaded); ++k)
                        {
                            if (inventory[k].getRules().getType() == j.getAmmoItem() && inventory[k].getSlot() == ground
                            && item.setAmmoItem(inventory[k]) == 0)
                            {
                                _save.getItems().Add(inventory[k]);
                                inventory[k].setSlot(righthand);
                                loaded = true;
                                // note: soldier is not owner of the ammo, we are using this fact when saving equipments
                            }
                        }
                    }
                    // only place the weapon onto the soldier when it's loaded with its layout-ammo (if any)
                    if (loaded)
                    {
                        item.moveToOwner(i);
                        item.setSlot(_game.getMod().getInventory(j.getSlot(), true));
                        item.setSlotX(j.getSlotX());
                        item.setSlotY(j.getSlotY());
                        if (Options.includePrimeStateInSavedLayout &&
                            (item.getRules().getBattleType() == BattleType.BT_GRENADE ||
                            item.getRules().getBattleType() == BattleType.BT_PROXIMITYGRENADE))
                        {
                            item.setFuseTimer(j.getFuseTimer());
                        }
                        _save.getItems().Add(item);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /**
     * Loads all XCom weaponry before anything else is distributed.
     */
    void loadWeapons()
    {
        List<BattleItem> inventory;
        // let's try to load this weapon, whether we equip it or not.
        foreach (var i in _craftInventoryTile.getInventory())
        {
            if (!i.getRules().isFixed() &&
                i.getRules().getCompatibleAmmo().Any() &&
                i.getAmmoItem() == null &&
                (i.getRules().getBattleType() == BattleType.BT_FIREARM || i.getRules().getBattleType() == BattleType.BT_MELEE))
            {
                bool loaded = false;
                inventory = _craftInventoryTile.getInventory();
                for (var j = 0; j < inventory.Count && !loaded; ++j)
                {
                    if (inventory[j].getSlot() == _game.getMod().getInventory("STR_GROUND", true) && i.setAmmoItem(inventory[j]) == 0)
                    {
                        _save.getItems().Add(inventory[j]);
                        inventory[j].setSlot(_game.getMod().getInventory("STR_RIGHT_HAND", true));
                        loaded = true;
                    }
                }
            }
        }
        inventory = _craftInventoryTile.getInventory();
        for (var i = 0; i < inventory.Count;)
        {
            if (inventory[i].getSlot() != _game.getMod().getInventory("STR_GROUND", true))
            {
                _craftInventoryTile.getInventory().RemoveAt(i);
                continue;
            }
            ++i;
        }
    }

    void autoEquip(List<BattleUnit> units, Mod.Mod mod, SavedBattleGame addToSave, List<BattleItem> craftInv,
            RuleInventory groundRuleInv, int worldShade, bool allowAutoLoadout, bool overrideEquipmentLayout)
    {
        for (int pass = 0; pass < 4; ++pass)
        {
            for (var j = 0; j < craftInv.Count;)
            {
                if (craftInv[j].getSlot() == groundRuleInv)
                {
                    bool add = false;

                    switch (pass)
                    {
                        // priority 1: rifles.
                        case 0:
                            add = craftInv[j].getRules().isRifle();
                            break;
                        // priority 2: pistols (assuming no rifles were found).
                        case 1:
                            add = craftInv[j].getRules().isPistol();
                            break;
                        // priority 3: ammunition.
                        case 2:
                            add = craftInv[j].getRules().getBattleType() == BattleType.BT_AMMO;
                            break;
                        // priority 4: leftovers.
                        case 3:
                            add = !craftInv[j].getRules().isPistol() &&
                                    !craftInv[j].getRules().isRifle() &&
                                    (craftInv[j].getRules().getBattleType() != BattleType.BT_FLARE || worldShade > TileEngine.MAX_DARKNESS_TO_SEE_UNITS);
                            break;
                        default:
                            break;
                    }

                    if (add)
                    {
                        foreach (var i in units)
                        {
                            if (!i.hasInventory() || i.getGeoscapeSoldier() == null || (!overrideEquipmentLayout && i.getGeoscapeSoldier().getEquipmentLayout().Any()))
                            {
                                continue;
                            }
                            // let's not be greedy, we'll only take a second extra clip
                            // if everyone else has had a chance to take a first.
                            bool allowSecondClip = (pass == 3);
                            if (_addItem(craftInv[j], i, mod, addToSave, allowAutoLoadout, allowSecondClip))
                            {
                                craftInv.RemoveAt(j);
                                add = false;
                                break;
                            }
                        }
                        if (!add)
                        {
                            continue;
                        }
                    }
                }
                ++j;
            }
        }

        // clean up moved items
        for (var i = 0; i < craftInv.Count;)
        {
            if (craftInv[i].getSlot() != groundRuleInv)
            {
                craftInv.RemoveAt(i);
            }
            else
            {
                if (addToSave != null)
                {
                    addToSave.getItems().Add(craftInv[i]);
                }
                ++i;
            }
        }
    }

    /**
     * Deploys the aliens, according to the alien deployment rules.
     * @param race Pointer to the alien race.
     * @param deployment Pointer to the deployment rules.
     */
    void deployAliens(AlienDeployment deployment)
    {
        // race defined by deployment if there is one.
        if (!string.IsNullOrEmpty(deployment.getRace()) && _game.getSavedGame().getMonthsPassed() > -1)
        {
            _alienRace = deployment.getRace();
        }

        if (_save.getDepth() > 0 && !_alienRace.Contains("_UNDERWATER"))
        {
            _alienRace = _alienRace + "_UNDERWATER";
        }

        AlienRace race = _game.getMod().getAlienRace(_alienRace);
        if (race == null)
        {
            throw new Exception("Map generator encountered an error: Unknown race: " + _alienRace + " defined in deployment: " + deployment.getType());
        }

        int month;
        if (_game.getSavedGame().getMonthsPassed() != -1)
        {
            month =
            ((uint)_game.getSavedGame().getMonthsPassed()) > _game.getMod().getAlienItemLevels().Count - 1 ?  // if
            _game.getMod().getAlienItemLevels().Count - 1 : // then
            _game.getSavedGame().getMonthsPassed();  // else
        }
        else
        {
            month = _alienItemLevel;
        }
        foreach (var d in deployment.getDeploymentData())
        {
            string alienName = race.getMember(d.alienRank);

            int quantity;

            if (_game.getSavedGame().getDifficulty() < GameDifficulty.DIFF_VETERAN)
                quantity = d.lowQty + RNG.generate(0, d.dQty); // beginner/experienced
            else if (_game.getSavedGame().getDifficulty() < GameDifficulty.DIFF_SUPERHUMAN)
                quantity = d.lowQty + ((d.highQty - d.lowQty) / 2) + RNG.generate(0, d.dQty); // veteran/genius
            else
                quantity = d.highQty + RNG.generate(0, d.dQty); // super (and beyond?)

            quantity += RNG.generate(0, d.extraQty);

            for (int i = 0; i < quantity; ++i)
            {
                bool outside = RNG.generate(0, 99) < d.percentageOutsideUfo;
                if (_ufo == null)
                    outside = false;
                Unit rule = _game.getMod().getUnit(alienName, true);
                BattleUnit unit = addAlien(rule, d.alienRank, outside);
                int itemLevel = _game.getMod().getAlienItemLevels()[month][RNG.generate(0, 9)];
                if (unit != null)
                {
                    // Built in weapons: the unit has this weapon regardless of loadout or what have you.
                    if (rule.getBuiltInWeapons().Any())
                    {
                        if (itemLevel >= rule.getBuiltInWeapons().Count)
                        {
                            itemLevel = rule.getBuiltInWeapons().Count - 1;
                        }
                        foreach (var j in rule.getBuiltInWeapons()[itemLevel])
                        {
                            RuleItem ruleItem = _game.getMod().getItem(j);
                            if (ruleItem != null)
                            {
                                BattleItem item = new BattleItem(ruleItem, ref _save.getCurrentItemId());
                                if (!addItem(item, unit))
                                {
                                    item = null;
                                }
                            }
                        }
                    }

                    // terrorist alien's equipment is a special case - they are fitted with a weapon which is the alien's name with suffix _WEAPON
                    if (rule.isLivingWeapon())
                    {
                        string terroristWeapon = rule.getRace().Substring(4);
                        terroristWeapon += "_WEAPON";
                        RuleItem ruleItem = _game.getMod().getItem(terroristWeapon);
                        if (ruleItem != null)
                        {
                            BattleItem item = new BattleItem(ruleItem, ref _save.getCurrentItemId());
                            if (!addItem(item, unit))
                            {
                                item = null;
                            }
                            else
                            {
                                unit.setTurretType(item.getRules().getTurretType());
                            }
                        }
                    }
                    else
                    {
                        if (!d.itemSets.Any())
                        {
                            throw new Exception("Unit generator encountered an error: item set not defined");
                        }
                        if (itemLevel >= d.itemSets.Count)
                        {
                            itemLevel = d.itemSets.Count - 1;
                        }
                        foreach (var it in d.itemSets[itemLevel].items)
                        {
                            RuleItem ruleItem = _game.getMod().getItem(it);
                            if (ruleItem != null)
                            {
                                BattleItem item = new BattleItem(ruleItem, ref _save.getCurrentItemId());
                                if (!addItem(item, unit))
                                {
                                    item = null;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /**
     * Adds an alien to the game and places him on a free spawnpoint.
     * @param rules Pointer to the Unit which holds info about the alien .
     * @param alienRank The rank of the alien, used for spawn point search.
     * @param outside Whether the alien should spawn outside or inside the UFO.
     * @return Pointer to the created unit.
     */
    BattleUnit addAlien(Unit rules, int alienRank, bool outside)
    {
        BattleUnit unit = new BattleUnit(rules, UnitFaction.FACTION_HOSTILE, _unitSequence++, _game.getMod().getArmor(rules.getArmor(), true), _game.getMod().getStatAdjustment((int)_game.getSavedGame().getDifficulty()), _save.getDepth());
        Node node = null;

        // safety to avoid index out of bounds errors
        if (alienRank > 7)
            alienRank = 7;
        /* following data is the order in which certain alien ranks spawn on certain node ranks */
        /* note that they all can fall back to rank 0 nodes - which is scout (outside ufo) */

        for (int i = 0; i < 7 && node == null; ++i)
        {
            if (outside)
                node = _save.getSpawnNode(0, unit); // when alien is instructed to spawn outside, we only look for node 0 spawnpoints
            else
                node = _save.getSpawnNode(Node.nodeRank[alienRank, i], unit);
        }

        int difficulty = _game.getSavedGame().getDifficultyCoefficient();

        if (node != null && _save.setUnitPosition(unit, node.getPosition()))
        {
            unit.setAIModule(new AIModule(_game.getSavedGame().getSavedBattle(), unit, node));
            unit.setRankInt(alienRank);
            unit.setSpecialWeapon(_save, _game.getMod());
            int dir = _save.getTileEngine().faceWindow(node.getPosition());
            Position craft = _game.getSavedGame().getSavedBattle().getUnits()[0].getPosition();
            if (_save.getTileEngine().distance(node.getPosition(), craft) <= 20 && RNG.percent(20 * difficulty))
                dir = unit.directionTo(craft);
            if (dir != -1)
                unit.setDirection(dir);
            else
                unit.setDirection(RNG.generate(0, 7));

            // we only add a unit if it has a node to spawn on.
            // (stops them spawning at 0,0,0)
            _save.getUnits().Add(unit);
        }
        else
        {
            // DEMIGOD DIFFICULTY: screw the player: spawn as many aliens as possible.
            if (_game.getMod().isDemigod() && placeUnitNearFriend(unit))
            {
                unit.setAIModule(new AIModule(_game.getSavedGame().getSavedBattle(), unit, null));
                unit.setRankInt(alienRank);
                unit.setSpecialWeapon(_save, _game.getMod());
                int dir = _save.getTileEngine().faceWindow(unit.getPosition());
                Position craft = _game.getSavedGame().getSavedBattle().getUnits()[0].getPosition();
                if (_save.getTileEngine().distance(unit.getPosition(), craft) <= 20 && RNG.percent(20 * difficulty))
                    dir = unit.directionTo(craft);
                if (dir != -1)
                    unit.setDirection(dir);
                else
                    unit.setDirection(RNG.generate(0, 7));

                _save.getUnits().Add(unit);
            }
            else
            {
                unit = null;
            }
        }

        return unit;
    }

    /**
     * Generates a map (set of tiles) for a new battlescape game.
     * @param script the script to use to build the map.
     */
    void generateMap(List<MapScript> script)
    {
        // set our ambient sound
        _save.setAmbientSound(_terrain.getAmbience());
        _save.setAmbientVolume(_terrain.getAmbientVolume());

        // set up our map generation vars
        _dummy = new MapBlock("dummy");

        init(true);

        MapBlock craftMap = null;
        var ufoMaps = new List<MapBlock>();

        int mapDataSetIDOffset = 0;
        int craftDataSetIDOffset = 0;

        // create an array to track command success/failure
        var conditionals = new Dictionary<int, bool>();

        foreach (var i in _terrain.getMapDataSets())
        {
            i.loadData(_game.getMod().getMCDPatch(i.getName()));
            _save.getMapDataSets().Add(i);
            mapDataSetIDOffset++;
        }

        RuleTerrain ufoTerrain = null;
        // lets generate the map now and store it inside the tile objects

        // this mission type is "hard-coded" in terms of map layout
        if (_save.getMissionType() == "STR_BASE_DEFENSE")
        {
            generateBaseMap();
        }

        //process script
        foreach (var i in script)
        {
            MapScript command = i;

            if (command.getLabel() > 0 && conditionals.ContainsKey(command.getLabel()))
            {
                throw new Exception("Map generator encountered an error: multiple commands are sharing the same label.");
            }
            bool success = conditionals[command.getLabel()] = false;

            // if this command runs conditionally on the failures or successes of previous commands
            if (command.getConditionals().Any())
            {
                bool execute = true;
                // compare the corresponding entries in the success/failure vector
                foreach (var condition in command.getConditionals())
                {
                    // positive numbers indicate conditional on success, negative means conditional on failure
                    // ie: [1, -2] means this command only runs if command 1 succeeded and command 2 failed.
                    if (conditionals.ContainsKey(Math.Abs(condition)))
                    {
                        if ((condition > 0 && !conditionals[condition]) || (condition < 0 && conditionals[Math.Abs(condition)]))
                        {
                            execute = false;
                            break;
                        }
                    }
                    else
                    {
                        throw new Exception("Map generator encountered an error: conditional command expected a label that did not exist before this command.");
                    }
                }
                if (!execute)
                {
                    continue;
                }
            }

            // if there's a chance a command won't execute by design, take that into account here.
            if (RNG.percent(command.getChancesOfExecution()))
            {
                // initialize the block selection arrays
                command.init();

                // each command can be attempted multiple times, as randomization within the rects may occur
                for (int j = 0; j < command.getExecutions(); ++j)
                {
                    int x, y;
                    MapBlock block = null;
                    switch (command.getType())
                    {
                        case MapScriptCommand.MSC_ADDBLOCK:
                            block = command.getNextBlock(_terrain);
                            // select an X and Y position from within the rects, using an even distribution
                            if (block != null && selectPosition(command.getRects(), out x, out y, block.getSizeX(), block.getSizeY()))
                            {
                                success = addBlock(x, y, block) || success;
                            }
                            break;
                        case MapScriptCommand.MSC_ADDLINE:
                            success = addLine((command.getDirection()), command.getRects());
                            break;
                        case MapScriptCommand.MSC_ADDCRAFT:
                            if (_craft != null)
                            {
                                craftMap = _craft.getRules().getBattlescapeTerrainData().getRandomMapBlock(999, 999, 0, false);
                                if (addCraft(craftMap, command, out _craftPos))
                                {
                                    // by default addCraft adds blocks from group 1.
                                    // this can be overwritten in the command by defining specific groups or blocks
                                    // or this behaviour can be suppressed by leaving group 1 empty
                                    // this is intentional to allow for TFTD's cruise liners/etc
                                    // in this situation, you can end up with ANYTHING under your craft, so be careful
                                    for (x = _craftPos.x; x < _craftPos.x + _craftPos.w; ++x)
                                    {
                                        for (y = _craftPos.y; y < _craftPos.y + _craftPos.h; ++y)
                                        {
                                            if (_blocks[x][y] != null)
                                            {
                                                loadMAP(_blocks[x][y], x * 10, y * 10, _terrain, 0);
                                            }
                                        }
                                    }
                                    _craftDeployed = true;
                                    success = true;
                                }
                            }
                            break;
                        case MapScriptCommand.MSC_ADDUFO:
                            // as above, note that the craft and the ufo will never be allowed to overlap.
                            // significant difference here is that we accept a UFOName string here to choose the UFO map
                            // and we store the UFO positions in a vector, which we iterate later when actually loading the
                            // map and route data. this makes it possible to add multiple UFOs to a single map
                            // IMPORTANTLY: all the UFOs must use _exactly_ the same MCD set.
                            // this is fine for most UFOs but it does mean small scouts can't be combined with larger ones
                            // unless some major alterations are done to the MCD sets and maps themselves beforehand
                            // this is because serializing all the MCDs is an implementational nightmare from my perspective,
                            // and modders can take care of all that manually on their end.
                            if (_game.getMod().getUfo(command.getUFOName()) != null)
                            {
                                ufoTerrain = _game.getMod().getUfo(command.getUFOName()).getBattlescapeTerrainData();
                            }
                            else if (_ufo != null)
                            {
                                ufoTerrain = _ufo.getRules().getBattlescapeTerrainData();
                            }

                            if (ufoTerrain != null)
                            {
                                MapBlock ufoMap = ufoTerrain.getRandomMapBlock(999, 999, 0, false);
                                if (addCraft(ufoMap, command, out SDL_Rect ufoPosTemp))
                                {
                                    _ufoPos.Add(ufoPosTemp);
                                    ufoMaps.Add(ufoMap);
                                    for (x = ufoPosTemp.x; x < ufoPosTemp.x + ufoPosTemp.w; ++x)
                                    {
                                        for (y = ufoPosTemp.y; y < ufoPosTemp.y + ufoPosTemp.h; ++y)
                                        {
                                            if (_blocks[x][y] != null)
                                            {
                                                loadMAP(_blocks[x][y], x * 10, y * 10, _terrain, 0);
                                            }
                                        }
                                    }
                                    success = true;
                                }
                            }
                            break;
                        case MapScriptCommand.MSC_DIGTUNNEL:
                            drillModules(command.getTunnelData(), command.getRects(), command.getDirection());
                            success = true; // this command is fail-proof
                            break;
                        case MapScriptCommand.MSC_FILLAREA:
                            block = command.getNextBlock(_terrain);
                            while (block != null)
                            {
                                if (selectPosition(command.getRects(), out x, out y, block.getSizeX(), block.getSizeY()))
                                {
                                    // fill area will succeed if even one block is added
                                    success = addBlock(x, y, block) || success;
                                }
                                else
                                {
                                    break;
                                }
                                block = command.getNextBlock(_terrain);
                            }
                            break;
                        case MapScriptCommand.MSC_CHECKBLOCK:
                            var rects = command.getRects();
                            for (var k = 0; k < rects.Count && !success; ++k)
                            {
                                for (x = rects[k].x; x != rects[k].x + rects[k].w && x != _mapsize_x / 10 && !success; ++x)
                                {
                                    for (y = rects[k].y; y != rects[k].y + rects[k].h && y != _mapsize_y / 10 && !success; ++y)
                                    {
                                        var groups = command.getGroups();
                                        var blocks = command.getBlocks();
                                        if (groups.Any())
                                        {
                                            for (var z = 0; z < groups.Count && !success; ++z)
                                            {
                                                success = _blocks[x][y].isInGroup(groups[z]);
                                            }
                                        }
                                        else if (blocks.Any())
                                        {
                                            for (var z = 0; z < blocks.Count && !success; ++z)
                                            {
                                                if ((uint)blocks[z] < _terrain.getMapBlocks().Count)
                                                {
                                                    success = (_blocks[x][y] == _terrain.getMapBlocks()[blocks[z]]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // wildcard, we don't care what block it is, we just wanna know if there's a block here
                                            success = (_blocks[x][y] != null);
                                        }
                                    }
                                }
                            }
                            break;
                        case MapScriptCommand.MSC_REMOVE:
                            success = removeBlocks(command);
                            break;
                        case MapScriptCommand.MSC_RESIZE:
                            if (_save.getMissionType() == "STR_BASE_DEFENSE")
                            {
                                throw new Exception("Map Generator encountered an error: Base defense map cannot be resized.");
                            }
                            if (_blocksToDo < (_mapsize_x / 10) * (_mapsize_y / 10))
                            {
                                throw new Exception("Map Generator encountered an error: One does not simply resize the map after adding blocks.");
                            }

                            if (command.getSizeX() > 0 && command.getSizeX() != _mapsize_x / 10)
                            {
                                _mapsize_x = command.getSizeX() * 10;
                            }
                            if (command.getSizeY() > 0 && command.getSizeY() != _mapsize_y / 10)
                            {
                                _mapsize_y = command.getSizeY() * 10;
                            }
                            if (command.getSizeZ() > 0 && command.getSizeZ() != _mapsize_z)
                            {
                                _mapsize_z = command.getSizeZ();
                            }
                            init(false);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        if (_blocksToDo != 0)
        {
            throw new Exception("Map failed to fully generate.");
        }

        loadNodes();

        if (ufoMaps.Any() && ufoTerrain != null)
        {
            foreach (var i in ufoTerrain.getMapDataSets())
            {
                i.loadData(_game.getMod().getMCDPatch(i.getName()));
                _save.getMapDataSets().Add(i);
                craftDataSetIDOffset++;
            }

            for (int i = 0; i < ufoMaps.Count; ++i)
            {
                loadMAP(ufoMaps[i], _ufoPos[i].x * 10, _ufoPos[i].y * 10, ufoTerrain, mapDataSetIDOffset);
                loadRMP(ufoMaps[i], _ufoPos[i].x * 10, _ufoPos[i].y * 10, Node.UFOSEGMENT);
                for (int j = 0; j < ufoMaps[i].getSizeX() / 10; ++j)
                {
                    for (int k = 0; k < ufoMaps[i].getSizeY() / 10; k++)
                    {
                        _segments[_ufoPos[i].x + j][_ufoPos[i].y + k] = Node.UFOSEGMENT;
                    }
                }
            }
        }

        if (craftMap != null)
        {
            foreach (var i in _craft.getRules().getBattlescapeTerrainData().getMapDataSets())
            {
                i.loadData(_game.getMod().getMCDPatch(i.getName()));
                _save.getMapDataSets().Add(i);
            }
            loadMAP(craftMap, _craftPos.x * 10, _craftPos.y * 10, _craft.getRules().getBattlescapeTerrainData(), mapDataSetIDOffset + craftDataSetIDOffset, true, true);
            loadRMP(craftMap, _craftPos.x * 10, _craftPos.y * 10, Node.CRAFTSEGMENT);
            for (int i = 0; i < craftMap.getSizeX() / 10; ++i)
            {
                for (int j = 0; j < craftMap.getSizeY() / 10; j++)
                {
                    _segments[_craftPos.x + i][_craftPos.y + j] = Node.CRAFTSEGMENT;
                }
            }
            for (int i = (_craftPos.x * 10) - 1; i <= (_craftPos.x * 10) + craftMap.getSizeX(); ++i)
            {
                for (int j = (_craftPos.y * 10) - 1; j <= (_craftPos.y * 10) + craftMap.getSizeY(); j++)
                {
                    for (int k = _mapsize_z - 1; k >= _craftZ; --k)
                    {
                        if (_save.getTile(new Position(i, j, k)) != null)
                        {
                            _save.getTile(new Position(i, j, k)).setDiscovered(true, 2);
                        }
                    }
                }
            }
        }

        _dummy = null;

        // special hacks to fill in empty floors on level 0
        for (int x = 0; x < _mapsize_x; ++x)
        {
            for (int y = 0; y < _mapsize_y; ++y)
            {
                if (_save.getTile(new Position(x, y, 0)).getMapData(TilePart.O_FLOOR) == null)
                {
                    _save.getTile(new Position(x, y, 0)).setMapData(MapDataSet.getScorchedEarthTile(), 1, 0, TilePart.O_FLOOR);
                }
            }
        }

        attachNodeLinks();
    }

    /**
     * Adds a craft (or UFO) to the map, and tries to add a landing zone type block underneath it.
     * @param craftMap the map for the craft in question.
     * @param command the script command to pull info from.
     * @param craftPos the position of the craft is stored here.
     * @return if the craft was placed or not.
     */
    bool addCraft(MapBlock craftMap, MapScript command, out SDL_Rect craftPos)
    {
        craftPos = new SDL_Rect
        {
            w = craftMap.getSizeX(),
            h = craftMap.getSizeY()
        };
        bool placed = false;

        placed = selectPosition(command.getRects(), out int x, out int y, craftPos.w, craftPos.h);
        // if ok, allocate it
        if (placed)
        {
            craftPos.x = x;
            craftPos.y = y;
            craftPos.w /= 10;
            craftPos.h /= 10;
            for (x = 0; x < craftPos.w; ++x)
            {
                for (y = 0; y < craftPos.h; ++y)
                {
                    _landingzone[craftPos.x + x][craftPos.y + y] = true;
                    MapBlock block = command.getNextBlock(_terrain);
                    if (block != null && _blocks[craftPos.x + x][craftPos.y + y] == null)
                    {
                        _blocks[craftPos.x + x][craftPos.y + y] = block;
                        _blocksToDo--;
                    }
                }
            }
        }

        return placed;
    }

    /**
     * Adds a single block to the map.
     * @param x the x position to add the block
     * @param y the y position to add the block
     * @param block the block to add.
     * @return if the block was added or not.
     */
    bool addBlock(int x, int y, MapBlock block)
    {
        int xSize = (block.getSizeX() - 1) / 10;
        int ySize = (block.getSizeY() - 1) / 10;

        for (int xd = 0; xd <= xSize; ++xd)
        {
            for (int yd = 0; yd != ySize; ++yd)
            {
                if (_blocks[x + xd][y + yd] != null)
                    return false;
            }
        }

        for (int xd = 0; xd <= xSize; ++xd)
        {
            for (int yd = 0; yd <= ySize; ++yd)
            {
                _blocks[x + xd][y + yd] = _dummy;
                _blocksToDo--;
            }
        }

        // mark the south edge of the block for drilling
        for (int xd = 0; xd <= xSize; ++xd)
        {
            _drillMap[x + xd][y + ySize] = (int)MapDirection.MD_VERTICAL;
        }
        // then the east edge
        for (int yd = 0; yd <= ySize; ++yd)
        {
            _drillMap[x + xSize][y + yd] = (int)MapDirection.MD_HORIZONTAL;
        }
        // then the far corner gets marked for both
        // this also marks 1x1 modules
        _drillMap[x + xSize][y + ySize] = (int)MapDirection.MD_BOTH;

        _blocks[x][y] = block;
        bool visible = (_save.getMissionType() == "STR_BASE_DEFENSE"); // yes, i'm hard coding these, big whoop, wanna fight about it?

        loadMAP(_blocks[x][y], x * 10, y * 10, _terrain, 0, visible);
        return true;
    }

    /**
     * Selects a position for a map block.
     * @param rects the positions to select from, none meaning the whole map.
     * @param X the x position for the block gets stored in this variable.
     * @param Y the y position for the block gets stored in this variable.
     * @param sizeX the x size of the block we want to add.
     * @param sizeY the y size of the block we want to add.
     * @return if a valid position was selected or not.
     */
    bool selectPosition(List<SDL_Rect> rects, out int X, out int Y, int sizeX, int sizeY)
    {
        var available = new List<SDL_Rect>();
        var valid = new List<KeyValuePair<int, int>>();
        SDL_Rect wholeMap;
        wholeMap.x = 0;
        wholeMap.y = 0;
        wholeMap.w = (_mapsize_x / 10);
        wholeMap.h = (_mapsize_y / 10);
        sizeX = (sizeX / 10);
        sizeY = (sizeY / 10);
        if (!rects.Any())
        {
            available.Add(wholeMap);
        }
        else
        {
            available = rects;
        }
        foreach (var i in available)
        {
            if (sizeX > i.w || sizeY > i.h)
            {
                continue;
            }
            for (int x = i.x; x + sizeX <= i.x + i.w && x + sizeX <= wholeMap.w; ++x)
            {
                for (int y = i.y; y + sizeY <= i.y + i.h && y + sizeY <= wholeMap.h; ++y)
                {
                    if (!valid.Contains(KeyValuePair.Create(x, y)))
                    {
                        bool add = true;
                        for (int xCheck = x; xCheck != x + sizeX; ++xCheck)
                        {
                            for (int yCheck = y; yCheck != y + sizeY; ++yCheck)
                            {
                                if (_blocks[xCheck][yCheck] != null)
                                {
                                    add = false;
                                }
                            }
                        }
                        if (add)
                        {
                            valid.Add(KeyValuePair.Create(x, y));
                        }
                    }
                }
            }
        }
        if (!valid.Any())
        {
            X = 0;
            Y = 0;
            return false;
        }
        KeyValuePair<int, int> selection = valid[RNG.generate(0, valid.Count - 1)];
        X = selection.Key;
        Y = selection.Value;
        return true;
    }

    /**
     * Loads an XCom format MAP file into the tiles of the battlegame.
     * @param mapblock Pointer to MapBlock.
     * @param xoff Mapblock offset in X direction.
     * @param yoff Mapblock offset in Y direction.
     * @param save Pointer to the current SavedBattleGame.
     * @param terrain Pointer to the Terrain rule.
     * @param discovered Whether or not this mapblock is discovered (eg. landingsite of the XCom plane).
     * @return int Height of the loaded mapblock (this is needed for spawpoint calculation...)
     * @sa http://www.ufopaedia.org/index.php?title=MAPS
     * @note Y-axis is in reverse order.
     */
    int loadMAP(MapBlock mapblock, int xoff, int yoff, RuleTerrain terrain, int mapDataSetOffset, bool discovered = false, bool craft = false)
    {
        int sizex, sizey, sizez;
        int x = xoff, y = yoff, z = 0;
        //sbyte[] size = new sbyte[3];
        byte[] value = new byte[4];
        string filename = $"MAPS/{mapblock.getName()}.MAP";
        uint terrainObjectID;

        try
        {
            // Load file
            using var mapFile = new BinaryReader(new FileStream(FileMap.getFilePath(filename), FileMode.Open));

            sizey = (int)mapFile.ReadSByte();
            sizex = (int)mapFile.ReadSByte();
            sizez = (int)mapFile.ReadSByte();

            mapblock.setSizeZ(sizez);

            string ss;
            if (sizez > _save.getMapSizeZ())
            {
                ss = $"Height of map {filename} too big for this mission, block is {sizez}, expected: {_save.getMapSizeZ()}";
                throw new Exception(ss);
            }

            if (sizex != mapblock.getSizeX() ||
                sizey != mapblock.getSizeY())
            {
                ss = $"Map block is not of the size specified {filename} is {sizex}x{sizey}, expected: {mapblock.getSizeX()}x{mapblock.getSizeY()}";
                throw new Exception(ss);
            }

            z += sizez - 1;

            for (int i = _mapsize_z - 1; i > 0; i--)
            {
                // check if there is already a layer - if so, we have to move Z up
                MapData floor = _save.getTile(new Position(x, y, i)).getMapData(TilePart.O_FLOOR);
                if (floor != null)
                {
                    z += i;
                    if (craft)
                    {
                        _craftZ = i;
                    }
                    break;
                }
            }

            if (z > (_save.getMapSizeZ() - 1))
            {
                if (_save.getMissionType() == "STR_BASE_DEFENSE")
                {
                    // we'll already have gone through _base.isOverlappingOrOverflowing() by the time we hit this, possibly multiple times
                    // let's just throw an exception and tell them to check the log, it'll have all the detail they'll need.
                    throw new Exception("Something is wrong with your base, check your log file for additional information.");
                }
                throw new Exception("Something is wrong in your map definitions, craft/ufo map is too tall?");
            }

            while (mapFile.Read(value, 0, value.Length) != 0)
            {
                for (int part = (int)TilePart.O_FLOOR; part <= (int)TilePart.O_OBJECT; ++part)
                {
                    terrainObjectID = ((byte)value[part]);
                    if (terrainObjectID > 0)
                    {
                        int mapDataSetID = mapDataSetOffset;
                        uint mapDataID = terrainObjectID;
                        MapData md = terrain.getMapData(ref mapDataID, ref mapDataSetID);
                        if (mapDataSetOffset > 0) // ie: ufo or craft.
                        {
                            _save.getTile(new Position(x, y, z)).setMapData(null, -1, -1, TilePart.O_OBJECT);
                        }
                        TilePart tp = (TilePart)part;
                        _save.getTile(new Position(x, y, z)).setMapData(md, (int)mapDataID, mapDataSetID, tp);
                    }
                }

                _save.getTile(new Position(x, y, z)).setDiscovered((discovered || mapblock.isFloorRevealed(z)), 2);

                x++;

                if (x == (sizex + xoff))
                {
                    x = xoff;
                    y++;
                }
                if (y == (sizey + yoff))
                {
                    y = yoff;
                    z--;
                }
            }

            if (mapFile.BaseStream.Position != mapFile.BaseStream.Length - 1)
            {
                throw new Exception("Invalid MAP file: " + filename);
            }

            mapFile.Close();

            if (_generateFuel)
            {
                // if one of the mapBlocks has an items array defined, don't deploy fuel algorithmically
                _generateFuel = !mapblock.getItems().Any();
            }
            foreach (var i in mapblock.getItems())
            {
                RuleItem rule = _game.getMod().getItem(i.Key, true);
                foreach (var j in i.Value)
                {
                    BattleItem item = new BattleItem(rule, ref _save.getCurrentItemId());
                    _save.getItems().Add(item);
                    _save.getTile(j + new Position(xoff, yoff, 0)).addItem(item, _game.getMod().getInventory("STR_GROUND", true));
                }
            }
            return sizez;
        }
        catch (Exception)
        {
            throw new Exception(filename + " not found");
        }
    }

    /**
     * Sets up all our various arrays and whatnot according to the size of the map.
     */
    void init(bool resetTerrain)
    {
        _blocks.Clear();
        _landingzone.Clear();
        _segments.Clear();
        _drillMap.Clear();

        for (var i = 0; i < _mapsize_x / 10; i++)
        {
            var t1 = new List<MapBlock>();
            for (var j = 0; j < _mapsize_y / 10; j++) t1.Add(new MapBlock());
            _blocks.Add(t1);
        }
        for (var i = 0; i < _mapsize_x / 10; i++)
        {
            var t2 = new List<bool>();
            for (var j = 0; j < _mapsize_y / 10; j++) t2[j] = false;
            _landingzone.Add(t2);
        }
        for (var i = 0; i < _mapsize_x / 10; i++)
        {
            var t3 = new List<int>();
            for (var j = 0; j < _mapsize_y / 10; j++) t3[j] = 0;
            _segments.Add(t3);
        }
        for (var i = 0; i < _mapsize_x / 10; i++)
        {
            var t4 = new List<int>();
            for (var j = 0; j < _mapsize_y / 10; j++) t4[j] = (int)MapDirection.MD_NONE;
            _drillMap.Add(t4);
        }

        _blocksToDo = (_mapsize_x / 10) * (_mapsize_y / 10);
        // creates the tile objects
        _save.initMap(_mapsize_x, _mapsize_y, _mapsize_z, resetTerrain);
        _save.initUtilities(_mod);
    }

    /**
     * Loads an XCom format RMP file into the spawnpoints of the battlegame.
     * @param mapblock Pointer to MapBlock.
     * @param xoff Mapblock offset in X direction.
     * @param yoff Mapblock offset in Y direction.
     * @param segment Mapblock segment.
     * @sa http://www.ufopaedia.org/index.php?title=ROUTES
     */
    void loadRMP(MapBlock mapblock, int xoff, int yoff, int segment)
    {
        byte[] value = new byte[24];
        string filename = $"ROUTES/{mapblock.getName()}.RMP";

        try
        {
            // Load file
            using var mapFile = new FileStream(FileMap.getFilePath(filename), FileMode.Open);

            int nodeOffset = _save.getNodes().Count;
            var badNodes = new List<int>();
            int nodesAdded = 0;
            while (mapFile.Read(value, 0, value.Length) != 0)
            {
                int pos_x = value[1];
                int pos_y = value[0];
                int pos_z = value[2];
                Node node;
                if (pos_x >= 0 && pos_x < mapblock.getSizeX() &&
                    pos_y >= 0 && pos_y < mapblock.getSizeY() &&
                    pos_z >= 0 && pos_z < mapblock.getSizeZ())
                {
                    Position pos = new Position(xoff + pos_x, yoff + pos_y, mapblock.getSizeZ() - 1 - pos_z);
                    int type = value[19];
                    int rank = value[20];
                    int flags = value[21];
                    int reserved = value[22];
                    int priority = value[23];
                    node = new Node(_save.getNodes().Count, pos, segment, type, rank, flags, reserved, priority);
                    for (int j = 0; j < 5; ++j)
                    {
                        int connectID = value[4 + j * 3];
                        // don't touch special values
                        if (connectID <= 250)
                        {
                            connectID += nodeOffset;
                        }
                        // 255/-1 = unused, 254/-2 = north, 253/-3 = east, 252/-4 = south, 251/-5 = west
                        else
                        {
                            connectID -= 256;
                        }
                        node.getNodeLinks().Add(connectID);
                    }
                }
                else
                {
                    // since we use getNodes().at(n) a lot, we have to push a dummy node into the vector to keep the connections sane,
                    // or else convert the node vector to a map, either way is as much work, so i'm sticking with vector for faster operation.
                    // this is because the "built in" nodeLinks reference each other by number, and it's gonna be implementational hell to try
                    // to adjust those numbers retroactively, post-culling. far better to simply mark these culled nodes as dummies, and discount their use
                    // that way, all the connections will still line up properly in the array.
                    node = new Node();
                    node.setDummy(true);
                    Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Bad node in RMP file: {filename} Node #{nodesAdded} is outside map boundaries at X:{pos_x} Y:{pos_y} Z:{pos_z}. Culling Node.");
                    badNodes.Add(nodesAdded);
                }
                _save.getNodes().Add(node);
                nodesAdded++;
            }

            foreach (var i in badNodes)
            {
                int nodeCounter = nodesAdded;
                var nodes = _save.getNodes();
                for (var j = nodes.Count - 1; j >= 0 && nodeCounter > 0; --j)
                {
                    if (!nodes[j].isDummy())
                    {
                        var nodeLinks = nodes[j].getNodeLinks();
                        for (var k = 0; k < nodeLinks.Count; k++)
                        {
                            if (nodeLinks[k] - nodeOffset == (uint)i)
                            {
                                Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} RMP file: {filename} Node #{nodeCounter - 1} is linked to Node #{i}, which was culled. Terminating Link.");
                                nodeLinks[k] = -1;
                            }
                        }
                    }
                    nodeCounter--;
                }
            }

            if (mapFile.Position != mapFile.Length - 1)
            {
                throw new Exception("Invalid RMP file: " + filename);
            }

            mapFile.Close();
        }
        catch (Exception)
        {
            throw new Exception(filename + " not found");
        }
    }

    /**
     * Attaches all the nodes together in an intricate web of lies.
     */
    void attachNodeLinks()
    {
        foreach (var i in _save.getNodes())
        {
            if (i.isDummy())
            {
                continue;
            }
            Node node = i;
            int segmentX = node.getPosition().x / 10;
            int segmentY = node.getPosition().y / 10;
            int[] neighbourSegments = new int[4];
            int[] neighbourDirections = { -2, -3, -4, -5 };
            int[] neighbourDirectionsInverted = { -4, -5, -2, -3 };

            if (segmentX == (_mapsize_x / 10) - 1)
                neighbourSegments[0] = -1;
            else
                neighbourSegments[0] = _segments[segmentX + 1][segmentY];
            if (segmentY == (_mapsize_y / 10) - 1)
                neighbourSegments[1] = -1;
            else
                neighbourSegments[1] = _segments[segmentX][segmentY + 1];
            if (segmentX == 0)
                neighbourSegments[2] = -1;
            else
                neighbourSegments[2] = _segments[segmentX - 1][segmentY];
            if (segmentY == 0)
                neighbourSegments[3] = -1;
            else
                neighbourSegments[3] = _segments[segmentX][segmentY - 1];

            var nodeLinks = node.getNodeLinks();
            for (var j = 0; j < nodeLinks.Count; j++)
            {
                for (int n = 0; n < 4; n++)
                {
                    if (nodeLinks[j] == neighbourDirections[n])
                    {
                        foreach (var k in _save.getNodes())
                        {
                            if (k.isDummy())
                            {
                                continue;
                            }
                            if (k.getSegment() == neighbourSegments[n])
                            {
                                var nodeLinks2 = k.getNodeLinks();
                                for (var l = 0; l < nodeLinks2.Count; l++)
                                {
                                    if (nodeLinks2[l] == neighbourDirectionsInverted[n])
                                    {
                                        nodeLinks2[l] = node.getID();
                                        nodeLinks[j] = k.getID();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /**
     * draws a line along the map either horizontally, vertically or both.
     * @param direction the direction to draw the line
     * @param rects the positions to allow the line to be drawn in.
     * @return if the blocks were added or not.
     */
    bool addLine(MapDirection direction, List<SDL_Rect> rects)
    {
	    if (direction == MapDirection.MD_BOTH)
	    {
		    if (addLine(MapDirection.MD_VERTICAL, rects))
		    {
			    addLine(MapDirection.MD_HORIZONTAL, rects);
			    return true;
		    }
		    return false;
	    }

	    int tries = 0;
	    bool placed = false;

	    int roadX = 0, roadY = 0;
	    int iteratorValue = roadX;
	    MapBlockType comparator = MapBlockType.MT_NSROAD;
	    MapBlockType typeToAdd = MapBlockType.MT_EWROAD;
	    int limit = _mapsize_x / 10;
	    if (direction == MapDirection.MD_VERTICAL)
	    {
		    iteratorValue = roadY;
		    comparator = MapBlockType.MT_EWROAD;
		    typeToAdd = MapBlockType.MT_NSROAD;
		    limit = _mapsize_y / 10;
	    }
	    while (!placed)
	    {
		    selectPosition(rects, out roadX, out roadY, 10, 10);
		    placed = true;
		    for (iteratorValue = 0; iteratorValue < limit; iteratorValue += 1)
		    {
			    if (_blocks[roadX][roadY] != null && _blocks[roadX][roadY].isInGroup((int)comparator) == false)
			    {
				    placed = false;
				    break;
			    }
		    }
		    if (tries++ > 20)
		    {
			    return false;
		    }
	    }
	    iteratorValue = 0;
	    while (iteratorValue < limit)
	    {
		    if (_blocks[roadX][roadY] == null)
		    {
			    addBlock(roadX, roadY, _terrain.getRandomMapBlock(10, 10, (int)typeToAdd));
		    }
		    else if (_blocks[roadX][roadY].isInGroup((int)comparator))
		    {
			    _blocks[roadX][roadY] = _terrain.getRandomMapBlock(10, 10, (int)MapBlockType.MT_CROSSING);
			    clearModule(roadX * 10, roadY * 10, 10, 10);
			    loadMAP(_blocks[roadX][roadY], roadX * 10, roadY * 10, _terrain, 0);
		    }
		    iteratorValue += 1;
	    }
	    return true;
    }

    /**
     * Clears a module from the map.
     * @param x the x offset.
     * @param y the y offset.
     * @param sizeX how far along the x axis to clear.
     * @param sizeY how far along the y axis to clear.
     */
    void clearModule(int x, int y, int sizeX, int sizeY)
    {
        for (int z = 0; z != _mapsize_z; ++z)
        {
            for (int dx = x; dx != x + sizeX; ++dx)
            {
                for (int dy = y; dy != y + sizeY; ++dy)
                {
                    Tile tile = _save.getTile(new Position(dx, dy, z));
                    for (var i = TilePart.O_FLOOR; i <= TilePart.O_OBJECT; i++)
                        tile.setMapData(null, -1, -1, i);
                }
            }
        }
    }

    /**
     * Drills a tunnel between existing map modules.
     * note that this drills all modules currently on the map,
     * so it should take place BEFORE the dirt is added in base defenses.
     * @param data the wall replacements and level to dig on.
     * @param rects the length/width of the tunnels themselves.
     * @param dir the direction to drill.
     */
    void drillModules(TunnelData data, List<SDL_Rect> rects, MapDirection dir)
    {
	    MCDReplacement wWall = data.getMCDReplacement("westWall");
	    MCDReplacement nWall = data.getMCDReplacement("northWall");
	    MCDReplacement corner = data.getMCDReplacement("corner");
	    MCDReplacement floor = data.getMCDReplacement("floor");
	    SDL_Rect rect;
	    rect.x = rect.y = rect.w = rect.h = 3;
	    if (rects.Any())
	    {
		    rect = rects.First();
	    }

	    for (int i = 0; i < (_mapsize_x / 10); ++i)
	    {
		    for (int j = 0; j < (_mapsize_y / 10); ++j)
		    {
			    if (_blocks[i][j] == null)
				    continue;

			    MapData md;

			    if (dir != MapDirection.MD_VERTICAL)
			    {
				    // drill east
				    if (i < (_mapsize_x / 10)-1 && (_drillMap[i][j] == (int)MapDirection.MD_HORIZONTAL || _drillMap[i][j] == (int)MapDirection.MD_BOTH) && _blocks[i+1][j] != null)
				    {
					    Tile tile;
					    // remove stuff
					    for (int k = rect.y; k != rect.y + rect.h; ++k)
					    {
						    tile = _save.getTile(new Position((i*10)+9, (j*10)+k, data.level));
						    if (tile != null)
						    {
							    tile.setMapData(null, -1, -1, TilePart.O_WESTWALL);
							    tile.setMapData(null, -1, -1, TilePart.O_OBJECT);
							    if (floor != default)
							    {
								    md = _terrain.getMapDataSets()[floor.set].getObject((uint)floor.entry);
								    tile.setMapData(md, floor.entry, floor.set, TilePart.O_FLOOR);
							    }

							    tile = _save.getTile(new Position((i+1)*10, (j*10)+k, data.level));
							    tile.setMapData(null, -1, -1, TilePart.O_WESTWALL);
							    MapData obj = tile.getMapData(TilePart.O_OBJECT);
							    if (obj != null && obj.getTUCost(MovementType.MT_WALK) == 0)
							    {
								    tile.setMapData(null, -1, -1, TilePart.O_OBJECT);
							    }
						    }
					    }

					    if (nWall != default)
					    {
						    md = _terrain.getMapDataSets()[nWall.set].getObject((uint)nWall.entry);
						    tile = _save.getTile(new Position((i*10)+9, (j*10)+rect.y, data.level));
						    tile.setMapData(md, nWall.entry, nWall.set, TilePart.O_NORTHWALL);
						    tile = _save.getTile(new Position((i*10)+9, (j*10)+rect.y+rect.h, data.level));
						    tile.setMapData(md, nWall.entry, nWall.set, TilePart.O_NORTHWALL);
					    }

					    if (corner != default)
					    {
						    md = _terrain.getMapDataSets()[corner.set].getObject((uint)corner.entry);
						    tile = _save.getTile(new Position((i+1)*10, (j*10)+rect.y, data.level));
						    if (tile.getMapData(TilePart.O_NORTHWALL) == null)
							    tile.setMapData(md, corner.entry, corner.set, TilePart.O_NORTHWALL);
					    }
				    }
			    }

			    if (dir != MapDirection.MD_HORIZONTAL)
			    {
				    // drill south
				    if (j < (_mapsize_y / 10)-1 && (_drillMap[i][j] == (int)MapDirection.MD_VERTICAL || _drillMap[i][j] == (int)MapDirection.MD_BOTH) && _blocks[i][j+1] != null)
				    {
					    // remove stuff
					    for (int k = rect.x; k != rect.x + rect.w; ++k)
					    {
						    Tile tile = _save.getTile(new Position((i*10)+k, (j*10)+9, data.level));
						    if (tile != null)
						    {
							    tile.setMapData(null, -1, -1, TilePart.O_NORTHWALL);
							    tile.setMapData(null, -1, -1, TilePart.O_OBJECT);
							    if (floor != default)
							    {
								    md = _terrain.getMapDataSets()[floor.set].getObject((uint)floor.entry);
								    tile.setMapData(md, floor.entry, floor.set, TilePart.O_FLOOR);
							    }

							    tile = _save.getTile(new Position((i*10)+k, (j+1)*10, data.level));
							    tile.setMapData(null, -1, -1, TilePart.O_NORTHWALL);
							    MapData obj = tile.getMapData(TilePart.O_OBJECT);
							    if (obj != null && obj.getTUCost(MovementType.MT_WALK) == 0)
							    {
								    tile.setMapData(null, -1, -1, TilePart.O_OBJECT);
							    }
						    }
					    }

					    if (wWall != default)
					    {
						    md = _terrain.getMapDataSets()[wWall.set].getObject((uint)wWall.entry);
						    Tile tile = _save.getTile(new Position((i*10)+rect.x, (j*10)+9, data.level));
						    tile.setMapData(md, wWall.entry, wWall.set, TilePart.O_WESTWALL);
						    tile = _save.getTile(new Position((i*10)+rect.x+rect.w, (j*10)+9, data.level));
						    tile.setMapData(md, wWall.entry, wWall.set, TilePart.O_WESTWALL);
					    }

					    if (corner != default)
					    {
						    md = _terrain.getMapDataSets()[corner.set].getObject((uint)corner.entry);
						    Tile tile = _save.getTile(new Position((i*10)+rect.x, (j+1)*10, data.level));
						    if (tile.getMapData(TilePart.O_WESTWALL) == null)
							    tile.setMapData(md, corner.entry, corner.set, TilePart.O_WESTWALL);
					    }
				    }
			    }
		    }
	    }
    }

    /**
     * Removes all blocks within a given set of rects, as defined in the command.
     * @param command contains all the info we need.
     * @return success of the removal.
     * @feel shame for having written this.
     */
    bool removeBlocks(MapScript command)
    {
        var deleted = new List<KeyValuePair<int, int>>();
        bool success = false;

        foreach (var k in command.getRects())
        {
            for (int x = k.x; x != k.x + k.w && x != _mapsize_x / 10; ++x)
            {
                for (int y = k.y; y != k.y + k.h && y != _mapsize_y / 10; ++y)
                {
                    if (_blocks[x][y] != null && _blocks[x][y] != _dummy)
                    {
                        var pos = KeyValuePair.Create(x, y);
                        if (command.getGroups().Any())
                        {
                            foreach (var z in command.getGroups())
                            {
                                if (_blocks[x][y].isInGroup(z))
                                {
                                    // the deleted vector should only contain unique entries
                                    if (!deleted.Contains(pos))
                                    {
                                        deleted.Add(pos);
                                    }
                                }
                            }
                        }
                        else if (command.getBlocks().Any())
                        {
                            foreach (var z in command.getBlocks())
                            {
                                if ((uint)z < _terrain.getMapBlocks().Count)
                                {
                                    // the deleted vector should only contain unique entries
                                    if (!deleted.Contains(pos))
                                    {
                                        deleted.Add(pos);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // the deleted vector should only contain unique entries
                            if (!deleted.Contains(pos))
                            {
                                deleted.Add(pos);
                            }
                        }
                    }
                }
            }
        }
        foreach (var z in deleted)
        {
            int x = z.Key;
            int y = z.Value;
            clearModule(x * 10, y * 10, _blocks[x][y].getSizeX(), _blocks[x][y].getSizeY());

            int delx = (_blocks[x][y].getSizeX() / 10);
            int dely = (_blocks[x][y].getSizeY() / 10);

            for (int dx = x; dx != x + delx; ++dx)
            {
                for (int dy = y; dy != y + dely; ++dy)
                {
                    _blocks[dx][dy] = null;
                    _blocksToDo++;
                }
            }
            // this command succeeds if even one block is removed.
            success = true;
        }
        return success;
    }

    /**
     * Generates a map based on the base's layout.
     * this doesn't drill or fill with dirt, the script must do that.
     */
    void generateBaseMap()
    {
        // add modules based on the base's layout
        foreach (var i in _base.getFacilities())
        {
            if (i.getBuildTime() == 0)
            {
                int num = 0;
                int xLimit = i.getX() + i.getRules().getSize() - 1;
                int yLimit = i.getY() + i.getRules().getSize() - 1;

                for (int y = i.getY(); y <= yLimit; ++y)
                {
                    for (int x = i.getX(); x <= xLimit; ++x)
                    {
                        // lots of crazy stuff here, which is for the hangars (or other large base facilities one may create)
                        // TODO: clean this mess up, make the mapNames a vector in the base module defs
                        // also figure out how to do the terrain sets on a per-block basis.
                        string mapname = i.getRules().getMapName();
                        string newname = mapname.Substring(0, mapname.Length - 2); // strip of last 2 digits
                        int mapnum = int.Parse(mapname.Substring(mapname.Length - 2, 2)); // get number
                        mapnum += num;
                        if (mapnum < 10) newname = $"{newname}0";
                        newname = $"{newname}{mapnum}";
                        addBlock(x, y, _terrain.getMapBlock(newname));
                        _drillMap[x][y] = (int)MapDirection.MD_NONE;
                        num++;
                        if (i.getRules().getStorage() > 0)
                        {
                            int groundLevel;
                            for (groundLevel = _mapsize_z - 1; groundLevel >= 0; --groundLevel)
                            {
                                if (!_save.getTile(new Position(x * 10, y * 10, groundLevel)).hasNoFloor(null))
                                    break;
                            }
                            // general stores - there is where the items are put
                            for (int k = x * 10; k != (x + 1) * 10; ++k)
                            {
                                for (int l = y * 10; l != (y + 1) * 10; ++l)
                                {
                                    // we only want every other tile, giving us a "checkerboard" pattern
                                    if ((k + l) % 2 == 0)
                                    {
                                        Tile t = _save.getTile(new Position(k, l, groundLevel));
                                        Tile tEast = _save.getTile(new Position(k + 1, l, groundLevel));
                                        Tile tSouth = _save.getTile(new Position(k, l + 1, groundLevel));
                                        if (t != null && t.getMapData(TilePart.O_FLOOR) != null && t.getMapData(TilePart.O_OBJECT) == null &&
                                            tEast != null && tEast.getMapData(TilePart.O_WESTWALL) == null &&
                                            tSouth != null && tSouth.getMapData(TilePart.O_NORTHWALL) == null)
                                        {
                                            _save.getStorageSpace().Add(new Position(k, l, groundLevel));
                                        }
                                    }
                                }
                            }
                            // let's put the inventory tile on the lower floor, just to be safe.
                            if (_craftInventoryTile == null)
                            {
                                _craftInventoryTile = _save.getTile(new Position((x * 10) + 5, (y * 10) + 5, groundLevel - 1));
                            }
                        }
                    }
                }
                for (int x = i.getX(); x <= xLimit; ++x)
                {
                    _drillMap[x][yLimit] = (int)MapDirection.MD_VERTICAL;
                }
                for (int y = i.getY(); y <= yLimit; ++y)
                {
                    _drillMap[xLimit][y] = (int)MapDirection.MD_HORIZONTAL;
                }
                _drillMap[xLimit][yLimit] = (int)MapDirection.MD_BOTH;
            }
        }
        _save.calculateModuleMap();
    }

    /**
     * Loads all the nodes from the map modules.
     */
    void loadNodes()
    {
        int segment = 0;
        for (int itY = 0; itY < (_mapsize_y / 10); itY++)
        {
            for (int itX = 0; itX < (_mapsize_x / 10); itX++)
            {
                _segments[itX][itY] = segment;
                if (_blocks[itX][itY] != null && _blocks[itX][itY] != _dummy)
                {
                    if (!(_blocks[itX][itY].isInGroup((int)MapBlockType.MT_LANDINGZONE) && _landingzone[itX][itY]))
                    {
                        loadRMP(_blocks[itX][itY], itX * 10, itY * 10, segment++);
                    }
                }
            }
        }
    }

    /**
     * Switches an existing battlescapesavegame to a new stage.
     */
    internal void nextStage()
    {
        RuleInventory ground = _game.getMod().getInventory("STR_GROUND", true);

        // preventively drop all units from soldier's inventory (makes handling easier)
        // 1. no alien/civilian living, dead or unconscious is allowed to transition
        // 2. no dead xcom unit is allowed to transition
        // 3. only living or unconscious xcom units can transition
        foreach (var unit in _save.getUnits())
        {
            if (unit.getOriginalFaction() == UnitFaction.FACTION_PLAYER && !unit.isOut())
            {
                var unitsToDrop = new List<BattleItem>();
                foreach (var item in unit.getInventory())
                {
                    if (item.getUnit() != null)
                    {
                        unitsToDrop.Add(item);
                    }
                }
                foreach (var corpseItem in unitsToDrop)
                {
                    corpseItem.moveToOwner(null);
                    unit.getTile().addItem(corpseItem, ground);
                    if (corpseItem.getUnit() != null && corpseItem.getUnit().getStatus() == UnitStatus.STATUS_UNCONSCIOUS)
                    {
                        corpseItem.getUnit().setPosition(unit.getTile().getPosition());
                    }
                }
            }
        }

        int aliensAlive = 0;
        // send all enemy units, or those not in endpoint area (if aborted) to time out
        foreach (var i in _save.getUnits())
        {
            i.clearVisibleUnits();
            i.clearVisibleTiles();

            Tile tmpTile = _save.getTile(i.getPosition());
            bool isInExit = i.isInExitArea(SpecialTileType.END_POINT) || i.liesInExitArea(tmpTile, SpecialTileType.END_POINT);

            if (i.getStatus() != UnitStatus.STATUS_DEAD                          // if they're not dead
                && ((i.getOriginalFaction() == UnitFaction.FACTION_PLAYER        // and they're a soldier
                && _save.isAborted()                                             // and you aborted
                && !isInExit)                                                    // and they're not on the exit
                || i.getOriginalFaction() != UnitFaction.FACTION_PLAYER))        // or they're not a soldier
            {
                if (i.getOriginalFaction() == UnitFaction.FACTION_HOSTILE && !i.isOut())
                {
                    if (i.getOriginalFaction() == i.getFaction())
                    {
                        aliensAlive++;
                    }
                    else if (i.getTile() != null)
                    {
                        var inventory = i.getInventory();
                        for (var j = 0; j < inventory.Count;)
                        {
                            if (!inventory[j].getRules().isFixed())
                            {
                                i.getTile().addItem(inventory[j], _game.getMod().getInventory("STR_GROUND", true));
                                inventory.RemoveAt(j);
                            }
                            else
                            {
                                ++j;
                            }
                        }
                    }
                }
                i.goToTimeOut();
                if (i.getAIModule() != null)
                {
                    i.setAIModule(null);
                }
            }
            if (i.getTile() != null)
            {
                Position pos = i.getPosition();
                int size = i.getArmor().getSize();
                for (int x = 0; x != size; ++x)
                {
                    for (int y = 0; y != size; ++y)
                    {
                        _save.getTile(pos + new Position(x, y, 0)).setUnit(null);
                    }
                }
            }
            i.setFire(0);
            i.setTile(null);
            i.setPosition(new Position(-1, -1, -1), false);
        }

        // remove all items not belonging to our soldiers from the map.
        // sort items into two categories:
        // the ones that we are guaranteed to be able to take home, barring complete failure (ie: stuff on the ship)
        // and the ones that are scattered about on the ground, that will be recovered ONLY on success.
        // this does not include items in your soldier's hands.
        List<BattleItem> takeHomeGuaranteed = _save.getGuaranteedRecoveredItems();
        List<BattleItem> takeHomeConditional = _save.getConditionalRecoveredItems();
        var takeToNextStage = new List<BattleItem>();
        var carryToNextStage = new List<BattleItem>();
        var removeFromGame = new List<BattleItem>();

        _save.resetTurnCounter();

        foreach (var i in _save.getItems())
        {
            // first off: don't process ammo loaded into weapons. at least not at this level. ammo will be handled simultaneously.
            if (!i.isAmmo())
            {
                List<BattleItem> toContainer = removeFromGame;
                // if it's recoverable, and it's not owned by someone
                if (((i.getUnit() != null && i.getUnit().getGeoscapeSoldier() != null) || i.getRules().isRecoverable()) && i.getOwner() == null)
                {
                    // first off: don't count primed grenades on the floor
                    if (i.getFuseTimer() == -1)
                    {
                        // protocol 1: all defenders dead, recover all items.
                        if (aliensAlive == 0)
                        {
                            // any corpses or unconscious units get put in the skyranger, as well as any unresearched items
                            if ((i.getUnit() != null &&
                                (i.getUnit().getOriginalFaction() != UnitFaction.FACTION_PLAYER ||
                                i.getUnit().getStatus() == UnitStatus.STATUS_DEAD))
                                || !_game.getSavedGame().isResearched(i.getRules().getRequirements()))
                            {
                                toContainer = takeHomeGuaranteed;
                            }
                            // otherwise it comes with us to stage two
                            else
                            {
                                toContainer = takeToNextStage;
                            }
                        }
                        // protocol 2: some of the aliens survived, meaning we ran to the exit zone.
                        // recover stuff depending on where it was at the end of the mission.
                        else
                        {
                            Tile tile = i.getTile();
                            if (tile != null)
                            {
                                // on a tile at least, so i'll give you the benefit of the doubt on this and give it a conditional recovery at this point
                                toContainer = takeHomeConditional;
                                if (tile.getMapData(TilePart.O_FLOOR) != null)
                                {
                                    // in the skyranger? it goes home.
                                    if (tile.getMapData(TilePart.O_FLOOR).getSpecialType() == SpecialTileType.START_POINT)
                                    {
                                        toContainer = takeHomeGuaranteed;
                                    }
                                    // on the exit grid? it goes to stage two.
                                    else if (tile.getMapData(TilePart.O_FLOOR).getSpecialType() == SpecialTileType.END_POINT)
                                    {
                                        // apply similar logic (for units) as in protocol 1
                                        if (i.getUnit() != null &&
                                            (i.getUnit().getOriginalFaction() != UnitFaction.FACTION_PLAYER ||
                                            i.getUnit().getStatus() == UnitStatus.STATUS_DEAD))
                                        {
                                            toContainer = takeHomeConditional;
                                        }
                                        else
                                        {
                                            toContainer = takeToNextStage;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // if a soldier is already holding it, let's let him keep it
                if (i.getOwner() != null && i.getOwner().getFaction() == UnitFaction.FACTION_PLAYER)
                {
                    toContainer = carryToNextStage;
                }

                // at this point, we know what happens with the item, so let's apply it to any ammo as well.
                BattleItem ammo = i.getAmmoItem();
                if (ammo != null && ammo != i)
                {
                    // break any tile links, because all the tiles are about to disappear.
                    ammo.setTile(null);
                    toContainer.Add(ammo);
                }
                // and now the actual item itself.
                i.setTile(null);
                toContainer.Add(i);
            }
        }

        // anything in the "removeFromGame" vector will now be discarded - they're all dead to us now.
        for (var i = 0; i < removeFromGame.Count; i++)
        {
            // fixed weapons, or anything that's otherwise "equipped" will need to be de-equipped
            // from their owners to make sure we don't have any null pointers to worry about later
            if (removeFromGame[i].getOwner() != null)
            {
                var inventory = removeFromGame[i].getOwner().getInventory();
                foreach (var j in inventory)
                {
                    if (removeFromGame[i] == j)
                    {
                        inventory.Remove(j);
                        break;
                    }
                }
            }
            removeFromGame[i] = null;
        }

        // empty the items vector
        _save.getItems().Clear();

        // rebuild it with only the items we want to keep active in battle for the next stage
        // here we add all the items that our soldiers are carrying, and we'll add the items on the
        // inventory tile after we've generated our map. everything else will either be in one of the
        // recovery arrays, or deleted from existence at this point.
        foreach (var i in carryToNextStage)
        {
            _save.getItems().Add(i);
        }

        AlienDeployment ruleDeploy = _game.getMod().getDeployment(_save.getMissionType(), true);
        _save.setTurnLimit(ruleDeploy.getTurnLimit());
        _save.setChronoTrigger(ruleDeploy.getChronoTrigger());
        _save.setCheatTurn(ruleDeploy.getCheatTurn());
        ruleDeploy.getDimensions(out _mapsize_x, out _mapsize_y, out _mapsize_z);
        int pick = RNG.generate(0, ruleDeploy.getTerrains().Count - 1);
        _terrain = _game.getMod().getTerrain(ruleDeploy.getTerrains()[pick], true);
        setDepth(ruleDeploy, true);
        _worldShade = ruleDeploy.getShade();

        List<MapScript> script = _game.getMod().getMapScript(_terrain.getScript());
        if (_game.getMod().getMapScript(ruleDeploy.getScript()) != null)
        {
            script = _game.getMod().getMapScript(ruleDeploy.getScript());
        }
        else if (!string.IsNullOrEmpty(ruleDeploy.getScript()))
        {
            throw new Exception("Map generator encountered an error: " + ruleDeploy.getScript() + " script not found.");
        }
        if (script == null)
        {
            throw new Exception("Map generator encountered an error: " + _terrain.getScript() + " script not found.");
        }

        generateMap(script);

        setupObjectives(ruleDeploy);

        int highestSoldierID = 0;
        bool selectedFirstSoldier = false;
        foreach (var j in _save.getUnits())
        {
            if (j.getOriginalFaction() == UnitFaction.FACTION_PLAYER)
            {
                if (!j.isOut())
                {
                    j.setTurnsSinceSpotted(255);
                    j.getVisibleTiles().Clear();
                    j.setCache(null);
                    if (!selectedFirstSoldier && j.getGeoscapeSoldier() != null)
                    {
                        _save.setSelectedUnit(j);
                        selectedFirstSoldier = true;
                    }
                    Node node = _save.getSpawnNode((int)NodeRank.NR_XCOM, j);
                    if (node != null || placeUnitNearFriend(j))
                    {
                        if (node != null)
                        {
                            _save.setUnitPosition(j, node.getPosition());
                        }
                        if (_craftInventoryTile == null)
                        {
                            _craftInventoryTile = j.getTile();
                        }
                        _craftInventoryTile.setUnit(j);
                        j.setVisible(false);
                        if (j.getId() > highestSoldierID)
                        {
                            highestSoldierID = j.getId();
                        }
                        //reset TUs, regain energy, etc. but don't take damage or go berserk
                        j.prepareNewTurn(false);
                    }
                }
            }
        }

        if (_save.getSelectedUnit() == null || _save.getSelectedUnit().isOut() || _save.getSelectedUnit().getFaction() != UnitFaction.FACTION_PLAYER)
        {
            _save.selectNextPlayerUnit();
        }

        foreach (var i in takeToNextStage)
        {
            _save.getItems().Add(i);
            if (!i.isAmmo())
            {
                _craftInventoryTile.addItem(i, ground);
                if (i.getUnit() != null)
                {
                    _craftInventoryTile.setUnit(i.getUnit());
                    i.getUnit().setPosition(_craftInventoryTile.getPosition());
                }
            }
        }

        _unitSequence = _save.getUnits().Last().getId() + 1;

        int unitCount = _save.getUnits().Count;

        // Let's figure out what race we're up against.
        _alienRace = ruleDeploy.getRace();

        var missionSites = _game.getSavedGame().getMissionSites();
        for (var i = 0; i < missionSites.Count && string.IsNullOrEmpty(_alienRace); ++i)
        {
            if (missionSites[i].isInBattlescape())
            {
                _alienRace = missionSites[i].getAlienRace();
            }
        }

        var alienBases = _game.getSavedGame().getAlienBases();
        for (var i = 0; i < alienBases.Count && string.IsNullOrEmpty(_alienRace); ++i)
        {
            if (alienBases[i].isInBattlescape())
            {
                _alienRace = alienBases[i].getAlienRace();
            }
        }

        deployAliens(ruleDeploy);

        if (unitCount == _save.getUnits().Count)
        {
            throw new Exception("Map generator encountered an error: no alien units could be placed on the map.");
        }

        deployCivilians(ruleDeploy.getCivilians());

        _save.setAborted(false);
        setMusic(ruleDeploy, true);
        _save.setGlobalShade(_worldShade);
        _save.getTileEngine().calculateSunShading();
        _save.getTileEngine().calculateTerrainLighting();
        _save.getTileEngine().calculateUnitLighting();
    }

    /**
     * Sets the world texture where a ufo crashed. This is used to determine the terrain.
     * @param texture Texture id of the polygon on the globe.
     */
    internal void setWorldTexture(Texture texture) =>
        _worldTexture = texture;

    /**
     * Sets the world shade where a ufo crashed. This is used to determine the battlescape light level.
     * @param shade Shade of the polygon on the globe.
     */
    internal void setWorldShade(int shade)
    {
        if (shade > 15) shade = 15;
        if (shade < 0) shade = 0;
        _worldShade = shade;
    }

    /**
     * Sets the XCom craft involved in the battle.
     * @param craft Pointer to XCom craft.
     */
    internal void setCraft(Craft craft)
    {
        _craft = craft;
        _craft.setInBattlescape(true);
    }

    /**
     * Sets the ufo involved in the battle.
     * @param ufo Pointer to UFO.
     */
    internal void setUfo(Ufo ufo)
    {
        _ufo = ufo;
        _ufo.setInBattlescape(true);
    }

    /**
     * Sets the mission site involved in the battle.
     * @param mission Pointer to mission site.
     */
    internal void setMissionSite(MissionSite mission)
    {
        _mission = mission;
        _mission.setInBattlescape(true);
    }

    /**
     * Sets the alien base involved in the battle.
     * @param base Pointer to alien base.
     */
    internal void setAlienBase(AlienBase @base)
    {
        _alienBase = @base;
        _alienBase.setInBattlescape(true);
    }
}
