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
 * Interactive view of a base.
 * Takes a certain base and displays all its facilities
 * and status, allowing players to manage them.
 */
internal class BaseView : InteractiveSurface
{
    const int BASE_SIZE = 6;
    const int GRID_SIZE = 32;

    Base _base;
    SurfaceSet _texture;
    BaseFacility _selFacility;
    BaseFacility[,] _facilities = new BaseFacility[BASE_SIZE, BASE_SIZE];
    Font _big, _small;
    Language _lang;
    int _gridX, _gridY, _selSize;
    Surface _selector;
    bool _blink;
    Timer _timer;
    byte _cellColor, _selectorColor;

    /**
     * Sets up a base view with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal BaseView(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _base = null;
        _texture = null;
        _selFacility = null;
        _big = null;
        _small = null;
        _lang = null;
        _gridX = 0;
        _gridY = 0;
        _selSize = 0;
        _selector = null;
        _blink = true;

        // Clear grid
        for (int i = 0; i < BASE_SIZE; ++i)
        {
            for (int j = 0; j < BASE_SIZE; ++j)
            {
                _facilities[i, j] = null;
            }
        }

        _timer = new Timer(100);
        _timer.onTimer((SurfaceHandler)blink);
        _timer.start();
    }

    /**
     * Deletes contents.
     */
    ~BaseView()
    {
        _selector = null;
        _timer = null;
    }

    /**
     * Makes the facility selector blink.
     */
    void blink()
    {
        _blink = !_blink;

        if (_selSize > 0)
        {
            SDL_Rect r;
            if (_blink)
            {
                r.w = _selector.getWidth();
                r.h = _selector.getHeight();
                r.x = 0;
                r.y = 0;
                _selector.drawRect(ref r, _selectorColor);
                r.w -= 2;
                r.h -= 2;
                r.x++;
                r.y++;
                _selector.drawRect(ref r, 0);
            }
            else
            {
                r.w = _selector.getWidth();
                r.h = _selector.getHeight();
                r.x = 0;
                r.y = 0;
                _selector.drawRect(ref r, 0);
            }
        }
    }

    /**
     * Returns the facility the mouse is currently over.
     * @return Pointer to base facility (0 if none).
     */
    internal BaseFacility getSelectedFacility() =>
	    _selFacility;

    /**
     * Changes the texture to use for drawing
     * the various base elements.
     * @param texture Pointer to SurfaceSet to use.
     */
    internal void setTexture(SurfaceSet texture) =>
        _texture = texture;

    /**
     * Changes the current base to display and
     * initializes the internal base grid.
     * @param base Pointer to base to display.
     */
    internal void setBase(Base @base)
    {
        _base = @base;
        _selFacility = null;

        // Clear grid
        for (int x = 0; x < BASE_SIZE; ++x)
        {
            for (int y = 0; y < BASE_SIZE; ++y)
            {
                _facilities[x, y] = null;
            }
        }

        // Fill grid with base facilities
        foreach (var i in _base.getFacilities())
        {
            for (int y = i.getY(); y < i.getY() + i.getRules().getSize(); ++y)
            {
                for (int x = i.getX(); x < i.getX() + i.getRules().getSize(); ++x)
                {
                    _facilities[x, y] = i;
                }
            }
        }

        _redraw = true;
    }

    /**
     * If enabled, the base view will respond to player input,
     * highlighting the selected facility.
     * @param size Facility length (0 disables it).
     */
    internal void setSelectable(int size)
    {
        _selSize = size;
        if (_selSize > 0)
        {
            _selector = new Surface(size * GRID_SIZE, size * GRID_SIZE, _x, _y);
            _selector.setPalette(getPaletteColors());
            SDL_Rect r;
            r.w = _selector.getWidth();
            r.h = _selector.getHeight();
            r.x = 0;
            r.y = 0;
            _selector.drawRect(ref r, _selectorColor);
            r.w -= 2;
            r.h -= 2;
            r.x++;
            r.y++;
            _selector.drawRect(ref r, 0);
            _selector.setVisible(false);
        }
        else
        {
            _selector = null;
        }
    }

