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
	internal const uint MAX_BASES = 8;
	const int MINI_SIZE = 14;

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

    /**
     * Draws the view of all the bases with facilities
     * in varying colors.
     */
    internal override void draw()
    {
	    base.draw();
	    for (int i = 0; i < MAX_BASES; ++i)
	    {
		    // Draw base squares
		    if (i == _base)
		    {
			    SDL_Rect r;
			    r.x = i * (MINI_SIZE + 2);
			    r.y = 0;
			    r.w = MINI_SIZE + 2;
			    r.h = MINI_SIZE + 2;
			    drawRect(ref r, 1);
		    }
		    _texture.getFrame(41).setX(i * (MINI_SIZE + 2));
		    _texture.getFrame(41).setY(0);
		    _texture.getFrame(41).blit(this);

		    // Draw facilities
		    if (i < _bases.Count)
		    {
			    SDL_Rect r;
			    @lock();
			    foreach (var f in _bases[i].getFacilities())
			    {
				    int color;
				    if (f.getBuildTime() == 0)
					    color = _green;
				    else
					    color = _red;

				    r.x = i * (MINI_SIZE + 2) + 2 + f.getX() * 2;
				    r.y = 2 + f.getY() * 2;
				    r.w = f.getRules().getSize() * 2;
				    r.h = f.getRules().getSize() * 2;
				    drawRect(ref r, (byte)(color + 3));
				    r.x++;
				    r.y++;
				    r.w--;
				    r.h--;
				    drawRect(ref r, (byte)(color + 5));
				    r.x--;
				    r.y--;
				    drawRect(ref r, (byte)(color + 2));
				    r.x++;
				    r.y++;
				    r.w--;
				    r.h--;
				    drawRect(ref r, (byte)(color + 3));
				    r.x--;
				    r.y--;
				    setPixel(r.x, r.y, (byte)(color + 1));
			    }
			    unlock();
		    }
	    }
    }

    /**
     * Selects the base the mouse is over.
     * @param action Pointer to an action.
     * @param state State that the action handlers belong to.
     */
    protected override void mouseOver(Action action, State state)
    {
	    _hoverBase = (uint)Math.Floor(action.getRelativeXMouse() / ((MINI_SIZE + 2) * action.getXScale()));
	    base.mouseOver(action, state);
    }

    internal override void setColor(byte color) =>
	    _green = color;

    internal override void setSecondaryColor(byte color) =>
	    _red = color;
}
