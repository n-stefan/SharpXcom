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
 * Controls screen which allows the user to
 * customize the various key shortcuts in the game.
 */
internal class OptionsControlsState : OptionsBaseState
{
    int _selected;
    OptionInfo _selKey;
    TextList _lstControls;
    byte _colorGroup, _colorSel, _colorNormal;
    List<OptionInfo> _controlsGeneral, _controlsGeo, _controlsBattle;

    /**
     * Initializes all the elements in the Controls screen.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     */
    internal OptionsControlsState(OptionsOrigin origin) : base(origin)
    {
        _selected = -1;
        _selKey = null;

        setCategory(_btnControls);

        // Create objects
        _lstControls = new TextList(200, 136, 94, 8);

        if (origin != OptionsOrigin.OPT_BATTLESCAPE)
        {
            add(_lstControls, "optionLists", "controlsMenu");
        }
        else
        {
            add(_lstControls, "optionLists", "battlescape");
        }

        centerAllSurfaces();

        // Set up objects
        _lstControls.setColumns(2, 152, 48);
        _lstControls.setWordWrap(true);
        _lstControls.setSelectable(true);
        _lstControls.setBackground(_window);
        _lstControls.onMouseClick(lstControlsClick, 0);
        _lstControls.onKeyboardPress(lstControlsKeyPress);
        _lstControls.setFocus(true);
        _lstControls.setTooltip("STR_CONTROLS_DESC");
        _lstControls.onMouseIn(txtTooltipIn);
        _lstControls.onMouseOut(txtTooltipOut);

        _colorGroup = _lstControls.getSecondaryColor();
        _colorSel = (byte)_lstControls.getScrollbarColor();
        _colorNormal = _lstControls.getColor();

        List<OptionInfo> options = Options.getOptionInfo();
        foreach (var i in options)
        {
            if (i.type() == OptionType.OPTION_KEY && !string.IsNullOrEmpty(i.description()))
            {
                if (i.category() == "STR_GENERAL")
                {
                    _controlsGeneral.Add(i);
                }
                else if (i.category() == "STR_GEOSCAPE")
                {
                    _controlsGeo.Add(i);
                }
                else if (i.category() == "STR_BATTLESCAPE")
                {
                    _controlsBattle.Add(i);
                }
            }
        }
    }

    /**
     *
     */
    ~OptionsControlsState() { }

    /**
     * Select a control for changing.
     * @param action Pointer to an action.
     */
    void lstControlsClick(Action action)
    {
        if (action.getDetails().button.button != SDL_BUTTON_LEFT && action.getDetails().button.button != SDL_BUTTON_RIGHT)
        {
            return;
        }
        if (_selected != -1)
        {
            int selected = _selected;
            _lstControls.setCellColor((uint)_selected, 0, _colorNormal);
            _lstControls.setCellColor((uint)_selected, 1, _colorNormal);
            _selected = -1;
            _selKey = null;
            if (selected == _lstControls.getSelectedRow())
                return;
        }
        _selected = (int)_lstControls.getSelectedRow();
        _selKey = getControl((uint)_selected);
        if (_selKey == null)
        {
            _selected = -1;
            return;
        }

        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            _lstControls.setCellColor((uint)_selected, 0, _colorSel);
            _lstControls.setCellColor((uint)_selected, 1, _colorSel);
        }
        else if (action.getDetails().button.button == SDL_BUTTON_RIGHT)
        {
            _lstControls.setCellText((uint)_selected, 1, string.Empty);
            _selKey.asKey() = SDL_Keycode.SDLK_UNKNOWN;
            _selected = -1;
            _selKey = null;
        }
    }

    /**
     * Gets the currently selected control.
     * @param sel Selected row.
     * @return Pointer to option, NULL if none selected.
     */
    OptionInfo getControl(uint sel)
    {
        if (sel > 0 &&
            sel <= _controlsGeneral.Count)
        {
            return _controlsGeneral[(int)(sel - 1)];
        }
        else if (sel > _controlsGeneral.Count + 2 &&
                 sel <= _controlsGeneral.Count + 2 + _controlsGeo.Count)
        {
            return _controlsGeo[(int)(sel - 1 - _controlsGeneral.Count - 2)];
        }
        else if (sel > _controlsGeneral.Count + 2 + _controlsGeo.Count + 2 &&
                 sel <= _controlsGeneral.Count + 2 + _controlsGeo.Count + 2 + _controlsBattle.Count)
        {
            return _controlsBattle[(int)(sel - 1 - _controlsGeneral.Count - 2 - _controlsGeo.Count - 2)];
        }
        else
        {
            return null;
        }
    }

    /**
     * Change selected control.
     * @param action Pointer to an action.
     */
    void lstControlsKeyPress(Action action)
    {
        if (_selected != -1)
        {
            SDL_Keycode key = action.getDetails().key.keysym.sym;
            if (key != 0)
            {
                _selKey.asKey() = key;
                string name = ucWords(SDL_GetKeyName(_selKey.asKey()));
                _lstControls.setCellText((uint)_selected, 1, name);
            }
            _lstControls.setCellColor((uint)_selected, 0, _colorNormal);
            _lstControls.setCellColor((uint)_selected, 1, _colorNormal);
            _selected = -1;
            _selKey = null;
        }
    }

    /**
     * Uppercases all the words in a string.
     * @param text Source string.
     * @return Destination string.
     */
    string ucWords(string text)
    {
        var str = text.ToCharArray();
	    if (str.Length != 0)
	    {
		    str[0] = char.ToUpper(str[0]);
	    }
        for (var i = Array.IndexOf(str, ' '); i != -1; i = Array.IndexOf(str, ' ', i + 1))
	    {
		    if (str.Length > i + 1)
			    str[i + 1] = char.ToUpper(str[i + 1]);
		    else
			    break;
	    }
	    return new string(str);
    }

    /**
     * Fills the controls list based on category.
     */
    internal override void init()
    {
	    base.init();
	    _lstControls.clearList();
	    _lstControls.addRow(2, tr("STR_GENERAL"), string.Empty);
	    _lstControls.setCellColor(0, 0, _colorGroup);
	    addControls(_controlsGeneral);
	    _lstControls.addRow(2, string.Empty, string.Empty);
	    _lstControls.addRow(2, tr("STR_GEOSCAPE"), string.Empty);
	    _lstControls.setCellColor((uint)(_controlsGeneral.Count + 2), 0, _colorGroup);
	    addControls(_controlsGeo);
	    _lstControls.addRow(2, string.Empty, string.Empty);
	    _lstControls.addRow(2, tr("STR_BATTLESCAPE"), string.Empty);
	    _lstControls.setCellColor((uint)(_controlsGeneral.Count + 2 + _controlsGeo.Count + 2), 0, _colorGroup);
	    addControls(_controlsBattle);
    }

    /**
     * Adds a bunch of controls to the list.
     * @param keys List of controls.
     */
    void addControls(List<OptionInfo> keys)
    {
	    foreach (var i in keys)
	    {
		    string name = tr(i.description());
		    SDL_Keycode key = i.asKey();
		    string keyName = ucWords(SDL_GetKeyName(key));
		    if (key == SDL_Keycode.SDLK_UNKNOWN)
			    keyName = string.Empty;
		    _lstControls.addRow(2, name, keyName);
	    }
    }
}