    /**
     * Returns the X position of the grid square
     * the mouse is currently over.
     * @return X position on the grid.
     */
    internal int getGridX() =>
	    _gridX;

    /**
     * Returns the Y position of the grid square
     * the mouse is currently over.
     * @return Y position on the grid.
     */
    internal int getGridY() =>
	    _gridY;

    /**
     * Prevents any mouseover bugs on dismantling base facilities before setBase has had time to update the base.
     */
    internal void resetSelectedFacility()
    {
        _facilities[_selFacility.getX(), _selFacility.getY()] = null;
        _selFacility = null;
    }

    /**
     * Returns if a certain facility can be successfully
     * placed on the currently selected square.
     * @param rule Facility type.
     * @return True if placeable, False otherwise.
     */
    internal bool isPlaceable(RuleBaseFacility rule)
    {
	    // Check if square isn't occupied
	    for (int y = _gridY; y < _gridY + rule.getSize(); ++y)
	    {
		    for (int x = _gridX; x < _gridX + rule.getSize(); ++x)
		    {
			    if (x < 0 || x >= BASE_SIZE || y < 0 || y >= BASE_SIZE)
			    {
				    return false;
			    }
			    if (_facilities[x, y] != null)
			    {
				    return false;
			    }
		    }
	    }

	    bool bq=Options.allowBuildingQueue;

	    // Check for another facility to connect to
	    for (int i = 0; i < rule.getSize(); ++i)
	    {
		    if ((_gridX > 0 && _facilities[_gridX - 1, _gridY + i] != null && (bq || _facilities[_gridX - 1, _gridY + i].getBuildTime() == 0)) ||
			    (_gridY > 0 && _facilities[_gridX + i, _gridY - 1] != null && (bq || _facilities[_gridX + i, _gridY - 1].getBuildTime() == 0)) ||
			    (_gridX + rule.getSize() < BASE_SIZE && _facilities[_gridX + rule.getSize(), _gridY + i] != null && (bq || _facilities[_gridX + rule.getSize(), _gridY + i].getBuildTime() == 0)) ||
			    (_gridY + rule.getSize() < BASE_SIZE && _facilities[_gridX + i, _gridY + rule.getSize()] != null && (bq || _facilities[_gridX + i, _gridY + rule.getSize()].getBuildTime() == 0)))
		    {
			    return true;
		    }
	    }

	    return false;
    }

    /**
     * Returns if the placed facility is placed in queue or not.
     * @param rule Facility type.
     * @return True if queued, False otherwise.
     */
    internal bool isQueuedBuilding(RuleBaseFacility rule)
    {
	    for (int i = 0; i < rule.getSize(); ++i)
	    {
		    if ((_gridX > 0 && _facilities[_gridX - 1, _gridY + i] != null && _facilities[_gridX - 1, _gridY + i].getBuildTime() == 0) ||
			    (_gridY > 0 && _facilities[_gridX + i, _gridY - 1] != null && _facilities[_gridX + i, _gridY - 1].getBuildTime() == 0) ||
			    (_gridX + rule.getSize() < BASE_SIZE && _facilities[_gridX + rule.getSize(), _gridY + i] != null && _facilities[_gridX + rule.getSize(), _gridY + i].getBuildTime() == 0) ||
			    (_gridY + rule.getSize() < BASE_SIZE && _facilities[_gridX + i, _gridY + rule.getSize()] != null && _facilities[_gridX + i, _gridY + rule.getSize()].getBuildTime() == 0))
		    {
			    return false;
		    }
	    }
	    return true;
    }

