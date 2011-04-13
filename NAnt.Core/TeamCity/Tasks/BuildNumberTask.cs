using NAnt.Core;
using NAnt.Core.Attributes;

using NAntExtensions.TeamCity.Common.BuildEnvironment;
using NAntExtensions.TeamCity.Common.Messaging;

namespace NAntExtensions.TeamCity.Tasks
{
	/// <summary>
	/// Reports the build number to TeamCity.
	/// </summary>
	/// <remarks>This task will only be executed within a TeamCity build.</remarks>
	/// <example>
	/// In the <c>build-number</c> attribute, you can use the <c>{build.number}</c> placeholder to include the current build
	/// number provided by TeamCity.
	/// <code>
	/// <![CDATA[
	/// <tc-buildnumber build-number="{build.number}-internal" />
	/// ]]></code>
	/// </example>
	/// <seealso href="http://www.jetbrains.net/confluence/display/TCD3/Build+Script+Interaction+with+TeamCity#BuildScriptInteractionwithTeamCity-ReportingBuildNumber">
	/// Build Script Interaction with TeamCity</seealso>
	[TaskName("tc-buildnumber")]
	public class BuildNumberTask : MessageTask
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BuildNumberTask"/> class.
		/// </summary>
		public BuildNumberTask() : this(null, null)
		{
		}

		internal BuildNumberTask(IBuildEnvironment environment, ITeamCityMessageProvider messageProvider)
			: base(environment, messageProvider)
		{
		}

		/// <summary>
		/// The new build number.
		/// </summary>
		[TaskAttribute("build-number", Required = true)]
		public string BuildNumber
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

			Log(Level.Verbose, "Reporting build number. Number={0}", BuildNumber);

			MessageProvider.BuildNumber(BuildNumber);
		}
	}
}