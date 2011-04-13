using NAnt.Core;
using NAnt.Core.Attributes;

using NAntExtensions.MbUnit.Tasks;
using NAntExtensions.MbUnit.Types;

namespace NAntExtensions.MbUnit.Functions
{
	///<summary>
	/// MbUnit functions.
	///</summary>
	[FunctionSet("mbunit", "MbUnit")]
	public class MbUnitFunctions : FunctionSetBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MbUnitFunctions"/> class.
		/// </summary>
		/// <param name="project">The project.</param>
		/// <param name="properties">The properties.</param>
		public MbUnitFunctions(Project project, PropertyDictionary properties) : base(project, properties)
		{
		}

		/// <summary>
		/// Returns the prefix for the counters that are initialized by the <see cref="InitializeMbUnitCountersTask"/> and updated
		/// by the <see cref="MbUnitTask"/>.
		/// </summary>
		/// <returns>
		/// 	The prefix string.
		/// </returns>
		/// <example>
		/// <code>
		/// <![CDATA[
		/// <echo message="The prefix for the MbUnit counters is: ${mbunit::get-counter-prefix()}" />
		/// ]]></code>
		/// </example>
		[Function("get-counter-prefix")]
		public static string GetCounterPrefix()
		{
			return Counter.Prefix;
		}
	}
}