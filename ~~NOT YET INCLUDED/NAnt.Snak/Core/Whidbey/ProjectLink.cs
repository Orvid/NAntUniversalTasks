using System;

namespace Snak.Core.Whidbey
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
        private Guid _projectTypeGuid;
		#endregion

		internal ProjectLink(){}

        public ProjectLink(string name, string location, Guid projectGuid, Guid projectTypeGuid)
		{
			_name = name;
			_location = location.ToLower();
			_projectGuid = projectGuid;
            _projectTypeGuid = projectTypeGuid;
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

        public Guid ProjectTypeGuid
        {
            get { return _projectTypeGuid; }
            set { _projectTypeGuid = value; }
		}

		public bool IsWebProject{
			get{ return _location.StartsWith(Uri.UriSchemeHttp); }
		}
	}
}
