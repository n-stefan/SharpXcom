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

struct OpenSetEntry
{
    internal int _cost;
    internal PathfindingNode _node;
}

/**
 * Helper class to compare entries through pointers.
 */
class EntryCompare : IComparer<OpenSetEntry>
{
    /**
	 * Compares entries @a *a and @a *b.
	 * @param a Pointer to first entry.
	 * @param b Pointer to second entry.
	 * @return True if entry @a *b must come before @a *a.
	 */
    public int Compare(OpenSetEntry a, OpenSetEntry b) =>
        a._cost.CompareTo(b._cost);
}

/**
 * A class that holds references to the nodes to be examined in pathfinding.
 */
internal class PathfindingOpenSet
{
    PriorityQueue<OpenSetEntry, EntryCompare> _queue;

    /**
     * Cleans up all the entries still in set.
     */
    ~PathfindingOpenSet() =>
        _queue.Clear();

	/**
	 * Places the node in the set.
	 * If the node was already in the set, the previous entry is discarded.
	 * It is the caller's responsibility to never re-add a node with a worse cost.
	 * @param node A pointer to the node to add.
	 */
	internal void push(PathfindingNode node)
	{
		OpenSetEntry entry = new OpenSetEntry();
		entry._node = node;
		entry._cost = node.getTUCost(false) + node.getTUGuess();
		if (node._openentry != null)
			node._openentry.Value._node = 0;
		node._openentry = entry;
		_queue.Enqueue(entry, new EntryCompare());
	}

	/**
	 * Gets the node with the least cost.
	 * After this call, the node is no longer in the set. It is an error to call this when the set is empty.
	 * @return A pointer to the node which had the least cost.
	 */
	internal PathfindingNode pop()
	{
		Debug.Assert(!empty());
		OpenSetEntry entry = _queue.Peek();
		PathfindingNode nd = entry._node;
		_queue.Dequeue();
		entry = default;
		nd._openentry = null;

		// Discarded entries might be visible now.
		removeDiscarded();
		return nd;
	}

	/// Is the set empty?
	internal bool empty() =>
		_queue.Count == 0;

	/**
	 * Keeps removing all discarded entries that have come to the top of the queue.
	 */
	void removeDiscarded()
	{
		while (_queue.Count != 0 && _queue.Peek()._node == null)
		{
			OpenSetEntry entry = _queue.Peek();
			_queue.Dequeue();
			entry = default;
		}
	}
}
