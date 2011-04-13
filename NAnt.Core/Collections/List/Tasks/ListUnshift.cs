#region Copyright and Licensing
//
// NAntCollections
// Copyright © 2007 Justin Kohlhepp, Jeff Wight
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
// Jeff Wight (jeffyw@gmail.com
//
#endregion

#region Using Statements
using System;
using System.Collections;
using NAnt.Core;
using NAnt.Core.Attributes;
#endregion

namespace NAntCollections.List
{
	/// <summary>
	/// Add an item to the bottom of a list stack.
	/// </summary>
    /// <example>
    ///   <para>
    ///   UnShift a new item onto the bottom of a list stack.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <list-unshift list="MyList" value="NewValue"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("list-unshift")]
	public class ListUnshift : ListAdd
    {
        #region Private Fields
        #endregion

        #region Task Attributes
		#endregion

        #region Execution
        /// <summary>
        /// Execute the task.
        /// </summary>
        protected override void ExecuteTask()
        {
            if(Position >= 0)
                throw new BuildException(string.Format("Position cannot be specified on list-unshift"));
            Position = 0;
            base.ExecuteTask();
        }
        #endregion
	}
}
