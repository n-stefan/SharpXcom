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

namespace SharpXcom.Geoscape;

/**
 * Window which inform the player that a research project is finished.
 * Allow him to view information about the project (Ufopaedia).
 */
internal class ResearchCompleteState : State
{
    RuleResearch _research, _bonus;
    Window _window;
    TextButton _btnReport, _btnOk;
    Text _txtTitle, _txtResearch;

    /**
     * Initializes all the elements in the EndResearch screen.
     * @param game Pointer to the core game.
     * @param newResearch Pointer to the completed research (or 0, if the ufopedia article shouldn't popup again).
     * @param bonus Pointer to bonus unlocked research.
     * @param research Pointer to the research project.
     */
    internal ResearchCompleteState(RuleResearch newResearch, RuleResearch bonus, RuleResearch research)
    {
        _research = newResearch;
        _bonus = bonus;

        _screen = false;

        // Create objects
        _window = new Window(this, 230, 140, 45, 30, WindowPopup.POPUP_BOTH);
        _btnOk = new TextButton(80, 16, 64, 146);
        _btnReport = new TextButton(80, 16, 176, 146);
        _txtTitle = new Text(230, 17, 45, 70);
        _txtResearch = new Text(230, 32, 45, 96);

        // Set palette
        setInterface("geoResearchComplete");

        add(_window, "window", "geoResearchComplete");
        add(_btnOk, "button", "geoResearchComplete");
        add(_btnReport, "button", "geoResearchComplete");
        add(_txtTitle, "text1", "geoResearchComplete");
        add(_txtResearch, "text2", "geoResearchComplete");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK05.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _btnReport.setText(tr("STR_VIEW_REPORTS"));
        _btnReport.onMouseClick(btnReportClick);
        _btnReport.onKeyboardPress(btnReportClick, Options.keyOk);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_RESEARCH_COMPLETED"));

        _txtResearch.setAlign(TextHAlign.ALIGN_CENTER);
        _txtResearch.setBig();
        _txtResearch.setWordWrap(true);
        if (research != null)
        {
            _txtResearch.setText(tr(research.getName()));
        }
    }

    /**
     * return to the previous screen
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();

    /**
     * open the Ufopaedia to the entry about the Research.
     * @param action Pointer to an action.
     */
    void btnReportClick(Action _)
    {
        _game.popState();
        string name;
        string bonusName;
        if (_bonus != null)
        {
            if (string.IsNullOrEmpty(_bonus.getLookup()))
                bonusName = _bonus.getName();
            else
                bonusName = _bonus.getLookup();
            Ufopaedia.Ufopaedia.openArticle(_game, bonusName);
        }
        if (_research != null)
        {
            if (string.IsNullOrEmpty(_research.getLookup()))
                name = _research.getName();
            else
                name = _research.getLookup();
            Ufopaedia.Ufopaedia.openArticle(_game, name);
        }
    }
}
