using System.IO;
using NAnt.Core;
using NAnt.Core.Attributes;

using Snak.Core;

namespace Snak.Tasks
{
	/// <summary>
	/// A base class for NAnt tasks that work with Visual Studio project's
	/// </summary>
	public abstract class ProjectTask : Task
	{
		private FileInfo _projectInfo;
		private string _config;

		/// <summary>
		/// Specifies the project which is being manipulated
		/// </summary>
		[TaskAttribute("project", Required=true)]
		public FileInfo ProjectInfo {
			get { return _projectInfo; }
			set { _projectInfo = value; }
		}

		/// <summary>
		/// Specifies the project's build configuration that's being used
		/// </summary>
		[TaskAttribute("config", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string Config {
			get { return _config; }
			set { _config = value; }
		}

		protected IProjectInfo GetProject()
		{
            return ProjectFactory.GetProject(ProjectInfo, Config, new NAntLoggingProxy(Log).Log);
		}
	}
}