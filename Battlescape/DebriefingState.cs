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

struct RecoveryItem { internal string name; internal int value; }

/* struct */ class DebriefingStat
{
    internal string item;
    internal int qty;
    internal int score;
    internal bool recovery;

    internal DebriefingStat(string _item, bool _recovery)
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
	List<KeyValuePair<string, UnitStats>> _soldierStats;

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

	protected override void init()
	{
		base.init();	

		if (_initDone)
		{
			return;
		}
		_initDone = true;

		prepareDebriefing();

		foreach (var i in _soldierStats)
		{
			_lstSoldierStats.addRow(13, i.Key,
					makeSoldierString(i.Value.tu),
					makeSoldierString(i.Value.stamina),
					makeSoldierString(i.Value.health),
					makeSoldierString(i.Value.bravery),
					makeSoldierString(i.Value.reactions),
					makeSoldierString(i.Value.firing),
					makeSoldierString(i.Value.throwing),
					makeSoldierString(i.Value.melee),
					makeSoldierString(i.Value.strength),
					makeSoldierString(i.Value.psiStrength),
					makeSoldierString(i.Value.psiSkill),
					string.Empty);
			// note: final dummy element to cause dot filling until the end of the line
		}

		int total = 0, statsY = 0, recoveryY = 0;
		int civiliansSaved = 0, civiliansDead = 0;
		int aliensKilled = 0, aliensStunned = 0;
		foreach (var i in _stats)
		{
			if (i.qty == 0)
				continue;

			string ss = $"{Unicode.TOK_COLOR_FLIP}{i.qty}{Unicode.TOK_COLOR_FLIP}";
			string ss2 = $"{Unicode.TOK_COLOR_FLIP}{i.score}";
			total += i.score;
			if (i.recovery)
			{
				_lstRecovery.addRow(3, tr(i.item), ss, ss2);
				recoveryY += 8;
			}
			else
			{
				_lstStats.addRow(3, tr(i.item), ss, ss2);
				statsY += 8;
			}
			if (i.item == "STR_CIVILIANS_SAVED")
			{
				civiliansSaved = i.qty;
			}
			if (i.item == "STR_CIVILIANS_KILLED_BY_XCOM_OPERATIVES" || i.item == "STR_CIVILIANS_KILLED_BY_ALIENS")
			{
				civiliansDead += i.qty;
			}
			if (i.item == "STR_ALIENS_KILLED")
			{
				aliensKilled += i.qty;
			}
			if (i.item == "STR_LIVE_ALIENS_RECOVERED")
			{
				aliensStunned += i.qty;
			}
		}
		if (civiliansSaved != 0 && civiliansDead == 0 && _missionStatistics.success == true)
		{
			_missionStatistics.valiantCrux = true;
		}

		string ss3 = total.ToString();
		_lstTotal.addRow(2, tr("STR_TOTAL_UC"), ss3);

		// add the points to our activity score
		if (_region != null)
		{
			_region.addActivityXcom(total);
		}
		if (_country != null)
		{
			_country.addActivityXcom(total);
		}

		if (recoveryY > 0)
		{
			_txtRecovery.setY(_lstStats.getY() + statsY + 5);
			_lstRecovery.setY(_txtRecovery.getY() + 8);
			_lstTotal.setY(_lstRecovery.getY() + recoveryY + 5);
		}
		else
		{
			_txtRecovery.setText(string.Empty);
			_lstTotal.setY(_lstStats.getY() + statsY + 5);
		}

		// Calculate rating
		string rating;
		if (total <= -200)
		{
			rating = "STR_RATING_TERRIBLE";
		}
		else if (total <= 0)
		{
			rating = "STR_RATING_POOR";
		}
		else if (total <= 200)
		{
			rating = "STR_RATING_OK";
		}
		else if (total <= 500)
		{
			rating = "STR_RATING_GOOD";
		}
		else
		{
			rating = "STR_RATING_EXCELLENT";
		}
		_missionStatistics.rating = rating;
		_missionStatistics.score = total;
		_txtRating.setText(tr("STR_RATING").arg(tr(rating)));

		SavedGame save = _game.getSavedGame();
		SavedBattleGame battle = save.getSavedBattle();

		_missionStatistics.daylight = save.getSavedBattle().getGlobalShade();
		_missionStatistics.id = _game.getSavedGame().getMissionStatistics().Count;
		_game.getSavedGame().getMissionStatistics().Add(_missionStatistics);

		// Award Best-of commendations.
		int[] bestScoreID = {0, 0, 0, 0, 0, 0, 0};
		int[] bestScore = {0, 0, 0, 0, 0, 0, 0};
		int bestOverallScorersID = 0;
		int bestOverallScore = 0;

		// Check to see if any of the dead soldiers were exceptional.
		foreach (var deadUnit in battle.getUnits())
		{
			if (deadUnit.getGeoscapeSoldier() == null || deadUnit.getStatus() != UnitStatus.STATUS_DEAD)
			{
				continue;
			}

			/// Post-mortem kill award
			int killTurn = -1;
			foreach (var killerUnit in battle.getUnits())
			{
				foreach (var kill in killerUnit.getStatistics().kills)
				{
					if (kill.id == deadUnit.getId())
					{
						killTurn = kill.turn;
						break;
					}
				}
				if (killTurn != -1)
				{
					break;
				}
			}
			int postMortemKills = 0;
			if (killTurn != -1)
			{
				foreach (var deadUnitKill in deadUnit.getStatistics().kills)
				{
					if (deadUnitKill.turn > killTurn && deadUnitKill.faction == UnitFaction.FACTION_HOSTILE)
					{
						postMortemKills++;
					}
				}
			}
			deadUnit.getGeoscapeSoldier().getDiary().awardPostMortemKill(postMortemKills);

			SoldierRank rank = deadUnit.getGeoscapeSoldier().getRank();
			// Rookies don't get this next award. No one likes them.
			if (rank == SoldierRank.RANK_ROOKIE)
			{
				continue;
			}

			/// Best-of awards
			// Find the best soldier per rank by comparing score.
			foreach (var j in _game.getSavedGame().getDeadSoldiers())
			{
				int score = j.getDiary().getScoreTotal(_game.getSavedGame().getMissionStatistics());

				// Don't forget this mission's score!
				if (j.getId() == deadUnit.getId())
				{
					score += _missionStatistics.score;
				}

				if (score > bestScore[(int)rank])
				{
					bestScoreID[(int)rank] = deadUnit.getId();
					bestScore[(int)rank] = score;
					if (score > bestOverallScore)
					{
						bestOverallScorersID = deadUnit.getId();
						bestOverallScore = score;
					}
				}
			}
		}
		// Now award those soldiers commendations!
		foreach (var deadUnit in battle.getUnits())
		{
			if (deadUnit.getGeoscapeSoldier() == null || deadUnit.getStatus() != UnitStatus.STATUS_DEAD)
			{
				continue;
			}
			if (deadUnit.getId() == bestScoreID[(int)deadUnit.getGeoscapeSoldier().getRank()])
			{
				deadUnit.getGeoscapeSoldier().getDiary().awardBestOfRank(bestScore[(int)deadUnit.getGeoscapeSoldier().getRank()]);
			}
			if (deadUnit.getId() == bestOverallScorersID)
			{
				deadUnit.getGeoscapeSoldier().getDiary().awardBestOverall(bestOverallScore);
			}
		}

		foreach (var j in battle.getUnits())
		{
			if (j.getGeoscapeSoldier() != null)
			{
				int soldierAlienKills = 0;
				int soldierAlienStuns = 0;
				foreach (var k in j.getStatistics().kills)
				{
					if (k.faction == UnitFaction.FACTION_HOSTILE && k.status == UnitStatus.STATUS_DEAD)
					{
						soldierAlienKills++;
					}
					if (k.faction == UnitFaction.FACTION_HOSTILE && k.status == UnitStatus.STATUS_UNCONSCIOUS)
					{
						soldierAlienStuns++;
					}
				}

				if (aliensKilled != 0 && aliensKilled == soldierAlienKills && _missionStatistics.success == true && aliensStunned == soldierAlienStuns)
				{
					j.getStatistics().nikeCross = true;
				}
				if (aliensStunned != 0 && aliensStunned == soldierAlienStuns && _missionStatistics.success == true && aliensKilled == 0)
				{
					j.getStatistics().mercyCross = true;
				}
				j.getStatistics().daysWounded = j.getGeoscapeSoldier().getWoundRecovery();
				_missionStatistics.injuryList[j.getGeoscapeSoldier().getId()] = j.getGeoscapeSoldier().getWoundRecovery();

				// Award Martyr Medal
				if (j.getMurdererId() == j.getId() && j.getStatistics().kills.Count != 0)
				{
					int martyrKills = 0; // How many aliens were killed on the same turn?
					int martyrTurn = -1;
					foreach (var unitKill in j.getStatistics().kills)
					{
						if ( unitKill.id == j.getId() )
						{
							martyrTurn = unitKill.turn;
							break;
						}
					}
					foreach (var unitKill in j.getStatistics().kills)
					{
						if (unitKill.turn == martyrTurn && unitKill.faction == UnitFaction.FACTION_HOSTILE)
						{
							martyrKills++;
						}
					}
					if (martyrKills > 0)
					{
						if (martyrKills > 10)
						{
							martyrKills = 10;
						}
						j.getStatistics().martyr = martyrKills;
					}
				}

				// Set the UnitStats delta
				j.getStatistics().delta = j.getGeoscapeSoldier().getCurrentStats() - j.getGeoscapeSoldier().getInitStats();

				j.getGeoscapeSoldier().getDiary().updateDiary(j.getStatistics(), _game.getSavedGame().getMissionStatistics(), _game.getMod());
				if (!j.getStatistics().MIA && !j.getStatistics().KIA && j.getGeoscapeSoldier().getDiary().manageCommendations(_game.getMod(), _game.getSavedGame().getMissionStatistics()))
				{
					_soldiersCommended.Add(j.getGeoscapeSoldier());
				}
				else if (j.getStatistics().MIA || j.getStatistics().KIA)
				{
					j.getGeoscapeSoldier().getDiary().manageCommendations(_game.getMod(), _game.getSavedGame().getMissionStatistics());
					_deadSoldiersCommended.Add(j.getGeoscapeSoldier());
				}
			}
		}

		_positiveScore = (total > 0);

		var participants = new List<Soldier>();
		foreach (var i in _game.getSavedGame().getSavedBattle().getUnits())
		{
			if (i.getGeoscapeSoldier() != null)
			{
				participants.Add(i.getGeoscapeSoldier());
			}
		}
		_promotions = _game.getSavedGame().handlePromotions(participants);

		_game.getSavedGame().setBattleGame(null);

		if (_positiveScore)
		{
			_game.getMod().playMusic(Mod.Mod.DEBRIEF_MUSIC_GOOD);
		}
		else
		{
			_game.getMod().playMusic(Mod.Mod.DEBRIEF_MUSIC_BAD);
		}
		if (_noContainment)
		{
			_game.pushState(new ErrorMessageState(tr("STR_ALIEN_DIES_NO_ALIEN_CONTAINMENT_FACILITY"), _palette, (byte)_game.getMod().getInterface("debriefing").getElement("errorMessage").color, "BACK01.SCR", _game.getMod().getInterface("debriefing").getElement("errorPalette").color));
			_noContainment = false;
		}
	}

	string makeSoldierString(int stat)
	{
		if (stat == 0) return string.Empty;

		return $"{Unicode.TOK_COLOR_FLIP}+{stat}{Unicode.TOK_COLOR_FLIP}";
	}

	/**
	 * Prepares debriefing: gathers Aliens, Corpses, Artefacts, UFO Components.
	 * Adds the items to the craft.
	 * Also calculates the soldiers experience, and possible promotions.
	 * If aborted, only the things on the exit area are recovered.
	 */
	void prepareDebriefing()
	{
		foreach (var i in _game.getMod().getItemsList())
		{
			RuleItem rule = _game.getMod().getItem(i);
			if (rule.getSpecialType() > 1)
			{
				RecoveryItem item = new RecoveryItem();
				item.name = i;
				item.value = rule.getRecoveryPoints();
				_recoveryStats[rule.getSpecialType()] = item;
				_missionStatistics.lootValue = item.value;
			}
		}

		SavedGame save = _game.getSavedGame();
		SavedBattleGame battle = save.getSavedBattle();
		AlienDeployment ruleDeploy = _game.getMod().getDeployment(battle.getMissionType());

		bool aborted = battle.isAborted();
		bool success = !aborted || battle.allObjectivesDestroyed();
		Craft craft = null;
		Base @base = null;
		string target = null;

		int playersInExitArea = 0; // if this stays 0 the craft is lost...
		int playersSurvived = 0; // if this stays 0 the craft is lost...
		int playersUnconscious = 0;
		int playersInEntryArea = 0;
		int playersMIA = 0;

		_stats.Add(new DebriefingStat("STR_ALIENS_KILLED", false));
		_stats.Add(new DebriefingStat("STR_ALIEN_CORPSES_RECOVERED", false));
		_stats.Add(new DebriefingStat("STR_LIVE_ALIENS_RECOVERED", false));
		_stats.Add(new DebriefingStat("STR_ALIEN_ARTIFACTS_RECOVERED", false));

		string objectiveCompleteText = null, objectiveFailedText = null;
		int objectiveCompleteScore = 0, objectiveFailedScore = 0;
		if (ruleDeploy != null)
		{
			if (ruleDeploy.getObjectiveCompleteInfo(out objectiveCompleteText, out objectiveCompleteScore))
			{
				_stats.Add(new DebriefingStat(objectiveCompleteText, false));
			}
			if (ruleDeploy.getObjectiveFailedInfo(out objectiveFailedText, out objectiveFailedScore))
			{
				_stats.Add(new DebriefingStat(objectiveFailedText, false));
			}
		}

		_stats.Add(new DebriefingStat("STR_CIVILIANS_KILLED_BY_ALIENS", false));
		_stats.Add(new DebriefingStat("STR_CIVILIANS_KILLED_BY_XCOM_OPERATIVES", false));
		_stats.Add(new DebriefingStat("STR_CIVILIANS_SAVED", false));
		_stats.Add(new DebriefingStat("STR_XCOM_OPERATIVES_KILLED", false));
		//_stats.Add(new DebriefingStat("STR_XCOM_OPERATIVES_RETIRED_THROUGH_INJURY", false));
		_stats.Add(new DebriefingStat("STR_XCOM_OPERATIVES_MISSING_IN_ACTION", false));
		_stats.Add(new DebriefingStat("STR_TANKS_DESTROYED", false));
		_stats.Add(new DebriefingStat("STR_XCOM_CRAFT_LOST", false));

		foreach (var i in _recoveryStats)
		{
			_stats.Add(new DebriefingStat(i.Value.name, true));
		}

		_missionStatistics.time = save.getTime();
		_missionStatistics.type = battle.getMissionType();
		_stats.Add(new DebriefingStat(_game.getMod().getAlienFuelName(), true));

		foreach (var i in save.getBases())
		{
			// in case we have a craft - check which craft it is about
			foreach (var j in i.getCrafts())
			{
				if (j.isInBattlescape())
				{
					foreach (var k in _game.getSavedGame().getRegions())
					{
						if (k.getRules().insideRegion(j.getLongitude(), j.getLatitude()))
						{
							_region = k;
							_missionStatistics.region = _region.getRules().getType();
							break;
						}
					}
					foreach (var k in _game.getSavedGame().getCountries())
					{
						if (k.getRules().insideCountry(j.getLongitude(), j.getLatitude()))
						{
							_country = k;
							_missionStatistics.country = _country.getRules().getType();
							break;
						}
					}
					craft = j;
					@base = i;
					if (craft.getDestination() != null)
					{
						_missionStatistics.markerName = craft.getDestination().getMarkerName();
						_missionStatistics.markerId = craft.getDestination().getMarkerId();
						target = craft.getDestination().getType();
						// Ignore custom mission names
						if (craft.getDestination() is AlienBase)
						{
							target = "STR_ALIEN_BASE";
						}
						else if (craft.getDestination() is MissionSite)
						{
							target = "STR_MISSION_SITE";
						}
					}
					craft.returnToBase();
					craft.setMissionComplete(true);
					craft.setInBattlescape(false);
				}
				else if (j.getDestination() != null)
				{
					Ufo u = j.getDestination() as Ufo;
					if (u != null && u.isInBattlescape())
					{
						j.returnToBase();
					}
					MissionSite ms = j.getDestination() as MissionSite;
					if (ms != null && ms.isInBattlescape())
					{
						j.returnToBase();
					}
				}
			}
			// in case we DON'T have a craft (base defense)
			if (i.isInBattlescape())
			{
				@base = i;
				target = @base.getType();
				@base.setInBattlescape(false);
				@base.cleanupDefenses(false);
				foreach (var k in _game.getSavedGame().getRegions())
				{
					if (k.getRules().insideRegion(@base.getLongitude(), @base.getLatitude()))
					{
						_region = k;
						_missionStatistics.region = _region.getRules().getType();
						break;
					}
				}
				foreach (var k in _game.getSavedGame().getCountries())
				{
					if (k.getRules().insideCountry(@base.getLongitude(), @base.getLatitude()))
					{
						_country = k;
						_missionStatistics.country= _country.getRules().getType();
						break;
					}
				}
				// Loop through the UFOs and see which one is sitting on top of the base... that is probably the one attacking you.
				foreach (var k in save.getUfos())
				{
					if (AreSame(k.getLongitude(), @base.getLongitude()) && AreSame(k.getLatitude(), @base.getLatitude()))
					{
						_missionStatistics.ufo = k.getRules().getType();
						_missionStatistics.alienRace = k.getAlienRace();
						break;
					}
				}
				if (aborted)
				{
					_destroyBase = true;
				}
				var facilities = @base.getFacilities();
				for (var k = 0; k < facilities.Count;)
				{
					// this facility was demolished
					if (battle.getModuleMap()[facilities[k].getX()][facilities[k].getY()].Value == 0)
					{
						@base.destroyFacility(facilities[k]);
					}
					else
					{
						++k;
					}
				}
				// this may cause the base to become disjointed, destroy the disconnected parts.
				@base.destroyDisconnectedFacilities();
			}
		}

		// mission site disappears (even when you abort)
		foreach (var i in save.getMissionSites())
		{
			if (i.isInBattlescape())
			{
				_missionStatistics.alienRace = i.getAlienRace();
				save.getMissionSites().Remove(i);
				break;
			}
		}

		// lets see what happens with units

		// first, we evaluate how many surviving XCom units there are, and how many are conscious
		// and how many have died (to use for commendations)
		int deadSoldiers = 0;
		foreach (var j in battle.getUnits())
		{
			if (j.getOriginalFaction() == UnitFaction.FACTION_PLAYER && j.getStatus() != UnitStatus.STATUS_DEAD)
			{
				if (j.getStatus() == UnitStatus.STATUS_UNCONSCIOUS || j.getFaction() == UnitFaction.FACTION_HOSTILE)
				{
					playersUnconscious++;
				}
				else if (j.getStatus() == UnitStatus.STATUS_IGNORE_ME && j.getStunlevel() >= j.getHealth())
				{
					// even for ignored xcom units, we need to know if they're conscious or unconscious
					playersUnconscious++;
				}
				else if (j.isInExitArea(SpecialTileType.END_POINT))
				{
					playersInExitArea++;
				}
				else if (j.isInExitArea(SpecialTileType.START_POINT))
				{
					playersInEntryArea++;
				}
				else if (aborted)
				{
					// if aborted, conscious xcom unit that is not on start/end point counts as MIA
					playersMIA++;
				}
				playersSurvived++;
			}
			else if (j.getOriginalFaction() == UnitFaction.FACTION_PLAYER && j.getStatus() == UnitStatus.STATUS_DEAD)
				deadSoldiers++;
		}
		// if all our men are unconscious, the aliens get to have their way with them.
		if (playersUnconscious + playersMIA == playersSurvived)
		{
			playersSurvived = playersMIA;
			foreach (var j in battle.getUnits())
			{
				if (j.getOriginalFaction() == UnitFaction.FACTION_PLAYER && j.getStatus() != UnitStatus.STATUS_DEAD)
				{
					if (j.getStatus() == UnitStatus.STATUS_UNCONSCIOUS || j.getFaction() == UnitFaction.FACTION_HOSTILE)
					{
						j.instaKill();
					}
					else if (j.getStatus() == UnitStatus.STATUS_IGNORE_ME && j.getStunlevel() >= j.getHealth())
					{
						j.instaKill();
					}
					else
					{
						// do nothing, units will be marked MIA later
					}
				}
			}
		}

		// if it's a UFO, let's see what happens to it
		foreach (var i in save.getUfos())
		{
			if (i.isInBattlescape())
			{
				_missionStatistics.ufo = i.getRules().getType();
				if (save.getMonthsPassed() != -1)
				{
					_missionStatistics.alienRace = i.getAlienRace();
				}
				_txtRecovery.setText(tr("STR_UFO_RECOVERY"));
				i.setInBattlescape(false);
				// if XCom failed to secure the landing zone, the UFO
				// takes off immediately and proceeds according to its mission directive
				if (i.getStatus() == UfoStatus.LANDED && (aborted || playersSurvived == 0))
				{
					 i.setSecondsRemaining(5);
				}
				// if XCom succeeds, or it's a crash site, the UFO disappears
				else
				{
					save.getUfos().Remove(i);
				}
				break;
			}
		}

		if (ruleDeploy != null && ruleDeploy.getEscapeType() != EscapeType.ESCAPE_NONE)
		{
			if (ruleDeploy.getEscapeType() != EscapeType.ESCAPE_EXIT)
			{
				success = playersInEntryArea > 0;
			}

			if (ruleDeploy.getEscapeType() != EscapeType.ESCAPE_ENTRY)
			{
				success = success || playersInExitArea > 0;
			}
		}

		playersInExitArea = 0;

		if (playersSurvived == 1)
		{
			foreach (var j in battle.getUnits())
			{
				// if only one soldier survived, give him a medal! (unless he killed all the others...)
				if (j.getStatus() != UnitStatus.STATUS_DEAD && j.getOriginalFaction() == UnitFaction.FACTION_PLAYER && !j.getStatistics().hasFriendlyFired() && deadSoldiers != 0)
				{
					j.getStatistics().loneSurvivor = true;
					break;
				}
				// if only one soldier survived AND none have died, means only one soldier went on the mission...
				if (j.getStatus() != UnitStatus.STATUS_DEAD && j.getOriginalFaction() == UnitFaction.FACTION_PLAYER && deadSoldiers == 0)
				{
					j.getStatistics().ironMan = true;
				}
			}
		}
		// alien base disappears (if you didn't abort)
		foreach (var i in save.getAlienBases())
		{
			if (i.isInBattlescape())
			{
				_txtRecovery.setText(tr("STR_ALIEN_BASE_RECOVERY"));
				bool destroyAlienBase = true;

				if (aborted || playersSurvived == 0)
				{
					if (!battle.allObjectivesDestroyed())
						destroyAlienBase = false;
				}

				if (ruleDeploy != null && !string.IsNullOrEmpty(ruleDeploy.getNextStage()))
				{
					_missionStatistics.alienRace = i.getAlienRace();
					destroyAlienBase = false;
				}

				success = destroyAlienBase;
				if (destroyAlienBase)
				{
					if (!string.IsNullOrEmpty(objectiveCompleteText))
					{
						addStat(objectiveCompleteText, 1, objectiveCompleteScore);
					}
					// Take care to remove supply missions for this base.
					save.getAlienMissions().ForEach(x => ClearAlienBase(x, i));

					save.getAlienBases().Remove(i);
					break;
				}
				else
				{
					i.setInBattlescape(false);
					break;
				}
			}
		}

		// time to care for units.
		foreach (var j in battle.getUnits())
		{
			UnitStatus status = j.getStatus();
			UnitFaction faction = j.getFaction();
			UnitFaction oldFaction = j.getOriginalFaction();
			int value = j.getValue();
			Soldier soldier = save.getSoldier(j.getId());

			if (j.getTile() == null)
			{
				Position pos = j.getPosition();
				if (pos == new Position(-1, -1, -1))
				{
					foreach (var k in battle.getItems())
					{
						if (k.getUnit() != null && k.getUnit() == j)
						{
							if (k.getOwner() != null)
							{
								pos = k.getOwner().getPosition();
							}
							else if (k.getTile() != null)
							{
								pos = k.getTile().getPosition();
							}
						}
					}
				}
				j.setTile(battle.getTile(pos));
			}

			if (status == UnitStatus.STATUS_DEAD)
			{ // so this is a dead unit
				if (oldFaction == UnitFaction.FACTION_HOSTILE && j.killedBy() == UnitFaction.FACTION_PLAYER)
				{
					addStat("STR_ALIENS_KILLED", 1, value);
				}
				else if (oldFaction == UnitFaction.FACTION_PLAYER)
				{
					if (soldier != null)
					{
						addStat("STR_XCOM_OPERATIVES_KILLED", 1, -value);
						j.updateGeoscapeStats(soldier);
						j.getStatistics().KIA = true;
						save.killSoldier(soldier); // in case we missed the soldier death on battlescape
					}
					else
					{ // non soldier player = tank
						addStat("STR_TANKS_DESTROYED", 1, -value);
					}
				}
				else if (oldFaction == UnitFaction.FACTION_NEUTRAL)
				{
					if (j.killedBy() == UnitFaction.FACTION_PLAYER)
						addStat("STR_CIVILIANS_KILLED_BY_XCOM_OPERATIVES", 1, -j.getValue() - (2 * (j.getValue() / 3)));
					else // if civilians happen to kill themselves XCOM shouldn't get penalty for it
						addStat("STR_CIVILIANS_KILLED_BY_ALIENS", 1, -j.getValue());
				}
			}
			else
			{ // so this unit is not dead...
				if (oldFaction == UnitFaction.FACTION_PLAYER)
				{
					if (((j.isInExitArea(SpecialTileType.START_POINT) || j.getStatus() == UnitStatus.STATUS_IGNORE_ME) && (battle.getMissionType() != "STR_BASE_DEFENSE" || success)) || !aborted || (aborted && j.isInExitArea(SpecialTileType.END_POINT)))
					{ // so game is not aborted or aborted and unit is on exit area
						j.postMissionProcedures(save, out var statIncrease);
						if (j.getGeoscapeSoldier() != null)
							_soldierStats.Add(KeyValuePair.Create<string, UnitStats>(j.getGeoscapeSoldier().getName(), statIncrease));
						playersInExitArea++;

						recoverItems(j.getInventory(), @base);

						if (soldier != null)
						{
							// calculate new statString
							soldier.calcStatString(_game.getMod().getStatStrings(), (Options.psiStrengthEval && _game.getSavedGame().isResearched(_game.getMod().getPsiRequirements())));
						}
						else
						{ // non soldier player = tank
							@base.getStorageItems().addItem(j.getType());
							RuleItem tankRule = _game.getMod().getItem(j.getType(), true);
							if (j.getItem("STR_RIGHT_HAND") != null)
							{
								BattleItem ammoItem = j.getItem("STR_RIGHT_HAND").getAmmoItem();
								if (tankRule.getCompatibleAmmo().Any() && ammoItem != null && ammoItem.getAmmoQuantity() > 0)
								{
									int total = ammoItem.getAmmoQuantity();

									if (tankRule.getClipSize() != 0) // meaning this tank can store multiple clips
									{
										total /= ammoItem.getRules().getClipSize();
									}

									@base.getStorageItems().addItem(tankRule.getCompatibleAmmo().First(), total);
								}
							}
							if (j.getItem("STR_LEFT_HAND") != null)
							{
								RuleItem secondaryRule = j.getItem("STR_LEFT_HAND").getRules();
								BattleItem ammoItem = j.getItem("STR_LEFT_HAND").getAmmoItem();
								if (secondaryRule.getCompatibleAmmo().Any() && ammoItem != null && ammoItem.getAmmoQuantity() > 0)
								{
									int total = ammoItem.getAmmoQuantity();

									if (secondaryRule.getClipSize() != 0) // meaning this tank can store multiple clips
									{
										total /= ammoItem.getRules().getClipSize();
									}

									@base.getStorageItems().addItem(secondaryRule.getCompatibleAmmo().First(), total);
								}
							}
						}
					}
					else
					{ // so game is aborted and unit is not on exit area
						addStat("STR_XCOM_OPERATIVES_MISSING_IN_ACTION", 1, -value);
						playersSurvived--;
						if (soldier != null)
						{
							j.updateGeoscapeStats(soldier);
							j.getStatistics().MIA = true;
							save.killSoldier(soldier);
						}
					}
				}
				else if (oldFaction == UnitFaction.FACTION_HOSTILE && (!aborted || j.isInExitArea(SpecialTileType.START_POINT)) && !_destroyBase
					// mind controlled units may as well count as unconscious
					&& faction == UnitFaction.FACTION_PLAYER && (!j.isOut() || j.getStatus() == UnitStatus.STATUS_IGNORE_ME))
				{
					if (j.getTile() != null)
					{
						foreach (var k in j.getInventory())
						{
							if (!k.getRules().isFixed())
							{
								j.getTile().addItem(k, _game.getMod().getInventory("STR_GROUND", true));
							}
						}
					}
					recoverAlien(j, @base);
				}
				else if (oldFaction == UnitFaction.FACTION_NEUTRAL)
				{
					// if mission fails, all civilians die
					if (aborted || playersSurvived == 0)
					{
						addStat("STR_CIVILIANS_KILLED_BY_ALIENS", 1, -j.getValue());
					}
					else
					{
						addStat("STR_CIVILIANS_SAVED", 1, j.getValue());
					}
				}
			}
		}
		bool lostCraft = false;
		if (craft != null && ((playersInExitArea == 0 && aborted) || (playersSurvived == 0)))
		{
			addStat("STR_XCOM_CRAFT_LOST", 1, -craft.getRules().getScore());
			// Since this is not a base defense mission, we can safely erase the craft,
			// without worrying it's vehicles' destructor calling double (on base defense missions
			// all vehicle object in the craft is also referenced by base.getVehicles() !!)
			@base.removeCraft(craft, false);
			craft = null; // To avoid a crash down there!!
			lostCraft = true;
			playersSurvived = 0; // assuming you aborted and left everyone behind
			success = false;
		}
		if ((aborted || playersSurvived == 0) && target == "STR_BASE")
		{
			foreach (var i in @base.getCrafts())
			{
				addStat("STR_XCOM_CRAFT_LOST", 1, -i.getRules().getScore());
			}
			playersSurvived = 0; // assuming you aborted and left everyone behind
			success = false;
		}
		if ((!aborted || success) && playersSurvived > 0) 	// RECOVER UFO : run through all tiles to recover UFO components and items
		{
			if (target == "STR_BASE")
			{
				_txtTitle.setText(tr("STR_BASE_IS_SAVED"));
			}
			else if (target == "STR_UFO")
			{
				_txtTitle.setText(tr("STR_UFO_IS_RECOVERED"));
			}
			else if (target == "STR_ALIEN_BASE")
			{
				_txtTitle.setText(tr("STR_ALIEN_BASE_DESTROYED"));
			}
			else
			{
				_txtTitle.setText(tr("STR_ALIENS_DEFEATED"));
				if (!string.IsNullOrEmpty(objectiveCompleteText))
				{
					int victoryStat = 0;
					if (ruleDeploy.getEscapeType() != EscapeType.ESCAPE_NONE)
					{
						if (ruleDeploy.getEscapeType() != EscapeType.ESCAPE_EXIT)
						{
							victoryStat += playersInEntryArea;
						}
						if (ruleDeploy.getEscapeType() != EscapeType.ESCAPE_ENTRY)
						{
							victoryStat += playersInExitArea;
						}
					}
					else
					{
						victoryStat = 1;
					}

					addStat(objectiveCompleteText, victoryStat, objectiveCompleteScore);
				}
			}

			if (!aborted)
			{
				// if this was a 2-stage mission, and we didn't abort (ie: we have time to clean up)
				// we can recover items from the earlier stages as well
				recoverItems(battle.getConditionalRecoveredItems(), @base);
				int nonRecoverType = 0;
				if (ruleDeploy != null && ruleDeploy.getObjectiveType() != 0)
				{
					nonRecoverType = ruleDeploy.getObjectiveType();
				}
				for (int i = 0; i < battle.getMapSizeXYZ(); ++i)
				{
					// get recoverable map data objects from the battlescape map
					for (var tp = TilePart.O_FLOOR; tp <= TilePart.O_OBJECT; ++tp)
					{
						//TilePart tp = (TilePart)part;
						if (battle.getTiles()[i].getMapData(tp) != null)
						{
							int specialType = (int)battle.getTiles()[i].getMapData(tp).getSpecialType();
							if (specialType != nonRecoverType && _recoveryStats.ContainsKey(specialType))
							{
								addStat(_recoveryStats[specialType].name, 1, _recoveryStats[specialType].value);
							}
						}
					}
					// recover items from the floor
					recoverItems(battle.getTiles()[i].getInventory(), @base);
				}
			}
			else
			{
				for (int i = 0; i < battle.getMapSizeXYZ(); ++i)
				{
					if (battle.getTiles()[i].getMapData(TilePart.O_FLOOR) != null && (battle.getTiles()[i].getMapData(TilePart.O_FLOOR).getSpecialType() == SpecialTileType.START_POINT))
						recoverItems(battle.getTiles()[i].getInventory(), @base);
				}
			}
		}
		else
		{
			if (lostCraft)
			{
				_txtTitle.setText(tr("STR_CRAFT_IS_LOST"));
			}
			else if (target == "STR_BASE")
			{
				_txtTitle.setText(tr("STR_BASE_IS_LOST"));
				_destroyBase = true;
			}
			else if (target == "STR_UFO")
			{
				_txtTitle.setText(tr("STR_UFO_IS_NOT_RECOVERED"));
			}
			else if (target == "STR_ALIEN_BASE")
			{
				_txtTitle.setText(tr("STR_ALIEN_BASE_STILL_INTACT"));
			}
			else
			{
				_txtTitle.setText(tr("STR_TERROR_CONTINUES"));
				if (!string.IsNullOrEmpty(objectiveFailedText))
				{
					addStat(objectiveFailedText, 1, objectiveFailedScore);
				}
			}

			if (playersSurvived > 0 && !_destroyBase)
			{
				// recover items from the craft floor
				for (int i = 0; i < battle.getMapSizeXYZ(); ++i)
				{
					if (battle.getTiles()[i].getMapData(TilePart.O_FLOOR) != null && (battle.getTiles()[i].getMapData(TilePart.O_FLOOR).getSpecialType() == SpecialTileType.START_POINT))
						recoverItems(battle.getTiles()[i].getInventory(), @base);
				}
			}
		}

		// recover all our goodies
		if (playersSurvived > 0)
		{
			int aadivider = (target == "STR_UFO") ? 10 : 150;
			foreach (var i in _stats)
			{
				// alien alloys recovery values are divided by 10 or divided by 150 in case of an alien base
				if (i.item == _recoveryStats[(int)SpecialTileType.ALIEN_ALLOYS].name)
				{
					i.qty = i.qty / aadivider;
					i.score = i.score / aadivider;
				}

				// recoverable battlescape tiles are now converted to items and put in base inventory
				if (i.recovery && i.qty > 0)
				{
					@base.getStorageItems().addItem(i.item, i.qty);
				}
			}

			// assuming this was a multi-stage mission,
			// recover everything that was in the craft in the previous stage
			recoverItems(battle.getGuaranteedRecoveredItems(), @base);
		}

		// calculate the clips for each type based on the recovered rounds.
		foreach (var i in _rounds)
		{
			int total_clips = i.Value / i.Key.getClipSize();
			if (total_clips > 0)
				@base.getStorageItems().addItem(i.Key.getType(), total_clips);
		}

		// reequip craft after a non-base-defense mission (of course only if it's not lost already (that case craft=0))
		if (craft != null)
		{
			reequipCraft(@base, craft, true);
		}

		if (target == "STR_BASE")
		{
			if (!_destroyBase)
			{
				// reequip crafts (only those on the base) after a base defense mission
				foreach (var c in @base.getCrafts())
				{
					if (c.getStatus() != "STR_OUT")
						reequipCraft(@base, c, false);
				}
				// Clear @base.getVehicles() objects, they aren't needed anymore.
				@base.getVehicles().Clear();
			}
			else if (_game.getSavedGame().getMonthsPassed() != -1)
			{
				foreach (var i in _game.getSavedGame().getBases())
				{
					if (i == @base)
					{
						@base = null; // To avoid similar (potential) problems as with the deleted craft
						_game.getSavedGame().getBases().Remove(i);
						break;
					}
				}
			}

			if (_region != null)
			{
				AlienMission am = _game.getSavedGame().findAlienMission(_region.getRules().getType(), MissionObjective.OBJECTIVE_RETALIATION);
				var ufos = _game.getSavedGame().getUfos();
				for (var i = 0; i < ufos.Count;)
				{
					if (ufos[i].getMission() == am)
					{
						ufos.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
				foreach (var i in _game.getSavedGame().getAlienMissions())
				{
					if ((AlienMission)i == am)
					{
						_game.getSavedGame().getAlienMissions().Remove(i);
						break;
					}
				}
			}
		}
		_missionStatistics.success = success;

		// remember the base for later use (of course only if it's not lost already (in that case base=0))
		_base = @base;
	}

	/**
	 * Adds to the debriefing stats.
	 * @param name The untranslated name of the stat.
	 * @param quantity The quantity to add.
	 * @param score The score to add.
	 */
	void addStat(string name, int quantity, int score)
	{
		for (var i = 0; i < _stats.Count; i++)
		{
			if (_stats[i].item == name)
			{
				_stats[i].qty = _stats[i].qty + quantity;
				_stats[i].score = _stats[i].score + score;
				break;
			}
		}
	}

	/**
	 * Recovers items from the battlescape.
	 *
	 * Converts the battlescape inventory into a geoscape itemcontainer.
	 * @param from Items recovered from the battlescape.
	 * @param base Base to add items to.
	 */
	void recoverItems(List<BattleItem> from, Base @base)
	{
		foreach (var it in from)
		{
			if (it.getRules().getName() == _game.getMod().getAlienFuelName())
			{
				// special case of an item counted as a stat
				addStat(_game.getMod().getAlienFuelName(), _game.getMod().getAlienFuelQuantity(), it.getRules().getRecoveryPoints());
			}
			else
			{
				if (it.getRules().isRecoverable() && !it.getXCOMProperty())
				{
					if (it.getRules().getBattleType() == BattleType.BT_CORPSE)
					{
						BattleUnit corpseUnit = it.getUnit();
						if (corpseUnit.getStatus() == UnitStatus.STATUS_DEAD)
						{
							@base.getStorageItems().addItem(corpseUnit.getArmor().getCorpseGeoscape(), 1);
							addStat("STR_ALIEN_CORPSES_RECOVERED", 1, it.getRules().getRecoveryPoints());
						}
						else if (corpseUnit.getStatus() == UnitStatus.STATUS_UNCONSCIOUS ||
								// or it's in timeout because it's unconscious from the previous stage
								// units can be in timeout and alive, and we assume they flee.
								(corpseUnit.getStatus() == UnitStatus.STATUS_IGNORE_ME &&
								corpseUnit.getHealth() > 0 &&
								corpseUnit.getHealth() < corpseUnit.getStunlevel()))
						{
							if (corpseUnit.getOriginalFaction() == UnitFaction.FACTION_HOSTILE)
							{
								recoverAlien(corpseUnit, @base);
							}
						}
					}
					// only add recovery points for unresearched items
					else if (!_game.getSavedGame().isResearched(it.getRules().getRequirements()))
					{
						addStat("STR_ALIEN_ARTIFACTS_RECOVERED", 1, it.getRules().getRecoveryPoints());
					}
				}

				// put items back in the base
				if (!it.getRules().isFixed() && it.getRules().isRecoverable())
				{
					switch (it.getRules().getBattleType())
					{
						case BattleType.BT_CORPSE: // corpses are handled above, do not process them here.
							break;
						case BattleType.BT_AMMO:
							// It's a clip, count any rounds left.
							_rounds[it.getRules()] += it.getAmmoQuantity();
							break;
						case BattleType.BT_FIREARM:
						case BattleType.BT_MELEE:
							// It's a weapon, count any rounds left in the clip.
							{
								BattleItem clip = it.getAmmoItem();
								if (clip != null && clip.getRules().getClipSize() > 0 && clip != it)
								{
									_rounds[clip.getRules()] += clip.getAmmoQuantity();
								}
							}
							// Fall-through, to recover the weapon itself.
							goto default;
						default:
							@base.getStorageItems().addItem(it.getRules().getType(), 1);
							break;
					}
					if (it.getRules().getBattleType() == BattleType.BT_NONE)
					{
						foreach (var c in @base.getCrafts())
						{
							c.reuseItem(it.getRules().getType());
						}
					}
				}
			}
		}
	}

	/**
	 * Recovers a live alien from the battlescape.
	 * @param from Battle unit to recover.
	 * @param base Base to add items to.
	 */
	void recoverAlien(BattleUnit from, Base @base)
	{
		// Zombie handling: don't recover a zombie.
		if (!string.IsNullOrEmpty(from.getSpawnUnit()))
		{
			// convert it, and mind control the resulting unit.
			// reason: zombies don't create unconscious bodies... ever.
			// the only way we can get into this situation is if psi-capture is enabled.
			// we can use that knowledge to our advantage to save having to make it unconscious and spawn a body item for it.
			BattleUnit newUnit = _game.getSavedGame().getSavedBattle().convertUnit(from, _game.getSavedGame(), _game.getMod());
			newUnit.convertToFaction(UnitFaction.FACTION_PLAYER);
			// don't process the zombie itself, our new unit just got added to the end of the vector we're iterating, and will be handled later.
			return;
		}
		string type = from.getType();
		if (@base.getAvailableContainment() == 0 && _game.getSavedGame().getMonthsPassed() > -1)
		{
			_noContainment = true;
			if (from.getArmor().getCorpseBattlescape().Any())
			{
				RuleItem corpseRule = _game.getMod().getItem(from.getArmor().getCorpseBattlescape().First());
				if (corpseRule != null && corpseRule.isRecoverable())
				{
					addStat("STR_ALIEN_CORPSES_RECOVERED", 1, corpseRule.getRecoveryPoints());
					string corpseItem = from.getArmor().getCorpseGeoscape();
					@base.getStorageItems().addItem(corpseItem, 1);
				}
			}
		}
		else
		{
			RuleResearch research = _game.getMod().getResearch(type);
			if (research != null && !_game.getSavedGame().isResearched(type))
			{
				// more points if it's not researched
				addStat("STR_LIVE_ALIENS_RECOVERED", 1, from.getValue() * 2);
			}
			else
			{
				// 10 points for recovery
				addStat("STR_LIVE_ALIENS_RECOVERED", 1, 10);
			}

			@base.getStorageItems().addItem(type, 1);
			_manageContainment = @base.getAvailableContainment() - (@base.getUsedContainment() * _limitsEnforced) < 0;
		}
	}

	/**
	 * Reequips a craft after a mission.
	 * @param base Base to reequip from.
	 * @param craft Craft to reequip.
	 * @param vehicleItemsCanBeDestroyed Whether we can destroy the vehicles on the craft.
	 */
	void reequipCraft(Base @base, Craft craft, bool vehicleItemsCanBeDestroyed)
	{
		Dictionary<string, int> craftItems = craft.getItems().getContents();
		foreach (var i in craftItems)
		{
			int qty = @base.getStorageItems().getItem(i.Key);
			if (qty >= i.Value)
			{
				@base.getStorageItems().removeItem(i.Key, i.Value);
			}
			else
			{
				int missing = i.Value - qty;
				@base.getStorageItems().removeItem(i.Key, qty);
				craft.getItems().removeItem(i.Key, missing);
				var stat = new ReequipStat { item = i.Key, qty = missing, craft = craft.getName(_game.getLanguage()) };
				_missingItems.Add(stat);
			}
		}

		// Now let's see the vehicles
		var craftVehicles = new ItemContainer();
		foreach (var i in craft.getVehicles())
			craftVehicles.addItem(i.getRules().getType());
		// Now we know how many vehicles (separated by types) we have to read
		// Erase the current vehicles, because we have to reAdd them (cause we want to redistribute their ammo)
		//if (vehicleItemsCanBeDestroyed)
		craft.getVehicles().Clear();
		// Ok, now read those vehicles
		foreach (var i in craftVehicles.getContents())
		{
			int qty = @base.getStorageItems().getItem(i.Key);
			RuleItem tankRule = _game.getMod().getItem(i.Key, true);
			int size = 4;
			if (_game.getMod().getUnit(tankRule.getType()) != null)
			{
				size = _game.getMod().getArmor(_game.getMod().getUnit(tankRule.getType()).getArmor(), true).getSize();
				size *= size;
			}
			int canBeAdded = Math.Min(qty, i.Value);
			if (qty < i.Value)
			{ // missing tanks
				int missing = i.Value - qty;
				var stat = new ReequipStat { item = i.Key, qty = missing, craft = craft.getName(_game.getLanguage()) };
				_missingItems.Add(stat);
			}
			if (!tankRule.getCompatibleAmmo().Any())
			{ // so this tank does NOT require ammo
				for (int j = 0; j < canBeAdded; ++j)
					craft.getVehicles().Add(new Vehicle(tankRule, tankRule.getClipSize(), size));
				@base.getStorageItems().removeItem(i.Key, canBeAdded);
			}
			else
			{ // so this tank requires ammo
				RuleItem ammo = _game.getMod().getItem(tankRule.getCompatibleAmmo().First(), true);
				int ammoPerVehicle, clipSize;
				if (ammo.getClipSize() > 0 && tankRule.getClipSize() > 0)
				{
					clipSize = tankRule.getClipSize();
					ammoPerVehicle = clipSize / ammo.getClipSize();
				}
				else
				{
					clipSize = ammo.getClipSize();
					ammoPerVehicle = clipSize;
				}
				int baqty = @base.getStorageItems().getItem(ammo.getType()); // Ammo Quantity for this vehicle-type on the base
				if (baqty < i.Value * ammoPerVehicle)
				{ // missing ammo
					int missing = (i.Value * ammoPerVehicle) - baqty;
					var stat = new ReequipStat { item = ammo.getType(), qty = missing, craft = craft.getName(_game.getLanguage()) };
					_missingItems.Add(stat);
				}
				canBeAdded = Math.Min(canBeAdded, baqty / ammoPerVehicle);
				if (canBeAdded > 0)
				{
					for (int j = 0; j < canBeAdded; ++j)
					{
						craft.getVehicles().Add(new Vehicle(tankRule, clipSize, size));
						@base.getStorageItems().removeItem(ammo.getType(), ammoPerVehicle);
					}
					@base.getStorageItems().removeItem(i.Key, canBeAdded);
				}
			}
		}
	}

	/**
	 * Removes the association between the alien mission and the alien base,
	 * if one existed.
	 * @param am Pointer to the alien mission.
	 */
	void ClearAlienBase(AlienMission am, AlienBase ab)
	{
		if (am.getAlienBase() == ab)
		{
			am.setAlienBase(null);
		}
	}
}
