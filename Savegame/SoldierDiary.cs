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
 * Each entry will be its own commendation.
 */
internal class SoldierCommendations
{
    string _type, _noun;
    int _decorationLevel;
    bool _isNew;

    /**
     * Initializes a new commendation entry from YAML.
     * @param node YAML node.
     */
    internal SoldierCommendations(YamlNode node) =>
	    load(node);

    /**
     * Initializes a soldier commendation.
     */
    internal SoldierCommendations(string commendationName, string noun)
    {
        _type = commendationName;
        _noun = noun;
        _decorationLevel = 0;
        _isNew = true;
    }

    /**
     *
     */
    ~SoldierCommendations() { }

    /**
     * Saves the commendation to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "commendationName", _type }
        };
        if (_noun != "noNoun") node.Add("noun", _noun);
	    node.Add("decorationLevel", _decorationLevel.ToString());
	    return node;
    }

    /**
     * Get the soldier's commendation's name.
     * @return string Commendation name.
     */
    internal string getType() =>
	    _type;

    /**
     * Loads the commendation from a YAML file.
     * @param node YAML node.
     */
    void load(YamlNode node)
    {
	    _type = node["commendationName"].ToString();
	    _noun = node["noun"].ToString() ?? "noNoun";
	    _decorationLevel = int.Parse(node["decorationLevel"].ToString());
	    _isNew = bool.TryParse(node["isNew"].ToString(), out bool isNew) ? isNew : false;
    }

    /**
     * Get the soldier's commendation's noun.
     * @return string Commendation noun
     */
    internal string getNoun() =>
	    _noun;

    /**
     * Get the soldier commendation level's description.
     * @return string Commendation level description.
     */
    internal string getDecorationDescription() =>
	    $"STR_AWARD_DECOR_{_decorationLevel}";

    /**
     * Get the soldier commendation level's int.
     * @return int Commendation level.
     */
    internal int getDecorationLevelInt() =>
	    _decorationLevel;

    /**
     * Get newness of commendation.
     * @return bool Is the commendation new?
     */
    internal bool isNew() =>
	    _isNew;

    /**
     * Set the newness of the commendation to old.
     */
    internal void makeOld() =>
        _isNew = false;

    /**
     * Get the soldier commendation level's name.
     * @return string Commendation level.
     */
    internal string getDecorationLevelName(int skipCounter) =>
        $"STR_AWARD_{_decorationLevel - skipCounter}";
}

internal class SoldierDiary
{
    List<SoldierCommendations> _commendations;
    List<BattleUnitKills> _killList;
    int _daysWoundedTotal, _totalShotByFriendlyCounter, _totalShotFriendlyCounter, _loneSurvivorTotal, _monthsService, _unconciousTotal, _shotAtCounterTotal,
        _hitCounterTotal, _ironManTotal, _longDistanceHitCounterTotal, _lowAccuracyHitCounterTotal, _shotsFiredCounterTotal, _shotsLandedCounterTotal,
        _shotAtCounter10in1Mission, _hitCounter5in1Mission, _timesWoundedTotal, _KIA, _allAliensKilledTotal, _allAliensStunnedTotal,
        _woundsHealedTotal, _allUFOs, _allMissionTypes, _statGainTotal, _revivedUnitTotal, _wholeMedikitTotal, _braveryGainTotal, _bestOfRank, _MIA,
        _martyrKillsTotal, _postMortemKills, _slaveKillsTotal, _bestSoldier, _revivedSoldierTotal, _revivedHostileTotal, _revivedNeutralTotal;
    bool _globeTrotter;
    List<int> _missionIdList;

