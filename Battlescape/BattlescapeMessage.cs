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

namespace SharpXcom.Battlescape;

/**
 * Generic window used to display messages
 * over the Battlescape map.
 */
internal class BattlescapeMessage : Surface
{
    Window _window;
    Text _text;

    /**
     * Sets up a blank Battlescape message with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal BattlescapeMessage(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _window = new Window(null, width, height, x, y, WindowPopup.POPUP_NONE);
        _window.setColor((byte)(Palette.blockOffset(0) - 1));
        _window.setHighContrast(true);

        _text = new Text(width, height, x, y);
        _text.setColor((byte)(Palette.blockOffset(0) - 1));
        _text.setAlign(TextHAlign.ALIGN_CENTER);
        _text.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
        _text.setHighContrast(true);
    }

    /**
     * Deletes surfaces.
     */
    ~BattlescapeMessage()
    {
        _window = null;
        _text = null;
    }

    /*
     * Sets the text color of the battlescape message.
     * @param color the new color.
     */
    internal void setTextColor(byte color) =>
        _text.setColor(color);

    /**
     * Blits the warning message.
     */
    internal override void blit(Surface surface)
    {
	    base.blit(surface);
	    _window.blit(surface);
	    _text.blit(surface);
    }

    /**
     * Changes the various resources needed for text rendering.
     * The different fonts need to be passed in advance since the
     * text size can change mid-text, and the language affects
     * how the text is rendered.
     * @param big Pointer to large-size font.
     * @param small Pointer to small-size font.
     * @param lang Pointer to current language.
     */
    internal override void initText(Font big, Font small, Language lang)
    {
	    _text.initText(big, small, lang);
	    _text.setBig();
    }

    /**
     * Replaces a certain amount of colors in the surface's palette.
     * @param colors Pointer to the set of colors.
     * @param firstcolor Offset of the first color to replace.
     * @param ncolors Amount of colors to replace.
     */
    internal override void setPalette(SDL_Color[] colors, int firstcolor = 0, int ncolors = 256)
    {
	    base.setPalette(colors, firstcolor, ncolors);
	    _window.setPalette(colors, firstcolor, ncolors);
	    _text.setPalette(colors, firstcolor, ncolors);
    }

    /**
     * Changes the message background.
     * @param background Pointer to background surface.
     */
    internal void setBackground(Surface background) =>
	    _window.setBackground(background);

    /**
     * Changes the message text.
     * @param message Message string.
     */
    internal void setText(string message) =>
	    _text.setText(message);

    /**
     * Changes the position of the surface in the X axis.
     * @param x X position in pixels.
     */
    internal override void setX(int x)
    {
	    base.setX(x);
	    _window.setX(x);
	    _text.setX(x);
    }

    /**
     * Changes the position of the surface in the Y axis.
     * @param y Y position in pixels.
     */
    internal override void setY(int y)
    {
	    base.setY(y);
	    _window.setY(y);
	    _text.setY(y);
    }

    /*
     * Special handling for setting the height of the battlescape message.
     * @param height the new height.
     */
    internal override void setHeight(int height)
    {
	    base.setHeight(height);
	    _window.setHeight(height);
	    _text.setHeight(height);
    }
}
