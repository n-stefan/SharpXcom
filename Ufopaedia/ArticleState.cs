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

namespace SharpXcom.Ufopaedia;

/**
 * UfopaediaArticle is the base class for all articles of various types.
 *
 * It encapsulates the basic characteristics.
 */
internal class ArticleState : State
{
    /// the article id
    protected string _id;
    /// screen elements common to all articles!
    protected Surface _bg;
    protected TextButton _btnOk;
    protected TextButton _btnPrev;
    protected TextButton _btnNext;

    /**
	 * Constructor
	 * @param game Pointer to current game.
	 * @param article_id The article id of this article state instance.
	 */
    protected ArticleState(string article_id)
    {
        _id = article_id;

        // init background and navigation elements
        _bg = new Surface(320, 200, 0, 0);
        _btnOk = new TextButton(30, 14, 5, 5);
        _btnPrev = new TextButton(30, 14, 40, 5);
        _btnNext = new TextButton(30, 14, 75, 5);
    }

    /**
	 * Destructor
	 */
    ~ArticleState() { }

	/**
	 * Set captions and click handlers for the common control elements.
	 */
	protected void initLayout()
	{
		add(_bg);
		add(_btnOk);
		add(_btnPrev);
		add(_btnNext);

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);
		_btnPrev.setText("<<");
		_btnPrev.onMouseClick(btnPrevClick);
		_btnPrev.onKeyboardPress(btnPrevClick, Options.keyGeoLeft);
		_btnNext.setText(">>");
		_btnNext.onMouseClick(btnNextClick);
		_btnNext.onKeyboardPress(btnNextClick, Options.keyGeoRight);
	}

	/**
	 * Returns to the previous screen.
	 * @param action Pointer to an action.
	 */
	void btnOkClick(Action _) =>
		_game.popState();

	/**
	 * Shows the previous available article.
	 * @param action Pointer to an action.
	 */
	void btnPrevClick(Action _) =>
		Ufopaedia.prev(_game);

	/**
	 * Shows the next available article. Loops to the first.
	 * @param action Pointer to an action.
	 */
	void btnNextClick(Action _) =>
		Ufopaedia.next(_game);

	protected string getDamageTypeText(ItemDamageType dt)
	{
		string type;
		switch (dt)
		{
			case ItemDamageType.DT_AP:
				type = "STR_DAMAGE_ARMOR_PIERCING";
				break;
			case ItemDamageType.DT_IN:
				type = "STR_DAMAGE_INCENDIARY";
				break;
			case ItemDamageType.DT_HE:
				type = "STR_DAMAGE_HIGH_EXPLOSIVE";
				break;
			case ItemDamageType.DT_LASER:
				type = "STR_DAMAGE_LASER_BEAM";
				break;
			case ItemDamageType.DT_PLASMA:
				type = "STR_DAMAGE_PLASMA_BEAM";
				break;
			case ItemDamageType.DT_STUN:
				type = "STR_DAMAGE_STUN";
				break;
			case ItemDamageType.DT_MELEE:
				type = "STR_DAMAGE_MELEE";
				break;
			case ItemDamageType.DT_ACID:
				type = "STR_DAMAGE_ACID";
				break;
			case ItemDamageType.DT_SMOKE:
				type = "STR_DAMAGE_SMOKE";
				break;
			default:
				type = "STR_UNKNOWN";
				break;
		}
		return type;
	}

	/// return the article id
	string getId() =>
		_id;
}
