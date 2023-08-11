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
 * Represents an ongoing alien mission.
 * Contains variable info about the mission, like spawn counter, target region
 * and current wave.
 * @sa RuleAlienMission
 */
internal class AlienMission
{
	RuleAlienMission _rule;
	uint _nextWave;
	uint _nextUfoCounter;
	uint _spawnCountdown;
	uint _liveUfos;
	int _uniqueID, _missionSiteZone;
	AlienBase _base;
	string _region, _race;

	internal AlienMission() { }

	internal AlienMission(RuleAlienMission rule)
	{
		_rule = rule;
		_nextWave = 0;
		_nextUfoCounter = 0;
		_spawnCountdown = 0;
		_liveUfos = 0;
		_uniqueID = 0;
		_missionSiteZone = -1;
		_base = null;
	}

	~AlienMission() { }

	/// Decrease number of live UFOs.
	internal void decreaseLiveUfos() =>
		--_liveUfos;

	/**
     * Saves the alien mission to a YAML file.
     * @return YAML node.
     */
	internal YamlNode save()
	{
		var node = new YamlMappingNode
		{
			{ "type", _rule.getType() },
			{ "region", _region },
			{ "race", _race },
			{ "nextWave", _nextWave.ToString() },
			{ "nextUfoCounter", _nextUfoCounter.ToString() },
			{ "spawnCountdown", _spawnCountdown.ToString() },
			{ "liveUfos", _liveUfos.ToString() },
			{ "uniqueID", _uniqueID.ToString() }
		};
		if (_base != null)
		{
			node.Add("alienBase", _base.saveId());
		}
		node.Add("missionSiteZone", _missionSiteZone.ToString());
		return node;
	}

	/**
     * @return The unique ID assigned to this mission.
     */
	internal int getId()
	{
		Debug.Assert(_uniqueID != 0, "Uninitialized mission!");
		return _uniqueID;
	}

	/// Gets the mission's ruleset.
	internal RuleAlienMission getRules() =>
		_rule;

	/// Increase number of live UFOs.
	internal void increaseLiveUfos() { ++_liveUfos; }

	/**
     * Loads the alien mission from a YAML file.
     * @param node The YAML node containing the data.
     * @param game The game data, required to locate the alien base.
     */
	internal void load(YamlNode node, SavedGame game)
	{
		_region = node["region"].ToString();
		_race = node["race"].ToString();
		_nextWave = uint.Parse(node["nextWave"].ToString());
		_nextUfoCounter = uint.Parse(node["nextUfoCounter"].ToString());
		_spawnCountdown = uint.Parse(node["spawnCountdown"].ToString());
		_liveUfos = uint.Parse(node["liveUfos"].ToString());
		_uniqueID = int.Parse(node["uniqueID"].ToString());
		YamlNode @base = node["alienBase"];
		if (@base != null)
		{
			int id = int.TryParse(@base.ToString(), out int result) ? result : -1;
			string type = "STR_ALIEN_BASE";
			// New format
			if (id == -1)
			{
				id = int.Parse(@base["id"].ToString());
				type = @base["type"].ToString();
			}
			var found = game.getAlienBases().Find(x => x.getId() == id && x.getDeployment().getMarkerName() == type);
			if (found == null)
			{
				throw new Exception("Corrupted save: Invalid base for mission.");
			}
			_base = found;
		}
		_missionSiteZone = int.Parse(node["missionSiteZone"].ToString());
	}

	/**
     * Check if a mission is over and can be safely removed from the game.
     * A mission is over if it will spawn no more UFOs and it has no UFOs still in
     * the game.
     * @return If the mission can be safely removed from game.
     */
	internal bool isOver()
	{
		if (_rule.getObjective() == MissionObjective.OBJECTIVE_INFILTRATION)
		{
			//Infiltrations continue for ever.
			return false;
		}
		if (_nextWave == _rule.getWaveCount() && _liveUfos == 0)
		{
			return true;
		}
		return false;
	}

	/**
     * The new time must be a multiple of 30 minutes, and more than 0.
     * Calling this on a finished mission has no effect.
     * @param minutes The minutes until the next UFO wave will spawn.
     */
	internal void setWaveCountdown(uint minutes)
	{
		Debug.Assert(minutes != 0 && minutes % 30 == 0);
		if (isOver())
		{
			return;
		}
		_spawnCountdown = minutes;
	}

