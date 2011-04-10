using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Snak.Core;
using Snak.Utilities;

namespace Snak.BuildDrop
{
    public abstract class AbstractDropNamingStrategy : IDropNamingStrategy
    {
        private BuildVersion _buildVersion = null;
        private string _solutionName = String.Empty;
        private string _solutionConfiguration = String.Empty;
        private DirectoryInfo _artefactDirectory;

        protected enum DropDirectoryCreateOrDeleteMode
        {
            Create,
            Create_IfExistsDeleteFirst,
            Delete
        }

        #region protected props

        protected BuildVersion BuildVersion
        {
            get { return _buildVersion; }
        }

        protected string SolutionName
        {
            get { return _solutionName; }
        }

        protected string SolutionConfiguration
        {
            get { return _solutionConfiguration; }
        }

        protected DirectoryInfo ArtefactDirectory
        {
            get { return _artefactDirectory; }
        }

        #endregion

        public AbstractDropNamingStrategy(string solutionName, DirectoryInfo artefactDirectory, BuildVersion buildVersion, string solutionConfiguration)
        {
            this._solutionName = solutionName;
            this._artefactDirectory = artefactDirectory;
            this._buildVersion = buildVersion;
            this._solutionConfiguration = solutionConfiguration;
        }

        #region IDropNamingStrategy members

        private DirectoryInfo _dropDirectory;
        private DirectoryInfo _dropDirectoryCurrent;
        private DirectoryInfo _dropDirectoryLast;
        private DirectoryInfo _packagesCommonDropDirectory;
        private DirectoryInfo _packagesCommonDropDirectoryCurrent;
        private DirectoryInfo _packagesCommonDropDirectoryLast;

        public virtual DirectoryInfo DropDirectory
        {
            get
            {
                return _dropDirectory;
            }
            protected set
            {
                this._dropDirectory = value;
            }
        }

        public virtual DirectoryInfo DropDirectoryCurrent
        {
            get
            {
                return _dropDirectoryCurrent;
            }
            protected set
            {
                this._dropDirectoryCurrent = value;
            }
        }

        public virtual DirectoryInfo DropDirectoryLast
        {
            get
            {
                return _dropDirectoryLast;
            }
            protected set
            {
                this._dropDirectoryLast = value;
            }
        }

        public virtual DirectoryInfo PackagesCommonDropDirectory
        {
            get
            {
                return _packagesCommonDropDirectory;
            }
            protected set
            {
                this._packagesCommonDropDirectory = value;
            }
        }

        public virtual DirectoryInfo PackagesCommonDropDirectoryCurrent
        {
            get
            {
                return _packagesCommonDropDirectoryCurrent;
            }
            protected set
            {
                this._packagesCommonDropDirectoryCurrent = value;
            }
        }

        public virtual DirectoryInfo PackagesCommonDropDirectoryLast
        {
            get
            {
                return _packagesCommonDropDirectoryLast;
            }
            protected set
            {
                this._packagesCommonDropDirectoryLast = value;
            }
        }

        public abstract DirectoryInfo GetPackageDropDirectory(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution);
        public abstract DirectoryInfo GetPackageDropDirectoryCurrent(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution);
        public abstract DirectoryInfo GetPackageLastDropDirectoryLast(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution);

        public virtual void CreateDropDirectories()
        {
            CreateOrDeleteDirectories(
                this.DropDirectory,
                this.DropDirectoryCurrent,
                DropDirectoryCreateOrDeleteMode.Create
            );
        }

        public virtual void CreatePackageDirectories(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution)
        {
            CreateOrDeleteDirectories(
                this.GetPackageDropDirectory(packageName, dropToOwnDirectory, packageIsPartOfSolution),
                this.GetPackageDropDirectoryCurrent(packageName, dropToOwnDirectory, packageIsPartOfSolution),
                DropDirectoryCreateOrDeleteMode.Create
            );
        }

        public virtual void CreatePackagesCommonDirectories()
        {
            CreateOrDeleteDirectories(
                this.PackagesCommonDropDirectory,
                this.PackagesCommonDropDirectoryCurrent,
                DropDirectoryCreateOrDeleteMode.Create
            );
        }

        public virtual void DeleteDropDirectories()
        {
            CreateOrDeleteDirectories(
                this.DropDirectory,
                this.DropDirectoryCurrent,
                DropDirectoryCreateOrDeleteMode.Delete
            );
        }

        public virtual void DeletePackageDirectories(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution)
        {
            CreateOrDeleteDirectories(
                this.GetPackageDropDirectory(packageName, dropToOwnDirectory, packageIsPartOfSolution),
                this.GetPackageDropDirectoryCurrent(packageName, dropToOwnDirectory, packageIsPartOfSolution),
                DropDirectoryCreateOrDeleteMode.Delete
            );
        }

