using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using txtrconvert.DataSpec;
using txtrconvert.Extensions;
using txtrconvert.Graphics;
using txtrconvert.Graphics.Formats;
using txtrconvert.Impl.Graphics;
using txtrconvert.IO;
using txtrconvert.Util;
using static txtrconvert.Util.Shared.StaticMembers;

// TODO: Migrate from System.Drawing.Bitmap to SixLabors.ImageSharp.Image
namespace txtrconvert
{
    public class Program
    {
        public const string TAG = nameof(Program);
        public static readonly Logger progLogger = new Logger(TAG);

        public static int Main(string[] args)
        {
            return Options.Parse(args, HandleExtract, HandleCrate, HandleRead);
        }

        public static int HandleExtract(Options.ExtractOptions options)
        {
            progLogger.Level = (options.Verbose) ? Logger.Levels.VERBOSE : (options.Silent) ? Logger.Levels.ERROR : Logger.Levels.INFO;
            progLogger.Flag = Logger.Flags.DEBUG | Logger.Flags.DEBUGCONSOLE | Logger.Flags.CLICONSOLE;
            if (options.CliColors) progLogger.Flag = Logger.Flags.CLICOLORS;
            if (options.LogFile)
            {
                progLogger.Flag = Logger.Flags.LOGFILE;
                progLogger.InitializeLogFile(Path.Combine(Environment.CurrentDirectory, $"{AssemblyInfo.AssemblyTitle}.log"));
            }

            // Input validation
            if (!ValidateInput(options.Input, isCreate: false, out string inDir, out string inFileName, out string inFileExt)) return 1;
            // Output validation
            if (!ValidateOutput(options.Output, isCreate: false, out _, out _, out _)) return 1;

            // TXTR is big endian with ASCII
            using (FileStream readerStream = new FileStream(options.Input, FileMode.Open, FileAccess.Read, FileShare.None))
            using (EndianBinaryReader reader = new EndianBinaryReader(readerStream, isLittleEndian: false, Encoding.ASCII, leaveOpen: false))
            {
                TXTR TXTR = new TXTR();
                progLogger.Verbose($"TXTR {inFileName}");
                progLogger.Verbose("==================");

                TXTR.texFormat = reader.ReadUInt32();
                AConverter texConverter = AConverter.Get((GX.TextureFormat)TXTR.texFormat);
                texConverter.TexFormat = (GX.TextureFormat)TXTR.texFormat;
                progLogger.Verbose($"Texture Format: {(GX.TextureFormat)TXTR.texFormat}");

                TXTR.width = reader.ReadUInt16();
                texConverter.Width = TXTR.width;
                progLogger.Verbose($"Width: {TXTR.width}");

                TXTR.height = reader.ReadUInt16();
                texConverter.Height = TXTR.height;
                progLogger.Verbose($"Height: {TXTR.height}");

                if (TXTR.width < AConverter.SizeLimit || TXTR.height < AConverter.SizeLimit)
                {
                    progLogger.Error($"Invalid dimensions: width='{TXTR.width}', height='{TXTR.height}'");
                    return 1;
                }

                TXTR.mipCount = reader.ReadUInt32();
                progLogger.Verbose($"Mipmap Count: {TXTR.mipCount}");
                if (TXTR.mipCount < 1 || TXTR.mipCount > 255)
                {
                    progLogger.Error($"Invalid mipmap count: '{TXTR.mipCount}'");
                    return 1;
                }

                if (texConverter.HasPalette())
                {
                    TXTR.palFormat = reader.ReadUInt32();
                    texConverter.PalFormat = (GX.PaletteFormat)TXTR.palFormat;
                    progLogger.Verbose($"Palette Format: {(GX.PaletteFormat)TXTR.palFormat}");

                    TXTR.palWidth = reader.ReadUInt16();
                    texConverter.PalWidth = TXTR.palWidth;
                    progLogger.Verbose($"Palette Width: {TXTR.palWidth}");

                    TXTR.palHeight = reader.ReadUInt16();
                    texConverter.PalHeight = TXTR.palHeight;
                    progLogger.Verbose($"Palette Height: {TXTR.palHeight}");

                    TXTR.palData = texConverter.FromPalette(reader.ReadBytes(texConverter.GetPaletteSize()));
                    progLogger.Verbose($"Palette Data: uint[{TXTR.palData.Length}]");
                }

                TXTR.texData = reader.ReadAllBytes();
                if (TXTR.texData.Length == 0)
                {
                    progLogger.Error("Texture data is empty");
                    return 1;
                }
                progLogger.Verbose($"Texture Data: byte[{TXTR.texData.Length}]");

                byte[] mipData = null;
                ushort mipwidth = TXTR.width, mipheight = TXTR.height;
                int mipOffs = 0;
                string mipFile = "";
                for (int m = 0; m < TXTR.mipCount; m++)
                {
                    if (!options.Mipmaps && m > 0) break;

                    mipData = new byte[TXTR.texData.Length - mipOffs];
                    if (mipData.Length <= 0)
                    {
                        progLogger.Error($"Mipmap data is empty for mipmap {m+1}");
                        return 1;
                    }
                    else
                    {
                        Array.Copy(TXTR.texData, mipOffs, mipData, 0, mipData.Length);
                    }

                    progLogger.VerbosePrefix(NewLine, $"Mipmap: {m+1}");
                    progLogger.Verbose($"Mipmap Width: {mipwidth}");
                    progLogger.Verbose($"Mipmap Height: {mipheight}");
                    progLogger.Verbose($"Mipmap Offset: {mipOffs}");

                    switch ((GX.TextureFormat)TXTR.texFormat)
                    {
                        case GX.TextureFormat.I4:
                            mipData = ((I4)texConverter).From(mipData);
                            break;
                        case GX.TextureFormat.I8:
                            mipData = ((I8)texConverter).From(mipData);
                            break;
                        case GX.TextureFormat.IA4:
                            mipData = ((IA4)texConverter).From(mipData);
                            break;
                        case GX.TextureFormat.IA8:
                            mipData = ((IA8)texConverter).From(mipData);
                            break;
                        case GX.TextureFormat.C4:
                            mipData = ((C4)texConverter).FromWithPalette(mipData, TXTR.palData);
                            break;
                        case GX.TextureFormat.C8:
                            mipData = ((C8)texConverter).FromWithPalette(mipData, TXTR.palData);
                            break;
                        case GX.TextureFormat.C14X2:
                            mipData = ((C14X2)texConverter).FromWithPalette(mipData, TXTR.palData);
                            break;
                        case GX.TextureFormat.RGB565:
                            mipData = ((RGB565)texConverter).From(mipData);
                            break;
                        case GX.TextureFormat.RGB5A3:
                            mipData = ((RGB5A3)texConverter).From(mipData);
                            break;
                        case GX.TextureFormat.RGBA32:
                            mipData = ((RGBA32)texConverter).From(mipData);
                            break;
                        case GX.TextureFormat.CMPR:
                            mipData = ((CMPR)texConverter).From(mipData);
                            break;
                        default:
                            progLogger.Error($"TXTR format '{(GX.TextureFormat)TXTR.texFormat}' ('0x{TXTR.texFormat:X}') is not supported for extracting.");
                            return 1;
                    }

                    if (mipData.Length != 0)
                    {
                        progLogger.Verbose($"Mipmap Data: byte[{mipData.Length}]");

                        mipFile = $"{options.Output}{Path.DirectorySeparatorChar}{inFileName}_{m+1}.png";
                        if (ValidateOutput(mipFile, isCreate: true, out _, out _, out _))
                        {
                            progLogger.Info($"Saving mipmap {m + 1} to '{mipFile}'");
                            using (Bitmap bmp = texConverter.ToBitmap(mipData))
                            {
                                bmp.Save(mipFile, ImageFormat.Png);
                            }
                        }
                    }
                    else
                    {
                        progLogger.Error($"Mipmap data is empty for mipmap {m+1}");
                        return 1;
                    }

                    // Mipmap levels are aligned to 32B.
                    mipOffs += Math.Max(texConverter.GetMipmapSize(), 32);
                    mipwidth /= 2;
                    mipheight /= 2;
                    texConverter.Width = mipwidth;
                    texConverter.Height = mipheight;

                    // It seems like anything below 4x4 has junk data
                    if (mipwidth < AConverter.SizeLimit || mipheight < AConverter.SizeLimit)
                        break;
                }
            }

            progLogger.InfoPrefix(NewLine, "Done.");
            return 0;
        }