    /**
     * Initializes a new blank diary.
     */
    internal SoldierDiary()
    {
        _daysWoundedTotal = 0;
        _totalShotByFriendlyCounter = 0;
        _totalShotFriendlyCounter = 0;
        _loneSurvivorTotal = 0;
        _monthsService = 0;
        _unconciousTotal = 0;
        _shotAtCounterTotal = 0;
        _hitCounterTotal = 0;
        _ironManTotal = 0;
        _longDistanceHitCounterTotal = 0;
        _lowAccuracyHitCounterTotal = 0;
        _shotsFiredCounterTotal = 0;
        _shotsLandedCounterTotal = 0;
        _shotAtCounter10in1Mission = 0;
        _hitCounter5in1Mission = 0;
        _timesWoundedTotal = 0;
        _KIA = 0;
        _allAliensKilledTotal = 0;
        _allAliensStunnedTotal = 0;
        _woundsHealedTotal = 0;
        _allUFOs = 0;
        _allMissionTypes = 0;
        _statGainTotal = 0;
        _revivedUnitTotal = 0;
        _wholeMedikitTotal = 0;
        _braveryGainTotal = 0;
        _bestOfRank = 0;
        _MIA = 0;
        _martyrKillsTotal = 0;
        _postMortemKills = 0;
        _slaveKillsTotal = 0;
        _bestSoldier = 0;
        _revivedSoldierTotal = 0;
        _revivedHostileTotal = 0;
        _revivedNeutralTotal = 0;
        _globeTrotter = false;
    }

    /**
     *
     */
    ~SoldierDiary()
    {
        _commendations.Clear();
        _killList.Clear();
    }

    /**
     * Get vector of mission ids.
     * @return Vector of mission ids.
     */
    internal List<int> getMissionIdList() =>
	    _missionIdList;

    /**
     * Get soldier commendations.
     * @return SoldierCommendations list of soldier's commendations.
     */
    internal List<SoldierCommendations> getSoldierCommendations() =>
        _commendations;

    /**
     * Returns the total months this soldier has been in service.
     */
    internal int getMonthsService() =>
	    _monthsService;

