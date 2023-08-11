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

internal class ArticleStateTFTDVehicle : ArticleStateTFTD
{
	TextList _lstStats, _lstStats2;

	internal ArticleStateTFTDVehicle(ArticleDefinitionTFTD defs) : base(defs)
	{
		_txtInfo.setHeight(72);

		Unit unit = _game.getMod().getUnit(defs.id, true);
		Armor armor = _game.getMod().getArmor(unit.getArmor(), true);
		RuleItem item = _game.getMod().getItem(defs.id, true);

		_lstStats = new TextList(150, 65, 168, 106);

		add(_lstStats);

		_lstStats.setColor((byte)(Palette.blockOffset(0)+2));
		_lstStats.setColumns(2, 100, 50);
		_lstStats.setDot(true);

		_lstStats2 = new TextList(195, 33, 25, 166);

		add(_lstStats2);

		_lstStats2.setColor((byte)(Palette.blockOffset(0) + 2));
		_lstStats2.setColumns(2, 65, 130);
		_lstStats2.setDot(true);

		string ss = unit.getStats().tu.ToString();
		_lstStats.addRow(2, tr("STR_TIME_UNITS"), ss);

		string ss2 = unit.getStats().health.ToString();
		_lstStats.addRow(2, tr("STR_HEALTH"), ss2);

		string ss3 = armor.getFrontArmor().ToString();
		_lstStats.addRow(2, tr("STR_FRONT_ARMOR"), ss3);

		string ss4 = armor.getSideArmor().ToString();
		_lstStats.addRow(2, tr("STR_LEFT_ARMOR"), ss4);

		string ss5 = armor.getSideArmor().ToString();
		_lstStats.addRow(2, tr("STR_RIGHT_ARMOR"), ss5);

		string ss6 = armor.getRearArmor().ToString();
		_lstStats.addRow(2, tr("STR_REAR_ARMOR"), ss6);

		string ss7 = armor.getUnderArmor().ToString();
		_lstStats.addRow(2, tr("STR_UNDER_ARMOR"), ss7);

		_lstStats2.addRow(2, tr("STR_WEAPON"), tr(defs.weapon));

		if (item.getCompatibleAmmo().Any())
		{
			RuleItem ammo = _game.getMod().getItem(item.getCompatibleAmmo().First(), true);

			string ss8 = ammo.getPower().ToString();
			_lstStats2.addRow(2, tr("STR_WEAPON_POWER"), ss8);

			_lstStats2.addRow(2, tr("STR_AMMUNITION"), tr(ammo.getName()));

			string ss9;
			if (item.getClipSize() > 0)
			{
				ss9 = item.getClipSize().ToString();
			}
			else
			{
				ss9 = ammo.getClipSize().ToString();
			}

			_lstStats2.addRow(2, tr("STR_ROUNDS"), ss9);
		}
		else
		{
			string ss8 = item.getPower().ToString();
			_lstStats2.addRow(2, tr("STR_WEAPON_POWER"), ss8);
		}

		for (uint i = 0; i != _lstStats.getRows(); ++i)
		{
			_lstStats.setCellColor(i, 1, (byte)(Palette.blockOffset(15) + 4));
		}
		for (uint i = 0; i != _lstStats2.getRows(); ++i)
		{
			_lstStats2.setCellColor(i, 1, (byte)(Palette.blockOffset(15) + 4));
		}

		centerAllSurfaces();
	}

	~ArticleStateTFTDVehicle() { }
}
