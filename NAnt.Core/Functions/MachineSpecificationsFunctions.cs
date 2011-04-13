using NAnt.Core;
using NAnt.Core.Attributes;

using NAntExtensions.Machine.Specifications.Tasks;
using NAntExtensions.Machine.Specifications.Types;

namespace NAntExtensions.Machine.Specifications.Functions
{
	///<summary>
	/// Machine.Specifications functions.
	///</summary>
	[FunctionSet("mspec", "Machine.Specifications")]
	public class MachineSpecificationsFunctions : FunctionSetBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MachineSpecificationsFunctions"/> class.
		/// </summary>
		/// <param name="project">The project.</param>
		/// <param name="properties">The properties.</param>
		public MachineSpecificationsFunctions(Project project, PropertyDictionary properties) : base(project, properties)
		{
		}

		/// <summary>
		/// Returns the prefix for the counters that are initialized by the <see cref="InitializeMachineSpecificationsCountersTask"/> and updated
		/// by the <see cref="MachineSpecificationsTask"/>.
		/// </summary>
		/// <returns>
		/// 	The prefix string.
		/// </returns>
		/// <example>
		/// <code>
		/// <![CDATA[
		/// <echo message="The prefix for the Machine.Specifications counters is: ${mspec::get-counter-prefix()}" />
		/// ]]></code>
		/// </example>
		[Function("get-counter-prefix")]
		public static string GetCounterPrefix()
		{
			return Counter.Prefix;
		}
	}
}