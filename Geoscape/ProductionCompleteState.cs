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
 * Window used to notify the player when
 * a production is completed.
 */
internal class ProductionCompleteState : State
{
    Base _base;
    GeoscapeState _state;
    productionProgress_e _endType;
    Window _window;
    TextButton _btnOk, _btnGotoBase;
    Text _txtMessage;

    /**
     * Initializes all the elements in a Production Complete window.
     * @param game Pointer to the core game.
     * @param base Pointer to base the production belongs to.
     * @param item Item that finished producing.
     * @param state Pointer to the Geoscape state.
     * @param endType What ended the production.
     */
    internal ProductionCompleteState(Base @base, string item, GeoscapeState state, productionProgress_e endType = productionProgress_e.PROGRESS_COMPLETE)
    {
        _base = @base;
        _state = state;
        _endType = endType;

        _screen = false;

        // Create objects
        _window = new Window(this, 256, 160, 32, 20, WindowPopup.POPUP_BOTH);
        _btnOk = new TextButton(118, 18, 40, 154);
        _btnGotoBase = new TextButton(118, 18, 162, 154);
        _txtMessage = new Text(246, 110, 37, 35);

        // Set palette
        setInterface("geoManufactureComplete");

        add(_window, "window", "geoManufactureComplete");
        add(_btnOk, "button", "geoManufactureComplete");
        add(_btnGotoBase, "button", "geoManufactureComplete");
        add(_txtMessage, "text1", "geoManufactureComplete");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK17.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        if (_endType != productionProgress_e.PROGRESS_CONSTRUCTION)
        {
            _btnGotoBase.setText(tr("STR_ALLOCATE_MANUFACTURE"));
        }
        else
        {
            _btnGotoBase.setText(tr("STR_GO_TO_BASE"));
        }
        _btnGotoBase.onMouseClick(btnGotoBaseClick);

        _txtMessage.setAlign(TextHAlign.ALIGN_CENTER);
        _txtMessage.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
        _txtMessage.setBig();
        _txtMessage.setWordWrap(true);
        string s = null;
        switch (_endType)
        {
            case productionProgress_e.PROGRESS_CONSTRUCTION:
                s = tr("STR_CONSTRUCTION_OF_FACILITY_AT_BASE_IS_COMPLETE").arg(item).arg(@base.getName());
                break;
            case productionProgress_e.PROGRESS_COMPLETE:
                s = tr("STR_PRODUCTION_OF_ITEM_AT_BASE_IS_COMPLETE").arg(item).arg(@base.getName());
                break;
            case productionProgress_e.PROGRESS_NOT_ENOUGH_MONEY:
                s = tr("STR_NOT_ENOUGH_MONEY_TO_PRODUCE_ITEM_AT_BASE").arg(item).arg(@base.getName());
                break;
            case productionProgress_e.PROGRESS_NOT_ENOUGH_MATERIALS:
                s = tr("STR_NOT_ENOUGH_SPECIAL_MATERIALS_TO_PRODUCE_ITEM_AT_BASE").arg(item).arg(@base.getName());
                break;
            default:
                Debug.Assert(false);
                break;
        }
        _txtMessage.setText(s);
    }

    /**
     *
     */
    ~ProductionCompleteState() { }

    /**
     * Closes the window.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();

    /**
     * Goes to the base for the respective production.
     * @param action Pointer to an action.
     */
    void btnGotoBaseClick(Action _)
    {
        _state.timerReset();
        _game.popState();
        if (_endType != productionProgress_e.PROGRESS_CONSTRUCTION)
        {
            _game.pushState(new ManufactureState(_base));
        }
        else
        {
            _game.pushState(new BasescapeState(_base, _state.getGlobe()));
        }
    }
}
