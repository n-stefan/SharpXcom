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
	/**
	 * User interface string identifier of body parts.
	 */
	static string[] PARTS_STRING =
	{
		"STR_HEAD",
		"STR_TORSO",
		"STR_RIGHT_ARM",
		"STR_LEFT_ARM",
		"STR_RIGHT_LEG",
		"STR_LEFT_LEG"
	};

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

	/**
	 * Draws the medikit view.
	 */
	protected override void draw()
	{
		SurfaceSet set = _game.getMod().getSurfaceSet("MEDIBITS.DAT");
		int fatal_wound = _unit.getFatalWound(_selectedPart);
		string ss, ss1;
		int green = 0;
		int red = 3;
		if (_game.getMod().getInterface("medikit") != null && _game.getMod().getInterface("medikit").getElement("body") != default)
		{
			green = _game.getMod().getInterface("medikit").getElement("body").color;
			red = _game.getMod().getInterface("medikit").getElement("body").color2;
		}
		this.@lock();
		for (int i = 0; i < set.getTotalFrames(); i++)
		{
			int wound = _unit.getFatalWound(i);
			Surface surface = set.getFrame(i);
			int baseColor = wound != 0 ? red : green;
			surface.blitNShade(this, getX(), getY(), 0, false, baseColor);
		}
		this.unlock();

		_redraw = false;
		if (_selectedPart == -1)
		{
			return;
		}
		ss = _game.getLanguage().getString(PARTS_STRING[_selectedPart]);
		ss1 = fatal_wound.ToString();
		_partTxt.setText(ss);
		_woundTxt.setText(ss1);
	}

	/**
	 * Handles clicks on the medikit view.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void mouseClick(Action action, State _)
	{
		SurfaceSet set = _game.getMod().getSurfaceSet("MEDIBITS.DAT");
		int x = (int)(action.getRelativeXMouse() / action.getXScale());
		int y = (int)(action.getRelativeYMouse() / action.getYScale());
		for (int i = 0; i < set.getTotalFrames(); i++)
		{
			Surface surface = set.getFrame(i);
			if (surface.getPixel(x, y) != 0)
			{
				_selectedPart = i;
				_redraw = true;
				break;
			}
		}
	}
}
