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
 * Diary window that shows
 * mission details for a soldier.
 */
internal class SoldierDiaryMissionState : State
{
    Soldier _soldier;
    int _rowEntry;
    Window _window;
    TextButton _btnOk, _btnPrev, _btnNext;
    Text _txtTitle, _txtUFO, _txtScore, _txtKills, _txtLocation, _txtRace, _txtDaylight, _txtDaysWounded;
    Text _txtNoRecord;
    TextList _lstKills;

    /**
     * Initializes all the elements in the Soldier Diary Mission window.
     * @param soldier Pointer to the selected soldier.
     * @param rowEntry number to get mission info from.
     */
    internal SoldierDiaryMissionState(Soldier soldier, int rowEntry)
    {
        _soldier = soldier;
        _rowEntry = rowEntry;

        _screen = false;

        // Create objects
        _window = new Window(this, 300, 128, 10, 36, WindowPopup.POPUP_HORIZONTAL);
        _btnOk = new TextButton(240, 16, 40, 140);
        _btnPrev = new TextButton(28, 14, 18, 44);
        _btnNext = new TextButton(28, 14, 274, 44);
        _txtTitle = new Text(262, 9, 29, 44);
        _txtUFO = new Text(262, 9, 29, 52);
        _txtScore = new Text(180, 9, 29, 68);
        _txtKills = new Text(120, 9, 169, 68);
        _txtLocation = new Text(180, 9, 29, 76);
        _txtRace = new Text(120, 9, 169, 76);
        _txtDaylight = new Text(120, 9, 169, 84);
        _txtDaysWounded = new Text(180, 9, 29, 84);
        _txtNoRecord = new Text(240, 9, 29, 100);
        _lstKills = new TextList(270, 32, 20, 100);

        // Set palette
        setInterface("soldierDiaryMission");

        add(_window, "window", "soldierDiaryMission");
        add(_btnOk, "button", "soldierDiaryMission");
        add(_btnPrev, "button", "soldierDiaryMission");
        add(_btnNext, "button", "soldierDiaryMission");
        add(_txtTitle, "text", "soldierDiaryMission");
        add(_txtUFO, "text", "soldierDiaryMission");
        add(_txtScore, "text", "soldierDiaryMission");
        add(_txtKills, "text", "soldierDiaryMission");
        add(_txtLocation, "text", "soldierDiaryMission");
        add(_txtRace, "text", "soldierDiaryMission");
        add(_txtDaylight, "text", "soldierDiaryMission");
        add(_txtDaysWounded, "text", "soldierDiaryMission");
        add(_txtNoRecord, "text", "soldierDiaryMission");
        add(_lstKills, "list", "soldierDiaryMission");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK16.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _btnPrev.setText("<<");
        _btnPrev.onMouseClick(btnPrevClick);
        _btnPrev.onKeyboardPress(btnPrevClick, Options.keyBattleNextUnit);

        _btnNext.setText(">>");
        _btnNext.onMouseClick(btnNextClick);
        _btnNext.onKeyboardPress(btnNextClick, Options.keyBattlePrevUnit);

        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);

        _txtUFO.setAlign(TextHAlign.ALIGN_CENTER);

        _lstKills.setColumns(3, 60, 110, 100);

        init(); // Populate the list
    }

    /**
     *
     */
    ~SoldierDiaryMissionState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();

    /**
     * Goes to the previous mission.
     * @param action Pointer to an action.
     */
    void btnPrevClick(Action _)
    {
        if (_rowEntry == 0)
            _rowEntry = _soldier.getDiary().getMissionTotal() - 1;
        else
            _rowEntry--;
        init();
    }

    /**
     * Goes to the next mission.
     * @param action Pointer to an action.
     */
    void btnNextClick(Action _)
    {
        _rowEntry++;
        if (_rowEntry >= _soldier.getDiary().getMissionTotal())
            _rowEntry = 0;
        init();
    }

    /**
     *  Clears all the variables and reinitializes the stats for the mission.
     *
     */
    protected override void init()
    {
	    base.init();
	    if (!_soldier.getDiary().getMissionIdList().Any())
	    {
		    _game.popState();
		    return;
	    }
	    List<MissionStatistics> missionStatistics = _game.getSavedGame().getMissionStatistics();
	    int missionId = _soldier.getDiary().getMissionIdList()[_rowEntry];
	    if (missionId > missionStatistics.Count)
	    {
		    missionId = 0;
	    }
	    int daysWounded = missionStatistics[missionId].injuryList[_soldier.getId()];

	    _lstKills.clearList();
        _txtTitle.setText(tr(missionStatistics[missionId].type));
	    if (missionStatistics[missionId].isUfoMission())
	    {
            _txtUFO.setText(tr(missionStatistics[missionId].ufo));
	    }
        _txtUFO.setVisible(missionStatistics[missionId].isUfoMission());
        _txtScore.setText(tr("STR_SCORE_VALUE").arg(missionStatistics[missionId].score));
        _txtLocation.setText(tr("STR_LOCATION").arg(tr(missionStatistics[missionId].getLocationString())));
        _txtRace.setText(tr("STR_RACE_TYPE").arg(tr(missionStatistics[missionId].alienRace)));
        _txtRace.setVisible(missionStatistics[missionId].alienRace != "STR_UNKNOWN");
        _txtDaylight.setText(tr("STR_DAYLIGHT_TYPE").arg(tr(missionStatistics[missionId].getDaylightString())));
	    _txtDaysWounded.setText(tr("STR_DAYS_WOUNDED").arg(daysWounded));
	    _txtDaysWounded.setVisible(daysWounded != 0);

	    int kills = 0;
	    bool stunOrKill = false;

	    foreach (var i in _soldier.getDiary().getKills())
	    {
		    if ((uint)i.mission != missionId) continue;

		    switch (i.status)
		    {
		        case UnitStatus.STATUS_DEAD:
			        kills++;
                    //Fall-through
                    goto case UnitStatus.STATUS_UNCONSCIOUS;
                case UnitStatus.STATUS_UNCONSCIOUS:
		        case UnitStatus.STATUS_PANICKING:
		        case UnitStatus.STATUS_TURNING:
			        stunOrKill = true;
                    break;
		        default:
			        break;
		    }

		    _lstKills.addRow(3, tr(i.getKillStatusString()),
							    i.getUnitName(_game.getLanguage()),
							    tr(i.weapon));
	    }

	    _txtNoRecord.setAlign(TextHAlign.ALIGN_CENTER);
	    _txtNoRecord.setText(tr("STR_NO_RECORD"));
	    _txtNoRecord.setVisible(!stunOrKill);

	    _txtKills.setText(tr("STR_KILLS").arg(kills));
    }
}
