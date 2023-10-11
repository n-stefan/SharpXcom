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
 * MiniMapView is the class used to display the map in the MiniMapState.
 */
internal class MiniMapView : InteractiveSurface
{
	const int MAX_FRAME = 2;

	Game _game;
	Camera _camera;
	SavedBattleGame _battleGame;
	int _frame;
	// these two are required for right-button scrolling on the minimap
	bool _isMouseScrolling;
	bool _isMouseScrolled;
	int _xBeforeMouseScrolling, _yBeforeMouseScrolling;
	int _mouseScrollX, _mouseScrollY;
	uint _mouseScrollingStartTime;
	int _totalMouseMoveX, _totalMouseMoveY;
	bool _mouseMovedOverThreshold;
	SurfaceSet _set;

	/**
	 * Initializes all the elements in the MiniMapView.
	 * @param w The MiniMapView width.
	 * @param h The MiniMapView height.
	 * @param x The MiniMapView x origin.
	 * @param y The MiniMapView y origin.
	 * @param game Pointer to the core game.
	 * @param camera The Battlescape camera.
	 * @param battleGame Pointer to the SavedBattleGame.
	 */
	internal MiniMapView(int w, int h, int x, int y, Game game, Camera camera, SavedBattleGame battleGame) : base(w, h, x, y)
	{
		_game = game;
		_camera = camera;
		_battleGame = battleGame;
		_frame = 0;
		_isMouseScrolling = false;
		_isMouseScrolled = false;
		_xBeforeMouseScrolling = 0;
		_yBeforeMouseScrolling = 0;
		_mouseScrollX = 0;
		_mouseScrollY = 0;
		_mouseScrollingStartTime = 0;
		_totalMouseMoveX = 0;
		_totalMouseMoveY = 0;
		_mouseMovedOverThreshold = false;

		_set = _game.getMod().getSurfaceSet("SCANG.DAT");
	}

	/**
	 * Increments the displayed level.
	 * @return New display level.
	 */
	internal int up()
	{
		_camera.setViewLevel(_camera.getViewLevel()+1);
		_redraw = true;
		return _camera.getViewLevel();
	}

	/**
	 * Decrements the displayed level.
	 * @return New display level.
	 */
	internal int down()
	{
		_camera.setViewLevel(_camera.getViewLevel()-1);
		_redraw = true;
		return _camera.getViewLevel();
	}

	/**
	 * Updates the minimap animation.
	 */
	internal void animate()
	{
		_frame++;
		if (_frame > MAX_FRAME)
		{
			_frame = 0;
		}
		_redraw = true;
	}
}
