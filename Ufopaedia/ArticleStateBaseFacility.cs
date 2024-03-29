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
 * ArticleStateBaseFacility has a caption, text, preview image and a stats block.
 * The facility image is found using the RuleBasefacility class.
 */
internal class ArticleStateBaseFacility : ArticleState
{
	Text _txtTitle;
	Surface _image;
	Text _txtInfo;
	TextList _lstInfo;

	internal ArticleStateBaseFacility(ArticleDefinitionBaseFacility defs) : base(defs.id)
	{
		RuleBaseFacility facility = _game.getMod().getBaseFacility(defs.id, true);

		// add screen elements
		_txtTitle = new Text(200, 17, 10, 24);

		// Set palette
		setPalette("PAL_BASESCAPE");

		base.initLayout();

		// add other elements
		add(_txtTitle);

		// Set up objects
		_game.getMod().getSurface("BACK09.SCR").blit(_bg);
		_btnOk.setColor(Palette.blockOffset(4));
		_btnPrev.setColor(Palette.blockOffset(4));
		_btnNext.setColor(Palette.blockOffset(4));

		_txtTitle.setColor((byte)(Palette.blockOffset(13)+10));
		_txtTitle.setBig();
		_txtTitle.setText(tr(defs.title));

		// build preview image
		int tile_size = 32;
		_image = new Surface(tile_size*2, tile_size*2, 232, 16);
		add(_image);

		SurfaceSet graphic = _game.getMod().getSurfaceSet("BASEBITS.PCK");
		Surface frame;
		int x_offset, y_offset;
		int x_pos, y_pos;
		int num;

		if (facility.getSize()==1)
		{
			x_offset = y_offset = tile_size/2;
		}
		else
		{
			x_offset = y_offset = 0;
		}

		num = 0;
		y_pos = y_offset;
		for (int y = 0; y < facility.getSize(); ++y)
		{
			x_pos = x_offset;
			for (int x = 0; x < facility.getSize(); ++x)
			{
				frame = graphic.getFrame(facility.getSpriteShape() + num);
				frame.setX(x_pos);
				frame.setY(y_pos);
				frame.blit(_image);

				if (facility.getSize()==1)
				{
					frame = graphic.getFrame(facility.getSpriteFacility() + num);
					frame.setX(x_pos);
					frame.setY(y_pos);
					frame.blit(_image);
				}

				x_pos += tile_size;
				num++;
			}
			y_pos += tile_size;
		}

		_txtInfo = new Text(300, 90, 10, 104);
		add(_txtInfo);

		_txtInfo.setColor((byte)(Palette.blockOffset(13)+10));
		_txtInfo.setWordWrap(true);
		_txtInfo.setScrollable(true);
		_txtInfo.setText(tr(defs.text));

		_lstInfo = new TextList(200, 42, 10, 42);
		add(_lstInfo);

		_lstInfo.setColor((byte)(Palette.blockOffset(13)+10));
		_lstInfo.setColumns(2, 140, 60);
		_lstInfo.setDot(true);

		_lstInfo.addRow(2, tr("STR_CONSTRUCTION_TIME"), tr("STR_DAY", (uint)facility.getBuildTime()));
		_lstInfo.setCellColor(0, 1, (byte)(Palette.blockOffset(13)+0));

		string ss = Unicode.formatFunding(facility.getBuildCost());
		_lstInfo.addRow(2, tr("STR_CONSTRUCTION_COST"), ss);
		_lstInfo.setCellColor(1, 1, (byte)(Palette.blockOffset(13)+0));

		ss = Unicode.formatFunding(facility.getMonthlyCost());
		_lstInfo.addRow(2, tr("STR_MAINTENANCE_COST"), ss);
		_lstInfo.setCellColor(2, 1, (byte)(Palette.blockOffset(13)+0));

		if (facility.getDefenseValue() > 0)
		{
			ss = facility.getDefenseValue().ToString();
			_lstInfo.addRow(2, tr("STR_DEFENSE_VALUE"), ss);
			_lstInfo.setCellColor(3, 1, (byte)(Palette.blockOffset(13)+0));

			ss = Unicode.formatPercentage(facility.getHitRatio());
			_lstInfo.addRow(2, tr("STR_HIT_RATIO"), ss);
			_lstInfo.setCellColor(4, 1, (byte)(Palette.blockOffset(13)+0));
		}
		centerAllSurfaces();
	}

	~ArticleStateBaseFacility() { }
}
