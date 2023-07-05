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

namespace SharpXcom.Geoscape;

/**
 * Generic window used to display all the
 * crafts targeting a certain point on the map.
 */
internal class TargetInfoState : State
{
    Target _target;
    Globe _globe;
    Window _window;
    TextButton _btnIntercept, _btnOk;
    TextEdit _edtTitle;
    Text _txtTargetted, _txtFollowers;

    /**
     * Initializes all the elements in the Target Info window.
     * @param game Pointer to the core game.
     * @param target Pointer to the target to show info from.
     * @param globe Pointer to the Geoscape globe.
     */
    internal TargetInfoState(Target target, Globe globe)
    {
        _target = target;
        _globe = globe;

        _screen = false;

        // Create objects
        _window = new Window(this, 192, 120, 32, 40, WindowPopup.POPUP_BOTH);
        _btnIntercept = new TextButton(160, 12, 48, 124);
        _btnOk = new TextButton(160, 12, 48, 140);
        _edtTitle = new TextEdit(this, 182, 32, 37, 46);
        _txtTargetted = new Text(182, 9, 37, 78);
        _txtFollowers = new Text(182, 40, 37, 88);

        // Set palette
        setInterface("targetInfo");

        add(_window, "window", "targetInfo");
        add(_btnIntercept, "button", "targetInfo");
        add(_btnOk, "button", "targetInfo");
        add(_edtTitle, "text2", "targetInfo");
        add(_txtTargetted, "text1", "targetInfo");
        add(_txtFollowers, "text1", "targetInfo");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

        _btnIntercept.setText(tr("STR_INTERCEPT"));
        _btnIntercept.onMouseClick(btnInterceptClick);

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _edtTitle.setBig();
        _edtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _edtTitle.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
        _edtTitle.setWordWrap(true);
        _edtTitle.setText(_target.getName(_game.getLanguage()));
        _edtTitle.onChange(edtTitleChange);

        _txtTargetted.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTargetted.setText(tr("STR_TARGETTED_BY"));
        _txtFollowers.setAlign(TextHAlign.ALIGN_CENTER);
        string ss = null;
        foreach (var i in _target.getFollowers())
        {
            ss = $"{ss}{i.getName(_game.getLanguage())}\n";
        }
        _txtFollowers.setText(ss);
    }

    /**
     *
     */
    ~TargetInfoState() { }

    /**
     * Picks a craft to intercept the UFO.
     * @param action Pointer to an action.
     */
    void btnInterceptClick(Engine.Action _) =>
        _game.pushState(new InterceptState(_globe, null, _target));

    /**
     * Closes the window.
     * @param action Pointer to an action.
     */
    void btnOkClick(Engine.Action _) =>
        _game.popState();

    /**
     * Changes the target name.
     * @param action Pointer to an action.
     */
    void edtTitleChange(Engine.Action action)
    {
        if (_edtTitle.getText() == _target.getDefaultName(_game.getLanguage()))
        {
            _target.setName(string.Empty);
        }
        else
        {
            _target.setName(_edtTitle.getText());
        }
        if (action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_RETURN ||
            action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_KP_ENTER)
        {
            _edtTitle.setText(_target.getName(_game.getLanguage()));
        }
    }
}
