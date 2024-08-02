﻿/*
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
 * A class that renders a specific unit, given its render rules
 * combining the right frames from the surfaceset.
 */
internal class UnitSprite : Surface
{
	BattleUnit _unit;
	BattleItem _itemR, _itemL;
	SurfaceSet _unitSurface, _itemSurfaceR, _itemSurfaceL;
	int _part, _animationFrame, _drawingRoutine;
	bool _helmet;
	KeyValuePair<byte, byte> _color;
	int _colorSize;

    /**
     * Sets up a UnitSprite with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal UnitSprite(int width, int height, int x, int y, bool helmet) : base(width, height, x, y)
    {
        _unit = null;
        _itemR = null;
        _itemL = null;
        _unitSurface = null;
        _itemSurfaceR = null;
        _itemSurfaceL = null;
        _part = 0;
        _animationFrame = 0;
        _drawingRoutine = 0;
        _helmet = helmet;
        _color = default;
        _colorSize = 0;
    }

    /**
     * Deletes the UnitSprite.
     */
    ~UnitSprite() { }

	/**
	 * Links this sprite to a BattleUnit to get the data for rendering.
	 * @param unit Pointer to the BattleUnit.
	 * @param part The part number for large units.
	 */
	internal void setBattleUnit(BattleUnit unit, int part = 0)
	{
		_unit = unit;
		_drawingRoutine = _unit.getArmor().getDrawingRoutine();
		_redraw = true;
		_part = part;

		if (Options.battleHairBleach)
		{
			_colorSize =_unit.getRecolor().Count;
			if (_colorSize != 0)
			{
				_color = _unit.getRecolor()[0];
			}
			else
			{
				_color = default;
			}
		}

		_itemR = unit.getItem("STR_RIGHT_HAND");
		if (_itemR != null && _itemR.getRules().isFixed())
		{
			_itemR = null;
		}
		_itemL = unit.getItem("STR_LEFT_HAND");
		if (_itemL != null && _itemL.getRules().isFixed())
		{
			_itemL = null;
		}
	}

	/**
	 * Changes the surface sets for the UnitSprite to get resources for rendering.
	 * @param unitSurface Pointer to the unit surface set.
	 * @param itemSurfaceR Pointer to the item surface set.
	 * @param itemSurfaceL Pointer to the item surface set.
	 */
	internal void setSurfaces(SurfaceSet unitSurface, SurfaceSet itemSurfaceR, SurfaceSet itemSurfaceL)
	{
		_unitSurface = unitSurface;
		_itemSurfaceR = itemSurfaceR;
		_itemSurfaceL = itemSurfaceL;
		_redraw = true;
	}

	/**
	 * Sets the animation frame for animated units.
	 * @param frame Frame number.
	 */
	internal void setAnimationFrame(int frame) =>
		_animationFrame = frame;

	/**
	 * Draws a unit, using the drawing rules of the unit.
	 * This function is called by Map, for each unit on the screen.
	 */
	internal override void draw()
	{
		base.draw();
		// Array of drawing routines
		System.Action[] routines =
		{
			drawRoutine0,
			drawRoutine1,
			drawRoutine2,
			drawRoutine3,
			drawRoutine4,
			drawRoutine5,
			drawRoutine6,
			drawRoutine7,
			drawRoutine8,
			drawRoutine9,
			drawRoutine0,
			drawRoutine11,
			drawRoutine12,
			drawRoutine0,
			drawRoutine0,
			drawRoutine0,
			drawRoutine12,
			drawRoutine4,
			drawRoutine4,
			drawRoutine19,
			drawRoutine20,
			drawRoutine21,
			drawRoutine3
		};
		// Call the matching routine
		routines[_drawingRoutine]();
	}

	void drawRecolored(Surface src)
	{
		if (_colorSize != 0)
		{
			@lock();
			ShaderDraw(new ColorReplace(), ShaderSurface(this), ShaderSurface(src), ShaderScalar(_color), ShaderScalar(_colorSize));
			unlock();
		}
		else
		{
			src.blit(this);
		}
	}

	/**
	 * Determines which weapons to display in the case of two-handed weapons.
	 */
	void sortRifles()
	{
		if (_itemR != null && _itemR.getRules().isTwoHanded())
		{
			if (_itemL != null && _itemL.getRules().isTwoHanded())
			{
				if (_unit.getActiveHand() == "STR_LEFT_HAND")
				{
					_itemR = _itemL;
				}
				_itemL = null;
			}
			else if (_unit.getStatus() != UnitStatus.STATUS_AIMING)
			{
				_itemL = null;
			}
		}
		else if (_itemL != null && _itemL.getRules().isTwoHanded())
		{
			if (_unit.getStatus() != UnitStatus.STATUS_AIMING)
			{
				_itemR = null;
			}
		}
	}

