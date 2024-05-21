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

namespace SharpXcom.Menu;

/**
 * New Battle that displays a list
 * of options to configure a new
 * standalone mission.
 */
internal class NewBattleState : State
{
    Craft _craft;
    Window _window;
    Text _txtTitle, _txtMapOptions, _txtAlienOptions;
    Frame _frameLeft, _frameRight;
    Text _txtMission, _txtCraft, _txtDarkness, _txtTerrain, _txtDifficulty, _txtAlienRace, _txtAlienTech, _txtDepth;
    ComboBox _cbxMission, _cbxCraft, _cbxTerrain, _cbxDifficulty, _cbxAlienRace;
    TextButton _btnOk, _btnCancel, _btnEquip, _btnRandom;
    Slider _slrDarkness, _slrAlienTech, _slrDepth;
    List<string> _missionTypes, _terrainTypes, _alienRaces, _crafts;

    /**
	 * Initializes all the elements in the New Battle window.
	 * @param game Pointer to the core game.
	 */
    internal NewBattleState()
	{
		_craft = null;

		// Create objects
		_window = new Window(this, 320, 200, 0, 0, WindowPopup.POPUP_BOTH);
		_txtTitle = new Text(320, 17, 0, 9);

		_txtMapOptions = new Text(148, 9, 8, 68);
		_frameLeft = new Frame(148, 96, 8, 78);
		_txtAlienOptions = new Text(148, 9, 164, 68);
		_frameRight = new Frame(148, 96, 164, 78);

		_txtMission = new Text(100, 9, 8, 30);
		_cbxMission = new ComboBox(this, 214, 16, 98, 26);

		_txtCraft = new Text(100, 9, 8, 50);
		_cbxCraft = new ComboBox(this, 106, 16, 98, 46);
		_btnEquip = new TextButton(106, 16, 206, 46);

		_txtDarkness = new Text(120, 9, 22, 83);
		_slrDarkness = new Slider(120, 16, 22, 93);

		_txtTerrain = new Text(120, 9, 22, 113);
		_cbxTerrain = new ComboBox(this, 120, 16, 22, 123);

		_txtDepth = new Text(120, 9, 22, 143);
		_slrDepth = new Slider(120, 16, 22, 153);

		_txtDifficulty = new Text(120, 9, 178, 83);
		_cbxDifficulty = new ComboBox(this, 120, 16, 178, 93);

		_txtAlienRace = new Text(120, 9, 178, 113);
		_cbxAlienRace = new ComboBox(this, 120, 16, 178, 123);

		_txtAlienTech = new Text(120, 9, 178, 143);
		_slrAlienTech = new Slider(120, 16, 178, 153);

		_btnOk = new TextButton(100, 16, 8, 176);
		_btnCancel = new TextButton(100, 16, 110, 176);
		_btnRandom = new TextButton(100, 16, 212, 176);

		// Set palette
		setInterface("newBattleMenu");

		add(_window, "window", "newBattleMenu");
		add(_txtTitle, "heading", "newBattleMenu");
		add(_txtMapOptions, "heading", "newBattleMenu");
		add(_frameLeft, "frames", "newBattleMenu");
		add(_txtAlienOptions, "heading", "newBattleMenu");
		add(_frameRight, "frames", "newBattleMenu");

		add(_txtMission, "text", "newBattleMenu");
		add(_txtCraft, "text", "newBattleMenu");
		add(_btnEquip, "button1", "newBattleMenu");

		add(_txtDarkness, "text", "newBattleMenu");
		add(_slrDarkness, "button1", "newBattleMenu");
		add(_txtDepth, "text", "newBattleMenu");
		add(_slrDepth, "button1", "newBattleMenu");
		add(_txtTerrain, "text", "newBattleMenu");
		add(_txtDifficulty, "text", "newBattleMenu");
		add(_txtAlienRace, "text", "newBattleMenu");
		add(_txtAlienTech, "text", "newBattleMenu");
		add(_slrAlienTech, "button1", "newBattleMenu");

		add(_btnOk, "button2", "newBattleMenu");
		add(_btnCancel, "button2", "newBattleMenu");
		add(_btnRandom, "button2", "newBattleMenu");

		add(_cbxTerrain, "button1", "newBattleMenu");
		add(_cbxAlienRace, "button1", "newBattleMenu");
		add(_cbxDifficulty, "button1", "newBattleMenu");
		add(_cbxCraft, "button1", "newBattleMenu");
		add(_cbxMission, "button1", "newBattleMenu");

		centerAllSurfaces();

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setBig();
		_txtTitle.setText(tr("STR_MISSION_GENERATOR"));

		_txtMapOptions.setText(tr("STR_MAP_OPTIONS"));

		_frameLeft.setThickness(3);

		_txtAlienOptions.setText(tr("STR_ALIEN_OPTIONS"));

		_frameRight.setThickness(3);

		_txtMission.setText(tr("STR_MISSION"));

		_txtCraft.setText(tr("STR_CRAFT"));

		_txtDarkness.setText(tr("STR_MAP_DARKNESS"));

		_txtDepth.setText(tr("STR_MAP_DEPTH"));

		_txtTerrain.setText(tr("STR_MAP_TERRAIN"));

		_txtDifficulty.setText(tr("STR_DIFFICULTY"));

		_txtAlienRace.setText(tr("STR_ALIEN_RACE"));

		_txtAlienTech.setText(tr("STR_ALIEN_TECH_LEVEL"));

		_missionTypes = _game.getMod().getDeploymentsList();
		_cbxMission.setOptions(_missionTypes, true);
		_cbxMission.onChange(cbxMissionChange);

		List<string> crafts = _game.getMod().getCraftsList();
		foreach (var i in crafts)
		{
			RuleCraft rule = _game.getMod().getCraft(i);
			if (rule.getSoldiers() > 0)
			{
				_crafts.Add(i);
			}
		}
		_cbxCraft.setOptions(_crafts, true);
		_cbxCraft.onChange(cbxCraftChange);

		_slrDarkness.setRange(0, 15);

		_slrDepth.setRange(1, 3);

		_cbxTerrain.onChange(cbxTerrainChange);

        var difficulty = new List<string>
        {
            tr("STR_1_BEGINNER"),
            tr("STR_2_EXPERIENCED"),
            tr("STR_3_VETERAN"),
            tr("STR_4_GENIUS"),
            tr("STR_5_SUPERHUMAN")
        };
        _cbxDifficulty.setOptions(difficulty);

		_alienRaces = _game.getMod().getAlienRacesList();
		for (var i = 0; i < _alienRaces.Count;)
		{
			if (_alienRaces[i].Contains("_UNDERWATER"))
			{
				_alienRaces.RemoveAt(i);
			}
			else
			{
				++i;
			}
		}
		_cbxAlienRace.setOptions(_alienRaces, true);

		_slrAlienTech.setRange(0, _game.getMod().getAlienItemLevels().Count - 1);

		_btnEquip.setText(tr("STR_EQUIP_CRAFT"));
		_btnEquip.onMouseClick(btnEquipClick);

		_btnRandom.setText(tr("STR_RANDOMIZE"));
		_btnRandom.onMouseClick(btnRandomClick);

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

		_btnCancel.setText(tr("STR_CANCEL"));
		_btnCancel.onMouseClick(btnCancelClick);
		_btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

		load();
	}

