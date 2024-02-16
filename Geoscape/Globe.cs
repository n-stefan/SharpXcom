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

///helper class for `Globe` for drawing earth globe with shadows
struct GlobeStaticData
{
    ///size of x & y of noise surface
    internal static int random_surf_size;
    ///array of shading gradient
    internal static short[] shade_gradient = new short[240];

    //initialization
    public GlobeStaticData()
    {
        random_surf_size = 60;

        //filling terminator gradient LUT
        for (int i = 0; i < 240; ++i)
        {
            int j = i - 120;

            if (j < -66) j = -16;
            else
            if (j < -48) j = -15;
            else
            if (j < -33) j = -14;
            else
            if (j < -22) j = -13;
            else
            if (j < -15) j = -12;
            else
            if (j < -11) j = -11;
            else
            if (j < -9) j = -10;

            if (j > 120) j = 19;
            else
            if (j > 98) j = 18;
            else
            if (j > 86) j = 17;
            else
            if (j > 74) j = 16;
            else
            if (j > 54) j = 15;
            else
            if (j > 38) j = 14;
            else
            if (j > 26) j = 13;
            else
            if (j > 18) j = 12;
            else
            if (j > 13) j = 11;
            else
            if (j > 10) j = 10;
            else
            if (j > 8) j = 9;

            shade_gradient[i] = (short)(j + 16);
        }
    }

    /**
	 * Function returning normal vector of sphere surface
	 * @param ox x cord of sphere center
	 * @param oy y cord of sphere center
	 * @param r radius of sphere
	 * @param x cord of point where we getting this vector
	 * @param y cord of point where we getting this vector
	 * @return normal vector of sphere surface
	 */
    internal static Cord circle_norm(double ox, double oy, double r, double x, double y)
    {
        double limit = r * r;
        double norm = 1.0 / r;
        var ret = new Cord();
        ret.x = (x - ox);
        ret.y = (y - oy);
        double temp = (ret.x) * (ret.x) + (ret.y) * (ret.y);
        if (limit > temp)
        {
            ret.x *= norm;
            ret.y *= norm;
            ret.z = Math.Sqrt(limit - temp) * norm;
            return ret;
        }
        else
        {
            ret.x = 0.0;
            ret.y = 0.0;
            ret.z = 0.0;
            return ret;
        }
    }
}

struct CreateShadow : IColorFunc<byte, Cord, Cord, short, int>
{
	internal static byte getShadowValue(Cord earth, Cord sun, short noise)
	{
		Cord temp = earth;
		//diff
		temp -= sun;
		//norm
		temp.x *= temp.x;
		temp.y *= temp.y;
		temp.z *= temp.z;
		temp.x += temp.z + temp.y;
		//we have norm of distance between 2 vectors, now stored in `x`

		temp.x -= 2;
		temp.x *= 125.0;

		if (temp.x < -110)
			temp.x = -31;
		else if (temp.x > 120)
			temp.x = 50;
		else
			temp.x = GlobeStaticData.shade_gradient[(short)temp.x + 120];

		temp.x -= noise;

		return (byte)Math.Clamp(temp.x, 0.0, 31.0);
	}

	internal static bool isOcean(byte dest) =>
		Globe.OCEAN_SHADING && dest >= Globe.OCEAN_COLOR && dest < Globe.OCEAN_COLOR + 32;

	internal static byte getOceanShadow(byte shadow) =>
        (byte)(Globe.OCEAN_COLOR + shadow);

	internal static byte getLandShadow(byte dest, byte shadow)
	{
		if (shadow == 0) return dest;
		int s = shadow / 3;
		int e = dest + s;
		int d = dest & ColorGroup;
		if (e > d + ColorShade)
			return (byte)(d + ColorShade);
		return (byte)e;
	}

	public void func(ref byte dest, Cord earth, Cord sun, short noise, int _)
	{
		if (dest != 0 && earth.z != 0)
		{
			byte shadow = getShadowValue(earth, sun, noise);
			//this pixel is ocean
			if (isOcean(dest))
			{
				dest = getOceanShadow(shadow);
			}
			//this pixel is land
			else
			{
				dest = getLandShadow(dest, shadow);
			}
		}
		else
		{
			dest = 0;
		}
	}
}

/**
 * Interactive globe view of the world.
 * Takes a flat world map made out of land polygons with
 * polar coordinates and renders it as a 3D-looking globe
 * with cartesian coordinates that the player can interact with.
 */
internal class Globe : InteractiveSurface
{
    const uint DOGFIGHT_ZOOM = 3;
    const int CITY_MARKER = 8;
    const double ROTATE_LATITUDE = 0.06;
    const double ROTATE_LONGITUDE = 0.10;
    const int NEAR_RADIUS = 25;

    short _cenX, _cenY;
    double _cenLon, _cenLat, _rotLon, _rotLat, _hoverLon, _hoverLat;
    double _craftLon, _craftLat, _craftRange;
    Game _game;
    bool _hover, _craft;
    int _blink;
    bool _isMouseScrolling, _isMouseScrolled;
    int _xBeforeMouseScrolling, _yBeforeMouseScrolling;
    double _lonBeforeMouseScrolling, _latBeforeMouseScrolling;
    uint _mouseScrollingStartTime;
    int _totalMouseMoveX, _totalMouseMoveY;
    bool _mouseMovedOverThreshold;
    RuleGlobe _rules;
    SurfaceSet _texture, _markerSet;
    Surface _markers, _countries, _radars;
    FastLineClip _clipper;
    Timer _blinkTimer, _rotTimer;
    uint _zoom, _zoomOld, _zoomTexture;
    GlobeStaticData static_data;
    double _radius, _radiusStep;
    List<Polygon> _cacheLand;
    ///data sample used for noise in shading
    List<short> _randomNoiseData;
    ///list of dimension of earth on screen per zoom level
    List<double> _zoomRadius;
    ///normal of each pixel in earth globe per zoom level
    List<List<Cord>> _earthData;

    internal static byte OCEAN_COLOR;
    internal static bool OCEAN_SHADING;
    internal static byte COUNTRY_LABEL_COLOR;
    internal static byte LINE_COLOR;
    internal static byte CITY_LABEL_COLOR;
    internal static byte BASE_LABEL_COLOR;

