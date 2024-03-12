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

namespace SharpXcom.Mod;

struct SlideshowHeader
{
    internal string musicId; // just the extension-less filename, like "GMWIN"
    internal int transitionSeconds; // number of seconds to show each slide
};

struct SlideshowSlide
{
    internal string imagePath; // relative path to slide image
    internal string caption; // STR id of caption text
    internal int w, h, x, y, color; // caption rect and color info
    internal TextHAlign align; // caption alignment
    internal int transitionSeconds; // number of seconds to show this slide
};

internal class RuleVideo : IRule
{
    string _id;
    bool _useUfoAudioSequence;
    List<string> _videos, _audioTracks;
    SlideshowHeader _slideshowHeader;
    List<SlideshowSlide> _slides;

    RuleVideo(string id) =>
        _id = id;

    public IRule Create(string type) =>
        new RuleVideo(type);

    ~RuleVideo() { }

	internal void load(YamlNode node)
	{
		_useUfoAudioSequence = bool.Parse(node["useUfoAudioSequence"].ToString());

		if (node["videos"] is YamlSequenceNode videos)
		{
			foreach (var i in videos.Children)
				_videos.Add(i.ToString());
		}

		if (node["audioTracks"] is YamlSequenceNode tracks)
		{
			foreach (var i in tracks.Children)
				_audioTracks.Add(i.ToString());
		}

		if (node["slideshow"] is YamlSequenceNode slideshow)
		{
			_slideshowHeader.musicId = slideshow["musicId"].ToString();
			_slideshowHeader.transitionSeconds = int.Parse(slideshow["transitionSeconds"].ToString());

			foreach (var i in ((YamlSequenceNode)slideshow["slides"]).Children)
			{
				var slide = new SlideshowSlide();
				_loadSlide(slide, i);
				_slides.Add(slide);
			}
		}
	}

	static void _loadSlide(SlideshowSlide slide, YamlNode node)
	{
		slide.imagePath = node["imagePath"].ToString();
		slide.caption = node["caption"].ToString();

		slide.w = node["captionSize"] != null ? int.Parse(node["captionSize"][0].ToString()) : Screen.ORIGINAL_WIDTH;
		slide.h = node["captionSize"] != null ? int.Parse(node["captionSize"][1].ToString()) : Screen.ORIGINAL_HEIGHT;

		slide.x = int.Parse(node["captionPos"][0].ToString());
		slide.y = int.Parse(node["captionPos"][1].ToString());

		slide.color = node["captionColor"] != null ? int.Parse(node["captionColor"].ToString()) : int.MaxValue;
		slide.transitionSeconds = int.Parse(node["transitionSeconds"].ToString());
		slide.align = node["captionAlign"] != null ? (TextHAlign)int.Parse(node["captionAlign"].ToString()) : TextHAlign.ALIGN_LEFT;
	}

	internal List<string> getVideos() =>
		_videos;

	internal List<SlideshowSlide> getSlides() =>
		_slides;

	internal List<string> getAudioTracks() =>
		_audioTracks;

	internal bool useUfoAudioSequence() =>
		_useUfoAudioSequence;

	internal SlideshowHeader getSlideshowHeader() =>
		_slideshowHeader;
}
