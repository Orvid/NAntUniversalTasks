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
#endregion

namespace NAntCollections.List
{
	/// <summary>
	/// Utility class for working with lists.
	/// </summary>
	internal abstract class ListManager
    {
        #region Private Static Fields
        private static readonly IDictionary<string, List<string>> _lists = new Dictionary<string, List<string>>();
        #endregion

        #region Public Static Methods
        /// <summary>
        /// Get a list based on a name.
        /// </summary>
        /// <param name="listName">The name of the list.</param>
        /// <remarks>
        /// If a list of the given name does not exist yet, it will
        /// be created and returned.
        /// </remarks>
        /// <returns>
        /// The list with the given <paramref name="name"/>.
        /// </returns>
        public static IList<string> GetList(string listName)
		{
            #region Preconditions
            if (String.IsNullOrEmpty(listName)) throw new ArgumentException("listName cannot be null or empty");
            #endregion

			if (!ListExists(listName))
				_lists[listName] = new List<string>();

			return _lists[listName];
		}

        /// <summary>
        /// Determine whether a list of a given name exists.
        /// </summary>
        /// <param name="listName">The name of the list.</param>
        /// <returns>
        /// <see langword="true" /> if the list exists;
        /// <see langword="false" /> otherwise.
        /// </returns>
		public static bool ListExists(string listName)
		{
            #region Preconditions
            if (String.IsNullOrEmpty(listName)) throw new ArgumentException("listName cannot be null or empty");
            #endregion

			return _lists.ContainsKey(listName);
        }

        /// <summary>
        /// Remove all lists from memory.
        /// </summary>
        /// <remarks>
        /// Primarily used by unit tests.
        /// </remarks>
        internal static void Reset()
        {
            _lists.Clear();
        }
        #endregion
    }
}
