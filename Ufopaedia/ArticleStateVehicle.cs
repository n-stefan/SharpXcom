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
 * ArticleStateVehicle has a caption, text and a stats block.
 */
internal class ArticleStateVehicle : ArticleState
{
	Text _txtTitle;
	Text _txtInfo;
	TextList _lstStats;

	internal ArticleStateVehicle(ArticleDefinitionVehicle defs) : base(defs.id)
	{
		Unit unit = _game.getMod().getUnit(defs.id, true);
		Armor armor = _game.getMod().getArmor(unit.getArmor(), true);
		RuleItem item = _game.getMod().getItem(defs.id, true);

		// add screen elements
		_txtTitle = new Text(310, 17, 5, 23);
		_txtInfo = new Text(300, 150, 10, 122);
		_lstStats = new TextList(300, 89, 10, 48);

		// Set palette
		setPalette("PAL_UFOPAEDIA");

		base.initLayout();

		// add other elements
		add(_txtTitle);
		add(_txtInfo);
		add(_lstStats);

		// Set up objects
		_game.getMod().getSurface("BACK10.SCR").blit(_bg);
		_btnOk.setColor(Palette.blockOffset(5));
		_btnPrev.setColor(Palette.blockOffset(5));
		_btnNext.setColor(Palette.blockOffset(5));

		_txtTitle.setColor((byte)(Palette.blockOffset(15)+4));
		_txtTitle.setBig();
		_txtTitle.setText(tr(defs.title));

		_txtInfo.setColor((byte)(Palette.blockOffset(15)-1));
		_txtInfo.setWordWrap(true);
		_txtInfo.setScrollable(true);
		_txtInfo.setText(tr(defs.text));

		_lstStats.setColor((byte)(Palette.blockOffset(15)+4));
		_lstStats.setColumns(2, 175, 145);
		_lstStats.setDot(true);

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

		_lstStats.addRow(2, tr("STR_WEAPON"), tr(defs.weapon));

		if (item.getCompatibleAmmo().Any())
		{
			RuleItem ammo = _game.getMod().getItem(item.getCompatibleAmmo().First(), true);

			string ss8 = ammo.getPower().ToString();
			_lstStats.addRow(2, tr("STR_WEAPON_POWER"), ss8);

			_lstStats.addRow(2, tr("STR_AMMUNITION"), tr(ammo.getName()));

			string ss9;
			if (item.getClipSize() > 0)
			{
				ss9 = item.getClipSize().ToString();
			}
			else
			{
				ss9 = ammo.getClipSize().ToString();
			}

			_lstStats.addRow(2, tr("STR_ROUNDS"), ss9);

			_txtInfo.setY(138);
		}
		else
		{
			string ss8 = item.getPower().ToString();
			_lstStats.addRow(2, tr("STR_WEAPON_POWER"), ss8);
		}
		centerAllSurfaces();
	}

	~ArticleStateVehicle() { }
}
