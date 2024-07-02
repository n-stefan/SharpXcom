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

internal class Zoom
{
	/**
	 * Wrapper around various software and OpenGL screen buffer pushing functions which zoom.
	 * Basically called just from Screen::flip()
	 *
	 * @param src The surface to zoom (input).
	 * @param dst The zoomed surface (output).
	 * @param topBlackBand Size of top black band in pixels (letterboxing).
	 * @param bottomBlackBand Size of bottom black band in pixels (letterboxing).
	 * @param leftBlackBand Size of left black band in pixels (letterboxing).
	 * @param rightBlackBand Size of right black band in pixels (letterboxing).
	 * @param glOut OpenGL output.
	 */
	internal static void flipWithZoom(SDL_Surface src, SDL_Surface dst, int topBlackBand, int bottomBlackBand, int leftBlackBand, int rightBlackBand, OpenGL glOut)
	{
		int dstWidth = dst.w - leftBlackBand - rightBlackBand;
		int dstHeight = dst.h - topBlackBand - bottomBlackBand;
		if (Screen.useOpenGL())
		{
#if !__NO_OPENGL
			if (glOut.buffer_surface != null)
			{
				SDL_BlitSurface(src.pixels, 0, glOut.buffer_surface.getSurface().pixels, 0); // TODO; this is less than ideal...

				glOut.refresh(glOut.linear, glOut.iwidth, glOut.iheight, (uint)dst.w, (uint)dst.h, topBlackBand, bottomBlackBand, leftBlackBand, rightBlackBand);
				SDL_GL_SwapWindow(dst.pixels); //SDL_GL_SwapBuffers()
			}
#endif
		}
		else if (topBlackBand <= 0 && bottomBlackBand <= 0 && leftBlackBand <= 0 && rightBlackBand <= 0)
		{
			_zoomSurfaceY(src, dst, 0, 0);
		}
		else if (dstWidth == src.w && dstHeight == src.h)
		{
			var dstrect = new SDL_Rect { x = (short)leftBlackBand, y = (short)topBlackBand, w = (ushort)src.w, h = (ushort)src.h };
			SDL_BlitSurface(src.pixels, 0, dst.pixels, ref dstrect);
		}
		else
		{
			var tmp = Marshal.PtrToStructure<SDL_Surface>(SDL_CreateRGBSurface(dst.flags, dstWidth, dstHeight, Surface.getFormat(dst).BitsPerPixel, 0, 0, 0, 0));
			_zoomSurfaceY(src, tmp, 0, 0);
			var palette = Surface.getPalette(src);
			if (palette.colors != nint.Zero && palette.ncolors != 0)
			{
				SDL_SetPaletteColors(tmp.pixels, /* SDL_LOGPAL|SDL_PHYSPAL, */ Marshal.PtrToStructure<SDL_Color[]>(palette.colors), 0, palette.ncolors);
			}
			var dstrect = new SDL_Rect { x = (short)leftBlackBand, y = (short)topBlackBand, w = (ushort)tmp.w, h = (ushort)tmp.h };
			SDL_BlitSurface(tmp.pixels, 0, dst.pixels, ref dstrect);
			SDL_FreeSurface(tmp.pixels);
		}
	}

