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
 * Screen shown when the player has to
 * place the access lift of a base.
 */
internal class PlaceLiftState : State
{
    Base _base;
    Globe _globe;
    bool _first;
    BaseView _view;
    Text _txtTitle;
    RuleBaseFacility _lift;

    /**
     * Initializes all the elements in the Place Lift screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param globe Pointer to the Geoscape globe.
     * @param first Is this a custom starting base?
     */
    internal PlaceLiftState(Base @base, Globe globe, bool first)
    {
        _base = @base;
        _globe = globe;
        _first = first;

        // Create objects
        _view = new BaseView(192, 192, 0, 8);
        _txtTitle = new Text(320, 9, 0, 0);

        // Set palette
        setInterface("placeFacility");

        add(_view, "baseView", "basescape");
        add(_txtTitle, "text", "placeFacility");

        centerAllSurfaces();

        // Set up objects
        _view.setTexture(_game.getMod().getSurfaceSet("BASEBITS.PCK"));
        _view.setBase(_base);
        foreach (var i in _game.getMod().getBaseFacilitiesList())
        {
            if (_game.getMod().getBaseFacility(i).isLift())
            {
                _lift = _game.getMod().getBaseFacility(i);
                break;
            }
        }
        _view.setSelectable(_lift.getSize());
        _view.onMouseClick(viewClick);

        _txtTitle.setText(tr("STR_SELECT_POSITION_FOR_ACCESS_LIFT"));
    }

    /**
     *
     */
    ~PlaceLiftState() { }

    /**
     * Processes clicking on facilities.
     * @param action Pointer to an action.
     */
    void viewClick(Action _)
    {
        BaseFacility fac = new BaseFacility(_lift, _base);
        fac.setX(_view.getGridX());
        fac.setY(_view.getGridY());
        _base.getFacilities().Add(fac);
        _game.popState();
        BasescapeState bState = new BasescapeState(_base, _globe);
        _game.getSavedGame().setSelectedBase((uint)(_game.getSavedGame().getBases().Count - 1));
        _game.pushState(bState);
        if (_first)
        {
            _game.pushState(new SelectStartFacilityState(_base, bState, _globe));
        }
    }
}
