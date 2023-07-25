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
    void scrollXY(int x, int y, bool redraw)
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
    void convertMapToScreen(Position mapPos, out Position screenPos) =>
        screenPos = new Position
        {
            z = 0, // not used
            x = mapPos.x * (_spriteWidth / 2) - mapPos.y * (_spriteWidth / 2),
            y = mapPos.x * (_spriteWidth / 4) + mapPos.y * (_spriteWidth / 4) - mapPos.z * ((_spriteHeight + _spriteWidth / 4) / 2)
        };
}
