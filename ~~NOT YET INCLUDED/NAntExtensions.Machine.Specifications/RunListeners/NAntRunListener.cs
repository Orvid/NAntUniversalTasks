using System;
using System.Text;

using Machine.Specifications;
using Machine.Specifications.Runner;

using NAnt.Core;

using NAntExtensions.Machine.Specifications.Types;
using NAntExtensions.TeamCity.Common.Helper;

namespace NAntExtensions.Machine.Specifications.RunListeners
{
	internal class NAntRunListener : RunListener, ISpecificationRunListener
	{
		readonly Task _task;
		int _contextCount;
		int _failedSpecificationCount;
		int _ignoredSpecificationCount;
		int _passedSpecificationCount;
		int _specificationCount;
		int _unimplementedSpecificationCount;

		public NAntRunListener(Task task)
		{
			Ensure.ArgumentIsNotNull(task, "task");
			_task = task;

			UpdateNAntProperties(_task.Properties, 0, 0, 0, 0, 0, 0);
		}

		public bool FailureOccurred
		{
			get { return _failedSpecificationCount > 0; }
		}

		#region ISpecificationRunListener Members
		public void OnAssemblyStart(AssemblyInfo assembly)
		{
			_task.Log(Level.Info, String.Format("{0}Specs in {1}:", Environment.NewLine, assembly.Name));
		}

		public void OnAssemblyEnd(AssemblyInfo assembly)
		{
		}

		public void OnRunStart()
		{
			_task.Log(Level.Info, "Running specs");

			_contextCount = 0;
			_specificationCount = 0;
			_failedSpecificationCount = 0;
			_unimplementedSpecificationCount = 0;
			_ignoredSpecificationCount = 0;
			_passedSpecificationCount = 0;
		}

		public void OnRunEnd()
		{
			StringBuilder summary = new StringBuilder();
			summary.Append(Environment.NewLine);
			summary.AppendFormat("Contexts: {0}, Specifications: {1}", _contextCount, _specificationCount);

			if (_failedSpecificationCount > 0 || _unimplementedSpecificationCount > 0 || _ignoredSpecificationCount > 0)
			{
				summary.AppendFormat(", {0} passed, {1} failed", _passedSpecificationCount, _failedSpecificationCount);

				if (_unimplementedSpecificationCount > 0)
				{
					summary.AppendFormat(", {0} not implemented", _unimplementedSpecificationCount);
				}

				if (_ignoredSpecificationCount > 0)
				{
					summary.AppendFormat(", {0} ignored", _ignoredSpecificationCount);
				}
			}

			_task.Log(Level.Info, summary.ToString());

			UpdateNAntProperties(_task.Properties,
			                     _contextCount,
			                     _specificationCount,
			                     _passedSpecificationCount,
			                     _failedSpecificationCount,
			                     _ignoredSpecificationCount,
			                     _unimplementedSpecificationCount);
		}

		public override void OnContextStart(ContextInfo context)
		{
			base.OnContextStart(context);

			_task.Log(Level.Verbose, String.Format("{0}{1}", Environment.NewLine, context.FullName));
		}

		public override void OnContextEnd(ContextInfo context)
		{
			base.OnContextEnd(context);

			_contextCount++;
		}

		public void OnSpecificationStart(SpecificationInfo specification)
		{
			_task.Log(Level.Verbose, String.Format("    \x00bb {0}", specification.Name));
		}

		public void OnSpecificationEnd(SpecificationInfo specification, Result result)
		{
			_specificationCount++;

			switch (result.Status)
			{
				case Status.Passing:
					_passedSpecificationCount++;
					break;

				case Status.NotImplemented:
					_unimplementedSpecificationCount++;
					_task.Log(Level.Verbose, "      (NOT IMPLEMENTED)");
					break;

				case Status.Ignored:
					_ignoredSpecificationCount++;
					_task.Log(Level.Verbose, "      (IGNORED)");
					break;
				default:
					_failedSpecificationCount++;

					StringBuilder line = new StringBuilder();

					if (!_task.Verbose)
					{
						// If the task is not verbose, we did not log the specification start, so we include it here.
						line.AppendLine(GetContextSpecNameWithFormat(_currentContext, specification));
					}

					line.AppendLine("      (FAIL)");
					line.AppendLine(result.Exception.ToString());

					_task.Log(Level.Error, line.ToString());
					break;
			}
		}

		public void OnFatalError(ExceptionResult exception)
		{
			_failedSpecificationCount++;
			_task.Log(Level.Error, String.Format("Fatal error: {0}", exception));
		}
		#endregion

		static void UpdateNAntProperties(PropertyDictionary properties,
		                                 int contexts,
		                                 int specifications,
		                                 int passedSpecifications,
		                                 int failedSpecifications,
		                                 int ignoredSpecifications,
		                                 int unimplementedSpecifications)
		{
			PropertyDictionaryHelper.AddOrUpdateInt(properties, Counter.Contexts, contexts);
			PropertyDictionaryHelper.AddOrUpdateInt(properties, Counter.Specifications, specifications);
			PropertyDictionaryHelper.AddOrUpdateInt(properties, Counter.PassedSpecifications, passedSpecifications);
			PropertyDictionaryHelper.AddOrUpdateInt(properties, Counter.FailedSpecifications, failedSpecifications);
			PropertyDictionaryHelper.AddOrUpdateInt(properties, Counter.IgnoredSpecifications, ignoredSpecifications);
			PropertyDictionaryHelper.AddOrUpdateInt(properties, Counter.UnimplementedSpecifications, unimplementedSpecifications);
		}
	}
}