using System;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using NAnt.VSNet.Types;
using Snak.Utilities;

namespace Snak.Core.Everett
{
	/// <summary>
	/// Represents a Visual Studio 2003 solution file, and provides access to it's projects
	/// </summary>
	/// <remarks>
	/// <example>
	///   <para>Some example</para>
	///   <code>
	///     <![CDATA[
	///     some code here
	///     ]]>
	///   </code>
	/// </example>
	/// </remarks>
	public class SolutionInfo : ISolutionInfo
	{
		#region Declarations
		private readonly FileInfo _solutionFile;
        private readonly string[] _buildTypes;
	    private readonly LoggingHandler _log;
	    private readonly ProjectLink[] _projects;
		private readonly Hashtable _projectsByGuid;
		private readonly Hashtable _projectBySolutionConfiguration;
		private readonly NAnt.VSNet.Types.WebMapCollection _projectWebMaps;
		private const string PROJECT_PATTERN = @"Project\(""([^""]+)""\)\s*=\s*""([^""]+)"",\s*""([^""]+)"",\s""([^""]+)";
		private const string PROJECT_BUILD_PATTERN	= @"(\{[^\}]+\}).SOLUTIONCONFIG.Build.0\s*=\s*([^\|]+)\|\.NET"; //";
		private const string BUILD_TYPES_PATTERN = @"^\s*([\w]+)\s*=\s*\1\s*$";
		#endregion

		#region Constructor(s)
        
        public SolutionInfo(FileInfo solutionFile, LoggingHandler log)
		{
			if (!solutionFile.Exists)
				throw new FileNotFoundException(solutionFile.Name);
			if (solutionFile.Extension != ".sln")
				throw new ArgumentException(string.Format("{0} is not a valid solution file", solutionFile.FullName), "solutionFile");

			_solutionFile = solutionFile;
            _log = log;
            log(LogLevel.Verbose, "Loading solution {0} as {1}", solutionFile.Name, GetType().FullName);

            // parse the solution file and create a list of all the projects within
			string solutionFileContents = File.ReadAllText(solutionFile.FullName);

            _projects = ExtractProjectLinks(solutionFile.Directory.FullName, solutionFileContents);
			_projectsByGuid = new Hashtable(_projects.Length);
			_projectWebMaps = new NAnt.VSNet.Types.WebMapCollection();
			foreach(ProjectLink project in _projects)
				_projectsByGuid.Add(project.ProjectGuid, project);

            _buildTypes = ExtractBuildTypes(solutionFileContents);
            _projectBySolutionConfiguration = ExtractProjectsByBuild(solutionFileContents, _projectsByGuid);
		}

		#endregion

		public FileInfo SolutionFile {
			get { return _solutionFile; }
		}

		/// <summary>
		/// A hashtable of maps between URI's (or URI prefixes) and local paths,
		/// to be used when resolving HTTP references for projects
		/// </summary>
		public NAnt.VSNet.Types.WebMapCollection WebMaps{
			get { return _projectWebMaps; }
		}

        /// <summary>
        /// An array of all the active build configurations for the solution
        /// (from which project configurations are mapped)
        /// One of these values is the input into <see cref="GetProjectsFor"/>
        /// </summary>
        public string[] Configurations
        {
            get { return _buildTypes; }
        }

		/// <summary>
		/// Retrieves an array of all the projects to be built in the specified solution configuration
		/// </summary>
		/// <param name="solutionConfiguration">A solution configuration (Debug, Release etc...)
		/// This is mapped back to project configurations based on what's configured within the solution,
		/// and doesn't neccesarily match any one project configuration name</param>
		/// <returns></returns>
		public ProjectInfo[] GetProjectsFor(string solutionConfiguration) 
		{
            ProjectLinkWithConfig[] links = GetProjectLinksFor(solutionConfiguration);
            if (links == null || links.Length == 0) return new ProjectInfo[0];

			ProjectInfo[] projects = new ProjectInfo[links.Length];
			for (int i = 0; i < projects.Length; i++) {
				projects[i] = GetProject(links[i]);
			}
			return projects;
		}

