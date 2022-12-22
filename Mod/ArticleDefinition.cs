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

namespace SharpXcom.Mod;

/// define article types
enum UfopaediaTypeId
{
    UFOPAEDIA_TYPE_UNKNOWN = 0,
    UFOPAEDIA_TYPE_CRAFT = 1,
    UFOPAEDIA_TYPE_CRAFT_WEAPON = 2,
    UFOPAEDIA_TYPE_VEHICLE = 3,
    UFOPAEDIA_TYPE_ITEM = 4,
    UFOPAEDIA_TYPE_ARMOR = 5,
    UFOPAEDIA_TYPE_BASE_FACILITY = 6,
    UFOPAEDIA_TYPE_TEXTIMAGE = 7,
    UFOPAEDIA_TYPE_TEXT = 8,
    UFOPAEDIA_TYPE_UFO = 9,
    UFOPAEDIA_TYPE_TFTD = 10,
    UFOPAEDIA_TYPE_TFTD_CRAFT = 11,
    UFOPAEDIA_TYPE_TFTD_CRAFT_WEAPON = 12,
    UFOPAEDIA_TYPE_TFTD_VEHICLE = 13,
    UFOPAEDIA_TYPE_TFTD_ITEM = 14,
    UFOPAEDIA_TYPE_TFTD_ARMOR = 15,
    UFOPAEDIA_TYPE_TFTD_BASE_FACILITY = 16,
    UFOPAEDIA_TYPE_TFTD_USO = 17
};

/**
 * ArticleDefinition is the base class for all article types.
 * This class is used to store all information about articles
 * required to generate an ArticleState from.
 */
internal class ArticleDefinition
{
    internal string section;
    protected UfopaediaTypeId _type_id;
    int _listOrder;
    string id;
    string title;
    List<string> requires;

    /**
	 * Constructor.
	 * @param type_id Article type of this instance.
	 */
    internal ArticleDefinition(UfopaediaTypeId type_id)
    {
        _type_id = type_id;
        _listOrder = 0;
    }

    /**
	 * Destructor.
	 */
    ~ArticleDefinition() { }

    /**
	 * Gets the list weight of the article.
	 * @return The list weight of the article.
	 */
    internal int getListOrder() =>
		_listOrder;

	/**
	 * Loads the article definition from a YAML file.
	 * @param node YAML node.
	 * @param listOrder The list weight for this article.
	 */
	internal void load(YamlNode node, int listOrder)
	{
		id = title = node["id"].ToString();
		section = node["section"].ToString();
        requires = ((YamlSequenceNode)node["requires"]).Children.Select(x => x.ToString()).ToList();
		title = node["title"].ToString();
        //_type_id = (UfopaediaTypeId)node["type_id"].as<int>(_type_id);
        _listOrder = int.Parse(node["listOrder"].ToString());
		if (_listOrder == 0)
		{
            _listOrder = listOrder;
		}
	}
}

/**
 * ArticleDefinitionCraft defines articles for craft, e.g. SKYRANGER.
 * They have a large background image, a stats block and a description positioned differently.
 */
class ArticleDefinitionCraft : ArticleDefinition
{
    /**
	 * Constructor (only setting type of base class).
	 */
    internal ArticleDefinitionCraft() : base(UfopaediaTypeId.UFOPAEDIA_TYPE_CRAFT) { }
}

/**
 * ArticleDefinitionCraftWeapon defines articles for craft weapons, e.g. STINGRAY, AVALANCHE.
 * They have a large background image and a stats block.
 */
class ArticleDefinitionCraftWeapon : ArticleDefinition
{
    /**
	 * Constructor (only setting type of base class).
	 */
    internal ArticleDefinitionCraftWeapon() : base(UfopaediaTypeId.UFOPAEDIA_TYPE_CRAFT_WEAPON) { }
}

/**
 * ArticleDefinitionVehicle defines articles for Vehicles, e.g. Tanks, etc.
 * They have a text description and a stats block.
 */
class ArticleDefinitionVehicle : ArticleDefinition
{
    /**
	 * Constructor (only setting type of base class)
	 */
    internal ArticleDefinitionVehicle() : base(UfopaediaTypeId.UFOPAEDIA_TYPE_VEHICLE) { }
}

/**
 * ArticleDefinitionItem defines articles for all Items, e.g. Weapons, Ammo, Equipment, etc.
 * They have an image (found in BIGOBS.PCK), an optional stats block, maybe ammo and a description.
 */
class ArticleDefinitionItem : ArticleDefinition
{
    /**
	 * Constructor (only setting type of base class).
	 */
    internal ArticleDefinitionItem() : base(UfopaediaTypeId.UFOPAEDIA_TYPE_ITEM) { }
}

/**
 * ArticleDefinitionArmor defines articles for Armor, e.g. Personal Armor, Flying Suit, etc.
 * They have an image (found in MAN_*.SPK) and a stats block.
 */
class ArticleDefinitionArmor : ArticleDefinition
{
    /**
	 * Constructor (only setting type of base class).
	 */
    internal ArticleDefinitionArmor() : base(UfopaediaTypeId.UFOPAEDIA_TYPE_ARMOR) { }
}

/**
 * ArticleDefinitionBaseFacility defines articles for base facilities, e.g. Access lift.
 * They have an image (found in BASEBITS.PCK), a stats block and a description.
 */
class ArticleDefinitionBaseFacility : ArticleDefinition
{
    /**
	 * Constructor (only setting type of base class).
	 */
    internal ArticleDefinitionBaseFacility() : base(UfopaediaTypeId.UFOPAEDIA_TYPE_BASE_FACILITY) { }
}

/**
 * ArticleDefinitionTextImage defines articles with text on the left and
 * an image on the right side of the screen, e.g. ALIEN LIFEFORMS, UFO COMPONENTS.
 */
class ArticleDefinitionTextImage : ArticleDefinition
{
    int text_width;

    /**
	 * Constructor (only setting type of base class).
	 */
    internal ArticleDefinitionTextImage() : base(UfopaediaTypeId.UFOPAEDIA_TYPE_TEXTIMAGE) =>
        text_width = 0;
}

/**
 * ArticleDefinitionText defines articles with only text, e.g. ALIEN RESEARCH.
 */
class ArticleDefinitionText : ArticleDefinition
{
    /**
	 * Constructor (only setting type of base class).
	 */
    internal ArticleDefinitionText() : base(UfopaediaTypeId.UFOPAEDIA_TYPE_TEXT) { }
}

/**
 * ArticleDefinitionUfo defines articles for UFOs, e.g. Small Scout, Terror Ship, etc.
 * They have an image (found in INTERWIN.DAT), a stats block and a description.
 */
class ArticleDefinitionUfo : ArticleDefinition
{
    /**
	 * Constructor (only setting type of base class).
	 */
    internal ArticleDefinitionUfo() : base(UfopaediaTypeId.UFOPAEDIA_TYPE_UFO) { }
}

/**
 * ArticleDefinitionTextImage defines articles with text on the left and
 * an image on the right side of the screen, e.g. ALIEN LIFEFORMS, UFO COMPONENTS.
 */
class ArticleDefinitionTFTD : ArticleDefinition
{
    int text_width;

    /**
	 * Constructor (only setting type of base class).
	 */
    internal ArticleDefinitionTFTD() : base(UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD) =>
        text_width = 0;
}
