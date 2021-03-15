using System;
using txtrconvert.Impl.Graphics;
using txtrconvert.Util;

namespace txtrconvert.Graphics.Formats
{
    public class C8 : AConverter
    {
        public override int BitsPerPixel { get => 8; }

        public override int BlockWidth { get => 8; }

        public override int BlockHeight { get => 4; }

        public C8()
            : base(SizeLimit, SizeLimit, 0, 0, GX.TextureFormat.C8, GX.PaletteFormat.IA8)
        {
        }

        public C8(uint pWidth)
            : base(pWidth, SizeLimit, 0, 0, GX.TextureFormat.C8, GX.PaletteFormat.IA8)
        {
        }

        public C8(uint pWidth, uint pHeight)
            : base(pWidth, pHeight, 0, 0, GX.TextureFormat.C8, GX.PaletteFormat.IA8)
        {
        }

        public C8(uint pWidth, uint pHeight, ushort pPalWidth)
            : base(pWidth, pHeight, pPalWidth, 0, GX.TextureFormat.C8, GX.PaletteFormat.IA8)
        {
        }

        public C8(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, GX.TextureFormat.C8, GX.PaletteFormat.IA8)
        {
        }

        public C8(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight, GX.TextureFormat pTexFormt)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, pTexFormt, GX.PaletteFormat.IA8)
        {
        }

        public C8(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight, GX.TextureFormat pTexFormt, GX.PaletteFormat pPalFormat)
            : base(pWidth, pHeight, pPalWidth, pPalHeight, pTexFormt, pPalFormat)
        {
        }

        public override byte[] From(in byte[] texData)
        {
            throw new NotSupportedException("C8 requires a palette");
        }

        public override byte[] FromWithPalette(in byte[] texData, in uint[] paletteData)
        {
            uint[] output = new uint[width * height];
            int i = 0;

            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 8)
                {
                    for (int y1 = y; y1 < y + 4; y1++)
                    {
                        for (int x1 = x; x1 < x + 8; x1++)
                        {
                            ushort pixel = texData[i++];

                            if (y1 >= height || x1 >= width)
                                continue;

                            output[y1 * width + x1] = paletteData[pixel];
                        }
                    }
                }
            }

            return Shared.ToByteArray(output);
        }

        public override byte[] To(in uint[] pixeldata)
        {
            throw new NotSupportedException("C8 requires a palette");
        }

        public override byte[] ToWithPalette(in uint[] rgbaData, in uint[] palData)
        {
            byte[] indexData = new byte[Shared.AddPadding(width, 8) * Shared.AddPadding(height, 4)];
            int i = 0;

            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 8)
                {
                    for (int y1 = y; y1 < y + 4; y1++)
                    {
                        for (int x1 = x; x1 < x + 8; x1++)
                        {
                            uint pixel;

                            if (y1 >= height || x1 >= width)
                                pixel = 0;
                            else
                                pixel = rgbaData[y1 * width + x1];

                            indexData[i++] = (byte)ToGetColorIndex(pixel, palData);
                        }
                    }
                }
            }

            return indexData;
        }
    }
}
