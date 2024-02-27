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

namespace SharpXcom.Battlescape;

enum CursorType { CT_NONE, CT_NORMAL, CT_AIM, CT_PSI, CT_WAYPOINT, CT_THROW };

/*
  1) Map origin is top corner.
  2) X axis goes downright. (width of the map)
  3) Y axis goes downleft. (length of the map
  4) Z axis goes up (height of the map)

           0,0
			/\
	    y+ /  \ x+
		   \  /
		    \/
 */

/**
 * Interactive map of the battlescape.
 */
internal class Map : InteractiveSurface
{
    const int SCROLL_INTERVAL = 15;
	const int BULLET_SPRITES = 35;

    Game _game;
    Surface _arrow;
    int _selectorX, _selectorY;
    int _mouseX, _mouseY;
    CursorType _cursorType;
    int _cursorSize;
    int _animFrame;
    Projectile _projectile;
    bool _projectileInFOV;
    bool _explosionInFOV, _launch;
    int _visibleMapHeight;
    bool _unitDying, _smoothCamera, _smoothingEngaged, _flashScreen;
    SurfaceSet _projectileSet;
    bool _showObstacles;
    int _iconHeight, _iconWidth, _messageColor;
    PathPreview _previewSetting;
    SavedBattleGame _save;
    List<byte> _transparencies;
    int _spriteWidth, _spriteHeight;
    BattlescapeMessage _message;
    Camera _camera;
    Timer _scrollMouseTimer, _scrollKeyTimer, _obstacleTimer;
    Text _txtAccuracy;
    List<Position> _waypoints;
	List<Explosion> _explosions;

    /**
     * Sets up a map with the specified size and position.
     * @param game Pointer to the core game.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     * @param visibleMapHeight Current visible map height.
     */
    internal Map(Game game, int width, int height, int x, int y, int visibleMapHeight) : base(width, height, x, y)
    {
        _game = game;
        _arrow = null;
        _selectorX = 0;
        _selectorY = 0;
        _mouseX = 0;
        _mouseY = 0;
        _cursorType = CursorType.CT_NORMAL;
        _cursorSize = 1;
        _animFrame = 0;
        _projectile = null;
        _projectileInFOV = false;
        _explosionInFOV = false;
        _launch = false;
        _visibleMapHeight = visibleMapHeight;
        _unitDying = false;
        _smoothingEngaged = false;
        _flashScreen = false;
        _projectileSet = null;
        _showObstacles = false;

        _iconHeight = _game.getMod().getInterface("battlescape").getElement("icons").h;
        _iconWidth = _game.getMod().getInterface("battlescape").getElement("icons").w;
        _messageColor = _game.getMod().getInterface("battlescape").getElement("messageWindows").color;

        _previewSetting = Options.battleNewPreviewPath;
        _smoothCamera = Options.battleSmoothCamera;
        if (Options.traceAI)
        {
            // turn everything on because we want to see the markers.
            _previewSetting = PathPreview.PATH_FULL;
        }
        _save = _game.getSavedGame().getSavedBattle();
        if (_game.getMod().getLUTs().Count > _save.getDepth())
        {
            _transparencies = _game.getMod().getLUTs()[_save.getDepth()];
        }

        _spriteWidth = _game.getMod().getSurfaceSet("BLANKS.PCK").getFrame(0).getWidth();
        _spriteHeight = _game.getMod().getSurfaceSet("BLANKS.PCK").getFrame(0).getHeight();
        _message = new BattlescapeMessage(320, (visibleMapHeight < 200) ? visibleMapHeight : 200, 0, 0);
        _message.setX(_game.getScreen().getDX());
        _message.setY((visibleMapHeight - _message.getHeight()) / 2);
        _message.setTextColor((byte)_messageColor);
        _camera = new Camera(_spriteWidth, _spriteHeight, _save.getMapSizeX(), _save.getMapSizeY(), _save.getMapSizeZ(), this, visibleMapHeight);
        _scrollMouseTimer = new Timer(SCROLL_INTERVAL);
        _scrollMouseTimer.onTimer((SurfaceHandler)scrollMouse);
        _scrollKeyTimer = new Timer(SCROLL_INTERVAL);
        _scrollKeyTimer.onTimer((SurfaceHandler)scrollKey);
        _camera.setScrollTimer(_scrollMouseTimer, _scrollKeyTimer);
        _obstacleTimer = new Timer(2500);
        _obstacleTimer.stop();
        _obstacleTimer.onTimer((SurfaceHandler)disableObstacles);

        _txtAccuracy = new Text(24, 9, 0, 0);
        _txtAccuracy.setSmall();
        _txtAccuracy.setPalette(_game.getScreen().getPalette());
        _txtAccuracy.setHighContrast(true);
        _txtAccuracy.initText(_game.getMod().getFont("FONT_BIG"), _game.getMod().getFont("FONT_SMALL"), _game.getLanguage());
    }

    /**
     * Deletes the map.
     */
    ~Map()
    {
        _scrollMouseTimer = null;
        _scrollKeyTimer = null;
        _obstacleTimer = null;
        _arrow = null;
        _message = null;
        _camera = null;
        _txtAccuracy = null;
    }

    /**
     * Sets the 3D cursor to selection/aim mode.
     * @param type Cursor type.
     * @param size Size of cursor.
     */
    internal void setCursorType(CursorType type, int size = 1)
    {
        _cursorType = type;
        if (_cursorType == CursorType.CT_NORMAL)
            _cursorSize = size;
        else
            _cursorSize = 1;
    }

    /**
     * Gets a list of waypoints on the map.
     * @return A list of waypoints.
     */
    internal List<Position> getWaypoints() =>
        _waypoints;

    /**
     * Timers only work on surfaces so we have to pass this on to the camera object.
     */
    void scrollMouse() =>
        _camera.scrollMouse();

    /**
     * Timers only work on surfaces so we have to pass this on to the camera object.
     */
    void scrollKey() =>
        _camera.scrollKey();

    /**
     * Disables obstacle markers.
     */
    internal void disableObstacles()
    {
        _showObstacles = false;
        if (_obstacleTimer != null)
        {
            _obstacleTimer.stop();
        }
    }

    /**
     * Sets the selector to a certain tile on the map.
     * @param mx mouse x position.
     * @param my mouse y position.
     */
    internal void setSelectorPosition(int mx, int my)
    {
        int oldX = _selectorX, oldY = _selectorY;

        _camera.convertScreenToMap(mx, my + _spriteHeight / 4, ref _selectorX, ref _selectorY);

        if (oldX != _selectorX || oldY != _selectorY)
        {
            _redraw = true;
        }
    }

    /**
     * Updates the selector to the last-known mouse position.
     */
    internal void refreshSelectorPosition() =>
        setSelectorPosition(_mouseX, _mouseY);

    /**
     * Sets the unitDying flag.
     * @param flag True if the unit is dying.
     */
    internal void setUnitDying(bool flag) =>
        _unitDying = flag;

    /**
     * Gets the pointer to the camera.
     * @return Pointer to camera.
     */
    internal Camera getCamera() =>
        _camera;

    /**
     * Get the hidden movement screen's vertical position.
     * @return the vertical position of the hidden movement window.
     */
    internal int getMessageY() =>
	    _message.getY();

    /**
     * Gets the current projectile sprite on the map.
     * @return Projectile or 0 if there is no projectile sprite on the map.
     */
    internal Projectile getProjectile() =>
	    _projectile;

    /**
     * Checks all units for if they need to be redrawn.
     */
    internal void cacheUnits()
    {
	    foreach (var i in _save.getUnits())
	    {
		    cacheUnit(i);
	    }
    }

