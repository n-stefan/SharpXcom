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

enum ArrowOrientation { ARROW_VERTICAL, ARROW_HORIZONTAL };

/**
 * List of Text's split into columns.
 * Contains a set of Text's that are automatically lined up by
 * rows and columns, like a big table, making it easy to manage
 * them together.
 */
internal class TextList : InteractiveSurface
{
    Font _big, _small, _font;
    uint _scroll, _visibleRows, _selRow;
    byte _color, _color2;
    bool _dot, _selectable, _condensed, _contrast, _wrap, _flooding;
    Surface _bg, _selector;
    int _margin;
    bool _scrolling;
    int _arrowPos, _scrollPos;
    ArrowOrientation _arrowType;
    ActionHandler _leftClick, _leftPress, _leftRelease, _rightClick, _rightPress, _rightRelease;
    int _arrowsLeftEdge, _arrowsRightEdge;
    ComboBox _comboBox;
    ArrowButton _up, _down;
    ScrollBar _scrollbar;
    List<List<Text>> _texts;
    List<ArrowButton> _arrowLeft, _arrowRight;
    List<uint> _columns, _rows;
    Dictionary<int, TextHAlign> _align;
    Language _lang;

    /**
     * Sets up a blank list with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal TextList(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _big = null;
        _small = null;
        _font = null;
        _scroll = 0;
        _visibleRows = 0;
        _selRow = 0;
        _color = 0;
        _dot = false;
        _selectable = false;
        _condensed = false;
        _contrast = false;
        _wrap = false;
        _flooding = false;
        _bg = null;
        _selector = null;
        _margin = 0;
        _scrolling = true;
        _arrowPos = -1;
        _scrollPos = 4;
        _arrowType = ArrowOrientation.ARROW_VERTICAL;
        _leftClick = null;
        _leftPress = null;
        _leftRelease = null;
        _rightClick = null;
        _rightPress = null;
        _rightRelease = null;
        _arrowsLeftEdge = 0;
        _arrowsRightEdge = 0;
        _comboBox = null;

        _up = new ArrowButton(ArrowShape.ARROW_BIG_UP, 13, 14, getX() + getWidth() + _scrollPos, getY());
        _up.setVisible(false);
        _up.setTextList(this);
        _down = new ArrowButton(ArrowShape.ARROW_BIG_DOWN, 13, 14, getX() + getWidth() + _scrollPos, getY() + getHeight() - 14);
        _down.setVisible(false);
        _down.setTextList(this);
        int h = Math.Max(_down.getY() - _up.getY() - _up.getHeight(), 1);
        _scrollbar = new ScrollBar(_up.getWidth(), h, getX() + getWidth() + _scrollPos, _up.getY() + _up.getHeight());
        _scrollbar.setVisible(false);
        _scrollbar.setTextList(this);
    }

    /**
     * Deletes all the stuff contained in the list.
     */
    ~TextList()
    {
        _texts.Clear();
        _arrowLeft.Clear();
        _arrowRight.Clear();
        _selector = null;
        _up = null;
        _down = null;
        _scrollbar = null;
    }

    /*
     * set the scroll depth.
     * @param scroll set the scroll depth to this.
     */
    internal void scrollTo(uint scroll)
    {
        if (!_scrolling)
            return;
        _scroll = (uint)Math.Clamp(scroll, 0, _rows.Count - _visibleRows);
        draw(); // can't just set _redraw here because reasons
        updateArrows();
    }

    /**
     * Scrolls the text in the list up by one row or to the top.
     * @param toMax If true then scrolls to the top of the list. false => one row up
     * @param scrollByWheel If true then use wheel scroll, otherwise scroll normally.
     */
    internal void scrollUp(bool toMax, bool scrollByWheel = false)
    {
        if (!_scrolling)
            return;
        if (_rows.Count > _visibleRows && _scroll > 0)
        {
            if (toMax)
            {
                scrollTo(0);
            }
            else
            {
                if (scrollByWheel)
                {
                    scrollTo(_scroll - Math.Min((uint)(Options.mousewheelSpeed), _scroll));
                }
                else
                {
                    scrollTo(_scroll - 1);
                }
            }
        }
    }

