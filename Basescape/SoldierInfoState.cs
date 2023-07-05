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

namespace SharpXcom.Basescape;

/**
 * Soldier Info screen that shows all the
 * info of a specific soldier.
 */
internal class SoldierInfoState : State
{
    Base _base;
    uint _soldierId;
    Soldier _soldier;
    List<Soldier> _list;
    Surface _bg, _rank;
    TextButton _btnOk, _btnPrev, _btnNext, _btnArmor, _btnSack, _btnDiary;
    TextEdit _edtSoldier;
    Text _txtRank, _txtMissions, _txtKills, _txtCraft, _txtRecovery, _txtPsionic, _txtDead;
    Text _txtTimeUnits, _txtStamina, _txtHealth, _txtBravery, _txtReactions, _txtFiring, _txtThrowing, _txtMelee, _txtStrength, _txtPsiStrength, _txtPsiSkill;
    Text _numTimeUnits, _numStamina, _numHealth, _numBravery, _numReactions, _numFiring, _numThrowing, _numMelee, _numStrength, _numPsiStrength, _numPsiSkill;
    Bar _barTimeUnits, _barStamina, _barHealth, _barBravery, _barReactions, _barFiring, _barThrowing, _barMelee, _barStrength, _barPsiStrength, _barPsiSkill;

