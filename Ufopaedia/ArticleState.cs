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
    ArticleState(string article_id)
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
}
