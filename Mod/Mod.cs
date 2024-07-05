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
 * Compares rules based on their list orders.
 */
struct compareRule : IComparer<string>
{
    Func<string, bool, IListOrder> _ruleLookup;

    internal compareRule(Func<string, bool, IListOrder> ruleLookup) =>
        _ruleLookup = ruleLookup;

    public int Compare(string x, string y)
    {
        IListOrder rule1 = _ruleLookup(x, true);
        IListOrder rule2 = _ruleLookup(y, true);
        return rule1.getListOrder() - rule2.getListOrder();
    }
}

/**
 * Craft weapons use the list order of their launcher item.
 */
struct compareRuleCraftWeapon : IComparer<string>
{
    Mod _mod;

    internal compareRuleCraftWeapon(Mod mod) =>
        _mod = mod;

    public int Compare(string x, string y)
    {
        IListOrder rule1 = _mod.getItem(_mod.getCraftWeapon(x).getLauncherItem(), true);
        IListOrder rule2 = _mod.getItem(_mod.getCraftWeapon(y).getLauncherItem(), true);
        return rule1.getListOrder() - rule2.getListOrder();
    }
}

/**
 * Armor uses the list order of their store item.
 * Itemless armor comes before all else.
 */
struct compareRuleArmor : IComparer<string>
{
    Mod _mod;

    internal compareRuleArmor(Mod mod) =>
        _mod = mod;

    public int Compare(string x, string y)
    {
        Armor armor1 = _mod.getArmor(x);
        Armor armor2 = _mod.getArmor(y);
        IListOrder rule1 = _mod.getItem(armor1.getStoreItem());
        IListOrder rule2 = _mod.getItem(armor2.getStoreItem());
        if (rule1 == null && rule2 == null)
            return armor1.GetHashCode() - armor2.GetHashCode(); // tiebreaker, don't care about order, pointers are as good as any
        else if (rule1 == null)
            return 1;
        else if (rule2 == null)
            return 0;
        else
            return rule1.getListOrder() - rule2.getListOrder();
    }
}

/**
 * Ufopaedia articles use section and list order.
 */
struct compareRuleArticleDefinition : IComparer<string>
{
    Mod _mod;
    Dictionary<string, int> _sections;

    internal compareRuleArticleDefinition(Mod mod)
    {
        _mod = mod;
        _sections = mod.getUfopaediaSections();
    }

    public int Compare(string x, string y)
    {
        ArticleDefinition rule1 = _mod.getUfopaediaArticle(x);
        ArticleDefinition rule2 = _mod.getUfopaediaArticle(y);
        if (rule1.section == rule2.section)
            return rule1.getListOrder() - rule2.getListOrder();
        else
            return _sections[rule1.section] - _sections[rule2.section];
    }
}

/**
 * Ufopaedia sections use article list order.
 */
struct compareSection : IComparer<string>
{
    Dictionary<string, int> _sections;

    internal compareSection(Mod mod) =>
        _sections = mod.getUfopaediaSections();

    public int Compare(string x, string y) =>
        _sections[x] - _sections[y];
}

/**
* Recolor class used in UFO
*/
struct HairXCOM1 : IColorFunc<byte, byte, int, int, int>
{
    const byte Hair = 9 << 4;
	internal const byte Face = 6 << 4;

	public void func(ref byte src, byte cutoff, int _1, int _2, int _3)
    {
        if (src > cutoff && src <= Face + Mod.ShadeMax)
		{
			src = (byte)(Hair + (src & Mod.ShadeMax) - 6); //make hair color like male in xcom_0.pck
		}
	}
}

/**
* Recolor class used in TFTD
*/
struct HairXCOM2 : IColorFunc<byte, int, int, int, int>
{
    const byte ManHairColor = 4 << 4;
    internal const byte WomanHairColor = 1 << 4;

    public void func(ref byte src, int _1, int _2, int _3, int _4)
    {
        if (src >= WomanHairColor && src <= WomanHairColor + Mod.ShadeMax)
        {
            src = (byte)(ManHairColor + (src & Mod.ShadeMax));
        }
    }
}

/**
* Recolor class used in TFTD
*/
struct FaceXCOM2 : IColorFunc<byte, int, int, int, int>
{
    const byte FaceColor = 10 << 4;
    internal const byte PinkColor = 14 << 4;

    public void func(ref byte src, int _1, int _2, int _3, int _4)
    {
        if (src >= FaceColor && src <= FaceColor + Mod.ShadeMax)
        {
            src = (byte)(PinkColor + (src & Mod.ShadeMax));
        }
    }
}

/**
* Recolor class used in TFTD
*/
struct BodyXCOM2 : IColorFunc<byte, int, int, int, int>
{
    internal const byte IonArmorColor = 8 << 4;

    public void func(ref byte src, int _1, int _2, int _3, int _4)
    {
        if (src == 153)
        {
            src = IonArmorColor + 12;
        }
        else if (src == 151)
        {
            src = IonArmorColor + 10;
        }
        else if (src == 148)
        {
            src = IonArmorColor + 4;
        }
        else if (src == 147)
        {
            src = IonArmorColor + 2;
        }
        else if (src >= HairXCOM2.WomanHairColor && src <= HairXCOM2.WomanHairColor + Mod.ShadeMax)
        {
            src = (byte)(IonArmorColor + (src & Mod.ShadeMax));
        }
    }
}

/**
* Recolor class used in TFTD
*/
struct FallXCOM2 : IColorFunc<byte, int, int, int, int>
{
    const byte RoguePixel = 151;

    public void func(ref byte src, int _1, int _2, int _3, int _4)
    {
        if (src == RoguePixel)
        {
            src = (byte)(FaceXCOM2.PinkColor + (src & Mod.ShadeMax) + 2);
        }
        else if (src >= BodyXCOM2.IonArmorColor && src <= BodyXCOM2.IonArmorColor + Mod.ShadeMax)
        {
            src = (byte)(FaceXCOM2.PinkColor + (src & Mod.ShadeMax));
        }
    }
}

/**
 * Mod data used when loading resources
 */
/* struct */ class ModData
{
    /// Mod name
    internal string name;
    /// Optional info about mod
    internal ModInfo info;
    /// Offset that mod use is common sets
    internal uint offset;
    /// Maximum size allowed by mod in common sets
    internal uint size;
}

/**
 * Contains all the game-specific static data that never changes
 * throughout the game, like rulesets and resources.
 */
internal class Mod
{
    internal const byte ShadeMax = 15;
    /// Predefined name for first loaded mod that have all original data
    const string ModNameMaster = "master";
    /// Predefined name for current mod that is loading rulesets.
    const string ModNameCurrent = "current";
    /// Reduction of size allocated for transparcey LUTs.
    const uint ModTransparceySizeReduction = 100;

    internal static int GEOSCAPE_CURSOR;
    internal static int BASESCAPE_CURSOR;
    internal static int UFOPAEDIA_CURSOR;
    internal static int GRAPHS_CURSOR;
    internal static int BATTLESCAPE_CURSOR;
    internal static int DOOR_OPEN;
    internal static int SLIDING_DOOR_OPEN;
    internal static int SLIDING_DOOR_CLOSE;
    internal static int SMALL_EXPLOSION;
    internal static int LARGE_EXPLOSION;
    internal static int EXPLOSION_OFFSET;
    internal static int SMOKE_OFFSET;
    internal static int UNDERWATER_SMOKE_OFFSET;
    internal static int ITEM_DROP;
    internal static int ITEM_THROW;
    internal static int ITEM_RELOAD;
    internal static int WALK_OFFSET;
    internal static int FLYING_SOUND;
    static int BUTTON_PRESS;
    internal static int UFO_FIRE;
    internal static int UFO_HIT;
    internal static int UFO_CRASH;
    internal static int UFO_EXPLODE;
    internal static int INTERCEPTOR_HIT;
    internal static int INTERCEPTOR_EXPLODE;
    internal static int DAMAGE_RANGE;
    internal static int EXPLOSIVE_DAMAGE_RANGE;
    static int[] WINDOW_POPUP = new int[3];
    internal static int[] FIRE_DAMAGE_RANGE = new int[2];
    internal static int[] DIFFICULTY_COEFFICIENT = new int[5];
    internal static string DEBRIEF_MUSIC_GOOD;
    internal static string DEBRIEF_MUSIC_BAD;

    Music _muteMusic;
    Sound _muteSound;
    Dictionary<string, Palette> _palettes;
    Dictionary<string, Font> _fonts;
    Dictionary<string, Surface> _surfaces;
    Dictionary<string, SurfaceSet> _sets;
    Dictionary<string, SoundSet> _sounds;
    Dictionary<string, Music> _musics;
    Dictionary<string, RuleCountry> _countries;
    Dictionary<string, RuleRegion> _regions;
    Dictionary<string, RuleBaseFacility> _facilities;
    Dictionary<string, RuleCraft> _crafts;
    Dictionary<string, RuleCraftWeapon> _craftWeapons;
    Dictionary<string, RuleItem> _items;
    Dictionary<string, RuleUfo> _ufos;
    Dictionary<string, RuleTerrain> _terrains;
    Dictionary<string, MapDataSet> _mapDataSets;
    Dictionary<string, RuleSoldier> _soldiers;
    Dictionary<string, Unit> _units;
    Dictionary<string, AlienRace> _alienRaces;
    Dictionary<string, AlienDeployment> _alienDeployments;
    Dictionary<string, Armor> _armors;
    Dictionary<string, ArticleDefinition> _ufopaediaArticles;
    Dictionary<string, RuleInventory> _invs;
    Dictionary<string, RuleResearch> _research;
    Dictionary<string, RuleManufacture> _manufacture;
    Dictionary<string, UfoTrajectory> _ufoTrajectories;
    Dictionary<string, RuleAlienMission> _alienMissions;
    Dictionary<string, RuleVideo> _videos;
    Dictionary<string, MCDPatch> _MCDPatches;
    Dictionary<string, List<MapScript>> _mapScripts;
    Dictionary<string, RuleCommendations> _commendations;
    Dictionary<string, RuleMissionScript> _missionScripts;
    Dictionary<string, List<ExtraSprites>> _extraSprites;
    List<KeyValuePair<string, ExtraSounds>> _extraSounds;
    Dictionary<string, ExtraStrings> _extraStrings;
    Dictionary<string, RuleInterface> _interfaces;
    Dictionary<string, SoundDefinition> _soundDefs;
    Dictionary<string, RuleMusic> _musicDefs;
    List<StatString> _statStrings;
    RuleGlobe _globe;
    RuleConverter _converter;
    int _costEngineer, _costScientist, _timePersonnel, _initialFunding, _turnAIUseGrenade, _turnAIUseBlaster, _defeatScore, _defeatFunds;
    bool _difficultyDemigod;
    GameTime _startingTime;
    StatAdjustment[] _statAdjustment = new StatAdjustment[5];
    int _facilityListOrder, _craftListOrder, _itemListOrder, _researchListOrder, _manufactureListOrder, _ufopaediaListOrder, _invListOrder;
    ModData _modCurrent;
    SDL_Color[] _statePalette;
    List<List<byte>> _transparencyLUTs;
    string _playingMusic;
    List<ModData> _modData;
    List<string> _countriesIndex, _regionsIndex, _facilitiesIndex, _craftsIndex, _craftWeaponsIndex, _itemsIndex, _invsIndex, _ufosIndex;
    List<string> _soldiersIndex, _aliensIndex, _deploymentsIndex, _armorsIndex, _ufopaediaIndex, _ufopaediaCatIndex, _researchIndex, _manufactureIndex;
    List<string> _alienMissionsIndex, _terrainIndex, _missionScriptIndex;
    Dictionary<string, int> _ufopaediaSections;
    string _fontName, _finalResearch;
    List<ushort> _voxelData;
    List<SDL_Color> _transparencies;
    YamlMappingNode _startingBase;
    KeyValuePair<string, int> _alienFuel;
    List<List<int>> _alienItemLevels;
    List<string> _psiRequirements; // it's a cache for psiStrengthEval

    /**
     * Creates an empty mod.
     */
    internal Mod()
    {
        _costEngineer = 0;
        _costScientist = 0;
        _timePersonnel = 0;
        _initialFunding = 0;
        _turnAIUseGrenade = 3;
        _turnAIUseBlaster = 3;
        _defeatScore = 0;
        _defeatFunds = 0;
        _difficultyDemigod = false;
        _startingTime = new GameTime(6, 1, 1, 1999, 12, 0, 0);
        _facilityListOrder = 0;
        _craftListOrder = 0;
        _itemListOrder = 0;
        _researchListOrder = 0;
        _manufactureListOrder = 0;
        _ufopaediaListOrder = 0;
        _invListOrder = 0;
        _modCurrent = default;
        _statePalette = null;

        _muteMusic = new Music();
        _muteSound = new Sound();
        _globe = new RuleGlobe();
        _converter = new RuleConverter();
        _statAdjustment[0].aimAndArmorMultiplier = 0.5;
        _statAdjustment[0].growthMultiplier = 0;
        for (int i = 1; i != 5; ++i)
        {
            _statAdjustment[i].aimAndArmorMultiplier = 1.0;
            _statAdjustment[i].growthMultiplier = i;
        }
    }

    /**
     * Deletes all the mod data from memory.
     */
    ~Mod()
    {
        _muteMusic = null;
        _muteSound = null;
        _globe = null;
        _converter = null;
        _fonts.Clear();
        _surfaces.Clear();
        _sets.Clear();
        _palettes.Clear();
        _musics.Clear();
        _sounds.Clear();
        _countries.Clear();
        _regions.Clear();
        _facilities.Clear();
        _crafts.Clear();
        _craftWeapons.Clear();
        _items.Clear();
        _ufos.Clear();
        _terrains.Clear();
        _mapDataSets.Clear();
        _soldiers.Clear();
        _units.Clear();
        _alienRaces.Clear();
        _alienDeployments.Clear();
        _armors.Clear();
        _ufopaediaArticles.Clear();
        _invs.Clear();
        _research.Clear();
        _manufacture.Clear();
        _ufoTrajectories.Clear();
        _alienMissions.Clear();
        _MCDPatches.Clear();
        _extraSprites.Clear();
        _extraSounds.Clear();
        _extraStrings.Clear();
        _interfaces.Clear();
        _mapScripts.Clear();
        _videos.Clear();
        _musicDefs.Clear();
        _missionScripts.Clear();
        _soundDefs.Clear();
        _statStrings.Clear();
        _commendations.Clear();
    }

