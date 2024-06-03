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
 * Maps canonical names to file paths and maintains the virtual file system
 * for resource files.
 */
internal class FileMap
{
    static Dictionary<string, string> _resources;
    static Dictionary<string, HashSet<string>> _vdirs;
    static HashSet<string> _emptySet;
    static List<KeyValuePair<string, List<string>>> _rulesets;

    static string _canonicalize(string @in) =>
		@in.ToLower();

    internal static string getFilePath(string relativeFilePath)
	{
		string canonicalRelativeFilePath = _canonicalize(relativeFilePath);
		if (!_resources.ContainsKey(canonicalRelativeFilePath))
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} requested file not found: {relativeFilePath}");
			return relativeFilePath;
		}

		return _resources[canonicalRelativeFilePath];
	}

    internal static HashSet<string> getVFolderContents(string relativePath)
	{
		string canonicalRelativePath = _canonicalize(relativePath);

		// trim of trailing '/' characters
		if (!string.IsNullOrEmpty(canonicalRelativePath) && canonicalRelativePath.EndsWith('/'))
		{
			canonicalRelativePath = canonicalRelativePath.TrimEnd('/');
		}

		if (!_vdirs.ContainsKey(canonicalRelativePath))
		{
			return _emptySet;
		}

		return _vdirs[canonicalRelativePath];
	}

    internal static bool isResourcesEmpty() =>
        !_resources.Any();

    internal static void clear()
    {
        _rulesets.Clear();
        _resources.Clear();
        _vdirs.Clear();
    }

	internal static void load(string modId, string path, bool ignoreMods)
	{
        Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   mapping resources in: {path}");
		_mapFiles(modId, path, string.Empty, ignoreMods);
	}

    internal static List<KeyValuePair<string, List<string>>> getRulesets() =>
        _rulesets;

	static void _mapFiles(string modId, string basePath, string relPath, bool ignoreMods)
	{
		string fullDir = basePath + (relPath.Length != 0 ? "/" + relPath : string.Empty);
		List<string> files = CrossPlatform.getFolderContents(fullDir);
		HashSet<string> rulesetFiles = _filterFiles(files, "rul");

		if (!ignoreMods && rulesetFiles.Any())
		{
            _rulesets.Insert(0, KeyValuePair.Create(modId, new List<string>()));
			foreach (var rulesetFile in rulesetFiles)
			{
				string fullpath = fullDir + "/" + rulesetFile;
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   recording ruleset: {fullpath}");
				_rulesets.First().Value.Add(fullpath);
			}
		}

		foreach (var file in files)
		{
			string fullpath = fullDir + "/" + file;

			if (CrossPlatform.folderExists(fullpath))
			{
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   recursing into: {fullpath}");
				_mapFiles(modId, basePath, _combinePath(relPath, file), ignoreMods);
				continue;
			}

			string canonicalFile = _canonicalize(file);
			if (canonicalFile == "metadata.yml" || rulesetFiles.Contains(file))
			{
                // no need to map mod metadata files or ruleset files
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   ignoring non-resource file: {fullpath}");
				continue;
			}

			// populate resource map
			string canonicalRelativeFilePath = _canonicalize(_combinePath(relPath, file));
			if (_resources.TryAdd(canonicalRelativeFilePath, fullpath))
			{
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   mapped resource: {canonicalRelativeFilePath} -> {fullpath}");
			}
			else
			{
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   resource already mapped by higher-priority mod; ignoring: {fullpath}");
			}

			// populate vdir map
			string canonicalRelativePath = _canonicalize(relPath);
			if (!_vdirs.ContainsKey(canonicalRelativePath))
			{
				_vdirs.Add(canonicalRelativePath, new HashSet<string>());
			}
			if (_vdirs[canonicalRelativePath].Add(canonicalFile))
			{
                Console.WriteLine($"{Log(SeverityLevel.LOG_VERBOSE)}   mapped file to virtual directory: {canonicalRelativePath} -> {canonicalFile}");
			}
		}
	}

	static string _combinePath(string prefixPath, string appendPath)
	{
		var ret = string.Empty;
		if (!string.IsNullOrEmpty(prefixPath))
		{
			ret += prefixPath + "/";
		}
		ret += appendPath;
		return ret;
	}

	internal static HashSet<string> filterFiles(List<string> files, string ext) =>
		_filterFiles(files, ext);

	internal static HashSet<string> filterFiles(HashSet<string> files, string ext) =>
		_filterFiles(files, ext);

	static HashSet<string> _filterFiles<T>(T files, string ext) where T : IEnumerable<string>
	{
		var ret = new HashSet<string>();
        int extLen = ext.Length + 1; // +1 for the '.'
		string canonicalExt = _canonicalize(ext);
		foreach (var file in files)
		{
			// less-than not less-than-or-equal since we should have at least
			// one character in the filename that is not part of the extension
			if (extLen < file.Length && 0 == _canonicalize(file.Substring(file.Length - (extLen - 1))).CompareTo(canonicalExt))
			{
				ret.Add(file);
			}
		}
		return ret;
	}
}
