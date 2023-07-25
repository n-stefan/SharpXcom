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
 * Screen that lets the user configure various
 * Geoscape options.
 */
internal class OptionsGeoscapeState : OptionsBaseState
{
    Text _txtDragScroll;
    ComboBox _cbxDragScroll;
    Text _txtScrollSpeed, _txtDogfightSpeed, _txtClockSpeed;
    Slider _slrScrollSpeed, _slrDogfightSpeed, _slrClockSpeed;
    Text _txtGlobeDetails;
    ToggleTextButton _btnGlobeCountries, _btnGlobeRadars, _btnGlobePaths;
    Text _txtOptions;
    ToggleTextButton _btnShowFunds;

    /**
     * Initializes all the elements in the Geoscape Options screen.
     * @param game Pointer to the core game.
     * @param origin Game section that originated this state.
     */
    internal OptionsGeoscapeState(OptionsOrigin origin) : base(origin)
    {
        setCategory(_btnGeoscape);

        // Create objects
        _txtDragScroll = new Text(114, 9, 206, 8);
        _cbxDragScroll = new ComboBox(this, 104, 16, 206, 18);

        _txtScrollSpeed = new Text(114, 9, 94, 8);
        _slrScrollSpeed = new Slider(104, 16, 94, 18);

        _txtDogfightSpeed = new Text(114, 9, 206, 40);
        _slrDogfightSpeed = new Slider(104, 16, 206, 50);

        _txtClockSpeed = new Text(114, 9, 94, 40);
        _slrClockSpeed = new Slider(104, 16, 94, 50);

        _txtGlobeDetails = new Text(114, 9, 94, 82);
        _btnGlobeCountries = new ToggleTextButton(104, 16, 94, 92);
        _btnGlobeRadars = new ToggleTextButton(104, 16, 94, 110);
        _btnGlobePaths = new ToggleTextButton(104, 16, 94, 128);

        _txtOptions = new Text(114, 9, 206, 82);
        _btnShowFunds = new ToggleTextButton(104, 16, 206, 92);

        add(_txtScrollSpeed, "text", "geoscapeMenu");
        add(_slrScrollSpeed, "button", "geoscapeMenu");

        add(_txtDogfightSpeed, "text", "geoscapeMenu");
        add(_slrDogfightSpeed, "button", "geoscapeMenu");

        add(_txtClockSpeed, "text", "geoscapeMenu");
        add(_slrClockSpeed, "button", "geoscapeMenu");

        add(_txtGlobeDetails, "text", "geoscapeMenu");
        add(_btnGlobeCountries, "button", "geoscapeMenu");
        add(_btnGlobeRadars, "button", "geoscapeMenu");
        add(_btnGlobePaths, "button", "geoscapeMenu");

        add(_txtOptions, "text", "geoscapeMenu");
        add(_btnShowFunds, "button", "geoscapeMenu");

        add(_txtDragScroll, "text", "geoscapeMenu");
        add(_cbxDragScroll, "button", "geoscapeMenu");

        centerAllSurfaces();

        // Set up objects
        _txtDragScroll.setText(tr("STR_DRAG_SCROLL"));

        var dragScrolls = new List<string>
        {
            tr("STR_DISABLED"),
            tr("STR_LEFT_MOUSE_BUTTON"),
            tr("STR_MIDDLE_MOUSE_BUTTON"),
            tr("STR_RIGHT_MOUSE_BUTTON")
        };

        _cbxDragScroll.setOptions(dragScrolls);
        _cbxDragScroll.setSelected((uint)Options.geoDragScrollButton);
        _cbxDragScroll.onChange(cbxDragScrollChange);
        _cbxDragScroll.setTooltip("STR_DRAG_SCROLL_DESC");
        _cbxDragScroll.onMouseIn(txtTooltipIn);
        _cbxDragScroll.onMouseOut(txtTooltipOut);

        _txtScrollSpeed.setText(tr("STR_SCROLL_SPEED"));

        _slrScrollSpeed.setRange(100, 10);
        _slrScrollSpeed.setValue(Options.geoScrollSpeed);
        _slrScrollSpeed.setTooltip("STR_SCROLL_SPEED_GEO_DESC");
        _slrScrollSpeed.onChange(slrScrollSpeedChange);
        _slrScrollSpeed.onMouseIn(txtTooltipIn);
        _slrScrollSpeed.onMouseOut(txtTooltipOut);

        _txtDogfightSpeed.setText(tr("STR_DOGFIGHT_SPEED"));

        _slrDogfightSpeed.setRange(50, 20);
        _slrDogfightSpeed.setValue(Options.dogfightSpeed);
        _slrDogfightSpeed.onChange(slrDogfightSpeedChange);
        _slrDogfightSpeed.setTooltip("STR_DOGFIGHT_SPEED_DESC");
        _slrDogfightSpeed.onMouseIn(txtTooltipIn);
        _slrDogfightSpeed.onMouseOut(txtTooltipOut);

        _txtClockSpeed.setText(tr("STR_CLOCK_SPEED"));

        _slrClockSpeed.setRange(250, 10);
        _slrClockSpeed.setValue(Options.geoClockSpeed);
        _slrClockSpeed.setTooltip("STR_CLOCK_SPEED_DESC");
        _slrClockSpeed.onChange(slrClockSpeedChange);
        _slrClockSpeed.onMouseIn(txtTooltipIn);
        _slrClockSpeed.onMouseOut(txtTooltipOut);

        _txtGlobeDetails.setText(tr("STR_GLOBE_DETAILS"));

        _btnGlobeCountries.setText(tr("STR_GLOBE_COUNTRIES"));
        _btnGlobeCountries.setPressed(Options.globeDetail);
        _btnGlobeCountries.onMouseClick(btnGlobeCountriesClick);
        _btnGlobeCountries.setTooltip("STR_GLOBE_COUNTRIES_DESC");
        _btnGlobeCountries.onMouseIn(txtTooltipIn);
        _btnGlobeCountries.onMouseOut(txtTooltipOut);

        _btnGlobeRadars.setText(tr("STR_GLOBE_RADARS"));
        _btnGlobeRadars.setPressed(Options.globeRadarLines);
        _btnGlobeRadars.onMouseClick(btnGlobeRadarsClick);
        _btnGlobeRadars.setTooltip("STR_GLOBE_RADARS_DESC");
        _btnGlobeRadars.onMouseIn(txtTooltipIn);
        _btnGlobeRadars.onMouseOut(txtTooltipOut);

        _btnGlobePaths.setText(tr("STR_GLOBE_FLIGHT_PATHS"));
        _btnGlobePaths.setPressed(Options.globeFlightPaths);
        _btnGlobePaths.onMouseClick(btnGlobePathsClick);
        _btnGlobePaths.setTooltip("STR_GLOBE_FLIGHT_PATHS_DESC");
        _btnGlobePaths.onMouseIn(txtTooltipIn);
        _btnGlobePaths.onMouseOut(txtTooltipOut);

        _txtOptions.setText(tr("STR_USER_INTERFACE_OPTIONS"));

        _btnShowFunds.setText(tr("STR_SHOW_FUNDS"));
        _btnShowFunds.setPressed(Options.showFundsOnGeoscape);
        _btnShowFunds.onMouseClick(btnShowFundsClick);
        _btnShowFunds.setTooltip("STR_SHOW_FUNDS_DESC");
        _btnShowFunds.onMouseIn(txtTooltipIn);
        _btnShowFunds.onMouseOut(txtTooltipOut);
    }

