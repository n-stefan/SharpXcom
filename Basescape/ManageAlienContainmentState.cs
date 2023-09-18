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
 * ManageAlienContainment screen that lets the player manage
 * alien numbers in a particular base.
 */
internal class ManageAlienContainmentState : State
{
    Base _base;
    OptionsOrigin _origin;
    uint _sel;
    int _aliensSold;
    Window _window;
    TextButton _btnOk, _btnCancel;
    Text _txtTitle, _txtUsed, _txtAvailable, _txtItem, _txtLiveAliens, _txtDeadAliens, _txtInterrogatedAliens;
    TextList _lstAliens;
    List<int> _qtys;
    List<string> _aliens;
    Timer _timerInc, _timerDec;

    /**
     * Initializes all the elements in the Manage Alien Containment screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param origin Game section that originated this state.
     */
    internal ManageAlienContainmentState(Base @base, OptionsOrigin origin)
    {
        _base = @base;
        _origin = origin;
        _sel = 0;
        _aliensSold = 0;

        bool overCrowded = Options.storageLimitsEnforced && _base.getFreeContainment() < 0;
        var researchList = new List<string>();
        foreach (var iter in _base.getResearch())
        {
            RuleResearch research = iter.getRules();
            RuleItem item = _game.getMod().getItem(research.getName());
            if (item != null && item.isAlien())
            {
                researchList.Add(research.getName());
            }
        }

        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        _btnOk = new TextButton(overCrowded ? 288 : 148, 16, overCrowded ? 16 : 8, 176);
        _btnCancel = new TextButton(148, 16, 164, 176);
        _txtTitle = new Text(310, 17, 5, 8);
        _txtAvailable = new Text(190, 9, 10, 24);
        _txtUsed = new Text(110, 9, 136, 24);
        _txtItem = new Text(120, 9, 10, 41);
        _txtLiveAliens = new Text(54, 18, 153, 32);
        _txtDeadAliens = new Text(54, 18, 207, 32);
        _txtInterrogatedAliens = new Text(54, 18, 261, 32);
        _lstAliens = new TextList(286, 112, 8, 53);

        // Set palette
        setInterface("manageContainment");

        add(_window, "window", "manageContainment");
        add(_btnOk, "button", "manageContainment");
        add(_btnCancel, "button", "manageContainment");
        add(_txtTitle, "text", "manageContainment");
        add(_txtAvailable, "text", "manageContainment");
        add(_txtUsed, "text", "manageContainment");
        add(_txtItem, "text", "manageContainment");
        add(_txtLiveAliens, "text", "manageContainment");
        add(_txtDeadAliens, "text", "manageContainment");
        add(_txtInterrogatedAliens, "text", "manageContainment");
        add(_lstAliens, "list", "manageContainment");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface((origin == OptionsOrigin.OPT_BATTLESCAPE) ? "BACK01.SCR" : "BACK05.SCR"));

