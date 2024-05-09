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

namespace SharpXcom.Battlescape;

/**
 * Explosion state not only handles explosions, but also bullet impacts!
 * Refactoring tip : ImpactBState.
 */
internal class ExplosionBState : BattleState
{
    BattleUnit _unit;
    Position _center;
    BattleItem _item;
    Tile _tile;
    int _power;
    bool _areaOfEffect, _lowerWeapon, _cosmetic;

    /**
     * Sets up an ExplosionBState.
     * @param parent Pointer to the BattleScape.
     * @param center Center position in voxelspace.
     * @param item Item involved in the explosion (eg grenade).
     * @param unit Unit involved in the explosion (eg unit throwing the grenade).
     * @param tile Tile the explosion is on.
     * @param lowerWeapon Whether the unit causing this explosion should now lower their weapon.
     */
    internal ExplosionBState(BattlescapeGame parent, Position center, BattleItem item, BattleUnit unit, Tile tile = null, bool lowerWeapon = false, bool cosmetic = false) : base(parent)
    {
        _unit = unit;
        _center = center;
        _item = item;
        _tile = tile;
        _power = 0;
        _areaOfEffect = false;
        _lowerWeapon = lowerWeapon;
        _cosmetic = cosmetic;
    }

    /**
     * Deletes the ExplosionBState.
     */
    ~ExplosionBState() { }

