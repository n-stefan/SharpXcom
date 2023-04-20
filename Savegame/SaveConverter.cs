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

enum TargetType { TARGET_NONE, TARGET_UFO, TARGET_CRAFT, TARGET_XBASE, TARGET_ABASE, TARGET_CRASH, TARGET_LANDED, TARGET_WAYPOINT, TARGET_TERROR, TARGET_PORT = 0x51, TARGET_ISLAND = 0x52, TARGET_SHIP = 0x53, TARGET_ARTEFACT = 0x54 };

/**
 * Container for savegame info displayed on listings.
 */
struct SaveOriginal
{
    internal int id;
    internal string name, date, time;
    internal bool tactical;

    public SaveOriginal() =>
        tactical = false;
}

/**
 * Handles conversion operations for original X-COM savegames.
 * @sa http://ufopaedia.org/index.php?title=Saved_Game_Files
 */
internal class SaveConverter
{
    internal const int NUM_SAVES = 10;
    string[] xcomStatus = { "STR_READY", "STR_OUT", "STR_REPAIRS", "STR_REFUELLING", "STR_REARMING" };
    string[] xcomAltitudes = { "STR_GROUND", "STR_VERY_LOW", "STR_LOW_UC", "STR_HIGH_UC", "STR_VERY_HIGH" };

    SavedGame _save;
    Mod.Mod _mod;
    RuleConverter _rules;
    int _year, _funds;
    string _saveName, _savePath;
    Dictionary<KeyValuePair<int, int>, AlienMission> _missions;
    List<Target> _targets;
    List<int> _targetDat;
    List<string> _aliens;
    List<Soldier> _soldiers;

    /**
     * Creates a new converter for the given save folder.
     * @param save Number of the save folder GAME_#
     * @param mod Mod to associate with this save.
     */
    internal SaveConverter(int save, Mod.Mod mod)
    {
        _save = null;
        _mod = mod;
        _rules = mod.getConverter();
        _year = 0;
        _funds = 0;

        string ssFolder = $"GAME_{save}";
        string ssPath = $"{Options.getMasterUserFolder()}{ssFolder}";
        _saveName = ssFolder;
        _savePath = ssPath;
        ssPath = $"{ssPath}/SAVEINFO.DAT";
        if (!CrossPlatform.fileExists(ssPath))
        {
            throw new Exception($"{_saveName} is not a valid save folder");
        }
    }

    ~SaveConverter() { }