	/**
	 * Drawing routine for XCom soldiers in overalls, sectoids (routine 0),
	 * mutons (routine 10),
	 * aquanauts (routine 13),
	 * calcinites, deep ones, gill men, lobster men, tasoths (routine 14),
	 * aquatoids (routine 15) (this one is no different, it just precludes breathing animations.
	 */
	void drawRoutine0()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		Surface torso = null, legs = null, leftArm = null, rightArm = null, itemR = null, itemL = null;
		// magic numbers
		const int legsStand = 16, legsKneel = 24;
		int maleTorso, femaleTorso, die, rarm1H, larm2H, rarm2H, rarmShoot, legsFloat, torsoHandsWeaponY = 0;
		if (_drawingRoutine <= 10)
		{
			die = 264; // ufo:eu death frame
			maleTorso = 32;
			femaleTorso = 267;
			rarm1H = 232;
			larm2H = 240;
			rarm2H = 248;
			rarmShoot = 256;
			legsFloat = 275;
		}
		else if (_drawingRoutine == 13)
		{
			if (_helmet)
			{
				die = 259; // aquanaut underwater death frame
				maleTorso = 32; // aquanaut underwater ion armour torso

				if (_unit.getArmor().getForcedTorso() == ForcedTorso.TORSO_USE_GENDER)
				{
					femaleTorso = 32; // aquanaut underwater plastic aqua armour torso
				}
				else
				{
					femaleTorso = 286; // aquanaut underwater magnetic ion armour torso
				}
				rarm1H = 248;
				larm2H = 232;
				rarm2H = rarmShoot = 240;
				legsFloat = 294;
			}
			else
			{
				die = 256; // aquanaut land death frame
				// aquanaut land torso
				maleTorso = 270;
				femaleTorso = 262;
				rarm1H = 248;
				larm2H = 232;
				rarm2H = rarmShoot = 240;
				legsFloat = 294;
			}
		}
		else
		{
			die = 256; // tftd unit death frame
			// tftd unit torso
			maleTorso = 32;
			femaleTorso = 262;
			rarm1H = 248;
			larm2H = 232;
			rarm2H = rarmShoot = 240;
			legsFloat = 294;
		}
		const int larmStand = 0, rarmStand = 8;
		int[] legsWalk = { 56, 56+24, 56+24*2, 56+24*3, 56+24*4, 56+24*5, 56+24*6, 56+24*7 };
		int[] larmWalk = { 40, 40+24, 40+24*2, 40+24*3, 40+24*4, 40+24*5, 40+24*6, 40+24*7 };
		int[] rarmWalk = { 48, 48+24, 48+24*2, 48+24*3, 48+24*4, 48+24*5, 48+24*6, 48+24*7 };
		int[] YoffWalk = {1, 0, -1, 0, 1, 0, -1, 0}; // bobbing up and down
		int[] mutonYoffWalk = {1, 1, 0, 0, 1, 1, 0, 0}; // bobbing up and down (muton)
		int[] aquatoidYoffWalk = {1, 0, 0, 1, 2, 1, 0, 0}; // bobbing up and down (aquatoid)
		int[] offX = { 8, 10, 7, 4, -9, -11, -7, -3 }; // for the weapons
		int[] offY = { -6, -3, 0, 2, 0, -4, -7, -9 }; // for the weapons
		int[] offX2 = { -8, 3, 5, 12, 6, -1, -5, -13 }; // for the left handed weapons
		int[] offY2 = { 1, -4, -2, 0, 3, 3, 5, 0 }; // for the left handed weapons
		int[] offX3 = { 0, 0, 2, 2, 0, 0, 0, 0 }; // for the weapons (muton)
		int[] offY3 = { -3, -3, -1, -1, -1, -3, -3, -2 }; // for the weapons (muton)
		int[] offX4 = { -8, 2, 7, 14, 7, -2, -4, -8 }; // for the left handed weapons
		int[] offY4 = { -3, -3, -1, 0, 3, 3, 0, 1 }; // for the left handed weapons
		int[] offX5 = { -1, 1, 1, 2, 0, -1, 0, 0 }; // for the weapons (muton)
		int[] offY5 = { 1, -1, -1, -1, -1, -1, -3, 0 }; // for the weapons (muton)
		int[] offX6 = { 0, 6, 6, 12, -4, -5, -5, -13 }; // for the left handed rifles
		int[] offY6 = { -4, -4, -1, 0, 5, 0, 1, 0 }; // for the left handed rifles
		int[] offX7 = { 0, 6, 8, 12, 2, -5, -5, -13 }; // for the left handed rifles (muton)
		int[] offY7 = { -4, -6, -1, 0, 3, 0, 1, 0 }; // for the left handed rifles (muton)
		const int offYKneel = 4;
		const int offXSprite = 16; // sprites are double width
		const int soldierHeight = 22;

		int unitDir = _unit.getDirection();
		int walkPhase = _unit.getWalkingPhase();

		if (_unit.getStatus() == UnitStatus.STATUS_COLLAPSING)
		{
			torso = _unitSurface.getFrame(die + _unit.getFallingPhase());
			torso.setX(offXSprite);
			drawRecolored(torso);
			return;
		}
		if (_drawingRoutine == 0 || _helmet)
		{
			if ((_unit.getGender() == SoldierGender.GENDER_FEMALE && _unit.getArmor().getForcedTorso() != ForcedTorso.TORSO_ALWAYS_MALE)
				|| _unit.getArmor().getForcedTorso() == ForcedTorso.TORSO_ALWAYS_FEMALE)
			{
				torso = _unitSurface.getFrame(femaleTorso + unitDir);
			}
			else
			{
				torso = _unitSurface.getFrame(maleTorso + unitDir);
			}
		}
		else
		{
			if (_unit.getGender() == SoldierGender.GENDER_FEMALE)
			{
				torso = _unitSurface.getFrame(femaleTorso + unitDir);
			}
			else
			{
				torso = _unitSurface.getFrame(maleTorso + unitDir);
			}
		}

		// when walking, torso(fixed sprite) has to be animated up/down
		if (_unit.getStatus() == UnitStatus.STATUS_WALKING)
		{
			if (_drawingRoutine == 10)
				torsoHandsWeaponY = mutonYoffWalk[walkPhase];
			else if (_drawingRoutine == 13 || _drawingRoutine == 14)
				torsoHandsWeaponY = YoffWalk[walkPhase]+1;
			else if (_drawingRoutine == 15)
				torsoHandsWeaponY = aquatoidYoffWalk[walkPhase];
			else
				torsoHandsWeaponY = YoffWalk[walkPhase];
			torso.setY(torsoHandsWeaponY);
			legs = _unitSurface.getFrame(legsWalk[unitDir] + walkPhase);
			leftArm = _unitSurface.getFrame(larmWalk[unitDir] + walkPhase);
			rightArm = _unitSurface.getFrame(rarmWalk[unitDir] + walkPhase);
			if (_drawingRoutine == 10 && unitDir == 3)
			{
				leftArm.setY(-1);
			}
		}
		else
		{
			if (_unit.isKneeled())
			{
				legs = _unitSurface.getFrame(legsKneel + unitDir);
			}
			else if (_unit.isFloating() && _unit.getMovementType() == MovementType.MT_FLY)
			{
				legs = _unitSurface.getFrame(legsFloat + unitDir);
			}
			else
			{
				legs = _unitSurface.getFrame(legsStand + unitDir);
			}
			leftArm = _unitSurface.getFrame(larmStand + unitDir);
			rightArm = _unitSurface.getFrame(rarmStand + unitDir);
		}

		sortRifles();

