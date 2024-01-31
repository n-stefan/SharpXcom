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

internal class ShaderRepeat<TPixel> : ShaderBase<TPixel>
{
	int _off_x;
	int _off_y;

	internal ShaderRepeat(Surface s) : base(s) =>
		setOffset(0, 0);

	internal ShaderRepeat(List<TPixel> f, int max_x, int max_y) : base(f, max_x, max_y) =>
		setOffset(0, 0);

	internal void setOffset(int x, int y)
	{
		_off_x = x;
		_off_y = y;
	}

	internal void addOffset(int x, int y)
	{
		_off_x += x;
		_off_y += y;
	}
}
