using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.Contrib.Tasks.Web;

using Snak.Core;
using Snak.BuildDrop;
using Snak.Types;
using Snak.Utilities;

namespace Snak.Tasks
{
	/// <summary>
	/// Iterates through all the projects in a solution and deploys them.
	/// 
	/// There are three types of deployment outputs:
	/// -Exe's - Deploy to their own folder, with dependencies
	/// -Webs' - Deploy to their own folder, with dependencies
	/// -DLLs  - Deploy to one common folder for the whole solution (the source for re-use)
	/// 
	/// Other project types are ignored for deployment, e.g. web deployment projects (.wdproj)
	/// </summary>
	[TaskName("deploySolution")]
	public class DeploySolutionTask : TaskContainer
	{
		#region Declarations
		
		private FileInfo _solution;
		private string _solutionConfiguration;
        private string _propertyName = String.Empty;
		private NAnt.VSNet.Types.WebMapCollection _webMaps = new NAnt.VSNet.Types.WebMapCollection();
		private string _artifactsDir = String.Empty;
        private string _buildLabelString = String.Empty;
		private bool _webProjects_SetUpVirtualDirectories = false;
		private string _webProjects_VirtualDirectoriesPrefix = String.Empty;
		private string _webProjects_VirtualDirectoriesSuffix = String.Empty;
        // _virDirSettings needs to be initialised otherwise the nant framework will throw an ex when it tries to add items 
        // (this ultimately gets exposed as a BuildElementCollection)
        private List<VirDirSetting> _virDirSettings = new List<VirDirSetting>();
        private TaskContainer _onProjectDeploy = null;
        private BuildVersion _buildLabel = null;

		#endregion

		/// <summary>
		/// The solution that's to be deployed
		/// </summary>
		[TaskAttribute("solution", Required=true)]
		public FileInfo Solution 
		{
			get { return _solution; }
			set { _solution = value; }
		}

