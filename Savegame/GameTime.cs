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

namespace SharpXcom.Savegame;

/**
 * Stores the current ingame time/date according to GMT.
 * Takes care of managing and representing each component,
 * as well as common time operations.
 */
internal class GameTime
{
    int _second, _minute, _hour, _weekday, _day, _month, _year;

    /**
     * Initializes a new ingame time with a certain starting point.
     * @param weekday Starting weekday.
     * @param day Starting day.
     * @param month Starting month.
     * @param year Starting year.
     * @param hour Starting hour.
     * @param minute Starting minute.
     * @param second Starting second.
     */
    internal GameTime(int weekday, int day, int month, int year, int hour, int minute, int second)
    {
        _second = second;
        _minute = minute;
        _hour = hour;
        _weekday = weekday;
        _day = day;
        _month = month;
        _year = year;
    }

    /**
     *
     */
    ~GameTime() { }

    /**
     * Saves the time to a YAML file.
     * @return YAML node.
     */
    internal YamlNode save()
    {
        var node = new YamlMappingNode
        {
            { "second", _second.ToString() },
            { "minute", _minute.ToString() },
            { "hour", _hour.ToString() },
            { "weekday", _weekday.ToString() },
            { "day", _day.ToString() },
            { "month", _month.ToString() },
            { "year", _year.ToString() }
        };
        return node;
    }

    /**
     * Loads the time from a YAML file.
     * @param node YAML node.
     */
    internal void load(YamlNode node)
    {
	    _second = int.Parse(node["second"].ToString());
	    _minute = int.Parse(node["minute"].ToString());
	    _hour = int.Parse(node["hour"].ToString());
	    _weekday = int.Parse(node["weekday"].ToString());
	    _day = int.Parse(node["day"].ToString());
	    _month = int.Parse(node["month"].ToString());
	    _year = int.Parse(node["year"].ToString());
    }

    /**
     * Returns the localized representation of the current
     * ingame day with the cardinal operator.
     * @param lang Pointer to current language.
     * @return Localized day string.
     */
    internal string getDayString(Language lang)
    {
	    string s;
	    switch (_day)
	    {
	        case 1:
	        case 21:
	        case 31:
		        s = "STR_DATE_FIRST";
		        break;
	        case 2:
	        case 22:
		        s = "STR_DATE_SECOND";
		        break;
	        case 3:
	        case 23:
		        s = "STR_DATE_THIRD";
		        break;
	        default:
		        s = "STR_DATE_FOURTH";
                break;
	    }
	    return lang.getString(s).arg(_day);
    }

    /**
     * Returns a localizable-string representation of
     * the current ingame month.
     * @return Month string ID.
     */
    internal string getMonthString()
    {
        string[] months = { "STR_JAN", "STR_FEB", "STR_MAR", "STR_APR", "STR_MAY", "STR_JUN", "STR_JUL", "STR_AUG", "STR_SEP", "STR_OCT", "STR_NOV", "STR_DEC" };
        return months[_month - 1];
    }

    /**
     * Returns the current ingame year.
     * @return Year.
     */
    internal int getYear() =>
	    _year;

    /**
     * Returns the current ingame hour.
     * @return Hour (0-23).
     */
    internal int getHour() =>
	    _hour;

    /**
     * Returns the current ingame minute.
     * @return Minute (0-59).
     */
    internal int getMinute() =>
	    _minute;
}
