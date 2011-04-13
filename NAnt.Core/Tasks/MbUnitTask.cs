using System;
using System.IO;

using MbUnit.Core;
using MbUnit.Core.Filters;
using MbUnit.Core.Graph;
using MbUnit.Core.Reports;
using MbUnit.Core.Reports.Serialization;
using MbUnit.Framework;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

using NAntExtensions.MbUnit.Reporting;
using NAntExtensions.MbUnit.Types;
using NAntExtensions.TeamCity.Common.BuildEnvironment;
using NAntExtensions.TeamCity.Common.Container;
using NAntExtensions.TeamCity.Common.Helper;

namespace NAntExtensions.MbUnit.Tasks
{
	/// <summary>
	/// Runs <see href="http://www.mbunit.com/">MbUnit</see> tests.
	/// </summary>
	/// <remarks>
	/// If the task is run within a TeamCity build, the test results are reported to TeamCity. Note that due to the way MbUnit
	/// logs its test results the timing information for the  tests shown by TeamCity 3.x are not valid. However, the durations
	/// shown by TeamCity 4.x and above are correct.</remarks>
	/// <example>
	/// Runs all tests in <c>Assembly.dll</c> and generates one HTML (<c>Assembly.Report.html</c>) and one XML 
	/// (<c>Assembly.Report.xml</c>) report in the <c>reports</c> directory. Test progress is reported (<c>verbose="true"</c>).
	/// <code>
	/// <![CDATA[
	/// <mbunit report-types="html;xml"
	///         report-filename-format="Assembly.Report"
	///         report-directory="reports"
	///         verbose="true"
	///         workingdir="build-directory">
	///         <assemblies>
	///             <include name="build-directory/Assembly.dll" />
	///         </assemblies>
	/// </mbunit>
	/// ]]></code>
	/// </example>
	/// <example>
	/// Runs all tests in <c>Assembly.dll</c> except those marked with <c>FixtureCategoryAttribute("Integration")</c>. Does not
	/// generate test reports and does not report progress.
	/// <code>
	/// <![CDATA[
	/// <mbunit workingdir="build-directory">
	///         <assemblies>
	///             <include name="build-directory/Assembly.dll" />
	///         </assemblies>
	///	        <categories>
	///	            <exclude name="Integration" />
	///	        </categories>
	///	</mbunit>
	/// ]]></code>
	/// </example>
	[TaskName("mbunit")]
	public class MbUnitTask : Task
	{
		IBuildEnvironment _buildEnvironment;
		string _reportFileNameFormat = "mbunit-{0}{1}";
		string _transformReportFileNameFormat;
		DirectoryInfo _workingDirectory;

