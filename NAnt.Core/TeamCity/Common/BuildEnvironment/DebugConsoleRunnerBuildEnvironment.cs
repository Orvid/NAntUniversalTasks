namespace NAntExtensions.TeamCity.Common.BuildEnvironment
{
	internal class DebugConsoleRunnerBuildEnvironment : IBuildEnvironment
	{
		#region IBuildEnvironment Members
		public bool IsTeamCityBuild
		{
			get { return true; }
		}

		public bool IsRunningWithTeamCityNAntRunner
		{
			get { return false; }
		}
		#endregion
	}
}