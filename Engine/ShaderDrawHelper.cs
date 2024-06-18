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

interface IColorFunc<DestType, Src0Type, Src1Type, Src2Type, Src3Type>
{
    void func(ref DestType destType, Src0Type src0Type, Src1Type src1Type, Src2Type src2Type, Src3Type src3Type);
}

interface IShaderParam { }

/**
 * This is surface argument to `ShaderDraw`.
 * every pixel of this surface will have type `Pixel`.
 * Modify pixels of this surface, that will modifying original data.
 */
class ShaderBase<TPixel> : IShaderParam
{
	protected TPixel _origin;
    protected GraphSubset _range_base;
    protected GraphSubset _range_domain;
    protected int _pitch;

	///copy constructor
	internal ShaderBase(ShaderBase<TPixel> s)
	{
		_origin = s.ptr();
		_range_base = s._range_base;
		_range_domain = s.getDomain();
		_pitch = s.pitch();
	}

	/**
	 * create surface using vector `f` as data source.
	 * surface will have `max_y` x `max_x` dimensions.
	 * size of `f` should be bigger than `max_y*max_x`.
	 * Attention: after use of this constructor you change size of `f` then `_orgin` will be invalid
	 * and use of this object will cause memory exception.
     * @param f vector that are treated as surface
     * @param max_x x dimension of `f`
     * @param max_y y dimension of `f`
     */
	internal ShaderBase(List<TPixel> f, int max_x, int max_y)
	{
		_origin = f[0];
		_range_base = new GraphSubset(max_x, max_y);
		_range_domain = new GraphSubset(max_x, max_y);
		_pitch = max_x;
	}

    /**
	 * create surface using surface `s` as data source.
	 * surface will have same dimensions as `s`.
	 * Attention: after use of this constructor you change size of surface `s`
	 * then `_orgin` will be invalid and use of this object will cause memory exception.
     * @param s vector that are treated as surface
     */
    internal ShaderBase(Surface s)
    {
        _origin = (TPixel)(object)s.getSurface().pixels;
        _range_base = new GraphSubset(s.getWidth(), s.getHeight());
		_range_domain = new GraphSubset(s.getWidth(), s.getHeight());
		_pitch = s.getSurface().pitch;
    }

    internal GraphSubset getDomain() =>
		_range_domain;

    internal GraphSubset getBaseDomain() =>
		_range_base;

    internal void setDomain(GraphSubset g) =>
		_range_domain = GraphSubset.intersection(g, _range_base);

    internal ref TPixel ptr() =>
		ref _origin;

    internal int pitch() =>
		_pitch;

    internal GraphSubset getImage() =>
		_range_domain;
}

/**
 * This is scalar argument to `ShaderDraw`.
 * when used in `ShaderDraw` return value of `t` to `ColorFunc::func` for every pixel
 */
class Scalar<T> : IShaderParam
{
    T @ref;

    internal Scalar(T t) =>
		@ref = t;
}

/**
 * This is empty argument to `ShaderDraw`.
 * when used in `ShaderDraw` return always 0 to `ColorFunc::func` for every pixel
 */
class Nothing : IShaderParam { }

class controller_base<T, TPixel>
	where T : ShaderBase<TPixel>
	where TPixel : IAdditionOperators<TPixel, TPixel, TPixel>
{
	TPixel data;
	TPixel ptr_pos_y;
	TPixel ptr_pos_x;
	GraphSubset range;
	int start_x;
	int start_y;
	KeyValuePair<int, int> step;

	protected controller_base(TPixel @base, GraphSubset d, GraphSubset r, KeyValuePair<int, int> s)
	{
		data = @base + (TPixel)(object)(d.beg_x * s.Key) + (TPixel)(object)(d.beg_y * s.Value);
		ptr_pos_y = default;
		ptr_pos_x = default;
		range = r;
		start_x = 0;
		start_y = 0;
		step = s;
	}

    internal GraphSubset get_range() =>
		range;

    internal void mod_range(ref GraphSubset r) =>
		r = GraphSubset.intersection(range, r);

    internal void set_range(GraphSubset r)
	{
		start_x = r.beg_x - range.beg_x;
		start_y = r.beg_y - range.beg_y;
		range = r;
	}

    internal void mod_y(int _, int __) =>
		ptr_pos_y = data + (TPixel)(object)(step.Key * start_x) + (TPixel)(object)(step.Value * start_y);

    internal void set_y(int begin, int _) =>
		ptr_pos_y += (TPixel)(object)(step.Value * begin);

    internal void inc_y() =>
		ptr_pos_y += (TPixel)(object)step.Value;

    internal void mod_x(int _, int __) =>
		ptr_pos_x = ptr_pos_y;

    internal void set_x(int begin, int _) =>
		ptr_pos_x += (TPixel)(object)(step.Key * begin);

    internal void inc_x() =>
		ptr_pos_x += (TPixel)(object)step.Key;

    internal ref TPixel get_ref() =>
		ref ptr_pos_x;
}

class controller<T, TPixel> : controller_base<T, TPixel>
	where T : ShaderBase<TPixel>
	where TPixel : IAdditionOperators<TPixel, TPixel, TPixel>
{
	internal controller(ShaderBase<TPixel> f) : base(f.ptr(), f.getDomain(), f.getImage(), KeyValuePair.Create(1, f.pitch())) { }
}