	/**
	 *
	 */
	~NewBattleState() { }

    /**
     * Updates Map Options based on the
     * current Mission type.
     * @param action Pointer to an action.
     */
    void cbxMissionChange(Action _)
    {
        AlienDeployment ruleDeploy = _game.getMod().getDeployment(_missionTypes[(int)_cbxMission.getSelected()]);
        var terrains = new HashSet<string>();

        // Get terrains associated with this mission
        List<string> deployTerrains, globeTerrains;
        deployTerrains = ruleDeploy.getTerrains();
        if (!deployTerrains.Any())
        {
            globeTerrains = _game.getMod().getGlobe().getTerrains(string.Empty);
        }
        else
        {
            globeTerrains = _game.getMod().getGlobe().getTerrains(ruleDeploy.getType());
        }
        foreach (var i in deployTerrains)
        {
            terrains.Add(i);
        }
        foreach (var i in globeTerrains)
        {
            terrains.Add(i);
        }
        _terrainTypes.Clear();
        var terrainStrings = new List<string>();
        foreach (var i in terrains)
        {
            _terrainTypes.Add(i);
            terrainStrings.Add("MAP_" + i);
        }

        // Hide controls that don't apply to mission
        _txtDarkness.setVisible(ruleDeploy.getShade() == -1);
        _slrDarkness.setVisible(ruleDeploy.getShade() == -1);
        _txtTerrain.setVisible(_terrainTypes.Count > 1);
        _cbxTerrain.setVisible(_terrainTypes.Count > 1);
        _cbxTerrain.setOptions(terrainStrings, true);
        _cbxTerrain.setSelected(0);
        cbxTerrainChange(null);
    }

