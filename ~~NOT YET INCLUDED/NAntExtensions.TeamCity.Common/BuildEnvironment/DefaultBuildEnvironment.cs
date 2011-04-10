using NAntExtensions.TeamCity.Common.Helper;

namespace NAntExtensions.TeamCity.Common.BuildEnvironment
{
	internal class DefaultBuildEnvironment : IBuildEnvironment
	{
		readonly IEnvironment _environment;

		public DefaultBuildEnvironment(IEnvironment environment)
		{
			Ensure.ArgumentIsNotNull(environment, "environment");
			_environment = environment;
		}

		#region IBuildEnvironment Members
		public bool IsTeamCityBuild
		{
			get { return _environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null; }
		}

		public bool IsRunningWithTeamCityNAntRunner
		{
			get
			{
				// HACK: This relies on an implementation detail. The NAnt runner sets these environment
				// variable whereas the Command Line runner does not.
				return
					// TeamCity < 4.1 EAP
					_environment.GetEnvironmentVariable("teamcity-dotnet-log-file") != null ||
					// TeamCity >= 4.1 EAP
					_environment.GetEnvironmentVariable("BUILD_NUMBER_FORMAT") != null;
			}
		}
		#endregion
	}
}