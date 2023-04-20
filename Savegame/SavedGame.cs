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
 * Enumerator containing all the possible game difficulties.
 */
enum GameDifficulty { DIFF_BEGINNER = 0, DIFF_EXPERIENCED, DIFF_VETERAN, DIFF_GENIUS, DIFF_SUPERHUMAN };

/**
 * Enumerator for the current game ending.
 */
enum GameEnding { END_NONE, END_WIN, END_LOSE };

/**
 * Container for savegame info displayed on listings.
 */
struct SaveInfo
{
    internal string fileName;
    internal string displayName;
    internal long timestamp;
    internal string isoDate, isoTime;
    internal string details;
    List<string> mods;
    internal bool reserved;
}

/**
 * The game data that gets written to disk when the game is saved.
 * A saved game holds all the variable info in a game like funds,
 * game time, current bases and contents, world activities, score, etc.
 */
internal class SavedGame
{
    GameDifficulty _difficulty;
    GameEnding _end;
    bool _ironman;
    GameTime _time;
    List<int> _researchScores;
    List<long> _funds, _maintenance, _incomes, _expenditures;
    double _globeLon, _globeLat;
    int _globeZoom;
    List<Country> _countries;
    List<Region> _regions;
    List<Base> _bases;
    List<Ufo> _ufos;
    List<Waypoint> _waypoints;
    List<MissionSite> _missionSites;
    List<AlienBase> _alienBases;
    AlienStrategy _alienStrategy;
    SavedBattleGame _battleGame;
    List<AlienMission> _activeMissions;
    bool _debug, _warned;
    int _monthsPassed;
    List<Soldier> _deadSoldiers;
    uint _selectedBase;
    string _lastselectedArmor; //contains the last selected armour
    List<MissionStatistics> _missionStatistics;
    string _name;
    string _graphRegionToggles;
    string _graphCountryToggles;
    string _graphFinanceToggles;
    Dictionary<string, int> _ids;
    List<RuleResearch> _discovered;
    List<RuleResearch> _poppedResearch;

    /**
     * Initializes a brand new saved game according to the specified difficulty.
     */
    internal SavedGame()
    {
        _difficulty = GameDifficulty.DIFF_BEGINNER;
        _end = GameEnding.END_NONE;
        _ironman = false;
        _globeLon = 0.0;
        _globeLat = 0.0;
        _globeZoom = 0;
        _battleGame = null;
        _debug = false;
        _warned = false;
        _monthsPassed = -1;
        _selectedBase = 0;

        _time = new GameTime(6, 1, 1, 1999, 12, 0, 0);
        _alienStrategy = new AlienStrategy();
        _funds.Add(0);
        _maintenance.Add(0);
        _researchScores.Add(0);
        _incomes.Add(0);
        _expenditures.Add(0);
        _lastselectedArmor = "STR_NONE_UC";
    }

    /**
     * Deletes the game content from memory.
     */
    ~SavedGame()
    {
        _time = null;
        _countries.Clear();
        _regions.Clear();
        _bases.Clear();
        _ufos.Clear();
        _waypoints.Clear();
        _missionSites.Clear();
        _alienBases.Clear();
        _alienStrategy = null;
        _activeMissions.Clear();
        _deadSoldiers.Clear();
        _missionStatistics.Clear();

        _battleGame = null;
    }

    /**
     * Get pointer to the battleGame object.
     * @return Pointer to the battleGame object.
     */
    internal SavedBattleGame getSavedBattle() =>
        _battleGame;

    /**
     * Returns the list of mission statistics.
     * @return Pointer to statistics list.
     */
    internal List<MissionStatistics> getMissionStatistics() =>
        _missionStatistics;

    /**
     * Registers a soldier's death in the memorial.
     * @param soldier Pointer to dead soldier.
     * @param cause Pointer to cause of death, NULL if missing in action.
     */
    internal bool killSoldier(Soldier soldier, BattleUnitKills cause)
    {
        foreach (var i in _bases)
        {
            foreach (var j in i.getSoldiers())
            {
                if (j == soldier)
                {
                    soldier.die(new SoldierDeath(_time, cause));
                    _deadSoldiers.Add(soldier);
                    return i.getSoldiers().Remove(j);
                }
            }
        }
        return false;
    }

    /**
     * Returns if the game is set to ironman mode.
     * Ironman games cannot be manually saved.
     * @return Tony Stark
     */
    internal bool isIronman() =>
	    _ironman;

    /**
     * Returns the game's name shown in Save screens.
     * @return Save name.
     */
    internal string getName() =>
	    _name;

