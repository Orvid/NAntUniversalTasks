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

namespace NAntCollections.List
{
	/// <summary>
	/// Add an item to a list.
	/// </summary>
    /// <example>
    ///   <para>
    ///   Add two items to MyList.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <list-add list="MyList" value="MyFirstValue"/>
    /// <list-add list="MyList" value="MySecondValue"/>
    ///     ]]>
    ///   </code>
    /// </example>
    /// <example>
    ///   <para>
    ///   Add a new item to MyList in the first position.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <list-add list="MyList" value="MyNewFirstValue" position="0"/>
    ///     ]]>
    ///   </code>
    /// </example>
	[TaskName("list-add")]
	public class ListAdd : ListBase
    {
        #region Private Fields
        private string _value;
        private int    _position = -1;
        #endregion

        #region Task Attributes
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
        /// Zero-based index of where the value should be inserted.
        /// All items in that position or after will be have their
        /// position shifted by one.  Specifying a position higher
        /// than the max current position of the list will just add
        /// at the end of the list.
        /// </summary>
        [TaskAttribute("position")]
        [Int32Validator(MinValue = 0)]
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

            if (Position == -1 || Position >= List.Count)
                List.Add(Value);
            else
                List.Insert(Position, Value);
        }
        #endregion
	}
}
