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
 * Diary screen that shows all the
 * missions a soldier has.
 */
internal class SoldierDiaryOverviewState : State
{
    Base _base;
    uint _soldierId;
    SoldierInfoState _soldierInfoState;
    List<Soldier> _list;
    Window _window;
    TextButton _btnOk, _btnPrev, _btnNext, _btnKills, _btnMissions, _btnCommendations;
    Text _txtTitle, _txtMission, _txtRating, _txtDate;
    TextList _lstDiary;
    Soldier _soldier;

    /**
     * Initializes all the elements in the Soldier Diary screen.
     * @param base Pointer to the base to get info from.
     * @param soldierId ID of the selected soldier.
     * @param soldierInfoState Pointer to the Soldier Info screen.
     */
    internal SoldierDiaryOverviewState(Base @base, uint soldierId, SoldierInfoState soldierInfoState)
    {
        _base = @base;
        _soldierId = soldierId;
        _soldierInfoState = soldierInfoState;

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
        _btnKills = new TextButton(70, 16, 8, 176);
        _btnMissions = new TextButton(70, 16, 86, 176);
        _btnCommendations = new TextButton(70, 16, 164, 176);
        _btnOk = new TextButton(70, 16, 242, 176);
        _btnPrev = new TextButton(28, 14, 8, 8);
        _btnNext = new TextButton(28, 14, 284, 8);
        _txtTitle = new Text(310, 16, 5, 8);
        _txtMission = new Text(114, 9, 16, 36);
        _txtRating = new Text(102, 9, 120, 36);
        _txtDate = new Text(90, 9, 218, 36);
        _lstDiary = new TextList(288, 120, 8, 44);

        // Set palette
        setInterface("soldierDiary");

        add(_window, "window", "soldierDiary");
        add(_btnOk, "button", "soldierDiary");
        add(_btnKills, "button", "soldierDiary");
        add(_btnMissions, "button", "soldierDiary");
        add(_btnCommendations, "button", "soldierDiary");
        add(_btnPrev, "button", "soldierDiary");
        add(_btnNext, "button", "soldierDiary");
        add(_txtTitle, "text1", "soldierDiary");
        add(_txtMission, "text2", "soldierDiary");
        add(_txtRating, "text2", "soldierDiary");
        add(_txtDate, "text2", "soldierDiary");
        add(_lstDiary, "list", "soldierDiary");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK02.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _btnKills.setText(tr("STR_COMBAT"));
        _btnKills.onMouseClick(btnKillsClick);

        _btnMissions.setText(tr("STR_PERFORMANCE"));
        _btnMissions.onMouseClick(btnMissionsClick);

        _btnCommendations.setText(tr("STR_AWARDS"));
        _btnCommendations.onMouseClick(btnCommendationsClick);
        _btnCommendations.setVisible(_game.getMod().getCommendationsList().Any());

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

        _txtMission.setText(tr("STR_MISSION"));

        _txtRating.setText(tr("STR_RATING_UC"));

        _txtDate.setText(tr("STR_DATE_UC"));

        _lstDiary.setColumns(5, 104, 98, 30, 25, 35);
        _lstDiary.setSelectable(true);
        _lstDiary.setBackground(_window);
        _lstDiary.setMargin(8);
        _lstDiary.onMouseClick(lstDiaryInfoClick);

        init(); // Populate the list
    }

    /**
     *
     */
    ~SoldierDiaryOverviewState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        _soldierInfoState.setSoldierId(_soldierId);
        _game.popState();
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnKillsClick(Action _) =>
        _game.pushState(new SoldierDiaryPerformanceState(_base, _soldierId, this, SoldierDiaryDisplay.DIARY_KILLS));

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnMissionsClick(Action _) =>
        _game.pushState(new SoldierDiaryPerformanceState(_base, _soldierId, this, SoldierDiaryDisplay.DIARY_MISSIONS));

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCommendationsClick(Action _) =>
        _game.pushState(new SoldierDiaryPerformanceState(_base, _soldierId, this, SoldierDiaryDisplay.DIARY_COMMENDATIONS));

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

    /**
     * Shows the selected soldier's info.
     * @param action Pointer to an action.
     */
    void lstDiaryInfoClick(Action _)
    {
        int absoluteRowEntry = (int)_lstDiary.getSelectedRow();
        _game.pushState(new SoldierDiaryMissionState(_soldier, absoluteRowEntry));
    }

    /**
     *  Clears all the variables and reinitializes the list of medals for the soldier.
     *
     */
    internal override void init()
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
        _txtTitle.setText(_soldier.getName());
        _lstDiary.clearList();

        List<MissionStatistics> missionStatistics = _game.getSavedGame().getMissionStatistics();

        uint row = 0;
        foreach (var j in missionStatistics)
        {
            int missionId = j.id;
            bool wasOnMission = false;

            // See if this mission is part of the soldier's vector of missions
            foreach (var k in _soldier.getDiary().getMissionIdList())
            {
                if (missionId == k)
                {
                    wasOnMission = true;
                    break;
                }
            }
            if (!wasOnMission)
            {
                continue;
            }

            string ss = j.time.getYear().ToString();

            _lstDiary.addRow(5, j.getMissionName(_game.getLanguage()),
                                j.getRatingString(_game.getLanguage()),
                                j.time.getDayString(_game.getLanguage()),
                                tr(j.time.getMonthString()),
                                ss);
            row++;
        }
        if (row > 0 && _lstDiary.getScroll() >= row)
        {
            _lstDiary.scrollTo(0);
        }
    }

    /**
     * Set the soldier's Id.
     */
    internal void setSoldierId(uint soldier) =>
        _soldierId = soldier;
}
