using System;
using System.IO;
using System.DirectoryServices;
using System.Text.RegularExpressions;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;
using NAnt.Contrib.Tasks.Web;

using Snak.Utilities;

namespace Snak.Tasks
{
    // TODO: just realised that shelling out to ASPNET_REGIIS causes all the sites on the server to go down 
    // even when we only specify the path to update the script mappings on e.g:
    // ASPNET_REGIIS.exe -s W3SVC/1/Root/SnakFunctionalTests_webapplication1_ANiceSuffix, 
    // obviously this is a bad thing.... 
    // reference of a possible fix: http://www.hanselman.com/blog/CommentView.aspx?guid=913eee57-3a50-4ad9-9d36-1a8a094055d2#commentstart

	/// <summary>
	/// A task that can change the target framework for a given web site or virtual directory.
	/// </summary>
	/// <remarks>
	/// The option to recursively change the framework version is not working as expected. 
	/// 
	/// This task creates the correct switches to pass to aspnet_regiis.exe (this program this task shells out to) but they don’t work from with this task or independently via the command line.
	///  
	/// It's possible that the framework change will always be recursively applied from the level you specific, be it either the website or virtual directory. E.g. if you don’t specify a virtual directory then the recursion will apply from the 
	/// website level, if you specify a virtual directory then it will be applied from that level down.
	/// </remarks>
	[TaskName("changeWebSiteAspNetVersion")]
	public class ChangeWebSiteAspNetVersionTask :  NAnt.Core.Tasks.ExternalProgramBase
	{
		private string _websiteName = String.Empty;
		private long _websiteId = -1;
		private string _server = String.Empty;
		private string _virtualDirToChange = String.Empty;
		private string _clrVersion = String.Empty; //Snak.FrameworkVersion.DotNet_2_0;
		private bool _recursivelyApplyChange = false;
		private string _pathToTool = String.Empty;
		
		private const string IIS_TYPE_IIS_WEB_SERVER = "IIsWebServer";
		private const string IIS_TYPE_IIS_WEB_VIRTUAL_DIR = "IIsWebVirtualDir";
		private const string IIS_TYPE_IIS_WEB_DIRECTORY = "IIsWebDirectory";
		
		private DirectoryEntry _iisNodeToChangeFrameworkOn = null;
		private DotNetFrameworkInfo _dotNetFrameworkInfo = null;

		/// <summary>
		/// The name of the website to change the mapping of, if no value is specified for the attribute virtualDirToChange then
		/// the framework will be changed at the web site level.
		/// 
		/// If there are more than one website of the given name then the first one found is used. Use websiteId for a more specific search.
		/// </summary>
		/// <remarks>If no value is speified for the websiteName attribute then a value bust be speified for websiteId.</remarks>
		[TaskAttribute("websiteName", Required=false)]
		[StringValidator(AllowEmpty=false)]
		public string WebsiteName
		{
			get { return (_websiteName ); }
			set { _websiteName = value; }
		}

		/// <summary>
		/// If no value is specified for the websiteId attribute then a value bust be specified for websiteName.
		/// </summary>
		[TaskAttribute("websiteId", Required=false)]
		public long WebsiteId
		{
			get { return ( _websiteId ); }
			set { _websiteId = value; }
		}

