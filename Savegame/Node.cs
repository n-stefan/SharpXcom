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

namespace SharpXcom.Savegame;

/**
 * Represents a node/spawnpoint in the battlescape, loaded from RMP files.
 * @sa http://www.ufopaedia.org/index.php?title=ROUTES
 */
internal class Node
{
    internal const int TYPE_DANGEROUS = 0x04; // an alien was shot here, stop patrolling to it like an idiot with a death wish

    int _id;
    int _segment;
    int _type;
    int _rank;
    int _flags;
    int _reserved;
    int _priority;
    bool _allocated;
    bool _dummy;
    Position _pos;
    List<int> _nodeLinks;

    Node()
    {
        _id = 0;
        _segment = 0;
        _type = 0;
        _rank = 0;
        _flags = 0;
        _reserved = 0;
        _priority = 0;
        _allocated = false;
        _dummy = false;
    }

    /**
     * Initializes a Node.
     * @param id
     * @param pos
     * @param segment
     * @param type
     * @param rank
     * @param flags
     * @param reserved
     * @param priority
     */
    Node(int id, Position pos, int segment, int type, int rank, int flags, int reserved, int priority)
    {
        _id = id;
        _pos = pos;
        _segment = segment;
        _type = type;
        _rank = rank;
        _flags = flags;
        _reserved = reserved;
        _priority = priority;
        _allocated = false;
        _dummy = false;
    }

    /**
     * clean up node.
     */
    ~Node() { }

    internal void freeNode() =>
        _allocated = false;

    internal bool isDummy() =>
	    _dummy;

    /**
     * Gets the Node's position.
     * @return position
     */
    internal Position getPosition() =>
	    _pos;

    /**
     * Gets the Node's type.
     * @return type
     */
    internal int getType() =>
	    _type;

    internal void setType(int type) =>
        _type = type;

    /**
     * Saves the UFO to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "id", _id.ToString() },
            { "position", _pos.save() },
            //node["segment"] = _segment;
            { "type", _type.ToString() },
            { "rank", _rank.ToString() },
            { "flags", _flags.ToString() },
            { "reserved", _reserved.ToString() },
            { "priority", _priority.ToString() },
            { "allocated", _allocated.ToString() },
            { "links", new YamlSequenceNode(_nodeLinks.Select(x => new YamlScalarNode(x.ToString()))) },
            { "dummy", _dummy.ToString() }
        };
        return node;
    }

    /**
     * Get the node's id
     * @return unique id
     */
    internal int getID() =>
	    _id;
}