    /**
     *
     */
    ~OptionsGeoscapeState() { }

    /**
     * Changes the Drag Scroll option.
     * @param action Pointer to an action.
     */
    void cbxDragScrollChange(Action _) =>
        Options.geoDragScrollButton = (int)_cbxDragScroll.getSelected();

    /**
     * Updates the scroll speed.
     * @param action Pointer to an action.
     */
    void slrScrollSpeedChange(Action _) =>
        Options.geoScrollSpeed = _slrScrollSpeed.getValue();

    /**
     * Updates the dogfight speed.
     * @param action Pointer to an action.
     */
    void slrDogfightSpeedChange(Action _) =>
        Options.dogfightSpeed = _slrDogfightSpeed.getValue();

    /**
     * Updates the clock speed.
     * @param action Pointer to an action.
     */
    void slrClockSpeedChange(Action _) =>
        Options.geoClockSpeed = _slrClockSpeed.getValue();

    /**
     * Changes the Globe Country Borders option.
     * @param action Pointer to an action.
     */
    void btnGlobeCountriesClick(Action _) =>
        Options.globeDetail = _btnGlobeCountries.getPressed();

    /**
     * Changes the Globe Radar Ranges option.
     * @param action Pointer to an action.
     */
    void btnGlobeRadarsClick(Action _) =>
        Options.globeRadarLines = _btnGlobeRadars.getPressed();

    /**
     * Changes the Globe Flight Paths option.
     * @param action Pointer to an action.
     */
    void btnGlobePathsClick(Action _) =>
        Options.globeFlightPaths = _btnGlobePaths.getPressed();

    /**
     * Changes the Show Funds option.
     * @param action Pointer to an action.
     */
    void btnShowFundsClick(Action _) =>
        Options.showFundsOnGeoscape = _btnShowFunds.getPressed();
}