	/**
     * This function is called when one of the mission's UFOs has finished it's time on the ground.
     * It takes care of sending the UFO to the next waypoint and marking them for removal as required.
     * It must set the game data in a way that the rest of the code understands what to do.
     * @param ufo The UFO that reached it's waypoint.
     * @param game The saved game information.
     */
	internal void ufoLifting(Ufo ufo, SavedGame game)
	{
		switch (ufo.getStatus())
		{
			case UfoStatus.FLYING:
				Debug.Assert(false, "Ufo is already on the air!");
				break;
			case UfoStatus.LANDED:
				{
					// base missions only get points when they are completed.
					if (_rule.getPoints() > 0 && _rule.getObjective() != MissionObjective.OBJECTIVE_BASE)
					{
						addScore(ufo.getLongitude(), ufo.getLatitude(), game);
					}
					ufo.setAltitude("STR_VERY_LOW");
					ufo.setSpeed((int)(ufo.getRules().getMaxSpeed() * ufo.getTrajectory().getSpeedPercentage(ufo.getTrajectoryPoint())));
				}
				break;
			case UfoStatus.CRASHED:
				// Mission expired
				ufo.setDetected(false);
				ufo.setStatus(UfoStatus.DESTROYED);
				break;
			case UfoStatus.DESTROYED:
				Debug.Assert(false, "UFO can't fly!");
				break;
		}
	}

	/**
	 * This function is called when one of the mission's UFOs arrives at it's current destination.
	 * It takes care of sending the UFO to the next waypoint, landing UFOs and
	 * marking them for removal as required. It must set the game data in a way that the rest of the code
	 * understands what to do.
	 * @param ufo The UFO that reached it's waypoint.
	 * @param engine The game engine, required to get access to game data and game rules.
	 * @param globe The earth globe, required to get access to land checks.
	 */
	internal void ufoReachedWaypoint(Ufo ufo, Game engine, Globe globe)
	{
		Mod.Mod mod = engine.getMod();
		SavedGame game = engine.getSavedGame();
		uint curWaypoint = ufo.getTrajectoryPoint();
		uint nextWaypoint = curWaypoint + 1;
		UfoTrajectory trajectory = ufo.getTrajectory();
		int waveNumber = (int)(_nextWave - 1);
		if (waveNumber < 0)
		{
			waveNumber = _rule.getWaveCount() - 1;
		}

		MissionWave wave = _rule.getWave((uint)waveNumber);
		if (nextWaypoint >= trajectory.getWaypointCount())
		{
			ufo.setDetected(false);
			ufo.setStatus(UfoStatus.DESTROYED);
			return;
		}
		ufo.setAltitude(trajectory.getAltitude(nextWaypoint));
		ufo.setTrajectoryPoint(nextWaypoint);
		RuleRegion regionRules = mod.getRegion(_region, true);
		KeyValuePair<double, double> pos = getWaypoint(trajectory, nextWaypoint, globe, regionRules);

		Waypoint wp = new Waypoint();
		wp.setLongitude(pos.Key);
		wp.setLatitude(pos.Value);
		ufo.setDestination(wp);
		if (ufo.getAltitude() != "STR_GROUND")
		{
			if (ufo.getLandId() != 0)
			{
				ufo.setLandId(0);
			}
			// Set next waypoint.
			ufo.setSpeed((int)(ufo.getRules().getMaxSpeed() * trajectory.getSpeedPercentage(nextWaypoint)));
		}
		else
		{
			// UFO landed.
			if (_missionSiteZone != -1 && wave.objective && trajectory.getZone(curWaypoint) == (uint)(_rule.getSpawnZone()))
			{
				// Remove UFO, replace with MissionSite.
				addScore(ufo.getLongitude(), ufo.getLatitude(), game);
				ufo.setStatus(UfoStatus.DESTROYED);

				MissionArea area = regionRules.getMissionZones()[(int)trajectory.getZone(curWaypoint)].areas[_missionSiteZone];
				Texture texture = mod.getGlobe().getTexture(area.texture);
				AlienDeployment deployment;
				if (mod.getDeployment(_rule.getSiteType()) != null)
				{
					deployment = mod.getDeployment(_rule.getSiteType());
				}
				else
				{
					if (texture == null)
					{
						throw new Exception("Error occurred while spawning mission site: " + _rule.getType());
					}
					deployment = mod.getDeployment(texture.getRandomDeployment(), true);
				}
				MissionSite missionSite = spawnMissionSite(game, deployment, area);
				if (missionSite != null)
				{
					List<Craft> followers = ufo.getCraftFollowers();
					foreach (var c in followers)
					{
						if (c.getNumSoldiers() != 0)
						{
							c.setDestination(missionSite);
						}
					}
				}
			}
			else if (trajectory.getID() == UfoTrajectory.RETALIATION_ASSAULT_RUN)
			{
				// Ignore what the trajectory might say, this is a base assault.
				// Remove UFO, replace with Base defense.
				ufo.setDetected(false);
				Base found = game.getBases().Find(x => AreSame(x.getLongitude(), ufo.getLongitude()) && AreSame(x.getLatitude(), ufo.getLatitude()));
				if (found == null)
				{
					ufo.setStatus(UfoStatus.DESTROYED);
					// Only spawn mission if the base is still there.
					return;
				}
				ufo.setDestination(found);
			}
			else
			{
				if (globe.insideLand(ufo.getLongitude(), ufo.getLatitude()))
				{
					// Set timer for UFO on the ground.
					ufo.setSecondsRemaining(trajectory.groundTimer() * 5);
					if (ufo.getDetected() && ufo.getLandId() == 0)
					{
						ufo.setLandId(engine.getSavedGame().getId("STR_LANDING_SITE"));
					}
				}
				else
				{
					// There's nothing to land on
					ufo.setSecondsRemaining(5);
				}
			}
		}
	}

