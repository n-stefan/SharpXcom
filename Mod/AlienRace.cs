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

namespace SharpXcom.Mod;

/**
 * Represents a specific race "family", or a "main race" if you wish.
 * Here is defined which ranks it contains and also which accompanying terror units.
 */
internal class AlienRace : IRule
{
    string _id;

    /**
     * Creates a blank alien race.
     * @param id String defining the id.
     */
    AlienRace(string id) =>
        _id = id;

    public IRule Create(string type) =>
        new AlienRace(type);

    ~AlienRace() { }
}
