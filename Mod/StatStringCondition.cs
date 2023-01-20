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

internal class StatStringCondition
{
    string _conditionName;
    int _minVal;
    int _maxVal;

    /**
     * Creates a blank StatStringCondition.
     * @param conditionName Name of the condition.
     * @param minVal Minimum value.
     * @param maxVal Maximum value.
     */
    internal StatStringCondition(string conditionName, int minVal, int maxVal)
    {
        _conditionName = conditionName;
        _minVal = minVal;
        _maxVal = maxVal;
    }

    /**
     * Cleans up the extra StatStringCondition.
     */
    ~StatStringCondition() { }
}