    /**
     * Saves the diary to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "commendations", new YamlSequenceNode(_commendations.Select(x => x.save())) },
            { "killList", new YamlSequenceNode(_killList.Select(x => x.save())) }
        };
        if (_missionIdList.Any()) node.Add("missionIdList", new YamlSequenceNode(_missionIdList.Select(x => new YamlScalarNode(x.ToString()))));
	    if (_daysWoundedTotal != 0) node.Add("daysWoundedTotal", _daysWoundedTotal.ToString());
	    if (_totalShotByFriendlyCounter != 0) node.Add("totalShotByFriendlyCounter", _totalShotByFriendlyCounter.ToString());
	    if (_totalShotFriendlyCounter != 0) node.Add("totalShotFriendlyCounter", _totalShotFriendlyCounter.ToString());
	    if (_loneSurvivorTotal != 0) node.Add("loneSurvivorTotal", _loneSurvivorTotal.ToString());
	    if (_monthsService != 0) node.Add("monthsService", _monthsService.ToString());
	    if (_unconciousTotal != 0) node.Add("unconciousTotal", _unconciousTotal.ToString());
	    if (_shotAtCounterTotal != 0) node.Add("shotAtCounterTotal", _shotAtCounterTotal.ToString());
	    if (_hitCounterTotal != 0) node.Add("hitCounterTotal", _hitCounterTotal.ToString());
	    if (_ironManTotal != 0) node.Add("ironManTotal", _ironManTotal.ToString());
	    if (_longDistanceHitCounterTotal != 0) node.Add("longDistanceHitCounterTotal", _longDistanceHitCounterTotal.ToString());
	    if (_lowAccuracyHitCounterTotal != 0) node.Add("lowAccuracyHitCounterTotal", _lowAccuracyHitCounterTotal.ToString());
	    if (_shotsFiredCounterTotal != 0) node.Add("shotsFiredCounterTotal", _shotsFiredCounterTotal.ToString());
	    if (_shotsLandedCounterTotal != 0) node.Add("shotsLandedCounterTotal", _shotsLandedCounterTotal.ToString());
	    if (_shotAtCounter10in1Mission != 0) node.Add("shotAtCounter10in1Mission", _shotAtCounter10in1Mission.ToString());
	    if (_hitCounter5in1Mission != 0) node.Add("hitCounter5in1Mission", _hitCounter5in1Mission.ToString());
	    if (_timesWoundedTotal != 0) node.Add("timesWoundedTotal", _timesWoundedTotal.ToString());
	    if (_allAliensKilledTotal != 0) node.Add("allAliensKilledTotal", _allAliensKilledTotal.ToString());
	    if (_allAliensStunnedTotal != 0) node.Add("allAliensStunnedTotal", _allAliensStunnedTotal.ToString());
	    if (_woundsHealedTotal != 0) node.Add("woundsHealedTotal", _woundsHealedTotal.ToString());
	    if (_allUFOs != 0) node.Add("allUFOs", _allUFOs.ToString());
	    if (_allMissionTypes != 0) node.Add("allMissionTypes", _allMissionTypes.ToString());
	    if (_statGainTotal != 0) node.Add("statGainTotal",_statGainTotal.ToString());
	    if (_revivedUnitTotal != 0) node.Add("revivedUnitTotal", _revivedUnitTotal.ToString());
	    if (_revivedSoldierTotal != 0) node.Add("revivedSoldierTotal", _revivedSoldierTotal.ToString());
	    if (_revivedHostileTotal != 0) node.Add("revivedHostileTotal", _revivedHostileTotal.ToString());
	    if (_revivedNeutralTotal != 0) node.Add("revivedNeutralTotal", _revivedNeutralTotal.ToString());
	    if (_wholeMedikitTotal != 0) node.Add("wholeMedikitTotal", _wholeMedikitTotal.ToString());
	    if (_braveryGainTotal != 0) node.Add("braveryGainTotal", _braveryGainTotal.ToString());
	    if (_bestOfRank != 0) node.Add("bestOfRank", _bestOfRank.ToString());
	    if (_bestSoldier != 0) node.Add("bestSoldier", _bestSoldier.ToString());
	    if (_martyrKillsTotal != 0) node.Add("martyrKillsTotal", _martyrKillsTotal.ToString());
	    if (_postMortemKills != 0) node.Add("postMortemKills", _postMortemKills.ToString());
	    if (_globeTrotter) node.Add("globeTrotter", _globeTrotter.ToString());
	    if (_slaveKillsTotal != 0) node.Add("slaveKillsTotal", _slaveKillsTotal.ToString());
        return node;
    }

    /**
     * Loads the diary from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node, Mod.Mod mod)
    {
        var commendations = node["commendations"] as YamlSequenceNode;
        if (commendations != null)
	    {
		    foreach (var i in commendations)
		    {
			    SoldierCommendations sc = new SoldierCommendations(i);
			    RuleCommendations commendation = mod.getCommendation(sc.getType());
			    if (commendation != null)
			    {
				    _commendations.Add(sc);
			    }
			    else
			    {
				    // obsolete commendation, ignore it... otherwise it would cause a crash later
				    sc = null;
			    }
		    }
	    }
        var killList = node["killList"] as YamlSequenceNode;
        if (killList != null)
	    {
		    foreach (var i in killList)
			    _killList.Add(new BattleUnitKills(i));
	    }
        _missionIdList = ((YamlSequenceNode)node["missionIdList"]).Children.Select(x => int.Parse(x.ToString())).ToList();
	    _daysWoundedTotal = int.Parse(node["daysWoundedTotal"].ToString());
	    _totalShotByFriendlyCounter = int.Parse(node["totalShotByFriendlyCounter"].ToString());
	    _totalShotFriendlyCounter = int.Parse(node["totalShotFriendlyCounter"].ToString());
	    _loneSurvivorTotal = int.Parse(node["loneSurvivorTotal"].ToString());
	    _monthsService = int.Parse(node["monthsService"].ToString());
	    _unconciousTotal = int.Parse(node["unconciousTotal"].ToString());
	    _shotAtCounterTotal = int.Parse(node["shotAtCounterTotal"].ToString());
	    _hitCounterTotal = int.Parse(node["hitCounterTotal"].ToString());
	    _ironManTotal = int.Parse(node["ironManTotal"].ToString());
	    _longDistanceHitCounterTotal = int.Parse(node["longDistanceHitCounterTotal"].ToString());
	    _lowAccuracyHitCounterTotal = int.Parse(node["lowAccuracyHitCounterTotal"].ToString());
	    _shotsFiredCounterTotal = int.Parse(node["shotsFiredCounterTotal"].ToString());
	    _shotsLandedCounterTotal = int.Parse(node["shotsLandedCounterTotal"].ToString());
	    _shotAtCounter10in1Mission = int.Parse(node["shotAtCounter10in1Mission"].ToString());
	    _hitCounter5in1Mission = int.Parse(node["hitCounter5in1Mission"].ToString());
	    _timesWoundedTotal = int.Parse(node["timesWoundedTotal"].ToString());
	    _allAliensKilledTotal = int.Parse(node["allAliensKilledTotal"].ToString());
	    _allAliensStunnedTotal = int.Parse(node["allAliensStunnedTotal"].ToString());
	    _woundsHealedTotal = int.Parse(node["woundsHealedTotal"].ToString());
	    _allUFOs = int.Parse(node["allUFOs"].ToString());
	    _allMissionTypes = int.Parse(node["allMissionTypes"].ToString());
	    _statGainTotal = int.Parse(node["statGainTotal"].ToString());
	    _revivedUnitTotal = int.Parse(node["revivedUnitTotal"].ToString());
	    _revivedSoldierTotal = int.Parse(node["revivedSoldierTotal"].ToString());
	    _revivedHostileTotal = int.Parse(node["revivedHostileTotal"].ToString());
	    _revivedNeutralTotal = int.Parse(node["revivedNeutralTotal"].ToString());
	    _wholeMedikitTotal = int.Parse(node["wholeMedikitTotal"].ToString());
	    _braveryGainTotal = int.Parse(node["braveryGainTotal"].ToString());
	    _bestOfRank = int.Parse(node["bestOfRank"].ToString());
        _bestSoldier = Convert.ToInt32(bool.Parse(node["bestSoldier"].ToString()));
        _martyrKillsTotal = int.Parse(node["martyrKillsTotal"].ToString());
	    _postMortemKills = int.Parse(node["postMortemKills"].ToString());
	    _globeTrotter = bool.Parse(node["globeTrotter"].ToString());
	    _slaveKillsTotal = int.Parse(node["slaveKillsTotal"].ToString());
    }

    /**
     *
     */
    internal Dictionary<string, int> getAlienRaceTotal()
    {
        var list = new Dictionary<string, int>();
        foreach (var kill in _killList)
        {
            list[kill.race]++;
        }
        return list;
    }

