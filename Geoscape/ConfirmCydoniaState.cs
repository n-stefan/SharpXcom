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
 * Screen that allows the player
 * to pick a target for a craft on the globe.
 */
internal class ConfirmCydoniaState : State
{
    Craft _craft;
    Window _window;
    TextButton _btnNo, _btnYes;
    Text _txtMessage;

    internal ConfirmCydoniaState(Craft craft)
    {
        _craft = craft;

        _screen = false;

        // Create objects
        _window = new Window(this, 256, 160, 32, 20);
        _btnYes = new TextButton(80, 20, 70, 142);
        _btnNo = new TextButton(80, 20, 170, 142);
        _txtMessage = new Text(224, 48, 48, 76);

        // Set palette
        setInterface("confirmCydonia");

        add(_window, "window", "confirmCydonia");
        add(_btnYes, "button", "confirmCydonia");
        add(_btnNo, "button", "confirmCydonia");
        add(_txtMessage, "text", "confirmCydonia");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK12.SCR"));

        _btnYes.setText(tr("STR_YES"));
        _btnYes.onMouseClick(btnYesClick);
        _btnYes.onKeyboardPress(btnYesClick, Options.keyOk);

        _btnNo.setText(tr("STR_NO"));
        _btnNo.onMouseClick(btnNoClick);
        _btnNo.onKeyboardPress(btnNoClick, Options.keyCancel);

        _txtMessage.setAlign(TextHAlign.ALIGN_CENTER);
        _txtMessage.setBig();
        _txtMessage.setWordWrap(true);
        _txtMessage.setText(tr("STR_ARE_YOU_SURE_CYDONIA"));
    }

    /**
     *
     */
    ~ConfirmCydoniaState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnYesClick(Action _)
    {
        _game.popState();
        _game.popState();

        SavedBattleGame bgame = new SavedBattleGame();
        _game.getSavedGame().setBattleGame(bgame);
        BattlescapeGenerator bgen = new BattlescapeGenerator(_game);
        foreach (var i in _game.getMod().getDeploymentsList())
        {
            AlienDeployment deployment = _game.getMod().getDeployment(i);
            if (deployment.isFinalDestination())
            {
                bgame.setMissionType(i);
                bgen.setAlienRace(deployment.getRace());
                break;
            }
        }
        bgen.setCraft(_craft);
        bgen.run();

        _game.pushState(new BriefingState(_craft));
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnNoClick(Action _) =>
        _game.popState();
}
