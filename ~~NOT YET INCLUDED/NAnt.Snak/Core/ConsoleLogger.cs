using System;
using System.Collections.Generic;
using System.Text;

namespace Snak.Core
{
    public class ConsoleLogger
    {
        private ConsoleLogger() { }

        public static void Log(LogLevel level, string message, params object[] args)
        {
            message = level.ToString() + " " + message;
            Console.WriteLine(message, args);
        }
    }
}
