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
		[Verb("read", HelpText = "Read information from a TXTR.")]
		public class ReadOptions
		{
			[Value(0,
				Required = true,
				HelpText = "TXTR file to be processed.",
				MetaName = "Input")]
			public string Input { get; set; }

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
						new Example("Read information from a TXTR", new ReadOptions {
							Input = "C:\\Users\\Samus\\Documents\\MP1Paks\\GGuiSys-pak\\c4cc7d02.TXTR",
						})
					};
				}
			}
		}
	}
}
