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

struct EngineData
{
    internal string name;
    internal int[] version;
}

/**
 * Represents mod metadata
 */
internal class ModInfo
{
    string _path;
    string _name, _desc, _version, _author, _url, _id, _master;
    bool _isMaster;
    int _reservedSpace;
    bool _engineOk;
    List<string> _externalResourceDirs;
    string _requiredExtendedVersion;
    string _requiredExtendedEngine;
    string _resourceConfigFile;

    /**
     * List of engines that current version support.
     */
    static EngineData[] supportedEngines = {
	    new() { name = OPENXCOM_VERSION_ENGINE, version = OPENXCOM_VERSION_NUMBER },
        new() { name = string.Empty, version = new[] { 0, 0, 0, 0 } } // assume that every engine support mods from base game, remove if its not true.
    };

    internal ModInfo(string path)
    {
        _path = path;
        _name = CrossPlatform.baseFilename(path);
        _desc = "No description.";
        _version = "1.0";
        _author = "unknown author";
        _id = _name;
        _master = "xcom1";
        _isMaster = false;
        _reservedSpace = 1;
        _engineOk = false;
    }

    /**
     * Checks if a given mod can be activated.
     * It must either be:
     * - a Master mod
     * - a standalone mod (no master)
     * - depend on the current Master mod
     * @param curMaster Id of the active master mod.
     * @return True if it's activable, false otherwise.
    */
    internal bool canActivate(string curMaster) =>
	    (isMaster() || string.IsNullOrEmpty(getMaster()) || getMaster() == curMaster);

    internal bool isMaster() =>
        _isMaster;

    internal string getMaster() =>
        _master;

    internal string getId() =>
        _id;

    internal string getVersion() =>
        _version;

    internal string getPath() =>
        _path;

    internal List<string> getExternalResourceDirs() =>
        _externalResourceDirs;

    internal string getName() =>
        _name;

    internal string getDescription() =>
        _desc;

    internal string getAuthor() =>
        _author;

    internal void load(string filename)
    {
        using var input = new StreamReader(filename);
        var yaml = new YamlStream();
        yaml.Load(input);
        var doc = (YamlMappingNode)yaml.Documents[0].RootNode;

	    _name     = doc.Children["name"].ToString();
	    _desc     = doc.Children["description"].ToString();
	    _version  = doc.Children["version"].ToString();
	    _author   = doc.Children["author"].ToString();
	    _id       = doc.Children["id"].ToString();
	    _isMaster = bool.Parse(doc.Children["isMaster"].ToString());
	    _reservedSpace = int.Parse(doc.Children["reservedSpace"].ToString());
	    if (doc.Children["requiredExtendedVersion"] != null)
	    {
		    _requiredExtendedVersion = doc.Children["requiredExtendedVersion"].ToString();
		    _requiredExtendedEngine = "Extended"; //for backward compatibility
	    }
	    _requiredExtendedEngine = doc.Children["requiredExtendedEngine"].ToString();

	    _engineOk = findCompatibleEngine(supportedEngines, _requiredExtendedEngine);

	    if (_reservedSpace < 1)
	    {
		    _reservedSpace = 1;
	    }
	    else if (_reservedSpace > 100)
	    {
		    _reservedSpace = 100;
	    }

	    if (_isMaster)
	    {
		    // default a master's master to none.  masters can still have
		    // masters, but they must be explicitly declared.
		    _master = string.Empty;
		    // only masters can load external resource dirs
		    _externalResourceDirs = ((YamlSequenceNode)doc.Children["loadResources"]).Select(x => ((YamlScalarNode)x).Value).ToList();
		    // or basic resource definition
		    _resourceConfigFile = doc.Children["resourceConfig"].ToString();
	    }

	    _master = doc.Children["master"].ToString();
	    if (_master == "*")
	    {
		    _master = string.Empty;
	    }
    }

    bool findCompatibleEngine(EngineData[] l, string v)
    {
	    for (int i = 0; i < l.Length; ++i)
	    {
		    if (l[i].name == v)
		    {
			    //TODO: add check for version
			    return true;
		    }
	    }
	    return false;
    }

    internal int getReservedSpace() =>
        _reservedSpace;

    internal string getResourceConfigFile() =>
        _resourceConfigFile;

    internal bool isEngineOk() =>
        _engineOk;

    internal string getRequiredExtendedEngine() =>
        _requiredExtendedEngine;
}
