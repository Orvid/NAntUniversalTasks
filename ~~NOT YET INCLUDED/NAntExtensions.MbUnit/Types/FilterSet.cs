using System.Collections;
using System.Collections.Specialized;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace NAntExtensions.MbUnit.Types
{
	/// <summary>
	/// A set of strings, mostly used to include and exclude certain test categories.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Strings can be grouped to sets, and later be referenced by their <see cref="DataTypeBase.ID" />.
	/// </para>
	/// </remarks>
	[ElementName("filterset")]
	public class FilterSet : IncludeSet
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FilterSet"/> class.
		/// </summary>
		public FilterSet()
		{
			Exclude = new PatternCollection();
		}

		/// <summary>
		/// Defines a single string for tests to exclude.
		/// </summary>
		[BuildElementArray("exclude")]
		public PatternCollection Exclude
		{
			get;
			set;
		}

		/// <summary>
		/// Adds a nested set of includes and excludes, or references other standalone <see cref="FilterSet"/>.
		/// </summary>
		/// <param name="filterSet">The <see cref="FilterSet" /> to add.</param>
		[BuildElement("filterset")]
		public void Append(FilterSet filterSet)
		{
			foreach (string include in filterSet.GetIncludePatterns())
			{
				Pattern pattern = new Pattern { PatternName = include };
				Include.Add(pattern);
			}

			foreach (string exclude in filterSet.GetExcludePatterns())
			{
				Pattern pattern = new Pattern { PatternName = exclude };
				Exclude.Add(pattern);
			}
		}

		/// <summary>
		/// Gets the exclude patterns.
		/// </summary>
		/// <returns></returns>
		public string[] GetExcludePatterns()
		{
			ArrayList includes = new ArrayList(Include.Count);

			foreach (Pattern exclude in Exclude)
			{
				if (exclude.IfDefined && !exclude.UnlessDefined)
				{
					includes.Add(exclude.PatternName);
				}
			}

			return (string[])includes.ToArray(typeof(string));
		}
	}
}