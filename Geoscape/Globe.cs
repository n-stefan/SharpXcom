﻿/*
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

struct CreateShadow
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
}