    /**
     * Sets up a globe with the specified size and position.
     * @param game Pointer to core game.
     * @param cenX X position of the center of the globe.
     * @param cenY Y position of the center of the globe.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal Globe(Game game, int cenX, int cenY, int width, int height, int x, int y) : base(width, height, x, y)
    {
        _cenX = (short)cenX;
        _cenY = (short)cenY;
        _rotLon = 0.0;
        _rotLat = 0.0;
        _hoverLon = 0.0;
        _hoverLat = 0.0;
        _craftLon = 0.0;
        _craftLat = 0.0;
        _craftRange = 0.0;
        _game = game;
        _hover = false;
        _craft = false;
        _blink = -1;
        _isMouseScrolling = false;
        _isMouseScrolled = false;
        _xBeforeMouseScrolling = 0;
        _yBeforeMouseScrolling = 0;
        _lonBeforeMouseScrolling = 0.0;
        _latBeforeMouseScrolling = 0.0;
        _mouseScrollingStartTime = 0;
        _totalMouseMoveX = 0;
        _totalMouseMoveY = 0;
        _mouseMovedOverThreshold = false;

        _rules = game.getMod().getGlobe();
        _texture = new SurfaceSet(_game.getMod().getSurfaceSet("TEXTURE.DAT"));
        _markerSet = new SurfaceSet(_game.getMod().getSurfaceSet("GlobeMarkers"));

        _countries = new Surface(width, height, x, y);
        _markers = new Surface(width, height, x, y);
        _radars = new Surface(width, height, x, y);
        _clipper = new FastLineClip(x, x + width, y, y + height);

        // Animation timers
        _blinkTimer = new Timer(100);
        _blinkTimer.onTimer((SurfaceHandler)blink);
        _blinkTimer.start();
        _rotTimer = new Timer(10);
        _rotTimer.onTimer((SurfaceHandler)rotate);

        _cenLon = _game.getSavedGame().getGlobeLongitude();
        _cenLat = _game.getSavedGame().getGlobeLatitude();
        _zoom = (uint)_game.getSavedGame().getGlobeZoom();
        _zoomOld = _zoom;

        setupRadii(width, height);
        setZoom(_zoom);

        //filling random noise "texture"
        _randomNoiseData = new List<short>(GlobeStaticData.random_surf_size * GlobeStaticData.random_surf_size);
        var random = new Random();
        for (var i = 0; i < _randomNoiseData.Count; ++i)
            _randomNoiseData[i] = (short)(random.Next() % 4);

        cachePolygons();
    }

    /**
     * Deletes the contained surfaces.
     */
    ~Globe()
    {
        _blinkTimer = null;
        _rotTimer = null;
        _countries = null;
        _markers = null;
        _texture = null;
        _markerSet = null;
        _radars = null;
        _clipper = null;

        _cacheLand.Clear();
    }

    /**
     * Changes the current globe zoom factor.
     * @param zoom New zoom.
     */
    void setZoom(uint zoom)
    {
        _zoom = (uint)Math.Clamp(zoom, (uint)0u, _zoomRadius.Count - 1);
        _zoomTexture = (uint)((2 - (int)Math.Floor(_zoom / 2.0)) * (_texture.getTotalFrames() / 3));
        _radius = _zoomRadius[(int)_zoom];
        _game.getSavedGame().setGlobeZoom((int)_zoom);
        if (_isMouseScrolling)
        {
            _lonBeforeMouseScrolling = _cenLon;
            _latBeforeMouseScrolling = _cenLat;
            _totalMouseMoveX = 0; _totalMouseMoveY = 0;
        }
        invalidate();
    }

    /*
     * Set up the Radius of earth at the various zoom levels.
     * @param width the new width of the globe.
     * @param height the new height of the globe.
     */
    void setupRadii(int width, int height)
    {
        _zoomRadius.Clear();

        _zoomRadius.Add(0.45 * height);
        _zoomRadius.Add(0.60 * height);
        _zoomRadius.Add(0.90 * height);
        _zoomRadius.Add(1.40 * height);
        _zoomRadius.Add(2.25 * height);
        _zoomRadius.Add(3.60 * height);

        _radius = _zoomRadius[(int)_zoom];
        _radiusStep = (_zoomRadius[(int)DOGFIGHT_ZOOM] - _zoomRadius[0]) / 10.0;

        _earthData = new List<List<Cord>>(_zoomRadius.Count);
        //filling normal field for each radius

        for (var r = 0; r < _zoomRadius.Count; ++r)
        {
            _earthData[r] = new List<Cord>(width * height);
            for (int j = 0; j < height; ++j)
                for (int i = 0; i < width; ++i)
                {
                    _earthData[r][width * j + i] = GlobeStaticData.circle_norm(width / 2, height / 2, _zoomRadius[r], i + .5, j + .5);
                }
        }
    }

    /**
     * Makes the globe markers blink.
     */
    void blink()
    {
        _blink = -_blink;

        foreach (var frame in _markerSet.getFrames())
        {
            if (frame.Key != CITY_MARKER)
                frame.Value.offset(_blink);
        }

        drawMarkers();
    }

    /**
     * Draws the markers of all the various things going
     * on around the world on top of the globe.
     */
    void drawMarkers()
    {
        _markers.clear();

        // Draw the base markers
        foreach (var @base in _game.getSavedGame().getBases())
        {
            drawTarget(@base, _markers);
        }

        // Draw the waypoint markers
        foreach (var waypoint in _game.getSavedGame().getWaypoints())
        {
            drawTarget(waypoint, _markers);
        }

        // Draw the mission site markers
        foreach (var missionSite in _game.getSavedGame().getMissionSites())
        {
            drawTarget(missionSite, _markers);
        }

        // Draw the alien base markers
        foreach (var alienBase in _game.getSavedGame().getAlienBases())
        {
            drawTarget(alienBase, _markers);
        }

        // Draw the UFO markers
        foreach (var ufo in _game.getSavedGame().getUfos())
        {
            drawTarget(ufo, _markers);
        }

        // Draw the craft markers
        foreach (var @base in _game.getSavedGame().getBases())
        {
            foreach (var craft in @base.getCrafts())
            {
                drawTarget(craft, _markers);
            }
        }
    }

    /**
     * Draws the marker for a specified target on the globe.
     * @param target Pointer to globe target.
     */
    void drawTarget(Target target, Surface surface)
    {
        if (target.getMarker() != -1 && !pointBack(target.getLongitude(), target.getLatitude()))
        {
            short x, y;
            polarToCart(target.getLongitude(), target.getLatitude(), out x, out y);
            Surface marker = _markerSet.getFrame(target.getMarker());
            marker.setX(x - marker.getWidth() / 2);
            marker.setY(y - marker.getHeight() / 2);
            marker.blit(surface);
        }
    }

    /**
     * Converts a polar point into a cartesian point for
     * mapping a polygon onto the 3D-looking globe.
     * @param lon Longitude of the polar point.
     * @param lat Latitude of the polar point.
     * @param x Pointer to the output X position.
     * @param y Pointer to the output Y position.
     */
    void polarToCart(double lon, double lat, out short x, out short y)
    {
        // Orthographic projection
        x = (short)(_cenX + (short)Math.Floor(_radius * Math.Cos(lat) * Math.Sin(lon - _cenLon)));
        y = (short)(_cenY + (short)Math.Floor(_radius * (Math.Cos(_cenLat) * Math.Sin(lat) - Math.Sin(_cenLat) * Math.Cos(lat) * Math.Cos(lon - _cenLon))));
    }

    /**
     * Checks if a polar point is on the back-half of the globe,
     * invisible to the player.
     * @param lon Longitude of the point.
     * @param lat Latitude of the point.
     * @return True if it's on the back, False if it's on the front.
     */
    bool pointBack(double lon, double lat)
    {
        double c = Math.Cos(_cenLat) * Math.Cos(lat) * Math.Cos(lon - _cenLon) + Math.Sin(_cenLat) * Math.Sin(lat);

        return c < 0.0;
    }

    /**
     * Rotates the globe by a set amount. Necessary
     * since the globe keeps rotating while a button
     * is pressed down.
     */
    void rotate()
    {
        _cenLon += _rotLon * ((110 - Options.geoScrollSpeed) / 100.0) / (_zoom + 1);
        _cenLat += _rotLat * ((110 - Options.geoScrollSpeed) / 100.0) / (_zoom + 1);
        _game.getSavedGame().setGlobeLongitude(_cenLon);
        _game.getSavedGame().setGlobeLatitude(_cenLat);
        invalidate();
    }

