using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using Microsoft.Win32;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;

using Snak.Core;

namespace Snak.Utilities
{
	/// <summary>
	/// Provides specific information regarding a particular .Net framework, the particular framework is specified in the constructor.
	/// </summary>
	/// <remarks>Most of this information can be found in the registry under HKLM:\Software\Microsoft\.NETFramework</remarks>
	internal class DotNetFrameworkInfo
	{
		private const String dotNetFrameworkKeyName = @"Software\Microsoft\.NETFramework";

		private string _clrVersion = String.Empty;
		private string sdkInstallRootv1_1 = String.Empty;
		private string sdkInstallRootv2_0 = String.Empty;

		private StringCollection _installedFrameworks = new StringCollection();
		private DotNetFrameworkVersion _dotNetFrameworkVersion = DotNetFrameworkVersion.v1_1;
		private DirectoryInfo _pathToFramework = null;
		private DirectoryInfo _frameworkInstallRoot = null;
		private DirectoryInfo _frameworkSdkInstallRoot = null;

		public string ClrVersion
		{
			get { return (_clrVersion ); }
		}

		public DirectoryInfo PathToFramework
		{
			get { return (_pathToFramework ); }
		}

		public DirectoryInfo FrameworkSdkInstallRoot
		{
			get { return (_frameworkSdkInstallRoot ); }
		}

		public DotNetFrameworkInfo(string clrVersion)
		{
			_clrVersion = clrVersion;		
	
			LoadVersionInformation();
		}

		public DotNetFrameworkVersion DotNetFrameworkVersion
		{
			get {return _dotNetFrameworkVersion; }
		}

		public DotNetFrameworkInfo(DotNetFrameworkVersion dotNetFrameworkVersion)
		{
			_dotNetFrameworkVersion = dotNetFrameworkVersion;

			LoadVersionInformation();
		}

		private void LoadVersionInformation()
		{
			RegistryKey dotNetInfoKey = null;
			RegistryKey dotNetPolicyKey = null;
			try
			{
				dotNetInfoKey = Registry.LocalMachine.OpenSubKey ( dotNetFrameworkKeyName );
				
				if ( null != dotNetInfoKey)
				{
					// get the location where the .net framework's are installed (this value should be the same for all versions)
					_frameworkInstallRoot = new DirectoryInfo((string)dotNetInfoKey.GetValue("InstallRoot", @"C:\Windows\Microsoft.NET\Framework"));

					// open the policy sub keys so we can grab the information about the versions installed on the machine 
					dotNetPolicyKey = dotNetInfoKey.OpenSubKey("policy");

					sdkInstallRootv1_1 = (string)dotNetInfoKey.GetValue("sdkInstallRootv1.1");
					sdkInstallRootv2_0 = (string)dotNetInfoKey.GetValue("sdkInstallRootv2.0");

					string[] subKeys = dotNetPolicyKey.GetSubKeyNames();

					for( int i =0; i < subKeys.Length; i ++)
					{
						if (subKeys[i].IndexOf("v") > -1)
						{
							// We assume there is always 1 and only 1 element in the policy key and its value is 
							// the sub version of the framework installed. 
							// Check out http://support.microsoft.com/?scid=kb;en-us;315291 for further info on the policy key
							_installedFrameworks.Add(subKeys[i] + "." + dotNetPolicyKey.OpenSubKey(subKeys[i]).GetValueNames()[0]);
						}
					}

					if (_installedFrameworks.Count ==0)
					{
						throw new BuildException("Could not find any information in registry relating to specific .Net framework installations.");
					}
				}
				else
				{
					throw new BuildException("Could not find any information in registry relating to the .Net framework.");
				}
			}
			finally
			{
				if ( dotNetPolicyKey != null )
				{
					dotNetPolicyKey.Close ( );
				}

				if ( dotNetInfoKey != null )
				{
					dotNetInfoKey.Close ( );
				}
			}
		
			//
			// from here on down we do some checks to make sure we have all the correct info
			//

			if (_clrVersion == String.Empty)
			{
				// if this is empty then we will use the value of _dotNetFrameworkVersion and simply return the latest framework version
				string defaultVersion = this._dotNetFrameworkVersion.ToString().Replace("_", ".");

				foreach (string version in _installedFrameworks)
				{
					if (version.StartsWith(defaultVersion))
					{
						_clrVersion = version;
						break;
					}
				}

				if (_clrVersion == String.Empty)
				{
					throw new BuildException("No version of .Net for the " + defaultVersion + " release was detected on the machine.");
				}
			}
			else if (_clrVersion != String.Empty)
			{
				if (! _installedFrameworks.Contains(_clrVersion))
					throw new BuildException("The clrVersion '" + _clrVersion + "' is not installed on the machine");
			
				string releaseVersion = Regex.Match(_clrVersion, @"v[0-9]{1}\.[0-9]{1}", RegexOptions.IgnoreCase).Value.Replace(".", "_");

				this._dotNetFrameworkVersion = (DotNetFrameworkVersion)Enum.Parse(typeof(DotNetFrameworkVersion),releaseVersion, true);
			}

			_pathToFramework = new DirectoryInfo(_frameworkInstallRoot.FullName + "\\" + _clrVersion);

			if (!_pathToFramework.Exists)
			{
				throw new BuildException("Althought the clrVersion '" + _clrVersion + "' was detected as being installed on the machine but its location at '" + _pathToFramework.FullName + "' does not exist or its access is denied.");
			}
	
			if (sdkInstallRootv1_1 != String.Empty && _clrVersion.StartsWith("v1.1"))
			{
				_frameworkSdkInstallRoot = new DirectoryInfo(sdkInstallRootv1_1);
			}
			else if (sdkInstallRootv2_0 != String.Empty && _clrVersion.StartsWith("v2.0"))
			{
				_frameworkSdkInstallRoot = new DirectoryInfo(sdkInstallRootv2_0);
			}
		}
	}
}
