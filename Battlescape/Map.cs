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
    Map(Game game, int width, int height, int x, int y, int visibleMapHeight) : base(width, height, x, y)
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
    void setSelectorPosition(int mx, int my)
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
    void init()
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
}
