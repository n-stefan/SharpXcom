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

enum BaseDefenseActionType { BDA_NONE, BDA_FIRE, BDA_RESOLVE, BDA_DESTROY, BDA_END };

/**
 * Base Defense Screen for when ufos try to attack.
 */
internal class BaseDefenseState : State
{
    GeoscapeState _state;
    Base _base;
    BaseDefenseActionType _action;
    int _thinkcycles, _row, _passes, _gravShields, _defenses, _attacks, _explosionCount;
    Ufo _ufo;
    Window _window;
    Text _txtTitle, _txtInit;
    TextList _lstDefenses;
    TextButton _btnOk;
    Timer _timer;

    /**
     * Initializes all the elements in the Base Defense screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base being attacked.
     * @param ufo Pointer to the attacking ufo.
     * @param state Pointer to the Geoscape.
     */
    internal BaseDefenseState(Base @base, Ufo ufo, GeoscapeState state)
    {
        _state = state;

        _base = @base;
        _action = BaseDefenseActionType.BDA_NONE;
        _row = -1;
        _passes = 0;
        _attacks = 0;
        _thinkcycles = 0;
        _ufo = ufo;
        // Create objects
        _window = new Window(this, 320, 200, 0, 0);
        _txtTitle = new Text(300, 17, 16, 6);
        _txtInit = new Text(300, 10, 16, 24);
        _lstDefenses = new TextList(300, 128, 16, 40);
        _btnOk = new TextButton(120, 18, 100, 170);

        // Set palette
        setInterface("baseDefense");

        add(_window, "window", "baseDefense");
        add(_btnOk, "button", "baseDefense");
        add(_txtTitle, "text", "baseDefense");
        add(_txtInit, "text", "baseDefense");
        add(_lstDefenses, "text", "baseDefense");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK04.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);
        _btnOk.setVisible(false);

        _txtTitle.setBig();
        _txtTitle.setText(tr("STR_BASE_UNDER_ATTACK").arg(_base.getName()));
        _txtInit.setVisible(false);

        _txtInit.setText(tr("STR_BASE_DEFENSES_INITIATED"));

        _lstDefenses.setColumns(3, 134, 70, 50);
        _gravShields = _base.getGravShields();
        _defenses = _base.getDefenses().Count;
        _timer = new Timer(250);
        _timer.onTimer((StateHandler)nextStep);
        _timer.start();

        _explosionCount = 0;
    }

    /**
     *
     */
    ~BaseDefenseState() =>
        _timer = null;

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        _timer.stop();
        _game.popState();
        if (_ufo.getStatus() != UfoStatus.DESTROYED)
        {
            _state.handleBaseDefense(_base, _ufo);
        }
        else
        {
            _base.cleanupDefenses(true);
        }
    }

    void nextStep()
    {
        if (_thinkcycles == -1)
            return;

        ++_thinkcycles;

        if (_thinkcycles == 1)
        {
            _txtInit.setVisible(true);
            return;
        }

        if (_thinkcycles > 1)
        {
            switch (_action)
            {
                case BaseDefenseActionType.BDA_DESTROY:
                    if (_explosionCount == 0)
                    {
                        _lstDefenses.addRow(2, tr("STR_UFO_DESTROYED"), " ", " ");
                        ++_row;
                        if (_row > 14)
                        {
                            _lstDefenses.scrollDown(true);
                        }
                    }
                    _game.getMod().getSound("GEO.CAT", (uint)Mod.Mod.UFO_EXPLODE).play();
                    if (++_explosionCount == 3)
                    {
                        _action = BaseDefenseActionType.BDA_END;
                    }
                    return;
                case BaseDefenseActionType.BDA_END:
                    _btnOk.setVisible(true);
                    _thinkcycles = -1;
                    return;
                default:
                    break;
            }
            if (_attacks == _defenses && _passes == _gravShields)
            {
                _action = BaseDefenseActionType.BDA_END;
                return;
            }
            else if (_attacks == _defenses && _passes < _gravShields)
            {
                _lstDefenses.addRow(3, tr("STR_GRAV_SHIELD_REPELS_UFO"), " ", " ");
                if (_row > 14)
                {
                    _lstDefenses.scrollDown(true);
                }
                ++_row;
                ++_passes;
                _attacks = 0;
                return;
            }

            BaseFacility def = _base.getDefenses()[_attacks];

            switch (_action)
            {
                case BaseDefenseActionType.BDA_NONE:
                    _lstDefenses.addRow(3, tr((def).getRules().getType()), " ", " ");
                    ++_row;
                    _action = BaseDefenseActionType.BDA_FIRE;
                    if (_row > 14)
                    {
                        _lstDefenses.scrollDown(true);
                    }
                    return;
                case BaseDefenseActionType.BDA_FIRE:
                    _lstDefenses.setCellText((uint)_row, 1, tr("STR_FIRING"));
                    _game.getMod().getSound("GEO.CAT", (uint)(def).getRules().getFireSound()).play();
                    _timer.setInterval(333);
                    _action = BaseDefenseActionType.BDA_RESOLVE;
                    return;
                case BaseDefenseActionType.BDA_RESOLVE:
                    if (!RNG.percent((def).getRules().getHitRatio()))
                    {
                        _lstDefenses.setCellText((uint)_row, 2, tr("STR_MISSED"));
                    }
                    else
                    {
                        _lstDefenses.setCellText((uint)_row, 2, tr("STR_HIT"));
                        _game.getMod().getSound("GEO.CAT", (uint)(def).getRules().getHitSound()).play();
                        int dmg = (def).getRules().getDefenseValue();
                        _ufo.setDamage(_ufo.getDamage() + (dmg / 2 + RNG.generate(0, dmg)));
                    }
                    if (_ufo.getStatus() == UfoStatus.DESTROYED)
                        _action = BaseDefenseActionType.BDA_DESTROY;
                    else
                        _action = BaseDefenseActionType.BDA_NONE;
                    ++_attacks;
                    _timer.setInterval(250);
                    return;
                default:
                    break;
            }
        }
    }
}
