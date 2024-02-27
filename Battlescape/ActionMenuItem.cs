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

namespace SharpXcom.Battlescape;

/**
 * A class that represents a single box in the action popup menu on the battlescape.
 * It shows the possible actions of an item, their TU cost and accuracy.
 * Mouse over highlights the action, when clicked the action is sent to the parent state.
 */
internal class ActionMenuItem : InteractiveSurface
{
	bool _highlighted;
	BattleActionType _action;
	int _tu, _highlightModifier;
	Frame _frame;
	Text _txtDescription, _txtAcc, _txtTU;

	/**
	 * Sets up an Action menu item.
	 * @param id The unique identifier of the menu item.
	 * @param game Pointer to the game.
	 * @param x Position on the x-axis.
	 * @param y Position on the y-axis.
	 */
	internal ActionMenuItem(int id, Game game, int x, int y) : base(272, 40, x + 24, y - (id*40))
	{
		_highlighted = false;
		_action = BattleActionType.BA_NONE;
		_tu = 0;

		Font big = game.getMod().getFont("FONT_BIG"), small = game.getMod().getFont("FONT_SMALL");
		Language lang = game.getLanguage();

		Element actionMenu = game.getMod().getInterface("battlescape").getElement("actionMenu");

		_highlightModifier = actionMenu.TFTDMode ? 12 : 3;

		_frame = new Frame(getWidth(), getHeight(), 0, 0);
		_frame.setHighContrast(true);
		_frame.setColor((byte)actionMenu.border);
		_frame.setSecondaryColor((byte)actionMenu.color2);
		_frame.setThickness(8);

		_txtDescription = new Text(200, 20, 10, 13);
		_txtDescription.initText(big, small, lang);
		_txtDescription.setBig();
		_txtDescription.setHighContrast(true);
		_txtDescription.setColor((byte)actionMenu.color);
		_txtDescription.setVisible(true);

		_txtAcc = new Text(100, 20, 140, 13);
		_txtAcc.initText(big, small, lang);
		_txtAcc.setBig();
		_txtAcc.setHighContrast(true);
		_txtAcc.setColor((byte)actionMenu.color);

		_txtTU = new Text(80, 20, 210, 13);
		_txtTU.initText(big, small, lang);
		_txtTU.setBig();
		_txtTU.setHighContrast(true);
		_txtTU.setColor((byte)actionMenu.color);
	}

	/**
	 * Deletes the ActionMenuItem.
	 */
	~ActionMenuItem()
	{
		_frame = null;
		_txtDescription = null;
		_txtAcc = null;
		_txtTU = null;
	}

	/**
	 * Links with an action and fills in the text fields.
	 * @param action The battlescape action.
	 * @param description The actions description.
	 * @param accuracy The actions accuracy, including the Acc> prefix.
	 * @param timeunits The timeunits string, including the TUs> prefix.
	 * @param tu The timeunits value.
	 */
	internal void setAction(BattleActionType action, string description, string accuracy, string timeunits, int tu)
	{
		_action = action;
		_txtDescription.setText(description);
		_txtAcc.setText(accuracy);
		_txtTU.setText(timeunits);
		_tu = tu;
		_redraw = true;
	}

	/**
	 * Gets the action that was linked to this menu item.
	 * @return Action that was linked to this menu item.
	 */
	internal BattleActionType getAction() =>
		_action;

	/**
	 * Gets the action tus that were linked to this menu item.
	 * @return The timeunits that were linked to this menu item.
	 */
	internal int getTUs() =>
		_tu;

	/**
	 * Draws the bordered box.
	 */
	internal override void draw()
	{
		_frame.blit(this);
		_txtDescription.blit(this);
		_txtAcc.blit(this);
		_txtTU.blit(this);
	}

	/**
	 * Processes a mouse hover in event.
	 * @param action Pointer to an action.
	 * @param state Pointer to a state.
	 */
	protected override void mouseIn(Action action, State state)
	{
		_highlighted = true;
		_frame.setSecondaryColor((byte)(_frame.getSecondaryColor() - _highlightModifier));
		draw();
		base.mouseIn(action, state);
	}

	/**
	 * Processes a mouse hover out event.
	 * @param action Pointer to an action.
	 * @param state Pointer to a state.
	 */
	protected override void mouseOut(Action action, State state)
	{
		_highlighted = false;
		_frame.setSecondaryColor((byte)(_frame.getSecondaryColor() + _highlightModifier));
		draw();
		base.mouseOut(action, state);
	}
}
