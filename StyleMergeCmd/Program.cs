using System.Collections.Generic;
using CommandLine;
using StyleMerge;

namespace StyleMergeCmd
{
	internal class Program
	{
		public class Options
		{
			[Option('i', "in", Required = true, HelpText = "Input file")]
			public string InputFile { get; set; }

			[Option('o', "out", Required = true, HelpText = "Output file")]
			public string OutputFile { get; set; }
		}

		public static void Main(string[] args)
		{
			Parser.Default.ParseArguments<Options>(args)
					.WithParsed(RunOptionsAndReturnExitCode)
					.WithNotParsed(HandleParseError)
				;
		}

		private static void RunOptionsAndReturnExitCode(Options opts)
		{
			var sourceHtml = System.IO.File.ReadAllText(opts.InputFile);
			var processedHtml = Inliner.ProcessHtml(sourceHtml);
			System.IO.File.WriteAllText(opts.OutputFile, processedHtml);
		}

		private static void HandleParseError(IEnumerable<Error> errs)
		{
		}
	}
}
