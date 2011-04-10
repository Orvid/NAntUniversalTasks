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
using NUnit.Framework;
#endregion

namespace NAntCollections.Tests.Utility
{
	/// <summary>
	/// Utility class that adds some common assertions
	/// missing from NUnit's Assert class.
	/// </summary>
	public static class AssertExt
    {
        #region Public Static Methods
        /// <summary>
        /// Assert that a string contains a particular value.
        /// </summary>
        /// <param name="haystack">The string to search in.</param>
        /// <param name="needle">The string to search for.</param>
		public static void StringContains(string haystack, string needle)
        {
            #region Preconditions
            if (String.IsNullOrEmpty(haystack)) throw new ArgumentException("haystack cannot be null or empty");
            if (String.IsNullOrEmpty(needle))   throw new ArgumentException("needle cannot be null or empty");
            #endregion

            if (haystack.IndexOf(needle) == -1)
				Assert.Fail("String '{0}' not found", needle);
		}

        /// <summary>
        /// Assert that a string does not contain a particular value.
        /// </summary>
        /// <param name="haystack">The string to search in.</param>
        /// <param name="needle">The string to search for.</param>
		public static void StringDoesNotContain(string haystack, string needle)
		{
            #region Preconditions
            if (String.IsNullOrEmpty(haystack)) throw new ArgumentException("haystack cannot be null or empty");
            if (String.IsNullOrEmpty(needle)) throw new ArgumentException("needle cannot be null or empty");
            #endregion

			if (haystack.IndexOf(needle) != -1)
				Assert.Fail("String '{0}' found", needle);
        }
        #endregion
    }
}
