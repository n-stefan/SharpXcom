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

namespace SharpXcom.Engine;

/**
 * A game state that receives user input and reacts accordingly.
 * Game states typically represent a whole window or screen that
 * the user interacts with, making the game... well, interactive.
 * They automatically handle child elements used to transmit
 * information from/to the user, and are linked to the core game
 * engine which manages them.
 */
internal class State
{
    /// Initializes static member
    internal static Game _game;
    protected bool _screen;
    protected InteractiveSurface _modal;
    protected RuleInterface _ruleInterface;
    protected RuleInterface _ruleInterfaceParent;
    protected SDL_Color[] _palette = new SDL_Color[256];
    protected byte _cursorColor;
    protected List<Surface> _surfaces;

    /// Sets game object pointer
    internal static void setGamePtr(Game game) =>
        _game = game;

    /**
     * Initializes a brand new state with no child elements.
     * By default states are full-screen.
     * @param game Pointer to the core game.
     */
    internal State()
    {
        _screen = true;
        _modal = null;
        _ruleInterface = null;
        _ruleInterfaceParent = null;

        _cursorColor = _game.getCursor().getColor();
    }

    /**
     * Deletes all the child elements contained in the state.
     */
    ~State() =>
        _surfaces.Clear();

    /**
     * Replaces a certain amount of colors in the state's palette.
     * @param colors Pointer to the set of colors.
     * @param firstcolor Offset of the first color to replace.
     * @param ncolors Amount of colors to replace.
     * @param immediately Apply changes immediately, otherwise wait in case of multiple setPalettes.
     */
    internal void setPalette(Span<SDL_Color> colors, int firstcolor = 0, int ncolors = 256, bool immediately = true)
    {
        if (colors != null)
        {
            colors.CopyTo(_palette.AsSpan(firstcolor, ncolors)); //memcpy(_palette + firstcolor, colors, ncolors * sizeof(SDL_Color));
        }
        if (immediately)
        {
            _game.getCursor().setPalette(_palette);
            _game.getCursor().draw();
            _game.getFpsCounter().setPalette(_palette);
            _game.getFpsCounter().draw();
            if (_game.getMod() != null)
            {
                _game.getMod().setPalette(_palette);
            }
        }
    }

    /**
     * Loads palettes from the game resources into the state.
     * @param palette String ID of the palette to load.
     * @param backpals BACKPALS.DAT offset to use.
     */
    internal void setPalette(string palette, int backpals)
    {
	    setPalette(_game.getMod().getPalette(palette).getColors(), 0, 256, false);
	    if (palette == "PAL_GEOSCAPE")
	    {
		    _cursorColor = (byte)Mod.Mod.GEOSCAPE_CURSOR;
	    }
	    else if (palette == "PAL_BASESCAPE")
	    {
            _cursorColor = (byte)Mod.Mod.BASESCAPE_CURSOR;
	    }
	    else if (palette == "PAL_UFOPAEDIA")
	    {
		    _cursorColor = (byte)Mod.Mod.UFOPAEDIA_CURSOR;
	    }
	    else if (palette == "PAL_GRAPHS")
	    {
		    _cursorColor = (byte)Mod.Mod.GRAPHS_CURSOR;
	    }
	    else
	    {
		    _cursorColor = (byte)Mod.Mod.BATTLESCAPE_CURSOR;
	    }
	    if (backpals != -1)
		    setPalette(_game.getMod().getPalette("BACKPALS.DAT").getColors(Palette.blockOffset((byte)backpals)), Palette.backPos, 16, false);
        setPalette(null); // delay actual update to the end
    }

    /**
     * Adds a new child surface for the state to take care of,
     * giving it the game's display palette. Once associated,
     * the state handles all of the surface's behaviour
     * and management automatically.
     * @param surface Child surface.
     * @note Since visible elements can overlap one another,
     * they have to be added in ascending Z-Order to be blitted
     * correctly onto the screen.
     */
    internal void add(Surface surface)
    {
        // Set palette
        surface.setPalette(_palette);

        // Set default text resources
        if (_game.getLanguage() != null && _game.getMod() != null)
            surface.initText(_game.getMod().getFont("FONT_BIG"), _game.getMod().getFont("FONT_SMALL"), _game.getLanguage());

        _surfaces.Add(surface);
    }

    /**
     * Returns whether this is a full-screen state.
     * This is used to optimize the state machine since full-screen
     * states automatically cover the whole screen, (whether they
     * actually use it all or not) so states behind them can be
     * safely ignored since they'd be covered up.
     * @return True if it's a screen, False otherwise.
     */
    internal bool isScreen() =>
	    _screen;