	/**
	 * Initializes the explosion.
	 * The animation and sound starts here.
	 * If the animation is finished, the actual effect takes place.
	 */
	protected override void init()
	{
		if (_item != null)
		{
			_power = _item.getRules().getPower();

			// this usually only applies to melee, but as a concession for modders i'll leave it here in case they wanna make bows or something.
			if (_item.getRules().isStrengthApplied() && _unit != null)
			{
				_power += _unit.getBaseStats().strength;
			}

			_areaOfEffect = _item.getRules().getBattleType() != BattleType.BT_MELEE &&
							_item.getRules().getExplosionRadius() != 0 &&
							!_cosmetic;
		}
		else if (_tile != null)
		{
			_power = _tile.getExplosive();
			_areaOfEffect = true;
		}
		else if (_unit != null && (_unit.getSpecialAbility() == (int)SpecialAbility.SPECAB_EXPLODEONDEATH || _unit.getSpecialAbility() == (int)SpecialAbility.SPECAB_BURN_AND_EXPLODE))
		{
			_power = _parent.getMod().getItem(_unit.getArmor().getCorpseGeoscape(), true).getPower();
			_areaOfEffect = true;
		}
		else
		{
			_power = 120;
			_areaOfEffect = true;
		}

		Tile t = _parent.getSave().getTile(new Position(_center.x/16, _center.y/16, _center.z/24));
		if (_areaOfEffect)
		{
			if (_power != 0)
			{
				int frame = Mod.Mod.EXPLOSION_OFFSET;
				if (_item != null)
				{
					frame = _item.getRules().getHitAnimation();
				}
				if (_parent.getDepth() > 0)
				{
					frame -= Explosion.EXPLODE_FRAMES;
				}
				int frameDelay = 0;
				int counter = Math.Max(1, (_power/5) / 5);
				_parent.getMap().setBlastFlash(true);
				int lowerLimit = Math.Max(1, _power/5);
				for (int i = 0; i < lowerLimit; i++)
				{
					int X = RNG.generate(-_power/2,_power/2);
					int Y = RNG.generate(-_power/2,_power/2);
					Position p = _center;
					p.x += X; p.y += Y;
					Explosion explosion = new Explosion(p, frame, frameDelay, true);
					// add the explosion on the map
					_parent.getMap().getExplosions().Add(explosion);
					if (i > 0 && i % counter == 0)
					{
						frameDelay++;
					}
				}
				_parent.setStateInterval(BattlescapeState.DEFAULT_ANIM_SPEED/2);
				// explosion sound
				if (_power <= 80)
					_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)Mod.Mod.SMALL_EXPLOSION).play();
				else
					_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)Mod.Mod.LARGE_EXPLOSION).play();

				_parent.getMap().getCamera().centerOnPosition(t.getPosition(), false);
			}
			else
			{
				_parent.popState();
			}
		}
		else
		// create a bullet hit
		{
			_parent.setStateInterval((uint)Math.Max(1, ((BattlescapeState.DEFAULT_ANIM_SPEED/2) - (10 * _item.getRules().getExplosionSpeed()))));
			int anim = _item.getRules().getHitAnimation();
			int sound = _item.getRules().getHitSound();
			if (_cosmetic) // Play melee animation on hitting and psiing
			{
				anim = _item.getRules().getMeleeAnimation();
			}

			if (anim != -1)
			{
				Explosion explosion = new Explosion(_center, anim, 0, false, _cosmetic);
				_parent.getMap().getExplosions().Add(explosion);
			}

			_parent.getMap().getCamera().setViewLevel(_center.z / 24);

			BattleUnit target = t.getUnit();
			if (_cosmetic && _parent.getSave().getSide() == UnitFaction.FACTION_HOSTILE && target != null && target.getFaction() == UnitFaction.FACTION_PLAYER)
			{
				_parent.getMap().getCamera().centerOnPosition(t.getPosition(), false);
			}
			if (sound != -1 && !_cosmetic)
			{
				// bullet hit sound
				_parent.getMod().getSoundByDepth((uint)_parent.getDepth(), (uint)sound).play(-1, _parent.getMap().getSoundAngle(_center / new Position(16,16,24)));
			}
		}
	}

	/**
	 * Animates explosion sprites. If their animation is finished remove them from the list.
	 * If the list is empty, this state is finished and the actual calculations take place.
	 */
	protected override void think()
	{
		if (!_parent.getMap().getBlastFlash())
		{
			var explosions = _parent.getMap().getExplosions();
			if (!explosions.Any())
				explode();

			for (var i = 0; i < explosions.Count;)
			{
				if (!explosions[i].animate())
				{
					explosions.RemoveAt(i);
					if (!explosions.Any())
					{
						explode();
						return;
					}
				}
				else
				{
					++i;
				}
			}
		}
	}

	/**
	 * Calculates the effects of the explosion.
	 */
	void explode()
	{
		bool terrainExplosion = false;
		SavedBattleGame save = _parent.getSave();
		// after the animation is done, the real explosion/hit takes place
		if (_item != null)
		{
			if (_unit == null && _item.getPreviousOwner() != null)
			{
				_unit = _item.getPreviousOwner();
			}

			BattleUnit victim = null;

			if (_areaOfEffect)
			{
				save.getTileEngine().explode(_center, _power, _item.getRules().getDamageType(), _item.getRules().getExplosionRadius(), _unit);
			}
			else if (!_cosmetic)
			{
				ItemDamageType type = _item.getRules().getDamageType();

				victim = save.getTileEngine().hit(_center, _power, type, _unit);
			}
			// check if this unit turns others into zombies
			if (!string.IsNullOrEmpty(_item.getRules().getZombieUnit())
				&& victim != null
				&& victim.getArmor().getSize() == 1
				&& (victim.getGeoscapeSoldier() != null || victim.getUnitRules().getRace() == "STR_CIVILIAN")
				&& string.IsNullOrEmpty(victim.getSpawnUnit()))
			{
				// converts the victim to a zombie on death
				victim.setRespawn(true);
				victim.setSpawnUnit(_item.getRules().getZombieUnit());
			}
		}
		if (_tile != null)
		{
			ItemDamageType DT;
			switch (_tile.getExplosiveType())
			{
				case 0:
					DT = ItemDamageType.DT_HE;
					break;
				case 5:
					DT = ItemDamageType.DT_IN;
					break;
				case 6:
					DT = ItemDamageType.DT_STUN;
					break;
				default:
					DT = ItemDamageType.DT_SMOKE;
					break;
			}
			if (DT != ItemDamageType.DT_HE)
			{
				_tile.setExplosive(0,0,true);
			}
			save.getTileEngine().explode(_center, _power, DT, _power/10);
			terrainExplosion = true;
		}
		if (_tile == null && _item == null)
		{
			int radius = 6;
			// explosion not caused by terrain or an item, must be by a unit (cyberdisc)
			if (_unit != null && (_unit.getSpecialAbility() == (int)SpecialAbility.SPECAB_EXPLODEONDEATH || _unit.getSpecialAbility() == (int)SpecialAbility.SPECAB_BURN_AND_EXPLODE))
			{
				radius = _parent.getMod().getItem(_unit.getArmor().getCorpseGeoscape(), true).getExplosionRadius();
			}
			save.getTileEngine().explode(_center, _power, ItemDamageType.DT_HE, radius);
			terrainExplosion = true;
		}

		if (!_cosmetic)
		{
			// now check for new casualties
			_parent.checkForCasualties(_item, _unit, false, terrainExplosion);
		}

		// if this explosion was caused by a unit shooting, now it's the time to put the gun down
		if (_unit != null && !_unit.isOut() && _lowerWeapon)
		{
			_unit.aim(false);
			_unit.setCache(null);
		}
		_parent.getMap().cacheUnits();
		_parent.popState();

		// check for terrain explosions
		Tile t = save.getTileEngine().checkForTerrainExplosions();
		if (t != null)
		{
			Position p = new Position(t.getPosition().x * 16, t.getPosition().y * 16, t.getPosition().z * 24);
			p += new Position(8,8,0);
			_parent.statePushFront(new ExplosionBState(_parent, p, null, _unit, t));
		}

		if (_item != null && (_item.getRules().getBattleType() == BattleType.BT_GRENADE || _item.getRules().getBattleType() == BattleType.BT_PROXIMITYGRENADE))
		{
			_parent.getSave().removeItem(_item);
		}
	}

	/**
	 * Explosions cannot be cancelled.
	 */
	protected override void cancel() { }
}
