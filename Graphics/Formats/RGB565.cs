using System;
using txtrconvert.Impl.Graphics;
using txtrconvert.Util;

namespace txtrconvert.Graphics.Formats
{
    public class RGB565 : AConverter
    {
        public override int BitsPerPixel { get => 16; }

        public override int BlockWidth { get => 4; }

        public override int BlockHeight { get => 4; }

        public RGB565()
            : base(SizeLimit, SizeLimit, 0, 0, GX.TextureFormat.RGB565, GX.PaletteFormat.IA8)
        {
        }

        public RGB565(uint pWidth)
            : base(pWidth, SizeLimit, 0, 0, GX.TextureFormat.RGB565, GX.PaletteFormat.IA8)
        {
        }

        public RGB565(uint pWidth, uint pHeight)
            : base(pWidth, pHeight, 0, 0, GX.TextureFormat.RGB565, GX.PaletteFormat.IA8)
        {
        }

        public RGB565(uint pWidth, uint pHeight, ushort pPalWidth)
            : base(pWidth, pHeight, pPalWidth, 0, GX.TextureFormat.RGB565, GX.PaletteFormat.IA8)
        {
        }

        public RGB565(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, GX.TextureFormat.RGB565, GX.PaletteFormat.IA8)
        {
        }

        public RGB565(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight, GX.TextureFormat pTexFormt)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, pTexFormt, GX.PaletteFormat.IA8)
        {
        }

        public RGB565(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight, GX.TextureFormat pTexFormt, GX.PaletteFormat pPalFormat)
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
                            ushort pixel = Shared.Swap(BitConverter.ToUInt16(texData, inp++ * 2));

                            if (y1 >= height || x1 >= width)
                                continue;

                            int b = (((pixel >> 11) & 0x1F) << 3) & 0xff;
                            int g = (((pixel >> 5) & 0x3F) << 2) & 0xff;
                            int r = (((pixel >> 0) & 0x1F) << 3) & 0xff;

                            output[y1 * width + x1] = (uint)((r << 0) | (g << 8) | (b << 16) | (255 << 24));
                        }
                    }
                }
            }

            return Shared.ToByteArray(output);
        }

        public override byte[] FromWithPalette(in byte[] texData, in uint[] paletteData)
        {
            throw new NotSupportedException("RGB565 has no palette");
        }

        public override byte[] To(in uint[] pixeldata)
        {
            int z = -1;
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

                            if (y >= height || x >= width)
                                newpixel = 0;
                            else
                            {
                                uint rgba = pixeldata[x + (y * width)];

                                uint b = (rgba >> 16) & 0xff;
                                uint g = (rgba >> 8) & 0xff;
                                uint r = (rgba >> 0) & 0xff;

                                newpixel = (ushort)(((b >> 3) << 11) | ((g >> 2) << 5) | ((r >> 3) << 0));
                            }

                            output[++z] = (byte)(newpixel >> 8);
                            output[++z] = (byte)(newpixel & 0xff);
                        }
                    }
                }
            }

            return output;
        }

        public override byte[] ToWithPalette(in uint[] rgbaData, in uint[] palData)
        {
            throw new NotSupportedException("RGB565 has no palette");
        }
    }
}
