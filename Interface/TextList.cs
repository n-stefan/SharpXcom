﻿/*
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
    internal TextList(int width, int height, int x = 0, int y = 0) : base(width, height, x, y)
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
    internal void setScrolling(bool scrolling, int scrollPos = 4)
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
    internal override void setColor(byte color)
    {
        _color = color;
        _up.setColor(color);
        _down.setColor(color);
        _scrollbar.setColor(color);
        foreach (var u in _texts)
        {
            foreach (var v in u)
            {
                v.setColor(color);
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
            txt.setPalette(this.getPalette());
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
                        w = (uint)(w + _font.getChar('.').getCrop().w + _font.getSpacing());
                        buf += '.';
                    }
                    if (_align[i] != TextHAlign.ALIGN_LEFT)
                    {
                        w = (uint)(w + _font.getChar('.').getCrop().w + _font.getSpacing());
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
                rowX = (int)(rowX + _columns[i]);
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
            a1.setPalette(this.getPalette());
            a1.setColor(_up.getColor());
            a1.onMouseClick(_leftClick, 0);
            a1.onMousePress(_leftPress);
            a1.onMouseRelease(_leftRelease);
            _arrowLeft.Add(a1);
            ArrowButton a2 = new ArrowButton(shape2, 11, 8, getX() + _arrowPos + 12, getY());
            a2.setListButton();
            a2.setPalette(this.getPalette());
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

    /**
     * Changes the horizontal margin placed around the text.
     * @param margin Margin in pixels.
     */
    internal void setMargin(int margin) =>
        _margin = margin;

    /**
     * Returns the currently selected row if the text
     * list is selectable.
     * @return Selected row, -1 if none.
     */
    internal uint getSelectedRow()
    {
	    if (!_rows.Any() || _selRow >= _rows.Count)
	    {
            unchecked { return (uint)-1; }
        }
        else
	    {
		    return _rows[(int)_selRow];
	    }
    }

    /**
     * Returns the secondary color of the text in the list.
     * @return Color value.
     */
    internal byte getSecondaryColor() =>
	    _color2;

    /**
     * Changes the text color of a whole row in the list.
     * @param row Row number.
     * @param color Text color.
     */
    internal void setRowColor(uint row, byte color)
    {
        foreach (var text in _texts[(int)row])
        {
            text.setColor(color);
        }
        _redraw = true;
    }

    /**
     * Changes the color of the arrow buttons in the list.
     * @param color Color value.
     */
    internal void setArrowColor(byte color)
    {
        _up.setColor(color);
        _down.setColor(color);
        _scrollbar.setColor(color);
    }

    /**
     * If enabled, the text in different columns will be separated by dots.
     * Otherwise, it will only be separated by blank space.
     * @param dot True for dots, False for spaces.
     */
    internal void setDot(bool dot) =>
        _dot = dot;

    /**
     * Sets the position of the column of arrow buttons
     * in the text list.
     * @param pos X in pixels (-1 to disable).
     * @param type Arrow orientation type.
     */
    internal void setArrowColumn(int pos, ArrowOrientation type)
    {
        _arrowPos = pos;
        _arrowType = type;
        _arrowsLeftEdge = getX() + _arrowPos;
        _arrowsRightEdge = _arrowsLeftEdge + 12 + 11;
    }

    /**
     * Sets a function to be called every time the left arrows are mouse pressed.
     * @param handler Action handler.
     */
    internal void onLeftArrowPress(ActionHandler handler)
    {
        _leftPress = handler;
        foreach (var i in _arrowLeft)
        {
            i.onMousePress(handler);
        }
    }

    /**
     * Sets a function to be called every time the left arrows are mouse released.
     * @param handler Action handler.
     */
    internal void onLeftArrowRelease(ActionHandler handler)
    {
        _leftRelease = handler;
        foreach (var i in _arrowLeft)
        {
            i.onMouseRelease(handler);
        }
    }

    /**
     * Sets a function to be called every time the left arrows are mouse clicked.
     * @param handler Action handler.
     */
    internal void onLeftArrowClick(ActionHandler handler)
    {
        _leftClick = handler;
        foreach (var i in _arrowLeft)
        {
            i.onMouseClick(handler, 0);
        }
    }

    /**
     * Sets a function to be called every time the right arrows are mouse pressed.
     * @param handler Action handler.
     */
    internal void onRightArrowPress(ActionHandler handler)
    {
        _rightPress = handler;
        foreach (var i in _arrowRight)
        {
            i.onMousePress(handler);
        }
    }

    /**
     * Sets a function to be called every time the right arrows are mouse released.
     * @param handler Action handler.
     */
    internal void onRightArrowRelease(ActionHandler handler)
    {
        _rightRelease = handler;
        foreach (var i in _arrowRight)
        {
            i.onMouseRelease(handler);
        }
    }

    /**
     * Sets a function to be called every time the right arrows are mouse clicked.
     * @param handler Action handler.
     */
    internal void onRightArrowClick(ActionHandler handler)
    {
        _rightClick = handler;
        foreach (var i in _arrowRight)
        {
            i.onMouseClick(handler, 0);
        }
    }

    /**
     * Gets the arrowsLeftEdge.
     * @return arrowsLeftEdge.
     */
    internal int getArrowsLeftEdge() =>
        _arrowsLeftEdge;

    /**
     * Gets the arrowsRightEdge.
     * @return arrowsRightEdge.
     */
    internal int getArrowsRightEdge() =>
        _arrowsRightEdge;

    /**
     * Changes the text of a specific Text object in the list.
     * @param row Row number.
     * @param column Column number.
     * @param text Text string.
     */
    internal void setCellText(uint row, uint column, string text)
    {
	    _texts[(int)row][(int)column].setText(text);
	    _redraw = true;
    }

    /**
     * Returns the color of the text in the list.
     * @return Color value.
     */
    internal byte getColor() =>
	    _color;

    /**
     * Enables/disables text wordwrapping. When enabled, rows can
     * take up multiple lines of the list, otherwise every row
     * is restricted to one line.
     * @param wrap Wordwrapping setting.
     */
    internal void setWordWrap(bool wrap) =>
        _wrap = wrap;

    /**
     * Changes the color of a specific Text object in the list.
     * @param row Row number.
     * @param column Column number.
     * @param color Text color.
     */
    internal void setCellColor(uint row, uint column, byte color)
    {
        _texts[(int)row][(int)column].setColor(color);
        _redraw = true;
    }

    internal int getScrollbarColor() =>
        _scrollbar.getColor();

    /*
     * get the scroll depth.
     * @return scroll depth.
     */
    internal uint getScroll() =>
        _scroll;

    /**
     * Returns the amount of physical rows stored in the list.
     * @return Number of rows.
     */
    internal uint getRows() =>
        (uint)_rows.Count;

    /**
     * Changes the text list to use the big-size font.
     */
    internal void setBig()
    {
        _font = _big;

        _selector = null;
        _selector = new Surface(getWidth(), _font.getHeight() + _font.getSpacing(), getX(), getY());
        _selector.setPalette(getPalette());
        _selector.setVisible(false);

        updateVisible();
    }

    /**
     * Updates the amount of visible rows according to the
     * current list and font size.
     */
    void updateVisible()
    {
        _visibleRows = 0;
        for (int y = 0; y < getHeight(); y += _font.getHeight() + _font.getSpacing())
        {
            _visibleRows++;
        }
        updateArrows();
    }

    internal void setFlooding(bool flooding) =>
        _flooding = flooding;

    /**
     * Returns the height of a specific text row in the list.
     * @param row Row number.
     * @return height in pixels.
     */
    internal int getNumTextLines(uint row) =>
	    _texts[(int)row].First().getNumLines();

    /**
     * Returns the height of a specific text row in the list.
     * @param row Row number.
     * @return height in pixels.
     */
    internal int getTextHeight(uint row) =>
	    _texts[(int)row].First().getTextHeight();

    /**
     * Returns the Y position of a specific text row in the list.
     * @param row Row number.
     * @return Y position in pixels.
     */
    internal int getRowY(uint row) =>
	    getY() + _texts[(int)row][0].getY();

    /**
     * Passes ticks to arrow buttons.
     */
    internal override void think()
    {
	    base.think();
	    _up.think();
	    _down.think();
	    _scrollbar.think();
	    foreach (var i in _arrowLeft)
	    {
		    i.think();
	    }
	    foreach (var i in _arrowRight)
	    {
		    i.think();
	    }
    }

    /**
     * Passes events to arrow buttons.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    internal override void handle(Action action, State state)
    {
	    base.handle(action, state);
	    _up.handle(action, state);
	    _down.handle(action, state);
	    _scrollbar.handle(action, state);
	    if (_arrowPos != -1 && _rows.Any())
	    {
		    uint startArrowIdx = _rows[(int)_scroll];
		    if (0 < _scroll && _rows[(int)_scroll] == _rows[(int)(_scroll - 1)])
		    {
			    // arrows for first partially-visible line of text are off-screen; don't process them
			    ++startArrowIdx;
		    }
		    uint endArrowIdx = _rows[(int)_scroll] + 1;
		    uint endRow = Math.Min((uint)_rows.Count, _scroll + _visibleRows);
		    for (int i = (int)(_scroll + 1); i < endRow; ++i)
		    {
			    if (_rows[i] != _rows[i - 1])
			    {
				    ++endArrowIdx;
			    }
		    }
		    for (int i = (int)startArrowIdx; i < endArrowIdx; ++i)
		    {
			    _arrowLeft[i].handle(action, state);
			    _arrowRight[i].handle(action, state);
		    }
	    }
    }

    /**
     * Returns the X position of a specific text column in the list.
     * @param column Column number.
     * @return X position in pixels.
     */
    internal int getColumnX(uint column) =>
	    getX() + _texts[0][(int)column].getX();

    /**
     * Gets the combobox that this list is attached to, if any.
     * @return the attached combobox.
     */
    internal ComboBox getComboBox() =>
	    _comboBox;

    /**
     * Draws the text list and all the text contained within.
     */
    internal override void draw()
    {
	    base.draw();
	    int y = 0;
	    if (_rows.Any())
	    {
		    // for wrapped items, offset the draw height above the visible surface
		    // so that the correct row appears at the top
		    for (var row = (int)_scroll; row > 0 && _rows[row] == _rows[row - 1]; --row)
		    {
			    y -= _font.getHeight() + _font.getSpacing();
		    }
		    for (var i = (int)_rows[(int)_scroll]; i < _texts.Count && i < _rows[(int)_scroll] + _visibleRows; ++i)
		    {
			    foreach (var j in _texts[i])
			    {
				    j.setY(y);
				    j.blit(this);
			    }
			    if (_texts[i].Any())
			    {
				    y += _texts[i].First().getHeight() + _font.getSpacing();
			    }
			    else
			    {
				    y += _font.getHeight() + _font.getSpacing();
			    }
		    }
	    }
    }

    /**
     * Blits the text list and selector.
     * @param surface Pointer to surface to blit onto.
     */
    internal override void blit(Surface surface)
    {
	    if (_visible && !_hidden)
	    {
		    _selector.blit(surface);
	    }
	    base.blit(surface);
	    if (_visible && !_hidden)
	    {
		    if (_arrowPos != -1 && _rows.Any())
		    {
			    int y = getY();
			    for (var row = (int)_scroll; row > 0 && _rows[row] == _rows[row - 1]; --row)
			    {
				    y -= _font.getHeight() + _font.getSpacing();
			    }
			    int maxY = getY() + getHeight();
			    for (var i = (int)_rows[(int)_scroll]; i < _texts.Count && i < _rows[(int)_scroll] + _visibleRows && y < maxY; ++i)
			    {
				    _arrowLeft[i].setY(y);
				    _arrowRight[i].setY(y);

				    if (y >= getY())
				    {
					    // only blit arrows that belong to texts that have their first row on-screen
					    _arrowLeft[i].blit(surface);
					    _arrowRight[i].blit(surface);
				    }

				    if (_texts[i].Any())
				    {
					    y += _texts[i].First().getHeight() + _font.getSpacing();
				    }
				    else
				    {
					    y += _font.getHeight() + _font.getSpacing();
				    }
			    }
		    }
		    _up.blit(surface);
		    _down.blit(surface);
		    _scrollbar.blit(surface);
	    }
    }

    /**
     * Unpresses all the arrow buttons.
     * @param state Pointer to running state.
     */
    internal override void unpress(State state)
    {
	    base.unpress(state);
	    foreach (var i in _arrowLeft)
	    {
		    i.unpress(state);
	    }
	    foreach (var i in _arrowRight)
	    {
		    i.unpress(state);
	    }
    }

    /**
     * Ignores any mouse clicks that aren't on a row.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    internal override void mousePress(Action action, State state)
    {
	    bool allowScroll = true;
	    if (Options.changeValueByMouseWheel != 0)
	    {
		    allowScroll = (action.getAbsoluteXMouse() < _arrowsLeftEdge || action.getAbsoluteXMouse() > _arrowsRightEdge);
	    }
	    if (allowScroll)
	    {
		    if (action.getDetails().wheel.y > 0) scrollUp(false, true); //button.button == SDL_BUTTON_WHEELUP
		    else if (action.getDetails().wheel.y < 0) scrollDown(false, true); //button.button == SDL_BUTTON_WHEELDOWN
	    }
	    if (_selectable)
	    {
		    if (_selRow < _rows.Count)
		    {
			    base.mousePress(action, state);
		    }
	    }
	    else
	    {
		    base.mousePress(action, state);
	    }
    }

    /*
     * Ignores any mouse clicks that aren't on a row.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void mouseRelease(Action action, State state)
    {
	    if (_selectable)
	    {
		    if (_selRow < _rows.Count)
		    {
			    base.mouseRelease(action, state);
		    }
	    }
	    else
	    {
		    base.mouseRelease(action, state);
	    }
    }

    /**
     * Ignores any mouse clicks that aren't on a row.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void mouseClick(Action action, State state)
    {
	    if (_selectable)
	    {
		    if (_selRow < _rows.Count)
		    {
			    base.mouseClick(action, state);
			    if (_comboBox != null && action.getDetails().button.button == SDL_BUTTON_LEFT)
			    {
				    _comboBox.setSelected(_selRow);
				    _comboBox.toggle();
			    }
		    }
	    }
	    else
	    {
		    base.mouseClick(action, state);
	    }
    }

    /**
     * Selects the row the mouse is over.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void mouseOver(Action action, State state)
    {
	    if (_selectable)
	    {
		    int rowHeight = _font.getHeight() + _font.getSpacing(); //theoretical line height
		    _selRow = (uint)Math.Max(0, (int)(_scroll + (int)Math.Floor(action.getRelativeYMouse() / (rowHeight * action.getYScale()))));
		    if (_selRow < _rows.Count)
		    {
			    Text selText = _texts[(int)_rows[(int)_selRow]].First();
			    int y = getY() + selText.getY();
			    int actualHeight = selText.getHeight() + _font.getSpacing(); //current line height
			    if (y < getY() || y + actualHeight > getY() + getHeight())
			    {
				    actualHeight /= 2;
			    }
			    if (y < getY())
			    {
				    y = getY();
			    }
			    if (_selector.getHeight() != actualHeight)
			    {
				    // resizing doesn't work, but recreating does, so let's do that!
				    _selector = null;
				    _selector = new Surface(getWidth(), actualHeight, getX(), y);
				    _selector.setPalette(getPalette());
			    }
			    _selector.setY(y);
			    _selector.copy(_bg);
			    if (_contrast)
			    {
				    _selector.offsetBlock(-5);
			    }
			    else if (_comboBox != null)
			    {
				    _selector.offset(+1, Palette.backPos);
			    }
			    else
			    {
				    _selector.offsetBlock(-10);
			    }
			    _selector.setVisible(true);
		    }
		    else
		    {
			    _selector.setVisible(false);
		    }
	    }

	    base.mouseOver(action, state);
    }

    /**
     * Deselects the row.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void mouseOut(Action action, State state)
    {
	    if (_selectable)
	    {
		    _selector.setVisible(false);
	    }

	    base.mouseOut(action, state);
    }

    /**
     * Changes the resources for the text in the list
     * and calculates the selector and visible amount of rows.
     * @param big Pointer to large-size font.
     * @param small Pointer to small-size font.
     * @param lang Pointer to current language.
     */
    internal override void initText(Font big, Font small, Language lang)
    {
	    _big = big;
	    _small = small;
	    _font = small;
	    _lang = lang;

	    _selector = null;
	    _selector = new Surface(getWidth(), _font.getHeight() + _font.getSpacing(), getX(), getY());
	    _selector.setPalette(getPalette());
	    _selector.setVisible(false);

	    updateVisible();
    }

    /**
     * Replaces a certain amount of colors in the palette of all
     * the text contained in the list.
     * @param colors Pointer to the set of colors.
     * @param firstcolor Offset of the first color to replace.
     * @param ncolors Amount of colors to replace.
     */
    internal override void setPalette(SDL_Color[] colors, int firstcolor = 0, int ncolors = 256)
    {
	    base.setPalette(colors, firstcolor, ncolors);
	    foreach (var u in _texts)
	    {
		    foreach (var v in u)
		    {
			    v.setPalette(colors, firstcolor, ncolors);
		    }
	    }
	    foreach (var i in _arrowLeft)
	    {
		    i.setPalette(colors, firstcolor, ncolors);
	    }
	    foreach (var i in _arrowRight)
	    {
		    i.setPalette(colors, firstcolor, ncolors);
	    }
	    if (_selector != null)
	    {
		    _selector.setPalette(colors, firstcolor, ncolors);
	    }
	    _up.setPalette(colors, firstcolor, ncolors);
	    _down.setPalette(colors, firstcolor, ncolors);
	    _scrollbar.setPalette(colors, firstcolor, ncolors);
    }

    /**
     * Changes the position of the surface in the X axis.
     * @param x X position in pixels.
     */
    internal override void setX(int x)
    {
	    base.setX(x);
	    _up.setX(getX() + getWidth() + _scrollPos);
	    _down.setX(getX() + getWidth() + _scrollPos);
	    _scrollbar.setX(getX() + getWidth() + _scrollPos);
	    if (_selector != null)
		    _selector.setX(getX());
    }

    /**
     * Changes the position of the surface in the Y axis.
     * @param y Y position in pixels.
     */
    internal override void setY(int y)
    {
	    base.setY(y);
	    _up.setY(getY());
	    _down.setY(getY() + getHeight() - 14);
	    _scrollbar.setY(_up.getY() + _up.getHeight());
	    if (_selector != null)
		    _selector.setY(getY());
    }

    /**
     * Changes the height of the text list.
     * @param height New height in pixels.
     */
    internal override void setHeight(int height)
    {
	    base.setHeight(height);
	    setY(getY());
	    int h = Math.Max(_down.getY() - _up.getY() - _up.getHeight(), 1);
	    _scrollbar.setHeight(h);
	    updateVisible();
    }

    /**
     * Changes the secondary color of the text in the list.
     * @param color Color value.
     */
    internal override void setSecondaryColor(byte color) =>
	    _color2 = color;

    internal override void setBorderColor(byte color)
    {
	    _up.setColor(color);
	    _down.setColor(color);
	    _scrollbar.setColor(color);
    }

    /**
     * Enables/disables high contrast color. Mostly used for
     * Battlescape text.
     * @param contrast High contrast setting.
     */
    internal override void setHighContrast(bool contrast)
    {
	    _contrast = contrast;
	    foreach (var u in _texts)
	    {
		    foreach (var v in u)
		    {
			    v.setHighContrast(contrast);
		    }
	    }
	    _scrollbar.setHighContrast(contrast);
    }

    /**
     * Changes the text list to use the small-size font.
     */
    void setSmall()
    {
	    _font = _small;

	    _selector = null;
	    _selector = new Surface(getWidth(), _font.getHeight() + _font.getSpacing(), getX(), getY());
	    _selector.setPalette(getPalette());
	    _selector.setVisible(false);

	    updateVisible();
    }

    /**
     * If enabled, the columns will match the text width.
     * Otherwise, they will have a fixed width.
     * @param condensed True for condensed layout, False for table layout.
     */
    void setCondensed(bool condensed) =>
	    _condensed = condensed;

    /**
     * Returns the margin of the text in the list.
     * @return Margin in pixels.
     */
    int getMargin() =>
	    _margin;
}
