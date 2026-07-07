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

enum OptionType { OPTION_BOOL, OPTION_INT, OPTION_FLOAT, OPTION_STRING, OPTION_KEY };

struct OptionDef
{
    internal bool b;
    internal int i;
    internal float f;
    internal string s;
    internal SDL_Keycode k;
}

unsafe struct OptionRef
{
    internal bool* b;
    internal int* i;
    internal float* f;
    internal string* s;
    internal SDL_Keycode* k;
}

/**
 * Helper class that ties metadata to particular options to help in serializing
 * and stuff. The option variable must already exist, this info just points to it.
 * Does some special shenanigans to be able to be tied to different variable types.
 */
internal class OptionInfo
{
    string _id, _desc, _cat;
    OptionType _type;
    OptionDef _def;
    OptionRef _ref;

    /**
     * Creates info for a boolean option.
     * @param id String ID used in serializing.
     * @param option Pointer to the option.
     * @param def Default option value.
     * @param desc Language ID for the option description (if any).
     * @param cat Language ID for the option category (if any).
     */
    unsafe internal OptionInfo(string id, ref bool option, bool def, string desc = "", string cat = "")
    {
        _id = id;
        _desc = desc;
        _cat = cat;
        _type = OptionType.OPTION_BOOL;

        fixed (bool* p = &option) { _ref.b = p; }
        _def.b = def;
    }

    /**
     * Creates info for an integer option.
     * @param id String ID used in serializing.
     * @param option Pointer to the option.
     * @param def Default option value.
     * @param desc Language ID for the option description (if any).
     * @param cat Language ID for the option category (if any).
     */
    unsafe internal OptionInfo(string id, ref int option, int def, string desc = "", string cat = "")
    {
        _id = id;
        _desc = desc;
        _cat = cat;
        _type = OptionType.OPTION_INT;

        fixed (int* p = &option) { _ref.i = p; }
        _def.i = def;
    }

    /**
     * Creates info for a float option.
     * @param id String ID used in serializing.
     * @param option Pointer to the option.
     * @param def Default option value.
     * @param desc Language ID for the option description (if any).
     * @param cat Language ID for the option category (if any).
     */
    unsafe internal OptionInfo(string id, ref float option, float def, string desc = "", string cat = "")
    {
        _id = id;
        _desc = desc;
        _cat = cat;
        _type = OptionType.OPTION_FLOAT;

        fixed (float* p = &option) { _ref.f = p; }
        _def.f = def;
    }

    /**
     * Creates info for a keyboard shortcut option.
     * @param id String ID used in serializing.
     * @param option Pointer to the option.
     * @param def Default option value.
     * @param desc Language ID for the option description (if any).
     * @param cat Language ID for the option category (if any).
     */
    unsafe internal OptionInfo(string id, ref SDL_Keycode option, SDL_Keycode def, string desc = "", string cat = "")
    {
        _id = id;
        _desc = desc;
        _cat = cat;
        _type = OptionType.OPTION_KEY;

        fixed (SDL_Keycode* p = &option) { _ref.k = p; }
        _def.k = def;
    }

    /**
     * Creates info for a string option.
     * @param id String ID used in serializing.
     * @param option Pointer to the option.
     * @param def Default option value.
     * @param desc Language ID for the option description (if any).
     * @param cat Language ID for the option category (if any).
     */
    unsafe internal OptionInfo(string id, ref string option, string def, string desc = "", string cat = "")
    {
        _id = id;
        _desc = desc;
        _cat = cat;
        _type = OptionType.OPTION_STRING;

        fixed (string* p = &option) { _ref.s = p; }
        _def.s = def;
    }

    /**
     * Resets an option back to its default value.
     */
    unsafe internal void reset()
    {
        switch (_type)
        {
            case OptionType.OPTION_BOOL:
                *_ref.b = _def.b;
                break;
            case OptionType.OPTION_INT:
                *_ref.i = _def.i;
                break;
            case OptionType.OPTION_FLOAT:
                *_ref.f = _def.f;
                break;
            case OptionType.OPTION_KEY:
                *_ref.k = _def.k;
                break;
            case OptionType.OPTION_STRING:
                *_ref.s = _def.s;
                break;
        }
    }

