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
 * Container for battle unit statistics.
 */
struct BattleUnitStatistics
{
    // Variables
    internal bool wasUnconcious;         ///< Tracks if the soldier fell unconcious
	internal int shotAtCounter;          ///< Tracks how many times the unit was shot at
	internal int hitCounter;             ///< Tracks how many times the unit was hit
	internal int shotByFriendlyCounter;  ///< Tracks how many times the unit was hit by a friendly
	internal int shotFriendlyCounter;    ///< Tracks how many times the unit was hit a friendly
	internal bool loneSurvivor;          ///< Tracks if the soldier was the only survivor
	internal bool ironMan;               ///< Tracks if the soldier was the only soldier on the mission
	internal int longDistanceHitCounter; ///< Tracks how many long distance shots were landed
	internal int lowAccuracyHitCounter;  ///< Tracks how many times the unit landed a low probability shot
	internal int shotsFiredCounter;      ///< Tracks how many times a unit has shot
	internal int shotsLandedCounter;     ///< Tracks how many times a unit has hit his target
	internal List<BattleUnitKills> kills;///< Tracks kills
	internal int daysWounded;            ///< Tracks how many days the unit was wounded for
	internal bool KIA;                   ///< Tracks if the soldier was killed in battle
	internal bool nikeCross;             ///< Tracks if a soldier killed every alien or killed and stunned every alien
	internal bool mercyCross;            ///< Tracks if a soldier stunned every alien
	internal int woundsHealed;           ///< Tracks how many times a fatal wound was healed by this unit
	internal UnitStats delta;            ///< Tracks the increase in unit stats (is not saved, only used during debriefing)
	internal int appliedStimulant;       ///< Tracks how many times this soldier applied stimulant
	internal int appliedPainKill;        ///< Tracks how many times this soldier applied pain killers
	internal int revivedSoldier;         ///< Tracks how many times this soldier revived another soldier
	internal int revivedHostile;         ///< Tracks how many times this soldier revived another hostile
	internal int revivedNeutral;         ///< Tracks how many times this soldier revived another civilian
	internal bool MIA;                   ///< Tracks if the soldier was left behind :(
	internal int martyr;                 ///< Tracks how many kills the soldier landed on the turn of his death
	internal int slaveKills;             ///< Tracks how many kills the soldier landed thanks to a mind controlled unit.

    public BattleUnitStatistics()
    {
        wasUnconcious = false;
        shotAtCounter = 0;
        hitCounter = 0;
        shotByFriendlyCounter = 0;
        shotFriendlyCounter = 0;
        loneSurvivor = false;
        ironMan = false;
        longDistanceHitCounter = 0;
        lowAccuracyHitCounter = 0;
        shotsFiredCounter = 0;
        shotsLandedCounter = 0;
        kills = null;
        daysWounded = 0;
        KIA = false;
        nikeCross = false;
        mercyCross = false;
        woundsHealed = 0;
        appliedStimulant = 0;
        appliedPainKill = 0;
        revivedSoldier = 0;
        revivedHostile = 0;
        revivedNeutral = 0;
        MIA = false;
        martyr = 0;
        slaveKills = 0;
    }

    /// Duplicate entry check
    internal bool duplicateEntry(UnitStatus status, int id)
	{
		foreach (var kill in kills)
		{
			if (kill.id == id && kill.status == status)
			{
				return true;
			}
		}
		return false;
	}

	/// Save function
	internal YamlNode save()
	{
        var node = new YamlMappingNode
        {
            { "wasUnconcious", wasUnconcious.ToString() }
        };
        if (kills.Any())
		{
            node.Add("kills", new YamlSequenceNode(kills.Select(x => x.save())));
		}
        if (shotAtCounter != 0) node.Add("shotAtCounter", shotAtCounter.ToString());
		if (hitCounter != 0) node.Add("hitCounter", hitCounter.ToString());
		if (shotByFriendlyCounter != 0) node.Add("shotByFriendlyCounter", shotByFriendlyCounter.ToString());
		if (shotFriendlyCounter != 0) node.Add("shotFriendlyCounter", shotFriendlyCounter.ToString());
		if (loneSurvivor) node.Add("loneSurvivor", loneSurvivor.ToString());
		if (ironMan) node.Add("ironMan", ironMan.ToString());
		if (longDistanceHitCounter != 0) node.Add("longDistanceHitCounter", longDistanceHitCounter.ToString());
        if (lowAccuracyHitCounter != 0) node.Add("lowAccuracyHitCounter", lowAccuracyHitCounter.ToString());
		if (shotsFiredCounter != 0) node.Add("shotsFiredCounter", shotsFiredCounter.ToString());
		if (shotsLandedCounter != 0) node.Add("shotsLandedCounter", shotsLandedCounter.ToString());
		if (nikeCross) node.Add("nikeCross", nikeCross.ToString());
		if (mercyCross) node.Add("mercyCross", mercyCross.ToString());
		if (woundsHealed != 0) node.Add("woundsHealed", woundsHealed.ToString());
		if (appliedStimulant != 0) node.Add("appliedStimulant", appliedStimulant.ToString());
        if (appliedPainKill != 0) node.Add("appliedPainKill", appliedPainKill.ToString());
		if (revivedSoldier != 0) node.Add("revivedSoldier", revivedSoldier.ToString());
		if (revivedHostile != 0) node.Add("revivedHostile", revivedHostile.ToString());
		if (revivedNeutral != 0) node.Add("revivedNeutral", revivedNeutral.ToString());
        if (martyr != 0) node.Add("martyr", martyr.ToString());
		if (slaveKills != 0) node.Add("slaveKills", slaveKills.ToString());
		return node;
	}

