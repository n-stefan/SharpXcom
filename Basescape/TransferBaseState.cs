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
 * Window that lets the player pick the base
 * to transfer items to.
 */
internal class TransferBaseState : State
{
    Base _base;
    Window _window;
    TextButton _btnCancel;
    Text _txtTitle, _txtFunds, _txtName, _txtArea;
    TextList _lstBases;
    List<Base> _bases;

    /**
     * Initializes all the elements in the Select Destination Base window.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     */
    internal TransferBaseState(Base @base)
    {
        _base = @base;

        // Create objects
        _window = new Window(this, 280, 140, 20, 30);
        _btnCancel = new TextButton(264, 16, 28, 146);
        _txtTitle = new Text(270, 17, 25, 38);
        _txtFunds = new Text(250, 9, 30, 54);
        _txtName = new Text(130, 17, 28, 64);
        _txtArea = new Text(130, 17, 160, 64);
        _lstBases = new TextList(248, 64, 28, 80);

        // Set palette
        setInterface("transferBaseSelect");

        add(_window, "window", "transferBaseSelect");
        add(_btnCancel, "button", "transferBaseSelect");
        add(_txtTitle, "text", "transferBaseSelect");
        add(_txtFunds, "text", "transferBaseSelect");
        add(_txtName, "text", "transferBaseSelect");
        add(_txtArea, "text", "transferBaseSelect");
        add(_lstBases, "list", "transferBaseSelect");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK13.SCR"));

        _btnCancel.setText(tr("STR_CANCEL"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_SELECT_DESTINATION_BASE"));

        _txtFunds.setText(tr("STR_CURRENT_FUNDS").arg(Unicode.formatFunding(_game.getSavedGame().getFunds())));

        _txtName.setText(tr("STR_NAME"));
        _txtName.setBig();

        _txtArea.setText(tr("STR_AREA"));
        _txtArea.setBig();

        _lstBases.setColumns(2, 130, 116);
        _lstBases.setSelectable(true);
        _lstBases.setBackground(_window);
        _lstBases.setMargin(2);
        _lstBases.onMouseClick(lstBasesClick);

        uint row = 0;
        foreach (var i in _game.getSavedGame().getBases())
        {
            if (i != _base)
            {
                // Get area
                string area = null;
                foreach (var j in _game.getSavedGame().getRegions())
                {
                    if (j.getRules().insideRegion(i.getLongitude(), i.getLatitude()))
                    {
                        area = tr(j.getRules().getType());
                        break;
                    }
                }
                string ss = $"{Unicode.TOK_COLOR_FLIP}{area}";
                _lstBases.addRow(2, i.getName(), ss);
                _bases.Add(i);
                row++;
            }
        }
    }

    /**
     *
     */
    ~TransferBaseState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _) =>
        _game.popState();

    /**
     * Shows the Transfer screen for the selected base.
     * @param action Pointer to an action.
     */
    void lstBasesClick(Action _) =>
        _game.pushState(new TransferItemsState(_base, _bases[(int)_lstBases.getSelectedRow()]));
}