    /**
     * Check if a certain unit needs to be redrawn.
     * @param unit Pointer to battleUnit.
     */
    internal void cacheUnit(BattleUnit unit)
    {
	    UnitSprite unitSprite = new UnitSprite(_spriteWidth * 2, _spriteHeight, 0, 0, _save.getDepth() != 0);
	    unitSprite.setPalette(this.getPaletteColors());
	    int numOfParts = unit.getArmor().getSize() * unit.getArmor().getSize();

	    if (unit.isCacheInvalid())
	    {
		    // 1 or 4 iterations, depending on unit size
		    for (int i = 0; i < numOfParts; i++)
		    {
			    Surface cache = unit.getCache(i);
			    if (cache == null) // no cache created yet
			    {
				    cache = new Surface(_spriteWidth * 2, _spriteHeight);
				    cache.setPalette(this.getPaletteColors());
			    }

			    unitSprite.setBattleUnit(unit, i);
			    unitSprite.setSurfaces(_game.getMod().getSurfaceSet(unit.getArmor().getSpriteSheet()),
									    _game.getMod().getSurfaceSet("HANDOB.PCK"),
									    _game.getMod().getSurfaceSet("HANDOB2.PCK"));
			    unitSprite.setAnimationFrame(_animFrame);
			    cache.clear();
			    unitSprite.blit(cache);
			    unit.setCache(cache, i);
		    }
	    }
	    unitSprite = null;
    }

    /**
     * Set the "explosion flash" bool.
     * @param flash should the screen be rendered in EGA this frame?
     */
    internal void setBlastFlash(bool flash) =>
	    _flashScreen = flash;

    /**
     * Gets a list of explosion sprites on the map.
     * @return A list of explosion sprites.
     */
    internal List<Explosion> getExplosions() =>
	    _explosions;

    /**
     * Returns the angle(left/right balance) of a sound effect,
     * based off a map position.
     * @param pos the map position to calculate the sound angle from.
     * @return the angle of the sound (280 to 440).
     */
    internal int getSoundAngle(Position pos)
    {
	    int midPoint = getWidth() / 2;
	    Position relativePosition;

	    _camera.convertMapToScreen(pos, out relativePosition);
	    // cap the position to the screen edges relative to the center,
	    // negative values indicating a left-shift, and positive values shifting to the right.
	    relativePosition.x = Math.Clamp((relativePosition.x + _camera.getMapOffset().x) - midPoint, -midPoint, midPoint);

	    // convert the relative distance to a relative increment of an 80 degree angle
	    // we use +- 80 instead of +- 90, so as not to go ALL the way left or right
	    // which would effectively mute the sound out of one speaker.
	    // since Mix_SetPosition uses modulo 360, we can't feed it a negative number, so add 360 instead.
	    return (int)(360 + (relativePosition.x / (midPoint / 80.0)));
    }

    /**
     * Initializes the map.
     */
    internal void init()
    {
	    // load the tiny arrow into a surface
	    int f = Palette.blockOffset(1); // yellow
	    int b = 15; // black
	    int[] pixels = { 0, 0, b, b, b, b, b, 0, 0,
					     0, 0, b, f, f, f, b, 0, 0,
					     0, 0, b, f, f, f, b, 0, 0,
					     b, b, b, f, f, f, b, b, b,
					     b, f, f, f, f, f, f, f, b,
					     0, b, f, f, f, f, f, b, 0,
					     0, 0, b, f, f, f, b, 0, 0,
					     0, 0, 0, b, f, b, 0, 0, 0,
					     0, 0, 0, 0, b, 0, 0, 0, 0 };

	    _arrow = new Surface(9, 9);
	    _arrow.setPalette(this.getPaletteColors());
	    _arrow.@lock();
	    for (int y = 0; y < 9;++y)
		    for (int x = 0; x < 9; ++x)
			    _arrow.setPixel(x, y, (byte)pixels[x+(y*9)]);
	    _arrow.unlock();

	    _projectile = null;
	    if (_save.getDepth() == 0)
	    {
		    _projectileSet = _game.getMod().getSurfaceSet("Projectiles");
	    }
	    else
	    {
		    _projectileSet = _game.getMod().getSurfaceSet("UnderwaterProjectiles");
	    }
    }

    /**
     * Resets obstacle markers.
     */
    internal void resetObstacles()
    {
	    for (int z = 0; z < _save.getMapSizeZ(); z++)
		    for (int y = 0; y < _save.getMapSizeY(); y++)
			    for (int x = 0; x < _save.getMapSizeX(); x++)
			    {
				    Tile tile = _save.getTile(new Position(x, y, z));
				    if (tile != null) tile.resetObstacle();
			    }
	    _showObstacles = false;
    }

    /**
     * Enables obstacle markers.
     */
    internal void enableObstacles()
    {
	    _showObstacles = true;
	    if (_obstacleTimer != null)
	    {
		    _obstacleTimer.stop();
		    _obstacleTimer.start();
	    }
    }

    /**
     * Puts a projectile sprite on the map.
     * @param projectile Projectile to place.
     */
    internal void setProjectile(Projectile projectile)
    {
	    _projectile = projectile;
	    if (projectile != null && Options.battleSmoothCamera)
	    {
		    _launch = true;
	    }
    }

    /**
     * Checks if the screen is still being rendered in EGA.
     * @return if we are still in EGA mode.
     */
    internal bool getBlastFlash() =>
	    _flashScreen;

    /**
     * Keeps the animation timers running.
     */
    protected override void think()
    {
	    _scrollMouseTimer.think(null, this);
	    _scrollKeyTimer.think(null, this);
	    _obstacleTimer.think(null, this);
    }

    /**
     * Reset the camera smoothing bool.
     */
    internal void resetCameraSmoothing() =>
	    _smoothingEngaged = false;

    /**
     * Get the icon height.
     */
    internal int getIconHeight() =>
	    _iconHeight;

    /**
     * Get the icon width.
     */
    internal int getIconWidth() =>
	    _iconWidth;

    /**
     * Draws the rectangle selector.
     * @param pos Pointer to a position.
     */
    internal void getSelectorPosition(out Position pos) =>
        pos = new Position
        {
            x = _selectorX,
            y = _selectorY,
            z = _camera.getViewLevel()
        };

    /**
     * Gets the cursor type.
     * @return cursortype.
     */
    internal CursorType getCursorType() =>
	    _cursorType;

    /**
     * Sets mouse-buttons' pressed state.
     * @param button Index of the button.
     * @param pressed The state of the button.
     */
    internal void setButtonsPressed(byte button, bool pressed) =>
	    setButtonPressed(button, pressed);

    /**
     * Handles animating tiles. 8 Frames per animation.
     * @param redraw Redraw the battlescape?
     */
    internal void animate(bool redraw)
    {
	    _animFrame++;
	    if (_animFrame == 8) _animFrame = 0;

	    // animate tiles
	    for (int i = 0; i < _save.getMapSizeXYZ(); ++i)
	    {
		    _save.getTiles()[i].animate();
	    }

	    // animate certain units (large flying units have a propulsion animation)
	    foreach (var i in _save.getUnits())
	    {
		    if (_save.getDepth() > 0 && !i.getFloorAbove())
		    {
			    i.breathe();
		    }
		    if (!i.isOut())
		    {
			    if (i.getArmor().getConstantAnimation())
			    {
				    i.setCache(null);
				    cacheUnit(i);
			    }
		    }
	    }

	    if (redraw) _redraw = true;
    }

