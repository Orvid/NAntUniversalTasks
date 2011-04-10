using System;
using System.Collections.Generic;
using System.Text;

namespace Snak.Core
{
    public delegate void LoggingHandler(LogLevel level, string message, params object[] args);
}
