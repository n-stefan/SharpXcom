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

namespace SharpXcom.Savegame;

internal class SerializationHelper
{
    internal static void serializeInt(ref Span<byte> buffer, byte sizeKey, int value)
    {
        /* The C spec explicitly requires *(Type*) pointer accesses to be
         * sizeof(Type) aligned, which is not guaranteed by the UInt8** buffer
         * passed in here.
         * memcpy() is explicitly designed to cope with any address alignment, so
         * use that to avoid undefined behaviour */
        switch (sizeKey)
        {
            case 1:
                Debug.Assert(value < 256);
                buffer[0] = (byte)value;
                break;
            case 2:
                short s16Value = (short)value;
                Debug.Assert(value < 65536);
                BitConverter.TryWriteBytes(buffer, s16Value);
                break;
            case 3:
                Debug.Assert(false); // no.
                break;
            case 4:
                uint u32Value = (uint)value;
                BitConverter.TryWriteBytes(buffer, u32Value);
                break;
            default:
                Debug.Assert(false); // get out.
                break;
        }

        buffer = buffer.Slice(sizeKey);
    }
}
