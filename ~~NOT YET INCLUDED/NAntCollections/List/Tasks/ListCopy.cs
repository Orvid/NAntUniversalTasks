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
    /// Copy an existing list into a new list.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Copy MyExistingList into MyNewList.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <list-copy list="MyExistingList" into="MyNewList"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("list-copy")]
    public class ListCopy : ListBase
    {
        #region Private Fields
        string _newListName;
        #endregion

        #region Task Attributes
        /// <summary>
        /// Name of the new list.
        /// </summary>
        [TaskAttribute("into", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string NewListName
        {
            get { return _newListName; }
            set { _newListName = value; }
        }
        #endregion

        #region Execution
        /// <summary>
        /// Execute the task.
        /// </summary>
        protected override void ExecuteTask()
        {
            base.ExecuteTask();

            if (ListManager.ListExists(NewListName))
				throw new BuildException(string.Format("List with name [{0}] already exists.", NewListName));

            IList<string> newList = ListManager.GetList(NewListName);
            foreach (string val in List)
                newList.Add(val);
        }
        #endregion
    }
}