		// holding an item
		if (_itemR != null)
		{
			// draw handob item
			if (_unit.getStatus() == UnitStatus.STATUS_AIMING && _itemR.getRules().isTwoHanded())
			{
				int dir = (unitDir + 2)%8;
				itemR = _itemSurfaceR.getFrame(_itemR.getRules().getHandSprite() + dir);
				itemR.setX(offX[unitDir]);
				itemR.setY(offY[unitDir]);
			}
			else
			{
				itemR = _itemSurfaceR.getFrame(_itemR.getRules().getHandSprite() + unitDir);
				if (_drawingRoutine == 10)
				{
					if (_itemR.getRules().isTwoHanded())
					{
						itemR.setX(offX3[unitDir]);
						itemR.setY(offY3[unitDir]);
					}
					else
					{
						itemR.setX(offX5[unitDir]);
						itemR.setY(offY5[unitDir]);
					}
				}
				else
				{
					itemR.setX(0);
					itemR.setY(0);
				}
			}

			// draw arms holding the item
			if (_itemR.getRules().isTwoHanded())
			{
				leftArm = _unitSurface.getFrame(larm2H + unitDir);
				if (_unit.getStatus() == UnitStatus.STATUS_AIMING)
				{
					rightArm = _unitSurface.getFrame(rarmShoot + unitDir);
				}
				else
				{
					rightArm = _unitSurface.getFrame(rarm2H + unitDir);
				}
			}
			else
			{
				if (_drawingRoutine == 10)
					rightArm = _unitSurface.getFrame(rarm2H + unitDir);
				else
					rightArm = _unitSurface.getFrame(rarm1H + unitDir);
			}

			// the fixed arm(s) have to be animated up/down when walking
			if (_unit.getStatus() == UnitStatus.STATUS_WALKING)
			{
				itemR.setY(itemR.getY() + torsoHandsWeaponY);
				rightArm.setY(torsoHandsWeaponY);
				if (_itemR.getRules().isTwoHanded())
					leftArm.setY(torsoHandsWeaponY);
			}
		}
		//if we are left handed or dual wielding...
		if (_itemL != null)
		{
			leftArm = _unitSurface.getFrame(larm2H + unitDir);
			itemL = _itemSurfaceL.getFrame(_itemL.getRules().getHandSprite() + unitDir);
			if (!_itemL.getRules().isTwoHanded())
			{
				if (_drawingRoutine == 10)
				{
					itemL.setX(offX4[unitDir]);
					itemL.setY(offY4[unitDir]);
				}
				else
				{
					itemL.setX(offX2[unitDir]);
					itemL.setY(offY2[unitDir]);
				}
			}
			else
			{
				itemL.setX(0);
				itemL.setY(0);
				rightArm = _unitSurface.getFrame(rarm2H + unitDir);
			}

			if (_unit.getStatus() == UnitStatus.STATUS_AIMING && _itemL.getRules().isTwoHanded())
			{
				int dir = (unitDir + 2)%8;
				itemL = _itemSurfaceL.getFrame(_itemL.getRules().getHandSprite() + dir);
				if (_drawingRoutine == 10)
				{
					itemL.setX(offX7[unitDir]);
					itemL.setY(offY7[unitDir]);
				}
				else
				{
					itemL.setX(offX6[unitDir]);
					itemL.setY(offY6[unitDir]);
				}
				rightArm = _unitSurface.getFrame(rarmShoot + unitDir);
			}

			if (_unit.getStatus() == UnitStatus.STATUS_WALKING)
			{
				itemL.setY(itemL.getY() + torsoHandsWeaponY);
				leftArm.setY(torsoHandsWeaponY);
				if (_itemL.getRules().isTwoHanded())
					rightArm.setY(torsoHandsWeaponY);
			}
		}
		// offset everything but legs when kneeled
		if (_unit.isKneeled())
		{
			if (_drawingRoutine == 13) // tftd torsos are stubby.
			{
				leftArm.setY(offYKneel + 1);
				rightArm.setY(offYKneel + 1);
				torso.setY(offYKneel + 1);
				itemR?.setY(itemR.getY() + offYKneel + 1);
				itemL?.setY(itemL.getY() + offYKneel + 1);
			}
			else
			{
				leftArm.setY(offYKneel);
				rightArm.setY(offYKneel);
				torso.setY(offYKneel);
				itemR?.setY(itemR.getY() + offYKneel);
				itemL?.setY(itemL.getY() + offYKneel);
			}
		}
		else if (_unit.getStatus() != UnitStatus.STATUS_WALKING)
		{
			leftArm.setY(0);
			rightArm.setY(0);
			torso.setY(0);
		}

		// items are calculated for soldier height (22) - some aliens are smaller, so item is drawn lower.
		if (itemR != null)
		{
			itemR.setY(itemR.getY() + (soldierHeight - _unit.getStandHeight()));
		}
		if (itemL != null)
		{
			itemL.setY(itemL.getY() + (soldierHeight - _unit.getStandHeight()));
		}

		// offset everything to the left by 16 pixels.
		// this is because we draw the sprites double wide, to accommodate weapons in-hand
		torso.setX(offXSprite);
		legs.setX(offXSprite);
		leftArm.setX(offXSprite);
		rightArm.setX(offXSprite);
		if (itemR != null)
			itemR.setX(itemR.getX() + offXSprite);
		if (itemL != null)
			itemL.setX(itemL.getX() + offXSprite);

		// fix the errant muton arm.
		if (itemR == null && _drawingRoutine == 10 && _unit.getStatus() == UnitStatus.STATUS_WALKING && unitDir == 2)
		{
			rightArm.setX(10);
		}

