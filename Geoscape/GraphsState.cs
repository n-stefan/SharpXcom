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

/* struct */ class GraphButInfo
{
    internal LocalizedText _name;
    internal int _color;
    internal bool _pushed;

    internal GraphButInfo(LocalizedText name, byte color)
    {
        _name = name;
        _color = color;
        _pushed = false;
    }
}

/**
 * Graphs screen for displaying graphs of various
 * monthly game data like activity and funding.
 */
internal class GraphsState : State
{
    const uint GRAPH_MAX_BUTTONS = 16;

    List<GraphButInfo> _regionToggles, _countryToggles;
    List<bool> _financeToggles;
    //will be only between 0 and Count
    uint _butRegionsOffset, _butCountriesOffset;
    InteractiveSurface _bg;
    InteractiveSurface _btnXcomRegion, _btnUfoRegion;
    InteractiveSurface _btnXcomCountry, _btnUfoCountry;
    InteractiveSurface _btnIncome, _btnFinance;
    InteractiveSurface _btnGeoscape;
    Text _txtTitle, _txtFactor;
    TextList _txtMonths, _txtYears;
    List<Text> _txtScale;
    List<ToggleTextButton> _btnRegions, _btnCountries, _btnFinances;
    List<Surface> _alienRegionLines, _alienCountryLines;
    List<Surface> _xcomRegionLines, _xcomCountryLines;
    ToggleTextButton _btnRegionTotal, _btnCountryTotal;
    List<Surface> _financeLines, _incomeLines;
    bool _alien, _income, _country, _finance;

