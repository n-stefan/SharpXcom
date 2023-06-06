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

namespace SharpXcom;

internal class FMath
{
    internal const double M_PI = 3.14159265358979323846;
    internal const double M_PI_2 = 1.57079632679489661923;
    internal const double M_PI_4 = 0.785398163397448309616;

    const double DBL_EPSILON = 2.2204460492503131e-016; // smallest such that 1.0+DBL_EPSILON != 1.0

    // Float operations

    internal static bool AreSame(double l, double r) =>
        Math.Abs(l - r) <= DBL_EPSILON * Math.Max(1.0, Math.Max(Math.Abs(l), Math.Abs(r)));

    // Degree operations

    internal static double Deg2Rad(double deg) =>
        deg * M_PI / 180.0;

    internal static double Xcom2Rad(int deg) =>
        deg * 0.125 * M_PI / 180.0;

    internal static double Nautical(double x) =>
        x * (1 / 60.0) * (M_PI / 180.0);
}
