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
using System.Collections;

namespace Snak.Tasks
{
	/// <summary>
	/// Iterates through all the projects in a solution and packages them.
	/// 
    /// There are three types of packaging outputs:
	/// -Exe's - Deploy to their own folder, with dependencies
	/// -Webs' - Deploy to their own folder, with dependencies
	/// -DLLs  - Deploy to one common folder for the whole solution (the source for re-use)
	/// 
    /// Other project types are ignored for packaging, e.g. web deployment projects (.wdproj)
	/// </summary>
    [TaskName("packageOutputs")]
	public class PackageOutputsTask : TaskContainer
	{
		#region Declarations

        private BuildVersion _buildLabel = null;
		private FileInfo _solution;
		private string _solutionConfiguration;
        private string _propertyName = String.Empty;
		private string _artifactsDir = String.Empty;
        private string _buildLabelString = String.Empty;
        private NAnt.VSNet.Types.WebMapCollection _webMaps = new NAnt.VSNet.Types.WebMapCollection();
        private TaskContainer _onBeforeProjectPackage = null;
        private TaskContainer _onPostProjectPackage = null;
        private List<Package> _additionalPackages = new List<Package>();
        private bool _failOnEmpty = true;

        private readonly string _currentProjectPropertySuffix = ".currentProject";
		#endregion
	
		/// <summary>
		/// The solution that's to be packaged
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
		/// been configured in the solution file. This task only packages projects for the given build config.
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
		/// The label to apply to this build, e.g 1.1.0.123
		/// </summary>
		[TaskAttribute("buildLabel", Required=false)]
		[StringValidator(AllowEmpty=false)]
        public string BuildLabelString
		{
			get{ return _buildLabelString; }
			set { _buildLabelString = value; }
		}

        /// <summary>
        /// WebMap of URL's to project references. (not used for 2005 projects as the path to the project file is contained in the solution)
        /// </summary>
        [BuildElementCollection("webmap", "map")]
        public NAnt.VSNet.Types.WebMapCollection WebMaps
        {
            get { return _webMaps; }
            set { _webMaps = value; }
        }

        /// <summary>
        /// Additional items to package tasks that get paexecuted after each project is deploy, provides a hook for the client to run task on a project by project basis
        /// </summary>
        [BuildElementCollection("additionalPackage", "package")]
        public List<Package> AdditionalPackages
        {
            get { return _additionalPackages; }
            set { _additionalPackages = value; }
        }

        /// <summary>
        /// Determines whether an error or a warning is generated
        /// if there are no projects active for the solution configuration supplied
        /// </summary>
        [TaskAttribute("failonempty", Required = false)]
        public bool FailOnEmpty
        {
            get { return _failOnEmpty; }
            set { _failOnEmpty = value; }
        }

        /// <summary>
        /// The tasks that get executed after before each project is packaged, provides a hook for the client to run task on a project by project basis
        /// </summary>
        [BuildElement("onBeforeProjectPackage")]
        public TaskContainer OnBeforeProjectPackage
        {
            get { return _onBeforeProjectPackage; }
            set { _onBeforeProjectPackage = value; }
        }

