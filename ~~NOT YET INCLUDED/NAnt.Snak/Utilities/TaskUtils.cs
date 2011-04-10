using System;
using System.IO;
using NAnt.Core;

namespace Snak.Utilities
{
	/// <summary>
	/// Provides utility functionality to support the custom nant tasks in this assembly
	/// </summary>
	public class TaskUtils
	{
		#region Constructor(s)
		private TaskUtils()
		{
		}
		#endregion

		/// <summary>
		/// Copies NAnt settings from one task to another
		/// </summary>
		/// <param name="source">The source task</param>
		/// <returns></returns>
		public static CopySettingsSource CopySettingsFrom(Task source)
		{
			return new CopySettingsSource(source);
		}

		/// <summary>
		/// Provides the destination class to support <see cref="CopySettingsFrom"/>
		/// </summary>
		public class CopySettingsSource
		{
			private Task _source;

			internal CopySettingsSource(Task source)
			{
				_source = source;
			}

			public void To(Task destination)
			{
				destination.FailOnError = _source.FailOnError;
				destination.Threshold = _source.Threshold;
				destination.Verbose = _source.Verbose;
				destination.Parent = _source.Parent;
				destination.Project = _source.Project;
				// raise the logging level from the parent, but don't exeed Verbose
				destination.Threshold = (Level)Math.Min((int)_source.Threshold + 1000, (int)Level.Verbose);
			}
		}

        internal static void RecursivelyApplyFileAttribute(DirectoryInfo directoryToTraverse, FileAttributes fileAttributes)
        {
            directoryToTraverse.Attributes = fileAttributes;

            foreach (FileInfo file in directoryToTraverse.GetFiles())
            {
                file.Attributes = fileAttributes;    
            }

            foreach (DirectoryInfo direcroty in directoryToTraverse.GetDirectories())
            {
                RecursivelyApplyFileAttribute(direcroty, fileAttributes);
            } 
        }

		internal static string LowerFirst(string name) 
		{
			if (name==null || name.Length==0) return name;
			if (name.Length==1) return name.ToLower();
			return Char.ToLower(name[0]) + name.Substring(1).ToString();
		}

		internal static string SafeToString(object value) 
		{
			return value==null ? string.Empty : value.ToString();
		}

#if(UNITTEST)
		[NUnit.Framework.TestFixture]
		public class TaskUtilsTester
		{
			[NUnit.Framework.Test(Description="")]
			public void TestSafeToString()
			{
				NUnit.Framework.Assert.AreEqual(string.Empty, SafeToString(null));
				NUnit.Framework.Assert.AreEqual("Moo", SafeToString("Moo"));
			}

			[NUnit.Framework.Test]
			public void TestLowerFirst()
			{
				NUnit.Framework.Assert.AreEqual("mOO", LowerFirst("MOO"));
				NUnit.Framework.Assert.AreEqual("mOo", LowerFirst("MOo"));
				NUnit.Framework.Assert.AreEqual("moo", LowerFirst("Moo"));
				NUnit.Framework.Assert.AreEqual("moo", LowerFirst("moo"));
				NUnit.Framework.Assert.AreEqual("m", LowerFirst("M"));
				NUnit.Framework.Assert.AreEqual("m", LowerFirst("m"));
			}
		}
#endif
	}
}
