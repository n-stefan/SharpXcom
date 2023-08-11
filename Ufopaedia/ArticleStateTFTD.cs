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
 * Every TFTD article has a title, text block and a background image, with little to no variation.
 */
internal class ArticleStateTFTD : ArticleState
{
	protected Text _txtInfo;
	Text _txtTitle;

	internal ArticleStateTFTD(ArticleDefinitionTFTD defs) : base(defs.id)
	{
		// Set palette
		setPalette("PAL_BASESCAPE");

		_btnOk.setX(227);
		_btnOk.setY(179);
		_btnOk.setHeight(10);
		_btnOk.setWidth(23);
		_btnOk.setColor((byte)(Palette.blockOffset(0)+2));
		_btnPrev.setX(254);
		_btnPrev.setY(179);
		_btnPrev.setHeight(10);
		_btnPrev.setWidth(23);
		_btnPrev.setColor((byte)(Palette.blockOffset(0)+2));
		_btnNext.setX(281);
		_btnNext.setY(179);
		_btnNext.setHeight(10);
		_btnNext.setWidth(23);
		_btnNext.setColor((byte)(Palette.blockOffset(0)+2));

		base.initLayout();

		_game.getMod().getSurface("BACK08.SCR").blit(_bg);
		_game.getMod().getSurface(defs.image_id).blit(_bg);

		_txtInfo = new Text(defs.text_width, 136, 320 - defs.text_width, 34);
		_txtTitle = new Text(284, 16, 36, 14);

		add(_txtTitle);
		add(_txtInfo);

		_txtTitle.setColor((byte)(Palette.blockOffset(0)+2));
		_txtTitle.setBig();
		_txtTitle.setWordWrap(true);
		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setText(tr(defs.title));

		_txtInfo.setColor((byte)(Palette.blockOffset(0)+2));
		_txtInfo.setWordWrap(true);
		_txtInfo.setScrollable(true);
		_txtInfo.setText(tr(defs.text));

		// all of the above are common to the TFTD articles.

		if (defs.getType() == UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD)
		{
			// this command is contained in all the subtypes of this article,
			// and probably shouldn't run until all surfaces are added.
			// in the case of a simple image/title/text article,
			// we're done adding surfaces for now.
			centerAllSurfaces();
		}

	}

	~ArticleStateTFTD() { }
}