	/**
	 * Add alien points to the country and region at the coordinates given.
	 * @param lon Longitudinal coordinates to check.
	 * @param lat Latitudinal coordinates to check.
	 * @param game The saved game information.
	 */
	void addScore(double lon, double lat, SavedGame game)
	{
		if (_rule.getObjective() == MissionObjective.OBJECTIVE_INFILTRATION)
			return; // pact score is a special case
		foreach (var region in game.getRegions())
		{
			if (region.getRules().insideRegion(lon, lat))
			{
				region.addActivityAlien(_rule.getPoints());
				break;
			}
		}
		foreach (var country in game.getCountries())
		{
			if (country.getRules().insideCountry(lon, lat))
			{
				country.addActivityAlien(_rule.getPoints());
				break;
			}
		}
	}

	/**
	 * Select a destination based on the criteria of our trajectory and desired waypoint.
	 * @param trajectory the trajectory in question.
	 * @param nextWaypoint the next logical waypoint in sequence (0 for newly spawned UFOs)
	 * @param globe The earth globe, required to get access to land checks.
	 * @param region the ruleset for the region of our mission.
	 * @return a set of lon and lat coordinates based on the criteria of the trajectory.
	 */
	KeyValuePair<double, double> getWaypoint(UfoTrajectory trajectory, uint nextWaypoint, Globe globe, RuleRegion region)
	{
		int waveNumber = (int)(_nextWave - 1);
		if (waveNumber < 0)
		{
			waveNumber = _rule.getWaveCount() - 1;
		}
		if (trajectory.getZone(nextWaypoint) >= region.getMissionZones().Count)
		{
			logMissionError(trajectory.getZone(nextWaypoint), region);
		}

		if (_missionSiteZone != -1 && _rule.getWave((uint)waveNumber).objective && trajectory.getZone(nextWaypoint) == (uint)(_rule.getSpawnZone()))
		{
			MissionArea area = region.getMissionZones()[_rule.getSpawnZone()].areas[_missionSiteZone];
			return KeyValuePair.Create(area.lonMin, area.latMin);
		}

		if (trajectory.getWaypointCount() > nextWaypoint + 1 && trajectory.getAltitude(nextWaypoint + 1) == "STR_GROUND")
		{
			return getLandPoint(globe, region, trajectory.getZone(nextWaypoint));
		}
		return region.getRandomPoint(trajectory.getZone(nextWaypoint));
	}

