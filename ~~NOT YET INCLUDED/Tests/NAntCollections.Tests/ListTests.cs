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
using NUnit.Framework;
using NAnt.Core;
using NAntCollections.List;
using NAntCollections.Utility;
using NAntCollections.Tests.Utility;
#endregion

namespace NAntCollections.Tests
{
	/// <summary>
	/// Unit tests related to list support.
	/// </summary>
	[TestFixture]
	public class ListTests
    {
        #region SetUp
        [SetUp]
        public void SetUp()
        {
            ListManager.Reset();
        }
        #endregion

        #region Add
        [Test]
		public void Add()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<echo message=""Count is ${list::count('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is 2");
		}

		[Test]
		public void AddDuplicateValues()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<echo message=""Count is ${list::count('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is 3");
		}

		[Test]
        [ExpectedException(typeof(BuildException))]
		public void AddEmptyValue()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value=''/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
		}

        [Test]
        public void AddAtPosition()
        {
            string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-add list='MyList' value='NewSecondVal' position='1'/>
				<echo message=""Second value is ${list::get-value('MyList', 1)}""/>
				<echo message=""Third value is ${list::get-value('MyList', 2)}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Second value is NewSecondVal");
            AssertExt.StringContains(result, "Third value is SecondVal");
        }

        [Test]
        [ExpectedException(typeof(BuildException))]
        public void AddAtPositionBadValue()
        {
            string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-add list='MyList' value='NewSecondVal' position='-1'/>
				<echo message=""Second value is ${list::get-value('MyList', 1)}""/>
				<echo message=""Third value is ${list::get-value('MyList', 2)}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
        }

        /// <summary>
        /// Inserting with a position higher than the max current position
        /// of the list will just insert at the end.
        /// </summary>
        [Test]
        public void AddAtPositionPastEndValue()
        {
            string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-add list='MyList' value='NewSecondVal' position='100'/>
				<echo message=""Second value is ${list::get-value('MyList', 1)}""/>
				<echo message=""Third value is ${list::get-value('MyList', 2)}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Second value is SecondVal");
            AssertExt.StringContains(result, "Third value is NewSecondVal");
        }
		#endregion

        #region Push
        [Test]
        public void Push()
        {
            string _xml = @"
				<list-push list='MyList' value='FirstVal'/>
				<list-push list='MyList' value='SecondVal'/>
				<echo message=""Count is ${list::count('MyList')}""/>
				<echo message=""First value is ${list::get-value('MyList', 0)}""/>
				<echo message=""Second value is ${list::get-value('MyList', 1)}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Count is 2");
            AssertExt.StringContains(result, "First value is FirstVal");
            AssertExt.StringContains(result, "Second value is SecondVal");
        }

        [Test]
        public void PushDuplicateValues()
        {
            string _xml = @"
				<list-push list='MyList' value='FirstVal'/>
				<list-push list='MyList' value='FirstVal'/>
				<list-push list='MyList' value='SecondVal'/>
				<echo message=""Count is ${list::count('MyList')}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Count is 3");
        }

        [Test]
        [ExpectedException(typeof(BuildException))]
        public void PushEmptyValue()
        {
            string _xml = @"
				<list-push list='MyList' value='FirstVal'/>
				<list-push list='MyList' value=''/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
        }

        [Test]
        [ExpectedException(typeof(BuildException))]
        public void PushAtPosition()
        {
            string _xml = @"
				<list-push list='MyList' value='FirstVal'/>
				<list-push list='MyList' value='SecondVal' position='1'/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
        }
        #endregion

        #region Pop
        [Test]
        public void Pop()
        {
            string _xml = @"
                <list-push list='MyList' value='FirstVal'/>
                <list-push list='MyList' value='SecondVal'/>
                <echo message=""Pop returns: ${list::pop('MyList')}""/>
                <echo message=""Count is ${list::count('MyList')}""/>
                ";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Pop returns: SecondVal");
            AssertExt.StringContains(result, "Count is 1");
        }

