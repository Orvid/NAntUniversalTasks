namespace NAntExtensions.TeamCity.Common.BuildEnvironment
{
	public interface IEnvironment
	{
		string GetEnvironmentVariable(string variable);
	}
}