    /**
     * Initializes all the elements in the Graphs screen.
     * @param game Pointer to the core game.
     */
    internal GraphsState()
    {
        _butRegionsOffset = 0;
        _butCountriesOffset = 0;

        // Create objects
        _bg = new InteractiveSurface(320, 200, 0, 0);
        _bg.onMousePress(shiftButtons, SDL_BUTTON_WHEELUP);
        _bg.onMousePress(shiftButtons, SDL_BUTTON_WHEELDOWN);
        _btnUfoRegion = new InteractiveSurface(32, 24, 96, 0);
        _btnUfoCountry = new InteractiveSurface(32, 24, 128, 0);
        _btnXcomRegion = new InteractiveSurface(32, 24, 160, 0);
        _btnXcomCountry = new InteractiveSurface(32, 24, 192, 0);
        _btnIncome = new InteractiveSurface(32, 24, 224, 0);
        _btnFinance = new InteractiveSurface(32, 24, 256, 0);
        _btnGeoscape = new InteractiveSurface(32, 24, 288, 0);
        _txtTitle = new Text(230, 16, 90, 28);
        _txtFactor = new Text(38, 11, 96, 28);
        _txtMonths = new TextList(205, 8, 115, 183);
        _txtYears = new TextList(200, 8, 121, 191);

        // Set palette
        setInterface("graphs");

        //add all our elements
        add(_bg);
        add(_btnUfoRegion);
        add(_btnUfoCountry);
        add(_btnXcomRegion);
        add(_btnXcomCountry);
        add(_btnIncome);
        add(_btnFinance);
        add(_btnGeoscape);
        add(_txtMonths, "scale", "graphs");
        add(_txtYears, "scale", "graphs");
        add(_txtTitle, "text", "graphs");
        add(_txtFactor, "text", "graphs");
        for (int scaleText = 0; scaleText != 10; ++scaleText)
        {
            _txtScale.Add(new Text(42, 16, 80, 171 - (scaleText * 14)));
            add(_txtScale[scaleText], "scale", "graphs");
        }
        byte regionTotalColor = (byte)_game.getMod().getInterface("graphs").getElement("regionTotal").color;
        byte countryTotalColor = (byte)_game.getMod().getInterface("graphs").getElement("countryTotal").color;

        //create buttons (sooooo many buttons)
        int offset = 0;
        foreach (var iter in _game.getSavedGame().getRegions())
        {
            // always save in toggles all the regions
            byte color = (byte)(13 + 8 * (offset % GRAPH_MAX_BUTTONS));
            _regionToggles.Add(new GraphButInfo(tr(iter.getRules().getType()), color));
            // initially add the GRAPH_MAX_BUTTONS having the first regions information
            if (offset < GRAPH_MAX_BUTTONS)
            {
                _btnRegions.Add(new ToggleTextButton(88, 11, 0, offset * 11));
                _btnRegions[offset].setText(tr(iter.getRules().getType()));
                _btnRegions[offset].setInvertColor(color);
                _btnRegions[offset].onMousePress(btnRegionListClick);
                add(_btnRegions[offset], "button", "graphs");
            }
            _alienRegionLines.Add(new Surface(320, 200, 0, 0));
            add(_alienRegionLines[offset]);
            _xcomRegionLines.Add(new Surface(320, 200, 0, 0));
            add(_xcomRegionLines[offset]);

            ++offset;
        }

        if (_regionToggles.Count < GRAPH_MAX_BUTTONS)
            _btnRegionTotal = new ToggleTextButton(88, 11, 0, _regionToggles.Count * 11);
        else
            _btnRegionTotal = new ToggleTextButton(88, 11, 0, (int)(GRAPH_MAX_BUTTONS * 11));
        _regionToggles.Add(new GraphButInfo(tr("STR_TOTAL_UC"), regionTotalColor));
        _btnRegionTotal.onMousePress(btnRegionListClick);
        _btnRegionTotal.setInvertColor(regionTotalColor);
        _btnRegionTotal.setText(tr("STR_TOTAL_UC"));
        _alienRegionLines.Add(new Surface(320, 200, 0, 0));
        add(_alienRegionLines[offset]);
        _xcomRegionLines.Add(new Surface(320, 200, 0, 0));
        add(_xcomRegionLines[offset]);
        add(_btnRegionTotal, "button", "graphs");

        offset = 0;
        foreach (var iter in _game.getSavedGame().getCountries())
        {
            // always save in toggles all the countries
            byte color = (byte)(13 + 8 * (offset % GRAPH_MAX_BUTTONS));
            _countryToggles.Add(new GraphButInfo(tr(iter.getRules().getType()), color));
            // initially add the GRAPH_MAX_BUTTONS having the first countries information
            if (offset < GRAPH_MAX_BUTTONS)
            {
                _btnCountries.Add(new ToggleTextButton(88, 11, 0, offset * 11));
                _btnCountries[offset].setInvertColor(color);
                _btnCountries[offset].setText(tr(iter.getRules().getType()));
                _btnCountries[offset].onMousePress(btnCountryListClick);
                add(_btnCountries[offset], "button", "graphs");
            }
            _alienCountryLines.Add(new Surface(320, 200, 0, 0));
            add(_alienCountryLines[offset]);
            _xcomCountryLines.Add(new Surface(320, 200, 0, 0));
            add(_xcomCountryLines[offset]);
            _incomeLines.Add(new Surface(320, 200, 0, 0));
            add(_incomeLines[offset]);

            ++offset;
        }

        if (_countryToggles.Count < GRAPH_MAX_BUTTONS)
            _btnCountryTotal = new ToggleTextButton(88, 11, 0, _countryToggles.Count * 11);
        else
            _btnCountryTotal = new ToggleTextButton(88, 11, 0, (int)(GRAPH_MAX_BUTTONS * 11));
        _countryToggles.Add(new GraphButInfo(tr("STR_TOTAL_UC"), countryTotalColor));
        _btnCountryTotal.onMousePress(btnCountryListClick);
        _btnCountryTotal.setInvertColor(countryTotalColor);
        _btnCountryTotal.setText(tr("STR_TOTAL_UC"));
        _alienCountryLines.Add(new Surface(320, 200, 0, 0));
        add(_alienCountryLines[offset]);
        _xcomCountryLines.Add(new Surface(320, 200, 0, 0));
        add(_xcomCountryLines[offset]);
        _incomeLines.Add(new Surface(320, 200, 0, 0));
        add(_incomeLines[offset]);
        add(_btnCountryTotal, "button", "graphs");

        for (int iter = 0; iter != 5; ++iter)
        {
            offset = iter;
            _btnFinances.Add(new ToggleTextButton(88, 11, 0, offset * 11));
            _financeToggles.Add(false);
            _btnFinances[offset].setInvertColor((byte)(13 + (8 * offset)));
            _btnFinances[offset].onMousePress(btnFinanceListClick);
            add(_btnFinances[offset], "button", "graphs");
            _financeLines.Add(new Surface(320, 200, 0, 0));
            add(_financeLines[offset]);
        }

        _btnFinances[0].setText(tr("STR_INCOME"));
        _btnFinances[1].setText(tr("STR_EXPENDITURE"));
        _btnFinances[2].setText(tr("STR_MAINTENANCE"));
        _btnFinances[3].setText(tr("STR_BALANCE"));
        _btnFinances[4].setText(tr("STR_SCORE"));

        // load back the button states
        var graphRegionToggles = new StringBuilder(_game.getSavedGame().getGraphRegionToggles());
        var graphCountryToggles = new StringBuilder(_game.getSavedGame().getGraphCountryToggles());
        var graphFinanceToggles = new StringBuilder(_game.getSavedGame().getGraphFinanceToggles());
        while (graphRegionToggles.Length < _regionToggles.Count) graphRegionToggles.Append('0');
        while (graphCountryToggles.Length < _countryToggles.Count) graphCountryToggles.Append('0');
        while (graphFinanceToggles.Length < _financeToggles.Count) graphFinanceToggles.Append('0');
        for (int i = 0; i < _regionToggles.Count; ++i)
        {
            _regionToggles[i]._pushed = ('0' == graphRegionToggles[i]) ? false : true;
            if (_regionToggles.Count - 1 == i)
                _btnRegionTotal.setPressed(_regionToggles[i]._pushed);
            else if (i < GRAPH_MAX_BUTTONS)
                _btnRegions[i].setPressed(_regionToggles[i]._pushed);
        }
        for (int i = 0; i < _countryToggles.Count; ++i)
        {
            _countryToggles[i]._pushed = ('0' == graphCountryToggles[i]) ? false : true;
            if (_countryToggles.Count - 1 == i)
                _btnCountryTotal.setPressed(_countryToggles[i]._pushed);
            else if (i < GRAPH_MAX_BUTTONS)
                _btnCountries[i].setPressed(_countryToggles[i]._pushed);
        }
        for (int i = 0; i < _financeToggles.Count; ++i)
        {
            _financeToggles[i] = ('0' == graphFinanceToggles[i]) ? false : true;
            _btnFinances[i].setPressed(_financeToggles[i]);
        }
        byte gridColor = (byte)_game.getMod().getInterface("graphs").getElement("graph").color;
        // set up the grid
        _bg.drawRect(125, 49, 188, 127, gridColor);

        for (int grid = 0; grid != 5; ++grid)
        {
            for (int y = 50 + grid; y <= 163 + grid; y += 14)
            {
                for (int x = 126 + grid; x <= 297 + grid; x += 17)
                {
                    byte color = (byte)(gridColor + grid + 1);
                    if (grid == 4)
                    {
                        color = 0;
                    }
                    _bg.drawRect((short)x, (short)y, (short)(16 - (grid * 2)), (short)(13 - (grid * 2)), color);
                }
            }
        }

        //set up the horizontal measurement units
        string[] months = { "STR_JAN", "STR_FEB", "STR_MAR", "STR_APR", "STR_MAY", "STR_JUN", "STR_JUL", "STR_AUG", "STR_SEP", "STR_OCT", "STR_NOV", "STR_DEC" };
        int month = _game.getSavedGame().getTime().getMonth();
        // i know using textlist for this is ugly and brutal, but YOU try getting this damn text to line up.
        // also, there's nothing wrong with being ugly or brutal, you should learn tolerance.
        _txtMonths.setColumns(12, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17);
        _txtMonths.addRow(12, " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ");
        _txtYears.setColumns(6, 34, 34, 34, 34, 34, 34);
        _txtYears.addRow(6, " ", " ", " ", " ", " ", " ");

        for (int iter = 0; iter != 12; ++iter)
        {
            if (month > 11)
            {
                month = 0;
                string ss = _game.getSavedGame().getTime().getYear().ToString();
                _txtYears.setCellText(0, (uint)(iter / 2), ss);
                if (iter > 2)
                {
                    string ss2 = (_game.getSavedGame().getTime().getYear() - 1).ToString();
                    _txtYears.setCellText(0, 0, ss2);
                }
            }
            _txtMonths.setCellText(0, (uint)iter, tr(months[month]));
            ++month;
        }

        // set up the vertical measurement units
        foreach (var iter in _txtScale)
        {
            iter.setAlign(TextHAlign.ALIGN_RIGHT);
        }
        btnUfoRegionClick(null);

        // Set up objects
        if (_game.getMod().getSurface("GRAPH.BDY", false) != null)
        {
            _game.getMod().getSurface("GRAPH.BDY").blit(_bg);
        }
        else
        {
            _game.getMod().getSurface("GRAPHS.SPK").blit(_bg);
        }

        _txtTitle.setAlign(TextHAlign.ALIGN_CENTER);

        _txtFactor.setText(tr("STR_FINANCE_THOUSANDS"));

        // Set up buttons
        _btnUfoRegion.onMousePress(btnUfoRegionClick);
        _btnUfoCountry.onMousePress(btnUfoCountryClick);
        _btnXcomRegion.onMousePress(btnXcomRegionClick);
        _btnXcomCountry.onMousePress(btnXcomCountryClick);
        _btnIncome.onMousePress(btnIncomeClick);
        _btnFinance.onMousePress(btnFinanceClick);
        _btnGeoscape.onMousePress(btnGeoscapeClick);
        _btnGeoscape.onKeyboardPress(btnGeoscapeClick, Options.keyCancel);
        _btnGeoscape.onKeyboardPress(btnGeoscapeClick, Options.keyGeoGraphs);

        centerAllSurfaces();
    }

