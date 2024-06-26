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

enum ColorNames { CRAFT_MIN, CRAFT_MAX, RADAR_MIN, RADAR_MAX, DAMAGE_MIN, DAMAGE_MAX, BLOB_MIN, RANGE_METER, DISABLED_WEAPON, DISABLED_AMMO, DISABLED_RANGE };

/**
 * Shows a dogfight (interception) between a
 * player craft and an UFO.
 */
internal class DogfightState : State
{
    const int STANDOFF_DIST = 560;

    GeoscapeState _state;
    Craft _craft;
    Ufo _ufo;
    int _timeout, _currentDist, _targetDist, _w1FireInterval, _w2FireInterval, _w1FireCountdown, _w2FireCountdown;
    bool _end, _destroyUfo, _destroyCraft, _ufoBreakingOff, _weapon1Enabled, _weapon2Enabled;
    bool _minimized, _endDogfight, _animatingHit, _waitForPoly, _waitForAltitude;
    int _ufoSize, _craftHeight, _currentCraftDamageColor, _interceptionNumber;
    uint _interceptionsCount;
    int _x, _y, _minimizedIconX, _minimizedIconY;
    Surface _window, _battle, _range1, _range2, _damage;
    InteractiveSurface _btnMinimize, _preview, _weapon1, _weapon2;
    ImageButton _btnStandoff, _btnCautious, _btnStandard, _btnAggressive, _btnDisengage, _btnUfo;
    Text _txtAmmo1, _txtAmmo2, _txtDistance, _txtStatus, _txtInterceptionNumber;
    InteractiveSurface _btnMinimizedIcon;
    ImageButton _mode;
    Timer _craftDamageAnimTimer;
    int[] _colors = new int[11];
    List<CraftWeaponProjectile> _projectiles;

    // UFO blobs graphics ...
    static int[,,] _ufoBlobs = //[8][13][13]
    {
		    /*0 STR_VERY_SMALL */
	    {
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 1, 2, 3, 2, 1, 0, 0, 0, 0},
            {0, 0, 0, 0, 1, 3, 5, 3, 1, 0, 0, 0, 0},
            {0, 0, 0, 0, 1, 2, 3, 2, 1, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        },
		    /*1 STR_SMALL */
	    {
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 1, 2, 2, 2, 1, 0, 0, 0, 0},
            {0, 0, 0, 1, 2, 3, 4, 3, 2, 1, 0, 0, 0},
            {0, 0, 0, 1, 2, 4, 5, 4, 2, 1, 0, 0, 0},
            {0, 0, 0, 1, 2, 3, 4, 3, 2, 1, 0, 0, 0},
            {0, 0, 0, 0, 1, 2, 2, 2, 1, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        },
		    /*2 STR_MEDIUM_UC */
	    {
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0},
            {0, 0, 0, 1, 1, 2, 2, 2, 1, 1, 0, 0, 0},
            {0, 0, 0, 1, 2, 3, 3, 3, 2, 1, 0, 0, 0},
            {0, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 0},
            {0, 0, 1, 2, 3, 5, 5, 5, 3, 2, 1, 0, 0},
            {0, 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 0},
            {0, 0, 0, 1, 2, 3, 3, 3, 2, 1, 0, 0, 0},
            {0, 0, 0, 1, 1, 2, 2, 2, 1, 1, 0, 0, 0},
            {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        },
		    /*3 STR_LARGE */
	    {
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0},
            {0, 0, 0, 1, 1, 2, 2, 2, 1, 1, 0, 0, 0},
            {0, 0, 1, 2, 2, 3, 3, 3, 2, 2, 1, 0, 0},
            {0, 0, 1, 2, 3, 4, 4, 4, 3, 2, 1, 0, 0},
            {0, 1, 2, 3, 4, 5, 5, 5, 4, 3, 2, 1, 0},
            {0, 1, 2, 3, 4, 5, 5, 5, 4, 3, 2, 1, 0},
            {0, 1, 2, 3, 4, 5, 5, 5, 4, 3, 2, 1, 0},
            {0, 0, 1, 2, 3, 4, 4, 4, 3, 2, 1, 0, 0},
            {0, 0, 1, 2, 2, 3, 3, 3, 2, 2, 1, 0, 0},
            {0, 0, 0, 1, 1, 2, 2, 2, 1, 1, 0, 0, 0},
            {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        },
		    /*4 STR_VERY_LARGE */
	    {
            {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0},
            {0, 0, 0, 1, 1, 2, 2, 2, 1, 1, 0, 0, 0},
            {0, 0, 1, 2, 2, 3, 3, 3, 2, 2, 1, 0, 0},
            {0, 1, 2, 3, 3, 4, 4, 4, 3, 3, 2, 1, 0},
            {0, 1, 2, 3, 4, 5, 5, 5, 4, 3, 2, 1, 0},
            {1, 2, 3, 4, 5, 5, 5, 5, 5, 4, 3, 2, 1},
            {1, 2, 3, 4, 5, 5, 5, 5, 5, 4, 3, 2, 1},
            {1, 2, 3, 4, 5, 5, 5, 5, 5, 4, 3, 2, 1},
            {0, 1, 2, 3, 4, 5, 5, 5, 4, 3, 2, 1, 0},
            {0, 1, 2, 3, 3, 4, 4, 4, 3, 3, 2, 1, 0},
            {0, 0, 1, 2, 2, 3, 3, 3, 2, 2, 1, 0, 0},
            {0, 0, 0, 1, 1, 2, 2, 2, 1, 1, 0, 0, 0},
            {0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0}
        },
		    /*5 STR_HUGE */
	    {
            {0, 0, 0, 1, 1, 2, 2, 2, 1, 1, 0, 0, 0},
            {0, 0, 1, 2, 2, 3, 3, 3, 2, 2, 1, 0, 0},
            {0, 1, 2, 3, 3, 4, 4, 4, 3, 3, 2, 1, 0},
            {1, 2, 3, 4, 4, 5, 5, 5, 4, 4, 3, 2, 1},
            {1, 2, 3, 4, 5, 5, 5, 5, 5, 4, 3, 2, 1},
            {2, 3, 4, 5, 5, 5, 5, 5, 5, 5, 4, 3, 2},
            {2, 3, 4, 5, 5, 5, 5, 5, 5, 5, 4, 3, 2},
            {2, 3, 4, 5, 5, 5, 5, 5, 5, 5, 4, 3, 2},
            {1, 2, 3, 4, 5, 5, 5, 5, 5, 4, 3, 2, 1},
            {1, 2, 3, 4, 4, 5, 5, 5, 4, 4, 3, 2, 1},
            {0, 1, 2, 3, 3, 4, 4, 4, 3, 3, 2, 1, 0},
            {0, 0, 1, 2, 2, 3, 3, 3, 2, 2, 1, 0, 0},
            {0, 0, 0, 1, 1, 2, 2, 2, 1, 1, 0, 0, 0}
        },
		    /*6 STR_VERY_HUGE :p */
	    {
            {0, 0, 0, 2, 2, 3, 3, 3, 2, 2, 0, 0, 0},
            {0, 0, 2, 3, 3, 4, 4, 4, 3, 3, 2, 0, 0},
            {0, 2, 3, 4, 4, 5, 5, 5, 4, 4, 3, 2, 0},
            {2, 3, 4, 5, 5, 5, 5, 5, 5, 5, 4, 3, 2},
            {2, 3, 4, 5, 5, 5, 5, 5, 5, 5, 4, 3, 2},
            {3, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4, 3},
            {3, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4, 3},
            {3, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4, 3},
            {2, 3, 4, 5, 5, 5, 5, 5, 5, 5, 4, 3, 2},
            {2, 3, 4, 5, 5, 5, 5, 5, 5, 5, 4, 3, 2},
            {0, 2, 3, 4, 4, 5, 5, 5, 4, 4, 3, 2, 0},
            {0, 0, 2, 3, 3, 4, 4, 4, 3, 3, 2, 0, 0},
            {0, 0, 0, 2, 2, 3, 3, 3, 2, 2, 0, 0, 0}
        },
		    /*7 STR_ENOURMOUS */
	    {
            {0, 0, 0, 3, 3, 4, 4, 4, 3, 3, 0, 0, 0},
            {0, 0, 3, 4, 4, 5, 5, 5, 4, 4, 3, 0, 0},
            {0, 3, 4, 5, 5, 5, 5, 5, 5, 5, 4, 3, 0},
            {3, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4, 3},
            {3, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4, 3},
            {4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4},
            {4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4},
            {4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4},
            {3, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4, 3},
            {3, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4, 3},
            {0, 3, 4, 5, 5, 5, 5, 5, 5, 5, 4, 3, 0},
            {0, 0, 3, 4, 4, 5, 5, 5, 4, 4, 3, 0, 0},
            {0, 0, 0, 3, 3, 4, 4, 4, 3, 3, 0, 0, 0}
        }
    };

