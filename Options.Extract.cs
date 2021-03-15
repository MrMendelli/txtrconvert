using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace txtrconvert
{
	public partial class Options
	{
		[Verb("extract", HelpText = "Extract a TXTR file to a PNG file.")]
		public class ExtractOptions
		{
			[Value(0,
				Required = true,
				HelpText = "TXTR file to be processed.",
				MetaName = "Input")]
			public string Input { get; set; }

			[Value(1,
				Required = true,
				HelpText = "Directory where the PNG file(s) will be saved to.",
				MetaName = "Output")]
			public string Output { get; set; }

			[Option('m', "mipmaps",
			  Default = false,
			  HelpText = "Extract mipmaps from the TXTR file. If specified, a PNG file of every mipmap "
							+ "will be written in the file pattern of '<TXTRFileName>_<MipmapIndex>.png'")]
			public bool Mipmaps { get; set; }

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
					return new List<Example>() {
						new Example("Extract a TXTR to PNG without extracting mipmaps", new ExtractOptions {
							Input = "C:\\Users\\Samus\\Documents\\MP1Paks\\Metroid1-pak\\087fc94e.TXTR",
							Output = "C:\\Users\\Samus\\Pictures\\MP1TXTRRip",
							Mipmaps = false
						}),
						new Example("Extract a TXTR and its mipmaps to PNG", new ExtractOptions {
							Input = "C:\\Users\\Samus\\Documents\\MP2Paks\\TestAnim-pak\\67bb9879.TXTR",
							Output = "C:\\Users\\Samus\\Pictures\\MP2TXTRRip",
							Mipmaps = true
						})
					};
				}
			}
		}
	}
}
