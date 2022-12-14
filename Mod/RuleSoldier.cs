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
 * Represents the creation data for an X-COM unit.
 * This info is copied to either Soldier for Geoscape or BattleUnit for Battlescape.
 * @sa Soldier BattleUnit
 */
internal class RuleSoldier : IRule
{
    string _type;
    int _costBuy, _costSalary, _standHeight, _kneelHeight, _floatHeight;
    int _femaleFrequency, _value, _transferTime;
    List<SoldierNamePool> _names;
    UnitStats _minStats, _maxStats, _statCaps;
    List<string> _requires;
    string _armor;
    List<int> _deathSoundMale, _deathSoundFemale;

    /**
     * Creates a blank ruleunit for a certain
     * type of soldier.
     * @param type String defining the type.
     */
    RuleSoldier(string type)
    {
        _type = type;
        _costBuy = 0;
        _costSalary = 0;
        _standHeight = 0;
        _kneelHeight = 0;
        _floatHeight = 0;
        _femaleFrequency = 50;
        _value = 20;
        _transferTime = 0;
    }

    public IRule Create(string type) =>
        new RuleSoldier(type);

    /**
     *
     */
    ~RuleSoldier() =>
        _names.Clear();

    /**
     * Gets the female appearance ratio.
     * @return The percentage ratio.
     */
    internal int getFemaleFrequency() =>
	    _femaleFrequency;

    /**
     * Returns the list of soldier name pools.
     * @return Pointer to soldier name pool list.
     */
    internal List<SoldierNamePool> getNames() =>
	    _names;

    /**
     * Gets the minimum stats for the random stats generator.
     * @return The minimum stats.
     */
    internal UnitStats getMinStats() =>
	    _minStats;

    /**
     * Gets the maximum stats for the random stats generator.
     * @return The maximum stats.
     */
    internal UnitStats getMaxStats() =>
	    _maxStats;

    /**
     * Returns the language string that names
     * this soldier. Each soldier type has a unique name.
     * @return Soldier name.
     */
    internal string getType() =>
	    _type;

    /**
     * Loads the soldier from a YAML file.
     * @param node YAML node.
     * @param mod Mod for the unit.
     */
    internal void load(YamlNode node, Mod mod)
    {
	    _type = node["type"].ToString();
	    // Just in case
	    if (_type == "XCOM")
		    _type = "STR_SOLDIER";
        _requires = ((YamlSequenceNode)node["requires"]).Children.Select(x => x.ToString()).ToList();
        var stats = new UnitStats();
        stats.load(node["minStats"]);
        _minStats.merge(stats);
        stats.load(node["maxStats"]);
        _maxStats.merge(stats);
        stats.load(node["statCaps"]);
        _statCaps.merge(stats);
	    _armor = node["armor"].ToString();
	    _costBuy = int.Parse(node["costBuy"].ToString());
	    _costSalary = int.Parse(node["costSalary"].ToString());
	    _standHeight = int.Parse(node["standHeight"].ToString());
	    _kneelHeight = int.Parse(node["kneelHeight"].ToString());
	    _floatHeight = int.Parse(node["floatHeight"].ToString());
	    _femaleFrequency = int.Parse(node["femaleFrequency"].ToString());
	    _value = int.Parse(node["value"].ToString());
	    _transferTime = int.Parse(node["transferTime"].ToString());

	    mod.loadSoundOffset(_type, _deathSoundMale, node["deathMale"], "BATTLE.CAT");
        mod.loadSoundOffset(_type, _deathSoundFemale, node["deathFemale"], "BATTLE.CAT");

	    foreach (var soldierName in ((YamlSequenceNode)node["soldierNames"]).Children)
	    {
		    string fileName = soldierName.ToString();
		    if (fileName == "delete")
		    {
                _names.Clear();
		    }
		    else
		    {
			    if (fileName[fileName.Length - 1] == '/')
			    {
				    // load all *.nam files in given directory
				    HashSet<string> names = FileMap.filterFiles(FileMap.getVFolderContents(fileName), "nam");
				    foreach (var name in names)
				    {
					    addSoldierNamePool(fileName + name);
				    }
			    }
			    else
			    {
				    // load given file
				    addSoldierNamePool(fileName);
			    }
		    }
	    }
    }

    void addSoldierNamePool(string namFile)
    {
	    SoldierNamePool pool = new SoldierNamePool();
	    pool.load(FileMap.getFilePath(namFile));
	    _names.Add(pool);
    }
}
