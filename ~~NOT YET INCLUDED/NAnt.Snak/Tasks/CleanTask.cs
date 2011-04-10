using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.Contrib.Tasks.Web;

using Snak.Core;

namespace Snak.Tasks
{
    [TaskName("clean")]
    public class CleanTask : Task
    {
        private FileInfo _solution = null;

        /// <summary>
        /// The solution that's to be deployed
        /// </summary>
        [TaskAttribute("solution", Required = false)]
        public FileInfo Solution
        {
            get { return _solution; }
            set { _solution = value; }
        }

        private string _solutionConfiguration;

        /// <summary>
        /// The build configuration within the solution.
        /// Project configurations are mapped against this build configuration based on what's
        /// been configured in the solution file. This task only deploys projects for the given build config.
        /// 
        /// Example VS 2005 value: Debug|Any CPU
        /// </summary>
        [TaskAttribute("config", Required = false)]
        [StringValidator(AllowEmpty = true)]
        public string SolutionConfiguration
        {
            get { return _solutionConfiguration; }
            set { _solutionConfiguration = value; }
        }

        private FileSet _items = null;

        /// <summary>
        /// the items to clean, this is optional if you pass in a solution via the Solution property
        /// </summary>
        [BuildElement("items")]
        public FileSet Items
        {
            get { return _items; }
            set { _items = value; }
        }

        protected override void ExecuteTask()
        {
            ISolutionInfo solution = null;
            IProjectInfo[] projects = null;

            if (this._solution == null && this._items == null)
            {
                throw new BuildException("You must specify either a solution file a file set via the items element.");
            }

            if (this._solution != null && this._items != null)
            {
                throw new BuildException("You can only specify either a solution file or a file set via the items element but not both.");
            }

            if (this._solution != null && this.SolutionConfiguration == String.Empty)
            {
                throw new BuildException("When specifying a solution file you must specify a value for the config attribute. e.g. config=' Debug|Any CPU'.");
            }

            if (this._solution != null)
            {
                _items = new FileSet();
                solution = SolutionFactory.GetSolution(this._solution, new NAntLoggingProxy(Log).Log);
                projects = solution.GetProjectsFor(this._solutionConfiguration);
                
                foreach (IProjectInfo project in projects)
                {
                    _items.Includes.Add(project.OutputPathFull.TrimEnd('\\') + "\\**");
                }
            }
          
            DeleteTask deleteTask = new DeleteTask();
            this.CopyTo(deleteTask);
            deleteTask.DeleteFileSet = _items;
            deleteTask.FailOnError = false;
            deleteTask.Execute();   
        }
    }
}
