using System.IO;
using System.Collections;

namespace Snak.Core
{
	public interface ISolutionInfo
	{
		/// <summary>
		/// Retrieve the location of the actual solution file
		/// </summary>
		FileInfo SolutionFile { get; }

		/// <summary>
		/// Retrieves an array of all the projects to be built in the specified solution configuration
		/// </summary>
		/// <param name="solutionConfiguration">A solution configuration (Debug, Release etc...)
		/// This is mapped back to project configurations based on what's configured within the solution,
		/// and doesn't neccesarily match any one project configuration name</param>
		/// <returns></returns>
		IProjectInfo[] GetProjectsFor(string solutionConfiguration);

		/// <summary>
		/// A collection of maps between URI's (or URI prefixes) and local paths,
		/// to be used when resolving HTTP references for projects
		/// </summary>
		NAnt.VSNet.Types.WebMapCollection WebMaps { get; }

        /// <summary>
        /// An array of all the active build configurations for the solution
        /// (from which project configurations are mapped)
        /// One of these values is the input into <see cref="GetProjectsFor"/>
        /// </summary>
        string[] Configurations { get; }
	}
}