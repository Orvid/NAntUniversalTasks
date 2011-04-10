using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;

using Snak.Core;

namespace Snak.Metrics
{
	/// <summary>
	/// Runs a regular expression against all the compile items within a project
	/// </summary>
	internal class ScanProjectFilesWithRegex
	{
		IProjectInfo _project;

		public ScanProjectFilesWithRegex(IProjectInfo project)
		{
			_project = project;
		}

		/// <summary>
		/// Returns the number of matches for the regular expression across all Compile files in the project
		/// </summary>
		public int MatchCount(string regex)
		{
			Regex re = new Regex(regex);
			return MatchCount(re);
		}

		/// <summary>
		/// Returns the number of matches for the regular expression across all Compile files in the project
		/// </summary>
		public int MatchCount(Regex regex)
		{
			MatchResults results = GetMatches(_project, regex);
			return results.MatchCount;
		}

		/// <summary>
		/// Returns the number of misses (unmatched lines) for the regular expression across all Compile files in the project
		/// </summary>
		public int MissCount(Regex regex)
		{
			MatchResults results = GetMatches(_project, regex);
			return results.LineCount - results.MatchCount;
		}

		/// <summary>
		/// Returns the number of misses (unmatched lines) for the regular expression across all Compile files in the project
		/// </summary>
		public int MissCount(string regex)
		{
			Regex re = new Regex(regex);
			return MissCount(re);
		}

		#region Support methods
		/// <summary>
		/// Get the total number of lines in all the files in the project, plus those lines
		/// that match the regular expression
		/// </summary>
		internal static MatchResults GetMatches(IProjectInfo project, Regex expression)
		{
			int lineCount = 0;
			int matchCount = 0;
			ArrayList matchedLines = new ArrayList();
			foreach(string fileRelativePath in project.GetProjectFiles(ProjectFileType.Compile))
			{
				string filePath = Path.Combine(project.ProjectDir, fileRelativePath);
				MatchResults results = GetMatches(filePath, expression);

				lineCount += results.LineCount;
				matchCount += results.MatchCount;
				matchedLines.Add(results.MatchedLines);
			}

			return new MatchResults(matchCount, lineCount, matchedLines);
		}

		/// <summary>
		/// Get the total number of lines in the file, plus those lines
		/// that match the regular expression
		/// </summary>
		internal static MatchResults GetMatches(string filePath, Regex expression)
		{
			using(TextReader reader = new StreamReader(File.OpenRead(filePath)))
				return GetMatches(reader, expression);
		}

		/// <summary>
		/// Get the total number of lines (remaining) in a text reader, plus those lines
		/// that match the regular expression
		/// </summary>
		internal static MatchResults GetMatches(TextReader reader, Regex expression)
		{
			int lineCount = 0;
			int matchCount = 0;
			ArrayList matchedLines = new ArrayList();

			string line = reader.ReadLine();
			while(line!=null)
			{
				lineCount++;
				Match match = expression.Match(line);
				if (match!=null && match.Success)
				{
					matchCount++;
					matchedLines.Add(line);
				}

				line = reader.ReadLine();
			}
			return new MatchResults(matchCount, lineCount, matchedLines);
		}

		internal struct MatchResults
		{
			public int MatchCount;
			public int LineCount;
			public ArrayList MatchedLines;

			public MatchResults(int matchCount, int lineCount, ArrayList matchedLines)
			{
				this.MatchCount = matchCount;
				this.LineCount = lineCount;
				this.MatchedLines = matchedLines;
			}
		}
#endregion

		[NUnit.Framework.TestFixture]
		public class ScanProjectfilesWithRegexTester
		{
			public void TestScanProjectFilesAgainstThisProject(){
				// TODO: Run this test instead against a canned project

				string projectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..");
				projectPath = Path.Combine(projectPath, "snak.csproj");

				IProjectInfo snakProject = ProjectFactory.GetProject(projectPath, "Debug", Log);

				ScanProjectFilesWithRegex scanner = new ScanProjectFilesWithRegex(snakProject);
				NUnit.Framework.Assert.AreEqual(1, scanner.MatchCount("class ScanProjectFilesWithRegex$"));
			}

            private void Log(LogLevel level, string message, params object[] args) { }
		}
	}
}