		/// <summary>
		/// The address of the iis server, e.g: localhost, or perhaps wks511660.intranet.justice.wa.gov.au 
		/// </summary>
		[TaskAttribute("server", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string Server
		{
			get { return ( _server ); }
			set { _server = value; }
		}

		/// <summary>
		/// If specified the framework will be changed only on virtual directory provided
		/// </summary>
		[TaskAttribute("virtualDirToChange", Required=false)]
		[StringValidator(AllowEmpty=false)]
		public string VirtualDirToChange
		{
			get { return ( _virtualDirToChange ); }
			set { _virtualDirToChange = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Cant guarantee this will work as aspnet_regiis.exe (this program this task shells out to) doesn’t seem to work. This task creates the correct switches but they don’t work from with this task or via the command line.</remarks>
		[TaskAttribute("recursivelyApplyChange", Required=false)]
		public bool RecursivelyApplyChange
		{
			get { return ( _recursivelyApplyChange ); }
			set { _recursivelyApplyChange = value; }
		}

		/// <summary>
		/// The .Net framework to change to, acceptable values are net-1.1 or net-2.0
		/// </summary>
		[TaskAttribute("clrVersion", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string ClrVersion
		{
			get { return ( _dotNetFrameworkInfo.ClrVersion ); }
			set 
			{
				if (_dotNetFrameworkInfo == null)
				{
					_dotNetFrameworkInfo = new DotNetFrameworkInfo(value);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal DotNetFrameworkInfo DotNetFrameworkInfo
		{
			get { return ( _dotNetFrameworkInfo); }
			set { _dotNetFrameworkInfo = value; }
		}

		public ChangeWebSiteAspNetVersionTask() : base() {} 

		/// <summary>
		/// Use this constructor if you've just created a virtual directory using CreateVirtualDirectoryWrapper and want to update it's
		/// framework version.
		/// </summary>
		/// <param name="createVirtualDirectoryWrapperThatsFinishedItsExecuteTask"></param>
		internal ChangeWebSiteAspNetVersionTask(CreateVirtualDirectoryWithSpecificAspNetVersionTask createVirDirWithSpecAspNetVerThatsFinishedItsExecuteTask)
		{
			_iisNodeToChangeFrameworkOn = new DirectoryEntry(createVirDirWithSpecAspNetVerThatsFinishedItsExecuteTask.AppRoot);
		}

		#region NAnt.Core.Tasks.ExternalProgramBase overriden items
              
		/// <summary>
		/// Gets the command-line arguments for the external program.
		/// </summary>
		/// <value>
		/// The command-line arguments for the external program.
		/// </value>
		public override string ProgramArguments 
		{
			get { return GetProgramArguments(); }
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

		protected override void ExecuteTask()
		{
			if (_iisNodeToChangeFrameworkOn == null)
			{
				if (_websiteId == -1 && _websiteName == String.Empty)
				{
					throw new ApplicationException("You must specify either a value for at least one of the following attributes: websiteId, websiteName");
				}

				DirectoryEntry webSite = null;

				// if the web site id was not specified we try find the web site based on the name (if there are more than one we just grab the first)
				if (_websiteId == -1)
				{
					DirectoryEntry iisServer = new DirectoryEntry(String.Format("IIS://{0}/W3SVC", _server));

					// try find the id based on the websiteName
					webSite = GetWebSite(iisServer, _websiteName);

					if (webSite == null)
					{
						throw new ApplicationException("Could not find the website based on the website name: '" + _websiteName + "'" );
					}
				}
				else
				{
					webSite = new DirectoryEntry(String.Format("IIS://{0}/W3SVC/{1}/Root", _server, _websiteId));

					if (webSite == null)
					{
						throw new ApplicationException("Could not find the website based on the website id: '" + _websiteId + "'" );
					}
				}

				if (_virtualDirToChange != String.Empty)
				{
					try
					{
						// set the local var that contains a reference to the virtual directory we are going to change  
						_iisNodeToChangeFrameworkOn = webSite.Children.Find(_virtualDirToChange, IIS_TYPE_IIS_WEB_VIRTUAL_DIR);
						Log(Level.Debug, "The virtual directory '{0}' was found, will change the framework on that virtual directory", _virtualDirToChange);
					}
					catch(Exception ex)
					{
						throw new ApplicationException("Error trying to find the virtual directory '" + _virtualDirToChange + "'.", ex);
					}
				}
				else
				{
					Log(Level.Debug, "No virtual directory found changing the framework version at the website level");
					// set the local var that contains a reference to the website we are going to change  
					_iisNodeToChangeFrameworkOn = webSite;
				}
			}

			 _pathToTool = _dotNetFrameworkInfo.PathToFramework.FullName + "\\aspnet_regiis.exe";

			if (!File.Exists(_pathToTool))
				throw new ApplicationException("The external program '" + _pathToTool + "' could not be found");

			Log(Level.Info, "Calling '{0}' with the following switches: '{1}'", _pathToTool, GetProgramArguments());
			base.ExecuteTask();	
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="server">A DirectoryEntry positioned at the server level e.g: new DirectoryEntry("IIS://localhost/W3SVC")</param>
		/// <param name="name"></param>
		/// <returns>a DirectoryEntry containing the information for the webisite if found, otherwise null</returns>
		private DirectoryEntry GetWebSite(DirectoryEntry server, string name)
		{
			// cant seem to do a: server.Children.Find("web site name", "") at the web server level e.g: IIS://localhost/W3SVC
			DirectoryEntry website = null;

			Log(Level.Debug, "Looking for website '{0}'", name);

			foreach (DirectoryEntry entry in server.Children)
			{
				if (entry.SchemaClassName == IIS_TYPE_IIS_WEB_DIRECTORY || entry.SchemaClassName == IIS_TYPE_IIS_WEB_SERVER || entry.SchemaClassName == IIS_TYPE_IIS_WEB_VIRTUAL_DIR)
				{
					if (entry.Properties != null && entry.Properties.Contains("ServerComment"))
					{
						PropertyValueCollection propertyValueCollection = entry.Properties["ServerComment"];

						if (String.Compare(propertyValueCollection.Value.ToString(), name, true) == 0)
						{
							website = entry;
							Log(Level.Debug, "Website '{0}' found.", name);
							break;
						}
					}
				}
			}

			if (website!= null)
				return website.Children.Find("root", "");
			else
				return null;
		}

		/// <summary>
		/// Builds the command like args for the external program
		/// </summary>
		/// <returns></returns>
		private string GetProgramArguments()
		{
			CommandLineBuilder builder = new CommandLineBuilder();

			if (_iisNodeToChangeFrameworkOn.Properties.Contains("AppRoot"))
			{
				PropertyValueCollection appRootProperty = _iisNodeToChangeFrameworkOn.Properties["AppRoot"];
				string path = Regex.Replace(appRootProperty.Value.ToString(), @"[^W3SVC]*W3SVC", "W3SVC");

				if (_recursivelyApplyChange)
				{
					builder.AppendSwitchIfNotNullOrEmpty("-s ", path);
				}
				else
				{
					Log(Level.Info, "The non recursive flag is being added to aspnet_regiis.exe's switches. Note that we cannot guarantee this will work as aspnet_regiis.exe (this program this task shells out to) doesn’t seem to work as expected. Even thought we pass the correct switches they don’t work from with this task or via the command line. It's possible that the framework change will be recursively applied.");
					builder.AppendSwitchIfNotNullOrEmpty("-sn ", path);
				}
			}
			else
			{
				throw new ApplicationException("The DirectoryEntry containing the reference to the IIS node to update does not have a AppRoot path, are you sure you have set the correct iis node?");
			}

			return builder.GetCommand();
		}
	}
}

