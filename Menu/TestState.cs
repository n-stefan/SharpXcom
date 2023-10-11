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

namespace SharpXcom.Menu;

/**
 * A state purely for testing game functionality.
 * Fun fact, this was the project's original main(),
 * used for testing and implementing basic engine
 * features until it grew a proper structure and was
 * put aside for actual game states. Useful as a
 * sandbox / testing ground.
 */
internal class TestState : State
{
    Window _window;
    Text _text;
    TextButton _button;
	TextList _list;
    NumberText _number;
    SurfaceSet _set;
    Slider _slider;
    ComboBox _comboBox;
    int _i;

    /**
	 * Initializes all the elements in the test screen.
	 * @param game Pointer to the core game.
	 */
    internal TestState()
	{
		// Create objects
		_window = new Window(this, 300, 180, 10, 10);
		_text = new Text(280, 120, 20, 50);
		_button = new TextButton(100, 20, 110, 150);
		_list = new TextList(300, 180, 10, 10);
		_number = new NumberText(50, 5, 200, 25);
		_set = _game.getMod().getSurfaceSet("BASEBITS.PCK");
		_set.getFrame(1);
		_slider = new Slider(100, 15, 50, 50);
		_comboBox = new ComboBox(this, 80, 16, 98, 100);
		// Set palette
		setPalette("PAL_BASESCAPE", 2);

		add(_window);
		add(_button);
		add(_text);
		add(_list);
		add(_number);
		add(_slider);
		add(_comboBox);

		centerAllSurfaces();

		// Set up objects
		_window.setColor((byte)(Palette.blockOffset(15) + 1));
		_window.setBackground(_game.getMod().getSurface("BACK04.SCR"));

		_button.setColor((byte)(Palette.blockOffset(15) + 1));
		_button.setText("LOLOLOL");

		_text.setColor((byte)(Palette.blockOffset(15) + 1));
		//_text.setBig();
		_text.setWordWrap(true);
		_text.setAlign(TextHAlign.ALIGN_CENTER);
		_text.setVerticalAlign(TextVAlign.ALIGN_MIDDLE);
		//_text.setText(tr("STR_COUNCIL_TERMINATED"));

		_list.setColor((byte)(Palette.blockOffset(15) + 1));
		_list.setColumns(3, 100, 50, 100);
		_list.addRow(2, "a", "b");
		_list.addRow(3, "lol", "welp", "yo");
		_list.addRow(1, "0123456789");

		_number.setColor((byte)(Palette.blockOffset(15) + 1));
		_number.setValue(1234567890);

		_slider.setColor((byte)(Palette.blockOffset(15) + 1));

		var difficulty = new List<string>();
		for (int i = 0; i != 3; ++i)
		{
			difficulty.Add(tr("STR_1_BEGINNER"));
			difficulty.Add(tr("STR_2_EXPERIENCED"));
			difficulty.Add(tr("STR_3_VETERAN"));
			difficulty.Add(tr("STR_4_GENIUS"));
			difficulty.Add(tr("STR_5_SUPERHUMAN"));
		}

		_comboBox.setColor((byte)(Palette.blockOffset(15) + 1));
		_comboBox.setOptions(difficulty);

		_i = 0;

		//_game.getMod().getPalette("PAL_GEOSCAPE").savePal("../../../Geoscape.pal");
		//_game.getMod().getPalette("PAL_BASESCAPE").savePal("../../../Basescape.pal");
		//_game.getMod().getPalette("PAL_UFOPAEDIA").savePal("../../../Ufopaedia.pal");
		//_game.getMod().getPalette("PAL_BATTLESCAPE").savePal("../../../Battlescape.pal");

		//_game.getMod().getFont("FONT_BIG").fix("../../../Big.bmp", 256);
		//_game.getMod().getFont("FONT_SMALL").fix("../../../Small.bmp", 128);
	}

	~TestState() { }

	protected override void think()
	{
		base.think();

		/*
		_text->setText(tr(_i));
		_i++;
		*/
	}
}
