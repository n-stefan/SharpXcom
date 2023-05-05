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

enum TransferType { TRANSFER_ITEM, TRANSFER_CRAFT, TRANSFER_SOLDIER, TRANSFER_SCIENTIST, TRANSFER_ENGINEER };

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
    internal Transfer(int hours)
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

    /**
     * Changes the craft being transferred.
     * @param craft Pointer to craft.
     */
    internal void setCraft(Craft craft) =>
        _craft = craft;

    /**
     * Changes the items being transferred.
     * @param id Item identifier.
     * @param qty Item quantity.
     */
    internal void setItems(string id, int qty = 1)
    {
	    _itemId = id;
	    _itemQty = qty;
    }

    /**
     * Changes the soldier being transferred.
     * @param soldier Pointer to soldier.
     */
    internal void setSoldier(Soldier soldier) =>
        _soldier = soldier;

    /**
     * Changes the scientists being transferred.
     * @param scientists Amount of scientists.
     */
    internal void setScientists(int scientists) =>
        _scientists = scientists;

    /**
     * Changes the engineers being transferred.
     * @param engineers Amount of engineers.
     */
    internal void setEngineers(int engineers) =>
        _engineers = engineers;

    /**
     * Get a pointer to the soldier being transferred.
     * @return a pointer to the soldier being moved.
     */
    internal Soldier getSoldier() =>
        _soldier;

    /**
     * Gets the craft being transferred.
     * @return a Pointer to craft.
     */
    internal Craft getCraft() =>
        _craft;

    /**
     * Returns the type of the contents of the transfer.
     * @return TransferType.
     */
    internal TransferType getType()
    {
	    if (_soldier != null)
	    {
		    return TransferType.TRANSFER_SOLDIER;
	    }
	    else if (_craft != null)
	    {
		    return TransferType.TRANSFER_CRAFT;
	    }
	    else if (_scientists != 0)
	    {
		    return TransferType.TRANSFER_SCIENTIST;
	    }
	    else if (_engineers != 0)
	    {
		    return TransferType.TRANSFER_ENGINEER;
	    }
	    return TransferType.TRANSFER_ITEM;
    }

    /**
     * Returns the quantity of items in the transfer.
     * @return Amount of items.
     */
    internal int getQuantity()
    {
	    if (_itemQty != 0)
	    {
		    return _itemQty;
	    }
	    else if (_scientists != 0)
	    {
		    return _scientists;
	    }
	    else if (_engineers != 0)
	    {
		    return _engineers;
	    }
	    return 1;
    }

    /**
     * Advances the transfer and takes care of
     * the delivery once it's arrived.
     * @param base Pointer to destination base.
     */
    internal void advance(Base @base)
    {
        _hours--;
        if (_hours <= 0)
        {
            if (_soldier != null)
            {
                @base.getSoldiers().Add(_soldier);
            }
            else if (_craft != null)
            {
                @base.getCrafts().Add(_craft);
                _craft.setBase(@base);
                _craft.checkup();
            }
            else if (_itemQty != 0)
            {
                @base.getStorageItems().addItem(_itemId, _itemQty);
            }
            else if (_scientists != 0)
            {
                @base.setScientists(@base.getScientists() + _scientists);
            }
            else if (_engineers != 0)
            {
                @base.setEngineers(@base.getEngineers() + _engineers);
            }
            _delivered = true;
        }
    }

    /**
     * Returns the time remaining until the
     * transfer arrives at its destination.
     * @return Amount of hours.
     */
    internal int getHours() =>
	    _hours;

    /**
     * Returns the items being transferred.
     * @return Item ID.
     */
    internal string getItems() =>
	    _itemId;
}
