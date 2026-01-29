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
    internal List<string> mods;
    internal bool reserved;
}

struct PromotionInfo
{
	internal int totalCommanders;
	internal int totalColonels;
	internal int totalCaptains;
	internal int totalSergeants;
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
	List<Soldier> _soldiers;

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
		try
		{
			using var tmp = new StreamWriter(tmpPath);

            var @out = new Emitter(tmp);
            var serializer = new Serializer();

			// Saves the brief game info used in the saves list
			var brief = new YamlMappingNode
			{
				{ "name", _name },
				{ "version", SHARPXCOM_VERSION_SHORT },
				{ "engine", SHARPXCOM_VERSION_ENGINE }
			};
			string git_sha = SHARPXCOM_VERSION_GIT;
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
			foreach (var i in activeMods)
			{
				modsList.Add(i.getId() + " ver: " + i.getVersion());
			}
			brief.Add("mods", new YamlSequenceNode(modsList.Select(x => new YamlScalarNode(x))));
			if (_ironman)
				brief.Add("ironman", _ironman.ToString());
            serializer.Serialize(@out, brief);
			// Saves the full game data to the save
            @out.Emit(new DocumentStart());
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
				{ "globeLon", serializeDouble(_globeLon) },
				{ "globeLat", serializeDouble(_globeLat) },
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
            serializer.Serialize(@out, node);

			// Save to temp
			// If this goes wrong, the original save will be safe
			tmp.Close();

			// If temp went fine, save for real
			// If this goes wrong, they will have the temp
			File.Copy(tmpPath, savPath);
		}
		catch (Exception)
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
    internal bool wasResearchPopped(RuleResearch research) =>
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
    internal int getCountryFunding()
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
    internal int getBaseMaintenance()
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
    internal void getDependableManufacture(List<RuleManufacture> dependables, RuleResearch research, Mod.Mod mod, Base @base)
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

    /**
     * Sets the last selected player base.
     * @param base number of the base.
     */
    internal void setSelectedBase(uint @base) =>
        _selectedBase = @base;

    /**
     * Returns the list of dead soldiers.
     * @return Pointer to soldier list.
     */
    internal List<Soldier> getDeadSoldiers() =>
        _deadSoldiers;

    /**
     * Changes the game's name shown in Save screens.
     * @param name New name.
     */
    internal void setName(string name) =>
	    _name = name;

    /**
     * Sets the last selected armour.
     * @param value The new value for last selected armor - Armor type string.
     */

    internal void setLastSelectedArmor(string value) =>
	    _lastselectedArmor = value;

    /**
     * Find a mission type in the active alien missions.
     * @param region The region string ID.
     * @param objective The active mission objective.
     * @return A pointer to the mission, or 0 if no mission matched.
     */
    internal AlienMission findAlienMission(string region, MissionObjective objective) =>
        _activeMissions.Find(x => x.getRegion() == region && x.getRules().getObjective() == objective);

    /**
     * return if the player has been
     * warned about poor performance.
     * @return true or false.
     */
    internal bool getWarned() =>
	    _warned;

    /**
     * sets the player's "warned" status.
     * @param warned set "warned" to this.
     */
    internal void setWarned(bool warned) =>
        _warned = warned;

    /**
     * Returns pointer to the Soldier given it's unique ID.
     * @param id A soldier's unique id.
     * @return Pointer to Soldier.
     */
    internal Soldier getSoldier(int id)
    {
	    foreach (var i in _bases)
	    {
		    foreach (var j in i.getSoldiers())
		    {
			    if (j.getId() == id)
			    {
				    return j;
			    }
		    }
	    }
	    foreach (var j in _deadSoldiers)
	    {
		    if (j.getId() == id)
		    {
			    return j;
		    }
	    }
	    return null;
    }

    /*
     * @return the GraphRegionToggles.
     */
    internal string getGraphRegionToggles() =>
	    _graphRegionToggles;

    /*
     * @return the GraphCountryToggles.
     */
    internal string getGraphCountryToggles() =>
	    _graphCountryToggles;

    /*
     * @return the GraphFinanceToggles.
     */
    internal string getGraphFinanceToggles() =>
	    _graphFinanceToggles;

    /**
     * Sets the GraphRegionToggles.
     * @param value The new value for GraphRegionToggles.
     */
    internal void setGraphRegionToggles(string value) =>
	    _graphRegionToggles = value;

