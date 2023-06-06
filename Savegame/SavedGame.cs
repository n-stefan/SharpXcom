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
 * Enumerator for the various save types.
 */
enum SaveType { SAVE_DEFAULT, SAVE_QUICK, SAVE_AUTO_GEOSCAPE, SAVE_AUTO_BATTLESCAPE, SAVE_IRONMAN, SAVE_IRONMAN_END };

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
    internal const string AUTOSAVE_GEOSCAPE = "_autogeo_.asav";
    internal const string AUTOSAVE_BATTLESCAPE = "_autobattle_.asav";
    internal const string QUICKSAVE = "_quick_.asav";

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
    internal Soldier killSoldier(Soldier soldier, BattleUnitKills cause = default)
    {
        foreach (var i in _bases)
        {
            foreach (var j in i.getSoldiers())
            {
                if (j == soldier)
                {
                    soldier.die(new SoldierDeath(_time, cause));
                    _deadSoldiers.Add(soldier);
                    i.getSoldiers().Remove(j);
                    return j;
                }
            }
        }
        return null;
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
    internal int getMonthsPassed() =>
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

    /**
     * Add a ResearchProject to the list of already discovered ResearchProject
     * @param research The newly found ResearchProject
     * @param mod the game Mod
     * @param base the base, in which the project was finished
     * @param score should the score be awarded or not?
     */
    internal void addFinishedResearch(RuleResearch research, Mod.Mod mod, Base @base, bool score = true)
    {
        // Not really a queue in C++ terminology (we don't need or want pop_front())
        var queue = new List<RuleResearch> { research };

        int currentQueueIndex = 0;
	    while (queue.Count > currentQueueIndex)
	    {
		    RuleResearch currentQueueItem = queue[currentQueueIndex];

		    // 1. Find out and remember if the currentQueueItem has any undiscovered "protected unlocks"
		    bool hasUndiscoveredProtectedUnlocks = hasUndiscoveredProtectedUnlock(currentQueueItem, mod);

		    // 2. If the currentQueueItem was *not* already discovered before, add it to discovered research
		    bool checkRelatedZeroCostTopics = true;
		    if (!isResearched(currentQueueItem.getName(), false))
		    {
			    _discovered.Add(currentQueueItem);
			    if (!hasUndiscoveredProtectedUnlocks && isResearched(currentQueueItem.getGetOneFree(), false))
			    {
				    // If the currentQueueItem can't tell you anything anymore, remove it from popped research
				    // Note: this is for optimisation purposes only, functionally it is *not* required...
				    // ... removing it prematurely leads to bugs, maybe we should not do it at all?
				    removePoppedResearch(currentQueueItem);
			    }
			    if (score)
			    {
				    addResearchScore(currentQueueItem.getPoints());
			    }
		    }
		    else
		    {
			    // If the currentQueueItem *was* already discovered before, check if it has any undiscovered "protected unlocks".
			    // If not, all zero-cost topics have already been processed before (during the first discovery)
			    // and we can basically terminate here (i.e. skip step 3.).
			    if (!hasUndiscoveredProtectedUnlocks)
			    {
				    checkRelatedZeroCostTopics = false;
			    }
		    }

		    // 3. If currentQueueItem is completed for the *first* time, or if it has any undiscovered "protected unlocks",
		    // process all related zero-cost topics
		    if (checkRelatedZeroCostTopics)
		    {
			    // 3a. Gather all available research projects
			    var availableResearch = new List<RuleResearch>();
			    if (@base != null)
			    {
				    // Note: even if two different but related projects are finished in two different bases at the same time,
				    // the algorithm is robust enough to treat them *sequentially* (i.e. as if one was researched first and the other second),
				    // thus calling this method for *one* base only is enough
				    getAvailableResearchProjects(availableResearch, mod, @base);
			    }
			    else
			    {
				    // Used in vanilla save converter only
				    getAvailableResearchProjects(availableResearch, mod, null);
			    }

			    // 3b. Iterate through all available projects and add zero-cost projects to the processing queue
			    foreach (var itProjectToTest in availableResearch)
			    {
				    // We are only interested in zero-cost projects!
				    if (itProjectToTest.getCost() == 0)
				    {
					    // We are only interested in *new* projects (i.e. not processed or scheduled for processing yet)
					    bool isAlreadyInTheQueue = false;
					    foreach (var itQueue in queue)
					    {
						    if (itQueue.getName() == itProjectToTest.getName())
						    {
							    isAlreadyInTheQueue = true;
							    break;
						    }
					    }

					    if (!isAlreadyInTheQueue)
					    {
						    if (!itProjectToTest.getRequirements().Any())
						    {
							    // no additional checks for "unprotected" topics
							    queue.Add(itProjectToTest);
						    }
						    else
						    {
							    // for "protected" topics, we need to check if the currentQueueItem can unlock it or not
							    foreach (var itUnlocks in currentQueueItem.getUnlocked())
							    {
								    if (itProjectToTest.getName() == itUnlocks)
								    {
									    queue.Add(itProjectToTest);
									    break;
								    }
							    }
						    }
					    }
				    }
			    }
		    }

		    // 4. process remaining items in the queue
		    ++currentQueueIndex;
	    }
    }

    /**
     * Get the list of RuleResearch which can be researched in a Base.
     * @param projects the list of ResearchProject which are available.
     * @param mod the game Mod
     * @param base a pointer to a Base
     * @param considerDebugMode Should debug mode be considered or not.
     */
    internal void getAvailableResearchProjects(List<RuleResearch> projects, Mod.Mod mod, Base @base, bool considerDebugMode = false)
    {
	    // This list is used for topics that can be researched even if *not all* dependencies have been discovered yet (e.g. STR_ALIEN_ORIGINS)
	    // Note: all requirements of such topics *have to* be discovered though! This will be handled elsewhere.
	    var unlocked = new List<RuleResearch>();
	    foreach (var it in _discovered)
	    {
		    foreach (var itUnlocked in it.getUnlocked())
		    {
			    unlocked.Add(mod.getResearch(itUnlocked, true));
		    }
	    }

	    // Create a list of research topics available for research in the given base
	    foreach (var iter in mod.getResearchList())
	    {
		    RuleResearch research = mod.getResearch(iter);

		    if ((considerDebugMode && _debug) || unlocked.Contains(research))
		    {
			    // Empty, these research topics are on the "unlocked list", *don't* check the dependencies!
		    }
		    else
		    {
			    // These items are not on the "unlocked list", we must check if "dependencies" are satisfied!
			    if (!isResearched(research.getDependencies(), considerDebugMode))
			    {
				    continue;
			    }
		    }

		    // Check if "requires" are satisfied
		    // IMPORTANT: research topics with "requires" will NEVER be directly visible to the player anyway
		    //   - there is an additional filter in NewResearchListState::fillProjectList(), see comments there for more info
		    //   - there is an additional filter in NewPossibleResearchState::NewPossibleResearchState()
		    //   - we do this check for other functionality using this method, namely SavedGame::addFinishedResearch()
		    //     - Note: when called from there, parameter considerDebugMode = false
		    if (!isResearched(research.getRequirements(), considerDebugMode))
		    {
			    continue;
		    }

		    // Remove the already researched topics from the list *UNLESS* they can still give you something more
		    if (isResearched(research.getName(), false))
		    {
			    if (!isResearched(research.getGetOneFree(), false))
			    {
				    // This research topic still has some more undiscovered "getOneFree" topics, keep it!
			    }
			    else if (hasUndiscoveredProtectedUnlock(research, mod))
			    {
				    // This research topic still has one or more undiscovered "protected unlocks", keep it!
			    }
			    else
			    {
				    // This topic can't give you anything else anymore, ignore it!
				    continue;
			    }
		    }

		    if (@base != null)
		    {
			    // Check if this topic is already being researched in the given base
			    List<ResearchProject> baseResearchProjects = @base.getResearch();
			    if (baseResearchProjects.Any(x => x.getRules() == research))
			    {
				    continue;
			    }

			    // Check for needed item in the given base
			    if (research.needItem() && @base.getStorageItems().getItem(research.getName()) == 0)
			    {
				    continue;
			    }
		    }
		    else
		    {
			    // Used in vanilla save converter only
			    if (research.needItem() && research.getCost() == 0)
			    {
				    continue;
			    }
		    }

		    // Haleluja, all checks passed, add the research topic to the list
		    projects.Add(research);
	    }
    }

    /**
     * Returns if a certain research topic has been completed.
     * @param research Research ID.
     * @param considerDebugMode Should debug mode be considered or not.
     * @return Whether it's researched or not.
     */
    internal bool isResearched(string research, bool considerDebugMode = true)
    {
	    //if (research.empty())
	    //	return true;
	    if (considerDebugMode && _debug)
		    return true;
	    foreach (var i in _discovered)
	    {
		    if (i.getName() == research)
			    return true;
	    }

	    return false;
    }

    /**
     * Returns if a certain list of research topics has been completed.
     * @param research List of research IDs.
     * @param considerDebugMode Should debug mode be considered or not.
     * @return Whether it's researched or not.
     */
    internal bool isResearched(List<string> research, bool considerDebugMode = true)
    {
	    if (!research.Any())
		    return true;
	    if (considerDebugMode && _debug)
		    return true;
	    var matches = research;
        for (var i = 0; i < _discovered.Count; ++i)
	    {
		    for (var j = 0; j < matches.Count; ++j)
		    {
			    if (_discovered[i].getName() == matches[j])
			    {
				    /* j = */ matches.RemoveAt(j);
				    break;
			    }
		    }
		    if (!matches.Any())
			    return true;
	    }

	    return false;
    }

    /**
     * adds to this month's research score
     * @param score the amount to add.
     */
    void addResearchScore(int score) =>
        _researchScores[^1] += score;

    /*
     * checks for and removes a research project from the "has been popped up" array
     * @param research is the project we are checking for and removing, if necessary.
     */
    void removePoppedResearch(RuleResearch research)
    {
        var index = _poppedResearch.IndexOf(research);
        if (index != -1)
	    {
		    _poppedResearch.RemoveAt(index);
	    }
    }

    /**
     * Returns if a research still has undiscovered "protected unlocks".
     * @param r Research to check.
     * @param mod the Game Mod
     * @return Whether it has any undiscovered "protected unlocks" or not.
     */
    internal bool hasUndiscoveredProtectedUnlock(RuleResearch r, Mod.Mod mod)
    {
	    // Note: checking for not yet discovered unlocks protected by "requires" (which also implies cost = 0)
	    foreach (var itUnlocked in r.getUnlocked())
	    {
		    RuleResearch unlock = mod.getResearch(itUnlocked, true);
		    if (unlock.getRequirements().Any())
		    {
			    if (!isResearched(unlock.getName(), false))
			    {
				    return true;
			    }
		    }
	    }
	    return false;
    }

    /*
     * marks a research topic as having already come up as "we can now research"
     * @param research is the project we want to add to the vector
     */
    internal void addPoppedResearch(RuleResearch research)
    {
	    if (!wasResearchPopped(research))
		    _poppedResearch.Add(research);
    }

    /*
     * checks if an unresearched topic has previously been popped up.
     * @param research is the project we are checking for
     * @return whether or not it has been popped up.
     */
    bool wasResearchPopped(RuleResearch research) =>
        _poppedResearch.Contains(research);

    /**
     * Returns the current time of the game.
     * @return Pointer to the game time.
     */
    internal GameTime getTime() =>
	    _time;

    /**
     * Returns the player's current funds.
     * @return Current funds.
     */
    internal long getFunds() =>
	    _funds.Last();

    /**
     * Returns the last selected player base.
     * @return Pointer to base.
     */
    internal Base getSelectedBase()
    {
        // in case a base was destroyed or something...
        if (_selectedBase < _bases.Count)
        {
            return _bases[(int)_selectedBase];
        }
        else
        {
            return _bases.First();
        }
    }

    /**
     * Gives the player his monthly funds, taking in account
     * all maintenance and profit costs.
     */
    internal void monthlyFunding()
    {
        _funds[_funds.Count - 1] += getCountryFunding() - getBaseMaintenance();
        _funds.Add(_funds.Last());
        _maintenance[_maintenance.Count - 1] = getBaseMaintenance();
        _maintenance.Add(0);
        _incomes.Add(getCountryFunding());
        _expenditures.Add(getBaseMaintenance());
        _researchScores.Add(0);

        if (_incomes.Count > 12)
            _incomes.RemoveAt(0);
        if (_expenditures.Count > 12)
            _expenditures.RemoveAt(0);
        if (_researchScores.Count > 12)
            _researchScores.RemoveAt(0);
        if (_funds.Count > 12)
            _funds.RemoveAt(0);
        if (_maintenance.Count > 12)
            _maintenance.RemoveAt(0);
    }

    /**
     * Adds up the monthly funding of all the countries.
     * @return Total funding.
     */
    int getCountryFunding()
    {
	    int total = 0;
	    foreach (var i in _countries)
	    {
		    total += i.getFunding().Last();
	    }
	    return total;
    }

    /**
     * Adds up the monthly maintenance of all the bases.
     * @return Total maintenance.
     */
    int getBaseMaintenance()
    {
	    int total = 0;
	    foreach (var i in _bases)
	    {
		    total += i.getMonthlyMaintenace();
	    }
	    return total;
    }

    /**
     * Returns the game's difficulty level.
     * @return Difficulty level.
     */
    internal GameDifficulty getDifficulty() =>
	    _difficulty;

    /**
     * Get the list of newly available manufacture projects once a ResearchProject has been completed. This function check for fake ResearchProject.
     * @param dependables the list of RuleManufacture which are now available.
     * @param research The RuleResearch which has just been discovered
     * @param mod the Game Mod
     * @param base a pointer to a Base
     */
    internal void getDependableManufacture(List<RuleManufacture> dependables, RuleResearch research, Mod.Mod mod, Base _)
    {
	    List<string> mans = mod.getManufactureList();
	    foreach (var iter in mans)
	    {
		    RuleManufacture m = mod.getManufacture(iter);
		    List<string> reqs = m.getRequirements();
		    if (isResearched(m.getRequirements()) && reqs.Contains(research.getName()))
		    {
			    dependables.Add(m);
		    }
	    }
    }

    /**
     * Changes the player's funds to a new value.
     * @param funds New funds.
     */
    internal void setFunds(long funds)
    {
        if (_funds.Last() > funds)
        {
            _expenditures[^1] += _funds.Last() - funds;
        }
        else
        {
            _incomes[^1] += funds - _funds.Last();
        }
        _funds[^1] = funds;
    }

    /**
     * Find the region containing this target.
     * @param target The target to locate.
     * @return Pointer to the region, or 0.
     */
    internal Region locateRegion(Target target) =>
	    locateRegion(target.getLongitude(), target.getLatitude());

    /**
     * Find the region containing this location.
     * @param lon The longtitude.
     * @param lat The latitude.
     * @return Pointer to the region, or 0.
     */
    internal Region locateRegion(double lon, double lat) =>
	    _regions.Find(x => x.getRules().insideRegion(lon, lat));

    /**
     * Returns the game's current ending.
     * @return Ending state.
     */
    internal GameEnding getEnding() =>
	    _end;

    /**
     * Changes the game's current ending.
     * @param end New ending.
     */
    internal void setEnding(GameEnding end) =>
	    _end = end;

    /**
     * Set battleGame object.
     * @param battleGame Pointer to the battleGame object.
     */
    internal void setBattleGame(SavedBattleGame battleGame) =>
        _battleGame = battleGame;

    /**
     * Returns the game's difficulty coefficient based
     * on the current level.
     * @return Difficulty coefficient.
     */
    internal int getDifficultyCoefficient() =>
	    Mod.Mod.DIFFICULTY_COEFFICIENT[Math.Min((int)_difficulty, 4)];
}
