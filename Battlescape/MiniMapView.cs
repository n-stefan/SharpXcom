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
	internal override void draw()
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
	internal override void mousePress(Action action, State state)
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

	/**
	 * Handles mouse clicks on the minimap. Will change the camera center to the clicked point.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void mouseClick(Action action, State state)
	{
		base.mouseClick(action, state);

		// The following is the workaround for a rare problem where sometimes
		// the mouse-release event is missed for any reason.
		// However if the SDL is also missed the release event, then it is to no avail :(
		// (this part handles the release if it is missed and now an other button is used)
		if (_isMouseScrolling) {
			if (action.getDetails().button.button != Options.battleDragScrollButton
			&& 0==(SDL_GetMouseState(0,0)&SDL_BUTTON((uint)Options.battleDragScrollButton))) { // so we missed again the mouse-release :(
				// Check if we have to revoke the scrolling, because it was too short in time, so it was a click
				if ((!_mouseMovedOverThreshold) && ((int)(SDL_GetTicks() - _mouseScrollingStartTime) <= (Options.dragScrollTimeTolerance)))
					{ _camera.centerOnPosition(_posBeforeMouseScrolling); _redraw = true; }
				_isMouseScrolled = _isMouseScrolling = false;
				stopScrolling(action);
			}
		}

		// Drag-Scroll release: release mouse-scroll-mode
		if (_isMouseScrolling)
		{
			// While scrolling, other buttons are ineffective
			if (action.getDetails().button.button == Options.battleDragScrollButton)
			{
				_isMouseScrolling = false;
				stopScrolling(action);
			}
			else
			{
				return;
			}
			// Check if we have to revoke the scrolling, because it was too short in time, so it was a click
			if ((!_mouseMovedOverThreshold) && ((int)(SDL_GetTicks() - _mouseScrollingStartTime) <= (Options.dragScrollTimeTolerance)))
			{
				_isMouseScrolled = false;
				stopScrolling(action);
				_camera.centerOnPosition(_posBeforeMouseScrolling);
				_redraw = true;
			}
			if (_isMouseScrolled) return;
		}

		if (action.getDetails().button.button == SDL_BUTTON_RIGHT)
		{
			((MiniMapState)state).btnOkClick(action);
		}

		if (action.getDetails().button.button == SDL_BUTTON_LEFT)
		{
			int origX = (int)(action.getRelativeXMouse() / action.getXScale());
			int origY = (int)(action.getRelativeYMouse() / action.getYScale());
			// get offset (in cells) of the click relative to center of screen
			int xOff = (origX / CELL_WIDTH) - ((getWidth() / 2) / CELL_WIDTH);
			int yOff = (origY / CELL_HEIGHT) - ((getHeight() / 2) / CELL_HEIGHT);
			// center the camera on this new position
			int newX = _camera.getCenterPosition().x + xOff;
			int newY = _camera.getCenterPosition().y + yOff;
			_camera.centerOnPosition(new Position(newX,newY,_camera.getViewLevel()));
			_redraw = true;
		}
	}

	void stopScrolling(Action action)
	{
		if (!Options.battleDragScrollInvert)
		{
			SDL_WarpMouseGlobal(_cursorPosition.x, _cursorPosition.y);
			action.setMouseAction(_cursorPosition.x, _cursorPosition.y, getX(), getY());
		}
		// reset our "mouse position stored" flag
		_cursorPosition.z = 0;
	}

	/**
	 * Handles moving into the minimap.
	 * Stops the mouse-scrolling mode, if its left on.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void mouseIn(Action action, State state)
	{
		base.mouseIn(action, state);

		_isMouseScrolling = false;
		setButtonPressed((byte)SDL_BUTTON_RIGHT, false);
	}

	/**
	 * Handles moving over the minimap.
	 * Will change the camera center when the mouse is moved in mouse-moving mode.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void mouseOver(Action action, State state)
	{
		base.mouseOver(action, state);

		if (_isMouseScrolling && action.getDetails().type == SDL_EventType.SDL_MOUSEMOTION)
		{
			// The following is the workaround for a rare problem where sometimes
			// the mouse-release event is missed for any reason.
			// However if the SDL is also missed the release event, then it is to no avail :(
			// (checking: is the dragScroll-mouse-button still pressed?)
			if (0==(SDL_GetMouseState(0,0)&SDL_BUTTON((uint)Options.battleDragScrollButton))) { // so we missed again the mouse-release :(
				// Check if we have to revoke the scrolling, because it was too short in time, so it was a click
				if ((!_mouseMovedOverThreshold) && ((int)(SDL_GetTicks() - _mouseScrollingStartTime) <= (Options.dragScrollTimeTolerance)))
				{
						_camera.centerOnPosition(_posBeforeMouseScrolling);
						_redraw = true;
				}
				_isMouseScrolled = _isMouseScrolling = false;
				stopScrolling(action);
				return;
			}

			_isMouseScrolled = true;

			if (Options.touchEnabled == false)
			{
				// Set the mouse cursor back
				SDL_EventState(SDL_EventType.SDL_MOUSEMOTION, SDL_IGNORE);
				SDL_WarpMouseGlobal(_xBeforeMouseScrolling, _yBeforeMouseScrolling);
				SDL_EventState(SDL_EventType.SDL_MOUSEMOTION, SDL_ENABLE);
			}

			// Check the threshold
			_totalMouseMoveX += action.getDetails().motion.xrel;
			_totalMouseMoveY += action.getDetails().motion.yrel;
			if (!_mouseMovedOverThreshold)
				_mouseMovedOverThreshold = ((Math.Abs(_totalMouseMoveX) > Options.dragScrollPixelTolerance) || (Math.Abs(_totalMouseMoveY) > Options.dragScrollPixelTolerance));

			// Calculate the move
			int newX, newY;
			int scrollX, scrollY;

			if (Options.battleDragScrollInvert)
			{
				scrollX = action.getDetails().motion.xrel;
				scrollY = action.getDetails().motion.yrel;
			}
			else
			{
				scrollX = -action.getDetails().motion.xrel;
				scrollY = -action.getDetails().motion.yrel;
			}
			_mouseScrollX += scrollX;
			_mouseScrollY += scrollY;
			newX = (int)(_posBeforeMouseScrolling.x + _mouseScrollX / action.getXScale() / 4);
			newY = (int)(_posBeforeMouseScrolling.y + _mouseScrollY / action.getYScale() / 4);

			// Keep the limits...
			if (newX < -1 || _camera.getMapSizeX() < newX)
			{
				_mouseScrollX -= scrollX;
				newX = _posBeforeMouseScrolling.x + _mouseScrollX / 4;
			}
			if (newY < -1 || _camera.getMapSizeY() < newY)
			{
				_mouseScrollY -= scrollY;
				newY = _posBeforeMouseScrolling.y + _mouseScrollY / 4;
			}

			// Scrolling
			_camera.centerOnPosition(new Position(newX,newY,_camera.getViewLevel()));
			_redraw = true;

			if (Options.touchEnabled == false)
			{
				// We don't want to see the mouse-cursor jumping :)
				if (Options.battleDragScrollInvert)
				{
					action.getDetails().motion.x = _xBeforeMouseScrolling;
					action.getDetails().motion.y = _yBeforeMouseScrolling;
				}
				else
				{
					var delta = new Position(-scrollX, -scrollY, 0);
					int barWidth = _game.getScreen().getCursorLeftBlackBand();
					int barHeight = _game.getScreen().getCursorTopBlackBand();
					int cursorX = _cursorPosition.x + delta.x;
					int cursorY =_cursorPosition.y + delta.y;
					_cursorPosition.x = Math.Clamp(cursorX, (int)Math.Round(getX() * action.getXScale()) + barWidth, (int)Math.Round((getX() + getWidth()) * action.getXScale()) + barWidth);
					_cursorPosition.y = Math.Clamp(cursorY, (int)Math.Round(getY() * action.getYScale()) + barHeight, (int)Math.Round((getY() + getHeight()) * action.getYScale()) + barHeight);
					action.getDetails().motion.x = _cursorPosition.x;
					action.getDetails().motion.y = _cursorPosition.y;
				}
			}
			_game.getCursor().handle(action);
		}
	}
}
