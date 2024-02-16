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
 * Class handling camera movement, either by mouse or by events on the battlescape.
 */
internal class Camera
{
	internal const int SCROLL_BORDER = 5;
	internal const int SCROLL_DIAGONAL_EDGE = 60;

    Timer _scrollMouseTimer, _scrollKeyTimer;
    int _spriteWidth, _spriteHeight;
    int _mapsize_x, _mapsize_y, _mapsize_z;
    int _screenWidth, _screenHeight;
    Position _mapOffset, _center;
    int _scrollMouseX, _scrollMouseY, _scrollKeyX, _scrollKeyY;
    bool _scrollTrigger;
    int _visibleMapHeight;
    bool _showAllLayers;
    Map _map;

    /**
     * Sets up a camera.
     * @param spriteWidth Width of map sprite.
     * @param spriteHeight Height of map sprite.
     * @param mapsize_x Current map size in X axis.
     * @param mapsize_y Current map size in Y axis.
     * @param mapsize_z Current map size in Z axis.
     * @param map Pointer to map surface.
     * @param visibleMapHeight Current height the view is at.
     */
    internal Camera(int spriteWidth, int spriteHeight, int mapsize_x, int mapsize_y, int mapsize_z, Map map, int visibleMapHeight)
    {
        _scrollMouseTimer = null;
        _scrollKeyTimer = null;
        _spriteWidth = spriteWidth;
        _spriteHeight = spriteHeight;
        _mapsize_x = mapsize_x;
        _mapsize_y = mapsize_y;
        _mapsize_z = mapsize_z;
        _screenWidth = map.getWidth();
        _screenHeight = map.getHeight();
        _mapOffset = new Position(-250, 250, 0);
        _scrollMouseX = 0;
        _scrollMouseY = 0;
        _scrollKeyX = 0;
        _scrollKeyY = 0;
        _scrollTrigger = false;
        _visibleMapHeight = visibleMapHeight;
        _showAllLayers = false;
        _map = map;
    }

    /**
     * Deletes the Camera.
     */
    ~Camera() { }

    /**
     * Handles scrolling with given deviation.
     * @param x X deviation.
     * @param y Y deviation.
     * @param redraw Redraw map or not.
     */
    internal void scrollXY(int x, int y, bool redraw)
    {
        _mapOffset.x += x;
        _mapOffset.y += y;

        do
        {
            convertScreenToMap((_screenWidth / 2), (_visibleMapHeight / 2), ref _center.x, ref _center.y);

            // Handling map bounds...
            // Ok, this is a prototype, it should be optimized.
            // Actually this should be calculated instead of slow-approximation.
            if (_center.x < 0) { _mapOffset.x -= 1; _mapOffset.y -= 1; continue; }
            if (_center.x > _mapsize_x - 1) { _mapOffset.x += 1; _mapOffset.y += 1; continue; }
            if (_center.y < 0) { _mapOffset.x += 1; _mapOffset.y -= 1; continue; }
            if (_center.y > _mapsize_y - 1) { _mapOffset.x -= 1; _mapOffset.y += 1; continue; }
            break;
        }
        while (true);

        _map.refreshSelectorPosition();
        if (redraw) _map.invalidate();
    }

    /**
     * Handles keyboard-scrolling.
     */
    internal void scrollKey() =>
        scrollXY(_scrollKeyX, _scrollKeyY, true);

    /**
     * Handles mouse-scrolling.
     */
    internal void scrollMouse() =>
        scrollXY(_scrollMouseX, _scrollMouseY, true);

    /**
     * Sets the camera's scrolling timer.
     * @param mouse Pointer to mouse timer.
     * @param key Pointer to key timer.
     */
    internal void setScrollTimer(Timer mouse, Timer key)
    {
        _scrollMouseTimer = mouse;
        _scrollKeyTimer = key;
    }

