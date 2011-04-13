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
using System.Text;
#endregion

namespace NAntCollections.Utility
{
    // HAD TO COMMENT OUT THIS CLASS BECAUSE IT USES
    // GENERICS WHICH NDOC IS NOT PREPARED TO DEAL WITH
    // AND CRASHES.
    //internal static class CollectionManagers
    //{
    //    #region Public Static Members
    //    /// <summary>
    //    /// Used to manage dictionary collections.
    //    /// </summary>
    //    public readonly static CollectionManager<Dictionary<string, string>> Dictionaries = new CollectionManager<Dictionary<string, string>>();

    //    /// <summary>
    //    /// Used to manage list collections.
    //    /// </summary>
    //    public readonly static CollectionManager<List<string>> Lists = new CollectionManager<List<string>>();
    //    #endregion

    //    #region Inner Class CollectionManager<T>
    //    /// <summary>
    //    /// Provides generic implementation of a managed collection of collections.
    //    /// </summary>
    //    /// <typeparam name="T"></typeparam>
    //    internal class CollectionManager<T>
    //            where T : ICollection, new()
    //    {
    //        #region Constructor
    //        internal CollectionManager()
    //        {
    //        }
    //        #endregion

    //        #region Private Fields
    //        private Dictionary<string, T> _collections = new Dictionary<string, T>();
    //        #endregion

    //        #region Public Methods
    //        /// <summary>
    //        /// Retrieve a collection based on a name.
    //        /// </summary>
    //        /// <param name="collectionName">The name of the collection.</param>
    //        /// <returns>The collection.</returns>
    //        /// <remarks>
    //        /// If a collection of that name does not exist,
    //        /// this method will create it.
    //        /// </remarks>
    //        public T Get(string collectionName)
    //        {
    //            if (!_collections.ContainsKey(collectionName))
    //                _collections[collectionName] = new T();

    //            return _collections[collectionName];
    //        }

    //        /// <summary>
    //        /// Indicates whether a collection of a particular name exists.
    //        /// </summary>
    //        /// <param name="collectionName">The name of the collection.</param>
    //        /// <returns>
    //        /// <see langword="true" /> if a collection with that
    //        /// name exists; otherwise <see langword="false" />.
    //        /// </returns>
    //        public bool Exists(string collectionName)
    //        {
    //            return _collections.ContainsKey(collectionName);
    //        }

    //        /// <summary>
    //        /// Removes all collections from memory.
    //        /// </summary>
    //        /// <remarks>
    //        /// Primarily used by UTs to reset
    //        /// state between each test.
    //        /// </remarks>
    //        public void Reset()
    //        {
    //            _collections.Clear();
    //        }
    //        #endregion

    //        #region Public Indexers
    //        /// <summary>
    //        /// Retrieve a collection based on a name.
    //        /// </summary>
    //        /// <param name="collectionName">The name of the collection.</param>
    //        /// <returns>The collection.</returns>
    //        /// <remarks>
    //        /// If a collection of that name does not exist,
    //        /// this method will create it.
    //        /// </remarks>
    //        public T this[string collectionName]
    //        {
    //            get
    //            {
    //                return Get(collectionName);
    //            }
    //        }
    //        #endregion
    //    }
    //    #endregion
    //}
}
