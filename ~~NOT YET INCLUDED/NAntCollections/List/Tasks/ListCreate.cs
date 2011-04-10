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
using System.Collections.Generic;
using System.Text;
using NAnt.Core;
using NAnt.Core.Attributes;
#endregion

namespace NAntCollections.List
{
    /// <summary>
    /// Create a list with an initial set of items.
    /// </summary>
    /// <example>
    ///   <para>
    ///   Create a list with three initial items.  But only
    ///   include the second item if a specific property is set.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <list-create dictionary="MyList">
    ///   <list-item value="MyFirstValue"/>
    ///   <list-item value="MySecondValue" if="${property::exists('my.property')}"/>
    ///   <list-item value="MyThirdValue"/>
    /// </list-create>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("list-create")]
    public class ListCreate : ListBase
    {
        #region Private Fields
        private ListItemElement[] _items;
        #endregion

        #region Element Collections
        /// <summary>
        /// The items that should be present in the
        /// list when it is created.
        /// </summary>
        [BuildElementArray("list-item", Required = true)]
        public ListItemElement[] Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
            }
        }
        #endregion

        #region Execution
        /// <summary>
        /// Execute the task.
        /// </summary>
        protected override void ExecuteTask()
        {
            base.ExecuteTask();

            foreach (ListItemElement item in Items)
            {
                if (item.If && !item.Unless)
                    List.Add(item.Value);
            }
        }
        #endregion
    }
}
