using System;
using System.Runtime.InteropServices;

namespace txtrconvert.DataSpec
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TXTR
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint texFormat;

        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort width;

        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort height;

        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint mipCount;

        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint palFormat;

        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort palWidth;

        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort palHeight;

        public uint[] palData;

        public byte[] texData;
    }
}
