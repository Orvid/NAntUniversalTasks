using System;
using System.IO;
using Snak.Tasks;

namespace Snak.Core
{
	/// <summary>
	/// Provides a facade over the various Everett / Whitbey solution file formats
	/// </summary>
	/// <remarks>
	/// <example>
	///   <para>Some example</para>
	///   <code>
	///     <![CDATA[
	///     some code here
	///     ]]>
	///   </code>
	/// </example>
	/// </remarks>
	public class SolutionFactory
	{
		#region Constructor(s)
		private SolutionFactory()
		{
		}
		#endregion

        /// <summary>
        /// Returns the correct <see cref="ISolutionInfo"/> implementation
        /// for the solution file supplied, with no logging enabled
        /// </summary>
        public static ISolutionInfo GetSolution(string solutionFilePath)
        {
            return GetSolution(new FileInfo(solutionFilePath), NullLogger.Log);
        }

		/// <summary>
		/// Returns the correct <see cref="ISolutionInfo"/> implementation
		/// for the solution file supplied
		/// </summary>
        public static ISolutionInfo GetSolution(string solutionFilePath, LoggingHandler log)
		{
			return GetSolution(new FileInfo(solutionFilePath), log);
		}

		/// <summary>
		/// Returns the correct <see cref="ISolutionInfo"/> implementation
		/// for the solution file supplied
		/// </summary>
        public static ISolutionInfo GetSolution(FileInfo solutionFile, LoggingHandler log)
		{
			try
			{
				bool isVS2005 = false;
				using (StreamReader sr = new StreamReader(solutionFile.FullName))
				{
					string firstLines = sr.ReadLine() + sr.ReadLine() + sr.ReadLine();
					if (firstLines.Contains(@"Microsoft Visual Studio Solution File, Format Version 9.00"))
					{
						isVS2005 = true;
					}
				}

				if (isVS2005)
					return Whidbey.SolutionInfo.CreateNew(solutionFile, log);
				else
                    return new Everett.SolutionInfo(solutionFile, log);


			}catch(Exception err)
			{
				throw new InvalidOperationException(string.Format("Failed to load solution from {0}", solutionFile, err.Message), err);	
			}
		}

#if(UNITTEST)
		[NUnit.Framework.TestFixture]
		[NUnit.Framework.Ignore("Tests for SolutionFactory not written yet")]
		public class SolutionFactoryTester{

			#region Setup test and mock objects
			SolutionFactory aSolutionFactory = new SolutionFactory();

			[NUnit.Framework.TestFixtureSetUp]
			public void Init(){}

			[NUnit.Framework.SetUp]
			public void Setup(){}
			#endregion

			[NUnit.Framework.Test(Description="")]
			public void TestSomeMethod(){

			}


			[NUnit.Framework.Test(Description="")]
			public void TestGetSolution()
			{

			}


			[NUnit.Framework.Test(Description="")]
			public void TestGetProject()
			{

			}

		}
#endif

	}
}