    /**
     * Toggles the full-screen flag. Used by windows to
     * keep the previous screen in display while the window
     * is still "popping up".
     */
    internal void toggleScreen() =>
        _screen = !_screen;

    /**
     * Shows all the hidden Surface child elements.
     */
    internal void showAll()
    {
        foreach (var surface in _surfaces)
            surface.setHidden(false);
    }

    /**
     * Get the localized text for dictionary key @a id.
     * This function forwards the call to Language.getString(string).
     * @param id The dictionary key to search for.
     * @return The localized text.
     */
    protected LocalizedText tr(string id) =>
	    _game.getLanguage().getString(id);

    /**
     * Takes care of any events from the core game engine,
     * and passes them on to its InteractiveSurface child elements.
     * @param action Pointer to an action.
     */
    internal virtual void handle(Action action)
    {
	    if (_modal == null)
	    {
		    for (var i = _surfaces.Count - 1; i >= 0; i--)
		    {
			    var j = (InteractiveSurface)_surfaces[i];
                if (j != null)
                    j.handle(action, this);
		    }
	    }
	    else
	    {
            _modal.handle(action, this);
	    }
    }

    /**
     * Each state will probably need its own resize handling,
     * so this space intentionally left blank
     * @param dX delta of X;
     * @param dY delta of Y;
     */
    internal void resize(ref int dX, ref int dY) =>
        recenter(dX, dY);

    /**
     * Initializes the state and its child elements. This is
     * used for settings that have to be reset every time the
     * state is returned to focus (eg. palettes), so can't
     * just be put in the constructor (remember there's a stack
     * of states, so they can be created once while being
     * repeatedly switched back into focus).
     */
    internal virtual void init()
    {
        _game.getScreen().setPalette(_palette);
        _game.getCursor().setPalette(_palette);
        _game.getCursor().setColor(_cursorColor);
        _game.getCursor().draw();
        _game.getFpsCounter().setPalette(_palette);
        _game.getFpsCounter().setColor(_cursorColor);
        _game.getFpsCounter().draw();
        if (_game.getMod() != null)
        {
            _game.getMod().setPalette(_palette);
        }
        foreach (var surface in _surfaces)
        {
            Window window = (Window)surface;
            if (window != null)
            {
                window.invalidate();
            }
        }
        if (_ruleInterface != null && !string.IsNullOrEmpty(_ruleInterface.getMusic()))
        {
            _game.getMod().playMusic(_ruleInterface.getMusic());
        }
    }

    /**
     * Resets the status of all the Surface child elements,
     * like unpressing buttons.
     */
    internal void resetAll()
    {
        foreach (var surface in _surfaces)
        {
            InteractiveSurface s = (InteractiveSurface)surface;
            if (s != null)
            {
                s.unpress(this);
                //s.setFocus(false);
            }
        }
    }

    /**
     * redraw all the text-type surfaces.
     */
    internal void redrawText()
    {
        foreach (var surface in _surfaces)
        {
            Text text = (Text)surface;
            TextButton button = (TextButton)surface;
            TextEdit edit = (TextEdit)surface;
            TextList list = (TextList)surface;
            if (text != null || button != null || edit != null || list != null)
            {
                surface.draw();
            }
        }
    }

    /**
     * Re-orients all the surfaces in the state.
     * @param dX delta of X;
     * @param dY delta of Y;
     */
    void recenter(int dX, int dY)
    {
        foreach (var surface in _surfaces)
        {
            surface.setX(surface.getX() + dX / 2);
            surface.setY(surface.getY() + dY / 2);
        }
    }

    /**
     * Runs any code the state needs to keep updating every
     * game cycle, like timers and other real-time elements.
     */
    internal virtual void think()
    {
        foreach (var surface in _surfaces)
            surface.think();
    }

    /**
     * Blits all the visible Surface child elements onto the
     * display screen, by order of addition.
     */
    internal virtual void blit()
    {
        foreach (var surface in _surfaces)
            surface.blit(_game.getScreen().getSurface());
    }

    /**
     * Changes the current modal surface. If a surface is modal,
     * then only that surface can receive events. This is used
     * when an element needs to take priority over everything else,
     * eg. focus.
     * @param surface Pointer to modal surface, NULL for no modal.
     */
    internal void setModal(InteractiveSurface surface) =>
        _modal = surface;

    /**
     * centers all the surfaces on the screen.
     */
    protected void centerAllSurfaces()
    {
        foreach (var surface in _surfaces)
        {
            surface.setX(surface.getX() + _game.getScreen().getDX());
            surface.setY(surface.getY() + _game.getScreen().getDY());
        }
    }
}
