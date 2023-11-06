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
	internal void setBattleUnit(BattleUnit unit, int part)
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
}