        _btnOk.setText(tr("STR_REMOVE_SELECTED"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

        _btnCancel.setText(tr("STR_CANCEL"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        if (overCrowded)
        {
            _btnCancel.setVisible(false);
            _btnOk.setVisible(false);
        }

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_MANAGE_CONTAINMENT"));

        _txtItem.setText(tr("STR_ALIEN"));

        _txtLiveAliens.setText(tr("STR_LIVE_ALIENS"));
        _txtLiveAliens.setWordWrap(true);
        _txtLiveAliens.setVerticalAlign(TextVAlign.ALIGN_BOTTOM);

        _txtDeadAliens.setText(tr("STR_DEAD_ALIENS"));
        _txtDeadAliens.setWordWrap(true);
        _txtDeadAliens.setVerticalAlign(TextVAlign.ALIGN_BOTTOM);

        _txtInterrogatedAliens.setText(tr("STR_UNDER_INTERROGATION"));
        _txtInterrogatedAliens.setWordWrap(true);
        _txtInterrogatedAliens.setVerticalAlign(TextVAlign.ALIGN_BOTTOM);

        _txtAvailable.setText(tr("STR_SPACE_AVAILABLE").arg(_base.getFreeContainment()));

        _txtUsed.setText(tr("STR_SPACE_USED").arg(_base.getUsedContainment()));

        _lstAliens.setArrowColumn(184, ArrowOrientation.ARROW_HORIZONTAL);
        _lstAliens.setColumns(4, 160, 64, 46, 46);
        _lstAliens.setSelectable(true);
        _lstAliens.setBackground(_window);
        _lstAliens.setMargin(2);
        _lstAliens.onLeftArrowPress(lstItemsLeftArrowPress);
        _lstAliens.onLeftArrowRelease(lstItemsLeftArrowRelease);
        _lstAliens.onLeftArrowClick(lstItemsLeftArrowClick);
        _lstAliens.onRightArrowPress(lstItemsRightArrowPress);
        _lstAliens.onRightArrowRelease(lstItemsRightArrowRelease);
        _lstAliens.onRightArrowClick(lstItemsRightArrowClick);
        _lstAliens.onMousePress(lstItemsMousePress);

        List<string> items = _game.getMod().getItemsList();
        foreach (var i in items)
        {
            int qty = _base.getStorageItems().getItem(i);
            if (qty > 0 && _game.getMod().getItem(i, true).isAlien())
            {
                _qtys.Add(0);
                _aliens.Add(i);
                string ss = qty.ToString();
                string rqty;
                if (researchList.Contains(i))
                {
                    rqty = "1";
                    researchList.Remove(i);
                }
                else
                {
                    rqty = "0";
                }
                _lstAliens.addRow(4, tr(i), ss, "0", rqty);
            }
        }

        foreach (var i in researchList)
        {
            _aliens.Add(i);
            _qtys.Add(0);
            _lstAliens.addRow(4, tr(i), "0", "0", "1");
            _lstAliens.setRowColor((uint)(_qtys.Count - 1), _lstAliens.getSecondaryColor());
        }
        _timerInc = new Timer(250);
        _timerInc.onTimer((StateHandler)increase);
        _timerDec = new Timer(250);
        _timerDec.onTimer((StateHandler)decrease);
    }

    /**
     *
     */
    ~ManageAlienContainmentState()
    {
        _timerInc = null;
        _timerDec = null;
    }

    /**
     * Deals with the selected aliens.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        for (int i = 0; i < _qtys.Count; ++i)
        {
            if (_qtys[i] > 0)
            {
                // remove the aliens
                _base.getStorageItems().removeItem(_aliens[i], _qtys[i]);

                if (Options.canSellLiveAliens)
                {
                    _game.getSavedGame().setFunds(_game.getSavedGame().getFunds() + _game.getMod().getItem(_aliens[i], true).getSellCost() * _qtys[i]);
                }
                else
                {
                    // add the corpses
                    _base.getStorageItems().addItem(
                        _game.getMod().getArmor(
                            _game.getMod().getUnit(
                                _aliens[i], true
                            ).getArmor(), true
                        ).getCorpseGeoscape(), _qtys[i]
                    ); // ;)
                }
            }
        }
        _game.popState();

        if (Options.storageLimitsEnforced && _base.storesOverfull())
        {
            _game.pushState(new SellState(_base, _origin));
            if (_origin == OptionsOrigin.OPT_BATTLESCAPE)
                _game.pushState(new ErrorMessageState(tr("STR_STORAGE_EXCEEDED").arg(_base.getName()), _palette, (byte)_game.getMod().getInterface("manageContainment").getElement("errorMessage").color, "BACK01.SCR", _game.getMod().getInterface("manageContainment").getElement("errorPalette").color));
            else
                _game.pushState(new ErrorMessageState(tr("STR_STORAGE_EXCEEDED").arg(_base.getName()), _palette, (byte)_game.getMod().getInterface("manageContainment").getElement("errorMessage").color, "BACK13.SCR", _game.getMod().getInterface("manageContainment").getElement("errorPalette").color));
        }
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _) =>
        _game.popState();

    /**
     * Starts decreasing the alien count.
     * @param action Pointer to an action.
     */
    void lstItemsLeftArrowPress(Action action)
    {
        _sel = _lstAliens.getSelectedRow();
        if (action.getDetails().button.button == SDL_BUTTON_LEFT && !_timerDec.isRunning()) _timerDec.start();
    }

    /**
     * Stops decreasing the alien count.
     * @param action Pointer to an action.
     */
    void lstItemsLeftArrowRelease(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            _timerDec.stop();
        }
    }

    /**
     * Decreases the selected alien count;
     * by one on left-click, to 0 on right-click.
     * @param action Pointer to an action.
     */
    void lstItemsLeftArrowClick(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_RIGHT) decreaseByValue(int.MaxValue);
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            decreaseByValue(1);
            _timerInc.setInterval(250);
            _timerDec.setInterval(250);
        }
    }

