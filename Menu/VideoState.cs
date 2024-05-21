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

struct soundInFile
{
	internal string catFile;
	internal int sound;
	internal int volume;
}

struct introSoundEffect
{
	internal int frameNumber;
	internal int sound;
}

struct AudioSequence
{
	Mod.Mod mod;
	Music m;
	Sound s;
	int trackPosition;
	FlcPlayer _flcPlayer;

	static introSoundEffect[] introSoundTrack =
	{
		new() { frameNumber = 0, sound = 0x200 }, // inserting this to keep the code simple
		new() { frameNumber = 149, sound = 0x11 },
		new() { frameNumber = 173, sound = 0x0C },
		new() { frameNumber = 183, sound = 0x0E },
		new() { frameNumber = 205, sound = 0x15 },
		new() { frameNumber = 211, sound = 0x201 },
		new() { frameNumber = 211, sound = 0x407 },
		new() { frameNumber = 223, sound = 0x7 },
		new() { frameNumber = 250, sound = 0x1 },
		new() { frameNumber = 253, sound = 0x1 },
		new() { frameNumber = 255, sound = 0x1 },
		new() { frameNumber = 257, sound = 0x1 },
		new() { frameNumber = 260, sound = 0x1 },
		new() { frameNumber = 261, sound = 0x3 },
		new() { frameNumber = 262, sound = 0x1 },
		new() { frameNumber = 264, sound = 0x1 },
		new() { frameNumber = 268, sound = 0x1 },
		new() { frameNumber = 270, sound = 0x1 },
		new() { frameNumber = 272, sound = 0x5 },
		new() { frameNumber = 272, sound = 0x1 },
		new() { frameNumber = 274, sound = 0x1 },
		new() { frameNumber = 278, sound = 0x1 },
		new() { frameNumber = 280, sound = 0x1 },
		new() { frameNumber = 282, sound = 0x8 },
		new() { frameNumber = 282, sound = 0x1 },
		new() { frameNumber = 284, sound = 0x1 },
		new() { frameNumber = 286, sound = 0x1 },
		new() { frameNumber = 288, sound = 0x1 },
		new() { frameNumber = 290, sound = 0x1 },
		new() { frameNumber = 292, sound = 0x6 },
		new() { frameNumber = 292, sound = 0x1 },
		new() { frameNumber = 296, sound = 0x1 },
		new() { frameNumber = 298, sound = 0x1 },
		new() { frameNumber = 300, sound = 0x1 },
		new() { frameNumber = 302, sound = 0x1 },
		new() { frameNumber = 304, sound = 0x1 },
		new() { frameNumber = 306, sound = 0x1 },
		new() { frameNumber = 308, sound = 0x1 },
		new() { frameNumber = 310, sound = 0x1 },
		new() { frameNumber = 312, sound = 0x1 },
		new() { frameNumber = 378, sound = 0x202 },
		new() { frameNumber = 378, sound = 0x9 }, // alarm
		new() { frameNumber = 386, sound = 0x9 },
		new() { frameNumber = 393, sound = 0x9 },
		new() { frameNumber = 399, sound = 0x17 }, // bleeps
		new() { frameNumber = 433, sound = 0x17 },
		new() { frameNumber = 463, sound = 0x12 }, // warning
		new() { frameNumber = 477, sound = 0x12 },
		new() { frameNumber = 487, sound = 0x13 }, // ufo detected
		new() { frameNumber = 495, sound = 0x16 }, // voice
		new() { frameNumber = 501, sound = 0x16 },
		new() { frameNumber = 512, sound = 0xd },  // feet -- not in original
		new() { frameNumber = 514, sound = 0xd },  // feet -- not in original
		new() { frameNumber = 522, sound = 0x0B }, // rifle grab
		new() { frameNumber = 523, sound = 0xd },  // feet -- not in original
		new() { frameNumber = 525, sound = 0xd },  // feet -- not in original
		new() { frameNumber = 534, sound = 0x18 },
		new() { frameNumber = 535, sound = 0x405 },
		new() { frameNumber = 560, sound = 0x407 },
		new() { frameNumber = 577, sound = 0x14 },
		new() { frameNumber = 582, sound = 0x405 },
		// { 582, 0x18 }, // landing! correcting to landing sound!
		new() { frameNumber = 582, sound = 0x19 },
		new() { frameNumber = 613, sound = 0x407 },
		new() { frameNumber = 615, sound = 0x10 },
		new() { frameNumber = 635, sound = 0x14 },
		new() { frameNumber = 638, sound = 0x14 },
		new() { frameNumber = 639, sound = 0x14 },
		new() { frameNumber = 644, sound = 0x2 },
		new() { frameNumber = 646, sound = 0x2 },
		new() { frameNumber = 648, sound = 0x2 },
		new() { frameNumber = 650, sound = 0x2 },
		new() { frameNumber = 652, sound = 0x2 },
		new() { frameNumber = 654, sound = 0x2 },
		new() { frameNumber = 656, sound = 0x2 },
		new() { frameNumber = 658, sound = 0x2 },
		new() { frameNumber = 660, sound = 0x2 },
		new() { frameNumber = 662, sound = 0x2 },
		new() { frameNumber = 664, sound = 0x2 },
		new() { frameNumber = 666, sound = 0x2 },
		new() { frameNumber = 668, sound = 0x401 },
		new() { frameNumber = 681, sound = 0x406 },
		new() { frameNumber = 687, sound = 0x402 },
		new() { frameNumber = 689, sound = 0x407 },
		new() { frameNumber = 694, sound = 0x0A },
		new() { frameNumber = 711, sound = 0x407 },
		new() { frameNumber = 711, sound = 0x0 },
		new() { frameNumber = 714, sound = 0x0 },
		new() { frameNumber = 716, sound = 0x4 },
		new() { frameNumber = 717, sound = 0x0 },
		new() { frameNumber = 720, sound = 0x0 },
		new() { frameNumber = 723, sound = 0x0 },
		new() { frameNumber = 726, sound = 0x5 },
		new() { frameNumber = 726, sound = 0x0 },
		new() { frameNumber = 729, sound = 0x0 },
		new() { frameNumber = 732, sound = 0x0 },
		new() { frameNumber = 735, sound = 0x0 },
		new() { frameNumber = 738, sound = 0x0 },
		new() { frameNumber = 741, sound = 0x0 },
		new() { frameNumber = 742, sound = 0x6 },
		new() { frameNumber = 744, sound = 0x0 },
		new() { frameNumber = 747, sound = 0x0 },
		new() { frameNumber = 750, sound = 0x0 },
		new() { frameNumber = 753, sound = 0x0 },
		new() { frameNumber = 756, sound = 0x0 },
		new() { frameNumber = 759, sound = 0x0 },
		new() { frameNumber = 762, sound = 0x0 },
		new() { frameNumber = 765, sound = 0x0 },
		new() { frameNumber = 768, sound = 0x0 },
		new() { frameNumber = 771, sound = 0x0 },
		new() { frameNumber = 774, sound = 0x0 },
		new() { frameNumber = 777, sound = 0x0 },
		new() { frameNumber = 780, sound = 0x0 },
		new() { frameNumber = 783, sound = 0x0 },
		new() { frameNumber = 786, sound = 0x0 },
		new() { frameNumber = 790, sound = 0x15 },
		new() { frameNumber = 790, sound = 0x15 },
		new() { frameNumber = 807, sound = 0x2 },
		new() { frameNumber = 810, sound = 0x2 },
		new() { frameNumber = 812, sound = 0x2 },
		new() { frameNumber = 814, sound = 0x2 },
		new() { frameNumber = 816, sound = 0x0 },
		new() { frameNumber = 819, sound = 0x0 },
		new() { frameNumber = 822, sound = 0x0 },
		new() { frameNumber = 824, sound = 0x40A },
		new() { frameNumber = 824, sound = 0x5 },
		new() { frameNumber = 827, sound = 0x6 },
		new() { frameNumber = 835, sound = 0x0F },
		new() { frameNumber = 841, sound = 0x0F },
		new() { frameNumber = 845, sound = 0x0F },
		new() { frameNumber = 855, sound = 0x407 },
		new() { frameNumber = 879, sound = 0x0C },
		new() { frameNumber = 65535, sound = 0x0FFFF }
	};

