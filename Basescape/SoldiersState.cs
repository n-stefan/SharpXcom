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
 * Soldiers screen that lets the player
 * manage all the soldiers in a base.
 */
internal class SoldiersState : State
{
    Base _base;
    Window _window;
    TextButton _btnOk, _btnPsiTraining, _btnMemorial;
    Text _txtTitle, _txtName, _txtRank, _txtCraft;
    TextList _lstSoldiers;

    /**
     * Initializes all the elements in the Soldiers screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     */
    internal SoldiersState(Base @base)
    {
        _base = @base;

        bool isPsiBtnVisible = Options.anytimePsiTraining && _base.getAvailablePsiLabs() > 0;

        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        if (isPsiBtnVisible)
        {
            _btnOk = new TextButton(96, 16, 216, 176);
            _btnPsiTraining = new TextButton(96, 16, 112, 176);
            _btnMemorial = new TextButton(96, 16, 8, 176);
        }
        else
        {
            _btnOk = new TextButton(148, 16, 164, 176);
            _btnPsiTraining = new TextButton(148, 16, 164, 176);
            _btnMemorial = new TextButton(148, 16, 8, 176);
        }
        _txtTitle = new Text(310, 17, 5, 8);
        _txtName = new Text(114, 9, 16, 32);
        _txtRank = new Text(102, 9, 130, 32);
        _txtCraft = new Text(82, 9, 222, 32);
        _lstSoldiers = new TextList(288, 128, 8, 40);

        // Set palette
        setInterface("soldierList");

        add(_window, "window", "soldierList");
        add(_btnOk, "button", "soldierList");
        add(_btnPsiTraining, "button", "soldierList");
        add(_btnMemorial, "button", "soldierList");
        add(_txtTitle, "text1", "soldierList");
        add(_txtName, "text2", "soldierList");
        add(_txtRank, "text2", "soldierList");
        add(_txtCraft, "text2", "soldierList");
        add(_lstSoldiers, "list", "soldierList");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK02.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _btnPsiTraining.setText(tr("STR_PSI_TRAINING"));
        _btnPsiTraining.onMouseClick(btnPsiTrainingClick);
        _btnPsiTraining.setVisible(isPsiBtnVisible);

        _btnMemorial.setText(tr("STR_MEMORIAL"));
        _btnMemorial.onMouseClick(btnMemorialClick);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_SOLDIER_LIST"));

        _txtName.setText(tr("STR_NAME_UC"));

        _txtRank.setText(tr("STR_RANK"));

        _txtCraft.setText(tr("STR_CRAFT"));

        _lstSoldiers.setColumns(3, 114, 92, 74);
        _lstSoldiers.setSelectable(true);
        _lstSoldiers.setBackground(_window);
        _lstSoldiers.setMargin(8);
        _lstSoldiers.onMouseClick(lstSoldiersClick);
    }

    /**
     *
     */
    ~SoldiersState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();

    /**
     * Opens the Psionic Training screen.
     * @param action Pointer to an action.
     */
    void btnPsiTrainingClick(Action _) =>
        _game.pushState(new AllocatePsiTrainingState(_base));

    /**
     * Opens the Memorial screen.
     * @param action Pointer to an action.
     */
    void btnMemorialClick(Action _) =>
        _game.pushState(new SoldierMemorialState());

    /**
     * Shows the selected soldier's info.
     * @param action Pointer to an action.
     */
    void lstSoldiersClick(Action _) =>
        _game.pushState(new SoldierInfoState(_base, _lstSoldiers.getSelectedRow()));
}
