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

namespace SharpXcom.Engine;

delegate void StateHandler();
delegate void SurfaceHandler();

/**
 * Timer used to run code in fixed intervals.
 * Used for code that should run at the same fixed interval
 * in various machines, based on miliseconds instead of CPU cycles.
 */
internal class Timer
{
    uint _start;
    uint _frameSkipStart;
    int _interval;
    bool _running;
    bool _frameSkipping;
    StateHandler _state;
    SurfaceHandler _surface;
    internal static uint gameSlowSpeed = 1;
    internal static int maxFrameSkip;

    /**
     * Initializes a new timer with a set interval.
     * @param interval Time interval in milliseconds.
     * @param frameSkipping Use frameskipping.
     */
    internal Timer(uint interval, bool frameSkipping = false)
    {
        _start = 0;
        _interval = (int)interval;
        _running = false;
        _frameSkipping = frameSkipping;
        _state = null;
        _surface = null;

        maxFrameSkip = Options.maxFrameSkip;
    }

    /**
     *
     */
    ~Timer() { }

    /**
     * Sets a state function for the timer to call every interval.
     * @param handler Event handler.
     */
    internal void onTimer(StateHandler handler) =>
        _state = handler;

    /**
     * Sets a surface function for the timer to call every interval.
     * @param handler Event handler.
     */
    internal void onTimer(SurfaceHandler handler) =>
        _surface = handler;

    /**
     * Starts the timer running and counting time.
     */
    internal void start()
    {
        _frameSkipStart = _start = slowTick();
        _running = true;
    }

    /**
     * Stops the timer from running.
     */
    internal void stop()
    {
        _start = 0;
        _running = false;
    }

    const uint accurate = 4;
    static uint old_time = SDL_GetTicks();
    static ulong false_time = (ulong)old_time << (byte)accurate;
    uint slowTick()
    {
        ulong new_time = ((ulong)SDL_GetTicks()) << (byte)accurate;
        false_time += (new_time - old_time) / gameSlowSpeed;
        old_time = (uint)new_time;
        return (uint)(false_time >> (byte)accurate);
    }

    /**
     * Returns the time passed since the last interval.
     * @return Time in milliseconds.
     */
    internal uint getTime()
    {
	    if (_running)
	    {
		    return slowTick() - _start;
	    }
	    return 0;
    }

    /**
     * Changes the timer's interval to a new value.
     * @param interval Interval in milliseconds.
     */
    internal void setInterval(uint interval) =>
        _interval = (int)interval;

    /**
     * The timer keeps calculating the passed time while it's running,
     * calling the respective action handler whenever the set interval passes.
     * @param state State that the action handler belongs to.
     * @param surface Surface that the action handler belongs to.
     */
    internal void think(State state, Surface surface)
    {
        long now = slowTick(); // must be signed to permit negative numbers
        Game game = state != null ? State._game : null; // this is used to make sure we stop calling *_state on *state in the loop once *state has been popped and deallocated
        //assert(!game || game.isState(state));

        if (_running)
        {
            if ((now - _frameSkipStart) >= _interval)
            {
                for (int i = 0; i <= maxFrameSkip && isRunning() && (now - _frameSkipStart) >= _interval; ++i)
                {
                    if (state != null && _state != null)
                    {
                        _state();
                    }
                    _frameSkipStart += (uint)_interval;
                    // breaking here after one iteration effectively returns this function to its old functionality:
                    if (game == null || !_frameSkipping || !game.isState(state)) break; // if game isn't set, we can't verify *state
                }

                if (_running && surface != null && _surface != null)
                {
                    _surface();
                }
                _start = slowTick();
                if (_start > _frameSkipStart) _frameSkipStart = _start; // don't play animations in ffwd to catch up :P
            }
        }
    }

    /**
     * Returns if the timer has been started.
     * @return Running state.
     */
    internal bool isRunning() =>
	    _running;
}
