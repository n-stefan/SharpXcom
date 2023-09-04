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
 * Briefing screen which displays info
 * about a Crash Site mission.
 */
internal class BriefingState : State
{
    Window _window;
    TextButton _btnOk;
    Text _txtTitle, _txtTarget, _txtCraft, _txtBriefing;
    string _cutsceneId, _musicId;

    /**
	 * Initializes all the elements in the Briefing screen.
	 * @param game Pointer to the core game.
	 * @param craft Pointer to the craft in the mission.
	 * @param base Pointer to the base in the mission.
	 */
    internal BriefingState(Craft craft = null, Base @base = null)
	{
		Options.baseXResolution = Options.baseXGeoscape;
		Options.baseYResolution = Options.baseYGeoscape;
		_game.getScreen().resetDisplay(false);

		_screen = true;
		// Create objects
		_window = new Window(this, 320, 200, 0, 0);
		_btnOk = new TextButton(120, 18, 100, 164);
		_txtTitle = new Text(300, 32, 16, 24);
		_txtTarget = new Text(300, 17, 16, 40);
		_txtCraft = new Text(300, 17, 16, 56);
		_txtBriefing = new Text(274, 94, 16, 72);

		string mission = _game.getSavedGame().getSavedBattle().getMissionType();
		AlienDeployment deployment = _game.getMod().getDeployment(mission);
		Ufo ufo = null;
		if (deployment == null && craft != null)
		{
			ufo = (Ufo)craft.getDestination();
			if (ufo != null) // landing site or crash site.
			{
				deployment = _game.getMod().getDeployment(ufo.getRules().getType());
			}
		}

		string title = mission;
		string desc = title + "_BRIEFING";
		if (deployment == null) // none defined - should never happen, but better safe than sorry i guess.
		{
			setPalette("PAL_GEOSCAPE", 0);
			_musicId = "GMDEFEND";
			_window.setBackground(_game.getMod().getSurface("BACK16.SCR"));
		}
		else
		{
			BriefingData data = deployment.getBriefingData();
			setPalette("PAL_GEOSCAPE", data.palette);
			_window.setBackground(_game.getMod().getSurface(data.background));
			_txtCraft.setY(56 + data.textOffset);
			_txtBriefing.setY(72 + data.textOffset);
			_txtTarget.setVisible(data.showTarget);
			_txtCraft.setVisible(data.showCraft);
			_cutsceneId = data.cutscene;
			_musicId = data.music;
			if (!string.IsNullOrEmpty(data.title))
			{
				title = data.title;
			}
			if (!string.IsNullOrEmpty(data.desc))
			{
				desc = data.desc;
			}
		}

		add(_window, "window", "briefing");
		add(_btnOk, "button", "briefing");
		add(_txtTitle, "text", "briefing");
		add(_txtTarget, "text", "briefing");
		add(_txtCraft, "text", "briefing");
		add(_txtBriefing, "text", "briefing");

		centerAllSurfaces();

		// Set up objects
		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

		_txtTitle.setBig();
		_txtTarget.setBig();
		_txtCraft.setBig();

		string s = null;
		if (craft != null)
		{
			if (craft.getDestination() != null)
			{
				_txtTarget.setText(craft.getDestination().getName(_game.getLanguage()));
			}

			s = tr("STR_CRAFT_").arg(craft.getName(_game.getLanguage()));
		}
		else if (@base != null)
		{
			s = tr("STR_BASE_UC_").arg(@base.getName());
		}
		_txtCraft.setText(s);

		_txtTitle.setText(tr(title));

		_txtBriefing.setWordWrap(true);
		_txtBriefing.setText(tr(desc));

		if (mission == "STR_BASE_DEFENSE")
		{
            // And make sure the base is unmarked.
            @base.setRetaliationTarget(false);
		}
	}

	/**
	 *
	 */
	~BriefingState() { }

    /**
     * Closes the window.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _)
    {
        _game.popState();
        Options.baseXResolution = Options.baseXBattlescape;
        Options.baseYResolution = Options.baseYBattlescape;
        _game.getScreen().resetDisplay(false);
        BattlescapeState bs = new BattlescapeState();
        int liveAliens = 0, liveSoldiers = 0;
        bs.getBattleGame().tallyUnits(out liveAliens, out liveSoldiers);
        if (liveAliens > 0)
        {
            _game.pushState(bs);
            _game.getSavedGame().getSavedBattle().setBattleState(bs);
            _game.pushState(new NextTurnState(_game.getSavedGame().getSavedBattle(), bs));
            _game.pushState(new InventoryState(false, bs));
        }
        else
        {
            Options.baseXResolution = Options.baseXGeoscape;
            Options.baseYResolution = Options.baseYGeoscape;
            _game.getScreen().resetDisplay(false);
            bs = null;
            _game.pushState(new AliensCrashState());
        }
    }

	protected override void init()
	{
		base.init();

		if (!string.IsNullOrEmpty(_cutsceneId))
		{
			_game.pushState(new CutsceneState(_cutsceneId));

			// don't play the cutscene again when we return to this state
			_cutsceneId = string.Empty;
		}
		else
		{
			_game.getMod().playMusic(_musicId);
		}
	}
}
