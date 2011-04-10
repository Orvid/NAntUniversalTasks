using System;
using System.IO;
using NAnt.Core;
using NAnt.Core.Attributes;

using Snak.ConfigurationTransformation;

namespace Snak.Tasks
{
	/// <summary>
    /// Uses the <see cref="ConfigurationTransformer"/> to pre-process an xml configuration
    /// file based on certain conditional statements which appear within the file.
    /// This version of the task processes one file into a seperate output file (or overwrites the original file)
    /// <para>
    /// This is useful in situations where you have to maintain multiple environments each having some different configuration settings. Its allows for all of the 
    /// setting to be stored in one file. For sensitive settings either encrypt the section or store it in the registry. For more information on this see 
    /// http://aspnet.4guysfromrolla.com/articles/021506-1.aspx
    /// </para></summary>
	[TaskName("transformConfigurationFile")]
	public class TransformConfigurationFileTask : Task
	{
		private FileInfo _inputConfigurationFile;
		private FileInfo _outputConfigurationFile;
		private string _targetEnvironment = String.Empty;

		[TaskAttribute("inputConfigurationFile", Required=true)]
		public FileInfo InputConfigurationFile
		{
			get { return _inputConfigurationFile; }
			set { _inputConfigurationFile = value; }
		}

		[TaskAttribute("outputConfigurationFile", Required=true)]
		public FileInfo OutputConfigurationFile
		{
			get { return _outputConfigurationFile; }
			set { _outputConfigurationFile = value; }
		}

		[TaskAttribute("targetEnvironment", Required=true)]
		public string TargetEnvironment
		{
			get { return _targetEnvironment; }
			set { _targetEnvironment = value; }
		}

		protected override void ExecuteTask()
		{
			if (!_inputConfigurationFile.Exists)
				throw new ApplicationException("Could not find the target configuration file '" + _inputConfigurationFile.FullName + "'");

            Log(Level.Verbose, "Transforming {0} as {1}", _inputConfigurationFile.FullName,
                _outputConfigurationFile.FullName);
			ConfigurationTransformer transformer = new ConfigurationTransformer();
			transformer.Transform(_targetEnvironment, _inputConfigurationFile.FullName, _outputConfigurationFile.FullName);				
		}
	}
}
