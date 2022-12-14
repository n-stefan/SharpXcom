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

namespace SharpXcom.Mod;

/**
 * Represents the contents of the Geoscape globe,
 * such as world polygons, polylines, etc.
 * @sa Globe
 */
internal class RuleGlobe
{
    List<Polygon> _polygons;
    List<Polyline> _polylines;
    Dictionary<int, Texture> _textures;

    /**
	 * Creates a blank ruleset for globe contents.
	 */
    internal RuleGlobe() { }

	/**
	 *
	 */
	~RuleGlobe()
	{
		_polygons.Clear();
		_polylines.Clear();
		_textures.Clear();
	}

    /**
     * Returns the list of polygons in the globe.
     * @return Pointer to the list of polygons.
     */
    internal List<Polygon> getPolygons() =>
        _polygons;
}