    /**
     * ReCalculates the remaining build-time of all queued buildings.
     */
    internal void reCalcQueuedBuildings()
    {
	    setBase(_base);
	    var facilities = new List<BaseFacility>();
	    foreach (var i in _base.getFacilities())
		    if (i.getBuildTime() > 0)
		    {
			    // Set all queued buildings to infinite.
			    if (i.getBuildTime() > i.getRules().getBuildTime()) i.setBuildTime(int.MaxValue);
			    facilities.Add(i);
		    }

	    // Applying a simple Dijkstra Algorithm
	    while (facilities.Any())
	    {
		    BaseFacility min = facilities[0];
		    foreach (var i in facilities)
			    if (i.getBuildTime() < min.getBuildTime()) min=i;
		    BaseFacility facility=min;
		    facilities.Remove(min);
		    RuleBaseFacility rule=facility.getRules();
		    int x=facility.getX(), y=facility.getY();
		    for (int i = 0; i < rule.getSize(); ++i)
		    {
			    if (x > 0) updateNeighborFacilityBuildTime(facility,_facilities[x - 1, y + i]);
			    if (y > 0) updateNeighborFacilityBuildTime(facility,_facilities[x + i, y - 1]);
			    if (x + rule.getSize() < BASE_SIZE) updateNeighborFacilityBuildTime(facility,_facilities[x + rule.getSize(), y + i]);
			    if (y + rule.getSize() < BASE_SIZE) updateNeighborFacilityBuildTime(facility,_facilities[x + i, y + rule.getSize()]);
		    }
	    }
    }

    /**
     * Updates the neighborFacility's build time. This is for internal use only (reCalcQueuedBuildings()).
     * @param facility Pointer to a base facility.
     * @param neighbor Pointer to a neighboring base facility.
     */
    void updateNeighborFacilityBuildTime(BaseFacility facility, BaseFacility neighbor)
    {
	    if (facility != null && neighbor != null
	    && neighbor.getBuildTime() > neighbor.getRules().getBuildTime()
	    && facility.getBuildTime() + neighbor.getRules().getBuildTime() < neighbor.getBuildTime())
		    neighbor.setBuildTime(facility.getBuildTime() + neighbor.getRules().getBuildTime());
    }

    /**
     * Keeps the animation timers running.
     */
    protected override void think() =>
	    _timer.think(null, this);

