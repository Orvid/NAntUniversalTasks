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
using System.Reflection;
using NUnit.Framework;
using NAnt.Core;
using NAntCollections.Dictionary;
using NAntCollections.Utility;
using NAntCollections.Tests.Utility;
#endregion

namespace NAntCollections.Tests
{
	/// <summary>
	/// Unit tests related to dictionary support.
	/// </summary>
	[TestFixture]
	public class DictionaryTests
    {
        #region SetUp
        [SetUp]
        public void SetUp()
        {
            DictionaryManager.Reset();
        }
        #endregion

        #region Add
        [Test]
		public void Add()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<echo message=""Count is: ${dict::count('MyDict')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is: 2");
		}

		[Test]
		[ExpectedException(typeof(BuildException))]
		public void AddEmptyValue()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value=''/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
		}

		[Test]
        [ExpectedException(typeof(BuildException))]
		public void AddEmptyKey()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='' value='FirstVal'/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
		}

		[Test]
		public void AddWithOverwrite()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstValAgain' overwrite='true'/>
				<echo message=""Value is: ${dict::get-value('MyDict', 'FirstKey')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Value is: FirstValAgain");
		}

		[Test]
        [ExpectedException(typeof(BuildException))]
		public void AddWithoutOverwrite()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstValAgain'/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
		}
		#endregion

		#region Remove
		[Test]
		public void Remove()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<dict-remove dictionary='MyDict' key='FirstKey'/>
				<echo message=""Count is: ${dict::count('MyDict')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is: 1");
		}

		[Test]
		public void RemoveNonexistantValue()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<dict-remove dictionary='MyDict' key='ThirdKey'/>
				<echo message=""Count is: ${dict::count('MyDict')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is: 2");
		}
		#endregion

		#region For Each
		[Test]
		public void ForEachWithKey()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<dict-add dictionary='MyDict' key='ThirdKey' value='ThirdVal'/>
				<dict-foreach dictionary='MyDict' key-property='_key'>
					<echo message=""Found key ${_key}""/>
				</dict-foreach>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Found key FirstKey");
			AssertExt.StringContains(result, "Found key ThirdKey");
		}

		[Test]
		public void ForEachWithValue()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<dict-add dictionary='MyDict' key='ThirdKey' value='ThirdVal'/>
				<dict-foreach dictionary='MyDict' value-property='_val'>
					<echo message=""Found value ${_val}""/>
				</dict-foreach>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Found value FirstVal");
			AssertExt.StringContains(result, "Found value ThirdVal");
		}

		[Test]
		public void ForEachWithKeyAndValue()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<dict-add dictionary='MyDict' key='ThirdKey' value='ThirdVal'/>
				<dict-foreach dictionary='MyDict' key-property='_key' value-property='_val'>
					<echo message=""Key ${_key} has value ${_val}""/>
				</dict-foreach>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Key FirstKey has value FirstVal");
			AssertExt.StringContains(result, "Key ThirdKey has value ThirdVal");
		}

		[Test]
		public void ForEachWithNeitherKeyNorValue()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<dict-add dictionary='MyDict' key='ThirdKey' value='ThirdVal'/>
				<dict-foreach dictionary='MyDict'>
					<echo message=""Hello world""/>
				</dict-foreach>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Hello world");
		}

		[Test]
		public void ForEachEmpty()
		{
			string _xml = @"
				<dict-foreach dictionary='MyDict'>
					<echo message=""Hello world""/>
				</dict-foreach>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringDoesNotContain(result, "Hello world");
		}
		#endregion

		#region Get Value
		[Test]
		public void GetValue()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<echo message=""Value of FirstKey is: ${dict::get-value('MyDict','FirstKey')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Value of FirstKey is: FirstVal");
		}

		[Test]
        [ExpectedException(typeof(BuildException))]
		public void GetValueDoesNotExist()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<echo message=""Value of ThirdKey is: ${dict::get-value('MyDict','ThirdKey')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
		}
		#endregion

		#region Contains Key
		[Test]
		public void ContainsKeyTrue()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<echo message=""Dictionary contains key 'FirstKey': ${dict::contains-key('MyDict','FirstKey')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Dictionary contains key 'FirstKey': True");
		}

		[Test]
		public void ContainsKeyFalse()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<echo message=""Dictionary contains key 'ThirdKey': ${dict::contains-key('MyDict','ThirdKey')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Dictionary contains key 'ThirdKey': False");
		}
		#endregion

		#region Count
		[Test]
		public void CountWithItems()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<echo message=""Count is: ${dict::count('MyDict')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is: 2");
		}

		[Test]
		public void CountWithoutItems()
		{
			string _xml = @"
				<echo message=""Count is: ${dict::count('MyDict')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is: 0");
		}

		[Test]
		public void CountWithItemsRemoved()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-remove dictionary='MyDict' key='FirstKey'/>
				<echo message=""Count is: ${dict::count('MyDict')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is: 0");
		}

		[Test]
		public void CountAfterClear()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-clear dictionary='MyDict'/>
				<echo message=""Count is: ${dict::count('MyDict')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is: 0");
		}
		#endregion

        #region IS Empty
        [Test]
        public void IsEmptyWithItems()
        {
            string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<echo message=""IsEmpty is: ${dict::is-empty('MyDict')}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "IsEmpty is: False");
        }

        [Test]
        public void IsEmptyWithoutItems()
        {
            string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-remove dictionary='MyDict' key='FirstKey'/>
				<echo message=""IsEmpty is: ${dict::is-empty('MyDict')}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "IsEmpty is: True");
        }

        [Test]
        public void IsEmptyDoesNotExist()
        {
            string _xml = @"
				<echo message=""IsEmpty is: ${dict::is-empty('MyDict')}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "IsEmpty is: True");
        }

        #endregion

		#region Clear
		[Test]
		public void Clear()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<dict-clear dictionary='MyDict'/>
				<echo message=""Count is: ${dict::count('MyDict')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is: 0");
		}

		[Test]
		public void ClearNothing()
		{
			string _xml = @"
				<dict-clear dictionary='MyDict'/>
				<echo message=""Count is: ${dict::count('MyDict')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Count is: 0");
		}
		#endregion

		#region Exists
		[Test]
		public void ExistsTrue()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<echo message=""Existance is: ${dict::exists('MyDict')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Existance is: True");
		}

		[Test]
		public void ExistsFalse()
		{
			string _xml = @"
				<echo message=""Existance is: ${dict::exists('MyDict')}""/>
				";
			string result = NAntScriptBuilder.BuildAndRunScript(_xml);
			AssertExt.StringContains(result, "Existance is: False");
		}

		[Test]
		public void ExistsAfterClear()
		{
			string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-clear dictionary='MyDict'/>
				<echo message=""Existance is: ${dict::exists('MyDict')}""/>
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
				<dict-add dictionary='MyDict' key='FirstKey'  value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<dict-add dictionary='MyDict' key='ThirdKey'  value='ThirdVal'/>
				<dict-dump dictionary='MyDict' verbose='true'/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
        }

        [Test]
        public void DumpKeysTruncated()
        {
            string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey'  value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<dict-add dictionary='MyDict' key='ThirdKey'  value='ThirdVal'/>
				<dict-add dictionary='MyDict' key='ThisIsTheFourKeyAndTheNameOfTheKeyIsReallyLongForReallyNoGoodReason'  value='ThirdVal'/>
				<dict-dump dictionary='MyDict' verbose='true'/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
        }

        [Test]
        public void DumpValuesTruncated()
        {
            string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey'  value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<dict-add dictionary='MyDict' key='ThirdKey'  value='ThirdVal'/>
				<dict-add dictionary='MyDict' key='FourthKey' value='FourthValWhichIsReallyLongForSomeReasonAndWillGetTruncatedByADump'/>
				<dict-dump dictionary='MyDict' verbose='true'/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
        }
        #endregion

        #region Copy
        [Test]
        public void Copy()
        {
            string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<dict-copy dictionary='MyDict' into='MyNewDict'/>
				<echo message=""Value is ${dict::get-value('MyNewDict','FirstKey')}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Value is FirstVal");
        }

        /// <summary>
        /// Assert that after a copy, the two dictionaries operate independently.
        /// </summary>
        [Test]
        public void CopyDictionariesIndependent()
        {
            string _xml = @"
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<dict-copy dictionary='MyDict' into='MyNewDict'/>
				<dict-remove dictionary='MyDict' key='FirstKey'/>
                <dict-add dictionary='MyNewDict' key='ThirdKey' value='ThirdVal'/>
				<echo message=""Count of old is ${dict::count('MyDict')}""/>
				<echo message=""Count of new is ${dict::count('MyNewDict')}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Count of old is 1");
            AssertExt.StringContains(result, "Count of new is 3");
        }

        [Test]
        [ExpectedException(typeof(BuildException))]
        public void CopyIntoExistingFails()
        {
            string _xml = @"
				<dict-add dictionary='MyExistingDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='FirstKey' value='FirstVal'/>
				<dict-add dictionary='MyDict' key='SecondKey' value='SecondVal'/>
				<dict-copy dictionary='MyDict' into='MyExistingDict'/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
        }
        #endregion

        #region Create
        [Test]
        public void Create()
        {
            string _xml = @"
				<dict-create dictionary='MyDict'>
                    <dict-item key='MyFirstKey' value='MyFirstValue'/>
                    <dict-item key='MySecondKey' value='MySecondValue'/>
                </dict-create>
				<echo message=""Count is ${dict::count('MyDict')}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Count is 2");
        }

        [Test]
        public void CreateIf()
        {
            string _xml = @"
				<dict-create dictionary='MyDict'>
                    <dict-item key='MyFirstKey' value='MyFirstValue' if='true'/>
                    <dict-item key='MySecondKey' value='MySecondValue' if='false'/>
                </dict-create>
				<echo message=""Count is ${dict::count('MyDict')}""/>
				<echo message=""Key's value is ${dict::get-value('MyDict','MyFirstKey')}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Count is 1");
            AssertExt.StringContains(result, "Key's value is MyFirstValue");
        }

        [Test]
        public void CreateUnless()
        {
            string _xml = @"
				<dict-create dictionary='MyDict'>
                    <dict-item key='MyFirstKey' value='MyFirstValue' unless='true'/>
                    <dict-item key='MySecondKey' value='MySecondValue' unless='false'/>
                </dict-create>
				<echo message=""Count is ${dict::count('MyDict')}""/>
				<echo message=""Key's value is ${dict::get-value('MyDict','MySecondKey')}""/>
				";
            string result = NAntScriptBuilder.BuildAndRunScript(_xml);
            AssertExt.StringContains(result, "Count is 1");
            AssertExt.StringContains(result, "Key's value is MySecondValue");
        }
        #endregion
    }
}
