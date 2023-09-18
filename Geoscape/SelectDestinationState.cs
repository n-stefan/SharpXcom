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
 * to pick a target for a craft on the globe.
 */
internal class SelectDestinationState : State
{
    Craft _craft;
    Globe _globe;
    InteractiveSurface _btnRotateLeft, _btnRotateRight, _btnRotateUp, _btnRotateDown, _btnZoomIn, _btnZoomOut;
    Window _window;
    TextButton _btnCancel, _btnCydonia;
    Text _txtTitle;

    /**
     * Initializes all the elements in the Select Destination window.
     * @param game Pointer to the core game.
     * @param craft Pointer to the craft to target.
     * @param globe Pointer to the Geoscape globe.
     */
    internal SelectDestinationState(Craft craft, Globe globe)
    {
        _craft = craft;
        _globe = globe;

        int dx = _game.getScreen().getDX();
        int dy = _game.getScreen().getDY();
        _screen = false;

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
        _btnCancel = new TextButton(60, 12, 110 + dx, 8);
        _btnCydonia = new TextButton(60, 12, 180 + dx, 8);
        _txtTitle = new Text(100, 16, 10 + dx, 6);

        // Set palette
        setInterface("geoscape");

        add(_btnRotateLeft);
        add(_btnRotateRight);
        add(_btnRotateUp);
        add(_btnRotateDown);
        add(_btnZoomIn);
        add(_btnZoomOut);

        add(_window, "genericWindow", "geoscape");
        add(_btnCancel, "genericButton1", "geoscape");
        add(_btnCydonia, "genericButton1", "geoscape");
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

        _txtTitle.setText(tr("STR_SELECT_DESTINATION"));
        _txtTitle.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
        _txtTitle.setWordWrap(true);

        if (!_craft.getRules().getSpacecraft() || !_game.getSavedGame().isResearched(_game.getMod().getFinalResearch()))
        {
            _btnCydonia.setVisible(false);
        }
        else
        {
            _btnCydonia.setText(tr("STR_CYDONIA"));
            _btnCydonia.onMouseClick(btnCydoniaClick);
        }

        if (_craft.getStatus() != "STR_OUT")
        {
            _globe.setCraftRange(_craft.getLongitude(), _craft.getLatitude(), _craft.getBaseRange());
            _globe.invalidate();
        }
    }

    /**
     *
     */
    ~SelectDestinationState() =>
        _globe.setCraftRange(0.0, 0.0, 0.0);

    /**
     * Processes any left-clicks for picking a target,
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

        // Clicking on a valid target
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            List<Target> v = _globe.getTargets(mouseX, mouseY, true);
            if (!v.Any())
            {
                Waypoint w = new Waypoint();
                w.setLongitude(lon);
                w.setLatitude(lat);
                v.Add(w);
            }
            _game.pushState(new MultipleTargetsState(v, _craft, null));
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
    void btnCancelClick(Action _) =>
        _game.popState();

    void btnCydoniaClick(Action _)
    {
        if (_craft.getNumSoldiers() > 0 || _craft.getNumVehicles() > 0)
        {
            _game.pushState(new ConfirmCydoniaState(_craft));
        }
    }

    /**
     * Stop the globe movement.
     */
    protected override void init()
    {
	    base.init();
	    _globe.rotateStop();
    }
}
