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

internal class Particle
{
    float _xOffset, _yOffset, _density;
    byte _color, _opacity, _size;

    /**
     * Creates a particle.
     * @param xOffset the horizontal offset for this particle (relative to the tile in screen space)
     * @param yOffset the vertical offset for this particle (relative to the tile in screen space)
     * @param density the density of the particle dictates the speed at which it moves upwards, and is inversely proportionate to its size.
     * @param color the color set to use from the transparency LUTs
     * @param opacity another reference for the LUT, this one is divided by 5 for the actual offset to use.
     */
    internal Particle(float xOffset, float yOffset, float density, byte color, byte opacity)
    {
        _xOffset = xOffset;
        _yOffset = yOffset;
        _density = density;
        _color = color;
        _opacity = opacity;
        _size = 0;

        //size is initialized at 0
        if (density < 100)
        {
            _size = 3;
        }
        else if (density < 125)
        {
            _size = 2;
        }
        else if (density < 150)
        {
            _size = 1;
        }
    }

    /**
     * Cleans up a particle.
     */
    ~Particle() { }

    /**
     * Animates the particle.
     * @return if we are done animating this particle yet.
     */
    internal bool animate()
    {
        _yOffset = (float)(_yOffset - ((320 - _density) / 256.0));
        _opacity--;
        _xOffset = (float)(_xOffset + (RNG.seedless(0, 1) * 2 - 1) * (0.25 + (float)RNG.seedless(0, 9) / 30));
        if (_opacity == 0)
        {
            return false;
	    }
	    return true;
    }

	/// Get the color.
	internal byte getColor() =>
        _color;

	/// Get the opacity.
	internal byte getOpacity() =>
        (byte)Math.Min((_opacity + 7) / 10, 3);

	/// Get the horizontal shift.
	internal float getX() =>
        _xOffset;

	/// Get the vertical shift.
	internal float getY() =>
        _yOffset;

	/// Get the size value.
	internal int getSize() =>
        _size;
}
