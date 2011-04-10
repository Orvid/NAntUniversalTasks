using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core.Attributes;
using NAnt.Core;

namespace Snak.Functions
{
    [FunctionSet("stringManipulation", "StringManipulation")]
    public class StringManipulationFunctions : FunctionSetBase
    {
        public StringManipulationFunctions(Project project, PropertyDictionary properties) : base(project, properties) { }

        [Function("upperCaseFirst")]
        public static string UpperCaseFirst(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length < 1) return value;
            if (value.Length == 1) return value.ToUpper();
            return value.Substring(0, 1).ToUpper() + value.Substring(1);
        }

        [Function("leftBit")]
        public static string LeftBit(string whole, string seperator)
        {
            int end = whole.IndexOf(seperator);
            return whole.Substring(0, end);
        }

        [Function("rightBit")]
        public static string RightBit(string whole, string seperator)
        {
            int end = whole.IndexOf(seperator);
            return whole.Substring(end + seperator.Length);
        }

        /// <summary>
        /// Converts a string (typically a build config) into one safe for
        ///	use within a file/path name.
        /// This is typically used in conjunction with the <see cref="PackageOutputsTask"/>,
		/// as this is how that task formats it's path names
        /// </summary>
        /// <param name="buildConfig"></param>
        /// <returns></returns>
        /// <example>This used to be done in nant as follows:
        /// <![CDATA[
        /// 	    <property name="safeConfigSuffix" value="${string::replace(string::replace(build.config,'|','-'),' ','')}"/>
        /// ]]>
        /// </example>
        [Function("makeSafeForPath")]
        public static string MakeSafeForPath(string buildConfig)
        {
            return BuildDrop.AbstractDropNamingStrategy.MakeSafeForPath(buildConfig);
        }
    }
}
