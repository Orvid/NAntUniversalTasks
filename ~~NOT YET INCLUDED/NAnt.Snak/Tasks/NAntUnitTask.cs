using System;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;

namespace Snak.Tasks
{
	/// <summary>
	/// Provides a task to run any test-targets within the the nant build script.
	/// These are targets with the suffix of 'tester'.
	/// Like the NUnit task, errors are collected, rather than immediately failing,
	/// so all tests are run.
	/// </summary>
	/// <remarks>
	/// <example>
	///   <para>Trivial example:</para>
	///   <code>
	///     <![CDATA[
	///     <nantunit/>
	///     ]]>
	///   </code>
	/// </example>
	/// </remarks>
	[TaskName("nantunit")]
	public class NAntUnitTask : NAntTask
	{
		protected override void ExecuteTask()
		{
			bool errorsRaised = false;
			foreach (Target target in Project.Targets)
				if (target.Name.EndsWith("tester"))
					try
					{
						target.Execute();
					}
					catch(Exception err)
					{
						errorsRaised = true;
						Log(Level.Error, err.ToString());
					}

			if (errorsRaised)
				throw new BuildException("NAntUnit tests failed", Location);
		}
	}
}
