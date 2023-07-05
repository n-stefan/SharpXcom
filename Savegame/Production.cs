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

enum productionProgress_e { PROGRESS_NOT_COMPLETE, PROGRESS_COMPLETE, PROGRESS_NOT_ENOUGH_MONEY, PROGRESS_NOT_ENOUGH_MATERIALS, PROGRESS_MAX, PROGRESS_CONSTRUCTION };

internal class Production
{
    RuleManufacture _rules;
    int _amount;
    bool _infinite;
    int _timeSpent;
    int _engineers;
    bool _sell;

    internal Production(RuleManufacture rules, int amount)
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

    internal int getAssignedEngineers() =>
	    _engineers;

    internal int getTimeSpent() =>
	    _timeSpent;

    internal int getAmountTotal() =>
	    _amount;

    internal bool getInfiniteAmount() =>
	    _infinite;

    internal bool getSellItems() =>
	    _sell;

    internal RuleManufacture getRules() =>
	    _rules;

    internal void setAssignedEngineers(int engineers) =>
        _engineers = engineers;

    internal void setTimeSpent(int done) =>
        _timeSpent = done;

	internal productionProgress_e step(Base b, SavedGame g, Mod.Mod m)
	{
		int done = getAmountProduced();
		_timeSpent += _engineers;

		if (done < getAmountProduced())
		{
			int produced;
			if (!getInfiniteAmount())
			{
				produced = Math.Min(getAmountProduced(), _amount) - done; // Math.Min is required because we don't want to overproduce
            }
			else
			{
				produced = getAmountProduced() - done;
			}
			int count = 0;
			do
			{
				foreach (var i in _rules.getProducedItems())
				{
					if (_rules.getCategory() == "STR_CRAFT")
					{
						Craft craft = new Craft(m.getCraft(i.Key, true), b, g.getId(i.Key));
						craft.setStatus("STR_REFUELLING");
						b.getCrafts().Add(craft);
						break;
					}
					else
					{
						if (m.getItem(i.Key, true).getBattleType() == BattleType.BT_NONE)
						{
							foreach (var c in b.getCrafts())
							{
								c.reuseItem(i.Key);
							}
						}
						if (getSellItems())
							g.setFunds(g.getFunds() + (m.getItem(i.Key, true).getSellCost() * i.Value));
						else
							b.getStorageItems().addItem(i.Key, i.Value);
					}
				}
				count++;
				if (count < produced)
				{
					// We need to ensure that player has enough cash/item to produce a new unit
					if (!haveEnoughMoneyForOneMoreUnit(g)) return productionProgress_e.PROGRESS_NOT_ENOUGH_MONEY;
					if (!haveEnoughMaterialsForOneMoreUnit(b, m)) return productionProgress_e.PROGRESS_NOT_ENOUGH_MATERIALS;
					startItem(b, g, m);
				}
			}
			while (count < produced);
		}
		if (getAmountProduced() >= _amount && !getInfiniteAmount()) return productionProgress_e.PROGRESS_COMPLETE;
		if (done < getAmountProduced())
		{
			// We need to ensure that player has enough cash/item to produce a new unit
			if (!haveEnoughMoneyForOneMoreUnit(g)) return productionProgress_e.PROGRESS_NOT_ENOUGH_MONEY;
			if (!haveEnoughMaterialsForOneMoreUnit(b, m)) return productionProgress_e.PROGRESS_NOT_ENOUGH_MATERIALS;
			startItem(b, g, m);
		}
		return productionProgress_e.PROGRESS_NOT_COMPLETE;
	}

	internal int getAmountProduced()
	{
		if (_rules.getManufactureTime() > 0)
			return _timeSpent / _rules.getManufactureTime();
		else
			return _amount;
	}

	bool haveEnoughMoneyForOneMoreUnit(SavedGame g) =>
		_rules.haveEnoughMoneyForOneMoreUnit(g.getFunds());

	bool haveEnoughMaterialsForOneMoreUnit(Base b, Mod.Mod m)
	{
		foreach (var iter in _rules.getRequiredItems())
		{
			if (m.getItem(iter.Key) != null && b.getStorageItems().getItem(iter.Key) < iter.Value)
				return false;
			else if (m.getCraft(iter.Key) != null && b.getCraftCount(iter.Key) < iter.Value)
				return false;
		}
		return true;
	}

	internal void startItem(Base b, SavedGame g, Mod.Mod m)
	{
		g.setFunds(g.getFunds() - _rules.getManufactureCost());
		foreach (var iter in _rules.getRequiredItems())
		{
			if (m.getItem(iter.Key) != null)
			{
				b.getStorageItems().removeItem(iter.Key, iter.Value);
			}
			else if (m.getCraft(iter.Key) != null)
			{
				// Find suitable craft
				foreach (var c in b.getCrafts())
				{
					if (c.getRules().getType() == iter.Key)
					{
						Craft craft = c;
						b.removeCraft(craft, true);
                        break;
					}
				}
			}
		}
	}

    internal void setInfiniteAmount(bool inf) =>
        _infinite = inf;

    internal void setAmountTotal(int amount) =>
        _amount = amount;

    internal void setSellItems(bool sell) =>
        _sell = sell;
}
