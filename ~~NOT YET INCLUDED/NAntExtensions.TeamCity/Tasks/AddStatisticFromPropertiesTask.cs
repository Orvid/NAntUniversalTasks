using System;
using System.Collections;

using NAnt.Core;
using NAnt.Core.Attributes;

using NAntExtensions.TeamCity.Common.BuildEnvironment;
using NAntExtensions.TeamCity.Common.Messaging;

namespace NAntExtensions.TeamCity.Tasks
{
	/// <summary>
	/// Reports build statistic values to TeamCity, based on NAnt properties matching the specified prefix.
	/// </summary>
	/// <remarks>This task will only be executed within a TeamCity build.</remarks>
	/// <example>
	/// <code>
	/// <![CDATA[
	/// <tc-addstatistic-fromprops starting-with="mspec."
	///                            ignore-case="true" />
	/// ]]></code>
	/// </example>
	/// <seealso href="http://www.jetbrains.net/confluence/display/TCD3/Build+Script+Interaction+with+TeamCity#BuildScriptInteractionwithTeamCity-ReportingandDisplayingCustomStatistics">
	/// Build Script Interaction with TeamCity</seealso>
	[TaskName("tc-addstatistic-fromprops")]
	public class AddStatisticFromPropertiesTask : MessageTask
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AddStatisticFromPropertiesTask"/> class.
		/// </summary>
		public AddStatisticFromPropertiesTask() : this(null, null)
		{
		}

		internal AddStatisticFromPropertiesTask(IBuildEnvironment environment, ITeamCityMessageProvider messageProvider)
			: base(environment, messageProvider)
		{
			IgnoreCase = true;
		}

		/// <summary>
		/// Adds TeamCity statistics values for all NAnt properties starting with this string.
		/// </summary>
		/// <value>The properties starting with.</value>
		[TaskAttribute("starting-with", Required = true)]
		public string PropertiesStartingWith
		{
			get;
			set;
		}

		/// <summary>
		/// Compare property names with a case insensitive comparison. The default value is <see langword="true" />.
		/// </summary>
		[TaskAttribute("ignore-case")]
		public bool IgnoreCase
		{
			get;
			set;
		}

		StringComparison PropertyKeyComparison
		{
			get
			{
				if (IgnoreCase)
				{
					return StringComparison.OrdinalIgnoreCase;
				}

				return StringComparison.Ordinal;
			}
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

			Log(Level.Info,
			    "Reporting statistic values from properties starting with '{0}' (case {1}sensitive)",
			    PropertiesStartingWith,
			    IgnoreCase ? "in" : String.Empty);

			if (Properties == null)
			{
				return;
			}

			foreach (DictionaryEntry property in Properties)
			{
				if (!property.Key.ToString().StartsWith(PropertiesStartingWith, PropertyKeyComparison))
				{
					continue;
				}

				Log(Level.Info, "Reporting build statistic value. Key={0} Value={1}", property.Key, property.Value);
				MessageProvider.BuildStatisticValue(property.Key.ToString(), property.Value.ToString());
			}
		}
	}
}