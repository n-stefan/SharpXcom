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

namespace SharpXcom.Interface;

enum TextHAlign { ALIGN_LEFT, ALIGN_CENTER, ALIGN_RIGHT };

enum TextVAlign { ALIGN_TOP, ALIGN_MIDDLE, ALIGN_BOTTOM };

struct PaletteShift : IColorFunc<byte, byte, int, int, int>
{
	public void func(ref byte dest, byte src, int off, int mul, int mid)
	{
		if (src != 0)
		{
			int inverseOffset = mid != 0 ? 2 * (mid - src) : 0;
			dest = (byte)(off + src * mul + inverseOffset);
		}
	}
}

/**
 * Text string displayed on screen.
 * Takes the characters from a Font and puts them together on screen
 * to display a string of text, taking care of any required aligning
 * or wrapping.
 */
internal class Text : InteractiveSurface
{
    Font _big, _small, _font, _fontOrig;
    Language _lang;
    string _text;
    List<int> _lineWidth, _lineHeight;
    bool _wrap, _invert, _contrast, _indent, _scroll;
    TextHAlign _align;
    TextVAlign _valign;
    byte _color, _color2;
    int _scrollY;
    string _processedText;

    /**
     * Sets up a blank text with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal Text(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _big = new Font();
        _small = new Font();
        _font = new Font();
        _fontOrig = new Font();
        _lang = new Language();
        _wrap = false;
        _invert = false;
        _contrast = false;
        _indent = false;
        _scroll = false;
        _align = TextHAlign.ALIGN_LEFT;
        _valign = TextVAlign.ALIGN_TOP;
        _color = 0;
        _color2 = 0;
        _scrollY = 0;
    }

    /**
     *
     */
    ~Text() { }

    /**
     * Changes the various resources needed for text rendering.
     * The different fonts need to be passed in advance since the
     * text size can change mid-text, and the language affects
     * how the text is rendered.
     * @param big Pointer to large-size font.
     * @param small Pointer to small-size font.
     * @param lang Pointer to current language.
     */
    internal void initText(Font big, Font small, Language lang)
    {
        _big = big;
        _small = small;
        _lang = lang;
        setSmall();
    }

    /**
     * Changes the text to use the small-size font.
     */
    internal void setSmall()
    {
        _font = _small;
        _fontOrig = _small;
        processText();
    }

    /**
     * Changes the text to use the big-size font.
     */
    internal void setBig()
    {
        _font = _big;
        _fontOrig = _big;
        processText();
    }

    /**
     * Changes the color used to render the text. Unlike regular graphics,
     * fonts are greyscale so they need to be assigned a specific position
     * in the palette to be displayed.
     * @param color Color value.
     */
    internal void setColor(byte color)
    {
        _color = color;
        _color2 = color;
        _redraw = true;
    }

    /**
     * Enables/disables text wordwrapping. When enabled, lines of
     * text are automatically split to ensure they stay within the
     * drawing area, otherwise they simply go off the edge.
     * @param wrap Wordwrapping setting.
     * @param indent Indent wrapped text.
     */
    internal void setWordWrap(bool wrap, bool indent = false)
    {
        if (wrap != _wrap || indent != _indent)
        {
            _wrap = wrap;
            _indent = indent;
            processText();
        }
    }

    /**
     * Changes the string displayed on screen.
     * @param text Text string.
     */
    internal void setText(string text)
    {
	    _text = text;
	    _font = _fontOrig;
	    processText();
	    // If big text won't fit the space, try small text
	    if (!string.IsNullOrEmpty(_text))
	    {
		    if (_font == _big && (getTextWidth() > getWidth() || getTextHeight() > getHeight()) && _text[_text.Length - 1] != '.')
		    {
			    _font = _small;
			    processText();
		    }
	    }
    }

