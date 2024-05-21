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
 * Window shown with all the facilities
 * available to build.
 */
internal class BuildFacilitiesState : State
{
    protected Base _base;
    protected State _state;
    protected Window _window;
    protected TextButton _btnOk;
    protected TextList _lstFacilities;
    protected Text _txtTitle;
    protected List<RuleBaseFacility> _facilities;

    /**
     * Initializes all the elements in the Build Facilities window.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param state Pointer to the base state to refresh.
     */
    internal BuildFacilitiesState(Base @base, State state)
    {
        _base = @base;
        _state = state;

        _screen = false;

        // Create objects
        _window = new Window(this, 128, 160, 192, 40, WindowPopup.POPUP_VERTICAL);
        _btnOk = new TextButton(112, 16, 200, 176);
        _lstFacilities = new TextList(104, 104, 200, 64);
        _txtTitle = new Text(118, 17, 197, 48);

        // Set palette
        setInterface("selectFacility");

        add(_window, "window", "selectFacility");
        add(_btnOk, "button", "selectFacility");
        add(_txtTitle, "text", "selectFacility");
        add(_lstFacilities, "list", "selectFacility");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK05.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_INSTALLATION"));

        _lstFacilities.setColumns(1, 104);
        _lstFacilities.setSelectable(true);
        _lstFacilities.setBackground(_window);
        _lstFacilities.setMargin(2);
        _lstFacilities.setWordWrap(true);
        _lstFacilities.setScrolling(true, 0);
        _lstFacilities.onMouseClick(lstFacilitiesClick);

        PopulateBuildList();
    }

    /**
     *
     */
    ~BuildFacilitiesState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();

    /**
     * Places the selected facility.
     * @param action Pointer to an action.
     */
    protected virtual void lstFacilitiesClick(Action _) =>
        _game.pushState(new PlaceFacilityState(_base, _facilities[(int)_lstFacilities.getSelectedRow()]));

    /**
     * Populates the build list from the current "available" facilities.
     */
    protected virtual void PopulateBuildList()
    {
        List<string> facilities = _game.getMod().getBaseFacilitiesList();
        foreach (var i in facilities)
        {
            RuleBaseFacility rule = _game.getMod().getBaseFacility(i);
            if (_game.getSavedGame().isResearched(rule.getRequirements()) && !rule.isLift())
                _facilities.Add(rule);
        }

        foreach (var i in _facilities)
        {
            _lstFacilities.addRow(1, tr(i.getType()));
        }
    }

    /**
     * The player can change the selected base
     * or change info on other screens.
     */
    internal override void init()
    {
	    _state.init();
	    base.init();
    }
}