        /// <summary>
        /// Retrieves an array of all the projects to be built in the specified solution configuration
        /// </summary>
        /// <param name="solutionConfiguration">A solution configuration (Debug, Release etc...)
        /// This is mapped back to project configurations based on what's configured within the solution,
        /// and doesn't neccesarily match any one project configuration name</param>
        /// <returns></returns>
        internal ProjectLinkWithConfig[] GetProjectLinksFor(string solutionConfiguration)
        {
            try
            {
                ProjectLinkWithConfig[] links = (ProjectLinkWithConfig[])_projectBySolutionConfiguration[solutionConfiguration];
                return (links == null) ? new ProjectLinkWithConfig[0] : links;
            }
            catch (Exception err)
            {
                string message = string.Format("Solution configuration '{0}' not located", solutionConfiguration);
                throw new InvalidOperationException(message, err);
            }
        }

		IProjectInfo[] ISolutionInfo.GetProjectsFor(string solutionConfiguration) {
			return GetProjectsFor(solutionConfiguration);
		}

		private ProjectInfo GetProject(ProjectLinkWithConfig link)
		{
            Guard.ArgumentIsNotNull(link, "link");
            Guard.IsNotNull(link.ProjectLink, string.Format("The {0} that hangs off the given {1} is null", typeof(ProjectLink).Name, typeof(ProjectLinkWithConfig).Name));
            Guard.GuidIsNotEmpty(link.ProjectLink.ProjectGuid, string.Format("The project guid is empty for project with name '{0}'", link.ProjectLink.Name));
		    _log(LogLevel.Verbose, string.Format("Creating a new project with name '{0}' at location '{1}' and project GUID of {2}", link.ProjectLink.Name, link.ProjectLink.Location, link.ProjectLink.ProjectGuid));

            try
            { 
                return new ProjectInfo(link.GetProjectLocation(SolutionFile.Directory, _projectWebMaps), link.Config);
            }
            catch (Exception err)
            {
                throw new Exception(
                    string.Format("Failed to convert project link into project for project {0} [Location={1}]", link.ProjectLink.Name, link.ProjectLink.Location),
                    err);
            }
		}

		private static ProjectLink[] ExtractProjectLinks(string solutionRoot, string solutionFileContents)
		{
			Regex projectMatcher = new Regex(PROJECT_PATTERN);
			MatchCollection matches = projectMatcher.Matches(solutionFileContents);
			List<ProjectLink> projects = new List<ProjectLink>(matches.Count);

			for (int projectIndex = 0; projectIndex < matches.Count; projectIndex++) {
				Match match = matches[projectIndex];	

				string solutionGuid = match.Groups[1].Value;
				string name = match.Groups[2].Value;
				string location = match.Groups[3].Value;
				string projectGuid = match.Groups[4].Value;

                if (location.ToLower().EndsWith(".etp"))
                {
                    // read through enterprise template project
                    // extract underlying projects
                    projects.AddRange(getProjectsFromEtp(solutionGuid, solutionRoot, Path.Combine(solutionRoot, location)));

                }
                else
                {
                    projects.Add(new ProjectLink(name, location, new Guid(projectGuid), new Guid(solutionGuid)));
                }
			}

			return projects.ToArray();
		}

