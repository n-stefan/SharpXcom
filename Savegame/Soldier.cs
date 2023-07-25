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

enum SoldierRank { RANK_ROOKIE, RANK_SQUADDIE, RANK_SERGEANT, RANK_CAPTAIN, RANK_COLONEL, RANK_COMMANDER };

enum SoldierGender { GENDER_MALE, GENDER_FEMALE };

enum SoldierLook { LOOK_BLONDE, LOOK_BROWNHAIR, LOOK_ORIENTAL, LOOK_AFRICAN };

/**
 * Represents a soldier hired by the player.
 * Soldiers have a wide variety of stats that affect
 * their performance during battles.
 */
internal class Soldier
{
    string _name;
    int _id, _improvement, _psiStrImprovement;
    RuleSoldier _rules;
    UnitStats _initialStats, _currentStats;
    SoldierRank _rank;
    Craft _craft;
    SoldierGender _gender;
    SoldierLook _look;
    int _missions, _kills, _recovery;
    bool _recentlyPromoted, _psiTraining;
    Armor _armor;
    List<EquipmentLayoutItem> _equipmentLayout;
    SoldierDeath _death;
    SoldierDiary _diary;
    string _statString;

	internal Soldier() { }

    /**
	 * Initializes a new soldier, either blank or randomly generated.
	 * @param rules Soldier ruleset.
	 * @param armor Soldier armor.
	 * @param id Unique soldier id for soldier generation.
	 */
    internal Soldier(RuleSoldier rules, Armor armor, int id = 0)
	{
		_id = id;
		_improvement = 0;
		_psiStrImprovement = 0;
		_rules = rules;
		_rank = SoldierRank.RANK_ROOKIE;
		_craft = null;
		_gender = SoldierGender.GENDER_MALE;
		_look = SoldierLook.LOOK_BLONDE;
		_missions = 0;
		_kills = 0;
		_recovery = 0;
		_recentlyPromoted = false;
		_psiTraining = false;
		_armor = armor;
		_death = null;
		_diary = new SoldierDiary();

        if (id != 0)
		{
			UnitStats minStats = rules.getMinStats();
			UnitStats maxStats = rules.getMaxStats();

			_initialStats.tu = RNG.generate(minStats.tu, maxStats.tu);
			_initialStats.stamina = RNG.generate(minStats.stamina, maxStats.stamina);
			_initialStats.health = RNG.generate(minStats.health, maxStats.health);
			_initialStats.bravery = RNG.generate(minStats.bravery/10, maxStats.bravery/10)*10;
			_initialStats.reactions = RNG.generate(minStats.reactions, maxStats.reactions);
			_initialStats.firing = RNG.generate(minStats.firing, maxStats.firing);
			_initialStats.throwing = RNG.generate(minStats.throwing, maxStats.throwing);
			_initialStats.strength = RNG.generate(minStats.strength, maxStats.strength);
			_initialStats.psiStrength = RNG.generate(minStats.psiStrength, maxStats.psiStrength);
			_initialStats.melee = RNG.generate(minStats.melee, maxStats.melee);
			_initialStats.psiSkill = minStats.psiSkill;

			_currentStats = _initialStats;

			List<SoldierNamePool> names = rules.getNames();
			if (names.Any())
			{
                int nationality = RNG.generate(0, names.Count - 1);
				_name = names[nationality].genName(_gender, rules.getFemaleFrequency());
				_look = (SoldierLook)names[nationality].genLook(4); // Once we add the ability to mod in extra looks, this will need to reference the ruleset for the maximum amount of looks.
			}
			else
			{
				// No possible names, just wing it
				_gender = (RNG.percent(rules.getFemaleFrequency()) ? SoldierGender.GENDER_FEMALE : SoldierGender.GENDER_MALE);
				_look = (SoldierLook)RNG.generate(0, 3);
				_name = (_gender == SoldierGender.GENDER_FEMALE) ? "Jane" : "John";
				_name = $"{_name} Doe";
			}
		}
	}

	/**
	 *
	 */
	~Soldier()
	{
		_equipmentLayout.Clear();
		_death = null;
		_diary = null;
	}

	/**
	 * Returns a localizable-string representation of
	 * the soldier's military rank.
	 * @return String ID for rank.
	 */
	internal string getRankString()
	{
		switch (_rank)
		{
			case SoldierRank.RANK_ROOKIE:
				return "STR_ROOKIE";
			case SoldierRank.RANK_SQUADDIE:
				return "STR_SQUADDIE";
			case SoldierRank.RANK_SERGEANT:
				return "STR_SERGEANT";
			case SoldierRank.RANK_CAPTAIN:
				return "STR_CAPTAIN";
			case SoldierRank.RANK_COLONEL:
				return "STR_COLONEL";
			case SoldierRank.RANK_COMMANDER:
				return "STR_COMMANDER";
			default:
				return string.Empty;
		}
	}

