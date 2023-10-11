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
 * Coloured box with text inside that fades out after it is displayed.
 * Used to display warning/error messages on the Battlescape.
 */
internal class WarningMessage : Surface
{
    byte _color, _fade;
    Text _text;
    Timer _timer;

    /**
     * Sets up a blank warning message with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal WarningMessage(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _color = 0;
        _fade = 0;

        _text = new Text(width, height, 0, 0);
        _text.setHighContrast(true);
        _text.setAlign(TextHAlign.ALIGN_CENTER);
        _text.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
        _text.setWordWrap(true);

        _timer = new Timer(50);
        _timer.onTimer((SurfaceHandler)fade);

        setVisible(false);
    }

    /**
     * Deletes timers.
     */
    ~WarningMessage()
    {
        _timer = null;
        _text = null;
    }

    /**
     * Plays the message fade animation.
     */
    void fade()
    {
        _fade++;
        _redraw = true;
        if (_fade == 24)
        {
            setVisible(false);
            _timer.stop();
        }
    }

    /**
     * Displays the warning message.
     * @param msg Message string.
     */
    internal void showMessage(string msg)
    {
	    _text.setText(msg);
	    _fade = 0;
	    _redraw = true;
	    setVisible(true);
	    _timer.start();
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
    internal void initText(Font big, Font small, Language lang) =>
        _text.initText(big, small, lang);

    /**
     * Changes the color for the message background.
     * @param color Color value.
     */
    internal void setColor(byte color) =>
        _color = color;

    /**
     * Changes the color for the message text.
     * @param color Color value.
     */
    internal void setTextColor(byte color) =>
        _text.setColor(color);

    /**
     * Keeps the animation timers running.
     */
    protected override void think() =>
	    _timer.think(null, this);
}
