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

enum ChronoTrigger { FORCE_LOSE, FORCE_ABORT, FORCE_WIN };

enum EscapeType { ESCAPE_NONE, ESCAPE_EXIT, ESCAPE_ENTRY, ESCAPE_EITHER };

struct ItemSet
{
    internal List<string> items;

    /**
	 * Loads the ItemSet from a YAML file.
	 * @param node YAML node.
	 */
    internal void load(YamlNode node) =>
        items = ((YamlSequenceNode)node["items"]).Children.Select(x => x.ToString()).ToList();
};

struct DeploymentData
{
    internal int alienRank;
    internal int lowQty, highQty, dQty, extraQty;
    internal int percentageOutsideUfo;
    internal List<ItemSet> itemSets;

    /**
	 * Loads the DeploymentData from a YAML file.
	 * @param node YAML node.
	 */
    internal void load(YamlNode node)
    {
        alienRank = int.Parse(node["alienRank"].ToString());
        lowQty = int.Parse(node["lowQty"].ToString());
        highQty = int.Parse(node["highQty"].ToString());
        dQty = int.Parse(node["dQty"].ToString());
        extraQty = int.Parse(node["extraQty"].ToString());
        percentageOutsideUfo = int.Parse(node["percentageOutsideUfo"].ToString());
        itemSets = ((YamlSequenceNode)node["itemSets"]).Children.Select(x =>
        {
            var set = new ItemSet(); set.load(x); return set;
        }).ToList();
    }
};

struct BriefingData
{
    int palette, textOffset;
    string title, desc, music, background, cutscene;
    bool showCraft, showTarget;
    
    public BriefingData()
    {
        palette = 0;
        textOffset = 0;
        music = "GMDEFEND";
        background = "BACK16.SCR";
        showCraft = true;
        showTarget = true;
    }

    /**
	 * Loads the BriefingData from a YAML file.
	 * @param node YAML node.
	 */
    internal void load(YamlNode node)
    {
        palette = int.Parse(node["palette"].ToString());
        textOffset = int.Parse(node["textOffset"].ToString());
        title = node["title"].ToString();
        desc = node["desc"].ToString();
        music = node["music"].ToString();
        background = node["background"].ToString();
        cutscene = node["cutscene"].ToString();
        showCraft = bool.Parse(node["showCraft"].ToString());
        showTarget = bool.Parse(node["showTarget"].ToString());
    }
};

/**
 * Represents a specific type of Alien Deployment.
 * Contains constant info about a Alien Deployment like
 * the number of aliens for each alien type and what items they carry
 * (itemset depends on alien technology advancement level 0, 1 or 2).
 * - deployment type can be a craft's name, but also alien base or cydonia.
 * - alienRank is used to check which nodeRanks can be used to deploy this unit
 *   + to match to a specific unit (=race/rank combination) that should be deployed.
 * @sa Node
 */
internal class AlienDeployment : IRule
{
    string _type;
    int _width, _length, _height, _civilians;
    int _shade;
    bool _finalDestination, _isAlienBase;
    string _alert, _alertBackground;
    string _markerName, _objectivePopup, _objectiveCompleteText, _objectiveFailedText;
    int _markerIcon, _durationMin, _durationMax, _minDepth, _maxDepth, _genMissionFrequency;
    int _objectiveType, _objectivesRequired, _objectiveCompleteScore, _objectiveFailedScore, _despawnPenalty, _points, _turnLimit, _cheatTurn;
    ChronoTrigger _chronoTrigger;
    EscapeType _escapeType;
    List<DeploymentData> _data;
    List<string> _terrains, _music;
    string _nextStage, _race, _script;
    string _winCutscene, _loseCutscene, _abortCutscene;
    BriefingData _briefingData;
    WeightedOptions _genMission;

    /**
     * Creates a blank ruleset for a certain
     * type of deployment data.
     * @param type String defining the type.
     */
    AlienDeployment(string type)
    {
        _type = type;
        _width = 0;
        _length = 0;
        _height = 0;
        _civilians = 0;
        _shade = -1;
        _finalDestination = false;
        _isAlienBase = false;
        _alert = "STR_ALIENS_TERRORISE";
        _alertBackground = "BACK03.SCR";
        _markerName = "STR_TERROR_SITE";
        _markerIcon = -1;
        _durationMin = 0;
        _durationMax = 0;
        _minDepth = 0;
        _maxDepth = 0;
        _genMissionFrequency = 0;
        _objectiveType = -1;
        _objectivesRequired = 0;
        _objectiveCompleteScore = 0;
        _objectiveFailedScore = 0;
        _despawnPenalty = 0;
        _points = 0;
        _turnLimit = 0;
        _cheatTurn = 20;
        _chronoTrigger = ChronoTrigger.FORCE_LOSE;
        _escapeType = EscapeType.ESCAPE_NONE;
    }

