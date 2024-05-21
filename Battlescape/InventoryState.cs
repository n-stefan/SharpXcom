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
 * Screen which displays soldier's inventory.
 */
internal class InventoryState : State
{
    const int _templateBtnX = 288;
    const int _createTemplateBtnY = 90;
    const int _applyTemplateBtnY = 113;

    bool _tu;
    BattlescapeState _parent;
    SavedBattleGame _battleGame;
    Surface _bg, _soldier;
    Text _txtName, _txtItem, _txtAmmo, _txtWeight, _txtTus, _txtFAcc, _txtReact, _txtPSkill, _txtPStr;
    BattlescapeButton _btnOk, _btnPrev, _btnNext, _btnUnload, _btnGround, _btnRank;
    BattlescapeButton _btnCreateTemplate, _btnApplyTemplate;
    Surface _selAmmo;
    Inventory _inv;
    string _currentTooltip;
    List<EquipmentLayoutItem> _curInventoryTemplate;

    /**
     * Initializes all the elements in the Inventory screen.
     * @param game Pointer to the core game.
     * @param tu Does Inventory use up Time Units?
     * @param parent Pointer to parent Battlescape.
     */
    internal InventoryState(bool tu, BattlescapeState parent)
    {
        _tu = tu;
        _parent = parent;

        _battleGame = _game.getSavedGame().getSavedBattle();

        if (Options.maximizeInfoScreens)
        {
            Options.baseXResolution = Screen.ORIGINAL_WIDTH;
            Options.baseYResolution = Screen.ORIGINAL_HEIGHT;
            _game.getScreen().resetDisplay(false);
        }
        else if (_battleGame.getTileEngine() == null)
        {
            Screen.updateScale(Options.battlescapeScale, ref Options.baseXBattlescape, ref Options.baseYBattlescape, true);
            _game.getScreen().resetDisplay(false);
        }

        // Create objects
        _bg = new Surface(320, 200, 0, 0);
        _soldier = new Surface(320, 200, 0, 0);
        _txtName = new Text(210, 17, 28, 6);
        _txtTus = new Text(40, 9, 245, 24);
        _txtWeight = new Text(70, 9, 245, 24);
        _txtFAcc = new Text(50, 9, 245, 32);
        _txtReact = new Text(50, 9, 245, 40);
        _txtPSkill = new Text(50, 9, 245, 48);
        _txtPStr = new Text(50, 9, 245, 56);
        _txtItem = new Text(160, 9, 128, 140);
        _txtAmmo = new Text(66, 24, 254, 64);
        _btnOk = new BattlescapeButton(35, 22, 237, 1);
        _btnPrev = new BattlescapeButton(23, 22, 273, 1);
        _btnNext = new BattlescapeButton(23, 22, 297, 1);
        _btnUnload = new BattlescapeButton(32, 25, 288, 32);
        _btnGround = new BattlescapeButton(32, 15, 289, 137);
        _btnRank = new BattlescapeButton(26, 23, 0, 0);
        _btnCreateTemplate = new BattlescapeButton(32, 22, _templateBtnX, _createTemplateBtnY);
        _btnApplyTemplate = new BattlescapeButton(32, 22, _templateBtnX, _applyTemplateBtnY);
        _selAmmo = new Surface(RuleInventory.HAND_W * RuleInventory.SLOT_W, RuleInventory.HAND_H * RuleInventory.SLOT_H, 272, 88);
        _inv = new Inventory(_game, 320, 200, 0, 0, _parent == null);

        // Set palette
        setPalette("PAL_BATTLESCAPE");

        add(_bg);

        // Set up objects
        _game.getMod().getSurface("TAC01.SCR").blit(_bg);

        add(_soldier);
        add(_txtName, "textName", "inventory", _bg);
        add(_txtTus, "textTUs", "inventory", _bg);
        add(_txtWeight, "textWeight", "inventory", _bg);
        add(_txtFAcc, "textFiring", "inventory", _bg);
        add(_txtReact, "textReaction", "inventory", _bg);
        add(_txtPSkill, "textPsiSkill", "inventory", _bg);
        add(_txtPStr, "textPsiStrength", "inventory", _bg);
        add(_txtItem, "textItem", "inventory", _bg);
        add(_txtAmmo, "textAmmo", "inventory", _bg);
        add(_btnOk, "buttonOK", "inventory", _bg);
        add(_btnPrev, "buttonPrev", "inventory", _bg);
        add(_btnNext, "buttonNext", "inventory", _bg);
        add(_btnUnload, "buttonUnload", "inventory", _bg);
        add(_btnGround, "buttonGround", "inventory", _bg);
        add(_btnRank, "rank", "inventory", _bg);
        add(_btnCreateTemplate, "buttonCreate", "inventory", _bg);
        add(_btnApplyTemplate, "buttonApply", "inventory", _bg);
        add(_selAmmo);
        add(_inv);

        // move the TU display down to make room for the weight display
        if (Options.showMoreStatsInInventoryView)
        {
            _txtTus.setY(_txtTus.getY() + 8);
        }

        centerAllSurfaces();

        _txtName.setBig();
        _txtName.setHighContrast(true);

        _txtTus.setHighContrast(true);

        _txtWeight.setHighContrast(true);

        _txtFAcc.setHighContrast(true);

        _txtReact.setHighContrast(true);

        _txtPSkill.setHighContrast(true);

        _txtPStr.setHighContrast(true);

        _txtItem.setHighContrast(true);

        _txtAmmo.setAlign(TextHAlign.ALIGN_CENTER);
        _txtAmmo.setHighContrast(true);

        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyBattleInventory);
        _btnOk.setTooltip("STR_OK");
        _btnOk.onMouseIn(txtTooltipIn);
        _btnOk.onMouseOut(txtTooltipOut);