	/**
	 * Updates craft accordingly.
	 * @param action Pointer to an action.
	 */
	void cbxCraftChange(Action _)
	{
		_craft.changeRules(_game.getMod().getCraft(_crafts[(int)_cbxCraft.getSelected()]));
		int current = _craft.getNumSoldiers();
		int max = _craft.getRules().getSoldiers();
		if (current > max)
		{
			var soldiers = _craft.getBase().getSoldiers();
			for (var i = soldiers.Count - 1; i >= 0 && current > max; --i)
			{
				if (soldiers[i].getCraft() == _craft)
				{
					soldiers[i].setCraft(null);
					current--;
				}
			}
		}
	}

	/**
	 * Updates the depth slider accordingly when terrain selection changes.
	 * @param action Pointer to an action.
	 */
	void cbxTerrainChange(Action _)
	{
		AlienDeployment ruleDeploy = _game.getMod().getDeployment(_missionTypes[(int)_cbxMission.getSelected()]);
		int minDepth = 0;
		int maxDepth = 0;
		if (ruleDeploy.getMaxDepth() > 0 || _game.getMod().getTerrain(_terrainTypes[(int)_cbxTerrain.getSelected()]).getMaxDepth() > 0 ||
			(ruleDeploy.getTerrains().Any() && _game.getMod().getTerrain(ruleDeploy.getTerrains().First()).getMaxDepth() > 0))
		{
			minDepth = 1;
			maxDepth = 3;
		}
		_txtDepth.setVisible(minDepth != maxDepth);
		_slrDepth.setVisible(minDepth != maxDepth);
		_slrDepth.setRange(minDepth, maxDepth);
		_slrDepth.setValue(minDepth);
	}

	/**
	 * Shows the Craft Info screen.
	 * @param action Pointer to an action.
	 */
	void btnEquipClick(Action _) =>
		_game.pushState(new CraftInfoState(_game.getSavedGame().getBases().First(), 0));

	/**
	 * Randomize the state
	 * @param action Pointer to an action.
	 */
	void btnRandomClick(Action _)
	{
		initSave();

		_cbxMission.setSelected((uint)RNG.generate(0, _missionTypes.Count - 1));
		cbxMissionChange(null);
		_cbxCraft.setSelected((uint)RNG.generate(0, _crafts.Count - 1));
		cbxCraftChange(null);
		_slrDarkness.setValue(RNG.generate(0, 15));
		_cbxTerrain.setSelected((uint)RNG.generate(0, _terrainTypes.Count - 1));
		cbxTerrainChange(null);
		_cbxAlienRace.setSelected((uint)RNG.generate(0, _alienRaces.Count - 1));
		_cbxDifficulty.setSelected((uint)RNG.generate(0, 4));
		_slrAlienTech.setValue(RNG.generate(0, _game.getMod().getAlienItemLevels().Count - 1));
	}

	/**
	 * Starts the battle.
	 * @param action Pointer to an action.
	 */
	void btnOkClick(Action _)
	{
		save();
		if (_missionTypes[(int)_cbxMission.getSelected()] != "STR_BASE_DEFENSE" && _craft.getNumSoldiers() == 0 && _craft.getNumVehicles() == 0)
		{
			return;
		}

		SavedBattleGame bgame = new SavedBattleGame();
		_game.getSavedGame().setBattleGame(bgame);
		bgame.setMissionType(_missionTypes[(int)_cbxMission.getSelected()]);
		BattlescapeGenerator bgen = new BattlescapeGenerator(_game);
		Base @base = null;

		bgen.setTerrain(_game.getMod().getTerrain(_terrainTypes[(int)_cbxTerrain.getSelected()]));

		// base defense
		if (_missionTypes[(int)_cbxMission.getSelected()] == "STR_BASE_DEFENSE")
		{
			@base = _craft.getBase();
			bgen.setBase(@base);
			_craft = null;
		}
		// alien base
		else if (_game.getMod().getDeployment(bgame.getMissionType()).isAlienBase())
		{
			AlienBase b = new AlienBase(_game.getMod().getDeployment(bgame.getMissionType()));
			b.setId(1);
			b.setAlienRace(_alienRaces[(int)_cbxAlienRace.getSelected()]);
			_craft.setDestination(b);
			bgen.setAlienBase(b);
			_game.getSavedGame().getAlienBases().Add(b);
		}
		// ufo assault
		else if (_craft != null && _game.getMod().getUfo(_missionTypes[(int)_cbxMission.getSelected()]) != null)
		{
			Ufo u = new Ufo(_game.getMod().getUfo(_missionTypes[(int)_cbxMission.getSelected()]));
			u.setId(1);
			_craft.setDestination(u);
			bgen.setUfo(u);
			// either ground assault or ufo crash
			if (RNG.generate(0,1) == 1)
			{
				u.setStatus(UfoStatus.LANDED);
				bgame.setMissionType("STR_UFO_GROUND_ASSAULT");
			}
			else
			{
				u.setStatus(UfoStatus.CRASHED);
				bgame.setMissionType("STR_UFO_CRASH_RECOVERY");
			}
			_game.getSavedGame().getUfos().Add(u);
		}
		// mission site
		else
		{
			AlienDeployment deployment = _game.getMod().getDeployment(bgame.getMissionType());
			RuleAlienMission mission = _game.getMod().getAlienMission(_game.getMod().getAlienMissionList().First()); // doesn't matter
			MissionSite m = new MissionSite(mission, deployment);
			m.setId(1);
			m.setAlienRace(_alienRaces[(int)_cbxAlienRace.getSelected()]);
			_craft.setDestination(m);
			bgen.setMissionSite(m);
			_game.getSavedGame().getMissionSites().Add(m);
		}

		if (_craft != null)
		{
			_craft.setSpeed(0);
			bgen.setCraft(_craft);
		}

		_game.getSavedGame().setDifficulty((GameDifficulty)_cbxDifficulty.getSelected());

		bgen.setWorldShade(_slrDarkness.getValue());
		bgen.setAlienRace(_alienRaces[(int)_cbxAlienRace.getSelected()]);
		bgen.setAlienItemlevel(_slrAlienTech.getValue());
		bgame.setDepth(_slrDepth.getValue());

		bgen.run();

		_game.popState();
		_game.popState();
		_game.pushState(new BriefingState(_craft, @base));
		_craft = null;
	}