    /**
     * Draws the whole map, part by part.
     */
    internal override void draw()
    {
	    if (!_redraw)
	    {
		    return;
	    }

	    // normally we'd call for a Surface.draw();
	    // but we don't want to clear the background with colour 0, which is transparent (aka black)
	    // we use colour 15 because that actually corresponds to the colour we DO want in all variations of the xcom and tftd palettes.
	    _redraw = false;
	    clear((uint)(Palette.blockOffset(0)+15));

	    Tile t;

	    _projectileInFOV = _save.getDebugMode();
	    if (_projectile != null)
	    {
		    t = _save.getTile(new Position(_projectile.getPosition(0).x/16, _projectile.getPosition(0).y/16, _projectile.getPosition(0).z/24));
		    if (_save.getSide() == UnitFaction.FACTION_PLAYER || (t != null && t.getVisible() != 0))
		    {
			    _projectileInFOV = true;
		    }
	    }
	    _explosionInFOV = _save.getDebugMode();
	    if (_explosions.Any())
	    {
		    foreach (var i in _explosions)
		    {
			    t = _save.getTile(new Position(i.getPosition().x/16, i.getPosition().y/16, i.getPosition().z/24));
			    if (t != null && (i.isBig() || t.getVisible() != 0))
			    {
				    _explosionInFOV = true;
				    break;
			    }
		    }
	    }

	    if ((_save.getSelectedUnit() != null && _save.getSelectedUnit().getVisible()) || _unitDying || _save.getSelectedUnit() == null || _save.getDebugMode() || _projectileInFOV || _explosionInFOV)
	    {
		    drawTerrain(this);
	    }
	    else
	    {
		    _message.blit(this);
	    }
    }

