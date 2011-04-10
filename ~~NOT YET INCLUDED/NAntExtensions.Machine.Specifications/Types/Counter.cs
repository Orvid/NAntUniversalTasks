using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NAntExtensions.Machine.Specifications.Types
{
	internal class Counter
	{
		internal const string Prefix = "mspec.";

		public const string Contexts = Prefix + "contexts";
		public const string FailedSpecifications = Prefix + "failedspecs";
		public const string IgnoredSpecifications = Prefix + "ignoredspecs";
		public const string PassedSpecifications = Prefix + "passedspecs";
		public const string Specifications = Prefix + "specs";
		public const string UnimplementedSpecifications = Prefix + "unimplementedspecs";

		public static IEnumerable<string> All
		{
			get
			{
				var values =
					from field in typeof(Counter).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField)
					where typeof(string).IsAssignableFrom(field.FieldType)
					select
						typeof(Counter).InvokeMember(field.Name,
						                             BindingFlags.GetField | BindingFlags.Static | BindingFlags.Public,
						                             null,
						                             null,
						                             null).ToString();

				return values;
			}
		}
	}
}