    /**
     * Converts screen coordinates to map coordinates.
     * @param screenX Screen x position.
     * @param screenY Screen y position.
     * @param mapX Map x position.
     * @param mapY Map y position.
     */
    internal void convertScreenToMap(int screenX, int screenY, ref int mapX, ref int mapY)
    {
	    // add half a tileheight to the mouseposition per layer we are above the floor
	    screenY += (-_spriteWidth/2) + (_mapOffset.z) * ((_spriteHeight + _spriteWidth / 4) / 2);

	    // calculate the actual x/y pixelposition on a diamond shaped map
	    // taking the view offset into account
	    mapY = - screenX + _mapOffset.x + 2 * screenY - 2 * _mapOffset.y;
	    mapX = screenY - _mapOffset.y - mapY / 4 - (_spriteWidth/4);

	    // to get the row&col itself, divide by the size of a tile
	    mapX /= (_spriteWidth / 4);
	    mapY /= _spriteWidth;

	    mapX = Math.Clamp(mapX, -1, _mapsize_x);
	    mapY = Math.Clamp(mapY, -1, _mapsize_y);
    }

    /**
     * Centers map on a certain position.
     * @param mapPos Position to center on.
     * @param redraw Redraw map or not.
     */
    internal void centerOnPosition(Position mapPos, bool redraw = true)
    {
        _center = mapPos;
        _center.x = Math.Clamp(_center.x, -1, _mapsize_x);
        _center.y = Math.Clamp(_center.y, -1, _mapsize_y);
        convertMapToScreen(_center, out Position screenPos);

        _mapOffset.x = -(screenPos.x - (_screenWidth / 2));
        _mapOffset.y = -(screenPos.y - (_visibleMapHeight / 2));

        _mapOffset.z = _center.z;
        if (redraw) _map.draw();
    }

    /**
     * Converts map coordinates X,Y,Z to screen positions X, Y.
     * @param mapPos X,Y,Z coordinates on the map.
     * @param screenPos Screen position.
     */
    internal void convertMapToScreen(Position mapPos, out Position screenPos) =>
        screenPos = new Position
        {
            z = 0, // not used
            x = mapPos.x * (_spriteWidth / 2) - mapPos.y * (_spriteWidth / 2),
            y = mapPos.x * (_spriteWidth / 4) + mapPos.y * (_spriteWidth / 4) - mapPos.z * ((_spriteHeight + _spriteWidth / 4) / 2)
        };

    /**
     * Gets the map offset.
     * @return The map offset.
     */
    internal Position getMapOffset() =>
	    _mapOffset;

    /**
     * Sets the view level.
     * @param viewlevel New view level.
     */
    internal void setViewLevel(int viewlevel)
    {
	    _mapOffset.z = Math.Clamp(viewlevel, 0, _mapsize_z - 1);
	    _map.draw();
    }

    internal void stopMouseScrolling() =>
	    _scrollMouseTimer.stop();

    /**
     * Sets the map offset.
     * @param pos The map offset.
     */
    internal void setMapOffset(Position pos) =>
	    _mapOffset = pos;

    /**
     * Gets the displayed level.
     * @return The displayed layer.
     */
    internal int getViewLevel() =>
	    _mapOffset.z;

    /**
     * Converts voxel coordinates X,Y,Z to screen positions X, Y.
     * @param voxelPos X,Y,Z coordinates of the voxel.
     * @param screenPos Screen position.
     */
    internal void convertVoxelToScreen(Position voxelPos, out Position screenPos)
    {
	    Position mapPosition = new Position(voxelPos.x / 16, voxelPos.y / 16, voxelPos.z / 24);
	    convertMapToScreen(mapPosition, out screenPos);
	    double dx = voxelPos.x - (mapPosition.x * 16);
	    double dy = voxelPos.y - (mapPosition.y * 16);
	    double dz = voxelPos.z - (mapPosition.z * 24);
	    screenPos.x += (int)(dx - dy) + (_spriteWidth/2);
	    screenPos.y += (int)(((_spriteHeight / 2.0)) + (dx / 2.0) + (dy / 2.0) - dz);
	    screenPos.x += _mapOffset.x;
	    screenPos.y += _mapOffset.y;
    }

