using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using txtrconvert.Graphics;
using static txtrconvert.Util.Shared.StaticMembers;

namespace txtrconvert
{
	public partial class Options
	{
		public delegate int ParseExtractDelegate(ExtractOptions options);

		public delegate int ParseCreateDelegate(CreateOptions options);

		public delegate int ParseReadDelegate(ReadOptions options);

		public static int Parse(in string[] args, ParseExtractDelegate ParseExtract, ParseCreateDelegate ParseCreate, ParseReadDelegate ParseRead)
        {
			Parser optionParser = new Parser(config => {
				config.HelpWriter = Console.Out;
				config.MaximumDisplayWidth = Console.WindowWidth;
				config.ParsingCulture = CultureInfo.CurrentCulture;
			});

			return optionParser.ParseArguments<ExtractOptions, CreateOptions, ReadOptions>(args)
				.MapResult(
					(ExtractOptions options) => ParseExtract(options),
					(CreateOptions options) => ParseCreate(options),
					(ReadOptions options) => ParseRead(options),
					(IEnumerable<Error> errors) => DisplayHelpFooter()
				);
		}

		private static int DisplayHelpFooter()
		{
			Console.Out.WriteLine(
				   "Notes:"
				+ $"{NewLine}    - For TXTR creation: each mipmap's size must be smaller than the first mipmap and half of the previous mipmap."
				+ $"{NewLine}    - For TXTR extraction and TXTR creation: the minimum mipmap size is 4x4."
				+ $"{NewLine}    - For TXTR Read: invalid data is not taken to account. If output looks invalid, it probably is."

				+ $"{NewLine}{NewLine}Texture Formats:"
				+ $"{NewLine}    - I4     = Intensity, 4bpp"
				+ $"{NewLine}    - I8     = Intensity, 8bpp"
				+ $"{NewLine}    - IA4    = Intensity, Alpha, 8bpp"
				+ $"{NewLine}    - IA8    = Intensity, Alpha, 16bpp"
				+ $"{NewLine}    - C4     = Indexed (Palette), 4bpp"
				+ $"{NewLine}    - C8     = Indexed (Palette), 8bpp"
				+ $"{NewLine}    - C14X2  = Indexed (Palette), 14bpp"
				+ $"{NewLine}    - RGB565 = RGB, 16bpp"
				+ $"{NewLine}    - RGB5A3 = RGB, Alpha, 16bpp"
				+ $"{NewLine}    - RGBA32 = RGB, Alpha, 32bpp"
				+ $"{NewLine}    - CMPR   = S3TC Compressed"

				+ $"{NewLine}{NewLine}Palette Formats:"
				+ $"{NewLine}    - IA8    = Intensity, Alpha, 16bpp"
				+ $"{NewLine}    - RGB565 = RGB, 16bpp"
				+ $"{NewLine}    - RGB5A3 = RGB, Alpha, 16bpp"
			);
			return 1;
		}
	}
}
