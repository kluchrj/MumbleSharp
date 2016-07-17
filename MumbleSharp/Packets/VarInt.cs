using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumbleSharp.Audio
{
    class VarInt
    {
        // Ported from https://github.com/layeh/gumble/blob/master/gumble/varint/write.go

        int Encode(ref byte[] b, int offset, int length, Int64 value)
        {
            // 111110__ + varint Negative recursive varint
            if (value < 0) {
                b[0] = 0xF8;

                return 1 + Encode(ref b, 1, b.Length, -value);
            }
            // 0xxxxxxx 7-bit positive number
            if (value <= 0x7F)
            {
                b[0] = (byte)value;

                return 1;
            }
            // 10xxxxxx + 1 byte 14-bit positive number
            if (value <= 0x3FFF)
            {
                b[0] = (byte)(((value >> 8) & 0x3F) | 0x80);

                b[1] = (byte)(value & 0xFF);

                return 2;
            }
            // 110xxxxx + 2 bytes 21-bit positive number
            if (value <= 0x1FFFFF)
            {
                b[0] = (byte)((value >> 16) & 0x1F | 0xC0);

                b[1] = (byte)((value >> 8) & 0xFF);

                b[2] = (byte)(value & 0xFF);

                return 3;
            }
            // 1110xxxx + 3 bytes 28-bit positive number
            if (value <= 0xFFFFFFF)
            {
                b[0] = (byte)((value >> 24) & 0xF | 0xE0);

                b[1] = (byte)((value >> 16) & 0xFF);

                b[2] = (byte)((value >> 8) & 0xFF);

                b[3] = (byte)(value & 0xFF);

                return 4;
            }
            // 111100__ + int (32-bit) 32-bit positive number
            if (value <= Int32.MaxValue)
            {
                b[0] = 0xF0;

                // Convert to big endian
                if (BitConverter.IsLittleEndian)
                    ReverseBytes((uint)value);

                byte[] num = BitConverter.GetBytes((uint)value);

                Buffer.BlockCopy(num, 0, b, 1, num.Length);

                return 5;
            }
            // 111101__ + long (64-bit) 64-bit number
            if (value <= Int64.MaxValue)
            {
                b[0] = 0xF4;

                // Convert to big endian
                if (BitConverter.IsLittleEndian)
                    ReverseBytes((ulong)value);

                byte[] num = BitConverter.GetBytes((ulong)value);

                Buffer.BlockCopy(num, 0, b, 1, num.Length);
                
                return 9;
            }

            return 0;
        }

        private static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        private static UInt64 ReverseBytes(UInt64 value)
        {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                   (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                   (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                   (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }
    }
}
