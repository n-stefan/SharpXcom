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
    internal static void flipWithZoom(SDL_Surface src, /* SDL_Window */ Screen dst, int topBlackBand, int bottomBlackBand, int leftBlackBand, int rightBlackBand, OpenGL glOut)
    {
        var nullrect = new SDL_Rect();
        int dstWidth = dst.getWidth() - leftBlackBand - rightBlackBand;
        int dstHeight = dst.getHeight() - topBlackBand - bottomBlackBand;
        if (Screen.useOpenGL())
        {
#if !__NO_OPENGL
            if (glOut.buffer_surface != null)
            {
                SDL_BlitSurface(src.pixels, nint.Zero, glOut.buffer_surface.getSurface().pixels, nint.Zero); // TODO; this is less than ideal...

                glOut.refresh(glOut.linear, glOut.iwidth, glOut.iheight, (uint)dst.getWidth(), (uint)dst.getHeight(), topBlackBand, bottomBlackBand, leftBlackBand, rightBlackBand);
                SDL_GL_SwapWindow(glOut.buffer_surface.getSurface().pixels); //SDL_GL_SwapBuffers()
            }
#endif
        }
        else if (topBlackBand <= 0 && bottomBlackBand <= 0 && leftBlackBand <= 0 && rightBlackBand <= 0)
        {
            //TODO
            //_zoomSurfaceY(src, dst, 0, 0);
        }
        else if (dstWidth == src.w && dstHeight == src.h)
        {
            var dstrect = new SDL_Rect { x = leftBlackBand, y = topBlackBand, w = src.w, h = src.h };
            SDL_BlitSurface(src.pixels, ref nullrect, dst.getWindow(), ref dstrect);
        }
        else
        {
            /* SDL_Surface */ nint tmp = SDL_CreateRGBSurface((uint)dst.getFlags(), dstWidth, dstHeight, dst.getBitsPerPixel(), 0, 0, 0, 0);
            //TODO
            //_zoomSurfaceY(src, tmp, 0, 0);
            var palette = Surface.getPalette(src);
            var colors = Marshal.PtrToStructure<SDL_Color[]>(palette.colors);
            SDL_SetPaletteColors(tmp, colors, 0, palette.ncolors);
            var dstrect = new SDL_Rect { x = leftBlackBand, y = topBlackBand, w = dstWidth /* tmp.w */, h = dstHeight /* tmp.h */ };
            SDL_BlitSurface(tmp, ref nullrect, dst.getWindow(), ref dstrect);
            SDL_FreeSurface(tmp);
        }
    }
}
