using System;
using System.Collections.Generic;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;

using Snak.Core;
using Snak.Utilities;
using NAnt.Core.Types;
using System.IO;
using NAnt.Core.Tasks;
using Snak.BuildDrop;

namespace Snak.Types
{
    [Serializable()]
    public class Package : Element
    {
        private PackageMode _mode = PackageMode.None;

        internal PackageMode Mode
        {
            get 
            {
                if (_mode == PackageMode.None)
                    CheckIsValidAndSetMode();

                return _mode; 
            }

            private set { _mode = value; }
        }

        private string _packageName;

        [TaskAttribute("name", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string PackageName
        {
            get { return _packageName; }
            set { _packageName = value; }
        }

        private FileInfo _file;

        [TaskAttribute("file", Required = false)]
        public FileInfo File
        {
            get { return _file; }
            set { _file = value; }
        }

        string _toFile = null;

        [TaskAttribute("toFile", Required = false)]
        public string ToFile
        {
            get { return _toFile; }
            set { _toFile = value; }
        }

        //private DirectoryInfo _fromDir;

        //[TaskAttribute("fromDir", Required = false)]
        //public DirectoryInfo FromDir
        //{
        //    get { return _fromDir; }
        //    set { _fromDir = value; }
        //}

        private string _toDir = null;

        [TaskAttribute("toDir", Required = false)]
        public string ToDir
        {
            get { return _toDir; }
            private set { _toDir = value; }
        }

        private FileSet _filesToPackage;

        /// <summary>
        /// The set of files in which to package
        /// </summary>
        [BuildElement("fileset", Required = false)]
        public FileSet FilesToPackage
        {
            get { return _filesToPackage; }
            set { _filesToPackage = value; }
        }

        /// <summary>
        /// packages the contents to both the labelled and current directories
        /// </summary>
        /// <param name="dropNamingStrategy"></param>
        public void PackageContents(IDropNamingStrategy dropNamingStrategy)
        {
            CheckIsValidAndSetMode();

            CopyTask copyFromWorkingFolderToLabeledDirTask = new CopyTask();
            CopyTask copyFromWorkingFolderToCurrentDirTask = new CopyTask();

            this.CopyTo(copyFromWorkingFolderToLabeledDirTask);
            this.CopyTo(copyFromWorkingFolderToCurrentDirTask);

            if (this._mode == PackageMode.PackageDirectory)
            {
                DirectoryInfo packagesLabeledDirectoryInfo = dropNamingStrategy.GetPackageDropDirectory(this.PackageName, true, false);
                copyFromWorkingFolderToLabeledDirTask.CopyFileSet = this.FilesToPackage;
                copyFromWorkingFolderToLabeledDirTask.ToDirectory = packagesLabeledDirectoryInfo;
                copyFromWorkingFolderToLabeledDirTask.Execute();

                DirectoryInfo packagesCurrentDirectoryInfo = dropNamingStrategy.GetPackageDropDirectoryCurrent(this.PackageName, true, false);
                copyFromWorkingFolderToCurrentDirTask.CopyFileSet = this.FilesToPackage;
                copyFromWorkingFolderToCurrentDirTask.ToDirectory = packagesCurrentDirectoryInfo;               
                copyFromWorkingFolderToCurrentDirTask.Execute();
            }
            else if (this._mode == PackageMode.PackageSingleFile)
            {
                FileInfo packagesLabeledFileInfo = new FileInfo(Path.Combine(dropNamingStrategy.DropDirectory.FullName, this.ToFile));

                copyFromWorkingFolderToLabeledDirTask.SourceFile = this.File;
                copyFromWorkingFolderToLabeledDirTask.ToFile = packagesLabeledFileInfo;
                copyFromWorkingFolderToLabeledDirTask.Execute();

                // we only drop to the current folder if the dropNamingStrategy.DropDirectoryCurrent is not null 
                if (dropNamingStrategy.DropDirectoryCurrent != null)
                {
                    FileInfo packagesCurrentFileInfo = new FileInfo(Path.Combine(dropNamingStrategy.DropDirectoryCurrent.FullName, this.ToFile));
                    copyFromWorkingFolderToCurrentDirTask.SourceFile = this.File;
                    copyFromWorkingFolderToCurrentDirTask.ToFile = packagesCurrentFileInfo;
                    copyFromWorkingFolderToCurrentDirTask.Execute();
                }
            }
            else
                throw new InvalidOperationException(GetGenericErrorString());
        }

        /// <summary>
        /// ensure the state of this task is valid, this task can either package a single file or a fileset, given attributes need to be present 
        /// to support each mode, this method just checks they are.
        /// </summary>
        /// <exception cref="InvalidOperationException"> an InvalidOperationException is thrown if the state is not valid</exception>
        private void CheckIsValidAndSetMode()
        {
            bool requiredAttributesSetForPackageDirectoryMode = (this.FilesToPackage != null && this.ToDir != null);
            bool requiredAttributesSetForPackageSingleFileMode = (this.File != null && this.ToFile != null);

            if (requiredAttributesSetForPackageDirectoryMode)
            {
                // make sure they didnt also speicfy the other attributes that are not valid for this mode.
                if (this.File != null || this.ToFile != null)
                    throw new InvalidOperationException(GetGenericErrorString());

                if (!this.FilesToPackage.BaseDirectory.Exists)
                    throw new InvalidOperationException("The directory to package could not be found at path '" + this.FilesToPackage.BaseDirectory.FullName + "'");

                this._mode = PackageMode.PackageDirectory;
            }

            if (requiredAttributesSetForPackageSingleFileMode)
            {
                // make sure they didnt also speicfy the other attributes that are not valid for this mode.
                if (this.FilesToPackage != null || this.ToDir != null)
                    throw new InvalidOperationException(GetGenericErrorString());

                if (!this.File.Exists)
                    throw new InvalidOperationException("The file to package could not be found at path '" + this.File.FullName + "'");

                this._mode = PackageMode.PackageSingleFile;
            }

            if (this._mode == PackageMode.None)
                throw new InvalidOperationException(GetGenericErrorString());
        }

        private string GetGenericErrorString()
        { 
            return "The package with name '" + this.PackageName + "' is not valid, all packages must have set either the fromFile and toFile attributes OR a file set element and the toDir attribute, but not both. You either copy a single file or an entire directory.";
        }
    }
}