    // Projectile blobs
    static int[,,] _projectileBlobs = //[4][6][3]
    {
		    /*0 STR_STINGRAY_MISSILE ?*/
	    {
            {0, 1, 0},
            {1, 9, 1},
            {1, 4, 1},
            {0, 3, 0},
            {0, 2, 0},
            {0, 1, 0}
        },
		    /*1 STR_AVALANCHE_MISSILE ?*/
	    {
            {1, 2, 1},
            {2, 9, 2},
            {2, 5, 2},
            {1, 3, 1},
            {0, 2, 0},
            {0, 1, 0}
        },
		    /*2 STR_CANNON_ROUND ?*/
	    {
            {0, 0, 0},
            {0, 7, 0},
            {0, 2, 0},
            {0, 1, 0},
            {0, 0, 0},
            {0, 0, 0}
        },
		    /*3 STR_FUSION_BALL ?*/
	    {
            {2, 4, 2},
            {4, 9, 4},
            {2, 4, 2},
            {0, 0, 0},
            {0, 0, 0},
            {0, 0, 0}
        }
    };

    /**
     * Initializes all the elements in the Dogfight window.
     * @param game Pointer to the core game.
     * @param state Pointer to the Geoscape.
     * @param craft Pointer to the craft intercepting.
     * @param ufo Pointer to the UFO being intercepted.
     */
    internal DogfightState(GeoscapeState state, Craft craft, Ufo ufo)
    {
        _state = state;
        _craft = craft;
        _ufo = ufo;
        _timeout = 50;
        _currentDist = 640;
        _targetDist = 560;
        _w1FireCountdown = 0;
        _w2FireCountdown = 0;
        _end = false;
        _destroyUfo = false;
        _destroyCraft = false;
        _ufoBreakingOff = false;
        _weapon1Enabled = true;
        _weapon2Enabled = true;
        _minimized = false;
        _endDogfight = false;
        _animatingHit = false;
        _waitForPoly = false;
        _waitForAltitude = false;
        _ufoSize = 0;
        _craftHeight = 0;
        _currentCraftDamageColor = 0;
        _interceptionNumber = 0;
        _interceptionsCount = 0;
        _x = 0;
        _y = 0;
        _minimizedIconX = 0;
        _minimizedIconY = 0;

        _screen = false;

        _craft.setInDogfight(true);

        // Create objects
        _window = new Surface(160, 96, _x, _y);
        _battle = new Surface(77, 74, _x + 3, _y + 3);
        _weapon1 = new InteractiveSurface(15, 17, _x + 4, _y + 52);
        _range1 = new Surface(21, 74, _x + 19, _y + 3);
        _weapon2 = new InteractiveSurface(15, 17, _x + 64, _y + 52);
        _range2 = new Surface(21, 74, _x + 43, _y + 3);
        _damage = new Surface(22, 25, _x + 93, _y + 40);

        _btnMinimize = new InteractiveSurface(12, 12, _x, _y);
        _preview = new InteractiveSurface(160, 96, _x, _y);
        _btnStandoff = new ImageButton(36, 15, _x + 83, _y + 4);
        _btnCautious = new ImageButton(36, 15, _x + 120, _y + 4);
        _btnStandard = new ImageButton(36, 15, _x + 83, _y + 20);
        _btnAggressive = new ImageButton(36, 15, _x + 120, _y + 20);
        _btnDisengage = new ImageButton(36, 15, _x + 120, _y + 36);
        _btnUfo = new ImageButton(36, 17, _x + 120, _y + 52);
        _txtAmmo1 = new Text(16, 9, _x + 4, _y + 70);
        _txtAmmo2 = new Text(16, 9, _x + 64, _y + 70);
        _txtDistance = new Text(40, 9, _x + 116, _y + 72);
        _txtStatus = new Text(150, 9, _x + 4, _y + 85);
        _btnMinimizedIcon = new InteractiveSurface(32, 20, _minimizedIconX, _minimizedIconY);
        _txtInterceptionNumber = new Text(16, 9, _minimizedIconX + 18, _minimizedIconY + 6);

        _mode = _btnStandoff;
        _craftDamageAnimTimer = new Timer(500);

        // Set palette
        setInterface("dogfight");

        add(_window);
        add(_battle);
        add(_weapon1);
        add(_range1);
        add(_weapon2);
        add(_range2);
        add(_damage);
        add(_btnMinimize);
        add(_btnStandoff, "standoffButton", "dogfight", _window);
        add(_btnCautious, "cautiousButton", "dogfight", _window);
        add(_btnStandard, "standardButton", "dogfight", _window);
        add(_btnAggressive, "aggressiveButton", "dogfight", _window);
        add(_btnDisengage, "disengageButton", "dogfight", _window);
        add(_btnUfo, "ufoButton", "dogfight", _window);
        add(_txtAmmo1, "numbers", "dogfight", _window);
        add(_txtAmmo2, "numbers", "dogfight", _window);
        add(_txtDistance, "distance", "dogfight", _window);
        add(_preview);
        add(_txtStatus, "text", "dogfight", _window);
        add(_btnMinimizedIcon);
        add(_txtInterceptionNumber, "minimizedNumber", "dogfight");

        _btnStandoff.invalidate(false);
        _btnCautious.invalidate(false);
        _btnStandard.invalidate(false);
        _btnAggressive.invalidate(false);
        _btnDisengage.invalidate(false);
        _btnUfo.invalidate(false);

        // Set up objects
        RuleInterface dogfightInterface = _game.getMod().getInterface("dogfight");

        Surface graphic;
        graphic = _game.getMod().getSurface("INTERWIN.DAT");
        graphic.setX(0);
        graphic.setY(0);
        graphic.getCrop().x = 0;
        graphic.getCrop().y = 0;
        graphic.getCrop().w = _window.getWidth();
        graphic.getCrop().h = _window.getHeight();
        _window.drawRect(ref graphic.getCrop(), 15);
        graphic.blit(_window);

        _preview.drawRect(ref graphic.getCrop(), 15);
        graphic.getCrop().y = dogfightInterface.getElement("previewTop").y;
        graphic.getCrop().h = dogfightInterface.getElement("previewTop").h;
        graphic.blit(_preview);
        graphic.setY(_window.getHeight() - dogfightInterface.getElement("previewBot").h);
        graphic.getCrop().y = dogfightInterface.getElement("previewBot").y;
        graphic.getCrop().h = dogfightInterface.getElement("previewBot").h;
        graphic.blit(_preview);
        if (string.IsNullOrEmpty(ufo.getRules().getModSprite()))
        {
            graphic.getCrop().y = dogfightInterface.getElement("previewMid").y + dogfightInterface.getElement("previewMid").h * _ufo.getRules().getSprite();
            graphic.getCrop().h = dogfightInterface.getElement("previewMid").h;
        }
        else
        {
            graphic = _game.getMod().getSurface(ufo.getRules().getModSprite());
        }
        graphic.setX(dogfightInterface.getElement("previewTop").x);
        graphic.setY(dogfightInterface.getElement("previewTop").h);
        graphic.blit(_preview);
        _preview.setVisible(false);
        _preview.onMouseClick(previewClick);

        _btnMinimize.onMouseClick(btnMinimizeClick);

        _btnStandoff.copy(_window);
        _btnStandoff.setGroup(_mode);
        _btnStandoff.onMousePress(btnStandoffPress);

        _btnCautious.copy(_window);
        _btnCautious.setGroup(_mode);
        _btnCautious.onMousePress(btnCautiousPress);

        _btnStandard.copy(_window);
        _btnStandard.setGroup(_mode);
        _btnStandard.onMousePress(btnStandardPress);

        _btnAggressive.copy(_window);
        _btnAggressive.setGroup(_mode);
        _btnAggressive.onMousePress(btnAggressivePress);

        _btnDisengage.copy(_window);
        _btnDisengage.onMousePress(btnDisengagePress);
        _btnDisengage.setGroup(_mode);

        _btnUfo.copy(_window);
        _btnUfo.onMouseClick(btnUfoClick);

        _txtDistance.setText("640");

        _txtStatus.setText(tr("STR_STANDOFF"));

        SurfaceSet set = _game.getMod().getSurfaceSet("INTICON.PCK");

        // Create the minimized dogfight icon.
        Surface frame = set.getFrame(_craft.getRules().getSprite());
        frame.setX(0);
        frame.setY(0);
        frame.blit(_btnMinimizedIcon);
        _btnMinimizedIcon.onMouseClick(btnMinimizedIconClick);
        _btnMinimizedIcon.setVisible(false);

        // Draw correct number on the minimized dogfight icon.
        string ss1;
        if (_craft.getInterceptionOrder() == 0)
        {
            int maxInterceptionOrder = 0;
            foreach (var baseIt in _game.getSavedGame().getBases())
            {
                foreach (var craftIt in baseIt.getCrafts())
                {
                    if (craftIt.getInterceptionOrder() > maxInterceptionOrder)
                    {
                        maxInterceptionOrder = craftIt.getInterceptionOrder();
                    }
                }
            }
            _craft.setInterceptionOrder(++maxInterceptionOrder);
        }
        ss1 = _craft.getInterceptionOrder().ToString();
        _txtInterceptionNumber.setText(ss1);
        _txtInterceptionNumber.setVisible(false);

        // define the colors to be used
        _colors[(int)ColorNames.CRAFT_MIN] = dogfightInterface.getElement("craftRange").color;
        _colors[(int)ColorNames.CRAFT_MAX] = dogfightInterface.getElement("craftRange").color2;
        _colors[(int)ColorNames.RADAR_MIN] = dogfightInterface.getElement("radarRange").color;
        _colors[(int)ColorNames.RADAR_MAX] = dogfightInterface.getElement("radarRange").color2;
        _colors[(int)ColorNames.DAMAGE_MIN] = dogfightInterface.getElement("damageRange").color;
        _colors[(int)ColorNames.DAMAGE_MAX] = dogfightInterface.getElement("damageRange").color2;
        _colors[(int)ColorNames.BLOB_MIN] = dogfightInterface.getElement("radarDetail").color;
        _colors[(int)ColorNames.RANGE_METER] = dogfightInterface.getElement("radarDetail").color2;
        _colors[(int)ColorNames.DISABLED_WEAPON] = dogfightInterface.getElement("disabledWeapon").color;
        _colors[(int)ColorNames.DISABLED_RANGE] = dogfightInterface.getElement("disabledWeapon").color2;
        _colors[(int)ColorNames.DISABLED_AMMO] = dogfightInterface.getElement("disabledAmmo").color;

        for (int i = 0; i < _craft.getRules().getWeapons(); ++i)
        {
            CraftWeapon w = _craft.getWeapons()[i];
            if (w == null)
                continue;

            Surface weapon = null, range = null;
            Text ammo = null;
            int x1, x2;
            if (i == 0)
            {
                weapon = _weapon1;
                range = _range1;
                ammo = _txtAmmo1;
                x1 = 2;
                x2 = 0;
            }
            else
            {
                weapon = _weapon2;
                range = _range2;
                ammo = _txtAmmo2;
                x1 = 0;
                x2 = 18;
            }

            // Draw weapon icon
            frame = set.getFrame(w.getRules().getSprite() + 5);

            frame.setX(0);
            frame.setY(0);
            frame.blit(weapon);

            // Draw ammo
            string ss = w.getAmmo().ToString();
            ammo.setText(ss);

            // Draw range (1 km = 1 pixel)
            byte color = (byte)_colors[(int)ColorNames.RANGE_METER];
            range.@lock();

            int rangeY = range.getHeight() - w.getRules().getRange(), connectY = 57;
            for (int x = x1; x <= x1 + 18; x += 2)
            {
                range.setPixel(x, rangeY, color);
            }

            int minY = 0, maxY = 0;
            if (rangeY < connectY)
            {
                minY = rangeY;
                maxY = connectY;
            }
            else if (rangeY > connectY)
            {
                minY = connectY;
                maxY = rangeY;
            }
            for (int y = minY; y <= maxY; ++y)
            {
                range.setPixel(x1 + x2, y, color);
            }
            for (int x = x2; x <= x2 + 2; ++x)
            {
                range.setPixel(x, connectY, color);
            }
            range.unlock();
        }

        if (!(_craft.getRules().getWeapons() > 0 && _craft.getWeapons()[0] != null))
        {
            _weapon1.setVisible(false);
            _range1.setVisible(false);
            _txtAmmo1.setVisible(false);
        }
        if (!(_craft.getRules().getWeapons() > 1 && _craft.getWeapons()[1] != null))
        {
            _weapon2.setVisible(false);
            _range2.setVisible(false);
            _txtAmmo2.setVisible(false);
        }

        // Draw damage indicator.
        frame = set.getFrame(_craft.getRules().getSprite() + 11);
        frame.setX(0);
        frame.setY(0);
        frame.blit(_damage);

        _craftDamageAnimTimer.onTimer((StateHandler)animateCraftDamage);

        // don't set these variables if the ufo is already engaged in a dogfight
        if (_ufo.getEscapeCountdown() == 0)
        {
            _ufo.setFireCountdown(0);
            int escapeCountdown = _ufo.getRules().getBreakOffTime() + RNG.generate(0, _ufo.getRules().getBreakOffTime()) - 30 * _game.getSavedGame().getDifficultyCoefficient();
            _ufo.setEscapeCountdown(Math.Max(1, escapeCountdown));
        }

        // technically this block is redundant, but i figure better to initialize the variables as SOMETHING
        if (_craft.getRules().getWeapons() > 0 && _craft.getWeapons()[0] != null)
        {
            _w1FireInterval = _craft.getWeapons()[0].getRules().getStandardReload();
        }
        if (_craft.getRules().getWeapons() > 1 && _craft.getWeapons()[1] != null)
        {
            _w2FireInterval = _craft.getWeapons()[1].getRules().getStandardReload();
        }

        // Set UFO size - going to be moved to Ufo class to implement simultaneous dogfights.
        string ufoSize = _ufo.getRules().getSize();
        if (ufoSize == "STR_VERY_SMALL")
        {
            _ufoSize = 0;
        }
        else if (ufoSize == "STR_SMALL")
        {
            _ufoSize = 1;
        }
        else if (ufoSize == "STR_MEDIUM_UC")
        {
            _ufoSize = 2;
        }
        else if (ufoSize == "STR_LARGE")
        {
            _ufoSize = 3;
        }
        else
        {
            _ufoSize = 4;
        }

        // Get crafts height. Used for damage indication.
        int width = _damage.getWidth() / 2;
        for (int y = 0; y < _damage.getHeight(); ++y)
        {
            byte pixelColor = _damage.getPixel(width, y);
            if (pixelColor >= _colors[(int)ColorNames.CRAFT_MIN] && pixelColor < _colors[(int)ColorNames.CRAFT_MAX])
            {
                ++_craftHeight;
            }
        }

        drawCraftDamage();

        // Used for weapon toggling.
        _weapon1.onMouseClick(weapon1Click);
        _weapon2.onMouseClick(weapon2Click);
    }

