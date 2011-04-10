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
using System.Text;
using NAnt.Core;
#endregion

namespace NAntCollections.Tests.Utility
{
	/// <summary>
	/// Assists in the creation of NAnt scripts.  Basically
	/// to avoid repeating the surrounding project tags and
	/// load tasks for each script.
	/// </summary>
	public static class NAntScriptBuilder
	{
		#region Public Static Methods
        /// <summary>
        /// Shortcut method for building and running a test script
        /// in one line of code.
        /// </summary>
        /// <param name="xml">The script to execute.</param>
        /// <returns>The standard out captured during the execution of NAnt.</returns>
        public static string BuildAndRunScript(string xml)
        {
            return BuildAndRunScript(xml, Level.Info);
        }

		/// <summary>
		/// Shortcut method for building and running a test script
		/// in one line of code.
		/// </summary>
        /// <param name="xml">The script to execute.</param>
        /// <param name="verbose">Indicates whether or not to execute NAnt with verbose switch.</param>
        /// <returns>The standard out captured during the execution of NAnt.</returns>
		public static string BuildAndRunScript(string xml, Level level)
        {
            #region Preconditions
            if (String.IsNullOrEmpty(xml)) throw new ArgumentException("xml cannot be null or empty");
            #endregion

            return NAntTestRunner.RunNAntTest(BuildScript(xml), level);
		}

        /// <summary>
        /// Packages the <see cref="NAntScriptBuilder.Contents" /> of the script so
        /// that it will execute property in NAnt, and the NAntCollections tasks
        /// can be found.
        /// </summary>
        /// <returns>The constructed script.</returns>
		public static string BuildScript(string contents)
		{
            #region Preconditions
            if (String.IsNullOrEmpty(contents)) throw new ArgumentException("contents cannot be null or empty");
            #endregion

			StringBuilder sb = new StringBuilder();
			sb.Append      ("<project>");
			sb.AppendFormat("<loadtasks assembly='{0}'/>", TestUtility.NAntCollectionsAssemblyPath);
			sb.Append      (contents);
			sb.Append      ("</project>");
			return sb.ToString();
		}
		#endregion
	}
}