		/// <summary>
		/// The build configuration within the solution.
		/// Project configurations are mapped against this build configuration based on what's
		/// been configured in the solution file. This task only deploys projects for the given build config.
		/// 
		/// Example VS 2005 value: Debug|Any CPU
		/// </summary>
		[TaskAttribute("config", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string SolutionConfiguration 
		{
			get { return _solutionConfiguration; }
			set { _solutionConfiguration = value; }
		}

        /// <summary>
        /// The property prefix that's prefixed to any properties set up by this task
        /// </summary>
        [TaskAttribute("property", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }
		
		/// <summary>
		/// WebMap of URL's to project references. (not used for 2005 projects as the path to the project file is contained in the solution)
		/// </summary>
		[BuildElementCollection("webmap", "map")]
		public NAnt.VSNet.Types.WebMapCollection WebMaps
		{
			get{ return _webMaps; }
			set{ _webMaps = value; }
		}

		/// <summary>
		/// The parent directory where the solution is to be deployed. This task will create an appropriate sub directory based on
		/// the build label and solution configuration.
		/// </summary>
		[TaskAttribute("artifactsDir", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string ArtifactsDir
		{
			get { return _artifactsDir; }
			set { _artifactsDir = value; }
		}

		/// <summary>
		/// The label to apply to this build, e.g 1.1
		/// </summary>
		[TaskAttribute("buildLabel", Required=false)]
		[StringValidator(AllowEmpty=false)]
        public string BuildLabelString
		{
			get{ return _buildLabelString; }
			set { _buildLabelString = value; }
		}

		/// <summary>
		/// If true virtual directories will be set up for each web project
		/// </summary>
		[TaskAttribute("webProjects_SetUpVirtualDirectories", Required=false)]
		public bool WebProjects_SetUpVirtualDirectories
		{
			get{ return _webProjects_SetUpVirtualDirectories; }
			set { _webProjects_SetUpVirtualDirectories = value; }
		}

		/// <summary>
		/// A string to prefix to the virtual directory name.
		/// </summary>
		/// <remarks>Only used if the task attribute webProjects_SetUpVirtualDirectories is set to true</remarks> 
		[TaskAttribute("webProjects_VirtualDirectoriesPrefix", Required=false)]
		public string WebProjects_VirtualDirectoriesPrefix
		{
			get{ return _webProjects_VirtualDirectoriesPrefix; }
			set { _webProjects_VirtualDirectoriesPrefix = value; }
		}

		/// <summary>
		/// A Suffix string for the virtual directory name
		/// </summary>
		/// <remarks>Only used if the task attribute webProjects_SetUpVirtualDirectories is set to true</remarks> 
		[TaskAttribute("webProjects_VirtualDirectoriesSuffix", Required=false)]
		public string WebProjects_VirtualDirectoriesSuffix
		{
			get{ return _webProjects_VirtualDirectoriesSuffix; }
			set { _webProjects_VirtualDirectoriesSuffix = value; }
		}

        /// <summary>
        /// Contains virtual directory setting for web projects in the solution, this is only used 
        /// if WebProjects_SetUpVirtualDirectories is true. Each virDirSetting gets mapped to a 
        /// web project in the solution.
        /// </summary>
        /// <example>
        /// <![CDATA[
        /// <deploySolution [other deploySolution attributes go here]>
        ///     <virDirSettings>                           
        ///         <virDirSetting 
        ///             projectName="myProjectName" 
        ///             clrVersion="v2.0.50727" 
        ///             authanonymous="false" 
        ///             authbasic="true" 
        ///             authntlm="false" 
        ///             enabledirbrowsing="true" 
        ///             defaultdoc="default.aspx" 
        ///             />
        ///         <virDirSetting 
        ///             projectName="myOtherProjectName" 
        ///             clrVersion="v2.0.50727" 
        ///             authanonymous="true" 
        ///             authbasic="false" 
        ///             authntlm="false" 
        ///             enabledirbrowsing="false" 
        ///             defaultdoc="custom.aspx" 
        ///             />
        ///     </virDirSettings>
        /// </deploySolution>
        /// ]]>
        /// </example>
        [BuildElementCollection("virDirSettings", "virDirSetting")]
        public List<VirDirSetting> VirDirSettings
        {
            get { return _virDirSettings; }
            set { _virDirSettings = value; }
        }
        
        /// <summary>
        /// The tasks that get executed on each project deploy, provides a hook for the client to run task on a project by project basis
        /// </summary>
        [BuildElement("onProjectDeploy")]
        public TaskContainer OnProjectDeploy
        {
            get { return _onProjectDeploy; }
            set { _onProjectDeploy = value; }
        }

		protected override void ExecuteTask()
		{
			if (!_solution.Exists)
				throw new ApplicationException("Could not find the solution file " + _solution.FullName + "\\" + _solution.Name);
			
			if (_webMaps.Count > 0)
				foreach (NAnt.VSNet.Types.WebMap item in _webMaps)
					Log(Level.Verbose, "Mapping from {0} to {1}", item.Url, item.Path);
			else
				Log(Level.Verbose, "No webmaps set - web projects in VS 2003 projects will be inferred from solution location if present.");

            _buildLabel = new BuildVersion(_buildLabelString);

			if (!Directory.Exists(_artifactsDir))
			{
				try
				{
					Log(Level.Verbose, "Could not find artefacts directory '" + _artifactsDir + "'. Trying to create now.");

					Directory.CreateDirectory(_artifactsDir);
				}
				catch
				{
					Log(Level.Error, "Could not create artefacts directory " + _artifactsDir + ".");
					throw;
				}
			}

            ISolutionInfo solution = SolutionFactory.GetSolution(Solution, new NAntLoggingProxy(Log).Log);
			solution.WebMaps.AddRange(this.WebMaps);

			ProjectPropertiesTask projectPropertiesTask = new ProjectPropertiesTask();
			TaskUtils.CopySettingsFrom(this).To(projectPropertiesTask);

            IDropNamingStrategy packageNamingStrategy = new CommonDirectoryDropNamingStrategy(this.Solution.Name, new DirectoryInfo(_artifactsDir), _buildLabel, _solutionConfiguration);
            
            BuildDrop.BuildDrop buildDropLocations = new BuildDrop.BuildDrop(packageNamingStrategy, solution, _solutionConfiguration);

            buildDropLocations.CreateDropDirectores();

            if (_onProjectDeploy == null)
                Log(Level.Verbose, "No nested TaskContainer 'onProjectDeploy' detected");

			try
			{
				foreach (IProjectInfo project in solution.GetProjectsFor(SolutionConfiguration)) 
				{
					// The web application projects actually seems to contain the information that needs to deployed (i.e. similar info as that we get from the project file), 
					// should I rely on the WebDeploymentProject's or the actual web application projects themselves to deploy from... we can only pick one otherwise we deploy it twice
					// for now I'll just rely on the web application projects themselves
					if (!project.IsWebDeploymentProject) 
					{
                        DeployProject(project, buildDropLocations.DropLocationsByProjectName[project.Name].DropToOwnDirectory, buildDropLocations);

                        // set up the project specific nant properties so they are available from within the _onProjectDeploy container
                        // this way any sub tasks that are running on a project by project basis can use them
                        SetProjectSpecificNantProperties(project, buildDropLocations);

                        // execute the _onProjectDeploy container allowing clients to do custom actions on deploy of each project 
                        if (_onProjectDeploy != null)
                        {
                            Log(Level.Verbose, "Nested TaskContainer 'onProjectDeploy' detected, running tasks in that container for project '{0}'", project.Name);
                            _onProjectDeploy.Execute();
                        }
					}
				}

				// Copy the files that we just built in the main assemblies directory into the '_Current' assemblies  directory 
                buildDropLocations.CommonAssembliesDropLocation.PublishDropToCurrentDir();
			} 
			catch
			{
				Log(Level.Error, "Error deploying a project.");
				throw;
			}	

			base.ExecuteTask();
		}

		/// <summary>
		/// Ensures deployment directories exist then calls out to DeployProjectTask to handle the project deployment
		/// </summary>
		/// <param name="project"></param>
		/// <param name="deployToOwnDirectory"></param>
		/// <param name="projectDeploymentDir"></param>
		/// <param name="projectDeploymentDirCurrent"></param>
        internal void DeployProject(IProjectInfo project, bool deployToOwnDirectory, BuildDrop.BuildDrop buildDropLocations)
		{
            DropLocation dropLocation = buildDropLocations.DropLocationsByProjectName[project.Name];
			DeployProjectTask deployProjectTask = new DeployProjectTask();
            VirDirSetting thisProjectsVirDirSetting = null;

			// A note about calling tasks from within other tasks
			// http://www.paraesthesia.com/blog/comments.php?id=1000_0_1_0_C
			// Interestingly enough, this (calling tasks from within tasks) isn't as straightforward as you might think, and NAnt documentation on this is, 
			// well, light. You can't just create the task object and call it, you actually have to give the created task some context about 
			// the environment it's working in. You do this by calling the CopyTo method on the task object. By and large, the way it looks is this:
			this.CopyTo(deployProjectTask);
			deployProjectTask.Config = this._solutionConfiguration;
			deployProjectTask.ProjectInfo = project.ProjectFile;
            deployProjectTask.TargetDir = dropLocation.DropDir;
			
			deployProjectTask.Execute();


            //
            // we need to publish the dlls that go to their own folder 
            //
			if (deployToOwnDirectory)
			{
				Log(Level.Verbose, "Updating 'Current' deployment directory for " + project.Name);
				// copy the files that we just built in the build directory into the 'current' build directory 
                dropLocation.PublishDropToCurrentDir();
			}

			if (project.IsWebProject && _webProjects_SetUpVirtualDirectories)
			{
                //
                // check that we have the correct virtual directory settings for this project, these should have been passed in via the nant script
                //
                if (_virDirSettings == null)
                    throw new BuildException("You have specified to created virtual directories for web projects within the solution you are deploying, however you have not specified a virDirSettings element within the deploySolution task element. This is required to configure the virtual directories.");

			    thisProjectsVirDirSetting = GetVDirSettingForProjectOrThrow(project);
                string inferredVDirName = _webProjects_VirtualDirectoriesPrefix + project.Name + _webProjects_VirtualDirectoriesSuffix;
                string vDirName = String.IsNullOrEmpty(thisProjectsVirDirSetting.VirtualDirectory)
                                      ? inferredVDirName
                                      : thisProjectsVirDirSetting.VirtualDirectory;

                //
                // ok, we've confirmed we have the correct virtual directory settings, now we try to delete any previous virtual dir and then  
                // we create the new one
                // 
			    
				Log(Level.Verbose, "Deleting (if exists) old virtual directory '" + vDirName + "' for web project:" + project.Name);

				DeleteVirtualDirectory deleteVirtualDirectory = new DeleteVirtualDirectory();
				// copy some context from this task to the deleteVirtualDirectory task
				this.CopyTo(deleteVirtualDirectory);
				deleteVirtualDirectory.VirtualDirectory = vDirName;
				deleteVirtualDirectory.FailOnError = false;
				deleteVirtualDirectory.Execute();

				Log(Level.Verbose, "Creating virtual directory '{0}' for web project:{1}", vDirName, project.Name);

				CreateVirtualDirectoryWithSpecificAspNetVersionTask createVDirTask = new CreateVirtualDirectoryWithSpecificAspNetVersionTask();
				this.CopyTo(createVDirTask);
				
                createVDirTask.FailOnError = false;
				createVDirTask.DirPath = dropLocation.DropDir;

				createVDirTask.DotNetFrameworkVersion = thisProjectsVirDirSetting.DotNetFrameworkVersion;
                createVDirTask.AuthBasic = thisProjectsVirDirSetting.AuthBasic;
                createVDirTask.AuthNtlm = thisProjectsVirDirSetting.AuthNtlm;
                createVDirTask.AuthAnonymous = thisProjectsVirDirSetting.AuthAnonymous;
                createVDirTask.EnableDirBrowsing = thisProjectsVirDirSetting.EnableDirBrowsing;
                createVDirTask.DefaultDoc = thisProjectsVirDirSetting.DefaultDoc;
                if (!String.IsNullOrEmpty(thisProjectsVirDirSetting.Server))
                    createVDirTask.Server = thisProjectsVirDirSetting.Server;
    	        createVDirTask.VirtualDirectory = vDirName;
			    createVDirTask.AccessScript = true;
				createVDirTask.Execute();
			}
		}

	    private VirDirSetting GetVDirSettingForProjectOrThrow(IProjectInfo project)
	    {
	        foreach (VirDirSetting virDirSetting in this._virDirSettings)
	            if (String.Compare(virDirSetting.ProjectName, project.Name, true) == 0)
	                return virDirSetting;

            throw new BuildException("Could not find a suitable virDirSetting match for the project '" + project.Name + "', please check the virDirSettings element beneath the deploySolution task element.");
        }

	    /// <summary>
        /// 
        /// </summary>
        private void SetProjectSpecificNantProperties(IProjectInfo project, BuildDrop.BuildDrop buildDropLocations)
        {
            DropLocation dropLocation = buildDropLocations.DropLocationsByProjectName[project.Name];

            // we just add all the properties of IProjectInfo using the ProjectPropertiesTask
            ProjectPropertiesTask projectPropertiesTask = new ProjectPropertiesTask();
            this.CopyTo(projectPropertiesTask);
            projectPropertiesTask.SetupProperties(project, _propertyName + ".currentProject");

            // add a few additional properties based on the drop location
            Properties[_propertyName + ".currentProject." + "dropDirCurrent"] = dropLocation.DropDirCurrent.FullName;
            Properties[_propertyName + ".currentProject." + "dropDirLabeled"] = dropLocation.DropDir.FullName;
            Properties[_propertyName + ".currentProject." + "dropDirPrevious"] = (dropLocation.DropDirPrevious != null) ? dropLocation.DropDirPrevious.FullName : String.Empty;
            Properties[_propertyName + ".currentProject." + "dropIsForSingleApplicaiton"] = dropLocation.DropToOwnDirectory.ToString();
        }

		private string FormatBuildConfigForSafeDirName(string configString)
		{
            return configString.Replace("|", "-").Replace(" ", "");
		}
	}
}