    /**
     * Returns the rendered text's width.
     * @param line Line to get the width, or -1 to get whole text width.
     * @return Width in pixels.
     */
    internal int getTextWidth(int line = -1)
    {
	    if (line == -1)
	    {
		    int width = 0;
		    foreach (var lineWidth in _lineWidth)
		    {
			    if (lineWidth > width)
			    {
				    width = lineWidth;
			    }
		    }
		    return width;
	    }
	    else
	    {
		    return _lineWidth[line];
	    }
    }

    /**
     * Returns the rendered text's height. Useful to check if wordwrap applies.
     * @param line Line to get the height, or -1 to get whole text height.
     * @return Height in pixels.
     */
    internal int getTextHeight(int line = -1)
    {
	    if (line == -1)
	    {
		    int height = 0;
		    foreach (var lineHeight in _lineHeight)
		    {
			    height += lineHeight;
		    }
		    return height;
	    }
	    else
	    {
		    return _lineHeight[line];
	    }
    }

    /**
     * Takes care of any text post-processing like converting
     * encoded text to individual codepoints and calculating
     * line metrics for alignment and wordwrapping.
     */
    void processText()
    {
        if (_font == null || _lang == null)
        {
            return;
        }

        _processedText = Unicode.convUtf8ToUtf32(_text);
        _lineWidth.Clear();
        _lineHeight.Clear();
        _scrollY = 0;

        int width = 0, word = 0;
        int space = 0, textIndentation = 0;
        bool start = true;
        Font font = _font;
        var str = new StringBuilder(_processedText);

        // Go through the text character by character
        for (int c = 0; c <= str.Length - 1; ++c)
        {
            // End of the line
            if (c == str.Length || Unicode.isLinebreak(str[c]))
            {
                // Add line measurements for alignment later
                _lineWidth.Add(width);
                _lineHeight.Add(font.getCharSize('\n').h);
                width = 0;
                word = 0;
                start = true;

                if (c == str.Length)
                    break;
                else if (str[c] == Unicode.TOK_NL_SMALL)
                    font = _small;
            }
            // Keep track of spaces for wordwrapping
            else if (Unicode.isSpace(str[c]) || Unicode.isSeparator(str[c]))
            {
                // Store existing indentation
                if (c == textIndentation)
                {
                    textIndentation++;
                }
                space = c;
                width += font.getCharSize(str[c]).w;
                word = 0;
                start = false;
            }
            // Keep track of the width of the last line and word
            else if (str[c] != Unicode.TOK_COLOR_FLIP)
            {
                int charWidth = font.getCharSize(str[c]).w;

                width += charWidth;
                word += charWidth;

                // Wordwrap if the last word doesn't fit the line
                if (_wrap && width >= getWidth() && (!start || _lang.getTextWrapping() == TextWrapping.WRAP_LETTERS))
                {
                    int indentLocation = c;
                    if (_lang.getTextWrapping() == TextWrapping.WRAP_WORDS || Unicode.isSpace(str[c]))
                    {
                        // Go back to the last space and put a linebreak there
                        width -= word;
                        indentLocation = space;
                        if (Unicode.isSpace(str[space]))
                        {
                            width -= font.getCharSize(str[space]).w;
                            str[space] = '\n';
                        }
                        else
                        {
                            str.Insert(space + 1, '\n');
                            indentLocation++;
                        }
                    }
                    else if (_lang.getTextWrapping() == TextWrapping.WRAP_LETTERS)
                    {
                        // Go back to the last letter and put a linebreak there
                        str.Insert(c, '\n');
                        width -= charWidth;
                    }

                    // Keep initial indentation of text
                    if (textIndentation > 0)
                    {
                        str.Insert(indentLocation + 1, "\t", textIndentation);
                        indentLocation += textIndentation;
                    }
                    // Indent due to word wrap.
                    if (_indent)
                    {
                        str.Insert(indentLocation + 1, '\t');
                        width += font.getCharSize('\t').w;
                    }

                    _lineWidth.Add(width);
                    _lineHeight.Add(font.getCharSize('\n').h);
                    if (_lang.getTextWrapping() == TextWrapping.WRAP_WORDS)
                    {
                        width = word;
                    }
                    else if (_lang.getTextWrapping() == TextWrapping.WRAP_LETTERS)
                    {
                        width = 0;
                    }
                    start = true;
                }
            }
        }

        _processedText = str.ToString();
        _redraw = true;
    }