	// an attempt at a mix of (subjectively) the best sounds from the two versions
	// difficult because we can't find a definitive map from old sequence numbers to SAMPLE3.CAT indexes
	// probably only the Steam version of the game comes with both INTRO.CAT and SAMPLE3.CAT
	static soundInFile?[] hybridIntroSounds =
	{
		new() { catFile = "SAMPLE3.CAT", sound = 24, volume = 32 }, // machine gun
		new() { catFile = "SAMPLE3.CAT", sound = 5, volume = 32 },   // plasma rifle
		new() { catFile = "SAMPLE3.CAT", sound = 23, volume = 32 }, // rifle
		new() { catFile = "INTRO.CAT", sound = 3, volume = 32 }, // some kind of death noise, urgh?
		new() { catFile = "INTRO.CAT", sound = 0x4, volume = 64 }, // mutdie
		new() { catFile = "INTRO.CAT", sound = 0x5, volume = 64 }, // dying alien
		new() { catFile = "INTRO.CAT", sound = 0x6, volume = 64 }, // another dying alien
		new() { catFile = "INTRO.CAT", sound = 0x7, volume = 32 }, // ??? ship flying? alien screech?
		new() { catFile = "SAMPLE3.CAT", sound = 0x8, volume = 32 }, // fscream
		new() { catFile = "SAMPLE3.CAT", sound = 11, volume = 32 }, // alarm
		new() { catFile = "SAMPLE3.CAT", sound = 4, volume = 32 }, // gun spinning up?
		new() { catFile = "INTRO.CAT", sound = 0xb, volume = 32 },  // reload; this one's not even in sample3
		new() { catFile = "SAMPLE3.CAT", sound = 19, volume = 48 },  // whoosh
		new() { catFile = "INTRO.CAT", sound = 0xd, volume = 32 },  // feet, also not in sample3
		new() { catFile = "INTRO.CAT", sound = 0xe, volume = 32 },  // low pulsating hum
		new() { catFile = "SAMPLE3.CAT", sound = 30, volume = 32 }, // energise
		new() { catFile = "SAMPLE3.CAT", sound = 21, volume = 32 }, // hatch
		new() { catFile = "INTRO.CAT", sound = 0x11, volume = 64 }, // phizz
		new() { catFile = "SAMPLE3.CAT", sound = 13, volume = 32 }, // warning
		new() { catFile = "SAMPLE3.CAT", sound = 14, volume = 32 }, // detected
		new() { catFile = "SAMPLE3.CAT", sound = 19, volume = 64 }, // UFO flyby whoosh?
		new() { catFile = "INTRO.CAT", sound = 0x15, volume = 32 }, // growl
		new() { catFile = "SAMPLE3.CAT", sound = 15, volume = 128 }, // voice
		new() { catFile = "SAMPLE3.CAT", sound = 12, volume = 32 }, // beep 1
		new() { catFile = "SAMPLE3.CAT", sound = 18, volume = 32 }, // takeoff
		new() { catFile = "SAMPLE3.CAT", sound = 20, volume = 32 }  // another takeoff/landing sound?? if it exists?
	};

