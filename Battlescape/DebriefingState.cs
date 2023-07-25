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

struct ReequipStat { internal string item; internal int qty; internal string craft; }

struct RecoveryItem { string name; int value; }

struct DebriefingStat
{
    string item;
    int qty;
    int score;
    bool recovery;

    DebriefingStat(string _item, bool _recovery)
    {
        item = _item;
        qty = 0;
        score = 0;
        recovery = _recovery;
    }
}

/**
 * Debriefing screen shown after a Battlescape
 * mission that displays the results.
 */
internal class DebriefingState : State
{
    Region _region;
    Country _country;
    bool _positiveScore, _noContainment, _manageContainment, _destroyBase, _promotions, _initDone;
    /// True when soldier stat improvements are shown rather than scores. Toggled with the corresponding button.
    bool _showSoldierStats;
    MissionStatistics _missionStatistics;
    int _limitsEnforced;
    Window _window;
    TextButton _btnOk, _btnStats;
    Text _txtTitle, _txtItem, _txtQuantity, _txtScore, _txtRecovery, _txtRating,
	     _txtSoldier, _txtTU, _txtStamina, _txtHealth, _txtBravery, _txtReactions,
	     _txtFiring, _txtThrowing, _txtMelee, _txtStrength, _txtPsiStrength, _txtPsiSkill;
    TextList _lstStats, _lstRecovery, _lstTotal, _lstSoldierStats;
    Text _txtTooltip;
    string _currentTooltip;
    List<DebriefingStat> _stats;
    Dictionary<int, RecoveryItem> _recoveryStats;
    Dictionary<RuleItem, int> _rounds;
    List<Soldier> _soldiersCommended, _deadSoldiersCommended;
    List<ReequipStat> _missingItems;
    Base _base;

