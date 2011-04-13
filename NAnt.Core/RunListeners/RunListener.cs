using System;

using Machine.Specifications.Runner;

namespace NAntExtensions.Machine.Specifications.RunListeners
{
	internal abstract class RunListener
	{
		protected ContextInfo _currentContext;

		public virtual void OnContextStart(ContextInfo context)
		{
			_currentContext = context;
		}

		public virtual void OnContextEnd(ContextInfo context)
		{
			_currentContext = null;
		}

		internal static string GetContextSpecName(ContextInfo context, SpecificationInfo specification)
		{
			return String.Format("{0} {1}", context.FullName, specification.Name);
		}

		internal static string GetContextSpecNameWithFormat(ContextInfo context, SpecificationInfo specification)
		{
			return String.Format("{0}{1}    \x00bb {2}", context.FullName, Environment.NewLine, specification.Name);
		}
	}
}