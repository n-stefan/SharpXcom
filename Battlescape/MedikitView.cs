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
 * Display a view of unit wounds
 */
internal class MedikitView : InteractiveSurface
{
	Game _game;
	int _selectedPart;
	BattleUnit _unit;
	Text _partTxt, _woundTxt;

	/**
	 * Initializes the Medikit view.
	 * @param w The MinikitView width.
	 * @param h The MinikitView height.
	 * @param x The MinikitView x origin.
	 * @param y The MinikitView y origin.
	 * @param game Pointer to the core game.
	 * @param unit The wounded unit.
	 * @param partTxt A pointer to a Text. Will be updated with the selected body part.
	 * @param woundTxt A pointer to a Text. Will be updated with the amount of fatal wound.
	 */
	internal MedikitView(int w, int h, int x, int y, Game game, BattleUnit unit, Text partTxt, Text woundTxt) : base(w, h, x, y)
	{
		_game = game;
		_selectedPart = 0;
		_unit = unit;
		_partTxt = partTxt;
		_woundTxt = woundTxt;

		updateSelectedPart();
		_redraw = true;
	}

	/**
	 * Updates the selected body part.
	 * If there is a wounded body part, selects that.
	 * Otherwise does not change the selected part.
	 */
	internal void updateSelectedPart()
	{
		for (int i = 0; i < 6; ++i)
		{
			if (_unit.getFatalWound(i) != 0)
			{
				_selectedPart = i;
				break;
			}
		}
	}

	/**
	 * Gets the selected body part.
	 * @return The selected body part.
	 */
	internal int getSelectedPart() =>
		_selectedPart;
}