    /**
     * Initializes all the elements in the Debriefing screen.
     * @param game Pointer to the core game.
     */
    internal DebriefingState()
    {
        _region = null;
        _country = null;
        _positiveScore = true;
        _noContainment = false;
        _manageContainment = false;
        _destroyBase = false;
        _initDone = false;
        _showSoldierStats = false;

        _missionStatistics = new MissionStatistics();

        Options.baseXResolution = Options.baseXGeoscape;
        Options.baseYResolution = Options.baseYGeoscape;
        _game.getScreen().resetDisplay(false);

        // Restore the cursor in case something weird happened
        _game.getCursor().setVisible(true);
        _limitsEnforced = Options.storageLimitsEnforced ? 1 : 0;

        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        _btnOk = new TextButton(40, 12, 16, 180);
        _btnStats = new TextButton(40, 12, 264, 180);
        _txtTitle = new Text(300, 17, 16, 8);
        _txtItem = new Text(180, 9, 16, 24);
        _txtQuantity = new Text(60, 9, 200, 24);
        _txtScore = new Text(55, 9, 270, 24);
        _txtRecovery = new Text(180, 9, 16, 60);
        _txtRating = new Text(200, 9, 64, 180);
        _lstStats = new TextList(290, 80, 16, 32);
        _lstRecovery = new TextList(290, 80, 16, 32);
        _lstTotal = new TextList(290, 9, 16, 12);

        // Second page (soldier stats)
        _txtSoldier = new Text(90, 9, 16, 24); //16..106 = 90
        _txtTU = new Text(18, 9, 106, 24); //106
        _txtStamina = new Text(18, 9, 124, 24); //124
        _txtHealth = new Text(18, 9, 142, 24); //142
        _txtBravery = new Text(18, 9, 160, 24); //160
        _txtReactions = new Text(18, 9, 178, 24); //178
        _txtFiring = new Text(18, 9, 196, 24); //196
        _txtThrowing = new Text(18, 9, 214, 24); //214
        _txtMelee = new Text(18, 9, 232, 24); //232
        _txtStrength = new Text(18, 9, 250, 24); //250
        _txtPsiStrength = new Text(18, 9, 268, 24); //268
        _txtPsiSkill = new Text(18, 9, 286, 24); //286..304 = 18

        _lstSoldierStats = new TextList(288, 128, 16, 32);

        _txtTooltip = new Text(200, 9, 64, 180);

        applyVisibility();

        // Set palette
        setInterface("debriefing");

        add(_window, "window", "debriefing");
        add(_btnOk, "button", "debriefing");
        add(_btnStats, "button", "debriefing");
        add(_txtTitle, "heading", "debriefing");
        add(_txtItem, "text", "debriefing");
        add(_txtQuantity, "text", "debriefing");
        add(_txtScore, "text", "debriefing");
        add(_txtRecovery, "text", "debriefing");
        add(_txtRating, "text", "debriefing");
        add(_lstStats, "list", "debriefing");
        add(_lstRecovery, "list", "debriefing");
        add(_lstTotal, "totals", "debriefing");

        add(_txtSoldier, "text", "debriefing");
        add(_txtTU, "text", "debriefing");
        add(_txtStamina, "text", "debriefing");
        add(_txtHealth, "text", "debriefing");
        add(_txtBravery, "text", "debriefing");
        add(_txtReactions, "text", "debriefing");
        add(_txtFiring, "text", "debriefing");
        add(_txtThrowing, "text", "debriefing");
        add(_txtMelee, "text", "debriefing");
        add(_txtStrength, "text", "debriefing");
        add(_txtPsiStrength, "text", "debriefing");
        add(_txtPsiSkill, "text", "debriefing");
        add(_lstSoldierStats, "list", "debriefing");
        add(_txtTooltip, "text", "debriefing");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _btnStats.onMouseClick(btnStatsClick);

        _txtTitle.setBig();

        _txtItem.setText(tr("STR_LIST_ITEM"));

        _txtQuantity.setText(tr("STR_QUANTITY_UC"));
        _txtQuantity.setAlign(TextHAlign.ALIGN_RIGHT);

        _txtScore.setText(tr("STR_SCORE"));

        _lstStats.setColumns(3, 224, 30, 64);
        _lstStats.setDot(true);

        _lstRecovery.setColumns(3, 224, 30, 64);
        _lstRecovery.setDot(true);

        _lstTotal.setColumns(2, 254, 64);
        _lstTotal.setDot(true);

        // Second page
        _txtSoldier.setText(tr("STR_NAME_UC"));

        _txtTU.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTU.setText(tr("STR_TIME_UNITS_ABBREVIATION"));
        _txtTU.setTooltip("STR_TIME_UNITS");
        _txtTU.onMouseIn(txtTooltipIn);
        _txtTU.onMouseOut(txtTooltipOut);

        _txtStamina.setAlign(TextHAlign.ALIGN_CENTER);
        _txtStamina.setText(tr("STR_STAMINA_ABBREVIATION"));
        _txtStamina.setTooltip("STR_STAMINA");
        _txtStamina.onMouseIn(txtTooltipIn);
        _txtStamina.onMouseOut(txtTooltipOut);

        _txtHealth.setAlign(TextHAlign.ALIGN_CENTER);
        _txtHealth.setText(tr("STR_HEALTH_ABBREVIATION"));
        _txtHealth.setTooltip("STR_HEALTH");
        _txtHealth.onMouseIn(txtTooltipIn);
        _txtHealth.onMouseOut(txtTooltipOut);

        _txtBravery.setAlign(TextHAlign.ALIGN_CENTER);
        _txtBravery.setText(tr("STR_BRAVERY_ABBREVIATION"));
        _txtBravery.setTooltip("STR_BRAVERY");
        _txtBravery.onMouseIn(txtTooltipIn);
        _txtBravery.onMouseOut(txtTooltipOut);

        _txtReactions.setAlign(TextHAlign.ALIGN_CENTER);
        _txtReactions.setText(tr("STR_REACTIONS_ABBREVIATION"));
        _txtReactions.setTooltip("STR_REACTIONS");
        _txtReactions.onMouseIn(txtTooltipIn);
        _txtReactions.onMouseOut(txtTooltipOut);

        _txtFiring.setAlign(TextHAlign.ALIGN_CENTER);
        _txtFiring.setText(tr("STR_FIRING_ACCURACY_ABBREVIATION"));
        _txtFiring.setTooltip("STR_FIRING_ACCURACY");
        _txtFiring.onMouseIn(txtTooltipIn);
        _txtFiring.onMouseOut(txtTooltipOut);

        _txtThrowing.setAlign(TextHAlign.ALIGN_CENTER);
        _txtThrowing.setText(tr("STR_THROWING_ACCURACY_ABBREVIATION"));
        _txtThrowing.setTooltip("STR_THROWING_ACCURACY");
        _txtThrowing.onMouseIn(txtTooltipIn);
        _txtThrowing.onMouseOut(txtTooltipOut);

        _txtMelee.setAlign(TextHAlign.ALIGN_CENTER);
        _txtMelee.setText(tr("STR_MELEE_ACCURACY_ABBREVIATION"));
        _txtMelee.setTooltip("STR_MELEE_ACCURACY");
        _txtMelee.onMouseIn(txtTooltipIn);
        _txtMelee.onMouseOut(txtTooltipOut);

        _txtStrength.setAlign(TextHAlign.ALIGN_CENTER);
        _txtStrength.setText(tr("STR_STRENGTH_ABBREVIATION"));
        _txtStrength.setTooltip("STR_STRENGTH");
        _txtStrength.onMouseIn(txtTooltipIn);
        _txtStrength.onMouseOut(txtTooltipOut);

        _txtPsiStrength.setAlign(TextHAlign.ALIGN_CENTER);
        _txtPsiStrength.setText(tr("STR_PSIONIC_STRENGTH_ABBREVIATION"));
        _txtPsiStrength.setTooltip("STR_PSIONIC_STRENGTH");
        _txtPsiStrength.onMouseIn(txtTooltipIn);
        _txtPsiStrength.onMouseOut(txtTooltipOut);

        _txtPsiSkill.setAlign(TextHAlign.ALIGN_CENTER);
        _txtPsiSkill.setText(tr("STR_PSIONIC_SKILL_ABBREVIATION"));
        _txtPsiSkill.setTooltip("STR_PSIONIC_SKILL");
        _txtPsiSkill.onMouseIn(txtTooltipIn);
        _txtPsiSkill.onMouseOut(txtTooltipOut);

        _lstSoldierStats.setColumns(13, 90, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 0);
        _lstSoldierStats.setAlign(TextHAlign.ALIGN_CENTER);
        _lstSoldierStats.setAlign(TextHAlign.ALIGN_LEFT, 0);
        _lstSoldierStats.setDot(true);
    }

