namespace txtrconvert.Graphics
{
    public static class GX
    {
        public enum TextureFormat : int
        {
            I4      = 0x0,  // Intensity, 4bpp
            I8      = 0x1,  // Intensity, 8bpp
            IA4     = 0x2,  // Intensity, Alpha, 8bpp
            IA8     = 0x3,  // Intensity, Alpha, 16bpp
            C4      = 0x4,  // Indexed (Palette), 4bpp
            C8      = 0x5,  // Indexed (Palette), 8bpp
            C14X2   = 0x6,  // Indexed (Palette), 14bpp
            RGB565  = 0x7,  // RGB, 16bpp
            RGB5A3  = 0x8,  // RGB, Alpha, 16bpp
            RGBA32  = 0x9,  // RGB, Alpha, 32bpp
            CMPR    = 0xA   // S3TC Compressed
        };

        public enum PaletteFormat : int
        {
            IA8     = 0x00, // Intensity, Alpha, 16bpp
            RGB565  = 0x01, // RGB, 16bpp
            RGB5A3  = 0x02  // RGB, Alpha, 16bpp
        }
    }
}
