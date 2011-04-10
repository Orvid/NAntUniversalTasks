using NAnt.Core;
using NAnt.Core.Attributes;

using NAntExtensions.TeamCity.Common.BuildEnvironment;
using NAntExtensions.TeamCity.Common.Container;

namespace NAntExtensions.TeamCity.Functions
{
	///<summary>
	/// TeamCity functions.
	///</summary>
	[FunctionSet("teamcity", "TeamCity")]
	public class TeamCityFunctions : FunctionSetBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TeamCityFunctions"/> class.
		/// </summary>
		/// <param name="project">The project.</param>
		/// <param name="properties">The properties.</param>
		public TeamCityFunctions(Project project, PropertyDictionary properties) : base(project, properties)
		{
		}

		/// <summary>
		/// Determines if the build is run as part of a TeamCity build.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this is a TeamCity build; otherwise, <c>false</c>.
		/// </returns>
		/// <example>
		/// <code>
		/// <![CDATA[
		/// <echo message="We are running inside of TeamCity: ${teamcity::is-teamcity-build()}" />
		/// ]]></code>
		/// </example>
		[Function("is-teamcity-build")]
		public static bool IsTeamCityBuild()
		{
			return IoC.Resolve<IBuildEnvironment>().IsTeamCityBuild;
		}
	}
}