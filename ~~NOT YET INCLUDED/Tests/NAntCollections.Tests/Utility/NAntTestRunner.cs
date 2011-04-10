#region Copyright and Licensing
//
// NAntCollections
// Copyright © 2007 Justin Kohlhepp
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2.1 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301 USA
//
// Justin Kohlhepp (justin@vsxp.com)
//
#endregion

#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NAnt.Core;
using NAntCollections.Utility;
#endregion

namespace NAntCollections.Tests.Utility
{
	/// <summary>
	/// Allows for the running of NAnt builds for testing purposes.
	/// </summary>
	public static class NAntTestRunner
    {
        #region Public Static Methods
        /// <summary>
        /// Executes a script in NAnt and returns the output.
        /// </summary>
        /// <param name="xml">The script to execute.</param>
        /// <returns>The output captured during the execution of NAnt.</returns>
        public static string RunNAntTest(string xml)
        {
            return RunNAntTest(xml, Level.Info);
        }

        /// <summary>
        /// Executes a script in NAnt and returns the output.
        /// </summary>
        /// <param name="xml">The script to execute.</param>
        /// <param name="level">
        /// The <see cref="NAnt.Core.Level"/> of messages 
        /// that should be included in output.
        /// </param>
        /// <returns>The output captured during the execution of NAnt.</returns>
        public static string RunNAntTest(string xml, Level level)
        {
            #region Preconditions
            if (string.IsNullOrEmpty(xml)) throw new ArgumentException("xml cannot be null or empty");
            #endregion

            // Get temp file path for our build script
            string xmlPath = Path.GetTempFileName();
			
            // Redirect Console buffers
            TextWriter oldConsoleOut = Console.Out;
            TextWriter oldConsoleError = Console.Error;
            StringWriter capturedConsoleOut = new StringWriter();
            Console.SetOut(capturedConsoleOut);
            Console.SetError(capturedConsoleOut);

            try
            {
                // Write build script out to temp file
                StreamWriter sw = new StreamWriter(xmlPath, false);
                sw.Write(xml);
                sw.Close();

                // Create NAnt project and execute
                Project p = new Project(xmlPath, level, 0);
                p.Execute();
            }
            finally
            {
                // Put our Console buffers back to normal
                Console.SetOut(oldConsoleOut);
                Console.SetError(oldConsoleError);
                capturedConsoleOut.Flush();
                capturedConsoleOut.Close();

                // Output the captured output
                Console.Out.Write(capturedConsoleOut.ToString());

                // NAnt is done - clean up our temp file
                if (File.Exists(xmlPath))
                    File.Delete(xmlPath);
            }

            return capturedConsoleOut.ToString();
        }
        #endregion
    }
}
