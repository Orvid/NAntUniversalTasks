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
using NAntCollections.Utility;
#endregion

namespace NAntCollections.List
{
	/// <summary>
	/// Remove an item from a list.
	/// </summary>
    /// <example>
    ///   <para>
    ///   Remove item with a specific value.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <list-remove list="MyList" value="TargetValue"/>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Remove first item.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <list-remove list="MyList" position="0"/>
    ///     ]]>
    ///   </code>
    /// </example>
	[TaskName("list-remove")]
	public class ListRemove : ListBase
	{
		#region Private Fields
		private string _value;
		private int    _position = -1;
		#endregion

        #region Attributes
        /// <summary>
        /// The value of the item to remove.
        /// Cannot specify both <see cref="ListRemove.Value" /> and
        /// <see cref="ListRemove.Position" /> at the same time.
        /// </summary>
        /// <remarks>
        /// If multiple items in the list have
        /// the specified value, only one will
        /// be removed.
        /// </remarks>
        [TaskAttribute("value")]
        [StringValidator(AllowEmpty = false)]
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// The position of the item to remove.
        /// Cannot specify both <see cref="ListRemove.Value" /> and
        /// <see cref="ListRemove.Position" /> at the same time.
        /// </summary>
        [TaskAttribute("position")]
        [Int32Validator(0, Int32.MaxValue)]
        public int Position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion

		#region Execution
        /// <summary>
        /// Execute the task.
        /// </summary>
		protected override void ExecuteTask()
		{
			base.ExecuteTask();

			// Cannot specify both Value and Position at the same time
			if (!String.IsNullOrEmpty(Value) && _position != -1)
				throw new BuildException("Cannot specify both position and value");

            if (!String.IsNullOrEmpty(Value))
            {
                // Remove by value
                List.Remove(Value);
            }
            else
            {
                // Remove by position
                if (Position >= List.Count)
                    throw new BuildException(string.Format("Position [{0}] does not exist in list [{1}]", Position, ListName));
				List.RemoveAt(Position);
            }
		}
		#endregion
	}
}