	// the pure MS-DOS experience
	static soundInFile?[] introCatOnlySounds =
	{
		new() { catFile = "INTRO.CAT", sound = 0x0, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x1, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x2, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x3, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x4, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x5, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x6, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x7, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x8, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x9, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0xa, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0xb, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0xc, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0xd, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0xe, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0xf, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x10, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x11, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x12, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x13, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x14, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x15, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x16, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x17, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x18, volume = 32 },
		new() { catFile = "INTRO.CAT", sound = 0x18, volume = 32 }
	};

	static soundInFile?[] sample3CatOnlySounds =
	{
		new() { catFile = "SAMPLE3.CAT", sound = 24, volume = 32 }, // machine gun
		new() { catFile = "SAMPLE3.CAT", sound = 5, volume = 32 },   // plasma rifle
		new() { catFile = "SAMPLE3.CAT", sound = 23, volume = 32 }, // rifle
		new() { catFile = "SAMPLE3.CAT", sound = 6, volume = 32 }, // some kind of death noise, urgh?
		new() { catFile = "SAMPLE3.CAT", sound = 9, volume = 64 }, // mutdie
		new() { catFile = "SAMPLE3.CAT", sound = 7, volume = 64 }, // dying alien
		new() { catFile = "SAMPLE3.CAT", sound = 27, volume = 64 }, // another dying alien
		new() { catFile = "SAMPLE3.CAT", sound = 4, volume = 32 }, // ??? ship flying? alien screech?
		new() { catFile = "SAMPLE3.CAT", sound = 0x8, volume = 32 }, // fscream
		new() { catFile = "SAMPLE3.CAT", sound = 11, volume = 32 }, // alarm
		new() { catFile = "SAMPLE3.CAT", sound = 4, volume = 32 }, // gun spinning up?
		new() { catFile = "INTRO.CAT", sound = 0xb, volume = 32 },  // reload; this one's not even in sample3
		new() { catFile = "SAMPLE3.CAT", sound = 19, volume = 48 },  // whoosh
		new() { catFile = "INTRO.CAT", sound = 0xd, volume = 32 },  // feet, also not in sample3
		new() { catFile = "SAMPLE3.CAT", sound = 2, volume = 32 },  // low pulsating hum
		new() { catFile = "SAMPLE3.CAT", sound = 30, volume = 32 }, // energise
		new() { catFile = "SAMPLE3.CAT", sound = 21, volume = 32 }, // hatch
		new() { catFile = "SAMPLE3.CAT", sound = 0, volume = 64 }, // phizz -- no equivalent in sample3.cat?
		new() { catFile = "SAMPLE3.CAT", sound = 13, volume = 32 }, // warning
		new() { catFile = "SAMPLE3.CAT", sound = 14, volume = 32 }, // detected
		new() { catFile = "SAMPLE3.CAT", sound = 19, volume = 64 }, // UFO flyby whoosh?
		new() { catFile = "SAMPLE3.CAT", sound = 3, volume = 32 }, // growl
		new() { catFile = "SAMPLE3.CAT", sound = 15, volume = 128 }, // voice
		new() { catFile = "SAMPLE3.CAT", sound = 12, volume = 32 }, // beep 1
		new() { catFile = "SAMPLE3.CAT", sound = 18, volume = 32 }, // takeoff
		new() { catFile = "SAMPLE3.CAT", sound = 20, volume = 32 }  // another takeoff/landing sound?? if it exists?
	};

