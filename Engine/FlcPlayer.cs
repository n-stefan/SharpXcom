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

enum PlayingState
{
	PLAYING,
	FINISHED,
	SKIPPED
}

// Taken from: http://www.compuphase.com/flic.htm
enum FileTypes
{
	FLI_TYPE = 0xAF11,
	FLC_TYPE = 0xAF12,
}

enum ChunkTypes
{
	COLOR_256 = 0x04,
	FLI_SS2 = 0x07, // or DELTA_FLC
	COLOR_64 = 0x0B,
	FLI_LC = 0x0C, // or DELTA_FLI
	BLACK = 0x0D,
	FLI_BRUN = 0x0F, // or BYTE_RUN
	FLI_COPY = 0x10,

	AUDIO_CHUNK = 0xAAAA, // This is the only exception, it's from TFTD
	PREFIX_CHUNK = 0xF100,
	FRAME_TYPE = 0xF1FA,
}

enum ChunkOpcodes
{
	PACKETS_COUNT = 0x0000, // 0000000000000000
	LAST_PIXEL = 0x8000, // 1000000000000000
	SKIP_LINES = 0xc000, // 1100000000000000
	MASK = SKIP_LINES
}

record struct AudioBuffer
{
	internal nint /* short[] */ samples;
	internal int sampleCount;
	internal int sampleBufSize;
	internal int currSamplePos;
}

struct AudioData
{
	internal int sampleRate;
	internal AudioBuffer loadingBuffer;
	internal AudioBuffer playingBuffer;
	internal SemaphoreSlim sharedLock;
}

internal class FlcPlayer
{
	byte[] _fileBuf;
	uint _fileSize;
	Memory<byte> _videoFrameData;
	Memory<byte> _chunkData;
	Memory<byte> _audioFrameData;
	ushort _frameCount;		/* Frame Counter */
	uint _headerSize;		/* Fli file size */
	ushort _headerType;		/* Fli header check */
	ushort _headerFrames;	/* Number of frames in flic */
	ushort _headerWidth;	/* Fli width */
	ushort _headerHeight;	/* Fli height */
	ushort _headerDepth;	/* Color depth */
	ushort _headerSpeed;	/* Number of video ticks between frame */
	uint _videoFrameSize;	/* Frame size in bytes */
	ushort _videoFrameType;
	ushort _frameChunks;	/* Number of chunks in frame */
	uint _chunkSize;		/* Size of chunk */
	ushort _chunkType;		/* Type of chunk */
	ushort _delayOverride;	/* FRAME_TYPE extension */
	uint _audioFrameSize;
	ushort _audioFrameType;
	System.Action _frameCallBack;
	SDL_Surface _mainScreen;
	Screen _realScreen;
	SDL_Color[] _colors = new SDL_Color[256];
	int _screenWidth;
	int _screenHeight;
	int _screenDepth;
	int _dx, _dy;
	int _offset;
	int _playingState;
	bool _hasAudio, _useInternalAudio;
	int _videoDelay;
	double _volume;
	AudioData _audioData;
	Game _game;

	internal FlcPlayer()
	{
		_fileBuf = null;
		_mainScreen = default;
		_realScreen = null;
		_game = null;

		_volume = Game.volumeExponent(Options.musicVolume);
	}

	~FlcPlayer() =>
		deInit();

	internal void deInit()
	{
		if (_mainScreen.pixels != nint.Zero && _realScreen != null)
		{
			if (_mainScreen.pixels != _realScreen.getSurface().getSurface().pixels)
				SDL_FreeSurface(_mainScreen.pixels);

			_mainScreen = default;
		}

		if (_fileBuf != null)
		{
			_fileBuf = null;

			deInitAudio();
		}
	}

	internal int getFrameCount() =>
		_frameCount;

	internal void setHeaderSpeed(int speed) =>
		_headerSpeed = (ushort)speed;

	internal bool wasSkipped() =>
		_playingState == (int)PlayingState.SKIPPED;

