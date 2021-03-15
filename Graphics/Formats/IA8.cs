using System;
using txtrconvert.Impl.Graphics;
using txtrconvert.Util;

namespace txtrconvert.Graphics.Formats
{
    public class IA8 : AConverter
    {
        public override int BitsPerPixel { get => 16; }

        public override int BlockWidth { get => 4; }

        public override int BlockHeight { get => 4; }

        public IA8()
            : base(SizeLimit, SizeLimit, 0, 0, GX.TextureFormat.IA8, GX.PaletteFormat.IA8)
        {
        }

        public IA8(uint pWidth)
            : base(pWidth, SizeLimit, 0, 0, GX.TextureFormat.IA8, GX.PaletteFormat.IA8)
        {
        }

        public IA8(uint pWidth, uint pHeight)
            : base(pWidth, pHeight, 0, 0, GX.TextureFormat.IA8, GX.PaletteFormat.IA8)
        {
        }

        public IA8(uint pWidth, uint pHeight, ushort pPalWidth)
            : base(pWidth, pHeight, pPalWidth, 0, GX.TextureFormat.IA8, GX.PaletteFormat.IA8)
        {
        }

        public IA8(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, GX.TextureFormat.IA8, GX.PaletteFormat.IA8)
        {
        }

        public IA8(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight, GX.TextureFormat pTexFormt)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, pTexFormt, GX.PaletteFormat.IA8)
        {
        }

        public IA8(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight, GX.TextureFormat pTexFormt, GX.PaletteFormat pPalFormat)
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
                    for (int y1 = y; y1 < y + 4; y1++)
                    {
                        for (int x1 = x; x1 < x + 4; x1++)
                        {
                            int pixel = Shared.Swap(BitConverter.ToUInt16(texData, inp++ * 2));

                            if (y1 >= height || x1 >= width)
                                continue;

                            uint a = (uint)(pixel >> 8);
                            uint i = (uint)(pixel & 0xff);

                            output[y1 * width + x1] = (i << 0) | (i << 8) | (i << 16) | (a << 24);
                        }
                    }
                }
            }

            return Shared.ToByteArray(output);
        }

        public override byte[] FromWithPalette(in byte[] texData, in uint[] paletteData)
        {
            throw new NotSupportedException("IA8 has no palette");
        }

        public override byte[] To(in uint[] pixeldata)
        {
            int inp = 0;
            byte[] output = new byte[Shared.AddPadding(width, 4) * Shared.AddPadding(height, 4) * 2];

            for (int y1 = 0; y1 < height; y1 += 4)
            {
                for (int x1 = 0; x1 < width; x1 += 4)
                {
                    for (int y = y1; y < y1 + 4; y++)
                    {
                        for (int x = x1; x < x1 + 4; x++)
                        {
                            ushort newpixel;

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

                                newpixel = (ushort)((a << 8) | i);
                            }

                            byte[] temp = BitConverter.GetBytes(newpixel);
                            Array.Reverse(temp);

                            output[inp++] = (byte)(newpixel >> 8);
                            output[inp++] = (byte)(newpixel & 0xff);
                        }
                    }
                }
            }

            return output;
        }

        public override byte[] ToWithPalette(in uint[] rgbaData, in uint[] palData)
        {
            throw new NotSupportedException("IA8 has no palette");
        }
    }
}
