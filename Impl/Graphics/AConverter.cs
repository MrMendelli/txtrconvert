using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using txtrconvert.Graphics;
using txtrconvert.Graphics.Formats;
using txtrconvert.IO;
using txtrconvert.Util;

namespace txtrconvert.Impl.Graphics
{
    // TODO: Migrate from System.Drawing.Bitmap to SixLabors.ImageSharp.Image
    /// <summary>
    /// Thanks for:<br/>
    ///  <br/>Documentation from:<br/>
    ///      • <see href="https://wiki.axiodl.com/w/TXTR_(Metroid_Prime)">Retro Modding Wiki - TXTR (Metroid Prime)</see><br/>
    ///      • <see href="http://wiki.tockdom.com/wiki/Image_Formats">Custom Mario Kart Wiiki - Image Formats</see><br/>
    ///  <br/>Code from:<br/>
    ///      • <see href="https://github.com/magcius/noclip.website">noclip.website by Jasper</see> | <b>MIT</b><br/>
    ///      • <see href="https://github.com/WiiDatabase/libWiiSharp">libWiiSharp by leathl</see> | <b>GNU GPL v3</b><br/>
    ///      • <see href="https://github.com/Gskartwii/ctoolswii">CTools Game Editing Package by Chadderz</see> | <b>GNU GPL v3</b><br/>
    /// </summary>
    public abstract class AConverter
    {
        public const uint SizeLimit = 4;

        #region ConverterBitInfo

        public abstract int BitsPerPixel { get; }

        public abstract int BlockWidth { get; }

        public abstract int BlockHeight { get; }

        #endregion

        #region Dimensions

        protected uint width = 4, height = 4;

        public uint Width { set { if (value >= SizeLimit) width = value; } get { return width; } }

        public uint Height { set { if (value >= SizeLimit) height = value; } get { return height; } }

        #endregion

        #region PaletteDimensions

        protected ushort palWidth = 0, palHeight = 0;

        public ushort PalWidth { set { palWidth = value; } get { return palWidth; } }

        public ushort PalHeight { set { palHeight = value; } get { return palHeight; } }

        #endregion

        #region Format

        protected GX.TextureFormat texFormat = GX.TextureFormat.I4;

        protected GX.PaletteFormat palFormat = GX.PaletteFormat.IA8;

        public GX.TextureFormat TexFormat { set { texFormat = value; } get { return texFormat; } }

        public GX.PaletteFormat PalFormat { set { palFormat = value; } get { return palFormat; } }

        #endregion

        #region (constructor)

        protected AConverter()
            : this(SizeLimit, SizeLimit, 0, 0, GX.TextureFormat.I4, GX.PaletteFormat.IA8)
        {
        }

        protected AConverter(uint pWidth)
            : this(pWidth, SizeLimit, 0, 0, GX.TextureFormat.I4, GX.PaletteFormat.IA8)
        {
        }

        protected AConverter(uint pWidth, uint pHeight)
            : this(pWidth, pHeight, 0, 0, GX.TextureFormat.I4, GX.PaletteFormat.IA8)
        {
        }

        protected AConverter(uint pWidth, uint pHeight, ushort pPalWidth)
            : this(pWidth, pHeight, pPalWidth, 0, GX.TextureFormat.I4, GX.PaletteFormat.IA8)
        {
        }

        protected AConverter(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight)
            : this(pWidth, pHeight, pPalWidth, pPalHeight, GX.TextureFormat.I4, GX.PaletteFormat.IA8)
        {
        }

        protected AConverter(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight, GX.TextureFormat pTexFormt)
            : this(pWidth, pHeight, pPalWidth, pPalHeight, pTexFormt, GX.PaletteFormat.IA8)
        {
        }

        protected AConverter(uint pWidth, uint pHeight, ushort pPalWidth, ushort pPalHeight, GX.TextureFormat pTexFormt, GX.PaletteFormat pPalFormat)
        {
            Width = pWidth;
            Height = pHeight;
            PalWidth = pPalWidth;
            PalHeight = pPalHeight;
            TexFormat = pTexFormt;
            PalFormat = pPalFormat;
        }

