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
// Jeff Wight (jeffyw@gmail.com)
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
	/// Functions for manipulating lists.
	/// </summary>
	[FunctionSet("list", "List")]
	public class ListFunctions : FunctionSetBase
	{
		#region Constructor
        /// <remarks>
        /// Not sure why FunctionSetBase should have a constructor if all
        /// of the function methods are static.  When will one of these
        /// ever get instantiated?
        /// </remarks>
        public ListFunctions(Project project, PropertyDictionary properties)
            : base(project, properties)
        {
        }
		#endregion

		#region Functions
		/// <summary>
		/// Get the value in a particular position in a list.
		/// </summary>
		/// <param name="listName">The name of the list.</param>
		/// <param name="position">The zero-indexed position in the list.</param>
		/// <returns>Value at the given position in the list.</returns>
        /// <exception cref="BuildException">
        /// If position is less than zero or exceeds the
        /// amount of items in the list.
        /// </exception>
        /// <example>
        ///   <para>
        ///   Get the first value of MyList.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <list-create list="MyList">
        ///   <list-item value="MyFirstValue"/>
        ///   <list-item value="MySecondValue"/>
        /// </list-create>
        /// <echo message="First value is ${list::get-value('MyList',0)}"/>
        ///     ]]>
        ///   </code>
        ///   <b>Result:</b> First value is MyFirstValue
        /// </example>
		[Function("get-value")]
		public static string GetValue(string listName, int position)
        {
            #region Preconditions
            if (String.IsNullOrEmpty(listName)) throw new ArgumentException("listName cannot be null or empty");
            #endregion

            IList<string> list = ListManager.GetList(listName);
			if (position < 0 || position >= list.Count )
				throw new BuildException(string.Format("List [{0}] does not have a position [{1}]", list, position));
			return (string) list[position];
		}

        /// <summary>
        /// Get the value of the top item on the list stack (last added).
        /// </summary>
        /// <param name="listName">The name of the list.</param>
        /// <returns>Value of the top item on the list stack.</returns>
        /// <example>
        ///   <para>
        ///   Get the top item on the list stack, remove it and return its value.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <list-add list="MyList" value="MyFirstValue"/>
        /// <list-add list="MyList" value="MySecondValue"/>
        /// <echo message="Last item added was ${list::pop(('MyList')}"/>
        ///     ]]>
        ///   </code>
        ///   <b>Result:</b> Last item added was MySecondValue
        /// </example>
        [Function("pop")]
        public static string Pop(string listName)
        {
            #region Preconditions
            if (String.IsNullOrEmpty(listName)) throw new ArgumentException("listName cannot be null or empty");
            #endregion

            IList<string> list = ListManager.GetList(listName);
            if (ListFunctions.IsEmpty(listName))
                throw new BuildException(string.Format("List [{0}] is empty", listName));
            string itemVal = list[list.Count-1];
            list.RemoveAt(list.Count - 1);
            return itemVal;
        }

        /// <summary>
        /// Get the value of the bottom item on the list stack (first added).
        /// </summary>
        /// <param name="listName">The name of the list.</param>
        /// <returns>Value of the bottom item on the list stack.</returns>
        /// <example>
        ///   <para>
        ///   Get the bottom item on the list stack, remove it and return its value.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <list-add list="MyList" value="MyFirstValue"/>
        /// <list-add list="MyList" value="MySecondValue"/>
        /// <echo message="First item added was ${list::shift(('MyList')}"/>
        ///     ]]>
        ///   </code>
        ///   <b>Result:</b> First item added was MyFirstValue
        /// </example>
        [Function("shift")]
        public static string Shift(string listName)
        {
            #region Preconditions
            if (String.IsNullOrEmpty(listName)) throw new ArgumentException("listName cannot be null or empty");
            #endregion

            IList<string> list = ListManager.GetList(listName);
            if (ListFunctions.IsEmpty(listName))
                throw new BuildException(string.Format("List [{0}] is empty", listName));
            string itemVal = list[0];
            list.RemoveAt(0);
            return itemVal;
        }

        /// <summary>
        /// Get the count of all items in a list.
        /// </summary>
        /// <param name="listName">The name of the list.</param>
        /// <returns>Count of items in the list.</returns>
        /// <example>
        ///   <para>
        ///   Get the count of items in MyList.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <list-create list="MyList">
        ///   <list-item value="MyFirstValue"/>
        ///   <list-item value="MySecondValue"/>
        /// </list-create>
        /// <echo message="Count is ${list::count('MyList')}"/>
        ///     ]]>
        ///   </code>
        ///   <b>Result:</b> Count is 2
        /// </example>
        [Function("count")]
        public static int Count(string listName)
		{
            #region Preconditions
            if (String.IsNullOrEmpty(listName)) throw new ArgumentException("listName cannot be null or empty");
            #endregion

            return ListManager.GetList(listName).Count;
		}

        /// <summary>
        /// Determine if the list is empty
        /// </summary>
        /// <param name="listName">The name of the list.</param>
        /// <returns><see langword="true" /> if the list has items, <see langword="false" /> if the list does NOT exist or has no items</returns>
        /// <example>
        ///   <para>
        ///   Determine if a list is emptly.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <echo message="List is empty: ${list::is-empty(MyList)}"/>
        /// <list-add list="MyList" value="MyFirstValue"/>
        /// <echo message="List is empty: ${list::is-empty(MyList)}"/>
        /// <list-remove list="MyList" value="MyFirstValue"/>
        /// <echo message="List is empty: ${list::is-empty(MyList)}"/>
        ///     ]]>
        ///   </code>
        ///   <b>Result:</b>
        ///     List is empty: True
        ///     List is empty: False
        ///     List is empty: True
        /// </example>
        [Function("is-empty")]
        public static bool IsEmpty(string listName)
        {
            #region Preconditions
            if (String.IsNullOrEmpty(listName)) throw new ArgumentException("listName cannot be null or empty");
            #endregion

            if (!ListManager.ListExists(listName)) return true;
            return (ListManager.GetList(listName).Count == 0);
        }

        /// <summary>
        /// Determine if a list contains a particular value.
        /// </summary>
        /// <param name="listName">The name of the list.</param>
        /// <param name="val">The value to search for.</param>
        /// <returns>
        /// <see langword="true" /> if list contains <paramref name="val" />; 
        /// otherwise <see langword="false" />.</returns>
        /// <example>
        ///   <para>
        ///   Determine if MyList contains FirstValue and ThirdValue.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <list-create list="MyList">
        ///   <list-item value="MyFirstValue"/>
        ///   <list-item value="MySecondValue"/>
        /// </list-create>
        /// <echo message="MyFirstValue found? ${list::contains('MyList','MyFirstValue')}"/>
        /// <echo message="MyThirdValue found? ${list::contains('MyList','MyThirdValue')}"/>
        ///     ]]>
        ///   </code>
        ///   <b>Result:</b><br/>
        ///   MyFirstValue found? True<br/>
        ///   MyThirdValue found? False
        /// </example>
        [Function("contains")]
        public static bool Contains(string listName, string val)
		{
            #region Preconditions
            if (String.IsNullOrEmpty(listName)) throw new ArgumentException("listName cannot be null or empty");
            if (String.IsNullOrEmpty(val))      throw new ArgumentException("val cannot be null or empty");
            #endregion

            return ListManager.GetList(listName).Contains(val);
		}

        /// <summary>
        /// Determine whether a list of a given name exists.
        /// </summary>
        /// <param name="listName">The name of the list.</param>
        /// <returns>
        /// <see langword="true" /> if the list exists (i.e. has been used by
        /// other list functions or tasks); otherwise <see langword="false" />.
        /// </returns>
        /// <example>
        ///   <para>
        ///   Determine if MyList exists.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <echo message="MyList exists? ${list::exists('MyList')}"/>
        /// <list-create list="MyList">
        ///   <list-item value="MyFirstValue"/>
        ///   <list-item value="MySecondValue"/>
        /// </list-create>
        /// <echo message="MyList exists now? ${list::exists('MyList')}"/>
        ///     ]]>
        ///   </code>
        ///   <b>Result:</b><br/>
        ///   MyList exists? False<br/>
        ///   MyList exists now? True
        /// </example>
		[Function("exists")]
        public static bool Exists(string listName)
		{
            #region Preconditions
            if (String.IsNullOrEmpty(listName)) throw new ArgumentException("listName cannot be null or empty");
            #endregion

            return ListManager.ListExists(listName);
		}
		#endregion
	}
}
