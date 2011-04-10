using System.Collections;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace NAntExtensions.MbUnit.Types
{
	/// <summary>
	/// A set of strings, mostly used to include certain tests.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Strings can be grouped to sets, and later be referenced by their <see cref="DataTypeBase.ID" />.
	/// </para>
	/// </remarks>
	[ElementName("includeset")]
	public class IncludeSet : DataTypeBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IncludeSet"/> class.
		/// </summary>
		public IncludeSet()
		{
			Include = new PatternCollection();
		}

		/// <summary>
		/// Defines a single string for tests to include.
		/// </summary>
		[BuildElementArray("include")]
		public PatternCollection Include
		{
			get;
			private set;
		}

		/// <summary>
		/// Adds a nested set of includes, or references other standalone 
		/// <see cref="IncludeSet"/>.
		/// </summary>
		/// <param name="includeSet">The <see cref="IncludeSet" /> to add.</param>
		[BuildElement("includeset")]
		public void Append(IncludeSet includeSet)
		{
			foreach (string patternName in includeSet.GetIncludePatterns())
			{
				Pattern pattern = new Pattern { PatternName = patternName };
				Include.Add(pattern);
			}
		}

		/// <summary>
		/// Gets the include patterns.
		/// </summary>
		/// <returns></returns>
		public string[] GetIncludePatterns()
		{
			ArrayList includes = new ArrayList(Include.Count);

			foreach (Pattern include in Include)
			{
				if (include.IfDefined && !include.UnlessDefined)
				{
					includes.Add(include.PatternName);
				}
			}

			return (string[]) includes.ToArray(typeof(string));
		}
	}
}