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
 * Frame that briefly shows some info like : Yasuaki Okamoto Has Panicked. It disappears after 2 seconds.
 */
internal class InfoboxState : State
{
	const int INFOBOX_DELAY = 2000;

	Frame _frame;
	Text _text;
	Timer _timer;

	/**
	 * Initializes all the elements.
	 * @param game Pointer to the core game.
	 * @param msg Message string.
	 */
	internal InfoboxState(string msg)
	{
		_screen = false;

		// Create objects
		_frame = new Frame(261, 122, 34, 10);
		_text = new Text(251, 112, 39, 15);

		// Set palette
		_game.getSavedGame().getSavedBattle().setPaletteByDepth(this);

		add(_frame, "infoBox", "battlescape");
		add(_text, "infoBox", "battlescape");

		centerAllSurfaces();

		_frame.setHighContrast(true);
		_frame.setThickness(9);

		_text.setAlign(TextHAlign.ALIGN_CENTER);
		_text.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
		_text.setBig();
		_text.setWordWrap(true);
		_text.setText(msg);
		_text.setHighContrast(true);

		_timer = new Timer(INFOBOX_DELAY);
		_timer.onTimer((StateHandler)close);
		_timer.start();
	}

	/**
	 *
	 */
	~InfoboxState() =>
		_timer = null;

	/**
	 * Closes the window.
	 */
	void close() =>
		_game.popState();

	/**
	 * Keeps the animation timers running.
	 */
	internal override void think() =>
		_timer.think(this, null);

	/**
	 * Closes the window.
	 * @param action Pointer to an action.
	 */
	internal override void handle(Action action)
	{
		base.handle(action);

		if (action.getDetails().type == SDL_EventType.SDL_KEYDOWN || action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
		{
			close();
		}
	}
}
