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
 * Functions for dealing with encoding, strings, text
 * and all related operations.
 */
internal class Unicode
{
    /* Special text tokens */
    internal const char TOK_NL_SMALL = (char)2;		/// line break and change to small font
	internal const char TOK_COLOR_FLIP = (char)1;	/// alternate between primary and secondary color
	internal const char TOK_NBSP = (char)0xA0;		/// non-breaking space

    static Encoding utf8;

    /**
	 * Store a UTF-8 locale to use when dealing with character conversions.
	 * Windows doesn't have a UTF-8 locale so we just use its APIs directly.
	 */
    internal static void getUtf8Locale()
	{
		// Try a UTF-8 locale (or default if none was found)
		utf8 = Encoding.UTF8;
        Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Detected locale: {utf8.EncodingName}");
    }

	/**
	 * Takes a filesystem path and converts it to a UTF-8 string.
	 * On Windows the C paths use the local ANSI codepage.
	 * Used for SDL APIs.
	 * @param src Filesystem path.
	 * @return UTF-8 string.
	 */
	internal static string convPathToUtf8(string src)
	{
        var bytes = Encoding.UTF8.GetBytes(src);
        return Encoding.UTF8.GetString(bytes);
	}

    /// Checks if a character is a linebreak.
    internal static bool isLinebreak(uint c) =>
		(c == '\n' || c == TOK_NL_SMALL);

    /// Checks if a character is a blank space (includes non-breaking spaces).
    internal static bool isSpace(uint c) =>
		(c == ' ' || c == TOK_NBSP);

    /// Checks if a character is a word separator.
    internal static bool isSeparator(uint c) =>
		(c == '-' || c == '/');

    /// Checks if a character is visible to the user.
    internal static bool isPrintable(uint c) =>
		(c > 32 && c != TOK_NBSP);

	/**
	 * Takes a Unicode 32-bit string and converts it
	 * to a 8-bit string encoded in UTF-8.
	 * Used for rendering text.
	 * @note Adapted from https://stackoverflow.com/a/148766/2683561
	 * @param src UTF-8 string.
	 * @return Unicode string.
	 */
	internal static string convUtf8ToUtf32(string src)
	{
        var bytes = Encoding.UTF8.GetBytes(src);
        return Encoding.UTF32.GetString(bytes);

        //if (string.IsNullOrEmpty(src))
        //	return null;
        //var @out = new List<uint>(src.Length);
        //uint codepoint = 0;
        //for (var i = 0; i < src.Length; i++)
        //{
        //	char ch = src[i];
        //	if (ch <= 0x7f)
        //		codepoint = ch;
        //	else if (ch <= 0xbf)
        //		codepoint = (uint)((codepoint << 6) | (ch & 0x3f));
        //	else if (ch <= 0xdf)
        //		codepoint = (uint)(ch & 0x1f);
        //	else if (ch <= 0xef)
        //		codepoint = (uint)(ch & 0x0f);
        //	else
        //		codepoint = (uint)(ch & 0x07);
        //	++i;
        //	if (i == src.Length - 1 || ((src[i] & 0xc0) != 0x80 && codepoint <= 0x10ffff))
        //	{
        //		@out[i] = codepoint;
        //	}
        //}
        //return @out;
    }

	/**
	 * Takes a Unicode 32-bit string and converts it
	 * to a 8-bit string encoded in UTF-8.
	 * Used for rendering text.
	 * @note Adapted from https://stackoverflow.com/a/148766/2683561
	 * @param src Unicode string.
	 * @return UTF-8 string.
	 */
	internal static string convUtf32ToUtf8(string src)
	{
        var bytes = Encoding.UTF32.GetBytes(src);
        return Encoding.UTF8.GetString(bytes);
	}

    /**
     * Takes a UTF-8 string and converts it to a filesystem path.
     * On Windows the C paths use the local ANSI codepage.
     * Used for SDL APIs.
     * @param src UTF-8 string.
     * @return Filesystem path.
     */
    internal static string convUtf8ToPath(string src)
    {
        var bytes = Encoding.UTF8.GetBytes(src);
        return Encoding.Default.GetString(bytes);
    }

    /**
     * Replaces every instance of a substring.
     * @param str The string to modify.
     * @param find The substring to find.
     * @param replace The substring to replace it with.
     */
    internal static string replace(string str, string find, string replace) =>
        str.Replace(find, replace);
}
