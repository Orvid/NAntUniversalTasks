using System;
using System.IO;
using System.Text.RegularExpressions;

using Snak.Core;

namespace Snak.Metrics
{
	/// <summary>
	/// Provides functionality to count the LinesOfCode in a project
	/// </summary>
	public class LOCCounter
	{
		// Define the expressions used to determine what *isnt* a LOC (simpler this way)
		// A LOC is a fairly vague term, we're just interested in excluding obvious non-LOCs
		// Something should be included as a LOC if it has some kind of effect on the compiled output
		// (because that means it's something that can break a build)
		// 
		// So we exclude
		// - whitespace lines
		// - comment-only lines
		// 
		// We still count:
		// - braces on a line by themselves (seems reasonable to me - you might break them)
		// - imports/using lines
		// - namespace declarations
		static readonly Regex NonLocLinesCSharp = new Regex(@"^\s*($|//)"); //|using|namespace)");
		static readonly Regex NonLocLinesVB = new Regex(@"^\s*($|')"); //|Imports|Namespace)");

		public LOCCounter()
		{
		}

		public int CountLines(IProjectInfo project)
		{
			ScanProjectFilesWithRegex scan = new ScanProjectFilesWithRegex(project);
			
			Regex expression;
			switch(project.ProjectLanguage)
			{
				case ProjectLanguage.CSHARP:
					expression = NonLocLinesCSharp;	// only whitespace then line break, or comment starts
					break;
				case ProjectLanguage.VisualBasic:
					expression = NonLocLinesVB;	// only whitespace then line break, or comment starts
					break;
				default:
					throw new NotImplementedException(string.Format("Project language {0} not implemented", project.ProjectLanguage));
			}

			int matches = scan.MissCount(expression);
			return matches;
		}

		[NUnit.Framework.TestFixture]
		public class LOCCounterTester
		{
			// comment line with whitespace line below

			// another comment line

			public void SpikeLOCCountInThisFile()
			{
				string fileLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\LOCCounter.cs");
				ScanProjectFilesWithRegex.MatchResults results = ScanProjectFilesWithRegex.GetMatches(fileLocation, NonLocLinesCSharp);
				string[] matchedLines = (string[])results.MatchedLines.ToArray(typeof(string));
				Console.WriteLine("Ignored lines:");
				Console.WriteLine(string.Join("\n", matchedLines));
				Console.WriteLine("{0} insignificant lines, so {1} LOC (from total {2})", results.MatchCount, results.LineCount - results.MatchCount, results.LineCount);
			}

			[NUnit.Framework.Test]
			public void TestLOCCountInCSharpTestData()
			{
				Stream stream = GetType().Assembly.GetManifestResourceStream(GetType(), "LOCCounterTestData.cs");
				TextReader reader = new StreamReader(stream);
				ScanProjectFilesWithRegex.MatchResults results = ScanProjectFilesWithRegex.GetMatches(reader, NonLocLinesCSharp);

				int loc = results.LineCount - results.MatchCount;
				NUnit.Framework.Assert.AreEqual(11, loc);
			}

			[NUnit.Framework.Test]
			public void TestLOCCountInVBTestData()
			{
				Stream stream = GetType().Assembly.GetManifestResourceStream(GetType(), "LOCCounterTestData.vb");
				TextReader reader = new StreamReader(stream);
				ScanProjectFilesWithRegex.MatchResults results = ScanProjectFilesWithRegex.GetMatches(reader, NonLocLinesVB);

				int loc = results.LineCount - results.MatchCount;
				NUnit.Framework.Assert.AreEqual(8, loc);
			}
		}
	}
}