    /**
     * Scrolls the text in the list down by one row or to the bottom.
     * @param toMax If true then scrolls to the bottom of the list. false => one row down
     * @param scrollByWheel If true then use wheel scroll, otherwise scroll normally.
     */
    internal void scrollDown(bool toMax, bool scrollByWheel = false)
    {
        if (!_scrolling)
            return;
        if (_rows.Count > _visibleRows && _scroll < _rows.Count - _visibleRows)
        {
            if (toMax)
            {
                scrollTo((uint)(_rows.Count - _visibleRows));
            }
            else
            {
                if (scrollByWheel)
                {
                    scrollTo((uint)(_scroll + Options.mousewheelSpeed));
                }
                else
                {
                    scrollTo(_scroll + 1);
                }
            }
        }
    }

    /**
     * Hooks up the button to work as part of an existing combobox,
     * updating the selection when it's pressed.
     * @param comboBox Pointer to combobox.
     */
    internal void setComboBox(ComboBox comboBox) =>
        _comboBox = comboBox;

    /**
     * Changes the columns that the list contains.
     * While rows can be unlimited, columns need to be specified
     * since they can have various widths for lining up the text.
     * @param cols Number of columns.
     * @param ... Width of each column.
     */
    internal void setColumns(int cols, params int[] args)
    {
        for (int i = 0; i < cols; ++i)
        {
            _columns.Add((uint)args[i]);
        }
    }

    /**
     * If enabled, the list will respond to player input,
     * highlighting selected rows and receiving clicks.
     * @param selectable Selectable setting.
     */
    internal void setSelectable(bool selectable) =>
        _selectable = selectable;

    /**
     * Changes the surface used to draw the background of the selector.
     * @param bg New background.
     */
    internal void setBackground(Surface bg)
    {
        _bg = bg;
        _scrollbar.setBackground(_bg);
    }

    /**
     * Changes the horizontal alignment of the text in the list. This doesn't change
     * the alignment of existing text, just the alignment of text added from then on.
     * @param align Horizontal alignment.
     * @param col the column to set the alignment for (defaults to -1, meaning "all")
     */
    internal void setAlign(TextHAlign align, int col = -1)
    {
        if (col == -1)
        {
            for (var i = 0; i < _columns.Count; ++i)
            {
                _align[i] = align;
            }
        }
        else
        {
            _align[col] = align;
        }
    }

    /**
     * Changes whether the list can be scrolled.
     * @param scrolling True to allow scrolling, false otherwise.
     * @param scrollPos Custom X position for the scroll buttons.
     */
    internal void setScrolling(bool scrolling, int scrollPos)
    {
        _scrolling = scrolling;
        if (scrollPos != _scrollPos)
        {
            _scrollPos = scrollPos;
            _up.setX(getX() + getWidth() + _scrollPos);
            _down.setX(getX() + getWidth() + _scrollPos);
            _scrollbar.setX(getX() + getWidth() + _scrollPos);
        }
    }

    /**
     * Returns the amount of visible rows stored in the list.
     * @return Number of rows.
     */
    internal uint getVisibleRows() =>
	    _visibleRows;

    /**
     * Updates the visibility of the arrow buttons according to
     * the current scroll position.
     */
    void updateArrows()
    {
        _up.setVisible(_rows.Count > _visibleRows /*&& _scroll > 0*/);
        _down.setVisible(_rows.Count > _visibleRows /*&& _scroll < _rows.size() - _visibleRows*/);
        _scrollbar.setVisible(_rows.Count > _visibleRows);
        _scrollbar.invalidate();
        _scrollbar.blit(this);
    }

    /**
     * Changes the color of the text in the list. This doesn't change
     * the color of existing text, just the color of text added from then on.
     * @param color Color value.
     */
    internal void setColor(byte color)
    {
        _color = color;
        _up.setColor(color);
        _down.setColor(color);
        _scrollbar.setColor(color);
        foreach (var text in _texts)
        {
            foreach (var t in text)
            {
                t.setColor(color);
            }
        }
    }

