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

namespace SharpXcom.Basescape;

/**
 * Basescape screen that shows a base's layout
 * and lets the player manage their bases.
 */
internal class BasescapeState : State
{
    Base _base;
    Globe _globe;
    Text _txtFacility, _txtLocation, _txtFunds;
    BaseView _view;
    MiniBaseView _mini;
    TextEdit _edtBase;
    TextButton _btnNewBase, _btnBaseInfo, _btnSoldiers, _btnCrafts, _btnFacilities, _btnResearch, _btnManufacture, _btnTransfer, _btnPurchase, _btnSell, _btnGeoscape;

    /**
     * Initializes all the elements in the Basescape screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param globe Pointer to the Geoscape globe.
     */
    internal BasescapeState(Base @base, Globe globe)
    {
        _base = @base;
        _globe = globe;

        // Create objects
        _txtFacility = new Text(192, 9, 0, 0);
        _view = new BaseView(192, 192, 0, 8);
        _mini = new MiniBaseView(128, 16, 192, 41);
        _edtBase = new TextEdit(this, 127, 17, 193, 0);
        _txtLocation = new Text(126, 9, 194, 16);
        _txtFunds = new Text(126, 9, 194, 24);
        _btnNewBase = new TextButton(128, 12, 192, 58);
        _btnBaseInfo = new TextButton(128, 12, 192, 71);
        _btnSoldiers = new TextButton(128, 12, 192, 84);
        _btnCrafts = new TextButton(128, 12, 192, 97);
        _btnFacilities = new TextButton(128, 12, 192, 110);
        _btnResearch = new TextButton(128, 12, 192, 123);
        _btnManufacture = new TextButton(128, 12, 192, 136);
        _btnTransfer = new TextButton(128, 12, 192, 149);
        _btnPurchase = new TextButton(128, 12, 192, 162);
        _btnSell = new TextButton(128, 12, 192, 175);
        _btnGeoscape = new TextButton(128, 12, 192, 188);

        // Set palette
        setInterface("basescape");

        add(_view, "baseView", "basescape");
        add(_mini, "miniBase", "basescape");
        add(_txtFacility, "textTooltip", "basescape");
        add(_edtBase, "text1", "basescape");
        add(_txtLocation, "text2", "basescape");
        add(_txtFunds, "text3", "basescape");
        add(_btnNewBase, "button", "basescape");
        add(_btnBaseInfo, "button", "basescape");
        add(_btnSoldiers, "button", "basescape");
        add(_btnCrafts, "button", "basescape");
        add(_btnFacilities, "button", "basescape");
        add(_btnResearch, "button", "basescape");
        add(_btnManufacture, "button", "basescape");
        add(_btnTransfer, "button", "basescape");
        add(_btnPurchase, "button", "basescape");
        add(_btnSell, "button", "basescape");
        add(_btnGeoscape, "button", "basescape");

        centerAllSurfaces();

        // Set up objects
        _view.setTexture(_game.getMod().getSurfaceSet("BASEBITS.PCK"));
        _view.onMouseClick(viewLeftClick, SDL_BUTTON_LEFT);
        _view.onMouseClick(viewRightClick, SDL_BUTTON_RIGHT);
        _view.onMouseOver(viewMouseOver);
        _view.onMouseOut(viewMouseOut);

        _mini.setTexture(_game.getMod().getSurfaceSet("BASEBITS.PCK"));
        _mini.setBases(_game.getSavedGame().getBases());
        _mini.onMouseClick(miniClick);
        _mini.onKeyboardPress(handleKeyPress);

        _edtBase.setBig();
        _edtBase.onChange(edtBaseChange);

        _btnNewBase.setText(tr("STR_BUILD_NEW_BASE_UC"));
        _btnNewBase.onMouseClick(btnNewBaseClick);

        _btnBaseInfo.setText(tr("STR_BASE_INFORMATION"));
        _btnBaseInfo.onMouseClick(btnBaseInfoClick);

        _btnSoldiers.setText(tr("STR_SOLDIERS_UC"));
        _btnSoldiers.onMouseClick(btnSoldiersClick);

        _btnCrafts.setText(tr("STR_EQUIP_CRAFT"));
        _btnCrafts.onMouseClick(btnCraftsClick);

        _btnFacilities.setText(tr("STR_BUILD_FACILITIES"));
        _btnFacilities.onMouseClick(btnFacilitiesClick);

        _btnResearch.setText(tr("STR_RESEARCH"));
        _btnResearch.onMouseClick(btnResearchClick);

        _btnManufacture.setText(tr("STR_MANUFACTURE"));
        _btnManufacture.onMouseClick(btnManufactureClick);

        _btnTransfer.setText(tr("STR_TRANSFER_UC"));
        _btnTransfer.onMouseClick(btnTransferClick);

        _btnPurchase.setText(tr("STR_PURCHASE_RECRUIT"));
        _btnPurchase.onMouseClick(btnPurchaseClick);

        _btnSell.setText(tr("STR_SELL_SACK_UC"));
        _btnSell.onMouseClick(btnSellClick);

        _btnGeoscape.setText(tr("STR_GEOSCAPE_UC"));
        _btnGeoscape.onMouseClick(btnGeoscapeClick);
        _btnGeoscape.onKeyboardPress(btnGeoscapeClick, Options.keyCancel);
    }