        _btnPrev.onMouseClick(btnPrevClick);
        _btnPrev.onKeyboardPress(btnPrevClick, Options.keyBattlePrevUnit);
        _btnPrev.setTooltip("STR_PREVIOUS_UNIT");
        _btnPrev.onMouseIn(txtTooltipIn);
        _btnPrev.onMouseOut(txtTooltipOut);

        _btnNext.onMouseClick(btnNextClick);
        _btnNext.onKeyboardPress(btnNextClick, Options.keyBattleNextUnit);
        _btnNext.setTooltip("STR_NEXT_UNIT");
        _btnNext.onMouseIn(txtTooltipIn);
        _btnNext.onMouseOut(txtTooltipOut);

        _btnUnload.onMouseClick(btnUnloadClick);
        _btnUnload.setTooltip("STR_UNLOAD_WEAPON");
        _btnUnload.onMouseIn(txtTooltipIn);
        _btnUnload.onMouseOut(txtTooltipOut);

        _btnGround.onMouseClick(btnGroundClick);
        _btnGround.setTooltip("STR_SCROLL_RIGHT");
        _btnGround.onMouseIn(txtTooltipIn);
        _btnGround.onMouseOut(txtTooltipOut);

        _btnRank.onMouseClick(btnRankClick);
        _btnRank.setTooltip("STR_UNIT_STATS");
        _btnRank.onMouseIn(txtTooltipIn);
        _btnRank.onMouseOut(txtTooltipOut);

        _btnCreateTemplate.onMouseClick(btnCreateTemplateClick);
        _btnCreateTemplate.onKeyboardPress(btnCreateTemplateClick, Options.keyInvCreateTemplate);
        _btnCreateTemplate.setTooltip("STR_CREATE_INVENTORY_TEMPLATE");
        _btnCreateTemplate.onMouseIn(txtTooltipIn);
        _btnCreateTemplate.onMouseOut(txtTooltipOut);

