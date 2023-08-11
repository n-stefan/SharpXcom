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

internal class ArticleStateTFTDItem : ArticleStateTFTD
{
	Text _txtShotType;
	Text _txtAccuracy;
	Text _txtTuCost;
	TextList _lstInfo;
	Text[] _txtAmmoType = new Text[3];
	Text[] _txtAmmoDamage = new Text[3];

	internal ArticleStateTFTDItem(ArticleDefinitionTFTD defs) : base(defs)
	{
		RuleItem item = _game.getMod().getItem(defs.id, true);

		List<string> ammo_data = item.getCompatibleAmmo();

		// SHOT STATS TABLE (for firearms only)
		if (item.getBattleType() == BattleType.BT_FIREARM)
		{
			_txtShotType = new Text(53, 17, 8, 157);
			add(_txtShotType);
			_txtShotType.setColor((byte)(Palette.blockOffset(0)+2));
			_txtShotType.setWordWrap(true);
			_txtShotType.setText(tr("STR_SHOT_TYPE"));

			_txtAccuracy = new Text(57, 17, 61, 157);
			add(_txtAccuracy);
			_txtAccuracy.setColor((byte)(Palette.blockOffset(0)+2));
			_txtAccuracy.setWordWrap(true);
			_txtAccuracy.setText(tr("STR_ACCURACY_UC"));

			_txtTuCost = new Text(56, 17, 118, 157);
			add(_txtTuCost);
			_txtTuCost.setColor((byte)(Palette.blockOffset(0)+2));
			_txtTuCost.setWordWrap(true);
			_txtTuCost.setText(tr("STR_TIME_UNIT_COST"));

			_lstInfo = new TextList(140, 55, 8, 170);
			add(_lstInfo);

			_lstInfo.setColor((byte)(Palette.blockOffset(15)+4)); // color for %-data!
			_lstInfo.setColumns(3, 70, 40, 30);

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
				_lstInfo.setCellColor(current_row, 0, (byte)(Palette.blockOffset(0)+2));
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
				_lstInfo.setCellColor(current_row, 0, (byte)(Palette.blockOffset(0)+2));
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
				_lstInfo.setCellColor(current_row, 0, (byte)(Palette.blockOffset(0)+2));
				current_row++;
			}
		}

		// AMMO column
		var ss = new StringBuilder();

		for (int i = 0; i<3; ++i)
		{
			_txtAmmoType[i] = new Text(120, 9, 168, 144 + i*10);
			add(_txtAmmoType[i]);
			_txtAmmoType[i].setColor((byte)(Palette.blockOffset(0)+2));
			_txtAmmoType[i].setWordWrap(true);

			_txtAmmoDamage[i] = new Text(20, 9, 300, 144 + i*10);
			add(_txtAmmoDamage[i]);
			_txtAmmoDamage[i].setColor((byte)(Palette.blockOffset(3)+6));
		}

		switch (item.getBattleType())
		{
			case BattleType.BT_FIREARM:
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
						}
					}
				}
				break;
			case BattleType.BT_AMMO:
			case BattleType.BT_GRENADE:
			case BattleType.BT_PROXIMITYGRENADE:
			case BattleType.BT_MELEE:
				_txtAmmoType[0].setText(tr(getDamageTypeText(item.getDamageType())));

				ss.Clear();
				ss.Append(item.getPower());
				_txtAmmoDamage[0].setText(ss.ToString());
				break;
			default: break;
		}

		if (!string.IsNullOrEmpty(_txtAmmoType[0].getText()))
			_txtInfo.setHeight(112);

		centerAllSurfaces();
	}

	~ArticleStateTFTDItem() { }
}