        public virtual void DeletePackagesCommonDirectories()
        {
            CreateOrDeleteDirectories(
                this.PackagesCommonDropDirectory,
                this.PackagesCommonDropDirectoryCurrent,
                DropDirectoryCreateOrDeleteMode.Delete
            );
        }

        #endregion

        #region private and protected methods

        protected delegate string GetDropDirectoryPath();

        protected DirectoryInfo GetDropDirectory(string packageName, bool dropToOwnDirectory, GetDropDirectoryPath dropDirectoryPath)
        {
            DirectoryInfo dropLocation = null;

            if (dropToOwnDirectory)
            {
                string path = dropDirectoryPath();

                dropLocation = new DirectoryInfo(path);
            }
            else
                dropLocation = this._packagesCommonDropDirectory;

            return dropLocation;
        }

        protected DirectoryInfo GetDropDirectoryCurrent(string packageName, bool dropToOwnDirectory, GetDropDirectoryPath dropDirectoryPath)
        {
            DirectoryInfo dropLocation = null;

            if (dropToOwnDirectory)
            {
                string path = dropDirectoryPath();

                dropLocation = new DirectoryInfo(path);
            }
            else
                dropLocation = this._packagesCommonDropDirectoryCurrent;

            return dropLocation;
        }

        protected DirectoryInfo TryGetDropDirectoryElseGetNull(string packageName, bool dropToOwnDirectory, GetDropDirectoryPath dropDirectoryPath)
        {
            DirectoryInfo dropLocation = null;

            if (dropToOwnDirectory)
            {
                string path = dropDirectoryPath();
                
                if (!String.IsNullOrEmpty(path))
                {
                    DirectoryInfo expectedDirectoryInfo = new DirectoryInfo(path);
                    dropLocation = (expectedDirectoryInfo.Exists) ? expectedDirectoryInfo : null;
                }
            }
            else
                dropLocation = this._packagesCommonDropDirectoryLast;

            return dropLocation;
        }

        public static string MakeSafeForPath(string configString)
        {
            configString = configString.Replace("|", "-").Replace(" ", "");
            foreach(char badChar in Path.GetInvalidPathChars())
                configString = configString.Replace(badChar.ToString(), String.Empty);
            return configString;
        }

        protected void CreateOrDeleteDirectories(DirectoryInfo labeledDir, DirectoryInfo commonDir, DropDirectoryCreateOrDeleteMode dropDirectoryCreateOrDeleteMode)
        {
            if (dropDirectoryCreateOrDeleteMode == DropDirectoryCreateOrDeleteMode.Delete)
            {
                if (labeledDir != null && labeledDir.Exists)
                    DeleteDropDirectory(labeledDir);

                if (commonDir != null && commonDir.Exists)
                    DeleteDropDirectory(commonDir);
            }
            else
            {
                CreateDirectory(labeledDir, dropDirectoryCreateOrDeleteMode);
                CreateDirectory(commonDir, dropDirectoryCreateOrDeleteMode);
            }
        }

        private void CreateDirectory(DirectoryInfo directoryToCreate, DropDirectoryCreateOrDeleteMode dropDirectoryCreateOrDeleteMode)
        {
            if (directoryToCreate != null)
            {
                if (directoryToCreate.Exists && (dropDirectoryCreateOrDeleteMode == DropDirectoryCreateOrDeleteMode.Create_IfExistsDeleteFirst))
                {
                    DeleteDropDirectory(directoryToCreate);
                }

                if (!directoryToCreate.Exists)
                    directoryToCreate.Create();
            }
        }

        private void DeleteDropDirectory(DirectoryInfo directoryToDelete)
        {
            // we used to swallow this (with the intent to log)
            // hummm. should the build die if we cant delete a location, probably
            // as there could be crap in there thats may need to get overridden 
            TaskUtils.RecursivelyApplyFileAttribute(directoryToDelete, FileAttributes.Normal);
            directoryToDelete.Delete(true);
        }

        #endregion

        #region tests

#if(UNITTEST)
        public abstract class AbstractDropNamingStrategyTester
        {
            protected void CheckDirectoryName(DirectoryInfo actual, DirectoryInfo expected, string description)
            {
                Console.WriteLine("-------------------");
                Console.WriteLine("Checking drop directory for project/solution '" + description + "'.");
                Console.WriteLine("The path: ");
                Console.WriteLine("'" + actual.FullName + "'");
                Console.WriteLine("should match the expected path of:");
                Console.WriteLine("'" + expected.FullName + "'");

                if (String.Compare(actual.FullName, expected.FullName, false) != 0)
                    NUnit.Framework.Assert.Fail("The build drop directory path '{0}' does not match the expected path of '{1}'.", actual.FullName, expected.FullName);

                Console.WriteLine("Passed...");
            }
        }
#endif

        #endregion
    }
}
