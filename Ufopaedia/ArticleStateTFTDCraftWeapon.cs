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

internal class ArticleStateTFTDCraftWeapon : ArticleStateTFTD
{
	TextList _lstInfo;

	internal ArticleStateTFTDCraftWeapon(ArticleDefinitionTFTD defs) : base(defs)
	{
		_txtInfo.setHeight(88);

		RuleCraftWeapon weapon = _game.getMod().getCraftWeapon(defs.id, true);

		_lstInfo = new TextList(150, 50, 168, 126);
		add(_lstInfo);

		_lstInfo.setColor((byte)(Palette.blockOffset(0)+2));
		_lstInfo.setColumns(2, 100, 68); // deliberately making this wider than the original to account for finnish.
		_lstInfo.setDot(true);

		_lstInfo.addRow(2, tr("STR_DAMAGE"), Unicode.formatNumber(weapon.getDamage()));
		_lstInfo.setCellColor(0, 1, (byte)(Palette.blockOffset(15)+4));

		_lstInfo.addRow(2, tr("STR_RANGE"), tr("STR_KILOMETERS").arg(weapon.getRange()));
		_lstInfo.setCellColor(1, 1, (byte)(Palette.blockOffset(15)+4));

		_lstInfo.addRow(2, tr("STR_ACCURACY"), Unicode.formatPercentage(weapon.getAccuracy()));
		_lstInfo.setCellColor(2, 1, (byte)(Palette.blockOffset(15)+4));

		_lstInfo.addRow(2, tr("STR_RE_LOAD_TIME"), tr("STR_SECONDS").arg(weapon.getStandardReload()));
		_lstInfo.setCellColor(3, 1, (byte)(Palette.blockOffset(15)+4));

		centerAllSurfaces();
	}

	~ArticleStateTFTDCraftWeapon() { }
}
