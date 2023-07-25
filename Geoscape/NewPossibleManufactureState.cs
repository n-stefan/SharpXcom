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
 * Window which inform the player of new possible manufacture projects.
 * Also allow to go to the ManufactureState to dispatch available engineers.
 */
internal class NewPossibleManufactureState : State
{
    Base _base;
    Window _window;
    TextButton _btnManufacture, _btnOk;
    Text _txtTitle;
    TextList _lstPossibilities;

    /**
     * Initializes all the elements in the EndManufacture screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param possibilities List of newly possible ManufactureProject
     */
    internal NewPossibleManufactureState(Base @base, List<RuleManufacture> possibilities)
    {
        _base = @base;

        _screen = false;

        // Create objects
        _window = new Window(this, 288, 180, 16, 10);
        _btnOk = new TextButton(160, 14, 80, 149);
        _btnManufacture = new TextButton(160, 14, 80, 165);
        _txtTitle = new Text(288, 40, 16, 20);
        _lstPossibilities = new TextList(250, 80, 35, 50);

        // Set palette
        setInterface("geoManufacture");

        add(_window, "window", "geoManufacture");
        add(_btnOk, "button", "geoManufacture");
        add(_btnManufacture, "button", "geoManufacture");
        add(_txtTitle, "text1", "geoManufacture");
        add(_lstPossibilities, "text2", "geoManufacture");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK17.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);
        _btnManufacture.setText(tr("STR_ALLOCATE_MANUFACTURE"));
        _btnManufacture.onMouseClick(btnManufactureClick);
        _btnManufacture.onKeyboardPress(btnManufactureClick, Options.keyOk);
        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_WE_CAN_NOW_PRODUCE"));

        _lstPossibilities.setColumns(1, 250);
        _lstPossibilities.setBig();
        _lstPossibilities.setAlign(TextHAlign.ALIGN_CENTER);
        _lstPossibilities.setScrolling(true, 0);
        foreach (var iter in possibilities)
        {
            _lstPossibilities.addRow(1, tr(iter.getName()));
        }
    }

    /**
     * return to the previous screen
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();

    /**
     * Open the ManufactureState so the player can dispatch available scientist.
     * @param action Pointer to an action.
     */
    void btnManufactureClick(Action _)
    {
        _game.popState();
        _game.pushState(new ManufactureState(_base));
    }
}
