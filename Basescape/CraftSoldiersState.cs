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

struct SortFunctor : IComparer<Soldier>
{
    Game _game;
    Func<Game, Soldier, int> _getStatFn;

    internal SortFunctor(Game game, Func<Game, Soldier, int> getStatFn) =>
		(_game, _getStatFn) = (game, getStatFn);

	public int Compare(Soldier a, Soldier b) =>
		_getStatFn(_game, a) - _getStatFn(_game, b);
}

/**
 * Select Squad screen that lets the player
 * pick the soldiers to assign to a craft.
 */
internal class CraftSoldiersState : State
{
	Base _base;
	uint _craft;
	byte _otherCraftColor;
	List<Soldier> _origSoldierOrder;
	Window _window;
	TextButton _btnOk;
	Text _txtTitle, _txtName, _txtRank, _txtCraft, _txtAvailable, _txtUsed;
	ComboBox _cbxSortBy;
	TextList _lstSoldiers;
	List<SortFunctor?> _sortFunctors;

	/**
	 * Initializes all the elements in the Craft Soldiers screen.
	 * @param game Pointer to the core game.
	 * @param base Pointer to the base to get info from.
	 * @param craft ID of the selected craft.
	 */
	internal CraftSoldiersState(Base @base, uint craft)
	{
		_base = @base;
		_craft = craft;
		_otherCraftColor = 0;
		_origSoldierOrder = _base.getSoldiers();

		// Create objects
		_window = new Window(this, 320, 200, 0, 0);
		_btnOk = new TextButton(148, 16, 164, 176);
		_txtTitle = new Text(300, 17, 16, 7);
		_txtName = new Text(114, 9, 16, 32);
		_txtRank = new Text(102, 9, 122, 32);
		_txtCraft = new Text(84, 9, 224, 32);
		_txtAvailable = new Text(110, 9, 16, 24);
		_txtUsed = new Text(110, 9, 122, 24);
		_cbxSortBy = new ComboBox(this, 148, 16, 8, 176, true);
		_lstSoldiers = new TextList(288, 128, 8, 40);

		// Set palette
		setInterface("craftSoldiers");

		add(_window, "window", "craftSoldiers");
		add(_btnOk, "button", "craftSoldiers");
		add(_txtTitle, "text", "craftSoldiers");
		add(_txtName, "text", "craftSoldiers");
		add(_txtRank, "text", "craftSoldiers");
		add(_txtCraft, "text", "craftSoldiers");
		add(_txtAvailable, "text", "craftSoldiers");
		add(_txtUsed, "text", "craftSoldiers");
		add(_lstSoldiers, "list", "craftSoldiers");
		add(_cbxSortBy, "button", "craftSoldiers");

		_otherCraftColor = (byte)_game.getMod().getInterface("craftSoldiers").getElement("otherCraft").color;

		centerAllSurfaces();

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK02.SCR"));

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

		_txtTitle.setBig();
		Craft c = _base.getCrafts()[(int)_craft];
		_txtTitle.setText(tr("STR_SELECT_SQUAD_FOR_CRAFT").arg(c.getName(_game.getLanguage())));

		_txtName.setText(tr("STR_NAME_UC"));

		_txtRank.setText(tr("STR_RANK"));

		_txtCraft.setText(tr("STR_CRAFT"));

        // populate sort options
        var sortOptions = new List<string>
        {
            tr("STR_ORIGINAL_ORDER")
        };
		_sortFunctors.Add(null);

		Push_In("STR_RANK", (Game _, Soldier s) => (int)s.getRank());
		Push_In("STR_MISSIONS2", (Game _, Soldier s) => s.getMissions());
		Push_In("STR_KILLS2", (Game _, Soldier s) => s.getKills());
		Push_In("STR_WOUND_RECOVERY2", (Game _, Soldier s) => s.getWoundRecovery());
		Push_In("STR_TIME_UNITS", (Game _, Soldier s) => s.getCurrentStats().tu);
		Push_In("STR_STAMINA", (Game _, Soldier s) => s.getCurrentStats().stamina);
		Push_In("STR_HEALTH", (Game _, Soldier s) => s.getCurrentStats().health);
		Push_In("STR_BRAVERY", (Game _, Soldier s) => s.getCurrentStats().bravery);
		Push_In("STR_REACTIONS", (Game _, Soldier s) => s.getCurrentStats().reactions);
		Push_In("STR_FIRING_ACCURACY", (Game _, Soldier s) => s.getCurrentStats().firing);
		Push_In("STR_THROWING_ACCURACY", (Game _, Soldier s) => s.getCurrentStats().throwing);
		Push_In("STR_STRENGTH", (Game _, Soldier s) => s.getCurrentStats().strength);

		// don't show psionic sort options until they actually have data they can use
		bool showPsiStrength = Options.psiStrengthEval
				&& _game.getSavedGame().isResearched(_game.getMod().getPsiRequirements());
		bool showPsiSkill = false;
		foreach (var i in _base.getSoldiers())
		{
			if (i.getCurrentStats().psiSkill > 0)
			{
				showPsiStrength = true;
				showPsiSkill = true;
				break;
			}
		}
		if (showPsiStrength)
		{
			Push_In("STR_PSIONIC_STRENGTH", (Game g, Soldier s) =>
			{
				// don't reveal (relative) psi strength before it would otherwise be known
				if (s.getCurrentStats().psiSkill > 0
				 || (Options.psiStrengthEval
					 && g.getSavedGame().isResearched(g.getMod().getPsiRequirements())))
				{
					return s.getCurrentStats().psiStrength;
				}
				return 0;
			});
		}
		if (showPsiSkill)
		{
			Push_In("STR_PSIONIC_SKILL", (Game _, Soldier s) =>
				// protect against negative psiSkill (possible when Options.anytimePsiTraining
				// is enabled)
				(s.getCurrentStats().psiSkill > 0) ? s.getCurrentStats().psiSkill : 0
			);
		}

		Push_In("STR_MELEE_ACCURACY", (Game _, Soldier s) => s.getCurrentStats().melee);

		_cbxSortBy.setOptions(sortOptions);
		_cbxSortBy.setSelected(0);
		_cbxSortBy.onChange(cbxSortByChange);
		_cbxSortBy.setText(tr("STR_SORT_BY"));

		_lstSoldiers.setArrowColumn(192, ArrowOrientation.ARROW_VERTICAL);
		_lstSoldiers.setColumns(3, 106, 102, 72);
		_lstSoldiers.setSelectable(true);
		_lstSoldiers.setBackground(_window);
		_lstSoldiers.setMargin(8);
		_lstSoldiers.onLeftArrowClick(lstItemsLeftArrowClick);
		_lstSoldiers.onRightArrowClick(lstItemsRightArrowClick);
		_lstSoldiers.onMouseClick(lstSoldiersClick, 0);
		_lstSoldiers.onMousePress(lstSoldiersMousePress);

		void Push_In(string strId, Func<Game, Soldier, int> functor)
		{
			sortOptions.Add(tr(strId));
			_sortFunctors.Add(new SortFunctor(_game, functor));
		}
	}