        [Test]
        [ExpectedException(typeof(BuildException))]
        public void PopEmptyList()
        {
            string _xml = @"
                <echo message=""Pop returns: ${list::pop('MyList')}""/>
                ";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
        }
        #endregion

        #region Shift
        [Test]
        public void Shift()
        {
            string _xml = @"
                <list-push list='MyList' value='FirstVal'/>
                <list-push list='MyList' value='SecondVal'/>
                <echo message=""Shift returns: ${list::shift('MyList')}""/>
                <echo message=""Count is ${list::count('MyList')}""/>
                ";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Shift returns: FirstVal");
            AssertExt.StringContains(result, "Count is 1");
        }

        [Test]
        [ExpectedException(typeof(BuildException))]
        public void ShiftEmptyList()
        {
            string _xml = @"
                <echo message=""Shift returns: ${list::shift('MyList')}""/>
                ";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
        }
        #endregion

        #region Unshift
        [Test]
        public void Unshift()
        {
            string _xml = @"
				<list-push list='MyList' value='FirstVal'/>
				<list-push list='MyList' value='SecondVal'/>
                <list-unshift list='MyList' value='NewFirstVal'/>
				<echo message=""Count is ${list::count('MyList')}""/>
				<echo message=""First value is ${list::get-value('MyList', 0)}""/>
				<echo message=""Third value is ${list::get-value('MyList', 2)}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Count is 3");
            AssertExt.StringContains(result, "First value is NewFirstVal");
            AssertExt.StringContains(result, "Third value is SecondVal");
        }

        [Test]
        public void UnshiftDuplicateValues()
        {
            string _xml = @"
				<list-push list='MyList' value='FirstVal'/>
				<list-push list='MyList' value='FirstVal'/>
				<list-unshift list='MyList' value='SecondVal'/>
				<echo message=""Count is ${list::count('MyList')}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Count is 3");
        }

        [Test]
        [ExpectedException(typeof(BuildException))]
        public void UnshiftEmptyValue()
        {
            string _xml = @"
				<list-push list='MyList' value='FirstVal'/>
				<list-unshift list='MyList' value=''/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
        }

        [Test]
        [ExpectedException(typeof(BuildException))]
        public void UnshiftAtPosition()
        {
            string _xml = @"
				<list-push list='MyList' value='FirstVal'/>
				<list-unshift list='MyList' value='SecondVal' position='1'/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
        }
        #endregion

        #region Remove
        [Test]
		public void RemoveByValue()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-remove list='MyList' value='FirstVal'/>
				<echo message=""Count is ${list::count('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is 1");
		}

		[Test]
		public void RemoveByPosition()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-remove list='MyList' position='0'/>
				<echo message=""Count is ${list::count('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is 1");
		}

		[Test]
        [ExpectedException(typeof(BuildException))]
		public void RemoveByPositionPastUpperBound()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-remove list='MyList' position='2'/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
		}

		[Test]
        [ExpectedException(typeof(BuildException))]
		public void RemoveByPositionPastLowerBound()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-remove list='MyList' position='-1'/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
		}

		[Test]
		public void RemoveByValueBadValue()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-remove list='MyList' value='ThirdVal'/>
				<echo message=""Count is ${list::count('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is 2");
		}

		[Test]
        [ExpectedException(typeof(BuildException))]
		public void RemoveBothValueAndPosition()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-remove list='MyList' value='FirstVal' position='0'/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
		}

