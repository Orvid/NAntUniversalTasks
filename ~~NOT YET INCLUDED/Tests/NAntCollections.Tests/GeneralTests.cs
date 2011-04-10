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
using NUnit.Framework;
using NAnt.Core;
using NAntCollections.Tests.Utility;
#endregion

namespace NAntCollections.Tests
{
	/// <summary>
	/// Tests not related to any specific functionality.
	/// </summary>
	[TestFixture]
	public class GeneralTests
    {
        #region NAnt Execution
        /// <summary>
		/// Make sure that our NAnt execution is working for correct build scripts.
		/// </summary>
        [Test]
        public void NAntExecution()
        {
            string _xml = @"
				<project>
					<property name='val' value='World'/>
					<echo message='Hello ${val}'/>
				</project>
				";
            string result = Utility.NAntTestRunner.RunNAntTest(_xml);
            AssertExt.StringContains(result, "Hello World");
        }

        /// <summary>
        /// Make sure that a failed build returns the appropriate exception.
        /// </summary>
        [Test]
        [ExpectedException(typeof(BuildException))]
        public void NAntFailedExecution()
        {
            string _xml = @"
				<project>
					<someFakeTask die='yes' />
				</project>
				";
            string result = Utility.NAntTestRunner.RunNAntTest(_xml);
        }
        #endregion
	}
}
