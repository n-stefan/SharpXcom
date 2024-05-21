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
 * Shows slideshow sequences.
 */
internal class SlideshowState : State
{
	SlideshowHeader _slideshowHeader;
	List<SlideshowSlide> _slideshowSlides;
	int _curScreen;
	bool _wasLetterboxed;
	List<InteractiveSurface> _slides;
	List<Text>_captions;
	Timer _transitionTimer;

	internal SlideshowState(SlideshowHeader slideshowHeader, List<SlideshowSlide> slideshowSlides)
	{
		_slideshowHeader = slideshowHeader;
		_slideshowSlides = slideshowSlides;
		_curScreen = -1;

		_wasLetterboxed = CutsceneState.initDisplay();

		// pre-render and queue up all the frames
		foreach (var it in _slideshowSlides)
		{
			InteractiveSurface slide =
				new InteractiveSurface(Screen.ORIGINAL_WIDTH, Screen.ORIGINAL_HEIGHT, 0, 0);
			slide.loadImage(FileMap.getFilePath(it.imagePath));
			slide.onMouseClick(screenClick);
			slide.onKeyboardPress(screenClick, Options.keyOk);
			slide.onKeyboardPress(screenSkip, Options.keyCancel);
			slide.setVisible(false);
			_slides.Add(slide);
			setPalette(slide.getPaletteColors());
			add(slide);

			// initialize with default rect; may get overridden by
			// category/id definition
			Text caption = new Text(it.w, it.h, it.x, it.y);
			caption.setColor((byte)it.color);
			caption.setText(tr(it.caption));
			caption.setAlign(it.align);
			caption.setWordWrap(true);
			caption.setVisible(false);
			_captions.Add(caption);
			add(caption);
		}

		centerAllSurfaces();

		int transitionSeconds = _slideshowHeader.transitionSeconds;
		if (_slideshowSlides.First().transitionSeconds > 0)
			transitionSeconds = _slideshowSlides.First().transitionSeconds;
		_transitionTimer = new Timer((uint)(transitionSeconds * 1000));
		_transitionTimer.onTimer((StateHandler)screenTimer);

		_game.getMod().playMusic(_slideshowHeader.musicId);
		_game.getCursor().setVisible(false);
		screenClick(null);
	}

	~SlideshowState() =>
		_transitionTimer = null;

	/**
	 * Shows the next screen in the slideshow; pops the state when there are no more slides
	 */
	void screenClick(Action action)
	{
		if (_curScreen >= 0)
		{
			_slides[_curScreen].setVisible(false);
			_captions[_curScreen].setVisible(false);
		}

		++_curScreen;

		// next screen
		if (_curScreen < (int)_slideshowSlides.Count)
		{
			int transitionSeconds = _slideshowHeader.transitionSeconds;
			if (_slideshowSlides[_curScreen].transitionSeconds > 0)
				transitionSeconds = _slideshowSlides[_curScreen].transitionSeconds;
			_transitionTimer.setInterval((uint)(transitionSeconds * 1000));
			_transitionTimer.start();
			setPalette(_slides[_curScreen].getPaletteColors());
			_slides[_curScreen].setVisible(true);
			_captions[_curScreen].setVisible(true);
			init();
		}
		else
		{
			screenSkip(action);
		}
	}

	/**
	 * Skips the slideshow
	 */
	void screenSkip(Action _)
	{
		// slideshow is over.  restore the screen scale and pop the state
		_game.getCursor().setVisible(true);
		CutsceneState.resetDisplay(_wasLetterboxed);
		_game.popState();
	}

	/**
	 * Shows the next screen on a timed basis.
	 */
	void screenTimer() =>
		screenClick(null);

	/**
	 * Handle timers.
	 */
	internal override void think() =>
		_transitionTimer.think(this, null);
}
