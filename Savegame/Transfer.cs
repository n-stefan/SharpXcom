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
 * Represents an item transfer.
 * Items are placed "in transit" whenever they are
 * purchased or transferred between bases.
 */
internal class Transfer
{
    int _hours;
    Soldier _soldier;
    Craft _craft;
    int _itemQty, _scientists, _engineers;
    bool _delivered;
    string _itemId;

    /**
     * Initializes a transfer.
     * @param hours Hours in-transit.
     */
    Transfer(int hours)
    {
        _hours = hours;
        _soldier = null;
        _craft = null;
        _itemQty = 0;
        _scientists = 0;
        _engineers = 0;
        _delivered = false;
    }

    /**
     * Cleans up undelivered transfers.
     */
    ~Transfer()
    {
        if (!_delivered)
        {
            _soldier = null;
            _craft = null;
        }
    }

    /**
     * Saves the transfer to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "hours", _hours.ToString() }
        };
        if (_soldier != null)
	    {
		    node.Add("soldier", _soldier.save());
	    }
	    else if (_craft != null)
	    {
		    node.Add("craft", _craft.save());
	    }
	    else if (_itemQty != 0)
	    {
		    node.Add("itemId", _itemId);
		    node.Add("itemQty", _itemQty.ToString());
	    }
	    else if (_scientists != 0)
	    {
		    node.Add("scientists", _scientists.ToString());
	    }
	    else if (_engineers != 0)
	    {
		    node.Add("engineers", _engineers.ToString());
	    }
	    if (_delivered)
	    {
		    node.Add("delivered", _delivered.ToString());
	    }
        return node;
    }
}
