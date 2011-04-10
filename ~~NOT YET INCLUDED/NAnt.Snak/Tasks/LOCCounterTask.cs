using System;
using NAnt.Core;
using NAnt.Core.Attributes;

using Snak.Core;
using Snak.Metrics;

namespace Snak.Tasks
{
	/// <summary>
	/// Counts lines of code in a project
	/// </summary>
	[TaskName("loc-count")]
	public class LOCCounterTask : ProjectTask
	{
		protected override void ExecuteTask()
		{
			this.Threshold = Level.Debug;

			IProjectInfo project = this.GetProject();
			LOCCounter counter = new LOCCounter();
			int count = counter.CountLines(project);

			Log(Level.Info, "Counted {0} LOC in project {1}", count, project.Name);
		}
	}
}