	/**
	 * cleans up dynamic state
	 */
	~CraftSoldiersState() =>
		_sortFunctors.Clear();

	/**
	 * Returns to the previous screen.
	 * @param action Pointer to an action.
	 */
	void btnOkClick(Action _) =>
		_game.popState();

	/**
	 * Reorders a soldier up.
	 * @param action Pointer to an action.
	 */
	void lstItemsLeftArrowClick(Action action)
	{
		uint row = _lstSoldiers.getSelectedRow();
		if (row > 0)
		{
			if (action.getDetails().button.button == SDL_BUTTON_LEFT)
			{
				moveSoldierUp(action, row);
			}
			else if (action.getDetails().button.button == SDL_BUTTON_RIGHT)
			{
				moveSoldierUp(action, row, true);
			}
		}
		_cbxSortBy.setText(tr("STR_SORT_BY"));
		unchecked { _cbxSortBy.setSelected((uint)-1); }
	}

	/**
	 * Reorders a soldier down.
	 * @param action Pointer to an action.
	 */
	void lstItemsRightArrowClick(Action action)
	{
		uint row = _lstSoldiers.getSelectedRow();
		int numSoldiers = _base.getSoldiers().Count;
		if (0 < numSoldiers && int.MaxValue >= numSoldiers && row < numSoldiers - 1)
		{
			if (action.getDetails().button.button == SDL_BUTTON_LEFT)
			{
				moveSoldierDown(action, row);
			}
			else if (action.getDetails().button.button == SDL_BUTTON_RIGHT)
			{
				moveSoldierDown(action, row, true);
			}
		}
		_cbxSortBy.setText(tr("STR_SORT_BY"));
		unchecked { _cbxSortBy.setSelected((uint)-1); }
	}

