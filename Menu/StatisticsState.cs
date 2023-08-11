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
 * Statistics window that shows up
 * at the end of the game.
 */
internal class StatisticsState : State
{
	Window _window;
	TextButton _btnOk;
	Text _txtTitle;
	TextList _lstStats;

	/**
	 * Initializes all the elements in the Statistics window.
	 * @param game Pointer to the core game.
	 */
	internal StatisticsState()
	{
		// Create objects
		_window = new Window(this, 320, 200, 0, 0, WindowPopup.POPUP_BOTH);
		_btnOk = new TextButton(50, 12, 135, 180);
		_txtTitle = new Text(310, 25, 5, 8);
		_lstStats = new TextList(280, 136, 12, 36);

		// Set palette
		setInterface("endGameStatistics");

		add(_window, "window", "endGameStatistics");
		add(_btnOk, "button", "endGameStatistics");
		add(_txtTitle, "text", "endGameStatistics");
		add(_lstStats, "list", "endGameStatistics");

		centerAllSurfaces();

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);

		_txtTitle.setBig();
		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);

		_lstStats.setColumns(2, 200, 80);
		_lstStats.setDot(true);

		listStats();
	}

	/**
	 *
	 */
	~StatisticsState() { }

	/**
	 * Returns to the previous screen.
	 * @param action Pointer to an action.
	 */
	void btnOkClick(Action _)
	{
		if (_game.getSavedGame().getEnding() == GameEnding.END_NONE)
		{
			_game.popState();
		}
		else
		{
			_game.setSavedGame(null);
			_game.setState(new GoToMainMenuState());
		}
	}

	void listStats()
	{
		SavedGame save = _game.getSavedGame();

		var ss = new StringBuilder();
		GameTime time = save.getTime();
		if (save.getEnding() == GameEnding.END_WIN)
		{
			ss.Append(tr("STR_VICTORY"));
		}
		else if (save.getEnding() == GameEnding.END_LOSE)
		{
			ss.Append(tr("STR_DEFEAT"));
		}
		else
		{
			ss.Append(tr("STR_STATISTICS"));
		}
		ss.Append($"{Unicode.TOK_NL_SMALL}{time.getDayString(_game.getLanguage())} {tr(time.getMonthString())} {time.getYear()}");
		_txtTitle.setText(ss.ToString());

		int totalScore = save.getResearchScores().Sum();
		foreach (var iter in save.getRegions())
		{
			totalScore += iter.getActivityXcom().Sum() - iter.getActivityAlien().Sum();
		}

		int monthlyScore = totalScore / (int)save.getResearchScores().Count;
		long totalIncome = save.getIncomes().Sum();
		long totalExpenses = save.getExpenditures().Sum();

		int alienBasesDestroyed = 0, xcomBasesLost = 0;
		int missionsWin = 0, missionsLoss = 0, nightMissions = 0;
		int bestScore = -9999, worstScore = 9999;
		foreach (var i in save.getMissionStatistics())
		{
			if (i.success)
			{
				missionsWin++;
			}
			else
			{
				missionsLoss++;
			}
			bestScore = Math.Max(bestScore, i.score);
			worstScore = Math.Min(worstScore, i.score);
			if (i.isDarkness())
			{
				nightMissions++;
			}
			if (i.isAlienBase() && i.success)
			{
				alienBasesDestroyed++;
			}
			if (i.isBaseDefense() && !i.success)
			{
				xcomBasesLost++;
			}
		}
		// Make sure dummy values aren't left in
		bestScore = (bestScore == -9999) ? 0 : bestScore;
		worstScore = (worstScore == 9999) ? 0 : worstScore;

		var allSoldiers = new List<Soldier>();
		allSoldiers.AddRange(save.getSoldiers());
		allSoldiers.AddRange(save.getDeadSoldiers());
		int soldiersRecruited = allSoldiers.Count;
		int soldiersLost = save.getDeadSoldiers().Count;

		int aliensKilled = 0, aliensCaptured = 0, friendlyKills = 0;
		int daysWounded = 0, longestMonths = 0;
		int shotsFired = 0, shotsLanded = 0;
		Dictionary<string, int> weaponKills = new(), alienKills = new();
		foreach (var i in allSoldiers)
		{
			SoldierDiary diary = i.getDiary();
			aliensKilled += diary.getKillTotal();
			aliensCaptured += diary.getStunTotal();
			daysWounded += diary.getDaysWoundedTotal();
			longestMonths = Math.Max(longestMonths, diary.getMonthsService());
			Dictionary<string, int> weaponTotal = diary.getWeaponTotal();
			shotsFired += diary.getShotsFiredTotal();
			shotsLanded += diary.getShotsLandedTotal();
			foreach (var j in weaponTotal)
			{
				if (!weaponKills.ContainsKey(j.Key))
				{
					weaponKills[j.Key] = j.Value;
				}
				else
				{
					weaponKills[j.Key] += j.Value;
				}
			}

			if (i.getDeath() != null && i.getDeath().getCause() != default)
			{
				BattleUnitKills kills = i.getDeath().getCause();
				if (kills.faction == UnitFaction.FACTION_PLAYER)
				{
					friendlyKills++;
				}
				if (!string.IsNullOrEmpty(kills.race))
				{
					if (!alienKills.ContainsKey(kills.race))
					{
						alienKills[kills.race] = 1;
					}
					else
					{
						alienKills[kills.race] += 1;
					}
				}
			}
		}
		int accuracy = 0;
		if (shotsFired > 0)
		{
			accuracy = 100 * shotsLanded / shotsFired;
		}

		int maxWeapon = 0;
		string highestWeapon = "STR_NONE";
		foreach (var i in weaponKills)
		{
			if (i.Value > maxWeapon)
			{
				maxWeapon = i.Value;
				highestWeapon = i.Key;
			}
		}
		int maxAlien = 0;
		string highestAlien = "STR_NONE";
		foreach (var i in alienKills)
		{
			if (i.Value > maxAlien)
			{
				maxAlien = i.Value;
				highestAlien = i.Key;
			}
		}

		Dictionary<string, int> ids = save.getAllIds();
		int alienBases = alienBasesDestroyed;
		foreach (var i in save.getAlienBases())
		{
			if (i.isDiscovered())
			{
				alienBases++;
			}
		}
		int ufosDetected = Math.Max(0, ids["STR_UFO"] - 1);
		int terrorSites = Math.Max(0, ids["STR_TERROR_SITE"] - 1);
		int totalCrafts = 0;
		foreach (var i in _game.getMod().getCraftsList())
		{
			totalCrafts += Math.Max(0, ids[i] - 1);
		}

		int xcomBases = save.getBases().Count + xcomBasesLost;
		int currentScientists = 0, currentEngineers = 0;
		foreach (var i in save.getBases())
		{
			currentScientists += i.getTotalScientists();
			currentEngineers += i.getTotalEngineers();
		}

		int countriesLost = 0;
		foreach (var i in save.getCountries())
		{
			if (i.getPact())
			{
				countriesLost++;
			}
		}

		int researchDone = save.getDiscoveredResearch().Count;

		string[] difficulty = { "STR_1_BEGINNER", "STR_2_EXPERIENCED", "STR_3_VETERAN", "STR_4_GENIUS", "STR_5_SUPERHUMAN" };

		_lstStats.addRow(2, tr("STR_DIFFICULTY"), tr(difficulty[(int)save.getDifficulty()]));
		_lstStats.addRow(2, tr("STR_AVERAGE_MONTHLY_RATING"), Unicode.formatNumber(monthlyScore));
		_lstStats.addRow(2, tr("STR_TOTAL_INCOME"), Unicode.formatFunding(totalIncome));
		_lstStats.addRow(2, tr("STR_TOTAL_EXPENDITURE"), Unicode.formatFunding(totalExpenses));
		_lstStats.addRow(2, tr("STR_MISSIONS_WON"), Unicode.formatNumber(missionsWin));
		_lstStats.addRow(2, tr("STR_MISSIONS_LOST"), Unicode.formatNumber(missionsLoss));
		_lstStats.addRow(2, tr("STR_NIGHT_MISSIONS"), Unicode.formatNumber(nightMissions));
		_lstStats.addRow(2, tr("STR_BEST_RATING"), Unicode.formatNumber(bestScore));
		_lstStats.addRow(2, tr("STR_WORST_RATING"), Unicode.formatNumber(worstScore));
		_lstStats.addRow(2, tr("STR_SOLDIERS_RECRUITED"), Unicode.formatNumber(soldiersRecruited));
		_lstStats.addRow(2, tr("STR_SOLDIERS_LOST"), Unicode.formatNumber(soldiersLost));
		_lstStats.addRow(2, tr("STR_ALIEN_KILLS"), Unicode.formatNumber(aliensKilled));
		_lstStats.addRow(2, tr("STR_ALIEN_CAPTURES"), Unicode.formatNumber(aliensCaptured));
		_lstStats.addRow(2, tr("STR_FRIENDLY_KILLS"), Unicode.formatNumber(friendlyKills));
		_lstStats.addRow(2, tr("STR_AVERAGE_ACCURACY"), Unicode.formatPercentage(accuracy));
		_lstStats.addRow(2, tr("STR_WEAPON_MOST_KILLS"), tr(highestWeapon));
		_lstStats.addRow(2, tr("STR_ALIEN_MOST_KILLS"), tr(highestAlien));
		_lstStats.addRow(2, tr("STR_LONGEST_SERVICE"), Unicode.formatNumber(longestMonths));
		_lstStats.addRow(2, tr("STR_TOTAL_DAYS_WOUNDED"), Unicode.formatNumber(daysWounded));
		_lstStats.addRow(2, tr("STR_TOTAL_UFOS"), Unicode.formatNumber(ufosDetected));
		_lstStats.addRow(2, tr("STR_TOTAL_ALIEN_BASES"), Unicode.formatNumber(alienBases));
		_lstStats.addRow(2, tr("STR_COUNTRIES_LOST"), Unicode.formatNumber(countriesLost));
		_lstStats.addRow(2, tr("STR_TOTAL_TERROR_SITES"), Unicode.formatNumber(terrorSites));
		_lstStats.addRow(2, tr("STR_TOTAL_BASES"), Unicode.formatNumber(xcomBases));
		_lstStats.addRow(2, tr("STR_TOTAL_CRAFT"), Unicode.formatNumber(totalCrafts));
		_lstStats.addRow(2, tr("STR_TOTAL_SCIENTISTS"), Unicode.formatNumber(currentScientists));
		_lstStats.addRow(2, tr("STR_TOTAL_ENGINEERS"), Unicode.formatNumber(currentEngineers));
		_lstStats.addRow(2, tr("STR_TOTAL_RESEARCH"), Unicode.formatNumber(researchDone));
	}
}
