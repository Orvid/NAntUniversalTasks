using System;
using System.Globalization;
using System.Text;

using NAnt.Core;

using NAntExtensions.TeamCity.Common.Helper;

namespace NAntExtensions.TeamCity.Common.Messaging
{
	internal class TeamCityMessageProvider : ITeamCityMessageProvider
	{
		readonly TeamCityLogWriter _writer;

		public TeamCityMessageProvider(TeamCityLogWriter writer, Task taskToUseForLogging)
		{
			Ensure.ArgumentIsNotNull(writer, "writer");
			Ensure.ArgumentIsNotNull(taskToUseForLogging, "taskToUseForLogging");

			_writer = writer;
			_writer.TaskToUseForLogging = taskToUseForLogging;
		}

		#region ITeamCityMessageProvider Members
		public void TestSuiteStarted(string assemblyName)
		{
			if (String.IsNullOrEmpty(assemblyName))
			{
				return;
			}

			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture,
			                                "##teamcity[testSuiteStarted name='{0}']",
			                                Formatter.FormatValue(assemblyName)));
		}

		public void TestSuiteFinished(string assemblyName)
		{
			if (String.IsNullOrEmpty(assemblyName))
			{
				return;
			}

			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture,
			                                "##teamcity[testSuiteFinished name='{0}']",
			                                Formatter.FormatValue(assemblyName)));
		}

		public void TestStarted(string testName)
		{
			if (String.IsNullOrEmpty(testName))
			{
				return;
			}

			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture,
			                                "##teamcity[testStarted name='{0}']",
			                                Formatter.FormatValue(testName)));
		}

		public void TestIgnored(string testName, string message)
		{
			if (String.IsNullOrEmpty(testName))
			{
				return;
			}

			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture,
			                                "##teamcity[testIgnored name='{0}' message='{1}']",
			                                Formatter.FormatValue(testName),
			                                Formatter.FormatValue(message)));
		}

		public void TestFailed(string testName, Exception exception)
		{
			TestFailed(testName, ExceptionInfo.FromException(exception));
		}

		public void TestFailed(string testName, ExceptionInfo exceptionInfo)
		{
			if (String.IsNullOrEmpty(testName))
			{
				return;
			}

			StringBuilder message = new StringBuilder();
			message.AppendFormat(CultureInfo.InvariantCulture,
			                     "##teamcity[testFailed name='{0}'",
			                     Formatter.FormatValue(testName));

			if (exceptionInfo != null)
			{
				StringBuilder formattedException = new StringBuilder();
				Formatter.FormatException(exceptionInfo, formattedException);
				Formatter.FormatValue(formattedException);

				message.AppendFormat(CultureInfo.InvariantCulture,
				                     " message='{0}' details='{1}' type='{2}'",
				                     Formatter.FormatValue(exceptionInfo.Message),
				                     formattedException,
				                     Formatter.FormatValue(exceptionInfo.Type));
			}

			message.Append("]");

			_writer.WriteLine(message.ToString());
		}

		public void TestFailed(string testName, string message, string stackTrace, string typeName)
		{
			if (String.IsNullOrEmpty(testName))
			{
				return;
			}

			StringBuilder line = new StringBuilder();
			line.AppendFormat(CultureInfo.InvariantCulture,
			                  "##teamcity[testFailed name='{0}'",
			                  Formatter.FormatValue(testName));

			line.AppendFormat(CultureInfo.InvariantCulture,
			                  " message='{0}' details='{1}' type='{2}'",
			                  Formatter.FormatValue(message),
			                  Formatter.FormatValue(stackTrace),
			                  Formatter.FormatValue(typeName));

			line.Append("]");

			_writer.WriteLine(line.ToString());
		}

		public void TestOutputStream(string testName, string outputStream)
		{
			if (String.IsNullOrEmpty(testName))
			{
				return;
			}

			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture,
			                                "##teamcity[testStdOut name='{0}' out='{1}']",
			                                Formatter.FormatValue(testName),
			                                Formatter.FormatValue(outputStream)));
		}

		public void TestErrorStream(string testName, string errorStream)
		{
			if (String.IsNullOrEmpty(testName))
			{
				return;
			}

			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture,
			                                "##teamcity[testStdErr name='{0}' out='{1}']",
			                                Formatter.FormatValue(testName),
			                                Formatter.FormatValue(errorStream)));
		}

		public void TestFinished(string testName)
		{
			TestFinished(testName, TimeSpan.MinValue);
		}

		public void TestFinished(string testName, TimeSpan duration)
		{
			if (String.IsNullOrEmpty(testName))
			{
				return;
			}

			StringBuilder message = new StringBuilder();
			message.AppendFormat(CultureInfo.InvariantCulture,
			                     "##teamcity[testFinished name='{0}'",
			                     Formatter.FormatValue(testName));

			if (duration.TotalMilliseconds >= 0)
			{
				message.AppendFormat(CultureInfo.InvariantCulture, " duration='{0}'", duration.TotalMilliseconds);
			}

			message.Append("]");

			_writer.WriteLine(message.ToString());
		}

		public void BuildNumber(string newBuildNumber)
		{
			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture,
			                                "##teamcity[buildNumber '{0}']",
			                                Formatter.FormatValue(newBuildNumber)));
		}

		public void BuildStatus(string status, string message)
		{
			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture,
			                                "##teamcity[buildStatus status='{0}' text='{1}']",
			                                Formatter.FormatValue(status).ToUpperInvariant(),
			                                Formatter.FormatValue(message)));
		}

		public void PublishBuildArtifacts(string pathToArtifacts)
		{
			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture,
			                                "##teamcity[publishArtifacts '{0}']",
			                                Formatter.FormatValue(pathToArtifacts)));
		}

		public void ProgressStart(string message)
		{
			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture,
			                                "##teamcity[progressStart '{0}']",
			                                Formatter.FormatValue(message)));
		}

		public void ProgressMessage(string message)
		{
			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture,
			                                "##teamcity[progressMessage '{0}']",
			                                Formatter.FormatValue(message)));
		}

		public void ProgressFinished(string message)
		{
			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture,
			                                "##teamcity[progressFinish '{0}']",
			                                Formatter.FormatValue(message)));
		}

		public void ImportData(string type, string pathToFile)
		{
			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture,
			                                "##teamcity[importData id='{0}' file='{1}']",
			                                Formatter.FormatValue(type),
			                                Formatter.FormatValue(pathToFile)));
		}

		public void BuildStatisticValue(string key, string value)
		{
			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture,
			                                "##teamcity[buildStatisticValue key='{0}' value='{1}']",
			                                Formatter.FormatValue(key),
			                                Formatter.FormatValue(value)));
		}

		public void SendMessage(string message, params object[] parameters)
		{
			if (String.IsNullOrEmpty(message))
			{
				return;
			}

			_writer.WriteLine(String.Format(CultureInfo.InvariantCulture, message, Formatter.FormatValues(parameters)));
		}
		#endregion
	}
}