    /**
     * Get list of kills sorted by rank
     * @return
     */
    internal Dictionary<string, int> getAlienRankTotal()
    {
        var list = new Dictionary<string, int>();
        foreach (var kill in _killList)
        {
            list[kill.rank]++;
        }
        return list;
    }

    /**
     *
     */
    internal Dictionary<string, int> getWeaponTotal()
    {
        var list = new Dictionary<string, int>();
        foreach (var kill in _killList)
        {
            if (kill.faction == UnitFaction.FACTION_HOSTILE)
                list[kill.weapon]++;
        }
        return list;
    }

    /**
     *
     */
    internal int getKillTotal()
    {
	    int killTotal = 0;

	    foreach (var i in _killList)
	    {
		    if (i.status == UnitStatus.STATUS_DEAD && i.faction == UnitFaction.FACTION_HOSTILE)
		    {
			    killTotal++;
		    }
	    }

	    return killTotal;
    }

    /**
     *
     */
    internal int getStunTotal()
    {
	    int stunTotal = 0;

	    foreach (var i in _killList)
	    {
		    if (i.status == UnitStatus.STATUS_UNCONSCIOUS && i.faction == UnitFaction.FACTION_HOSTILE)
		    {
			    stunTotal++;
		    }
	    }

	    return stunTotal;
    }

