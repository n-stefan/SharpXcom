
//TODO:
// yaml load defaults
// string interpolation => StringBuilder
// Path.Combine
// SDL_Surface => SDL_Texture

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

/** @mainpage
 * @author OpenXcom Developers
 *
 * OpenXcom is an open-source clone of the original X-Com
 * written entirely in C++ and SDL. This documentation contains info
 * on every class contained in the source code and its public methods.
 * The code itself also contains in-line comments for more complicated
 * code blocks. Hopefully all of this will make the code a lot more
 * readable for you in case you which to learn or make use of it in
 * your own projects, though note that all the source code is licensed
 * under the GNU General Public License. Enjoy!
 */

Game game;

// If you can't tell what the main() is for you should have your
// programming license revoked...
AppDomain.CurrentDomain.UnhandledException += crashLogger;
// Uncomment to debug crash handler
// AddVectoredContinueHandler(1, crashLogger);

CrossPlatform.getErrorDialog();

#if DEBUG
    Logger.reportingLevel = SeverityLevel.LOG_DEBUG;
#else
    Logger.reportingLevel = SeverityLevel.LOG_INFO;
#endif
if (!Options.init(args))
    Environment.Exit(0); //EXIT_SUCCESS
string title = $"SharpXcom {SHARPXCOM_VERSION_SHORT}{SHARPXCOM_VERSION_GIT}";
if (Options.verboseLogging)
    Logger.reportingLevel = SeverityLevel.LOG_VERBOSE;
Options.baseXResolution = Options.displayWidth;
Options.baseYResolution = Options.displayHeight;

game = new Game(title);
State.setGamePtr(game);
game.setState(new StartState());
game.run();

// Comment this for faster exit.
game = null;
Environment.Exit(0); //EXIT_SUCCESS

// Crash handling routines
static void crashLogger(object sender, UnhandledExceptionEventArgs args) =>
    CrossPlatform.crashDump((Exception)args.ExceptionObject, string.Empty);