	/**
	 * Get a random point inside the given region zone.
	 * The point will be used to land a UFO, so it HAS to be on land (UNLESS it's landing on a city).
	 * @param globe reference to the globe data.
	 * @param region reference to the region we want a land point in.
	 * @param zone the missionZone set within the region to find a landing zone in.
	 * @return a set of longitudinal and latitudinal coordinates.
	 */
	KeyValuePair<double, double> getLandPoint(Globe globe, RuleRegion region, uint zone)
	{
		if (zone >= region.getMissionZones().Count || region.getMissionZones()[(int)zone].areas.Count == 0)
		{
			logMissionError(zone, region);
		}

		KeyValuePair<double, double> pos;

		if (region.getMissionZones()[(int)zone].areas[0].isPoint()) // if a UFO wants to land on a city, let it.
		{
			pos = region.getRandomPoint(zone);
		}
		else
		{
			int tries = 0;
			do
			{
				pos = region.getRandomPoint(zone);
				++tries;
			}
			while (!(globe.insideLand(pos.Key, pos.Value)
				&& region.insideRegion(pos.Key, pos.Value))
				&& tries < 100);

			if (tries == 100)
			{
				Console.WriteLine($"{Log(SeverityLevel.LOG_DEBUG)} Region: {region.getType()} Longitude: {pos.Key} Latitude: {pos.Value} invalid zone: {zone} ufo forced to land on water!");
			}
		}
		return pos;
	}

	void logMissionError(uint zone, RuleRegion region)
	{
		if (region.getMissionZones().Any())
		{
			string ss = zone.ToString();
			string ss2 = (region.getMissionZones().Count - 1).ToString();
			throw new Exception("Error occurred while trying to determine waypoint for mission type: " + _rule.getType() + " in region: " + region.getType() + ", mission tried to find a waypoint in zone " + ss + " but this region only has zones valid up to " + ss2 + ".");
		}
		else
		{
			throw new Exception("Error occurred while trying to determine waypoint for mission type: " + _rule.getType() + " in region: " + region.getType() + ", region has no valid zones.");
		}
	}

	/**
	 * Attempt to spawn a Mission Site at a given location.
	 * @param game reference to the saved game.
	 * @param rules reference to the game rules.
	 * @param area the point on the globe at which to spawn this site.
	 * @return a pointer to the mission site.
	 */
	MissionSite spawnMissionSite(SavedGame game, AlienDeployment deployment, MissionArea area)
	{
		if (deployment != null)
		{
			MissionSite missionSite = new MissionSite(_rule, deployment);
			missionSite.setLongitude(RNG.generate(area.lonMin, area.lonMax));
			missionSite.setLatitude(RNG.generate(area.latMin, area.latMax));
			missionSite.setId(game.getId(deployment.getMarkerName()));
			missionSite.setSecondsRemaining((uint)(RNG.generate(deployment.getDurationMin(), deployment.getDurationMax()) * 3600));
			missionSite.setAlienRace(_race);
			missionSite.setTexture(area.texture);
			missionSite.setCity(area.name);
			game.getMissionSites().Add(missionSite);
			return missionSite;
		}
		return null;
	}

	/// Gets the mission's race.
	internal string getRace() =>
		_race;

