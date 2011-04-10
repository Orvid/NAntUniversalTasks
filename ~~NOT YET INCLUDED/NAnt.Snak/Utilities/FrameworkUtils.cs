using System;
using System.Collections;
using System.IO;
using System.Security.Permissions;
using Microsoft.Win32;

namespace Snak.Utilities
{
	/// <summary>
	/// Utility functionality for dealing with .Net framework versions, and where they're installed
	/// </summary>
	/// <remarks>
	public class FrameworkUtils
	{
		#region Declarations
		private static string frameworkInstallPath;

		private const string NET_INSTALL_ROOT_KEY = @"SOFTWARE\Microsoft\.NETFramework";
		private const string NET_INSTALL_ROOT_NAME = @"InstallRoot";
		#endregion

		static FrameworkUtils(){
			RegistryKey frameworkKey = Registry.LocalMachine.OpenSubKey(NET_INSTALL_ROOT_KEY, false);
			frameworkInstallPath = (string)frameworkKey.GetValue(NET_INSTALL_ROOT_NAME);
		}

		public static string GetFrameworkFolder(string version)
		{
			return Path.Combine(frameworkInstallPath, version);
		}

		[RegistryPermission(SecurityAction.Assert, Read=@"HKLM\" + NET_INSTALL_ROOT_NAME)]
		public static string[] GetInstalledVersions()
		{
			RegistryKey frameworkKey = Registry.LocalMachine.OpenSubKey(NET_INSTALL_ROOT_KEY, false);
			string[] names = frameworkKey.GetSubKeyNames();
			ArrayList versionNames = new ArrayList(names.Length);
			for (int i = 0; i < names.Length; i++) {
				if (names[i].StartsWith("v"))
					versionNames.Add(names[i]);
			}
			return (string[])versionNames.ToArray(typeof(string));
		}

#if(UNITTEST)
		[NUnit.Framework.TestFixture]
		[NUnit.Framework.Ignore("Tests for FrameworkUtils not written yet")]
		public class FrameworkUtilsTester {

			#region Setup test and mock objects
			FrameworkUtils aFrameworkUtils = new FrameworkUtils();

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
