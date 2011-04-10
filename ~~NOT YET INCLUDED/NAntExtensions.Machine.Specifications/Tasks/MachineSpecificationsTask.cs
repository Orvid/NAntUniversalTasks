using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Machine.Specifications.Model;
using Machine.Specifications.Reporting;
using Machine.Specifications.Runner;
using Machine.Specifications.Runner.Impl;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

using NAntExtensions.Machine.Specifications.RunListeners;
using NAntExtensions.TeamCity.Common.BuildEnvironment;
using NAntExtensions.TeamCity.Common.Container;
using NAntExtensions.TeamCity.Common.Helper;
using NAntExtensions.TeamCity.Common.Messaging;

namespace NAntExtensions.Machine.Specifications.Tasks
{
	/// <summary>
	/// Runs <see href="http://www.assembla.com/wiki/show/machine">Machine.Specifications</see> tests.
	/// </summary>
	/// <remarks>
	/// If the task is run within a TeamCity build, the test results are reported to TeamCity.</remarks>
	/// <example>
	/// <code>
	/// <![CDATA[
	/// <mspec report-directory="reports"
	///	       report-filename="Assembly.Report.html"
	///        workingdir="build-directory"
	///        include-time-info="true"
	///        verbose="true">
	///        <assemblies>
	///            <include name="build-directory/Assembly.dll" />
	///        </assemblies>
	/// </mspec>
	/// ]]></code>
	/// </example>
	[TaskName("mspec")]
	public class MachineSpecificationsTask : Task
	{
		IBuildEnvironment _buildEnvironment;
		DirectoryInfo _workingDirectory;

		/// <summary>
		/// Initializes a new instance of the <see cref="MachineSpecificationsTask"/> class.
		/// </summary>
		public MachineSpecificationsTask() : this(IoC.Resolve<IBuildEnvironment>())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MachineSpecificationsTask"/> class.
		/// </summary>
		/// <param name="buildEnvironment">The build environment.</param>
		public MachineSpecificationsTask(IBuildEnvironment buildEnvironment)
		{
			BuildEnvironment = buildEnvironment;
		}

		IBuildEnvironment BuildEnvironment
		{
			get { return _buildEnvironment; }
			set
			{
				Ensure.ArgumentIsNotNull(value, "value");
				_buildEnvironment = value;
			}
		}

		/// <summary>
		/// The assemblies to include in the test run.
		/// </summary>
		/// <value>The assemblies.</value>
		[BuildElementArray("assemblies", Required = true, ElementType = typeof(FileSet))]
		public FileSet[] Assemblies
		{
			get;
			set;
		}

		/// <summary>
		/// Target output folder for the reports.
		/// </summary>
		/// <value>The report directory.</value>
		[TaskAttribute("report-directory")]
		public string ReportDirectory
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the report file name.
		/// </summary>
		/// <value>The report file name format.</value>
		[TaskAttribute("report-filename")]
		public string ReportFilename
		{
			get;
			set;
		}

		/// <summary>
		/// The directory in which the test run will be executed. If you do not specify this value, the current directory is used.
		/// </summary>
		/// <value>The working directory.</value>
		[TaskAttribute("workingdir")]
		public DirectoryInfo WorkingDirectory
		{
			get
			{
				if (_workingDirectory == null)
				{
					return new DirectoryInfo(Environment.CurrentDirectory);
				}
				return _workingDirectory;
			}
			set { _workingDirectory = value; }
		}

		/// <summary>
		///  <see langword="true" />, if the time when the report has been generated should be included in the report; otherwise, <see langword="false" />.
		/// The default value is <see langword="false" />.
		/// </summary>
		[TaskAttribute("include-time-info")]
		[BooleanValidator]
		public bool IncludeTimeInfo
		{
			get;
			set;
		}

		/// <summary>
		/// Executes the task.
		/// </summary>
		protected override void ExecuteTask()
		{
			DisplayTaskConfiguration();

			if (FileSetHelper.Count(Assemblies) == 0)
			{
				Log(Level.Warning, "No specification assemblies, aborting task");
				return;
			}

			List<ISpecificationRunListener> listeners = SetUpListeners();
			NAntRunListener nantRunListener = new NAntRunListener(this);
			listeners.Add(nantRunListener);

			AggregateRunListener rootListener = new AggregateRunListener(listeners);
			ISpecificationRunner runner = new AppDomainRunner(rootListener, RunOptions.Default);

			RunSpecifications(Assemblies, runner);

			Log(Level.Info, "Finished running specs");

			if (nantRunListener.FailureOccurred)
			{
				throw new BuildException("There were failing specifications. Please see the build log.");
			}
		}

		void DisplayTaskConfiguration()
		{
			Log(Level.Info, "Machine.Specifications {0} test runner", typeof(Subject).Assembly.GetName().Version);

			Log(Level.Verbose, "Assemblies:");
			foreach (string fileName in FileSetHelper.Flatten(Assemblies))
			{
				Log(Level.Verbose, "\t{0}", fileName);
			}

			Log(Level.Verbose, "Working directory: {0}", WorkingDirectory);
			Log(Level.Verbose, "Report directory: {0}", ReportDirectory);
			Log(Level.Verbose, "Report file name: {0}", ReportFilename);
		}

		void RunSpecifications(FileSet[] assemblies, ISpecificationRunner runner)
		{
			Ensure.ArgumentIsNotNull(assemblies, "assemblies");
			Ensure.ArgumentIsNotNull(runner, "runner");

			foreach (string fileName in FileSetHelper.Flatten(assemblies))
			{
				RunSpecificationAssembly(fileName, runner);
			}
		}

		void RunSpecificationAssembly(string fileName, ISpecificationRunner runner)
		{
			Ensure.ArgumentIsNotNullOrEmptyString(fileName, "fileName");

			try
			{
				string originalWorkingDirectory = Environment.CurrentDirectory;
				try
				{
					Environment.CurrentDirectory = WorkingDirectory.FullName;

					Log(Level.Info, "Loading assembly {0}", fileName);
					Assembly assembly = Assembly.LoadFrom(fileName);

					runner.RunAssembly(assembly);
				}
				finally
				{
					Environment.CurrentDirectory = originalWorkingDirectory;
				}
			}
			catch (Exception ex)
			{
				throw new BuildException("Unexpected error while running specs", ex);
			}
		}

		List<ISpecificationRunListener> SetUpListeners()
		{
			List<ISpecificationRunListener> result = new List<ISpecificationRunListener>();

			string reportPath = Path.Combine(ReportDirectory, ReportFilename);
			if (!String.IsNullOrEmpty(reportPath))
			{
				GenerateHtmlReportRunListener htmlReport = new GenerateHtmlReportRunListener(reportPath, IncludeTimeInfo);
				result.Add(htmlReport);
			}

			if (BuildEnvironment.IsTeamCityBuild)
			{
				TeamCityRunListener teamCity =
					new TeamCityRunListener(IoC.Resolve<ITeamCityMessageProvider>(new { taskToUseForLogging = this }));

				result.Add(teamCity);
			}

			return result;
		}
	}
}