    /**
     *
     */
    ~GraphsState()
    {
        var graphRegionToggles = new StringBuilder();
        var graphCountryToggles = new StringBuilder();
        var graphFinanceToggles = new StringBuilder();
        for (int i = 0; i < _regionToggles.Count; ++i)
        {
            graphRegionToggles.Append(_regionToggles[i]._pushed ? '1' : '0');
            _regionToggles[i] = default;
        }
        for (int i = 0; i < _countryToggles.Count; ++i)
        {
            graphCountryToggles.Append(_countryToggles[i]._pushed ? '1' : '0');
            _countryToggles[i] = default;
        }
        for (int i = 0; i < _financeToggles.Count; ++i)
        {
            graphFinanceToggles.Append(_financeToggles[i] ? '1' : '0');
        }
        _game.getSavedGame().setGraphRegionToggles(graphRegionToggles.ToString());
        _game.getSavedGame().setGraphCountryToggles(graphCountryToggles.ToString());
        _game.getSavedGame().setGraphFinanceToggles(graphFinanceToggles.ToString());
    }

    /**
     * 'Shift' the buttons to display only GRAPH_MAX_BUTTONS - reset their state from toggles
     */
    void shiftButtons(Action action)
    {
        // only if active 'screen' is other than finance
        if (_finance)
            return;
        // select the data's we'll processing - regions or countries
        if (_country)
        {
            // too few countries? - return
            if (_countryToggles.Count <= GRAPH_MAX_BUTTONS)
                return;
            else if (action.getDetails().wheel.y > 0) //button.button == SDL_BUTTON_WHEELUP
                scrollButtons(_countryToggles, _btnCountries, _butCountriesOffset, -1);
            else if (action.getDetails().wheel.y < 0) //button.button == SDL_BUTTON_WHEELDOWN
                scrollButtons(_countryToggles, _btnCountries, _butCountriesOffset, 1);
        }
        else
        {
            // too few regions? - return
            if (_regionToggles.Count <= GRAPH_MAX_BUTTONS)
                return;
            else if (action.getDetails().wheel.y > 0) //button.button == SDL_BUTTON_WHEELUP
                scrollButtons(_regionToggles, _btnRegions, _butRegionsOffset, -1);
            else if (action.getDetails().wheel.y < 0) //button.button == SDL_BUTTON_WHEELDOWN
                scrollButtons(_regionToggles, _btnRegions, _butRegionsOffset, 1);
        }
    }