    /**
     * Takes care of pre-calculating all the polygons currently visible
     * on the globe and caching them so they only need to be recalculated
     * when the globe is actually moved.
     */
    void cachePolygons() =>
        cache(_rules.getPolygons(), _cacheLand);

    /**
     * Caches a set of polygons.
     * @param polygons Pointer to list of polygons.
     * @param cache Pointer to cache.
     */
    void cache(List<Polygon> polygons, List<Polygon> cache)
    {
        // Clear existing cache
        cache.Clear();

        // Pre-calculate values to cache
        foreach (var polygon in polygons)
        {
            // Is quad on the back face?
            double closest = 0.0;
            double z;
            double furthest = 0.0;
            for (int j = 0; j < polygon.getPoints(); ++j)
            {
                z = Math.Cos(_cenLat) * Math.Cos(polygon.getLatitude(j)) * Math.Cos(polygon.getLongitude(j) - _cenLon) + Math.Sin(_cenLat) * Math.Sin(polygon.getLatitude(j));
                if (z > closest)
                    closest = z;
                else if (z < furthest)
                    furthest = z;
            }
            if (-furthest > closest)
                continue;

            Polygon p = new Polygon(polygon);

            // Convert coordinates
            for (int j = 0; j < p.getPoints(); ++j)
            {
                short x, y;
                polarToCart(p.getLongitude(j), p.getLatitude(j), out x, out y);
                p.setX(j, x);
                p.setY(j, y);
            }

            cache.Add(p);
        }
    }

    /**
     * Returns the current globe zoom factor.
     * @return Current zoom (0-5).
     */
    internal uint getZoom() =>
        _zoom;

    /**
     * Resets the rotation speed and timer.
     */
    internal void rotateStop()
    {
        _rotLon = 0.0;
        _rotLat = 0.0;
        _rotTimer.stop();
    }

    /**
     * Resets longitude rotation speed and timer.
     */
    internal void rotateStopLon()
    {
        _rotLon = 0.0;
        if (AreSame(_rotLat, 0.0))
        {
            _rotTimer.stop();
        }
    }

    /**
     * Resets latitude rotation speed and timer.
     */
    internal void rotateStopLat()
    {
        _rotLat = 0.0;
        if (AreSame(_rotLon, 0.0))
        {
            _rotTimer.stop();
        }
    }

    /**
     * Checks if a polar point is inside the globe's landmass.
     * @param lon Longitude of the point.
     * @param lat Latitude of the point.
     * @return True if it's inside, False if it's outside.
     */
    internal bool insideLand(double lon, double lat) =>
        (getPolygonFromLonLat(lon, lat)) != null;

    /**
     * Sets a downwards rotation speed and starts the timer.
     */
    internal void rotateDown()
    {
        _rotLat = ROTATE_LATITUDE;
        if (!_rotTimer.isRunning()) _rotTimer.start();
    }

    /**
     * Sets a upwards rotation speed and starts the timer.
     */
    internal void rotateUp()
    {
        _rotLat = -ROTATE_LATITUDE;
        if (!_rotTimer.isRunning()) _rotTimer.start();
    }

    /**
     * Increases the zoom level on the globe.
     */
    internal void zoomIn()
    {
        if (_zoom < _zoomRadius.Count - 1)
        {
            setZoom(_zoom + 1);
        }
    }

    /**
     * Decreases the zoom level on the globe.
     */
    internal void zoomOut()
    {
        if (_zoom > 0)
        {
            setZoom(_zoom - 1);
        }
    }

    /**
     * Zooms the globe out as far as possible.
     */
    internal void zoomMin()
    {
        if (_zoom > 0)
        {
            setZoom(0);
        }
    }

    /**
     * Zooms the globe in as close as possible.
     */
    internal void zoomMax()
    {
        if (_zoom < _zoomRadius.Count - 1)
        {
            setZoom((uint)(_zoomRadius.Count - 1));
        }
    }

    /**
     * Sets a leftwards rotation speed and starts the timer.
     */
    internal void rotateLeft()
    {
        _rotLon = -ROTATE_LONGITUDE;
        if (!_rotTimer.isRunning()) _rotTimer.start();
    }

    /**
     * Sets a rightwards rotation speed and starts the timer.
     */
    internal void rotateRight()
    {
        _rotLon = ROTATE_LONGITUDE;
        if (!_rotTimer.isRunning()) _rotTimer.start();
    }

    /**
     * Stores the zoom used before a dogfight.
     */
    internal void saveZoomDogfight() =>
        _zoomOld = _zoom;

    /**
     * Zooms the globe smoothly into dogfight level.
     * @return Is the globe already zoomed in?
     */
    internal bool zoomDogfightIn()
    {
        if (_zoom < DOGFIGHT_ZOOM)
        {
            double radiusNow = _radius;
            if (radiusNow + _radiusStep >= _zoomRadius[(int)DOGFIGHT_ZOOM])
            {
                setZoom(DOGFIGHT_ZOOM);
            }
            else
            {
                if (radiusNow + _radiusStep >= _zoomRadius[(int)(_zoom + 1)])
                    _zoom++;
                setZoom(_zoom);
                _radius = radiusNow + _radiusStep;
            }
            return false;
        }
        return true;
    }

    /**
     * Zooms the globe smoothly out of dogfight level.
     * @return Is the globe already zoomed out?
     */
    internal bool zoomDogfightOut()
    {
        if (_zoom > _zoomOld)
        {
            double radiusNow = _radius;
            if (radiusNow - _radiusStep <= _zoomRadius[(int)_zoomOld])
            {
                setZoom(_zoomOld);
            }
            else
            {
                if (radiusNow - _radiusStep <= _zoomRadius[(int)(_zoom - 1)])
                    _zoom--;
                setZoom(_zoom);
                _radius = radiusNow - _radiusStep;
            }
            return false;
        }
        return true;
    }

    Polygon getPolygonFromLonLat(double lon, double lat)
    {
        const double zDiscard = 0.75f;
        double coslat = Math.Cos(lat);
        double sinlat = Math.Sin(lat);

        foreach (var i in _rules.getPolygons())
        {
            double x, y, z, x2, y2;
            double clat, clon;
            z = 0;
            for (int j = 0; j < i.getPoints(); ++j)
            {
                z = coslat * Math.Cos(i.getLatitude(j)) * Math.Cos(i.getLongitude(j) - lon) + sinlat * Math.Sin(i.getLatitude(j));
                if (z < zDiscard) break; //discarded
            }
            if (z < zDiscard) continue; //discarded

            bool odd = false;

            clat = i.getLatitude(0); //initial point
            clon = i.getLongitude(0);
            x = Math.Cos(clat) * Math.Sin(clon - lon);
            y = coslat * Math.Sin(clat) - sinlat * Math.Cos(clat) * Math.Cos(clon - lon);

            for (int j = 0; j < i.getPoints(); ++j)
            {
                int k = (j + 1) % i.getPoints(); //index of next point in poly
                clat = i.getLatitude(k);
                clon = i.getLongitude(k);

                x2 = Math.Cos(clat) * Math.Sin(clon - lon);
                y2 = coslat * Math.Sin(clat) - sinlat * Math.Cos(clat) * Math.Cos(clon - lon);
                if (((y > 0) != (y2 > 0)) && (0 < (x2 - x) * (0 - y) / (y2 - y) + x))
                    odd = !odd;
                x = x2;
                y = y2;

            }
            if (odd) return i;
        }
        return null;
    }

