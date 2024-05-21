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

namespace SharpXcom.Menu;

/**
 * Shows cutscenes: inspects the relevant rules and loads the appropriate state
 * for showing slideshows or videos.
 */
internal class CutsceneState : State
{
    internal const string WIN_GAME = "winGame";
    internal const string LOSE_GAME = "loseGame";

    string _cutsceneId;

    internal CutsceneState(string cutsceneId) =>
        _cutsceneId = cutsceneId;

    ~CutsceneState() { }

	internal override void init()
	{
		base.init();

		// pop self off stack and replace with actual player state
		_game.popState();

		if (_cutsceneId == WIN_GAME || _cutsceneId == LOSE_GAME)
		{
			if (_game.getSavedGame().getMonthsPassed() > -1)
			{
				_game.setState(new StatisticsState());
			}
			else
			{
				_game.setSavedGame(null);
				_game.setState(new GoToMainMenuState());
			}
		}

		RuleVideo videoRule = _game.getMod().getVideo(_cutsceneId);
		if (videoRule == null)
		{
			return;
		}

		bool fmv = false, slide = false;
		if (videoRule.getVideos().Any())
		{
			string file = FileMap.getFilePath(videoRule.getVideos().First());
			fmv = CrossPlatform.fileExists(file);
		}
		if (videoRule.getSlides().Any())
		{
			string file = FileMap.getFilePath(videoRule.getSlides().First().imagePath);
			slide = CrossPlatform.fileExists(file);
		}

		if (fmv && (!slide || Options.preferredVideo == VideoFormat.VIDEO_FMV))
		{
			_game.pushState(new VideoState(videoRule.getVideos(), videoRule.getAudioTracks(), videoRule.useUfoAudioSequence()));
		}
		else if (slide && (!fmv || Options.preferredVideo == VideoFormat.VIDEO_SLIDE))
		{
			_game.pushState(new SlideshowState(videoRule.getSlideshowHeader(), videoRule.getSlides()));
		}
		else
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} cutscene definition empty: {_cutsceneId}");
		}
	}

	internal static bool initDisplay()
	{
		bool letterboxed = Options.keepAspectRatio;
		Options.keepAspectRatio = true;
		Options.baseXResolution = Screen.ORIGINAL_WIDTH;
		Options.baseYResolution = Screen.ORIGINAL_HEIGHT;
		_game.getScreen().resetDisplay(false);
		return letterboxed;
	}

	internal static void resetDisplay(bool wasLetterboxed)
	{
		Options.keepAspectRatio = wasLetterboxed;
		Screen.updateScale(Options.geoscapeScale, ref Options.baseXGeoscape, ref Options.baseYGeoscape, true);
		_game.getScreen().resetDisplay(false);
	}
}
