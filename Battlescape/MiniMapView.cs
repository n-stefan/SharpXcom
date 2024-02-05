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
	const int CELL_WIDTH = 4;
	const int CELL_HEIGHT = 4;
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
	Position _posBeforeMouseScrolling, _cursorPosition;

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

	/**
	 * Draws the minimap.
	 */
	protected override void draw()
	{
		int _startX = _camera.getCenterPosition().x - ((getWidth() / CELL_WIDTH) / 2);
		int _startY = _camera.getCenterPosition().y - ((getHeight() / CELL_HEIGHT) / 2);

		base.draw();
		if (_set == null)
		{
			return;
		}
		drawRect(0, 0, (short)getWidth(), (short)getHeight(), 15);
		this.@lock();
		for (int lvl = 0; lvl <= _camera.getCenterPosition().z; lvl++)
		{
			int py = _startY;
			for (int y = getY(); y < getHeight() + getY(); y += CELL_HEIGHT)
			{
				int px = _startX;
				for (int x = getX(); x < getWidth() + getX(); x += CELL_WIDTH)
				{
					var p = new Position(px, py, lvl);
					Tile t = _battleGame.getTile(p);
					if (t == null)
					{
						px++;
						continue;
					}
					for (var i = TilePart.O_FLOOR; i <= TilePart.O_OBJECT; i++)
					{
						MapData data = t.getMapData((TilePart)i);

						if (data != null && data.getMiniMapIndex() != 0)
						{
							Surface s = _set.getFrame(data.getMiniMapIndex() + 35);
							if (s != null)
							{
								int shade = 16;
								if (t.isDiscovered(2))
								{
									shade = t.getShade();
									if (shade > 7) shade = 7; //vanilla
								}
								s.blitNShade(this, x, y, shade);
							}
						}
					}
					// alive units
					if (t.getUnit() != null && t.getUnit().getVisible())
					{
						int frame = t.getUnit().getMiniMapSpriteIndex();
						int size = t.getUnit().getArmor().getSize();
						frame += (t.getPosition().y - t.getUnit().getPosition().y) * size;
						frame += t.getPosition().x - t.getUnit().getPosition().x;
						frame += _frame * size * size;
						Surface s = _set.getFrame(frame);
						if (size > 1 && t.getUnit().getFaction() == UnitFaction.FACTION_NEUTRAL)
						{
							s.blitNShade(this, x, y, 0, false, Pathfinding.red);
						}
						else
						{
							s.blitNShade(this, x, y, 0);
						}
					}
					// perhaps (at least one) item on this tile?
					if (t.isDiscovered(2) && t.getInventory().Any())
					{
						int frame = 9 + _frame;
						Surface s = _set.getFrame(frame);
						s.blitNShade(this, x, y, 0);
					}

					px++;
				}
				py++;
			}
		}
		this.unlock();
		int centerX = getWidth() / 2 - 1;
		int centerY = getHeight() / 2 - 1;
		byte color = (byte)(1 + _frame * 3);
		int xOffset = CELL_WIDTH / 2;
		int yOffset = CELL_HEIGHT / 2;
		drawLine((short)(centerX - CELL_WIDTH), (short)(centerY - CELL_HEIGHT),
             (short)(centerX - xOffset), (short)(centerY - yOffset),
			 color); // top left
		drawLine((short)(centerX + xOffset), (short)(centerY - yOffset),
             (short)(centerX + CELL_WIDTH), (short)(centerY - CELL_HEIGHT),
			 color); // top right
		drawLine((short)(centerX - CELL_WIDTH), (short)(centerY + CELL_HEIGHT),
             (short)(centerX - xOffset), (short)(centerY + yOffset),
			 color); // bottom left
		drawLine((short)(centerX + CELL_WIDTH), (short)(centerY + CELL_HEIGHT),
             (short)(centerX + xOffset), (short)(centerY + yOffset),
			 color); //bottom right
	}

	/**
	 * Handles mouse presses on the minimap. Enters mouse-moving mode when the right button is used.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void mousePress(Action action, State state)
	{
		base.mousePress(action, state);

		if (action.getDetails().button.button == Options.battleDragScrollButton)
		{
			_isMouseScrolling = true;
			_isMouseScrolled = false;
			SDL_GetMouseState(out _xBeforeMouseScrolling, out _yBeforeMouseScrolling);
			_posBeforeMouseScrolling = _camera.getCenterPosition();
			if (!Options.battleDragScrollInvert && _cursorPosition.z == 0)
			{
				_cursorPosition.x = action.getDetails().motion.x;
				_cursorPosition.y = action.getDetails().motion.y;
				// the Z is irrelevant to our mouse position, but we can use it as a boolean to check if the position is set or not
				_cursorPosition.z = 1;
			}
			_mouseScrollX = 0; _mouseScrollY = 0;
			_totalMouseMoveX = 0; _totalMouseMoveY = 0;
			_mouseMovedOverThreshold = false;
			_mouseScrollingStartTime = SDL_GetTicks();
		}
	}
}