        /// <summary>
        /// The tasks that get executed after each project is packaged, provides a hook for the client to run task on a project by project basis
        /// </summary>
        [BuildElement("onPostProjectPackage")]
        public TaskContainer OnPostProjectPackage
        {
            get { return _onPostProjectPackage; }
            set { _onPostProjectPackage = value; }
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

            IDropNamingStrategy dropNamingStrategy = new LabelDirectoryDropNamingStrategy(this.Solution.Name, new DirectoryInfo(_artifactsDir), _buildLabel, _solutionConfiguration);

            BuildDrop.BuildDrop buildDropLocations = new BuildDrop.BuildDrop(dropNamingStrategy, solution, _solutionConfiguration);

            buildDropLocations.CreateDropDirectores();

            if (_additionalPackages == null || (this._additionalPackages != null && this._additionalPackages.Count == 0))
                Log(Level.Verbose, "No additional package loaded, will just package the solution output.");

            try
            {
                IProjectInfo[] projects = solution.GetProjectsFor(SolutionConfiguration);
                if (projects.Length == 0)
                {
                    string message = string.Format("No projects retrieved for {0} [{1}]", Solution.FullName, SolutionConfiguration);
                    if (FailOnEmpty)
                        throw new BuildException(message, Location);
                    else
                        Log(Level.Warning, message);
                }
                else
                {
                    foreach (IProjectInfo project in projects)
                    {
                        // The web application projects actually seems to contain the information that needs to deployed (i.e. similar info as that we get from the project file), 
                        // should I rely on the WebDeploymentProject's or the actual web application projects themselves to deploy from... we can only pick one otherwise we deploy it twice
                        // for now I'll just rely on the web application projects themselves
                        if (!project.IsWebDeploymentProject)
                        {
                            Log(Level.Verbose, "Packaging project " + project.ProjectName);

                            // set additional nant properties that also reflect the output directory too
                            // this way any sub tasks that are running on a project by project basis can use them
                            SetProjectSpecificNantProperties(project);

                            if (_onBeforeProjectPackage != null)
                                _onBeforeProjectPackage.Execute();

                            DropLocation dropLocation = buildDropLocations.DropLocationsByProjectName[project.Name];
                            PackageProject(project, dropLocation);

                            // set additional nant properties that also reflect the output directory too
                            // this way any sub tasks that are running on a project by project basis can use them
                            SetDropDirSpecificNantProperties(dropLocation);

                            if (_onPostProjectPackage != null)
                                _onPostProjectPackage.Execute();
                        }
                    }
                }

                // Need to clear the properties down again, or listeners in the next loop
                // might well do all kinds of wierd stuff
                ClearProjectProperties();

                foreach (Package package in this._additionalPackages)
                {
                    Log(Level.Verbose, "Packaging additional package " + package.PackageName);
                    // TODO: Need to setup pseudo-properties here
                    // that look a bit like the project properties
                    // and also make the output structure available to the onPostProjectPackage clients
                    // Actually on second thoughts maybe this doesn't make sense
                    SetupProperty("projectName", package.Name);

                    if (_onBeforeProjectPackage != null)
                        _onBeforeProjectPackage.Execute();

                    package.PackageContents(dropNamingStrategy);

                    if (_onPostProjectPackage != null)
                        _onPostProjectPackage.Execute();
                }

                // Copy the files that we just built in the main assemblies directory into the '_Current' assemblies  directory 
                buildDropLocations.CommonAssembliesDropLocation.PublishDropToCurrentDir();
            }
            catch
            {
                Log(Level.Error, "Error packaging a project.");
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
        internal void PackageProject(IProjectInfo project, DropLocation dropLocation)
        {
            DeployProjectTask deployProjectTask = new DeployProjectTask();
    
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

            if (dropLocation.DropToOwnDirectory)
            {
                Log(Level.Verbose, "Updating 'Current' deployment directory for " + project.Name);
                // copy the files that we just built in the build directory into the 'current' build directory 
                dropLocation.PublishDropToCurrentDir();
            }
        }

        /// <summary>
        /// Assigns properties for the current project being deployed to NAnt properties
        /// </summary>
        private void SetProjectSpecificNantProperties(IProjectInfo project)
        {
            // we just add all the properties of IProjectInfo using the ProjectPropertiesTask
            ProjectPropertiesTask projectPropertiesTask = new ProjectPropertiesTask();
            this.CopyTo(projectPropertiesTask);
            projectPropertiesTask.SetupProperties(project, _propertyName + _currentProjectPropertySuffix);
        }

        /// <summary>
        /// Assigns properties for the current <see cref="DropLocation"/> to NAnt properties
        /// </summary>
        private void SetDropDirSpecificNantProperties(DropLocation dropLocation)
        {
            // add a few additional properties based on the drop location
            SetupProperty("currentProject.dropDirCurrent", dropLocation.DropDirCurrent.FullName);
            SetupProperty("currentProject.dropDirLabeled", dropLocation.DropDir.FullName);
            SetupProperty("currentProject.dropDirPrevious", (dropLocation.DropDirPrevious != null) ? dropLocation.DropDirPrevious.FullName : String.Empty);
            SetupProperty("currentProject.dropIsForSingleApplication", dropLocation.DropToOwnDirectory);
        }

        private void ClearProjectProperties()
        {
            // No indexer on Properties, so the double loop is required
            // so we don't run into the old 'cant delete from collection when enumerating it' issue
            
            List<string> propertiesToRemove = new List<string>();
            foreach(DictionaryEntry item in Properties)
                if (item.Key.ToString().StartsWith(_propertyName + _currentProjectPropertySuffix))
                    propertiesToRemove.Add(item.Key.ToString());

            foreach(string name in propertiesToRemove)
                Properties.Remove(name);
        }

        private void SetupProperty(string name, object value)
        {
            name = PropertyName + "." + TaskUtils.LowerFirst(name);
            Properties[name] = TaskUtils.SafeToString(value);
            Log(Level.Verbose, "Setting {0} to {1}", name, Properties[name]);
        }

        //private string FormatBuildConfigForSafeDirName(string configString)
        //{
        //    return configString.Replace("|", "-").Replace(" ", "");
        //}
	}
}