    /**
     * Loads an option value from the corresponding map
     * (eg. for command-line options).
     * @param map Options map.
     */
    unsafe internal void load(Dictionary<string, string> map)
    {
        string id = _id.ToLower();
        if (map.TryGetValue(id, out var value))
        {
            switch (_type)
            {
                case OptionType.OPTION_BOOL:
                    *_ref.b = bool.Parse(value);
                    break;
                case OptionType.OPTION_INT:
                    *_ref.i = int.Parse(value);
                    break;
                case OptionType.OPTION_FLOAT:
                    *_ref.f = float.Parse(value);
                    break;
                case OptionType.OPTION_KEY:
                    *_ref.k = Enum.Parse<SDL_Keycode>(value);
                    break;
                case OptionType.OPTION_STRING:
                    *_ref.s = value;
                    break;
            }
        }
    }

    /**
     * Loads an option value from the corresponding YAML.
     * @param node Options YAML node.
     */
    unsafe internal void load(YamlNode node)
    {
        switch (_type)
        {
            case OptionType.OPTION_BOOL:
                *_ref.b = node[_id] != null ? bool.Parse(node[_id].ToString()) : _def.b;
                break;
            case OptionType.OPTION_INT:
                *_ref.i = node[_id] != null ? int.Parse(node[_id].ToString()) : _def.i;
                break;
            case OptionType.OPTION_FLOAT:
                *_ref.f = node[_id] != null ? float.Parse(node[_id].ToString()) : _def.f;
                break;
            case OptionType.OPTION_KEY:
                *_ref.k = node[_id] != null ? (SDL_Keycode)int.Parse(node[_id].ToString()) : _def.k;
                break;
            case OptionType.OPTION_STRING:
                *_ref.s = node[_id] != null ? node[_id].ToString() : _def.s;
                break;
        }
    }

    /**
     * Returns the variable type of the option.
     * @return Option type.
     */
    internal OptionType type() =>
        _type;

    /**
     * Returns the description of the option. Options with
     * descriptions show up in the Options screens.
     * @return Language string ID for the description.
     */
    internal string description() =>
        _desc;

    /**
     * Returns the category of the option. Options with
     * categories show up in the Options screens.
     * @return Language string ID for the category.
     */
    internal string category() =>
        _cat;

    /**
     * Returns the pointer to the key option,
     * or throws an exception if it's not a key.
     * @return Pointer to the option.
     */
    unsafe internal ref SDL_Keycode asKey()
    {
        if (_type != OptionType.OPTION_KEY)
        {
            throw new Exception(_id + " is not a key!");
        }
        return ref *_ref.k;
    }

    /**
     * Returns the pointer to the boolean option,
     * or throws an exception if it's not a boolean.
     * @return Pointer to the option.
     */
    unsafe internal ref bool asBool()
    {
        if (_type != OptionType.OPTION_BOOL)
        {
            throw new Exception(_id + " is not a boolean!");
        }
        return ref *_ref.b;
    }

    /**
     * Returns the pointer to the integer option,
     * or throws an exception if it's not a integer.
     * @return Pointer to the option.
     */
    unsafe internal ref int asInt()
    {
        if (_type != OptionType.OPTION_INT)
        {
            throw new Exception(_id + " is not an integer!");
        }
        return ref *_ref.i;
    }

    /**
     * Returns the pointer to the float option,
     * or throws an exception if it's not a float.
     * @return Pointer to the option.
     */
    unsafe internal ref float asFloat()
    {
        if (_type != OptionType.OPTION_FLOAT)
        {
            throw new Exception(_id + " is not a float!");
        }
        return ref *_ref.f;
    }

    /**
     * Returns the pointer to the string option,
     * or throws an exception if it's not a string.
     * @return Pointer to the option.
     */
    unsafe ref string asString()
    {
        if (_type != OptionType.OPTION_STRING)
        {
            throw new Exception(_id + " is not a string!");
        }
        return ref *_ref.s;
    }

    /**
     * Saves an option value to the corresponding YAML.
     * @param node Options YAML node.
     */
    unsafe internal void save(YamlNode node)
    {
        switch (_type)
        {
            case OptionType.OPTION_BOOL:
                ((YamlMappingNode)node).Add(_id, (*_ref.b).ToString());
                break;
            case OptionType.OPTION_INT:
                ((YamlMappingNode)node).Add(_id, (*_ref.i).ToString());
                break;
            case OptionType.OPTION_FLOAT:
                ((YamlMappingNode)node).Add(_id, (*_ref.f).ToString());
                break;
            case OptionType.OPTION_KEY:
                ((YamlMappingNode)node).Add(_id, (*_ref.k).ToString());
                break;
            case OptionType.OPTION_STRING:
                ((YamlMappingNode)node).Add(_id, *_ref.s);
                break;
        }
    }
}