	/**
	 * Returns to the previous screen.
	 * @param action Pointer to an action.
	 */
	void btnCancelClick(Action _)
	{
		save();
		_game.setSavedGame(null);
		_game.popState();
	}

	/**
	 * Initializes a new savegame with
	 * everything available.
	 */
	void initSave()
	{
		Mod.Mod mod = _game.getMod();
		SavedGame save = new SavedGame();
		Base @base = new Base(mod);
		YamlNode starter = _game.getMod().getStartingBase();
		@base.load(starter, save, true, true);
		save.getBases().Add(@base);

		// Kill everything we don't want in this base
		@base.getSoldiers().Clear();
		@base.getCrafts().Clear();
		@base.getStorageItems().getContents().Clear();

		_craft = new Craft(mod.getCraft(_crafts[(int)_cbxCraft.getSelected()]), @base, 1);
		@base.getCrafts().Add(_craft);

		UnitStats stats;
		// Generate soldiers
		for (int i = 0; i < 30; ++i)
		{
			int randomType = RNG.generate(0, _game.getMod().getSoldiersList().Count - 1);
			Soldier soldier = mod.genSoldier(save, _game.getMod().getSoldiersList()[randomType]);

			for (int n = 0; n < 5; ++n)
			{
				if (RNG.percent(70))
					continue;
				soldier.promoteRank();

				stats = soldier.getCurrentStats();
				stats.tu 			+= RNG.generate(0, 5);
				stats.stamina		+= RNG.generate(0, 5);
				stats.health		+= RNG.generate(0, 5);
				stats.bravery		+= RNG.generate(0, 5);
				stats.reactions		+= RNG.generate(0, 5);
				stats.firing		+= RNG.generate(0, 5);
				stats.throwing		+= RNG.generate(0, 5);
				stats.strength		+= RNG.generate(0, 5);
				stats.psiStrength	+= RNG.generate(0, 5);
				stats.melee			+= RNG.generate(0, 5);
				stats.psiSkill		+= RNG.generate(0, 20);
			}
			stats = soldier.getCurrentStats();
			stats.bravery = (int)Math.Ceiling(stats.bravery / 10.0) * 10; // keep it a multiple of 10

			@base.getSoldiers().Add(soldier);
			if (i < _craft.getRules().getSoldiers())
				soldier.setCraft(_craft);
		}

		// Generate items
		List<string> items = mod.getItemsList();
		foreach (var i in items)
		{
			RuleItem rule = _game.getMod().getItem(i);
			if (rule.getBattleType() != BattleType.BT_CORPSE && rule.isRecoverable())
			{
				@base.getStorageItems().addItem(i, 1);
				if (rule.getBattleType() != BattleType.BT_NONE && !rule.isFixed() && rule.getBigSprite() > -1)
				{
					_craft.getItems().addItem(i, 1);
				}
			}
		}

		// Add research
		List<string> research = mod.getResearchList();
		foreach (var i in research)
		{
			save.addFinishedResearchSimple(mod.getResearch(i));
		}

		_game.setSavedGame(save);
		cbxMissionChange(null);
	}