    /**
     *  Get the soldier's accuracy.
     */
    internal int getAccuracy()
    {
	    if (_shotsFiredCounterTotal != 0)
		    return 100 * _shotsLandedCounterTotal / _shotsFiredCounterTotal;
	    return 0;
    }

    /**
     *
     */
    internal int getControlTotal()
    {
	    int controlTotal = 0;

	    foreach (var i in _killList)
	    {
		    if (i.status == UnitStatus.STATUS_TURNING && i.faction == UnitFaction.FACTION_HOSTILE)
		    {
			    controlTotal++;
		    }
	    }

	    return controlTotal;
    }

    /**
     *  Get a map of the amount of missions done in each region.
     *  @param MissionStatistics
     */
    internal Dictionary<string, int> getRegionTotal(List<MissionStatistics> missionStatistics)
    {
	    var regionTotal = new Dictionary<string, int>();

	    foreach (var i in missionStatistics)
	    {
		    foreach (var j in _missionIdList)
		    {
			    if (j == i.id)
			    {
				    regionTotal[i.region]++;
			    }
		    }
	    }

	    return regionTotal;
    }

    /**
     *  Get a map of the amount of missions done in each type.
     *  @param MissionStatistics
     */
    internal Dictionary<string, int> getTypeTotal(List<MissionStatistics> missionStatistics)
    {
	    var typeTotal = new Dictionary<string, int>();

	    foreach (var i in missionStatistics)
	    {
		    foreach (var j in _missionIdList)
		    {
			    if (j == i.id)
			    {
				    typeTotal[i.type]++;
			    }
		    }
	    }

	    return typeTotal;
    }

    /**
     *  Get a map of the amount of missions done in each UFO.
     *  @param MissionStatistics
     */
    internal Dictionary<string, int> getUFOTotal(List<MissionStatistics> missionStatistics)
    {
	    var ufoTotal = new Dictionary<string, int>();

	    foreach (var i in missionStatistics)
	    {
		    foreach (var j in _missionIdList)
		    {
			    if (j == i.id)
			    {
				    ufoTotal[i.ufo]++;
			    }
		    }
	    }

	    return ufoTotal;
    }

    /**
     *
     */
    internal int getMissionTotal() =>
	    _missionIdList.Count;

    /**
     *  Get the total if wins.
     *  @param Mission Statistics
     */
    internal int getWinTotal(List<MissionStatistics> missionStatistics)
    {
	    int winTotal = 0;

	    foreach (var i in missionStatistics)
	    {
		    foreach (var j in _missionIdList)
		    {
			    if (j == i.id)
			    {
				    if (i.success)
				    {
					    winTotal++;
				    }
			    }
		    }
	    }

	    return winTotal;
    }

    /**
     *  Get the total score.
     *  @param Mission Statistics
     */
    internal int getScoreTotal(List<MissionStatistics> missionStatistics)
    {
	    int scoreTotal = 0;

	    foreach (var i in missionStatistics)
	    {
		    foreach (var j in _missionIdList)
		    {
			    if (j == i.id)
			    {
				    scoreTotal += i.score;
			    }
		    }
	    }

	    return scoreTotal;
    }

    /**
     *
     */
    internal int getDaysWoundedTotal() =>
	    _daysWoundedTotal;

    /**
     * Get vector of kills.
     * @return vector of BattleUnitKills
     */
    internal List<BattleUnitKills> getKills() =>
	    _killList;

    /**
     * Increment soldier's service time one month
     */
    internal void addMonthlyService() =>
        _monthsService++;

    /**
     *
     */
    internal int getShotsFiredTotal() =>
	    _shotsFiredCounterTotal;

    /**
     *
     */
    internal int getShotsLandedTotal() =>
	    _shotsLandedCounterTotal;

    /**
     * Award special commendation to the original 8 soldiers.
     */
    internal void awardOriginalEightCommendation()
    {
	    // TODO: Unhardcode this
	    _commendations.Add(new SoldierCommendations("STR_MEDAL_ORIGINAL8_NAME", "NoNoun"));
    }
}