    /**
     * Enables/disables high contrast color. Mostly used for
     * Battlescape UI.
     * @param contrast High contrast setting.
     */
    internal void setHighContrast(bool contrast)
    {
        _contrast = contrast;
        _redraw = true;
    }

    /**
     * Changes the way the text is aligned horizontally
     * relative to the drawing area.
     * @param align Horizontal alignment.
     */
    internal void setAlign(TextHAlign align)
    {
        _align = align;
        _redraw = true;
    }

    /**
     * Changes the way the text is aligned vertically
     * relative to the drawing area.
     * @param valign Vertical alignment.
     */
    internal void setVerticalAlign(TextVAlign valign)
    {
        _valign = valign;
        _redraw = true;
    }

    /**
     * Changes the secondary color used to render the text. The text
     * switches between the primary and secondary color whenever there's
     * a 0x01 in the string.
     * @param color Color value.
     */
    internal void setSecondaryColor(byte color)
    {
        _color2 = color;
        _redraw = true;
    }

    internal int getNumLines() =>
	    _wrap ? _lineHeight.Count : 1;

    /**
     * Returns the string displayed on screen.
     * @return Text string.
     */
    internal string getText() =>
	    _text;

    /**
     * Returns the font currently used by the text.
     * @return Pointer to font.
     */
    internal Font getFont() =>
	    _font;

    /**
     * Returns the color used to render the text.
     * @return Color value.
     */
    internal byte getColor() =>
	    _color;

    /**
     * Allows the text to be scrollable via mouse wheel.
     */
    internal void setScrollable(bool scroll) =>
	    _scroll = scroll;

    /**
     * Draws all the characters in the text with a really
     * nasty complex gritty text rendering algorithm logic stuff.
     */
    protected override void draw()
    {
	    base.draw();
	    if (string.IsNullOrEmpty(_text) || _font == null)
	    {
		    return;
	    }

	    // Show text borders for debugging
	    if (Options.debugUi)
	    {
		    SDL_Rect r;
		    r.w = getWidth();
		    r.h = getHeight();
		    r.x = 0;
		    r.y = 0;
		    this.drawRect(ref r, 5);
		    r.w-=2;
		    r.h-=2;
		    r.x++;
		    r.y++;
		    this.drawRect(ref r, 0);
	    }

	    int x = 0, y = 0, line = 0, height = 0;
	    Font font = _font;
	    int color = _color;
	    string s = _processedText;

	    height = getTextHeight();

	    if (_scroll)
	    {
		    y = _scrollY;
	    }
	    else
	    {
		    switch (_valign)
		    {
		        case TextVAlign.ALIGN_TOP:
			        y = 0;
			        break;
		        case TextVAlign.ALIGN_MIDDLE:
			        y = (int)Math.Ceiling((getHeight() - height) / 2.0);
			        break;
		        case TextVAlign.ALIGN_BOTTOM:
			        y = getHeight() - height;
			        break;
		    }
	    }

	    x = getLineX(line);

	    // Set up text color
	    int mul = 1;
	    if (_contrast)
	    {
		    mul = 3;
	    }

	    // Set up text direction
	    int dir = 1;
	    if (_lang.getTextDirection() == TextDirection.DIRECTION_RTL)
	    {
		    dir = -1;
	    }

	    // Invert text by inverting the font palette on index 3 (font palettes use indices 1-5)
	    int mid = _invert ? 3 : 0;

	    // Draw each letter one by one
	    foreach (var c in s)
	    {
		    if (Unicode.isSpace(c) || c == '\t')
		    {
			    x += dir * font.getCharSize(c).w;
		    }
		    else if (Unicode.isLinebreak(c))
		    {
			    line++;
			    y += font.getCharSize(c).h;
			    x = getLineX(line);
			    if (c == Unicode.TOK_NL_SMALL)
			    {
				    font = _small;
			    }
		    }
		    else if (c == Unicode.TOK_COLOR_FLIP)
		    {
			    color = (color == _color ? _color2 : _color);
		    }
		    else
		    {
			    if (dir < 0)
				    x += dir * font.getCharSize(c).w;
			    Surface chr = font.getChar(c);
			    chr.setX(x);
			    chr.setY(y);
			    ShaderDraw(new PaletteShift(), ShaderSurface(this, 0, 0), ShaderCrop(chr), ShaderScalar(color), ShaderScalar(mul), ShaderScalar(mid));
			    if (dir > 0)
				    x += dir * font.getCharSize(c).w;
		    }
	    }
    }