    /**
     * Starts increasing the alien count.
     * @param action Pointer to an action.
     */
    void lstItemsRightArrowPress(Action action)
    {
        _sel = _lstAliens.getSelectedRow();
        if (action.getDetails().button.button == SDL_BUTTON_LEFT && !_timerInc.isRunning()) _timerInc.start();
    }

    /**
     * Stops increasing the alien count.
     * @param action Pointer to an action.
     */
    void lstItemsRightArrowRelease(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            _timerInc.stop();
        }
    }

    /**
     * Increases the selected alien count;
     * by one on left-click, to max on right-click.
     * @param action Pointer to an action.
     */
    void lstItemsRightArrowClick(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_RIGHT) increaseByValue(int.MaxValue);
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            increaseByValue(1);
            _timerInc.setInterval(250);
            _timerDec.setInterval(250);
        }
    }

    /**
     * Handles the mouse-wheels on the arrow-buttons.
     * @param action Pointer to an action.
     */
    void lstItemsMousePress(Action action)
    {
        _sel = _lstAliens.getSelectedRow();
        if (action.getDetails().wheel.y > 0) //button.button == SDL_BUTTON_WHEELUP
        {
            _timerInc.stop();
            _timerDec.stop();
            if (action.getAbsoluteXMouse() >= _lstAliens.getArrowsLeftEdge() &&
                action.getAbsoluteXMouse() <= _lstAliens.getArrowsRightEdge())
            {
                increaseByValue(Options.changeValueByMouseWheel);
            }
        }
        else if (action.getDetails().wheel.y < 0) //button.button == SDL_BUTTON_WHEELDOWN
        {
            _timerInc.stop();
            _timerDec.stop();
            if (action.getAbsoluteXMouse() >= _lstAliens.getArrowsLeftEdge() &&
                action.getAbsoluteXMouse() <= _lstAliens.getArrowsRightEdge())
            {
                decreaseByValue(Options.changeValueByMouseWheel);
            }
        }
    }

    /**
     * Increases the quantity of the selected alien to exterminate by one.
     */
    void increase()
    {
        _timerDec.setInterval(50);
        _timerInc.setInterval(50);
        increaseByValue(1);
    }

    /**
     * Decreases the quantity of the selected alien to exterminate by one.
     */
    void decrease()
    {
        _timerInc.setInterval(50);
        _timerDec.setInterval(50);
        decreaseByValue(1);
    }

    /**
     * Decreases the quantity of the selected alien to exterminate by "change".
     * @param change How much we want to remove.
     */
    void decreaseByValue(int change)
    {
        if (change <= 0 || _qtys[(int)_sel] <= 0) return;
        change = Math.Min(_qtys[(int)_sel], change);
        _qtys[(int)_sel] -= change;
        _aliensSold -= change;
        updateStrings();
    }

    /**
     * Increases the quantity of the selected alien to exterminate by "change".
     * @param change How much we want to add.
     */
    void increaseByValue(int change)
    {
        int qty = getQuantity() - _qtys[(int)_sel];
        if (change <= 0 || qty <= 0) return;

        change = Math.Min(qty, change);
        _qtys[(int)_sel] += change;
        _aliensSold += change;
        updateStrings();
    }

    /**
     * Gets the quantity of the currently selected alien on the base.
     * @return Quantity of selected alien on the base.
     */
    int getQuantity() =>
        _base.getStorageItems().getItem(_aliens[(int)_sel]);

    /**
     * Updates the quantity-strings of the selected alien.
     */
    void updateStrings()
    {
        int qty = getQuantity() - _qtys[(int)_sel];
        string ss = qty.ToString();
        string ss2 = _qtys[(int)_sel].ToString();

        _lstAliens.setRowColor(_sel, (qty == 0) ? _lstAliens.getSecondaryColor() : _lstAliens.getColor());
        _lstAliens.setCellText(_sel, 1, ss);
        _lstAliens.setCellText(_sel, 2, ss2);

        int aliens = _base.getUsedContainment() - _aliensSold;
        int spaces = _base.getAvailableContainment() - aliens;
        if (Options.storageLimitsEnforced)
        {
            _btnOk.setVisible(spaces >= 0);
        }
        _txtAvailable.setText(tr("STR_SPACE_AVAILABLE").arg(spaces));
        _txtUsed.setText(tr("STR_SPACE_USED").arg(aliens));
    }

    /**
     * Runs the arrow timers.
     */
    protected override void think()
    {
	    base.think();

	    _timerInc.think(this, null);
	    _timerDec.think(this, null);
    }
}
