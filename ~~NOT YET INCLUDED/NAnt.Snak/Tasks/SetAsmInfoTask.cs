using System;
using System.IO;
using NAnt.Core.Attributes;

using Snak.Core;

namespace Snak.Tasks
{
	/// <summary>
	/// Overwrites portions of an existing AssemblyInfo file
	/// </summary>
	[TaskName("setasminfo")]
	public class SetAsmInfoTask : ProjectTask
	{
		private string _assemblyKeyFile;

		/// <summary>
		/// Allows overwriting of the AssemblyInfo's AssemblyKeyFile
		/// </summary>
		[TaskAttribute("assemblyKeyFile", Required=false)]
		[StringValidator(AllowEmpty=false)]
		public string AssemblyKeyFile
		{
			get { return _assemblyKeyFile; }
			set { _assemblyKeyFile = value; }
		}

		protected override void ExecuteTask()
		{
			IProjectInfo project = GetProject();
			FileInfo assemblyInfo = new FileInfo("assemblyinfo" + project.CodeFileExtension);
			if (assemblyInfo.Exists)
			{
				string assemblyInfoContents = File.ReadAllText(assemblyInfo.FullName);
			}
		}


#if(UNITTEST)
		[NUnit.Framework.TestFixture]
		public class SetAsmInfoTaskTester{

			#region Setup test and mock objects
			SetAsmInfoTask aSetAsmInfoTask = new SetAsmInfoTask();

			[NUnit.Framework.TestFixtureSetUp]
			public void Init(){}

			[NUnit.Framework.SetUp]
			public void Setup(){}
			#endregion

			[NUnit.Framework.Test(Description="")]
			public void TestSomeMethod(){

			}
		}
#endif
	}
}