    /**
	 * Saves a saved game's contents to a YAML file.
	 * @param filename YAML filename.
	 */
    internal void save(string filename)
	{
		string savPath = Options.getMasterUserFolder() + filename;
		string tmpPath = savPath + ".tmp";
		using var tmp = new StreamWriter(tmpPath);
		if (tmp == null)
		{
			throw new Exception("Failed to save " + filename);
		}

        //YAML::Emitter out;
        var doc = new YamlDocument("root");
        var str = new YamlStream(doc);

        // Saves the brief game info used in the saves list
        var brief = new YamlMappingNode
        {
            { "name", _name },
            { "version", OPENXCOM_VERSION_SHORT },
            { "engine", OPENXCOM_VERSION_ENGINE }
        };
        string git_sha = OPENXCOM_VERSION_GIT;
		if (!string.IsNullOrEmpty(git_sha) && git_sha[0] == '.')
		{
			git_sha = git_sha.Remove(0, 1);
		}
		brief.Add("build", git_sha);
		brief.Add("time", _time.save());
		if (_battleGame != null)
		{
			brief.Add("mission", _battleGame.getMissionType());
			brief.Add("turn", _battleGame.getTurn().ToString());
		}

		// only save mods that work with the current master
		List<ModInfo> activeMods = Options.getActiveMods();
		var modsList = new List<string>();
		foreach (var activeMod in activeMods)
		{
			modsList.Add(activeMod.getId() + " ver: " + activeMod.getVersion());
		}
		brief.Add("mods", new YamlSequenceNode(modsList.Select(x => new YamlScalarNode(x))));
		if (_ironman)
			brief.Add("ironman", _ironman.ToString());
        ((YamlSequenceNode)doc.RootNode).Add(brief);
        // Saves the full game data to the save
        //out << YAML::BeginDoc;
        var node = new YamlMappingNode
        {
            { "difficulty", ((int)_difficulty).ToString() },
            { "end", ((int)_end).ToString() },
            { "monthsPassed", _monthsPassed.ToString() },
            { "graphRegionToggles", _graphRegionToggles },
            { "graphCountryToggles", _graphCountryToggles },
            { "graphFinanceToggles", _graphFinanceToggles },
            { "rng", RNG.getSeed().ToString() },
            { "funds", new YamlSequenceNode(_funds.Select(x => new YamlScalarNode(x.ToString()))) },
            { "maintenance", new YamlSequenceNode(_maintenance.Select(x => new YamlScalarNode(x.ToString()))) },
            { "researchScores", new YamlSequenceNode(_researchScores.Select(x => new YamlScalarNode(x.ToString()))) },
            { "incomes", new YamlSequenceNode(_incomes.Select(x => new YamlScalarNode(x.ToString()))) },
            { "expenditures", new YamlSequenceNode(_expenditures.Select(x => new YamlScalarNode(x.ToString()))) },
            { "warned", _warned.ToString() },
            { "globeLon", _globeLon.ToString() },
            { "globeLat", _globeLat.ToString() },
            { "globeZoom", _globeZoom.ToString() },
            { "ids", new YamlSequenceNode(_ids.Select(x => new YamlMappingNode(x.Key, x.Value.ToString()))) },
            { "countries", new YamlSequenceNode(_countries.Select(x => x.save())) },
            { "regions", new YamlSequenceNode(_regions.Select(x => x.save())) },
            { "bases", new YamlSequenceNode(_bases.Select(x => x.save())) },
            { "waypoints", new YamlSequenceNode(_waypoints.Select(x => x.save())) },
            { "missionSites", new YamlSequenceNode(_missionSites.Select(x => x.save())) },
            // Alien bases must be saved before alien missions.
            { "alienBases", new YamlSequenceNode(_alienBases.Select(x => x.save())) },
            // Missions must be saved before UFOs, but after alien bases.
            { "alienMissions", new YamlSequenceNode(_activeMissions.Select(x => x.save())) },
            // UFOs must be after missions
            { "ufos", new YamlSequenceNode(_ufos.Select(x => x.save(getMonthsPassed() == -1))) },
            { "discovered", new YamlSequenceNode(_discovered.Select(x => new YamlScalarNode(x.getName()))) },
            { "poppedResearch", new YamlSequenceNode(_poppedResearch.Select(x => new YamlScalarNode(x.getName()))) },
            { "alienStrategy", _alienStrategy.save() },
            { "deadSoldiers", new YamlSequenceNode(_deadSoldiers.Select(x => x.save())) }
        };
        if (Options.soldierDiaries)
		{
            node.Add("missionStatistics", new YamlSequenceNode(_missionStatistics.Select(x => x.save())));
		}
		if (_battleGame != null)
		{
			node.Add("battleGame", _battleGame.save());
		}
        ((YamlSequenceNode)doc.RootNode).Add(node);

		// Save to temp
		// If this goes wrong, the original save will be safe
		str.Save(tmp);
		tmp.Close();
		if (tmp == null)
		{
			throw new Exception("Failed to save " + filename);
		}

		// If temp went fine, save for real
		// If this goes wrong, they will have the temp
        using var sav = new StreamWriter(savPath);
        if (sav == null)
		{
			throw new Exception("Failed to save " + filename);
		}
        str.Save(sav);
		sav.Close();
		if (sav == null)
		{
			throw new Exception("Failed to save " + filename);
		}

        // Everything went fine, delete the temp
        // We don't care if this fails
        CrossPlatform.deleteFile(tmpPath);
	}

