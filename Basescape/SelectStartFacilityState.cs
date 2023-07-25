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
internal class SelectStartFacilityState : BuildFacilitiesState
{
    Globe _globe;

    /**
     * Initializes all the elements in the Build Facilities window.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param state Pointer to the base state to refresh.
     * @param globe Pointer to the globe to refresh.
     */
    internal SelectStartFacilityState(Base @base, State state, Globe globe) : base(@base, state)
    {
        _globe = globe;

        _facilities = _game.getMod().getCustomBaseFacilities();

        _btnOk.setText(tr("STR_RESET"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(null, Options.keyCancel);

        _lstFacilities.onMouseClick(lstFacilitiesClick);

        populateBuildList();
    }

    /**
     *
     */
    ~SelectStartFacilityState() { }

    /**
     * Resets the base building.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        _base.getFacilities().Clear();
        _game.popState();
        _game.popState();
        _game.pushState(new PlaceLiftState(_base, _globe, true));
    }

    /**
     * Places the selected facility.
     * @param action Pointer to an action.
     */
    void lstFacilitiesClick(Action _) =>
        _game.pushState(new PlaceStartFacilityState(_base, this, _facilities[(int)_lstFacilities.getSelectedRow()]));

    /**
     * Populates the build list from the current "available" facilities.
     */
    void populateBuildList()
    {
        _lstFacilities.clearList();
        foreach (var i in _facilities)
        {
            _lstFacilities.addRow(1, tr(i.getType()));
        }
    }

    /**
     * Callback from PlaceStartFacilityState.
     * Removes placed facility from the list.
     */
    internal void facilityBuilt()
    {
        _facilities.RemoveAt((int)_lstFacilities.getSelectedRow());
        if (!_facilities.Any())
        {
            _game.popState();
            _game.popState(); // return to geoscape, force timer to start.
        }
        else
        {
            populateBuildList();
        }
    }
}