	/**
	 * Moves a soldier up on the list.
	 * @param action Pointer to an action.
	 * @param row Selected soldier row.
	 * @param max Move the soldier to the top?
	 */
	void moveSoldierUp(Action action, uint row, bool max = false)
	{
		Soldier s = _base.getSoldiers()[(int)row];
		if (max)
		{
			_base.getSoldiers().RemoveAt((int)row);
			_base.getSoldiers().Insert(0, s);
		}
		else
		{
			_base.getSoldiers()[(int)row] = _base.getSoldiers()[(int)(row - 1)];
			_base.getSoldiers()[(int)(row - 1)] = s;
			if (row != _lstSoldiers.getScroll())
			{
				SDL_WarpMouseGlobal(action.getLeftBlackBand() + action.getXMouse(), action.getTopBlackBand() + action.getYMouse() - (ushort)(8 * action.getYScale()));
			}
			else
			{
				_lstSoldiers.scrollUp(false);
			}
		}
		initList();
	}

	/**
	 * Moves a soldier down on the list.
	 * @param action Pointer to an action.
	 * @param row Selected soldier row.
	 * @param max Move the soldier to the bottom?
	 */
	void moveSoldierDown(Action action, uint row, bool max = false)
	{
		Soldier s = _base.getSoldiers()[(int)row];
		if (max)
		{
			_base.getSoldiers().RemoveAt((int)row);
			_base.getSoldiers().Insert(_base.getSoldiers().Count - 1, s);
		}
		else
		{
			_base.getSoldiers()[(int)row] = _base.getSoldiers()[(int)(row + 1)];
			_base.getSoldiers()[(int)(row + 1)] = s;
			if (row != _lstSoldiers.getVisibleRows() - 1 + _lstSoldiers.getScroll())
			{
				SDL_WarpMouseGlobal(action.getLeftBlackBand() + action.getXMouse(), action.getTopBlackBand() + action.getYMouse() + (ushort)(8 * action.getYScale()));
			}
			else
			{
				_lstSoldiers.scrollDown(false);
			}
		}
		initList();
	}

	/**
	 * Sorts the soldiers list by the selected criterion
	 * @param action Pointer to an action.
	 */
	void cbxSortByChange(Action _)
	{
		uint selIdx = _cbxSortBy.getSelected();
		unchecked
		{
			if (selIdx == (uint)-1)
			{
				return;
			}
		}

		SortFunctor? compFunc = _sortFunctors[(int)selIdx];
		if (compFunc.HasValue)
		{
			_base.getSoldiers().Sort(compFunc.Value);
		}
		else
		{
			// restore original ordering, ignoring (of course) those
			// soldiers that have been sacked since this state started
			foreach (var it in _origSoldierOrder)
			{
				Soldier soldierIt = _base.getSoldiers().Find(x => x == it);
				if (soldierIt != null)
				{
					Soldier s = soldierIt;
					_base.getSoldiers().Remove(soldierIt);
					_base.getSoldiers().Insert(_base.getSoldiers().Count - 1, s);
				}
			}
		}

		initList();
	}