    public IRule Create(string type) =>
        new AlienDeployment(type);

    /**
     *
     */
    ~AlienDeployment() { }

    /**
     * Returns the language string that names
     * this deployment. Each deployment type has a unique name.
     * @return Deployment name.
     */
    internal string getType() =>
	    _type;

    /**
     * Returns the globe marker icon for this mission.
     * @return Marker sprite, -1 if none.
     */
    internal int getMarkerIcon() =>
	    _markerIcon;

    /**
     * Loads the Deployment from a YAML file.
     * @param node YAML node.
     * @param mod Mod for the deployment.
     */
    internal void load(YamlNode node, Mod mod)
    {
	    _type = node["type"].ToString();
        _data = ((YamlSequenceNode)node["data"]).Children.Select(x =>
        {
            var data = new DeploymentData(); data.load(x); return data;
        }).ToList();
	    _width = int.Parse(node["width"].ToString());
	    _length = int.Parse(node["length"].ToString());
	    _height = int.Parse(node["height"].ToString());
	    _civilians = int.Parse(node["civilians"].ToString());
        _terrains = ((YamlSequenceNode)node["terrains"]).Children.Select(x => x.ToString()).ToList();
	    _shade = int.Parse(node["shade"].ToString());
	    _nextStage = node["nextStage"].ToString();
	    _race = node["race"].ToString();
	    _finalDestination = bool.Parse(node["finalDestination"].ToString());
	    _winCutscene = node["winCutscene"].ToString();
	    _loseCutscene = node["loseCutscene"].ToString();
	    _abortCutscene = node["abortCutscene"].ToString();
	    _script = node["script"].ToString();
	    _alert = node["alert"].ToString();
	    _alertBackground = node["alertBackground"].ToString();
        _briefingData.load(node["briefing"]);
	    _markerName = node["markerName"].ToString();
	    if (node["markerIcon"] != null)
	    {
		    _markerIcon = mod.getOffset(int.Parse(node["markerIcon"].ToString()), 8);
	    }
	    if (node["depth"] != null)
	    {
		    _minDepth = int.Parse(node["depth"][0].ToString());
		    _maxDepth = int.Parse(node["depth"][1].ToString());
	    }
	    if (node["duration"] != null)
	    {
		    _durationMin = int.Parse(node["duration"][0].ToString());
		    _durationMax = int.Parse(node["duration"][1].ToString());
	    }
        _music = ((YamlSequenceNode)node["music"]).Children.Select(x => x.ToString()).ToList();
	    _objectiveType = int.Parse(node["objectiveType"].ToString());
	    _objectivesRequired = int.Parse(node["objectivesRequired"].ToString());
	    _objectivePopup = node["objectivePopup"].ToString();

	    if (node["objectiveComplete"] != null)
	    {
		    _objectiveCompleteText = node["objectiveComplete"][0].ToString();
		    _objectiveCompleteScore = int.Parse(node["objectiveComplete"][1].ToString());
	    }
	    if (node["objectiveFailed"] != null)
	    {
		    _objectiveFailedText = node["objectiveFailed"][0].ToString();
		    _objectiveFailedScore = int.Parse(node["objectiveFailed"][1].ToString());
	    }
	    _despawnPenalty = int.Parse(node["despawnPenalty"].ToString());
	    _points = int.Parse(node["points"].ToString());
	    _cheatTurn = int.Parse(node["cheatTurn"].ToString());
	    _turnLimit = int.Parse(node["turnLimit"].ToString());
	    _chronoTrigger = (ChronoTrigger)int.Parse(node["chronoTrigger"].ToString());
	    _isAlienBase = bool.Parse(node["alienBase"].ToString());
	    _escapeType = (EscapeType)int.Parse(node["escapeType"].ToString());
	    if (node["genMission"] != null)
	    {
		    _genMission.load(node["genMission"]);
	    }
	    _genMissionFrequency = int.Parse(node["genMissionFreq"].ToString());
    }

