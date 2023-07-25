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
 * along with OpenXcom.  If not, see <http:///www.gnu.org/licenses/>.
 */

namespace SharpXcom.Geoscape;

/**
 * Window that allows the player
 * to confirm a craft's new destination.
 */
internal class ConfirmDestinationState : State
{
    Craft _craft;
    Target _target;
    Window _window;
    TextButton _btnOk, _btnCancel;
    Text _txtTarget;

    /**
     * Initializes all the elements in the Confirm Destination window.
     * @param game Pointer to the core game.
     * @param craft Pointer to the craft to retarget.
     * @param target Pointer to the selected target (NULL if it's just a point on the globe).
     */
    internal ConfirmDestinationState(Craft craft, Target target)
    {
        _craft = craft;
        _target = target;

        Waypoint w = (Waypoint)_target;
        _screen = false;

        // Create objects
        _window = new Window(this, 244, 72, 6, 64);
        _btnOk = new TextButton(50, 12, 68, 104);
        _btnCancel = new TextButton(50, 12, 138, 104);
        _txtTarget = new Text(232, 32, 12, 72);

        // Set palette
        setInterface("confirmDestination", w != null && w.getId() == 0);

        add(_window, "window", "confirmDestination");
        add(_btnOk, "button", "confirmDestination");
        add(_btnCancel, "button", "confirmDestination");
        add(_txtTarget, "text", "confirmDestination");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK12.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

        _btnCancel.setText(tr("STR_CANCEL_UC"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        _txtTarget.setBig();
        _txtTarget.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTarget.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
        _txtTarget.setWordWrap(true);
        if (w != null && w.getId() == 0)
        {
            _txtTarget.setText(tr("STR_TARGET").arg(tr("STR_WAY_POINT")));
        }
        else
        {
            _txtTarget.setText(tr("STR_TARGET").arg(_target.getName(_game.getLanguage())));
        }
    }

    /**
     *
     */
    ~ConfirmDestinationState() { }

    /**
     * Confirms the selected target for the craft.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        Waypoint w = (Waypoint)_target;
        if (w != null && w.getId() == 0)
        {
            w.setId(_game.getSavedGame().getId("STR_WAY_POINT"));
            _game.getSavedGame().getWaypoints().Add(w);
        }
        _craft.setDestination(_target);
        _craft.setStatus("STR_OUT");
        _game.popState();
        _game.popState();
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _)
    {
        Waypoint w = (Waypoint)_target;
        if (w != null && w.getId() == 0)
        {
            _target = null;
        }
        _game.popState();
    }
}