    /**
     * Sets the GraphCountryToggles.
     * @param value The new value for GraphCountryToggles.
     */
    internal void setGraphCountryToggles(string value) =>
	    _graphCountryToggles = value;

    /**
     * Sets the GraphFinanceToggles.
     * @param value The new value for GraphFinanceToggles.
     */
    internal void setGraphFinanceToggles(string value) =>
	    _graphFinanceToggles = value;

    /**
     * Gets the last selected armour
     * @return last used armor type string
     */
    internal string getLastSelectedArmor() =>
	    _lastselectedArmor;

    /**
     * Changes if the game is set to ironman mode.
     * Ironman games cannot be manually saved.
     * @param ironman Tony Stark
     */
    internal void setIronman(bool ironman) =>
        _ironman = ironman;

    /**
     * Add a ResearchProject to the list of already discovered ResearchProject
     * @param research The newly found ResearchProject
     */
    internal void addFinishedResearchSimple(RuleResearch research) =>
	    _discovered.Add(research);

    /**
     * Get the list of RuleManufacture which can be manufacture in a Base.
     * @param productions the list of Productions which are available.
     * @param mod the Game Mod
     * @param base a pointer to a Base
     */
    internal void getAvailableProductions(List<RuleManufacture> productions, Mod.Mod mod, Base @base)
    {
	    List<string> items = mod.getManufactureList();
	    List<Production> baseProductions = @base.getProductions();

	    foreach (var iter in items)
	    {
		    RuleManufacture m = mod.getManufacture(iter);
		    if (!isResearched(m.getRequirements()))
		    {
			    continue;
		    }
            if (baseProductions.Any(x => x.getRules() == m))
		    {
			    continue;
		    }
		    productions.Add(m);
	    }
    }

    /**
     * Returns the list of soldiers in the base.
     * @return Pointer to the soldier list.
     */
    internal List<Soldier> getSoldiers() =>
	    _soldiers;

    /**
    * Resets the list of unique object IDs.
    * @param ids New ID list.
    */
    internal Dictionary<string, int> getAllIds() =>
	    _ids;

    /**
     *  Returns the list of already discovered ResearchProject
     * @return the list of already discovered ResearchProject
     */
    internal List<RuleResearch> getDiscoveredResearch() =>
	    _discovered;

    /**
     * Handles the higher promotions (not the rookie-squaddie ones).
     * @param participants a list of soldiers that were actually present at the battle.
     * @return Whether or not some promotions happened - to show the promotions screen.
     */
    internal bool handlePromotions(List<Soldier> participants)
    {
	    int soldiersPromoted = 0;
	    Soldier highestRanked = null;
	    var soldierData = new PromotionInfo();
	    var soldiers = new List<Soldier>();
	    foreach (var i in _bases)
	    {
		    foreach (var j in i.getSoldiers())
		    {
			    soldiers.Add(j);
			    processSoldier(j, ref soldierData);
		    }
		    foreach (var j in i.getTransfers())
		    {
			    if (j.getType() == TransferType.TRANSFER_SOLDIER)
			    {
				    soldiers.Add(j.getSoldier());
				    processSoldier(j.getSoldier(), ref soldierData);
			    }
		    }
	    }

	    int totalSoldiers = soldiers.Count;

	    if (soldierData.totalCommanders == 0)
	    {
		    if (totalSoldiers >= 30)
		    {
			    highestRanked = inspectSoldiers(soldiers, participants, SoldierRank.RANK_COLONEL);
			    if (highestRanked != null)
			    {
				    // only promote one colonel to commander
				    highestRanked.promoteRank();
				    soldiersPromoted++;
				    soldierData.totalCommanders++;
				    soldierData.totalColonels--;
			    }
		    }
	    }

	    if ((totalSoldiers / 23) > soldierData.totalColonels)
	    {
		    while ((totalSoldiers / 23) > soldierData.totalColonels)
		    {
			    highestRanked = inspectSoldiers(soldiers, participants, SoldierRank.RANK_CAPTAIN);
			    if (highestRanked != null)
			    {
				    highestRanked.promoteRank();
				    soldiersPromoted++;
				    soldierData.totalColonels++;
				    soldierData.totalCaptains--;
			    }
			    else
			    {
				    break;
			    }
		    }
	    }

	    if ((totalSoldiers / 11) > soldierData.totalCaptains)
	    {
		    while ((totalSoldiers / 11) > soldierData.totalCaptains)
		    {
			    highestRanked = inspectSoldiers(soldiers, participants, SoldierRank.RANK_SERGEANT);
			    if (highestRanked != null)
			    {
				    highestRanked.promoteRank();
				    soldiersPromoted++;
				    soldierData.totalCaptains++;
				    soldierData.totalSergeants--;
			    }
			    else
			    {
				    break;
			    }
		    }
	    }

	    if ((totalSoldiers / 5) > soldierData.totalSergeants)
	    {
		    while ((totalSoldiers / 5) > soldierData.totalSergeants)
		    {
			    highestRanked = inspectSoldiers(soldiers, participants, SoldierRank.RANK_SQUADDIE);
			    if (highestRanked != null)
			    {
				    highestRanked.promoteRank();
				    soldiersPromoted++;
				    soldierData.totalSergeants++;
			    }
			    else
			    {
				    break;
			    }
		    }
	    }

	    return (soldiersPromoted > 0);
    }

