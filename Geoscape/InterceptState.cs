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
 * Intercept window that lets the player launch
 * crafts into missions from the Geoscape.
 */
internal class InterceptState : State
{
    Globe _globe;
    Base _base;
    Target _target;
    Window _window;
    TextButton _btnCancel, _btnGotoBase;
    Text _txtTitle, _txtCraft, _txtStatus, _txtBase, _txtWeapons;
    TextList _lstCrafts;
    List<Craft> _crafts;

    /**
     * Initializes all the elements in the Intercept window.
     * @param game Pointer to the core game.
     * @param globe Pointer to the Geoscape globe.
     * @param base Pointer to base to show contained crafts (NULL to show all crafts).
     * @param target Pointer to target to intercept (NULL to ask user for target).
     */
    internal InterceptState(Globe globe, Base @base = null, Target target = null)
    {
        _globe = globe;
        _base = @base;
        _target = target;

        _screen = false;

        // Create objects
        _window = new Window(this, 320, 140, 0, 30, WindowPopup.POPUP_HORIZONTAL);
        _btnCancel = new TextButton(_base != null ? 142 : 288, 16, 16, 146);
        _btnGotoBase = new TextButton(142, 16, 162, 146);
        _txtTitle = new Text(300, 17, 10, 46);
        _txtCraft = new Text(86, 9, 14, 70);
        _txtStatus = new Text(70, 9, 100, 70);
        _txtBase = new Text(80, 9, 170, 70);
        _txtWeapons = new Text(80, 17, 238, 62);
        _lstCrafts = new TextList(288, 64, 8, 78);

        // Set palette
        setInterface("intercept");

        add(_window, "window", "intercept");
        add(_btnCancel, "button", "intercept");
        add(_btnGotoBase, "button", "intercept");
        add(_txtTitle, "text1", "intercept");
        add(_txtCraft, "text2", "intercept");
        add(_txtStatus, "text2", "intercept");
        add(_txtBase, "text2", "intercept");
        add(_txtWeapons, "text2", "intercept");
        add(_lstCrafts, "list", "intercept");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK12.SCR"));

        _btnCancel.setText(tr("STR_CANCEL"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyGeoIntercept);

        _btnGotoBase.setText(tr("STR_GO_TO_BASE"));
        _btnGotoBase.onMouseClick(btnGotoBaseClick);
        _btnGotoBase.setVisible(_base != null);

        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
        _txtTitle.setBig();
        _txtTitle.setText(tr("STR_LAUNCH_INTERCEPTION"));

        _txtCraft.setText(tr("STR_CRAFT"));

        _txtStatus.setText(tr("STR_STATUS"));

        _txtBase.setText(tr("STR_BASE"));

        _txtWeapons.setText(tr("STR_WEAPONS_CREW_HWPS"));

        _lstCrafts.setColumns(4, 86, 70, 80, 46);
        _lstCrafts.setSelectable(true);
        _lstCrafts.setBackground(_window);
        _lstCrafts.setMargin(6);
        _lstCrafts.onMouseClick(lstCraftsLeftClick);
        _lstCrafts.onMouseClick(lstCraftsRightClick, SDL_BUTTON_RIGHT);

        uint row = 0;
        foreach (var i in _game.getSavedGame().getBases())
        {
            if (_base != null && i != _base)
                continue;
            foreach (var j in i.getCrafts())
            {
                string ss;
                if (j.getNumWeapons() > 0)
                {
                    ss = $"{Unicode.TOK_COLOR_FLIP}{j.getNumWeapons()}{Unicode.TOK_COLOR_FLIP}";
                }
                else
                {
                    ss = "0";
                }
                ss = $"{ss}/";
                if (j.getNumSoldiers() > 0)
                {
                    ss = $"{ss}{Unicode.TOK_COLOR_FLIP}{j.getNumSoldiers()}{Unicode.TOK_COLOR_FLIP}";
                }
                else
                {
                    ss = $"{ss}0";
                }
                ss = $"{ss}/";
                if (j.getNumVehicles() > 0)
                {
                    ss = $"{ss}{Unicode.TOK_COLOR_FLIP}{j.getNumVehicles()}{Unicode.TOK_COLOR_FLIP}";
                }
                else
                {
                    ss = $"{ss}0";
                }
                _crafts.Add(j);
                _lstCrafts.addRow(4, j.getName(_game.getLanguage()), tr(j.getStatus()), i.getName(), ss);
                if (j.getStatus() == "STR_READY")
                {
                    _lstCrafts.setCellColor(row, 1, _lstCrafts.getSecondaryColor());
                }
                row++;
            }
        }
    }

    /**
     *
     */
    ~InterceptState() { }

    /**
     * Closes the window.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _) =>
        _game.popState();

    /**
     * Goes to the base for the respective craft.
     * @param action Pointer to an action.
     */
    void btnGotoBaseClick(Action _)
    {
        _game.popState();
        _game.pushState(new BasescapeState(_base, _globe));
    }

    /**
     * Pick a target for the selected craft.
     * @param action Pointer to an action.
     */
    void lstCraftsLeftClick(Action _)
    {
        Craft c = _crafts[(int)_lstCrafts.getSelectedRow()];
        if (c.getStatus() == "STR_READY" || ((c.getStatus() == "STR_OUT" || Options.craftLaunchAlways) && !c.getLowFuel() && !c.getMissionComplete()))
        {
            _game.popState();
            if (_target == null)
            {
                _game.pushState(new SelectDestinationState(c, _globe));
            }
            else
            {
                _game.pushState(new ConfirmDestinationState(c, _target));
            }
        }
    }

    /**
     * Centers on the selected craft.
     * @param action Pointer to an action.
     */
    void lstCraftsRightClick(Action _)
    {
        Craft c = _crafts[(int)_lstCrafts.getSelectedRow()];
        if (c.getStatus() == "STR_OUT")
        {
            _globe.center(c.getLongitude(), c.getLatitude());
            _game.popState();
        }
    }
}