    /**
     * Returns a specific font from the mod.
     * @param name Name of the font.
     * @return Pointer to the font.
     */
    internal Font getFont(string name, bool error = true) =>
	    getRule(name, "Font", _fonts, error);

    /**
     * Changes the palette of all the graphics in the mod.
     * @param colors Pointer to the set of colors.
     * @param firstcolor Offset of the first color to replace.
     * @param ncolors Amount of colors to replace.
     */
    internal void setPalette(SDL_Color[] colors, int firstcolor = 0, int ncolors = 256)
    {
        _statePalette = colors;
        foreach (var i in _fonts)
        {
            i.Value.setPalette(colors, firstcolor, ncolors);
        }
        foreach (var i in _surfaces)
        {
            if (!CrossPlatform.compareExt(i.Key, "LBM"))
                i.Value.setPalette(colors, firstcolor, ncolors);
        }
        foreach (var i in _sets)
        {
            i.Value.setPalette(colors, firstcolor, ncolors);
        }
    }

    /**
     * Gets a specific rule element by ID.
     * @param id String ID of the rule element.
     * @param name Human-readable name of the rule type.
     * @param map Map associated to the rule type.
     * @param error Throw an error if not found.
     * @return Pointer to the rule element, or NULL if not found.
     */
    T getRule<T>(string id, string name, Dictionary<string, T> map, bool error) where T : class
    {
	    if (string.IsNullOrEmpty(id))
	    {
		    return null;
	    }
	    if (map.TryGetValue(id, out var value) && value != null)
	    {
		    return value;
	    }
	    else
	    {
		    if (error)
		    {
			    throw new Exception($"{name} {id} not found");
		    }
		    return null;
	    }
    }

    /**
     * Gets information on an interface.
     * @param id the interface we want info on.
     * @return the interface.
     */
    internal RuleInterface getInterface(string id, bool error = true) =>
	    getRule(id, "Interface", _interfaces, error);

    /**
     * Returns a specific surface set from the mod.
     * @param name Name of the surface set.
     * @return Pointer to the surface set.
     */
    internal SurfaceSet getSurfaceSet(string name, bool error = true)
    {
	    lazyLoadSurface(name);
	    return getRule(name, "Sprite Set", _sets, error);
    }

    /**
     * Returns the list of color LUTs in the mod.
     * @return Pointer to the list of LUTs.
     */
    internal List<List<byte>> getLUTs() =>
	    _transparencyLUTs;

    /**
     * Loads any extra sprites associated to a surface when
     * it's first requested.
     * @param name Surface name.
     */
    void lazyLoadSurface(string name)
    {
	    if (Options.lazyLoadResources)
	    {
		    if (_extraSprites.TryGetValue(name, out var extraSprites))
		    {
			    foreach (var extraSprite in extraSprites)
			    {
                    loadExtraSprite(extraSprite);
			    }
		    }
	    }
    }

    void loadExtraSprite(ExtraSprites spritePack)
    {
        if (spritePack.isLoaded())
            return;

        if (spritePack.getSingleImage())
        {
            Surface surface = null;
            if (_surfaces.TryGetValue(spritePack.getType(), out var value))
            {
                surface = value;
            }

            _surfaces[spritePack.getType()] = spritePack.loadSurface(surface);
            if (_statePalette != null)
            {
                _surfaces[spritePack.getType()].setPalette(_statePalette);
            }
        }
        else
        {
            SurfaceSet set = null;
            if (_sets.TryGetValue(spritePack.getType(), out var value))
            {
                set = value;
            }

            _sets[spritePack.getType()] = spritePack.loadSurfaceSet(set);
            if (_statePalette != null)
            {
                _sets[spritePack.getType()].setPalette(_statePalette);
            }
        }
    }

    /**
     * Returns the rules for a specific inventory.
     * @param id Inventory type.
     * @return Inventory ruleset.
     */
    internal RuleInventory getInventory(string id, bool error = false) =>
	    getRule(id, "Inventory", _invs, error);

    /**
     * Returns the rules for the specified item.
     * @param id Item type.
     * @return Rules for the item, or 0 when the item is not found.
     */
    internal RuleItem getItem(string id, bool error = false)
    {
	    if (id == Armor.NONE)
	    {
            return null;
	    }
	    return getRule(id, "Item", _items, error);
    }

    /**
     * Plays the specified track if it's not already playing.
     * @param name Name of the music.
     * @param id Id of the music, 0 for random.
     */
    internal void playMusic(string name, int id = 0)
    {
	    if (!Options.mute && _playingMusic != name)
	    {
		    int loop = -1;
		    // hacks
		    if (!Options.musicAlwaysLoop && (name == "GMSTORY" || name == "GMWIN" || name == "GMLOSE"))
		    {
			    loop = 0;
		    }

		    Music music = null;
		    if (id == 0)
		    {
			    music = getRandomMusic(name);
		    }
		    else
		    {
			    string ss = $"{name}{id}";
			    music = getMusic(ss);
		    }
            music.play(loop);
		    if (music != _muteMusic)
		    {
			    _playingMusic = name;
		    }
	    }
    }

    /**
     * Returns a specific music from the mod.
     * @param name Name of the music.
     * @return Pointer to the music.
     */
    internal Music getMusic(string name, bool error = true)
    {
	    if (Options.mute)
	    {
		    return _muteMusic;
	    }
	    else
	    {
		    return getRule(name, "Music", _musics, error);
	    }
    }

    /**
     * Returns a random music from the mod.
     * @param name Name of the music to pick from.
     * @return Pointer to the music.
     */
    Music getRandomMusic(string name)
    {
	    if (Options.mute)
	    {
            return _muteMusic;
	    }
	    else
	    {
		    var music = new List<Music>();
		    foreach (var i in _musics)
		    {
			    if (i.Key.Contains(name))
			    {
				    music.Add(i.Value);
			    }
		    }
            if (!music.Any())
		    {
			    return _muteMusic;
		    }
		    else
		    {
			    return music[RNG.seedless(0, music.Count - 1)];
		    }
	    }
    }

    /**
     * Returns a specific palette from the mod.
     * @param name Name of the palette.
     * @return Pointer to the palette.
     */
    internal Palette getPalette(string name, bool error = true) =>
	    getRule(name, "Palette", _palettes, error);

    /**
     * Returns a specific surface from the mod.
     * @param name Name of the surface.
     * @return Pointer to the surface.
     */
    internal Surface getSurface(string name, bool error = true)
    {
	    lazyLoadSurface(name);
	    return getRule(name, "Sprite", _surfaces, error);
    }

    internal static void resetGlobalStatics()
    {
        DOOR_OPEN = 3;
        SLIDING_DOOR_OPEN = 20;
        SLIDING_DOOR_CLOSE = 21;
        SMALL_EXPLOSION = 2;
        LARGE_EXPLOSION = 5;
        EXPLOSION_OFFSET = 0;
        SMOKE_OFFSET = 8;
        UNDERWATER_SMOKE_OFFSET = 0;
        ITEM_DROP = 38;
        ITEM_THROW = 39;
        ITEM_RELOAD = 17;
        WALK_OFFSET = 22;
        FLYING_SOUND = 15;
        BUTTON_PRESS = 0;
        WINDOW_POPUP[0] = 1;
        WINDOW_POPUP[1] = 2;
        WINDOW_POPUP[2] = 3;
        UFO_FIRE = 8;
        UFO_HIT = 12;
        UFO_CRASH = 10;
        UFO_EXPLODE = 11;
        INTERCEPTOR_HIT = 10;
        INTERCEPTOR_EXPLODE = 13;
        GEOSCAPE_CURSOR = 252;
        BASESCAPE_CURSOR = 252;
        BATTLESCAPE_CURSOR = 144;
        UFOPAEDIA_CURSOR = 252;
        GRAPHS_CURSOR = 252;
        DAMAGE_RANGE = 100;
        EXPLOSIVE_DAMAGE_RANGE = 50;
        FIRE_DAMAGE_RANGE[0] = 5;
        FIRE_DAMAGE_RANGE[1] = 10;
        DEBRIEF_MUSIC_GOOD = "GMMARS";
        DEBRIEF_MUSIC_BAD = "GMMARS";

        Globe.OCEAN_COLOR = Palette.blockOffset(12);
        Globe.OCEAN_SHADING = true;
        Globe.COUNTRY_LABEL_COLOR = 239;
        Globe.LINE_COLOR = 162;
        Globe.CITY_LABEL_COLOR = 138;
        Globe.BASE_LABEL_COLOR = 133;

        TextButton.soundPress = null;

        Window.soundPopup[0] = null;
        Window.soundPopup[1] = null;
        Window.soundPopup[2] = null;

        Pathfinding.red = 3;
        Pathfinding.yellow = 10;
        Pathfinding.green = 4;

        DIFFICULTY_COEFFICIENT[0] = 0;
        DIFFICULTY_COEFFICIENT[1] = 1;
        DIFFICULTY_COEFFICIENT[2] = 2;
        DIFFICULTY_COEFFICIENT[3] = 3;
        DIFFICULTY_COEFFICIENT[4] = 4;
    }

    /**
     * Gets the rules for the Geoscape globe.
     * @return Pointer to globe rules.
     */
    internal RuleGlobe getGlobe() =>
	    _globe;

    /**
     * Gets the list of external strings.
     * @return The list of external strings.
     */
    internal Dictionary<string, ExtraStrings> getExtraStrings() =>
	    _extraStrings;

    /**
     * Loads a list of mods specified in the options.
     * @param mods List of <modId, rulesetFiles> pairs.
     */
    internal void loadAll(List<KeyValuePair<string, List<string>>> mods)
    {
        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Loading rulesets...");
	    _modData.Clear();
	    _modData = new List<ModData>(mods.Count);

        var usedModNames = new HashSet<string>
        {
            ModNameMaster,
            ModNameCurrent
        };

        // calculated offsets and other things for all mods
        var offset = 0;
	    for (var i = 0; mods.Count > i; ++i)
	    {
		    string modId = mods[i].Key;
		    if (!usedModNames.Add(modId))
		    {
                throwModOnErrorHelper(modId, "this mod name is already used");
		    }
            ModInfo modInfo = Options.getModInfos()[modId];
            int size = modInfo.getReservedSpace();
		    _modData[i].name = modId;
		    _modData[i].offset = (uint)(1000 * offset);
		    _modData[i].info = modInfo;
            _modData[i].size = (uint)(1000 * size);
		    offset += size;
	    }

	    // load rulesets that can affect loading vanilla resources
	    for (var i = 0; _modData.Count > i; ++i)
	    {
		    _modCurrent = _modData[i];
		    ModInfo info = _modCurrent.info;
		    if (info.isMaster() && !string.IsNullOrEmpty(info.getResourceConfigFile()))
		    {
			    string path = info.getPath() + "/" + info.getResourceConfigFile();
			    if (CrossPlatform.fileExists(path))
			    {
				    loadResourceConfigFile(path);
			    }
		    }
	    }

	    // vanilla resources load
	    _modCurrent = _modData[0];
	    loadVanillaResources();

	    // load rest rulesets
	    for (var i = 0; mods.Count > i; ++i)
	    {
		    try
		    {
			    _modCurrent = _modData[i];
			    loadMod(mods[i].Value);
		    }
		    catch (Exception e)
		    {
			    string modId = mods[i].Key;
			    throwModOnErrorHelper(modId, e.Message);
		    }
	    }

	    //back master
	    _modCurrent = _modData[0];
	    sortLists();
	    loadExtraResources();
	    modResources();
    }

