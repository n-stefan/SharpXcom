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
 * Mini view of a base.
 * Takes all the bases and displays their layout
 * and allows players to swap between them.
 */
internal class MiniBaseView : InteractiveSurface
{
    List<Base> _bases;
    SurfaceSet _texture;
    uint _base, _hoverBase;
    byte _red, _green;

    /**
     * Sets up a mini base view with the specified size and position.
     * @param width Width in pixels.
     * @param height Height in pixels.
     * @param x X position in pixels.
     * @param y Y position in pixels.
     */
    internal MiniBaseView(int width, int height, int x, int y) : base(width, height, x, y)
    {
        _bases = null;
        _texture = null;
        _base = 0;
        _hoverBase = 0;
        _red = 0;
        _green = 0;
    }

    /**
     *
     */
    ~MiniBaseView() { }

    /**
     * Changes the texture to use for drawing
     * the various base elements.
     * @param texture Pointer to SurfaceSet to use.
     */
    internal void setTexture(SurfaceSet texture) =>
        _texture = texture;

    /**
     * Changes the current list of bases to display.
     * @param bases Pointer to base list to display.
     */
    internal void setBases(List<Base> bases)
    {
        _bases = bases;
        _redraw = true;
    }

    /**
     * Changes the base that is currently selected on
     * the mini base view.
     * @param base ID of base.
     */
    internal void setSelectedBase(uint @base)
    {
        _base = @base;
        _redraw = true;
    }

    /**
     * Returns the base the mouse cursor is currently over.
     * @return ID of the base.
     */
    internal uint getHoveredBase() =>
	    _hoverBase;
}
