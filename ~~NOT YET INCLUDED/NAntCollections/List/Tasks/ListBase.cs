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

namespace NAntCollections.List
{
	/// <summary>
	/// Base class for all tasks which operate on a single list.
	/// </summary>
	public class ListBase : NAnt.Core.Task
    {
        #region Private Fields
        private IList<string> _list;
		private string _listName;
        #endregion

        #region Protected Properties
        /// <summary>
        /// List related to the current Task.
        /// </summary>
        protected IList<string> List
        {
            get
            {
                return _list;
            }
        }
        #endregion

        #region Task Attributes
        /// <summary>
        /// Name of the list to operate on.
        /// </summary>
        [TaskAttribute("list", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string ListName
        {
            get { return _listName; }
            set { _listName = value; }
        }
        #endregion

        #region Execution
        /// <summary>
        /// Execute the task.
        /// </summary>
        protected override void ExecuteTask()
		{
			_list = ListManager.GetList(ListName);
        }
        #endregion
    }
}
