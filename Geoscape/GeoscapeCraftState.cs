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
 * Craft window that displays info about
 * a specific craft out on the Geoscape.
 */
internal class GeoscapeCraftState : State
{
    Craft _craft;
    Globe _globe;
    Waypoint _waypoint;
    Window _window;
    TextButton _btnBase, _btnTarget, _btnPatrol, _btnCancel;
    Text _txtTitle, _txtStatus, _txtBase, _txtSpeed, _txtMaxSpeed, _txtAltitude, _txtFuel, _txtDamage, _txtW1Name, _txtW1Ammo, _txtW2Name, _txtW2Ammo, _txtRedirect, _txtSoldier, _txtHWP;

    /**
     * Initializes all the elements in the Geoscape Craft window.
     * @param game Pointer to the core game.
     * @param craft Pointer to the craft to display.
     * @param globe Pointer to the Geoscape globe.
     * @param waypoint Pointer to the last UFO position (if redirecting the craft).
     */
    internal GeoscapeCraftState(Craft craft, Globe globe, Waypoint waypoint)
    {
        _craft = craft;
        _globe = globe;
        _waypoint = waypoint;

        _screen = false;

        // Create objects
        _window = new Window(this, 240, 184, 8, 8, WindowPopup.POPUP_BOTH);
        _btnBase = new TextButton(212, 12, 22, 124);
        _btnTarget = new TextButton(212, 12, 22, 140);
        _btnPatrol = new TextButton(212, 12, 22, 156);
        _btnCancel = new TextButton(212, 12, 22, 172);
        _txtTitle = new Text(210, 17, 32, 20);
        _txtStatus = new Text(210, 17, 32, 36);
        _txtBase = new Text(210, 9, 32, 52);
        _txtSpeed = new Text(210, 9, 32, 60);
        _txtMaxSpeed = new Text(210, 9, 32, 68);
        _txtAltitude = new Text(210, 9, 32, 76);
        _txtFuel = new Text(130, 9, 32, 84);
        _txtDamage = new Text(80, 9, 164, 84);
        _txtW1Name = new Text(130, 9, 32, 92);
        _txtW1Ammo = new Text(80, 9, 164, 92);
        _txtW2Name = new Text(130, 9, 32, 100);
        _txtW2Ammo = new Text(80, 9, 164, 100);
        _txtRedirect = new Text(230, 17, 13, 108);
        _txtSoldier = new Text(80, 9, 164, 68);
        _txtHWP = new Text(80, 9, 164, 76);

        // Set palette
        setInterface("geoCraft");

        add(_window, "window", "geoCraft");
        add(_btnBase, "button", "geoCraft");
        add(_btnTarget, "button", "geoCraft");
        add(_btnPatrol, "button", "geoCraft");
        add(_btnCancel, "button", "geoCraft");
        add(_txtTitle, "text1", "geoCraft");
        add(_txtStatus, "text1", "geoCraft");
        add(_txtBase, "text3", "geoCraft");
        add(_txtSpeed, "text3", "geoCraft");
        add(_txtMaxSpeed, "text3", "geoCraft");
        add(_txtAltitude, "text3", "geoCraft");
        add(_txtFuel, "text3", "geoCraft");
        add(_txtDamage, "text3", "geoCraft");
        add(_txtW1Name, "text3", "geoCraft");
        add(_txtW1Ammo, "text3", "geoCraft");
        add(_txtW2Name, "text3", "geoCraft");
        add(_txtW2Ammo, "text3", "geoCraft");
        add(_txtRedirect, "text3", "geoCraft");
        add(_txtSoldier, "text3", "geoCraft");
        add(_txtHWP, "text3", "geoCraft");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK12.SCR"));

        _btnBase.setText(tr("STR_RETURN_TO_BASE"));
        _btnBase.onMouseClick(btnBaseClick);

        _btnTarget.setText(tr("STR_SELECT_NEW_TARGET"));
        _btnTarget.onMouseClick(btnTargetClick);

        _btnPatrol.setText(tr("STR_PATROL"));
        _btnPatrol.onMouseClick(btnPatrolClick);

        _btnCancel.setText(tr("STR_CANCEL_UC"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setText(_craft.getName(_game.getLanguage()));

        _txtStatus.setWordWrap(true);
        string status;
        if (_waypoint != null)
        {
            status = tr("STR_INTERCEPTING_UFO").arg(_waypoint.getId());
        }
        else if (_craft.getLowFuel())
        {
            status = tr("STR_LOW_FUEL_RETURNING_TO_BASE");
        }
        else if (_craft.getMissionComplete())
        {
            status = tr("STR_MISSION_COMPLETE_RETURNING_TO_BASE");
        }
        else if (_craft.getDestination() == null)
        {
            status = tr("STR_PATROLLING");
        }
        else if (_craft.getDestination() == (Target)_craft.getBase())
        {
            status = tr("STR_RETURNING_TO_BASE");
        }
        else
        {
            Ufo u = (Ufo)_craft.getDestination();
            if (u != null)
            {
                if (_craft.isInDogfight())
                {
                    status = tr("STR_TAILING_UFO");
                }
                else if (u.getStatus() == UfoStatus.FLYING)
                {
                    status = tr("STR_INTERCEPTING_UFO").arg(u.getId());
                }
                else
                {
                    status = tr("STR_DESTINATION_UC_").arg(u.getName(_game.getLanguage()));
                }
            }
            else
            {
                status = tr("STR_DESTINATION_UC_").arg(_craft.getDestination().getName(_game.getLanguage()));
            }
        }
        _txtStatus.setText(tr("STR_STATUS_").arg(status));

        _txtBase.setText(tr("STR_BASE_UC").arg(_craft.getBase().getName()));

        int speed = _craft.getSpeed();
        if (_craft.isInDogfight())
        {
            Ufo ufo = (Ufo)_craft.getDestination();
            if (ufo != null)
            {
                speed = ufo.getSpeed();
            }
        }
        _txtSpeed.setText(tr("STR_SPEED_").arg(Unicode.formatNumber(speed)));

        _txtMaxSpeed.setText(tr("STR_MAXIMUM_SPEED_UC").arg(Unicode.formatNumber(_craft.getRules().getMaxSpeed())));

        string altitude = _craft.getAltitude() == "STR_GROUND" ? "STR_GROUNDED" : _craft.getAltitude();
        if (_craft.getRules().isWaterOnly() && !_globe.insideLand(_craft.getLongitude(), _craft.getLatitude()))
        {
            altitude = "STR_AIRBORNE";
        }
        _txtAltitude.setText(tr("STR_ALTITUDE_").arg(tr(altitude)));

        _txtFuel.setText(tr("STR_FUEL").arg(Unicode.formatPercentage(_craft.getFuelPercentage())));

        _txtDamage.setText(tr("STR_DAMAGE_UC_").arg(Unicode.formatPercentage(_craft.getDamagePercentage())));

        if (_craft.getRules().getWeapons() > 0 && _craft.getWeapons()[0] != null)
        {
            CraftWeapon w1 = _craft.getWeapons()[0];
            _txtW1Name.setText(tr("STR_WEAPON_ONE").arg(tr(w1.getRules().getType())));
            _txtW1Ammo.setText(tr("STR_ROUNDS_").arg(w1.getAmmo()));
        }
        else
        {
            _txtW1Name.setText(tr("STR_WEAPON_ONE").arg(tr("STR_NONE_UC")));
            _txtW1Ammo.setVisible(false);
        }

        if (_craft.getRules().getWeapons() > 1 && _craft.getWeapons()[1] != null)
        {
            CraftWeapon w2 = _craft.getWeapons()[1];
            _txtW2Name.setText(tr("STR_WEAPON_TWO").arg(tr(w2.getRules().getType())));
            _txtW2Ammo.setText(tr("STR_ROUNDS_").arg(w2.getAmmo()));
        }
        else
        {
            _txtW2Name.setText(tr("STR_WEAPON_TWO").arg(tr("STR_NONE_UC")));
            _txtW2Ammo.setVisible(false);
        }

        _txtRedirect.setBig();
        _txtRedirect.setAlign(TextHAlign.ALIGN_CENTER);
        _txtRedirect.setText(tr("STR_REDIRECT_CRAFT"));

        string ss11 = $"{tr("STR_SOLDIERS_UC")}>{Unicode.TOK_COLOR_FLIP}{_craft.getNumSoldiers()}";
        _txtSoldier.setText(ss11);

        string ss12 = $"{tr("STR_HWPS")}>{Unicode.TOK_COLOR_FLIP}{_craft.getNumVehicles()}";
        _txtHWP.setText(ss12);

        if (_waypoint == null)
        {
            _txtRedirect.setVisible(false);
        }
        else
        {
            _btnCancel.setText(tr("STR_GO_TO_LAST_KNOWN_UFO_POSITION"));
        }

        if (_craft.getLowFuel() || _craft.getMissionComplete())
        {
            _btnBase.setVisible(false);
            _btnTarget.setVisible(false);
            _btnPatrol.setVisible(false);
        }

        if (_craft.getRules().getSoldiers() == 0)
            _txtSoldier.setVisible(false);
        if (_craft.getRules().getVehicles() == 0)
            _txtHWP.setVisible(false);
    }

    /**
     *
     */
    ~GeoscapeCraftState() { }

    /**
     * Returns the craft back to its base.
     * @param action Pointer to an action.
     */
    void btnBaseClick(Engine.Action _)
    {
        _game.popState();
        _craft.returnToBase();
        _waypoint = null;
    }

    /**
     * Changes the craft's target.
     * @param action Pointer to an action.
     */
    void btnTargetClick(Engine.Action _)
    {
        _game.popState();
        _game.pushState(new SelectDestinationState(_craft, _globe));
        _waypoint = null;
    }

    /**
     * Sets the craft to patrol the current location.
     * @param action Pointer to an action.
     */
    void btnPatrolClick(Engine.Action _)
    {
        _game.popState();
        _craft.setDestination(null);
        _waypoint = null;
    }

    /**
     * Closes the window.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Engine.Action _)
    {
        // Go to the last known UFO position
        if (_waypoint != null)
        {
            _waypoint.setId(_game.getSavedGame().getId("STR_WAY_POINT"));
            _game.getSavedGame().getWaypoints().Add(_waypoint);
            _craft.setDestination(_waypoint);
        }
        // Cancel
        _game.popState();
    }
}