	internal void think(Game engine, Globe globe)
	{
		Mod.Mod mod = engine.getMod();
		SavedGame game = engine.getSavedGame();
		if (_nextWave >= _rule.getWaveCount())
			return;
		if (_spawnCountdown > 30)
		{
			_spawnCountdown -= 30;
			return;
		}
		MissionWave wave = _rule.getWave(_nextWave);
		UfoTrajectory trajectory = mod.getUfoTrajectory(wave.trajectory, true);
		Ufo ufo = spawnUfo(game, mod, globe, wave, trajectory);
		if (ufo != null)
		{
			//Some missions may not spawn a UFO!
			game.getUfos().Add(ufo);
		}
		else if ((mod.getDeployment(wave.ufoType) != null && mod.getUfo(wave.ufoType) == null && !string.IsNullOrEmpty(mod.getDeployment(wave.ufoType).getMarkerName())) // a mission site that we want to spawn directly
				|| (_rule.getObjective() == MissionObjective.OBJECTIVE_SITE && wave.objective)) // or we want to spawn one at random according to our terrain
		{
			int index = (_rule.getSpawnZone() == -1) ? (int)trajectory.getZone(0) : _rule.getSpawnZone();
			List<MissionArea> areas = mod.getRegion(_region, true).getMissionZones()[index].areas;
			MissionArea area = areas[(_missionSiteZone == -1) ? RNG.generate(0, areas.Count - 1) : _missionSiteZone];
			Texture texture = mod.getGlobe().getTexture(area.texture);
			AlienDeployment deployment;
			if (mod.getDeployment(wave.ufoType) != null)
			{
				deployment = mod.getDeployment(wave.ufoType);
			}
			else
			{
				if (texture == null)
				{
					throw new Exception("Error occurred while spawning mission site: " + _rule.getType());
				}
				deployment = mod.getDeployment(texture.getRandomDeployment(), true);
			}
			spawnMissionSite(game, deployment, area);
		}

		++_nextUfoCounter;
		if (_nextUfoCounter >= wave.ufoCount)
		{
			_nextUfoCounter = 0;
			++_nextWave;
		}
		if (_rule.getObjective() == MissionObjective.OBJECTIVE_INFILTRATION && _nextWave == _rule.getWaveCount())
		{
			foreach (var c in game.getCountries())
			{
				RuleRegion region = mod.getRegion(_region, true);
				if (!c.getPact() && !c.getNewPact() && region.insideRegion(c.getRules().getLabelLongitude(), c.getRules().getLabelLatitude()))
				{
					c.setNewPact();
					List<MissionArea> areas = region.getMissionZones()[_rule.getSpawnZone()].areas;
					MissionArea area;
					KeyValuePair<double, double> pos;
					int tries = 0;
					do
					{
						area = areas[RNG.generate(0, areas.Count - 1)];
						pos = KeyValuePair.Create(
							RNG.generate(Math.Min(area.lonMin, area.lonMax), Math.Max(area.lonMin, area.lonMax)),
							RNG.generate(Math.Min(area.latMin, area.latMax), Math.Max(area.latMin, area.latMax))
						);
						++tries;
					}
					while (!(globe.insideLand(pos.Key, pos.Value)
						&& region.insideRegion(pos.Key, pos.Value))
						&& tries < 100);
					spawnAlienBase(engine, area, pos);
					break;
				}
			}
			// Infiltrations loop for ever.
			_nextWave = 0;
		}
		if (_rule.getObjective() == MissionObjective.OBJECTIVE_BASE && _nextWave == _rule.getWaveCount())
		{
			RuleRegion region = mod.getRegion(_region, true);
			List<MissionArea> areas = region.getMissionZones()[_rule.getSpawnZone()].areas;
			MissionArea area;
			KeyValuePair<double, double> pos;
			int tries = 0;
			do
			{
				area = areas[RNG.generate(0, areas.Count - 1)];
				pos = KeyValuePair.Create(
					RNG.generate(Math.Min(area.lonMin, area.lonMax), Math.Max(area.lonMin, area.lonMax)),
					RNG.generate(Math.Min(area.latMin, area.latMax), Math.Max(area.latMin, area.latMax))
				);
				++tries;
			}
			while (!(globe.insideLand(pos.Key, pos.Value)
				&& region.insideRegion(pos.Key, pos.Value))
				&& tries < 100);
			spawnAlienBase(engine, area, pos);
		}

		if (_nextWave != _rule.getWaveCount())
		{
			uint spawnTimer = _rule.getWave(_nextWave).spawnTimer / 30;
			_spawnCountdown = (uint)((spawnTimer / 2 + RNG.generate(0, spawnTimer)) * 30);
		}
	}