    void scrollButtons(List<GraphButInfo> toggles, List<ToggleTextButton> buttons, uint offset, int step)
    {
        if ((step + (int)offset) < 0 || offset + step + GRAPH_MAX_BUTTONS >= toggles.Count)
            return;
        // set the next offset - cheaper to do it from starters
        offset += (uint)step;
        uint i = 0;
        var iterb = 0;
        for (var itert = (int)offset; itert < toggles.Count && i < GRAPH_MAX_BUTTONS; ++itert, ++iterb, ++i)
        {
            updateButton(toggles[itert], buttons[iterb]);
        }
    }

    void updateButton(GraphButInfo from, ToggleTextButton to)
    {
        to.setText(from._name);
        to.setInvertColor((byte)from._color);
        to.setPressed(from._pushed);
    }

    /**
     * Handles a click on a region button.
     * @param action Pointer to an action.
     */
    void btnRegionListClick(Action action)
    {
        int number = 0;
        ToggleTextButton button = (ToggleTextButton)action.getSender();

        if (button == _btnRegionTotal)
        {
            number = _regionToggles.Count - 1;
        }
        else
        {
            for (int i = 0; i < _btnRegions.Count; ++i)
            {
                if (button == _btnRegions[i])
                {
                    number = (int)(i + _butRegionsOffset);
                    break;
                }
            }
        }

        _regionToggles[number]._pushed = button.getPressed();

        drawLines();
    }

    /**
     * Handles a click on a country button.
     * @param action Pointer to an action.
     */
    void btnCountryListClick(Action action)
    {
        int number = 0;
        ToggleTextButton button = (ToggleTextButton)action.getSender();

        if (button == _btnCountryTotal)
        {
            number = _countryToggles.Count - 1;
        }
        else
        {
            for (int i = 0; i < _btnCountries.Count; ++i)
            {
                if (button == _btnCountries[i])
                {
                    number = (int)(i + _butCountriesOffset);
                    break;
                }
            }
        }

        _countryToggles[number]._pushed = button.getPressed();

        drawLines();
    }

    /**
     * handles a click on a finances button.
     * @param action Pointer to an action.
     */
    void btnFinanceListClick(Action action)
    {
        int number = 0;
        ToggleTextButton button = (ToggleTextButton)action.getSender();

        for (int i = 0; i < _btnFinances.Count; ++i)
        {
            if (button == _btnFinances[i])
            {
                number = i;
                break;
            }
        }

        _financeLines[number].setVisible(!_financeToggles[number]);
        _financeToggles[number] = button.getPressed();

        drawLines();
    }

    /**
     * Switches to the UFO Region Activity screen.
     * @param action Pointer to an action.
     */
    void btnUfoRegionClick(Action _)
    {
        _alien = true;
        _income = false;
        _country = false;
        _finance = false;
        resetScreen();
        drawLines();
        foreach (var iter in _btnRegions)
        {
            iter.setVisible(true);
        }
        _btnRegionTotal.setVisible(true);
        _txtTitle.setBig();
        _txtTitle.setText(tr("STR_UFO_ACTIVITY_IN_AREAS"));
    }

    /**
     * Switches to the UFO Country activity screen.
     * @param action Pointer to an action.
     */
    void btnUfoCountryClick(Action _)
    {
        _alien = true;
        _income = false;
        _country = true;
        _finance = false;
        resetScreen();
        drawLines();
        foreach (var iter in _btnCountries)
        {
            iter.setVisible(true);
        }
        _btnCountryTotal.setVisible(true);
        _txtTitle.setBig();
        _txtTitle.setText(tr("STR_UFO_ACTIVITY_IN_COUNTRIES"));
    }

    /**
     * Switches to the XCom Region activity screen.
     * @param action Pointer to an action.
     */
    void btnXcomRegionClick(Action _)
    {
        _alien = false;
        _income = false;
        _country = false;
        _finance = false;
        resetScreen();
        drawLines();
        foreach (var iter in _btnRegions)
        {
            iter.setVisible(true);
        }
        _btnRegionTotal.setVisible(true);
        _txtTitle.setBig();
        _txtTitle.setText(tr("STR_XCOM_ACTIVITY_IN_AREAS"));
    }