    /**
	 * Returns the soldier's full name (and, optionally, statString).
	 * @param statstring Add stat string?
	 * @param maxLength Restrict length to a certain value.
	 * @return Soldier name.
	 */
    internal string getName(bool statstring = false, uint maxLength = 20)
	{
		if (statstring && !string.IsNullOrEmpty(_statString))
		{
			string name = Unicode.convUtf8ToUtf32(_name);
			if (name.Length + _statString.Length > maxLength)
			{
				return Unicode.convUtf32ToUtf8(name.Substring(0, (int)(maxLength - _statString.Length))) + "/" + _statString;
			}
			else
			{
				return _name + "/" + _statString;
			}
		}
		else
		{
			return _name;
		}
	}

    /**
     * Kills the soldier in the Geoscape.
     * @param death Pointer to death data.
     */
    internal void die(SoldierDeath death)
    {
        _death = null;
        _death = death;

        // Clean up associations
        _craft = null;
        _psiTraining = false;
        _recentlyPromoted = false;
        _recovery = 0;
        _equipmentLayout.Clear();
    }

	/**
	 * Saves the soldier to a YAML file.
	 * @return YAML node.
	 */
	internal YamlNode save()
	{
        var node = new YamlMappingNode
        {
            { "type", _rules.getType() },
            { "id", _id.ToString() },
            { "name", _name },
            { "initialStats", _initialStats.save() },
            { "currentStats", _currentStats.save() },
            { "rank", ((int)_rank).ToString() }
        };
        if (_craft != null)
		{
			node.Add("craft", _craft.saveId());
		}
		node.Add("gender", ((int)_gender).ToString());
		node.Add("look", ((int)_look).ToString());
		node.Add("missions", _missions.ToString());
		node.Add("kills", _kills.ToString());
		if (_recovery > 0)
			node.Add("recovery", _recovery.ToString());
		node.Add("armor", _armor.getType());
		if (_psiTraining)
			node.Add("psiTraining", _psiTraining.ToString());
		node.Add("improvement", _improvement.ToString());
		node.Add("psiStrImprovement", _psiStrImprovement.ToString());
		if (_equipmentLayout.Any())
		{
            node.Add("equipmentLayout", new YamlSequenceNode(_equipmentLayout.Select(x => x.save())));
		}
		if (_death != null)
		{
			node.Add("death", _death.save());
		}
		if (Options.soldierDiaries && (_diary.getMissionIdList().Any() || _diary.getSoldierCommendations().Any() || _diary.getMonthsService() > 0))
		{
			node.Add("diary", _diary.save());
		}

		return node;
	}

    /**
     * Assigns the soldier to a new craft.
     * @param craft Pointer to craft.
     */
    internal void setCraft(Craft craft) =>
        _craft = craft;

	/**
	 * Loads the soldier from a YAML file.
	 * @param node YAML node.
	 * @param mod Game mod.
	 * @param save Pointer to savegame.
	 */
	internal void load(YamlNode node, Mod.Mod mod, SavedGame save)
	{
		_id = int.Parse(node["id"].ToString());
		_name = node["name"].ToString();
		_initialStats.load(node["initialStats"]);
		_currentStats.load(node["currentStats"]);
		_rank = (SoldierRank)int.Parse(node["rank"].ToString());
		_gender = (SoldierGender)int.Parse(node["gender"].ToString());
		_look = (SoldierLook)int.Parse(node["look"].ToString());
		_missions = int.Parse(node["missions"].ToString());
		_kills = int.Parse(node["kills"].ToString());
		_recovery = int.Parse(node["recovery"].ToString());
		Armor armor = mod.getArmor(node["armor"].ToString());
		if (armor == null)
		{
			armor = mod.getArmor(mod.getSoldier(mod.getSoldiersList().First()).getArmor());
		}
		_armor = armor;
		_psiTraining = bool.Parse(node["psiTraining"].ToString());
		_improvement = int.Parse(node["improvement"].ToString());
		_psiStrImprovement = int.Parse(node["psiStrImprovement"].ToString());
		var layout = node["equipmentLayout"] as YamlSequenceNode;
        if (layout != null)
		{
			foreach (var i in layout)
			{
				EquipmentLayoutItem layoutItem = new EquipmentLayoutItem(i);
				if (mod.getInventory(layoutItem.getSlot()) != null)
				{
					_equipmentLayout.Add(layoutItem);
				}
				else
				{
					layoutItem = null;
				}
			}
		}
		if (node["death"] != null)
		{
			_death = new SoldierDeath();
			_death.load(node["death"]);
		}
		if (node["diary"] != null)
		{
			_diary = new SoldierDiary();
			_diary.load(node["diary"], mod);
		}
		calcStatString(mod.getStatStrings(), (Options.psiStrengthEval && save.isResearched(mod.getPsiRequirements())));
	}

