using NAnt.Core;
using NAnt.Core.Attributes;

using NAntExtensions.TeamCity.Common.BuildEnvironment;
using NAntExtensions.TeamCity.Common.Messaging;

namespace NAntExtensions.TeamCity.Tasks
{
	/// <summary>
	/// Publishes build artifacts while the build is still running.
	/// </summary>
	/// <remarks>This task will only be executed within a TeamCity build. This message is supported by TeamCity 4.x and above.
	/// If you import FxCop results, these will appear in the Code Inspection tab of the build results page.</remarks>
	/// <example>
	/// <code>
	/// <![CDATA[
	/// <tc-importdata type="FxCop"
	///	               path="build-directory/FxCop.xml" />
	/// ]]></code>
	/// </example>
	/// <seealso href="http://www.jetbrains.net/confluence/display/TCD4/Build+Script+Interaction+with+TeamCity#BuildScriptInteractionwithTeamCity-ReportingFxCopInspectionResults">
	/// Build Script Interaction with TeamCity</seealso>
	/// <seealso href="http://www.jetbrains.net/confluence/display/TCD4/FxCop_#FxCop_-UsingServiceMessages">
	/// Importing FxCop results to TeamCity</seealso>
	[TaskName("tc-importdata")]
	public class ImportDataTask : MessageTask
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ImportDataTask"/> class.
		/// </summary>
		public ImportDataTask() : this(null, null)
		{
		}

		internal ImportDataTask(IBuildEnvironment environment, ITeamCityMessageProvider messageProvider)
			: base(environment, messageProvider)
		{
		}

		/// <summary>
		/// The type of data to import.
		/// </summary>
		[TaskAttribute("type", Required = true)]
		public string Type
		{
			get;
			set;
		}
		
		/// <summary>
		/// The data file to import.
		/// </summary>
		[TaskAttribute("path", Required = true)]
		public string PathToFile
		{
			get;
			set;
		}

		/// <summary>
		/// Executes the task.
		/// </summary>
		protected override void ExecuteTask()
		{
			if (ShouldSkipTaskExecution)
			{
				return;
			}

			Log(Level.Verbose, "Importing data. Type={0}, Path={1}", Type, PathToFile);

			MessageProvider.ImportData(Type, PathToFile);
		}
	}
}