using System;

using MbUnit.Core.Reports.Serialization;

using NAnt.Core;

using NAntExtensions.TeamCity.Common.Container;
using NAntExtensions.TeamCity.Common.Helper;
using NAntExtensions.TeamCity.Common.Messaging;

namespace NAntExtensions.MbUnit.Reporting
{
	internal class TeamCityReportGenerator
	{
		public void Render(ReportResult result, ITeamCityMessageProvider messageProvider)
		{
			foreach (ReportAssembly reportAssembly in result.Assemblies)
			{
				messageProvider.TestSuiteStarted(reportAssembly.Name);

				foreach (ReportNamespace reportNamespace in reportAssembly.Namespaces)
				{
					RecurseNameSpaces(reportNamespace, messageProvider);
				}

				messageProvider.TestSuiteFinished(reportAssembly.Name);
			}
		}

		static void RecurseNameSpaces(ReportNamespace reportNamespace, ITeamCityMessageProvider messageProvider)
		{
			foreach (ReportFixture reportFixture in reportNamespace.Fixtures)
			{
				foreach (ReportRun run in reportFixture.Runs)
				{
					messageProvider.TestStarted(run.Name);

					if (run.Result == ReportRunResult.Ignore || run.Result == ReportRunResult.Skip)
					{
						messageProvider.TestIgnored(run.Name, run.Result.ToString());
					}
					else if (run.Result == ReportRunResult.Failure)
					{
						ExceptionInfo exceptionInfo = GetExceptionInfo(run.Exception);

						messageProvider.TestFailed(run.Name, exceptionInfo);
					}

					if (!string.IsNullOrEmpty(run.ConsoleOut))
					{
						messageProvider.TestOutputStream(run.Name, run.ConsoleOut);
					}
					if (!string.IsNullOrEmpty(run.ConsoleError))
					{
						messageProvider.TestErrorStream(run.Name, run.ConsoleError);
					}

					messageProvider.TestFinished(run.Name, TimeSpan.FromSeconds(run.Duration));
				}
			}

			foreach (ReportNamespace child in reportNamespace.Namespaces)
			{
				RecurseNameSpaces(child, messageProvider);
			}
		}

		static ExceptionInfo GetExceptionInfo(ReportException reportException)
		{
			if (reportException == null)
			{
				return null;
			}

			ExceptionInfo exceptionInfo = new ExceptionInfo();
			exceptionInfo.Type = reportException.Type;
			exceptionInfo.Message = reportException.Message;
			exceptionInfo.Source = reportException.Source;
			exceptionInfo.StackTrace = reportException.StackTrace;

			if (reportException.Exception != null)
			{
				exceptionInfo.InnerException = GetExceptionInfo(reportException.Exception);
			}

			return exceptionInfo;
		}

		public static void RenderReport(ReportResult result, Task task)
		{
			Ensure.ArgumentIsNotNull(result, "result");
			Ensure.ArgumentIsNotNull(task, "task");

			ITeamCityMessageProvider messageProvider = IoC.Resolve<ITeamCityMessageProvider>(new { taskToUseForLogging = task });

			TeamCityReportGenerator report = new TeamCityReportGenerator();
			report.Render(result, messageProvider);
		}
	}
}