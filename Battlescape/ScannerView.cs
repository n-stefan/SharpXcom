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

	/**
	 * Draws the ScannerView view.
	 */
	protected override void draw()
	{
		SurfaceSet set = _game.getMod().getSurfaceSet("DETBLOB.DAT");
		Surface surface = null;

		clear();

		this.@lock();
		for (int x = -9; x < 10; x++)
		{
			for (int y = -9; y < 10; y++)
			{
				for (int z = 0; z < _game.getSavedGame().getSavedBattle().getMapSizeZ(); z++)
				{
					Tile t = _game.getSavedGame().getSavedBattle().getTile(new Position(x,y,z) + new Position(_unit.getPosition().x, _unit.getPosition().y, 0));
					if (t != null && t.getUnit() != null && t.getUnit().getMotionPoints() != 0)
					{
						int frame = (t.getUnit().getMotionPoints() / 5);
						if (frame >= 0)
						{
							if (frame > 5) frame = 5;
							surface = set.getFrame(frame + _frame);
							surface.blitNShade(this, getX()+((9+x)*8)-4, getY()+((9+y)*8)-4, 0);
						}
					}
				}
			}
		}

		// the arrow of the direction the unit is pointed
		surface = set.getFrame(7 + _unit.getDirection());

		surface.blitNShade(this, getX()+(9*8)-4, getY()+(9*8)-4, 0);
		this.unlock();
	}

	/**
	 * Handles clicks on the scanner view.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void mouseClick(Action _, State __)
	{
	}
}
