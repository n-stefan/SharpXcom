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
 * Generic purpose functions that need different
 * implementations for different platforms.
 */
internal class CrossPlatform
{
    /**
	 * Determines the available Linux error dialogs.
	 */
    internal static void getErrorDialog() { }

	/**
	 * Sets the window titlebar icon.
	 * For Windows, use the embedded resource icon.
	 * For other systems, use a PNG icon.
	 * @param winResource ID for Windows icon.
	 * @param path Path to PNG icon for Unix.
	 */
	internal static void setWindowIcon(string path)
	{
		string utf8 = Unicode.convPathToUtf8(path);
        nint /* SDL_Surface */ icon = IMG_Load(utf8);
		if (icon != nint.Zero)
		{
            SDL_SetWindowIcon(nint.Zero, icon);
			SDL_FreeSurface(icon);
		}
    }

    /// Gets the pathless filename of a file.
    internal static string baseFilename(string path) =>
		Path.GetFileName(path);

	/**
	 * Compares the extension in a filename (case-insensitive).
	 * @param filename Filename to compare.
	 * @param extension Extension to compare to.
	 * @return If the extensions match.
	 */
	internal static bool compareExt(string filename, string extension)
	{
		if (string.IsNullOrEmpty(extension))
			return true;
		int j = filename.Length - extension.Length;
		if (j <= 0)
			return false;
		if (filename[j - 1] != '.')
			return false;
		for (int i = 0; i < extension.Length; ++i)
		{
			if (char.ToLower(filename[j + i]) != char.ToLower(extension[i]))
				return false;
		}
		return true;
	}

    /**
	 * Gets the executable path in DOS-style (short) form.
	 * For non-Windows systems, just use a dummy path.
	 * @return Executable path.
	 */
    internal static string getDosPath()
    {
        string path = string.Empty, bufstr;
        if (!string.IsNullOrEmpty(Environment.ProcessPath))
        {
            bufstr = Environment.ProcessPath;
            int c1 = bufstr.IndexOf('\\');
            path += bufstr.Substring(0, c1 + 1);
            int c2 = bufstr.IndexOf('\\', c1 + 1);
            while (c2 != -1)
            {
                string dirname = bufstr.Substring(c1 + 1, c2 - c1 - 1);
                if (dirname == "..")
                {
                    path = path.Substring(0, path.LastIndexOf('\\', path.Length - 2));
                }
                else
                {
                    if (dirname.Length > 8)
                        dirname = dirname.Substring(0, 6) + "~1";
                    dirname = dirname.ToUpper();
                    path += dirname;
                }
                c1 = c2;
                c2 = bufstr.IndexOf('\\', c1 + 1);
                if (c2 != -1)
                    path += '\\';
            }
        }
        else
        {
            path = "C:\\GAMES\\OPENXCOM";
        }
        return path;
    }

	/**
	 * Generates a timestamp of the current time.
	 * @return String in D-M-Y_H-M-S format.
	 */
	internal static string now() =>
		DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss", CultureInfo.InvariantCulture);

	/**
	 * Logs the details of this crash and shows an error.
	 * @param ex Pointer to exception data (PEXCEPTION_POINTERS on Windows, signal int on Unix)
	 * @param err Exception message, if any.
	 */
	internal static void crashDump(Exception ex, string err)
	{
		var error = (ex is AccessViolationException) ?
			"Memory access violation. This usually indicates something missing in a mod." :
			ex.Message;
        Console.WriteLine($"{Log(SeverityLevel.LOG_FATAL)} A fatal error has occurred: {error}");
		stackTrace(ex);
		//string dumpName = Options.getUserFolder();
		//dumpName += now() + ".dmp";
		//using var dumpFile = new FileStream(dumpName, FileMode.Create, FileAccess.ReadWrite);
        //MINIDUMP_EXCEPTION_INFORMATION exceptionInformation;
		//exceptionInformation.ThreadId = GetCurrentThreadId();
		//exceptionInformation.ExceptionPointers = exception;
		//exceptionInformation.ClientPointers = FALSE;
		//if (MiniDumpWriteDump(GetCurrentProcess(), GetCurrentProcessId(), dumpFile, MiniDumpNormal, exception ? &exceptionInformation : NULL, NULL, NULL))
		//{
		//	Log(LOG_FATAL) << "Crash dump generated at " << dumpName;
		//}
		//else
		//{
			Console.WriteLine($"{Log(SeverityLevel.LOG_FATAL)} No crash dump generated.");
		//}
		string msg = $"OpenXcom has crashed: {error}{Environment.NewLine}More details here: {Logger.logFile()}{Environment.NewLine}If this error was unexpected, please report it to the developers.";
		showError(msg);
	}

	/**
	 * Displays a message box with an error message.
	 * @param error Error message.
	 */
	static void showError(string error)
	{
        //MessageBoxA(NULL, error.c_str(), "OpenXcom Error", MB_ICONERROR | MB_OK);
        Console.WriteLine($"{Log(SeverityLevel.LOG_FATAL)} {error}");
	}

