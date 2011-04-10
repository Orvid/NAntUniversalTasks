using System;
using System.IO;
using NAnt.Core;
using NAnt.Core.Attributes;
using Snak.Core;

namespace Snak.Tasks
{
	/// <summary>
	/// Provides a simple way of packaging up a project's output for deployment
	/// </summary>
    /// <remarks>
    /// This task creates an output drop from a project that mirrors the expected output
    /// from an installer (content files + application binaries in application root folder),
    /// and also understands the specific semantics of web applications (content files in root,
    /// binaries in /bin folder)
    /// </remarks>
	[TaskName("deployproject")]
	public class DeployProjectTask : ProjectTask
	{
		private DirectoryInfo _targetDir;

		/// <summary>
		/// The deployment destination for the project output
		/// </summary>
		[TaskAttribute("to", Required=true)]
		public DirectoryInfo TargetDir
		{
			get { return _targetDir; }
			set { _targetDir = value; }
		}

		protected override void ExecuteTask()
		{
			IProjectInfo project = GetProject();

            Log(Level.Verbose, "\n" + new String('=',20));
			Log(Level.Info, "Deploying project {0} to {1}", project.ProjectName, _targetDir);

			if (!_targetDir.Exists) _targetDir.Create();

		    DeployProjectOutput(project);
		    DeployProjectContent(project, _targetDir);
		}

	    /// <summary>
        /// Deploy binaries (and other files) from the project output dir (and sub directories) into the output folder
        /// (or a BIN subdirectory, if a web project)
	    /// </summary>
        private void DeployProjectOutput(IProjectInfo project)
	    {
	        DirectoryInfo sourceDirectory = new DirectoryInfo(project.OutputPathFull);
	        DirectoryInfo destinationDirectory = project.IsWebProject
	                                                   ? _targetDir.CreateSubdirectory("bin")
	                                                   : _targetDir;

	        new CopyDirectory(sourceDirectory, Log).To(destinationDirectory);
	    }

	    /// <summary>
        /// Deploy *content* files from the project to the output folder, recreating the directory structure
	    /// </summary>
	    private void DeployProjectContent(IProjectInfo project, DirectoryInfo destinationRoot) {
	        string[] sourceFilesRelative = project.GetProjectFiles(ProjectFileType.Content);
	        foreach (string sourceFileRelative in sourceFilesRelative)
	        {
	            FileInfo sourceFile = new FileInfo(Path.Combine(project.ProjectDir, sourceFileRelative));

                string targetDestinationDir = String.Empty;

                if (!sourceFileRelative.StartsWith(@"..\"))
                    // only do this if the sourceFileRelative is in a subdirectory and not somewhere up the tree
                    // if we are dealing with a linked/shared file the call to GetDirectoryName below returns less desirable results
                    //  e.g. run this in powershell: [System.IO.Path]::GetDirectoryName("..\log4net.config")
                    // and the result is ..
                    // Passing .. to the CopyFile.To method below results in an error.
                    targetDestinationDir = Path.GetDirectoryName(sourceFileRelative);    // just strips the filename off the end

                new CopyFile(sourceFile, Log).To(destinationRoot, targetDestinationDir);
	        }
	    }
	    
	    #region CopyFile

	    /// <summary>
	    /// Provides a fluid-interface API into a copy-file operation
	    /// </summary>
	    /// <remarks>Total overkill, but it makes the code ultra-readable</remarks>
		internal class CopyFile : LogsToTaskOutput
		{
			public readonly FileInfo SourceFile;

			public CopyFile(FileInfo sourceFile, NAntLoggingHandler logHandler)
	        :base(logHandler)
			{
				this.SourceFile = sourceFile;
			}

			public void To(DirectoryInfo destinationPath)
			{
                string destinationFile = Path.Combine(destinationPath.FullName, SourceFile.Name);
                Log(Level.Verbose, "\tCopying {0} to {1}", SourceFile.FullName, destinationFile);
				if (File.Exists(destinationFile))
					File.SetAttributes(destinationFile, FileAttributes.Normal);

			    SourceFile.CopyTo(destinationFile, true);
			}

			public void To(DirectoryInfo destinationRoot, string destinationRelative)
			{
			    // Resolve the relative path (and create if required)
                DirectoryInfo destinationPath = (destinationRelative.Trim() == string.Empty)
					? destinationRoot
					: destinationRoot.CreateSubdirectory(destinationRelative);

			    // Chain into simpler overload
			    To(destinationPath);
			}
		}
		#endregion

        #region CopyDirectory

        /// <summary>
        /// Provides a fluid-interface API into a directory-copy operation
        /// </summary>
        /// <remarks>that as confusing as hell!!!</remarks>
        internal class CopyDirectory : LogsToTaskOutput
        {
            public readonly DirectoryInfo SourceDirectory;

            public CopyDirectory(DirectoryInfo sourceDirectory, NAntLoggingHandler logHandler)
                : base(logHandler)
            {
                this.SourceDirectory = sourceDirectory;
            }

            public void To(DirectoryInfo destinationDirectory)
            {
                Log(Level.Verbose, "\tCopying {0} to {1}", SourceDirectory, destinationDirectory);

                if (!destinationDirectory.Exists)
                    destinationDirectory.Create();

                foreach (FileInfo binariesSourceFile in SourceDirectory.GetFiles())
                {
                    new CopyFile(binariesSourceFile, Log).To(destinationDirectory);
                }

                foreach (DirectoryInfo directoryToCopy in SourceDirectory.GetDirectories())
                {
                    // we want to maintain the directory structure as we are copying
                    // so we grab just the directory name from directoryToCopy and create 
                    // a directory with that same name under destinationDirectory
                    string nestedDestinationDirectoryPath = Path.Combine(destinationDirectory.FullName, directoryToCopy.Name);
                    DirectoryInfo nestedDestinationDirectory = new DirectoryInfo(nestedDestinationDirectoryPath);
                    nestedDestinationDirectory.Create(); // this call wont blow up if it exists

                    new CopyDirectory(directoryToCopy, Log).To(nestedDestinationDirectory);
                }
            }
        }

        #endregion

        #region LogsToTaskOutput
        internal abstract class LogsToTaskOutput
	    {
	        public readonly NAntLoggingHandler Log;
	        
	        protected LogsToTaskOutput(NAntLoggingHandler logHandler)
	        {
	            this.Log = logHandler;
	        }
	    }
	    #endregion

    }
}
