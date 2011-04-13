using NAntExtensions.TeamCity.Common.BuildEnvironment;
using NAntExtensions.TeamCity.Common.Container;
using NAntExtensions.TeamCity.Common.Helper;
using NAntExtensions.TeamCity.Common.Messaging;

namespace NAntExtensions.TeamCity.Tasks
{
	/// <summary>
	/// The base class for NAnt tasks that support messaging to TeamCity.
	/// </summary>
	public abstract class MessageTask : TeamCityTask
	{
		ITeamCityMessageProvider _messageProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageTask"/> class.
		/// </summary>
		/// <param name="buildEnvironment">The build environment.</param>
		/// <param name="messageProvider">The message provider.</param>
		protected MessageTask(IBuildEnvironment buildEnvironment, ITeamCityMessageProvider messageProvider)
			: base(buildEnvironment)
		{
			MessageProvider = messageProvider ?? IoC.Resolve<ITeamCityMessageProvider>(new { taskToUseForLogging = this });
		}

		/// <summary>
		/// Gets or sets the message provider.
		/// </summary>
		/// <value>The message provider.</value>
		protected ITeamCityMessageProvider MessageProvider
		{
			get { return _messageProvider; }
			set
			{
				Ensure.ArgumentIsNotNull(value, "value");
				_messageProvider = value;
			}
		}
	}
}