    /**
     * Checks if map coordinates X,Y,Z are on screen.
     * @param mapPos Coordinates to check.
     * @param unitWalking True to offset coordinates for a unit walking.
     * @param unitSize size of unit (0 - single, 1 - 2x2, etc, used for walking only
     * @param boundary True if it's for caching calculation
     * @return True if the map coordinates are on screen.
     */
    internal bool isOnScreen(Position mapPos, bool unitWalking, int unitSize, bool boundary)
    {
	    Position screenPos;
	    convertMapToScreen(mapPos, out screenPos);
	    int posx = _spriteWidth/2, posy = _spriteHeight - _spriteWidth/4;
	    int sizex = _spriteWidth/2, sizey = _spriteHeight/2;
	    if (unitSize > 0)
	    {
		    posy -= _spriteWidth /4;
		    sizex = _spriteWidth*unitSize;
		    sizey = _spriteWidth*unitSize/2;
	    }
	    screenPos.x += _mapOffset.x + posx;
	    screenPos.y += _mapOffset.y + posy;
	    if (unitWalking)
	    {
    /* pretty hardcoded hack to handle overlapping by icons
    (they are always in the center at the bottom of the screen)
    Free positioned icons would require more complex workaround.
    __________
    |________|
    ||      ||
    || ____ ||
    ||_|XX|_||
    |________|
     */
		    if (boundary) //to make sprite updates even being slightly outside of screen
		    {
			    sizex += _spriteWidth;
			    sizey += _spriteWidth/2;
		    }
		    if ( screenPos.x < 0 - sizex
			    || screenPos.x >= _screenWidth + sizex
			    || screenPos.y < 0 - sizey
			    || screenPos.y >= _screenHeight + sizey ) return false; //totally outside
		    int side = ( _screenWidth - _map.getIconWidth() ) / 2;
		    if ( (screenPos.y < (_screenHeight - _map.getIconHeight()) + sizey) ) return true; //above icons
		    if ( (side > 1) && ( (screenPos.x < side + sizex) || (screenPos.x >= (_screenWidth - side - sizex)) ) ) return true; //at sides (if there are any)
		    return false;
	    }
	    else
	    {
		    return screenPos.x >= 0
			    && screenPos.x <= _screenWidth - 10
			    && screenPos.y >= 0
			    && screenPos.y <= _screenHeight - 10;
	    }
    }

    /**
     * Resizes the viewable window of the camera.
     */
    internal void resize()
    {
	    _screenWidth = _map.getWidth();
	    _screenHeight = _map.getHeight();
	    _visibleMapHeight = _map.getHeight() - _map.getIconHeight();
    }

    /**
     * Handles jumping with given deviation.
     * @param x X deviation.
     * @param y Y deviation.
     */
    internal void jumpXY(int x, int y)
    {
	    _mapOffset.x += x;
	    _mapOffset.y += y;
	    convertScreenToMap((_screenWidth / 2), (_visibleMapHeight / 2), ref _center.x, ref _center.y);
    }

    /**
     * Goes one level up.
     */
    internal void up()
    {
	    if (_mapOffset.z < _mapsize_z - 1)
	    {
		    _mapOffset.z++;
		    _mapOffset.y += _spriteHeight * 3 / 5;
		    _map.draw();
	    }
    }

    /**
     * Goes one level down.
     */
    internal void down()
    {
	    if (_mapOffset.z > 0)
	    {
		    _mapOffset.z--;
		    _mapOffset.y -= _spriteHeight * 3 / 5;
		    _map.draw();
	    }
    }

    /**
     * Toggles showing all map layers.
     * @return New layer setting.
     */
    internal int toggleShowAllLayers()
    {
	    _showAllLayers = !_showAllLayers;
	    return _showAllLayers?2:1;
    }

    /**
     * Checks if the camera is showing all map layers.
     * @return Current layer setting.
     */
    internal bool getShowAllLayers() =>
	    _showAllLayers;

    /**
     * Gets map's center position.
     * @return Map's center position.
     */
    internal Position getCenterPosition()
    {
	    _center.z = _mapOffset.z;
	    return _center;
    }

