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
 * Screen that allows the player
 * to confirm a new base on the globe.
 * Note: This is different from the starting base screen, BaseNameState
 */
internal class ConfirmNewBaseState : State
{
    Base _base;
    Globe _globe;
    int _cost;
    Window _window;
    TextButton _btnOk, _btnCancel;
    Text _txtCost, _txtArea;

    /**
     * Initializes all the elements in the Confirm New Base window.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to place.
     * @param globe Pointer to the Geoscape globe.
     */
    internal ConfirmNewBaseState(Base @base, Globe globe)
    {
        _base = @base;
        _globe = globe;
        _cost = 0;

        _screen = false;

        // Create objects
        _window = new Window(this, 224, 72, 16, 64);
        _btnOk = new TextButton(54, 12, 68, 104);
        _btnCancel = new TextButton(54, 12, 138, 104);
        _txtCost = new Text(120, 9, 68, 80);
        _txtArea = new Text(120, 9, 68, 90);

        // Set palette
        setInterface("geoscape");

        add(_window, "genericWindow", "geoscape");
        add(_btnOk, "genericButton2", "geoscape");
        add(_btnCancel, "genericButton2", "geoscape");
        add(_txtCost, "genericText", "geoscape");
        add(_txtArea, "genericText", "geoscape");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

        _btnCancel.setText(tr("STR_CANCEL_UC"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        string area = null;
        foreach (var i in _game.getSavedGame().getRegions())
        {
            if (i.getRules().insideRegion(_base.getLongitude(), _base.getLatitude()))
            {
                _cost = i.getRules().getBaseCost();
                area = tr(i.getRules().getType());
                break;
            }
        }

        _txtCost.setText(tr("STR_COST_").arg(Unicode.formatFunding(_cost)));

        _txtArea.setText(tr("STR_AREA_").arg(area));
    }

    /**
     *
     */
    ~ConfirmNewBaseState() { }

    /**
     * Go to the Place Access Lift screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        if (_game.getSavedGame().getFunds() >= _cost)
        {
            _game.getSavedGame().setFunds(_game.getSavedGame().getFunds() - _cost);
            _game.getSavedGame().getBases().Add(_base);
            _game.pushState(new BaseNameState(_base, _globe, false));
        }
        else
        {
            _game.pushState(new ErrorMessageState(tr("STR_NOT_ENOUGH_MONEY"), _palette, (byte)_game.getMod().getInterface("geoscape").getElement("genericWindow").color, "BACK01.SCR", _game.getMod().getInterface("geoscape").getElement("palette").color));
        }
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _)
    {
        _globe.onMouseOver(null);
        _game.popState();
    }
}
