using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;
using NAnt.Core;
using NAnt.Core.Tasks;
using NAnt.Core.Types;

using Snak;
using Snak.Utilities;

namespace Snak.BuildDrop
{
    /// <summary>
    /// Defines a drop location for a project or solution.
    /// 
    /// There are 2 drop directories for any given item (either a project or solution), firstly a drop directory that’s suffix is the build label.
    /// Secondly there is a drop directory that has suffix of _Current, this will always contain the latest build. This provides a well known 
    /// point of reference build outputs (useful if other application need to pull in the latest dependencies).
    /// </summary>
    public class DropLocation
    {
        public static DropLocation CreateForCommonDropLocation(IDropNamingStrategy dropNamingStrategy)
        {
            return new DropLocation(dropNamingStrategy);
        }

        public static DropLocation CreateForPackage(IDropNamingStrategy dropNamingStrategy, string projectName, bool dropToOwnDirectory, bool dropIsPartOfSolution)
        {
            return new DropLocation(dropNamingStrategy, projectName, dropToOwnDirectory, dropIsPartOfSolution);
        }

        private IDropNamingStrategy _dropNamingStrategy = null;
        private string _projectName = String.Empty;

        private DirectoryInfo _dropDir = null;
        private DirectoryInfo _dropDirCurrent = null;
        private DirectoryInfo _dropDirPrevious = null;
        private bool _dropToOwnDirectory = true;

        /// <summary>
        /// Drop directory that gets a suffix which is the build label
        /// </summary>
        public DirectoryInfo DropDir
        {
            get { return _dropDir; }
        }

        /// <summary>
        /// The drop directory that contains the build output from the last build
        /// </summary>
        public DirectoryInfo DropDirCurrent
        {
            get { return _dropDirCurrent; }
        }

        /// <summary>
        /// The drop directory that contains the build output from the build before last
        /// </summary>
        public DirectoryInfo DropDirPrevious
        {
            get { return _dropDirPrevious; }
        }

        /// <summary>
        /// defines if this DropLocation is for a single application as apposed to a shared drop location for multiple projects
        /// </summary>
        public bool DropToOwnDirectory
        {
            get { return _dropToOwnDirectory; }
        }

        private DropLocation(IDropNamingStrategy dropNamingStrategy) 
            : this
            (
                dropNamingStrategy,
                "CommonDropLocation", 
                false,
                dropNamingStrategy.PackagesCommonDropDirectory,
                dropNamingStrategy.PackagesCommonDropDirectoryCurrent,
                dropNamingStrategy.PackagesCommonDropDirectoryLast
            )
        { 
        
        }

        private DropLocation(IDropNamingStrategy dropNamingStrategy, string projectName, bool dropToOwnDirectory, bool dropIsPartOfSolution)
            : this
            (
                dropNamingStrategy, projectName, dropToOwnDirectory,
                dropNamingStrategy.GetPackageDropDirectory(projectName, dropToOwnDirectory, dropIsPartOfSolution),
                dropNamingStrategy.GetPackageDropDirectoryCurrent(projectName, dropToOwnDirectory, dropIsPartOfSolution),
                dropNamingStrategy.GetPackageLastDropDirectoryLast(projectName, dropToOwnDirectory, dropIsPartOfSolution)
            )
        {

        }

        /// <summary>
        /// provides reference to the drop locations for a build item (e.g. a project or a solution output)
        /// </summary>
        /// <param name="dropDir"></param>
        /// <param name="dropDirCurrent"></param>
        /// <param name="dropIsForSingleApplicaiton"></param>
        private DropLocation(
            IDropNamingStrategy dropNamingStrategy,
            string projectName,
            bool dropToOwnDirectory,
            DirectoryInfo dropDir,
            DirectoryInfo dropDirCurrent,
            DirectoryInfo dropDirPrevious
            )
        {
            if (dropNamingStrategy == null)
                throw new ArgumentNullException("dropNamingStrategy");

            if (String.IsNullOrEmpty(projectName))
                throw new ArgumentException("The projectName cannot be null or empty.", "projectName");

            this._projectName = projectName;
            this._dropNamingStrategy = dropNamingStrategy;
            this._dropToOwnDirectory = dropToOwnDirectory;

            this._dropDir = dropDir;
            this._dropDirCurrent = dropDirCurrent;
            this._dropDirPrevious = dropDirPrevious;
        }


        /// <summary>
        /// copies the files in DropDirlabeled to DropDirCurrent
        /// </summary>
        public void PublishDropToCurrentDir()
        {
            try
            {
                DirectoryInfo currentBuildDir = null;
                string newFile = String.Empty;

                FileSet fileSet = new FileSet();
                fileSet.BaseDirectory = _dropDir;
                fileSet.Includes.Add(@"**\*");

                foreach (string fileName in fileSet.FileNames)
                {
                    newFile = fileName.Replace(_dropDir.FullName, _dropDirCurrent.FullName);
                    currentBuildDir = new DirectoryInfo(Path.GetDirectoryName(newFile));

                    if (!currentBuildDir.Exists)
                        currentBuildDir.Create();

                    FileInfo file = new FileInfo(fileName);
                    file.CopyTo(newFile, true);
                }
            }
            catch
            {
                // TODO: logging.
                // Log(Level.Error, "Error publishing files in the directory '{0}' to '{1}'", dirToCopyFrom.FullName, dirToPublishTo.FullName);
                throw;
            }
        }

        //internal void CreateDropDirectories()
        //{
        //    if (this._projectName == COMMON_DROP_DIR_KEY)
        //        _dropNamingStrategy.CreateDropDirectories();
        //    else
        //        _dropNamingStrategy.CreatePackageDirectories(_projectName, this._dropToOwnDirectory);
        //}

        //internal void DeleteDropDirectories()
        //{
        //    if (this._projectName == COMMON_DROP_DIR_KEY)
        //        _dropNamingStrategy.DeleteDropDirectories();
        //    else
        //        _dropNamingStrategy.DeletePackageDirectories(_projectName, this._dropToOwnDirectory);
        //}
    }
}
