using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

namespace txtrconvert.Util
{
    public static class Shared
    {
        public static class StaticMembers
        {
            public static readonly string NewLine = Environment.NewLine;
        }

        public static int Align(int n, int multiple)
        {
            int mask = (multiple - 1);
            return (n + mask) & ~mask;
        }

        /// <summary>
        /// Pads the given value to a multiple of the given padding value, default padding value is 64.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long AddPadding(long value)
        {
            return AddPadding(value, 64);
        }

        /// <summary>
        /// Pads the given value to a multiple of the given padding value, default padding value is 64.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static long AddPadding(long value, int padding)
        {
            if (value % padding != 0)
            {
                value = value + (padding - (value % padding));
            }

            return value;
        }

        /// <summary>
        /// Pads the given value to a multiple of the given padding value, default padding value is 64.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int AddPadding(int value)
        {
            return AddPadding(value, 64);
        }

        /// <summary>
        /// Pads the given value to a multiple of the given padding value, default padding value is 64.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static int AddPadding(int value, int padding)
        {
            if (value % padding != 0)
            {
                value = value + (padding - (value % padding));
            }

            return value;
        }

        /// <summary>
        /// Swaps endianness.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ushort Swap(ushort value)
        {
            return (ushort)IPAddress.HostToNetworkOrder((short)value);
        }

        /// <summary>
        /// Swaps endianness.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static uint Swap(uint value)
        {
            return (uint)IPAddress.HostToNetworkOrder((int)value);
        }

        /// <summary>
        /// Swaps endianness
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ulong Swap(ulong value)
        {
            return (ulong)IPAddress.HostToNetworkOrder((long)value);
        }

        /// <summary>
        /// Turns a ushort array into a byte array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(ushort[] array)
        {
            List<byte> results = new List<byte>();
            foreach (ushort value in array)
            {
                byte[] converted = BitConverter.GetBytes(value);
                results.AddRange(converted);
            }
            return results.ToArray();
        }

        /// <summary>
        /// Turns a uint array into a byte array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(uint[] array)
        {
            List<byte> results = new List<byte>();
            foreach (uint value in array)
            {
                byte[] converted = BitConverter.GetBytes(value);
                results.AddRange(converted);
            }
            return results.ToArray();
        }

        /// <summary>
        /// Turns a byte array into a uint array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static uint[] ToUIntArray(byte[] array)
        {
            UInt32[] converted = new UInt32[array.Length / 4];
            int j = 0;

            for (int i = 0; i < array.Length; i += 4)
                converted[j++] = BitConverter.ToUInt32(array, i);

            return converted;
        }

        /// <summary>
        /// Turns a byte array into a ushort array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static ushort[] ToUShortArray(byte[] array)
        {
            ushort[] converted = new ushort[array.Length / 2];
            int j = 0;

            for (int i = 0; i < array.Length; i += 2)
                converted[j++] = BitConverter.ToUInt16(array, i);

            return converted;
        }
    }
}
