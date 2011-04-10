using NAnt.Core;
using NAnt.Core.Attributes;

using NAntExtensions.Machine.Specifications.Types;
using NAntExtensions.TeamCity.Common.Helper;

namespace NAntExtensions.Machine.Specifications.Tasks
{
	/// <summary>
	/// Initializes all <see href="http://www.assembla.com/wiki/show/machine">Machine.Specifications</see> counters with a
	/// value of 0. Existing property values will be overwritten.
	/// </summary>
	/// <remarks>
	/// The <see cref="MachineSpecificationsTask"/> updates these NAnt properties when the test run has completed:
	/// <list type="bullet">
	/// <listheader><term>NAnt property name</term><description>Description</description></listheader>
	/// <item><term>mspec.contexts</term><description>The number of contexts.</description></item>
	/// <item><term>mspec.specs</term><description>The number of specifications.</description></item>
	/// <item><term>mspec.passedspecs</term><description>The number of successful specifications.</description></item>
	/// <item><term>mspec.failedspecs</term><description>The number of failed specifications.</description></item>
	/// <item><term>mspec.ignoredspecs</term><description>The number of ignored specifications.</description></item>
	/// <item><term>mspec.unimplementedspecs</term><description>The number of unimplemented specifications.</description>
	/// </item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <code>
	/// <![CDATA[
	/// <mspec-initcounters />
	/// ]]></code>
	/// </example>
	[TaskName("mspec-initcounters")]
	public class InitializeMachineSpecificationsCountersTask : Task
	{
		/// <summary>
		/// Executes the task.
		/// </summary>
		protected override void ExecuteTask()
		{
			foreach (string counter in Counter.All)
			{
				PropertyDictionaryHelper.SetInt(Properties, counter, 0);	
			}
		}
	}
}