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

namespace SharpXcom.Engine;

/**
 * Fast line clip.
 */
internal class FastLineClip
{
    double FC_xn, FC_yn, FC_xk, FC_yk;
    internal double Wxlef, Wxrig, Wytop, Wybot;

    /// Creates a fastlineclip.
    internal FastLineClip(double Wxl, double Wxr, double Wyt, double Wyb)
    {
        FC_xn = 0;
        FC_yn = 0;
        FC_xk = 0;
        FC_yk = 0;

        Wxlef = Wxl;
        Wxrig = Wxr;
        Wytop = Wyt;
        Wybot = Wyb;
    }

    /// Cleans up the fastlineclip.
    ~FastLineClip() { }
}
