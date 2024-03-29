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

namespace SharpXcom.Menu;

/**
 * window used to confirm save game deletion.
 */
internal class DeleteGameState : State
{
    OptionsOrigin _origin;
    string _filename;
    Window _window;
    TextButton _btnNo, _btnYes;
    Text _txtMessage;

    /**
     * Initializes all the elements in the Confirmation screen.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     * @param save Name of the save file to delete.
     */
    internal DeleteGameState(OptionsOrigin origin, string save)
    {
        _origin = origin;

        _filename = Options.getMasterUserFolder() + save;
        _screen = false;

        // Create objects
        _window = new Window(this, 256, 100, 32, 50, WindowPopup.POPUP_BOTH);
        _btnYes = new TextButton(60, 18, 60, 122);
        _btnNo = new TextButton(60, 18, 200, 122);
        _txtMessage = new Text(246, 32, 37, 70);

        // Set palette
        setInterface("saveMenus", false, _game.getSavedGame() != null ? _game.getSavedGame().getSavedBattle() : null);

        add(_window, "confirmDelete", "saveMenus");
        add(_btnYes, "confirmDelete", "saveMenus");
        add(_btnNo, "confirmDelete", "saveMenus");
        add(_txtMessage, "confirmDelete", "saveMenus");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

        _btnYes.setText(tr("STR_YES"));
        _btnYes.onMouseClick(btnYesClick);
        _btnYes.onKeyboardPress(btnYesClick, Options.keyOk);

        _btnNo.setText(tr("STR_NO"));
        _btnNo.onMouseClick(btnNoClick);
        _btnNo.onKeyboardPress(btnNoClick, Options.keyCancel);

        _txtMessage.setAlign(TextHAlign.ALIGN_CENTER);
        _txtMessage.setBig();
        _txtMessage.setWordWrap(true);
        _txtMessage.setText(tr("STR_IS_IT_OK_TO_DELETE_THE_SAVED_GAME"));

        if (_origin == OptionsOrigin.OPT_BATTLESCAPE)
        {
            applyBattlescapeTheme();
        }
    }

    /**
     *
     */
    ~DeleteGameState() { }

    void btnYesClick(Action _)
    {
        _game.popState();
        if (!CrossPlatform.deleteFile(_filename))
        {
            string error = tr("STR_DELETE_UNSUCCESSFUL");
            if (_origin != OptionsOrigin.OPT_BATTLESCAPE)
                _game.pushState(new ErrorMessageState(error, _palette, (byte)_game.getMod().getInterface("errorMessages").getElement("geoscapeColor").color, "BACK01.SCR", _game.getMod().getInterface("errorMessages").getElement("geoscapePalette").color));
            else
                _game.pushState(new ErrorMessageState(error, _palette, (byte)_game.getMod().getInterface("errorMessages").getElement("battlescapeColor").color, "TAC00.SCR", _game.getMod().getInterface("errorMessages").getElement("battlescapePalette").color));
        }
    }

    void btnNoClick(Action _) =>
        _game.popState();
}
