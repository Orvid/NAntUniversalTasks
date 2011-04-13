using NAnt.Core;
using NAnt.Core.Attributes;

using NAntExtensions.MbUnit.Types;
using NAntExtensions.TeamCity.Common.Helper;

namespace NAntExtensions.MbUnit.Tasks
{
	/// <summary>
	/// Initializes all <see href="http://www.mbunit.com/">MbUnit</see> counters with a value of 0. Existing property values
	/// will be overwritten.
	/// </summary>
	/// <remarks>
	/// The <see cref="MbUnitTask"/> updates these NAnt properties when the test run has completed:
	/// <list type="bullet">
	/// <listheader><term>NAnt property name</term><description>Description</description></listheader>
	/// <item><term>mbunit.asserts</term><description>The number of asserts.</description></item>
	/// <item><term>mbunit.failures</term><description>The number of failed tests.</description></item>
	/// <item><term>mbunit.ignored</term><description>The number of ignored tests.</description></item>
	/// <item><term>mbunit.run</term><description>The number of tests run.</description></item>
	/// <item><term>mbunit.skipped</term><description>The number of skipped tests.</description></item>
	/// <item><term>mbunit.successes</term><description>The number of successful test.</description></item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <code>
	/// <![CDATA[
	/// <mbunit-initcounters />
	/// ]]></code>
	/// </example>
	[TaskName("mbunit-initcounters")]
	public class InitializeMbUnitCountersTask : Task
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