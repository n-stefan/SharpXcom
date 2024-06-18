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

enum TextDirection { DIRECTION_LTR, DIRECTION_RTL };

enum TextWrapping { WRAP_AUTO, WRAP_WORDS, WRAP_LETTERS };

/**
 * Contains strings used throughout the game for localization.
 * Languages are just a set of strings identified by an ID string.
 */
internal class Language
{
    LanguagePlurality _handler;
    TextDirection _direction;
    TextWrapping _wrap;
    static Dictionary<string, string> _names;
    static List<string> _rtl, _cjk;
    Dictionary<string, LocalizedText> _strings;

    /**
     * Initializes an empty language file.
     */
    internal Language()
    {
        _handler = null;
        _direction = TextDirection.DIRECTION_LTR;
        _wrap = TextWrapping.WRAP_WORDS;

        // maps don't have initializers :(
        if (!_names.Any())
        {
            _names["en-US"] = "English (US)";
            _names["en-GB"] = "English (UK)";
            // _names["ar"] = "العربية"; needs fonts
            _names["bg"] = "Български";
            _names["ca-ES"] = "Català";
            _names["cs"] = "Česky";
            _names["cy"] = "Cymraeg";
            _names["da"] = "Dansk";
            _names["de"] = "Deutsch";
            _names["el"] = "Ελληνικά";
            _names["et"] = "Eesti";
            _names["es-ES"] = "Español (ES)";
            _names["es-419"] = "Español (AL)";
            _names["fr"] = "Français (FR)";
            _names["fr-CA"] = "Français (CA)";
            _names["fi"] = "Suomi";
            _names["ga"] = "Gaeilge";
            _names["hr"] = "Hrvatski";
            _names["hu"] = "Magyar";
            _names["it"] = "Italiano";
            _names["is"] = "Íslenska";
            _names["ja"] = "日本語";
            _names["ko"] = "한국어";
            _names["lb"] = "Lëtzebuergesch";
            _names["lv"] = "Latviešu";
            _names["nl"] = "Nederlands";
            _names["no"] = "Norsk";
            _names["pl"] = "Polski";
            _names["pt-BR"] = "Português (BR)";
            _names["pt-PT"] = "Português (PT)";
            _names["ro"] = "Română";
            _names["ru"] = "Русский";
            _names["sk"] = "Slovenčina";
            _names["sl"] = "Slovenščina";
            _names["sv"] = "Svenska";
            // _names["th"] = "ไทย"; needs fonts
            _names["tr"] = "Türkçe";
            _names["uk"] = "Українська";
            _names["vi"] = "Tiếng Việt";
            _names["zh-CN"] = "中文";
            _names["zh-TW"] = "文言";
        }
        if (!_rtl.Any())
        {
            //_rtl.push_back("he"); needs translation
        }
        if (!_cjk.Any())
        {
            _cjk.Add("ja");
            _cjk.Add("ko");
            _cjk.Add("zh-CN");
            _cjk.Add("zh-TW");
        }

        string id = Options.language;
        _handler = LanguagePlurality.create(id);
        if (!_rtl.Contains(id))
        {
            _direction = TextDirection.DIRECTION_LTR;
        }
        else
        {
            _direction = TextDirection.DIRECTION_RTL;
        }
        if (Options.wordwrap == TextWrapping.WRAP_AUTO)
        {
            if (!_cjk.Contains(id))
            {
                _wrap = TextWrapping.WRAP_WORDS;
            }
            else
            {
                _wrap = TextWrapping.WRAP_LETTERS;
            }
        }
        else
        {
            _wrap = Options.wordwrap;
        }
    }

    /**
     *
     */
    ~Language()
    {
        _handler = null;
    }

    /**
     * Returns the wrapping rules to use for rendering
     * text in this language.
     * @return Text wrapping.
     */
    internal TextWrapping getTextWrapping() =>
	    _wrap;

    /**
     * Returns the localized text with the specified ID.
     * If it's not found, just returns the ID.
     * @param id ID of the string.
     * @return String with the requested ID.
     */
    internal LocalizedText getString(string id)
    {
	    if (string.IsNullOrEmpty(id))
	    {
		    return id;
	    }
	    // Check if translation strings recently learned pluralization.
	    if (_strings.ContainsKey(id))
	    {
		    return getString(id, uint.MaxValue);
	    }
	    else
	    {
		    return _strings[id];
	    }
    }