    /**
     * Handles camera mouse shortcuts.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    internal void mousePress(Action action, State _)
    {
	    if (action.getDetails().button.button == SDL_BUTTON_LEFT && Options.battleEdgeScroll == ScrollType.SCROLL_TRIGGER)
	    {
		    _scrollTrigger = true;
		    mouseOver(action, null);
	    }
	    else if (Options.battleDragScrollButton != SDL_BUTTON_MIDDLE || (SDL_GetMouseState(0,0)&SDL_BUTTON((uint)Options.battleDragScrollButton)) == 0)
	    {
            if (action.getDetails().wheel.y > 0) //button.button == SDL_BUTTON_WHEELUP
		    {
			    up();
		    }
            else if (action.getDetails().wheel.y < 0) //button.button == SDL_BUTTON_WHEELDOWN
		    {
			    down();
		    }
	    }
    }

	/**
	 * Handles mouse over events.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	internal void mouseOver(Action action, State _)
	{
		if (_map.getCursorType() == CursorType.CT_NONE)
		{
			return;
		}

		if (Options.battleEdgeScroll == ScrollType.SCROLL_AUTO || _scrollTrigger)
		{
			int posX = action.getXMouse();
			int posY = action.getYMouse();
			int scrollSpeed = Options.battleScrollSpeed;

			//left scroll
			if (posX < (SCROLL_BORDER * action.getXScale()) && posX >= 0)
			{
				_scrollMouseX = scrollSpeed;
				// if close to top or bottom, also scroll diagonally
				//downleft
				if (posY < (SCROLL_DIAGONAL_EDGE * action.getYScale()) && posY >= 0)
				{
					_scrollMouseY = scrollSpeed/2;
				}
				//upleft
				else if (posY > (_screenHeight - SCROLL_DIAGONAL_EDGE) * action.getYScale())
				{
					_scrollMouseY = -scrollSpeed/2;
				}
				else _scrollMouseY = 0;
			}
			//right scroll
			else if (posX > (_screenWidth - SCROLL_BORDER) * action.getXScale())
			{
				_scrollMouseX = -scrollSpeed;
				// if close to top or bottom, also scroll diagonally
				//downright
				if (posY <= (SCROLL_DIAGONAL_EDGE * action.getYScale()) && posY >= 0)
				{
					_scrollMouseY = scrollSpeed/2;
				}
				//upright
				else if (posY > (_screenHeight - SCROLL_DIAGONAL_EDGE) * action.getYScale())
				{
					_scrollMouseY = -scrollSpeed/2;
				}
				else _scrollMouseY = 0;
			}
			else if (posX != 0)
			{
				_scrollMouseX = 0;
			}

			//up
			if (posY < (SCROLL_BORDER * action.getYScale()) && posY >= 0)
			{
				_scrollMouseY = scrollSpeed;
				// if close to left or right edge, also scroll diagonally
				//up left
				if (posX < (SCROLL_DIAGONAL_EDGE * action.getXScale()) && posX >= 0)
				{
					_scrollMouseX = scrollSpeed;
					_scrollMouseY /=2;
				}
				//up right
				else if (posX > (_screenWidth - SCROLL_DIAGONAL_EDGE) * action.getXScale())
				{
					_scrollMouseX = -scrollSpeed;
					_scrollMouseY /=2;
				}
			}
			//down
			else if (posY > (_screenHeight- SCROLL_BORDER) * action.getYScale())
			{
				_scrollMouseY = -scrollSpeed;
				// if close to left or right edge, also scroll diagonally
				//down left
				if (posX < (SCROLL_DIAGONAL_EDGE * action.getXScale()) && posX >= 0)
				{
					_scrollMouseX = scrollSpeed;
					_scrollMouseY /=2;
				}
				//down right
				else if (posX > (_screenWidth - SCROLL_DIAGONAL_EDGE) * action.getXScale())
				{
					_scrollMouseX = -scrollSpeed;
					_scrollMouseY /=2;
				}
			}
			else if (posY != 0 && _scrollMouseX == 0)
			{
				_scrollMouseY = 0;
			}

			if ((_scrollMouseX != 0 || _scrollMouseY != 0) && !_scrollMouseTimer.isRunning() && !_scrollKeyTimer.isRunning() && 0==(SDL_GetMouseState(0,0)&SDL_BUTTON((uint)Options.battleDragScrollButton)))
			{
				_scrollMouseTimer.start();
			}
			else if ((_scrollMouseX == 0 && _scrollMouseY == 0) && _scrollMouseTimer.isRunning())
			{
				_scrollMouseTimer.stop();
			}
		}
	}

    /**
     * Handles camera mouse shortcuts.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    internal void mouseRelease(Action action, State _)
    {
	    if (action.getDetails().button.button == SDL_BUTTON_LEFT && Options.battleEdgeScroll == ScrollType.SCROLL_TRIGGER)
	    {
		    _scrollMouseX = 0;
		    _scrollMouseY = 0;
		    _scrollMouseTimer.stop();
		    _scrollTrigger = false;
		    int posX = action.getXMouse();
		    int posY = action.getYMouse();
		    if ((posX < (SCROLL_BORDER * action.getXScale()) && posX > 0)
			    || (posX > (_screenWidth - SCROLL_BORDER) * action.getXScale())
			    || (posY < (SCROLL_BORDER * action.getYScale()) && posY > 0)
			    || (posY > (_screenHeight - SCROLL_BORDER) * action.getYScale()))
			    // A cheap hack to avoid handling this event as a click
			    // on the map when the mouse is on the scroll-border
			    action.getDetails().button.button = 0;
	    }
    }

    /**
     * Gets the map size x.
     * @return The map size x.
     */
    internal int getMapSizeX() =>
	    _mapsize_x;

