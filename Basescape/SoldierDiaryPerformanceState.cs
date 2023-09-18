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

enum SoldierDiaryDisplay { DIARY_KILLS, DIARY_MISSIONS, DIARY_COMMENDATIONS };

/**
 * Diary screen that lists soldier totals.
 */
internal class SoldierDiaryPerformanceState : State
{
    Base _base;
    uint _soldierId;
    SoldierDiaryOverviewState _soldierDiaryOverviewState;
    SoldierDiaryDisplay _display;
    int _lastScrollPos;
    List<Soldier> _list;
    Window _window;
    TextButton _btnOk, _btnPrev, _btnNext, _btnKills, _btnMissions, _btnCommendations;
    Text _txtTitle, _txtMedalName, _txtMedalLevel, _txtMedalInfo;
    TextList _lstPerformance, _lstKillTotals, _lstMissionTotals, _lstCommendations;
    List<Surface> _commendations, _commendationDecorations;
    TextButton _group;
    List<string> _commendationsListEntry;
    Soldier _soldier;
    SurfaceSet _commendationSprite, _commendationDecoration;

    /**
     * Initializes all the elements in the Soldier Diary Totals screen.
     * @param base Pointer to the base to get info from.
     * @param soldier ID of the selected soldier.
     * @param soldierInfoState Pointer to the Soldier Diary screen.
     * @param display Type of totals to display.
     */
    internal SoldierDiaryPerformanceState(Base @base, uint soldierId, SoldierDiaryOverviewState soldierDiaryOverviewState, SoldierDiaryDisplay display)
    {
        _base = @base;
        _soldierId = soldierId;
        _soldierDiaryOverviewState = soldierDiaryOverviewState;
        _display = display;
        _lastScrollPos = 0;

        if (_base == null)
        {
            _list = _game.getSavedGame().getDeadSoldiers();
        }
        else
        {
            _list = _base.getSoldiers();
        }

        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        _btnPrev = new TextButton(28, 14, 8, 8);
        _btnNext = new TextButton(28, 14, 284, 8);
        _btnKills = new TextButton(70, 16, 8, 176);
        _btnMissions = new TextButton(70, 16, 86, 176);
        _btnCommendations = new TextButton(70, 16, 164, 176);
        _btnOk = new TextButton(70, 16, 242, 176);
        _txtTitle = new Text(310, 16, 5, 8);
        _lstPerformance = new TextList(288, 128, 8, 28);
        _lstKillTotals = new TextList(302, 9, 8, 164);
        _lstMissionTotals = new TextList(302, 9, 8, 164);
        // Commendation stats
        _txtMedalName = new Text(120, 18, 16, 36);
        _txtMedalLevel = new Text(120, 18, 186, 36);
        _txtMedalInfo = new Text(280, 32, 20, 135);
        _lstCommendations = new TextList(240, 80, 48, 52);
        for (int i = 0; i != 10; ++i)
        {
            _commendations.Add(new Surface(31, 8, 16, 52 + 8 * i));
            _commendationDecorations.Add(new Surface(31, 8, 16, 52 + 8 * i));
        }

        // Set palette
        setInterface("soldierDiaryPerformance");

        add(_window, "window", "soldierDiaryPerformance");
        add(_btnOk, "button", "soldierDiaryPerformance");
        add(_btnKills, "button", "soldierDiaryPerformance");
        add(_btnMissions, "button", "soldierDiaryPerformance");
        add(_btnCommendations, "button", "soldierDiaryPerformance");
        add(_btnPrev, "button", "soldierDiaryPerformance");
        add(_btnNext, "button", "soldierDiaryPerformance");
        add(_txtTitle, "text1", "soldierDiaryPerformance");
        add(_lstPerformance, "list", "soldierDiaryPerformance");
        add(_lstKillTotals, "text2", "soldierDiaryPerformance");
        add(_lstMissionTotals, "text2", "soldierDiaryPerformance");
        add(_txtMedalName, "text2", "soldierDiaryPerformance");
        add(_txtMedalLevel, "text2", "soldierDiaryPerformance");
        add(_txtMedalInfo, "text2", "soldierDiaryPerformance");
        add(_lstCommendations, "list", "soldierDiaryPerformance");
        for (int i = 0; i != 10; ++i)
        {
            add(_commendations[i]);
            add(_commendationDecorations[i]);
        }

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK02.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _btnKills.setText(tr("STR_COMBAT"));
        _btnKills.onMouseClick(btnKillsToggle);

        _btnMissions.setText(tr("STR_PERFORMANCE"));
        _btnMissions.onMouseClick(btnMissionsToggle);

        _btnCommendations.setText(tr("STR_AWARDS"));
        _btnCommendations.onMouseClick(btnCommendationsToggle);

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

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);

