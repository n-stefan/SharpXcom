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

namespace SharpXcom.Ufopaedia;

/**
 * UfopaediaStartState is the screen that opens when clicking Ufopaedia button in Geoscape.
 * Presents buttons to all sections of Ufopaedia, opening a UfopaediaSelectState on click.
 */
internal class UfopaediaStartState : State
{
    protected const uint CAT_MIN_BUTTONS = 9;
    protected const uint CAT_MAX_BUTTONS = 10;

    int _offset, _scroll;
    List<string> _cats;
    Window _window;
    Text _txtTitle;
    TextButton _btnOk;
    ArrowButton _btnScrollUp, _btnScrollDown;
    Timer _timerScroll;
    List<TextButton> _btnSections;

    internal UfopaediaStartState()
    {
        _offset = 0;
        _scroll = 0;
        _cats = _game.getMod().getUfopaediaCategoryList();

        _screen = false;

        // set background window
        _window = new Window(this, 256, 180, 32, 10, WindowPopup.POPUP_BOTH);

        // set title
        _txtTitle = new Text(220, 17, 50, 33);

        // Set palette
        setInterface("ufopaedia");

        add(_window, "window", "ufopaedia");
        add(_txtTitle, "text", "ufopaedia");

        _btnOk = new TextButton(220, 12, 50, 167);
        add(_btnOk, "button1", "ufopaedia");

        // set buttons
        int y = 50;
        uint numButtons = Math.Min((uint)_cats.Count, CAT_MAX_BUTTONS);
        if (numButtons > CAT_MIN_BUTTONS)
            y -= (int)(13 * (numButtons - CAT_MIN_BUTTONS));

        _btnScrollUp = new ArrowButton(ArrowShape.ARROW_BIG_UP, 13, 14, 270, y);
        add(_btnScrollUp, "button1", "ufopaedia");
        _btnScrollDown = new ArrowButton(ArrowShape.ARROW_BIG_DOWN, 13, 14, 270, 152);
        add(_btnScrollDown, "button1", "ufopaedia");

        for (uint i = 0; i < numButtons; ++i)
        {
            TextButton button = new TextButton(220, 12, 50, y);
            y += 13;

            add(button, "button1", "ufopaedia");

            button.onMouseClick(btnSectionClick);
            button.onMousePress(btnScrollUpClick, SDL_BUTTON_WHEELUP);
            button.onMousePress(btnScrollDownClick, SDL_BUTTON_WHEELDOWN);

            _btnSections.Add(button);
        }
        updateButtons();
        if (_btnSections.Any())
            _txtTitle.setY(_btnSections.First().getY() - _txtTitle.getHeight());

        centerAllSurfaces();

        _window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

        _txtTitle.setBig();
        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setText(tr("STR_UFOPAEDIA"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyGeoUfopedia);

        _btnScrollUp.setVisible(_cats.Count > CAT_MAX_BUTTONS);
        _btnScrollUp.onMousePress(btnScrollUpPress);
        _btnScrollUp.onMouseRelease(btnScrollRelease);
        _btnScrollDown.setVisible(_cats.Count > CAT_MAX_BUTTONS);
        _btnScrollDown.onMousePress(btnScrollDownPress);
        _btnScrollDown.onMouseRelease(btnScrollRelease);

        _timerScroll = new Timer(50);
        _timerScroll.onTimer((StateHandler)scroll);
    }

    /**
	 * Deletes timers.
	 */
    ~UfopaediaStartState() =>
        _timerScroll = null;

    /**
	 * Displays the list of articles for this section.
	 * @param action Pointer to an action.
	 */
    void btnSectionClick(Action action)
    {
        for (int i = 0; i < _btnSections.Count; ++i)
        {
            if (action.getSender() == _btnSections[i])
            {
                _game.pushState(new UfopaediaSelectState(_cats[_offset + i]));
                break;
            }
        }
    }

    /**
	 * Scrolls the section buttons up.
	 * @param action Pointer to an action.
	 */
    void btnScrollUpClick(Action _)
    {
        _scroll = -1;
        scroll();
    }

    /**
	 * Scrolls the section buttons down.
	 * @param action Pointer to an action.
	 */
    void btnScrollDownClick(Action _)
    {
        _scroll = 1;
        scroll();
    }

    /**
	 * Returns to the previous screen.
	 * @param action Pointer to an action.
	 */
    void btnOkClick(Action _) =>
        _game.popState();

    /**
	 * Starts scrolling the section buttons up.
	 * @param action Pointer to an action.
	 */
    void btnScrollUpPress(Action _)
    {
        _scroll = -1;
        _timerScroll.start();
    }

    /**
	 * Stops scrolling the section buttons.
	 * @param action Pointer to an action.
	 */
    void btnScrollRelease(Action _) =>
        _timerScroll.stop();

    /**
	 * Starts scrolling the section buttons down.
	 * @param action Pointer to an action.
	 */
    void btnScrollDownPress(Action _)
    {
        _scroll = 1;
        _timerScroll.start();
    }

    /**
	 * Offsets the list of section buttons.
	 */
    void scroll()
    {
        if (_cats.Count > CAT_MAX_BUTTONS)
        {
            _offset = Math.Clamp(_offset + _scroll, 0, (int)(_cats.Count - CAT_MAX_BUTTONS));
            updateButtons();
        }
    }

    /**
	 * Updates the section button labels based on scroll.
	 */
    void updateButtons()
    {
        for (int i = 0; i < _btnSections.Count; ++i)
        {
            _btnSections[i].setText(tr(_cats[_offset + i]));
        }
    }
}