		[Test]
		public void RemoveWithDuplicateValues()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-remove list='MyList' value='FirstVal'/>
				<echo message=""First count is ${list::count('MyList')}""/>
				<list-remove list='MyList' value='FirstVal'/>
				<echo message=""Second count is ${list::count('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "First count is 2");
			AssertExt.StringContains(result, "Second count is 1");
		}
		#endregion

		#region Clear
		[Test]
		public void ClearWithItems()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-clear list='MyList'/>
				<echo message=""Count is ${list::count('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is 0");
		}

		[Test]
		public void ClearEmpty()
		{
			string _xml = @"
				<list-clear list='MyList'/>
				<echo message=""Count is ${list::count('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is 0");
		}
		#endregion

		#region For Each
		[Test]
		public void ForEach()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-add list='MyList' value='ThirdVal'/>
				<list-foreach list='MyList' value-property='_val'>
					<echo message=""Value is ${_val}""/>
				</list-foreach>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Value is FirstVal");
			AssertExt.StringContains(result, "Value is ThirdVal");
		}

		[Test]
		public void ForEachEmpty()
		{
			string _xml = @"
				<list-foreach list='MyList' value-property='_val'>
					<echo message=""Hello ${_val}""/>
				</list-foreach>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringDoesNotContain(result, "Hello");
		}
		#endregion

		#region Count
		[Test]
		public void CountWithNoItems()
		{
			string _xml = @"
				<echo message=""Count is ${list::count('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is 0");
		}

		[Test]
		public void CountWithItems()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<echo message=""Count is ${list::count('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is 2");
		}

		[Test]
		public void CountWithItemsRemoved()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-remove list='MyList' value='FirstVal'/>
				<echo message=""Count is ${list::count('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is 0");
		}

		[Test]
		public void CountAfterClear()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-clear list='MyList'/>
				<echo message=""Count is ${list::count('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is 0");
		}
		#endregion

        #region IS Empty
        [Test]
        public void IsEmptyWithItems()
        {
            string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<echo message=""IsEmpty is: ${list::is-empty('MyList')}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "IsEmpty is: False");
        }

        [Test]
        public void IsEmptyWithoutItems()
        {
            string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-remove list='MyList' value='FirstVal'/>
				<echo message=""IsEmpty is: ${list::is-empty('MyList')}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "IsEmpty is: True");
        }

        [Test]
        public void IsEmptyDoesNotExist()
        {
            string _xml = @"
				<echo message=""IsEmpty is: ${list::is-empty('MyList')}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "IsEmpty is: True");
        }

        #endregion

		#region Contains
		[Test]
		public void ContainsTrue()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<echo message=""List contains 'FirstVal': ${list::contains('MyList','FirstVal')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "List contains 'FirstVal': True");
		}

		[Test]
		public void ContainsFalse()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<echo message=""List contains 'ThirdVal': ${list::contains('MyList','ThirdVal')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "List contains 'ThirdVal': False");
		}
		#endregion

		#region GetValue
		[Test]
		public void GetValue()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-add list='MyList' value='ThirdVal'/>
				<echo message=""Second value is ${list::get-value('MyList', 1)}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Second value is SecondVal");
		}

		[Test]
        [ExpectedException(typeof(BuildException))]
		public void GetValueOutsideUpperBound()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-add list='MyList' value='ThirdVal'/>
				<echo message=""Value is ${list::get-value('MyList', 3)}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
		}

		[Test]
        [ExpectedException(typeof(BuildException))]
		public void GetValueOutsideLowerBound()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-add list='MyList' value='ThirdVal'/>
				<echo message=""Value is ${list::get-value('MyList', -1)}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
		}
		#endregion