	static int[] arrowBob = {0,1,2,1,0,1,2,1};
	/**
	 * Draw the terrain.
	 * Keep this function as optimised as possible. It's big to minimise overhead of function calls.
	 * @param surface The surface to draw on.
	 */
	void drawTerrain(Surface surface)
	{
		int frameNumber = 0;
		Surface tmpSurface;
		Tile tile;
		int beginX = 0, endX = _save.getMapSizeX() - 1;
		int beginY = 0, endY = _save.getMapSizeY() - 1;
		int beginZ = 0, endZ = _camera.getShowAllLayers()?_save.getMapSizeZ() - 1:_camera.getViewLevel();
		Position mapPosition, screenPosition, bulletPositionScreen;
		int bulletLowX=16000, bulletLowY=16000, bulletLowZ=16000, bulletHighX=0, bulletHighY=0, bulletHighZ=0;
		int dummy = 0;
		BattleUnit unit = null;
		int tileShade, wallShade, tileColor, obstacleShade;

		NumberText _numWaypid = null;

		// if we got bullet, get the highest x and y tiles to draw it on
		if (_projectile != null && !_explosions.Any())
		{
			int part = _projectile.getItem() != null ? 0 : BULLET_SPRITES-1;
			for (int i = 0; i <= part; ++i)
			{
				if (_projectile.getPosition(1-i).x < bulletLowX)
					bulletLowX = _projectile.getPosition(1-i).x;
				if (_projectile.getPosition(1-i).y < bulletLowY)
					bulletLowY = _projectile.getPosition(1-i).y;
				if (_projectile.getPosition(1-i).z < bulletLowZ)
					bulletLowZ = _projectile.getPosition(1-i).z;
				if (_projectile.getPosition(1-i).x > bulletHighX)
					bulletHighX = _projectile.getPosition(1-i).x;
				if (_projectile.getPosition(1-i).y > bulletHighY)
					bulletHighY = _projectile.getPosition(1-i).y;
				if (_projectile.getPosition(1-i).z > bulletHighZ)
					bulletHighZ = _projectile.getPosition(1-i).z;
			}
			// divide by 16 to go from voxel to tile position
			bulletLowX = bulletLowX / 16;
			bulletLowY = bulletLowY / 16;
			bulletLowZ = bulletLowZ / 24;
			bulletHighX = bulletHighX / 16;
			bulletHighY = bulletHighY / 16;
			bulletHighZ = bulletHighZ / 24;

			// if the projectile is outside the viewport - center it back on it
			_camera.convertVoxelToScreen(_projectile.getPosition(), out bulletPositionScreen);

			if (_projectileInFOV)
			{
				Position newCam = _camera.getMapOffset();
				if (newCam.z != bulletHighZ) //switch level
				{
					newCam.z = bulletHighZ;
					if (_projectileInFOV)
					{
						_camera.setMapOffset(newCam);
						_camera.convertVoxelToScreen(_projectile.getPosition(), out bulletPositionScreen);
					}
				}
				if (_smoothCamera)
				{
					if (_launch)
					{
						_launch = false;
						if ((bulletPositionScreen.x < 1 || bulletPositionScreen.x > surface.getWidth() - 1 ||
							bulletPositionScreen.y < 1 || bulletPositionScreen.y > _visibleMapHeight - 1))
						{
							_camera.centerOnPosition(new Position(bulletLowX, bulletLowY, bulletHighZ), false);
							_camera.convertVoxelToScreen(_projectile.getPosition(), out bulletPositionScreen);
						}
					}
					if (!_smoothingEngaged)
					{
						if (bulletPositionScreen.x < 1 || bulletPositionScreen.x > surface.getWidth() - 1 ||
							bulletPositionScreen.y < 1 || bulletPositionScreen.y > _visibleMapHeight - 1)
						{
							_smoothingEngaged = true;
						}
					}
					else
					{
						_camera.jumpXY(surface.getWidth() / 2 - bulletPositionScreen.x, _visibleMapHeight / 2 - bulletPositionScreen.y);
					}
				}
				else
				{
					bool enough;
					do
					{
						enough = true;
						if (bulletPositionScreen.x < 0)
						{
							_camera.jumpXY(+surface.getWidth(), 0);
							enough = false;
						}
						else if (bulletPositionScreen.x > surface.getWidth())
						{
							_camera.jumpXY(-surface.getWidth(), 0);
							enough = false;
						}
						else if (bulletPositionScreen.y < 0)
						{
							_camera.jumpXY(0, +_visibleMapHeight);
							enough = false;
						}
						else if (bulletPositionScreen.y > _visibleMapHeight)
						{
							_camera.jumpXY(0, -_visibleMapHeight);
							enough = false;
						}
						_camera.convertVoxelToScreen(_projectile.getPosition(), out bulletPositionScreen);
					}
					while (!enough);
				}
			}
		}

		// get corner map coordinates to give rough boundaries in which tiles to redraw are
		_camera.convertScreenToMap(0, 0, ref beginX, ref dummy);
		_camera.convertScreenToMap(surface.getWidth(), 0, ref dummy, ref beginY);
		_camera.convertScreenToMap(surface.getWidth() + _spriteWidth, surface.getHeight() + _spriteHeight, ref endX, ref dummy);
		_camera.convertScreenToMap(0, surface.getHeight() + _spriteHeight, ref dummy, ref endY);
		beginY -= (_camera.getViewLevel() * 2);
		beginX -= (_camera.getViewLevel() * 2);
		if (beginX < 0)
			beginX = 0;
		if (beginY < 0)
			beginY = 0;

		bool pathfinderTurnedOn = _save.getPathfinding().isPathPreviewed();

		if (_waypoints.Any() || (pathfinderTurnedOn && ((_previewSetting & PathPreview.PATH_TU_COST) != 0)))
		{
			_numWaypid = new NumberText(15, 15, 20, 30);
			_numWaypid.setPalette(getPaletteColors());
			_numWaypid.setColor((byte)(pathfinderTurnedOn ? _messageColor + 1 : Palette.blockOffset(1)));
		}

		surface.@lock();
		for (int itZ = beginZ; itZ <= endZ; itZ++)
		{
			bool topLayer = itZ == endZ;
			for (int itX = beginX; itX <= endX; itX++)
			{
				for (int itY = beginY; itY <= endY; itY++)
				{
					mapPosition = new Position(itX, itY, itZ);
					_camera.convertMapToScreen(mapPosition, out screenPosition);
					screenPosition += _camera.getMapOffset();

					// only render cells that are inside the surface
					if (screenPosition.x > -_spriteWidth && screenPosition.x < surface.getWidth() + _spriteWidth &&
						screenPosition.y > -_spriteHeight && screenPosition.y < surface.getHeight() + _spriteHeight )
					{
						tile = _save.getTile(mapPosition);

						if (tile == null) continue;

						if (tile.isDiscovered(2))
						{
							tileShade = tile.getShade();
							obstacleShade = tileShade;
							if (_showObstacles)
							{
								if (tile.isObstacle())
								{
									if (tileShade > 7) obstacleShade = 7;
									if (tileShade < 2) obstacleShade = 2;
									obstacleShade += (arrowBob[_animFrame] * 2 - 2);
								}
							}
						}
						else
						{
							tileShade = 16;
							obstacleShade = 16;
							unit = null;
						}

						tileColor = tile.getMarkerColor();

						// Draw floor
						tmpSurface = tile.getSprite((int)TilePart.O_FLOOR);
						if (tmpSurface != null)
						{
							if (tile.getObstacle((int)TilePart.O_FLOOR))
								tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y - tile.getMapData(TilePart.O_FLOOR).getYOffset(), obstacleShade, false);
							else
								tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y - tile.getMapData(TilePart.O_FLOOR).getYOffset(), tileShade, false);
						}
						unit = tile.getUnit();

						// Draw cursor back
						if (_cursorType != CursorType.CT_NONE && _selectorX > itX - _cursorSize && _selectorY > itY - _cursorSize && _selectorX < itX+1 && _selectorY < itY+1 && !_save.getBattleState().getMouseOverIcons())
						{
							if (_camera.getViewLevel() == itZ)
							{
								if (_cursorType != CursorType.CT_AIM)
								{
									if (unit != null && (unit.getVisible() || _save.getDebugMode()))
										frameNumber = (_animFrame % 2); // yellow box
									else
										frameNumber = 0; // red box
								}
								else
								{
									if (unit != null && (unit.getVisible() || _save.getDebugMode()))
										frameNumber = 7 + (_animFrame / 2); // yellow animated crosshairs
									else
										frameNumber = 6; // red static crosshairs
								}
								tmpSurface = _game.getMod().getSurfaceSet("CURSOR.PCK").getFrame(frameNumber);
								tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y, 0);
							}
							else if (_camera.getViewLevel() > itZ)
							{
								frameNumber = 2; // blue box
								tmpSurface = _game.getMod().getSurfaceSet("CURSOR.PCK").getFrame(frameNumber);
								tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y, 0);
							}
						}

						// special handling for a moving unit in background of tile.
						const int backPosSize = 3;
						Position[] backPos =
						{
							new Position(0, -1, 0),
							new Position(-1, -1, 0),
							new Position(-1, 0, 0),
						};

						for (int b = 0; b < backPosSize; ++b)
						{
							drawUnit(surface, _save.getTile(mapPosition + backPos[b]), tile, screenPosition, tileShade, obstacleShade, topLayer);
						}

						// Draw walls
						if (!tile.isVoid())
						{
							// Draw west wall
							tmpSurface = tile.getSprite((int)TilePart.O_WESTWALL);
							if (tmpSurface != null)
							{
								if ((tile.getMapData(TilePart.O_WESTWALL).isDoor() || tile.getMapData(TilePart.O_WESTWALL).isUFODoor())
									 && tile.isDiscovered(0))
									wallShade = tile.getShade();
								else
									wallShade = tileShade;
								if (tile.getObstacle((int)TilePart.O_WESTWALL))
									tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y - tile.getMapData(TilePart.O_WESTWALL).getYOffset(), obstacleShade, false);
								else
									tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y - tile.getMapData(TilePart.O_WESTWALL).getYOffset(), wallShade, false);
							}
							// Draw north wall
							tmpSurface = tile.getSprite((int)TilePart.O_NORTHWALL);
							if (tmpSurface != null)
							{
								if ((tile.getMapData(TilePart.O_NORTHWALL).isDoor() || tile.getMapData(TilePart.O_NORTHWALL).isUFODoor())
									 && tile.isDiscovered(1))
									wallShade = tile.getShade();
								else
									wallShade = tileShade;
								if (tile.getObstacle((int)TilePart.O_NORTHWALL))
									tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y - tile.getMapData(TilePart.O_NORTHWALL).getYOffset(), obstacleShade, tile.getMapData(TilePart.O_WESTWALL) != null);
								else
									tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y - tile.getMapData(TilePart.O_NORTHWALL).getYOffset(), wallShade, tile.getMapData(TilePart.O_WESTWALL) != null);
							}
							// Draw object
							if (tile.getMapData(TilePart.O_OBJECT) != null && (tile.getMapData(TilePart.O_OBJECT).getBigWall() < 6 || tile.getMapData(TilePart.O_OBJECT).getBigWall() == 9))
							{
								tmpSurface = tile.getSprite((int)TilePart.O_OBJECT);
								if (tmpSurface != null)
								{
									if (tile.getObstacle((int)TilePart.O_OBJECT))
										tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y - tile.getMapData(TilePart.O_OBJECT).getYOffset(), obstacleShade, false);
									else
										tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y - tile.getMapData(TilePart.O_OBJECT).getYOffset(), tileShade, false);
								}
							}
							// draw an item on top of the floor (if any)
							int sprite = tile.getTopItemSprite();
							if (sprite != -1)
							{
								tmpSurface = _game.getMod().getSurfaceSet("FLOOROB.PCK").getFrame(sprite);
								tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y + tile.getTerrainLevel(), tileShade, false);
							}

						}

						// check if we got bullet && it is in Field Of View
						if (_projectile != null && _projectileInFOV)
						{
							tmpSurface = null;
							if (_projectile.getItem() != null)
							{
								tmpSurface = _projectile.getSprite();

								Position voxelPos = _projectile.getPosition();
								// draw shadow on the floor
								voxelPos.z = _save.getTileEngine().castedShade(voxelPos);
								if (voxelPos.x / 16 >= itX &&
									voxelPos.y / 16 >= itY &&
									voxelPos.x / 16 <= itX+1 &&
									voxelPos.y / 16 <= itY+1 &&
									voxelPos.z / 24 == itZ &&
									_save.getTileEngine().isVoxelVisible(voxelPos))
								{
									_camera.convertVoxelToScreen(voxelPos, out bulletPositionScreen);
									tmpSurface.blitNShade(surface, bulletPositionScreen.x - 16, bulletPositionScreen.y - 26, 16);
								}

								voxelPos = _projectile.getPosition();
								// draw thrown object
								if (voxelPos.x / 16 >= itX &&
									voxelPos.y / 16 >= itY &&
									voxelPos.x / 16 <= itX+1 &&
									voxelPos.y / 16 <= itY+1 &&
									voxelPos.z / 24 == itZ &&
									_save.getTileEngine().isVoxelVisible(voxelPos))
								{
									_camera.convertVoxelToScreen(voxelPos, out bulletPositionScreen);
									tmpSurface.blitNShade(surface, bulletPositionScreen.x - 16, bulletPositionScreen.y - 26, 0);
								}

							}
							else
							{
								// draw bullet on the correct tile
								if (itX >= bulletLowX && itX <= bulletHighX && itY >= bulletLowY && itY <= bulletHighY)
								{
									int begin = 0;
									int end = BULLET_SPRITES;
									int direction = 1;
									if (_projectile.isReversed())
									{
										begin = BULLET_SPRITES - 1;
										end = -1;
										direction = -1;
									}

									for (int i = begin; i != end; i += direction)
									{
										tmpSurface = _projectileSet.getFrame(_projectile.getParticle(i));
										if (tmpSurface != null)
										{
											Position voxelPos = _projectile.getPosition(1-i);
											// draw shadow on the floor
											voxelPos.z = _save.getTileEngine().castedShade(voxelPos);
											if (voxelPos.x / 16 == itX &&
												voxelPos.y / 16 == itY &&
												voxelPos.z / 24 == itZ &&
												_save.getTileEngine().isVoxelVisible(voxelPos))
											{
												_camera.convertVoxelToScreen(voxelPos, out bulletPositionScreen);
												bulletPositionScreen.x -= tmpSurface.getWidth() / 2;
												bulletPositionScreen.y -= tmpSurface.getHeight() / 2;
												tmpSurface.blitNShade(surface, bulletPositionScreen.x, bulletPositionScreen.y, 16);
											}

											// draw bullet itself
											voxelPos = _projectile.getPosition(1-i);
											if (voxelPos.x / 16 == itX &&
												voxelPos.y / 16 == itY &&
												voxelPos.z / 24 == itZ &&
												_save.getTileEngine().isVoxelVisible(voxelPos))
											{
												_camera.convertVoxelToScreen(voxelPos, out bulletPositionScreen);
												bulletPositionScreen.x -= tmpSurface.getWidth() / 2;
												bulletPositionScreen.y -= tmpSurface.getHeight() / 2;
												tmpSurface.blitNShade(surface, bulletPositionScreen.x, bulletPositionScreen.y, 0);
											}
										}
									}
								}
							}
						}
						unit = tile.getUnit();
						// Draw soldier from this tile or below
						drawUnit(surface, tile, tile, screenPosition, tileShade, obstacleShade, topLayer);

						// special handling for a moving unit in forground of tile.
						const int frontPosSize = 5;
						Position[] frontPos =
						{
							new Position(-1, +1, 0),
							new Position(0, +1, 0),
							new Position(+1, +1, 0),
							new Position(+1, 0, 0),
							new Position(+1, -1, 0),
						};

						for (int f = 0; f < frontPosSize; ++f)
						{
							drawUnit(surface, _save.getTile(mapPosition + frontPos[f]), tile, screenPosition, tileShade, obstacleShade, topLayer);
						}

						// Draw smoke/fire
						if (tile.getSmoke() != 0 && tile.isDiscovered(2))
						{
							frameNumber = 0;
							int shade = 0;
							if (tile.getFire() == 0)
							{
								if (_save.getDepth() > 0)
								{
									frameNumber += Mod.Mod.UNDERWATER_SMOKE_OFFSET;
								}
								else
								{
									frameNumber += Mod.Mod.SMOKE_OFFSET;
								}
								frameNumber += (int)Math.Floor((tile.getSmoke() / 6.0) - 0.1); // see http://www.ufopaedia.org/images/c/cb/Smoke.gif
								shade = tileShade;
							}

							if ((_animFrame / 2) + tile.getAnimationOffset() > 3)
							{
								frameNumber += ((_animFrame / 2) + tile.getAnimationOffset() - 4);
							}
							else
							{
								frameNumber += (_animFrame / 2) + tile.getAnimationOffset();
							}
							tmpSurface = _game.getMod().getSurfaceSet("SMOKE.PCK").getFrame(frameNumber);
							tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y, shade);
						}

						//draw particle clouds
						foreach (var i in tile.getParticleCloud())
						{
							int vaporX = (int)(screenPosition.x + i.getX());
							int vaporY = (int)(screenPosition.y + i.getY());
							if ((int)(_transparencies.Count) >= (i.getColor() + 1) * 1024)
							{
								switch (i.getSize())
								{
									case 3:
										surface.setPixel(vaporX+1, vaporY+1, _transparencies[(i.getColor() * 1024) + (i.getOpacity() * 256) + surface.getPixel(vaporX+1, vaporY+1)]);
										goto case 2;
									case 2:
										surface.setPixel(vaporX + 1, vaporY, _transparencies[(i.getColor() * 1024) + (i.getOpacity() * 256) + surface.getPixel(vaporX + 1, vaporY)]);
										goto case 1;
									case 1:
										surface.setPixel(vaporX, vaporY + 1, _transparencies[(i.getColor() * 1024) + (i.getOpacity() * 256) + surface.getPixel(vaporX, vaporY + 1)]);
										goto default;
									default:
										surface.setPixel(vaporX, vaporY, _transparencies[(i.getColor() * 1024) + (i.getOpacity() * 256) + surface.getPixel(vaporX, vaporY)]);
										break;
								}
							}
						}

						// Draw Path Preview
						if (tile.getPreview() != -1 && tile.isDiscovered(0) && ((_previewSetting & PathPreview.PATH_ARROWS) != 0))
						{
							if (itZ > 0 && tile.hasNoFloor(_save.getTile(tile.getPosition() + new Position(0,0,-1))))
							{
								tmpSurface = _game.getMod().getSurfaceSet("Pathfinding").getFrame(11);
								if (tmpSurface != null)
								{
									tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y+2, 0, false, tile.getMarkerColor());
								}
							}
							tmpSurface = _game.getMod().getSurfaceSet("Pathfinding").getFrame(tile.getPreview());
							if (tmpSurface != null)
							{
								tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y + tile.getTerrainLevel(), 0, false, tileColor);
							}
						}
						if (!tile.isVoid())
						{
							// Draw object
							if (tile.getMapData(TilePart.O_OBJECT) != null && tile.getMapData(TilePart.O_OBJECT).getBigWall() >= 6 && tile.getMapData(TilePart.O_OBJECT).getBigWall() != 9)
							{
								tmpSurface = tile.getSprite((int)TilePart.O_OBJECT);
								if (tmpSurface != null)
								{
									if (tile.getObstacle((int)TilePart.O_OBJECT))
										tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y - tile.getMapData(TilePart.O_OBJECT).getYOffset(), obstacleShade, false);
									else
										tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y - tile.getMapData(TilePart.O_OBJECT).getYOffset(), tileShade, false);
								}
							}
						}
						// Draw cursor front
						if (_cursorType != CursorType.CT_NONE && _selectorX > itX - _cursorSize && _selectorY > itY - _cursorSize && _selectorX < itX+1 && _selectorY < itY+1 && !_save.getBattleState().getMouseOverIcons())
						{
							if (_camera.getViewLevel() == itZ)
							{
								if (_cursorType != CursorType.CT_AIM)
								{
									if (unit != null && (unit.getVisible() || _save.getDebugMode()))
										frameNumber = 3 + (_animFrame % 2); // yellow box
									else
										frameNumber = 3; // red box
								}
								else
								{
									if (unit != null && (unit.getVisible() || _save.getDebugMode()))
										frameNumber = 7 + (_animFrame / 2); // yellow animated crosshairs
									else
										frameNumber = 6; // red static crosshairs
								}
								tmpSurface = _game.getMod().getSurfaceSet("CURSOR.PCK").getFrame(frameNumber);
								tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y, 0);

								// UFO extender accuracy: display adjusted accuracy value on crosshair in real-time.
								if (_cursorType == CursorType.CT_AIM && Options.battleUFOExtenderAccuracy)
								{
									BattleAction action = _save.getBattleGame().getCurrentAction();
									RuleItem weapon = action.weapon.getRules();
									int accuracy = action.actor.getFiringAccuracy(action.type, action.weapon);
									int distanceSq = _save.getTileEngine().distanceUnitToPositionSq(action.actor, new Position(itX, itY,itZ), false);
									int distance = (int)Math.Ceiling(Math.Sqrt((float)distanceSq));
									int upperLimit = 200;
									int lowerLimit = weapon.getMinRange();
									switch (action.type)
									{
										case BattleActionType.BA_AIMEDSHOT:
											upperLimit = weapon.getAimRange();
											break;
										case BattleActionType.BA_SNAPSHOT:
											upperLimit = weapon.getSnapRange();
											break;
										case BattleActionType.BA_AUTOSHOT:
											upperLimit = weapon.getAutoRange();
											break;
										default:
											break;
									}
									// at this point, let's assume the shot is adjusted and set the text amber.
									_txtAccuracy.setColor((byte)(Palette.blockOffset((byte)(Pathfinding.yellow - 1)) -1));

									if (distance > upperLimit)
									{
										accuracy -= (distance - upperLimit) * weapon.getDropoff();
									}
									else if (distance < lowerLimit)
									{
										accuracy -= (lowerLimit - distance) * weapon.getDropoff();
									}
									else
									{
										// no adjustment made? set it to green.
										_txtAccuracy.setColor((byte)(Palette.blockOffset((byte)(Pathfinding.green - 1)) -1));
									}

									bool outOfRange = distanceSq > weapon.getMaxRangeSq();
									// special handling for short ranges and diagonals
									if (outOfRange)
									{
										// special handling for maxRange 1: allow it to target diagonally adjacent tiles (one diagonal move)
										if (weapon.getMaxRange() == 1 && distanceSq <= 3)
										{
											outOfRange = false;
										}
										// special handling for maxRange 2: allow it to target diagonally adjacent tiles (one diagonal move + one straight move)
										else if (weapon.getMaxRange() == 2 && distanceSq <= 6)
										{
											outOfRange = false;
										}
									}
									// zero accuracy or out of range: set it red.
									if (accuracy <= 0 || outOfRange)
									{
										accuracy = 0;
										_txtAccuracy.setColor((byte)(Palette.blockOffset((byte)(Pathfinding.red - 1)) -1));
									}
									_txtAccuracy.setText(Unicode.formatPercentage(accuracy));
									_txtAccuracy.draw();
									_txtAccuracy.blitNShade(surface, screenPosition.x, screenPosition.y, 0);
								}
							}
							else if (_camera.getViewLevel() > itZ)
							{
								frameNumber = 5; // blue box
								tmpSurface = _game.getMod().getSurfaceSet("CURSOR.PCK").getFrame(frameNumber);
								tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y, 0);
							}
							if ((int)_cursorType > 2 && _camera.getViewLevel() == itZ)
							{
								int[] frame = {0, 0, 0, 11, 13, 15};
								tmpSurface = _game.getMod().getSurfaceSet("CURSOR.PCK").getFrame(frame[(int)_cursorType] + (_animFrame / 4));
								tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y, 0);
							}
						}

						// Draw waypoints if any on this tile
						int waypid = 1;
						int waypXOff = 2;
						int waypYOff = 2;

						foreach (var i in _waypoints)
						{
							if (i == mapPosition)
							{
								if (waypXOff == 2 && waypYOff == 2)
								{
									tmpSurface = _game.getMod().getSurfaceSet("CURSOR.PCK").getFrame(7);
									tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y, 0);
								}
								if (_save.getBattleGame().getCurrentAction().type == BattleActionType.BA_LAUNCH)
								{
									_numWaypid.setValue((uint)waypid);
									_numWaypid.draw();
									_numWaypid.blitNShade(surface, screenPosition.x + waypXOff, screenPosition.y + waypYOff, 0);

									waypXOff += waypid > 9 ? 8 : 6;
									if (waypXOff >= 26)
									{
										waypXOff = 2;
										waypYOff += 8;
									}
								}
							}
							waypid++;
						}
					}
				}
			}
		}
		if (pathfinderTurnedOn)
		{
			if (_numWaypid != null)
			{
				_numWaypid.setBordered(true); // give it a border for the pathfinding display, makes it more visible on snow, etc.
			}
			for (int itZ = beginZ; itZ <= endZ; itZ++)
			{
				for (int itX = beginX; itX <= endX; itX++)
				{
					for (int itY = beginY; itY <= endY; itY++)
					{
						mapPosition = new Position(itX, itY, itZ);
						_camera.convertMapToScreen(mapPosition, out screenPosition);
						screenPosition += _camera.getMapOffset();

						// only render cells that are inside the surface
						if (screenPosition.x > -_spriteWidth && screenPosition.x < surface.getWidth() + _spriteWidth &&
							screenPosition.y > -_spriteHeight && screenPosition.y < surface.getHeight() + _spriteHeight )
						{
							tile = _save.getTile(mapPosition);
							Tile tileBelow = _save.getTile(mapPosition - new Position(0,0,1));
							if (tile == null || !tile.isDiscovered(0) || tile.getPreview() == -1)
								continue;
							int adjustment = -tile.getTerrainLevel();
							if ((_previewSetting & PathPreview.PATH_ARROWS) != 0)
							{
								if (itZ > 0 && tile.hasNoFloor(tileBelow))
								{
									tmpSurface = _game.getMod().getSurfaceSet("Pathfinding").getFrame(23);
									if (tmpSurface != null)
									{
										tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y+2, 0, false, tile.getMarkerColor());
									}
								}
								int overlay = tile.getPreview() + 12;
								tmpSurface = _game.getMod().getSurfaceSet("Pathfinding").getFrame(overlay);
								if (tmpSurface != null)
								{
									tmpSurface.blitNShade(surface, screenPosition.x, screenPosition.y - adjustment, 0, false, tile.getMarkerColor());
								}
							}

							if ((_previewSetting & PathPreview.PATH_TU_COST) != 0 && tile.getTUMarker() > -1)
							{
								int off = tile.getTUMarker() > 9 ? 5 : 3;
								if (_save.getSelectedUnit() != null && _save.getSelectedUnit().getArmor().getSize() > 1)
								{
									adjustment += 1;
									if (!((_previewSetting & PathPreview.PATH_ARROWS) != 0))
									{
										adjustment += 7;
									}
								}
								_numWaypid.setValue((uint)tile.getTUMarker());
								_numWaypid.draw();
								if (!((_previewSetting & PathPreview.PATH_ARROWS) != 0))
								{
									_numWaypid.blitNShade(surface, screenPosition.x + 16 - off, screenPosition.y + (29-adjustment), 0, false, tile.getMarkerColor() );
								}
								else
								{
									_numWaypid.blitNShade(surface, screenPosition.x + 16 - off, screenPosition.y + (22-adjustment), 0);
								}
							}
						}
					}
				}
			}
			if (_numWaypid != null)
			{
				_numWaypid.setBordered(false); // make sure we remove the border in case it's being used for missile waypoints.
			}
		}
		unit = (BattleUnit)_save.getSelectedUnit();
		if (unit != null && (_save.getSide() == UnitFaction.FACTION_PLAYER || _save.getDebugMode()) && unit.getPosition().z <= _camera.getViewLevel())
		{
			_camera.convertMapToScreen(unit.getPosition(), out screenPosition);
			screenPosition += _camera.getMapOffset();
			var offset = new Position();
			calculateWalkingOffset(unit, offset, out _);
			if (unit.getArmor().getSize() > 1)
			{
				offset.y += 4;
			}
			offset.y += 24 - (unit.getHeight() + unit.getFloatHeight());
			if (unit.isKneeled())
			{
				offset.y -= 2;
			}
			if (this.getCursorType() != CursorType.CT_NONE)
			{
				_arrow.blitNShade(surface, screenPosition.x + offset.x + (_spriteWidth / 2) - (_arrow.getWidth() / 2), screenPosition.y + offset.y - _arrow.getHeight() + arrowBob[_animFrame], 0);
			}
		}
		_numWaypid = null;

		// check if we got big explosions
		if (_explosionInFOV)
		{
			// big explosions cause the screen to flash as bright as possible before any explosions are actually drawn.
			// this causes everything to look like EGA for a single frame.
			if (_flashScreen)
			{
				for (int x = 0, y = 0; x < surface.getWidth() && y < surface.getHeight();)
				{
					byte pixel = surface.getPixel(x, y);
					pixel = (byte)((pixel / 16) * 16);
					surface.setPixelIterative(ref x, ref y, pixel);
				}
				_flashScreen = false;
			}
			else
			{
				foreach (var i in _explosions)
				{
					_camera.convertVoxelToScreen(i.getPosition(), out bulletPositionScreen);
					if (i.isBig())
					{
						if (i.getCurrentFrame() >= 0)
						{
							tmpSurface = _game.getMod().getSurfaceSet("X1.PCK").getFrame(i.getCurrentFrame());
							tmpSurface.blitNShade(surface, bulletPositionScreen.x - (tmpSurface.getWidth() / 2), bulletPositionScreen.y - (tmpSurface.getHeight() / 2), 0);
						}
					}
					else if (i.isHit())
					{
						tmpSurface = _game.getMod().getSurfaceSet("HIT.PCK").getFrame(i.getCurrentFrame());
						tmpSurface.blitNShade(surface, bulletPositionScreen.x - 15, bulletPositionScreen.y - 25, 0);
					}
					else
					{
						tmpSurface = _game.getMod().getSurfaceSet("SMOKE.PCK").getFrame(i.getCurrentFrame());
						tmpSurface.blitNShade(surface, bulletPositionScreen.x - 15, bulletPositionScreen.y - 15, 0);
					}
				}
			}
		}
		surface.unlock();
	}

	/**
	 * Draw part of unit graphic that overlap current tile.
	 * @param surface
	 * @param unitTile
	 * @param currTile
	 * @param currTileScreenPosition
	 * @param shade
	 * @param obstacleShade unitShade override for no LOF obstacle indicator
	 * @param topLayer
	 */
	void drawUnit(Surface surface, Tile unitTile, Tile currTile, Position currTileScreenPosition, int shade, int obstacleShade, bool topLayer)
	{
		const int tileFoorWidth = 32;
		const int tileFoorHeight = 16;
		const int tileHeight = 40;

		if (unitTile == null)
		{
			return;
		}
		BattleUnit bu = unitTile.getUnit();
		var unitOffset = new Position();
		bool unitFromBelow = false;
		if (bu == null)
		{
			Tile below = _save.getTile(unitTile.getPosition() + new Position(0,0,-1));
			if (below != null && unitTile.hasNoFloor(below))
			{
				bu = below.getUnit();
				if (bu == null)
				{
					return;
				}
				unitFromBelow = true;
			}
			else
			{
				return;
			}
		}

		if (!(bu.getVisible() || _save.getDebugMode()))
		{
			return;
		}

		unitOffset.x = unitTile.getPosition().x - bu.getPosition().x;
		unitOffset.y = unitTile.getPosition().y - bu.getPosition().y;
		int part = unitOffset.x + unitOffset.y*2;
		Surface tmpSurface = bu.getCache(part);
		if (tmpSurface == null)
		{
			return;
		}

		bool moving = bu.getStatus() == UnitStatus.STATUS_WALKING || bu.getStatus() == UnitStatus.STATUS_FLYING;
		int bonusWidth = moving ? 0 : tileFoorWidth;
		int topMargin = 0;
		int bottomMargin = 0;

		//if unit is from below then we draw only part that in in tile
		if (unitFromBelow)
		{
			bottomMargin = -tileFoorHeight / 2;
			topMargin = tileFoorHeight;
		}
		else if (topLayer)
		{
			topMargin = 2 * tileFoorHeight;
		}
		else
		{
			Tile top = _save.getTile(unitTile.getPosition() + new Position(0, 0, +1));
			if (top != null && top.hasNoFloor(unitTile))
			{
				topMargin = -tileFoorHeight / 2;
			}
			else
			{
				topMargin = tileFoorHeight;
			}
		}

		GraphSubset mask = new GraphSubset(tileFoorWidth + bonusWidth, tileHeight + topMargin + bottomMargin).offset(currTileScreenPosition.x - bonusWidth / 2, currTileScreenPosition.y - topMargin);

		if (moving)
		{
			GraphSubset leftMask = mask.offset(-tileFoorWidth/2, 0);
			GraphSubset rightMask = mask.offset(+tileFoorWidth/2, 0);
			int direction = bu.getDirection();
			Position partCurr = currTile.getPosition();
			Position partDest = bu.getDestination() + unitOffset;
			Position partLast = bu.getLastPosition() + unitOffset;
			bool isTileDestPos = positionHaveSameXY(partDest, partCurr);
			bool isTileLastPos = positionHaveSameXY(partLast, partCurr);

			//adjusting mask
			if (positionHaveSameXY(partLast, partDest))
			{
				if (currTile == unitTile)
				{
					//no change
				}
				else
				{
					//nothing to draw
					return;
				}
			}
			else if (isTileDestPos)
			{
				//unit is moving to this tile
				switch (direction)
				{
				case 0:
				case 1:
					mask = GraphSubset.intersection(mask, rightMask);
					break;
				case 2:
					//no change
					break;
				case 3:
					//no change
					break;
				case 4:
					//no change
					break;
				case 5:
				case 6:
					mask = GraphSubset.intersection(mask, leftMask);
					break;
				case 7:
					//nothing to draw
					return;
				}
			}
			else if (isTileLastPos)
			{
				//unit is exiting this tile
				switch (direction)
				{
				case 0:
					//no change
					break;
				case 1:
				case 2:
					mask = GraphSubset.intersection(mask, leftMask);
					break;
				case 3:
					//nothing to draw
					return;
				case 4:
				case 5:
					mask = GraphSubset.intersection(mask, rightMask);
					break;
				case 6:
					//no change
					break;
				case 7:
					//no change
					break;
				}
			}
			else
			{
				Position leftPos = partCurr + new Position(-1, 0, 0);
				Position rightPos = partCurr + new Position(0, -1, 0);
				if (!topLayer && (partDest.z > partCurr.z || partLast.z > partCurr.z))
				{
					//unit change layers, it will be drawn by upper layer not lower.
					return;
				}
				else if (
					(direction == 1 && (partDest == rightPos || partLast == leftPos)) ||
					(direction == 5 && (partDest == leftPos || partLast == rightPos)))
				{
					mask = new GraphSubset(tileFoorWidth, tileHeight + 2 * tileFoorHeight).offset(currTileScreenPosition.x, currTileScreenPosition.y - 2 * tileFoorHeight);
				}
				else
				{
					//unit is not moving close to tile
					return;
				}
			}
		}
		else if (unitTile != currTile)
		{
			return;
		}

		Position tileScreenPosition;
		_camera.convertMapToScreen(unitTile.getPosition() + new Position(0,0, unitFromBelow ? -1 : 0), out tileScreenPosition);
		tileScreenPosition += _camera.getMapOffset();

		// draw unit
		var offset = new Position();
		int shadeOffset;
		calculateWalkingOffset(bu, offset, out shadeOffset);
		int tileShade = currTile.isDiscovered(2) ? currTile.getShade() : 16;
		int unitShade = (tileShade * (16 - shadeOffset) + shade * shadeOffset) / 16;
		if (!moving && unitTile.getObstacle(4))
		{
			unitShade = obstacleShade;
		}
		tmpSurface.blitNShade(surface, tileScreenPosition.x + offset.x - _spriteWidth / 2, tileScreenPosition.y + offset.y, unitShade, mask);
		// draw fire
		if (bu.getFire() > 0)
		{
			int frameNumber = 4 + (_animFrame / 2);
			tmpSurface = _game.getMod().getSurfaceSet("SMOKE.PCK").getFrame(frameNumber);
			tmpSurface.blitNShade(surface, tileScreenPosition.x + offset.x, tileScreenPosition.y + offset.y, 0, mask);
		}
		if (bu.getBreathFrame() > 0)
		{
			tmpSurface = _game.getMod().getSurfaceSet("BREATH-1.PCK").getFrame(bu.getBreathFrame() - 1);
			// lower the bubbles for shorter or kneeling units.
			offset.y += (22 - bu.getHeight());
			if (tmpSurface != null)
			{
				tmpSurface.blitNShade(surface, tileScreenPosition.x + offset.x, tileScreenPosition.y + offset.y - 30, tileShade, mask);
			}
		}
	}

	/**
	 * Check two positions if have same XY cords
	 */
	static bool positionHaveSameXY(Position a, Position b) =>
		a.x == b.x && a.y == b.y;

	/**
	 * Calculates the offset of a soldier, when it is walking in the middle of 2 tiles.
	 * @param unit Pointer to BattleUnit.
	 * @param offset Pointer to the offset to return the calculation.
	 */
	void calculateWalkingOffset(BattleUnit unit, Position offset, out int shadeOffset)
	{
		int[] offsetX = { 1, 1, 1, 0, -1, -1, -1, 0 };
		int[] offsetY = { 1, 0, -1, -1, -1, 0, 1, 1 };
		int phase = unit.getWalkingPhase() + unit.getDiagonalWalkingPhase();
		int dir = unit.getDirection();
		int midphase = 4 + 4 * (dir % 2);
		int endphase = 8 + 8 * (dir % 2);
		int size = unit.getArmor().getSize();

		//if (shadeOffset)
		//{
			shadeOffset = endphase == 16 ? phase : phase * 2;
		//}

		if (size > 1)
		{
			if (dir < 1 || dir > 5)
				midphase = endphase;
			else if (dir == 5)
				midphase = 12;
			else if (dir == 1)
				midphase = 5;
			else
				midphase = 1;
		}
		if (unit.getVerticalDirection() != 0)
		{
			midphase = 4;
			endphase = 8;
		}
		else
		if ((unit.getStatus() == UnitStatus.STATUS_WALKING || unit.getStatus() == UnitStatus.STATUS_FLYING))
		{
			if (phase < midphase)
			{
				offset.x = phase * 2 * offsetX[dir];
				offset.y = - phase * offsetY[dir];
			}
			else
			{
				offset.x = (phase - endphase) * 2 * offsetX[dir];
				offset.y = - (phase - endphase) * offsetY[dir];
			}
		}

		// If we are walking in between tiles, interpolate it's terrain level.
		if (unit.getStatus() == UnitStatus.STATUS_WALKING || unit.getStatus() == UnitStatus.STATUS_FLYING)
		{
			if (phase < midphase)
			{
				int fromLevel = getTerrainLevel(unit.getPosition(), size);
				int toLevel = getTerrainLevel(unit.getDestination(), size);
				if (unit.getPosition().z > unit.getDestination().z)
				{
					// going down a level, so toLevel 0 becomes +24, -8 becomes  16
					toLevel += 24*(unit.getPosition().z - unit.getDestination().z);
				}else if (unit.getPosition().z < unit.getDestination().z)
				{
					// going up a level, so toLevel 0 becomes -24, -8 becomes -16
					toLevel = -24*(unit.getDestination().z - unit.getPosition().z) + Math.Abs(toLevel);
				}
				offset.y += ((fromLevel * (endphase - phase)) / endphase) + ((toLevel * (phase)) / endphase);
			}
			else
			{
				// from phase 4 onwards the unit behind the scenes already is on the destination tile
				// we have to get it's last position to calculate the correct offset
				int fromLevel = getTerrainLevel(unit.getLastPosition(), size);
				int toLevel = getTerrainLevel(unit.getDestination(), size);
				if (unit.getLastPosition().z > unit.getDestination().z)
				{
					// going down a level, so fromLevel 0 becomes -24, -8 becomes -32
					fromLevel -= 24*(unit.getLastPosition().z - unit.getDestination().z);
				}else if (unit.getLastPosition().z < unit.getDestination().z)
				{
					// going up a level, so fromLevel 0 becomes +24, -8 becomes 16
					fromLevel = 24*(unit.getDestination().z - unit.getLastPosition().z) - Math.Abs(fromLevel);
				}
				offset.y += ((fromLevel * (endphase - phase)) / endphase) + ((toLevel * (phase)) / endphase);
			}
		}
		else
		{
			offset.y += getTerrainLevel(unit.getPosition(), size);
			if (_save.getDepth() > 0)
			{
				unit.setFloorAbove(false);

				// make sure this unit isn't obscured by the floor above him, otherwise it looks weird.
				if (_camera.getViewLevel() > unit.getPosition().z)
				{
					for (int z = Math.Min(_camera.getViewLevel(), _save.getMapSizeZ() - 1); z != unit.getPosition().z; --z)
					{
						if (!_save.getTile(new Position(unit.getPosition().x, unit.getPosition().y, z)).hasNoFloor(null))
						{
							unit.setFloorAbove(true);
							break;
						}
					}
				}
			}
		}
	}

	/**
	  * Terrainlevel goes from 0 to -24. For a larger sized unit, we need to pick the highest terrain level, which is the lowest number...
	  * @param pos Position.
	  * @param size Size of the unit we want to get the level from.
	  * @return terrainlevel.
	  */
	int getTerrainLevel(Position pos, int size)
	{
		int lowestlevel = 0;

		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				int l = _save.getTile(pos + new Position(x,y,0)).getTerrainLevel();
				if (l < lowestlevel)
					lowestlevel = l;
			}
		}

		return lowestlevel;
	}

	/**
	 * Handles mouse presses on the map.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void mousePress(Action action, State state)
	{
		base.mousePress(action, state);
		_camera.mousePress(action, state);
	}

	/**
	 * Handles mouse releases on the map.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void mouseRelease(Action action, State state)
	{
		base.mouseRelease(action, state);
		_camera.mouseRelease(action, state);
	}

	/**
	 * Handles mouse over events on the map.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void mouseOver(Action action, State state)
	{
		base.mouseOver(action, state);
		_camera.mouseOver(action, state);
		_mouseX = (int)action.getAbsoluteXMouse();
		_mouseY = (int)action.getAbsoluteYMouse();
		setSelectorPosition(_mouseX, _mouseY);
	}

	/**
	 * Handles keyboard presses on the map.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void keyboardPress(Action action, State state)
	{
		base.keyboardPress(action, state);
		_camera.keyboardPress(action, state);
	}

	/**
	 * Handles keyboard releases on the map.
	 * @param action Pointer to an action.
	 * @param state State that the action handlers belong to.
	 */
	protected override void keyboardRelease(Action action, State state)
	{
		base.keyboardRelease(action, state);
		_camera.keyboardRelease(action, state);
	}
}
