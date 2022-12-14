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
     * Initializes a soldier commendation.
     */
    SoldierCommendations(string commendationName, string noun)
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
}
