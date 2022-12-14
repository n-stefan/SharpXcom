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
 * A class that holds pathfinding info for a certain node on the map.
 */
internal class PathfindingNode
{
    Position _pos;
    bool _checked;
    int _tuCost;
    PathfindingNode _prevNode;
    int _prevDir;
    /// Approximate cost to reach goal position.
    int _tuGuess;
    // Invasive field needed by PathfindingOpenSet
    OpenSetEntry _openentry;

    /**
     * Sets up a PathfindingNode.
     * @param pos Position.
     */
    internal PathfindingNode(Position pos)
    {
        _pos = pos;
        _checked = false;
        _tuCost = 0;
        _prevNode = null;
        _prevDir = 0;
        _tuGuess = 0;
        _openentry = default;
    }

    /**
     * Deletes the PathfindingNode.
     */
    ~PathfindingNode() { }
}