		/// <summary>
		/// Initializes a new instance of the <see cref="MbUnitTask"/> class.
		/// </summary>
		public MbUnitTask()
			: this(IoC.Resolve<IBuildEnvironment>())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MbUnitTask"/> class.
		/// </summary>
		/// <param name="buildEnvironment">The build environment.</param>
		public MbUnitTask(IBuildEnvironment buildEnvironment)
		{
			BuildEnvironment = buildEnvironment;

			FilterAuthors = new IncludeSet();
			FilterCategories = new FilterSet();
			FilterNamespaces = new IncludeSet();
			FilterTypes = new IncludeSet();
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
		/// Gets or sets the report format to generate. Valid values are: <see langword="text" />, <see langword="xml" />, 
		/// <see langword="dox" />, <see langword="html" /> and <see langword="transform" />. If you specify 
		/// <see langword="transform" />, you will also have to set the <see cref="Transform"/> attribute. You can set multiple
		/// values separated by a semicolon (<see langword=";" />).
		/// </summary>
		/// <value>The report types.</value>
		[TaskAttribute("report-types")]
		public string ReportTypes
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the assemblies to include in the test run.
		/// </summary>
		/// <value>The assemblies.</value>
		[BuildElementArray("assemblies", Required = true, ElementType = typeof(FileSet))]
		public FileSet[] Assemblies
		{
			get;
			set;
		}

		/// <summary>
		/// Path where the assemblies can be loaded.
		/// </summary>
		/// <value>The assembly paths.</value>
		[BuildElement("assembly-paths", Required = false)]
		public DirSet AssemblyPaths
		{
			get;
			set;
		}

		/// <summary>
		/// The report file name format for all reports except the <see langword="transform" /> report. The default value is <c>mbunit-result-{0}{1}</c>
		/// where <c>{0}</c> is replaced by the date and <c>{1}</c> is replaced by the time.
		/// </summary>
		/// <value>The report file name format.</value>
		[TaskAttribute("report-filename-format", Required = false)]
		public string ReportFileNameFormat
		{
			get { return _reportFileNameFormat; }
			set { _reportFileNameFormat = value; }
		}

		/// <summary>
		/// Target output folder for the reports.
		/// </summary>
		/// <value>The report directory.</value>
		[TaskAttribute("report-directory", Required = false)]
		public string ReportDirectory
		{
			get;
			set;
		}

		/// <summary>
		/// The XSL transformation file to use for the <see langword="transform" /> report.
		/// </summary>
		/// <value>The transform.</value>
		[TaskAttribute("transform", Required = false)]
		public FileInfo Transform
		{
			get;
			set;
		}

		/// <summary>
		/// The report file name format to use for the <see langword="transform" /> report. If you do not specify this value, the
		/// value from 
		/// <see cref="ReportFileNameFormat"/> is used.
		/// </summary>
		/// <value>The transform report file name format.</value>
		[TaskAttribute("transform-report-filename-format", Required = false)]
		public string TransformReportFileNameFormat
		{
			get
			{
				if (String.IsNullOrEmpty(_transformReportFileNameFormat))
				{
					return ReportFileNameFormat;
				}
				return _transformReportFileNameFormat;
			}
			set { _transformReportFileNameFormat = value; }
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
		/// The categories to include in and exclude from the test run (see MbUnit's <see cref="FixtureCategoryAttribute"/>).
		/// </summary>
		[BuildElement("categories", Required = false)]
		public FilterSet FilterCategories
		{
			get;
			set;
		}

		/// <summary>
		/// Test fixtures by these authors will be included in the test run (see MbUnit's <see cref="AuthorAttribute"/>).
		/// </summary>
		[BuildElement("authors", Required = false)]
		public IncludeSet FilterAuthors
		{
			get;
			set;
		}

		/// <summary>
		/// Test fixtures in namespaces starting with these values will be included in the test run.
		/// </summary>
		[BuildElement("namespaces", Required = false)]
		public IncludeSet FilterNamespaces
		{
			get;
			set;
		}

		/// <summary>
		/// Test fixtures with these names (full names, that is, namespace and type name) will be included in the test run.
		/// </summary>
		[BuildElement("types", Required = false)]
		public IncludeSet FilterTypes
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
				Log(Level.Warning, "No test assemblies, aborting task");
				return;
			}

			ReportResult result = RunTests(Assemblies);
			GenerateReports(result, ReportTypes);

			if (result.Counter.FailureCount > 0)
			{
				throw new BuildException("There were failing tests. Please see the build log.");
			}
		}

		void DisplayTaskConfiguration()
		{
			Log(Level.Info, "MbUnit {0} test runner", typeof(Fixture).Assembly.GetName().Version);

			Log(Level.Verbose, "Assemblies:");
			foreach (string fileName in FileSetHelper.Flatten(Assemblies))
			{
				Log(Level.Verbose, "\t{0}", fileName);
			}

			Log(Level.Verbose, "Working directory: {0}", WorkingDirectory);
			Log(Level.Verbose, "Report types: {0}", ReportTypes);
			Log(Level.Verbose, "Report directory: {0}", ReportDirectory);
			Log(Level.Verbose, "Report file name format: {0}", ReportFileNameFormat);
			Log(Level.Verbose, "Transform: {0}", Transform == null ? String.Empty : Transform.FullName);
			Log(Level.Verbose, "Transform report file name format: {0}", TransformReportFileNameFormat);
		}

		ReportResult RunTests(FileSet[] assemblies)
		{
			Ensure.ArgumentIsNotNull(assemblies, "assemblies");

			ReportResult result = new ReportResult();
			foreach (string fileName in FileSetHelper.Flatten(assemblies))
			{
				bool testsFailed = RunTestAssembly(fileName, result);
				if (testsFailed)
				{
					break;
				}
			}

			return result;
		}

		bool RunTestAssembly(string fileName, ReportResult reportResult)
		{
			Ensure.ArgumentIsNotNullOrEmptyString(fileName, "fileName");

			string[] assemblyNames = new[] { fileName };
			string[] dirNames = null;
			if (AssemblyPaths != null)
			{
				dirNames = new string[AssemblyPaths.DirectoryNames.Count];
				AssemblyPaths.DirectoryNames.CopyTo(dirNames, 0);
			}

			try
			{
				Log(Level.Info, "Loading assembly {0}", fileName);
				using (
					TestDomainDependencyGraph graph = TestDomainDependencyGraph.BuildGraph(assemblyNames,
					                                                                       dirNames,
					                                                                       CreateFilter(),
					                                                                       Verbose))
				{
					graph.Log += TestRunInfo_Log;
					string originalWorkingDirectory = Environment.CurrentDirectory;

					try
					{
						Environment.CurrentDirectory = WorkingDirectory.FullName;

						ReportResult runResult = graph.RunTests();
						Log(Level.Info, "Finished running tests");

						Log(Level.Info, "Merging results");
						reportResult.Merge(runResult);

						UpdateNAntProperties(Properties, runResult);

						return runResult.Counter.FailureCount == 0;
					}
					finally
					{
						Environment.CurrentDirectory = originalWorkingDirectory;
						graph.Log -= TestRunInfo_Log;
					}
				}
			}
			catch (Exception ex)
			{
				throw new BuildException("Unexpected error while running tests", ex);
			}
		}

