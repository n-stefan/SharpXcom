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
 * A string that is already translated.
 * Using this class allows argument substitution in the translated strings.
 */
internal class LocalizedText
{
    string _text; ///< The actual localized text.
	uint _nextArg; ///< The next argument ID.

    /**
	 * Create a LocalizedText from a localized string.
	 */
    internal LocalizedText(string text)
	{
		_text = text;
		_nextArg = 0;
	}

	/**
	 * Create a LocalizedText with some arguments already replaced.
	 */
	LocalizedText(string text, uint replaced)
	{
		_text = text;
		_nextArg = replaced + 1;
	}

    /**
	 * Typecast to constant string reference.
	 * This is used to avoid copying when the string will not change.
	 */
    public static implicit operator string(LocalizedText localizedText) =>
		localizedText._text;

    public static implicit operator LocalizedText(string text) =>
        new(text);

	/**
	 * Replace the next argument placeholder with @a val.
	 * @tparam T The type of the replacement value. It should be streamable to std::owstringstream.
	 * @param val The value to place in the next placeholder's position.
	 * @return The translated string with all occurrences of the marker replaced by @a val.
	 */
	internal LocalizedText arg<T>(T val)
	{
		_text = _text.Replace($"{{{_nextArg}}}", val.ToString());
		++_nextArg;
        return this;
	}
}