	internal void delay(uint milliseconds)
	{
		uint pauseStart = SDL_GetTicks();
		while (_playingState != (int)PlayingState.SKIPPED && SDL_GetTicks() < (pauseStart + milliseconds))
		{
			SDLPolling();
		}
	}

	void SDLPolling()
	{
		SDL_Event @event;
		while (SDL_PollEvent(out @event) != 0)
		{
			switch (@event.type)
			{
				case SDL_EventType.SDL_MOUSEBUTTONDOWN:
				case SDL_EventType.SDL_KEYDOWN:
					_playingState = (int)PlayingState.SKIPPED;
					break;
				case SDL_EventType.SDL_WINDOWEVENT: //SDL_VIDEORESIZE
					if (Options.allowResize && @event.window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
					{
						Options.newDisplayWidth = Options.displayWidth = Math.Max(Screen.ORIGINAL_WIDTH, @event.window.data1);
						Options.newDisplayHeight = Options.displayHeight = Math.Max(Screen.ORIGINAL_HEIGHT, @event.window.data2);
						if (_mainScreen.pixels != _realScreen.getSurface().getSurface().pixels)
						{
							_realScreen.resetDisplay();
						}
						else
						{
							_realScreen.resetDisplay();
							_mainScreen = _realScreen.getSurface().getSurface();
						}
					}
					break;
				case SDL_EventType.SDL_QUIT:
					Environment.Exit(0);
					goto default;
				default:
					break;
			}
		}
	}

	void deInitAudio()
	{
		if (_game != null)
		{
			if (!Options.mute)
			{
				Mix_HookMusic(null, nint.Zero);
				Mix_CloseAudio();
				_game.initAudio();
			}
		}
		else if (_audioData.sharedLock != null)
			_audioData.sharedLock.Dispose();

		if (_audioData.loadingBuffer != default)
		{
			Marshal.FreeHGlobal(_audioData.loadingBuffer.samples);
			_audioData.loadingBuffer = default;
		}

		if (_audioData.playingBuffer != default)
		{
			Marshal.FreeHGlobal(_audioData.playingBuffer.samples);
			_audioData.playingBuffer = default;
		}
	}

	/**
	 * Initialize data structures needed buy the player and read the whole file into memory
	 * @param filename Video file name
	 * @param frameCallback Function to call each video frame
	 * @param game Pointer to the Game instance
	 * @param dx An offset on the x axis for the video to be rendered
	 * @param dy An offset on the y axis for the video to be rendered
	 */
	internal bool init(string filename, System.Action frameCallBack, Game game, bool useInternalAudio, int dx, int dy)
	{
		if (_fileBuf != null)
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Trying to init a video player that is already initialized");
			return false;
		}

		_frameCallBack = frameCallBack;
		_realScreen = game.getScreen();
		_realScreen.clear();
		_game = game;
		_useInternalAudio = useInternalAudio;
		_dx = dx;
		_dy = dy;

		_fileSize = 0;
		_frameCount = 0;
		_audioFrameData = null;
		_hasAudio = false;
		_audioData.loadingBuffer = default;
		_audioData.playingBuffer = default;

		try
		{
			using var file = new FileStream(filename, FileMode.Open);

			long size = file.Length;
			file.Seek(0, SeekOrigin.Begin);

			// TODO: substitute with a cross-platform memory mapped file?
			_fileBuf = new byte[size];
			_fileSize = (uint)size;
			file.Read(_fileBuf, 0, (int)size);
			file.Close();
		}
		catch (Exception)
		{
			Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Could not open FLI/FLC file: {filename}");
			return false;
		}

		_audioFrameData = new Memory<byte>(_fileBuf, 127, _fileBuf.Length - 128);

		// Let's read the first 128 bytes
		readFileHeader();

		// If it's a FLC or FLI file, it's ok
		if (_headerType == (ushort)/*SDL_SwapLE16(*/FileTypes.FLI_TYPE/*)*/ || (_headerType == (ushort)/*SDL_SwapLE16(*/FileTypes.FLC_TYPE/*)*/))
		{
			_screenWidth = _headerWidth;
			_screenHeight = _headerHeight;
			_screenDepth = 8;

            Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Playing flx, {_screenWidth}x{_screenHeight}, {_headerFrames} frames");
		}
		else
		{
            Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} Flx file failed header check.");
			return false;
		}
		if (_screenWidth > _realScreen.getSurface().getWidth() && Options.displayWidth >= _screenWidth)
		{
			// base resolution of video is higher than our surface width
			// and our display resolution allows a hi-res video
			// set base resolution to video resolution
			Options.baseXResolution = _screenWidth;
			Options.baseYResolution = _screenHeight;
			_realScreen.resetDisplay();
		}
		// If the current surface used is at 8bpp use it
		if (Surface.getFormat(_realScreen.getSurface().getSurface()).BitsPerPixel == 8)
		{
			_mainScreen = _realScreen.getSurface().getSurface();
		}
		else // Otherwise create a new one
		{
			nint mainScreen = SDL_CreateRGBSurface(SDL_SWSURFACE, _realScreen.getSurface().getWidth(), _realScreen.getSurface().getHeight(), 8, 0, 0, 0, 0); //SDL_AllocSurface
			_mainScreen = Marshal.PtrToStructure<SDL_Surface>(mainScreen);
		}