    /**
     * Initializes all the elements in the Soldier Info screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from. NULL to use the dead soldiers list.
     * @param soldierId ID of the selected soldier.
     */
    internal SoldierInfoState(Base @base, uint soldierId)
    {
        _base = @base;
        _soldierId = soldierId;
        _soldier = null;

        if (_base == null)
        {
            _list = _game.getSavedGame().getDeadSoldiers();
            if (_soldierId >= _list.Count)
            {
                _soldierId = 0;
            }
            else
            {
                _soldierId = (uint)(_list.Count - (1 + _soldierId));
            }
        }
        else
        {
            _list = _base.getSoldiers();
        }

        // Create objects
        _bg = new Surface(320, 200, 0, 0);
        _rank = new Surface(26, 23, 4, 4);
        _btnPrev = new TextButton(28, 14, 0, 33);
        _btnOk = new TextButton(48, 14, 30, 33);
        _btnNext = new TextButton(28, 14, 80, 33);
        _btnArmor = new TextButton(110, 14, 130, 33);
        _edtSoldier = new TextEdit(this, 210, 16, 40, 9);
        _btnSack = new TextButton(60, 14, 260, 33);
        _btnDiary = new TextButton(60, 14, 260, 48);
        _txtRank = new Text(130, 9, 0, 48);
        _txtMissions = new Text(100, 9, 130, 48);
        _txtKills = new Text(100, 9, 200, 48);
        _txtCraft = new Text(130, 9, 0, 56);
        _txtRecovery = new Text(180, 9, 130, 56);
        _txtPsionic = new Text(150, 9, 0, 66);
        _txtDead = new Text(150, 9, 130, 33);

        int yPos = 80;
        int step = 11;

        _txtTimeUnits = new Text(120, 9, 6, yPos);
        _numTimeUnits = new Text(18, 9, 131, yPos);
        _barTimeUnits = new Bar(170, 7, 150, yPos);
        yPos += step;

        _txtStamina = new Text(120, 9, 6, yPos);
        _numStamina = new Text(18, 9, 131, yPos);
        _barStamina = new Bar(170, 7, 150, yPos);
        yPos += step;

        _txtHealth = new Text(120, 9, 6, yPos);
        _numHealth = new Text(18, 9, 131, yPos);
        _barHealth = new Bar(170, 7, 150, yPos);
        yPos += step;

        _txtBravery = new Text(120, 9, 6, yPos);
        _numBravery = new Text(18, 9, 131, yPos);
        _barBravery = new Bar(170, 7, 150, yPos);
        yPos += step;

        _txtReactions = new Text(120, 9, 6, yPos);
        _numReactions = new Text(18, 9, 131, yPos);
        _barReactions = new Bar(170, 7, 150, yPos);
        yPos += step;

        _txtFiring = new Text(120, 9, 6, yPos);
        _numFiring = new Text(18, 9, 131, yPos);
        _barFiring = new Bar(170, 7, 150, yPos);
        yPos += step;

        _txtThrowing = new Text(120, 9, 6, yPos);
        _numThrowing = new Text(18, 9, 131, yPos);
        _barThrowing = new Bar(170, 7, 150, yPos);
        yPos += step;

        _txtMelee = new Text(120, 9, 6, yPos);
        _numMelee = new Text(18, 9, 131, yPos);
        _barMelee = new Bar(170, 7, 150, yPos);
        yPos += step;

        _txtStrength = new Text(120, 9, 6, yPos);
        _numStrength = new Text(18, 9, 131, yPos);
        _barStrength = new Bar(170, 7, 150, yPos);
        yPos += step;

        _txtPsiStrength = new Text(120, 9, 6, yPos);
        _numPsiStrength = new Text(18, 9, 131, yPos);
        _barPsiStrength = new Bar(170, 7, 150, yPos);
        yPos += step;

        _txtPsiSkill = new Text(120, 9, 6, yPos);
        _numPsiSkill = new Text(18, 9, 131, yPos);
        _barPsiSkill = new Bar(170, 7, 150, yPos);

        // Set palette
        setInterface("soldierInfo");

        add(_bg);
        add(_rank);
        add(_btnOk, "button", "soldierInfo");
        add(_btnPrev, "button", "soldierInfo");
        add(_btnNext, "button", "soldierInfo");
        add(_btnArmor, "button", "soldierInfo");
        add(_edtSoldier, "text1", "soldierInfo");
        add(_btnSack, "button", "soldierInfo");
        add(_btnDiary, "button", "soldierInfo");
        add(_txtRank, "text1", "soldierInfo");
        add(_txtMissions, "text1", "soldierInfo");
        add(_txtKills, "text1", "soldierInfo");
        add(_txtCraft, "text1", "soldierInfo");
        add(_txtRecovery, "text1", "soldierInfo");
        add(_txtPsionic, "text2", "soldierInfo");
        add(_txtDead, "text2", "soldierInfo");

        add(_txtTimeUnits, "text2", "soldierInfo");
        add(_numTimeUnits, "numbers", "soldierInfo");
        add(_barTimeUnits, "barTUs", "soldierInfo");

        add(_txtStamina, "text2", "soldierInfo");
        add(_numStamina, "numbers", "soldierInfo");
        add(_barStamina, "barEnergy", "soldierInfo");

        add(_txtHealth, "text2", "soldierInfo");
        add(_numHealth, "numbers", "soldierInfo");
        add(_barHealth, "barHealth", "soldierInfo");

        add(_txtBravery, "text2", "soldierInfo");
        add(_numBravery, "numbers", "soldierInfo");
        add(_barBravery, "barBravery", "soldierInfo");

        add(_txtReactions, "text2", "soldierInfo");
        add(_numReactions, "numbers", "soldierInfo");
        add(_barReactions, "barReactions", "soldierInfo");

        add(_txtFiring, "text2", "soldierInfo");
        add(_numFiring, "numbers", "soldierInfo");
        add(_barFiring, "barFiring", "soldierInfo");

        add(_txtThrowing, "text2", "soldierInfo");
        add(_numThrowing, "numbers", "soldierInfo");
        add(_barThrowing, "barThrowing", "soldierInfo");

        add(_txtMelee, "text2", "soldierInfo");
        add(_numMelee, "numbers", "soldierInfo");
        add(_barMelee, "barMelee", "soldierInfo");

        add(_txtStrength, "text2", "soldierInfo");
        add(_numStrength, "numbers", "soldierInfo");
        add(_barStrength, "barStrength", "soldierInfo");

        add(_txtPsiStrength, "text2", "soldierInfo");
        add(_numPsiStrength, "numbers", "soldierInfo");
        add(_barPsiStrength, "barPsiStrength", "soldierInfo");

        add(_txtPsiSkill, "text2", "soldierInfo");
        add(_numPsiSkill, "numbers", "soldierInfo");
        add(_barPsiSkill, "barPsiSkill", "soldierInfo");

        centerAllSurfaces();

        // Set up objects
        _game.getMod().getSurface("BACK06.SCR").blit(_bg);

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _btnPrev.setText("<<");
        if (_base == null)
        {
            _btnPrev.onMouseClick(btnNextClick);
            _btnPrev.onKeyboardPress(btnNextClick, Options.keyBattlePrevUnit);
        }
        else
        {
            _btnPrev.onMouseClick(btnPrevClick);
            _btnPrev.onKeyboardPress(btnPrevClick, Options.keyBattlePrevUnit);
        }

        _btnNext.setText(">>");
        if (_base == null)
        {
            _btnNext.onMouseClick(btnPrevClick);
            _btnNext.onKeyboardPress(btnPrevClick, Options.keyBattleNextUnit);
        }
        else
        {
            _btnNext.onMouseClick(btnNextClick);
            _btnNext.onKeyboardPress(btnNextClick, Options.keyBattleNextUnit);
        }

        _btnArmor.setText(tr("STR_ARMOR"));
        _btnArmor.onMouseClick(btnArmorClick);

        _edtSoldier.setBig();
        _edtSoldier.onChange(edtSoldierChange);
        _edtSoldier.onMousePress(edtSoldierPress);

        _btnSack.setText(tr("STR_SACK"));
        _btnSack.onMouseClick(btnSackClick);

        _btnDiary.setText(tr("STR_DIARY"));
        _btnDiary.onMouseClick(btnDiaryClick);

        _txtPsionic.setText(tr("STR_IN_PSIONIC_TRAINING"));

        _txtTimeUnits.setText(tr("STR_TIME_UNITS"));

        _barTimeUnits.setScale(1.0);

        _txtStamina.setText(tr("STR_STAMINA"));

        _barStamina.setScale(1.0);

        _txtHealth.setText(tr("STR_HEALTH"));

        _barHealth.setScale(1.0);

        _txtBravery.setText(tr("STR_BRAVERY"));

        _barBravery.setScale(1.0);

        _txtReactions.setText(tr("STR_REACTIONS"));

        _barReactions.setScale(1.0);

        _txtFiring.setText(tr("STR_FIRING_ACCURACY"));

        _barFiring.setScale(1.0);

        _txtThrowing.setText(tr("STR_THROWING_ACCURACY"));

        _barThrowing.setScale(1.0);

        _txtMelee.setText(tr("STR_MELEE_ACCURACY"));

        _barMelee.setScale(1.0);

        _txtStrength.setText(tr("STR_STRENGTH"));

        _barStrength.setScale(1.0);

        _txtPsiStrength.setText(tr("STR_PSIONIC_STRENGTH"));

        _barPsiStrength.setScale(1.0);

        _txtPsiSkill.setText(tr("STR_PSIONIC_SKILL"));

        _barPsiSkill.setScale(1.0);
    }

