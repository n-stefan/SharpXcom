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
 * Displays a view of units movement.
 */
internal class ScannerView : InteractiveSurface
{
	Game _game;
	BattleUnit _unit;
	int _frame;

	/**
	 * Initializes the Scanner view.
	 * @param w The ScannerView width.
	 * @param h The ScannerView height.
	 * @param x The ScannerView x origin.
	 * @param y The ScannerView y origin.
	 * @param game Pointer to the core game.
	 * @param unit The current unit.
	 */
	internal ScannerView(int w, int h, int x, int y, Game game, BattleUnit unit) : base(w, h, x, y)
	{
		_game = game;
		_unit = unit;
		_frame = 0;

		_redraw = true;
	}

	/**
	 * Updates the scanner animation.
	 */
	internal void animate()
	{
		_frame++;
		if (_frame > 1)
		{
			_frame = 0;
		}
		_redraw = true;
	}
}
