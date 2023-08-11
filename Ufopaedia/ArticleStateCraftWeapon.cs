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
 * ArticleStateCraftWeapon has a caption, background image and a stats block.
 */
internal class ArticleStateCraftWeapon : ArticleState
{
	Text _txtTitle;
	Text _txtInfo;
	TextList _lstInfo;

	internal ArticleStateCraftWeapon(ArticleDefinitionCraftWeapon defs) : base(defs.id)
	{
		RuleCraftWeapon weapon = _game.getMod().getCraftWeapon(defs.id, true);

		// add screen elements
		_txtTitle = new Text(200, 32, 5, 24);

		// Set palette
		setPalette("PAL_BATTLEPEDIA");

		base.initLayout();

		// add other elements
		add(_txtTitle);

		// Set up objects
		_game.getMod().getSurface(defs.image_id).blit(_bg);
		_btnOk.setColor(Palette.blockOffset(1));
		_btnPrev.setColor(Palette.blockOffset(1));
		_btnNext.setColor(Palette.blockOffset(1));

		_txtTitle.setColor((byte)(Palette.blockOffset(14)+15));
		_txtTitle.setBig();
		_txtTitle.setWordWrap(true);
		_txtTitle.setText(tr(defs.title));

		_txtInfo = new Text(310, 32, 5, 160);
		add(_txtInfo);

		_txtInfo.setColor((byte)(Palette.blockOffset(14)+15));
		_txtInfo.setWordWrap(true);
		_txtInfo.setScrollable(true);
		_txtInfo.setText(tr(defs.text));

		_lstInfo = new TextList(250, 111, 5, 80);
		add(_lstInfo);

		_lstInfo.setColor((byte)(Palette.blockOffset(14)+15));
		_lstInfo.setColumns(2, 180, 70);
		_lstInfo.setDot(true);
		_lstInfo.setBig();

		_lstInfo.addRow(2, tr("STR_DAMAGE"), Unicode.formatNumber(weapon.getDamage()));
		_lstInfo.setCellColor(0, 1, (byte)(Palette.blockOffset(15)+4));

		_lstInfo.addRow(2, tr("STR_RANGE"), tr("STR_KILOMETERS").arg(weapon.getRange()));
		_lstInfo.setCellColor(1, 1, (byte)(Palette.blockOffset(15)+4));

		_lstInfo.addRow(2, tr("STR_ACCURACY"), Unicode.formatPercentage(weapon.getAccuracy()));
		_lstInfo.setCellColor(2, 1, (byte)(Palette.blockOffset(15)+4));

		_lstInfo.addRow(2, tr("STR_RE_LOAD_TIME"), tr("STR_SECONDS").arg(weapon.getStandardReload()));
		_lstInfo.setCellColor(3, 1, (byte)(Palette.blockOffset(15)+4));

		_lstInfo.addRow(2, tr("STR_ROUNDS"), Unicode.formatNumber(weapon.getAmmoMax()));
		_lstInfo.setCellColor(4, 1, (byte)(Palette.blockOffset(15)+4));

		centerAllSurfaces();
	}

	~ArticleStateCraftWeapon() { }
}