	/**
	 * Calculates the soldier's statString
	 * Calculates the soldier's statString.
	 * @param statStrings List of statString rules.
	 * @param psiStrengthEval Are psi stats available?
	 */
	internal void calcStatString(List<StatString> statStrings, bool psiStrengthEval) =>
		_statString = StatString.calcStatString(_currentStats, statStrings, psiStrengthEval, _psiTraining);

	/**
	 * returns whether or not the unit is in psi training
	 * @return true/false
	 */
	internal bool isInPsiTraining() =>
		_psiTraining;

	/**
	 * Returns the soldier's rules.
	 * @return rulesoldier
	 */
	internal RuleSoldier getRules() =>
		_rules;

    /**
     * Trains a soldier's Psychic abilities after 1 month.
     */
    internal void trainPsi()
    {
        int psiSkillCap = _rules.getStatCaps().psiSkill;
        int psiStrengthCap = _rules.getStatCaps().psiStrength;

        _improvement = _psiStrImprovement = 0;
        // -10 days - tolerance threshold for switch from anytimePsiTraining option.
        // If soldier has psiskill -10..-1, he was trained 20..59 days. 81.7% probability, he was trained more that 30 days.
        if (_currentStats.psiSkill < -10 + _rules.getMinStats().psiSkill)
            _currentStats.psiSkill = _rules.getMinStats().psiSkill;
        else if (_currentStats.psiSkill <= _rules.getMaxStats().psiSkill)
        {
            int max = _rules.getMaxStats().psiSkill + _rules.getMaxStats().psiSkill / 2;
            _improvement = RNG.generate(_rules.getMaxStats().psiSkill, max);
        }
        else
        {
            if (_currentStats.psiSkill <= (psiSkillCap / 2)) _improvement = RNG.generate(5, 12);
            else if (_currentStats.psiSkill < psiSkillCap) _improvement = RNG.generate(1, 3);

            if (Options.allowPsiStrengthImprovement)
            {
                if (_currentStats.psiStrength <= (psiStrengthCap / 2)) _psiStrImprovement = RNG.generate(5, 12);
                else if (_currentStats.psiStrength < psiStrengthCap) _psiStrImprovement = RNG.generate(1, 3);
            }
        }
        _currentStats.psiSkill += _improvement;
        _currentStats.psiStrength += _psiStrImprovement;
        if (_currentStats.psiSkill > psiSkillCap) _currentStats.psiSkill = psiSkillCap;
        if (_currentStats.psiStrength > psiStrengthCap) _currentStats.psiStrength = psiStrengthCap;
    }

	/**
	 * Returns the amount of time until the soldier is healed.
	 * @return Number of days.
	 */
	internal int getWoundRecovery() =>
		_recovery;

    /**
     * Heals soldier wounds.
     */
    internal void heal() =>
        _recovery--;

    /**
     * Trains a soldier's Psychic abilities after 1 day.
     * (anytimePsiTraining option)
     */
    internal void trainPsi1Day()
    {
        if (!_psiTraining)
        {
            _improvement = 0;
            return;
        }

        if (_currentStats.psiSkill > 0) // yes, 0. _rules->getMinStats().psiSkill was wrong.
        {
            if (8 * 100 >= _currentStats.psiSkill * RNG.generate(1, 100) && _currentStats.psiSkill < _rules.getStatCaps().psiSkill)
            {
                ++_improvement;
                ++_currentStats.psiSkill;
            }

            if (Options.allowPsiStrengthImprovement)
            {
                if (8 * 100 >= _currentStats.psiStrength * RNG.generate(1, 100) && _currentStats.psiStrength < _rules.getStatCaps().psiStrength)
                {
                    ++_psiStrImprovement;
                    ++_currentStats.psiStrength;
                }
            }
        }
        else if (_currentStats.psiSkill < _rules.getMinStats().psiSkill)
        {
            if (++_currentStats.psiSkill == _rules.getMinStats().psiSkill) // initial training is over
            {
                _improvement = _rules.getMaxStats().psiSkill + RNG.generate(0, _rules.getMaxStats().psiSkill / 2);
                _currentStats.psiSkill = _improvement;
            }
        }
        else // minStats.psiSkill <= 0 && _currentStats.psiSkill == minStats.psiSkill
            _currentStats.psiSkill -= RNG.generate(30, 60);    // set initial training from 30 to 60 days
    }

