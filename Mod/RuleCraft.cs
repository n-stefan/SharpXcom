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
 * Represents a specific type of craft.
 * Contains constant info about a craft like
 * costs, speed, capacities, consumptions, etc.
 * @sa Craft
 */
internal class RuleCraft : IListOrder, IRule
{
    string _type;
    int _sprite, _marker;
    int _fuelMax, _damageMax, _speedMax, _accel, _weapons, _soldiers, _vehicles, _costBuy, _costRent, _costSell;
    int _repairRate, _refuelRate, _radarRange, _radarChance, _sightRange, _transferTime, _score;
    RuleTerrain _battlescapeTerrainData;
    bool _spacecraft;
    int _listOrder, _maxItems, _maxAltitude;
    List<string> _requires;
    string _refuelItem;
    List<List<int>> _deployment;

    /**
     * Creates a blank ruleset for a certain
     * type of craft.
     * @param type String defining the type.
     */
    RuleCraft(string type)
    {
        _type = type;
        _sprite = -1;
        _marker = -1;
        _fuelMax = 0;
        _damageMax = 0;
        _speedMax = 0;
        _accel = 0;
        _weapons = 0;
        _soldiers = 0;
        _vehicles = 0;
        _costBuy = 0;
        _costRent = 0;
        _costSell = 0;
        _repairRate = 1;
        _refuelRate = 1;
        _radarRange = 672;
        _radarChance = 100;
        _sightRange = 1696;
        _transferTime = 0;
        _score = 0;
        _battlescapeTerrainData = null;
        _spacecraft = false;
        _listOrder = 0;
        _maxItems = 0;
        _maxAltitude = -1;
    }

    public IRule Create(string type) =>
        new RuleCraft(type);

    /**
     *
     */
    ~RuleCraft() =>
        _battlescapeTerrainData = null;

    /**
     * Gets the maximum number of weapons that
     * can be equipped onto the craft.
     * @return The weapon capacity.
     */
    internal uint getWeapons() =>
        (uint)_weapons;

    /**
     * Gets the maximum speed of the craft flying
     * around the Geoscape.
     * @return The speed in knots.
     */
    internal int getMaxSpeed() =>
	    _speedMax;

    /**
     * Returns the globe marker for the craft type.
     * @return Marker sprite, -1 if none.
     */
    internal int getMarker() =>
	    _marker;

    /**
     * Gets the list weight for this research item.
     * @return The list weight.
     */
    public int getListOrder() =>
        _listOrder;

    /**
     * Loads the craft from a YAML file.
     * @param node YAML node.
     * @param mod Mod for the craft.
     * @param modIndex A value that offsets the sounds and sprite values to avoid conflicts.
     * @param listOrder The list weight for this craft.
     */
    internal void load(YamlNode node, Mod mod, int listOrder)
    {
        _type = node["type"].ToString();
        _requires = ((YamlSequenceNode)node["requires"]).Children.Select(x => x.ToString()).ToList();
	    if (node["sprite"] != null)
	    {
            // used in
            // Surface set (baseOffset):
            //   BASEBITS.PCK (33)
            //   INTICON.PCK (11)
            //   INTICON.PCK (0)
            //
            // Final index in surfaceset is `baseOffset + sprite + (sprite > 4 ? modOffset : 0)`
            _sprite = mod.getOffset(int.Parse(node["sprite"].ToString()), 4);
	    }
	    if (node["marker"] != null)
	    {
		    _marker = mod.getOffset(int.Parse(node["marker"].ToString()), 8);
	    }
	    _fuelMax = int.Parse(node["fuelMax"].ToString());
	    _damageMax = int.Parse(node["damageMax"].ToString());
	    _speedMax = int.Parse(node["speedMax"].ToString());
	    _accel = int.Parse(node["accel"].ToString());
	    _weapons = int.Parse(node["weapons"].ToString());
	    _soldiers = int.Parse(node["soldiers"].ToString());
	    _vehicles = int.Parse(node["vehicles"].ToString());
	    _costBuy = int.Parse(node["costBuy"].ToString());
	    _costRent = int.Parse(node["costRent"].ToString());
	    _costSell = int.Parse(node["costSell"].ToString());
	    _refuelItem = node["refuelItem"].ToString();
	    _repairRate = int.Parse(node["repairRate"].ToString());
	    _refuelRate = int.Parse(node["refuelRate"].ToString());
	    _radarRange = int.Parse(node["radarRange"].ToString());
	    _radarChance = int.Parse(node["radarChance"].ToString());
	    _sightRange = int.Parse(node["sightRange"].ToString());
	    _transferTime = int.Parse(node["transferTime"].ToString());
	    _score = int.Parse(node["score"].ToString());
	    if (node["battlescapeTerrainData"] is YamlNode terrain)
	    {
		    RuleTerrain rule = new RuleTerrain(terrain["name"].ToString());
		    rule.load(terrain, mod);
		    _battlescapeTerrainData = rule;
	    }
        foreach (var i in ((YamlSequenceNode)node["deployment"]).Children)
        {
            var deployment = new List<int>();
            foreach (var j in ((YamlSequenceNode)i).Children)
            {
                deployment.Add(int.Parse(j.ToString()));
            }
            _deployment.Add(deployment);
        }
	    _spacecraft = bool.Parse(node["spacecraft"].ToString());
	    _listOrder = int.Parse(node["listOrder"].ToString());
	    if (_listOrder == 0)
	    {
		    _listOrder = listOrder;
	    }
	    _maxAltitude = int.Parse(node["maxAltitude"].ToString());
	    _maxItems = int.Parse(node["maxItems"].ToString());
    }

