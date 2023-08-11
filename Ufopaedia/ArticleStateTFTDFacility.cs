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

internal class ArticleStateTFTDFacility : ArticleStateTFTD
{
	TextList _lstInfo;

	internal ArticleStateTFTDFacility(ArticleDefinitionTFTD defs) : base(defs)
	{
		_txtInfo.setHeight(112);

		RuleBaseFacility facility = _game.getMod().getBaseFacility(defs.id, true);

		_lstInfo = new TextList(150, 50, 168, 150);
		add(_lstInfo);

		_lstInfo.setColor((byte)(Palette.blockOffset(0)+2));
		_lstInfo.setColumns(2, 104, 46);
		_lstInfo.setDot(true);

		string ss;
		uint row = 0;
		if (facility.getDefenseValue() > 0)
		{
			_lstInfo.setY(_lstInfo.getY() - 16);
			_txtInfo.setHeight(_txtInfo.getHeight() - 16);
			ss = facility.getDefenseValue().ToString();
			_lstInfo.addRow(2, tr("STR_DEFENSE_VALUE"), ss);
			_lstInfo.setCellColor(row++, 1, (byte)(Palette.blockOffset(15)+4));

			ss = Unicode.formatPercentage(facility.getHitRatio());
			_lstInfo.addRow(2, tr("STR_HIT_RATIO"), ss);
			_lstInfo.setCellColor(row++, 1, (byte)(Palette.blockOffset(15)+4));
		}

		_lstInfo.addRow(2, tr("STR_CONSTRUCTION_TIME"), tr("STR_DAY", (uint)facility.getBuildTime()));
		_lstInfo.setCellColor(row++, 1, (byte)(Palette.blockOffset(15)+4));

		ss = Unicode.formatFunding(facility.getBuildCost());
		_lstInfo.addRow(2, tr("STR_CONSTRUCTION_COST"), ss);
		_lstInfo.setCellColor(row++, 1, (byte)(Palette.blockOffset(15)+4));

		ss = Unicode.formatFunding(facility.getMonthlyCost());
		_lstInfo.addRow(2, tr("STR_MAINTENANCE_COST"), ss);
		_lstInfo.setCellColor(row++, 1, (byte)(Palette.blockOffset(15)+4));

		centerAllSurfaces();
	}

	~ArticleStateTFTDFacility() { }
}
