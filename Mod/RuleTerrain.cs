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
 * Represents a specific type of Battlescape Terrain.
 * - the names of the objectsets needed in this specific terrain.
 * - the mapblocks that can be used to build this terrain.
 * @sa http://www.ufopaedia.org/index.php?title=TERRAIN
 */
internal class RuleTerrain : IRule
{
    List<MapBlock> _mapBlocks;
    string _name, _script;
    int _minDepth, _maxDepth, _ambience;
    double _ambientVolume;

    internal RuleTerrain() { }

    /**
     * RuleTerrain construction.
     */
    RuleTerrain(string name)
    {
        _name = name;
        _script = "DEFAULT";
        _minDepth = 0;
        _maxDepth = 0;
        _ambience = -1;
        _ambientVolume = 0.5;
    }

    public IRule Create(string type) =>
        new RuleTerrain(type);

    /**
     * Ruleterrain only holds mapblocks. Map datafiles are referenced.
     */
    ~RuleTerrain() =>
        _mapBlocks.Clear();
}