		// blit order depends on unit direction, and whether we are holding a 2 handed weapon.
		switch (unitDir)
		{
			case 0: itemR?.blit(this); itemL?.blit(this); drawRecolored(leftArm); drawRecolored(legs); drawRecolored(torso); drawRecolored(rightArm); break;
			case 1: drawRecolored(leftArm); drawRecolored(legs); itemL?.blit(this); drawRecolored(torso); itemR?.blit(this); drawRecolored(rightArm); break;
			case 2: drawRecolored(leftArm); drawRecolored(legs); drawRecolored(torso); itemL?.blit(this); itemR?.blit(this); drawRecolored(rightArm); break;
			case 3:
				if (_unit.getStatus() != UnitStatus.STATUS_AIMING  && ((_itemR != null && _itemR.getRules().isTwoHanded()) || (_itemL != null && _itemL.getRules().isTwoHanded())))
				{
					drawRecolored(legs); drawRecolored(torso); drawRecolored(leftArm); itemR?.blit(this); itemL?.blit(this); drawRecolored(rightArm);
				}
				else
				{
					drawRecolored(legs); drawRecolored(torso); drawRecolored(leftArm); drawRecolored(rightArm); itemR?.blit(this); itemL?.blit(this);
				}
				break;
			case 4:	drawRecolored(legs); drawRecolored(rightArm); drawRecolored(torso); drawRecolored(leftArm); itemR?.blit(this); itemL?.blit(this); break;
			case 5:
				if (_unit.getStatus() != UnitStatus.STATUS_AIMING  && ((_itemR != null && _itemR.getRules().isTwoHanded()) || (_itemL != null && _itemL.getRules().isTwoHanded())))
				{
					drawRecolored(rightArm); drawRecolored(legs); drawRecolored(torso); drawRecolored(leftArm); itemR?.blit(this); itemL?.blit(this);
				}
				else
				{
					drawRecolored(rightArm); drawRecolored(legs); itemR?.blit(this); itemL?.blit(this); drawRecolored(torso); drawRecolored(leftArm);
				}
				break;
			case 6: drawRecolored(rightArm); itemR?.blit(this); itemL?.blit(this); drawRecolored(legs); drawRecolored(torso); drawRecolored(leftArm); break;
			case 7:
				if (_unit.getStatus() != UnitStatus.STATUS_AIMING  && ((_itemR != null && _itemR.getRules().isTwoHanded()) || (_itemL != null && _itemL.getRules().isTwoHanded())))
				{
					drawRecolored(rightArm); itemR?.blit(this); itemL?.blit(this); drawRecolored(leftArm); drawRecolored(legs); drawRecolored(torso);
				}
				else
				{
					itemR?.blit(this); itemL?.blit(this); drawRecolored(leftArm); drawRecolored(rightArm); drawRecolored(legs); drawRecolored(torso);
				}
				break;
		}
		torso.setX(0);
		legs.setX(0);
		leftArm.setX(0);
		rightArm.setX(0);
		if (itemR != null)
			itemR.setX(0);
		if (itemL != null)
			itemL.setX(0);
	}

	/**
	 * Drawing routine for floaters.
	 */
	void drawRoutine1()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		Surface torso = null, leftArm = null, rightArm = null, itemR = null, itemL = null;
		// magic numbers
		const int stand = 16, walk = 24, die = 64;
		const int larm = 8, rarm = 0, larm2H = 67, rarm2H = 75, rarmShoot = 83, rarm1H= 91; // note that arms are switched vs "normal" sheets
		int[] yoffWalk = {0, 0, 0, 0, 0, 0, 0, 0}; // bobbing up and down
		int[] offX = { 8, 10, 7, 4, -9, -11, -7, -3 }; // for the weapons
		int[] offY = { -6, -3, 0, 2, 0, -4, -7, -9 }; // for the weapons
		int[] offX2 = { -8, 3, 7, 13, 6, -3, -5, -13 }; // for the weapons
		int[] offY2 = { 1, -4, -1, 0, 3, 3, 5, 0 }; // for the weapons
		int[] offX3 = { 0, 6, 6, 12, -4, -5, -5, -13 }; // for the left handed rifles
		int[] offY3 = { -4, -4, -1, 0, 5, 0, 1, 0 }; // for the left handed rifles
		const int offXSprite = 16; // sprites are double width

		if (_unit.getStatus() == UnitStatus.STATUS_COLLAPSING)
		{
			torso = _unitSurface.getFrame(die + _unit.getFallingPhase());
			torso.setX(offXSprite);
			drawRecolored(torso);
			return;
		}

		int unitDir = _unit.getDirection();
		int walkPhase = _unit.getWalkingPhase();

		leftArm = _unitSurface.getFrame(larm + unitDir);
		rightArm = _unitSurface.getFrame(rarm + unitDir);
		// when walking, torso(fixed sprite) has to be animated up/down
		if (_unit.getStatus() == UnitStatus.STATUS_WALKING)
		{
			torso = _unitSurface.getFrame((int)(walk + (5 * unitDir) + (walkPhase / 1.6))); // floater only has 5 walk animations instead of 8
			torso.setY(yoffWalk[walkPhase]);
		}
		else
		{
			torso = _unitSurface.getFrame(stand + unitDir);
		}

		sortRifles();

		// holding an item
		if (_itemR != null)
		{
			// draw handob item
			if (_unit.getStatus() == UnitStatus.STATUS_AIMING && _itemR.getRules().isTwoHanded())
			{
				int dir = (_unit.getDirection() + 2)%8;
				itemR = _itemSurfaceR.getFrame(_itemR.getRules().getHandSprite() + dir);
				itemR.setX(offX[unitDir]);
				itemR.setY(offY[unitDir]);
			}
			else
			{
				itemR = _itemSurfaceR.getFrame(_itemR.getRules().getHandSprite() + unitDir);
				itemR.setX(0);
				itemR.setY(0);
			}
			// draw arms holding the item
			if (_itemR.getRules().isTwoHanded())
			{
				leftArm = _unitSurface.getFrame(larm2H + unitDir);
				if (_unit.getStatus() == UnitStatus.STATUS_AIMING)
				{
					rightArm = _unitSurface.getFrame(rarmShoot + unitDir);
				}
				else
				{
					rightArm = _unitSurface.getFrame(rarm2H + unitDir);
				}
			}
			else
			{
				rightArm = _unitSurface.getFrame(rarm1H + unitDir);
			}
		}

		//if we are left handed or dual wielding...
		if (_itemL != null)
		{
			leftArm = _unitSurface.getFrame(larm2H + unitDir);
			itemL = _itemSurfaceL.getFrame(_itemL.getRules().getHandSprite() + unitDir);
			if (!_itemL.getRules().isTwoHanded())
			{
				itemL.setX(offX2[unitDir]);
				itemL.setY(offY2[unitDir]);
			}
			else
			{
				itemL.setX(0);
				itemL.setY(0);
				rightArm = _unitSurface.getFrame(rarm2H + unitDir);
			}

			if (_unit.getStatus() == UnitStatus.STATUS_AIMING && _itemL.getRules().isTwoHanded())
			{
				int dir = (unitDir + 2)%8;
				itemL = _itemSurfaceL.getFrame(_itemL.getRules().getHandSprite() + dir);
				itemL.setX(offX3[unitDir]);
				itemL.setY(offY3[unitDir]);
				rightArm = _unitSurface.getFrame(rarmShoot + unitDir);
			}

			if (_unit.getStatus() == UnitStatus.STATUS_WALKING)
			{
				leftArm.setY(yoffWalk[walkPhase]);
				itemL.setY(itemL.getY() + yoffWalk[walkPhase]);
				if (_itemL.getRules().isTwoHanded())
					rightArm.setY(yoffWalk[walkPhase]);
			}
		}

		if (_unit.getStatus() != UnitStatus.STATUS_WALKING)
		{
			leftArm.setY(0);
			rightArm.setY(0);
			torso.setY(0);
		}

		// offset everything to the left by 16 pixels.
		// this is because we draw the sprites double wide, to accommodate weapons in-hand
		torso.setX(offXSprite);
		leftArm.setX(offXSprite);
		rightArm.setX(offXSprite);
		if (itemR != null)
			itemR.setX(itemR.getX() + offXSprite);
		if (itemL != null)
			itemL.setX(itemL.getX() + offXSprite);

		// blit order depends on unit direction.
		switch (unitDir)
		{
			case 0: itemR?.blit(this); itemL?.blit(this); drawRecolored(leftArm); drawRecolored(torso); drawRecolored(rightArm); break;
			case 1: drawRecolored(leftArm); drawRecolored(torso); drawRecolored(rightArm); itemR?.blit(this); itemL?.blit(this); break;
			case 2: drawRecolored(leftArm); drawRecolored(torso); drawRecolored(rightArm); itemR?.blit(this); itemL?.blit(this); break;
			case 3:	drawRecolored(torso); drawRecolored(leftArm); drawRecolored(rightArm); itemR?.blit(this); itemL?.blit(this); break;
			case 4:	drawRecolored(torso); drawRecolored(leftArm); drawRecolored(rightArm); itemR?.blit(this); itemL?.blit(this); break;
			case 5:	drawRecolored(rightArm); drawRecolored(torso); drawRecolored(leftArm); itemR?.blit(this); itemL?.blit(this); break;
			case 6: drawRecolored(rightArm); itemR?.blit(this); itemL?.blit(this); drawRecolored(torso); drawRecolored(leftArm); break;
			case 7:	drawRecolored(rightArm); itemR?.blit(this); itemL?.blit(this); drawRecolored(leftArm); drawRecolored(torso); break;
		}
		torso.setX(0);
		leftArm.setX(0);
		rightArm.setX(0);
		if (itemR != null)
			itemR.setX(0);
		if (itemL != null)
			itemL.setX(0);
	}

	/**
	 * Drawing routine for XCom tanks.
	 */
	void drawRoutine2()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		int[] offX = { -2, -7, -5, 0, 5, 7, 2, 0 }; // hovertank offsets
		int[] offy = { -1, -3, -4, -5, -4, -3, -1, -1 }; // hovertank offsets
		const int offXSprite = 16; // sprites are double width

		Surface s = null;

		int hoverTank = _unit.getMovementType() == MovementType.MT_FLY ? 32 : 0;
		int turret = _unit.getTurretType();

		// draw the animated propulsion below the hwp
		if (_part > 0 && hoverTank != 0)
		{
			s = _unitSurface.getFrame(104 + ((_part-1) * 8) + _animationFrame);
			s.setX(offXSprite);
			drawRecolored(s);
		}

		// draw the tank itself
		s = _unitSurface.getFrame(hoverTank + (_part * 8) + _unit.getDirection());
		s.setX(offXSprite);
		drawRecolored(s);

		// draw the turret, together with the last part
		if (_part == 3 && turret != -1)
		{
			s = _unitSurface.getFrame(64 + (turret * 8) + _unit.getTurretDirection());
			int turretOffsetX = 0;
			int turretOffsetY = -4;
			if (hoverTank != 0)
			{
				turretOffsetX += offX[_unit.getDirection()];
				turretOffsetY += offy[_unit.getDirection()];
			}
			s.setX(turretOffsetX + offXSprite);
			s.setY(turretOffsetY);
			drawRecolored(s);
		}
	}

	/**
	 * Drawing routine for cyberdiscs. (3)
	 * and helicopters (22)
	 */
	void drawRoutine3()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		Surface s = null;
		const int offXSprite = 16; // sprites are double width

		// draw the animated propulsion below the hwp
		if (_drawingRoutine == 3)
		{
			if (_part > 0)
			{
				s = _unitSurface.getFrame(32 + ((_part-1) * 8) + _animationFrame);
				s.setX(offXSprite);
				drawRecolored(s);
			}
		}
		s = _unitSurface.getFrame((_part * 8) + _unit.getDirection());

		// offset everything to the left by 16 pixels.
		// this is because we draw the sprites double wide, to accommodate weapons in-hand
		s.setX(offXSprite);

		drawRecolored(s);

		// draw the animated propulsion above the hwp
		if (_drawingRoutine == 22)
		{
			if (_part > 0)
			{
				s = _unitSurface.getFrame(32 + ((_part-1) * 8) + _animationFrame);
				s.setX(offXSprite);
				drawRecolored(s);
			}
		}
	}

	/**
	 * Drawing routine for civilians, ethereals, zombies (routine 4),
	 * tftd civilians, tftd zombies (routine 17), more tftd civilians (routine 18).
	 * Very easy: first 8 is standing positions, then 8 walking sequences of 8, finally death sequence of 3
	 */
	void drawRoutine4()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		Surface s = null, itemR = null, itemL = null;
		int stand = 0, walk = 8, die = 72;
		int[] offX = { 8, 10, 7, 4, -9, -11, -7, -3 }; // for the weapons
		int[] offY = { -6, -3, 0, 2, 0, -4, -7, -9 }; // for the weapons
		int[] offX2 = { -8, 3, 5, 12, 6, -1, -5, -13 }; // for the weapons
		int[] offY2 = { 1, -4, -2, 0, 3, 3, 5, 0 }; // for the weapons
		int[] offX3 = { 0, 6, 6, 12, -4, -5, -5, -13 }; // for the left handed rifles
		int[] offY3 = { -4, -4, -1, 0, 5, 0, 1, 0 }; // for the left handed rifles
		int[] standConvert = { 3, 2, 1, 0, 7, 6, 5, 4 }; // array for converting stand frames for some tftd civilians
		const int offXSprite = 16; // sprites are double width

		if (_drawingRoutine == 17) // tftd civilian - first set
		{
			stand = 64;
			walk = 0;
		}
		else if (_drawingRoutine == 18) // tftd civilian - second set
		{
			stand = 140;
			walk = 76;
			die = 148;
		}

		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		int unitDir = _unit.getDirection();

		if (_unit.getStatus() == UnitStatus.STATUS_COLLAPSING)
		{
			s = _unitSurface.getFrame(die + _unit.getFallingPhase());
			s.setX(offXSprite);
			drawRecolored(s);
			return;
		}
		else if (_unit.getStatus() == UnitStatus.STATUS_WALKING)
		{
			s = _unitSurface.getFrame(walk + (8 * unitDir) + _unit.getWalkingPhase());
		}
		else if (_drawingRoutine != 17)
		{
			s = _unitSurface.getFrame(stand + unitDir);
		}
		else
		{
			s = _unitSurface.getFrame(stand + standConvert[unitDir]);
		}

		sortRifles();

		if (_itemR != null && !_itemR.getRules().isFixed())
		{
			// draw handob item
			if (_unit.getStatus() == UnitStatus.STATUS_AIMING && _itemR.getRules().isTwoHanded())
			{
				int dir = (unitDir + 2)%8;
				itemR = _itemSurfaceR.getFrame(_itemR.getRules().getHandSprite() + dir);
				itemR.setX(offX[unitDir]);
				itemR.setY(offY[unitDir]);
			}
			else
			{
				if (_itemR.getSlot().getId() == "STR_RIGHT_HAND")
				{
				itemR = _itemSurfaceR.getFrame(_itemR.getRules().getHandSprite() + unitDir);
				itemR.setX(0);
				itemR.setY(0);
				}
				else
				{
				itemR = _itemSurfaceR.getFrame(_itemR.getRules().getHandSprite() + unitDir);
				itemR.setX(offX2[unitDir]);
				itemR.setY(offY2[unitDir]);
				}
			}
		}

		//if we are dual wielding...
		if (_itemL != null && !_itemL.getRules().isFixed())
		{
			itemL = _itemSurfaceL.getFrame(_itemL.getRules().getHandSprite() + unitDir);
			if (!_itemL.getRules().isTwoHanded())
			{
				itemL.setX(offX2[unitDir]);
				itemL.setY(offY2[unitDir]);
			}
			else
			{
				itemL.setX(0);
				itemL.setY(0);
			}

			if (_unit.getStatus() == UnitStatus.STATUS_AIMING && _itemL.getRules().isTwoHanded())
			{
				int dir = (unitDir + 2)%8;
				itemL = _itemSurfaceL.getFrame(_itemL.getRules().getHandSprite() + dir);
				itemL.setX(offX3[unitDir]);
				itemL.setY(offY3[unitDir]);
			}
		}

		// offset everything to the right by 16 pixels.
		// this is because we draw the sprites double wide, to accommodate weapons in-hand
		s.setX(offXSprite);
		if (itemR != null)
			itemR.setX(itemR.getX() + offXSprite);
		if (itemL != null)
			itemL.setX(itemL.getX() + offXSprite);

		switch (unitDir)
		{
			case 0: itemL?.blit(this); itemR?.blit(this); drawRecolored(s); break;
			case 1: itemL?.blit(this); drawRecolored(s); itemR?.blit(this); break;
			case 2: drawRecolored(s); itemL?.blit(this); itemR?.blit(this); break;
			case 3: drawRecolored(s); itemR?.blit(this); itemL?.blit(this); break;
			case 4: drawRecolored(s); itemR?.blit(this); itemL?.blit(this); break;
			case 5: itemR?.blit(this); drawRecolored(s); itemL?.blit(this); break;
			case 6: itemR?.blit(this); drawRecolored(s); itemL?.blit(this); break;
			case 7: itemR?.blit(this); itemL?.blit(this); drawRecolored(s); break;
		}
		s.setX(0);
		if (itemR != null)
			itemR.setX(0);
		if (itemL != null)
			itemL.setX(0);
	}

	/**
	 * Drawing routine for sectopods and reapers.
	 */
	void drawRoutine5()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		Surface s = null;
		const int offXSprite = 16; // sprites are double width

		if (_unit.getStatus() == UnitStatus.STATUS_WALKING)
		{
			s = _unitSurface.getFrame( 32 + (_unit.getDirection() * 16) + (_part * 4) + ((_unit.getWalkingPhase() / 2) % 4));
		}
		else
		{
			s = _unitSurface.getFrame((_part * 8) + _unit.getDirection());
		}

		// offset everything to the right by 16 pixels.
		// this is because we draw the sprites double wide, to accommodate weapons in-hand
		s.setX(offXSprite);
		drawRecolored(s);
	}

	/**
	 * Drawing routine for snakemen.
	 */
	void drawRoutine6()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		Surface torso = null, legs = null, leftArm = null, rightArm = null, itemR = null, itemL = null;
		// magic numbers
		const int Torso = 24, legsStand = 16, die = 96;
		const int larmStand = 0, rarmStand = 8, rarm1H = 99, larm2H = 107, rarm2H = 115, rarmShoot = 123;
		int[] legsWalk = { 32, 40, 48, 56, 64, 72, 80, 88 };
		int[] yoffWalk = {3, 3, 2, 1, 0, 0, 1, 2}; // bobbing up and down
		int[] xoffWalka = {0, 0, 1, 2, 3, 3, 2, 1};
		int[] xoffWalkb = {0, 0, -1, -2, -3, -3, -2, -1};
		int[] yoffStand = {2, 1, 1, 0, 0, 0, 0, 0};
		int[] offX = { 8, 10, 5, 2, -8, -10, -5, -2 }; // for the weapons
		int[] offY = { -6, -3, 0, 0, 2, -3, -7, -9 }; // for the weapons
		int[] offX2 = { -8, 2, 7, 13, 7, 0, -3, -15 }; // for the weapons
		int[] offY2 = { 1, -4, -2, 0, 3, 3, 5, 0 }; // for the weapons
		int[] offX3 = { 0, 6, 6, 12, -4, -5, -5, -13 }; // for the left handed rifles
		int[] offY3 = { -4, -4, -1, 0, 5, 0, 1, 0 }; // for the left handed rifles
		const int offXSprite = 16; // sprites are double width

		if (_unit.getStatus() == UnitStatus.STATUS_COLLAPSING)
		{
			torso = _unitSurface.getFrame(die + _unit.getFallingPhase());
			torso.setX(offXSprite);
			drawRecolored(torso);
			return;
		}

		int unitDir = _unit.getDirection();
		int walkPhase = _unit.getWalkingPhase();

		torso = _unitSurface.getFrame(Torso + unitDir);
		leftArm = _unitSurface.getFrame(larmStand + unitDir);
		rightArm = _unitSurface.getFrame(rarmStand + unitDir);


		// when walking, torso(fixed sprite) has to be animated up/down
		if (_unit.getStatus() == UnitStatus.STATUS_WALKING)
		{
			int xoffWalk = 0;
			if (unitDir < 3)
				xoffWalk = xoffWalka[walkPhase];
			if (unitDir < 7 && unitDir > 3)
				xoffWalk = xoffWalkb[walkPhase];
			torso.setY(yoffWalk[walkPhase]);
			torso.setX(xoffWalk);
			legs = _unitSurface.getFrame(legsWalk[unitDir] + walkPhase);
			rightArm.setY(yoffWalk[walkPhase]);
			leftArm.setY(yoffWalk[walkPhase]);
			rightArm.setX(xoffWalk);
			leftArm.setX(xoffWalk);
		}
		else
		{
			legs = _unitSurface.getFrame(legsStand + unitDir);
		}

		sortRifles();

		// holding an item
		if (_itemR != null)
		{
			// draw handob item
			if (_unit.getStatus() == UnitStatus.STATUS_AIMING && _itemR.getRules().isTwoHanded())
			{
				int dir = (unitDir + 2)%8;
				itemR = _itemSurfaceR.getFrame(_itemR.getRules().getHandSprite() + dir);
				itemR.setX(offX[unitDir]);
				itemR.setY(offY[unitDir]);
			}
			else
			{
				itemR = _itemSurfaceR.getFrame(_itemR.getRules().getHandSprite() + unitDir);
				itemR.setX(0);
				itemR.setY(0);
				if (!_itemR.getRules().isTwoHanded())
				{
					itemR.setY(yoffStand[unitDir]);
				}
			}


			// draw arms holding the item
			if (_itemR.getRules().isTwoHanded())
			{
				leftArm = _unitSurface.getFrame(larm2H + unitDir);
				if (_unit.getStatus() == UnitStatus.STATUS_AIMING)
				{
					rightArm = _unitSurface.getFrame(rarmShoot + unitDir);
				}
				else
				{
					rightArm = _unitSurface.getFrame(rarm2H + unitDir);
				}
			}
			else
			{
				rightArm = _unitSurface.getFrame(rarm1H + unitDir);
			}


			// the fixed arm(s) have to be animated up/down when walking
			if (_unit.getStatus() == UnitStatus.STATUS_WALKING)
			{
				itemR.setY(yoffWalk[walkPhase]);
				rightArm.setY(yoffWalk[walkPhase]);
				if (_itemR.getRules().isTwoHanded())
					leftArm.setY(yoffWalk[walkPhase]);
			}
		}
		//if we are left handed or dual wielding...
		if (_itemL != null)
		{
			leftArm = _unitSurface.getFrame(larm2H + unitDir);
			itemL = _itemSurfaceL.getFrame(_itemL.getRules().getHandSprite() + unitDir);
			if (!_itemL.getRules().isTwoHanded())
			{
				itemL.setX(offX2[unitDir]);
				itemL.setY(offY2[unitDir]);
			}
			else
			{
				itemL.setX(0);
				itemL.setY(0);
				if (!_itemL.getRules().isTwoHanded())
				{
					itemL.setY(yoffStand[unitDir]);
				}
				rightArm = _unitSurface.getFrame(rarm2H + unitDir);
			}

			if (_unit.getStatus() == UnitStatus.STATUS_AIMING && _itemL.getRules().isTwoHanded())
			{
				int dir = (unitDir + 2)%8;
				itemL = _itemSurfaceL.getFrame(_itemL.getRules().getHandSprite() + dir);
				itemL.setX(offX3[unitDir]);
				itemL.setY(offY3[unitDir]);
				rightArm = _unitSurface.getFrame(rarmShoot + unitDir);
			}

			if (_unit.getStatus() == UnitStatus.STATUS_WALKING)
			{
				leftArm.setY(yoffWalk[walkPhase]);
				itemL.setY(offY2[unitDir] + yoffWalk[walkPhase]);
				if (_itemL.getRules().isTwoHanded())
					rightArm.setY(yoffWalk[walkPhase]);
			}
		}
		// offset everything but legs when kneeled
		if (_unit.getStatus() != UnitStatus.STATUS_WALKING)
		{
			leftArm.setY(0);
			rightArm.setY(0);
			torso.setY(0);
		}
		// offset everything to the right by 16 pixels.
		// this is because we draw the sprites double wide, to accommodate weapons in-hand
		torso.setX(offXSprite);
		legs.setX(offXSprite);
		leftArm.setX(offXSprite);
		rightArm.setX(offXSprite);
		if (itemR != null)
			itemR.setX(itemR.getX() + offXSprite);
		if (itemL != null)
			itemL.setX(itemL.getX() + offXSprite);

		// blit order depends on unit direction.
		switch (unitDir)
		{
			case 0: itemR?.blit(this); itemL?.blit(this); drawRecolored(leftArm); drawRecolored(legs); drawRecolored(torso); drawRecolored(rightArm); break;
			case 1: drawRecolored(leftArm); drawRecolored(legs); itemL?.blit(this); drawRecolored(torso); itemR?.blit(this); drawRecolored(rightArm); break;
			case 2: drawRecolored(leftArm); drawRecolored(legs); drawRecolored(torso); drawRecolored(rightArm); itemR?.blit(this); itemL?.blit(this); break;
			case 3: drawRecolored(legs); drawRecolored(torso); drawRecolored(leftArm); drawRecolored(rightArm); itemR?.blit(this); itemL?.blit(this); break;
			case 4:	drawRecolored(rightArm); drawRecolored(legs); drawRecolored(torso); drawRecolored(leftArm); itemR?.blit(this); itemL?.blit(this); break;
			case 5:	drawRecolored(rightArm); drawRecolored(legs); drawRecolored(torso); drawRecolored(leftArm); itemR?.blit(this); itemL?.blit(this); break;
			case 6: drawRecolored(rightArm); drawRecolored(legs); itemR?.blit(this); itemL?.blit(this); drawRecolored(torso); drawRecolored(leftArm); break;
			case 7:	itemR?.blit(this); itemL?.blit(this); drawRecolored(leftArm); drawRecolored(rightArm); drawRecolored(legs); drawRecolored(torso); break;
		}
		torso.setX(0);
		legs.setX(0);
		leftArm.setX(0);
		rightArm.setX(0);
		if (itemR != null)
			itemR.setX(itemR.getX() + 0);
		if (itemL != null)
			itemL.setX(itemL.getX() + 0);
	}

	/**
	 * Drawing routine for chryssalid.
	 */
	void drawRoutine7()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		Surface torso = null, legs = null, leftArm = null, rightArm = null;
		// magic numbers
		const int Torso = 24, legsStand = 16, die = 224;
		const int larmStand = 0, rarmStand = 8;
		int[] legsWalk = { 48, 48+24, 48+24*2, 48+24*3, 48+24*4, 48+24*5, 48+24*6, 48+24*7 };
		int[] larmWalk = { 32, 32+24, 32+24*2, 32+24*3, 32+24*4, 32+24*5, 32+24*6, 32+24*7 };
		int[] rarmWalk = { 40, 40+24, 40+24*2, 40+24*3, 40+24*4, 40+24*5, 40+24*6, 40+24*7 };
		int[] yoffWalk = {1, 0, -1, 0, 1, 0, -1, 0}; // bobbing up and down
		const int offXSprite = 16; // sprites are double width

		if (_unit.getStatus() == UnitStatus.STATUS_COLLAPSING)
		{
			torso = _unitSurface.getFrame(die + _unit.getFallingPhase());
			torso.setX(offXSprite);
			drawRecolored(torso);
			return;
		}

		int unitDir = _unit.getDirection();
		int walkPhase = _unit.getWalkingPhase();

		torso = _unitSurface.getFrame(Torso + unitDir);

		// when walking, torso(fixed sprite) has to be animated up/down
		if (_unit.getStatus() == UnitStatus.STATUS_WALKING)
		{
			torso.setY(yoffWalk[walkPhase]);
			legs = _unitSurface.getFrame(legsWalk[unitDir] + walkPhase);
			leftArm = _unitSurface.getFrame(larmWalk[unitDir] + walkPhase);
			rightArm = _unitSurface.getFrame(rarmWalk[unitDir] + walkPhase);
		}
		else
		{

			legs = _unitSurface.getFrame(legsStand + unitDir);
			leftArm = _unitSurface.getFrame(larmStand + unitDir);
			rightArm = _unitSurface.getFrame(rarmStand + unitDir);
			leftArm.setY(0);
			rightArm.setY(0);
			torso.setY(0);
		}
		// offset everything to the right by 16 pixels.
		// this is because we draw the sprites double wide, to accommodate weapons in-hand
		torso.setX(offXSprite);
		legs.setX(offXSprite);
		leftArm.setX(offXSprite);
		rightArm.setX(offXSprite);

		// blit order depends on unit direction
		switch (unitDir)
		{
			case 0: drawRecolored(leftArm); drawRecolored(legs); drawRecolored(torso); drawRecolored(rightArm); break;
			case 1: drawRecolored(leftArm); drawRecolored(legs); drawRecolored(torso); drawRecolored(rightArm); break;
			case 2: drawRecolored(leftArm); drawRecolored(legs); drawRecolored(torso); drawRecolored(rightArm); break;
			case 3: drawRecolored(legs); drawRecolored(torso); drawRecolored(leftArm); drawRecolored(rightArm); break;
			case 4: drawRecolored(rightArm); drawRecolored(legs); drawRecolored(torso); drawRecolored(leftArm); break;
			case 5: drawRecolored(rightArm); drawRecolored(legs); drawRecolored(torso); drawRecolored(leftArm); break;
			case 6: drawRecolored(rightArm); drawRecolored(legs); drawRecolored(torso); drawRecolored(leftArm); break;
			case 7: drawRecolored(leftArm); drawRecolored(rightArm); drawRecolored(legs); drawRecolored(torso); break;
		}
	}

	/**
	 * Drawing routine for silacoids.
	 */
	void drawRoutine8()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		Surface legs = null;
		// magic numbers
		const int Body = 0, aim = 5, die = 6;
		int[] Pulsate = { 0, 1, 2, 3, 4, 3, 2, 1 };
		const int offXSprite = 16; // sprites are double width

		legs = _unitSurface.getFrame(Body + Pulsate[_animationFrame]);
		_redraw = true;

		if (_unit.getStatus() == UnitStatus.STATUS_COLLAPSING)
			legs = _unitSurface.getFrame(die + _unit.getFallingPhase());

		else if (_unit.getStatus() == UnitStatus.STATUS_AIMING)
			legs = _unitSurface.getFrame(aim);

		// offset everything to the right by 16 pixels.
		// this is because we draw the sprites double wide, to accommodate weapons in-hand
		legs.setX(offXSprite);

		drawRecolored(legs);
	}

	/**
	 * Drawing routine for celatids.
	 */
	void drawRoutine9()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		Surface torso = null;
		// magic numbers
		const int Body = 0, die = 25;
		const int offXSprite = 16; // sprites are double width

		torso = _unitSurface.getFrame(Body + _animationFrame);
		_redraw = true;

		if (_unit.getStatus() == UnitStatus.STATUS_COLLAPSING)
			torso = _unitSurface.getFrame(die + _unit.getFallingPhase());

		// offset everything to the right by 16 pixels.
		// this is because we draw the sprites double wide, to accommodate weapons in-hand
		torso.setX(offXSprite);

		drawRecolored(torso);
	}

	/**
	 * Drawing routine for tftd tanks.
	 */
	void drawRoutine11()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		int[] offTurretX = { -2, -6, -5, 0, 5, 6, 2, 0 }; // turret offsets
		int[] offTurretYAbove = { 5, 3, 0, 0, 0, 3, 5, 4 }; // turret offsets
		int[] offTurretYBelow = { -11, -13, -16, -16, -16, -13, -11, -12 }; // turret offsets
		const int offXSprite = 16; // sprites are double width

		int body = 0;
		int animFrame = _unit.getWalkingPhase() % 4;
		if (_unit.getMovementType() == MovementType.MT_FLY)
		{
			body = 128;
			animFrame = _animationFrame % 4;
		}

		Surface s = _unitSurface.getFrame(body + (_part * 4) + 16 * _unit.getDirection() + animFrame);
		s.setY(4);
		s.setX(offXSprite);
		drawRecolored(s);

		int turret = _unit.getTurretType();
		// draw the turret, overlapping all 4 parts
		if ((_part == 3 || _part == 0) && turret != -1 && !_unit.getFloorAbove())
		{
			s = _unitSurface.getFrame(256 + (turret * 8) + _unit.getTurretDirection());
			s.setX(offTurretX[_unit.getDirection()] + offXSprite);
			if (_part == 3)
				s.setY(offTurretYBelow[_unit.getDirection()]);
			else
				s.setY(offTurretYAbove[_unit.getDirection()]);
			drawRecolored(s);
		}
	}

	/**
	 * Drawing routine for hallucinoids (routine 12) and biodrones (routine 16).
	 */
	void drawRoutine12()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		const int die = 8;
		const int offXSprite = 16; // sprites are double width

		Surface s = null;
		s = _unitSurface.getFrame((_part * 8) + _animationFrame);
		_redraw = true;

		if ( (_unit.getStatus() == UnitStatus.STATUS_COLLAPSING) && (_drawingRoutine == 16) )
		{
			// biodrone death frames
			s = _unitSurface.getFrame(die + _unit.getFallingPhase());
		}
		s.setX(offXSprite);
		drawRecolored(s);
	}

	/**
	 * Drawing routine for tentaculats.
	 */
	void drawRoutine19()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		Surface s = null;
		// magic numbers
		const int stand = 0, move = 8, die = 16;
		const int offXSprite = 16; // sprites are double width

		if (_unit.getStatus() == UnitStatus.STATUS_COLLAPSING)
		{
			s = _unitSurface.getFrame(die + _unit.getFallingPhase());
		}
		else if (_unit.getStatus() == UnitStatus.STATUS_WALKING)
		{
			s = _unitSurface.getFrame(move + _unit.getDirection());
		}
		else
		{
			s = _unitSurface.getFrame(stand + _unit.getDirection());
		}
		s.setX(offXSprite);
		drawRecolored(s);
	}

	/**
	 * Drawing routine for triscenes.
	 */
	void drawRoutine20()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		Surface s = null;
		const int offXSprite = 16; // sprites are double width

		if (_unit.getStatus() == UnitStatus.STATUS_WALKING)
		{
			s = _unitSurface.getFrame((_unit.getWalkingPhase()/2%4) + 5 * (_part + 4 * _unit.getDirection()));
		}
		else
		{
			s = _unitSurface.getFrame(5 * (_part + 4 * _unit.getDirection()));
		}
		s.setX(offXSprite);
		drawRecolored(s);
	}

	/**
	 * Drawing routine for xarquids.
	 */
	void drawRoutine21()
	{
		if (_unit.isOut())
		{
			// unit is drawn as an item
			return;
		}

		Surface s = null;
		const int offXSprite = 16; // sprites are double width

		s = _unitSurface.getFrame((_part * 4) + (_unit.getDirection() * 16) + (_animationFrame % 4));
		_redraw = true;
		s.setX(offXSprite);
		drawRecolored(s);
	}
}