        _btnApplyTemplate.onMouseClick(btnApplyTemplateClick);
        _btnApplyTemplate.onKeyboardPress(btnApplyTemplateClick, Options.keyInvApplyTemplate);
        _btnApplyTemplate.onKeyboardPress(onClearInventory, Options.keyInvClear);
        _btnApplyTemplate.onKeyboardPress(onAutoequip, Options.keyInvAutoEquip);
        _btnApplyTemplate.setTooltip("STR_APPLY_INVENTORY_TEMPLATE");
        _btnApplyTemplate.onMouseIn(txtTooltipIn);
        _btnApplyTemplate.onMouseOut(txtTooltipOut);

        // only use copy/paste buttons in setup (i.e. non-tu) mode
        if (_tu)
        {
            _btnCreateTemplate.setVisible(false);
            _btnApplyTemplate.setVisible(false);
        }
        else
        {
            _updateTemplateButtons(true);
        }

        _inv.draw();
        _inv.setTuMode(_tu);
        _inv.setSelectedUnit(_game.getSavedGame().getSavedBattle().getSelectedUnit());
        _inv.onMouseClick(invClick, 0);
        _inv.onMouseOver(invMouseOver);
        _inv.onMouseOut(invMouseOut);

        _txtTus.setVisible(_tu);
        _txtWeight.setVisible(Options.showMoreStatsInInventoryView);
        _txtFAcc.setVisible(Options.showMoreStatsInInventoryView && !_tu);
        _txtReact.setVisible(Options.showMoreStatsInInventoryView && !_tu);
        _txtPSkill.setVisible(Options.showMoreStatsInInventoryView && !_tu);
        _txtPStr.setVisible(Options.showMoreStatsInInventoryView && !_tu);
    }

    /**
     *
     */
    ~InventoryState()
    {
        _clearInventoryTemplate(_curInventoryTemplate);

        if (_battleGame.getTileEngine() != null)
        {
            if (Options.maximizeInfoScreens)
            {
                Screen.updateScale(Options.battlescapeScale, ref Options.baseXBattlescape, ref Options.baseYBattlescape, true);
                _game.getScreen().resetDisplay(false);
            }
            Tile inventoryTile = _battleGame.getSelectedUnit().getTile();
            _battleGame.getTileEngine().applyGravity(inventoryTile);
            _battleGame.getTileEngine().calculateTerrainLighting(); // dropping/picking up flares
            _battleGame.getTileEngine().recalculateFOV();
        }
        else
        {
            Screen.updateScale(Options.geoscapeScale, ref Options.baseXGeoscape, ref Options.baseYGeoscape, true);
            _game.getScreen().resetDisplay(false);
        }
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        if (_inv.getSelectedItem() != null)
            return;
        _game.popState();
        Tile inventoryTile = _battleGame.getSelectedUnit().getTile();
        if (!_tu)
        {
            saveEquipmentLayout();
            _battleGame.resetUnitTiles();
            if (_battleGame.getTurn() == 1)
            {
                _battleGame.randomizeItemLocations(inventoryTile);
                if (inventoryTile.getUnit() != null)
                {
                    // make sure we select the unit closest to the ramp.
                    _battleGame.setSelectedUnit(inventoryTile.getUnit());
                }
            }

            // initialize xcom units for battle
            foreach (var j in _battleGame.getUnits())
            {
                if (j.getOriginalFaction() != UnitFaction.FACTION_PLAYER || j.isOut())
                {
                    continue;
                }

                j.prepareNewTurn(false);
            }
        }
    }

    /**
     * Saves the soldiers' equipment-layout.
     */
    void saveEquipmentLayout()
    {
        foreach (var i in _battleGame.getUnits())
        {
            // we need X-Com soldiers only
            if (i.getGeoscapeSoldier() == null) continue;

            List<EquipmentLayoutItem> layoutItems = i.getGeoscapeSoldier().getEquipmentLayout();

            // clear the previous save
            if (layoutItems.Any())
            {
                layoutItems.Clear();
            }

            // save the soldier's items
            // note: with using getInventory() we are skipping the ammos loaded, (they're not owned) because we handle the loaded-ammos separately (inside)
            foreach (var j in i.getInventory())
            {
                string ammo;
                if (j.needsAmmo() && null != j.getAmmoItem()) ammo = j.getAmmoItem().getRules().getType();
                else ammo = "NONE";
                layoutItems.Add(new EquipmentLayoutItem(
                    j.getRules().getType(),
                    j.getSlot().getId(),
                    j.getSlotX(),
                    j.getSlotY(),
                    ammo,
                    j.getFuseTimer()
                ));
            }
        }
    }

    /**
     * Shows a tooltip for the appropriate button.
     * @param action Pointer to an action.
     */
    void txtTooltipIn(Action action)
    {
        if (_inv.getSelectedItem() == null && Options.battleTooltips)
        {
            _currentTooltip = action.getSender().getTooltip();
            _txtItem.setText(tr(_currentTooltip));
        }
    }

    /**
     * Clears the tooltip text.
     * @param action Pointer to an action.
     */
    void txtTooltipOut(Action action)
    {
        if (_inv.getSelectedItem() == null && Options.battleTooltips)
        {
            if (_currentTooltip == action.getSender().getTooltip())
            {
                _currentTooltip = string.Empty;
                _txtItem.setText(string.Empty);
            }
        }
    }

    /**
     * Selects the previous soldier.
     * @param action Pointer to an action.
     */
    void btnPrevClick(Action _)
    {
        if (_inv.getSelectedItem() != null)
        {
            return;
        }

        if (_parent != null)
        {
            _parent.selectPreviousPlayerUnit(false, false, true);
        }
        else
        {
            _battleGame.selectPreviousPlayerUnit(false, false, true);
        }
        init();
    }

    /**
     * Selects the next soldier.
     * @param action Pointer to an action.
     */
    void btnNextClick(Action _)
    {
        if (_inv.getSelectedItem() != null)
        {
            return;
        }
        if (_parent != null)
        {
            _parent.selectNextPlayerUnit(false, false, true);
        }
        else
        {
            _battleGame.selectNextPlayerUnit(false, false, true);
        }
        init();
    }

    /**
     * Unloads the selected weapon.
     * @param action Pointer to an action.
     */
    void btnUnloadClick(Action _)
    {
        if (_inv.unload())
        {
            _txtItem.setText(string.Empty);
            _txtAmmo.setText(string.Empty);
            _selAmmo.clear();
            updateStats();
            _game.getMod().getSoundByDepth(0, (uint)Mod.Mod.ITEM_DROP).play();
        }
    }

    /**
     * Shows more ground items / rearranges them.
     * @param action Pointer to an action.
     */
    void btnGroundClick(Action _) =>
        _inv.arrangeGround();

    /**
     * Shows the unit info screen.
     * @param action Pointer to an action.
     */
    void btnRankClick(Action _) =>
        _game.pushState(new UnitInfoState(_battleGame.getSelectedUnit(), _parent, true, false));

    void btnCreateTemplateClick(Action _)
    {
        // don't accept clicks when moving items
        if (_inv.getSelectedItem() != null)
        {
            return;
        }

        // clear current template
        _clearInventoryTemplate(_curInventoryTemplate);

        // copy inventory instead of just keeping a pointer to it.  that way
        // create/apply can be used as an undo button for a single unit and will
        // also work as expected if inventory is modified after 'create' is clicked
        List<BattleItem> unitInv = _battleGame.getSelectedUnit().getInventory();
        foreach (var j in unitInv)
        {
            if (j.getRules().isFixed())
            {
                // don't copy fixed weapons into the template
                continue;
            }

            string ammo;
            if (j.needsAmmo() && j.getAmmoItem() != null)
            {
                ammo = j.getAmmoItem().getRules().getType();
            }
            else
            {
                ammo = "NONE";
            }

            _curInventoryTemplate.Add(new EquipmentLayoutItem(
                    j.getRules().getType(),
                    j.getSlot().getId(),
                    j.getSlotX(),
                    j.getSlotY(),
                    ammo,
                    j.getFuseTimer()));
        }

        // give audio feedback
        _game.getMod().getSoundByDepth((uint)_battleGame.getDepth(), (uint)Mod.Mod.ITEM_DROP).play();
        _refreshMouse();
    }

    void btnApplyTemplateClick(Action _)
    {
        // don't accept clicks when moving items
        // it's ok if the template is empty -- it will just result in clearing the
        // unit's inventory
        if (_inv.getSelectedItem() != null)
        {
            return;
        }

        BattleUnit unit = _battleGame.getSelectedUnit();
        List<BattleItem> unitInv = unit.getInventory();
        Tile groundTile = unit.getTile();
        List<BattleItem> groundInv = groundTile.getInventory();
        RuleInventory groundRuleInv = _game.getMod().getInventory("STR_GROUND", true);

        _clearInventory(_game, unitInv, groundTile);

        // attempt to replicate inventory template by grabbing corresponding items
        // from the ground.  if any item is not found on the ground, display warning
        // message, but continue attempting to fulfill the template as best we can
        bool itemMissing = false;
        foreach (var templateIt in _curInventoryTemplate)
        {
            // search for template item in ground inventory
            bool needsAmmo = _game.getMod().getItem(templateIt.getItemType(), true).getCompatibleAmmo().Any();
            bool found = false;
            bool rescan = true;
            while (rescan)
            {
                rescan = false;

                string targetAmmo = templateIt.getAmmoItem();
                BattleItem matchedWeapon = null;
                BattleItem matchedAmmo = null;
                foreach (var groundItem in groundInv)
                {
                    // if we find the appropriate ammo, remember it for later for if we find
                    // the right weapon but with the wrong ammo
                    string groundItemName = groundItem.getRules().getType();
                    if (needsAmmo && targetAmmo == groundItemName)
                    {
                        matchedAmmo = groundItem;
                    }

                    if (templateIt.getItemType() == groundItemName)
                    {
                        // if the template item would overlap with an existing item (i.e. a fixed
                        // weapon that didn't get cleared in _clearInventory() above), skip it
                        if (Inventory.overlapItems(unit, groundItem,
                                        _game.getMod().getInventory(templateIt.getSlot(), true),
                                        templateIt.getSlotX(), templateIt.getSlotY()))
                        {
                            // don't display 'item not found' warning message
                            found = true;
                            break;
                        }

                        // if the loaded ammo doesn't match the template item's,
                        // remember the weapon for later and continue scanning
                        BattleItem loadedAmmo = groundItem.getAmmoItem();
                        if ((needsAmmo && loadedAmmo != null && targetAmmo != loadedAmmo.getRules().getType())
                         || (needsAmmo && loadedAmmo == null))
                        {
                            // remember the last matched weapon for simplicity (but prefer empty weapons if any are found)
                            if (matchedWeapon == null || matchedWeapon.getAmmoItem() != null)
                            {
                                matchedWeapon = groundItem;
                            }
                            continue;
                        }

                        // move matched item from ground to the appropriate inv slot
                        groundItem.setOwner(unit);
                        groundItem.setTile(null);
                        groundItem.setSlot(_game.getMod().getInventory(templateIt.getSlot(), true));
                        groundItem.setSlotX(templateIt.getSlotX());
                        groundItem.setSlotY(templateIt.getSlotY());
                        groundItem.setFuseTimer(templateIt.getFuseTimer());
                        unitInv.Add(groundItem);
                        groundInv.Remove(groundItem);
                        found = true;
                        break;
                    }
                }

                // if we failed to find an exact match, but found unloaded ammo and
                // the right weapon, unload the target weapon, load the right ammo, and use it
                if (!found && matchedWeapon != null && (!needsAmmo || matchedAmmo != null))
                {
                    // unload the existing ammo (if any) from the weapon
                    BattleItem loadedAmmo = matchedWeapon.getAmmoItem();
                    if (loadedAmmo != null)
                    {
                        groundTile.addItem(loadedAmmo, groundRuleInv);
                        matchedWeapon.setAmmoItem(null);
                    }

                    // load the correct ammo into the weapon
                    if (matchedAmmo != null)
                    {
                        matchedWeapon.setAmmoItem(matchedAmmo);
                        groundTile.removeItem(matchedAmmo);
                    }

                    // rescan and pick up the newly-loaded/unloaded weapon
                    rescan = true;
                }
            }

            if (!found)
            {
                itemMissing = true;
            }
        }

        if (itemMissing)
        {
            _inv.showWarning(tr("STR_NOT_ENOUGH_ITEMS_FOR_TEMPLATE"));
        }

        // refresh ui
        _inv.arrangeGround(false);
        updateStats();
        _refreshMouse();

        // give audio feedback
        _game.getMod().getSoundByDepth((uint)_battleGame.getDepth(), (uint)Mod.Mod.ITEM_DROP).play();
    }

    void onClearInventory(Action _)
    {
        // don't act when moving items
        if (_inv.getSelectedItem() != null)
        {
            return;
        }

        BattleUnit unit = _battleGame.getSelectedUnit();
        List<BattleItem> unitInv = unit.getInventory();
        Tile groundTile = unit.getTile();

        _clearInventory(_game, unitInv, groundTile);

        // refresh ui
        _inv.arrangeGround(false);
        updateStats();
        _refreshMouse();

        // give audio feedback
        _game.getMod().getSoundByDepth((uint)_battleGame.getDepth(), (uint)Mod.Mod.ITEM_DROP).play();
    }

    void onAutoequip(Action _)
    {
        // don't act when moving items
        if (_inv.getSelectedItem() != null)
        {
            return;
        }

        BattleUnit unit = _battleGame.getSelectedUnit();
        Tile groundTile = unit.getTile();
        List<BattleItem> groundInv = groundTile.getInventory();
        Mod.Mod mod = _game.getMod();
        RuleInventory groundRuleInv = mod.getInventory("STR_GROUND", true);
        int worldShade = _battleGame.getGlobalShade();

        var units = new List<BattleUnit> { unit };
        BattlescapeGenerator.autoEquip(units, mod, null, groundInv, groundRuleInv, worldShade, true, true);

        // refresh ui
        _inv.arrangeGround(false);
        updateStats();
        _refreshMouse();

        // give audio feedback
        _game.getMod().getSoundByDepth((uint)_battleGame.getDepth(), (uint)Mod.Mod.ITEM_DROP).play();
    }

    /**
     * Updates item info.
     * @param action Pointer to an action.
     */
    void invClick(Action _) =>
        updateStats();

    /**
     * Shows item info.
     * @param action Pointer to an action.
     */
    void invMouseOver(Action _)
    {
        if (_inv.getSelectedItem() != null)
        {
            return;
        }

        BattleItem item = _inv.getMouseOverItem();
        if (item != null)
        {
            if (item.getUnit() != null && item.getUnit().getStatus() == UnitStatus.STATUS_UNCONSCIOUS)
            {
                _txtItem.setText(item.getUnit().getName(_game.getLanguage()));
            }
            else
            {
                if (_game.getSavedGame().isResearched(item.getRules().getRequirements()))
                {
                    _txtItem.setText(tr(item.getRules().getName()));
                }
                else
                {
                    _txtItem.setText(tr("STR_ALIEN_ARTIFACT"));
                }
            }
            string s = null;
            if (item.getAmmoItem() != null && item.needsAmmo())
            {
                s = tr("STR_AMMO_ROUNDS_LEFT").arg(item.getAmmoItem().getAmmoQuantity());
                SDL_Rect r;
                r.x = 0;
                r.y = 0;
                r.w = RuleInventory.HAND_W * RuleInventory.SLOT_W;
                r.h = RuleInventory.HAND_H * RuleInventory.SLOT_H;
                _selAmmo.drawRect(ref r, (byte)_game.getMod().getInterface("inventory").getElement("grid").color);
                r.x++;
                r.y++;
                r.w -= 2;
                r.h -= 2;
                _selAmmo.drawRect(ref r, (byte)(Palette.blockOffset(0) + 15));
                item.getAmmoItem().getRules().drawHandSprite(_game.getMod().getSurfaceSet("BIGOBS.PCK"), _selAmmo);
                _updateTemplateButtons(false);
            }
            else
            {
                _selAmmo.clear();
                _updateTemplateButtons(!_tu);
            }
            if (item.getAmmoQuantity() != 0 && item.needsAmmo())
            {
                s = tr("STR_AMMO_ROUNDS_LEFT").arg(item.getAmmoQuantity());
            }
            else if (item.getRules().getBattleType() == BattleType.BT_MEDIKIT)
            {
                s = tr("STR_MEDI_KIT_QUANTITIES_LEFT").arg(item.getPainKillerQuantity()).arg(item.getStimulantQuantity()).arg(item.getHealQuantity());
            }
            _txtAmmo.setText(s);
        }
        else
        {
            if (string.IsNullOrEmpty(_currentTooltip))
            {
                _txtItem.setText(string.Empty);
            }
            _txtAmmo.setText(string.Empty);
            _selAmmo.clear();
            _updateTemplateButtons(!_tu);
        }
    }

    /**
     * Hides item info.
     * @param action Pointer to an action.
     */
    void invMouseOut(Action _)
    {
        _txtItem.setText(string.Empty);
        _txtAmmo.setText(string.Empty);
        _selAmmo.clear();
        _updateTemplateButtons(!_tu);
    }

    void _updateTemplateButtons(bool isVisible)
    {
        if (isVisible)
        {
            if (!_curInventoryTemplate.Any())
            {
                // use "empty template" icons
                _game.getMod().getSurface("InvCopy").blit(_btnCreateTemplate);
                _game.getMod().getSurface("InvPasteEmpty").blit(_btnApplyTemplate);
                _btnApplyTemplate.setTooltip("STR_CLEAR_INVENTORY");
            }
            else
            {
                // use "active template" icons
                _game.getMod().getSurface("InvCopyActive").blit(_btnCreateTemplate);
                _game.getMod().getSurface("InvPaste").blit(_btnApplyTemplate);
                _btnApplyTemplate.setTooltip("STR_APPLY_INVENTORY_TEMPLATE");
            }
            _btnCreateTemplate.initSurfaces();
            _btnApplyTemplate.initSurfaces();
        }
        else
        {
            _btnCreateTemplate.clear();
            _btnApplyTemplate.clear();
        }
    }

    void _refreshMouse()
    {
        // send a mouse motion event to refresh any hover actions
        int x, y;
        SDL_GetMouseState(out x, out y);
        SDL_WarpMouseGlobal(x + 1, y);

        // move the mouse back to avoid cursor creep
        SDL_WarpMouseGlobal(x, y);
    }

    /**
     * Updates the soldier stats (Weight, TU).
     */
    void updateStats()
    {
        BattleUnit unit = _battleGame.getSelectedUnit();

        _txtTus.setText(tr("STR_TIME_UNITS_SHORT").arg(unit.getTimeUnits()));

        int weight = unit.getCarriedWeight(_inv.getSelectedItem());
        _txtWeight.setText(tr("STR_WEIGHT").arg(weight).arg(unit.getBaseStats().strength));
        if (weight > unit.getBaseStats().strength)
        {
            _txtWeight.setSecondaryColor((byte)_game.getMod().getInterface("inventory").getElement("weight").color2);
        }
        else
        {
            _txtWeight.setSecondaryColor((byte)_game.getMod().getInterface("inventory").getElement("weight").color);
        }

        _txtFAcc.setText(tr("STR_ACCURACY_SHORT").arg((int)(unit.getBaseStats().firing * unit.getHealth()) / unit.getBaseStats().health));

        _txtReact.setText(tr("STR_REACTIONS_SHORT").arg(unit.getBaseStats().reactions));

        if (unit.getBaseStats().psiSkill > 0)
        {
            _txtPSkill.setText(tr("STR_PSIONIC_SKILL_SHORT").arg(unit.getBaseStats().psiSkill));
        }
        else
        {
            _txtPSkill.setText(string.Empty);
        }

        if (unit.getBaseStats().psiSkill > 0 || (Options.psiStrengthEval && _game.getSavedGame().isResearched(_game.getMod().getPsiRequirements())))
        {
            _txtPStr.setText(tr("STR_PSIONIC_STRENGTH_SHORT").arg(unit.getBaseStats().psiStrength));
        }
        else
        {
            _txtPStr.setText(string.Empty);
        }
    }

    static void _clearInventory(Game game, List<BattleItem> unitInv, Tile groundTile)
    {
        RuleInventory groundRuleInv = game.getMod().getInventory("STR_GROUND", true);

        // clear unit's inventory (i.e. move everything to the ground)
        for (var i = 0; i < unitInv.Count;)
        {
            if (unitInv[i].getRules().isFixed())
            {
                // don't drop fixed weapons
                ++i;
                continue;
            }

            unitInv[i].setOwner(null);
            groundTile.addItem(unitInv[i], groundRuleInv);
            unitInv.RemoveAt(i);
        }
    }

    static void _clearInventoryTemplate(List<EquipmentLayoutItem> inventoryTemplate) =>
        inventoryTemplate.Clear();

    /**
     * Updates all soldier stats when the soldier changes.
     */
    internal override void init()
    {
        base.init();
        BattleUnit unit = _battleGame.getSelectedUnit();

        // no selected unit, close inventory
        if (unit == null)
        {
            btnOkClick(null);
            return;
        }
        // skip to the first unit with inventory
        if (!unit.hasInventory())
        {
            if (_parent != null)
            {
                _parent.selectNextPlayerUnit(false, false, true, _tu);
            }
            else
            {
                _battleGame.selectNextPlayerUnit(false, false, true);
            }
            // no available unit, close inventory
            if (_battleGame.getSelectedUnit() == null || !_battleGame.getSelectedUnit().hasInventory())
            {
                // starting a mission with just vehicles
                btnOkClick(null);
                return;
            }
            else
            {
                unit = _battleGame.getSelectedUnit();
            }
        }

        unit.setCache(null);
        _soldier.clear();
        _btnRank.clear();

        _txtName.setBig();
        _txtName.setText(unit.getName(_game.getLanguage()));
        _inv.setSelectedUnit(unit);
        Soldier s = unit.getGeoscapeSoldier();
        if (s != null)
        {
            SurfaceSet texture = _game.getMod().getSurfaceSet("SMOKE.PCK");
            texture.getFrame(20 + (int)s.getRank()).setX(0);
            texture.getFrame(20 + (int)s.getRank()).setY(0);
            texture.getFrame(20 + (int)s.getRank()).blit(_btnRank);

            string look = s.getArmor().getSpriteInventory();
            if (s.getGender() == SoldierGender.GENDER_MALE)
                look += "M";
            else
                look += "F";
            if (s.getLook() == SoldierLook.LOOK_BLONDE)
                look += "0";
            if (s.getLook() == SoldierLook.LOOK_BROWNHAIR)
                look += "1";
            if (s.getLook() == SoldierLook.LOOK_ORIENTAL)
                look += "2";
            if (s.getLook() == SoldierLook.LOOK_AFRICAN)
                look += "3";
            look += ".SPK";
            if (_game.getMod().getSurface(look, false) == null)
            {
                look = s.getArmor().getSpriteInventory() + ".SPK";
            }
            _game.getMod().getSurface(look).blit(_soldier);
        }
        else
        {
            Surface armorSurface = _game.getMod().getSurface(unit.getArmor().getSpriteInventory(), false);
            if (armorSurface != null)
            {
                armorSurface.blit(_soldier);
            }
        }

        updateStats();
        _refreshMouse();
    }

    /**
     * Takes care of any events from the core game engine.
     * @param action Pointer to an action.
     */
    internal override void handle(Action action)
    {
	    base.handle(action);

	    if (action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
	    {
		    if (action.getDetails().button.button == SDL_BUTTON_X1)
		    {
			    btnNextClick(action);
		    }
		    else if (action.getDetails().button.button == SDL_BUTTON_X2)
		    {
			    btnPrevClick(action);
		    }
	    }
    }
}
