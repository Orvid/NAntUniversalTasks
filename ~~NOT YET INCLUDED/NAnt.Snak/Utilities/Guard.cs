using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Snak.Utilities
{
    public static class Guard
    {
        [DebuggerStepThrough]
        public static void ArgumentIsNotNull(object argument, string name)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        [DebuggerStepThrough]
        public static void ArgumentGuidIsNotEmpty(Guid argument, string name)
        {
            if (argument == Guid.Empty)
            {
                throw new ArgumentException("The argument '" + name + "' cannot be == Guid.Empty");
            }
        }

        [DebuggerStepThrough]
        public static void ArgumentDoesNotEqual0(int argument, string name)
        {
            if (argument == 0)
            {
                throw new ArgumentException("The argument '" + name +"' cannot be == 0");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="test">if true the Argument is incorrect and an ArgumentException is thrown</param>
        /// <param name="message"></param>
        [DebuggerStepThrough]
        public static void ArgumentIsIncorrect(bool test, string message)
        {
            if (test)
            {
                throw new ArgumentException(message);
            }
        }

        [DebuggerStepThrough]
        public static void ArgumentStringIsNotNullOrEmpty(string argument, string name)
        {
            if (String.IsNullOrEmpty(argument))
            {
                throw new ArgumentException("The given string argument cannot be null or empty.", name);
            }
        }

        [DebuggerStepThrough]
        public static void StringIsNotNullOrEmpty(string name, string message)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException(message);
            }
        }

        [DebuggerStepThrough]
        public static void IsTrue(bool argument, string message)
        {
            if (argument == false)
            {
                throw new InvalidOperationException(message);
            }
        }

        [DebuggerStepThrough]
        public static void IsNotTrue(bool argument, string message)
        {
            if (argument == true)
            {
                throw new InvalidOperationException(message);
            }
        }

        [DebuggerStepThrough]
        public static void IsNotNull(object argument, string message)
        {
            if (argument == null)
            {
                throw new InvalidOperationException(message);
            }
        }

        [DebuggerStepThrough]
        public static void GuidIsNotEmpty(Guid argument, string message)
        {
            if (argument == Guid.Empty)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