		return true;
	}

	void readFileHeader()
	{
		_headerSize = readU32(_fileBuf);
		_headerType = readU16(_fileBuf.AsSpan(4));
		_headerFrames = readU16(_fileBuf.AsSpan(6));
		_headerWidth = readU16(_fileBuf.AsSpan(8));
		_headerHeight = readU16(_fileBuf.AsSpan(10));
		_headerDepth = readU16(_fileBuf.AsSpan(12));
		_headerSpeed = readU16(_fileBuf.AsSpan(16));
	}

	int readS32(Span<byte> src) =>
		(src[3] << 24) | (src[2] << 16) | (src[1] << 8) | src[0];

	uint readU32(Span<byte> src) =>
		(uint)((src[3] << 24) | (src[2] << 16) | (src[1] << 8) | src[0]);

	short readS16(Span<byte> src) =>
        (short)((src[1] << 8) | src[0]);

	ushort readU16(Span<byte> src) =>
		(ushort)((src[1] << 8) | src[0]);

	/**
	 * Starts decoding and playing the FLI/FLC file
	 */
	internal void play(bool skipLastFrame)
	{
		_playingState = (int)PlayingState.PLAYING;

		// Vertically center the video
		_dy = (_mainScreen.h - _headerHeight) / 2;

		_offset = _dy * _mainScreen.pitch + Surface.getFormat(_mainScreen).BytesPerPixel * _dx;

		// Skip file header
		_videoFrameData = new Memory<byte>(_fileBuf, 127, _fileBuf.Length - 128);
		_audioFrameData = _videoFrameData;

		while (!shouldQuit())
		{
			if (_frameCallBack != null)
				_frameCallBack();
			else // TODO: support both, in the case the callback is not some audio?
				decodeAudio(2);

			if (!shouldQuit())
				decodeVideo(skipLastFrame);

			if(!shouldQuit())
				SDLPolling();
		}
	}

	bool shouldQuit() =>
		_playingState == (int)PlayingState.FINISHED || _playingState == (int)PlayingState.SKIPPED;

	void decodeAudio(int frames)
	{
		int audioFramesFound = 0;

		while (audioFramesFound < frames && !isEndOfFile(_audioFrameData))
		{
			if (!isValidFrame(_audioFrameData.Span, _audioFrameSize, _audioFrameType))
			{
				_playingState = (int)PlayingState.FINISHED;
				break;
			}

			switch ((ChunkTypes)_audioFrameType)
			{
				case ChunkTypes.FRAME_TYPE:
				case ChunkTypes.PREFIX_CHUNK:
					_audioFrameData = _audioFrameData.Slice((int)_audioFrameSize);
					break;
				case ChunkTypes.AUDIO_CHUNK:
					ushort sampleRate;

					sampleRate = readU16(_audioFrameData.Span.Slice(8));

					_chunkData = _audioFrameData.Slice(16);

					playAudioFrame(sampleRate);

					_audioFrameData = _audioFrameData.Slice((int)(_audioFrameSize + 16));

					++audioFramesFound;

					break;
			}
		}
	}

	bool isEndOfFile(Memory<byte> pos) =>
		pos.Length == 0;
		//return (pos - _fileBuf) == (int)(_fileSize); // should be Sint64, but let's assume the videos won't be 2gb

	bool isValidFrame(Span<byte> frameHeader, uint frameSize, ushort frameType)
	{
		frameSize = readU32(frameHeader);
		frameType = readU16(frameHeader.Slice(4));

		return (frameType == (ushort)ChunkTypes.FRAME_TYPE || frameType == (ushort)ChunkTypes.AUDIO_CHUNK || frameType == (ushort)ChunkTypes.PREFIX_CHUNK);
	}

	unsafe void playAudioFrame(ushort sampleRate)
	{
		/* TFTD audio header (10 bytes)
		* Uint16 unknown1 - always 0
		* Uint16 sampleRate
		* Uint16 unknown2 - always 1 (Channels? bytes per sample?)
		* Uint16 unknown3 - always 10 (No idea)
		* Uint16 unknown4 - always 0
		* Uint8[] unsigned 1-byte 1-channel PCM data of length _chunkSize_ (so the total chunk is _chunkSize_ + 6-byte flc header + 10 byte audio header */

		if (_useInternalAudio)
		{
			if (!_hasAudio)
			{
				_audioData.sampleRate = sampleRate;
				_hasAudio = true;
				initAudio(AUDIO_S16SYS, 1);
			}
			else
			{
				/* Cannot change sample rate mid-video */
				Debug.Assert(sampleRate == _audioData.sampleRate);
			}

			_audioData.sharedLock.Wait();
			AudioBuffer loadingBuff = _audioData.loadingBuffer;
			Debug.Assert(loadingBuff.currSamplePos == 0);
            int newSize = (int)((_audioFrameSize + loadingBuff.sampleCount) * 2);
            if (newSize > loadingBuff.sampleBufSize)
			{
				/* If the sample count has changed, we need to reallocate (Handles initial state
				* of '0' sample count too, as realloc(NULL, size) == malloc(size) */
				Marshal.ReAllocHGlobal(loadingBuff.samples, newSize);
				loadingBuff.sampleBufSize = newSize;
			}

			for (int i = 0; i < _audioFrameSize; i++)
			{
                *((short*)loadingBuff.samples + loadingBuff.sampleCount + i) = (short)((float)((_chunkData.Span[i]) - 128) * 240 * _volume);
            }
            loadingBuff.sampleCount = (int)(loadingBuff.sampleCount + _audioFrameSize);

			_audioData.sharedLock.Release();
		}
		else
		{
			_audioData.sampleRate = sampleRate; // this is used to keep the framerate correct
		}
	}

	void initAudio(ushort format, byte channels)
	{
		_videoDelay = (int)(1000 / (_audioData.sampleRate / _audioFrameSize ));
		if (_useInternalAudio)
		{
			if (!Options.mute)
			{
				if (Mix_OpenAudio(_audioData.sampleRate, format, channels, (int)(_audioFrameSize * 2)) != 0)
				{
					Console.WriteLine($"{Log(SeverityLevel.LOG_ERROR)} {Mix_GetError()}");
					Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} Failed to init cutscene audio");
					Options.mute = true;
				}
			}

			/* Start runnable */
			_audioData.sharedLock = new SemaphoreSlim(1);

			_audioData.loadingBuffer = new AudioBuffer();
			_audioData.loadingBuffer.currSamplePos = 0;
			_audioData.loadingBuffer.sampleCount = 0;
			_audioData.loadingBuffer.samples = Marshal.AllocHGlobal((int)(_audioFrameSize * 2));
			_audioData.loadingBuffer.sampleBufSize = (int)(_audioFrameSize * 2);

			_audioData.playingBuffer = new AudioBuffer();
			_audioData.playingBuffer.currSamplePos = 0;
			_audioData.playingBuffer.sampleCount = 0;
			_audioData.playingBuffer.samples = Marshal.AllocHGlobal((int)(_audioFrameSize * 2));
			_audioData.playingBuffer.sampleBufSize = (int)(_audioFrameSize * 2);

			if (!Options.mute)
			{
				Mix_HookMusic(audioCallback, /* _audioData */ nint.Zero);
			}
		}
	}

	unsafe void audioCallback(nint userData, nint stream, int len)
	{
		//AudioData audio = (AudioData)userData;

		//AudioBuffer playBuff = audio.playingBuffer;

		while (len > 0)
		{
			if (_audioData.playingBuffer.sampleCount > 0)
			{
				int bytesToCopy = Math.Min(len, _audioData.playingBuffer.sampleCount * 2);
				NativeMemory.Copy((byte*)(_audioData.playingBuffer.samples + _audioData.playingBuffer.currSamplePos), (byte*)stream, (nuint)bytesToCopy);

				_audioData.playingBuffer.currSamplePos += bytesToCopy / 2;
				_audioData.playingBuffer.sampleCount -= bytesToCopy / 2;
				len -= bytesToCopy;

				Debug.Assert(_audioData.playingBuffer.sampleCount >= 0);
			}

			if (len > 0)
			{
				/* Need to swap buffers */
				_audioData.playingBuffer.currSamplePos = 0;
				_audioData.sharedLock.Wait();
				AudioBuffer tempBuff = _audioData.playingBuffer;
				/* _audioData.playingBuffer = */ _audioData.playingBuffer = _audioData.loadingBuffer;
				_audioData.loadingBuffer = tempBuff;
				_audioData.sharedLock.Release();

				if (_audioData.playingBuffer.sampleCount == 0)
					break;
			}
		}
	}

	void decodeVideo(bool skipLastFrame)
	{
		bool videoFrameFound = false;

		while (!videoFrameFound)
		{
			if (!isValidFrame(_videoFrameData.Span, _videoFrameSize, _videoFrameType))
			{
				_playingState = (int)PlayingState.FINISHED;
				break;
			}

			switch ((ChunkTypes)_videoFrameType)
			{
				case ChunkTypes.FRAME_TYPE:

					uint delay;

					_frameChunks = readU16(_videoFrameData.Span.Slice(6));
					_delayOverride = readU16(_videoFrameData.Span.Slice(8));

					if (_headerType == (ushort)FileTypes.FLI_TYPE)
					{
						delay = (uint)(_delayOverride > 0 ? _delayOverride : _headerSpeed * (1000.0 / 70.0));
					}
					else if (_useInternalAudio && _frameCallBack == null) // this means TFTD videos are playing
					{
						delay = (uint)_videoDelay;
					}
					else
					{
						delay = _headerSpeed;
					}

					waitForNextFrame(delay);

					// Skip the frame header, we are not interested in the rest
					_chunkData = _videoFrameData.Slice(16);

					_videoFrameData = _videoFrameData.Slice((int)_videoFrameSize);
					// If this frame is the last one, don't play it
					if(isEndOfFile(_videoFrameData))
						_playingState = (int)PlayingState.FINISHED;

					if(!shouldQuit() || !skipLastFrame)
						playVideoFrame();

					videoFrameFound = true;

					break;
				case ChunkTypes.AUDIO_CHUNK:
					_videoFrameData = _videoFrameData.Slice((int)(_videoFrameSize + 16));
					break;
				case ChunkTypes.PREFIX_CHUNK:
					// Just skip it
					_videoFrameData = _videoFrameData.Slice((int)_videoFrameSize);

					break;
			}
		}
	}

	static uint oldTick = 0;
	void waitForNextFrame(uint delay)
	{
		uint newTick;
		uint currentTick;

		currentTick = SDL_GetTicks();
		if (oldTick == 0)
		{
			oldTick = currentTick;
			newTick = oldTick;
		}
		else
			newTick = oldTick + delay;

		if (_hasAudio)
		{
			while (currentTick < newTick)
			{
				while ((newTick - currentTick) > 10 && !isEndOfFile(_audioFrameData))
				{
					decodeAudio(1);
					currentTick = SDL_GetTicks();
				}
				SDL_Delay(1);
				currentTick = SDL_GetTicks();
			}
		}
		else
		{
			while (currentTick < newTick)
			{
				SDL_Delay(1);
				currentTick = SDL_GetTicks();
			}
		}
		oldTick = SDL_GetTicks();
	}

	void playVideoFrame()
	{
		++_frameCount;
		if (SDL_LockSurface(_mainScreen.pixels) < 0)
			return;
		int chunkCount = _frameChunks;

		for (int i = 0; i < chunkCount; ++i)
		{
			_chunkSize = readU32(_chunkData.Span);
			_chunkType = readU16(_chunkData.Span.Slice(4));

			switch (_chunkType)
			{
				case (ushort)ChunkTypes.COLOR_256:
					color256();
					break;
				case (ushort)ChunkTypes.FLI_SS2:
					fliSS2();
					break;
				case (ushort)ChunkTypes.COLOR_64:
					color64();
					break;
				case (ushort)ChunkTypes.FLI_LC:
					fliLC();
					break;
				case (ushort)ChunkTypes.BLACK:
					black();
					break;
				case (ushort)ChunkTypes.FLI_BRUN:
					fliBRun();
					break;
				case (ushort)ChunkTypes.FLI_COPY:
					fliCopy();
					break;
				case 18:
					break;
				default:
					Console.WriteLine($"{Log(SeverityLevel.LOG_WARNING)} Ieek an non implemented chunk type:{_chunkType}");
					break;
			}

			_chunkData = _chunkData.Slice((int)_chunkSize);
		}

		SDL_UnlockSurface(_mainScreen.pixels);

		/* TODO: Track which rectangles have really changed */
		//SDL_UpdateRect(_mainScreen, 0, 0, 0, 0);
		if (_mainScreen.pixels != _realScreen.getSurface().getSurface().pixels)
			SDL_BlitSurface(_mainScreen.pixels, nint.Zero, _realScreen.getSurface().getSurface().pixels, nint.Zero);

		_realScreen.flip();
	}

	void color256()
	{
		Span<byte> pSrc;
		ushort numColorPackets;
		ushort numColors = 0;
		byte numColorsSkip;

		pSrc = _chunkData.Span.Slice(6);
		numColorPackets = readU16(pSrc);
		pSrc = pSrc.Slice(2);

		while (numColorPackets-- != 0)
		{
			numColorsSkip = (byte)(pSrc[0] + numColors); pSrc = pSrc.Slice(1);
			numColors = pSrc[0]; pSrc = pSrc.Slice(1);
			if (numColors == 0)
			{
				numColors = 256;
			}

			for (int i = 0; i < numColors; ++i)
			{
				_colors[i].r = pSrc[0]; pSrc = pSrc.Slice(1);
				_colors[i].g = pSrc[0]; pSrc = pSrc.Slice(1);
				_colors[i].b = pSrc[0]; pSrc = pSrc.Slice(1);
			}

			if (_mainScreen.pixels != _realScreen.getSurface().getSurface().pixels)
				SDL_SetPaletteColors(_mainScreen.pixels, _colors, numColorsSkip, numColors);
			_realScreen.setPalette(_colors, numColorsSkip, numColors, true);

			if (numColorPackets >= 1)
			{
				++numColors;
			}
		}
	}

	unsafe void fliSS2()
	{
		Span<byte> pSrc, pDst, pTmpDst;
		sbyte countData;
		byte columSkip, fill1, fill2;
		ushort lines;
		short count;
		bool setLastByte = false;
		byte lastByte = 0;

		pSrc = _chunkData.Span.Slice(6);
		pDst = new Span<byte>((byte*)_mainScreen.pixels + _offset, _mainScreen.w * _mainScreen.h * (_screenDepth / 8) - _offset);

		lines = readU16(pSrc);

		pSrc = pSrc.Slice(2);

		while (lines-- != 0)
		{
			count = readS16(/* (Sint8 *) */ pSrc);
			pSrc = pSrc.Slice(2);

			if (((ChunkOpcodes)count & ChunkOpcodes.MASK) == ChunkOpcodes.SKIP_LINES)
			{
				pDst = pDst.Slice((-count)*_mainScreen.pitch);
				++lines;
				continue;
			}

			else if (((ChunkOpcodes)count & ChunkOpcodes.MASK) == ChunkOpcodes.LAST_PIXEL)
			{
				setLastByte = true;
				lastByte = ((byte)(count & 0x00FF));
				count = readS16(/* (Sint8 *) */ pSrc);
				pSrc = pSrc.Slice(2);
			}

			if (((ChunkOpcodes)count & ChunkOpcodes.MASK) == ChunkOpcodes.PACKETS_COUNT)
			{
				pTmpDst = pDst;
				while (count-- != 0)
				{
					columSkip = pSrc[0]; pSrc = pSrc.Slice(1);
					pTmpDst = pTmpDst.Slice(columSkip);
					countData = (sbyte)pSrc[0]; pSrc = pSrc.Slice(1);

					if (countData > 0)
					{
						pSrc.Slice(0, 2 * countData).CopyTo(pTmpDst);
						pTmpDst = pTmpDst.Slice(2 * countData);
						pSrc = pSrc.Slice(2 * countData);
					}
					else
					{
						if (countData < 0)
						{
							countData = (sbyte)-countData;

							fill1 = pSrc[0]; pSrc = pSrc.Slice(1);
							fill2 = pSrc[0]; pSrc = pSrc.Slice(1);
							while (countData-- != 0)
							{
								pTmpDst[0] = fill1; pTmpDst = pTmpDst.Slice(1);
								pTmpDst[0] = fill2; pTmpDst = pTmpDst.Slice(1);
							}
						}
					}
				}

				if (setLastByte)
				{
					setLastByte = false;
					pDst[_mainScreen.pitch - 1] = lastByte;
				}
				pDst = pDst.Slice(_mainScreen.pitch);
			}
		}
	}

	void color64()
	{
		Span<byte> pSrc;
		ushort NumColors, NumColorPackets;
		byte NumColorsSkip;

		pSrc = _chunkData.Span.Slice(6);
		NumColorPackets = readU16(pSrc);
		pSrc = pSrc.Slice(2);

		while (NumColorPackets-- != 0)
		{
			NumColorsSkip = pSrc[0]; pSrc = pSrc.Slice(1);
			NumColors = pSrc[0]; pSrc = pSrc.Slice(1);

			if (NumColors == 0)
			{
				NumColors = 256;
			}

			for (int i = 0; i < NumColors; ++i)
			{
				_colors[i].r = (byte)(pSrc[0] << 2); pSrc = pSrc.Slice(1);
				_colors[i].g = (byte)(pSrc[0] << 2); pSrc = pSrc.Slice(1);
				_colors[i].b = (byte)(pSrc[0] << 2); pSrc = pSrc.Slice(1);
			}

			if (_mainScreen.pixels != _realScreen.getSurface().getSurface().pixels)
				SDL_SetPaletteColors(_mainScreen.pixels, _colors, NumColorsSkip, NumColors);
			_realScreen.setPalette(_colors, NumColorsSkip, NumColors, true);
		}
	}

	unsafe void fliLC()
	{
		Span<byte> pSrc, pDst, pTmpDst;
		sbyte countData;
		byte countSkip;
		byte fill;
		ushort lines, tmp;
		int packetsCount;

		pSrc = _chunkData.Span.Slice(6);
		pDst = new Span<byte>((byte*)_mainScreen.pixels + _offset, _mainScreen.w * _mainScreen.h * (_screenDepth / 8) - _offset);

		tmp = readU16(pSrc);
		pSrc = pSrc.Slice(2);
		pDst = pDst.Slice(tmp*_mainScreen.pitch);
		lines = readU16(pSrc);
		pSrc = pSrc.Slice(2);

		while (lines-- != 0)
		{
			pTmpDst = pDst;
			packetsCount = pSrc[0]; pSrc = pSrc.Slice(1);

			while (packetsCount-- != 0)
			{
				countSkip = pSrc[0]; pSrc = pSrc.Slice(1);
				pTmpDst = pTmpDst.Slice(countSkip);
				countData = (sbyte)pSrc[0]; pSrc = pSrc.Slice(1);
				if (countData > 0)
				{
					while (countData-- != 0)
					{
						pTmpDst[0] = pSrc[0]; pTmpDst = pTmpDst.Slice(1); pSrc = pSrc.Slice(1);
					}
				}
				else
				{
					if (countData < 0)
					{
						countData = (sbyte)-countData;

						fill = pSrc[0]; pSrc = pSrc.Slice(1);
						while (countData-- != 0)
						{
							pTmpDst[0] = fill; pTmpDst = pTmpDst.Slice(1);
						}
					}
				}
			}
			pDst = pDst.Slice(_mainScreen.pitch);
		}
	}

	unsafe void black()
	{
		Span<byte> pDst;
		int Lines = _screenHeight;
		pDst = new Span<byte>((byte*)_mainScreen.pixels + _offset, _mainScreen.w * _mainScreen.h * (_screenDepth / 8) - _offset);

		while (Lines-- > 0)
		{
			pDst.Slice(0, _screenHeight).Fill(0);
			pDst = pDst.Slice(_mainScreen.pitch);
		}
	}

	unsafe void fliBRun()
	{
		Span<byte> pSrc, pDst, pTmpDst; byte fill;
		sbyte countData;
		int heightCount;

		heightCount = _headerHeight;
		pSrc = _chunkData.Span.Slice(6); // Skip chunk header
		pDst = new Span<byte>((byte*)_mainScreen.pixels + _offset, _mainScreen.w * _mainScreen.h * (_screenDepth / 8) - _offset);

		while (heightCount-- != 0)
		{
			pTmpDst = pDst;
			pSrc = pSrc.Slice(1); // Read and skip the packet count value

			int pixels = 0;
			while (pixels != _headerWidth)
			{
				countData = (sbyte)pSrc[0]; pSrc = pSrc.Slice(1);
				if (countData > 0)
				{
					fill = pSrc[0]; pSrc = pSrc.Slice(1);

					pTmpDst.Slice(0, countData).Fill(fill);
					pTmpDst = pTmpDst.Slice(countData);
					pixels += countData;
				}
				else
				{
					if (countData < 0)
					{
						countData = (sbyte)-countData;

						pSrc.Slice(0, countData).CopyTo(pTmpDst);
						pTmpDst = pTmpDst.Slice(countData);
						pSrc = pSrc.Slice(countData);
						pixels += countData;
					}
				}
			}
			pDst = pDst.Slice(_mainScreen.pitch);
		}
	}

	unsafe void fliCopy()
	{
		Span<byte> pSrc, pDst;
		int Lines = _screenHeight;
		pSrc = _chunkData.Span.Slice(6);
		pDst = new Span<byte>((byte*)_mainScreen.pixels + _offset, _mainScreen.w * _mainScreen.h * (_screenDepth / 8) - _offset);

		while (Lines-- != 0)
		{
			pSrc.Slice(0, _screenWidth).CopyTo(pDst);
			pSrc = pSrc.Slice(_screenWidth);
			pDst = pDst.Slice(_mainScreen.pitch);
		}
	}

	void stop() =>
		_playingState = (int)PlayingState.FINISHED;
}
