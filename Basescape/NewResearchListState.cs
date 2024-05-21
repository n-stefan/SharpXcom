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
 * Window which displays possible research projects.
 */
internal class NewResearchListState : State
{
    Base _base;
    Window _window;
    TextButton _btnOK;
    Text _txtTitle;
    TextList _lstResearch;
    List<RuleResearch> _projects;

    /**
     * Initializes all the elements in the Research list screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     */
    internal NewResearchListState(Base @base)
    {
        _base = @base;

        _screen = false;

        _window = new Window(this, 230, 140, 45, 30, WindowPopup.POPUP_BOTH);
        _btnOK = new TextButton(214, 16, 53, 146);
        _txtTitle = new Text(214, 16, 53, 38);
        _lstResearch = new TextList(198, 88, 53, 54);

        // Set palette
        setInterface("selectNewResearch");

        add(_window, "window", "selectNewResearch");
        add(_btnOK, "button", "selectNewResearch");
        add(_txtTitle, "text", "selectNewResearch");
        add(_lstResearch, "list", "selectNewResearch");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK05.SCR"));

        _btnOK.setText(tr("STR_OK"));
        _btnOK.onMouseClick(btnOKClick);
        _btnOK.onKeyboardPress(btnOKClick, Options.keyCancel);

        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_NEW_RESEARCH_PROJECTS"));

        _lstResearch.setColumns(1, 190);
        _lstResearch.setSelectable(true);
        _lstResearch.setBackground(_window);
        _lstResearch.setMargin(8);
        _lstResearch.setAlign(TextHAlign.ALIGN_CENTER);
        _lstResearch.onMouseClick(onSelectProject);
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOKClick(Action _) =>
        _game.popState();

    /**
     * Selects the RuleResearch to work on.
     * @param action Pointer to an action.
     */
    void onSelectProject(Action _) =>
        _game.pushState(new ResearchInfoState(_base, _projects[(int)_lstResearch.getSelectedRow()]));

    /**
     * Initializes the screen (fills the list).
     */
    internal override void init()
    {
	    base.init();
	    fillProjectList();
    }

    /**
     * Fills the list with possible ResearchProjects.
     */
    void fillProjectList()
    {
	    _projects.Clear();
	    _lstResearch.clearList();
	    // Note: this is the *only* place where this method is called with considerDebugMode = true
	    _game.getSavedGame().getAvailableResearchProjects(_projects, _game.getMod() , _base, true);
	    var it = 0;
	    while (it < _projects.Count)
	    {
		    // EXPLANATION
		    // -----------
		    // Projects with "requires" can only be discovered/researched indirectly
		    //  - this is because we can't reliably determine if they are unlocked or not
		    // Example:
		    //  - Alien Origins + Alien Leader => ALIEN_LEADER_PLUS is discovered
		    //  - Alien Leader + Alien Origins => ALIEN_LEADER_PLUS is NOT discovered (you need to research another alien leader/commander)
		    // If we wanted to allow also direct research of projects with "requires",
		    // we would need to implement a slightly more complicated unlocking algorithm
		    // and more importantly, we would need to remember the list of unlocked topics
		    // in the save file (currently this is not done, the list is calculated on-the-fly).
		    // Summary:
		    //  - it would be possible to remove this condition, but more refactoring would be needed
		    //  - for now, handling "requires" via zero-cost helpers (e.g. STR_LEADER_PLUS)... is enough
		    if (!_projects[it].getRequirements().Any())
		    {
			    _lstResearch.addRow(1, tr(_projects[it].getName()));
			    ++it;
		    }
		    else
		    {
			    _projects.RemoveAt(it);
		    }
	    }
    }
}
