namespace NAntExtensions.TeamCity.Common.BuildEnvironment
{
	public interface IBuildEnvironment
	{
		bool IsTeamCityBuild
		{
			get;
		}

		bool IsRunningWithTeamCityNAntRunner
		{
			get;
		}
	}
}