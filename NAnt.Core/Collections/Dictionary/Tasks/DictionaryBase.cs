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
using NAnt.Core;
using NAnt.Core.Attributes;
using NAntCollections.Utility;
#endregion

namespace NAntCollections.Dictionary
{
	/// <summary>
	/// Base class for all tasks which operate on a single dictionary.
	/// </summary>
	public abstract class DictionaryBase : NAnt.Core.Task
	{
		#region Private Fields
		private string _dictionaryName;
		private IDictionary<string ,string> _dictionary;
		#endregion

        #region Protected Properties
        /// <summary>
        /// The Dictionary related to this Task.
        /// </summary>
        protected IDictionary<string, string> Dictionary { get { return _dictionary; } }
        #endregion

        #region Task Attributes
        /// <summary>
        /// The name of the dictionary to operate on.
        /// </summary>
        [TaskAttribute("dictionary", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string DictionaryName
        {
            get { return _dictionaryName; }
            set { _dictionaryName = value; }
        }
        #endregion

		#region Execution
        /// <summary>
        /// Execute the task.
        /// </summary>
		protected override void ExecuteTask()
		{
			_dictionary = DictionaryManager.GetDictionary(DictionaryName);
		}
		#endregion
	}
}
