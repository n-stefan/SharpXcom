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
    Soldier(RuleSoldier rules, Armor armor, int id)
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
}
