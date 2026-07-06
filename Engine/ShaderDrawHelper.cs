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

/**
 * This is surface argument to `ShaderDraw`.
 * every pixel of this surface will have type `Pixel`.
 * Modify pixels of this surface, that will modifying original data.
 */
class ShaderBase<TPixel>
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
    unsafe internal ShaderBase(Surface s)
    {
        _origin = (TPixel)(object)s.getSurface()->pixels;
        _range_base = new GraphSubset(s.getWidth(), s.getHeight());
        _range_domain = new GraphSubset(s.getWidth(), s.getHeight());
        _pitch = s.getSurface()->pitch;
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
class Scalar<T>
{
    internal T @ref;

    internal Scalar(T t) =>
        @ref = t;
}

/**
 * This is empty argument to `ShaderDraw`.
 * when used in `ShaderDraw` return always 0 to `ColorFunc::func` for every pixel
 */
class Nothing { }

abstract class BaseController
{
    //cant use this function
    internal virtual GraphSubset get_range() => throw new NotImplementedException();
 
	internal virtual void mod_range(ref GraphSubset _) { }

	internal virtual void set_range(GraphSubset _) { }

	internal virtual void mod_y(int _, int __) { }

	internal virtual void set_y(int _, int __) { }

	internal virtual void inc_y() { }

	internal virtual void mod_x(int _, int __) { }

	internal virtual void set_x(int _, int __) { }

	internal virtual void inc_x() { }

    internal virtual ref object get_ref() => throw new NotImplementedException();

    internal static BaseController Create<T>(T type) =>
        type switch
        {
            Nothing => new NothingController(),
            //Scalar<byte> sb => new ScalarController<byte>(sb),
            Scalar<int> si => new ScalarController<int>(si),
            //Scalar<nint> sn => new ScalarController<nint>(sn),
            //Scalar<double> sd => new ScalarController<double>(sd),
            //Scalar<float> sf => new ScalarController<float>(sf),
            //ShaderBase<byte> sbb => new ShaderBaseController<byte>(sbb),
            //ShaderBase<int> sbi => new ShaderBaseController<int>(sbi),
            ShaderBase<nint> sbn => new ShaderBaseController<nint>(sbn),
            //ShaderBase<double> sbd => new ShaderBaseController<double>(sbd),
            //ShaderBase<float> sbf => new ShaderBaseController<float>(sbf),
            _ => throw new NotImplementedException()
        };
}

/// implementation for not used arg
class NothingController : BaseController
{
	int i;

    internal NothingController() => i = 0;

    internal new ref int get_ref() => ref i;
}

/// implementation for scalars types aka `int`, `double`, `float`
class ScalarController<T> : BaseController
{
	T @ref;

    internal ScalarController(Scalar<T> s) => @ref = s.@ref;

    internal new ref T get_ref() => ref @ref;
}

class ShaderBaseController<T> : BaseController
{
    T data;
    T ptr_pos_y;
    T ptr_pos_x;
    GraphSubset range;
    int start_x;
    int start_y;
    KeyValuePair<int, int> step;

    ShaderBaseController(T @base, GraphSubset d, GraphSubset r, KeyValuePair<int, int> s)
    {
        data = Add(@base, (d.beg_x * s.Key) + (d.beg_y * s.Value));
        ptr_pos_y = default;
        ptr_pos_x = default;
        range = r;
        start_x = 0;
        start_y = 0;
        step = s;
    }

    internal ShaderBaseController(ShaderBase<T> f) : this(f.ptr(), f.getDomain(), f.getImage(), KeyValuePair.Create(1, f.pitch())) { }

    internal override GraphSubset get_range() =>
        range;

    internal override void mod_range(ref GraphSubset r) =>
        r = GraphSubset.intersection(range, r);

    internal override void set_range(GraphSubset r)
    {
        start_x = r.beg_x - range.beg_x;
        start_y = r.beg_y - range.beg_y;
        range = r;
    }

    internal override void mod_y(int _, int __) =>
        ptr_pos_y = Add(data, (step.Key * start_x) + (step.Value * start_y));

    internal override void set_y(int begin, int _) =>
        ptr_pos_y = Add(ptr_pos_y, (step.Value * begin));

    internal override void inc_y() =>
        ptr_pos_y = Add(ptr_pos_y, step.Value);

    internal override void mod_x(int _, int __) =>
        ptr_pos_x = ptr_pos_y;

    internal override void set_x(int begin, int _) =>
        ptr_pos_x = Add(ptr_pos_x, (step.Key * begin));

    internal override void inc_x() =>
        ptr_pos_x = Add(ptr_pos_x, step.Key);

    internal new ref T get_ref() =>
        ref ptr_pos_x;

    T Add(T lhs, int rhs) =>
        lhs switch
        {
            //byte b => (T)(object)(b + rhs),
            //int i => (T)(object)(i + rhs),
            nint n => (T)(object)(n + rhs),
            //double d => (T)(object)(d + rhs),
            //float f => (T)(object)(f + rhs),
            _ => throw new NotImplementedException()
        };
}