    /**
     * Get the polygons texture at a given point
     * @param lon Longitude of the point.
     * @param lat Latitude of the point.
     * @param texture pointer to texture ID returns -1 when polygon not found
     * @param shade pointer to shade
     */
    internal void getPolygonTextureAndShade(double lon, double lat, out int texture, out int shade)
    {
        ///this is shade conversion from 0..31 levels of geoscape to battlescape levels 0..15
        int[] worldshades = {0, 0, 0, 0, 1, 1, 2, 2,
                             3, 3, 4, 4, 5, 5, 6, 6,
                             7, 7, 8, 8, 9, 9,10,11,
                             11,12,12,13,13,14,15,15};

        shade = worldshades[CreateShadow.getShadowValue(new Cord(0.0, 0.0, 1.0), getSunDirection(lon, lat), 0)];
        Polygon t = getPolygonFromLonLat(lon, lat);
        texture = (t == null) ? -1 : t.getTexture();
    }

    /**
     * Rotates the globe to center on a certain
     * polar point on the world map.
     * @param lon Longitude of the point.
     * @param lat Latitude of the point.
     */
    internal void center(double lon, double lat)
    {
        _cenLon = lon;
        _cenLat = lat;
        _game.getSavedGame().setGlobeLongitude(_cenLon);
        _game.getSavedGame().setGlobeLatitude(_cenLat);
        invalidate();
    }

    /**
     * Get position of sun from point on globe
     * @param lon longitude of position
     * @param lat latitude of position
     * @return position of sun
     */
    Cord getSunDirection(double lon, double lat)
    {
        double curTime = _game.getSavedGame().getTime().getDaylight();
        double rot = curTime * 2 * M_PI;
        double sun;

        if (Options.globeSeasons)
        {
            int[] MonthDays1 = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 };
            int[] MonthDays2 = { 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };

            int year = _game.getSavedGame().getTime().getYear();
            int month = _game.getSavedGame().getTime().getMonth() - 1;
            int day = _game.getSavedGame().getTime().getDay() - 1;

            double tm = (double)((_game.getSavedGame().getTime().getHour() * 60
                + _game.getSavedGame().getTime().getMinute()) * 60
                + _game.getSavedGame().getTime().getSecond()) / 86400; //day fraction is also taken into account

            double CurDay;
            if (year % 4 == 0 && !(year % 100 == 0 && year % 400 != 0))
                CurDay = (MonthDays2[month] + day + tm) / 366 - 0.219; //spring equinox (start of astronomic year)
            else
                CurDay = (MonthDays1[month] + day + tm) / 365 - 0.219;
            if (CurDay < 0) CurDay += 1.0;

            sun = -0.261 * Math.Sin(CurDay * 2 * M_PI);
        }
        else
            sun = 0;

        var sun_direction = new Cord(Math.Cos(rot + lon), Math.Sin(rot + lon) * -Math.Sin(lat), Math.Sin(rot + lon) * Math.Cos(lat));

        var pole = new Cord(0, Math.Cos(lat), Math.Sin(lat));

        if (sun > 0)
            sun_direction *= 1.0 - sun;
        else
            sun_direction *= 1.0 + sun;

