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

/**
 * Defines the various severity levels of
 * information logged by the game.
 */
enum SeverityLevel
{
    LOG_FATAL,      /**< Something horrible has happened and the game is going to die! */
    LOG_ERROR,      /**< Something bad happened but we can still move on. */
    LOG_WARNING,    /**< Something weird happened, nothing special but it's good to know. */
    LOG_INFO,       /**< Useful information for users/developers to help debug and figure stuff out. */
    LOG_DEBUG,      /**< Purely test stuff to help developers implement, not really relevant to users. */
    LOG_VERBOSE     /**< Extra details that even developers won't really need 90% of the time. */
};

/**
 * A basic logging and debugging class, prints output to stdout/files.
 * @note Wasn't really satisfied with any of the libraries around
 * so I rolled my own. Based on http://www.drdobbs.com/cpp/logging-in-c/201804215
 */
internal class Logger
{
    internal static SeverityLevel reportingLevel { get; set; } = SeverityLevel.LOG_DEBUG;
    protected static string os;
    static string[] buffer = { "FATAL", "ERROR", "WARN", "INFO", "DEBUG", "VERB" };

    Logger() { }

    ~Logger()
    {
        os = $"{os}{Environment.NewLine}";
        string ss;
        ss = $"[{CrossPlatform.now()}]\t{os}";
        using var file = new StreamWriter(logFile(), true);
        if (file != null)
        {
            file.WriteLine(ss);
            file.Flush();
            file.Close();
        }
        if (file == null || reportingLevel == SeverityLevel.LOG_DEBUG || reportingLevel == SeverityLevel.LOG_VERBOSE)
        {
            Console.Error.WriteLine(os);
            Console.Error.Flush();
        }
    }

    static string toString(SeverityLevel level) =>
        buffer[(int)level];

    static string get(SeverityLevel level = SeverityLevel.LOG_INFO) =>
        $"{os}[{toString(level)}]\t";

    internal static string Log(SeverityLevel level)
    {
        if (level > reportingLevel) return string.Empty;
        else return get(level);
    }

    internal static string _logFile = "openxcom.log";
    internal static string logFile() =>
        _logFile;
}
