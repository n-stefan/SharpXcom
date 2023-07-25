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
 * Screen shown monthly to allow changing
 * soldiers currently in psi training.
 */
internal class AllocatePsiTrainingState : State
{
    uint _sel;
    Base _base;
    Window _window;
    Text _txtTitle, _txtTraining, _txtName, _txtRemaining;
    Text _txtPsiStrength, _txtPsiSkill;
    TextButton _btnOk;
    TextList _lstSoldiers;
    int _labSpace;
    List<Soldier> _soldiers;

    /**
     * Initializes all the elements in the Psi Training screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to handle.
     */
    internal AllocatePsiTrainingState(Base @base)
    {
        _sel = 0;

        _base = @base;
        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        _txtTitle = new Text(300, 17, 10, 8);
        _txtRemaining = new Text(300, 10, 10, 24);
        _txtName = new Text(64, 10, 10, 40);
        _txtPsiStrength = new Text(80, 20, 124, 32);
        _txtPsiSkill = new Text(80, 20, 188, 32);
        _txtTraining = new Text(48, 20, 270, 32);
        _btnOk = new TextButton(160, 14, 80, 174);
        _lstSoldiers = new TextList(290, 112, 8, 52);

        // Set palette
        setInterface("allocatePsi");

        add(_window, "window", "allocatePsi");
        add(_btnOk, "button", "allocatePsi");
        add(_txtName, "text", "allocatePsi");
        add(_txtTitle, "text", "allocatePsi");
        add(_txtRemaining, "text", "allocatePsi");
        add(_txtPsiStrength, "text", "allocatePsi");
        add(_txtPsiSkill, "text", "allocatePsi");
        add(_txtTraining, "text", "allocatePsi");
        add(_lstSoldiers, "list", "allocatePsi");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_PSIONIC_TRAINING"));

        _labSpace = @base.getAvailablePsiLabs() - @base.getUsedPsiLabs();
        _txtRemaining.setText(tr("STR_REMAINING_PSI_LAB_CAPACITY").arg(_labSpace));

        _txtName.setText(tr("STR_NAME"));

        _txtPsiStrength.setText(tr("STR_PSIONIC__STRENGTH"));

        _txtPsiSkill.setText(tr("STR_PSIONIC_SKILL_IMPROVEMENT"));

        _txtTraining.setText(tr("STR_IN_TRAINING"));

        _lstSoldiers.setAlign(TextHAlign.ALIGN_RIGHT, 3);
        _lstSoldiers.setColumns(4, 114, 80, 62, 30);
        _lstSoldiers.setSelectable(true);
        _lstSoldiers.setBackground(_window);
        _lstSoldiers.setMargin(2);
        _lstSoldiers.onMouseClick(lstSoldiersClick);
        uint row = 0;
        foreach (var s in @base.getSoldiers())
        {
            string ssStr;
            string ssSkl;
            _soldiers.Add(s);
            if (s.getCurrentStats().psiSkill > 0 || (Options.psiStrengthEval && _game.getSavedGame().isResearched(_game.getMod().getPsiRequirements())))
            {
                ssStr = $"   {s.getCurrentStats().psiStrength}";
                if (Options.allowPsiStrengthImprovement) ssStr = $"{ssStr}/+{s.getPsiStrImprovement()}";
            }
            else
            {
                ssStr = tr("STR_UNKNOWN");
            }
            if (s.getCurrentStats().psiSkill > 0)
            {
                ssSkl = $"{s.getCurrentStats().psiSkill}/+{s.getImprovement()}";
            }
            else
            {
                ssSkl = "0/+0";
            }
            if (s.isInPsiTraining())
            {
                _lstSoldiers.addRow(4, s.getName(true), ssStr, ssSkl, tr("STR_YES"));
                _lstSoldiers.setRowColor(row, _lstSoldiers.getSecondaryColor());
            }
            else
            {
                _lstSoldiers.addRow(4, s.getName(true), ssStr, ssSkl, tr("STR_NO"));
                _lstSoldiers.setRowColor(row, _lstSoldiers.getColor());
            }
            row++;
        }
    }

    /**
     *
     */
    ~AllocatePsiTrainingState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        foreach (var i in _base.getSoldiers())
        {
            i.calcStatString(_game.getMod().getStatStrings(), (Options.psiStrengthEval && _game.getSavedGame().isResearched(_game.getMod().getPsiRequirements())));
        }
        _game.popState();
    }

    /**
     * Assigns / removes a soldier from Psi Training.
     * @param action Pointer to an action.
     */
    void lstSoldiersClick(Action action)
    {
        _sel = _lstSoldiers.getSelectedRow();
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            if (!_base.getSoldiers()[(int)_sel].isInPsiTraining())
            {
                if (_base.getUsedPsiLabs() < _base.getAvailablePsiLabs())
                {
                    _lstSoldiers.setCellText(_sel, 3, tr("STR_YES"));
                    _lstSoldiers.setRowColor(_sel, _lstSoldiers.getSecondaryColor());
                    _labSpace--;
                    _txtRemaining.setText(tr("STR_REMAINING_PSI_LAB_CAPACITY").arg(_labSpace));
                    _base.getSoldiers()[(int)_sel].setPsiTraining(true);
                }
            }
            else
            {
                _lstSoldiers.setCellText(_sel, 3, tr("STR_NO"));
                _lstSoldiers.setRowColor(_sel, _lstSoldiers.getColor());
                _labSpace++;
                _txtRemaining.setText(tr("STR_REMAINING_PSI_LAB_CAPACITY").arg(_labSpace));
                _base.getSoldiers()[(int)_sel].setPsiTraining(false);
            }
        }
    }
}