        pole *= sun;
        sun_direction += pole;
        double norm = sun_direction.norm();
        //norm should be always greater than 0
        norm = 1.0 / norm;
        sun_direction *= norm;
        return sun_direction;
    }

    internal void setCraftRange(double lon, double lat, double range)
    {
        _craft = (range > 0.0);
        _craftLon = lon;
        _craftLat = lat;
        _craftRange = range;
    }

    /**
     * Converts a cartesian point into a polar point for
     * mapping a globe click onto the flat world map.
     * @param x X position of the cartesian point.
     * @param y Y position of the cartesian point.
     * @param lon Pointer to the output longitude.
     * @param lat Pointer to the output latitude.
     */
    internal void cartToPolar(short x, short y, out double lon, out double lat)
    {
	    // Orthographic projection
	    x -= _cenX;
	    y -= _cenY;

	    double rho = Math.Sqrt((double)(x*x + y*y));
	    double c = Math.Asin(rho / _radius);
	    if ( AreSame(rho, 0.0) )
	    {
		    lat = _cenLat;
		    lon = _cenLon;

	    }
	    else
	    {
		    lat = Math.Asin((y * Math.Sin(c) * Math.Cos(_cenLat)) / rho + Math.Cos(c) * Math.Sin(_cenLat));
		    lon = Math.Atan2(x * Math.Sin(c),(rho * Math.Cos(_cenLat) * Math.Cos(c) - y * Math.Sin(_cenLat) * Math.Sin(c))) + _cenLon;
	    }

	    // Keep between 0 and 2xPI
	    while (lon < 0)
		    lon += 2 * M_PI;
	    while (lon >= 2 * M_PI)
		    lon -= 2 * M_PI;
    }

    /**
     * Returns a list of all the targets currently near a certain
     * cartesian point over the globe.
     * @param x X coordinate of point.
     * @param y Y coordinate of point.
     * @param craft Only get craft targets.
     * @return List of pointers to targets.
     */
    internal List<Target> getTargets(int x, int y, bool craft)
    {
	    var v = new List<Target>();
	    if (!craft)
	    {
		    foreach (var i in _game.getSavedGame().getBases())
		    {
			    if (i.getLongitude() == 0.0 && i.getLatitude() == 0.0)
				    continue;

			    if (targetNear(i, x, y))
			    {
				    v.Add(i);
			    }

			    foreach (var j in i.getCrafts())
			    {
				    if (j.getLongitude() == i.getLongitude() && j.getLatitude() == i.getLatitude() && j.getDestination() == null)
					    continue;

				    if (targetNear(j, x, y))
				    {
					    v.Add(j);
				    }
			    }
		    }
	    }
	    foreach (var i in _game.getSavedGame().getUfos())
	    {
		    if (!i.getDetected())
			    continue;

		    if (targetNear(i, x, y))
		    {
			    v.Add(i);
		    }
	    }
	    foreach (var i in _game.getSavedGame().getWaypoints())
	    {
		    if (targetNear(i, x, y))
		    {
			    v.Add(i);
		    }
	    }
	    foreach (var i in _game.getSavedGame().getMissionSites())
	    {
		    if (targetNear(i, x, y))
		    {
			    v.Add(i);
		    }
	    }
	    foreach (var i in _game.getSavedGame().getAlienBases())
	    {
		    if (!i.isDiscovered())
		    {
			    continue;
		    }
		    if (targetNear(i, x, y))
		    {
			    v.Add(i);
		    }
	    }
	    return v;
    }

    /**
     * Checks if a certain target is near a certain cartesian point
     * (within a circled area around it) over the globe.
     * @param target Pointer to target.
     * @param x X coordinate of point.
     * @param y Y coordinate of point.
     * @return True if it's near, false otherwise.
     */
    bool targetNear(Target target, int x, int y)
    {
	    short tx, ty;
	    if (pointBack(target.getLongitude(), target.getLatitude()))
		    return false;
	    polarToCart(target.getLongitude(), target.getLatitude(), out tx, out ty);

	    int dx = x - tx;
	    int dy = y - ty;
	    return (dx * dx + dy * dy <= NEAR_RADIUS);
    }

    internal void setNewBaseHoverPos(double lon, double lat)
    {
        _hoverLon = lon;
        _hoverLat = lat;
    }

    internal void setNewBaseHover(bool hover) =>
        _hover = hover;

    /**
     * Keeps the animation timers running.
     */
    protected override void think()
    {
	    _blinkTimer.think(null, this);
	    _rotTimer.think(null, this);
    }

    /*
     * Resizes the geoscape.
     */
    internal void resize()
    {
	    Surface[] surfaces = {this, _markers, _countries, _radars};
	    int width = Options.baseXGeoscape - 64;
	    int height = Options.baseYGeoscape;

	    for (int i = 0; i < 4; ++i)
	    {
		    surfaces[i].setWidth(width);
		    surfaces[i].setHeight(height);
		    surfaces[i].invalidate();
	    }
	    _clipper.Wxrig = width;
	    _clipper.Wybot = height;
	    _cenX = (short)(width / 2);
	    _cenY = (short)(height / 2);
	    setupRadii(width, height);
	    invalidate();
    }

    /**
     * Draws the whole globe, part by part.
     */
    protected override void draw()
    {
	    if (_redraw)
	    {
		    cachePolygons();
	    }
	    base.draw();
	    drawOcean();
	    drawLand();
	    drawRadars();
	    drawFlights();
	    drawShadow();
	    drawMarkers();
	    drawDetail();
    }

    /**
     * Renders the ocean, shading it according to the time of day.
     */
    void drawOcean()
    {
	    @lock();
	    drawCircle((short)(_cenX + 1), _cenY, (short)(_radius + 20), OCEAN_COLOR);
    //	ShaderDraw<Ocean>(ShaderSurface(this));
	    unlock();
    }

    /**
     * Renders the land, taking all the visible world polygons
     * and texturing and shading them accordingly.
     */
    void drawLand()
    {
	    short[] x = new short[4], y = new short[4];

	    foreach (var i in _cacheLand)
	    {
		    // Convert coordinates
		    for (int j = 0; j < i.getPoints(); ++j)
		    {
			    x[j] = i.getX(j);
			    y[j] = i.getY(j);
		    }

		    // Apply textures according to zoom and shade
		    drawTexturedPolygon(x, y, i.getPoints(), _texture.getFrame((int)(i.getTexture() + _zoomTexture)), 0, 0);
	    }
    }

    /**
     * Draws the radar ranges of player bases on the globe.
     */
    void drawRadars()
    {
	    _radars.clear();

	    // Draw craft circle instead of radar circles to avoid confusion
	    if (_craft)
	    {
		    _radars.@lock();

		    if (_craftRange < M_PI)
		    {
			    drawGlobeCircle(_craftLat, _craftLon, _craftRange, 64);
			    drawGlobeCircle(_craftLat, _craftLon, _craftRange - 0.025, 64, 2);
		    }

		    _radars.unlock();
		    return;
	    }

	    if (!Options.globeRadarLines)
		    return;

	    double tr, range;
	    double lat, lon;
	    var ranges = new List<double>();

	    _radars.@lock();

	    if (_hover)
	    {
		    List<string> facilities = _game.getMod().getBaseFacilitiesList();
		    foreach (var i in facilities)
		    {
			    range = Nautical(_game.getMod().getBaseFacility(i).getRadarRange());
			    drawGlobeCircle(_hoverLat,_hoverLon,range,48);
			    if (Options.globeAllRadarsOnBaseBuild) ranges.Add(range);
		    }
	    }

	    // Draw radars around bases
	    foreach (var i in _game.getSavedGame().getBases())
	    {
		    lat = i.getLatitude();
		    lon = i.getLongitude();
		    // Cheap hack to hide bases when they haven't been placed yet
		    if (( !(AreSame(lon, 0.0) && AreSame(lat, 0.0)) )/* &&
			    !pointBack(i.getLongitude(), i.getLatitude())*/)
		    {
			    if (_hover && Options.globeAllRadarsOnBaseBuild)
			    {
				    for (int j=0; j<ranges.Count; j++) drawGlobeCircle(lat,lon,ranges[j],48);
			    }
			    else
			    {
				    range = 0;
				    foreach (var j in i.getFacilities())
				    {
					    if (j.getBuildTime() == 0)
					    {
						    tr = j.getRules().getRadarRange();
						    if (tr > range) range = tr;
					    }
				    }
				    range = Nautical(range);

				    if (range>0) drawGlobeCircle(lat,lon,range,48);
			    }

		    }

		    foreach (var j in i.getCrafts())
		    {
			    if (j.getStatus() != "STR_OUT")
				    continue;
			    lat=j.getLatitude();
			    lon=j.getLongitude();
			    range = Nautical(j.getRules().getRadarRange());

			    if (range>0) drawGlobeCircle(lat,lon,range,24);
		    }
	    }

	    _radars.unlock();
    }

    /**
     *	Draw globe range circle
     */
    void drawGlobeCircle(double lat, double lon, double radius, int segments, int frac = 1)
    {
	    double x, y, x2 = 0, y2 = 0;
	    double lat1, lon1;
	    double seg = M_PI / ((double)segments / 2);
	    int i = 0;
	    for (double az = 0; az <= M_PI*2+0.01; az+=seg) //48 circle segments
	    {
		    //calculating sphere-projected circle
		    lat1 = Math.Asin(Math.Sin(lat) * Math.Cos(radius) + Math.Cos(lat) * Math.Sin(radius) * Math.Cos(az));
		    lon1 = lon + Math.Atan2(Math.Sin(az) * Math.Sin(radius) * Math.Cos(lat), Math.Cos(radius) - Math.Sin(lat) * Math.Sin(lat1));
		    polarToCart(lon1, lat1, out x, out y);
		    if ( AreSame(az, 0.0) ) //first vertex is for initialization only
		    {
			    x2=x;
			    y2=y;
			    continue;
		    }
		    if (!pointBack(lon1,lat1) && i % frac == 0)
			    XuLine(_radars, this, x, y, x2, y2, 6);
		    x2=x; y2=y;
		    i++;
	    }
    }

    void polarToCart(double lon, double lat, out double x, out double y)
    {
	    // Orthographic projection
	    x = _cenX + _radius * Math.Cos(lat) * Math.Sin(lon - _cenLon);
	    y = _cenY + _radius * (Math.Cos(_cenLat) * Math.Sin(lat) - Math.Sin(_cenLat) * Math.Cos(lat) * Math.Cos(lon - _cenLon));
    }

    void XuLine(Surface surface, Surface src, double x1, double y1, double x2, double y2, int shade)
    {
	    if (_clipper.LineClip(ref x1,ref y1,ref x2,ref y2) != 1) return; //empty line

	    double deltax = x2-x1, deltay = y2-y1;
	    bool inv;
	    short tcol;
	    double len,x0,y0,SX,SY;
	    if (Math.Abs((int)y2-(int)y1) > Math.Abs((int)x2-(int)x1))
	    {
		    len=Math.Abs((int)y2-(int)y1);
		    inv=false;
	    }
	    else
	    {
		    len=Math.Abs((int)x2-(int)x1);
		    inv=true;
	    }

	    if (y2<y1) {
	    SY=-1;
      } else if ( AreSame(deltay, 0.0) ) {
	    SY=0;
      } else {
	    SY=1;
      }

	    if (x2<x1) {
	    SX=-1;
      } else if ( AreSame(deltax, 0.0) ) {
	    SX=0;
      } else {
	    SX=1;
      }

	    x0=x1;  y0=y1;
	    if (inv)
		    SY=(deltay/len);
	    else
		    SX=(deltax/len);

	    while (len>0)
	    {
		    tcol=src.getPixel((int)x0,(int)y0);
		    if (tcol != 0)
		    {
			    if (CreateShadow.isOcean((byte)tcol))
			    {
				    tcol = CreateShadow.getOceanShadow((byte)(shade + 8));
			    }
			    else
			    {
				    tcol = CreateShadow.getLandShadow((byte)tcol, (byte)(shade * 3));
			    }
			    surface.setPixel((int)x0, (int)y0, (byte)tcol);
		    }
		    x0+=SX;
		    y0+=SY;
		    len-=1.0;
	    }
    }

    /**
     * Draws the flight paths of player craft flying on the globe.
     */
    void drawFlights()
    {
	    //_radars.clear();

	    if (!Options.globeFlightPaths)
		    return;

	    // Lock the surface
	    _radars.@lock();

	    // Draw the craft flight paths
	    foreach (var i in _game.getSavedGame().getBases())
	    {
		    foreach (var j in i.getCrafts())
		    {
			    // Hide crafts docked at base
			    if (j.getStatus() != "STR_OUT" || j.getDestination() == null /*|| pointBack(j.getLongitude(), j.getLatitude())*/)
				    continue;

			    double lon1 = j.getLongitude();
			    double lat1 = j.getLatitude();
			    double lon2 = j.getDestination().getLongitude();
			    double lat2 = j.getDestination().getLatitude();

			    if (j.isMeetCalculated())
			    {
				    lon2 = j.getMeetLongitude();
				    lat2 = j.getMeetLatitude();
			    }
			    drawPath(_radars, lon1, lat1, lon2, lat2);

			    if (j.isMeetCalculated())
			    {
				    lon1 = j.getDestination().getLongitude();
				    lat1 = j.getDestination().getLatitude();

				    drawPath(_radars, lon1, lat1, lon2, lat2);
			    }
		    }
	    }

	    // Unlock the surface
	    _radars.unlock();
    }

    void drawPath(Surface surface, double lon1, double lat1, double lon2, double lat2)
    {
	    double length;
	    short count;
	    double x1, y1, x2, y2;
	    CordPolar p1, p2;
	    Cord a = (Cord)new CordPolar(lon1, lat1);
	    Cord b = (Cord)new CordPolar(lon2, lat2);

	    if (-b == a)
		    return;

	    b -= a;

	    //longer path have more parts
	    length = b.norm();
	    length *= length*15;
	    count = (short)(length + 1);
	    b /= count;
	    p1 = (CordPolar)a;
	    polarToCart(p1.lon, p1.lat, out x1, out y1);
	    for (int i = 0; i < count; ++i)
	    {
		    a += b;
		    p2 = (CordPolar)a;
		    polarToCart(p2.lon, p2.lat, out x2, out y2);

		    if (!pointBack(p1.lon, p1.lat) && !pointBack(p2.lon, p2.lat))
		    {
			    XuLine(surface, this, x1, y1, x2, y2, 8);
		    }

		    p1 = p2;
		    x1 = x2;
		    y1 = y2;
	    }
    }

    void drawShadow()
    {
	    ShaderMove<Cord> earth = new ShaderMove<Cord>(_earthData[(int)_zoom], getWidth(), getHeight());
	    ShaderRepeat<short> noise = new ShaderRepeat<short>(_randomNoiseData, GlobeStaticData.random_surf_size, GlobeStaticData.random_surf_size);

	    earth.setMove(_cenX-getWidth()/2, _cenY-getHeight()/2);

	    @lock();
	    ShaderDraw(new CreateShadow(), ShaderSurface(this), earth, ShaderScalar(getSunDirection(_cenLon, _cenLat)), noise);
	    unlock();
    }

	static int debugType = 0;
	static bool canSwitchDebugType = false;
    /**
     * Draws the details of the countries on the globe,
     * based on the current zoom level.
     */
    void drawDetail()
    {
	    _countries.clear();

	    if (!Options.globeDetail)
		    return;

	    // Draw the country borders
	    if (_zoom >= 1)
	    {
		    // Lock the surface
		    _countries.@lock();

		    foreach (var i in _rules.getPolylines())
		    {
			    short[] x = [2], y = [2];
			    for (int j = 0; j < i.getPoints() - 1; ++j)
			    {
				    // Don't draw if polyline is facing back
				    if (pointBack(i.getLongitude(j), i.getLatitude(j)) || pointBack(i.getLongitude(j + 1), i.getLatitude(j + 1)))
					    continue;

				    // Convert coordinates
				    polarToCart(i.getLongitude(j), i.getLatitude(j), out x[0], out y[0]);
				    polarToCart(i.getLongitude(j + 1), i.getLatitude(j + 1), out x[1], out y[1]);

				    _countries.drawLine(x[0], y[0], x[1], y[1], LINE_COLOR);
			    }
		    }

		    // Unlock the surface
		    _countries.unlock();
	    }

	    // Draw the country names
	    if (_zoom >= 2)
	    {
		    Text label = new Text(100, 9, 0, 0);
		    label.setPalette(getPaletteColors());
		    label.initText(_game.getMod().getFont("FONT_BIG"), _game.getMod().getFont("FONT_SMALL"), _game.getLanguage());
		    label.setAlign(TextHAlign.ALIGN_CENTER);
		    label.setColor(COUNTRY_LABEL_COLOR);

		    short x, y;
		    foreach (var i in _game.getSavedGame().getCountries())
		    {
			    // Don't draw if label is facing back
			    if (pointBack(i.getRules().getLabelLongitude(), i.getRules().getLabelLatitude()))
				    continue;

			    // Convert coordinates
			    polarToCart(i.getRules().getLabelLongitude(), i.getRules().getLabelLatitude(), out x, out y);

			    label.setX(x - 50);
			    label.setY(y);
			    label.setText(_game.getLanguage().getString(i.getRules().getType()));
			    label.blit(_countries);
		    }

		    label = null;
	    }

	    // Draw the city and base markers
	    if (_zoom >= 3)
	    {
		    Text label = new Text(100, 9, 0, 0);
		    label.setPalette(getPaletteColors());
		    label.initText(_game.getMod().getFont("FONT_BIG"), _game.getMod().getFont("FONT_SMALL"), _game.getLanguage());
		    label.setAlign(TextHAlign.ALIGN_CENTER);
		    label.setColor(CITY_LABEL_COLOR);

		    short x, y;
		    foreach (var i in _game.getSavedGame().getRegions())
		    {
			    foreach (var j in i.getRules().getCities())
			    {
				    drawTarget(j, _countries);

				    // Don't draw if city is facing back
				    if (pointBack(j.getLongitude(), j.getLatitude()))
					    continue;

				    // Convert coordinates
				    polarToCart(j.getLongitude(), j.getLatitude(), out x, out y);

				    label.setX(x - 50);
				    label.setY(y + 2);
				    label.setText(j.getName(_game.getLanguage()));
				    label.blit(_countries);
			    }
		    }
		    // Draw bases names
		    foreach (var j in _game.getSavedGame().getBases())
		    {
			    if (j.getMarker() == -1 || pointBack(j.getLongitude(), j.getLatitude()))
				    continue;
			    polarToCart(j.getLongitude(), j.getLatitude(), out x, out y);
			    label.setX(x - 50);
			    label.setY(y + 2);
			    label.setColor(BASE_LABEL_COLOR);
			    label.setText(j.getName());
			    label.blit(_countries);
		    }

		    label = null;
	    }

	    if (_game.getSavedGame().getDebugMode())
	    {
		    int color;
		    canSwitchDebugType = true;
		    if (debugType == 0)
		    {
			    color = 0;
			    foreach (var i in _game.getSavedGame().getCountries())
			    {
				    color += 10;
				    for (int k = 0; k != i.getRules().getLatMax().Count; ++k)
				    {
					    double lon2 = i.getRules().getLonMax()[k];
					    double lon1 = i.getRules().getLonMin()[k];
					    double lat2 = i.getRules().getLatMax()[k];
					    double lat1 = i.getRules().getLatMin()[k];

					    drawVHLine(_countries, lon1, lat1, lon2, lat1, (byte)color);
					    drawVHLine(_countries, lon1, lat2, lon2, lat2, (byte)color);
					    drawVHLine(_countries, lon1, lat1, lon1, lat2, (byte)color);
					    drawVHLine(_countries, lon2, lat1, lon2, lat2, (byte)color);
				    }
			    }
		    }
		    else if (debugType == 1)
		    {
			    color = 0;
			    foreach (var i in _game.getSavedGame().getRegions())
			    {
				    color += 10;
				    for (int k = 0; k != i.getRules().getLatMax().Count; ++k)
				    {
					    double lon2 = i.getRules().getLonMax()[k];
					    double lon1 = i.getRules().getLonMin()[k];
					    double lat2 = i.getRules().getLatMax()[k];
					    double lat1 = i.getRules().getLatMin()[k];

					    drawVHLine(_countries, lon1, lat1, lon2, lat1, (byte)color);
					    drawVHLine(_countries, lon1, lat2, lon2, lat2, (byte)color);
					    drawVHLine(_countries, lon1, lat1, lon1, lat2, (byte)color);
					    drawVHLine(_countries, lon2, lat1, lon2, lat2, (byte)color);
				    }
			    }
		    }
		    else if (debugType == 2)
		    {
			    foreach (var i in _game.getSavedGame().getRegions())
			    {
				    color = -1;
				    foreach (var j in i.getRules().getMissionZones())
				    {
					    color += 2;
					    foreach (var k in j.areas)
					    {
						    double lon2 = k.lonMax;
						    double lon1 = k.lonMin;
						    double lat2 = k.latMax;
						    double lat1 = k.latMin;

						    drawVHLine(_countries, lon1, lat1, lon2, lat1, (byte)color);
						    drawVHLine(_countries, lon1, lat2, lon2, lat2, (byte)color);
						    drawVHLine(_countries, lon1, lat1, lon1, lat2, (byte)color);
						    drawVHLine(_countries, lon2, lat1, lon2, lat2, (byte)color);
					    }
				    }
			    }
		    }
	    }
	    else
	    {
		    if (canSwitchDebugType)
		    {
			    ++debugType;
			    if (debugType > 2) debugType = 0;
			    canSwitchDebugType = false;
		    }
	    }
    }

    void drawVHLine(Surface surface, double lon1, double lat1, double lon2, double lat2, byte color)
    {
	    double sx = lon2 - lon1;
	    double sy = lat2 - lat1;
	    double ln1, lt1, ln2, lt2;
	    int seg;
	    short x1, y1, x2, y2;

	    if (sx<0) sx += 2*M_PI;

	    if (Math.Abs(sx)<0.01)
	    {
		    seg = (int)Math.Abs(sy/(2*M_PI)*48);
		    if (seg == 0) ++seg;
	    }
	    else
	    {
		    seg = (int)Math.Abs(sx/(2*M_PI)*96);
		    if (seg == 0) ++seg;
	    }

	    sx /= seg;
	    sy /= seg;

	    for (int i = 0; i < seg; ++i)
	    {
		    ln1 = lon1 + sx*i;
		    lt1 = lat1 + sy*i;
		    ln2 = lon1 + sx*(i+1);
		    lt2 = lat1 + sy*(i+1);

		    if (!pointBack(ln2, lt2)&&!pointBack(ln1, lt1))
		    {
			    polarToCart(ln1,lt1,out x1,out y1);
			    polarToCart(ln2,lt2,out x2,out y2);
			    surface.drawLine(x1, y1, x2, y2, color);
		    }
	    }
    }

    /**
     * Blits the globe onto another surface.
     * @param surface Pointer to another surface.
     */
    protected override void blit(Surface surface)
    {
	    base.blit(surface);
	    _radars.blit(surface);
	    _countries.blit(surface);
	    _markers.blit(surface);
    }

    /**
     * Ignores any mouse clicks that are outside the globe.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void mousePress(Action action, State state)
    {
	    double lon, lat;
	    cartToPolar((short)Math.Floor(action.getAbsoluteXMouse()), (short)Math.Floor(action.getAbsoluteYMouse()), out lon, out lat);

	    if (action.getDetails().button.button == Options.geoDragScrollButton)
	    {
		    _isMouseScrolling = true;
		    _isMouseScrolled = false;
		    SDL_GetMouseState(out _xBeforeMouseScrolling, out _yBeforeMouseScrolling);
		    _lonBeforeMouseScrolling = _cenLon;
		    _latBeforeMouseScrolling = _cenLat;
		    _totalMouseMoveX = 0; _totalMouseMoveY = 0;
		    _mouseMovedOverThreshold = false;
		    _mouseScrollingStartTime = SDL_GetTicks();
	    }
	    // Check for errors
	    //if (lat == lat && lon == lon)
	    //{
		    base.mousePress(action, state);
	    //}
    }

    /**
     * Ignores any mouse clicks that are outside the globe.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void mouseRelease(Action action, State state)
    {
	    double lon, lat;
	    cartToPolar((short)Math.Floor(action.getAbsoluteXMouse()), (short)Math.Floor(action.getAbsoluteYMouse()), out lon, out lat);
	    if (action.getDetails().button.button == Options.geoDragScrollButton)
	    {
		    stopScrolling(action);
	    }
	    // Check for errors
	    //if (lat == lat && lon == lon)
	    //{
		    base.mouseRelease(action, state);
	    //}
    }

    /**
     * Move the mouse back to where it started after we finish drag scrolling.
     * @param action Pointer to an action.
     */
    void stopScrolling(Action action)
    {
	    SDL_WarpMouseGlobal(_xBeforeMouseScrolling, _yBeforeMouseScrolling);
	    action.setMouseAction(_xBeforeMouseScrolling, _yBeforeMouseScrolling, getX(), getY());
    }

    /**
     * Ignores any mouse clicks that are outside the globe
     * and handles globe rotation and zooming.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void mouseClick(Action action, State state)
    {
        if (action.getDetails().wheel.y > 0) //button.button == SDL_BUTTON_WHEELUP
	    {
		    zoomIn();
	    }
        else if (action.getDetails().wheel.y < 0) //button.button == SDL_BUTTON_WHEELDOWN
	    {
		    zoomOut();
	    }

	    double lon, lat;
	    cartToPolar((short)Math.Floor(action.getAbsoluteXMouse()), (short)Math.Floor(action.getAbsoluteYMouse()), out lon, out lat);

	    // The following is the workaround for a rare problem where sometimes
	    // the mouse-release event is missed for any reason.
	    // However if the SDL is also missed the release event, then it is to no avail :(
	    // (this part handles the release if it is missed and now an other button is used)
	    if (_isMouseScrolling)
	    {
		    if (action.getDetails().button.button != Options.geoDragScrollButton
			    && 0 == (SDL_GetMouseState(0, 0)&SDL_BUTTON((uint)Options.geoDragScrollButton)))
		    { // so we missed again the mouse-release :(
			    // Check if we have to revoke the scrolling, because it was too short in time, so it was a click
			    if ((!_mouseMovedOverThreshold) && ((int)(SDL_GetTicks() - _mouseScrollingStartTime) <= (Options.dragScrollTimeTolerance)))
			    {
				    center(_lonBeforeMouseScrolling, _latBeforeMouseScrolling);
			    }
			    _isMouseScrolled = _isMouseScrolling = false;
			    stopScrolling(action);
		    }
	    }

	    // DragScroll-Button release: release mouse-scroll-mode
	    if (_isMouseScrolling)
	    {
		    // While scrolling, other buttons are ineffective
		    if (action.getDetails().button.button == Options.geoDragScrollButton)
		    {
			    _isMouseScrolling = false;
			    stopScrolling(action);
		    }
		    else
		    {
			    return;
		    }
		    // Check if we have to revoke the scrolling, because it was too short in time, so it was a click
		    if ((!_mouseMovedOverThreshold) && ((int)(SDL_GetTicks() - _mouseScrollingStartTime) <= (Options.dragScrollTimeTolerance)))
		    {
			    _isMouseScrolled = false;
			    stopScrolling(action);
			    center(_lonBeforeMouseScrolling, _latBeforeMouseScrolling);
		    }
		    if (_isMouseScrolled) return;
	    }

	    // Check for errors
	    //if (lat == lat && lon == lon)
	    //{
		    base.mouseClick(action, state);
		    if (action.getDetails().button.button == SDL_BUTTON_RIGHT)
		    {
			    center(lon, lat);
		    }
	    //}
    }

    /**
     * Ignores any mouse hovers that are outside the globe.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void mouseOver(Action action, State state)
    {
	    double lon, lat;
	    cartToPolar((short)Math.Floor(action.getAbsoluteXMouse()), (short)Math.Floor(action.getAbsoluteYMouse()), out lon, out lat);

	    if (_isMouseScrolling && action.getDetails().type == SDL_EventType.SDL_MOUSEMOTION)
	    {
		    // The following is the workaround for a rare problem where sometimes
		    // the mouse-release event is missed for any reason.
		    // (checking: is the dragScroll-mouse-button still pressed?)
		    // However if the SDL is also missed the release event, then it is to no avail :(
		    if (0 == (SDL_GetMouseState(0, 0)&SDL_BUTTON((uint)Options.geoDragScrollButton)))
		    { // so we missed again the mouse-release :(
			    // Check if we have to revoke the scrolling, because it was too short in time, so it was a click
			    if ((!_mouseMovedOverThreshold) && ((int)(SDL_GetTicks() - _mouseScrollingStartTime) <= (Options.dragScrollTimeTolerance)))
			    {
				    center(_lonBeforeMouseScrolling, _latBeforeMouseScrolling);
			    }
			    _isMouseScrolled = _isMouseScrolling = false;
			    stopScrolling(action);
			    return;
		    }

		    _isMouseScrolled = true;

		    if (Options.touchEnabled == false)
		    {
			    // Set the mouse cursor back
			    SDL_EventState(SDL_EventType.SDL_MOUSEMOTION, SDL_IGNORE);
			    SDL_WarpMouseGlobal((_game.getScreen().getWidth() - 100) / 2 , _game.getScreen().getHeight() / 2);
			    SDL_EventState(SDL_EventType.SDL_MOUSEMOTION, SDL_ENABLE);
		    }

		    // Check the threshold
		    _totalMouseMoveX += action.getDetails().motion.xrel;
		    _totalMouseMoveY += action.getDetails().motion.yrel;

		    if (!_mouseMovedOverThreshold)
			    _mouseMovedOverThreshold = ((Math.Abs(_totalMouseMoveX) > Options.dragScrollPixelTolerance) || (Math.Abs(_totalMouseMoveY) > Options.dragScrollPixelTolerance));

		    // Scrolling
		    if (Options.geoDragScrollInvert)
		    {
			    double newLon = ((double)_totalMouseMoveX / action.getXScale()) * ROTATE_LONGITUDE/(_zoom+1)/2;
			    double newLat = ((double)_totalMouseMoveY / action.getYScale()) * ROTATE_LATITUDE/(_zoom+1)/2;
			    center(_lonBeforeMouseScrolling + newLon / (Options.geoScrollSpeed / 10), _latBeforeMouseScrolling + newLat / (Options.geoScrollSpeed / 10));
		    }
		    else
		    {
			    double newLon = -action.getDetails().motion.xrel * ROTATE_LONGITUDE/(_zoom+1)/2;
			    double newLat = -action.getDetails().motion.yrel * ROTATE_LATITUDE/(_zoom+1)/2;
			    center(_cenLon + newLon / (Options.geoScrollSpeed / 10), _cenLat + newLat / (Options.geoScrollSpeed / 10));
		    }

		    if (Options.touchEnabled == false)
		    {
			    // We don't want to see the mouse-cursor jumping :)
			    action.setMouseAction(_xBeforeMouseScrolling, _yBeforeMouseScrolling, getX(), getY());
			    action.getDetails().motion.x = _xBeforeMouseScrolling; action.getDetails().motion.y = _yBeforeMouseScrolling;
		    }

		    _game.getCursor().handle(action);
	    }

	    if (Options.touchEnabled == false &&
		    _isMouseScrolling &&
		    (action.getDetails().motion.x != _xBeforeMouseScrolling ||
		    action.getDetails().motion.y != _yBeforeMouseScrolling))
	    {
		    action.setMouseAction(_xBeforeMouseScrolling, _yBeforeMouseScrolling, getX(), getY());
		    action.getDetails().motion.x = _xBeforeMouseScrolling; action.getDetails().motion.y = _yBeforeMouseScrolling;
	    }
	    // Check for errors
	    //if (lat == lat && lon == lon)
	    //{
		    base.mouseOver(action, state);
	    //}
    }

    /**
     * Handles globe keyboard shortcuts.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void keyboardPress(Action action, State state)
    {
	    base.keyboardPress(action, state);
	    if (action.getDetails().key.keysym.sym == Options.keyGeoToggleDetail)
	    {
		    toggleDetail();
	    }
	    if (action.getDetails().key.keysym.sym == Options.keyGeoToggleRadar)
	    {
		    toggleRadarLines();
	    }
    }

    /**
     * Switches the amount of detail shown on the globe.
     * With detail on, country and city details are shown when zoomed in.
     */
    void toggleDetail()
    {
	    Options.globeDetail = !Options.globeDetail;
	    drawDetail();
    }

    /*
     * Turns Radar lines on or off.
     */
    void toggleRadarLines()
    {
	    Options.globeRadarLines = !Options.globeRadarLines;
	    drawRadars();
    }
}
