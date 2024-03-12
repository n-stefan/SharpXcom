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
 * Represents a country that funds the player.
 * Contains variable info about a country like
 * monthly funding and various activities.
 */
internal class Country
{
    RuleCountry _rules;
    bool _pact, _newPact;
    List<int> _funding, _activityXcom, _activityAlien;
    int _satisfaction;

    /**
     * Initializes a country of the specified type.
     * @param rules Pointer to ruleset.
     * @param gen Generate new funding.
     */
    internal Country(RuleCountry rules, bool gen = true)
    {
        _rules = rules;
        _pact = false;
        _newPact = false;
        _funding = null;
        _satisfaction = 2;

        if (gen)
        {
            _funding.Add(_rules.generateFunding());
        }
        _activityAlien.Add(0);
        _activityXcom.Add(0);
    }

    /**
     *
     */
    ~Country() { }

    /**
     * Saves the country to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "type", _rules.getType() },
            { "funding", new YamlSequenceNode(_funding.Select(x => new YamlScalarNode(x.ToString()))) },
            { "activityXcom", new YamlSequenceNode(_activityXcom.Select(x => new YamlScalarNode(x.ToString()))) },
            { "activityAlien", new YamlSequenceNode(_activityAlien.Select(x => new YamlScalarNode(x.ToString()))) }
        };
        if (_pact)
	    {
            node.Add("pact", _pact.ToString());
	    }
	    else if (_newPact)
	    {
		    node.Add("newPact", _newPact.ToString());
	    }
	    return node;
    }

    /**
     * Gets the country's alien activity level.
     * @return activity level.
     */
    internal ref List<int> getActivityAlien() =>
	    ref _activityAlien;

    /**
     * Gets the country's xcom activity level.
     * @return activity level.
     */
    internal ref List<int> getActivityXcom() =>
	    ref _activityXcom;

    /**
     * Returns the country's current monthly funding.
     * @return Monthly funding.
     */
    internal ref List<int> getFunding() =>
	    ref _funding;

    /**
     * sign a new pact.
     */
    internal void setPact() =>
        _pact = true;

    /**
     * sign a new pact at month's end.
     */
    internal void setNewPact() =>
        _newPact = true;

    /**
     * Returns the ruleset for the country's type.
     * @return Pointer to ruleset.
     */
    internal RuleCountry getRules() =>
	    _rules;

    /**
     * Adds to the country's alien activity level.
     * @param activity how many points to add.
     */
    internal void addActivityAlien(int activity) =>
        _activityAlien[^1] += activity;

    /**
     * Adds to the country's xcom activity level.
     * @param activity how many points to add.
     */
    internal void addActivityXcom(int activity) =>
        _activityXcom[^1] += activity;

    /**
     * no setter for this one, as it gets set automatically
     * at month's end if _newPact is set.
     * @return if we have signed a pact.
     */
    internal bool getPact() =>
	    _pact;

    /**
     * @return if we will sign a new pact.
     */
    internal bool getNewPact() =>
	    _newPact;

    /*
     * Keith Richards would be so proud
     * @return satisfaction level, 0 = alien pact, 1 = unhappy, 2 = satisfied, 3 = happy.
     */
    internal int getSatisfaction()
    {
	    if (_pact)
		    return 0;
	    return _satisfaction;
    }

    /**
     * reset all the counters,
     * calculate this month's funding,
     * set the change value for the month.
     * @param xcomTotal the council's xcom score
     * @param alienTotal the council's alien score
     * @param pactScore the penalty for signing a pact
     */
    internal void newMonth(int xcomTotal, int alienTotal, int pactScore)
    {
	    _satisfaction = 2;
	    int funding = getFunding().Last();
	    int good = (xcomTotal / 10) + _activityXcom.Last();
	    int bad = (alienTotal / 20) + _activityAlien.Last();
	    int oldFunding = _funding.Last() / 1000;
	    int newFunding = (oldFunding * RNG.generate(5, 20) / 100) * 1000;

	    if (bad <= good + 30)
	    {
		    if (good > bad + 30)
		    {
			    if (RNG.generate(0, good) > bad)
			    {
				    // don't go over the cap
				    int cap = getRules().getFundingCap()*1000;
				    if (funding + newFunding > cap)
					    newFunding = cap - funding;
				    if (newFunding != 0)
					    _satisfaction = 3;
			    }
		    }
	    }
	    else
	    {
		    if (RNG.generate(0, bad) > good)
		    {
			    if (newFunding != 0)
			    {
				    newFunding = -newFunding;
				    // don't go below zero
				    if (funding + newFunding < 0)
					    newFunding = 0 - funding;
				    if (newFunding != 0)
					    _satisfaction = 1;
			    }
		    }
	    }

	    // about to be in cahoots
	    if (_newPact && !_pact)
	    {
		    _newPact = false;
		    _pact = true;
		    addActivityAlien(pactScore);
	    }

	    // set the new funding and reset the activity meters
	    if (_pact)
		    _funding.Add(0);
	    else if (_satisfaction != 2)
		    _funding.Add(funding + newFunding);
	    else
		    _funding.Add(funding);

	    _activityAlien.Add(0);
	    _activityXcom.Add(0);
	    if (_activityAlien.Count > 12)
		    _activityAlien.RemoveAt(0);
	    if (_activityXcom.Count > 12)
		    _activityXcom.RemoveAt(0);
	    if (_funding.Count > 12)
		    _funding.RemoveAt(0);
    }

    /**
     * Changes the country's current monthly funding.
     * @param funding Monthly funding.
     */
    internal void setFunding(int funding) =>
        _funding[^1] = funding;

    /**
     * Loads the country from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
		_funding = ((YamlSequenceNode)node["funding"]).Children.Select(x => int.Parse(x.ToString())).ToList();
	    _activityXcom = ((YamlSequenceNode)node["activityXcom"]).Children.Select(x => int.Parse(x.ToString())).ToList();
	    _activityAlien = ((YamlSequenceNode)node["activityAlien"]).Children.Select(x => int.Parse(x.ToString())).ToList();
	    _pact = bool.Parse(node["pact"].ToString());
	    _newPact = bool.Parse(node["newPact"].ToString());
    }
}