	// sample3: 18 is takeoff, 20 is landing; 19 is flyby whoosh sound, not sure for which craft

	static List<soundInFile?> introSounds = new(hybridIntroSounds.Concat(introCatOnlySounds).Concat(sample3CatOnlySounds).Concat(null));

	internal AudioSequence(Mod.Mod _mod, FlcPlayer flcPlayer)
	{
		mod = _mod;
		m = null;
		s = null;
		trackPosition = 0;
		_flcPlayer = flcPlayer;
	}

	internal void play()
	{
		while (_flcPlayer.getFrameCount() >= introSoundTrack[trackPosition].frameNumber)
		{
			int command = introSoundTrack[trackPosition].sound;

			if ((command & 0x200) != 0)
			{
#if !__NO_MUSIC
				switch(command)
				{
				case 0x200:
					Console.WriteLine($"{Log(SeverityLevel.LOG_DEBUG)} Playing gmintro1");
					m = mod.getMusic("GMINTRO1");
					m.play(1);
					break;
				case 0x201:
					Console.WriteLine($"{Log(SeverityLevel.LOG_DEBUG)} Playing gmintro2");
					m = mod.getMusic("GMINTRO2");
					m.play(1);
					break;
				case 0x202:
					Console.WriteLine($"{Log(SeverityLevel.LOG_DEBUG)} Playing gmintro3");
					m = mod.getMusic("GMINTRO3");
					m.play(1);
					//Mix_HookMusicFinished(_FlcPlayer.stop);
					break;
				}
#endif
			}
			else if ((command & 0x400) != 0)
			{
				int newSpeed = (command & 0xff);
				_flcPlayer.setHeaderSpeed(newSpeed);
				Console.WriteLine($"{Log(SeverityLevel.LOG_DEBUG)} Frame delay now: {newSpeed}");
			}
			else if (command <= 0x19)
			{
				for (var sounds = 0; sounds < introSounds.Count && introSounds[sounds] != null; ++sounds) // try hybrid sound set, then intro.cat or sample3.cat alone
				{
					soundInFile sf = introSounds[sounds + command].Value;
					int channel = trackPosition % 4; // use at most four channels to play sound effects
					double ratio = (double)Options.soundVolume / MIX_MAX_VOLUME;
					Console.WriteLine($"{Log(SeverityLevel.LOG_DEBUG)} playing: {sf.catFile}:{sf.sound} for index {command}");
					s = mod.getSound(sf.catFile, (uint)sf.sound, false);
					if (s != null)
					{
						s.play(channel);
						Mix_Volume(channel, (int)(sf.volume * ratio));
						break;
					}
					else Console.WriteLine($"{Log(SeverityLevel.LOG_DEBUG)} Couldn't play {sf.catFile}:{sf.sound}");
				}
			}
			++trackPosition;
		}
	}
}