        // Text is decided in init()
        _lstPerformance.setColumns(2, 273, 15);
        _lstPerformance.setDot(true);

        _lstKillTotals.setColumns(4, 72, 72, 72, 86);

        _lstMissionTotals.setColumns(4, 72, 72, 72, 86);

        _txtMedalName.setText(tr("STR_MEDAL_NAME"));

        _txtMedalLevel.setText(tr("STR_MEDAL_DECOR_LEVEL"));

        _txtMedalInfo.setWordWrap(true);

        _lstCommendations.setColumns(2, 138, 100);
        _lstCommendations.setSelectable(true);
        _lstCommendations.setBackground(_window);
        _lstCommendations.onMouseOver(lstInfoMouseOver);
        _lstCommendations.onMouseOut(lstInfoMouseOut);
        _lstCommendations.onMousePress(handle);

        if (_display == SoldierDiaryDisplay.DIARY_KILLS)
        {
            _group = _btnKills;
        }
        else if (_display == SoldierDiaryDisplay.DIARY_MISSIONS)
        {
            _group = _btnMissions;
        }
        else if (_display == SoldierDiaryDisplay.DIARY_COMMENDATIONS)
        {
            _group = _btnCommendations;
        }
        _btnKills.setGroup(_group);
        _btnMissions.setGroup(_group);
        _btnCommendations.setGroup(_group);

