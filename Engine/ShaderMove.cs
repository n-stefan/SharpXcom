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

	ShaderMove(ShaderMove<TPixel> f) : base(f)
	{
		_move_x = f._move_x;
		_move_y = f._move_y;
	}

	ShaderMove(List<TPixel> f, int max_x, int max_y) : base(f, max_x, max_y)
	{
		_move_x = 0;
		_move_y = 0;
	}

	ShaderMove(List<TPixel> f, int max_x, int max_y, int move_x, int move_y) : base(f, max_x, max_y)
	{
		_move_x = move_x;
		_move_y = move_y;
	}

	GraphSubset getImage()
	{
		return base._range_domain.offset(_move_x, _move_y);
	}

	void setMove(int x, int y)
	{
		_move_x = x;
		_move_y = y;
	}

	void addMove(int x, int y)
	{
		_move_x += x;
		_move_y += y;
	}
};

partial class Shader
{
    /**
	 * Create warper from Surface
	 * @param s standard 8bit OpenXcom surface
	 * @return
	 */
    internal static ShaderMove<byte> ShaderSurface(Surface s) =>
		new(s);
}
