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

namespace SharpXcom.Ufopaedia;

/**
 * UfopaediaSelectState is the screen that lists articles of a given type.
 */
internal class UfopaediaSelectState : State
{
    string _section;
    Window _window;
    Text _txtTitle;
    TextButton _btnOk;
    TextList _lstSelection;
    List<ArticleDefinition> _article_list;

    internal UfopaediaSelectState(string section)
    {
        _section = section;

        _screen = false;

        // set background window
        _window = new Window(this, 256, 180, 32, 10, WindowPopup.POPUP_NONE);

        // set title
        _txtTitle = new Text(224, 17, 48, 26);

        // set buttons
        _btnOk = new TextButton(224, 16, 48, 166);
        _lstSelection = new TextList(224, 104, 40, 50);

        // Set palette
        setInterface("ufopaedia");

        add(_window, "window", "ufopaedia");
        add(_txtTitle, "text", "ufopaedia");
        add(_btnOk, "button2", "ufopaedia");
        add(_lstSelection, "list", "ufopaedia");

        centerAllSurfaces();

        _window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_SELECT_ITEM"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _lstSelection.setColumns(1, 206);
        _lstSelection.setSelectable(true);
        _lstSelection.setBackground(_window);
        _lstSelection.setMargin(18);
        _lstSelection.setAlign(TextHAlign.ALIGN_CENTER);
        _lstSelection.onMouseClick(lstSelectionClick);

        loadSelectionList();
    }

    ~UfopaediaSelectState() { }

    /**
	 * Returns to the previous screen.
	 * @param action Pointer to an action.
	 */
    void btnOkClick(Engine.Action _) =>
        _game.popState();

    /**
	 *
	 * @param action Pointer to an action.
	 */
    void lstSelectionClick(Engine.Action _) =>
        Ufopaedia.openArticle(_game, _article_list[(int)_lstSelection.getSelectedRow()]);

    void loadSelectionList()
    {
        _article_list.Clear();
        Ufopaedia.list(_game.getSavedGame(), _game.getMod(), _section, _article_list);
        foreach (var it in _article_list)
        {
            _lstSelection.addRow(1, tr(it.title));
        }
    }
}