    /**
     * Switches to the XCom Country activity screen.
     * @param action Pointer to an action.
     */
    void btnXcomCountryClick(Action _)
    {
        _alien = false;
        _income = false;
        _country = true;
        _finance = false;
        resetScreen();
        drawLines();
        foreach (var iter in _btnCountries)
        {
            iter.setVisible(true);
        }
        _btnCountryTotal.setVisible(true);
        _txtTitle.setBig();
        _txtTitle.setText(tr("STR_XCOM_ACTIVITY_IN_COUNTRIES"));
    }

    /**
     * Switches to the Income screen.
     * @param action Pointer to an action.
     */
    void btnIncomeClick(Action _)
    {
        _alien = false;
        _income = true;
        _country = true;
        _finance = false;
        resetScreen();
        drawLines();
        _txtFactor.setVisible(true);
        foreach (var iter in _btnCountries)
        {
            iter.setVisible(true);
        }
        _btnCountryTotal.setVisible(true);
        _txtTitle.setBig();
        _txtTitle.setText(tr("STR_INCOME"));
    }

    /**
     * Switches to the Finances screen.
     * @param action Pointer to an action.
     */
    void btnFinanceClick(Action _)
    {
        _alien = false;
        _income = false;
        _country = false;
        _finance = true;
        resetScreen();
        drawLines();
        foreach (var iter in _btnFinances)
        {
            iter.setVisible(true);
        }
        _txtTitle.setBig();
        _txtTitle.setText(tr("STR_FINANCE"));
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnGeoscapeClick(Action _) =>
        _game.popState();

    /**
     * remove all elements from view
     */
    void resetScreen()
    {
        foreach (var iter in _alienRegionLines)
        {
            iter.setVisible(false);
        }
        foreach (var iter in _alienCountryLines)
        {
            iter.setVisible(false);
        }
        foreach (var iter in _xcomRegionLines)
        {
            iter.setVisible(false);
        }
        foreach (var iter in _xcomCountryLines)
        {
            iter.setVisible(false);
        }
        foreach (var iter in _incomeLines)
        {
            iter.setVisible(false);
        }
        foreach (var iter in _financeLines)
        {
            iter.setVisible(false);
        }

        foreach (var iter in _btnRegions)
        {
            iter.setVisible(false);
        }
        foreach (var iter in _btnCountries)
        {
            iter.setVisible(false);
        }
        foreach (var iter in _btnFinances)
        {
            iter.setVisible(false);
        }

        _btnRegionTotal.setVisible(false);
        _btnCountryTotal.setVisible(false);
        _txtFactor.setVisible(false);
    }

    /**
     * instead of having all our line drawing in one giant ridiculous routine, just use the one we need.
     */
    void drawLines()
    {
        if (!_country && !_finance)
        {
            drawRegionLines();
        }
        else if (!_finance)
        {
            drawCountryLines();
        }
        else
        {
            drawFinanceLines();
        }
    }

    /**
     * Sets up the screens and draws the lines for region buttons
     * to toggle on and off
     */
    void drawRegionLines()
    {
        //calculate the totals, and set up our upward maximum
        int upperLimit = 0;
        int lowerLimit = 0;
        int[] totals = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        for (int entry = 0; entry != _game.getSavedGame().getFundsList().Count; ++entry)
        {
            int total = 0;
            if (_alien)
            {
                for (int iter = 0; iter != _game.getSavedGame().getRegions().Count; ++iter)
                {
                    total += _game.getSavedGame().getRegions()[iter].getActivityAlien()[entry];
                    if (_game.getSavedGame().getRegions()[iter].getActivityAlien()[entry] > upperLimit && _regionToggles[iter]._pushed)
                    {
                        upperLimit = _game.getSavedGame().getRegions()[iter].getActivityAlien()[entry];
                    }
                    if (_game.getSavedGame().getRegions()[iter].getActivityAlien()[entry] < lowerLimit && _regionToggles[iter]._pushed)
                    {
                        lowerLimit = _game.getSavedGame().getRegions()[iter].getActivityAlien()[entry];
                    }
                }
            }
            else
            {
                for (int iter = 0; iter != _game.getSavedGame().getRegions().Count; ++iter)
                {
                    total += _game.getSavedGame().getRegions()[iter].getActivityXcom()[entry];
                    if (_game.getSavedGame().getRegions()[iter].getActivityXcom()[entry] > upperLimit && _regionToggles[iter]._pushed)
                    {
                        upperLimit = _game.getSavedGame().getRegions()[iter].getActivityXcom()[entry];
                    }
                    if (_game.getSavedGame().getRegions()[iter].getActivityXcom()[entry] < lowerLimit && _regionToggles[iter]._pushed)
                    {
                        lowerLimit = _game.getSavedGame().getRegions()[iter].getActivityXcom()[entry];
                    }
                }
            }
            if (_regionToggles.Last()._pushed && total > upperLimit)
                upperLimit = total;
        }

        //adjust the scale to fit the upward maximum
        double range = upperLimit - lowerLimit;
        double low = lowerLimit;
        int check = 10;
        int grids = 9; // cells in grid
        while (range > check * grids)
        {
            check *= 2;
        }

        lowerLimit = 0;
        upperLimit = check * grids;

        if (low < 0)
        {
            while (low < lowerLimit)
            {
                lowerLimit -= check;
                upperLimit -= check;
            }
        }
        range = upperLimit - lowerLimit;
        double units = range / 126;
        // draw region lines
        for (int entry = 0; entry != _game.getSavedGame().getRegions().Count; ++entry)
        {
            Region region = _game.getSavedGame().getRegions()[entry];
            _alienRegionLines[entry].clear();
            _xcomRegionLines[entry].clear();
            var newLineVector = new List<short>();
            int reduction = 0;
            for (int iter = 0; iter != 12; ++iter)
            {
                int x = 312 - (iter * 17);
                int y = (int)(175 - (-lowerLimit / units));
                if (_alien)
                {
                    if (iter < region.getActivityAlien().Count)
                    {
                        reduction = (int)(region.getActivityAlien()[region.getActivityAlien().Count - (1 + iter)] / units);
                        y -= reduction;
                        totals[iter] += region.getActivityAlien()[region.getActivityAlien().Count - (1 + iter)];
                    }
                }
                else
                {
                    if (iter < region.getActivityXcom().Count)
                    {
                        reduction = (int)(region.getActivityXcom()[region.getActivityXcom().Count - (1 + iter)] / units);
                        y -= reduction;
                        totals[iter] += region.getActivityXcom()[region.getActivityXcom().Count - (1 + iter)];
                    }
                }
                if (y >= 175)
                    y = 175;
                newLineVector.Add((short)y);
                if (newLineVector.Count > 1 && _alien)
                    _alienRegionLines[entry].drawLine((short)x, (short)y, (short)(x + 17), newLineVector[newLineVector.Count - 2], (byte)(_regionToggles[entry]._color + 4));
                else if (newLineVector.Count > 1)
                    _xcomRegionLines[entry].drawLine((short)x, (short)y, (short)(x + 17), newLineVector[newLineVector.Count - 2], (byte)(_regionToggles[entry]._color + 4));
            }

            if (_alien)
                _alienRegionLines[entry].setVisible(_regionToggles[entry]._pushed);
            else
                _xcomRegionLines[entry].setVisible(_regionToggles[entry]._pushed);
        }

        // set up the "total" line
        if (_alien)
            _alienRegionLines.Last().clear();
        else
            _xcomRegionLines.Last().clear();

        byte color = (byte)_game.getMod().getInterface("graphs").getElement("regionTotal").color2;
        var newLineVector2 = new List<short>();
        for (int iter = 0; iter != 12; ++iter)
        {
            int x = 312 - (iter * 17);
            int y = (int)(175 - (-lowerLimit / units));
            if (totals[iter] > 0)
            {
                int reduction = (int)(totals[iter] / units);
                y -= reduction;
            }
            newLineVector2.Add((short)y);
            if (newLineVector2.Count > 1)
            {
                if (_alien)
                    _alienRegionLines.Last().drawLine((short)x, (short)y, (short)(x + 17), newLineVector2[newLineVector2.Count - 2], color);
                else
                    _xcomRegionLines.Last().drawLine((short)x, (short)y, (short)(x + 17), newLineVector2[newLineVector2.Count - 2], color);
            }
        }
        if (_alien)
            _alienRegionLines.Last().setVisible(_regionToggles.Last()._pushed);
        else
            _xcomRegionLines.Last().setVisible(_regionToggles.Last()._pushed);
        updateScale(lowerLimit, upperLimit);
        _txtFactor.setVisible(false);
    }

    /**
     * updates the text on the vertical scale
     * @param lowerLimit minimum value
     * @param upperLimit maximum value
     */
    void updateScale(double lowerLimit, double upperLimit)
    {
        double increment = ((upperLimit - lowerLimit) / 9);
        if (increment < 10)
        {
            increment = 10;
        }
        double text = lowerLimit;
        for (int i = 0; i < 10; ++i)
        {
            _txtScale[i].setText(Unicode.formatNumber((int)text));
            text += increment;
        }
    }

    /**
     * Sets up the screens and draws the lines for country buttons
     * to toggle on and off
     */
    void drawCountryLines()
    {
        //calculate the totals, and set up our upward maximum
        int upperLimit = 0;
        int lowerLimit = 0;
        int[] totals = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        for (int entry = 0; entry != _game.getSavedGame().getFundsList().Count; ++entry)
        {
            int total = 0;
            if (_alien)
            {
                for (int iter = 0; iter != _game.getSavedGame().getCountries().Count; ++iter)
                {
                    total += _game.getSavedGame().getCountries()[iter].getActivityAlien()[entry];
                    if (_game.getSavedGame().getCountries()[iter].getActivityAlien()[entry] > upperLimit && _countryToggles[iter]._pushed)
                    {
                        upperLimit = _game.getSavedGame().getCountries()[iter].getActivityAlien()[entry];
                    }
                }
            }
            else if (_income)
            {
                for (int iter = 0; iter != _game.getSavedGame().getCountries().Count; ++iter)
                {
                    total += _game.getSavedGame().getCountries()[iter].getFunding()[entry] / 1000;
                    if (_game.getSavedGame().getCountries()[iter].getFunding()[entry] / 1000 > upperLimit && _countryToggles[iter]._pushed)
                    {
                        upperLimit = _game.getSavedGame().getCountries()[iter].getFunding()[entry] / 1000;
                    }
                }
            }
            else
            {
                for (int iter = 0; iter != _game.getSavedGame().getCountries().Count; ++iter)
                {
                    total += _game.getSavedGame().getCountries()[iter].getActivityXcom()[entry];
                    if (_game.getSavedGame().getCountries()[iter].getActivityXcom()[entry] > upperLimit && _countryToggles[iter]._pushed)
                    {
                        upperLimit = _game.getSavedGame().getCountries()[iter].getActivityXcom()[entry];
                    }
                    if (_game.getSavedGame().getCountries()[iter].getActivityXcom()[entry] < lowerLimit && _countryToggles[iter]._pushed)
                    {
                        lowerLimit = _game.getSavedGame().getCountries()[iter].getActivityXcom()[entry];
                    }

                }
            }
            if (_countryToggles.Last()._pushed && total > upperLimit)
                upperLimit = total;
        }

        //adjust the scale to fit the upward maximum
        double range = upperLimit - lowerLimit;
        double low = lowerLimit;
        int grids = 9; // cells in grid
        int check = _income ? 50 : 10;
        while (range > check * grids)
        {
            check *= 2;
        }

        lowerLimit = 0;
        upperLimit = check * grids;

        if (low < 0)
        {
            while (low < lowerLimit)
            {
                lowerLimit -= check;
                upperLimit -= check;
            }
        }

        range = upperLimit - lowerLimit;
        double units = range / 126;

        // draw country lines
        for (int entry = 0; entry != _game.getSavedGame().getCountries().Count; ++entry)
        {
            Country country = _game.getSavedGame().getCountries()[entry];
            _alienCountryLines[entry].clear();
            _xcomCountryLines[entry].clear();
            _incomeLines[entry].clear();
            var newLineVector = new List<short>();
            int reduction = 0;
            for (int iter = 0; iter != 12; ++iter)
            {
                int x = 312 - (iter * 17);
                int y = (int)(175 - (-lowerLimit / units));
                if (_alien)
                {
                    if (iter < country.getActivityAlien().Count)
                    {
                        reduction = (int)(country.getActivityAlien()[country.getActivityAlien().Count - (1 + iter)] / units);
                        y -= reduction;
                        totals[iter] += country.getActivityAlien()[country.getActivityAlien().Count - (1 + iter)];
                    }
                }
                else if (_income)
                {
                    if (iter < country.getFunding().Count)
                    {
                        reduction = (int)((country.getFunding()[country.getFunding().Count - (1 + iter)] / 1000) / units);
                        y -= reduction;
                        totals[iter] += country.getFunding()[country.getFunding().Count - (1 + iter)] / 1000;
                    }
                }
                else
                {
                    if (iter < country.getActivityXcom().Count)
                    {
                        reduction = (int)(country.getActivityXcom()[country.getActivityXcom().Count - (1 + iter)] / units);
                        y -= reduction;
                        totals[iter] += country.getActivityXcom()[country.getActivityXcom().Count - (1 + iter)];
                    }
                }
                if (y >= 175)
                    y = 175;
                newLineVector.Add((short)y);
                if (newLineVector.Count > 1 && _alien)
                    _alienCountryLines[entry].drawLine((short)x, (short)y, (short)(x + 17), newLineVector[newLineVector.Count - 2], (byte)(_countryToggles[entry]._color + 4));
                else if (newLineVector.Count > 1 && _income)
                    _incomeLines[entry].drawLine((short)x, (short)y, (short)(x + 17), newLineVector[newLineVector.Count - 2], (byte)(_countryToggles[entry]._color + 4));
                else if (newLineVector.Count > 1)
                    _xcomCountryLines[entry].drawLine((short)x, (short)y, (short)(x + 17), newLineVector[newLineVector.Count - 2], (byte)(_countryToggles[entry]._color + 4));
            }
            if (_alien)
                _alienCountryLines[entry].setVisible(_countryToggles[entry]._pushed);
            else if (_income)
                _incomeLines[entry].setVisible(_countryToggles[entry]._pushed);
            else
                _xcomCountryLines[entry].setVisible(_countryToggles[entry]._pushed);
        }
        if (_alien)
            _alienCountryLines.Last().clear();
        else if (_income)
            _incomeLines.Last().clear();
        else
            _xcomCountryLines.Last().clear();

        // set up the "total" line
        var newLineVector2 = new List<short>();
        byte color = (byte)_game.getMod().getInterface("graphs").getElement("countryTotal").color2;
        for (int iter = 0; iter != 12; ++iter)
        {
            int x = 312 - (iter * 17);
            int y = (int)(175 - (-lowerLimit / units));
            if (totals[iter] > 0)
            {
                int reduction = (int)(totals[iter] / units);
                y -= reduction;
            }
            newLineVector2.Add((short)y);
            if (newLineVector2.Count > 1)
            {
                if (_alien)
                    _alienCountryLines.Last().drawLine((short)x, (short)y, (short)(x + 17), newLineVector2[newLineVector2.Count - 2], color);
                else if (_income)
                    _incomeLines.Last().drawLine((short)x, (short)y, (short)(x + 17), newLineVector2[newLineVector2.Count - 2], color);
                else
                    _xcomCountryLines.Last().drawLine((short)x, (short)y, (short)(x + 17), newLineVector2[newLineVector2.Count - 2], color);
            }
        }
        if (_alien)
            _alienCountryLines.Last().setVisible(_countryToggles.Last()._pushed);
        else if (_income)
            _incomeLines.Last().setVisible(_countryToggles.Last()._pushed);
        else
            _xcomCountryLines.Last().setVisible(_countryToggles.Last()._pushed);
        updateScale(lowerLimit, upperLimit);
        _txtFactor.setVisible(_income);
    }

    /**
     * Sets up the screens and draws the lines for the finance buttons
     * to toggle on and off
     */
    void drawFinanceLines()
    {
        //set up arrays
        int upperLimit = 0;
        int lowerLimit = 0;
        long[] incomeTotals = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        long[] balanceTotals = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        long[] expendTotals = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        long[] maintTotals = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] scoreTotals = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        maintTotals[0] = _game.getSavedGame().getBaseMaintenance() / 1000;

        // start filling those arrays with score values
        // determine which is the highest one being displayed, so we can adjust the scale
        for (int entry = 0; entry != _game.getSavedGame().getFundsList().Count; ++entry)
        {
            int invertedEntry = _game.getSavedGame().getFundsList().Count - (1 + entry);
            maintTotals[entry] += _game.getSavedGame().getMaintenances()[invertedEntry] / 1000;
            balanceTotals[entry] = _game.getSavedGame().getFundsList()[invertedEntry] / 1000;
            scoreTotals[entry] = _game.getSavedGame().getResearchScores()[invertedEntry];

            foreach (var iter in _game.getSavedGame().getRegions())
            {
                scoreTotals[entry] += iter.getActivityXcom()[invertedEntry] - iter.getActivityAlien()[invertedEntry];
            }

            if (_financeToggles[2])
            {
                if (maintTotals[entry] > upperLimit)
                {
                    upperLimit = (int)maintTotals[entry];
                }
                if (maintTotals[entry] < lowerLimit)
                {
                    lowerLimit = (int)maintTotals[entry];
                }
            }
            if (_financeToggles[3])
            {
                if (balanceTotals[entry] > upperLimit)
                {
                    upperLimit = (int)balanceTotals[entry];
                }
                if (balanceTotals[entry] < lowerLimit)
                {
                    lowerLimit = (int)balanceTotals[entry];
                }
            }
            if (_financeToggles[4])
            {
                if (scoreTotals[entry] > upperLimit)
                {
                    upperLimit = scoreTotals[entry];
                }
                if (scoreTotals[entry] < lowerLimit)
                {
                    lowerLimit = scoreTotals[entry];
                }
            }
        }

        for (int entry = 0; entry != _game.getSavedGame().getExpenditures().Count; ++entry)
        {
            expendTotals[entry] = _game.getSavedGame().getExpenditures()[_game.getSavedGame().getExpenditures().Count - (entry + 1)] / 1000;
            incomeTotals[entry] = _game.getSavedGame().getIncomes()[_game.getSavedGame().getIncomes().Count - (entry + 1)] / 1000;

            if (_financeToggles[0] && incomeTotals[entry] > upperLimit)
            {
                upperLimit = (int)incomeTotals[entry];
            }
            if (_financeToggles[1] && expendTotals[entry] > upperLimit)
            {
                upperLimit = (int)expendTotals[entry];
            }
        }

        double range = upperLimit - lowerLimit;
        double low = lowerLimit;
        int check = 250;
        int grids = 9; // cells in grid
        while (range > check * grids)
        {
            check *= 2;
        }

        lowerLimit = 0;
        upperLimit = check * grids;

        if (low < 0)
        {
            while (low < lowerLimit)
            {
                lowerLimit -= check;
                upperLimit -= check;
            }
        }
        //toggle screens
        for (int button = 0; button != 5; ++button)
        {
            _financeLines[button].setVisible(_financeToggles[button]);
            _financeLines[button].clear();
        }
        range = upperLimit - lowerLimit;
        //figure out how many units to the pixel, then plot the points for the graph and connect the dots.
        double units = range / 126;
        for (int button = 0; button != 5; ++button)
        {
            var newLineVector = new List<short>();
            for (int iter = 0; iter != 12; ++iter)
            {
                int x = 312 - (iter * 17);
                int y = (int)(175 - (-lowerLimit / units));
                int reduction = 0;
                switch (button)
                {
                    case 0:
                        reduction = (int)(incomeTotals[iter] / units);
                        break;
                    case 1:
                        reduction = (int)(expendTotals[iter] / units);
                        break;
                    case 2:
                        reduction = (int)(maintTotals[iter] / units);
                        break;
                    case 3:
                        reduction = (int)(balanceTotals[iter] / units);
                        break;
                    case 4:
                        reduction = (int)(scoreTotals[iter] / units);
                        break;
                }
                y -= reduction;
                newLineVector.Add((short)y);
                int offset = button % 2 != 0 ? 8 : 0;
                if (newLineVector.Count > 1)
                    _financeLines[button].drawLine((short)x, (short)y, (short)(x + 17), newLineVector[newLineVector.Count - 2], (byte)(Palette.blockOffset((byte)((button / 2) + 1)) + offset));
            }
        }
        updateScale(lowerLimit, upperLimit);
        _txtFactor.setVisible(true);
    }
}