    /**
     * Checks how many soldiers of a rank exist and which one has the highest score.
     * @param soldiers full list of live soldiers.
     * @param participants list of participants on this mission.
     * @param rank Rank to inspect.
     * @return the highest ranked soldier
     */
    Soldier inspectSoldiers(List<Soldier> soldiers, List<Soldier> participants, SoldierRank rank)
    {
	    int highestScore = 0;
	    Soldier highestRanked = null;
	    foreach (var i in soldiers)
	    {
		    if (i.getRank() == rank)
		    {
			    int score = getSoldierScore(i);
			    if (score > highestScore && (!Options.fieldPromotions || participants.Contains(i)))
			    {
				    highestScore = score;
				    highestRanked = i;
			    }
		    }
	    }
	    return highestRanked;
    }

    /**
     * Evaluate the score of a soldier based on all of his stats, missions and kills.
     * @param soldier the soldier to get a score for.
     * @return this soldier's score.
     */
    int getSoldierScore(Soldier soldier)
    {
	    UnitStats s = soldier.getCurrentStats();
	    int v1 = 2 * s.health + 2 * s.stamina + 4 * s.reactions + 4 * s.bravery;
	    int v2 = v1 + 3*( s.tu + 2*( s.firing ) );
	    int v3 = v2 + s.melee + s.throwing + s.strength;
	    if (s.psiSkill > 0) v3 += s.psiStrength + 2 * s.psiSkill;
	    return v3 + 10 * ( soldier.getMissions() + soldier.getKills() );
    }

    /**
     * Processes a soldier, and adds their rank to the promotions data array.
     * @param soldier the soldier to process.
     * @param soldierData the data array to put their info into.
     */
    void processSoldier(Soldier soldier, ref PromotionInfo soldierData)
    {
	    switch (soldier.getRank())
	    {
	        case SoldierRank.RANK_COMMANDER:
		        soldierData.totalCommanders++;
		        break;
	        case SoldierRank.RANK_COLONEL:
		        soldierData.totalColonels++;
		        break;
	        case SoldierRank.RANK_CAPTAIN:
		        soldierData.totalCaptains++;
		        break;
	        case SoldierRank.RANK_SERGEANT:
		        soldierData.totalSergeants++;
		        break;
	        default:
		        break;
	    }
    }

    /**
     * Gets the current debug mode.
     * @return Debug mode.
     */
    internal bool getDebugMode() =>
	    _debug;