    /**
     * Gets the cost of rent for a month.
     * @return The cost.
     */
    internal int getRentCost() =>
	    _costRent;

    /**
     * Gets how much damage is removed from the
     * craft while repairing.
     * @return The amount of damage.
     */
    internal int getRepairRate() =>
	    _repairRate;

    /**
     * Gets what item is required while
     * the craft is refuelling.
     * @return The item ID or "" if none.
     */
    internal string getRefuelItem() =>
	    _refuelItem;

    /**
     * Gets the maximum fuel the craft can contain.
     * @return The fuel amount.
     */
    internal int getMaxFuel() =>
	    _fuelMax;

    /**
     * Gets the language string that names
     * this craft. Each craft type has a unique name.
     * @return The craft's name.
     */
    internal string getType() =>
	    _type;

    /**
     * Gets the craft's radar range
     * for detecting UFOs.
     * @return The range in nautical miles.
     */
    internal int getRadarRange() =>
	    _radarRange;

    /// Gets the craft's radar chance.
    internal int getRadarChance() =>
        _radarChance;

    /**
     * Gets how much fuel is added to the
     * craft while refuelling.
     * @return The amount of fuel.
     */
    internal int getRefuelRate() =>
	    _refuelRate;

    /**
     * Gets the craft's sight range
     * for detecting bases.
     * @return The range in nautical miles.
     */
    internal int getSightRange() =>
	    _sightRange;

    /**
     * Gets the maximum number of soldiers that
     * the craft can carry.
     * @return The soldier capacity.
     */
    internal int getSoldiers() =>
	    _soldiers;

    /**
     * Gets the number of points you lose
     * when this craft is destroyed.
     * @return The score in points.
     */
    internal int getScore() =>
	    _score;

    /**
     * If the craft is underwater, it can only dogfight over polygons.
     * TODO: Replace this with its own flag.
     * @return underwater or not
     */
    internal bool isWaterOnly() =>
	    _maxAltitude > -1;

    /**
     * Gets the maximum damage (damage the craft can take)
     * of the craft.
     * @return The maximum damage.
     */
    internal int getMaxDamage() =>
	    _damageMax;

    /**
     * Gets the maximum altitude this craft can dogfight to.
     * @return max altitude (0-4).
     */
    internal int getMaxAltitude() =>
	    _maxAltitude;

    /**
     * Gets the deployment layout for this craft.
     * @return The deployment layout.
     */
    internal List<List<int>> getDeployment() =>
	    _deployment;

    /**
     * Gets the terrain data needed to draw the Craft in the battlescape.
     * @return The terrain data.
     */
    internal RuleTerrain getBattlescapeTerrainData() =>
        _battlescapeTerrainData;
}
