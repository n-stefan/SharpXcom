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

namespace SharpXcom.Basescape;

/**
 * Craft Info screen that shows all the
 * info of a specific craft.
 */
internal class CraftInfoState : State
{
    Base _base;
    uint _craftId;
    Craft _craft;
    Window _window;
    TextButton _btnOk, _btnW1, _btnW2, _btnCrew, _btnEquip, _btnArmor;
    TextEdit _edtCraft;
    Text _txtDamage, _txtFuel;
    Text _txtW1Name, _txtW1Ammo, _txtW2Name, _txtW2Ammo;
    Surface _sprite, _weapon1, _weapon2, _crew, _equip;

    /**
     * Initializes all the elements in the Craft Info screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param craftId ID of the selected craft.
     */
    internal CraftInfoState(Base @base, uint craftId)
    {
        _base = @base;
        _craftId = craftId;
        _craft = null;

        // Create objects
        if (_game.getSavedGame().getMonthsPassed() != -1)
        {
            _window = new Window(this, 320, 200, 0, 0, WindowPopup.POPUP_BOTH);
        }
        else
        {
            _window = new Window(this, 320, 200, 0, 0, WindowPopup.POPUP_NONE);
        }
        _btnOk = new TextButton(64, 24, 128, 168);
        _btnW1 = new TextButton(24, 32, 14, 48);
        _btnW2 = new TextButton(24, 32, 282, 48);
        _btnCrew = new TextButton(64, 16, 14, 96);
        _btnEquip = new TextButton(64, 16, 14, 120);
        _btnArmor = new TextButton(64, 16, 14, 144);
        _edtCraft = new TextEdit(this, 140, 16, 80, 8);
        _txtDamage = new Text(100, 17, 14, 24);
        _txtFuel = new Text(82, 17, 228, 24);
        _txtW1Name = new Text(95, 16, 46, 48);
        _txtW1Ammo = new Text(75, 24, 46, 64);
        _txtW2Name = new Text(95, 16, 184, 48);
        _txtW2Ammo = new Text(75, 24, 204, 64);
        _sprite = new Surface(32, 40, 144, 52);
        _weapon1 = new Surface(15, 17, 121, 63);
        _weapon2 = new Surface(15, 17, 184, 63);
        _crew = new Surface(220, 18, 85, 96);
        _equip = new Surface(220, 18, 85, 121);

        // Set palette
        setInterface("craftInfo");

        add(_window, "window", "craftInfo");
        add(_btnOk, "button", "craftInfo");
        add(_btnW1, "button", "craftInfo");
        add(_btnW2, "button", "craftInfo");
        add(_btnCrew, "button", "craftInfo");
        add(_btnEquip, "button", "craftInfo");
        add(_btnArmor, "button", "craftInfo");
        add(_edtCraft, "text1", "craftInfo");
        add(_txtDamage, "text1", "craftInfo");
        add(_txtFuel, "text1", "craftInfo");
        add(_txtW1Name, "text2", "craftInfo");
        add(_txtW1Ammo, "text3", "craftInfo");
        add(_txtW2Name, "text2", "craftInfo");
        add(_txtW2Ammo, "text3", "craftInfo");
        add(_sprite);
        add(_weapon1);
        add(_weapon2);
        add(_crew);
        add(_equip);

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK14.SCR"));

        _btnOk.setText(tr("STR_OK"));
        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

        _btnW1.setText("1");
        _btnW1.onMouseClick(btnW1Click);

        _btnW2.setText("2");
        _btnW2.onMouseClick(btnW2Click);

        _btnCrew.setText(tr("STR_CREW"));
        _btnCrew.onMouseClick(btnCrewClick);

        _btnEquip.setText(tr("STR_EQUIPMENT_UC"));
        _btnEquip.onMouseClick(btnEquipClick);

        _btnArmor.setText(tr("STR_ARMOR"));
        _btnArmor.onMouseClick(btnArmorClick);

        _edtCraft.setBig();
        _edtCraft.setAlign(TextHAlign.ALIGN_CENTER);
        _edtCraft.onChange(edtCraftChange);

        _txtW1Name.setWordWrap(true);

        _txtW2Name.setWordWrap(true);
    }

    /**
     *
     */
    ~CraftInfoState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Engine.Action _) =>
        _game.popState();

    /**
     * Goes to the Select Armament window for
     * the first weapon.
     * @param action Pointer to an action.
     */
    void btnW1Click(Engine.Action _) =>
        _game.pushState(new CraftWeaponsState(_base, _craftId, 0));

    /**
     * Goes to the Select Armament window for
     * the second weapon.
     * @param action Pointer to an action.
     */
    void btnW2Click(Engine.Action _) =>
        _game.pushState(new CraftWeaponsState(_base, _craftId, 1));

    /**
     * Goes to the Select Squad screen.
     * @param action Pointer to an action.
     */
    void btnCrewClick(Engine.Action _) =>
        _game.pushState(new CraftSoldiersState(_base, _craftId));

    /**
     * Goes to the Select Equipment screen.
     * @param action Pointer to an action.
     */
    void btnEquipClick(Engine.Action _) =>
        _game.pushState(new CraftEquipmentState(_base, _craftId));

    /**
     * Goes to the Select Armor screen.
     * @param action Pointer to an action.
     */
    void btnArmorClick(Engine.Action _) =>
        _game.pushState(new CraftArmorState(_base, _craftId));

    /**
     * Changes the Craft name.
     * @param action Pointer to an action.
     */
    void edtCraftChange(Engine.Action action)
    {
        if (_edtCraft.getText() == _craft.getDefaultName(_game.getLanguage()))
        {
            _craft.setName(string.Empty);
        }
        else
        {
            _craft.setName(_edtCraft.getText());
        }
        if (action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_RETURN ||
            action.getDetails().key.keysym.sym == SDL_Keycode.SDLK_KP_ENTER)
        {
            _edtCraft.setText(_craft.getName(_game.getLanguage()));
        }
    }
}
