using System;
using txtrconvert.Impl.Graphics;
using txtrconvert.Util;

namespace txtrconvert.Graphics.Formats
{
    public class RGBA32 : AConverter
    {
        public override int BitsPerPixel { get => 32; }

        public override int BlockWidth { get => 4; }

        public override int BlockHeight { get => 4; }

        public RGBA32()
            : base(SizeLimit, SizeLimit, 0, 0, GX.TextureFormat.RGBA32, GX.PaletteFormat.IA8)
        {
        }

        public RGBA32(uint pWidth)
            : base(pWidth, SizeLimit, 0, 0, GX.TextureFormat.RGBA32, GX.PaletteFormat.IA8)
        {
        }

        public RGBA32(uint pWidth, uint pHeight)
            : base(pWidth, pHeight, 0, 0, GX.TextureFormat.RGBA32, GX.PaletteFormat.IA8)
        {
        }

        public RGBA32(uint pWidth, uint pHeight, ushort pPalWidth)
            : base(pWidth, pHeight, pPalWidth, 0, GX.TextureFormat.RGBA32, GX.PaletteFormat.IA8)
        {
        }

        public RGBA32(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, GX.TextureFormat.RGBA32, GX.PaletteFormat.IA8)
        {
        }

        public RGBA32(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight, GX.TextureFormat pTexFormt)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, pTexFormt, GX.PaletteFormat.IA8)
        {
        }

        public RGBA32(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight, GX.TextureFormat pTexFormt, GX.PaletteFormat pPalFormat)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, pTexFormt, pPalFormat)
        {
        }

        public override byte[] From(in byte[] texData)
        {
            uint[] output = new uint[width * height];
            int inp = 0;

            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        for (int y1 = y; y1 < y + 4; y1++)
                        {
                            for (int x1 = x; x1 < x + 4; x1++)
                            {
                                ushort pixel = Shared.Swap(BitConverter.ToUInt16(texData, inp++ * 2));

                                if ((x1 >= width) || (y1 >= height))
                                    continue;

                                if (k == 0)
                                {
                                    int a = (pixel >> 8) & 0xff;
                                    int r = (pixel >> 0) & 0xff;
                                    output[x1 + (y1 * width)] |= (uint)((r << 16) | (a << 24));
                                }
                                else
                                {
                                    int g = (pixel >> 8) & 0xff;
                                    int b = (pixel >> 0) & 0xff;
                                    output[x1 + (y1 * width)] |= (uint)((g << 8) | (b << 0));
                                }
                            }
                        }
                    }
                }
            }

            return Shared.ToByteArray(output);
        }

        public override byte[] FromWithPalette(in byte[] texData, in uint[] paletteData)
        {
            throw new NotSupportedException("RGBA32 has no palette");
        }

        public override byte[] To(in uint[] pixeldata)
        {
            int z = 0, iv = 0;
            byte[] output = new byte[Shared.AddPadding(width, 4) * Shared.AddPadding(height, 4) * 4];
            uint[] lr = new uint[32], lg = new uint[32], lb = new uint[32], la = new uint[32];

            for (int y1 = 0; y1 < height; y1 += 4)
            {
                for (int x1 = 0; x1 < width; x1 += 4)
                {
                    for (int y = y1; y < (y1 + 4); y++)
                    {
                        for (int x = x1; x < (x1 + 4); x++)
                        {
                            uint rgba;

                            if (y >= height || x >= width)
                                rgba = 0;
                            else
                                rgba = pixeldata[x + (y * width)];

                            lr[z] = (uint)(rgba >> 16) & 0xff;
                            lg[z] = (uint)(rgba >> 8) & 0xff;
                            lb[z] = (uint)(rgba >> 0) & 0xff;
                            la[z] = (uint)(rgba >> 24) & 0xff;

                            z++;
                        }
                    }

                    if (z == 16)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            output[iv++] = (byte)(la[i]);
                            output[iv++] = (byte)(lr[i]);
                        }
                        for (int i = 0; i < 16; i++)
                        {
                            output[iv++] = (byte)(lg[i]);
                            output[iv++] = (byte)(lb[i]);
                        }

                        z = 0;
                    }
                }
            }

            return output;
        }

        public override byte[] ToWithPalette(in uint[] rgbaData, in uint[] palData)
        {
            throw new NotSupportedException("RGBA32 has no palette");
        }
    }
}
