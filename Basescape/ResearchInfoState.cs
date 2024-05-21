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
 * Window which allows changing of the number of assigned scientist to a project.
 */
internal class ResearchInfoState : State
{
    Base _base;
    ResearchProject _project;
    RuleResearch _rule;
    Timer _timerMore, _timerLess;
    Window _window;
    Text _txtTitle, _txtAvailableScientist, _txtAvailableSpace, _txtAllocatedScientist, _txtMore, _txtLess;
    TextButton _btnCancel;
    TextButton _btnOk;
    ArrowButton _btnMore, _btnLess;
    InteractiveSurface _surfaceScientists;

    /**
     * Initializes all the elements in the ResearchProject screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param rule A RuleResearch which will be used to create a new ResearchProject
     */
    internal ResearchInfoState(Base @base, RuleResearch rule)
    {
        _base = @base;
        _project = new ResearchProject(rule, (int)rule.getCost() * RNG.generate(50, 150) / 100);
        _rule = rule;

        buildUi();
    }

    /**
     * Initializes all the elements in the ResearchProject screen.
     * @param game Pointer to the core game.
     * @param base Pointer to the base to get info from.
     * @param project A ResearchProject to modify
     */
    internal ResearchInfoState(Base @base, ResearchProject project)
    {
        _base = @base;
        _project = project;
        _rule = null;

        buildUi();
    }

    /**
     * Frees up memory that's not automatically cleaned on exit
     */
    ~ResearchInfoState()
    {
        _timerLess = null;
        _timerMore = null;
    }

    /**
     * Builds dialog.
     */
    void buildUi()
    {
        _screen = false;

        _window = new Window(this, 230, 140, 45, 30);
        _txtTitle = new Text(210, 17, 61, 40);

        _txtAvailableScientist = new Text(210, 9, 61, 60);
        _txtAvailableSpace = new Text(210, 9, 61, 70);
        _txtAllocatedScientist = new Text(210, 17, 61, 80);
        _txtMore = new Text(110, 17, 85, 100);
        _txtLess = new Text(110, 17, 85, 120);
        _btnCancel = new TextButton(90, 16, 61, 145);
        _btnOk = new TextButton(90, 16, 169, 145);

        _btnMore = new ArrowButton(ArrowShape.ARROW_BIG_UP, 13, 14, 195, 100);
        _btnLess = new ArrowButton(ArrowShape.ARROW_BIG_DOWN, 13, 14, 195, 120);

        _surfaceScientists = new InteractiveSurface(230, 140, 45, 30);
        _surfaceScientists.onMouseClick(handleWheel, 0);

        // Set palette
        setInterface("allocateResearch");

        add(_surfaceScientists);
        add(_window, "window", "allocateResearch");
        add(_btnOk, "button2", "allocateResearch");
        add(_btnCancel, "button2", "allocateResearch");
        add(_txtTitle, "text", "allocateResearch");
        add(_txtAvailableScientist, "text", "allocateResearch");
        add(_txtAvailableSpace, "text", "allocateResearch");
        add(_txtAllocatedScientist, "text", "allocateResearch");
        add(_txtMore, "text", "allocateResearch");
        add(_txtLess, "text", "allocateResearch");
        add(_btnMore, "button1", "allocateResearch");
        add(_btnLess, "button1", "allocateResearch");

        centerAllSurfaces();

        // Set up objects
        _window.setBackground(_game.getMod().getSurface("BACK05.SCR"));

        _txtTitle.setBig();

        _txtTitle.setText(_rule != null ? tr(_rule.getName()) : tr(_project.getRules().getName()));

        _txtAllocatedScientist.setBig();

        _txtMore.setText(tr("STR_INCREASE"));
        _txtLess.setText(tr("STR_DECREASE"));

        _txtMore.setBig();
        _txtLess.setBig();

        if (_rule != null)
        {
            _base.addResearch(_project);
            if (_rule.needItem() && _rule.destroyItem())
            {
                _base.getStorageItems().removeItem(_rule.getName(), 1);
            }
        }
        setAssignedScientist();
        _btnMore.onMousePress(morePress);
        _btnMore.onMouseRelease(moreRelease);
        _btnMore.onMouseClick(moreClick, 0);
        _btnLess.onMousePress(lessPress);
        _btnLess.onMouseRelease(lessRelease);
        _btnLess.onMouseClick(lessClick, 0);

        _timerMore = new Timer(250);
        _timerMore.onTimer((StateHandler)more);
        _timerLess = new Timer(250);
        _timerLess.onTimer((StateHandler)less);

        _btnOk.onMouseClick(btnOkClick);
        _btnOk.onKeyboardPress(btnOkClick, Options.keyOk);
        if (_rule != null)
        {
            _btnOk.setText(tr("STR_START_PROJECT"));
            _btnCancel.setText(tr("STR_CANCEL_UC"));
            _btnCancel.onKeyboardPress(btnCancelClick, Options.keyCancel);
        }
        else
        {
            _btnOk.setText(tr("STR_OK"));
            _btnCancel.setText(tr("STR_CANCEL_PROJECT"));
            _btnOk.onKeyboardPress(btnOkClick, Options.keyCancel);
        }
        _btnCancel.onMouseClick(btnCancelClick);
    }