    /**
     *
     */
    ~SoldierInfoState() { }

    /**
     * Updates soldier stats when
     * the soldier changes.
     */
    void init()
    {
        base.init();
        if (!_list.Any())
        {
            _game.popState();
            return;
        }
        if (_soldierId >= _list.Count)
        {
            _soldierId = 0;
        }
        _soldier = _list[(int)_soldierId];
        _edtSoldier.setBig();
        _edtSoldier.setText(_soldier.getName());
        UnitStats initial = _soldier.getInitStats();
        UnitStats current = _soldier.getCurrentStats();

        UnitStats withArmor = current;
        withArmor += _soldier.getArmor().getStats();

        SurfaceSet texture = _game.getMod().getSurfaceSet("BASEBITS.PCK");
        texture.getFrame(_soldier.getRankSprite()).setX(0);
        texture.getFrame(_soldier.getRankSprite()).setY(0);
        texture.getFrame(_soldier.getRankSprite()).blit(_rank);

        string ss = withArmor.tu.ToString();
        _numTimeUnits.setText(ss);
        _barTimeUnits.setMax(current.tu);
        _barTimeUnits.setValue(withArmor.tu);
        _barTimeUnits.setValue2(Math.Min(withArmor.tu, initial.tu));

        string ss2 = withArmor.stamina.ToString();
        _numStamina.setText(ss2);
        _barStamina.setMax(current.stamina);
        _barStamina.setValue(withArmor.stamina);
        _barStamina.setValue2(Math.Min(withArmor.stamina, initial.stamina));

        string ss3 = withArmor.health.ToString();
        _numHealth.setText(ss3);
        _barHealth.setMax(current.health);
        _barHealth.setValue(withArmor.health);
        _barHealth.setValue2(Math.Min(withArmor.health, initial.health));

        string ss4 = withArmor.bravery.ToString();
        _numBravery.setText(ss4);
        _barBravery.setMax(current.bravery);
        _barBravery.setValue(withArmor.bravery);
        _barBravery.setValue2(Math.Min(withArmor.bravery, initial.bravery));

        string ss5 = withArmor.reactions.ToString();
        _numReactions.setText(ss5);
        _barReactions.setMax(current.reactions);
        _barReactions.setValue(withArmor.reactions);
        _barReactions.setValue2(Math.Min(withArmor.reactions, initial.reactions));

        string ss6 = withArmor.firing.ToString();
        _numFiring.setText(ss6);
        _barFiring.setMax(current.firing);
        _barFiring.setValue(withArmor.firing);
        _barFiring.setValue2(Math.Min(withArmor.firing, initial.firing));

        string ss7 = withArmor.throwing.ToString();
        _numThrowing.setText(ss7);
        _barThrowing.setMax(current.throwing);
        _barThrowing.setValue(withArmor.throwing);
        _barThrowing.setValue2(Math.Min(withArmor.throwing, initial.throwing));

        string ss8 = withArmor.melee.ToString();
        _numMelee.setText(ss8);
        _barMelee.setMax(current.melee);
        _barMelee.setValue(withArmor.melee);
        _barMelee.setValue2(Math.Min(withArmor.melee, initial.melee));

        string ss9 = withArmor.strength.ToString();
        _numStrength.setText(ss9);
        _barStrength.setMax(current.strength);
        _barStrength.setValue(withArmor.strength);
        _barStrength.setValue2(Math.Min(withArmor.strength, initial.strength));

        string wsArmor;
        string armorType = _soldier.getArmor().getType();
        if (armorType == _soldier.getRules().getArmor())
        {
            wsArmor = tr("STR_ARMOR_").arg(tr(armorType));
        }
        else
        {
            wsArmor = tr(armorType);
        }

        _btnArmor.setText(wsArmor);

        _btnSack.setVisible(_game.getSavedGame().getMonthsPassed() > -1 && !(_soldier.getCraft() != null && _soldier.getCraft().getStatus() == "STR_OUT"));

        _txtRank.setText(tr("STR_RANK_").arg(tr(_soldier.getRankString())));

        _txtMissions.setText(tr("STR_MISSIONS").arg(_soldier.getMissions()));

        _txtKills.setText(tr("STR_KILLS").arg(_soldier.getKills()));

        string craft;
        if (_soldier.getCraft() == null)
        {
            craft = tr("STR_NONE_UC");
        }
        else
        {
            craft = _soldier.getCraft().getName(_game.getLanguage());
        }
        _txtCraft.setText(tr("STR_CRAFT_").arg(craft));

        if (_soldier.getWoundRecovery() > 0)
        {
            _txtRecovery.setText(tr("STR_WOUND_RECOVERY").arg(tr("STR_DAY", (uint)_soldier.getWoundRecovery())));
        }
        else
        {
            _txtRecovery.setText(string.Empty);
        }

        _txtPsionic.setVisible(_soldier.isInPsiTraining());

        if (current.psiSkill > 0 || (Options.psiStrengthEval && _game.getSavedGame().isResearched(_game.getMod().getPsiRequirements())))
        {
            string ss14 = withArmor.psiStrength.ToString();
            _numPsiStrength.setText(ss14);
            _barPsiStrength.setMax(current.psiStrength);
            _barPsiStrength.setValue(withArmor.psiStrength);
            _barPsiStrength.setValue2(Math.Min(withArmor.psiStrength, initial.psiStrength));

            _txtPsiStrength.setVisible(true);
            _numPsiStrength.setVisible(true);
            _barPsiStrength.setVisible(true);
        }
        else
        {
            _txtPsiStrength.setVisible(false);
            _numPsiStrength.setVisible(false);
            _barPsiStrength.setVisible(false);
        }

        if (current.psiSkill > 0)
        {
            string ss15 = withArmor.psiSkill.ToString();
            _numPsiSkill.setText(ss15);
            _barPsiSkill.setMax(current.psiSkill);
            _barPsiSkill.setValue(withArmor.psiSkill);
            _barPsiSkill.setValue2(Math.Min(withArmor.psiSkill, initial.psiSkill));

            _txtPsiSkill.setVisible(true);
            _numPsiSkill.setVisible(true);
            _barPsiSkill.setVisible(true);
        }
        else
        {
            _txtPsiSkill.setVisible(false);
            _numPsiSkill.setVisible(false);
            _barPsiSkill.setVisible(false);
        }

        // Dead can't talk
        if (_base == null)
        {
            _btnArmor.setVisible(false);
            _btnSack.setVisible(false);
            _txtCraft.setVisible(false);
            _txtDead.setVisible(true);
            string status = "STR_MISSING_IN_ACTION";
            if (_soldier.getDeath() != null && _soldier.getDeath().getCause() != default)
            {
                status = "STR_KILLED_IN_ACTION";
            }
            _txtDead.setText(tr(status, (uint)_soldier.getGender()));
        }
        else
        {
            _txtDead.setVisible(false);
        }
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Engine.Action _)
    {
        _game.popState();
        if (_game.getSavedGame().getMonthsPassed() > -1 && Options.storageLimitsEnforced && _base != null && _base.storesOverfull())
        {
            _game.pushState(new SellState(_base));
            _game.pushState(new ErrorMessageState(tr("STR_STORAGE_EXCEEDED").arg(_base.getName()), _palette, (byte)_game.getMod().getInterface("soldierInfo").getElement("errorMessage").color, "BACK01.SCR", _game.getMod().getInterface("soldierInfo").getElement("errorPalette").color));
        }
    }

