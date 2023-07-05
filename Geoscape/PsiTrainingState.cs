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

namespace SharpXcom.Geoscape;

/**
 * Screen shown monthly when the player has psi labs available.
 */
internal class PsiTrainingState : State
{
    Window _window;
    Text _txtTitle;
    TextButton _btnOk;
    List<Base> _bases;
    List<TextButton> _btnBases;

    /**
	 * Initializes all the elements in the Psi Training screen.
	 * @param game Pointer to the core game.
	 */
    internal PsiTrainingState()
	{
		// Create objects
		_window = new Window(this, 320, 200, 0, 0);
		_txtTitle = new Text(300, 17, 10, 16);
		_btnOk = new TextButton(160, 14, 80, 174);

		// Set palette
		setInterface("psiTraining");

		add(_window, "window", "psiTraining");
		add(_btnOk, "button2", "psiTraining");
		add(_txtTitle, "text", "psiTraining");

		// Set up objects
		_window.setBackground(_game.getMod().getSurface("BACK01.SCR"));

		_btnOk.setText(tr("STR_OK"));
		_btnOk.onMouseClick(btnOkClick);
		_btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);

		_txtTitle.setBig();
		_txtTitle.setAlign(TextHAlign.ALIGN_CENTER);
		_txtTitle.setText(tr("STR_PSIONIC_TRAINING"));

		int buttons = 0;
		foreach (var b in _game.getSavedGame().getBases())
		{
			if (b.getAvailablePsiLabs() != 0)
			{
				TextButton btnBase = new TextButton(160, 14, 80, 40 + 16 * buttons);
				btnBase.onMouseClick(btnBaseXClick);
				btnBase.setText(b.getName());
				add(btnBase, "button1", "psiTraining");
				_bases.Add(b);
				_btnBases.Add(btnBase);
				++buttons;
				if (buttons >= 8)
				{
					break;
				}
			}
		}

		centerAllSurfaces();
	}

	/**
	 *
	 */
	~PsiTrainingState() { }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Engine.Action _) =>
        _game.popState();

    /**
     * Goes to the allocation screen for the corresponding base.
     * @param action Pointer to an action.
     */
    void btnBaseXClick(Engine.Action action)
    {
        for (int i = 0; i < _btnBases.Count; ++i)
        {
            if (action.getSender() == _btnBases[i])
            {
                _game.pushState(new AllocatePsiTrainingState(_bases[i]));
                break;
            }
        }
    }
}
