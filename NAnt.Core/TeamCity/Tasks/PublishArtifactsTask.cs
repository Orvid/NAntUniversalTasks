using NAnt.Core;
using NAnt.Core.Attributes;

using NAntExtensions.TeamCity.Common.BuildEnvironment;
using NAntExtensions.TeamCity.Common.Messaging;

namespace NAntExtensions.TeamCity.Tasks
{
	/// <summary>
	/// Publishes build artifacts while the build is still running.
	/// </summary>
	/// <remarks>This task will only be executed within a TeamCity build. Artifacts that are specified in the build
	/// configuration setting will be published as usual.</remarks>
	/// <example>
	/// <code>
	/// <![CDATA[
	/// <tc-publishartifacts path="build-directory/*.zip" />
	/// ]]></code>
	/// </example>
	/// <seealso href="http://www.jetbrains.net/confluence/display/TCD3/Build+Script+Interaction+with+TeamCity#BuildScriptInteractionwithTeamCity-artPublishing">
	/// Build Script Interaction with TeamCity</seealso>
	[TaskName("tc-publishartifacts")]
	public class PublishArtifactsTask : MessageTask
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PublishArtifactsTask"/> class.
		/// </summary>
		public PublishArtifactsTask() : this(null, null)
		{
		}

		internal PublishArtifactsTask(IBuildEnvironment environment, ITeamCityMessageProvider messageProvider)
			: base(environment, messageProvider)
		{
		}

		/// <summary>
		/// The files matching the path will be visible as artifacts of the running build.
		/// </summary>
		[TaskAttribute("path", Required = true)]
		public string PathToArtifacts
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

			Log(Level.Verbose, "Publishing build artifacts. Path={0}", PathToArtifacts);

			MessageProvider.PublishBuildArtifacts(PathToArtifacts);
		}
	}
}