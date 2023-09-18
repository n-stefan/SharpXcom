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
 * Displays a list of possible targets.
 */
internal class MultipleTargetsState : State
{
    const int MARGIN = 10;
    const int SPACING = 4;
    const int BUTTON_HEIGHT = 16;

    List<Target> _targets;
    Craft _craft;
    GeoscapeState _state;
    Window _window;
    List<TextButton> _btnTargets;

    /**
     * Initializes all the elements in the Multiple Targets window.
     * @param game Pointer to the core game.
     * @param targets List of targets to display.
     * @param craft Pointer to craft to retarget (NULL if none).
     * @param state Pointer to the Geoscape state.
     */
    internal MultipleTargetsState(List<Target> targets, Craft craft, GeoscapeState state)
    {
        _targets = targets;
        _craft = craft;
        _state = state;

        _screen = false;

        if (_targets.Count > 1)
        {
            int winHeight = BUTTON_HEIGHT * _targets.Count + SPACING * (_targets.Count - 1) + MARGIN * 2;
            int winY = (200 - winHeight) / 2;
            int btnY = winY + MARGIN;

            // Create objects
            _window = new Window(this, 136, winHeight, 60, winY, WindowPopup.POPUP_VERTICAL);

            // Set palette
            setInterface("multipleTargets");

            add(_window, "window", "multipleTargets");

            // Set up objects
            _window.setBackground(_game.getMod().getSurface("BACK15.SCR"));

            int y = btnY;
            for (int i = 0; i < _targets.Count; ++i)
            {
                TextButton button = new TextButton(116, BUTTON_HEIGHT, 70, y);
                button.setText(_targets[i].getName(_game.getLanguage()));
                button.onMouseClick(btnTargetClick);
                add(button, "button", "multipleTargets");

                _btnTargets.Add(button);

                y += button.getHeight() + SPACING;
            }
            _btnTargets[0].onKeyboardPress(btnCancelClick, Options.keyCancel);

            centerAllSurfaces();
        }
    }

    /**
     *
     */
    ~MultipleTargetsState() { }

    /**
     * Pick a target to display.
     * @param action Pointer to an action.
     */
    void btnTargetClick(Action action)
    {
        for (int i = 0; i < _btnTargets.Count; ++i)
        {
            if (action.getSender() == _btnTargets[i])
            {
                popupTarget(_targets[i]);
                break;
            }
        }
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _) =>
        _game.popState();

    /**
     * Displays the right popup for a specific target.
     * @param target Pointer to target.
     */
    void popupTarget(Target target)
    {
        _game.popState();
        if (_craft == null)
        {
            Base b = (Base)target;
            Craft c = (Craft)target;
            Ufo u = (Ufo)target;
            if (b != null)
            {
                _game.pushState(new InterceptState(_state.getGlobe(), b));
            }
            else if (c != null)
            {
                _game.pushState(new GeoscapeCraftState(c, _state.getGlobe(), null));
            }
            else if (u != null)
            {
                _game.pushState(new UfoDetectedState(u, _state, false, u.getHyperDetected()));
            }
            else
            {
                _game.pushState(new TargetInfoState(target, _state.getGlobe()));
            }
        }
        else
        {
            _game.pushState(new ConfirmDestinationState(_craft, target));
        }
    }

    /**
     * Resets the palette and ignores the window
     * if there's only one target.
     */
    protected override void init()
    {
	    if (_targets.Count == 1)
	    {
		    popupTarget(_targets.First());
	    }
	    else
	    {
		    base.init();
	    }
    }
}