	/**
	 * Resets the menu music and savegame
	 * when coming back from the battlescape.
	 */
	internal override void init()
	{
		base.init();

		if (_craft == null)
		{
			load();
		}
	}

	/**
	 * Loads new battle data from a YAML file.
	 * @param filename YAML filename.
	 */
	void load(string filename = "battle")
	{
		string s = Options.getMasterUserFolder() + filename + ".cfg";
		if (!CrossPlatform.fileExists(s))
		{
			initSave();
		}
		else
		{
			try
			{
				using var input = new StreamReader(s);
				var yaml = new YamlStream();
				yaml.Load(input);
				var doc = (YamlMappingNode)yaml.Documents[0].RootNode;
				_cbxMission.setSelected((uint)Math.Min(uint.Parse(doc.Children["mission"].ToString()), _missionTypes.Count - 1));
				cbxMissionChange(null);
				_cbxCraft.setSelected((uint)Math.Min(uint.Parse(doc.Children["craft"].ToString()), _crafts.Count - 1));
				_slrDarkness.setValue((int)uint.Parse(doc.Children["darkness"].ToString()));
				_cbxTerrain.setSelected((uint)Math.Min(uint.Parse(doc.Children["terrain"].ToString()), _terrainTypes.Count - 1));
				cbxTerrainChange(null);
				_cbxAlienRace.setSelected((uint)Math.Min(uint.Parse(doc.Children["alienRace"].ToString()), _alienRaces.Count - 1));
				_cbxDifficulty.setSelected(uint.Parse(doc.Children["difficulty"].ToString()));
				_slrAlienTech.setValue((int)uint.Parse(doc.Children["alienTech"].ToString()));

				if (doc.Children["base"] != null)
				{
					Mod.Mod mod = _game.getMod();
					SavedGame save = new SavedGame();

					Base @base = new Base(mod);
					@base.load(doc.Children["base"], save, false);
					save.getBases().Add(@base);

					// Add research
					List<string> research = mod.getResearchList();
					foreach (var i in research)
					{
						save.addFinishedResearchSimple(mod.getResearch(i));
					}

					// Generate items
					@base.getStorageItems().getContents().Clear();
					List<string> items = mod.getItemsList();
					foreach (var i in items)
					{
						RuleItem rule = _game.getMod().getItem(i);
						if (rule.getBattleType() != BattleType.BT_CORPSE && rule.isRecoverable())
						{
							@base.getStorageItems().addItem(i, 1);
						}
					}

					// Fix invalid contents
					if (!@base.getCrafts().Any())
					{
						string craftType = _crafts[(int)_cbxCraft.getSelected()];
						_craft = new Craft(_game.getMod().getCraft(craftType), @base, save.getId(craftType));
						@base.getCrafts().Add(_craft);
					}
					else
					{
						_craft = @base.getCrafts().First();
						foreach (var i in _craft.getItems().getContents())
						{
							RuleItem rule = _game.getMod().getItem(i.Key);
							if (rule == null)
							{
								_craft.getItems().getContents()[i.Key] = 0;
							}
						}
					}

					_game.setSavedGame(save);
				}
				else
				{
					initSave();
				}
			}
			catch (YamlException e)
			{
				Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {e.Message}");
				initSave();
			}
		}
	}

	/**
	 * Saves new battle data to a YAML file.
	 * @param filename YAML filename.
	 */
	void save(string filename = "battle")
	{
		string s = Options.getMasterUserFolder() + filename + ".cfg";
		try
		{
            using var sav = new StreamWriter(s);
			var @out = new Emitter(sav);
			var node = new YamlMappingNode();
            var serializer = new Serializer();

			node.Add("mission", _cbxMission.getSelected().ToString());
			node.Add("craft", _cbxCraft.getSelected().ToString());
			node.Add("darkness", _slrDarkness.getValue().ToString());
			node.Add("terrain", _cbxTerrain.getSelected().ToString());
			node.Add("alienRace", _cbxAlienRace.getSelected().ToString());
			node.Add("difficulty", _cbxDifficulty.getSelected().ToString());
			node.Add("alienTech", _slrAlienTech.getValue().ToString());
			node.Add("base", _game.getSavedGame().getBases().First().save());
            serializer.Serialize(@out, node);

			sav.Close();
		}
		catch (Exception)
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} Failed to save {filename}.cfg");
		}
	}
}