    /*
     * @return the month counter.
     */
    int getMonthsPassed() =>
	    _monthsPassed;

    /**
     * Returns the current longitude of the Geoscape globe.
     * @return Longitude.
     */
    internal double getGlobeLongitude() =>
	    _globeLon;

    /**
     * Returns the current latitude of the Geoscape globe.
     * @return Latitude.
     */
    internal double getGlobeLatitude() =>
	    _globeLat;

    /**
     * Returns the current zoom level of the Geoscape globe.
     * @return Zoom level.
     */
    internal int getGlobeZoom() =>
	    _globeZoom;

    /**
     * Changes the current zoom level of the Geoscape globe.
     * @param zoom Zoom level.
     */
    internal void setGlobeZoom(int zoom) =>
        _globeZoom = zoom;

    /**
     * Returns the list of player bases.
     * @return Pointer to base list.
     */
    internal List<Base> getBases() =>
        _bases;

    /**
     * Returns the list of craft waypoints.
     * @return Pointer to waypoint list.
     */
    internal List<Waypoint> getWaypoints() =>
        _waypoints;

    /**
     * Returns the list of mission sites.
     * @return Pointer to mission site list.
     */
    internal List<MissionSite> getMissionSites() =>
        _missionSites;

    /**
      * Returns the list of alien bases.
      * @return Pointer to alien base list.
      */
    internal List<AlienBase> getAlienBases() =>
        _alienBases;

    /**
     * Returns the list of alien UFOs.
     * @return Pointer to UFO list.
     */
    internal List<Ufo> getUfos() =>
        _ufos;

    /**
     * Changes the current longitude of the Geoscape globe.
     * @param lon Longitude.
     */
    internal void setGlobeLongitude(double lon) =>
        _globeLon = lon;

    /**
     * Changes the current latitude of the Geoscape globe.
     * @param lat Latitude.
     */
    internal void setGlobeLatitude(double lat) =>
        _globeLat = lat;

    /**
     * return the list of income scores
     * @return list of income scores.
     */
    internal ref List<long> getIncomes() =>
	    ref _incomes;

    /**
     * Returns the list of countries in the game world.
     * @return Pointer to country list.
     */
    internal List<Country> getCountries() =>
        _countries;

    /**
     * Returns the list of world regions.
     * @return Pointer to region list.
     */
    internal List<Region> getRegions() =>
        _regions;

    /**
     * return the list of expenditures scores
     * @return list of expenditures scores.
     */
    internal ref List<long> getExpenditures() =>
	    ref _expenditures;

    /**
     * return the list of monthly maintenance costs
     * @return list of maintenances.
     */
    internal ref List<long> getMaintenances() =>
	    ref _maintenance;

    /**
     * Returns the player's funds for the last 12 months.
     * @return funds.
     */
    internal ref List<long> getFundsList() =>
	    ref _funds;

    /**
     * return the list of research scores
     * @return list of research scores.
     */
    internal ref List<int> getResearchScores() =>
	    ref _researchScores;

    /// Full access to the alien strategy data.
    internal AlienStrategy getAlienStrategy() =>
        _alienStrategy;

    /**
     * Resets the list of unique object IDs.
     * @param ids New ID list.
     */
    internal void setAllIds(Dictionary<string, int> ids) =>
	    _ids = ids;

    /**
     * Changes the current time of the game.
     * @param time Game time.
     */
    internal void setTime(GameTime time) =>
	    _time = time;

    /**
     * Changes the game's difficulty to a new level.
     * @param difficulty New difficulty.
     */
    internal void setDifficulty(GameDifficulty difficulty) =>
        _difficulty = difficulty;

    /*
     * Increment the month counter.
     */
    internal void addMonth() =>
        ++_monthsPassed;

    /**
     * Returns the latest ID for the specified object
     * and increases it.
     * @param name Object name.
     * @return Latest ID number.
     */
    internal int getId(string name)
    {
	    if (_ids.TryGetValue(name, out var id))
	    {
            return id++;
        }
        else
	    {
		    _ids[name] = 1;
		    return _ids[name]++;
	    }
    }

    /// Full access to the current alien missions.
    internal List<AlienMission> getAlienMissions() =>
        _activeMissions;
}