    /**
     * Gets the map size y.
     * @return The map size y.
     */
    internal int getMapSizeY() =>
	    _mapsize_y;

    /**
     * Handles camera keyboard shortcuts.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    internal void keyboardPress(Action action, State _)
    {
	    if (_map.getCursorType() == CursorType.CT_NONE)
	    {
		    return;
	    }

	    var key = action.getDetails().key.keysym.sym;
	    int scrollSpeed = Options.battleScrollSpeed;
	    if (key == Options.keyBattleLeft)
	    {
		    _scrollKeyX = scrollSpeed;
	    }
	    else if (key == Options.keyBattleRight)
	    {
		    _scrollKeyX = -scrollSpeed;
	    }
	    else if (key == Options.keyBattleUp)
	    {
		    _scrollKeyY = scrollSpeed;
	    }
	    else if (key == Options.keyBattleDown)
	    {
		    _scrollKeyY = -scrollSpeed;
	    }

	    if ((_scrollKeyX != 0 || _scrollKeyY != 0) && !_scrollKeyTimer.isRunning() && !_scrollMouseTimer.isRunning() && 0==(SDL_GetMouseState(0,0)&SDL_BUTTON((uint)Options.battleDragScrollButton)))
	    {
		    _scrollKeyTimer.start();
	    }
	    else if ((_scrollKeyX == 0 && _scrollKeyY == 0) && _scrollKeyTimer.isRunning())
	    {
		    _scrollKeyTimer.stop();
	    }
    }

	/**
	 * Handles camera keyboard shortcuts.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	internal void keyboardRelease(Action action, State _)
	{
		if (_map.getCursorType() == CursorType.CT_NONE)
		{
			return;
		}

		var key = action.getDetails().key.keysym.sym;
		if (key == Options.keyBattleLeft)
		{
			_scrollKeyX = 0;
		}
		else if (key == Options.keyBattleRight)
		{
			_scrollKeyX = 0;
		}
		else if (key == Options.keyBattleUp)
		{
			_scrollKeyY = 0;
		}
		else if (key == Options.keyBattleDown)
		{
			_scrollKeyY = 0;
		}

		if ((_scrollKeyX != 0 || _scrollKeyY != 0) && !_scrollKeyTimer.isRunning() && !_scrollMouseTimer.isRunning() && 0==(SDL_GetMouseState(0,0)&SDL_BUTTON((uint)Options.battleDragScrollButton)))
		{
			_scrollKeyTimer.start();
		}
		else if ((_scrollKeyX == 0 && _scrollKeyY == 0) && _scrollKeyTimer.isRunning())
		{
			_scrollKeyTimer.stop();
		}
	}
}
