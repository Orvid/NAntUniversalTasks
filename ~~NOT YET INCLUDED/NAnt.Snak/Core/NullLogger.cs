using System;
using System.Collections.Generic;
using System.Text;

namespace Snak.Core
{
    public class NullLogger
    {
        private NullLogger() { }

        public static void Log(LogLevel level, string message, params object[] args) { }
    }
}