        #endregion

        #region Palette

        public bool HasPalette()
        {
            return texFormat switch
            {
                GX.TextureFormat.C4 or GX.TextureFormat.C8 or GX.TextureFormat.C14X2 => true,
                _ => false,
            };
        }

        public int GetPaletteSize()
        {
            /*int paletteSize = 0;

            switch (texFormat)
            {
                case TextureFormat.C4:
                    paletteSize = 16;
                    break;
                case TextureFormat.C8:
                    paletteSize = 256;
                    break;
                case TextureFormat.C14X2:
                    paletteSize = 16384;
                    break;
            }

            // All palette-formats are 16-bit.
            return paletteSize * 2;*/
            return (palWidth * palHeight) * 2;
        }

        public (ushort pPalWidth, ushort pPalHeight) GetPaletteWidthHeight(in uint[] palData)
        {
            if (palWidth != 0 && palHeight != 0) return (palWidth, palHeight);
            else if (palData == null) return (0, 0);
            else return texFormat switch
            {
                GX.TextureFormat.C4 => (pPalWidth: 1, pPalHeight: (ushort)palData.Length),
                GX.TextureFormat.C8 => (pPalWidth: (ushort)palData.Length, pPalHeight: 1),
                // TODO: What is the palette width/height for C14X2?
                GX.TextureFormat.C14X2 => (pPalWidth: (ushort)palData.Length, pPalHeight: 1),
                _ => (0, 0),
            };
        }

        #endregion

        public int GetMipmapSize()
        {
            int numPixels = Shared.Align((int)width, 0x08) * Shared.Align((int)height, 0x08);
            return texFormat switch
            {
                GX.TextureFormat.I4 => numPixels / 2,
                GX.TextureFormat.I8 => numPixels,
                GX.TextureFormat.IA4 => numPixels,
                GX.TextureFormat.IA8 => numPixels * 2,
                GX.TextureFormat.C4 => numPixels / 2,
                GX.TextureFormat.C8 => numPixels,
                GX.TextureFormat.C14X2 => numPixels * 2,
                GX.TextureFormat.RGB565 => numPixels * 2,
                GX.TextureFormat.RGB5A3 => numPixels * 2,
                GX.TextureFormat.RGBA32 => numPixels * 4,
                GX.TextureFormat.CMPR => numPixels / 2,
                _ => numPixels,
            };
        }

        #region From

        public uint[] FromBitmap(in Bitmap bmp)
        {
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, (int)width, (int)height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte[] pixelData = new byte[bmpData.Height * (int)Math.Abs(bmpData.Stride)];
            Marshal.Copy(bmpData.Scan0, pixelData, 0, pixelData.Length);

            bmp.UnlockBits(bmpData);
            return Shared.ToUIntArray(pixelData);
        }

        public uint[] FromPalette(in byte[] palData)
        {
            int itemcount = palData.Length >> 1;
            int r = 0, g = 0, b = 0, a = 255;

            uint[] output = new uint[itemcount];
            for (int i = 0; i < itemcount; i++)
            {
                if (i >= itemcount) continue;

                ushort pixel = BitConverter.ToUInt16(new byte[] { palData[i * 2 + 1], palData[i * 2] }, 0);

                if (palFormat.Equals(GX.PaletteFormat.IA8)) //IA8
                {
                    r = pixel & 0xff;
                    b = r;
                    g = r;
                    a = pixel >> 8;
                }
                else if (palFormat.Equals(GX.PaletteFormat.RGB565)) //RGB565
                {
                    b = (((pixel >> 11) & 0x1F) << 3) & 0xff;
                    g = (((pixel >> 5) & 0x3F) << 2) & 0xff;
                    r = (((pixel >> 0) & 0x1F) << 3) & 0xff;
                    a = 255;
                }
                else //RGB5A3
                {
                    if ((pixel & (1 << 15)) != 0) //RGB555
                    {
                        a = 255;
                        b = (((pixel >> 10) & 0x1F) * 255) / 31;
                        g = (((pixel >> 5) & 0x1F) * 255) / 31;
                        r = (((pixel >> 0) & 0x1F) * 255) / 31;
                    }
                    else //RGB4A3
                    {
                        a = (((pixel >> 12) & 0x07) * 255) / 7;
                        b = (((pixel >> 8) & 0x0F) * 255) / 15;
                        g = (((pixel >> 4) & 0x0F) * 255) / 15;
                        r = (((pixel >> 0) & 0x0F) * 255) / 15;
                    }
                }

                output[i] = (uint)((r << 0) | (g << 8) | (b << 16) | (a << 24));
            }

            return output;
        }

