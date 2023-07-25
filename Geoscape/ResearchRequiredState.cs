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
 * Window shown when the player researches a weapon
 * before the respective clip.
 */
internal class ResearchRequiredState : State
{
    Window _window;
    TextButton _btnOk;
    Text _txtTitle;

    /**
	 * Initializes all the elements in the Research Required screen.
	 * @param game Pointer to the core game.
	 * @param item Pointer to the researched weapon.
	 */
    internal ResearchRequiredState(RuleItem item)
	{
		_screen = false;

		// Create objects
		_window = new Window(this, 288, 180, 16, 10);
		_btnOk = new TextButton(160, 18, 80, 150);
		_txtTitle = new Text(288, 80, 16, 50);

		// Set palette
		setInterface("geoResearchRequired");

		add(_window, "window", "geoResearchRequired");
		add(_btnOk, "button", "geoResearchRequired");
		add(_txtTitle, "text1", "geoResearchRequired");

		centerAllSurfaces();

		string weapon = item.getType();
		string clip = item.getCompatibleAmmo().First();

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK05.SCR"));

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

		_txtTitle.setBig();
		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
		_txtTitle.setText(tr("STR_YOU_NEED_TO_RESEARCH_ITEM_TO_PRODUCE_ITEM")
						   .arg(tr(clip))
						   .arg(tr(weapon)));
	}

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();
}
