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
 * ArticleStateTextImage has a title, text block and a background image.
 */
internal class ArticleStateTextImage : ArticleState
{
	Text _txtTitle;
	Text _txtInfo;

	internal ArticleStateTextImage(ArticleDefinitionTextImage defs) : base(defs.id)
	{
		// add screen elements
		_txtTitle = new Text(defs.text_width, 48, 5, 22);

		// Set palette
		setPalette("PAL_UFOPAEDIA");

		base.initLayout();

		// add other elements
		add(_txtTitle);

		// Set up objects
		_game.getMod().getSurface(defs.image_id).blit(_bg);
		_btnOk.setColor((byte)(Palette.blockOffset(5)+3));
		_btnPrev.setColor((byte)(Palette.blockOffset(5)+3));
		_btnNext.setColor((byte)(Palette.blockOffset(5)+3));

		_txtTitle.setColor((byte)(Palette.blockOffset(15)+4));
		_txtTitle.setBig();
		_txtTitle.setWordWrap(true);
		_txtTitle.setText(tr(defs.title));

		int text_height = _txtTitle.getTextHeight();

		_txtInfo = new Text(defs.text_width, 162, 5, 23 + text_height);
		add(_txtInfo);

		_txtInfo.setColor((byte)(Palette.blockOffset(15)-1));
		_txtInfo.setWordWrap(true);
		_txtInfo.setScrollable(true);
		_txtInfo.setText(tr(defs.text));

		centerAllSurfaces();
	}

	~ArticleStateTextImage() { }
}