	/// Friendly fire check
	internal bool hasFriendlyFired()
	{
		foreach (var i in kills)
		{
			if (i.faction == UnitFaction.FACTION_PLAYER)
				return true;
		}
		return false;
	}
}

/**
 * Container for battle unit kills statistics.
 */
record struct BattleUnitKills
{
    // Variables
    string name;
    internal UnitFaction faction;
    internal UnitStatus status;
    internal int mission, turn, id;
    internal UnitSide side;
    internal UnitBodyPart bodypart;
    internal string type, rank, race, weapon, weaponAmmo;

    public BattleUnitKills()
    {
        faction = UnitFaction.FACTION_HOSTILE;
        status = UnitStatus.STATUS_IGNORE_ME;
        mission = 0;
        turn = 0;
        id = 0;
        side = UnitSide.SIDE_FRONT;
        bodypart = UnitBodyPart.BODYPART_HEAD;
    }

	internal BattleUnitKills(YamlNode node) =>
        load(node);

    /// Decide victim name, race and rank.
    internal void setUnitStats(BattleUnit unit)
    {
        name = string.Empty;
        type = string.Empty;
        if (unit.getGeoscapeSoldier() != null)
        {
            name = unit.getGeoscapeSoldier().getName();
        }
        else
        {
            type = unit.getType();
        }

        if (unit.getOriginalFaction() == UnitFaction.FACTION_PLAYER)
        {
            // Soldiers
            if (unit.getGeoscapeSoldier() != null)
            {
                if (!string.IsNullOrEmpty(unit.getGeoscapeSoldier().getRankString()))
                {
                    rank = unit.getGeoscapeSoldier().getRankString();
                }
                else
                {
                    rank = "STR_SOLDIER";
                }
                if (unit.getUnitRules() != null && !string.IsNullOrEmpty(unit.getUnitRules().getRace()))
                {
                    race = unit.getUnitRules().getRace();
                }
                else
                {
                    race = "STR_FRIENDLY";
                }
            }
            // HWPs
            else
            {
                if (unit.getUnitRules() != null && !string.IsNullOrEmpty(unit.getUnitRules().getRank()))
                {
                    rank = unit.getUnitRules().getRank();
                }
                else
                {
                    rank = "STR_HWPS";
                }
                if (unit.getUnitRules() != null && !string.IsNullOrEmpty(unit.getUnitRules().getRace()))
                {
                    race = unit.getUnitRules().getRace();
                }
                else
                {
                    race = "STR_FRIENDLY";
                }
            }
        }
        // Aliens
        else if (unit.getOriginalFaction() == UnitFaction.FACTION_HOSTILE)
        {
            if (unit.getUnitRules() != null && !string.IsNullOrEmpty(unit.getUnitRules().getRank()))
            {
                rank = unit.getUnitRules().getRank();
            }
            else
            {
                rank = "STR_LIVE_SOLDIER";
            }
            if (unit.getUnitRules() != null && !string.IsNullOrEmpty(unit.getUnitRules().getRace()))
            {
                race = unit.getUnitRules().getRace();
            }
            else
            {
                race = "STR_HOSTILE";
            }
        }
        // Civilians
        else if (unit.getOriginalFaction() == UnitFaction.FACTION_NEUTRAL)
        {
            if (unit.getUnitRules() != null && !string.IsNullOrEmpty(unit.getUnitRules().getRank()))
            {
                rank = unit.getUnitRules().getRank();
            }
            else
            {
                rank = "STR_CIVILIAN";
            }
            if (unit.getUnitRules() != null && !string.IsNullOrEmpty(unit.getUnitRules().getRace()))
            {
                race = unit.getUnitRules().getRace();
            }
            else
            {
                race = "STR_NEUTRAL";
            }
        }
        // Error
        else
        {
            rank = "STR_UNKNOWN";
            race = "STR_UNKNOWN";
        }
    }

    /// Make turn unique across mission
    internal void setTurn(int unitTurn, UnitFaction unitFaction) =>
        turn = unitTurn * 3 + (int)unitFaction;

