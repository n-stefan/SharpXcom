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

/**
 * Text button with a list dropdown when pressed.
 * Allows selection from multiple available options.
 */
internal class ComboBox : InteractiveSurface
{
    const int HORIZONTAL_MARGIN = 2;
    const int VERTICAL_MARGIN = 3;
    const int MAX_ITEMS = 10;
    const int BUTTON_WIDTH = 14;
    const int TEXT_HEIGHT = 8;

    ActionHandler _change;
    uint _sel;
    State _state;
    Language _lang;
    bool _toggled;
    bool _popupAboveButton;
    TextButton _button;
    Surface _arrow;
    Window _window;
    TextList _list;
    byte _color;

    /**
     * Sets up a combobox with the specified size and position.
     * @param state Pointer to state the combobox belongs to.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal ComboBox(State state, int width, int height, int x = 0, int y = 0, bool popupAboveButton = false) : base(width, height, x, y)
    {
        _change = null;
        _sel = 0;
        _state = state;
        _lang = null;
        _toggled = false;
        _popupAboveButton = popupAboveButton;

        _button = new TextButton(width, height, x, y);
        _button.setComboBox(this);

        _arrow = new Surface(11, 8, x + width - BUTTON_WIDTH, y + 4);

        int popupHeight = MAX_ITEMS * TEXT_HEIGHT + VERTICAL_MARGIN * 2;
        int popupY = getPopupWindowY(height, y, popupHeight, popupAboveButton);
        _window = new Window(state, width, popupHeight, x, popupY);
        _window.setThinBorder();

        _list = new TextList(width - HORIZONTAL_MARGIN * 2 - BUTTON_WIDTH + 1,
                            popupHeight - (VERTICAL_MARGIN * 2 + 2),
                            x + HORIZONTAL_MARGIN,
                            popupY + VERTICAL_MARGIN);
        _list.setComboBox(this);
        _list.setColumns(1, _list.getWidth());
        _list.setSelectable(true);
        _list.setBackground(_window);
        _list.setAlign(TextHAlign.ALIGN_CENTER);
        _list.setScrolling(true, 0);

        toggle(true);
    }

    /**
     * Deletes all the stuff contained in the list.
     */
    ~ComboBox()
    {
        _button = null;
        _arrow = null;
        _window = null;
        _list = null;
    }

    static int getPopupWindowY(int buttonHeight, int buttonY, int popupHeight, bool popupAboveButton)
    {
        int belowButtonY = buttonY + buttonHeight;
        if (popupAboveButton)
        {
            // used when popup list won't fit below the button; display it above
            return buttonY - popupHeight;
        }
        return belowButtonY;
    }

    /**
     * Opens/closes the combo box list.
     * @param first Is it the initialization toggle?
     */
    internal void toggle(bool first = false)
    {
        _window.setVisible(!_window.getVisible());
        _list.setVisible(!_list.getVisible());
        _state.setModal(_window.getVisible() ? this : null);
        if (!first && !_window.getVisible())
        {
            _toggled = true;
        }
        if (_list.getVisible())
        {
            if (_sel < _list.getVisibleRows() / 2)
            {
                _list.scrollTo(0);
            }
            else
            {
                _list.scrollTo(_sel - _list.getVisibleRows() / 2);
            }
        }
    }

    /**
     * Changes the color used to draw the combo box.
     * @param color Color value.
     */
    internal override void setColor(byte color)
    {
        _color = color;
        drawArrow();
        _button.setColor(_color);
        _window.setColor(_color);
        _list.setColor(_color);
    }

    /**
     * Changes the list of available options to choose from.
     * @param options List of strings.
     * @param translate True for a list of string IDs, false for a list of raw strings.
     */
    internal void setOptions(List<string> options, bool translate = false)
    {
        setDropdown(options.Count);
	    _list.clearList();
	    foreach (var option in options)
	    {
		    if (translate)
			    _list.addRow(1, _lang.getString(option));
		    else
			    _list.addRow(1, option);
	    }
        setSelected(_sel);
    }

    /**
     * Changes the currently selected option.
     * @param sel Selected row.
     */
    internal void setSelected(uint sel)
    {
        _sel = sel;
        if (_sel < _list.getTexts())
        {
            _button.setText(_list.getCellText(_sel, 0));
        }
    }

    /**
     * Updates the size of the dropdown list based on
     * the number of options available.
     * @param options Number of options.
     */
    void setDropdown(int options)
    {
        int items = Math.Min(options, MAX_ITEMS);
        int h = _button.getFont().getHeight() + _button.getFont().getSpacing();
        int dy = (Options.baseYResolution - 200) / 2;
        while (_window.getY() + items * h + VERTICAL_MARGIN * 2 > 200 + dy)
        {
            items--;
        }

        int popupHeight = items * h + VERTICAL_MARGIN * 2;
        int popupY = getPopupWindowY(getHeight(), getY(), popupHeight, _popupAboveButton);
        _window.setY(popupY);
        _window.setHeight(popupHeight);
        _list.setY(popupY + VERTICAL_MARGIN);
        _list.setHeight(items * h);
    }