	static bool proclaimed = false;
	unsafe static uint* sax, say;
	/**
	 * Internal 8-bit Zoomer without smoothing.
	 * Source code originally from SDL_gfx (LGPL) with permission by author.
	 *
	 * Zooms 8bit palette/Y 'src' surface to 'dst' surface.
	 * Assumes src and dst surfaces are of 8-bit depth.
	 * Assumes dst surface was allocated with the correct dimensions.
	 *
	 * @param src The surface to zoom (input).
	 * @param dst The zoomed surface (output).
	 * @param flipx Flag indicating if the image should be horizontally flipped.
	 * @param flipy Flag indicating if the image should be vertically flipped.
	 * @return 0 for success or -1 for error.
	 */
	unsafe static int _zoomSurfaceY(SDL_Surface src, SDL_Surface dst, int flipx, int flipy)
	{
		int x, y;
		uint* csax, csay;
		int csx, csy;
		byte* sp, dp, csp;
		int dgap;

		//if (Screen::use32bitScaler())
		//{
			//if (Options::useXBRZFilter)
			//{
			//	// check the resolution to see which scale we need
			//	for (size_t factor = 2; factor <= 6; factor++)
			//	{
			//		if (dst->w == src->w * (int)factor && dst->h == src->h * (int)factor)
			//		{
			//			xbrz::scale(factor, (uint32_t*)src->pixels, (uint32_t*)dst->pixels, src->w, src->h, xbrz::RGB);
			//			return 0;
			//		}
			//	}
			//}

			//if (Options::useHQXFilter)
			//{
			//	static bool initDone = false;

			//	if (!initDone)
			//	{
			//		hqxInit();
			//		initDone = true;
			//	}

			//	// HQX_API void HQX_CALLCONV hq2x_32_rb( uint32_t * src, uint32_t src_rowBytes, uint32_t * dest, uint32_t dest_rowBytes, int width, int height );

			//	if (dst->w == src->w * 2 && dst->h == src->h * 2)
			//	{
			//		hq2x_32_rb((uint32_t*)src->pixels, src->pitch, (uint32_t*)dst->pixels, dst->pitch, src->w, src->h);
			//		return 0;
			//	}

			//	if (dst->w == src->w * 3 && dst->h == src->h * 3)
			//	{
			//		hq3x_32_rb((uint32_t*)src->pixels, src->pitch, (uint32_t*)dst->pixels, dst->pitch, src->w, src->h);
			//		return 0;
			//	}

			//	if (dst->w == src->w * 4 && dst->h == src->h * 4)
			//	{
			//		hq4x_32_rb((uint32_t*)src->pixels, src->pitch, (uint32_t*)dst->pixels, dst->pitch, src->w, src->h);
			//		return 0;
			//	}
			//}
		//}

		//if (Options::useScaleFilter)
		//{
		//	// check the resolution to see which of scale2x, scale3x, etc. we need
		//	for (size_t factor = 2; factor <= 4; factor++)
		//	{
		//		if (dst->w == src->w * (int)factor && dst->h == src->h * (int)factor && !scale_precondition(factor, src->format->BytesPerPixel, src->w, src->h))
		//		{
		//			scale(factor, dst->pixels, dst->pitch, src->pixels, src->pitch, src->format->BytesPerPixel, src->w, src->h);
		//			return 0;
		//		}
		//	}
		//}

		// if we're scaling by a factor of 2 or 4, try to use a more efficient function
		/*
		if (src->format->BytesPerPixel == 1 && dst->format->BytesPerPixel == 1)
		{

	#ifdef __SSE2__
			static bool _haveSSE2 = haveSSE2();

			if (_haveSSE2 &&
				!((ptrdiff_t)src->pixels % 16) &&
				!((ptrdiff_t)dst->pixels % 16)) // alignment check
			{
				if (dst->w == src->w * 2 && dst->h == src->h * 2) return  zoomSurface2X_SSE2(src, dst);
				else if (dst->w == src->w * 4 && dst->h == src->h * 4) return  zoomSurface4X_SSE2(src, dst);
			} else
			{
				static bool complained = false;

				if (!complained)
				{
					complained = true;
					Log(LOG_ERROR) << "Misaligned surface buffers.";
				}
			}
	#endif

	// __WORDSIZE is defined on Linux, SIZE_MAX on Windows
	#if defined(__WORDSIZE) && (__WORDSIZE == 64) || defined(SIZE_MAX) && (SIZE_MAX > 0xFFFFFFFF)
			if (dst->w == src->w * 2 && dst->h == src->h * 2) return  zoomSurface2X_64bit(src, dst);
			else if (dst->w == src->w * 4 && dst->h == src->h * 4) return  zoomSurface4X_64bit(src, dst);
	#else
			if (sizeof(void *) == 8)
			{
				if (dst->w == src->w * 2 && dst->h == src->h * 2) return  zoomSurface2X_64bit(src, dst);
				else if (dst->w == src->w * 4 && dst->h == src->h * 4) return  zoomSurface4X_64bit(src, dst);
			}
			else
			{
				if (dst->w == src->w * 2 && dst->h == src->h * 2) return  zoomSurface2X_32bit(src, dst);
				else if (dst->w == src->w * 4 && dst->h == src->h * 4) return  zoomSurface4X_32bit(src, dst);
			}
	#endif

			// maybe X is scaled by 2 or 4 but not Y?
			if (dst->w == src->w * 4) return zoomSurface4X_XAxis_32bit(src, dst);
			else if (dst->w == src->w * 2) return zoomSurface2X_XAxis_32bit(src, dst);
		}
		*/
		if (!proclaimed)
		{
			Console.WriteLine($"{Log(SeverityLevel.LOG_INFO)} Using software scaling routine. For best results, try an OpenGL filter.");
			proclaimed = true;
		}

		/*
		* Allocate memory for row increments
		*/
		if ((sax = (uint*)Marshal.ReAllocHGlobal((nint)sax, (dst.w + 1) * sizeof(uint))) == (uint*)0) {
			sax = (uint*)0;
			return (-1);
		}
		if ((say = (uint*)Marshal.ReAllocHGlobal((nint)say, (dst.h + 1) * sizeof(uint))) == (uint*)0) {
			say = (uint*)0;
			//free(sax);
			return (-1);
		}

		/*
		* Pointer setup
		*/
		sp = csp = (byte*)src.pixels;
		dp = (byte*)dst.pixels;
		dgap = dst.pitch - dst.w;

		if (flipx != 0) csp += (src.w-1);
		if (flipy != 0) csp  = ((byte*)csp + src.pitch*(src.h-1));

		/*
		* Precalculate row increments
		*/
		csx = 0;
		csax = sax;
		for (x = 0; x < dst.w; x++) {
			csx += src.w;
			*csax = 0;
			while (csx >= dst.w) {
				csx -= dst.w;
				(*csax)++;
			}
			(*csax) = (uint)((*csax) * (flipx != 0 ? -1 : 1));
			csax++;
		}
		csy = 0;
		csay = say;
		for (y = 0; y < dst.h; y++) {
			csy += src.h;
			*csay = 0;
			while (csy >= dst.h) {
				csy -= dst.h;
				(*csay)++;
			}
			(*csay) = (uint)((*csay) * src.pitch * (flipy != 0 ? -1 : 1));
			csay++;
		}
		/*
		* Draw
		*/
		csay = say;
		for (y = 0; y < dst.h; y++) {
			csax = sax;
			sp = csp;
			for (x = 0; x < dst.w; x++) {
				/*
				* Draw
				*/
				*dp = *sp;
				/*
				* Advance source pointers
				*/
				sp += (*csax);
				csax++;
				/*
				* Advance destination pointer
				*/
				dp++;
			}
			/*
			* Advance source pointer (for row)
			*/
			csp += (*csay);
			csay++;

			/*
			* Advance destination pointers
			*/
			dp += dgap;
		}

		/*
		* Never remove temp arrays
		*/
		//free(sax);
		//free(say);

		return 0;
	}

    /**
     * Checks the SSE2 feature bit returned by the CPUID instruction
     * @return Does the CPU support SSE2?
     */
    static bool haveSSE2() =>
        Sse2.X64.IsSupported;
}
