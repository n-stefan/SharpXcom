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

namespace SharpXcom.Menu;

struct compareSaveName : IComparer<SaveInfo>
{
	bool _reverse;

	internal compareSaveName(bool reverse) =>
        _reverse = reverse;

    public int Compare(SaveInfo a, SaveInfo b)
    {
        if (a.reserved == b.reserved)
        {
            return Unicode.naturalCompare(a.displayName, b.displayName);
        }
        else
        {
            return _reverse ? (b.reserved ? 1 : -1) : (a.reserved ? 1 : -1);
        }
    }
}

struct compareSaveTimestamp : IComparer<SaveInfo>
{
	bool _reverse;

    internal compareSaveTimestamp(bool reverse) =>
        _reverse = reverse;

    public int Compare(SaveInfo a, SaveInfo b)
	{
		if (a.reserved == b.reserved)
		{
            return (int)(a.timestamp - b.timestamp);
		}
		else
		{
            return _reverse ? (b.reserved ? 1 : -1) : (a.reserved ? 1 : -1);
        }
    }
}

/**
 * Base class for saved game screens which
 * provides the common layout and listing.
 */
internal class ListGamesState : State
{
    protected OptionsOrigin _origin;
    protected uint _firstValidRow;
    protected bool _autoquick, _sortable;
    protected Window _window;
    protected TextButton _btnCancel;
    protected Text _txtTitle, _txtName, _txtDate, _txtDelete, _txtDetails;
    protected TextList _lstSaves;
    protected ArrowButton _sortName, _sortDate;
    protected List<SaveInfo> _saves;

    /**
     * Initializes all the elements in the Saved Game screen.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     * @param firstValidRow First row containing saves.
     * @param autoquick Show auto/quick saved games?
     */
    protected ListGamesState(OptionsOrigin origin, int firstValidRow, bool autoquick)
    {
        _origin = origin;
        _firstValidRow = (uint)firstValidRow;
        _autoquick = autoquick;
        _sortable = true;

        _screen = false;

        // Create objects
        _window = new Window(this, 320, 200, 0, 0, WindowPopup.POPUP_BOTH);
        _btnCancel = new TextButton(80, 16, 120, 172);
        _txtTitle = new Text(310, 17, 5, 7);
        _txtDelete = new Text(310, 9, 5, 23);
        _txtName = new Text(150, 9, 16, 32);
        _txtDate = new Text(110, 9, 204, 32);
        _lstSaves = new TextList(288, 112, 8, 42);
        _txtDetails = new Text(288, 16, 16, 156);
        _sortName = new ArrowButton(ArrowShape.ARROW_NONE, 11, 8, 16, 32);
        _sortDate = new ArrowButton(ArrowShape.ARROW_NONE, 11, 8, 204, 32);

        // Set palette
        setInterface("geoscape", true, _game.getSavedGame() != null ? _game.getSavedGame().getSavedBattle() : null);

        add(_window, "window", "saveMenus");
        add(_btnCancel, "button", "saveMenus");
        add(_txtTitle, "text", "saveMenus");
        add(_txtDelete, "text", "saveMenus");
        add(_txtName, "text", "saveMenus");
        add(_txtDate, "text", "saveMenus");
        add(_lstSaves, "list", "saveMenus");
        add(_txtDetails, "text", "saveMenus");
        add(_sortName, "text", "saveMenus");
        add(_sortDate, "text", "saveMenus");

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

        _btnCancel.setText(tr("STR_CANCEL"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);

        _txtDelete.setAlign(TextHAlign.ALIGN_CENTER);
        _txtDelete.setText(tr("STR_RIGHT_CLICK_TO_DELETE"));

        _txtName.setText(tr("STR_NAME"));

        _txtDate.setText(tr("STR_DATE"));

        _lstSaves.setColumns(3, 188, 60, 40);
        _lstSaves.setSelectable(true);
        _lstSaves.setBackground(_window);
        _lstSaves.setMargin(8);
        _lstSaves.onMouseOver(lstSavesMouseOver);
        _lstSaves.onMouseOut(lstSavesMouseOut);
        _lstSaves.onMousePress(lstSavesPress);

        _txtDetails.setWordWrap(true);
        _txtDetails.setText(tr("STR_DETAILS").arg(string.Empty));

        _sortName.setX(_sortName.getX() + _txtName.getTextWidth() + 5);
        _sortName.onMouseClick(sortNameClick);

        _sortDate.setX(_sortDate.getX() + _txtDate.getTextWidth() + 5);
        _sortDate.onMouseClick(sortDateClick);

        updateArrows();
    }

    /**
     *
     */
    ~ListGamesState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _) =>
        _game.popState();

