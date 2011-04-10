using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using Snak.Core;
using Snak.Tasks;

namespace Snak.BuildDrop
{
    /// <summary>
    /// Provides a single location for the management of directories used when SNAK is dropping the build.
    /// 
    /// Drop locations for the projects depends upon the project type (e.g. web project, class library, this information is stored in IProjectInfo.OutputType)
    /// each solution gets a main assemblies folder, web, exe's and winexe's get their own folder, class libraries just get added to the main assemblies folder. These folders 
    /// get a Suffixed with the build label. In addition to these folders we also have a _Current folder. This is basically a copy of what’s in the 
    /// folder with the highest build label. Check out the test for example folder names.
    ///
    /// You can either use this class when building and then droping the build or if you want to pick up a build and do further work (e.g. deployment). 
    /// </summary>
    public class BuildDrop
    {
        private string _solutionName = String.Empty;
        //private DirectoryInfo _artifactsDirectory = null;
        private DropLocation _commonAssembliesDropLocation = null;
        private Dictionary<string, DropLocation> _dropLocationsByProjectName = new Dictionary<string, DropLocation>();
        private ISolutionInfo _solution = null;
        //private BuildVersion _buildLabel = null;
        private IDropNamingStrategy _dropNamingStrategy;

        //private DirectoryInfo _labeledDropDir;

        //internal DirectoryInfo LabeledDropDir
        //{
        //    get { return _labeledDropDir; }
        //    private set { _labeledDropDir = value; }
        //}

        //private DirectoryInfo _labeledDropPrevious;

        //internal DirectoryInfo LabeledDropDirPrevious
        //{
        //    get { return _labeledDropPrevious; }
        //    private set { _labeledDropPrevious = value; }
        //}

        //private DirectoryInfo _currentDropDir;

        //internal DirectoryInfo CurrentDropDir
        //{
        //    get { return _currentDropDir; }
        //    private set { _currentDropDir = value; }
        //}

        /// <summary>
        /// Common drop location for projects that don’t get dropped into their own folder.
        /// </summary>
        public DropLocation CommonAssembliesDropLocation
        {
            get { return _commonAssembliesDropLocation; }
        }

        /// <summary>
        /// Drop locations by project name
        /// </summary>
        /// <remarks>
        /// if the project shares a drop location (e.g. the project type is not a web app, windows app, service app but rather a class library)
        /// then its reference in this property will be then same as the property CommonAssembliesDropLocation
        /// </remarks>
        public Dictionary<string, DropLocation> DropLocationsByProjectName
        {
            get { return (this._dropLocationsByProjectName); }   
        }

        /// <summary>
        /// provides a single point of reference for drop locations for solutions and their projects
        /// </summary>
        /// <param name="artifactsDirDirectory">The containing directory which the build will be dropped into (sub directories will be created with the appropriate name)</param>
        /// <param name="solution">The solution which contains the projects to drop</param>
        /// <param name="solutionConfiguration">The build configuration (e.g. .Net 1.1: "Debug", .Net 2.0: "Debug|Any Cpu" ) </param>
        /// <param name="buildLabel">The build label information</param>
        public BuildDrop(IDropNamingStrategy dropNamingStrategy, ISolutionInfo solution, string solutionConfiguration)
        {
            if (dropNamingStrategy == null)
                throw new ArgumentNullException("dropNamingStrategy");
            if (solution == null)
                throw new ArgumentNullException("solution");
            if (solutionConfiguration == null || solutionConfiguration == String.Empty)
                throw new ArgumentException("the argument solutionConfiguration cannot be null or empty", "solutionConfiguration");
            
            _dropNamingStrategy = dropNamingStrategy;
            _solution = solution;
            
            DropLocation dropLocation = null;

            _commonAssembliesDropLocation = DropLocation.CreateForCommonDropLocation(_dropNamingStrategy);

            foreach (IProjectInfo project in _solution.GetProjectsFor(solutionConfiguration))
            {
                bool deployToOwnDirectory = (project.IsWebProject || project.OutputType == ProjectOutputType.Exe || project.OutputType == ProjectOutputType.WinExe);

                dropLocation =  DropLocation.CreateForPackage(_dropNamingStrategy, project.Name, deployToOwnDirectory, true);

                if (this._dropLocationsByProjectName.ContainsKey(project.Name))
                    throw new ApplicationException("While adding a unique DropLocation for each project the project with name '" + project.Name + "' was already added. There should only be one project with this name associated with the active solution configuration (which is currently '" + solutionConfiguration +"').");
                else
                    this._dropLocationsByProjectName.Add(project.Name, dropLocation);
            }
        }

        /// <summary>
        /// Creates all the drop directories for solution passed to the constructor
        /// </summary>
        public void CreateDropDirectores()
        {
            // first delete the directories if they exist
            _dropNamingStrategy.DeleteDropDirectories();

            // create the root level drop
            _dropNamingStrategy.CreateDropDirectories();
            // create the common drop for all the packages
            _dropNamingStrategy.CreatePackagesCommonDirectories();

            // for each project we now create its drop directory
            foreach (KeyValuePair<string, DropLocation> keyValuePair in _dropLocationsByProjectName)
            {
                _dropNamingStrategy.CreatePackageDirectories(keyValuePair.Key, keyValuePair.Value.DropToOwnDirectory, true);
            }
        }

        /// <summary>
        /// Deletes all the drop directories for solution passed to the constructor
        /// </summary>
        public void DeleteDropDirectores()
        {
            // delete the root drop directories
            _dropNamingStrategy.DeleteDropDirectories();
            // delete the common package drop directory
            _dropNamingStrategy.DeletePackagesCommonDirectories();

            // for each project we now delete its drop directory
            foreach (KeyValuePair<string, DropLocation> keyValuePair in _dropLocationsByProjectName)
            {
                _dropNamingStrategy.DeletePackageDirectories(keyValuePair.Key, keyValuePair.Value.DropToOwnDirectory, true);
            }
        }
    }
}
