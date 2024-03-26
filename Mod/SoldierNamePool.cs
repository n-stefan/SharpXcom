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

namespace SharpXcom.Mod;

/**
 * Pool of soldier names to generate random names.
 * Each pool contains a set of first names (male or female) and last names.
 * The first names define the soldier's gender, and are randomly associated
 * with a last name.
 */
internal class SoldierNamePool
{
    List<string> _maleFirst, _femaleFirst, _maleLast, _femaleLast;
    List<int> _lookWeights;
    int _totalWeight, _femaleFrequency;

    /**
     * Initializes a new pool with blank lists of names.
     */
    internal SoldierNamePool()
    {
        _totalWeight = 0;
        _femaleFrequency = -1;
    }

    /**
     *
     */
    ~SoldierNamePool() { }

    /**
     * Generates an int representing the index of the soldier's look, when passed the maximum index value.
     * @param numLooks The maximum index.
     * @return The index of the soldier's look.
     */
    internal uint genLook(uint numLooks)
    {
        int look = 0;
        const int minimumChance = 2;    // minimum chance of a look being selected if it isn't enumerated. This ensures that looks MUST be zeroed to not appear.

        while (_lookWeights.Count < numLooks)
        {
            _lookWeights.Add(minimumChance);
            _totalWeight += minimumChance;
        }
        while (_lookWeights.Count > numLooks)
        {
            _totalWeight -= _lookWeights.Last();
            _lookWeights.RemoveAt(_lookWeights.Count - 1);
        }

        int random = RNG.generate(0, _totalWeight);
        foreach (var lookWeight in _lookWeights)
        {
            if (random <= lookWeight)
            {
                return (uint)look;
            }
            random -= lookWeight;
            ++look;
        }

        return (uint)RNG.generate(0, (int)(numLooks - 1));
    }

    /**
     * Returns a new random name (first + last) from the
     * lists of names contained within.
     * @param gender Returned gender of the name.
     * @return The soldier's name.
     */
    internal string genName(SoldierGender gender, int femaleFrequency)
    {
	    string name;
	    bool female;
	    if (_femaleFrequency > -1)
	    {
		    female = RNG.percent(_femaleFrequency);
	    }
	    else
	    {
		    female = RNG.percent(femaleFrequency);
	    }

	    if (!female)
	    {
		    gender = SoldierGender.GENDER_MALE;
            int first = RNG.generate(0, _maleFirst.Count - 1);
		    name = _maleFirst[first];
		    if (_maleLast.Any())
		    {
                int last = RNG.generate(0, _maleLast.Count - 1);
			    name = $"{name} {_maleLast[last]}";
		    }
	    }
	    else
	    {
		    gender = SoldierGender.GENDER_FEMALE;
            int first = RNG.generate(0, _femaleFirst.Count - 1);
		    name = _femaleFirst[first];
		    if (_femaleLast.Any())
		    {
                int last = RNG.generate(0, _femaleLast.Count - 1);
			    name = $"{name} {_femaleLast[last]}";
		    }
	    }
	    return name;
    }

	/**
	 * Loads the pool from a YAML file.
	 * @param filename YAML file.
	 */
	internal void load(string filename)
	{
        using var input = new StreamReader(filename);
        var yaml = new YamlStream();
        yaml.Load(input);
        YamlMappingNode doc = (YamlMappingNode)yaml.Documents[0].RootNode;

		foreach (var i in ((YamlSequenceNode)doc["maleFirst"]).Children)
		{
			string name = i.ToString();
			_maleFirst.Add(name);
		}
		foreach (var i in ((YamlSequenceNode)doc["femaleFirst"]).Children)
		{
			string name = i.ToString();
			_femaleFirst.Add(name);
		}
		foreach (var i in ((YamlSequenceNode)doc["maleLast"]).Children)
		{
			string name = i.ToString();
			_maleLast.Add(name);
		}
		foreach (var i in ((YamlSequenceNode)doc["femaleLast"]).Children)
		{
			string name = i.ToString();
			_femaleLast.Add(name);
		}
		if (!_femaleFirst.Any())
		{
			_femaleFirst = _maleFirst;
		}
		if (!_femaleLast.Any())
		{
			_femaleLast = _maleLast;
		}
        _lookWeights = ((YamlSequenceNode)doc["lookWeights"]).Children.Select(x => int.Parse(x.ToString())).ToList();
		_totalWeight = 0;
		foreach (var i in _lookWeights)
		{
			_totalWeight += i;
		}
		_femaleFrequency = int.Parse(doc["femaleFrequency"].ToString());
	}
}