    /**
     * Shows the details of the currently hovered save.
     * @param action Pointer to an action.
     */
    void lstSavesMouseOver(Action _)
    {
        int sel = (int)(_lstSaves.getSelectedRow() - _firstValidRow);
        string wstr = null;
        if (sel >= 0 && sel < (int)_saves.Count)
        {
            wstr = _saves[sel].details;
        }
        _txtDetails.setText(tr("STR_DETAILS").arg(wstr));
    }

    /**
     * Clears the details.
     * @param action Pointer to an action.
     */
    void lstSavesMouseOut(Action _) =>
        _txtDetails.setText(tr("STR_DETAILS").arg(string.Empty));

    /**
     * Deletes the selected save.
     * @param action Pointer to an action.
     */
    void lstSavesPress(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_RIGHT && _lstSaves.getSelectedRow() >= _firstValidRow)
        {
            _game.pushState(new DeleteGameState(_origin, _saves[(int)(_lstSaves.getSelectedRow() - _firstValidRow)].fileName));
        }
    }

    /**
     * Sorts the saves by name.
     * @param action Pointer to an action.
     */
    void sortNameClick(Action _)
    {
        if (_sortable)
        {
            if (Options.saveOrder == SaveSort.SORT_NAME_ASC)
            {
                Options.saveOrder = SaveSort.SORT_NAME_DESC;
            }
            else
            {
                Options.saveOrder = SaveSort.SORT_NAME_ASC;
            }
            updateArrows();
            _lstSaves.clearList();
            sortList(Options.saveOrder);
        }
    }

    /**
     * Sorts the saves by date.
     * @param action Pointer to an action.
     */
    void sortDateClick(Action _)
    {
        if (_sortable)
        {
            if (Options.saveOrder == SaveSort.SORT_DATE_ASC)
            {
                Options.saveOrder = SaveSort.SORT_DATE_DESC;
            }
            else
            {
                Options.saveOrder = SaveSort.SORT_DATE_ASC;
            }
            updateArrows();
            _lstSaves.clearList();
            sortList(Options.saveOrder);
        }
    }

    /**
     * Updates the sorting arrows based
     * on the current setting.
     */
    void updateArrows()
    {
	    _sortName.setShape(ArrowShape.ARROW_NONE);
	    _sortDate.setShape(ArrowShape.ARROW_NONE);
	    switch (Options.saveOrder)
	    {
	        case SaveSort.SORT_NAME_ASC:
		        _sortName.setShape(ArrowShape.ARROW_SMALL_UP);
		        break;
	        case SaveSort.SORT_NAME_DESC:
		        _sortName.setShape(ArrowShape.ARROW_SMALL_DOWN);
		        break;
	        case SaveSort.SORT_DATE_ASC:
		        _sortDate.setShape(ArrowShape.ARROW_SMALL_UP);
		        break;
	        case SaveSort.SORT_DATE_DESC:
		        _sortDate.setShape(ArrowShape.ARROW_SMALL_DOWN);
		        break;
	    }
    }

    /**
     * Sorts the save game list.
     * @param sort Order to sort the games in.
     */
    void sortList(SaveSort sort)
    {
        switch (sort)
        {
            case SaveSort.SORT_NAME_ASC:
                _saves.Sort(new compareSaveName(false));
                break;
            case SaveSort.SORT_NAME_DESC:
                _saves.Sort(new compareSaveName(true));
                break;
            case SaveSort.SORT_DATE_ASC:
                _saves.Sort(new compareSaveTimestamp(false));
                break;
            case SaveSort.SORT_DATE_DESC:
                _saves.Sort(new compareSaveTimestamp(true));
                break;
        }
        updateList();
    }

    /**
     * Updates the save game list with the current list
     * of available savegames.
     */
    void updateList()
    {
        uint row = 0;
        byte color = _lstSaves.getSecondaryColor();
        foreach (var save in _saves)
        {
            _lstSaves.addRow(3, save.displayName, save.isoDate, save.isoTime);
            if (save.reserved && _origin != OptionsOrigin.OPT_BATTLESCAPE)
            {
                _lstSaves.setRowColor(row, color);
            }
            row++;
        }
    }
}
