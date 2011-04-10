using System;
using System.IO;
using System.Xml;

using Snak.Utilities;
using Snak.Tasks;

namespace Snak.Core
{
	/// <summary>
	/// Retrieves the currect <see cref="IProjectInfo"/> implementation
	/// for a VS project file, based on its version
	/// </summary>
	public class ProjectFactory
	{
		private ProjectFactory(){}

		/// <summary>
		/// Retrieves the correct <see cref="IProjectInfo"/> implementation
		/// for a VS project file, based on its version
		/// </summary>
		/// <param name="projectFilePath">A local path to a project file</param>
		/// <param name="config">The config for the retrieved project</param>
		/// <returns></returns>
		public static IProjectInfo GetProject(string projectFilePath, string config, LoggingHandler log)
		{
			return GetProject(new FileInfo(projectFilePath), config, log);
		}

		/// <summary>
		/// Retrieves the correct <see cref="IProjectInfo"/> implementation
		/// for a VS project file, based on its version
		/// </summary>
		/// <param name="projectFilePath">A local path to a project file</param>
		/// <param name="config">The config for the retrieved project</param>
		/// <returns></returns>
        public static IProjectInfo GetProject(FileInfo projectFilePath, string config, LoggingHandler log)
        {
            if (!projectFilePath.Exists) throw new FileNotFoundException(projectFilePath.FullName);

            XmlDocument document = new XmlDocument();
            using (FileStream stream = projectFilePath.OpenRead())
            {
                XmlTextReader reader = new XmlTextReader(projectFilePath.FullName, stream);
                document.Load(reader);
            }

            string projectType;
            try
            {
                projectType = DetermineProjectTypeForProject(document);
            }
            catch (Exception err)
            {
                throw new Exception(string.Format("Error determining project type for {0}", projectFilePath), err);
            }

            switch (projectType)
            {
                // This switch case is a bit nasty
                // but easiest way of still allowing the 'which project type' determiniation
                // to be unit tested (see below), without a full refactoring into a factory pattern

                case "Snak.Core.Whidbey.ProjectInfo":
                    return Whidbey.ProjectInfo.CreateNew(projectFilePath, config, log);

                case "Snak.Core.Everett.ProjectInfo":
                    {
                        // If config has been passed with a platform suffix, remove it
                        // These type of projects don't support that
                        // But (in the case of a VS2005 solution containing a BTS 2006 project)
                        // it does sometimes happen
                        if (config.Contains("|")) config = config.Split(new char[]{'|'}, 2)[0];
                        return new Everett.ProjectInfo(projectFilePath, config);
                    }

                //				case ProjectType.Ranier:
                //					return Ranier.ProjectInfo(projectFilePath);
                default:
                    throw new ArgumentException(string.Format("Unknown project type {0} in {1}", projectType, projectFilePath));
            }
        }

		/// <summary>
		/// Attempts to determine the correct project type for a VS project file
		/// by inspecting the XML for its version indicator
		/// </summary>
        private static string DetermineProjectTypeForProject(XmlDocument projectfile)
		{
			// attempt to find vs2005 (whitbey) project
			XmlNamespaceManager ns = new XmlNamespaceManager(projectfile.NameTable);
			ns.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003");
			XmlNode node = projectfile.SelectSingleNode("//msb:Project/msb:PropertyGroup/msb:ProductVersion/text()", ns) as XmlText;
			if (node!=null && node.Value.StartsWith("8."))
				return typeof(Whidbey.ProjectInfo).FullName;

            // use the everett parser with biztalk 2006 project files
            // since BTS projects still not using MSBuild (2008?)
            node = projectfile.SelectSingleNode("/VisualStudioProject/*") as XmlElement;
            if (node.Name == "BIZTALK" 
                && node.Attributes["ProductVersion"].Value.StartsWith("3.5.")
                )
            {
                return typeof(Everett.ProjectInfo).FullName;
            }

			// attempt to find vs2003 (everett) project
			node = projectfile.SelectSingleNode("/VisualStudioProject/*/@ProductVersion") as XmlAttribute;
			if (node!=null && node.Value.StartsWith("7."))
                return typeof(Everett.ProjectInfo).FullName;

			// TODO: attempt to find vs2002 (ranier) project??

			throw new ArgumentException("Unable to determine project file version");
		}

		[NUnit.Framework.TestFixture]
		public class ProjectFactoryTester
		{
			[NUnit.Framework.Test]
			public void TestParseEverettProject()
			{
				XmlDocument projectXml = new XmlDocument();
				projectXml.Load(ResourceUtils.GetResourceStream(typeof(Everett.SolutionInfo), "TestCSharpWinformsProject2003.csproj"));

                NUnit.Framework.Assert.AreEqual(typeof(Everett.ProjectInfo).FullName, DetermineProjectTypeForProject(projectXml), "Failed to determine project type for known 2003 project");
			}

			[NUnit.Framework.Test]
			public void TestParseWhitbeyProject()
			{
				XmlDocument projectXml = new XmlDocument();
				projectXml.Load(ResourceUtils.GetResourceStream(typeof(Whidbey.SolutionInfo), "TestCSharpWinformsProject2005.csproj"));

                NUnit.Framework.Assert.AreEqual(typeof(Whidbey.ProjectInfo).FullName, DetermineProjectTypeForProject(projectXml), "Failed to determine project type for known 2005 project");
			}
		}
	}

    //public enum ProjectType
    //{
    //    Ranier,
    //    Everett,
    //    Whitbey
    //}
}