    /**
     * Cleans up the dogfight state.
     */
    ~DogfightState()
    {
        _craftDamageAnimTimer = null;
        _projectiles.Clear();
    }

    /**
     * Hides the front view of the UFO.
     * @param action Pointer to an action.
     */
    void previewClick(Action _)
    {
        _preview.setVisible(false);
        // Reenable all other buttons to prevent misclicks
        _btnStandoff.setVisible(true);
        _btnCautious.setVisible(true);
        _btnStandard.setVisible(true);
        _btnAggressive.setVisible(true);
        _btnDisengage.setVisible(true);
        _btnUfo.setVisible(true);
        _btnMinimize.setVisible(true);
        _weapon1.setVisible(true);
        _weapon2.setVisible(true);
    }

    /**
     * Minimizes the dogfight window.
     * @param action Pointer to an action.
     */
    void btnMinimizeClick(Action _)
    {
        if (!_ufo.isCrashed() && !_craft.isDestroyed() && !_ufoBreakingOff)
        {
            if (_currentDist >= STANDOFF_DIST)
            {
                setMinimized(true);
            }
            else
            {
                setStatus("STR_MINIMISE_AT_STANDOFF_RANGE_ONLY");
            }
        }
    }

    /**
     * Sets the state to minimized/maximized status.
     * @param minimized Is the dogfight minimized?
     */
    internal void setMinimized(bool minimized)
    {
	    // set these to the same as the incoming minimized state
	    _minimized = minimized;
	    _btnMinimizedIcon.setVisible(minimized);
	    _txtInterceptionNumber.setVisible(minimized);

	    // set these to the opposite of the incoming minimized state
	    _window.setVisible(!minimized);
	    _btnStandoff.setVisible(!minimized);
	    _btnCautious.setVisible(!minimized);
	    _btnStandard.setVisible(!minimized);
	    _btnAggressive.setVisible(!minimized);
	    _btnDisengage.setVisible(!minimized);
	    _btnUfo.setVisible(!minimized);
	    _btnMinimize.setVisible(!minimized);
	    _battle.setVisible(!minimized);
	    _weapon1.setVisible(!minimized);
	    _range1.setVisible(!minimized);
	    _weapon2.setVisible(!minimized);
	    _range2.setVisible(!minimized);
	    _damage.setVisible(!minimized);
	    _txtAmmo1.setVisible(!minimized);
	    _txtAmmo2.setVisible(!minimized);
	    _txtDistance.setVisible(!minimized);
	    _txtStatus.setVisible(!minimized);

	    // set to false regardless
	    _preview.setVisible(false);
    }