    /**
     *
     */
    ~DebriefingState()
    {
        _stats.Clear();
        _recoveryStats.Clear();
        _rounds.Clear();
    }

    /**
    * Shows a tooltip for the appropriate text.
    * @param action Pointer to an action.
    */
    void txtTooltipIn(Action action)
    {
	    _currentTooltip = action.getSender().getTooltip();
	    _txtTooltip.setText(tr(_currentTooltip));}

    /**
    * Clears the tooltip text.
    * @param action Pointer to an action.
    */
    void txtTooltipOut(Action action)
    {
	    if (_currentTooltip == action.getSender().getTooltip())
	    {
		    _txtTooltip.setText(string.Empty);
	    }
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        _game.popState();
        if (_game.getSavedGame().getMonthsPassed() == -1)
        {
            _game.setState(new MainMenuState());
        }
        else
        {
            if (_deadSoldiersCommended.Any())
            {
                _game.pushState(new CommendationLateState(_deadSoldiersCommended));
            }
            if (_soldiersCommended.Any())
            {
                _game.pushState(new CommendationState(_soldiersCommended));
            }
            if (!_destroyBase)
            {
                if (_promotions)
                {
                    _game.pushState(new PromotionsState());
                }
                if (_missingItems.Any())
                {
                    _game.pushState(new CannotReequipState(_missingItems));
                }
                if (_manageContainment)
                {
                    _game.pushState(new ManageAlienContainmentState(_base, OptionsOrigin.OPT_BATTLESCAPE));
                    _game.pushState(new ErrorMessageState(tr("STR_CONTAINMENT_EXCEEDED").arg(_base.getName()), _palette, (byte)_game.getMod().getInterface("debriefing").getElement("errorMessage").color, "BACK01.SCR", _game.getMod().getInterface("debriefing").getElement("errorPalette").color));
                }
                if (!_manageContainment && Options.storageLimitsEnforced && _base.storesOverfull())
                {
                    _game.pushState(new SellState(_base, OptionsOrigin.OPT_BATTLESCAPE));
                    _game.pushState(new ErrorMessageState(tr("STR_STORAGE_EXCEEDED").arg(_base.getName()), _palette, (byte)_game.getMod().getInterface("debriefing").getElement("errorMessage").color, "BACK01.SCR", _game.getMod().getInterface("debriefing").getElement("errorPalette").color));
                }
            }

            // Autosave after mission
            if (_game.getSavedGame().isIronman())
            {
                _game.pushState(new SaveGameState(OptionsOrigin.OPT_GEOSCAPE, SaveType.SAVE_IRONMAN, _palette));
            }
            else if (Options.autosave)
            {
                _game.pushState(new SaveGameState(OptionsOrigin.OPT_GEOSCAPE, SaveType.SAVE_AUTO_GEOSCAPE, _palette));
            }
        }
    }

    /**
     * Displays soldiers' stat increases.
     * @param action Pointer to an action.
     */
    void btnStatsClick(Action _)
    {
        _showSoldierStats = !_showSoldierStats;
        applyVisibility();
    }

    void applyVisibility()
    {
        // First page (scores)
        _txtItem.setVisible(!_showSoldierStats);
        _txtQuantity.setVisible(!_showSoldierStats);
        _txtScore.setVisible(!_showSoldierStats);
        _txtRecovery.setVisible(!_showSoldierStats);
        _txtRating.setVisible(!_showSoldierStats);
        _lstStats.setVisible(!_showSoldierStats);
        _lstRecovery.setVisible(!_showSoldierStats);
        _lstTotal.setVisible(!_showSoldierStats);

        // Second page (soldier stats)
        _txtSoldier.setVisible(_showSoldierStats);
        _txtTU.setVisible(_showSoldierStats);
        _txtStamina.setVisible(_showSoldierStats);
        _txtHealth.setVisible(_showSoldierStats);
        _txtBravery.setVisible(_showSoldierStats);
        _txtReactions.setVisible(_showSoldierStats);
        _txtFiring.setVisible(_showSoldierStats);
        _txtThrowing.setVisible(_showSoldierStats);
        _txtMelee.setVisible(_showSoldierStats);
        _txtStrength.setVisible(_showSoldierStats);
        _txtPsiStrength.setVisible(_showSoldierStats);
        _txtPsiSkill.setVisible(_showSoldierStats);
        _lstSoldierStats.setVisible(_showSoldierStats);
        _txtTooltip.setVisible(_showSoldierStats);

        // Set text on toggle button accordingly
        _btnStats.setText(_showSoldierStats ? tr("STR_SCORE") : tr("STR_STATS"));
    }
}
