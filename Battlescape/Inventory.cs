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

/**
 * Interactive view of an inventory.
 * Lets the player view and manage a soldier's equipment.
 */
internal class Inventory : InteractiveSurface
{
    Game _game;
    BattleUnit _selUnit;
    BattleItem _selItem;
    bool _tu, _base;
    BattleItem _mouseOverItem;
    int _groundOffset, _animFrame;
    int _depth;
    Surface _grid, _items, _selection;
    WarningMessage _warning;
    NumberText _stackNumber;
    Engine.Timer _animTimer;
    List<KeyValuePair<int, int>> _grenadeIndicators;

    /**
     * Sets up an inventory with the specified size and position.
     * @param game Pointer to core game.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     * @param base Is the inventory being called from the basescape?
     */
    Inventory(Game game, int width, int height, int x, int y, bool @base) : base(width, height, x, y)
    {
        _game = game;
        _selUnit = null;
        _selItem = null;
        _tu = true;
        _base = @base;
        _mouseOverItem = null;
        _groundOffset = 0;
        _animFrame = 0;

        _depth = _game.getSavedGame().getSavedBattle().getDepth();
        _grid = new Surface(width, height, 0, 0);
        _items = new Surface(width, height, 0, 0);
        _selection = new Surface(RuleInventory.HAND_W * RuleInventory.SLOT_W, RuleInventory.HAND_H * RuleInventory.SLOT_H, x, y);
        _warning = new WarningMessage(224, 24, 48, 176);
        _stackNumber = new NumberText(15, 15, 0, 0);
        _stackNumber.setBordered(true);

        _warning.initText(_game.getMod().getFont("FONT_BIG"), _game.getMod().getFont("FONT_SMALL"), _game.getLanguage());
        _warning.setColor((byte)_game.getMod().getInterface("battlescape").getElement("warning").color2);
        _warning.setTextColor((byte)_game.getMod().getInterface("battlescape").getElement("warning").color);

        _animTimer = new Engine.Timer(125);
        _animTimer.onTimer((SurfaceHandler)drawPrimers);
        _animTimer.start();
    }

    /**
     * Deletes inventory surfaces.
     */
    ~Inventory()
    {
        _grid = null;
        _items = null;
        _selection = null;
        _warning = null;
        _stackNumber = null;
        _animTimer = null;
    }

    /**
     * Shows primer warnings on all live grenades.
     */
    void drawPrimers()
    {
        int[] Pulsate = { 0, 1, 2, 3, 4, 3, 2, 1 };
        if (_animFrame == 8)
        {
            _animFrame = 0;
        }
        Surface tempSurface = _game.getMod().getSurfaceSet("SCANG.DAT").getFrame(6);
        foreach (var i in _grenadeIndicators)
        {
            tempSurface.blitNShade(_items, i.Key, i.Value, Pulsate[_animFrame]);
        }
        _animFrame++;
    }

    /**
     * Checks if an item in a certain slot position would
     * overlap with any other inventory item.
     * @param unit Pointer to current unit.
     * @param item Pointer to battle item.
     * @param slot Inventory slot, or NULL if none.
     * @param x X position in slot.
     * @param y Y position in slot.
     * @return If there's overlap.
     */
    internal static bool overlapItems(BattleUnit unit, BattleItem item, RuleInventory slot, int x, int y)
    {
        if (slot.getType() != InventoryType.INV_GROUND)
        {
            foreach (var i in unit.getInventory())
            {
                if (i.getSlot() == slot && i.occupiesSlot(x, y, item))
                {
                    return true;
                }
            }
        }
        else if (unit.getTile() != null)
        {
            foreach (var i in unit.getTile().getInventory())
            {
                if (i.occupiesSlot(x, y, item))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