	/**
	 * Shows the selected soldier's info.
	 * @param action Pointer to an action.
	 */
	void lstSoldiersClick(Action action)
	{
		double mx = action.getAbsoluteXMouse();
		if (mx >= _lstSoldiers.getArrowsLeftEdge() && mx < _lstSoldiers.getArrowsRightEdge())
		{
			return;
		}
		uint row = _lstSoldiers.getSelectedRow();
		if (action.getDetails().button.button == SDL_BUTTON_LEFT)
		{
			Craft c = _base.getCrafts()[(int)_craft];
			Soldier s = _base.getSoldiers()[(int)_lstSoldiers.getSelectedRow()];
			byte color = _lstSoldiers.getColor();
			if (s.getCraft() == c)
			{
				s.setCraft(null);
				_lstSoldiers.setCellText(row, 2, tr("STR_NONE_UC"));
			}
			else if (s.getCraft() != null && s.getCraft().getStatus() == "STR_OUT")
			{
				color = _otherCraftColor;
			}
			else if (c.getSpaceAvailable() > 0 && s.getWoundRecovery() == 0)
			{
				s.setCraft(c);
				_lstSoldiers.setCellText(row, 2, c.getName(_game.getLanguage()));
				color = _lstSoldiers.getSecondaryColor();
			}
			_lstSoldiers.setRowColor(row, color);

			_txtAvailable.setText(tr("STR_SPACE_AVAILABLE").arg(c.getSpaceAvailable()));
			_txtUsed.setText(tr("STR_SPACE_USED").arg(c.getSpaceUsed()));
		}
		else if (action.getDetails().button.button == SDL_BUTTON_RIGHT)
		{
			_game.pushState(new SoldierInfoState(_base, row));
		}
	}

	/**
	 * Handles the mouse-wheels on the arrow-buttons.
	 * @param action Pointer to an action.
	 */
	void lstSoldiersMousePress(Action action)
	{
		if (Options.changeValueByMouseWheel == 0)
			return;
		uint row = _lstSoldiers.getSelectedRow();
		int numSoldiers = _base.getSoldiers().Count;
		if (action.getDetails().wheel.y > 0 && //button.button == SDL_BUTTON_WHEELUP
			row > 0)
		{
			if (action.getAbsoluteXMouse() >= _lstSoldiers.getArrowsLeftEdge() &&
				action.getAbsoluteXMouse() <= _lstSoldiers.getArrowsRightEdge())
			{
				moveSoldierUp(action, row);
			}
		}
		else if (action.getDetails().wheel.y < 0 && //button.button == SDL_BUTTON_WHEELDOWN
				 0 < numSoldiers && int.MaxValue >= numSoldiers && row < numSoldiers - 1)
		{
			if (action.getAbsoluteXMouse() >= _lstSoldiers.getArrowsLeftEdge() &&
				action.getAbsoluteXMouse() <= _lstSoldiers.getArrowsRightEdge())
			{
				moveSoldierDown(action, row);
			}
		}
	}

	/**
	 * Shows the soldiers in a list
	 */
	void initList()
	{
		uint originalScrollPos = _lstSoldiers.getScroll();
		uint row = 0;
		_lstSoldiers.clearList();
		Craft c = _base.getCrafts()[(int)_craft];
		foreach (var i in _base.getSoldiers())
		{
			_lstSoldiers.addRow(3, i.getName(true, 19), tr(i.getRankString()), i.getCraftString(_game.getLanguage()));

			byte color;
			if (i.getCraft() == c)
			{
				color = _lstSoldiers.getSecondaryColor();
			}
			else if (i.getCraft() != null)
			{
				color = _otherCraftColor;
			}
			else
			{
				color = _lstSoldiers.getColor();
			}
			_lstSoldiers.setRowColor(row, color);
			row++;
		}

		_lstSoldiers.draw();
		_lstSoldiers.scrollTo(originalScrollPos);

		_txtAvailable.setText(tr("STR_SPACE_AVAILABLE").arg(c.getSpaceAvailable()));
		_txtUsed.setText(tr("STR_SPACE_USED").arg(c.getSpaceUsed()));
	}
}
