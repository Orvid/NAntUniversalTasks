using System.Collections.Generic;

using NAnt.Core.Types;

namespace NAntExtensions.TeamCity.Common.Helper
{
	public static class FileSetHelper
	{
		public static IEnumerable<string> Flatten(FileSet[] fileSets)
		{
			Ensure.ArgumentIsNotNull(fileSets, "fileSets");

			foreach (FileSet assemblySet in fileSets)
			{
				foreach (string fileName in assemblySet.FileNames)
				{
					yield return fileName;
				}
			}
		}
		
		public static int Count(FileSet[] fileSets)
		{
			Ensure.ArgumentIsNotNull(fileSets, "fileSets");

			int count = 0;
			foreach (FileSet assemblies in fileSets)
			{
				count += assemblies.FileNames.Count;
			}

			return count;
		}
	}
}