	/**
	 * Spawn an alien base.
	 * @param engine The game engine, required to get access to game data and game rules.
	 * @param zone The mission zone, required for determining the base coordinates.
	 */
	void spawnAlienBase(Game engine, MissionArea area, KeyValuePair<double, double> pos)
	{
		SavedGame game = engine.getSavedGame();
		Mod.Mod ruleset = engine.getMod();
		// Once the last UFO is spawned, the aliens build their base.
		AlienDeployment deployment;
		Texture texture = ruleset.getGlobe().getTexture(area.texture);
		if (ruleset.getDeployment(_rule.getSiteType()) != null)
		{
			deployment = ruleset.getDeployment(_rule.getSiteType());
		}
		else if (texture != null && texture.getDeployments().Any())
		{
			deployment = ruleset.getDeployment(texture.getRandomDeployment(), true);
		}
		else
		{
			deployment = ruleset.getDeployment("STR_ALIEN_BASE_ASSAULT", true);
		}
		AlienBase ab = new AlienBase(deployment);
		ab.setAlienRace(_race);
		ab.setId(game.getId(deployment.getMarkerName()));
		ab.setLongitude(pos.Key);
		ab.setLatitude(pos.Value);
		game.getAlienBases().Add(ab);
		addScore(ab.getLongitude(), ab.getLatitude(), game);
	}

	/**
	 * This function will spawn a UFO according the mission rules.
	 * Some code is duplicated between cases, that's ok for now. It's on different
	 * code paths and the function is MUCH easier to read written this way.
	 * @param game The saved game information.
	 * @param mod The mod.
	 * @param globe The globe, for land checks.
	 * @param wave The wave for the desired UFO.
	 * @param trajectory The rule for the desired trajectory.
	 * @return Pointer to the spawned UFO. If the mission does not desire to spawn a UFO, 0 is returned.
	 */
	Ufo spawnUfo(SavedGame game, Mod.Mod mod, Globe globe, MissionWave wave, UfoTrajectory trajectory)
	{
		RuleRegion regionRules;
		Ufo ufo;
		Waypoint wp;
		KeyValuePair<double, double> pos;
		RuleUfo ufoRule = mod.getUfo(wave.ufoType);
		if (_rule.getObjective() == MissionObjective.OBJECTIVE_RETALIATION)
		{
			regionRules = mod.getRegion(_region, true);
			Base found = game.getBases().Find(x => regionRules.insideRegion(x.getLongitude(), x.getLatitude()) && x.getRetaliationTarget());
			if (found != null)
			{
				// Spawn a battleship straight for the XCOM base.
				RuleUfo battleshipRule = mod.getUfo(_rule.getSpawnUfo(), true);
				UfoTrajectory assaultTrajectory = mod.getUfoTrajectory(UfoTrajectory.RETALIATION_ASSAULT_RUN, true);
				ufo = new Ufo(battleshipRule);
				ufo.setMissionInfo(this, assaultTrajectory);
				if (trajectory.getAltitude(0) == "STR_GROUND")
				{
					pos = getLandPoint(globe, regionRules, trajectory.getZone(0));
				}
				else
				{
					pos = regionRules.getRandomPoint(trajectory.getZone(0));
				}
				ufo.setAltitude(assaultTrajectory.getAltitude(0));
				ufo.setSpeed((int)(assaultTrajectory.getSpeedPercentage(0) * battleshipRule.getMaxSpeed()));
				ufo.setLongitude(pos.Key);
				ufo.setLatitude(pos.Value);
				wp = new Waypoint();
				wp.setLongitude(found.getLongitude());
				wp.setLatitude(found.getLatitude());
				ufo.setDestination(wp);
				return ufo;
			}
		}
		else if (_rule.getObjective() == MissionObjective.OBJECTIVE_SUPPLY)
		{
			if (ufoRule == null || (wave.objective && _base == null))
			{
				// No base to supply!
				return null;
			}
			// Our destination is always an alien base.
			ufo = new Ufo(ufoRule);
			ufo.setMissionInfo(this, trajectory);
			regionRules = mod.getRegion(_region, true);
			if (trajectory.getAltitude(0) == "STR_GROUND")
			{
				pos = getLandPoint(globe, regionRules, trajectory.getZone(0));
			}
			else
			{
				pos = regionRules.getRandomPoint(trajectory.getZone(0));
			}
			ufo.setAltitude(trajectory.getAltitude(0));
			ufo.setSpeed((int)(trajectory.getSpeedPercentage(0) * ufoRule.getMaxSpeed()));
			ufo.setLongitude(pos.Key);
			ufo.setLatitude(pos.Value);
			wp = new Waypoint();
			if (trajectory.getAltitude(1) == "STR_GROUND")
			{
				if (wave.objective)
				{
					// Supply ships on supply missions land on bases, ignore trajectory zone.
					pos = KeyValuePair.Create(_base.getLongitude(), _base.getLatitude());
				}
				else
				{
					// Other ships can land where they want.
					pos = getLandPoint(globe, regionRules, trajectory.getZone(1));
				}
			}
			else
			{
				pos = regionRules.getRandomPoint(trajectory.getZone(1));
			}
			wp.setLongitude(pos.Key);
			wp.setLatitude(pos.Value);
			ufo.setDestination(wp);
			return ufo;
		}
		if (ufoRule == null)
			return null;
		// Spawn according to sequence.
		ufo = new Ufo(ufoRule);
		ufo.setMissionInfo(this, trajectory);
		regionRules = mod.getRegion(_region, true);
		pos = getWaypoint(trajectory, 0, globe, regionRules);
		ufo.setAltitude(trajectory.getAltitude(0));
		if (trajectory.getAltitude(0) == "STR_GROUND")
		{
			ufo.setSecondsRemaining(trajectory.groundTimer() * 5);
		}
		ufo.setSpeed((int)(trajectory.getSpeedPercentage(0) * ufoRule.getMaxSpeed()));
		ufo.setLongitude(pos.Key);
		ufo.setLatitude(pos.Value);
		wp = new Waypoint();
		pos = getWaypoint(trajectory, 1, globe, regionRules);
		wp.setLongitude(pos.Key);
		wp.setLatitude(pos.Value);
		ufo.setDestination(wp);
		return ufo;
	}

