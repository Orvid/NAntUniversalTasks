using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace Snak.Functions
{
    [FunctionSet("dateManipulation", "DateManipulation")]
    public class DateManipulationFunctions : FunctionSetBase
    {
        public DateManipulationFunctions(Project project, PropertyDictionary properties) : base(project, properties) { }

        [Function("getAnsiDate")]
        public static string GetAnsiDate()
        {
            return DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        [Function("getTimeInTwoMinutesTime")]
        public static string GetTimeInTwoMinutesTime()
        {
            return DateTime.Now.AddMinutes(2).ToString("hh:mm:ss");
        }
    }
}