    /**
     * Logs the stack back trace leading up to this function call.
     * @param ctx Pointer to stack context (PCONTEXT on Windows), NULL to use current context.
     */
    static void stackTrace(Exception ex) =>
        Console.WriteLine($"{Log(SeverityLevel.LOG_FATAL)} {ex.StackTrace}");

	/**
	 * Checks if a certain path exists and is a folder.
	 * @param path Full path to folder.
	 * @return Does it exist?
	 */
	internal static bool folderExists(string path) =>
		Directory.Exists(path);

    /**
	 * Gets the name of all the files
	 * contained in a certain folder.
	 * @param path Full path to folder.
	 * @param ext Extension of files ("" if it doesn't matter).
	 * @return Ordered list of all the files.
	 */
	internal static List<string> getFolderContents(string path, string ext = "")
	{
		var files = new List<string>();
		string[] allFiles;

		try
		{
			allFiles = Directory.GetFiles(path);
		}
		catch (Exception)
		{
			string errorMessage = $"Failed to open directory: {path}";
			throw new IOException(errorMessage);
		}

		foreach (var file in allFiles)
		{
			if (!string.IsNullOrEmpty(file) && file[0] == '.')
			{
				//skip ".", "..", ".git", ".svn", ".bashrc", ".ssh" etc.
				continue;
			}
			if (!compareExt(file, ext))
			{
				continue;
			}

			files.Add(file);
		}
        files.Sort();
		return files;
	}

	internal static string searchDataFolder(string foldername)
	{
		// Correct folder separator
		string name = foldername;
		if (OperatingSystem.IsWindows())
		{
            name = name.Replace('/', Path.PathSeparator);
		}

		// Check current data path
		string path = Options.getDataFolder() + name;
		if (folderExists(path))
		{
			return path;
		}

		// Check every other path
		foreach (var item in Options.getDataList())
		{
			path = item + name;
			if (folderExists(path))
			{
				Options.setDataFolder(item);
				return path;
			}
		}

		// Give up
		return foldername;
	}

	/**
	 * Adds an ending slash to a path if necessary.
	 * @param path Folder path.
	 * @return Terminated path.
	 */
	internal static string endPath(string path)
	{
		if (!string.IsNullOrEmpty(path) && !path.EndsWith(Path.PathSeparator))
			return path + Path.PathSeparator;
		return path;
	}

