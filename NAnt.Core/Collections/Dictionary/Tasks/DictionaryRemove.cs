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
using NAnt.Core;
using NAnt.Core.Attributes;
#endregion

namespace NAntCollections.Dictionary
{
	/// <summary>
	/// Remove an item from a dictionary.  If the key is not
	/// present in the dictionary, the task does nothing.
	/// </summary>
    /// <example>
    ///   <para>
    ///   Remove item with key MyFirstKey from MyDict.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <dict-remove dictionary="MyDict" key="MyFirstKey"/>
    ///     ]]>
    ///   </code>
    /// </example>
	[TaskName("dict-remove")]
	public class DictionaryRemoveTask : DictionaryBase
	{
		#region Private Fields
		private string _key;
		#endregion

		#region Task Attributes
        /// <summary>
        /// The key of the item to remove.
        /// </summary>
		[TaskAttribute("key", Required=true)]
		[StringValidator (AllowEmpty = false)]
		public string Key
		{
			get { return _key; }
			set { _key = value; }
		}
		#endregion

		#region Execution
        /// <summary>
        /// Execute the task.
        /// </summary>
		protected override void ExecuteTask()
		{
			base.ExecuteTask();

			Dictionary.Remove(Key);
		}
		#endregion
	}
}
