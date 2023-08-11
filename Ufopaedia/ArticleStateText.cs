﻿/*
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
 * ArticleStateText has only a caption and a text.
 */
internal class ArticleStateText : ArticleState
{
	Text _txtTitle;
	Text _txtInfo;

	internal ArticleStateText(ArticleDefinitionText defs) : base(defs.id)
	{
		// add screen elements
		_txtTitle = new Text(296, 17, 5, 23);
		_txtInfo = new Text(296, 150, 10, 48);

		// Set palette
		setPalette("PAL_UFOPAEDIA");

		base.initLayout();

		// add other elements
		add(_txtTitle);
		add(_txtInfo);

		centerAllSurfaces();

		// Set up objects
		_game.getMod().getSurface("BACK10.SCR").blit(_bg);
		_btnOk.setColor(Palette.blockOffset(5));
		_btnPrev.setColor(Palette.blockOffset(5));
		_btnNext.setColor(Palette.blockOffset(5));

		_txtTitle.setColor((byte)(Palette.blockOffset(15)+4));
		_txtTitle.setBig();
		_txtTitle.setText(tr(defs.title));

		_txtInfo.setColor((byte)(Palette.blockOffset(15)-1));
		_txtInfo.setWordWrap(true);
		_txtInfo.setScrollable(true);
		_txtInfo.setText(tr(defs.text));
	}

	~ArticleStateText() { }
}
