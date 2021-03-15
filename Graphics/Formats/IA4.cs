using System;
using txtrconvert.Impl.Graphics;
using txtrconvert.Util;

namespace txtrconvert.Graphics.Formats
{
    public class IA4 : AConverter
    {
        public override int BitsPerPixel { get => 8; }

        public override int BlockWidth { get => 8; }

        public override int BlockHeight { get => 4; }

        public IA4()
            : base(SizeLimit, SizeLimit, 0, 0, GX.TextureFormat.IA4, GX.PaletteFormat.IA8)
        {
        }

        public IA4(uint pWidth)
            : base(pWidth, SizeLimit, 0, 0, GX.TextureFormat.IA4, GX.PaletteFormat.IA8)
        {
        }

        public IA4(uint pWidth, uint pHeight)
            : base(pWidth, pHeight, 0, 0, GX.TextureFormat.IA4, GX.PaletteFormat.IA8)
        {
        }

        public IA4(uint pWidth, uint pHeight, ushort pPalWidth)
            : base(pWidth, pHeight, pPalWidth, 0, GX.TextureFormat.IA4, GX.PaletteFormat.IA8)
        {
        }

        public IA4(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, GX.TextureFormat.IA4, GX.PaletteFormat.IA8)
        {
        }

        public IA4(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight, GX.TextureFormat pTexFormt)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, pTexFormt, GX.PaletteFormat.IA8)
        {
        }

        public IA4(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight, GX.TextureFormat pTexFormt, GX.PaletteFormat pPalFormat)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, pTexFormt, pPalFormat)
        {
        }

        public override byte[] From(in byte[] texData)
        {
            uint[] output = new uint[width * height];
            int inp = 0;

            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 8)
                {
                    for (int y1 = y; y1 < y + 4; y1++)
                    {
                        for (int x1 = x; x1 < x + 8; x1++)
                        {
                            int pixel = texData[inp++];

                            if (y1 >= height || x1 >= width)
                                continue;

                            int i = ((pixel & 0x0F) * 255 / 15) & 0xff;
                            int a = (((pixel >> 4) * 255) / 15) & 0xff;

                            output[y1 * width + x1] = (uint)((i << 0) | (i << 8) | (i << 16) | (a << 24));
                        }
                    }
                }
            }

            return Shared.ToByteArray(output);
        }

        public override byte[] FromWithPalette(in byte[] texData, in uint[] paletteData)
        {
            throw new NotSupportedException("IA4 has no palette");
        }

        public override byte[] To(in uint[] pixeldata)
        {
            int inp = 0;
            byte[] output = new byte[Shared.AddPadding(width, 8) * Shared.AddPadding(height, 4)];

            for (int y1 = 0; y1 < height; y1 += 4)
            {
                for (int x1 = 0; x1 < width; x1 += 8)
                {
                    for (int y = y1; y < y1 + 4; y++)
                    {
                        for (int x = x1; x < x1 + 8; x++)
                        {
                            byte newpixel;

                            if (x >= width || y >= height)
                                newpixel = 0;
                            else
                            {
                                uint rgba = pixeldata[x + (y * width)];

                                uint r = (rgba >> 0) & 0xff;
                                uint g = (rgba >> 8) & 0xff;
                                uint b = (rgba >> 16) & 0xff;

                                uint i = ((r + g + b) / 3) & 0xff;
                                uint a = (rgba >> 24) & 0xff;

                                newpixel = (byte)((((i * 15) / 255) & 0xf) | (((a * 15) / 255) << 4));
                            }

                            output[inp++] = newpixel;
                        }
                    }
                }
            }

            return output;
        }

        public override byte[] ToWithPalette(in uint[] rgbaData, in uint[] palData)
        {
            throw new NotSupportedException("IA4 has no palette");
        }
    }
}