    /// Gets the mission's region.
    internal string getRegion() =>
		_region;

    /// Sets the mission's race.
    internal void setRace(string race) =>
		_race = race;

    /**
     * Assigns a unique ID to this mission.
     * It is an error to assign two IDs to the same mission.
     * @param id The UD to assign.
     */
    internal void setId(int id)
    {
        Debug.Assert(_uniqueID == 0, "Reassigning ID!");
        _uniqueID = id;
    }

	/*
	 * Sets the mission's region. if the region is incompatible with
	 * actually carrying out an attack, use the "fallback" region as
	 * defined in the ruleset.
	 * (this is a slight difference from the original, which just
	 * defaulted them to zone[0], North America)
	 * @param region the region we want to try to set the mission to.
	 * @param mod the mod, in case we need to swap out the region.
	 */
	internal void setRegion(string region, Mod.Mod mod)
	{
		RuleRegion r = mod.getRegion(region, true);
		if (!string.IsNullOrEmpty(r.getMissionRegion()))
		{
			_region = r.getMissionRegion();
		}
		else
		{
			_region = region;
		}
	}

    /**
     * Tell the mission which entry in the zone array we're targetting for our missionSite payload.
     * @param zone the number of the zone to target, synonymous with a city.
     */
    internal void setMissionSiteZone(int zone) =>
        _missionSiteZone = zone;

    internal void start(uint initialCount = 0)
    {
        _nextWave = 0;
        _nextUfoCounter = 0;
        _liveUfos = 0;
        if (initialCount == 0)
        {
            uint spawnTimer = _rule.getWave(0).spawnTimer / 30;
            _spawnCountdown = (uint)((spawnTimer / 2 + RNG.generate(0, spawnTimer)) * 30);
        }
        else
        {
            _spawnCountdown = initialCount;
        }
    }

	/**
	 * Sets the alien base associated with this mission.
	 * Only the alien supply missions care about this.
	 * @param base A pointer to an alien base.
	 */
	internal void setAlienBase(AlienBase @base) =>
		_base = @base;

	/**
	 * This function is called when one of the mission's UFOs is shot down (crashed or destroyed).
	 * Currently the only thing that happens is delaying the next UFO in the mission sequence.
	 * @param ufo The UFO that was shot down.
	 */
	internal void ufoShotDown(Ufo ufo)
	{
		switch (ufo.getStatus())
		{
			case UfoStatus.FLYING:
			case UfoStatus.LANDED:
				Debug.Assert(false, "Ufo seems ok!");
				break;
			case UfoStatus.CRASHED:
			case UfoStatus.DESTROYED:
				if (_nextWave != _rule.getWaveCount())
				{
					// Delay next wave
					_spawnCountdown += (uint)(30 * (RNG.generate(0, 400) + 48));
				}
				break;
		}
	}
}