    /**
     * Draws the view of all the facilities in the base, connectors
     * between them and crafts landed in hangars.
     */
    protected override void draw()
    {
	    base.draw();

	    // Draw grid squares
	    for (int x = 0; x < BASE_SIZE; ++x)
	    {
		    for (int y = 0; y < BASE_SIZE; ++y)
		    {
			    Surface frame = _texture.getFrame(0);
			    frame.setX(x * GRID_SIZE);
			    frame.setY(y * GRID_SIZE);
			    frame.blit(this);
		    }
	    }

        var crafts = _base.getCrafts();
	    var c = 0;

	    foreach (var i in _base.getFacilities())
	    {
		    // Draw facility shape
		    int num = 0;
		    for (int y = i.getY(); y < i.getY() + i.getRules().getSize(); ++y)
		    {
			    for (int x = i.getX(); x < i.getX() + i.getRules().getSize(); ++x)
			    {
				    Surface frame;

				    int outline = Math.Max(i.getRules().getSize() * i.getRules().getSize(), 3);
				    if (i.getBuildTime() == 0)
					    frame = _texture.getFrame(i.getRules().getSpriteShape() + num);
				    else
					    frame = _texture.getFrame(i.getRules().getSpriteShape() + num + outline);

				    frame.setX(x * GRID_SIZE);
				    frame.setY(y * GRID_SIZE);
				    frame.blit(this);

				    num++;
			    }
		    }
	    }

	    foreach (var i in _base.getFacilities())
	    {
		    // Draw connectors
		    if (i.getBuildTime() == 0)
		    {
			    // Facilities to the right
			    int x = i.getX() + i.getRules().getSize();
			    if (x < BASE_SIZE)
			    {
				    for (int y = i.getY(); y < i.getY() + i.getRules().getSize(); ++y)
				    {
					    if (_facilities[x, y] != null && _facilities[x, y].getBuildTime() == 0)
					    {
						    Surface frame = _texture.getFrame(7);
						    frame.setX(x * GRID_SIZE - GRID_SIZE / 2);
						    frame.setY(y * GRID_SIZE);
						    frame.blit(this);
					    }
				    }
			    }

			    // Facilities to the bottom
			    int yy = i.getY() + i.getRules().getSize();
			    if (yy < BASE_SIZE)
			    {
				    for (int subX = i.getX(); subX < i.getX() + i.getRules().getSize(); ++subX)
				    {
					    if (_facilities[subX, yy] != null && _facilities[subX, yy].getBuildTime() == 0)
					    {
						    Surface frame = _texture.getFrame(8);
						    frame.setX(subX * GRID_SIZE);
						    frame.setY(yy * GRID_SIZE - GRID_SIZE / 2);
						    frame.blit(this);
					    }
				    }
			    }
		    }
	    }

	    foreach (var i in _base.getFacilities())
	    {
		    // Draw facility graphic
		    int num = 0;
		    for (int y = i.getY(); y < i.getY() + i.getRules().getSize(); ++y)
		    {
			    for (int x = i.getX(); x < i.getX() + i.getRules().getSize(); ++x)
			    {
				    if (i.getRules().getSize() == 1)
				    {
					    Surface frame = _texture.getFrame(i.getRules().getSpriteFacility() + num);
					    frame.setX(x * GRID_SIZE);
					    frame.setY(y * GRID_SIZE);
					    frame.blit(this);
				    }

				    num++;
			    }
		    }

		    // Draw crafts
		    i.setCraft(null);
		    if (i.getBuildTime() == 0 && i.getRules().getCrafts() > 0)
		    {
			    if (c != crafts.Count - 1)
			    {
				    if (crafts[c].getStatus() != "STR_OUT")
				    {
					    Surface frame = _texture.getFrame(crafts[c].getRules().getSprite() + 33);
					    frame.setX(i.getX() * GRID_SIZE + (i.getRules().getSize() - 1) * GRID_SIZE / 2 + 2);
					    frame.setY(i.getY() * GRID_SIZE + (i.getRules().getSize() - 1) * GRID_SIZE / 2 - 4);
					    frame.blit(this);
                        i.setCraft(crafts[c]);
				    }
				    ++c;
			    }
		    }

		    // Draw time remaining
		    if (i.getBuildTime() > 0)
		    {
			    Text text = new Text(GRID_SIZE * i.getRules().getSize(), 16, 0, 0);
			    text.setPalette(getPaletteColors());
			    text.initText(_big, _small, _lang);
			    text.setX(i.getX() * GRID_SIZE);
			    text.setY(i.getY() * GRID_SIZE + (GRID_SIZE * i.getRules().getSize() - 16) / 2);
			    text.setBig();
			    string ss = i.getBuildTime().ToString();
			    text.setAlign(TextHAlign.ALIGN_CENTER);
			    text.setColor(_cellColor);
			    text.setText(ss);
			    text.blit(this);
			    text = null;
		    }
	    }
    }

    /**
     * Blits the base view and selector.
     * @param surface Pointer to surface to blit onto.
     */
    protected override void blit(Surface surface)
    {
	    base.blit(surface);
	    if (_selector != null)
	    {
		    _selector.blit(surface);
	    }
    }

	/**
	 * Selects the facility the mouse is over.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void mouseOver(Action action, State state)
	{
		_gridX = (int)Math.Floor(action.getRelativeXMouse() / (GRID_SIZE * action.getXScale()));
		_gridY = (int)Math.Floor(action.getRelativeYMouse() / (GRID_SIZE * action.getYScale()));
		if (_gridX >= 0 && _gridX < BASE_SIZE && _gridY >= 0 && _gridY < BASE_SIZE)
		{
			_selFacility = _facilities[_gridX, _gridY];
			if (_selSize > 0)
			{
				if (_gridX + _selSize - 1 < BASE_SIZE && _gridY + _selSize - 1 < BASE_SIZE)
				{
					_selector.setX(_x + _gridX * GRID_SIZE);
					_selector.setY(_y + _gridY * GRID_SIZE);
					_selector.setVisible(true);
				}
				else
				{
					_selector.setVisible(false);
				}
			}
		}
		else
		{
			_selFacility = null;
			if (_selSize > 0)
			{
				_selector.setVisible(false);
			}
		}

		base.mouseOver(action, state);
	}

	/**
	 * Deselects the facility.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void mouseOut(Action action, State state)
	{
		_selFacility = null;
		if (_selSize > 0)
		{
			_selector.setVisible(false);
		}

		base.mouseOut(action, state);
	}
}
