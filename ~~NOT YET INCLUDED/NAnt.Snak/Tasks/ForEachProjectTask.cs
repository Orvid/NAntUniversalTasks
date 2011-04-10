using System;
using System.IO;
using NAnt.Core;
using NAnt.Core.Attributes;

using Snak.Core;
using Snak.Utilities;

namespace Snak.Tasks
{
	/// <summary>
	/// Iterates through all the projects in a solution, executing child
	/// tasks once per project. Project attributes are loaded in as nant
	/// properties, so child tasks can gain easy access to aspects of the
	/// 'current' project.
	/// </summary>
	/// <example>
	///   <para>Loops over all files in the project directory.</para>
	///   <code>eg:
	///     <![CDATA[
	/// <foreachproject property="filename" solution="solutionPath">
	///     <do>
	///         <echo message="project: ${project.name}"/>
	///     </do>
	/// </foreach>
	///     ]]>
	///   </code>
	/// </example>
	[TaskName("foreachproject")]
	public class ForEachProjectTask : TaskContainer
	{
		#region Declarations
		private FileInfo _solution;
		private string _solutionConfiguration;
		private string _propertyName;
		private TaskContainer _do;
        private bool _failOnEmpty = true;
        private NAnt.VSNet.Types.WebMapCollection _webMaps = new NAnt.VSNet.Types.WebMapCollection();
		#endregion

		/// <summary>
		/// The solution that's parsed to locate all the properties
		/// </summary>
		[TaskAttribute("solution", Required=true)]
		public FileInfo Solution 
		{
			get { return _solution; }
			set { _solution = value; }
		}

		/// <summary>
		/// The build configuration within the solution.
		/// Project configurations are mapped against this build configuration based on what's
		/// been configured in the solution file.
		/// </summary>
		[TaskAttribute("config", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string SolutionConfiguration 
		{
			get { return _solutionConfiguration; }
			set { _solutionConfiguration = value; }
		}

		/// <summary>
		/// The property prefix that's used to load up all the project properties
		/// </summary>
		[TaskAttribute("property", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string PropertyName 
		{
			get { return _propertyName; }
			set { _propertyName = value; }
		}

		/// <summary>
		/// The tasks that get executed against each project
		/// </summary>
		[BuildElement("do")]
		public TaskContainer Do 
		{
			get { return _do; }
			set { _do = value; }
		}

        /// <summary>
        /// Determines whether an error or a warning is generated
        /// if there are no projects active for the solution configuration supplied
        /// </summary>
        [TaskAttribute("failonempty", Required = false)]
        public bool FailOnEmpty
        {
            get { return _failOnEmpty; }
            set { _failOnEmpty = value; }
        }

		/// <summary>
		/// WebMap of URL's to project references.
		/// </summary>
		[BuildElementCollection("webmap", "map")]
		public NAnt.VSNet.Types.WebMapCollection WebMaps
		{
			get{ return _webMaps; }
		}
 
		/// <summary>
		/// Handles actually doing the work of the task, by executing all
		/// the child tasks in the DO block for each project in the solution,
		/// having setup that project's properties first.
		/// </summary>
        protected override void ExecuteChildTasks()
        {
            if (_webMaps.Count > 0)
                foreach (NAnt.VSNet.Types.WebMap item in _webMaps)
                    Log(Level.Verbose, "Mapping from {0} to {1}", item.Url, item.Path);
            else
                Log(Level.Verbose, "No webmaps set - web projects will be inferred from solution location if present");

            IProjectInfo[] projects;
            try
            {
                ISolutionInfo solution = SolutionFactory.GetSolution(Solution, new NAntLoggingProxy(Log).Log);
                solution.WebMaps.AddRange(this.WebMaps);

                projects = solution.GetProjectsFor(SolutionConfiguration);
            }
            catch (Exception err)
            {
                string message = String.Format("Failed to get projects [Solution={0}; Configuration={1}]", Solution.FullName, SolutionConfiguration);
                throw new BuildException(message, Location, err);
            }

            if (projects.Length == 0)
            {
                string message = string.Format("No projects retrieved for {0} [{1}]", Solution.FullName, SolutionConfiguration);
                if (FailOnEmpty)
                    throw new BuildException(message, Location);
                else
                    Log(Level.Warning, message);
            }

            if (_do == null)
                Log(Level.Warning, "Warning: No tasks set");
            else
            {
                ProjectPropertiesTask projectPropertiesTask = new ProjectPropertiesTask();
                TaskUtils.CopySettingsFrom(this).To(projectPropertiesTask);

                foreach (IProjectInfo project in projects)
                {
                    Log(Level.Verbose, "Running tasks against project " + project.ProjectName);

                    // setup nant properties based on this solution
                    projectPropertiesTask.SetupProperties(project, PropertyName);

                    // execute all the tasks with those properties set
                    try
                    {
                        _do.Execute();
                    }
                    catch (Exception err)
                    {
                        string message = string.Format("Error executing <do> against {0}: {1}", project.Name, err.Message);
                        throw new BuildException(message, Location, err);
                    }
                }
            }
        }



#if(UNITTEST)
		/// <summary>
		/// Provides unit tests for the ForEachProjectTask
		/// Note that most of the functional tests are actually within the nant tests script
		/// </summary>
		[NUnit.Framework.TestFixture]
		public class ForEachProjectTaskTester{


		}
#endif
	}
}
