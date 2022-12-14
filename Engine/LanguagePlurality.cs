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
 *e
 * You should have received a copy of the GNU General Public License
 * along with OpenXcom.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace SharpXcom.Engine;

delegate LanguagePlurality PFCreate();

/**
 * This class is the interface used to find plural forms for the different languages.
 * Derived classes implement getSuffix() according to the specific language's rules.
 */
internal abstract class LanguagePlurality
{
	static Dictionary<string, PFCreate> s_factoryFunctions;

    protected LanguagePlurality() { /* Empty by design. */ }

    /// Allow proper destruction through base pointer.
    ~LanguagePlurality() { /* Empty by design. */ }

	/**
	 * Search and create a handler for the plurality rules of @a language.
	 * If the language was not found, a default with the same rules as English is returned.
	 * @param language The target language.
	 * @return A newly created LanguagePlurality instance for the given language.
	 * @internal The first time this is called, we populate the language => rules mapping.
	 */
	internal static LanguagePlurality create(string language)
	{
		// Populate factory the first time we are called.
		if (!s_factoryFunctions.Any())
		{
            s_factoryFunctions.Add("fr", ZeroOneSingular.create);
			s_factoryFunctions.Add("fr-CA", ZeroOneSingular.create);
			s_factoryFunctions.Add("hu", NoSingular.create);
			s_factoryFunctions.Add("tr", NoSingular.create);
			s_factoryFunctions.Add("cs", CzechPlurality.create);
			s_factoryFunctions.Add("pl", PolishPlurality.create);
			s_factoryFunctions.Add("ro", RomanianPlurality.create);
			s_factoryFunctions.Add("ru", CyrillicPlurality.create);
			s_factoryFunctions.Add("sk", CzechPlurality.create);
			s_factoryFunctions.Add("uk", CyrillicPlurality.create);
			s_factoryFunctions.Add("ja", NoSingular.create);
			s_factoryFunctions.Add("ko", NoSingular.create);
			s_factoryFunctions.Add("zh-CN", NoSingular.create);
			s_factoryFunctions.Add("zh-TW", NoSingular.create);
			s_factoryFunctions.Add("hr", CroatianPlurality.create);
		}
		PFCreate creator = OneSingular.create;
		if (s_factoryFunctions.ContainsKey(language))
		{
			creator = s_factoryFunctions[language];
		}
		return creator();
	}

	internal abstract string getSuffix(uint n);
}

/**
 * Plurality rules where 0 is also singular.
 * Provide rules for languages where 0 and 1 are singular and everything else is plural.
 * @note one = 0-1; other = ...
 */
class ZeroOneSingular : LanguagePlurality
{
	internal override string getSuffix(uint n)
	{
		if (n == 0 || n == 1)
		{
			return "_one";
		}
		return "_other";
	}
	
	internal static LanguagePlurality create() =>
		new ZeroOneSingular();
}

/**
 * Plurality rules where there is no singular.
 * Provide rules for languages where everything is plural.
 * @note other = ...
 */
class NoSingular : LanguagePlurality
{
	internal override string getSuffix(uint _) =>
		"_other";

	internal static LanguagePlurality create() =>
		new NoSingular();
}

/**
 * Plurality rules for Czech and Slovak languages.
 * @note one = 1; few = 2-4; other = ...
 */
class CzechPlurality : LanguagePlurality
{
	internal override string getSuffix(uint n)
	{
		if (n == 1)
		{
			return "_one";
		}
		else if (n >= 2 && n <= 4)
		{
			return "_few";
		}
		return "_other";
	}

	internal static LanguagePlurality create() =>
		new CzechPlurality();
}

/**
 * Plurality rules for Cyrillic languages (Russian, Ukrainian, etc.)
 * @note one = 1, 21, 31...; few = 2-4, 22-24, 32-34...; many = 0, 5-20, 25-30, 35-40...; other = ...
 */
class CyrillicPlurality : LanguagePlurality
{
	internal override string getSuffix(uint n)
	{
		if (n % 10 == 1 && n % 100 != 11)
		{
			return "_one";
		}
		else if ((n % 10 >= 2 && n % 10 <= 4) &&
				!(n % 100 >= 12 && n % 100 <= 14))
		{
			return "_few";
		}
		else if (n % 10 == 0 ||
				(n % 10 >= 5 && n % 10 <= 9) ||
				(n % 100 >= 11 && n % 100 <= 14))
		{
			return "_many";
		}
		return "_other";
	}

	internal static LanguagePlurality create() =>
		new CyrillicPlurality();
}

/**
 * Plurality rules for the Polish language.
 * @note one = 1; few = 2-4, 22-24, 32-34...; many = 0, 5-21, 25-31, 35-41, ...; other = ...
 */
class PolishPlurality : LanguagePlurality
{
	internal override string getSuffix(uint n)
	{
		if (n == 1)
		{
			return "_one";
		}
		else if ((n % 10 >= 2 && n % 10 <= 4) &&
				!(n % 100 >= 12 && n % 100 <= 14))
		{
			return "_few";
		}
		else if ((n % 10 <= 1) ||
				(n % 10 >= 5 && n % 10 <= 9) ||
				(n % 100 >= 12 && n % 100 <= 14))
		{
			return "_many";
		}
		return "_other";
	}

	internal static LanguagePlurality create() =>
		new PolishPlurality();
}

/**
 * Plurality rules for Romanian and Moldavian languages.
 * @note one = 1; few = 0, 2-19, 101-119...; other = ...
 */
class RomanianPlurality : LanguagePlurality
{
	internal override string getSuffix(uint n)
	{
		if (n == 1)
		{
			return "_one";
		}
		else if (n == 0 ||
				(n % 100 >= 1 && n % 100 <= 19))
		{
			return "_few";
		}
		return "_other";
	}

	internal static LanguagePlurality create() =>
		new RomanianPlurality();
}

/**
 * Plurality rules for Croatian and Serbian languages.
 * @note one = 1, 21, 31...; few = 2-4, 22-24, 32-34, ...; other = ...
 */
class CroatianPlurality : LanguagePlurality
{
	internal override string getSuffix(uint n)
	{
		if (n % 10 == 1 && n % 100 != 11)
		{
			return "_one";
		}
		else if ((n % 10 >= 2 && n % 10 <= 4) &&
				!(n % 100 >= 12 && n % 100 <= 14))
		{
			return "_few";
		}
		return "_other";
	}

	internal static LanguagePlurality create() =>
		new CroatianPlurality();
}

/**
 * Default plurality rules.
 * Provide rules for languages where 1 is singular and everything else is plural.
 * @note one = 1; other = ...
 */
class OneSingular : LanguagePlurality
{
	internal override string getSuffix(uint n)
	{
		if (n == 1)
		{
			return "_one";
		}
		return "_other";
	}

	internal static LanguagePlurality create() =>
		new OneSingular();
}