        public abstract byte[] From(in byte[] texData);

        public abstract byte[] FromWithPalette(in byte[] texData, in uint[] paletteData);

        public Bitmap ToBitmap(in byte[] data)
        {
            Bitmap bmp = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);

            try
            {
                BitmapData bmpData = bmp.LockBits(
                                        new Rectangle(0, 0, (int)width, (int)height),
                                        ImageLockMode.WriteOnly, bmp.PixelFormat);

                Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
                bmp.UnlockBits(bmpData);
            }
            catch { bmp.Dispose(); throw; }

            return bmp;
        }

        #endregion

        #region To

        public uint[] ToPalette(in uint[] rgbaData)
        {
            int palLength = GetPaletteSize();

            List<uint> palette = new List<uint>();

            palette.Add(0);

            for (int i = 1; i < rgbaData.Length; i++)
            {
                if (palette.Count == palLength) break;
                if (((rgbaData[i] >> 24) & 0xff) < ((texFormat == GX.TextureFormat.C14X2) ? 1 : 25)) continue;

                if (!palette.Contains(rgbaData[i]))
                {
                    palette.Add(rgbaData[i]);
                }
            }

            while (palette.Count % 16 != 0) { palette.Add(0xffffffff); }

            return palette.ToArray();
        }

        protected ushort ToPaletteValue(int rgba)
        {
            int newpixel = 0, r, g, b, a;

            if (palFormat.Equals(GX.PaletteFormat.IA8))
            {
                int intensity = ((((rgba >> 0) & 0xff) + ((rgba >> 8) & 0xff) + ((rgba >> 16) & 0xff)) / 3) & 0xff;
                int alpha = (rgba >> 24) & 0xff;

                newpixel = (ushort)((alpha << 8) | intensity);
            }
            else if (palFormat.Equals(GX.PaletteFormat.RGB565))
            {
                newpixel = (ushort)(((((rgba >> 16) & 0xff) >> 3) << 11) | ((((rgba >> 8) & 0xff) >> 2) << 5) | ((((rgba >> 0) & 0xff) >> 3) << 0));
            }
            else
            {
                r = (rgba >> 16) & 0xff;
                g = (rgba >> 8) & 0xff;
                b = (rgba >> 0) & 0xff;
                a = (rgba >> 24) & 0xff;

                if (a <= 0xda) //RGB4A3
                {
                    newpixel &= ~(1 << 15);

                    r = ((r * 15) / 255) & 0xf;
                    g = ((g * 15) / 255) & 0xf;
                    b = ((b * 15) / 255) & 0xf;
                    a = ((a * 7) / 255) & 0x7;

                    newpixel |= a << 12;
                    newpixel |= b << 0;
                    newpixel |= g << 4;
                    newpixel |= r << 8;
                }
                else //RGB5
                {
                    newpixel |= (1 << 15);

                    r = ((r * 31) / 255) & 0x1f;
                    g = ((g * 31) / 255) & 0x1f;
                    b = ((b * 31) / 255) & 0x1f;

                    newpixel |= b << 0;
                    newpixel |= g << 5;
                    newpixel |= r << 10;
                }
            }

            return (ushort)newpixel;
        }

