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
 * Monthly Costs screen that displays all
 * the maintenance costs of a particular base.
 */
internal class MonthlyCostsState : State
{
    Base _base;
    Window _window;
    TextButton _btnOk;
    Text _txtTitle, _txtCost, _txtQuantity, _txtTotal, _txtRental, _txtSalaries, _txtIncome, _txtMaintenance;
    TextList _lstCrafts, _lstSalaries, _lstMaintenance, _lstTotal;

    /**
     * Initializes all the elements in the Monthly Costs screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     */
    internal MonthlyCostsState(Base @base)
    {
        _base = @base;

        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        _btnOk = new TextButton(300, 20, 10, 170);
        _txtTitle = new Text(310, 17, 5, 12);
        _txtCost = new Text(80, 9, 115, 32);
        _txtQuantity = new Text(55, 9, 195, 32);
        _txtTotal = new Text(60, 9, 249, 32);
        _txtRental = new Text(150, 9, 10, 40);
        _txtSalaries = new Text(150, 9, 10, 80);
        _txtIncome = new Text(150, 9, 10, 146);
        _txtMaintenance = new Text(150, 9, 10, 154);
        _lstCrafts = new TextList(288, 32, 10, 48);
        _lstSalaries = new TextList(288, 40, 10, 88);
        _lstMaintenance = new TextList(300, 9, 10, 128);
        _lstTotal = new TextList(100, 9, 205, 150);

        // Set palette
        setInterface("costsInfo");

        add(_window, "window", "costsInfo");
        add(_btnOk, "button", "costsInfo");
        add(_txtTitle, "text1", "costsInfo");
        add(_txtCost, "text1", "costsInfo");
        add(_txtQuantity, "text1", "costsInfo");
        add(_txtTotal, "text1", "costsInfo");
        add(_txtRental, "text1", "costsInfo");
        add(_lstCrafts, "list", "costsInfo");
        add(_txtSalaries, "text1", "costsInfo");
        add(_lstSalaries, "list", "costsInfo");
        add(_lstMaintenance, "text1", "costsInfo");
        add(_txtIncome, "list", "costsInfo");
        add(_txtMaintenance, "list", "costsInfo");
        add(_lstTotal, "text2", "costsInfo");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK13.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_MONTHLY_COSTS"));

        _txtCost.setText(tr("STR_COST_PER_UNIT"));

        _txtQuantity.setText(tr("STR_QUANTITY"));

        _txtTotal.setText(tr("STR_TOTAL"));

        _txtRental.setText(tr("STR_CRAFT_RENTAL"));

        _txtSalaries.setText(tr("STR_SALARIES"));

        string ss = $"{tr("STR_INCOME")}={Unicode.formatFunding(_game.getSavedGame().getCountryFunding())}";
        _txtIncome.setText(ss);

        string ss2 = $"{tr("STR_MAINTENANCE")}={Unicode.formatFunding(_game.getSavedGame().getBaseMaintenance())}";
        _txtMaintenance.setText(ss2);

        _lstCrafts.setColumns(4, 125, 70, 44, 50);
        _lstCrafts.setDot(true);

        List<string> crafts = _game.getMod().getCraftsList();
        foreach (var i in crafts)
        {
            RuleCraft craft = _game.getMod().getCraft(i);
            if (craft.getRentCost() != 0 && _game.getSavedGame().isResearched(craft.getRequirements()))
            {
                string ss3 = _base.getCraftCount(i).ToString();
                _lstCrafts.addRow(4, tr(i), Unicode.formatFunding(craft.getRentCost()), ss3, Unicode.formatFunding(_base.getCraftCount(i) * craft.getRentCost()));
            }
        }

        _lstSalaries.setColumns(4, 125, 70, 44, 50);
        _lstSalaries.setDot(true);

        List<string> soldiers = _game.getMod().getSoldiersList();
        foreach (var i in soldiers)
        {
            RuleSoldier soldier = _game.getMod().getSoldier(i);
            if (soldier.getSalaryCost() != 0 && _game.getSavedGame().isResearched(soldier.getRequirements()))
            {
                string ss4 = _base.getSoldierCount(i).ToString();
                string name = i;
                if (soldiers.Count == 1)
                {
                    name = "STR_SOLDIERS";
                }
                _lstSalaries.addRow(4, tr(name), Unicode.formatFunding(soldier.getSalaryCost()), ss4, Unicode.formatFunding(_base.getSoldierCount(i) * soldier.getSalaryCost()));
            }
        }
        string ss5 = _base.getTotalEngineers().ToString();
        _lstSalaries.addRow(4, tr("STR_ENGINEERS"), Unicode.formatFunding(_game.getMod().getEngineerCost()), ss5, Unicode.formatFunding(_base.getTotalEngineers() * _game.getMod().getEngineerCost()));
        string ss6 = _base.getTotalScientists().ToString();
        _lstSalaries.addRow(4, tr("STR_SCIENTISTS"), Unicode.formatFunding(_game.getMod().getScientistCost()), ss6, Unicode.formatFunding(_base.getTotalScientists() * _game.getMod().getScientistCost()));

        _lstMaintenance.setColumns(2, 239, 60);
        _lstMaintenance.setDot(true);
        string ss7 = $"{Unicode.TOK_COLOR_FLIP}{Unicode.formatFunding(_base.getFacilityMaintenance())}";
        _lstMaintenance.addRow(2, tr("STR_BASE_MAINTENANCE"), ss7);

        _lstTotal.setColumns(2, 44, 55);
        _lstTotal.setDot(true);
        _lstTotal.addRow(2, tr("STR_TOTAL"), Unicode.formatFunding(_base.getMonthlyMaintenace()));
    }

    /**
     *
     */
    ~MonthlyCostsState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();
}
