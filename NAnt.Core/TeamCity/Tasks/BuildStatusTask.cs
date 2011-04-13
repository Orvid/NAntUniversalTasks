using NAnt.Core;
using NAnt.Core.Attributes;

using NAntExtensions.TeamCity.Common.BuildEnvironment;
using NAntExtensions.TeamCity.Common.Messaging;
using NAntExtensions.TeamCity.Types;

namespace NAntExtensions.TeamCity.Tasks
{
	/// <summary>
	/// Reports the build status to TeamCity.
	/// </summary>
	/// <remarks>This task will only be executed within a TeamCity build.</remarks>
	/// <example>
	/// Sets the build status to "failed" and replaces the current message.
	/// <code>
	/// <![CDATA[
	/// <tc-buildstatus message="The build has failed"
	///                 type="Failure" />
	/// ]]></code>
	/// </example>
	/// <example>
	/// Appends the code coverage value to the default build status message.<para><c>{build.status.text}</c> is an optional
	/// substitution pattern which represents the default status message, calculated automatically by TeamCity using passed
	/// test count, compilation messages and so on. Note that it is not possible to append multiple messages to the build
	/// status, as <c>{build.status.text}</c> resolves to the default build status text as calculated by TeamCity. To append
	/// multiple messages, use the <see cref="StatusTextTask"/>.</para><para>The resulting message will be
	/// <c><![CDATA[<default status text>, Code Coverage <x>% ]]></c>.</para>
	/// <code>
	/// <![CDATA[
	/// <tc-buildstatus message="{build.status.text}, Code coverage: ${math::round(double::parse(codecoverage))}%" />
	/// ]]></code>
	/// </example>
	/// <seealso href="http://www.jetbrains.net/confluence/display/TCD3/Build+Script+Interaction+with+TeamCity#BuildScriptInteractionwithTeamCity-ReportingBuildStatus">
	/// Build Script Interaction with TeamCity</seealso>
	[TaskName("tc-buildstatus")]
	public class BuildStatusTask : MessageTask
	{
		StatusType _statusType;
		bool _statusTypeHasBeenSetExplicitly;

		/// <summary>
		/// Initializes a new instance of the <see cref="BuildStatusTask"/> class.
		/// </summary>
		public BuildStatusTask() : this(null, null)
		{
		}

		internal BuildStatusTask(IBuildEnvironment environment, ITeamCityMessageProvider messageProvider)
			: base(environment, messageProvider)
		{
		}

		/// <summary>
		/// The status type to report to TeamCity. If you do not specify this value, just the <see cref="Message"/> is reported.
		/// </summary>
		/// <value>The type of the status.</value>
		[TaskAttribute("type")]
		public StatusType StatusType
		{
			get { return _statusType; }
			set
			{
				_statusType = value;
				_statusTypeHasBeenSetExplicitly = true;
			}
		}

		/// <summary>
		/// The message to include with the status report.
		/// </summary>
		/// <value>The message.</value>
		[TaskAttribute("message")]
		public string Message
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

			string statusType = null;
			if (_statusTypeHasBeenSetExplicitly)
			{
				statusType = StatusType.ToString();
			}

			Log(Level.Verbose, "Reporting build status. Type={0}, Message={1}", statusType ?? "(null)", Message ?? "(null)");

			MessageProvider.BuildStatus(statusType, Message);
		}
	}
}