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

namespace NAntCollections.Dictionary
{
    /// <summary>
    /// Copy an existing dictionary into a new dictionary.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Copy MyExistingDict into MyNewDict.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <dict-copy list="MyExistingDict" into="MyNewDict"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("dict-copy")]
    public class DictionaryCopy : DictionaryBase
    {
        #region Private Fields
        private string _newDictionaryName;
        #endregion

        #region Task Attributes
        /// <summary>
        /// Name of the new dictionary.
        /// </summary>
        [TaskAttribute("into", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string NewDictionaryName
        {
            get { return _newDictionaryName; }
            set { _newDictionaryName = value; }
        }
        #endregion

        #region Execution
        /// <summary>
        /// Execute the task.
        /// </summary>
        protected override void ExecuteTask()
        {
            base.ExecuteTask();

            if (DictionaryManager.DictionaryExists(NewDictionaryName))
                throw new BuildException(string.Format("Dictionary with name [{0}] already exists.", NewDictionaryName));

            IDictionary<string, string> newDictionary = DictionaryManager.GetDictionary(NewDictionaryName);
            foreach (string key in Dictionary.Keys)
                newDictionary[key] = Dictionary[key];
        }
        #endregion
    }
}