    /**
     * Updates the status text and restarts
     * the text timeout counter.
     * @param status New status text.
     */
    void setStatus(string status)
    {
	    _txtStatus.setText(tr(status));
	    _timeout = 50;
    }

    /**
     * Switches to Standoff mode (maximum range).
     * @param action Pointer to an action.
     */
    void btnStandoffPress(Action _)
    {
        if (!_ufo.isCrashed() && !_craft.isDestroyed() && !_ufoBreakingOff)
        {
            _end = false;
            setStatus("STR_STANDOFF");
            _targetDist = STANDOFF_DIST;
        }
    }

    /**
     * Switches to Cautious mode (maximum weapon range).
     * @param action Pointer to an action.
     */
    void btnCautiousPress(Action _)
    {
        if (!_ufo.isCrashed() && !_craft.isDestroyed() && !_ufoBreakingOff)
        {
            _end = false;
            setStatus("STR_CAUTIOUS_ATTACK");
            if (_craft.getRules().getWeapons() > 0 && _craft.getWeapons()[0] != null)
            {
                _w1FireInterval = _craft.getWeapons()[0].getRules().getCautiousReload();
            }
            if (_craft.getRules().getWeapons() > 1 && _craft.getWeapons()[1] != null)
            {
                _w2FireInterval = _craft.getWeapons()[1].getRules().getCautiousReload();
            }
            minimumDistance();
        }
    }

    /**
     * Sets the craft to the minimum distance
     * required to fire a weapon.
     */
    void minimumDistance()
    {
        int max = 0;
        foreach (var i in _craft.getWeapons())
        {
            if (i == null)
                continue;
            if (i.getRules().getRange() > max && i.getAmmo() > 0)
            {
                max = i.getRules().getRange();
            }
        }
        if (max == 0)
        {
            _targetDist = STANDOFF_DIST;
        }
        else
        {
            _targetDist = max * 8;
        }
    }

    /**
     * Switches to Standard mode (minimum weapon range).
     * @param action Pointer to an action.
     */
    void btnStandardPress(Action _)
    {
        if (!_ufo.isCrashed() && !_craft.isDestroyed() && !_ufoBreakingOff)
        {
            _end = false;
            setStatus("STR_STANDARD_ATTACK");
            if (_craft.getRules().getWeapons() > 0 && _craft.getWeapons()[0] != null)
            {
                _w1FireInterval = _craft.getWeapons()[0].getRules().getStandardReload();
            }
            if (_craft.getRules().getWeapons() > 1 && _craft.getWeapons()[1] != null)
            {
                _w2FireInterval = _craft.getWeapons()[1].getRules().getStandardReload();
            }
            maximumDistance();
        }
    }

    /**
     * Sets the craft to the maximum distance
     * required to fire a weapon.
     */
    void maximumDistance()
    {
        int min = 1000;
        foreach (var i in _craft.getWeapons())
        {
            if (i == null)
                continue;
            if (i.getRules().getRange() < min && i.getAmmo() > 0)
            {
                min = i.getRules().getRange();
            }
        }
        if (min == 1000)
        {
            _targetDist = STANDOFF_DIST;
        }
        else
        {
            _targetDist = min * 8;
        }
    }

    /**
     * Switches to Aggressive mode (minimum range).
     * @param action Pointer to an action.
     */
    void btnAggressivePress(Action _)
    {
        if (!_ufo.isCrashed() && !_craft.isDestroyed() && !_ufoBreakingOff)
        {
            _end = false;
            setStatus("STR_AGGRESSIVE_ATTACK");
            if (_craft.getRules().getWeapons() > 0 && _craft.getWeapons()[0] != null)
            {
                _w1FireInterval = _craft.getWeapons()[0].getRules().getAggressiveReload();
            }
            if (_craft.getRules().getWeapons() > 1 && _craft.getWeapons()[1] != null)
            {
                _w2FireInterval = _craft.getWeapons()[1].getRules().getAggressiveReload();
            }
            _targetDist = 64;
        }
    }

    /**
     * Disengages from the UFO.
     * @param action Pointer to an action.
     */
    void btnDisengagePress(Action _)
    {
        if (!_ufo.isCrashed() && !_craft.isDestroyed() && !_ufoBreakingOff)
        {
            _end = true;
            setStatus("STR_DISENGAGING");
            _targetDist = 800;
        }
    }

    /**
     * Shows a front view of the UFO.
     * @param action Pointer to an action.
     */
    void btnUfoClick(Action _)
    {
        _preview.setVisible(true);
        // Disable all other buttons to prevent misclicks
        _btnStandoff.setVisible(false);
        _btnCautious.setVisible(false);
        _btnStandard.setVisible(false);
        _btnAggressive.setVisible(false);
        _btnDisengage.setVisible(false);
        _btnUfo.setVisible(false);
        _btnMinimize.setVisible(false);
        _weapon1.setVisible(false);
        _weapon2.setVisible(false);
    }

    /**
     * Maximizes the interception window.
     * @param action Pointer to an action.
     */
    void btnMinimizedIconClick(Action _)
    {
        if (_craft.getRules().isWaterOnly() && _ufo.getAltitudeInt() > _craft.getRules().getMaxAltitude())
        {
            _state.popup(new DogfightErrorState(_craft, tr("STR_UNABLE_TO_ENGAGE_DEPTH")));
            setWaitForAltitude(true);
        }
        else if (_craft.getRules().isWaterOnly() && !_state.getGlobe().insideLand(_craft.getLongitude(), _craft.getLatitude()))
        {
            _state.popup(new DogfightErrorState(_craft, tr("STR_UNABLE_TO_ENGAGE_AIRBORNE")));
            setWaitForPoly(true);
        }
        else
        {
            setMinimized(false);
        }
    }

    internal void setWaitForAltitude(bool wait) =>
        _waitForAltitude = wait;

    internal void setWaitForPoly(bool wait) =>
        _waitForPoly = wait;