	static HashSet<string> notFoundIds;
    /**
     * Returns the localized text with the specified ID, in the proper form for @a n.
     * The substitution of @a n has already happened in the returned LocalizedText.
     * If it's not found, just returns the ID.
     * @param id ID of the string.
     * @param n Number to use to decide the proper form.
     * @return String with the requested ID.
     */
    internal LocalizedText getString(string id, uint n)
    {
	    Debug.Assert(!string.IsNullOrEmpty(id));
        string id1 = $"{id}_zero", id2 = $"{id}{_handler.getSuffix(n)}", id3 = $"{id}_other";
        if (n == 0)
	    {
		    if (!_strings.ContainsKey(id1) && // Try specialized form
                !_strings.ContainsKey(id2) && // Try proper form by language
                !_strings.ContainsKey(id3)) // Try default form
            {
                // Give up
                if (!notFoundIds.Contains(id))
                {
                    notFoundIds.Add(id);
                    Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {id} not found in {Options.language}");
                }
                return id;
            }
        }
	    
	    if (n == uint.MaxValue) // Special case
	    {
		    if (!notFoundIds.Contains(id))
		    {
			    notFoundIds.Add(id);
                Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} {id} has plural format in ``{Options.language}``. Code assumes singular format.");
                //Hint: Change ``getstring(ID).arg(value)`` to ``getString(ID, value)`` in appropriate files.
		    }
            return _strings[id1] ?? _strings[id2] ?? _strings[id3];
	    }
	    else
	    {
            string marker = "{N}", val = n.ToString(), txt = _strings[id1] ?? _strings[id2] ?? _strings[id3];
            return Unicode.replace(txt, marker, val);
	    }
    }

    /**
     * Returns the localized text with the specified ID, in the proper form for the gender.
     * If it's not found, just returns the ID.
     * @param id ID of the string.
     * @param gender Current soldier gender.
     * @return String with the requested ID.
     */
    LocalizedText getString(string id, SoldierGender gender)
    {
	    string genderId;
	    if (gender == SoldierGender.GENDER_MALE)
	    {
		    genderId = id + "_MALE";
	    }
	    else
	    {
		    genderId = id + "_FEMALE";
	    }
	    return getString(genderId);
    }

    /// Checks if a language is in the supported name list.
    internal static bool isSupported(string lang) =>
        _names.ContainsKey(lang);

    /**
     * Loads a language file from an external path.
     * @param path Language file path.
     */
    internal void loadFile(string path)
    {
	    try
	    {
		    if (CrossPlatform.fileExists(path))
		    {
                load(path);
		    }
	    }
	    catch (YamlException e)
	    {
		    throw new Exception(path + ": " + e.Message);
	    }
    }

    /**
     * Loads a language file from a mod's ExtraStrings.
     * @param extraStrings List of ExtraStrings.
     * @param id Language ID.
     */
    internal void loadRule(Dictionary<string, ExtraStrings> extraStrings, string id)
    {
	    if (extraStrings.TryGetValue(id, out ExtraStrings extras))
	    {
		    foreach (var strings in extras.getStrings())
		    {
			    _strings[strings.Key] = loadString(strings.Value);
		    }
	    }
    }

    /**
     * Replaces all special string markers with the appropriate characters.
     * @param string Original string.
     * @return New converted string.
     */
    string loadString(string s)
    {
	    s = Unicode.replace(s, "{NEWLINE}", "\n");
        s = Unicode.replace(s, "{SMALLLINE}", "\x02"); // Unicode::TOK_NL_SMALL
        s = Unicode.replace(s, "{ALT}", "\x01"); // Unicode::TOK_COLOR_FLIP
	    return s;
    }

    /**
     * Loads a language file in Ruby-on-Rails YAML format.
     * Not that this has anything to do with Ruby, but since it's a
     * widely-supported format and we already have YAML, it was convenient.
     * @param filename Filename of the YAML file.
     */
    void load(string filename)
    {
        using var input = new StreamReader(filename);
        var yaml = new YamlStream();
        yaml.Load(input);
	    YamlMappingNode lang;
	    if (((YamlMappingNode)yaml.Documents[0].RootNode).Children[0].Value is YamlMappingNode m)
	    {
		    lang = m;
	    }
	    // Fallback when file is missing language specifier
	    else
	    {
		    lang = (YamlMappingNode)yaml.Documents[0].RootNode;
	    }
	    foreach (var i in lang.Children)
	    {
		    // Regular strings
		    if (i.Value is YamlScalarNode)
		    {
                string value = i.Value.ToString();
			    if (!string.IsNullOrEmpty(value))
			    {
				    string key = i.Key.ToString();
				    _strings[key] = loadString(value);
			    }
		    }
		    // Strings with plurality
		    else if (i.Value is YamlMappingNode n)
		    {
			    foreach (var j in n.Children)
			    {
                    string value = j.Value.ToString();
				    if (!string.IsNullOrEmpty(value))
				    {
					    string key = i.Key.ToString() + "_" + j.Key.ToString();
                        _strings[key] = loadString(value);
				    }
			    }
		    }
	    }
    }

    /**
     * Gets all the languages found in the
     * Data folder and returns their properties.
     * @param files List of language filenames.
     * @param names List of language human-readable names.
     */
    internal static void getList(List<string> files, List<string> names)
    {
        files = CrossPlatform.getFolderContents(CrossPlatform.searchDataFolder("common/Language"), "yml");
        names.Clear();

        for (int i = 0; i < files.Count; i++)
        {
            files[i] = CrossPlatform.noExt(files[i]);
            string name;
            if (_names.TryGetValue(files[i], out string lang))
            {
                name = lang;
            }
            else
            {
                name = files[i];
            }
            names.Add(name);
        }
    }

    /**
     * Returns the direction to use for rendering
     * text in this language.
     * @return Text direction.
     */
    internal TextDirection getTextDirection() =>
	    _direction;

    /**
     * Outputs all the language IDs and strings
     * to an HTML table.
     * @param filename HTML file.
     */
    void toHtml(string filename)
    {
        try
        {
            using var htmlFile = new StreamWriter(filename);
	        htmlFile.WriteLine("<table border=\"1\" width=\"100%\">");
	        htmlFile.WriteLine("<tr><th>ID String</th><th>English String</th></tr>");
	        foreach (var i in _strings)
	        {
		        htmlFile.WriteLine($"<tr><td>{i.Key}</td><td>");
		        string s = i.Value;
		        foreach (var j in s)
		        {
			        if (j == Unicode.TOK_NL_SMALL || j == '\n')
			        {
				        htmlFile.Write("<br />");
			        }
			        else
			        {
				        htmlFile.Write(j);
			        }
		        }
		        htmlFile.WriteLine("</td></tr>");
	        }
	        htmlFile.WriteLine("</table>");
	        htmlFile.Close();
	    }
	    catch (Exception e)
	    {
            Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} {e.Message}");
	    }
    }
}
