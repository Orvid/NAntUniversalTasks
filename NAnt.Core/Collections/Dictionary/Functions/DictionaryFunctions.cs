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
using NAnt.Core;
using NAnt.Core.Attributes;
using NAntCollections.Utility;
#endregion

namespace NAntCollections.Dictionary
{
	/// <summary>
	/// Functions for manipulating dictionaries.
	/// </summary>
	[FunctionSet("dict", "Dictionary")]
	public class DictionaryFunctions : FunctionSetBase
	{
		#region Constructor
        /// <remarks>
        /// Not sure why FunctionSetBase should have a constructor if all
        /// of the function methods are static.  When will one of these
        /// ever get instantiated?
        /// </remarks>
        public DictionaryFunctions(Project project, PropertyDictionary properties)
            : base(project, properties)
        {
        }
		#endregion

		#region Functions
        /// <summary>
        /// Get the value of an item in a dictionary for a particular key.
        /// </summary>
        /// <param name="dictName">The name of the dictionary.</param>
        /// <param name="key">The key in the dictionary.</param>
        /// <returns>The value in the dictionary for <paramref name="key" />.</returns>
        /// <example>
        ///   <para>
        ///   Get the value of key 'MyFirstKey'.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <dict-create dictionary="MyDict">
        ///   <dict-item key="MyFirstKey" value="MyFirstValue"/>
        /// </dict-create>
        /// <echo message="My value is ${dict::get-value('MyDict','MyFirstKey')}"/>
        ///     ]]>
        ///   </code>
        ///   <b>Result:</b> My value is MyFirstValue
        /// </example>
		[Function("get-value")]
		public static string GetValue(string dictName, string key)
        {
            #region Preconditions
            if (String.IsNullOrEmpty(dictName)) throw new ArgumentException("dictName cannot be null or empty");
            if (String.IsNullOrEmpty(key))      throw new ArgumentException("key cannot be null or empty");
            #endregion

            return DictionaryManager.GetDictionary(dictName)[key];
		}

        /// <summary>
        /// Get the count of all items in a dictionary.
        /// </summary>
        /// <param name="dictName">The name of the dictionary.</param>
        /// <returns>The number of items in the dictionary.</returns>
        /// <example>
        ///   <para>
        ///   Get the count of items in key 'MyDict'.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <dict-create dictionary="MyDict">
        ///   <dict-item key="MyFirstKey" value="MyFirstValue"/>
        ///   <dict-item key="MySecondKey" value="MySecondValue"/>
        /// </dict-create>
        /// <echo message="Count is ${dict::count('MyDict')}"/>
        ///     ]]>
        ///   </code>
        ///   <b>Result:</b> Count is 2
        /// </example>
		[Function("count")]
		public static int Count(string dictName)
		{
            #region Preconditions
            if (String.IsNullOrEmpty(dictName)) throw new ArgumentException("dictName cannot be null or empty");
            #endregion

            return DictionaryManager.GetDictionary(dictName).Count;
		}

        /// <summary>
        /// Determine if the dictionary is empty
        /// </summary>
        /// <param name="dictName">The name of the dictionary.</param>
        /// <returns><see langword="true" /> if the dictionary has items, <see langword="false" /> if the dictionary does NOT exist or has no items</returns>
        /// <example>
        ///   <para>
        ///   Determine if a dictionary is empty.
        ///   </para>
        ///   <code>
        ///     <![CDATA[
        /// <echo message="Dictionary is empty: ${dict::is-empty('MyDict')}"/>
        /// <dict-add dictionary="MyDict" key="MyFirstKey" value="MyFirstValue"/>
        /// <echo message="Dictionary is empty: ${dict::is-empty('MyDict')}"/>
        /// <dict-remove dictionary="MyDict" key="MyFirstKey"/>
        /// <echo message="Dictionary is empty: ${dict::is-empty('MyDict')}"/>
        ///     ]]>
        ///   </code>
        ///   <b>Result:</b><br/>
        ///     Dictionary is empty: True<br/>
        ///     Dictionary is empty: False<br/>
        ///     Dictionary is empty: True<br/>
        /// </example>
        [Function("is-empty")]
        public static bool IsEmpty(string dictName)
        {
            #region Preconditions
            if (String.IsNullOrEmpty(dictName)) throw new ArgumentException("dictName cannot be null or empty");
            #endregion

            if (!DictionaryManager.DictionaryExists(dictName)) return true;
            return (DictionaryManager.GetDictionary(dictName).Count == 0);
        }

        /// <summary>
        /// Determine whether a dictionary contains a particular key.
        /// </summary>
        /// <param name="dictName">The name of the dictionary.</param>
        /// <param name="key">The key in the dictionary.</param>
        /// <returns>
        /// <see langword="true" /> if the dictionary contains an item with 
        /// that <paramref name="key" />; otherwise <see langword="false" />.
        /// </returns>
        /// <example>
        ///   <para>Determine if MyDict contains an item with key MyFirstKey.</para>
        ///   <code>
        ///     <![CDATA[
        /// <dict-create dictionary="MyDict">
        ///   <dict-item key="MyFirstKey" value="MyFirstValue"/>
        ///   <dict-item key="MySecondKey" value="MySecondValue"/>
        /// </dict-create>
        /// <echo message="MyFirstKey found: ${dict::contains-key('MyDict','MyFirstKey)}"/>
        /// <echo message="MyThirdKey found: ${dict::contains-key('MyDict','MyThirdKey)}"/>
        ///     ]]>
        ///   </code>
        ///   <b>Result:</b><br/>
        ///   MyFirstKey found: True<br/>
        ///   MySecondKey found: False
        /// </example>
		[Function("contains-key")]
		public static bool ContainsKey(string dictName, string key)
		{
            #region Preconditions
            if (String.IsNullOrEmpty(dictName)) throw new ArgumentException("dictName cannot be null or empty");
            if (String.IsNullOrEmpty(key)) throw new ArgumentException("key cannot be null or empty");
            #endregion

            return DictionaryManager.GetDictionary(dictName).ContainsKey(key);
		}

        /// <summary>
        /// Determine whether a dictionary of a given name exists.
        /// </summary>
        /// <param name="dictName">The name of the dictionary.</param>
        /// <returns>
        /// <see langword="true" /> if the dictionary exists (i.e. has been used by
        /// other dictionary functions or tasks); otherwise <see langword="false" />.
        /// </returns>
        /// <example>
        ///   <para>Determine if MyDict exists.</para>
        ///   <code>
        ///     <![CDATA[
        /// <echo message="MyDict exists? ${dict::exists('MyDict'}"/>
        /// <dict-add dictionary="MyDict" key="MyFirstKey" value="MyFirstValue"/>
        /// <echo message="MyDict exists now? ${dict::exists('MyDict'}"/>
        ///     ]]>
        ///   </code>
        ///   <b>Result:</b><br/>
        ///   MyDict exists? False<br/>
        ///   MyDict exists now? True
        /// </example>
		[Function("exists")]
		public static bool Exists(string dictName)
		{
            #region Preconditions
            if (String.IsNullOrEmpty(dictName)) throw new ArgumentException("dictName cannot be null or empty");
            #endregion

            return DictionaryManager.DictionaryExists(dictName);
		}
		#endregion
	}
}