	/// Save
	internal YamlNode save()
	{
		var node = new YamlMappingNode();
		if (!string.IsNullOrEmpty(name))
			node.Add("name", name);
		if (!string.IsNullOrEmpty(type))
			node.Add("type", type);
		node.Add("rank", rank);
		node.Add("race", race);
		node.Add("weapon", weapon);
		node.Add("weaponAmmo", weaponAmmo);
		node.Add("status", ((int)status).ToString());
		node.Add("faction", ((int)faction).ToString());
		node.Add("mission", mission.ToString());
		node.Add("turn", turn.ToString());
		node.Add("side", ((int)side).ToString());
		node.Add("bodypart", ((int)bodypart).ToString());
		node.Add("id", id.ToString());
        return node;
	}

	/// Load
	internal void load(YamlNode node)
	{
        YamlNode n = node["name"];
        if (n != null)
		{
			name = n.ToString();
		}
		type = node["type"].ToString();
		rank = node["rank"].ToString();
		race = node["race"].ToString();
		weapon = node["weapon"].ToString();
		weaponAmmo = node["weaponAmmo"].ToString();
		status = (UnitStatus)int.Parse(node["status"].ToString());
		faction = (UnitFaction)int.Parse(node["faction"].ToString());
		mission = int.Parse(node["mission"].ToString());
		turn = int.Parse(node["turn"].ToString());
		side = (UnitSide)int.Parse(node["side"].ToString());
		bodypart = (UnitBodyPart)int.Parse(node["bodypart"].ToString());
		id = int.Parse(node["id"].ToString());
	}

    /// Convert kill Status to string.
    internal string getKillStatusString()
	{
		switch (status)
		{
		    case UnitStatus.STATUS_DEAD:        return "STR_KILLED";
		    case UnitStatus.STATUS_UNCONSCIOUS: return "STR_STUNNED";
		    case UnitStatus.STATUS_PANICKING:   return "STR_PANICKED";
		    case UnitStatus.STATUS_TURNING:     return "STR_MINDCONTROLLED";
		    default:                            return "status error";
		}
	}

	/// Get human-readable victim name.
	internal string getUnitName(Language lang)
	{
		if (!string.IsNullOrEmpty(name))
		{
			return name;
		}
		else if (!string.IsNullOrEmpty(type))
		{
			return lang.getString(type);
		}
		else
		{
            return $"{lang.getString(race)} {lang.getString(rank)}";
		}
	}

	/// Convert victim Status to string.
	internal string getUnitStatusString()
	{
		switch (status)
		{
		    case UnitStatus.STATUS_DEAD:        return "STATUS_DEAD";
		    case UnitStatus.STATUS_UNCONSCIOUS: return "STATUS_UNCONSCIOUS";
		    case UnitStatus.STATUS_PANICKING:   return "STATUS_PANICKING";
		    case UnitStatus.STATUS_TURNING:     return "STATUS_TURNING";
		    default:                            return "status error";
		}
	}

	/// Convert victim Faction to string.
	internal string getUnitFactionString()
	{
		switch (faction)
		{
		    case UnitFaction.FACTION_PLAYER:    return "FACTION_PLAYER";
		    case UnitFaction.FACTION_HOSTILE:   return "FACTION_HOSTILE";
		    case UnitFaction.FACTION_NEUTRAL:   return "FACTION_NEUTRAL";
		    default:                            return "faction error";
		}
	}

	/// Convert victim Side to string.
	internal string getUnitSideString()
	{
		switch (side)
		{
		    case UnitSide.SIDE_FRONT:           return "SIDE_FRONT";
		    case UnitSide.SIDE_LEFT:            return "SIDE_LEFT";
		    case UnitSide.SIDE_RIGHT:           return "SIDE_RIGHT";
		    case UnitSide.SIDE_REAR:            return "SIDE_REAR";
		    case UnitSide.SIDE_UNDER:           return "SIDE_UNDER";
		    default:                            return "side error";
		}
	}

	/// Convert victim Body part to string.
	internal string getUnitBodyPartString()
	{
		switch (bodypart)
		{
		    case UnitBodyPart.BODYPART_HEAD:        return "BODYPART_HEAD";
		    case UnitBodyPart.BODYPART_TORSO:       return "BODYPART_TORSO";
		    case UnitBodyPart.BODYPART_RIGHTARM:    return "BODYPART_RIGHTARM";
		    case UnitBodyPart.BODYPART_LEFTARM:     return "BODYPART_LEFTARM";
		    case UnitBodyPart.BODYPART_RIGHTLEG:    return "BODYPART_RIGHTLEG";
		    case UnitBodyPart.BODYPART_LEFTLEG:     return "BODYPART_LEFTLEG";
		    default:                                return "body part error";
		}
	}

	/// Check to see if turn was on HOSTILE side
	internal bool hostileTurn()
	{
		if ((turn - 1) % 3 == 0) return true;
		return false;
	}

	// Functions
	/// Make turn unique across all kills
	internal int makeTurnUnique() =>
		turn += mission * 300; // Maintains divisibility by 3 as well
}