	/**
	 * Creates a folder at the specified path.
	 * @note Only creates the last folder on the path.
	 * @param path Full path.
	 * @return Folder created or not.
	 */
	internal static bool createFolder(string path)
	{
		try
		{
			Directory.CreateDirectory(path);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	/**
	 * Checks if a certain path exists and is a file.
	 * @param path Full path to file.
	 * @return Does it exist?
	 */
	internal static bool fileExists(string path) =>
		File.Exists(path);

	/**
	 * Builds a list of predefined paths for the Data folder
	 * according to the running system.
	 * @return List of data paths.
	 */
	internal static List<string> findDataFolders()
	{
		var list = new List<string>();

		if (OperatingSystem.IsWindows())
		{
			string path;

			// Get Documents folder
			if (!string.IsNullOrEmpty(path = Environment.GetFolderPath(Environment.SpecialFolder.Personal)))
			{
				path = Path.Combine(path, "OpenXcom\\");
				list.Add(path);
			}

			// Get binary directory
			if (!string.IsNullOrEmpty(path = Environment.ProcessPath))
			{
				path = Path.GetDirectoryName(path);
				list.Add(path);
			}

			// Get working directory
			if (!string.IsNullOrEmpty(path = Environment.CurrentDirectory))
			{
				list.Add(path);
			}
		}
		else if (OperatingSystem.IsLinux())
		{
			var home = getHome();
			string path;

			// Get user-specific data folders
			string xdg_data_home;
			if (!string.IsNullOrEmpty(xdg_data_home = Environment.GetEnvironmentVariable("XDG_DATA_HOME")))
			{
				path = $"{xdg_data_home}/openxcom/";
			}
			else
			{
				path = $"{home}/.local/share/openxcom/";
			}
			list.Add(path);

			// Get global data folders
			string xdg_data_dirs;
			if (!string.IsNullOrEmpty(xdg_data_dirs = Environment.GetEnvironmentVariable("XDG_DATA_DIRS")))
			{
				var xdg_data_dirs_copy = xdg_data_dirs.Split(':');
				foreach (var dir in xdg_data_dirs_copy)
				{
					path = $"{dir}/openxcom/";
					list.Add(path);
				}
			}
			list.Add("/usr/local/share/openxcom/");
			list.Add("/usr/share/openxcom/");
#if DATADIR
			path = $"{DATADIR}/";
			list.Add(path);
#endif

			// Get working directory
			list.Add("./");
		}

		return list;
	}

	/**
	 * Finds the Config folder according to the running system.
	 * @return Config path.
	 */
	internal static string findConfigFolder()
	{
		if (OperatingSystem.IsWindows())
		{
			return string.Empty;
		}
		else if (OperatingSystem.IsLinux())
		{
			var home = getHome();
			string path;
			// Get config folders
			string xdg_config_home;
			if (!string.IsNullOrEmpty(xdg_config_home = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")))
			{
				path = $"{xdg_config_home}/openxcom/";
				return path;
			}
			else
			{
				path = $"{home}/.config/openxcom/";
				return path;
			}
		}
		return null;
	}

	/**
	 * Builds a list of predefined paths for the User folder
	 * according to the running system.
	 * @return List of data paths.
	 */
	internal static List<string> findUserFolders()
	{
		var list = new List<string>();

		if (OperatingSystem.IsWindows())
		{
			string path;

			// Get Documents folder
			if (!string.IsNullOrEmpty(path = Environment.GetFolderPath(Environment.SpecialFolder.Personal)))
			{
				path = Path.Combine(path, "OpenXcom\\");
				list.Add(path);
			}

			// Get binary directory
			if (!string.IsNullOrEmpty(path = Environment.ProcessPath))
			{
				path = Path.GetDirectoryName(path);
				path = Path.Combine(path, "user\\");
				list.Add(path);
			}

			// Get working directory
			if (!string.IsNullOrEmpty(path = Environment.CurrentDirectory))
			{
				path = Path.Combine(path, "user\\");
				list.Add(path);
			}
		}
		else if (OperatingSystem.IsLinux())
		{
			var home = getHome();
			string path;

			// Get user folders
			string xdg_data_home;
			if (!string.IsNullOrEmpty(xdg_data_home = Environment.GetEnvironmentVariable("XDG_DATA_HOME")))
			{
				path = $"{xdg_data_home}/openxcom/";
			}
			else
			{
				path = $"{home}/.local/share/openxcom/";
			}
			list.Add(path);

			// Get old-style folder
			path = $"{home}/.openxcom/";
			list.Add(path);

			// Get working directory
			list.Add("./user/");
		}

		return list;
	}

	/**
	 * Gets the user's home folder according to the system.
	 * @return Absolute path to home folder.
	 */
	static string getHome()
	{
		var home = Environment.GetEnvironmentVariable("HOME");
		if (string.IsNullOrEmpty(home))
		{
			home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		}
		return home;
	}

	/**
	 * Checks if the system's default quit shortcut was pressed.
	 * @param ev SDL event.
	 * @return Is quitting necessary?
	 */
	internal static bool isQuitShortcut(SDL_Event ev)
	{
		if (OperatingSystem.IsWindows())
		{
			// Alt + F4
			return (ev.type == SDL_EventType.SDL_KEYDOWN && ev.key.keysym.sym == SDL_Keycode.SDLK_F4 && (ev.key.keysym.mod & SDL_Keymod.KMOD_ALT) != 0);
		}
		else
		{
			//TODO add other OSs shortcuts.
			return false;
		}
	}

	/**
	 * Replaces invalid filesystem characters with _.
	 * @param filename Original filename.
	 * @return Filename without invalid characters.
	 */
	internal static string sanitizeFilename(string filename) =>
        filename.Replace('<', '_').Replace('>', '_').Replace(':', '_').Replace('"', '_').Replace('/', '_').Replace('?', '_').Replace('\\', '_');

	/**
	 * Removes a file from the specified path.
	 * @param path Full path to file.
	 * @return True if the operation succeeded, False otherwise.
	 */
	internal static bool deleteFile(string path)
	{
		try
		{
            File.Delete(path);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	/**
	 * Moves a file from one path to another,
	 * replacing any existing file.
	 * @param src Source path.
	 * @param dest Destination path.
	 * @return True if the operation succeeded, False otherwise.
	 */
	internal static bool moveFile(string src, string dest)
	{
		try
		{
			File.Move(src, dest, true);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	internal static string searchDataFile(string filename)
	{
		// Correct folder separator
		string name = filename;
		if (OperatingSystem.IsWindows())
		name = name.Replace('/', Path.PathSeparator);

		// Check current data path
		string path = Options.getDataFolder() + name;
		if (fileExists(path))
		{
			return path;
		}

		// Check every other path
		foreach (var item in Options.getDataList())
		{
			path = item + name;
			if (fileExists(path))
			{
				Options.setDataFolder(item);
				return path;
			}
		}

		// Give up
		return filename;
	}

	/**
	 * Gets the current locale of the system in language-COUNTRY format.
	 * @return Locale string.
	 */
	internal static string getLocale() =>
		CultureInfo.CurrentCulture.Name;

	/**
	 * Removes the extension from a filename. Only the
	 * last dot is considered.
	 * @param filename Original filename.
	 * @return Filename without the extension.
	 */
	internal static string noExt(string filename) =>
		Path.GetFileNameWithoutExtension(filename);
}
