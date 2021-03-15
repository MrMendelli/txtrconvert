using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using txtrconvert.Graphics;

namespace txtrconvert
{
	public partial class Options
	{
		[Verb("create", HelpText = "Create a TXTR file from a PNG file.")]
		public class CreateOptions
		{
			[Value(0,
				Required = true,
				HelpText = "PNG file to be processed.",
				MetaName = "Input")]
			public string Input { get; set; }

			[Value(1,
				Required = true,
				HelpText = "TXTR file to be saved.",
				MetaName = "Output")]
			public string Output { get; set; }

			[Option('m', "mipmaps",
			  Default = 1,
			  HelpText = "Set the amount of mipmaps to generate. 1 mipmap is equal to no mipmaps. If specified, "
							+ "this image will be halved in size for each mipmap level. Make sure the number of "
							+ "mipamp levels do not amount to an image size that is below 4x4.")]
			public int Mipmaps { get; set; }

			[Option('t', "texformat",
			  Default = GX.TextureFormat.I4,
			  HelpText = "Set the texture format for the TXTR file.")]
			public GX.TextureFormat TextureFormat { get; set; }

			[Option('p', "palformat",
			  Default = GX.PaletteFormat.IA8,
			  HelpText = "Set the palette format for the TXTR file.")]
			public GX.PaletteFormat PaletteFormat { get; set; }

			[Option('v', "verbose",
			  Default = false,
			  HelpText = "Output extra verbose information.")]
			public bool Verbose { get; set; }

			[Option('s', "silent",
			  Default = false,
			  HelpText = "Be silent (do not output any information).")]
			public bool Silent { get; set; }

			[Option('c', "clicolors",
			  Default = false,
			  HelpText = "Output to console with colors (for logging only).")]
			public bool CliColors { get; set; }

			[Option('l', "logfile",
			  Default = false,
			  HelpText = "Log output to a file.")]
			public bool LogFile { get; set; }

			[Usage(ApplicationAlias = AssemblyInfo.AssemblyTitle)]
			public static IEnumerable<Example> Examples
			{
				get
				{
					// TODO: Create - Usage example
					return new List<Example>() {
						new Example("TODO", new CreateOptions {})
					};
				}
			}
		}
	}
}
