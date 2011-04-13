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

namespace NAntCollections.Dictionary
{
	/// <summary>
	/// Utility class for working with dictionaries.
	/// </summary>
	internal abstract class DictionaryManager
    {
        #region Private Static Fields
        private static IDictionary<string, IDictionary<string,string>> _dictionaries = new Dictionary<string, IDictionary<string,string>>();
        #endregion

        #region Public Static Methods
        /// <summary>
        /// Get a dictionary based on a name.
        /// </summary>
        /// <param name="name">The name of the dictionary.</param>
        /// <remarks>
        /// If a dictionary of the given name does not exist yet, it will
        /// be created and returned.
        /// </remarks>
        /// <returns>
        /// The dictionary with the given <paramref name="name"/>.
        /// </returns>
        public static IDictionary<string, string> GetDictionary(string name)
		{
            #region Preconditions
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("name cannot be null or empty");
            #endregion

			if (!DictionaryExists(name))
				_dictionaries[name] = new Dictionary<string, string>();

			return _dictionaries[name];
		}

        /// <summary>
        /// Determine whether a dictionary of a given name exists.
        /// </summary>
        /// <param name="name">The name of the dictionary.</param>
        /// <returns>
        /// <see langword="true" /> if the dictionary exists;
        /// <see langword="false" /> otherwise.
        /// </returns>
		public static bool DictionaryExists(string name)
        {
            #region Preconditions
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("name cannot be null or empty");
            #endregion

            return _dictionaries.ContainsKey(name);
        }

        /// <summary>
        /// Remove all dictionaries from memory.
        /// </summary>
        /// <remarks>
        /// Primarily used by unit tests.
        /// </remarks>
        internal static void Reset()
        {
            _dictionaries.Clear();
        }
        #endregion
    }
}
