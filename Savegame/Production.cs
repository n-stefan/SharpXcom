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

internal class Production
{
    RuleManufacture _rules;
    int _amount;
    bool _infinite;
    int _timeSpent;
    int _engineers;
    bool _sell;

    Production(RuleManufacture rules, int amount)
    {
        _rules = rules;
        _amount = amount;
        _infinite = false;
        _timeSpent = 0;
        _engineers = 0;
        _sell = false;
    }

    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "item", getRules().getName() },
            { "assigned", getAssignedEngineers().ToString() },
            { "spent", getTimeSpent().ToString() },
            { "amount", getAmountTotal().ToString() },
            { "infinite", getInfiniteAmount().ToString() }
        };
        if (getSellItems())
		    node.Add("sell", getSellItems().ToString());
        return node;
    }

    int getAssignedEngineers() =>
	    _engineers;

    int getTimeSpent() =>
	    _timeSpent;

    int getAmountTotal() =>
	    _amount;

    bool getInfiniteAmount() =>
	    _infinite;

    bool getSellItems() =>
	    _sell;

    RuleManufacture getRules() =>
	    _rules;
}