    /**
     * Increases or decreases the scientists according the mouse-wheel used.
     * @param action Pointer to an Action.
     */
    void handleWheel(Action action)
    {
        if (action.getDetails().wheel.y > 0) moreByValue(Options.changeValueByMouseWheel);
        else if (action.getDetails().wheel.y < 0) lessByValue(Options.changeValueByMouseWheel);
    }

    /**
     * Starts the timeMore timer.
     * @param action Pointer to an Action.
     */
    void morePress(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_LEFT) _timerMore.start();
    }

    /**
     * Stops the timeMore timer.
     * @param action Pointer to an Action.
     */
    void moreRelease(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            _timerMore.setInterval(250);
            _timerMore.stop();
        }
    }

    /**
     * Allocates scientists to the current project;
     * one scientist on left-click, all scientists on right-click.
     * @param action Pointer to an Action.
     */
    void moreClick(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_RIGHT)
            moreByValue(int.MaxValue);
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
            moreByValue(1);
    }

    /**
     * Starts the timeLess timer.
     * @param action Pointer to an Action.
     */
    void lessPress(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_LEFT) _timerLess.start();
    }

    /**
     * Stops the timeLess timer.
     * @param action Pointer to an Action.
     */
    void lessRelease(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
        {
            _timerLess.setInterval(250);
            _timerLess.stop();
        }
    }

    /**
     * Removes scientists from the current project;
     * one scientist on left-click, all scientists on right-click.
     * @param action Pointer to an Action.
     */
    void lessClick(Action action)
    {
        if (action.getDetails().button.button == SDL_BUTTON_RIGHT)
            lessByValue(int.MaxValue);
        if (action.getDetails().button.button == SDL_BUTTON_LEFT)
            lessByValue(1);
    }

    /**
     * Adds one scientist to the project if possible.
     */
    void more()
    {
        _timerMore.setInterval(50);
        moreByValue(1);
    }

    /**
     * Removes one scientist from the project if possible.
     */
    void less()
    {
        _timerLess.setInterval(50);
        lessByValue(1);
    }

    /**
     * Returns to the previous screen.
     * @param action Pointer to an action.
     */
    void btnOkClick(Action _) =>
        _game.popState();

    /**
     * Returns to the previous screen, removing the current project from the active
     * research list.
     * @param action Pointer to an action.
     */
    void btnCancelClick(Action _)
    {
        _base.removeResearch(_project);
        _game.popState();
    }

    /**
     * Adds the given number of scientists to the project if possible.
     * @param change Number of scientists to add.
     */
    void moreByValue(int change)
    {
        if (0 >= change) return;
        int freeScientist = _base.getAvailableScientists();
        int freeSpaceLab = _base.getFreeLaboratories();
        if (freeScientist > 0 && freeSpaceLab > 0)
        {
            change = Math.Min(Math.Min(freeScientist, freeSpaceLab), change);
            _project.setAssigned(_project.getAssigned() + change);
            _base.setScientists(_base.getScientists() - change);
            setAssignedScientist();
        }
    }

    /**
     * Removes the given number of scientists from the project if possible.
     * @param change Number of scientists to subtract.
     */
    void lessByValue(int change)
    {
        if (0 >= change) return;
        int assigned = _project.getAssigned();
        if (assigned > 0)
        {
            change = Math.Min(assigned, change);
            _project.setAssigned(assigned - change);
            _base.setScientists(_base.getScientists() + change);
            setAssignedScientist();
        }
    }

    /**
     * Updates count of assigned/free scientists and available lab space.
     */
    void setAssignedScientist()
    {
        _txtAvailableScientist.setText(tr("STR_SCIENTISTS_AVAILABLE_UC").arg(_base.getAvailableScientists()));
        _txtAvailableSpace.setText(tr("STR_LABORATORY_SPACE_AVAILABLE_UC").arg(_base.getFreeLaboratories()));
        _txtAllocatedScientist.setText(tr("STR_SCIENTISTS_ALLOCATED").arg(_project.getAssigned()));
    }

    /**
     * Runs state functionality every cycle (used to update the timer).
     */
    internal override void think()
    {
	    base.think();

	    _timerLess.think(this, null);
	    _timerMore.think(this, null);
    }
}
