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
	/// Loop through each item in a list.
	/// </summary>
    /// <example>
    ///   <para>
    ///   Echo the value of each item in MyList.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <list-foreach list="MyList" value-property="_val">
    ///   <echo message="Value: ${_val}"/>
    /// </list-foreach>
    ///     ]]>
    ///   </code>
    /// </example>
	[TaskName("list-foreach")]
	public class ListForEach : TaskContainer
    {
        #region Private Fields
        private string _valueProperty, _listName;
        #endregion

        #region Task Attributes
        /// <summary>
        /// The name of the list to operate on.
        /// </summary>
        [TaskAttribute("list", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string ListName
        {
            get { return _listName; }
            set { _listName = value; }
        }

        /// <summary>
        /// The property to set to the value of the current item in the loop.
        /// </summary>
        [TaskAttribute("value-property")]
        [StringValidator(AllowEmpty = false)]
        public string ValueProperty
        {
            get { return _valueProperty; }
            set { _valueProperty = value; }
        }
        #endregion

        #region Execution
        /// <summary>
        /// Execute the task.
        /// </summary>
        protected override void ExecuteTask()
		{
			IList<string> list = ListManager.GetList(ListName);
			foreach (string val in list)
			{
				if (ValueProperty != null)
					NAntUtility.AddOrOverwriteProperty(Project, ValueProperty, val);

                // Base execute handles executing all child tasks.
				base.ExecuteTask();
			}
        }
        #endregion
	}
}
