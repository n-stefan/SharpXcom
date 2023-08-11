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
 * ArticleStateCraft has a caption, text, background image and a stats block.
 * The layout of the description text and stats block can vary,
 * depending on the background craft image.
 */
internal class ArticleStateCraft : ArticleState
{
	Text _txtTitle;
	Text _txtInfo;
	Text _txtStats;

	internal ArticleStateCraft(ArticleDefinitionCraft defs) : base(defs.id)
	{
		RuleCraft craft = _game.getMod().getCraft(defs.id, true);

		// add screen elements
		_txtTitle = new Text(210, 32, 5, 24);

		// Set palette
		setPalette("PAL_UFOPAEDIA");

		base.initLayout();

		// add other elements
		add(_txtTitle);

		// Set up objects
		_game.getMod().getSurface(defs.image_id).blit(_bg);
		_btnOk.setColor((byte)(Palette.blockOffset(15)-1));
		_btnPrev.setColor((byte)(Palette.blockOffset(15)-1));
		_btnNext.setColor((byte)(Palette.blockOffset(15)-1));

		_txtTitle.setColor((byte)(Palette.blockOffset(14)+15));
		_txtTitle.setBig();
		_txtTitle.setWordWrap(true);
		_txtTitle.setText(tr(defs.title));

		_txtInfo = new Text(defs.rect_text.width, defs.rect_text.height, defs.rect_text.x, defs.rect_text.y);
		add(_txtInfo);

		_txtInfo.setColor((byte)(Palette.blockOffset(14)+15));
		_txtInfo.setWordWrap(true);
		_txtInfo.setScrollable(true);
		_txtInfo.setText(tr(defs.text));

		_txtStats = new Text(defs.rect_stats.width, defs.rect_stats.height, defs.rect_stats.x, defs.rect_stats.y);
		add(_txtStats);

		_txtStats.setColor((byte)(Palette.blockOffset(14)+15));
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

	~ArticleStateCraft() { }
}