    /**
     * Draws the arrow used to indicate the combo box.
     */
    void drawArrow()
    {
        _arrow.clear();

        SDL_Rect square;
        int color = _color + 1;
        if (color == 256)
            color++;

        // Draw arrow triangle 1
        square.x = 1;
        square.y = 2;
        square.w = 9;
        square.h = 1;

        for (; square.w > 1; square.w -= 2)
        {
            _arrow.drawRect(ref square, (byte)(color + 2));
            square.x++;
            square.y++;
        }
        _arrow.drawRect(ref square, (byte)(color + 2));

        // Draw arrow triangle 2
        square.x = 2;
        square.y = 2;
        square.w = 7;
        square.h = 1;

        for (; square.w > 1; square.w -= 2)
        {
            _arrow.drawRect(ref square, (byte)color);
            square.x++;
            square.y++;
        }
        _arrow.drawRect(ref square, (byte)color);
    }

    /**
     * Changes the color of the arrow buttons in the list.
     * @param color Color value.
     */
    internal void setArrowColor(byte color) =>
        _list.setArrowColor(color);

    /**
     * Sets a function to be called every time the slider's value changes.
     * @param handler Action handler.
     */
    internal void onChange(ActionHandler handler) =>
        _change = handler;

    /**
     * Returns the currently selected option.
     * @return Selected row.
     */
    internal uint getSelected() =>
	    _sel;

    /**
     * Sets a function to be called every time the mouse moves in to the listbox surface.
     * @param handler Action handler.
     */
    internal void onListMouseIn(ActionHandler handler) =>
	    _list.onMouseIn(handler);

    /**
     * Sets a function to be called every time the mouse moves out of the listbox surface.
     * @param handler Action handler.
     */
    internal void onListMouseOut(ActionHandler handler) =>
	    _list.onMouseOut(handler);

    /**
     * Sets a function to be called every time the mouse moves over the listbox surface.
     * @param handler Action handler.
     */
    internal void onListMouseOver(ActionHandler handler) =>
	    _list.onMouseOver(handler);

    internal uint getHoveredListIdx()
    {
        unchecked
        {
            var ret = (uint)-1;
            if (_list.getVisible())
            {
                ret = _list.getSelectedRow();
            }
            if ((uint)-1 == ret)
            {
                ret = _sel;
            }
            return ret;
        }
    }

    /**
     * sets the button text independent of the currently selected option.
     * @param text the text to display
     */
    internal void setText(string text) =>
	    _button.setText(text);

    /**
     * Passes ticks to arrow buttons.
     */
    internal override void think()
    {
	    _button.think();
	    _arrow.think();
	    _window.think();
	    _list.think();
	    base.think();
    }

    /**
     * Passes events to internal components.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    internal override void handle(Action action, State state)
    {
	    _button.handle(action, state);
	    _list.handle(action, state);
	    base.handle(action, state);
	    int topY = Math.Min(getY(), _window.getY());
	    if (_window.getVisible() && action.getDetails().type == SDL_EventType.SDL_MOUSEBUTTONDOWN &&
		    (action.getAbsoluteXMouse() < getX() || action.getAbsoluteXMouse() >= getX() + getWidth() ||
		     action.getAbsoluteYMouse() < topY || action.getAbsoluteYMouse() >= topY + getHeight() + _window.getHeight()))
	    {
		    toggle();
	    }
	    if (_toggled)
	    {
		    if (_change != null)
		    {
			    _change(action);
		    }
		    _toggled = false;
	    }
    }

    /**
     * Blits the combo box components.
     * @param surface Pointer to surface to blit onto.
     */
    internal override void blit(Surface surface)
    {
	    base.blit(surface);
	    _list.invalidate();
	    if (_visible && !_hidden)
	    {
		    _button.blit(surface);
		    _arrow.blit(surface);
		    _window.blit(surface);
		    _list.blit(surface);
	    }
    }

    /**
     * Changes the resources for the text in the combo box.
     * @param big Pointer to large-size font.
     * @param small Pointer to small-size font.
     * @param lang Pointer to current language.
     */
    internal override void initText(Font big, Font small, Language lang)
    {
	    _lang = lang;
	    _button.initText(big, small, lang);
	    _list.initText(big, small, lang);
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
	    _button.setPalette(colors, firstcolor, ncolors);
	    _arrow.setPalette(colors, firstcolor, ncolors);
	    _window.setPalette(colors, firstcolor, ncolors);
	    _list.setPalette(colors, firstcolor, ncolors);
    }

    /**
     * Changes the position of the surface in the X axis.
     * @param x X position in pixels.
     */
    internal override void setX(int x)
    {
	    base.setX(x);
	    _button.setX(x);
	    _arrow.setX(x + getWidth() - BUTTON_WIDTH);
	    _window.setX(x);
	    _list.setX(x + HORIZONTAL_MARGIN);
    }

    /**
     * Changes the position of the surface in the Y axis.
     * @param y Y position in pixels.
     */
    internal override void setY(int y)
    {
	    base.setY(y);
	    _button.setY(y);
	    _arrow.setY(y + 4);

	    int popupHeight = _window.getHeight();
	    int popupY = getPopupWindowY(getHeight(), y, popupHeight, _popupAboveButton);
	    _window.setY(popupY);
	    _list.setY(popupY + VERTICAL_MARGIN);
    }

    /**
     * Enables/disables high contrast color. Mostly used for
     * Battlescape UI.
     * @param contrast High contrast setting.
     */
    internal override void setHighContrast(bool contrast)
    {
	    _button.setHighContrast(contrast);
	    _window.setHighContrast(contrast);
	    _list.setHighContrast(contrast);
    }
}
