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
 * Options window that displays the
 * advanced game settings.
 */
internal class OptionsAdvancedState : OptionsBaseState
{
    TextList _lstOptions;
    byte _colorGroup;
    List<OptionInfo> _settingsGeneral, _settingsGeo, _settingsBattle;

    /**
     * Initializes all the elements in the Advanced Options window.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     */
    internal OptionsAdvancedState(OptionsOrigin origin) : base(origin)
    {
        setCategory(_btnAdvanced);

        // Create objects
        _lstOptions = new TextList(200, 136, 94, 8);

        if (origin != OptionsOrigin.OPT_BATTLESCAPE)
        {
            add(_lstOptions, "optionLists", "advancedMenu");
        }
        else
        {
            add(_lstOptions, "optionLists", "battlescape");
        }
        centerAllSurfaces();

        // how much room do we need for YES/NO
        Text text = new Text(100, 9, 0, 0);
        text.initText(_game.getMod().getFont("FONT_BIG"), _game.getMod().getFont("FONT_SMALL"), _game.getLanguage());
        text.setText(tr("STR_YES"));
        int yes = text.getTextWidth();
        text.setText(tr("STR_NO"));
        int no = text.getTextWidth();

        int rightcol = Math.Max(yes, no) + 2;
        int leftcol = _lstOptions.getWidth() - rightcol;

        // Set up objects
        _lstOptions.setAlign(TextHAlign.ALIGN_RIGHT, 1);
        _lstOptions.setColumns(2, leftcol, rightcol);
        _lstOptions.setWordWrap(true);
        _lstOptions.setSelectable(true);
        _lstOptions.setBackground(_window);
        _lstOptions.onMouseClick(lstOptionsClick, 0);
        _lstOptions.onMouseOver(lstOptionsMouseOver);
        _lstOptions.onMouseOut(lstOptionsMouseOut);

        _colorGroup = _lstOptions.getSecondaryColor();

        List<OptionInfo> options = Options.getOptionInfo();
        foreach (var i in options)
        {
            if (i.type() != OptionType.OPTION_KEY && !string.IsNullOrEmpty(i.description()))
            {
                if (i.category() == "STR_GENERAL")
                {
                    _settingsGeneral.Add(i);
                }
                else if (i.category() == "STR_GEOSCAPE")
                {
                    _settingsGeo.Add(i);
                }
                else if (i.category() == "STR_BATTLESCAPE")
                {
                    _settingsBattle.Add(i);
                }
            }
        }
    }

    /**
     *
     */
    ~OptionsAdvancedState() { }

    /**
     * Changes the clicked setting.
     * @param action Pointer to an action.
     */
    void lstOptionsClick(Engine.Action action)
    {
        byte button = action.getDetails().button.button;
        if (button != SDL_BUTTON_LEFT && button != SDL_BUTTON_RIGHT)
        {
            return;
        }
        uint sel = _lstOptions.getSelectedRow();
        OptionInfo setting = getSetting(sel);
        if (setting == null) return;

        string settingText = null;
        if (setting.type() == OptionType.OPTION_BOOL)
        {
            bool b = setting.asBool();
            b = !b;
            settingText = b ? tr("STR_YES") : tr("STR_NO");
        }
        else if (setting.type() == OptionType.OPTION_INT) // integer variables will need special handling
        {
            int i = setting.asInt();

            int increment = (button == SDL_BUTTON_LEFT) ? 1 : -1; // left-click increases, right-click decreases
            if (i == Options.changeValueByMouseWheel || i == Options.FPS || i == Options.FPSInactive)
            {
                increment *= 10;
            }
            i += increment;

            int min = 0, max = 0;
            if (i == Options.battleExplosionHeight)
            {
                min = 0;
                max = 3;
            }
            else if (i == Options.changeValueByMouseWheel)
            {
                min = 0;
                max = 50;
            }
            else if (i == Options.FPS)
            {
                min = 0;
                max = 120;
            }
            else if (i == Options.FPSInactive)
            {
                min = 10;
                max = 120;
            }
            else if (i == Options.mousewheelSpeed)
            {
                min = 1;
                max = 7;
            }
            else if (i == Options.autosaveFrequency)
            {
                min = 1;
                max = 5;
            }

            if (i < min)
            {
                i = max;
            }
            else if (i > max)
            {
                i = min;
            }

            settingText = i.ToString();
        }
        _lstOptions.setCellText(sel, 1, settingText);
    }

    void lstOptionsMouseOver(Engine.Action _)
    {
        uint sel = _lstOptions.getSelectedRow();
        OptionInfo setting = getSetting(sel);
        string desc = null;
        if (setting != null)
        {
            desc = tr(setting.description() + "_DESC");
        }
        _txtTooltip.setText(desc);
    }

    void lstOptionsMouseOut(Engine.Action _) =>
        _txtTooltip.setText(string.Empty);

    /**
     * Gets the currently selected setting.
     * @param sel Selected row.
     * @return Pointer to option, NULL if none selected.
     */
    OptionInfo getSetting(uint sel)
    {
        if (sel > 0 &&
            sel <= _settingsGeneral.Count)
        {
            return _settingsGeneral[(int)(sel - 1)];
        }
        else if (sel > _settingsGeneral.Count + 2 &&
                 sel <= _settingsGeneral.Count + 2 + _settingsGeo.Count)
        {
            return _settingsGeo[(int)(sel - 1 - _settingsGeneral.Count - 2)];
        }
        else if (sel > _settingsGeneral.Count + 2 + _settingsGeo.Count + 2 &&
                 sel <= _settingsGeneral.Count + 2 + _settingsGeo.Count + 2 + _settingsBattle.Count)
        {
            return _settingsBattle[(int)(sel - 1 - _settingsGeneral.Count - 2 - _settingsGeo.Count - 2)];
        }
        else
        {
            return null;
        }
    }
}