/**
 * Shows video cinematics.
 */
internal class VideoState : State
{
	List<string> _videos, _tracks;
	bool _useUfoAudioSequence;
	static AudioSequence audioSequence = default;

	/**
	 * Initializes all the elements in the Intro screen.
	 * @param game Pointer to the core game.
	 * @param wasLetterBoxed Was the game letterboxed?
	 */
	internal VideoState(List<string> videos, List<string> tracks, bool useUfoAudioSequence)
	{
		_videos = videos;
		_tracks = tracks;
		_useUfoAudioSequence = useUfoAudioSequence;
	}

	/**
	 *
	 */
	~VideoState() { }

	internal override void init()
	{
		base.init();

		bool wasLetterboxed = CutsceneState.initDisplay();

		bool ufoIntroSoundFileDosExists = false;
		bool ufoIntroSoundFileWinExists = false;
		int prevMusicVol = Options.musicVolume;
		int prevSoundVol = Options.soundVolume;
		if (_useUfoAudioSequence)
		{
			HashSet<string> soundDir = FileMap.getVFolderContents("SOUND");
			ufoIntroSoundFileDosExists = soundDir.Contains("intro.cat");
			ufoIntroSoundFileWinExists = soundDir.Contains("sample3.cat");

			if (!ufoIntroSoundFileDosExists && !ufoIntroSoundFileWinExists)
			{
				_useUfoAudioSequence = false;
			}
			else
			{
				// ensure user can hear both music and sound effects for the
				// vanilla intro sequence
				Options.musicVolume = Options.soundVolume = Math.Max(prevMusicVol, prevSoundVol);
				_game.setVolume(Options.soundVolume, Options.musicVolume, -1);
			}
		}
		_game.getCursor().setVisible(false);

		int dx = (Options.baseXResolution - Screen.ORIGINAL_WIDTH) / 2;
		int dy = (Options.baseYResolution - Screen.ORIGINAL_HEIGHT) / 2;

		// We can only do a fade out in 8bpp, otherwise instantly end it
		bool fade = (_game.getScreen().getBitsPerPixel() == 8);
		const int FADE_DELAY = 45;
		const int FADE_STEPS = 20;

		FlcPlayer flcPlayer = null;
		int audioCounter = 0;
		foreach (var it in _videos)
		{
			bool useInternalAudio = true;
			if (_tracks.Any() && _tracks.Count > audioCounter && _game.getMod().getMusic(_tracks[audioCounter]) != null)
			{
				_game.getMod().getMusic(_tracks[audioCounter]).play(0);
				useInternalAudio = false;
			}
			audioCounter++;
			string videoFileName = FileMap.getFilePath(it);

			if (!CrossPlatform.fileExists(videoFileName))
			{
				continue;
			}

			if (flcPlayer == null)
			{
				flcPlayer = new FlcPlayer();
			}

			if (_useUfoAudioSequence)
			{
				audioSequence = new AudioSequence(_game.getMod(), flcPlayer);
			}

			flcPlayer.init(videoFileName,
				 _useUfoAudioSequence ? audioHandler : null,
				 _game, useInternalAudio, dx, dy);
			flcPlayer.play(_useUfoAudioSequence);
			if (_useUfoAudioSequence)
			{
				flcPlayer.delay(10000);
				audioSequence = default;
			}
			flcPlayer.deInit();

			if (flcPlayer.wasSkipped())
			{
				fade = false;
				break;
			}
		}

		if (flcPlayer != null)
		{
			flcPlayer = null;
		}

	#if !__NO_MUSIC
		// fade out!
		if (fade)
		{
			Mix_FadeOutChannel(-1, FADE_DELAY * FADE_STEPS);
			// SDL_Mixer has trouble with native midi and volume on windows,
			// which is the most likely use case, so f@%# it.
			if (Mix_GetMusicType(0) != Mix_MusicType.MUS_MID)
			{
				Mix_FadeOutMusic(FADE_DELAY * FADE_STEPS);
				func_fade();
			}
			else
			{
				Mix_HaltMusic();
			}
		}
		else
		{
			Mix_HaltChannel(-1);
			Mix_HaltMusic();
		}
	#endif

		if (fade)
		{
			var pal = new SDL_Color[256];
			var pal2 = new SDL_Color[256];
			Array.Copy(_game.getScreen().getPalette(), pal, 256);
			for (int i = FADE_STEPS; i > 0; --i)
			{
				SDL_Event @event;
				if (SDL_PollEvent(out @event) != 0 && @event.type == SDL_EventType.SDL_KEYDOWN) break;
				for (int color = 0; color < 256; ++color)
				{
					pal2[color].r = (byte)((((int)pal[color].r) * i) / 20);
					pal2[color].g = (byte)((((int)pal[color].g) * i) / 20);
					pal2[color].b = (byte)((((int)pal[color].b) * i) / 20);
					pal2[color].a = pal[color].a;
				}
				_game.getScreen().setPalette(pal2, 0, 256, true);
				_game.getScreen().flip();
				SDL_Delay(FADE_DELAY);
			}
		}
		_game.getScreen().clear();
		_game.getScreen().flip();

		if (_useUfoAudioSequence)
		{
			Options.musicVolume = prevMusicVol;
			Options.soundVolume = prevSoundVol;
			_game.setVolume(Options.soundVolume, Options.musicVolume, Options.uiVolume);
		}

	#if !__NO_MUSIC
		Sound.stop();
		Music.stop();
	#endif

		_game.getCursor().setVisible(true);
		CutsceneState.resetDisplay(wasLetterboxed);
		_game.popState();
	}

	static void audioHandler() =>
		audioSequence.play();
}
