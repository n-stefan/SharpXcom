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

internal class Shader
{
	/**
	 * Universal blit function
	 * @tparam ColorFunc class that contains static function `func` that get 5 arguments
	 * function is used to modify these arguments.
	 * @param dest_frame destination surface modified by function.
	 * @param src0_frame surface or scalar
	 * @param src1_frame surface or scalar
	 * @param src2_frame surface or scalar
	 * @param src3_frame surface or scalar
	 */
	static void ShaderDraw<ColorFunc, DestType, Src0Type, Src1Type, Src2Type, Src3Type>(DestType dest_frame, Src0Type src0_frame, Src1Type src1_frame, Src2Type src2_frame, Src3Type src3_frame)
         where DestType : ShaderBase<DestType>, INumber<DestType>
         where Src0Type : ShaderBase<Src0Type>, INumber<Src0Type>
         where Src1Type : ShaderBase<Src1Type>, INumber<Src1Type>
         where Src2Type : ShaderBase<Src2Type>, INumber<Src2Type>
         where Src3Type : ShaderBase<Src3Type>, INumber<Src3Type>
    {
        //creating helper objects
        var dest = new controller<DestType>(dest_frame);
		var src0 = new controller<Src0Type>(src0_frame);
		var src1 = new controller<Src1Type>(src1_frame);
		var src2 = new controller<Src2Type>(src2_frame);
		var src3 = new controller<Src3Type>(src3_frame);

		//get basic draw range in 2d space
		GraphSubset end_temp = dest.get_range();

		//intersections with src ranges
		src0.mod_range(ref end_temp);
		src1.mod_range(ref end_temp);
		src2.mod_range(ref end_temp);
		src3.mod_range(ref end_temp);

		GraphSubset end = end_temp;
		if (end.size_x() == 0 || end.size_y() == 0)
			return;
		//set final draw range in 2d space
		dest.set_range(end);
		src0.set_range(end);
		src1.set_range(end);
		src2.set_range(end);
		src3.set_range(end);

		int begin_y = 0, end_y = end.size_y();
		//determining iteration range in y-axis
		dest.mod_y(begin_y, end_y);
		src0.mod_y(begin_y, end_y);
		src1.mod_y(begin_y, end_y);
		src2.mod_y(begin_y, end_y);
		src3.mod_y(begin_y, end_y);
		if (begin_y>=end_y)
			return;
		//set final iteration range
		dest.set_y(begin_y, end_y);
		src0.set_y(begin_y, end_y);
		src1.set_y(begin_y, end_y);
		src2.set_y(begin_y, end_y);
		src3.set_y(begin_y, end_y);

		//iteration on y-axis
		for (int y = end_y-begin_y; y>0; --y, dest.inc_y(), src0.inc_y(), src1.inc_y(), src2.inc_y(), src3.inc_y())
		{
			int begin_x = 0, end_x = end.size_x();
			//determining iteration range in x-axis
			dest.mod_x(begin_x, end_x);
			src0.mod_x(begin_x, end_x);
			src1.mod_x(begin_x, end_x);
			src2.mod_x(begin_x, end_x);
			src3.mod_x(begin_x, end_x);
			if (begin_x>=end_x)
				continue;
			//set final iteration range
			dest.set_x(begin_x, end_x);
			src0.set_x(begin_x, end_x);
			src1.set_x(begin_x, end_x);
			src2.set_x(begin_x, end_x);
			src3.set_x(begin_x, end_x);

			//iteration on x-axis
			for (int x = end_x-begin_x; x>0; --x, dest.inc_x(), src0.inc_x(), src1.inc_x(), src2.inc_x(), src3.inc_x())
			{
				ColorFunc.func(dest.get_ref(), src0.get_ref(), src1.get_ref(), src2.get_ref(), src3.get_ref());
			}
		}
	}

	internal static void ShaderDraw<ColorFunc, DestType, Src0Type, Src1Type, Src2Type>(DestType dest_frame, Src0Type src0_frame, Src1Type src1_frame, Src2Type src2_frame)
         where DestType : ShaderBase<DestType>, INumber<DestType>
         where Src0Type : ShaderBase<Src0Type>, INumber<Src0Type>
         where Src1Type : ShaderBase<Src1Type>, INumber<Src1Type>
         where Src2Type : ShaderBase<Src2Type>, INumber<Src2Type>
	{
        ShaderDraw<ColorFunc, DestType, Src0Type, Src1Type, Src2Type, Nothing>(dest_frame, src0_frame, src1_frame, src2_frame, new Nothing());
	}

	internal static void ShaderDraw<ColorFunc, DestType, Src0Type, Src1Type>(DestType dest_frame, Src0Type src0_frame, Src1Type src1_frame)
         where DestType : ShaderBase<DestType>, INumber<DestType>
         where Src0Type : ShaderBase<Src0Type>, INumber<Src0Type>
         where Src1Type : ShaderBase<Src1Type>, INumber<Src1Type>
    {
		ShaderDraw<ColorFunc, DestType, Src0Type, Src1Type, Nothing, Nothing>(dest_frame, src0_frame, src1_frame, new Nothing(), new Nothing());
    }

	internal static Scalar<T> ShaderScalar<T>(T t) =>
		new(t);
}
