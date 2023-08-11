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

internal class ArticleStateTFTDCraft : ArticleStateTFTD
{
	protected Text _txtStats;

	internal ArticleStateTFTDCraft(ArticleDefinitionTFTD defs) : base(defs)
	{
		_txtInfo.setHeight(80);

		RuleCraft craft = _game.getMod().getCraft(defs.id, true);
		_txtStats = new Text(131, 56, 187, 116);
		add(_txtStats);

		_txtStats.setColor((byte)(Palette.blockOffset(0)+2));
		_txtStats.setSecondaryColor((byte)(Palette.blockOffset(15)+4));

		var ss = new StringBuilder();
		ss.Append($"{tr("STR_MAXIMUM_SPEED_UC").arg(Unicode.formatNumber(craft.getMaxSpeed()))}\n");
		ss.Append($"{tr("STR_ACCELERATION").arg(craft.getAcceleration())}\n");
		ss.Append($"{tr("STR_FUEL_CAPACITY").arg(Unicode.formatNumber(craft.getMaxFuel()))}\n");
		ss.Append($"{tr("STR_WEAPON_PODS").arg(craft.getWeapons())}\n");
		ss.Append($"{tr("STR_DAMAGE_CAPACITY_UC").arg(Unicode.formatNumber(craft.getMaxDamage()))}\n");
		ss.Append($"{tr("STR_CARGO_SPACE").arg(craft.getSoldiers())}\n");
		ss.Append(tr("STR_HWP_CAPACITY").arg(craft.getVehicles()));
		_txtStats.setText(ss.ToString());

		centerAllSurfaces();
	}

	~ArticleStateTFTDCraft() { }
}