    /**
     * Adds a new row of text to the list, automatically creating
     * the required Text objects lined up where they need to be.
     * @param cols Number of columns.
     * @param ... Text for each cell in the new row.
     */
    internal void addRow(int cols, params string[] args)
    {
        int ncols;
        if (cols > 0)
        {
            ncols = cols;
        }
        else
        {
            ncols = 1;
        }

        var temp = new List<Text>();
        // Positions are relative to list surface.
        int rowX = 0, rowY = 0, rows = 1, rowHeight = 0;
        if (_texts.Any())
        {
            rowY = _texts.Last().First().getY() + _texts.Last().First().getHeight() + _font.getSpacing();
        }

        for (int i = 0; i < ncols; ++i)
        {
            int width;
            // Place text
            if (_flooding)
            {
                width = 340;
            }
            else
            {
                width = (int)_columns[i];
            }
            Text txt = new Text(width, _font.getHeight(), _margin + rowX, rowY);
            txt.setPalette(this.getPaletteColors());
            txt.initText(_big, _small, _lang);
            txt.setColor(_color);
            txt.setSecondaryColor(_color2);
            if (_align.ContainsKey(i))
            {
                txt.setAlign(_align[i]);
            }
            txt.setHighContrast(_contrast);
            if (_font == _big)
            {
                txt.setBig();
            }
            else
            {
                txt.setSmall();
            }
            if (cols > 0)
                txt.setText(args[i]);
            // grab this before we enable word wrapping so we can use it to calculate
            // the total row height below
            int vmargin = _font.getHeight() - txt.getTextHeight();
            // Wordwrap text if necessary
            if (_wrap && txt.getTextWidth() > txt.getWidth())
            {
                txt.setWordWrap(true, true);
                rows = Math.Max(rows, txt.getNumLines());
            }
            rowHeight = Math.Max(rowHeight, txt.getTextHeight() + vmargin);

            // Places dots between text
            if (_dot && i < cols - 1)
            {
                string buf = txt.getText();
                uint w = (uint)txt.getTextWidth();
                while (w < _columns[i])
                {
                    if (_align[i] != TextHAlign.ALIGN_RIGHT)
                    {
                        w += (uint)(_font.getChar('.').getCrop().w + _font.getSpacing());
                        buf += '.';
                    }
                    if (_align[i] != TextHAlign.ALIGN_LEFT)
                    {
                        w += (uint)(_font.getChar('.').getCrop().w + _font.getSpacing());
                        buf.Insert(0, ".");
                    }
                }
                txt.setText(buf);
            }

            temp.Add(txt);
            if (_condensed)
            {
                rowX += txt.getTextWidth();
            }
            else
            {
                rowX += (int)_columns[i];
            }
        }

        // ensure all elements in this row are the same height
        for (int i = 0; i < cols; ++i)
        {
            temp[i].setHeight(rowHeight);
        }

        _texts.Add(temp);
        for (int i = 0; i < rows; ++i)
        {
            _rows.Add((uint)(_texts.Count - 1));
        }

        // Place arrow buttons
        // Position defined w.r.t. main window, NOT TextList.
        if (_arrowPos != -1)
        {
            ArrowShape shape1, shape2;
            if (_arrowType == ArrowOrientation.ARROW_VERTICAL)
            {
                shape1 = ArrowShape.ARROW_SMALL_UP;
                shape2 = ArrowShape.ARROW_SMALL_DOWN;
            }
            else
            {
                shape1 = ArrowShape.ARROW_SMALL_LEFT;
                shape2 = ArrowShape.ARROW_SMALL_RIGHT;
            }
            ArrowButton a1 = new ArrowButton(shape1, 11, 8, getX() + _arrowPos, getY());
            a1.setListButton();
            a1.setPalette(this.getPaletteColors());
            a1.setColor(_up.getColor());
            a1.onMouseClick(_leftClick, 0);
            a1.onMousePress(_leftPress);
            a1.onMouseRelease(_leftRelease);
            _arrowLeft.Add(a1);
            ArrowButton a2 = new ArrowButton(shape2, 11, 8, getX() + _arrowPos + 12, getY());
            a2.setListButton();
            a2.setPalette(this.getPaletteColors());
            a2.setColor(_up.getColor());
            a2.onMouseClick(_rightClick, 0);
            a2.onMousePress(_rightPress);
            a2.onMouseRelease(_rightRelease);
            _arrowRight.Add(a2);
        }

        _redraw = true;
        updateArrows();
    }

    /**
     * Returns the amount of text rows stored in the list.
     * @return Number of rows.
     */
    internal uint getTexts() =>
        (uint)_texts.Count;

    /**
     * Returns the text of a specific Text object in the list.
     * @param row Row number.
     * @param column Column number.
     * @return Text string.
     */
    internal string getCellText(uint row, uint column) =>
        _texts[(int)row][(int)column].getText();

    /**
     * Removes all the rows currently stored in the list.
     */
    internal void clearList()
    {
        _texts.Clear();
        scrollUp(true, false);
        _rows.Clear();
        _redraw = true;
    }
}