	/**
	 * Returns the craft the soldier is assigned to.
	 * @return Pointer to craft.
	 */
	internal Craft getCraft() =>
		_craft;

	/**
	 * Returns the soldier's military rank.
	 * @return Rank enum.
	 */
	internal SoldierRank getRank() =>
		_rank;

    /**
     * Returns the list of EquipmentLayoutItems of a soldier.
     * @return Pointer to the EquipmentLayoutItem list.
     */
    internal List<EquipmentLayoutItem> getEquipmentLayout() =>
        _equipmentLayout;

	/**
	 * Returns the soldier's gender.
	 * @return Gender.
	 */
	internal SoldierGender getGender() =>
		_gender;

	/**
	 * Returns the soldier's look.
	 * @return Look.
	 */
	internal SoldierLook getLook() =>
		_look;

	/**
	 * Returns the soldier's amount of missions.
	 * @return Missions.
	 */
	internal int getMissions() =>
		_missions;

	/**
	 * Returns the unit's current armor.
	 * @return Pointer to armor data.
	 */
	internal Armor getArmor() =>
		_armor;

    /**
     * Get pointer to current stats.
     */
    internal UnitStats getCurrentStats() =>
        _currentStats;

	/**
	 * Returns the soldier's unique ID. Each soldier
	 * can be identified by its ID. (not it's name)
	 * @return Unique ID.
	 */
	internal int getId() =>
		_id;

    /**
     * Get pointer to initial stats.
     */
    internal UnitStats getInitStats() =>
        _initialStats;

    /**
     * changes whether or not the unit is in psi training
     */
    internal void setPsiTraining(bool psi) =>
        _psiTraining = psi;

	/**
	 * returns this soldier's psionic strength improvement score for this month.
	 */
	internal int getPsiStrImprovement() =>
		_psiStrImprovement;

	/**
	 * returns this soldier's psionic skill improvement score for this month.
	 * @return score
	 */
	internal int getImprovement() =>
		_improvement;

	/**
	 * Returns a graphic representation of
	 * the soldier's military rank.
	 * @note THE MEANING OF LIFE
	 * @return Sprite ID for rank.
	 */
	internal int getRankSprite() =>
		42 + (int)_rank;

	/**
	 * Returns the soldier's amount of kills.
	 * @return Kills.
	 */
	internal int getKills() =>
		_kills;

	/**
	 * Returns the soldier's death details.
	 * @return Pointer to death data. NULL if no death has occurred.
	 */
	internal SoldierDeath getDeath() =>
		_death;

	/**
	 * Changes the soldier's full name.
	 * @param name Soldier name.
	 */
	internal void setName(string name) =>
		_name = name;

    /**
     * Changes the unit's current armor.
     * @param armor Pointer to armor data.
     */
    internal void setArmor(Armor armor) =>
        _armor = armor;

    /**
     * Returns the soldier's diary.
     * @return Diary.
     */
    internal SoldierDiary getDiary() =>
        _diary;

    /**
     * Returns the unit's promotion status and resets it.
     * @return True if recently promoted, False otherwise.
     */
    internal bool isPromoted()
    {
        bool promoted = _recentlyPromoted;
        _recentlyPromoted = false;
        return promoted;
    }

	/**
	 * Returns the soldier's craft string, which
	 * is either the soldier's wounded status,
	 * the assigned craft name, or none.
	 * @param lang Language to get strings from.
	 * @return Full name.
	 */
	internal string getCraftString(Language lang)
	{
		string s;
		if (_recovery > 0)
		{
			s = lang.getString("STR_WOUNDED");
		}
		else if (_craft == null)
		{
			s = lang.getString("STR_NONE_UC");
		}
		else
		{
			s = _craft.getName(lang);
		}
		return s;
	}

	/**
	 * Increase the soldier's military rank.
	 */
	internal void promoteRank()
	{
		_rank = (SoldierRank)((int)_rank + 1);
		if (_rank > SoldierRank.RANK_SQUADDIE)
		{
			// only promotions above SQUADDIE are worth to be mentioned
			_recentlyPromoted = true;
		}
	}
}
