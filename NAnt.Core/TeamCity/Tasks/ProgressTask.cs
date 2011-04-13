using System;

using NAnt.Core;
using NAnt.Core.Attributes;

using NAntExtensions.TeamCity.Common.BuildEnvironment;
using NAntExtensions.TeamCity.Common.Messaging;
using NAntExtensions.TeamCity.Types;

namespace NAntExtensions.TeamCity.Tasks
{
	/// <summary>
	/// Reports the build progress to TeamCity.
	/// </summary>
	/// <remarks>This task will only be executed within a TeamCity build.</remarks>
	/// <example>
	/// <code>
	/// <![CDATA[
	/// <tc-progress message="Running tests"
	///              type="Start" />
	/// 
	/// ... some build activity ...
	/// 
	/// <tc-progress message="Running tests"
	///              type="End" />
	/// ]]></code>
	/// </example>
	/// <example>
	/// <code>
	/// <![CDATA[
	/// <tc-progress message="Something happened"
	///              type="Message" />
	/// ]]></code>
	/// </example>
	/// <seealso href="http://www.jetbrains.net/confluence/display/TCD3/Build+Script+Interaction+with+TeamCity#BuildScriptInteractionwithTeamCity-progressReporting">
	/// Build Script Interaction with TeamCity</seealso>
	[TaskName("tc-progress")]
	public class ProgressTask : MessageTask
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ProgressTask"/> class.
		/// </summary>
		public ProgressTask() : this(null, null)
		{
		}

		internal ProgressTask(IBuildEnvironment environment, ITeamCityMessageProvider messageProvider)
			: base(environment, messageProvider)
		{
		}

		/// <summary>
		/// The progress type to report to TeamCity. The default value is <see cref="NAntExtensions.TeamCity.Types.ProgressType.Message"/>.
		/// </summary>
		/// <value>The type of the progress.</value>
		[TaskAttribute("type", Required = false)]
		public ProgressType ProgressType
		{
			get;
			set;
		}

		/// <summary>
		/// The message to show on the TeamCity dashboard.
		/// </summary>
		[TaskAttribute("message", Required = false)]
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

			Log(Level.Verbose, "Reporting progress. Type={0}, Message={1}", ProgressType, Message ?? "(null)");

			switch (ProgressType)
			{
				case ProgressType.Message:
					MessageProvider.ProgressMessage(Message);
					break;
				case ProgressType.Start:
					MessageProvider.ProgressStart(Message);
					break;
				case ProgressType.End:
					MessageProvider.ProgressFinished(Message);
					break;
				default:
					throw new BuildException(String.Format("Unknown progress type: '{0}'", ProgressType));
			}
		}
	}
}