    /**
     * Goes to the next soldier.
     * @param action Pointer to an action.
     */
    void btnNextClick(Engine.Action _)
    {
        _soldierId++;
        if (_soldierId >= _list.Count)
            _soldierId = 0;
        init();
    }

    /**
     * Goes to the previous soldier.
     * @param action Pointer to an action.
     */
    void btnPrevClick(Engine.Action _)
    {
        if (_soldierId == 0)
            _soldierId = (uint)(_list.Count - 1);
        else
            _soldierId--;
        init();
    }

    /**
     * Shows the Select Armor window.
     * @param action Pointer to an action.
     */
    void btnArmorClick(Engine.Action _)
    {
        if (_soldier.getCraft() == null || (_soldier.getCraft() != null && _soldier.getCraft().getStatus() != "STR_OUT"))
        {
            _game.pushState(new SoldierArmorState(_base, _soldierId));
        }
    }

    /**
     * Changes the soldier's name.
     * @param action Pointer to an action.
     */
    void edtSoldierChange(Engine.Action _) =>
        _soldier.setName(_edtSoldier.getText());

    /**
     * Disables the soldier input.
     * @param action Pointer to an action.
     */
    void edtSoldierPress(Engine.Action _)
    {
        if (_base == null)
        {
            _edtSoldier.setFocus(false);
        }
    }

    /**
     * Shows the Sack Soldier window.
     * @param action Pointer to an action.
     */
    void btnSackClick(Engine.Action _) =>
        _game.pushState(new SackSoldierState(_base, _soldierId));

    /**
     * Shows the Diary Soldier window.
     * @param action Pointer to an action.
     */
    void btnDiaryClick(Engine.Action _) =>
        _game.pushState(new SoldierDiaryOverviewState(_base, _soldierId, this));

    /**
     * Set the soldier Id.
     */
    internal void setSoldierId(uint soldier) =>
        _soldierId = soldier;
}
