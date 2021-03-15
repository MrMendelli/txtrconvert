using System;
using txtrconvert.Impl.Graphics;
using txtrconvert.Util;

namespace txtrconvert.Graphics.Formats
{
    public class RGB5A3 : AConverter
    {
        public override int BitsPerPixel { get => 16; }

        public override int BlockWidth { get => 4; }

        public override int BlockHeight { get => 4; }

        public RGB5A3()
            : base(SizeLimit, SizeLimit, 0, 0, GX.TextureFormat.RGB5A3, GX.PaletteFormat.IA8)
        {
        }

        public RGB5A3(uint pWidth)
            : base(pWidth, SizeLimit, 0, 0, GX.TextureFormat.RGB5A3, GX.PaletteFormat.IA8)
        {
        }

        public RGB5A3(uint pWidth, uint pHeight)
            : base(pWidth, pHeight, 0, 0, GX.TextureFormat.RGB5A3, GX.PaletteFormat.IA8)
        {
        }

        public RGB5A3(uint pWidth, uint pHeight, ushort pPalWidth)
            : base(pWidth, pHeight, pPalWidth, 0, GX.TextureFormat.RGB5A3, GX.PaletteFormat.IA8)
        {
        }

        public RGB5A3(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, GX.TextureFormat.RGB5A3, GX.PaletteFormat.IA8)
        {
        }

        public RGB5A3(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight, GX.TextureFormat pTexFormt)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, pTexFormt, GX.PaletteFormat.IA8)
        {
        }

        public RGB5A3(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight, GX.TextureFormat pTexFormt, GX.PaletteFormat pPalFormat)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, pTexFormt, pPalFormat)
        {
        }

        public override byte[] From(in byte[] texData)
        {
            uint[] output = new uint[width * height];
            int inp = 0;
            int r, g, b;
            int a = 0;

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

                            if ((pixel & (1 << 15)) != 0)
                            {
                                b = (((pixel >> 10) & 0x1F) * 255) / 31;
                                g = (((pixel >> 5) & 0x1F) * 255) / 31;
                                r = (((pixel >> 0) & 0x1F) * 255) / 31;
                                a = 255;
                            }
                            else
                            {
                                a = (((pixel >> 12) & 0x07) * 255) / 7;
                                b = (((pixel >> 8) & 0x0F) * 255) / 15;
                                g = (((pixel >> 4) & 0x0F) * 255) / 15;
                                r = (((pixel >> 0) & 0x0F) * 255) / 15;
                            }

                            output[(y1 * width) + x1] = (uint)((r << 0) | (g << 8) | (b << 16) | (a << 24));
                        }
                    }
                }
            }

            return Shared.ToByteArray(output);
        }

        public override byte[] FromWithPalette(in byte[] texData, in uint[] paletteData)
        {
            throw new NotSupportedException("RGB5A3 has no palette");
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
                            int newpixel;

                            if (y >= height || x >= width)
                                newpixel = 0;
                            else
                            {
                                int rgba = (int)pixeldata[x + (y * width)];
                                newpixel = 0;

                                int r = (rgba >> 16) & 0xff;
                                int g = (rgba >> 8) & 0xff;
                                int b = (rgba >> 0) & 0xff;
                                int a = (rgba >> 24) & 0xff;

                                if (a <= 0xda) //RGB4A3
                                {
                                    newpixel &= ~(1 << 15);

                                    r = ((r * 15) / 255) & 0xf;
                                    g = ((g * 15) / 255) & 0xf;
                                    b = ((b * 15) / 255) & 0xf;
                                    a = ((a * 7) / 255) & 0x7;

                                    newpixel |= (a << 12) | (r << 8) | (g << 4) | b;
                                }
                                else //RGB5
                                {
                                    newpixel |= (1 << 15);

                                    r = ((r * 31) / 255) & 0x1f;
                                    g = ((g * 31) / 255) & 0x1f;
                                    b = ((b * 31) / 255) & 0x1f;

                                    newpixel |= (r << 10) | (g << 5) | b;
                                }
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
            throw new NotSupportedException("RGB5A3 has no palette");
        }
    }
}
