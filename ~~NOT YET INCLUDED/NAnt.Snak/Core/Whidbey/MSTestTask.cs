using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Specialized;
using Microsoft.Win32;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;

using Snak.Utilities;

namespace Snak.Core.Whidbey
{
	/// <summary>
	/// Wraps the MSTest.exe test runner, responsible for running VSTS tests
	/// </summary>
	[TaskName("msTest")]
	public class MSTestTask :  NAnt.Core.Tasks.ExternalProgramBase
	{
		#region Constants

		const String win32KeyName = @"SOFTWARE\Microsoft\VisualStudio\8.0";
		const String win64KeyName = @"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\8.0";
		const String installDirValue = @"InstallDir";
		const String toolName = "MSTEST.EXE";
        
		#endregion  

		#region private vars

		private string _pathToTool = String.Empty;

		// switch values are stored in the following 3 vars
		private StringDictionary _simpleProperties = new StringDictionary();
		private bool _unique;
		private bool _noIsolation;

		// we default to deleting the resuts file as speified by the property ResultsFile
		private bool _deleteResultsFileIfExists = true;

		#endregion 

		#region Public Properties

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// noLogo
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// testContainers
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/// <summary>
		/// Gets or sets the list of items you can pass to the 
		/// /testcontainer: 
		/// </summary>
		/// <remarks>
		/// You have to set either <see cref="TestMetaData"/> or 
		/// <see cref="TestContainers"/> for this task to work.
		/// </remarks>
		[TaskAttribute("testContainers")]
		public string TestContainers
		{
			get { return ( GetKey ( "TestContainers" ) ); }
			set { _simpleProperties [ "TestContainers" ] = value; }
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// testMetaData
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/// <summary>
		/// Gets or sets the test meta data file to use.
		/// </summary>
		/// <remarks>
		/// You have to set either <see cref="TestMetaData"/> or 
		/// <see cref="TestContainers"/> for this task to work.
		/// </remarks>
		[TaskAttribute("testMetaData", Required=false)]
		public string TestMetaData
		{
			get { return ( GetKey ( "TestMetaData" ) ); }
			set { _simpleProperties [ "TestMetaData" ] = value; }
		}


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// runConfig
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/// <summary>
		/// Gets or sets the file passed to /runconfig:
		/// </summary>
		/// <remarks>
		/// See <see cref="ResultsFile"/> for a discussion on how this file is
		/// inspected to form the final name.
		/// </remarks>
		[TaskAttribute("runConfig", Required=false)]
		public string RunConfig
		{
			get { return ( GetKey ( "RunConfig" ) ); }
			set { _simpleProperties [ "RunConfig" ] = value; }
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// resultsFile
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/// <summary>
		/// Gets or sets the name of the output results file.
		/// </summary>
		/// <remarks>
		/// If a file extension is not specified, the standard .TRX will be 
		/// applied.
		/// <para>
		/// If directory information is present in the name, the directory will
		/// be created if it does not exist.
		/// </para>
		/// <para>
		/// If <see cref="UniqueName"/> is set to true, a date and timestamp 
		/// will be applied to the filename.
		/// </para>
		/// <para>
		/// If a <see cref="RunConfig"/> file is specified, that file will be 
		/// examined for timestamp and naming conventions.  The 
		/// <see cref="UniqueName"/> value is ignored.
		/// </para>
		/// </remarks>
		[TaskAttribute("resultsFile", Required=true)]
		public string ResultsFile
		{
			get { return ( GetKey ( "ResultsFile" ) ); }
			set { _simpleProperties [ "ResultsFile" ] = value; }
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// deleteResultsFileIfExists (not a switch of MSTest.exe)
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		[TaskAttribute("deleteResultsFileIfExists", Required=false)]
		public bool DeleteResultsFileIfExists
		{
			get { return ( _deleteResultsFileIfExists ); }
			set { _deleteResultsFileIfExists = value; }
		}
		
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// testLists
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// The meta data test lists to execute with /testlist:
		/// 
		/// Multiple lists can be specified and should be separated with a / 
		/// </summary>
		/// <remarks>
		/// This switch can only be used if <see cref="TestMetaData"/> is set.
		/// </remarks>
		[TaskAttribute("testLists")]
		public string TestLists
		{
			get { return ( GetKey ( "TestLists" ) ); }
			set { _simpleProperties [ "TestLists" ] = value; }
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// tests
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// The tests to execute from a metadata file or test container.
		/// </summary>
		[TaskAttribute("tests")]
		public string Tests
		{
			get { return ( GetKey ( "Tests" ) ); }
			set { _simpleProperties [ "Tests" ] = value; }
		}


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// unique
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		[TaskAttribute("unique", Required=false)]
		public bool Unique
		{
			get { return ( _unique ); }
			set { _unique = value; }
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// noIsolation
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////	

		/// <summary>
		/// The value for the /noisolation switch.
		/// </summary>
		/// <remarks>
		/// Defaults to false.
		/// </remarks>
		[TaskAttribute("noIsolation", Required=false)]
		public bool NoIsolation
		{
			get { return ( _noIsolation ); }
			set { _noIsolation = value; }
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// detail
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		//		/// <summary>
		//		/// Any /detail options requested in the output.
		//		/// </summary>
		//		/// <summary>
		//		/// The property prefix that's used to load up all the project properties, 
		//		/// </summary>
		//		[TaskAttribute("property", Required=true)]
		//		[StringValidator(AllowEmpty=false)]
		//		public string PropertyName 
		//		{
		//			get { return _propertyName; }
		//			set { _propertyName = value; }
		//		}

/*



		/// <summary>
		/// Publish results to the Team Foundation Server.
		/// </summary>
		public String Publish
		{
			get { return ( GetKey ( "Publish" ) ); }
			set { _simpleProperties [ "Publish" ] = value; }
		}

		/// <summary>
		/// The build identifier to be used to publish test results.
		/// </summary>
		public String PublishBuild
		{
			get { return ( GetKey ( "PublishBuild" ) ); }
			set { _simpleProperties [ "PublishBuild" ] = value; }
		}

		/// <summary>
		/// The name of the test results file to publish. If none is specified, 
		/// use the file produced by the current test run.
		/// </summary>
		public String PublishResultsFile
		{
			get { return ( GetKey ( "PublishResultsFile" ) ); }
			set { _simpleProperties [ "PublishResultsFile" ] = value; }
		}

		/// <summary>
		/// The name of the team project to which the build belongs. Specify 
		/// this when publishing test results.
		/// </summary>
		public String TeamProject
		{
			get { return ( GetKey ( "TeamProject" ) ); }
			set { _simpleProperties [ "TeamProject" ] = value; }
		}

		/// <summary>
		/// The platform of the build against which to publish test results.
		/// </summary>
		public String Platform
		{
			get { return ( GetKey ( "Platform" ) ); }
			set { _simpleProperties [ "Platform" ] = value; }
		}

		/// <summary>
		/// The flavor of the build against which to publish test results.
		/// </summary>
		public String Flavor
		{
			get { return ( GetKey ( "Flavor" ) ); }
			set { _simpleProperties [ "Flavor" ] = value; }
		}
        */



		
		#endregion

		#region overriden implementations
		
		/// <summary>
		/// Gets the command-line arguments for the external program.
		/// </summary>
		/// <value>
		/// The command-line arguments for the external program.
		/// </value>
		public override string ProgramArguments 
		{
			get { return GetCommandLineString(); }
		}

		/// <summary>
		/// Gets the filename of the external program to start.
		/// </summary>
		/// <value>
		/// The filename of the external program.
		/// </value>
		public override string ProgramFileName 
		{
			get 
			{
				return _pathToTool;
			}
		}

		#endregion 

		public MSTestTask()
		{
			RegistryKey reg = null;
			try
			{
				// First look in the Win64 place.
				reg = Registry.LocalMachine.OpenSubKey ( win64KeyName );
				if ( null == reg )
				{
					// Try the Win32 key.
					reg = Registry.LocalMachine.OpenSubKey ( win32KeyName );
				}
				if ( null != reg )
				{
					// Read the ProductDir string value.
					String dir = reg.GetValue ( installDirValue ,
						String.Empty ) as String;
					if ( dir.Length > 0 )
					{
						// Poke on the tool name.
						_pathToTool = dir + toolName;
					}
				}
			}
			finally
			{
				if ( null != reg )
				{
					reg.Close ( );
				}
			}

			if (_pathToTool == String.Empty)
			{
				// piers, this ok.. better ex perhaps?
				throw new  ApplicationException("The MSTest.exe file could not be found, are you sure Visual Studio 2005 (testers edition or greater e.g. VSTS) is installed on the machine.");
			}

			this.ExeName = toolName;
		}

		protected override void ExecuteTask()
		{
			if (_deleteResultsFileIfExists && File.Exists(GetKey("ResultsFile")))
			{
				Console.WriteLine("Deleting the results file from a previous test run: " + GetKey("ResultsFile"));
				File.Delete(GetKey("ResultsFile"));
			}

			Console.WriteLine("Calling MSTest with the following switches:" + GetCommandLineString()); 
			base.ExecuteTask();

			// todo: move this and DumpProperties() to unit tests below! maybe a use for it there...
			System.Reflection.PropertyInfo [] properties = this.GetType().GetProperties();
			DumpProperties(this, properties);
		}

		private string GetCommandLineString()
		{
			CommandLineBuilder builder = new CommandLineBuilder();

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/// noLogo
			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			builder.AppendSwitch ( "/nologo" );

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/// testContainers
			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			builder.AppendSwitchIfNotNullOrEmpty ( "/testcontainer:" , GetKey ( "Testcontainer" ) );

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/// testMetaData
			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			builder.AppendSwitchIfNotNullOrEmpty ( "/testmetadata:" , GetKey ( "TestMetaData" ) );

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/// runConfig
			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			builder.AppendSwitchIfNotNullOrEmpty ( "/runconfig:" , GetKey ( "RunConfig" ) );

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/// resultsFile
			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			builder.AppendSwitchIfNotNullOrEmpty ( "/resultsfile:" , GetKey ( "ResultsFile" ) );

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/// testLists
			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			builder.AppendSwitchIfNotNullOrEmpty ( "/testlist:" , GetKey ( "TestLists" ) );

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/// tests
			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			builder.AppendSwitchIfNotNullOrEmpty ( "/test:" ,  GetKey ( "Tests" ) );
			
			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/// unique
			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			if (_unique)
				builder.AppendSwitch("/unique:");

			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			/// noIsolation
			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			if (_noIsolation)
				builder.AppendSwitch( "/noisolation:");
//
//
//			// Any details?  Pile on!
//			builder.AppendSwitchIfNotNullOrEmpty ( "/detail:" , detailList );

//			// All those Team System parameters.
//			builder.AppendSwitchIfNotNullOrEmpty ( "/publish:" ,
//				GetKey ( "Publish" ) );
//			builder.AppendSwitchIfNotNullOrEmpty ( "/publishbuild:" ,
//				GetKey ( "PublishBuild" ) );
//			builder.AppendSwitchIfNotNullOrEmpty ( "/publishresultsfile:" ,
//				GetKey ( "PublishResultsFile" ) );
//			builder.AppendSwitchIfNotNullOrEmpty ( "/teamproject:" ,
//				GetKey ( "TeamProject" ) );
//			builder.AppendSwitchIfNotNullOrEmpty ( "/platform:" ,
//				GetKey ( "Platform" ) );
//			builder.AppendSwitchIfNotNullOrEmpty ( "/flavor:" , GetKey ( "Flavor" ) );

			return builder.GetCommand();
		}

		#region Private Helper Methods
		
		private String GetKey ( String key )
		{
			String ret = String.Empty;
			
			if (_simpleProperties.ContainsKey(key))
			{
				ret = _simpleProperties[key];									   
			}

			return ret;
		}

		#endregion

		private void DumpProperties(object the_object, System.Reflection.PropertyInfo [] properties)
		{
			Console.WriteLine("");
			Console.WriteLine("{0} -----------------",the_object.GetType().Name);
			foreach (System.Reflection.PropertyInfo property in properties)
			{
				Type prop_type = property.PropertyType;
				string val = "";
				if ((prop_type == typeof(int)) 
					|| (prop_type == typeof(string)) 
					|| (prop_type == typeof(bool)) 
					|| (prop_type == typeof(float)) 
					|| (prop_type == typeof(double)) 
					|| (prop_type == typeof(UInt32)) 
					|| (prop_type == typeof(UInt64)) 
					|| (prop_type == typeof(UInt32)) 
					|| (prop_type == typeof(UInt16)) 
					|| (prop_type == typeof(System.DateTime)))
				{
					object obj_val = property.GetValue(the_object, null);
					if (obj_val == null)
					{
						val = "null";
					}
					else
					{
						val = string.Format( "\"{0}\"",obj_val.ToString() );
					}
				}
				else
				{
					val = "object=" + prop_type.ToString() + "";
				}
				Console.WriteLine("  {0} = {1}", property.Name, val);

			}
		}

	}
}
