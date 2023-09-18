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

namespace SharpXcom.Battlescape;

/**
 * A class that represents an explosion animation. Map is the owner of an instance of this class during its short life.
 * It represents both a bullet hit, as a real explosion animation.
 */
internal class Explosion
{
    const int HIT_FRAMES = 4;
    internal const int EXPLODE_FRAMES = 8;
    const int BULLET_FRAMES = 10;

	Position _position;
	int _currentFrame, _startFrame, _frameDelay;
	bool _big, _hit;

    /**
     * Sets up a Explosion sprite with the specified size and position.
     * @param position Explosion center position in voxel x/y/z.
     * @param startFrame A startframe - can be used to offset different explosions at different frames.
     * @param big Flag to indicate it is a bullet hit (false), or a real explosion (true).
     * @param hit True for melee and psi attacks.
     */
    internal Explosion(Position position, int startFrame, int frameDelay = 0, bool big = false, bool hit = false)
    {
        _position = position;
        _currentFrame = startFrame;
        _startFrame = startFrame;
        _frameDelay = frameDelay;
        _big = big;
        _hit = hit;
    }

    /**
     * Deletes the Explosion.
     */
    ~Explosion() { }

    /**
     * Animates the explosion further.
     * @return false If the animation is finished.
     */
    internal bool animate()
    {
	    if (_frameDelay > 0)
	    {
		    _frameDelay--;
		    return true;
	    }

	    _currentFrame++;
	    if ((_hit && _currentFrame == _startFrame + HIT_FRAMES) ||
		    (_big && _currentFrame == _startFrame + EXPLODE_FRAMES) ||
		    (!_big && !_hit && _currentFrame == _startFrame + BULLET_FRAMES))
	    {
		    return false;
	    }
	    else
	    {
		    return true;
	    }
    }
}