    /**
     * Gets all the info of the saves found in the user folder.
     * @param lang Loaded language.
     * @param autoquick Include autosaves and quicksaves.
     * @return List of saves info.
     */
    internal static List<SaveInfo> getList(Language lang, bool autoquick)
    {
	    var info = new List<SaveInfo>();
	    string curMaster = Options.getActiveMaster();
	    List<string> saves = CrossPlatform.getFolderContents(Options.getMasterUserFolder(), "sav");

	    if (autoquick)
	    {
		    List<string> asaves = CrossPlatform.getFolderContents(Options.getMasterUserFolder(), "asav");
		    saves.InsertRange(0, asaves);
	    }
	    foreach (var i in saves)
	    {
		    try
		    {
			    SaveInfo saveInfo = getSaveInfo(i, lang);
			    if (!_isCurrentGameType(saveInfo, curMaster))
			    {
				    continue;
			    }
			    info.Add(saveInfo);
		    }
		    catch (YamlException e)
		    {
                Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} {i}: {e.Message}");
			    continue;
		    }
		    catch (Exception e)
		    {
                Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} {i}: {e.Message}");
			    continue;
		    }
	    }

	    return info;
    }

    static bool _isCurrentGameType(SaveInfo saveInfo, string curMaster)
    {
	    bool matchMasterMod = false;
	    if (!saveInfo.mods.Any())
	    {
		    // if no mods listed in the savegame, this is an old-style
		    // savegame.  assume "xcom1" as the game type.
		    matchMasterMod = (curMaster == "xcom1");
	    }
	    else
	    {
		    foreach (var i in saveInfo.mods)
		    {
			    string name = sanitizeModName(i);
			    if (name == curMaster)
			    {
				    matchMasterMod = true;
				    break;
			    }
		    }
	    }

	    if (!matchMasterMod)
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_DEBUG)} skipping save from inactive master: {saveInfo.fileName}");
	    }

	    return matchMasterMod;
    }

    /**
     * Removes version number from a mod name, if any.
     * @param name Mod id from a savegame.
     * @return Sanitized mod name.
     */
    internal static string sanitizeModName(string name)
    {
	    int versionInfoBreakPoint = name.IndexOf(" ver: ");
	    if (versionInfoBreakPoint == -1)
	    {
		    return name;
	    }
	    else
	    {
		    return name.Substring(0, versionInfoBreakPoint);
	    }
    }

    /**
     * Gets the info of a specific save file.
     * @param file Save filename.
     * @param lang Loaded language.
     */
    static SaveInfo getSaveInfo(string file, Language lang)
    {
	    var fullname = Options.getMasterUserFolder() + file;
        using var input = new StreamReader(fullname);
        var yaml = new YamlStream();
        yaml.Load(input);
        var doc = yaml.Documents[0].RootNode;
	    SaveInfo save;

	    save.fileName = file;

	    if (save.fileName == QUICKSAVE)
	    {
		    save.displayName = lang.getString("STR_QUICK_SAVE_SLOT");
		    save.reserved = true;
	    }
	    else if (save.fileName == AUTOSAVE_GEOSCAPE)
	    {
		    save.displayName = lang.getString("STR_AUTO_SAVE_GEOSCAPE_SLOT");
		    save.reserved = true;
	    }
	    else if (save.fileName == AUTOSAVE_BATTLESCAPE)
	    {
		    save.displayName = lang.getString("STR_AUTO_SAVE_BATTLESCAPE_SLOT");
		    save.reserved = true;
	    }
	    else
	    {
		    if (doc["name"] != null)
		    {
			    save.displayName = doc["name"].ToString();
		    }
		    else
		    {
			    save.displayName = Unicode.convPathToUtf8(CrossPlatform.noExt(file));
		    }
		    save.reserved = false;
	    }

	    save.timestamp = CrossPlatform.getDateModified(fullname);
	    KeyValuePair<string, string> str = CrossPlatform.timeToString(save.timestamp);
	    save.isoDate = str.Key;
	    save.isoTime = str.Value;
        save.mods = doc["mods"] != null ? ((YamlSequenceNode)doc["mods"]).Children.Select(x => x.ToString()).ToList() : new List<string>();

	    var details = new StringBuilder();
	    if (doc["turn"] != null)
	    {
		    details.Append($"{lang.getString("STR_BATTLESCAPE")}: {lang.getString(doc["mission"].ToString())}, ");
		    details.Append(lang.getString("STR_TURN").arg(int.Parse(doc["turn"].ToString())));
	    }
	    else
	    {
		    GameTime time = new GameTime(6, 1, 1, 1999, 12, 0, 0);
		    time.load(doc["time"]);
		    details.Append($"{lang.getString("STR_GEOSCAPE")}: ");
		    details.Append($"{time.getDayString(lang)} {lang.getString(time.getMonthString())} {time.getYear()}, ");
		    details.Append($"{time.getHour()}:{time.getMinute():D2}");
	    }
	    if (bool.Parse(doc["ironman"].ToString()))
	    {
		    details.Append($" ({lang.getString("STR_IRONMAN")})");
	    }
	    save.details = details.ToString();

	    return save;
    }

    /**
     * Toggles debug mode.
     */
    internal void setDebugMode() =>
	    _debug = !_debug;

	/**
	 * Loads a saved game's contents from a YAML file.
	 * @note Assumes the saved game is blank.
	 * @param filename YAML filename.
	 * @param mod Mod for the saved game.
	 */
	internal void load(string filename, Mod.Mod mod)
	{
		string s = Options.getMasterUserFolder() + filename;
        using var input = new StreamReader(s);
        var yaml = new YamlStream();
        yaml.Load(input);
		if (!yaml.Documents.Any())
		{
			throw new Exception(filename + " is not a valid save file");
		}

		// Get brief save info
        var brief = (YamlMappingNode)yaml.Documents[0].RootNode;
		_time.load(brief["time"]);
		if (brief["name"] != null)
		{
			_name = brief["name"].ToString();
		}
		else
		{
			_name = Unicode.convPathToUtf8(filename);
		}
		_ironman = bool.Parse(brief["ironman"].ToString());

		// Get full save data
        var doc = (YamlMappingNode)yaml.Documents[1].RootNode;
		_difficulty = (GameDifficulty)int.Parse(doc["difficulty"].ToString());
		_end = (GameEnding)int.Parse(doc["end"].ToString());
		if (doc["rng"] != null && (_ironman || !Options.newSeedOnLoad))
			RNG.setSeed(ulong.Parse(doc["rng"].ToString()));
		_monthsPassed = int.Parse(doc["monthsPassed"].ToString());
		_graphRegionToggles = doc["graphRegionToggles"].ToString();
		_graphCountryToggles = doc["graphCountryToggles"].ToString();
		_graphFinanceToggles = doc["graphFinanceToggles"].ToString();
		_funds = ((YamlSequenceNode)doc["funds"]).Children.Select(x => long.Parse(x.ToString())).ToList();
		_maintenance = ((YamlSequenceNode)doc["maintenance"]).Children.Select(x => long.Parse(x.ToString())).ToList();
		_researchScores = ((YamlSequenceNode)doc["researchScores"]).Children.Select(x => int.Parse(x.ToString())).ToList();
		_incomes = ((YamlSequenceNode)doc["incomes"]).Children.Select(x => long.Parse(x.ToString())).ToList();
		_expenditures = ((YamlSequenceNode)doc["expenditures"]).Children.Select(x => long.Parse(x.ToString())).ToList();
		_warned = bool.Parse(doc["warned"].ToString());
		_globeLon = double.Parse(doc["globeLon"].ToString());
		_globeLat = double.Parse(doc["globeLat"].ToString());
		_globeZoom = int.Parse(doc["globeZoom"].ToString());
		_ids = ((YamlMappingNode)doc["ids"]).Children.ToDictionary(x => x.Key.ToString(), x => int.Parse(x.Value.ToString()));

		foreach (var i in ((YamlSequenceNode)doc["countries"]).Children)
		{
			string type = i["type"].ToString();
			if (mod.getCountry(type) != null)
			{
				Country c = new Country(mod.getCountry(type), false);
				c.load(i);
				_countries.Add(c);
			}
			else
			{
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load country {type}");
			}
		}

		foreach (var i in ((YamlSequenceNode)doc["regions"]).Children)
		{
			string type = i["type"].ToString();
			if (mod.getRegion(type) != null)
			{
				Region r = new Region(mod.getRegion(type));
				r.load(i);
				_regions.Add(r);
			}
			else
			{
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load region {type}");
			}
		}

		// Alien bases must be loaded before alien missions
		foreach (var i in ((YamlSequenceNode)doc["alienBases"]).Children)
		{
			string deployment = i["deployment"] != null ? i["deployment"].ToString() : "STR_ALIEN_BASE_ASSAULT";
			if (mod.getDeployment(deployment) != null)
			{
				AlienBase b = new AlienBase(mod.getDeployment(deployment));
				b.load(i);
				_alienBases.Add(b);
			}
			else
			{
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load deployment for alien base {deployment}");
			}
		}

		// Missions must be loaded before UFOs.
		foreach (var it in ((YamlSequenceNode)doc["alienMissions"]).Children)
		{
			string missionType = it["type"].ToString();
			if (mod.getAlienMission(missionType) != null)
			{
				RuleAlienMission mRule = mod.getAlienMission(missionType);
				AlienMission mission = new AlienMission(mRule);
				mission.load(it, this);
				_activeMissions.Add(mission);
			}
			else
			{
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load mission {missionType}");
			}
		}

		foreach (var i in ((YamlSequenceNode)doc["ufos"]).Children)
		{
			string type = i["type"].ToString();
			if (mod.getUfo(type) != null)
			{
				Ufo u = new Ufo(mod.getUfo(type));
				u.load(i, mod, this);
				_ufos.Add(u);
			}
			else
			{
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load UFO {type}");
			}
		}

		foreach (var i in ((YamlSequenceNode)doc["waypoints"]).Children)
		{
			Waypoint w = new Waypoint();
			w.load(i);
			_waypoints.Add(w);
		}

		// Backwards compatibility
		foreach (var i in ((YamlSequenceNode)doc["terrorSites"]).Children)
		{
			string type = "STR_ALIEN_TERROR";
			string deployment = "STR_TERROR_MISSION";
			if (mod.getAlienMission(type) != null && mod.getDeployment(deployment) != null)
			{
				MissionSite m = new MissionSite(mod.getAlienMission(type), mod.getDeployment(deployment));
				m.load(i);
				_missionSites.Add(m);
			}
			else
			{
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load mission {type} deployment {deployment}");
			}
		}

		foreach (var i in ((YamlSequenceNode)doc["missionSites"]).Children)
		{
			string type = i["type"].ToString();
			string deployment = i["deployment"] != null ? i["deployment"].ToString() : "STR_TERROR_MISSION";
			if (mod.getAlienMission(type) != null && mod.getDeployment(deployment) != null)
			{
				MissionSite m = new MissionSite(mod.getAlienMission(type), mod.getDeployment(deployment));
				m.load(i);
				_missionSites.Add(m);
			}
			else
			{
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load mission {type} deployment {deployment}");
			}
		}

		// Discovered Techs Should be loaded before Bases (e.g. for PSI evaluation)
		foreach (var it in ((YamlSequenceNode)doc["discovered"]).Children)
		{
			string research = it.ToString();
			if (mod.getResearch(research) != null)
			{
				_discovered.Add(mod.getResearch(research));
			}
			else
			{
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load research {research}");
			}
		}

		foreach (var i in ((YamlSequenceNode)doc["bases"]).Children)
		{
			Base b = new Base(mod);
			b.load(i, this, false);
			_bases.Add(b);
		}

		foreach (var it in ((YamlSequenceNode)doc["poppedResearch"]).Children)
		{
			string id = it.ToString();
			if (mod.getResearch(id) != null)
			{
				_poppedResearch.Add(mod.getResearch(id));
			}
			else
			{
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load research {id}");
			}
		}
		_alienStrategy.load(doc["alienStrategy"]);

		foreach (var i in ((YamlSequenceNode)doc["deadSoldiers"]).Children)
		{
			string type = i["type"] != null ? i["type"].ToString() : mod.getSoldiersList().First();
			if (mod.getSoldier(type) != null)
			{
				Soldier soldier = new Soldier(mod.getSoldier(type), null);
				soldier.load(i, mod, this);
				_deadSoldiers.Add(soldier);
			}
			else
			{
				Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Failed to load soldier {type}");
			}
		}

		foreach (var i in ((YamlSequenceNode)doc["missionStatistics"]).Children)
		{
			MissionStatistics ms = new MissionStatistics();
			ms.load(i);
			_missionStatistics.Add(ms);
		}

		if (doc["battleGame"] is YamlNode battle)
		{
			_battleGame = new SavedBattleGame();
			_battleGame.load(battle, mod, this);
		}
	}

	/**
	 * Get the list of newly available research projects once a ResearchProject has been completed.
	 * @param before the list of available RuleResearch before completing new research.
	 * @param after the list of available RuleResearch after completing new research.
	 * @param diff the list of newly available RuleResearch after completing new research (after - before).
	 */
	internal void getNewlyAvailableResearchProjects(ref List<RuleResearch> before, ref List<RuleResearch> after, ref List<RuleResearch> diff)
	{
		// History lesson:
		// Completely rewritten the original recursive algorithm, because it was inefficient, unreadable and wrong
		// a/ inefficient: it could call SavedGame::getAvailableResearchProjects() way too many times
		// b/ unreadable: because of recursion
		// c/ wrong: could end in an endless loop! in two different ways! (not in vanilla, but in mods)

		// Note:
		// We could move the sorting of "before" vector right after its creation to optimize a little bit more.
		// But sorting a short list is negligible compared to other operations we had to do to get to this point.
		// So I decided to leave it here, so that it's 100% clear what's going on.
		var comparer = new CompareRuleResearch();
		before.Sort(comparer);
		after.Sort(comparer);
		diff = (List<RuleResearch>)after.Except(before, new EqualRuleResearch());
	}
}