		#region Exists
		[Test]
		public void ExistsTrue()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<echo message=""Existance is: ${list::exists('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Existance is: True");
		}

		[Test]
		public void ExistsFalse()
		{
			string _xml = @"
				<echo message=""Existance is: ${list::exists('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Existance is: False");
		}

		[Test]
		public void ExistsAfterClear()
		{
			string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-clear list='MyList'/>
				<echo message=""Existance is: ${list::exists('MyList')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Existance is: True");
		}
		#endregion

        #region Dump
        [Test]
        public void DumpNormalLengths()
        {
            string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-add list='MyList' value='ThirdVal'/>
				<list-add list='MyList' value='FourthVal'/>
				<list-add list='MyList' value='FifthVal'/>
				<list-add list='MyList' value='SixthVal'/>
				<list-add list='MyList' value='SeventhVal'/>
				<list-add list='MyList' value='EigthVal'/>
				<list-add list='MyList' value='NinthVal'/>
				<list-add list='MyList' value='TenthVal'/>
				<list-add list='MyList' value='EleventhVal'/>
                <list-dump list='MyList' verbose='true'/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
        }

        [Test]
        public void DumpValuesTruncated()
        {
            string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
				<list-add list='MyList' value='ThirdValIsSuperReallyLongAndWillGetTruncatedAtTheExtremeWidthOfTheConsoleBecauseThatIsHowThingsWork'/>
				<list-add list='MyList' value='FourthVal'/>
                <list-dump list='MyList' verbose='true'/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
        }
        #endregion

        #region Copy
        [Test]
        public void Copy()
        {
            string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
                <list-copy list='MyList' into='MyNewList'/>
				<echo message=""Value from old list is ${list::get-value('MyList', 0)}""/>
				<echo message=""Value from new list is ${list::get-value('MyNewList', 0)}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Value from old list is FirstVal");
            AssertExt.StringContains(result, "Value from new list is FirstVal");
        }

        /// <summary>
        /// Assert that once a list is copied into a new list
        /// the two lists are independent as far as adding and
        /// removing items.
        /// </summary>
        [Test]
        public void CopyListsIndependent()
        {
            string _xml = @"
				<list-add list='MyList' value='FirstVal'/>
				<list-add list='MyList' value='SecondVal'/>
                <list-copy list='MyList' into='MyNewList'/>
                <list-remove list='MyList' value='SecondVal'/>
                <list-add list='MyNewList' value='ThirdVal'/>
				<echo message=""Count of old list is ${list::count('MyList')}""/>
				<echo message=""Count of new list is ${list::count('MyNewList')}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Count of old list is 1");
            AssertExt.StringContains(result, "Count of new list is 3");
        }

        [Test]
        [ExpectedException(typeof(BuildException))]
        public void CopyIntoExistingListFails()
        {
            string _xml = @"
				<list-add list='ExistingList' value='FirstVal'/>
				<list-add list='MyList' value='FirstVal'/>
                <list-copy list='MyList' into='ExistingList'/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
        }
        #endregion

        #region Create
        [Test]
        public void Create()
        {
            string _xml = @"
				<list-create list='MyList'>
                    <list-item value='MyFirstValue'/>
                    <list-item value='MySecondValue'/>
                </list-create>
				<echo message=""Count is ${list::count('MyList')}""/>
				<echo message=""Second value is ${list::get-value('MyList',1)}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Count is 2");
            AssertExt.StringContains(result, "Second value is MySecondValue");
        }

        [Test]
        public void CreateIf()
        {
            string _xml = @"
				<list-create list='MyList'>
                    <list-item value='MyFirstValue'/>
                    <list-item value='MySecondValue' if='false'/>
                    <list-item value='MyThirdValue' if='true'/>
                </list-create>
				<echo message=""Count is ${list::count('MyList')}""/>
				<echo message=""Second value is ${list::get-value('MyList',1)}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Count is 2");
            AssertExt.StringContains(result, "Second value is MyThirdValue");
        }

        [Test]
        public void CreateUnless()
        {
            string _xml = @"
				<list-create list='MyList'>
                    <list-item value='MyFirstValue'/>
                    <list-item value='MySecondValue' unless='false'/>
                    <list-item value='MyThirdValue' unless='true'/>
                </list-create>
				<echo message=""Count is ${list::count('MyList')}""/>
				<echo message=""Second value is ${list::get-value('MyList',1)}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Count is 2");
            AssertExt.StringContains(result, "Second value is MySecondValue");
        }
        #endregion
    }
}
