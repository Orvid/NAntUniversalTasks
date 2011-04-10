using System;

namespace Snak.Core.Everett
{
	/// <summary>
	/// Represents a <see cref="SolutionInfo"/>'s links to it's <see cref="ProjectInfo"/>'s
	/// </summary>
	internal class ProjectLink
	{
		#region Declarations
		private string _name;
		private string _location;
		private Guid _projectGuid;
		private Guid _solutionGuid;
		#endregion

		internal ProjectLink(){}

		public ProjectLink(string name, string location, Guid projectGuid, Guid solutionGuid)
		{
			_name = name;
			_location = location.ToLower();
			_projectGuid = projectGuid;
			_solutionGuid = solutionGuid;
		}

		public string Name {
			get { return _name; }
			set { _name = value; }
		}

		public string Location {
			get { return _location; }
			set { _location = (value==null) ? null : value.ToLower(); }
		}

		public Guid ProjectGuid {
			get { return _projectGuid; }
			set { _projectGuid = value; }
		}

		public Guid SolutionGuid {
			get { return _solutionGuid; }
			set { _solutionGuid = value; }
		}

		public bool IsWebProject{
			get{ return IsPathWebProject(_location); }
		}

        internal static bool IsPathWebProject(string projectPath)
        {
            return projectPath.StartsWith(Uri.UriSchemeHttp);
        }
    }
}
