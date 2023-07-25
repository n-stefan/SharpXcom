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
 * Displays info on an alien base.
 */
internal class AlienBaseState : State
{
    GeoscapeState _state;
    AlienBase _base;
    Window _window;
    TextButton _btnOk;
    Text _txtTitle;

    /**
     * Initializes all the elements in the Aliens Base discovered window.
     * @param game Pointer to the core game.
     * @param base Pointer to the alien base to get info from.
     * @param state Pointer to the Geoscape.
     */
    internal AlienBaseState(AlienBase @base, GeoscapeState state)
    {
        _state = state;
        _base = @base;

        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        _btnOk = new TextButton(50, 12, 135, 180);
        _txtTitle = new Text(308, 60, 6, 60);

        setInterface("alienBase");

        add(_window, "window", "alienBase");
        add(_btnOk, "text", "alienBase");
        add(_txtTitle, "button", "alienBase");

        centerAllSurfaces();


        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK13.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setBig();
        _txtTitle.setWordWrap(true);

        // Check location of base
        string region = null, country = null;
        foreach (var i in _game.getSavedGame().getCountries())
        {
            if (i.getRules().insideCountry(_base.getLongitude(), _base.getLatitude()))
            {
                country = tr(i.getRules().getType());
                break;
            }
        }
        foreach (var i in _game.getSavedGame().getRegions())
        {
            if (i.getRules().insideRegion(_base.getLongitude(), _base.getLatitude()))
            {
                region = tr(i.getRules().getType());
                break;
            }
        }
        string location;
        if (!string.IsNullOrEmpty(country))
        {
            location = tr("STR_COUNTRIES_COMMA").arg(country).arg(region);
        }
        else if (!string.IsNullOrEmpty(region))
        {
            location = region;
        }
        else
        {
            location = tr("STR_UNKNOWN");
        }
        _txtTitle.setText(tr("STR_XCOM_AGENTS_HAVE_LOCATED_AN_ALIEN_BASE_IN_REGION").arg(location));
    }

    /**
     *
     */
    ~AlienBaseState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        _state.timerReset();
        _state.getGlobe().center(_base.getLongitude(), _base.getLatitude());
        _game.popState();
    }
}