        init(); // Populate the list
    }

    /**
     *
     */
    ~SoldierDiaryPerformanceState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        _soldierDiaryOverviewState.setSoldierId(_soldierId);
        _game.popState();
    }

    /**
     * Display Kills totals.
     */
    void btnKillsToggle(Action _)
    {
        _display = SoldierDiaryDisplay.DIARY_KILLS;
        init();
    }

    /**
     * Display Missions totals.
     */
    void btnMissionsToggle(Action _)
    {
        _display = SoldierDiaryDisplay.DIARY_MISSIONS;
        init();
    }

    /**
     * Display Commendations.
     */
    void btnCommendationsToggle(Action _)
    {
        _display = SoldierDiaryDisplay.DIARY_COMMENDATIONS;
        init();
    }

    /**
     * Goes to the next soldier.
     * @param action Pointer to an action.
     */
    void btnNextClick(Action _)
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
    void btnPrevClick(Action _)
    {
        if (_soldierId == 0)
            _soldierId = (uint)(_list.Count - 1);
        else
            _soldierId--;
        init();
    }

    /*
     *
     */
    void lstInfoMouseOver(Action _)
    {
        uint _sel;
        _sel = _lstCommendations.getSelectedRow();

        if (!_commendationsListEntry.Any() || _sel > _commendationsListEntry.Count - 1)
        {
            _txtMedalInfo.setText(string.Empty);
        }
        else
        {
            _txtMedalInfo.setText(_commendationsListEntry[(int)_sel]);
        }
    }

    /*
     *  Clears the Medal information
     */
    void lstInfoMouseOut(Action _) =>
        _txtMedalInfo.setText(string.Empty);

    /**
     *  Clears all the variables and reinitializes the list of kills or missions for the soldier.
     */
    protected override void init()
    {
        base.init();
        // Clear sprites
        for (int i = 0; i != 10; ++i)
        {
            _commendations[i].clear();
            _commendationDecorations[i].clear();
        }
        // Reset scroll depth for lists
        _lstPerformance.scrollTo(0);
        _lstKillTotals.scrollTo(0);
        _lstMissionTotals.scrollTo(0);
        _lstCommendations.scrollTo(0);
        _lastScrollPos = 0;
        _lstPerformance.setVisible(_display != SoldierDiaryDisplay.DIARY_COMMENDATIONS);
        // Set visibility for kills
        _lstKillTotals.setVisible(_display == SoldierDiaryDisplay.DIARY_KILLS);
        // Set visibility for missions
        _lstMissionTotals.setVisible(_display == SoldierDiaryDisplay.DIARY_MISSIONS);
        // Set visibility for commendations
        _txtMedalName.setVisible(_display == SoldierDiaryDisplay.DIARY_COMMENDATIONS);
        _txtMedalLevel.setVisible(_display == SoldierDiaryDisplay.DIARY_COMMENDATIONS);
        _txtMedalInfo.setVisible(_display == SoldierDiaryDisplay.DIARY_COMMENDATIONS);
        _lstCommendations.setVisible(_display == SoldierDiaryDisplay.DIARY_COMMENDATIONS);
        _btnCommendations.setVisible(_game.getMod().getCommendationsList().Any());

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
        _lstKillTotals.clearList();
        _lstMissionTotals.clearList();
        _commendationsListEntry.Clear();
        _txtTitle.setText(_soldier.getName());
        _lstPerformance.clearList();
        _lstCommendations.clearList();
        if (_display == SoldierDiaryDisplay.DIARY_KILLS)
        {
            Dictionary<string, int>[] mapArray = { _soldier.getDiary().getAlienRaceTotal(), _soldier.getDiary().getAlienRankTotal(), _soldier.getDiary().getWeaponTotal() };
            string[] titleArray = { "STR_NEUTRALIZATIONS_BY_RACE", "STR_NEUTRALIZATIONS_BY_RANK", "STR_NEUTRALIZATIONS_BY_WEAPON" };

            for (int i = 0; i != 3; ++i)
            {
                _lstPerformance.addRow(1, tr(titleArray[i]));
                _lstPerformance.setRowColor(_lstPerformance.getRows() - 1, _lstPerformance.getSecondaryColor());
                foreach (var j in mapArray[i])
                {
                    string ss = j.Value.ToString();
                    _lstPerformance.addRow(2, tr(j.Key), ss);
                }
                if (i != 2)
                {
                    _lstPerformance.addRow(1, string.Empty);
                }
            }

            if (_soldier.getCurrentStats().psiSkill > 0 || (Options.psiStrengthEval && _game.getSavedGame().isResearched(_game.getMod().getPsiRequirements())))
            {
                _lstKillTotals.addRow(4, tr("STR_KILLS").arg(_soldier.getDiary().getKillTotal()),
                                            tr("STR_STUNS").arg(_soldier.getDiary().getStunTotal()),
                                            tr("STR_DIARY_ACCURACY").arg(_soldier.getDiary().getAccuracy()),
                                            tr("STR_MINDCONTROLS").arg(_soldier.getDiary().getControlTotal()));
            }
            else
            {
                _lstKillTotals.addRow(3, tr("STR_KILLS").arg(_soldier.getDiary().getKillTotal()),
                                            tr("STR_STUNS").arg(_soldier.getDiary().getStunTotal()),
                                            tr("STR_DIARY_ACCURACY").arg(_soldier.getDiary().getAccuracy()));
            }

        }
        else if (_display == SoldierDiaryDisplay.DIARY_MISSIONS)
        {
            Dictionary<string, int>[] mapArray = { _soldier.getDiary().getRegionTotal(_game.getSavedGame().getMissionStatistics()), _soldier.getDiary().getTypeTotal(_game.getSavedGame().getMissionStatistics()), _soldier.getDiary().getUFOTotal(_game.getSavedGame().getMissionStatistics()) };
            string[] titleArray = { "STR_MISSIONS_BY_LOCATION", "STR_MISSIONS_BY_TYPE", "STR_MISSIONS_BY_UFO" };

            for (int i = 0; i != 3; ++i)
            {
                _lstPerformance.addRow(1, tr(titleArray[i]));
                _lstPerformance.setRowColor(_lstPerformance.getRows() - 1, _lstPerformance.getSecondaryColor());
                foreach (var j in mapArray[i])
                {
                    if (j.Key == "NO_UFO") continue;
                    string ss = j.Value.ToString();
                    _lstPerformance.addRow(2, tr(j.Key), ss);
                }
                if (i != 2)
                {
                    _lstPerformance.addRow(1, string.Empty);
                }
            }

            _lstMissionTotals.addRow(4, tr("STR_MISSIONS").arg(_soldier.getDiary().getMissionTotal()),
                                        tr("STR_WINS").arg(_soldier.getDiary().getWinTotal(_game.getSavedGame().getMissionStatistics())),
                                        tr("STR_SCORE_VALUE").arg(_soldier.getDiary().getScoreTotal(_game.getSavedGame().getMissionStatistics())),
                                        tr("STR_DAYS_WOUNDED").arg(_soldier.getDiary().getDaysWoundedTotal()));
        }
        else if (_display == SoldierDiaryDisplay.DIARY_COMMENDATIONS && _game.getMod().getCommendationsList().Any())
        {
            foreach (var i in _soldier.getDiary().getSoldierCommendations())
            {
                RuleCommendations commendation = _game.getMod().getCommendation(i.getType());
                if (i.getNoun() != "noNoun")
                {
                    _lstCommendations.addRow(2, tr(i.getType()).arg(tr(i.getNoun())), tr(i.getDecorationDescription()));
                    _commendationsListEntry.Add(tr(commendation.getDescription()).arg(tr(i.getNoun())));
                }
                else
                {
                    _lstCommendations.addRow(2, tr(i.getType()), tr(i.getDecorationDescription()));
                    _commendationsListEntry.Add(tr(commendation.getDescription()));
                }
            }
            drawSprites();
        }
    }

    /**
     * Draws sprites
     *
     */
    void drawSprites()
    {
        if (_display != SoldierDiaryDisplay.DIARY_COMMENDATIONS) return;

        // Commendation sprites
        _commendationSprite = _game.getMod().getSurfaceSet("Commendations");
        _commendationDecoration = _game.getMod().getSurfaceSet("CommendationDecorations");

        // Clear sprites
        for (int i = 0; i != 10; ++i)
        {
            _commendations[i].clear();
            _commendationDecorations[i].clear();
        }

        int vectorIterator = 0; // Where we are currently located in the vector
        int scrollDepth = (int)_lstCommendations.getScroll(); // So we know where to start

        foreach (var i in _list[(int)_soldierId].getDiary().getSoldierCommendations())
        {
            RuleCommendations commendation = _game.getMod().getCommendation(i.getType());
            // Skip commendations that are not visible in the textlist
            if (vectorIterator < scrollDepth || vectorIterator - scrollDepth >= (int)_commendations.Count)
            {
                vectorIterator++;
                continue;
            }

            int _sprite = commendation.getSprite();
            int _decorationSprite = i.getDecorationLevelInt();

            // Handle commendation sprites
            _commendationSprite.getFrame(_sprite).setX(0);
            _commendationSprite.getFrame(_sprite).setY(0);
            _commendationSprite.getFrame(_sprite).blit(_commendations[vectorIterator - scrollDepth]);

            // Handle commendation decoration sprites
            if (_decorationSprite != 0)
            {
                _commendationDecoration.getFrame(_decorationSprite).setX(0);
                _commendationDecoration.getFrame(_decorationSprite).setY(0);
                _commendationDecoration.getFrame(_decorationSprite).blit(_commendationDecorations[vectorIterator - scrollDepth]);
            }

            vectorIterator++;
        }
    }

    /**
     * Runs state functionality every cycle.
     * Used to update sprite vector
     */
    protected override void think()
    {
	    base.think();

	    if ((uint)_lastScrollPos != _lstCommendations.getScroll())
	    {
		    drawSprites();
		    _lastScrollPos = (int)_lstCommendations.getScroll();
	    }
    }
}
