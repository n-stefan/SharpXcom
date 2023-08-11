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
 * ArticleStateUfo has a caption, text, preview image and a stats block.
 * The UFO image is found using the RuleUfo class.
 */
internal class ArticleStateUfo : ArticleState
{
	Text _txtTitle;
	Surface _image;
	Text _txtInfo;
	TextList _lstInfo;

	internal ArticleStateUfo(ArticleDefinitionUfo defs) : base(defs.id)
	{
		RuleUfo ufo = _game.getMod().getUfo(defs.id, true);

		// add screen elements
		_txtTitle = new Text(155, 32, 5, 24);

		// Set palette
		setPalette("PAL_GEOSCAPE");

		base.initLayout();

		// add other elements
		add(_txtTitle);

		// Set up objects
		_game.getMod().getSurface("BACK11.SCR").blit(_bg);
		_btnOk.setColor((byte)(Palette.blockOffset(8)+5));
		_btnPrev.setColor((byte)(Palette.blockOffset(8)+5));
		_btnNext.setColor((byte)(Palette.blockOffset(8)+5));

		_txtTitle.setColor((byte)(Palette.blockOffset(8)+5));
		_txtTitle.setBig();
		_txtTitle.setWordWrap(true);
		_txtTitle.setText(tr(defs.title));

		_image = new Surface(160, 52, 160, 6);
		add(_image);

		RuleInterface dogfightInterface = _game.getMod().getInterface("dogfight");
		Surface graphic = _game.getMod().getSurface("INTERWIN.DAT");
		graphic.setX(0);
		graphic.setY(0);
		graphic.getCrop().x = 0;
		graphic.getCrop().y = 0;
		graphic.getCrop().w = _image.getWidth();
		graphic.getCrop().h = _image.getHeight();
		_image.drawRect(ref graphic.getCrop(), 15);
		graphic.blit(_image);

		if (string.IsNullOrEmpty(ufo.getModSprite()))
		{
			graphic.getCrop().y = dogfightInterface.getElement("previewMid").y + dogfightInterface.getElement("previewMid").h * ufo.getSprite();
			graphic.getCrop().h = dogfightInterface.getElement("previewMid").h;
		}
		else
		{
			graphic = _game.getMod().getSurface(ufo.getModSprite());
		}
		graphic.setX(0);
		graphic.setY(0);
		graphic.blit(_image);

		_txtInfo = new Text(300, 50, 10, 140);
		add(_txtInfo);

		_txtInfo.setColor((byte)(Palette.blockOffset(8)+5));
		_txtInfo.setWordWrap(true);
		_txtInfo.setScrollable(true);
		_txtInfo.setText(tr(defs.text));

		_lstInfo = new TextList(310, 64, 10, 68);
		add(_lstInfo);

		centerAllSurfaces();

		_lstInfo.setColor((byte)(Palette.blockOffset(8)+5));
		_lstInfo.setColumns(2, 200, 110);
//		_lstInfo.setCondensed(true);
		_lstInfo.setBig();
		_lstInfo.setDot(true);

		_lstInfo.addRow(2, tr("STR_DAMAGE_CAPACITY"), Unicode.formatNumber(ufo.getMaxDamage()));

		_lstInfo.addRow(2, tr("STR_WEAPON_POWER"), Unicode.formatNumber(ufo.getWeaponPower()));

		_lstInfo.addRow(2, tr("STR_WEAPON_RANGE"), tr("STR_KILOMETERS").arg(ufo.getWeaponRange()));

		_lstInfo.addRow(2, tr("STR_MAXIMUM_SPEED"), tr("STR_KNOTS").arg(Unicode.formatNumber(ufo.getMaxSpeed())));
	}

	~ArticleStateUfo() { }
}
