using System;

namespace NAntExtensions.TeamCity.Common.BuildEnvironment
{
	internal class SystemEnvironment : IEnvironment
	{
		#region IEnvironment Members
		public string GetEnvironmentVariable(string variable)
		{
			return Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Process);
		}
		#endregion
	}
}