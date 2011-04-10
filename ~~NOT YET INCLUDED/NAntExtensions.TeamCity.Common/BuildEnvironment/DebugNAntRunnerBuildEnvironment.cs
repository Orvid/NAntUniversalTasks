namespace NAntExtensions.TeamCity.Common.BuildEnvironment
{
	internal class DebugNAntRunnerBuildEnvironment : IBuildEnvironment
	{
		#region IBuildEnvironment Members
		public bool IsTeamCityBuild
		{
			get { return true; }
		}

		public bool IsRunningWithTeamCityNAntRunner
		{
			get { return true; }
		}
		#endregion
	}
}