    /**
     * Animates interceptor damage by changing the color and redrawing the image.
     */
    void animateCraftDamage()
    {
        if (_minimized)
        {
            return;
        }
        --_currentCraftDamageColor;
        if (_currentCraftDamageColor < _colors[(int)ColorNames.DAMAGE_MIN])
        {
            _currentCraftDamageColor = _colors[(int)ColorNames.DAMAGE_MAX];
        }
        drawCraftDamage();
    }

    /**
     * Toggles usage of weapon number 1.
     * @param action Pointer to an action.
     */
    void weapon1Click(Action _)
    {
        _weapon1Enabled = !_weapon1Enabled;
        recolor(0, _weapon1Enabled);
    }

    /**
     * Toggles usage of weapon number 2.
     * @param action Pointer to an action.
     */
    void weapon2Click(Action _)
    {
        _weapon2Enabled = !_weapon2Enabled;
        recolor(1, _weapon2Enabled);
    }

    /**
     * Changes colors of weapon icons, range indicators and ammo texts base on current weapon state.
     * @param weaponNo - number of weapon for which colors must be changed.
     * @param currentState - state of weapon (enabled = true, disabled = false).
     */
    void recolor(int weaponNo, bool currentState)
    {
	    InteractiveSurface weapon = null;
	    Text ammo = null;
	    Surface range = null;
	    if (weaponNo == 0)
	    {
		    weapon = _weapon1;
		    ammo = _txtAmmo1;
		    range = _range1;
	    }
	    else if (weaponNo == 1)
	    {
		    weapon = _weapon2;
		    ammo = _txtAmmo2;
		    range = _range2;
	    }
	    else
	    {
		    return;
	    }

	    if (currentState)
	    {
            weapon.offset(-_colors[(int)ColorNames.DISABLED_WEAPON]);
		    ammo.offset(-_colors[(int)ColorNames.DISABLED_AMMO]);
		    range.offset(-_colors[(int)ColorNames.DISABLED_RANGE]);
	    }
	    else
	    {
		    weapon.offset(_colors[(int)ColorNames.DISABLED_WEAPON]);
		    ammo.offset(_colors[(int)ColorNames.DISABLED_AMMO]);
		    range.offset(_colors[(int)ColorNames.DISABLED_RANGE]);
	    }
    }

    /**
     * Draws interceptor damage according to percentage of HP's left.
     */
    void drawCraftDamage()
    {
        if (_craft.getDamagePercentage() != 0)
        {
            if (!_craftDamageAnimTimer.isRunning())
            {
                _craftDamageAnimTimer.start();
                if (_currentCraftDamageColor < _colors[(int)ColorNames.DAMAGE_MIN])
                {
                    _currentCraftDamageColor = _colors[(int)ColorNames.DAMAGE_MIN];
                }
            }
            int damagePercentage = _craft.getDamagePercentage();
            int rowsToColor = (int)Math.Floor((double)_craftHeight * (double)(damagePercentage / 100.0));
            if (rowsToColor == 0)
            {
                return;
            }
            int rowsColored = 0;
            bool rowColored = false;
            for (int y = 0; y < _damage.getHeight(); ++y)
            {
                rowColored = false;
                for (int x = 0; x < _damage.getWidth(); ++x)
                {
                    int pixelColor = _damage.getPixel(x, y);
                    if (pixelColor >= _colors[(int)ColorNames.DAMAGE_MIN] && pixelColor <= _colors[(int)ColorNames.DAMAGE_MAX])
                    {
                        _damage.setPixel(x, y, (byte)_currentCraftDamageColor);
                        rowColored = true;
                    }
                    if (pixelColor >= _colors[(int)ColorNames.CRAFT_MIN] && pixelColor < _colors[(int)ColorNames.CRAFT_MAX])
                    {
                        _damage.setPixel(x, y, (byte)_currentCraftDamageColor);
                        rowColored = true;
                    }
                }
                if (rowColored)
                {
                    ++rowsColored;
                }
                if (rowsColored == rowsToColor)
                {
                    break;
                }
            }
        }
    }

    /**
     * Returns the UFO associated to this dogfight.
     * @return Returns pointer to UFO object associated to this dogfight.
     */
    internal Ufo getUfo() =>
	    _ufo;

    /**
     * Returns the craft associated to this dogfight.
     * @return Returns pointer to craft object associated to this dogfight.
     */
    internal Craft getCraft() =>
	    _craft;

    internal bool getWaitForAltitude() =>
	    _waitForAltitude;

    internal bool getWaitForPoly() =>
	    _waitForPoly;

    /**
     * Returns true if state is minimized. Otherwise returns false.
     * @return Is the dogfight minimized?
     */
    internal bool isMinimized() =>
	    _minimized;

    /**
     * Returns interception number.
     * @return interception number
     */
    internal int getInterceptionNumber() =>
	    _interceptionNumber;

    /**
     * Sets interceptions count. Used to properly position the window.
     * @param count Amount of interception windows.
     */
    internal void setInterceptionsCount(uint count)
    {
	    _interceptionsCount = count;
	    calculateWindowPosition();
	    moveWindow();
    }

    /**
     * Sets interception number. Used to draw proper number when window minimized.
     * @param number ID number.
     */
    internal void setInterceptionNumber(int number) =>
	    _interceptionNumber = number;

    /**
     * Checks whether the dogfight should end.
     * @return Returns true if the dogfight should end, otherwise returns false.
     */
    internal bool dogfightEnded() =>
	    _endDogfight;

    /**
     * Calculates dogfight window position according to
     * number of active interceptions.
     */
    void calculateWindowPosition()
    {
        _minimizedIconX = 5;
        _minimizedIconY = (5 * _interceptionNumber) + (16 * (_interceptionNumber - 1));

        if (_interceptionsCount == 1)
        {
            _x = 80;
            _y = 52;
        }
        else if (_interceptionsCount == 2)
        {
            if (_interceptionNumber == 1)
            {
                _x = 80;
                _y = 0;
            }
            else // 2
            {
                _x = 80;
                //_y = (_game.getScreen().getHeight() / 2) - 96;
                _y = 200 - _window.getHeight();//96;
            }
        }
        else if (_interceptionsCount == 3)
        {
            if (_interceptionNumber == 1)
            {
                _x = 80;
                _y = 0;
            }
            else if (_interceptionNumber == 2)
            {
                _x = 0;
                //_y = (_game.getScreen().getHeight() / 2) - 96;
                _y = 200 - _window.getHeight();//96;
            }
            else // 3
            {
                //_x = (_game.getScreen().getWidth() / 2) - 160;
                //_y = (_game.getScreen().getHeight() / 2) - 96;
                _x = 320 - _window.getWidth();//160;
                _y = 200 - _window.getHeight();//96;
            }
        }
        else
        {
            if (_interceptionNumber == 1)
            {
                _x = 0;
                _y = 0;
            }
            else if (_interceptionNumber == 2)
            {
                //_x = (_game.getScreen().getWidth() / 2) - 160;
                _x = 320 - _window.getWidth();//160;
                _y = 0;
            }
            else if (_interceptionNumber == 3)
            {
                _x = 0;
                //_y = (_game.getScreen().getHeight() / 2) - 96;
                _y = 200 - _window.getHeight();//96;
            }
            else // 4
            {
                //_x = (_game.getScreen().getWidth() / 2) - 160;
                //_y = (_game.getScreen().getHeight() / 2) - 96;
                _x = 320 - _window.getWidth();//160;
                _y = 200 - _window.getHeight();//96;
            }
        }
        _x += _game.getScreen().getDX();
        _y += _game.getScreen().getDY();
    }

    /**
     * Relocates all dogfight window elements to
     * calculated position. This is used when multiple
     * interceptions are running.
     */
    void moveWindow()
    {
        int x = _window.getX() - _x;
        int y = _window.getY() - _y;
        foreach (var i in _surfaces)
        {
            i.setX(i.getX() - x);
            i.setY(i.getY() - y);
        }
        _btnMinimizedIcon.setX(_minimizedIconX); _btnMinimizedIcon.setY(_minimizedIconY);
        _txtInterceptionNumber.setX(_minimizedIconX + 18); _txtInterceptionNumber.setY(_minimizedIconY + 6);
    }

