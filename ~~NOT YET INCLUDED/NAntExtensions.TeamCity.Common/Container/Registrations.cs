using System.Collections.Generic;

using Castle.Core;
using Castle.MicroKernel.Registration;

using NAntExtensions.TeamCity.Common.BuildEnvironment;
using NAntExtensions.TeamCity.Common.Messaging;

namespace NAntExtensions.TeamCity.Common.Container
{
	internal static class Registrations
	{
		internal static IEnumerable<IRegistration> Get()
		{
			yield return Component.For<ITeamCityMessageProvider>().ImplementedBy<TeamCityMessageProvider>();
			yield return Component.For<IEnvironment>().ImplementedBy<SystemEnvironment>();
			yield return Component.For<TeamCityLogWriter>().ImplementedBy<DefaultTeamCityLogWriter>().LifeStyle.Is(LifestyleType.Transient);

			// Uncomment these if you want to debug certain TeamCity environments.
			//yield return Component.For<IBuildEnvironment>().ImplementedBy<DebugConsoleRunnerBuildEnvironment>();
			//yield return Component.For<IBuildEnvironment>().ImplementedBy<DebugNAntRunnerBuildEnvironment>();
			yield return Component.For<IBuildEnvironment>().ImplementedBy<DefaultBuildEnvironment>();
		}
	}
}