    /**
     *
     */
    ~BasescapeState()
    {
        // Clean up any temporary bases
        bool exists = false;
        var bases = _game.getSavedGame().getBases();
        for (var i = 0; i < bases.Count && !exists; ++i)
        {
            if (bases[i] == _base)
            {
                exists = true;
                break;
            }
        }
        if (!exists)
        {
            _base = null;
        }
    }

    /**
     * Processes clicking on facilities.
     * @param action Pointer to an action.
     */
    void viewLeftClick(Action _)
    {
        BaseFacility fac = _view.getSelectedFacility();
        if (fac != null)
        {
            // Is facility in use?
            if (fac.inUse())
            {
                _game.pushState(new ErrorMessageState(tr("STR_FACILITY_IN_USE"), _palette, (byte)_game.getMod().getInterface("basescape").getElement("errorMessage").color, "BACK13.SCR", _game.getMod().getInterface("basescape").getElement("errorPalette").color));
            }
            // Would base become disconnected?
            else if (_base.getDisconnectedFacilities(fac).Any())
            {
                _game.pushState(new ErrorMessageState(tr("STR_CANNOT_DISMANTLE_FACILITY"), _palette, (byte)_game.getMod().getInterface("basescape").getElement("errorMessage").color, "BACK13.SCR", _game.getMod().getInterface("basescape").getElement("errorPalette").color));
            }
            else
            {
                _game.pushState(new DismantleFacilityState(_base, _view, fac));
            }
        }
    }

    /**
     * Processes right clicking on facilities.
     * @param action Pointer to an action.
     */
    void viewRightClick(Action _)
    {
        BaseFacility f = _view.getSelectedFacility();
        if (f == null)
        {
            _game.pushState(new BaseInfoState(_base, this));
        }
        else if (f.getRules().getCrafts() > 0)
        {
            if (f.getCraft() == null)
            {
                _game.pushState(new CraftsState(_base));
            }
            else
                for (int craft = 0; craft < _base.getCrafts().Count; ++craft)
                {
                    if (f.getCraft() == _base.getCrafts()[craft])
                    {
                        _game.pushState(new CraftInfoState(_base, (uint)craft));
                        break;
                    }
                }
        }
        else if (f.getRules().getStorage() > 0)
        {
            _game.pushState(new SellState(_base));
        }
        else if (f.getRules().getPersonnel() > 0)
        {
            _game.pushState(new SoldiersState(_base));
        }
        else if (f.getRules().getPsiLaboratories() > 0 && Options.anytimePsiTraining && _base.getAvailablePsiLabs() > 0)
        {
            _game.pushState(new AllocatePsiTrainingState(_base));
        }
        else if (f.getRules().getLaboratories() > 0)
        {
            _game.pushState(new ResearchState(_base));
        }
        else if (f.getRules().getWorkshops() > 0)
        {
            _game.pushState(new ManufactureState(_base));
        }
        else if (f.getRules().getAliens() > 0)
        {
            _game.pushState(new ManageAlienContainmentState(_base, OptionsOrigin.OPT_GEOSCAPE));
        }
        else if (f.getRules().isLift() || f.getRules().getRadarRange() > 0)
        {
            _game.popState();
        }
    }

    /**
     * Displays the name of the facility the mouse is over.
     * @param action Pointer to an action.
     */
    void viewMouseOver(Action _)
    {
        BaseFacility f = _view.getSelectedFacility();
        string ss = null;
        if (f != null)
        {
            if (f.getRules().getCrafts() == 0 || f.getBuildTime() > 0)
            {
                ss = tr(f.getRules().getType());
            }
            else
            {
                ss = tr(f.getRules().getType());
                if (f.getCraft() != null)
                {
                    ss = $"{ss} {tr("STR_CRAFT_").arg(f.getCraft().getName(_game.getLanguage()))}";
                }
            }
        }
        _txtFacility.setText(ss);
    }

    /**
     * Clears the facility name.
     * @param action Pointer to an action.
     */
    void viewMouseOut(Action _) =>
        _txtFacility.setText(string.Empty);

    /**
     * Selects a new base to display.
     * @param action Pointer to an action.
     */
    void miniClick(Action _)
    {
        var @base = _mini.getHoveredBase();
        if (@base < _game.getSavedGame().getBases().Count)
        {
            _base = _game.getSavedGame().getBases()[(int)@base];
            init();
        }
    }

