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
 * along with OpenXcom.  If not, see <http:///www.gnu.org/licenses/>.
 */

namespace SharpXcom.Geoscape;

/**
 * Screen that allows the player
 * to pick a target for a craft on the globe.
 */
internal class BaseDestroyedState : State
{
    Base _base;
    Window _window;
    TextButton _btnOk;
    Text _txtMessage;

    internal BaseDestroyedState(Base @base)
    {
        _base = @base;

        _screen = false;

        // Create objects
        _window = new Window(this, 256, 160, 32, 20);
        _btnOk = new TextButton(100, 20, 110, 142);
        _txtMessage = new Text(224, 48, 48, 76);

        // Set palette
        setInterface("baseDestroyed");

        add(_window, "window", "baseDestroyed");
        add(_btnOk, "button", "baseDestroyed");
        add(_txtMessage, "text", "baseDestroyed");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK15.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _txtMessage.setAlign(TextHAlign.ALIGN_CENTER);
        _txtMessage.setBig();
        _txtMessage.setWordWrap(true);

        _txtMessage.setText(tr("STR_THE_ALIENS_HAVE_DESTROYED_THE_UNDEFENDED_BASE").arg(_base.getName()));

        int k;
        var regions = _game.getSavedGame().getRegions();
        for (k = 0; k < regions.Count; k++)
        {
            if (regions[k].getRules().insideRegion(@base.getLongitude(), @base.getLatitude()))
            {
                break;
            }
        }

        AlienMission am = _game.getSavedGame().findAlienMission(regions[k].getRules().getType(), MissionObjective.OBJECTIVE_RETALIATION);
        var ufos = _game.getSavedGame().getUfos();
        for (var i = 0; i < ufos.Count;)
        {
            if (ufos[i].getMission() == am)
            {
                ufos.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }

        foreach (var i in _game.getSavedGame().getAlienMissions())
        {
            if ((AlienMission)i == am)
            {
                _game.getSavedGame().getAlienMissions().Remove(i);
                break;
            }
        }
    }

    /**
     *
     */
    ~BaseDestroyedState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Engine.Action _)
    {
        _game.popState();
        foreach (var i in _game.getSavedGame().getBases())
        {
            if (i == _base)
            {
                _game.getSavedGame().getBases().Remove(i);
                break;
            }
        }
    }
}
