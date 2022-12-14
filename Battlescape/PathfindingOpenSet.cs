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
    PathfindingNode _node;
};

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
}
