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

/**
 * Random Number Generator used throughout the game
 * for all your randomness needs. Uses a 64-bit xorshift
 * pseudorandom number generator.
 */
internal class RNG
{
    /* This is a good generator if you're short on memory, but otherwise we
       rather suggest to use a xorshift128+ (for maximum speed) or
       xorshift1024* (for speed and very long period) generator. */

    static ulong x = (ulong)new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds(); /* The state must be seeded with a nonzero value. */

    static ulong next()
    {
        x ^= x >> 12; // a
        x ^= x << 25; // b
        x ^= x >> 27; // c
        return x * 2685821657736338717UL;
    }

    /**
     * Generates a random integer number within a certain range.
     * @param min Minimum number, inclusive.
     * @param max Maximum number, inclusive.
     * @return Generated number.
     */
    internal static int generate(int min, int max)
    {
        ulong num = next();
        return (int)(num % (ulong)(max - min + 1) + (ulong)min);
    }

    /**
     * Generates a random percent chance of an event occurring,
     * and returns the result
     * @param value Value percentage (0-100%)
     * @return True if the chance succeeded.
     */
    internal static bool percent(int value) =>
        (generate(0, 99) < value);

    /**
     * Generates a random integer number within a certain range.
     * Distinct from "generate" in that it doesn't touch the seed.
     * @param min Minimum number, inclusive.
     * @param max Maximum number, inclusive.
     * @return Generated number.
     */
    internal static int seedless(int min, int max) =>
        (new Random().Next() % (max - min + 1) + min);

    /**
     * Returns the current seed in use by the generator.
     * @return Current seed.
     */
    internal static ulong getSeed() =>
        x;

    /**
     * Generates a random decimal number within a certain range.
     * @param min Minimum number.
     * @param max Maximum number.
     * @return Generated number.
     */
    internal static double generate(double min, double max)
    {
        double num = next();
        return (num / ((double)ulong.MaxValue / (max - min)) + min);
    }

	/// Shuffles a list randomly.
	/**
	 * Randomly changes the orders of the elements in a list.
	 * @param list The container to randomize.
	 */
	internal static void shuffle<T>(List<T> list)
	{
		if (!list.Any())
			return;
        for (int i = list.Count - 1; i > 0; --i)
            (list[generate(0, i)], list[i]) = (list[i], list[generate(0, i)]);
    }

    /**
     * Changes the current seed in use by the generator.
     * @param n New seed.
     */
    internal static void setSeed(ulong n) =>
	    x = n;
}
