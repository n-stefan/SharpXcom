﻿/*
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
 * Displays info on a detected mission site.
 */
internal class MissionDetectedState : State
{
    MissionSite _mission;
    GeoscapeState _state;
    Window _window;
    TextButton _btnIntercept, _btnCenter, _btnCancel;
    Text _txtTitle, _txtCity;

    /**
     * Initializes all the elements in the Mission Detected window.
     * @param game Pointer to the core game.
     * @param mission Pointer to the respective Mission Site.
     * @param state Pointer to the Geoscape.
     */
    internal MissionDetectedState(MissionSite mission, GeoscapeState state)
    {
        _mission = mission;
        _state = state;

        _screen = false;

        // Create objects
        _window = new Window(this, 256, 200, 0, 0, WindowPopup.POPUP_BOTH);
        _btnIntercept = new TextButton(200, 16, 28, 130);
        _btnCenter = new TextButton(200, 16, 28, 150);
        _btnCancel = new TextButton(200, 16, 28, 170);
        _txtTitle = new Text(246, 32, 5, 48);
        _txtCity = new Text(246, 17, 5, 80);

        // Set palette
        setInterface("terrorSite");

        add(_window, "window", "terrorSite");
        add(_btnIntercept, "button", "terrorSite");
        add(_btnCenter, "button", "terrorSite");
        add(_btnCancel, "button", "terrorSite");
        add(_txtTitle, "text", "terrorSite");
        add(_txtCity, "text", "terrorSite");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface(mission.getDeployment().getAlertBackground()));

        _btnIntercept.setText(tr("STR_INTERCEPT"));
        _btnIntercept.onMouseClick(btnInterceptClick);

        _btnCenter.setText(tr("STR_CENTER_ON_SITE_TIME_5_SECONDS"));
        _btnCenter.onMouseClick(btnCenterClick);

        _btnCancel.setText(tr("STR_CANCEL_UC"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setWordWrap(true);
        _txtTitle.setText(tr(mission.getDeployment().getAlertMessage()));

        _txtCity.setBig();
        _txtCity.setAlign(TextHAlign.ALIGN_CENTER);
        _txtCity.setText(tr(mission.getCity()));
    }

    /**
     *
     */
    ~MissionDetectedState() { }

    /**
     * Picks a craft to intercept the mission site.
     * @param action Pointer to an action.
     */
    void btnInterceptClick(Action _)
    {
        _state.timerReset();
        _state.getGlobe().center(_mission.getLongitude(), _mission.getLatitude());
        _game.pushState(new InterceptState(_state.getGlobe(), null, _mission));
    }

    /**
     * Centers on the mission site and returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCenterClick(Action _)
    {
        _state.timerReset();
        _state.getGlobe().center(_mission.getLongitude(), _mission.getLatitude());
        _game.popState();
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _) =>
        _game.popState();
}
