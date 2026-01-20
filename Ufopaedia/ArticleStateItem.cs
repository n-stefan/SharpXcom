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
 * ArticleStateItem has a caption, text, preview image and a stats block.
 * The facility image is found using the RuleBasefacility class.
 */
internal class ArticleStateItem : ArticleState
{
	Text _txtTitle;
	Surface _image;
	Text _txtShotType;
	Text _txtAccuracy;
	Text _txtTuCost;
	TextList _lstInfo;
	Text _txtInfo;
	Text[] _txtAmmoType = new Text[3];
	Text[] _txtAmmoDamage = new Text[3];
	Surface[] _imageAmmo = new Surface[3];
	Text _txtDamage;
	Text _txtAmmo;

	internal ArticleStateItem(ArticleDefinitionItem defs) : base(defs.id)
	{
		RuleItem item = _game.getMod().getItem(defs.id, true);

		// add screen elements
		_txtTitle = new Text(148, 32, 5, 24);

		// Set palette
		setPalette("PAL_BATTLEPEDIA");

		base.initLayout();

		// add other elements
		add(_txtTitle);

		// Set up objects
		_game.getMod().getSurface("BACK08.SCR").blit(_bg);
		_btnOk.setColor(Palette.blockOffset(9));
		_btnPrev.setColor(Palette.blockOffset(9));
		_btnNext.setColor(Palette.blockOffset(9));

		_txtTitle.setColor((byte)(Palette.blockOffset(14)+15));
		_txtTitle.setBig();
		_txtTitle.setWordWrap(true);
		_txtTitle.setText(tr(defs.title));

		// IMAGE
		_image = new Surface(32, 48, 157, 5);
		add(_image);

		item.drawHandSprite(_game.getMod().getSurfaceSet("BIGOBS.PCK"), _image);

		List<string> ammo_data = item.getCompatibleAmmo();

		// SHOT STATS TABLE (for firearms only)
		if (item.getBattleType() == BattleType.BT_FIREARM)
		{
			_txtShotType = new Text(100, 17, 8, 66);
			add(_txtShotType);
			_txtShotType.setColor((byte)(Palette.blockOffset(14)+15));
			_txtShotType.setWordWrap(true);
			_txtShotType.setText(tr("STR_SHOT_TYPE"));

			_txtAccuracy = new Text(50, 17, 104, 66);
			add(_txtAccuracy);
			_txtAccuracy.setColor((byte)(Palette.blockOffset(14)+15));
			_txtAccuracy.setWordWrap(true);
			_txtAccuracy.setText(tr("STR_ACCURACY_UC"));

			_txtTuCost = new Text(60, 17, 158, 66);
			add(_txtTuCost);
			_txtTuCost.setColor((byte)(Palette.blockOffset(14)+15));
			_txtTuCost.setWordWrap(true);
			_txtTuCost.setText(tr("STR_TIME_UNIT_COST"));

			_lstInfo = new TextList(204, 55, 8, 82);
			add(_lstInfo);

			_lstInfo.setColor((byte)(Palette.blockOffset(15)+4)); // color for %-data!
			_lstInfo.setColumns(3, 100, 52, 52);
			_lstInfo.setBig();

			uint current_row = 0;
			if (item.getTUAuto()>0)
			{
				string tu = Unicode.formatPercentage(item.getTUAuto());
				if (item.getFlatRate())
				{
					tu = tu[..^1];
				}
				_lstInfo.addRow(3,
								 tr("STR_SHOT_TYPE_AUTO"),
								 Unicode.formatPercentage(item.getAccuracyAuto()),
								 tu);
				_lstInfo.setCellColor(current_row, 0, (byte)(Palette.blockOffset(14)+15));
				current_row++;
			}

			if (item.getTUSnap()>0)
			{
				string tu = Unicode.formatPercentage(item.getTUSnap());
				if (item.getFlatRate())
				{
					tu = tu[..^1];
				}
				_lstInfo.addRow(3,
								 tr("STR_SHOT_TYPE_SNAP"),
								 Unicode.formatPercentage(item.getAccuracySnap()),
								 tu);
				_lstInfo.setCellColor(current_row, 0, (byte)(Palette.blockOffset(14)+15));
				current_row++;
			}

			if (item.getTUAimed()>0)
			{
				string tu = Unicode.formatPercentage(item.getTUAimed());
				if (item.getFlatRate())
				{
					tu = tu[..^1];
				}
				_lstInfo.addRow(3,
								 tr("STR_SHOT_TYPE_AIMED"),
								 Unicode.formatPercentage(item.getAccuracyAimed()),
								 tu);
				_lstInfo.setCellColor(current_row, 0, (byte)(Palette.blockOffset(14)+15));
				current_row++;
			}

			// text_info is BELOW the info table
			_txtInfo = new Text((ammo_data.Count<3 ? 300 : 180), 56, 8, 138);
		}
		else
		{
			// text_info is larger and starts on top
			_txtInfo = new Text(300, 125, 8, 67);
		}

		add(_txtInfo);

		_txtInfo.setColor((byte)(Palette.blockOffset(14)+15));
		_txtInfo.setWordWrap(true);
		_txtInfo.setScrollable(true);
		_txtInfo.setText(tr(defs.text));

		// AMMO column
		var ss = new StringBuilder();

		for (int i = 0; i<3; ++i)
		{
			_txtAmmoType[i] = new Text(82, 16, 194, 20 + i*49);
			add(_txtAmmoType[i]);
			_txtAmmoType[i].setColor((byte)(Palette.blockOffset(14)+15));
			_txtAmmoType[i].setAlign(TextHAlign.ALIGN_CENTER);
			_txtAmmoType[i].setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
			_txtAmmoType[i].setWordWrap(true);

			_txtAmmoDamage[i] = new Text(82, 17, 194, 40 + i*49);
			add(_txtAmmoDamage[i]);
			_txtAmmoDamage[i].setColor(Palette.blockOffset(2));
			_txtAmmoDamage[i].setAlign(TextHAlign.ALIGN_CENTER);
			_txtAmmoDamage[i].setBig();

			_imageAmmo[i] = new Surface(32, 48, 280, 16 + i*49);
			add(_imageAmmo[i]);
		}

		switch (item.getBattleType())
		{
			case BattleType.BT_FIREARM:
				_txtDamage = new Text(82, 10, 194, 7);
				add(_txtDamage);
				_txtDamage.setColor((byte)(Palette.blockOffset(14)+15));
				_txtDamage.setAlign(TextHAlign.ALIGN_CENTER);
				_txtDamage.setText(tr("STR_DAMAGE_UC"));

				_txtAmmo = new Text(50, 10, 268, 7);
				add(_txtAmmo);
				_txtAmmo.setColor((byte)(Palette.blockOffset(14)+15));
				_txtAmmo.setAlign(TextHAlign.ALIGN_CENTER);
				_txtAmmo.setText(tr("STR_AMMO"));

				if (!ammo_data.Any())
				{
					_txtAmmoType[0].setText(tr(getDamageTypeText(item.getDamageType())));

					ss.Clear();
					ss.Append(item.getPower());
					if (item.getShotgunPellets() != 0)
					{
						ss.Append($"x{item.getShotgunPellets()}");
					}
					_txtAmmoDamage[0].setText(ss.ToString());
				}
				else
				{
					for (int i = 0; i < Math.Min(ammo_data.Count, (uint)3); ++i)
					{
						ArticleDefinition ammo_article = _game.getMod().getUfopaediaArticle(ammo_data[i], true);
						if (Ufopaedia.isArticleAvailable(_game.getSavedGame(), ammo_article))
						{
							RuleItem ammo_rule = _game.getMod().getItem(ammo_data[i], true);
							_txtAmmoType[i].setText(tr(getDamageTypeText(ammo_rule.getDamageType())));

							ss.Clear();
							ss.Append(ammo_rule.getPower());
							if (ammo_rule.getShotgunPellets() != 0)
							{
								ss.Append($"x{ammo_rule.getShotgunPellets()}");
							}
							_txtAmmoDamage[i].setText(ss.ToString());

							ammo_rule.drawHandSprite(_game.getMod().getSurfaceSet("BIGOBS.PCK"), _imageAmmo[i]);
						}
					}
				}
				break;
			case BattleType.BT_AMMO:
			case BattleType.BT_GRENADE:
			case BattleType.BT_PROXIMITYGRENADE:
			case BattleType.BT_MELEE:
				_txtDamage = new Text(82, 10, 194, 7);
				add(_txtDamage);
				_txtDamage.setColor((byte)(Palette.blockOffset(14)+15));
				_txtDamage.setAlign(TextHAlign.ALIGN_CENTER);
				_txtDamage.setText(tr("STR_DAMAGE_UC"));

				_txtAmmoType[0].setText(tr(getDamageTypeText(item.getDamageType())));

				ss.Clear();
				ss.Append(item.getPower());
				if (item.getShotgunPellets() != 0)
				{
					ss.Append($"x{item.getShotgunPellets()}");
				}
				_txtAmmoDamage[0].setText(ss.ToString());
				break;
			default: break;
		}

		centerAllSurfaces();
	}

	~ArticleStateItem() { }
}
