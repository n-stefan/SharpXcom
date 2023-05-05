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

namespace SharpXcom.Menu;

/**
 * Generic window used to display error messages.
 */
internal class ErrorMessageState : State
{
    Window _window;
    TextButton _btnOk;
    Text _txtMessage;

    /**
	 * Initializes all the elements in an error window.
	 * @param game Pointer to the core game.
	 * @param msg Text string for the message to display.
	 * @param palette Parent state palette.
	 * @param color Color of the UI controls.
	 * @param bg Background image.
	 * @param bgColor Background color (-1 for Battlescape).
	 */
    internal ErrorMessageState(string msg, SDL_Color[] palette, byte color, string bg, int bgColor) =>
		create(msg, palette, color, bg, bgColor);

	/**
	 *
	 */
	~ErrorMessageState() { }

	/**
	 * Creates the elements in an error window.
	 * @param str Text string for the message to display.
	 * @param palette Parent state palette.
	 * @param color Color of the UI controls.
	 * @param bg Background image.
	 * @param bgColor Background color (-1 for Battlescape).
	 */
	void create(string str, SDL_Color[] palette, byte color, string bg, int bgColor)
	{
		_screen = false;

		// Create objects
		_window = new Window(this, 256, 160, 32, 20, WindowPopup.POPUP_BOTH);
		_btnOk = new TextButton(120, 18, 100, 154);
		_txtMessage = new Text(246, 80, 37, 50);

		// Set palette
		setPalette(palette);
		if (bgColor != -1)
			setPalette(_game.getMod().getPalette("BACKPALS.DAT").getColors(Palette.blockOffset((byte)bgColor)), Palette.backPos, 16);

		add(_window);
		add(_btnOk);
		add(_txtMessage);

		centerAllSurfaces();

		// Set up objects
		_window.setColor(color);
		_window.setBackground(_game.getMod().getSurface(bg));

		_btnOk.setColor(color);
		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

		_txtMessage.setColor(color);
		_txtMessage.setAlign(TextHAlign.ALIGN_CENTER);
		_txtMessage.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
		_txtMessage.setBig();
		_txtMessage.setWordWrap(true);
		_txtMessage.setText(str);

		if (bgColor == -1)
		{
			_window.setHighContrast(true);
			_btnOk.setHighContrast(true);
			_txtMessage.setHighContrast(true);
		}
	}

    /**
     * Closes the window.
     * @param action Pointer to an action.
     */
    void btnOkClick(Engine.Action _) =>
        _game.popState();
}
