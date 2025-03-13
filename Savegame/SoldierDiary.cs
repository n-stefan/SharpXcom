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
    internal SoldierCommendations(string commendationName, string noun = "noNoun")
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
    internal void load(YamlNode node)
    {
	    _type = node["commendationName"].ToString();
	    _noun = node["noun"] != null ? node["noun"].ToString() : "noNoun";
	    _decorationLevel = int.Parse(node["decorationLevel"].ToString());
	    _isNew = bool.Parse(node["isNew"].ToString());
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

	/**
	 * Add a level of decoration to the commendation.
	 * Sets isNew to true.
	 */
	internal void addDecoration()
	{
		_decorationLevel++;
		_isNew = true;
	}
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
        if (node["commendations"] is YamlSequenceNode commendations)
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
        if (node["killList"] is YamlSequenceNode killList)
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

	/**
	 * Manage the soldier's commendations.
	 * Award new ones, if deserved.
	 * @return bool Has a commendation been awarded?
	 */
	internal bool manageCommendations(Mod.Mod mod, List<MissionStatistics> missionStatistics)
	{
		const int BATTLE_TYPES = 13;
		const int DAMAGE_TYPES = 11;
		string[] battleTypeArray = { "BT_NONE", "BT_FIREARM", "BT_AMMO", "BT_MELEE", "BT_GRENADE",	"BT_PROXIMITYGRENADE", "BT_MEDIKIT", "BT_SCANNER", "BT_MINDPROBE", "BT_PSIAMP", "BT_FLARE", "BT_CORPSE", "BT_END" };
		string[] damageTypeArray = { "DT_NONE", "DT_AP", "DT_IN", "DT_HE", "DT_LASER", "DT_PLASMA", "DT_STUN", "DT_MELEE", "DT_ACID", "DT_SMOKE", "DT_END"};

		Dictionary<string, RuleCommendations> commendationsList = mod.getCommendationsList();
		bool awardedCommendation = false;                   // This value is returned if at least one commendation was given.
		var nextCommendationLevel = new Dictionary<string, int>();   // Noun, threshold.
		var modularCommendations = new List<string>();      // Commendation name.
		bool awardCommendationBool = false;                 // This value determines if a commendation will be given.
		// Loop over all possible commendations

		var i = commendationsList.GetEnumerator();
		i.MoveNext();
		while (i.Current.Key != null)
		{
			awardCommendationBool = true;
			nextCommendationLevel.Clear();
			nextCommendationLevel["noNoun"] = 0;
			modularCommendations.Clear();
			// Loop over all the soldier's commendations, see if we already have the commendation.
			// If so, get the level and noun.
			foreach (var j in _commendations)
			{
				if ( i.Current.Key == j.getType() )
				{
					nextCommendationLevel[j.getNoun()] = j.getDecorationLevelInt() + 1;
				}
			}
			// Go through each possible criteria. Assume the medal is awarded, set to false if not.
			// As soon as we find a medal criteria that we FAIL TO achieve, then we are not awarded a medal.
			foreach (var j in i.Current.Value.getCriteria())
			{
				// Skip this medal if we have reached its max award level.
				if ((uint)nextCommendationLevel["noNoun"] >= j.Value.Count)
				{
					awardCommendationBool = false;
					break;
				}
				// These criteria have no nouns, so only the nextCommendationLevel["noNoun"] will ever be used.
				else if( nextCommendationLevel.Count(x => x.Key == "noNoun") == 1 &&
					  ( (j.Key == "totalKills" && (uint)getKillTotal() < (uint)j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalMissions" && _missionIdList.Count < (uint)j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalWins" && getWinTotal(missionStatistics) < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalScore" && getScoreTotal(missionStatistics) < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalStuns" && getStunTotal() < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalDaysWounded" && _daysWoundedTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalBaseDefenseMissions" && getBaseDefenseMissionTotal(missionStatistics) < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalTerrorMissions" && getTerrorMissionTotal(missionStatistics) < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalNightMissions" && getNightMissionTotal(missionStatistics) < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalNightTerrorMissions" && getNightTerrorMissionTotal(missionStatistics) < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalMonthlyService" && _monthsService < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalFellUnconcious" && _unconciousTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalShotAt10Times" && _shotAtCounter10in1Mission < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalHit5Times" && _hitCounter5in1Mission < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalFriendlyFired" && (_totalShotByFriendlyCounter < j.Value[nextCommendationLevel["noNoun"]] || _KIA != 0 || _MIA != 0)) ||
						(j.Key == "total_lone_survivor" && _loneSurvivorTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalIronMan" && _ironManTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalImportantMissions" && getImportantMissionTotal(missionStatistics) < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalLongDistanceHits" && _longDistanceHitCounterTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalLowAccuracyHits" && _lowAccuracyHitCounterTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalReactionFire" && getReactionFireKillTotal(mod) < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalTimesWounded" && _timesWoundedTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalDaysWounded" && _daysWoundedTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalValientCrux" && getValiantCruxTotal(missionStatistics) < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "isDead" && _KIA < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalTrapKills" && getTrapKillTotal(mod) < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalAlienBaseAssaults" && getAlienBaseAssaultTotal(missionStatistics) < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalAllAliensKilled" && _allAliensKilledTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalAllAliensStunned" && _allAliensStunnedTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalWoundsHealed" && _woundsHealedTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalAllUFOs" && _allUFOs < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalAllMissionTypes" && _allMissionTypes < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalStatGain" && _statGainTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalRevives" && _revivedUnitTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalSoldierRevives" && _revivedSoldierTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalHostileRevives" && _revivedHostileTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalNeutralRevives" && _revivedNeutralTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalWholeMedikit" && _wholeMedikitTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalBraveryGain" && _braveryGainTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "bestOfRank" && _bestOfRank < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "bestSoldier" && (int)_bestSoldier < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "isMIA" && _MIA < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalMartyrKills" && _martyrKillsTotal < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalPostMortemKills" && _postMortemKills < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "globeTrotter" && Convert.ToInt32(_globeTrotter) < j.Value[nextCommendationLevel["noNoun"]]) ||
						(j.Key == "totalSlaveKills" && _slaveKillsTotal < j.Value[nextCommendationLevel["noNoun"]]) ) )
				{
					awardCommendationBool = false;
					break;
				}
				// Medals with the following criteria are unique because they need a noun.
				// And because they loop over a map<> (this allows for maximum moddability).
				else if (j.Key == "totalKillsWithAWeapon" || j.Key == "totalMissionsInARegion" || j.Key == "totalKillsByRace" || j.Key == "totalKillsByRank")
				{
					var tempTotal = new Dictionary<string, int>();
					if (j.Key == "totalKillsWithAWeapon")
						tempTotal = getWeaponTotal();
					else if (j.Key == "totalMissionsInARegion")
						tempTotal = getRegionTotal(missionStatistics);
					else if (j.Key == "totalKillsByRace")
						tempTotal = getAlienRaceTotal();
					else if (j.Key == "totalKillsByRank")
						tempTotal = getAlienRankTotal();
					// Loop over the temporary map.
					// Match nouns and decoration levels.
					foreach (var k in tempTotal)
					{
						int criteria = -1;
						string noun = k.Key;
						// If there is no matching noun, get the first award criteria.
						if (nextCommendationLevel.Count(x => x.Key == noun) == 0)
							criteria = j.Value.First();
						// Otherwise, get the criteria that reflects the soldier's commendation level.
						else if ((uint)nextCommendationLevel[noun] != j.Value.Count)
							criteria = j.Value[nextCommendationLevel[noun]];

						// If a criteria was set AND the stat's count exceeds the criteria.
						if (criteria != -1 && k.Value >= criteria)
						{
							modularCommendations.Add(noun);
						}
					}
					// If it is still empty, we did not get a commendation.
					if (!modularCommendations.Any())
					{
						awardCommendationBool = false;
						break;
					}
				}
				// Medals that are based on _how_ a kill was achieved are found here.
				else if (j.Key == "killsWithCriteriaCareer" || j.Key == "killsWithCriteriaMission" || j.Key == "killsWithCriteriaTurn")
				{
					// Fetch the kill criteria list.
					if (i.Current.Value.getKillCriteria() == null)
						break;
					List<List<KeyValuePair<int, List<string>>>> _killCriteriaList = i.Current.Value.getKillCriteria();

					int totalKillGroups = 0; // holds the total number of kill groups which satisfy one of the OR criteria blocks
					bool enoughForNextCommendation = false;

					// Loop over the OR vectors.
					// if OR criteria are not disjunctive (e.g. "kill 1 enemy" or "kill 1 enemy"), each one will be counted and added to totals - avoid that if you want good statistics
					for (var orCriteria = 0; orCriteria < _killCriteriaList.Count; ++orCriteria)
					{
						// prepare counters
						var referenceBlockCounters = new List<int>(orCriteria); //referenceBlockCounters.assign(orCriteria.Count, 0);
						int referenceTotalCounters = 0;
						for (var andCriteria = 0; andCriteria < _killCriteriaList[orCriteria].Count; ++andCriteria)
						{
							int index = andCriteria - orCriteria;
							referenceBlockCounters[index] = _killCriteriaList[orCriteria][andCriteria].Key;
							referenceTotalCounters += _killCriteriaList[orCriteria][andCriteria].Key;
						}
						var currentBlockCounters = new List<int>();
						if (j.Key == "killsWithCriteriaCareer") {
							currentBlockCounters = referenceBlockCounters;
						}
						int currentTotalCounters = referenceTotalCounters;
						int lastTimeSpan = -1;
						bool skipThisTimeSpan = false;
						// Loop over the KILLS, seeking to fulfill all criteria from entire AND block within the specified time span (career/mission/turn)
						foreach (var singleKill in _killList)
						{
							int thisTimeSpan = -1;
							if (j.Key == "killsWithCriteriaMission")
							{
								thisTimeSpan = singleKill.mission;
							}
							else if (j.Key == "killsWithCriteriaTurn")
							{
								thisTimeSpan = singleKill.turn;
							}
							if (thisTimeSpan != lastTimeSpan)
							{
								// next time span, reset counters
								lastTimeSpan = thisTimeSpan;
								skipThisTimeSpan = false;
								currentBlockCounters = referenceBlockCounters;
								currentTotalCounters = referenceTotalCounters;
							}
							// same time span, we're skipping the rest of it if we already fulfilled criteria
							else if (skipThisTimeSpan)
							{
								continue;
							}

							bool andCriteriaMet = false;

							// Loop over the AND vectors.
							for (var andCriteria = 0; andCriteria < _killCriteriaList[orCriteria].Count; ++andCriteria)
							{
								bool foundMatch = true;

								// Loop over the DETAILs of one AND vector.
								foreach (var detail in _killCriteriaList[orCriteria][andCriteria].Value)
								{
									int battleType = 0;
									for (; battleType != BATTLE_TYPES; ++battleType)
									{
										if (detail == battleTypeArray[battleType])
										{
											break;
										}
									}

									int damageType = 0;
									for (; damageType != DAMAGE_TYPES; ++damageType)
									{
										if (detail == damageTypeArray[damageType])
										{
											break;
										}
									}

									// See if we find _no_ matches with any criteria. If so, break and try the next kill.
									RuleItem weapon = mod.getItem(singleKill.weapon);
									RuleItem weaponAmmo = mod.getItem(singleKill.weaponAmmo);
									if (weapon == null || weaponAmmo == null ||
										(singleKill.rank != detail && singleKill.race != detail &&
										 singleKill.weapon != detail && singleKill.weaponAmmo != detail &&
										 singleKill.getUnitStatusString() != detail && singleKill.getUnitFactionString() != detail &&
										 singleKill.getUnitSideString() != detail && singleKill.getUnitBodyPartString() != detail &&
										 (int)weaponAmmo.getDamageType() != damageType && (int)weapon.getBattleType() != battleType))
									{
										foundMatch = false;
										break;
									}
								} /// End of DETAIL loop.

								if (foundMatch)
								{
									int index = andCriteria - orCriteria;
									// some current block counters might go into negatives, this is used to tally career kills correctly
									// currentTotalCounters will always ensure we're counting in proper batches
									if (currentBlockCounters[index]-- > 0 && --currentTotalCounters <= 0)
									{
										// we just counted all counters in a block to zero, this certainly means that the entire block criteria is fulfilled
										andCriteriaMet = true;
										break;
									}
								}
							} /// End of AND loop.

							if (andCriteriaMet)
							{
								// early exit if we got enough, no reason to continue iterations
								if (++totalKillGroups >= j.Value[nextCommendationLevel["noNoun"]])
								{
									enoughForNextCommendation = true;
									break;
								}
							
								// "killsWithCriteriaTurn" and "killsWithCriteriaMission" are "peak achivements", they are counted once per their respective time span if criteria are fulfilled
								// so if we got them, we're skipping the rest of this time span to avoid counting more than once
								// e.g. 20 kills in a mission will not be counted as "10 kills in a mission" criteria twice
								// "killsWithCriteriaCareer" are totals, so they are never skipped this way
								if (j.Key == "killsWithCriteriaTurn" || j.Key == "killsWithCriteriaMission")
								{
									skipThisTimeSpan = true;
								}
								// for career kills we'll ADD reference counters to the current values and recalculate current total
								// this is used to count instances of full criteria blocks, e.g. if rules state that a career commendation must be awarded for 2 kills of alien leaders
								// and 1 kill of  alien commander, then we must ensure there's 2 leader kills + 1 commander kill for each instance of criteria fulfilled
								else if (j.Key == "killsWithCriteriaCareer")
								{
									currentTotalCounters = 0;
									for (int k = 0; k < currentBlockCounters.Count; k++)
									{
										currentBlockCounters[k] += referenceBlockCounters[k];
										currentTotalCounters += Math.Max(currentBlockCounters[k], 0);
									}
								}
							}
						} /// End of KILLs loop.

						if (enoughForNextCommendation)
							break; // stop iterating here too, we've got enough

					} /// End of OR loop.

					if (!enoughForNextCommendation)
						awardCommendationBool = false;

				}
			}
			if (awardCommendationBool)
			{
				// If we do not have modular medals, but are awarded a different medal,
				// its noun will be "noNoun".
				if (!modularCommendations.Any())
				{
					modularCommendations.Add("noNoun");
				}
				foreach (var j in modularCommendations)
				{
					bool newCommendation = true;
					foreach (var k in _commendations)
					{
						if ( k.getType() == i.Current.Key && k.getNoun() == j)
						{
							k.addDecoration();
							newCommendation = false;
							break;
						}
					}
					if (newCommendation)
					{
						_commendations.Add(new SoldierCommendations(i.Current.Key, j));
					}
				}
				awardedCommendation = true;
			}
			else
			{
				i.MoveNext();
			}
		}
		return awardedCommendation;
	}

	/**
	 *  Get the total of base defense missions.
	 *  @param Mission Statistics
	 */
	int getBaseDefenseMissionTotal(List<MissionStatistics> missionStatistics)
	{
		int baseDefenseMissionTotal = 0;

		foreach (var i in missionStatistics)
		{
			foreach (var j in _missionIdList)
			{
				if (j == i.id)
				{
					if (i.success && i.isBaseDefense())
					{
						baseDefenseMissionTotal++;
					}
				}
			}
		}

		return baseDefenseMissionTotal;
	}

	/**
	 *  Get the total of terror missions.
	 *  @param Mission Statistics
	 */
	int getTerrorMissionTotal(List<MissionStatistics> missionStatistics)
	{
		/// Not a UFO, not the base, not the alien base or colony
		int terrorMissionTotal = 0;

		foreach (var i in missionStatistics)
		{
			foreach (var j in _missionIdList)
			{
				if (j == i.id)
				{
					if (i.success && !i.isBaseDefense() && !i.isUfoMission() && !i.isAlienBase())
					{
						terrorMissionTotal++;
					}
				}
			}
		}

		return terrorMissionTotal;
	}

	/**
	 *  Get the total of night missions.
	 *  @param Mission Statistics
	 */
	int getNightMissionTotal(List<MissionStatistics> missionStatistics)
	{
		int nightMissionTotal = 0;

		foreach (var i in missionStatistics)
		{
			foreach (var j in _missionIdList)
			{
				if (j == i.id)
				{
					if (i.success && i.isDarkness() && !i.isBaseDefense() && !i.isAlienBase())
					{
						nightMissionTotal++;
					}
				}
			}
		}

		return nightMissionTotal;
	}

	/**
	 *  Get the total of night terror missions.
	 *  @param Mission Statistics
	 */
	int getNightTerrorMissionTotal(List<MissionStatistics> missionStatistics)
	{
		int nightTerrorMissionTotal = 0;

		foreach (var i in missionStatistics)
		{
			foreach (var j in _missionIdList)
			{
				if (j == i.id)
				{
					if (i.success && i.isDarkness() && !i.isBaseDefense() && !i.isUfoMission() && !i.isAlienBase())
					{
						nightTerrorMissionTotal++;
					}
				}
			}
		}

		return nightTerrorMissionTotal;
	}

	/**
	 *  Get the total of important missions.
	 *  @param Mission Statistics
	 */
	int getImportantMissionTotal(List<MissionStatistics> missionStatistics)
	{
		int importantMissionTotal = 0;

		foreach (var i in missionStatistics)
		{
			foreach (var j in _missionIdList)
			{
				if (j == i.id)
				{
					if (i.success && i.type != "STR_UFO_CRASH_RECOVERY")
					{
						importantMissionTotal++;
					}
				}
			}
		}

		return importantMissionTotal;
	}

    /**
	 *  Get reaction kill total.
	 */
    int getReactionFireKillTotal(Mod.Mod mod)
    {
        int reactionFireKillTotal = 0;

        foreach (var i in _killList)
        {
            RuleItem item = mod.getItem(i.weapon);
            if (i.hostileTurn() && item != null && item.getBattleType() != BattleType.BT_GRENADE && item.getBattleType() != BattleType.BT_PROXIMITYGRENADE)
            {
                reactionFireKillTotal++;
            }
        }

        return reactionFireKillTotal;
    }

	/**
	 *  Get the Valient Crux total.
	 *  @param Mission Statistics
	 */
	int getValiantCruxTotal(List<MissionStatistics> missionStatistics)
	{
		int valiantCruxTotal = 0;

		foreach (var i in missionStatistics)
		{
			foreach (var j in _missionIdList)
			{
				if (j == i.id && i.valiantCrux)
				{
					valiantCruxTotal++;
				}
			}
		}

		return valiantCruxTotal;
	}

	/**
	 *  Get trap kills total.
	 */
	int getTrapKillTotal(Mod.Mod mod)
	{
		int trapKillTotal = 0;

		foreach (var i in _killList)
		{
			RuleItem item = mod.getItem(i.weapon);
			if (i.hostileTurn() && (item == null || item.getBattleType() == BattleType.BT_GRENADE || item.getBattleType() == BattleType.BT_PROXIMITYGRENADE))
			{
				trapKillTotal++;
			}
		}

		return trapKillTotal;
	}

	/**
	 *  Get the total of alien base assaults.
	 *  @param Mission Statistics
	 */
	int getAlienBaseAssaultTotal(List<MissionStatistics> missionStatistics)
	{
		int alienBaseAssaultTotal = 0;

		foreach (var i in missionStatistics)
		{
			foreach (var j in _missionIdList)
			{
				if (j == i.id)
				{
					if (i.success && i.isAlienBase())
					{
						alienBaseAssaultTotal++;
					}
				}
			}
		}

		return alienBaseAssaultTotal;
	}

	/**
	 * Award post-humous kills commendation.
	 */
	internal void awardPostMortemKill(int kills) =>
		_postMortemKills = kills;

	/**
	 * Award post-humous best-of commendation.
	 */
	internal void awardBestOfRank(int score) =>
		_bestOfRank = score;

	/**
	 * Award post-humous best-of commendation.
	 */
	internal void awardBestOverall(int score) =>
		_bestSoldier = score;

	/**
	 * Update soldier diary statistics.
	 * @param unitStatistics BattleUnitStatistics to get stats from.
	 * @param missionStatistics MissionStatistics to get stats from.
	 */
	internal void updateDiary(BattleUnitStatistics unitStatistics, List<MissionStatistics> allMissionStatistics, Mod.Mod rules)
	{
		if (!allMissionStatistics.Any()) return;
		MissionStatistics missionStatistics = allMissionStatistics.Last();
		List<BattleUnitKills> unitKills = unitStatistics.kills;
		foreach (var kill in unitKills)
		{
			kill.makeTurnUnique();
			_killList.Add(kill);
		}
		unitKills.Clear();
		if (missionStatistics.success)
		{
			if (unitStatistics.loneSurvivor)
				_loneSurvivorTotal++;
			if (unitStatistics.ironMan)
				_ironManTotal++;
			if (unitStatistics.nikeCross)
				_allAliensKilledTotal++;
			if (unitStatistics.mercyCross)
				_allAliensStunnedTotal++;
		}
		_daysWoundedTotal += unitStatistics.daysWounded;
		if (unitStatistics.daysWounded != 0)
			_timesWoundedTotal++;

		if (unitStatistics.wasUnconcious)
			_unconciousTotal++;
		_shotAtCounterTotal += unitStatistics.shotAtCounter;
		_shotAtCounter10in1Mission += (unitStatistics.shotAtCounter)/10;
		_hitCounterTotal += unitStatistics.hitCounter;
		_hitCounter5in1Mission += (unitStatistics.hitCounter)/5;
		_totalShotByFriendlyCounter += unitStatistics.shotByFriendlyCounter;
		_totalShotFriendlyCounter += unitStatistics.shotFriendlyCounter;
		_longDistanceHitCounterTotal += unitStatistics.longDistanceHitCounter;
		_lowAccuracyHitCounterTotal += unitStatistics.lowAccuracyHitCounter;
		_shotsFiredCounterTotal += unitStatistics.shotsFiredCounter;
		_shotsLandedCounterTotal += unitStatistics.shotsLandedCounter;
		if (unitStatistics.KIA)
			_KIA++;
		if (unitStatistics.MIA)
			_MIA++;
		_woundsHealedTotal += unitStatistics.woundsHealed;
		if (getUFOTotal(allMissionStatistics).Count >= rules.getUfosList().Count)
			_allUFOs = 1;
		if ((getUFOTotal(allMissionStatistics).Count + getTypeTotal(allMissionStatistics).Count) == (rules.getUfosList().Count + rules.getDeploymentsList().Count - 2))
			_allMissionTypes = 1;
		if (getCountryTotal(allMissionStatistics).Count == rules.getCountriesList().Count)
			_globeTrotter = true;
		_martyrKillsTotal += unitStatistics.martyr;
		_slaveKillsTotal += unitStatistics.slaveKills;

		// Stat change long hand calculation
		_statGainTotal = 0; // Reset.
		_statGainTotal += unitStatistics.delta.tu;
		_statGainTotal += unitStatistics.delta.stamina;
		_statGainTotal += unitStatistics.delta.health;
		_statGainTotal += unitStatistics.delta.bravery / 10; // Normalize
		_statGainTotal += unitStatistics.delta.reactions;
		_statGainTotal += unitStatistics.delta.firing;
		_statGainTotal += unitStatistics.delta.throwing;
		_statGainTotal += unitStatistics.delta.strength;
		_statGainTotal += unitStatistics.delta.psiStrength;
		_statGainTotal += unitStatistics.delta.melee;
		_statGainTotal += unitStatistics.delta.psiSkill;

		_braveryGainTotal = unitStatistics.delta.bravery;
		_revivedUnitTotal += (unitStatistics.revivedSoldier + unitStatistics.revivedHostile + unitStatistics.revivedNeutral);
		_revivedSoldierTotal += unitStatistics.revivedSoldier;
		_revivedNeutralTotal += unitStatistics.revivedNeutral;
		_revivedHostileTotal += unitStatistics.revivedHostile;
		_wholeMedikitTotal += Math.Min( Math.Min(unitStatistics.woundsHealed, unitStatistics.appliedStimulant), unitStatistics.appliedPainKill);
		_missionIdList.Add(missionStatistics.id);
	}

	/**
	 *  Get a map of the amount of missions done in each country.
	 *  @param MissionStatistics
	 */
	Dictionary<string, int> getCountryTotal(List<MissionStatistics> missionStatistics)
	{
		var countryTotal = new Dictionary<string, int>();

		foreach (var i in missionStatistics)
		{
			foreach (var j in _missionIdList)
			{
				if (j == i.id)
				{
					countryTotal[i.country]++;
				}
			}
		}

		return countryTotal;
	}

	/**
	 * Award commendations to the soldier.
	 * @param type string
	 * @param noun string
	 */
	void awardCommendation(string type, string noun = "noNoun")
	{
		bool newCommendation = true;
		foreach (var i in _commendations)
		{
			if ( i.getType() == type && i.getNoun() == noun)
			{
				i.addDecoration();
				newCommendation = false;
				break;
			}
		}
		if (newCommendation)
		{
			_commendations.Add(new SoldierCommendations(type, noun));
		}
	}
}
