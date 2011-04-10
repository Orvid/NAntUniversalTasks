using System;
using System.IO;
using NAnt.Core;
using NAnt.Core.Attributes;

using Snak.ConfigurationTransformation;
using NAnt.Core.Types;

namespace Snak.Tasks
{
    /// <summary>
    /// Uses the <see cref="ConfigurationTransformer"/> to pre-process an xml configuration
    /// file based on certain conditional statements which appear within the file.
    /// This version of the task all files within a fileset, and processes them in-place (ie overwrites them)
    /// <para>
    /// This is useful in situations where you have to maintain multiple environments each having some different configuration settings. Its allows for all of the 
    /// setting to be stored in one file. For sensitive settings either encrypt the section or store it in the registry. For more information on this see 
    /// http://aspnet.4guysfromrolla.com/articles/021506-1.aspx
    /// </para></summary>
    [TaskName("transformConfigurationFiles")]
    public class TransformConfigurationFilesTask : Task
    {
        private string _targetEnvironment = String.Empty;

        private FileSet _items;

        /// <summary>
        /// the items to transform
        /// </summary>
        [BuildElement("items", Required = true)]
        public FileSet Items
        {
            get { return _items; }
            set { _items = value; }
        }

        [TaskAttribute("targetEnvironment", Required = true)]
        public string TargetEnvironment
        {
            get { return _targetEnvironment; }
            set { _targetEnvironment = value; }
        }

        protected override void ExecuteTask()
        {
            foreach (string fileName in _items.FileNames)
            {
                Log(Level.Verbose, "Transforming {0} in-place", fileName);
                ConfigurationTransformer transformer = new ConfigurationTransformer();
                transformer.Transform(_targetEnvironment, fileName, fileName);
            }
        }
    }
}