    /**
     * Selects a new base to display.
     * @param action Pointer to an action.
     */
    void handleKeyPress(Action action)
    {
        if (action.getDetails().type == SDL_EventType.SDL_KEYDOWN)
        {
            SDL_Keycode[] baseKeys = {Options.keyBaseSelect1,
                             Options.keyBaseSelect2,
                             Options.keyBaseSelect3,
                             Options.keyBaseSelect4,
                             Options.keyBaseSelect5,
                             Options.keyBaseSelect6,
                             Options.keyBaseSelect7,
                             Options.keyBaseSelect8};
            var key = action.getDetails().key.keysym.sym;
            for (int i = 0; i < _game.getSavedGame().getBases().Count; ++i)
            {
                if (key == baseKeys[i])
                {
                    _base = _game.getSavedGame().getBases()[i];
                    init();
                    break;
                }
            }
        }
    }

    /**
     * Changes the Base name.
     * @param action Pointer to an action.
     */
    void edtBaseChange(Action _) =>
        _base.setName(_edtBase.getText());

    /**
     * Goes to the Build New Base screen.
     * @param action Pointer to an action.
     */
    void btnNewBaseClick(Action _)
    {
        Base @base = new Base(_game.getMod());
        _game.popState();
        _game.pushState(new BuildNewBaseState(@base, _globe, false));
    }

    /**
     * Goes to the Base Info screen.
     * @param action Pointer to an action.
     */
    void btnBaseInfoClick(Action _) =>
        _game.pushState(new BaseInfoState(_base, this));

    /**
     * Goes to the Soldiers screen.
     * @param action Pointer to an action.
     */
    void btnSoldiersClick(Action _) =>
        _game.pushState(new SoldiersState(_base));

    /**
     * Goes to the Crafts screen.
     * @param action Pointer to an action.
     */
    void btnCraftsClick(Action _) =>
        _game.pushState(new CraftsState(_base));

    /**
     * Opens the Build Facilities window.
     * @param action Pointer to an action.
     */
    void btnFacilitiesClick(Action _) =>
        _game.pushState(new BuildFacilitiesState(_base, this));

    /**
     * Goes to the Research screen.
     * @param action Pointer to an action.
     */
    void btnResearchClick(Action _) =>
        _game.pushState(new ResearchState(_base));

    /**
     * Goes to the Manufacture screen.
     * @param action Pointer to an action.
     */
    void btnManufactureClick(Action _) =>
        _game.pushState(new ManufactureState(_base));

    /**
     * Goes to the Purchase screen.
     * @param action Pointer to an action.
     */
    void btnPurchaseClick(Action _) =>
        _game.pushState(new PurchaseState(_base));

    /**
     * Goes to the Select Destination Base window.
     * @param action Pointer to an action.
     */
    void btnTransferClick(Action _) =>
        _game.pushState(new TransferBaseState(_base));

    /**
     * Goes to the Sell screen.
     * @param action Pointer to an action.
     */
    void btnSellClick(Action _) =>
        _game.pushState(new SellState(_base));

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnGeoscapeClick(Action _) =>
        _game.popState();

    /**
     * Changes the base currently displayed on screen.
     * @param base Pointer to new base to display.
     */
    internal void setBase(Base @base)
    {
        if (_game.getSavedGame().getBases().Any())
        {
            // Check if base still exists
            bool exists = false;
            for (uint i = 0; i < _game.getSavedGame().getBases().Count; ++i)
            {
                if (_game.getSavedGame().getBases()[(int)i] == @base)
                {
                    _base = @base;
                    _mini.setSelectedBase(i);
                    _game.getSavedGame().setSelectedBase(i);
                    exists = true;
                    break;
                }
            }
            // If base was removed, select first one
            if (!exists)
            {
                _base = _game.getSavedGame().getBases().First();
                _mini.setSelectedBase(0);
                _game.getSavedGame().setSelectedBase(0);
            }
        }
        else
        {
            // Use a blank base for special case when player has no bases
            _base = new Base(_game.getMod());
            _mini.setSelectedBase(0);
            _game.getSavedGame().setSelectedBase(0);
        }
    }

    /**
     * The player can change the selected base
     * or change info on other screens.
     */
    internal override void init()
    {
	    base.init();

	    setBase(_base);
	    _view.setBase(_base);
	    _mini.draw();
	    _edtBase.setText(_base.getName());

	    // Get area
	    foreach (var i in _game.getSavedGame().getRegions())
	    {
		    if (i.getRules().insideRegion(_base.getLongitude(), _base.getLatitude()))
		    {
			    _txtLocation.setText(tr(i.getRules().getType()));
			    break;
		    }
	    }

	    _txtFunds.setText(tr("STR_FUNDS").arg(Unicode.formatFunding(_game.getSavedGame().getFunds())));

	    _btnNewBase.setVisible(_game.getSavedGame().getBases().Count < MiniBaseView.MAX_BASES);
    }
}
