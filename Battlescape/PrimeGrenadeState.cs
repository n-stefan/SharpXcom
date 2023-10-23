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
 * along with OpenXcom.  If not, see <http:///www.gnu.org/licenses/>.
 */

namespace SharpXcom.Battlescape;

/**
 * Window that allows the player
 * to set the timer of an explosive.
 */
internal class PrimeGrenadeState : State
{
	BattleAction _action;
	bool _inInventoryView;
	BattleItem _grenadeInInventory;
	Text _title;
	Frame _frame;
	Surface _bg;
	InteractiveSurface[] _button = new InteractiveSurface[24];
	Text[] _number = new Text[24];

	/**
	 * Initializes all the elements in the Prime Grenade window.
	 * @param game Pointer to the core game.
	 * @param action Pointer to the action.
	 * @param inInventoryView Called from inventory?
	 * @param grenadeInInventory Pointer to associated grenade.
	 */
	PrimeGrenadeState(BattleAction action, bool inInventoryView, BattleItem grenadeInInventory)
	{
		_action = action;
		_inInventoryView = inInventoryView;
		_grenadeInInventory = grenadeInInventory;

		_screen = false;

		// Create objects
		_title = new Text(192, 24, 65, 44);
		_frame = new Frame(192, 27, 65, 37);
		_bg = new Surface(192, 93, 65, 45);

		int x = 67;
		int y = 68;
		for (int i = 0; i < 24; ++i)
		{
			_button[i] = new InteractiveSurface(22, 22, x-1+((i%8)*24), y-4+((i/8)*25));
			_number[i] = new Text(20, 20, x+((i%8)*24), y-1+((i/8)*25));
		}

		// Set palette
		if (inInventoryView)
		{
			setPalette("PAL_BATTLESCAPE");
		}
		else
		{
			_game.getSavedGame().getSavedBattle().setPaletteByDepth(this);
		}

		Element grenadeBackground = _game.getMod().getInterface("battlescape").getElement("grenadeBackground");

		// Set up objects
		add(_bg);
		_bg.drawRect(0, 0, (short)_bg.getWidth(), (short)_bg.getHeight(), (byte)grenadeBackground.color);

		add(_frame, "grenadeMenu", "battlescape");
		_frame.setThickness(3);
		_frame.setHighContrast(true);

		add(_title, "grenadeMenu", "battlescape");
		_title.setAlign(TextHAlign.ALIGN_CENTER);
		_title.setBig();
		_title.setText(tr("STR_SET_TIMER"));
		_title.setHighContrast(true);

		for (int i = 0; i < 24; ++i)
		{
			SDL_Rect square;
			add(_button[i]);
			_button[i].onMouseClick(btnClick);
			square.x = 0;
			square.y = 0;
			square.w = _button[i].getWidth();
			square.h = _button[i].getHeight();
			_button[i].drawRect(ref square, (byte)grenadeBackground.border);
			square.x++;
			square.y++;
			square.w -= 2;
			square.h -= 2;
			_button[i].drawRect(ref square, (byte)grenadeBackground.color2);

			string ss = i.ToString();
			add(_number[i], "grenadeMenu", "battlescape");
			_number[i].setBig();
			_number[i].setText(ss);
			_number[i].setHighContrast(true);
			_number[i].setAlign(TextHAlign.ALIGN_CENTER);
			_number[i].setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
		}

		centerAllSurfaces();
		lowerAllSurfaces();
	}

	/**
	 *
	 */
	~PrimeGrenadeState() { }

	/**
	 * Executes the action corresponding to this action menu item.
	 * @param action Pointer to an action.
	 */
	void btnClick(Action action)
	{
		int btnID = -1;

		if (action.getDetails().button.button == SDL_BUTTON_RIGHT)
		{
			if (!_inInventoryView) _action.value = btnID;
			_game.popState();
			return;
		}

		// got to find out which button was pressed
		for (int i = 0; i < 24 && btnID == -1; ++i)
		{
			if (action.getSender() == _button[i])
			{
				btnID = i;
			}
		}

		if (btnID != -1)
		{
			if (_inInventoryView) _grenadeInInventory.setFuseTimer(0 + btnID);
			else _action.value = btnID;
			_game.popState();
			if (!_inInventoryView) _game.popState();
		}
	}

	/**
	 * Closes the window on right-click.
	 * @param action Pointer to an action.
	 */
	protected override void handle(Action action)
	{
		base.handle(action);
		if (action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN && action.getDetails().button.button == SDL_BUTTON_RIGHT)
		{
			if (!_inInventoryView) _action.value = -1;
			_game.popState();
		}
	}
}
