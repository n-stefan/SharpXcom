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

namespace SharpXcom.Geoscape;

internal struct Cord : IAdditionOperators<Cord, Cord, Cord>
{
    internal double x, y, z;

    public Cord()
    {
        x = 0.0;
        y = 0.0;
        z = 0.0;
    }

    internal Cord(double px, double py, double pz)
    {
        x = px;
        y = py;
        z = pz;
    }

    public static Cord operator +(Cord a, Cord b) =>
        new(a.x + b.x, a.y + b.y, a.z + b.z);

    public static Cord operator -(Cord a) =>
        new(-a.x, -a.y, -a.z);

    public static Cord operator -(Cord a, Cord b) =>
        new(a.x - b.x, a.y - b.y, a.z - b.z);

    public static Cord operator *(Cord a, double b) =>
        new(a.x * b, a.y * b, a.z * b);

    public static Cord operator /(Cord a, double b)
	{
		double re = 1.0/b;
		return new(a.x * re, a.y * re, a.z * re);
	}

    public static bool operator ==(Cord a, Cord b) =>
		AreSame(a.x, b.x) && AreSame(a.y, b.y) && AreSame(a.z, b.z);

    public static bool operator !=(Cord a, Cord b) =>
        !(a == b);

    public static explicit operator Cord(CordPolar pol)
    {
	    var x = Math.Sin(pol.lon) * Math.Cos(pol.lat);
	    var y = Math.Sin(pol.lat);
	    var z = Math.Cos(pol.lon) * Math.Cos(pol.lat);
        return new Cord(x, y, z);
    }

    internal double norm() =>
        Math.Sqrt(x * x + y * y + z * z);
}

struct CordPolar
{
	internal double lon, lat;

	internal CordPolar(double plon, double plat)
	{
		lon = plon;
		lat = plat;
	}

	CordPolar(CordPolar pol)
	{
		lon = pol.lon;
		lat = pol.lat;
	}

	public CordPolar()
	{
		lon = 0;
		lat = 0;
	}

    public static explicit operator CordPolar(Cord c)
    {
	    double inv = 1/c.norm();
	    var lat = Math.Asin(c.y * inv);
	    var lon = Math.Atan2(c.x, c.z);
        return new CordPolar(lon, lat);
    }
}
