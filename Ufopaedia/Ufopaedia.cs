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

namespace SharpXcom.Ufopaedia;

/**
 * This static class encapsulates all functions related to Ufopaedia
 * for the game.
 * Main purpose is to open Ufopaedia from Geoscape, navigate between articles
 * and release new articles after successful research.
 */
internal class Ufopaedia
{
    // This section is meant for articles, that have to be activated,
    // but have no own entry in a list. E.g. Ammunition items.
    // Maybe others as well, that should just not be selectable.
    internal const string UFOPAEDIA_NOT_AVAILABLE = "STR_NOT_AVAILABLE";
}
