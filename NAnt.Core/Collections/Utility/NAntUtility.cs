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
using NAnt.Core;
#endregion

namespace NAntCollections.Utility
{
	/// <summary>
	/// Utility methods/properties to make it easier to deal with NAnt.
	/// </summary>
	internal static class NAntUtility
    {
        #region Public Static Properties
        /// <summary>
        /// Returns the total width of the NAnt output console.
        /// </summary>
        /// <remarks>
        /// This is hardcoded at 75 right now.  For some reason Console.BufferWidth
        /// is throwing a System.IO error when NAnt is executed inside UTs.
        /// </remarks>
        public static int ConsoleWidth
        {
            get
            {
                return 75;
            }
        }
        #endregion

        #region Public Static Methods
        /// <summary>
        /// Sets a property to a value for a given NAnt project.  If the property
        /// already exists on the project, the old value will be overwritten with
        /// the new value.
        /// </summary>
        /// <param name="proj">The project on which to set the property.</param>
        /// <param name="prop">The name of the property to set.</param>
        /// <param name="val">The value to set the property to.</param>
        public static void AddOrOverwriteProperty(Project proj, string prop, string val)
        {
            #region Preconditions
            if (proj == null) throw new ArgumentNullException("proj");
            if (prop == null) throw new ArgumentNullException("prop");
            if (val == null)  throw new ArgumentNullException("val");
            #endregion

            if (proj.Properties.Contains(prop))
				proj.Properties.Remove(prop);
			proj.Properties.Add(prop, val);
        }
        #endregion
    }
}