    /**
     * Helper function used to disable invalid mod and throw exception to quit game
     * @param modId Mod id
     * @param error Error message
     */
    static void throwModOnErrorHelper(string modId, string error)
    {
        string errorStream = $"failed to load '{Options.getModInfos()[modId].getName()}'";

	    if (!Options.debug)
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} disabling mod with invalid ruleset: {modId}");
            var index = Options.mods.FindIndex(x => x.Key == modId && x.Value == true);
		    if (index == -1)
		    {
                Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} cannot find broken mod in mods list: {modId}");
                Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} clearing mods list");
			    Options.mods.Clear();
		    }
		    else
		    {
                Options.mods[index] = KeyValuePair.Create(Options.mods[index].Key, false);
		    }
		    Options.save();

		    errorStream += "; mod disabled";
	    }
	    errorStream += $"{Environment.NewLine}{error}";

	    throw new Exception(errorStream);
    }

    /**
     * Sorts all our lists according to their weight.
     */
    void sortLists()
    {
        _itemsIndex.Sort(new compareRule(getItem));
        _craftsIndex.Sort(new compareRule(getCraft));
        _facilitiesIndex.Sort(new compareRule(getBaseFacility));
        _researchIndex.Sort(new compareRule(getResearch));
        _manufactureIndex.Sort(new compareRule(getManufacture));
        _invsIndex.Sort(new compareRule(getInventory));
        // special cases
        _craftWeaponsIndex.Sort(new compareRuleCraftWeapon(this));
        _armorsIndex.Sort(new compareRuleArmor(this));
        _ufopaediaSections[Ufopaedia.Ufopaedia.UFOPAEDIA_NOT_AVAILABLE] = 0;
        _ufopaediaIndex.Sort(new compareRuleArticleDefinition(this));
        _ufopaediaCatIndex.Sort(new compareSection(this));
    }

    /**
     * Returns the rules for the specified craft.
     * @param id Craft type.
     * @return Rules for the craft.
     */
    internal RuleCraft getCraft(string id, bool error = false) =>
	    getRule(id, "Craft", _crafts, error);

    /**
     * Returns the rules for the specified base facility.
     * @param id Facility type.
     * @return Rules for the facility.
     */
    internal RuleBaseFacility getBaseFacility(string id, bool error = false) =>
	    getRule(id, "Facility", _facilities, error);

    /**
     * Returns the rules for the specified research project.
     * @param id Research project type.
     * @return Rules for the research project.
     */
    internal RuleResearch getResearch(string id, bool error = false) =>
	    getRule(id, "Research", _research, error);

    /**
     * Returns the rules for the specified manufacture project.
     * @param id Manufacture project type.
     * @return Rules for the manufacture project.
     */
    internal RuleManufacture getManufacture(string id, bool error = false) =>
	    getRule(id, "Manufacture", _manufacture, error);

    /**
     * Returns the rules for the specified craft weapon.
     * @param id Craft weapon type.
     * @return Rules for the craft weapon.
     */
    internal RuleCraftWeapon getCraftWeapon(string id, bool error = false) =>
	    getRule(id, "Craft Weapon", _craftWeapons, error);

    /**
     * Returns the info about a specific armor.
     * @param name Armor name.
     * @return Rules for the armor.
     */
    internal Armor getArmor(string name, bool error = false) =>
	    getRule(name, "Armor", _armors, error);

    /**
     * Returns the article definition for a given name.
     * @param name Article name.
     * @return Article definition.
     */
    internal ArticleDefinition getUfopaediaArticle(string name, bool error = false) =>
        getRule(name, "UFOpaedia Article", _ufopaediaArticles, error);

    /// For internal use only
    internal Dictionary<string, int> getUfopaediaSections() =>
        _ufopaediaSections;

    /**
     * Applies necessary modifications to vanilla resources.
     */
    void modResources()
    {
        // we're gonna need these
        getSurface("GEOBORD.SCR");
        getSurface("ALTGEOBORD.SCR", false);
        getSurface("BACK07.SCR");
        getSurface("ALTBACK07.SCR", false);
        getSurface("BACK06.SCR");
        getSurface("UNIBORD.PCK");
        getSurfaceSet("HANDOB.PCK");
        getSurfaceSet("FLOOROB.PCK");
        getSurfaceSet("BIGOBS.PCK");

        // embiggen the geoscape background by mirroring the contents
        // modders can provide their own backgrounds via ALTGEOBORD.SCR
        if (!_surfaces.ContainsKey("ALTGEOBORD.SCR"))
        {
            int newWidth = 320 - 64, newHeight = 200;
            Surface newGeo = new Surface(newWidth * 3, newHeight * 3);
            Surface oldGeo = _surfaces["GEOBORD.SCR"];
            for (int x = 0; x < newWidth; ++x)
            {
                for (int y = 0; y < newHeight; ++y)
                {
                    newGeo.setPixel(newWidth + x, newHeight + y, oldGeo.getPixel(x, y));
                    newGeo.setPixel(newWidth - x - 1, newHeight + y, oldGeo.getPixel(x, y));
                    newGeo.setPixel(newWidth * 3 - x - 1, newHeight + y, oldGeo.getPixel(x, y));

                    newGeo.setPixel(newWidth + x, newHeight - y - 1, oldGeo.getPixel(x, y));
                    newGeo.setPixel(newWidth - x - 1, newHeight - y - 1, oldGeo.getPixel(x, y));
                    newGeo.setPixel(newWidth * 3 - x - 1, newHeight - y - 1, oldGeo.getPixel(x, y));

                    newGeo.setPixel(newWidth + x, newHeight * 3 - y - 1, oldGeo.getPixel(x, y));
                    newGeo.setPixel(newWidth - x - 1, newHeight * 3 - y - 1, oldGeo.getPixel(x, y));
                    newGeo.setPixel(newWidth * 3 - x - 1, newHeight * 3 - y - 1, oldGeo.getPixel(x, y));
                }
            }
            _surfaces["ALTGEOBORD.SCR"] = newGeo;
        }

        // here we create an "alternate" background surface for the base info screen.
        if (!_surfaces.ContainsKey("ALTBACK07.SCR"))
        {
            _surfaces["ALTBACK07.SCR"] = new Surface(320, 200);
            _surfaces["ALTBACK07.SCR"].loadScr(FileMap.getFilePath("GEOGRAPH/BACK07.SCR"));
            for (int y = 172; y >= 152; --y)
                for (int x = 5; x <= 314; ++x)
                    _surfaces["ALTBACK07.SCR"].setPixel(x, y + 4, _surfaces["ALTBACK07.SCR"].getPixel(x, y));
            for (int y = 147; y >= 134; --y)
                for (int x = 5; x <= 314; ++x)
                    _surfaces["ALTBACK07.SCR"].setPixel(x, y + 9, _surfaces["ALTBACK07.SCR"].getPixel(x, y));
            for (int y = 132; y >= 109; --y)
                for (int x = 5; x <= 314; ++x)
                    _surfaces["ALTBACK07.SCR"].setPixel(x, y + 10, _surfaces["ALTBACK07.SCR"].getPixel(x, y));
        }

        // we create extra rows on the soldier stat screens by shrinking them all down one pixel.

        // first, let's do the base info screen
        // erase the old lines, copying from a +2 offset to account for the dithering
        for (int y = 91; y < 199; y += 12)
            for (int x = 0; x < 149; ++x)
                _surfaces["BACK06.SCR"].setPixel(x, y, _surfaces["BACK06.SCR"].getPixel(x, y + 2));
        // drawn new lines, use the bottom row of pixels as a basis
        for (int y = 89; y < 199; y += 11)
            for (int x = 0; x < 149; ++x)
                _surfaces["BACK06.SCR"].setPixel(x, y, _surfaces["BACK06.SCR"].getPixel(x, 199));
        // finally, move the top of the graph up by one pixel, offset for the last iteration again due to dithering.
        for (int y = 72; y < 80; ++y)
            for (int x = 0; x < 320; ++x)
            {
                _surfaces["BACK06.SCR"].setPixel(x, y, _surfaces["BACK06.SCR"].getPixel(x, y + (y == 79 ? 2 : 1)));
            }

        // now, let's adjust the battlescape info screen.
        // erase the old lines, no need to worry about dithering on this one.
        for (int y = 39; y < 199; y += 10)
            for (int x = 0; x < 169; ++x)
                _surfaces["UNIBORD.PCK"].setPixel(x, y, _surfaces["UNIBORD.PCK"].getPixel(x, 30));
        // drawn new lines, use the bottom row of pixels as a basis
        for (int y = 190; y > 37; y -= 9)
            for (int x = 0; x < 169; ++x)
                _surfaces["UNIBORD.PCK"].setPixel(x, y, _surfaces["UNIBORD.PCK"].getPixel(x, 199));
        // move the top of the graph down by eight pixels to erase the row we don't need (we actually created ~1.8 extra rows earlier)
        for (int y = 37; y > 29; --y)
            for (int x = 0; x < 320; ++x)
            {
                _surfaces["UNIBORD.PCK"].setPixel(x, y, _surfaces["UNIBORD.PCK"].getPixel(x, y - 8));
                _surfaces["UNIBORD.PCK"].setPixel(x, y - 8, 0);
            }

        // copy constructor doesn't like doing this directly, so let's make a second handobs file the old fashioned way.
        // handob2 is used for all the left handed sprites.
        _sets["HANDOB2.PCK"] = new SurfaceSet(_sets["HANDOB.PCK"].getWidth(), _sets["HANDOB.PCK"].getHeight());
        Dictionary<int, Surface> handob = _sets["HANDOB.PCK"].getFrames();
        foreach (var item in handob)
        {
            Surface surface1 = _sets["HANDOB2.PCK"].addFrame(item.Key);
            Surface surface2 = item.Value;
            surface1.setPalette(surface2.getPalette());
            surface2.blit(surface1);
        }
    }

    /**
     * Loads the vanilla resources required by the game.
     */
    void loadVanillaResources()
    {
        // Create Geoscape surface
        _sets["GlobeMarkers"] = new SurfaceSet(3, 3);
        // Create Sound sets
        _sounds["GEO.CAT"] = new SoundSet();
        _sounds["BATTLE.CAT"] = new SoundSet();
        _sounds["BATTLE2.CAT"] = new SoundSet();

        // Load palettes
        string[] pal = { "PAL_GEOSCAPE", "PAL_BASESCAPE", "PAL_GRAPHS", "PAL_UFOPAEDIA", "PAL_BATTLEPEDIA" };
        for (var i = 0; i < pal.Length; ++i)
        {
            string s = "GEODATA/PALETTES.DAT";
            _palettes[pal[i]] = new Palette();
            _palettes[pal[i]].loadDat(FileMap.getFilePath(s), 256, Palette.palOffset(i));
        }
        {
            string s1 = "GEODATA/BACKPALS.DAT";
            string s2 = "BACKPALS.DAT";
            _palettes[s2] = new Palette();
            _palettes[s2].loadDat(FileMap.getFilePath(s1), 128);
        }

        // Correct Battlescape palette
        {
            string s1 = "GEODATA/PALETTES.DAT";
            string s2 = "PAL_BATTLESCAPE";
            _palettes[s2] = new Palette();
            _palettes[s2].loadDat(FileMap.getFilePath(s1), 256, Palette.palOffset(4));

            // Last 16 colors are a greyish gradient
            SDL_Color[] gradient =
            {
                new() { r = 140, g = 152, b = 148, a = 255 },
                new() { r = 132, g = 136, b = 140, a = 255 },
                new() { r = 116, g = 124, b = 132, a = 255 },
                new() { r = 108, g = 116, b = 124, a = 255 },
                new() { r = 92, g = 104, b = 108, a = 255 },
                new() { r = 84, g = 92, b = 100, a = 255 },
                new() { r = 76, g = 80, b = 92, a = 255 },
                new() { r = 56, g = 68, b = 84, a = 255 },
                new() { r = 48, g = 56, b = 68, a = 255 },
                new() { r = 40, g = 48, b = 56, a = 255 },
                new() { r = 32, g = 36, b = 48, a = 255 },
                new() { r = 24, g = 28, b = 32, a = 255 },
                new() { r = 16, g = 20, b = 24, a = 255 },
                new() { r = 8, g = 12, b = 16, a = 255 },
                new() { r = 3, g = 4, b = 8, a = 255 },
                new() { r = 3, g = 3, b = 6, a = 255 }
            };
            Span<SDL_Color> color = _palettes[s2].getColors(Palette.backPos + 16);
            for (var i = 0; i < gradient.Length; ++i)
            {
                color[i] = gradient[i];
            }
        }

        // Load surfaces
        {
            string s1 = "GEODATA/INTERWIN.DAT";
            string s2 = "INTERWIN.DAT";
            _surfaces[s2] = new Surface(160, 600);
            _surfaces[s2].loadScr(FileMap.getFilePath(s1));
        }

        HashSet<string> geographFiles = FileMap.getVFolderContents("GEOGRAPH");
        HashSet<string> scrs = FileMap.filterFiles(geographFiles, "SCR");
        foreach (var scr in scrs)
        {
            string fname = scr.ToUpper();
            _surfaces[fname] = new Surface(320, 200);
            _surfaces[fname].loadScr(FileMap.getFilePath("GEOGRAPH/" + fname));
        }
        HashSet<string> bdys = FileMap.filterFiles(geographFiles, "BDY");
        foreach (var bdy in bdys)
        {
            string fname = bdy.ToUpper();
            _surfaces[fname] = new Surface(320, 200);
            _surfaces[fname].loadBdy(FileMap.getFilePath("GEOGRAPH/" + fname));
        }

        HashSet<string> spks = FileMap.filterFiles(geographFiles, "SPK");
        foreach (var spk in spks)
        {
            string fname = spk.ToUpper();
            _surfaces[fname] = new Surface(320, 200);
            _surfaces[fname].loadSpk(FileMap.getFilePath("GEOGRAPH/" + fname));
        }

        // Load surface sets
        string[] sets = { "BASEBITS.PCK", "INTICON.PCK", "TEXTURE.DAT" };

        for (var i = 0; i < sets.Length; ++i)
        {
            string s = $"GEOGRAPH/{sets[i]}";

            string ext = sets[i].Substring(sets[i].LastIndexOf('.') + 1, sets[i].Length);
            if (ext == "PCK")
            {
                string tab = CrossPlatform.noExt(sets[i]) + ".TAB";
                string s2 = $"GEOGRAPH/{tab}";
                _sets[sets[i]] = new SurfaceSet(32, 40);
                _sets[sets[i]].loadPck(FileMap.getFilePath(s), FileMap.getFilePath(s2));
            }
            else
            {
                _sets[sets[i]] = new SurfaceSet(32, 32);
                _sets[sets[i]].loadDat(FileMap.getFilePath(s));
            }
        }
        {
            string s1 = "GEODATA/SCANG.DAT";
            string s2 = "SCANG.DAT";
            _sets[s2] = new SurfaceSet(4, 4);
            _sets[s2].loadDat(FileMap.getFilePath(s1));
        }

        if (!Options.mute)
        {
            // Load sounds
            HashSet<string> soundFiles = FileMap.getVFolderContents("SOUND");

            if (!_soundDefs.Any())
            {
                string[] catsId = { "GEO.CAT", "BATTLE.CAT" };
                string[] catsDos = { "SOUND2.CAT", "SOUND1.CAT" };
                string[] catsWin = { "SAMPLE.CAT", "SAMPLE2.CAT" };

                // Try the preferred format first, otherwise use the default priority
                string[][] cats = { null, catsWin, catsDos };
                if (Options.preferredSound == SoundFormat.SOUND_14)
                    cats[0] = catsWin;
                else if (Options.preferredSound == SoundFormat.SOUND_10)
                    cats[0] = catsDos;

                Options.currentSound = SoundFormat.SOUND_AUTO;
                for (var i = 0; i < catsId.Length; ++i)
                {
                    SoundSet sound = null;
                    for (var j = 0; j < cats.Length && sound == null; ++j)
                    {
                        bool wav = true;
                        if (cats[j] == null)
                            continue;
                        else if (cats[j] == catsDos)
                            wav = false;
                        string fname = cats[j][i];
                        fname = fname.ToLower();
                        if (soundFiles.Contains(fname))
                        {
                            sound = _sounds[catsId[i]];
                            sound.loadCat(FileMap.getFilePath("SOUND/" + cats[j][i]), wav);
                            Options.currentSound = (wav) ? SoundFormat.SOUND_14 : SoundFormat.SOUND_10;
                        }
                    }
                    if (sound == null)
                    {
                        string ss = $"{catsId[i]} not found: {catsWin[i]} or {catsDos[i]} required";
                        throw new Exception(ss);
                    }
                }
            }
            else
            {
                foreach (var soundDef in _soundDefs)
                {
                    string fname = soundDef.Value.getCATFile();
                    fname = fname.ToLower();
                    if (soundFiles.Contains(fname))
                    {
                        if (!_sounds.ContainsKey(soundDef.Key))
                        {
                            _sounds[soundDef.Key] = new SoundSet();
                        }
                        foreach (var soundList in soundDef.Value.getSoundList())
                        {
                            _sounds[soundDef.Key].loadCatbyIndex(FileMap.getFilePath("SOUND/" + fname), soundList);
                        }
                    }
                    else
                    {
                        throw new Exception(fname + " not found");
                    }
                }
            }

            if (soundFiles.Contains("intro.cat"))
            {
                SoundSet s = _sounds["INTRO.CAT"] = new SoundSet();
                s.loadCat(FileMap.getFilePath("SOUND/INTRO.CAT"), false);
            }

            if (soundFiles.Contains("sample3.cat"))
            {
                SoundSet s = _sounds["SAMPLE3.CAT"] = new SoundSet();
                s.loadCat(FileMap.getFilePath("SOUND/SAMPLE3.CAT"), true);
            }
        }

        loadBattlescapeResources(); // TODO load this at battlescape start, unload at battlescape end?

        //update number of shared indexes in surface sets and sound sets
        {
            string[] surfaceNames = { "BIGOBS.PCK", "FLOOROB.PCK", "HANDOB.PCK", "SMOKE.PCK", "HIT.PCK", "BASEBITS.PCK", "INTICON.PCK" };

            for (var i = 0; i < surfaceNames.Length; ++i)
            {
                SurfaceSet s = _sets[surfaceNames[i]];
                s.setMaxSharedFrames((int)s.getTotalFrames());
            }
            //special case for surface set that is loaded later
            {
                SurfaceSet s = _sets["Projectiles"];
                s.setMaxSharedFrames(385);
            }
            {
                SurfaceSet s = _sets["UnderwaterProjectiles"];
                s.setMaxSharedFrames(385);
            }
            {
                SurfaceSet s = _sets["GlobeMarkers"];
                s.setMaxSharedFrames(9);
            }
            //HACK: because of value "hitAnimation" from item that is used as offet in "X1.PCK", this set need have same number of shared frames as "SMOKE.PCK".
            {
                SurfaceSet s = _sets["X1.PCK"];
                s.setMaxSharedFrames((int)_sets["SMOKE.PCK"].getMaxSharedFrames());
            }
        }
        {
            string[] soundNames = { "BATTLE.CAT", "GEO.CAT" };

            for (var i = 0; i < soundNames.Length; ++i)
            {
                SoundSet s = _sounds[soundNames[i]];
                s.setMaxSharedSounds((int)s.getTotalSounds());
            }
            //HACK: case for underwater surface, it should share same offsets as "BATTLE.CAT"
            {
                SoundSet s = _sounds["BATTLE2.CAT"];
                s.setMaxSharedSounds((int)_sounds["BATTLE.CAT"].getTotalSounds());
            }
        }
    }

    /**
     * Loads the extra resources defined in rulesets.
     */
    void loadExtraResources()
    {
        using var input = new StreamReader(FileMap.getFilePath("Language/" + _fontName));
        var yaml = new YamlStream();
        yaml.Load(input);

        // Load fonts
		Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Loading fonts... {_fontName}");
        foreach (var node in ((YamlSequenceNode)yaml.Documents[0].RootNode["fonts"]).Children)
        {
            string id = node["id"].ToString();
            Font font = new Font();
            font.load(node);
            _fonts[id] = font;
        }

#if !__NO_MUSIC
        // Load musics
        if (!Options.mute)
        {
            HashSet<string> soundFiles = FileMap.getVFolderContents("SOUND");

            // Check which music version is available
            CatFile adlibcat = null, aintrocat = null;
            GMCatFile gmcat = null;

            foreach (var soundFile in soundFiles)
            {
                if (soundFile == "adlib.cat")
                {
                    adlibcat = new CatFile(FileMap.getFilePath("SOUND/" + soundFile));
                }
                else if (soundFile == "aintro.cat")
                {
                    aintrocat = new CatFile(FileMap.getFilePath("SOUND/" + soundFile));
                }
                else if (soundFile == "gm.cat")
                {
                    gmcat = new GMCatFile(FileMap.getFilePath("SOUND/" + soundFile));
                }
            }

            // Try the preferred format first, otherwise use the default priority
            MusicFormat[] priority = { Options.preferredMusic, MusicFormat.MUSIC_FLAC, MusicFormat.MUSIC_OGG, MusicFormat.MUSIC_MP3, MusicFormat.MUSIC_MOD, MusicFormat.MUSIC_WAV, MusicFormat.MUSIC_ADLIB, MusicFormat.MUSIC_GM, MusicFormat.MUSIC_MIDI };
            foreach (var musicDef in _musicDefs)
            {
                Music music = null;
                for (var j = 0; j < priority.Length && music == null; ++j)
                {
                    music = loadMusic(priority[j], musicDef.Value, adlibcat, aintrocat, gmcat);
                }
                if (music != null)
                {
                    _musics[musicDef.Key] = music;
                }

            }

            gmcat = null;
            adlibcat = null;
            aintrocat = null;
        }
#endif

        if (!Options.lazyLoadResources)
        {
            Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Loading extra resources from ruleset...");
            foreach (var extraSprite in _extraSprites)
            {
                foreach (var item in extraSprite.Value)
                {
                    loadExtraSprite(item);
                }
            }
        }

        if (!Options.mute)
        {
            foreach (var extraSound in _extraSounds)
            {
                string setName = extraSound.Key;
                ExtraSounds soundPack = extraSound.Value;
                SoundSet set = null;

                if (_sounds.ContainsKey(setName))
                {
                    set = _sounds[setName];
                }
                _sounds[setName] = soundPack.loadSoundSet(set);
            }
        }

        TextButton.soundPress = getSound("GEO.CAT", (uint)BUTTON_PRESS);
        Window.soundPopup[0] = getSound("GEO.CAT", (uint)WINDOW_POPUP[0]);
        Window.soundPopup[1] = getSound("GEO.CAT", (uint)WINDOW_POPUP[1]);
        Window.soundPopup[2] = getSound("GEO.CAT", (uint)WINDOW_POPUP[2]);
    }

    /**
     * Returns a specific sound from the mod.
     * @param set Name of the sound set.
     * @param sound ID of the sound.
     * @return Pointer to the sound.
     */
    internal Sound getSound(string set, uint sound, bool error = true)
    {
	    if (Options.mute)
	    {
		    return _muteSound;
	    }
	    else
	    {
		    SoundSet ss = getSoundSet(set, error);
		    if (ss != null)
		    {
			    Sound s = ss.getSound(sound);
			    if (s == null && error)
			    {
				    string err = $"Sound {sound} in {set} not found";
				    throw new Exception(err);
			    }
			    return s;
		    }
		    else
		    {
			    return null;
		    }
	    }
    }

    /**
     * Returns a specific sound set from the mod.
     * @param name Name of the sound set.
     * @return Pointer to the sound set.
     */
    SoundSet getSoundSet(string name, bool error = true) =>
        getRule(name, "Sound Set", _sounds, error);

    static string[] exts = { string.Empty, ".flac", ".ogg", ".mp3", ".mod", ".wav", string.Empty, string.Empty, ".mid" };
    /**
     * Loads the specified music file format.
     * @param fmt Format of the music.
     * @param rule Parameters of the music.
     * @param adlibcat Pointer to ADLIB.CAT if available.
     * @param aintrocat Pointer to AINTRO.CAT if available.
     * @param gmcat Pointer to GM.CAT if available.
     * @return Pointer to the music file, or NULL if it couldn't be loaded.
     */
    Music loadMusic(MusicFormat fmt, RuleMusic rule, CatFile adlibcat, CatFile aintrocat, GMCatFile gmcat)
    {
	    /* MUSIC_AUTO, MUSIC_FLAC, MUSIC_OGG, MUSIC_MP3, MUSIC_MOD, MUSIC_WAV, MUSIC_ADLIB, MUSIC_GM, MUSIC_MIDI */
	    Music music = null;
	    HashSet<string> soundContents = FileMap.getVFolderContents("SOUND");
	    int track = rule.getCatPos();
	    try
	    {
		    // Try Adlib music
		    if (fmt == MusicFormat.MUSIC_ADLIB)
		    {
			    if (adlibcat != null && Options.audioBitDepth == 16)
			    {
                    music = new AdlibMusic(rule.getNormalization());
				    if (track < adlibcat.getAmount())
				    {
                        music.load(adlibcat.load((uint)track, true), (int)adlibcat.getObjectSize((uint)track));
				    }
				    // separate intro music
				    else if (aintrocat != null)
				    {
					    track -= adlibcat.getAmount();
					    if (track < aintrocat.getAmount())
					    {
                            music.load(aintrocat.load((uint)track, true), (int)aintrocat.getObjectSize((uint)track));
					    }
					    else
					    {
						    music = null;
					    }
				    }
			    }
		    }
		    // Try MIDI music (from GM.CAT)
		    else if (fmt == MusicFormat.MUSIC_GM)
		    {
			    // DOS MIDI
			    if (gmcat != null && track < gmcat.getAmount())
			    {
                    music = gmcat.loadMIDI((uint)track);
			    }
		    }
		    // Try digital tracks
		    else
		    {
                string fname = rule.getName() + exts[(int)fmt];
                fname = fname.ToLower();

			    if (soundContents.Contains(fname))
			    {
				    music = new Music();
                    music.load(FileMap.getFilePath("SOUND/" + fname));
			    }
		    }
	    }
	    catch (Exception e)
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} {e.Message}");
		    if (music != null) music = null;
	    }
	    return music;
    }

    /**
     * Loads the resources required by the Battlescape.
     */
    void loadBattlescapeResources()
    {
        // Load Battlescape ICONS
        _sets["SPICONS.DAT"] = new SurfaceSet(32, 24);
        _sets["SPICONS.DAT"].loadDat(FileMap.getFilePath("UFOGRAPH/SPICONS.DAT"));
        _sets["CURSOR.PCK"] = new SurfaceSet(32, 40);
        _sets["CURSOR.PCK"].loadPck(FileMap.getFilePath("UFOGRAPH/CURSOR.PCK"), FileMap.getFilePath("UFOGRAPH/CURSOR.TAB"));
        _sets["SMOKE.PCK"] = new SurfaceSet(32, 40);
        _sets["SMOKE.PCK"].loadPck(FileMap.getFilePath("UFOGRAPH/SMOKE.PCK"), FileMap.getFilePath("UFOGRAPH/SMOKE.TAB"));
        _sets["HIT.PCK"] = new SurfaceSet(32, 40);
        _sets["HIT.PCK"].loadPck(FileMap.getFilePath("UFOGRAPH/HIT.PCK"), FileMap.getFilePath("UFOGRAPH/HIT.TAB"));
        _sets["X1.PCK"] = new SurfaceSet(128, 64);
        _sets["X1.PCK"].loadPck(FileMap.getFilePath("UFOGRAPH/X1.PCK"), FileMap.getFilePath("UFOGRAPH/X1.TAB"));
        _sets["MEDIBITS.DAT"] = new SurfaceSet(52, 58);
        _sets["MEDIBITS.DAT"].loadDat(FileMap.getFilePath("UFOGRAPH/MEDIBITS.DAT"));
        _sets["DETBLOB.DAT"] = new SurfaceSet(16, 16);
        _sets["DETBLOB.DAT"].loadDat(FileMap.getFilePath("UFOGRAPH/DETBLOB.DAT"));
        _sets["Projectiles"] = new SurfaceSet(3, 3);
        _sets["UnderwaterProjectiles"] = new SurfaceSet(3, 3);

        // Load Battlescape Terrain (only blanks are loaded, others are loaded just in time)
        _sets["BLANKS.PCK"] = new SurfaceSet(32, 40);
        _sets["BLANKS.PCK"].loadPck(FileMap.getFilePath("TERRAIN/BLANKS.PCK"), FileMap.getFilePath("TERRAIN/BLANKS.TAB"));

        // Load Battlescape units
        HashSet<string> unitsContents = FileMap.getVFolderContents("UNITS");
        HashSet<string> usets = FileMap.filterFiles(unitsContents, "PCK");
        foreach (var item in usets)
        {
            string path = FileMap.getFilePath("UNITS/" + item);
            string tab = FileMap.getFilePath("UNITS/" + CrossPlatform.noExt(item) + ".TAB");
            string fname = item;
            fname = fname.ToUpper();
            if (fname != "BIGOBS.PCK")
                _sets[fname] = new SurfaceSet(32, 40);
            else
                _sets[fname] = new SurfaceSet(32, 48);
            _sets[fname].loadPck(path, tab);
        }
        // incomplete chryssalid set: 1.0 data: stop loading.
        if (_sets.TryGetValue("CHRYS.PCK", out var set) && set.getFrame(225) == null)
        {
            Console.WriteLine($"{Log(SeverityLevel.LOG_FATAL)} Version 1.0 data detected");
            throw new Exception("Invalid CHRYS.PCK, please patch your X-COM data to the latest version");
        }
        // TFTD uses the loftemps dat from the terrain folder, but still has enemy unknown's version in the geodata folder, which is short by 2 entries.
        HashSet<string> terrainContents = FileMap.getVFolderContents("TERRAIN");
        if (terrainContents.Contains("loftemps.dat"))
        {
            MapDataSet.loadLOFTEMPS(FileMap.getFilePath("TERRAIN/LOFTEMPS.DAT"), _voxelData);
        }
        else
        {
            MapDataSet.loadLOFTEMPS(FileMap.getFilePath("GEODATA/LOFTEMPS.DAT"), _voxelData);
        }

        string[] scrs = { "TAC00.SCR" };

        for (var i = 0; i < scrs.Length; ++i)
        {
            _surfaces[scrs[i]] = new Surface(320, 200);
            _surfaces[scrs[i]].loadScr(FileMap.getFilePath("UFOGRAPH/" + scrs[i]));
        }

        // lower case so we can find them in the contents map
        string[] lbms = { "d0.lbm", "d1.lbm", "d2.lbm", "d3.lbm" };
        string[] pals = { "PAL_BATTLESCAPE", "PAL_BATTLESCAPE_1", "PAL_BATTLESCAPE_2", "PAL_BATTLESCAPE_3" };

        SDL_Color[] backPal = {
            new() { r = 0, g = 5, b = 4, a = 255 },
            new() { r = 0, g = 10, b = 34, a = 255 },
            new() { r = 2, g = 9, b = 24, a = 255 },
            new() { r = 2, g = 0, b = 24, a = 255 }
        };

        HashSet<string> ufographContents = FileMap.getVFolderContents("UFOGRAPH");
        for (var i = 0; i < lbms.Length; ++i)
        {
            if (!ufographContents.Contains(lbms[i]))
            {
                continue;
            }

            if (i == 0)
            {
                _palettes["PAL_BATTLESCAPE"] = null;
            }

            Surface tempSurface = new Surface(1, 1);
            tempSurface.loadImage(FileMap.getFilePath("UFOGRAPH/" + lbms[i]));
            _palettes[pals[i]] = new Palette();
            SDL_Color[] colors = tempSurface.getPalette();
            colors[255] = backPal[i];
            _palettes[pals[i]].setColors(colors, 256);
            createTransparencyLUT(_palettes[pals[i]]);
            tempSurface = null;
        }

        string[] spks = { "TAC01.SCR", "DETBORD.PCK", "DETBORD2.PCK", "ICONS.PCK", "MEDIBORD.PCK", "SCANBORD.PCK", "UNIBORD.PCK" };

        for (var i = 0; i < spks.Length; ++i)
        {
            string fname = spks[i];
            fname = fname.ToLower();
            if (!ufographContents.Contains(fname))
            {
                continue;
            }

            _surfaces[spks[i]] = new Surface(320, 200);
            _surfaces[spks[i]].loadSpk(FileMap.getFilePath("UFOGRAPH/" + spks[i]));
        }

        HashSet<string> bdys = FileMap.filterFiles(ufographContents, "BDY");
        foreach (var bdy in bdys)
        {
            string idxName = bdy;
            idxName = idxName.ToUpper();
            idxName = idxName.Substring(0, idxName.Length - 3);
            if (idxName.Substring(0, 3) == "MAN")
            {
                idxName = idxName + "SPK";
            }
            else if (idxName == "TAC01.")
            {
                idxName = idxName + "SCR";
            }
            else
            {
                idxName = idxName + "PCK";
            }
            _surfaces[idxName] = new Surface(320, 200);
            _surfaces[idxName].loadBdy(FileMap.getFilePath("UFOGRAPH/" + bdy));
        }

        // Load Battlescape inventory
        HashSet<string> invs = FileMap.filterFiles(ufographContents, "SPK");
        foreach (var inv in invs)
        {
            string fname = inv;
            fname = fname.ToUpper();
            _surfaces[fname] = new Surface(320, 200);
            _surfaces[fname].loadSpk(FileMap.getFilePath("UFOGRAPH/" + fname));
        }

        //"fix" of color index in original solders sprites
        if (Options.battleHairBleach)
        {
            var hairXCOM1 = new HairXCOM1();
            var hairXCOM2 = new HairXCOM2();
            var faceXCOM2 = new FaceXCOM2();
            var fallXCOM2 = new FallXCOM2();
            var bodyXCOM2 = new BodyXCOM2();

            //personal armor
            var name = "XCOM_1.PCK";
            if (_sets.TryGetValue(name, out var xcom_1))
            {
                for (int i = 0; i < 8; ++i)
                {
                    //chest frame
                    Surface surf = xcom_1.getFrame(4 * 8 + i);
                    ShaderMove<byte> head = new ShaderMove<byte>(surf);
                    GraphSubset dim = head.getBaseDomain();
                    surf.@lock();
                    dim.beg_y = 6;
                    dim.end_y = 9;
                    head.setDomain(dim);
                    ShaderDraw(hairXCOM1, head, ShaderScalar<byte>(HairXCOM1.Face + 5));
                    dim.beg_y = 9;
                    dim.end_y = 10;
                    head.setDomain(dim);
                    ShaderDraw(hairXCOM1, head, ShaderScalar<byte>(HairXCOM1.Face + 6));
                    surf.unlock();
                }

                for (int i = 0; i < 3; ++i)
                {
                    //fall frame
                    Surface surf = xcom_1.getFrame(264 + i);
                    ShaderMove<byte> head = new ShaderMove<byte>(surf);
                    GraphSubset dim = head.getBaseDomain();
                    dim.beg_y = 0;
                    dim.end_y = 24;
                    dim.beg_x = 11;
                    dim.end_x = 20;
                    head.setDomain(dim);
                    surf.@lock();
                    ShaderDraw(hairXCOM1, head, ShaderScalar<byte>(HairXCOM1.Face + 6));
                    surf.unlock();
                }
            }

            //all TFTD armors
            name = "TDXCOM_?.PCK";
            for (int j = 0; j < 3; ++j)
            {
                name.Replace("?", $"0{j}");
                if (_sets.TryGetValue(name, out var xcom_2))
                {
                    for (int i = 0; i < 16; ++i)
                    {
                        //chest frame without helm
                        Surface surf = xcom_2.getFrame(262 + i);
                        surf.@lock();
                        if (i < 8)
                        {
                            //female chest frame
                            ShaderMove<byte> head = new ShaderMove<byte>(surf);
                            GraphSubset dim = head.getBaseDomain();
                            dim.beg_y = 6;
                            dim.end_y = 18;
                            head.setDomain(dim);
                            ShaderDraw(hairXCOM2, head);

                            if (j == 2)
                            {
                                //fix some pixels in ION armor that was overwrite by previous function
                                if (i == 0)
                                {
                                    surf.setPixel(18, 14, 16);
                                }
                                else if (i == 3)
                                {
                                    surf.setPixel(19, 12, 20);
                                }
                                else if (i == 6)
                                {
                                    surf.setPixel(13, 14, 16);
                                }
                            }
                        }

                        //we change face to pink, to prevent mixup with ION armor backpack that have same color group.
                        ShaderDraw(faceXCOM2, new ShaderMove<byte>(surf));
                        surf.unlock();
                    }

                    for (int i = 0; i < 2; ++i)
                    {
                        //fall frame (first and second)
                        Surface surf = xcom_2.getFrame(256 + i);
                        surf.@lock();

                        ShaderMove<byte> head = new ShaderMove<byte>(surf);
                        GraphSubset dim = head.getBaseDomain();
                        dim.beg_y = 0;
                        if (j == 3)
                        {
                            dim.end_y = 11 + 5 * i;
                        }
                        else
                        {
                            dim.end_y = 17;
                        }
                        head.setDomain(dim);
                        ShaderDraw(fallXCOM2, head);

                        //we change face to pink, to prevent mixup with ION armor backpack that have same color group.
                        ShaderDraw(faceXCOM2, new ShaderMove<byte>(surf));
                        surf.unlock();
                    }

                    //Palette fix for ION armor
                    if (j == 2)
                    {
                        int size = (int)xcom_2.getTotalFrames();
                        for (int i = 0; i < size; ++i)
                        {
                            Surface surf = xcom_2.getFrame(i);
                            surf.@lock();
                            ShaderDraw(bodyXCOM2, new ShaderMove<byte>(surf));
                            surf.unlock();
                        }
                    }
                }
            }
        }
    }

    /**
     * Preamble:
     * this is the most horrible function i've ever written, and it makes me sad.
     * this is, however, a necessary evil, in order to save massive amounts of time in the draw function.
     * when used with the default TFTD mod, this function loops 4,194,304 times
     * (4 palettes, 4 tints, 4 levels of opacity, 256 colors, 256 comparisons per)
     * each additional tint in the rulesets will result in over a million iterations more.
     * @param pal the palette to base the lookup table on.
     */
    void createTransparencyLUT(Palette pal)
    {
        const int opacityMax = 4;
        Span<SDL_Color> palColors = pal.getColors(0);
        List<byte> lookUpTable;
        // start with the color sets
        lookUpTable = new List<byte>(_transparencies.Count * 256 * opacityMax);
        foreach (var tint in _transparencies)
        {
            // then the opacity levels, using the alpha channel as the step
            for (int opacity = 1; opacity <= opacityMax; ++opacity)
            {
                // pseudo interpolation of palette color with tint
                // for small values `op` its should behave same as original TFTD
                // but for bigger values it make result closer to tint color
                int op = Math.Clamp(opacity * tint.a, 0, 64);
                float co = (float)(1.0f - Math.Sqrt(op / 64.0f)); // 1.0 -> 0.0
                float to = op * 1.0f; // 0.0 -> 64.0

                // then the palette itself
                for (int currentColor = 0; currentColor < 256; ++currentColor)
                {
                    SDL_Color desiredColor;

                    desiredColor.r = (byte)Math.Min(255, (int)Round((palColors[currentColor].r * co) + (tint.r * to)));
                    desiredColor.g = (byte)Math.Min(255, (int)Round((palColors[currentColor].g * co) + (tint.g * to)));
                    desiredColor.b = (byte)Math.Min(255, (int)Round((palColors[currentColor].b * co) + (tint.b * to)));

                    byte closest = (byte)currentColor;
                    int lowestDifference = int.MaxValue;
                    // if opacity is zero then we stay with current color, transparet color will stay same too
                    if (op != 0 && currentColor != 0)
                    {
                        // now compare each color in the palette to find the closest match to our desired one
                        for (int comparator = 1; comparator < 256; ++comparator)
                        {
                            int currentDifference = (int)(Math.Sqrt(desiredColor.r - palColors[comparator].r) +
                                Math.Sqrt(desiredColor.g - palColors[comparator].g) +
                                Math.Sqrt(desiredColor.b - palColors[comparator].b));

                            if (currentDifference < lowestDifference)
                            {
                                closest = (byte)comparator;
                                lowestDifference = currentDifference;
                            }
                        }
                    }
                    lookUpTable.Add(closest);
                }
            }
        }
        _transparencyLUTs.Add(lookUpTable);
    }

    /**
     * Loads a ruleset from a YAML file that have basic resources configuration.
     * @param filename YAML filename.
     */
    void loadResourceConfigFile(string filename)
    {
        using var input = new StreamReader(filename);
        var yaml = new YamlStream();
        yaml.Load(input);
        var doc = yaml.Documents[0].RootNode;

	    foreach (var node in ((YamlSequenceNode)doc["soundDefs"]).Children)
	    {
            SoundDefinition rule = loadRule(node, _soundDefs);
		    if (rule != null)
		    {
                rule.load(node);
		    }
	    }

        var luts = doc["transparencyLUTs"];
        if (luts != null)
	    {
		    uint start = _modCurrent.offset / ModTransparceySizeReduction;
		    uint limit =  _modCurrent.size / ModTransparceySizeReduction;
            uint curr = 0;

            _transparencies = new List<SDL_Color>((int)(start + limit));
		    foreach (var i in ((YamlSequenceNode)luts).Children)
		    {
			    var c = i["colors"];
			    if (c is YamlSequenceNode seq)
			    {
				    foreach (var j in seq.Children)
				    {
					    if (curr == limit)
					    {
						    throw new Exception("transparencyLUTs mod limit reach");
					    }
					    SDL_Color color;
					    color.r = (byte)int.Parse(j[0].ToString());
                        color.g = (byte)int.Parse(j[1].ToString());
					    color.b = (byte)int.Parse(j[2].ToString());
					    color.a = (byte)(j[3] != null ? int.Parse(j[3].ToString()) : 2);
                        // technically its breaking change as it always overwritte from offset `start + 0` but no two mods could work correctly before this change.
                        _transparencies[(int)(start + curr++)] = color;
				    }
			    }
			    else
			    {
				    throw new Exception("unknown transparencyLUTs node type");
			    }
		    }
	    }
    }

    /**
     * Loads a list of rulesets from YAML files for the mod at the specified index. The first
     * mod loaded should be the master at index 0, then 1, and so on.
     * @param rulesetFiles List of rulesets to load.
     */
    void loadMod(List<string> rulesetFiles)
    {
	    foreach (var rulesetFile in rulesetFiles)
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)} - {rulesetFile}");
		    try
		    {
			    loadFile(rulesetFile);
		    }
		    catch (YamlException e)
		    {
                throw new Exception(rulesetFile + ": " + e.Message);
		    }
	    }

	    // these need to be validated, otherwise we're gonna get into some serious trouble down the line.
	    // it may seem like a somewhat arbitrary limitation, but there is a good reason behind it.
	    // i'd need to know what results are going to be before they are formulated, and there's a hierarchical structure to
	    // the order in which variables are determined for a mission, and the order is DIFFERENT for regular missions vs
	    // missions that spawn a mission site. where normally we pick a region, then a mission based on the weights for that region.
	    // a terror-type mission picks a mission type FIRST, then a region based on the criteria defined by the mission.
	    // there is no way i can conceive of to reconcile this difference to allow mixing and matching,
	    // short of knowing the results of calls to the RNG before they're determined.
	    // the best solution i can come up with is to disallow it, as there are other ways to achieve what this would amount to anyway,
	    // and they don't require time travel. - Warboy
	    foreach (var missionScript in _missionScripts)
	    {
		    RuleMissionScript rule = missionScript.Value;
            HashSet<string> missions = rule.getAllMissionTypes();
		    if (missions.Any())
		    {
                foreach (var mission in missions)
                {
                    if (getAlienMission(mission) == null)
                    {
                        throw new Exception("Error with MissionScript: " + missionScript.Key + ": alien mission type: " + mission + " not defined, do not incite the judgement of Amaunator.");
                    }
                    bool isSiteType = getAlienMission(mission).getObjective() == MissionObjective.OBJECTIVE_SITE;
                    rule.setSiteType(isSiteType);
                    if (getAlienMission(mission) != null && (getAlienMission(mission).getObjective() == MissionObjective.OBJECTIVE_SITE) != isSiteType)
                    {
                        throw new Exception("Error with MissionScript: " + missionScript.Key + ": cannot mix terror/non-terror missions in a single command, so sayeth the wise Alaundo.");
                    }
			    }
		    }
	    }

	    // instead of passing a pointer to the region load function and moving the alienMission loading before region loading
	    // and sanitizing there, i'll sanitize here, i'm sure this sanitation will grow, and will need to be refactored into
	    // its own function at some point, but for now, i'll put it here next to the missionScript sanitation, because it seems
	    // the logical place for it, given that this sanitation is required as a result of moving all terror mission handling
	    // into missionScripting behaviour. apologies to all the modders that will be getting errors and need to adjust their
	    // rulesets, but this will save you weird errors down the line.
	    foreach (var region in _regions)
	    {
		    // bleh, make copies, const correctness kinda screwed me here.
		    WeightedOptions weights = region.Value.getAvailableMissions();
		    List<string> names = weights.getNames();
		    foreach (var name in names)
		    {
			    if (getAlienMission(name) == null)
			    {
				    throw new Exception("Error with MissionWeights: Region: " + region.Key + ": alien mission type: " + name + " not defined, do not incite the judgement of Amaunator.");
			    }
			    if (getAlienMission(name).getObjective() == MissionObjective.OBJECTIVE_SITE)
			    {
				    throw new Exception("Error with MissionWeights: Region: " + region.Key + " has " + name + " listed. Terror mission can only be invoked via missionScript, so sayeth the Spider Queen.");
			    }
		    }
	    }
    }

    /**
     * Returns the rules for the specified alien mission.
     * @param id Alien mission type.
     * @return Rules for the alien mission.
     */
    internal RuleAlienMission getAlienMission(string id, bool error = false) =>
        getRule(id, "Alien Mission", _alienMissions, error);

    /**
     * Loads a rule element, adding/removing from vectors as necessary.
     * @param node YAML node.
     * @param map Map associated to the rule type.
     * @param index Index vector for the rule type.
     * @param key Rule key name.
     * @return Pointer to new rule if one was created, or NULL if one was removed.
     */
    T loadRule<T>(YamlNode node, Dictionary<string, T> map, List<string> index = null, string key = "type") where T : IRule
    {
        T rule = default;
	    if (node[key] != null)
	    {
		    string type = node[key].ToString();
		    if (map.TryGetValue(type, out var value))
		    {
			    rule = value;
		    }
		    else
		    {
                rule = (T)rule.Create(type);
			    map[type] = rule;
			    if (index != null)
			    {
				    index.Add(type);
			    }
		    }
	    }
	    else if (node["delete"] != null)
	    {
		    string type = node["delete"].ToString();
		    if (map.ContainsKey(type))
		    {
			    map.Remove(type);
		    }
		    if (index != null)
		    {
                index.Remove(type);
		    }
	    }
	    return rule;
    }

    /**
     * Loads a ruleset's contents from a YAML file.
     * Rules that match pre-existing rules overwrite them.
     * @param filename YAML filename.
     */
    void loadFile(string filename)
    {
        using var input = new StreamReader(filename);
        var yaml = new YamlStream();
        yaml.Load(input);
        YamlNode doc = yaml.Documents[0].RootNode;

        foreach (var i in ((YamlSequenceNode)doc["countries"]).Children)
	    {
		    RuleCountry rule = loadRule(i, _countries, _countriesIndex);
		    if (rule != null)
		    {
                rule.load(i);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["regions"]).Children)
	    {
            RuleRegion rule = loadRule(i, _regions, _regionsIndex);
		    if (rule != null)
		    {
                rule.load(i);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["facilities"]).Children)
	    {
		    RuleBaseFacility rule = loadRule(i, _facilities, _facilitiesIndex);
		    if (rule != null)
		    {
			    _facilityListOrder += 100;
                rule.load(i, this, _facilityListOrder);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["crafts"]).Children)
	    {
		    RuleCraft rule = loadRule(i, _crafts, _craftsIndex);
		    if (rule != null)
		    {
			    _craftListOrder += 100;
                rule.load(i, this, _craftListOrder);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["craftWeapons"]).Children)
	    {
		    RuleCraftWeapon rule = loadRule(i, _craftWeapons, _craftWeaponsIndex);
		    if (rule != null)
		    {
                rule.load(i, this);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["items"]).Children)
	    {
		    RuleItem rule = loadRule(i, _items, _itemsIndex);
		    if (rule != null)
		    {
			    _itemListOrder += 100;
                rule.load(i, this, _itemListOrder);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["ufos"]).Children)
	    {
		    RuleUfo rule = loadRule(i, _ufos, _ufosIndex);
		    if (rule != null)
		    {
                rule.load(i, this);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["invs"]).Children)
	    {
		    RuleInventory rule = loadRule(i, _invs, _invsIndex, "id");
		    if (rule != null)
		    {
			    _invListOrder += 10;
                rule.load(i, _invListOrder);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["terrains"]).Children)
	    {
            RuleTerrain rule = loadRule(i, _terrains, _terrainIndex, "name");
		    if (rule != null)
		    {
                rule.load(i, this);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["armors"]).Children)
	    {
		    Armor rule = loadRule(i, _armors, _armorsIndex);
		    if (rule != null)
		    {
			    rule.load(i);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["soldiers"]).Children)
	    {
		    RuleSoldier rule = loadRule(i, _soldiers, _soldiersIndex);
		    if (rule != null)
		    {
			    rule.load(i, this);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["units"]).Children)
	    {
		    Unit rule = loadRule(i, _units);
		    if (rule != null)
		    {
                rule.load(i, this);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["alienRaces"]).Children)
	    {
            AlienRace rule = loadRule(i, _alienRaces, _aliensIndex, "id");
		    if (rule != null)
		    {
                rule.load(i);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["alienDeployments"]).Children)
	    {
		    AlienDeployment rule = loadRule(i, _alienDeployments, _deploymentsIndex);
		    if (rule != null)
		    {
			    rule.load(i, this);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["research"]).Children)
	    {
		    RuleResearch rule = loadRule(i, _research, _researchIndex, "name");
		    if (rule != null)
		    {
			    _researchListOrder += 100;
			    rule.load(i, _researchListOrder);
			    if (bool.Parse(i["unlockFinalMission"].ToString()))
                {
				    _finalResearch = i["name"].ToString();
			    }
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["manufacture"]).Children)
	    {
		    RuleManufacture rule = loadRule(i, _manufacture, _manufactureIndex, "name");
		    if (rule != null)
		    {
			    _manufactureListOrder += 100;
			    rule.load(i, _manufactureListOrder);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["ufopaedia"]).Children)
	    {
		    if (i["id"] != null)
		    {
			    string id = i["id"].ToString();
			    ArticleDefinition rule;
			    if (_ufopaediaArticles.TryGetValue(id, out var value))
			    {
				    rule = value;
			    }
			    else
			    {
                    UfopaediaTypeId type = (UfopaediaTypeId)int.Parse(i["type_id"].ToString());
				    switch (type)
				    {
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_CRAFT: rule = new ArticleDefinitionCraft(); break;
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_CRAFT_WEAPON: rule = new ArticleDefinitionCraftWeapon(); break;
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_VEHICLE: rule = new ArticleDefinitionVehicle(); break;
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_ITEM: rule = new ArticleDefinitionItem(); break;
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_ARMOR: rule = new ArticleDefinitionArmor(); break;
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_BASE_FACILITY: rule = new ArticleDefinitionBaseFacility(); break;
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_TEXTIMAGE: rule = new ArticleDefinitionTextImage(); break;
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_TEXT: rule = new ArticleDefinitionText(); break;
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_UFO: rule = new ArticleDefinitionUfo(); break;
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD:
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD_CRAFT:
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD_CRAFT_WEAPON:
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD_VEHICLE:
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD_ITEM:
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD_ARMOR:
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD_BASE_FACILITY:
				        case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD_USO:
                            rule = new ArticleDefinitionTFTD();
					        break;
				        default: rule = null; break;
				    }
				    _ufopaediaArticles[id] = rule;
                    _ufopaediaIndex.Add(id);
			    }
			    _ufopaediaListOrder += 100;
			    rule.load(i, _ufopaediaListOrder);
			    if (rule.section != Ufopaedia.Ufopaedia.UFOPAEDIA_NOT_AVAILABLE)
			    {
				    if (!_ufopaediaSections.ContainsKey(rule.section))
				    {
					    _ufopaediaSections[rule.section] = rule.getListOrder();
					    _ufopaediaCatIndex.Add(rule.section);
				    }
				    else
				    {
					    _ufopaediaSections[rule.section] = Math.Min(_ufopaediaSections[rule.section], rule.getListOrder());
				    }
			    }
		    }
		    else if (i["delete"] != null)
		    {
			    string type = i["delete"].ToString();
			    if (_ufopaediaArticles.ContainsKey(type))
			    {
				    _ufopaediaArticles.Remove(type);
			    }
                if (_ufopaediaIndex.Contains(type))
			    {
				    _ufopaediaIndex.Remove(type);
			    }
		    }
	    }
        // Bases can't be copied, so for savegame purposes we store the node instead
        YamlMappingNode @base = (YamlMappingNode)doc["startingBase"];
	    if (@base != null)
	    {
		    foreach (var i in @base.Children)
		    {
                _startingBase = new YamlMappingNode([i.Key.ToString(), i.Value]);
		    }
	    }
	    if (doc["startingTime"] != null)
	    {
            _startingTime.load(doc["startingTime"]);
	    }
	    _costEngineer = int.Parse(doc["costEngineer"].ToString());
	    _costScientist = int.Parse(doc["costScientist"].ToString());
	    _timePersonnel = int.Parse(doc["timePersonnel"].ToString());
	    _initialFunding = int.Parse(doc["initialFunding"].ToString());
        _alienFuel = KeyValuePair.Create(doc["alienFuel"][0].ToString(), int.Parse(doc["alienFuel"][1].ToString()));
	    _fontName = doc["fontName"].ToString();
	    _turnAIUseGrenade = int.Parse(doc["turnAIUseGrenade"].ToString());
	    _turnAIUseBlaster = int.Parse(doc["turnAIUseBlaster"].ToString());
	    _defeatScore = int.Parse(doc["defeatScore"].ToString());
	    _defeatFunds = int.Parse(doc["defeatFunds"].ToString());
	    _difficultyDemigod = bool.Parse(doc["difficultyDemigod"].ToString());
	    if (doc["difficultyCoefficient"] != null)
	    {
		    var num = 0;
		    for (var i = 0; i < ((YamlSequenceNode)doc["difficultyCoefficient"]).Children.Count && num < 5; ++i)
		    {
			    DIFFICULTY_COEFFICIENT[num] = int.Parse(((YamlSequenceNode)doc["difficultyCoefficient"]).Children[i].ToString());
			    _statAdjustment[num].growthMultiplier = DIFFICULTY_COEFFICIENT[num];
			    ++num;
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["ufoTrajectories"]).Children)
	    {
		    UfoTrajectory rule = loadRule(i, _ufoTrajectories, null, "id");
		    if (rule != null)
		    {
                rule.load(i);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["alienMissions"]).Children)
	    {
            RuleAlienMission rule = loadRule(i, _alienMissions, _alienMissionsIndex);
		    if (rule != null)
		    {
			    rule.load(i);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["alienItemLevels"]).Children)
        {
            _alienItemLevels.Add(((YamlSequenceNode)i).Children.Select(x => int.Parse(x.ToString())).ToList());
        }
	    foreach (var i in ((YamlSequenceNode)doc["MCDPatches"]).Children)
	    {
		    string type = i["type"].ToString();
		    if (_MCDPatches.ContainsKey(type))
		    {
			    _MCDPatches[type].load(i);
		    }
		    else
		    {
			    MCDPatch patch = new MCDPatch();
			    patch.load(i);
			    _MCDPatches[type] = patch;
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["extraSprites"]).Children)
	    {
		    if (i["type"] != null)
		    {
			    string type = i["type"].ToString();
                ExtraSprites extraSprites = new ExtraSprites();
			    ModData data = _modCurrent;
			    // doesn't support modIndex
			    if (type == "TEXTURE.DAT")
				    data = _modData[0];
                extraSprites.load(i, data);
			    _extraSprites[type].Add(extraSprites);
		    }
		    else if (i["delete"] != null)
		    {
			    string type = i["delete"].ToString();
			    if (_extraSprites.ContainsKey(type))
			    {
				    _extraSprites.Remove(type);
			    }
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["extraSounds"]).Children)
	    {
		    string type = i["type"].ToString();
		    ExtraSounds extraSounds = new ExtraSounds();
            extraSounds.load(i, _modCurrent);
		    _extraSounds.Add(KeyValuePair.Create(type, extraSounds));
	    }
	    foreach (var i in ((YamlSequenceNode)doc["extraStrings"]).Children)
	    {
		    string type = i["type"].ToString();
		    if (_extraStrings.ContainsKey(type))
		    {
			    _extraStrings[type].load(i);
		    }
		    else
		    {
			    ExtraStrings extraStrings = new ExtraStrings();
                extraStrings.load(i);
			    _extraStrings[type] = extraStrings;
		    }
	    }

	    foreach (var i in ((YamlSequenceNode)doc["statStrings"]).Children)
	    {
		    StatString statString = new StatString();
            statString.load(i);
		    _statStrings.Add(statString);
	    }

	    foreach (var i in ((YamlSequenceNode)doc["interfaces"]).Children)
	    {
            RuleInterface rule = loadRule(i, _interfaces);
		    if (rule != null)
		    {
			    rule.load(i);
		    }
	    }
	    if (doc["globe"] != null)
	    {
		    _globe.load(doc["globe"]);
	    }
	    if (doc["converter"] != null)
	    {
		    _converter.load(doc["converter"]);
	    }
	    if (doc["constants"] is YamlNode constants)
	    {
		    //backward compatibility version
		    if (constants is YamlSequenceNode seq)
		    {
			    foreach (var i in seq.Children)
			    {
				    loadConstants(i);
			    }
		    }
		    else
		    {
			    loadConstants(constants);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["mapScripts"]).Children)
	    {
		    string type = i["type"].ToString();
		    if (i["delete"] != null)
		    {
			    type = i["delete"].ToString();
		    }
		    if (_mapScripts.ContainsKey(type))
		    {
                _mapScripts[type].Clear();
		    }
		    foreach (var j in ((YamlSequenceNode)i["commands"]).Children)
		    {
                MapScript mapScript = new MapScript();
                mapScript.load(j);
			    _mapScripts[type].Add(mapScript);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["missionScripts"]).Children)
	    {
		    RuleMissionScript rule = loadRule(i, _missionScripts, _missionScriptIndex, "type");
		    if (rule != null)
		    {
                rule.load(i);
		    }
	    }

	    // refresh _psiRequirements for psiStrengthEval
	    foreach (var i in _facilitiesIndex)
	    {
            RuleBaseFacility rule = getBaseFacility(i);
		    if (rule.getPsiLaboratories() > 0)
		    {
                _psiRequirements = rule.getRequirements();
                break;
		    }
	    }

	    foreach (var i in ((YamlSequenceNode)doc["cutscenes"]).Children)
	    {
		    RuleVideo rule = loadRule(i, _videos);
		    if (rule != null)
		    {
                rule.load(i);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["musics"]).Children)
	    {
		    RuleMusic rule = loadRule(i, _musicDefs);
		    if (rule != null)
		    {
			    rule.load(i);
		    }
	    }
	    foreach (var i in ((YamlSequenceNode)doc["commendations"]).Children)
	    {
		    string type = i["type"].ToString();
		    RuleCommendations commendations = new RuleCommendations();
            commendations.load(i);
		    _commendations[type] = commendations;
	    }
	    var count = 0;
	    for (var i = 0; i < ((YamlSequenceNode)doc["aimAndArmorMultipliers"]).Children.Count && count < 5; ++i)
	    {
		    _statAdjustment[count].aimAndArmorMultiplier = double.Parse(((YamlSequenceNode)doc["aimAndArmorMultipliers"]).Children[i].ToString());
		    ++count;
	    }
	    if (doc["statGrowthMultipliers"] != null)
	    {
            _statAdjustment[0].statGrowth = UnitStats.decode(doc["statGrowthMultipliers"]);
		    for (var i = 1; i != 5; ++i)
		    {
                _statAdjustment[i].statGrowth = _statAdjustment[0].statGrowth;
		    }
	    }
    }

    /**
     * Returns the appropriate mod-based offset for a sound.
     * If the ID is bigger than the soundset contents, the mod offset is applied.
     * @param parent Name of parent node, used for better error message
     * @param sound Member to load new sound ID index.
     * @param node Node with data
     * @param set Name of the soundset to lookup.
     */
    internal void loadSoundOffset(string parent, int sound, YamlNode node, string set)
    {
	    if (node != null)
	    {
		    loadOffsetNode(parent, sound, node, getSoundSet(set).getMaxSharedSounds(), set, 1);
	    }
    }

    /**
     * Gets the mod offset array for a certain sound.
     * @param parent Name of parent node, used for better error message
     * @param sounds Member to load new list of sound ID indexes.
     * @param node Node with data
     * @param set Name of the soundset to lookup.
     */
    internal void loadSoundOffset(string parent, List<int> sounds, YamlNode node, string set)
    {
	    if (node != null)
	    {
		    int maxShared = getSoundSet(set).getMaxSharedSounds();
		    sounds.Clear();
		    if (node.NodeType == YamlNodeType.Sequence)
		    {
			    foreach (var item in ((YamlSequenceNode)node).Children)
			    {
				    sounds.Add(-1);
				    loadOffsetNode(parent, sounds.Last(), item, maxShared, set, 1);
			    }
		    }
		    else
		    {
			    sounds.Add(-1);
			    loadOffsetNode(parent, sounds.Last(), node, maxShared, set, 1);
		    }
	    }
    }

    /**
     * Returns the appropriate mod-based offset for a sprite.
     * If the ID is bigger than the surfaceset contents, the mod offset is applied.
     * @param parent Name of parent node, used for better error message
     * @param sprite Member to load new sprite ID index.
     * @param node Node with data
     * @param set Name of the surfaceset to lookup.
     * @param multiplier Value used by `projectile` surface set to convert projectile offset to index offset in surface.
     */
    internal void loadSpriteOffset(string parent, int sprite, YamlNode node, string set, uint multiplier = 1)
    {
	    if (node != null)
	    {
            loadOffsetNode(parent, sprite, node, getRule(set, "Sprite Set", _sets, true).getMaxSharedFrames(), set, multiplier);
	    }
    }

    /**
     * Gets the mod offset array for a certain transparency index.
     * @param parent Name of parent node, used for better error message.
     * @param index Member to load new transparency index.
     * @param node Node with data.
     */
    internal void loadTransparencyOffset(string parent, int index, YamlNode node)
    {
	    if (node != null)
	    {
            loadOffsetNode(parent, index, node, 0, "TransparencyLUTs", 1, ModTransparceySizeReduction);
	    }
    }

    /**
     * Returns the appropriate mod-based offset for a generic ID.
     * If the ID is bigger than the max, the mod offset is applied.
     * @param id Numeric ID.
     * @param max Maximum vanilla value.
     */
    internal int getOffset(int id, int max)
    {
	    Debug.Assert(_modCurrent != null);
        if (id > max)
            return (int)(id + _modCurrent.offset);
        else
            return id;
    }

    /**
     * Returns the info about a specific map data file.
     * @param name Datafile name.
     * @return Rules for the datafile.
     */
    internal MapDataSet getMapDataSet(string name)
    {
	    if (!_mapDataSets.ContainsKey(name))
	    {
		    MapDataSet set = new MapDataSet(name);
		    _mapDataSets[name] = set;
		    return set;
	    }
	    else
	    {
		    return _mapDataSets[name];
	    }
    }

    /**
     * Get offset and index for sound set or sprite set.
     * @param parent Name of parent node, used for better error message
     * @param offset Member to load new value.
     * @param node Node with data
     * @param shared Max offset limit that is shared for every mod
     * @param multiplier Value used by `projectile` surface set to convert projectile offset to index offset in surface.
     * @param sizeScale Value used by transparency colors, reduce total number of avaialbe space for offset.
     */
    void loadOffsetNode(string parent, int offset, YamlNode node, int shared, string set, uint multiplier, uint sizeScale = 1)
    {
        Debug.Assert(_modCurrent != null);
        ModData curr = _modCurrent;
	    if (node.NodeType == YamlNodeType.Scalar)
	    {
		    offset = int.Parse(node.ToString());
	    }
	    else if (node.NodeType == YamlNodeType.Mapping)
	    {
		    offset = int.Parse(node["index"].ToString());
		    string mod = node["mod"].ToString();
		    if (mod == ModNameMaster)
		    {
			    curr = _modData[0];
		    }
		    else if (mod == ModNameCurrent)
		    {
			    //nothing
		    }
		    else
		    {
                ModData n = null;
			    for (var i = 0; i < _modData.Count; ++i)
			    {
				    ModData d = _modData[i];
				    if (d.name == mod)
				    {
					    n = d;
                        break;
				    }
			    }

			    if (n != null)
			    {
				    curr = n;
			    }
			    else
			    {
				    string err = $"Error for '{parent}': unknown mod '{mod}' used";
				    throw new Exception(err);
			    }
		    }
	    }

	    if (offset < -1)
	    {
		    string err = $"Error for '{parent}': offset '{offset}' has incorrect value in set '{set}' at line {node.Start.Line}";
		    throw new Exception(err);
	    }
	    else if (offset == -1)
	    {
		    //ok
	    }
	    else
	    {
		    int f = offset;
		    f = (int)(f * multiplier);
		    if ((uint)f > curr.size / sizeScale)
		    {
			    string err = $"Error for '{parent}': offset '{offset}' exceeds mod size limit {(curr.size / multiplier / sizeScale)} in set '{set}'";
			    throw new Exception(err);
		    }
		    if (f >= shared)
			    f = (int)(f + curr.offset / sizeScale);
            offset = f;
	    }
    }

    /**
     * Loads "constants" node.
     */
    void loadConstants(YamlNode node)
    {
	    loadSoundOffset("constants", DOOR_OPEN, node["doorSound"], "BATTLE.CAT");
	    loadSoundOffset("constants", SLIDING_DOOR_OPEN, node["slidingDoorSound"], "BATTLE.CAT");
	    loadSoundOffset("constants", SLIDING_DOOR_CLOSE, node["slidingDoorClose"], "BATTLE.CAT");
	    loadSoundOffset("constants", SMALL_EXPLOSION, node["smallExplosion"], "BATTLE.CAT");
	    loadSoundOffset("constants", LARGE_EXPLOSION, node["largeExplosion"], "BATTLE.CAT");

	    loadSpriteOffset("constants", EXPLOSION_OFFSET, node["explosionOffset"], "X1.PCK");
	    loadSpriteOffset("constants", SMOKE_OFFSET, node["smokeOffset"], "SMOKE.PCK");
	    loadSpriteOffset("constants", UNDERWATER_SMOKE_OFFSET, node["underwaterSmokeOffset"], "SMOKE.PCK");

	    loadSoundOffset("constants", ITEM_DROP, node["itemDrop"], "BATTLE.CAT");
	    loadSoundOffset("constants", ITEM_THROW, node["itemThrow"], "BATTLE.CAT");
	    loadSoundOffset("constants", ITEM_RELOAD, node["itemReload"], "BATTLE.CAT");
	    loadSoundOffset("constants", WALK_OFFSET, node["walkOffset"], "BATTLE.CAT");
	    loadSoundOffset("constants", FLYING_SOUND, node["flyingSound"], "BATTLE.CAT");

	    loadSoundOffset("constants", BUTTON_PRESS, node["buttonPress"], "GEO.CAT");
	    if (node["windowPopup"] != null)
	    {
            var k = 0;
		    for (var j = 0; j < ((YamlSequenceNode)node["windowPopup"]).Children.Count && k < 3; ++j, ++k)
		    {
			    loadSoundOffset("constants", WINDOW_POPUP[k], ((YamlSequenceNode)node["windowPopup"]).Children[j], "GEO.CAT");
		    }
	    }
	    loadSoundOffset("constants", UFO_FIRE, node["ufoFire"], "GEO.CAT");
	    loadSoundOffset("constants", UFO_HIT, node["ufoHit"], "GEO.CAT");
	    loadSoundOffset("constants", UFO_CRASH, node["ufoCrash"], "GEO.CAT");
	    loadSoundOffset("constants", UFO_EXPLODE, node["ufoExplode"], "GEO.CAT");
	    loadSoundOffset("constants", INTERCEPTOR_HIT, node["interceptorHit"], "GEO.CAT");
	    loadSoundOffset("constants", INTERCEPTOR_EXPLODE, node["interceptorExplode"], "GEO.CAT");
	    GEOSCAPE_CURSOR = int.Parse(node["geoscapeCursor"].ToString());
	    BASESCAPE_CURSOR = int.Parse(node["basescapeCursor"].ToString());
	    BATTLESCAPE_CURSOR = int.Parse(node["battlescapeCursor"].ToString());
	    UFOPAEDIA_CURSOR = int.Parse(node["ufopaediaCursor"].ToString());
	    GRAPHS_CURSOR = int.Parse(node["graphsCursor"].ToString());
	    DAMAGE_RANGE = int.Parse(node["damageRange"].ToString());
        EXPLOSIVE_DAMAGE_RANGE = int.Parse(node["explosiveDamageRange"].ToString());
        var num = 0;
	    for (var j = 0; j < ((YamlSequenceNode)node["fireDamageRange"]).Children.Count && num < 2; ++j)
	    {
		    FIRE_DAMAGE_RANGE[num] = int.Parse(((YamlSequenceNode)node["fireDamageRange"]).Children[j].ToString());
            ++num;
	    }
	    DEBRIEF_MUSIC_GOOD = node["goodDebriefingMusic"].ToString();
        DEBRIEF_MUSIC_BAD = node["badDebriefingMusic"].ToString();
    }

    /**
    * Gets the rules for the Save Converter.
    * @return Pointer to converter rules.
    */
    internal RuleConverter getConverter() =>
	    _converter;

    /**
     * Gets an MCDPatch.
     * @param id The ID of the MCDPatch we want.
     * @return The MCDPatch based on ID, or 0 if none defined.
     */
    internal MCDPatch getMCDPatch(string id) =>
        _MCDPatches.TryGetValue(id, out var patch) ? patch : null;

    /**
     * Returns the list of voxeldata in the mod.
     * @return Pointer to the list of voxeldata.
     */
    internal List<ushort> getVoxelData() =>
        _voxelData;

    /**
     * Returns the rules for the specified country.
     * @param id Country type.
     * @return Rules for the country.
     */
    internal RuleCountry getCountry(string id, bool error = false) =>
	    getRule(id, "Country", _countries, error);

    /**
     * Returns the rules for the specified region.
     * @param id Region type.
     * @return Rules for the region.
     */
    internal RuleRegion getRegion(string id, bool error = false) =>
	    getRule(id, "Region", _regions, error);

    /**
     * Gets the defined starting time.
     * @return The time the game starts in.
     */
    internal GameTime getStartingTime() =>
	    _startingTime;

    /**
     * Returns the info about a specific deployment.
     * @param name Deployment name.
     * @return Rules for the deployment.
     */
    internal AlienDeployment getDeployment(string name, bool error = false) =>
	    getRule(name, "Alien Deployment", _alienDeployments, error);

    /**
     * Returns the rules for the specified UFO.
     * @param id UFO type.
     * @return Rules for the UFO.
     */
    internal RuleUfo getUfo(string id, bool error = false) =>
	    getRule(id, "UFO", _ufos, error);

    /**
     * Returns the info about a specific unit.
     * @param name Unit name.
     * @return Rules for the units.
     */
    internal RuleSoldier getSoldier(string name, bool error = false) =>
	    getRule(name, "Soldier", _soldiers, error);

    /**
     * Returns the list of all soldiers
     * provided by the mod.
     * @return List of soldiers.
     */
    internal List<string> getSoldiersList() =>
	    _soldiersIndex;

    /**
     * Returns the list of research projects.
     * @return The list of research projects.
     */
    internal List<string> getResearchList() =>
	    _researchIndex;

    /**
     * Gets the list of StatStrings.
     * @return The list of StatStrings.
     */
    internal List<StatString> getStatStrings() =>
	    _statStrings;

    /**
     * Gets the research-requirements for Psi-Lab (it's a cache for psiStrengthEval)
     */
    internal List<string> getPsiRequirements() =>
	    _psiRequirements;

    /**
     * Returns the rules for the specified commendation.
     * @param id Commendation type.
     * @return Rules for the commendation.
     */
    internal RuleCommendations getCommendation(string id, bool error = false) =>
	    getRule(id, "Commendation", _commendations, error);

    /**
     * Returns the data for the specified ufo trajectory.
     * @param id Ufo trajectory id.
     * @return A pointer to the data for the specified ufo trajectory.
     */
    internal UfoTrajectory getUfoTrajectory(string id, bool error = false) =>
	    getRule(id, "Trajectory", _ufoTrajectories, error);

    /**
     * Returns the cost of an individual engineer
     * for purchase/maintenance.
     * @return Cost.
     */
    internal int getEngineerCost() =>
	    _costEngineer;

    /**
     * Returns the cost of an individual scientist
     * for purchase/maintenance.
     * @return Cost.
     */
    internal int getScientistCost() =>
	    _costScientist;

    internal List<string> getMissionScriptList() =>
	    _missionScriptIndex;

    internal RuleMissionScript getMissionScript(string name, bool error = false) =>
	    getRule(name, "Mission Script", _missionScripts, error);

    /**
     * Returns the info about a specific unit.
     * @param name Unit name.
     * @return Rules for the units.
     */
    internal Unit getUnit(string name, bool error = false) =>
	    getRule(name, "Unit", _units, error);

    /**
     * Returns the list of manufacture projects.
     * @return The list of manufacture projects.
     */
    internal List<string> getManufactureList() =>
	    _manufactureIndex;

    /**
     * Returns the list of all regions
     * provided by the mod.
     * @return List of regions.
     */
    internal List<string> getRegionsList() =>
	    _regionsIndex;

    /**
     * Returns the info about a specific alien race.
     * @param name Race name.
     * @return Rules for the race.
     */
    internal AlienRace getAlienRace(string name, bool error = false) =>
	    getRule(name, "Alien Race", _alienRaces, error);

    /**
     * Returns the rules for the specified terrain.
     * @param name Terrain name.
     * @return Rules for the terrain.
     */
    internal RuleTerrain getTerrain(string name, bool error = false) =>
	    getRule(name, "Terrain", _terrains, error);

    /**
     * Returns the list of all terrains
     * provided by the mod.
     * @return List of terrains.
     */
    internal List<string> getTerrainList() =>
	    _terrainIndex;

    internal List<MapScript> getMapScript(string id) =>
	    _mapScripts.TryGetValue(id, out List<MapScript> mapScript) ? mapScript : null;

    /**
     * Gets the name of the item to be used as alien fuel.
     * @return the name of the fuel.
     */
    internal string getAlienFuelName() =>
	    _alienFuel.Key;

    /**
     * Gets the alien item level table.
     * @return A deep array containing the alien item levels.
     */
    internal List<List<int>> getAlienItemLevels() =>
	    _alienItemLevels;

    /**
     * Returns a specific sound from either the land or underwater sound set.
     * @param depth the depth of the battlescape.
     * @param sound ID of the sound.
     * @return Pointer to the sound.
     */
    internal Sound getSoundByDepth(uint depth, uint sound, bool error = true)
    {
	    if (depth == 0)
		    return getSound("BATTLE.CAT", sound, error);
	    else
		    return getSound("BATTLE2.CAT", sound, error);
    }

    /**
     * Returns the list of inventories.
     * @return The list of inventories.
     */
    internal List<string> getInvsList() =>
	    _invsIndex;

    internal StatAdjustment getStatAdjustment(int difficulty)
    {
        if (difficulty >= 4)
        {
            return _statAdjustment[4];
        }
        return _statAdjustment[difficulty];
    }

    /**
     * Enables non-vanilla difficulty features.
     * Dehumanize yourself and face the Warboy.
     * @return Is the player screwed?
    */
    internal bool isDemigod() =>
	    _difficultyDemigod;

    /**
     * Returns the list of all craft weapons
     * provided by the mod.
     * @return List of craft weapons.
     */
    internal List<string> getCraftWeaponsList() =>
	    _craftWeaponsIndex;

    /**
     * Returns the list of all armors
     * provided by the mod.
     * @return List of armors.
     */
    internal List<string> getArmorsList() =>
	    _armorsIndex;

    /**
     * Returns the list of all items
     * provided by the mod.
     * @return List of items.
     */
    internal List<string> getItemsList() =>
	    _itemsIndex;

    internal Dictionary<string, SoundDefinition> getSoundDefinitions() =>
	    _soundDefs;

    /**
     * Gets the list of commendations provided by the mod.
     * @return The list of commendations.
     */
    internal Dictionary<string, RuleCommendations> getCommendationsList() =>
	    _commendations;

    /**
     * Returns the list of all crafts
     * provided by the mod.
     * @return List of crafts.
     */
    internal List<string> getCraftsList() =>
	    _craftsIndex;

    /**
     * Returns the list of all articles
     * provided by the mod.
     * @return List of articles.
     */
    internal List<string> getUfopaediaList() =>
	    _ufopaediaIndex;

    /**
    * Returns the list of all article categories
    * provided by the mod.
    * @return List of categories.
    */
    internal List<string> getUfopaediaCategoryList() =>
	    _ufopaediaCatIndex;

    internal string getFinalResearch() =>
	    _finalResearch;

    /**
     * Returns the list of all alien deployments
     * provided by the mod.
     * @return List of alien deployments.
     */
    internal List<string> getDeploymentsList() =>
	    _deploymentsIndex;

    /**
     * Returns the rules for a random alien mission based on a specific objective.
     * @param objective Alien mission objective.
     * @return Rules for the alien mission.
     */
    internal RuleAlienMission getRandomMission(MissionObjective objective, uint monthsPassed)
    {
	    int totalWeight = 0;
	    var possibilities = new Dictionary<int, RuleAlienMission>();
	    foreach (var i in _alienMissions)
	    {
		    if (i.Value.getObjective() == objective && i.Value.getWeight(monthsPassed) > 0)
		    {
			    totalWeight += i.Value.getWeight(monthsPassed);
			    possibilities[totalWeight] = i.Value;
		    }
	    }
	    if (totalWeight > 0)
	    {
		    int pick = RNG.generate(1, totalWeight);
		    foreach (var i in possibilities)
		    {
			    if (pick <= i.Key)
			    {
				    return i.Value;
			    }
		    }
	    }
	    return null;
    }

    /**
     * Returns the minimum amount of score the player can have,
     * otherwise they are defeated. Changes based on difficulty.
     * @return Score.
     */
    internal int getDefeatScore() =>
	    _defeatScore;

    /**
     * Returns the minimum amount of funds the player can have,
     * otherwise they are defeated.
     * @return Funds.
     */
    internal int getDefeatFunds() =>
	    _defeatFunds;

    /**
     * Returns the list of all base facilities
     * provided by the mod.
     * @return List of base facilities.
     */
    internal List<string> getBaseFacilitiesList() =>
	    _facilitiesIndex;

    /**
     * Generates and returns a list of facilities for custom bases.
     * The list contains all the facilities that are listed in the 'startingBase'
     * part of the ruleset.
     * @return The list of facilities for custom bases.
     */
    internal List<RuleBaseFacility> getCustomBaseFacilities()
    {
	    var placeList = new List<RuleBaseFacility>();

        foreach (var i in ((YamlSequenceNode)_startingBase["facilities"]).Children)
	    {
		    string type = i["type"].ToString();
		    RuleBaseFacility facility = getBaseFacility(type, true);
		    if (!facility.isLift())
		    {
			    placeList.Add(facility);
		    }
	    }
	    return placeList;
    }

    /**
     * Returns the time it takes to transfer personnel
     * between bases.
     * @return Time in hours.
     */
    internal int getPersonnelTime() =>
	    _timePersonnel;

    /**
     * Creates a new randomly-generated soldier.
     * @param save Saved game the soldier belongs to.
     * @param type The soldier type to generate.
     * @return Newly generated soldier.
     */
    internal Soldier genSoldier(SavedGame save, string type)
    {
	    Soldier soldier = null;
	    int newId = save.getId("STR_SOLDIER");
	    if (string.IsNullOrEmpty(type))
	    {
		    type = _soldiersIndex.First();
	    }

	    // Check for duplicates
	    // Original X-COM gives up after 10 tries so might as well do the same here
	    bool duplicate = true;
	    for (int tries = 0; tries < 10 && duplicate; ++tries)
	    {
		    soldier = null;
		    soldier = new Soldier(getSoldier(type, true), getArmor(getSoldier(type, true).getArmor(), true), newId);
		    duplicate = false;
            var bases = save.getBases();
            for (var i = 0; i < bases.Count && !duplicate; ++i)
		    {
                var soldiers = bases[i].getSoldiers();
                for (var j = 0; j < soldiers.Count && !duplicate; ++j)
			    {
				    if (soldiers[j].getName() == soldier.getName())
				    {
					    duplicate = true;
				    }
			    }
                var transfers = bases[i].getTransfers();
                for (var k = 0; k < transfers.Count && !duplicate; ++k)
			    {
				    if (transfers[k].getType() == TransferType.TRANSFER_SOLDIER && transfers[k].getSoldier().getName() == soldier.getName())
				    {
					    duplicate = true;
				    }
			    }
		    }
	    }

	    // calculate new statString
	    soldier.calcStatString(getStatStrings(), (Options.psiStrengthEval && save.isResearched(getPsiRequirements())));

	    return soldier;
    }

    /**
     * Returns the list of all alien races.
     * provided by the mod.
     * @return List of alien races.
     */
    internal List<string> getAlienRacesList() =>
	    _aliensIndex;

    /**
     * Returns the list of alien mission types.
     * @return The list of alien mission types.
     */
    internal List<string> getAlienMissionList() =>
	    _alienMissionsIndex;

    /**
     * Gets the defined starting base.
     * @return The starting base definition.
     */
    internal YamlNode getStartingBase() =>
	    _startingBase;

    /**
     * Generates a brand new saved game with starting data.
     * @return A new saved game.
     */
    internal SavedGame newSave()
    {
	    SavedGame save = new SavedGame();

	    // Add countries
	    foreach (var i in _countriesIndex)
	    {
		    RuleCountry country = getCountry(i);
		    if (country.getLonMin().Any())
			    save.getCountries().Add(new Country(country));
	    }
	    // Adjust funding to total $6M
	    int missing = ((_initialFunding - save.getCountryFunding()/1000) / (int)save.getCountries().Count) * 1000;
	    foreach (var i in save.getCountries())
	    {
		    int funding = i.getFunding().Last() + missing;
		    if (funding < 0)
		    {
			    funding = i.getFunding().Last();
		    }
		    i.setFunding(funding);
	    }
	    save.setFunds(save.getCountryFunding());

	    // Add regions
	    foreach (var i in _regionsIndex)
	    {
		    RuleRegion region = getRegion(i);
		    if (region.getLonMin().Any())
			    save.getRegions().Add(new Region(region));
	    }

	    // Set up starting base
	    Base @base = new Base(this);
	    @base.load(_startingBase, save, true);
	    save.getBases().Add(@base);

	    // Correct IDs
	    foreach (var i in @base.getCrafts())
	    {
		    save.getId(i.getRules().getType());
	    }

        // Determine starting transport craft
        Craft transportCraft = null;
	    foreach (var c in @base.getCrafts())
	    {
		    if (c.getRules().getSoldiers() > 0)
		    {
			    transportCraft = c;
			    break;
		    }
	    }

	    // Determine starting soldier types
	    List<string> soldierTypes = _soldiersIndex;
	    for (var i = 0; i < soldierTypes.Count;)
	    {
		    if (!getSoldier(soldierTypes[i]).getRequirements().Any())
		    {
			    ++i;
		    }
		    else
		    {
			    soldierTypes.RemoveAt(i);
		    }
	    }

	    YamlNode node = _startingBase["randomSoldiers"];
	    var randomTypes = new List<string>();
	    if (node != null)
	    {
		    // Starting soldiers specified by type
		    if (node is YamlMappingNode m)
		    {
                var randomSoldiers = m != null ? m.Children.ToDictionary(x => x.ToString(), x => int.Parse(x.ToString())) : new Dictionary<string, int>();
			    foreach (var i in randomSoldiers)
			    {
                    for (int s = 0; s < int.Parse(i.Value.ToString()); ++s)
				    {
					    randomTypes.Add(i.Key.ToString());
				    }
			    }
		    }
		    // Starting soldiers specified by amount
		    else if (node is YamlScalarNode)
		    {
			    int randomSoldiers = int.Parse(node.ToString());
			    for (int s = 0; s < randomSoldiers; ++s)
			    {
				    randomTypes.Add(soldierTypes[RNG.generate(0, soldierTypes.Count - 1)]);
			    }
		    }
		    // Generate soldiers
		    int maxSoldiersInTransportCraft = 0;
		    if (transportCraft != null)
		    {
			    maxSoldiersInTransportCraft = transportCraft.getRules().getSoldiers();
                var vehicles = transportCraft.getVehicles();
			    for (var v = 0; v < vehicles.Count;)
			    {
				    if ((int)maxSoldiersInTransportCraft < vehicles[v].getSize())
				    {
					    @base.getStorageItems().addItem(vehicles[v].getRules().getType(), 1);
					    if (vehicles[v].getAmmo() > 0 && vehicles[v].getRules().getCompatibleAmmo().Any())
					    {
						    @base.getStorageItems().addItem(
							    vehicles[v].getRules().getCompatibleAmmo().First(),
							    vehicles[v].getAmmo() / getItem(vehicles[v].getRules().getCompatibleAmmo().First()).getClipSize());
					    }
					    vehicles.RemoveAt(v);
				    }
				    else
				    {
					    maxSoldiersInTransportCraft -= vehicles[v].getSize();
					    ++v;
				    }
			    }
		    }

		    for (int i = 0; i < randomTypes.Count; ++i)
		    {
			    Soldier soldier = genSoldier(save, randomTypes[i]);
			    if (transportCraft != null && i < maxSoldiersInTransportCraft)
			    {
				    soldier.setCraft(transportCraft);
			    }
			    @base.getSoldiers().Add(soldier);
			    // Award soldier a special 'original eight' commendation
			    if (_commendations.ContainsKey("STR_MEDAL_ORIGINAL8_NAME"))
			    {
				    SoldierDiary diary = soldier.getDiary();
				    diary.awardOriginalEightCommendation();
				    foreach (var comm in diary.getSoldierCommendations())
				    {
					    comm.makeOld();
				    }
			    }
		    }
	    }

	    // Setup alien strategy
	    save.getAlienStrategy().init(this);
	    save.setTime(_startingTime);

	    return save;
    }

    /**
     * Returns the smallest facility's radar range.
     * @return The minimum range.
     */
    internal int getMinRadarRange()
    {
        int minRadarRange = 0;

        {
            foreach (var i in _facilitiesIndex)
            {
                RuleBaseFacility f = getBaseFacility(i);
                if (f == null) continue;

                int radarRange = f.getRadarRange();
                if (radarRange > 0 && (minRadarRange == 0 || minRadarRange > radarRange))
                {
                    minRadarRange = radarRange;
                }
            }
        }

        return minRadarRange;
    }

    /**
     * Returns the data for the specified video cutscene.
     * @param id Video id.
     * @return A pointer to the data for the specified video.
     */
    internal RuleVideo getVideo(string id, bool error = false) =>
	    getRule(id, "Video", _videos, error);

    /**
     * Returns the list of all ufos
     * provided by the mod.
     * @return List of ufos.
     */
    internal List<string> getUfosList() =>
	    _ufosIndex;

    /**
     * Returns the list of all countries
     * provided by the mod.
     * @return List of countries.
     */
    internal List<string> getCountriesList() =>
	    _countriesIndex;

    /**
     * Gets the amount of alien fuel to recover.
     * @return the amount to recover.
     */
    internal int getAlienFuelQuantity() =>
        _alienFuel.Value;

	/// Gets first turn when AI can use Blaster launcher.
	internal int getTurnAIUseBlaster() =>
        _turnAIUseBlaster;

	/// Gets first turn when AI can use grenade.
	internal int getTurnAIUseGrenade() =>
        _turnAIUseGrenade;

    /**
     * Returns the list of inventories.
     * @return Pointer to inventory list.
     */
    internal Dictionary<string, RuleInventory> getInventories() =>
	    _invs;
}
