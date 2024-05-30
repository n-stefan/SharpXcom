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

enum NodeRank { NR_SCOUT = 0, NR_XCOM, NR_SOLDIER, NR_NAVIGATOR, NR_LEADER, NR_ENGINEER, NR_MISC1, NR_MEDIC, NR_MISC2 };

/**
 * Represents a node/spawnpoint in the battlescape, loaded from RMP files.
 * @sa http://www.ufopaedia.org/index.php?title=ROUTES
 */
internal class Node
{
    internal const int TYPE_DANGEROUS = 0x04; // an alien was shot here, stop patrolling to it like an idiot with a death wish
    internal const int TYPE_SMALL = 0x02; // large unit can not spawn here when this bit is set
    internal const int TYPE_FLYING = 0x01; // non-flying unit can not spawn here when this bit is set
    internal const int CRAFTSEGMENT = 1000;
    internal const int UFOSEGMENT = 2000;

    /* following data is the order in which certain alien ranks spawn on certain node ranks */
    /* note that they all can fall back to rank 0 nodes - which is scout (outside ufo) */
    internal static int[,] nodeRank = new int[8, 7] {
	    { 4, 3, 5, 8, 7, 2, 0 }, //commander
	    { 4, 3, 5, 8, 7, 2, 0 }, //leader
	    { 5, 4, 3, 2, 7, 8, 0 }, //engineer
	    { 7, 6, 2, 8, 3, 4, 0 }, //medic
	    { 3, 4, 5, 2, 7, 8, 0 }, //navigator
	    { 2, 5, 3, 4, 6, 8, 0 }, //soldier
	    { 2, 5, 3, 4, 6, 8, 0 }, //terrorist
	    { 2, 5, 3, 4, 6, 8, 0 }  }; //also terrorist

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

    internal Node()
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
    internal Node(int id, Position pos, int segment, int type, int rank, int flags, int reserved, int priority)
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
            { "position", Position.encode(_pos) },
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

    /**
     * Get the priority of this spawnpoint.
     * @return priority
     */
    internal int getPriority() =>
	    _priority;

    /**
     * Get the rank of units that can spawn on this node.
     * @return noderank
     */
    internal NodeRank getRank() =>
	    (NodeRank)_rank;

    /// get the node's paths
    internal List<int> getNodeLinks() =>
        _nodeLinks;

    internal void setDummy(bool dummy) =>
        _dummy = dummy;

    /**
     * Gets the Node's segment.
     * @return segment
     */
    internal int getSegment() =>
	    _segment;

    internal bool isTarget() =>
	    _reserved == 5;

    internal bool isAllocated() =>
	    _allocated;

	/// gets "flags" variable, which is really the patrolling desirability value
	internal int getFlags() =>
        _flags;

    internal void allocateNode() =>
	    _allocated = true;

    /**
     * Loads the UFO from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    _id = int.Parse(node["id"].ToString());
        _pos = Position.decode(node["position"]);
	    //_segment = node["segment"].as<int>(_segment);
	    _type = int.Parse(node["type"].ToString());
	    _rank = int.Parse(node["rank"].ToString());
	    _flags = int.Parse(node["flags"].ToString());
	    _reserved = int.Parse(node["reserved"].ToString());
	    _priority = int.Parse(node["priority"].ToString());
	    _allocated = bool.Parse(node["allocated"].ToString());
        _nodeLinks = ((YamlSequenceNode)node["links"]).Children.Select(x => int.Parse(x.ToString())).ToList();
	    _dummy = bool.Parse(node["dummy"].ToString());
    }
}
