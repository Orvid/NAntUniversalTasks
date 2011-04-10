using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;
using System.IO;
using System.Text.RegularExpressions;
using Snak.Core;

namespace Snak.Tasks
{
    /// <summary>
    /// Sets the AssemblyVersion attribute in a give file/s
    /// </summary>
    /// <remarks>
    /// Searches for the following text in the specified files: AssemblyVersion("ANY_4_POINT_VERSION_NUMBER_HERE")
    /// and replaces the version numbers with those passed into the task. If you omit version numbers when constructing the 
    /// task xml then they will not be modified in the file. If the pattern AssemblyVersion("ANY_4_POINT_VERSION_NUMBER_HERE") 
    /// appears multiple times it will be modified in each place. If there are '*' in the AssemblyVersion string they will be normalised
    /// into a 4 part version number e.g. 0.0.0.0
    /// </remarks>
    [TaskName("setAssemblyVersion")]
    public class SetAssemblyVersionTask : Task
    {
        private string _major;

        [TaskAttribute("major", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string Major
        {
            get { return _major; }
            set { _major = value; }
        }

        private string _minor;

        [TaskAttribute("minor", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string Minor
        {
            get { return _minor; }
            set { _minor = value; }
        }

        private string _build;

        [TaskAttribute("build", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string Build
        {
            get { return _build; }
            set { _build = value; }
        }

        private string _revision;

        [TaskAttribute("revision", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string Revision
        {
            get { return _revision; }
            set { _revision = value; }
        }

        private FileSet _items = null;

        /// <summary>
        /// the items to transform
        /// </summary>
        [BuildElement("items", Required = true)]
        public FileSet Items
        {
            get { return _items; }
            set { _items = value; }
        }

        protected override void ExecuteTask()
        {
            SetVersion();
        }

        private void SetVersion()
        {
            string pattern = @"AssemblyVersion\(""(?<buildLabel>[^""]*)""\)";

            foreach (string fileName in _items.FileNames)
            {
                string fileText = File.ReadAllText(fileName);

                string major = String.Empty;
                string minor = String.Empty;
                string build = String.Empty;
                string revision = String.Empty;

                fileText = Regex.Replace(
                    fileText,
                    pattern,
                    delegate(Match match)
                    {
                        string label = match.Groups["buildLabel"].Value;
                        BuildVersion buildLabel = new BuildVersion(label);

                        major = (_major != null) ? _major : buildLabel.Version.Major.ToString();
                        minor = (_minor != null) ? _minor : buildLabel.Version.Minor.ToString();
                        build = (_build != null) ? _build : buildLabel.Version.Build.ToString();
                        revision = (_revision != null) ? _revision : buildLabel.Version.Revision.ToString();

                        string newLabel = String.Format("{0}.{1}.{2}.{3}", major, minor, build, revision);

                        Log(Level.Verbose, "Setting AssemblyVersion in file '{0}' from '{1}' to '{2}'", fileName, label, newLabel);

                        return String.Format("AssemblyVersion(\"{0}\")", newLabel);
                    }
                );

                Log(Level.Verbose, "Setting write permission on file '{0}' to FileAttributes.Normal", fileName);
                File.SetAttributes(fileName, FileAttributes.Normal);
                File.WriteAllText(fileName, fileText);
            }
        }
    }
}
