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

internal struct GraphSubset
{
    //define part of surface
    internal int beg_x, end_x;
    internal int beg_y, end_y;

    internal GraphSubset(int max_x, int max_y)
    {
        beg_x = 0;
        end_x = max_x;
        beg_y = 0;
        end_y = max_y;
    }

	internal int size_x() =>
		end_x - beg_x;

	internal int size_y() =>
		end_y - beg_y;

    static void intersection_range(ref int begin_a, ref int end_a, ref int begin_b, ref int end_b)
	{
		if (begin_a >= end_b || begin_b >= end_a)
		{
			//intersection is empty
			end_a = begin_a;
		}
		else
		{
			begin_a = Math.Max(begin_a, begin_b);
			end_a = Math.Min(end_a, end_b);
		}
	}

	internal static GraphSubset intersection(GraphSubset a, GraphSubset b)
	{
		GraphSubset ret = a;
		intersection_range(ref ret.beg_x, ref ret.end_x, ref b.beg_x, ref b.end_x);
		intersection_range(ref ret.beg_y, ref ret.end_y, ref b.beg_y, ref b.end_y);
		return ret;
	}
}