		void GenerateReports(ReportResult reportResult, string reportTypes)
		{
			Ensure.ArgumentIsNotNull(reportResult, "reportResult");

			if (BuildEnvironment.IsTeamCityBuild)
			{
				TeamCityReportGenerator.RenderReport(reportResult, this);
			}

			if (String.IsNullOrEmpty(reportTypes))
			{
				return; 
			}
			
			Log(Level.Info, "Generating reports");
			foreach (string reportType in reportTypes.Split(';'))
			{
				string reportFileName = null;
				Log(Level.Verbose, "Report type: {0}", reportType);
				switch (reportType.ToLower())
				{
					case "text":
						reportFileName = TextReport.RenderToText(reportResult, ReportDirectory, ReportFileNameFormat);
						break;
					case "xml":
						reportFileName = XmlReport.RenderToXml(reportResult, ReportDirectory, ReportFileNameFormat);
						break;
					case "html":
						reportFileName = HtmlReport.RenderToHtml(reportResult, ReportDirectory, ReportFileNameFormat);
						break;
					case "dox":
						reportFileName = DoxReport.RenderToDox(reportResult, ReportDirectory, ReportFileNameFormat);
						break;
					case "transform":
						if (Transform == null)
						{
							throw new BuildException(String.Format("No transform specified for report type '{0}'", reportType));
						}

						reportFileName = HtmlReport.RenderToHtml(reportResult,
						                                         ReportDirectory,
						                                         Transform.FullName,
						                                         TransformReportFileNameFormat);
						break;
					default:
						Log(Level.Error, "Unknown report type {0}", reportType);
						break;
				}

				if (reportFileName != null)
				{
					Log(Level.Info, "Created report at {0}", reportFileName);
				}
			}
		}

		IFixtureFilter CreateFilter()
		{
			FixtureFilterBase filter = FixtureFilters.Any;

			if (FilterAuthors != null)
			{
				foreach (string include in FilterAuthors.GetIncludePatterns())
				{
					filter = FixtureFilters.And(filter, FixtureFilters.Author(include));
				}
			}

			if (FilterCategories != null)
			{
				foreach (string include in FilterCategories.GetIncludePatterns())
				{
					filter = FixtureFilters.And(filter, FixtureFilters.Category(include));
				}

				foreach (string exclude in FilterCategories.GetExcludePatterns())
				{
					filter = FixtureFilters.And(filter, FixtureFilters.Category(exclude, true));
				}
			}

			if (FilterNamespaces != null)
			{
				foreach (string include in FilterNamespaces.GetIncludePatterns())
				{
					filter = FixtureFilters.And(filter, FixtureFilters.Namespace(include));
				}
			}

			if (FilterTypes != null)
			{
				foreach (string include in FilterTypes.GetIncludePatterns())
				{
					filter = FixtureFilters.And(filter, FixtureFilters.Type(include));
				}
			}

			return filter;
		}

		static void UpdateNAntProperties(PropertyDictionary properties, ReportResult reportResult)
		{
			PropertyDictionaryHelper.AddOrUpdateInt(properties, Counter.Asserts, reportResult.Counter.AssertCount);
			PropertyDictionaryHelper.AddOrUpdateInt(properties, Counter.Failures, reportResult.Counter.FailureCount);
			PropertyDictionaryHelper.AddOrUpdateInt(properties, Counter.Ignored, reportResult.Counter.IgnoreCount);
			PropertyDictionaryHelper.AddOrUpdateInt(properties, Counter.Run, reportResult.Counter.RunCount);
			PropertyDictionaryHelper.AddOrUpdateInt(properties, Counter.Skipped, reportResult.Counter.SkipCount);
			PropertyDictionaryHelper.AddOrUpdateInt(properties, Counter.Successes, reportResult.Counter.SuccessCount);
		}

		void TestRunInfo_Log(string message)
		{
			Log(Level.Info, message);
		}
	}
}