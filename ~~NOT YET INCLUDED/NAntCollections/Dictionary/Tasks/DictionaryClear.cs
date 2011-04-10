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
using NAnt.Core.Attributes;
#endregion

namespace NAntCollections.Dictionary
{
	/// <summary>
	/// Remove all items from a dictionary.
	/// </summary>
    /// <example>
    ///   <para>
    ///   Clear all items from MyDict.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <dict-clear dictionary="MyDict"/>
    ///     ]]>
    ///   </code>
    /// </example>
	[TaskName("dict-clear")]
	public class DictionaryClear : DictionaryBase
    {
        #region Execution
        /// <summary>
        /// Execute the task.
        /// </summary>
        protected override void ExecuteTask()
		{
			base.ExecuteTask();
	
			Dictionary.Clear();
        }
        #endregion
    }
}
