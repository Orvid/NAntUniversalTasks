using Machine.Specifications;
using Machine.Specifications.Runner;

using NAntExtensions.TeamCity.Common.Helper;
using NAntExtensions.TeamCity.Common.Messaging;

namespace NAntExtensions.Machine.Specifications.RunListeners
{
	internal class TeamCityRunListener : RunListener, ISpecificationRunListener
	{
		readonly ITeamCityMessageProvider _messageProvider;

		public TeamCityRunListener(ITeamCityMessageProvider messageProvider)
		{
			Ensure.ArgumentIsNotNull(messageProvider, "messageProvider");
			_messageProvider = messageProvider;
		}

		#region ISpecificationRunListener Members
		public void OnAssemblyStart(AssemblyInfo assembly)
		{
			_messageProvider.TestSuiteStarted(assembly.Name);
		}

		public void OnAssemblyEnd(AssemblyInfo assembly)
		{
			_messageProvider.TestSuiteFinished(assembly.Name);
		}

		public void OnRunStart()
		{
		}

		public void OnRunEnd()
		{
		}

		public void OnSpecificationStart(SpecificationInfo specification)
		{
			_messageProvider.TestStarted(GetContextSpecName(_currentContext, specification));
		}

		public void OnSpecificationEnd(SpecificationInfo specification, Result result)
		{
			string specName = GetContextSpecName(_currentContext, specification);

			switch (result.Status)
			{
				case Status.Passing:
					break;
				case Status.Failing:
					_messageProvider.TestFailed(specName,
					                            result.Exception.Message,
					                            result.Exception.StackTrace,
					                            result.Exception.FullTypeName);
					break;
				case Status.Ignored:
					_messageProvider.TestIgnored(specName, null);
					break;
				case Status.NotImplemented:
					_messageProvider.TestIgnored(specName, "Not implemented");
					break;
				default:
					break;
			}

			_messageProvider.TestFinished(specName);
		}

		public void OnFatalError(ExceptionResult exception)
		{
			_messageProvider.TestFailed("Fatal error", exception.Message, exception.StackTrace, exception.FullTypeName);
		}
		#endregion
	}
}