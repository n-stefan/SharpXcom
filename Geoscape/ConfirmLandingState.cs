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
 * along with OpenXcom.  If not, see <http:///www.gnu.org/licenses/>.
 */

namespace SharpXcom.Geoscape;

/**
 * Window that allows the player
 * to confirm a craft landing at its destination.
 */
internal class ConfirmLandingState : State
{
    Craft _craft;
    Texture _texture;
    int _shade;
    Window _window;
    TextButton _btnYes, _btnNo;
    Text _txtMessage, _txtBegin;

    /**
     * Initializes all the elements in the Confirm Landing window.
     * @param game Pointer to the core game.
     * @param craft Pointer to the craft to confirm.
     * @param texture Texture of the landing site.
     * @param shade Shade of the landing site.
     */
    internal ConfirmLandingState(Craft craft, Texture texture, int shade)
    {
        _craft = craft;
        _texture = texture;
        _shade = shade;

        _screen = false;

        // Create objects
        _window = new Window(this, 216, 160, 20, 20, WindowPopup.POPUP_BOTH);
        _btnYes = new TextButton(80, 20, 40, 150);
        _btnNo = new TextButton(80, 20, 136, 150);
        _txtMessage = new Text(206, 80, 25, 40);
        _txtBegin = new Text(206, 17, 25, 130);

        // Set palette
        setInterface("confirmLanding");

        add(_window, "window", "confirmLanding");
        add(_btnYes, "button", "confirmLanding");
        add(_btnNo, "button", "confirmLanding");
        add(_txtMessage, "text", "confirmLanding");
        add(_txtBegin, "text", "confirmLanding");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK15.SCR"));

        _btnYes.setText(tr("STR_YES"));
        _btnYes.onMouseClick(btnYesClick);
        _btnYes.onKeyboardPress(btnYesClick, Options.keyOk);

        _btnNo.setText(tr("STR_NO"));
        _btnNo.onMouseClick(btnNoClick);
        _btnNo.onKeyboardPress(btnNoClick, Options.keyCancel);

        _txtMessage.setBig();
        _txtMessage.setAlign(TextHAlign.ALIGN_CENTER);
        _txtMessage.setWordWrap(true);
        _txtMessage.setText(tr("STR_CRAFT_READY_TO_LAND_NEAR_DESTINATION")
                             .arg(_craft.getName(_game.getLanguage()))
                             .arg(_craft.getDestination().getName(_game.getLanguage())));

        _txtBegin.setBig();
        _txtBegin.setAlign(TextHAlign.ALIGN_CENTER);
        string ss = $"{Unicode.TOK_COLOR_FLIP}{tr("STR_BEGIN_MISSION")}";
        _txtBegin.setText(ss);
    }

    /**
     *
     */
    ~ConfirmLandingState() { }

    /**
     * Enters the mission.
     * @param action Pointer to an action.
     */
    void btnYesClick(Action _)
    {
        _game.popState();
        Ufo u = (Ufo)_craft.getDestination();
        MissionSite m = (MissionSite)_craft.getDestination();
        AlienBase b = (AlienBase)_craft.getDestination();

        SavedBattleGame bgame = new SavedBattleGame();
        _game.getSavedGame().setBattleGame(bgame);
        BattlescapeGenerator bgen = new BattlescapeGenerator(_game);
        bgen.setWorldTexture(_texture);
        bgen.setWorldShade(_shade);
        bgen.setCraft(_craft);
        if (u != null)
        {
            if (u.getStatus() == UfoStatus.CRASHED)
                bgame.setMissionType("STR_UFO_CRASH_RECOVERY");
            else
                bgame.setMissionType("STR_UFO_GROUND_ASSAULT");
            bgen.setUfo(u);
            bgen.setAlienRace(u.getAlienRace());
        }
        else if (m != null)
        {
            bgame.setMissionType(m.getDeployment().getType());
            bgen.setMissionSite(m);
            bgen.setAlienRace(m.getAlienRace());
        }
        else if (b != null)
        {
            bgame.setMissionType(b.getDeployment().getType());
            bgen.setAlienBase(b);
            bgen.setAlienRace(b.getAlienRace());
            bgen.setWorldTexture(null);
        }
        else
        {
            throw new Exception("No mission available!");
        }
        bgen.run();
        _game.pushState(new BriefingState(_craft));
    }

    /**
     * Returns the craft to base and closes the window.
     * @param action Pointer to an action.
     */
    void btnNoClick(Action _)
    {
        _craft.returnToBase();
        _game.popState();
    }

    /*
     * Make sure we aren't returning to base.
     */
    internal override void init()
    {
	    base.init();
	    Base b = (Base)_craft.getDestination();
	    if (b == _craft.getBase())
		    _game.popState();
    }
}
