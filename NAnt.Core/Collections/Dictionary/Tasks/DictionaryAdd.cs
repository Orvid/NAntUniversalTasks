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
	/// Add an item to a dictionary.  
	/// </summary>
    /// <remarks>
    /// <para>
    /// If <see cref="DictionaryAdd.Key" /> is already present in dictionary and
    /// overwrite is <see langword="false" />, task will fail.
    /// </para>
    /// <para>
    /// If a dictionary of the provided name does not exist,
    /// it will be created.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Add two items to MyDict.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <dict-add dictionary="MyDict" key="MyFirstKey"  value="MyFirstValue"/>
    /// <dict-add dictionary="MyDict" key="MySecondKey" value="MySecondValue"/>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Add an item to MyDict, and then overwrite its value.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <dict-add dictionary="MyDict" key="MyFirstKey" value="MyFirstValue"/>
    /// <dict-add dictionary="MyDict" key="MyFirstKey" value="MyReplacedFirstValue" overwrite="true"/>
    ///     ]]>
    ///   </code>
    /// </example>
	[TaskName("dict-add")]
	public class DictionaryAdd : DictionaryBase
	{
		#region Private Fields
		private bool   _overwrite = false;
		private string _key, _value;
		#endregion

		#region Task Attributes
        /// <summary>
        /// The key of the new item.
        /// </summary>
		[TaskAttribute("key", Required=true)]
		[StringValidator (AllowEmpty = false)]
		public string Key
		{
			get { return _key; }
			set { _key = value; }
		}

        /// <summary>
        /// The value of the new item.
        /// </summary>
		[TaskAttribute("value", Required=true)]
		[StringValidator (AllowEmpty = false)]
		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

        /// <summary>
        /// If the specified <see cref="DictionaryAdd.Key" /> is already
        /// present in the dictionary, this will cause the current value
        /// to be overwritten with the specified <see cref="DictionaryAdd.Value" />.
        /// The default is <see langword="false" />.
        /// </summary>
		[TaskAttribute("overwrite")]
		[BooleanValidator()]
		public bool Overwrite
		{
			get { return _overwrite; }
			set { _overwrite = value; }
		}
		#endregion

		#region Execution
        /// <summary>
        /// Execute the task.
        /// </summary>
		protected override void ExecuteTask()
		{
			base.ExecuteTask();

            // Overwriting an existing value is only
            // allowed if overwrite is true.
			if (Dictionary.ContainsKey(Key))
			{
				if (Overwrite)
					Dictionary.Remove(Key);
				else
					throw new BuildException(string.Format("Key [{0}] already present in dictionary [{1}].  Set overwrite to true to allow overwrites.", Key, DictionaryName));
			}

			Dictionary.Add(Key, Value);
		}
		#endregion
	}
}