    /**
     * Returns the globe marker name for this mission.
     * @return String ID for marker name.
     */
    internal string getMarkerName() =>
	    _markerName;

    /**
     * Gets the score penalty against XCom for this site existing.
     * This penalty is applied half-hourly for sites and daily for bases.
     * @return the number of points the aliens get per half hour.
     */
    internal int getPoints() =>
	    _points;

    /**
     * Gets the score penalty XCom receives for letting this mission despawn.
     * @return the score for letting this site despawn.
     */
    internal int getDespawnPenalty() =>
	    _despawnPenalty;

    /**
     * Returns the minimum duration for this mission type.
     * @return Duration in hours.
     */
    internal int getDurationMin() =>
	    _durationMin;

    /**
     * Returns the maximum duration for this mission type.
     * @return Duration in hours.
     */
    internal int getDurationMax() =>
	    _durationMax;

    internal string chooseGenMissionType() =>
	    _genMission.choose();

    internal int getGenMissionFrequency() =>
	    _genMissionFrequency;

    /**
     * Gets the maximum number of turns we have before this mission ends.
     * @return the turn limit.
     */
    internal int getTurnLimit() =>
	    _turnLimit;

    /**
     * Gets the action type to perform when the timer expires.
     * @return the action type to perform.
     */
    internal ChronoTrigger getChronoTrigger() =>
	    _chronoTrigger;

    /**
     * Gets the turn at which the players become exposed to the AI.
     * @return the turn to start cheating.
     */
    internal int getCheatTurn() =>
	    _cheatTurn;

    /**
     * Gets dimensions.
     * @param width Width.
     * @param length Length.
     * @param height Height.
     */
    internal void getDimensions(out int width, out int length, out int height)
    {
	    width = _width;
	    length = _length;
	    height = _height;
    }

    /**
     * Gets the terrain for battlescape generation.
     * @return The terrain.
     */
    internal List<string> getTerrains() =>
	    _terrains;

    /**
     * Gets the shade level for battlescape generation.
     * @return The shade level.
     */
    internal int getShade() =>
	    _shade;

    /**
     * Gets the script to use to generate a mission of this type.
     * @return The script to use to generate a mission of this type.
     */
    internal string getScript() =>
	    _script;

    /**
     * Gets the number of civilians.
     * @return The number of civilians.
     */
    internal int getCivilians() =>
	    _civilians;

    /**
     * Gets The maximum depth for this deployment.
     * @return The maximum depth.
     */
    internal int getMaxDepth() =>
	    _maxDepth;

    /**
     * Gets The minimum depth for this deployment.
     * @return The minimum depth.
     */
    internal int getMinDepth() =>
	    _minDepth;

    /**
     * Gets The list of musics this deployment has to choose from.
     * @return The list of track names.
     */
    internal List<string> getMusic() =>
	    _music;

    /**
     * Gets the target type for this mission (ie: alien control consoles and synomium devices).
     * @return the target type for this mission.
     */
    internal int getObjectiveType() =>
	    _objectiveType;

    /**
     * Gets the number of objectives required by this mission.
     * @return the number of objectives required.
     */
    internal int getObjectivesRequired() =>
	    _objectivesRequired;

    /**
     * Gets the string name for the popup to splash when the objective conditions are met.
     * @return the string to pop up.
     */
    internal string getObjectivePopup() =>
	    _objectivePopup;

    /**
     * Gets the next stage of the mission.
     * @return The next stage of the mission.
     */
    internal string getNextStage() =>
	    _nextStage;

    /**
    * Gets the cutscene to play when the mission is aborted.
    * @return the cutscene to play when the mission is aborted.
    */
    internal string getAbortCutscene() =>
	    _abortCutscene;

    /**
     * Gets the cutscene to play when the mission is lost.
     * @return the cutscene to play when the mission is lost.
     */
    internal string getLoseCutscene() =>
	    _loseCutscene;

    /**
     * Gets the cutscene to play when the mission is won.
     * @return the cutscene to play when the mission is won.
     */
    internal string getWinCutscene() =>
	    _winCutscene;

    /**
     * Gets the race to use on the next stage of the mission.
     * @return The race for the next stage of the mission.
     */
    internal string getRace() =>
	    _race;

    /**
     * Gets a pointer to the data.
     * @return Pointer to the data.
     */
    internal List<DeploymentData> getDeploymentData() =>
        _data;
}
