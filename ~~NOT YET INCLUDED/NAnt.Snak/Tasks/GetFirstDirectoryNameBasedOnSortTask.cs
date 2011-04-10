using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;

using NAnt.Core;
using NAnt.Core.Attributes;

using NUnit.Framework;

namespace Snak.Tasks
{
	/// <summary>
	/// Gets the full path to the first directoy in a list based on a given filter
	/// </summary>
	[TaskName("getFirstDirectoryNameBasedOnSort")] 
	public class GetFirstDirectoryNameBasedOnSortTask : TaskContainer
	{ 
		private string _pathToContainingFolder = String.Empty;
		private string _directoryNameFilter = String.Empty;
		private string _commonDirectoryNamePart = String.Empty;
		private bool _sortDescending = false;
		private string _propertyName = String.Empty;
		private string _defaultValue = String.Empty;

		private SortedList _sortedDirectories = null;
		
		/// <summary>
		/// The path to the directory in which the desired sub directory exists
		/// </summary>
		[TaskAttribute("pathToContainingFolder", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string PathToContainingFolder 
		{
			get { return _pathToContainingFolder; }
			set { _pathToContainingFolder = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Defaults to false</remarks>
		[TaskAttribute("sortDescending", Required=false)]
		public bool SortDescending 
		{
			get { return _sortDescending; }
			set { _sortDescending = value; }
		}

		/// <summary>
		/// The property prefix that's used to load up all the project properties
		/// </summary>
		[TaskAttribute("property", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string PropertyName 
		{
			get { return _propertyName; }
			set { _propertyName = value; }
		}

		/// <summary>
		/// Sets a default value for the property FullPathToFirstDirectory, if no directory is found this will be used
		/// </summary>
		[TaskAttribute("defaultValue", Required=false)]
		[StringValidator(AllowEmpty=true)]
		public string DefaultValue
		{
			get { return _defaultValue; }
			set { _defaultValue = value; }
		}

		/// <summary>
		/// The filter to be applied to the sub directories of the directory identified by PathToContainingFolder.
		/// 
		/// This property must contain a wildcard (*)at either the beginning or the end of its value. This wildcard denotes
		/// the difference between the directories for which you want to obtain only one of. For example if you have the following directories 
		/// 
		/// 1_MyApplicationProductionLogs
		/// 2_MyApplicationProductionLogs
		/// 3_MyApplicationProductionLogs
		/// 4_MyApplicationProductionLogs
		/// 
		/// you would specify "*_MyApplicationProductionLogs" for the value of this property
		/// 
		/// or perhaps 
		/// 
		/// MyApplicationProductionLogs_1
		/// MyApplicationProductionLogs_2
		/// MyApplicationProductionLogs_3
		/// MyApplicationProductionLogs_4
		/// 
		/// you would specify "MyApplicationProductionLogs_*" for the value of this property
		/// 
		/// This task will sort the directories based on the value of the wildcard (in the above example on a number) and 
		/// return either the first or last directory based on the value of the property SortDescending.
		/// </summary>
		[TaskAttribute("directoryNameFilter", Required=true)]
		public string DirectoryNameFilter
		{
			get { return _directoryNameFilter; }
			set 
			{
				char[] trimThis = new char[] {'*'};

				if (value.TrimStart(trimThis).TrimEnd(trimThis).IndexOf("*", 0) > -1)
					throw new ApplicationException("The property " + System.Reflection.MethodBase.GetCurrentMethod() + " can only have a wildcard (*) at the beginning or at the end but not in the middle.");

				if ((value.StartsWith("*") && ! value.EndsWith("*")) || (value.EndsWith("*") && ! value.StartsWith("*")))
					_directoryNameFilter = value; 
				else
					throw new ApplicationException("The property " + System.Reflection.MethodBase.GetCurrentMethod() + " can only have a wildcard (*) mask at the beginning or at the end but not both");
			
				_commonDirectoryNamePart = _directoryNameFilter.Replace("*", "");
			}
		}

		protected override void ExecuteTask()
		{
			string directoryFullName = String.Empty;

			if (Directory.Exists(_pathToContainingFolder))
			{
				Log(Level.Verbose, "Entering" + System.Reflection.MethodInfo.GetCurrentMethod().Name);

				DirectoryInfo directoryInfo = new DirectoryInfo(_pathToContainingFolder);
				ArrayList directories = new ArrayList(directoryInfo.GetDirectories(_directoryNameFilter));

				Log(Level.Verbose, "Found {0} directories matching the filter {1}", directories.Count, this._directoryNameFilter);

				if (directories.Count > 0)
				{
					DirectoryInfo currentDirectoryInfo;
					string uniqueDirectoryNamePart = String.Empty;

					_sortedDirectories = new SortedList(new NumericalComparer(this._sortDescending));

					Log(Level.Verbose, "Backwards looping over directories and removing ones that dont begin or end in a unique number.");
		
					for (int i = directories.Count - 1; i >= 0; i --)
					{
						currentDirectoryInfo = (DirectoryInfo) directories[i];
						uniqueDirectoryNamePart = currentDirectoryInfo.Name.Replace(_commonDirectoryNamePart, "");

						try
						{
							int dirIncrement = int.Parse(uniqueDirectoryNamePart);
							_sortedDirectories[dirIncrement] = currentDirectoryInfo;
						}
						catch
						{
							Log(Level.Verbose, "Removing directory '" + currentDirectoryInfo.Name + "' because it does not have a numerical increment part.");
							directories.RemoveAt(i);
						}
					}

					Log(Level.Verbose, "Sorting directories and getting the first on in the list.");

					directoryFullName = ((DirectoryInfo)_sortedDirectories.GetByIndex(0)).FullName;

					Log(Level.Info, "Found " + directories.Count + " directories matching the filter '" + this._directoryNameFilter + "'");
					Log(Level.Info, "First directory is  " + directoryFullName);
				}

				Log(Level.Verbose, "Setting the nant property '" + _propertyName + "." + "FullPathToFirstDirectory' to the value '" + directoryFullName + "'");
			}
			else
			{
				string path = _pathToContainingFolder;

				if (!Path.IsPathRooted(path))
					path = Path.GetFullPath(path);
				
				Log(Level.Verbose, "The directory " + path + " does not exist.");
			}

			if (directoryFullName == String.Empty && _defaultValue != String.Empty)
			{
				Log(Level.Verbose, "No directory found, using the default value of '"+_defaultValue+"'");
				directoryFullName = _defaultValue;
			}
			else if (directoryFullName == String.Empty && _defaultValue == String.Empty)
			{
				Log(Level.Verbose, "No directory found.");		
			}

			Properties[_propertyName + "." + "FullPathToFirstDirectory"] = directoryFullName;
		}
 
		/// <summary>
		/// 
		/// </summary>
		private class NumericalComparer : IComparer
		{
			bool _sortDescending;
			
			public NumericalComparer(bool sortDescending)
			{
				this._sortDescending = sortDescending;
			}

			public int Compare( object x, object y )
			{
				int intX = (int)x;
				int intY = (int)y;

				if (intX == intY)
				{
					return 0;
				}
				else if (intX > intY)
				{
                    return (this._sortDescending) ? -1 : 1; 
				}
				else
				{
                    return (this._sortDescending) ? 1 : -1;
				}
			}			
		}
	}
}