    /**
     * Runs the higher level dogfight functionality.
     */
    internal override void think()
    {
        if (!_endDogfight)
        {
            update();
            _craftDamageAnimTimer.think(this, null);
        }
        if (!_craft.isInDogfight() || _craft.getDestination() != _ufo || _ufo.getStatus() == UfoStatus.LANDED)
        {
            endDogfight();
        }
    }

    /**
     * Ends the dogfight.
     */
    void endDogfight()
    {
        if (_endDogfight)
            return;
        if (_craft != null)
        {
            _craft.setInDogfight(false);
            _craft.setInterceptionOrder(0);
        }
        // set the ufo as "free" for the next engagement (as applicable)
        if (_ufo != null)
            _ufo.setInterceptionProcessed(false);
        _endDogfight = true;
    }

    /**
     * Updates all the elements in the dogfight, including ufo movement,
     * weapons fire, projectile movement, ufo escape conditions,
     * craft and ufo destruction conditions, and retaliation mission generation, as applicable.
     */
    void update()
    {
	    bool finalRun = false;
	    // Check if craft is not low on fuel when window minimized, and
	    // Check if crafts destination hasn't been changed when window minimized.
	    Ufo u = (Ufo)_craft.getDestination();
	    if (u != _ufo || !_craft.isInDogfight() || _craft.getLowFuel() || (_minimized && _ufo.isCrashed()))
	    {
		    endDogfight();
		    return;
	    }

	    if (!_minimized)
	    {
		    animate();
		    if (!_ufo.isCrashed() && !_ufo.isDestroyed() && !_craft.isDestroyed() && !_ufo.getInterceptionProcessed())
		    {
			    _ufo.setInterceptionProcessed(true);
			    int escapeCounter = _ufo.getEscapeCountdown();

			    if (escapeCounter > 0)
			    {
				    escapeCounter--;
				    _ufo.setEscapeCountdown(escapeCounter);
				    // Check if UFO is breaking off.
				    if (escapeCounter == 0)
				    {
					    _ufo.setSpeed(_ufo.getRules().getMaxSpeed());
				    }
			    }
			    if (_ufo.getFireCountdown() > 0)
			    {
				    _ufo.setFireCountdown(_ufo.getFireCountdown() - 1);
			    }
		    }
	    }
	    // Crappy craft is chasing UFO.
	    if (_ufo.getSpeed() > _craft.getRules().getMaxSpeed())
	    {
		    _ufoBreakingOff = true;
		    finalRun = true;
		    setStatus("STR_UFO_OUTRUNNING_INTERCEPTOR");
	    }
	    else
	    {
		    _ufoBreakingOff = false;
	    }

	    bool projectileInFlight = false;
	    if (!_minimized)
	    {
		    int distanceChange = 0;

		    // Update distance
		    if (!_ufoBreakingOff)
		    {
			    if (_currentDist < _targetDist && !_ufo.isCrashed() && !_craft.isDestroyed())
			    {
				    distanceChange = 4;
				    if (_currentDist + distanceChange >_targetDist)
				    {
					    distanceChange = _targetDist - _currentDist;
				    }
			    }
			    else if (_currentDist > _targetDist && !_ufo.isCrashed() && !_craft.isDestroyed())
			    {
				    distanceChange = -2;
			    }

			    // don't let the interceptor mystically push or pull its fired projectiles
			    foreach (var it in _projectiles)
			    {
				    if (it.getGlobalType() != CraftWeaponProjectileGlobalType.CWPGT_BEAM && it.getDirection() == (int)Directions.D_UP) it.setPosition(it.getPosition() + distanceChange);
			    }
		    }
		    else
		    {
			    distanceChange = 4;

			    // UFOs can try to outrun our missiles, don't adjust projectile positions here
			    // If UFOs ever fire anything but beams, those positions need to be adjust here though.
		    }

		    _currentDist += distanceChange;

		    string ss = _currentDist.ToString();
		    _txtDistance.setText(ss);

		    // Move projectiles and check for hits.
		    foreach (var it in _projectiles)
		    {
			    CraftWeaponProjectile p = it;
			    p.move();
			    // Projectiles fired by interceptor.
			    if (p.getDirection() == (int)Directions.D_UP)
			    {
				    // Projectile reached the UFO - determine if it's been hit.
				    if (((p.getPosition() >= _currentDist) || (p.getGlobalType() == CraftWeaponProjectileGlobalType.CWPGT_BEAM && p.toBeRemoved())) && !_ufo.isCrashed() && !p.getMissed())
				    {
					    // UFO hit.
					    if (RNG.percent((p.getAccuracy() * (100 + 300 / (5 - _ufoSize)) + 100) / 200))
					    {
						    // Formula delivered by Volutar
						    int damage = RNG.generate(p.getDamage() / 2, p.getDamage());
						    _ufo.setDamage(_ufo.getDamage() + damage);
						    if (_ufo.isCrashed())
						    {
							    _ufo.setShotDownByCraftId(_craft.getUniqueId());
							    _ufo.setSpeed(0);
							    _ufo.setDestination(null);
							    // if the ufo got destroyed here, these no longer apply
							    _ufoBreakingOff = false;
							    finalRun = false;
							    _end = false;
						    }
						    if (_ufo.getHitFrame() == 0)
						    {
							    _animatingHit = true;
							    _ufo.setHitFrame(3);
						    }

						    setStatus("STR_UFO_HIT");
						    _game.getMod().getSound("GEO.CAT", (uint)Mod.Mod.UFO_HIT).play();
						    p.remove();
					    }
					    // Missed.
					    else
					    {
						    if (p.getGlobalType() == CraftWeaponProjectileGlobalType.CWPGT_BEAM)
						    {
							    p.remove();
						    }
						    else
						    {
							    p.setMissed(true);
						    }
					    }
				    }
				    // Check if projectile passed it's maximum range.
				    if (p.getGlobalType() == CraftWeaponProjectileGlobalType.CWPGT_MISSILE && p.getPosition() / 8 >= p.getRange())
				    {
					    p.remove();
				    }
				    else if (!_ufo.isCrashed())
				    {
					    projectileInFlight = true;
				    }
			    }
			    // Projectiles fired by UFO.
			    else if (p.getDirection() == (int)Directions.D_DOWN)
			    {
				    if (p.getGlobalType() == CraftWeaponProjectileGlobalType.CWPGT_MISSILE || (p.getGlobalType() == CraftWeaponProjectileGlobalType.CWPGT_BEAM && p.toBeRemoved()))
				    {
					    if (RNG.percent(p.getAccuracy()))
					    {
						    // Formula delivered by Volutar
						    int damage = RNG.generate(0, _ufo.getRules().getWeaponPower());
						    if (damage != 0)
						    {
							    _craft.setDamage(_craft.getDamage() + damage);
							    drawCraftDamage();
							    setStatus("STR_INTERCEPTOR_DAMAGED");
							    _game.getMod().getSound("GEO.CAT", (uint)Mod.Mod.INTERCEPTOR_HIT).play(); //10
							    if (_mode == _btnCautious && _craft.getDamagePercentage() >= 50)
							    {
								    _targetDist = STANDOFF_DIST;
							    }
						    }
					    }
					    p.remove();
				    }
			    }
		    }

		    // Remove projectiles that hit or missed their target.
		    for (var it = 0; it < _projectiles.Count;)
		    {
			    if (_projectiles[it].toBeRemoved() == true || (_projectiles[it].getMissed() == true && _projectiles[it].getPosition() <= 0))
			    {
				    _projectiles.RemoveAt(it);
			    }
			    else
			    {
				    ++it;
			    }
		    }

		    // Handle weapons and craft distance.
		    for (int i = 0; i < _craft.getRules().getWeapons(); ++i)
		    {
			    CraftWeapon w = _craft.getWeapons()[i];
			    if (w == null)
			    {
				    continue;
			    }
			    int wTimer;
			    if (i == 0)
			    {
				    wTimer = _w1FireCountdown;
			    }
			    else
			    {
				    wTimer = _w2FireCountdown;
			    }

			    // Handle weapon firing
			    if (wTimer == 0 && _currentDist <= w.getRules().getRange() * 8 && w.getAmmo() > 0 && _mode != _btnStandoff
				    && _mode != _btnDisengage && !_ufo.isCrashed() && !_craft.isDestroyed())
			    {
				    if (i == 0)
				    {
					    if (_weapon1Enabled)
					    {
						    fireWeapon1();
						    projectileInFlight = true;
					    }
				    }
				    else
				    {
					    if (_weapon2Enabled)
					    {
						    fireWeapon2();
						    projectileInFlight = true;
					    }
				    }
			    }
			    else if (wTimer > 0)
			    {
				    if (i == 0)
				    {
					    _w1FireCountdown--;
				    }
				    else
				    {
					    _w2FireCountdown--;
				    }
			    }

			    if (w.getAmmo() == 0 && !projectileInFlight && !_craft.isDestroyed())
			    {
				    // Handle craft distance according to option set by user and available ammo.
				    if (_mode == _btnCautious)
				    {
					    minimumDistance();
				    }
				    else if (_mode == _btnStandard)
				    {
					    maximumDistance();
				    }
			    }
		    }

		    // Handle UFO firing.
		    if (_currentDist <= _ufo.getRules().getWeaponRange() * 8 && !_ufo.isCrashed() && !_craft.isDestroyed())
		    {
			    if (_ufo.getShootingAt() == 0)
			    {
				    _ufo.setShootingAt(_interceptionNumber);
			    }
			    if (_ufo.getShootingAt() == _interceptionNumber)
			    {
				    if (_ufo.getFireCountdown() == 0)
				    {
					    ufoFireWeapon();
				    }
			    }
		    }
		    else if (_ufo.getShootingAt() == _interceptionNumber)
		    {
			    _ufo.setShootingAt(0);
		    }
	    }

	    // Check when battle is over.
	    if (_end == true && (((_currentDist > 640 || _minimized) && (_mode == _btnDisengage || _ufoBreakingOff == true)) || (_timeout == 0 && (_ufo.isCrashed() || _craft.isDestroyed()))))
	    {
		    if (_ufoBreakingOff)
		    {
			    _ufo.move();
			    _craft.setDestination(_ufo);
		    }
		    if (!_destroyCraft && (_destroyUfo || _mode == _btnDisengage))
		    {
			    _craft.returnToBase();
		    }
		    if (_ufo.isCrashed())
		    {
			    List<Craft> followers = _ufo.getCraftFollowers();
			    foreach (var i in followers)
			    {
				    if (i.getNumSoldiers() == 0 && i.getNumVehicles() == 0)
				    {
					    i.returnToBase();
				    }
			    }
		    }
		    endDogfight();
	    }

	    if (_currentDist > 640 && _ufoBreakingOff)
	    {
		    finalRun = true;
	    }

	    // End dogfight if craft is destroyed.
	    if (!_end)
	    {
		    if (_craft.isDestroyed())
		    {
			    setStatus("STR_INTERCEPTOR_DESTROYED");
			    _timeout += 30;
			    _game.getMod().getSound("GEO.CAT", (uint)Mod.Mod.INTERCEPTOR_EXPLODE).play();
			    finalRun = true;
			    _destroyCraft = true;
			    _ufo.setShootingAt(0);
		    }

		    // End dogfight if UFO is crashed or destroyed.
		    if (_ufo.isCrashed())
		    {
			    AlienMission mission = _ufo.getMission();
			    mission.ufoShotDown(_ufo);
			    // Check for retaliation trigger.
			    int retaliationOdds = mission.getRules().getRetaliationOdds();
			    if (retaliationOdds == -1)
			    {
				    retaliationOdds = 100 - (4 * (24 - _game.getSavedGame().getDifficultyCoefficient()));
			    }

			    if (RNG.percent(retaliationOdds))
			    {
				    // Spawn retaliation mission.
				    string targetRegion;
				    if (RNG.percent(50 - 6 * _game.getSavedGame().getDifficultyCoefficient()))
				    {
					    // Attack on UFO's mission region
					    targetRegion = _ufo.getMission().getRegion();
				    }
				    else
				    {
					    // Try to find and attack the originating base.
					    targetRegion = _game.getSavedGame().locateRegion(_craft.getBase()).getRules().getType();
					    // TODO: If the base is removed, the mission is canceled.
				    }
				    // Difference from original: No retaliation until final UFO lands (Original: Is spawned).
				    if (_game.getSavedGame().findAlienMission(targetRegion, MissionObjective.OBJECTIVE_RETALIATION) == null)
				    {
					    RuleAlienMission rule = _game.getMod().getRandomMission(MissionObjective.OBJECTIVE_RETALIATION, (uint)_game.getSavedGame().getMonthsPassed());
					    AlienMission newMission = new AlienMission(rule);
					    newMission.setId(_game.getSavedGame().getId("ALIEN_MISSIONS"));
					    newMission.setRegion(targetRegion, _game.getMod());
					    newMission.setRace(_ufo.getAlienRace());
					    newMission.start(newMission.getRules().getWave(0).spawnTimer); // fixed delay for first scout
					    _game.getSavedGame().getAlienMissions().Add(newMission);
				    }
			    }

			    if (_ufo.isDestroyed())
			    {
				    if (_ufo.getShotDownByCraftId().Key == _craft.getUniqueId().Key && _ufo.getShotDownByCraftId().Value == _craft.getUniqueId().Value)
				    {
					    foreach (var country in _game.getSavedGame().getCountries())
					    {
						    if (country.getRules().insideCountry(_ufo.getLongitude(), _ufo.getLatitude()))
						    {
							    country.addActivityXcom(_ufo.getRules().getScore()*2);
							    break;
						    }
					    }
					    foreach (var region in _game.getSavedGame().getRegions())
					    {
						    if (region.getRules().insideRegion(_ufo.getLongitude(), _ufo.getLatitude()))
						    {
							    region.addActivityXcom(_ufo.getRules().getScore()*2);
							    break;
						    }
					    }
					    setStatus("STR_UFO_DESTROYED");
					    _game.getMod().getSound("GEO.CAT", (uint)Mod.Mod.UFO_EXPLODE).play(); //11
				    }
				    _destroyUfo = true;
			    }
			    else
			    {
				    if (_ufo.getShotDownByCraftId().Key == _craft.getUniqueId().Key && _ufo.getShotDownByCraftId().Value == _craft.getUniqueId().Value)
				    {
					    setStatus("STR_UFO_CRASH_LANDS");
					    _game.getMod().getSound("GEO.CAT", (uint)Mod.Mod.UFO_CRASH).play(); //10
					    foreach (var country in _game.getSavedGame().getCountries())
					    {
						    if (country.getRules().insideCountry(_ufo.getLongitude(), _ufo.getLatitude()))
						    {
							    country.addActivityXcom(_ufo.getRules().getScore());
							    break;
						    }
					    }
					    foreach (var region in _game.getSavedGame().getRegions())
					    {
						    if (region.getRules().insideRegion(_ufo.getLongitude(), _ufo.getLatitude()))
						    {
							    region.addActivityXcom(_ufo.getRules().getScore());
							    break;
						    }
					    }
				    }
				    if (!_state.getGlobe().insideLand(_ufo.getLongitude(), _ufo.getLatitude()))
				    {
					    _ufo.setStatus(UfoStatus.DESTROYED);
					    _destroyUfo = true;
				    }
				    else
				    {
					    _ufo.setSecondsRemaining((uint)(RNG.generate(24, 96)*3600));
					    _ufo.setAltitude("STR_GROUND");
					    if (_ufo.getCrashId() == 0)
					    {
						    _ufo.setCrashId(_game.getSavedGame().getId("STR_CRASH_SITE"));
					    }
				    }
			    }
			    _timeout += 30;
			    if (_ufo.getShotDownByCraftId().Key != _craft.getUniqueId().Key && _ufo.getShotDownByCraftId().Value != _craft.getUniqueId().Value)
			    {
				    _timeout += 50;
				    _ufo.setHitFrame(3);
			    }
			    finalRun = true;

			    if (_ufo.getStatus() == UfoStatus.LANDED)
			    {
				    _timeout += 30;
				    finalRun = true;
				    _ufo.setShootingAt(0);
			    }
		    }
	    }

	    if (!projectileInFlight && finalRun)
	    {
		    _end = true;
	    }
    }