    /**
     * Calculates the starting X position for a line of text.
     * @param line The line number (0 = first, etc).
     * @return The X position in pixels.
     */
    int getLineX(int line)
    {
	    int x = 0;
	    switch (_lang.getTextDirection())
	    {
	        case TextDirection.DIRECTION_LTR:
		        switch (_align)
		        {
		            case TextHAlign.ALIGN_LEFT:
			            break;
		            case TextHAlign.ALIGN_CENTER:
			            x = (int)Math.Ceiling((getWidth() + _font.getSpacing() - _lineWidth[line]) / 2.0);
			            break;
		            case TextHAlign.ALIGN_RIGHT:
			            x = getWidth() - 1 - _lineWidth[line];
			            break;
		        }
		        break;
	        case TextDirection.DIRECTION_RTL:
		        switch (_align)
		        {
		            case TextHAlign.ALIGN_LEFT:
			            x = getWidth() - 1;
			            break;
		            case TextHAlign.ALIGN_CENTER:
			            x = getWidth() - (int)Math.Ceiling((getWidth() + _font.getSpacing() - _lineWidth[line]) / 2.0);
			            break;
		            case TextHAlign.ALIGN_RIGHT:
			            x = _lineWidth[line];
			            break;
		        }
		        break;
	    }
	    return x;
    }

    /**
     * Enables/disables color inverting. Mostly used to make
     * button text look pressed along with the button.
     * @param invert Invert setting.
     */
    internal void setInvert(bool invert)
    {
	    _invert = invert;
	    _redraw = true;
    }

    /**
     * Returns the way the text is aligned horizontally
     * relative to the drawing area.
     * @return Horizontal alignment.
     */
    internal TextHAlign getAlign() =>
	    _align;

    /**
     * Returns the way the text is aligned vertically
     * relative to the drawing area.
     * @return Horizontal alignment.
     */
    internal TextVAlign getVerticalAlign() =>
	    _valign;

    /**
     * Handles scrolling.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void mousePress(Action action, State state)
    {
	    base.mousePress(action, state);
	    if (_scroll &&
		    (action.getDetails().wheel.y > 0 || //button.button == SDL_BUTTON_WHEELUP
		    action.getDetails().wheel.y < 0)) //button.button == SDL_BUTTON_WHEELDOWN
	    {
		    int scrollArea = getHeight() - getTextHeight();
		    if (scrollArea < 0)
		    {
			    int scrollAmount = _font.getHeight() + _font.getSpacing();
			    if (action.getDetails().wheel.y < 0) //button.button == SDL_BUTTON_WHEELDOWN
				    scrollAmount = -scrollAmount;

			    _scrollY = Math.Clamp(_scrollY + scrollAmount, scrollArea, 0);
			    _redraw = true;
		    }
	    }
    }
}
