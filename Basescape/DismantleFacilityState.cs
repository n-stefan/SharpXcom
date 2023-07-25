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
 * Window shown when the player tries to
 * dismantle a facility.
 */
internal class DismantleFacilityState : State
{
    Base _base;
    BaseView _view;
    BaseFacility _fac;
    Window _window;
    TextButton _btnOk, _btnCancel;
    Text _txtTitle, _txtFacility;

    /**
     * Initializes all the elements in a Dismantle Facility window.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param view Pointer to the baseview to update.
     * @param fac Pointer to the facility to dismantle.
     */
    internal DismantleFacilityState(Base @base, BaseView view, BaseFacility fac)
    {
        _base = @base;
        _view = view;
        _fac = fac;

        _screen = false;

        // Create objects
        _window = new Window(this, 152, 80, 20, 60);
        _btnOk = new TextButton(44, 16, 36, 115);
        _btnCancel = new TextButton(44, 16, 112, 115);
        _txtTitle = new Text(142, 9, 25, 75);
        _txtFacility = new Text(142, 9, 25, 85);

        // Set palette
        setInterface("dismantleFacility");

        add(_window, "window", "dismantleFacility");
        add(_btnOk, "button", "dismantleFacility");
        add(_btnCancel, "button", "dismantleFacility");
        add(_txtTitle, "text", "dismantleFacility");
        add(_txtFacility, "text", "dismantleFacility");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK13.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

        _btnCancel.setText(tr("STR_CANCEL_UC"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_DISMANTLE"));

        _txtFacility.setAlign(TextHAlign.ALIGN_CENTER);
        _txtFacility.setText(tr(_fac.getRules().getType()));
    }

    /**
     *
     */
    ~DismantleFacilityState() { }

    /**
     * Dismantles the facility and returns
     * to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        if (!_fac.getRules().isLift())
        {
            // Give refund if this is an unstarted, queued build.
            if (_fac.getBuildTime() > _fac.getRules().getBuildTime())
            {
                _game.getSavedGame().setFunds(_game.getSavedGame().getFunds() + _fac.getRules().getBuildCost());
            }

            foreach (var i in _base.getFacilities())
            {
                if (i == _fac)
                {
                    _base.getFacilities().Remove(i);
                    _view.resetSelectedFacility();
                    _fac = null;
                    if (Options.allowBuildingQueue) _view.reCalcQueuedBuildings();
                    break;
                }
            }
        }
        // Remove whole base if it's the access lift
        else
        {
            foreach (var i in _game.getSavedGame().getBases())
            {
                if (i == _base)
                {
                    _game.getSavedGame().getBases().Remove(i);
                    _base = null;
                    break;
                }
            }
        }
        _game.popState();
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _) =>
        _game.popState();
}