        protected uint ToGetColorIndex(uint value, in uint[] palData)
        {
            uint minDistance = 0x7FFFFFFF;
            uint colorIndex = 0;

            if (((value >> 24) & 0xFF) < ((texFormat.Equals(GX.TextureFormat.C14X2)) ? 1 : 25)) return 0;
            ushort color = ToPaletteValue((int)value);

            for (int i = 0; i < palData.Length; i++)
            {
                ushort curPal = ToPaletteValue((int)palData[i]);

                if (color == curPal) return (uint)i;
                uint curDistance = ToGetDistance(color, curPal); //(uint)Math.Abs(Math.Abs(color) - Math.Abs(curVal));

                if (curDistance < minDistance)
                {
                    minDistance = curDistance;
                    colorIndex = (uint)i;
                }
            }

            return colorIndex;
        }

        protected uint ToGetDistance(ushort color, ushort paletteColor)
        {
            uint curCol = ToToRgbaValue(color);
            uint palCol = ToToRgbaValue(paletteColor);

            uint curA = (curCol >> 24) & 0xFF;
            uint curR = (curCol >> 16) & 0xFF;
            uint curG = (curCol >> 8) & 0xFF;
            uint curB = (curCol >> 0) & 0xFF;

            uint palA = (palCol >> 24) & 0xFF;
            uint palR = (palCol >> 16) & 0xFF;
            uint palG = (palCol >> 8) & 0xFF;
            uint palB = (palCol >> 0) & 0xFF;

            uint distA = Math.Max(curA, palA) - Math.Min(curA, palA);
            uint distR = Math.Max(curR, palR) - Math.Min(curR, palR);
            uint distG = Math.Max(curG, palG) - Math.Min(curG, palG);
            uint distB = Math.Max(curB, palB) - Math.Min(curB, palB);

            return distA + distR + distG + distB;
        }

        protected uint ToToRgbaValue(ushort pixel)
        {
            int rgba = 0, r, g, b, a;

            if (palFormat.Equals(GX.PaletteFormat.IA8))
            {
                int i = (pixel >> 8);
                a = pixel & 0xff;

                rgba = (i << 0) | (i << 8) | (i << 16) | (a << 24);
            }
            else if (palFormat.Equals(GX.PaletteFormat.RGB565))
            {
                b = (((pixel >> 11) & 0x1F) << 3) & 0xff;
                g = (((pixel >> 5) & 0x3F) << 2) & 0xff;
                r = (((pixel >> 0) & 0x1F) << 3) & 0xff;
                a = 255;

                rgba = (r << 0) | (g << 8) | (b << 16) | (a << 24);
            }
            else
            {
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

                rgba = (r << 0) | (g << 8) | (b << 16) | (a << 24);
            }

            return (uint)rgba;
        }

        public abstract byte[] To(in uint[] pixeldata);

        public abstract byte[] ToWithPalette(in uint[] rgbaData, in uint[] palData);

        #endregion

        #region Get

        public static AConverter Get(GX.TextureFormat texFormat)
        {
            return texFormat switch
            {
                GX.TextureFormat.I4 => new I4(),
                GX.TextureFormat.I8 => new I8(),
                GX.TextureFormat.IA4 => new IA4(),
                GX.TextureFormat.IA8 => new IA8(),
                GX.TextureFormat.C4 => new C4(),
                GX.TextureFormat.C8 => new C8(),
                GX.TextureFormat.C14X2 => new C14X2(),
                GX.TextureFormat.RGB565 => new RGB565(),
                GX.TextureFormat.RGB5A3 => new RGB5A3(),
                GX.TextureFormat.RGBA32 => new RGBA32(),
                GX.TextureFormat.CMPR => new CMPR(),
                _ => throw new ArgumentException($"There is no Converter for TextureFormat '0x{texFormat:X}'"),
            };
        }

        #endregion
    }
}
