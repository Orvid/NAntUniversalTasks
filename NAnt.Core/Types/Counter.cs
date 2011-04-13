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

namespace NAntExtensions.MbUnit.Types
{
    internal class Counter
    {
        internal const string Prefix = "mbunit.";

        public const string Asserts = Prefix + "asserts";
        public const string Failures = Prefix + "failures";
        public const string Ignored = Prefix + "ignored";
        public const string Run = Prefix + "run";
        public const string Skipped = Prefix + "skipped";
        public const string Successes = Prefix + "successes";

        public static IEnumerable<string> All
        {
            get
            {
                List<string> result = new List<string>();

                foreach (
                    FieldInfo field in typeof(Counter).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField))
                {
                    if (!typeof(string).IsAssignableFrom(field.FieldType))
                    {
                        continue;
                    }

                    result.Add(
                        typeof(Counter).InvokeMember(field.Name,
                                                     BindingFlags.GetField | BindingFlags.Static | BindingFlags.Public,
                                                     null,
                                                     null,
                                                     null).ToString());
                }

                return result;
            }
        }
    }
}