    /**
     * Gets all the info of the saves found in the user folder.
     * @param lang Loaded language.
     * @param info Returned list of saves info.
     */
    internal static void getList(Language lang, SaveOriginal[] info)
    {
	    try
        {
            for (int i = 0; i < NUM_SAVES; ++i)
	        {
		        SaveOriginal save;
		        save.id = 0;

		        int id = i + 1;
		        string ss = $"{Options.getMasterUserFolder()}GAME_{id}{Path.PathSeparator}SAVEINFO.DAT";
                using var datFile = new FileStream(ss, FileMode.Open);
                var buffer = new byte[datFile.Length];
                datFile.Read(buffer, 0, buffer.Length);

                string name = BitConverter.ToString(buffer, 0x02);
                int year = BitConverter.ToUInt16(buffer, 0x1C);
                int month = BitConverter.ToUInt16(buffer, 0x1E);
                int day = BitConverter.ToUInt16(buffer, 0x20);
                int hour = BitConverter.ToUInt16(buffer, 0x22);
                int minute = BitConverter.ToUInt16(buffer, 0x24);
                bool tactical = buffer[0x26] != 0;
                
                GameTime time = new GameTime(0, day, month + 1, year, hour, minute, 0);
                
                string ssDate = $"{time.getDayString(lang)}  {lang.getString(time.getMonthString())}  {time.getYear()}";
                string ssTime = $"{time.getHour()}:{time.getMinute():D2}";
                
                save.id = id;
                save.name = name;
                save.date = ssDate;
                save.time = ssTime;
                save.tactical = tactical;
                
                datFile.Close();
		        info[i] = save;
	        }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /**
     * Converts an original X-COM save into an OpenXcom save.
     * @return New OpenXcom save.
     */
    internal SavedGame loadOriginal()
    {
        _save = new SavedGame();

        // Load globe data
        _save.getIncomes().Clear();
        for (int i = 0; i < _rules.getCountries().Count; ++i)
        {
            Country country = new Country(_mod.getCountry(_rules.getCountries()[i], true));
            country.getActivityAlien().Clear();
            country.getActivityXcom().Clear();
            country.getFunding().Clear();
            _save.getCountries().Add(country);
        }
        for (int i = 0; i < _rules.getRegions().Count; ++i)
        {
            Region region = new Region(_mod.getRegion(_rules.getRegions()[i], true));
            region.getActivityAlien().Clear();
            region.getActivityXcom().Clear();
            _save.getRegions().Add(region);
        }
        loadDatXcom();
        loadDatAlien();
        loadDatDiplom();
        loadDatLease();

        // Load graph data
        _save.getExpenditures().Clear();
        _save.getMaintenances().Clear();
        _save.getFundsList().Clear();
        _save.getResearchScores().Clear();
        loadDatLIGlob();
        loadDatUIGlob();
        loadDatIGlob();

        // Load alien data
        loadDatZonal();
        loadDatActs();
        loadDatMissions();

        // Load player data
        loadDatLoc();
        loadDatBase();
        loadDatAStore();
        loadDatCraft();
        loadDatSoldier();
        loadDatTransfer();
        loadDatResearch();
        loadDatUp();
        loadDatProject();
        loadDatBProd();
        loadDatXBases();

        return _save;
    }

    byte[] binaryBuffer(string filename)
    {
        byte[] buffer;
        string s = _savePath + Path.PathSeparator + filename;
        try
        {
            using var datFile = new FileStream(s, FileMode.Open);
            buffer = new byte[datFile.Length];
            datFile.Read(buffer, 0, buffer.Length);
            datFile.Close();
        }
        catch (Exception)
        {
		    throw new Exception(filename + " not found");
        }
	    return buffer;
    }

    /**
     * Loads the XCOM.DAT file.
     * Contains X-COM graph info.
     */
    void loadDatXcom()
    {
        byte[] data = binaryBuffer("XCOM.DAT");

        int ENTRIES = _rules.getCountries().Count + _rules.getRegions().Count;
        const int MONTHS = 12;
        for (int i = 0; i < ENTRIES * MONTHS; ++i)
        {
            int score = BitConverter.ToInt32(data, i);
            int j = i % ENTRIES;
            // country
            if (j < _rules.getCountries().Count)
            {
                _save.getCountries()[j].getActivityXcom().Add(score);
            }
            // region
            else
            {
                j -= _rules.getCountries().Count;
                _save.getRegions()[j].getActivityXcom().Add(score);
            }
        }
    }

    /**
     * Loads the ALIEN.DAT file.
     * Contains Alien graph info.
     */
    void loadDatAlien()
    {
        byte[] data = binaryBuffer("ALIEN.DAT");

        int ENTRIES = _rules.getCountries().Count + _rules.getRegions().Count;
        const int MONTHS = 12;
        for (int i = 0; i < ENTRIES * MONTHS; ++i)
        {
            int score = BitConverter.ToInt32(data, i);
            int j = i % ENTRIES;
            // country
            if (j < _rules.getCountries().Count)
            {
                _save.getCountries()[j].getActivityAlien().Add(score);
            }
            // region
            else
            {
                j -= _rules.getCountries().Count;
                _save.getRegions()[j].getActivityAlien().Add(score);
            }
        }
    }

    /**
     * Loads the DIPLOM.DAT file.
     * Contains country status.
     */
    void loadDatDiplom()
    {
        byte[] data = binaryBuffer("DIPLOM.DAT");

        const int MONTHS = 12;
        var income = new List<long>();
        for (int i = 0; i < MONTHS; ++i)
        {
            income.Add(0);
        }

        const int ENTRY_SIZE = 36;
        for (int i = 0; i < _rules.getCountries().Count; ++i)
        {
            int cdata = i * ENTRY_SIZE;
            Country country = _save.getCountries()[i];

            int satisfaction = BitConverter.ToInt16(data, cdata + 0x02);
            for (int j = 0; j < MONTHS; ++j)
            {
                int funding = BitConverter.ToInt16(data, cdata + 0x04 + j * sizeof(short));
                funding *= 1000;
                income[j] += funding;
                country.getFunding().Add(funding);
            }
            bool pact = satisfaction == 0;
            bool newPact = BitConverter.ToInt16(data, cdata + 0x1E) != 0;

            if (pact)
                country.setPact();
            if (newPact)
                country.setNewPact();
        }
        _save.getIncomes() = income;
    }

    /**
     * Loads the LEASE.DAT file.
     * Contains globe camera settings.
     */
    void loadDatLease()
    {
        byte[] data = binaryBuffer("LEASE.DAT");

        double lat = -Xcom2Rad(BitConverter.ToInt16(data, 0x00));
        double lon = -Xcom2Rad(BitConverter.ToInt16(data, 0x06));
        _save.setGlobeLongitude(lon);
        _save.setGlobeLatitude(lat);

        int zoom = BitConverter.ToInt16(data, 0x0C);
        int[] DISTANCE = { 90, 120, 180, 360, 450, 720 };
        for (int i = 0; i < 6; ++i)
        {
            if (zoom == DISTANCE[i])
            {
                _save.setGlobeZoom(i);
                break;
            }
        }
    }

    /**
     * Loads the LIGLOB.DAT file.
     * Contains financial data.
     */
    void loadDatLIGlob()
    {
        byte[] data = binaryBuffer("LIGLOB.DAT");

        const int MONTHS = 12;
        for (int i = 0; i < MONTHS; ++i)
        {
            int expenditure = BitConverter.ToInt32(data, 0x04 + i * sizeof(int));
            int maintenance = BitConverter.ToInt32(data, 0x34 + i * sizeof(int));
            int balance = BitConverter.ToInt32(data, 0x64 + i * sizeof(int));
            _save.getExpenditures().Add(expenditure);
            _save.getMaintenances().Add(maintenance);
            _save.getFundsList().Add(balance);
        }

        _funds = BitConverter.ToInt32(data);
    }

    /**
     * Loads the UIGLOB.DAT file.
     * Contains Geoscape number IDs and scores.
     */
    void loadDatUIGlob()
    {
        byte[] data = binaryBuffer("UIGLOB.DAT");

        var ids = new Dictionary<string, int>();
        for (int i = 0; i < _rules.getMarkers().Count; ++i)
        {
            ids[_rules.getMarkers()[i]] = BitConverter.ToUInt16(data, i * sizeof(ushort));
        }
        ids["STR_CRASH_SITE"] = ids["STR_LANDING_SITE"] = ids["STR_UFO"];

        _year = BitConverter.ToUInt16(data, 0x16);

        const int MONTHS = 12;
        for (int i = 0; i < MONTHS; ++i)
        {
            int score = BitConverter.ToInt16(data, 0x18 + i * sizeof(short));
            _save.getResearchScores().Add(score);
        }

        // Loads the SITE.DAT file (TFTD only).
        string s = _savePath + Path.PathSeparator + "SITE.DAT";
        if (CrossPlatform.fileExists(s))
        {
            byte[] sitedata = binaryBuffer("SITE.DAT");
            int generatedArtifactSiteMissions = BitConverter.ToUInt16(sitedata, 0x24);
            if (generatedArtifactSiteMissions > 0)
            {
                _save.getAlienStrategy().addMissionRun("artifacts", generatedArtifactSiteMissions);

                int spawnedArtifactSites = generatedArtifactSiteMissions;
                char siteTypeToBeSpawned = (char)sitedata[0x26];
                if (siteTypeToBeSpawned == 'T')
                {
                    // before the first hour of the month, the mission was generated already, but the site has not spawned yet
                    spawnedArtifactSites--;
                }
                else
                {
                    // after the first hour of the month
                    // or not an artifact site type
                }
                ids["STR_ARTIFACT_SITE"] = spawnedArtifactSites + 1; // OXC stores the ID of the next site, thus +1
            }
        }

        _save.setAllIds(ids);
    }

    /**
     * Loads the IGLOB.DAT file.
     * Contains game date, time and difficulty.
     */
    void loadDatIGlob()
    {
        byte[] data = binaryBuffer("IGLOB.DAT");

        int month = BitConverter.ToInt32(data, 0x00) + 1;
        int weekday = BitConverter.ToInt32(data, 0x04) + 1;
        int day = BitConverter.ToInt32(data, 0x08);
        int hour = BitConverter.ToInt32(data, 0x0C);
        int minute = BitConverter.ToInt32(data, 0x10);
        int second = BitConverter.ToInt32(data, 0x14);
        _save.setTime(new GameTime(weekday, day, month, _year, hour, minute, second));

        // account for difficulty bug
        if (data.Length > 0x3C)
        {
            int coefficient = BitConverter.ToInt32(data, 0x3C);
            for (int i = (int)GameDifficulty.DIFF_BEGINNER; i <= (int)GameDifficulty.DIFF_SUPERHUMAN; ++i)
            {
                if (coefficient == Mod.Mod.DIFFICULTY_COEFFICIENT[i])
                {
                    _save.setDifficulty((GameDifficulty)i);
                    break;
                }
            }
        }

        // Fix up the months
        int monthsPassed = month + (_year - _mod.getStartingTime().getYear()) * 12;
        for (int i = 0; i < monthsPassed; ++i)
        {
            _save.addMonth();
        }
        graphVector(ref _save.getIncomes(), month, _year != _mod.getStartingTime().getYear());
        graphVector(ref _save.getExpenditures(), month, _year != _mod.getStartingTime().getYear());
        graphVector(ref _save.getMaintenances(), month, _year != _mod.getStartingTime().getYear());
        graphVector(ref _save.getFundsList(), month, _year != _mod.getStartingTime().getYear());
        graphVector(ref _save.getResearchScores(), month, _year != _mod.getStartingTime().getYear());
        for (int i = 0; i < _rules.getCountries().Count; ++i)
        {
            Country country = _save.getCountries()[i];
            graphVector(ref country.getActivityAlien(), month, _year != _mod.getStartingTime().getYear());
            graphVector(ref country.getActivityXcom(), month, _year != _mod.getStartingTime().getYear());
            graphVector(ref country.getFunding(), month, _year != _mod.getStartingTime().getYear());
        }
        for (int i = 0; i < _rules.getRegions().Count; ++i)
        {
            Region region = _save.getRegions()[i];
            graphVector(ref region.getActivityAlien(), month, _year != _mod.getStartingTime().getYear());
            graphVector(ref region.getActivityXcom(), month, _year != _mod.getStartingTime().getYear());
        }
        _save.getFundsList()[^1] = _funds;
    }

    /**
     * Corrects vectors of graph data.
     * Original X-COM uses months as array indexes,
     * while OpenXcom stores month data in a linear fashion.
     * @param vector Vector of graph data.
     * @param month Current month.
     * @param year Has game gone longer than a year?
     */
    void graphVector<T>(ref List<T> vector, int month, bool year)
    {
	    if (year)
	    {
		    var newVector = new List<T>();
		    int i = month;
		    do
		    {
			    newVector.Add(vector[i]);
			    i = (i + 1) % vector.Count;
		    }
		    while (i != month);
		    vector = newVector;
	    }
	    else
	    {
		    //vector.resize(month);
	    }
    }

    /**
     * Loads the ZONAL.DAT file.
     * Contains alien region chances.
     */
    void loadDatZonal()
    {
        byte[] data = binaryBuffer("ZONAL.DAT");

        var chances = new Dictionary<string, int>();
        const int REGIONS = 12;
        for (int i = 0; i < REGIONS; ++i)
        {
            chances[_rules.getRegions()[i]] = data[i];
        }
        var node = new YamlMappingNode
        {
            { "regions", new YamlSequenceNode(chances.Select(x => new YamlMappingNode(x.Key, x.Value.ToString()))) }
        };
        _save.getAlienStrategy().load(node);
    }

    /**
     * Loads the ACTS.DAT file.
     * Contains alien mission chances.
     */
    void loadDatActs()
    {
        byte[] data = binaryBuffer("ACTS.DAT");

        var chances = new Dictionary<string, Dictionary<string, int>>();
        const int REGIONS = 12;
        const int MISSIONS = 7;
        for (int i = 0; i < REGIONS * MISSIONS; ++i)
        {
            int mission = i % MISSIONS;
            int region = i / MISSIONS;

            chances[_rules.getRegions()[region]][_rules.getMissions()[mission]] = data[i];
        }

        var node = new YamlSequenceNode();
        foreach (var chance in chances)
        {
            var subnode = new YamlMappingNode
            {
                { "region", chance.Key },
                { "missions", new YamlSequenceNode(chance.Value.Select(x => new YamlMappingNode(x.Key, x.Value.ToString()))) }
            };
            ((YamlSequenceNode)node["possibleMissions"]).Add(subnode);
        }
        _save.getAlienStrategy().load(node);
    }

    /**
     * Loads the MISSIONS.DAT file.
     * Contains ongoing alien missions.
     */
    void loadDatMissions()
    {
        byte[] data = binaryBuffer("MISSIONS.DAT");

        const int REGIONS = 12;
        const int MISSIONS = 7;
        const int ENTRY_SIZE = 8;
        for (int i = 0; i < REGIONS * MISSIONS; ++i)
        {
            int mdata = i * ENTRY_SIZE;
            int wave = BitConverter.ToUInt16(data, mdata + 0x00);
            if (wave != 0xFFFF)
            {
                int ufoCounter = BitConverter.ToUInt16(data, mdata + 0x02);
                int spawn = BitConverter.ToUInt16(data, mdata + 0x04);
                int race = BitConverter.ToUInt16(data, mdata + 0x06);
                int mission = i % MISSIONS;
                int region = i / MISSIONS;

                AlienMission m = new AlienMission(_mod.getAlienMission(_rules.getMissions()[mission], true));
                var node = new YamlMappingNode
                {
                    { "region", _rules.getRegions()[region] },
                    { "race", _rules.getCrews()[race] },
                    { "nextWave", wave.ToString() },
                    { "nextUfoCounter", ufoCounter.ToString() },
                    { "spawnCountdown", (spawn * 30).ToString() },
                    { "uniqueID", _save.getId("ALIEN_MISSIONS").ToString() }
                };
                if (m.getRules().getObjective() == MissionObjective.OBJECTIVE_SITE)
                {
                    int missionZone = 3; // pick a city for terror missions
                    RuleRegion rule = _mod.getRegion(_rules.getRegions()[region], true);
                    if (rule.getMissionZones().Count <= 3)
                    {
                        // try to account for TFTD's artefacts and such
                        missionZone = 0;
                    }
                    node.Add("missionSiteZone", RNG.generate(0, rule.getMissionZones()[missionZone].areas.Count - 1).ToString());
                }
                m.load(node, _save);
                _save.getAlienMissions().Add(m);
                _missions[KeyValuePair.Create(mission, region)] = m;
            }
        }
    }

    /**
     * Loads the LOC.DAT file.
     * Contains globe markers.
     */
    void loadDatLoc()
    {
        byte[] data = binaryBuffer("LOC.DAT");

        const int ENTRIES = 50;
        int ENTRY_SIZE = data.Length / ENTRIES;
        for (int i = 0; i < ENTRIES; ++i)
        {
            int tdata = i * ENTRY_SIZE;
            TargetType type = (TargetType)data[tdata];

            int dat = data[tdata + 0x01];
            double lon = Xcom2Rad(BitConverter.ToInt16(data, tdata + 0x02));
            double lat = Xcom2Rad(BitConverter.ToInt16(data, tdata + 0x04));
            int timer = BitConverter.ToInt16(data, tdata + 0x06);
            int id = BitConverter.ToInt16(data, tdata + 0x0A);
            //TODO: Check
            BitVector32 visibility = new BitVector32(BitConverter.ToInt32(data, tdata + 0x10));
            bool detected = !visibility[BitVector32.CreateMask()];

            // can't declare variables in switches :(
            Target target = null;
            Ufo ufo = null;
            Craft craft = null;
            Base xbase = null;
            AlienBase abase = null;
            Waypoint waypoint = null;
            MissionSite mission = null;
            switch (type)
            {
                case TargetType.TARGET_NONE:
                    target = null;
                    break;
                case TargetType.TARGET_UFO:
                case TargetType.TARGET_CRASH:
                case TargetType.TARGET_LANDED:
                    ufo = new Ufo(_mod.getUfo(_rules.getUfos()[0], true));
                    ufo.setId(id);
                    ufo.setCrashId(id);
                    ufo.setLandId(id);
                    ufo.setSecondsRemaining((uint)timer);
                    ufo.setDetected(detected);
                    target = ufo;
                    break;
                case TargetType.TARGET_CRAFT:
                    craft = new Craft(_mod.getCraft(_rules.getCrafts()[0], true), null, id);
                    target = craft;
                    break;
                case TargetType.TARGET_XBASE:
                    xbase = new Base(_mod);
                    target = xbase;
                    break;
                case TargetType.TARGET_ABASE:
                    abase = new AlienBase(_mod.getDeployment("STR_ALIEN_BASE_ASSAULT", true));
                    abase.setId(id);
                    abase.setAlienRace(_rules.getCrews()[dat]);
                    abase.setDiscovered(detected);
                    _save.getAlienBases().Add(abase);
                    target = abase;
                    break;
                case TargetType.TARGET_WAYPOINT:
                    waypoint = new Waypoint();
                    waypoint.setId(id);
                    _save.getWaypoints().Add(waypoint);
                    target = waypoint;
                    break;
                case TargetType.TARGET_TERROR:
                    mission = new MissionSite(_mod.getAlienMission("STR_ALIEN_TERROR", true), _mod.getDeployment("STR_TERROR_MISSION", true));
                    break;
                case TargetType.TARGET_PORT:
                    mission = new MissionSite(_mod.getAlienMission("STR_ALIEN_SURFACE_ATTACK", true), _mod.getDeployment("STR_PORT_TERROR", true));
                    break;
                case TargetType.TARGET_ISLAND:
                    mission = new MissionSite(_mod.getAlienMission("STR_ALIEN_SURFACE_ATTACK", true), _mod.getDeployment("STR_ISLAND_TERROR", true));
                    break;
                case TargetType.TARGET_SHIP:
                    mission = new MissionSite(_mod.getAlienMission("STR_ALIEN_SHIP_ATTACK", true), _mod.getDeployment("STR_CARGO_SHIP_P1", true));
                    break;
                case TargetType.TARGET_ARTEFACT:
                    mission = new MissionSite(_mod.getAlienMission("STR_ALIEN_ARTIFACT", true), _mod.getDeployment("STR_ARTIFACT_SITE_P1", true));
                    break;
            }
            if (mission != null)
            {
                mission.setId(id);
                mission.setAlienRace(_rules.getCrews()[dat]);
                mission.setSecondsRemaining((uint)(timer * 3600));
                mission.setDetected(detected);
                _save.getMissionSites().Add(mission);
                target = mission;
            }
            if (target != null)
            {
                target.setLongitude(lon);
                target.setLatitude(lat);
            }
            _targets.Add(target);
            _targetDat.Add(dat);
        }
    }

    /**
     * Loads the BASE.DAT file.
     * Contains X-COM base contents.
     */
    void loadDatBase()
    {
        byte[] data = binaryBuffer("BASE.DAT");

        const int BASES = 8;
        const int BASE_SIZE = 6;
        const int FACILITIES = BASE_SIZE * BASE_SIZE;
        int ENTRY_SIZE = data.Length / BASES;
        var bases = new List<Base>(BASES);
        for (int i = 0; i < _targets.Count; ++i)
        {
            Base @base = (Base)_targets[i];
            if (@base != null)
            {
                int j = _targetDat[i];
                int bdata = j * ENTRY_SIZE;
                string name = BitConverter.ToString(data, bdata);
                // facilities
                for (int k = 0; k < FACILITIES; ++k)
                {
                    byte facilityType = data[bdata + _rules.getOffset("BASE.DAT_FACILITIES") + k];
                    if (facilityType < _rules.getFacilities().Count)
                    {
                        BaseFacility facility = new BaseFacility(_mod.getBaseFacility(_rules.getFacilities()[facilityType], true), @base);
                        int x = k % BASE_SIZE;
                        int y = k / BASE_SIZE;
                        int days = data[bdata + _rules.getOffset("BASE.DAT_FACILITIES") + FACILITIES + k];
                        facility.setX(x);
                        facility.setY(y);
                        facility.setBuildTime(days);
                        @base.getFacilities().Add(facility);
                    }
                }
                int engineers = data[bdata + _rules.getOffset("BASE.DAT_ENGINEERS")];
                int scientists = data[bdata + _rules.getOffset("BASE.DAT_SCIENTISTS")];
                // items
                for (int k = 0; k < _rules.getItems().Count; ++k)
                {
                    int qty = BitConverter.ToUInt16(data, bdata + _rules.getOffset("BASE.DAT_ITEMS") + k * 2);
                    if (qty != 0 && _rules.getItems()[k].Any())
                    {
                        @base.getStorageItems().addItem(_rules.getItems()[k], qty);
                    }
                }
                @base.setEngineers(engineers);
                @base.setScientists(scientists);
                @base.setName(name);
                bases[j] = @base;
            }
        }

        foreach (var @base in bases)
        {
            if (@base != null)
            {
                _save.getBases().Add(@base);
            }
        }
    }

    /**
     * Loads the ASTORE.DAT file.
     * Contains X-COM alien containment.
     */
    void loadDatAStore()
    {
        byte[] data = binaryBuffer("ASTORE.DAT");

        const int ENTRY_SIZE = 12;
        int ENTRIES = data.Length / ENTRY_SIZE;
        for (int i = 0; i < ENTRIES; ++i)
        {
            int adata = i * ENTRY_SIZE;
            int race = data[adata + 0x00];
            string liveAlien = null;
            if (race != 0)
            {
                int rank = data[adata + 0x01];
                int @base = data[adata + 0x02];
                liveAlien = _rules.getAlienRaces()[race];
                liveAlien += _rules.getAlienRanks()[rank];
                if (@base != 0xFF)
                {
                    Base b = (Base)_targets[@base];
                    b.getStorageItems().addItem(liveAlien);
                }
            }
            _aliens.Add(liveAlien);
        }
    }

    /**
     * Loads the CRAFT.DAT file.
     * Contains X-COM craft and Alien UFOs.
     */
    void loadDatCraft()
    {
        byte[] data = binaryBuffer("CRAFT.DAT");

        int ENTRY_SIZE = data.Length / _targets.Count;
        for (int i = 0; i < _targets.Count; ++i)
        {
            int j = _targetDat[i];
            int cdata = j * ENTRY_SIZE;
            int type = data[cdata];
            if (type != 0xFF)
            {
                var node = new YamlMappingNode();
                Craft craft = (Craft)_targets[i];
                if (craft != null)
                {
                    craft.changeRules(_mod.getCraft(_rules.getCrafts()[type], true));

                    int lweapon = data[cdata + _rules.getOffset("CRAFT.DAT_LEFT_WEAPON")];
                    int lammo = BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_LEFT_AMMO"));
                    if (lweapon != 0xFF)
                    {
                        CraftWeapon cw = new CraftWeapon(_mod.getCraftWeapon(_rules.getCraftWeapons()[lweapon], true), lammo);
                        craft.getWeapons()[0] = cw;
                    }
                    int flight = data[cdata + _rules.getOffset("CRAFT.DAT_FLIGHT")];
                    int rweapon = data[cdata + _rules.getOffset("CRAFT.DAT_RIGHT_WEAPON")];
                    int rammo = data[cdata + _rules.getOffset("CRAFT.DAT_RIGHT_AMMO")];
                    if (rweapon != 0xFF)
                    {
                        CraftWeapon cw = new CraftWeapon(_mod.getCraftWeapon(_rules.getCraftWeapons()[rweapon], true), rammo);
                        craft.getWeapons()[1] = cw;
                    }

                    node.Add("damage", ((int)BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_DAMAGE"))).ToString());
                    node.Add("speed", ((int)BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_SPEED"))).ToString());
                    int dest = BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_DESTINATION"));
                    node.Add("fuel", ((int)BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_FUEL"))).ToString());
                    int @base = BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_BASE"));
                    node.Add("status", xcomStatus[BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_STATUS"))]);

                    // vehicles
                    const int VEHICLES = 5;
                    for (int k = 0; k < VEHICLES; ++k)
                    {
                        int qty = data[cdata + _rules.getOffset("CRAFT.DAT_ITEMS") + k];
                        for (int v = 0; v < qty; ++v)
                        {
                            RuleItem rule = _mod.getItem(_rules.getItems()[k + 10], true);
                            craft.getVehicles().Add(new Vehicle(rule, rule.getClipSize(), 4));
                        }
                    }
                    // items
                    const int ITEMS = 50;
                    for (int k = VEHICLES; k < VEHICLES + ITEMS; ++k)
                    {
                        int qty = data[cdata + _rules.getOffset("CRAFT.DAT_ITEMS") + k];
                        if (qty != 0 && !string.IsNullOrEmpty(_rules.getItems()[k + 10]))
                        {
                            craft.getItems().addItem(_rules.getItems()[k + 10], qty);
                        }
                    }

                    var state = new BitVector32(BitConverter.ToInt32(data, cdata + _rules.getOffset("CRAFT.DAT_STATE")));
                    int mask = BitVector32.CreateMask();
                    mask = BitVector32.CreateMask(mask);
                    node.Add("lowFuel", state[mask].ToString());

                    craft.load(node, _mod, _save);

                    if (flight != 0 && dest != 0xFFFF)
                    {
                        Target t = _targets[dest];
                        craft.setDestination(t);
                    }
                    if (@base != 0xFFFF)
                    {
                        Base b = (Base)_targets[@base];
                        craft.setBase(b, false);
                        b.getCrafts().Add(craft);
                    }
                }
                Ufo ufo = (Ufo)_targets[i];
                if (ufo != null)
                {
                    ufo.changeRules(_mod.getUfo(_rules.getUfos()[type - 5], true));
                    node.Add("damage", ((int)BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_DAMAGE"))).ToString());
                    node.Add("altitude", xcomAltitudes[BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_ALTITUDE"))]);
                    node.Add("speed", ((int)BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_SPEED"))).ToString());
                    var subNode = new YamlMappingNode
                    {
                        { "lon", Xcom2Rad(BitConverter.ToInt16(data, cdata + _rules.getOffset("CRAFT.DAT_DEST_LON"))).ToString() },
                        { "lat", Xcom2Rad(BitConverter.ToInt16(data, cdata + _rules.getOffset("CRAFT.DAT_DEST_LAT"))).ToString() }
                    };
                    node.Add("dest", subNode);

                    int mission = BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_MISSION"));
                    int region = BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_REGION"));
                    string trajectory = null;
                    AlienMission m = _missions[KeyValuePair.Create(mission, region)];
                    if (m == null)
                    {
                        var subnode = new YamlMappingNode();
                        m = new AlienMission(_mod.getAlienMission(_rules.getMissions()[mission], true));
                        subnode.Add("region", _rules.getRegions()[region]);
                        subnode.Add("race", _rules.getCrews()[BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_RACE"))]);
                        subnode.Add("nextWave", "1");
                        subnode.Add("nextUfoCounter", "0");
                        subnode.Add("spawnCountdown", "1000");
                        subnode.Add("uniqueID", _save.getId("ALIEN_MISSIONS").ToString());
                        m.load(subnode, _save);
                        _save.getAlienMissions().Add(m);
                        _missions[KeyValuePair.Create(mission, region)] = m;
                        if (mission == 6)
                        {
                            trajectory = UfoTrajectory.RETALIATION_ASSAULT_RUN;
                        }
                    }
                    node.Add("mission", m.getId().ToString());
                    m.increaseLiveUfos();
                    if (string.IsNullOrEmpty(trajectory))
                    {
                        trajectory = $"P{BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_TRAJECTORY"))}";
                    }
                    node.Add("trajectory", trajectory);
                    node.Add("trajectoryPoint", ((int)BitConverter.ToUInt16(data, cdata + _rules.getOffset("CRAFT.DAT_TRAJECTORY_POINT"))).ToString());
                    BitVector32 state = new BitVector32(BitConverter.ToInt32(data, cdata + _rules.getOffset("CRAFT.DAT_STATE")));
                    int mask = BitVector32.CreateMask();
                    mask = BitVector32.CreateMask(mask);
                    mask = BitVector32.CreateMask(mask);
                    mask = BitVector32.CreateMask(mask);
                    mask = BitVector32.CreateMask(mask);
                    mask = BitVector32.CreateMask(mask);
                    mask = BitVector32.CreateMask(mask);
                    node.Add("hyperDetected", state[mask].ToString());

                    ufo.load(node, _mod, _save);
                    ufo.setSpeed(ufo.getSpeed());
                    if (ufo.getStatus() == UfoStatus.CRASHED)
                    {
                        ufo.setSecondsRemaining(ufo.getSecondsRemaining() * 3600);
                    }
                    else if (ufo.getStatus() == UfoStatus.LANDED)
                    {
                        ufo.setSecondsRemaining(ufo.getSecondsRemaining() * 5);
                    }
                    else
                    {
                        ufo.setSecondsRemaining(0);
                    }

                    _save.getUfos().Add(ufo);
                }
            }
        }
    }

    /**
     * Loads the SOLDIER.DAT file.
     * Contains X-COM soldiers.
     */
    void loadDatSoldier()
    {
        byte[] data = binaryBuffer("SOLDIER.DAT");

        const int SOLDIERS = 250;
        int ENTRY_SIZE = data.Length / SOLDIERS;
        for (int i = 0; i < SOLDIERS; ++i)
        {
            int sdata = i * ENTRY_SIZE;
            int rank = BitConverter.ToUInt16(data, sdata + _rules.getOffset("SOLDIER.DAT_RANK"));
            if (rank != 0xFFFF)
            {
                var node = new YamlMappingNode();
                int @base = BitConverter.ToUInt16(data, sdata + _rules.getOffset("SOLDIER.DAT_BASE"));
                int craft = BitConverter.ToUInt16(data, sdata + _rules.getOffset("SOLDIER.DAT_CRAFT"));
                node.Add("missions", ((int)BitConverter.ToInt16(data, sdata + _rules.getOffset("SOLDIER.DAT_MISSIONS"))).ToString());
                node.Add("kills", ((int)BitConverter.ToInt16(data, sdata + _rules.getOffset("SOLDIER.DAT_KILLS"))).ToString());
                node.Add("recovery", ((int)BitConverter.ToInt16(data, sdata + _rules.getOffset("SOLDIER.DAT_RECOVERY"))).ToString());
                node.Add("name", BitConverter.ToString(data, sdata + _rules.getOffset("SOLDIER.DAT_NAME")));
                node.Add("rank", rank.ToString());

                UnitStats initial;
                initial.tu = data[sdata + _rules.getOffset("SOLDIER.DAT_INITIAL_TU")];
                initial.health = data[sdata + _rules.getOffset("SOLDIER.DAT_INITIAL_HE")];
                initial.stamina = data[sdata + _rules.getOffset("SOLDIER.DAT_INITIAL_STA")];
                initial.reactions = data[sdata + _rules.getOffset("SOLDIER.DAT_INITIAL_RE")];
                initial.strength = data[sdata + _rules.getOffset("SOLDIER.DAT_INITIAL_STR")];
                initial.firing = data[sdata + _rules.getOffset("SOLDIER.DAT_INITIAL_FA")];
                initial.throwing = data[sdata + _rules.getOffset("SOLDIER.DAT_INITIAL_TA")];
                initial.melee = data[sdata + _rules.getOffset("SOLDIER.DAT_INITIAL_ME")];
                initial.psiStrength = data[sdata + _rules.getOffset("SOLDIER.DAT_INITIAL_PST")];
                initial.psiSkill = data[sdata + _rules.getOffset("SOLDIER.DAT_INITIAL_PSK")];
                initial.bravery = 110 - (10 * data[sdata + _rules.getOffset("SOLDIER.DAT_INITIAL_BR")]);
                node.Add("initialStats", initial.save());

                UnitStats current;
                current.tu = data[sdata + _rules.getOffset("SOLDIER.DAT_IMPROVED_TU")];
                current.health = data[sdata + _rules.getOffset("SOLDIER.DAT_IMPROVED_HE")];
                current.stamina = data[sdata + _rules.getOffset("SOLDIER.DAT_IMPROVED_STA")];
                current.reactions = data[sdata + _rules.getOffset("SOLDIER.DAT_IMPROVED_RE")];
                current.strength = data[sdata + _rules.getOffset("SOLDIER.DAT_IMPROVED_STR")];
                current.firing = data[sdata + _rules.getOffset("SOLDIER.DAT_IMPROVED_FA")];
                current.throwing = data[sdata + _rules.getOffset("SOLDIER.DAT_IMPROVED_TA")];
                current.melee = data[sdata + _rules.getOffset("SOLDIER.DAT_IMPROVED_ME")];
                current.psiStrength = 0;
                current.psiSkill = 0;
                current.bravery = 10 * data[sdata + _rules.getOffset("SOLDIER.DAT_IMPROVED_BR")];
                current += initial;
                node.Add("currentStats", current.save());

                int armor = data[sdata + _rules.getOffset("SOLDIER.DAT_ARMOR")];
                node.Add("armor", _rules.getArmor()[armor]);
                node.Add("improvement", ((int)data[sdata + _rules.getOffset("SOLDIER.DAT_PSI")]).ToString());
                node.Add("psiTraining", ((int)BitConverter.ToChar(data, sdata + _rules.getOffset("SOLDIER.DAT_PSILAB")) != 0).ToString());
                node.Add("gender", ((int)data[sdata + _rules.getOffset("SOLDIER.DAT_GENDER")]).ToString());
                node.Add("look", ((int)data[sdata + _rules.getOffset("SOLDIER.DAT_LOOK")]).ToString());
                node.Add("id", _save.getId("STR_SOLDIER").ToString());

                Soldier soldier = new Soldier(_mod.getSoldier(_mod.getSoldiersList().First(), true), null);
                soldier.load(node, _mod, _save);
                if (@base != 0xFFFF)
                {
                    Base b = (Base)_targets[@base];
                    b.getSoldiers().Add(soldier);
                }
                if (craft != 0xFFFF)
                {
                    Craft c = (Craft)_targets[craft];
                    soldier.setCraft(c);
                }
                _soldiers.Add(soldier);
            }
            else
            {
                _soldiers.Add(null);
            }
        }
    }
}
