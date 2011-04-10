using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NAnt.Core.Attributes;
using NAnt.Core;
using NAnt.Core.Functions;

namespace Snak.Tasks
{
    [TaskName("loadVersion")]
    public class LoadVersionTask : Task
    {
        private FileInfo _versionFile;

        /// <summary>
        /// The path to a version file, this is a txt file containing only a version number string e.g. 1.2.12.234
        /// </summary>
        [TaskAttribute("versionFile", Required = false)]
        public FileInfo VersionFile
        {
            get { return _versionFile; }
            set { _versionFile = value; }
        }

        private string _versionString = String.Empty;

        [TaskAttribute("versionString", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string VersionString
        {
            get { return _versionString; }
            set { _versionString = value; }
        }

        private string _propertyName = String.Empty;

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

        private string _overrideMajorValue = String.Empty;

        [TaskAttribute("overrideMajorValue", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string OverrideMajorValue
        {
            get { return _overrideMajorValue; }
            set { _overrideMajorValue = value; }
        }

        private string _overrideMinorValue = String.Empty;

        [TaskAttribute("overrideMinorValue", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string OverrideMinorValue
        {
            get { return _overrideMinorValue; }
            set { _overrideMinorValue = value; }
        }

        private string _overrideBuildValue = String.Empty;

        [TaskAttribute("overrideBuildValue", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string OverrideBuildValue
        {
            get { return _overrideBuildValue; }
            set { _overrideBuildValue = value; }
        }

        private string _overrideRevisionValue = String.Empty;

        [TaskAttribute("overrideRevisionValue", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string OverrideRevisionValue
        {
            get { return _overrideRevisionValue; }
            set { _overrideRevisionValue = value; }
        }
        protected override void ExecuteTask()
        {
            if (_versionFile == null && String.IsNullOrEmpty(_versionString))
                throw new InvalidOperationException("You must specify either the versionFile or versionString attributes for the loadVersionFileTask task");

            string versionString = null;

            if (_versionFile != null)
            {
                if (!_versionFile.Exists)
                    throw new InvalidOperationException("The version file cannot be found at location '" + _versionFile.FullName + "' please ensure this file exists");

                versionString = File.ReadAllText(_versionFile.FullName);

                if (String.IsNullOrEmpty(versionString))
                    throw new InvalidOperationException("The version file at path '" + _versionFile.FullName + "' does not contain any text.");
            }
            else
                versionString = _versionString;

            Version version = new Version(versionString.Trim());
            version = OverrideVersion(version);

            SetProperty(_propertyName + ".version", version.ToString());
            SetProperty(_propertyName + ".version.major", VersionFunctions.GetMajor(version).ToString());
            SetProperty(_propertyName + ".version.minor", VersionFunctions.GetMinor(version).ToString());
            SetProperty(_propertyName + ".version.build", VersionFunctions.GetBuild(version).ToString());
            SetProperty(_propertyName + ".version.revision", VersionFunctions.GetRevision(version).ToString());
            SetProperty(_propertyName + ".version.path", version.ToString().Replace('.', '_'));
        }

        private void SetProperty(string propertyName, string propertyValue)
        {
            Log(Level.Info, "Setting property '" + propertyName + "' to '" + propertyValue + "'.");
            Properties[propertyName] = propertyValue;
        }

        private Version OverrideVersion(Version version)
        {
            Version newVersion = null;
            StringBuilder newVersionString = new StringBuilder();
            
            newVersionString.Append(String.IsNullOrEmpty(_overrideMajorValue) ? version.Major.ToString() : _overrideMajorValue);
            newVersionString.Append(".");
            newVersionString.Append(String.IsNullOrEmpty(_overrideMinorValue) ? version.Minor.ToString() : _overrideMinorValue);
            newVersionString.Append(".");
            newVersionString.Append(String.IsNullOrEmpty(_overrideBuildValue) ? version.Build.ToString() : _overrideBuildValue);
            newVersionString.Append(".");
            newVersionString.Append(String.IsNullOrEmpty(_overrideRevisionValue) ? version.Revision.ToString() : _overrideRevisionValue);

            try
            {
                newVersion = new Version(newVersionString.ToString());
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error trying to override the build label with a value of " + newVersionString + ", please check the inner exception", ex);
            }

            return newVersion;
        }
    }
}