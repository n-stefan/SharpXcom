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
 * along with OpenXcom.  If not, see <http:///www.gnu.org/licenses/>.
 */

namespace SharpXcom.Geoscape;

/**
 * Screen that allows the player
 * to place a new base on the globe.
 */
internal class BuildNewBaseState : State
{
    Base _base;
    Globe _globe;
    bool _first;
    double _oldlat, _oldlon;
    int _mousex, _mousey;
    bool _oldshowradar;
    InteractiveSurface _btnRotateLeft, _btnRotateRight, _btnRotateUp, _btnRotateDown, _btnZoomIn, _btnZoomOut;
    Window _window;
    TextButton _btnCancel;
    Text _txtTitle;
    Timer _hoverTimer;

    /**
     * Initializes all the elements in the Build New Base window.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to place.
     * @param globe Pointer to the Geoscape globe.
     * @param first Is this the first base in the game?
     */
    internal BuildNewBaseState(Base @base, Globe globe, bool first)
    {
        _base = @base;
        _globe = globe;
        _first = first;
        _oldlat = 0;
        _oldlon = 0;
        _mousex = 0;
        _mousey = 0;

        int dx = _game.getScreen().getDX();
        int dy = _game.getScreen().getDY();
        _screen = false;

        _oldshowradar = Options.globeRadarLines;
        if (!_oldshowradar)
            Options.globeRadarLines = true;
        // Create objects
        _btnRotateLeft = new InteractiveSurface(12, 10, 259 + dx * 2, 176 + dy);
        _btnRotateRight = new InteractiveSurface(12, 10, 283 + dx * 2, 176 + dy);
        _btnRotateUp = new InteractiveSurface(13, 12, 271 + dx * 2, 162 + dy);
        _btnRotateDown = new InteractiveSurface(13, 12, 271 + dx * 2, 187 + dy);
        _btnZoomIn = new InteractiveSurface(23, 23, 295 + dx * 2, 156 + dy);
        _btnZoomOut = new InteractiveSurface(13, 17, 300 + dx * 2, 182 + dy);

        _window = new Window(this, 256, 28, 0, 0);
        _window.setX(dx);
        _window.setDY(0);
        _btnCancel = new TextButton(54, 12, 186 + dx, 8);
        _txtTitle = new Text(180, 16, 8 + dx, 6);

        _hoverTimer = new Timer(50);
        _hoverTimer.onTimer((StateHandler)hoverRedraw);
        _hoverTimer.start();

        // Set palette
        setInterface("geoscape");

        add(_btnRotateLeft);
        add(_btnRotateRight);
        add(_btnRotateUp);
        add(_btnRotateDown);
        add(_btnZoomIn);
        add(_btnZoomOut);

        add(_window, "genericWindow", "geoscape");
        add(_btnCancel, "genericButton2", "geoscape");
        add(_txtTitle, "genericText", "geoscape");

        // Set up objects
        _globe.onMouseClick(globeClick);

        _btnRotateLeft.onMousePress(btnRotateLeftPress);
        _btnRotateLeft.onMouseRelease(btnRotateLeftRelease);
        _btnRotateLeft.onKeyboardPress(btnRotateLeftPress, Options.keyGeoLeft);
        _btnRotateLeft.onKeyboardRelease(btnRotateLeftRelease, Options.keyGeoLeft);

        _btnRotateRight.onMousePress(btnRotateRightPress);
        _btnRotateRight.onMouseRelease(btnRotateRightRelease);
        _btnRotateRight.onKeyboardPress(btnRotateRightPress, Options.keyGeoRight);
        _btnRotateRight.onKeyboardRelease(btnRotateRightRelease, Options.keyGeoRight);

        _btnRotateUp.onMousePress(btnRotateUpPress);
        _btnRotateUp.onMouseRelease(btnRotateUpRelease);
        _btnRotateUp.onKeyboardPress(btnRotateUpPress, Options.keyGeoUp);
        _btnRotateUp.onKeyboardRelease(btnRotateUpRelease, Options.keyGeoUp);

        _btnRotateDown.onMousePress(btnRotateDownPress);
        _btnRotateDown.onMouseRelease(btnRotateDownRelease);
        _btnRotateDown.onKeyboardPress(btnRotateDownPress, Options.keyGeoDown);
        _btnRotateDown.onKeyboardRelease(btnRotateDownRelease, Options.keyGeoDown);

        _btnZoomIn.onMouseClick(btnZoomInLeftClick, (byte)SDL_BUTTON_LEFT);
        _btnZoomIn.onMouseClick(btnZoomInRightClick, (byte)SDL_BUTTON_RIGHT);
        _btnZoomIn.onKeyboardPress(btnZoomInLeftClick, Options.keyGeoZoomIn);

        _btnZoomOut.onMouseClick(btnZoomOutLeftClick, (byte)SDL_BUTTON_LEFT);
        _btnZoomOut.onMouseClick(btnZoomOutRightClick, (byte)SDL_BUTTON_RIGHT);
        _btnZoomOut.onKeyboardPress(btnZoomOutLeftClick, Options.keyGeoZoomOut);

        // dirty hacks to get the rotate buttons to work in "classic" style
        _btnRotateLeft.setListButton();
        _btnRotateRight.setListButton();
        _btnRotateUp.setListButton();
        _btnRotateDown.setListButton();

        _window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

        _btnCancel.setText(tr("STR_CANCEL_UC"));
        _btnCancel.onMouseClick(btnCancelClick);
        _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);

