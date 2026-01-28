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
    internal OpenSetEntry _openentry;

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
        _openentry = null;
    }

    /**
     * Deletes the PathfindingNode.
     */
    ~PathfindingNode() { }

    /**
     * Gets the TU cost.
     * @param missile Is this a missile?
     * @return The TU cost.
     */
    internal int getTUCost(bool missile)
    {
	    if (missile)
		    return 0;
	    else
		    return _tuCost;
    }

    /**
     * Gets the previous node.
     * @return Pointer to the previous node.
     */
    internal PathfindingNode getPrevNode() =>
	    _prevNode;

    /**
     * Gets the previous walking direction for how we got on this node.
     * @return Previous vector.
     */
    internal int getPrevDir() =>
	    _prevDir;

    /**
     * Gets the node position.
     * @return Node position.
     */
    internal Position getPosition() =>
	    _pos;

    /**
     * Gets the checked status of this node.
     * @return True, if this node was checked.
     */
    internal bool isChecked() =>
	    _checked;

	/// Marks the node as checked.
	internal void setChecked() =>
        _checked = true;

    /**
     * Connects the node. This will connect the node to the previous node along the path to @a target
     * and update the pathfinding information.
     * @param tuCost The total cost of the path so far.
     * @param prevNode The previous node along the path.
     * @param prevDir The direction FROM the previous node.
     * @param target The target position (used to update our guess cost).
     */
    internal void connect(int tuCost, PathfindingNode prevNode, int prevDir, Position target)
    {
	    _tuCost = tuCost;
	    _prevNode = prevNode;
	    _prevDir = prevDir;
	    if (!inOpenSet()) // Otherwise we have this already.
	    {
		    Position d = target - _pos;
		    d *= d;
		    _tuGuess = (int)(4 * Math.Sqrt((double)(d.x + d.y + d.z)));
	    }
    }

	/// Is this node already in a PathfindingOpenSet?
	internal bool inOpenSet() =>
        _openentry != null;

    /**
     * Resets the node.
     */
    internal void reset()
    {
	    _checked = false;
	    _openentry = null;
    }

	/// Gets the approximate cost to reach the target position.
	internal int getTUGuess() =>
        _tuGuess;

    /**
     * Connects the node. This will connect the node to the previous node along the path.
     * @param tuCost The total cost of the path so far.
     * @param prevNode The previous node along the path.
     * @param prevDir The direction FROM the previous node.
     */
    internal void connect(int tuCost, PathfindingNode prevNode, int prevDir)
    {
	    _tuCost = tuCost;
	    _prevNode = prevNode;
	    _prevDir = prevDir;
	    _tuGuess = 0;
    }
}
