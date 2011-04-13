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
    /// Represents an individual item in a list.
    /// </summary>
    [ElementName("list-item")]
    public class ListItemElement : IfUnlessElement
    {
        #region Private Members
        private string _value;
        #endregion

        #region Element Attributes
        /// <summary>
        /// The value of the list item.
        /// </summary>
        [TaskAttribute("value", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
        #endregion
    }
}