    /**
     *	Each time a UFO will try to fire it's cannons
     *	a calculation is made. There's only 10% chance
     *	that it will actually fire.
     */
    void ufoFireWeapon()
    {
	    int fireCountdown = Math.Max(1, (_ufo.getRules().getWeaponReload() - 2 * _game.getSavedGame().getDifficultyCoefficient()));
	    _ufo.setFireCountdown(RNG.generate(0, fireCountdown) + fireCountdown);

	    setStatus("STR_UFO_RETURN_FIRE");
	    CraftWeaponProjectile p = new CraftWeaponProjectile();
	    p.setType(CraftWeaponProjectileType.CWPT_PLASMA_BEAM);
	    p.setAccuracy(60);
	    p.setDamage(_ufo.getRules().getWeaponPower());
	    p.setDirection((int)Directions.D_DOWN);
	    p.setHorizontalPosition(CraftWeaponProjectile.HP_CENTER);
	    p.setPosition(_currentDist - (_ufo.getRules().getRadius() / 2));
	    _projectiles.Add(p);
	    _game.getMod().getSound("GEO.CAT", (uint)Mod.Mod.UFO_FIRE).play();
    }

    /**
     * Fires a shot from the first weapon
     * equipped on the craft.
     */
    void fireWeapon1()
    {
	    CraftWeapon w1 = _craft.getWeapons()[0];
	    if (w1.setAmmo(w1.getAmmo() - 1))
	    {
		    _w1FireCountdown = _w1FireInterval;

		    string ss = w1.getAmmo().ToString();
		    _txtAmmo1.setText(ss);

		    CraftWeaponProjectile p = w1.fire();
		    p.setDirection((int)Directions.D_UP);
		    p.setHorizontalPosition(CraftWeaponProjectile.HP_LEFT);
		    _projectiles.Add(p);

		    _game.getMod().getSound("GEO.CAT", (uint)w1.getRules().getSound()).play();
	    }
    }

