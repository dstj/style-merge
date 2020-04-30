using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

			[Option('o', "out", Required = false, HelpText = "Output file (when missing, '.inline' is added to input file)")]
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
			var inputFile = opts.InputFile;
			var sourceHtml = GetDocument(inputFile);
			var processedHtml = Inliner.ProcessHtml(sourceHtml);

			var outputFile = opts.OutputFile;
			if (string.IsNullOrEmpty(outputFile)) {
				var name = System.IO.Path.GetFileNameWithoutExtension(inputFile);
				var ext = System.IO.Path.GetExtension(inputFile);
				outputFile = $"{name}.inline{ext}";
			}
			System.IO.File.WriteAllText(outputFile, processedHtml);
		}

		private static string GetDocument(string inputFile)
		{
			var sourceHtml = System.IO.File.ReadAllText(inputFile);
			var dir = System.IO.Path.GetDirectoryName(inputFile);

			sourceHtml = EmbedLinkedCss(sourceHtml, dir);

			return sourceHtml;
		}

		private static string EmbedLinkedCss(string sourceHtml, string dir)
		{
			const string pattern = @"<link(?: (rel|type|href|media)=""(.*?)"")+ */?>";
			var regex = new Regex(pattern, RegexOptions.Compiled);
			var matches = regex.Matches(sourceHtml);
			foreach (var m in matches.Cast<Match>()) {
				var fullMatch = m.Value;

				int hrefIdx;
				var captureAttrNames = m.Groups[1].Captures;
				for (hrefIdx = 0; hrefIdx < captureAttrNames.Count; ++hrefIdx) {
					var attr = captureAttrNames[hrefIdx].Value;
					if (attr == "href")
						break;
				}

				var hrefValue = m.Groups[2].Captures[hrefIdx].Value;
				if (!System.IO.Path.IsPathRooted(hrefValue)) {
					hrefValue = System.IO.Path.Combine(dir, hrefValue);
				}

				var cssFileSource = System.IO.File.ReadAllText(hrefValue);
				sourceHtml = sourceHtml.Replace(fullMatch, $"<style>{cssFileSource}</style>");
			}

			return sourceHtml;
		}

		private static void HandleParseError(IEnumerable<Error> errs)
		{
		}
	}
}