        _txtTitle.setText(tr("STR_SELECT_SITE_FOR_NEW_BASE"));
        _txtTitle.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
        _txtTitle.setWordWrap(true);

        if (_first)
        {
            _btnCancel.setVisible(false);
        }
    }

    /**
     *
     */
    ~BuildNewBaseState()
    {
        if (Options.globeRadarLines != _oldshowradar)
        {
            Options.globeRadarLines = false;
        }
        _hoverTimer = null;
    }

    void hoverRedraw()
    {
        double lon, lat;
        _globe.cartToPolar((short)_mousex, (short)_mousey, out lon, out lat);
        //if (lon == lon && lat == lat)
        //{
            _globe.setNewBaseHoverPos(lon, lat);
            _globe.setNewBaseHover(true);
        //}
        if (Options.globeRadarLines && !(AreSame(_oldlat, lat) && AreSame(_oldlon, lon)))
        {
            _oldlat = lat;
            _oldlon = lon;
            _globe.invalidate();
        }
    }

    /**
     * Processes any left-clicks for base placement,
     * or right-clicks to scroll the globe.
     * @param action Pointer to an action.
     */
    void globeClick(Action action)
    {
        double lon, lat;
        int mouseX = (int)Math.Floor(action.getAbsoluteXMouse()), mouseY = (int)Math.Floor(action.getAbsoluteYMouse());
        _globe.cartToPolar((short)mouseX, (short)mouseY, out lon, out lat);

        // Ignore window clicks
        if (mouseY < 28)
        {
            return;
        }

        // Clicking on a polygon for a base location
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            if (_globe.insideLand(lon, lat))
            {
                _base.setLongitude(lon);
                _base.setLatitude(lat);
                foreach (var i in _base.getCrafts())
                {
                    i.setLongitude(lon);
                    i.setLatitude(lat);
                }
                if (_first)
                {
                    _game.pushState(new BaseNameState(_base, _globe, _first));
                }
                else
                {
                    _game.pushState(new ConfirmNewBaseState(_base, _globe));
                }
            }
            else
            {
                _game.pushState(new ErrorMessageState(tr("STR_XCOM_BASE_CANNOT_BE_BUILT"), _palette, (byte)_game.getMod().getInterface("geoscape").getElement("genericWindow").color, "BACK01.SCR", _game.getMod().getInterface("geoscape").getElement("palette").color));
            }
        }
    }

    /**
     * Starts rotating the globe to the left.
     * @param action Pointer to an action.
     */
    void btnRotateLeftPress(Action _) =>
        _globe.rotateLeft();

    /**
     * Stops rotating the globe to the left.
     * @param action Pointer to an action.
     */
    void btnRotateLeftRelease(Action _) =>
        _globe.rotateStopLon();

    /**
     * Starts rotating the globe to the right.
     * @param action Pointer to an action.
     */
    void btnRotateRightPress(Action _) =>
        _globe.rotateRight();

    /**
     * Stops rotating the globe to the right.
     * @param action Pointer to an action.
     */
    void btnRotateRightRelease(Action _) =>
        _globe.rotateStopLon();

    /**
     * Starts rotating the globe upwards.
     * @param action Pointer to an action.
     */
    void btnRotateUpPress(Action _) =>
        _globe.rotateUp();

    /**
     * Stops rotating the globe upwards.
     * @param action Pointer to an action.
     */
    void btnRotateUpRelease(Action _) =>
        _globe.rotateStopLat();

    /**
     * Starts rotating the globe downwards.
     * @param action Pointer to an action.
     */
    void btnRotateDownPress(Action _) =>
        _globe.rotateDown();

    /**
     * Stops rotating the globe downwards.
     * @param action Pointer to an action.
     */
    void btnRotateDownRelease(Action _) =>
        _globe.rotateStopLat();

    /**
     * Zooms into the globe.
     * @param action Pointer to an action.
     */
    void btnZoomInLeftClick(Action _) =>
        _globe.zoomIn();

    /**
     * Zooms the globe maximum.
     * @param action Pointer to an action.
     */
    void btnZoomInRightClick(Action _) =>
        _globe.zoomMax();

    /**
     * Zooms out of the globe.
     * @param action Pointer to an action.
     */
    void btnZoomOutLeftClick(Action _) =>
        _globe.zoomOut();

    /**
     * Zooms the globe minimum.
     * @param action Pointer to an action.
     */
    void btnZoomOutRightClick(Action _) =>
        _globe.zoomMin();

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _)
    {
        _base = null;
        _game.popState();
    }
}
