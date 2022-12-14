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

namespace SharpXcom.Engine;

internal class ShaderMove<TPixel> : ShaderBase<TPixel>
{
    int _move_x;
    int _move_y;

    internal ShaderMove(Surface s) : base(s)
    {
        _move_x = s.getX();
        _move_y = s.getY();
    }

    internal ShaderMove(Surface s, int move_x, int move_y) : base(s)
    {
        _move_x = move_x;
        _move_y = move_y;
    }

    /**
     * Create warper from Surface
     * @param s standard 8bit OpenXcom surface
     * @return
     */
    internal static ShaderMove<TPixel> ShaderSurface(Surface s) =>
        new(s);
}