    /**
     * Fires a shot from the second weapon
     * equipped on the craft.
     */
    void fireWeapon2()
    {
	    CraftWeapon w2 = _craft.getWeapons()[1];
	    if (w2.setAmmo(w2.getAmmo() - 1))
	    {
		    _w2FireCountdown = _w2FireInterval;

		    string ss = w2.getAmmo().ToString();
		    _txtAmmo2.setText(ss);

		    CraftWeaponProjectile p = w2.fire();
		    p.setDirection((int)Directions.D_UP);
		    p.setHorizontalPosition(CraftWeaponProjectile.HP_RIGHT);
		    _projectiles.Add(p);

		    _game.getMod().getSound("GEO.CAT", (uint)w2.getRules().getSound()).play();
	    }
    }

    /**
     * Animates the window with a palette effect.
     */
    void animate()
    {
	    // Animate radar waves and other stuff.
	    for (int x = 0; x < _window.getWidth(); ++x)
	    {
		    for (int y = 0; y < _window.getHeight(); ++y)
		    {
			    byte radarPixelColor = _window.getPixel(x, y);
			    if (radarPixelColor >= _colors[(int)ColorNames.RADAR_MIN] && radarPixelColor < _colors[(int)ColorNames.RADAR_MAX])
			    {
				    ++radarPixelColor;
				    if (radarPixelColor >= _colors[(int)ColorNames.RADAR_MAX])
				    {
					    radarPixelColor = (byte)_colors[(int)ColorNames.RADAR_MIN];
				    }
				    _window.setPixel(x, y, radarPixelColor);
			    }
		    }
	    }

	    _battle.clear();

	    // Draw UFO.
	    if (!_ufo.isDestroyed())
	    {
		    drawUfo();
	    }

	    // Draw projectiles.
	    foreach (var it in _projectiles)
	    {
		    drawProjectile(it);
	    }

	    // Clears text after a while
	    if (_timeout == 0)
	    {
		    _txtStatus.setText(string.Empty);
	    }
	    else
	    {
		    _timeout--;
	    }

	    // Animate UFO hit.
	    bool lastHitAnimFrame = false;
	    if (_animatingHit && _ufo.getHitFrame() > 0)
	    {
		    _ufo.setHitFrame(_ufo.getHitFrame() - 1);
		    if (_ufo.getHitFrame() == 0)
		    {
			    _animatingHit = false;
			    lastHitAnimFrame = true;
		    }
	    }

	    // Animate UFO crash landing.
	    if (_ufo.isCrashed() && _ufo.getHitFrame() == 0 && !lastHitAnimFrame)
	    {
		    --_ufoSize;
	    }
    }

    /*
     * Draws projectiles on the radar screen.
     * Depending on what type of projectile it is, it's
     * shape will be different. Currently works for
     * original sized blobs 3 x 6 pixels.
     */
    void drawProjectile(CraftWeaponProjectile p)
    {
	    int xPos = _battle.getWidth() / 2 + p.getHorizontalPosition();
	    // Draw missiles.
	    if (p.getGlobalType() == CraftWeaponProjectileGlobalType.CWPGT_MISSILE)
	    {
		    xPos -= 1;
		    int yPos = _battle.getHeight() - p.getPosition() / 8;
		    for (int x = 0; x < 3; ++x)
		    {
			    for (int y = 0; y < 6; ++y)
			    {
				    int pixelOffset = _projectileBlobs[(int)p.getType(), y, x];
				    if (pixelOffset == 0)
				    {
					    continue;
				    }
				    else
				    {
					    byte radarPixelColor = _window.getPixel(xPos + x + 3, yPos + y + 3); // + 3 cause of the window frame
					    byte color = (byte)(radarPixelColor - pixelOffset);
					    if (color < _colors[(int)ColorNames.BLOB_MIN])
					    {
						    color = (byte)_colors[(int)ColorNames.BLOB_MIN];
					    }
					    _battle.setPixel(xPos + x, yPos + y, color);
				    }
			    }
		    }
	    }
	    // Draw beams.
	    else if (p.getGlobalType() == CraftWeaponProjectileGlobalType.CWPGT_BEAM)
	    {
		    int yStart = _battle.getHeight() - 2;
		    int yEnd = _battle.getHeight() - (_currentDist / 8);
		    byte pixelOffset = (byte)p.getState();
		    for (int y = yStart; y > yEnd; --y)
		    {
			    byte radarPixelColor = _window.getPixel(xPos + 3, y + 3);
			    byte color = (byte)(radarPixelColor - pixelOffset);
			    if (color < _colors[(int)ColorNames.BLOB_MIN])
			    {
				    color = (byte)_colors[(int)ColorNames.BLOB_MIN];
			    }
			    _battle.setPixel(xPos, y, color);
		    }
	    }
    }

    /*
     * Draws the UFO blob on the radar screen.
     * Currently works only for original sized blobs
     * 13 x 13 pixels.
     */
    void drawUfo()
    {
	    if (_ufoSize < 0 || _ufo.isDestroyed())
	    {
		    return;
	    }
	    int currentUfoXposition =  _battle.getWidth() / 2 - 6;
	    int currentUfoYposition = _battle.getHeight() - (_currentDist / 8) - 6;
	    for (int y = 0; y < 13; ++y)
	    {
		    for (int x = 0; x < 13; ++x)
		    {
			    byte pixelOffset = (byte)_ufoBlobs[_ufoSize + _ufo.getHitFrame(), y, x];
			    if (pixelOffset == 0)
			    {
				    continue;
			    }
			    else
			    {
				    if (_ufo.isCrashed() || _ufo.getHitFrame() > 0)
				    {
					    pixelOffset *= 2;
				    }
				    byte radarPixelColor = _window.getPixel(currentUfoXposition + x + 3, currentUfoYposition + y + 3); // + 3 cause of the window frame
				    byte color = (byte)(radarPixelColor - pixelOffset);
				    if (color < _colors[(int)ColorNames.BLOB_MIN])
				    {
					    color = (byte)_colors[(int)ColorNames.BLOB_MIN];
				    }
				    _battle.setPixel(currentUfoXposition + x, currentUfoYposition + y, color);
			    }
		    }
	    }
    }
}
