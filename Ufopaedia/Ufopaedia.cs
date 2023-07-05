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
 * This static class encapsulates all functions related to Ufopaedia
 * for the game.
 * Main purpose is to open Ufopaedia from Geoscape, navigate between articles
 * and release new articles after successful research.
 */
internal class Ufopaedia
{
    // This section is meant for articles, that have to be activated,
    // but have no own entry in a list. E.g. Ammunition items.
    // Maybe others as well, that should just not be selectable.
    internal const string UFOPAEDIA_NOT_AVAILABLE = "STR_NOT_AVAILABLE";

	/// current selected article index (for prev/next navigation).
	protected static uint _current_index;

    /**
	 * Open Ufopaedia start state, presenting the section selection buttons.
	 * @param game Pointer to actual game.
	 */
    internal static void open(Game game) =>
        game.pushState(new UfopaediaStartState());

    /**
	 * Set UPSaved index and open the new state.
	 * @param game Pointer to actual game.
	 * @param article Article definition of the article to open.
	 */
    internal static void openArticle(Game game, ArticleDefinition article)
    {
        string id = article.id;
        _current_index = getArticleIndex(game.getSavedGame(), game.getMod(), id);
        unchecked
        {
            if (_current_index != (uint)-1)
            {
                game.pushState(createArticleState(article));
            }
        }
    }

    /**
	 * Checks if selected article_id is available -> if yes, open it.
	 * @param game Pointer to actual game.
	 * @param article_id Article id to find.
	 */
    internal static void openArticle(Game game, string article_id)
	{
        string id = article_id;
		_current_index = getArticleIndex(game.getSavedGame(), game.getMod(), id);
        unchecked
        {
			if (_current_index != (uint)-1)
			{
				ArticleDefinition article = game.getMod().getUfopaediaArticle(id);
				game.pushState(createArticleState(article));
			}
		}
	}

    /**
	 * Gets the index of the selected article_id in the visible list.
	 * If the id is not found, returns -1.
	 * @param save Pointer to saved game.
	 * @param mod Pointer to mod.
	 * @param article_id Article id to find.
	 * @returns Index of the given article id in the internal list, -1 if not found.
	 */
    static uint getArticleIndex(SavedGame save, Mod.Mod mod, string article_id)
    {
        string UC_ID = article_id + "_UC";
        List<ArticleDefinition> articles = getAvailableArticles(save, mod);
        for (int it = 0; it < articles.Count; ++it)
        {
            if (articles[it].id == article_id)
            {
                return (uint)it;
            }
        }
        for (int it = 0; it < articles.Count; ++it)
        {
            if (articles[it].id == UC_ID)
            {
                article_id = UC_ID;
                return (uint)it;
            }
        }
        for (int it = 0; it < articles.Count; ++it)
        {
            foreach (var j in articles[it].requires)
            {
                if (article_id == j)
                {
                    article_id = articles[it].id;
                    return (uint)it;
                }
            }
        }
        unchecked { return (uint)-1; }
    }

    /**
	 * Return an ArticleList with all the currently visible ArticleIds.
	 * @param save Pointer to saved game.
	 * @param mod Pointer to mod.
	 * @return List of visible ArticleDefinitions.
	 */
    static List<ArticleDefinition> getAvailableArticles(SavedGame save, Mod.Mod mod)
    {
        List<string> list = mod.getUfopaediaList();
        var articles = new List<ArticleDefinition>();
        foreach (var it in list)
        {
            ArticleDefinition article = mod.getUfopaediaArticle(it);
            if (isArticleAvailable(save, article) && article.section != UFOPAEDIA_NOT_AVAILABLE)
            {
                articles.Add(article);
            }
        }
        return articles;
    }

    /**
	 * Fill an ArticleList with the currently visible ArticleIds of the given section.
	 * @param save Pointer to saved game.
	 * @param mod Pointer to mod.
	 * @param section Article section to find, e.g. "XCOM Crafts & Armaments", "Alien Lifeforms", etc.
	 * @param data Article definition list object to fill data in.
	 */
    internal static void list(SavedGame save, Mod.Mod mod, string section, List<ArticleDefinition> data)
	{
		List<ArticleDefinition> articles = getAvailableArticles(save, mod);
		foreach (var it in articles)
		{
			if (it.section == section)
			{
				data.Add(it);
			}
		}
	}

    /**
	 * Checks, if an article has already been released.
	 * @param save Pointer to saved game.
	 * @param article Article definition to release.
	 * @returns true, if the article is available.
	 */
    static bool isArticleAvailable(SavedGame save, ArticleDefinition article) =>
        save.isResearched(article.requires);

    /**
	 * Creates a new article state dependent on the given article definition.
	 * @param game Pointer to actual game.
	 * @param article Article definition to create from.
	 * @returns Article state object if created, 0 otherwise.
	 */
    static ArticleState createArticleState(ArticleDefinition article)
    {
        switch (article.getType())
        {
            case UfopaediaTypeId.UFOPAEDIA_TYPE_CRAFT:
                return new ArticleStateCraft((ArticleDefinitionCraft)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_CRAFT_WEAPON:
                return new ArticleStateCraftWeapon((ArticleDefinitionCraftWeapon)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_VEHICLE:
                return new ArticleStateVehicle((ArticleDefinitionVehicle)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_ITEM:
                return new ArticleStateItem((ArticleDefinitionItem)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_ARMOR:
                return new ArticleStateArmor((ArticleDefinitionArmor)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_BASE_FACILITY:
                return new ArticleStateBaseFacility((ArticleDefinitionBaseFacility)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_TEXT:
                return new ArticleStateText((ArticleDefinitionText)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_TEXTIMAGE:
                return new ArticleStateTextImage((ArticleDefinitionTextImage)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_UFO:
                return new ArticleStateUfo((ArticleDefinitionUfo)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD:
                return new ArticleStateTFTD((ArticleDefinitionTFTD)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD_CRAFT:
                return new ArticleStateTFTDCraft((ArticleDefinitionTFTD)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD_CRAFT_WEAPON:
                return new ArticleStateTFTDCraftWeapon((ArticleDefinitionTFTD)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD_VEHICLE:
                return new ArticleStateTFTDVehicle((ArticleDefinitionTFTD)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD_ITEM:
                return new ArticleStateTFTDItem((ArticleDefinitionTFTD)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD_ARMOR:
                return new ArticleStateTFTDArmor((ArticleDefinitionTFTD)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD_BASE_FACILITY:
                return new ArticleStateTFTDFacility((ArticleDefinitionTFTD)article);
            case UfopaediaTypeId.UFOPAEDIA_TYPE_TFTD_USO:
                return new ArticleStateTFTDUso((ArticleDefinitionTFTD)article);
            default: break;
        }
        return 0;
    }
}
