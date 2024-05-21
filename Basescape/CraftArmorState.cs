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
 * Select Armor screen that lets the player
 * pick armor for the soldiers on the craft.
 */
internal class CraftArmorState : State
{
    Base _base;
    uint _craft;
    Window _window;
    TextButton _btnOk;
    Text _txtTitle, _txtName, _txtCraft, _txtArmor;
    TextList _lstSoldiers;

    /**
     * Initializes all the elements in the Craft Armor screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param craft ID of the selected craft.
     */
    internal CraftArmorState(Base @base, uint craft)
    {
        _base = @base;
        _craft = craft;

        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        _btnOk = new TextButton(288, 16, 16, 176);
        _txtTitle = new Text(300, 17, 16, 7);
        _txtName = new Text(114, 9, 16, 32);
        _txtCraft = new Text(76, 9, 130, 32);
        _txtArmor = new Text(100, 9, 199, 32);
        _lstSoldiers = new TextList(292, 128, 8, 40);

        // Set palette
        setInterface("craftArmor");

        add(_window, "window", "craftArmor");
        add(_btnOk, "button", "craftArmor");
        add(_txtTitle, "text", "craftArmor");
        add(_txtName, "text", "craftArmor");
        add(_txtCraft, "text", "craftArmor");
        add(_txtArmor, "text", "craftArmor");
        add(_lstSoldiers, "list", "craftArmor");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK14.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setText(tr("STR_SELECT_ARMOR"));

        _txtName.setText(tr("STR_NAME_UC"));

        _txtCraft.setText(tr("STR_CRAFT"));

        _txtArmor.setText(tr("STR_ARMOR"));

        _lstSoldiers.setColumns(3, 114, 69, 101);
        _lstSoldiers.setSelectable(true);
        _lstSoldiers.setBackground(_window);
        _lstSoldiers.setMargin(8);
        _lstSoldiers.setScrolling(true, 0);
        _lstSoldiers.onMousePress(lstSoldiersClick);

        byte otherCraftColor = (byte)_game.getMod().getInterface("craftArmor").getElement("otherCraft").color;
        uint row = 0;
        Craft c = _base.getCrafts()[(int)_craft];
        foreach (var i in _base.getSoldiers())
        {
            _lstSoldiers.addRow(3, i.getName(true), i.getCraftString(_game.getLanguage()), tr(i.getArmor().getType()));

            byte color;
            if (i.getCraft() == c)
            {
                color = _lstSoldiers.getSecondaryColor();
            }
            else if (i.getCraft() != null)
            {
                color = otherCraftColor;
            }
            else
            {
                color = _lstSoldiers.getColor();
            }
            _lstSoldiers.setRowColor(row, color);
            row++;
        }
    }

    /**
     *
     */
    ~CraftArmorState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();

    /**
     * Shows the Select Armor window.
     * @param action Pointer to an action.
     */
    void lstSoldiersClick(Action action)
    {
        Soldier s = _base.getSoldiers()[(int)_lstSoldiers.getSelectedRow()];
        if (!(s.getCraft() != null && s.getCraft().getStatus() == "STR_OUT"))
        {
            if (action.getDetails().button.button == SDL_BUTTON_LEFT)
            {
                _game.pushState(new SoldierArmorState(_base, _lstSoldiers.getSelectedRow()));
            }
            else if (action.getDetails().button.button == SDL_BUTTON_RIGHT)
            {
                SavedGame save;
                save = _game.getSavedGame();
                Armor a = _game.getMod().getArmor(save.getLastSelectedArmor());
                if (a != null && (!a.getUnits().Any() || a.getUnits().Contains(s.getRules().getType())))
                {
                    if (save.getMonthsPassed() != -1)
                    {
                        if (_base.getStorageItems().getItem(a.getStoreItem()) > 0 || a.getStoreItem() == Armor.NONE)
                        {
                            if (s.getArmor().getStoreItem() != Armor.NONE)
                            {
                                _base.getStorageItems().addItem(s.getArmor().getStoreItem());
                            }
                            if (a.getStoreItem() != Armor.NONE)
                            {
                                _base.getStorageItems().removeItem(a.getStoreItem());
                            }

                            s.setArmor(a);
                            _lstSoldiers.setCellText(_lstSoldiers.getSelectedRow(), 2, tr(a.getType()));
                        }
                    }
                    else
                    {
                        s.setArmor(a);
                        _lstSoldiers.setCellText(_lstSoldiers.getSelectedRow(), 2, tr(a.getType()));
                    }
                }
            }
        }
    }

    /**
     * The soldier armors can change
     * after going into other screens.
     */
    internal override void init()
    {
	    base.init();
	    uint row = 0;
	    foreach (var i in _base.getSoldiers())
	    {
		    _lstSoldiers.setCellText(row, 2, tr(i.getArmor().getType()));
		    row++;
	    }
    }
}
