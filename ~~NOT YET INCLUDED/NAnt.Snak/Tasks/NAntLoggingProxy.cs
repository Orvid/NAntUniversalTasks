using System;
using System.Collections.Generic;
using System.Text;

namespace Snak.Tasks
{
    public delegate void NAntLoggingHandler(NAnt.Core.Level level, string message, params object[] args);

    /// <summary>
    /// Provides a 'bridge' class between the nant logging delegate and the snak logging delegate
    /// This is intended to preserve the decoupling between the core classes and NAnt
    /// </summary>
    internal class NAntLoggingProxy
    {
        readonly NAntLoggingHandler _log;

        public NAntLoggingProxy(NAntLoggingHandler log)
        {
            _log = log;
        }

        public void Log(Snak.Core.LogLevel level, string message, params object[] args)
        {
            NAnt.Core.Level nantLevel = (NAnt.Core.Level)level;
            _log(nantLevel, message, args);
        }
    }
}