        public static int HandleCrate(Options.CreateOptions options)
        {
            progLogger.Level = (options.Verbose) ? Logger.Levels.VERBOSE : (options.Silent) ? Logger.Levels.ERROR : Logger.Levels.INFO;
            progLogger.Flag = Logger.Flags.DEBUG | Logger.Flags.DEBUGCONSOLE | Logger.Flags.CLICONSOLE;
            if (options.CliColors) progLogger.Flag = Logger.Flags.CLICOLORS;
            if (options.LogFile)
            {
                progLogger.Flag = Logger.Flags.LOGFILE;
                progLogger.InitializeLogFile(Path.Combine(Environment.CurrentDirectory, $"{AssemblyInfo.AssemblyTitle}.log"));
            }

            // Input validation
            if (!ValidateInput(options.Input, isCreate: true, out _, out string inFileName, out string inFileExt)) return 1;
            // Output validation
            if (!ValidateOutput(options.Output, isCreate: true, out _, out string outFileName, out _)) return 1;

            // TXTR is big endian with ASCII
            using (FileStream writerStream = new FileStream(options.Output, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(writerStream, isLittleEndian: false, Encoding.ASCII, leaveOpen: false))
            using (Bitmap img = (Bitmap)Image.FromFile(options.Input))
            {
                TXTR TXTR = new TXTR();
                progLogger.Verbose($"TXTR {outFileName}");
                progLogger.Verbose("==================");

                TXTR.texFormat = (uint)(int)Enum.Parse(typeof(GX.TextureFormat), options.TextureFormat.ToString());
                AConverter texConverter = AConverter.Get((GX.TextureFormat)TXTR.texFormat);
                texConverter.TexFormat = (GX.TextureFormat)TXTR.texFormat;
                writer.Write(TXTR.texFormat);
                progLogger.Verbose($"Texture Format: {(GX.TextureFormat)TXTR.texFormat}");

                TXTR.width = (ushort)img.Width;
                texConverter.Width = TXTR.width;
                writer.Write(TXTR.width);
                progLogger.Verbose($"Width: {TXTR.width}");

                TXTR.height = (ushort)img.Height;
                texConverter.Height = TXTR.height;
                writer.Write(TXTR.height);
                progLogger.Verbose($"Height: {TXTR.height}");

                if (TXTR.width < AConverter.SizeLimit || TXTR.height < AConverter.SizeLimit)
                {
                    progLogger.Error($"Invalid dimensions: width='{TXTR.width}', height='{TXTR.height}'");
                    return 1;
                }

                TXTR.mipCount = (uint)options.Mipmaps;
                if (TXTR.mipCount < 1 || TXTR.mipCount > 255)
                {
                    progLogger.Error($"Invalid mipmap count: '{TXTR.mipCount}'");
                    return 1;
                }
                else if (TXTR.width / (2 * TXTR.mipCount) < AConverter.SizeLimit || TXTR.height / (2 * TXTR.mipCount) < AConverter.SizeLimit)
                {
                    progLogger.Error($"Cannot write {TXTR.mipCount} mipmaps as the size decrease would exceed the size limit.");
                    return 1;
                }
                writer.Write(TXTR.mipCount);
                progLogger.Verbose($"Mipmap Count: {TXTR.mipCount}");

                if (texConverter.HasPalette())
                {
                    TXTR.palFormat = (uint)(int)Enum.Parse(typeof(GX.PaletteFormat), options.PaletteFormat.ToString());
                    texConverter.PalFormat = (GX.PaletteFormat)TXTR.palFormat;
                    writer.Write(TXTR.palFormat);
                    progLogger.Verbose($"Palette Format: {(GX.PaletteFormat)TXTR.palFormat}");

                    TXTR.palData = texConverter.ToPalette(texConverter.FromBitmap((Bitmap)img));
                    (ushort pPalWidth, ushort pPalHeight) = texConverter.GetPaletteWidthHeight(TXTR.palData);

                    TXTR.palWidth = pPalWidth;
                    texConverter.PalWidth = TXTR.palWidth;
                    writer.Write(TXTR.palWidth);
                    progLogger.Verbose($"Palette Width: {TXTR.palWidth}");

                    TXTR.palHeight = pPalHeight;
                    texConverter.PalHeight = TXTR.palHeight;
                    writer.Write(TXTR.palHeight);
                    progLogger.Verbose($"Palette Height: {TXTR.palHeight}");
                    foreach (uint palP in TXTR.palData) writer.Write(palP);
                    writer.Write(Array.ConvertAll(TXTR.palData, i => (byte)i));
                    progLogger.Verbose($"Palette Data: uint[{TXTR.palData.Length}]");
                }

                int mipwidth = img.Width, mipheight = img.Height;
                byte[] mipData = Array.Empty<byte>();
                uint[] texData = Array.Empty<uint>();
                for (int m = 0; m < options.Mipmaps; m++)
                {
                    if (options.Mipmaps == 1 && m > 0) break;

                    progLogger.VerbosePrefix(NewLine, $"Mipmap: {m+1}");

                    if (m == 0)
                    {
                        texConverter.Width = (uint)img.Width;
                        texConverter.Height = (uint)img.Height;
                    }
                    else
                    {
                        texConverter.Width = (uint)mipwidth;
                        texConverter.Height = (uint)mipheight;
                    }

                    if (m != 0 && (texConverter.Width / 2 != mipwidth / 2 || texConverter.Height / 2 != mipheight / 2))
                    {
                        progLogger.Error($"The size of mipmap {m + 1} is not half of size than the previous mipmap size: width='{TXTR.width}' != mipwidth='{mipwidth}', height='{TXTR.height}' != mipheight='{mipheight}'");
                        return 1;
                    }

                    using (Bitmap texBmp = new Bitmap(img, (int)texConverter.Width, (int)texConverter.Height))
                    {
                        texData = texConverter.FromBitmap(texBmp);
                    }
                    if (texData.Length == 0)
                    {
                        progLogger.Error($"Texture data is empty");
                        return 1;
                    }
                    progLogger.Verbose($"Texture Data: byte[{texData.Length}]");

                    if (texConverter.Width < AConverter.SizeLimit || texConverter.Height < AConverter.SizeLimit)
                    {
                        progLogger.Error($"Invalid dimensions for mipmap {m+1}: width='{TXTR.width}', height='{TXTR.height}'");
                        return 1;
                    }
                    progLogger.Verbose($"Mipmap Width: {img.Width}");
                    progLogger.Verbose($"Mipmap Height: {img.Height}");

                    switch ((GX.TextureFormat)((int)TXTR.texFormat))
                    {
                        case GX.TextureFormat.I4:
                            mipData = ((I4)texConverter).To(texData);
                            break;
                        case GX.TextureFormat.I8:
                            mipData = ((I8)texConverter).To(texData);
                            break;
                        case GX.TextureFormat.IA4:
                            mipData = ((IA4)texConverter).To(texData);
                            break;
                        case GX.TextureFormat.IA8:
                            mipData = ((IA8)texConverter).To(texData);
                            break;
                        case GX.TextureFormat.C4:
                            mipData = ((C4)texConverter).ToWithPalette(texData, TXTR.palData);
                            break;
                        case GX.TextureFormat.C8:
                            mipData = ((C8)texConverter).ToWithPalette(texData, TXTR.palData);
                            break;
                        case GX.TextureFormat.C14X2:
                            mipData = ((C14X2)texConverter).ToWithPalette(texData, TXTR.palData);
                            break;
                        case GX.TextureFormat.RGB565:
                            mipData = ((RGB565)texConverter).To(texData);
                            break;
                        case GX.TextureFormat.RGB5A3:
                            mipData = ((RGB5A3)texConverter).To(texData);
                            break;
                        case GX.TextureFormat.RGBA32:
                            mipData = ((RGBA32)texConverter).To(texData);
                            break;
                            // TODO: Support CMPR
                            /*case GX.TextureFormat.CMPR:
                               mipData = ((CMPR)texConverter).To(texData);
                            break; */
                          default:
                        progLogger.Error($"TXTR format '{(GX.TextureFormat)TXTR.texFormat}' ('0x{TXTR.texFormat:X}') is not supported for creating.");
                            return 1;
                    }

                    if (mipData.Length == 0)
                    {
                        progLogger.Error($"Mipmap {m+1} data is empty");
                        return 1;
                    }
                    progLogger.Verbose($"Mipmap Data: byte[{mipData.Length}]");

                    progLogger.Info($"Saving mipmap {m+1} to '{options.Output}'");
                    writer.Write(mipData);

                    mipwidth /= 2;
                    mipheight /= 2;
                }
            }

            progLogger.InfoPrefix(NewLine, "Done.");
            return 0;
        }

        public static int HandleRead(Options.ReadOptions options)
        {
            progLogger.Level = Logger.Levels.INFO;
            progLogger.Flag = Logger.Flags.DEBUG | Logger.Flags.DEBUGCONSOLE | Logger.Flags.CLICONSOLE;
            if (options.CliColors) progLogger.Flag = Logger.Flags.CLICOLORS;
            if (options.LogFile)
            {
                progLogger.Flag = Logger.Flags.LOGFILE;
                progLogger.InitializeLogFile(Path.Combine(Environment.CurrentDirectory, $"{AssemblyInfo.AssemblyTitle}.log"));
            }

            // Input validation
            if (!ValidateInput(options.Input, isCreate: false, out _, out string inFileName, out _)) return 1;

            // TXTR is big endian with ASCII
            using (FileStream readerStream = new FileStream(options.Input, FileMode.Open, FileAccess.Read, FileShare.None))
            using (EndianBinaryReader reader = new EndianBinaryReader(readerStream, isLittleEndian: false, Encoding.ASCII, leaveOpen: false))
            {
                TXTR TXTR = new TXTR();
                progLogger.Info($"TXTR {inFileName}");
                progLogger.Info("==================");

                TXTR.texFormat = reader.ReadUInt32();
                AConverter texConverter = AConverter.Get((GX.TextureFormat)TXTR.texFormat);
                texConverter.TexFormat = (GX.TextureFormat)TXTR.texFormat;
                progLogger.Info($"Texture Format: {(GX.TextureFormat)TXTR.texFormat}");

                TXTR.width = reader.ReadUInt16();
                texConverter.Width = TXTR.width;
                progLogger.Info($"Width: {TXTR.width}");
                TXTR.height = reader.ReadUInt16();
                texConverter.Height = TXTR.height;
                progLogger.Info($"Height: {TXTR.height}");

                TXTR.mipCount = reader.ReadUInt32();
                progLogger.Info($"Mipmap Count: {TXTR.mipCount}");

                if (texConverter.HasPalette())
                {
                    TXTR.palFormat = reader.ReadUInt32();
                    texConverter.PalFormat = (GX.PaletteFormat)TXTR.palFormat;
                    progLogger.Info($"Palette Format: {(GX.PaletteFormat)TXTR.palFormat}");

                    TXTR.palWidth = reader.ReadUInt16();
                    texConverter.PalWidth = TXTR.palWidth;
                    progLogger.Info($"Palette Width: {TXTR.palWidth}");

                    TXTR.palHeight = reader.ReadUInt16();
                    texConverter.PalHeight = TXTR.palHeight;
                    progLogger.Info($"Palette Height: {TXTR.palHeight}");
                    TXTR.palData = texConverter.FromPalette(reader.ReadBytes(texConverter.GetPaletteSize()));
                    progLogger.Info($"Palette Data: uint[{TXTR.palData.Length}]");
                }

                TXTR.texData = reader.ReadAllBytes();
                progLogger.Info($"Texture Data: byte[{TXTR.texData.Length}]");
            }

            progLogger.InfoPrefix(NewLine, "Done.");
            return 0;
        }

        private static bool ValidateInput(string input, bool isCreate, out string inDir, out string inFileName, out string inFileExt)
        {
            try
            {
                inDir = Path.GetDirectoryName(input);
                inFileName = Path.GetFileNameWithoutExtension(input);
                inFileExt = Path.GetExtension(input);
            }
            catch
            {
                progLogger.Error($"Invalid input file path: '{input}'.");
                inDir = "";
                inFileName = "";
                inFileExt = "";
                return false;
            }

            if (!inFileExt.ToLower().Equals(isCreate ? ".png" : ".txtr"))
            {
                progLogger.Error($"Input file '{input}' must be a {(isCreate ? "PNG" : "TXTR")} file.");
                return false;
            }
            else if (!Directory.Exists(inDir))
            {
                string inDirRel = $".{Path.DirectorySeparatorChar}{inDir}";
                if (!Directory.Exists(inDirRel))
                {
                    progLogger.Error($"Input directory does not exist at: '{inDir}'.");
                    return false;
                }
            }
            else if (!File.Exists(input))
            {
                progLogger.Error($"Input file does not exist at: '{input}'.");
                return false;
            }
            return true;
        }

        private static bool ValidateOutput(string output, bool isCreate, out string outDir, out string outFileName, out string outFileExt)
        {
            try
            {
                outDir = Path.GetDirectoryName(output);
                outFileName = Path.GetFileNameWithoutExtension(output);
                outFileExt = Path.GetExtension(output);
            }
            catch
            {
                progLogger.Error($"Invalid input file path: '{output}'.");
                outDir = "";
                outFileName = "";
                outFileExt = "";
                return false;
            }

            if (File.Exists(output))
            {
                if (isCreate)
                    progLogger.Warning($"Output file '{output}' already exists.");
                else
                    progLogger.Error($"Output '{output}' must be a directory.");
                return false;
            }
            /*if (File.Exists(output) && !isCreate)
            {
                progLogger.Error("Output must be a directory.");
                return false;
            }
            else if (!File.Exists(output) && isCreate)
            {
                progLogger.Error($"Output file does not exist at: '{output}'.");
                return false;
            }*/

            if (!Directory.Exists(outDir))
            {
                string outDirRel = $".{Path.DirectorySeparatorChar}{outDir}";
                if (!Directory.Exists(outDirRel))
                {
                    progLogger.Error($"Output directory does not exist at: '{outDir}'.");
                    return false;
                }
            }

            return true;
        }
    }
}