        /// <summary>
        /// Returns project files embedded in Etp file
        /// </summary>
        private static List<ProjectLink> getProjectsFromEtp(string solutionGuid, string solutionRoot, string etpFile)
        {
            List<ProjectLink> projectInfos = new List<ProjectLink>();

            string etpRoot = Path.GetDirectoryName(etpFile);

            XPathDocument doc = new XPathDocument(etpFile);
            XPathNavigator nav = doc.CreateNavigator();
            XPathNodeIterator it = nav.Select("descendant::EFPROJECT/GENERAL/References/Reference");
           
            while (it.MoveNext())
            {
                string file = it.Current.SelectSingleNode("FILE").Value;
                string projectGuid = it.Current.SelectSingleNode("GUIDPROJECTID").Value;
                string location = file;

                if (location.EndsWith(".etp"))
                {
                    // read through enterprise template project and extract underlying projects
                    projectInfos.AddRange(getProjectsFromEtp(solutionGuid, solutionRoot, Path.Combine(etpRoot, file)));
                }
                else
                {
                    // For non-web projects, concatinate the current enterprise template project path
                    if (!ProjectLink.IsPathWebProject(file))
                    {
                        location = Path.Combine(etpRoot, location);

                        // Make location relative to solution
                        location = location.Replace(solutionRoot, string.Empty);

                        // Remove prefix
                        if (location.StartsWith(@"\"))
                        {
                            location = location.Remove(0, 1);
                        }
                    }

                    string name = Path.GetFileNameWithoutExtension(file);
                    projectInfos.Add(new ProjectLink(name, location, new Guid(projectGuid), new Guid(solutionGuid)));
                }
            }

            return projectInfos;
        }

        private Hashtable ExtractProjectsByBuild(string solutionFileContents, Hashtable projects) 
		{
			Guard.ArgumentStringIsNotNullOrEmpty(solutionFileContents, "solutionFileContents");
			string[] buildTypes = ExtractBuildTypes(solutionFileContents);

			// Now create a hashtable that matches projects by the solution configurations they're enabled in
			Hashtable projectsByConfig = new Hashtable(buildTypes.Length);
			foreach(string solutionConfig in buildTypes)
			{
				Regex projectConfigMatcher = new Regex(PROJECT_BUILD_PATTERN.Replace("SOLUTIONCONFIG", solutionConfig));
				MatchCollection projectMatches = projectConfigMatcher.Matches(solutionFileContents);
				ProjectLinkWithConfig[] projectsForThisConfig = new ProjectLinkWithConfig[projectMatches.Count];

				for (int matchIndex = 0; matchIndex < projectMatches.Count; matchIndex++) {
					Match projectConfigMatch = projectMatches[matchIndex];

					// look at the details for the project
					string projectGuid = projectConfigMatch.Groups[1].Value;
					string projectConfig = projectConfigMatch.Groups[2].Value;

					// pull the project for that guid back out of the projects hashtable
					ProjectLink projectLink = (ProjectLink)projects[new Guid(projectGuid)];
                    Guard.IsNotNull(projectLink, string.Format("Failed to find ProjectLink for guid {0}", projectGuid));

					// ...and add it into the array for this configuration
					projectsForThisConfig[matchIndex] = new ProjectLinkWithConfig(projectLink, projectConfig);
				}

				// finally, add all the projects active in this configuration into the hashtable
				projectsByConfig.Add(solutionConfig, projectsForThisConfig);
			}
			return projectsByConfig;
		}

		private static string[] ExtractBuildTypes(string solutionFileContents) {
            Guard.ArgumentStringIsNotNullOrEmpty(solutionFileContents, "solutionFileContents");
			Regex projectTypeMatcher = new Regex(BUILD_TYPES_PATTERN, RegexOptions.Multiline);
			MatchCollection matches = projectTypeMatcher.Matches(solutionFileContents);
	
			// Create a list of all the types of build (Debug, Release)
			// ... ie all the solution configurations
			string[] buildTypes = new string[matches.Count];
			for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++) 
			{
				buildTypes[matchIndex] = matches[matchIndex].Groups[1].Value;
			}
			return buildTypes;
		}

		private static string ReplaceLeft(string value, string prefix, string newPrefix)
		{
			if (!value.StartsWith(prefix))
				throw new InvalidOperationException("Value must start with prefix to begin with");

			return newPrefix + value.Substring(prefix.Length);
		}

		internal class ProjectLinkWithConfig
		{
			private ProjectLink _projectLink;
			private string _config;

			public ProjectLinkWithConfig(){}

			public ProjectLinkWithConfig(ProjectLink projectLink, string config)
			{
				_projectLink = projectLink;
				_config = config;
			}

			public ProjectLink ProjectLink 
			{
				get { return _projectLink; }
				set { _projectLink = value; }
			}

			public string Config 
			{
				get { return _config; }
				set { _config = value; }
			}

            public FileInfo GetProjectLocation(DirectoryInfo solutionRoot, NAnt.VSNet.Types.WebMapCollection webProjectMappings)
            {
                Console.WriteLine("Getting project location for {0} [IsWebProject={1}, ProjectLocation={2}]", this.ProjectLink.Name, this.ProjectLink.IsWebProject, this.ProjectLink.Location);
                // Get shot of the simple case - just find the project relative to the solution
                if (!ProjectLink.IsWebProject)
                    return new FileInfo(Path.Combine(solutionRoot.FullName, ProjectLink.Location));

                // Otherwise we have to deal with a HTTP style project (one way or the other)
                string mappedPath = webProjectMappings.FindBestMatch(ProjectLink.Location);
                if (mappedPath != null)
                {
                    FileInfo mappedFile = new FileInfo(mappedPath);
                    AssertFileExists(mappedFile, "Unable to locate web project {0} after mapping to {1}", ProjectLink.Location, mappedPath);
                    return mappedFile;
                }
                else
                {
                    // Attempt to find the project directly under the solution
                    string projectFileName = Path.GetFileName(ProjectLink.Location);
                    string projectDirectory = Path.GetFileName(Path.GetDirectoryName(ProjectLink.Location));
                    string pathAttempted = Path.Combine(Path.Combine(solutionRoot.FullName, projectDirectory), projectFileName);
                    FileInfo inferredFile = new FileInfo(pathAttempted);
                    AssertFileExists(inferredFile, "Failed to infer path to web project {0} at {1}", ProjectLink.Location, inferredFile);
                    return inferredFile;
                }
            }

			private static void AssertFileExists(FileInfo fileInfo, string message, params object[] args)
			{
				if (!fileInfo.Exists)
				{
					//System.Diagnostics.Debugger.Launch();
					throw new FileNotFoundException(string.Format(message, args), fileInfo.FullName);
				}
			}
		}

#if(UNITTEST)
		[NUnit.Framework.TestFixture]
		public class SolutionInfoTester{

            // this assumes that the tests are always going to be run from the \bin\debug directory
            private const string SOLUTION_NAME = @"..\..\..\TestArtifacts\SampleApplications\v1.1\C#\SolutionWithMultipleDifferingProjects.sln";
            private string testSolutionRoot = String.Empty;

			#region Setup test and mock objects
			SolutionInfo solutionInfo;
			string solutionFileContents;

			[NUnit.Framework.TestFixtureSetUp]
			public void Init(){

                testSolutionRoot = Path.GetFullPath(Path.GetDirectoryName(SOLUTION_NAME)).ToLower();
                solutionInfo = new SolutionInfo(new FileInfo(Path.GetFullPath(SOLUTION_NAME)), NullLogger.Log);
				// don't map for the first WebApplication1 project - this is left to be inferred
				WebMap map = new WebMap();
				map.Url = "http://localhost/Webapplication1";
				map.Path = new FileInfo(Path.Combine(testSolutionRoot, @"AspNetWebApplication\WebApplication1"));
				solutionInfo.WebMaps.Add(map);
                map = new WebMap();
                map.Url = "http://localhost/WebService1";
                map.Path = new FileInfo(Path.Combine(testSolutionRoot, @"AspNetWebApplication\WebService1"));
                solutionInfo.WebMaps.Add(map);

				using(TextReader reader = solutionInfo.SolutionFile.OpenText())
					solutionFileContents = reader.ReadToEnd();

				if (solutionFileContents==null)
					throw new FileLoadException("Failed to load solution contents");
			}

			[NUnit.Framework.SetUp]
			public void Setup()
			{
			}
			#endregion


            [NUnit.Framework.Test()]
            public void KeithsTest()
            {
                FileInfo file = new FileInfo(@"C:\VSTS\ICMS\Branches\eBusinessFeatures\Icms\IcmsAll.sln");
                SolutionInfo s = new SolutionInfo(file, NullLogger.Log);        
            }

		    [NUnit.Framework.Test()]
			public void TestReplacePrefix()
			{
				NUnit.Framework.Assert.AreEqual("Something", ReplaceLeft("Nothing", "No", "Some"));
				NUnit.Framework.Assert.AreEqual("Nothing", ReplaceLeft("Something", "Some", "No"));
			}

			[NUnit.Framework.Test(Description="")]
			public void TestParseSolutionFile(){
				string solutionFileDemo = @"Microsoft Visual Studio Solution File, Format Version 8.00
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""SNAK"", ""SNAK\SNAK.csproj"", ""{3CFFC105-BE5E-4968-A74D-D0A9EC7CE340}""
Project(""{00a0a0C0-301F-11D3-BF4B-00C04F79a0a0}"") = ""Other"", ""Other\Other.csproj"", ""{3C0a0a0a-BE5E-4968-A74D-000000000001}""
	ProjectSection(ProjectDependencies) = postProject
	...etc...";

				ProjectLink[] projects = ExtractProjectLinks(Environment.CurrentDirectory, solutionFileDemo);

				NUnit.Framework.Assert.IsNotNull(projects, "ExtractProjectInfos returned null");
				NUnit.Framework.Assert.AreEqual(2, projects.Length);
				NUnit.Framework.Assert.AreEqual("SNAK", projects[0].Name);
				NUnit.Framework.Assert.AreEqual("Other", projects[1].Name);
				NUnit.Framework.Assert.AreEqual("SNAK\\SNAK.csproj".ToLower(), projects[0].Location);
				NUnit.Framework.Assert.AreEqual("3CFFC105-BE5E-4968-A74D-D0A9EC7CE340".ToLower(), projects[0].ProjectGuid.ToString());
			}

			[NUnit.Framework.Test]
			public void TestExtractBuildTypes()
			{
				string[] buildTypes = ExtractBuildTypes(solutionFileContents);

				NUnit.Framework.Assert.AreEqual(2, buildTypes.Length);
				NUnit.Framework.Assert.AreEqual("Debug", buildTypes[0]);
				NUnit.Framework.Assert.AreEqual("Release", buildTypes[1]);
			}

			[NUnit.Framework.Test]
			public void TestSolutionInfo()
			{
				IProjectInfo[] projects;

				projects = solutionInfo.GetProjectsFor("Debug");
				NUnit.Framework.Assert.AreEqual(7, projects.Length);
			
				projects = solutionInfo.GetProjectsFor("Release");
				NUnit.Framework.Assert.AreEqual(6, projects.Length);
			}

			public void SpikeProjectBuildPatternRegex()
			{
				Init();
				Regex regex = new Regex(PROJECT_BUILD_PATTERN.Replace("SOLUTIONCONFIG", "Debug"));
				NUnit.Framework.Assert.IsTrue(regex.IsMatch(solutionFileContents));
				Console.WriteLine(regex.Match(solutionFileContents).Value);
			}

			[NUnit.Framework.Test]
			[NUnit.Framework.ExpectedException(typeof(ArgumentException))]
			public void TestGetBadProject()
			{
                new SolutionInfo(new FileInfo(this.GetType().Assembly.Location), NullLogger.Log);
			}

            [NUnit.Framework.Test]
            public void TestEnterpriseTemplateSolution()
            {
                const string ETP_SOLUTION_NAME = @"..\..\..\TestArtifacts\SampleApplications\v1.1\C#\SolutionWithMultipleDifferingProjects.etp.sln";
                //SolutionInfo solution = new SolutionInfo(new FileInfo(ETP_SOLUTION_NAME));
                //ProjectLinkWithConfig[] projects = solution.GetProjectLinksFor("Debug");
                string solutionContents = File.ReadAllText(ETP_SOLUTION_NAME);
                ProjectLink[] projectLinks = SolutionInfo.ExtractProjectLinks(Path.GetDirectoryName(ETP_SOLUTION_NAME), solutionContents);

                NUnit.Framework.Assert.IsTrue(projectLinks.Length == 3, "Failed to locate all the projects in the ETP solution");

                SolutionInfo info = new SolutionInfo(new FileInfo(ETP_SOLUTION_NAME), NullLogger.Log);

                //int i = 0;
            }
		}
#endif

	}
}
